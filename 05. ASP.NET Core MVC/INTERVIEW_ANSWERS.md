# ASP.NET Core MVC — Interview Answers

Answers for [INTERVIEW_QUESTIONS.md](./INTERVIEW_QUESTIONS.md).  
Written for clarity and recall — concepts only, no interview coaching.

> **Scope:** All chapters

---

## Chapter 01. Introduction to MVC Pattern

#### Q1. What is the MVC pattern?

**Answer:** MVC (Model–View–Controller) separates an application into three cooperating parts: the Model holds data and domain logic, the View renders presentation, and the Controller handles input and orchestrates the flow between Model and View. In ASP.NET Core MVC, HTTP requests map to controller actions that prepare models and select Razor views for the response.

- The Controller receives the request, invokes services or repositories, and decides which view to render or whether to redirect.
- The Model represents domain entities, view models, and validation state passed to the view.
- The View is passive markup (Razor) that displays data the controller prepared — it does not own business rules.
- Separation keeps UI changes independent from business logic and makes each layer testable in isolation.
- ASP.NET Core 8 implements server-side MVC with endpoint routing, DI, and filter pipelines layered on the generic host.

---

#### Q2. What is the role of the Model in ASP.NET Core MVC?

**Answer:** In ASP.NET Core MVC, "Model" refers to the data and state an action passes to a view or receives from model binding — including domain entities, view models, DTOs, and `ModelState`. It is not a single class; it is the data contract between controller, binding, validation, and view.

- View models shape exactly what a form or page needs, excluding sensitive or irrelevant database fields.
- Domain entities represent business objects and invariants, typically accessed through application services rather than directly in views.
- `ModelState` carries validation results from Data Annotations, FluentValidation, or manual errors after binding.
- The model should not reference HTTP concerns like `HttpContext` or Razor-specific types.
- Strongly typed models (`@model`) give compile-time safety compared to `ViewBag` or `ViewData`.

---

#### Q3. What is the role of the View in ASP.NET Core MVC?

**Answer:** The View is the presentation layer — Razor `.cshtml` files that render HTML from a model the controller supplies. Views should format and display data, not perform business logic, data access, or authorization decisions.

- Razor combines HTML with C# expressions, tag helpers, and partial views for reusable markup fragments.
- Views receive data through a strongly typed `@model`, or less preferably through `ViewData`/`ViewBag`.
- Layouts (`_Layout.cshtml`), sections, and partial views compose the final page structure.
- Views are compiled at publish time by default in ASP.NET Core 8, with optional runtime compilation in Development.
- Keeping views thin ensures pricing, authorization, and persistence rules stay testable in services.

---

#### Q4. What is the role of the Controller in ASP.NET Core MVC?

**Answer:** The Controller is the HTTP adapter that maps URLs and verbs to action methods, binds input, calls application services, and selects the response — a view, redirect, or status result. It orchestrates the request; it should not own business rules or data access.

- Controllers inherit from `Controller` (MVC with views) or `ControllerBase` (API-only) and are discovered by routing.
- Actions return `IActionResult`/`Task<IActionResult>` to produce views, redirects, JSON, or HTTP error codes.
- Constructor injection supplies scoped services such as repositories, mediators, or application services.
- Controllers check `ModelState.IsValid`, enforce authorization via attributes or filters, and map service results to HTTP responses.
- A thin controller validates input, delegates work, and chooses the presentation — nothing more.

---

#### Q5. What is the difference between MVC and MVP?

**Answer:** In MVC, the Controller drives the flow and pushes data to a passive View. In MVP (Model–View–Presenter), a Presenter mediates all interaction — the View is dumb, raises events, and the Presenter updates both the Model and the View.

- MVC fits request/response web apps where the server controls page flow per HTTP request.
- MVP is common in desktop UI (WinForms) where the view raises events and the presenter handles them synchronously.
- ASP.NET Core MVC implements Controller-driven flow, not automatic Presenter wiring.
- Teams sometimes extract presenter-like service classes to slim controllers, but the framework still routes to controller actions.
- Confusing the two leads to putting orchestration logic inside Razor views instead of controllers or services.

---

#### Q6. What is the difference between MVC and MVVM?

**Answer:** MVVM (Model–View–ViewModel) uses a ViewModel with bindable properties and commands; the View binds declaratively and changes propagate two-way. MVC uses a Controller to handle requests and push a model into a largely passive View.

- MVVM dominates XAML stacks (WPF, MAUI) and client-side SPA frameworks with observable state.
- ASP.NET Core MVC view models share the name but are typically one-way snapshots for server-rendered forms, not full MVVM unless client-side binding is added.
- In MVC, each HTTP request is stateless — there is no ongoing two-way binding unless JavaScript frameworks provide it.
- Blazor leans closer to component/state patterns but classic MVC remains request-driven.
- Placing domain rules in a view model's methods blurs MVVM presentation state with business logic.

---

#### Q7. What is the difference between ASP.NET Core MVC and Razor Pages?

**Answer:** Both use Razor for server-rendered HTML, but MVC organizes code around controllers and actions with `{controller}/{action}` routing, while Razor Pages colocates a PageModel with each `.cshtml` using page-centric routing.

- MVC suits apps with many controllers, areas, shared filters, and REST-style URL conventions across resources.
- Razor Pages suit page-focused workflows (forms, wizards, admin CRUD) with less ceremony per page.
- Razor Pages use `@page` directives and `PageModel` classes instead of controller action pairs.
- Both support tag helpers, validation, authorization, and the same underlying Razor view engine.
- The choice is organizational — the rendering pipeline and DI model are shared in ASP.NET Core 8.

---

#### Q8. What is the difference between ASP.NET Core MVC and a SPA + Minimal API approach?

**Answer:** ASP.NET Core MVC renders HTML on the server and uses full page loads or partial updates; a SPA + Minimal API serves JSON from API endpoints and renders UI entirely in the browser with JavaScript.

- MVC keeps validation, routing, and auth integrated in one host with cookie-based sessions and form posts.
- SPA + Minimal API splits backend (JSON endpoints) and frontend (React, Angular, etc.), often using JWT or token auth.
- MVC fits form-heavy, SEO-sensitive, or server-rendered admin apps with simpler deployment.
- SPA fits rich client interactivity, offline-like UX, or multiple clients sharing one API.
- Hybrid approaches (MVC + AJAX partials) exist but increase complexity compared to picking one primary model.

---

#### Q9. What does "thin controller" mean and why is it preferred?

**Answer:** A thin controller limits itself to HTTP concerns — binding input, calling services, checking `ModelState`, and returning views or redirects — without embedding business rules, SQL, or email logic. This makes controllers easy to test, reuse, and maintain.

- Business logic moves to application or domain services callable from MVC, Minimal APIs, and background workers.
- Data access hides behind repositories or `DbContext` interfaces injected into services, not controllers.
- Thin controllers reduce duplication when the same use case is exposed via web and API endpoints.
- Unit tests mock service interfaces instead of spinning up databases or HTTP contexts.
- The controller becomes a stable adapter even when business rules change frequently.

---

#### Q10. What is the "fat controller" anti-pattern?

**Answer:** A fat controller stuffs data access, validation rules, mapping, external HTTP calls, and email sending directly into action methods — often hundreds of lines per action. It violates single responsibility and makes testing and reuse difficult.

- Actions become untestable without full stack integration tests and real databases.
- Business rules duplicated in controllers cannot be shared with API projects or batch jobs.
- Direct `DbContext`, ADO.NET, or `new HttpClient()` usage in actions causes lifetime and scalability issues.
- Refactoring requires touching controller code for every business change instead of isolated services.
- Fat controllers often pass EF entities straight to views, coupling UI to the database schema.

---

#### Q11. Where should business logic live in an MVC application?

**Answer:** Business logic belongs in domain entities (for core invariants) and application services (for use-case orchestration) — not in controllers, views, or Razor PageModels beyond coordination. Controllers call these services and map results to HTTP responses.

- Domain rules like discount eligibility, stock checks, or pricing policies live in domain methods or dedicated policy classes.
- Application services coordinate transactions, call repositories, and publish domain events across a use case.
- Controllers translate HTTP input into service commands and map outcomes to views or redirects.
- Views display computed results from view models — they do not recalculate business values.
- Centralizing rules ensures MVC forms, REST APIs, and background jobs enforce the same behavior.

---

#### Q12. Where should data access logic live in an MVC application?

**Answer:** Data access belongs in repositories, query services, or EF Core `DbContext` usage behind interfaces registered in DI — not in controllers or views. Controllers and services depend on abstractions like `IOrderRepository`, not raw SQL.

- Repositories encapsulate queries, persistence, and mapping from database rows to domain objects.
- EF Core `DbContext` is registered scoped per request and injected into repositories or services.
- Controllers never open connections or call `SaveChanges` directly in well-structured apps.
- Views must never query databases — not even via `@inject AppDbContext`.
- Separating data access enables unit tests with fakes and integration tests focused on the repository layer.

---

#### Q13. Where should input validation live in an MVC application?

**Answer:** Input validation — required fields, string length, format, range — belongs on view models or input DTOs using Data Annotations, FluentValidation, or `IValidatableObject`. Server-side validation always runs in the action or a filter before any persist operation.

- Data Annotations on view models generate both server-side checks and client-side `data-val-*` attributes.
- Business validation (e.g., "credit limit exceeded") belongs in services and adds errors to `ModelState` manually.
- Validation on EF entities shared with MVC risks wrong-layer coupling and API inconsistencies.
- Client-side validation improves UX but is bypassable — server checks are mandatory.
- `ModelState.IsValid` in the action gates the call to persistence services.

---

#### Q14. How does a request flow through Model, View, and Controller?

**Answer:** Kestrel receives the HTTP request, middleware runs (routing, auth, etc.), routing selects a controller action, model binding constructs input models, the controller invokes services, and the action returns a view result with a model that Razor renders into HTML.

- Routing maps the URL and HTTP verb to `Controller.Action` via conventional or attribute routes.
- Model binding populates action parameters from form fields, route values, and query strings; validation runs during binding.
- The controller calls application services with bound input and receives domain results or view models.
- On success the action returns `View(model)` or `RedirectToAction`; on validation failure it redisplays the form with errors.
- The view engine locates the `.cshtml` file, applies `_ViewStart` layout, and renders HTML sent back through the middleware pipeline.

---

#### Q15. What is the difference between a domain entity and a ViewModel?

**Answer:** A domain entity models business concepts and persistence with identity, relationships, and invariants tied to the database. A ViewModel shapes data specifically for one view or form — including display fields, validation attributes, and no sensitive or unnecessary properties.

- Entities may have navigation properties, concurrency tokens, and internal fields not meant for UI binding.
- ViewModels prevent over-posting by exposing only editable fields on create/edit forms.
- Entities belong in the domain/data layer; view models belong in the presentation layer.
- Mapping between entity and view model happens in the controller or a dedicated mapper at the boundary.
- Passing entities directly to Razor couples views to schema changes and exposes hidden fields to mass assignment.

---

#### Q16. What symptoms appear when business logic is placed in the View?

**Answer:** Business logic in Razor views produces untested calculations, duplicated rules across pages, and inconsistencies between UI, API, PDF, and email outputs. Changes require hunting `.cshtml` files instead of updating a single service.

- Pricing, tax, or discount math in `@{}` blocks bypasses unit tests on services.
- Authorization checks in views can be skipped by crafting requests or using alternate endpoints.
- Magic numbers and policy thresholds scattered in markup drift from domain rules over time.
- Performance issues appear when views perform per-row calculations or service calls inside loops.
- Production bugs surface where the view shows one value but the database or API stores another.

---

#### Q17. What symptoms appear when data access is placed in the Controller?

**Answer:** Data access in controllers creates thick actions with raw SQL or LINQ, makes unit testing require a real database, and mixes HTTP handling with query logic. Changes to queries force controller edits and invite N+1 or transaction bugs.

- Actions grow with `DbContext` queries, `SaveChanges`, and manual mapping inline.
- Controller tests become integration tests needing `WebApplicationFactory` or SQL Server.
- The same query logic gets duplicated when adding API endpoints or background jobs.
- Connection management, retry, and transaction boundaries are ad hoc instead of centralized.
- Security risks increase when SQL or EF queries embedded in controllers lack consistent authorization checks.

---

#### Q18. What is Post-Redirect-Get (PRG) and why is it used in MVC?

**Answer:** Post-Redirect-Get completes a successful POST by returning an HTTP redirect to a GET action instead of rendering the view directly. This prevents duplicate form submissions when the user refreshes the browser after a create or update.

- After processing a valid POST, the action returns `RedirectToAction("Details", new { id })` rather than `return View()`.
- The browser's refresh on the GET URL is safe — it does not resubmit the form body.
- PRG is standard for create, update, and delete operations in server-rendered MVC apps.
- Validation failures typically redisplay the form with `return View(model)` without redirect so `ModelState` errors remain.
- `TempData` can carry flash messages across the redirect when needed.

---

## Chapter 02. Controllers & Actions

#### Q1. What is a controller in ASP.NET Core MVC?

**Answer:** A controller is a class marked with `[ApiController]` or inheriting `Controller`/`ControllerBase` that contains action methods handling HTTP requests. ASP.NET Core discovers controllers through assembly scanning and maps routes to their public action methods.

- MVC controllers inherit `Controller`, gaining view helpers like `View()`, `ViewData`, and `TempData`.
- API controllers inherit `ControllerBase` and typically use attribute routing with `[ApiController]`.
- Controllers are registered implicitly — no manual `AddController` per type — when `AddControllersWithViews()` is called.
- `[Route]`, `[HttpGet]`, and area attributes define how URLs reach specific actions.
- Each controller groups related actions (e.g., `ProductsController` for product CRUD).

---

#### Q2. What is an action method?

**Answer:** An action method is a public instance method on a controller that routing selects to handle a request. It accepts bound parameters, performs work, and returns an `IActionResult` that executes to produce the HTTP response.

- Actions must be public, non-static, and not decorated with `[NonAction]` to be invocable.
- Method name defaults to the action segment in conventional routing (`/Home/Index` → `Index()`).
- Parameters are populated by model binding from route, query, form, and body sources.
- Return types include `IActionResult`, `ActionResult<T>`, or concrete results like `ViewResult`.
- `[HttpGet]`, `[HttpPost]`, and other verb attributes disambiguate overloaded action names.

---

#### Q3. What is `IActionResult` and why use it instead of returning raw objects?

**Answer:** `IActionResult` is the abstraction for executable results — views, redirects, status codes, files, and JSON — that the framework runs to set the HTTP response. It decouples the action's decision from the mechanics of writing the response.

- Helper methods like `View()`, `RedirectToAction()`, `NotFound()`, and `BadRequest()` return typed `IActionResult` implementations.
- Returning a raw object from an API action works with formatters but returning `ActionResult<T>` preserves OpenAPI metadata and typed contracts.
- `IActionResult` enables consistent filter and middleware interaction before the result executes.
- Different result types set headers, status codes, and content independently of the action signature.
- Actions can return heterogeneous outcomes (404 vs 200 view) under one return type.

---

#### Q4. What is the difference between `View()` and `Json()`?

**Answer:** `View()` selects a Razor view, sets `ViewData`, and renders HTML through the view engine with content negotiation toward `text/html`. `Json()` serializes an object to JSON using System.Text.Json (or configured formatters) with `application/json`.

- `View()` uses view discovery (`Views/{Controller}/{Action}.cshtml`) and supports layouts and partials.
- `Json()` is for AJAX endpoints or API-style responses from an MVC controller without a Razor template.
- `View()` can pass a model as `View(model)`; `Json(data)` writes the serialized object directly.
- Returning `View()` to an XHR client expecting JSON produces HTML — a common integration mistake.
- API projects typically use `ControllerBase` with `Ok(dto)` instead of `Json()` for consistent status codes.

---

#### Q5. What is the difference between `RedirectToAction` and `Redirect`?

**Answer:** `RedirectToAction` generates a URL from route values (controller, action, route parameters) using the routing system — so URL changes from route templates stay correct. `Redirect(string url)` sends the client to an explicit absolute or relative URL string.

- `RedirectToAction("Index", "Home", new { id = 5 })` participates in link generation and route constraints.
- `Redirect("/Home/Index/5")` hardcodes the path and breaks if route patterns change.
- `RedirectToAction` supports PRG after POST without hardcoding URLs in controllers.
- `RedirectToRoute` targets a named route instead of controller/action names.
- Both return HTTP 302 (or 301 for permanent) and leave model binding state behind on the new GET.

---

#### Q6. Why should action methods return `Task<IActionResult>` instead of `async void`?

**Answer:** Controller actions must return `Task` or `Task<IActionResult>` so the framework awaits them, captures exceptions, and writes the result to the response. `async void` is fire-and-forget — exceptions crash the process and return values are lost.

- `async void` is reserved for event handlers, not ASP.NET Core action methods.
- Unhandled exceptions from `async void` actions may terminate the worker process.
- An `async void` action that calls `View(model)` without returning it produces an empty response.
- Proper async actions accept `CancellationToken` and propagate cancellation to I/O calls.
- `Task<IActionResult>` lets filters and middleware observe completion and status correctly.

---

#### Q7. How does dependency injection work in MVC controllers?

**Answer:** ASP.NET Core resolves controllers through the DI container, injecting constructor parameters registered in `Program.cs`. Controllers should request dependencies via constructor injection — not `HttpContext.RequestServices` lookups.

- Services are registered with lifetimes: singleton, scoped (typical for `DbContext`), or transient.
- The framework activates a new controller instance per request with scoped dependencies aligned to the request scope.
- `[FromServices]` on a parameter also resolves from DI but constructor injection is the standard pattern.
- `[ActivatorUtilitiesConstructor]` marks which constructor to use when multiple exist.
- Missing registrations fail at startup or first request with `InvalidOperationException`, not silent nulls.

---

#### Q8. What does the `Controller` base class provide?

**Answer:** The `Controller` base class extends `ControllerBase` with view-related helpers and context properties for MVC scenarios. It adds methods like `View()`, `PartialView()`, and access to `ViewData`, `ViewBag`, `TempData`, and `ModelState`.

- `ControllerBase` provides `Ok()`, `BadRequest()`, `NotFound()`, and HTTP context access for API-style results.
- `Controller` adds Razor-specific `View()` overloads and `ViewComponent()` helpers.
- Both expose `HttpContext`, `Request`, `Response`, `User`, and `Url` helper properties.
- `ModelState` on the controller tracks binding and validation errors for the current request.
- JSON-only APIs should inherit `ControllerBase` to avoid pulling in view infrastructure.

---

#### Q9. What is `ModelState` and when should an action check it?

**Answer:** `ModelState` is a dictionary of model binding and validation results keyed by property name, populated during model binding from Data Annotations and manual errors. Actions must check `ModelState.IsValid` before persisting data on POST requests.

- Invalid annotation validation adds errors automatically during binding — but only if the action checks before saving.
- Manual business rule failures call `ModelState.AddModelError("Property", "message")`.
- On failure, return `View(model)` to redisplay the form with validation tag helpers showing errors.
- Skipping the check allows invalid or malicious input to reach the database despite client validation.
- `ModelState` is request-scoped and does not survive redirects without TempData or a second validation pass.

---

#### Q10. What are `[HttpGet]` and `[HttpPost]` used for?

**Answer:** These attributes restrict which HTTP verbs can invoke an action, disambiguating overloads with the same name and enforcing RESTful verb semantics. GET actions display forms or read data; POST actions accept form submissions and mutate state.

- Without verb attributes, two actions named `Create` cause ambiguous routing exceptions.
- Browsers follow links with GET — state changes must use POST, PUT, PATCH, or DELETE.
- `[HttpPost]` pairs with `[ValidateAntiForgeryToken]` on form submissions that change data.
- `[AcceptVerbs("GET", "POST")]` allows multiple verbs on one action when intentional.
- Attribute routing uses HTTP method constraints alongside route templates for action selection.

---

#### Q11. What is `[ValidateAntiForgeryToken]` and when is it required?

**Answer:** `[ValidateAntiForgeryToken]` validates that a POST request includes a matching antiforgery token, preventing cross-site request forgery attacks that trick a logged-in user's browser into submitting unwanted forms. It is required on every state-changing POST action that uses cookie-based authentication.

- The form tag helper automatically renders a hidden `__RequestVerificationToken` field.
- The server compares the form token with the cookie token issued by antiforgery middleware.
- AJAX POSTs must send the token via header `RequestVerificationToken` or form field manually.
- `[AutoValidateAntiforgeryToken]` on the controller applies validation to all unsafe verbs.
- GET actions do not need antiforgery validation because safe methods should not mutate state.

---

#### Q12. What is the difference between `[FromBody]`, `[FromForm]`, and default binding in MVC actions?

**Answer:** Binding source attributes tell model binders where to read values: `[FromBody]` deserializes JSON from the request body, `[FromForm]` reads form fields, and default binding combines route, query, and form values by convention.

- Standard HTML forms post `application/x-www-form-urlencoded` or `multipart/form-data` — not JSON — so `[FromBody]` leaves the model empty.
- `[FromForm]` explicitly binds form fields for POST actions with complex models.
- `[FromRoute]` and `[FromQuery]` target route segments and query string parameters.
- `[FromBody]` typically allows one parameter per action because the body is a single stream.
- Default binding without attributes works for simple parameters and MVC form posts in most scenarios.

---

#### Q13. How are controllers activated per request?

**Answer:** ASP.NET Core uses `IControllerActivator` (default: `DefaultControllerActivator`) to create a controller instance for each request by resolving the type from DI with constructor dependencies. The instance lives for the duration of the action invocation and is eligible for garbage collection afterward.

- Controllers are not singletons — scoped services like `DbContext` align with per-request activation.
- `ActivatorUtilities.CreateInstance` fills constructor parameters from the request's service provider.
- Custom activators can wrap controllers with logging or interceptors but rarely replace default behavior.
- `IDisposable` on controllers is not the recommended resource cleanup pattern — use scoped services instead.
- Controller types do not need explicit registration in DI unless using factory or custom activator patterns.

---

#### Q14. What is the difference between returning `NotFound()` and `BadRequest()`?

**Answer:** `NotFound()` returns HTTP 404, meaning the requested resource does not exist at the given identifier. `BadRequest()` returns HTTP 400, meaning the client sent malformed or invalid input that prevents processing.

- Valid ID format but missing database row → `NotFound()`.
- Null, negative, or whitespace ID when an ID is required → `BadRequest()` or validation problem.
- Inverting these confuses API clients, monitoring alerts, and caching behavior.
- MVC may return HTML error pages; API controllers should return ProblemDetails for consistency.
- `[ApiController]` automatically translates invalid model state to 400 with validation details.

---

#### Q15. Why should controllers avoid a static service locator?

**Answer:** A static service locator hides dependencies, requires global mutable state, bypasses constructor injection, and breaks scoped lifetimes — especially for `DbContext`. ASP.NET Core DI already provides per-request resolution without static access.

