# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/07. Fluent API & Data Annotations`
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) After a schema review, QA reports that `CatalogProducts.Name` truncates at 50 characters even though the entity still shows `[MaxLength(100)]`. Review the configuration split across files. What conflicted, what actually landed in SQL Server, and how do you fix it so the team is not surprised again?

```csharp
// Models/CatalogProduct.cs
[Required(AllowEmptyStrings = false)]
[MaxLength(100)]
public string Name { get; set; } = string.Empty;

// Data/CatalogDbContext.cs — added during a hotfix
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<CatalogProduct>(entity =>
    {
        entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
    });
}
```

---

#### Q2. (R) Finance rejects migrated totals: penny amounts drift after bulk price imports. The team points at `CatalogProduct.UnitPrice`. Review the entity and migration snippet. What is wrong with the money mapping, and what do you change before the next deploy?

```csharp
// Models/CatalogProduct.cs
public decimal UnitPrice { get; set; }

// Generated migration Up()
migrationBuilder.CreateTable(
    name: "CatalogProducts",
    columns: table => new
    {
        UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
    });
```

A separate branch added Fluent configuration:

```csharp
entity.Property(e => e.UnitPrice).HasPrecision(19, 4);
```

…but left `[Column(TypeName = "decimal(18,2)")]` on the property from an earlier scaffold.

---

#### Q3. (R) Production paging on order history is slow after go-live. The `OrderLines` table has ~2M rows. Review the relationship configuration — FK exists, but DBAs see a table scan on every lookup by product. What is missing, and how do you fix it in EF Core?

```csharp
public class OrderLine
{
    public int OrderLineId { get; set; }
    public int CatalogProductId { get; set; }
    public int Quantity { get; set; }
    public CatalogProduct Product { get; set; } = null!;
}

// CatalogDbContext.OnModelCreating
modelBuilder.Entity<OrderLine>(entity =>
{
    entity.HasOne(e => e.Product)
          .WithMany()
          .HasForeignKey(e => e.CatalogProductId)
          .OnDelete(DeleteBehavior.Restrict);
    // no index configuration
});
```

---

#### Q4. (R) With `#nullable enable`, a developer "fixes" compiler warnings on `CatalogProduct` but SaveChanges behavior changes in staging. Review the property declarations. What do nullable reference types vs `[Required]` each control, and what would you standardize?

```csharp
#nullable enable

public class CatalogProduct
{
    public int CatalogProductId { get; set; }

    // Developer removed [Required] — "string is already non-nullable"
    public string Name { get; set; } = string.Empty;

    [Required]
    public string? Sku { get; set; }

    public string? Description { get; set; }
}
```

---

#### Q5. (P) Your team inherits scaffolded `Supplier` rows from a legacy database and adds `[StringLength(20)]` on `SupplierCode` while `CatalogDbContext` already calls `HasIndex(e => e.SupplierCode).IsUnique()` and `HasMaxLength(20)` in Fluent API. Builds succeed; tests pass locally. What production risks remain if you leave both styles on the same property, and what convention would you enforce?

---

#### Q6. (R) A pricing microservice bulk-inserts catalog rows. One bad batch passes C# compilation and EF model validation but corrupts amounts in SQL Server. Review the insert path and configuration. List the issues and prioritized fixes.

```csharp
var batch = new[]
{
    new CatalogProduct { Name = "Widget", Sku = "WDG-001", UnitPrice = 12.3456789m },
    new CatalogProduct { Name = "Gadget", Sku = "GDG", UnitPrice = 0m }
};

context.CatalogProducts.AddRange(batch);
await context.SaveChangesAsync();

// Entity — no precision facet
public decimal UnitPrice { get; set; }

// DbContext — only Supplier uses Fluent; CatalogProduct is annotation-only
```

---
