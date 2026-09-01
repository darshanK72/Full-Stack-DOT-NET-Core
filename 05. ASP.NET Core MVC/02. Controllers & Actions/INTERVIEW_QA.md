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

What is a controller in ASP.NET Core MVC?

**Answer:** A controller is a class marked with `[ApiController]` or inheriting `Controller`/`ControllerBase` that contains action methods handling HTTP requests. ASP.NET Core discovers controllers through assembly scanning and maps routes to their public action methods.

- MVC controllers inherit `Controller`, gaining view helpers like `View()`, `ViewData`, and `TempData`.
- API controllers inherit `ControllerBase` and typically use attribute routing with `[ApiController]`.
- Controllers are registered implicitly — no manual `AddController` per type — when `AddControllersWithViews()` is called.
- `[Route]`, `[HttpGet]`, and area attributes define how URLs reach specific actions.
- Each controller groups related actions (e.g., `ProductsController` for product CRUD).

---

## Q2. What is an action method?

What is an action method?

**Answer:** An action method is a public instance method on a controller that routing selects to handle a request. It accepts bound parameters, performs work, and returns an `IActionResult` that executes to produce the HTTP response.

- Actions must be public, non-static, and not decorated with `[NonAction]` to be invocable.
- Method name defaults to the action segment in conventional routing (`/Home/Index` → `Index()`).
- Parameters are populated by model binding from route, query, form, and body sources.
- Return types include `IActionResult`, `ActionResult<T>`, or concrete results like `ViewResult`.
- `[HttpGet]`, `[HttpPost]`, and other verb attributes disambiguate overloaded action names.

---

## Q3. What is `IActionResult` and why use it instead of returning raw objects?

What is `IActionResult` and why use it instead of returning raw objects?

**Answer:** `IActionResult` is the abstraction for executable results — views, redirects, status codes, files, and JSON — that the framework runs to set the HTTP response. It decouples the action's decision from the mechanics of writing the response.

- Helper methods like `View()`, `RedirectToAction()`, `NotFound()`, and `BadRequest()` return typed `IActionResult` implementations.
- Returning a raw object from an API action works with formatters but returning `ActionResult<T>` preserves OpenAPI metadata and typed contracts.
- `IActionResult` enables consistent filter and middleware interaction before the result executes.
- Different result types set headers, status codes, and content independently of the action signature.
- Actions can return heterogeneous outcomes (404 vs 200 view) under one return type.

---

## Q4. What is the difference between `View()` and `Json()`?

What is the difference between `View()` and `Json()`?

**Answer:** `View()` selects a Razor view, sets `ViewData`, and renders HTML through the view engine with content negotiation toward `text/html`. `Json()` serializes an object to JSON using System.Text.Json (or configured formatters) with `application/json`.

- `View()` uses view discovery (`Views/{Controller}/{Action}.cshtml`) and supports layouts and partials.
- `Json()` is for AJAX endpoints or API-style responses from an MVC controller without a Razor template.
- `View()` can pass a model as `View(model)`; `Json(data)` writes the serialized object directly.
- Returning `View()` to an XHR client expecting JSON produces HTML — a common integration mistake.
- API projects typically use `ControllerBase` with `Ok(dto)` instead of `Json()` for consistent status codes.

---

## Q5. What is the difference between `RedirectToAction` and `Redirect`?

What is the difference between `RedirectToAction` and `Redirect`?

**Answer:** `RedirectToAction` generates a URL from route values (controller, action, route parameters) using the routing system — so URL changes from route templates stay correct. `Redirect(string url)` sends the client to an explicit absolute or relative URL string.

- `RedirectToAction("Index", "Home", new { id = 5 })` participates in link generation and route constraints.
- `Redirect("/Home/Index/5")` hardcodes the path and breaks if route patterns change.
- `RedirectToAction` supports PRG after POST without hardcoding URLs in controllers.
- `RedirectToRoute` targets a named route instead of controller/action names.
- Both return HTTP 302 (or 301 for permanent) and leave model binding state behind on the new GET.

---

## Q6. Why should action methods return `Task<IActionResult>` instead of `async void`?

Why should action methods return `Task<IActionResult>` instead of `async void`?

**Answer:** Controller actions must return `Task` or `Task<IActionResult>` so the framework awaits them, captures exceptions, and writes the result to the response. `async void` is fire-and-forget — exceptions crash the process and return values are lost.