- Hidden dependencies make unit testing impossible without mutating global state before each test.
- Resolving scoped services from singletons or static fields causes captive dependency bugs and stale contexts.
- `HttpContext.RequestServices.GetService<T>()` in actions masks missing registrations until runtime.
- Constructor injection makes dependencies explicit, verifiable, and compatible with analyzer tools.
- Static locators were an anti-pattern before Core and remain one in ASP.NET Core 8.

---

#### Q16. What is the purpose of constructor injection in controllers?

**Answer:** Constructor injection declares all dependencies explicitly in the controller's constructor, letting the DI container wire them at activation time. It enables testing with mocks, enforces correct service lifetimes, and fails fast when a service is not registered.

- Dependencies appear as private readonly fields set once at construction.
- The compiler and IDE surface required services — no hidden lookups in action bodies.
- Integration and unit tests pass fake implementations through the constructor without HTTP context.
- ASP.NET Core resolves the graph automatically for each request-scoped controller instance.
- Optional `[ActivatorUtilitiesConstructor]` selects the injectable constructor when multiple exist.

---

#### Q17. What is the difference between conventional routing and attribute routing on a controller?

**Answer:** Conventional routing maps URLs to `{controller}/{action}/{id?}` patterns registered in `Program.cs` with `MapControllerRoute`. Attribute routing places `[Route]` templates directly on controllers and actions, enabling precise, REST-style URLs independent of class names.

- Conventional routing uses naming conventions — `ProductsController.Index` maps to `/Products/Index`.
- Attribute routing supports templates like `[HttpGet("api/products/{id:int}")]` on individual actions.
- Attribute-routed controllers often use `[Route("[controller]")]` at the class level with verb attributes on actions.
- Both can coexist; attribute routes take precedence when both match.
- Tag helpers generate URLs from route names and attribute templates when `asp-controller` and `asp-action` are used.

---

#### Q18. Can you implement `IDisposable` on a controller to manage resources? Why or why not?

**Answer:** Implementing `IDisposable` on a controller is unreliable because ASP.NET Core does not guarantee deterministic disposal timing for controller instances. Unmanaged or expensive resources should live in scoped services that DI disposes at the end of the request.

- Controller lifetime is tied to action invocation, but disposal is not part of the documented cleanup contract.
- Native handles or connections in controller fields risk leaks or double-dispose under concurrency.
- Register `IDisposable`/`IAsyncDisposable` implementations as scoped services and inject them instead.
- `IResourceFilter` can wrap acquire/release around an action if lifetime must match action boundaries.
- The framework disposes scoped services created in the request scope automatically after the response completes.

---

## Chapter 03. Views & Razor Syntax

#### Q1. What is a Razor view?

**Answer:** A Razor view is a `.cshtml` file mixing HTML with C# code using the `@` syntax, compiled into a class that renders HTML output. Razor is the default view engine for ASP.NET Core MVC and supports layouts, partials, tag helpers, and strongly typed models.

- Views live under `Views/{ControllerName}/` or `Views/Shared/` by convention.
- The `@model` directive declares the expected type for compile-time checking and IntelliSense.
- Razor expressions HTML-encode output by default to mitigate XSS.
- Views are precompiled into assemblies at publish in Release builds by default.
- The view engine locates templates using conventional paths and optional area prefixes.

---

#### Q2. What is the `@model` directive?

**Answer:** The `@model` directive at the top of a view declares the strongly typed model type, accessible as `Model` in the markup. It enables compile-time checking, IntelliSense, and clear contracts between controller and view.

- Example: `@model ProductEditViewModel` lets the view use `@Model.Name` with type safety.
- Only one `@model` directive is allowed per view.
- The controller passes the instance via `return View(viewModel)`.
- Without `@model`, the view relies on untyped `ViewData`/`ViewBag` or `dynamic`.
- Partial views can also declare `@model` for reusable typed fragments.

---

#### Q3. What is the difference between `@` and `@@` in Razor?

**Answer:** A single `@` transitions from HTML to C# code — variables, expressions, and directives. `@@` renders a literal `@` character in the HTML output, escaping the Razor transition.

- `@DateTime.Now` evaluates the expression and writes the encoded result.
- `@@` produces `@` in output — useful for email addresses or CSS `@media` in `<style>` blocks.
- Directives like `@model`, `@using`, and `@inject` also start with a single `@`.
- Code blocks use `@{ ... }` for multiple statements without inline output.
- Misusing `@` in CSS or JavaScript strings can accidentally invoke Razor parsing.

---

#### Q4. How does Razor automatically encode output and why does it matter?

**Answer:** Razor HTML-encodes expressions written with `@` before writing them to the response, converting characters like `<`, `>`, and `"` to HTML entities. This prevents browser interpretation of user-supplied content as active markup — the primary defense against XSS in server-rendered pages.

- Encoding applies to `@Model.UserComment` in text and most attribute contexts.
- User content rendered without encoding can execute scripts in victims' browsers.
- Encoding is automatic — developers must explicitly opt out with `@Html.Raw()` for trusted HTML.
- Different output contexts (JavaScript, URLs) may need additional encoding beyond HTML encoding.
- ASP.NET Core 8 Razor uses the same encoding pipeline for all standard `@` expressions.

---

#### Q5. What is the difference between `@Html.Raw` and default Razor output?

**Answer:** Default `@` output HTML-encodes values; `@Html.Raw(string)` writes the string unchanged into the response. Raw should only be used on trusted or server-sanitized HTML — never on unvalidated user input.

- Encoded output displays `<script>` as visible text; Raw executes or injects it as markup.
- Rich text scenarios require an allowlist sanitizer before Raw, not Raw alone.
- Tag helpers and `@` expressions are safe by default for typical display scenarios.
- Stored XSS vulnerabilities commonly come from `@Html.Raw(Model.UserContent)` on database fields.
- Prefer encoding unless the content is known safe or has been sanitized server-side.

---

#### Q6. What is a code block (`@{ }`) in Razor?

**Answer:** A Razor code block wraps arbitrary C# statements that do not directly emit output — variable declarations, loops with manual markup, conditionals, and method calls. It separates control logic from inline expressions.

- `@{ var count = Model.Items.Count; }` declares variables for later use in markup.
- Multi-line logic like `if/else` with HTML mixed inside uses `@if` or code blocks.
- Code blocks should contain presentation logic only — not database queries or business rules.
- `@{}` at the top level runs during view rendering on each request.
- Excessive logic in code blocks signals the need for view models or view components.

---

#### Q7. What is the difference between a strongly typed view and a dynamic view?

**Answer:** A strongly typed view declares `@model MyViewModel` and accesses typed properties via `Model`. A dynamic view uses `ViewBag`, `ViewData`, or no model — relying on runtime dictionary keys or `dynamic` without compile-time checks.

- Strong typing catches property renames at compile time and enables IntelliSense in the view.
- Dynamic views fail silently on typos in `ViewBag.Title` vs `ViewBag.Titel`.
- Strongly typed partials enforce contracts when reused across pages.
- Dynamic data is acceptable for single optional messages but scales poorly on complex pages.
- Controllers pass typed models with `return View(myViewModel)` for the strongly typed approach.

---

#### Q8. What logic should not belong in a Razor view?

**Answer:** Views should not contain business rules, data access, authorization decisions, or complex calculations — only presentation formatting and layout. Any logic that affects correctness, security, or money belongs in services with the controller supplying ready-to-display view models.

- Database queries via `@inject DbContext` in views bypass service-layer testing.
- Pricing, tax, and discount calculations duplicated in views drift from API and batch job logic.
- Authorization checks in Razor can be bypassed by alternate routes or direct API calls.
- Heavy `@{}` blocks with business `if` chains belong in application services or view model mapping.
- Views may format dates and currencies but should not decide business outcomes.

---

#### Q9. What is `@inject` used for in Razor views?

**Answer:** The `@inject` directive requests a service from DI into the view — creating a property the Razor page can use during rendering. It suits small presentation helpers like localization or configuration, not data access or business services.

- Example: `@inject IViewLocalizer Localizer` then `@Localizer["Key"]` in markup.
- Injected services follow DI lifetimes — scoped services align with the request.
- Overusing `@inject` for repositories encourages fat views and N+1 query patterns.
- View Components are the preferred pattern when a view needs its own data-loading logic.
- Services injected into views should be presentation-oriented, not domain repositories.

---

#### Q10. What is the `@functions` block in Razor?

**Answer:** The `@functions` block declares methods and properties on the generated view class — typically small presentation helpers like CSS class mappers or formatters. It is not intended for data access or business logic.

- Example: `@functions { string StatusClass(string s) => s == "Active" ? "green" : "red"; }`
- Functions are callable from the markup in the same view.
- Async data loading in `@functions` is an anti-pattern — load data in the controller or view component.
- Helpers duplicated across views should move to shared partials, tag helpers, or static helper classes.
- `@functions` compiles into the view's generated class at build or runtime compilation time.

---

#### Q11. What is the difference between Razor runtime compilation and precompilation?

**Answer:** Precompilation compiles `.cshtml` files into assemblies at build or publish time, so production serves views from DLLs without Roslyn at runtime. Runtime compilation reads `.cshtml` from disk and compiles on demand with file watching — intended for Development hot reload.

- Release publish defaults to precompiled views in `ProjectName.Views.dll`.
- Runtime compilation requires `AddRazorRuntimeCompilation()` and the runtime compilation NuGet package.
- Precompilation catches view syntax errors at build time instead of first request.
- Runtime compilation adds CPU overhead and requires `.cshtml` files on the server.
- Production should use precompilation; runtime compilation should be guarded with `IsDevelopment()`.

---

#### Q12. When would you enable `AddRazorRuntimeCompilation`?

**Answer:** Enable `AddRazorRuntimeCompilation` during local Development so editing `.cshtml` files takes effect without rebuilding the entire project. It should not run in Production because it adds compile cost, requires source files on disk, and widens the attack surface if files can be modified.

- Register conditionally: `if (builder.Environment.IsDevelopment()) mvcBuilder.AddRazorRuntimeCompilation();`
- Useful when iterating on layout, CSS classes, and markup with fast feedback.
- Staging and Production rely on publish artifacts and redeploy to change views.
- Missing guard means view edits on a server apply immediately — a misconfiguration signal.
- Requires the `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` package.

---

#### Q13. What is a partial view and when do you use one?

**Answer:** A partial view is a reusable Razor fragment without a layout, rendered into a parent view for shared markup such as form fields, cards, or pagination. Use partials to avoid copy-pasting HTML across multiple views with the same UI component.

- Partial views live in `Views/Shared/` or controller-specific folders, often prefixed with `_`.
- Pass a strongly typed model with `<partial name="_ProductCard" model="item" />`.
- Partials do not run `_ViewStart` layout wrapping — they render inline content only.
- Use partials for static markup reuse; use View Components when independent data loading is needed.
- Returning `PartialView()` from an action sends HTML fragments for AJAX replacement.

---

#### Q14. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?

**Answer:** Both render a partial view, but `<partial>` is the recommended tag helper syntax while `Html.PartialAsync` is the older HTML helper API. The tag helper integrates with Razor tooling and avoids some synchronous rendering pitfalls of older helper patterns.

- `<partial name="_LoginPartial" model="Model.User" />` is declarative and preferred in ASP.NET Core 8.
- `PartialAsync` returns `Task<IHtmlContent>` and must be awaited: `@await Html.PartialAsync("_Name", model)`.
- Avoid `Html.Partial` (sync) — it can deadlock in certain contexts; use `PartialAsync` or the tag helper.
- Both resolve partial views by name using the same view engine location expander.
- Tag helpers participate in Razor compilation and attribute IntelliSense.

---

#### Q15. What is `ViewData` and how is it accessed in Razor?

**Answer:** `ViewData` is a `ViewDataDictionary` passed from controller to view for untyped key-value data surviving a single request. Access values with string keys: `ViewData["Title"]` in the controller and `@ViewData["Title"]` in the view.

- Values are `object` — casts may fail silently or require `(string)ViewData["Key"]`.
- `ViewData` shares backing storage with `ViewBag` in the same request.
- Common for page titles and layout metadata when a full view model is not used.
- Magic string keys typo easily — shared constants or strongly typed models are safer.
- `ViewData` does not survive redirects; use `TempData` for cross-request flash data.

---

#### Q16. What causes "The view 'X' was not found" errors?

**Answer:** The view engine cannot locate a matching `.cshtml` file or precompiled view for the requested name — usually due to wrong path conventions, case sensitivity on Linux, missing publish artifacts, or explicit view names that do not exist.

- `return View()` expects `Views/{Controller}/{Action}.cshtml` unless a different name is passed.
- Linux deployments are case-sensitive — `Index.cshtml` vs `index.cshtml` fails on Linux only.
- Publish output may lack `Views/` folder or `ProjectName.Views.dll` if misconfigured.
- Area views require `Areas/{Area}/Views/{Controller}/{View}.cshtml`.
- Typos in `return View("CustomName")` or wrong controller name in routing cause mismatches.

---

#### Q17. What is `_ViewImports.cshtml` used for?

**Answer:** `_ViewImports.cshtml` applies shared directives to all views in its folder and subfolders — `@using` namespaces, `@addTagHelper`, `@inject`, and `@model` inheritance via `@inherits` patterns. It reduces repetition across views.

- Place `@using MyApp.ViewModels` once instead of in every view.
- `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` enables tag helpers project-wide.
- Nested `_ViewImports.cshtml` in areas merge with parent imports.
- It does not render HTML — only sets compilation context for child views.
- Root `Views/_ViewImports.cshtml` applies to all views unless overridden deeper.

---

#### Q18. What is `_ViewStart.cshtml` used for?

**Answer:** `_ViewStart.cshtml` runs before each view renders, typically setting the layout with `Layout = "_Layout"`. It centralizes layout assignment so individual views do not repeat the layout directive.

- Located at `Views/_ViewStart.cshtml` and optionally in area view folders.
- Sets `Layout` property — views can override with `Layout = null` or a different layout path.
- Executes in hierarchical order from root to area-specific `_ViewStart`.
- Does not replace `_ViewImports` — imports handle namespaces; view start handles layout.
- Child views focus on content while `_ViewStart` wraps them in the shared chrome.

---

---

## Chapter 04. Layouts, Sections & Partial Views

#### Q1. What is a layout in ASP.NET Core MVC?

**Answer:** A layout is a Razor view that defines the shared page shell — HTML document structure, navigation, CSS/JS references, and footer — while individual views supply only their page-specific content. Layouts eliminate duplicating chrome across every `.cshtml` file and keep site-wide markup in one place.

- Layouts live typically in `Views/Shared/` (e.g., `_Layout.cshtml`) and are applied via `_ViewStart.cshtml` or a per-view `Layout` property.
- The layout calls `@RenderBody()` where the child view's markup is injected during rendering.
- Child views can optionally define named `@section` blocks that the layout renders at specific positions (e.g., scripts at the bottom).
- A view can opt out with `Layout = null` for standalone pages such as login or print views.

---

#### Q2. What does `@RenderBody()` do in a layout?

**Answer:** `@RenderBody()` is the placeholder in a layout where the rendering engine inserts the content of the currently executing view. Without it, the child view's markup is discarded and the page appears blank even though compilation succeeds.

- It is called exactly once per layout — the primary content channel for the view pipeline.
- Content placed outside `@section` blocks in the child view is captured into the body; section content is routed separately.
- Nested layouts each have their own `@RenderBody()` — the inner layout's body receives the page view, and the outer layout's body receives the fully rendered inner layout output.
- Missing `@RenderBody()` is a common silent bug: the layout renders headers/footers but no main content.

---

#### Q3. What is a section in Razor (`@section`)?

**Answer:** A section is a named content block defined in a child view with `@section SectionName { ... }` and rendered in the layout with `@RenderSection("SectionName")`. Sections let pages inject optional or required fragments — such as page-specific CSS, scripts, or a hero banner — into predetermined layout slots.

- Sections are declared in the view, not the layout; the layout decides where and whether to render them.
- Common pattern: `@section Scripts { <script src="~/js/page.js"></script> }` rendered at the bottom of `_Layout.cshtml`.
- Section content is captured during view execution and deferred until the layout reaches the matching `@RenderSection` call.
- Unlike `@RenderBody()`, a view can define multiple sections with different names.

---

#### Q4. What is the difference between `@RenderSection("Scripts", required: true)` and `required: false`?

**Answer:** The `required` parameter controls whether the layout throws at render time when a view omits that section. `required: true` enforces a contract — every view using the layout must define the section (even if empty); `required: false` silently skips rendering when the section is undefined.

- `required: true` throws `InvalidOperationException` if the view does not contain `@section Scripts { ... }` — useful when every page must participate in a shared script pipeline.
- `required: false` is appropriate for optional analytics, SEO meta tags, or page-specific styles that only some views need.
- Use `IsSectionDefined("Scripts")` in the layout to conditionally wrap optional section output without relying on exceptions.
- Mixed policies across a site cause inconsistent behavior: some pages crash, others silently omit scripts.

---

#### Q5. What is `_ViewStart.cshtml` and how does it apply layouts?

**Answer:** `_ViewStart.cshtml` runs before every view in its folder and subfolders, setting shared view properties — most commonly `Layout = "_Layout"`. It centralizes layout assignment so individual views do not repeat the same `Layout` line.

- The file executes in order from the view's folder upward; the closest `_ViewStart.cshtml` to the view wins for properties it sets.
- Root `Views/_ViewStart.cshtml` applies a default layout to all views unless a subfolder overrides it.
- A child folder's `_ViewStart.cshtml` can set `Layout = null` or a different layout name, affecting only views under that path.
- `_ViewStart` runs at compile/render time — it is not a view users see; it configures the view execution context.

---

#### Q6. What is the difference between a layout and a partial view?

**Answer:** A layout wraps an entire page and defines the outer document structure with `@RenderBody()` and sections; a partial view is a reusable Razor fragment rendered inside a parent view or layout. Layouts establish the page template; partials compose smaller UI pieces within that template.

- Layouts are applied automatically via `_ViewStart` or the `Layout` property; partials are invoked explicitly with `<partial>` or `Html.PartialAsync`.
- A page has one layout (or none); it can include many partials (`_ValidationScriptsPartial`, `_LoginPartial`).
- Layouts participate in the view-start pipeline and section forwarding; partials inherit the parent's `ViewData`/`ViewBag` unless given an explicit model.
- Partials do not replace `@RenderBody()` — they render inline wherever invoked.

---

#### Q7. How do nested layouts work?

**Answer:** Nested layouts chain by setting `Layout = "_ParentLayout"` in an intermediate layout file. The innermost view fills the inner layout's `@RenderBody()`, and the rendered inner layout output fills the outer layout's `@RenderBody()` — building the page inside-out.

- Example: `_AdminLayout.cshtml` sets `Layout = "_Layout"` and provides admin sidebar chrome around `@RenderBody()`.
- Each layout level must call `@RenderBody()` once; only the outermost layout typically contains `<html>` and `<head>`.
- Sections do not automatically bubble up — intermediate layouts must forward them with `@section Scripts { @RenderSection("Scripts", required: false) }`.
- Missing section forwarding in a middle layout is a common cause of scripts defined in a page never reaching the root layout.

---

#### Q8. How are `@section` definitions passed from a view to a layout?

**Answer:** When a view defines `@section Scripts { ... }`, Razor captures that block during view execution and stores it until the active layout calls `@RenderSection("Scripts")`. The layout then renders the captured content at that call site — typically after shared scripts so page-specific code runs last.

- Section matching is by name — the string in `@section` must match the argument to `@RenderSection`.
- If multiple layout levels exist, each intermediate layout must explicitly forward sections to the parent layout.
- Content outside any section becomes part of `@RenderBody()`; section content is excluded from the body stream.
- Sections execute in the context of the defining view, so they can reference the view's `@model` and local variables.

---

#### Q9. Can a view define the same section name twice? What happens?

**Answer:** No — Razor allows each section name only once per view. Defining `@section Scripts` twice in the same view causes a compile-time error; sections do not merge like duplicate HTML tags.

- The compiler reports a duplicate section definition before the app runs.
- To combine script blocks, merge them into a single `@section Scripts { ... }` or extract shared scripts into a partial invoked inside one section.
- This strict rule differs from calling `Html.PartialAsync` multiple times, which is allowed.
- Nested layouts may each define forwarding sections — the restriction applies per view file, not per layout chain.

---

#### Q10. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?

**Answer:** Both render a partial view asynchronously, but the `<partial>` tag helper is the recommended ASP.NET Core 8 approach — cleaner syntax, full DI integration, and consistent with other tag helpers. `Html.PartialAsync` is the older HTML helper API; functionally similar for simple cases.

- `<partial name="_MyPartial" model="Model.Items" />` avoids `@await` in many contexts and reads clearly in markup.
- `Html.PartialAsync` returns `IHtmlContent` and requires `@await` — it remains valid for dynamic partial names computed at runtime.
- Both pass an optional model and `ViewDataDictionary`; neither runs a controller — they only render existing Razor.
- Prefer `<partial>` in new code; use `PartialAsync` when the partial name is built programmatically in C# code within the view.

---

#### Q11. How does Razor locate partial views?

**Answer:** Razor searches a defined set of locations relative to the calling view, starting with the current controller's view folder, then `Views/Shared/`, using the partial name (with or without leading underscore). The first matching file wins.

- For a partial named `_ProductTile`, search paths include `Views/{Controller}/_ProductTile.cshtml` and `Views/Shared/_ProductTile.cshtml`.
- Explicit paths such as `~/Views/Shared/_ProductTile.cshtml` skip ambiguous discovery.
- Partial resolution is based on the **calling view's** context, not the layout's location.
- A partial in `Views/Shared/` with the same name as one in an Area can cause unexpected binding — use distinct names or fully qualified paths.

---

#### Q12. How does partial view resolution differ in Areas?

**Answer:** Area views add area-specific search paths via the area view location expander. Partials invoked from an Area view are searched under `Areas/{AreaName}/Views/{Controller}/`, then `Areas/{AreaName}/Views/Shared/`, then application `Views/Shared/`.

- Area isolation is not absolute — root `Views/Shared/` remains in the search order and can shadow Area intent if names collide.
- Use `~/Areas/Billing/Views/Shared/_LineItems.cshtml` when you must guarantee the Area version.
- Layout resolution in Areas follows a similar expanded path — Area `_ViewStart` controls which layout file is selected.
- View Components use separate discovery via `[AreaViewLocationFormats]` and are often clearer for Area-specific widgets with data loading.

---

#### Q13. What is the difference between `@RenderSection` and `@await Html.PartialAsync`?

**Answer:** `@RenderSection` renders content **defined by the child view** into a layout slot — a top-down contract between view and layout. `Html.PartialAsync` renders a **separate Razor file** inline — a horizontal composition of reusable fragments, independent of the layout section pipeline.

- Sections are declared in the page view and consumed by the layout; partials are standalone files invoked from any view or layout.
- Sections support the optional/required flag; partials always render when called (or throw if not found).
- Partials accept an explicit model parameter; sections inherit the defining view's scope.
- Use sections for page-specific scripts/styles destined for layout placeholders; use partials for shared UI fragments used across many pages.

