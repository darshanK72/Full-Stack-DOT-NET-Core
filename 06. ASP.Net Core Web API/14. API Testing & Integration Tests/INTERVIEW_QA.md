# API Testing & Integration Tests — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is `WebApplicationFactory` in ASP.NET Core?](#q1-what-is-webapplicationfactory-in-aspnet-core)
2. [Q2. What is the difference between unit tests and integration tests for APIs?](#q2-what-is-the-difference-between-unit-tests-and-integration-tests-for-apis)
3. [Q3. What is an in-memory test server for Web APIs?](#q3-what-is-an-in-memory-test-server-for-web-apis)
4. [Q4. What does `CreateClient()` on WebApplicationFactory return?](#q4-what-does-createclient-on-webapplicationfactory-return)
5. [Q5. What is the test pyramid for API development?](#q5-what-is-the-test-pyramid-for-api-development)
6. [Q6. What is Mock `HttpMessageHandler` used for?](#q6-what-is-mock-httpmessagehandler-used-for)
7. [Q7. Why should integration tests not use the production database?](#q7-why-should-integration-tests-not-use-the-production-database)
8. [Q8. What is Testcontainers for API testing?](#q8-what-is-testcontainers-for-api-testing)
9. [Q9. What is the difference between EF InMemory and real SQL for API tests?](#q9-what-is-the-difference-between-ef-inmemory-and-real-sql-for-api-tests)
10. [Q10. What is `ConfigureWebHost` in WebApplicationFactory?](#q10-what-is-configurewebhost-in-webapplicationfactory)
11. [Q11. How do you test authenticated API endpoints?](#q11-how-do-you-test-authenticated-api-endpoints)
12. [Q12. What is `PostAsJsonAsync` in integration tests?](#q12-what-is-postasjsonasync-in-integration-tests)
13. [Q13. What causes flaky parallel integration tests?](#q13-what-causes-flaky-parallel-integration-tests)
14. [Q14. What is the difference between mocking a service vs mocking HttpClient?](#q14-what-is-the-difference-between-mocking-a-service-vs-mocking-httpclient)
15. [Q15. What is a test fixture for API integration tests?](#q15-what-is-a-test-fixture-for-api-integration-tests)
16. [Q16. What is seed data in API integration tests?](#q16-what-is-seed-data-in-api-integration-tests)
17. [Q17. What is the difference between testing controllers directly vs testing via HTTP?](#q17-what-is-the-difference-between-testing-controllers-directly-vs-testing-via-http)
18. [Q18. Why must HttpClient instances be disposed properly in tests?](#q18-why-must-httpclient-instances-be-disposed-properly-in-tests)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is `WebApplicationFactory` in ASP.NET Core?

**Concepts**
- WebApplicationFactory<TEntryPoint> in-process bootstrapping
- TestServer — full middleware and DI pipeline without a network port
- ConfigureWebHost override for service replacement
- Microsoft.AspNetCore.Mvc.Testing package dependency
- `public partial class Program` exposure for top-level statements

**Answer**

`WebApplicationFactory<TEntryPoint>` bootstraps the real application assembly in an in-memory test server, running the full middleware pipeline, routing, DI container, and configuration without listening on a TCP port. The reason this matters is that integration tests can send real HTTP requests that exercise the same code path as production, rather than instantiating controllers in isolation. I subclass the factory to override `ConfigureWebHost` and swap out services like the database or external HTTP clients, and the test project references `Microsoft.AspNetCore.Mvc.Testing` to get access to it. For top-level statement programs I expose `public partial class Program { }` so the factory can reference it as its generic type argument.

---

## Q2. What is the difference between unit tests and integration tests for APIs?

**Concepts**
- Unit test isolation with mocked dependencies
- Integration test full HTTP pipeline coverage
- Test pyramid — unit/integration/E2E distribution
- Routing and binding gaps that unit tests cannot catch
- JSON contract and auth pipeline verification

**Answer**

Unit tests isolate a single class — a service or validator — with mocked dependencies and no HTTP pipeline, so they run fast and pinpoint logic failures precisely. The limitation is that they cannot catch routing misconfiguration, model binding source errors, or auth middleware gaps because none of those pieces are wired up. Integration tests boot the application and verify behavior through real HTTP requests against the full pipeline, which is where those issues surface. The test pyramid favors many unit tests at the base, a moderate layer of integration tests, and very few end-to-end tests against live external systems, since cost and flakiness increase as you move up.

---

## Q3. What is an in-memory test server for Web APIs?

**Concepts**
- TestServer in-process hosting without TCP
- CreateClient() base address wiring
- Pipeline fidelity compared to production Kestrel
- No port conflicts in CI environments

**Answer**

The in-memory test server (`TestServer`) hosts the ASP.NET Core app inside the test process itself, accepting `HttpClient` requests that flow through the real middleware pipeline without opening a network port. `WebApplicationFactory` configures this automatically, so `factory.CreateClient()` returns an `HttpClient` whose base address points at the test server and whose handler dispatches requests in-process. The behavior matches production pipeline semantics — model binding, filters, auth — which is why it is more valuable than directly instantiating controllers. It is not identical to production Kestrel for HTTP/2 edge cases or TLS, so I supplement with smoke tests against a deployed environment when those details matter.

---

## Q4. What does `CreateClient()` on WebApplicationFactory return?

**Concepts**
- Preconfigured HttpClient wired to TestServer
- WebApplicationFactoryClientOptions for redirect and cookie behavior
- Shared service provider across all client requests
- Factory and handler lifetime and disposal

**Answer**

`CreateClient()` returns a preconfigured `HttpClient` wired to the factory's in-memory test server, with the application's base address already set and a handler that dispatches requests through the hosted pipeline. I can pass `WebApplicationFactoryClientOptions` to control redirect handling, cookie handling, and the base address. The client shares the factory's service provider, so any service overrides I made in `ConfigureWebHost` apply to every request from that client. I need to dispose the factory — typically in fixture teardown — to release the host and handler resources; clients created via the standard `CreateClient()` pattern are managed by the factory's handler pool.

---

## Q5. What is the test pyramid for API development?

**Concepts**
- Test pyramid — unit/integration/E2E layers
- WebApplicationFactory for the integration tier
- E2E tests against staging kept out of default CI
- Inverted pyramid anti-pattern

**Answer**

The test pyramid puts many fast unit tests at the base, a moderate layer of integration tests in the middle, and few slow end-to-end tests at the top. For Web APIs specifically, unit tests cover services, validators, and mapping logic with mocked `DbContext` or repositories; integration tests use `WebApplicationFactory` with a test database to verify HTTP contracts, status codes, ProblemDetails shapes, and auth; E2E tests hit a deployed staging environment for critical flows and stay out of the default CI run because they are slow and depend on external systems. Inverted pyramids — many Selenium or live-API tests with few unit tests — slow feedback cycles significantly and make it harder to isolate the root cause of failures.

---

## Q6. What is Mock `HttpMessageHandler` used for?

**Concepts**
- HttpMessageHandler interception for outbound HTTP
- SendAsync override returning canned HttpResponseMessage
- IHttpClientFactory test handler registration
- Service isolation vs HTTP boundary isolation distinction

**Answer**

A mock `HttpMessageHandler` intercepts `HttpClient` outbound calls in tests, returning canned responses without hitting the network. I subclass `HttpMessageHandler`, override `SendAsync` to assert the request URL, headers, and body, then return an `HttpResponseMessage` with test JSON. I can wire this up via `new HttpClient(mockHandler)` or by replacing the typed client's handler in `IHttpClientFactory` through `ConfigureTestServices`. This is the right tool when I want to verify that my service sends the correct payload to an external API — payment, identity, notifications — without sandbox charges or network dependency. The important distinction is that the API under test is still the real code; only the downstream HTTP is faked.

---

## Q7. Why should integration tests not use the production database?

**Concepts**
- Test data mutation and shared state risk
- ConfigureWebHost DbContext connection string replacement
- Non-deterministic failures from parallel test runs
- Production connection string isolation from test projects

**Answer**

Integration tests mutate data, seed fixtures, and often run in parallel, which means pointing them at a production or shared dev database risks destroying real records, hitting unique constraint collisions from concurrent test runs, and creating order-dependent failures. The fix is to override the `DbContext` registration in `ConfigureWebHost` with Testcontainers SQL, a local SQLite file, or a dedicated CI-only database. I never embed production connection strings in test projects or pipeline variables consumed by default `dotnet test`. Tests that leave debris in a shared database break subsequent runs and interfere with other developers' local environments, so isolation is a correctness requirement, not just a hygiene preference.

---

## Q8. What is Testcontainers for API testing?

**Concepts**
- Docker container lifecycle managed by test fixture
- Testcontainers.MsSql and IAsyncLifetime integration
- Real SQL semantics vs EF InMemory limitations
- CI Docker agent requirement and startup cost

**Answer**

Testcontainers spins up real Docker containers — SQL Server, PostgreSQL, Redis — during the test run, giving integration tests authentic database behavior without a permanent shared instance. I use NuGet packages like `Testcontainers.MsSql` with xUnit's `IAsyncLifetime` so the container starts in fixture setup and is destroyed after tests complete. The key advantage over EF InMemory is that real SQL enforces constraints, transactions, and provider-specific types, which means migration application, concurrency tokens, and raw SQL queries are actually tested. CI agents running GitHub Actions or Azure DevOps typically support Docker, though the 10–60 second startup per fixture is the main trade-off versus InMemory.

---

## Q9. What is the difference between EF InMemory and real SQL for API tests?

**Concepts**
- EF InMemory dictionary store vs relational provider
- Foreign key and unique constraint enforcement gap
- FromSqlRaw and migration inapplicability with InMemory
- Test tier selection by persistence concern

**Answer**

EF Core InMemory stores data in a process-local dictionary — it is fast and requires no infrastructure, but it ignores SQL semantics entirely. Foreign keys are not enforced, unique constraints do not trigger, transactions have no isolation, and `FromSqlRaw` queries simply do not work. Real SQL via Testcontainers or a local SQL instance validates what production actually enforces, which is why I reserve InMemory for tests focused purely on HTTP routing, serialization, or controller logic where the persistence layer is trivial. When I am testing EF migrations, concurrency tokens, or SQL-specific queries, I always use a real relational provider — the additional startup cost is worth the accuracy.

---

## Q10. What is `ConfigureWebHost` in WebApplicationFactory?

**Concepts**
- ConfigureWebHost override in factory subclass
- ConfigureTestServices for post-app DI replacement
- UseEnvironment("Testing") for config file selection
- Registration order — ConfigureTestServices runs after Program.cs

**Answer**

I override `ConfigureWebHost(IWebHostBuilder builder)` in a `WebApplicationFactory` subclass to change the test host configuration after the app's `Program.cs` has already run. The key method inside it is `builder.ConfigureTestServices(services => { ... })`, which lets me replace or decorate DI registrations — removing the real SQL `DbContext`, swapping in test auth handlers, replacing outbound HTTP handlers — because it runs after the app's own service registration. I also call `builder.UseEnvironment("Testing")` to load `appsettings.Testing.json` overrides. Everything I configure here applies to every request through `CreateClient()`, since it executes before the test server starts.

---

## Q11. How do you test authenticated API endpoints?

**Concepts**
- JWT bearer token attachment via DefaultRequestHeaders
- Test authentication handler with injected claims
- ClaimsIdentity construction for policy testing
- Anonymous vs authenticated vs forbidden path coverage

**Answer**

Integration tests must satisfy the same authentication middleware the app uses, since the real pipeline runs. For JWT-based APIs I either attach a valid token generated with a test signing key — `client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testToken)` — or I replace the authentication scheme entirely via `ConfigureTestServices`, adding a test handler that auto-succeeds and injects claims directly into the principal. The second approach is faster and avoids crypto setup for tests focused on authorization logic rather than token validation. I cover three distinct cases: anonymous requests that should get 401, authenticated requests with the right claims that should succeed, and authenticated requests with insufficient permissions that should get 403.

---

## Q12. What is `PostAsJsonAsync` in integration tests?

**Concepts**
- System.Net.Http.Json extension methods
- Automatic JSON serialization and Content-Type header
- ReadFromJsonAsync<T> response deserialization
- camelCase defaults matching ASP.NET Core conventions

**Answer**

`PostAsJsonAsync` is an `HttpClient` extension from `System.Net.Http.Json` that serializes a CLR object to JSON, sets `Content-Type: application/json`, and POSTs to the given URL in one call. I use it in integration tests to exercise model binding and validation on API endpoints without manually constructing the request body. I pair it with `ReadFromJsonAsync<T>()` to deserialize the response DTO and assert properties directly. It uses `System.Text.Json` defaults — camelCase property names — which match ASP.NET Core's API conventions, so the binding and serialization behavior in tests matches production unless a different naming policy has been configured explicitly.

---

## Q13. What causes flaky parallel integration tests?

**Concepts**
- xUnit parallel class execution and shared database state
- Unique constraint and lock contention from concurrent writes
- Collection fixture serialization with [Collection]
- Unique database name per test instance pattern

**Answer**

Parallel tests that share one database, one Testcontainers instance without isolation, or mutable static seed data race on writes and locks, which produces intermittent duplicate key errors, "database is locked" messages, or order-dependent pass/fail results. xUnit runs test classes in parallel by default, so a static `WebApplicationFactory` with a shared InMemory database name collides across classes in the same assembly. I fix this by using `[Collection("Database")]` to serialize tests that share a fixture, or by giving each test a unique database name — a `Guid.NewGuid()` suffix on the connection string. Flaky retries mask the design problem; deterministic isolation is the right solution, not `[Retry]` attributes.

---

## Q14. What is the difference between mocking a service vs mocking HttpClient?

**Concepts**
- IService mock at DI boundary — pipeline still runs
- HttpMessageHandler mock for outbound HTTP
- Boundary selection based on what is under test
- Over-mocking integration tests anti-pattern

**Answer**

Mocking a service like `IPaymentService` replaces an internal DI dependency so the HTTP pipeline, routing, and controllers still execute while the service itself is faked. Mocking `HttpMessageHandler` leaves the real service code running and only intercepts outbound HTTP calls that service makes. I choose the boundary based on what I am testing: if I want to verify controller behavior when payment always succeeds, I mock `IPaymentService`; if I want to verify that `PaymentService` builds correct HTTP requests and parses responses, I mock the handler and let the real service run. Over-mocking services inside a `WebApplicationFactory` test largely defeats the purpose of integration testing — it reduces the test to a unit test with the overhead of a full pipeline.

---

## Q15. What is a test fixture for API integration tests?

**Concepts**
- IClassFixture<T> shared resource per test class
- IAsyncLifetime for async container startup
- Collection fixture for cross-class sharing
- Fixture scope and isolation boundary tradeoff

**Answer**

A test fixture creates expensive shared resources once per test class or collection — `WebApplicationFactory`, a Testcontainers database, seeded data — and disposes them after all tests finish, which amortizes the startup cost. In xUnit I implement `IClassFixture<CustomWebApplicationFactory>` on my test class and receive the factory via constructor injection. For async startup such as Docker container initialization I implement `IAsyncLifetime` on the fixture. When I need to share one factory across multiple test classes that should not run in parallel, I use a collection fixture. The scope of the fixture defines how much state tests share — a broader scope gives faster tests but demands stricter isolation between test methods.

---

## Q16. What is seed data in API integration tests?

**Concepts**
- Pre-populated test database state before assertions
- Seed in fixture setup after migrations
- Reset strategy — recreate vs truncate vs transaction rollback
- GUID-based stable IDs to avoid identity seed assumptions

**Answer**

Seed data pre-populates the test database with known entities before assertions run — users, products, orders — so tests start from a predictable state rather than depending on other tests having run first. I apply seeds in fixture setup after migrations run, or I insert via `DbContext` in each test's arrange phase for finer control. The reset strategy matters: recreating the database per test is safest but slow; truncating tables or rolling back a transaction after each test is faster but requires careful design. I avoid hard-coded integer IDs that assume identity seed values and instead capture IDs from the seed inserts or use well-known GUIDs, so tests remain stable across schema changes.

---

## Q17. What is the difference between testing controllers directly vs testing via HTTP?

**Concepts**
- Controller direct instantiation bypassing pipeline
- WebApplicationFactory full HTTP round-trip verification
- [ApiController] automatic 400 response coverage
- Binding source, route template, and content negotiation testing

**Answer**

Direct controller testing instantiates the controller with mocked dependencies and calls action methods, which means routing, model binding, filters, and middleware are all skipped. That is fast and useful for isolating branching logic inside actions, but it cannot catch binding source mistakes, wrong route templates, or auth pipeline gaps. HTTP testing via `WebApplicationFactory` sends real requests through the full pipeline, so `[ApiController]` automatic 400 responses, `[FromBody]` binding, and content negotiation are all exercised. Direct tests also require manual setup of `ControllerContext`, `ModelState`, and `HttpContext`, which adds boilerplate. For verifying the API's public HTTP contract, the HTTP approach is the right choice.

---

## Q18. Why must HttpClient instances be disposed properly in tests?

**Concepts**
- HttpClient socket and connection pool ownership
- WebApplicationFactory disposal in fixture teardown
- Socket exhaustion and port starvation in long CI runs
- Testcontainers and factory disposal ordering

**Answer**

`HttpClient` and `WebApplicationFactory` hold sockets, `TestServer` hosts, and connection pool resources that the garbage collector may not finalize promptly during a long CI run. Undisposed clients and factories leak handles, causing port exhaustion and socket starvation that manifests as slow or failing subsequent test assemblies. I dispose `WebApplicationFactory` in fixture teardown — via `Dispose()` or `IAsyncDisposable` — since it manages the underlying handler pool for clients created from it. If I create `HttpClient` instances with custom handlers outside the factory pattern, each must be disposed per test. The disposal order matters when using Testcontainers: stop the container, dispose the factory, then release any clients created outside the factory.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

#### Gotcha 1. POST returning 200 instead of 201

**Concepts**
- HTTP 201 Created with Location header for resource creation
- CreatedAtAction and CreatedAtRoute response helpers
- REST client reliance on status codes and Location header

**Answer**

A successful resource creation must return HTTP 201 Created because that status communicates where the new resource lives via the `Location` header — returning 200 omits that contract and breaks REST clients that rely on status codes to decide their next action. I use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with a `Location` header pointing at the new resource URL, including the created representation or a minimal payload when clients need immediate data. OpenAPI-generated SDKs and standard HTTP client libraries inspect the status code, so returning 200 hides the resource URL from them silently.

---

#### Gotcha 2. GET that mutates state

**Concepts**
- HTTP safe and idempotent method semantics
- Browser prefetch and CDN cache replay risk
- GET read-only contract enforcement

**Answer**

GET must be safe and idempotent, which means performing deletes or updates inside a GET handler violates HTTP semantics and creates real hazards. Browsers, CDNs, and link-preview crawlers may invoke GET URLs without any user intent, so side effects run unintentionally. Cached GET responses can replay destructive operations or deliver stale mutations across clients. State changes belong on POST, PUT, PATCH, or DELETE — GET stays read-only.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Concepts**
- HTTP status codes driving retry logic and APM alerting
- ProblemDetails and ValidationProblemDetails for failures
- Envelope error pattern anti-pattern

**Answer**

Business failures must map to appropriate 4xx or 5xx status codes because HTTP status codes are what drive client retry logic, API gateway routing, and APM alerting. A 200 response with a `{ success: false }` flag forces every client to parse the body before knowing whether the call worked, which bypasses standard HTTP semantics entirely. I return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures and 404, 409, or 422 for domain errors. Envelope patterns like `{ success: false }` require custom handling in every consumer and break OpenAPI contract expectations since the schema reports 200 as the success contract.

---

#### Gotcha 4. Returning EF entities from API actions

**Concepts**
- Navigation property N+1 during serialization
- Circular reference serializer loop risk
- DTO decoupling from database schema

**Answer**

EF Core entities expose navigation properties, shadow fields, and circular references that are not designed for public contracts. When the JSON serializer encounters a lazy-loaded navigation it triggers a database query per row, and circular references between entities cause serializer loops or require fragile reference-handling settings. I always serialize DTOs with explicit shapes so the API contract is decoupled from the database schema and clients only receive the fields they need. This also means schema migrations do not break the API contract for fields that were not part of the DTO.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Concepts**
- System.Text.Json camelCase default in ASP.NET Core 8
- Silent binding failure on case mismatch
- JsonPropertyName attribute and PropertyNamingPolicy override

**Answer**

ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json`, so PascalCase property names from some clients bind as missing, leaving model properties at default values and causing silent data loss on POST and PUT — the request appears to succeed but the data is wrong. I align expectations using `[JsonPropertyName("PropertyName")]` on specific fields, a custom `PropertyNamingPolicy`, or `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)` when I must accept mixed casing from a legacy client. The failure is especially insidious because the server returns 201 or 204 with no error while the data is silently incomplete.

---

#### Gotcha 6. GET with `[FromBody]`

**Concepts**
- GET body stripping by proxies and HTTP clients
- [FromQuery] for simple filters
- OpenAPI and browser fetch GET body restrictions

**Answer**

Many HTTP clients, proxies, and caches ignore or strip GET request bodies, so filters sent as JSON in a GET request fail silently or never reach the action. Model binding for `[FromBody]` on GET is not reliably supported across the HTTP ecosystem. I use query strings with `[FromQuery]` for simple filter parameters, or POST to a dedicated search endpoint for complex filter objects that would otherwise be passed as a body. OpenAPI tools and browser `fetch` also discourage or block GET bodies, which makes this pattern fragile in production regardless of what ASP.NET Core itself accepts.

---

#### Gotcha 7. CORS as server security

**Concepts**
- CORS as browser-only enforcement mechanism
- curl and server-to-server bypass of CORS
- Authentication and authorization as real API security

**Answer**

CORS is enforced only by browsers — it does not stop curl, Postman, server-to-server calls, or any direct API request. CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they authenticate nothing. A public API without auth is fully accessible to any non-browser client regardless of the CORS policy configured. I register `AddCors` and `UseCors` specifically to enable browser SPA access, and I enforce JWT, cookies, or API keys separately as the actual security mechanism.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Concepts**
- Access-Control-Allow-Origin wildcard and credentials incompatibility
- WithOrigins explicit list requirement for credentialed requests
- AllowCredentials requirement for cookies and Authorization headers

**Answer**

Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers, so `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined in ASP.NET Core — the framework will not emit a valid CORS response for credentialed requests with a wildcard origin. I specify every trusted frontend origin explicitly with `WithOrigins`, including local dev URLs and production domains, and pair that with `AllowCredentials()`. Credentialed cross-origin calls require both a matching explicit origin and `Access-Control-Allow-Credentials: true` in the response headers.

---

#### Gotcha 9. Swagger UI exposed in Production

**Concepts**
- OpenAPI schema reconnaissance risk
- Environment-gated Swagger UI registration
- Production API surface disclosure

**Answer**

Public Swagger UI discloses the full API surface, schemas, and try-it-out access to anyone who discovers the `/swagger` endpoint, which makes it useful reconnaissance for attackers. I wrap `MapSwagger` and `UseSwaggerUI` in `Program.cs` with an environment check so they only serve in Development or Staging, and for internal tooling that needs OpenAPI in production I gate it behind authentication middleware. Exposed OpenAPI documents reveal internal endpoint names, field names, and enum values that simplify targeted attacks even without try-it-out access.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Concepts**
- [ApiController] enabling automatic model-state 400 responses
- [FromBody] inference for complex types
- Inconsistent error contracts from mixed controller conventions

**Answer**

Without `[ApiController]`, automatic 400 `ValidationProblemDetails` responses, binding source inference, and attribute routing behaviors differ from controllers that do have it. A mix of attributed and non-attributed controllers in the same Web API produces inconsistent error contracts — some endpoints return 200 with invalid models while others automatically validate and reject. I apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions and clients can rely on consistent behavior.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Concepts**
- Sync-over-async thread-pool starvation
- SynchronizationContext deadlock under ASP.NET Core
- async Task<IActionResult> propagation through service layer

**Answer**

Blocking on `.Result` or `.Wait()` in async API actions ties up Kestrel request threads while I/O completes, which reduces throughput under concurrent load since the thread cannot serve other requests. Deadlocks occur when the blocked thread holds a synchronization context the async continuation needs to resume on — this is a classic symptom in some hosting environments. I mark controller actions `async Task<IActionResult>` and propagate `await` through the entire service layer down to EF Core and `HttpClient` calls, so no thread is blocked waiting for I/O.

---

#### Gotcha 12. Liveness probe includes SQL check

**Concepts**
- Liveness vs readiness probe semantics in Kubernetes
- Unnecessary pod restart from database-down liveness failure
- /health/live lightweight self-check vs /health/ready dependency check

**Answer**

If the liveness probe fails when SQL is down, Kubernetes restarts the pod — but restarting the application cannot fix a database outage, so the restarts are both unnecessary and harmful since they interrupt in-flight requests. Liveness answers whether the process itself is healthy enough to continue running; readiness answers whether the pod should receive traffic. I put SQL, Redis, and external service checks on the readiness probe only, mapping `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck` or custom dependency tags so pods are removed from the load balancer during an outage without being killed.

---

#### Gotcha 13. N+1 queries in list endpoints

**Concepts**
- Lazy-loaded navigation property per-row SQL query
- LINQ projection to DTO in a single query
- Include/ThenInclude for explicit eager loading

**Answer**

Returning entities with lazy-loaded navigation properties triggers one SQL query per row in the list — a `GET /api/orders` that returns 100 orders with a `Customer` navigation fires 101 queries. I fix this by projecting directly to DTOs in LINQ so EF Core generates a single query with only the columns needed, or by using `Include`/`ThenInclude` for graphs that must be loaded together. The key is that serialization must never drive database queries; all data needed for the response should be fetched in a bounded number of round trips before serialization begins.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Concepts**
- Offset pagination instability under concurrent writes
- Keyset pagination with stable indexed key
- Cursor token exposure in response metadata

**Answer**

`Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are inserted or deleted between page requests, causing duplicates or gaps in the client's view. Keyset pagination avoids this by using `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key from the previous response — since the key is stable, concurrent inserts and deletes do not shift the window. I expose cursor tokens in link headers or response metadata for high-churn data, and I keep offset pagination only for small, mostly static tables where the instability risk is negligible.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Concepts**
- Field resolver per-parent database query explosion
- DataLoader batching into single IN clause
- Eager loading at root query as alternative

**Answer**

Field resolvers in HotChocolate that query the database per parent row explode into N+1 SQL calls under load — a list of 100 authors each resolving `books` individually fires 101 queries instead of one batched query. The fix is DataLoader: I register a batch loader in DI that collects author IDs during field resolution and issues a single `WHERE AuthorId IN (...)` query, distributing results back to the individual resolvers. When the client always requests nested fields together, I can also eager-load at the root query, but DataLoader is the more flexible solution since it only loads what was actually selected.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Concepts**
- Native gRPC HTTP/2 trailing headers inaccessible to browsers
- gRPC-Web middleware translation requirement
- CORS configuration alongside gRPC-Web

**Answer**

Native gRPC uses HTTP/2 binary framing and trailing headers that browser `fetch` and `XMLHttpRequest` APIs do not expose to JavaScript, which means standard gRPC clients work server-to-server but not from a browser. Blazor WASM and SPA browsers require the gRPC-Web protocol — I add `AddGrpcWeb()` and call `EnableGrpcWeb()` on mapped gRPC services to translate between gRPC-Web and native gRPC on the server side. I also configure CORS for the browser origin alongside gRPC-Web, since cross-origin browser calls still enforce CORS on preflight and response headers.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Review this "unit test" for `PaymentClient`. CI is flaky and once charged a real sandbox because someone removed the `[Explicit]` attribute.

```csharp
[Test]
public async Task Charge_succeeds_for_valid_card()
{
    var client = new PaymentClient(new HttpClient());
    var result = await client.ChargeAsync("4111111111111111", 99.00m);
    Assert.That(result.Status, Is.EqualTo("approved"));
}
```

`PaymentClient` constructor takes `HttpClient` and calls `https://api.payment-provider.com/charge`.

**Concepts**
- Live network call disguised as unit test
- HttpMessageHandler boundary for outbound HTTP isolation
- IHttpClientFactory test handler replacement
- Test pyramid placement — unit vs integration vs E2E

**Answer**

This test is an integration test against a live payment network, not a unit test — it hits a real URL, requires network access and credentials, and can trigger real charges. The first problem is that `HttpClient` is constructed with no handler override, so `ChargeAsync` makes a real outbound HTTP call on every run. The second problem is that the test has no mechanism to assert what payload was sent — it only checks the result, so a server-side change could break the test even when the client code is correct. I would fix this by injecting a mock `HttpMessageHandler` that intercepts `SendAsync`, asserts the request shape (URL, method, body), and returns a canned `HttpResponseMessage` with the approved JSON. True end-to-end sandbox tests belong behind `[Explicit]` or a separate pipeline stage that runs on-demand with properly managed secrets, never as part of default `dotnet test`.

---

#### Q2. (R) Review this integration test setup. Tests pass locally but fail in CI with unique constraint violations and stale data from prior runs.

```csharp
public class OrdersApiTests
{
    private WebApplicationFactory<Program> _factory = new();

    [Test]
    public async Task Post_order_returns_201()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/orders",
            new { CustomerId = 1, Total = 50m });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Get_orders_includes_new_order()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/orders");
        var body = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        Assert.That(body, Has.Count.GreaterThan(0));
    }
}
```

`Program.cs` registers `AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString))` pointing at the shared dev database. No test collection or factory customization.

**Concepts**
- WebApplicationFactory using real service registrations without override
- ConfigureTestServices DbContext replacement requirement
- Shared dev database mutation and test ordering dependency
- Seed and reset strategy per fixture

**Answer**

`WebApplicationFactory` boots the real application with real registrations unless `ConfigureWebHost` overrides them, so this setup connects to the shared dev SQL database and mutates it on every test run. The unique constraint violations in CI happen because a previous run left rows behind that conflict with new inserts. The ordering dependency is equally fragile — `Get_orders_includes_new_order` passes only when `Post_order_returns_201` has already run, which is not guaranteed under parallel or reordered execution. I would fix this by subclassing `WebApplicationFactory<Program>` and overriding `ConfigureWebHost` to replace the SQL Server registration with a Testcontainers instance or a per-fixture SQLite file, apply migrations in fixture setup, and either truncate tables or recreate the database between tests. I would also add `[Collection("Database")]` to serialize tests sharing this fixture, since parallel execution against one container causes the same race conditions.

---

#### Q3. (R) Review these parallel integration tests. Roughly 10% of CI runs fail with "database is locked" or duplicate key errors; re-running the job usually passes.

```csharp
[Parallelizable(ParallelScope.All)]
public class CustomerApiTests
{
    private static readonly WebApplicationFactory<Program> Factory = new();

    [Test]
    public async Task Create_customer_assigns_id()
    {
        var client = Factory.CreateClient();
        var email = $"user-{Guid.NewGuid()}@test.com";
        var response = await client.PostAsJsonAsync("/api/customers", new { Email = email });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Seed_reference_data_exists()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/customers/roles/admin");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
```

Both tests mutate the same SQLite file configured in `appsettings.Testing.json`.

**Concepts**
- Parallelizable all-scope with shared SQLite file lock contention
- Static factory shared singleton connection
- Unique database per test or per collection fixture
- Flaky test as design symptom, not infrastructure noise

**Answer**

`[Parallelizable(ParallelScope.All)]` runs all tests in the class concurrently, and both tests share a single static `WebApplicationFactory` connected to the same SQLite file. SQLite uses file-level locking, so concurrent writes collide and produce "database is locked" errors. The 10% failure rate with pass-on-retry is the classic signature of a race condition — the tests are not actually reliable, the race just does not manifest most of the time. The cleanest fix is to give each test its own database: generate a unique connection string per test using a `Guid` suffix, or use an in-memory SQLite with a unique name per factory instance. If the factory must be shared for performance, I would remove `[Parallelizable(ParallelScope.All)]` and serialize the database tests, or move to a Testcontainers instance per collection fixture. Re-running flaky parallel tests is never the right solution — it hides a database isolation design problem.

---

#### Q4. (R) Review this integration test for a protected endpoint. The test fails with 401 Unauthorized even though the handler works in Swagger with a bearer token.

```csharp
[Test]
public async Task Get_profile_returns_user()
{
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/api/me");
    var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();

    Assert.That(profile!.Email, Is.EqualTo("test@example.com"));
}
```

Production `Program.cs` uses `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` and `[Authorize]` on `MeController`. The test factory does not customize authentication.

**Concepts**
- Real JWT bearer pipeline running in test factory
- Test authentication handler with auto-succeed claims
- DefaultRequestHeaders.Authorization for token attachment
- Status code assertion before body deserialization

**Answer**

`WebApplicationFactory` runs the real authentication middleware, so `[Authorize]` on `MeController` requires a valid `Authorization: Bearer` header — but the test sends no token, which is why it gets 401 before the action ever runs. The secondary bug is that the test tries to deserialize the 401 response body as `ProfileDto`, which will either return null or throw, obscuring the real issue. I would fix this in one of two ways: replace the authentication configuration in `ConfigureTestServices` with a test scheme that auto-succeeds and injects the required claims — `services.AddAuthentication("Test").AddScheme<..., TestAuthHandler>("Test", ...)` — or generate a valid JWT signed with a test key matching the `JwtBearerOptions` override. I would also assert `response.StatusCode` before reading the body so the test fails at the right assertion with a clear message.

---

#### Q5. (R) Review this test class named `OrderServiceIntegrationTests`. The reviewer says it is not an integration test — agree or disagree, and what would you rename or restructure?

```csharp
[TestFixture]
public class OrderServiceIntegrationTests
{
    [Test]
    public async Task CreateOrder_calls_repository_and_returns_id()
    {
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(42);

        var service = new OrderService(mockRepo.Object, Mock.Of<ILogger<OrderService>>());
        var id = await service.CreateOrderAsync(new CreateOrderDto { Total = 10m });

        Assert.That(id, Is.EqualTo(42));
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    }
}
```

No `WebApplicationFactory`, no HTTP, no database.

**Concepts**
- Unit test with mocked repository vs integration test definition
- Test pyramid misclassification consequences
- True integration test requiring WebApplicationFactory with real database
- Mock boundary — collaboration contract vs composition root

**Answer**

I agree with the reviewer — this is a unit test. It mocks `IOrderRepository` and `ILogger`, instantiates `OrderService` directly, and exercises only the service's internal logic. No HTTP pipeline, database, DI container, or middleware participates. The name matters because "integration tests" implies a different level of trust to the team — if this suite passes, engineers should not conclude that the API's routing, DI wiring, or database integration works. I would rename the class to `OrderServiceTests` and move it to the unit test project. To complement it with a real integration test, I would add a `WebApplicationFactory`-based test that POSTs to `/api/orders` and asserts a row exists in the test database, which is what proves composition and persistence work together. The mock-based test stays at the base of the pyramid for fast business rule feedback.

---

#### Q6. (P) Your team debates testing an `InventoryService` that calls an external REST API. One engineer wants `WebApplicationFactory` end-to-end; another wants a mocked `HttpMessageHandler` in a unit test. Map each choice to the **test pyramid** — what belongs at the base vs the top, and what would you run on every PR?

**Concepts**
- Test pyramid layer selection by scope and cost
- HttpMessageHandler mock at base — outbound HTTP contract
- WebApplicationFactory in middle — in-process API integration
- E2E sandbox tests at top — scheduled, not per-PR

**Answer**

The two approaches test different things and belong at different pyramid levels — they are complementary, not competing. The mocked `HttpMessageHandler` belongs at the base: it tests that `InventoryService` builds the correct URL, headers, and request body and correctly parses the vendor's response format. This is fast, deterministic, and exercises the service's HTTP contract logic without any pipeline overhead. `WebApplicationFactory` belongs in the middle integration tier: it tests that the service is correctly registered in DI, that the controller or minimal endpoint wires it up correctly, and that auth and serialization work end-to-end — with the outbound HTTP still mocked at the handler level so no real vendor calls are made. True E2E tests against the vendor's sandbox sit at the top — scheduled or triggered manually, gated behind secrets management, and never on every PR because they are slow, flaky by nature, and can incur costs. I would run handler-mock unit tests plus narrow `WebApplicationFactory` integration tests on every PR, and schedule sandbox tests separately.

---

#### Q7. (D) Compare **EF Core InMemory provider** vs **Testcontainers (SQL Server/PostgreSQL)** for API integration tests. When is InMemory acceptable, when does it lie about SQL semantics, and what is the operational cost of Testcontainers in CI?

**Concepts**
- EF InMemory dictionary store — no SQL semantics
- Foreign key, constraint, and transaction fidelity gap
- Testcontainers Docker startup cost in CI
- Provider selection by test concern

**Answer**

EF InMemory is a lightweight fake — data lives in a process-local dictionary and there is no SQL translation, no constraint enforcement, no transaction isolation, and no provider-specific types. It is acceptable when the test is focused on the HTTP layer — routing, serialization, authorization — and the data access is so trivial that SQL semantics do not matter. It lies about SQL whenever the test needs to verify foreign key behavior, unique constraints, `FromSqlRaw`, EF migration application, concurrency tokens, or provider-specific column types like `datetimeoffset` or JSON columns — all of these work differently or not at all with InMemory. Testcontainers runs a real database engine in Docker, which matches production semantics accurately, but adds 10–60 seconds of fixture startup and requires a Docker daemon on the CI agent. I mitigate the cost by sharing one container per test collection rather than per test and using SQL LocalDB on Windows agents as a faster alternative. My rule of thumb: Testcontainers for any test that touches EF migrations, data-access queries, or persistence logic; InMemory only for controller-level tests where the store is a detail.

---

#### Q8. (M) Show the correct shape of a custom `WebApplicationFactory<Program>` that replaces `AppDbContext` with a test database and seeds reference data once per fixture. What goes in `ConfigureWebHost` vs `ConfigureServices`, and why does order matter?

**Concepts**
- ConfigureTestServices running after Program.cs registration
- RemoveAll to deregister existing DbContext options
- Services.CreateScope() for post-build seeding
- `public partial class Program` for factory generic argument

**Answer**

I subclass `WebApplicationFactory<Program>`, override `ConfigureWebHost`, set the environment to `Testing`, and use `ConfigureTestServices` to replace the `DbContext` registration after the app's `Program.cs` has already run. The order matters because `ConfigureTestServices` runs after the app's service registration, so I must call `services.RemoveAll(typeof(DbContextOptions<AppDbContext>))` before adding the test connection to avoid duplicate registrations. After building the host I seed reference data by creating a scope from `factory.Services` and resolving `AppDbContext` directly:

```csharp
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.AddDbContext<AppDbContext>(o =>
                o.UseSqlite("Data Source=testdb.sqlite"));
        });
    }
}

// Fixture
[OneTimeSetUp]
public async Task Init()
{
    _factory = new ApiFactory();
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedReferenceData(db);
}
```

Using `builder.ConfigureServices` instead of `ConfigureTestServices` would run before the app's own registration, so the app could re-register the SQL Server `DbContext` and override the test database. The `public partial class Program { }` declaration in the app project is required so the factory can reference it as the generic type argument.

---

#### Q9. (R) Review this handler-based test. It passes but leaks sockets in CI until the agent runs out of ephemeral ports.

```csharp
[Test]
public async Task FetchCatalog_parses_response()
{
    var handler = new Mock<HttpMessageHandler>();
    handler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"items\":[]}")
        });

    var client = new HttpClient(handler.Object)
    {
        BaseAddress = new Uri("https://catalog.internal/")
    };

    var service = new CatalogService(client);
    var items = await service.GetItemsAsync();

    Assert.That(items, Is.Empty);
}
```

**Concepts**
- HttpClient IDisposable socket and handler ownership
- `using var client` disposal per test
- Parallel test socket exhaustion pattern
- IHttpClientFactory for production lifetime management

**Answer**

`HttpClient` is `IDisposable` and owns the handler's connection pool — creating a new instance per test without disposing it leaves sockets in `TIME_WAIT` state until the GC finalizes them, which under parallel test runs exhausts the CI agent's ephemeral port range. The fix is a single line: `using var client = new HttpClient(handler.Object);`. The `HttpClient` disposes the handler when it is disposed, so no separate handler disposal is needed here. For shared state I would hoist the handler and client to a fixture-level field, disposing the client in `TearDown`. The deeper lesson is the same as `IHttpClientFactory` in production — handler lifetime must be managed explicitly, not left to garbage collection.
