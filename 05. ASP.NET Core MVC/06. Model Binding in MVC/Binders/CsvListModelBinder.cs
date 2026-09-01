/*
 * FILE ROLE: Custom IModelBinder that parses a comma-separated query/form value
 *            into a List<string>. Also demonstrates IModelBinderProvider for
 *            global registration and IValueProvider for raw value access.
 * SECTIONS IN THIS FILE:
 *   4a. IValueProvider — how raw values are surfaced to binders
 *   4b. IModelBinder — BindModelAsync implementation
 *   4c. IModelBinderProvider — GetBinder for global registration
 */

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelBindingMvc.Binders;

/*
 * SECTION 4a: IValueProvider — HOW RAW VALUES REACH BINDERS
 *
 * The binder infrastructure reads raw string values through IValueProvider.
 * A ModelBindingContext exposes ctx.ValueProvider, which is a CompositeValueProvider
 * that aggregates all registered providers (route, form, query string) in priority order.
 *
 * Getting a value from the provider:
 *   ValueProviderResult result = ctx.ValueProvider.GetValue(ctx.ModelName);
 *
 *   result.FirstValue         — first (or only) string value; null if not found
 *   result.Values             — StringValues (one or more values, e.g. repeated keys)
 *   result == ValueProviderResult.None  — key not present in ANY provider
 *
 * Value provider priority in MVC (matches binding precedence):
 *   1. RouteValueProvider     — values from {route} segments
 *   2. FormValueProvider      — values from form POST body
 *   3. QueryStringValueProvider — values from URL ?key=value
 *
 * Building a custom IValueProvider:
 *   Rarely needed — the built-in providers cover route, form, and query.
 *   Useful for binding from cookies, XML bodies, or custom header formats.
 *   Implement IValueProvider + IValueProviderFactory, then register via
 *   options.ValueProviderFactories.Insert(0, new MyValueProviderFactory()).
 */

/*
 * SECTION 4b: IModelBinder — BindModelAsync IMPLEMENTATION
 *
 * IModelBinder.BindModelAsync(ModelBindingContext context) is the core contract:
 *   - Read the raw value from context.ValueProvider.
 *   - Parse / transform it into the target type.
 *   - Set context.Result to ModelBindingResult.Success(value)
 *     OR ModelBindingResult.Failed() (also add to context.ModelState on parse errors).
 *
 * ModelBindingContext members used here:
 *   context.ModelName        — the parameter/property name being bound (key to look up)
 *   context.ValueProvider    — access to all value sources
 *   context.ModelState       — dictionary to record binding errors
 *   context.Result           — must be set before returning (Success or Failed)
 *   context.ModelMetadata    — type info about the target (context.ModelType here)
 *
 * This binder handles the pattern: ?tags=Electronics,Sale,Featured
 * The comma-separated string is split and returned as List<string>.
 * Standard repeated-key notation (?tags=Electronics&tags=Sale) still works via
 * the default collection binder; this binder enables the comma-separated shorthand.
 *
 * Registration options:
 *   a) [ModelBinder(BinderType = typeof(CsvListModelBinder))] on a parameter — opt-in.
 *   b) CsvListModelBinderProvider registered globally — applies to all List<string>
 *      parameters without needing the attribute.
 *   Option (a) is safer in most apps; option (b) is used in Program.cs to demonstrate
 *   global registration.
 *
 * PITFALL — returning without setting context.Result:
 *   If you return without setting context.Result, the binder chain treats the binder
 *   as if it did not handle the parameter and falls through to the next provider.
 *   Always set context.Result before returning from BindModelAsync.
 */
public class CsvListModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Read the raw value from the current value provider chain
        ValueProviderResult valueResult = context.ValueProvider.GetValue(context.ModelName);

        if (valueResult == ValueProviderResult.None)
        {
            // Key not present in any value provider — leave unbound (not an error)
            context.Result = ModelBindingResult.Success(new List<string>());
            return Task.CompletedTask;
        }

        // Record the attempted value in ModelState (makes validation error messages clear)
        context.ModelState.SetModelValue(context.ModelName, valueResult);

        string? rawValue = valueResult.FirstValue;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            // Empty string → empty list (not an error for an optional list)
            context.Result = ModelBindingResult.Success(new List<string>());
            return Task.CompletedTask;
        }

        // Split on comma, trim whitespace, filter empty segments
        var items = new List<string>();
        foreach (string segment in rawValue.Split(','))
        {
            string trimmed = segment.Trim();
            if (trimmed.Length > 0)
                items.Add(trimmed);
        }

        context.Result = ModelBindingResult.Success(items); // binding succeeded
        return Task.CompletedTask;
    }
}

/*
 * SECTION 4c: IModelBinderProvider — GetBinder FOR GLOBAL REGISTRATION
 *
 * IModelBinderProvider.GetBinder(ModelBinderProviderContext context) is called
 * by the MVC framework to determine which IModelBinder to use for a given type.
 *
 * Return null to pass to the next provider in the chain; return an IModelBinder
 * instance to claim the parameter.
 *
 * This provider activates CsvListModelBinder for any List<string> parameter.
 * It is inserted at position 0 in Program.cs so it runs BEFORE the default
 * collection binder — otherwise the default binder would claim List<string> first.
 *
 * Registration in Program.cs:
 *   builder.Services.AddControllersWithViews(options =>
 *   {
 *       options.ModelBinderProviders.Insert(0, new CsvListModelBinderProvider());
 *   });
 *
 * PITFALL — provider position:
 *   options.ModelBinderProviders is an ordered list. Providers are tried in order.
 *   If your provider is added after a built-in provider that already handles the
 *   target type, yours will never be reached. Use Insert(0, …) to ensure priority,
 *   or verify existing providers with options.ModelBinderProviders.Any(p => …).
 */
public class CsvListModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Only handle List<string> — leave everything else to later providers
        if (context.Metadata.ModelType == typeof(List<string>))
            return new CsvListModelBinder();

        return null; // not our type — pass to next provider
    }
}
