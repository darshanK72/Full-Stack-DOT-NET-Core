# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/03. Code-First Models & Migrations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Two developers branch from the same commit where `InitialCreate` is the only migration. Alice adds `AddProductSku` on Monday; Bob adds `AddCategoryDescription` on Tuesday. Both run `dotnet ef migrations add` locally against the same snapshot. After merge, the repo has two timestamped migrations whose Designer files both list `InitialCreate` as the parent, and Git conflicted on `StoreDbContextModelSnapshot.cs`. CI runs `dotnet ef database update` on a fresh LocalDB and fails. What went wrong, and how do you resolve it without losing either schema change?

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

---

#### Q4. (P) A developer adds `public int StockQuantity { get; set; }` to `Product`, merges to `main`, and deploys. The pipeline builds and publishes the app but **does not** run `dotnet ef database update`. Production startup calls `context.Database.Migrate()` as in this chapter's `MigrationDemo.TryApplyMigrations`. Production database last applied migration is still `InitialCreate`. What fails first — build, startup, or first query — and what is the correct deploy sequence for schema changes?

---

#### Q5. (R) A throwaway integration test project created the `EfCoreTutorial` database with `EnsureCreated()`. The main app (this chapter) is deployed and calls `Database.Migrate()` on startup. Startup logs:

```
SqlException: There is already an object named 'Categories' in the database.
```

Review the catch block in this chapter's `MigrationDemo.TryApplyMigrations` — what state is the database in, why does `Migrate()` fail, and what are the safe recovery options for dev vs production?

---

#### Q6. (D) Your team debates where migrations run for the `StoreDbContext` / SQL Server app: **(A)** `Database.Migrate()` in `Program.cs` on every app startup, **(B)** `dotnet ef database update` in the CI/CD pipeline before swapping traffic, or **(C)** generating idempotent SQL scripts for DBAs to run manually. Under multi-instance Kubernetes, blue/green deploys, and strict change windows, which option(s) do you recommend and why?
