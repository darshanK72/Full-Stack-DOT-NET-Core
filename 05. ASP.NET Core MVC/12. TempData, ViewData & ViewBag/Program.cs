/*
 * TOPIC: TempData, ViewData & ViewBag
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   MVC controllers and views must exchange data. ASP.NET Core provides three
 *   built-in mechanisms with very different lifetimes, type-safety, and storage
 *   behaviours. TempData uniquely survives one redirect, making it the right
 *   tool for the Post-Redirect-Get (PRG) pattern — the industry standard for
 *   preventing double-form-submission after a POST.
 *
 * WHAT YOU WILL LEARN:
 *    1. ViewData — ViewDataDictionary, string keys, boxing/unboxing, request lifetime
 *    2. ViewBag — dynamic wrapper over the same ViewDataDictionary
 *    3. ViewData vs ViewBag comparison (same underlying store)
 *    4. [ViewData] attribute — auto-populate ViewData from a controller property
 *    5. TempData — ITempDataDictionary, read-once, survives one redirect
 *    6. CookieTempDataProvider vs SessionStateTempDataProvider
 *    7. [TempData] attribute on controller properties
 *    8. Peek and Keep — reading TempData without consuming it
 *    9. PRG pattern — Post-Redirect-Get with TempData success/error messages
 *   10. ITempDataProvider — swapping the storage back-end
 *   11. Storing complex objects in TempData (JSON serialisation requirement)
 *   12. @model vs ViewBag/ViewData — when to use each, anti-patterns
 *
 * CHAPTER MAP (open files in this order):
 *   SECTION  1-3  → Program.cs                              Overview + Registration + Providers
 *   SECTION  4    → Models/ProductFormModel.cs              Form model + TempData serialisation
 *   SECTION  5    → Controllers/DemoController.cs           ViewData (ViewDataDictionary)
 *   SECTION  6    → Controllers/DemoController.cs           ViewBag + comparison table
 *   SECTION  7    → Controllers/DemoController.cs           [ViewData] attribute
 *   SECTION  8    → Views/Products/ViewDataDemo.cshtml      Reading ViewData & ViewBag in Razor
 *   SECTION  9    → Controllers/ProductsController.cs       TempData + [TempData] attribute
 *   SECTION 10    → Controllers/ProductsController.cs       Peek & Keep
 *   SECTION 11    → Controllers/ProductsController.cs       PRG pattern (Create POST → redirect)
 *   SECTION 12    → Views/Products/Create.cshtml            Form POST view
 *   SECTION 13    → Views/Products/Index.cshtml             TempData messages after redirect
 *   SECTION 14    → Program.cs Quick Reference              @model vs ViewBag/ViewData guidance
 */

/*
 * SECTION 1: OVERVIEW — THREE DATA-PASSING MECHANISMS
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core MVC provides three controller-to-view data-passing mechanisms:
 *
 *   Mechanism   Type                           Lifetime            Type safety
 *   ──────────  ─────────────────────────────  ──────────────────  ──────────────────────────
 *   ViewData    ViewDataDictionary             Current request     None — object? values;
 *               (IDictionary<string,object?>)  only; dies on       cast/unbox required in view
 *                                              redirect
 *   ViewBag     dynamic (DynamicViewData)      Same store as       None — no compile-time
 *                                              ViewData; current   checking; runtime errors
 *                                              request only        possible
 *   TempData    ITempDataDictionary            Survives ONE        None — same cast/unbox;
 *               (IDictionary<string,object?>)  redirect; read-     values must be JSON
 *                                              once semantics      serialisable
 *
 * Rule of thumb:
 *   Use @model (ViewModel) for primary page data — typed, IntelliSense, testable.
 *   Use TempData for cross-redirect messages (success, error, info banners).
 *   ViewData/ViewBag are acceptable for layout-level data (page title, breadcrumbs).
 *   Avoid ViewData/ViewBag for primary page data — prefer a strongly-typed ViewModel.
 *
 * COVERED IN DETAIL → 05. ViewModels & Strongly Typed Views
 */

