using System;

namespace RawSqlAndStoredProcedures.Models;

/*
 * FILE ROLE: Product entity mapped to dbo.Products — the target type for FromSqlRaw and
 *            stored procedures that return the full product row shape.
 *
 * SECTIONS IN THIS FILE:
 *   1. Product entity — maps to dbo.Products
 */

/*
 * SECTION 1: PRODUCT ENTITY — MAPS TO dbo.Products
 *
 * FromSqlRaw and stored-procedure queries that SELECT entity columns materialize into this
 * type when you call DbSet<Product>.FromSqlRaw(...).
 *
 * Column mapping (same idea as Dapper default mapping and ADO.NET reader column names):
 *
 *   SQL column         | C# property      | Notes
 *   -------------------|------------------|------------------------------------------
 *   ProductId          | ProductId        | PK — must appear in SELECT for tracking
 *   ProductName        | ProductName      | required string
 *   UnitPrice          | UnitPrice        | decimal(10,2) in SQL
 *   StockQuantity      | StockQuantity    | int
 *   DiscontinuedDate   | DiscontinuedDate | nullable DateTime — NULL = active product
 *
 * Pitfall: If your raw SQL omits a non-nullable property, EF throws at materialization.
 * Pitfall: Column aliases must match property names unless you configure [Column("Alias")].
 *
 * COVERED IN EF CORE ch.03 → Code-First Models & Migrations (entity conventions).
 * COVERED IN DAPPER ch.04 → Mapping (column-to-property name matching).
 */
public sealed class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public DateTime? DiscontinuedDate { get; set; }

    public bool IsDiscontinued => DiscontinuedDate.HasValue; // derived — not a DB column
}
