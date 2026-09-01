using System.ComponentModel.DataAnnotations;

namespace DataAnnotationsValidation.Models;

/*
 * FILE ROLE: Registration form ViewModel — full showcase of the most-used DataAnnotations.
 * SECTIONS IN THIS FILE:
 *   1a. [Required]          — field must be non-null / non-empty
 *   1b. [StringLength], [MaxLength], [MinLength] — length constraints
 *   1c. [EmailAddress], [Phone], [Url], [RegularExpression] — format validators
 *   1d. [DataType]          — semantic rendering hint (password masking, etc.)
 *   1e. [Compare]           — cross-field equality for password confirmation
 *   1f. Error message customization — {0} placeholder, {1}/{2} range, resource files
 */

/*
 * SECTION 1: REGISTRATIONVIEWMODEL — DataAnnotations showcase
 *
 * All built-in attributes live in System.ComponentModel.DataAnnotations (no extra
 * packages).  Validation runs automatically: after model binding on a POST, MVC
 * calls Validator.TryValidateObject, iterates every attribute on every property,
 * and stores failures in ModelState.
 *
 * ATTRIBUTE QUICK REFERENCE:
 * ┌──────────────────────┬────────────────────────────────────────────────────┐
 * │ Attribute            │ What it validates                                  │
 * ├──────────────────────┼────────────────────────────────────────────────────┤
 * │ [Required]           │ Non-null; non-empty string; non-default value type │
 * │ [StringLength(max)]  │ Length ≤ max; optional MinimumLength lower bound   │
 * │ [MaxLength(n)]       │ Length ≤ n (also sets EF Core column width)        │
 * │ [MinLength(n)]       │ Length ≥ n (or collection count ≥ n)               │
 * │ [EmailAddress]       │ x@y format via built-in regex                      │
 * │ [Phone]              │ Digits, spaces, +, -, (, ) patterns                │
 * │ [Url]                │ http:// or https:// prefix required                │
 * │ [RegularExpression]  │ Full custom regex pattern                          │
 * │ [DataType]           │ Semantic hint only — rendering and mild checks     │
 * │ [Compare]            │ Value must equal another named property            │
 * └──────────────────────┴────────────────────────────────────────────────────┘
 *
 * NULL BEHAVIOUR: format attributes ([EmailAddress], [Phone], [Url],
 * [RegularExpression]) silently pass when the value is null or empty — they only
 * validate a provided value.  Always pair with [Required] when the field is
 * mandatory, or the field will accept a missing value without complaint.
 *
 * NULLABLE PATTERN: declaring properties as string? (nullable) is the recommended
 * MVC model-binding pattern.  The property CAN be null before the form is submitted;
 * [Required] catches null/empty at validation time.
 */
