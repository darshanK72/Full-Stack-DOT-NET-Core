using System;
using System.ComponentModel.DataAnnotations;

namespace DataAnnotationsValidation.Validation;

/*
 * FILE ROLE: Custom validation attribute — demonstrates how to inherit ValidationAttribute
 *            and override IsValid to create a reusable rule.
 * SECTIONS IN THIS FILE:
 *   4. PastDateAttribute — custom ValidationAttribute with FormatErrorMessage support
 */

/*
 * SECTION 4: CUSTOM ValidationAttribute — PastDateAttribute
 *
 * WHEN TO CREATE A CUSTOM ATTRIBUTE:
 *   - The built-in attributes cannot express the rule ([Range] with dynamic values,
 *     domain-specific patterns, business rules beyond format checks)
 *   - The rule is reused across multiple models
 *   - You want the rule to participate in MVC's automatic validation pipeline
 *
 * STEPS:
 *   1. Inherit from ValidationAttribute (System.ComponentModel.DataAnnotations)
 *   2. Override IsValid(object? value, ValidationContext validationContext)
 *        — receives the raw property value (object?) and the context
 *        — return null / ValidationResult.Success for valid
 *        — return new ValidationResult(message) for invalid
 *   3. Pass a default message template to the base constructor for {0} support
 *   4. Call FormatErrorMessage(displayName) to apply the {0} placeholder
 *
 * SIMPLE OVERRIDE (no ValidationContext needed):
 *   protected override ValidationResult? IsValid(object? value)
 *   Use this when you only need the value — no access to display name or model.
 *
 * FULL OVERRIDE (with ValidationContext):
 *   protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
 *   Use when you need:
 *     ctx.DisplayName        — [Display(Name="...")] or property name
 *     ctx.ObjectInstance     — the full model (for cross-field reads)
 *     ctx.MemberName         — raw property name
 *     ctx.GetService<T>()    — resolve DI services (e.g., IRepository for uniqueness checks)
 *
 * ATTRIBUTE TARGET:
 *   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
 *   Restricts use to properties and fields only (not classes or methods).
 *   AllowMultiple = false prevents applying the same attribute twice on one property.
 *
 * CLIENT-SIDE SUPPORT (PREVIEW → 14. Client-Side Validation):
 *   Custom attributes do NOT emit client-side validation data-val-* attributes
 *   automatically.  To add client-side support, implement IClientModelValidator
 *   (or IClientValidatable in older MVC) and add the jQuery Validate rule adapter.
 *   COVERED IN DETAIL LATER → 14. Client-Side Validation
 */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class PastDateAttribute : ValidationAttribute
{
    /*
     * Pass a default error message template to base() — it becomes the default
     * when the caller omits ErrorMessage on the attribute.
     * {0} will be replaced by FormatErrorMessage(displayName) at validation time.
     */
    public PastDateAttribute() : base("{0} must be a date in the past.")
    {
    }

    /*
     * IsValid — the core validation logic.
     *
     * NULL HANDLING: return ValidationResult.Success (null) when value is null.
     *   The attribute treats null as valid — pair [PastDate] with [Required]
     *   on the property when null should be rejected.
     *
     * PATTERN MATCH: "value is DateTime date" safely casts and binds in one step.
     *   If the type is not DateTime (wrong property type), validation passes
     *   silently — the type mismatch is a programming error, not a user error.
     */
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;        // null is valid — [Required] handles mandatory

        if (value is DateTime date && date >= DateTime.Today)  // today or any future date = invalid
        {
            // FormatErrorMessage applies the {0} placeholder using the property's display name
            string message = FormatErrorMessage(validationContext.DisplayName);
            return new ValidationResult(message);
        }

        return ValidationResult.Success;            // past date — ValidationResult.Success == null
    }
}