---

#### Q14. Why pass a strongly typed model to a partial instead of `ViewBag`?

**Answer:** A strongly typed model gives compile-time checking, IntelliSense, and explicit data contracts — the partial declares `@model MyType` and receives only what it needs. `ViewBag` is dynamic, untyped, and invisible to refactoring tools, making partials fragile and hard to test.

- `model="Model.Lines"` documents exactly what the partial requires without relying on magic string keys.
- Strong typing prevents null-reference and typo errors that `ViewBag.Orders` hides until runtime.
- Unit tests and view components can construct the model directly; `ViewBag` requires dictionary setup.
- Reserve `ViewBag`/`ViewData` for rare layout-level messages — not for structured data passed to partials.

---

#### Q15. How does a child view override the layout assigned in `_ViewStart`?

**Answer:** A view overrides `_ViewStart` by setting the `Layout` property at the top of the `.cshtml` file — for example `@{ Layout = "_PrintLayout"; }` or `@{ Layout = null; }`. The view-level assignment takes precedence over `_ViewStart` for that view only.

- Override syntax: `@{ Layout = "~/Views/Shared/_AdminLayout.cshtml"; }` using a fully qualified path when needed.
- `Layout = null` renders the view as a standalone HTML page without any layout wrapper.
- `_ViewStart` still runs first; the view's own `Layout` assignment replaces the value `_ViewStart` set.
- Per-action layout selection can also be done in the controller with `return View("Name", "_LayoutName")` overloads.

---

#### Q16. What happens if a required section is not defined in a view?

**Answer:** When the layout calls `@RenderSection("Scripts", required: true)` and the view omits `@section Scripts`, Razor throws `InvalidOperationException` at render time — the page fails with a 500 error. This is intentional enforcement of a layout contract.

- The error identifies the missing section name and the layout file — it only appears when that view is rendered, not at build time.
- Fix by adding `@section Scripts { }` (even empty) or changing the layout to `required: false` if the section is truly optional.
- Smoke-testing every view against its layout prevents production surprises when a new layout adds a required section.
- Alternatively, provide default scripts in the layout and use `IsSectionDefined` to append page-specific scripts optionally.

---

#### Q17. What is the `Shared` folder under `Views` used for?

**Answer:** `Views/Shared/` holds views shared across controllers — layouts (`_Layout.cshtml`), partials (`_ValidationScriptsPartial.cshtml`), error pages, and editor templates. Razor searches here when a view or partial is not found in the controller-specific folder.

- Layout files, `_ViewImports.cshtml`, and `_ViewStart.cshtml` commonly live in or apply to Shared.
- Partials used by multiple controllers belong in Shared to avoid duplication under each controller folder.
- Editor templates and display templates for `Html.EditorFor`/`DisplayFor` also reside under `Shared/EditorTemplates/` and `Shared/DisplayTemplates/`.
- Area-specific shared views go in `Areas/{AreaName}/Views/Shared/` for Area-scoped reuse.

---

#### Q18. How does `_ViewStart` layout resolution work in Areas?

**Answer:** Area views run `Areas/{AreaName}/Views/_ViewStart.cshtml` first, then fall back to root `Views/_ViewStart.cshtml` if the Area file does not exist or does not set all properties. Layout name resolution uses area-expanded paths — `_Layout` may resolve to `Areas/Admin/Views/Shared/_Layout.cshtml` before root Shared.

- Set Area `_ViewStart` explicitly: `Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml"` to avoid accidentally picking the root site layout.
- An unused `Areas/Admin/Views/Shared/_Layout.cshtml` has no effect until `_ViewStart` references it.
- Asset paths in Area layouts should use `~/` application-root syntax — not relative `../css/admin.css` — to avoid 404s when the wrong layout renders.
- Nested `_ViewStart` files inside `Areas/Admin/Views/Dashboard/` can further override layout for that subfolder only.

---

## Chapter 05. ViewModels & Strongly Typed Views

#### Q1. What is a ViewModel in ASP.NET Core MVC?

**Answer:** A ViewModel is a plain C# class shaped specifically for a view's display and input needs — containing only the properties, validation rules, and UI metadata that a particular Razor page requires. It decouples the presentation layer from domain entities and database schema.

- ViewModels live in the web project (or a shared contracts project) — not in the domain or EF entity layer.
- Each screen or partial typically has its own ViewModel (`ProductEditViewModel`, `OrderListItemViewModel`) rather than one generic type.
- Controllers map entities to ViewModels on GET and map ViewModels back to entities or commands on POST.
- Strongly typed views declare `@model MyViewModel` and get compile-time checking for property access.

---

#### Q2. What is the difference between a domain entity and a ViewModel?

**Answer:** A domain entity models business concepts and persistence — ids, navigations, concurrency tokens, and database constraints. A ViewModel models a UI concern — display labels, dropdown options, formatted read-only fields, and whitelisted editable inputs.

- Entities carry EF annotations, navigation properties, and internal fields (`InternalMargin`, `RowVersion`); ViewModels expose only what the user should see or edit.
- Entities change when the database schema changes; ViewModels change when the UI changes — independent lifecycles.
- Passing entities to views couples Razor to EF and invites over-posting; ViewModels define an explicit trust boundary.
- Mapping between them happens in the controller, a dedicated mapper, or an application service — never by exposing entities directly.

---

#### Q3. What is a strongly typed view?

**Answer:** A strongly typed view declares its model type with `@model MyViewModel` at the top, giving the Razor compiler and IDE knowledge of available properties. Access uses `Model.PropertyName` with IntelliSense, compile-time errors on typos, and clear data contracts.

- Contrast with untyped views that rely on `ViewBag`/`ViewData` dynamic access — no compile-time safety.
- Tag helpers such as `asp-for="PropertyName"` require a strongly typed model to generate correct `name`, `id`, and validation attributes.
- The controller passes the model via `return View(viewModel)` — ASP.NET Core sets `ViewData.Model` automatically.
- Partial views can also be strongly typed with their own `@model` distinct from the parent page model.

---

#### Q4. Why should you not pass EF entities directly to Razor views?

**Answer:** EF entities expose the full persistence graph — navigation properties, shadow properties, and internal columns — to HTML and model binding. This causes over-posting on POST, lazy-load surprises in the view, schema coupling, and circular reference errors when serializing for AJAX.

- Hidden or `[BindNever]` fields on entities do not stop attackers from posting extra form keys.
- Navigation properties may trigger unintended database queries during rendering (N+1 in views).
- EF change-tracker conflicts arise when GET loads a tracked entity and POST binds a second instance of the same type.
- ViewModels project flat, intentional shapes — `CustomerName` as a string instead of a `Customer` navigation object.

---

#### Q5. What is over-posting (mass assignment) and how do ViewModels prevent it?

**Answer:** Over-posting occurs when model binding sets properties the user should not control — such as `IsAdmin` or `DiscountPercent` — because those properties exist on the bound type even if the Razor form omits them. Attackers add extra form fields via dev tools to escalate privileges or change prices.

- ViewModels whitelist only editable fields — properties not on the ViewModel cannot be bound from the POST.
- Server-side mapping copies only ViewModel properties onto the tracked entity; undeclared POST keys are ignored.
- `[Bind(Include = "...")]` on entities is a weaker alternative — ViewModels are the preferred MVC pattern.
- AutoMapper `ReverseMap()` can reintroduce over-posting if it maps all entity members — use explicit allow lists on POST.

---

#### Q6. What is the difference between a Create ViewModel and an Edit ViewModel?

**Answer:** Create and Edit ViewModels reflect different trust boundaries — create has no id, requires identity fields like `Sku`, and omits admin-only flags; edit carries an id (often hidden), treats immutable fields as read-only, and may include concurrency tokens.

- `CreateProductViewModel`: no `ProductId`, required `Sku`, no `IsDiscontinued` on insert forms.
- `EditProductViewModel`: route-bound id, display-only `Sku`, mutable fields only, optional `[Timestamp] RowVersion`.
- Separate actions (`Create` POST vs `Edit` POST) allow distinct authorization and validation rules.
- A single god ViewModel for both flows risks id tampering (`ProductId = 0` overwriting rows) and wrong validation on read-only fields.

---

#### Q7. What is the purpose of a read-only/details ViewModel?

**Answer:** A details ViewModel exposes only display-safe fields for read-only pages — formatted dates, computed totals, status labels — with no editable inputs or sensitive internal data. It prevents accidental round-tripping, hidden-field leakage, and edit-template mistakes on read pages.

- Contains display strings (`CustomerDisplayName`, `FormattedTotal`) rather than raw FK ids and navigations.
- Excludes internal notes, margin percentages, audit timestamps, and PII not needed for the viewer's role.
- Details views use plain text or `@Html.DisplayFor` — not `asp-for` tag helpers that generate hidden inputs.
- Separate from edit ViewModels even when showing the same entity — different shape, different authorization.

---

#### Q8. Why should sensitive fields (e.g., `IsAdmin`, internal margin) be excluded from ViewModels?

**Answer:** Any property on a bound ViewModel can be set via crafted POST requests regardless of whether Razor rendered an input for it. Sensitive fields must not exist on user-facing ViewModels — role flags and internal pricing belong on admin-only ViewModels behind authorization.

- HTML hidden fields are not security — attackers POST arbitrary key/value pairs.
- Internal margin or cost data in ViewModels may appear in HTML source even on read-only pages if templates leak them.
- Admin operations should use separate endpoints, ViewModels, and `[Authorize(Roles = "Admin")]` guards.
- Compliance (PII, financial data) requires minimizing data exposed to the browser, not just hiding it visually.

---

#### Q9. What is the difference between mapping in the controller vs using AutoMapper?

**Answer:** Manual mapping in the controller assigns each property explicitly — maximum control, no magic, easy to audit for over-posting. AutoMapper uses convention-based profiles to reduce boilerplate but can hide security gaps if profiles use broad `ReverseMap()` or `ForAllMembers` rules.

- Manual: `order.ShipDate = vm.ShipDate` — verbose but safe and obvious in code review.
- AutoMapper: `CreateMap<Order, OrderEditViewModel>()` for GET; POST maps only allowed members with explicit ignores.
- AutoMapper `ProjectTo<T>()` helps read-only lists with EF `IQueryable` projection — still inspect generated SQL.
- Prefer manual mapping on security-sensitive POST paths; use AutoMapper for complex GET projections off the hot path.

---

#### Q10. What problems occur when one DTO is shared between MVC views and REST APIs?

**Answer:** A shared DTO forces conflicting serialization and exposure rules — API clients need full contracts with camelCase JSON, while MVC forms need whitelisted, human-oriented fields without secrets. One type leads to `[JsonIgnore]` hacks, accidental cost leakage in HTML, and breaking API changes when the UI changes.

- MVC ViewModels answer "what does this form render and round-trip?"; API DTOs answer "what does the JSON contract look like?"
- API versioning and OpenAPI metadata do not belong on ViewModels; display names and select lists do not belong on API DTOs.
- Share mapping from the domain entity to each outward type — do not inherit API DTOs from ViewModels.
- A thin shared record for common scalars (`Name`, `Sku`) is acceptable; diverge outward types for each consumer.

---

#### Q11. How should navigation properties be handled in ViewModels for partial views?

**Answer:** Flatten navigations into display-friendly scalar properties on the ViewModel — `CustomerName`, `CategoryLabel` — rather than passing EF navigation objects to partials. Partials receive small, focused ViewModels with only the fields they render.

- `_OrderLinePartial` uses `@model OrderLineViewModel` with `ProductName` and `Quantity` — not `@model OrderLine` with `Product` navigation.
- Populate flattened fields via LINQ projection in the controller or query layer before the view runs.
- Avoid `@inject` + lazy loading in partials inside loops — batch data upstream into the ViewModel.
- Nested ViewModels (`AddressViewModel` inside `CheckoutViewModel`) are fine when the partial owns that subgraph — still no EF types.

---

#### Q12. Where should validation attributes be placed — entity or ViewModel?

**Answer:** Input validation attributes (`[Required]`, `[StringLength]`, `[Range]`) belong on the ViewModel — the type MVC binds and validates on POST. Database constraints (max length, precision, indexes) belong in EF Fluent API on entities; domain rules belong in services or domain validators.

- MVC validates the action parameter type — if that is the ViewModel, entity annotations are bypassed when mapping skips the entity during binding.
- Entity annotations tie business rule changes to migrations and the wrong layer.
- Use `IValidatableObject` or FluentValidation on ViewModels for cross-field rules (password confirmation, date ranges).
- Keep `[Timestamp]` and DB-specific attributes on entities; keep `[Compare]`, `[Display]`, and form rules on ViewModels.

---

#### Q13. What is the difference between presentation logic and business logic in ViewModels?

**Answer:** Presentation logic formats data for display — computed labels, select-list options, `bool` to "Yes"/"No" strings, and `[Display(Name = "...")]` metadata. Business logic enforces domain rules — pricing caps, inventory checks, authorization decisions — and belongs in services, not ViewModels.

- Acceptable in ViewModel: `public string StatusLabel => IsActive ? "Active" : "Inactive";` for display.
- Not acceptable: `public decimal Total => Lines.Sum(l => l.Qty * l.GetDiscountedPrice());` with DB calls or domain rules.
- Business validation that must hold regardless of UI belongs in the domain/service layer, duplicated-checked on POST after ViewModel validation passes.
- ViewModels may hold UI state (`SelectedCategoryId`, `AvailableCategories` list) — not transactional behavior.

---

#### Q14. How do ViewModels help with unit testing controllers?

**Answer:** ViewModels are plain POCOs — tests construct them directly, invoke controller actions, and assert on `ViewResult` models without spinning up EF, SQL, or Razor. Tests verify mapping, validation gates, and authorization without rendering views.

- Arrange: `var vm = new EditProductViewModel { ProductId = 1, Name = "Test" };`
- Act: `var result = await controller.Edit(1, vm) as ViewResult;`
- Assert: `Assert.IsType<EditProductViewModel>(result.Model); Assert.False(controller.ModelState.IsValid);`
- Mock services return entities; controller maps to/from ViewModels — tests stay focused on HTTP/orchestration concerns.

---

#### Q15. What is a composite/page ViewModel and when might you split it?

**Answer:** A composite ViewModel aggregates multiple UI regions into one `@model` for a dashboard or wizard page. Split it when the page grows many unrelated properties, partials reuse the entire model, or widgets need independent queries and authorization — decompose into View Component ViewModels instead.

- God ViewModels (40+ properties, nested lists) create merge conflicts, authorization leaks in partials, and expensive single queries.
- Split by widget boundary: `OrdersGridViewModel`, `ProfileCardViewModel` — each loaded by a View Component in the same HTTP request.
- Page shell ViewModel holds layout metadata only; heavy widgets use `Component.InvokeAsync` with focused types.
- Avoid splitting into dozens of AJAX round-trips unless a widget is genuinely slow — View Components preserve one page load.

---

#### Q16. Why can returning an entity to `PartialView` cause serialization errors?

**Answer:** EF entities with bidirectional navigations (`Order.Customer.Orders`) form object graphs that System.Text.Json cannot serialize by default — it throws a cycle detection exception. Even Razor partials risk accidental enumeration of circular graphs through debugging or custom helpers.

- `return PartialView("_OrderSummary", order)` passes a graph, not a flat DTO — AJAX or SignalR endpoints reusing the same action hit JSON serializers.
- Lazy-loaded navigations cause unpredictable query counts and null references in partials.
- Define `OrderSummaryViewModel` with flat fields; project in LINQ before passing to the partial.
- `ReferenceHandler.IgnoreCycles` is a band-aid — fix the contract with ViewModels.

---

#### Q17. What is `[BindNever]` and when is it used on ViewModels?

**Answer:** `[BindNever]` tells the model binder to skip a property during POST binding — the binder will not set that property from form values even if keys are present. On ViewModels it protects properties populated only on GET (dropdown lists, display ids) from being overwritten by tampered POST data.

- Apply to `AvailableCategories`, `RowVersion` display copies, or route-supplied ids set server-side after binding.
- Prefer omitting sensitive properties from the ViewModel entirely over `[BindNever]` — exclusion beats opt-out.
- On entities, `[BindNever]` on navigations prevents binding graphs — but ViewModels are the primary defense.
- `[BindNever]` does not remove properties from GET rendering — it only affects inbound binding.

---

#### Q18. How do nullable reference types affect ViewModel design?

**Answer:** Nullable reference types (`#nullable enable`) must align with HTML form semantics — optional text fields need `string?`, required fields need `[Required]` with `= ""` or the `required` modifier. Model binding sets omitted optional fields to `null`, which contradicts non-nullable `string` declarations and causes `NullReferenceException` in `.Trim()` calls.

- Optional: `public string? MiddleName { get; set; }` — use `model.MiddleName?.Trim()`.
- Required: `public string FirstName { get; set; } = "";` plus `[Required]` — satisfies NRT and binding.
- Non-nullable reference without initializer triggers CS8618 — fix with defaults, not `#nullable disable` suppression.
- NRT on ViewModels reflects form optional/required behavior, not necessarily database nullability.

---

## Chapter 06. Model Binding in MVC

#### Q1. What is model binding in ASP.NET Core MVC?

**Answer:** Model binding is the framework mechanism that maps HTTP request data — form fields, query strings, route values, and headers — onto action method parameters and complex object properties. It runs before the action executes, populating ViewModels and primitives from the incoming request.

- Value providers supply key/value pairs; model binders convert strings to CLR types and set properties recursively on complex types.
- Validation runs during binding when Data Annotations or `IValidatableObject` are present — results accumulate in `ModelState`.
- Default binding source for MVC controller actions is form + route + query — not JSON body unless `[FromBody]` is specified.
- Custom binders implement `IModelBinder` for non-standard formats (currency parsing, composite keys).

---

#### Q2. How does model binding work for HTML form POSTs?

**Answer:** Browser forms POST `application/x-www-form-urlencoded` or `multipart/form-data` key/value pairs. The form value provider reads these keys, and the complex object model binder matches names to property paths — `ProductName` → `model.ProductName`, `Address.City` → `model.Address.City` — building the object graph.

- Tag helpers generate `name` attributes that match the ViewModel property path automatically via `asp-for`.
- Route values and query strings also participate unless restricted with `[FromForm]`.
- Binding order and culture affect parsing of dates and numbers from text inputs.
- After binding, check `ModelState.IsValid` before persisting — MVC does not auto-reject invalid models like `[ApiController]` does.

---

#### Q3. What is the difference between `[FromForm]` and `[FromBody]` in MVC?

**Answer:** `[FromForm]` binds from form fields, query, and route values — the standard HTML form path. `[FromBody]` binds from the HTTP request body using input formatters (typically JSON) — the standard Web API path. They read different parts of the request and use different parsers.

- MVC Razor form POSTs should use default binding or `[FromForm]` — not `[FromBody]`.
- `[FromBody]` requires `Content-Type: application/json` and a JSON-serializable body.
- A single action can combine `[FromRoute] int id` with `[FromForm] MyViewModel model`.
- Mixing them incorrectly produces empty models without obvious errors — the action runs with default values.

---

#### Q4. Why does `[FromBody]` fail when posting a standard HTML form?

**Answer:** Standard HTML forms submit `application/x-www-form-urlencoded` or `multipart/form-data` in the body, but `[FromBody]` expects JSON parsed by the JSON input formatter. The form value provider is bypassed, so the model binder finds no JSON body to deserialize and leaves the model empty.

- Browser `<form method="post">` never sends JSON unless JavaScript intercepts and converts the payload.
- `ModelState` may appear valid because no conversion errors occurred — properties simply stayed at defaults.
- Fix: remove `[FromBody]` for traditional MVC forms; keep it only for AJAX/API endpoints with explicit JSON clients.
- Symmetric trap: posting JSON to an action without `[FromBody]` also yields an empty model.

---

#### Q5. How are collection properties bound from form fields (`Lines[0].Sku`)?

**Answer:** Collection binding uses indexed field names — `Lines[0].Sku`, `Lines[0].Qty`, `Lines[1].Sku` — to populate `List<T>` or `T[]` properties. The binder creates list entries at each index and sets nested properties on each element.

- Tag helpers and editor templates generate indexed names automatically for dynamic lists.
- Indices must be **contiguous starting at zero** for reliable binding — gaps cause null/default entries at missing indices.
- Dictionary binding uses similar syntax: `Lines[SKU-123].Qty`.
- After binding, filter empty rows server-side if the UI may submit blank template rows.

---

#### Q6. What happens when collection indices are non-contiguous after deleting a row?

**Answer:** If the client deletes row index 1 but leaves `Lines[0]` and `Lines[2]`, the binder creates a list with a default/null element at index 1 and data at index 2. Server logic iterating all indices may process phantom rows or misalign SKUs with quantities.

- Empty `LineItemViewModel` at gap indices may pass weak validation and corrupt updates.
- Fix: re-index rows in JavaScript before submit so indices are `0, 1, 2, ...` without gaps.
- Server-side: `model.Lines.Where(l => !string.IsNullOrEmpty(l.Sku))` before processing.
- Prefer stable keys (SKU-based) with a custom binder for complex editable grids.

---

#### Q7. How does model binding handle nested objects (`Address.City`)?

**Answer:** Nested object binding uses dotted property paths in form field names — `Address.City`, `Address.PostalCode` — to populate child object properties on the parent ViewModel. The binder instantiates nested types as needed when values are present.

- Tag helpers with `asp-for="Address.City"` emit the correct prefixed names automatically.
- `Html.EditorFor(m => m.Address)` renders templates with proper prefix.
- Partial views must preserve prefix — `<partial for="Address" />` or `ViewData.TemplateInfo.HtmlFieldPrefix = "Address"`.
- Raw `name="City"` in a partial loses the `Address.` prefix and binds to a top-level `City` property instead.

---

#### Q8. How do partial views affect model binding prefix for nested properties?

**Answer:** Partial views rendered without a prefix inherit only the partial's `@model` property names — `name="City"` instead of `name="Address.City"`. The model binder cannot map unprefixed fields to nested properties on the parent ViewModel.

- Use `<partial name="_AddressEditor" for="Model.Address" />` or `Html.EditorFor(m => m.Address)` to maintain prefix.
- Manually set `ViewData.TemplateInfo.HtmlFieldPrefix = "Address"` before rendering nested partials.
- The `<partial for="...">` tag helper in ASP.NET Core sets prefix correctly for nested binding.
- Misbound nested objects silently remain null/default — a common checkout and profile-form bug.

---

#### Q9. What happens when optional nullable fields (`int?`, `DateTime?`) are left empty?

**Answer:** Empty text inputs for nullable value types bind to `null` — the intended signal for "no value provided." This distinguishes optional fields from explicit zero or minimum-date entries.

- `int? Quantity` with cleared input → `null`, not `0`.
- `DateTime? ShipDate` with cleared `<input type="date">` → `null`, not `DateTime.MinValue`.
- Non-nullable `int`/`DateTime` on optional fields bind empty input to `0`/`0001-01-01` or add conversion errors — use nullable types for optional form fields.
- Check `ModelState` for conversion failures on required non-nullable fields separately.

