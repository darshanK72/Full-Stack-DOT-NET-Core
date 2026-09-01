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

What is REST?

**Answer:** REST (Representational State Transfer) is an architectural style for distributed hypermedia systems that uses standard HTTP methods, stateless request/response exchanges, and resource-oriented URLs. It treats server-side data as resources addressable by URIs and manipulated through a uniform interface.

- Resources are identified by URLs such as `/api/orders/42`, not by action names in the path.
- Each request carries enough context (auth, representation format) because the server holds no client session state between calls.
- HTTP verbs express intent: GET reads, POST creates, PUT replaces, PATCH partial-updates, DELETE removes.
- Representations (JSON, XML) transfer resource state — the same resource can have multiple formats.
- ASP.NET Core 8 Web API implements REST principles through attribute-routed controllers, standard status codes, and content negotiation.

---

## Q2. What is a Web API?

What is a Web API?

**Answer:** A Web API is an HTTP-based service that exposes application functionality to clients over the network, typically returning structured data (JSON) rather than HTML pages. In ASP.NET Core 8, Web APIs are built with controller or minimal API endpoints mapped to HTTP routes.

- Clients include browsers, mobile apps, partner systems, and microservices calling over HTTPS.
- APIs use HTTP methods, status codes, and headers as the contract instead of UI-specific responses.
- ASP.NET Core Web API projects use `AddControllers()`, `[ApiController]`, and `MapControllers()` in the middleware pipeline.
- Authentication, validation, and serialization are handled by the framework pipeline before and after action execution.
- Web APIs differ from MVC views in that they return machine-readable payloads, not Razor-rendered HTML.

---

## Q3. What are the main REST architectural constraints?

What are the main REST architectural constraints?

**Answer:** REST defines six constraints: client-server separation, statelessness, cacheability, uniform interface, layered system, and optionally code-on-demand. Together they promote scalability, independent evolution of clients and servers, and predictable HTTP semantics.

- **Client-server:** UI concerns stay on the client; the server exposes data and operations through an API boundary.
- **Stateless:** Each request is self-contained — no server-side session state tied to a specific client connection.
- **Cacheable:** Responses declare cacheability so intermediaries can reduce load when appropriate.
- **Uniform interface:** Standard HTTP methods, resource identification via URI, and self-descriptive messages simplify integration.
- **Layered system:** Clients cannot tell whether they talk to the origin server, a gateway, or a cache — proxies and load balancers are transparent.
- **Code-on-demand (optional):** Server can extend client behavior by sending executable code — rarely used in JSON APIs.

---

## Q4. What is HTTP idempotency, and which methods are idempotent?

What is HTTP idempotency, and which methods are idempotent?

**Answer:** An HTTP method is idempotent when repeating the same request multiple times produces the same side-effect state as executing it once. GET, PUT, DELETE, HEAD, and OPTIONS are idempotent; POST is not, because each call may create a new resource.

- **GET/HEAD/OPTIONS:** Safe reads — repeated calls do not change server state.
- **PUT:** Replacing `/api/items/5` with the same body twice converges to the same resource state.
- **DELETE:** Deleting an already-deleted resource may return 404, but the end state (resource absent) is the same.
- **POST:** Each retry can create duplicate records unless the server implements idempotency keys.
- Clients safely retry idempotent requests after network timeouts without fear of multiplying side effects.

---

## Q5. What is the difference between PUT and POST?

What is the difference between PUT and POST?

**Answer:** POST creates a new resource at a collection URI and the server typically assigns the identifier; PUT replaces or creates a resource at a known URI supplied by the client. POST is not idempotent; PUT is idempotent on a specific resource path.

- `POST /api/orders` — server generates `id`, returns `201 Created` with a Location header.
- `PUT /api/orders/42` — client targets a known URI; repeated calls replace the same resource.
- POST is used when the client does not know the final resource URL before creation.
- PUT sends the full representation — omitted fields may be cleared depending on server semantics.
- ASP.NET Core actions map these with `[HttpPost]` and `[HttpPut("{id}")]` respectively.

