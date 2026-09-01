# API Controllers & Action Results — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 02. API Controllers & Action Results](#chapter-02-api-controllers-action-results)
  - [Q1. What is an API controller in ASP.NET Core?](#chapter-02-api-controllers-action-results-q1)
  - [Q2. What does the `[ApiController]` attribute do?](#chapter-02-api-controllers-action-results-q2)
  - [Q3. What is the difference between `ControllerBase` and `Control…](#chapter-02-api-controllers-action-results-q3)
  - [Q4. What is `IActionResult`?](#chapter-02-api-controllers-action-results-q4)
  - [Q5. What is `ActionResult<T>` and how does it differ from `IActi…](#chapter-02-api-controllers-action-results-q5)
  - [Q6. What is `CreatedAtAction` and when do you use it?](#chapter-02-api-controllers-action-results-q6)
  - [Q7. What is the difference between `CreatedAtAction` and `Create…](#chapter-02-api-controllers-action-results-q7)
  - [Q8. What does the `Ok()` helper return?](#chapter-02-api-controllers-action-results-q8)
  - [Q9. What does `NoContent()` return and when is it appropriate?](#chapter-02-api-controllers-action-results-q9)
  - [Q10. What is the difference between `NotFound()` and `BadRequest(…](#chapter-02-api-controllers-action-results-q10)
  - [Q11. What HTTP status does `Conflict()` map to?](#chapter-02-api-controllers-action-results-q11)
  - [Q12. Why should API actions return DTOs instead of EF entities?](#chapter-02-api-controllers-action-results-q12)
  - [Q13. What is the Location header used for in API responses?](#chapter-02-api-controllers-action-results-q13)
  - [Q14. What is the difference between synchronous and asynchronous …](#chapter-02-api-controllers-action-results-q14)
  - [Q15. What problem does blocking on `.Result` cause in API control…](#chapter-02-api-controllers-action-results-q15)
  - [Q16. What is the difference between returning `Ok(entity)` and `C…](#chapter-02-api-controllers-action-results-q16)
  - [Q17. What does `[ProducesResponseType]` do on an API action?](#chapter-02-api-controllers-action-results-q17)
  - [Q18. What is a "thin controller" in Web API design?](#chapter-02-api-controllers-action-results-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 02. API Controllers & Action Results

### Q1. What is an API controller in ASP.NET Core? {#chapter-02-api-controllers-action-results-q1}

What is an API controller in ASP.NET Core?

**Answer:** An API controller is a class decorated with `[ApiController]` and route attributes that handles HTTP requests and returns data responses (JSON, XML, files) instead of views. It inherits from `ControllerBase` and contains action methods mapped to HTTP verbs.

- Registered via `builder.Services.AddControllers()` and `app.MapControllers()` in ASP.NET Core 8.
- Actions return `IActionResult`, `ActionResult<T>`, or concrete result types instead of `View()`.
- Constructor injection provides scoped services like repositories or application services.
- API controllers participate in model binding, validation, content negotiation, and filter pipelines.
- They are discovered by the routing system through attribute route templates on the class and actions.

---

### Q2. What does the `[ApiController]` attribute do? {#chapter-02-api-controllers-action-results-q2}

What does the `[ApiController]` attribute do?

**Answer:** `[ApiController]` enables opinionated API behaviors: automatic 400 responses for validation failures, binding source inference, attribute routing requirements, and ProblemDetails-compatible error responses. It signals the framework to apply Web API conventions instead of MVC view conventions.

- Invalid model state automatically produces HTTP 400 with `ValidationProblemDetails` — no manual `ModelState` check needed.
- Complex types from the body are inferred as `[FromBody]`; route values as `[FromRoute]`.
- `[ApiController]` requires attribute routing — conventional `{controller}/{action}` routing is not used.
- Multipart form and `[FromForm]` binding behave differently with source inference enabled.
- All controllers in a project should consistently use or omit `[ApiController]` to avoid mixed error contracts.

---

### Q3. What is the difference between `ControllerBase` and `Controller`? {#chapter-02-api-controllers-action-results-q3}

What is the difference between `ControllerBase` and `Controller`?

**Answer:** `ControllerBase` provides core API features (action results, model binding, HTTP context) without view-related helpers; `Controller` inherits `ControllerBase` and adds view methods like `View()`, `PartialView()`, and `ViewBag`. Web APIs inherit `ControllerBase`; MVC apps with Razor views inherit `Controller`.

- `ControllerBase` includes `Ok()`, `NotFound()`, `BadRequest()`, `CreatedAtAction()`, and related helpers.
- `Controller` adds `View()`, `ViewData`, `ViewBag`, and TempData for HTML rendering.
- API projects should use `ControllerBase` to avoid accidental view dependencies.
- Both support authorization attributes, filters, and model binding identically.
- Minimal APIs bypass controller classes entirely but produce equivalent HTTP responses.

---

### Q4. What is `IActionResult`? {#chapter-02-api-controllers-action-results-q4}

What is `IActionResult`?

**Answer:** `IActionResult` is the non-generic interface representing an HTTP response produced by executing a controller action — status code, headers, and optional body. Action methods return `IActionResult` or `Task<IActionResult>` to defer response execution until the result pipeline runs.

- Concrete implementations include `OkObjectResult`, `NotFoundResult`, `CreatedAtActionResult`, and `FileResult`.
- Helper methods like `Ok()`, `NotFound()`, and `BadRequest()` return `IActionResult` instances.
- The result executor selects an output formatter and writes the response after the action returns.
- `IActionResult` allows one action to return different result types based on conditions.
- OpenAPI tooling inspects `[ProducesResponseType]` attributes when the return type is non-generic `IActionResult`.

---

### Q5. What is `ActionResult<T>` and how does it differ from `IActionResult`? {#chapter-02-api-controllers-action-results-q5}

What is `ActionResult<T>` and how does it differ from `IActionResult`?

**Answer:** `ActionResult<T>` is a generic union type that documents the success response type `T` while still allowing error results like `NotFound()` or `BadRequest()`. It improves OpenAPI schema generation and compile-time clarity compared to bare `IActionResult`.

- Success: `return customerDto;` implicitly wraps as 200 OK with typed body.
- Error: `return NotFound();` still compiles — implicit conversion to `ActionResult<T>`.
- Swagger documents `T` as the 200 response schema automatically.
- Prefer `ActionResult<T>` when the happy path returns a known DTO type.
- Use `IActionResult` when responses vary widely (files, redirects, heterogeneous shapes).

---

### Q6. What is `CreatedAtAction` and when do you use it? {#chapter-02-api-controllers-action-results-q6}

What is `CreatedAtAction` and when do you use it?

**Answer:** `CreatedAtAction` returns HTTP 201 Created with a Location header generated by resolving a named action and route values through the routing system. Use it after POST (or PUT that creates) when the client needs the canonical URI of the new resource.

- Example: `return CreatedAtAction(nameof(GetById), new { id = order.Id }, orderDto);`
- The Location header points to the GET action that retrieves the created resource.
- Route values must include all template parameters (`id`, `tenantId`, etc.) or the URL 404s.
- Returns 201 with the created entity (or DTO) in the response body.
- Preferred over manual URL strings because it respects path base, lowercase URLs, and route refactors.

---

### Q7. What is the difference between `CreatedAtAction` and `CreatedAtRoute`? {#chapter-02-api-controllers-action-results-q7}

What is the difference between `CreatedAtAction` and `CreatedAtRoute`?

**Answer:** `CreatedAtAction` generates the Location URL by action method name and route values; `CreatedAtRoute` generates it by a named route defined with `Name = "..."` on a route attribute. Both return 201 Created — the choice depends on whether you anchor links to action names or stable route names.

- `CreatedAtAction(nameof(GetById), new { id = 5 }, dto)` — resolves via action name.
- `CreatedAtRoute("GetOrderById", new { id = 5 }, dto)` — requires `[HttpGet("{id}", Name = "GetOrderById")]`.
- Named routes survive action method renames if the route name stays constant.
- Both participate in `LinkGenerator` and respect `UsePathBase` when configured correctly.
- `Created(string uri, object value)` bypasses routing but breaks when URLs change behind gateways.

---

### Q8. What does the `Ok()` helper return? {#chapter-02-api-controllers-action-results-q8}

What does the `Ok()` helper return?

**Answer:** `Ok()` returns HTTP 200 OK — either with no body (`Ok()`) or with a serialized object (`Ok(dto)`). It is the standard success response for GET requests and updates where the client expects the resource in the body.

- `Ok()` produces `OkResult` — status 200, empty body.
- `Ok(dto)` produces `OkObjectResult` — status 200 with JSON/XML serialized body.
- Content negotiation selects the output formatter based on Accept header and `[Produces]`.
- Use 200 for successful reads and updates that return a representation.
- Do not use `Ok()` for resource creation — use `CreatedAtAction` for 201 instead.

---

### Q9. What does `NoContent()` return and when is it appropriate? {#chapter-02-api-controllers-action-results-q9}

What does `NoContent()` return and when is it appropriate?

**Answer:** `NoContent()` returns HTTP 204 with an empty body, signaling successful processing without a response payload. It is appropriate for DELETE and for PUT/PATCH when the client does not need the updated entity echoed back.

- Maps to `NoContentResult` in the action result pipeline.
- Must not include a response body — clients expect zero content length.
- Common for DELETE: `return NoContent();` after removing a resource.
- Alternative to `Ok()` when returning a resource after update is unnecessary.
- Cache invalidation middleware and HTTP clients rely on 204 semantics for delete operations.

---

### Q10. What is the difference between `NotFound()` and `BadRequest()`? {#chapter-02-api-controllers-action-results-q10}

What is the difference between `NotFound()` and `BadRequest()`?

**Answer:** `NotFound()` returns HTTP 404 when the requested resource does not exist at the given identifier; `BadRequest()` returns HTTP 400 when the request is invalid, malformed, or fails validation. They map to different failure categories in the HTTP specification.

- `NotFound()` — `GET /api/orders/999` where order 999 is absent.
- `BadRequest()` — invalid JSON, missing required fields, or business rule violation on input.
- `BadRequest(ModelState)` or `ValidationProblem()` includes field-level error details.
- `[ApiController]` auto-returns 400 for validation failures without explicit `BadRequest()` calls.
- Using the wrong status code breaks client retry logic and monitoring alerts.

---

### Q11. What HTTP status does `Conflict()` map to? {#chapter-02-api-controllers-action-results-q11}

What HTTP status does `Conflict()` map to?

**Answer:** `Conflict()` returns HTTP 409 Conflict, indicating the request could not be completed due to a conflict with the current state of the resource. Typical cases include duplicate unique keys, version mismatch, or concurrent update failures.

- Example: creating a user with an email that already exists.
- Also used for optimistic concurrency failures when an ETag or row version does not match.
- Distinct from 400 (bad input) and 404 (resource missing).
- ASP.NET Core maps `return Conflict()` to `ConflictResult` or `ConflictObjectResult` with optional body.
- Clients may fetch the current resource state and retry after receiving 409.

---

### Q12. Why should API actions return DTOs instead of EF entities? {#chapter-02-api-controllers-action-results-q12}

Why should API actions return DTOs instead of EF entities?

**Answer:** DTOs expose only the fields clients need, decoupling the HTTP contract from the database schema and preventing leaks of internal data, navigation properties, and circular references. EF entities change with migrations; DTOs provide a stable, intentional API surface.

- Entities may serialize `InternalNotes`, audit fields, or lazy-loaded navigation graphs unintentionally.
- Circular references between entities cause `JsonException` at runtime.
- DTOs allow different read and write shapes (`CreateOrderDto` vs `OrderResponseDto`).
- Projection with `Select` in EF Core queries avoids over-fetching and N+1 problems.
- Map entities to DTOs in the service layer or with tools like AutoMapper before returning from actions.

---

### Q13. What is the Location header used for in API responses? {#chapter-02-api-controllers-action-results-q13}

What is the Location header used for in API responses?

**Answer:** The Location header specifies the URI of a resource — most commonly on 201 Created responses to point clients to the newly created item. It can also appear on 3xx redirects to indicate where the client should look next.

- Generated automatically by `CreatedAtAction`, `CreatedAtRoute`, and `Created` helpers.
- Clients use it to fetch the resource without parsing the response body for an ID.
- Must reflect the publicly reachable URI, accounting for reverse proxy path bases.
- Integration tests should assert Location header value and verify follow-up GET succeeds.
- An incorrect Location header causes 404 on the client's next request.

---

### Q14. What is the difference between synchronous and asynchronous controller actions? {#chapter-02-api-controllers-action-results-q14}

What is the difference between synchronous and asynchronous controller actions?

**Answer:** Synchronous actions return `IActionResult` directly and block the request thread during I/O; asynchronous actions return `Task<IActionResult>` or `Task<ActionResult<T>>` and release the thread while awaiting I/O operations. ASP.NET Core 8 expects async actions for database, HTTP, and file operations.

- Async: `public async Task<ActionResult<OrderDto>> Get(int id) => Ok(await _service.GetAsync(id));`
- Sync: `public IActionResult Get(int id) => Ok(_service.Get(id));` — acceptable only for in-memory work.
- Async improves scalability under concurrent load by not holding threads during I/O waits.
- Kestrel handles thousands of concurrent connections efficiently with async actions.
- Action signature must use `async`/`await` all the way through the call chain.

---

### Q15. What problem does blocking on `.Result` cause in API controllers? {#chapter-02-api-controllers-action-results-q15}

What problem does blocking on `.Result` cause in API controllers?

**Answer:** Calling `.Result` or `.Wait()` on an incomplete `Task` inside an async-capable request blocks the thread pool thread while the async operation continues on another thread, causing thread-pool starvation and potential deadlocks under load. Always use `await` instead.

- Common anti-pattern: `var data = _service.GetAsync(id).Result;` in a sync action.
- Under concurrency, blocked threads accumulate and Kestrel cannot accept new requests.
- Classic deadlock when the continuation needs the same synchronization context.
- Integration tests with low concurrency miss this — production traffic exposes it.
- Fix: make the action `async Task<ActionResult<T>>` and `await` the service call.

---

### Q16. What is the difference between returning `Ok(entity)` and `CreatedAtAction` for POST? {#chapter-02-api-controllers-action-results-q16}

What is the difference between returning `Ok(entity)` and `CreatedAtAction` for POST?

**Answer:** `Ok(entity)` on POST returns HTTP 200 without a Location header — treating creation as a generic success. `CreatedAtAction` returns HTTP 201 with a Location header pointing to the new resource URI, following REST create semantics.

- 200 on create hides the canonical URL from REST clients and mobile SDKs.
- 201 explicitly signals resource birth and provides the address for subsequent GET/PATCH/DELETE.
- `CreatedAtAction` includes the created DTO in the body plus the Location header.
- OpenAPI documents should declare 201 for POST create endpoints, not 200.
- Use `Ok()` on POST only for non-create operations like search or command processing.

---

### Q17. What does `[ProducesResponseType]` do on an API action? {#chapter-02-api-controllers-action-results-q17}

What does `[ProducesResponseType]` do on an API action?

**Answer:** `[ProducesResponseType]` declares possible HTTP status codes and response body types for an action, feeding Swagger/OpenAPI generation and API explorer metadata. It documents the contract beyond what the return type alone conveys.

- Example: `[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]`
- Multiple attributes document different outcomes: 200, 404, 400 on the same action.
- Essential when the return type is `IActionResult` and the success type is not inferable.
- OpenAPI clients use this to generate typed response handling code.
- Does not change runtime behavior — it is metadata for documentation and tooling.

---

### Q18. What is a "thin controller" in Web API design? {#chapter-02-api-controllers-action-results-q18}

What is a "thin controller" in Web API design?

**Answer:** A thin controller validates input, authorizes the caller, calls one application service method, and maps the result to an HTTP response — it contains no business logic, data access, or cross-cutting orchestration. Fat controllers embed rules that belong in services, handlers, or domain layers.

- Controller: check auth → validate → `_orderService.CreateAsync(dto)` → `CreatedAtAction`.
- Business rules, transactions, and email sending live in the service or MediatR handler.
- Thin controllers are easier to unit test — mock the service, assert HTTP mapping.
- Mapping domain exceptions to HTTP status codes (404, 409) is the controller's job.
- Injecting services alone does not make a controller thin if orchestration remains in the action.

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

#### Q1. (R) Review this controller action under load. Integration tests pass locally; production threads spike and requests time out under concurrent traffic.

```csharp
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var report = _reportService.GenerateAsync(id).Result;
        return Ok(report);
    }
}
```

---

**Answer:**

**Answer:** Blocking on `.Result` inside a synchronous action captures the request thread while waiting for async I/O — under load this causes thread-pool starvation and cascading timeouts. The action must be `async Task<ActionResult<T>>` with `await` end-to-end.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `GenerateAsync` | Thread-pool starvation; classic ASP.NET Core production failure |
| API design | Sync action wrapping async service | Misleading signature hides blocking behavior |
| Scalability | Each request holds a thread during report generation | Kestrel queue grows; health checks fail |
| Testing | Low concurrency tests miss deadlock/starvation | Passes CI, fails peak traffic |

**Fix (priority order):**

1. Change to `public async Task<ActionResult<ReportDto>> Get(int id)` and `return Ok(await _reportService.GenerateAsync(id));`
2. Ensure service method stays async through database and file I/O — no sync-over-async deeper in the stack.
3. Add load test or parallel integration test to CI for hot endpoints.

**Production takeaway:** See debrief async snippet — **sync controller + `.Result`** is one of the most common Karat production traps. See C# Module 06 — async all the way.

---

---

#### Q2. (R) Review resource creation. Mobile clients create accounts successfully but cannot find the new user URI for subsequent PATCH calls — OpenAPI documents `201` with a Location header.

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    var user = _users.Create(dto);
    return Ok(user);
}
```

`UsersController` uses `[Route("api/[controller]")]`; action name is `Create`.

---

**Answer:**

**Answer:** Returning `200 OK` with a body omits the `Location` header clients use to discover the canonical resource URI — create operations should return `201 Created` via `CreatedAtAction` or `CreatedAtRoute` pointing at the GET-by-id action.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | `Ok(user)` on create returns `200` | Violates REST create convention; mobile SDK expects `201` |
| Contract | No `Location` header | Clients cannot link to `/api/users/{id}` without parsing body |
| Caching / semantics | `200` implies existing representation | Intermediaries may cache incorrectly |
| OpenAPI drift | Docs promise `201` | Partner integration tests fail in CI |

**Fix (priority order):**

1. `return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto);` — rename action consistently.
2. Return a response DTO, not EF entity with navigation properties.
3. Align Swagger response metadata with `[ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]`.

**Production takeaway:** Debrief explicitly calls out **201 + Location** — this is contract hygiene, not ceremony.

---

---

#### Q3. (R) Review `CreatedAtAction` usage. After deploy, the Location header points to `GET /api/users/GetUser/5` which returns 404.

```csharp
[HttpPost]
public ActionResult<UserDto> Create([FromBody] CreateUserDto dto)
{
    var created = _users.Create(dto);
    return CreatedAtAction(
        nameof(GetUser),
        new { id = created.Id },
        created);
}

[HttpGet("{id:int}")]
public ActionResult<UserDto> GetById(int id) => Ok(_users.Get(id));
```

---

**Answer:**

**Answer:** `CreatedAtAction` resolves the target action by name — `nameof(GetUser)` does not match `GetById`, so link generation targets a non-existent action or wrong route template. Action names, route values, and HTTP verb must align with an existing GET endpoint.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | Wrong action name in `CreatedAtAction` | Location URL 404 — clients cannot fetch created resource |
| API | Mismatch between `GetUser` vs `GetById` | Compile-time string would fail; `nameof` hid wrong symbol if renamed |
| Contract | Broken hypermedia without HATEOAS | Even minimal REST expects working Location |
| Testing | Missing assertion on Location header in integration tests | Bug ships to mobile |

**Fix (priority order):**

1. `return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);`
2. Verify route values include all template parameters (`id` matches `[HttpGet("{id:int}")]`.
3. Add `WebApplicationFactory` test asserting `Response.Headers.Location` and follow-up GET succeeds.

**Production takeaway:** **`CreatedAtAction` is link generation** — broken names are production 404s, not refactor nits.

---

---

#### Q4. (P) When should a production Web API action return `ActionResult<T>` vs `IActionResult`, and how does that choice affect OpenAPI schema generation and unit testing?

---

**Answer:**

**Answer:** Prefer `ActionResult<T>` when success returns a typed body and you still need `NotFound`, `BadRequest`, or other results — Swagger documents the success type while allowing multiple status codes; use bare `IActionResult` when return shape varies wildly or is non-generic.

- **`ActionResult<T>`:** Documents `T` for OpenAPI/NSwag; compiler helps when you `return dto` directly; still allows `return NotFound()` as implicit conversion.
- **`IActionResult`:** Flexible for file downloads, redirects, or heterogeneous responses — OpenAPI may need `[ProducesResponseType]` attributes for each status.
- **Avoid `Task<T>` alone** on API actions if you need to return `404`/`400` without throwing — you lose unified result types.
- **Testing:** Assert `result.Result` is `OkObjectResult` with typed value, or use `Assert.IsType<ActionResult<UserDto>>`.
- **Minimal APIs:** `Results<T>` is the parallel pattern — controllers use `ActionResult<T>` for the same reason.

**Production takeaway:** The choice is about **contract clarity in OpenAPI and compile-time safety**, not performance.

---

---

#### Q5. (R) Review DELETE and update responses. Cache invalidation middleware keys on status code; QA reports stale list pages after delete.

```csharp
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    _repo.Delete(id);
    return Ok(new { deleted = true, id });
}

[HttpPut("{id:int}")]
public IActionResult Replace(int id, [FromBody] UpdateDto dto)
{
    _repo.Replace(id, dto);
    return Ok();
}
```

---

**Answer:**

**Answer:** DELETE should return `204 NoContent` when the body is empty — returning `200` with a JSON wrapper prevents cache middleware from recognizing delete semantics, and empty `Ok()` on PUT is ambiguous. Use `NoContent()` for successful delete/replace without a body.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | DELETE returns `200` with body | CDN/edge cache rules may not invalidate list keys |
| API design | Ad-hoc `{ deleted: true }` envelope | Clients parse unnecessary payload |
| Consistency | PUT returns `Ok()` with no content | Should be `204 NoContent` unless returning updated resource |
| REST | DELETE success commonly `204` | Tooling and HTTP clients expect no body |

**Fix (priority order):**

1. `return NoContent();` after successful delete.
2. PUT either returns `204` or `200` with full updated DTO — pick one and document.
3. Align cache middleware with `204`/`404` on DELETE for list invalidation.

**Production takeaway:** **Status code choice drives infrastructure behavior** — not just JSON shape.

---

---

#### Q6. (R) Review validation behavior. Frontend sends invalid JSON bodies but receives `200`-series responses with null fields processed as defaults.

```csharp
public class ProductsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequest request)
    {
        var product = _svc.Create(request);
        return Ok(product);
    }
}

public class CreateProductRequest
{
    [Required] public string Sku { get; set; } = "";
    [Range(0.01, 100000)] public decimal Price { get; set; }
}
```

No `[ApiController]` on the class; `[Required]` attributes present on the model.

---

**Answer:**

**Answer:** Without `[ApiController]`, automatic 400 validation on model state is disabled — `[Required]` attributes are ignored unless you check `ModelState.IsValid`. Add `[ApiController]` or explicit validation and return `ValidationProblem()`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Validation | Missing `[ApiController]` | Invalid requests reach service layer |
| Data integrity | Empty SKU creates products with `""` | Bad rows in database |
| HTTP | Should return `400 Bad Request` | Clients cannot show field errors |
| API contract | No `ProblemDetails` shape | Frontend parsing breaks |

**Fix (priority order):**

1. Add `[ApiController]` to controller — enables automatic 400 on validation failure.
2. Or explicitly: `if (!ModelState.IsValid) return ValidationProblem(ModelState);`
3. Enable `AddProblemDetails()` in `Program.cs` for consistent RFC 7807 responses.

**Production takeaway:** **`[ApiController]` is not decorative** — it activates API-specific behaviors including validation 400.

---

---

#### Q7. (D) Review this "thin controller" refactor proposal. The interviewer asks whether moving logic to the service layer actually fixed the design problems.

```csharp
[HttpPost("checkout")]
public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutRequest req)
{
    if (req.Items.Count == 0) return BadRequest("No items");
    if (!_inventory.HasStock(req.Items)) return Conflict("Out of stock");
    if (!_payments.Authorize(req.PaymentToken, req.Total)) return PaymentRequired();
    var order = await _orders.PlaceAsync(req);
    await _email.SendConfirmationAsync(order);
    await _analytics.TrackPurchaseAsync(order);
    return CreatedAtAction(nameof(Get), new { id = order.Id }, order.ToDto());
}
```

All dependencies are injected; no `[Authorize]` yet.

---

**Answer:**

**Answer:** Injecting services did not make the controller thin — orchestration, authorization gaps, and cross-cutting concerns (email, analytics) still live in the action. A true application service or command handler should own the workflow; the controller should authorize, validate, call one method, and map the result.

- **Still fat:** Stock check, payment, order placement, email, and analytics in one action — hard to test and reuse from message consumers.
- **Missing `[Authorize]`:** Payment operations exposed without policy — security review failure regardless of layering.
- **HTTP mapping:** Controller should map domain exceptions to `Conflict`, `402 Payment Required`, etc. — not embed business rules inline.
- **Better shape:** `CheckoutCommand` handled by `IOrderCheckoutHandler.CheckoutAsync` returning `Result<Order>`; controller maps to `CreatedAtAction`.
- **Trade-off:** One orchestrator class is fine for small teams — but this action is not "thin" yet.

**Production takeaway:** Karat distinguishes **DI wiring** from **separation of concerns** — thin controller means one job per action.

---

---

#### Q8. (R) Review return types and leaked domain models. Security scan flags internal fields in JSON responses.

```csharp
[HttpGet("{id:int}")]
public IActionResult Get(int id)
{
    var entity = _db.Orders
        .Include(o => o.InternalNotes)
        .Include(o => o.PaymentAudit)
        .FirstOrDefault(o => o.Id == id);

    if (entity is null) return NotFound();
    return Ok(entity);
}
```

Entity types map 1:1 to EF Core tables; no `[JsonIgnore]` or DTO projection.

---

**Answer:**

**Answer:** Returning EF entities directly serializes every public property — including internal audit and notes fields — and couples the HTTP contract to the database schema. Project to a response DTO and return `ActionResult<OrderDto>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `InternalNotes`, `PaymentAudit` in JSON | Data leak to clients |
| Design | Entity used as API contract | Schema changes break clients on every migration |
| Performance | `.Include` loads unnecessary graphs | Over-fetching on every GET |
| Serialization | Circular references may cause 500 | JsonException at runtime |

**Fix (priority order):**

1. Map to `OrderDto` with only client fields — `return Ok(entity.ToDto());`
2. Remove broad `.Include` — project in query or map in service.
3. Add integration test asserting response JSON excludes internal property names.

**Production takeaway:** **`Ok(entity)` is a common leak** — action results carry DTOs, not persistence models.

---

---

#### Q9. (M) Compare `CreatedAtAction`, `CreatedAtRoute`, and `Created(uri, value)` for a multi-tenant API where the public URL is `https://api.example.com/tenant/{tenantId}/orders/{id}`. Which helper survives renamed actions and attribute route refactors?

```csharp
[Route("api/{tenantId}/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public ActionResult<OrderDto> Create(string tenantId, [FromBody] CreateOrderDto dto) { /* ... */ }
}
```

**Answer:**

**Answer:** `CreatedAtAction` resolves via action name and route values — it survives action renames when you use `nameof` and include `tenantId` in route values; `CreatedAtRoute` is stronger when you name a route template explicitly; raw `Created(string uri, ...)` is brittle when host or path prefix changes behind a gateway.

- **`CreatedAtAction(nameof(Get), new { tenantId, id = order.Id }, dto)`:** Participates in routing system; fixes host/scheme when `LinkGenerator` configured with forwarded headers.
- **`CreatedAtRoute("GetOrderById", values, dto)`:** Requires `[HttpGet("{id}", Name = "GetOrderById")]` — survives action method renames if route name kept stable.
- **`Created($"/tenant/{tenantId}/orders/{id}", dto)`:** Hard-coded path breaks behind path base, API management prefix, or lowercase URL policy.
- **Multi-tenant:** Must pass **all** route parameters (`tenantId`, `id`) or Location 404s.
- **Prefer:** Named route or `CreatedAtAction` with `nameof` — avoid manual string URLs in production.

**Production takeaway:** Link generation must go through **`LinkGenerator`/routing** so reverse-proxy path bases and renames do not break clients.

---
