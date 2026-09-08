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

## Gotchas — Routing & Endpoints (Interview Traps)

---

#### Gotcha 1. `AmbiguousMatchException` at runtime — two routes with identical precedence

**Concepts**
- Endpoint routing scoring templates by specificity at startup
- `AmbiguousMatchException` when two endpoints have equal score for the same path
- Duplicate `[HttpGet("users")]` on two controllers causing runtime failure
- Mixing Minimal API and controller endpoints with identical templates

**Answer**

When two or more endpoints have identical routing precedence for the same HTTP method and path, the endpoint matcher throws `AmbiguousMatchException` at runtime for matching requests. Unlike compile-time errors, this failure only surfaces when the conflicting path is actually requested. Common causes include identical `[HttpGet("orders")]` on two controllers, a Minimal API `MapGet("/orders", ...)` alongside a controller action with the same template, or a base controller route combined with an action-level template that accidentally matches an existing route. Detect conflicts early by enumerating `EndpointDataSource` entries in a startup integration test or by using `UseRouting` with `DeveloperExceptionPage` in development.

---

#### Gotcha 2. Route constraint mismatch returns 404 — no error or log entry explains the miss

**Concepts**
- Route constraints evaluated as part of template matching
- Constraint failure returning 404, not 400 or 500
- `{id:int}` not matching non-integer values silently
- Incorrect constraint syntax compiling but never matching any request

**Answer**

When a route template includes a constraint — `{id:int}`, `{date:datetime}`, `{slug:regex(^[a-z]+$)}` — and the incoming request value does not satisfy the constraint, the route does not match and a 404 is returned. No log entry, no error message, and no indication that a matching route exists but failed a constraint. A client sending `GET /orders/abc` when the route is `[HttpGet("{id:int}")]` receives 404 without any hint that the route exists. Debugging this requires manually checking the route template and constraint, often by temporarily removing the constraint to confirm the route matches without it.

---

#### Gotcha 3. Attribute route on a derived controller inherits from base — combined template may be wrong

**Concepts**
- `[Route]` on base controller class inherited by derived controllers
- Derived controller `[Route]` replacing, not appending to, base route
- Token replacement `[controller]` evaluated on the concrete type
- Route template combination rules for class-level and method-level attributes

**Answer**

A base controller class with `[Route("api/[controller]")]` and an action `[HttpGet("details")]` produces the route `api/BaseController/details` — not `api/DerivedController/details`. When a derived controller inherits from a base with a class-level `[Route]`, the derived class must add its own `[Route]` to override the base route. Adding `[Route("api/[controller]")]` on the derived class uses `[controller]` token replacement, which resolves to the derived class name. Forgetting this means all actions from both the base and derived classes share the same URL prefix, potentially causing `AmbiguousMatchException` or routing requests to the wrong controller.

---

#### Gotcha 4. `[HttpGet]` without a template vs `[Route]` — matching behavior differs

**Concepts**
- `[HttpGet]` without template matching root controller route
- `[HttpGet("")]` explicitly matching the empty path segment
- `[Route("path")]` on an action combined with controller-level route
- Order of route attributes on the same action

**Answer**

`[HttpGet]` without a template means the action matches at the controller's root route — for `[Route("api/orders")]` controller, `[HttpGet]` matches `GET /api/orders`. `[HttpGet("")]` is equivalent. Applying both `[HttpGet]` and `[Route("path")]` to the same action creates two separate routes for the action, not a combined one. Multiple `[Route]` attributes on the same action register multiple templates pointing to the same handler, which is intentional for versioned or aliased routes but surprising when done accidentally. Verify the effective URL by checking `EndpointDataSource.Endpoints` at startup in a test.

---

#### Gotcha 5. `MapControllers()` is required — without it, controller attribute routes return 404

**Concepts**
- `AddControllers()` registering MVC services in DI
- `MapControllers()` activating endpoint routing from controller attributes
- 404 with no error when `MapControllers()` is omitted
- Distinction between service registration and route activation

**Answer**

`builder.Services.AddControllers()` registers the MVC infrastructure — model binding, validation, filters, formatters — but does not create any routes. Routes are created by `app.MapControllers()`, which scans controller classes for routing attributes and registers them with the endpoint routing system. Omitting `MapControllers()` means no controller routes exist and every request to a controller path returns 404 silently. This separation between registration and activation is intentional in the minimal hosting model but trips up developers used to the conventional `app.UseEndpoints(e => e.MapControllers())` pattern from earlier versions, which has the same requirement but was historically bundled differently.