---

## Q6. What is the difference between PUT and PATCH?

What is the difference between PUT and PATCH?

**Answer:** PUT replaces the entire resource representation at a URI; PATCH applies a partial update, changing only the fields included in the request. Both target a known resource URI, but PATCH avoids sending the full object when only a few properties change.

- PUT requires the client to send the complete resource — missing properties may be set to null or default.
- PATCH sends a delta — JSON Merge Patch (`application/merge-patch+json`) or JSON Patch (`application/json-patch+json`) are common formats.
- PUT is idempotent; PATCH can be idempotent if the patch operation is defined that way, but complex patches may not be.
- ASP.NET Core 8 supports PATCH actions with DTOs or dedicated patch document types bound from the body.
- Choosing PATCH reduces bandwidth and concurrency conflicts when updating single fields like `status`.

---

## Q7. What does it mean for an HTTP method to be "safe"?

What does it mean for an HTTP method to be "safe"?

**Answer:** A safe HTTP method does not modify server-side state — it only retrieves information. GET and HEAD are safe methods; clients, caches, and crawlers may invoke them without causing side effects.

- Safe methods can be prefetched, cached, and logged without risk of unintended mutations.
- Using GET for delete, charge, or submit operations violates safety and causes duplicate actions on refresh.
- Safe does not mean "harmless" — a GET may return sensitive data, so authorization still applies.
- POST, PUT, PATCH, and DELETE are unsafe because they change server state.
- REST-compliant APIs reserve GET exclusively for read operations.

---

## Q8. When should an API return HTTP 201 Created vs 200 OK?

When should an API return HTTP 201 Created vs 200 OK?

**Answer:** Return 201 Created when a POST (or sometimes PUT) successfully creates a new resource; return 200 OK when returning an existing resource or processing a request that does not create a new URI. 201 signals resource birth and typically includes a Location header.

- POST that inserts a record → `201 Created` with the new resource URI in the Location header.
- GET that returns data → `200 OK` with the representation in the body.
- PUT that updates an existing resource → `200 OK` or `204 No Content`, not 201.
- ASP.NET Core uses `CreatedAtAction`, `CreatedAtRoute`, or `Created(uri, value)` to produce 201 responses.
- Returning 200 on create hides the canonical URI from standard HTTP client libraries.

---

## Q9. When should an API return HTTP 204 No Content?

When should an API return HTTP 204 No Content?

**Answer:** Return 204 No Content when the request succeeded but there is no response body to send. Common cases include successful DELETE, PUT, or PATCH where the client does not need the updated representation.

- DELETE that removes a resource → `204 NoContent()` in ASP.NET Core.
- PUT/PATCH update where the client already has the data and does not need echo-back.
- 204 must not include a message body — only headers (e.g., ETag) may accompany it.
- Contrasts with 200 OK which includes a body with the resource or confirmation payload.
- Use 200 with a body when the client expects the updated entity in the response.

---

## Q10. What is the difference between HTTP 400 Bad Request and 404 Not Found?

What is the difference between HTTP 400 Bad Request and 404 Not Found?

**Answer:** 400 Bad Request means the request was malformed or failed validation — the server understood the target but rejected the input. 404 Not Found means the requested resource URI or identifier does not exist on the server.

- Missing required fields, invalid JSON, or failed Data Annotations → 400 with `ValidationProblemDetails`.
- `GET /api/orders/99999` when order 99999 does not exist → 404.
- 400 is about request quality; 404 is about resource existence at the given address.
- `[ApiController]` in ASP.NET Core 8 automatically returns 400 for model validation failures.
- Returning 200 with an error message for either case breaks standard client error handling.

---

## Q11. What is HATEOAS?

What is HATEOAS?

**Answer:** HATEOAS (Hypermedia As The Engine Of Application State) means API responses include links that describe available next actions on a resource, enabling clients to discover transitions dynamically. It is the highest level of the Richardson Maturity Model for REST.