- `async void` is reserved for event handlers, not ASP.NET Core action methods.
- Unhandled exceptions from `async void` actions may terminate the worker process.
- An `async void` action that calls `View(model)` without returning it produces an empty response.
- Proper async actions accept `CancellationToken` and propagate cancellation to I/O calls.
- `Task<IActionResult>` lets filters and middleware observe completion and status correctly.

---

## Q7. How does dependency injection work in MVC controllers?

How does dependency injection work in MVC controllers?

**Answer:** ASP.NET Core resolves controllers through the DI container, injecting constructor parameters registered in `Program.cs`. Controllers should request dependencies via constructor injection — not `HttpContext.RequestServices` lookups.

- Services are registered with lifetimes: singleton, scoped (typical for `DbContext`), or transient.
- The framework activates a new controller instance per request with scoped dependencies aligned to the request scope.
- `[FromServices]` on a parameter also resolves from DI but constructor injection is the standard pattern.
- `[ActivatorUtilitiesConstructor]` marks which constructor to use when multiple exist.
- Missing registrations fail at startup or first request with `InvalidOperationException`, not silent nulls.

---

## Q8. What does the `Controller` base class provide?

What does the `Controller` base class provide?

**Answer:** The `Controller` base class extends `ControllerBase` with view-related helpers and context properties for MVC scenarios. It adds methods like `View()`, `PartialView()`, and access to `ViewData`, `ViewBag`, `TempData`, and `ModelState`.

- `ControllerBase` provides `Ok()`, `BadRequest()`, `NotFound()`, and HTTP context access for API-style results.
- `Controller` adds Razor-specific `View()` overloads and `ViewComponent()` helpers.
- Both expose `HttpContext`, `Request`, `Response`, `User`, and `Url` helper properties.
- `ModelState` on the controller tracks binding and validation errors for the current request.
- JSON-only APIs should inherit `ControllerBase` to avoid pulling in view infrastructure.

---

## Q9. What is `ModelState` and when should an action check it?

What is `ModelState` and when should an action check it?

**Answer:** `ModelState` is a dictionary of model binding and validation results keyed by property name, populated during model binding from Data Annotations and manual errors. Actions must check `ModelState.IsValid` before persisting data on POST requests.

- Invalid annotation validation adds errors automatically during binding — but only if the action checks before saving.
- Manual business rule failures call `ModelState.AddModelError("Property", "message")`.
- On failure, return `View(model)` to redisplay the form with validation tag helpers showing errors.
- Skipping the check allows invalid or malicious input to reach the database despite client validation.
- `ModelState` is request-scoped and does not survive redirects without TempData or a second validation pass.

---

## Q10. What are `[HttpGet]` and `[HttpPost]` used for?

What are `[HttpGet]` and `[HttpPost]` used for?

**Answer:** These attributes restrict which HTTP verbs can invoke an action, disambiguating overloads with the same name and enforcing RESTful verb semantics. GET actions display forms or read data; POST actions accept form submissions and mutate state.

- Without verb attributes, two actions named `Create` cause ambiguous routing exceptions.
- Browsers follow links with GET — state changes must use POST, PUT, PATCH, or DELETE.
- `[HttpPost]` pairs with `[ValidateAntiForgeryToken]` on form submissions that change data.
- `[AcceptVerbs("GET", "POST")]` allows multiple verbs on one action when intentional.
- Attribute routing uses HTTP method constraints alongside route templates for action selection.

---

## Q11. What is `[ValidateAntiForgeryToken]` and when is it required?

What is `[ValidateAntiForgeryToken]` and when is it required?

**Answer:** `[ValidateAntiForgeryToken]` validates that a POST request includes a matching antiforgery token, preventing cross-site request forgery attacks that trick a logged-in user's browser into submitting unwanted forms. It is required on every state-changing POST action that uses cookie-based authentication.

- The form tag helper automatically renders a hidden `__RequestVerificationToken` field.
- The server compares the form token with the cookie token issued by antiforgery middleware.
- AJAX POSTs must send the token via header `RequestVerificationToken` or form field manually.
- `[AutoValidateAntiforgeryToken]` on the controller applies validation to all unsafe verbs.
- GET actions do not need antiforgery validation because safe methods should not mutate state.

---

## Q12. What is the difference between `[FromBody]`, `[FromForm]`, and default binding in MVC actions?

What is the difference between `[FromBody]`, `[FromForm]`, and default binding in MVC actions?

