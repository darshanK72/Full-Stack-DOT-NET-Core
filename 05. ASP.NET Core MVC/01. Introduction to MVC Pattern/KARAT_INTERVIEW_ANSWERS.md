# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/01. Introduction to MVC Pattern`

---

#### Q1. (R) A team migrating from WebForms ports this "controller." It compiles and renders in dev. What architectural problems appear at scale, and how should responsibilities move in ASP.NET Core MVC?

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

#### Q2. (D) A desktop WPF team and a web team both say they use "MV-something." Compare **MVC**, **MVP**, and **MVVM** for ASP.NET Core MVC — which pattern does the framework implement, where do the others still appear, and what confusion causes production bugs?

**Answer:** ASP.NET Core MVC implements **Model–View–Controller** for server-rendered web: the controller orchestrates HTTP, the model carries data and validation state, and the view (Razor) renders HTML. MVP and MVVM appear elsewhere in the .NET ecosystem and only partially overlap — conflating them leads to putting UI logic in the wrong layer.

- **MVC (ASP.NET Core MVC):** Controller receives request, invokes services, selects view with a model. View is passive markup; controller drives flow. Fits request/response web apps with server-side rendering.
- **MVP (Model–View–Presenter):** Presenter mediates; view is dumb and raises events. Common in WinForms / older UI. In web terms, a **presenter-like** class sometimes replaces fat controllers — but ASP.NET Core does not wire this automatically.
- **MVVM (Model–View–ViewModel):** ViewModel exposes bindable state and commands; view binds declaratively (WPF, MAUI, Blazor in spirit). Razor **view models** borrow the name but are not full MVVM unless you add client-side binding frameworks (Knockout, Alpine, SPA).
- **Confusion bugs:** Treating Razor like XAML (business rules in ViewModel that belong in domain services); putting presenter-style orchestration in views via `@{}` code blocks; expecting two-way binding semantics on server-rendered forms without JavaScript.
- **ASP.NET Core alignment:** Entities/domain → **Model**; DTOs/`EditViewModel` → view-facing **Model**; `Controller` → coordination; `.cshtml` → **View**. API projects drop the V and return JSON from controllers or minimal endpoints.

**Production takeaway:** Name the **direction of control** — in MVC the controller pushes data to the view; in MVVM the view binds to the view model. Mixing them without discipline recreates fat controllers or fat views.

---

#### Q3. (R) Review this order checkout action promoted to production. Tests pass with an in-memory database. What breaks under load or when requirements change?

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

#### Q4. (P) Where should **input validation**, **business rules**, and **data access** live in a well-structured ASP.NET Core MVC app — and what symptoms appear when each is placed in the wrong layer (View, Controller, or Service)?

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

#### Q5. (R) This refactor was sold as "thin controllers + testability." Unit tests on `ProductsControllerTests` still require `TestServer` and a real database. Diagnose the layering mistake.

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

#### Q6. (D) Product asks for a **server-rendered admin CRUD** app (forms, validation messages, role-based pages). When do you choose **MVC**, **Razor Pages**, or **Minimal APIs + SPA**, and what would make you reverse the decision after six months?

**Answer:** For form-heavy, server-rendered admin CRUD with roles, **Razor Pages or MVC** are the natural fit; Minimal APIs + SPA pays off for rich client interactivity but adds operational and contract overhead — choose based on UX complexity and team skills, not hype.

- **Choose MVC** when you want conventional `{Controller}/{Action}` URLs, shared filters across disparate resources, and teams already organized around controllers (multiple related controllers, areas, complex attribute routing).
- **Choose Razor Pages** when each page is a cohesive form/list (page-centric routing), you want colocated PageModel + `.cshtml`, and admin CRUD maps 1:1 to pages — less ceremony than controller-per-resource for simple flows.
- **Choose Minimal APIs + SPA** when you need rich client state, offline-ish UX, or mobile clients sharing the same backend — not when the requirement is mostly POST-redirect-GET forms with server validation messages.
- **Reverse to MVC/Razor Pages from SPA:** validation duplicated client/server, auth cookie + API token complexity, SEO irrelevant but deployment doubled (API + static host), team slower on front-end than C#.
- **Reverse to SPA from Razor:** heavy partial-page AJAX everywhere (fighting the model), real-time dashboards, or API must serve non-browser clients anyway — then invest in API layer deliberately.
- **Role-based pages:** `[Authorize(Roles = "Admin")]` on PageModel or controller; policy-based auth works in both MVC and Razor Pages — not a differentiator.

