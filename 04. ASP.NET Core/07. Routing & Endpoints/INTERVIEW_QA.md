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

**Concepts**
- Routing mapping request URL and method to an endpoint handler
- `EndpointDataSource` holding all registered route templates
- Endpoint metadata used by authorization, CORS, and rate limiting
- No matched endpoint resulting in 404 without further processing

**Answer**

Routing is the mechanism that maps an incoming HTTP request URL and method to the code that handles it — a controller action, a minimal API delegate, or another endpoint. In ASP.NET Core 8, routing runs early in the pipeline via endpoint routing middleware and produces an `Endpoint` with metadata (HTTP methods, authorization, constraints) before most request processing continues. The router compares the request path and verb against registered route templates stored in an `EndpointDataSource`, and matched endpoints carry metadata used later for authorization, CORS, rate limiting, and OpenAPI generation. Routing applies equally to MVC controllers, Razor Pages, minimal APIs, and SignalR hubs registered with `Map*`. If no endpoint matches, the pipeline continues without a selected endpoint and typically returns 404.

---

## Q2. What is endpoint routing?

**Concepts**
- Unified routing model collecting all endpoints at startup
- `UseRouting()` selecting the endpoint — `HttpContext.GetEndpoint()` populated
- Authorization middleware reading endpoint metadata after selection
- Replacement for the legacy two-stage routing model

**Answer**

Endpoint routing is the unified routing model introduced in ASP.NET Core 3.0 where all endpoints are collected at startup and matched in one pass before the rest of the pipeline executes endpoint-specific logic. ASP.NET Core 8 uses endpoint routing by default — `UseRouting()` runs the `EndpointRoutingMiddleware` which sets `HttpContext.GetEndpoint()` for downstream middleware, so authorization, CORS, and rate limiting can inspect endpoint metadata (such as `[Authorize]` or `RequireAuthorization()`) after the endpoint is known but before the handler executes. At startup, `MapControllers()`, `MapGet()`, and similar calls register routes into a shared endpoint table. This replaces the legacy two-stage routing model where routing and dispatch were separate middleware passes.

---

## Q3. What is the difference between attribute routing and conventional routing?

**Concepts**
- Attribute routing — per-action `[Route]` and `[HttpGet]` attributes
- Conventional routing — centralized `{controller}/{action}` templates
- Both registering into the same endpoint table
- Greenfield REST APIs preferring attribute routing or `MapGroup`

**Answer**

Attribute routing declares routes directly on controllers and actions with `[Route]` and `[HttpGet]` attributes, while conventional routing uses centralized route templates like `{controller=Home}/{action=Index}/{id?}` in `Program.cs`. ASP.NET Core 8 REST APIs almost always use attribute routing or minimal API `MapGroup` for explicit, version-friendly URLs. Attribute routing gives per-action control — `[Route("api/v1/[controller]")]` plus `[HttpGet("{id:int}")]` defines exact templates — while conventional routing maps URL segments to controller and action names by naming convention without per-action attributes. Both styles register into the same endpoint table, so attribute routes typically provide more specific templates for APIs. Greenfield Web APIs should prefer attribute routing or `MapGroup` and omit unused conventional MVC patterns.

---

## Q4. How does `[Route("api/[controller]")]` work?

**Concepts**
- `[controller]` token replaced with class name minus the `Controller` suffix
- Token replacement at endpoint registration time, not per request
- `[action]` token for method names
- Explicit literal override when URL must not follow class naming

**Answer**

The `[controller]` token is a route parameter replaced at startup with the controller class name minus the `Controller` suffix — `ProductsController` becomes `products` (case depends on URL generation settings). Combined with `[HttpGet("{id:int}")]`, a `ProductsController` exposes `GET /api/products/{id}`. Route tokens like `[controller]` and `[action]` are resolved from type and method names when endpoints are built, and `OrdersController` becomes `orders`. Token replacement happens at endpoint registration time, not per request. Override casing via route options or use explicit literals (`[Route("api/products")]`) when the URL must not follow class naming conventions.

---

## Q5. What are route constraints, and why use them?

**Concepts**
- Constraints restricting what values a route parameter accepts
- Built-in constraints: `:int`, `:guid`, `:decimal`, `:datetime`, `:regex(...)`
- 404 on non-matching segments instead of reaching the action with invalid input
- Custom constraints via `IRouteConstraint` and `RouteOptions.ConstraintMap`

