# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/03. Code-First Models & Migrations`

---

#### Q1. (R) Two developers branch from the same commit where `InitialCreate` is the only migration. Alice adds `AddProductSku` on Monday; Bob adds `AddCategoryDescription` on Tuesday. Both run `dotnet ef migrations add` locally against the same snapshot. After merge, the repo has two timestamped migrations whose Designer files both list `InitialCreate` as the parent, and Git conflicted on `StoreDbContextModelSnapshot.cs`. CI runs `dotnet ef database update` on a fresh LocalDB and fails. What went wrong, and how do you resolve it without losing either schema change?

**Answer:** Both developers generated sibling migrations from the same parent snapshot, so EF Core sees a branched migration history — only one linear chain can be applied, and the merged `StoreDbContextModelSnapshot.cs` cannot represent two divergent model states at once.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Workflow | Parallel `migrations add` from identical base | Two migrations with same parent; non-linear history |
| Source control | Conflicting `StoreDbContextModelSnapshot.cs` | Snapshot out of sync with one or both migration files |
| CI/runtime | `dotnet ef database update` on clean DB | Second sibling migration may never apply or build metadata fails |
| Team process | No "migration lock" or rebase-before-add rule | Repeatable merge pain on every parallel schema change |

**Fix (priority order):**

1. **Pick one branch's migration to keep temporarily** — apply it locally on a scratch database to preserve its DDL intent.
2. **Remove the conflicting sibling migration(s)** from `Migrations/` (both `.cs`, `.Designer.cs`, and reset snapshot if needed).
3. **Merge model changes in C# first** — combine Alice's `Sku` property and Bob's `Description` on `Product`/`Category` in one working tree.
4. Run **`dotnet ef migrations add CombinedProductAndCategoryChanges`** to produce a **single** new migration and one authoritative snapshot.
5. On databases that already applied one sibling migration (dev machines), either `dotnet ef database drop` (dev only) or manually reconcile `__EFMigrationsHistory` and schema — never do this casually in production.
6. **Prevent recurrence:** short-lived feature branches, pull latest before `migrations add`, or designate one "schema owner" per sprint; some teams add a CI check that migration timestamps are strictly increasing with a single parent chain.

**Production takeaway:** Migrations are version-controlled DDL — treat parallel adds like parallel edits to one file. The snapshot is the merge conflict surface; the fix is one linear migration chain, not hand-editing two sibling `Up()` methods. See **Program.cs** Quick Reference — "Edit applied migration Up() by hand → history out of sync."

---

#### Q2. (R) A developer renames a column in the chapter's `Product` entity to match API naming:

```csharp
// Before
public decimal UnitPrice { get; set; }

// After
public decimal Price { get; set; }
```

