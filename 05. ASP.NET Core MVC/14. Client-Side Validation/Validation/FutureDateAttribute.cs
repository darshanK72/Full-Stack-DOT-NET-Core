/*
 * FILE ROLE: Implements a custom validation attribute that enforces a future date rule
 *            on both the server (via IsValid) and the client (via IClientModelValidator /
 *            AddValidation), so the same attribute drives both validation tiers.
 * SECTIONS IN THIS FILE:
 *   4. IClientModelValidator — server attribute + client metadata
 *      4a. IsValid — server-side logic
 *      4b. AddValidation — emitting data-val-futuredate attribute
 *      4c. MergeAttribute helper — safe attribute writing
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClientSideValidation.Validation;

/*
 * SECTION 4: IClientModelValidator — SERVER ATTRIBUTE + CLIENT METADATA
 *
 * To get client-side validation from a custom attribute you need TWO things:
 *
 *   A) Server side — inherit ValidationAttribute and override IsValid().
 *      This is what ModelState checks in the POST action.
 *
 *   B) Client side — implement IClientModelValidator (namespace:
 *      Microsoft.AspNetCore.Mvc.ModelBinding.Validation).
 *      Its single method, AddValidation(), is called by the Tag Helper infrastructure
 *      when rendering an <input>. You write into context.Attributes to emit
 *      data-val-* HTML attributes.
 *
 * After the HTML attribute is on the page, a matching jQuery adapter must be
 * registered in JavaScript BEFORE $.validator.unobtrusive.parse() runs.
 * See Section 8 in Views/Shared/_ValidationScriptsPartial.cshtml.
 *
 * IClientModelValidator lives in the package:
 *   Microsoft.AspNetCore.Mvc.Core (included transitively via the Web SDK — no extra NuGet)
 *
 * [AttributeUsage] restricts where the attribute can be applied.
 * AllowMultiple = false means a property cannot have two [FutureDate] attributes.
 */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class FutureDateAttribute : ValidationAttribute, IClientModelValidator
{
    /*
     * --- 4a. IsValid — server-side logic ---
     *
     * Called by the MVC model binder during server-side validation.
     * Returning ValidationResult.Success means the value is valid.
     * Returning new ValidationResult(message) adds the error to ModelState.
     *
     * Important: if value is null, return Success here — let [Required] handle
     * the "missing value" case so error messages are not duplicated.
     */
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) // null = not provided; [Required] handles this separately
            return ValidationResult.Success;

        if (value is DateTime dateValue)
        {
            if (dateValue.Date <= DateTime.Today) // past or today is invalid
                return new ValidationResult(GetErrorMessage());
        }

        return ValidationResult.Success;
    }

    /*
     * --- 4b. AddValidation — emitting the data-val-futuredate attribute ---
     *
     * AddValidation() is called once per <input asp-for="..."> render.
     * You add entries to context.Attributes; each entry becomes an HTML attribute
     * on the rendered element.
     *
     * REQUIRED entries:
     *   "data-val"              = "true"       — master switch; marks the field for parsing
     *   "data-val-{rulename}"   = "message"    — the rule; value = error message shown to user
     *
     * OPTIONAL extra parameters (passed to the JS adapter):
     *   "data-val-{rulename}-{paramname}" = "value"
     *   e.g. data-val-range-min="18", data-val-range-max="120"
     *   FutureDateAttribute has no extra parameters — the rule is boolean (pass/fail).
     *
     * Naming convention for {rulename}:
     *   Must be lowercase; must match the name passed to jQuery.validator.addMethod()
     *   and to $.validator.unobtrusive.adapters.addBool() in the JS file.
     *
     * context.Attributes is IDictionary<string, string>.
     * MergeAttribute only adds if the key is not already present (safe for stacking).
     */
    public void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");        // master switch
        MergeAttribute(context.Attributes, "data-val-futuredate", GetErrorMessage()); // rule
    }

    /*
     * --- 4c. MergeAttribute helper — safe attribute writing ---
     *
     * Two attributes on the same property could both try to set "data-val" to "true".
     * MergeAttribute ensures the first one wins (no duplicate key exception).
     * This mirrors the helper used internally by the built-in validation attributes.
     */
    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key)) // only add if not already present
            attributes.Add(key, value);
    }

    private string GetErrorMessage()
    {
        // Fall back to a default message if the caller did not set ErrorMessage
        return string.IsNullOrEmpty(ErrorMessage)
            ? "The date must be in the future."
            : ErrorMessage;
    }
}

/*
 * CUSTOM ADAPTER PATTERN (JavaScript — see _ValidationScriptsPartial.cshtml Section 8)
 *
 * After defining the attribute above, you register the client counterpart in JS:
 *
 *   // 1. Register the validator method (the actual test)
 *   jQuery.validator.addMethod("futuredate", function(value, element) {
 *       if (!value) return true; // no value → let [Required] handle it
 *       var d = new Date(value);
 *       return !isNaN(d.getTime()) && d > new Date(); // must be strictly in the future
 *   });
 *
 *   // 2. Register the adapter (maps data-val-futuredate → the method above)
 *   $.validator.unobtrusive.adapters.addBool("futuredate");
 *   //                                        ^^^^^^^^^^^
 *   //                                        must match data-val-{name} and addMethod name
 *
 * Four adapter helper methods (choose based on parameter count):
 *   addBool(name)                        — no extra params; rule is pass/fail
 *   addSingleVal(name, paramName)        — one extra param: data-val-{name}-{paramName}
 *   addMinMax(name, minRule, maxRule,    — two params: min / max (like range / length)
 *             minParam, maxParam)
 *   add(name, params[], fn(options))     — full control; fn sets options.rules / options.messages
 *
 * If you add extra parameters in AddValidation() you would use addSingleVal or add().
 * Example with a parameter (minimum-hours-ahead):
 *   C#:  MergeAttribute(ctx.Attributes, "data-val-futuredate-minhours", "24");
 *   JS:  $.validator.unobtrusive.adapters.addSingleVal("futuredate", "minhours");
 *        jQuery.validator.addMethod("futuredate", function(value, element, minhours) { ... });
 */