**Answer**

Route constraints restrict what values a route parameter may accept — for example `{id:int}` only matches integers and `{slug:alpha}` only letters. They prevent ambiguous matches, reject malformed URLs with 404 instead of binding errors, and improve route precedence during matching. A request to `/users/not-a-guid` against `{id:guid}` fails route matching and returns 404 rather than reaching the action and failing in model binding. Built-in constraints include `:int`, `:guid`, `:decimal`, `:datetime`, `:regex(...)`, and `:minlength(n)`. Constraints also make one template more specific than another, helping the matcher choose the correct endpoint. Custom constraints implement `IRouteConstraint` and register via `RouteOptions.ConstraintMap`.

---

## Q6. What is the difference between `{id}` and `{id:int}` in a route template?

**Concepts**
- `{id}` matching any non-empty string segment
- `{id:int}` accepting only integer values and failing at routing for non-integers
- Constrained routes ranking higher in precedence than unconstrained
- Narrowest constraint for domain type — `:guid` for UUIDs, `:int` for numeric keys

**Answer**

`{id}` accepts any non-empty string segment, while `{id:int}` accepts only integer values that fit `int`, since the constraint is evaluated at routing time rather than model binding. Without `:int`, the parameter binds as a string and the framework may attempt type conversion in model binding, which can throw or produce unexpected 500 errors on invalid input rather than a clean 404. `{id}` matches `abc`, `123`, and any single path segment, making it less selective during route matching. `{id:int}` rejects non-integer segments at routing time, typically returning 404 before the action runs, and constrained routes rank higher in precedence than unconstrained parameter routes with the same shape. Use the narrowest constraint that matches your domain type.

---

## Q7. How does ASP.NET Core decide which endpoint handles a request?

**Concepts**
- Endpoint matcher evaluating method and path against all registered endpoints
- Precedence rules: literals over parameters, constrained over unconstrained, longer over shorter
- `AmbiguousMatchException` when two endpoints have equal precedence
- `HttpContext` storing the selected endpoint for downstream middleware

**Answer**

The endpoint matcher evaluates all registered endpoints for the request HTTP method and path, filters to those whose templates match, then selects the best match by precedence rules. The selected endpoint is stored on `HttpContext` and executed after authorization and other endpoint-aware middleware. Only endpoints whose HTTP method constraints include the request verb are candidates. Template matching compares literal segments exactly and binds parameter segments to values. Precedence favors literal segments over parameters, constrained parameters over unconstrained, and longer templates over shorter ones. If two endpoints are equally specific, ASP.NET Core 8 throws `AmbiguousMatchException` at runtime.

---

## Q8. What happens when two routes match the same request?

**Concepts**
- `AmbiguousMatchException` when two endpoints have equal precedence
- More specific route winning silently when one has higher precedence
- Common causes: duplicate `[HttpGet("{id}")]` or overlapping minimal API and controller routes
- `EndpointDataSource` inspection for detecting collisions in CI

**Answer**

When two endpoints have equal precedence for the same method and path, the runtime throws `AmbiguousMatchException` — this is a configuration error that must be fixed before production. When one route is more specific (literal vs parameter, constraint vs open), the more specific route wins without error. Common causes include duplicate `[HttpGet("{id}")]` on two actions or overlapping minimal API and controller routes. Fix by renaming routes, adding constraints (`{id:int}` vs `{slug}`), or removing duplicate registrations. Use `EndpointDataSource` inspection or Development endpoint debugging to list collisions at startup or in CI. Never rely on declaration order alone when templates are equally specific.

---

## Q9. What is `MapControllers()`?

**Concepts**
- `MapControllers()` registering attribute-routed controller endpoints
- Scanning `ControllerActionEndpointDataSource` at call time
- Does not register conventional `{controller}/{action}` routes
- Placed after `UseAuthentication()` and `UseAuthorization()` in the pipeline

**Answer**

