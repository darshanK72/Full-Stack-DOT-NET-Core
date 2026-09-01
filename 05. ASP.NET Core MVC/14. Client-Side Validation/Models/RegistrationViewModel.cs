/*
 * FILE ROLE: Teaches how each DataAnnotation attribute maps to HTML data-val-* attributes
 *            that jquery.validate.unobtrusive reads to configure client-side validation rules.
 * SECTIONS IN THIS FILE:
 *   2. DataAnnotations → data-val-* Attributes
 *      2a. [Required]           → data-val-required
 *      2b. [StringLength]       → data-val-length, data-val-length-min/max
 *      2c. [Range]              → data-val-range, data-val-range-min/max
 *      2d. [RegularExpression]  → data-val-regex, data-val-regex-pattern
 *      2e. [Compare]            → data-val-equalto, data-val-equalto-other
 *      2f. [EmailAddress]       → data-val-email
 */

using System.ComponentModel.DataAnnotations;

namespace ClientSideValidation.Models;

/*
 * SECTION 2: DataAnnotations → data-val-* Attributes
 *
 * When the asp-for Tag Helper renders an <input> it calls model metadata to read
 * every ValidationAttribute on the property and emits matching data-val-* attributes.
 * jquery.validate.unobtrusive then reads those attributes at document-ready and
 * registers equivalent jquery.validate rules — no JavaScript you write yourself.
 *
 * Rendered HTML example for UserName below:
 *   <input id="UserName" name="UserName" type="text"
 *          data-val="true"
 *          data-val-required="The UserName field is required."
 *          data-val-length="The field UserName must be between 3 and 50 characters."
 *          data-val-length-min="3"
 *          data-val-length-max="50" />
 *
 * data-val="true" is the master switch — without it the field is skipped entirely.
 *
 * Attribute → HTML attribute mapping table:
 * ┌──────────────────────────────────┬───────────────────────────────────────────────────┐
 * │ DataAnnotation                   │ data-val-* attributes emitted                     │
 * ├──────────────────────────────────┼───────────────────────────────────────────────────┤
 * │ [Required]                       │ data-val-required="message"                       │
 * │ [StringLength(max, Min=min)]     │ data-val-length + data-val-length-min/max         │
 * │ [Range(min, max)]                │ data-val-range + data-val-range-min/max           │
 * │ [RegularExpression(pattern)]     │ data-val-regex + data-val-regex-pattern           │
 * │ [Compare("OtherProp")]           │ data-val-equalto + data-val-equalto-other         │
 * │ [EmailAddress]                   │ data-val-email="message"                          │
 * │ [MinLength(n)] / [MaxLength(n)]  │ data-val-minlength/maxlength + -min/-max         │
 * └──────────────────────────────────┴───────────────────────────────────────────────────┘
 *
 * Custom ErrorMessage:
 *   [Required(ErrorMessage = "Please enter your username.")]
 *   The string you pass becomes the message in the data-val-* attribute value.
 *
 * Nullable vs non-nullable:
 *   With <Nullable>enable</Nullable>, a non-nullable string property automatically
 *   implies [Required] — the MVC binder treats an empty submission as null and
 *   ModelState becomes invalid. Mark optional strings as string? to remove this.
 */
public sealed class RegistrationViewModel
{
    /*
     * --- 2a. [Required] → data-val-required ---
     *
     * Rendered:
     *   data-val="true"
     *   data-val-required="The UserName field is required."
     *
     * jquery.validate rule added: required
     * Triggers: on form submit and on field blur.
     */
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 50 characters.")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty; // non-nullable; empty default avoids CS8618

    /*
     * --- 2b. [StringLength] → data-val-length ---
     *
     * Rendered (combined with [Required] above):
     *   data-val-length="Username must be between 3 and 50 characters."
     *   data-val-length-min="3"
     *   data-val-length-max="50"
     *
     * jquery.validate rules: minlength, maxlength
     * [MaxLength(n)] and [MinLength(n)] emit data-val-maxlength/minlength instead;
     * prefer [StringLength] when you need both ends in one attribute.
     */
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    /*
     * --- 2f. [EmailAddress] → data-val-email ---
     *
     * Rendered:
     *   data-val-email="Enter a valid email address."
     *
     * jquery.validate rule: email
     * Checks format only client-side; real deliverability requires server-side checks.
     *
     * --- Password: [StringLength] with MinimumLength ---
     *
     * Rendered:
     *   data-val-length="Password must be 8–100 characters."
     *   data-val-length-min="8"
     *   data-val-length-max="100"
     */
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must be 8–100 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /*
     * --- 2e. [Compare] → data-val-equalto ---
     *
     * Rendered:
     *   data-val-equalto="Passwords do not match."
     *   data-val-equalto-other="*.Password"
     *
     * data-val-equalto-other uses the MVC naming convention "*.PropertyName"
     * where * is the model prefix. jquery.validate.unobtrusive resolves this
     * to the actual field id at runtime.
     *
     * jquery.validate rule: equalTo
     * Pitfall: [Compare] works in one direction only — the confirmation field
     * compares itself to the original. Changing the original after typing in the
     * confirm field does NOT re-trigger validation on the original field.
     */
    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /*
     * --- 2c. [Range] → data-val-range ---
     *
     * Rendered:
     *   data-val-range="Age must be between 18 and 120."
     *   data-val-range-min="18"
     *   data-val-range-max="120"
     *
     * jquery.validate rule: range
     * Works for int, double, decimal, and DateTime (pass typeof(DateTime) overload).
     * Pitfall: [Range] on a DateTime requires the string overload
     *   [Range(typeof(DateTime), "2000-01-01", "2099-12-31")] and culture-awareness.
     */
    [Required(ErrorMessage = "Age is required.")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
    [Display(Name = "Age")]
    public int? Age { get; set; } // int? so unsubmitted field yields null not 0

    /*
     * --- 2d. [RegularExpression] → data-val-regex ---
     *
     * Rendered:
     *   data-val-regex="Phone must be exactly 10 digits."
     *   data-val-regex-pattern="^\d{10}$"
     *
     * jquery.validate rule: regex (added by jquery.validate.unobtrusive)
     * Note: the .NET regex engine and the JavaScript RegExp engine differ slightly.
     * Always test the same pattern in both environments. Named groups (?<name>...)
     * are not supported in all browsers — use non-named groups or character classes.
     */
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; } // nullable — phone is optional on this form
}
