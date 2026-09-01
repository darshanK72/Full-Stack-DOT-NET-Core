/*
 * FILE ROLE: Strongly-typed ViewModel used by the form tag helper demo.
 *            Properties drive <label asp-for>, <input asp-for>, <select asp-for>,
 *            <textarea asp-for>, and all validation tag helpers.
 *
 * SECTIONS IN THIS FILE:
 *   1. ContactFormViewModel — properties with DataAnnotations + SelectList metadata
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TagHelpers.Models;

/*
 * SECTION 1: CONTACT FORM VIEW MODEL
 *
 * How asp-for reads this class at render time:
 *
 *   Attribute            | Read from                | Emitted HTML
 *   ---------------------|--------------------------|------------------------------
 *   asp-for="Name"       | Property name            | id="Name" name="Name"
 *   (label)              | [Display(Name = "...")]  | label text
 *   (input type)         | [DataType] / CLR type    | type="text"/"email"/"date"...
 *   (validation attrs)   | [Required], [StringLength]| data-val="true" data-val-required
 *   asp-for="BirthDate"  | DateTime?                | type="datetime-local" by default
 *   asp-format="{0:...}" | format string            | formatted value attribute
 *   asp-for="AcceptTerms"| bool                     | type="checkbox"
 *
 * SelectList for asp-items:
 *   asp-items on <select asp-for="CategoryId"> expects IEnumerable<SelectListItem>.
 *   SelectList is a named subclass of MultiSelectList that accepts any IEnumerable
 *   and projects a value field and text field via reflection on property names.
 *
 *   IMPORTANT: Do NOT mark Categories or GroupedOptions with [Required].
 *   These properties carry dropdown metadata — they are NOT posted back with the form.
 *   Always repopulate them in the controller before returning View(model) on validation failure.
 *
 * SelectListGroup for <optgroup>:
 *   Assigning SelectListItem.Group = new SelectListGroup { Name = "..." } causes the
 *   Razor SelectTagHelper to wrap <option> elements inside <optgroup label="..."> tags.
 */
public class ContactFormViewModel
{
    // --- Text fields ---

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name must be 1–100 characters.")]
    [Display(Name = "Full Name")]             // <label asp-for="Name"> → "Full Name"
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email Address")]         // [EmailAddress] → <input type="email">
    public string Email { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be 10–2000 characters.")]
    [Display(Name = "Your Message")]          // drives <textarea asp-for="Message">
    public string Message { get; set; } = string.Empty;

    // --- Category dropdown (asp-for + asp-items) ---

    [Display(Name = "Category")]
    public int CategoryId { get; set; }       // int → <select> selected value

    // Populated by controller; NOT posted back; nullable so repopulation is explicit
    public SelectList? Categories { get; set; }

    // Populated for optgroup demo — each SelectListItem has a Group assigned
    public IEnumerable<SelectListItem>? GroupedOptions { get; set; }

    // --- Date with asp-format ---

    [DataType(DataType.Date)]                 // [DataType] overrides inferred type="datetime-local"
    [Display(Name = "Date of Birth")]
    public DateTime? BirthDate { get; set; } // nullable → no default value in input

    // --- Checkbox ---

    [Display(Name = "I accept the terms and conditions")]
    public bool AcceptTerms { get; set; }    // bool → <input type="checkbox">
}