**Answer:** Binding source attributes tell model binders where to read values: `[FromBody]` deserializes JSON from the request body, `[FromForm]` reads form fields, and default binding combines route, query, and form values by convention.

- Standard HTML forms post `application/x-www-form-urlencoded` or `multipart/form-data` — not JSON — so `[FromBody]` leaves the model empty.
- `[FromForm]` explicitly binds form fields for POST actions with complex models.
- `[FromRoute]` and `[FromQuery]` target route segments and query string parameters.
- `[FromBody]` typically allows one parameter per action because the body is a single stream.
- Default binding without attributes works for simple parameters and MVC form posts in most scenarios.

---

## Q13. How are controllers activated per request?

How are controllers activated per request?

**Answer:** ASP.NET Core uses `IControllerActivator` (default: `DefaultControllerActivator`) to create a controller instance for each request by resolving the type from DI with constructor dependencies. The instance lives for the duration of the action invocation and is eligible for garbage collection afterward.

- Controllers are not singletons — scoped services like `DbContext` align with per-request activation.
- `ActivatorUtilities.CreateInstance` fills constructor parameters from the request's service provider.
- Custom activators can wrap controllers with logging or interceptors but rarely replace default behavior.
- `IDisposable` on controllers is not the recommended resource cleanup pattern — use scoped services instead.
- Controller types do not need explicit registration in DI unless using factory or custom activator patterns.

---

## Q14. What is the difference between returning `NotFound()` and `BadRequest()`?

What is the difference between returning `NotFound()` and `BadRequest()`?

**Answer:** `NotFound()` returns HTTP 404, meaning the requested resource does not exist at the given identifier. `BadRequest()` returns HTTP 400, meaning the client sent malformed or invalid input that prevents processing.

- Valid ID format but missing database row → `NotFound()`.
- Null, negative, or whitespace ID when an ID is required → `BadRequest()` or validation problem.
- Inverting these confuses API clients, monitoring alerts, and caching behavior.
- MVC may return HTML error pages; API controllers should return ProblemDetails for consistency.
- `[ApiController]` automatically translates invalid model state to 400 with validation details.

---

## Q15. Why should controllers avoid a static service locator?

Why should controllers avoid a static service locator?

**Answer:** A static service locator hides dependencies, requires global mutable state, bypasses constructor injection, and breaks scoped lifetimes — especially for `DbContext`. ASP.NET Core DI already provides per-request resolution without static access.

- Hidden dependencies make unit testing impossible without mutating global state before each test.
- Resolving scoped services from singletons or static fields causes captive dependency bugs and stale contexts.
- `HttpContext.RequestServices.GetService<T>()` in actions masks missing registrations until runtime.
- Constructor injection makes dependencies explicit, verifiable, and compatible with analyzer tools.
- Static locators were an anti-pattern before Core and remain one in ASP.NET Core 8.

---

## Q16. What is the purpose of constructor injection in controllers?

What is the purpose of constructor injection in controllers?

**Answer:** Constructor injection declares all dependencies explicitly in the controller's constructor, letting the DI container wire them at activation time. It enables testing with mocks, enforces correct service lifetimes, and fails fast when a service is not registered.

- Dependencies appear as private readonly fields set once at construction.
- The compiler and IDE surface required services — no hidden lookups in action bodies.
- Integration and unit tests pass fake implementations through the constructor without HTTP context.
- ASP.NET Core resolves the graph automatically for each request-scoped controller instance.
- Optional `[ActivatorUtilitiesConstructor]` selects the injectable constructor when multiple exist.

---

## Q17. What is the difference between conventional routing and attribute routing on a controller?

What is the difference between conventional routing and attribute routing on a controller?

**Answer:** Conventional routing maps URLs to `{controller}/{action}/{id?}` patterns registered in `Program.cs` with `MapControllerRoute`. Attribute routing places `[Route]` templates directly on controllers and actions, enabling precise, REST-style URLs independent of class names.

- Conventional routing uses naming conventions — `ProductsController.Index` maps to `/Products/Index`.
- Attribute routing supports templates like `[HttpGet("api/products/{id:int}")]` on individual actions.
- Attribute-routed controllers often use `[Route("[controller]")]` at the class level with verb attributes on actions.
- Both can coexist; attribute routes take precedence when both match.
- Tag helpers generate URLs from route names and attribute templates when `asp-controller` and `asp-action` are used.

