# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/14. API Testing & Integration Tests`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [02. C# Language Fundamentals/09. Unit Testing/04. Mocking & Test Doubles](../../02.%20C%23%20Language%20Fundamentals/09.%20Unit%20Testing/04.%20Mocking%20&%20Test%20Doubles/KARAT_INTERVIEW_QUESTIONS.md) (stub vs fake vs mock)

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

#### Q6. (P) Your team debates testing an `InventoryService` that calls an external REST API. One engineer wants `WebApplicationFactory` end-to-end; another wants a mocked `HttpMessageHandler` in a unit test. Map each choice to the **test pyramid** — what belongs at the base vs the top, and what would you run on every PR?

---

#### Q7. (D) Compare **EF Core InMemory provider** vs **Testcontainers (SQL Server/PostgreSQL)** for API integration tests. When is InMemory acceptable, when does it lie about SQL semantics, and what is the operational cost of Testcontainers in CI?

---

#### Q8. (M) Show the correct shape of a custom `WebApplicationFactory<Program>` that replaces `AppDbContext` with a test database and seeds reference data once per fixture. What goes in `ConfigureWebHost` vs `ConfigureServices`, and why does order matter?

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
