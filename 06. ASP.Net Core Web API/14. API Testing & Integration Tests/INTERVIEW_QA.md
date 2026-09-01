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

What is `WebApplicationFactory` in ASP.NET Core?

**Answer:** `WebApplicationFactory<TEntryPoint>` bootstraps the real application assembly in an in-memory test server, running the full middleware pipeline, routing, DI, and configuration without listening on a network port. Integration tests use it to send real HTTP requests against the app as deployed.

- Reference the API project's `Program` class (or expose it via `public partial class Program` for top-level statements).
- Subclass the factory to override `ConfigureWebHost` and replace services (database, auth, external HTTP clients).
- Tests live in a separate xUnit/NUnit project referencing `Microsoft.AspNetCore.Mvc.Testing`.
- Exercises Kestrel's `TestServer` host — closer to production behavior than mocking controllers in isolation.

---

## Q2. What is the difference between unit tests and integration tests for APIs?

What is the difference between unit tests and integration tests for APIs?

**Answer:** Unit tests isolate a class (service, validator) with mocked dependencies and no HTTP pipeline. Integration tests boot the application (or a slice of it) and verify behavior through HTTP requests, real middleware, serialization, and often a real or containerized database.

- Unit tests are fast and pinpoint logic failures; they do not catch routing, binding, or middleware misconfiguration.
- Integration tests catch issues like wrong status codes, auth pipeline gaps, and JSON contract mismatches.
- The test pyramid favors many unit tests, fewer integration tests, and minimal end-to-end tests against live external systems.
- Karat-style reviews often fail candidates who label live HTTP calls to sandbox APIs as "unit tests."

---

## Q3. What is an in-memory test server for Web APIs?

What is an in-memory test server for Web APIs?

**Answer:** The in-memory test server (`TestServer`) hosts the ASP.NET Core app inside the test process, accepting `HttpClient` requests that flow through the real middleware pipeline without opening a TCP port. `WebApplicationFactory` configures this server automatically.

- `factory.CreateClient()` returns an `HttpClient` whose `BaseAddress` points at the test server.
- Requests are in-process — no firewall or port conflicts in CI.
- Behavior matches production pipeline semantics (model binding, filters, auth) unlike direct controller instantiation.
- Not identical to production Kestrel (HTTP/2 edge cases, TLS) — supplement with deploy-environment smoke tests when needed.

---

## Q4. What does `CreateClient()` on WebApplicationFactory return?

What does `CreateClient()` on WebApplicationFactory return?

**Answer:** `CreateClient()` returns a preconfigured `HttpClient` wired to the factory's in-memory test server, with the application's base address and a handler that dispatches requests through the hosted pipeline. Tests call `GetAsync`, `PostAsJsonAsync`, etc., on this client.

- Optional `WebApplicationFactoryClientOptions` set base address, redirect handling, and cookie handling.
- The client shares the factory's service provider — service overrides in `ConfigureWebHost` apply to all requests from that client.
- Create one client per test or reuse from a fixture depending on isolation needs.
- Must dispose the factory (and client if created with `CreateDefaultClient` patterns that require it) to release host resources.

---

## Q5. What is the test pyramid for API development?

What is the test pyramid for API development?

**Answer:** The test pyramid recommends many fast unit tests at the base, a moderate layer of integration tests, and few slow end-to-end tests at the top. For Web APIs, unit tests cover services and validators; integration tests cover HTTP contracts; E2E tests cover critical flows against deployed environments.

- Unit: business rules, mapping, validation logic with mocked `DbContext` or repositories.
- Integration: `WebApplicationFactory` + test database for POST/GET status codes, ProblemDetails shape, auth.
- E2E: post-deploy smoke against staging with real dependencies — keep out of default CI when flaky or costly.
- Inverted pyramids (many Selenium/live-API tests, few unit tests) slow feedback and hide root causes.

---

## Q6. What is Mock `HttpMessageHandler` used for?

What is Mock `HttpMessageHandler` used for?

