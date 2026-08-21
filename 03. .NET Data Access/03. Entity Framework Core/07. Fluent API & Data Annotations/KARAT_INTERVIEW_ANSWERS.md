# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/07. Fluent API & Data Annotations`

---

#### Q1. (R) After a schema review, QA reports that `CatalogProducts.Name` truncates at 50 characters even though the entity still shows `[MaxLength(100)]`. Review the configuration split across files. What conflicted, what actually landed in SQL Server, and how do you fix it so the team is not surprised again?

**Answer:** EF Core applied **both** configuration sources, but when annotation and Fluent API disagree on the same mapping facet, **Fluent API wins** — the column is `NVARCHAR(50)`, not 100. Developers reading only `CatalogProduct.cs` will misdiagnose truncation as an application bug.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Mapping conflict | `[MaxLength(100)]` vs `HasMaxLength(50)` on same property | Schema uses 50; annotation misleads code review and API docs |
| Maintainability | Two sources of truth for one column | Future edits update only one side; silent drift repeats |
| Validation | `[StringLength]` / data-annotation validators may still use 100 in some paths | Client-side or manual validation can disagree with database cap |

**Fix (priority order):**

1. Pick **one** authoritative rule per property — remove the loser (`[MaxLength(100)]` **or** the Fluent `HasMaxLength(50)`), matching the intended business limit.
2. Regenerate or add a migration so the database, model snapshot, and entity agree; verify with `dotnet ef migrations script` or SSMS column definition.
3. Adopt team convention: simple per-property rules on the entity **or** centralized Fluent/`IEntityTypeConfiguration<T>` — not both for the same facet (matches **CatalogDbContext.cs** precedence note).
4. Add a PR checklist item: "no duplicate mapping facets across annotations and Fluent."

**Production takeaway:** Karat uses dual-configuration drift to test whether you know Fluent wins at runtime — reading the entity file alone is not enough. See this chapter's **CatalogProduct** (annotations) vs **Supplier** (Fluent-only) split.

---

#### Q2. (R) Finance rejects migrated totals: penny amounts drift after bulk price imports. The team points at `CatalogProduct.UnitPrice`. Review the entity and migration snippet. What is wrong with the money mapping, and what do you change before the next deploy?

**Answer:** Money columns need an explicit **precision and scale** (`HasPrecision` / `[Precision]`); relying on SQL Server's default `decimal(18,2)` silently rounds values like `12.3456789m` to two decimal places at persistence. When Fluent `HasPrecision(19,4)` conflicts with `[Column(TypeName = "decimal(18,2)")]`, **Fluent API wins** — but stale attributes and old migrations still confuse reviewers and can block clean diffs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Default `decimal(18,2)` without explicit facet | Sub-cent amounts rounded on INSERT/UPDATE; finance reconciliation fails |
| Mapping conflict | `[Column(TypeName = "decimal(18,2)")]` vs `HasPrecision(19, 4)` | Team unsure which scale is live; annotation lies on scaffolded entity |
| Migration hygiene | Existing migration hard-codes `decimal(18,2)` | New precision requires a new migration; altering column may lock large tables |

**Fix (priority order):**

1. Decide business scale (e.g., `(19,4)` for unit prices) and configure **once**: `entity.Property(e => e.UnitPrice).HasPrecision(19, 4);` or `[Precision(19, 4)]` — not both conflicting.
2. Remove obsolete `[Column(TypeName = ...)]` if Fluent owns the facet; regenerate migration to `ALTER COLUMN` with correct precision/scale.
3. Validate rounding rules in the domain layer if display/storage scales differ (store 4 dp, present 2 dp in UI).
4. Add integration tests that insert boundary decimals and assert round-trip equality from SQL.

```csharp
entity.Property(e => e.UnitPrice).HasPrecision(19, 4);
```

