# API Versioning — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is API versioning?](#q1-what-is-api-versioning)
2. [Q2. What is URL path versioning?](#q2-what-is-url-path-versioning)
3. [Q3. What is header-based API versioning?](#q3-what-is-header-based-api-versioning)
4. [Q4. What is query string API versioning?](#q4-what-is-query-string-api-versioning)
5. [Q5. What is media type (Accept header) API versioning?](#q5-what-is-media-type-accept-header-api-versioning)
6. [Q6. What is the difference between breaking and non-breaking API changes?](#q6-what-is-the-difference-between-breaking-and-non-breaking-api-changes)
7. [Q7. What does `DefaultApiVersion` configure?](#q7-what-does-defaultapiversion-configure)
8. [Q8. What does `AssumeDefaultVersionWhenUnspecified` do?](#q8-what-does-assumedefaultversionwhenunspecified-do)
9. [Q9. What is the `[ApiVersion]` attribute?](#q9-what-is-the-apiversion-attribute)
10. [Q10. What is `ReportApiVersions`?](#q10-what-is-reportapiversions)
11. [Q11. Why is API versioning needed?](#q11-why-is-api-versioning-needed)
12. [Q12. What are the trade-offs of URL path vs header versioning?](#q12-what-are-the-trade-offs-of-url-path-vs-header-versioning)
13. [Q13. What is a deprecation strategy for old API versions?](#q13-what-is-a-deprecation-strategy-for-old-api-versions)
14. [Q14. What is the Sunset HTTP header?](#q14-what-is-the-sunset-http-header)
15. [Q15. What is an additive vs breaking change in JSON APIs?](#q15-what-is-an-additive-vs-breaking-change-in-json-apis)
16. [Q16. How does CDN caching interact with query-string versioning?](#q16-how-does-cdn-caching-interact-with-query-string-versioning)
17. [Q17. What is `Asp.Versioning.Mvc`?](#q17-what-is-aspversioningmvc)
18. [Q18. What is the difference between versioning the URL vs versioning the response schema?](#q18-what-is-the-difference-between-versioning-the-url-vs-versioning-the-response-schema)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is API versioning?

**Concepts**
- API versioning — multiple concurrent contract versions coexist
- Client migration window — old and new contracts live simultaneously
- `Asp.Versioning.Mvc` — versioning package for ASP.NET Core
- Version readers — URL, header, query string, media type
- Breaking changes isolated per version — clients update on their schedule

**Answer**

API versioning is the practice of managing multiple concurrent versions of an API contract so existing clients continue working while new features and breaking changes ship in later versions. Without it, any breaking JSON field change forces all clients — mobile apps, partner integrations, generated SDKs — to update simultaneously, which is operationally impractical for public APIs. ASP.NET Core 8 implements versioning via the `Asp.Versioning.Mvc` package, which provides version readers for URL segments, request headers, query string parameters, and media types. Each version can have distinct controllers, actions, DTOs, and OpenAPI documents, and the package integrates with the ApiExplorer infrastructure so Swashbuckle can generate separate versioned documents.

---

## Q2. What is URL path versioning?

**Concepts**
- URL path versioning — version embedded in URI segment
- `[Route("api/v{version:apiVersion}/[controller]")]` — route template
- Gateway-friendly — version visible in access logs and proxy rules
- URL churn — clients must update base URL on major version upgrade
- Most common strategy for public REST APIs

**Answer**

URL path versioning embeds the version in the URI path — `/api/v1/products` and `/api/v2/products` — making the version immediately visible in access logs, browser address bars, and API gateway routing rules. I configure it with `[Route("api/v{version:apiVersion}/[controller]")]` and `[ApiVersion("1.0")]` on the controller. The main advantage is that version identification is unambiguous and easy to configure in reverse proxies and CDN cache rules by path prefix. The trade-off is URL churn — every major version change requires clients to update their base URL. Despite this, URL path versioning is the most common strategy for public REST APIs with diverse, long-lived client integrations because its visibility makes debugging and routing straightforward.

---

## Q3. What is header-based API versioning?

**Concepts**
- Header versioning — version in an HTTP request header
- `HeaderApiVersionReader("X-Api-Version")` — configuration
- Clean URLs — identical across versions
- CDN forwarding requirement — version header must be proxied to origin
- Duplicate routes without `[MapToApiVersion]` — wrong version selection risk

**Answer**

Header-based versioning sends the API version in an HTTP request header such as `X-Api-Version: 2.0` rather than in the URL, keeping the URI path identical across versions. I configure it with `options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version")`. The clean URL advantage is that clients can change versions without updating any URL — only the header value changes. The downsides are that the version is invisible in URLs so it cannot be tested in a browser address bar, CDNs and reverse proxies must be configured to forward the version header to the origin server, and without `[MapToApiVersion]` on individual actions it is easy to accidentally route requests to the wrong controller version when multiple versioned controllers share the same route template.

---

## Q4. What is query string API versioning?

**Concepts**
- Query string versioning — `?api-version=2.0` parameter
- `QueryStringApiVersionReader("api-version")` — configuration
- CDN cache key — query strings ignored by default, causing cross-version pollution
- `AssumeDefaultVersionWhenUnspecified` — omitted parameter fallback
- Easy addition to existing APIs without route template changes

**Answer**

Query string versioning passes the version as a URL parameter — `GET /api/products?api-version=2.0` — making it easy to add to existing APIs without changing route templates. I configure it with `options.ApiVersionReader = new QueryStringApiVersionReader("api-version")`. The primary risk is CDN caching — most CDNs cache responses by URL path and ignore query string differences by default, so `?api-version=1.0` and `?api-version=2.0` may share the same cache entry and serve v1 JSON to v2 clients. I configure the CDN to include `api-version` in the cache key or use `Cache-Control: private` on versioned endpoints during migration. Clients can accidentally omit the parameter when `AssumeDefaultVersionWhenUnspecified` is enabled, silently routing to the default version without knowing it.

---

## Q5. What is media type (Accept header) API versioning?

**Concepts**
- Media type versioning — version in `Accept` header vendor media type
- `Accept: application/vnd.myapi.v2+json` — vendor-specific format
- `MediaTypeApiVersionReader` — reader configuration
- Complex client support — requires custom Accept header formatting
- Purest REST approach — version as part of representation format

**Answer**

Media type versioning embeds the version in the `Accept` header using a vendor-specific media type — `Accept: application/vnd.myapi.v2+json` — so the version is part of the representation format rather than the URL or a separate header. I configure it with `options.ApiVersionReader = new MediaTypeApiVersionReader()`. This is the purest REST approach from a spec perspective, but it is the most complex to implement and use. Most browser tools and HTTP client libraries do not make it easy to construct vendor media type headers, testing requires curl or Postman configuration, and the version-in-media-type approach also interacts with content negotiation formatters in non-trivial ways. I use it only when the API design explicitly calls for strict REST media type semantics.

---

## Q6. What is the difference between breaking and non-breaking API changes?

**Concepts**
- Breaking changes — removals, renames, type changes, status code changes
- Additive changes — new optional fields, new endpoints, new optional parameters
- JSON ignore-unknown — clients typically ignore new fields
- New major version — required for breaking changes
- Additive changes — safe within current version

**Answer**

Breaking changes alter the contract in ways that cause existing clients to fail without code changes — removing a field means clients that read that field get null, renaming `"name"` to `"fullName"` silently drops the value for every client using the old key, changing `id` from `int` to `string` breaks strongly typed clients, and changing HTTP status codes breaks clients with status-based error handling. Additive changes extend the contract without affecting clients that ignore new elements — adding an optional `fullName` field alongside the existing `name`, adding a new optional query parameter, or adding a new endpoint. JSON clients typically ignore unknown properties by default, which is why adding fields is safe within the current version. The rule I follow is: breaking changes require a new major version with a migration window, additive changes can ship in the current version.

---

## Q7. What does `DefaultApiVersion` configure?

**Concepts**
- `DefaultApiVersion` — version used for unspecified requests
- `AssumeDefaultVersionWhenUnspecified` — must be true for default to apply
- Stable deployed version — default should track the oldest widely-used version
- Silent upgrade risk — changing default to latest breaks clients that omit version

**Answer**

`DefaultApiVersion` sets the API version applied when a client does not include a version identifier in its request and `AssumeDefaultVersionWhenUnspecified` is `true`. I set it to the oldest stable deployed version during migration — for example, `options.DefaultApiVersion = new ApiVersion(1, 0)` — so clients that omit the version parameter continue reaching v1 without being silently upgraded. Setting the default to the latest major version is a common production incident source because any client that omits version information is silently moved to the breaking new contract. The default version should only change after a published sunset date for the old default has passed and all known clients have migrated.

---

## Q8. What does `AssumeDefaultVersionWhenUnspecified` do?

**Concepts**
- `AssumeDefaultVersionWhenUnspecified = true` — routes unversioned requests to default
- `AssumeDefaultVersionWhenUnspecified = false` — unversioned requests return 404
- Contractual choice — document whether omitting version is supported
- Silent default upgrade risk — changing default version breaks unversioned clients

**Answer**

When `AssumeDefaultVersionWhenUnspecified` is `true`, requests that do not include a version identifier are routed to `DefaultApiVersion` — so `/api/products` without a version segment matches the v1 controller when the default is 1.0. When `false`, unversioned requests do not match any versioned route and return 404, forcing clients to always be explicit about their version. I prefer `false` for new public APIs because explicit versioning makes the contract clearer and prevents accidental silent upgrades when the default version changes. For APIs migrating from an unversioned state, `true` provides backward compatibility for legacy clients that cannot be updated to include version information before the migration window closes. The choice must be documented — clients need to know whether omitting the version is a supported contract.

---

## Q9. What is the `[ApiVersion]` attribute?

**Concepts**
- `[ApiVersion]` — declares supported versions on a controller
- Multiple versions on one controller — `[ApiVersion("1.0")] [ApiVersion("2.0")]`
- `[MapToApiVersion]` — restricts an action to a specific version on a shared controller
- `[ApiVersionNeutral]` — endpoint serves all versions
- Version reader selects requested version; `[ApiVersion]` declares what server offers

**Answer**

`[ApiVersion]` declares which API version or versions a controller supports — `[ApiVersion("1.0")]` or `[ApiVersion("2.0")]`. A controller can declare support for multiple versions simultaneously, with `[MapToApiVersion("2.0")]` on individual actions to restrict specific actions to a specific version within a shared controller class. This pattern is useful when v1 and v2 share most actions but differ in a few. `[ApiVersionNeutral]` marks a controller or action as version-agnostic — health check and metadata endpoints that should respond regardless of which version the client requests. The version reader determines which version the client requests from the URL, header, or query string; `[ApiVersion]` declares what versions the server makes available for routing to match against.

---

## Q10. What is `ReportApiVersions`?

**Concepts**
- `ReportApiVersions` — adds version headers to every response
- `api-supported-versions` response header — lists active versions
- `api-deprecated-versions` response header — lists deprecated versions
- Client discovery — no external documentation needed
- Pair with `Deprecation` / `Sunset` headers

**Answer**

When `ReportApiVersions` is enabled, ASP.NET Core adds response headers to every API response listing the supported and deprecated versions — typically `api-supported-versions: 1.0, 2.0` and `api-deprecated-versions: 0.9`. This allows client developers to discover which versions are active and which are on the way out without consulting separate documentation, since every API response carries that information. I configure it in `AddApiVersioning(options => options.ReportApiVersions = true)` and pair it with `Deprecation: true` and `Sunset: <date>` headers on deprecated version responses so client developers can monitor their API usage and plan migrations proactively.

---

## Q11. Why is API versioning needed?

**Concepts**
- Client migration window — multiple versions coexist
- Mobile apps and partner integrations — cannot update instantly
- Breaking change isolation — old contract stable while new ships
- Separate OpenAPI documents — accurate per-version SDK generation
- Sunset strategy — deprecation with deadlines

**Answer**

API versioning is needed because clients cannot update instantaneously — mobile app releases go through store review cycles, partner integrations have their own development timelines, and enterprise consumers may have frozen codebases with long change windows. Without versioning, any breaking change to the JSON contract is a coordinated big-bang release that requires every consumer to update simultaneously, which is operationally impossible for a public API with many integrators. Versioning provides a migration window where old and new contracts coexist, giving clients months to adopt the breaking change on their own schedule. It also enables accurate per-version OpenAPI documents and SDK generation, so v2 clients get v2 types without being confused by v1 schemas.

---

## Q12. What are the trade-offs of URL path vs header versioning?

**Concepts**
- URL path — visible, log-friendly, gateway-compatible, URL churn on upgrade
- Header versioning — clean URLs, hidden contract, CDN cache issues
- Query string — easy to add, CDN hostile without cache key config
- Public APIs — URL path preferred for diverse client support
- Internal service fleets — header versioning practical with controlled clients

**Answer**

URL path versioning is visible in access logs, easy to configure in reverse proxy routing rules and CDN cache keys by path prefix, and unambiguous for debugging — the trade-off is URL churn when clients update to a new major version. Header versioning keeps URLs identical across versions, which avoids client-side URL updates, but the version is invisible in logs, cannot be tested in a browser address bar without extra tooling, and requires proxy and CDN configuration to forward the header to the origin. Query string versioning is the easiest to retrofit onto an existing API without route template changes but is hostile to CDN caching unless the CDN is explicitly configured to vary the cache key on the `api-version` parameter. I use URL path versioning for public APIs with diverse clients because the visibility and simplicity outweigh the URL churn cost, and header versioning for internal service-to-service APIs where I control all clients.

---

## Q13. What is a deprecation strategy for old API versions?

**Concepts**
- `Deprecation: true` response header — RFC 8594 deprecation signal
- `Sunset: <date>` response header — removal deadline
- `Link: <docs>; rel="successor-version"` — migration destination
- OpenAPI `deprecated: true` — marks operations in the schema
- Usage monitoring — do not remove until traffic drops below threshold

**Answer**

A deprecation strategy gives clients advance notice with a concrete timeline before a version is removed. I send `Deprecation: true` and `Sunset: <date>` headers on all responses from deprecated version endpoints, pointing clients toward the migration documentation via `Link: <https://docs.example.com/v2-migration>; rel="successor-version"`. I also mark all operations in the v1 OpenAPI document as `deprecated: true` so the Swagger UI and generated SDKs surface warnings to developers. The sunset date must be far enough in advance for the slowest client to migrate — typically three to six months for partner APIs. Critically, I monitor deprecated endpoint traffic and do not remove the version until usage drops below an agreed threshold after the sunset date, since some clients may have missed the communication.

---

## Q14. What is the Sunset HTTP header?

**Concepts**
- `Sunset` HTTP header — RFC 8594 removal deadline date
- `Deprecation: true` — companion signal that endpoint is deprecated
- `Link` header with `rel="successor-version"` — migration target
- Machine-readable deadline — API gateways and monitoring can automate on it
- Log deprecated endpoint requests — drive partner outreach

**Answer**

The `Sunset` HTTP header specifies the date after which an endpoint or API version may be removed — for example, `Sunset: Sat, 01 Feb 2027 00:00:00 GMT` — giving clients a concrete machine-readable deadline for migration aligned with RFC 8594. I pair it with `Deprecation: true` to signal that the endpoint is deprecated but still functional until the sunset date. API gateways and monitoring tools can parse the Sunset date and automate warnings or alerts for clients approaching the deadline. The `Link` header with `rel="successor-version"` completes the signaling by pointing to the replacement endpoint or version documentation. I also log requests to deprecated endpoints and use the data for partner outreach — direct communication to active consumers before removal is more reliable than relying on passive header discovery.

---

## Q15. What is an additive vs breaking change in JSON APIs?

**Concepts**
- Additive change — new optional fields, new endpoints, new optional parameters
- Breaking change — field removal, rename, type change, status code change
- JSON ignore-unknown property behavior — clients skip new fields
- Backward compatible — old clients keep working after additive change
- New major version required — for any breaking change

**Answer**

Additive changes extend the contract without removing or altering anything existing — adding a `"fullName"` property alongside `"name"`, adding a new optional query parameter, or adding a new endpoint. Old clients that do not know about the new fields simply ignore them, which is the default behavior of most JSON deserializers. Breaking changes modify or remove something existing clients depend on — renaming `"name"` to `"fullName"` with no fallback causes every client reading the old key to get null, changing `id` from `int` to `string` breaks typed deserializers, and altering pagination from offset to cursor breaks every client page-walking the list. The rule is strict: any change that causes existing clients to fail without code modification requires a new major version with a migration window.

---

## Q16. How does CDN caching interact with query-string versioning?

**Concepts**
- CDN cache key — URL path only by default, ignores query strings
- Cross-version cache pollution — v1 response served to v2 client
- CDN query string cache key inclusion — explicit configuration required
- URL path versioning — distinct cache keys by path, no configuration needed
- `Cache-Control: private` or `no-store` — per-endpoint mitigation

**Answer**

CDNs cache responses by URL path by default and typically ignore query string parameters in the cache key. This means `GET /api/items?api-version=1.0` and `GET /api/items?api-version=2.0` share the same cache entry, and whichever response is cached first is served to both clients — v1 JSON to v2 clients or v2 JSON to v1 clients. I fix this by configuring the CDN to include `api-version` in the cache key, or by setting `Cache-Control: private` on versioned endpoints so intermediaries do not cache them. URL path versioning avoids this problem entirely since `/api/v1/items` and `/api/v2/items` are distinct cache keys that need no special CDN configuration. `Cache-Control: public, max-age=300` on query-string versioned responses amplifies the cross-version pollution across multiple clients.

---

## Q17. What is `Asp.Versioning.Mvc`?

**Concepts**
- `Asp.Versioning.Mvc` — NuGet package successor to `Microsoft.AspNetCore.Mvc.Versioning`
- `AddApiVersioning()` — service registration and reader configuration
- `.AddApiExplorer()` — Swagger integration with group name format
- `ApiVersionReader.Combine()` — multiple version readers simultaneously
- Works with controller-based and minimal APIs

**Answer**

`Asp.Versioning.Mvc` is the NuGet package that adds API versioning support to ASP.NET Core MVC and Web API, succeeding the deprecated `Microsoft.AspNetCore.Mvc.Versioning`. I register it with `builder.Services.AddApiVersioning(options => { options.DefaultApiVersion = new ApiVersion(1, 0); options.AssumeDefaultVersionWhenUnspecified = true; options.ReportApiVersions = true; })`. For Swagger integration I chain `.AddApiExplorer(options => { options.GroupNameFormat = "'v'VVV"; options.SubstituteApiVersionInUrl = true; })` which sets the ApiExplorer group name format used by `DocInclusionPredicate` to route actions to versioned documents. Multiple version readers can be combined with `ApiVersionReader.Combine(new UrlSegmentApiVersionReader(), new HeaderApiVersionReader("X-Api-Version"))` for APIs that need to support both strategies simultaneously.

---

## Q18. What is the difference between versioning the URL vs versioning the response schema?

**Concepts**
- URL versioning — distinct routes and controllers per version
- Schema versioning — same URL, different JSON shape by version
- URL versioning — clearest separation, distinct OpenAPI documents
- Schema versioning — URL stability, harder to document and test
- Media type versioning — common mechanism for schema-only versioning

**Answer**

URL versioning changes the endpoint address per major version — `/api/v1/orders` routes to a v1 controller returning a v1 DTO shape, while `/api/v2/orders` routes to a separate v2 controller returning a v2 DTO shape. The two versions have distinct routes, distinct controllers or actions, and separate OpenAPI documents, which is the clearest separation and easiest to test. Schema versioning keeps the same URL but returns different JSON shapes based on the requested version — typically communicated through a header or Accept media type — so the routing stays identical but the response payload varies. Schema versioning avoids URL churn but is significantly harder to document accurately since one endpoint URL represents multiple contracts, and testing requires version-specific client configurations. I prefer URL versioning for breaking changes with distinct structural differences and schema versioning only for minor representation adjustments within a negotiated media type.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

#### Gotcha 1. POST returning 200 instead of 201

**Concepts**
- HTTP 201 Created — correct status for resource creation
- Location header — URI of the new resource
- `CreatedAtAction` — sets both status and Location

**Answer**

A POST that creates a resource must return 201 Created with a Location header, not 200 OK. I use `CreatedAtAction(nameof(Get), new { id = newEntity.Id }, newEntity)` since it sets both the correct status code and the Location header. Returning 200 hides the resource location from HTTP client libraries and OpenAPI-generated SDKs.

---

#### Gotcha 2. GET that mutates state

**Concepts**
- HTTP GET — safe and idempotent
- Browser prefetch and CDN cache replay
- POST/PUT/PATCH/DELETE — correct verbs for mutations

**Answer**

GET must be safe and idempotent — browsers prefetch URLs, CDNs cache and replay GET responses, and crawlers follow links without user intent. A side-effecting GET runs its mutation uncontrollably. I keep GET read-only and use the appropriate mutation verb.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Concepts**
- HTTP status codes — semantic failure signaling
- `ProblemDetails` / `ValidationProblemDetails` — RFC 7807 error bodies
- 200 masking failures — invisible in APM and gateways

**Answer**

Returning 200 with a failure flag forces every consumer to parse the body to detect failure. I return `ValidationProblemDetails` with 400 for validation failures, 404 for missing resources, 409 for conflicts, and 422 for domain violations so the HTTP layer carries the failure signal.

---

#### Gotcha 4. Returning EF entities from API actions

**Concepts**
- EF navigation properties — lazy-load triggers during serialization
- Circular references — serializer loop risk
- DTOs — explicit public contract, no schema leakage

**Answer**

EF Core entities expose internal columns, navigation properties, and circular references. Lazy-loaded navigations trigger SQL during JSON writing and circular references cause serializer loops. I map entities to response DTOs before returning from actions.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Concepts**
- `System.Text.Json` camelCase default
- Silent binding failure — PascalCase keys arrive as null
- `PropertyNameCaseInsensitive` — migration compatibility

**Answer**

ASP.NET Core 8 defaults to camelCase JSON, so legacy clients sending PascalCase keys get null bindings and a silent success response with wrong data. The migration fix is `PropertyNameCaseInsensitive = true`; the permanent fix is for the client to adopt camelCase.

---

#### Gotcha 6. GET with `[FromBody]`

**Concepts**
- GET body — stripped by clients and proxies
- `[FromQuery]` — correct source for GET filters
- `POST /search` — for complex filter payloads

**Answer**

Most HTTP clients and proxies strip GET request bodies, so `[FromBody]` on GET actions fails silently with null models. I use `[FromQuery]` for filter parameters and a `POST /search` endpoint for complex objects.

---

#### Gotcha 7. CORS as server security

**Concepts**
- CORS — browser-only enforcement
- Non-browser clients — unaffected
- Authentication and authorization — real API security boundary

**Answer**

CORS is a browser policy — curl, Postman, and server-to-server clients are unaffected. Authentication and authorization middleware protect the API from all unauthorized callers regardless of CORS configuration.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Concepts**
- `AllowAnyOrigin()` — wildcard origin, incompatible with credentials
- `WithOrigins` — explicit allowlist for credentialed requests
- CORS specification — forbids wildcard + credentials

**Answer**

The CORS specification forbids combining `Access-Control-Allow-Origin: *` with credentials. When the SPA sends cookies or an Authorization header I use `WithOrigins("https://app.example.com").AllowCredentials()`.

---

#### Gotcha 9. Swagger UI exposed in Production

**Concepts**
- Swagger UI in production — full API surface exposed
- `IsDevelopment()` environment check
- OpenAPI JSON for CI vs interactive UI for developers

**Answer**

Swagger UI in production exposes every endpoint and schema for reconnaissance. I gate `UseSwagger()` and `UseSwaggerUI()` behind `if (app.Environment.IsDevelopment())` and serve the JSON document separately for CI tooling through an IP-restricted path.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Concepts**
- `[ApiController]` — automatic validation, binding inference
- Inconsistent error contracts — mixed controller setup

**Answer**

Without `[ApiController]`, automatic 400 `ValidationProblemDetails` responses and binding inference do not apply, causing inconsistent error contracts. I apply `[ApiController]` at the assembly level.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Concepts**
- `.Result` / `.Wait()` — sync-over-async blocking
- Thread-pool starvation — blocked threads reduce throughput
- `async Task<IActionResult>` — correct signature

**Answer**

Blocking on `.Result` ties up thread-pool threads, reducing concurrent capacity. I mark actions `async Task<IActionResult>` and propagate `await` through the service layer.

---

#### Gotcha 12. Liveness probe includes SQL check

**Concepts**
- Liveness probe — pod restart signal
- Readiness probe — load balancer exclusion
- SQL down — dependency failure, not pod failure

**Answer**

A failed liveness probe causes Kubernetes to restart the pod. SQL being down cannot be healed by restarting the app, so the SQL check belongs on the readiness probe.

---

#### Gotcha 13. N+1 queries in list endpoints

**Concepts**
- N+1 query problem — one SQL per row
- DTO projection with `Select` — single JOIN query
- `Include` / `ThenInclude` — eager load

**Answer**

Serializing entities with lazy-loaded navigation properties triggers one SQL query per row. I fix this by projecting to DTOs in LINQ for a single query, or using `Include`/`ThenInclude` for explicit eager loads.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Concepts**
- Offset pagination — shifts on concurrent mutations
- Keyset pagination — stable cursor on indexed key
- Cursor tokens in response metadata

**Answer**

`Skip`/`Take` shifts when rows are inserted or deleted concurrently. Keyset pagination anchors on the last seen key — `WHERE id > @lastId ORDER BY id LIMIT @pageSize` — which is stable under concurrent mutations.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Concepts**
- Field resolvers — per-parent-row execution by default
- DataLoader — batches sub-queries within a request
- HotChocolate DataLoader registration in DI

**Answer**

Field resolvers in HotChocolate execute per parent row — 100 authors with a `books` resolver executes 101 queries. DataLoader collects all keys within a request phase and dispatches one batched query. I register DataLoader classes scoped to the request.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Concepts**
- Native gRPC — HTTP/2 binary framing inaccessible to browsers
- gRPC-Web — browser-compatible translation
- `AddGrpcWeb()` / `EnableGrpcWeb()` and CORS

**Answer**

Browsers cannot access native gRPC's HTTP/2 framing. gRPC-Web wraps messages in a format browsers can use via Fetch, enabled by `AddGrpcWeb()` and `EnableGrpcWeb()`. Cross-origin calls also need CORS configured.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (D) A public payments API must support v1 clients for 18 months while shipping v2 with breaking JSON shape changes. Compare URL path versioning (`/api/v2/...`), `X-Api-Version` header, and `?api-version=2.0` query — cacheability, gateway routing, discoverability, and client SDK ergonomics.

---

**Concepts**
- URL path versioning — distinct cache keys, visible in logs, URL churn
- Header versioning — clean URLs, CDN forwarding required, invisible to browser
- Query string versioning — CDN cache key pollution without explicit config
- Gateway routing — URL path simplest, header requires header-based rules
- Client SDK ergonomics — URL base change vs header config vs query param

**Answer**

For an 18-month coexistence window on a public payments API with diverse clients, URL path versioning is the strongest choice because it satisfies all four dimensions reliably. Cache keys are distinct by path — `/api/v1/payments` and `/api/v2/payments` — requiring no CDN configuration. Gateway routing uses simple path prefix rules. Clients see the version in the URL and can confirm which version they are calling from access logs without additional tooling. The SDK ergonomic cost is that clients change their base URL from `/api/v1` to `/api/v2`, which is a one-time, explicit, and reviewable change.

Header versioning with `X-Api-Version` keeps URLs stable, which sounds attractive, but CDNs that cache by path without forwarding the header to vary the cache key will serve the wrong version body. Every client must configure the custom header, which requires updating HTTP client settings rather than a URL string — more error-prone for partners without strong SDK support. Gateway routing requires header-based rules which are less universal than path prefix rules in older APIM products.

Query string versioning is the riskiest for a payments API — CDN cache pollution is a critical issue since a v1 response cached for `GET /api/payments?api-version=1.0` may be served to a v2 client that uses `?api-version=2.0` if the CDN ignores query strings. For payments, serving a stale or wrong-version response is not just a bug but a financial correctness issue. Query string versioning requires CDN configuration changes that the API team may not control, making it operationally fragile for a public API.

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

**Concepts**
- `AssumeDefaultVersionWhenUnspecified = false` — unversioned requests return 404
- Route template `v{version:apiVersion}` — requires version segment in URL
- Product owner expectation — unversioned URL should route to v1
- `AssumeDefaultVersionWhenUnspecified = true` — enables default fallback
- `SubstituteApiVersionInUrl = true` — ApiExplorer integration for Swagger

**Answer**

The 404 is caused by `AssumeDefaultVersionWhenUnspecified = false`. The route template `api/v{version:apiVersion}/products` requires a version segment in the URL — a request to `/api/products` has no version segment and no fallback is configured, so routing finds no matching endpoint and returns 404.

The fix is to change `options.AssumeDefaultVersionWhenUnspecified = true`. This tells the versioning middleware to treat unversioned requests as if they specified `DefaultApiVersion` (1.0), so `/api/products` routes to the v1 controller. The route template `v{version:apiVersion}` also needs to handle this case — with `SubstituteApiVersionInUrl = true` configured in `.AddApiExplorer()`, the Swagger UI substitutes the version into the path, but routing for unversioned calls requires that the version segment be optional or omitted from the template. The cleaner production approach is to keep `AssumeDefaultVersionWhenUnspecified = true` and document that unversioned requests are equivalent to version 1.0 in the API changelog, so partners are aware of the default.

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

**Concepts**
- Route template collision — both controllers share `api/[controller]` path
- Controller name convention — `[controller]` strips the `V1`/`V2` suffix
- `[MapToApiVersion]` — restricts actions to specific versions on shared routes
- Header version reader — reads `X-Api-Version` correctly
- Route ambiguity — framework picks first match when routes collide

**Answer**

The issue is route template collision. Both `OrdersV1Controller` and `OrdersV2Controller` use `[Route("api/[controller]")]`, and the `[controller]` token strips the `V1`/`V2` suffix by convention, so both resolve to `api/orders`. When the framework sees two controllers matching the same path, it uses the first registered one — which is v1 — regardless of the `X-Api-Version` header. The header reader is working correctly and extracting `2.0` from the header, but routing has already resolved to the v1 controller before versioning can select between them.

The fix is to give both controllers the same route template explicitly and rely on versioning to disambiguate, which requires that the route templates truly match. Rather than `[Route("api/[controller]")]` with the version suffix stripped inconsistently, I would rename both controllers to `OrdersController`, apply `[ApiVersion("1.0")]` and `[ApiVersion("2.0")]` respectively, and use `[MapToApiVersion]` on conflicting actions within a shared controller, or keep separate controller classes with `[ApiVersion]` and identical `[Route("api/orders")]` templates so the versioning middleware can select between them correctly.

---

#### Q4. (P) v1 of `GET /api/customers/{id}` returns `name`; v2 returns `fullName` and drops `name`. What constitutes a breaking change vs a safe additive change, and how do you document the migration in OpenAPI and release notes without breaking existing integrators?

---

**Concepts**
- Breaking change — `name` field removal breaks existing clients
- Additive migration path — `fullName` added alongside `name` in a transitional step
- OpenAPI `deprecated` field — marks old properties in schema
- Release notes and changelogs — communicate changes to integrators
- Version sunset — remove `name` only after confirmed client migration

**Answer**

Removing `name` and replacing it with `fullName` is a breaking change — every client reading `response.name` gets null after the switch, which silently corrupts any display or business logic depending on that field. There is no JSON-level backward compatibility mechanism for field removal.

The correct migration path has three stages. In v1 I add `fullName` as an additional field alongside `name` — this is an additive change that existing clients ignore — and mark `name` as deprecated in the OpenAPI schema using an `[Obsolete]` comment or a Swashbuckle schema filter that adds `"deprecated": true` to the `name` property. The release notes announce the additive change and the sunset timeline for `name`. In v2, released after the migration window, I remove `name` and keep only `fullName`. Clients that read the deprecation signals in the OpenAPI schema, the `Deprecation` response header on v1 endpoints, and the release notes have time to migrate before the breaking change ships. I monitor which clients are still sending requests to v1 and do direct outreach to integrators who have not moved before the v1 sunset date.

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

**Concepts**
- RFC 8594 `Deprecation` header — standard signal, not `X-Deprecated`
- `Sunset` header — RFC 8594 concrete removal deadline
- `Link` header with `rel="successor-version"` — migration destination
- OpenAPI `deprecated: true` — marks operations in schema for tooling
- `X-Deprecated` — non-standard, ignored by API gateways and monitoring tools

**Answer**

The proposed middleware uses `X-Deprecated: true` which is a non-standard header — API gateways, SDK generators, and monitoring tools that follow RFC 8594 will not recognize it. The correct header is `Deprecation: true` per RFC 8594, which standard tooling and generated clients parse for migration warnings. Without a `Sunset` date, clients have no concrete deadline to plan against, which means they treat the deprecation as optional and indefinitely defer migration.

The complete signaling should be:

```csharp
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/v1"))
    {
        context.Response.Headers.Append("Deprecation", "true");
        context.Response.Headers.Append("Sunset", "Sat, 01 Mar 2027 00:00:00 GMT");
        context.Response.Headers.Append("Link", "<https://docs.example.com/v2-migration>; rel=\"successor-version\"");
    }
    await next();
});
```

The `Sunset` date gives clients a concrete deadline six months out. The `Link` header points directly to the migration guide so clients do not have to search for documentation. I also update the v1 OpenAPI document to mark all operations as `deprecated: true` so Swagger UI shows warnings and NSwag-generated clients emit deprecation warnings at compile time. Finally I add monitoring on v1 endpoint request counts with an alert threshold so the team knows which clients are still active as the sunset date approaches.

---

#### Q6. (M) With `Asp.Versioning.Mvc` and Swashbuckle, explain how each API version appears as a separate OpenAPI document, how `DocInclusionPredicate` maps controller versions to `SwaggerDoc` names, and what breaks if v2 actions share DTO type names with v1.

---

**Concepts**
- `SwaggerDoc("v1", ...)` / `SwaggerDoc("v2", ...)` — separate document registrations
- `DocInclusionPredicate` — `(docName, apiDesc) => apiDesc.GroupName == docName`
- `GroupNameFormat` — sets ApiExplorer group name from `[ApiVersion]`
- Schema ID collision — v1 and v2 `OrderDto` produce merged schema
- `CustomSchemaIds` — required when DTO names are reused across versions

**Answer**

`Asp.Versioning.Mvc` populates the ApiExplorer `GroupName` for each action based on the `[ApiVersion]` attribute on its controller and the `GroupNameFormat` configured in `.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV")` — so a v1 controller produces `GroupName = "v1"` and a v2 controller produces `GroupName = "v2"`.

`AddSwaggerGen` registers a separate document for each version with `options.SwaggerDoc("v1", new OpenApiInfo { ... })` and `options.SwaggerDoc("v2", ...)`. The `DocInclusionPredicate` then routes each action to the matching document: `options.DocInclusionPredicate((docName, apiDesc) => apiDesc.GroupName == docName)` ensures v1 actions appear only in the `"v1"` document and v2 actions appear only in the `"v2"` document.

If v1 and v2 both have a class named `OrderDto` in different namespaces, Swashbuckle assigns them both the same schema ID `"OrderDto"`. The second one processed silently overwrites the first in `components.schemas`, producing one schema with properties from only one of the types. Generated TypeScript or C# clients then use the wrong schema for one version — v2 clients may get v1 property names, or vice versa. The fix is `options.CustomSchemaIds(type => type.FullName?.Replace("+", "."))` in `AddSwaggerGen`, which produces `"Acme.Api.Contracts.v1.OrderDto"` and `"Acme.Api.Contracts.v2.OrderDto"` as distinct schema IDs, preserving both schemas independently in both documents.

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

**Concepts**
- CDN cache key — path only by default, ignores `?api-version`
- Cross-version cache pollution — v1 response served to v2 clients
- `Cache-Control: public, max-age=300` — amplifies cross-version sharing
- CDN query string cache key configuration — `api-version` must be included
- URL path versioning — eliminates the problem without CDN configuration

**Answer**

The CDN caches `GET /api/items` by path without varying on the `api-version` query string. The first response to arrive — say v1 — is cached under the key `/api/items`, and all subsequent requests regardless of the `?api-version=2.0` parameter hit the same cache entry and receive the stale v1 response. `Cache-Control: public, max-age=300` makes this worse because the 5-minute TTL ensures the wrong version persists across many requests.

The immediate fix is to change `Cache-Control` to `private` or `no-store` for these versioned endpoints, which prevents the CDN from caching them at all — this stops the cross-version pollution but loses CDN benefits entirely. The proper fix is to configure the CDN to include `api-version` in the cache key, which varies cached responses by version. Most CDN providers support this through cache key rules or query string allowlist configuration. The lasting architectural fix is to switch to URL path versioning — `/api/v1/items` and `/api/v2/items` are distinct cache keys by path, requiring no CDN configuration changes and eliminating the problem class entirely.

---

#### Q8. (D) Default version behavior: `AssumeDefaultVersionWhenUnspecified = true` with `DefaultApiVersion = 2.0` while v1 remains deployed for legacy partners. What operational and contract risks does this create, and when is implicit default versioning acceptable vs requiring explicit version on every call?

---

**Concepts**
- Default version as silent upgrade — unversioned clients move to v2 automatically
- Legacy partner impact — unversioned calls break if v2 is a breaking change from v1
- Implicit default risk — accidental version bump on default change
- Explicit versioning — safer for public APIs with breaking changes
- `ReportApiVersions` — helps clients discover what version they are hitting

**Answer**

Setting `DefaultApiVersion = 2.0` with `AssumeDefaultVersionWhenUnspecified = true` silently moves every client that omits a version parameter from v1 to v2. If v2 has breaking JSON shape changes — which the scenario implies — every legacy partner that calls `/api/products` without `?api-version=1.0` or the `v1` path segment suddenly receives v2 responses without any request-side change on their part. They may not immediately notice if the broken field is in an infrequently read path, which means the incident surfaces in production data corruption rather than a clean error.

The operational risks are: immediate breakage for any partner relying on the default, silent data corruption if the partner's application does not validate response shapes, and the absence of any monitoring signal since the request succeeds with a 200 using the new version. The contract risk is that partners have a reasonable expectation that a default they relied on continues to behave consistently unless explicitly notified of a change.

Implicit default versioning is acceptable when the default version is stable and the team has confirmed — through traffic monitoring and partner communication — that no clients rely on unversioned behavior. It is also acceptable for internal service-to-service APIs where all clients are controlled and can be updated atomically. For public APIs with breaking changes, I require explicit versioning on every call and set `AssumeDefaultVersionWhenUnspecified = false`, or at minimum keep the default pinned to the oldest supported version until after the v2 migration window closes and all known partners have explicitly opted into v2.
