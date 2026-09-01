/*
 * FILE ROLE: ProductsController — demonstrates applying MVC action filters at
 *   every scope: global (via Program.cs registration), controller level (via
 *   [ServiceFilter]), and action level (via [TypeFilter] and [PerformanceActionFilter]).
 *   Shows how filter scope affects execution order and how individual actions opt in
 *   to specific filters without affecting the whole controller.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 9  — Applying filters at all three scopes (global, controller, action),
 *                ServiceFilterAttribute, TypeFilterAttribute, attribute-based filters,
 *                filter execution order demonstration, [RequireApiKey] IFilterFactory
 */

using System.Collections.Generic;
using ActionFiltersMvc.Filters;
using ActionFiltersMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace ActionFiltersMvc.Controllers;

/*
 * SECTION 9: APPLYING FILTERS AT ALL THREE SCOPES
 * ─────────────────────────────────────────────────────────────────────────────
 * FILTER SCOPE RECAP:
 *   Global      → options.Filters.Add<T>() in Program.cs — applies to EVERY action
 *   Controller  → [ServiceFilter] / [TypeFilter] / IFilterFactory attr on the class
 *   Action      → [ServiceFilter] / [TypeFilter] / IFilterFactory attr on the method
 *
 * This controller illustrates all three by layering:
 *   • Global filters from Program.cs:
 *       MvcExceptionFilter  — catches every unhandled exception app-wide
 *       AuditActionFilter   — logs every action entry/exit app-wide
 *
 *   • Controller-level filter (applies to ALL actions in ProductsController):
 *       [ServiceFilter(typeof(ETagResourceFilter))]
 *         → ETagResourceFilter is registered as Scoped in DI (see Program.cs)
 *         → ServiceFilter resolves it from DI; lifetime matches DI registration
 *         → Adds ETag headers to all Products responses
 *
 *   • Action-level filters (apply to specific actions only):
 *       [TypeFilter(typeof(ValidateModelActionFilter))] on Create POST
 *         → TypeFilter does NOT require DI registration; framework resolves ctor deps
 *         → ValidateModelActionFilter has no ctor deps so TypeFilter is simple here
 *         → [TypeFilter(typeof(T), Arguments = new object[] { val })] passes ctor args
 *
 *       [PerformanceActionFilter] on Create POST and ThrowError
 *         → Direct attribute usage; PerformanceActionFilter : ActionFilterAttribute
 *         → No DI involved; instance is shared (state stored in HttpContext.Items)
 *
 * EXECUTION ORDER for Create POST (outermost to innermost — "before" phase):
 *   1. Global:     MvcExceptionFilter wraps everything (exception stage)
 *   2. Global:     AuditActionFilter.OnActionExecutionAsync (before await next)
 *   3. Controller: ETagResourceFilter.OnResourceExecuting
 *   4. Action:     ValidateModelActionFilter.OnActionExecuting
 *   5. Action:     PerformanceActionFilter.OnActionExecuting
 *      [ Action method Create(ProductViewModel) runs here ]
 *   6. Action:     PerformanceActionFilter.OnActionExecuted
 *   7. Action:     ValidateModelActionFilter.OnActionExecuted
 *   8. Action:     PerformanceActionFilter.OnResultExecuting
 *      [ ViewResult / RedirectResult executes here ]
 *   9. Action:     PerformanceActionFilter.OnResultExecuted
 *  10. Controller: ETagResourceFilter.OnResourceExecuted
 *  11. Global:     AuditActionFilter.OnActionExecutionAsync (after await next)
 *
 * NOTE: MvcExceptionFilter runs at the EXCEPTION stage — it is not in the execution
 *   order above unless an exception is thrown; then it fires between Action and Result.
 *
 * [RequireApiKey] (IFilterFactory) EXAMPLE:
 *   Uncomment [RequireApiKey] on any action to require a valid X-Api-Key header.
 *   It is commented out here so the browser demo works without configuring headers.
 *   See Filters/ApiKeyAuthorizationFilter.cs for the implementation details.
 */
[ServiceFilter(typeof(ETagResourceFilter))]  // controller-level: ETag on all Products responses
public class ProductsController : Controller
{
    // ── GET /Products/Index ──────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Index()
    {
        // Global AuditActionFilter logs this action automatically.
        // Global ETagResourceFilter (controller-level) may short-circuit with 304.
        IEnumerable<ProductViewModel> products = ProductViewModel.GetSamples(); // sample data
        return View(products);
    }

    // ── GET /Products/Create ─────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProductViewModel()); // empty form
    }

    // ── POST /Products/Create ────────────────────────────────────────────────

    [HttpPost]
    [TypeFilter(typeof(ValidateModelActionFilter))]  // action-level: validate ModelState before running
    [PerformanceActionFilter]                         // action-level: time the action + add X-Action-Ms header
    // [RequireApiKey]                               // uncomment to require X-Api-Key header (IFilterFactory demo)
    public IActionResult Create(ProductViewModel model)
    {
        // This code only executes if ValidateModelActionFilter allowed it through,
        // i.e., ModelState.IsValid == true.  Invalid models are short-circuited
        // back to the Create view by the filter BEFORE reaching here.
        TempData["Success"] = $"Product '{model.Name}' created successfully."; // survives redirect
        return RedirectToAction(nameof(Index));                                 // PRG pattern
    }

    // ── GET /Products/ThrowError ─────────────────────────────────────────────

    [HttpGet]
    [PerformanceActionFilter]                         // times the action even though it throws
    public IActionResult ThrowError()
    {
        // Deliberately throws to demonstrate MvcExceptionFilter (registered globally).
        // Navigate to /Products/ThrowError in the browser to see the Error.cshtml view.
        // Access with Accept: application/json to see the ProblemDetails JSON response.
        throw new System.InvalidOperationException(
            "This exception is deliberate — demonstrating MvcExceptionFilter.");
    }
}