---

#### Q10. How does culture affect date and number binding from form fields?

**Answer:** Model binding parses date and number strings using `CultureInfo.CurrentCulture` unless overridden — so `31/01/2026` fails on an `en-US` server expecting `1/31/2026`. Users see "The value is not valid" errors despite typing correctly for their locale.

- `RequestLocalization` middleware sets culture from cookies, query string, or `Accept-Language`.
- Prefer `<input type="date">` posting ISO `yyyy-MM-dd` — culture-invariant for dates.
- Decimal separators differ (`1,5` vs `1.5`) — culture-aware apps must configure supported cultures explicitly.
- API JSON with ISO 8601 dates avoids culture issues; traditional MVC text boxes do not.

---

#### Q11. What is `RequestLocalization` and how does it relate to model binding?

**Answer:** `RequestLocalization` middleware establishes `CultureInfo.CurrentCulture` and `CurrentUICulture` per request from configured providers. Model binders and Data Annotations use these cultures to parse and format dates, numbers, and currency from form fields.

- Register in `Program.cs`: `app.UseRequestLocalization(options => { options.SupportedCultures = ...; })`.
- Providers include `QueryStringRequestCultureProvider`, `CookieRequestCultureProvider`, and `AcceptLanguageHeaderRequestCultureProvider`.
- Without it, all users inherit the server's default culture — often `en-US` in cloud deployments.
- Display formatting in views and parse binding share the same culture when localization is configured correctly.

---

#### Q12. How does file upload binding work (`IFormFile`)?

**Answer:** `IFormFile` properties on ViewModels bind from file input fields when the form uses `enctype="multipart/form-data"`. The binder captures filename, content type, and stream for uploaded files alongside other form fields in the same POST.

- Use `<input asp-for="UploadedFile" type="file" />` inside a multipart form.
- Multiple files use `IFormFileCollection` or `List<IFormFile>`.
- Validate size (`[RequestSizeLimit]`, `MultipartBodyLengthLimit`), extension whitelist, and scan in production.
- `IFormFile` is null when no file is selected or when enctype is wrong — always null-check before processing.

---

#### Q13. What `enctype` is required for file upload forms?

**Answer:** Forms that include file inputs must set `enctype="multipart/form-data"`. The default `application/x-www-form-urlencoded` cannot carry binary file content — `IFormFile` binds as null even when other text fields bind correctly.

- Tag helper: `<form asp-action="Upload" method="post" enctype="multipart/form-data">`.
- Both metadata fields and file inputs can coexist in one multipart POST.
- Large uploads may need `RequestSizeLimit` or Kestrel `MaxRequestBodySize` configuration.
- Missing enctype is the most common cause of "file upload always null" bugs.

---

#### Q14. What is over-posting during model binding and how is it prevented?

**Answer:** Over-posting is when attackers POST values for properties not shown in the UI — binding sets every matching key on the parameter type. Prevention requires binding to a ViewModel with only allowed properties and mapping explicitly to entities on the server.

- `[Bind(Include = "Name,Email")]` on entities is a partial fix — ViewModels are preferred.
- Never bind POST directly to EF entities with sensitive columns (`IsAdmin`, `Role`, internal pricing).
- `[BindNever]` on specific properties helps but does not replace input model whitelisting.
- Integration-test POSTs with extra fields to verify privileged properties remain unchanged.

---

#### Q15. What are `[BindNever]` and `[Bind]` used for?

**Answer:** `[BindNever]` excludes a property from model binding — inbound POST values are ignored for that property. `[Bind(Prefix = "", Include = "A,B,C")]` or `[Bind(Exclude = "...")]` restricts binding to an explicit allow or deny list on the parameter type.

- `[BindNever]` on navigations and read-only collections prevents graph binding on entities.
- `[Bind(Include = "...")]` on action parameters whitelists bindable members — weaker than dedicated ViewModels.
- `[BindNever]` on ViewModel properties protects server-populated dropdown lists from tampering.
- Security-sensitive POST paths should use ViewModels, not broad `[Bind]` on entities.

---

#### Q16. How do checkboxes bind to `bool` and `bool?` properties?

**Answer:** HTML checkboxes submit their value when checked and **omit the key entirely** when unchecked. For non-nullable `bool`, ASP.NET Core treats a missing key as `false`. Tag helpers for `bool` add a hidden `false` input so unchecked explicitly posts `false`.

- `bool AcceptedTerms`: standard helper posts hidden `false` + checkbox `true` — unchecked yields `false`.
- `bool? Newsletter`: tri-state (`null` = unchanged) breaks when the hidden-false pattern forces `false` instead of `null`.
- For tri-state nullable booleans, use manual markup without the hidden companion or an enum (`Unchanged`, `OptIn`, `OptOut`).
- `[Required]` on non-nullable `bool` checkboxes fails to work as expected — use `[Range(typeof(bool), "true", "true")]` for required consent.

---

#### Q17. What is the hidden-field pattern for checkboxes?

**Answer:** The checkbox tag helper renders a hidden input with `false` before the checkbox input with `true` — both share the same name. Unchecked forms submit only the hidden `false`; checked forms submit both (the checkbox value overrides), ensuring non-nullable `bool` properties always receive an explicit true or false.

- Without the hidden field, unchecked checkboxes would not appear in the POST at all — fine for `bool?` tri-state, wrong for required `bool`.
- Pattern: `<input type="hidden" name="IsActive" value="false" /><input type="checkbox" name="IsActive" value="true" />`.
- Do not apply this pattern to `bool?` when `null` means "no change" — it forces `false` on unchecked.
- Required legal consent checkboxes rely on this pattern plus server-side validation for `true`.

---

#### Q18. What is a custom `IModelBinder` and when would you create one?

**Answer:** A custom `IModelBinder` implements `BindModelAsync` to convert non-standard request data into a model property or parameter type that built-in binders cannot handle. Register it via `IModelBinderProvider` for types like custom value objects, composite keys, or culture-specific currency parsing.

- Implement `IModelBinder` with **stateless** logic — no instance fields storing request data across binds.
- Register through `ModelBinderProvider` in `AddControllersWithViews(options => options.ModelBinderProviders.Insert(0, new MyProvider()))`.
- Use cases: binding `Lines[SKU-A].Qty` dictionaries, parsing `"$1,234.56"` currency strings, combining split fields into one type.
- Avoid singleton registration of stateful binders — concurrent requests corrupt shared fields under load.

---

---

> **Target:** ASP.NET Core 8 MVC

---

## Chapter 07. Data Annotations & Validation

#### Q1. What are Data Annotations in ASP.NET Core MVC?

**Answer:** Data Annotations are declarative attributes (from `System.ComponentModel.DataAnnotations`) placed on ViewModel or DTO properties to express validation rules, display metadata, and binding hints. MVC reads them during model binding and validation to populate `ModelState` and to emit client-side validation attributes in Razor views.

- Common validation attributes include `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Compare]`, and `[RegularExpression]`.
- Display attributes such as `[Display(Name = "...")]` and `[DataType(DataType.Password)]` shape labels and HTML input types in Tag Helpers.
- Annotations on the posted model are evaluated server-side by the validation pipeline when the action runs — they do not replace an explicit `ModelState.IsValid` check in MVC controllers.
- Place annotations on ViewModels rather than EF entities when the same entity is shared across persistence and UI layers.

---

#### Q2. What is `ModelState` and how does it relate to validation?

**Answer:** `ModelState` is a dictionary on the controller (via `ControllerBase.ModelState`) that records binding errors and validation failures for each model property. After model binding, the validation system adds entries keyed by property name; the action uses it to decide whether the submitted data is acceptable.

- Each key (e.g., `"Email"`) holds a `ModelStateEntry` with attempted value, validation errors, and binding exceptions.
- Validation attributes, `IValidatableObject`, and custom `ValidationAttribute` subclasses all write into `ModelState` through the same pipeline.
- Tag Helpers such as `asp-validation-for` read `ModelState` to render field-level error messages in the view.
- `ModelState` is scoped to the current HTTP request and is not preserved across `RedirectToAction` unless you explicitly rehydrate it.

---

#### Q3. Why must server-side validation always be performed even with client validation?

**Answer:** Client-side validation runs only in the browser and can be disabled, bypassed, or forged with tools like curl or Postman. Server-side validation is the authoritative gate before persisting data or performing security-sensitive operations.

- Attackers can POST directly to the action without JavaScript or with modified `data-val-*` attributes removed from the HTML.
- Client validation improves UX with immediate feedback but provides zero security guarantees on its own.
- MVC controllers do not automatically return 400 on invalid models unless you check `ModelState.IsValid` (unlike `[ApiController]` automatic validation).
- Production code must validate on the server even when unobtrusive jQuery validation is enabled in the layout.

---

#### Q4. What does `ModelState.IsValid` check?

**Answer:** `ModelState.IsValid` returns `true` only when every entry in `ModelState` has no validation or binding errors. It is the standard guard in POST actions before calling services or saving to the database.

- Binding failures (type conversion, missing required value types) mark entries invalid before annotation validation runs.
- Cross-property rules from `IValidatableObject.Validate` also contribute errors that make `IsValid` false.
- Checking `IsValid` alone does not run validation — validation runs during model binding; `IsValid` only reports the outcome.
- On failure, return `View(model)` (or `PartialView`) with the same model so field names and error keys align with the form.

---

#### Q5. What is the difference between `[Required]` and `[AllowNull]`?

**Answer:** `[Required]` means the value must be present and non-empty for reference types and non-nullable value types; for nullable value types it means the value must not be `null`. `[AllowNull]` (from `System.Diagnostics.CodeAnalysis`) is a nullable reference type annotation telling the compiler the property may be assigned `null` — it does not enforce runtime validation.

- `[Required]` on `string` fails for `null` or `""` (and whitespace-only by default with data annotations).
- `[Required]` on `int?` fails when the field is omitted and binds as `null`; on non-nullable `int`, a missing field may bind as `0`, which satisfies `[Required]`.
- `[AllowNull]` affects static analysis (NRT) and documentation, not MVC validation behavior.
- Use `[Required(AllowEmptyStrings = true)]` when an empty string is acceptable but `null` is not.

---

#### Q6. Why does `[Required]` not work as expected on a `bool` checkbox?

**Answer:** A non-nullable `bool` checkbox that is unchecked does not post a value, so model binding sets the property to `false` — a valid non-null value. `[Required]` checks for presence, not for `true`, so an unchecked required consent checkbox never fails validation.

- HTML checkboxes only submit `"on"` or `"true"` when checked; absent fields bind as `false` for `bool`.
- `[Required]` on `bool?` fails only when the value is `null`, not when it is `false`.
- For "must accept terms" scenarios, validate explicitly that the value is `true` via `[Range(typeof(bool), "true", "true")]`, a custom attribute, or `IValidatableObject`.
- The hidden-field checkbox pattern (`<input type="hidden" value="false" />` plus checkbox) still posts `false` when unchecked, not `null`.

---

#### Q7. What is `[Compare]` used for?

**Answer:** `[Compare("OtherProperty")]` validates that the decorated property's value equals another property on the same model instance — commonly used for `ConfirmPassword` matching `Password`.

- Comparison runs server-side during validation after both properties are bound.
- The client-side unobtrusive adapter emits `data-val-equalto-other` so browsers can show a match error before submit.
- Property names in `[Compare]` must match exactly; typos silently weaken validation.
- `[Compare]` compares string representation; for complex types use custom validation instead.

---

#### Q8. What are `[Range]` and `[StringLength]` used for?

**Answer:** `[StringLength(maximumLength)]` constrains the length of a string property (with optional minimum). `[Range(min, max)]` constrains numeric or date values to an inclusive bounds window defined by constants or type-specific overloads.

- `[StringLength(100, MinimumLength = 3)]` validates character count, not byte size — important for Unicode text.
- `[Range(1, 100)]` on `int` rejects values outside the interval; use `[Range(typeof(decimal), "0.01", "9999.99")]` for decimal bounds.
- Both attributes generate corresponding `data-val-length` and `data-val-range` attributes for unobtrusive client validation.
- Error messages can be customized via `ErrorMessage` or resource-based `ErrorMessageResourceName`.

---

#### Q9. What is `[RegularExpression]` used for?

**Answer:** `[RegularExpression("pattern")]` validates that a string property matches a .NET regular expression, useful for phone numbers, postal codes, or usernames with specific formats.

- The pattern uses .NET regex syntax, which may differ slightly from JavaScript regex used on the client.
- Unobtrusive validation maps the pattern to `data-val-regex-pattern` for jQuery Validate.
- Overly complex patterns can be hard to maintain; consider `IValidatableObject` or FluentValidation for multi-field format rules.
- Always re-validate on the server — client regex is advisory only.

---

#### Q10. What is `IValidatableObject` and when do you use it?

**Answer:** `IValidatableObject` is an interface with a `Validate(ValidationContext)` method that returns `IEnumerable<ValidationResult>` for cross-property or conditional rules that single attributes cannot express.

- Implement it on the ViewModel when validation depends on multiple fields (e.g., end date after start date).
- Results can target a specific member name or be model-level with an empty member name for summary display.
- It runs as part of the same MVC validation pass after property-level attributes are evaluated.
- Use it when logic is simple and colocated with the model; move elaborate rule sets to FluentValidation for maintainability.

---

#### Q11. What is the difference between `IValidatableObject` and a custom `ValidationAttribute`?

**Answer:** A custom `ValidationAttribute` decorates individual properties (or the class with `ValidationAttribute` usage on the type) and is reusable across models. `IValidatableObject` centralizes all object-level rules in one `Validate` method on that type.

- Attributes compose declaratively on properties; custom attributes can be reused with different parameters.
- `IValidatableObject` fits cross-field rules without creating many one-off attributes.
- Custom attributes can provide client-side adapters; `IValidatableObject` is server-only unless you add a separate client adapter.
- Both integrate with `ModelState`; choose based on reusability and whether the rule is property-scoped or object-scoped.

---

#### Q12. What is `[Remote]` validation and how does it work?

**Answer:** `[Remote(action, controller, area)]` triggers an AJAX call during client-side validation to ask the server whether a value is acceptable — typical for checking username or email availability before form submit.

- The attribute generates `data-val-remote-url` pointing at a controller action that returns `true`/`false` or JSON indicating validity.
- The remote action must be idempotent, fast, and secured — it is callable without submitting the full form.
- Server-side validation must still enforce the same rule on POST; `[Remote]` is not invoked automatically during server validation.
- Include antiforgery or rate limiting on remote endpoints to reduce abuse in production.

---

#### Q13. What is `[ValidateNever]` and when is it applied?

**Answer:** `[ValidateNever]` (from `Microsoft.AspNetCore.Mvc.ModelBinding.Validation`) excludes a property from validation even if it has data annotation attributes or would otherwise be validated. It is commonly applied to properties populated server-side that should not be validated from user input.

- Use on navigation properties, audit fields, or server-assigned IDs on ViewModels bound from forms.
- It prevents spurious validation errors on properties the user did not post.
- Pair with `[BindNever]` when the property should neither bind nor validate from the request.
- Do not use it to skip validation on user-editable fields — that creates security holes.

---

#### Q14. Why should validation attributes not be placed on EF entities shared with MVC?

**Answer:** EF Core entities represent the persistence model; UI validation rules, display names, and `[Required]` semantics often differ from database constraints. Sharing annotations couples the database schema to presentation concerns and causes incorrect validation on API or batch import paths.

- A column may be required in the database but optional on a create form step; a ViewModel expresses that per screen.
- Navigation properties on entities can trigger unintended validation of related graphs during MVC binding.
- API DTOs and MVC ViewModels need different rules than the entity; annotations on the entity force one-size-fits-all behavior.
- Keep EF configuration in Fluent API or data annotations limited to persistence; put MVC validation on ViewModels.

---

#### Q15. What are `asp-validation-for` and `asp-validation-summary`?

**Answer:** These are Tag Helpers that render validation UI from `ModelState`. `asp-validation-for="PropertyName"` outputs a `<span>` with the field error message; `asp-validation-summary` renders a summary block of all errors or model-level errors only.

- `asp-validation-for` targets a single property and pairs with inputs generated by `asp-for` on the same property name.
- `asp-validation-summary` accepts `ValidationSummary.All` or `ValidationSummary.ModelOnly` to control which errors appear.
- They emit no content when `ModelState` has no errors for the requested scope.
- Require `_ValidationScriptsPartial` (jQuery Validate + unobtrusive) in the layout for client-side messages to appear before POST.

---

#### Q16. What is the difference between `ValidationSummary` `ModelOnly` and `All`?

**Answer:** `ValidationSummary.All` lists every error in `ModelState`, including property-level errors that may also appear next to fields. `ValidationSummary.ModelOnly` shows only errors not tied to a specific property (empty member name) or excludes property-level keys depending on configuration — typically used for a top-of-form alert without duplicating field messages.

- Use `ModelOnly` when each field already has `asp-validation-for` and the summary should show cross-cutting errors only.
- Use `All` when you want a single consolidated error list, often on small forms without per-field spans.
- Duplicate display occurs if you use `All` alongside `asp-validation-for` for the same properties.
- Model-level errors from `ModelState.AddModelError(string.Empty, message)` appear in `ModelOnly` summaries.

---

#### Q17. What is unobtrusive client validation?

**Answer:** Unobtrusive client validation is ASP.NET Core MVC's approach where Tag Helpers emit `data-val-*` HTML attributes from server-side metadata, and jQuery Validate Unobtrusive reads those attributes without embedding inline JavaScript in the view.

- Enabled by referencing `jquery.validate.js` and `jquery.validate.unobtrusive.js` after jQuery.
- Validation rules mirror data annotations where adapters exist; the browser blocks submit and shows messages before a round trip.
- It respects `ClientValidationEnabled` on the view context and can be disabled per request if needed.
- Server-side validation must always duplicate these rules for security regardless of unobtrusive setup.

---

#### Q18. What is the difference between Data Annotations and FluentValidation?

**Answer:** Data Annotations are attributes on properties evaluated by the built-in validation infrastructure. FluentValidation defines rule classes (`AbstractValidator<T>`) with a fluent API, registered in DI and invoked by the FluentValidation.AspNetCore integration package.

- Annotations are lightweight and visible on the ViewModel; FluentValidation centralizes complex rules in separate validator classes.
- FluentValidation excels at conditional chains, async rules, and large rule sets without polluting the model type.
- Both populate `ModelState` when integrated; FluentValidation requires explicit registration (`AddFluentValidation`, `AddValidatorsFromAssembly`).
- Annotations automatically drive unobtrusive client adapters; FluentValidation needs additional setup for client-side parity.

---

## Chapter 08. Tag Helpers

#### Q1. What are Tag Helpers in ASP.NET Core MVC?

**Answer:** Tag Helpers are server-side components that participate in Razor view rendering by transforming HTML-like elements and attributes into valid, route-aware, model-bound markup. They replace many HTML Helpers with a more natural HTML-centric syntax in `.cshtml` files.

- They run at view compilation/render time and can add, remove, or modify tags and attributes based on `ViewContext` and model metadata.
- Built-in helpers live in `Microsoft.AspNetCore.Mvc.TagHelpers` and cover forms, links, scripts, environments, and partials.
- Tag Helpers are opt-in per element via `asp-*` attributes or target elements registered with `[HtmlTargetElement]`.
- ASP.NET Core 8 MVC continues Tag Helpers as the default approach in project templates alongside fully supported HTML Helpers.

---

#### Q2. What is the difference between Tag Helpers and HTML Helpers?

**Answer:** HTML Helpers are C# methods on `IHtmlHelper` (e.g., `@Html.TextBoxFor`) that return `IHtmlContent` strings. Tag Helpers are classes that process HTML elements in Razor, keeping markup readable and closer to designer-friendly HTML.

- Tag Helpers understand `asp-*` attributes; HTML Helpers use method parameters and anonymous objects for HTML attributes.
- Both generate similar output for forms and validation, but Tag Helpers integrate more cleanly with IntelliSense in HTML elements.
- HTML Helpers remain supported and are preferable for dynamic HTML built in code or heavily customized editor templates.
- Tag Helpers participate in a ordered pipeline and can wrap child content; HTML Helpers are single method calls.

---

#### Q3. What does `asp-for` do on an input element?

**Answer:** `asp-for="PropertyName"` binds the input to a model property expression, generating correct `name`, `id`, and `value` attributes for model binding and labeling. It also applies validation and display metadata from data annotations.

- For `EditViewModel.Title`, it emits `name="Title"`, `id="Title"`, and the current value for POST round trips.
- It adds `data-val-*` attributes when client validation is enabled and the property has validation metadata.
- Expression trees support nested paths (`asp-for="Address.City"`) and respect HTML field prefixes in partial views.
- The `@model` type must match the property expression; mismatched model types break validation key alignment.

---

#### Q4. What do `asp-action` and `asp-controller` do on a form or anchor?

**Answer:** These attributes tell the Form or Anchor Tag Helper which controller action URL to generate using the application's route table and ambient route values. They produce correct `action` or `href` URLs without hardcoding paths.

- Omitted values default to the current controller or action from route data (ambient values).
- Combined with `asp-route-id` or other `asp-route-*` attributes, they append route parameters for link generation.
- Form Tag Helper also sets HTTP method via `method="post"` and injects antiforgery tokens for POST forms.
- For areas, `asp-area` must be set explicitly when linking across area boundaries.

---

#### Q5. What is `asp-validation-for`?

**Answer:** `asp-validation-for` is a Tag Helper that renders a `<span class="field-validation-valid">` (or error class) displaying the first validation error for the specified model property from `ModelState`.

- It uses the same expression as `asp-for` so the `ModelState` key matches the posted field name.
- On validation failure after POST, the span shows the server error message from annotations or custom validation.
- With unobtrusive scripts loaded, jQuery Validate also populates this span on the client before submit.
- Place it adjacent to the corresponding input for accessible error association.

---

#### Q6. What is `asp-validation-summary`?

**Answer:** `asp-validation-summary` renders a `<div>` listing validation errors from `ModelState` according to the selected summary mode (`All` or `ModelOnly`).

- It is typically placed at the top of a form to show model-level or consolidated errors.
- The helper adds `validation-summary-errors` CSS class when errors exist.
- Works with both server-returned errors and client-side unobtrusive validation blocking submit.
- Avoid duplicating property errors in the summary when per-field `asp-validation-for` spans are already present unless using `ModelOnly`.

---

#### Q7. How do Tag Helpers generate antiforgery tokens for forms?

**Answer:** The Form Tag Helper automatically injects a hidden `<input>` with the antiforgery token when the form uses `method="post"`. No manual token markup is required when using `<form asp-action="..." method="post">`.

- The token pairs with `[ValidateAntiForgeryToken]` or global `[AutoValidateAntiforgeryToken]` on the target action.
- GET forms do not receive tokens because GET should be idempotent and not mutate state.
- AJAX requests must manually send the token via header `RequestVerificationToken` or form field when not using the Form Tag Helper.
- Token generation uses `IAntiforgery` services configured in `AddControllersWithViews`.

