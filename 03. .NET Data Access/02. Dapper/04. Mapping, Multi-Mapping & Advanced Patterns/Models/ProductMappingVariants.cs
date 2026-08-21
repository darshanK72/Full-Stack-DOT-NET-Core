namespace DapperMappingAndAdvancedPatterns.Models;

/*
 * FILE ROLE:
 *   Row types demonstrating SQL column aliases and [Column] attribute mapping (SECTION 2).
 *
 * SECTIONS IN THIS FILE:
 *   2a. SQL column aliases (AS) — primary Dapper approach
 *   2b. [Column] attribute — requires custom TypeMap registration
 */

/*
 * SECTION 2a: SQL COLUMN ALIASES (AS) — PRIMARY DAPPER APPROACH
 *
 * When C# property names differ from database column names, alias columns in SQL:
 *
 *   SELECT ProductId AS Id, ProductName AS Title, UnitPrice AS Price FROM dbo.Products
 *
 * Dapper maps alias names to properties — no attributes or config required.
 * -------------------------------------------------------------------------
 */
public sealed class ProductWithAlias
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

/*
 * SECTION 2b: [Column] ATTRIBUTE — REQUIRES CUSTOM TYPE MAP
 *
 * System.ComponentModel.DataAnnotations.Schema.Column documents the DB column name,
 * but Dapper does NOT read [Column] by default (unlike EF Core).
 *
 * Price property maps to UnitPrice column once ColumnAttributeTypeMap.RegisterOnce() runs.
 * -------------------------------------------------------------------------
 */
public sealed class ColumnMappedProduct
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Schema.Column("UnitPrice")]
    public decimal Price { get; set; }
}
