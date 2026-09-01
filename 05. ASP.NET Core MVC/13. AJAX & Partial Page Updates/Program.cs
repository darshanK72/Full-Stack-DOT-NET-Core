/*
 * TOPIC: AJAX & Partial Page Updates
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   Traditional MVC pages reload entirely on every user action. AJAX (Asynchronous
 *   JavaScript and XML — now almost always JSON or HTML) lets the browser exchange
 *   data with the server in the background, updating only the parts of the page
 *   that changed. The result: faster perceived performance, less bandwidth, and
 *   richer interactions without a full page flash.
 *
 *   ASP.NET Core MVC supports AJAX natively through two complementary mechanisms:
 *     Server side  — PartialViewResult, Json(data), X-Requested-With detection
 *     Client side  — the browser's built-in Fetch API (modern replacement for XMLHttpRequest)
 *
 * WHAT YOU WILL LEARN:
 *    1.  What Is AJAX and partial page updates              (Program.cs SECTION 1)
 *    2.  MVC + Antiforgery service registration             (Program.cs SECTION 2)
 *    3.  ProductViewModel — model for partials and JSON     (Models/ProductViewModel.cs)
 *    4.  In-memory ProductService                           (Services/ProductService.cs)
 *    5.  PartialViewResult + IsAjaxRequest pattern          (Controllers/ProductsController.cs)
 *    6.  Json(data) — returning JSON from MVC               (Controllers/ProductsController.cs)
 *    7.  HTML vs JSON — decision guide                      (Controllers/ProductsController.cs)
 *    8.  Anti-forgery tokens with AJAX                      (Controllers/ProductsController.cs)
 *    9.  Fetch API — loading partial views via AJAX         (Views/Products/Index.cshtml)
 *   10.  Form submission via AJAX (FormData + JSON)         (Views/Products/Index.cshtml)
 *   11.  Multiple regions, optimistic UI, spinners          (Views/Products/Index.cshtml)
 *   12.  _ProductCard partial view                          (Views/Products/_ProductCard.cshtml)
 *   13.  _ProductList partial view                          (Views/Products/_ProductList.cshtml)
 *   14.  _Layout — antiforgery meta tag                     (Views/Shared/_Layout.cshtml)
 *   15.  htmx — PREVIEW                                     (Program.cs QUICK REFERENCE)
 *
 * CHAPTER MAP (open files in this order):
 *   SECTION  1  → Program.cs (this file)               What Is AJAX
 *   SECTION  2  → Program.cs (this file)               MVC + Antiforgery Registration
 *   SECTION  3  → Models/ProductViewModel.cs           ViewModel for partials & JSON
 *   SECTION  4  → Services/ProductService.cs           In-memory data service
 *   SECTION  5  → Controllers/ProductsController.cs    PartialViewResult + IsAjaxRequest
 *   SECTION  6  → Controllers/ProductsController.cs    Json(data)
 *   SECTION  7  → Controllers/ProductsController.cs    HTML vs JSON decision
 *   SECTION  8  → Controllers/ProductsController.cs    Anti-forgery with AJAX
 *   SECTION  9  → Views/Products/Index.cshtml          Fetch API + partial view load
 *   SECTION 10  → Views/Products/Index.cshtml          FormData + JSON.stringify POSTs
 *   SECTION 11  → Views/Products/Index.cshtml          Multiple regions, optimistic UI, spinners
 *   SECTION 12  → Views/Products/_ProductCard.cshtml   Single product card partial
 *   SECTION 13  → Views/Products/_ProductList.cshtml   Product list partial
 *   SECTION 14  → Views/Shared/_Layout.cshtml          Antiforgery meta tag in layout
 */

