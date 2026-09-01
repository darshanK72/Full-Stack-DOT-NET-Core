/*
 * FILE ROLE: Demonstrates IActionFilter (synchronous) — the most common filter type
 *   used for ModelState validation.  ValidateModelActionFilter checks ModelState in
 *   OnActionExecuting and short-circuits the pipeline with a ViewResult (for MVC
 *   controllers) or BadRequestObjectResult (for API controllers) before the action
 *   method runs, preventing invalid data from reaching business logic.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 4  — IActionFilter: OnActionExecuting, OnActionExecuted,
 *                ActionExecutingContext short-circuit, ViewResult vs BadRequestObjectResult,
 *                detecting MVC vs API controller context
 */

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFiltersMvc.Filters;

/*
 * SECTION 4: IActionFilter — SYNCHRONOUS ACTION FILTER WITH SHORT-CIRCUIT
 * ─────────────────────────────────────────────────────────────────────────────
 * IActionFilter is the synchronous interface for the action filter stage.
 * It splits the before/after logic into two separate methods:
 *
 *   OnActionExecuting(ActionExecutingContext context)
 *     → Called BEFORE the action method runs.
 *     → Set context.Result to short-circuit and skip the action.
 *     → Action arguments are already bound and available via context.ActionArguments.
 *
 *   OnActionExecuted(ActionExecutedContext context)
 *     → Called AFTER the action method has returned (or short-circuited upstream).
 *     → Can inspect or replace context.Result.
 *     → context.Canceled == true if a previous filter short-circuited this action.
 *
 * SHORT-CIRCUIT MECHANICS:
 *   Setting context.Result in OnActionExecuting stops the pipeline at this filter.
 *   The ACTION METHOD does not run.  However, Result filters (IResultFilter) STILL
 *   run for the short-circuit result — this allows result filters to post-process
 *   any IActionResult, including short-circuit ones.
 *
 * MVC vs API CONTEXT:
 *   In a mixed app with both MVC (inheriting Controller) and Web API (inheriting
 *   ControllerBase only) controllers, a shared validation filter must detect which
 *   type of controller is handling the request:
 *
 *   context.Controller is Controller mvcCtrl   → true for MVC controllers (have Views)
 *   context.Controller is ControllerBase       → true for both MVC and API controllers
 *
 *   For MVC: return a ViewResult to re-display the form with validation errors.
 *   For API: return BadRequestObjectResult with the ModelState errors as JSON.
 *
 * ModelState vs Data Annotations:
 *   ModelState accumulates errors from:
 *     · Data annotation attributes ([Required], [StringLength], [Range]) on the model
 *     · Manual additions via ModelState.AddModelError() in the action
 *     · Complex type binding failures (e.g., non-numeric value for an int property)
 *   ModelState.IsValid == false means at least one error exists.
 *   Errors are keyed by property name, e.g., ModelState["Name"].Errors[0].ErrorMessage.
 *
 * ActionExecutingContext KEY PROPERTIES used here:
 *   .ModelState           — binding + validation errors accumulated before this filter
 *   .Controller           — the controller instance (cast to Controller for MVC helpers)
 *   .ActionArguments      — IDictionary<string, object?> of bound action parameters
 *   .RouteData.Values     — route values dictionary {controller, action, id, ...}
 *   .Result               — set to IActionResult to short-circuit
 *
 * REGISTRATION:
 *   Applied via [TypeFilter(typeof(ValidateModelActionFilter))] on individual actions.
 *   TypeFilter does NOT require this class to be registered in DI — the framework
 *   instantiates it via constructor injection from the request DI scope automatically.
 *   This filter has no constructor dependencies so TypeFilter is straightforward here.
 */
public sealed class ValidateModelActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return; // model is valid — let the action proceed normally
        }

        // Determine whether this is an MVC controller (has access to View helpers)
        // or an API controller (only ControllerBase, no Razor view engine)
        if (context.Controller is Controller mvcController)
        {
            // ── MVC path: re-display the form view with ModelState errors ──────
            // Determine the view name from the current route action value so the
            // correct form template is shown (e.g., "Create" for POST /Products/Create)
            string viewName = context.RouteData.Values["action"] as string ?? "Create";

            // Extract the first action argument (the view model) from the bound params.
            // The view model must be passed back so form fields retain their typed values.
            object? model = context.ActionArguments.Values.FirstOrDefault();

            // Short-circuit: set Result; action method will NOT run.
            // mvcController.View() returns a ViewResult using the controller's ViewData,
            // which already contains the accumulated ModelState errors — asp-validation-for
            // tag helpers and @Html.ValidationMessageFor will display them automatically.
            context.Result = mvcController.View(viewName, model);
        }
        else
        {
            // ── API path: return 400 Bad Request with validation errors as JSON ─
            // ControllerBase.ModelState is the same error source; serialised as RFC 7807
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // OnActionExecuted runs AFTER the action method (or after short-circuit).
        // context.Canceled == true when OnActionExecuting set a Result and short-circuited.
        // Nothing to do here — validation is purely a pre-action concern.
        // Override this method to inspect/mutate the Result AFTER the action runs,
        // e.g., to add response headers or log the result type.
    }
}
