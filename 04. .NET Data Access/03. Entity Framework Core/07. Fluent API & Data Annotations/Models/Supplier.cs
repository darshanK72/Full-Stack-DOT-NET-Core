namespace FluentApiAndDataAnnotations.Models;

/*
 * FILE ROLE: Plain POCO configured entirely through Fluent API in OnModelCreating.
 *
 * SECTIONS IN THIS FILE:
 *   3. POCO target for Fluent API (no data annotations)
 */

/*
 * =========================================================================
 * SECTION 3: POCO TARGET FOR FLUENT API
 * =========================================================================
 *
 * Not every rule belongs on the entity class. Teams often keep domain types
 * free of persistence attributes and configure mapping in DbContext:
 *
 *   Data annotations on entity  | Attributes on properties ([Required], etc.)
 *   Fluent API in DbContext   | modelBuilder.Entity<T>(...) in OnModelCreating
 *
 * When to prefer Fluent API:
 *   - Mapping that does not fit a single property (indexes, owned types, keys)
 *   - Keep entities clean for reuse across layers or generated code (ch04 scaffold)
 *   - Configuration shared across DbContext variants (read-only vs read-write)
 *
 * When annotations are fine:
 *   - Simple required/max-length on one property — visible at a glance
 *   - Validation attributes reused by ASP.NET Core model binding (Web API modules)
 *
 * Supplier has *no* data annotations — all rules live in CatalogDbContext.cs.
 * Compare with CatalogProduct.cs (annotation-driven) in the same chapter.
 * -------------------------------------------------------------------------
 */
public class Supplier
{
    public int SupplierId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty; // unique index via Fluent API
    public Address Address { get; set; } = null!;            // owned type — configured in DbContext

    public override string ToString()
    {
        return $"{SupplierId,2} | {SupplierCode,-6} | {CompanyName,-18} | {Address}";
    }
}
