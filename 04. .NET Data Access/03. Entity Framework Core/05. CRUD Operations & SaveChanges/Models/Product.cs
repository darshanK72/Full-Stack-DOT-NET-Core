using System;

namespace CrudOperationsAndSaveChanges.Models;

/*
 * FILE ROLE: Code-First entity mapped to dbo.Products — the type every CRUD
 *            operation in this chapter creates, reads, updates, or deletes.
 *
 * SECTIONS IN THIS FILE:
 *   1. Product entity for CRUD operations
 */

/*
 * =========================================================================
 * SECTION 1: PRODUCT ENTITY FOR CRUD OPERATIONS
 * =========================================================================
 *
 * An entity is a plain C# class that EF Core maps to a database table.
 * Conventions (no fluent config yet — see ch07) infer:
 *
 *   Class Product     -> table Products (pluralized DbSet name)
 *   ProductId         -> primary key (Id or {TypeName}Id suffix)
 *   ProductName, etc. -> columns with matching names
 *
 * CRUD at the database layer:
 *
 *   Operation | EF Core API (this chapter)     | SQL generated on SaveChanges
 *   ----------|--------------------------------|----------------------------
 *   Create    | DbSet.Add(entity)              | INSERT
 *   Read      | DbSet.Find / FirstOrDefault    | SELECT (LINQ depth -> ch08)
 *   Update    | change tracked props / Update()| UPDATE
 *   Delete    | DbSet.Remove(entity)           | DELETE
 *
 * Properties use { get; set; } so EF can materialize rows from SELECT results
 * and write back changes. init-only works for read-only scenarios; set is
 * required for typical tracked updates in this demo.
 *
 * Relationships and navigation properties are intentionally omitted here —
 * COVERED IN DETAIL LATER -> 06. Relationships & Navigation Properties.
 * -------------------------------------------------------------------------
 */
public sealed class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsDiscontinued { get; set; }

    public override string ToString()
    {
        string status = IsDiscontinued ? "discontinued" : "active";
        return $"{ProductId,2} | {ProductName,-14} | ${UnitPrice,7:F2} | stock {StockQuantity,3} | {status}";
    }
}