**Answer:** A mock `HttpMessageHandler` intercepts `HttpClient` outbound calls in tests, returning canned responses without hitting the network. Register it via `new HttpClient(mockHandler)` or `IHttpClientFactory` test configuration to isolate services that call external APIs.

- Subclass `HttpMessageHandler` and override `SendAsync` to assert request URL, headers, and body, then return `HttpResponseMessage` with test JSON.
- Libraries like Moq can mock `Protected()` `SendAsync` on the handler base class.
- Tests verify your service sends the correct payload to payment, identity, or notification APIs without sandbox charges.
- Distinguish this from integration tests — the API under test is real; only downstream HTTP is faked.

---

## Q7. Why should integration tests not use the production database?

Why should integration tests not use the production database?

**Answer:** Integration tests mutate data, seed fixtures, and may run in parallel — pointing at production or shared dev databases risks destroying real data, causing unique constraint collisions, and creating non-deterministic failures. Tests need isolated, disposable storage.

- Override `DbContext` registration in `ConfigureWebHost` with Testcontainers SQL, local SQLite, or a dedicated CI database.
- Never embed production connection strings in test projects or pipeline variables consumed by default `dotnet test`.
- Tests that leave debris break subsequent runs and other developers' local environments.
- Production-like data belongs in staging with controlled E2E suites, not automated integration test defaults.

---

## Q8. What is Testcontainers for API testing?

What is Testcontainers for API testing?

**Answer:** Testcontainers spins up real Docker containers (SQL Server, PostgreSQL, Redis) during test runs, giving integration tests authentic database behavior without a shared permanent instance. The container is created in fixture setup and destroyed after tests complete.

- NuGet packages like `Testcontainers.MsSql` integrate with xUnit `IAsyncLifetime` fixtures.
- Tests hit real SQL semantics (constraints, transactions, raw SQL) that EF InMemory does not emulate.
- CI agents must support Docker — GitHub Actions and Azure DevOps pipelines commonly do.
- Slower than InMemory but far more reliable for testing EF migrations, concurrency, and SQL-specific queries.

---

## Q9. What is the difference between EF InMemory and real SQL for API tests?

What is the difference between EF InMemory and real SQL for API tests?

**Answer:** EF Core InMemory provider stores data in a process-local dictionary — fast and simple but ignores SQL semantics like foreign keys, unique constraints, transactions, and raw SQL. Real SQL (Testcontainers or local instance) validates what production actually enforces.

- InMemory suits tests focused purely on HTTP routing and serialization with trivial persistence.
- Migration application, concurrency tokens, and `FromSqlRaw` require a relational provider.
- InMemory provider name changed in EF Core 8 — use `UseInMemoryDatabase` knowing limitations are documented by Microsoft.
- Many teams use InMemory for fast controller tests and SQL containers for repository or full-stack integration tests.

---

## Q10. What is `ConfigureWebHost` in WebApplicationFactory?

What is `ConfigureWebHost` in WebApplicationFactory?

**Answer:** Override `ConfigureWebHost(IWebHostBuilder builder)` in a custom `WebApplicationFactory` subclass to change the test host configuration — swap connection strings, register test auth handlers, replace `HttpClient` handlers, or set `Environment` to "Testing".

- Call `builder.ConfigureTestServices(services => { ... })` to replace or decorate DI registrations after the app's `Program.cs` runs.
- Use `builder.UseEnvironment("Testing")` to trigger test-specific `appsettings.Testing.json` overrides.
- Typical pattern: remove `DbContext` SQL registration, add InMemory or Testcontainers connection.
- Runs before the test server starts — all requests through `CreateClient()` see the overridden services.

---

## Q11. How do you test authenticated API endpoints?

How do you test authenticated API endpoints?

**Answer:** Integration tests must satisfy the same authentication middleware the app uses — attach a valid JWT in `Authorization: Bearer`, register a test authentication handler that auto-succeeds, or use `ConfigureTestServices` to replace `AddAuthentication` with a scheme that injects claims.

