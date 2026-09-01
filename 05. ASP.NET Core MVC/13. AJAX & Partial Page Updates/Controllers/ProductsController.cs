using AjaxPartialUpdates.Models;
using AjaxPartialUpdates.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AjaxPartialUpdates.Controllers;

/*
 * FILE ROLE: ProductsController teaches every server-side AJAX response pattern:
 *   returning partial views, detecting AJAX via the X-Requested-With header,
 *   returning JSON, choosing HTML vs JSON, and protecting POST actions with
 *   anti-forgery tokens for both FormData and JSON AJAX requests.
 *
 * SECTIONS IN THIS FILE:
 *   5. PartialViewResult + IsAjaxRequest — returning partial HTML from a controller
 *   6. Json(data) — returning JSON from an MVC controller action
 *   7. HTML vs JSON — decision guide for choosing the right response type
 *   8. Anti-Forgery Tokens with AJAX — [ValidateAntiForgeryToken] + AJAX patterns
 */

/*
 * SECTION 5: PartialViewResult AND THE IsAjaxRequest PATTERN
 * ─────────────────────────────────────────────────────────────────────────────
 * PartialViewResult is the action return type for actions that serve HTML
 * fragments. Unlike ViewResult (a full page with layout), PartialViewResult
 * renders only the named partial view — no _Layout.cshtml wrapper.
 *
 * SYNTAX:
 *   return PartialView("_PartialName", model);  // with model
 *   return PartialView("_PartialName");          // no model
 *
 * VIEW LOOKUP ORDER — framework searches in this order:
 *   1. Views/{CurrentController}/_PartialName.cshtml
 *   2. Views/Shared/_PartialName.cshtml
 *   3. Pages/Shared/_PartialName.cshtml
 * The leading underscore is a naming CONVENTION (signals partial), not a rule.
 *
 * PARTIALVIEWRESULT vs VIEWRESULT vs IACTIONRESULT:
 *   PartialViewResult   — strongly typed for partial views; no layout rendered
 *   ViewResult          — full page view; uses layout from _ViewStart.cshtml
 *   IActionResult       — use when the action can return different result types
 *                         (e.g., PartialView on AJAX, View on direct navigation)
 *
 * THE IsAjaxRequest PATTERN — X-Requested-With HEADER:
 *   Classic jQuery automatically added X-Requested-With: XMLHttpRequest to every
 *   $.ajax() call. The Fetch API does NOT set this header by default — you must
 *   add it explicitly in the fetch() options (demonstrated in Index.cshtml).
 *
 *   The header lets a single action URL serve both full pages (direct navigation)
 *   and fragments (AJAX calls), avoiding the need for two separate routes:
 *
 *     bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
 *     if (isAjax) return PartialView("_ProductList", products);
 *     return View("Index", products);  // full page for initial/direct navigation
 *
 *   This is a convention — not enforced by the framework. For actions that are
 *   ONLY ever called via AJAX, use PartialViewResult directly (ProductCard action
 *   below) rather than the dual-mode pattern.
 *
 * PITFALL — Direct URL navigation to a PartialViewResult action:
 *   If the user navigates directly to /Products/ProductCard/1 in the browser,
 *   they get a fragment of HTML without layout — usually an undesirable UX.
 *   Solutions: (1) use the IsAjaxRequest check to fallback to a full view,
 *   (2) redirect to the full page with the item highlighted, or
 *   (3) accept it if the URL is truly internal-only.
 */
public class ProductsController : Controller
{
    private readonly ProductService _service;

    public ProductsController(ProductService service)
    {
        _service = service; // injected Singleton; same instance for all requests
    }

    // Full-page entry point — sends all products to the Index view
    public IActionResult Index()
    {
        IEnumerable<ProductViewModel> products = _service.GetAll();
        return View(products); // renders Views/Products/Index.cshtml with _Layout
    }

    // AJAX-only action — always returns a partial view HTML fragment
    // Return type is PartialViewResult (not IActionResult) to make the intent explicit
    // Client call: fetch('/Products/ProductCard/3') → response.text() → element.innerHTML
    public IActionResult ProductCard(int id)
    {
        ProductViewModel? product = _service.GetById(id); // returns null if not found
        if (product == null)
            return NotFound(); // 404 — client JS checks response.ok before using the HTML
        return PartialView("_ProductCard", product); // Views/Products/_ProductCard.cshtml
    }

