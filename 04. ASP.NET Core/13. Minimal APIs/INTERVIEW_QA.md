# Minimal APIs — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are Minimal APIs in ASP.NET Core?](#q1-what-are-minimal-apis-in-aspnet-core)
2. [Q2. How do Minimal APIs differ from controller-based APIs?](#q2-how-do-minimal-apis-differ-from-controller-based-apis)
3. [Q3. How do you define a GET endpoint with Minimal APIs?](#q3-how-do-you-define-a-get-endpoint-with-minimal-apis)
4. [Q4. What is `MapGet`, `MapPost`, `MapPut`, `MapDelete`?](#q4-what-is-mapget-mappost-mapput-mapdelete)
5. [Q5. How does parameter binding work in Minimal API route handlers?](#q5-how-does-parameter-binding-work-in-minimal-api-route-handlers)
6. [Q6. What is the difference between `Results.Ok()` and `TypedResults.Ok()`?](#q6-what-is-the-difference-between-resultsok-and-typedresultsok)
7. [Q7. What is `IResult`, and why use it?](#q7-what-is-iresult-and-why-use-it)
8. [Q8. What are endpoint filters in Minimal APIs?](#q8-what-are-endpoint-filters-in-minimal-apis)
9. [Q9. How do you add validation to a Minimal API endpoint?](#q9-how-do-you-add-validation-to-a-minimal-api-endpoint)
10. [Q10. How do you organize Minimal APIs with `MapGroup`?](#q10-how-do-you-organize-minimal-apis-with-mapgroup)
11. [Q11. How do you apply authorization to Minimal API endpoints?](#q11-how-do-you-apply-authorization-to-minimal-api-endpoints)
12. [Q12. How does OpenAPI/Swagger discover Minimal API endpoints?](#q12-how-does-openapiswagger-discover-minimal-api-endpoints)
13. [Q13. What is `ExcludeFromDescription()` used for?](#q13-what-is-excludefromdescription-used-for)
14. [Q14. When would you choose Minimal APIs over controllers?](#q14-when-would-you-choose-minimal-apis-over-controllers)
15. [Q15. When would Minimal APIs become a poor long-term choice?](#q15-when-would-minimal-apis-become-a-poor-long-term-choice)
16. [Q16. How is DI used in Minimal API handlers?](#q16-how-is-di-used-in-minimal-api-handlers)
17. [Q17. What is `AddEndpointsApiExplorer()`?](#q17-what-is-addendpointsapiexplorer)
18. [Q18. How do you return HTTP 201 Created from a Minimal API?](#q18-how-do-you-return-http-201-created-from-a-minimal-api)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are Minimal APIs in ASP.NET Core?

What are Minimal APIs in ASP.NET Core?

**Answer:** Minimal APIs are a lightweight way to define HTTP endpoints directly on the application builder without creating MVC controller classes, using `MapGet`, `MapPost`, and related extension methods to bind routes to delegates or methods in `Program.cs` or extension classes.

- They were introduced to reduce ceremony for small services, microservices, and prototypes while still running on the same Kestrel host, middleware pipeline, and dependency injection container as controller-based apps.
- A minimal endpoint is still a first-class routed endpoint in ASP.NET Core 8 — it participates in endpoint routing, authorization metadata, OpenAPI generation, and endpoint filters.
- Handlers can be lambda expressions, local functions, or static/instance methods registered through extension methods such as `TodoEndpoints.Map(app)`.
- They compile to the same hosting model as `WebApplication.CreateBuilder` — there is no separate runtime; only the API surface area is smaller.

---

## Q2. How do Minimal APIs differ from controller-based APIs?

How do Minimal APIs differ from controller-based APIs?

**Answer:** Controller-based APIs organize endpoints in classes inheriting `ControllerBase` with action methods and `[ApiController]` conventions, while Minimal APIs map routes to functions with explicit metadata and opt-in validation rather than inheriting MVC's opinionated defaults.

- Controllers get automatic model validation responses (400 ProblemDetails) through `[ApiController]`; Minimal APIs require endpoint filters or manual validation unless you add those behaviors explicitly.
- MVC uses action filters, authorization filters, and result filters; Minimal APIs use endpoint filters and middleware instead — there is no `[Authorize]` attribute unless you apply `.RequireAuthorization()` on the route.
- Controllers return `IActionResult`/`ActionResult<T>` executed by MVC infrastructure; Minimal APIs prefer `IResult`/`TypedResults`, which write responses directly without invoking MVC result executors.
- Controllers scale well for large teams with established folder conventions (`Controllers/`, `Services/`); Minimal APIs scale when you enforce the same separation through extension methods and avoid a monolithic `Program.cs`.

---

## Q3. How do you define a GET endpoint with Minimal APIs?

How do you define a GET endpoint with Minimal APIs?

**Answer:** Call `app.MapGet` with a route template and a handler delegate that returns a response type or `IResult`, registering the endpoint during application configuration after `builder.Build()`.

```csharp
app.MapGet("/weather", () => new[] { "Sunny", "Cloudy" });

app.MapGet("/items/{id:int}", (int id, IItemService items) =>
    items.Find(id) is { } item ? Results.Ok(item) : Results.NotFound());
```

- Route parameters bind by name to handler parameters when types are compatible (`{id:int}` binds to `int id`).
- Services such as `IItemService` resolve from the request scope automatically when listed as handler parameters.
- Returning a plain object implicitly produces `200 OK` with JSON serialization using the configured `System.Text.Json` options.
- Named endpoints (`.WithName("GetItem")`) support link generation for `Created`/`Accepted` responses and OpenAPI operation IDs.

---

## Q4. What is `MapGet`, `MapPost`, `MapPut`, `MapDelete`?

What is `MapGet`, `MapPost`, `MapPut`, `MapDelete`?

**Answer:** These are extension methods on `IEndpointRouteBuilder` (typically `WebApplication`) that register HTTP endpoints for a specific verb and path template, connecting them to a handler delegate and optional metadata.

- `MapGet` handles GET, `MapPost` handles POST, `MapPut` handles PUT, and `MapDelete` handles DELETE — each rejects requests using other verbs for that route unless you chain additional maps.
- They return `RouteHandlerBuilder`, which supports fluent configuration: `.Produces<T>()`, `.RequireAuthorization()`, `.WithTags()`, `.AddEndpointFilter<T>()`, and `.WithName()`.
- `MapMethods` and `Map` provide lower-level control when you need custom verbs or non-standard routing branches.
- Under endpoint routing, these calls add `Endpoint` objects to the route table consumed by the routing middleware at request time.

---

## Q5. How does parameter binding work in Minimal API route handlers?

How does parameter binding work in Minimal API route handlers?

**Answer:** The minimal hosting binder resolves handler parameters from route values, query strings, headers, the request body, dependency injection, and framework types such as `HttpContext` and `CancellationToken`, using attributes to disambiguate when multiple sources could apply.

- Route template values bind by parameter name (`/users/{userId}` → `Guid userId`); built-in route constraints (`:int`, `:guid`) validate format before the handler runs.
- `[FromBody]` binds JSON once per request; `[FromQuery]` and `[FromHeader]` select alternate sources; `[FromServices]` forces DI resolution even when a name collision exists.
- Types registered in DI (`DbContext`, repositories, `IOptions<T>`) bind from `HttpContext.RequestServices` using the per-request scope.
- `[AsParameters]` on a record or class aggregates multiple bindable properties (route + query + body) into one parameter object with predictable property-level binding.

---

## Q6. What is the difference between `Results.Ok()` and `TypedResults.Ok()`?

What is the difference between `Results.Ok()` and `TypedResults.Ok()`?

**Answer:** Both return an `IResult` that writes a 200 OK response, but `TypedResults.Ok<T>(T value)` preserves the generic response type at compile time so OpenAPI tools and source generators can infer accurate response schemas.

- `Results.Ok(dto)` works at runtime but often appears as an untyped or loosely typed schema in Swagger/OpenAPI documents.
- `TypedResults.Ok<OrderDto>(order)` emits metadata that `AddOpenApi` and Swashbuckle use to document the response body shape and status code explicitly.
- Both avoid allocating MVC `ObjectResult` infrastructure — the minimal pipeline executes `IResult.ExecuteAsync` directly.
- Prefer `TypedResults` when client code generation, contract testing, or Native AOT trimming depends on strongly typed endpoint metadata.

---

## Q7. What is `IResult`, and why use it?

What is `IResult`, and why use it?

**Answer:** `IResult` is the interface implemented by built-in minimal API response helpers (`Results`, `TypedResults`) that encapsulates how to write an HTTP response, including status code, headers, and body, without going through MVC result executors.

- Implementations such as `Ok<T>`, `NotFound`, `ValidationProblem`, and `Redirect` know how to serialize themselves through `ExecuteAsync(HttpContext)`.
- Returning `IResult` makes alternate response shapes explicit in the handler signature (`Task<IResult>`), which improves readability compared to magic implicit status codes.
- Chaining `.Produces<T>(StatusCodes.Status200OK)` on the route adds metadata even when the handler returns a custom `IResult` implementation.
- Custom types can implement `IResult` for specialized response formatting while staying compatible with the minimal hosting pipeline.

---

## Q8. What are endpoint filters in Minimal APIs?

What are endpoint filters in Minimal APIs?

**Answer:** Endpoint filters are hooks that run immediately before and after a minimal API route handler, similar to action filters in MVC but scoped to a single endpoint or group, enabling validation, logging, and short-circuiting without global middleware.

- Register with `.AddEndpointFilter<ValidationFilter>()` on a route or `MapGroup`, or register a global filter through DI as `IEndpointFilter`.
- Filters receive `EndpointFilterInvocationContext` with bound arguments and can return a result early (for example `Results.ValidationProblem`) without calling the next delegate.
- They run after routing and model binding but before the handler executes, making them the idiomatic place for request validation in Minimal APIs.
- Unlike middleware, endpoint filters see the specific bound parameters for that route and can access endpoint metadata such as authorization requirements.

---

## Q9. How do you add validation to a Minimal API endpoint?

How do you add validation to a Minimal API endpoint?

**Answer:** Minimal APIs do not automatically validate models the way `[ApiController]` does, so you add validation through endpoint filters, third-party libraries such as FluentValidation, or built-in validation extensions that inspect bound parameters before the handler runs.

- An endpoint filter can iterate `context.Arguments`, run `Validator.TryValidateObject` or FluentValidation's `ValidateAsync`, and return `Results.ValidationProblem(errors)` on failure.
- Data annotations on DTOs work when you explicitly invoke validation — they are not enforced unless a filter or helper calls a validator.
- ASP.NET Core 8 templates may include `.AddValidation()` extensions that wire common validation behavior for minimal endpoints.
- Combine validation filters with `[AsParameters]` record types so route, query, and body fields validate as one cohesive request object.

---

## Q10. How do you organize Minimal APIs with `MapGroup`?

How do you organize Minimal APIs with `MapGroup`?

**Answer:** `MapGroup` creates a route prefix and shared configuration for related endpoints, letting you apply tags, authorization, filters, and OpenAPI metadata once instead of repeating it on every route.

```csharp
var todos = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .RequireAuthorization();

todos.MapGet("/", GetAll);
todos.MapPost("/", Create);
todos.MapGet("/{id:int}", GetById);
```

- Groups inherit fluent metadata — `.RequireAuthorization("PolicyName")` on the group protects all mapped child routes unless a route calls `.AllowAnonymous()`.
- Nested groups compose for versioning (`/api/v1`, `/api/v2`) or domain boundaries (`/api/billing`, `/api/inventory`).
- Extract group registration into static extension methods (`TodoEndpoints.Map(app)`) to keep `Program.cs` readable as the API grows.
- `MapGroup` organizes URLs and cross-cutting concerns; it does not version contracts — separate DTO types per major version remain necessary.

---

## Q11. How do you apply authorization to Minimal API endpoints?

How do you apply authorization to Minimal API endpoints?

**Answer:** Register authentication and authorization services, place `UseAuthentication()` and `UseAuthorization()` in the middleware pipeline, define policies with `AddAuthorizationBuilder()`, and call `.RequireAuthorization()` or `.RequireAuthorization("PolicyName")` on routes or groups.

- JWT bearer example: `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` plus `AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("Admin"))`.
- Apply per route: `app.MapDelete("/admin/users/{id}", Handler).RequireAuthorization("AdminOnly");`
- Public endpoints omit authorization metadata or call `.AllowAnonymous()` when a fallback policy requires authentication globally.
- Middleware order matters: authentication and authorization must run after routing (`UseRouting`) and before the endpoint executes in ASP.NET Core 8.

---

## Q12. How does OpenAPI/Swagger discover Minimal API endpoints?

How does OpenAPI/Swagger discover Minimal API endpoints?

**Answer:** Register `AddEndpointsApiExplorer()` (for Swashbuckle) or `AddOpenApi()` (built-in .NET 8 OpenAPI support), map endpoints with discoverable metadata, and use `TypedResults` plus `.Produces<T>()` so schema generators infer request and response types.

- `WithName`, `WithTags`, `WithSummary`, and `WithDescription` attach documentation metadata consumed by OpenAPI generators.
- Returning opaque types or untyped `object` yields empty or generic schemas — `TypedResults.Ok<CustomerDto>(dto)` fixes inference for client code generation.
- Swashbuckle requires `AddSwaggerGen()` and middleware (`UseSwagger`, `UseSwaggerUI`); .NET 8's `MapOpenApi()` serves the document without Swashbuckle if configured.
- Internal routes can opt out with `.ExcludeFromDescription()` so they do not appear in the public API document.

---

## Q13. What is `ExcludeFromDescription()` used for?

What is `ExcludeFromDescription()` used for?

**Answer:** `ExcludeFromDescription()` marks a minimal API endpoint so OpenAPI/Swagger generators omit it from the published API description, which is useful for internal diagnostics, health probes, or admin-only operations you do not want in client-facing contracts.

- The endpoint remains fully callable at runtime — exclusion affects documentation only, not routing or authorization.
- Typical uses include `/internal/reload-cache`, operational hooks, or duplicate routes kept for backward compatibility during migration.
- Combine with proper authorization on sensitive routes; excluding an endpoint from Swagger does not secure it.
- Controllers have analogous mechanisms (`[ApiExplorerSettings(IgnoreApi = true)]`); minimal APIs use the fluent `.ExcludeFromDescription()` method.

---

## Q14. When would you choose Minimal APIs over controllers?

When would you choose Minimal APIs over controllers?

**Answer:** Choose Minimal APIs when the service is small, the team wants minimal ceremony, or you need fast iteration on a focused HTTP surface — microservices, internal tools, prototypes, and simple CRUD APIs are common fits.

- Fewer files and no controller base-class hierarchy reduce boilerplate when endpoint count is low and cross-cutting rules are simple.
- Cloud-native and containerized workloads benefit from a compact startup path and straightforward `Program.cs` when paired with extension-method organization.
- Performance characteristics are comparable to controllers for typical JSON APIs — the choice is primarily about structure and team conventions, not raw throughput.
- Minimal APIs still support DI, middleware, auth, validation filters, and OpenAPI when configured explicitly.

---

## Q15. When would Minimal APIs become a poor long-term choice?

When would Minimal APIs become a poor long-term choice?

**Answer:** Minimal APIs become harder to maintain when the API grows large, many cross-cutting concerns stack up, or the team relies on MVC conventions for consistency — a sprawling `Program.cs` with inline lambdas and no test seams is the common failure mode.

- Large teams often prefer controllers for discoverability (`Controllers/OrdersController.cs`), consistent filter usage, and established code-review patterns.
- Complex validation, versioning, and hypermedia requirements need disciplined extension methods, filters, and DTO separation — without that discipline, minimal APIs accumulate "god file" debt faster than controllers.
- If the project already standardizes on MVC patterns (areas, view results, complex filter pipelines), forcing Minimal APIs splits conventions across services.
- Minimal APIs are not wrong at scale, but they require the same architectural boundaries as controllers — handlers in dedicated classes, filters for validation, and DI for all infrastructure.

---

## Q16. How is DI used in Minimal API handlers?

How is DI used in Minimal API handlers?

**Answer:** Handler parameters are resolved from the per-request `IServiceProvider` (`HttpContext.RequestServices`), so any service registered in DI can be injected by listing its type as a parameter — constructor injection applies to named methods, not inline lambdas that capture no service provider.

- Scoped services (`DbContext`, repositories) work correctly when resolved per invocation because each request creates a scope.
- Singleton services resolve from the root provider; avoid capturing scoped services in closures created at startup (captive dependency).
- `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` bind like any other service; snapshot refreshes per request when configuration reloads.
- For lambdas that need many dependencies, prefer static handler methods or class-based handlers registered through DI (`AddScoped<OrderHandler>()` + method group reference).

---

## Q17. What is `AddEndpointsApiExplorer()`?

What is `AddEndpointsApiExplorer()`?

**Answer:** `AddEndpointsApiExplorer()` registers the API explorer services that collect endpoint metadata (HTTP methods, routes, parameters, response types) from minimal endpoints and controllers, enabling Swashbuckle and other tools to generate OpenAPI documents.

- It implements `IApiDescriptionGroupCollectionProvider`, which Swashbuckle's `SwaggerGen` consumes to build schemas and operation lists.
- Call it in the service configuration phase: `builder.Services.AddEndpointsApiExplorer();` alongside `AddSwaggerGen()` when using Swashbuckle.
- .NET 8's built-in `AddOpenApi()` provides an alternative pipeline that also relies on endpoint metadata being present and correctly typed.
- Without an API explorer and without explicit `.Produces<T>()` metadata, minimal endpoints may appear in Swagger with incomplete or generic schemas.

---

## Q18. How do you return HTTP 201 Created from a Minimal API?

How do you return HTTP 201 Created from a Minimal API?

**Answer:** Return `Results.Created(uri, value)` or `TypedResults.Created<Uri, T>(uri, value)` from the handler, supplying the URI of the newly created resource for the `Location` header and the response body DTO.

```csharp
app.MapPost("/orders", async (CreateOrderRequest req, IOrderService svc) =>
{
    var order = await svc.CreateAsync(req);
    return TypedResults.Created($"/orders/{order.Id}", order);
});
```

- The first argument is the resource URI (string or `Uri`) placed in the `Location` header — it should identify the new entity, not the collection URL.
- The response body typically contains the created representation or a subset; status code is 201 automatically.
- Use `.WithName("CreateOrder")` and link generation helpers when the URI should be generated from route names rather than string interpolation.
- `Results.CreatedAtRoute` is available when named routes are defined, analogous to MVC's `CreatedAtAction`.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Answer:** In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing breaks endpoint-aware authorization and policy resolution.

- The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`).
- When auth runs before routing, the endpoint has not been selected yet and `[Authorize]` metadata on minimal routes or controllers may not apply correctly.
- Symptoms include anonymous access to protected endpoints or 401 responses without proper challenge behavior.
- Always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Answer:** Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`.

- The singleton holds one scoped instance forever instead of one per request — EF change trackers accumulate unrelated entities.
- Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup.
- Fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.
- This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Answer:** Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected.

- `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` in a singleton is an anti-pattern.
- `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly.
- Register named or typed clients: `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>();`
- Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Answer:** `IOptions<T>` captures configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled.

- `IOptionsSnapshot<T>` recalculates per request scope; `IOptionsMonitor<T>` supports change notifications via `OnChange`.
- Singleton services must use `IOptionsMonitor<T>` or read options inside scoped operations if they need live updates.
- Misconfiguration persists silently until process restart when `.Value` was cached at construction.
- See Chapter 05 for the full options lifetime comparison.

---

#### Gotcha 5. GET with `[FromBody]`

**Answer:** Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production.

- Query strings and route values are the correct binding sources for GET requests.
- Complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys.
- Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development.
- REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Answer:** ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled.

- Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero).
- Prefer standardizing clients on camelCase and documenting the contract in OpenAPI.
- Optional mitigation: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — but explicit camelCase contracts are cleaner.
- Add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Answer:** Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown.

- Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis.
- Always use `throw;` when rethrowing after logging or cleanup in a catch block.
- Wrap in a new exception only when adding context: `throw new OrderProcessingException("...", ex)` to preserve `InnerException`.
- This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Answer:** Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require.

- Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front.
- TLS certificates are easier to manage at the proxy layer with automatic renewal.
- Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.
- Containers often bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Answer:** Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts.

- Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.
- Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments.
- The file is development ergonomics, not runtime configuration.
- Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Answer:** A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics.

- PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent.
- Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.
- Create DTOs may use non-nullable bool when explicit values are always required on insert.
- Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Answer:** Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs.

- Call `UseForwardedHeaders()` early, before middleware that reads scheme or host (HTTPS redirection, link generation, rate limiting by IP).
- Configure `ForwardedHeadersOptions` to trust only your reverse proxy network — trusting all proxies enables header spoofing.
- Headers include `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`.
- Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Answer:** Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default — placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP.

- Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`.
- Sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers.
- Accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident.
- Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Answer:** SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers.

- Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`.
- Scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes.
- Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting.
- Order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Answer:** A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration.

- Hosted services live for the application lifetime — scoped dependencies must not be constructor-injected.
- Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes.
- Same rule applies to timers and `Task.Run` loops started from singletons.
- Enable `ValidateScopes` to catch this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Answer:** SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events.

- Sticky sessions keep one client on one node but do not route events raised on other nodes to that client.
- Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application.
- Raw WebSocket apps need equivalent custom pub/sub — SignalR's backplane is the built-in solution.
- Test scale-out with at least two instances before launch, not single-node staging alone.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

## Gotchas — ASP.NET Core (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this minimal API registration. The app starts and passes smoke tests, but order totals drift under concurrent load. What is wrong?

```csharp
var orderTotals = new Dictionary<int, decimal>();

app.MapPost("/orders/{id:int}/lines", async (int id, OrderLine line, AppDbContext db) =>
{
    db.OrderLines.Add(new OrderLineEntity { OrderId = id, Sku = line.Sku, Qty = line.Qty });
    await db.SaveChangesAsync();

    if (!orderTotals.ContainsKey(id))
        orderTotals[id] = 0;
    orderTotals[id] += line.UnitPrice * line.Qty;

    return Results.Ok(new { orderId = id, runningTotal = orderTotals[id] });
});
```

*(Assume `AppDbContext` is scoped and registered correctly.)*

---

**Answer:**

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

---

#### Q2. (M) A teammate returns `IActionResult` from a minimal API route handler while another uses `Results.Ok()` and `TypedResults.Created()`. When does each approach fit, and what does ASP.NET Core lose when you pick the wrong one?

---

**Answer:**

**Answer:** Prefer `IResult` / `TypedResults` in minimal APIs because they carry compile-time response metadata for OpenAPI and avoid allocating MVC infrastructure; `IActionResult` works via compatibility shims but sacrifices typed endpoint metadata and can produce vague Swagger schemas.

- `Results.Ok(value)` and `TypedResults.Ok<T>(T value)` implement `IResult` — the minimal hosting pipeline executes them directly without invoking MVC result executors.
- `TypedResults.Created<Uri, T>(uri, value)` preserves generic response types so `AddOpenApi` / Swashbuckle can emit accurate status codes and body schemas.
- Returning raw `IActionResult` (e.g., `new OkObjectResult(dto)`) forces the framework to adapt MVC result types — functional but weaker for source-generated OpenAPI and AOT trimming scenarios.
- Returning naked DTOs (`CustomerDto`) implicitly becomes `200 OK` — convenient but hides alternate responses (404, 422) from metadata unless `.Produces<T>()` is chained.
- Use `Results.Problem()` / `TypedResults.Problem()` for consistent RFC 7807 error bodies aligned with global exception handling.

**Production takeaway:** Karat tests whether you know minimal APIs are not "controller actions without classes" — response typing is part of the contract, not decoration.

---

---

#### Q3. (P) You need request-body validation on a minimal API POST without MVC controllers. How do you validate a DTO and return RFC 7807 `ProblemDetails` on failure using endpoint filters?

---

**Answer:**

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

---

#### Q4. (R) OpenAPI/Swagger shows every minimal endpoint as `200 OK` with an empty schema, and one POST is missing from the document entirely. Review the setup — what is wrong and how do you fix it for client generation?

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/reports", async (ReportRequest req, IReportService svc) =>
{
    await svc.EnqueueAsync(req);
    return Results.Accepted();
});

app.MapGet("/reports/{id:guid}", (Guid id, IReportService svc) =>
    svc.GetStatus(id)); // returns anonymous object or DTO

// Dev-only internal endpoint — should not appear in public OpenAPI
app.MapDelete("/admin/purge-cache", () => Results.NoContent())
   .ExcludeFromDescription();
```

*(Focus on metadata, response typing, and document completeness — not generic "install Swashbuckle" advice.)*

---

**Answer:**

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

---

#### Q5. (P) How do you protect a subset of minimal API endpoints with JWT bearer auth and a named authorization policy while leaving health checks anonymous? Where do you register requirements vs apply them on routes?

---

**Answer:**

**Answer:** Register authentication and authorization in services, call `UseAuthentication()` then `UseAuthorization()` in the pipeline, define policies with `AddAuthorizationBuilder()`, and apply `.RequireAuthorization("PolicyName")` per route or group while leaving `/health` unannotated.

- `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` and `AddAuthorizationBuilder().AddPolicy("OrdersWrite", p => p.RequireRole("OrderWriter"))`.
- Middleware order: `UseAuthentication()` before `UseAuthorization()` before `Map*` endpoints.
- Protect routes: `app.MapPost("/orders", Handler).RequireAuthorization("OrdersWrite");` — groups inherit via `app.MapGroup("/api").RequireAuthorization();`.
- Leave health anonymous: `app.MapHealthChecks("/health").AllowAnonymous();` or simply omit authorization metadata.
- Fallback policy (`options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()`) secures everything by default — then explicitly `.AllowAnonymous()` on public endpoints.

**Production takeaway:** Minimal APIs have no `[Authorize]` attribute by default — authorization is fluent metadata on the endpoint; missing `.RequireAuthorization()` leaves routes open even when JWT is configured.

---

---

#### Q6. (D) The team wants URL-based API versioning (`/api/v1/...`, `/api/v2/...`) using `MapGroup` without duplicating middleware and OpenAPI tags. What structure would you use, and what breaks if v1 and v2 share the same route parameter names but different DTO shapes?

---

**Answer:**

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

---

#### Q7. (R) Review this `Program.cs` excerpt from a production service. What maintainability, testability, and DI problems will appear as the API grows?

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var conn = builder.Configuration.GetConnectionString("Default");
var cache = new MemoryCache(new MemoryCacheOptions());
var http = new HttpClient();

app.MapGet("/customers/{id}", async (string id) =>
{
    if (cache.TryGetValue(id, out Customer? c)) return Results.Ok(c);
    await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(conn).Options);
    c = await db.Customers.FindAsync(id);
    cache.Set(id, c, TimeSpan.FromMinutes(10));
    return c is null ? Results.NotFound() : Results.Ok(c);
});

app.MapPost("/customers", async (CustomerDto dto) =>
{
    var resp = await http.PostAsJsonAsync("https://legacy.example/validate", dto);
    resp.EnsureSuccessStatusCode();
    await using var db = new AppDbContext(/* same inline options */);
    db.Customers.Add(new Customer { Name = dto.Name });
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{dto.Name}", dto);
});

app.Run();
```

---

**Answer:**

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

---

#### Q8. (M) Minimal APIs resolve route-handler parameters from route values, body, services, and `HttpContext`. A handler injects `IOptions<FeatureFlags>` and `[FromServices] IAuditLogger` alongside `[FromBody] CreateOrderRequest`. What determines injection order and failure modes when a parameter cannot be bound?



**Answer:**

**Answer:** The parameter binding pipeline tries explicit bind sources first (route/query/body/header attributes), then services from DI, then special types like `HttpContext` and `CancellationToken`; an ambiguous or unregistered service parameter fails at first request (or startup with validation) with a binding exception, not a 404.

- Route `{id}` binds to `int id` by name; `[FromBody]` takes JSON body once per request — duplicate body parameters are invalid.
- Service parameters resolve from `RequestServices` (request scope) — scoped services work; singleton-only capture in constructed delegates still risks captive dependencies if cached incorrectly.
- `IOptions<T>` / `IOptionsSnapshot<T>` resolve from DI — snapshot refreshes per request when options reload.
- `[FromServices]` forces DI even when name collision with route value could occur.
- Failure modes: missing required route value → 400; wrong JSON shape → 400; unregistered service → 500 at invoke time with `InvalidOperationException`; optional parameters use nullable types or default values.
- `[AsParameters]` aggregates bindable properties into one complex parameter with deterministic property binding order.

**Production takeaway:** Binding errors surface at runtime on first hit — integration tests per endpoint catch missing registrations faster than manual OpenAPI review.

---