`MapControllers()` is an extension on `WebApplication` that registers endpoint routes for all controller actions decorated with attribute routing. It must be called in `Program.cs` for controller-based APIs to respond — without it, controller types exist in DI but no HTTP endpoints are mapped. It scans the DI-registered `ControllerActionEndpointDataSource` and adds action endpoints to the route table. It works with `[ApiController]` Web APIs and traditional MVC controllers that use `[Route]` attributes. It does not register conventional `{controller}/{action}` routes — those require separate `MapControllerRoute` calls. Typically placed after `UseRouting()`, `UseAuthentication()`, and `UseAuthorization()` in the pipeline.

---

## Q10. What is `MapGroup()` in Minimal APIs?

**Concepts**
- Route prefix shared by a set of minimal API endpoints
- Group-level metadata: `.WithTags()`, `.RequireAuthorization()`, `.AddEndpointFilter()`
- Nested groups composing prefixes
- Mirror of controller `[Route]` prefix patterns

**Answer**

`MapGroup()` creates a route prefix shared by a set of minimal API endpoints, reducing repetition and enabling group-level metadata. Calling `app.MapGroup("/api/v1/customers")` and then `.MapGet("/{id:int}", ...)` registers `GET /api/v1/customers/{id}`. Group prefixes compose when nested — `MapGroup("/api").MapGroup("/v1")` adds both segments. Groups support `.WithTags()`, `.RequireAuthorization()`, `.WithOpenApi()`, and `.AddEndpointFilter()` for all child routes, which replaces verbose repeated path strings and mirrors controller `[Route]` prefix patterns. Each mapped delegate in the group inherits the combined prefix automatically.

---

## Q11. What is link generation, and why does it matter for `CreatedAtAction`?

**Concepts**
- Link generation building URLs from route names and values, not hard-coded strings
- `CreatedAtAction` using link generation to populate the `Location` header
- Forwarded headers required for correct scheme and host in generated URLs
- Minimal API named routes with `WithName()` for `Results.CreatedAtRoute`

**Answer**

Link generation builds URLs from route names, controller/action identifiers, and route values rather than hard-coded strings. `CreatedAtAction` uses link generation to populate the `Location` header on 201 Created responses with the canonical URI of the new resource, so `CreatedAtAction(nameof(GetById), new { id = order.Id }, order)` resolves the URL from the routing table rather than concatenating strings. Hard-coded URLs break when route templates, versioning prefixes, or host paths change. Link generation reads `HttpContext.Request.Scheme`, `Host`, and `PathBase` — forwarded headers must be correct behind proxies or the generated URL will use `http://` and an internal hostname. Minimal APIs use named routes (`WithName("GetOrderById")`) with `Results.CreatedAtRoute` for the same behavior.

---

## Q12. How do you return HTTP 201 Created with a `Location` header?

**Concepts**
- 201 status code with `Location` header pointing to the new resource URI
- `CreatedAtAction` or `CreatedAtRoute` in MVC
- `Results.Created` or `Results.CreatedAtRoute` in Minimal APIs
- Body containing the created DTO to avoid a follow-up GET

**Answer**

Return status 201 with a `Location` header pointing to the new resource URI and include the created representation in the body. In MVC, use `CreatedAtAction`, `CreatedAtRoute`, or `Created`; in minimal APIs, use `Results.Created` or `Results.CreatedAtRoute`. `return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity)` uses link generation so the URL stays correct when templates change. The body should contain the created DTO so clients need not immediately re-fetch. Status code must be 201, not 200 — HTTP caches, REST clients, and generated SDKs depend on correct semantics for idempotent retry and resource location.

---

## Q13. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?

**Concepts**
- `CreatedAtAction` generating URL from controller action name and route values
- `CreatedAtRoute` generating URL from a named route
- Named routes decoupling link generation from controller type names
- Both relying on correct `HttpContext` scheme and host for absolute URLs

**Answer**

`CreatedAtAction` generates a URL from a controller action name and route values, while `CreatedAtRoute` generates a URL from a named route registered in the routing table. Both return 201 with a `Location` header and response body. `CreatedAtAction(nameof(GetById), new { id = 5 }, dto)` targets an action on the current or specified controller, while `CreatedAtRoute("GetOrderById", new { id = 5 }, dto)` requires a route name from `[HttpGet(Name = "...")]` or `WithName(...)`. Named routes decouple link generation from controller type names, which is useful after refactoring or with minimal APIs. Both rely on correct `HttpContext` scheme and host for absolute URLs in the `Location` header, so forwarded headers must be configured behind a proxy.

