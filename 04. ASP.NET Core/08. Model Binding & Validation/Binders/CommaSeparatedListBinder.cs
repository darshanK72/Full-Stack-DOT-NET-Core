/*
 * FILE ROLE: Custom IModelBinder that parses a comma-separated query-string value
 *            into a List<int>, paired with an IModelBinderProvider for registration.
 * SECTIONS IN THIS FILE:
 *   5a. IModelBinder — BindModelAsync, ModelBindingContext
 *   5b. ValueProviderResult — reading the raw value
 *   5c. ModelState error accumulation on bad tokens
 *   5d. IModelBinderProvider — type-based binder selection
 */

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelBindingValidation.Binders;

/*
 * SECTION 5a: IModelBinder
 *
 * IModelBinder has one method:
 *   Task BindModelAsync(ModelBindingContext bindingContext)
 *
 * Responsibilities:
 *   1. Read the raw value from bindingContext.ValueProvider.
 *   2. Convert it to the target CLR type.
 *   3. Set bindingContext.Result to a ModelBindingResult.
 *   4. Add model errors to bindingContext.ModelState for bad input.
 *
 * ModelBindingContext key members:
 *   ModelName          The name of the parameter/property being bound
 *   ValueProvider      The composite provider (route, query, form, …)
 *   ModelState         The ModelStateDictionary — add errors here
 *   Metadata           ModelMetadata describing the target type
 *   Result             Set to ModelBindingResult.Success(value) or .Failed()
 *
 * PITFALL — not setting Result:
 *   If you leave Result unset, the framework treats binding as skipped (not failed).
 *   Always set Result — even to ModelBindingResult.Success with the default value —
 *   so downstream code receives a valid object rather than null.
 */
public sealed class CommaSeparatedListBinder : IModelBinder
{
    /*
     * SECTION 5b: ValueProviderResult — READING THE RAW VALUE
     *
     * bindingContext.ValueProvider is a composite of all active value providers
     * (query string, route data, form data). GetValue(modelName) returns a
     * ValueProviderResult which holds:
     *   FirstValue   The first string value for the key
     *   Values       All string values for the key (used for repeated keys)
     *
     * ValueProviderResult.None means no matching key was found in any provider.
     * Return an empty list in that case rather than failing — the parameter is
     * optional by default.
     */
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);   // guard; available .NET 6+

        string modelName = bindingContext.ModelName;         // e.g. "ids"
        ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueResult == ValueProviderResult.None)
        {
            // Key absent from all providers → return empty list; not a binding failure
            bindingContext.Result = ModelBindingResult.Success(new List<int>());
            return Task.CompletedTask;
        }

        // Register the raw value in ModelState so validation errors bind correctly
        bindingContext.ModelState.SetModelValue(modelName, valueResult);

        string? raw = valueResult.FirstValue;                // "1,5,12"
        List<int> ids = new List<int>();

        /*
         * SECTION 5c: PARSING AND ModelState ERROR ACCUMULATION
         *
         * Split on comma, trim each token, parse to int. Unrecognized tokens add
         * an error to ModelState (keyed to modelName) so the client receives a
         * descriptive validation error rather than a silent 0.
         *
         * After adding model errors the Result is still Success(ids) — a partial
         * list is returned alongside the errors. The [ApiController] filter then
         * detects ModelState.IsValid == false and short-circuits to 422/400.
         */
        if (!string.IsNullOrWhiteSpace(raw))
        {
            string[] parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string token = part.Trim();
                if (int.TryParse(token, out int id))
                {
                    ids.Add(id);
                }
                else
                {
                    bindingContext.ModelState.AddModelError(
                        modelName,
                        $"'{token}' is not a valid integer id.");
                }
            }
        }

        bindingContext.Result = ModelBindingResult.Success(ids);
        return Task.CompletedTask;
    }
}

/*
 * SECTION 5d: IModelBinderProvider — TYPE-BASED BINDER SELECTION
 *
 * IModelBinderProvider decides whether a binder should handle a given type.
 * ASP.NET Core walks the ModelBinderProviders list in order; the first provider
 * that returns a non-null binder wins.
 *
 * REGISTRATION OPTIONS:
 *
 * Option A — global (all List<int> parameters use this binder):
 *   builder.Services.AddControllers(options =>
 *       options.ModelBinderProviders.Insert(0, new CommaSeparatedListBinderProvider()));
 *   Caution: this replaces the default collection binder for List<int> everywhere,
 *   including repeated-key bindings (PaginationQuery.CategoryIds). Use only when
 *   all List<int> parameters in the app should accept comma-separated input.
 *
 * Option B — explicit per-parameter (recommended for this demo):
 *   [ModelBinder(BinderType = typeof(CommaSeparatedListBinder))] List<int> ids
 *   The [ModelBinder] attribute bypasses provider resolution entirely and uses
 *   the named binder directly. This is the approach used in ProductsController.
 *
 * GetBinder contract:
 *   Return a non-null IModelBinder instance → this provider handles the type.
 *   Return null → move on to the next provider in the list.
 */
public sealed class CommaSeparatedListBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Only handle List<int>; return null for all other types
        if (context.Metadata.ModelType == typeof(List<int>))
            return new CommaSeparatedListBinder();

        return null;
    }
}
