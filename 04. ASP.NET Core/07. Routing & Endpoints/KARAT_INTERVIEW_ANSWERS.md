# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_ANSWERS.md) in this folder.

> **Folder:** `05. ASP.NET Core/07. Routing & Endpoints`

---

#### Q1. (P) When an API creates a new resource and returns HTTP 201 Created, how should it include the newly created resource and its `Location` header? Compare MVC `CreatedAtAction` with minimal API `Results.Created`.

**Answer:** Return 201 with a `Location` header pointing at the canonical GET URI for the new id, and include the created representation in the body — use link generation (`CreatedAtAction` / named route) rather than hard-coded strings so URLs stay correct when templates change.

```csharp
// MVC
return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);

// Minimal API
return Results.Created($"/api/orders/{order.Id}", order);
// Prefer: Results.CreatedAtRoute("GetOrderById", new { id = order.Id }, order);
```

- `CreatedAtAction` resolves controller action URL from routing table — ensures `Location` matches `[HttpGet("{id:int}")]` template.
- Body should be the created DTO, not empty — clients avoid a follow-up GET for confirmation.
- Status must be **201**, not 200 — caches and HTTP semantics depend on it.
- Minimal APIs: named routes (`WithName("GetOrderById")`) enable stable link generation for tests and HATEOAS.

**Production takeaway:** 201 is a contract — wrong or missing `Location` breaks SDKs and idempotent retry logic.

---

#### Q2. (R) Review this POST endpoint. Integration tests report broken `Location` in production behind nginx.

```csharp
[HttpPost]
public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
{
    var order = await _service.CreateAsync(request, ct);
    return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
}

[HttpGet("{id:int}")]
public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken ct)
    => Ok(await _service.GetAsync(id, ct));
```

**Answer:** `CreatedAtAction` builds URLs from `HttpContext.Request` scheme/host — without forwarded headers applied **before** routing and URL generation, `Location` uses `http://` and internal hostnames behind TLS-terminating nginx.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Hosting | `UseForwardedHeaders` after `MapControllers` | `Request.Scheme` stays `http`; wrong `Location` scheme |
| Link generation | Hard dependency on correct `Host`/`Scheme` | Clients follow `http://` link — mixed content or wrong host |
| Routing | Action names match — not the bug | Masks real proxy configuration issue in local tests |

**Fix (priority order):**

1. `app.UseForwardedHeaders()` early — before HTTPS redirection, auth, and endpoint execution (configure `ForwardedHeadersOptions` with trusted proxies).
2. Set `KnownProxies` / `KnownNetworks` in production — do not accept headers from arbitrary clients.
3. Add integration test simulating `X-Forwarded-Proto: https` and assert `Location` header scheme.

**Production takeaway:** Link generation and redirects share the same forwarded-header requirement — see Hosting folder for full nginx setup.

---

#### Q3. (M) A codebase mixes attribute routes on controllers with conventional `MapControllerRoute`. Explain how endpoint routing merges them, and when you would standardize on one approach for a greenfield API.

**Answer:** Endpoint routing collects **all** endpoints (attribute-mapped actions and conventional routes) into one route table at startup — attribute routes typically win specificity for API controllers; conventional routes remain for legacy MVC views.

- Attribute routing: `[Route("api/[controller]")]` + `[HttpGet("{id}")]` — explicit, version-friendly, best for REST APIs.
- Conventional: `{controller}/{action}/{id?}` — convention over configuration for server-rendered MVC.
- Both register into `EndpointDataSource`; matching picks the best template by precedence (literal segments beat parameters; longer templates beat shorter).
- Greenfield REST API: **attribute only**, disable unused conventional routes to avoid accidental exposure (`MapControllers()` without default `{controller}/{action}` pattern).
- Minimal APIs add a third style — keep URL design consistent across controllers and `MapGroup` endpoints.

**Production takeaway:** Mixed routing confuses link generation and security audits — APIs should be attribute- or group-based exclusively.

---

#### Q4. (R) Review these two controller actions. `GET /api/products/sale` intermittently returns the wrong handler's response.