- A response might include `_links: { "self": "...", "cancel": "..." }` pointing to related operations.
- Clients follow links instead of hard-coding URL patterns for every workflow step.
- Full HATEOAS adds contract complexity — many production APIs rely on OpenAPI documentation instead.
- ASP.NET Core can generate links via `LinkGenerator`, `CreatedAtAction`, or custom hypermedia envelopes.
- HATEOAS is optional in practice; resource-oriented URLs and correct HTTP verbs are the baseline.

---

## Q12. What is RPC-style API design vs RESTful resource design?

What is RPC-style API design vs RESTful resource design?

**Answer:** RPC-style APIs expose operations as named endpoints (often POST to `/api/CreateOrder`), while RESTful design models nouns as resources manipulated by HTTP verbs on standard URIs (`POST /api/orders`). REST aligns with HTTP caching, idempotency, and standard tooling.

- RPC: `POST /api/orders/CancelOrder` — verb in URL, procedure-oriented.
- REST: `DELETE /api/orders/42` or `PATCH /api/orders/42` with `{ "status": "cancelled" }`.
- RPC is simpler for complex transactions but produces non-standard contracts and poor cache behavior.
- REST resources have stable identifiers and predictable CRUD mappings.
- Many APIs blend both — REST for CRUD, RPC-style sub-resources for non-CRUD workflows like `/api/orders/42/payments`.

---

## Q13. What is the difference between REST and SOAP?

What is the difference between REST and SOAP?

**Answer:** REST is an architectural style using HTTP with lightweight formats like JSON and human-readable URLs; SOAP is a protocol with strict XML envelopes, WSDL contracts, and transport-level features built into the specification. REST is the dominant choice for modern public and mobile APIs.

- REST uses standard HTTP methods and status codes; SOAP typically posts XML to a single endpoint.
- REST payloads are lean JSON; SOAP messages include envelope headers, body, and fault elements in XML.
- SOAP has built-in WS-* standards for transactions, security, and reliability — heavier but common in enterprise ESB integrations.
- REST scales well with browsers, CDNs, and HTTP/2; SOAP tooling is heavier but strongly typed via WSDL.
- ASP.NET Core 8 focuses on REST/JSON Web APIs; SOAP requires separate WCF or third-party middleware.

---

## Q14. Why should GET requests not perform state-changing operations?

Why should GET requests not perform state-changing operations?

**Answer:** GET is defined as a safe, idempotent read — browsers, proxies, crawlers, and prefetchers may execute GET requests without user intent, causing unintended mutations. State changes belong on POST, PUT, PATCH, or DELETE.

- Refreshing a browser repeats the last GET — charging a payment via GET creates duplicate charges.
- GET URLs appear in server logs, browser history, and referrer headers — exposing sensitive operations.
- Caches may store GET responses — a destructive GET cached incorrectly is catastrophic.
- Search engines crawling GET delete links can remove data unintentionally.
- REST and HTTP specifications explicitly require GET to have no side effects.

---

## Q15. What is a REST resource vs a REST collection?

What is a REST resource vs a REST collection?

**Answer:** A collection is a group of resources exposed at a plural noun URI (e.g., `/api/orders`), while an individual resource is a single item within that collection at a specific URI (e.g., `/api/orders/42`). Collections support listing and creation; individual resources support read, update, and delete.

- `GET /api/orders` — returns a list (collection) of order representations.
- `POST /api/orders` — creates a new member in the collection.
- `GET /api/orders/42` — returns a single resource identified by `42`.
- `PUT /api/orders/42` — replaces that specific resource.
- Nested collections use path hierarchy: `/api/customers/3/orders` for orders belonging to customer 3.

---

## Q16. What is the purpose of the Location header on a 201 response?

What is the purpose of the Location header on a 201 response?

**Answer:** The Location header on a 201 Created response tells the client the canonical URI of the newly created resource. Clients use it to fetch, update, or link to the resource without parsing the response body for an identifier.