**Production takeaway:** `decimal` in C# does not imply database scale — EF must map money explicitly. Conflicting annotation + Fluent on precision is the same "two bosses" trap as max length. See **CatalogProduct.UnitPrice** comment in this chapter (`decimal(18,2)` by convention).

---

#### Q3. (R) Production paging on order history is slow after go-live. The `OrderLines` table has ~2M rows. Review the relationship configuration — FK exists, but DBAs see a table scan on every lookup by product. What is missing, and how do you fix it in EF Core?

**Answer:** EF Core creates the **foreign-key constraint** but does **not** automatically create a **nonclustered index** on the FK column. Without `HasIndex(e => e.CatalogProductId)`, queries filtering or joining on `CatalogProductId` scan the heap as row count grows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | No index on `CatalogProductId` FK column | Table scans on `WHERE CatalogProductId = @id`; slow joins and paging |
| Operations | FK present so dev assumes "indexed" | Local dev with tiny data hides issue; prod latency spikes after scale |
| Design | Only relationship configured, not access paths | Common EF misconception — constraint ≠ index |

**Fix (priority order):**

1. Add index in Fluent API: `entity.HasIndex(e => e.CatalogProductId);` (name it in migrations for clarity).
2. Generate and deploy migration; verify plan uses seek/lookup in SSMS/`SET STATISTICS IO ON`.
3. For composite queries (e.g., product + date range), consider composite index `{ CatalogProductId, CreatedUtc }` based on actual query shapes.
4. Document team rule: **every FK used in filters/joins gets an index** unless a covering clustered key already serves it.

```csharp
modelBuilder.Entity<OrderLine>(entity =>
{
    entity.HasOne(e => e.Product)
          .WithMany()
          .HasForeignKey(e => e.CatalogProductId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(e => e.CatalogProductId);
});
```

**Production takeaway:** This chapter teaches `HasIndex` for **unique** business keys (**SupplierCode**); the same API applies to FK columns — uniqueness is optional, seek performance is the goal. Contrast with **DemonstrateUniqueIndexViolation** in **Program.cs**.

---

#### Q4. (R) With `#nullable enable`, a developer "fixes" compiler warnings on `CatalogProduct` but SaveChanges behavior changes in staging. Review the property declarations. What do nullable reference types vs `[Required]` each control, and what would you standardize?

**Answer:** **Nullable reference types** are a **compile-time** C# feature; **`[Required]`** is an EF Core / DataAnnotations **mapping and validation** facet. A non-nullable `string Name` without `[Required]` is still mapped **required by EF convention**, but removing `[Required(AllowEmptyStrings = false)]` allows empty string at validation. `[Required]` on `string? Sku` is contradictory — the column is required in the model while the type promises nullability to the compiler.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `string Name` without `[Required(AllowEmptyStrings = false)]` | `""` may pass where business forbids empty names (**Program.cs** demo pitfall) |
| Type/model mismatch | `[Required]` on `string? Sku` | CS8618 quieted but intent unclear; reviewers assume Sku optional |
| Layer confusion | Treating NRT as substitute for EF attributes | Compiler green; database/validation rules diverge from API contracts |

**Fix (priority order):**

1. Align each property: required value → `string` + `[Required(AllowEmptyStrings = false)]` when empty string must fail; optional → `string?` without `[Required]`.
2. Remove `[Required]` from nullable reference properties unless you intentionally require non-null at persistence (then use non-nullable `string` with initializer).
3. Standardize: **NRT for C# contracts**, **`[Required]` / Fluent `IsRequired()` for persistence and SaveChanges validation** — document on the team wiki.
4. Mirror rules in ASP.NET model binding if the same entities surface in APIs.

```csharp
[Required(AllowEmptyStrings = false)]
[MaxLength(100)]
public string Name { get; set; } = string.Empty;

[StringLength(20, MinimumLength = 3)]
public string Sku { get; set; } = string.Empty;

public string? Description { get; set; }
```

