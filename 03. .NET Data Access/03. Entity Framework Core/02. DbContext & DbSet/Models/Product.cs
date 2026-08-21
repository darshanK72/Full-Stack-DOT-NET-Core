namespace DbContextAndDbSet.Models;

/*
 * FILE ROLE: Defines the Product entity type that AppDbContext exposes through DbSet<Product>.
 *
 * SECTIONS IN THIS FILE:
 *   1. Product entity POCO - maps to dbo.Products via EF Core conventions
 */

/*
 * SECTION 1: PRODUCT ENTITY POCO
 *
 * DbSet<T> requires T to be a class (reference type). EF Core maps public properties with
 * getters/setters to table columns using naming conventions:
 *
 *   C# type              | Default SQL mapping (SqlServer provider)
 *   ---------------------|------------------------------------------
 *   int Id               | INT PRIMARY KEY (convention: property named Id)
 *   string Name          | NVARCHAR(MAX) when no max length configured
 *   decimal UnitPrice    | DECIMAL(18, 2) by default for decimal
 *
 * Full fluent configuration and data annotations -> ch.07 Fluent API & Data Annotations.
 * Code-First migrations that evolve this schema -> ch.03 Code-First Models & Migrations.
 *
 * Pitfall: EF Core ignores fields without a public property accessor unless explicitly
 * configured. Use auto-properties for entities you want mapped.
 */
public sealed class Product
{
    public int Id { get; set; }                       // convention key when named Id
    public string Name { get; set; } = string.Empty; // non-null default avoids CS8618 warnings
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }

    public override string ToString() =>
        $"[{Id}] {Name} @ ${UnitPrice:F2} (stock {StockQuantity})";
}