- `client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testToken)` when using real JWT validation with a test signing key.
- `services.AddAuthentication("Test").AddScheme<..., TestAuthHandler>("Test", ...)` bypasses token crypto in focused pipeline tests.
- Set `[Authorize]` policies by adding required claims in the test handler's `ClaimsIdentity`.
- Testing anonymous vs authenticated vs forbidden (403) paths requires separate requests with different identities.

---

## Q12. What is `PostAsJsonAsync` in integration tests?

What is `PostAsJsonAsync` in integration tests?

**Answer:** `PostAsJsonAsync` is an `HttpClient` extension (from `System.Net.Http.Json`) that serializes a CLR object to JSON, sets `Content-Type: application/json`, and POSTs to the given URL. Integration tests use it to exercise model binding and validation on API endpoints.

- Pair with `ReadFromJsonAsync<T>()` to deserialize response DTOs and assert properties.
- Uses `System.Text.Json` defaults — camelCase property names match ASP.NET Core 8 API conventions.
- Assert `response.StatusCode`, `response.Headers.Location`, and ProblemDetails on validation failures.
- Available on `HttpClient` returned from `WebApplicationFactory.CreateClient()`.

---

## Q13. What causes flaky parallel integration tests?

What causes flaky parallel integration tests?

**Answer:** Parallel tests that share one database file, one Testcontainers instance without isolation, or mutable static seed data race on writes and locks — producing intermittent duplicate key errors, "database is locked," or order-dependent pass/fail. Fix isolation or disable parallelism for shared-state collections.

- xUnit runs test classes in parallel by default — `[Collection("Database")]` serializes tests sharing a fixture.
- Static `WebApplicationFactory` fields with shared InMemory database name collide across classes.
- Use unique database names per test (`Guid.NewGuid()` suffix) or one container per collection fixture.
- Flaky retries mask design problems — prefer deterministic isolation over `[Retry]` attributes.

---

## Q14. What is the difference between mocking a service vs mocking HttpClient?

What is the difference between mocking a service vs mocking HttpClient?

**Answer:** Mocking a service (e.g., `IPaymentService`) replaces an internal DI dependency — the HTTP pipeline, routing, and controllers still run. Mocking `HttpMessageHandler` replaces outbound HTTP from a typed client while the real service code executes and calls the fake handler.

- Service mock: test controller or filter behavior when payment always succeeds without running `PaymentService` logic.
- Handler mock: test `PaymentService` builds correct HTTP requests and parses responses; pipeline includes real service registration.
- Choose the boundary under test — mock at the edge closest to what you want to verify.
- Over-mocking services in integration tests reduces the test to a unit test with extra steps.

---

## Q15. What is a test fixture for API integration tests?

What is a test fixture for API integration tests?

**Answer:** A test fixture (xUnit `IClassFixture<T>` or NUnit `OneTimeSetUp`) creates expensive shared resources once per test class or collection — `WebApplicationFactory`, Testcontainers database, seeded data — and disposes them after tests finish. It amortizes startup cost while controlling isolation boundaries.

- `public class OrderTests : IClassFixture<CustomWebApplicationFactory>` receives the factory via constructor injection.
- `IAsyncLifetime` fixtures start Docker containers asynchronously before any test runs.
- Collection fixtures share one factory across multiple test classes that must not run in parallel.
- Fixture scope defines how much state tests share — broader scope means stronger isolation requirements.

---

## Q16. What is seed data in API integration tests?

What is seed data in API integration tests?

**Answer:** Seed data pre-populates the test database with known entities before assertions run — users, products, orders — so tests start from a predictable state. Apply seeds in fixture setup after migrations, or insert via `DbContext` in each test's arrange phase.

- Enables `GET /api/orders/1` to return a known row without depending on another test's POST.
- Reset strategy: recreate database per test, truncate tables, or use transactions rolled back after each test.
- Avoid hard-coded ids that assume identity seed values — capture ids from seed inserts or use well-known GUIDs.
- Seed only what the test needs to keep arrange sections readable and fast.

---

## Q17. What is the difference between testing controllers directly vs testing via HTTP?

