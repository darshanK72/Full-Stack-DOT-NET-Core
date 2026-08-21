# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/14. API Testing & Integration Tests`

---

#### Q1. (R) Review this "unit test" for `PaymentClient`. CI is flaky and once charged a real sandbox because someone removed the `[Explicit]` attribute.

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

#### Q2. (R) Review this integration test setup. Tests pass locally but fail in CI with unique constraint violations and stale data from prior runs.

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

#### Q3. (R) Review these parallel integration tests. Roughly 10% of CI runs fail with "database is locked" or duplicate key errors; re-running the job usually passes.

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

#### Q4. (R) Review this integration test for a protected endpoint. The test fails with 401 Unauthorized even though the handler works in Swagger with a bearer token.

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

#### Q5. (R) Review this test class named `OrderServiceIntegrationTests`. The reviewer says it is not an integration test — agree or disagree, and what would you rename or restructure?

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

#### Q6. (P) Your team debates testing an `InventoryService` that calls an external REST API. One engineer wants `WebApplicationFactory` end-to-end; another wants a mocked `HttpMessageHandler` in a unit test. Map each choice to the **test pyramid** — what belongs at the base vs the top, and what would you run on every PR?

**Answer:** The **test pyramid** puts fast, isolated tests at the base and expensive full-stack tests at the top. **Mocked `HttpMessageHandler`** belongs at the base — verify `InventoryService` builds correct URLs, headers, and parses responses without network. **`WebApplicationFactory`** sits in the middle — verify your API's HTTP pipeline, auth, serialization, and that the service is registered and invoked correctly through a controller/minimal endpoint, still with the external API mocked at the handler. **True E2E** against the vendor sandbox is the thin top — scheduled, manual, or nightly, not every PR.

- **Every PR:** unit tests with mock handler; narrow integration tests (factory + test DB + mocked outbound HTTP).
- **WebApplicationFactory vs HttpMessageHandler:** factory tests **your host**; handler mock tests **your client code** — complementary, not either/or.
- **Debrief mapping:** "Test pyramid — WebApplicationFactory vs mocked HttpMessageHandler" = factory for **in-process API integration**, handler for **outbound HTTP unit isolation**.

**Production takeaway:** Run factory tests when middleware, binding, and DI matter; run handler mocks when algorithm and HTTP contract matter — do not pay factory startup cost for pure service logic.

---

#### Q7. (D) Compare **EF Core InMemory provider** vs **Testcontainers (SQL Server/PostgreSQL)** for API integration tests. When is InMemory acceptable, when does it lie about SQL semantics, and what is the operational cost of Testcontainers in CI?

**Answer:** **InMemory** is a lightweight fake — no SQL, no constraints, no translation quirks — good for testing repository logic that does not depend on provider behavior, bad for proving your LINQ translates correctly. **Testcontainers** spins real database engines in Docker — matches production semantics (constraints, transactions, raw SQL, concurrency) at the cost of slower startup and CI Docker requirement.

- **InMemory acceptable:** testing API layer mapping, auth, validation — when data access is mocked or behavior is trivial key-value.
- **InMemory lies:** FK constraints ignored, no `FromSql`, different transaction isolation, `Include`/translation bugs hidden, provider-specific types (`datetimeoffset`, JSON columns).
- **Testcontainers cost:** 10–60s fixture startup, Docker in CI, parallel job resource usage — mitigate with shared container per collection, reuse, or SQL LocalDB on Windows agents.
- **Recommendation:** Testcontainers (or dedicated test SQL instance) for EF integration tests that assert queries and migrations; InMemory only with eyes open on limitations.

**Production takeaway:** **Mocking vs real DB** — InMemory is a **fake**, not SQL; Testcontainers is the production-readiness choice for data access integration tests.

---

#### Q8. (M) Show the correct shape of a custom `WebApplicationFactory<Program>` that replaces `AppDbContext` with a test database and seeds reference data once per fixture. What goes in `ConfigureWebHost` vs `ConfigureServices`, and why does order matter?

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

#### Q9. (R) Review this handler-based test. It passes but leaks sockets in CI until the agent runs out of ephemeral ports.

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
