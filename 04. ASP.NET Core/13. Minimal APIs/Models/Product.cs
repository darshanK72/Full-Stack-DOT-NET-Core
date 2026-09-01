/*
 * FILE ROLE: Defines the Product entity and request DTOs used by ProductEndpoints
 *            to demonstrate [FromBody] binding, TypedResults, and DataAnnotations
 *            validation (consumed by ValidationEndpointFilter).
 * SECTIONS IN THIS FILE:
 *   4a. Product entity          — the response type returned by GET endpoints
 *   4b. CreateProductRequest    — DTO for POST /products (body-bound, validated)
 *   4c. UpdateProductRequest    — DTO for PUT /products/{id} (body-bound, validated)
 */

using System.ComponentModel.DataAnnotations;

namespace MinimalApis.Models;

/*
 * SECTION 4a: PRODUCT ENTITY
 *
 * This is the response model returned by all GET and write endpoints.
 * It is NOT used as the request body for create/update — those use dedicated
 * request DTOs (below) so that callers cannot set the auto-assigned Id.
 *
 * Keeping entity and request DTO separate is a common Minimal APIs pattern:
 *   Request DTO  — only fields the caller supplies (no Id, no computed fields)
 *   Entity       — full record including server-assigned and computed fields
 *
 * Nullable: all reference-type properties get default initializers to satisfy
 * the CS8618 "non-nullable property must be assigned" warning.
 */
public sealed class Product
{
    public int Id { get; set; }                            // server-assigned primary key
    public string Name { get; set; } = string.Empty;      // initialized to avoid CS8618
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}

/*
 * SECTION 4b: CREATEPRODUCTREQUEST — POST /products BODY DTO
 *
 * DataAnnotations on request DTOs power the ValidationEndpointFilter:
 *   [Required]       — field must be present and non-empty
 *   [StringLength]   — enforces min/max character count
 *   [Range]          — enforces numeric bounds
 *
 * The filter calls Validator.TryValidateObject() and returns
 * Results.ValidationProblem() (400 + RFC 7807 body) if any rule fails.
 * See Filters/ValidationEndpointFilter.cs for the filter implementation.
 *
 * Pitfall: DataAnnotations validate the surface of the object; for nested
 * objects or custom cross-field rules, implement IValidatableObject or use
 * FluentValidation (a third-party library).
 */
public sealed class CreateProductRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}

/*
 * SECTION 4c: UPDATEPRODUCTREQUEST — PUT /products/{id} BODY DTO
 *
 * Identical shape to CreateProductRequest — in a real app you might share a
 * base class, but separate types give you freedom to add update-specific fields
 * (e.g., a ConcurrencyToken) without breaking the create DTO.
 */
public sealed class UpdateProductRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
