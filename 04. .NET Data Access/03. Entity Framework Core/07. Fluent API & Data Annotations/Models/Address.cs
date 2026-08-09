namespace FluentApiAndDataAnnotations.Models;

/*
 * FILE ROLE: Value object used as an owned type — columns stored on the parent row.
 *
 * SECTIONS IN THIS FILE:
 *   6. Owned types (preview)
 */

/*
 * =========================================================================
 * SECTION 6: OWNED TYPES (PREVIEW)
 * =========================================================================
 *
 * An *owned type* is a CLR class whose properties are persisted as columns on
 * the parent entity's table (or a separate table with a 1:1 FK — table-splitting
 * is advanced and omitted here).
 *
 * Use when a group of columns has no independent identity:
 *   Supplier.Address  ->  Street, City, PostalCode live on dbo.Suppliers
 *
 * Typical SQL shape (same table, column prefix by default):
 *   Suppliers.SupplierId, CompanyName, SupplierCode, Address_Street, Address_City, ...
 *
 * Configuration is Fluent API only in this chapter:
 *   entity.OwnsOne(e => e.Address, ...)  — see Data/CatalogDbContext.cs
 *
 * Owned types vs regular entities:
 *   Owned type     | No own DbSet<T>; always accessed through parent
 *   Regular entity | DbSet<T>; own table and primary key
 *
 * COVERED IN DETAIL LATER — owned types, nested owned types, and JSON columns
 * may expand in a dedicated topic if your schema needs complex value objects.
 * -------------------------------------------------------------------------
 */
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Street}, {City} {PostalCode}";
    }
}