---

## Q18. Can you implement `IDisposable` on a controller to manage resources? Why or why not?

Can you implement `IDisposable` on a controller to manage resources? Why or why not?

**Answer:** Implementing `IDisposable` on a controller is unreliable because ASP.NET Core does not guarantee deterministic disposal timing for controller instances. Unmanaged or expensive resources should live in scoped services that DI disposes at the end of the request.

- Controller lifetime is tied to action invocation, but disposal is not part of the documented cleanup contract.
- Native handles or connections in controller fields risk leaks or double-dispose under concurrency.
- Register `IDisposable`/`IAsyncDisposable` implementations as scoped services and inject them instead.
- `IResourceFilter` can wrap acquire/release around an action if lifetime must match action boundaries.
- The framework disposes scoped services created in the request scope automatically after the response completes.

---

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Answer:** Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently.

- Views should render data the controller or ViewModel already prepared.
- Authorization belongs in filters, policies, or controller/service checks before the view executes.
- Calculations in Razor cannot be tested independently and often diverge from API or batch logic.
- Keep Razor limited to presentation formatting — not business decisions.

---

#### Gotcha 2. EF entities passed directly to views

**Answer:** Binding and displaying EF Core entities exposes navigation properties, causes over-posting on POST, and couples the UI to the database schema.

- Lazy-loaded navigations can trigger unexpected queries during rendering.
- Mass assignment can update properties the user should not control (e.g., `IsAdmin`).
- Use dedicated ViewModels with only the fields the view needs.
- Map between entities and ViewModels in the controller or a mapping service.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Answer:** Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model empty while the action runs with default values.

- Remove `[FromBody]` for conventional form POSTs and let model binding read form fields.
- Use `[FromBody]` only when the client sends JSON with the correct Content-Type.
- Silent binding failure is a common source of "my POST action receives null model" bugs.
- AJAX forms using `FormData` follow the same form binding rules as full-page forms.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Answer:** Client-side validation is bypassable — attackers POST directly without browser scripts. Server-side validation is mandatory before any persist, redirect, or side effect.

- Always gate POST actions with `if (!ModelState.IsValid) return View(model);` or equivalent.
- Client validation improves UX for legitimate users only.
- Remote validation and unobtrusive rules are not security boundaries.
- Treat missing server validation as a security defect regardless of client script presence.

---

#### Gotcha 5. `return View()` after successful POST

**Answer:** Returning the same view after a successful POST causes duplicate submission when the user refreshes the page — the browser resubmits the POST body.

- Use Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create/update.
- PRG separates the mutation (POST) from the display (GET).
- Flash success messages via TempData on the redirect target.
- AJAX partial POSTs have a similar concern — disable submit during request or use idempotent server logic.

---

#### Gotcha 6. `ModelState` after redirect

**Answer:** `ModelState` is request-scoped and does not survive `RedirectToAction`. Validation errors are lost unless rehydrated through TempData, a second validation pass on GET, or by redisplaying the form without redirect on failure only.

- Common pattern: redirect only on success; on validation failure return `View(model)` with errors inline.
- To survive redirect on failure, serialize errors to TempData or use PRG with a form-specific error cache.
- Do not assume errors automatically follow the user after redirect.
- AJAX partial forms avoid redirect and can return the form partial with `ModelState` errors directly.

---

#### Gotcha 7. TempData read twice in layout and view

**Answer:** TempData is consumed on first read by default. If the layout reads a flash message, the view sees nothing unless you use `Peek()` or `Keep()`.

- Use `TempData.Peek("Message")` in the layout to read without consuming.
- Or call `TempData.Keep("Message")` after the layout read so the view can read it too.
- Prefer a single consumption point — typically the layout or a dedicated partial, not both.
- Cookie-based TempData has size limits; avoid storing large payloads.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Answer:** Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong conventional route.

- Every area controller must declare `[Area("AreaName")]` matching its folder.
- Area routing is registered separately in `Program.cs` with the `{area:exists}` constraint.
- Without the attribute, MVC treats the controller as a root controller.
- Verify area registration order — specific area routes before catch-all default routes.

---

#### Gotcha 9. Link generation without `asp-area`

**Answer:** Tag Helpers default to the current area context when generating URLs. Links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment.

