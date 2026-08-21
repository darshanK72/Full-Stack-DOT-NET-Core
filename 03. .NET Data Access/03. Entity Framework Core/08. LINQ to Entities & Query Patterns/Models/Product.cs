namespace LinqToEntitiesAndQueryPatterns.Models;

/*
 * FILE ROLE:
 *   Product entity  -  primary target for IQueryable, filtering, projection, and global filter demos.
 *
 * SECTIONS IN THIS FILE:
 *   1. Product entity
 */

/*
 * SECTION 1: PRODUCT ENTITY
 *
 * IsDiscontinued supports the global query filter preview in StoreDbContext
 * (soft-hide discontinued rows from every query unless IgnoreQueryFilters() is used).
 *
 * DbSet<Product> exposes IQueryable<Product>  -  the starting point for LINQ to Entities.
 * -------------------------------------------------------------------------
 */
public sealed class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public bool IsDiscontinued { get; set; }

    public Category? Category { get; set; } // many-to-one navigation (FK CategoryId)
}