    // Dual-mode action — returns partial on AJAX, full page on direct navigation
    // Test in browser: GET /Products/ProductList?category=Electronics
    public IActionResult ProductList(string? category)
    {
        IEnumerable<ProductViewModel> products = _service.GetByCategory(category);

        // Read the X-Requested-With header to detect AJAX calls
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        if (isAjax)
            return PartialView("_ProductList", products); // fragment only
        return View("Index", products);                   // full page with layout
    }

    /*
     * SECTION 6: Json(data) — RETURNING JSON FROM AN MVC CONTROLLER
     * ─────────────────────────────────────────────────────────────────────────
     * return Json(data) returns a JsonResult — the MVC equivalent of returning
     * a JSON body. Under the hood, ASP.NET Core serializes the object using
     * System.Text.Json (the default in .NET 6+), which produces camelCase
     * property names by default.
     *
     * EXAMPLES:
     *   return Json(product);                            // 200 + JSON body
     *   return Json(new { success = true, id = 42 });    // anonymous object → JSON
     *   return Json(products.ToList());                  // collection → JSON array
     *
     * PROPERTY NAME CASING (System.Text.Json default in .NET 8):
     *   C# ProductViewModel.Name  → JSON "name"     (camelCase)
     *   C# ProductViewModel.InStock → JSON "inStock"
     *
     *   To change: builder.Services.AddControllersWithViews()
     *                .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);
     *   (null = PascalCase; default = camelCaseNamingPolicy)
     *
     * STATUS CODES WITH JSON:
     *   return Json(data);            // always 200 OK
     *   return NotFound();            // 404, no body — use for strict REST
     *   return BadRequest(new{...});  // 400 + JSON error body — better for AJAX
     *   return StatusCode(500, obj);  // custom status + JSON
     *
     * SEARCH ACTION — returns JSON carrying data for THREE DOM regions at once.
     * The client updates count, category badges, and product list from one response.
     * See SECTION 11 in Index.cshtml for the client-side implementation.
     */

    // GET /Products/ProductJson/3 — returns one product as JSON for the Fetch demo
    public IActionResult ProductJson(int id)
    {
        ProductViewModel? product = _service.GetById(id);
        if (product == null)
            return NotFound(); // client JS checks response.status === 404 separately
        return Json(product);  // serialized by System.Text.Json; property names are camelCase
    }

    // GET /Products/Search?term=keyboard — returns JSON for multiple-region update
    // Response shape: { count, term, categories[], products[] }
    public IActionResult Search(string? term)
    {
        List<ProductViewModel> results = _service.Search(term).ToList();
        return Json(new
        {
            count      = results.Count,
            term       = term ?? string.Empty,
            categories = results.Select(p => p.Category).Distinct().OrderBy(c => c), // unique categories in results
            products   = results                                                        // full product data
        });
    }

    /*
     * SECTION 7: HTML vs JSON — CHOOSING THE RIGHT RESPONSE TYPE
     * ─────────────────────────────────────────────────────────────────────────
     * Returning HTML (PartialView) vs returning JSON is an architectural choice
     * that affects who controls presentation, coupling, and reuse.
     *
     * RETURN HTML (PartialView) WHEN:
     *   ✓ The server owns the final presentation (Razor templates, server-side logic)
     *   ✓ The fragment is inserted into the DOM with minimal JS (element.innerHTML = html)
     *   ✓ The team is primarily C#/Razor focused (less client-side JS to maintain)
     *   ✓ Consistency with the rest of the MVC site is important
     *   ✓ The partial contains complex server-side rendering (loops, conditionals, localization)
     *   ✗ Avoid when: the same data feeds multiple client types (browser + mobile app + CLI)
     *   ✗ Avoid when: the client needs to transform the data differently than the server template
     *
     * RETURN JSON WHEN:
     *   ✓ Multiple clients consume the same endpoint (browser + mobile app + 3rd-party)
     *   ✓ The client must update MULTIPLE independent DOM regions from one response
     *   ✓ The client handles presentation entirely (SPA patterns, client-side templates)
     *   ✓ The data is consumed by non-rendering code (charts, maps, counters)
     *   ✓ Progressive enhancement: degraded fallback for no-JS environments
     *   ✗ Avoid when: the response is purely a UI fragment owned by the server template
     *
     * HYBRID APPROACH — return JSON wrapping HTML strings:
     *   return Json(new { listHtml = "...", count = 5 })
     *   Rarely recommended — the coupling of HTML strings in JSON is fragile and
     *   hard to maintain. Prefer either pure HTML (PartialView) or pure data (Json).
     *
     * RULE OF THUMB:
     *   If JS just sets element.innerHTML = response → return PartialView (HTML)
     *   If JS reads .count, .items, .name and builds DOM → return Json (JSON)
     *   If unsure and building MVC-first → return PartialView first; refactor to Json
     *   if a second client appears.
     */

