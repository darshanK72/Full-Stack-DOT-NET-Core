/*
 * FILE ROLE: Request DTO decorated with DataAnnotations attributes, demonstrating
 *            all major built-in validation attributes used in ASP.NET Core APIs.
 * SECTIONS IN THIS FILE:
 *   1.  DataAnnotations overview — how attributes integrate with ModelState
 *   2.  [Required] — value must be present and non-empty
 *   3.  [StringLength] — character-count constraint
 *   4.  [Range] — numeric (or comparable) range
 *   5.  [RegularExpression] — regex pattern match
 *   6.  [EmailAddress] — email format shortcut
 *   7.  [Compare] — two properties must be equal
 *   8.  Custom attribute — [FutureDateAttribute] from Validation/
 */

using System;
using System.ComponentModel.DataAnnotations;
using ModelBindingValidation.Validation;

namespace ModelBindingValidation.Models;

/*
 * SECTION 1: DataAnnotations OVERVIEW
 *
 * System.ComponentModel.DataAnnotations provides declarative, attribute-based
 * validation. Each attribute is evaluated during model binding before the
 * action method body runs (when [ApiController] is present).
 *
 * How errors accumulate:
 *   - Each failing attribute adds a ValidationResult to ModelState.
 *   - ASP.NET Core checks ModelState.IsValid after all attributes run.
 *   - With [ApiController], an invalid ModelState returns 400/422 automatically
 *     (configurable via InvalidModelStateResponseFactory in Program.cs).
 *
 * DTO vs domain model:
 *   Annotate the inbound DTO (request shape), not the domain entity. Keeping
 *   validation attributes on the DTO means your domain model stays persistence-
 *   and HTTP-agnostic. Map the validated DTO to the domain object in the service.
 *
 * BINDING SOURCE for this DTO: [FromBody]
 *   ProductsController.Create receives CreateProductRequest from the JSON body:
 *     POST /api/products  Content-Type: application/json
 *     {
 *       "name": "Widget Pro",
 *       "price": 29.99,
 *       "stock": 100,
 *       "sku": "WGT-0042",
 *       "contactEmail": "vendor@example.com",
 *       "confirmEmail": "vendor@example.com",
 *       "availableFrom": "2026-09-01T00:00:00Z"
 *     }
 */
public sealed class CreateProductRequest
{
    /*
     * SECTION 2: [Required] and [StringLength]
     *
     * [Required] — rejects null, empty string, and whitespace-only strings.
     *   With <Nullable>enable</Nullable> and a non-nullable property, the compiler
     *   guarantees the property is never null in your code; [Required] is still
     *   needed to catch a missing JSON key during binding (the binder sets null
     *   even on a non-nullable property when the key is absent).
     *
     * [StringLength(max)] — rejects strings longer than max.
     *   MinimumLength adds a lower bound; set ErrorMessage for friendly text.
     *   Pair with [Required] — [StringLength] passes on null (null has no length).
     */
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;       // = string.Empty → avoids CS8618

    /*
     * SECTION 4: [Range]
     *
     * [Range(min, max)] works on numeric types (int, double, decimal) and also on
     * any type that implements IComparable.
     *
     * For decimal literals in attributes, append 'm' — attribute arguments must
     * be compile-time constants, and the overload accepts double here, so the
     * framework converts the double at runtime. For exact decimal semantics,
     * use the string overload: [Range(typeof(decimal), "0.01", "99999.99")].
     */
    [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99,999.99.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }

    /*
     * SECTION 5: [RegularExpression]
     *
     * [RegularExpression(pattern)] validates the entire string against the pattern
     * (implicitly anchored with ^ and $).
     *
     * PITFALL — @ prefix:
     *   Always use a verbatim string literal (@"…") for regex patterns in attributes
     *   so backslashes are not doubled. "^\\d{4}$" vs @"^\d{4}$" — same result but
     *   verbatim is far more readable.
     *
     * PITFALL — null inputs:
     *   [RegularExpression] passes silently on null. Add [Required] if null must
     *   also be rejected.
     *
     * Example here: SKU format = 2–4 uppercase letters, hyphen, 4 digits (WGT-0042).
     */
    [RegularExpression(@"^[A-Z]{2,4}-\d{4}$",
        ErrorMessage = "SKU must be 2–4 uppercase letters, a hyphen, then 4 digits (e.g. WGT-0042).")]
    public string? Sku { get; set; }

    /*
     * SECTION 6: [EmailAddress]
     *
     * [EmailAddress] is a shortcut for a commonly needed regex. Internally it
     * checks for an @ sign and a domain segment; it is NOT a full RFC-5321 check.
     * For stricter validation, use [RegularExpression] with a stricter pattern
     * or validate against a real SMTP server.
     *
     * The property is nullable (string?) — ContactEmail is optional. If provided,
     * it must be a valid email format. [EmailAddress] passes on null automatically.
     */
    [EmailAddress(ErrorMessage = "ContactEmail must be a valid email address.")]
    public string? ContactEmail { get; set; }

    /*
     * SECTION 7: [Compare]
     *
     * [Compare(otherPropertyName)] checks that this property's value equals the
     * named property's value. Commonly used for email/password confirmation fields.
     *
     * Internals:
     *   The framework uses reflection to read the other property by name. The
     *   comparison is case-sensitive by default (object.Equals). Both null values
     *   are considered equal (null == null → passes).
     *
     * PITFALL — property name string:
     *   The name is a string literal, not compiler-checked. Use nameof() to get a
     *   compile-time error if the property is renamed.
     */
    [Compare(nameof(ContactEmail),
        ErrorMessage = "ConfirmEmail must match ContactEmail.")]
    public string? ConfirmEmail { get; set; }

    /*
     * SECTION 8: CUSTOM ATTRIBUTE — [FutureDateAttribute]
     *
     * See Validation/FutureDateAttribute.cs for the full implementation.
     *
     * Custom attributes compose with built-in ones: AvailableFrom is optional
     * (no [Required]), so a missing value passes both [FutureDateAttribute] and
     * the model. Only when provided must the date be in the future.
     */
    [FutureDateAttribute]
    public DateTime? AvailableFrom { get; set; }           // nullable → optional field
}
