# Routing & API Conventions — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is attribute routing in ASP.NET Core Web API?](#q1-what-is-attribute-routing-in-aspnet-core-web-api)
2. [Q2. What does the `[Route("api/[controller]")]` template mean?](#q2-what-does-the-routeapicontroller-template-mean)
3. [Q3. What is the difference between attribute routing and conventional routing for Web APIs?](#q3-what-is-the-difference-between-attribute-routing-and-conventional-routing-for-web-apis)
4. [Q4. Why is attribute routing preferred for REST APIs?](#q4-why-is-attribute-routing-preferred-for-rest-apis)
5. [Q5. What is the `[controller]` token in a route template?](#q5-what-is-the-controller-token-in-a-route-template)
6. [Q6. What is RESTful route design for resource URLs?](#q6-what-is-restful-route-design-for-resource-urls)
7. [Q7. What is route ambiguity and how can it occur in API routing?](#q7-what-is-route-ambiguity-and-how-can-it-occur-in-api-routing)
8. [Q8. What are nested resource routes?](#q8-what-are-nested-resource-routes)
9. [Q9. What is `RouteOptions.LowercaseUrls`?](#q9-what-is-routeoptionslowercaseurls)
10. [Q10. What is `UsePathBase` and how does it affect API URLs?](#q10-what-is-usepathbase-and-how-does-it-affect-api-urls)
11. [Q11. What do `[HttpGet]`, `[HttpPost]`, etc. specify?](#q11-what-do-httpget-httppost-etc-specify)
12. [Q12. What is the difference between route templates on controller vs action?](#q12-what-is-the-difference-between-route-templates-on-controller-vs-action)
13. [Q13. What is link generation in ASP.NET Core routing?](#q13-what-is-link-generation-in-aspnet-core-routing)
14. [Q14. Why should API routes use nouns instead of verbs?](#q14-why-should-api-routes-use-nouns-instead-of-verbs)
15. [Q15. What is a route constraint (e.g., `{id:int}`)?](#q15-what-is-a-route-constraint-eg-idint)
16. [Q16. What happens when two actions match the same route?](#q16-what-happens-when-two-actions-match-the-same-route)
17. [Q17. What is the difference between `[Route]` at class level vs `[HttpGet("path")]` on action?](#q17-what-is-the-difference-between-route-at-class-level-vs-httpgetpath-on-action)
18. [Q18. How does `[ApiController]` affect route parameter binding?](#q18-how-does-apicontroller-affect-route-parameter-binding)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is attribute routing in ASP.NET Core Web API?

What is attribute routing in ASP.NET Core Web API?

**Answer:** Attribute routing maps URLs to controller actions using `[Route]`, `[HttpGet]`, `[HttpPost]`, and related attributes placed on classes and methods. The route template is co-located with the action it serves, replacing conventional `{controller}/{action}` patterns.

- Class level: `[Route("api/[controller]")]` sets the base prefix.
- Action level: `[HttpGet("{id:int}")]` appends segments and HTTP method constraints.
- Registered via `MapControllers()` — no `MapControllerRoute` needed for pure API projects.
- Attribute routes support route constraints, defaults, and named routes.
- ASP.NET Core 8 Web API projects use attribute routing exclusively for REST endpoints.

---

## Q2. What does the `[Route("api/[controller]")]` template mean?

What does the `[Route("api/[controller]")]` template mean?

**Answer:** This template creates a route prefix `api/` followed by the controller name with the `Controller` suffix removed — `OrdersController` becomes `api/orders`. Action-level route attributes append to this base template.

- `[controller]` is a token replaced at runtime by the framework.
- `OrdersController` with `[HttpGet("{id}")]` matches `GET /api/orders/{id}`.
- Token replacement is convention-based — renaming the class changes the public URL.
- Combine with `RouteOptions.LowercaseUrls = true` to emit lowercase segments.
- For stable public URLs, use a literal: `[Route("api/orders")]` instead of `[controller]`.

---

## Q3. What is the difference between attribute routing and conventional routing for Web APIs?

What is the difference between attribute routing and conventional routing for Web APIs?

**Answer:** Attribute routing defines URLs on each action with attributes; conventional routing uses centralized route templates in `Program.cs` like `{controller=Home}/{action=Index}/{id?}`. Web APIs use attribute routing; conventional routing is primarily for MVC views.

- Conventional: `/Orders/GetOrderById/5` — exposes action names in URLs.
- Attribute: `/api/orders/5` — RESTful resource paths with HTTP verb constraints.
- Conventional routing makes multiple GET actions on one controller collide.
- Attribute routing co-locates the contract with the handler for clarity and OpenAPI discovery.
- `[ApiController]` requires attribute routing — conventional routes are ignored for API controllers.

---

## Q4. Why is attribute routing preferred for REST APIs?

Why is attribute routing preferred for REST APIs?

**Answer:** Attribute routing expresses resource-oriented URLs directly on actions, supports HTTP method constraints, and integrates cleanly with Swagger and `[ApiController]` binding inference. It avoids leaking implementation names and enables precise RESTful path design.

- Templates like `[HttpGet("{id:int}")]` and `[HttpPost]` map cleanly to REST verbs.
- Each action declares its own URL — no collision between multiple GET methods.
- OpenAPI/Swashbuckle reads attribute routes to generate accurate path documentation.
- Route constraints (`{id:int}`) disambiguate overlapping templates.
- Teams can apply consistent prefixes via conventions without sacrificing per-action control.

---

## Q5. What is the `[controller]` token in a route template?

What is the `[controller]` token in a route template?

**Answer:** The `[controller]` token is a route parameter placeholder replaced with the controller class name minus the `Controller` suffix. It keeps route templates DRY but ties public URLs to C# class names.

- `ProductsController` → segment `Products` (or `products` with lowercase URLs).
- Renaming `ProductsController` to `ItemsController` changes the URL from `/api/products` to `/api/items`.
- Override with `[ControllerName("products")]` to decouple class name from URL segment.
- Prefer literal route segments for public APIs where URL stability matters across refactors.
- The token works at the class-level `[Route]` template, not inside action templates alone.

---

## Q6. What is RESTful route design for resource URLs?

What is RESTful route design for resource URLs?

**Answer:** RESTful routes use plural nouns for collections, path parameters for identifiers, and HTTP verbs for operations — avoiding action verbs in the path. Nesting expresses ownership: `/api/customers/3/orders/7`.

- Collections: `GET /api/orders`, `POST /api/orders`.
- Single resource: `GET /api/orders/42`, `PUT /api/orders/42`, `DELETE /api/orders/42`.
- Sub-resources: `POST /api/orders/42/payments` for non-CRUD operations on a parent.
- No verbs in paths: reject `/api/orders/create` or `/api/getOrderById`.
- Query strings handle filtering and pagination: `GET /api/orders?status=shipped&page=2`.

---

## Q7. What is route ambiguity and how can it occur in API routing?

What is route ambiguity and how can it occur in API routing?

**Answer:** Route ambiguity occurs when two or more actions match the same HTTP method and URL pattern, leaving the framework unable to select a single endpoint. It arises from overlapping templates, missing constraints, or duplicate HTTP method registrations.

- `{category}` (string) and `{id:int}` both match `/api/products/10` — string wins unpredictably.
- Two `[HttpDelete("{id}")]` actions with different parameter types cause ambiguous matches.
- Literal segments (`featured`) must be declared separately from parameterized segments.
- Fix with route constraints (`{id:int}`), distinct path prefixes, or consolidated actions.
- ASP.NET Core logs ambiguous match warnings at startup or returns 500 at runtime.

---

## Q8. What are nested resource routes?

What are nested resource routes?

**Answer:** Nested resource routes express parent-child relationships in the URL hierarchy, placing the child resource under the parent's path. They indicate scope and ownership without embedding verbs.

- Example: `GET /api/customers/3/orders/7` — order 7 belonging to customer 3.
- Controller route: `[Route("api/customers/{customerId}/orders")]` with `[HttpGet("{orderId:int}")]`.
- `CreatedAtAction` must include all parent route values (`customerId`, `orderId`) in link generation.
- Deep nesting (>2 levels) becomes unwieldy — balance REST purity with URL readability.
- Authorization often checks parent ownership: customer 3 can only access their own orders.

---

## Q9. What is `RouteOptions.LowercaseUrls`?

What is `RouteOptions.LowercaseUrls`?

**Answer:** `RouteOptions.LowercaseUrls` is a global configuration that generates all URL paths in lowercase during link generation and route matching. Setting it to `true` makes `/api/Orders` resolve as `/api/orders`.

- Configured in `Program.cs`: `builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);`.
- Affects `CreatedAtAction`, `LinkGenerator`, and URL generation throughout the app.
- Case-sensitive clients must use lowercase URLs consistently after enabling.
- Does not change controller class names — only the emitted URL segments.
- Partner documentation and integration tests must align with the lowercase policy.

---

## Q10. What is `UsePathBase` and how does it affect API URLs?

What is `UsePathBase` and how does it affect API URLs?

**Answer:** `UsePathBase` strips a path prefix from incoming requests so the app can be hosted behind a reverse proxy or sub-path without rewriting every route attribute. Link generation must include the same path base for Location headers to be correct.

- Example: gateway exposes `/api/v1/store/orders/5` but the app routes `/orders/5`.
- `app.UsePathBase("/api/v1/store")` must run early in the middleware pipeline.
- `CreatedAtAction` includes PathBase when the request carries it — test clients must set it too.
- Combine with `ForwardedHeaders` middleware for correct public scheme and host in absolute URLs.
- Mismatch between PathBase and gateway config causes 404s and broken Location headers.

---

## Q11. What do `[HttpGet]`, `[HttpPost]`, etc. specify?

What do `[HttpGet]`, `[HttpPost]`, etc. specify?

**Answer:** HTTP method attributes (`[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`, `[HttpDelete]`) constrain which HTTP verb matches an action and optionally append a route template segment. They combine with the controller's base `[Route]` to form the full URL pattern.

- `[HttpGet]` on an action with no template matches the controller base route.
- `[HttpGet("{id:int}")]` adds `{id}` segment and restricts to GET requests.
- POST to a GET-only action returns 405 Method Not Allowed.
- Each action typically carries exactly one HTTP method attribute.
- Method attributes are required for API controllers — bare `[Route]` without a verb does not expose the action.

---

## Q12. What is the difference between route templates on controller vs action?

What is the difference between route templates on controller vs action?

**Answer:** The controller-level `[Route]` sets the shared prefix for all actions in that class; action-level `[HttpGet("...")]` templates append relative segments and HTTP method constraints. Together they form the complete route pattern.

- Controller: `[Route("api/v1/[controller]")]` → base `/api/v1/orders`.
- Action: `[HttpGet("{id:int}")]` → full route `GET /api/v1/orders/{id}`.
- Action templates starting with `~/` override the controller prefix entirely.
- Action templates starting with `/` are absolute from the application root.
- Keeping version and prefix at controller level avoids repeating them on every action.

---

## Q13. What is link generation in ASP.NET Core routing?

What is link generation in ASP.NET Core routing?

**Answer:** Link generation creates URLs from route names, action names, and route values using the routing system's `LinkGenerator` service. It powers `CreatedAtAction`, `CreatedAtRoute`, `Url.Action`, and hypermedia links in responses.

- Input: action name + `{ id = 5, tenantId = 3 }` → output: `/api/tenants/3/orders/5`.
- Respects route constraints, lowercase URL options, and path base settings.
- Broken link generation (wrong action name, missing route value) produces 404 Location headers.
- Named routes (`Name = "GetOrder"`) decouple link generation from method renames.
- Prefer link generation over hard-coded URL strings for maintainability behind gateways.

---

## Q14. Why should API routes use nouns instead of verbs?

Why should API routes use nouns instead of verbs?

**Answer:** Nouns identify resources; HTTP methods express the operation — combining verbs in URLs duplicates method semantics and produces RPC-style endpoints that break caching, idempotency, and standard REST tooling. `/api/orders` with POST creates; `/api/createOrder` does not.

- Verbs in paths: `/api/orders/cancel/42` — non-standard, not cacheable, hard to document.
- Nouns with methods: `DELETE /api/orders/42` or `PATCH /api/orders/42` with status change.
- Gateway policies, rate limits, and monitoring rules apply per HTTP method on resource paths.
- OpenAPI groups operations by resource path, not by verb-named endpoints.
- Exception: sub-resource actions that are not CRUD may use nouns for processes: `/api/orders/42/cancellations`.

---

## Q15. What is a route constraint (e.g., `{id:int}`)?

What is a route constraint (e.g., `{id:int}`)?

**Answer:** A route constraint restricts which values match a route parameter — `{id:int}` accepts only integers, `{slug:alpha}` only letters. Constraints disambiguate overlapping templates and reject invalid parameter types early.

- `{id:int}` — matches `42`, rejects `abc` (returns 404 instead of hitting wrong action).
- `{date:datetime}` — validates date format in the URL segment.
- Custom constraints implement `IRouteConstraint` for domain-specific rules.
- Without constraints, `{category}` (string) competes with `{id:int}` for numeric paths.
- Constraints appear in attribute routes: `[HttpGet("{id:int}")]` on the action.

---

## Q16. What happens when two actions match the same route?

What happens when two actions match the same route?

**Answer:** ASP.NET Core cannot dispatch the request to a single action and throws an `AmbiguousMatchException` at runtime or logs a warning at startup. The request fails with HTTP 500 unless one action is removed or templates are disambiguated.

- Common cause: two actions with identical `[HttpGet("{id}")]` on the same controller.
- Another cause: `{id:int}` and `{id:guid}` both matching certain values (rare).
- Fix by adding constraints, merging into one action, or using different path prefixes.
- Swagger may show duplicate operations before the runtime failure is discovered.
- Code review should reject duplicate HTTP method + template combinations on one controller.

---

## Q17. What is the difference between `[Route]` at class level vs `[HttpGet("path")]` on action?

What is the difference between `[Route]` at class level vs `[HttpGet("path")]` on action?

**Answer:** Class-level `[Route]` defines the shared URL prefix for the controller; action-level `[HttpGet("path")]` adds a relative segment and binds the action to the GET verb. `[Route]` alone on an action without an HTTP method attribute does not create an endpoint in API projects.

- Class: `[Route("api/[controller]")]` — all actions inherit this prefix.
- Action: `[HttpGet("{id}")]` — matches GET with `{id}` appended to the prefix.
- `[HttpGet("active")]` → `GET /api/orders/active` (literal segment before parameters).
- Action-only `[Route("api/orders")]` without `[HttpGet]` is insufficient for API controllers.
- HTTP method attributes carry optional route templates; bare `[Route]` on actions is for non-API MVC scenarios.

---

## Q18. How does `[ApiController]` affect route parameter binding?

How does `[ApiController]` affect route parameter binding?

**Answer:** `[ApiController]` enables binding source inference — simple types from route templates bind as `[FromRoute]`, complex types from the body bind as `[FromBody]`, and query parameters bind as `[FromQuery]` without explicit attributes. This reduces boilerplate but requires understanding the inference rules.

- `Get(int id)` with `{id}` in the route template → bound from route automatically.
- `Post(OrderDto dto)` → bound from request body automatically.
- `Get([FromQuery] string filter)` — simple types not in the route template default to query string.
- `[FromServices]` must still be explicit for service injection into action parameters.
- Misunderstanding inference causes silent null values — e.g., expecting body binding on a simple type gets query binding instead.

---

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

#### Gotcha 1. POST returning 200 instead of 201

**Answer:** A successful resource creation with POST should return HTTP 201 Created and tell the client where the new resource lives — returning 200 OK omits that contract and breaks REST clients that rely on status codes and the Location header.

- Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with a Location header pointing at the new resource URL.
- Include the created representation or a minimal payload in the response body when clients need immediate data without a follow-up GET.
- Returning 200 for create operations hides the new resource URL from standard HTTP client libraries and OpenAPI-generated SDKs.

---

#### Gotcha 2. GET that mutates state

**Answer:** GET must be safe and idempotent — performing deletes or updates on GET violates HTTP semantics, breaks caching proxies, and creates security holes when URLs are prefetched, logged, or opened in email clients.

- Browsers, CDNs, and link-preview crawlers may invoke GET URLs without user intent, so side effects run unintentionally.
- Cached GET responses can replay destructive operations or stale mutations across clients.
- Use POST, PUT, PATCH, or DELETE for state changes and keep GET read-only.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Answer:** Business failures must map to appropriate 4xx or 5xx status codes — a 200 response with an error flag forces every client to parse the body instead of using standard HTTP semantics, retries, and monitoring.

- Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures and 404, 409, or 422 for domain errors.
- HTTP status codes drive client retry logic, API gateways, and APM alerting; a 200 masks failures in dashboards.
- Envelope patterns like `{ success: false }` require custom handling in every consumer and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Answer:** EF Core entities expose navigation properties, shadow fields, and circular references that are not meant for public contracts — serialize DTOs with explicit shapes and never leak database schema to clients.

- Lazy-loaded navigations trigger N+1 queries during serialization and can pull entire object graphs into the response.
- Circular references between entities cause JSON serializer loops or require fragile reference-handling settings.
- DTOs decouple the API contract from schema migrations and let you expose only the fields clients need.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Answer:** ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json` — PascalCase property names from some clients bind as missing properties, leaving model properties at default values and causing silent data loss on POST and PUT.

- `[JsonPropertyName("PropertyName")]` or a custom `PropertyNamingPolicy` aligns server expectations with legacy client payloads.
- Enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)` when you must accept mixed casing.
- Silent binding failures produce 201/204 success responses with partially saved data and no validation error.

---

#### Gotcha 6. GET with `[FromBody]`

**Answer:** Many HTTP clients, proxies, and caches ignore or strip GET request bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8 Web API.

- Model binding for `[FromBody]` on GET is not reliably supported across the HTTP ecosystem.
- Use query strings with `[FromQuery]` for simple filters or POST to a dedicated search endpoint for complex filter objects.
- OpenAPI tools and browser fetch also discourage or block GET bodies, making the pattern fragile in production.

---

#### Gotcha 7. CORS as server security

**Answer:** CORS is enforced by browsers only — it does not stop curl, Postman, server-to-server calls, or direct API requests; authentication and authorization still protect the API.

- CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they do not authenticate callers.
- A public API without auth remains fully accessible to any non-browser client regardless of CORS policy.
- Register `AddCors` and `UseCors` for browser SPA access, and enforce JWT, cookies, or API keys separately for real security.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Answer:** Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers — you must specify explicit origins with `WithOrigins` and call `AllowCredentials`.

- `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined; ASP.NET Core will not emit a valid CORS response for credentialed requests.
- List every trusted frontend origin explicitly, including local dev URLs and production domains.
- Credentialed cross-origin calls require both matching origins and `Access-Control-Allow-Credentials: true`.

---

#### Gotcha 9. Swagger UI exposed in Production

**Answer:** Public Swagger UI discloses the full API surface, schemas, and try-it-out access — gate it behind authentication or disable it outside Development and Staging in ASP.NET Core 8.

- `MapSwagger` and `UseSwaggerUI` in `Program.cs` should be wrapped in environment checks or authorization middleware.
- Exposed OpenAPI documents reveal internal endpoints, field names, and enum values useful for reconnaissance.
- Production APIs typically serve OpenAPI only to authenticated developers or internal tooling, not the public internet.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Answer:** Without `[ApiController]`, automatic 400 `ValidationProblemDetails`, binding source inference, and attribute routing behaviors differ — mixed controllers in the same Web API produce inconsistent error contracts.

- `[ApiController]` enables automatic model-state validation responses and `[FromBody]` inference for complex types.
- Controllers missing the attribute may return 200 with invalid models or require manual `ModelState` checks.
- Apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Answer:** Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation and deadlocks under load — always `await` async service and database calls in ASP.NET Core 8.

- Sync-over-async ties up request threads while I/O completes, reducing throughput on Kestrel under concurrent load.
- Deadlocks occur when the blocked thread holds a synchronization context the continuation needs to resume.
- Mark controller actions `async Task<IActionResult>` and propagate `await` through the service layer to EF Core and HTTP clients.

---

#### Gotcha 12. Liveness probe includes SQL check

**Answer:** If the liveness probe fails when SQL is down, Kubernetes restarts pods that cannot fix the dependency — put SQL, Redis, and external service checks on readiness only.

- Liveness answers whether the process should be killed and restarted; a down database is not healed by restarting the app.
- Readiness removes the pod from the load balancer until dependencies recover without unnecessary restarts.
- Map `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck` or custom dependency tags.

---

#### Gotcha 13. N+1 queries in list endpoints

**Answer:** Returning entities with lazy-loaded navigation properties triggers one SQL query per row — use projection with `Select`, explicit `Include`, or DTO mapping to fetch list data in a bounded number of queries.

- Serializing a list of `Order` entities with `Customer` navigation can execute 1 + N queries under default lazy loading.
- Project directly to DTOs in LINQ so EF Core generates a single query with only the columns needed.
- For graphs that must be included, use `Include`/`ThenInclude` or split queries deliberately rather than relying on lazy load during JSON output.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Answer:** Concurrent inserts and deletes between offset pages cause duplicate or skipped rows — use keyset or cursor pagination ordered by a stable, indexed key for large datasets in Web API list endpoints.

- `Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are added or removed between requests.
- Keyset pagination uses `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key from the previous response.
- Offset pagination remains acceptable for small, mostly static tables; expose cursor tokens in link headers or response metadata for high-churn data.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Answer:** Field resolvers in HotChocolate or other GraphQL servers that query the database per parent row explode SQL under load — batch related loads with DataLoader or resolve joins at the root query.

- A list of 100 authors each resolving `books` individually executes 101 queries instead of one batched query.
- Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips.
- Eager-load or project at the root query when the client always requests nested fields together.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Answer:** Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript — browser clients need gRPC-Web middleware plus CORS configuration in ASP.NET Core 8.

- Standard `@grpc/grpc-js` in Node or .NET clients works server-to-server; Blazor WASM and SPA browsers require the gRPC-Web protocol.
- Add `AddGrpcWeb()` and `EnableGrpcWeb()` on mapped gRPC services to translate between gRPC-Web and native gRPC.
- Configure CORS for the browser origin alongside gRPC-Web, since cross-origin browser calls still enforce CORS on preflight and response headers.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this controller after a rename from `CustomerController` to `ClientsController`. Partner calls to `POST /api/customers` return 404; Swagger still lists `/api/Clients`.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    [HttpPost]
    public ActionResult<ClientDto> Create([FromBody] CreateClientDto dto) { /* ... */ }
}
```

Partners were onboarded with lowercase `/api/customers` from the old controller name. No `[Route("api/customers")]` override exists.

---

**Answer:**

**Answer:** `[Route("api/[controller]")]` derives the segment from the class name — renaming to `ClientsController` changed the public URL to `/api/clients` without a compatibility shim. Production APIs need explicit stable routes or versioning when renaming controllers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | `[controller]` token replaced `customers` with `clients` | Breaking change for all integrated partners |
| Conventions | Lowercase URL option may emit `/api/clients` vs partner `/api/customers` | Case-sensitive clients 404 |
| Documentation | Swagger reflects new name only | Partner docs diverge |
| Design | Resource name tied to C# class name | Refactors become contract changes |

**Fix (priority order):**

1. Pin route: `[Route("api/customers")]` on controller (or `[Route("api/[controller]")]` + `[ControllerName("customers")]`).
2. Keep `/api/clients` as deprecated alias during migration if rename is intentional.
3. Add contract tests asserting partner URLs before release.

**Production takeaway:** **`[controller]` is convenient, not stable** — public resource names should be explicit in route templates.

---

---

#### Q2. (R) Review ambiguous attribute routes. `GET /api/products/10` intermittently hits the wrong action depending on build order.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok(_repo.Get(id));

    [HttpGet("{category}")]
    public IActionResult GetByCategory(string category) => Ok(_repo.ByCategory(category));

    [HttpGet("featured")]
    public IActionResult Featured() => Ok(_repo.Featured());
}
```

Request `GET /api/products/featured` works; `GET /api/products/10` sometimes binds `category = "10"`.

---

**Answer:**

**Answer:** `{category}` is a catch-all string segment that competes with `{id:int}` — numeric slugs bind as category name `"10"` when that template wins ordering. Literal segments like `featured` must be declared, and overlapping parameter routes need constraints or consolidation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | `{category}` matches any string including numeric | `10` routed to category action instead of by-id |
| Routing | Ambiguous template precedence | Intermittent behavior across deployments |
| REST design | Category and id share one path level | Model confusion — prefer `/categories/{name}` vs `/{id:int}` |
| API contract | Clients cannot predict response shape | Same URL returns category list or single product |

**Fix (priority order):**

1. Split routes: `GET /api/products/{id:int}` and `GET /api/products/categories/{category}`.
2. Or constrain category: `[HttpGet("category/{category}")]` for non-numeric categories only.
3. Register literal `featured` before parameterized templates (still prefer explicit paths).

**Production takeaway:** Attribute routing **requires explicit disambiguation** — int constraints alone do not fix string route competition.

---

---

#### Q3. (R) Review RESTful route design for a code review. Which templates would you reject before merge?

```csharp
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpPost("GenerateInvoice")]
    public IActionResult Generate([FromBody] GenerateDto dto) => Ok();

    [HttpGet("GetInvoice/{invoiceId}")]
    public IActionResult GetInvoice(int invoiceId) => Ok();

    [HttpPost("{invoiceId}/SendEmail")]
    public IActionResult SendEmail(int invoiceId) => Ok();
}
```

---

**Answer:**

**Answer:** Verb phrases in URLs (`GenerateInvoice`, `GetInvoice`, `SendEmail`) are RPC-style and duplicate HTTP method meaning — reject in favor of noun-based resources and proper verbs on standard paths.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| REST | Verbs in path (`GenerateInvoice`, `GetInvoice`) | Non-standard; breaks HTTP verb semantics |
| Consistency | Mixed prefix patterns on actions | OpenAPI clutter; harder gateway policies |
| Design | POST for read-like `GetInvoice` path with GET verb missing | Wrong idempotency expectations |
| Maintainability | Action names embedded in URLs | URL churn when refactoring actions |

**Fix (priority order):**

1. `POST /api/invoices` (generate/create), `GET /api/invoices/{invoiceId}`, `POST /api/invoices/{invoiceId}/email` or sub-resource `/notifications`.
2. Remove redundant verb segments from templates.
3. Document side-effect sub-resources explicitly in OpenAPI.

**Production takeaway:** Code review here tests **resource naming judgment**, not memorizing `[Route]` syntax.

---

---

#### Q4. (M) After enabling lowercase URLs (`RouteOptions.LowercaseUrls = true`), `CreatedAtAction` generates `Location: /api/orders/5` but the gateway publicly exposes `/api/v1/store/orders/5`. What routing and link-generation pieces are missing?

```csharp
// Program.cs excerpt
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
var app = builder.Build();
app.UsePathBase("/api/v1/store");
app.MapControllers();
```

---

**Answer:**

**Answer:** `UsePathBase` sets the request path base for matching but link generation must include that path base (and forwarded headers for public scheme/host) — configure `HttpContext.Request.PathBase` awareness via middleware order and optionally `LinkGenerator` base address for absolute URLs.

- **`UsePathBase("/api/v1/store")`:** Must run early so routing and link generation see the path base — verify order before `MapControllers`.
- **Location header:** `CreatedAtAction` may omit path base if request PathBase not set on test client — integration tests must set `Request.Path` + PathBase or use `CreateClient` with `BaseAddress`.
- **Gateway:** Configure `ForwardedHeaders` so absolute URLs use public host/scheme, not internal pod URL.
- **Options:** `services.Configure<RouteOptions>` lowercase affects generated URLs — ensure partner docs use same casing policy.
- **Fix:** `Created(uri, value)` with `Url.Action`/`LinkGenerator.GetUriByAction` including path base, or set client BaseAddress in tests to match gateway.

**Production takeaway:** **Route templates and public URLs diverge** behind path bases — link generation is a production routing topic, not just controller attributes.

---

---

#### Q5. (D) A junior developer proposes conventional routing for a new public JSON API because "Startup.cs tutorials use `MapControllerRoute`." Argue for or against attribute routing for Web APIs in ASP.NET Core 8.

---

**Answer:**

**Answer:** Public Web APIs should use attribute routing with `[ApiController]` — conventional `{controller}/{action}` routes expose implementation names, complicate RESTful URLs, and fight OpenAPI grouping; conventional routing remains for MVC views, not JSON APIs.

- **Against conventional for APIs:** URLs like `/Orders/GetOrderById` leak action names; HTTP method constraints are weaker; multiple GET actions collide.
- **For attribute routing:** Co-locate template with action; express REST resources; `[HttpGet("{id}")]` clarity; API explorer/Swagger reads attributes directly.
- **When conventional appears:** Legacy MVC apps or admin areas — not greenfield REST APIs.
- **Hybrid:** `MapControllers()` only — no `MapControllerRoute` needed for pure API projects.
- **Team norm:** Document `[Route("api/v1/[controller]")]` at assembly or use `AddControllers(options => options.Conventions.Add(...))` for prefix consistency.

**Production takeaway:** Karat wants **convention choice reasoning** — attribute routing is the production default for ASP.NET Core Web APIs.

---

---

#### Q6. (R) Review `[ApiController]` route prefix behavior. Integration tests call `/api/v1/orders` but receive 404 — unit tests on the controller pass.

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult Get(int id) => Ok(_orders.Get(id));
}

// Test project helper:
var client = _factory.CreateClient();
var response = await client.GetAsync("/api/orders/1");
```

No global route prefix configured in `Program.cs`.

---

**Answer:**

**Answer:** The controller route includes `v1` but the test client requests `/api/orders/1` — missing version segment. `[ApiController]` does not infer global prefixes; the test must match the attribute template exactly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Testing | Wrong URL in integration test | False confidence from passing unit tests |
| Routing | Template is `api/v1/[controller]` | `/api/orders` does not match |
| Process | Unit tests invoke action directly | Bypass routing — misses template bugs |
| Versioning | v1 in route requires client update | Partners must include version in path |

**Fix (priority order):**

1. Update test: `GetAsync("/api/v1/orders/1")`.
2. Add global route prefix via `options.Conventions` if team wants centralized version segment.
3. Prefer `WebApplicationFactory` routing tests for every public endpoint path.

**Production takeaway:** **Unit tests without routing lie** — API conventions must be verified through HTTP with correct templates.

---

---

#### Q7. (R) Review duplicate HTTP method registration. Swagger shows two operations for `DELETE /api/items/{id}`; one returns 405 in production.

```csharp
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) { _repo.Delete(id); return NoContent(); }

    [HttpDelete("{id}")]
    public IActionResult Remove(string id) { _repo.Delete(int.Parse(id)); return NoContent(); }
}
```

---

**Answer:**

**Answer:** Two DELETE actions differ only by parameter type (`int` vs `string`) on the same template — routing cannot stably choose, causing ambiguous match or build-time warnings. Keep one action and parse or constrain the parameter.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | Duplicate `[HttpDelete("{id}")]` templates | Ambiguous endpoint selection |
| Runtime | One match may 405 Method Not Allowed | Unpredictable delete behavior |
| Design | `int.Parse` in string action | 500 on non-numeric id instead of 404 |
| OpenAPI | Duplicate operation IDs | Client generation fails |

**Fix (priority order):**

1. Single `[HttpDelete("{id:int}")]` action.
2. Remove string overload; use route constraint for int ids.
3. Regenerate Swagger with unique operation names.

**Production takeaway:** **One endpoint, one handler** — duplicate attribute routes are a merge blocker.

---

---

#### Q8. (R) Review nested resource routes and link generation for a parent/child REST surface.

```csharp
[Route("api/customers/{customerId:int}/orders")]
[ApiController]
public class CustomerOrdersController : ControllerBase
{
    [HttpPost]
    public ActionResult<OrderDto> Create(int customerId, [FromBody] CreateOrderDto dto)
    {
        var order = _svc.Create(customerId, dto);
        return CreatedAtAction(nameof(Get), new { orderId = order.Id }, order);
    }

    [HttpGet("{orderId:int}")]
    public ActionResult<OrderDto> Get(int customerId, int orderId) => Ok(_svc.Get(customerId, orderId));
}
```

`CreatedAtAction` Location returns `/api/customers/3/orders?orderId=7` without the order segment.

**Answer:**

**Answer:** `CreatedAtAction` must include **all** route parameters matching the GET template — `orderId` belongs in the path `{orderId:int}`, not as a dangling query value because `nameof(Get)` target expects both `customerId` and `orderId` in the path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Link generation | Missing `customerId` or wrong param name in `CreatedAtAction` values | Location URL missing `{orderId}` segment |
| Routing | Generated `/api/customers/3/orders?orderId=7` | GET expects `/api/customers/3/orders/7` |
| REST | Broken nested resource URI | Clients cannot follow Location |
| Testing | No assertion on Location path shape | Regression on create flows |

**Fix (priority order):**

1. `return CreatedAtAction(nameof(Get), new { customerId, orderId = order.Id }, order.ToDto());`
2. Or name the GET route: `[HttpGet("{orderId:int}", Name = "GetCustomerOrder")]` and use `CreatedAtRoute`.
3. Integration test: POST then GET Location URL returns 200.

**Production takeaway:** Nested REST routes multiply **route value requirements** for link generation — every parent segment must appear in `CreatedAtAction` values.

---
