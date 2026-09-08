# Integration with EF Core — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is the typical DbContext lifetime in a Web API request?](#q1-what-is-the-typical-dbcontext-lifetime-in-a-web-api-request)
2. [Q2. Why should API controllers avoid returning EF entities directly?](#q2-why-should-api-controllers-avoid-returning-ef-entities-directly)
3. [Q3. What is the N+1 query problem in API endpoints?](#q3-what-is-the-n1-query-problem-in-api-endpoints)
4. [Q4. What is `AsNoTracking` and when should read-only API actions use it?](#q4-what-is-asnotracking-and-when-should-read-only-api-actions-use-it)
5. [Q5. What is the difference between `Include` and projection (`Select`) in API queries?](#q5-what-is-the-difference-between-include-and-projection-select-in-api-queries)
6. [Q6. What is `SaveChangesAsync` in the context of API POST/PUT actions?](#q6-what-is-savechangesasync-in-the-context-of-api-postput-actions)
7. [Q7. What is `DbUpdateConcurrencyException` in Web APIs?](#q7-what-is-dbupdateconcurrencyexception-in-web-apis)
8. [Q8. What is the repository pattern for Web APIs?](#q8-what-is-the-repository-pattern-for-web-apis)
9. [Q9. What is `IQueryable` and why is returning it from repositories risky?](#q9-what-is-iqueryable-and-why-is-returning-it-from-repositories-risky)
10. [Q10. What is the difference between scoped DbContext and `IDbContextFactory`?](#q10-what-is-the-difference-between-scoped-dbcontext-and-idbcontextfactory)
11. [Q11. What is a transaction boundary in an API checkout flow?](#q11-what-is-a-transaction-boundary-in-an-api-checkout-flow)
12. [Q12. What is pagination with Skip and Take?](#q12-what-is-pagination-with-skip-and-take)
13. [Q13. What causes unstable pagination in concurrent APIs?](#q13-what-causes-unstable-pagination-in-concurrent-apis)
14. [Q14. What is DTO projection with EF Core Select?](#q14-what-is-dto-projection-with-ef-core-select)
15. [Q15. What happens when DbContext is injected into a Singleton service?](#q15-what-happens-when-dbcontext-is-injected-into-a-singleton-service)
16. [Q16. What is lazy loading and why is it problematic for APIs?](#q16-what-is-lazy-loading-and-why-is-it-problematic-for-apis)
17. [Q17. What is the difference between `FindAsync` and `FirstOrDefaultAsync` in APIs?](#q17-what-is-the-difference-between-findasync-and-firstordefaultasync-in-apis)
18. [Q18. What is `AddDbContextFactory` used for in Web APIs?](#q18-what-is-adddbcontextfactory-used-for-in-web-apis)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is the typical DbContext lifetime in a Web API request?

**Concepts**
- Scoped DbContext lifetime matching one HTTP request via AddDbContext
- Change tracking isolated between concurrent requests
- Captive dependency risk when singletons capture the scoped context
- Background work requiring a separate scope or IDbContextFactory
- SaveChangesAsync flushing the tracked unit of work at request end

**Answer**

`DbContext` is registered as scoped by default via `AddDbContext`, so one instance is created when the HTTP request begins and disposed when it ends. That aligns the unit of work with a single API call and keeps change tracking isolated between concurrent clients — one request's tracked entities never bleed into another's. The scoped context is injected into controllers and scoped services just like any other DI dependency. Capturing a scoped `DbContext` in a singleton causes either an `ObjectDisposedException` after the first request scope ends or cross-request data leaks because singleton fields survive beyond the scope boundary. Background operations triggered inside a request — queue processing, fire-and-forget jobs — must not hold the request's `DbContext` after the response, since its scope ends then; use `IDbContextFactory` or `IServiceScopeFactory.CreateScope()` per operation instead.

---

## Q2. Why should API controllers avoid returning EF entities directly?

**Concepts**
- Navigation properties leaking schema details not intended for public contracts
- Lazy-loading N+1 triggered during JSON serialization with proxy entities
- Circular reference serializer loops between related entities
- DTO decoupling API surface from persistence schema
- API contract stability across EF model refactors

**Answer**

EF entities carry navigation properties, change-tracker state, and database-internal fields that were never meant to be a public HTTP contract, so serializing them leaks schema details and tightly couples clients to table structure. Navigation properties can trigger lazy-loading during JSON serialization when proxies are enabled — the serializer accesses a property, EF fires a SQL query, and this repeats for every row in a list, producing N+1 round trips invisibly. Circular references between related entities cause JSON serializer loops or require `ReferenceHandler.IgnoreCycles` settings that hide design problems rather than fix them. DTOs with explicit shapes give the API a stable surface that evolves independently of entity or table refactors — adding audit columns, renaming fields, or changing relationships does not break the API contract when DTOs intermediate the mapping.

---

## Q3. What is the N+1 query problem in API endpoints?

**Concepts**
- One query loading parents plus N queries per row for related navigation
- Lazy loading triggering extra SQL during serialization or post-materialization access
- Include and ThenInclude for eager loading in one round trip
- EF projection using Select for single-query DTO fetch
- Connection pool exhaustion from unbounded per-row queries under load

**Answer**

N+1 occurs when a list endpoint loads a parent collection and then each item triggers an additional query for a related navigation — one query for 100 orders plus 100 queries for each order's customer, totaling 101 SQL round trips. The most common cause in APIs is returning entity objects with lazy-loaded navigation properties: the JSON serializer accesses a navigation, EF fires a SELECT, and this repeats once per parent row without any visible code in the controller. Another cause is post-materialization loops where code accesses `order.Customer` after `ToListAsync` for entities loaded without their related data. Fix with a single translated query: `.Select(o => new OrderSummaryDto { CustomerName = o.Customer.Name, LineCount = o.Lines.Count })` translates to SQL that retrieves everything in one round trip, or use explicit `.Include(o => o.Customer)` with `.ThenInclude` before materialization. Validate with EF logging or an APM tool — list endpoints should execute a fixed small number of SQL round trips regardless of result set size.

---

## Q4. What is `AsNoTracking` and when should read-only API actions use it?

**Concepts**
- AsNoTracking skipping change-tracker snapshot — reduced memory and CPU
- Read-only GET endpoints never calling SaveChanges — tracking waste
- AsNoTrackingWithIdentityResolution for graph deduplication without full tracking
- Tracked entities accumulating in scoped context memory for request lifetime
- Combining AsNoTracking with projection for maximum efficiency on hot reads

**Answer**

`AsNoTracking()` tells EF Core not to add returned entities to the change tracker, which means no identity snapshot, no original-values storage, and no change detection overhead. Read-only GET endpoints should use it by default because they never call `SaveChangesAsync` — tracked entities sit in memory consuming RAM and CPU from change detection for the entire scoped context lifetime without providing any benefit. On a list returning 500 products, `AsNoTracking` eliminates the snapshot overhead for each row. `AsNoTrackingWithIdentityResolution()` is a middle option when the result contains repeated references — it deduplicates object instances within the result set without full tracking cost. Command endpoints (POST, PUT, DELETE) that load and then modify entities omit `AsNoTracking` so the change detector picks up mutations on `SaveChangesAsync`. Hot read paths get maximum benefit from combining `AsNoTracking()` with a `Select` projection that fetches only DTO columns from SQL.

---

## Q5. What is the difference between `Include` and projection (`Select`) in API queries?

**Concepts**
- Include eagerly loading full entity graphs into the change tracker
- Select projecting directly to DTO shapes in SQL — only required columns
- Cartesian explosion from multiple Include on collection navigations
- AsSplitQuery splitting collection includes into separate SQL statements
- Projection preferred for read-only API responses to minimize data over the wire

**Answer**

`Include(o => o.Lines)` eagerly loads related entities as full entity graphs into the change tracker — even if the DTO needs only a line count, EF materializes every `OrderLine` column. `.Select(o => new OrderDto { LineCount = o.Lines.Count })` translates the projection to SQL that returns only the DTO fields, typically as a subquery or JOIN, sending less data over the wire and keeping no entities in the change tracker. Projection is preferred for read-only API responses since it is naturally `AsNoTracking` and fetches only what clients need. `Include` is appropriate when the service layer must modify related entities before saving, because tracked navigations allow change detection to pick up mutations automatically. Multiple `Include` calls on collection navigations produce a cartesian explosion where SQL rows multiply; `AsSplitQuery()` mitigates this by generating separate SELECT statements per include level while still loading full entities — projection remains cheaper when the DTO shape is known upfront.

---

## Q6. What is `SaveChangesAsync` in the context of API POST/PUT actions?

**Concepts**
- SaveChangesAsync persisting all tracked Add/Update/Remove in one implicit transaction
- Returning row count and generated primary key values after insert
- Awaiting to avoid thread-pool starvation under concurrent load
- BeginTransactionAsync for multi-step flows spanning multiple SaveChanges calls
- Implicit transaction rolling back all changes on any individual failure

**Answer**

`SaveChangesAsync` persists all tracked insert, update, and delete operations accumulated in the current `DbContext` to the database as one transactional unit — either all changes commit or all roll back on failure. API command actions call it after validating input and applying mutations to tracked entities or after `Add`/`Update`/`Remove`. The return value is the number of affected rows; after an insert, generated identity column values are populated on the in-memory entity, so you can pass `entity.Id` to `CreatedAtAction` without a follow-up query. Always `await` it in async controller actions rather than blocking with `.Result` to avoid thread-pool starvation under load. For multi-step business flows like checkout where multiple logical operations must either fully succeed or fully fail, wrap the steps in `await context.Database.BeginTransactionAsync()` and call `CommitAsync` at the end so partial commits do not leave data inconsistent across the intermediate `SaveChangesAsync` calls.

---

## Q7. What is `DbUpdateConcurrencyException` in Web APIs?

**Concepts**
- DbUpdateConcurrencyException thrown when affected row count is zero on update/delete
- Optimistic concurrency using rowversion or [Timestamp] token columns
- Mapping to 409 Conflict rather than 500 Internal Server Error
- Last-write-wins behavior when no concurrency token configured
- Client retry pattern with fresh data after 409 on concurrent edits

**Answer**

EF Core throws `DbUpdateConcurrencyException` when an `UPDATE` or `DELETE` affects zero rows, because another request changed or deleted the same row between when the current request loaded it and when it tried to save. This happens when a concurrency token — `[Timestamp]` on a `rowversion` column or a configured token — no longer matches, since EF includes the original token value in the `WHERE` clause and zero rows match. Web APIs must catch this exception and return **409 Conflict** rather than letting it propagate as a 500; the client then knows to refresh the resource and retry with the latest state. Without a concurrency token, EF has no basis for detecting conflicts and last-write-wins silently overwrites prior updates, which causes data loss on concurrent edits. This is most common on PUT and PATCH endpoints for resources that multiple users or browser tabs can edit simultaneously — add a `byte[] RowVersion` property with `[Timestamp]` to the entity and include it in the DTO's round-trip.

---

## Q8. What is the repository pattern for Web APIs?

**Concepts**
- Repository wrapping data access behind interfaces like IOrderRepository
- Controllers depending on abstractions not AppDbContext directly
- Test seam enabling fakes without a real database in unit tests
- Generic IRepository<T> leaking IQueryable — intent-based methods preferred
- Repository and service registered scoped, same lifetime as DbContext

**Answer**

The repository pattern wraps data access operations behind interfaces such as `IOrderRepository`, hiding EF queries from controllers and services so the HTTP layer focuses on request/response mapping and the persistence layer owns query logic. Controllers depend on `IOrderService` or `IOrderRepository` rather than `AppDbContext` directly, which creates a test seam where fakes can be injected without requiring a real database in unit tests. Generic `IRepository<T>` abstractions often leak `IQueryable` and re-expose EF details to callers, defeating the purpose — prefer specific, intent-named methods like `GetOpenOrdersByCustomerAsync(customerId)` that return concrete result types. EF Core already implements repository and unit-of-work patterns internally; explicit repository classes add value primarily when team conventions require a clear persistence boundary or when testing strategy demands mocking data access. Repositories register as scoped in DI, matching the DbContext lifetime so they share the same request-scoped context.

---

## Q9. What is `IQueryable` and why is returning it from repositories risky?

**Concepts**
- IQueryable as composable deferred database query executing on enumeration
- Repository callers appending arbitrary filters leaking EF translation rules
- DbContext disposed exception when IQueryable enumerated outside scope
- Client-side evaluation on untranslatable expressions causing full table loads
- Concrete return types like Task<List<T>> keeping query boundaries inside repositories

**Answer**

`IQueryable<T>` represents a composable, deferred database query that executes only when enumerated. Returning it from a repository method lets callers append `Where`, `OrderBy`, and `Select` — which looks flexible but leaks EF-specific translation behavior to layers that should not know about it and makes the SQL shape unpredictable and untestable. Callers may accidentally append expressions that cannot translate to SQL, causing silent client-side evaluation that pulls entire tables into memory and filters in C#. If the `DbContext` is disposed before the caller enumerates the `IQueryable` — common in async fire-and-forget or middleware scenarios — EF throws `ObjectDisposedException`. The fix is to execute the query inside the repository with `ToListAsync`, `FirstOrDefaultAsync`, or a keyset-paginated method, and return a concrete `Task<List<T>>`, `Task<T?>`, or a custom `PagedResult<T>` type. If composition is genuinely needed, keep it inside well-named repository methods whose SQL shape is tested as a unit.

---

## Q10. What is the difference between scoped DbContext and `IDbContextFactory`?

**Concepts**
- Scoped DbContext one instance shared per HTTP request via AddDbContext
- IDbContextFactory creating independent contexts on demand via CreateDbContextAsync
- Factory-created contexts requiring explicit disposal
- Singleton services requiring factory — never scoped DbContext injection
- AddDbContextPool reusing pooled instances while still scoping to the request

**Answer**

Scoped `DbContext` from `AddDbContext` is injected once per HTTP request and all services in that request's DI scope share the same instance — convenient for CRUD where one request equals one unit of work. `IDbContextFactory<TContext>` from `AddDbContextFactory` creates fresh context instances on demand, decoupled from any DI scope, so callers manage the lifetime explicitly with `await using var context = factory.CreateDbContextAsync()`. The factory is required when singleton services need database access (since they cannot consume scoped DI), when a single request spawns parallel operations that must use isolated contexts without shared change trackers, or in background hosted services where no HTTP scope exists. `AddDbContextPool` is a performance optimization that reuses context instances across requests but still respects scoped lifetime per request — it is not the same as `IDbContextFactory` and does not help singleton services. Web APIs use scoped `DbContext` for typical CRUD and factories for hosted services, GraphQL DataLoaders, and any scenario where multiple independent contexts are needed within one logical operation.

---

## Q11. What is a transaction boundary in an API checkout flow?

**Concepts**
- Explicit BeginTransactionAsync wrapping multiple SaveChanges calls atomically
- Partial commit leaving inventory inconsistent without explicit transaction
- Transaction scope limited to database work — not external HTTP calls
- ExecutionStrategy retry wrapping the transaction body for transient SQL failures
- Distributed transaction replacement using outbox pattern across microservices

**Answer**

A transaction boundary defines the atomic unit of work — either all persistence steps succeed (deduct inventory, create order, record payment) or none are committed. In EF Core, one `SaveChangesAsync` after all entity mutations in the same context uses an implicit transaction; when the checkout flow requires multiple separate `SaveChangesAsync` calls — or multiple repositories — wrap the whole operation in `await using var tx = await context.Database.BeginTransactionAsync()` and call `tx.CommitAsync()` only when all steps succeed. Without this, a failure between two `SaveChangesAsync` calls leaves data partially committed: inventory reduced but no order row created. Keep transactions short — hold locks only during the database work, never across external HTTP calls to a payment gateway, since those can take seconds and block rows. When using `SqlServerRetryingExecutionStrategy` for transient fault handling, wrap the transaction in a retry delegate because the strategy cannot retry transactions that span its own execution boundary automatically.

---

## Q12. What is pagination with Skip and Take?

**Concepts**
- Skip and Take translating to SQL OFFSET FETCH for windowed row return
- pageSize cap preventing unbounded queries from clients
- Stable OrderBy required for deterministic page slices
- Total count metadata enabling client-side page count rendering
- Link headers or response metadata communicating next/prev cursor tokens

**Answer**

Offset pagination uses `.Skip((page - 1) * pageSize).Take(pageSize)` to return a fixed window of rows. EF Core translates this to `OFFSET ... ROWS FETCH NEXT ... ROWS ONLY` in SQL Server. Always combine with `OrderBy` before `Skip` — without a stable sort the database may return rows in arbitrary physical order, meaning different pages return overlapping or missing rows. Cap `pageSize` to a maximum (e.g., 100) to prevent clients from requesting unbounded result sets. Return pagination metadata — total count, current page, page size, and ideally cursor tokens for next/previous — in the response body or `Link` headers. Total count requires a separate `CountAsync` query unless approximated via statistics; cache it briefly on hot read endpoints to reduce the per-request overhead of counting large tables on every page request.

---

## Q13. What causes unstable pagination in concurrent APIs?

**Concepts**
- Concurrent inserts shifting row positions between offset pagination requests
- Duplicate and skipped rows from page drift on high-write tables
- Keyset pagination anchored to a stable indexed column — no drift
- Large OFFSET performance cost scanning and discarding preceding rows
- Cursor token in API response as the keyset anchor for next page

**Answer**

Offset pagination is unstable under concurrent writes because the dataset changes between page requests. A new row inserted at position 50 in a `page = 1` result pushes all subsequent rows one position — when the client requests `page = 2`, the first row of page 2 is the same as the last row of page 1 (a duplicate) or a row from page 1 appears on page 2 (a skip), depending on whether the insert happened before or after the offset. `Skip(1000)` also forces the database to count and discard 1000 rows regardless of whether the client sees them, which is expensive on large tables. Keyset pagination avoids both problems: `WHERE id > @lastSeenId ORDER BY id TAKE @pageSize` anchors the window to a specific key value rather than a row count, so inserts before that key do not shift the anchor, and the index seek is efficient. Return the last key as a cursor token in the response, and the client passes it back as the `after` parameter for the next page.

---

## Q14. What is DTO projection with EF Core Select?

**Concepts**
- Select projecting inside LINQ query to DTO shapes before materialization
- EF translating projection to SQL selecting only required columns
- Naturally AsNoTracking — no entity enters the change tracker
- Navigation flattening CustomerName = o.Customer.Name in one query
- Client-side method calls in Select breaking SQL translation

**Answer**

DTO projection maps database rows directly to response types inside the LINQ expression: `.Select(p => new ProductDto(p.Id, p.Name, p.Price))`. EF Core translates the lambda to SQL that selects only the columns referenced in the DTO constructor or initializer, meaning unused columns never travel over the network or consume memory. Projection queries are naturally `AsNoTracking` because no entity is ever materialized — the change tracker has nothing to record. Navigations can be flattened in a single query when EF can translate the access: `CustomerName = o.Customer.Name` generates a JOIN in SQL without a separate round trip for each row. Keep projection expressions to translatable property access and simple operators; calling non-translatable C# methods like `string.Format` inside `Select` breaks translation and falls back to client-side evaluation, loading all columns then filtering in memory. Use record types or primary constructors for DTOs to make projection lambdas concise.

---

## Q15. What happens when DbContext is injected into a Singleton service?

**Concepts**
- Captive dependency — scoped service captured by longer-lived singleton
- ObjectDisposedException when request scope ends and context is disposed
- Thread-safety violations from concurrent request access to shared context
- ValidateScopes and ValidateOnBuild catching captive dependencies at startup
- IDbContextFactory or IServiceScopeFactory as fixes for singleton database access

**Answer**

Injecting a scoped `DbContext` into a singleton creates a captive dependency — the singleton's constructor runs once and captures the context indefinitely, but that context's scope ends when the first HTTP request completes and DI disposes it. Subsequent requests use the same singleton which now holds a disposed `DbContext`, producing `ObjectDisposedException` or, worse, stale change-tracker data that silently returns outdated entities. Under concurrent traffic, multiple requests use the same `DbContext` instance simultaneously, which is not thread-safe and causes data corruption in the change tracker. ASP.NET Core surfaces this at startup in Development when `ValidateScopes` is enabled — `BuildServiceProvider(validateScopes: true)` throws an `InvalidOperationException` describing the captive dependency before any request reaches the singleton. Fix by making the service scoped, injecting `IDbContextFactory<TContext>` and creating a context per operation, or injecting `IServiceScopeFactory` and creating a new scope per unit of work inside the singleton's method calls.

---

## Q16. What is lazy loading and why is it problematic for APIs?

**Concepts**
- Lazy loading automatically querying navigations on property access via proxy
- N+1 triggered by serializer walking entity graph without explicit includes
- UseLazyLoadingProxies typically disabled in API projects
- Navigation access after DbContext disposal throwing InvalidOperationException
- Explicit Include or projection making query cost visible and measurable

**Answer**

Lazy loading automatically fires a SQL query when a navigation property is accessed on a tracked proxy entity. In Web APIs this is problematic because serializers walk the object graph without any code in the controller: the JSON serializer touches `order.Customer`, EF fires a `SELECT` for that customer, and this repeats for every row in a list result without any obvious query happening in the action method. Dozens of lazy loads per request fire invisibly, exhausting the database connection pool under load. Lazy loading is also fragile after context disposal — if the entity leaves the request scope (cached in memory, returned to a background job), accessing a navigation throws `InvalidOperationException` because the context is gone. For these reasons, `UseLazyLoadingProxies` is typically disabled in API projects in favor of explicit `Include`/`ThenInclude` or projection, which make query cost visible in the code and measurable in EF logging.

---

## Q17. What is the difference between `FindAsync` and `FirstOrDefaultAsync` in APIs?

**Concepts**
- FindAsync checking change-tracker identity cache before hitting the database
- FirstOrDefaultAsync always executing SQL regardless of tracked state
- FindAsync limited to primary key values — no arbitrary predicates
- AsNoTracking on FirstOrDefaultAsync for read-only detail endpoints
- Identity resolution in EF returning tracked instance from FirstOrDefaultAsync when already loaded

**Answer**

`FindAsync(key)` checks the context's identity cache first — if an entity with that primary key is already tracked in the current request scope, it returns it without a round trip to the database; only on a cache miss does it execute SQL. `FirstOrDefaultAsync(e => e.Id == id)` always translates to a SQL WHERE clause and executes it, regardless of whether the entity is already tracked. `FindAsync` is limited to primary key lookups and cannot accept arbitrary predicates, so for any filter beyond the PK, `FirstOrDefaultAsync` is required. For GET-by-id endpoints that have not loaded the entity earlier in the request, both produce the same SQL; `FindAsync` offers a potential single-request cache hit benefit in scenarios where checkout loads and then immediately updates the same entity in one request. For read-only detail endpoints, use `FirstOrDefaultAsync` with `AsNoTracking()` to skip both change-tracker overhead and identity resolution.

---

## Q18. What is `AddDbContextFactory` used for in Web APIs?

**Concepts**
- AddDbContextFactory registering factory creating fresh DbContext on demand
- Factory-created context requiring explicit disposal with await using
- Parallel operations within one request needing isolated change trackers
- Singleton and background service database access requiring factory not scoped injection
- GraphQL DataLoader and streaming handlers as primary Web API use cases

**Answer**

`AddDbContextFactory<AppDbContext>()` registers a factory that creates fresh `DbContext` instances outside the normal request scope, each with its own change tracker and connection. Use `await using var context = await factory.CreateDbContextAsync()` — the caller owns lifetime and must dispose. Web APIs use this for parallel operations within one request where two concurrent tasks must not share a change tracker, for singleton background services that process work items after HTTP responses have ended, and for GraphQL DataLoaders that batch database calls across many field resolvers in parallel. `IHostedService` workers and `BackgroundService` implementations that process queue items independently of HTTP scopes are the most common scenario since they have no DI request scope to inject a scoped `DbContext` from. Register with the same options and connection string as the scoped registration; both can coexist in DI and do not conflict — scoped injection satisfies normal CRUD controllers and the factory satisfies services that need independent context lifetimes.

---

## Gotchas — EF Core Integration (Interview Traps)

---

#### Gotcha 1. `DbContext` scoped lifetime injected into a singleton service

**Concepts**
- `DbContext` registered as scoped — one instance per HTTP request
- Singleton service holding a scoped `DbContext` — context outlives the request scope
- Captured scoped `DbContext` used across requests — concurrency and stale state bugs
- `IServiceScopeFactory` to create a scope inside a singleton and resolve scoped services safely

**Answer**

`DbContext` is registered as scoped by `AddDbContext<T>()`, meaning one instance per request lifetime. When a singleton service receives a `DbContext` via constructor injection, the context is captured at application startup and reused across all requests — it accumulates change tracker state, stale cached entities, and concurrent access from multiple threads. I use `IServiceScopeFactory` in singleton services to create an explicit scope and resolve a fresh `DbContext` within it, disposing the scope when the unit of work completes, rather than holding a long-lived context reference.

---

#### Gotcha 2. `DbContext` disposed before async continuation completes

**Concepts**
- `DbContext` lifetime tied to the DI scope (HTTP request)
- `await` on a background thread after the request scope is disposed
- `IQueryable<T>` deferred execution after context disposal — `ObjectDisposedException`
- `await` fully before returning; avoid `Task.Run` with captured context

**Answer**

An `await` that resumes on a thread-pool thread after the HTTP request scope has been disposed causes `ObjectDisposedException` when EF Core tries to execute a deferred LINQ query or access a navigation property. This happens when `IQueryable<T>` is returned from a service method and evaluated by the controller after the service's scope has ended, or when `Task.Run(async () => ...)` captures the `DbContext` and executes after the request ends. I materialize all queries before returning from service methods — `await query.ToListAsync()` — and never pass an `IQueryable<T>` or `DbContext` reference across scope boundaries.

---

#### Gotcha 3. N+1 queries from lazy loading during JSON serialization

**Concepts**
- Lazy loading via navigation property access — one SQL query per navigation per entity
- JSON serializer traversing navigation properties during serialization
- 100-row list response executing 101 queries
- `Include`/`ThenInclude` for eager loading; DTO projection for single query

**Answer**

Lazy loading triggers a database query each time a navigation property is accessed. During JSON serialization, each `Order.Customer` access on a 100-item list fires a separate `SELECT` — 101 queries total. This is invisible in unit tests and development with small datasets but catastrophic in production with real volumes. I disable lazy loading by default (`UseLazyLoadingProxies` off, no `virtual` navigation properties) and use explicit `Include`/`ThenInclude` for eager loading or DTO projection via `Select(o => new OrderDto {...})` to generate a single JOIN query that fetches only the required columns.

---

#### Gotcha 4. Running migrations at startup under concurrent deployment

**Concepts**
- `context.Database.MigrateAsync()` on startup — runs migrations before accepting traffic
- Multiple pods starting simultaneously — race condition applying migrations
- EF Core migration history table (`__EFMigrationsHistory`) provides row-lock idempotency
- Startup probe holding traffic until migration completes vs migrations as deployment step

**Answer**

Running `MigrateAsync()` at startup is convenient for single-pod development but risky in a multi-pod production deployment — all pods start simultaneously and compete to apply the same migration. EF Core uses a row-level lock on `__EFMigrationsHistory`, so only one pod applies each migration while others wait, but all pods block at startup until migration completes. A long migration (large table backfill) extends startup time across all pods, triggering liveness probe failures. I run migrations as a separate deployment step (a Kubernetes job or release pipeline step) before the new version is rolled out, and remove `MigrateAsync()` from startup code.

---

#### Gotcha 5. Tracking queries for read-only API responses — change tracker overhead

**Concepts**
- Default EF Core behavior — entities tracked in change tracker for all queries
- Read-only API endpoints — tracking wastes memory and CPU for change detection
- `AsNoTracking()` — disables tracking, reducing memory and improving performance
- `AsNoTrackingWithIdentityResolution()` — no tracking but still deduplicates same-key entities

**Answer**

By default, EF Core tracks every entity returned by a query in the change tracker to support `SaveChanges()`. For read-only API endpoints that return DTOs or never modify the entities, tracking is pure overhead — the change tracker allocates snapshot data and performs identity map lookups that serve no purpose. I apply `AsNoTracking()` to all queries in read-only repository methods, reducing memory allocation by up to 50% for list queries. For queries that may return the same entity multiple times through different navigation paths, `AsNoTrackingWithIdentityResolution()` prevents duplicated objects without the full tracking overhead.

---

#### Gotcha 6. `DbContext` not thread-safe — concurrent access from async methods

**Concepts**
- `DbContext` is not thread-safe — not designed for concurrent operations
- `Task.WhenAll` with multiple awaits on the same `DbContext` — concurrent access exception
- Each logical operation should use its own `DbContext` instance or scope
- `InvalidOperationException: A second operation was started on this context`

**Answer**

`DbContext` is explicitly not thread-safe — concurrent access from multiple threads causes `InvalidOperationException: A second operation was started on this context instance before a previous asynchronous operation completed`. This happens when `await Task.WhenAll(context.Orders.ToListAsync(), context.Products.ToListAsync())` runs two queries concurrently on the same context. Each concurrent operation needs its own scoped `DbContext` instance. Within a single request, I await queries sequentially on one context or create separate scopes for parallel operations using `IServiceScopeFactory`.

---

#### Gotcha 7. Returning `IQueryable<T>` from controller or service

**Concepts**
- `IQueryable<T>` — query definition, not yet executed
- Controller returning `IQueryable<T>` — serializer evaluates query after method returns
- Scope disposal window — `DbContext` may be disposed before serializer reads the query
- Always materialize with `ToListAsync()` before returning from a service

**Answer**

`IQueryable<T>` is a query definition that executes when enumerated. Returning it from a service method and serializing it in the controller action risks executing the query after the DI scope (and the `DbContext`) has been disposed. The JSON serializer enumerates the `IQueryable` while writing the response, which occurs after the controller action method returns — by which time the request scope may be ending. I always materialize queries before returning from service methods: `return await query.ToListAsync(cancellationToken)`, giving back a concrete `List<T>` that no longer depends on the `DbContext`.

---

#### Gotcha 8. Global query filter bypassed with `IgnoreQueryFilters`

**Concepts**
- `HasQueryFilter(e => e.TenantId == currentTenantId)` — automatic multi-tenant filter
- `IgnoreQueryFilters()` — bypasses all global filters for that query
- Developers bypassing filters for admin endpoints — risk of cross-tenant data leakage
- Audit trail and authorization layer needed alongside query filters

**Answer**

EF Core global query filters (for soft delete, multi-tenancy, or row-level security) are a convenient safety net, but `IgnoreQueryFilters()` bypasses all of them in a single call. A developer adding an admin endpoint that calls `IgnoreQueryFilters()` to see all tenants' data accidentally exposes cross-tenant records if the endpoint lacks proper authorization. I add authorization checks that explicitly verify the caller has cross-tenant admin access before any query using `IgnoreQueryFilters()`, and code-review any new use of this method because it disables a security-relevant constraint.

---

#### Gotcha 9. `FromSqlRaw` with string interpolation — SQL injection

**Concepts**
- `FromSqlRaw($"SELECT * FROM Orders WHERE name = '{name}'")`  — injects raw string into SQL
- `FromSqlRaw("SELECT * FROM Orders WHERE name = {0}", name)` — parameterized, safe
- `FromSqlInterpolated($"SELECT * FROM Orders WHERE name = {name}")` — safe interpolation, converts to parameters
- EF Core parameterization — user input in `{0}` placeholders is parameterized, not interpolated

**Answer**

`FromSqlRaw` with a C# interpolated string (`$"..."`) builds the SQL by string interpolation before passing it to EF Core, inserting the raw user input directly into the query string — a classic SQL injection vulnerability. EF Core cannot parameterize a string that has already had user values interpolated into it. The safe alternatives are `FromSqlRaw("...WHERE name = {0}", name)` where `{0}` is a positional parameter placeholder EF Core converts to a `DbParameter`, or `FromSqlInterpolated($"...WHERE name = {name}")` which explicitly handles the interpolated values as parameters. I treat any `FromSqlRaw` call with a C# interpolated string as a code review rejection.

---

#### Gotcha 10. Eager loading all navigation properties unnecessarily

**Concepts**
- Full `Include`/`ThenInclude` chain — fetches entire object graph
- Over-fetching — columns and rows the action never uses in the response
- Response DTO needing only `CustomerId` and `CustomerName` — full Customer join wasted
- DTO projection in LINQ — `Select(o => new OrderSummaryDto {...})` fetching only needed columns

**Answer**

Including every navigation property with a chain of `Include`/`ThenInclude` fetches the full object graph from the database even when the endpoint needs only a few fields. A `GET /api/orders` endpoint that projects to `OrderSummaryDto` with `orderId`, `total`, and `customerName` does not need the full `Customer`, `Address`, and `Product` entities. Over-fetching with `Include` produces larger SQL result sets, more memory allocation, and slower serialization. I project directly to the DTO in the LINQ query: `Select(o => new OrderSummaryDto { OrderId = o.Id, CustomerName = o.Customer.Name, Total = o.Total })` which generates a SQL query with only the required joins and columns.

---

## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- Controller owning EF queries and SaveChanges blocking test seams
- POST returning 200 violating REST create contract — no Location or 201
- Missing concurrency token causing DbUpdateConcurrencyException to surface as 500
- Service layer separating HTTP concerns from persistence unit of work
- CreatedAtAction returning 201 with id for client resource discovery

**Answer**

Injecting `AppDbContext` directly into the controller couples HTTP semantics, transaction boundaries, and query logic in one class, which makes it difficult to unit test (requires a real database), scatters query logic across action methods, and makes concurrency boundaries invisible. The first issue to fix is the architecture: introduce an `IOrderService` registered scoped that owns all `DbContext` usage — the controller maps HTTP input to service parameters and service results to HTTP responses only. The second issue is the `POST` returning `Ok()` — a create action must return `CreatedAtAction("Get", new { id = created.Id }, createdDto)` to give the 201 status code and a `Location` header, so clients know the new resource's URL without parsing a response body. The third issue is missing concurrency handling: without a `[Timestamp]` or `rowversion` concurrency token on `Order`, concurrent updates silently last-write-win; with one configured, EF throws `DbUpdateConcurrencyException` when another request updated the row between load and save — catch it in the service and return `Conflict()` (409) so the client knows to refresh and retry. Keeping `AppDbContext` scoped via DI is already correct and should stay as-is.

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

**Concepts**
- Lazy loading firing per-row navigation queries during projection materialization
- Navigation access in Select requiring translatable expression tree before materialization
- EF translating Customer.Name and Lines.Count to JOIN and subquery in SQL
- UseLazyLoadingProxies globally enabled defeating single-query projection benefit
- Disabling lazy loading to fail fast rather than silently produce N+1

**Answer**

The projection references `o.Customer.Name` and `o.Lines.Count` inside `Select`, which EF Core 8 can translate to SQL as a JOIN and scalar subquery respectively — in a project without lazy loading enabled, this would produce one SQL statement. The problem here is that lazy loading is enabled globally via `UseLazyLoadingProxies`. When EF materializes the entities for the `Select` call and the proxy intercepts navigation access, it fires separate SELECT statements for `Customer` and `Lines` per row rather than letting the expression tree translation handle them in SQL. With 100 rows and two navigations each, that produces 1 + 100 + 100 = 201 queries. The fix has two parts: first, disable `UseLazyLoadingProxies` for the API project because lazy loading on APIs silently converts any navigation access into N+1 and provides no benefit compared to explicit projection or include; second, verify the `Select` expression is fully translatable by checking EF logs — `o.Customer.Name` and `o.Lines.Count` should translate when lazy loading is disabled, producing one SQL query with a LEFT JOIN and a subquery count. Add `AsNoTracking()` on the query as well since this is a read-only list endpoint.

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

**Concepts**
- Missing AsNoTracking causing all entities and includes to enter the change tracker
- Include loading full Supplier and Reviews entity graphs not needed after DTO mapping
- Over-fetching columns and rows — no pagination, no column projection
- Client-side Select(ProductDto.FromEntity) running after full entity materialization
- AsNoTracking plus projection eliminating tracker overhead and over-fetching together

**Answer**

There are three layered problems compounding during flash sales. First, the query has no `AsNoTracking()` — every `Product`, `Supplier`, and `Review` entity returned by the query enters the change tracker as a full snapshot including original-values. During a flash sale with hundreds of concurrent requests each returning the full catalog, the scoped context accumulates thousands of tracked entities in memory, causing the GC spikes. Second, `Include(p => p.Supplier)` and `Include(p => p.Reviews)` load full entity graphs including all columns, then `products.Select(ProductDto.FromEntity)` runs after materialization in C# — columns the DTO ignores still traveled over the wire and consumed memory. Third, there is no pagination — the endpoint returns the entire catalog on every request, which is unbounded. Fix in priority order: add `AsNoTracking()` immediately to eliminate tracker pressure; replace `Include` plus client-side map with a server-side `.Select(p => new ProductDto { ... })` projection so SQL returns only DTO columns and no `Supplier` or `Review` data beyond the fields used; add pagination with `Skip`/`Take` and a stable `OrderBy` to cap result size. For catalog endpoints that rarely change, also consider a short-TTL response cache at the controller or CDN level to reduce load during flash sales.

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

**Concepts**
- Two separate SaveChangesAsync calls as two independent implicit transactions
- Partial commit leaving inventory decremented without matching order row
- BeginTransactionAsync wrapping both saves in one atomic unit
- Null-forgiving operator on FindAsync result risking NullReferenceException
- Idempotency key preventing double-deduct on client retry after timeout

**Answer**

The two `SaveChangesAsync` calls execute as two independent implicit transactions. If anything fails after the first commits — a network interruption, an application exception, a process crash — the stock decrement is permanent but the order insert never ran, leaving inventory inconsistent with the order table. This is the partial commit scenario and the root cause of the missing order rows. Fix by wrapping both saves in one explicit transaction: `await using var tx = await _db.Database.BeginTransactionAsync()` before any mutation, accumulate both `product.Stock -= req.Quantity` and `_db.Orders.Add(...)`, call `SaveChangesAsync` once after both changes so EF sends both SQL statements in the same round trip, then call `tx.CommitAsync()` — if anything throws, the transaction rolls back and neither change persists. A secondary issue is `product!` with the null-forgiving operator: if `req.ProductId` is invalid, `FindAsync` returns null and the null-forgiving operator causes `NullReferenceException` at `product.Stock` — add a null check and return `NotFound()` explicitly. In production checkout APIs, also accept an idempotency key header so client retries after timeouts replay the same outcome rather than decrementing stock twice.

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

**Concepts**
- IQueryable leaked from repository composable in controller bypassing repository abstraction
- In-memory test provider skipping SQL translation that SQL Server applies
- Synchronous ToList blocking thread-pool threads — async ToListAsync required
- Always-applied Include fetching Lines even when DTO does not use them
- Intent-based repository methods encapsulating query shape and execution

**Answer**

Returning `IQueryable` from the repository leaks the query composition point to the controller, meaning the full expression tree including the controller's `Where` filter is executed in SQL — but in-memory test fakes use LINQ-to-Objects, which handles edge cases in date comparisons, timezone behavior, and string culture differently from SQL Server. What passes in tests fails in production because the translation gap is invisible. The synchronous `.ToList()` in the controller blocks the thread-pool thread while waiting for SQL, reducing Kestrel throughput under concurrent load — it should be `await _repo.SearchAsync(since)` with `ToListAsync` inside the repository. The `Include(o => o.Lines)` is always applied in `GetAll()` regardless of whether the DTO uses `Lines`, fetching and tracking the full line graph on every call including the `Search` endpoint. The repository's `GetAll()` returning `IQueryable` also means any caller in the codebase can append arbitrary expressions, making query shape unpredictable and difficult to test. Fix by replacing `GetAll()` with an intent-based async method: `Task<List<OrderSummaryDto>> SearchByDateAsync(DateTime since)` that encapsulates the filter, projects to DTO, adds `AsNoTracking`, executes with `ToListAsync`, and returns a concrete result type.

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

**Concepts**
- Missing OrderBy before Skip/Take producing undefined row order
- Offset pagination page drift from concurrent inserts between page requests
- Keyset pagination anchoring window to a stable indexed key eliminating drift
- Separate CountAsync on full table on every request adding query overhead
- pageSize cap missing — clients can request unbounded result sets

**Answer**

There are two layered issues. First, there is no `OrderBy` before `Skip`/`Take` — without an explicit sort, SQL Server returns rows in unspecified physical order, which means different page requests get different orderings and pages overlap even without concurrent inserts. Add `.OrderBy(p => p.Id)` or a stable multi-column sort before the offset. Second, even with stable sorting, offset pagination drifts under concurrent inserts: a product inserted at row 20 while a client is reading page 2 shifts all subsequent products one position, so the client receives a duplicate or skipped product at the page boundary. For a product catalog with active inserts, use keyset pagination: accept `afterId` as a query parameter instead of `page`, and query `_db.Products.Where(p => p.Id > afterId).OrderBy(p => p.Id).Take(pageSize)` — this anchors the window to a specific key and no row drift occurs. Return the last id in the response as the cursor for the next request. The `CountAsync()` on every request adds a full table scan; cache it or compute it only when the client requests the first page, since total counts on paginated APIs are approximate after the first page anyway. Also add a `pageSize` cap to prevent unbounded requests.

---

#### Q7. (P) An API team registers `AppDbContext` as scoped and injects it into controllers, services, and a **Singleton** `PricingCacheWarmupService` that preloads prices at startup. What failure mode appears in production, and what patterns fix EF usage in background work?

**Concepts**
- Captive dependency: scoped DbContext captured by singleton service
- ObjectDisposedException after first request scope ends
- ValidateScopes and ValidateOnBuild catching the misconfiguration at startup
- IDbContextFactory creating independent context per background operation
- IServiceScopeFactory.CreateScope for singleton services needing scoped DI

**Answer**

`PricingCacheWarmupService` is a singleton — it is constructed once and lives for the application lifetime. The scoped `AppDbContext` injected into its constructor is bound to the first request scope created during startup, which ends and is disposed shortly after the warmup runs. In Development with `ValidateScopes = true`, ASP.NET Core refuses to start with an `InvalidOperationException` describing the captive dependency. In Production without scope validation, the app starts but the context captured in the singleton is disposed after the first scope ends — any subsequent warmup refresh or retry call on `_db` in the singleton throws `ObjectDisposedException`. The failure is intermittent in production because it depends on timing of the first scope disposal relative to background re-use. There are two correct patterns. The first is `IDbContextFactory<AppDbContext>`: inject the factory into the singleton and call `await using var context = await _factory.CreateDbContextAsync()` inside each warmup operation — each call creates a fresh context that the caller disposes when done. The second is `IServiceScopeFactory`: inject it into the singleton and inside the warmup method call `using var scope = _scopeFactory.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>()` — this creates a proper scoped context for the duration of the operation. Cache the resulting DTOs or primitives in the singleton's fields, never the context or tracked entities.

---

#### Q8. (D) You inherit an API where every list endpoint returns full EF entities (including navigation graphs) serialized directly to JSON. Product list responses are 2 MB and Swagger shows circular reference warnings. Compare three remediation options and when you would pick each.

**Concepts**
- DTO projection as the preferred fix — minimal payload, stable contract, no cycle risk
- AutoMapper from entity as short-term migration path for large codebases
- GraphQL or OData as client-driven shape for diverse consumer field requirements
- ReferenceHandler.IgnoreCycles as a warning sign not a production solution
- Payload size reduction from column projection eliminating unused navigation data

**Answer**

The circular reference warnings confirm that navigation property graphs loop — `Order` references `Customer` which references `Orders` which loops. `ReferenceHandler.IgnoreCycles` suppresses the exception but silently omits data, which breaks clients that expect the full response shape. There are three remediation options with different trade-offs. The first is DTO projection with `Select`: replace entity serialization with `.Select(p => new ProductListItemDto(p.Id, p.Name, p.Price))` so EF generates SQL that fetches only DTO columns, no navigations enter the change tracker, and there are no circular references by design. This reduces the 2 MB response to only the fields clients actually use and is the right choice when you own the API surface and can define stable response contracts. The second is AutoMapper from loaded entities with explicit includes: faster to migrate in a large codebase where adding `Select` projections to hundreds of endpoints would take weeks — map entities to DTOs after loading with controlled includes and no lazy loading. This still loads more columns than projection but fixes serialization issues quickly and gives time to optimize individual endpoints with projection later. The third is GraphQL or OData for client-driven field selection: each client requests exactly the fields it needs, eliminating over-fetch by design. Pick this when the team has multiple diverse consumer clients needing different field sets and is willing to invest in DataLoader for N+1 prevention and proper authorization guards on field-level access — not as a band-aid for the existing entity serialization problem.

---

#### Q9. (M) A teammate proposes `AddDbContextFactory<AppDbContext>()` alongside scoped `AppDbContext` for the same API. Under what request patterns does `IDbContextFactory` help, and when should handlers keep using scoped `DbContext` from DI?

**Concepts**
- Scoped DbContext as the correct choice for one HTTP request = one unit of work
- IDbContextFactory for parallel tasks needing isolated change trackers within one request
- Background hosted service database access requiring factory not scoped injection
- Two contexts in one request not sharing transaction without explicit coordination
- Factory-created context requiring explicit disposal — not managed by DI scope

**Answer**

Scoped `DbContext` from DI is correct for the vast majority of Web API handlers where one HTTP request equals one logical unit of work — all the services participating in a single request share the same context instance, which means they share the same change tracker and an implicit transaction boundary around `SaveChangesAsync`. This is the right model for CRUD endpoints, business workflows within one request, and any handler where one transactional save at the end is the desired behavior. `IDbContextFactory` is the right choice when the factory solves a specific problem the scoped context cannot: a singleton service that must query the database outside any HTTP request scope, a hosted service or `BackgroundService` that processes work items with no associated HTTP scope, a handler that spawns parallel database tasks within a single request where each task needs an isolated change tracker to avoid conflicts, or a GraphQL DataLoader that batches field resolutions across concurrent resolvers. Registering both `AddDbContextFactory` and `AddDbContext` is valid — they do not conflict. The scoped registration satisfies constructors that declare `AppDbContext`, and the factory satisfies constructors that declare `IDbContextFactory<AppDbContext>`. The critical constraint when using both in the same request is that factory-created contexts are independent of the scoped context — they do not share the change tracker, so writes to a factory context and the scoped context do not participate in the same implicit transaction without an explicit `BeginTransactionAsync` coordination.
