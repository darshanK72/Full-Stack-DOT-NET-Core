/*
 * FILE ROLE: Full attribute-routing demonstration — [Route] on the controller,
 *            HTTP-verb attributes, route tokens, constraints, named routes,
 *            and programmatic URL generation with CreatedAtAction and Url.RouteUrl.
 * SECTIONS IN THIS FILE:
 *   12. Attribute Routing on Controller — [Route] prefix
 *   13. HTTP Verb Attributes ([HttpGet], [HttpPost], [HttpPut], [HttpDelete], [HttpPatch])
 *   14. Route Tokens ([controller], [action], [area])
 *   15. Route Constraints in Attribute Routing
 *   16. Route Names
 *   17. CreatedAtAction and Url.RouteUrl for URL Generation
 */

using Microsoft.AspNetCore.Mvc;
using RoutingAttributeRouting.Models;
using System.Collections.Generic;

namespace RoutingAttributeRouting.Controllers;

/*
 * SECTION 12: ATTRIBUTE ROUTING ON CONTROLLER — [Route] PREFIX
 * ─────────────────────────────────────────────────────────────────────────────
 * When [Route] is applied at the CLASS level it defines a route PREFIX that is
 * prepended to every action-level route in the controller.
 *
 *   [Route("[controller]")]          // prefix = "products"  (token resolved below)
 *   public class ProductsController  // "products" because [controller] = class name - "Controller"
 *
 * CRITICAL RULE: A controller with a class-level [Route] is COMPLETELY OUTSIDE
 * conventional routing.  None of the MapControllerRoute patterns in Program.cs
 * will ever match its actions.  Attribute routing and conventional routing are
 * MUTUALLY EXCLUSIVE per action — an action is either attribute-routed or
 * conventional-routed, never both.
 *
 *   Controller / Action               Routing mode
 *   ──────────────────────────────    ─────────────────
 *   HomeController (no [Route])       Conventional only
 *   ProductsController ([Route] ✓)    Attribute only
 *   BlogController (mixed)            See BlogController SECTION 18
 *
 * COMBINING CONTROLLER AND ACTION ROUTES:
 *   Controller [Route("products")]  +  Action [HttpGet("{id}")]  =  "products/{id}"
 *   Controller [Route("v1/[controller]")] is also valid for versioned APIs.
 *
 * Multiple [Route] attributes on a CONTROLLER define multiple prefixes — every
 * action template is combined with each prefix, creating N × M route entries.
 *
 * Multiple [Route] attributes on an ACTION define multiple templates for that
 * single action — useful for legacy URL support or canonical/alias routes.
 */
[Route("[controller]")] // prefix resolves to "products"; combined with action templates below
public class ProductsController : Controller
{
    /*
     * SECTION 13: HTTP VERB ATTRIBUTES
     * ─────────────────────────────────────────────────────────────────────────────
     * HTTP verb attributes serve two purposes simultaneously:
     *   1. Constrain the route to a specific HTTP method (GET, POST, etc.)
     *   2. Optionally extend the controller prefix with an additional template segment
     *
     *   Attribute              Equivalent constraint     Optional template arg
     *   ─────────────────────  ───────────────────────   ──────────────────────────────
     *   [HttpGet]              HTTP GET only             none → uses controller prefix alone
     *   [HttpGet("{id}")]      HTTP GET only             prefix + "/{id}"
     *   [HttpPost]             HTTP POST only            none → uses controller prefix alone
     *   [HttpPost("create")]   HTTP POST only            prefix + "/create"
     *   [HttpPut("{id}")]      HTTP PUT only             prefix + "/{id}"
     *   [HttpDelete("{id}")]   HTTP DELETE only          prefix + "/{id}"
     *   [HttpPatch("{id}")]    HTTP PATCH only           prefix + "/{id}"
     *
     * [Route] without an HTTP verb attribute accepts ALL HTTP methods.
     * Use verb attributes on actions when the method matters (CRUD APIs, forms).
     *
     * OVERLOADED ROUTES — same URL, different method:
     *   [HttpGet]   public IActionResult Create()         → GET  /products/create (show form)
     *   [HttpPost]  public IActionResult Store(...)       → POST /products        (submit form)
     *   Both share the same prefix "products" but differ only by HTTP verb.
     *
     * 405 METHOD NOT ALLOWED:
     *   If a URL matches a route template but the HTTP method does not match,
     *   the framework returns 405 (not 404).  This is a routing feature, not a bug.
     */

