/*
 * FILE ROLE: Implements a custom ValidationAttribute that rejects dates in the past,
 *            showing how to extend the DataAnnotations framework with reusable rules.
 * SECTIONS IN THIS FILE:
 *   4a. Custom ValidationAttribute — inherit, override IsValid
 *   4b. FormatErrorMessage — use the {0} placeholder convention
 *   4c. MemberNames — tell the framework which property failed
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace ModelBindingValidation.Validation;

/*
 * SECTION 4a: CUSTOM ValidationAttribute
 *
 * Inherit from System.ComponentModel.DataAnnotations.ValidationAttribute and
 * override IsValid(object? value, ValidationContext validationContext).
 *
 * Why override the two-argument overload instead of IsValid(object? value)?
 *   The single-argument version cannot return MemberNames, so the error is not
 *   associated with the specific property in ModelState. The two-argument version
 *   lets you return a ValidationResult that names the failing field.
 *
 * Lifecycle in ASP.NET Core model binding:
 *   1. Value providers read raw HTTP data (query string, body, route, form).
 *   2. Model binders convert raw strings to CLR types.
 *   3. DataAnnotations validation runs:
 *        a. Per-property DataAnnotations attributes (including custom ones).
 *        b. IValidatableObject.Validate() — only if step a passed.
 *   4. [ApiController] filter checks ModelState.IsValid and auto-returns 422/400.
 *
 * PITFALL — null values:
 *   Return ValidationResult.Success when value is null so [Required] handles the
 *   "missing" case separately. Combining both responsibilities in one attribute
 *   produces confusing error messages.
 *
 * PITFALL — timezone:
 *   DateTime.Now is local time; DateTime.UtcNow is UTC. Use UtcNow when the
 *   application might run across time zones. Clients should send UTC ISO-8601.
 */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    /*
     * SECTION 4b: FormatErrorMessage CONVENTION
     *
     * Pass the error message template to the base constructor. The {0} placeholder
     * is replaced with the display name of the property at runtime when
     * FormatErrorMessage(displayName) is called.
     *
     * This keeps error messages consistent with built-in attribute messages such as
     * "The {0} field is required." (from [Required]).
     */
    public FutureDateAttribute()
        : base("The {0} field must be a date and time in the future (UTC).")
    {
    }

    /*
     * SECTION 4c: IsValid — RETURN ValidationResult WITH MemberNames
     *
     * Return ValidationResult.Success   → attribute passes.
     * Return new ValidationResult(msg)  → attribute fails; error added to ModelState.
     *
     * Passing new[] { validationContext.MemberName } in the ValidationResult
     * associates the error with the specific property key in ModelState, so the
     * client receives errors keyed by property name rather than the model root.
     */
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;        // null → defer to [Required] if present

        if (value is DateTime date && date.ToUniversalTime() > DateTime.UtcNow)
            return ValidationResult.Success;        // future date ✓

        // Build the error message using the {0} → display-name convention
        string errorMessage = FormatErrorMessage(validationContext.DisplayName);

        // Return the failing member name so ModelState keys the error correctly
        string memberName = validationContext.MemberName ?? string.Empty;
        return new ValidationResult(errorMessage, new[] { memberName });
    }
}
