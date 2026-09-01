/*
 * FILE ROLE: Demonstrates a minimal MVC controller — the "C" in MVC.
 *   Shows how a controller class is declared, how an action method is structured,
 *   how a ViewModel is constructed and passed to the view, and what return types
 *   are typically used. This is the first file to open after Program.cs.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 12: Controller class declaration, inheritance, and action method
 *               (sub-sections: 12a class / naming, 12b action method / return)
 */

using IntroductionToMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntroductionToMvc.Controllers;

/*
 * SECTION 12: CONTROLLER CLASS — NAMING, INHERITANCE, AND ACTION METHOD
 * ─────────────────────────────────────────────────────────────────────────────
 * --- 12a. CLASS DECLARATION AND INHERITANCE ---
 *
 * A controller is a plain C# class that satisfies the discovery rules described
 * in SECTION 8 (Program.cs). Most MVC controllers inherit from Controller, which
 * provides factory methods for building IActionResult responses.
 *
 * INHERITING Controller vs ControllerBase:
 *
 *   Base class        View-related members   Use when
 *   ────────────────  ─────────────────────  ─────────────────────────────────────
 *   Controller        Yes: View(), ViewBag,  MVC — the action returns HTML views
 *                     ViewData, TempData
 *   ControllerBase    None                   Web API — the action returns JSON only
 *
 * KEY MEMBERS PROVIDED BY Controller (beyond ControllerBase):
 *   View(model)                       returns ViewResult (renders .cshtml)
 *   PartialView(viewName, model)       returns PartialViewResult (no layout)
 *   ViewBag                            dynamic bag for view data (see ch. 12)
 *   ViewData                           ViewDataDictionary for layout data
 *   TempData                           persists data across a redirect (see ch. 12)
 *
 * KEY MEMBERS PROVIDED BY ControllerBase (available in both):
 *   Json(data)                         returns JsonResult (200 + JSON)
 *   Redirect(url)                      returns RedirectResult (302)
 *   RedirectToAction(action)           returns RedirectToActionResult
 *   NotFound()                         returns NotFoundResult (404)
 *   BadRequest()                       returns BadRequestResult (400)
 *   Unauthorized()                     returns UnauthorizedResult (401)
 *   Ok(data)                           returns OkObjectResult (200 + JSON)
 *   HttpContext                        access to request, response, user, connection
 *   Request                            HttpRequest (headers, body, query, form)
 *   Response                           HttpResponse (status, headers, cookies)
 *   RouteData                          parsed route values for the current request
 *   User                               ClaimsPrincipal (authenticated user identity)
 *
 * CONSTRUCTOR INJECTION:
 *   Services registered in DI are injected via the constructor. This controller
 *   has no injected dependencies to keep the demo minimal. In a real application:
 *     private readonly IProductService _products;
 *     public HomeController(IProductService products) { _products = products; }
 *   COVERED IN DETAIL LATER → 02. Controllers & Actions
 */
public class HomeController : Controller
{
    /*
     * --- 12b. ACTION METHOD: Index ---
     *
     * An action method is any public method on a controller class (unless
     * decorated with [NonAction]). The framework calls it after routing and
     * model binding complete for the matched request.
     *
     * NAMING → URL mapping (via the default convention route):
     *   HomeController.Index()    ←→   /  (or /Home/Index)
     *   HomeController.Details()  ←→   /Home/Details
     *   ProductController.List()  ←→   /Product/List
     *
     * RETURN TYPE — IActionResult:
     *   IActionResult is the common return type — the action decides at runtime
     *   which concrete result to return (View, Json, Redirect, NotFound, etc.).
     *   Using the interface as the declared type avoids coupling the method
     *   signature to one specific HTTP response format.
     *
     *   Alternatives:
     *     ActionResult<T>     compile-time typed JSON responses (preferred in Web API)
     *     ViewResult          concrete type (too specific — prevents switching to Redirect)
     *     Task<IActionResult> async action (use when calling async services)
     *
     * BUILDING THE VIEWMODEL:
     *   The controller is responsible for constructing the ViewModel — querying a
     *   service, mapping entity fields to ViewModel properties, and passing the
     *   finished ViewModel to View().
     *
     *   In production: var products = await _products.GetFeaturedAsync();
     *   Here: hardcoded to demonstrate the wiring without a database dependency.
     *
     * View() OVERLOADS:
     *   return View()              — uses convention: Views/Home/Index.cshtml
     *   return View(model)         — same convention path + typed model for template
     *   return View("Other")       — explicit view name: Views/Home/Other.cshtml
     *   return View("Other", model) — explicit name + model
     *   return View("~/Views/Shared/Custom.cshtml", model) — absolute project path
     *
     * COVERED IN DETAIL LATER → 02. Controllers & Actions
     */
    public IActionResult Index()
    {
        // Build a sample ViewModel; in production this comes from an injected service
        var model = new ProductViewModel
        {
            Id = 1,                 // product identifier
            Name = "Widget Pro",    // display name for the page heading
            Price = 29.99m,         // decimal literal — m suffix required for decimal type
            Category = "Tools",     // category label rendered in the view
            IsAvailable = true      // availability flag for conditional markup in the view
        };

        return View(model); // render Views/Home/Index.cshtml with the ProductViewModel
    }
}
