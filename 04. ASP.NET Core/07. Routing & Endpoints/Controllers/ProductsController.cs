/*
 * FILE ROLE: Demonstrates attribute routing on an ASP.NET Core MVC controller —
 *            [Route] on the class, [HttpGet/Post/Put/Delete] on actions,
 *            inline route constraints, and HttpContext.GetRouteData.
 * SECTIONS IN THIS FILE:
 *   2.  [Route] — controller-level attribute routing
 *   2a. [HttpGet] / [HttpPost] / [HttpPut] / [HttpDelete] on actions
 *   2b. Inline route constraints in attribute routes ({id:int}, {category:alpha}, {**slug})
 *   2c. HttpContext.GetRouteData
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RoutingEndpoints.Models;
using System;

namespace RoutingEndpoints.Controllers;

/*
 * SECTION 2: [Route] — CONTROLLER-LEVEL ATTRIBUTE ROUTING
 *
 * Attribute routing maps HTTP requests directly to controller actions via
 * attributes rather than a central MapControllerRoute pattern (conventional routing).
 *
 * [ApiController] activates:
 *   • Automatic HTTP 400 ProblemDetails response when model binding fails
 *   • Binding source inference: complex types → [FromBody], simple route tokens → [FromRoute]
 *   • Problem-details error formatting (RFC 7807)
 *
 * [Route("api/[controller]")] on the class:
 *   • Sets the shared URL prefix for ALL actions in this controller.
 *   • [controller] token expands to the class name minus the "Controller" suffix,
 *     lowercased at runtime:  ProductsController → "products"
 *   • Every action below is therefore under /api/products/...
 *
 * Token substitution reference:
 *   [controller]  → controller name without suffix      (ProductsController → products)
 *   [action]      → method name                         (rarely used in REST APIs)
 *   [area]        → MVC area name                       (for area-based routing)
 *
 * Attribute routing vs conventional routing:
 *   Attribute     — explicit, per-action, preferred for REST APIs
 *   Conventional  — central MapControllerRoute pattern, preferred for page-based MVC
 *   Both can coexist (see Section 14 in Program.cs); attribute routes take precedence.
 */
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // In-memory sample data — satisfies return types without requiring a real database.
    private static readonly Product[] _products = new[]
    {
        new Product { Id = 1, Name = "Widget",    Price = 9.99m,   Category = "Gadgets" },
        new Product { Id = 2, Name = "Gizmo",     Price = 19.99m,  Category = "Gadgets" },
        new Product { Id = 3, Name = "Doohickey", Price = 4.99m,   Category = "Tools"   },
    };

    /*
     * --- 2a. [HttpGet] / [HttpPost] / [HttpPut] / [HttpDelete] ON ACTIONS ---
     *
     * Each HTTP-method attribute restricts the action to one verb.  The string
     * argument is a RELATIVE template appended to the class-level [Route] prefix.
     *
     *   [HttpGet]                →  GET    /api/products
     *   [HttpGet("{id:int}")]    →  GET    /api/products/{id}  (id must be int)
     *   [HttpPost]               →  POST   /api/products
     *   [HttpPut("{id:int}")]    →  PUT    /api/products/{id}
     *   [HttpDelete("{id:int}")] →  DELETE /api/products/{id}
     *
     * Return types:
     *   IActionResult       — flexible; wraps Ok(), CreatedAtAction(), NoContent(), etc.
     *   ActionResult<T>     — typed variant; supports both T and IActionResult returns.
     *
     * [ApiController] binding-source inference:
     *   Simple type parameter whose name matches a route token → bound from route values.
     *   Complex type parameter not in the route                → bound from request body.
     *   Explicit [FromBody] / [FromQuery] override inference.
     */

    // GET /api/products — return all products
    [HttpGet]
    public IActionResult GetAll() => Ok(_products); // 200 OK with JSON array

    // GET /api/products/1 — single product; :int constraint rejects non-integer segments
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) // [ApiController] infers [FromRoute] for id
    {
        Product? product = Array.Find(_products, p => p.Id == id); // null when not found
        return product is null ? NotFound() : Ok(product);         // 404 or 200
    }

    // POST /api/products — create; JSON body bound to product via [FromBody]
    [HttpPost]
    public IActionResult Create([FromBody] Product product)
    {
        // CreatedAtAction returns HTTP 201 Created with a Location header.
        // routeValues must match GetById's template token: { id = product.Id }
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT /api/products/1 — full update
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Product product)
    {
        bool found = Array.Exists(_products, p => p.Id == id);
        // In production: replace the stored record at id with the incoming product.
        // Body binding mechanics are detailed in → 08. Model Binding & Validation.
        return found && product.Price >= 0 ? NoContent() : NotFound(); // 204 or 404
    }

    // DELETE /api/products/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        bool found = Array.Exists(_products, p => p.Id == id);
        return found ? NoContent() : NotFound(); // 204 or 404
    }

    /*
     * --- 2b. INLINE ROUTE CONSTRAINTS IN ATTRIBUTE ROUTES ---
     *
     * Constraints narrow route matching to values that satisfy a predicate.
     * Syntax:  {parameter:constraint}   or chained:  {parameter:int:min(1)}
     *
     * Common constraints:
     *   :int          32-bit integer            /products/42 ✓   /products/abc ✗
     *   :alpha        ASCII alphabetic only     /category/tools ✓  /category/42 ✗
     *   :guid         standard GUID format
     *   :min(n)       integer >= n
     *   :range(n,m)   integer in [n, m]
     *   :regex(pat)   matches regular expression ({{ }} escapes braces inside pattern)
     *   {**slug}      catch-all — captures rest of path including slashes
     *
     * Key design point — constraints affect ROUTE MATCHING (404 on failure), not validation:
     *   A failing constraint returns 404 "no matching route".
     *   Validation attributes ([Required], [Range]) return 400 "bad request".
     *   Do not use constraints as a substitute for business-rule validation.
     */

    // GET /api/products/category/gadgets — :alpha rejects paths like /category/42
    [HttpGet("category/{category:alpha}")]
    public IActionResult GetByCategory(string category)
    {
        Product[] results = Array.FindAll(_products, p =>
            string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
        return Ok(results);
    }

    // GET /api/products/search/tools/handheld/drills
    // {**slug} captures everything after /search/ including slashes
    // slug == "tools/handheld/drills"   (the full remaining path)
    [HttpGet("search/{**slug}")]
    public IActionResult Search(string slug)
        => Ok(new { Query = slug, Results = Array.Empty<Product>() });

    /*
     * --- 2c. HttpContext.GetRouteData ---
     *
     * GetRouteData() is an extension method on HttpContext (Microsoft.AspNetCore.Routing).
     * It returns a RouteData object containing:
     *
     *   routeData.Values     — RouteValueDictionary of parsed route tokens
     *                          e.g. { "controller": "products", "action": "GetRouteInfo" }
     *   routeData.DataTokens — extra metadata attached at registration time
     *   routeData.Routers    — matched IRouter chain (typically empty in endpoint routing)
     *
     * ControllerBase already exposes RouteData as an inherited property — it is equivalent
     * to HttpContext.GetRouteData().  The extension method form is useful in middleware and
     * non-controller code where ControllerBase is not available.
     *
     * Common use cases:
     *   • Middleware that reads the matched controller/action for logging or feature-flagging
     *   • Action filters that inspect route tokens at execution time
     *   • Link generation helpers that need the current request's route values
     */

    // GET /api/products/routeinfo — reads and returns the current request's route values
    [HttpGet("routeinfo")]
    public IActionResult GetRouteInfo()
    {
        RouteData routeData = HttpContext.GetRouteData();                         // extension method
        string controller = routeData.Values["controller"]?.ToString() ?? "n/a"; // nullable object → string
        string action     = routeData.Values["action"]?.ToString()     ?? "n/a";
        return Ok(new { Controller = controller, Action = action, AllValues = routeData.Values });
    }
}
