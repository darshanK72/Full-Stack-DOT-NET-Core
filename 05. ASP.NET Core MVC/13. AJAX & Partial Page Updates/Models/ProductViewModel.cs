using System.ComponentModel.DataAnnotations;

namespace AjaxPartialUpdates.Models;

/*
 * FILE ROLE: ProductViewModel is the model class threaded through every
 *            AJAX scenario in this chapter — partial views, JSON responses,
 *            FormData POSTs, and JSON POSTs. It also carries data annotations
 *            so [ValidateAntiForgeryToken] + ModelState.IsValid is meaningful.
 *
 * SECTIONS IN THIS FILE:
 *   3. ProductViewModel — model for partial views and AJAX responses
 */

/*
 * SECTION 3: ProductViewModel — MODEL FOR PARTIAL VIEWS AND AJAX RESPONSES
 * ─────────────────────────────────────────────────────────────────────────────
 * This ViewModel is used in three distinct ways throughout the chapter:
 *
 *   1. As the model for partial views (_ProductCard.cshtml, _ProductList.cshtml)
 *      The partial view receives it via: return PartialView("_ProductCard", vm);
 *
 *   2. As the deserialized body of JSON POST requests
 *      The controller receives it via: [HttpPost] AddProductJson([FromBody] ProductViewModel vm)
 *
 *   3. As the source of JSON data returned to the client
 *      return Json(vm); — serialized with System.Text.Json by default in .NET 8
 *
 * DATA ANNOTATIONS AND MODEL VALIDATION:
 *   Data annotations on the ViewModel drive server-side validation via ModelState.
 *   The controller checks ModelState.IsValid before persisting any data.
 *   Annotations also drive client-side validation (when unobtrusive validation
 *   is wired up), but in this chapter validation is handled server-side.
 *
 *   [Required]    — property must be non-null and non-empty
 *   [StringLength] — max/min character length constraint
 *   [Range]       — numeric bounds (inclusive)
 *
 * JSON SERIALIZATION (System.Text.Json, .NET 8 default):
 *   By default, property names are camelCase in JSON output:
 *     C# Id    → JSON "id"
 *     C# Name  → JSON "name"
 *     C# InStock → JSON "inStock"
 *   This matters in Index.cshtml where JS reads data.id, data.name, data.inStock.
 *
 * COMPUTED PROPERTY:
 *   InStock has no setter — it is computed from Stock. It is not bound from
 *   the request body (it is set by the server based on Stock). This is fine
 *   for [FromBody] binding because System.Text.Json simply ignores JSON fields
 *   that do not have a setter on the target type.
 *
 * COVERED IN DETAIL LATER:
 *   Data annotations and server validation → 07. Data Annotations & Validation
 */
public class ProductViewModel
{
    public int Id { get; set; } // set by ProductService.Add(); not sent from client

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 9999.99, ErrorMessage = "Price must be between $0.01 and $9999.99.")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 10000)]
    public int Stock { get; set; }

    // Computed — not bound from request; derived from Stock by the server
    public bool InStock => Stock > 0;
}
