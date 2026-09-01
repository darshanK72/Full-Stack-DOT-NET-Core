/*
 * TOPIC: Model Binding in MVC
 *
 * WHY IT MATTERS:
 *   In ASP.NET Core MVC, every HTTP request arrives as raw bytes — URL segments,
 *   query strings, form fields, JSON bodies, uploaded files. Model binding is the
 *   infrastructure that automatically maps those raw values onto strongly-typed C#
 *   parameters and model objects so your action methods work with real .NET types,
 *   not strings scraped from HttpRequest. ModelState then records any binding or
 *   validation errors, giving you a single, consistent API to inspect before acting
 *   on user data. Getting binding right is the foundation of safe, over-posting-free
 *   MVC applications.
 *
 * WHAT YOU WILL LEARN:
 *    1. Binding sources: [FromForm], [FromRoute], [FromQuery], [FromBody] — purpose,
 *       attributes, and MVC inference rules
 *    2. Binding precedence order in MVC (how sources are tried in sequence)
 *    3. Complex type binding from form fields — dot-notation (Product.Name, Product.Price)
 *    4. Collection binding from form fields — index notation (Items[0].Name, Items[0].Price)
 *    5. Scalar action parameters (int id, string? search) — inference rules
 *    6. [Bind] include list — allow-list to prevent over-posting on model types
 *    7. [BindNever] — opt specific properties out of binding permanently
 *    8. ModelState dictionary — IsValid, Errors, Keys, AddModelError
 *    9. Custom IModelBinder for non-standard value formats (CSV list)
 *   10. IValueProvider — how raw values reach model binders
 *   11. IModelBinderProvider — registering a custom binder globally
 *   12. Over-posting prevention — [Bind] vs dedicated ViewModel approach
 *   13. IFormFile — single and multiple file uploads in MVC forms
 *   14. Binding DateTime, nullable types, and TryUpdateModelAsync
 *
 * CHAPTER MAP:
 *   1. OrderFormModel — complex + collection form binding  → Models/OrderFormModel.cs
 *   2. SearchQuery    — [FromQuery] scalar binding         → Models/SearchQuery.cs
 *   3. FileUploadModel — IFormFile binding                 → Models/FileUploadModel.cs
 *   4. CsvListModelBinder — custom IModelBinder            → Binders/CsvListModelBinder.cs
 *   5. OrdersController — all sources, ModelState          → Controllers/OrdersController.cs
 *   6. Views            — HTML forms wiring the demos      → Views/Orders/
 *   7. MVC registration and binder provider               → Program.cs (this file, below)
 *
 * READ ORDER: follow the Chapter Map 1–6, then return here for Section 7.
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModelBindingMvc.Binders;

/*
 * SECTION 7: MVC REGISTRATION AND GLOBAL BINDER PROVIDER
 *
 * AddControllersWithViews() registers the full MVC stack:
 *   - Value providers (form, route, query string)
 *   - Model binder providers (built-in chain for primitives, complex types, collections,
 *     IFormFile, DateTime, nullable types, …)
 *   - DataAnnotations validator
 *   - Razor view engine
 *
 * Binding source precedence in MVC (no [ApiController], no explicit [From*]):
 * ────────────────────────────────────────────────────────────────────────────
 *   The binder chain tries value providers in this ORDER for each parameter:
 *     1. [FromRoute]   — explicit or inferred from route segment match
 *     2. [FromForm]    — multipart/form-data or x-www-form-urlencoded
 *     3. [FromQuery]   — URL query string
 *   The first provider that returns a value wins. Body ([FromBody]) is NEVER
 *   inferred in MVC — you must add it explicitly. IFormFile always uses [FromForm].
 *
 * Note: [ApiController] changes inference (adds body inference for complex types).
 *   MVC controllers (deriving from Controller, not ControllerBase + [ApiController])
 *   do NOT get that automatic inference — explicit [From*] is preferred for clarity.
 *
 * Global custom binder provider registration:
 *   options.ModelBinderProviders.Insert(0, new CsvListModelBinderProvider())
 *   Inserts at position 0 so it runs before the default collection binder.
 *   The Insert(0, …) position matters: the first provider that returns non-null wins.
 *   See Binders/CsvListModelBinder.cs for the implementation.
 *
 * Alternative: [ModelBinder(BinderType = typeof(CsvListModelBinder))] on the specific
 *   parameter — opt-in per parameter without affecting other List<string> parameters.
 */
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Register the custom CSV binder globally at position 0 so it runs first
    options.ModelBinderProviders.Insert(0, new CsvListModelBinderProvider());
});

