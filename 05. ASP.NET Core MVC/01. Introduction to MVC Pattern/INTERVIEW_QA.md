# Introduction to MVC Pattern — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 01. Introduction to MVC Pattern](#chapter-01-introduction-to-mvc-pattern)
  - [Q1. What is the MVC pattern?](#chapter-01-introduction-to-mvc-pattern-q1)
  - [Q2. What is the role of the Model in ASP.NET Core MVC?](#chapter-01-introduction-to-mvc-pattern-q2)
  - [Q3. What is the role of the View in ASP.NET Core MVC?](#chapter-01-introduction-to-mvc-pattern-q3)
  - [Q4. What is the role of the Controller in ASP.NET Core MVC?](#chapter-01-introduction-to-mvc-pattern-q4)
  - [Q5. What is the difference between MVC and MVP?](#chapter-01-introduction-to-mvc-pattern-q5)
  - [Q6. What is the difference between MVC and MVVM?](#chapter-01-introduction-to-mvc-pattern-q6)
  - [Q7. What is the difference between ASP.NET Core MVC and Razor Pa…](#chapter-01-introduction-to-mvc-pattern-q7)
  - [Q8. What is the difference between ASP.NET Core MVC and a SPA + …](#chapter-01-introduction-to-mvc-pattern-q8)
  - [Q9. What does "thin controller" mean and why is it preferred?](#chapter-01-introduction-to-mvc-pattern-q9)
  - [Q10. What is the "fat controller" anti-pattern?](#chapter-01-introduction-to-mvc-pattern-q10)
  - [Q11. Where should business logic live in an MVC application?](#chapter-01-introduction-to-mvc-pattern-q11)
  - [Q12. Where should data access logic live in an MVC application?](#chapter-01-introduction-to-mvc-pattern-q12)
  - [Q13. Where should input validation live in an MVC application?](#chapter-01-introduction-to-mvc-pattern-q13)
  - [Q14. How does a request flow through Model, View, and Controller?](#chapter-01-introduction-to-mvc-pattern-q14)
  - [Q15. What is the difference between a domain entity and a ViewMod…](#chapter-01-introduction-to-mvc-pattern-q15)
  - [Q16. What symptoms appear when business logic is placed in the Vi…](#chapter-01-introduction-to-mvc-pattern-q16)
  - [Q17. What symptoms appear when data access is placed in the Contr…](#chapter-01-introduction-to-mvc-pattern-q17)
  - [Q18. What is Post-Redirect-Get (PRG) and why is it used in MVC?](#chapter-01-introduction-to-mvc-pattern-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 01. Introduction to MVC Pattern

### Q1. What is the MVC pattern? {#chapter-01-introduction-to-mvc-pattern-q1}

What is the MVC pattern?

**Answer:** MVC (Model–View–Controller) separates an application into three cooperating parts: the Model holds data and domain logic, the View renders presentation, and the Controller handles input and orchestrates the flow between Model and View. In ASP.NET Core MVC, HTTP requests map to controller actions that prepare models and select Razor views for the response.

- The Controller receives the request, invokes services or repositories, and decides which view to render or whether to redirect.
- The Model represents domain entities, view models, and validation state passed to the view.
- The View is passive markup (Razor) that displays data the controller prepared — it does not own business rules.
- Separation keeps UI changes independent from business logic and makes each layer testable in isolation.
- ASP.NET Core 8 implements server-side MVC with endpoint routing, DI, and filter pipelines layered on the generic host.

---

### Q2. What is the role of the Model in ASP.NET Core MVC? {#chapter-01-introduction-to-mvc-pattern-q2}

What is the role of the Model in ASP.NET Core MVC?

**Answer:** In ASP.NET Core MVC, "Model" refers to the data and state an action passes to a view or receives from model binding — including domain entities, view models, DTOs, and `ModelState`. It is not a single class; it is the data contract between controller, binding, validation, and view.

- View models shape exactly what a form or page needs, excluding sensitive or irrelevant database fields.
- Domain entities represent business objects and invariants, typically accessed through application services rather than directly in views.
- `ModelState` carries validation results from Data Annotations, FluentValidation, or manual errors after binding.
- The model should not reference HTTP concerns like `HttpContext` or Razor-specific types.
- Strongly typed models (`@model`) give compile-time safety compared to `ViewBag` or `ViewData`.

---

### Q3. What is the role of the View in ASP.NET Core MVC? {#chapter-01-introduction-to-mvc-pattern-q3}

What is the role of the View in ASP.NET Core MVC?

**Answer:** The View is the presentation layer — Razor `.cshtml` files that render HTML from a model the controller supplies. Views should format and display data, not perform business logic, data access, or authorization decisions.

- Razor combines HTML with C# expressions, tag helpers, and partial views for reusable markup fragments.
- Views receive data through a strongly typed `@model`, or less preferably through `ViewData`/`ViewBag`.
- Layouts (`_Layout.cshtml`), sections, and partial views compose the final page structure.
- Views are compiled at publish time by default in ASP.NET Core 8, with optional runtime compilation in Development.
- Keeping views thin ensures pricing, authorization, and persistence rules stay testable in services.

---

### Q4. What is the role of the Controller in ASP.NET Core MVC? {#chapter-01-introduction-to-mvc-pattern-q4}

What is the role of the Controller in ASP.NET Core MVC?

**Answer:** The Controller is the HTTP adapter that maps URLs and verbs to action methods, binds input, calls application services, and selects the response — a view, redirect, or status result. It orchestrates the request; it should not own business rules or data access.

- Controllers inherit from `Controller` (MVC with views) or `ControllerBase` (API-only) and are discovered by routing.
- Actions return `IActionResult`/`Task<IActionResult>` to produce views, redirects, JSON, or HTTP error codes.
- Constructor injection supplies scoped services such as repositories, mediators, or application services.
- Controllers check `ModelState.IsValid`, enforce authorization via attributes or filters, and map service results to HTTP responses.
- A thin controller validates input, delegates work, and chooses the presentation — nothing more.

---

### Q5. What is the difference between MVC and MVP? {#chapter-01-introduction-to-mvc-pattern-q5}

What is the difference between MVC and MVP?

**Answer:** In MVC, the Controller drives the flow and pushes data to a passive View. In MVP (Model–View–Presenter), a Presenter mediates all interaction — the View is dumb, raises events, and the Presenter updates both the Model and the View.

- MVC fits request/response web apps where the server controls page flow per HTTP request.
- MVP is common in desktop UI (WinForms) where the view raises events and the presenter handles them synchronously.
- ASP.NET Core MVC implements Controller-driven flow, not automatic Presenter wiring.
- Teams sometimes extract presenter-like service classes to slim controllers, but the framework still routes to controller actions.
- Confusing the two leads to putting orchestration logic inside Razor views instead of controllers or services.

---

### Q6. What is the difference between MVC and MVVM? {#chapter-01-introduction-to-mvc-pattern-q6}

What is the difference between MVC and MVVM?

**Answer:** MVVM (Model–View–ViewModel) uses a ViewModel with bindable properties and commands; the View binds declaratively and changes propagate two-way. MVC uses a Controller to handle requests and push a model into a largely passive View.

- MVVM dominates XAML stacks (WPF, MAUI) and client-side SPA frameworks with observable state.
- ASP.NET Core MVC view models share the name but are typically one-way snapshots for server-rendered forms, not full MVVM unless client-side binding is added.
- In MVC, each HTTP request is stateless — there is no ongoing two-way binding unless JavaScript frameworks provide it.
- Blazor leans closer to component/state patterns but classic MVC remains request-driven.
- Placing domain rules in a view model's methods blurs MVVM presentation state with business logic.

---

### Q7. What is the difference between ASP.NET Core MVC and Razor Pages? {#chapter-01-introduction-to-mvc-pattern-q7}

What is the difference between ASP.NET Core MVC and Razor Pages?

**Answer:** Both use Razor for server-rendered HTML, but MVC organizes code around controllers and actions with `{controller}/{action}` routing, while Razor Pages colocates a PageModel with each `.cshtml` using page-centric routing.

- MVC suits apps with many controllers, areas, shared filters, and REST-style URL conventions across resources.
- Razor Pages suit page-focused workflows (forms, wizards, admin CRUD) with less ceremony per page.
- Razor Pages use `@page` directives and `PageModel` classes instead of controller action pairs.
- Both support tag helpers, validation, authorization, and the same underlying Razor view engine.
- The choice is organizational — the rendering pipeline and DI model are shared in ASP.NET Core 8.

---

### Q8. What is the difference between ASP.NET Core MVC and a SPA + Minimal API approach? {#chapter-01-introduction-to-mvc-pattern-q8}

What is the difference between ASP.NET Core MVC and a SPA + Minimal API approach?

**Answer:** ASP.NET Core MVC renders HTML on the server and uses full page loads or partial updates; a SPA + Minimal API serves JSON from API endpoints and renders UI entirely in the browser with JavaScript.

- MVC keeps validation, routing, and auth integrated in one host with cookie-based sessions and form posts.
- SPA + Minimal API splits backend (JSON endpoints) and frontend (React, Angular, etc.), often using JWT or token auth.
- MVC fits form-heavy, SEO-sensitive, or server-rendered admin apps with simpler deployment.
- SPA fits rich client interactivity, offline-like UX, or multiple clients sharing one API.
- Hybrid approaches (MVC + AJAX partials) exist but increase complexity compared to picking one primary model.

---

### Q9. What does "thin controller" mean and why is it preferred? {#chapter-01-introduction-to-mvc-pattern-q9}

What does "thin controller" mean and why is it preferred?

**Answer:** A thin controller limits itself to HTTP concerns — binding input, calling services, checking `ModelState`, and returning views or redirects — without embedding business rules, SQL, or email logic. This makes controllers easy to test, reuse, and maintain.

- Business logic moves to application or domain services callable from MVC, Minimal APIs, and background workers.
- Data access hides behind repositories or `DbContext` interfaces injected into services, not controllers.
- Thin controllers reduce duplication when the same use case is exposed via web and API endpoints.
- Unit tests mock service interfaces instead of spinning up databases or HTTP contexts.
- The controller becomes a stable adapter even when business rules change frequently.

---

### Q10. What is the "fat controller" anti-pattern? {#chapter-01-introduction-to-mvc-pattern-q10}

What is the "fat controller" anti-pattern?

**Answer:** A fat controller stuffs data access, validation rules, mapping, external HTTP calls, and email sending directly into action methods — often hundreds of lines per action. It violates single responsibility and makes testing and reuse difficult.

- Actions become untestable without full stack integration tests and real databases.
- Business rules duplicated in controllers cannot be shared with API projects or batch jobs.
- Direct `DbContext`, ADO.NET, or `new HttpClient()` usage in actions causes lifetime and scalability issues.
- Refactoring requires touching controller code for every business change instead of isolated services.
- Fat controllers often pass EF entities straight to views, coupling UI to the database schema.

---

### Q11. Where should business logic live in an MVC application? {#chapter-01-introduction-to-mvc-pattern-q11}

Where should business logic live in an MVC application?

**Answer:** Business logic belongs in domain entities (for core invariants) and application services (for use-case orchestration) — not in controllers, views, or Razor PageModels beyond coordination. Controllers call these services and map results to HTTP responses.

- Domain rules like discount eligibility, stock checks, or pricing policies live in domain methods or dedicated policy classes.
- Application services coordinate transactions, call repositories, and publish domain events across a use case.
- Controllers translate HTTP input into service commands and map outcomes to views or redirects.
- Views display computed results from view models — they do not recalculate business values.
- Centralizing rules ensures MVC forms, REST APIs, and background jobs enforce the same behavior.

---

### Q12. Where should data access logic live in an MVC application? {#chapter-01-introduction-to-mvc-pattern-q12}

Where should data access logic live in an MVC application?

**Answer:** Data access belongs in repositories, query services, or EF Core `DbContext` usage behind interfaces registered in DI — not in controllers or views. Controllers and services depend on abstractions like `IOrderRepository`, not raw SQL.

- Repositories encapsulate queries, persistence, and mapping from database rows to domain objects.
- EF Core `DbContext` is registered scoped per request and injected into repositories or services.
- Controllers never open connections or call `SaveChanges` directly in well-structured apps.
- Views must never query databases — not even via `@inject AppDbContext`.
- Separating data access enables unit tests with fakes and integration tests focused on the repository layer.

---

### Q13. Where should input validation live in an MVC application? {#chapter-01-introduction-to-mvc-pattern-q13}

Where should input validation live in an MVC application?

**Answer:** Input validation — required fields, string length, format, range — belongs on view models or input DTOs using Data Annotations, FluentValidation, or `IValidatableObject`. Server-side validation always runs in the action or a filter before any persist operation.

- Data Annotations on view models generate both server-side checks and client-side `data-val-*` attributes.
- Business validation (e.g., "credit limit exceeded") belongs in services and adds errors to `ModelState` manually.
- Validation on EF entities shared with MVC risks wrong-layer coupling and API inconsistencies.
- Client-side validation improves UX but is bypassable — server checks are mandatory.
- `ModelState.IsValid` in the action gates the call to persistence services.

---

### Q14. How does a request flow through Model, View, and Controller? {#chapter-01-introduction-to-mvc-pattern-q14}

How does a request flow through Model, View, and Controller?

**Answer:** Kestrel receives the HTTP request, middleware runs (routing, auth, etc.), routing selects a controller action, model binding constructs input models, the controller invokes services, and the action returns a view result with a model that Razor renders into HTML.

- Routing maps the URL and HTTP verb to `Controller.Action` via conventional or attribute routes.
- Model binding populates action parameters from form fields, route values, and query strings; validation runs during binding.
- The controller calls application services with bound input and receives domain results or view models.
- On success the action returns `View(model)` or `RedirectToAction`; on validation failure it redisplays the form with errors.
- The view engine locates the `.cshtml` file, applies `_ViewStart` layout, and renders HTML sent back through the middleware pipeline.

---

### Q15. What is the difference between a domain entity and a ViewModel? {#chapter-01-introduction-to-mvc-pattern-q15}

What is the difference between a domain entity and a ViewModel?

**Answer:** A domain entity models business concepts and persistence with identity, relationships, and invariants tied to the database. A ViewModel shapes data specifically for one view or form — including display fields, validation attributes, and no sensitive or unnecessary properties.

- Entities may have navigation properties, concurrency tokens, and internal fields not meant for UI binding.
- ViewModels prevent over-posting by exposing only editable fields on create/edit forms.
- Entities belong in the domain/data layer; view models belong in the presentation layer.
- Mapping between entity and view model happens in the controller or a dedicated mapper at the boundary.
- Passing entities directly to Razor couples views to schema changes and exposes hidden fields to mass assignment.

---

### Q16. What symptoms appear when business logic is placed in the View? {#chapter-01-introduction-to-mvc-pattern-q16}

What symptoms appear when business logic is placed in the View?

**Answer:** Business logic in Razor views produces untested calculations, duplicated rules across pages, and inconsistencies between UI, API, PDF, and email outputs. Changes require hunting `.cshtml` files instead of updating a single service.

- Pricing, tax, or discount math in `@{}` blocks bypasses unit tests on services.
- Authorization checks in views can be skipped by crafting requests or using alternate endpoints.
- Magic numbers and policy thresholds scattered in markup drift from domain rules over time.
- Performance issues appear when views perform per-row calculations or service calls inside loops.
- Production bugs surface where the view shows one value but the database or API stores another.

---

### Q17. What symptoms appear when data access is placed in the Controller? {#chapter-01-introduction-to-mvc-pattern-q17}

What symptoms appear when data access is placed in the Controller?

**Answer:** Data access in controllers creates thick actions with raw SQL or LINQ, makes unit testing require a real database, and mixes HTTP handling with query logic. Changes to queries force controller edits and invite N+1 or transaction bugs.

- Actions grow with `DbContext` queries, `SaveChanges`, and manual mapping inline.
- Controller tests become integration tests needing `WebApplicationFactory` or SQL Server.
- The same query logic gets duplicated when adding API endpoints or background jobs.
- Connection management, retry, and transaction boundaries are ad hoc instead of centralized.
- Security risks increase when SQL or EF queries embedded in controllers lack consistent authorization checks.

---

### Q18. What is Post-Redirect-Get (PRG) and why is it used in MVC? {#chapter-01-introduction-to-mvc-pattern-q18}

What is Post-Redirect-Get (PRG) and why is it used in MVC?

**Answer:** Post-Redirect-Get completes a successful POST by returning an HTTP redirect to a GET action instead of rendering the view directly. This prevents duplicate form submissions when the user refreshes the browser after a create or update.

- After processing a valid POST, the action returns `RedirectToAction("Details", new { id })` rather than `return View()`.
- The browser's refresh on the GET URL is safe — it does not resubmit the form body.
- PRG is standard for create, update, and delete operations in server-rendered MVC apps.
- Validation failures typically redisplay the form with `return View(model)` without redirect so `ModelState` errors remain.
- `TempData` can carry flash messages across the redirect when needed.

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

---

**Answer:**

**Answer:** This controller recreates WebForms page lifecycle and code-behind inside MVC — mixing transport detection, ADO.NET data access, validation, and side effects in one action — which blocks async I/O, testability, and clear separation of concerns.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Architecture | `IsPostBack()` / GET-vs-POST branching mimics WebForms `Page_Load` | Wrong mental model; fights RESTful action design |
| Data access | Raw `SqlConnection` / `SqlCommand` in controller | Untestable; no DI; connection leaks if refactored poorly |
| Design | Business validation and `SendWelcomeEmail` inline | Fat controller; cannot reuse from API or background job |
| MVC | `ViewBag.Message` for state | Untyped view contract; magic strings |
| Runtime | Sync ADO.NET on default thread pool | Poor scalability vs async EF/repository |
| Config | `Configuration["DefaultConnection"]` static access | Bypasses options pattern and test configuration |

**Fix (priority order):**

1. Split into explicit actions: `GET Index` (show form) and `POST Index` or `POST LoadCustomer` with `[FromForm]` model binding — drop `IsPostBack`.
2. Inject `ICustomerRepository` or `AppDbContext` (scoped); move SQL to repository/service layer.
3. Move email to `ICustomerNotificationService`; validate with Data Annotations or FluentValidation on a view model/DTO.
4. Return strongly typed `CustomerDetailsViewModel` to the view — not entity + `ViewBag`.

**Production takeaway:** Karat uses WebForms carryover to test whether you recognize MVC as **action-per-intent + injected services**, not a renamed `.aspx.cs` file.

---

---

#### Q2. (D) A desktop WPF team and a web team both say they use "MV-something." Compare **MVC**, **MVP**, and **MVVM** for ASP.NET Core MVC — which pattern does the framework implement, where do the others still appear, and what confusion causes production bugs?

---

**Answer:**

**Answer:** ASP.NET Core MVC implements **Model–View–Controller** for server-rendered web: the controller orchestrates HTTP, the model carries data and validation state, and the view (Razor) renders HTML. MVP and MVVM appear elsewhere in the .NET ecosystem and only partially overlap — conflating them leads to putting UI logic in the wrong layer.

- **MVC (ASP.NET Core MVC):** Controller receives request, invokes services, selects view with a model. View is passive markup; controller drives flow. Fits request/response web apps with server-side rendering.
- **MVP (Model–View–Presenter):** Presenter mediates; view is dumb and raises events. Common in WinForms / older UI. In web terms, a **presenter-like** class sometimes replaces fat controllers — but ASP.NET Core does not wire this automatically.
- **MVVM (Model–View–ViewModel):** ViewModel exposes bindable state and commands; view binds declaratively (WPF, MAUI, Blazor in spirit). Razor **view models** borrow the name but are not full MVVM unless you add client-side binding frameworks (Knockout, Alpine, SPA).
- **Confusion bugs:** Treating Razor like XAML (business rules in ViewModel that belong in domain services); putting presenter-style orchestration in views via `@{}` code blocks; expecting two-way binding semantics on server-rendered forms without JavaScript.
- **ASP.NET Core alignment:** Entities/domain → **Model**; DTOs/`EditViewModel` → view-facing **Model**; `Controller` → coordination; `.cshtml` → **View**. API projects drop the V and return JSON from controllers or minimal endpoints.

**Production takeaway:** Name the **direction of control** — in MVC the controller pushes data to the view; in MVVM the view binds to the view model. Mixing them without discipline recreates fat controllers or fat views.

---

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

---

**Answer:**

**Answer:** The action owns inventory rules, persistence, and outbound payment HTTP in one method — a classic fat controller — and uses `new HttpClient()` per request, which causes socket exhaustion and untestable payment integration under real concurrency.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Checkout orchestration, stock mutation, pricing in controller | Unmaintainable; duplicate logic when adding Minimal API or worker |
| Data | Multiple `FindAsync` in loop (N+1 queries) | Latency spikes under load |
| Correctness | Stock decrements without transaction/ concurrency token | Overselling when two checkouts race |
| HTTP | `new HttpClient()` per request | Socket exhaustion; DNS stale handler (See ASP.NET Core — `IHttpClientFactory`) |
| Security | No authorization check that `cartId` belongs to current user | IDOR — charge another user's cart |
| UX | Returns `View("Error", string)` for stock failure | Inconsistent error handling vs `ProblemDetails` / validation pattern |

**Fix (priority order):**

1. Extract `ICheckoutService.CheckoutAsync(cartId, userId, ct)` — controller only validates auth, calls service, maps result to `RedirectToAction` or error view.
2. Wrap stock + order creation in a transaction; use row versioning or `UPDATE ... WHERE Stock >= @qty`.
3. Replace `new HttpClient()` with typed `HttpClient` via `IHttpClientFactory` or `IPaymentGateway` abstraction.
4. Verify `cart.UserId == currentUserId` before any mutation.

**Production takeaway:** Fat controllers often **pass unit tests with in-memory EF** yet fail on concurrency, HTTP resource limits, and authorization — Karat stacks multiple categories in one snippet.

---

---

#### Q4. (P) Where should **input validation**, **business rules**, and **data access** live in a well-structured ASP.NET Core MVC app — and what symptoms appear when each is placed in the wrong layer (View, Controller, or Service)?

---

**Answer:**

**Answer:** Input validation belongs on view models/DTOs (Data Annotations, FluentValidation, or `IValidatableObject`); business rules belong in domain or application services; data access belongs in repositories or the DbContext behind an interface — controllers coordinate and map results, views render only.

| Concern | Correct layer | Wrong layer symptom |
|---|---|---|
| **Input validation** (required, range, format) | View model + model binding / FluentValidation filter | Rules in Razor → duplicated in API; untestable; bypass via crafted POST |
| **Business rules** (discount eligibility, stock policy) | Domain entity methods or application service | Rules in controller → duplicated across MVC and Minimal API; 400-line actions |
| **Data access** (queries, persistence) | Repository / DbContext behind `IProductRepository` | SQL in view → security hole; EF in Razor → compile/runtime chaos |
| **Presentation** (formatting, "show sidebar") | View model or view helper / Tag Helper | Business logic in view model `ApplyBusinessRules()` → reused as domain truth incorrectly |

- Controllers should: authorize, bind input, call services, choose view/redirect, surface `ModelState` errors — not compute pricing policies inline.
- Views should: render model state, display validation summaries — not call `SaveChanges`.
- When a rule appears in two places (MVC form + JSON API), **lift it to a service** the controller and minimal endpoints both call.

**Production takeaway:** Layer violations are interview gold because symptoms show up as **duplication, security bugs, and tests that need full stack** — not as compile errors.

---

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

---

**Answer:**

**Answer:** The controller is still the query and orchestration layer — it owns EF LINQ, sorting, and filtering — so any meaningful test must exercise HTTP + database; extracting a service with an interface is the missing step for true controller unit tests.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | LINQ and persistence in controller | "Unit" tests become integration tests |
| Testability | No seam for `IProductQueryService` | Cannot mock product lists; slow CI |
| MVC | `ViewBag` for search metadata | View contract untyped; brittle layout tests |
| API shape | Returns entity list directly to view | Over-posting / leakage if entities gain navigation props |

**Fix (priority order):**

1. Introduce `IProductQueryService.SearchAsync(string? term, ct)` returning `IReadOnlyList<ProductListItemDto>`.
2. Controller becomes: call service → build `ProductIndexViewModel` (search term + items + count) → return `View(model)`.
3. Unit-test controller with mocked `IProductQueryService` — assert `ViewResult` model without `WebApplicationFactory`.
4. Keep one or two integration tests for routing and binding — not every filter scenario.

**Production takeaway:** Thin controller means **no I/O in the controller** — not merely shorter methods. Karat distinguishes controller unit tests from slice/integration tests.

---

---

#### Q6. (D) Product asks for a **server-rendered admin CRUD** app (forms, validation messages, role-based pages). When do you choose **MVC**, **Razor Pages**, or **Minimal APIs + SPA**, and what would make you reverse the decision after six months?

---

**Answer:**

**Answer:** For form-heavy, server-rendered admin CRUD with roles, **Razor Pages or MVC** are the natural fit; Minimal APIs + SPA pays off for rich client interactivity but adds operational and contract overhead — choose based on UX complexity and team skills, not hype.

- **Choose MVC** when you want conventional `{Controller}/{Action}` URLs, shared filters across disparate resources, and teams already organized around controllers (multiple related controllers, areas, complex attribute routing).
- **Choose Razor Pages** when each page is a cohesive form/list (page-centric routing), you want colocated PageModel + `.cshtml`, and admin CRUD maps 1:1 to pages — less ceremony than controller-per-resource for simple flows.
- **Choose Minimal APIs + SPA** when you need rich client state, offline-ish UX, or mobile clients sharing the same backend — not when the requirement is mostly POST-redirect-GET forms with server validation messages.
- **Reverse to MVC/Razor Pages from SPA:** validation duplicated client/server, auth cookie + API token complexity, SEO irrelevant but deployment doubled (API + static host), team slower on front-end than C#.
- **Reverse to SPA from Razor:** heavy partial-page AJAX everywhere (fighting the model), real-time dashboards, or API must serve non-browser clients anyway — then invest in API layer deliberately.
- **Role-based pages:** `[Authorize(Roles = "Admin")]` on PageModel or controller; policy-based auth works in both MVC and Razor Pages — not a differentiator.

**Production takeaway:** Server-rendered admin CRUD is still **Razor-first territory**; Karat wants you to reject Minimal API + React defaults when the problem is forms and authorization gates, not JSON throughput.

---

---

#### Q7. (M) A browser submits `POST /Orders/Create` with form fields bound to `CreateOrderViewModel`. Trace responsibilities through **Model**, **View**, and **Controller** from first byte received through HTML response — where does model binding stop and where must business logic not run?

---

**Answer:**

**Answer:** Kestrel and middleware build `HttpContext`; routing selects the action; **model binding** constructs `CreateOrderViewModel` from form values and runs annotation validation; the **controller** calls application services using that input; the **model** returned to the view is a display/view model (or the same VM with errors); the **view** renders HTML — business invariants are enforced in services/domain, not in binding or Razor.

1. **Transport → MVC entry:** Routing maps to `OrdersController.Create(CreateOrderViewModel vm)`. Model binder populates `vm` from form fields; `ModelState` records validation errors from Data Annotations — this is **input shaping**, not domain validation.
2. **Controller:** If `!ModelState.IsValid`, return `View(vm)` — no service call. Otherwise call `IOrderService.CreateAsync(vm.ToCommand(), userId)` — **orchestration only**.
3. **Model (domain/service):** Service checks business rules (credit limit, product availability), persists via repository, returns result DTO or throws domain exception — **must not** depend on `Controller` or `ViewBag`.
4. **Controller (response):** Success → redirect (PRG pattern) or return `View` with result view model; failure → add errors to `ModelState`, redisplay form.
5. **View:** Razor renders `<form>`, validation tag helpers / `asp-validation-for`, displays `Model` properties — **no** `SaveChanges`, no pricing rules in `@{}` blocks.

**Production takeaway:** Model binding ends at **well-formed input**; confusing it with business validation leads to rules that APIs and batch jobs never execute.

---

---

#### Q8. (R) A shared `_Layout.cshtml` and three feature views rely on `ViewBag` keys set inconsistently across controllers. QA reports intermittent blank sidebars and wrong page titles in production only on certain pods. Review the pattern — what is wrong?

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

---

**Answer:**

**Answer:** `ViewBag`/`ViewData` are untyped dictionaries with string keys — inconsistent key names across controllers silently fail views, and relying on them for layout contracts creates fragile presentation logic that no compiler checks.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | `ViewBag.Title` vs `ViewBag.PageTitle`; `ViewData["Sidebar"]` vs `ViewBag.ShowSidebar` | Layout renders empty title / missing sidebar — looks "intermittent" by route |
| Type safety | `ViewBag` is `dynamic` | Typos fail at runtime only on affected pages |
| Testability | Layout depends on side effects not on view model | Hard to assert correct chrome in tests |
| Scale-out red herring | "Certain pods" often means **certain routes** hit different controllers | Misdiagnosed as deployment issue |
| Partial | `_Sidebar` model from `ViewBag.CurrentUserDisplay` unset on Reports | Null reference or empty sidebar |

**Fix (priority order):**

1. Introduce `LayoutViewModel` or base `PageViewModel` with `Title`, `ShowSidebar`, `UserDisplay` — set in controller or filter.
2. Use `@model PageViewModel` in `_Layout.cshtml` (via `_ViewStart` passing model or view component for layout chrome).
3. Prefer **View Components** for sidebar/user menu — encapsulate data loading instead of magic `ViewBag` keys.
4. Add integration tests per area asserting HTML contains expected `<title>` and sidebar marker.

**Production takeaway:** ViewBag leakage is a **maintainability trap** — Karat uses inconsistent keys to test whether you push toward strongly typed view models and layout contracts.

---

---

#### Q9. (R) A consultant puts domain rules inside the view model and presentation logic in the entity. Review before merge — what fails at compile time, runtime, and in API reuse?

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

---

**Answer:**

**Answer:** The entity references `HttpContext` (web concern in domain layer) and the view model mutates business state in `ApplyBusinessRules` — inverting MVC separation and making entities unusable from background workers or API projects without referencing ASP.NET Core.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Layering | `Product.IsEligibleForDiscount(HttpContext)` on entity | Domain depends on ASP.NET Core; breaks clean architecture and non-web callers |
| Layering | `ProductEditViewModel.ApplyBusinessRules()` encodes promo policy | Rule hidden in UI layer; API creates invalid products |
| Design | `DisplayLabel` on entity | Acceptable as read-only projection, but often belongs on DTO for API versioning |
| Correctness | Silent `UnitPrice = 0` when name short | Surprising side effect; should be validation error |
| Reuse | API controller returns `Product` entity | Leaks discount logic tied to HTTP user to JSON consumers incorrectly |

**Fix (priority order):**

1. Move eligibility to `IDiscountPolicy.IsEligible(product, userContext)` in application layer; pass `ClaimsPrincipal` or role from controller.
2. Keep view model for input fields only; validate with FluentValidation; call domain service on submit.
3. Map entities → DTOs/view models at boundary; never pass entities with `HttpContext` methods to Razor.
4. Replace silent price zeroing with explicit validation message on `UnitPrice` / `Name`.

**Production takeaway:** Wrong-layer responsibilities compile fine — Karat tests whether you catch **web dependencies in domain** and **business rules in view models** before they spread.

---

---

#### Q10. (P) You inherit a "fat controller" codebase (~400 lines per action). Describe a **practical extraction sequence** to reach thin controllers without a big-bang rewrite — what moves first, what stays in the controller, and how you keep shipping?



**Answer:**

**Answer:** Extract in vertical slices: first isolate I/O behind interfaces, then move business rules into application services, then introduce view models — keep controllers as HTTP adapters throughout so each PR is shippable and behavior-preserving.

1. **Stabilize with characterization tests** — integration tests on critical routes (checkout, login, payment) before moving code; avoids rewrite regressions.
2. **Extract I/O first** — `HttpClient`, SMTP, file storage, raw SQL → injected services (`IPaymentGateway`, `IEmailSender`, repositories). Controller keeps orchestration temporarily; immediate win for testing and `IHttpClientFactory`.
3. **Extract use-case services** — one application service per user story (`PlaceOrder`, `RegisterUser`); controller shrinks to validate → call → map result. Move transactions and domain rules here.
4. **Introduce view models** — replace `ViewBag` and entity-in-view with explicit types; can happen per controller alongside step 3.
5. **What stays in controller** — authorization attributes, model binding, `ModelState`, choosing view/redirect/status code, mapping service exceptions to user-facing errors (or ProblemDetails for APIs).
6. **Cross-cutting** — filters for repeated auth/logging; avoid hiding business logic in filters unless truly cross-cutting.
7. **Keep shipping** — one action or one controller per PR; feature flags for risky paths; do not block on "pure" domain layer if a thin `OrderService` already removes 80% of fat.

**Production takeaway:** Refactors win interviews when you describe **incremental seams** (I/O → use cases → view models) — not a three-month "rewrite to clean architecture" that never merges.

---