public sealed class RegistrationViewModel
{
    // --- 1a. [Required] -------------------------------------------------------
    /*
     * [Required] — the most fundamental annotation.  Fails when the value is:
     *   - null
     *   - an empty string ""
     *   - a whitespace-only string (AllowEmptyStrings defaults to false)
     *
     * The ErrorMessage {0} placeholder is replaced at runtime with the field's
     * display name — resolved from [Display(Name="...")] if present, or the
     * property name otherwise.
     *
     * For value types (int, DateTime), [Required] only makes sense on nullable
     * types (int?, DateTime?) — non-nullable value types always have a value.
     */
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(20, MinimumLength = 3,
        ErrorMessage = "{0} must be between {2} and {1} characters.")] // {2}=min {1}=max
    [Display(Name = "Username")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    // --- 1b. [StringLength], [MaxLength], [MinLength] -------------------------
    /*
     * [StringLength(maxLength)] validates total character count AND communicates
     *   column size to EF Core.  Its MinimumLength named parameter adds a lower
     *   bound in the same attribute.
     *   ErrorMessage placeholders: {0}=display name  {1}=max  {2}=min
     *
     * [MaxLength(n)] — upper-bound only.  Lighter weight; also read by EF Core
     *   for column size.  ErrorMessage placeholder: {0}=display name  {1}=n
     *
     * [MinLength(n)] — lower-bound only.  Useful for collections (List<T>) as well
     *   as strings.  ErrorMessage placeholder: {0}=display name  {1}=n
     *
     * DIFFERENCE: [StringLength] vs [MaxLength]:
     *   - [StringLength] shows both bounds in one message ({1}/{2}).
     *   - [MaxLength] / [MinLength] each handle one bound independently.
     *   - Both set EF Core column sizes when applied to entity properties.
     *   - Prefer [StringLength] when you want a single readable message for both
     *     bounds; use [MaxLength]/[MinLength] when bounds come from separate rules.
     */
    [MaxLength(200, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [MinLength(2, ErrorMessage = "{0} must be at least {1} characters.")]
    [Display(Name = "City")]
    public string? City { get; set; }

    // --- 1c. Format constraint attributes -------------------------------------
    /*
     * [EmailAddress] — validates x@y.z format using a built-in regex.
     *   Does NOT verify the address exists; only checks the format.
     *
     * [Phone] — validates phone-like format: digits, spaces, +, -, (, ).
     *   Very permissive by design — handles international formats loosely.
     *
     * [Url] — validates http:// or https:// prefix.  Rejects relative URLs.
     *
     * [RegularExpression(pattern)] — full custom regex validation.  The pattern
     *   must match the ENTIRE value (anchored to start/end automatically).
     *   PITFALL: write a descriptive ErrorMessage — the raw regex is not
     *   user-friendly ("^\d{5}(-\d{4})?$" means nothing to an end user).
     *
     * All four attributes silently pass null/empty — pair with [Required] when
     * the field is mandatory (see UserName above for the combined pattern).
     */
    [Required(ErrorMessage = "{0} is required.")]
    [EmailAddress(ErrorMessage = "{0} must be a valid email address.")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "{0} must be a valid phone number.")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }                    // optional — no [Required]

    [Url(ErrorMessage = "{0} must start with http:// or https://.")]
    [Display(Name = "Website")]
    public string? Website { get; set; }                        // optional

    [RegularExpression(@"^\d{5}(-\d{4})?$",
        ErrorMessage = "{0} must be a 5-digit ZIP code (12345) or ZIP+4 (12345-6789).")]
    [Display(Name = "ZIP Code")]
    public string? ZipCode { get; set; }                        // optional

    // --- 1d. [DataType] — semantic rendering hint ----------------------------
    /*
     * [DataType(DataType.xxx)] adds a semantic type hint that Razor Tag Helpers
     * use to choose the correct HTML input type:
     *   DataType.Password      → <input type="password">   (masked)
     *   DataType.Date          → <input type="date">
     *   DataType.EmailAddress  → <input type="email">
     *   DataType.PhoneNumber   → <input type="tel">
     *   DataType.Currency      → no input effect; used by [DisplayFormat]
     *   DataType.MultilineText → <textarea>
     *
     * [DataType] does NOT add a validation rule on its own.  It is purely a
     * rendering/UX hint.  Always combine with [Required], [EmailAddress], etc.
     * for actual validation.
     *
     * FULL [DataType] reference — covered with display/format attributes:
     *   → Models/ProductViewModel.cs Section 2b
     */
    [Required(ErrorMessage = "{0} is required.")]
    [DataType(DataType.Password)]                               // <input type="password">
    [Display(Name = "Password")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "{0} must be at least {2} characters.")]
    public string? Password { get; set; }

    // --- 1e. [Compare] — cross-field equality --------------------------------
    /*
     * [Compare(nameof(OtherProperty))] fails when this property's value differs
     * from the other named property.  The most common use: password confirmation.
     *
     * HOW IT WORKS:
     *   - The attribute resolves OtherProperty on the same model type.
     *   - The ValidationResult is attached to THIS property (ConfirmPassword),
     *     so the error message appears next to the confirmation field.
     *
     * PITFALL: If [Required] fires on Password (null), the [Compare] on
     * ConfirmPassword may also fire — the user sees two errors for one omission.
     * Add [Required] to ConfirmPassword too, so both fields get individual errors.
     */
    [Required(ErrorMessage = "{0} is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare(nameof(Password), ErrorMessage = "{0} does not match the password.")]
    public string? ConfirmPassword { get; set; }

    // --- 1f. Error message customization -------------------------------------
    /*
     * THREE WAYS TO CUSTOMIZE ERROR MESSAGES:
     *
     * 1. Hardcoded string — no placeholders:
     *      [Required(ErrorMessage = "Age is required.")]
     *
     * 2. {0} placeholder — replaced with [Display(Name="...")] or property name:
     *      [Required(ErrorMessage = "{0} is required.")]
     *
     *    Additional placeholders by attribute:
     *      [StringLength] : {0}=name  {1}=max  {2}=min
     *      [Range]        : {0}=name  {1}=min  {2}=max
     *      [MaxLength]    : {0}=name  {1}=n
     *      [MinLength]    : {0}=name  {1}=n
     *      [RegularExpression]: {0}=name  {1}=pattern
     *
     * 3. Resource file (localization) — PREVIEW:
     *      [Required(
     *          ErrorMessageResourceType = typeof(ValidationMessages),
     *          ErrorMessageResourceName = nameof(ValidationMessages.AgeRequired))]
     *      Reads the string from a .resx resource file.  Full localization setup
     *      (IStringLocalizer, AddLocalization(), request culture providers) is
     *      COVERED IN DETAIL LATER → ASP.NET Core Localization documentation.
     */
    [Required(ErrorMessage = "Age is required.")]               // hardcoded — no {0}
    [Range(18, 120, ErrorMessage = "{0} must be between {1} and {2}.")]  // {1}=18 {2}=120
    [Display(Name = "Age")]
    public int? Age { get; set; }
}