---

## Q14. What role do forwarded headers play in URL generation behind a reverse proxy?

**Concepts**
- `X-Forwarded-Proto` and `X-Forwarded-Host` correcting scheme and hostname
- `UseForwardedHeaders()` required early before HTTPS redirection and endpoint execution
- `KnownProxies` / `KnownNetworks` limiting trusted sources
- Wrong scheme causing `http://` in `Location` headers and redirect loops

**Answer**

When TLS terminates at nginx, IIS, or Cloudflare, Kestrel sees HTTP and an internal hostname unless forwarded headers are applied. `UseForwardedHeaders()` reads `X-Forwarded-Proto`, `X-Forwarded-Host`, and `X-Forwarded-For` so `HttpContext.Request.Scheme` and `Host` reflect the client-facing URL used in link generation. Without forwarded headers, `CreatedAtAction` and redirects emit `http://` and internal hostnames, breaking HTTPS enforcement and client-side navigation. `UseForwardedHeaders()` must run early — before HTTPS redirection, authentication, and endpoint execution. Configure `ForwardedHeadersOptions.KnownProxies` or `KnownNetworks` to trust only your edge proxies, since trusting all proxies enables header spoofing.

---

## Q15. What is route order / route precedence?

**Concepts**
- Specificity rules: literals > constrained parameters > unconstrained parameters
- Longer templates beating shorter templates at the same specificity
- Equal-specificity duplicates causing `AmbiguousMatchException`
- Source file order not breaking ties — only specificity matters

**Answer**

Route precedence determines which endpoint wins when multiple templates could match, based on specificity rules rather than source file order alone. ASP.NET Core 8 ranks literal segments above parameters, constrained parameters above unconstrained, and longer templates above shorter ones. `[HttpGet("sale")]` beats `[HttpGet("{category}")]` because a literal segment is more specific than a parameter, and `{id:guid}` is more specific than `{id}` because of the constraint. Equal-specificity duplicates cause `AmbiguousMatchException` — order does not break ties. Design APIs with distinct literal paths or constraints to avoid accidental shadowing such as `/products/sale` being shadowed by `/products/{category}`.

---

## Q16. How do HTTP methods map to controller actions or minimal API endpoints?

**Concepts**
- `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`, `[HttpPatch]` on actions
- `MapGet`, `MapPost`, etc. on minimal APIs — one delegate per verb
- Shared path with different verbs requiring separate endpoints
- `[AcceptVerbs]` for less common verb combinations

**Answer**

HTTP verbs map through method constraints — `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`, and `[HttpPatch]` on actions, or `MapGet`, `MapPost`, etc. on minimal APIs. The matcher requires the request method to match the endpoint HTTP method metadata, so `[HttpGet("{id}")]` and `[HttpPut("{id}")]` on the same controller share a path but differ by verb and are distinct endpoints. `[AcceptVerbs("GET", "HEAD")]` or `[Route(..., Order = ...)]` handle less common combinations. Minimal APIs map one delegate per verb — `MapGet("/items", ...)` and `MapPost("/items", ...)` are distinct endpoints in the same group, and a single route template can support multiple methods via separate endpoint registrations.

---

## Q17. What is the difference between endpoint routing and legacy routing middleware?

**Concepts**
- Legacy routing embedding route matching inside `UseMvc()` middleware
- Endpoint routing separating selection (`UseRouting`) from execution
- `HttpContext.GetEndpoint()` exposing endpoint metadata to the full pipeline
- `UseEndpoints` merged into `Map*` calls on `WebApplication` in minimal hosting

**Answer**

Legacy routing (pre-3.0) used `UseMvc()` with routing middleware that did not integrate endpoint metadata into authorization and other middleware, so middleware could not inspect which endpoint was selected. Endpoint routing separates selection (`UseRouting`) from execution (`MapControllers` / terminal middleware), letting authorization, CORS, and rate limiting inspect the matched endpoint via `HttpContext.GetEndpoint()` before the handler runs. Legacy model ran route matching inside MVC middleware without a unified endpoint abstraction, which meant `[Authorize]` policy resolution could only happen inside MVC. ASP.NET Core 8 applications use endpoint routing exclusively — `UseEndpoints` was merged into `Map*` calls on `WebApplication` in the minimal hosting model.