---

#### Q8. What is `asp-route-*` used for?

**Answer:** `asp-route-{parameterName}` supplies route values when generating URLs for anchors and forms, where `{parameterName}` matches a route template token or action parameter name.

- Example: `asp-route-id="@Model.ProductId"` on `<a asp-action="Details">` produces `/Products/Details/5`.
- Multiple `asp-route-*` attributes map to named segments in attribute routes or conventional routes.
- Values merge with ambient route data from the current request unless overridden.
- Unknown route values may become query string parameters depending on the route template.

---

#### Q9. What does `asp-append-version` do?

**Answer:** `asp-append-version="true"` on `<script>` or `<link>` Tag Helpers appends a cache-busting query string (file hash) to static file URLs so browsers fetch new versions after deployment.

- Works with files served through static files middleware from `wwwroot`.
- Prevents stale JavaScript or CSS after releases without manual version query strings.
- The hash changes when file content changes; unchanged files keep stable URLs for caching.
- Commonly used in layout files for bundled app scripts and styles.

---

#### Q10. What is the `<environment>` tag helper used for?

**Answer:** The `<environment>` Tag Helper conditionally renders its child content based on the hosting environment name (`Development`, `Staging`, `Production`, or custom names from `IWebHostEnvironment`).

- Use `include="Development"` to load unminified scripts or Browser Link only during local development.
- Use `exclude="Development"` to load CDN production assets with fallback tags in non-development environments.
- Multiple environment names can be comma-separated in `include` or `exclude`.
- It avoids runtime `if (env.IsDevelopment())` blocks cluttering layout markup.

---

#### Q11. How are Tag Helpers registered in `_ViewImports.cshtml`?

**Answer:** `_ViewImports.cshtml` at the `Views` folder (and Area views) registers Tag Helpers for all views in that folder tree using `@addTagHelper` directives.

- The standard line `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` enables all built-in MVC Tag Helpers.
- Custom Tag Helpers register with `@addTagHelper *, YourAssemblyName`.
- Child folders inherit imports from parent `_ViewImports.cshtml` files unless overridden.
- Registration scope means helpers apply to every view under that path without repeating directives.

---

#### Q12. What are `@addTagHelper` and `@removeTagHelper`?

**Answer:** `@addTagHelper` imports Tag Helper types or assemblies into the Razor compilation scope for the current folder and descendants. `@removeTagHelper` excludes specific helpers where global registration would conflict with legacy markup.

- Syntax: `@addTagHelper [namespace.]TagHelperName, AssemblyName` or wildcard `*` for all helpers in an assembly.
- `@removeTagHelper` is useful in a subfolder's `_ViewImports` when third-party HTML must not be rewritten by a broad helper target.
- Removal is scoped to the folder tree where the directive appears.
- Order matters only insofar as remove directives must reference helpers already added by parent imports.

---

#### Q13. What is the difference between `<partial>` and `Html.PartialAsync` as a tag helper?

**Answer:** Both render a partial view, but `<partial name="_Name" model="..." />` is declarative HTML-like syntax while `@await Html.PartialAsync("_Name", model)` is an explicit helper call returning `IHtmlContent`.

- The `<partial>` Tag Helper resolves the partial by name with the same view location conventions as `PartialAsync`.
- Tag Helper syntax avoids `@await` in the middle of HTML blocks and reads cleaner in designer-friendly markup.
- Both execute the partial asynchronously and do not run a controller action — they render a view fragment only.
- Pass a strongly typed model to either approach; avoid `ViewBag` for partial data when type safety matters.

---

#### Q14. What is the `!` prefix (opt-out) on Tag Helpers?

**Answer:** Prefixing an attribute with `!` (e.g., `<input !name="custom" />`) opts that element out of Tag Helper processing so the Razor engine leaves the attribute and tag unchanged.

- Useful when a literal HTML attribute would otherwise match a Tag Helper target and be rewritten unexpectedly.
- Alternative opt-out: omit all `asp-*` attributes on elements that should not be processed, or use `@removeTagHelper` for broader suppression.
- Common during migration from HTML Helpers when mixing literal markup with Tag Helper-enabled views.
- The opt-out applies per attribute, not globally to the entire element unless no `asp-*` triggers remain.

---

#### Q15. How does a custom Tag Helper work (`TagHelper` base class)?

**Answer:** A custom Tag Helper inherits `TagHelper`, is decorated with `[HtmlTargetElement(...)]` to declare which tags and attributes it targets, and overrides `Process` or `ProcessAsync` to modify `TagHelperOutput`.

- `TagHelperContext` provides element name and attribute values; `TagHelperOutput` is the mutable result tag.
- Set `output.TagName`, `Attributes`, `Content`, or `TagMode` to transform or suppress the element.
- Register the helper assembly in `_ViewImports.cshtml` with `@addTagHelper`.
- Use `Order` property to run before or after built-in helpers when sharing the same target element.

---

#### Q16. What is Tag Helper processing order and why does it matter?

**Answer:** When multiple Tag Helpers target the same element, they run in ascending `Order` value (lower numbers first). Built-in MVC Tag Helpers use negative orders so they typically execute before custom helpers at default order `0`.

- Custom helpers that modify attributes set by built-ins should set a higher `Order` (e.g., `1000`) to run after `InputTagHelper` finishes.
- Order applies among helpers on the **same element**, not parent-child elements in the DOM.
- Parent wrappers that call `GetChildContentAsync()` still let child element helpers run during child content rendering first.
- Incorrect order can strip or overwrite `name`, `id`, or `data-val-*` attributes generated by built-ins.

---

#### Q17. How do you register Tag Helpers from a Razor Class Library?

**Answer:** Pack Tag Helper classes in a Razor Class Library (RCL), reference the RCL from the MVC app, and add `@addTagHelper *, YourRclAssembly` in `_ViewImports.cshtml` (root and Areas as needed).

- RCLs can ship shared partials, Tag Helpers, and static assets consumed by multiple MVC applications.
- Tag Helpers in RCLs follow the same `[HtmlTargetElement]` and `TagHelper` base class patterns as app-local helpers.
- Views embedded in the RCL resolve relative to the library's virtual path conventions.
- Version the RCL package so consuming apps pick up helper changes consistently across solutions.

---

#### Q18. What HTML attributes do Tag Helpers emit for client-side validation (`data-val-*`)?

**Answer:** Input Tag Helpers emit unobtrusive validation attributes derived from model metadata, including `data-val="true"`, `data-val-required`, `data-val-length`, `data-val-range`, `data-val-regex`, `data-val-equalto`, and corresponding `data-val-*-msg` message attributes.

- `data-val-required` appears when `[Required]` applies; message in `data-val-required` or shared `data-valmsg-for` spans.
- `[StringLength]` maps to `data-val-length-max` / `data-val-length-min`; `[Range]` to `data-val-range-max` / `data-val-range-min`.
- `[Compare]` emits `data-val-equalto-other` referencing the paired property name.
- jQuery Validate Unobtrusive parses these attributes at runtime; hand-written `<input>` without `asp-for` lacks them unless added manually.

---

## Chapter 09. Routing & Attribute Routing

#### Q1. What is conventional routing in ASP.NET Core MVC?

**Answer:** Conventional routing maps URLs to controllers and actions using route templates registered in `Program.cs` with `MapControllerRoute`, typically `{controller=Home}/{action=Index}/{id?}`. The dispatcher extracts segment values and invokes the matching action method.

- Route names and patterns are centralized in endpoint configuration rather than on controller classes.
- Defaults supply `Home` and `Index` when segments are omitted, making `/` resolve to `HomeController.Index`.
- Multiple named routes can coexist; the endpoint routing middleware selects the first matching template by registration order and specificity.
- Conventional routing remains the default in MVC templates alongside optional attribute routes on controllers.

---

#### Q2. What is the default route pattern `{controller=Home}/{action=Index}/{id?}`?

**Answer:** This template maps the first URL segment to a controller name (without the `Controller` suffix), the second to an action method name, and an optional third `{id}` segment to an action parameter named `id`.

- `/` uses defaults → `HomeController.Index()` with `id = null`.
- `/Products/Details/42` → `ProductsController.Details(42)` when such an action exists.
- Matching is case-insensitive by default for controller and action names in ASP.NET Core 8.
- The optional `{id?}` token allows actions with or without an `id` parameter on the same template.

---

#### Q3. What is attribute routing in MVC controllers?

**Answer:** Attribute routing decorates controllers and actions with `[Route]`, `[HttpGet]`, `[HttpPost]`, and related attributes to define URL templates directly on the types that handle them. `MapControllers()` in `Program.cs` discovers these routes at startup.

- Controller-level `[Route("api/[controller]")]` prefixes all actions in that controller.
- Action-level `[HttpGet("{id:int}")]` adds HTTP method constraints and parameter tokens.
- Attribute routes can coexist with conventional routes in the same application.
- Explicit templates are preferred for REST-style APIs and marketing-friendly fixed URLs within MVC apps.

---

#### Q4. What is the difference between conventional routing and attribute routing?

**Answer:** Conventional routing defines URL patterns centrally in `Program.cs` and infers controller/action from segments. Attribute routing colocates URL templates on controller classes, making each action's public URL explicit in code.

- Conventional routing suits traditional `{controller}/{action}` sites with consistent URL shapes.
- Attribute routing suits fine-grained control, versioning prefixes, and literal segments that would collide with conventional defaults.
- Link generation with Tag Helpers works with both; attribute routes require matching template tokens in `asp-route-*` values.
- Mixing both requires careful ordering — attribute routes and conventional routes compete in the same endpoint route table.

---

#### Q5. What are route constraints (e.g., `:int`, `:exists`)?

**Answer:** Route constraints restrict which values a route parameter accepts. `:int` requires an integer; `:guid`, `:alpha`, `:minlength(n)`, and custom `IRouteConstraint` implementations filter matches before an action is selected.

- `{id:int}` rejects `orders/abc` so the route does not match and another route may handle the request.
- `{area:exists}` ensures the area segment matches a registered area name in the application.
- Constraints participate in URL generation as well as matching — invalid values may prevent link creation.
- Failed constraint match yields "no route matched" (404), not an action invocation with invalid data.

---

#### Q6. Why does route registration order matter in `MapControllerRoute`?

**Answer:** Endpoint routing evaluates registered routes in order; the first route template that matches the request wins. More specific routes must be registered before general catch-all patterns like the default `{controller}/{action}/{id?}`.

- A slug route `products/{id}` registered before the default route captures `/products/sale` as `id = "sale"` instead of reaching a `Sale` action via conventional segments.
- Named routes do not affect matching priority — registration sequence in `Program.cs` does.
- Attribute routes from `MapControllers()` are merged into the same endpoint collection with their own order metadata.
- Integration tests should cover ambiguous URLs when adding new conventional routes.

---

#### Q7. How does the `{area:exists}` constraint work?

**Answer:** The `exists` constraint on an `area` route parameter verifies that the captured area name corresponds to a known MVC area registered in the application (controllers decorated with `[Area("Name")]`). If the area does not exist, the route fails to match.

- Used in the standard areas template: `{area:exists}/{controller=Home}/{action=Index}/{id?}`.
- Prevents arbitrary first segments from being interpreted as areas when they are not defined.
- Area discovery relies on `[Area]` attributes — folder placement alone is insufficient.
- Link generation with `asp-area` must use a registered area name or URL generation fails or omits the segment incorrectly.

---

#### Q8. What is the difference between `[Route]` on a controller vs on an action?

**Answer:** Controller-level `[Route("prefix")]` establishes a shared URL prefix for all actions in that controller. Action-level `[Route("segment")]` or verb attributes append or override segments relative to the controller prefix.

- `[Route("[controller]")]` on the controller plus `[HttpGet("{id}")]` on an action yields `/Products/{id}` for `ProductsController`.
- Action-only `[HttpGet("/absolute")]` with a leading slash ignores controller prefix and maps from site root.
- HTTP method attributes (`[HttpGet]`, `[HttpPost]`) are a form of attribute routing with method constraints.
- Action templates combine with controller templates unless the action template is root-absolute (starts with `/`).

---

#### Q9. What does a leading slash in `[HttpGet("/export/{year}")]` mean?

**Answer:** A route template starting with `/` is absolute from the application root and does not combine with any controller-level route prefix. `/export/{year}` maps to that path regardless of a `[Route("api")]` on the controller.

- Without the leading slash, `export/{year}` would append to the controller prefix (e.g., `/api/export/2024`).
- Absolute routes are useful for fixed marketing or report URLs independent of controller naming conventions.
- Root-absolute templates ignore `[Route]` prefixes on the controller class.
- Link generation must use matching absolute templates or appropriate `asp-route-*` values.

---

#### Q10. What is a catch-all route parameter (`{*slug}`)?

**Answer:** A catch-all parameter uses the `*` prefix (e.g., `{*slug}`) and greedily captures the remainder of the URL path including slashes, typically as a single string segment for file paths or CMS content.

- `/docs/{*slug}` matches `/docs/a/b/c` with `slug = "a/b/c"`.
- Catch-all parameters must usually be the last segment in the template.
- Useful for serving nested virtual paths from one action (documentation browsers, wildcard CMS pages).
- Route constraints on catch-all parameters are limited compared to single-segment parameters.

---

#### Q11. How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action selection?

**Answer:** HTTP method attributes constrain which requests can select an action during endpoint matching. `[HttpGet]` matches GET requests; `[HttpPost]` matches POST; unsupported verbs on a matched route may return 405 Method Not Allowed.

- An action without a verb attribute may accept any method unless restricted by convention or `[ActionName]`.
- Multiple actions on the same route template must differ by HTTP method to disambiguate.
- `[AcceptVerbs("GET", "HEAD")]` allows listing multiple verbs explicitly.
- Method constraints apply to attribute-routed endpoints and to actions reached through conventional routing when verb attributes are present.

---

#### Q12. What happens when two actions match the same route?

**Answer:** ASP.NET Core throws `AmbiguousActionException` at runtime (or fails startup discovery in some cases) when multiple actions are equally eligible for the same HTTP method and route template. The framework requires unambiguous endpoint selection.

- Differentiate with distinct route templates, HTTP methods, or route constraints.
- `[HttpPost]` vs `[HttpGet]` on the same path template is valid; two `[HttpGet]` actions on the same template is not.
- Action naming alone does not disambiguate if both match the incoming URL equally.
- Refactor colliding actions or add literal segments and constraints to restore single match.

---

#### Q13. How does Tag Helper link generation (`asp-controller`, `asp-action`) relate to routing?

**Answer:** Anchor and Form Tag Helpers call into URL generation (`IUrlHelper`) using registered endpoint data, ambient route values, and supplied `asp-route-*` attributes to produce URLs that match the route table.

- Generated links reflect attribute routes and conventional routes without hardcoding paths.
- Ambient values from the current request fill omitted `asp-controller` / `asp-action` parameters.
- Incorrect or missing route values produce URLs that 404 when clicked because no endpoint matches.
- `asp-protocol` and `asp-host` override scheme and host for absolute URL generation.

---

#### Q14. Why must `area` be specified when generating links to area controllers from outside the area?

**Answer:** URL generation uses ambient route values from the current request. A view outside an area has no ambient `area` value, so `asp-controller="Dashboard"` targets a root controller, not `Areas/Admin/Controllers/DashboardController`.

- Explicit `asp-area="Admin"` sets the area route value for correct path generation.
- From within an area, ambient `area` flows to links unless overridden with `asp-area=""` for root controllers.
- Missing `asp-area` is a common cause of 404 links from main site navigation to admin modules.
- Area route templates require the area segment in the generated URL.

---

#### Q15. What is `LowercaseUrls` and how does it affect link generation?

**Answer:** `LowercaseUrls` is an option on `RouteOptions` (configured via `services.Configure<RouteOptions>`) that normalizes generated URLs to lowercase for controller, action, and route parameter names in link output.

- `/Products/Details` becomes `/products/details` in generated links when enabled.
- Incoming request URL matching remains case-insensitive by default unless `LowercaseUrls` is paired with routing options that enforce case.
- Improves URL consistency for SEO and Linux-hosted deployments where path casing can matter behind proxies.
- Tag Helper and `Url.Action` output respect this setting automatically.

---

#### Q16. What is the difference between "no route matched" and "405 Method Not Allowed"?

**Answer:** No route matched (404) means no endpoint template fit the URL path. 405 Method Not Allowed means a route matched the path but no action accepts the request's HTTP method (e.g., POST to a GET-only action).

- Wrong URL shape or missing area segment → typically 404 Not Found.
- Correct URL with wrong verb (GET on `[HttpPost]` only action) → 405 with `Allow` header listing permitted methods when configured.
- Distinguishing them guides debugging: 404 fixes routing templates; 405 fixes HTTP method attributes or form method.
- Conventional routes without verb attributes may accept unintended methods if not guarded with `[HttpGet]` etc.

---

#### Q17. How do optional route parameters (`{id?}`) and defaults interact?

**Answer:** The `?` marks a route parameter as optional in the template. Inline defaults in the pattern (`{action=Index}`) or `defaults:` anonymous object in `MapControllerRoute` supply values when segments are omitted.

- `/catalog` with pattern `catalog/{action=List}/{id?}` yields `action = List`, `id = null`.
- Defaults in the route registration are not the same as C# optional parameters — they live in the route table.
- Optional parameters must appear after required segments in the template.
- Link generation omits optional segments when values equal defaults, producing shorter URLs when configured.

---

#### Q18. What is the areas route pattern and how does it differ from the default route?

**Answer:** The standard areas pattern is `{area:exists}/{controller=Home}/{action=Index}/{id?}`, registered with `MapControllerRoute` and requiring `[Area("AreaName")]` on area controllers. It inserts an area segment before controller and action, routing to `Areas/{AreaName}/Controllers` and `Areas/{AreaName}/Views`.

- Default route has no area segment: `{controller=Home}/{action=Index}/{id?}` for root controllers in `/Controllers`.
- Areas enable parallel controller names (e.g., `Admin/HomeController` vs root `HomeController`).
- Register the areas route before or with careful ordering relative to the default route so area URLs resolve correctly.
- Views and `_ViewStart` under `Areas/{AreaName}/Views` follow separate layout resolution from root `Views`.

---

> **Target:** ASP.NET Core 8 MVC

---

## Chapter 10. Areas

#### Q1. What are Areas in ASP.NET Core MVC?

**Answer:** Areas are a feature that partitions a single MVC application into logical sections with their own URL prefix, controllers, and views — for example `/Admin/Users` and `/Store/Products` — while sharing the same host, DI container, and domain services.

- Each area is identified by a route segment (`Admin`, `Store`) and maps to controllers under `Areas/{AreaName}/Controllers/`.
- Areas let one deployable app serve multiple UI surfaces (public site, admin portal, support desk) without duplicating `Program.cs` or database infrastructure.
- The view engine resolves Razor files from `Areas/{AreaName}/Views/{Controller}/{Action}.cshtml` when route data includes an `area` value.
- Areas are an organizational and routing feature, not a substitute for separate microservices or network isolation boundaries.

---

#### Q2. Why use Areas instead of controller name prefixes?

**Answer:** Areas provide first-class routing, view discovery, and link generation keyed by the `area` route value, whereas prefixing controller names (`AdminUsersController`) only changes type names and produces awkward URLs like `/AdminUsers/Index`.

- The standard area route pattern `{area:exists}/{controller}/{action}` produces clean, predictable URLs (`/Admin/Users/Index`) that product and QA teams can reason about.
- Tag Helpers and `RedirectToAction` accept `asp-area` and `{ area = "Admin" }` route values — prefix naming offers no equivalent metadata.
- Two areas can reuse the same controller name (`HomeController` in root and in Admin) because the `area` segment disambiguates them.
- Folder conventions (`Areas/Admin/Controllers`, `Areas/Admin/Views`) align with scaffolding, view location expanders, and IDE tooling.

---

#### Q3. What folder structure is required for an Area?

**Answer:** An area requires a root folder under `Areas/{AreaName}/` with at minimum a `Controllers/` subfolder for area controllers and a `Views/` subfolder mirroring the standard MVC view layout.

- Controllers live at `Areas/Admin/Controllers/UsersController.cs` — not directly under `Areas/Admin/`.
- Views follow `Areas/Admin/Views/{ControllerName}/{ActionName}.cshtml`, with optional `Areas/Admin/Views/Shared/` for area-specific layouts and partials.
- `Areas/Admin/Views/_ViewStart.cshtml` and `_ViewImports.cshtml` are strongly recommended for layout and import scoping within the area.
- The physical folder name and the string passed to `[Area("Admin")]` should match the area route segment used in URLs.

---

#### Q4. What is the `[Area("Admin")]` attribute and why is it required?

**Answer:** `[Area("Admin")]` is a class-level attribute on a controller that registers it with MVC's area routing and view discovery system, associating the controller with the `Admin` area route segment.

- Without it, a controller in `Areas/Admin/Controllers/` is not matched by the `{area:exists}` route template and typically returns 404.
- The attribute value becomes route data `area = "Admin"`, which the view engine uses to search under `Areas/Admin/Views/` instead of `/Views/`.
- The project compiles without the attribute — the failure appears only at runtime when a request hits the area URL.
- Link generation from outside the area requires explicit `asp-area="Admin"` because ambient area values are absent on root requests.

---

#### Q5. How is area routing registered in `Program.cs`?

**Answer:** Area routing is registered with `app.MapControllerRoute` (or `MapAreaControllerRoute`) after `app.MapControllers()` setup, typically defining a named route with the `{area:exists}` constraint before the default route.

- Call `app.MapControllerRoute` with pattern `"{area:exists}/{controller=Home}/{action=Index}/{id?}"` and name `"areas"`.
- Register the area route **before** the default `{controller}/{action}/{id?}` route so the first URL segment is interpreted as `area`, not `controller`.
- ASP.NET Core 8 supports the same endpoint routing infrastructure used by Minimal APIs — area routes are conventional MVC routes mapped at startup.
- Optional dedicated routes (e.g., `Admin/{controller=Dashboard}/{action=Index}` with `defaults: new { area = "Admin" }`) can shorten URLs for specific areas.

---

#### Q6. What is the standard areas route pattern?

**Answer:** The standard pattern is `{area:exists}/{controller=Home}/{action=Index}/{id?}`, where `{area:exists}` constrains the first segment to a registered area name and supplies defaults for controller and action when omitted.

- A request to `/Admin/Users/Edit/5` yields route values `{ area = "Admin", controller = "Users", action = "Edit", id = "5" }`.
- The `:exists` constraint ensures unknown first segments do not falsely match as areas and fall through to other routes.
- Defaults allow `/Admin` or `/Admin/Users` to resolve to `Home/Index` or `Users/Index` within the area when configured.
- The pattern mirrors the default site route but prepends the area segment as the primary namespace.

---

#### Q7. Why does route registration order matter for Areas?

