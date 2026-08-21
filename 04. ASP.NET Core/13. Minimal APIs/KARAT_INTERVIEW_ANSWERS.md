# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md).

> **Folder:** `05. ASP.NET Core/13. Minimal APIs`

---

#### Q1. (R) Review this minimal API registration. The app starts and passes smoke tests, but order totals drift under concurrent load. What is wrong?

**Answer:** The handler keeps mutable order totals in a process-wide `Dictionary` captured by the delegate while also persisting lines through scoped `DbContext` — the in-memory aggregate is shared across all requests, is not thread-safe, and diverges from the database under concurrency.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Static-like `orderTotals` dictionary outside DI | Cross-request shared mutable state |
| Concurrency | Non-thread-safe `Dictionary` read/modify | Corrupted totals, lost updates under load |
| Correctness | Running total not sourced from DB | Drift from persisted lines; wrong financial figures |
| Scale-out | In-memory dict per process | Different totals per instance behind load balancer |

**Fix (priority order):**

1. Remove the shared dictionary; compute totals from the database (`SUM` query or domain service) or use a scoped service keyed by order id within the request only.
2. If caching is required, use `IMemoryCache` with explicit keys and expiration, or a distributed cache with version stamps — never a bare static field.
3. Return the persisted aggregate from a repository method so the response reflects committed state.

**Production takeaway:** Minimal API delegates are registered once at startup — any closed-over mutable state behaves like a singleton and will leak across users unless deliberately scoped.

---

#### Q2. (M) A teammate returns `IActionResult` from a minimal API route handler while another uses `Results.Ok()` and `TypedResults.Created()`. When does each approach fit, and what does ASP.NET Core lose when you pick the wrong one?

**Answer:** Prefer `IResult` / `TypedResults` in minimal APIs because they carry compile-time response metadata for OpenAPI and avoid allocating MVC infrastructure; `IActionResult` works via compatibility shims but sacrifices typed endpoint metadata and can produce vague Swagger schemas.

- `Results.Ok(value)` and `TypedResults.Ok<T>(T value)` implement `IResult` — the minimal hosting pipeline executes them directly without invoking MVC result executors.
- `TypedResults.Created<Uri, T>(uri, value)` preserves generic response types so `AddOpenApi` / Swashbuckle can emit accurate status codes and body schemas.
- Returning raw `IActionResult` (e.g., `new OkObjectResult(dto)`) forces the framework to adapt MVC result types — functional but weaker for source-generated OpenAPI and AOT trimming scenarios.
- Returning naked DTOs (`CustomerDto`) implicitly becomes `200 OK` — convenient but hides alternate responses (404, 422) from metadata unless `.Produces<T>()` is chained.
- Use `Results.Problem()` / `TypedResults.Problem()` for consistent RFC 7807 error bodies aligned with global exception handling.

**Production takeaway:** Karat tests whether you know minimal APIs are not "controller actions without classes" — response typing is part of the contract, not decoration.

---

#### Q3. (P) You need request-body validation on a minimal API POST without MVC controllers. How do you validate a DTO and return RFC 7807 `ProblemDetails` on failure using endpoint filters?

**Answer:** Register an endpoint filter (globally or per route) that runs after model binding, validates with `ValidationContext` or FluentValidation, and short-circuits with `Results.ValidationProblem(errors)` before the handler executes.

- Add `.AddEndpointFilter<ValidationFilter>()` on the route or `builder.Services.AddSingleton<IEndpointFilter, ValidationFilter>()` for global registration.
- In the filter, inspect `context.Arguments` for the bound DTO; run `Validator.TryValidateObject` or `_validator.ValidateAsync`.
- On failure, return `Results.ValidationProblem(dictionary, statusCode: StatusCodes.Status422UnprocessableEntity)` — clients receive field-scoped errors in ProblemDetails shape.
- Combine with `[AsParameters]` record types so query/route/body bind into one validateable parameter object.
- Do not rely on `[ApiController]` automatic 400 behavior — minimal endpoints opt in explicitly via filters or the built-in `.AddValidation()` extensions in newer templates.

