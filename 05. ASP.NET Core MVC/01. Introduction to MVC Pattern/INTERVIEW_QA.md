# Introduction to MVC Pattern — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is the MVC pattern?](#q1-what-is-the-mvc-pattern)
2. [Q2. What is the role of the Model in ASP.NET Core MVC?](#q2-what-is-the-role-of-the-model-in-aspnet-core-mvc)
3. [Q3. What is the role of the View in ASP.NET Core MVC?](#q3-what-is-the-role-of-the-view-in-aspnet-core-mvc)
4. [Q4. What is the role of the Controller in ASP.NET Core MVC?](#q4-what-is-the-role-of-the-controller-in-aspnet-core-mvc)
5. [Q5. What is the difference between MVC and MVP?](#q5-what-is-the-difference-between-mvc-and-mvp)
6. [Q6. What is the difference between MVC and MVVM?](#q6-what-is-the-difference-between-mvc-and-mvvm)
7. [Q7. What is the difference between ASP.NET Core MVC and Razor Pages?](#q7-what-is-the-difference-between-aspnet-core-mvc-and-razor-pages)
8. [Q8. What is the difference between ASP.NET Core MVC and a SPA + Minimal API approach?](#q8-what-is-the-difference-between-aspnet-core-mvc-and-a-spa-minimal-api-approach)
9. [Q9. What does "thin controller" mean and why is it preferred?](#q9-what-does-thin-controller-mean-and-why-is-it-preferred)
10. [Q10. What is the "fat controller" anti-pattern?](#q10-what-is-the-fat-controller-anti-pattern)
11. [Q11. Where should business logic live in an MVC application?](#q11-where-should-business-logic-live-in-an-mvc-application)
12. [Q12. Where should data access logic live in an MVC application?](#q12-where-should-data-access-logic-live-in-an-mvc-application)
13. [Q13. Where should input validation live in an MVC application?](#q13-where-should-input-validation-live-in-an-mvc-application)
14. [Q14. How does a request flow through Model, View, and Controller?](#q14-how-does-a-request-flow-through-model-view-and-controller)
15. [Q15. What is the difference between a domain entity and a ViewModel?](#q15-what-is-the-difference-between-a-domain-entity-and-a-viewmodel)
16. [Q16. What symptoms appear when business logic is placed in the View?](#q16-what-symptoms-appear-when-business-logic-is-placed-in-the-view)
17. [Q17. What symptoms appear when data access is placed in the Controller?](#q17-what-symptoms-appear-when-data-access-is-placed-in-the-controller)
18. [Q18. What is Post-Redirect-Get (PRG) and why is it used in MVC?](#q18-what-is-post-redirect-get-prg-and-why-is-it-used-in-mvc)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is the MVC pattern?

**Concepts**
- Model — data and domain logic
- View — passive presentation markup
- Controller — HTTP adapter orchestrating Model and View
- Separation enabling independent testability of each layer

**Answer**

MVC (Model–View–Controller) separates an application into three cooperating parts: the Model holds data and domain logic, the View renders presentation, and the Controller handles input and orchestrates the flow between Model and View. The reason this separation matters is that UI changes stay independent from business logic, and each layer can be tested in isolation. In ASP.NET Core MVC, HTTP requests map to controller actions that prepare models and select Razor views for the response. The Controller receives the request, invokes services or repositories, and decides which view to render or whether to redirect. The View is passive markup that displays data the controller prepared — it does not own business rules.

---

## Q2. What is the role of the Model in ASP.NET Core MVC?

**Concepts**
- View models — shaped exactly for the form or page
- Domain entities — business objects accessed through services
- `ModelState` — validation results from binding and annotation checks
- Strongly typed `@model` — compile-time safety over `ViewBag`

**Answer**

In ASP.NET Core MVC, "Model" refers to the data and state an action passes to a view or receives from model binding — including domain entities, view models, DTOs, and `ModelState`. It is not a single class but the data contract between controller, binding, validation, and view. View models shape exactly what a form or page needs, excluding sensitive or irrelevant database fields, while domain entities represent business objects and invariants typically accessed through application services rather than directly in views. `ModelState` carries validation results from Data Annotations, FluentValidation, or manual errors after binding. The model should not reference HTTP concerns like `HttpContext` or Razor-specific types, and strongly typed models with `@model` give compile-time safety compared to `ViewBag` or `ViewData`.

---

## Q3. What is the role of the View in ASP.NET Core MVC?

**Concepts**
- Razor `.cshtml` — HTML combined with C# expressions and tag helpers
- Strongly typed `@model` vs `ViewData`/`ViewBag`
- Layouts, sections, and partial views for page structure
- Razor precompilation — syntax errors caught at build time

**Answer**

The View is the presentation layer — Razor `.cshtml` files that render HTML from a model the controller supplies. Views should format and display data and not perform business logic, data access, or authorization decisions, since placing those concerns in Razor makes them untestable and creates divergence across code paths. Razor combines HTML with C# expressions, tag helpers, and partial views for reusable markup fragments. Views receive data through a strongly typed `@model` or less preferably through `ViewData` and `ViewBag`. Layouts, sections, and partial views compose the final page structure, and views are compiled at publish time by default in ASP.NET Core 8 with optional runtime compilation in Development.

---

## Q4. What is the role of the Controller in ASP.NET Core MVC?

**Concepts**
- Controller — HTTP adapter, maps URLs and verbs to action methods
- `IActionResult` — view, redirect, JSON, or HTTP error results
- Thin controller — delegates to application services
- `ModelState.IsValid` — gates persistence after binding

**Answer**

The Controller is the HTTP adapter that maps URLs and verbs to action methods, binds input, calls application services, and selects the response — a view, redirect, or status result. It orchestrates the request but should not own business rules or data access, because doing so makes it impossible to reuse the same logic from an API endpoint or background job. Controllers inherit from `Controller` for MVC with views or `ControllerBase` for API-only, and actions return `IActionResult` or `Task<IActionResult>` to produce views, redirects, JSON, or HTTP error codes. Constructor injection supplies scoped services, and controllers check `ModelState.IsValid`, enforce authorization via attributes or filters, and map service results to HTTP responses.

---

## Q5. What is the difference between MVC and MVP?

**Concepts**
- MVC — controller drives flow, pushes data to passive View
- MVP — Presenter mediates everything, View raises events
- MVP common in WinForms/desktop — event-driven, synchronous
- Presenter-like services in MVC — pattern adopted manually, not wired by framework

**Answer**

In MVC, the Controller drives the flow and pushes data to a passive View. In MVP (Model–View–Presenter), a Presenter mediates all interaction — the View is dumb, raises events, and the Presenter updates both the Model and the View. MVC fits request-response web apps where the server controls page flow per HTTP request, while MVP is common in desktop UI such as WinForms where the view raises events and the presenter handles them synchronously. ASP.NET Core MVC implements Controller-driven flow — the framework does not wire Presenters automatically. Teams sometimes extract presenter-like service classes to slim controllers, but routing still reaches controller actions. Confusing the two leads to putting orchestration logic inside Razor views instead of controllers or services.

---

## Q6. What is the difference between MVC and MVVM?

**Concepts**
- MVVM — ViewModel with bindable properties, two-way declarative binding
- MVC — request-driven, server pushes model to passive View
- XAML stacks (WPF, MAUI) — MVVM native
- Razor view models — one-way snapshots, not full MVVM without JS binding

**Answer**

MVVM (Model–View–ViewModel) uses a ViewModel with bindable properties and commands — the View binds declaratively and changes propagate two-way. MVC uses a Controller to handle requests and push a model into a largely passive View. MVVM dominates XAML stacks such as WPF and MAUI and client-side SPA frameworks with observable state. ASP.NET Core MVC view models share the name but are typically one-way snapshots for server-rendered forms, not full MVVM unless client-side binding is added. In MVC, each HTTP request is stateless so there is no ongoing two-way binding unless JavaScript frameworks provide it. Placing domain rules in a view model's methods blurs MVVM presentation state with business logic, which is a common layering mistake when developers carry MVVM habits into server-rendered MVC.

---

## Q7. What is the difference between ASP.NET Core MVC and Razor Pages?

**Concepts**
- MVC — controllers and actions with `{controller}/{action}` routing
- Razor Pages — page-centric routing with colocated PageModel
- MVC strength — multiple controllers, areas, shared filters
- Razor Pages strength — form-focused workflows with less ceremony

**Answer**

Both use Razor for server-rendered HTML, but MVC organizes code around controllers and actions with `{controller}/{action}` routing, while Razor Pages colocates a PageModel with each `.cshtml` using page-centric routing. MVC suits apps with many controllers, areas, shared filters, and REST-style URL conventions across resources, since the controller-action model fits cases where multiple actions share common behavior via filters. Razor Pages suit page-focused workflows such as forms, wizards, and admin CRUD with less ceremony per page since you get one PageModel per page rather than one controller per resource. Both support tag helpers, validation, authorization, and the same underlying Razor view engine, and the choice is organizational since the rendering pipeline and DI model are shared in ASP.NET Core 8.

---

## Q8. What is the difference between ASP.NET Core MVC and a SPA + Minimal API approach?

**Concepts**
- MVC — server-rendered HTML, full-page loads, cookie-based auth
- SPA + Minimal API — JSON endpoints, browser-side rendering, JWT auth
- MVC strengths — form-heavy, SEO-sensitive, simpler deployment
- SPA strengths — rich interactivity, multiple clients sharing one API

**Answer**

ASP.NET Core MVC renders HTML on the server and uses full page loads or partial updates, while a SPA + Minimal API serves JSON from API endpoints and renders UI entirely in the browser with JavaScript. MVC keeps validation, routing, and auth integrated in one host with cookie-based sessions and form posts, making it a natural fit for form-heavy, SEO-sensitive, or server-rendered admin apps with simpler deployment. SPA + Minimal API splits backend (JSON endpoints) and frontend (React, Angular, etc.), often using JWT or token auth, and fits rich client interactivity, offline-like UX, or multiple clients sharing one API. Hybrid approaches with MVC and AJAX partials exist but increase complexity compared to picking one primary model.

---

## Q9. What does "thin controller" mean and why is it preferred?

**Concepts**
- Thin controller — HTTP concerns only: bind, validate, call service, return result
- Business logic in application or domain services — reusable from multiple entry points
- Data access behind repository interfaces — injected into services
- Unit tests mock service interfaces — no database or HTTP context needed

**Answer**

A thin controller limits itself to HTTP concerns — binding input, calling services, checking `ModelState`, and returning views or redirects — without embedding business rules, SQL, or email logic. The reason this matters is that business logic in a controller cannot be reused when the same use case is exposed via a web form, an API endpoint, or a background worker. Business logic moves to application or domain services, data access hides behind repositories or `DbContext` interfaces injected into services, and unit tests mock service interfaces rather than spinning up databases or HTTP contexts. The controller becomes a stable adapter even when business rules change frequently, since the change is isolated to the service layer.

---

## Q10. What is the "fat controller" anti-pattern?

**Concepts**
- Fat controller — data access, validation, HTTP calls, and side effects in action methods
- Untestable without full stack integration tests
- Business rules duplicated across controller actions — not shareable
- EF entities passed to views — couples UI to database schema

**Answer**

A fat controller stuffs data access, validation rules, mapping, external HTTP calls, and email sending directly into action methods — often hundreds of lines per action. The core problem is that actions become untestable without full stack integration tests and real databases, since every dependency is created inline rather than injected. Business rules duplicated in controllers cannot be shared with API projects or batch jobs, so the same logic drifts over time. Direct `DbContext`, ADO.NET, or `new HttpClient()` usage in actions causes lifetime and scalability issues, and refactoring requires touching controller code for every business change instead of isolated services. Fat controllers often pass EF entities straight to views, coupling the UI to the database schema and exposing over-posting vulnerabilities.

---

## Q11. Where should business logic live in an MVC application?

**Concepts**
- Domain entities — core invariants such as discount eligibility
- Application services — use-case orchestration, transactions, domain events
- Controllers — HTTP translation only, no business computation
- Single source of truth — MVC, API, and batch jobs enforce same rules

**Answer**

Business logic belongs in domain entities for core invariants and application services for use-case orchestration — not in controllers, views, or Razor PageModels beyond coordination. Domain rules like discount eligibility, stock checks, or pricing policies live in domain methods or dedicated policy classes, while application services coordinate transactions, call repositories, and publish domain events across a use case. Controllers translate HTTP input into service commands and map outcomes to views or redirects. Views display computed results from view models and do not recalculate business values. Centralizing rules ensures MVC forms, REST APIs, and background jobs enforce the same behavior, which is the single most important reason to keep business logic out of controllers and views.

---

## Q12. Where should data access logic live in an MVC application?

**Concepts**
- Repositories or query services — encapsulate queries and persistence
- `DbContext` scoped per request — injected into repositories or services
- Views must never query databases — not even via `@inject AppDbContext`
- Separation enables unit tests with fakes

**Answer**

Data access belongs in repositories, query services, or EF Core `DbContext` usage behind interfaces registered in DI — not in controllers or views. Controllers and services depend on abstractions like `IOrderRepository` rather than raw SQL so the data layer can be replaced or mocked. EF Core `DbContext` is registered scoped per request and injected into repositories or services, not controllers directly. Controllers must never open connections or call `SaveChanges` in well-structured apps, and views must never query databases — not even via `@inject AppDbContext` since that bypasses service-layer testing and creates N+1 query risks. Separating data access enables unit tests with fakes and integration tests focused on the repository layer.

---

## Q13. Where should input validation live in an MVC application?

**Concepts**
- Data Annotations on view models — server-side checks and client-side `data-val-*`
- Business validation in services — added to `ModelState` manually
- `ModelState.IsValid` gates persistence in the action
- Client-side validation — UX only, bypassable, not a security boundary

**Answer**

Input validation — required fields, string length, format, range — belongs on view models or input DTOs using Data Annotations, FluentValidation, or `IValidatableObject`. Server-side validation always runs in the action or a filter before any persist operation because client-side validation is bypassable by direct POST. Data Annotations on view models generate both server-side checks and client-side `data-val-*` attributes for unobtrusive JavaScript. Business validation such as "credit limit exceeded" belongs in services and adds errors to `ModelState` manually after the binding check. Putting validation on EF entities shared with MVC risks wrong-layer coupling and API inconsistencies. `ModelState.IsValid` in the action gates the call to persistence services.

---

## Q14. How does a request flow through Model, View, and Controller?

**Concepts**
- Routing — maps URL and verb to controller action
- Model binding — populates parameters from form, route, query; runs annotation validation
- Controller invokes services — receives domain results or view models
- `View(model)` or `RedirectToAction` — on success; `View(model)` with errors on failure

**Answer**

Kestrel receives the HTTP request, middleware runs for routing and auth, routing selects a controller action, model binding constructs input models from form fields, route values, and query strings, and validation runs during binding. The controller calls application services with bound input and receives domain results or view models. On success the action returns `View(model)` or `RedirectToAction`; on validation failure it returns the form view with `ModelState` errors. The view engine locates the `.cshtml` file, applies `_ViewStart` layout, and renders HTML sent back through the middleware pipeline. The key invariant is that business logic must not run in the view engine — the model passed to the view should already contain all computed values.

---

## Q15. What is the difference between a domain entity and a ViewModel?

**Concepts**
- Domain entity — persistence identity, relationships, invariants
- ViewModel — shaped for one view or form, display fields and validation only
- Over-posting prevention — ViewModels expose only editable fields
- Mapping at boundary — controller or dedicated mapper between layers

**Answer**

A domain entity models business concepts and persistence with identity, relationships, and invariants tied to the database. A ViewModel shapes data specifically for one view or form — including display fields, validation attributes, and no sensitive or unnecessary properties. Entities may have navigation properties, concurrency tokens, and internal fields not meant for UI binding, which is why passing entities directly to Razor couples views to schema changes and exposes hidden fields to mass assignment. ViewModels prevent over-posting by exposing only editable fields on create and edit forms. Entities belong in the domain and data layer while view models belong in the presentation layer, and mapping happens in the controller or a dedicated mapper at the boundary.

---

## Q16. What symptoms appear when business logic is placed in the View?

**Concepts**
- Untested calculations in `@{}` blocks — no unit test coverage
- Duplicated rules across pages — diverge from API and batch logic
- Authorization checks in Razor — bypassable via alternate endpoints

**Answer**

Business logic in Razor views produces untested calculations, duplicated rules across pages, and inconsistencies between UI, API, PDF, and email outputs. Pricing, tax, or discount math in `@{}` blocks bypasses unit tests on services — changes require hunting `.cshtml` files instead of updating a single service. Authorization checks in views can be skipped by crafting requests or using alternate endpoints since the view is never the security boundary. Magic numbers and policy thresholds scattered in markup drift from domain rules over time, and performance issues appear when views perform per-row calculations or service calls inside loops. Production bugs surface where the view shows one value but the database or API stores another.

---

## Q17. What symptoms appear when data access is placed in the Controller?

**Concepts**
- Fat actions with LINQ or raw SQL — untestable without real database
- Duplicated query logic across controller actions
- Ad-hoc transaction and connection management — N+1 and retry gaps

**Answer**

Data access in controllers creates thick actions with raw SQL or LINQ, makes unit testing require a real database, and mixes HTTP handling with query logic. Actions grow with `DbContext` queries, `SaveChanges`, and manual mapping inline, so controller tests become integration tests needing `WebApplicationFactory` or SQL Server. The same query logic gets duplicated when adding API endpoints or background jobs, and connection management, retry, and transaction boundaries become ad hoc rather than centralized. Security risks increase when SQL or EF queries embedded in controllers lack consistent authorization checks, because each action author must remember to add the guard independently.

---

## Q18. What is Post-Redirect-Get (PRG) and why is it used in MVC?

**Concepts**
- PRG — successful POST redirects to GET, preventing browser refresh resubmission
- `RedirectToAction` after valid POST — standard create/update/delete pattern
- Validation failures — `return View(model)` without redirect keeps `ModelState` errors
- `TempData` — carries flash messages across the redirect

**Answer**

Post-Redirect-Get completes a successful POST by returning an HTTP redirect to a GET action instead of rendering the view directly. The reason this matters is that the browser's refresh on the GET URL is safe — it does not resubmit the form body — whereas refreshing after a direct `return View()` resubmits the POST and can create duplicate records or payments. After processing a valid POST, the action returns `RedirectToAction("Details", new { id })` rather than `return View()`. Validation failures typically redisplay the form with `return View(model)` without redirect so `ModelState` errors remain accessible. `TempData` can carry flash messages across the redirect when a success notification is needed on the GET page.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Concepts**
- Pricing and discount calculations in `.cshtml` — no unit test coverage
- Authorization checks in Razor — bypassable by alternate routes
- Rules diverge from API and batch logic over time

**Answer**

Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently. Views should render data the controller or ViewModel already prepared — not compute it. Authorization belongs in filters, policies, or controller and service checks before the view executes. Calculations in Razor cannot be tested independently and often diverge from API or batch logic. Razor should be limited to presentation formatting, not business decisions.

---

#### Gotcha 2. EF entities passed directly to views

**Concepts**
- Lazy-loaded navigations — unexpected queries during rendering
- Over-posting — mass assignment via unlocked navigation properties
- ViewModels — expose only the fields the view needs

**Answer**

Binding and displaying EF Core entities exposes navigation properties, causes over-posting on POST, and couples the UI to the database schema. Lazy-loaded navigations can trigger unexpected queries during rendering and mass assignment can update properties the user should not control such as `IsAdmin`. The fix is to use dedicated ViewModels with only the fields the view needs and to map between entities and ViewModels in the controller or a mapping service.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Concepts**
- Browser forms — `application/x-www-form-urlencoded`, not JSON
- `[FromBody]` uses JSON input formatter — leaves model empty
- Silent binding failure — action runs with default values

**Answer**

Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model empty while the action runs with default values. The fix is to remove `[FromBody]` for conventional form POSTs and let model binding read form fields — use `[FromBody]` only when the client sends JSON with the correct Content-Type. Silent binding failure is a common source of "my POST action receives null model" bugs.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Concepts**
- Client-side validation — bypassable by direct POST
- Server-side validation — mandatory before persist or side effect
- Missing server check — treat as security defect

**Answer**

Client-side validation is bypassable — attackers POST directly without browser scripts. Server-side validation is mandatory before any persist, redirect, or side effect. I always gate POST actions with `if (!ModelState.IsValid) return View(model);` or equivalent. Client validation improves UX for legitimate users only and must never be treated as a security boundary.

---

#### Gotcha 5. `return View()` after successful POST

**Concepts**
- `return View()` after POST — browser refresh resubmits the POST body
- PRG — `return RedirectToAction(nameof(Index))` after successful mutation
- `TempData` — flash success messages across the redirect

**Answer**

Returning the same view after a successful POST causes duplicate submission when the user refreshes the page because the browser resubmits the POST body. The fix is Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create or update separates the mutation from the display. Flash success messages go via TempData on the redirect target.

---

#### Gotcha 6. `ModelState` after redirect

**Concepts**
- `ModelState` — request-scoped, does not survive redirect
- On failure — `return View(model)` with errors inline
- Cross-redirect errors — serialize to TempData or re-validate on GET

**Answer**

`ModelState` is request-scoped and does not survive `RedirectToAction`. The common pattern is to redirect only on success and on validation failure return `View(model)` with errors inline so `ModelState` remains accessible. To survive redirect on failure, serialize errors to TempData or use PRG with a form-specific error cache. AJAX partial forms avoid redirect and can return the form partial with `ModelState` errors directly.

---

#### Gotcha 7. TempData read twice in layout and view

**Concepts**
- TempData consumed on first read by default
- `TempData.Peek` — read without consuming
- `TempData.Keep` — mark for second read

**Answer**

TempData is consumed on first read by default. If the layout reads a flash message, the view sees nothing unless `Peek()` or `Keep()` is used. I use `TempData.Peek("Message")` in the layout to read without consuming, or call `TempData.Keep("Message")` after the layout read so the view can also read it. I prefer a single consumption point — typically the layout or a dedicated partial, not both. Cookie-based TempData has size limits so large payloads should not be stored there.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Concepts**
- `[Area("AreaName")]` — required for area route discovery
- Area routing registered with `{area:exists}` constraint
- Without attribute — MVC treats controller as root controller

**Answer**

Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong conventional route. Every area controller must declare `[Area("AreaName")]` matching its folder. Area routing is registered separately in `Program.cs` with the `{area:exists}` constraint, and specific area routes must come before catch-all default routes.

---

#### Gotcha 9. Link generation without `asp-area`

**Concepts**
- Tag Helpers — default to current area context
- Cross-area links — require explicit `asp-area` and `asp-controller`
- `Url.Action` — pass `new { area = "Admin" }` in route values

**Answer**

Tag Helpers default to the current area context when generating URLs. Links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment. From within an area, omitting `asp-area` keeps links inside the current area which can be incorrect. Cross-area links require both `asp-area` and `asp-controller` plus `asp-action`. The same rule applies to `Url.Action` — pass `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Concepts**
- Unchecked checkbox posts nothing — model binding sets `bool` to `false`
- `[Required]` on `bool` never fails — `false` is a valid non-null value
- `bool?` with `[Required]` — forces explicit consent selection

**Answer**

A missing unchecked checkbox posts nothing and model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid value — not null or empty. I use `bool?` with `[Required]` to require an explicit true selection for consent checkboxes, or the hidden-field pattern: a hidden input posting `false` plus a checkbox posting `true` so unchecked still posts `false` deliberately with known intent.

---

#### Gotcha 11. Collection binding with gap indices

**Concepts**
- Model binder expects contiguous zero-based indices
- Gap indices — index 1 missing causes truncation or misalignment
- Reindex client-side after row deletion

**Answer**

Deleting a row from a dynamic form leaving indices such as `Lines[0]` and `Lines[2]` breaks model binder alignment — index 1 is missing and subsequent items may bind incorrectly or truncate. I reindex client-side after row deletion so indices are contiguous starting at zero, or implement a custom `IModelBinder` that tolerates non-contiguous indices. Partial views rendering collection editors must maintain consistent index naming.

---

#### Gotcha 12. `@Html.Raw` with user content

**Concepts**
- Default Razor `@` — HTML-encodes output, prevents XSS
- `@Html.Raw` — bypasses encoding, executes injected script
- Allowlist sanitizer before Raw for trusted rich text

**Answer**

Default Razor encoding prevents XSS by HTML-encoding output. `@Html.Raw(Model.UserComment)` renders attacker-supplied script if the content is not sanitized server-side. I prefer `@Model.UserComment` (auto-encoded) for plain text or sanitize with a trusted HTML sanitizer library before using Raw on the sanitized output. Content-Security-Policy limits blast radius but does not replace encoding.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Concepts**
- Form tag helpers emit antiforgery token automatically
- `fetch` / jQuery AJAX — must send token manually in header or form field
- `[AutoValidateAntiforgeryToken]` — validates all unsafe verb methods

**Answer**

Form tag helpers emit antiforgery tokens automatically, but `fetch` and jQuery AJAX must manually send `RequestVerificationToken` as a header or `__RequestVerificationToken` form field or POSTs fail with 400 antiforgery errors. I read the hidden field value from the page and include it on every mutating AJAX request. `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe methods and missing tokens fail before the action runs. I do not disable antiforgery on MVC cookie-auth endpoints to "fix" AJAX — I add the token instead.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Concepts**
- Hubs are not registered in DI for direct injection
- `IHubContext<THub>` — singleton proxy for broadcasting from controllers
- Redis backplane or Azure SignalR for multi-instance fan-out

**Answer**

Hubs are not registered in DI for direct injection into controllers. Injecting a concrete `Hub` fails activation or produces an instance without connection context. The correct pattern is to inject `IHubContext<THub>` which is a singleton proxy registered by `AddSignalR()` and use it to broadcast messages from controllers, services, or background jobs.

---

#### Gotcha 15. SignalR scale-out without backplane

**Concepts**
- Sticky sessions — per-client affinity, not cross-instance event routing
- Redis backplane or Azure SignalR — required for multi-instance fan-out

**Answer**

Sticky sessions alone do not fan-out events across server instances. A controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users connected to instance B without a backplane. Multi-node deployments need `AddStackExchangeRedis` or `AddAzureSignalR` so messages sent from any instance reach clients on all instances.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A team migrating from WebForms ports this "controller." It compiles and renders in dev. What architectural problems appear at scale, and how should responsibilities move in ASP.NET Core MVC?

```csharp
public class CustomerPageController : Controller
{
    public IActionResult Index()
    {
        if (!IsPostBack()) // extension from old WebForms helper
            return View();

        var id = int.Parse(Request.Form["CustomerId"]);
        using var conn = new SqlConnection(Configuration["DefaultConnection"]);
        conn.Open();
        var cmd = new SqlCommand("SELECT * FROM Customers WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        var reader = cmd.ExecuteReader();
        var model = new Customer();
        if (reader.Read())
        {
            model.Name = reader["Name"].ToString();
            model.Email = reader["Email"].ToString();
            if (model.Email.Contains("@") == false)
                ModelState.AddModelError("", "Invalid email");
            else
                SendWelcomeEmail(model.Email); // SMTP call inline
        }
        ViewBag.Message = "Loaded";
        return View(model);
    }
}
```

**Concepts**
- `IsPostBack()` — WebForms page lifecycle in MVC action
- Raw ADO.NET in controller — untestable, no DI, connection leak risk
- Business validation and `SendWelcomeEmail` inline — fat controller
- `ViewBag.Message` — untyped view contract

**Answer**

This controller recreates WebForms page lifecycle inside MVC — mixing transport detection, ADO.NET data access, validation, and side effects in one action, which blocks async I/O, testability, and clear separation of concerns.

The `IsPostBack()` extension is a WebForms concept that fights RESTful action design — MVC uses separate `[HttpGet]` and `[HttpPost]` actions, not a branching `Page_Load` method. The raw `SqlConnection` and `SqlCommand` in the controller action are untestable without a real SQL Server, bypass DI, and risk connection leaks if refactored poorly. Email sending inline in the controller means the same logic cannot be reused from an API or background job, and `ViewBag.Message` is an untyped view contract with magic strings. Synchronous ADO.NET on the default thread pool blocks threads under concurrent load.

The fixes in priority order: split into explicit actions — `GET Index` to show the form and `POST Index` with `[FromForm]` model binding, dropping `IsPostBack`; inject `ICustomerRepository` or `AppDbContext` (scoped) and move SQL to the repository or service layer; move email to `ICustomerNotificationService` and validate with Data Annotations or FluentValidation on a view model; return a strongly typed `CustomerDetailsViewModel` to the view rather than an entity plus `ViewBag`.

---

#### Q2. (D) A desktop WPF team and a web team both say they use "MV-something." Compare **MVC**, **MVP**, and **MVVM** for ASP.NET Core MVC — which pattern does the framework implement, where do the others still appear, and what confusion causes production bugs?

**Concepts**
- MVC — controller drives HTTP flow, view is passive server-rendered markup
- MVP — Presenter mediates, View raises events (WinForms, not ASP.NET Core)
- MVVM — two-way bindable ViewModel (WPF, MAUI, Blazor in spirit)
- Razor view models — one-way snapshots, not full MVVM without JS binding

**Answer**

ASP.NET Core MVC implements Model–View–Controller for server-rendered web: the controller orchestrates HTTP, the model carries data and validation state, and the view renders HTML. MVP and MVVM appear elsewhere in the .NET ecosystem and only partially overlap with what the ASP.NET Core framework wires.

In MVC, the controller receives a request, invokes services, selects a view with a model, and the view is passive markup driven by the controller. In MVP, the Presenter mediates — the view is dumb and raises events while the Presenter handles them. This pattern appears in WinForms and older desktop apps but not in ASP.NET Core directly, though teams sometimes extract presenter-like classes to slim controllers. In MVVM, the ViewModel exposes bindable state and commands and the view binds declaratively, which is native to WPF, MAUI, and Blazor components. Razor view models share the name but are one-way snapshots per request, not observable MVVM unless a client-side binding framework is added.

The confusion that produces bugs: treating Razor like XAML so business rules end up in the ViewModel that belongs in domain services; putting presenter-style orchestration in views via `@{}` code blocks; expecting two-way binding semantics on server-rendered forms without JavaScript. ASP.NET Core is request-driven — the controller pushes data to the view, and there is no ongoing binding between requests.

---

#### Q3. (R) Review this order checkout action promoted to production. Tests pass with an in-memory database. What breaks under load or when requirements change?

```csharp
public class OrdersController : Controller
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Checkout(int cartId)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstAsync(c => c.Id == cartId);
        if (cart.Items.Count == 0)
            return RedirectToAction("Index");

        decimal total = 0;
        foreach (var item in cart.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product.Stock < item.Qty)
                return View("Error", $"Out of stock: {product.Name}");
            product.Stock -= item.Qty;
            total += product.Price * item.Qty;
        }

        var order = new Order { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier), Total = total };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var client = new HttpClient();
        await client.PostAsJsonAsync("https://payments.internal/charge", new { order.Id, total });

        return RedirectToAction("Confirm", new { order.Id });
    }
}
```

**Concepts**
- Fat controller — checkout orchestration, stock mutation, pricing all inline
- N+1 queries — `FindAsync` per cart item inside a loop
- No transaction or concurrency token — overselling under concurrent checkouts
- `new HttpClient()` per request — socket exhaustion
- IDOR — no authorization that `cartId` belongs to current user

**Answer**

The action has five problems that pass in-memory tests but fail under real concurrency or security review.

The checkout orchestration, inventory rules, pricing, and payment HTTP call are all in one controller action, making it impossible to reuse the same flow from an API endpoint or to unit test any part without the full stack. The `FindAsync` inside a loop is an N+1 — 50 cart items means 50 database round trips, causing latency spikes under concurrent load. Stock decrements happen without a transaction or concurrency token, so two simultaneous checkouts for the last unit will both read `Stock = 1`, both decrement, and both save, overselling the product. `new HttpClient()` is created per request, causing socket exhaustion and stale DNS handlers under traffic. There is no check that `cart.UserId == currentUserId`, so any authenticated user can check out any cart by guessing `cartId` — a classic IDOR vulnerability.

The fixes in priority order: extract `ICheckoutService.CheckoutAsync(cartId, userId, ct)` so the controller only validates auth, calls the service, and maps the result to a redirect or error view; wrap stock and order creation in a transaction with row versioning or `UPDATE ... WHERE Stock >= @qty`; replace `new HttpClient()` with a typed `HttpClient` via `IHttpClientFactory` or an `IPaymentGateway` abstraction; verify `cart.UserId == currentUserId` before any mutation.

---

#### Q4. (P) Where should **input validation**, **business rules**, and **data access** live in a well-structured ASP.NET Core MVC app — and what symptoms appear when each is placed in the wrong layer?

**Concepts**
- Input validation on view models — `[Required]`, `[EmailAddress]`, FluentValidation
- Business rules in domain or application services — not controllers or views
- Data access in repositories behind interfaces — not views or actions
- Wrong-layer symptoms: duplication, security bugs, integration-test-only testability

**Answer**

Input validation belongs on view models and DTOs via Data Annotations, FluentValidation, or `IValidatableObject`. Business rules belong in domain entities or application services. Data access belongs in repositories or `DbContext` behind an interface. Controllers coordinate by authorizing, binding input, calling services, and choosing the view or redirect. Views render model state and display validation summaries only.

When validation lives in Razor, rules are duplicated in the API endpoint and untestable without the view engine, and a crafted POST bypasses them. When business rules live in the controller, the same pricing or stock logic gets copied to every endpoint that needs it, and 400-line action methods are the result. When data access lives in the view via `@inject AppDbContext`, SQL appears in view render traces, lazy navigation loads trigger unexpectedly, and the view is impossible to test without a live database. When a rule appears in two places — an MVC form and a JSON API — it should be lifted to a service that both call, since divergence causes silent correctness bugs that only manifest in one entry point.

---

#### Q5. (R) This refactor was sold as "thin controllers + testability." Unit tests on `ProductsControllerTests` still require `TestServer` and a real database. Diagnose the layering mistake.

```csharp
public class ProductsController : Controller
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q));

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        ViewBag.SearchTerm = q;
        ViewBag.ResultCount = products.Count;
        return View(products);
    }
}

// ProductsControllerTests — "unit" test
[Fact]
public async Task Index_filters_by_search_term()
{
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var response = await client.GetAsync("/Products?q=widget");
    response.EnsureSuccessStatusCode();
}
```

**Concepts**
- LINQ in controller — no seam for mocking product queries
- No `IProductQueryService` interface — forces integration tests
- `ViewBag` for search metadata — untyped, brittle layout tests
- Entity list returned to view — leakage if navigation properties added

**Answer**

The controller still owns the EF LINQ query, sorting, and filtering, so any meaningful test must spin up HTTP and a real database. "Thin controller" is a misnomer here — the action has data access and orchestration inlined, just shorter than before. The missing step is extracting a service with an interface.

The specific problem is that `AppDbContext` is injected directly and LINQ runs inside the action, which means there is no seam to inject a fake product list. The test is forced to use `WebApplicationFactory` with a real database because the controller cannot be constructed with a mock. `ViewBag` for search term and result count is an untyped view contract that breaks silently on typos. Returning a raw entity list to the view risks leakage if navigation properties are added to `Product` later.

The fixes in priority order: introduce `IProductQueryService.SearchAsync(string? term, ct)` returning `IReadOnlyList<ProductListItemDto>`; the controller calls the service and builds a `ProductIndexViewModel` (search term, items, count); unit tests construct the controller with a mocked `IProductQueryService` and assert the `ViewResult` model without `WebApplicationFactory`; keep one or two integration tests for routing and binding, not every filter scenario.

---

#### Q6. (D) Product asks for a **server-rendered admin CRUD** app. When do you choose **MVC**, **Razor Pages**, or **Minimal APIs + SPA**, and what would make you reverse the decision after six months?

**Concepts**
- MVC — conventional routing, areas, shared filters, disparate controller resources
- Razor Pages — page-centric, colocated PageModel, form-focused CRUD
- Minimal API + SPA — rich client state, multiple consumers, non-browser clients
- Reversal signals — AJAX everywhere fighting server-render, or real-time requirements

**Answer**

For form-heavy, server-rendered admin CRUD with roles, Razor Pages or MVC are the natural fit. Minimal APIs + SPA pays off for rich client interactivity but adds operational and contract overhead — the choice should be based on UX complexity and team skills, not default to SPA.

I choose MVC when I want conventional `{Controller}/{Action}` URLs, shared filters across disparate resources, and teams already organized around controllers with multiple related controllers, areas, and complex attribute routing. I choose Razor Pages when each page is a cohesive form or list, page-centric routing is natural, and admin CRUD maps one-to-one to pages — less ceremony than a controller per resource for simple flows. I choose Minimal APIs + SPA when I need rich client state, offline-like UX, or mobile clients sharing the same backend — not when the requirement is mostly POST-redirect-GET forms with server validation messages.

I would reverse from SPA to MVC or Razor Pages if validation is duplicated client and server, auth cookie plus API token complexity is hurting the team, deployment doubled a build pipeline without real benefit, or the team is slower on front-end than C#. I would reverse from Razor to SPA if heavy partial-page AJAX is fighting the model everywhere, the feature requires real-time dashboards, or an API must serve non-browser clients anyway and investing in the API layer makes sense. Role-based pages with `[Authorize(Roles = "Admin")]` work equally well in MVC and Razor Pages and are not a differentiator.

---

#### Q7. (M) A browser submits `POST /Orders/Create` with form fields bound to `CreateOrderViewModel`. Trace responsibilities through **Model**, **View**, and **Controller** from first byte received through HTML response.

**Concepts**
- Model binding — input shaping from form values, not domain validation
- Controller — `ModelState.IsValid` gate, then service call only
- Domain/service layer — business rules, persistence, no HTTP dependency
- View — renders form, validation tag helpers, no `SaveChanges` or pricing logic

**Answer**

Kestrel and middleware build `HttpContext`, routing selects `OrdersController.Create(CreateOrderViewModel vm)`, and model binding populates `vm` from form fields. `ModelState` records validation errors from Data Annotations during binding — this is input shaping, not domain validation. If `!ModelState.IsValid`, the action returns `View(vm)` immediately without calling any service. Otherwise it calls `IOrderService.CreateAsync(vm.ToCommand(), userId)` — orchestration only, with no business rules in the action.

The service checks business rules such as credit limits and product availability, persists via repository, and returns a result DTO or throws a domain exception. The service must not depend on `Controller` or `ViewBag` — it is callable from MVC, Minimal APIs, and background jobs. Back in the controller, on success the action returns a redirect following the PRG pattern; on service failure it adds errors to `ModelState` and redisplays the form.

The view renders the `<form>`, validation tag helpers via `asp-validation-for`, and displays `Model` properties — no `SaveChanges`, no pricing rules in `@{}` blocks. The boundary where business logic must not run is in binding (that is input shaping) and in the view (that is presentation only) — both before and after the service call.

---

#### Q8. (R) A shared `_Layout.cshtml` and three feature views rely on `ViewBag` keys set inconsistently across controllers. QA reports intermittent blank sidebars and wrong page titles in production only on certain pods.

```csharp
// HomeController
public IActionResult Index()
{
    ViewBag.Title = "Dashboard";
    ViewBag.ShowSidebar = true;
    ViewBag.CurrentUserDisplay = User.Identity?.Name;
    return View();
}

// ReportsController — different developer, six months later
public IActionResult Sales()
{
    ViewBag.PageTitle = "Sales"; // not ViewBag.Title
    ViewData["Sidebar"] = true;  // not ViewBag.ShowSidebar
    return View();
}
```

```html
<!-- _Layout.cshtml -->
<title>@ViewBag.Title</title>
@if (ViewBag.ShowSidebar == true) { <partial name="_Sidebar" model="ViewBag.CurrentUserDisplay" /> }
```

**Concepts**
- `ViewBag`/`ViewData` magic string keys — no compile-time check
- Inconsistent key names — silent null fallback in layout
- "Intermittent by pod" — actually intermittent by route, not deployment
- Strongly typed `PageViewModel` — compile-time safety and IntelliSense

**Answer**

`ViewBag.Title` and `ViewBag.PageTitle` are unrelated keys — the typo writes a value the layout never reads, so `@ViewBag.Title` is null on Sales pages and the `<title>` renders empty or falls through to a default. The same applies to `ViewBag.ShowSidebar` versus `ViewData["Sidebar"]`. Because these are dynamic dictionaries with string keys, no compiler or IDE check catches the mismatch. The "intermittent by pod" diagnosis is a red herring — it is actually intermittent by route, since some controllers set the right keys and others do not, but the pattern looks environmental in logs because different routes hit different controllers.

The fixes in priority order: introduce a `PageViewModel` or base `LayoutViewModel` with typed `Title`, `ShowSidebar`, and `UserDisplay` properties and set it in the controller or a base filter; use `@model PageViewModel` in `_Layout.cshtml` via `_ViewStart` or pass through a view component for layout chrome; prefer View Components for sidebar and user menu to encapsulate data loading rather than relying on magic `ViewBag` keys; add integration tests per area asserting HTML contains the expected `<title>` and sidebar marker.

---

#### Q9. (R) A consultant puts domain rules inside the view model and presentation logic in the entity. Review before merge.

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }

    public string DisplayLabel => $"{Name} ({UnitPrice:C})";

    public bool IsEligibleForDiscount(HttpContext httpContext)
    {
        var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
        return role == "Wholesale" && UnitPrice > 100;
    }
}

public class ProductEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }

    public void ApplyBusinessRules()
    {
        if (UnitPrice < 0) throw new InvalidOperationException("Price cannot be negative");
        if (Name.Length < 3) UnitPrice = 0; // "promo rule"
    }
}
```

**Concepts**
- Domain entity depending on `HttpContext` — web concern in domain layer
- `ApplyBusinessRules()` in view model — business rule hidden in UI layer
- Entity with `HttpContext` dependency — unusable from background workers
- Silent `UnitPrice = 0` side effect — should be a validation error

**Answer**

The entity references `HttpContext` which is a web concern — the domain layer must not depend on ASP.NET Core so it can be called from background workers, API projects, and batch jobs without referencing the HTTP stack. `IsEligibleForDiscount(HttpContext)` on a domain entity creates a hard dependency on `Microsoft.AspNetCore.Http` that breaks any non-web consumer. The view model's `ApplyBusinessRules()` encodes a promo policy in the UI layer so when an API creates a product it bypasses this rule entirely, allowing invalid products to be saved.

The specific problems: `Product.IsEligibleForDiscount` depends on ASP.NET Core and cannot be called from a background worker or a CLI tool; `ProductEditViewModel.ApplyBusinessRules()` silently sets `UnitPrice = 0` when `Name.Length < 3` rather than returning a validation error, making the side effect invisible to the caller; `DisplayLabel` on the entity is acceptable as a read-only projection but often belongs on the DTO for API versioning concerns.

The fixes in priority order: move eligibility to `IDiscountPolicy.IsEligible(product, userContext)` in the application layer, passing `ClaimsPrincipal` or a role value from the controller; keep the view model for input fields only and validate with FluentValidation; replace the silent price zeroing with an explicit validation message on `UnitPrice` / `Name`; map entities to DTOs or view models at the boundary and never pass entities with HTTP dependencies to Razor.

---

#### Q10. (P) You inherit a "fat controller" codebase (~400 lines per action). Describe a practical extraction sequence to reach thin controllers without a big-bang rewrite.

**Concepts**
- Characterization tests — integration coverage before moving code
- Extract I/O first — `HttpClient`, SMTP, SQL → injected services
- Use-case services — one service per user story, controller calls and maps
- View models — replace `ViewBag` and entity-in-view with explicit types

**Answer**

I extract in vertical slices: first isolate I/O behind interfaces, then move business rules into application services, then introduce view models — keeping controllers as HTTP adapters throughout so each PR is shippable and behavior-preserving.

I start by adding characterization tests — integration tests on critical routes such as checkout, login, and payment — before moving any code, to avoid rewrite regressions. Then I extract I/O first: `HttpClient`, SMTP, file storage, and raw SQL move to injected services such as `IPaymentGateway`, `IEmailSender`, and repositories. The controller keeps orchestration temporarily but gains immediately testable dependencies and the `IHttpClientFactory` fix. Next I extract use-case services — one application service per user story such as `PlaceOrder` and `RegisterUser`. The controller shrinks to validate, call, and map result. Transactions and domain rules move here. I introduce view models alongside this step, replacing `ViewBag` and entity-in-view with explicit types per controller.

What stays in the controller throughout this process: authorization attributes, model binding, `ModelState`, choosing view or redirect or status code, and mapping service exceptions to user-facing errors or `ProblemDetails` for APIs. I avoid hiding business logic in filters unless it is truly cross-cutting. I keep shipping by working one action or one controller per PR, use feature flags for risky paths, and do not block on a "pure" domain layer if a thin `OrderService` already removes 80% of the fat.

---

## Scenario-Based Questions continue above