**Answer:** MVC evaluates routes in registration order and uses the first match — if the default route is registered before the area route, `/Admin/Users` is parsed as `controller=Admin, action=Users` instead of `area=Admin, controller=Users`.

- The area route is more specific and must be registered first so `{area:exists}` consumes the leading segment correctly.
- Misordered routes cause intermittent bugs depending on which URL shape is tested (`/Users/Index` vs `/Admin/Users/Index`).
- Named routes do not change matching priority — only registration order and template specificity matter.
- Integration tests should assert `RouteData.Values["area"]` for area URLs to catch order regressions.

---

#### Q8. How do you generate links to area controllers using Tag Helpers?

**Answer:** Use `asp-area`, `asp-controller`, and `asp-action` on anchor and form tag helpers to emit URLs that include the area route segment.

- From a root view linking into Admin: `<a asp-area="Admin" asp-controller="Users" asp-action="Index">Users</a>` generates `/Admin/Users`.
- `RedirectToAction` requires the same route value: `RedirectToAction(nameof(Index), new { area = "Admin" })`.
- When already inside an area, ambient values may carry the current area — cross-area links still need an explicit `asp-area` for the target area.
- Omitting `asp-area` when linking from a root layout to an area controller produces root URLs (`/Users/Index`) that miss the area prefix.

---

#### Q9. What happens when `asp-controller` is used without `asp-area` from within an Area view?

**Answer:** Tag Helpers inherit ambient route values from the current request, so `asp-controller="Users"` from an Admin view typically generates `/Admin/Users/Index` using the current area context.

- Ambient area values flow from the executing request's route data — links within the same area often work without explicit `asp-area`.
- Linking to a **root** controller from an area view requires `asp-area=""` (empty string) to clear the ambient area and target `/Home/Index` on the root site.
- Linking to a **different** area requires explicit `asp-area="Support"` — ambient Admin context would otherwise stay in the URL.
- Redirects after POST must also pass area route values explicitly or the user may leave the area URL space.

---

#### Q10. What is the difference between root `Controllers` and `Areas/Admin/Controllers`?

**Answer:** Root controllers in `/Controllers` serve the default site without an area route segment (`/Home/Index`), while area controllers in `Areas/Admin/Controllers` require the area prefix (`/Admin/Home/Index`) and carry `[Area("Admin")]`.

- Root controllers resolve views from `/Views/{Controller}/{Action}.cshtml`; area controllers resolve from `Areas/Admin/Views/{Controller}/{Action}.cshtml`.
- Both share the same DI container, middleware pipeline, and domain services — only routing and view location differ.
- The same controller class name can exist in both locations because the fully qualified type and route values differ.
- Root controllers do not use `[Area]`; area controllers must declare it for discovery and view resolution.

---

#### Q11. Can two controllers have the same name in different Areas?

**Answer:** Yes — MVC disambiguates by the `area` route value, so `Areas/Admin/Controllers/HomeController` and `Areas/Store/Controllers/HomeController` coexist as separate types matched by `/Admin/Home` vs `/Store/Home`.

- This is legal at compile time because they are different classes in different namespaces/folders.
- Link generation and integration tests must include the `area` route value — omitting it always targets the root controller of that name if one exists.
- Duplicate names increase navigation and testing confusion; some teams rename to `AdminHomeController` for clarity, but the framework does not require it.
- Route order and explicit area segments determine which controller handles a request — there is no automatic ambiguity resolution beyond route values.

---

#### Q12. Where should shared partials used by multiple Areas live?

**Answer:** Cross-area partials belong in the application root `/Views/Shared/`, which the view engine searches after the area's own `Shared` folder when resolving partial names.

- Area-specific partials with different markup or styling stay in `Areas/{AreaName}/Views/Shared/`.
- Invoke shared partials from area views with `<partial name="_OrderSummary" model="..." />` — resolution falls back to root `Views/Shared`.
- Copying the same partial into each area causes drift when one copy is updated and others are not.
- For reuse across multiple MVC applications, extract views into a Razor Class Library with embedded resources.

---

#### Q13. How does layout resolution work for Area views?

**Answer:** Area views resolve layouts through a hierarchical search starting in the area's view folders, then falling back to root `/Views/Shared/`, guided by `Areas/{AreaName}/Views/_ViewStart.cshtml`.

- `Areas/Admin/Views/_ViewStart.cshtml` typically sets `Layout = "_Layout"`, resolving to `Areas/Admin/Views/Shared/_Layout.cshtml` first.
- If the layout is not found in the area, the view engine searches `/Views/Shared/_Layout.cshtml` at the application root.
- Nested layouts work the same as root views — a child layout in the area can call `@RenderBody()` and define sections.
- Each area can maintain distinct chrome (navigation, branding) via its own `_Layout.cshtml` without affecting other areas.

---

#### Q14. What is `Areas/{AreaName}/Views/_ViewStart.cshtml` used for?

**Answer:** It applies layout and other view-start directives to every Razor view under that area's `Views` folder, equivalent to root `_ViewStart.cshtml` but scoped to the area subtree.

- Typical content: `@{ Layout = "_Layout"; }` pointing to the area's shared layout.
- Runs before each view in `Areas/Admin/Views/` is rendered — individual views can override with `Layout = null` or a different layout path.
- Does not affect root `/Views/` or other areas — each area has its own independent `_ViewStart` chain.
- Can also set common `ViewBag` title prefixes or other view-level defaults for the area.

---

#### Q15. How do `_ViewImports` files scope between root Views and Area Views?

**Answer:** `_ViewImports.cshtml` applies hierarchically to views in its directory and subdirectories — root `/Views/_ViewImports.cshtml` does not import into `Areas/Admin/Views/` unless a separate area `_ViewImports` exists there.

- Each area needs its own `Areas/Admin/Views/_ViewImports.cshtml` for area-specific `@using`, `@inject`, and `@addTagHelper` directives.
- Root `_ViewImports` should contain site-wide usings only — polluting it with area-specific namespaces couples root views to Admin internals.
- Shared tag helpers (e.g., `Microsoft.AspNetCore.Mvc.TagHelpers`) are often duplicated in both root and area `_ViewImports` files.
- Think of each area's `_ViewImports` as a mini root for that view subtree with the same scoping rules.

---

#### Q16. How do you apply authorization to an entire Area?

**Answer:** Apply authorization structurally with an area authorization convention, a base controller class all area controllers inherit, or a global filter scoped to the area namespace — not by relying on per-controller `[Authorize]` alone.

- Register a convention: `options.Conventions.Add(new AreaAuthorizationConvention("Admin", "Administrator"))` in `AddControllersWithViews`.
- Create `AdminBaseController` with `[Authorize(Policy = "Administrator")]` and inherit all Admin controllers from it.
- Use `[AllowAnonymous]` on specific controllers (e.g., `AccountController` login) to override area-wide requirements.
- Fallback policies affect the entire app — prefer targeted area conventions over a global deny-all when only some areas need protection.

---

#### Q17. How do you map `/Admin` to a default dashboard action in the Admin area?

**Answer:** Register a dedicated route before the generic area route with a fixed area prefix and default controller/action values, pinning `area = "Admin"` in route defaults.

- Pattern example: `"Admin/{controller=Dashboard}/{action=Index}/{id?}"` with `defaults: new { area = "Admin" }` maps `/Admin` to `DashboardController.Index` in the Admin area.
- `DashboardController` must carry `[Area("Admin")]` and live under `Areas/Admin/Controllers/`.
- Register this route **before** the generic `{area:exists}` route and before the default site route to prevent `Admin` being captured as a root controller name.
- Alternative: `MapGet("/Admin", () => Results.Redirect("/Admin/Dashboard"))` for a simple shortcut without changing controller defaults.

---

#### Q18. When should you use Areas vs Razor Class Libraries vs separate applications?

**Answer:** Use Areas for one deployable MVC app with distinct URL namespaces and shared domain logic; use Razor Class Libraries for shared UI packages across apps; use separate applications when release independence, scaling, or security isolation require different deployable boundaries.

- **Areas fit** when one team ships one container, sharing `DbContext`, authentication cookies, and services across Admin, Store, and Marketing surfaces.
- **RCLs fit** when multiple MVC hosts need identical partials, tag helpers, or embedded views without sharing business boundaries.
- **Separate apps fit** when Admin must be network-isolated (VPN-only), teams release on different cadences, or Store needs independent scale-out beyond what one process provides.
- Areas are a routing and view organization tool — they do not provide process isolation, separate databases, or independent deployment pipelines.

---

## Chapter 11. Action Filters in MVC

#### Q1. What are action filters in ASP.NET Core MVC?

**Answer:** Action filters are components that run before and after MVC action methods (and around other pipeline stages) to implement cross-cutting concerns such as logging, validation, caching, and authorization without duplicating code in every action.

- They implement interfaces like `IActionFilter`, `IAsyncActionFilter`, `IAuthorizationFilter`, `IResourceFilter`, `IResultFilter`, and `IExceptionFilter`.
- Filters receive rich MVC context objects (`ActionExecutingContext`, `ResultExecutingContext`) with route data, action arguments, and the ability to short-circuit by setting `context.Result`.
- They are registered globally, at the controller level, or on individual actions via attributes or `AddControllersWithViews` configuration.
- Built-in attributes such as `[Authorize]`, `[ValidateAntiForgeryToken]`, and `[ResponseCache]` are implemented as filters.

---

#### Q2. What is the MVC filter pipeline?

**Answer:** The MVC filter pipeline is the ordered sequence of filter stages that wrap endpoint execution after routing selects an MVC action but before and after the action method and result execute.

- Stages run in order: authorization → resource → action → (action method) → exception handling → result.
- Within each stage, filters run by scope (global → controller → action) and then by `IOrderedFilter.Order`.
- A filter can set `context.Result` to short-circuit remaining stages — for example, returning 401 before model binding completes.
- The pipeline is distinct from middleware — filters only run for MVC controller actions and Razor Pages, not for Minimal API endpoints unless equivalent middleware is used.

---

#### Q3. What are the filter stages (authorization, resource, action, exception, result)?

**Answer:** MVC defines five filter stages that execute in a fixed order around the action method and its result, each addressing a different concern in the request lifecycle.

- **Authorization filters** (`IAuthorizationFilter`) run first — enforce authentication/authorization and antiforgery validation before resource and action stages.
- **Resource filters** (`IResourceFilter`) wrap the rest of the pipeline — useful for short-circuiting before model binding or caching per request via `HttpContext.Items`.
- **Action filters** (`IActionFilter` / `IAsyncActionFilter`) run immediately before and after the action method — logging, model-state checks, and idempotency guards belong here.
- **Exception filters** (`IExceptionFilter`) handle exceptions thrown from actions or earlier filters when not caught elsewhere.
- **Result filters** (`IResultFilter`) run before and after the `IActionResult` executes — view rendering, output caching, and response header manipulation.

---

#### Q4. What is the difference between action filters and middleware?

**Answer:** Middleware runs for every request that reaches it in the pipeline (including static files and Minimal APIs), while action filters run only for matched MVC/Razor Page endpoints after routing and endpoint selection.

- Middleware executes before routing and has no access to action descriptors, bound models, or `ActionArguments` — filters have full MVC context.
- Short-circuiting in middleware prevents later middleware from running; filter short-circuiting skips remaining filter stages and the action but cannot undo middleware that already executed.
- Cross-cutting concerns affecting all requests (correlation IDs, HTTPS, static files) belong in middleware; per-action audit with route values belongs in action filters.
- Expensive work that must run before authentication (rate limiting all traffic) belongs in middleware placed before auth — `[Authorize]` filters run too late to prevent that cost.

---

#### Q5. What are `IActionFilter` and `IAsyncActionFilter`?

**Answer:** `IActionFilter` defines synchronous `OnActionExecuting` and `OnActionExecuted` methods; `IAsyncActionFilter` defines `OnActionExecutionAsync` for async work before and after the action without blocking thread-pool threads.

- `OnActionExecuting` runs before the action — set `context.Result` to skip the action (e.g., return `BadRequestObjectResult` for invalid model state).
- `OnActionExecuted` runs after the action — inspect or modify `context.Result`, log exceptions from `context.Exception`.
- Prefer `IAsyncActionFilter` when the filter performs I/O (database lookups, HTTP calls) — blocking with `.GetAwaiter().GetResult()` in `IActionFilter` causes thread-pool starvation under load.
- Both interfaces participate in the same ordering rules via `IOrderedFilter.Order`.

---

#### Q6. What is `IAuthorizationFilter`?

**Answer:** `IAuthorizationFilter` runs in the authorization stage before resource and action filters, determining whether the caller may execute the action — the built-in `[Authorize]` attribute implements this interface.

- `OnAuthorization` receives `AuthorizationFilterContext` with `HttpContext.User`, endpoint metadata, and the ability to set `context.Result` to `ChallengeResult` or `ForbidResult`.
- `[ValidateAntiForgeryToken]` is implemented as an authorization filter (`ValidateAntiForgeryTokenAuthorizationFilter`), not an action filter — invalid tokens reject before model binding.
- Authorization filters run after authentication middleware has populated `HttpContext.User` but before the action executes.
- Multiple authorization filters all run in order; any one can short-circuit with an unauthorized result.

---

#### Q7. What is `IExceptionFilter`?

**Answer:** `IExceptionFilter` handles exceptions thrown during action execution or earlier filter stages, allowing MVC-specific error responses such as custom views or JSON `ProblemDetails` before the exception propagates to middleware.

- `OnException` receives `ExceptionContext` with the thrown exception and can set `context.ExceptionHandled = true` and assign `context.Result`.
- Setting `ExceptionHandled = true` and then rethrowing causes double handling when global exception middleware also processes the same fault — pick one strategy.
- For API-heavy apps, centralized `IExceptionHandler` middleware often replaces exception filters; filters remain useful for returning area-specific HTML error views.
- Do not expose raw `Exception.Message` in Production responses — log server-side and return safe generic messages.

---

#### Q8. What is `IResultFilter`?

**Answer:** `IResultFilter` runs before and after the `IActionResult` executes — for example before a `ViewResult` renders Razor and after the view engine produces output.

- `OnResultExecuting` can replace `context.Result` entirely (e.g., return cached `ContentResult` instead of executing a `ViewResult`) to skip view rendering.
- Writing directly to the response stream in `OnResultExecuting` without replacing `context.Result` does not prevent the original result from executing — assign a new result to short-circuit.
- `OnResultExecuted` runs after the result executes — useful for response compression hooks or logging rendered status codes.
- Result filters do not run for static file responses or Minimal API `IResult` handlers that bypass the MVC result pipeline.

---

#### Q9. What is `IResourceFilter`?

**Answer:** `IResourceFilter` wraps execution of all later filter stages and the action itself, running immediately after authorization filters and before model binding and action filters on the way in.

- `OnResourceExecuting` can set `context.Result` to short-circuit before model binding — useful for request-level caching checks via `HttpContext.Items`.
- `OnResourceExecuted` runs after the action and result complete on the way out.
- `HttpContext.Items` is the correct per-request cache for resource filters — static fields leak data across requests and tenants.
- Resource filters are less commonly authored than action filters but are the earliest stage that still has full MVC endpoint context.

---

#### Q10. What is the difference between global, controller-level, and action-level filters?

**Answer:** Filters apply at three scopes — global filters affect every MVC action, controller-level filters affect all actions on that controller, and action-level filters affect only the decorated action — with all three potentially running on a single request.

- Global filters register in `AddControllersWithViews(options => options.Filters.Add...)` or via `AddService<T>()`.
- Controller-level filters use class attributes: `[ServiceFilter(typeof(AuditFilter))]` on the controller class.
- Action-level attributes on a single method add filters only for that action.
- When multiple scopes apply, filters within the same stage run in order: global → controller → action, then by `IOrderedFilter.Order` within each scope.

---

#### Q11. How does filter order (`IOrderedFilter`) work?

**Answer:** Filters implementing `IOrderedFilter` expose an `Order` property — lower values run first on the executing (before) leg and last on the executed (after) leg, mirroring middleware's onion model.

- Default `Order` is `0` when not specified — explicit negative values (e.g., `-1000`) run before default-ordered filters on the executing side.
- On the executing leg: sort ascending by `Order` — idempotency guards should use large negative values to run before validation filters.
- On the executed leg: order reverses — validation's `OnActionExecuted` runs before idempotency's post-action logic.
- Scope (global → controller → action) is applied before `Order` within the same filter type and stage.

---

#### Q12. What is the difference between `[ServiceFilter]` and `[TypeFilter]`?

**Answer:** `[ServiceFilter(typeof(MyFilter))]` resolves the filter entirely from DI using its registered constructor dependencies, while `[TypeFilter(typeof(MyFilter), Arguments = new object[] { 60 })]` constructs the filter via a factory, supplying extra constructor arguments alongside DI-resolved services.

- `ServiceFilter` requires `builder.Services.AddScoped<MyFilter>()` (or appropriate lifetime) — all constructor parameters come from DI.
- `TypeFilter` supports passing primitive or configuration values (e.g., rate limit threshold) as `Arguments` while DI fills interfaces like `ILogger` or `IMemoryCache`.
- Both require filter dependencies to be registered in the service collection — missing registrations throw at first request activation.
- `TypeFilter` is useful when the same filter type needs different parameter values on different controllers without separate registered types.

---

#### Q13. How do you register a global filter in `AddControllersWithViews`?

