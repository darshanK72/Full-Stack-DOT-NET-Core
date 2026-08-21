# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/04. Database-First & Reverse Engineering`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A teammate "fixes" scaffold output by adding a computed helper directly in the generated entity, then runs re-scaffold with `--force` after a DBA adds a column. Review what they changed and predict what happens on the next build.

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

They did **not** create `Product.Partial.cs`. After `dotnet ef dbcontext scaffold ... --force`, CI reports missing members where services call `product.InventoryValue`. What went wrong, and what is the correct file layout?

---

#### Q2. (R) Review this partial-class attempt. The developer wants `DisplayLabel` and a `[NotMapped]` cache field to survive re-scaffold. What fails at compile time or at runtime, and how should the files be organized?

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

Assume scaffold ran successfully once. The app calls `category.DisplayLabel` in a Razor view. What breaks?

---

#### Q3. (P) Production integrates with a legacy SQL Server database owned by a DBA team. They deploy a script renaming `dbo.Products.UnitPrice` to `ListPrice` and changing `StockQuantity` from `INT` to `SMALLINT`. Your team runs:

```bash
dotnet ef dbcontext scaffold "<conn>" Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir Models --context-dir Data --context InventoryDbContext --force
dotnet build
```

Walk through the **re-scaffold workflow** in order: what the scaffold overwrites, what survives, what fails first in the solution, and where fixes belong (generated vs partial vs app code).

---

#### Q4. (D) You inherit a 15-year-old ERP database: `dbo` holds ~180 operational tables, `audit` holds change-log tables, and several tables use non-plural names (`tblCustMaster`, `OrdLine`). The app only reads `dbo.Categories`, `dbo.Products`, and `dbo.OrdLine`. How do you scaffold without dragging the entire schema into the model, and what **re-scaffold drift** risks appear when a new developer scaffolds without the same filters?

---

#### Q5. (R) Before re-scaffold, a developer tuned delete behavior and decimal precision directly in the generated DbContext. Review the diff they intend to keep "because scaffold gets it wrong":

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

They run `--force` re-scaffold next sprint. What regressions appear in production behavior and compile output?

---

#### Q6. (M) Two developers re-scaffold the same legacy database on different branches. Dev A scaffolds with `--schema dbo --no-pluralize`; Dev B runs scaffold with no filters on a default template. Both commit generated files. At merge, `InventoryDbContext.cs` and entity files conflict heavily. Explain the **mechanism** of scaffold output drift and the team process you would put in place so re-scaffold stays repeatable (command, filters, partials, CI).

---
