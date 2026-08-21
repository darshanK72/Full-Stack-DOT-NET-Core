# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/04. Database-First & Reverse Engineering`

---

#### Q1. (R) A teammate "fixes" scaffold output by adding a computed helper directly in the generated entity, then runs re-scaffold with `--force` after a DBA adds a column. Review what they changed and predict what happens on the next build.

**Answer:** Hand-editing `Product.cs` violates the database-first contract — `--force` replaces every generated entity file, so `InventoryValue` and `StockStatus` disappear while the new column appears, and any code referencing those members fails to compile until the logic is moved to a partial file the scaffold never touches.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Workflow | Custom members in generated `Product.cs` | Lost on every `--force` re-scaffold |
| Compile | Services/views call removed properties | CS1061 — build blocked after re-scaffold |
| Maintainability | "Fix scaffold output by hand" anti-pattern | Team repeats the same lost edits each DBA release |
| Design | Computed helpers mixed with persistence shape | Blurs generated mapping vs domain logic |

**Fix (priority order):**

1. Move `InventoryValue` and `StockStatus` to `Models/Product.Partial.cs` with the same namespace and `partial class Product` (as in this chapter's tutorial layout).
2. Re-run scaffold with `--force`; verify `Product.cs` is regenerated cleanly and `Product.Partial.cs` is untouched.
3. Add a team rule: **never edit** `Models/*.cs` or `Data/*DbContext.cs` after initial scaffold — only `*.Partial.cs` and non-generated services.
4. Document the exact scaffold command in README or `ScaffoldWorkflow.SampleReScaffoldCommand` so everyone produces identical output.

**Production takeaway:** Re-scaffold is designed to overwrite generated files — Karat tests whether you know partial classes are the survival mechanism, not Git history. See **Program.cs** Section 7 and **ScaffoldWorkflow.cs** re-scaffold checklist.

---

#### Q2. (R) Review this partial-class attempt. The developer wants `DisplayLabel` and a `[NotMapped]` cache field to survive re-scaffold. What fails at compile time or at runtime, and how should the files be organized?

**Answer:** `CategoryHelpers` is a separate static class, so `category.DisplayLabel` does not exist on the entity — views or services calling that member fail at compile time. The partial file structure is otherwise correct for `[NotMapped]` members, but the display helper must live on `partial class Category`, not a sibling helper type.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `DisplayLabel` on static helper, not on `Category` | CS1061 if views call `category.DisplayLabel` |
| API design | Extension logic disconnected from entity | Callers pass `Category` into static helper — easy to bypass and duplicate |
| Partial pattern | `Category.Partial.cs` exists but omits the desired instance member | Re-scaffold survives, but the feature is incomplete |
| Runtime | `_productCountCache` never populated in snippet | `CachedProductCount` always 0 — silent logic bug if used for UI |

**Fix (priority order):**

1. Add `DisplayLabel` as an instance property on `partial class Category` in `Category.Partial.cs` (mirror this chapter's tutorial).
2. Delete or repurpose `CategoryHelpers.cs` — keep one place for category display logic.
3. If `CachedProductCount` is real, populate `_productCountCache` in application code or remove the dead field.
4. Confirm namespace matches scaffolded `Category.cs` exactly so partial merging works.

```csharp
public partial class Category
{
    public string DisplayLabel =>
        string.IsNullOrWhiteSpace(Description) ? Name : $"{Name} - {Description}";
}
```

**Production takeaway:** Partial classes require the **same type name and namespace** — a helper class is not a partial. Karat stacks "survives re-scaffold" knowledge with whether the API shape actually compiles.

---

#### Q3. (P) Production integrates with a legacy SQL Server database owned by a DBA team. They deploy a script renaming `dbo.Products.UnitPrice` to `ListPrice` and changing `StockQuantity` from `INT` to `SMALLINT`. Your team runs scaffold with `--force` then `dotnet build`. Walk through the re-scaffold workflow in order.

**Answer:** SQL Server remains the source of truth — scaffold regenerates entities and fluent mapping from live metadata, overwrites generated files, leaves `*.Partial.cs` alone, and compile errors surface first in application code that still references old property names or assumes `int` for stock.

**Workflow (priority order):**

1. **DBA script applied** — column rename and type change exist in SQL before scaffold (see **InventoryBootstrap.cs** mindset: schema changes start in SQL, not C#).
2. **Stash or revert manual edits** in generated `Models/*.cs` and `Data/InventoryDbContext.cs` — there should be none; hand fixes would be destroyed anyway.
3. **Run scaffold with `--force`** — overwrites `Product.cs`, `InventoryDbContext.cs`, etc.; emits `ListPrice` instead of `UnitPrice`, maps `StockQuantity` as `short`/`int` per provider conventions.
4. **`*.Partial.cs` untouched** — `Product.Partial.cs` still references `UnitPrice` in `InventoryValue => UnitPrice * StockQuantity` → **compile error CS1061** (first obvious break).
5. **Fix app layer and partials** — update partial computed properties to `ListPrice`; update services, DTOs, API contracts, and tests — **not** the regenerated mapping file.
6. **`dotnet build` + targeted tests** — verify decimal reads/writes and that `SMALLINT` range is acceptable for business rules.

| File kind | After re-scaffold |
|---|---|
| `Models/Product.cs` | Overwritten — new property names/types |
| `Models/Product.Partial.cs` | Untouched — may need manual update for renamed columns |
| `Data/InventoryDbContext.cs` | Overwritten — fluent config refreshed |
| Repositories / API / tests | Untouched — likely broken until updated |

**Production takeaway:** Re-scaffold shifts breakage to **call sites and partials**, not to "fixing" generated mapping. See **ScaffoldWorkflow.PrintReScaffoldChecklist** — step 4 is fix app code, not generated files.

---

#### Q4. (D) You inherit a 15-year-old ERP database: `dbo` holds ~180 operational tables, `audit` holds change-log tables, and several tables use non-plural names (`tblCustMaster`, `OrdLine`). The app only reads `dbo.Categories`, `dbo.Products`, and `dbo.OrdLine`. How do you scaffold without dragging the entire schema into the model, and what re-scaffold drift risks appear when a new developer scaffolds without the same filters?

**Answer:** Scaffold only required objects with repeated `--table` flags (and `--schema dbo` if you want schema boundary), optionally `--no-pluralize` for legacy names — never scaffold the full 180-table catalog into one DbContext. Drift happens when two developers run different filter sets and commit conflicting generated files.

- **Recommended command shape** (from this chapter's **ScaffoldFilterPreview**):

```bash
dotnet ef dbcontext scaffold "<conn>" Microsoft.EntityFrameworkCore.SqlServer \
    --schema dbo \
    --table dbo.Categories --table dbo.Products --table dbo.OrdLine \
    --output-dir Models --context-dir Data --context ErpReadDbContext \
    --no-pluralize --force
```

- **`--schema dbo`** excludes `audit.ChangeLog` and other audit objects unless explicitly needed.
- **`--table`** limits entities to the three operational tables — avoids 177 unused `DbSet<>` entries, navigation sprawl, and slow design-time builds.
- **`--no-pluralize`** keeps `DbSet` names aligned with legacy table names (`OrdLine` not `OrdLines`) when the database does not pluralize.

**Re-scaffold drift risks:**

| Drift source | Symptom |
|---|---|
| Dev scaffolds **all tables** vs filtered subset | Massive diff, accidental `DbSet` for audit tables, merge hell |
| Missing `--no-pluralize` | `DbSet` property renames break LINQ and DI registration |
| Different `--context` name or output folders | Duplicate DbContexts, wrong namespace, CS0111/CS0101 conflicts |
| Hand-edits in generated files between scaffolds | Lost customizations; unpredictable merge conflicts |

**Team process:** Check in a script or documented one-liner (like `ScaffoldWorkflow.SampleReScaffoldCommand`); treat it as the only allowed scaffold invocation; never edit generated entities; use partials for computed legacy labels (`DisplayLabel` pattern). Optional CI guard: fail PRs that touch generated paths without the scaffold script in the commit message or a recorded checksum.

**Production takeaway:** Legacy integration is as much **process** as tooling — Karat tests filtering flags plus the organizational failure mode when scaffold commands diverge.

---

#### Q5. (R) Before re-scaffold, a developer tuned delete behavior and decimal precision directly in the generated DbContext. Review the diff they intend to keep "because scaffold gets it wrong". They run `--force` re-scaffold next sprint. What regressions appear?

**Answer:** Re-scaffold restores provider-default fluent configuration — their manual `decimal(18, 4)` and `DeleteBehavior.Restrict` are lost, and removing the `OnModelCreatingPartial` call breaks any legitimate extensions wired through that hook.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Workflow | Hand-edited `OnModelCreating` in generated DbContext | Overwritten on `--force` — precision and delete rule revert |
| Correctness | Default delete may become `ClientSetNull` / cascade per FK metadata | Orphan inserts or unexpected cascade deletes against legacy DB rules |
| Data | `decimal(18, 2)` vs `(18, 4)` mismatch | Silent rounding or truncation on money fields |
| Compile / extensibility | `OnModelCreatingPartial` call removed | Custom partial DbContext configuration never runs — hidden regressions |

**Fix (priority order):**

1. Move non-default mapping into `InventoryDbContext.Partial.cs` implementing `partial void OnModelCreatingPartial(ModelBuilder modelBuilder)` — scaffold preserves the hook call in generated file.
2. For precision/delete rules scaffold consistently gets wrong, use `IEntityTypeConfiguration<Product>` classes in a **non-generated** `Configurations/` folder registered from the partial hook.
3. Re-scaffold with `--force`; verify generated file calls `OnModelCreatingPartial(modelBuilder)` at the end of `OnModelCreating`.
4. Add integration tests that assert delete behavior and decimal scale against a test database — catches re-scaffold regressions early.

```csharp
// Data/InventoryDbContext.Partial.cs  (never overwritten)
partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");
        entity.HasOne(d => d.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    });
}
```

**Production takeaway:** DbContext fluent tweaks belong in partials or separate configuration classes — same rule as entity partials. See **InventoryDbContext.cs** comments on `OnModelCreatingPartial`.

---

#### Q6. (M) Two developers re-scaffold the same legacy database on different branches. Dev A scaffolds with `--schema dbo --no-pluralize`; Dev B runs scaffold with no filters on a default template. Both commit generated files. At merge, `InventoryDbContext.cs` and entity files conflict heavily. Explain scaffold output drift and the team process for repeatable re-scaffold.

**Answer:** EF reverse-engineering is deterministic **only when connection, filters, provider, and EF tool version are identical** — different flags produce different type surfaces (`DbSet` count, property names, namespaces), so merging two scaffold outputs is merging two unrelated models, not a normal code conflict.

**Mechanism:**

- Scaffold reads **live database metadata** at execution time — Dev B pulls every readable table/view, emitting hundreds of entity files Dev A never generated.
- **`--no-pluralize` vs default pluralization** changes `DbSet` property names (`Products` vs `Product`, `OrdLine` vs `OrdLines`) — same table, different C# API.
- **`--schema` / `--table` omission** includes `audit.*` objects — extra entities and relationships Dev A excluded.
- Generated files are **meant to be replaced wholesale** — merge conflict markers inside `OnModelCreating` produce invalid C# that neither side intentionally wrote.

**Repeatable team process:**

1. **Single source command** — script in repo (e.g., `ScaffoldWorkflow.SampleReScaffoldCommand` extended with your filters); README says "never run scaffold without this script."
2. **Pin tooling** — same `dotnet-ef` global tool version and `Microsoft.EntityFrameworkCore.Design` package version in `.csproj`; record in `global.json` or tool manifest if needed.
3. **Generated vs owned split** — only `*.Partial.cs`, `Configurations/`, services are hand-edited; treat `Models/*.cs` and `Data/*DbContext.cs` as build artifacts.
4. **Re-scaffold on main after DBA release** — one person runs script, commits regeneration; feature branches rebase instead of re-scaffolding locally.
5. **CI** — `dotnet build` on PR; optional job fails if generated folder changes without the scaffold script also changing (proves intentional regen).
6. **Conflict policy** — if two branches both regenerated, **pick one scaffold run and re-run on merged DB state** — do not manually merge `InventoryDbContext.cs`.

**Production takeaway:** Re-scaffold drift is a process failure, not a Git skill issue — Karat tests whether you treat scaffold output like compiled output with a pinned recipe. See **Program.cs** QUICK REFERENCE — partial classes vs generated files.

---
