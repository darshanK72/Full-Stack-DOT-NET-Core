/*
 * FILE ROLE: ProductFormModel is the typed form-submission model for the PRG pattern
 *            demo. It also documents TempData's JSON-serialisation requirement for
 *            complex types and explains why [TempData] on ViewModel properties is a no-op.
 * SECTIONS IN THIS FILE:
 *   SECTION 4a. ProductFormModel — form fields with Data Annotations
 *   SECTION 4b. TempData serialisation requirement — storing complex types via JSON
 */

using System.ComponentModel.DataAnnotations;

namespace TempDataViewDataViewBag.Models;

/*
 * SECTION 4a: PRODUCTFORMMODEL — FORM-SUBMISSION MODEL
 * ─────────────────────────────────────────────────────────────────────────────
 * ProductFormModel is a plain C# class (POCO) that carries the fields a user
 * enters in the Create form. MVC's model binder populates it from the HTTP POST
 * body automatically — each form field name must match a property name exactly
 * (case-insensitive).
 *
 * Data Annotations on the properties drive server-side validation via ModelState:
 *   [Required]      → ModelState.IsValid = false if null/empty string
 *   [Range]         → ModelState.IsValid = false if value outside bounds
 *   [StringLength]  → ModelState.IsValid = false if string exceeds max length
 *
 * The Razor view accesses these annotations automatically through tag helpers:
 *   asp-for="Name"              → renders <input> with correct name, id, type
 *   asp-validation-for="Name"   → renders <span> that shows ModelState errors
 *
 * COVERED IN DETAIL → 07. Data Annotations & Validation
 */
public sealed class ProductFormModel
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters.")]
    public string Name { get; set; } = string.Empty;   // initialised to avoid nullable warning

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 10_000.00, ErrorMessage = "Price must be between 0.01 and 10,000.")]
    public decimal Price { get; set; }

    [Range(0, 10_000, ErrorMessage = "Quantity must be between 0 and 10,000.")]
    public int Quantity { get; set; }
}

/*
 * SECTION 4b: TEMPDATA SERIALISATION REQUIREMENT
 * ─────────────────────────────────────────────────────────────────────────────
 * Both TempData providers (Cookie and Session) serialise TempData values to JSON
 * before storing and deserialise them on the next request. This imposes a constraint:
 *
 *   Type category                    TempData support
 *   ─────────────────────────────── ────────────────────────────────────────────
 *   string, int, long, bool,        Supported natively — primitive JSON types
 *   double, float, decimal, Guid,
 *   DateTime, DateTimeOffset
 *   ─────────────────────────────── ────────────────────────────────────────────
 *   Class instances, arrays,        NOT supported directly — must be serialised
 *   collections, records            manually before storing and deserialised after
 *
 * Attempting to store a class instance directly:
 *   TempData["Product"] = new ProductFormModel();  // → InvalidOperationException at runtime
 *
 * Correct pattern — manual JSON serialisation:
 *
 *   // In the POST action (before redirect):
 *   var json = System.Text.Json.JsonSerializer.Serialize(model);
 *   TempData["PendingProduct"] = json;             // stored as string in cookie/session
 *
 *   // In the next GET action (after redirect):
 *   if (TempData["PendingProduct"] is string json2)
 *   {
 *       var restored = System.Text.Json.JsonSerializer
 *                          .Deserialize<ProductFormModel>(json2);
 *   }
 *
 * [TEMPDATA] ATTRIBUTE ON VIEWMODEL PROPERTIES:
 *   The [TempData] attribute (Microsoft.AspNetCore.Mvc.TempDataAttribute) is
 *   designed exclusively for CONTROLLER properties. ASP.NET Core's activator
 *   (TempDataDictionaryControllerPropertyActivator) only processes properties on
 *   types that derive from ControllerBase.
 *
 *   Applying [TempData] to a property on ProductFormModel (or any non-controller
 *   class) has NO effect — no framework code reads or writes TempData for it.
 *   Use the controller property approach shown in Controllers/ProductsController.cs
 *   (SECTION 9) instead.
 *
 * NOTE: This class itself is NOT stored in TempData. Only primitive string/int values
 *       are stored via TempData in this project. Complex-type storage is shown above
 *       as a commented code pattern for reference.
 */