What is the difference between testing controllers directly vs testing via HTTP?

**Answer:** Direct controller testing instantiates the controller with mocked dependencies and calls action methods — skipping routing, model binding, filters, and middleware. HTTP testing via `WebApplicationFactory` sends real requests through the full pipeline, catching binding source mistakes and auth gaps.

- Direct: fast, good for branching logic inside actions when pipeline is tested elsewhere.
- HTTP: validates `[ApiController]` automatic 400 responses, `[FromBody]` binding, route templates, and content negotiation.
- Direct tests require manual setup of `ControllerContext`, `ModelState`, and `HttpContext`.
- ASP.NET Core 8 interview and production guidance favors HTTP integration tests for contract verification.

---

## Q18. Why must HttpClient instances be disposed properly in tests?

Why must HttpClient instances be disposed properly in tests?

**Answer:** `HttpClient` and `WebApplicationFactory` hold sockets, `TestServer` hosts, and connection pool resources. Undisposed clients and factories leak handles in long CI runs, causing port exhaustion, socket starvation, and slow or failing subsequent test assemblies.

- Dispose `WebApplicationFactory` in fixture teardown (`Dispose()` or `IAsyncDisposable`).
- `CreateClient()` clients are typically short-lived per test; factory manages the underlying handler pool when reused.
- Do not create a new `HttpClient` per test without disposal when using custom handlers outside the factory pattern.
- Testcontainers and factory disposal order: stop container, dispose factory, release clients.

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

---

**Answer:**

**Answer:** This test is an **integration test against a live network** — it is slow, flaky, requires secrets, and can cause real side effects (charges). External HTTP must be stubbed at the `HttpMessageHandler` boundary or replaced with `IHttpClientFactory` + test handler, not hit production or sandbox URLs in automated runs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Test type | Real `HttpClient` to payment API | Flaky CI; accidental real charges |
| Isolation | No mock/fake of HTTP boundary | Depends on network, credentials, provider uptime |
| Naming | Called unit test | Wrong pyramid placement — hides cost |

**Fix (priority order):**

1. Inject `HttpMessageHandler` (mock `SendAsync`) or use `MockHttpMessageHandler` / WireMock.NET — assert request shape and return canned JSON.
2. Register named client in tests via `IHttpClientFactory` with handler replacement in `ConfigureTestServices`.
3. Gate true end-to-end sandbox tests behind `[Explicit]` or a separate pipeline stage with secrets — never default `dotnet test`.
4. Rename test to reflect layer: `Charge_sends_expected_payload` with mocked handler.

**Production takeaway:** **Unit test service calling external API without real HTTP** — debrief expectation is handler-level isolation, not "hope sandbox is up." See C# Module 09 — stub vs mock.

---

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

---

**Answer:**

**Answer:** `WebApplicationFactory` boots the **real application** with **real registrations** unless overridden — pointing at the shared dev SQL database causes tests to **mutate shared state** and observe order-dependent results. Integration tests need an isolated database per run or per fixture.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Database | No `ConfigureTestServices` override for `DbContext` | Tests pollute dev DB; CI collisions |
| Isolation | Shared factory without seed/reset strategy | `Get_orders` depends on `Post_order` execution order |
| Lifecycle | New factory field but default config | Same connection string as development |

**Fix (priority order):**

1. Subclass `WebApplicationFactory<Program>` — override `ConfigureWebHost` to replace `UseSqlServer` with Testcontainers SQL, local SQLite file per fixture, or EF InMemory **only** if SQL semantics not under test.
2. Apply migrations + seed in fixture `OneTimeSetUp`; truncate or recreate schema between tests if needed.
3. Use `[Collection("Database")]` or disable parallelization for tests sharing one container.
4. Never use shared dev/production connection strings in automated tests.

**Production takeaway:** **WebApplicationFactory** tests the real pipeline — the factory must also **replace external state** (DB, auth, message bus), not only call `CreateClient()`.

---

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

---

**Answer:**