- Example: `Location: https://api.example.com/api/orders/42` after creating order 42.
- Enables standard HTTP client libraries to follow the URI automatically.
- ASP.NET Core generates it via `CreatedAtAction`, `CreatedAtRoute`, or `Created(uri, value)`.
- The header value should be an absolute or root-relative URI reachable by the client.
- Omitting Location on 201 breaks REST client expectations and mobile SDK conventions.

---

## Q17. What is API versioning and why is it needed?

What is API versioning and why is it needed?

**Answer:** API versioning allows multiple incompatible contract generations to coexist so existing clients keep working while new clients adopt improved endpoints. It is needed because breaking changes to URLs, fields, or behavior would otherwise force all consumers to update simultaneously.

- Breaking changes include renaming fields, changing types, or removing endpoints.
- Versioning strategies include URL path (`/api/v2/orders`), headers (`X-Api-Version`), query strings, or media type negotiation.
- ASP.NET Core uses packages like `Asp.Versioning.Mvc` with `[ApiVersion("2.0")]` attributes.
- Non-breaking additive changes (new optional fields) often do not require a new version.
- Deprecation policies and sunset headers communicate timeline for retiring old versions.

---

## Q18. What makes an endpoint "RESTful" vs merely "HTTP-based"?

What makes an endpoint "RESTful" vs merely "HTTP-based"?

**Answer:** A RESTful endpoint models resources with noun-based URIs, uses HTTP methods according to their defined semantics, returns appropriate status codes, and stays stateless. An HTTP-based endpoint simply uses HTTP transport but may ignore method semantics, use verb-filled URLs, or wrap all responses in 200 OK.

- RESTful: `DELETE /api/items/5` returns 204; HTTP-based: `POST /api/items/delete/5` returns 200 with `{ "success": true }`.
- RESTful responses use standard status codes; HTTP-based APIs embed success/failure flags in the body.
- RESTful APIs are cacheable and idempotent where the HTTP specification expects it.
- Merely returning JSON over HTTP does not make an API RESTful.
- ASP.NET Core 8 provides conventions (`[ApiController]`, attribute routing) that steer toward RESTful design.

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

**Answer:**

**Answer:** Charging money via `GET` violates safe-method semantics and REST's uniform interface — browsers, proxies, and prefetchers may repeat the request, causing duplicate side effects. Checkout must be a non-safe verb (`POST`) with `201 Created` or `303 See Other`, never `GET` with `200 OK`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| REST / HTTP | `GET` performs state change (`Charge`) | Not safe; retries and refreshes re-execute payment |
| REST | Uses query string for mutation | Violates idempotent read expectations; logged in access logs |
| API contract | Returns `200 OK` for resource creation | Clients cannot distinguish create vs read; no `Location` header |
| Security | Sensitive `amount` in URL | Leaks PII/financial data via logs, referrer headers, browser history |

**Fix (priority order):**

1. Replace with `[HttpPost]` (or `POST /api/orders`) that creates an order — return `CreatedAtAction` with `201`.
2. Accept payment details in the request body, not query string.
3. Add idempotency key header (`Idempotency-Key`) for payment retries if the gateway supports it.

**Production takeaway:** Karat uses "GET checkout" to test whether you connect **HTTP method safety** to real money bugs — not whether you can recite REST acronym expansions.

---

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

**Answer:**

**Answer:** Every operation is a POST to a verb-named sub-path with RPC-style names — that is remote procedure call over HTTP, not resource-oriented REST. Migrate toward noun-based routes and correct HTTP verbs while keeping deprecated RPC routes behind a version or `[Obsolete]` shim during transition.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Verb in URL (`CreateOrder`, `CancelOrder`) | Non-standard contract; hard to cache and document in OpenAPI |
| HTTP | `GetOrderById` uses POST with body | Breaks caching, CDN rules, and HTTP semantics |
| REST | No stable resource identifiers in path | Clients cannot link to `/api/orders/{id}` |
| Maintainability | One POST pattern for all operations | Gateway auth, rate limits, and monitoring cannot differ by operation type |

**Fix (priority order):**

