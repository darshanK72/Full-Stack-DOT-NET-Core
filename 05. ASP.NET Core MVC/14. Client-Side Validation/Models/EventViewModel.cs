/*
 * FILE ROLE: Shows how a model that uses a custom validation attribute (FutureDateAttribute)
 *            integrates with the IClientModelValidator pipeline so both server-side and
 *            client-side validation are driven by the same attribute.
 * SECTIONS IN THIS FILE:
 *   3. Custom Validator Attribute — Consumer Model
 */

using System;
using System.ComponentModel.DataAnnotations;
using ClientSideValidation.Validation;

namespace ClientSideValidation.Models;

/*
 * SECTION 3: CUSTOM VALIDATOR ATTRIBUTE — CONSUMER MODEL
 *
 * This view-model uses [FutureDate], which is a custom attribute defined in
 * Validation/FutureDateAttribute.cs. Because FutureDateAttribute implements
 * IClientModelValidator, the asp-for Tag Helper also emits a matching data-val-*
 * attribute so the JavaScript side can validate without a round-trip.
 *
 * Rendered HTML for EventDate:
 *   <input id="EventDate" name="EventDate" type="datetime-local"
 *          data-val="true"
 *          data-val-required="Event date is required."
 *          data-val-futuredate="Event date must be in the future." />
 *
 * The "futuredate" key is set by FutureDateAttribute.AddValidation().
 * On the JavaScript side a matching adapter maps this attribute to a
 * jQuery.validator method named "futuredate" — see Section 8 in
 * Views/Shared/_ValidationScriptsPartial.cshtml.
 *
 * Summary of how a custom attribute reaches the client:
 *   1. Attribute class inherits ValidationAttribute  — server validation
 *   2. Attribute class implements IClientModelValidator — emits data-val-*
 *   3. jQuery.validator.addMethod("futuredate", fn)  — client rule
 *   4. $.validator.unobtrusive.adapters.addBool(...)  — adapter maps data-val-* → rule
 */
public sealed class EventViewModel
{
    [Required(ErrorMessage = "Event title is required.")]
    [StringLength(200, MinimumLength = 3,
        ErrorMessage = "Title must be between 3 and 200 characters.")]
    [Display(Name = "Event Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; } // nullable — optional field

    /*
     * [FutureDate] is the custom attribute that implements IClientModelValidator.
     * The Tag Helper reads it via IModelMetadataProvider and calls AddValidation(),
     * which adds data-val-futuredate="..." to the rendered <input> element.
     */
    [Required(ErrorMessage = "Event date is required.")]
    [FutureDate(ErrorMessage = "Event date must be in the future.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Event Date")]
    public DateTime? EventDate { get; set; } // nullable so unsubmitted value = null, not default

    [Required(ErrorMessage = "Max attendees is required.")]
    [Range(1, 1000, ErrorMessage = "Max attendees must be between 1 and 1000.")]
    [Display(Name = "Max Attendees")]
    public int? MaxAttendees { get; set; }
}
