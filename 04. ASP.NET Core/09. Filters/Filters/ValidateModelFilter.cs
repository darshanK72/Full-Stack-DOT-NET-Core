/*
 * FILE ROLE: Teaches IActionFilter for model validation — demonstrates how
 *            OnActionExecuting can short-circuit the request pipeline when
 *            ModelState is invalid, centralizing validation logic that would
 *            otherwise repeat across every action method.
 * SECTIONS IN THIS FILE:
 *   4. IActionFilter — ModelState validation, short-circuiting, TypeFilter usage
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Filters.Filters;

/*
 * SECTION 4: MODELSTATE VALIDATION — IActionFilter SHORT-CIRCUIT
 * ─────────────────────────────────────────────────────────────────────────────
 * ModelState.IsValid is false when:
 *   • Required properties are absent from the request body
 *   • Values fail data annotations ([Range], [StringLength], [EmailAddress], …)
 *   • IValidatableObject.Validate() returns errors
 *   • The request body cannot be deserialized (missing required JSON fields)
 *
 * WITHOUT a filter, every action must repeat:
 *   if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
 *
 * WITH ValidateModelFilter that check lives in one class and is applied
 * declaratively at controller or action scope via [TypeFilter(typeof(ValidateModelFilter))].
 *
 * IActionFilter — SYNC VARIANT:
 *   void OnActionExecuting(ActionExecutingContext context)
 *   void OnActionExecuted(ActionExecutedContext context)
 *
 * Use the sync variant when the filter does NO async I/O.  Adding async overhead
 * (Task, await) for purely in-memory ModelState checks is unnecessary.
 *
 * SHORT-CIRCUITING via OnActionExecuting:
 *   Setting context.Result in OnActionExecuting skips the action method.
 *   ASP.NET Core still runs OnActionExecuted for filters that already entered
 *   their Before phase, and it still runs the Result-stage filters for the
 *   result assigned to context.Result.
 *
 * ValidationProblemDetails:
 *   Wraps ModelState errors in the RFC 7807 Problem Details format:
 *   {
 *     "type":   "https://tools.ietf.org/html/rfc7807",
 *     "title":  "One or more validation errors occurred.",
 *     "status": 422,
 *     "errors": {
 *       "Item": ["The Item field is required."]
 *     }
 *   }
 *
 * Why TypeFilter (not ServiceFilter)?
 *   ValidateModelFilter has NO constructor dependencies.
 *   TypeFilter avoids a DI registration that would clutter IServiceCollection.
 *   Apply:  [TypeFilter(typeof(ValidateModelFilter))]
 *
 *   If the filter later gains a dependency (ILogger), switch to ServiceFilter:
 *     builder.Services.AddScoped<ValidateModelFilter>();
 *     [ServiceFilter(typeof(ValidateModelFilter))]
 */
public sealed class ValidateModelFilter : IActionFilter
{
    /*
     * OnActionExecuting — fires BEFORE the action body runs.
     * This is the only place to short-circuit based on ModelState.
     */
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            // 422 Unprocessable Entity with RFC 7807 error body — short-circuit
            context.Result = new UnprocessableEntityObjectResult(
                new ValidationProblemDetails(context.ModelState)); // action will NOT run
        }
    }

    /*
     * OnActionExecuted — fires AFTER the action (or after short-circuit from
     * a deeper filter in the Before phase).
     * This filter has nothing to do in the After phase.
     */
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // no-op — validation concerns are fully handled in OnActionExecuting
    }
}
