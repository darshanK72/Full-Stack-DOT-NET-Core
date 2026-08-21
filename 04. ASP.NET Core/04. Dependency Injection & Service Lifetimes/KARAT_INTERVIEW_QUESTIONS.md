# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/04. Dependency Injection & Service Lifetimes`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (M) You register two payment gateways: `AddSingleton<IPaymentProcessor, StripeProcessor>()` and `AddSingleton<IPaymentProcessor, PayPalProcessor>()`. Injecting `IPaymentProcessor` resolves one implementation; injecting `IEnumerable<IPaymentProcessor>` resolves both. How does the container behave, and when does single-interface injection silently surprise you?

---

#### Q2. (M) How do `ValidateScopes` and `ValidateOnBuild` on `UseDefaultServiceProvider()` catch captive dependencies and missing registrations at startup rather than under load?

---

#### Q3. (R) In an e-commerce app, review this cart service registered as singleton. What production failures appear under concurrent shoppers and multi-instance deployment?

```csharp
builder.Services.AddSingleton<ICartService, CartService>();

public class CartService : ICartService
{
    private readonly List<CartItem> _items = new();
    public void AddItem(CartItem item) => _items.Add(item);
    public IReadOnlyList<CartItem> GetItems() => _items;
}
```

---

#### Q4. (R) A scoped `DbContext` is constructor-injected into a singleton `BackgroundService` that processes a queue. The app starts but throws `ObjectDisposedException` under load. Explain the captive dependency and fix it.

```csharp
public class OrderQueueWorker : BackgroundService
{
    private readonly AppDbContext _db;
    public OrderQueueWorker(AppDbContext db) => _db = db;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var order = await DequeueAsync();
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(stoppingToken);
        }
    }
}
```

---

#### Q5. (D) A team caches per-user preferences in `static Dictionary<string, UserPrefs>` inside a singleton service. What breaks under concurrent load, parallel tests, and horizontal scale-out?

---

#### Q6. (P) .NET 8 keyed services: you need `"primary"` and `"fallback"` implementations of `INotificationSender` in the same process. How do you register and resolve them without ambiguous `INotificationSender` injection?

---

#### Q7. (R) Review this outbound HTTP integration. Memory usage climbs until OOM and DNS changes never apply. What is wrong and what replaces `new HttpClient()`?

```csharp
public class PricingService : IPricingService
{
    public async Task<decimal> GetPriceAsync(int sku, CancellationToken ct)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://pricing.internal");
        var json = await client.GetStringAsync($"/api/prices/{sku}", ct);
        return JsonSerializer.Deserialize<PriceDto>(json)!.Amount;
    }
}
```

*(Registered as `AddScoped<IPricingService, PricingService>()` — called on every product page view.)*

---

#### Q8. (P) When should you inject `IServiceScopeFactory` vs `IDbContextFactory<AppDbContext>` vs a custom `Func<IServiceProvider, T>` factory? Compare a nightly batch job vs per-message queue worker vs on-demand controller action.

---

#### Q9. (R) Review DI registration for a decorator-style audit logger. Only `ConsoleAuditLogger` ever runs; `FileAuditLogger` is never invoked. What registration mistake caused this?

```csharp
builder.Services.AddSingleton<IAuditLogger, FileAuditLogger>();
builder.Services.AddSingleton<IAuditLogger, ConsoleAuditLogger>();
builder.Services.AddSingleton<IOrderService, OrderService>();

public class OrderService(IOrderRepository repo, IAuditLogger audit) : IOrderService
{
    public async Task PlaceOrderAsync(Order order)
    {
        await repo.AddAsync(order);
        audit.Log($"Placed {order.Id}");
    }
}
```

---

#### Q10. (R) Review lifetimes for a read-heavy catalog API. Intermittent wrong prices and stale inventory appear after deploy. Identify lifetime mismatches.

```csharp
builder.Services.AddSingleton<CatalogCache>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();

public class CatalogService(CatalogCache cache, IProductRepository repo) : ICatalogService
{
    public async Task<Product?> GetAsync(int id) =>
        cache.Get(id) ?? cache.Store(id, await repo.GetByIdAsync(id));
}

public class CatalogCache
{
    private readonly Dictionary<int, Product> _items = new();
    public Product? Get(int id) => _items.GetValueOrDefault(id);
    public Product Store(int id, Product p) => _items[id] = p;
}
```

*( `ProductRepository` holds scoped `DbContext`; `CatalogService` is singleton. )*
