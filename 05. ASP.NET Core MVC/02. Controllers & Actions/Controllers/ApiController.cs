/*
 * FILE ROLE: Demonstrates ControllerBase vs Controller (deep dive), [ApiController]
 *            behaviours, [Controller] / [NonController] on concrete helper classes,
 *            and the full ObjectResult family — Ok<T>, BadRequest, NotFound,
 *            Created, and NoContent — for JSON API responses.
 *
 * SECTIONS IN THIS FILE:
 *   15. ControllerBase vs Controller — deep dive and [ApiController] attribute
 *   16. [NonController] on a helper class — opt-out of routing
 *   17. ObjectResult family — typed result helpers for API responses
 */

using ControllersActions.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ControllersActions.Controllers;

/*
 * SECTION 15: ControllerBase VS Controller — DEEP DIVE
 * ─────────────────────────────────────────────────────────────────────────────
 * ControllerBase is the root class for all ASP.NET Core controller types.
 * Controller extends it specifically for MVC / Razor view scenarios.
 *
 * ControllerBase provides:
 *   ┌──────────────────────────────────────────────────────────────────────┐
 *   │ Result helpers          HttpContext / routing access                 │
 *   │ Ok / NotFound /         Request, Response, RouteData                │
 *   │ BadRequest / Created /  User (ClaimsPrincipal)                      │
 *   │ NoContent / StatusCode  ModelState (validation)                     │
 *   │ Json / Content / File   ControllerContext, Url (IUrlHelper)         │
 *   └──────────────────────────────────────────────────────────────────────┘
 *
 * Controller ADDS (on top of ControllerBase):
 *   View() / PartialView()    — invokes the Razor view engine
 *   ViewData                  — ViewDataDictionary; passed to the view
 *   ViewBag                   — dynamic wrapper around ViewData
 *   TempData                  — ITempDataDictionary; survives one redirect
 *
 * CHOOSE:
 *   Inherit Controller       when actions return HTML (Razor views)
 *   Inherit ControllerBase   when actions return JSON / data (REST APIs)
 *
 * [ApiController] ATTRIBUTE:
 *   Applying [ApiController] to a ControllerBase subclass enables four
 *   automatic behaviours that make API development more ergonomic:
 *
 *   1. Attribute routing required
 *      Actions must use [Route] / [HttpGet] / [HttpPost] etc.
 *      Conventional route templates (MapControllerRoute) are not used.
 *      Attempting to use a conventionally-routed ApiController causes a
 *      runtime error, not a build error.
 *
 *   2. Automatic 400 on invalid ModelState
 *      If ModelState.IsValid is false after model binding, the framework
 *      returns HTTP 400 with a ValidationProblemDetails body automatically —
 *      no manual if (!ModelState.IsValid) return BadRequest(ModelState) needed.
 *
 *   3. Binding source inference
 *      [ApiController] infers binding sources for action parameters:
 *        • Complex types from body   → [FromBody] inferred
 *        • Simple types from route   → [FromRoute] inferred when in route template
 *        • Simple types from query   → [FromQuery] inferred otherwise
 *      Without [ApiController], all parameters default to [FromQuery] or
 *      model binding composite logic; [FromBody] must be explicit.
 *
 *   4. ProblemDetails error responses
 *      Status-code responses (400, 404, etc.) automatically include
 *      RFC 7807 ProblemDetails bodies: { "type", "title", "status", "detail" }.
 *      Configure with builder.Services.AddProblemDetails().
 *
 * COVERED IN DETAIL LATER:
 *   Model binding attribute inference → 06. Model Binding in MVC
 *   ProblemDetails                    → ASP.NET Core APIs module
 */

/*
 * SECTION 16: [NonController] ON A HELPER CLASS
 * ─────────────────────────────────────────────────────────────────────────────
 * Any class placed in the Controllers/ folder whose name ends in "Controller"
 * or that inherits ControllerBase is picked up by MVC routing by default.
 *
 * [NonController] prevents that registration. It is the escape hatch for:
 *   • Abstract base controllers that provide shared logic but should not route
 *   • Helper / utility classes accidentally named with the "Controller" suffix
 *   • Test doubles or stubs in test projects that inherit controller types
 *
 * The class below is a helper that shares logic across API controllers.
 * Without [NonController], MVC would attempt to route requests to it.
 * With [NonController], it is completely invisible to the routing system.
 */
[NonController] // MVC routing ignores this class — it is a shared helper, not a controller
internal sealed class CatalogControllerHelper // name ends in "Controller" but routing skips it
{
    // Returns a consistently formatted "not found" error body used across API controllers
    internal static object NotFoundBody(int id) =>
        new { error = $"Product with id {id} was not found.", code = "PRODUCT_NOT_FOUND" };
}

