namespace CodeFirstModelsAndMigrations.Models;

/*
 * FILE ROLE: Second entity demonstrating foreign-key and relationship conventions.
 *
 * SECTIONS IN THIS FILE:
 *   1. Code-First entity classes (Product)
 *   2. EF Core conventions (foreign keys, required relationships)
 */

/*
 * =========================================================================
 * SECTION 1: CODE-FIRST ENTITY CLASSES - Product
 * =========================================================================
 *
 * Product lives in dbo.Products (plural of DbSet name). Each row represents
 * one sellable item in a category. Scalar properties become columns; navigation
 * properties describe relationships but do not become columns themselves.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: EF CORE CONVENTIONS - FOREIGN KEYS
 * =========================================================================
 *
 * EF Core discovers relationships through navigation properties:
 *
 *   Product.CategoryId  +  Product.Category  ->  FK from Products to Categories
 *
 * Rules applied automatically:
 *   - CategoryId column created in Products (FK property name = {Navigation}Id)
 *   - Relationship required by default when FK property is non-nullable int
 *   - Index on FK column (SQL Server provider)
 *   - Cascade delete for required relationships (SQL Server default)
 *
 * Shadow properties (FK columns with no CLR property) are supported but this
 * chapter uses an explicit CategoryId for clarity.
 *
 * Relationship cardinality, cascade rules, and many-to-many ->
 * COVERED IN DETAIL LATER -> 06. Relationships & Navigation Properties.
 * -------------------------------------------------------------------------
 */
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int CategoryId { get; set; }              // FK scalar - convention pairs with Category navigation
    public Category Category { get; set; } = null!;  // navigation - required reference (null! defers to EF materialization)

    public override string ToString()
    {
        return $"{ProductId,2} | {Name,-14} | ${UnitPrice,7:F2} | CategoryId {CategoryId}";
    }
}