    // GET /products  →  list all products
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["GeneratedUrlById"]   = Url.RouteUrl("product-detail", new { id = 1 }); // /products/1
        ViewData["GeneratedUrlSearch"] = Url.Action("Search", "Products", new { name = "laptop" });
        return View(); // renders Views/Products/Index.cshtml
    }

    /*
     * SECTION 14: ROUTE TOKENS — [controller], [action], [area]
     * ─────────────────────────────────────────────────────────────────────────────
     * Route tokens are bracketed placeholders INSIDE a route template that the
     * framework resolves at application startup (not at request time):
     *
     *   Token          Resolves to
     *   ─────────────  ──────────────────────────────────────────────────────────
     *   [controller]   Controller class name minus the "Controller" suffix, lower-cased
     *                  ProductsController → "products"
     *   [action]       Action method name, lower-cased
     *                  Details → "details"
     *   [area]         Area name (empty string outside of an area)
     *
     * WHERE TOKENS CAN APPEAR:
     *   • Controller-level [Route] template  — most common; defines the URL prefix
     *   • Action-level [Route] or verb attribute template  — can use [action] here
     *   • Both levels simultaneously
     *
     * EXAMPLE COMBINATIONS:
     *   [Route("[controller]/[action]")]  →  products/details, products/search, etc.
     *   [Route("[area]/[controller]/[action]")]  →  admin/products/edit (in Admin area)
     *
     * WHY USE TOKENS:
     *   • Renames automatically: rename ProductsController → ItemsController and
     *     all URL prefixes change from "products" to "items" without touching [Route].
     *   • Avoids magic strings in route templates.
     *
     * THIS CONTROLLER uses [Route("[controller]")] — the prefix "products" is derived
     * from the class name.  If the class were renamed, the URL prefix updates automatically.
     *
     * SECTION 15: ROUTE CONSTRAINTS IN ATTRIBUTE ROUTING
     * ─────────────────────────────────────────────────────────────────────────────
     * Constraints narrow WHICH URLs match a route template.  They are applied INLINE
     * with the colon syntax:  {parameter:constraint}  or  {parameter:c1:c2}  (chained).
     *
     * Common inline constraints:
     *
     *   Constraint              Syntax                   Matches
     *   ─────────────────────   ──────────────────────   ──────────────────────────────────
     *   int                     {id:int}                 integer (−2147483648 to 2147483647)
     *   long                    {id:long}                64-bit integer
     *   double                  {price:double}           floating-point number
     *   bool                    {flag:bool}              true or false
     *   guid                    {id:guid}                Globally Unique Identifier
     *   alpha                   {name:alpha}             letters only (a-z, A-Z)
     *   minlength(n)            {name:minlength(3)}      string with ≥ n characters
     *   maxlength(n)            {name:maxlength(50)}     string with ≤ n characters
     *   length(n)               {name:length(5)}         string with exactly n characters
     *   length(min,max)         {name:length(3,10)}      string length in [min, max]
     *   min(n)                  {age:min(18)}            integer ≥ n
     *   max(n)                  {age:max(120)}           integer ≤ n
     *   range(min,max)          {age:range(18,65)}       integer in [min, max]
     *   regex(expr)             {code:regex(^\\d{{5}}$)} matches regular expression
     *   required                {name:required}          non-empty string
     *
     * CHAINING:  {name:alpha:minlength(3)}  means: must be alpha AND at least 3 chars.
     *
     * CONSTRAINT VS VALIDATION:
     *   Constraints affect ROUTING (does this URL match?).
     *   DataAnnotations / ModelState affect VALIDATION (is the value valid?).
     *   Use constraints for type safety and disambiguation; use validation for business rules.
     *
     * GOTCHA — no match = 404, not 400:
     *   GET /products/abc when {id:int} is defined → 404 (route doesn't match).
     *   This can surprise users; prefer validation + friendly error messages for UX.
     */

    // GET /products/5  →  must be integer (constraint: :int)
    // Name = "product-detail" is the route name — see SECTION 16
    [HttpGet("{id:int}", Name = "product-detail")]
    public IActionResult Details(int id) // id is already guaranteed int by the constraint
    {
        ViewData["ProductId"] = id;
        return View(); // no Details.cshtml — exists to demonstrate routing
    }

    // GET /products/search/laptop  →  name must be letters-only AND ≥ 3 characters
    // Chained constraints:  :alpha  and  :minlength(3)
    [HttpGet("search/{name:alpha:minlength(3)}")]
    public IActionResult Search(string name) // arrives here only if name passes both constraints
    {
        ViewData["SearchTerm"] = name;
        return View(); // no Search.cshtml — exists to demonstrate routing
    }

    // GET /products/create  →  show create form
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(); // no Create.cshtml — exists to demonstrate routing
    }

    // POST /products  →  handle form submit (no extra segment — same prefix "products")
    [HttpPost]
    public IActionResult Store(string name, decimal price)
    {
        // In a real controller: validate, save to DB, then redirect
        return RedirectToAction(nameof(Index)); // POST-Redirect-GET pattern
    }

    // PUT /products/5  →  full replace; id must be integer
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, string name, decimal price)
    {
        return NoContent(); // 204 — typical REST response for successful PUT
    }

    // DELETE /products/5  →  remove resource; id must be integer
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return NoContent(); // 204 — typical REST response for successful DELETE
    }

    // PATCH /products/5  →  partial update; id must be integer
    [HttpPatch("{id:int}")]
    public IActionResult Patch(int id, string field, string value)
    {
        return NoContent(); // 204
    }

    /*
     * SECTION 16: ROUTE NAMES
     * ─────────────────────────────────────────────────────────────────────────────
     * Every route (conventional or attribute) can have an optional NAME.
     * Names are used for LINK GENERATION — they bypass route selection logic
     * and go directly to the named route to build the URL.
     *
     * Setting a route name:
     *   Attribute routing:    [HttpGet("{id:int}", Name = "product-detail")]
     *                         [Route("slug/{slug}", Name = "product-slug")]
     *   Conventional routing: app.MapControllerRoute(name: "blog-monthly", pattern: "...");
     *
     * Using a route name:
     *   In controllers:       Url.RouteUrl("product-detail", new { id = 5 })
     *   In Razor views:       @Url.RouteUrl("product-detail", new { id = 5 })
     *   Tag helpers:          <a asp-route="product-detail" asp-route-id="5">...</a>
     *   RedirectToRoute:      return RedirectToRoute("product-detail", new { id = 5 });
     *   CreatedAtRoute:       return CreatedAtRoute("product-detail", new { id = newId }, obj);
     *
     * UNIQUENESS REQUIREMENT:
     *   Route names must be UNIQUE across the application.  Duplicate names throw
     *   InvalidOperationException at startup.  By convention, use kebab-case with
     *   controller prefix:  "product-detail", "blog-monthly", "order-confirm".
     *
     * ROUTE NAME vs ROUTE TEMPLATE:
     *   Route names are OPAQUE labels for link generation; they have no effect on
     *   URL matching (which route is selected for an incoming request).
     *   URL matching is based on template pattern + constraints + HTTP verb only.
     */

    // This action already has Name = "product-detail" in SECTION 15 above.
    // The named route "product-by-slug" demonstrates a separate name on a different action.
    [HttpGet("slug/{slug:minlength(3)}", Name = "product-by-slug")]
    public IActionResult BySlug(string slug)
    {
        ViewData["Slug"] = slug;
        return View(); // no BySlug.cshtml — routing demo
    }

    /*
     * SECTION 17: CreatedAtAction AND Url.RouteUrl FOR URL GENERATION
     * ─────────────────────────────────────────────────────────────────────────────
     * --- 17a. CreatedAtAction ---
     *
     *   Used in POST actions that CREATE a new resource.  Returns HTTP 201 Created
     *   with a Location response header pointing to the newly created resource.
     *
     *   return CreatedAtAction(actionName, routeValues, value);
     *     actionName    — the action method that retrieves the created resource
     *     routeValues   — values to substitute into that action's route template
     *     value         — the response body (the created object as JSON)
     *
     *   The framework calls Url.Action(actionName, controller, routeValues) internally
     *   to build the Location URL.
     *
     *   Overloads:
     *     CreatedAtAction(actionName, value)
     *     CreatedAtAction(actionName, routeValues, value)
     *     CreatedAtAction(actionName, controllerName, routeValues, value)
     *
     * --- 17b. CreatedAtRoute ---
     *
     *   Same intent as CreatedAtAction but uses a ROUTE NAME instead of action/controller:
     *
     *   return CreatedAtRoute("product-detail", new { id = newId }, createdObject);
     *
     *   Prefer CreatedAtRoute when the target route has a stable name — more resilient
     *   to action method renames.
     *
     * --- 17c. Url.RouteUrl for link generation ---
     *
     *   Url.RouteUrl("route-name")                         → URL for named route, no extras
     *   Url.RouteUrl("route-name", routeValues)            → with route value substitution
     *   Url.RouteUrl("route-name", values, protocol)       → with protocol (http/https)
     *   Url.RouteUrl("route-name", values, protocol, host) → absolute URL
     *
     *   Compared to Url.Action, Url.RouteUrl is more explicit — it never uses
     *   ambient route values.  Use it when you need precise, predictable URL generation.
     *
     * --- 17d. Generating absolute URLs (protocol + host) ---
     *
     *   Url.Action("Index", "Home", null, Request.Scheme)
     *   Url.Action("Index", "Home", null, "https", "example.com")
     *
     *   Absolute URLs are needed for:
     *     • Email templates (links that must work outside the browser session)
     *     • API Location headers (technically can be relative, but absolute is standard)
     *     • Open-graph meta tags in HTML head
     */

    // POST /products/api  →  simulate API create; returns 201 + Location header
    [HttpPost("api")]
    public IActionResult ApiCreate(string name, decimal price)
    {
        // Simulate assigning a new ID after a DB insert
        var newId = 99;

        // CreatedAtAction: Location header → GET /products/99 (the Details action)
        return CreatedAtAction(
            actionName: nameof(Details),                  // target action for Location URL
            routeValues: new { id = newId },             // fills {id:int} in the Details route
            value: new { Id = newId, Name = name, Price = price }); // JSON response body
    }

    // POST /products/named-api  →  same concept using CreatedAtRoute with route name
    [HttpPost("named-api")]
    public IActionResult ApiCreateNamed(string name, decimal price)
    {
        var newId = 100;

        // CreatedAtRoute: uses the "product-detail" route name set in SECTION 15
        return CreatedAtRoute(
            routeName: "product-detail",                            // the named route
            routeValues: new { id = newId },                       // value for {id:int}
            value: new { Id = newId, Name = name, Price = price }); // JSON body
    }
}