**Answer:** Register the filter type in DI and add it to `MvcOptions.Filters` inside `AddControllersWithViews`, using `AddService<T>()` for DI-managed filters or `Add<T>()` for filters without scoped dependencies.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<AuditActionFilter>();
});
```

- `AddService<T>()` resolves the filter from DI per request — required when the filter injects scoped services like `DbContext`.
- `Add<T>()` without service registration creates the filter via `ObjectFactory` and can cause captive dependency errors with scoped services.
- Global filters apply to all MVC controller actions but not to Minimal API endpoints mapped separately.
- Filter order can be controlled by implementing `IOrderedFilter` on the filter class.

---

#### Q14. What is `[ValidateAntiForgeryToken]` as a filter?

**Answer:** `[ValidateAntiForgeryToken]` is an authorization filter attribute that validates the antiforgery token on unsafe HTTP methods (POST, PUT, DELETE) before the action executes, protecting against cross-site request forgery.

- It is implemented by `ValidateAntiForgeryTokenAuthorizationFilter` — it runs in the authorization stage, not the action stage.
- Validation checks for `__RequestVerificationToken` form field or `RequestVerificationToken` header matching the cookie token issued by `IAntiforgery`.
- Invalid or missing tokens cause a 400 Bad Request before model binding and action execution — the action method never runs.
- Form Tag Helpers emit the token automatically; AJAX and `fetch` calls must include the token manually.

---

#### Q15. What is `[AutoValidateAntiforgeryToken]`?

**Answer:** `[AutoValidateAntiforgeryToken]` is a controller-level attribute (or global convention) that applies antiforgery validation to all unsafe HTTP methods on every action in the controller without decorating each action individually.

- Equivalent to placing `[ValidateAntiForgeryToken]` on every POST, PUT, PATCH, and DELETE action in the controller.
- Safe methods (GET, HEAD, OPTIONS) are not validated — idempotent reads remain unaffected.
- When applied globally via `AutoValidateAntiforgeryTokenAttribute` as a filter convention, all MVC POST actions require tokens unless opted out.
- AJAX JSON endpoints on the same controller inherit the requirement — clients must send the token header or receive 400 responses.

---

#### Q16. What is `[IgnoreAntiforgeryToken]`?

**Answer:** `[IgnoreAntiforgeryToken]` opts an action or controller out of antiforgery validation, adding metadata that causes `ValidateAntiForgeryTokenAuthorizationFilter` to skip token checks for that endpoint.

- Use for webhook endpoints that authenticate via HMAC signatures or API keys instead of cookie-based CSRF tokens.
- Overrides `[AutoValidateAntiforgeryToken]` on the same controller for specific actions decorated with `[IgnoreAntiforgeryToken]`.
- Should not be applied to state-changing browser-facing forms or cookie-authenticated AJAX endpoints — it removes CSRF protection.
- Minimal API and bearer-token APIs typically do not use antiforgery at all — CSRF protection is relevant for cookie-based browser sessions.

---

#### Q17. What is `[Authorize]` as an authorization filter?

**Answer:** `[Authorize]` is an authorization filter attribute that enforces authentication and optional policy/role requirements before the action executes, setting `context.Result` to a challenge or forbid response when the user is not permitted.

- Runs after authentication middleware has populated `HttpContext.User` from cookies, JWT bearer, or other handlers.
- Supports `Roles`, `Policy`, and `AuthenticationSchemes` properties to target specific authorization requirements.
- `[AllowAnonymous]` on an action overrides controller-level or global `[Authorize]` for that endpoint.
- Authorization filters short-circuit the pipeline — unauthorized requests never reach model binding or the action method.

---

#### Q18. When should you use a filter instead of middleware for MVC-specific concerns?

**Answer:** Use filters when the concern requires MVC context — action name, route values, bound model arguments, or `IActionResult` manipulation — and use middleware when the concern applies to all request types or must run before routing.

- Per-action audit logging with `ActionArguments` and controller metadata belongs in an action filter — middleware only sees the URL path.
- Uniform antiforgery validation on MVC POST actions is filter-based; middleware has no built-in equivalent with the same token contract.
- Response caching of rendered views via result filters requires access to `ViewResult` — middleware cannot intercept view engine output the same way.
- Correlation IDs, HTTPS redirection, request size limits, and static file handling belong in middleware because they apply before endpoint selection or outside MVC entirely.

---

## Chapter 12. TempData, ViewData & ViewBag

#### Q1. What is `ViewBag` in ASP.NET Core MVC?

**Answer:** `ViewBag` is a dynamic property on `Controller` and `ViewPage` that provides a loosely typed dictionary for passing ad hoc data from an action to a view without declaring a ViewModel property.

- Implemented as a wrapper around `ViewData` using `dynamic` — assignments like `ViewBag.Title = "Home"` store entries in the shared view dictionary.
- Property access in Razor (`@ViewBag.Title`) resolves at runtime — typos compile without error and render as blank output.
- Suitable for incidental page metadata (title, active tab, layout flags) rather than primary page data or form models.
- Does not survive redirects — it is scoped to the current request's view rendering only.

---

#### Q2. What is `ViewData` and how does it differ from `ViewBag`?

**Answer:** `ViewData` is a strongly keyed `ViewDataDictionary` on the controller and view context; `ViewBag` is a dynamic wrapper over the same underlying dictionary, so writes through either are visible to both.

- `ViewData["Title"] = "Home"` and `ViewBag.Title = "Home"` store the same entry — they are not separate stores.
- `ViewData` supports typed access via `ViewData.Model` (the `@model` type) and requires string keys for arbitrary entries.
- `ViewBag` offers dot-syntax convenience but sacrifices compile-time checking on property names in views.
- Both are request-scoped and do not persist across `RedirectToAction` — unlike `TempData`.

---

#### Q3. What is `TempData` and when is it used?

**Answer:** `TempData` is a dictionary backed by a temp-data provider (cookie or session) that persists values across a redirect to the next HTTP request, making it ideal for flash messages after Post-Redirect-Get flows.

- Values written in a POST action survive `RedirectToAction` and are available when the subsequent GET action and view render.
- Typical use: `TempData["SuccessMessage"] = "Order created."` after a successful form submission followed by redirect to a details page.
- Backed by `ITempDataProvider` — the default in ASP.NET Core 8 serializes data into an encrypted cookie via Data Protection.
- Designed for short-lived, small payloads (status messages, flags) — not for transporting large view models or domain entities.

---

#### Q4. What is the difference between `ViewBag`, `ViewData`, and `TempData`?

**Answer:** All three pass data from controllers to views, but they differ in typing, lifetime, and persistence across redirects.

- **ViewBag / ViewData:** request-scoped only — available during the current action's view rendering; lost after `RedirectToAction`.
- **TempData:** survives one redirect (by default) via cookie or session provider — designed for flash messaging across PRG.
- **ViewBag** is dynamic; **ViewData** uses string keys; **TempData** uses string keys with cross-request persistence semantics.
- For primary page data and forms, prefer a strongly typed ViewModel over any of the three — they are supplementary mechanisms.

---

#### Q5. Why is `TempData` used after `RedirectToAction`?

**Answer:** After a redirect the browser issues a new GET request with no connection to the previous POST's `ViewBag`, `ViewData`, or `ModelState` — TempData is the built-in mechanism to carry a small message or flag into that next request.

- `RedirectToAction` returns 302/303 — the POST response body (including any ViewBag values) is discarded by the browser.
- TempData serializes values into the temp-data cookie (or session) so the GET action's layout or view can display a success banner.
- Without TempData, the user completes an action but sees no confirmation on the redirected page.
- Only the route values (e.g., `{ id = orderId }`) and TempData survive — reload the entity from the database on GET rather than passing it through TempData.

---

#### Q6. What is the Post-Redirect-Get (PRG) pattern?

**Answer:** PRG is the pattern of responding to a successful POST with an HTTP redirect to a GET action, preventing the browser from re-submitting the form when the user refreshes the page.

- POST validates input and performs the mutation; on success, return `RedirectToAction(nameof(Details), new { id })` instead of `return View()`.
- The browser's refresh on the GET page repeats a safe read — not the original POST — avoiding duplicate creates or charges.
- On validation failure, return `View(model)` in the same POST response to preserve `ModelState` without redirecting.
- TempData carries ephemeral success messages across the redirect; the GET action loads fresh data from services by route id.

---

#### Q7. Why doesn't `ModelState` survive a redirect?

**Answer:** `ModelState` is stored in the controller's `ViewDataDictionary` for the current HTTP request only — a redirect ends that request and starts a new one with an empty `ModelState`.

- After `RedirectToAction`, the GET action receives no knowledge of previous validation errors unless explicitly rehydrated.
- Model binding on the GET request populates a fresh model from route/query values, not from the failed POST fields.
- This is by design — PRG intentionally separates the command (POST) from the query (GET) request lifecycle.
- Re-displaying validation errors after redirect requires TempData serialization helpers, a second validation pass on GET, or avoiding redirect on validation failure.

---

#### Q8. How can validation errors survive a redirect?

**Answer:** Prefer `return View(model)` on validation failure without redirect to keep `ModelState` intact; if redirect is required, serialize errors to TempData or re-validate on the GET action.

- Standard pattern: invalid POST → `return View(model)` (same request, ModelState preserved); valid POST → redirect with TempData success message.
- Third-party helpers (e.g., `TempData.Put("ModelState", ModelState)`) serialize errors to TempData — watch cookie size limits.
- Alternative: redirect to GET with the entity id and run server-side validation again in the GET action before displaying the form.
- Client-side validation state is also lost on redirect — the GET view re-renders from server-side ModelState or fresh validation.

---

#### Q9. What is the difference between cookie-based and session-based TempData?

**Answer:** Cookie-based TempData (the default `CookieTempDataProvider`) serializes values into an encrypted cookie sent with the next request; session-based TempData stores values server-side in ASP.NET session keyed by session id.

- **Cookie provider:** no server-side session store required; works across load-balanced pods without sticky sessions; subject to cookie size limits (~4 KB per cookie).
- **Session provider (`SessionStateTempDataProvider`):** supports larger arbitrary objects; requires session middleware and either sticky sessions or distributed session (Redis/SQL) in multi-server deployments.
- Cookie TempData uses ASP.NET Core Data Protection for encryption — key rings must be synchronized across instances or cookies become unreadable after deploy to another node.
- Configure via `builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider()` or cookie provider in MVC options.

---

#### Q10. What happens to TempData when it is read?

**Answer:** Reading a TempData key with the indexer (`TempData["Key"]`) marks it for deletion at the end of the current request — it is a read-once-by-default flash semantics.

- After the request completes, marked keys are removed and will not appear on the subsequent request.
- If both the layout and the view read the same key in one request, the first read consumes it unless `Peek` is used for the first access.
- `TempData.Keep("Key")` explicitly preserves a key for the **next** request even after it was read.
- This consume-on-read behavior makes TempData behave like a one-shot message queue, not persistent session state.

---

#### Q11. What is `TempData.Keep()` used for?

**Answer:** `TempData.Keep("Key")` marks a TempData entry to survive into the next HTTP request even after it has been read in the current request — extending flash message life across a redirect chain.

- Use when a message must display across two consecutive GET requests (e.g., a multi-step redirect flow).
- Without `Keep`, a read TempData key is deleted at end of request and absent on the next round-trip.
- Overusing `Keep` causes messages to reappear on unintended pages — flash messages should normally die after one display.
- Distinct from `Peek`: `Keep` affects the **next** request; `Peek` allows multiple reads within the **same** request.

---

#### Q12. What is `TempData.Peek()` used for?

**Answer:** `TempData.Peek("Key")` reads a TempData value without marking it for deletion, allowing multiple components in the same request (layout and view) to read the same flash message.

- Use in the layout to inspect a message while leaving it available for the child view in the same render pass.
- The key is still subject to normal deletion at end of request unless also `Keep()`'d for the next request.
- Preferred over double indexer reads when both layout and view need the same TempData key in one round-trip.
- Best practice: centralize flash display in a single `_FlashMessages.cshtml` partial invoked from the layout to avoid double-read issues entirely.

---

#### Q13. What are the size limits of cookie-based TempData?

**Answer:** Cookie-based TempData serializes all entries into a single cookie (typically `.AspNetCore.Mvc.CookieTempDataProvider`), constrained by browser cookie limits of approximately 4096 bytes per cookie and total request header size limits of roughly 8–16 KB.

- Storing large objects (full view models, validation error collections for 40+ fields) exceeds cookie capacity and throws or silently fails on redirect.
- Total header size includes all cookies, antiforgery tokens, and auth cookies — TempData competes for the same budget.
- Store only small identifiers and messages in TempData (e.g., `orderId`, `"Saved successfully"`) and reload data from services on GET.
- Switch to session-based TempData with distributed session only when large flash payloads are genuinely required and session infrastructure already exists.

---

#### Q14. Why does session-based TempData fail behind load balancers without sticky sessions?

**Answer:** Session-based TempData stores data server-side keyed by session id — after redirect, the browser may hit a different pod that does not hold that session entry, returning empty TempData.

- In-memory session on one node is invisible to other nodes in a round-robin load-balanced deployment.
- Sticky sessions (session affinity) route the same client to the same pod, masking the problem but reducing failover flexibility.
- Fix: use cookie-based TempData (default) for flash strings, or configure distributed session (Redis, SQL Server) shared by all pods.
- Data Protection key ring synchronization is also required for cookie TempData across nodes — unrelated to session but equally critical for multi-instance deployments.

---

#### Q15. When should you use `ViewBag`/`ViewData` instead of a ViewModel?

**Answer:** Use ViewBag or ViewData for incidental page metadata that is not part of the primary data contract — page title, active navigation tab, breadcrumb flags, or one-off layout toggles — not for form models or domain data.

- `ViewBag.Title = "Dashboard"` in an action and `@ViewBag.Title` in a layout is a common, acceptable pattern.
- Passing `"ActiveTab" => "Settings"` to highlight navigation avoids bloating a ViewModel with presentation-only properties.
- Action filters can set ViewData entries consumed by layouts without changing the action's return model.
- Anything with validation attributes, client-side validation, or more than a few simple properties belongs in a strongly typed ViewModel.

---

#### Q16. When should you not use `ViewBag` for layout data?

**Answer:** Avoid ViewBag for data that multiple views depend on with compile-time safety, data shared between layout and child views with complex types, or any value where a typo causes silent runtime failures.

- Strongly typed layout models or view components provide compile-time checking that ViewBag's dynamic access cannot offer.
- When the layout and view both need the same value, ViewBag typos (`ViewBag.UsreName`) render blank without build errors.
- Authorization or role checks should not live in ViewBag flags set from actions — use policy-based authorization and view components instead.
- Large or structured data (user profile objects, cart summaries) should use ViewModels or view components, not ViewBag dynamic properties.

---

#### Q17. Does TempData work on AJAX partial responses the same as full page redirects?

**Answer:** No — TempData is designed for the next full HTTP request after a redirect; AJAX partial updates that return HTML or JSON in the same POST response cycle do not re-render the layout where TempData is typically consumed.

- The layout executes on the initial full-page load — TempData written during an AJAX POST is not injected into an already-rendered DOM.
- Partial view responses replace a page fragment — the layout's TempData block does not re-execute.
- For AJAX success messages, return the message in the JSON response body or embed a toast element in the returned partial HTML.
- Reserve TempData for full-page POST → redirect → GET flows; use response payloads, SignalR, or client-side state for in-place updates.

---

#### Q18. What data should never be stored in TempData?

**Answer:** Never store sensitive secrets, large domain objects, PII beyond minimal identifiers, or anything that should persist beyond a single flash display — TempData is serialized into cookies or session and consumed ephemerally.

- Passwords, API keys, credit card numbers, and auth tokens must not pass through TempData — cookies are client-visible (even if encrypted) and logs may capture values.
- Full entity graphs or large view models exceed cookie size limits and expose internal schema details unnecessarily.
- Instead of storing an `OrderSummaryViewModel` in TempData, store `TempData["OrderId"] = id` and load the summary from the service on the GET action.
- Treat TempData as a flash notification channel — short strings, status flags, and route-correlated identifiers only.

---

> **Target:** ASP.NET Core 8 MVC

---

## Chapter 13. AJAX & Partial Page Updates

#### Q1. What is a partial page update in ASP.NET Core MVC?

**Answer:** A partial page update refreshes only a fragment of the current page instead of performing a full browser navigation. The server returns a Razor partial view (HTML fragment) or JSON, and client-side JavaScript swaps that content into a target DOM container.

- Typical use cases include infinite scroll, inline edit panels, cart summaries, and filter grids without reloading the layout.
- The initial page still renders server-side for SEO and no-JavaScript fallback; AJAX enhances the same partial used on first load.
- ASP.NET Core 8 MVC supports this through `PartialView()`, unobtrusive AJAX, `fetch`, HTMX, or similar client swap patterns.
- Partial updates keep layout, navigation, and global scripts stable while only the widget region changes.

---

#### Q2. What is `PartialView()` and what does it return?

**Answer:** `PartialView()` is a controller helper that returns a `PartialViewResult` rendering a named Razor view without a layout. The HTTP response body is an HTML fragment suitable for injection into an existing page.

- Overloads accept a view name and optional model: `return PartialView("_CartSummary", model);`.
- Unlike `View()`, partial views do not apply `_ViewStart` layout by default — they render markup only.
- The result still runs through the MVC filter pipeline, model binding, and validation like any action result.
- Partial views should be strongly typed and live under `Views/Shared` or controller-specific `Views` folders.

---

#### Q3. What is the difference between returning `PartialView` and `Json` from an AJAX action?

**Answer:** `PartialView` returns server-rendered HTML that Razor encodes by default; `Json()` returns serialized data for the client to render. Choose based on who owns markup generation and whether you need shared partials across full-page and AJAX paths.

- `PartialView` reuses existing Razor partials, preserves encoding rules, and works well when markup is complex or must match server-rendered output.
- `Json` suits optimistic UI, mobile/API consumers, or rich client frameworks that build DOM from DTOs.
- HTML fragments tie the contract to DOM structure; JSON ties it to a stable DTO schema.
- Many MVC apps standardize on `PartialView` for list refresh and reserve JSON for non-HTML clients or lightweight state updates.

---

#### Q4. How does model binding differ for AJAX POST with `FormData` vs JSON?

**Answer:** `FormData` and `application/x-www-form-urlencoded` bodies bind through the form value provider to action parameters and complex types without `[FromBody]`. JSON requires `Content-Type: application/json` and `[FromBody]` on a complex type for the JSON input formatter to deserialize.

- Simple parameters (`int id`) bind from form fields by name; JSON simple parameters do not bind unless wrapped in a model class with `[FromBody]`.
- `FormData` supports file uploads (`IFormFile`); JSON does not carry multipart files without a separate upload endpoint.
- Mixing `[FromBody]` on an action with a form-encoded AJAX POST leaves the model empty or default — a common silent bug.
- Match the client payload shape to the action signature explicitly in ASP.NET Core 8 MVC.

---

#### Q5. How do you include an antiforgery token in a `fetch`/AJAX request?

**Answer:** Read the hidden field `__RequestVerificationToken` rendered by the form tag helper or `@Html.AntiForgeryToken()` and send it as the `RequestVerificationToken` header or as a form field in the POST body. The antiforgery cookie is sent automatically on same-origin requests.

```javascript
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
await fetch('/Cart/Add', {
    method: 'POST',
    headers: { 'RequestVerificationToken': token },
    body: new URLSearchParams({ productId: '42', quantity: '1' })
});
```

- For `FormData`, append `__RequestVerificationToken` alongside other fields.
- Cross-origin requests require CORS with credentials and cannot use wildcard origins when cookies are involved.
- Missing or mismatched tokens produce **400 Bad Request** before the action executes.
- `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe HTTP methods automatically.

---

#### Q6. What is `RequestVerificationToken` and how is it validated on AJAX POSTs?

**Answer:** `RequestVerificationToken` is the HTTP header name ASP.NET Core antiforgery accepts as an alternative to the hidden form field `__RequestVerificationToken`. The antiforgery filter compares the submitted token with the token stored in the antiforgery cookie.

- Validation runs as a filter before the action method — mismatch or absence yields 400, not 401.
- The cookie-and-field pair prevents cross-site request forgery for cookie-authenticated MVC forms.
- Header name is case-insensitive; the value must match the rendered hidden field value.
- Bearer-authenticated API endpoints typically skip antiforgery; cookie-auth MVC partial endpoints must not.

---

#### Q7. What is the difference between `[ValidateAntiForgeryToken]` and `[AutoValidateAntiforgeryToken]` for AJAX?

**Answer:** `[ValidateAntiForgeryToken]` applies antiforgery validation to a single action. `[AutoValidateAntiforgeryToken]` applies it to all unsafe HTTP methods (POST, PUT, PATCH, DELETE) on the decorated controller or action scope.

- Both use the same token mechanism — cookie plus field or `RequestVerificationToken` header.
- `[AutoValidateAntiforgeryToken]` at the controller level is the preferred default for MVC apps with many POST actions.
- AJAX POSTs fail identically under either attribute when the token is missing.
- `[IgnoreAntiforgeryToken]` opts a specific action out — use only for webhooks or explicitly authenticated API endpoints.

---

#### Q8. What is unobtrusive AJAX (`data-ajax="true"`)?

**Answer:** Unobtrusive AJAX is a jQuery-based library (`jquery.unobtrusive-ajax`) that intercepts form and link submissions marked with `data-ajax="true"` and performs asynchronous requests, swapping the response into a target element without full page reload.

- Attributes such as `data-ajax-url`, `data-ajax-update`, `data-ajax-method`, and `data-ajax-confirm` declare behavior in HTML.
- The server still returns `PartialView` or other action results; the library handles the HTTP call and DOM update.
- It integrates with MVC form tag helpers and antiforgery tokens when forms are rendered server-side.
- After dynamic HTML injection, unobtrusive validation and AJAX parsers may need to be re-run on the new DOM subtree.

---

#### Q9. Why should AJAX partial endpoints check response status before injecting HTML?

**Answer:** Without checking `response.ok` or the HTTP status code, error responses — including full error pages from exception middleware — get injected into the partial container via `innerHTML`, breaking layout and hiding failures from users.

- A 500 from `UseExceptionHandler` may return a full `/Home/Error` page with layout and navigation nested inside a widget.
- Client code should branch on status: show a toast or inline error for non-2xx responses instead of blindly assigning `response.text()`.
- Server-side, detect AJAX requests via `Accept` header, `X-Requested-With`, or a custom header and return compact JSON or minimal error fragments.
- Always validate success before parsing HTML or updating the DOM.

---

#### Q10. What `Cache-Control` headers should dynamic partial views use?

**Answer:** User-specific or frequently changing partials should use `Cache-Control: private, no-store` or `[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]` to prevent browsers and CDNs from serving stale HTML.

- Anonymous fragments that can be cached briefly may use short TTL with `VaryByQueryKeys` for cache keys tied to parameters.
- Add `Vary: Cookie` when output differs by authentication state — otherwise shared caches may serve one user's cart to another.
- Do not apply static-file CDN cache rules to MVC partial GET routes by default.
- Inventory badges, cart summaries, and personalized widgets are typical no-store candidates.

---

#### Q11. What XSS risks exist when injecting server-rendered HTML via `innerHTML`?

**Answer:** Assigning server HTML to `innerHTML` causes the browser to parse and execute any script or event handlers embedded in that markup. If the partial used `@Html.Raw` on user content or unsanitized data, attacker-controlled script runs in the victim's session.

- Razor's default `@` encoding is safe; `Html.Raw` on user-influenced strings bypasses that protection.
- AJAX partials are not safer than full pages — any injected HTML is an XSS surface.
- Prefer structured JSON plus `textContent` for user-generated text, or encode first and highlight second server-side.
- Content-Security-Policy is defense in depth, not a substitute for proper encoding.

---

#### Q12. Why do duplicate HTML `id` attributes break AJAX-loaded partials?

**Answer:** HTML requires unique `id` values document-wide. Loading the same partial twice duplicates ids such as `edit-form`, so `getElementById`, label associations, and one-time event listeners target only the first match.

- Replace ids with classes inside partials and use event delegation on a stable parent container.
- When ids are required for accessibility, scope them with a suffix: `id="edit-row-@Model.Id"`.
- After each `innerHTML` swap, call an `initPanel(container)` to rebind unobtrusive validation and AJAX parsers.
- Duplicate ids produce invalid HTML and unpredictable JavaScript behavior across multiple AJAX-loaded panels.

---

#### Q13. What is the difference between `Html.PartialAsync` returned from an action vs a full `View`?

**Answer:** `Html.PartialAsync` in a view synchronously renders a partial into the current page during server-side composition. `PartialView()` from a controller action returns a standalone HTTP response containing only the partial HTML for AJAX or direct requests.

- `PartialAsync` is for embedding fragments while building a full page response.
- `PartialView()` as an action result is the AJAX endpoint contract — no layout, fragment only.
- Both render the same `.cshtml` file but differ in HTTP context: inline composition vs separate request/response.
- AJAX clients call the action URL and inject the returned fragment; full pages call `PartialAsync` during initial render.

---

#### Q14. How do you handle validation errors in AJAX form submissions?

**Answer:** On the server, check `ModelState.IsValid` and return the form partial with validation messages when invalid, or return 400 with `ValidationProblemDetails` for API-style clients. On the client, replace the form container with the returned partial and re-parse unobtrusive validation.

- Return `PartialView("_Form", model)` with 200 or 400 depending on your client convention — consistency matters more than the specific status code.
- Ensure the partial includes `asp-validation-for` spans and optionally `asp-validation-summary`.
- After injecting the updated form, call `$.validator.unobtrusive.parse('#form-container')` to reattach client rules.
- Server-side validation remains mandatory; client validation is UX only.

---

#### Q15. What happens when `UseExceptionHandler` returns a full error page to a partial AJAX request?

**Answer:** The global exception handler renders the configured error view — often a full page with layout — and the AJAX client injects that entire page HTML into a widget container, producing nested navigation, broken styling, and no user-visible error message.

- The HTTP status may be 500 while the body looks like a normal HTML page.
- Partial-update apps need separate error handling for AJAX: return JSON `{ error: "..." }`, ProblemDetails, or a minimal error partial.
- Client scripts must check `response.ok` before assigning response text to `innerHTML`.
- Use `IExceptionHandler` or exception filters that branch on request type for dual error shapes.

---

#### Q16. What is `[FromBody]` vs form-urlencoded binding for AJAX filter endpoints?

**Answer:** `[FromBody]` expects JSON deserialized by the JSON input formatter. Form-urlencoded or `URLSearchParams` bodies bind through the form value provider when `[FromBody]` is omitted.

- Sending `URLSearchParams` to an action with `[FromBody] ProductFilter filter` leaves the model null or default — the grid shows wrong results silently.
- Fix by removing `[FromBody]` for form-style AJAX filters, or send JSON with `Content-Type: application/json` and `JSON.stringify`.
- MVC partial actions commonly use form encoding to mirror standard HTML form binding.
- Align Content-Type, binding source, and action signature explicitly.

---

#### Q17. What is the difference between jQuery unobtrusive AJAX and `fetch` + `innerHTML`?

**Answer:** jQuery unobtrusive AJAX declares behavior via `data-ajax-*` attributes and handles submission, antiforgery, and DOM swap automatically. `fetch` + `innerHTML` requires explicit token headers, status checks, error handling, and re-parsing of unobtrusive scripts after each swap.

- jQuery unobtrusive couples you to jQuery lifecycle and manual re-parse after dynamic updates.
- `fetch` is native, lighter, and pairs well with modern patterns or HTMX-style swaps.
- Both approaches typically receive the same `PartialView` HTML from the server.
- HTMX (`hx-post`, `hx-target`, `hx-swap`) offers declarative swaps without jQuery while keeping server-rendered partials.

---

#### Q18. How do you design separate actions for full-page POST vs AJAX POST?

**Answer:** Either split into distinct actions (`Create` for full page, `CreatePartial` for AJAX) or branch inside one action using an AJAX detection header such as `X-Requested-With: XMLHttpRequest` or `Accept: text/html` vs `application/json`.

- Full-page success should use Post-Redirect-Get (`RedirectToAction`) to prevent duplicate submission on refresh.
- Full-page validation failure returns `View(model)` with layout; AJAX failure returns `PartialView("_Form", model)`.
- AJAX success returns a row partial or updated widget; full-page success redirects to a list or details page.
- Sharing one action without branching causes browsers to navigate to bare HTML fragments or duplicate rows on refresh.

---

## Chapter 14. Client-Side Validation

#### Q1. What is client-side validation in ASP.NET Core MVC?

