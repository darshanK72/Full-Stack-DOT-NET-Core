# API Versioning — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 07. API Versioning](#chapter-07-api-versioning)
  - [Q1. What is API versioning?](#chapter-07-api-versioning-q1)
  - [Q2. What is URL path versioning?](#chapter-07-api-versioning-q2)
  - [Q3. What is header-based API versioning?](#chapter-07-api-versioning-q3)
  - [Q4. What is query string API versioning?](#chapter-07-api-versioning-q4)
  - [Q5. What is media type (Accept header) API versioning?](#chapter-07-api-versioning-q5)
  - [Q6. What is the difference between breaking and non-breaking API…](#chapter-07-api-versioning-q6)
  - [Q7. What does `DefaultApiVersion` configure?](#chapter-07-api-versioning-q7)
  - [Q8. What does `AssumeDefaultVersionWhenUnspecified` do?](#chapter-07-api-versioning-q8)
  - [Q9. What is the `[ApiVersion]` attribute?](#chapter-07-api-versioning-q9)
  - [Q10. What is `ReportApiVersions`?](#chapter-07-api-versioning-q10)
  - [Q11. Why is API versioning needed?](#chapter-07-api-versioning-q11)
  - [Q12. What are the trade-offs of URL path vs header versioning?](#chapter-07-api-versioning-q12)
  - [Q13. What is a deprecation strategy for old API versions?](#chapter-07-api-versioning-q13)
  - [Q14. What is the Sunset HTTP header?](#chapter-07-api-versioning-q14)
  - [Q15. What is an additive vs breaking change in JSON APIs?](#chapter-07-api-versioning-q15)
  - [Q16. How does CDN caching interact with query-string versioning?](#chapter-07-api-versioning-q16)
  - [Q17. What is `Asp.Versioning.Mvc`?](#chapter-07-api-versioning-q17)
  - [Q18. What is the difference between versioning the URL vs version…](#chapter-07-api-versioning-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 07. API Versioning

### Q1. What is API versioning? {#chapter-07-api-versioning-q1}

What is API versioning?

**Answer:** API versioning is the practice of managing multiple concurrent versions of an API contract so existing clients continue working while new features and breaking changes ship in later versions. ASP.NET Core 8 supports versioning via the `Asp.Versioning.Mvc` package with URL, header, query, and media type readers.

- Clients specify a version explicitly or rely on a configured default version.
- Each version can have distinct controllers, actions, DTOs, and OpenAPI documents.
- Versioning decouples client migration timelines from server deployment schedules.
- Without versioning, breaking JSON field changes force all clients to update simultaneously.

---

### Q2. What is URL path versioning? {#chapter-07-api-versioning-q2}

What is URL path versioning?

**Answer:** URL path versioning embeds the version in the URI path — for example, `/api/v1/products` and `/api/v2/products`. It is the most visible and gateway-friendly strategy, making version identification obvious in logs, proxies, and browser address bars.

- Route template: `[Route("api/v{version:apiVersion}/[controller]")]` with `[ApiVersion("1.0")]` on controllers.
- Easy to configure in reverse proxies, API gateways, and CDN cache rules by path prefix.
- URL churn when versioning — clients must update base URLs for major version changes.
- Most common strategy for public REST APIs with long-lived client integrations.

---

### Q3. What is header-based API versioning? {#chapter-07-api-versioning-q3}

What is header-based API versioning?

**Answer:** Header-based versioning sends the API version in an HTTP request header — for example, `X-Api-Version: 2.0` or `api-version: 2.0`. URLs remain clean and identical across versions while the server selects the version from the header value.

- Configure with `options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version")`.
- Invisible in URLs — harder to test in a browser without tools like curl or Postman.
- CDNs and proxies must forward the version header to the origin server.
- Requires unambiguous controller/action mapping — duplicate routes without `[MapToApiVersion]` cause wrong version selection.

---

### Q4. What is query string API versioning? {#chapter-07-api-versioning-q4}

What is query string API versioning?

**Answer:** Query string versioning passes the version as a URL parameter — for example, `GET /api/products?api-version=2.0`. It is easy to add to existing APIs without changing route templates but interacts poorly with CDN caching.

- Configure with `options.ApiVersionReader = new QueryStringApiVersionReader("api-version")`.
- Clients can omit the parameter if `AssumeDefaultVersionWhenUnspecified` is enabled.
- CDNs that cache by path only may serve the wrong version body when query strings differ but share the same cache key.
- Easy for clients to forget the parameter — accidental default version usage causes silent behavior changes.

---

### Q5. What is media type (Accept header) API versioning? {#chapter-07-api-versioning-q5}

What is media type (Accept header) API versioning?

**Answer:** Media type versioning embeds the version in the `Accept` header using a vendor-specific media type — for example, `Accept: application/vnd.myapi.v2+json`. The server content-negotiates the version from the Accept header value.

- Configure with `options.ApiVersionReader = new MediaTypeApiVersionReader()`.
- Pure REST approach — version is part of the representation format, not the URL.
- Complex for clients and poorly supported by browser tools and some HTTP libraries.
- Requires custom media type registration and formatter configuration alongside versioning middleware.

---

### Q6. What is the difference between breaking and non-breaking API changes? {#chapter-07-api-versioning-q6}

What is the difference between breaking and non-breaking API changes?

**Answer:** Breaking changes alter the contract in ways that cause existing clients to fail — removing fields, renaming properties, changing types, or altering HTTP semantics. Non-breaking (additive) changes extend the contract without affecting clients that ignore new elements.

- Breaking: remove `name`, rename to `fullName` only, change `id` from int to string, change response status codes.
- Additive: add optional `fullName` alongside existing `name`, add new optional query parameters, add new endpoints.
- Breaking changes require a new major API version; additive changes are safe within the current version.
- JSON clients typically ignore unknown properties — adding fields is safe; removing or renaming is not.

---

### Q7. What does `DefaultApiVersion` configure? {#chapter-07-api-versioning-q7}

What does `DefaultApiVersion` configure?

**Answer:** `DefaultApiVersion` sets the API version applied when a client does not specify a version and `AssumeDefaultVersionWhenUnspecified` is true. It defines which version unversioned requests route to — typically the oldest supported version during migration.

- Example: `options.DefaultApiVersion = new ApiVersion(1, 0)` routes unspecified requests to v1.
- Should remain on the stable, widely deployed version until a published sunset date passes.
- Setting default to the latest major version silently upgrades clients that omit version info — a common production incident source.
- Works with all version readers — URL, header, query, and media type.

---

### Q8. What does `AssumeDefaultVersionWhenUnspecified` do? {#chapter-07-api-versioning-q8}

What does `AssumeDefaultVersionWhenUnspecified` do?

**Answer:** When `true`, requests that do not include a version identifier are routed to `DefaultApiVersion`. When `false`, unversioned requests do not match versioned routes and typically return 404.

- `true`: `/api/products` (without version segment) maps to v1 if default is 1.0 — convenient for legacy clients.
- `false`: clients must explicitly include the version — safer for public APIs where silent defaults cause accidental breaking upgrades.
- The choice is contractual — document whether omitting the version is supported and which version applies.
- Changing this setting on deploy can shift behavior for all clients that omit version information.

---

### Q9. What is the `[ApiVersion]` attribute? {#chapter-07-api-versioning-q9}

What is the `[ApiVersion]` attribute?

**Answer:** `[ApiVersion]` declares which API version(s) a controller or action supports — for example, `[ApiVersion("1.0")]` or `[ApiVersion("2.0")]`. Combined with `[MapToApiVersion]`, it maps specific actions to specific versions on shared controllers.

- A controller can declare multiple versions: `[ApiVersion("1.0")] [ApiVersion("2.0")]`.
- `[MapToApiVersion("2.0")]` on an action restricts it to v2 even when the controller supports both versions.
- Works with `[ApiVersionNeutral]` for endpoints that serve all versions (health checks, metadata).
- The version reader (URL, header, query) selects which version the client requests; `[ApiVersion]` declares what the server offers.

---

### Q10. What is `ReportApiVersions`? {#chapter-07-api-versioning-q10}

What is `ReportApiVersions`?

**Answer:** When `ReportApiVersions` is enabled, ASP.NET Core adds response headers listing supported API versions — typically `api-supported-versions` and `api-deprecated-versions`. Clients discover available versions without consulting external documentation.

- Example response header: `api-supported-versions: 1.0, 2.0`.
- Helps client developers identify which versions are active and which are deprecated.
- Pair with deprecation headers (`Deprecation`, `Sunset`) for migration planning.
- Configure in `AddApiVersioning(options => options.ReportApiVersions = true)`.

---

### Q11. Why is API versioning needed? {#chapter-07-api-versioning-q11}

Why is API versioning needed?

**Answer:** API versioning lets server teams evolve the API — add features, fix design mistakes, and restructure payloads — without forcing all clients to update simultaneously. It provides a migration window where old and new contracts coexist.

- Mobile apps and partner integrations cannot update instantly — they need months to adopt breaking changes.
- Without versioning, any breaking JSON change is a coordinated big-bang release across all consumers.
- Supports deprecation strategies with sunset dates, giving clients time to migrate.
- Enables separate OpenAPI documents and SDK generation per version for accurate client tooling.

---

### Q12. What are the trade-offs of URL path vs header versioning? {#chapter-07-api-versioning-q12}

What are the trade-offs of URL path vs header versioning?

**Answer:** URL path versioning is visible, log-friendly, and gateway-compatible but changes URLs on major upgrades. Header versioning keeps URLs stable but is invisible to caches, harder to test in browsers, and requires proxy header forwarding.

- URL: obvious in access logs, easy CDN cache key separation, simple proxy routing rules — but URL churn for clients.
- Header: clean URLs across versions, same route templates — but hidden contract, CDN cache key issues, poor browser testability.
- URL versioning is preferred for public APIs with diverse clients; header versioning suits internal services with controlled client fleets.
- Query string versioning is a middle ground — easy to add but hostile to CDN caching without explicit cache key configuration.

---

### Q13. What is a deprecation strategy for old API versions? {#chapter-07-api-versioning-q13}

What is a deprecation strategy for old API versions?

**Answer:** A deprecation strategy communicates that an old version will be removed, gives clients a migration deadline, and monitors usage before shutdown. It combines HTTP deprecation headers, OpenAPI `deprecated: true` markers, documentation, and usage metrics.

- Send `Deprecation: true` and `Sunset: Sat, 01 Feb 2027 00:00:00 GMT` headers on deprecated version responses.
- Include `Link: <https://docs.example.com/migration>; rel="successor-version"` pointing to the replacement API.
- Mark operations `deprecated: true` in the v1 OpenAPI document.
- Monitor deprecated endpoint usage — do not remove a version until sunset date passes and usage drops below threshold.

---

### Q14. What is the Sunset HTTP header? {#chapter-07-api-versioning-q14}

What is the Sunset HTTP header?

**Answer:** The `Sunset` HTTP header specifies the date after which an endpoint or API version may be removed — for example, `Sunset: Sat, 01 Feb 2027 00:00:00 GMT`. It gives clients a concrete deadline for migration, aligned with RFC 8594 semantics.

- Pair with `Deprecation: true` to signal the endpoint is deprecated but still functional until the sunset date.
- Clients and API gateways can automate warnings or block requests after the sunset date.
- Include a `Link` header with `rel="successor-version"` pointing to the replacement endpoint or version.
- Log and monitor requests to deprecated endpoints to drive partner outreach before removal.

---

### Q15. What is an additive vs breaking change in JSON APIs? {#chapter-07-api-versioning-q15}

What is an additive vs breaking change in JSON APIs?

**Answer:** Additive changes add new optional elements without altering existing ones — new JSON properties, new endpoints, new optional query parameters. Breaking changes modify or remove existing elements that clients depend on — field renames, type changes, removed endpoints, or altered status codes.

- Additive: add `"fullName"` alongside existing `"name"` — old clients ignore the new field.
- Breaking: rename `"name"` to `"fullName"` only — old clients lose the name field.
- Additive: new `GET /api/v1/products/search` endpoint — existing endpoints unchanged.
- Breaking: change pagination from offset to cursor — clients parsing offset responses break.

---

### Q16. How does CDN caching interact with query-string versioning? {#chapter-07-api-versioning-q16}

How does CDN caching interact with query-string versioning?

**Answer:** CDNs cache responses by URL path by default — `GET /api/items?api-version=1.0` and `?api-version=2.0` may share the same cache key if the CDN ignores query strings, returning stale v1 JSON to v2 clients.

- Configure the CDN to include `api-version` in the cache key or disable caching for versioned endpoints.
- URL path versioning avoids this issue — `/api/v1/items` and `/api/v2/items` are distinct cache keys.
- `Cache-Control: public, max-age=300` on versioned responses amplifies cross-version cache pollution.
- Use `Cache-Control: private` or `no-store` for authenticated APIs during migration testing.

---

### Q17. What is `Asp.Versioning.Mvc`? {#chapter-07-api-versioning-q17}

What is `Asp.Versioning.Mvc`?

**Answer:** `Asp.Versioning.Mvc` is the NuGet package (successor to `Microsoft.AspNetCore.Mvc.Versioning`) that adds API versioning support to ASP.NET Core MVC and Web API controllers. It provides version readers, `[ApiVersion]` attributes, ApiExplorer integration, and Swagger document grouping.

- Register with `builder.Services.AddApiVersioning()` and configure readers, defaults, and reporting.
- Add `.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV")` for Swagger integration.
- Supports URL segment, header, query string, and media type version readers — combinable with `ApiVersionReader.Combine()`.
- Works with controller-based and minimal API projects through the Asp.Versioning.Http package.

---

### Q18. What is the difference between versioning the URL vs versioning the response schema? {#chapter-07-api-versioning-q18}

What is the difference between versioning the URL vs versioning the response schema?

**Answer:** URL versioning changes the endpoint address per major version — `/api/v1/orders` vs `/api/v2/orders` — with potentially different controllers and routes. Schema versioning keeps the same URL but returns different JSON shapes based on the requested version — often via header or Accept negotiation.

- URL versioning: distinct routes, controllers, and OpenAPI documents per version — clearest separation.
- Schema versioning: same route serves multiple response shapes — harder to document and test but avoids URL churn.
- URL versioning is preferred for breaking changes with different endpoint structures.
- Schema versioning suits minor representation changes within a negotiated format — more common with media type versioning.

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

#### Q1. (D) A public payments API must support v1 clients for 18 months while shipping v2 with breaking JSON shape changes. Compare URL path versioning (`/api/v2/...`), `X-Api-Version` header, and `?api-version=2.0` query — cacheability, gateway routing, discoverability, and client SDK ergonomics.

---

**Answer:**

_Answer not found._

---

#### Q2. (P) Review URL-based versioning registration. Clients calling `/api/products` without a version segment receive 404; product owner expected unversioned calls to hit v1.

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = false;
    options.ReportApiVersions = true;
});

builder.Services.AddControllers();

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok(_products.ListV1());
}
```

---

**Answer:**

_Answer not found._

---

#### Q3. (R) Review header-based versioning. Mobile app sends `X-Api-Version: 2.0` but always receives v1 payload shape; server logs show route matched v1 controller.

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

[ApiVersion("1.0")]
[Route("api/[controller]")]
public class OrdersV1Controller : ControllerBase { /* ... */ }

[ApiVersion("2.0")]
[Route("api/[controller]")]
public class OrdersV2Controller : ControllerBase { /* ... */ }
```

Both controllers registered; no `[MapToApiVersion]` or versioned route constraints on actions.

---

**Answer:**

_Answer not found._

---

#### Q4. (P) v1 of `GET /api/customers/{id}` returns `name`; v2 returns `fullName` and drops `name`. What constitutes a breaking change vs a safe additive change, and how do you document the migration in OpenAPI and release notes without breaking existing integrators?

---

**Answer:**

_Answer not found._

---

#### Q5. (P) v1 endpoints must announce deprecation before removal in six months. Review the proposed middleware and response headers for Sunset / Link / `Deprecation` signaling — what should clients and API gateways observe?

```csharp
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/v1"))
        context.Response.Headers.Append("X-Deprecated", "true");
    await next();
});
```

No `Sunset` date, no `Link` relation to v2 documentation, OpenAPI still marks v1 as current.

---

**Answer:**

_Answer not found._

---

#### Q6. (M) With `Asp.Versioning.Mvc` and Swashbuckle, explain how each API version appears as a separate OpenAPI document, how `DocInclusionPredicate` maps controller versions to `SwaggerDoc` names, and what breaks if v2 actions share DTO type names with v1.

---

**Answer:**

_Answer not found._

---

#### Q7. (R) Review query-string versioning behind a CDN. Cached responses serve v1 JSON to v2 clients intermittently.

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
});

[ApiVersion("1.0")]
[HttpGet("api/items")]
public IActionResult ListV1() => Ok(_items.ListV1());

[ApiVersion("2.0")]
[HttpGet("api/items")]
public IActionResult ListV2() => Ok(_items.ListV2());
```

CDN caches `GET /api/items` without varying on query string; `Cache-Control: public, max-age=300` set on responses.

---

**Answer:**

_Answer not found._

---

#### Q8. (D) Default version behavior: `AssumeDefaultVersionWhenUnspecified = true` with `DefaultApiVersion = 2.0` while v1 remains deployed for legacy partners. What operational and contract risks does this create, and when is implicit default versioning acceptable vs requiring explicit version on every call?



**Answer:**

_Answer not found._

---
