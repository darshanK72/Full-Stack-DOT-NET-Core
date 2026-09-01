/*
 * FILE ROLE: Conventional routing target — demonstrates how the default route
 *            pattern maps URLs to this controller, how to read route data inside
 *            an action, and how to generate URLs programmatically with IUrlHelper,
 *            Url.Action, Url.RouteUrl, and RedirectToAction.
 * SECTIONS IN THIS FILE:
 *    9. Conventional Routing Target
 *   10. Accessing Route Data Inside an Action
 *   11. URL Generation in Controllers (Url.Action, Url.RouteUrl, RedirectToAction, IUrlHelper)
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RoutingAttributeRouting.Models;
using System.Collections.Generic;

namespace RoutingAttributeRouting.Controllers;

/*
 * SECTION 9: CONVENTIONAL ROUTING TARGET
 * ─────────────────────────────────────────────────────────────────────────────
 * HomeController is the DEFAULT target of the conventional route:
 *
 *   pattern:  {controller=Home}/{action=Index}/{id?}
 *
 * "controller=Home" means: if the {controller} segment is absent from the URL,
 * use "Home" as the default → GET / resolves to HomeController.
 * "action=Index" means: if {action} is absent, default to Index → GET / → Index().
 * "{id?}" means: id is optional → GET /home/index and GET /home/index/42 both work.
 *
 * URL-to-action resolution for conventional routing:
 *
 *   URL                  controller  action     id
 *   ─────────────────    ──────────  ─────────  ────
 *   /                    Home        Index      (null)
 *   /home                Home        Index      (null)
 *   /home/index          Home        Index      (null)
 *   /home/about          Home        About      (null)
 *   /home/about/7        Home        About      7
 *
 * NOTE: HomeController has NO [Route] attribute.  Controllers without [Route] at
 * the class level participate ONLY in conventional routing — they are never
 * matched by an attribute route template.
 *
 * CONTRAST: ProductsController has [Route("[controller]")] and participates ONLY
 * in attribute routing.  It is never matched by the conventional route patterns
 * registered in Program.cs.
 */
public class HomeController : Controller
{
    /*
     * SECTION 10: ACCESSING ROUTE DATA INSIDE AN ACTION
     * ─────────────────────────────────────────────────────────────────────────────
     * The routing middleware populates RouteData BEFORE the action executes.
     * The controller base class exposes it through three overlapping APIs:
     *
     *   API                                  Type                   Use
     *   ────────────────────────────────     ───────────────────    ─────────────────────────────────
     *   RouteData.Values["key"]              object?                raw segment values from the URL
     *   ControllerContext.RouteData          RouteData              same as RouteData; context alias
     *   HttpContext.GetRouteValue("key")     object?                extension method; same values
     *   HttpContext.GetRouteData()           RouteData?             full RouteData via HttpContext
     *
     * RouteData.Values keys always include "controller" and "action".
     * Optional segments (like {id?}) are present only when the URL contained them.
     *
     * RouteData.DataTokens:
     *   Extra metadata attached to a route at registration time, not extracted from the URL.
     *   Example: area name is stored in DataTokens["area"] for area-specific routes.
     *
     * This action populates RouteDebugViewModel so Index.cshtml can display what
     * the routing system resolved, making the route match visible in the browser.
     */
    public IActionResult Index()
    {
        // Collect all route values into a typed dictionary for the view
        var routeValues = new Dictionary<string, string?>();
        foreach (KeyValuePair<string, object?> kvp in RouteData.Values)
        {
            routeValues[kvp.Key] = kvp.Value?.ToString(); // convert object? → string?
        }

        var model = new RouteDebugViewModel
        {
            Controller = RouteData.Values["controller"]?.ToString() ?? string.Empty,
            Action     = RouteData.Values["action"]?.ToString()     ?? string.Empty,
            Area       = RouteData.Values["area"]?.ToString(),           // null if not in an Area
            MatchedTemplate = "{controller=Home}/{action=Index}/{id?}",  // conventional route pattern
            RouteValues = routeValues
        };

        // URL generation data — populated here so Index.cshtml can display generated URLs
        // alongside the tag-helper links that do the same thing declaratively.
        ViewData["UrlToProducts"]    = Url.Action("Index", "Products");                    // /products
        ViewData["UrlToProductById"] = Url.Action("Details", "Products", new { id = 5 }); // /products/5
        ViewData["UrlByRouteName"]   = Url.RouteUrl("product-detail", new { id = 10 });   // /products/10

        return View(model); // renders Views/Home/Index.cshtml with RouteDebugViewModel
    }

