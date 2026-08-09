using System.ComponentModel.DataAnnotations;

namespace FluentApiAndDataAnnotations.Models;

/*
 * FILE ROLE: Entity configured primarily with data annotations on properties.
 *
 * SECTIONS IN THIS FILE:
 *   1. [Required] — non-null columns and validation
 *   2. [MaxLength] and [StringLength] — string column size
 */

/*
 * =========================================================================
 * SECTION 1: [Required] — NON-NULL COLUMNS AND VALIDATION
 * =========================================================================
 *
 * [Required] from System.ComponentModel.DataAnnotations marks a property as
 * mandatory. EF Core uses it when building the model:
 *
 *   Effect on schema     | Non-nullable column (NOT NULL in SQL Server)
 *   Effect on validation | SaveChanges validates before INSERT/UPDATE
 *
 * For reference types (string), [Required] makes the column required even when
 * nullable reference types (NRT) are enabled. Without [Required], a non-nullable
 * string property is also treated as required by convention — [Required] adds
 * explicit validation messages and documents intent.
 *
 * Validation runs client-side only if you use ASP.NET model binding; in a console
 * app EF validates at SaveChanges via internal validators.
 *
 * Pitfall: [Required] on string allows empty string by default (AllowEmptyStrings = true).
 * Use [Required(AllowEmptyStrings = false)] when "" must fail validation.
 * Pitfall: [Required] on int/value types has little effect — value types are
 * never null unless int?. Use [Required] on strings and navigation properties.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: [MaxLength] AND [StringLength] — STRING COLUMN SIZE
 * =========================================================================
 *
 * Both limit string length; they map to the same EF Core facet (max length):
 *
 *   Attribute       | Typical use
 *   ----------------|--------------------------------------------------
 *   [MaxLength(n)]  | Any type with length (string, byte[], collections preview)
 *   [StringLength]  | Strings only; optional MinimumLength for validation
 *
 * Schema effect: NVARCHAR(n) instead of NVARCHAR(MAX) — better for indexes and
 * storage predictability.
 *
 * Prefer [MaxLength] when you only care about the database column cap.
 * Use [StringLength(max, MinimumLength = min)] when you also want validation
 * range messages (e.g. SKU must be 3–20 characters).
 *
 * Fluent API equivalent (shown on Supplier in CatalogDbContext.cs):
 *   entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
 * -------------------------------------------------------------------------
 */
public class CatalogProduct
{
    public int CatalogProductId { get; set; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // NOT NULL, NVARCHAR(100); "" fails validation

    [StringLength(500)]
    public string? Description { get; set; } // nullable optional text, NVARCHAR(500)

    [StringLength(20, MinimumLength = 3)]
    public string Sku { get; set; } = string.Empty; // validation: 3–20 chars at SaveChanges

    public decimal UnitPrice { get; set; } // decimal(18,2) by SQL Server convention

    public override string ToString()
    {
        return $"{CatalogProductId,2} | {Sku,-8} | {Name,-20} | ${UnitPrice,7:F2}";
    }
}