var app = builder.Build();

app.UseStaticFiles();   // serves wwwroot assets (CSS, JS)
app.UseRouting();

/*
 * MVC convention routing — {controller}/{action}/{id?}
 * This single route drives all OrdersController actions in this chapter.
 */
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Orders}/{action=Create}/{id?}");

app.Run();

/*
 * ─────────────────────────────────────────────────────────────────────────────
 * QUICK REFERENCE — Model Binding in MVC
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * BINDING SOURCE ATTRIBUTES
 *   [FromForm]              form fields (multipart or url-encoded); implied for IFormFile
 *   [FromRoute]             {segment} in the route template
 *   [FromQuery]             ?key=value query string
 *   [FromBody]              request body (JSON/XML); must be explicit in MVC
 *   [FromHeader(Name="X-")] HTTP request header
 *
 * MVC BINDING PRECEDENCE (no [ApiController])
 *   Route → Form → Query string
 *   [FromBody] is never inferred — add it explicitly
 *   IFormFile → always [FromForm]
 *
 * COMPLEX TYPE BINDING (form/query)
 *   Product.Name, Product.Price    → dot-notation maps to nested object
 *   Items[0].Name, Items[0].Qty    → index notation maps to List<T>
 *   Tags[0]=a&Tags[1]=b            → index notation for scalar collections
 *   tags=a&tags=b                  → repeated key notation (no index needed)
 *
 * OVER-POSTING PREVENTION
 *   [Bind("Name,Price")]           allow-list on action parameter or model
 *   [BindNever]                    opt a property out of binding entirely
 *   ViewModel pattern              separate class with only safe-to-bind properties
 *
 * ModelState
 *   ModelState.IsValid             false if any binding or validation error
 *   ModelState.Errors              per-key error collections
 *   ModelState.Keys                all keys with errors
 *   ModelState.AddModelError(k, m) add a custom business-rule error
 *   ModelState.Remove(key)         clear a key's errors (rare; rebind edge cases)
 *
 * IFormFile
 *   IFormFile                      single uploaded file in [FromForm] context
 *   IList<IFormFile>               multiple files from one or many file inputs
 *   file.FileName                  original client filename (untrusted)
 *   file.Length                    byte count
 *   await file.CopyToAsync(stream) write to a target stream
 *   file.ContentType               MIME type (untrusted — verify server-side)
 *
 * TryUpdateModelAsync
 *   await TryUpdateModelAsync(obj, prefix, x => x.Prop1, x => x.Prop2)
 *   Binds and validates a model object from current form values;
 *   returns false when ModelState.IsValid is false.
 *
 * CUSTOM IModelBinder
 *   Implement BindModelAsync(ModelBindingContext ctx)
 *   ctx.ValueProvider.GetValue(ctx.ModelName) — read raw value
 *   ctx.Result = ModelBindingResult.Success(value)
 *   ctx.Result = ModelBindingResult.Failed()
 *   Pair with IModelBinderProvider.GetBinder(ModelBinderProviderContext ctx)
 *   Register: options.ModelBinderProviders.Insert(0, provider)
 *
 * DATETIME / NULLABLE BINDING
 *   DateTime         parsed with CultureInfo.InvariantCulture; common formats accepted
 *   DateTime?        bound to null when field is absent or empty string
 *   Nullable<T>      same as T? — null when value missing or unparseable (+ error)
 * ─────────────────────────────────────────────────────────────────────────────
 */
