/*
 * FILE ROLE: Defines the Product DTO used as a request/response body by
 *            ProductsController and referenced in the Model Binding preview (Section 15).
 * SECTIONS IN THIS FILE:
 *   1. Product DTO
 */

namespace RoutingEndpoints.Models;

/*
 * SECTION 1: PRODUCT DTO
 *
 * A Data Transfer Object (DTO) is a plain class used to carry data between the
 * HTTP layer and the rest of the application.  For an endpoint routing chapter
 * the DTO is intentionally thin: it holds only the fields needed by route handlers.
 *
 * In a real project this type often lives in a shared class library so controllers,
 * minimal-API endpoints, and background services all reference the same type.
 *
 * { get; init; } properties:
 *   init-only setters let the object be constructed via an object initializer but
 *   prevent mutation afterward — a safe default for API response objects.
 *
 * Default values (= string.Empty):
 *   Required to satisfy the nullable-enabled compiler: string properties that are
 *   not assigned in every constructor path must carry a non-null default.
 *
 * PREVIEW (covered in depth in 08. Model Binding & Validation):
 *   Add [Required], [Range], [StringLength] from System.ComponentModel.DataAnnotations
 *   to this class to drive automatic 400 validation with [ApiController].
 *   Example:
 *     [Required]          public string Name  { get; init; } = string.Empty;
 *     [Range(0, 100_000)] public decimal Price { get; init; }
 */
public sealed class Product
{
    public int Id { get; init; }                          // matched by /products/{id:int}
    public string Name { get; init; } = string.Empty;    // default avoids CS8618 nullable warning
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty; // used by /category/{category:alpha}
}
