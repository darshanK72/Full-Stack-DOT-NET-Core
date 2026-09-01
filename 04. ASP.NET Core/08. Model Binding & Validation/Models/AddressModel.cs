/*
 * FILE ROLE: Demonstrates IValidatableObject — the model-level interface for
 *            cross-property validation rules that cannot be expressed with
 *            single-property DataAnnotations attributes.
 * SECTIONS IN THIS FILE:
 *   3a. IValidatableObject — purpose and pipeline position
 *   3b. Validate() implementation — yield return pattern for multiple errors
 *   3c. Cross-property rules — IsInternational implies Country required
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModelBindingValidation.Models;

/*
 * SECTION 3a: IValidatableObject
 *
 * Implement IValidatableObject when a validation rule depends on two or more
 * properties of the same model — something no single-property attribute can do.
 *
 * Pipeline position (IMPORTANT):
 *   ASP.NET Core calls Validate() ONLY AFTER all per-property DataAnnotations
 *   attributes have passed. If any [Required], [Range], etc. fails, Validate()
 *   is skipped. Design Validate() assuming properties already satisfy their
 *   individual constraints.
 *
 * Return type: IEnumerable<ValidationResult>
 *   Use `yield return` to emit zero, one, or many errors lazily.
 *   Return an empty enumerable (or yield-break) when everything is valid.
 *   Each ValidationResult carries:
 *     - errorMessage  : human-readable description
 *     - memberNames   : IEnumerable<string> of the property keys to attach the
 *                       error to in ModelState (can be more than one)
 *
 * Complex type binding:
 *   When AddressModel is a property of another model (e.g., CreateProductRequest),
 *   ASP.NET Core binds its sub-properties from the same source using dot-notation:
 *     JSON:         { "shippingAddress": { "street": "…", "city": "…" } }
 *     [FromQuery]:  ?shippingAddress.street=…&shippingAddress.city=…
 *   Validation is recursive: AddressModel's attributes and Validate() all run.
 */
public sealed class AddressModel : IValidatableObject
{
    [Required(ErrorMessage = "Street is required.")]
    [StringLength(200, ErrorMessage = "Street cannot exceed 200 characters.")]
    public string Street { get; set; } = string.Empty;      // init default avoids CS8618

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string? State { get; set; }                      // nullable — optional for international

    [Required(ErrorMessage = "Postal code is required.")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Postal code must be 3–10 characters.")]
    public string PostalCode { get; set; } = string.Empty;

    public bool IsInternational { get; set; }               // drives conditional validation

    [StringLength(100)]
    public string? Country { get; set; }                    // required only when IsInternational

    /*
     * SECTION 3b: Validate() IMPLEMENTATION — yield return PATTERN
     *
     * yield return lets you emit errors one by one without building a collection.
     * The caller (the validation framework) iterates lazily.
     *
     * If no errors apply, the method exits without yielding — the enumerable is
     * empty, which the framework treats as "all cross-property rules passed."
     *
     * SECTION 3c: CROSS-PROPERTY RULES
     *
     * Rule 1 — International addresses require Country.
     * Rule 2 — Domestic addresses require State.
     *
     * These cannot be expressed with [Required] alone because they are conditional
     * on another property (IsInternational). IValidatableObject handles them cleanly.
     */
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsInternational && string.IsNullOrWhiteSpace(Country))
        {
            // Member name must match the property name for ModelState keying
            yield return new ValidationResult(
                "Country is required for international addresses.",
                new[] { nameof(Country) });
        }

        if (!IsInternational && string.IsNullOrWhiteSpace(State))
        {
            yield return new ValidationResult(
                "State is required for domestic addresses.",
                new[] { nameof(State) });
        }
    }
}