**Answer:** Client-side validation runs validation rules in the browser before or during form submission, providing immediate feedback without a full round-trip. In ASP.NET Core 8 MVC it is built on jQuery Validate plus the unobtrusive adapter layer reading `data-val-*` attributes emitted by tag helpers.

- It mirrors server-side Data Annotations for UX — faster feedback on required fields, ranges, lengths, and patterns.
- It does not replace server validation; attackers can bypass browser scripts entirely.
- Enabled by including validation scripts and using tag helpers (`asp-for`, `asp-validation-for`) on forms.
- Can be toggled per request via `ViewContext.ClientValidationEnabled`.

---

#### Q2. What is unobtrusive validation?

**Answer:** Unobtrusive validation separates validation rules from JavaScript code by storing them as HTML `data-val-*` attributes on form fields. `jquery.validate.unobtrusive.js` reads those attributes at page load and configures jQuery Validate without inline script blocks.

- Tag helpers generate attributes from model metadata and Data Annotations at render time.
- Keeps views free of hand-written validation JavaScript for standard annotation rules.
- Custom validation attributes require explicit client adapters to participate in unobtrusive validation.
- After AJAX partial swaps, call `$.validator.unobtrusive.parse()` on the new form container.

---

#### Q3. What scripts are required for unobtrusive client validation?

**Answer:** Load jQuery, then `jquery.validate.min.js`, then `jquery.validate.unobtrusive.min.js` in that order. All three are required — loading only jQuery Validate leaves `data-val-*` attributes inert.

- jQuery must load before the validation scripts.
- Scripts typically live in `wwwroot/lib/jquery-validation` and `wwwroot/lib/jquery-validation-unobtrusive`.
- ASP.NET Core 8 project templates reference these via LibMan or npm and bundle them in layout or a partial.
- Missing the unobtrusive bridge is a common cause of "validation attributes present but nothing happens."

---

#### Q4. What is `_ValidationScriptsPartial`?

**Answer:** `_ValidationScriptsPartial` is a shared Razor partial that references the jQuery Validate and jQuery Validate Unobtrusive script files. Views include it in the `@section Scripts` block only on pages that need client validation.

- Keeps validation script references in one place instead of duplicating paths across every form view.
- Should be rendered after jQuery and before any page-specific scripts that depend on validation.
- Can be conditionally included when `ViewContext.ClientValidationEnabled` is true.
- Does not enable validation by itself — tag helpers must emit `data-val-*` attributes on form fields.

---

#### Q5. What are `data-val-*` attributes and how are they generated?

**Answer:** `data-val-*` attributes are HTML metadata on input elements that describe client validation rules. Tag helpers use `IHtmlGenerator` and `ModelMetadata` to emit them from Data Annotations and validator metadata at render time.

- Examples: `data-val="true"`, `data-val-required`, `data-val-required-message`, `data-val-range`, `data-val-regex`.
- Hand-written `<input name="Email">` without tag helpers carries no `data-val-*` — client validation will not run.
- `ViewContext.ClientValidationEnabled = false` suppresses attribute generation even with tag helpers.
- Custom attributes need `IClientModelValidator` adapters to emit corresponding `data-val-*` hooks.

---

#### Q6. What is the relationship between Data Annotations and client-side validation?

**Answer:** Data Annotations on ViewModels define server-side validation rules and, when client validation is enabled, drive the `data-val-*` attributes that unobtrusive JavaScript consumes. The annotation is the single declarative source; the client rule is a best-effort mirror.

- Standard annotations (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`) map automatically to unobtrusive adapters.
- Custom `ValidationAttribute` subclasses require explicit client adapters — annotations alone do not generate client rules.
- Server `ModelState` validation always runs from the same metadata regardless of client script presence.
- Place annotations on ViewModels, not EF entities shared across persistence and UI concerns.

---

#### Q7. Why is client-side validation not sufficient for security?

**Answer:** Client-side validation executes entirely in the attacker's browser and can be disabled, modified, or bypassed by posting directly with curl, Postman, or custom HTTP clients. Only server-side `ModelState` validation before persistence enforces rules.

- Never skip `ModelState.IsValid` because "the browser validates."
- Remote validation (`[Remote]`) is also bypassable and subject to async race conditions.
- Client validation improves UX for legitimate users; server validation provides the security boundary.
- API endpoints and SPA clients have no unobtrusive layer at all — server checks are the only enforcement.

---

#### Q8. What is `jquery.validate.unobtrusive.js` responsible for?

**Answer:** It bridges MVC-generated `data-val-*` attributes to jQuery Validate by parsing the DOM, creating rule objects, and attaching validators to forms. It also provides adapters for standard and custom validation attributes.

- Runs at page load (or after manual `parse()` calls) to wire rules without inline scripts.
- Maps annotation metadata to jQuery Validate methods (`required`, `range`, `regex`, `remote`).
- Supports adding custom adapters via `$.validator.unobtrusive.adapters.add` for custom business rules.
- Without this file, `data-val-*` attributes are present but no rules are registered.

---

#### Q9. What is `asp-validation-for` used for?

**Answer:** `asp-validation-for` is a tag helper that renders a `<span>` element for displaying field-level validation messages for a specific model property. It generates `data-valmsg-for` attributes wired to jQuery Validate message placement.

- Shows both client-side and server-side errors for the named property after POST.
- Typically placed adjacent to the corresponding `asp-for` input in the form.
- CSS classes toggle between `field-validation-valid` and `field-validation-error` based on state.
- Required for visible per-field errors when using `asp-validation-summary="ModelOnly"`.

---

#### Q10. What is the difference between `asp-validation-summary="All"` and `"ModelOnly"`?

**Answer:** `"All"` renders property-level and model-level errors in the summary list. `"ModelOnly"` renders only model-level errors added with `ModelState.AddModelError(string.Empty, message)` — property errors appear only in adjacent `asp-validation-for` spans.

- Use `"All"` for a single error banner listing all failures at the top of the form.
- Use `"ModelOnly"` when every field has its own `asp-validation-for` span and the summary is for cross-field or business-rule errors only.
- `"None"` suppresses the summary entirely.
- Choosing `"ModelOnly"` with no per-field spans leaves users seeing an empty summary when only property rules fail.

---

#### Q11. How does `[Remote]` validation work on the client?

**Answer:** `[Remote]` generates a jQuery Validate `remote` rule that sends an AJAX GET or POST to a specified action to check field validity asynchronously — for example, username availability. The server action returns `true` (valid) or `false` (invalid) as JSON.

- Provides immediate UX feedback without a full form POST.
- The remote call is asynchronous — the form may submit before the remote response completes.
- Always re-validate the same rule in the POST action or service; treat `[Remote]` as a hint only.
- Disable submit or show pending state while remote validation is in flight to reduce race conditions.

---

#### Q12. What is a client validation adapter for custom `ValidationAttribute`s?

**Answer:** A client validation adapter implements `IClientModelValidator` and translates a custom server-side `ValidationAttribute` into `data-val-*` attributes plus optional jQuery Validate custom method registration. Without it, custom attributes validate server-side only.

- Implement `AddValidation(ClientModelValidationContext context)` to emit attributes like `data-val-mustbefuturedate`.
- Register the adapter via `ClientModelValidatorProvider` in `AddControllersWithViews` options.
- Add a matching jQuery Validate custom method or unobtrusive adapter that reads the emitted attributes.
- The server `ValidationAttribute` remains the authoritative rule; the client adapter mirrors it for UX.

---

#### Q13. What is `ClientValidationEnabled` on `ViewContext`?

**Answer:** `ClientValidationEnabled` is a boolean flag on `ViewContext` that controls whether tag helpers emit `data-val-*` attributes during view rendering. When false, inputs render without client validation metadata.

- Set it in `_ViewImports.cshtml` or individual views — commonly tied to environment (enabled in Development, optionally disabled in Production).
- Disabling it does not affect server-side `ModelState` validation on POST.
- When false, including `_ValidationScriptsPartial` wastes bandwidth but causes no harm.
- Useful for reducing script payload or avoiding stale client rules during server-side debugging.

---

#### Q14. Why does hand-written HTML input lose client-side validation?

**Answer:** Plain HTML inputs without tag helpers do not receive `data-val-*` attributes because unobtrusive validation is metadata-driven at render time via `IHtmlGenerator`. Without those attributes, jQuery unobtrusive has no rules to attach.

- `<input name="Quantity" type="number" />` binds on POST but validates only server-side if the action checks `ModelState`.
- Use `asp-for="Quantity"` instead of raw HTML, or manually add every required `data-val-*` attribute (error-prone).
- `asp-validation-for` spans also require tag helpers to generate correct `data-valmsg-for` wiring.
- Replacing tag helpers with custom markup is a frequent cause of "server validates but client does not."

---

#### Q15. How do you localize jQuery Validate error messages?

**Answer:** Server-side localization via `.resx` files or `IValidationMetadataProvider` affects `ModelState` messages and optionally `data-val-*-message` on rendered fields. jQuery Validate's built-in method messages (email, number, date) remain English unless you load a localized messages file or override `$.validator.messages`.

- Inspect rendered HTML — if `data-val-required-message` is French, unobtrusive uses it for annotation-backed rules.
- Load `messages_fr.min.js` after `jquery.validate.min.js` and before unobtrusive parses the form.
- Or call `$.extend($.validator.messages, { required: "...", email: "..." })` in a script block.
- Align `RequestLocalization` culture with the validation messages script loaded for the current culture.

---

#### Q16. Why must server actions still check `ModelState.IsValid` when client validation is enabled?

**Answer:** Client validation is optional UX that runs only when browsers execute your scripts correctly. Any HTTP client can POST invalid or malicious payloads directly to the action, bypassing all client rules.

- `ModelState.IsValid` is the enforcement gate before persistence, redirects, or side effects.
- Client and server validation should share the same ViewModel annotations but serve different roles.
- Failing to check `ModelState` while client validation is enabled creates a false sense of security.
- Return `View(model)` or appropriate error responses when server validation fails.

---

#### Q17. Why doesn't client validation fire when using a button click handler instead of form submit?

**Answer:** jQuery unobtrusive validation intercepts the form `submit` event. A `type="button"` click handler that calls `fetch` or `$.post` directly bypasses the validator unless it explicitly calls `$form.valid()` and aborts when validation fails.

- Guard custom AJAX: `if (!$('#form').valid()) return;` before sending the request.
- Or use `type="submit"` and prevent default in a submit handler: `e.preventDefault(); if ($(this).valid()) { ... }`.
- This pattern is common in AJAX partial-form saves where developers avoid full page POST.
- Server-side validation in the action remains mandatory regardless of client guard.

---

#### Q18. What is the difference between MVC unobtrusive validation and SPA/API validation?

**Answer:** MVC unobtrusive validation applies to Razor-rendered HTML forms with jQuery Validate scripts. SPA/API validation runs server-side via `[ApiController]` automatic 400 responses, FluentValidation, or explicit `ModelState` checks — the browser SPA must implement its own client rules separately.

- Shared ViewModel annotations enforce server rules on both paths if each pipeline validates, but unobtrusive scripts never run for JSON API consumers.
- `[ApiController]` returns `ValidationProblemDetails` (400) when model binding/validation fails.
- SPAs typically use libraries like Zod or React Hook Form for UX — duplicated rules, not a substitute for server checks.
- Do not assume annotations on a shared DTO automatically protect every host entry point.

---

## Chapter 15. Real-Time UI with SignalR

#### Q1. What is SignalR and how does it relate to ASP.NET Core MVC?

**Answer:** SignalR is a real-time communication library for ASP.NET Core that enables server-to-client push over WebSockets, Server-Sent Events, or long polling. In MVC apps it complements traditional request/response pages with live notifications, chat, dashboards, and progress updates.

- MVC renders the initial Razor page; JavaScript connects to a SignalR hub for ongoing updates.
- Hubs are mapped alongside MVC routes in `Program.cs` via `app.MapHub<THub>("path")`.
- Controllers broadcast to connected clients through `IHubContext<THub>` after business actions.
- SignalR shares the same host, authentication middleware, and DI container as ASP.NET Core 8 MVC.

---

#### Q2. What is a SignalR Hub?

**Answer:** A Hub is a high-level pipeline class that defines methods callable from clients and uses `Clients`, `Groups`, and `Context` to push messages to connected browsers. It inherits from `Hub` or `Hub<T>`.

- Hub methods are invoked from JavaScript via `connection.invoke("MethodName", args)`.
- Server code calls `await Clients.All.SendAsync("EventName", data)` to push to connections.
- Hubs should be thin transport layers — business logic belongs in injected services.
- Hub instances are transient per invocation, not scoped per connection or user.

---

#### Q3. What is the difference between a Hub and an MVC controller?

**Answer:** Controllers handle HTTP request/response cycles and return action results. Hubs maintain persistent bidirectional connections and push messages asynchronously outside the HTTP request lifecycle.

- Controllers are activated per HTTP request; hubs handle multiple invocations over a long-lived connection.
- Controllers return `IActionResult`; hubs return `Task` and push via `Clients` collections.
- Use controllers for page rendering and form POSTs; use hubs for real-time events to already-connected clients.
- Broadcast from controllers to clients via `IHubContext<THub>`, not by injecting the hub class directly.

---

#### Q4. How do you map a Hub endpoint in `Program.cs`?

**Answer:** Register SignalR in services and map the hub endpoint after routing middleware is configured.

```csharp
builder.Services.AddSignalR();

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/hubs/chat");
app.MapControllerRoute(/* ... */);
app.Run();
```

- Hub path is independent of MVC conventional routes — use a dedicated prefix like `/hubs/`.
- Authentication middleware must run before `MapHub` when hubs require `[Authorize]`.
- For scale-out, add `.AddStackExchangeRedis(...)` or `.AddAzureSignalR(...)` to `AddSignalR()`.
- `UsePathBase` and forwarded headers must be configured before hub mapping for subpath deployments.

---

#### Q5. What is `IHubContext` and why is it used from MVC controllers?

**Answer:** `IHubContext<THub>` is a singleton service registered by SignalR that allows any application code — MVC controllers, background services, or middleware — to send messages to connected clients without being inside a hub method.

- Inject `IHubContext<ChatHub>` into a controller and call `await _hubContext.Clients.All.SendAsync("OrderShipped", id)`.
- It provides the same `Clients`, `Groups`, and `User` targeting APIs as inside a hub.
- Works across the application layer while hubs remain the client-invokable entry point.
- With a Redis backplane or Azure SignalR, messages reach connections on all server instances.

---

#### Q6. Why should you not inject a Hub directly into a controller?

**Answer:** Hub classes are not registered in DI for direct injection — they are instantiated by SignalR per invocation with connection context. Injecting a concrete `Hub` fails activation, produces wrong lifetimes, or gives an instance without active connection state.

- `IHubContext<THub>` is the correct abstraction for sending from controllers and services.
- Hubs hold connection-specific `Context`; a DI-resolved hub instance is not tied to any live connection.
- Do not register `services.AddSingleton<MyHub>()` to work around this anti-pattern.
- Business notifications should flow: controller → service → `IHubContext` → clients.

---

#### Q7. What is a SignalR backplane (e.g., Redis) and when is it needed?

**Answer:** A backplane uses pub/sub (typically Redis) to propagate SignalR messages across all server instances so a send originating on one node reaches clients connected to other nodes.

- Required when running multiple ASP.NET Core instances and events originate on any instance (controller actions, background jobs, other users).
- Register: `builder.Services.AddSignalR().AddStackExchangeRedis(connectionString, options => { options.Configuration.ChannelPrefix = "MyApp:"; });`
- The backplane synchronizes message fan-out — it does not replicate connection state, group membership, or custom in-memory registries.
- Without a backplane, `IHubContext.Clients.Group("room").SendAsync` on instance A misses sockets on instance B.

---

#### Q8. What is the difference between sticky sessions and a SignalR backplane?

**Answer:** Sticky sessions (session affinity) route a user's WebSocket to the same server instance for the connection lifetime. A backplane broadcasts messages to all instances regardless of which node holds the connection.

- Sticky sessions alone work only when all events originate on the same instance as the socket — rare in MVC apps with controller-triggered broadcasts.
- Sticky sessions do not help when a controller on instance B sends to a user connected to instance A.
- Backplane solves cross-instance message routing; sticky sessions solve connection routing without cross-node sends.
- On rolling deploy or reconnect, affinity may change — group membership must be re-established either way.

---

#### Q9. How does cookie authentication apply to SignalR connections?

**Answer:** Cookie authentication used by MVC applies to SignalR when the client sends credentials on the negotiate request and the hub is protected with `[Authorize]`. The auth cookie must be included via same-origin requests or `withCredentials: true` on cross-origin connections with proper CORS.

- Without `[Authorize]` on the hub, connections succeed as anonymous even if the user is logged into MVC pages.
- Client: `.withUrl("/hubs/chat", { withCredentials: true })` for cookie auth.
- For JWT bearer auth, use `accessTokenFactory` on the client because WebSockets cannot always send Authorization headers.
- Middleware order: `UseAuthentication()` before `MapHub`; `Context.User` reflects the authenticated principal inside hub methods.

---

#### Q10. What are SignalR Groups and how do clients join them?

**Answer:** Groups are named collections of connections used to target broadcasts to subsets of clients — chat rooms, document collaborators, or per-tenant channels. Server code adds connections with `await Groups.AddToGroupAsync(Context.ConnectionId, groupName)`.

- Clients join by calling a hub method: `await connection.invoke("JoinRoom", "support-42")` after `connection.start()` completes.
- Alternatively, add to groups in `OnConnectedAsync` based on query string, claims, or user id.
- Send to a group: `await Clients.Group("support-42").SendAsync("ReceiveMessage", message)`.
- Group membership is per connection ID — clients must rejoin after every reconnect.

---

#### Q11. What happens to group membership on SignalR reconnect?

**Answer:** Reconnect assigns a new `ConnectionId` and all previous group memberships are lost. The server must re-add the connection to groups in `OnConnectedAsync` or via a client `JoinRoom` call after `connection.onreconnected`.

- Automatic reconnect restores the transport only — not application group state or connection-scoped dictionaries.
- Client pattern: `connection.onreconnected(() => connection.invoke("JoinRoom", roomName))`.
- Key durable state by user id in database or Redis, not by connection id.
- Push a snapshot to `Clients.Caller` after rejoin so the UI recovers missed updates.

---

#### Q12. What is the negotiate step in SignalR?

**Answer:** Negotiate is the initial HTTP request the SignalR client sends to the hub URL to discover available transports (WebSockets, SSE, long polling), obtain a connection token, and establish authentication before upgrading the connection.

- Occurs before WebSocket upgrade — CORS and authentication must succeed at this HTTP stage.
- Failures here prevent any real-time connection even if WebSocket works in non-browser tools.
- Cross-origin negotiate requires CORS with credentials when using cookie authentication.
- Wrong hub URL (missing `PathBase`, wrong subpath) produces 404 at negotiate — connection never starts.

---

#### Q13. What CORS settings are required for cross-origin SignalR connections?

**Answer:** Cross-origin SignalR requires a CORS policy specifying the exact SPA origin with `.AllowCredentials()` — wildcard origins cannot be used with credentials. Allow required headers and methods for negotiate preflight.

```csharp
builder.Services.AddCors(o => o.AddPolicy("Spa", p =>
    p.WithOrigins("https://app.example.com")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));
```

- Call `app.UseCors("Spa")` before `MapHub`.
- Client: `.withUrl("https://api.example.com/hubs/notifications", { withCredentials: true })` for cookies.
- For JWT bearer, use `accessTokenFactory` instead of credentials.
- Postman bypasses CORS — browser console errors are the diagnostic source.

---

#### Q14. What is Azure SignalR Service and when would you use it?

**Answer:** Azure SignalR Service is a managed service that hosts and scales WebSocket connections separately from your MVC application instances. Your app runs hub logic while Azure handles connection memory and fan-out at scale.

- Register: `builder.Services.AddSignalR().AddAzureSignalR(connectionString)`.
- Use when concurrent connections exceed what app servers can hold, or multi-region deployment is needed.
- `IHubContext<T>` usage in controllers is unchanged — the SDK forwards messages through the service.
- Sticky sessions are not required; connection ownership moves to Azure.
- Trade managed scaling for vendor coupling, SKU limits, and upstream endpoint configuration.

---

#### Q15. What is `HubException` and how should hub errors be returned to clients?

**Answer:** `HubException` is a SignalR-specific exception that sends an error payload to the calling client for recoverable business failures without necessarily terminating the entire connection. Unhandled generic exceptions in hub methods can close the connection.

- For validation errors, prefer `await Clients.Caller.SendAsync("OperationFailed", new { code, message })` and return without throwing.
- Use `throw new HubException("User-friendly message")` for errors the client should display.
- Implement `IHubFilter` for global exception mapping and logging with correlation IDs.
- Never expose stack traces or internal details to clients — log server-side only.

---

#### Q16. How do you invoke hub methods from JavaScript in a Razor view?

**Answer:** Include the `@microsoft/signalr` client library, build a connection to the hub URL, start it, then invoke server methods and register client-side handlers for server pushes.

```html
<script src="~/lib/microsoft/signalr/dist/browser/signalr.min.js"></script>
<script>
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("@Url.Content("~/hubs/comments")", { withCredentials: true })
        .withAutomaticReconnect()
        .build();

    connection.on("NewComment", (comment) => { /* update DOM */ });

    connection.start()
        .then(() => connection.invoke("JoinRoom", roomId))
        .catch(err => console.error(err));
</script>
```

- Always `await connection.start()` (or `.then()`) before any `invoke` call.
- Generate hub URL with `Url.Content` to respect `PathBase` and subpath deployments.
- Match client script version to the server SignalR package version.
- Handle `onreconnected` to rejoin groups and refresh UI state.

---

#### Q17. What are `Clients.All`, `Clients.Caller`, and `Clients.Others`?

**Answer:** These are built-in client targeting collections on `IHubCallerClients` for addressing connected clients without manually tracking connection IDs.

- `Clients.All` — every connected client on the hub (subject to backplane fan-out across instances).
- `Clients.Caller` — only the connection that invoked the current hub method.
- `Clients.Others` — all connections except the caller.
- Also available: `Clients.User(userId)`, `Clients.Group(groupName)`, and `Clients.Client(connectionId)` for precise targeting.

---

#### Q18. How does `PathBase` affect SignalR hub URLs behind a reverse proxy?

**Answer:** When the app is deployed under a subpath (e.g., `/apps/mvc`), `UsePathBase` must be configured and hub URLs must include that prefix. Hard-coded `/hubs/chat` ignores the path base and negotiate returns 404 in staging while working locally at the site root.

- Generate URLs server-side: `@Url.Content("~/hubs/chat")` so the path base is applied automatically.
- Configure `app.UseForwardedHeaders()` and `UsePathBase("/apps/mvc")` before routing and `MapHub`.
- Reverse proxy must forward WebSocket upgrades and negotiate requests to the correct path.
- Misconfigured forwarded headers can also break cookie auth and HTTPS scheme detection.

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