- From within an area, omitting `asp-area` keeps links inside the current area — sometimes incorrectly.
- Cross-area links require both `asp-area` and `asp-controller` (and `asp-action`).
- Wrong URLs produce 404 or hit unintended controllers.
- Same rule applies to `Url.Action` — pass `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Answer:** A missing unchecked checkbox posts nothing and model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid value — not null or empty.

- Use `bool?` with `[Required]` to require an explicit true selection for consent checkboxes.
- Or use the hidden-field pattern: hidden input `false` plus checkbox `true` so unchecked still posts `false` deliberately.
- Server-side, verify explicit consent with a dedicated check rather than relying on `[Required]` alone.
- This applies to both full-page forms and AJAX form posts.

---

#### Gotcha 11. Collection binding with gap indices

**Answer:** Deleting a row from a dynamic form leaving indices such as `Lines[0]` and `Lines[2]` breaks model binder alignment — index 1 is missing and subsequent items may bind incorrectly or truncate.

- Reindex client-side after row deletion so indices are contiguous starting at zero.
- Or implement a custom `IModelBinder` that tolerates non-contiguous indices.
- Partial views rendering collection editors must maintain consistent index naming.
- Test add/delete row scenarios explicitly in complex form POSTs.

---

#### Gotcha 12. `@Html.Raw` with user content

**Answer:** Default Razor encoding prevents XSS by HTML-encoding output. `@Html.Raw(Model.UserComment)` renders attacker-supplied script if the content is not sanitized server-side.

- Encode first, then apply safe formatting — never wrap raw user input in HTML.
- AJAX-loaded partials injected via `innerHTML` execute injected script the same as full pages.
- Prefer `@Model.UserComment` (auto-encoded) or sanitize with a trusted HTML sanitizer library.
- Content-Security-Policy limits blast radius but does not replace encoding.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Answer:** Form tag helpers emit antiforgery tokens automatically, but `fetch` and jQuery AJAX must manually send `RequestVerificationToken` header or `__RequestVerificationToken` form field or POSTs fail with 400 antiforgery errors.

- Read the hidden field value from the page and include it on every mutating AJAX request.
- Same-origin requests send the antiforgery cookie automatically.
- `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe methods — missing tokens fail before the action runs.
- Do not disable antiforgery on MVC cookie-auth endpoints to "fix" AJAX — add the token instead.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Answer:** Hubs are not registered in DI for direct injection into controllers. Use `IHubContext<THub>` to broadcast messages from controllers, services, or background jobs.

- Injecting a concrete `Hub` fails activation or produces an instance without connection context.
- `IHubContext<T>` is a singleton proxy registered by `AddSignalR()`.
- Pair with Redis backplane or Azure SignalR for multi-instance fan-out.
- Keep hubs thin; business logic stays in scoped or transient services.

---

#### Gotcha 15. SignalR scale-out without backplane

**Answer:** Sticky sessions alone do not fan-out events across server instances. Multi-node deployments need a Redis backplane or Azure SignalR Service so messages sent from any instance reach clients on all instances.

- Controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users connected to instance B without a backplane.
- Sticky sessions route connections but do not route cross-instance messages.
- Group membership and connection IDs are local to each instance.
- Register `AddStackExchangeRedis` or `AddAzureSignalR` when scaling beyond a single node.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

## Gotchas — ASP.NET Core MVC (Interview Traps)

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

---

**Answer:**

**Answer:** Returning `object` compiles because `BadRequest`, `NotFound`, and the entity all coerce to `object`, but you lose typed result contracts, OpenAPI metadata, and predictable content negotiation — the action is an API endpoint on `Controller` without `[ApiController]` or explicit `ActionResult<T>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API design | `object` return instead of `ActionResult<ProductDto>` | Swagger/generators cannot infer response schema |
| IActionResult | Mixing `IActionResult` helpers with raw entity | Formatter selection is implicit; harder to test result types |
| MVC vs API | Inherits `Controller` (View support) for JSON route | Wrong base for JSON-only API — use `ControllerBase` + `[ApiController]` |
| HTTP semantics | `BadRequest(string)` on invalid id | Plain text 400 — prefer ProblemDetails for API clients |

**Fix (priority order):**

1. Change base to `ControllerBase`, add `[ApiController]`, return `ActionResult<ProductDto>`.
2. Map entity to DTO; use `return Ok(dto)`, `return NotFound()`, `return ValidationProblem()` as appropriate.
3. Add `[ProducesResponseType]` attributes for OpenAPI and contract tests.

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

**Production takeaway:** `object` and untyped `IActionResult` misuse hide API contracts — Karat tests whether you reach for `ActionResult<T>` and the right controller base.

---

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

---

**Answer:**

**Answer:** Controller actions must never be `async void` — the framework cannot await them, exceptions propagate to `SynchronizationContext` as unhandled faults, and the `View(...)` result is discarded because the method returns `void` instead of `Task<IActionResult>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `async void Details` | Unhandled exceptions can tear down the process |
| IActionResult | `View(order)` not returned | Client gets empty/wrong response even when no exception |
| API design | Missing null guard before View | Possible runtime error inside view |
| Scalability | Async work not tied to request cancellation | Slow queries continue after client disconnect |