1. Introduce canonical routes: `POST /api/orders`, `GET /api/orders/{id}`, `DELETE /api/orders/{id}` (or PATCH for cancel state).
2. Keep old routes as thin wrappers calling the same service layer — mark deprecated in OpenAPI.
3. Publish migration guide with sunset date; add integration tests for both surfaces during overlap.

**Production takeaway:** Real migrations rarely flip every client overnight — show **resource modeling plus backward-compatible deprecation**, not purity lectures.

---

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

**Answer:**

**Answer:** Wrapping failures in HTTP 200 with a boolean flag breaks REST clients, monitors, and retry logic — HTTP status codes exist precisely to signal outcome class. Return `400 Bad Request` for validation failures and `404 Not Found` when the resource id does not exist.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | Validation failure returns `200 OK` | Load balancers and APM treat request as success |
| HTTP semantics | Missing resource returns `200` with message | Clients must parse body; caches may store "errors" |
| API design | Ad-hoc `{ success, message }` envelope | Inconsistent with `ProblemDetails` and OpenAPI tooling |
| Observability | Alerts on 5xx/4xx rates miss business failures | Incidents hide in 200 traffic |

**Fix (priority order):**

1. Return `BadRequest(ProblemDetails)` or `ValidationProblem(ModelState)` for invalid quantity.
2. Return `NotFound()` when `Find` returns null.
3. Return `204 NoContent` or `200` with updated DTO on success — not a success flag wrapper.

**Production takeaway:** **Status code discipline** is part of REST maturity — envelope JSON does not replace HTTP semantics in production APIs.

---

---

#### Q4. (M) A mobile client retries `PUT /api/customers/42` after a timeout. The first request actually succeeded but the client never received the response. Explain why PUT is the right verb here and what idempotency guarantees the server should document.

---

**Answer:**

**Answer:** PUT to a known URI is defined as idempotent — repeating the same full representation should converge the resource to the same state, so a retry after a network timeout should not create a second customer or duplicate side effects. Document that PUT replaces the resource at `{id}` and that repeated identical payloads yield the same final state (same etag/version).

- **Idempotency:** Multiple identical PUTs should not multiply records or increment counters unintentionally — server applies replace semantics on `/customers/42`.
- **Contrast with POST:** `POST /api/customers` creates a new resource each retry unless you add an idempotency key — wrong verb for "update known id."
- **Response on retry:** Second PUT may return `200 OK` or `204 NoContent` instead of `201` — both are valid; include `ETag` for concurrency control.
- **Not automatic:** If your handler increments `Revision` on every PUT regardless of body equality, document that behavior — true idempotency may require comparing payload hash.
- **Client guidance:** Safe to retry PUT on timeout; use `If-Match` etag to avoid lost updates when payload changed between attempts.

**Production takeaway:** Idempotency is a **contract promise** tied to verb + server behavior — Karat wants scenario reasoning, not "PUT is idempotent" in isolation.

---

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

**Answer:**

**Answer:** `GET /delete` performs destructive work with a safe verb, and putting secrets in query strings violates transport and caching expectations. Both endpoints will misbehave behind shared caches and violate REST safe-method rules.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| REST / HTTP | Delete via GET | Crawlers, prefetch, CDN cache may trigger deletion |
| Security | `apiKey` in query string | Keys appear in logs, analytics, browser history |
| Caching | GET responses may be cached | Search results stale; destructive GET catastrophic if cached path wrong |
| API design | Action name in path for mutation | RPC pattern; not addressable as `/api/items/{id}` |

**Fix (priority order):**

1. Change delete to `[HttpDelete("{id:int}")]` or POST to a sub-resource only if DELETE is blocked (document exception).
2. Move API key to header (`Authorization` or `X-Api-Key`).
3. Set `Cache-Control: no-store` on sensitive GETs; never mutate on GET.

**Production takeaway:** REST constraint violations become **security and cache incidents** in production — not style guide nitpicks.

---

---

#### Q6. (D) Product asks for full HATEOAS on every list response (`_links.self`, `_links.next`, etc.). When is hypermedia worth the contract cost in a B2B JSON API, and when is a stable OpenAPI document plus explicit pagination query params enough?