---

## Q18. What is `AmbiguousMatchException`, and what causes it?

**Concepts**
- `AmbiguousMatchException` thrown when two endpoints have identical precedence
- Equal-specificity duplicates as configuration bugs
- `EndpointDataSource` inspection for early detection
- Specificity vs shadowing — ambiguity is always a hard failure

**Answer**

`AmbiguousMatchException` is thrown when two or more endpoints have identical precedence for the same HTTP method and URL path, so the matcher cannot choose a single handler. It indicates a routing configuration bug that must be resolved by deduplicating or disambiguating routes. Typical causes include duplicate `[HttpGet("users")]` on two actions, or a minimal API and a controller sharing the same template. Fix by adding constraints, renaming paths, merging handlers, or removing duplicate registrations. Detect early by enumerating `EndpointDataSource` endpoints in a startup test or CI check. Unlike shadowing (where a more specific route wins silently), ambiguity is always a hard failure that surfaces at runtime.

---

## Gotchas — ASP.NET Core (Interview Traps)

---

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- `UseRouting` before `UseAuthentication` and `UseAuthorization`
- Endpoint metadata availability for auth middleware
- Correct pipeline order in `Program.cs`

**Answer**

In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing means the endpoint has not been selected yet, which breaks endpoint-aware authorization and policy resolution. The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`). Symptoms of wrong order include anonymous access to protected endpoints or 401 responses without proper challenge behavior, so always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive `DbContext` living past its scope
- Stale EF change tracker accumulating unrelated entities
- `ValidateScopes` as the detection mechanism

**Answer**

Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`. The singleton holds one scoped instance forever instead of one per request, so EF change trackers accumulate unrelated entities across requests. Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup, and fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- Socket exhaustion from per-use `HttpClient` instantiation
- `HttpMessageHandler` lifecycle managed by `IHttpClientFactory`
- Named and typed client registration pattern