    /*
     * SECTION 8: ANTI-FORGERY TOKENS WITH AJAX
     * ─────────────────────────────────────────────────────────────────────────
     * [ValidateAntiForgeryToken] is an action filter that rejects any POST request
     * that does not include a valid CSRF token. It MUST be on every state-changing
     * action (POST, PUT, DELETE) — omitting it opens the action to CSRF attacks.
     *
     * HOW THE TOKEN REACHES THE SERVER:
     *
     *   Pattern A — FormData POST (AJAX form with @Html.AntiForgeryToken()):
     *     1. @Html.AntiForgeryToken() renders a hidden <input name="__RequestVerificationToken">
     *     2. new FormData(form) captures the hidden field automatically
     *     3. fetch(url, { method: 'POST', body: formData }) sends it in the multipart body
     *     4. [ValidateAntiForgeryToken] reads it from IFormCollection
     *     Key: do NOT set Content-Type manually — FormData sets multipart/form-data + boundary
     *
     *   Pattern B — JSON POST (fetch with JSON.stringify):
     *     1. _Layout.cshtml injects the token value into <meta name="csrf-token">
     *     2. JS reads: document.querySelector('meta[name="csrf-token"]').content
     *     3. fetch sets header: 'RequestVerificationToken': tokenValue
     *     4. [ValidateAntiForgeryToken] reads it from the request header
     *     Key: the header name ("RequestVerificationToken") must match AntiforgeryOptions.HeaderName
     *          configured in Program.cs SECTION 2 (defaults to "RequestVerificationToken")
     *
     * ANTIFORGERY TOKEN LOOKUP ORDER (IAntiforgery.ValidateRequestAsync):
     *   1. Check request header AntiforgeryOptions.HeaderName ("RequestVerificationToken")
     *   2. If not found, check form body field AntiforgeryOptions.FormFieldName ("__RequestVerificationToken")
     *   If neither is present → 400 Bad Request (AntiforgeryValidationException)
     *
     * [AutoValidateAntiforgeryToken] ALTERNATIVE:
     *   Applied at the controller class level, it automatically validates on all
     *   unsafe HTTP methods (POST, PUT, DELETE, PATCH) without decorating each action.
     *   GET/HEAD/OPTIONS/TRACE are always exempt (they must be idempotent).
     *
     * PITFALL — [FromBody] + [ValidateAntiForgeryToken]:
     *   [ValidateAntiForgeryToken] runs as an action filter BEFORE model binding.
     *   It checks the header (Pattern B) because the body is JSON — not a form.
     *   Forgetting to send the header on a JSON POST results in HTTP 400 before
     *   the action method body executes, which can look like a silent failure.
     *
     * PITFALL — CORS + antiforgery:
     *   Antiforgery tokens are per-origin; cross-origin AJAX requests cannot read
     *   the token cookie (SameSite + HttpOnly). For cross-origin APIs, use
     *   JWT Bearer tokens instead of cookie-based antiforgery.
     */

    // POST /Products/AddProduct — FormData submission (Pattern A)
    // Form in Index.cshtml contains @Html.AntiForgeryToken(); new FormData(form) captures it
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddProduct(ProductViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            // Return the partial view; Razor can display ModelState errors with tag helpers
            return PartialView("_ProductCard", vm); // client replaces the form area with error feedback
        }
        ProductViewModel saved = _service.Add(vm); // assign server-side Id, persist to list
        return PartialView("_ProductCard", saved);  // return HTML for the newly created card
    }

    // POST /Products/AddProductJson — JSON POST (Pattern B)
    // Client sends { Content-Type: application/json } + { RequestVerificationToken: <token> }
    // [FromBody] binds the JSON request body to ProductViewModel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddProductJson([FromBody] ProductViewModel? vm)
    {
        if (vm == null)
            return BadRequest(new { success = false, error = "Request body was null or not valid JSON." });

        if (!ModelState.IsValid)
        {
            IEnumerable<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors }); // 400 + JSON error list
        }

        ProductViewModel saved = _service.Add(vm);
        return Json(new { success = true, product = saved }); // 200 + confirmation JSON
    }
}