/*
 * SECTION 2: MVC SERVICES REGISTRATION
 * ─────────────────────────────────────────────────────────────────────────────
 * AddControllersWithViews() registers all MVC services in one call:
 *   - Controller activation + model binding + routing
 *   - Razor view engine (compiles .cshtml at runtime or build time)
 *   - TempData infrastructure (CookieTempDataProvider registered by default)
 *   - Tag Helpers and HTML Helpers
 *   - Validation (ModelState, DataAnnotations)
 *
 * AddMvc() is a superset — also registers Razor Pages. Use AddControllersWithViews()
 * for pure MVC projects; AddMvc() when the same app mixes MVC and Razor Pages.
 *
 * AddRazorPages() — registers Razor Pages only (no controller-based MVC).
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(); // MVC + Razor views + TempData (cookie-based)

/*
 * SECTION 3: TEMPDATA PROVIDERS — CookieTempDataProvider vs SessionStateTempDataProvider
 * ─────────────────────────────────────────────────────────────────────────────
 * ITempDataProvider is the abstraction behind TempData storage. Two providers
 * ship with ASP.NET Core MVC:
 *
 *   Provider                         Storage         Default?  Size limit
 *   ─────────────────────────────── ─────────────── ──────── ─────────────────────
 *   CookieTempDataProvider           HTTP cookie     YES       ~4 KB (browser limit)
 *   SessionStateTempDataProvider     Server session  No        Server memory / cache
 *
 * CookieTempDataProvider (default — no extra configuration needed):
 *   - Serialises TempData to JSON and Base64-encodes it into an HTTP cookie
 *   - Cookie is protected by ASP.NET Core Data Protection (HMAC tamper-proof)
 *   - Cookie name: .AspNetCore.Mvc.CookieTempDataProvider
 *   - Works in stateless / multi-server deployments without sticky sessions
 *     (because Data Protection keys must be shared across nodes in a farm)
 *   - Practical limit: ~4 KB total. Avoid storing large payloads.
 *
 * SessionStateTempDataProvider:
 *   - Stores TempData in server-side ISession (backed by IDistributedCache)
 *   - No cookie size limit — suitable for larger objects
 *   - Requires AddSession() + UseSession() and, for multi-server, a distributed
 *     cache such as Redis (AddStackExchangeRedisCache)
 *
 * ITempDataProvider (custom):
 *   - Implement the interface and register as a singleton:
 *     builder.Services.AddSingleton<ITempDataProvider, MyProvider>();
 *
 * To switch to session-based TempData (uncomment the block below):
 *
 *   builder.Services.AddSession(options =>
 *   {
 *       options.IdleTimeout        = TimeSpan.FromMinutes(20);
 *       options.Cookie.HttpOnly    = true;
 *       options.Cookie.IsEssential = true;  // GDPR: essential cookies don't need consent
 *   });
 *   builder.Services.AddControllersWithViews()
 *                   .AddSessionStateTempDataProvider(); // replaces cookie provider
 *   // ... then in the pipeline (before UseRouting):
 *   app.UseSession();
 *
 * This project uses CookieTempDataProvider — no extra lines required.
 */

// AddSession + AddSessionStateTempDataProvider are commented out intentionally.
// CookieTempDataProvider is already registered by AddControllersWithViews() above.

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // detailed stack traces; never expose in Production
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // serve wwwroot/ files (CSS, JS, images)
app.UseRouting();       // match URL to controller/action route

// app.UseSession();    // uncomment if switching to SessionStateTempDataProvider

/*
 * Default MVC route: {controller=Products}/{action=Index}/{id?}
 * → / or /Products/Index lands on ProductsController.Index()
 * → /Demo/ViewDataDemo        lands on DemoController.ViewDataDemo()
 */
app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run(); // start Kestrel; blocks until Ctrl+C / SIGTERM

