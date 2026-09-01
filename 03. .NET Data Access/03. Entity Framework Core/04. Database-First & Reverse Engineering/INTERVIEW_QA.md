# 04. Database-First & Reverse Engineering — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q25. What is database-first in EF Core?](#q25-what-is-database-first-in-ef-core)
- [Q26. How do you scaffold a `DbContext` from an existing database?](#q26-how-do-you-scaffold-a-dbcontext-from-an-existing-database)
- [Q27. When is database-first preferred over code-first?](#q27-when-is-database-first-preferred-over-code-first)
- [Q28. What are the limitations of reverse-engineered models?](#q28-what-are-the-limitations-of-reverse-engineered-models)
- [Q29. How do you refresh a scaffolded model after database schema changes?](#q29-how-do-you-refresh-a-scaffolded-model-after-database-schema-changes)
- [Q30. Can you combine scaffolded models with manual partial classes?](#q30-can-you-combine-scaffolded-models-with-manual-partial-classes)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 04. Database-First & Reverse Engineering

---

## Q25. What is database-first in EF Core?

**Concepts**
- existing schema as authoritative source
- dotnet ef dbcontext scaffold reverse engineering
- DBA-owned schema integration
- generated entity classes and DbContext

**Answer**

Database-first in EF Core means the relational schema already exists — often maintained by database administrators or legacy systems — and you generate C# entity classes and a `DbContext` from live tables using reverse engineering (scaffolding). The database remains the authoritative schema definition. Common when integrating with existing enterprise databases or stored-procedure-heavy systems; Scaffolding reads table metadata through the provider's information schema queries. Generated code reflects current columns, keys, and relationships as EF Core interprets them. Ongoing schema changes require re-scaffolding or manual model updates to stay aligned.

---

## Q26. How do you scaffold a `DbContext` from an existing database?

**Concepts**
- scaffold command with connection string and provider
- output directory and context name options
- table and schema filter flags
- force overwrite flag for refresh

**Answer**

Run `dotnet ef dbcontext scaffold "<connection-string>" Microsoft.EntityFrameworkCore.SqlServer` (or another provider package) with options for output directory, context name, and table filters. The command generates entity classes and a `DbContext` with `DbSet` properties and Fluent configuration. `--output-dir Models` and `--context-dir Data` organize generated files; `--tables Orders,Products` limits scaffolding to specific tables; `--schema dbo` filters by schema. `--data-annotations` emits attributes instead of fluent calls in `OnModelCreating`. Use `--force` to overwrite prior scaffold output after intentional regeneration.

---

## Q27. When is database-first preferred over code-first?

**Concepts**
- legacy database modernization pattern
- DBA-controlled schema change advisory
- multi-client shared schema constraint
- vendor database read-only integration

**Answer**

Database-first fits when the database predates the application, multiple clients share one schema, or organizational policy requires database administrators to own all structural changes. It also suits reporting over vendor databases where you cannot dictate schema from C#. Legacy modernization layers a .NET API over unchanged SQL Server schemas; Regulated environments where schema changes go through database change advisory boards, not application migrations alone. Read-heavy integration with third-party databases where scaffolding is faster than manual model authoring. Less ideal for greenfield apps where the team wants model-driven evolution entirely in the .NET repository.

---

## Q28. What are the limitations of reverse-engineered models?

**Concepts**
- point-in-time schema snapshot limitation
- awkward column-name-to-property mapping
- re-scaffold overwrite of customizations
- missing business rules from triggers

**Answer**

Scaffolded models reflect the database at one point in time and may include naming mismatches, missing navigation ergonomics, and no domain logic. They can misinterpret views, triggers, complex keys, or provider-specific types without manual correction. Table and column names map literally — `cust_nm` becomes awkward property names unless renamed in partial classes; Not all database constructs scaffold cleanly: table-valued functions, certain composite keys, or undocumented views. Re-scaffolding overwrites generated files unless you isolate custom code in partial classes. Scaffolding does not capture business rules enforced only in triggers or check constraints as C# validation.

---

## Q29. How do you refresh a scaffolded model after database schema changes?

**Concepts**
- force re-scaffold command workflow
- partial class isolation for custom code
- manual entity edit plus migration alternative
- scaffold command documentation for repeatability

**Answer**

Re-run `dotnet ef dbcontext scaffold` with the same options and `--force` to regenerate entities and context, then merge any custom partial class extensions that were not overwritten. Alternatively, hand-edit the model and add a code-first migration if you have switched to owning schema from the application. Compare diffs carefully — regenerated files replace prior scaffold output entirely; Keep custom logic in `*.Partial.cs` files or separate configuration classes excluded from overwrite. For small changes, manual entity updates plus a migration may be less disruptive than full re-scaffold. Document the scaffold command in the repository so the team reproduces identical output.

---

## Q30. Can you combine scaffolded models with manual partial classes?

**Concepts**
- partial class pattern for customization
- re-scaffold safety with separate files
- IEntityTypeConfiguration for additional mapping
- hybrid long-term maintenance approach

**Answer**

Yes. EF Core scaffolding generates partial classes intentionally — you add `Product.Partial.cs` with the same `partial class Product` to attach computed properties, methods, or interfaces without touching regenerated files. Partial `DbContext` classes extend `OnModelCreating` with hand-written Fluent API. Regenerated scaffold files stay overwrite-safe; partials persist across re-scaffold; Use partials for `[NotMapped]` properties, validation helpers, and domain behavior. Additional `IEntityTypeConfiguration<T>` classes configure mapping without editing generated context code. This hybrid pattern is the standard way to maintain database-first models long term.

---

## Gotchas

---

## Gotcha 4. Leaked connections exhaust the pool

**Concepts**
- connection pool exhaustion from undisposed connections
- await using for guaranteed disposal
- DbContext undisposed as equivalent risk

**Answer**

Failing to dispose `SqlConnection`, `SqlDataReader`, or abandoning a `using` block early leaks connection pool slots until timeout, eventually causing "timeout expired obtaining connection from pool" errors under load. Always use `await using` for connections and readers so disposal runs on exceptions too; Symptoms appear only under concurrent load, making this a classic production-only failure mode. Long-lived undisposed `DbContext` instances cause the same exhaustion pattern.

---

## Scenario-Based Questions (Karat Format)

---

## Q114. (R) A teammate "fixes" scaffold output by adding a computed helper directly in the generated entity, then runs re-scaffold with `--force` after a DBA adds a column. Review what they changed and predict what happens on the next build.

```csharp
// Models/Product.cs  (generated — edited by hand)
public partial class Product
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public virtual Category Category { get; set; } = null!;

    // Added during code review — "keeps logic near the entity"
    public decimal InventoryValue => UnitPrice * StockQuantity;
    public string StockStatus => StockQuantity > 0 ? "Available" : "Out of stock";
}
```

**Concepts**
- custom code in generated entity overwritten by re-scaffold
- partial class vs generated file isolation
- force re-scaffold overwrite behavior
- compile-time failure from duplicate members

**Answer**

Hand-editing `Product.cs` violates the database-first contract — `--force` replaces every generated entity file, so `InventoryValue` and `StockStatus` disappear while the new column appears, and any code referencing those members fails to compile until the logic is moved to a partial file the scaffold never touches.

1. Move `InventoryValue` and `StockStatus` to `Models/Product.Partial.cs` with the same namespace and `partial class Product` (as in this chapter's tutorial layout).
2. Re-run scaffold with `--force`; verify `Product.cs` is regenerated cleanly and `Product.Partial.cs` is untouched.
3. Add a team rule: **never edit** `Models/*.cs` or `Data/*DbContext.cs` after initial scaffold — only `*.Partial.cs` and non-generated services.
4. Document the exact scaffold command in README or `ScaffoldWorkflow.SampleReScaffoldCommand` so everyone produces identical output.

---

## Q115. (R) Review this partial-class attempt. The developer wants `DisplayLabel` and a `[NotMapped]` cache field to survive re-scaffold. What fails at compile time or at runtime, and how should the files be organized?

```csharp
// Models/CategoryHelpers.cs
namespace InventoryApp.Models;

public class CategoryHelpers
{
    public static string DisplayLabel(Category c) =>
        string.IsNullOrWhiteSpace(c.Description) ? c.Name : $"{c.Name} - {c.Description}";
}
```

```csharp
// Models/Category.Partial.cs
namespace InventoryApp.Models;

public partial class Category
{
    private Dictionary<int, int>? _productCountCache;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int CachedProductCount => _productCountCache?.Count ?? 0;
}
```

```csharp
// Models/Category.cs  (scaffolded)
namespace InventoryApp.Models;

public partial class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
```

**Concepts**
- partial class file organization for re-scaffold safety
- NotMapped property and cache field placement
- generated vs partial class boundary
- runtime behavior from wrong partial placement

**Answer**

`CategoryHelpers` is a separate static class, so `category.DisplayLabel` does not exist on the entity — views or services calling that member fail at compile time. The partial file structure is otherwise correct for `[NotMapped]` members, but the display helper must live on `partial class Category`, not a sibling helper type.

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

---

## Q116. (P) Production integrates with a legacy SQL Server database owned by a DBA team. They deploy a script renaming `dbo.Products.UnitPrice` to `ListPrice` and changing `StockQuantity` from `INT` to `SMALLINT`. Your team runs:

```bash
dotnet ef dbcontext scaffold "<conn>" Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir Models --context-dir Data --context InventoryDbContext --force
dotnet build
```

**Concepts**
- DBA column rename and type change causing EF mismatch
- dotnet ef dbcontext scaffold refresh workflow
- runtime InvalidOperationException from schema drift
- model update priority after DBA schema change

**Answer**

_Answer not found._

---

## Q117. (D) You inherit a 15-year-old ERP database: `dbo` holds ~180 operational tables, `audit` holds change-log tables, and several tables use non-plural names (`tblCustMaster`, `OrdLine`). The app only reads `dbo.Categories`, `dbo.Products`, and `dbo.OrdLine`. How do you scaffold without dragging the entire schema into the model, and what **re-scaffold drift** risks appear when a new developer scaffolds without the same filters?

**Concepts**
- selective scaffold with table and schema filters
- legacy ERP with 180-table schema
- re-scaffold drift from inconsistent filter flags
- team scaffold command documentation

**Answer**

_Answer not found._

---

## Q118. (R) Before re-scaffold, a developer tuned delete behavior and decimal precision directly in the generated DbContext. Review the diff they intend to keep "because scaffold gets it wrong":

```csharp
// Data/InventoryDbContext.cs  (generated — hand-edited)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.ProductId);
        entity.ToTable("Products");
        entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)"); // DBA uses 4 dp
        entity.HasOne(d => d.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // legacy DB forbids cascade
    });
    // OnModelCreatingPartial not called — removed "by mistake" during edit
}
```

**Concepts**
- developer edits to generated DbContext overwritten
- delete behavior and decimal precision in generated code
- partial class and IEntityTypeConfiguration for customization
- re-scaffold overwrite risk on generated files

**Answer**

_Answer not found._

---

## Q119. (M) Two developers re-scaffold the same legacy database on different branches. Dev A scaffolds with `--schema dbo --no-pluralize`; Dev B runs scaffold with no filters on a default template. Both commit generated files. At merge, `InventoryDbContext.cs` and entity files conflict heavily. Explain the **mechanism** of scaffold output drift and the team process you would put in place so re-scaffold stays repeatable (command, filters, partials, CI).

**Concepts**
- divergent scaffold output from different filter flags
- merge conflict on InventoryDbContext and entities
- repeatable scaffold command with documented filters
- partial class CI enforcement strategy

**Answer**

_Answer not found._
