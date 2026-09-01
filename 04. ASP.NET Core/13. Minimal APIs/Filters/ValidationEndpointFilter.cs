/*
 * FILE ROLE: Implements a reusable IEndpointFilter that validates every complex
 *            request argument using DataAnnotations before the endpoint handler
 *            executes, returning Results.ValidationProblem() on failure.
 * SECTIONS IN THIS FILE:
 *   7a. IEndpointFilter — pipeline concept and interface contract
 *   7b. ValidationEndpointFilter — DataAnnotations-based validation
 *   7c. Results.ValidationProblem — RFC 7807 problem details response
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MinimalApis.Filters;

/*
 * SECTION 7a: IENDPOINTFILTER — ENDPOINT PIPELINE CONCEPT
 *
 * An endpoint filter wraps the execution of a handler, similar to middleware
 * but scoped to a single endpoint or route group.  Multiple filters form a
 * pipeline: each calls `next(context)` to pass control to the next filter,
 * ultimately reaching the handler.
 *
 *   Request → Filter A → Filter B → Handler → Filter B (after) → Filter A (after)
 *
 * Interface:
 *   ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx,
 *                                  EndpointFilterDelegate next)
 *
 * EndpointFilterInvocationContext exposes:
 *   .Arguments   — IList<object?> of all handler parameters in order
 *   .HttpContext  — the raw HttpContext for the request
 *
 * Registration (on a single endpoint):
 *   app.MapPost("/products", handler).AddEndpointFilter<ValidationEndpointFilter>();
 *
 * Registration (on a group — applies to every endpoint in the group):
 *   var g = app.MapGroup("/products");
 *   g.AddEndpointFilter<ValidationEndpointFilter>();
 *   g.MapPost("/", handler);   // filter runs here
 *   g.MapPut("/{id}", handler); // and here
 *
 * vs Action filters (MVC):
 *   Action filters are IActionFilter / IAsyncActionFilter on controllers.
 *   Endpoint filters are the Minimal APIs equivalent — same pipeline concept,
 *   different interface, no dependency on the MVC pipeline.
 *
 * SECTION 7b: VALIDATIONENDPOINTFILTER — DATAANNOTATIONS VALIDATION
 *
 * The filter iterates every handler argument.  For each argument that is a
 * non-null object, it runs Validator.TryValidateObject() to check all
 * DataAnnotations attributes ([Required], [Range], [StringLength], etc.).
 *
 * If validation fails, the filter short-circuits by returning a problem response
 * WITHOUT calling next() — the handler never runs.
 * If validation passes, it calls await next(context) and returns the handler's result.
 *
 * Pitfall: Validator.TryValidateObject with validateAllProperties: true checks
 * all annotated properties but does NOT recurse into nested objects.  For deep
 * graphs, call Validator.TryValidateObject on each nested object separately or
 * use FluentValidation (third-party library with full recursive support).
 *
 * SECTION 7c: RESULTS.VALIDATIONPROBLEM — RFC 7807 PROBLEM DETAILS
 *
 * Results.ValidationProblem(errors) returns HTTP 400 with a body shaped as:
 *   {
 *     "type":   "https://tools.ietf.org/html/rfc7807",
 *     "title":  "One or more validation errors occurred.",
 *     "status": 400,
 *     "errors": {
 *       "Name":  ["The Name field is required."],
 *       "Price": ["The field Price must be between 0.01 and 999999.99."]
 *     }
 *   }
 *
 * The `errors` dictionary maps field name → array of error messages.
 * This matches ASP.NET Core's ModelState validation problem format, so clients
 * that already handle controller-based validation errors work without changes.
 *
 * TypedResults equivalent: TypedResults.ValidationProblem(errors) — same body,
 * but returns ValidationProblem (strongly typed) rather than IResult.
 */
public sealed class ValidationEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        // Inspect every parameter the handler declared
        foreach (var argument in context.Arguments)
        {
            if (argument is null) continue; // skip missing optional parameters

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(argument); // wraps the object

            bool isValid = Validator.TryValidateObject(
                argument,
                validationContext,
                validationResults,
                validateAllProperties: true); // check every annotated property

            if (!isValid)
            {
                // Build the errors dictionary expected by ValidationProblem
                var errors = new Dictionary<string, string[]>();
                foreach (var result in validationResults)
                {
                    // MemberNames is IEnumerable<string>; use first member name as key
                    var key = result.MemberNames.FirstOrDefault() ?? "General";
                    if (!errors.ContainsKey(key))
                        errors[key] = new string[] { result.ErrorMessage ?? "Invalid value." };
                }

                // Short-circuit: return 400 ValidationProblem without calling the handler
                return Results.ValidationProblem(errors); // RFC 7807 problem details
            }
        }

        // All arguments valid — pass control to the next filter or the handler
        return await next(context);
    }
}
