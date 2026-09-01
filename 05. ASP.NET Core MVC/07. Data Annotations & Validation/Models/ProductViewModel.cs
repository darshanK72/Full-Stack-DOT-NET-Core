using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DataAnnotationsValidation.Models;

/*
 * FILE ROLE: Product ViewModel — demonstrates [Range], [DataType], [Display],
 *            [DisplayFormat], [ScaffoldColumn], [CreditCard], and [ValidateNever].
 * SECTIONS IN THIS FILE:
 *   2a. [Range]             — numeric and date range validation
 *   2b. [DataType]          — semantic type for rendering; full enum reference
 *   2c. [Display]           — override field label name and order
 *   2d. [DisplayFormat]     — control output format and null display text
 *   2e. [ScaffoldColumn]    — hide a property from auto-generated scaffolding
 *   2f. [CreditCard]        — Luhn-algorithm credit card number validation
 *   2g. [ValidateNever]     — skip ALL validation for a property during model binding
 */

/*
 * SECTION 2: PRODUCTVIEWMODEL
 *
 * While RegistrationViewModel focuses on input constraints ([Required], [StringLength],
 * format validators), ProductViewModel demonstrates the attributes that control
 * numeric ranges, display formatting, scaffolding, and validation skipping.
 */
public sealed class ProductViewModel
{
    // --- 2a. [Range] — numeric and date range validation ----------------------
    /*
     * [Range(min, max)] validates that the value falls within [min, max] (inclusive).
     * Works for int, double, decimal, DateTime (via string overload), and any type
     * that implements IComparable.
     *
     * OVERLOADS:
     *   [Range(double min, double max)]          — numeric range
     *   [Range(int min, int max)]                — integer range
     *   [Range(typeof(DateTime), "2000-01-01", "2099-12-31")] — date range via strings
     *
     * ErrorMessage placeholders: {0}=display name  {1}=min  {2}=max
     *
     * PITFALL: [Range] on a non-nullable int will always validate (default is 0).
     *   Use int? (nullable) and add [Required] when the field must be provided.
     *
     * PITFALL: [Range] does NOT fire when the value is null.  Pair with [Required]
     *   when null should be rejected.
     */
    [Required(ErrorMessage = "{0} is required.")]
    [Range(0.01, 99999.99, ErrorMessage = "{0} must be between {1:C} and {2:C}.")]
    [Display(Name = "Price")]
    public decimal? Price { get; set; }

    [Range(0, 10000, ErrorMessage = "{0} must be between {1} and {2}.")]
    [Display(Name = "Stock Quantity")]
    public int? StockQuantity { get; set; }               // optional — null means "not set"

    // --- 2b. [DataType] — semantic type for rendering ------------------------
    /*
     * [DataType(DataType.xxx)] does NOT add a validation rule by itself.
     * It is a hint to Razor Tag Helpers and HTML helpers about which HTML input
     * type and display format to use.
     *
     * FULL DataType ENUM REFERENCE:
     * ┌────────────────────────┬──────────────────────────────────────────────┐
     * │ DataType               │ Effect                                       │
     * ├────────────────────────┼──────────────────────────────────────────────┤
     * │ DataType.Password      │ <input type="password"> — chars masked       │
     * │ DataType.Date          │ <input type="date">     — date picker        │
     * │ DataType.Time          │ <input type="time">     — time picker        │
     * │ DataType.DateTime      │ <input type="datetime-local">                │
     * │ DataType.EmailAddress  │ <input type="email">    — mobile keyboard    │
     * │ DataType.PhoneNumber   │ <input type="tel">      — phone keyboard     │
     * │ DataType.Url           │ <input type="url">                           │
     * │ DataType.Currency      │ No input effect; used by [DisplayFormat]     │
     * │ DataType.MultilineText │ <textarea>                                   │
     * │ DataType.CreditCard    │ <input type="tel">      — pairs with [CreditCard] │
     * │ DataType.PostalCode    │ <input type="text"> with postal semantics    │
     * └────────────────────────┴──────────────────────────────────────────────┘
     *
     * Tag Helper: <input asp-for="LaunchDate"> reads [DataType] automatically.
     */
    [DataType(DataType.Date)]                               // renders <input type="date">
    [Display(Name = "Launch Date")]
    public DateTime? LaunchDate { get; set; }