**Production takeaway:** Karat stacks NRT and EF validation — they solve different problems. See **CatalogProduct.cs** Section 1 on `[Required]` vs conventions and empty-string default.

---

#### Q5. (P) Your team inherits scaffolded `Supplier` rows from a legacy database and adds `[StringLength(20)]` on `SupplierCode` while `CatalogDbContext` already calls `HasIndex(e => e.SupplierCode).IsUnique()` and `HasMaxLength(20)` in Fluent API. Builds succeed; tests pass locally. What production risks remain if you leave both styles on the same property, and what convention would you enforce?

**Answer:** When values agree, EF merges facets and tests pass, but **duplicate configuration** still creates drift risk on the next edit — someone updates the annotation during a scaffold regen and misses Fluent, or vice versa. Indexes and uniqueness belong in Fluent anyway; sprinkling annotations on a POCO designed for **OnModelCreating** (this chapter's **Supplier**) fights the intended architecture.

- **Drift risk:** Scaffold regen overwrites entity attributes; Fluent block unchanged → next migration surprises (length, required, index name).
- **Review blind spot:** Reviewers see `[StringLength(20)]` on the class and skip **CatalogDbContext** — unique index or filtered index rules stay invisible.
- **Validation duplication:** DataAnnotations validators and EF Fluent may both apply; conflicting error messages in API vs console paths.
- **Convention:** Keep **Supplier** annotation-free; all mapping in `ConfigureSupplier` or `IEntityTypeConfiguration<Supplier>`. Use annotations on **CatalogProduct**-style simple entities only when the team owns the file and won't rescaffold.
- **Production enforcement:** Analyzer or PR rule — "no `[MaxLength]` / `[Required]` on entities configured in Fluent for the same property"; single `IEntityTypeConfiguration` per aggregate in large apps.

**Production takeaway:** Builds succeeding does not mean configuration is maintainable — Karat tests judgment about **one boss per facet**, not whether EF can merge identical duplicates today.

---

#### Q6. (R) A pricing microservice bulk-inserts catalog rows. One bad batch passes C# compilation and EF model validation but corrupts amounts in SQL Server. Review the insert path and configuration. List the issues and prioritized fixes.

**Answer:** The batch relies on implicit `decimal(18,2)` mapping, so high-precision literals are **rounded at the database** without throwing; short SKUs may pass or fail depending on whether `[StringLength(20, MinimumLength = 3)]` is present on the entity — and annotation-only configuration on **CatalogProduct** means no Fluent safety net for money or indexes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `UnitPrice = 12.3456789m` with default scale 2 | Stored as `12.35` — silent financial drift |
| Validation gap | `Sku = "GDG"` (3 chars) — edge of `[StringLength]` min | May pass validation but business rules may require longer SKUs; empty-name case not shown but same class of bug |
| Configuration | CatalogProduct has no `HasPrecision` / explicit money facet | Bulk import magnifies rounding across thousands of rows |
| Observability | SaveChanges succeeds | No exception; reconciliation finds discrepancies later |

**Fix (priority order):**

1. Add explicit money mapping: `HasPrecision(19, 4)` (or team standard) via Fluent or `[Precision]` on `UnitPrice`; add migration.
2. Validate in domain/service before `AddRange`: scale, min SKU length, non-empty name — do not rely on DB rounding as validation.
3. Consider `[StringLength(20, MinimumLength = 3)]` + `[Required(AllowEmptyStrings = false)]` consistently (as in chapter **CatalogProduct**); fail fast before `SaveChanges`.
4. For bulk paths, use transactions and row-level error reporting; optionally `ExecuteUpdate`/bulk extensions with explicit column types.
5. Add tests that assert inserted `UnitPrice` equals source after read-back from SQL.

**Production takeaway:** EF SaveChanges success only means constraints satisfied — not that business precision or string rules held. Pair annotations/Fluent from this chapter with **pre-save domain validation** for money and identifiers. See **DemonstrateAnnotationValidation** in **Program.cs**.

---