```csharp
public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
{
    foreach (var arg in ctx.Arguments)
    {
        if (arg is null) continue;
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(arg, new ValidationContext(arg), results, true))
            return Results.ValidationProblem(results.ToDictionary(
                r => r.MemberNames.FirstOrDefault() ?? "", r => new[] { r.ErrorMessage! }));
    }
    return await next(ctx);
}
```

**Production takeaway:** Validation filters are the minimal-API equivalent of MVC's automatic model-state filter — skipping them means invalid payloads reach business logic silently.

---

#### Q4. (R) OpenAPI/Swagger shows every minimal endpoint as `200 OK` with an empty schema, and one POST is missing from the document entirely. Review the setup — what is wrong and how do you fix it for client generation?

**Answer:** Anonymous return types and implicit status codes prevent schema inference, and `ExcludeFromDescription()` intentionally removes the admin route — public endpoints need explicit `.Produces<T>()` / `TypedResults` and XML or `[EndpointDescription]` metadata so OpenAPI documents accurate contracts.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| OpenAPI | `MapGet` returns untyped `object` from service | Empty or generic schema; bad client codegen |
| OpenAPI | `Results.Accepted()` without `.Produces()` | Wrong status (shows 200) or missing 202 body |
| API contract | Anonymous objects / opaque service return | Breaking changes invisible to consumers |
| Intentional | `ExcludeFromDescription()` on admin route | Correct for internal ops — not a bug if deliberate |

**Fix (priority order):**

1. Return `TypedResults.Ok<ReportStatusDto>(dto)` or chain `.Produces<ReportStatusDto>(StatusCodes.Status200OK)`.
2. For accepted POST, use `.Produces(StatusCodes.Status202Accepted)` or return `TypedResults.Accepted(uri, payload)`.
3. Enable `builder.Services.AddOpenApi()` / Swashbuckle schema filters; annotate with `.WithName()`, `.WithTags("Reports")`, and `[EndpointSummary]` for discoverability.
4. Keep `ExcludeFromDescription()` only on truly internal routes; verify public surface matches product spec.

**Production takeaway:** Minimal APIs do not inherit controller conventions — every response type and status code you want documented must be declared or inferred from `TypedResults`.

---

#### Q5. (P) How do you protect a subset of minimal API endpoints with JWT bearer auth and a named authorization policy while leaving health checks anonymous? Where do you register requirements vs apply them on routes?

**Answer:** Register authentication and authorization in services, call `UseAuthentication()` then `UseAuthorization()` in the pipeline, define policies with `AddAuthorizationBuilder()`, and apply `.RequireAuthorization("PolicyName")` per route or group while leaving `/health` unannotated.

- `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` and `AddAuthorizationBuilder().AddPolicy("OrdersWrite", p => p.RequireRole("OrderWriter"))`.
- Middleware order: `UseAuthentication()` before `UseAuthorization()` before `Map*` endpoints.
- Protect routes: `app.MapPost("/orders", Handler).RequireAuthorization("OrdersWrite");` — groups inherit via `app.MapGroup("/api").RequireAuthorization();`.
- Leave health anonymous: `app.MapHealthChecks("/health").AllowAnonymous();` or simply omit authorization metadata.
- Fallback policy (`options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()`) secures everything by default — then explicitly `.AllowAnonymous()` on public endpoints.

**Production takeaway:** Minimal APIs have no `[Authorize]` attribute by default — authorization is fluent metadata on the endpoint; missing `.RequireAuthorization()` leaves routes open even when JWT is configured.

---

#### Q6. (D) The team wants URL-based API versioning (`/api/v1/...`, `/api/v2/...`) using `MapGroup` without duplicating middleware and OpenAPI tags. What structure would you use, and what breaks if v1 and v2 share the same route parameter names but different DTO shapes?

**Answer:** Nest versioned `MapGroup` chains with shared extension methods for cross-cutting filters, tag OpenAPI per version, and treat v1/v2 DTOs as separate contract types even when route templates match — reusing the same handler signature for different major versions silently breaks clients.

- Structure:

```csharp
var v1 = app.MapGroup("/api/v1/orders").WithTags("Orders v1").RequireAuthorization();
v1.MapGet("/{orderId:guid}", GetOrderV1);
var v2 = app.MapGroup("/api/v2/orders").WithTags("Orders v2").RequireAuthorization();
v2.MapGet("/{orderId:guid}", GetOrderV2);
```

- Extract shared concerns into `static RouteGroupBuilder AddOrderDefaults(this RouteGroupBuilder g) => g.AddEndpointFilter<AuditFilter>();`
- **Breakage:** v2 changes `OrderDto` shape (renamed fields, different enums) but reuses v1 handler — binders succeed while JSON contract diverges; clients on v2 receive v1 semantics.
- Prefer separate record types (`OrderV1Response`, `OrderV2Response`) and distinct handler methods even when logic delegates to shared domain services.
- Document deprecation headers (`Sunset`, `Link`) on v1 group via filter when phasing out.

**Production takeaway:** `MapGroup` organizes routes; it does not version contracts — major versions need explicit types and OpenAPI tags, not just a path prefix.

---

#### Q7. (R) Review this `Program.cs` excerpt from a production service. What maintainability, testability, and DI problems will appear as the API grows?

**Answer:** This is a god-`Program.cs` that manually constructs `DbContext`, `HttpClient`, and cache outside the container — it bypasses DI lifetimes, disposes nothing correctly, and embeds infrastructure in route lambdas, making the API untestable and fragile under configuration changes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | `new AppDbContext(...)` inside handlers | No scoped lifetime; connection leaks; untestable |
| DI | `new HttpClient()` as singleton field | Socket exhaustion (see `IHttpClientFactory`) |
| DI | `new MemoryCache` outside container | No size limits config; not injectable/mockable |
| API design | `Results.Created` with name not id | Wrong Location header; broken REST contract |
| Maintainability | All logic inline in `Program.cs` | God file; no unit test seams; merge conflicts |
| Configuration | Connection string captured at build | Stale config if reload needed; wrong in tests |

**Fix (priority order):**

1. Register `AddDbContext<AppDbContext>`, `AddMemoryCache`, `AddHttpClient("legacy", ...)` in `builder.Services`.
2. Move handlers to static or instance classes (`CustomerEndpoints.Map(app)`) injectable via DI.
3. Fix `Created` to use persisted entity id: `TypedResults.Created($"/customers/{entity.Id}", dto)`.
4. Add validation filter, exception handler, and OpenAPI metadata in extension methods.

**Production takeaway:** Minimal APIs encourage small `Program.cs` — the anti-pattern is not lambdas themselves but bypassing the same DI and hosting rules controllers follow.

---

#### Q8. (M) Minimal APIs resolve route-handler parameters from route values, body, services, and `HttpContext`. A handler injects `IOptions<FeatureFlags>` and `[FromServices] IAuditLogger` alongside `[FromBody] CreateOrderRequest`. What determines injection order and failure modes when a parameter cannot be bound?

**Answer:** The parameter binding pipeline tries explicit bind sources first (route/query/body/header attributes), then services from DI, then special types like `HttpContext` and `CancellationToken`; an ambiguous or unregistered service parameter fails at first request (or startup with validation) with a binding exception, not a 404.

- Route `{id}` binds to `int id` by name; `[FromBody]` takes JSON body once per request — duplicate body parameters are invalid.
- Service parameters resolve from `RequestServices` (request scope) — scoped services work; singleton-only capture in constructed delegates still risks captive dependencies if cached incorrectly.
- `IOptions<T>` / `IOptionsSnapshot<T>` resolve from DI — snapshot refreshes per request when options reload.
- `[FromServices]` forces DI even when name collision with route value could occur.
- Failure modes: missing required route value → 400; wrong JSON shape → 400; unregistered service → 500 at invoke time with `InvalidOperationException`; optional parameters use nullable types or default values.
- `[AsParameters]` aggregates bindable properties into one complex parameter with deterministic property binding order.

**Production takeaway:** Binding errors surface at runtime on first hit — integration tests per endpoint catch missing registrations faster than manual OpenAPI review.
