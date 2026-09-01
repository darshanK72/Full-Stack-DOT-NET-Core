/*
 * FILE ROLE: Form ViewModel for the Products create/edit page. Demonstrates
 *            [Display], [DisplayFormat], [Required], [Range] on form fields,
 *            nullable types, and a lightweight CategoryOption DTO that keeps
 *            ViewModels independent of ASP.NET MVC framework types.
 * SECTIONS IN THIS FILE:
 *  13. Form ViewModels — purpose and separation from display VMs
 *  14. CategoryOption — lightweight DTO for dropdown independence
 *  15. ProductFormViewModel — full form VM with validation attributes
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.ViewModels;

/*
 * SECTION 13: FORM VIEWMODELS — SEPARATE FROM DISPLAY VIEWMODELS
 * ─────────────────────────────────────────────────────────────────────────────
 * A FORM VIEWMODEL serves two purposes in one request cycle:
 *
 *   GET (render the form):
 *     - Controller fills the VM with current values (for edit) or defaults (for create).
 *     - Controller fills CategoryOptions with available categories for the dropdown.
 *     - View renders <input>, <select>, and <textarea> elements bound to VM properties.
 *
 *   POST (process the submitted form):
 *     - Model binder maps HTTP form fields → ViewModel properties.
 *     - ModelState.IsValid checks [Required], [Range] etc.
 *     - If valid: controller maps VM → domain entity and saves.
 *     - If invalid: controller re-fills CategoryOptions and returns View(vm) to show errors.
 *
 * WHY SEPARATE FROM DISPLAY VIEWMODELS?
 *   ProductDetailViewModel has read-only, formatted values with no validation attributes.
 *   ProductFormViewModel has writable, unformatted values with [Required]/[Range] for
 *   model binding and validation. Merging them into one class causes:
 *     - [DisplayFormat] interfering with input field values (e.g. "$1,299.99" can't parse)
 *     - Validation attributes on display-only properties causing false failures
 *     - The form receiving fields the user should not be able to change (e.g. CreatedAt)
 *
 * COMBINED CREATE + EDIT VM (this class):
 *   Id == null  → Create action  (POST to /Products/Form)
 *   Id != null  → Edit action    (POST to /Products/Form/3)
 *   Many teams prefer two separate classes (CreateProductViewModel, EditProductViewModel)
 *   to avoid the nullable Id entirely. Both approaches are valid.
 *
 * OVER-POSTING PROTECTION:
 *   Because ProductFormViewModel only exposes the fields the form edits, a POST
 *   with extra fields (e.g. InternalCost) is silently ignored by the model binder.
 *   This is the primary security reason for using form ViewModels over domain entities.
 */

/*
 * SECTION 14: CategoryOption — LIGHTWEIGHT DROPDOWN DTO
 * ─────────────────────────────────────────────────────────────────────────────
 * Form ViewModels often need a list of options for <select> dropdowns.
 * There are two common approaches:
 *
 *   Approach A: Use Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
 *     Pro:  Works directly with Html.DropDownListFor and <select asp-items>.
 *     Con:  Couples the ViewModel to an ASP.NET MVC framework type. Makes
 *           ViewModels harder to test and reuse outside an MVC context.
 *
 *   Approach B: Use a plain DTO (this approach — CategoryOption)
 *     Pro:  ViewModel stays a plain C# class with no framework dependencies.
 *           Unit tests can create CategoryOption without mocking MVC infrastructure.
 *     Con:  The view must convert: new SelectList(Model.CategoryOptions, "Id", "Name")
 *
 * RECOMMENDATION:
 *   Use Approach B (plain DTO) in ViewModels, then convert in the view or
 *   in a view helper. This keeps the ViewModel layer portable and testable.
 *
 * The string property names "Id" and "Name" in new SelectList(..., "Id", "Name")
 * are string-based reflection — if you rename the properties here, update the
 * view's SelectList call to match. Consider using nameof() in a helper to
 * keep them in sync:
 *   new SelectList(Model.CategoryOptions, nameof(CategoryOption.Id), nameof(CategoryOption.Name))
 */
public class CategoryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/*
 * SECTION 15: ProductFormViewModel — FORM VM WITH FULL METADATA
 * ─────────────────────────────────────────────────────────────────────────────
 * NULLABLE TYPES IN FORM VIEWMODELS:
 *
 *   int? Id — null for create; set for edit. Nullable value type.
 *
 *   string Name — non-nullable + [Required] = the field is mandatory.
 *     With <Nullable>enable</Nullable>, a non-nullable string property that
 *     has no [Required] attribute still fails model binding when the field is
 *     empty (the binder sees "" and treats it as null for a non-nullable type).
 *     Being explicit with [Required] is good practice for clarity.
 *
 *   string? Description — nullable + no [Required] = optional field.
 *     The model binder maps an empty form field to null, which is correct.
 *
 *   decimal Price — non-nullable value type + [Required] + [Range]:
 *     [Required] on a value type is redundant (value types cannot be null),
 *     but it produces a useful validation error message when the field is blank.
 *
 * [Display(Name = "...")] — Html.LabelFor reads this for the <label> text.
 *   Without [Display], LabelFor uses the property name as-is ("StockQuantity").
 *   With [Display(Name = "Stock Quantity")], the label reads naturally.
 *
 * [DisplayFormat(DataFormatString = "{0:F2}")] — formats the Price for display.
 *   ApplyFormatInEditMode = false (the default) means EditorFor renders
 *   the raw decimal (e.g. "79.99") not the formatted string ("$79.99").
 *   This is important: a user cannot type "$79.99" into a numeric input and
 *   have it parse correctly as 79.99m.
 *
 * [Range(min, max)] — enforced by ModelState.IsValid; displays an error if
 *   the submitted value falls outside the range.
 *
 * [StringLength(max)] — maximum character count; enforced by ModelState.IsValid.
 *
 * CategoryOptions — NOT bound by the POST model binder (collection of complex
 *   objects is not submitted as form data). The controller must re-populate this
 *   on a failed POST before returning View(vm) so the dropdown re-renders.
 *   Forgetting to re-populate is a common bug — the view throws a null reference
 *   when it tries to iterate an empty CategoryOptions on redisplay.
 */
public class ProductFormViewModel
{
    // null on Create; populated on Edit — determines which route the form POSTs to
    public int? Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }        // optional; nullable

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99.")]
    [Display(Name = "Price")]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = false)]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;      // default true for new products

    [Required(ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    // Dropdown data — filled by GET action, NOT submitted with the form POST
    // IMPORTANT: re-populate this before returning View(vm) on a failed POST
    public IEnumerable<CategoryOption> CategoryOptions { get; set; }
        = new List<CategoryOption>();
}
