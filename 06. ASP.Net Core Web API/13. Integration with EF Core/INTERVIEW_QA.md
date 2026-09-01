# Integration with EF Core — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 13. Integration with EF Core](#chapter-13-integration-with-ef-core)
  - [Q1. What is the typical DbContext lifetime in a Web API request?](#chapter-13-integration-with-ef-core-q1)
  - [Q2. Why should API controllers avoid returning EF entities direc…](#chapter-13-integration-with-ef-core-q2)
  - [Q3. What is the N+1 query problem in API endpoints?](#chapter-13-integration-with-ef-core-q3)
  - [Q4. What is `AsNoTracking` and when should read-only API actions…](#chapter-13-integration-with-ef-core-q4)
  - [Q5. What is the difference between `Include` and projection (`Se…](#chapter-13-integration-with-ef-core-q5)
  - [Q6. What is `SaveChangesAsync` in the context of API POST/PUT ac…](#chapter-13-integration-with-ef-core-q6)
  - [Q7. What is `DbUpdateConcurrencyException` in Web APIs?](#chapter-13-integration-with-ef-core-q7)
  - [Q8. What is the repository pattern for Web APIs?](#chapter-13-integration-with-ef-core-q8)
  - [Q9. What is `IQueryable` and why is returning it from repositori…](#chapter-13-integration-with-ef-core-q9)
  - [Q10. What is the difference between scoped DbContext and `IDbCont…](#chapter-13-integration-with-ef-core-q10)
  - [Q11. What is a transaction boundary in an API checkout flow?](#chapter-13-integration-with-ef-core-q11)
  - [Q12. What is pagination with Skip and Take?](#chapter-13-integration-with-ef-core-q12)
  - [Q13. What causes unstable pagination in concurrent APIs?](#chapter-13-integration-with-ef-core-q13)
  - [Q14. What is DTO projection with EF Core Select?](#chapter-13-integration-with-ef-core-q14)
  - [Q15. What happens when DbContext is injected into a Singleton ser…](#chapter-13-integration-with-ef-core-q15)
  - [Q16. What is lazy loading and why is it problematic for APIs?](#chapter-13-integration-with-ef-core-q16)
  - [Q17. What is the difference between `FindAsync` and `FirstOrDefau…](#chapter-13-integration-with-ef-core-q17)
  - [Q18. What is `AddDbContextFactory` used for in Web APIs?](#chapter-13-integration-with-ef-core-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 13. Integration with EF Core

### Q1. What is the typical DbContext lifetime in a Web API request? {#chapter-13-integration-with-ef-core-q1}

What is the typical DbContext lifetime in a Web API request?

**Answer:** In ASP.NET Core 8 Web APIs, `DbContext` is registered as **scoped** by default via `AddDbContext`, so one instance is created per HTTP request and disposed when the request completes. That aligns the unit of work with a single API call and keeps change tracking isolated between concurrent clients.

- `AddDbContext<AppDbContext>()` registers the context in the scoped DI container — the same scope as controllers and request-scoped services.
- A scoped context must not be captured by singleton services; doing so causes `ObjectDisposedException` or cross-request data leaks under concurrency.
- Long-running background work started from a request should not hold the request's `DbContext` after the response — use `IDbContextFactory` or a new scope instead.
- `SaveChangesAsync` runs against the tracked entities accumulated during that single request scope before disposal.

---

### Q2. Why should API controllers avoid returning EF entities directly? {#chapter-13-integration-with-ef-core-q2}

Why should API controllers avoid returning EF entities directly?

**Answer:** EF entities carry navigation properties, change-tracker state, and database-internal fields that were never meant to be a public HTTP contract. Serializing them leaks schema details, risks circular reference errors, and couples clients to your persistence model.

- Navigation properties can trigger lazy-loading N+1 queries during JSON serialization if proxies are enabled.
- Entities expose columns and relationships clients should not see (internal flags, audit fields, soft-delete markers).
- DTOs give you a stable API surface that can evolve independently of table or entity refactors.
- `[ApiController]` actions should map entities to DTOs in the service layer or via projection before returning `Ok(dto)`.

---

### Q3. What is the N+1 query problem in API endpoints? {#chapter-13-integration-with-ef-core-q3}

What is the N+1 query problem in API endpoints?

**Answer:** N+1 occurs when one query loads a parent collection and then each item triggers an additional query for a related navigation — for example, 1 query for 100 orders plus 100 queries for each order's customer. List endpoints become slow and exhaust the database connection pool under load.

- Common cause: returning entities with lazy-loaded navigations or accessing navigations after materialization without `Include`.
- Another cause: field-by-field resolver or loop that calls `context.Orders.Where(o => o.CustomerId == id)` per row.
- Fix with a single translated query: `.Select(o => new OrderDto { CustomerName = o.Customer.Name })` or explicit `.Include()` with split queries.
- Validate with EF logging or APM — list endpoints should target one (or a fixed small number of) SQL round trips.

---

### Q4. What is `AsNoTracking` and when should read-only API actions use it? {#chapter-13-integration-with-ef-core-q4}

What is `AsNoTracking` and when should read-only API actions use it?

**Answer:** `AsNoTracking()` tells EF Core not to snapshot entities in the change tracker, reducing memory and CPU for queries whose results are only serialized to the client. Read-only GET endpoints should use it by default because they never call `SaveChanges`.

- Tracked entities remain in memory for the entire scoped `DbContext` lifetime — expensive on large list responses.
- `AsNoTrackingWithIdentityResolution()` deduplicates repeated references in graphs when needed without full tracking.
- Command endpoints (POST/PUT/DELETE) that update entities typically omit `AsNoTracking` so changes are detected on `SaveChangesAsync`.
- Hot read paths can combine `AsNoTracking()` with projection (`Select`) to fetch only columns the DTO requires.

---

### Q5. What is the difference between `Include` and projection (`Select`) in API queries? {#chapter-13-integration-with-ef-core-q5}

What is the difference between `Include` and projection (`Select`) in API queries?

**Answer:** `Include` eagerly loads related entities into the change tracker as full entity graphs, while `Select` projects directly into DTOs in SQL, returning only the columns and shapes the API needs. Projection is usually preferred for read-only API responses.

- `Include(o => o.Lines)` generates JOIN or split queries and materializes complete `OrderLine` entities even if the client only needs a count or title.
- `.Select(o => new OrderDto(...))` translates to SQL that returns exactly the DTO fields — less data over the wire and less memory in the app.
- `Include` is appropriate when the service layer must modify related entities before save.
- `AsSplitQuery()` with multiple `Include`s avoids cartesian explosion on collection navigations but still loads full entities.

---

### Q6. What is `SaveChangesAsync` in the context of API POST/PUT actions? {#chapter-13-integration-with-ef-core-q6}

What is `SaveChangesAsync` in the context of API POST/PUT actions?

**Answer:** `SaveChangesAsync` persists all tracked insert, update, and delete operations accumulated in the current `DbContext` to the database in one transactional unit. API command actions call it after validating input and applying changes to entities or after `Add`/`Update`/`Remove`.

- Returns the number of affected rows; use the returned entity's generated keys (identity columns) after insert for `CreatedAtAction` responses.
- Runs inside an implicit transaction — all changes succeed or all roll back on failure.
- Should be awaited in async controller actions to avoid blocking thread-pool threads under load.
- For multi-step business flows (checkout, transfer), wrap multiple `SaveChanges` calls or operations in an explicit `BeginTransactionAsync` boundary.

---

### Q7. What is `DbUpdateConcurrencyException` in Web APIs? {#chapter-13-integration-with-ef-core-q7}

What is `DbUpdateConcurrencyException` in Web APIs?

**Answer:** EF Core throws `DbUpdateConcurrencyException` when an update or delete affects zero rows because another request changed or deleted the same row first — typically when a concurrency token (`[Timestamp]`/`rowversion` or configured token) no longer matches. Web APIs should catch this and return **409 Conflict**, not 500.

- Optimistic concurrency assumes conflicts are rare; the client must retry with fresh data.
- Without a concurrency token, last-write-wins silently overwrites prior updates.
- Map to `Conflict()` or a `ProblemDetails` response with a clear message for the client to refresh and retry.
- Common in PUT/PATCH endpoints on resources edited concurrently by multiple users or tabs.

---

### Q8. What is the repository pattern for Web APIs? {#chapter-13-integration-with-ef-core-q8}

What is the repository pattern for Web APIs?

**Answer:** The repository pattern wraps data access behind interfaces such as `IOrderRepository`, hiding EF queries from controllers and services. It centralizes query logic, simplifies unit testing with fakes, and keeps HTTP layers thin — though many teams use `DbContext` directly in application services instead of a generic repository.

- Controllers depend on `IOrderService` or `IOrderRepository`, not `AppDbContext` directly.
- Generic `IRepository<T>` abstractions often leak `IQueryable` and re-expose EF — prefer specific, use-case-driven methods.
- Repositories are registered scoped, same lifetime as `DbContext`.
- EF Core already implements repository and unit-of-work patterns; add explicit repositories when testing or team conventions require a clear persistence boundary.

---

### Q9. What is `IQueryable` and why is returning it from repositories risky? {#chapter-13-integration-with-ef-core-q9}

What is `IQueryable` and why is returning it from repositories risky?

**Answer:** `IQueryable<T>` represents a composable, deferred database query that executes only when enumerated. Returning it from repositories lets callers append filters, sorting, and paging — but also leaks EF-specific behavior, makes SQL shape unpredictable, and can cause queries to run outside the intended scope or after the `DbContext` is disposed.

- Callers may accidentally trigger client-side evaluation or multiple enumerations (double database hits).
- Exposing `IQueryable` from a repository ties upper layers to LINQ and EF translation rules.
- Prefer returning `Task<List<T>>`, `Task<T?>`, or paginated result types with explicit parameters.
- If composition is needed, keep it inside the repository or service with well-named methods.

---

### Q10. What is the difference between scoped DbContext and `IDbContextFactory`? {#chapter-13-integration-with-ef-core-q10}

What is the difference between scoped DbContext and `IDbContextFactory`?

**Answer:** Scoped `DbContext` from `AddDbContext` is injected once per HTTP request and shares the request's DI scope. `IDbContextFactory<TContext>` from `AddDbContextFactory` creates new context instances on demand — useful for parallel work, background tasks, or Blazor where a single scope does not map to one logical operation.

- Factory-created contexts must be disposed (`await using var context = await factory.CreateDbContextAsync()`).
- Do not inject scoped `DbContext` into singleton services; use the factory to create short-lived contexts instead.
- `AddDbContextPool` reuses context instances for performance in scoped request scenarios but still behaves as scoped per request.
- Web APIs use scoped `DbContext` for typical CRUD; factories appear in hosted services, GraphQL DataLoaders, or multi-threaded batch jobs.

---

### Q11. What is a transaction boundary in an API checkout flow? {#chapter-13-integration-with-ef-core-q11}

What is a transaction boundary in an API checkout flow?

**Answer:** A transaction boundary defines the atomic unit of work — either all persistence steps succeed (deduct inventory, create order, record payment) or none are committed. In EF Core, use `await context.Database.BeginTransactionAsync()` or a single `SaveChangesAsync` after all related changes when they fit one context.

- Partial commits (inventory reduced but order missing) indicate a missing or incorrectly scoped transaction.
- Distributed transactions across microservices use outbox patterns or sagas, not one EF transaction spanning databases.
- Keep transactions short — hold locks only for necessary database work, not external HTTP calls to payment gateways.
- `ExecutionStrategy` with retry (SQL transient failures) wraps transactions when using `SqlServerRetryingExecutionStrategy`.

---

### Q12. What is pagination with Skip and Take? {#chapter-13-integration-with-ef-core-q12}

What is pagination with Skip and Take?

**Answer:** Offset pagination uses `Skip((page - 1) * pageSize).Take(pageSize)` to return a fixed window of rows for list endpoints. Clients pass `page` and `pageSize` query parameters; the API returns the slice plus optional total count metadata.

- EF Core translates `Skip`/`Take` to `OFFSET`/`FETCH` in SQL Server.
- Always cap `pageSize` (e.g., max 100) to prevent unbounded queries.
- Include stable sort order (`OrderBy`) — without it, pages can return duplicate or missing rows between requests.
- Return pagination metadata in the response body or `Link` headers (`rel="next"`, `rel="prev"`).

---

### Q13. What causes unstable pagination in concurrent APIs? {#chapter-13-integration-with-ef-core-q13}

What causes unstable pagination in concurrent APIs?

**Answer:** Offset pagination is unstable when rows are inserted or deleted while a client walks pages — new rows shift positions, causing duplicates or skipped records between page 2 and page 3. High-write tables under concurrent load expose this frequently.

- `Skip(1000).Take(50)` becomes expensive on large offsets because the database still scans skipped rows.
- Keyset (cursor) pagination uses `WHERE Id > @lastSeenId ORDER BY Id TAKE 50` for stable, efficient paging on indexed columns.
- Timestamp-based cursors work when ids are not sequential but require tie-breaker columns.
- Document pagination strategy in the API contract so clients know whether totals and offsets are approximate.

---

### Q14. What is DTO projection with EF Core Select? {#chapter-13-integration-with-ef-core-q14}

What is DTO projection with EF Core Select?

**Answer:** DTO projection maps database rows directly to response types inside the LINQ query: `.Select(p => new ProductDto(p.Id, p.Name, p.Price))`. EF Core translates the expression to SQL that selects only required columns, avoiding entity materialization and extra mapping steps.

- Projection queries are naturally `AsNoTracking` — no entities enter the change tracker.
- Navigations can be flattened in one query: `CustomerName = o.Customer.Name` when translatable.
- Use records or constructors in DTOs for concise projection expressions.
- Client-side methods in `Select` break translation — keep projections to translatable property access and simple operators.

---

### Q15. What happens when DbContext is injected into a Singleton service? {#chapter-13-integration-with-ef-core-q15}

What happens when DbContext is injected into a Singleton service?

**Answer:** Injecting a scoped `DbContext` into a singleton creates a captive dependency — the singleton lives for the application lifetime but holds a disposed or shared context across requests. This causes `ObjectDisposedException`, stale data, and thread-safety violations under concurrent API traffic.

- ASP.NET Core DI validates scopes at startup in Development when `ValidateScopes` is enabled, surfacing the misconfiguration early.
- Singleton caches must not store entities tracked by a context — they become detached stale graphs.
- Fix by making the service scoped, or inject `IDbContextFactory<TContext>` and create a context per operation.
- Background singleton services should create a new DI scope (`IServiceScopeFactory.CreateScope()`) per work item.

---

### Q16. What is lazy loading and why is it problematic for APIs? {#chapter-13-integration-with-ef-core-q16}

What is lazy loading and why is it problematic for APIs?

**Answer:** Lazy loading automatically queries related entities when navigation properties are accessed on tracked proxies. In Web APIs, serializing an entity graph or touching navigations after the initial query triggers unexpected extra SQL (N+1) during response generation.

- Enabled via `UseLazyLoadingProxies()` — often disabled in API projects in favor of explicit loading or projection.
- Serializers walking object graphs can fire dozens of lazy loads per request without obvious code in the controller.
- Explicit `Include` or projection makes query cost visible and measurable in one place.
- If proxies are enabled, returning entities directly from actions is especially dangerous.

---

### Q17. What is the difference between `FindAsync` and `FirstOrDefaultAsync` in APIs? {#chapter-13-integration-with-ef-core-q17}

What is the difference between `FindAsync` and `FirstOrDefaultAsync` in APIs?

**Answer:** `FindAsync(key)` uses the context's local cache first, then queries by primary key — efficient for composite or single keys already tracked. `FirstOrDefaultAsync(predicate)` always translates to a SQL query with a `WHERE` clause and is required for non-key lookups or filters.

- `FindAsync` only works with primary key values, not arbitrary predicates.
- `FirstOrDefaultAsync(o => o.Id == id)` hits the database even if the entity is already tracked (unless EF's identity resolution applies in specific cases).
- For GET-by-id endpoints, either works when querying by PK; `FindAsync` can skip a round trip if the entity is cached in the context.
- Use `FirstOrDefaultAsync` with `AsNoTracking()` for read-only detail endpoints when the entity is not already tracked.

---

### Q18. What is `AddDbContextFactory` used for in Web APIs? {#chapter-13-integration-with-ef-core-q18}

What is `AddDbContextFactory` used for in Web APIs?

**Answer:** `AddDbContextFactory<AppDbContext>()` registers a factory that creates fresh `DbContext` instances outside the normal request scope. Web APIs use it for parallel operations within one request, background queue processors, or services that must not share a scoped context across threads.

- Register with `builder.Services.AddDbContextFactory<AppDbContext>(options => ...)` alongside or instead of scoped `AddDbContext` depending on needs.
- Each `CreateDbContext()` / `CreateDbContextAsync()` returns a context the caller must dispose.
- Common in GraphQL DataLoaders, report generators, and `IHostedService` workers that process jobs after the HTTP response.
- Factory options can mirror pooled configuration but instances are not shared across concurrent callers.

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

#### Q1. (R) Review this `OrdersController`. QA reports intermittent `DbUpdateConcurrencyException` and slow list endpoints under load.

```csharp
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> Get(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        return Ok(new OrderDto(order.Id, order.Total, order.CustomerId));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest req)
    {
        _db.Orders.Add(new Order { CustomerId = req.CustomerId, Total = req.Total });
        await _db.SaveChangesAsync();
        return Ok();
    }
}
```

The team says "controllers are thin — we just use EF directly." What breaks at scale, and what would you change first?

---

**Answer:**

**Answer:** Injecting `DbContext` directly into controllers couples HTTP, transactions, and persistence in one class — it works in demos but blocks test seams, scatters query logic, and makes concurrency and unit-of-work boundaries easy to get wrong. The immediate fixes are a service/repository layer, correct HTTP semantics on create, and explicit concurrency handling — not more logic in the controller.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Architecture | Controller owns EF queries and `SaveChanges` | Untestable without DB; business rules spread across endpoints |
| API contract | `POST` returns `200 OK` without `Location` or body | Clients cannot discover new resource id; violates REST create pattern |
| Concurrency | No row version / `DbUpdateConcurrencyException` handling | Intermittent 500s when two updates touch same order |
| DI / scope | Long controller methods hold scoped context for full request | Large graphs tracked longer than needed — memory pressure |

**Fix (priority order):**

1. Introduce `IOrderService` (scoped) — controller maps HTTP ↔ DTOs only; service owns `DbContext` usage.
2. Return `CreatedAtAction` with the new id after `SaveChangesAsync` (201 + Location).
3. Add concurrency token on `Order` and catch `DbUpdateConcurrencyException` → 409 Conflict.
4. Keep `AppDbContext` scoped via DI — never register as singleton.

**Production takeaway:** "Thin controller" means thin on HTTP concerns, not "inject DbContext and call EF inline." Karat uses this to test whether you know where persistence boundaries belong.

---

---

#### Q2. (R) Review this API projection. APM shows 1 query for the order list and 200 extra queries when the endpoint is hit with 100 rows.

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> List()
{
    var orders = await _db.Orders
        .Where(o => o.Status == OrderStatus.Open)
        .Select(o => new OrderSummaryDto
        {
            Id = o.Id,
            CustomerName = o.Customer.Name,
            LineCount = o.Lines.Count
        })
        .ToListAsync();

    return Ok(orders);
}
```

`Customer` and `Lines` are navigation properties; lazy loading is enabled globally.

---

**Answer:**

**Answer:** The projection references navigations (`Customer.Name`, `Lines.Count`) without translating them in a single SQL shape — with lazy loading enabled, materializing `Order` entities (or partially evaluated graphs) triggers one query per parent for each navigation access. The fix is a single translated query using projection or explicit `Include`/`AsSplitQuery`, and disable lazy loading for API projects.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| N+1 | Navigation props in projection with lazy loading | 1 + N (+ M) SQL round trips per request |
| Query shape | `Select` may not fully translate if client eval sneaks in | Silent fallback to client evaluation in older patterns |
| Performance | List endpoint scales with row count | DB connection pool exhaustion under load |

**Fix (priority order):**

1. Project in one query — EF translates `CustomerName = o.Customer.Name` and `LineCount = o.Lines.Count` into JOIN/subquery SQL when navigations are included in the expression tree before materialization.
2. If using explicit loading pattern: `.Include(o => o.Customer)` and use split query for collections — still one round trip per include level, not N+1.
3. Disable lazy loading (`UseLazyLoadingProxies` off) in API apps — fail fast if navigations accessed accidentally.
4. Validate with logging: `LogTo` or APM — assert query count = 1 for list endpoints.

**Production takeaway:** API projections must be **fully translatable** or **eager-loaded once** — N+1 is the default failure mode when navigations appear in DTO mapping.

---

---

#### Q3. (R) Review this read-only catalog endpoint. Memory spikes on the API pods during flash sales; GC pauses correlate with `GET /api/products`.

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
    [FromQuery] string? category)
{
    IQueryable<Product> query = _db.Products;

    if (!string.IsNullOrEmpty(category))
        query = query.Where(p => p.Category == category);

    var products = await query
        .Include(p => p.Supplier)
        .Include(p => p.Reviews)
        .ToListAsync();

    return Ok(products.Select(ProductDto.FromEntity));
}
```

No writes occur on this action. `_db` is scoped per request via DI.

---

**Answer:**

**Answer:** Read-only list endpoints still **track every entity** returned by EF unless you opt out. `Include` pulls large graphs into the change tracker for the lifetime of the scoped `DbContext`, multiplying memory per request when thousands of products load with suppliers and reviews.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracking | No `AsNoTracking()` on read query | Full snapshot of entities + relationships in memory |
| Over-fetching | `Include` on list endpoint | Multiplies payload and tracked graph size |
| API design | Maps full entities then DTO | Loads columns and navigations clients never see |

**Fix (priority order):**

1. Add `.AsNoTracking()` (or `AsNoTrackingWithIdentityResolution` if needed) on all read-only queries.
2. Replace `Include` + entity map with `.Select(p => new ProductDto(...))` — SQL projects only required columns.
3. Paginate — never return unbounded catalog lists (see Q6).
4. For hot read paths, consider cached read models or compiled queries — still untracked.

**Production takeaway:** **AsNoTracking is the default for GET list/detail in APIs** — tracking is for commands that call `SaveChanges`, not for serialization endpoints.

---

---

#### Q4. (R) Review this checkout flow. Money is deducted from inventory but the order row is missing when support queries SQL after a transient failure mid-request.

```csharp
[HttpPost("checkout")]
public async Task<IActionResult> Checkout(CheckoutRequest req)
{
    var product = await _db.Products.FindAsync(req.ProductId);
    if (product!.Stock < req.Quantity) return BadRequest("Out of stock");

    product.Stock -= req.Quantity;
    await _db.SaveChangesAsync();

    _db.Orders.Add(new Order { ProductId = req.ProductId, Qty = req.Quantity });
    await _db.SaveChangesAsync();

    return Ok();
}
```

---

**Answer:**

**Answer:** Two separate `SaveChangesAsync` calls mean **two independent transactions** — stock can commit while order insert fails on the next flush, leaving inventory inconsistent with orders. Checkout must be one atomic unit of work.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Transaction | No explicit transaction wrapping both saves | Partial commit — stock decremented, no order row |
| Correctness | Null-forgiving `product!` after `FindAsync` | NullReference if id invalid — fails after partial path |
| API design | No idempotency key for checkout | Retries may double-charge stock |

**Fix (priority order):**

1. Wrap in `await using var tx = await _db.Database.BeginTransactionAsync()` — single `SaveChangesAsync` at end, then `CommitAsync`.
2. Prefer one `SaveChanges` after all entity mutations in the same context (transaction still recommended for isolation).
3. Return structured errors; use row-level concurrency on `Stock` to prevent oversell.
4. Add idempotency key header for POST checkout in production APIs.

**Production takeaway:** **One business operation = one transaction** — "transaction per request" often means explicit `BeginTransaction` for multi-entity commands, not relying on implicit single SaveChanges boundaries across failure points.

---

---

#### Q5. (R) Review this repository and controller pair. Filtering by date works in unit tests (in-memory list) but returns wrong counts in production and sometimes throws after deploy.

```csharp
public class OrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public IQueryable<Order> GetAll() => _db.Orders.Include(o => o.Lines);
}

[HttpGet]
public ActionResult<IEnumerable<OrderDto>> Search([FromQuery] DateTime since)
{
    var orders = _repo.GetAll()
        .Where(o => o.CreatedUtc >= since)
        .ToList();

    return Ok(orders.Select(OrderDto.FromEntity));
}
```

The controller is synchronous; `_repo.GetAll()` returns `IQueryable<Order>`.

---

**Answer:**

**Answer:** Returning **`IQueryable` from the repository** and composing in the controller is valid only if execution stays deferred until the controller builds the full expression tree. Calling synchronous `.ToList()` in the controller forces client-side evaluation of anything not yet translated, pulls entire tables into memory, and breaks when the provider differs from the in-memory test fake.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Repository leak | `GetAll()` exposes composable `IQueryable` | Unbounded queries; filters may not translate |
| Sync over async | `.ToList()` blocks; no `ToListAsync` | Thread pool pressure; inconsistent with async pipeline |
| Design | `Include` always applied in `GetAll` | Over-fetch even when DTO needs no lines |
| Testing gap | In-memory provider hides translation bugs | Passes tests, fails SQL Server |

**Fix (priority order):**

1. Push filter + pagination into repository method: `Task<PagedResult<OrderDto>> SearchAsync(DateTime since, int page, ...)`.
2. Execute with `ToListAsync` **inside** repository on a fully built `IQueryable` — never return `IQueryable` to controllers unless you fully control expression boundaries.
3. Make controller action async end-to-end.
4. Integration-test against real SQL (or Testcontainers) for query translation.

**Production takeaway:** **Leaked `IQueryable`** is a common "works in unit test" trap — the repository should expose intent-based methods, not raw composable queries.

---

---

#### Q6. (R) Review this paginated list endpoint. Page 2 sometimes repeats rows from page 1; under concurrent inserts, clients see duplicates and gaps.

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<ProductDto>>> GetPage(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var skip = (page - 1) * pageSize;

    var items = await _db.Products
        .Skip(skip)
        .Take(pageSize)
        .Select(p => new ProductDto(p.Id, p.Name, p.Price))
        .ToListAsync();

    var total = await _db.Products.CountAsync();

    return Ok(new PagedResult<ProductDto>(items, total, page, pageSize));
}
```

---

**Answer:**

**Answer:** **`Skip`/`Take` without a stable `OrderBy`** produces undefined row order — SQL Server (and others) may return rows in any physical order, so pages overlap or skip as data moves. Pagination requires deterministic sort plus, for high-churn feeds, keyset pagination.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pagination | Missing `OrderBy` before `Skip`/`Take` | Duplicate/missing rows across pages |
| Concurrency | Offset pagination under concurrent inserts | Classic page drift during writes |
| Performance | `CountAsync()` on full table every request | Expensive on large tables |

**Fix (priority order):**

1. Add stable sort: `.OrderBy(p => p.Id)` or `.OrderByDescending(p => p.CreatedUtc).ThenBy(p => p.Id)`.
2. For live catalogs, prefer keyset: `Where(p => p.Id > lastId).OrderBy(p => p.Id).Take(pageSize)`.
3. Cache or approximate total count when exact total is not required.
4. Combine with `AsNoTracking` and column projection (Q3).

**Production takeaway:** **Pagination + EF** always means **ORDER BY + SKIP/FETCH** — without order, pages are nondeterministic.

---

---

#### Q7. (P) An API team registers `AppDbContext` as scoped and injects it into controllers, services, and a **Singleton** `PricingCacheWarmupService` that preloads prices at startup. What failure mode appears in production, and what patterns fix EF usage in background work?

---

**Answer:**

**Answer:** A singleton cannot consume a scoped `DbContext` — with `ValidateScopes` enabled, the app fails at startup; without validation, you get **captive dependency**: one disposed context reused across the app lifetime, or `ObjectDisposedException` after the first request scope ends. Background EF work needs its own scope per operation.

- Register `IDbContextFactory<AppDbContext>` or create a scope in the hosted service: `using var scope = _scopeFactory.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();`.
- Run warmup inside `IHostedService.StartAsync` with a **fresh scope**, not constructor-injected context.
- Never hold `DbContext` in singleton fields; cache **DTOs/primitives**, not the context.
- Enable `ValidateOnBuild` and scope validation in development to catch this at startup.

**Production takeaway:** Same captive-dependency rule as DI gotchas — **singleton + DbContext** is always wrong; factory or scoped resolution per job is the fix.

---

---

#### Q8. (D) You inherit an API where every list endpoint returns full EF entities (including navigation graphs) serialized directly to JSON. Product list responses are 2 MB and Swagger shows circular reference warnings. Compare three remediation options and when you would pick each.

---

**Answer:**

**Answer:** Returning tracked entity graphs couples persistence model to HTTP contract, over-fetches data, and invites circular reference hacks (`ReferenceHandler.IgnoreCycles`) that hide design problems.

- **DTO + projection (preferred for most REST APIs):** `.Select(p => new ProductListItemDto(...))` with `AsNoTracking` — smallest payload, stable contract, no cycle risk. Pick when clients need predictable JSON and you own the API surface.
- **AutoMapper / manual mapper from entity with explicit includes:** Faster migration from legacy code; still load too much if includes are broad. Pick for short-term refactor when many endpoints must ship quickly — plan to narrow queries.
- **GraphQL or OData (selective fields):** Client-driven shape — adds complexity, auth, and N+1 risk. Pick when many clients need different field sets and you will invest in DataLoader/guards — not as a band-aid for lazy entity serialization.

**Production takeaway:** Serialize **contracts**, not **EF graphs** — `ReferenceHandler.IgnoreCycles` is a warning sign, not a production strategy.

---

---

#### Q9. (M) A teammate proposes `AddDbContextFactory<AppDbContext>()` alongside scoped `AppDbContext` for the same API. Under what request patterns does `IDbContextFactory` help, and when should handlers keep using scoped `DbContext` from DI?

---

**Answer:**

**Answer:** Scoped `DbContext` from DI matches **one HTTP request = one unit of work** — controllers and services in the same request share the same tracker and transaction. `IDbContextFactory` creates **short-lived contexts** on demand — ideal when one request needs **multiple isolated units of work** (parallel tasks, middleware that must not share tracker state, background work triggered from a request).

- Use **scoped injection** for normal CRUD endpoints and services participating in one transaction per request.
- Use **factory** when: spawning `Task.Run` work (anti-pattern but seen), multi-tenant parallel queries, gRPC/streaming handlers that outlive a single logical UoW, or hosted services (with `CreateDbContext()` per operation).
- Do not inject both into the same class without clear boundaries — two contexts do not share change tracker; dual writes need explicit transaction coordination.
- Register factory with same options as scoped context (same connection, interceptors).

**Production takeaway:** Factory is not a replacement for scoped context in typical Web API actions — it solves **context lifetime shorter or multiple per operation** than the HTTP scope.

---

---
