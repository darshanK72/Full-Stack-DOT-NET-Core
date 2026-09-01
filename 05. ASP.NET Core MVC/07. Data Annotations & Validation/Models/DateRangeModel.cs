using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAnnotationsValidation.Models;

/*
 * FILE ROLE: Demonstrates IValidatableObject for cross-property (multi-field) validation.
 * SECTIONS IN THIS FILE:
 *   3. IValidatableObject — cross-property rules that span two or more fields
 */

/*
 * SECTION 3: IValidatableObject — CROSS-PROPERTY VALIDATION
 *
 * WHY IT EXISTS:
 *   DataAnnotation attributes validate a SINGLE property in isolation.  They cannot
 *   express rules that depend on multiple properties at once — e.g., "end date must
 *   be after start date" or "discount cannot exceed the base price".
 *
 * SOLUTION: Implement IValidatableObject on the model class.  Its Validate() method
 * receives the whole model instance and can read any combination of properties.
 *
 * INTERFACE CONTRACT:
 *   IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
 *
 *   yield return ValidationResult.Success (null)        → field passes
 *   yield return new ValidationResult(message)          → error; no member name
 *   yield return new ValidationResult(message, members) → error on specific fields
 *
 *   Use new[] { nameof(Prop) } for members to attach the error to a specific field
 *   so asp-validation-for="Prop" in the view can display it next to that field.
 *
 * EXECUTION ORDER:
 *   1. MVC runs all property-level DataAnnotation attributes first.
 *   2. If ALL [Required] checks pass (no null/empty required fields),
 *      MVC calls IValidatableObject.Validate().
 *   3. If any [Required] already failed, Validate() is NOT called — the form has
 *      errors before cross-property checking even begins.
 *
 * PITFALL: IValidatableObject.Validate() is SKIPPED when using
 *   Validator.TryValidateProperty() (validates one property at a time).
 *   It RUNS when using Validator.TryValidateObject(model, ctx, results,
 *   validateAllProperties: true) — which is what MVC's pipeline calls.
 *
 * ALTERNATIVE: FluentValidation library — richer rule DSL, better testability,
 *   supports cross-property rules with cleaner syntax.  IValidatableObject is the
 *   built-in zero-dependency option.
 */
public sealed class DateRangeModel : IValidatableObject
{
    [Required(ErrorMessage = "{0} is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Display(Name = "Label")]
    [MaxLength(50, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Label { get; set; }

    /*
     * Validate() — cross-property rules run after all attribute validations pass.
     *
     * ValidationResult constructor overloads:
     *   new ValidationResult(string errorMessage)
     *       — adds an error to ModelState with no specific field key.
     *       — shows in asp-validation-summary but NOT under any individual field.
     *
     *   new ValidationResult(string errorMessage, IEnumerable<string> memberNames)
     *       — attaches the error to the named field(s) in ModelState.
     *       — shows under that field's asp-validation-for tag AND in the summary.
     *
     * Access the full model via validationContext.ObjectInstance when you need
     * to read properties not directly on 'this' (e.g., a nested object).
     */
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)                               // EndDate must be after StartDate
            yield return new ValidationResult(
                "End Date must be later than Start Date.",
                new[] { nameof(EndDate) });                     // attach error to EndDate field

        if (StartDate > DateTime.Today)                         // StartDate must not be future
            yield return new ValidationResult(
                "Start Date cannot be in the future.",
                new[] { nameof(StartDate) });                   // attach error to StartDate field
    }
}
