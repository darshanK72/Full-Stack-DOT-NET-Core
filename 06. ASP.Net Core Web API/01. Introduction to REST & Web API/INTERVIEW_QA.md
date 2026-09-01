# Introduction to REST & Web API — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is REST?](#q1-what-is-rest)
2. [Q2. What is a Web API?](#q2-what-is-a-web-api)
3. [Q3. What are the main REST architectural constraints?](#q3-what-are-the-main-rest-architectural-constraints)
4. [Q4. What is HTTP idempotency, and which methods are idempotent?](#q4-what-is-http-idempotency-and-which-methods-are-idempotent)
5. [Q5. What is the difference between PUT and POST?](#q5-what-is-the-difference-between-put-and-post)
6. [Q6. What is the difference between PUT and PATCH?](#q6-what-is-the-difference-between-put-and-patch)
7. [Q7. What does it mean for an HTTP method to be "safe"?](#q7-what-does-it-mean-for-an-http-method-to-be-safe)
8. [Q8. When should an API return HTTP 201 Created vs 200 OK?](#q8-when-should-an-api-return-http-201-created-vs-200-ok)
9. [Q9. When should an API return HTTP 204 No Content?](#q9-when-should-an-api-return-http-204-no-content)
10. [Q10. What is the difference between HTTP 400 Bad Request and 404 Not Found?](#q10-what-is-the-difference-between-http-400-bad-request-and-404-not-found)
11. [Q11. What is HATEOAS?](#q11-what-is-hateoas)
12. [Q12. What is RPC-style API design vs RESTful resource design?](#q12-what-is-rpc-style-api-design-vs-restful-resource-design)
13. [Q13. What is the difference between REST and SOAP?](#q13-what-is-the-difference-between-rest-and-soap)
14. [Q14. Why should GET requests not perform state-changing operations?](#q14-why-should-get-requests-not-perform-state-changing-operations)
15. [Q15. What is a REST resource vs a REST collection?](#q15-what-is-a-rest-resource-vs-a-rest-collection)
16. [Q16. What is the purpose of the Location header on a 201 response?](#q16-what-is-the-purpose-of-the-location-header-on-a-201-response)
17. [Q17. What is API versioning and why is it needed?](#q17-what-is-api-versioning-and-why-is-it-needed)
18. [Q18. What makes an endpoint "RESTful" vs merely "HTTP-based"?](#q18-what-makes-an-endpoint-restful-vs-merely-http-based)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is REST?

**Concepts**
- Resource-oriented URLs identifying server-side data
- Stateless request/response — server holds no client session between calls
- Uniform interface: HTTP verbs expressing intent (GET/POST/PUT/PATCH/DELETE)
- Representations transferring resource state in JSON or XML
- Architectural style, not a protocol

**Answer**

REST (Representational State Transfer) is an architectural style for distributed hypermedia systems that uses standard HTTP methods, stateless request/response exchanges, and resource-oriented URLs. Resources are identified by URIs like `/api/orders/42`, not by action names in the path, and each request is self-contained since the server holds no client session state between calls. HTTP verbs express intent — GET reads, POST creates, PUT replaces, PATCH partially updates, DELETE removes — so the verb and noun together describe the operation completely. ASP.NET Core Web API implements REST through attribute-routed controllers, standard status codes, and content negotiation.

---

## Q2. What is a Web API?

**Concepts**
- HTTP-based service returning structured data (JSON) rather than HTML
- `AddControllers()`, `[ApiController]`, and `MapControllers()` pipeline
- Clients including browsers, mobile apps, microservices, and partner systems
- HTTP methods, status codes, and headers as the machine-readable contract

**Answer**

A Web API is an HTTP-based service that exposes application functionality to clients over the network, returning structured data — typically JSON — rather than HTML pages. Clients include browsers running SPAs, mobile apps, partner systems, and microservices. In ASP.NET Core, Web APIs use `AddControllers()`, `[ApiController]`, and `MapControllers()` in the middleware pipeline, with authentication, validation, and serialization handled by the framework before and after action execution. The key distinction from MVC views is that APIs return machine-readable payloads, so HTTP status codes and headers carry the same weight as the response body in conveying outcome.

---

## Q3. What are the main REST architectural constraints?

**Concepts**
- Client-server separation of UI and data concerns
- Statelessness — each request self-contained
- Cacheability declared by responses
- Uniform interface via standard HTTP methods and URIs
- Layered system making proxies and gateways transparent
- Code-on-demand (optional)

**Answer**

REST defines six constraints. Client-server separation keeps UI concerns on the client and data operations behind the API boundary, enabling them to evolve independently. Statelessness means each request carries enough context — auth, representation format — since the server holds no client session state. Cacheability lets responses declare whether they can be cached so intermediaries reduce load appropriately. The uniform interface uses standard HTTP methods, resource identification via URI, and self-descriptive messages so any client can integrate predictably. The layered system means clients cannot tell whether they talk to the origin server, a gateway, or a cache — proxies and load balancers are transparent. Code-on-demand is optional and rarely used in JSON APIs.

---

## Q4. What is HTTP idempotency, and which methods are idempotent?

**Concepts**
- Idempotency — repeating the same request produces the same side-effect state
- GET, PUT, DELETE, HEAD, OPTIONS as idempotent methods
- POST as non-idempotent — each call may create a new resource
- Safe retry of idempotent requests after network timeouts
- Idempotency keys as workaround for non-idempotent POST retries

**Answer**

An HTTP method is idempotent when repeating the same request multiple times produces the same side-effect state as executing it once. GET, PUT, DELETE, HEAD, and OPTIONS are idempotent. GET and HEAD are also safe — they do not change server state at all. PUT replacing a resource at `/api/items/5` with the same body twice converges to the same state. DELETE of an already-deleted resource may return 404, but the end state — resource absent — is the same. POST is not idempotent because each retry may create a new record, which is why clients safely retry idempotent requests after network timeouts but must use idempotency keys to avoid duplicates on POST retries.

---

## Q5. What is the difference between PUT and POST?

**Concepts**
- POST creating at a collection URI with server-assigned identifier
- PUT replacing or creating at a client-known URI
- POST non-idempotent; PUT idempotent on a specific resource path
- `201 Created` with `Location` header for POST; `200` or `204` for PUT update

**Answer**

POST creates a new resource at a collection URI and the server typically assigns the identifier — `POST /api/orders` returns `201 Created` with a `Location` header pointing to the new resource. PUT replaces or creates a resource at a known URI supplied by the client — `PUT /api/orders/42` targets a specific resource and repeated calls converge to the same state. I use POST when the client does not know the final resource URL before creation. PUT sends the full representation, so omitted fields may be cleared depending on server semantics. ASP.NET Core maps these with `[HttpPost]` and `[HttpPut("{id}")]` respectively.

---

## Q6. What is the difference between PUT and PATCH?

**Concepts**
- PUT replacing the entire resource representation
- PATCH applying a partial delta — only changed fields
- JSON Merge Patch (`application/merge-patch+json`) and JSON Patch formats
- PATCH reducing bandwidth and concurrency conflicts on single-field updates

**Answer**

PUT replaces the entire resource representation at a URI, so missing properties may be set to null or default — the client must send the complete resource. PATCH applies a partial update, changing only the fields included in the request, which avoids sending the full object when only a few properties change. JSON Merge Patch (`application/merge-patch+json`) and JSON Patch (`application/json-patch+json`) are common PATCH formats. PUT is idempotent; PATCH can be idempotent if the patch operation is defined that way, but complex patches may not be. I choose PATCH when updating single fields like `status` to reduce bandwidth and minimize concurrency conflicts from overwriting unchanged data.

---

## Q7. What does it mean for an HTTP method to be "safe"?

**Concepts**
- Safe method not modifying server state — retrieval only
- GET and HEAD as the safe methods
- Caches, prefetchers, and crawlers invoking safe methods freely
- Safe does not mean authorization-free
- State changes reserved for POST, PUT, PATCH, DELETE

**Answer**

A safe HTTP method does not modify server-side state — it only retrieves information. GET and HEAD are safe, which is why clients, caches, and crawlers may invoke them without causing side effects. Using GET for delete, charge, or submit operations violates safety and causes duplicate side effects on browser refresh or prefetch. Safe does not mean "harmless" — a GET may return sensitive data, so authorization still applies. REST-compliant APIs reserve GET exclusively for read operations and use POST, PUT, PATCH, or DELETE for anything that changes server state.

---

## Q8. When should an API return HTTP 201 Created vs 200 OK?

**Concepts**
- `201 Created` signaling resource birth with `Location` header
- `200 OK` for returning an existing resource or processing without creating a new URI
- `CreatedAtAction` / `CreatedAtRoute` / `Created` generating 201 in ASP.NET Core
- Returning `200` on create hiding canonical URI from HTTP client libraries

**Answer**

I return `201 Created` when a POST (or sometimes PUT) successfully creates a new resource — the response includes a `Location` header pointing to the new resource URI. `200 OK` is appropriate when returning an existing resource on GET, or processing a request that does not create a new URI. A PUT that updates an existing resource returns `200 OK` or `204 No Content`, not `201`. In ASP.NET Core I use `CreatedAtAction`, `CreatedAtRoute`, or `Created(uri, value)` to produce `201` responses. Returning `200` on create hides the canonical URI from standard HTTP client libraries and OpenAPI-generated SDKs that expect a `Location` header.

---

## Q9. When should an API return HTTP 204 No Content?

**Concepts**
- `204` signaling successful processing with no response body
- Common for DELETE and PUT/PATCH without echo-back
- `204` must not include a message body
- `200` with body when client expects the updated entity

**Answer**

I return `204 No Content` when the request succeeded but there is no response body to send. The common cases are successful DELETE, and PUT or PATCH where the client does not need the updated representation echoed back. `204` must not include a message body — only headers like `ETag` may accompany it. I return `200` with a body instead when the client expects the updated entity in the response. In ASP.NET Core, `return NoContent()` produces `204` and the framework enforces the empty body contract.

---

## Q10. What is the difference between HTTP 400 Bad Request and 404 Not Found?

**Concepts**
- `400` — request malformed or failed validation; server understood the target
- `404` — requested resource URI or identifier does not exist
- `[ApiController]` automatically returning `400` for model validation failures
- Returning `200` with error message breaking standard HTTP client handling

**Answer**

`400 Bad Request` means the request was malformed or failed validation — the server understood the target but rejected the input. Missing required fields, invalid JSON, or failed Data Annotations produce `400` with `ValidationProblemDetails`. `404 Not Found` means the requested resource URI or identifier does not exist on the server — `GET /api/orders/99999` when order 99999 is absent. `400` is about request quality; `404` is about resource existence. `[ApiController]` in ASP.NET Core automatically returns `400` for model validation failures. Returning `200` with an error message for either case breaks standard HTTP client error handling and monitoring.

---

## Q11. What is HATEOAS?

**Concepts**
- Hypermedia As The Engine Of Application State
- Responses including links describing available next actions
- Richardson Maturity Model level 3
- `LinkGenerator` and `CreatedAtAction` as link generation primitives
- Full HATEOAS optional in practice; OpenAPI documentation as common alternative

**Answer**

HATEOAS means API responses include links that describe available next actions on a resource, enabling clients to discover workflow transitions dynamically rather than hard-coding URL patterns. A response might include `_links: { "self": "...", "cancel": "..." }` pointing to related operations. It is the highest level of the Richardson Maturity Model for REST. Full HATEOAS adds contract complexity and versioning surface, so many production APIs rely on OpenAPI documentation instead. ASP.NET Core can generate links via `LinkGenerator`, `CreatedAtAction`, or custom hypermedia envelopes. Resource-oriented URLs and correct HTTP verbs are the baseline — HATEOAS is optional.

---

## Q12. What is RPC-style API design vs RESTful resource design?

**Concepts**
- RPC exposing operations as named endpoints (often all POST)
- REST modeling nouns as resources manipulated by HTTP verbs
- RPC producing non-standard contracts with poor cache behavior
- Blended designs for complex non-CRUD workflows

**Answer**

RPC-style APIs expose operations as named endpoints — `POST /api/CreateOrder`, `POST /api/CancelOrder` — while RESTful design models nouns as resources manipulated by HTTP verbs: `POST /api/orders`, `DELETE /api/orders/42`. REST aligns with HTTP caching, idempotency, and standard tooling since the verb and noun together describe the operation. RPC is simpler for complex transactions but produces non-standard contracts that gateways, monitoring, and client SDKs cannot interpret by HTTP semantics alone. Many APIs blend both approaches — REST for CRUD and RPC-style sub-resources for non-CRUD workflows like `/api/orders/42/payments`.

---

## Q13. What is the difference between REST and SOAP?

**Concepts**
- REST as architectural style using HTTP with JSON and human-readable URLs
- SOAP as protocol with strict XML envelopes and WSDL contracts
- WS-* standards in SOAP for transactions, security, and reliability
- REST scaling well with browsers, CDNs, and HTTP/2
- ASP.NET Core focusing on REST/JSON; SOAP requiring WCF or third-party middleware

**Answer**

REST is an architectural style using HTTP with lightweight JSON payloads and human-readable URLs. SOAP is a protocol with strict XML envelopes, WSDL contracts, and transport-level features — WS-Security, WS-AtomicTransaction, WS-ReliableMessaging — built into the specification. REST uses standard HTTP methods and status codes; SOAP typically posts XML to a single endpoint regardless of operation. SOAP tooling is heavier but strongly typed via WSDL and common in enterprise ESB integrations where those WS-* guarantees matter. REST scales well with browsers, CDNs, and HTTP/2 and is the dominant choice for modern public and mobile APIs. ASP.NET Core focuses on REST/JSON Web APIs; SOAP requires separate WCF or third-party middleware.

---

## Q14. Why should GET requests not perform state-changing operations?

**Concepts**
- GET defined as safe and idempotent — no side effects
- Browsers, proxies, crawlers, and prefetchers executing GET without user intent
- Caches potentially replaying cached destructive GETs
- Sensitive GETs appearing in logs, history, and referrer headers

**Answer**

GET is defined as safe and idempotent — browsers, proxies, crawlers, and prefetchers may execute GET requests without user intent, which causes unintended mutations when GET performs state changes. Refreshing a browser repeats the last GET — charging a payment via GET creates duplicate charges. GET URLs appear in server logs, browser history, and referrer headers, exposing sensitive operations to unintended observers. Caches may store GET responses, and a destructive GET cached incorrectly is catastrophic. The HTTP specification explicitly requires GET to have no side effects, and REST depends on this contract for caching correctness.

---

## Q15. What is a REST resource vs a REST collection?

**Concepts**
- Collection as group of resources at a plural noun URI
- Individual resource as single item at a specific URI with an identifier
- Collections supporting listing and creation
- Individual resources supporting read, update, and delete
- Nested collections expressing parent-child ownership

**Answer**

A collection is a group of resources exposed at a plural noun URI — `GET /api/orders` returns a list, `POST /api/orders` creates a new member. An individual resource is a single item within that collection at a specific URI — `GET /api/orders/42` returns order 42, `PUT /api/orders/42` replaces it, `DELETE /api/orders/42` removes it. Collections support listing and creation; individual resources support read, update, and delete. Nested collections use path hierarchy to express ownership: `/api/customers/3/orders` for orders belonging to customer 3.

---

## Q16. What is the purpose of the Location header on a 201 response?

**Concepts**
- Location header pointing to canonical URI of the newly created resource
- `CreatedAtAction` / `CreatedAtRoute` / `Created` generating Location in ASP.NET Core
- Standard HTTP client libraries following Location automatically
- Absolute or root-relative URI reachable by the client
- Omitting Location breaking REST client expectations

**Answer**

The `Location` header on a `201 Created` response tells the client the canonical URI of the newly created resource so they can fetch, update, or link to it without parsing the response body for an identifier. For example: `Location: https://api.example.com/api/orders/42` after creating order 42. Standard HTTP client libraries follow this URI automatically. In ASP.NET Core I generate it via `CreatedAtAction`, `CreatedAtRoute`, or `Created(uri, value)` rather than manual strings, since these helpers respect path base, lowercase URL options, and route refactors. Omitting `Location` on `201` breaks REST client expectations and mobile SDK conventions.

---

## Q17. What is API versioning and why is it needed?

**Concepts**
- Multiple incompatible contract generations coexisting
- Breaking changes forcing simultaneous consumer updates without versioning
- URL path, header, query string, and media type versioning strategies
- `Asp.Versioning.Mvc` with `[ApiVersion]` attributes
- Deprecation policies and sunset headers for retiring old versions

**Answer**

API versioning allows multiple incompatible contract generations to coexist so existing clients keep working while new clients adopt improved endpoints. It is needed because breaking changes — renamed fields, changed types, removed endpoints — would otherwise force all consumers to update simultaneously. Versioning strategies include URL path (`/api/v2/orders`), headers (`X-Api-Version`), query strings, or media type negotiation. In ASP.NET Core I use `Asp.Versioning.Mvc` with `[ApiVersion("2.0")]` attributes. Non-breaking additive changes like new optional fields often do not require a new version, and deprecation policies communicated via sunset headers give consumers a timeline for retiring old versions.

---

## Q18. What makes an endpoint "RESTful" vs merely "HTTP-based"?

**Concepts**
- Noun-based URIs identifying resources
- HTTP methods used according to defined semantics
- Appropriate status codes rather than embedding success/failure in body
- Stateless requests without server session state
- Cacheability and idempotency where the HTTP specification expects it

**Answer**

A RESTful endpoint models resources with noun-based URIs, uses HTTP methods according to their defined semantics, returns appropriate status codes, and stays stateless. An HTTP-based endpoint simply uses HTTP transport but may ignore method semantics, use verb-filled URLs, or wrap all responses in `200 OK`. RESTful: `DELETE /api/items/5` returns `204`; HTTP-based: `POST /api/items/delete/5` returns `200` with `{ "success": true }`. RESTful responses use standard status codes so gateways, monitors, and retry logic can act on the outcome class without parsing the body. Merely returning JSON over HTTP does not make an API RESTful — the method semantics, status code discipline, and statelessness are equally required.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

#### Gotcha 1. POST returning 200 instead of 201

**Concepts**
- `201 Created` with `Location` header as REST create contract
- `CreatedAtAction` / `CreatedAtRoute` / `Created` generating correct response
- `200` hiding new resource URL from HTTP client libraries

**Answer**

A successful resource creation via POST should return `201 Created` with a `Location` header pointing to the new resource URI — returning `200 OK` omits that contract and breaks REST clients that rely on status codes and the `Location` header. I use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to produce `201` with a route-generated `Location`. Returning `200` for create operations hides the new resource URL from standard HTTP client libraries and OpenAPI-generated SDKs.

---

#### Gotcha 2. GET that mutates state

**Concepts**
- GET defined as safe and idempotent
- Browsers, CDNs, and crawlers invoking GET without user intent
- Cached GET responses replaying destructive operations

**Answer**

GET must be safe and idempotent — performing deletes or updates on GET violates HTTP semantics, breaks caching proxies, and creates security holes when URLs are prefetched, logged, or opened in email clients. Browsers, CDNs, and link-preview crawlers may invoke GET URLs without user intent so side effects run unintentionally. I use POST, PUT, PATCH, or DELETE for state changes and keep GET read-only.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Concepts**
- Business failures mapping to 4xx/5xx status codes
- `ProblemDetails` / `ValidationProblemDetails` as standard error shapes
- HTTP status codes driving client retry logic, gateways, and APM alerting

**Answer**

Business failures must map to appropriate `4xx` or `5xx` status codes — a `200` response with an error flag forces every client to parse the body instead of using standard HTTP semantics. I return `ValidationProblemDetails` or `ProblemDetails` with `400` for validation failures and `404`, `409`, or `422` for domain errors. HTTP status codes drive client retry logic, API gateways, and APM alerting; a `200` masks failures in dashboards and causes incident blind spots.

---

#### Gotcha 4. Returning EF entities from API actions

**Concepts**
- Navigation properties triggering N+1 queries during serialization
- Circular references causing JSON serializer loops
- DTOs as stable public contract decoupled from schema migrations

**Answer**

EF Core entities expose navigation properties, shadow fields, and circular references that are not meant for public contracts. Lazy-loaded navigations trigger N+1 queries during serialization and can pull entire object graphs into the response. Circular references between entities cause `JsonException` at runtime. I return DTOs projected from EF queries, which decouples the API contract from schema migrations and exposes only the fields clients need.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Concepts**
- ASP.NET Core 8 defaulting to camelCase via `System.Text.Json`
- PascalCase client payloads binding as missing properties
- `PropertyNameCaseInsensitive` or `[JsonPropertyName]` for legacy client alignment

**Answer**

ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json` — PascalCase property names from some clients bind as missing properties, leaving model properties at default values and causing silent data loss on POST and PUT. I use `[JsonPropertyName("PropertyName")]` or a custom `PropertyNamingPolicy` to align server expectations with legacy client payloads, or enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)` when accepting mixed casing.

---

#### Gotcha 6. GET with `[FromBody]`

**Concepts**
- HTTP clients, proxies, and caches ignoring or stripping GET request bodies
- `[FromQuery]` for simple filters; POST to search endpoint for complex filter objects
- OpenAPI tools discouraging GET bodies

**Answer**

Many HTTP clients, proxies, and caches ignore or strip GET request bodies — filters sent as JSON in GET requests fail silently or never reach the action. Model binding for `[FromBody]` on GET is not reliably supported across the HTTP ecosystem. I use query strings with `[FromQuery]` for simple filters or POST to a dedicated search endpoint for complex filter objects.

---

#### Gotcha 7. CORS as server security

**Concepts**
- CORS enforced by browsers only — does not stop curl or server-to-server calls
- Authentication and authorization as actual security boundary
- `AddCors` and `UseCors` for browser SPA access only

**Answer**

CORS is enforced by browsers only — it does not stop curl, Postman, server-to-server calls, or direct API requests. CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they do not authenticate callers. A public API without auth remains fully accessible to any non-browser client regardless of CORS policy. I register `AddCors` and `UseCors` for browser SPA access, and enforce JWT, cookies, or API keys separately for actual security.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Concepts**
- Browsers rejecting `*` origin with credentialed requests
- `WithOrigins` and `AllowCredentials` required together
- Explicit origin listing including local dev and production domains

**Answer**

Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers — I must specify explicit origins with `WithOrigins` and call `AllowCredentials`. `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined; ASP.NET Core will not emit a valid CORS response for credentialed requests. I list every trusted frontend origin explicitly, including local dev URLs and production domains.

---

#### Gotcha 9. Swagger UI exposed in Production

**Concepts**
- Swagger UI disclosing full API surface and try-it-out access
- Environment check gating `MapSwagger` and `UseSwaggerUI`
- Exposed OpenAPI revealing endpoint names and enum values for reconnaissance

**Answer**

Public Swagger UI discloses the full API surface, schemas, and try-it-out access — I gate it behind authentication or disable it outside Development and Staging. `MapSwagger` and `UseSwaggerUI` in `Program.cs` should be wrapped in environment checks or authorization middleware. Exposed OpenAPI documents reveal internal endpoints, field names, and enum values useful for reconnaissance.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Concepts**
- `[ApiController]` enabling automatic `400` validation and binding source inference
- Mixed controllers producing inconsistent error contracts
- Assembly-level attribute for consistent application

**Answer**

Without `[ApiController]`, automatic `400 ValidationProblemDetails`, binding source inference, and attribute routing behaviors differ — mixed controllers in the same Web API produce inconsistent error contracts. Controllers missing the attribute may return `200` with invalid models or require manual `ModelState` checks. I apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Concepts**
- `.Result` / `.Wait()` blocking the thread pool thread while async I/O continues
- Thread-pool starvation under concurrent load
- Classic deadlock from synchronization context contention
- `async Task<IActionResult>` with `await` as the fix

**Answer**

Blocking on `.Result` or `.Wait()` inside async-capable request actions causes thread-pool starvation — each blocked thread holds a slot while I/O completes, so Kestrel cannot accept new requests under concurrent load. Classic deadlocks occur when the blocked thread holds the synchronization context the continuation needs to resume. I always mark controller actions `async Task<IActionResult>` and `await` all the way through the service layer.

---

#### Gotcha 12. Liveness probe includes SQL check

**Concepts**
- Liveness checking whether the process should be restarted
- Readiness removing pod from load balancer until dependencies recover
- SQL, Redis, and external service checks belonging on readiness only

**Answer**

If the liveness probe fails when SQL is down, Kubernetes restarts pods that cannot fix the dependency — a restarted app still cannot reach the same down SQL server. I put SQL, Redis, and external service checks on readiness only. Liveness answers whether the process should be killed and restarted; readiness answers whether it should receive traffic. I map `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck` or custom dependency tags.

---

#### Gotcha 13. N+1 queries in list endpoints

**Concepts**
- Lazy-loaded navigation properties triggering one SQL query per row
- `Select` projection to DTOs generating a single bounded query
- `Include`/`ThenInclude` for intentional eager loading

**Answer**

Returning entities with lazy-loaded navigation properties triggers one SQL query per row — serializing a list of `Order` entities with `Customer` navigation executes 1 + N queries under default lazy loading. I project directly to DTOs in LINQ so EF Core generates a single query with only the columns needed. For graphs that must be included, I use `Include`/`ThenInclude` or split queries deliberately rather than relying on lazy load during JSON output.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Concepts**
- Concurrent inserts and deletes shifting offset window between pages
- Keyset pagination using stable indexed key for consistent results
- Offset pagination acceptable for small static tables

**Answer**

Concurrent inserts and deletes between offset pages cause duplicate or skipped rows — `Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are added or removed. Keyset pagination uses `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key from the previous response, so the window is stable regardless of concurrent writes. Offset pagination remains acceptable for small, mostly static tables.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Concepts**
- Field resolvers querying the database per parent row
- DataLoader batching concurrent field resolutions into single round-trips
- Eager-loading or root-query projection as alternative

**Answer**

Field resolvers in HotChocolate or other GraphQL servers that query the database per parent row explode SQL under load — 100 authors each resolving `books` individually executes 101 queries. I register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips, or I eager-load at the root query when the client always requests nested fields together.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Concepts**
- Native gRPC using HTTP/2 binary framing not exposed to browser JavaScript
- gRPC-Web middleware translating between browser and native gRPC
- CORS configuration required alongside gRPC-Web for cross-origin browser calls

**Answer**

Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript — browser clients need gRPC-Web middleware plus CORS configuration. Standard `@grpc/grpc-js` in Node or .NET clients works server-to-server; Blazor WASM and SPA browsers require the gRPC-Web protocol. I add `AddGrpcWeb()` and `EnableGrpcWeb()` on mapped gRPC services to translate between gRPC-Web and native gRPC, and configure CORS for the browser origin.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this "RESTful" order API. QA reports duplicate charges when users refresh the browser after checkout. Which REST constraints are violated and what status codes should change?

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet("checkout")]
    public IActionResult Checkout([FromQuery] int customerId, [FromQuery] decimal amount)
    {
        var order = _orders.Charge(customerId, amount);
        return Ok(order);
    }
}
```

---

**Answer**

Charging money via `GET` violates the safe-method constraint — browsers, proxies, and prefetchers may repeat the request without user intent, causing duplicate charges on refresh. The verb `GET` carries a semantics promise that it does not modify server state, so anything that calls `Charge` must use a non-safe verb. The response `200 OK` is also wrong for resource creation — a new order was created, so the response should be `201 Created` with a `Location` header pointing to the new order. Putting sensitive financial data in query string parameters exposes it in server logs, browser history, and referrer headers. The fix is to replace the action with `[HttpPost]` accepting a request body with customer and amount, returning `CreatedAtAction` for `201`. An idempotency key header allows safe retry of the POST without duplicate charges if the payment gateway supports it.

---

#### Q2. (R) A legacy integration team exposes this controller and claims it is REST. Identify RPC-in-REST smells and how you would reshape routes and verbs without breaking existing clients immediately.

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpPost("CreateOrder")]
    public IActionResult CreateOrder([FromBody] CreateOrderDto dto) => Ok(_svc.Create(dto));

    [HttpPost("CancelOrder")]
    public IActionResult CancelOrder([FromBody] CancelOrderDto dto) => Ok(_svc.Cancel(dto.Id));

    [HttpPost("GetOrderById")]
    public IActionResult GetOrderById([FromBody] int id) => Ok(_svc.Get(id));
}
```

---

**Answer**

Every operation is a POST to a verb-named sub-path — that is remote procedure call over HTTP, not resource-oriented REST. `GetOrderById` uses POST with a body for a read operation, which breaks caching, CDN rules, and HTTP idempotency expectations. `CreateOrder` and `CancelOrder` embed the operation in the URL, duplicating what the HTTP verb should express. The canonical REST shapes are `POST /api/orders` for creation, `GET /api/orders/{id}` for retrieval, and `DELETE /api/orders/{id}` or `PATCH /api/orders/{id}` with a status change for cancellation. To avoid breaking existing clients immediately, I keep the old verb-in-path routes as thin wrappers calling the same service layer — marked as deprecated in OpenAPI with a sunset date — while the new canonical routes go live. Integration tests cover both surfaces during the overlap period so regressions are caught before the deprecated routes are retired.

---

#### Q3. (R) Review error handling in this inventory API. Clients cannot distinguish "bad request" from "not found," and failed updates sometimes return HTTP 200 with `{ "success": false }`.

```csharp
[HttpPut("{id:int}")]
public IActionResult Update(int id, [FromBody] UpdateItemDto dto)
{
    if (dto.Quantity < 0)
        return Ok(new { success = false, message = "Invalid quantity" });

    var item = _repo.Find(id);
    if (item is null)
        return Ok(new { success = false, message = "Not found" });

    _repo.Update(id, dto);
    return Ok(new { success = true });
}
```

---

**Answer**

Wrapping failures in `200 OK` with a boolean flag breaks REST clients, monitors, and retry logic — load balancers and APM treat every `200` as success so real failures become invisible. A negative quantity is a `400 Bad Request` validation failure; a missing resource is `404 Not Found`. The ad-hoc `{ success, message }` envelope is inconsistent with `ProblemDetails` RFC 7807 and OpenAPI tooling that clients depend on. The fix is `return BadRequest(new ValidationProblemDetails(...))` or `return ValidationProblem(ModelState)` for invalid quantity, `return NotFound()` when `Find` returns null, and `return NoContent()` or `return Ok(updatedDto)` on success — not a success flag wrapper.

---

#### Q4. (M) A mobile client retries `PUT /api/customers/42` after a timeout. The first request actually succeeded but the client never received the response. Explain why PUT is the right verb here and what idempotency guarantees the server should document.

---

**Answer**

PUT to a known URI is defined as idempotent — repeating the same full representation should converge the resource to the same state, so a retry after a network timeout should not create a second customer or duplicate side effects. This is precisely the scenario PUT was designed for: the client knows the resource identifier, the body is the complete replacement, and repeating the same payload produces the same final state. The server should document that `PUT /api/customers/42` replaces the resource at that ID, and that repeated identical payloads yield the same result. If the action increments a `Revision` field on every PUT regardless of body equality, that behavior should be documented — true idempotency may require comparing the incoming payload against the current state. The response on a successful retry may return `200 OK` or `204 No Content` rather than `201`; both are valid for an update. I also recommend including an `ETag` in the response and supporting `If-Match` on the retry so the client can detect a lost update when the resource changed between attempts.

---

#### Q5. (R) Review this search endpoint from a security audit. What REST and HTTP semantics are wrong, and what breaks under caching proxies?

```csharp
[HttpGet("delete")]
public IActionResult Delete([FromQuery] int id)
{
    _repo.Delete(id);
    return NoContent();
}

[HttpGet("search")]
public IActionResult Search([FromQuery] string q, [FromQuery] string apiKey)
{
    return Ok(_repo.Search(q, apiKey));
}
```

---

**Answer**

`GET /delete` performs destructive work with a safe verb — crawlers, link-preview tools, CDNs, and browser prefetchers may trigger deletion without user intent. Shared caches may store the response and replay the deletion path or serve a stale `204` incorrectly. The `apiKey` in the query string leaks the credential into server logs, browser history, analytics pipelines, and referrer headers with every search request. The fixes are: change the delete endpoint to `[HttpDelete("{id:int}")]` so it uses the correct unsafe idempotent verb; move the API key to a header (`Authorization` or `X-Api-Key`) so it travels outside the URL; and set `Cache-Control: no-store` on sensitive GET responses that must not be cached.

---

#### Q6. (D) Product asks for full HATEOAS on every list response (`_links.self`, `_links.next`, etc.). When is hypermedia worth the contract cost in a B2B JSON API, and when is a stable OpenAPI document plus explicit pagination query params enough?

---

**Answer**

HATEOAS pays off when clients must discover available actions dynamically — long-lived public APIs with many heterogeneous consumers releasing independently, or when legal or compliance requirements demand server-controlled "allowed next steps" rather than client-hard-coded workflows. For most internal B2B integrations where all consumers are known, releases are coordinated, and clients are code-generated from an OpenAPI document, full HATEOAS adds contract complexity without proportional benefit. A pragmatic middle ground is including `Link` headers or minimal `next`/`prev` URLs for pagination without full HAL or JSON-LD on every entity — this solves the real pagination stability problem without the full HATEOAS surface. Full hypermedia complicates caching — every new link relation becomes a potential breaking-change surface — and testing fixtures must be updated whenever link shapes change. Richardson maturity level 3 is aspirational; resource-oriented URLs with correct verbs and status codes are the production baseline.

---

#### Q7. (P) You are introducing `/api/v2/customers` while v1 stays live for six months. Compare URL path versioning, `X-Api-Version` header, and `Accept: application/vnd.company.customers.v2+json` — which fits gateway routing, mobile apps, and breaking DTO changes?

---

**Answer**

URL path versioning — `/api/v2/customers` — is easiest for gateways, logs, and mobile hard-coded base paths since route rules in nginx or YARP are trivial and Swagger can expose `/v1` and `/v2` groups as separate documents. Breaking DTO renames are obvious in path-versioned contracts because the version is visible in every URL. Header versioning with `X-Api-Version` keeps URLs identical for all versions, which is clean for additive changes, but proxies may strip unknown headers and CDNs typically key caches only on URL, so cached v1 responses can be returned for v2 requests. Media type versioning via `Accept: application/vnd.company.customers.v2+json` aligns with strict REST content-negotiation principles but is cumbersome for mobile teams and Postman testing, and most API gateways cannot route on `Accept` headers as easily as on path segments. For breaking DTO changes I always introduce a new version — never silently change a v1 field type. I use `Asp.Versioning.Mvc` with `[ApiVersion("2.0")]` attributes and dual-map controllers or produce versioned Swagger docs. The organization-wide choice matters more than which strategy is theoretically "most RESTful."

---

#### Q8. (R) Review this mixed-style API surface from a fintech partner integration. Prioritize the highest-risk contract issues for production.

```csharp
[HttpPost]
[Route("api/transfers/sendMoney")]          // verb in path
public IActionResult SendMoney([FromBody] TransferDto dto) => Ok(_transfers.Send(dto));

[HttpGet("api/transfers/{id}/status")]      // duplicate api prefix on action
public IActionResult Status(int id) => Ok(_transfers.Status(id));

[HttpDelete("transfers")]                   // missing leading api segment; body on DELETE
public IActionResult Cancel([FromBody] CancelDto dto) => Ok();
```

`TransfersController` has `[Route("api/[controller]")]` at class level.

---

**Answer**

The highest-risk issue is that action-level absolute route templates override the class-level `[Route("api/[controller]")]`, so the actual URLs are unpredictable. `[Route("api/transfers/sendMoney")]` on an action ignores the class prefix and registers as `/api/transfers/sendMoney` directly, but `[HttpDelete("transfers")]` without a leading slash appends to the class prefix, resolving to `/api/Transfers/transfers` — a path that diverges from what partner documentation likely shows. This means some endpoints are reachable while others return 404 in ways that depend on routing resolution order. The verb-in-path `sendMoney` is an RPC smell but is a lower-risk issue than the broken routing. DELETE with a body is poorly supported by some clients and intermediaries — I replace it with `DELETE /api/transfers/{id}` where the ID is in the path, or `POST /api/transfers/{id}/cancellations` with an idempotency key. The fix is to normalize all action templates to relative paths on a consistent class prefix, regenerate OpenAPI, and run contract tests against the partner sandbox before going live.

---