    /*
     * SECTION 11: URL GENERATION IN CONTROLLERS
     * ─────────────────────────────────────────────────────────────────────────────
     * ASP.NET Core provides multiple ways to generate URLs from within a controller.
     * All of them go through the same underlying link-generation engine — they are
     * different API surfaces over the same IUrlHelper (or LinkGenerator in .NET 5+).
     *
     * --- 11a. IUrlHelper — the Url property ---
     *
     *   IUrlHelper is exposed by Controller.Url.  It generates relative URLs by
     *   default; pass protocol + host to get an absolute URL.
     *
     *   Url.Action(action)                          → same controller, given action
     *   Url.Action(action, controller)              → cross-controller
     *   Url.Action(action, controller, routeValues) → with extra segments / query string
     *   Url.Action(action, controller, values, protocol, host)  → absolute URL
     *
     *   Url.RouteUrl(routeName)                     → by route name (see SECTION 16)
     *   Url.RouteUrl(routeName, routeValues)        → route name + values
     *   Url.RouteUrl(routeName, values, protocol, host) → absolute
     *
     * --- 11b. RedirectToAction — redirect helpers ---
     *
     *   RedirectToAction(action)                    → 302 to same controller, given action
     *   RedirectToAction(action, controller)        → 302 cross-controller
     *   RedirectToAction(action, routeValues)       → 302 with route values
     *   RedirectToActionPermanent(action)           → 301 permanent
     *   RedirectToRoute(routeName, routeValues)     → 302 by route name
     *   RedirectToRoutePermanent(routeName, values) → 301 by route name
     *
     * --- 11c. CreatedAtAction — used in POST actions ---
     *
     *   Returns HTTP 201 Created with a Location header pointing to the new resource.
     *   Used most often in Web API actions (see ProductsController SECTION 17).
     *
     *   return CreatedAtAction(nameof(Details), new { id = newId }, createdObject);
     *
     * --- 11d. Conventional route URL generation rules ---
     *
     *   When generating via Url.Action("About", "Home"), MVC:
     *     1. Finds the route template(s) that can produce a URL matching those values
     *     2. Fills in required segments; adds extra values as query-string parameters
     *     3. Returns the shortest matching URL (ambient route values are reused)
     *
     *   AMBIENT ROUTE VALUES: The current request's route values are "ambient".
     *   Url.Action("About") with no controller reuses the current controller.
     *   Url.Action("Details", "Products") sets controller explicitly, discarding ambient.
     *
     * --- 11e. IUrlHelper vs LinkGenerator ---
     *
     *   IUrlHelper       — bound to the current HttpContext; available in controllers & views
     *   LinkGenerator    — injected service; can generate URLs outside HTTP context
     *                      (background jobs, email templates, etc.)
     *
     *   COVERED IN DETAIL LATER → LinkGenerator is in 10. Areas (dependency injection context)
     */
    public IActionResult About()
    {
        // RedirectToAction: 302 redirect — demonstrates the simplest form of URL generation
        // The framework resolves "Index" → HomeController.Index via the conventional route.
        return RedirectToAction(nameof(Index)); // nameof avoids magic string — refactor-safe
    }

    public IActionResult UrlDemo()
    {
        // IUrlHelper — Url IS IUrlHelper; the Controller property is an alias
        IUrlHelper helper = Url;   // demonstrates the interface type explicitly

        // Url.Action — relative URLs (string? because the route might not exist)
        string? toHome          = helper.Action("Index", "Home");                               // "/"
        string? toProducts      = helper.Action("Index", "Products");                           // "/products"
        string? toProductById   = helper.Action("Details", "Products", new { id = 7 });        // "/products/7"
        string? toSearch        = helper.Action("Search", "Products", new { name = "laptop" }); // "/products/search/laptop"

        // Url.RouteUrl — by named route (route name set via Name = "..." on the attribute or MapControllerRoute)
        string? byRouteName     = helper.RouteUrl("product-detail", new { id = 42 }); // "/products/42"
        string? byConvRouteName = helper.RouteUrl("default", new { controller = "Blog", action = "Index" });

        // Store in ViewData for the view to render (no dedicated UrlDemo.cshtml — we redirect)
        ViewData["UrlDemo_Home"]          = toHome;
        ViewData["UrlDemo_Products"]      = toProducts;
        ViewData["UrlDemo_Product7"]      = toProductById;
        ViewData["UrlDemo_Search"]        = toSearch;
        ViewData["UrlDemo_ByRouteName"]   = byRouteName;
        ViewData["UrlDemo_ConvRoute"]     = byConvRouteName;

        // RedirectToRoute — generate URL by route name and redirect
        // Uncomment to see redirect in action:
        // return RedirectToRoute("product-detail", new { id = 1 });

        return RedirectToAction(nameof(Index)); // redirect back to Index to display ViewData
    }
}
