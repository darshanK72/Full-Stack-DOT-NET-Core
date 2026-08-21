# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/02. Controllers & Actions`

---

#### Q1. (R) Review this action from a code review. The developer says "it compiles and works in Swagger." What is wrong with the return type and result handling?

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

#### Q2. (R) A junior developer converts synchronous actions to async. After deploy, unhandled exceptions crash the worker process on slow queries. Review the action.

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

#### Q3. (R) Review constructor and field usage on this MVC controller. Index works; other actions intermittently throw `NullReferenceException`.

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

#### Q4. (R) A legacy module still resolves dependencies through a static locator. Review and explain what breaks for testing and lifetimes.

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

#### Q5. (R) The same controller serves a Razor admin page and a React widget on the dashboard. QA reports "HTML instead of JSON" for one endpoint. Review.

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

#### Q6. (R) Review this account controller. Security finds login CSRF exposure and duplicate route matches in integration tests.

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

#### Q7. (R) Review this create action. Invalid posts persist bad data; QA sees no validation messages on the form.

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

#### Q8. (R) Attribute routing tests fail with `AmbiguousMatchException`. Review these action signatures.

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

#### Q9. (R) Review this controller pulled from a tutorial. The team wants thin controllers and unit tests without a database.

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

#### Q10. (P) An internal MVC app accepts POST forms for role changes. Pen test reports missing CSRF protection on one action. Review the pattern and what production setup is required beyond the attribute.

**Answer:** State-changing POST actions must carry `[ValidateAntiForgeryToken]` and the form must emit the request verification token; globally, antiforgery services must be registered (default in MVC) and tokens must flow on every mutating form — including AJAX if used.

- Add `[ValidateAntiForgeryToken]` on POST `EditUser` — pairs with `[HttpPost]`.
- Razor `<form asp-action="EditUser" method="post">` tag helper renders `@Html.AntiForgeryToken()` automatically.
- For AJAX POSTs, send header `RequestVerificationToken` with token from `<input name="__RequestVerificationToken">`.
- Optional defense in depth: `[AutoValidateAntiforgeryToken]` at controller level for all unsafe verbs.
- Ensure `[HttpGet]` on GET overload — `[HttpPost]` on POST; role changes must never be GET.
- SameSite cookies and HTTPS protect token in transit; antiforgery does not replace authorization — still verify admin role in action or filter.

**Production takeaway:** Anti-forgery is a **pair**: server validation attribute + client token on every POST — missing either fails pen tests silently until production.

---

#### Q11. (D) API consumers report inconsistent status codes: missing resources return 400, malformed ids return 404. Review this action and explain the correct HTTP semantics for MVC vs JSON API responses.

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

#### Q12. (M) A developer implements `IDisposable` on a controller to release an expensive native handle created in the constructor. Under load, handles leak and `_handle` is sometimes already disposed. Explain controller activation, lifetime, and the correct pattern.

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