/*
 * SECTION 1: WHAT IS AJAX AND PARTIAL PAGE UPDATES
 * ─────────────────────────────────────────────────────────────────────────────
 * AJAX stands for Asynchronous JavaScript and XML. The "XML" part is now largely
 * historical — modern AJAX exchanges HTML fragments or JSON, not XML.
 *
 * THE PROBLEM AJAX SOLVES:
 *   Without AJAX, every user action that needs server data triggers a full page
 *   reload: the browser discards the current DOM, downloads a new HTML document,
 *   re-parses CSS and JS, and re-renders. This causes a visible flash, wastes
 *   bandwidth (headers, unchanged parts of the page), and resets UI state like
 *   scroll positions and open accordions.
 *
 * WITH AJAX:
 *   The browser sends an HTTP request in the background (via the Fetch API or
 *   XMLHttpRequest), receives a fragment of HTML or structured JSON data, and
 *   updates only the relevant part of the existing DOM. The rest of the page
 *   is untouched.
 *
 * TWO RESPONSE STRATEGIES — covered in SECTION 7:
 *
 *   Strategy A — Return HTML (partial view):
 *     Server renders a Razor partial view and returns the HTML string.
 *     Client inserts it directly into the DOM via element.innerHTML = html.
 *     Pros: server controls presentation; re-uses Razor logic; no client-side templating.
 *     Cons: tightly couples server and client to a specific DOM shape.
 *
 *   Strategy B — Return JSON:
 *     Server returns structured data as JSON.
 *     Client reads the data and builds or updates the DOM itself.
 *     Pros: flexible; same endpoint can serve different clients (browser, mobile app).
 *     Cons: requires client-side templating logic; more JS to maintain.
 *
 * BROWSER SUPPORT:
 *   The Fetch API is supported by all modern browsers (Chrome 42+, Firefox 39+,
 *   Safari 10.1+, Edge 14+). It is the modern replacement for XMLHttpRequest.
 *   No polyfill is needed for current browser targets.
 *
 * PARTIAL PAGE UPDATES IN ASP.NET CORE MVC:
 *   The pattern works naturally with MVC:
 *     Controller action → return PartialView("_Name", model)  → HTML fragment
 *     Controller action → return Json(data)                   → JSON payload
 *     Razor view         → JavaScript Fetch calls              → updates the DOM
 */

/*
 * SECTION 2: MVC SERVICES + ANTIFORGERY REGISTRATION
 * ─────────────────────────────────────────────────────────────────────────────
 * AddControllersWithViews() registers everything needed for the Controller-View
 * pattern: MVC controllers, Razor view engine, model binders, data annotations
 * validation, tag helpers, and antiforgery services.
 *
 * ANTIFORGERY (CSRF PROTECTION):
 *   Cross-Site Request Forgery (CSRF) attacks trick a logged-in user's browser
 *   into submitting a forged request to your server. ASP.NET Core defends against
 *   CSRF by embedding a unique per-session token in every form. The server validates
 *   this token on POST requests — a forged request from another site cannot know
 *   the token and will be rejected with HTTP 400.
 *
 *   AddControllersWithViews() calls AddAntiforgery() internally. Calling it again
 *   explicitly (as below) lets us configure options — in particular the header name
 *   that AJAX requests must use when they cannot include the token in a form field:
 *
 *   options.HeaderName = "RequestVerificationToken"   (this is also the default value)
 *
 *   Why configure it? Documentation. When working on a team, making the expected
 *   header name explicit in startup code avoids the "why isn't antiforgery working
 *   on my JSON POST?" class of bugs. The JS in Index.cshtml reads this same name.
 *
 * SERVICE REGISTRATION SUMMARY:
 *   AddControllersWithViews()   — MVC pipeline: controllers, views, model binding
 *   AddAntiforgery(options)     — CSRF tokens; sets the JS-visible header name
 *   AddSingleton<ProductService>— in-memory data store for the chapter demos
 *
 * COVERED IN DETAIL LATER:
 *   DI lifetimes → 04. Dependency Injection & Service Lifetimes
 */

using AjaxPartialUpdates.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(); // MVC: controllers + Razor view engine

builder.Services.AddAntiforgery(options =>
{
    // HeaderName is the HTTP request header that AJAX JSON POSTs use to send
    // the antiforgery token. The Fetch API puts the token here because JSON
    // requests have no form body where the hidden __RequestVerificationToken
    // field would normally live. "RequestVerificationToken" is the default value.
    options.HeaderName = "RequestVerificationToken"; // header JS must set on JSON POSTs
});

builder.Services.AddSingleton<ProductService>(); // single shared in-memory list for the demo

var app = builder.Build();

/*
 * MIDDLEWARE PIPELINE AND ROUTE SETUP
 * ─────────────────────────────────────────────────────────────────────────────
 * The pipeline for an MVC app follows a standard order:
 *
 *   UseStaticFiles()      — serve wwwroot/ files (CSS, images, JS bundles) without
 *                           routing overhead; short-circuits before UseRouting()
 *   UseRouting()          — parse the URL and identify the matching endpoint
 *   UseAuthorization()    — enforce [Authorize] attributes (no auth configured here)
 *   MapControllerRoute()  — conventional {controller}/{action}/{id?} route
 *
 * The default route maps:
 *   /                         → Products/Index
 *   /Products/ProductCard/3   → Products/ProductCard(id=3)
 *   /Products/AddProduct      → Products/AddProduct (POST)
 *
 * COVERED IN DETAIL LATER:
 *   Routing depth → 09. Routing & Attribute Routing
 *   Middleware pipeline → ASP.NET Core 03. Middleware Pipeline
 */
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // full stack trace in dev; never expose in production
}
else
{
    app.UseExceptionHandler("/Home/Error"); // generic error page in non-dev environments
}

