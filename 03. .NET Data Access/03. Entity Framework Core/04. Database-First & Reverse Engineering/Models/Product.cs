namespace DatabaseFirstAndReverseEngineering.Models;

/*
 * FILE ROLE:
 *   Scaffolded-style entity for dbo.Products with FK navigation to Category.
 *
 * SECTIONS IN THIS FILE:
 *   6. Generated Product entity
 */

/*
 * SECTION 6: GENERATED PRODUCT ENTITY
 *
 * Foreign keys become scalar FK property (CategoryId) plus optional navigation (Category).
 * Decimal columns map to decimal; identity columns map to int with ValueGeneratedOnAdd in fluent config.
 *
 * Re-scaffold overwrites this file - extend via Product.Partial.cs instead.
 * -------------------------------------------------------------------------
 */
public partial class Product
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int StockQuantity { get; set; }

    public virtual Category Category { get; set; } = null!;
}