/*
 * QUICK REFERENCE — TempData, ViewData & ViewBag
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * VIEWDATA (ViewDataDictionary — IDictionary<string,object?>)
 *   Controller:
 *     ViewData["Title"]  = "My Page";           // string value (no boxing)
 *     ViewData["Count"]  = 42;                  // int BOXED to object?
 *     ViewData["Tags"]   = new List<string>();  // reference type stored as object?
 *   View:
 *     @ViewData["Title"]                         // renders: calls ToString() on object?
 *     @{ var n = (int)ViewData["Count"]!; }      // explicit unbox — throws if wrong type
 *     @{ var n = ViewData["Count"] as int?; }    // safe nullable unbox
 *     @{ var t = ViewData["Tags"] as List<string>; }  // cast reference type
 *   Pitfall:
 *     ViewData["Missing"] returns null (no KeyNotFoundException)
 *     (int)ViewData["Wrong"]! → InvalidCastException at runtime if stored type differs
 *
 * VIEWBAG (dynamic wrapper — same ViewDataDictionary under the hood)
 *   Controller:
 *     ViewBag.Title    = "My Page";   // == ViewData["Title"] = "My Page"
 *     ViewBag.Category = "Books";     // new dynamic member
 *   View:
 *     @ViewBag.Title                  // dynamic access — null if not set
 *     @ViewBag.Category
 *   Key rule: ViewData["X"] and ViewBag.X always refer to the SAME entry.
 *             Last write wins: ViewData["X"] = "a"; ViewBag.X = "b"; → "b"
 *
 * [VIEWDATA] ATTRIBUTE (controller property auto-synced to ViewData)
 *   [ViewData]
 *   public string? PageTitle { get; set; }
 *   // In action: PageTitle = "Hello" → ViewData["PageTitle"] = "Hello" automatically
 *   // In view:   @ViewData["PageTitle"]  or  @ViewBag.PageTitle
 *
 * TEMPDATA (ITempDataDictionary — survives ONE redirect, then consumed)
 *   Controller (set before redirect):
 *     TempData["Success"] = "Saved!";        // string — survives redirect
 *     TempData["Count"]   = 5;               // primitives supported
 *     return RedirectToAction("Index");
 *   View (after redirect — consumed on first read):
 *     @if (TempData["Success"] is string msg) { <p>@msg</p> }
 *   PEEK (read without consuming):
 *     var msg = TempData.Peek("Success") as string;   // still in TempData after this
 *   KEEP (preserve after consuming):
 *     var msg = TempData["Success"] as string;        // consumed
 *     TempData.Keep("Success");                       // un-marks deletion
 *     TempData.Keep();                                // keep ALL entries
 *
 * [TEMPDATA] ATTRIBUTE (controller property auto-synced to TempData)
 *   [TempData]
 *   public string? StatusMessage { get; set; }
 *   // In POST action: StatusMessage = "Done" → saved to TempData on redirect
 *   // In GET action:  StatusMessage is loaded from TempData (and consumed)
 *
 * SERIALISATION REQUIREMENT (TempData — complex types need manual JSON)
 *   // Store:
 *   TempData["Model"] = System.Text.Json.JsonSerializer.Serialize(productFormModel);
 *   // Retrieve:
 *   var m = System.Text.Json.JsonSerializer.Deserialize<ProductFormModel>(
 *               TempData["Model"] as string ?? "{}");
 *
 * PRG PATTERN (Post-Redirect-Get):
 *   POST /Products/Create → validate → TempData["StatusMessage"] = "..." → 302 Redirect
 *   GET  /Products/Index  → read TempData["StatusMessage"] → render banner
 *   F5 on GET page → no re-POST; just re-fetches the GET page
 *
 * SECTION 14: @MODEL vs VIEWBAG / VIEWDATA — GUIDANCE
 * ─────────────────────────────────────────────────────────────────────────────
 * PREFER @model (strongly-typed ViewModel) when:
 *   - Passing the primary page data (product list, form fields, search results)
 *   - You want IntelliSense, compile-time safety, and rename refactoring
 *   - Using tag helpers: asp-for and asp-validation-for require a typed model
 *   - Writing testable actions: return View(model) is easy to assert in unit tests
 *
 * ACCEPTABLE use of ViewData / ViewBag:
 *   - Layout-level data: page title, breadcrumb, active nav item, theme name
 *   - Data that is peripheral to the page's primary ViewModel
 *   - Select-list items: ViewBag.CategoryList = new SelectList(categories, "Id", "Name")
 *
 * ANTI-PATTERNS to avoid:
 *   ┌──────────────────────────────────────────────────────────────────────────┐
 *   │ Anti-pattern                    Problem                                  │
 *   ├──────────────────────────────────────────────────────────────────────────┤
 *   │ ViewBag for all page data       No IntelliSense; runtime null references  │
 *   │ ViewData["Items"] as list       Boxing/unboxing; cast exceptions           │
 *   │ TempData for primary page data  Consumed on read; breaks on page refresh  │
 *   │ ViewData across redirects       Does NOT survive redirects → use TempData │
 *   │ Large objects in TempData       ~4 KB cookie limit; use session provider  │
 *   │ Untested ViewBag keys in view   Typo in key → silent null, no build error │
 *   └──────────────────────────────────────────────────────────────────────────┘
 *
 * NEXT CHAPTERS:
 *   05. ViewModels & Strongly Typed Views   — ViewModel pattern in depth
 *   06. Model Binding in MVC                — how POST data reaches action params
 *   07. Data Annotations & Validation       — [Required], [Range], ModelState
 *   13. AJAX & Partial Page Updates         — partial views, fetch, Unobtrusive Ajax
 */