---

**Answer:**

**Answer:** HATEOAS pays off when clients must discover available actions dynamically (long-lived public APIs, evolving workflows) or when you want server-driven state transitions; for most internal B2B integrations with code-generated clients, OpenAPI plus stable pagination parameters is simpler and equally production-ready.

- **Favor HATEOAS** when many heterogeneous clients consume the same API without coordinated releases, or when legal/compliance requires server-controlled "allowed next steps."
- **Skip full HATEOAS** when all consumers are your mobile/web apps with simultaneous deploys — links duplicate what OpenAPI already documents.
- **Pragmatic middle:** Include `Link` header or minimal `next`/`prev` URLs for pagination without full HAL/JSON-LD on every entity.
- **Cost:** Hypermedia complicates caching, testing fixtures, and versioning — every new link relation becomes a breaking-change surface.
- **Karat angle:** Saying "HATEOAS is optional in REST" is correct — **Richardson maturity level 3** is aspirational, not a gate for shipping.

**Production takeaway:** Judgment beats ideology — choose hypermedia when **discovery reduces coupling**, not because a checklist says level 3.

---

---

#### Q7. (P) You are introducing `/api/v2/customers` while v1 stays live for six months. Compare URL path versioning, `X-Api-Version` header, and `Accept: application/vnd.company.customers.v2+json` — which fits gateway routing, mobile apps, and breaking DTO changes?

---

**Answer:**

**Answer:** URL path versioning (`/api/v2/customers`) is easiest for gateways, logs, and mobile hard-coded base paths; header or media-type versioning keeps URLs clean but complicates caching, browser testing, and CDN rules — pick one org-wide and enforce via ASP.NET Core API versioning middleware.

- **URL path:** Route rules in nginx/YARP are trivial; Swagger can expose `/v1` and `/v2` groups; breaking renames are obvious to partners.
- **Header (`X-Api-Version`):** Same URL for all versions — good for additive changes; bad when proxies strip unknown headers or caches key only on URL.
- **Media type (Accept vendor MIME):** Fine-grained content negotiation; heavy for mobile teams and Postman users; aligns with strict REST purists.
- **Breaking DTO changes:** Prefer new version with mapped adapters — never silently change v1 field types.
- **ASP.NET Core:** Use `Asp.Versioning.Mvc` with `[ApiVersion("2.0")]` and dual-map controllers or versioned Swagger docs.

**Production takeaway:** Versioning is an **ops + client lifecycle** decision — preview chapter sets up that you will implement, not just name three strategies.

---

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

**Answer:**

**Answer:** Route template stacking is inconsistent (double `api` prefix, missing segments), DELETE-with-body is poorly supported by clients and proxies, and verb-in-path RPC remains — the worst immediate risk is unreachable or ambiguous routes after `[controller]` expansion combined with absolute paths on actions.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | `[Route("api/transfers/sendMoney")]` on action ignores class `[Route("api/[controller]")]` convention | Duplicate or unexpected URLs; OpenAPI shows wrong paths |
| Routing | `[HttpDelete("transfers")]` may resolve to `/api/Transfers/transfers` | Partner docs diverge from actual matcher |
| HTTP | DELETE with body | Some clients omit body; intermediaries may strip it |
| Design | `sendMoney` verb in path | Non-REST; cannot apply standard idempotency keys on resource |
| Consistency | Mixed absolute and relative route attributes | Link generation and `CreatedAtAction` break |

**Fix (priority order):**

1. Normalize class route: `[Route("api/[controller]")]`; actions use `[HttpPost]`, `[HttpGet("{id}/status")]`, `[HttpDelete("{id}")]` only.
2. Replace DELETE body with `DELETE /api/transfers/{id}` or POST `/api/transfers/{id}/cancellations` with idempotency key.
3. Regenerate OpenAPI and contract tests against partner sandbox before go-live.

**Production takeaway:** **Route hygiene** is REST/Web API basics — messy templates cause production outages before anyone debates HATEOAS.

---