They run `dotnet ef migrations add RenameUnitPriceToPrice` and inspect the generated `Up()`:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "UnitPrice",
        table: "Products");

    migrationBuilder.AddColumn<decimal>(
        name: "Price",
        table: "Products",
        type: "decimal(18,2)",
        nullable: false,
        defaultValue: 0m);
}
```

The migration has not been applied to production yet, but staging already has 50,000 product rows. What is wrong with shipping this migration as-is, and what would you change?

**Answer:** EF Core treats an unannotated property rename as drop-then-add, which **destroys existing `UnitPrice` values** and briefly leaves every row at the `defaultValue` of `0m` — silent data loss on staging/production.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `DropColumn` + `AddColumn` instead of rename | All historical prices wiped |
| Data | `defaultValue: 0m` on non-nullable column | Every product shows $0.00 after migrate |
| Operations | Looks like a safe migration in code review | Passes CI on empty LocalDB; fails in real data environments |

**Fix (priority order):**

1. **Before applying anywhere with data**, remove or revert the bad migration (`dotnet ef migrations remove` if not yet applied to shared DBs).
2. Regenerate with an explicit rename — **Package Manager Console:** `Add-Migration RenameUnitPriceToPrice` after `[Column("UnitPrice")]` on `Price` then second migration, or prefer editing `Up()` to:
   ```csharp
   migrationBuilder.RenameColumn(
       name: "UnitPrice",
       table: "Products",
       newName: "Price");
   ```
3. For complex renames, use **`dotnet ef migrations add ...` then hand-edit `Up()`/`Down()`** before first apply to shared environments — never edit after apply.
4. Validate on a **copy of staging data**: `SELECT COUNT(*) FROM Products WHERE Price = 0` should not jump to 50,000.
5. Add a team rule: **review generated `Up()` for `DropColumn`/`AddColumn` pairs** on entity renames — flag as data-loss risk in PR template.

**Production takeaway:** Code-First renames are not rename-safe by default; the generated migration is a suggestion. Karat tests whether you read `Up()` DDL, not just the C# property name. See **Models/Product.cs** — `UnitPrice` maps by convention; renaming the property changes the column mapping unless you intervene.

---

#### Q3. (R) A teammate "fixes" seeding so every deploy refreshes demo catalog data. Review the change to the chapter's `SeedSampleDataIfEmpty` pattern:

```csharp
public static void SeedCatalog(StoreDbContext context)
{
    if (!context.Categories.Any(c => c.Name == "Office"))
    {
        context.Categories.Add(new Category { Name = "Office" });
    }
    if (!context.Categories.Any(c => c.Name == "Field"))
    {
        context.Categories.Add(new Category { Name = "Field" });
    }
    context.SaveChanges();

    var officeId = context.Categories.Single(c => c.Name == "Office").CategoryId;

    context.Products.AddRange(
        new Product { Name = "Notebook", UnitPrice = 4.50m, CategoryId = officeId },
        new Product { Name = "Pen Pack", UnitPrice = 6.25m, CategoryId = officeId });
    context.SaveChanges();
}
```

`Program.Main` now calls `SeedCatalog(context)` on every startup after `Database.Migrate()`. Locally it looks fine; after two restarts in staging, duplicate `"Notebook"` rows appear and integration tests fail on product counts. Diagnose the idempotency gaps and describe a production-safe seeding approach.

**Answer:** Categories are guarded by name, but **products are inserted unconditionally on every startup**, so each restart adds another `"Notebook"` and `"Pen Pack"` — the chapter's original `if (context.Products.Any()) return;` guard was removed only halfway.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No idempotency check before `Products.AddRange` | Duplicate rows every deploy/restart |
| Design | Name-based category guard but not for products | Inconsistent seed contract |
| Testing | Passes first run; fails on second | Flaky integration tests in staging |
| Production | Startup seeding with partial guards | Data pollution in long-lived environments |

**Fix (priority order):**

1. Restore a **single gate** — either `if (context.Products.Any()) return;` (chapter pattern in **MigrationDemo.SeedSampleDataIfEmpty**) or check each product by natural key before insert.
2. Use **natural-key upsert** for reference data:
   ```csharp
   if (!context.Products.Any(p => p.Name == "Notebook"))
       context.Products.Add(new Product { ... });
   ```
3. Prefer **`HasData` in `OnModelCreating`** for static lookup tables — runs through migrations, versioned with schema (see EF Core seeding docs); use `Migrate()` once, not per-startup inserts.
4. For production reference data, use **idempotent SQL scripts** or a dedicated one-time migration/`INSERT ... WHERE NOT EXISTS` — not `Main` on every pod restart.
5. Separate **demo seed** (dev only) from **production bootstrap** (migrations/SQL) — gate demo seed behind `IHostEnvironment.IsDevelopment()`.

**Production takeaway:** Idempotent seeding means "safe to run N times" — guard every entity type or use migration-embedded seed. The chapter's `SeedSampleDataIfEmpty` is intentionally empty-check-first; partial guards are worse than no seed. See **Program.cs** Main — seed runs after `Migrate()`, so it executes on every instance in a scale-out deploy.

---

#### Q4. (P) A developer adds `public int StockQuantity { get; set; }` to `Product`, merges to `main`, and deploys. The pipeline builds and publishes the app but **does not** run `dotnet ef database update`. Production startup calls `context.Database.Migrate()` as in this chapter's `MigrationDemo.TryApplyMigrations`. Production database last applied migration is still `InitialCreate`. What fails first — build, startup, or first query — and what is the correct deploy sequence for schema changes?

**Answer:** **Build succeeds** — the new property is only C# until a migration exists. If no new migration was generated, **startup `Migrate()` is a no-op** (still at `InitialCreate`), and the **first query or save involving `StockQuantity` fails** at runtime with a column mismatch (`Invalid column name 'StockQuantity'`) or silent mis-mapping if the column is missing from SELECT projections.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Process | Model changed without `dotnet ef migrations add` | App expects column that DB does not have |
| Deploy | Pipeline publishes app without schema step | Runtime failure after deploy |
| Assumption | "`Migrate()` on startup fixes everything" | Only applies **checked-in** migrations — not pending model edits |

**Correct deploy sequence (priority order):**

1. **Developer:** change entity → `dotnet ef migrations add AddProductStockQuantity` → commit migration + snapshot → PR review `Up()` DDL.
2. **CI:** build + test against database updated in test job (`dotnet ef database update` on LocalDB/Testcontainers).
3. **Deploy:** run **`dotnet ef database update`** (or apply generated SQL script) **before or as first step** of release — while old app still running if migration is backward-compatible, or during maintenance window if not.
4. **Then** deploy new app bits. If using startup `Migrate()`, treat it as a **safety net**, not the primary schema pipeline — and use **single-instance migration lock** or run migrations outside the web process to avoid races.
5. **Verify:** `SELECT MigrationId FROM __EFMigrationsHistory` includes the new migration; smoke test `Products` read/write.

**Production takeaway:** Pending **model** changes ≠ pending **migrations**. `Database.Migrate()` only replays files under `Migrations/` — see **MigrationDemo.TryApplyMigrations** and **Program.cs** Quick Reference row "Model change without new migration → Runtime errors / column mismatch."

---

#### Q5. (R) A throwaway integration test project created the `EfCoreTutorial` database with `EnsureCreated()`. The main app (this chapter) is deployed and calls `Database.Migrate()` on startup. Startup logs:

```
SqlException: There is already an object named 'Categories' in the database.
```

Review the catch block in this chapter's `MigrationDemo.TryApplyMigrations` — what state is the database in, why does `Migrate()` fail, and what are the safe recovery options for dev vs production?

**Answer:** The database has **physical tables but no (or incomplete) `__EFMigrationsHistory`** — `EnsureCreated()` built schema without recording migrations, so `Migrate()` tries to run `InitialCreate.Up()` and hits existing `Categories`/`Products` objects.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Schema path | `EnsureCreated()` then `Migrate()` on same database | Two incompatible schema evolution paths |
| History | Missing or empty `__EFMigrationsHistory` | EF thinks no migrations applied |
| Runtime | `CreateTable` in `Up()` against existing objects | SqlException — startup fails or demo skips |

**Fix (priority order):**

**Development / local (data disposable):**

1. `dotnet ef database drop --force` then `dotnet ef database update` — cleanest path per chapter message in **MigrationDemo** catch block.
2. Or drop only conflicting tables + history table, then `Migrate()`.

**Production (data must survive — rare for this exact mistake, but same class of problem):**

1. **Do not** `drop database` if data matters.
2. **Baseline** the database: create `__EFMigrationsHistory` if missing, insert row for `InitialCreate` (and any subsequent migrations whose DDL already exists) after verifying schema matches snapshot — effectively telling EF "these are already applied."
3. Generate **`dotnet ef migrations script`** and compare to live schema; fix drift manually with DBA review.
4. **Prevent:** ban `EnsureCreated()` outside throwaway tests; use dedicated test databases; document one schema path per environment.

**Production takeaway:** `EnsureCreated()` and migrations must never share a long-lived database — **MigrationCliReference.ExplainEnsureCreatedVsMigrate** and **Program.cs** Quick Reference. The chapter's catch handles this explicitly for LocalDB demos; production recovery is baseline/history repair, not blind drop.

---

#### Q6. (D) Your team debates where migrations run for the `StoreDbContext` / SQL Server app: **(A)** `Database.Migrate()` in `Program.cs` on every app startup, **(B)** `dotnet ef database update` in the CI/CD pipeline before swapping traffic, or **(C)** generating idempotent SQL scripts for DBAs to run manually. Under multi-instance Kubernetes, blue/green deploys, and strict change windows, which option(s) do you recommend and why?

**Answer:** Prefer **(B) pipeline-applied migrations** or **(C) reviewed idempotent scripts** for production; use **(A) startup `Migrate()`** only for dev, test, and small single-instance apps — not as the primary strategy when multiple pods start concurrently or DBAs own change windows.

**Option A — `Database.Migrate()` at startup:**

- **Pros:** Simple; app self-heals dev/test; matches this chapter's **MigrationDemo** demo.
- **Cons:** **Race conditions** when N pods call `Migrate()` simultaneously; migrations run inside app security context; long `Up()` blocks startup probes; hard to fit strict maintenance windows; rollback couples app + schema.

**Option B — `dotnet ef database update` in CI/CD (recommended default):**

- Run as a **dedicated deploy step/job** before traffic shift; one execution per release.
- Use migration bundles (`dotnet ef migrations bundle`) for self-contained executables without SDK on agents.
- Pair with backward-compatible migrations when doing rolling deploys (expand-contract pattern).

**Option C — idempotent SQL scripts for DBAs:**

- Best when **separation of duties** requires DBA approval, auditing, or manual rollback scripts.
- `dotnet ef migrations script --idempotent` emits `IF NOT EXISTS` guards against partial applies.
- App deploy and schema deploy decouple — fits change windows; slower feedback loop.

**Recommendation matrix:**

| Context | Choice |
|---|---|
| Local / chapter demo | `Migrate()` in `Main` — **Program.cs** |
| CI integration tests | `database update` on ephemeral DB |
| Production K8s, multi-instance | **B** or **C** — never N parallel startup migrators |
| Regulated / DBA-owned SQL Server | **C** with reviewed scripts + history table verification |

**Production takeaway:** The mechanism (`Migrate()` vs CLI vs script) matters less than **exactly-once, ordered application** before new code depends on new columns — Karat tests deploy judgment tied to **StoreDbContext** and `__EFMigrationsHistory`, not CLI trivia. See **MigrationCliReference** Section 6 — production often runs updates from CI/CD instead of app startup.