app.UseStaticFiles(); // serve wwwroot/ before routing to short-circuit early
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}"); // Products/Index is the home page

app.Run(); // start Kestrel; block until shutdown

/*
 * QUICK REFERENCE — AJAX & Partial Page Updates
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * SERVER SIDE — RETURNING RESPONSES FROM MVC CONTROLLERS:
 *
 *   return PartialView("_Name", model);     // HTML fragment, no layout
 *   return PartialView("_Name");            // HTML fragment, no model
 *   return View("ViewName", model);         // full page with layout
 *   return Json(data);                      // JSON via System.Text.Json
 *   return Json(data, new JsonSerializerOptions { ... }); // custom options
 *   return NotFound();                      // 404; combine with Json for AJAX errors
 *   return BadRequest(new { error = "…" }); // 400 with JSON error body
 *
 * DETECTING AJAX REQUESTS (X-Requested-With):
 *   bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
 *   // Dual-mode action: return partial if AJAX, full page otherwise
 *
 * ANTI-FORGERY TOKENS:
 *   On forms:     @Html.AntiForgeryToken() — renders hidden __RequestVerificationToken input
 *                 new FormData(form) captures it automatically
 *   On JSON POST: read from <meta name="csrf-token"> tag in layout
 *                 set header: 'RequestVerificationToken': token
 *   Action attr:  [ValidateAntiForgeryToken] — validates token from form OR header
 *
 * CLIENT SIDE — FETCH API PATTERNS:
 *
 *   // GET → HTML fragment (PartialView)
 *   const res = await fetch('/Products/ProductCard/1', {
 *       headers: { 'X-Requested-With': 'XMLHttpRequest' }
 *   });
 *   element.innerHTML = await res.text();
 *
 *   // GET → JSON data
 *   const res = await fetch('/Products/ProductJson/1');
 *   const data = await res.json();
 *
 *   // POST → FormData (token from hidden form field)
 *   const res = await fetch('/Products/AddProduct', {
 *       method: 'POST',
 *       body: new FormData(form)   // includes __RequestVerificationToken automatically
 *       // Do NOT set Content-Type — fetch sets multipart/form-data with boundary
 *   });
 *
 *   // POST → JSON (token from meta tag header)
 *   const token = document.querySelector('meta[name="csrf-token"]').content;
 *   const res = await fetch('/Products/AddProductJson', {
 *       method: 'POST',
 *       headers: {
 *           'Content-Type': 'application/json',
 *           'RequestVerificationToken': token
 *       },
 *       body: JSON.stringify(data)
 *   });
 *
 *   // Error handling pattern
 *   try {
 *       const res = await fetch(url, options);
 *       if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);
 *       const data = await res.json(); // or res.text()
 *   } catch (err) {
 *       // covers network failures AND our manually thrown HTTP errors
 *       console.error(err.message);
 *   }
 *
 * OPTIMISTIC UI PATTERN:
 *   1. Insert placeholder in DOM immediately (before server responds)
 *   2. Await server confirmation
 *   3. On success: replace placeholder with confirmed server data
 *   4. On failure: remove placeholder; show error
 *
 * ─── SECTION 15 PREVIEW: htmx ────────────────────────────────────────────────
 * htmx (https://htmx.org) is a lightweight JavaScript library (~14 KB) that lets
 * HTML attributes drive AJAX behaviour, eliminating most hand-written JS.
 *
 * Core attributes:
 *   hx-get="/Products/ProductCard/1"   — issue GET on trigger
 *   hx-post="/Products/AddProduct"     — issue POST on form submit
 *   hx-target="#container"             — where to put the response HTML
 *   hx-swap="innerHTML"                — how to insert: innerHTML, outerHTML, beforeend…
 *   hx-trigger="click"                 — DOM event that fires the request
 *   hx-include="[name='__RequestVerificationToken']"  — include antiforgery field
 *
 * Example that replaces a manual fetch → innerHTML in about one line:
 *   <button hx-get="/Products/ProductCard/1"
 *           hx-target="#product-card-container"
 *           hx-swap="innerHTML"
 *           hx-headers='{"X-Requested-With":"XMLHttpRequest"}'>
 *     Load Card
 *   </button>
 *
 * htmx is well-suited for server-side rendering (SSR) teams that want AJAX
 * interactivity without building a full SPA or maintaining complex JS.
 * Use it when: actions are CRUD-oriented, the server owns the HTML, and the
 * team prefers HTML-over-the-wire to client-side state management.
 *
 * COVERED IN DETAIL LATER → 17. htmx (when that chapter is created)
 */