**Fix (priority order):**

1. Signature: `public async Task<IActionResult> Details(int id, CancellationToken ct)`.
2. `var order = await _orders.GetAsync(id, ct);` then `return order is null ? NotFound() : View(order);`.
3. Enable nullable reference types and guard clauses before rendering.

**Production takeaway:** `async void` is only for event handlers — in controllers it is a production incident waiting to happen. See C# async fundamentals — always `async Task`/`Task<T>` in ASP.NET Core actions.

---

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

---

**Answer:**

**Answer:** Resolving services inside an action via `HttpContext.RequestServices` is the service-locator anti-pattern; `_invoices` is only set when `Index` runs first, so `Approve` and direct POST hits see a null field despite `[Required]` services being registered in DI.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | Parameterless constructor; service resolved in action | Hidden dependency; violates explicit constructor injection |
| Correctness | `_invoices` field set only in `Index` | `NullReferenceException` on POST without visiting Index |
| Testability | Cannot inject mock `IInvoiceService` in unit tests | Forces integration tests for controller logic |
| Design | `GetRequiredService` in controller | Masks missing registration until runtime |

**Fix (priority order):**

1. Constructor injection: `public InvoicesController(IInvoiceService invoices) => _invoices = invoices;`.
2. Remove all `RequestServices` lookups from actions.
3. Register `IInvoiceService` as scoped in `Program.cs`; verify with controller unit tests.

**Production takeaway:** DI in controllers means **constructor injection only** — per-action service location is a common Karat trap stacked with nullable field state.

---

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

---

**Answer:**

**Answer:** A static `ServiceLocator` hides dependencies, requires mutating global state before requests, and bypasses ASP.NET Core's scoped `DbContext` lifetime — controllers should receive `AppDbContext` or a repository via constructor injection.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI / design | Static `ServiceLocator.Provider` | Global mutable state; race conditions in tests |
| Lifetime | `AppDbContext` resolved manually | May capture wrong scope; concurrent request issues |
| Data access | Raw SQL in controller via `FromSqlRaw` | No repository boundary; hard to mock |
| Security | `EXEC usp_MonthlyReport` in controller | SQL surface in web layer; parameterization unclear |

**Fix (priority order):**

1. Delete `ServiceLocator`; inject `IReportService` or `AppDbContext` through constructor.
2. Move SQL to repository/service; use parameterized commands or EF APIs.
3. Return a view model from service layer — controller returns `View(model)` only.

**Production takeaway:** Static service locators were an anti-pattern before Core and remain one — Core's DI container already solves activation per request.

---

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

---

**Answer:**

**Answer:** `GET /admin/products/42` hits `Get`, which always returns a Razor `View` — the React client never reaches `Lookup` because route templates differ; returning `View` when the client sends `Accept: application/json` yields HTML, not JSON.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Return type | `Get` returns `View` for API path | SPA receives HTML document instead of JSON |
| Routing | Two endpoints with different templates (`{id}` vs `lookup/{id}`) | Client calls wrong route — easy integration mismatch |
| API/MVC mix | Same controller serves HTML and JSON | Content negotiation not applied — MVC ignores Accept for View |
| Design | No `[Produces("application/json")]` on JSON action | Ambiguous contract for consumers |

**Fix (priority order):**

1. Split concerns: `AdminProductsController` (Views) vs `AdminProductsApiController` : `ControllerBase` (JSON).
2. Or change React to call `/admin/products/lookup/42` consistently and document routes.
3. For dual-format endpoints, use explicit formatters or separate actions — do not return `View` to XHR clients.

**Production takeaway:** View vs Json is not interchangeable — wrong return type is a routing/design bug, not a serialization tweak.

---

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

---

**Answer:**