**Answer**

Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected. `HttpClient` is disposable but not meant for per-use disposal — the OS connection handle is held by the handler, not the client object. `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly; register named or typed clients with `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()`. Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- `IOptions<T>` — fixed snapshot at first resolution
- `IOptionsSnapshot<T>` — per-request recalculation, scoped
- `IOptionsMonitor<T>` — singleton-safe with change notifications
- Stale configuration when `.Value` is cached in a constructor field

**Answer**

`IOptions<T>` captures a configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled. `IOptionsSnapshot<T>` recalculates per request scope so a singleton cannot inject it without creating a captive dependency. `IOptionsMonitor<T>` is the singleton-safe wrapper that supports change notifications via `OnChange` and exposes `CurrentValue` for the latest merged configuration.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET body stripped by clients, proxies, and CDNs
- `[FromQuery]` with `[AsParameters]` for complex GET filters
- Silent binding failure rather than explicit error

**Answer**

Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production. Query strings and route values are the correct binding sources for GET requests, and complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys. REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- Default `JsonNamingPolicy.CamelCase` in ASP.NET Core 8
- Silent binding to default values on case mismatch
- `PropertyNameCaseInsensitive` as a compatibility bridge

**Answer**

ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys may not bind unless case-insensitive matching is enabled. Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values. Prefer standardizing clients on camelCase and documenting the contract in OpenAPI, and add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- `throw ex` resetting the stack trace to the catch block
- `throw;` preserving the original stack trace
- `InnerException` preservation when wrapping in a new exception

**Answer**

Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown. Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis, so always use `throw;` when rethrowing after logging or cleanup in a catch block. Wrap in a new exception only when adding context — `throw new OrderProcessingException("...", ex)` — to preserve `InnerException`.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel as application server vs full edge gateway
- TLS termination, WAF, and rate limiting at the reverse proxy
- `UseForwardedHeaders` required for accurate client IP and scheme

**Answer**

Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require. Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front. Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- `launchSettings.json` as development-only launch configuration
- Production host using environment variables, not launch profiles
- `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS` as runtime configuration

**Answer**

Settings in `Properties/launchSettings.json` apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts. Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings. Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- Non-nullable `bool` defaulting to `false` on JSON omission
- Three-state intent: unspecified, opt-in, opt-out
- `bool?` or enum tri-state for partial-update DTOs

**Answer**

A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics. PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent. Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- `X-Forwarded-Proto` and `X-Forwarded-For` headers
- Wrong scheme causing broken HTTPS redirects and cookie secure flags
- `KnownProxies` configuration to prevent header spoofing

**Answer**

Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs. Call `UseForwardedHeaders()` early, before middleware that reads scheme or host. Configure `ForwardedHeadersOptions` to trust only your reverse proxy network since trusting all proxies enables header spoofing.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- `UseStaticFiles()` serving all `wwwroot` contents unauthenticated
- Secrets and config files must stay outside the web root
- Build pipeline verification before deploy

**Answer**

Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default, so placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP. Only public assets belong in `wwwroot`, while sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers. Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback returning `index.html` for unmatched routes including `/api/*`
- API endpoint registration ordering before fallback
- CORS and Swagger failures masked by HTML responses

**Answer**

SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers. Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`, and scope fallback to non-API paths. The correct order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- Singleton `BackgroundService` incompatible with constructor-injected scoped services
- `CreateAsyncScope()` per job to create a fresh scope
- `ValidateScopes` catching this defect at startup

**Answer**

A singleton `BackgroundService` that injects scoped services directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration. Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes. Enabling `ValidateScopes` catches this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR hub broadcasting to connected clients on the same instance only
- Redis or Azure Service Bus backplane for multi-node event routing
- Sticky sessions insufficient without a backplane

**Answer**

SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane, users on different nodes never receive each other's real-time events. Sticky sessions keep one client on one node but do not route events raised on other nodes to that client. Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application, and test scale-out with at least two instances before launch.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (P) When an API creates a new resource and returns HTTP 201 Created, how should it include the newly created resource and its `Location` header? Compare MVC `CreatedAtAction` with minimal API `Results.Created`.

**Concepts**
- 201 as a contract — wrong or missing `Location` breaking SDKs and retry logic
- `CreatedAtAction` resolving URL from the routing table via link generation
- `Results.CreatedAtRoute` with named routes for stable minimal API URLs
- Body containing the created DTO to avoid follow-up GET

**Answer**

Return 201 with a `Location` header pointing at the canonical GET URI for the new resource and include the created representation in the body — use link generation rather than hard-coded strings so URLs stay correct when templates change.

```csharp
// MVC
return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);

// Minimal API
return Results.Created($"/api/orders/{order.Id}", order);
// Prefer: Results.CreatedAtRoute("GetOrderById", new { id = order.Id }, order);
```

`CreatedAtAction` resolves the controller action URL from the routing table, ensuring the `Location` matches the `[HttpGet("{id:int}")]` template even if the prefix changes. The body should be the created DTO so clients avoid a follow-up GET for confirmation, and the status must be 201, not 200. Minimal APIs should use named routes (`WithName("GetOrderById")`) for stable link generation in tests and HATEOAS scenarios.

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

**Concepts**
- `UseForwardedHeaders` must run before endpoint execution to affect link generation
- Middleware order bug causing `http://` scheme in `Location` header
- `X-Forwarded-Proto` correcting scheme seen by `HttpContext.Request.Scheme`

**Answer**

`CreatedAtAction` generates the `Location` header from `HttpContext.Request.Scheme` and `Host`, so when nginx terminates TLS the Kestrel-facing request arrives as HTTP with an internal hostname. Forwarded headers must be processed before link generation runs — registering `UseForwardedHeaders` after `MapControllers` means the endpoint executes first with the uncorrected scheme, so the generated URL is `http://internal-host/...` instead of `https://api.example.com/...`. Move `app.UseForwardedHeaders()` to run early in the pipeline, before `UseRouting`, `UseAuthentication`, and `MapControllers`, and configure `ForwardedHeadersOptions.KnownProxies` with the nginx IP to prevent header spoofing.

---

#### Q3. (M) A codebase mixes `[Route("api/[controller]")]` on controllers with `app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}")`. Explain how endpoint routing merges attribute and conventional routes, and when you would standardize on one approach for a greenfield API.

**Concepts**
- Attribute routes and conventional routes registering into the same endpoint table
- Attribute routes typically more specific so they win over conventional
- Conventional routes needed for Razor Pages or MVC UI, not REST APIs
- Greenfield REST APIs standardizing on attribute routing or `MapGroup`

**Answer**

Both attribute routes and conventional routes register into the same `EndpointDataSource` at startup, so the matcher considers them together. Attribute routes generally produce more specific templates (literal segments plus constraints) so they win over the open `{controller}/{action}` conventional template when both could match a URL. In a codebase mixing both, a `POST /api/orders` from an `[HttpPost]` on `OrdersController` uses the attribute route while a `GET /Home/Index` falls through to the conventional route. For a greenfield REST API there is no reason to keep conventional routing — standardize on attribute routing (`[Route("api/[controller]")]` + verb attributes) or minimal API `MapGroup` since it makes the URL contract explicit, supports versioning prefixes, and avoids the ambiguity of `{controller}/{action}` accidentally matching API paths.

---

#### Q4. (R) Review these two controller actions. `GET /api/products/sale` intermittently returns the wrong handler's response after deploy.

```csharp
[HttpGet("{category}")]
public IActionResult GetByCategory(string category) => Ok(_repo.ByCategory(category));

[HttpGet("sale")]
public IActionResult GetSaleItems() => Ok(_repo.SaleItems());
```

Route order looks fine in source control; both actions live on `ProductsController` with `[Route("api/[controller]")]`.

**Concepts**
- Literal segment `"sale"` having higher precedence than parameter `{category}`
- Precedence rules resolving correctly but deployment order varying between builds
- `AmbiguousMatchException` not thrown because specificities differ
- Verify correct behavior with an integration test asserting `/api/products/sale` hits `GetSaleItems`

**Answer**

Precedence rules say the literal `[HttpGet("sale")]` is more specific than the parameterized `[HttpGet("{category}")]`, so `GET /api/products/sale` should always route to `GetSaleItems`. The "intermittent wrong response" is almost certainly a deployment artifact — both endpoints coexist correctly in the routing table, but if a prior code version did not have the `sale` literal, cached clients or a stale load-balanced node may be serving the old code that only had `{category}`. The routing table itself is deterministic — verify with an integration test that `GET /api/products/sale` returns sale items and `GET /api/products/electronics` returns the category result. If the issue persists, check that all pods have received the current deployment and that no old instances remain in rotation.

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

**Concepts**
- Route constraint failing to reject the request at routing time
- Duplicate unconstrained route matching before the constrained one
- `[ApiController]` automatic 400 for binding failures when action is reached
- Route constraint as first line of defense — invalid format should 404 at routing

**Answer**

With `{id:guid}`, invalid GUID strings should fail route matching and return 404 before the action runs. A 500 `FormatException` means the constraint is not being evaluated for this request, which has two likely causes: another route without `:guid` (perhaps `[HttpGet("{id}")]` on the same controller) matches first and passes the string to model binding where `Guid` conversion throws, or the constraint is being bypassed because the action is also reachable via conventional routing. Check `EndpointDataSource` to confirm only the constrained endpoint exists for this path, and ensure `[ApiController]` is on the controller so that binding failures reaching the action return 400 `ProblemDetails` rather than 500. Remove any duplicate `{id}` route on the same controller and verify with an integration test that `/api/users/not-a-guid` returns 404.

---

#### Q6. (P) An SPA behind Cloudflare calls your API's OpenAPI-generated client; pagination links in responses use `http://` while the browser page is `https://`. Which routing/link-generation inputs must be correct, and where does forwarded header middleware belong?

**Concepts**
- `HttpContext.Request.Scheme` and `Host` as inputs to link generation
- `UseForwardedHeaders` correcting scheme from `X-Forwarded-Proto`
- Early middleware placement before any URL-generating code
- Cloudflare passing `X-Forwarded-Proto: https` that must be trusted

**Answer**

Link generation reads `HttpContext.Request.Scheme`, `Request.Host`, and `Request.PathBase` to build absolute URLs. Cloudflare terminates TLS and forwards requests to Kestrel as HTTP with `X-Forwarded-Proto: https` and `X-Forwarded-Host: api.example.com`. For the generated pagination links to use `https://api.example.com/...`, `UseForwardedHeaders()` must process those headers before any middleware or endpoint that generates URLs — including HTTPS redirection, authentication, and endpoint execution. Place `app.UseForwardedHeaders()` at the very top of the middleware pipeline, configure `ForwardedHeadersOptions` with Cloudflare's IP ranges in `KnownNetworks`, and set `ForwardLimit = 1` if Cloudflare is the only proxy. Without this, `Request.Scheme` stays `http` so all generated links are `http://` regardless of what the browser sees.

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

**Concepts**
- `MapGroup` prefix composition — `/api` + `/v2` + `/customers` = `/api/v2/customers`
- Client calling unversioned URL when the endpoint is versioned
- OpenAPI listing the correct versioned path for SDK generation
- Integration tests per group prefix verifying the final composed route

**Answer**

The v2 invoices route is correctly registered at `/api/v2/customers/{id}/invoices` because group prefixes compose: `api` + `v2` + `customers` + the endpoint template. The 404 for `/api/v2/customers/5/invoices` is not a registration bug — the client is calling `/api/customers/5/invoices` (the unversioned path) and expecting it to work, but that path has no invoices endpoint. The fix is either to align the client URL to `/api/v2/customers/5/invoices`, or to add the invoices endpoint to the unversioned `customers` group as well. Document the version prefix in OpenAPI `servers` and route group metadata, use `.WithTags("Customers v2")` for grouping in Swagger, and add integration tests per group prefix that assert the final composed URL.

---

#### Q8. (M) Two endpoints match the same HTTP method and path template after a refactor — startup throws `AmbiguousMatchException`. What tools or configuration surface the conflict, and how do route precedence and specificity resolve `{id}` vs `{id:int}` vs literal segments?

**Concepts**
- `AmbiguousMatchException` indicating equal-precedence duplicates
- `EndpointDataSource` inspection listing all registered routes
- Specificity order: literals > constrained parameters > unconstrained parameters
- Equal-specificity not resolved by order — only specificity breaks ties

**Answer**

`AmbiguousMatchException` means two endpoints have identical precedence for the same HTTP method and path, so the matcher cannot choose. To surface the conflict, inject `EndpointDataSource` in a startup test and enumerate all endpoints to find duplicates, or add a diagnostic endpoint in Development that lists all routes from `IEnumerable<EndpointDataSource>`. Specificity rules resolve competing templates deterministically: a literal segment like `sale` beats a parameter `{category}`, a constrained parameter `{id:int}` beats an unconstrained `{id}`, and a longer template beats a shorter one of the same shape. Equal-specificity templates — such as two `[HttpGet("{id}")]` on different actions — cannot be disambiguated by order; one must be renamed, constrained differently (`:int` vs `:guid`), or moved to a different route prefix. The fix is never to add constraints hoping order will save you — routes must be unambiguous by template shape.

---

#### Q9. (D) A team debates `MapGet` minimal endpoints vs attribute-routed controllers for a new internal admin API. Trade-offs for link generation, filters, versioning, OpenAPI, and testability — what would you choose and why?

**Concepts**
- Minimal APIs: lightweight, co-located, less ceremony for simple CRUD
- Controllers: built-in action filters, model binding conventions, easier versioning with `Asp.Versioning`
- Both supporting `IEndpointFilter`, OpenAPI generation, and full DI
- Decision factors: team familiarity, filter complexity, API versioning strategy

**Answer**

I would choose controllers for an internal admin API that will grow over time, because the built-in action filter pipeline (authorization, validation, response caching, custom filters) and the `[ApiController]` automatic model validation with `ProblemDetails` provide a mature set of conventions that minimal APIs require more manual wiring to replicate. `Asp.Versioning` integrates naturally with attribute-routed controllers via `[ApiVersion]` attributes and namespace-based conventions. Link generation (`CreatedAtAction`) and OpenAPI metadata (`ProducesResponseType`) are first-class in controllers. For testability, controllers work seamlessly with `WebApplicationFactory` and standard integration testing patterns. Minimal APIs are the better choice for lightweight microservices, utility endpoints, or when the team is already familiar with the functional style and the endpoint logic is simple enough that the filter ceremony would be overkill. The key signal is filter complexity — if you need action filters, model binding customization, or versioning, controllers pay off quickly; if you are building stateless pass-through endpoints, `MapGet` keeps the codebase smaller.