```csharp
[HttpGet("{category}")]
public IActionResult GetByCategory(string category) => Ok(_repo.ByCategory(category));

[HttpGet("sale")]
public IActionResult GetSaleItems() => Ok(_repo.SaleItems());
```

**Answer:** The `{category}` route is more general and may capture `"sale"` as a category string before the literal `sale` route is considered — literal segments must be ordered or placed so they take precedence over parameter routes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | `{category}` matches `"sale"` | `GetByCategory("sale")` runs instead of `GetSaleItems` |
| API contract | Ambiguous URL semantics | Clients see category filter results, not sale items |
| Design | Literal and parameter at same level | Classic REST routing footgun |

**Fix (priority order):**

1. Move literal route first in source (ASP.NET Core prefers more specific templates — but `[HttpGet("sale")]` should beat `{category}` when both on same controller; verify with `[HttpGet("sale")]` **above** or use constraint `[HttpGet("{category:regex(^(?!sale$).+)})]` — simplest fix: rename to `[HttpGet("categories/{category}")]`.
2. Best: `[HttpGet("~/api/products/sale")]` or separate static path `[Route("sale")]` on dedicated action with ordering verified via `dotnet run` endpoint debug.
3. Add integration test: `GET /api/products/sale` must hit sale endpoint.

**Production takeaway:** Parameter routes swallow literal path segments — Karat tests route specificity, not memorizing attribute order trivia.

---

#### Q5. (R) Review route constraints and binding. Clients call `GET /api/users/not-a-guid` and receive 500 instead of 404.

```csharp
[HttpGet("{id:guid}")]
public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken ct)
{
    var user = await _users.FindAsync(id, ct);
    if (user is null) return NotFound();
    return Ok(user);
}
```

**Answer:** With `{id:guid}`, invalid GUID strings should fail route matching (404) — a 500 `FormatException` means the constraint is missing on the matched route, a global filter throws on binding failure, or a duplicate route without `:guid` is matching first.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Route constraint | Constraint not applied on matched endpoint | Default `{id}` route binds string → Guid conversion throws |
| Error handling | Exception middleware converts binding to 500 | Clients cannot distinguish bad input from server fault |
| API design | Missing `[ApiController]` automatic 400 | Should return 400 ProblemDetails for failed binding when action is reached |

**Fix (priority order):**

1. Ensure only `[HttpGet("{id:guid}")]` exists — remove duplicate `[HttpGet("{id}")]`.
2. Enable route constraint rejection: invalid format should not match — verify with `dotnet --urls` and endpoint listing (`/debug/endpoint` in dev).
3. Return ProblemDetails for model binding failures when actions accept loose templates.

**Production takeaway:** Route constraints are the first line of defense — without them, bad URLs become 500s and pollute error budgets.

---

#### Q6. (P) An SPA behind Cloudflare calls your API; pagination links use `http://` while the browser page is `https://`. Which routing/link-generation inputs must be correct, and where does forwarded header middleware belong?

**Answer:** URL generation reads `HttpContext.Request.Scheme`, `Host`, and `PathBase` — behind Cloudflare or nginx you must configure `ForwardedHeadersOptions` (`X-Forwarded-Proto`, `X-Forwarded-Host`) and call `UseForwardedHeaders()` before any middleware that generates links or redirects.

- Configure trusted proxy IPs (Cloudflare ranges or nginx node) — `KnownNetworks` / `ForwardLimit`.
- Middleware order: `UseForwardedHeaders` → `UseHttpsRedirection` → `UseAuthentication` → `MapControllers` / minimal endpoints.
- For link generation in services without `HttpContext`, inject `IHttpContextAccessor` or pass `LinkGenerator` with explicit `HttpContext`.
- Set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` in some hosting templates as a reminder.
- Test: assert `IUrlHelper.Action` and `LinkGenerator.GetUriByName` emit `https://` in staging behind load balancer.

**Production takeaway:** Routing produces paths; hosting context produces scheme/host — both must be correct for HATEOAS and `Location` headers.

---

#### Q7. (R) Review this minimal API layout. `GET /api/v2/customers/5/invoices` returns 404 but `GET /api/customers/5/invoices` works.

```csharp
var api = app.MapGroup("/api");
var customers = api.MapGroup("/customers");
customers.MapGet("/{id:int}", (int id) => Results.Ok(GetCustomer(id)));

var v2 = api.MapGroup("/v2");
var v2Customers = v2.MapGroup("/customers");
v2Customers.MapGet("/{id:int}/invoices", (int id) => Results.Ok(GetInvoices(id)));
```

**Answer:** The v2 invoices route is registered at `/api/v2/customers/{id}/invoices`, not under the unversioned customers group — if clients call `/api/customers/5/invoices`, no endpoint matches unless you add that route explicitly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Route groups | Prefix stacking — `api` + `v2` + `customers` | Full path is `/api/v2/customers/{id}/invoices` only |
| Versioning | Client calls unversioned URL | 404 — not a bug in group registration but contract mismatch |
| Documentation | OpenAPI may list wrong base path | SDK generated against `/api/customers/...` fails |

**Fix (priority order):**

1. Align client URL with registered template or add parallel route on `customers` group: `customers.MapGet("/{id:int}/invoices", ...)`.
2. Document version prefix in OpenAPI `servers` and route group metadata.
3. Use `.WithTags("Customers v2")` and integration tests per group prefix.

**Production takeaway:** `MapGroup` prefixes compose — Karat tests whether you can read the final template, not just nested group syntax.

---

#### Q8. (M) Two endpoints match the same HTTP method and path template after a refactor — startup throws `AmbiguousMatchException`. What tools surface the conflict, and how do precedence rules work?

**Answer:** Duplicate method + template registrations cause ambiguous matching at runtime (or startup validation in some versions) — use endpoint introspection (`MapGet` duplicate scan, `/debug/endpoint` in Development, or unit tests over `EndpointDataSource`) to list collisions before deploy.

- **Precedence:** Literal segments > parameter segments; `{id:int}` more specific than `{id}`; longer templates beat shorter; first registered wins only when specificity is equal (avoid equal templates entirely).
- Common cause: copy-paste `[HttpGet("{id}")]` on two actions, or minimal API + controller sharing same path.
- Fix: rename one route, add HTTP method constraint, or merge handlers; use `[HttpGet("{id:int}")]` vs `[HttpGet("{slug}")]` with regex constraints to disambiguate.
- CI: test that calls `GetRequiredService<EndpointDataSource>()` and asserts unique route signatures.

**Production takeaway:** Ambiguous routes are deploy blockers — catch them in CI, not in production traffic splits.

---

#### Q9. (D) A team debates `MapGet` minimal endpoints vs attribute-routed controllers for a new internal admin API. Trade-offs for link generation, filters, versioning, OpenAPI, and testability?

**Answer:** Controllers excel when you need filters, model binding, `CreatedAtAction`, and consistent `[Authorize]` attributes across many endpoints; minimal APIs suit small surface areas, AOT-friendly micro-endpoints, and vertical slice files — internal admin APIs with CRUD and authorization often stay with controllers for familiarity and filter pipelines.

| Factor | Controllers | Minimal APIs |
|---|---|---|
| Link generation | `CreatedAtAction`, `IUrlHelper` mature | Named routes + `CreatedAtRoute` — works, less discoverable |
| Filters | Action/global filters, model validation pipeline | Endpoint filters (`AddEndpointFilter`) — newer, less ecosystem |
| Versioning | Well-trodden `[ApiVersion]` patterns | `MapGroup("/v{version}")` — clean but manual |
| OpenAPI | Swashbuckle / built-in — rich | Built-in OpenAPI metadata on delegates |
| Testability | `WebApplicationFactory` + controller tests | Same factory; invoke HTTP against mapped routes |

- Choose minimal for **few, high-throughput** endpoints; controllers for **large admin CRUD** with validation attributes and consistent ProblemDetails.
- Hybrid is fine — document boundary so routing and auth policies stay coherent.

**Production takeaway:** Karat wants trade-off reasoning — neither style wins universally; link generation and auth uniformity often tip admin APIs toward controllers.

---