**Answer:** Without `[HttpPost]` on the POST overload, both `Login` actions match GET and POST; without `[ValidateAntiForgeryToken]` on the mutating action, login CSRF is possible; `Logout` should be POST-only as well.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP verbs | Missing `[HttpGet]` / `[HttpPost]` on overloads | Ambiguous action selection; both verbs hit same overload |
| Security | No `[ValidateAntiForgeryToken]` on POST Login | CSRF — attacker submits forged login/logout |
| Security | GET-accessible `Logout` | CSRF logout or prefetch triggers sign-out |
| Routing | Duplicate action name without verb disambiguation | `AmbiguousMatchException` or wrong action chosen |

**Fix (priority order):**

1. `[HttpGet] public IActionResult Login()` and `[HttpPost][ValidateAntiForgeryToken] public IActionResult Login(LoginViewModel model)`.
2. `[HttpPost][ValidateAntiForgeryToken] public IActionResult Logout()`.
3. Ensure Razor form includes antiforgery (`<form>` tag helper does automatically).

**Production takeaway:** HTTP verb attributes are not optional on overloaded action names — they disambiguate routing and enforce safe verbs for state-changing operations.

---

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

---

**Answer:**

**Answer:** The POST action never checks `ModelState.IsValid` before calling `SaveChanges`, so DataAnnotations on `CustomerViewModel` run during binding but invalid models still insert rows — the view never redisplays validation errors.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Validation | `ModelState.IsValid` ignored | Empty names and invalid emails persisted |
| UX | No `return View(model)` on failure | User sees success redirect or silent bad data |
| Data access | Controller calls `_db` directly | Thick controller — harder to unit test validation path |
| Design | Entity built manually from ViewModel | Mapping errors bypass validation attributes |

**Fix (priority order):**

1. Guard POST: `if (!ModelState.IsValid) return View(model);` before any persistence.
2. Move persistence to `ICustomerService.Create(model)`; keep controller thin.
3. Add server-side validation tests asserting invalid POST returns 200 View with errors, not redirect.

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

**Production takeaway:** Model binding populates `ModelState` — ignoring it is one of the most common MVC production bugs Karat surfaces.

---

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

---

**Answer:**

**Answer:** Two actions share the same HTTP method and route template `GET catalog/items` — attribute routing cannot distinguish them by parameter presence alone when both templates are identical strings.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | Duplicate `[HttpGet("items")]` templates | `AmbiguousMatchException` at startup or first request |
| Design | Optional `category` not reflected in route | Query-based filter should use one action or different routes |
| Maintainability | Overloaded action names without route differentiation | Breaks link generation and tests |

**Fix (priority order):**

1. Merge into one action: `Items(string? category)` — if `category` null, featured; else filter.
2. Or separate routes: `[HttpGet("items")]` and `[HttpGet("items/category/{category}")]`.
3. Run route constraint tests at startup (`MapControllers` + integration test hitting both URLs).

**Production takeaway:** Duplicate action names require **different route templates or HTTP verbs** — parameter lists alone do not create distinct attribute routes.

---

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

---

**Answer:**

**Answer:** The controller owns ADO.NET connection management, SQL, and mapping — it cannot be tested without SQL Server and violates separation of concerns; `IConfiguration` for connection strings belongs in a registered repository, not action code.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Controller performs data access | Untestable without database; SRP violation |
| DI | Connection string via `_config` in controller | Infrastructure leaked into presentation layer |
| Performance | Opens connection per request in action | No pooling strategy visibility; sync I/O blocks threads |
| Security | Ad-hoc SQL string | Injection risk if later parameterized incorrectly |

**Fix (priority order):**

1. Introduce `IEmployeeRepository.GetAll()` registered scoped in DI.
2. Controller: `return View(await _employees.GetAllAsync(ct));` — one line.
3. Unit test repository with in-memory fake; integration test SQL separately.

**Production takeaway:** "Fat controller" data access is a Karat classic — the fix is inject a service/repository, not move SQL to a static helper.

---

---

#### Q10. (P) An internal MVC app accepts POST forms for role changes. Pen test reports missing CSRF protection on one action. Review the pattern and what production setup is required beyond the attribute.

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

---

**Answer:**

**Answer:** State-changing POST actions must carry `[ValidateAntiForgeryToken]` and the form must emit the request verification token; globally, antiforgery services must be registered (default in MVC) and tokens must flow on every mutating form — including AJAX if used.