---

#### Gotcha 6. Route parameter optional vs nullable query string — fundamentally different binding

**Concepts**
- Optional route parameter `{id?}` matching with or without the segment
- Nullable query string parameter `int?` binding from absence of key
- `[FromRoute]` vs `[FromQuery]` explicit binding source attributes
- URL structure changes when route vs query string approach is chosen

**Answer**

An optional route segment `{id?}` produces different URL structures depending on presence — `GET /orders` and `GET /orders/123` are different paths. A nullable query string parameter `int? id` always uses `GET /orders` with an optional `?id=123` suffix. These are not interchangeable: changing from route to query string binding changes the API contract and breaks existing clients. Use `[FromRoute]` and `[FromQuery]` explicitly to make binding intent clear. When an action has both route and query string parameters with the same name, the binding source must be explicit — ASP.NET Core applies precedence rules that may not match the intended behavior.

---

#### Gotcha 7. Conventional routing order matters — more specific routes must be registered before more general ones

**Concepts**
- `MapControllerRoute` evaluated in registration order for conventional routing
- First matching pattern wins — more general patterns shadow specific ones
- Attribute routing evaluated by specificity, not registration order
- Mixing conventional and attribute routing in the same application

**Answer**

In conventional routing (using `MapControllerRoute`), routes are evaluated in registration order and the first match wins. A general catch-all route registered before a specific route shadows the specific route — requests that should match the specific route are handled by the catch-all instead. This is different from attribute routing, where specificity is calculated and the most specific template wins regardless of registration order. Mixing conventional and attribute routing in the same application adds complexity: attribute-routed controllers are not affected by `MapControllerRoute` patterns, but conventional controllers are, and understanding which applies to each controller is critical during debugging.

---

#### Gotcha 8. Route values take precedence over query string — unexpected parameter binding

**Concepts**
- Route values populated by matched URL segments
- Query string keys with same name as route parameters overridden by route values
- `HttpContext.Request.RouteValues` vs `HttpContext.Request.Query`
- Model binding source priority: route → query → body

**Answer**

When a route template contains `{id}` and a request arrives with both `/orders/5?id=10`, the model binder receives `id=5` from route values and `id=10` from the query string. Route values take priority, so the action receives `5`. This matters when APIs evolve and the same parameter name is used in both the URL template and as a query parameter for filtering — the route value silently wins. Use `[FromRoute]` and `[FromQuery]` attributes explicitly to avoid ambiguous binding and to document intent clearly to both the runtime and API consumers.

---

#### Gotcha 9. Endpoint metadata is not available until after `UseRouting` runs

**Concepts**
- Endpoint selection happening inside `UseRouting`
- `IEndpointFeature` on `HttpContext` populated by routing
- Middleware reading `context.GetEndpoint()` before routing returning `null`
- Authorization and rate limiting requiring routing to have run first

**Answer**

`context.GetEndpoint()` returns `null` until `UseRouting` has run and selected an endpoint. Any middleware registered before `UseRouting` that tries to read endpoint metadata — authorization policies, rate limit policies, or custom attributes — sees `null` and cannot apply endpoint-specific behavior. This is why `UseAuthentication` and `UseAuthorization` must be registered after `UseRouting`: they read `[Authorize]` attributes and policy names from the selected endpoint's metadata, which only exists after routing has matched the request. Custom middleware that reads custom endpoint metadata should similarly be placed after `UseRouting` in the pipeline.

---

#### Gotcha 10. `MapFallbackToFile` intercepts unmatched API paths — API 404s become 200 HTML responses

**Concepts**
- SPA fallback returning `index.html` for all unmatched routes
- Registration order — API endpoints must be mapped before fallback
- API 404 responses masked as HTTP 200 with HTML body
- Conditional fallback excluding `/api` prefix to protect API 404 semantics

**Answer**

`MapFallbackToFile("index.html")` matches any request that no other endpoint claims, including `GET /api/nonexistent`. When registered before `MapControllers()`, it intercepts API paths that should return 404 and returns `index.html` with status 200 instead. JSON clients receive unexpected HTML, fetch calls throw `SyntaxError` when parsing the HTML body as JSON, and debugging is difficult because the status code is 200. Always register `MapControllers()` and all API endpoint mappings before `MapFallbackToFile`. For extra protection, add a path exclusion guard that prevents the fallback from matching requests beginning with `/api`.

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