    // --- 2c. [Display] — override field label and order ----------------------
    /*
     * [Display(Name = "...")] overrides the property name used in:
     *   - <label asp-for="..."> output
     *   - Error message {0} placeholder
     *   - Html.LabelFor / Html.DisplayNameFor
     *
     * Additional [Display] parameters:
     *   Order     — controls field ordering in scaffolded UI (default: 10000)
     *   GroupName — groups related fields in scaffolded forms
     *   Prompt    — placeholder text in scaffolded inputs
     *   Description — tooltip text in scaffolded UI
     *   ShortName — abbreviated label for compact UI
     *
     * PITFALL: [Display] is cosmetic for views.  It does NOT affect database
     *   column names (use [Column] from System.ComponentModel.DataAnnotations.Schema).
     */
    [Required(ErrorMessage = "{0} is required.")]
    [Display(Name = "Product Name", Order = 1, Prompt = "Enter product name")]
    [StringLength(150, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? ProductName { get; set; }

    // --- 2d. [DisplayFormat] — output format and null display text -----------
    /*
     * [DisplayFormat] controls how a value is RENDERED (not validated):
     *
     *   DataFormatString     — format string passed to String.Format({0:xxx}, value)
     *   ApplyFormatInEditMode — apply format string to <input> values as well
     *                           (default: false — edit mode shows raw value)
     *   NullDisplayText      — text shown when value is null in display mode
     *   HtmlEncode           — whether to HTML-encode the output (default: true)
     *
     * PITFALL: ApplyFormatInEditMode = true can break HTML date inputs.
     *   For dates, use "yyyy-MM-dd" (ISO format) — browsers expect this exact format
     *   in <input type="date">.  Other formats cause the date picker to show empty.
     *
     * COMMON PATTERNS:
     *   [DisplayFormat(DataFormatString = "{0:C}")]           — currency: $1,234.56
     *   [DisplayFormat(DataFormatString = "{0:P1}")]          — percent: 12.3%
     *   [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
     *   [DisplayFormat(NullDisplayText = "N/A")]              — null placeholder
     */
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)] // currency display
    [Display(Name = "Formatted Price")]                       // computed display — no setter
    public string FormattedPrice => Price.HasValue
        ? Price.Value.ToString("C")
        : "Not set";

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Release Date")]
    public DateTime? ReleaseDate { get; set; }

    [Display(Name = "Description")]
    [DisplayFormat(NullDisplayText = "No description provided.")]
    public string? Description { get; set; }

    // --- 2e. [ScaffoldColumn(false)] — hide from scaffolding -----------------
    /*
     * [ScaffoldColumn(false)] tells scaffolding tools (Visual Studio Add Scaffold,
     * EF Core migrations) to omit this property from auto-generated forms and views.
     *
     * USE CASES:
     *   - Internal tracking fields (audit timestamps, concurrency tokens)
     *   - Computed/derived values that should never appear in an edit form
     *   - Properties that are set server-side, not by the user
     *
     * IMPORTANT: [ScaffoldColumn(false)] does NOT prevent the property from being
     * model-bound or validated on a POST.  Use [ValidateNever] (Section 2g) or a
     * [Bind] attribute on the controller action to exclude it from binding entirely.
     */
    [ScaffoldColumn(false)]                                   // hidden in scaffolded forms
    public string InternalCode { get; set; } = Guid.NewGuid().ToString("N")[..8];

    // --- 2f. [CreditCard] — Luhn-algorithm validation -------------------------
    /*
     * [CreditCard] validates credit card number format using the Luhn algorithm.
     * The Luhn check verifies that the digit checksum is correct — it catches typos
     * but does NOT verify that the card exists or has funds.
     *
     * PAIRING: Combine [CreditCard] with [DataType(DataType.CreditCard)] so the
     * browser uses <input type="tel"> (numeric mobile keyboard) and the server
     * validates the format.
     *
     * [CreditCard] silently passes null — add [Required] for mandatory fields.
     *
     * CLIENT-SIDE: [CreditCard] emits data-val-creditcard="..." attributes that
     * jquery.validate.unobtrusive reads for client-side Luhn checks.
     * COVERED IN DETAIL LATER → 14. Client-Side Validation
     */
    [CreditCard(ErrorMessage = "{0} is not a valid credit card number.")]
    [DataType(DataType.CreditCard)]                           // <input type="tel">
    [Display(Name = "Card Number")]
    public string? CardNumber { get; set; }

    // --- 2g. [ValidateNever] — skip ALL validation for a property -------------
    /*
     * [ValidateNever] (Microsoft.AspNetCore.Mvc.ModelBinding.Validation) instructs
     * the MVC model binding pipeline to skip ALL DataAnnotations validation for
     * the decorated property — as if no attributes existed on it.
     *
     * WHEN TO USE:
     *   - Read-only/lookup data loaded server-side and re-attached to the model
     *     for the view (e.g., a dropdown list of categories from the database).
     *   - Computed properties populated by the controller, not the user.
     *   - Properties intentionally excluded from the submitted form.
     *
     * IMPORTANT DIFFERENCES:
     *   [ValidateNever]   — skips validation only (property IS still model-bound)
     *   [BindNever]       — skips model binding entirely (property is never populated)
     *   [Bind("Prop1,Prop2")] on controller action — allowlist of bindable properties
     *
     * NOTE: [ValidateNever] is in Microsoft.AspNetCore.Mvc.ModelBinding.Validation —
     * NOT in System.ComponentModel.DataAnnotations.  It requires a reference to the
     * Microsoft.AspNetCore.Mvc package (included automatically in web SDK projects).
     */
    [ValidateNever]                                           // never validated on POST
    public List<string> Categories { get; set; } = new List<string>(); // populated server-side
}