- Add `[ValidateAntiForgeryToken]` on POST `EditUser` — pairs with `[HttpPost]`.
- Razor `<form asp-action="EditUser" method="post">` tag helper renders `@Html.AntiForgeryToken()` automatically.
- For AJAX POSTs, send header `RequestVerificationToken` with token from `<input name="__RequestVerificationToken">`.
- Optional defense in depth: `[AutoValidateAntiforgeryToken]` at controller level for all unsafe verbs.
- Ensure `[HttpGet]` on GET overload — `[HttpPost]` on POST; role changes must never be GET.
- SameSite cookies and HTTPS protect token in transit; antiforgery does not replace authorization — still verify admin role in action or filter.

**Production takeaway:** Anti-forgery is a **pair**: server validation attribute + client token on every POST — missing either fails pen tests silently until production.

---

---

#### Q11. (D) API consumers report inconsistent status codes: missing resources return 400, malformed ids return 404. Review this action and explain the correct HTTP semantics for MVC vs JSON API responses.

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

---

**Answer:**

**Answer:** Status codes are inverted — empty/whitespace `sku` is a **client error (400 Bad Request)**, while unknown inventory is **404 Not Found**; returning `BadRequest` for missing resources breaks REST client retry/idempotency assumptions.

| Condition | Wrong response | Correct response | Rationale |
|---|---|---|---|
| `sku` null/whitespace | 404 NotFound | **400 Bad Request** | Malformed request — client sent invalid input |
| `sku` too long | 400 Bad Request | **400 Bad Request** | Validation failure — correct |
| Known format, item missing | 400 Bad Request | **404 Not Found** | Resource identifier valid but not found |
| Item exists | 200 + JSON | **200 OK** | Correct |

- Use `ControllerBase` + `[ApiController]` for JSON APIs — automatic ProblemDetails on validation failures.
- Distinguish **invalid identifier format** (400) from **valid identifier, no resource** (404).
- For MVC HTML: `NotFound()` may return custom error page; for API always avoid HTML error bodies on JSON routes.
- Log 404 for missing SKUs at Information; 400 for malformed input at Warning — aids monitoring.

**Production takeaway:** 404 vs 400 is a contract decision — Karat tests whether you map "bad syntax" to 400 and "not in database" to 404, not the reverse.

---

---

#### Q12. (M) A developer implements `IDisposable` on a controller to release an expensive native handle created in the constructor. Under load, handles leak and `_handle` is sometimes already disposed. Explain controller activation, lifetime, and the correct pattern.

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

**Answer:**

**Answer:** ASP.NET Core **does not guarantee** one controller instance per request or that `Dispose` on `Controller` runs promptly — `IControllerActivator` creates controllers per invocation semantics that differ from classic ASP.NET; implementing `IDisposable` on the controller is unreliable for resource cleanup.

- **Activation:** Default `DefaultControllerActivator` resolves the controller from DI (transient controller type registration is uncommon — controllers are typically registered as transient implicitly per request via `ActivatorUtilities`).
- **Lifetime:** Controller instance lifetime is scoped to the action invocation; **`Dispose()` on `Controller` is not part of the documented cleanup contract** the way scoped services are — native handles in fields race under concurrency if controller instance were reused (it should not be, but `Dispose` timing is undefined).
- **Problem:** `_handle` created in field initializer per controller construction — if activation creates multiple instances or `Dispose` never called, native resources leak; if called twice, `ObjectDisposedException`.
- **Correct pattern:** Wrap `NativeScanner` in a **scoped service** `INativeScanner` registered `AddScoped`, implement `IAsyncDisposable`/`IDisposable` on the **service**, inject into controller — DI disposes scoped services at end of request.
- Alternative: **`IResourceFilter`** or **`IAsyncActionFilter`** for acquire/release around the action if lifetime must match action only.
- Do not use finalizers on controllers; do not hold unmanaged resources directly on `Controller` subclasses.

```csharp
// Register: services.AddScoped<INativeScanner, NativeScanner>();
public class ScannerController : Controller
{
    private readonly INativeScanner _scanner;
    public ScannerController(INativeScanner scanner) => _scanner = scanner;

    public IActionResult Scan() => Json(_scanner.ReadBarcode());
}
```

**Production takeaway:** Controller activation resolves dependencies — **resource lifetime belongs in scoped services**, not `IDisposable` on the controller type.

---