/*
 * SECTION 17: ObjectResult FAMILY — TYPED RESULT HELPERS FOR API RESPONSES
 * ─────────────────────────────────────────────────────────────────────────────
 * The ObjectResult family serialises an object to the response body using content
 * negotiation (JSON by default). These helpers are the primary vocabulary for
 * REST API responses in a ControllerBase-derived controller.
 *
 * Helper          Returns type            HTTP status   Body
 * ─────────────   ─────────────────────   ───────────   ─────────────────────────────
 * Ok()            OkResult                200           empty
 * Ok(payload)     OkObjectResult          200           JSON-serialised payload
 * NotFound()      NotFoundResult          404           empty (or ProblemDetails with [ApiController])
 * NotFound(obj)   NotFoundObjectResult    404           JSON-serialised obj
 * BadRequest()    BadRequestResult        400           empty
 * BadRequest(obj) BadRequestObjectResult  400           JSON-serialised obj
 * BadRequest(MS)  BadRequestObjectResult  400           ValidationProblemDetails (ModelState)
 * Created(uri,obj) CreatedResult          201           JSON; Location header set to uri
 * NoContent()     NoContentResult         204           empty (no body allowed by HTTP spec)
 * StatusCode(n)   StatusCodeResult        n             empty
 * StatusCode(n,v) ObjectResult            n             JSON-serialised v
 *
 * All ObjectResult subclasses go through content negotiation — the formatter
 * (JSON by default) is chosen based on the request Accept header and registered
 * output formatters. Use Json(obj) to bypass negotiation and force JSON always.
 *
 * CREATED vs ACCEPTED:
 *   Created(uri, obj) → 201: resource was created synchronously; uri points to it.
 *   202 Accepted       → use StatusCode(202, obj) when processing is asynchronous.
 */
[ApiController]         // enables automatic 400, binding inference, ProblemDetails
[Route("api/catalog")]  // base route: /api/catalog — required with [ApiController]
public class ApiController : ControllerBase // ControllerBase: no View(), no ViewData
{
    // Static list simulates a repository — replaced by real DB access in practice
    private static readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Widget",    Price = 9.99m,  Category = "Hardware"    },
        new Product { Id = 2, Name = "Gadget",    Price = 24.99m, Category = "Electronics" },
        new Product { Id = 3, Name = "Doohickey", Price = 4.99m,  Category = "Misc"        },
    };

    // GET /api/catalog  →  returns all products as JSON array
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_products); // OkObjectResult: 200 + JSON array
    }

    // GET /api/catalog/{id}  →  returns one product or 404
    [HttpGet("{id:int}")]  // route constraint :int ensures routing only matches integers
    public IActionResult GetById(int id)  // id inferred [FromRoute] by [ApiController]
    {
        Product? product = _products.Find(p => p.Id == id); // List<T>.Find returns T?
        if (product is null)
            return NotFound(CatalogControllerHelper.NotFoundBody(id)); // 404 + structured error
        return Ok(product); // 200 + serialised Product JSON object
    }

    // POST /api/catalog  →  creates a new product, returns 201 with Location header
    [HttpPost]
    public IActionResult Create([FromBody] Product product) // [FromBody] explicit; [ApiController] infers it too
    {
        // In a real app: validate, save to DB, get the assigned Id back, then return Created().
        // Here we return the received product with a simulated URI.
        string location = $"/api/catalog/{product.Id}"; // URI of the new resource
        return Created(location, product);               // 201 + Location header + JSON body
    }

    // PUT /api/catalog/{id}  →  replaces a product; returns 200 or 404
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Product product)
    {
        Product? existing = _products.Find(p => p.Id == id);
        if (existing is null)
            return NotFound(CatalogControllerHelper.NotFoundBody(id)); // 404 if not found
        _ = product; // in a real app: update existing with product fields, save, return Ok
        return Ok(product); // 200 + updated product body
    }

    // DELETE /api/catalog/{id}  →  removes a product; returns 204 No Content
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        Product? existing = _products.Find(p => p.Id == id);
        if (existing is null)
            return NotFound(CatalogControllerHelper.NotFoundBody(id)); // 404 if not found
        _ = existing; // in a real app: _context.Products.Remove(existing); await _context.SaveChangesAsync()
        return NoContent(); // 204 — success; HTTP spec prohibits a body on 204 responses
    }

    // GET /api/catalog/validate?name=Widget  →  demonstrates BadRequest with detail
    [HttpGet("validate")]
    public IActionResult Validate(string? name) // name inferred [FromQuery] by [ApiController]
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Query parameter 'name' is required." }); // 400 + body
        bool found = _products.Exists(p => p.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        return Ok(new { name = name, exists = found }); // 200 + search result
    }
}