**Answer:** Parallel tests against a **single shared SQLite file** (or one database without isolation) race on writes and schema locks — failures are intermittent, which matches flaky CI. Either isolate storage per test or serialize access.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Parallelism | `[Parallelizable(ParallelScope.All)]` + shared DB file | Lock contention; duplicate key races |
| Static factory | One `WebApplicationFactory` for all tests | Shared singleton services and connection |
| Data | Tests assume exclusive DB | Non-deterministic pass/fail |

**Fix (priority order):**

1. Disable parallel scope for DB tests: `[NonParallelizable]` or collection fixture with single worker.
2. Better: unique database per test — `Guid` suffix connection string, Testcontainers per collection, or `:memory:` SQLite **per factory instance** (note: shared in-memory needs separate connection strings).
3. Avoid static mutable seed data tests running concurrently with creators.
4. Track flaky tests — parallel DB tests without isolation are a design bug, not "CI noise."

**Production takeaway:** **Flaky parallel tests** with EF almost always mean **shared mutable database** — fix isolation before retry policies.

---

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

---

**Answer:**

**Answer:** Integration tests must **authenticate the test client** the same way the app expects — JWT bearer validation runs in the real pipeline. `CreateClient()` sends no token unless you configure a test auth handler or attach `Authorization` headers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Auth | No bearer token on `HttpClient` | 401 before action runs — test fails for wrong reason |
| Factory | Default factory uses production auth config | Requires test issuer/keys or handler swap |
| Assertion | Deserializes body on 401 | Secondary failures obscure status check |

**Fix (priority order):**

1. Replace authentication in tests: `services.AddAuthentication("Test").AddScheme<..., TestAuthHandler>(...)` in `ConfigureTestServices`.
2. Or generate valid JWT with test signing key matching `ConfigureTestServices` override of `JwtBearerOptions`.
3. Helper: `client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testToken);`
4. Assert `response.StatusCode` before reading body; use `[Authorize]` policy names in test token claims.

**Production takeaway:** **Auth in integration tests** is part of the system under test — use test scheme or real token factory, not `[AllowAnonymous]` on production controllers.

---

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

---

**Answer:**

**Answer:** Agree — this is a **unit test** of `OrderService` with a **mocked repository**. No ASP.NET pipeline, database, or HTTP participates. Naming it integration misplaces it on the test pyramid and invites false confidence about end-to-end behavior.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Classification | Mocks `IOrderRepository` — all IO faked | Tests logic only, not wiring |
| Naming | `IntegrationTests` suffix | Team runs wrong suite expectations |
| Coverage gap | Real failures (DI, mapping, auth) untested | Production bugs in composition root |

**Fix (priority order):**

1. Rename to `OrderServiceTests` — move to unit test project/folder.
2. Add true integration test: `WebApplicationFactory` + `POST /api/orders` + test DB assert row exists.
3. Keep this mock-based test for fast feedback on business rules — base of pyramid.
4. Document pyramid: many unit, fewer integration, few E2E.

**Production takeaway:** **Mocking vs real DB** — mocks test **collaboration contracts**; integration tests prove **composition and persistence** work together.

---

---

#### Q6. (P) Your team debates testing an `InventoryService` that calls an external REST API. One engineer wants `WebApplicationFactory` end-to-end; another wants a mocked `HttpMessageHandler` in a unit test. Map each choice to the **test pyramid** — what belongs at the base vs the top, and what would you run on every PR?

---

**Answer:**

**Answer:** The **test pyramid** puts fast, isolated tests at the base and expensive full-stack tests at the top. **Mocked `HttpMessageHandler`** belongs at the base — verify `InventoryService` builds correct URLs, headers, and parses responses without network. **`WebApplicationFactory`** sits in the middle — verify your API's HTTP pipeline, auth, serialization, and that the service is registered and invoked correctly through a controller/minimal endpoint, still with the external API mocked at the handler. **True E2E** against the vendor sandbox is the thin top — scheduled, manual, or nightly, not every PR.

