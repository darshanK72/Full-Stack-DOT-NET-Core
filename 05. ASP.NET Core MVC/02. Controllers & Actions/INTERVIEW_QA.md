# Controllers & Actions — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a controller in ASP.NET Core MVC?](#q1-what-is-a-controller-in-aspnet-core-mvc)
2. [Q2. What is an action method?](#q2-what-is-an-action-method)
3. [Q3. What is `IActionResult` and why use it instead of returning raw objects?](#q3-what-is-iactionresult-and-why-use-it-instead-of-returning-raw-objects)
4. [Q4. What is the difference between `View()` and `Json()`?](#q4-what-is-the-difference-between-view-and-json)
5. [Q5. What is the difference between `RedirectToAction` and `Redirect`?](#q5-what-is-the-difference-between-redirecttoaction-and-redirect)
6. [Q6. Why should action methods return `Task<IActionResult>` instead of `async void`?](#q6-why-should-action-methods-return-taskiactionresult-instead-of-async-void)
7. [Q7. How does dependency injection work in MVC controllers?](#q7-how-does-dependency-injection-work-in-mvc-controllers)
8. [Q8. What does the `Controller` base class provide?](#q8-what-does-the-controller-base-class-provide)
9. [Q9. What is `ModelState` and when should an action check it?](#q9-what-is-modelstate-and-when-should-an-action-check-it)
10. [Q10. What are `[HttpGet]` and `[HttpPost]` used for?](#q10-what-are-httpget-and-httppost-used-for)
11. [Q11. What is `[ValidateAntiForgeryToken]` and when is it required?](#q11-what-is-validateantiforgerytoken-and-when-is-it-required)
12. [Q12. What is the difference between `[FromBody]`, `[FromForm]`, and default binding in MVC actions?](#q12-what-is-the-difference-between-frombody-fromform-and-default-binding-in-mvc-actions)
13. [Q13. How are controllers activated per request?](#q13-how-are-controllers-activated-per-request)
14. [Q14. What is the difference between returning `NotFound()` and `BadRequest()`?](#q14-what-is-the-difference-between-returning-notfound-and-badrequest)
15. [Q15. Why should controllers avoid a static service locator?](#q15-why-should-controllers-avoid-a-static-service-locator)
16. [Q16. What is the purpose of constructor injection in controllers?](#q16-what-is-the-purpose-of-constructor-injection-in-controllers)
17. [Q17. What is the difference between conventional routing and attribute routing on a controller?](#q17-what-is-the-difference-between-conventional-routing-and-attribute-routing-on-a-controller)
18. [Q18. Can you implement `IDisposable` on a controller to manage resources? Why or why not?](#q18-can-you-implement-idisposable-on-a-controller-to-manage-resources-why-or-why-not)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is a controller in ASP.NET Core MVC?

**Concepts**
- Controller — class discovered by assembly scanning, containing action methods
- `Controller` base — MVC with views; `ControllerBase` — API-only
- `[ApiController]` — enables automatic model validation, binding inference
- Route, verb, and area attributes drive action selection

**Answer**

A controller is a class marked with `[ApiController]` or inheriting `Controller` or `ControllerBase` that contains action methods handling HTTP requests. ASP.NET Core discovers controllers through assembly scanning and maps routes to their public action methods. MVC controllers inherit `Controller` and gain view helpers like `View()`, `ViewData`, and `TempData`, while API controllers inherit `ControllerBase` and typically use attribute routing with `[ApiController]`. Controllers are registered implicitly — no manual `AddController` per type — when `AddControllersWithViews()` or `AddControllers()` is called. `[Route]`, `[HttpGet]`, and area attributes define how URLs reach specific actions and each controller groups related actions such as `ProductsController` for product CRUD.

---

## Q2. What is an action method?

**Concepts**
- Action method — public instance method selected by routing
- `[NonAction]` — excludes a public method from routing
- Model binding — populates parameters from route, query, form, and body
- Return types: `IActionResult`, `ActionResult<T>`, concrete result types

**Answer**

An action method is a public instance method on a controller that routing selects to handle a request. It accepts bound parameters, performs work, and returns an `IActionResult` that executes to produce the HTTP response. Actions must be public, non-static, and not decorated with `[NonAction]` to be invocable. The method name defaults to the action segment in conventional routing so `/Home/Index` maps to `Index()`. Parameters are populated by model binding from route, query, form, and body sources. Return types include `IActionResult`, `ActionResult<T>`, or concrete results like `ViewResult`, and `[HttpGet]`, `[HttpPost]`, and other verb attributes disambiguate overloaded action names.

---

## Q3. What is `IActionResult` and why use it instead of returning raw objects?

**Concepts**
- `IActionResult` — executable result abstraction, decouples decision from response writing
- `ActionResult<T>` — preserves typed contract and OpenAPI metadata
- Helper methods: `View()`, `RedirectToAction()`, `NotFound()`, `BadRequest()`
- Heterogeneous outcomes under one return type

**Answer**

`IActionResult` is the abstraction for executable results — views, redirects, status codes, files, and JSON — that the framework runs to set the HTTP response. It decouples the action's decision from the mechanics of writing the response so actions can return different outcomes such as 404 versus 200 with a view under one return type. Helper methods like `View()`, `RedirectToAction()`, `NotFound()`, and `BadRequest()` return typed `IActionResult` implementations. Returning a raw object from an API action works with formatters, but returning `ActionResult<T>` preserves OpenAPI metadata and typed contracts. `IActionResult` also enables consistent filter and middleware interaction before the result executes, since filters can inspect and short-circuit before execution runs.

---

## Q4. What is the difference between `View()` and `Json()`?

**Concepts**
- `View()` — Razor view discovery, HTML output via view engine
- `Json()` — serializes object to `application/json` with System.Text.Json
- `View()` to XHR client — produces HTML, breaks JSON parsers
- API projects prefer `Ok(dto)` over `Json()` for consistent status codes

**Answer**

`View()` selects a Razor view, sets `ViewData`, and renders HTML through the view engine with content toward `text/html`. `Json()` serializes an object to JSON using System.Text.Json or configured formatters with `application/json`. `View()` uses view discovery to find `Views/{Controller}/{Action}.cshtml` and supports layouts and partials, while `Json()` is for AJAX endpoints or API-style responses from an MVC controller without a Razor template. Returning `View()` to an XHR client expecting JSON produces HTML — a common integration mistake that surfaces as parse errors in JavaScript. API projects typically use `ControllerBase` with `Ok(dto)` rather than `Json()` for consistent status codes and ProblemDetails error responses.

---

## Q5. What is the difference between `RedirectToAction` and `Redirect`?

**Concepts**
- `RedirectToAction` — URL generated from routing system via controller/action names
- `Redirect(url)` — hardcoded string, breaks when route templates change
- `RedirectToRoute` — targets a named route
- Both return HTTP 302 by default; 301 for permanent redirects

**Answer**

`RedirectToAction` generates a URL from route values using the routing system, so URL changes from route templates stay correct automatically. `Redirect(string url)` sends the client to an explicit absolute or relative URL string, which breaks if route patterns change. `RedirectToAction("Index", "Home", new { id = 5 })` participates in link generation and route constraints, while `Redirect("/Home/Index/5")` hardcodes the path. `RedirectToAction` supports the Post-Redirect-Get pattern after POST without hardcoding URLs in controllers, and `RedirectToRoute` targets a named route instead of controller and action names. Both return HTTP 302 by default or 301 for permanent redirects and leave model binding state behind on the new GET.

---

## Q6. Why should action methods return `Task<IActionResult>` instead of `async void`?

**Concepts**
- `async void` — fire-and-forget, exceptions crash the process
- `Task<IActionResult>` — framework awaits and captures exceptions
- Empty response from `async void` — return value discarded
- `CancellationToken` parameter — propagate request cancellation to I/O

**Answer**

Controller actions must return `Task` or `Task<IActionResult>` so the framework awaits them, captures exceptions, and writes the result to the response. `async void` is fire-and-forget — exceptions propagate to `SynchronizationContext` and may terminate the worker process, and return values are lost. `async void` is reserved for event handlers, not ASP.NET Core action methods. An `async void` action that calls `View(model)` without returning it produces an empty response even without throwing. Proper async actions accept `CancellationToken` and propagate cancellation to I/O calls so slow queries are cancelled when the client disconnects. `Task<IActionResult>` also lets filters and middleware observe completion and status correctly.

---

## Q7. How does dependency injection work in MVC controllers?

**Concepts**
- Constructor injection — standard pattern for controller dependencies
- Per-request controller activation — scoped services aligned to request scope
- `[FromServices]` — parameter-level DI resolution, less common
- Missing registrations fail at startup or first request — not silent nulls

**Answer**

ASP.NET Core resolves controllers through the DI container, injecting constructor parameters registered in `Program.cs`. The key is to use constructor injection rather than `HttpContext.RequestServices` lookups, since constructor injection makes dependencies explicit and verifiable. Services are registered with lifetimes — singleton, scoped (typical for `DbContext`), or transient — and the framework activates a new controller instance per request with scoped dependencies aligned to the request scope. `[FromServices]` on a parameter also resolves from DI but constructor injection is the standard pattern. `[ActivatorUtilitiesConstructor]` marks which constructor to use when multiple exist. Missing registrations fail at startup or first request with `InvalidOperationException`, not silent nulls.

---

## Q8. What does the `Controller` base class provide?

**Concepts**
- `Controller` extends `ControllerBase` — adds `View()`, `TempData`, `ViewBag`, `ViewData`
- `ControllerBase` — `Ok()`, `BadRequest()`, `NotFound()`, HTTP context access
- JSON-only APIs should use `ControllerBase` — avoids view infrastructure overhead

**Answer**

The `Controller` base class extends `ControllerBase` with view-related helpers and context properties for MVC scenarios. It adds methods like `View()`, `PartialView()`, and access to `ViewData`, `ViewBag`, `TempData`, and `ModelState`. `ControllerBase` provides `Ok()`, `BadRequest()`, `NotFound()`, and HTTP context access for API-style results, and both expose `HttpContext`, `Request`, `Response`, `User`, and `Url` helper properties. `ModelState` on the controller tracks binding and validation errors for the current request. JSON-only APIs should inherit `ControllerBase` to avoid pulling in view infrastructure, since the view helpers add overhead that serves no purpose in an API controller.

---

## Q9. What is `ModelState` and when should an action check it?

**Concepts**
- `ModelState` — dictionary of binding and validation results by property name
- `ModelState.IsValid` — must be checked on POST before any persistence
- `ModelState.AddModelError` — manual business rule failures
- Request-scoped — does not survive redirect without TempData

**Answer**

`ModelState` is a dictionary of model binding and validation results keyed by property name, populated during model binding from Data Annotations and manual errors. Actions must check `ModelState.IsValid` before persisting data on POST requests, since invalid annotation validation adds errors automatically during binding but only takes effect if the action checks before saving. Manual business rule failures call `ModelState.AddModelError("Property", "message")`. On failure, the action returns `View(model)` to redisplay the form with validation tag helpers showing errors. Skipping the check allows invalid or malicious input to reach the database despite client validation being in place. `ModelState` is request-scoped and does not survive redirects without TempData or a second validation pass.

---

## Q10. What are `[HttpGet]` and `[HttpPost]` used for?

**Concepts**
- HTTP verb attributes — disambiguate overloaded action names
- GET — safe, idempotent, reads state; POST — mutates state
- `[HttpPost]` paired with `[ValidateAntiForgeryToken]` for form submissions
- `AmbiguousMatchException` — result of two same-name actions without verb disambiguation

**Answer**

These attributes restrict which HTTP verbs can invoke an action, disambiguating overloads with the same name and enforcing RESTful verb semantics. Without verb attributes, two actions named `Create` cause `AmbiguousMatchException` since routing cannot choose between them. Browsers follow links with GET so state changes must use POST, PUT, PATCH, or DELETE. `[HttpPost]` pairs with `[ValidateAntiForgeryToken]` on form submissions that change data to prevent CSRF. `[AcceptVerbs("GET", "POST")]` allows multiple verbs on one action when intentional, and attribute routing uses HTTP method constraints alongside route templates for action selection.

---

## Q11. What is `[ValidateAntiForgeryToken]` and when is it required?

**Concepts**
- Antiforgery — prevents CSRF on cookie-authenticated POST actions
- Form tag helper — renders `__RequestVerificationToken` hidden field automatically
- AJAX POST — must send token via header manually
- `[AutoValidateAntiforgeryToken]` — validates all unsafe verbs on the controller

**Answer**

`[ValidateAntiForgeryToken]` validates that a POST request includes a matching antiforgery token, preventing cross-site request forgery attacks that trick a logged-in user's browser into submitting unwanted forms. It is required on every state-changing POST action that uses cookie-based authentication because cookies are sent automatically by browsers on same-origin requests, making CSRF possible. The form tag helper automatically renders a hidden `__RequestVerificationToken` field and the server compares it with the cookie token issued by antiforgery middleware. AJAX POSTs must send the token via header `RequestVerificationToken` or form field manually. `[AutoValidateAntiforgeryToken]` on the controller applies validation to all unsafe verbs so individual actions need not repeat the attribute.

---

## Q12. What is the difference between `[FromBody]`, `[FromForm]`, and default binding in MVC actions?

**Concepts**
- `[FromBody]` — deserializes JSON from request body stream
- `[FromForm]` — reads `application/x-www-form-urlencoded` or `multipart/form-data`
- Default binding — combines route, query, and form values by convention
- Browser HTML forms — not JSON, so `[FromBody]` leaves model empty

**Answer**

Binding source attributes tell model binders where to read values. `[FromBody]` deserializes JSON from the request body using the configured input formatter, `[FromForm]` reads form fields, and default binding combines route, query, and form values by convention. Standard HTML forms post `application/x-www-form-urlencoded` or `multipart/form-data` — not JSON — so `[FromBody]` leaves the model empty and the action runs with default values. `[FromForm]` explicitly binds form fields for POST actions with complex models. `[FromRoute]` and `[FromQuery]` target route segments and query string parameters respectively. `[FromBody]` typically allows only one parameter per action because the body is a single stream. Default binding without attributes works for simple parameters and MVC form posts in most scenarios.

---

## Q13. How are controllers activated per request?

**Concepts**
- `IControllerActivator` — creates one controller instance per request
- `ActivatorUtilities.CreateInstance` — resolves constructor parameters from request scope
- Not singletons — scoped services align with per-request lifetime
- `IDisposable` on controllers — unreliable, not the recommended cleanup pattern

**Answer**

ASP.NET Core uses `IControllerActivator` (default: `DefaultControllerActivator`) to create a controller instance for each request by resolving the type from DI with constructor dependencies. The instance lives for the duration of the action invocation and is eligible for garbage collection afterward. Controllers are not singletons, so scoped services like `DbContext` align correctly with per-request activation. `ActivatorUtilities.CreateInstance` fills constructor parameters from the request's service provider. Custom activators can wrap controllers with logging or interceptors but rarely replace default behavior. `IDisposable` on controllers is not the recommended resource cleanup pattern — scoped services should own and dispose expensive resources instead.

---

## Q14. What is the difference between returning `NotFound()` and `BadRequest()`?

**Concepts**
- `NotFound()` — HTTP 404, valid identifier but resource does not exist
- `BadRequest()` — HTTP 400, client sent malformed or invalid input
- Inverting these breaks API client retry logic and monitoring alerts
- `[ApiController]` auto-translates invalid model state to 400

**Answer**

`NotFound()` returns HTTP 404, meaning the requested resource does not exist at the given identifier. `BadRequest()` returns HTTP 400, meaning the client sent malformed or invalid input that prevents processing. The key distinction is that a valid ID format with no matching database row should return `NotFound()`, while a null, negative, or whitespace ID when an ID is required should return `BadRequest()` or a validation problem. Inverting these confuses API clients, monitoring alerts, and caching behavior — a 404 implies retry might succeed later, while a 400 implies the client must fix its request. `[ApiController]` automatically translates invalid model state to 400 with validation details, and MVC API controllers should return `ProblemDetails` for consistent error bodies.

---

## Q15. Why should controllers avoid a static service locator?

**Concepts**
- Static service locator — hidden dependencies, global mutable state
- Captive dependency risk — resolving scoped services from statics
- Constructor injection — explicit, verifiable, analyzer-compatible
- `HttpContext.RequestServices.GetService<T>()` — masks missing registrations

**Answer**

A static service locator hides dependencies, requires global mutable state, bypasses constructor injection, and breaks scoped lifetimes — especially for `DbContext`. Hidden dependencies make unit testing impossible without mutating global state before each test, and resolving scoped services from singletons or static fields causes captive dependency bugs and stale contexts. `HttpContext.RequestServices.GetService<T>()` in actions masks missing registrations until runtime rather than failing at startup. Constructor injection makes dependencies explicit, verifiable, and compatible with analyzer tools. Static locators were an anti-pattern before Core and remain one in ASP.NET Core 8 since the DI container already provides per-request resolution without static access.

---

## Q16. What is the purpose of constructor injection in controllers?

**Concepts**
- Constructor injection — explicit dependency declaration
- Fail-fast — missing registrations detected at startup or first request
- Mock-friendly — unit tests pass fake implementations through constructor
- `[ActivatorUtilitiesConstructor]` — selects injectable constructor when multiple exist

**Answer**

Constructor injection declares all dependencies explicitly in the controller's constructor, letting the DI container wire them at activation time. The reason this matters is that dependencies appear as private readonly fields set once at construction, so the compiler and IDE surface required services without hidden lookups in action bodies. Integration and unit tests pass fake implementations through the constructor without needing an HTTP context. ASP.NET Core resolves the graph automatically for each request-scoped controller instance and fails fast with a clear error when a service is not registered, rather than producing null references at arbitrary runtime points. `[ActivatorUtilitiesConstructor]` selects the injectable constructor when multiple exist.

---

## Q17. What is the difference between conventional routing and attribute routing on a controller?

**Concepts**
- Conventional routing — `{controller}/{action}/{id?}` pattern in `Program.cs`
- Attribute routing — `[Route]` templates directly on controllers and actions
- Conventional routes use naming conventions; attribute routes use explicit templates
- Both can coexist; attribute routes take precedence when both match

**Answer**

Conventional routing maps URLs to `{controller}/{action}/{id?}` patterns registered in `Program.cs` with `MapControllerRoute`. Attribute routing places `[Route]` templates directly on controllers and actions, enabling precise REST-style URLs independent of class names. Conventional routing uses naming conventions so `ProductsController.Index` maps to `/Products/Index`, while attribute routing supports templates like `[HttpGet("api/products/{id:int}")]` on individual actions. Attribute-routed controllers often use `[Route("[controller]")]` at the class level with verb attributes on actions. Both can coexist in the same application and attribute routes take precedence when both match. Tag helpers generate URLs from route names and attribute templates when `asp-controller` and `asp-action` are used.

---

## Q18. Can you implement `IDisposable` on a controller to manage resources? Why or why not?

**Concepts**
- Controller disposal — not part of the documented cleanup contract
- Scoped services — DI disposes them at end of request reliably
- `IResourceFilter` — alternative for action-scoped acquire/release
- Native handles in controller fields — leak or double-dispose risk

**Answer**

Implementing `IDisposable` on a controller is unreliable because ASP.NET Core does not guarantee deterministic disposal timing for controller instances as part of the documented cleanup contract. Unmanaged or expensive resources should live in scoped services that DI disposes at the end of the request. Controller lifetime is tied to action invocation but disposal is not the same as scoped service disposal — native handles or connections in controller fields risk leaks or double-dispose under concurrency. I register `IDisposable` or `IAsyncDisposable` implementations as scoped services and inject them instead, since DI disposes scoped services created in the request scope automatically after the response completes. `IResourceFilter` can wrap acquire and release around an action if lifetime must match action boundaries precisely.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Concepts**
- Pricing and discount calculations in `.cshtml` — no unit test coverage
- Authorization checks in Razor — bypassable by alternate routes

**Answer**

Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently. Views should render data the controller or ViewModel already prepared. Calculations in Razor cannot be tested independently and often diverge from API or batch logic. Razor should be limited to presentation formatting, not business decisions.

---

#### Gotcha 2. EF entities passed directly to views

**Concepts**
- Lazy-loaded navigations — unexpected queries during rendering
- Over-posting — mass assignment via unlocked navigation properties

**Answer**

Binding and displaying EF Core entities exposes navigation properties, causes over-posting on POST, and couples the UI to the database schema. Lazy-loaded navigations can trigger unexpected queries during rendering and mass assignment can update properties the user should not control such as `IsAdmin`. The fix is dedicated ViewModels with only the fields the view needs.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Concepts**
- Browser forms — `application/x-www-form-urlencoded`, not JSON
- `[FromBody]` uses JSON input formatter — leaves model empty silently

**Answer**

Standard browser forms send `application/x-www-form-urlencoded`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model empty while the action runs with default values. I remove `[FromBody]` for conventional form POSTs and use it only when the client sends JSON with the correct Content-Type.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Concepts**
- Client-side validation — bypassable by direct POST
- Server-side `ModelState.IsValid` — mandatory security gate

**Answer**

Client-side validation is bypassable by direct POST. Server-side validation is mandatory before any persist, redirect, or side effect. I always gate POST actions with `if (!ModelState.IsValid) return View(model);`. Missing server validation is a security defect regardless of client script presence.

---

#### Gotcha 5. `return View()` after successful POST

**Concepts**
- Browser refresh after `return View()` — resubmits POST body
- PRG — `return RedirectToAction` after successful mutation

**Answer**

Returning the same view after a successful POST causes duplicate submission when the user refreshes the page. The fix is Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create or update. Flash success messages go via TempData on the redirect target.

---

#### Gotcha 6. `ModelState` after redirect

**Concepts**
- `ModelState` — request-scoped, does not survive redirect
- On failure — `return View(model)` with errors inline

**Answer**

`ModelState` is request-scoped and does not survive `RedirectToAction`. The correct pattern is to redirect only on success and on validation failure return `View(model)` with errors inline. To survive redirect on failure, serialize errors to TempData or use PRG with a form-specific error cache.

---

#### Gotcha 7. TempData read twice in layout and view

**Concepts**
- TempData consumed on first read by default
- `TempData.Peek` — read without consuming

**Answer**

TempData is consumed on first read by default. If the layout reads a flash message, the view sees nothing unless `Peek()` or `Keep()` is used. I prefer a single consumption point — typically the layout or a dedicated partial, not both. Cookie-based TempData has size limits.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Concepts**
- `[Area("AreaName")]` — required for area route discovery
- Without attribute — controller treated as root, returns 404

**Answer**

Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong route. Every area controller must declare `[Area("AreaName")]` matching its folder. Area routing is registered separately with the `{area:exists}` constraint.

---

#### Gotcha 9. Link generation without `asp-area`

**Concepts**
- Tag Helpers — default to current area context
- Cross-area links — require explicit `asp-area` and `asp-controller`

**Answer**

Tag Helpers default to the current area context when generating URLs. Links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment. Cross-area links require both `asp-area` and `asp-controller`. The same rule applies to `Url.Action` with `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Concepts**
- Unchecked checkbox posts nothing — model binding sets `bool` to `false`
- `[Required]` on `bool` never fails — `false` is a valid non-null value

**Answer**

An unchecked checkbox posts nothing and model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid value. I use `bool?` with `[Required]` to require explicit true selection for consent checkboxes.

---

#### Gotcha 11. Collection binding with gap indices

**Concepts**
- Model binder expects contiguous zero-based indices
- Gap indices — truncation or misalignment after row deletion

**Answer**

Deleting a form row leaving indices such as `Lines[0]` and `Lines[2]` breaks model binder alignment. I reindex client-side after row deletion so indices are contiguous starting at zero, or implement a custom `IModelBinder` that tolerates non-contiguous indices.

---

#### Gotcha 12. `@Html.Raw` with user content

**Concepts**
- Default Razor `@` — HTML-encodes, prevents XSS
- `@Html.Raw` — bypasses encoding, executes injected script

**Answer**

Default Razor encoding prevents XSS. `@Html.Raw(Model.UserComment)` renders attacker-supplied script if the content is not sanitized server-side. I prefer `@Model.UserComment` (auto-encoded) for plain text or sanitize with a trusted HTML sanitizer library before using Raw.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Concepts**
- Form tag helpers — emit antiforgery token automatically
- `fetch` / jQuery AJAX — must send token manually

**Answer**

Form tag helpers emit antiforgery tokens automatically but `fetch` and jQuery AJAX must manually send `RequestVerificationToken` as a header or form field. `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe verb methods. I do not disable antiforgery on MVC cookie-auth endpoints — I add the token instead.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Concepts**
- Hubs not registered in DI for direct injection
- `IHubContext<THub>` — singleton proxy for broadcasting

**Answer**

Hubs are not registered in DI for direct injection. Injecting a concrete `Hub` fails activation or produces an instance without connection context. The correct pattern is `IHubContext<THub>`, which is a singleton proxy registered by `AddSignalR()`.

---

#### Gotcha 15. SignalR scale-out without backplane

**Concepts**
- Sticky sessions — per-client affinity, not cross-instance event routing
- `AddStackExchangeRedis` or `AddAzureSignalR` — multi-instance fan-out

**Answer**

Sticky sessions alone do not fan-out events across server instances. A controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users on instance B. Multi-node deployments need `AddStackExchangeRedis` or `AddAzureSignalR`.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this action from a code review. The developer says "it compiles and works in Swagger." What is wrong with the return type and result handling?

```csharp
public class ProductsController : Controller
{
    private readonly IProductService _products;

    public ProductsController(IProductService products) => _products = products;

    [HttpGet("{id:int}")]
    public object Get(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid product id");
        var product = _products.GetById(id);
        if (product is null)
            return NotFound();
        return product;
    }
}
```

*(Route prefix: `[Route("api/[controller]")]` on the controller.)*

**Concepts**
- `object` return type — loses typed contract and OpenAPI metadata
- `Controller` base for JSON route — wrong base class, should be `ControllerBase`
- `BadRequest(string)` — plain text 400, not ProblemDetails
- `ActionResult<ProductDto>` — correct return type for API endpoint

**Answer**

Returning `object` compiles because `BadRequest`, `NotFound`, and the entity all coerce to `object`, but the result is a loss of typed contracts, OpenAPI metadata, and predictable content negotiation. Swagger cannot infer response schemas, so generated clients lose type information. Inheriting `Controller` rather than `ControllerBase` pulls in view infrastructure that is unused and inappropriate for a JSON-only route. `BadRequest(string)` returns plain text rather than `ProblemDetails`, which breaks API client error parsing.

The fixes in priority order: change the base to `ControllerBase`, add `[ApiController]`, and return `ActionResult<ProductDto>`.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public ActionResult<ProductDto> Get(int id)
    {
        if (id <= 0) return BadRequest();
        var product = _products.GetById(id);
        return product is null ? NotFound() : Ok(product.ToDto());
    }
}
```

Map the entity to a DTO, use `Ok(dto)`, `NotFound()`, and `ValidationProblem()` as appropriate, and add `[ProducesResponseType]` attributes for OpenAPI and contract tests.

---

#### Q2. (R) A junior developer converts synchronous actions to async. After deploy, unhandled exceptions crash the worker process on slow queries. Review the action.

```csharp
public class OrdersController : Controller
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    [HttpGet("details/{id:int}")]
    public async void Details(int id)
    {
        var order = await _orders.GetAsync(id);
        View(order);
    }
}
```

**Concepts**
- `async void` — exceptions propagate to SynchronizationContext, crash process
- `View(order)` not returned — client gets empty or wrong response
- Missing `CancellationToken` — slow queries not cancelled on client disconnect

**Answer**

Controller actions must never be `async void`. The framework cannot await an `async void` method, so exceptions propagate to the `SynchronizationContext` as unhandled faults and can tear down the process. The `View(order)` call is not returned, so the client receives an empty or wrong response even when no exception occurs — calling a result factory method without `return` has no effect.

The fix: change the signature to `public async Task<IActionResult> Details(int id, CancellationToken ct)`, await with the token so slow queries are cancelled when the client disconnects, and return the result: `return order is null ? NotFound() : View(order);`. Adding null guard before the view call also prevents a potential runtime error inside the view engine.

---

#### Q3. (R) Review constructor and field usage on this MVC controller. Index works; other actions intermittently throw `NullReferenceException`.

```csharp
public class InvoicesController : Controller
{
    private IInvoiceService? _invoices;

    public InvoicesController() { }

    public IActionResult Index()
    {
        _invoices = HttpContext.RequestServices.GetRequiredService<IInvoiceService>();
        return View(_invoices.GetOpen());
    }

    [HttpPost]
    public IActionResult Approve(int id)
    {
        _invoices!.Approve(id);
        return RedirectToAction(nameof(Index));
    }
}
```

**Concepts**
- Service locator in action — hidden dependency, `_invoices` set only when `Index` runs first
- Parameterless constructor — bypasses DI constructor injection
- `NullReferenceException` on POST without visiting Index first
- Constructor injection as fix — explicit, testable, always initialized

**Answer**

`_invoices` is only set when `Index` runs, so any direct `POST /Invoices/Approve` without first visiting Index sees a null field. The `HttpContext.RequestServices.GetRequiredService` pattern is the service-locator anti-pattern — it hides the dependency from the constructor, makes unit testing impossible without mutating global state, and produces null fields rather than failing at startup. The parameterless constructor is the root cause since DI is never given the opportunity to provide the service.

The fix: add `public InvoicesController(IInvoiceService invoices) => _invoices = invoices;`, remove the parameterless constructor, and delete all `RequestServices` lookups from actions. Register `IInvoiceService` as scoped in `Program.cs` and verify with a controller unit test that passes a mock.

---

#### Q4. (R) A legacy module still resolves dependencies through a static locator. Review and explain what breaks for testing and lifetimes.

```csharp
public static class ServiceLocator
{
    public static IServiceProvider Provider { get; set; } = default!;
}

public class ReportsController : Controller
{
    public IActionResult Monthly()
    {
        var db = ServiceLocator.Provider.GetRequiredService<AppDbContext>();
        var rows = db.Reports.FromSqlRaw("EXEC usp_MonthlyReport").ToList();
        return View(rows);
    }
}
```

**Concepts**
- Static `ServiceLocator.Provider` — global mutable state, race conditions in tests
- `AppDbContext` resolved manually — wrong scope, concurrent request issues
- Raw SQL in controller — no repository boundary, hard to mock
- Constructor injection as fix

**Answer**

A static `ServiceLocator` requires mutating `ServiceLocator.Provider` before each test and introduces global mutable state that causes race conditions in parallel test runs. The `AppDbContext` resolved from the static provider may capture a wrong scope — if `Provider` is the root provider rather than the request scope, `DbContext` becomes effectively a singleton, which is thread-unsafe and causes stale change tracking across concurrent requests. Raw SQL in the controller bypasses any repository abstraction, making the action impossible to unit test without a real database.

The fix: delete `ServiceLocator`; inject `IReportService` or `AppDbContext` through the constructor; move the SQL to a repository or service; return a view model from the service layer so the controller returns `View(model)` only.

---

#### Q5. (R) The same controller serves a Razor admin page and a React widget on the dashboard. QA reports "HTML instead of JSON" for one endpoint. Review.

```csharp
[Route("admin/products")]
public class AdminProductsController : Controller
{
    private readonly IProductService _products;

    public AdminProductsController(IProductService products) => _products = products;

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var product = _products.GetById(id);
        if (product is null)
            return NotFound();
        return View("ProductDetail", product);
    }

    [HttpGet("lookup/{id:int}")]
    public IActionResult Lookup(int id)
    {
        var product = _products.GetById(id);
        return Json(product);
    }
}
```

*(React client calls `GET /admin/products/42` with `Accept: application/json`.)*

**Concepts**
- `Get` returns `View` for a route the React client calls — HTML instead of JSON
- Two different route templates (`{id}` vs `lookup/{id}`) — easy integration mismatch
- `Controller` base + `View()` — ignores `Accept` header, always returns HTML

**Answer**

`GET /admin/products/42` matches the `Get` action which always returns a Razor `View`, so the React client receives an HTML document regardless of the `Accept: application/json` header — MVC does not content-negotiate away from `View()`. The client never reaches `Lookup` because it is at a different route template (`lookup/{id}`).

The core issue is mixing HTML and JSON concerns in one controller with routes that look similar but serve completely different clients. The fix is to split concerns: keep `AdminProductsController : Controller` for the Razor admin page and create a separate `AdminProductsApiController : ControllerBase` with `[ApiController]` for JSON routes, or change the React client to call `/admin/products/lookup/42` consistently and document the routes. For dual-format endpoints, explicit formatters or separate actions are required — returning `View` to XHR clients is always a routing or design bug, not a serialization tweak.

---

#### Q6. (R) Review this account controller. Security finds login CSRF exposure and duplicate route matches in integration tests.

```csharp
public class AccountController : Controller
{
    public IActionResult Login() => View();

    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        // sign-in logic
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        // sign-out logic
        return RedirectToAction("Login");
    }
}
```

**Concepts**
- Missing `[HttpGet]` / `[HttpPost]` — ambiguous action selection
- No `[ValidateAntiForgeryToken]` on POST Login — CSRF vulnerability
- GET-accessible `Logout` — CSRF logout via prefetch or forged link

**Answer**

Without `[HttpPost]` on the POST overload, both `Login` actions match both GET and POST requests, causing `AmbiguousMatchException` or routing the wrong action. Without `[ValidateAntiForgeryToken]` on the mutating action, an attacker can submit a forged login or logout. `Logout` accessible via GET means a prefetch or embedded image URL triggers sign-out.

The fixes: add `[HttpGet]` to the no-parameter `Login` overload and `[HttpPost][ValidateAntiForgeryToken]` to the model-accepting overload; add `[HttpPost][ValidateAntiForgeryToken]` to `Logout` so it only executes on an intentional form submit. The Razor form tag helper renders the antiforgery token automatically, so no code change is needed in the view other than confirming it uses `<form method="post">` via the tag helper.

---

#### Q7. (R) Review this create action. Invalid posts persist bad data; QA sees no validation messages on the form.

```csharp
public class CustomersController : Controller
{
    private readonly AppDbContext _db;

    public CustomersController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public IActionResult Create(CustomerViewModel model)
    {
        _db.Customers.Add(new Customer
        {
            Name = model.Name,
            Email = model.Email
        });
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}
```

*(ViewModel has `[Required]` on `Name` and `[EmailAddress]` on `Email`.)*

**Concepts**
- Missing `ModelState.IsValid` check — invalid data persisted despite annotations
- No `return View(model)` on failure — user sees redirect, not validation errors
- Direct `_db` in controller — thick controller, no service boundary

**Answer**

The POST action never checks `ModelState.IsValid` before calling `SaveChanges`, so Data Annotations on `CustomerViewModel` run during binding and populate `ModelState` with errors, but those errors are ignored and the invalid entity is inserted anyway. The user is redirected to Index rather than seeing validation messages because there is no `return View(model)` path.

The fix: add `if (!ModelState.IsValid) return View(model);` before any persistence, add `[ValidateAntiForgeryToken]`, and move persistence to `ICustomerService.Create(model)` to keep the controller thin.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(CustomerViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    _customers.Create(model);
    return RedirectToAction(nameof(Index));
}
```

---

#### Q8. (R) Attribute routing tests fail with `AmbiguousMatchException`. Review these action signatures.

```csharp
[Route("catalog")]
public class CatalogController : Controller
{
    [HttpGet("items")]
    public IActionResult Items() => View(_service.GetFeatured());

    [HttpGet("items")]
    public IActionResult ItemsByCategory(string category)
        => View("Items", _service.GetByCategory(category));
}
```

**Concepts**
- Duplicate `[HttpGet("items")]` — `AmbiguousMatchException` at startup or first request
- Parameter presence alone does not create distinct attribute routes
- Merge into one action or use different route templates

**Answer**

Two actions share the identical HTTP method and route template `GET catalog/items`. Attribute routing cannot distinguish them by parameter presence alone when templates are identical strings, so it throws `AmbiguousMatchException`. The `category` parameter being optional does not create a separate route because route templates — not method signatures — determine routing.

The fixes: merge into one action with `Items(string? category)` where null returns featured and non-null filters; or separate routes such as `[HttpGet("items")]` and `[HttpGet("items/category/{category}")]`. Route constraint tests should be run at startup and in integration tests hitting both URLs to catch this before deploy.

---

#### Q9. (R) Review this controller pulled from a tutorial. The team wants thin controllers and unit tests without a database.

```csharp
public class EmployeesController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        using var conn = new SqlConnection(_config.GetConnectionString("HrDb"));
        conn.Open();
        using var cmd = new SqlCommand("SELECT Id, Name, Dept FROM Employees", conn);
        var list = new List<Employee>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new Employee { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        return View(list);
    }
}
```

*(Constructor with `IConfiguration _config` omitted for brevity.)*

**Concepts**
- ADO.NET in controller action — untestable without SQL Server
- `IConfiguration` for connection string in controller — infrastructure in presentation layer
- Synchronous I/O on thread pool — blocks under concurrent requests

**Answer**

The controller owns ADO.NET connection management, SQL, and mapping in the action, which means any meaningful test requires SQL Server. `IConfiguration` for a connection string in the controller leaks infrastructure concern into the presentation layer and bypasses any pooling strategy visibility. Synchronous ADO.NET blocks a thread pool thread for the duration of the query, reducing concurrency under load.

The fixes: introduce `IEmployeeRepository.GetAllAsync()` registered scoped in DI; the controller becomes `return View(await _employees.GetAllAsync(ct));` — one line. Unit test the repository with an in-memory fake and integration test the SQL separately, so controller tests never touch a database.

---

#### Q10. (P) An internal MVC app accepts POST forms for role changes. Pen test reports missing CSRF protection on one action. Review the pattern and what production setup is required.

```csharp
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult EditUser(int id) => View(_users.Get(id));

    [HttpPost]
    public IActionResult EditUser(EditUserViewModel model)
    {
        _users.UpdateRole(model.UserId, model.Role);
        return RedirectToAction(nameof(Index));
    }
}
```

*(Razor form uses `<form asp-action="EditUser">` but the POST action has no antiforgery attribute.)*

**Concepts**
- `[ValidateAntiForgeryToken]` on POST — required for cookie-auth state-changing actions
- Razor form tag helper — renders token automatically
- AJAX POST — must send `RequestVerificationToken` header manually
- `[AutoValidateAntiforgeryToken]` — controller-level all-unsafe-verb protection

**Answer**

State-changing POST actions with cookie authentication must carry `[ValidateAntiForgeryToken]`. The Razor `<form asp-action="EditUser" method="post">` tag helper renders `@Html.AntiForgeryToken()` automatically, so the token is present in the form but the server is not validating it because the attribute is missing from the POST action. An attacker can craft a page that submits to `/Admin/EditUser` and any admin visiting the attacker's page will unknowingly trigger a role change.

I add `[ValidateAntiForgeryToken]` on the POST `EditUser` action. For AJAX POSTs, I send the `RequestVerificationToken` header with the token from the hidden field. I optionally add `[AutoValidateAntiforgeryToken]` at the controller level to protect all unsafe verbs automatically. I confirm `[HttpGet]` is on the GET overload and `[HttpPost]` on the POST overload so role changes cannot be triggered via GET. Antiforgery does not replace authorization — I still verify the admin role in the action or via a policy attribute.

---

#### Q11. (D) API consumers report inconsistent status codes: missing resources return 400, malformed ids return 404. Review this action and explain the correct HTTP semantics.

```csharp
[Route("api/inventory")]
public class InventoryController : Controller
{
    [HttpGet("{sku}")]
    public IActionResult Get(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return NotFound();

        if (sku.Length > 20)
            return BadRequest("SKU too long");

        var item = _inventory.Find(sku);
        if (item is null)
            return BadRequest($"Unknown SKU: {sku}");
        return Json(item);
    }
}
```

**Concepts**
- 400 Bad Request — client sent invalid or malformed input
- 404 Not Found — valid identifier but resource does not exist
- Inverted status codes — breaks client retry logic and monitoring
- `[ApiController]` + `ControllerBase` — ProblemDetails for consistent API errors

**Answer**

Status codes are inverted — empty or whitespace `sku` is a client error (400 Bad Request) since the client sent invalid input, while an unknown SKU with a valid format is a not-found case (404 Not Found).

| Condition | Wrong response | Correct response |
|---|---|---|
| `sku` null/whitespace | 404 NotFound | 400 Bad Request — malformed input |
| `sku` too long | 400 Bad Request | 400 Bad Request — correct |
| Valid format, item missing | 400 Bad Request | 404 Not Found — valid id, no resource |
| Item exists | 200 + JSON | 200 OK — correct |

The key distinction is: **invalid identifier format** → 400; **valid identifier, no resource** → 404. A 404 implies retry might succeed (the resource could be created later) while a 400 implies the client must fix its request. I also change the base to `ControllerBase` with `[ApiController]` and use `ProblemDetails` via `ValidationProblem()` rather than plain-text `BadRequest(string)` so API clients get structured error bodies.

---

#### Q12. (M) A developer implements `IDisposable` on a controller to release an expensive native handle. Under load, handles leak and `_handle` is sometimes already disposed. Explain controller activation, lifetime, and the correct pattern.

```csharp
public class ScannerController : Controller, IDisposable
{
    private readonly NativeScanner _handle = new();

    public ScannerController() { }

    public IActionResult Scan()
    {
        var result = _handle.ReadBarcode();
        return Json(result);
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}
```

*(Assume default `ControllerActivator` and no custom `IControllerFactory`.)*

**Concepts**
- Controller `IDisposable` — not part of documented cleanup contract
- `NativeScanner` in field initializer — created per construction, timing of disposal undefined
- Scoped service as fix — DI disposes at end of request reliably
- `IResourceFilter` — alternative for action-scoped acquire/release

**Answer**

ASP.NET Core does not guarantee that `Dispose()` on a controller runs promptly or at all as part of the documented cleanup contract. The controller instance lifetime is tied to action invocation but disposal timing differs from scoped service disposal, which is reliable and happens at end-of-request. `NativeScanner` is created in a field initializer per controller construction — if `Dispose` is never called, the handle leaks; if it is called more than once under concurrency, `ObjectDisposedException` occurs.

The correct pattern is to wrap `NativeScanner` in a scoped service `INativeScanner` registered with `AddScoped`, implement `IAsyncDisposable` or `IDisposable` on the service, and inject it into the controller. DI disposes scoped services created in the request scope automatically after the response completes.

```csharp
// Register: services.AddScoped<INativeScanner, NativeScanner>();
public class ScannerController : Controller
{
    private readonly INativeScanner _scanner;
    public ScannerController(INativeScanner scanner) => _scanner = scanner;

    public IActionResult Scan() => Json(_scanner.ReadBarcode());
}
```

`IResourceFilter` is an alternative if the resource lifetime must match action boundaries precisely rather than the full request scope. I never hold unmanaged resources directly on `Controller` subclasses.
