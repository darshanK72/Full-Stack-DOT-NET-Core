# Routing & Endpoints — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is routing in ASP.NET Core?](#q1-what-is-routing-in-aspnet-core)
2. [Q2. What is endpoint routing?](#q2-what-is-endpoint-routing)
3. [Q3. What is the difference between attribute routing and conventional routing?](#q3-what-is-the-difference-between-attribute-routing-and-conventional-routing)
4. [Q4. How does `[Route("api/[controller]")]` work?](#q4-how-does-routeapicontroller-work)
5. [Q5. What are route constraints, and why use them?](#q5-what-are-route-constraints-and-why-use-them)
6. [Q6. What is the difference between `{id}` and `{id:int}` in a route template?](#q6-what-is-the-difference-between-id-and-idint-in-a-route-template)
7. [Q7. How does ASP.NET Core decide which endpoint handles a request?](#q7-how-does-aspnet-core-decide-which-endpoint-handles-a-request)
8. [Q8. What happens when two routes match the same request?](#q8-what-happens-when-two-routes-match-the-same-request)
9. [Q9. What is `MapControllers()`?](#q9-what-is-mapcontrollers)
10. [Q10. What is `MapGroup()` in Minimal APIs?](#q10-what-is-mapgroup-in-minimal-apis)
11. [Q11. What is link generation, and why does it matter for `CreatedAtAction`?](#q11-what-is-link-generation-and-why-does-it-matter-for-createdataction)
12. [Q12. How do you return HTTP 201 Created with a `Location` header?](#q12-how-do-you-return-http-201-created-with-a-location-header)
13. [Q13. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?](#q13-what-is-the-difference-between-createdataction-and-createdatroute)
14. [Q14. What role do forwarded headers play in URL generation behind a reverse proxy?](#q14-what-role-do-forwarded-headers-play-in-url-generation-behind-a-reverse-proxy)
15. [Q15. What is route order / route precedence?](#q15-what-is-route-order-route-precedence)
16. [Q16. How do HTTP methods map to controller actions or minimal API endpoints?](#q16-how-do-http-methods-map-to-controller-actions-or-minimal-api-endpoints)
17. [Q17. What is the difference between endpoint routing and legacy routing middleware?](#q17-what-is-the-difference-between-endpoint-routing-and-legacy-routing-middleware)
18. [Q18. What is `AmbiguousMatchException`, and what causes it?](#q18-what-is-ambiguousmatchexception-and-what-causes-it)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is routing in ASP.NET Core?

What is routing in ASP.NET Core?

**Answer:** Routing is the mechanism that maps an incoming HTTP request URL and method to the code that handles it — a controller action, a minimal API delegate, or another endpoint. In ASP.NET Core 8, routing runs early in the pipeline via endpoint routing middleware and produces an `Endpoint` with metadata (HTTP methods, authorization, constraints) before most request processing continues.

- The router compares the request path and verb against registered route templates stored in an `EndpointDataSource`.
- Matched endpoints carry metadata used later for authorization, CORS, rate limiting, and OpenAPI generation.
- Routing applies equally to MVC controllers, Razor Pages, minimal APIs, and SignalR hubs registered with `Map*`.
- If no endpoint matches, the pipeline continues without a selected endpoint and typically returns 404.

---

## Q2. What is endpoint routing?

What is endpoint routing?

**Answer:** Endpoint routing is the unified routing model introduced in ASP.NET Core 3.0 where all endpoints are collected at startup and matched in one pass before the rest of the pipeline executes endpoint-specific logic. ASP.NET Core 8 uses endpoint routing by default — `UseRouting()` selects the endpoint, and `UseEndpoints()` / `Map*` executes it.

- At startup, `MapControllers()`, `MapGet()`, and similar calls register routes into a shared endpoint table.
- `UseRouting()` runs the `EndpointRoutingMiddleware`, which sets `HttpContext.GetEndpoint()` for downstream middleware.
- Authorization middleware reads endpoint metadata (`[Authorize]`, `RequireAuthorization()`) after the endpoint is known.
- This replaces the legacy two-stage routing model where routing and dispatch were separate middleware passes.

---

## Q3. What is the difference between attribute routing and conventional routing?

What is the difference between attribute routing and conventional routing?

**Answer:** Attribute routing declares routes directly on controllers and actions with `[Route]` and `[HttpGet]` attributes, while conventional routing uses centralized route templates like `{controller=Home}/{action=Index}/{id?}` in `Program.cs`. ASP.NET Core 8 REST APIs almost always use attribute routing or minimal API `MapGroup` for explicit, version-friendly URLs.

- Attribute routing gives per-action control — `[Route("api/v1/[controller]")]` plus `[HttpGet("{id:int}")]` defines exact templates.
- Conventional routing maps URL segments to controller and action names by naming convention without per-action attributes.
- Both styles register into the same endpoint table; attribute routes typically provide more specific templates for APIs.
- Greenfield Web APIs should prefer attribute routing or `MapGroup` and omit unused conventional MVC patterns.

---

## Q4. How does `[Route("api/[controller]")]` work?

How does `[Route("api/[controller]")]` work?

**Answer:** The `[controller]` token is a route parameter replaced at startup with the controller class name minus the `Controller` suffix — `ProductsController` becomes `products` by default (case depends on URL generation settings). Combined with `[HttpGet("{id:int}")]`, a `ProductsController` exposes `GET /api/products/{id}`.

- Route tokens like `[controller]` and `[action]` are resolved from type and method names when endpoints are built.
- The replacement uses the controller name without the `Controller` suffix: `OrdersController` → `orders`.
- Token replacement happens at endpoint registration time, not per request.
- You can override casing via route options or use explicit literals (`[Route("api/products")]`) when the URL must not follow class naming.

---

## Q5. What are route constraints, and why use them?

What are route constraints, and why use them?

**Answer:** Route constraints restrict what values a route parameter may accept — for example `{id:int}` only matches integers, `{slug:alpha}` only letters. They prevent ambiguous matches, reject malformed URLs with 404 instead of binding errors, and improve route precedence during matching.

- Built-in constraints include `:int`, `:guid`, `:decimal`, `:datetime`, `:regex(...)`, and `:minlength(n)`.
- A request to `/users/not-a-guid` against `{id:guid}` fails route matching (404) rather than reaching the action.
- Constraints make one template more specific than another, helping the matcher choose the correct endpoint.
- Custom constraints implement `IRouteConstraint` and register via `RouteOptions.ConstraintMap`.

---

## Q6. What is the difference between `{id}` and `{id:int}` in a route template?

What is the difference between `{id}` and `{id:int}` in a route template?

**Answer:** `{id}` accepts any non-empty string segment, while `{id:int}` accepts only integer values that fit `int`. Without `:int`, the parameter binds as a string and the framework may attempt type conversion in model binding, which can throw or produce unexpected 500 errors on invalid input.

- `{id}` matches `abc`, `123`, and any single path segment — less selective during route matching.
- `{id:int}` rejects non-integer segments at routing time, typically returning 404 before the action runs.
- Constrained routes rank higher in precedence than unconstrained parameter routes with the same shape.
- Use the narrowest constraint that matches your domain type (`:guid` for UUIDs, `:int` for numeric keys).

---

## Q7. How does ASP.NET Core decide which endpoint handles a request?

How does ASP.NET Core decide which endpoint handles a request?

**Answer:** The endpoint matcher evaluates all registered endpoints for the request HTTP method and path, filters to those whose templates match, then selects the best match by precedence rules. The selected endpoint is stored on `HttpContext` and executed after authorization and other endpoint-aware middleware.

- Only endpoints whose HTTP method constraints include the request verb (GET, POST, etc.) are candidates.
- Template matching compares literal segments exactly and binds parameter segments to values.
- Precedence favors literal segments over parameters, constrained parameters over unconstrained, and longer templates over shorter ones.
- If two endpoints are equally specific, ASP.NET Core 8 throws `AmbiguousMatchException` at runtime.

---

## Q8. What happens when two routes match the same request?

What happens when two routes match the same request?

**Answer:** When two endpoints have equal precedence for the same method and path, the runtime throws `AmbiguousMatchException` — this is a configuration error that must be fixed before production. When one route is more specific (literal vs parameter, constraint vs open), the more specific route wins without error.

- Common causes include duplicate `[HttpGet("{id}")]` on two actions or overlapping minimal API and controller routes.
- Fix by renaming routes, adding constraints (`{id:int}` vs `{slug}`), or removing duplicate registrations.
- Use `EndpointDataSource` inspection or Development endpoint debugging to list collisions at startup or in CI.
- Never rely on declaration order alone when templates are equally specific.

---

## Q9. What is `MapControllers()`?

What is `MapControllers()`?

**Answer:** `MapControllers()` is an extension on `WebApplication` that registers endpoint routes for all controller actions decorated with attribute routing. It must be called in `Program.cs` for controller-based APIs to respond — without it, controller types exist but no HTTP endpoints are mapped.

- It scans the DI-registered `ControllerActionEndpointDataSource` and adds action endpoints to the route table.
- Works with `[ApiController]` Web APIs and traditional MVC controllers that use `[Route]` attributes.
- Does not register conventional `{controller}/{action}` routes — those require separate `MapControllerRoute` calls.
- Typically placed after `UseRouting()`, `UseAuthentication()`, and `UseAuthorization()` in the pipeline.

---

## Q10. What is `MapGroup()` in Minimal APIs?

What is `MapGroup()` in Minimal APIs?

**Answer:** `MapGroup()` creates a route prefix shared by a set of minimal API endpoints, reducing repetition and enabling group-level metadata. Calling `app.MapGroup("/api/v1/customers")` and then `.MapGet("/{id:int}", ...)` registers `GET /api/v1/customers/{id}`.

- Group prefixes compose when nested — `MapGroup("/api").MapGroup("/v1")` adds both segments.
- Groups support `.WithTags()`, `.RequireAuthorization()`, `.WithOpenApi()`, and `.AddEndpointFilter()` for all child routes.
- They replace verbose repeated path strings and mirror controller `[Route]` prefix patterns.
- Each mapped delegate in the group inherits the combined prefix automatically.

---

## Q11. What is link generation, and why does it matter for `CreatedAtAction`?

What is link generation, and why does it matter for `CreatedAtAction`?

**Answer:** Link generation builds URLs from route names, controller/action identifiers, and route values rather than hard-coded strings. `CreatedAtAction` uses link generation to populate the `Location` header on 201 Created responses with the canonical URI of the new resource.

- `CreatedAtAction(nameof(GetById), new { id = order.Id }, order)` resolves the URL from the routing table.
- Hard-coded URLs break when route templates, versioning prefixes, or host paths change.
- Link generation reads `HttpContext.Request.Scheme`, `Host`, and `PathBase` — forwarded headers must be correct behind proxies.
- Minimal APIs use named routes (`WithName("GetOrderById")`) with `Results.CreatedAtRoute` for the same behavior.

---

## Q12. How do you return HTTP 201 Created with a `Location` header?

How do you return HTTP 201 Created with a `Location` header?

**Answer:** Return status 201 with a `Location` header pointing to the new resource URI and include the created representation in the body. In MVC, use `CreatedAtAction`, `CreatedAtRoute`, or `Created`; in minimal APIs, use `Results.Created` or `Results.CreatedAtRoute`.

- MVC: `return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);`
- Minimal API: `return Results.Created($"/api/orders/{order.Id}", order);` or named route variant.
- The body should contain the created DTO so clients need not immediately re-fetch.
- Status code must be 201, not 200 — HTTP caches and REST clients depend on correct semantics.

---

## Q13. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?

What is the difference between `CreatedAtAction` and `CreatedAtRoute`?

**Answer:** `CreatedAtAction` generates a URL from a controller action name and route values, while `CreatedAtRoute` generates a URL from a named route registered in the routing table. Both return 201 with a `Location` header and response body.

- `CreatedAtAction(nameof(GetById), new { id = 5 }, dto)` targets an action on the current or specified controller.
- `CreatedAtRoute("GetOrderById", new { id = 5 }, dto)` requires a route name from `[HttpGet(Name = "...")]` or `WithName(...)`.
- Named routes decouple link generation from controller type names — useful after refactoring or with minimal APIs.
- Both rely on correct `HttpContext` scheme and host for absolute URLs in the `Location` header.

---

## Q14. What role do forwarded headers play in URL generation behind a reverse proxy?

What role do forwarded headers play in URL generation behind a reverse proxy?

**Answer:** When TLS terminates at nginx, IIS, or Cloudflare, Kestrel sees HTTP and an internal hostname unless forwarded headers are applied. `UseForwardedHeaders()` reads `X-Forwarded-Proto`, `X-Forwarded-Host`, and `X-Forwarded-For` so `HttpContext.Request.Scheme` and `Host` reflect the client-facing URL used in link generation.

- Without forwarded headers, `CreatedAtAction` and redirects emit `http://` and internal hostnames.
- `UseForwardedHeaders()` must run early — before HTTPS redirection, authentication, and endpoint execution.
- Configure `ForwardedHeadersOptions.KnownProxies` or `KnownNetworks` to trust only your edge proxies.
- Set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` in container templates as a reminder to enable processing.

---

## Q15. What is route order / route precedence?

What is route order / route precedence?

**Answer:** Route precedence determines which endpoint wins when multiple templates could match, based on specificity rules rather than source file order alone. ASP.NET Core 8 ranks literal segments above parameters, constrained parameters above unconstrained, and longer templates above shorter ones.

- `[HttpGet("sale")]` beats `[HttpGet("{category}")]` because a literal segment is more specific than a parameter.
- `{id:guid}` is more specific than `{id}` because of the constraint.
- Equal-specificity duplicates cause `AmbiguousMatchException` — order does not break ties.
- Design APIs with distinct literal paths or constraints to avoid accidental shadowing (e.g., `/products/sale` vs `/products/{category}`).

---

## Q16. How do HTTP methods map to controller actions or minimal API endpoints?

How do HTTP methods map to controller actions or minimal API endpoints?

**Answer:** HTTP verbs map through method constraints — `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`, and `[HttpPatch]` on actions, or `MapGet`, `MapPost`, etc. on minimal APIs. A single route template can support multiple methods via separate endpoints or `[HttpGet]` / `[HttpPost]` on the same path with different actions.

- The matcher requires the request method to match the endpoint HTTP method metadata.
- `[HttpGet("{id}")]` and `[HttpPut("{id}")]` on the same controller share a path but differ by verb.
- `[AcceptVerbs("GET", "HEAD")]` or `[Route(..., Order = ...)]` handle less common combinations.
- Minimal APIs map one delegate per verb — `MapGet("/items", ...)` and `MapPost("/items", ...)` are distinct endpoints.

---

## Q17. What is the difference between endpoint routing and legacy routing middleware?

What is the difference between endpoint routing and legacy routing middleware?

**Answer:** Legacy routing (pre-3.0) used `UseMvc()` with routing middleware that did not integrate endpoint metadata into authorization and other middleware. Endpoint routing separates selection (`UseRouting`) from execution (`MapControllers` / terminal middleware), letting authorization, CORS, and rate limiting inspect the matched endpoint before the handler runs.

- Legacy model ran route matching inside MVC middleware without a unified endpoint abstraction.
- Endpoint routing exposes `Endpoint` metadata to the entire pipeline via `HttpContext.GetEndpoint()`.
- ASP.NET Core 8 applications use endpoint routing exclusively — legacy patterns are obsolete.
- `UseEndpoints` was merged into `Map*` calls on `WebApplication` in the minimal hosting model.

---

## Q18. What is `AmbiguousMatchException`, and what causes it?

What is `AmbiguousMatchException`, and what causes it?

**Answer:** `AmbiguousMatchException` is thrown when two or more endpoints have identical precedence for the same HTTP method and URL path, so the matcher cannot choose a single handler. It indicates a routing configuration bug that must be resolved by deduplicating or disambiguating routes.

- Typical causes: duplicate `[HttpGet("users")]` on two actions, or minimal API plus controller sharing the same template.
- Fix by adding constraints, renaming paths, merging handlers, or removing duplicate registrations.
- Detect early by enumerating `EndpointDataSource` endpoints in a startup test or CI check.
- Unlike shadowing (where a more specific route wins silently), ambiguity is always a hard failure.

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

#### Q1. (P) When an API creates a new resource and returns HTTP 201 Created, how should it include the newly created resource and its `Location` header? Compare MVC `CreatedAtAction` with minimal API `Results.Created`.

---

**Answer:**

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

---

#### Q2. (R) Review this POST endpoint. Integration tests report `Location: http://localhost/api/orders/42` but production clients receive a broken link.

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

Production runs behind nginx TLS termination; `UseForwardedHeaders` is registered **after** `MapControllers`. Swagger "Try it" shows correct HTTPS locally.

---

**Answer:**

_Answer not found._

---

#### Q3. (M) A codebase mixes `[Route("api/[controller]")]` on controllers with `app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}")`. Explain how endpoint routing merges attribute and conventional routes, and when you would standardize on one approach for a greenfield API.

---

**Answer:**

_Answer not found._

---

#### Q4. (R) Review these two controller actions. `GET /api/products/sale` intermittently returns the wrong handler's response after deploy.

```csharp
[HttpGet("{category}")]
public IActionResult GetByCategory(string category) => Ok(_repo.ByCategory(category));

[HttpGet("sale")]
public IActionResult GetSaleItems() => Ok(_repo.SaleItems());
```

Route order looks fine in source control; both actions live on `ProductsController` with `[Route("api/[controller]")]`.

---

**Answer:**

_Answer not found._

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

Global exception middleware logs `FormatException` from model binding.

---

**Answer:**

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

---

#### Q6. (P) An SPA behind Cloudflare calls your API's OpenAPI-generated client; pagination links in responses use `http://` while the browser page is `https://`. Which routing/link-generation inputs must be correct, and where does forwarded header middleware belong?

---

**Answer:**

_Answer not found._

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

---

**Answer:**

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

---

#### Q8. (M) Two endpoints match the same HTTP method and path template after a refactor — startup throws `AmbiguousMatchException`. What tools or configuration surface the conflict, and how do route precedence and specificity resolve `{id}` vs `{id:int}` vs literal segments?

---

**Answer:**

_Answer not found._

---

#### Q9. (D) A team debates `MapGet` minimal endpoints vs attribute-routed controllers for a new internal admin API. Trade-offs for link generation, filters, versioning, OpenAPI, and testability — what would you choose and why?



**Answer:**

_Answer not found._

---