- **Every PR:** unit tests with mock handler; narrow integration tests (factory + test DB + mocked outbound HTTP).
- **WebApplicationFactory vs HttpMessageHandler:** factory tests **your host**; handler mock tests **your client code** — complementary, not either/or.
- **Debrief mapping:** "Test pyramid — WebApplicationFactory vs mocked HttpMessageHandler" = factory for **in-process API integration**, handler for **outbound HTTP unit isolation**.

**Production takeaway:** Run factory tests when middleware, binding, and DI matter; run handler mocks when algorithm and HTTP contract matter — do not pay factory startup cost for pure service logic.

---

---

#### Q7. (D) Compare **EF Core InMemory provider** vs **Testcontainers (SQL Server/PostgreSQL)** for API integration tests. When is InMemory acceptable, when does it lie about SQL semantics, and what is the operational cost of Testcontainers in CI?

---

**Answer:**

**Answer:** **InMemory** is a lightweight fake — no SQL, no constraints, no translation quirks — good for testing repository logic that does not depend on provider behavior, bad for proving your LINQ translates correctly. **Testcontainers** spins real database engines in Docker — matches production semantics (constraints, transactions, raw SQL, concurrency) at the cost of slower startup and CI Docker requirement.

- **InMemory acceptable:** testing API layer mapping, auth, validation — when data access is mocked or behavior is trivial key-value.
- **InMemory lies:** FK constraints ignored, no `FromSql`, different transaction isolation, `Include`/translation bugs hidden, provider-specific types (`datetimeoffset`, JSON columns).
- **Testcontainers cost:** 10–60s fixture startup, Docker in CI, parallel job resource usage — mitigate with shared container per collection, reuse, or SQL LocalDB on Windows agents.
- **Recommendation:** Testcontainers (or dedicated test SQL instance) for EF integration tests that assert queries and migrations; InMemory only with eyes open on limitations.

**Production takeaway:** **Mocking vs real DB** — InMemory is a **fake**, not SQL; Testcontainers is the production-readiness choice for data access integration tests.

---

---

#### Q8. (M) Show the correct shape of a custom `WebApplicationFactory<Program>` that replaces `AppDbContext` with a test database and seeds reference data once per fixture. What goes in `ConfigureWebHost` vs `ConfigureServices`, and why does order matter?

---

**Answer:**

**Answer:** Subclass `WebApplicationFactory<Program>`, override `ConfigureWebHost`, set environment to `Testing`, and use `ConfigureTestServices` to **replace** `DbContext` registration after the app's `Program` runs — then seed via fixture setup using a scope from `factory.Services`.

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

- **`ConfigureWebHost`:** environment, config overrides, `ConfigureTestServices` for DI replacements.
- **`ConfigureServices` on builder** runs earlier — app `Program` may re-register; prefer `ConfigureTestServices` to override after app registration.
- **`Program` visibility:** expose `public partial class Program { }` for factory generic argument.

**Production takeaway:** Factory customization replaces **registrations**, not duplicate `Program.cs` — seed through `Services.CreateScope()` after host builds.

---

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

---

**Answer:**

**Answer:** `HttpClient` is ** IDisposable** and owns the handler's connection pool — creating a new client per test without disposal leaks sockets until GC finalizes. Tests must `using var client` or dispose in `[TearDown]`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | `HttpClient` not disposed | Socket exhaustion in CI |
| Handler | `Mock<HttpMessageHandler>` without `Dispose` delegation | Minor — client disposal is main fix |
| Pattern | New client per test without factory | Multiplies sockets under parallel runs |

**Fix (priority order):**

1. `using var client = new HttpClient(handler.Object);` — or reuse static `HttpClient` per handler instance (handler lifetime matches tests).
2. Prefer `IHttpClientFactory` in production; in tests, one client per fixture with shared handler.
3. Call `handler.Verify()` then dispose; do not create unbounded clients in loops.
4. Monitor CI for `TIME_WAIT` / port exhaustion — classic HttpClient test smell.

**Production takeaway:** Same rule as production **`IHttpClientFactory`** — manage **handler lifetime**; in tests, dispose clients explicitly.

---

---