**Production takeaway:** Server-rendered admin CRUD is still **Razor-first territory**; Karat wants you to reject Minimal API + React defaults when the problem is forms and authorization gates, not JSON throughput.

---

#### Q7. (M) A browser submits `POST /Orders/Create` with form fields bound to `CreateOrderViewModel`. Trace responsibilities through **Model**, **View**, and **Controller** from first byte received through HTML response — where does model binding stop and where must business logic not run?

**Answer:** Kestrel and middleware build `HttpContext`; routing selects the action; **model binding** constructs `CreateOrderViewModel` from form values and runs annotation validation; the **controller** calls application services using that input; the **model** returned to the view is a display/view model (or the same VM with errors); the **view** renders HTML — business invariants are enforced in services/domain, not in binding or Razor.

1. **Transport → MVC entry:** Routing maps to `OrdersController.Create(CreateOrderViewModel vm)`. Model binder populates `vm` from form fields; `ModelState` records validation errors from Data Annotations — this is **input shaping**, not domain validation.
2. **Controller:** If `!ModelState.IsValid`, return `View(vm)` — no service call. Otherwise call `IOrderService.CreateAsync(vm.ToCommand(), userId)` — **orchestration only**.
3. **Model (domain/service):** Service checks business rules (credit limit, product availability), persists via repository, returns result DTO or throws domain exception — **must not** depend on `Controller` or `ViewBag`.
4. **Controller (response):** Success → redirect (PRG pattern) or return `View` with result view model; failure → add errors to `ModelState`, redisplay form.
5. **View:** Razor renders `<form>`, validation tag helpers / `asp-validation-for`, displays `Model` properties — **no** `SaveChanges`, no pricing rules in `@{}` blocks.

**Production takeaway:** Model binding ends at **well-formed input**; confusing it with business validation leads to rules that APIs and batch jobs never execute.

---

#### Q8. (R) A shared `_Layout.cshtml` and three feature views rely on `ViewBag` keys set inconsistently across controllers. QA reports intermittent blank sidebars and wrong page titles in production only on certain pods. Review the pattern — what is wrong?

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

#### Q9. (R) A consultant puts domain rules inside the view model and presentation logic in the entity. Review before merge — what fails at compile time, runtime, and in API reuse?

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

#### Q10. (P) You inherit a "fat controller" codebase (~400 lines per action). Describe a **practical extraction sequence** to reach thin controllers without a big-bang rewrite — what moves first, what stays in the controller, and how you keep shipping?

**Answer:** Extract in vertical slices: first isolate I/O behind interfaces, then move business rules into application services, then introduce view models — keep controllers as HTTP adapters throughout so each PR is shippable and behavior-preserving.

1. **Stabilize with characterization tests** — integration tests on critical routes (checkout, login, payment) before moving code; avoids rewrite regressions.
2. **Extract I/O first** — `HttpClient`, SMTP, file storage, raw SQL → injected services (`IPaymentGateway`, `IEmailSender`, repositories). Controller keeps orchestration temporarily; immediate win for testing and `IHttpClientFactory`.
3. **Extract use-case services** — one application service per user story (`PlaceOrder`, `RegisterUser`); controller shrinks to validate → call → map result. Move transactions and domain rules here.
4. **Introduce view models** — replace `ViewBag` and entity-in-view with explicit types; can happen per controller alongside step 3.
5. **What stays in controller** — authorization attributes, model binding, `ModelState`, choosing view/redirect/status code, mapping service exceptions to user-facing errors (or ProblemDetails for APIs).
6. **Cross-cutting** — filters for repeated auth/logging; avoid hiding business logic in filters unless truly cross-cutting.
7. **Keep shipping** — one action or one controller per PR; feature flags for risky paths; do not block on "pure" domain layer if a thin `OrderService` already removes 80% of fat.

**Production takeaway:** Refactors win interviews when you describe **incremental seams** (I/O → use cases → view models) — not a three-month "rewrite to clean architecture" that never merges.
