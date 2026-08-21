# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/04. Dependency Injection & Service Lifetimes`

---

#### Q1. (M) You register two payment gateways: `AddSingleton<IPaymentProcessor, StripeProcessor>()` and `AddSingleton<IPaymentProcessor, PayPalProcessor>()`. Injecting `IPaymentProcessor` resolves one implementation; injecting `IEnumerable<IPaymentProcessor>` resolves both. How does the container behave, and when does single-interface injection silently surprise you?

**Answer:** The built-in container stores multiple descriptors for the same service type; resolving `IPaymentProcessor` returns the **last** registration, while `IEnumerable<IPaymentProcessor>` injects a composite enumerating all registrations in registration order.

- Duplicate `AddSingleton<IPaymentProcessor, T>()` calls append descriptors — not overwrite.
- Single-interface injection is **last-wins** — PayPal would win in the example unless registration order is intentional.
- `IEnumerable<IPaymentProcessor>` activates all implementations — use for plugin fan-out, composite health checks, or strategy chains.
- Alternatives when you need named impls: **keyed services** (.NET 8+), factory delegates, or separate interfaces (`IStripeProcessor`, `IPayPalProcessor`).
- Decorator pattern: register decorator as the single `TInterface` and inject inner as concrete or keyed type to avoid ambiguity.

**Production takeaway:** Multi-provider designs fail silently when developers inject `TInterface` expecting "all gateways" — Karat tests `IEnumerable<T>` vs single `T` from debrief.

---

#### Q2. (M) How do `ValidateScopes` and `ValidateOnBuild` on `UseDefaultServiceProvider()` catch captive dependencies and missing registrations at startup rather than under load?

**Answer:** `ValidateOnBuild` resolves the entire service graph at startup to surface missing registrations; `ValidateScopes` throws when a singleton (or root scope) resolves a scoped service directly — exposing captive dependencies like scoped `DbContext` in a singleton hosted service.

- Configure on the host builder:

```csharp
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
```

- `ValidateOnBuild` — first chance to catch `Unable to resolve service for type 'IOptions<SmtpOptions>'` before traffic.
- `ValidateScopes` — catches singleton constructor injecting scoped `AppDbContext` at root provider creation.
- Does not replace fixing architecture — still use `IServiceScopeFactory` per operation in background workers.
- Enable in CI/staging — small startup cost, large production save.

**Production takeaway:** Apps that "start fine" locally but fail under load often skipped these flags — debrief favorite for captive dependency detection.

---

#### Q3. (R) In an e-commerce app, review this cart service registered as singleton. What production failures appear under concurrent shoppers and multi-instance deployment?

**Answer:** A singleton `CartService` with an instance `_items` list shares one cart across every user and request — cross-user leakage, race corruption, and inconsistent carts when scaled to multiple instances.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `AddSingleton` + mutable `_items` field | All users share one cart — privacy and billing incidents |
| Concurrency | `List<T>` mutated without synchronization | Corrupted collection or lost items under load |
| Scale-out | In-memory state is per-process | Cart differs per pod; sticky sessions fail |
| Design | No user/session key on cart data | Cannot distinguish shoppers |

**Fix (priority order):**

1. Register **scoped** `ICartService` keyed by request + user identity, or remove in-memory cart from singleton entirely.
2. Persist cart lines in Redis/SQL with `userId` / session id for multi-instance deployments.
3. Keep singleton services stateless — pass identity into scoped services that load/store cart data.

```csharp
builder.Services.AddScoped<ICartService, CartService>(); // + user id in service API
// or distributed: ICartStore backed by Redis, singleton orchestrator only
```

**Production takeaway:** E-commerce lifetime mistakes are severity-1 — wrong charges and privacy incidents, not benign test noise.

---

#### Q4. (R) A scoped `DbContext` is constructor-injected into a singleton `BackgroundService` that processes a queue. The app starts but throws `ObjectDisposedException` under load. Explain the captive dependency and fix it.

**Answer:** The singleton outlives any request scope; the scoped `DbContext` is created once at root scope and disposed when that scope ends — the worker holds a disposed context (captive dependency), causing intermittent `ObjectDisposedException` on queue processing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Scoped `DbContext` in singleton `BackgroundService` | Context disposed while worker runs |
| Runtime | `ObjectDisposedException` on deferred saves | Poison messages, data loss |
| Design | One context shared across unrelated orders | Stale change tracker, wrong commits |
| Observability | May start without `ValidateScopes` in dev | Bug reaches prod under timing |

**Fix (priority order):**

1. Inject `IServiceScopeFactory` (or `IDbContextFactory<AppDbContext>`) instead of `DbContext`.
2. Create **new scope per message** (or batch), resolve fresh context, dispose scope when done.
3. Enable `ValidateScopes` + `ValidateOnBuild` in staging.

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
_db.Orders.Add(order);
await db.SaveChangesAsync(stoppingToken);
```

**Production takeaway:** Background workers own scope boundaries — request-scoped services never belong in singleton constructors (debrief captive dependency).

---

#### Q5. (D) A team caches per-user preferences in `static Dictionary<string, UserPrefs>` inside a singleton service. What breaks under concurrent load, parallel tests, and horizontal scale-out?

**Answer:** Process-wide static mutable state is not thread-safe by default, not shared across instances, and survives across tests in the same process — causing corruption, stale prefs, cross-test pollution, and inconsistent reads after scale-out.

- **Concurrency:** `Dictionary<,>` throws or corrupts under concurrent writes — use `ConcurrentDictionary` or external store, but static still wrong for multi-instance.
- **Scale-out:** Each replica has its own static cache — update on node A invisible on node B until external sync.
- **Parallel tests:** xUnit parallel classes share static — flaky tests unless isolated collections or no static state.
- **Memory:** Unbounded keys → OOM — no eviction tied to session lifetime.
- **Privacy:** Key collision or missing tenant prefix → cross-user preference bleed.

**Recommended:** `IMemoryCache` with tenant-scoped keys + TTL, or Redis/SQL per user — resolve via scoped service per request.

**Production takeaway:** Static mutable caches in singletons are debrief Heisenbugs — appear under parallel tests or second replica, not solo dev runs.

---

#### Q6. (P) .NET 8 keyed services: you need `"primary"` and `"fallback"` implementations of `INotificationSender` in the same process. How do you register and resolve them without ambiguous `INotificationSender` injection?

**Answer:** Use keyed DI — register with `AddKeyedSingleton<INotificationSender, EmailSender>("primary")` and resolve via `[FromKeyedServices("primary")]`, `GetRequiredKeyedService`, or keyed factory delegate — avoids last-wins single registration.

```csharp
builder.Services.AddKeyedSingleton<INotificationSender, EmailSender>("primary");
builder.Services.AddKeyedSingleton<INotificationSender, SmsSender>("fallback");

public class AlertService(
    [FromKeyedServices("primary")] INotificationSender primary,
    [FromKeyedServices("fallback")] INotificationSender fallback) { }
```

- Unkeyed `INotificationSender` injection remains ambiguous if multiple unkeyed registrations exist — prefer keyed only or single default + keyed alternates.
- Keyed services work with singleton/scoped/transient lifetimes per key.
- Alternative pre-.NET 8: separate interfaces, factory pattern, or `IServiceProvider` locator (discouraged).

**Production takeaway:** Keyed services replace hacky `IEnumerable<T>` indexing when you need **named implementations**, not all implementations.

---

#### Q7. (R) Review this outbound HTTP integration. Memory usage climbs until OOM and DNS changes never apply. What is wrong and what replaces `new HttpClient()`?

**Answer:** Instantiating `HttpClient` per call exhausts ephemeral ports and socket handles under load and ignores DNS TTL — register a typed client with `IHttpClientFactory` (or `AddHttpClient`) so handlers and connections are pooled and DNS refreshes apply.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource | `new HttpClient()` per request in scoped service | Socket exhaustion, SNAT port fatigue |
| DNS | Disposed client pattern still recreated each call | Stale DNS after failover |
| DI | Should use typed `HttpClient` from factory | Missing Polly, logging, correlation handlers |
| Performance | TLS handshake repeated | Latency and CPU spike on catalog pages |

**Fix (priority order):**

1. Register typed client: `builder.Services.AddHttpClient<IPricingService, PricingService>(c => c.BaseAddress = new Uri("..."));`
2. Inject `HttpClient` via constructor on `PricingService` — do not dispose injected client.
3. Add resilience handler (timeout, retry) via Polly integration if needed.

```csharp
public class PricingService(HttpClient http) : IPricingService
{
    public async Task<decimal> GetPriceAsync(int sku, CancellationToken ct)
    {
        var json = await http.GetStringAsync($"/api/prices/{sku}", ct);
        return JsonSerializer.Deserialize<PriceDto>(json)!.Amount;
    }
}
```

**Production takeaway:** `new HttpClient()` in ASP.NET Core services is a classic Karat review — factory is mandatory for production outbound calls.

---

#### Q8. (P) When should you inject `IServiceScopeFactory` vs `IDbContextFactory<AppDbContext>` vs a custom `Func<IServiceProvider, T>` factory? Compare a nightly batch job vs per-message queue worker vs on-demand controller action.

**Answer:** Controllers use natural request scope for scoped services; long-running singleton workers use `IServiceScopeFactory` to create scopes per unit of work; high-throughput DbContext creation uses `IDbContextFactory` for lightweight context spin-up without full scope overhead.

| Scenario | Tool | Why |
|---|---|---|
| Controller action | Inject scoped `AppDbContext` / services directly | Request scope matches HTTP lifetime |
| Per-message queue worker | `IServiceScopeFactory.CreateAsyncScope()` | Full scoped graph per message |
| Nightly batch touching many aggregates | `IServiceScopeFactory` per batch/chunk | Dispose contexts between batches |
| High-frequency short DB reads | `IDbContextFactory<AppDbContext>` | Pool-friendly context creation |
| Complex optional dependency | Custom factory delegate registration | Encapsulate creation logic |

- `IServiceScopeFactory` resolves **any** scoped service graph — use when worker needs repository + context + unit of work together.
- `IDbContextFactory` when only EF context needed with explicit `await using var ctx = await factory.CreateDbContextAsync()`.
- Avoid `Func<IServiceProvider,T>` unless simplifying tests or multi-implementation selection — prefer typed factories.

**Production takeaway:** Factory choice is about **scope boundary granularity**, not preference — wrong choice revives captive dependency bugs.

---

#### Q9. (R) Review DI registration for a decorator-style audit logger. Only `ConsoleAuditLogger` ever runs; `FileAuditLogger` is never invoked. What registration mistake caused this?

**Answer:** Two `AddSingleton<IAuditLogger, ...>()` registrations make single `IAuditLogger` injection **last-wins** — only `ConsoleAuditLogger` resolves; `FileAuditLogger` is orphaned unless you inject `IEnumerable<IAuditLogger>` or use a composite/decorator registration.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | Duplicate interface registration — last wins | File audit never runs — compliance gap |
| Design | Expected decorator chain but registered peer implementations | Silent loss of audit sink |
| Observability | Console shows logs; file empty in prod | Incident reconstruction fails |
| Architecture | Misunderstanding of container multi-registration rules | Repeat across services |

**Fix (priority order):**

1. **Composite:** Register `IAuditLogger` as `CompositeAuditLogger(IEnumerable<IAuditLogger> loggers)` after registering concretes as themselves or keyed.
2. **Decorator:** `services.AddSingleton<IAuditLogger, FileAuditLogger>(); services.Decorate<IAuditLogger, ConsoleAuditLogger>();` (with Scrutor or manual factory).
3. **Explicit:** Inject `IEnumerable<IAuditLogger>` in `OrderService` if fan-out intended.

```csharp
builder.Services.AddSingleton<FileAuditLogger>();
builder.Services.AddSingleton<ConsoleAuditLogger>();
builder.Services.AddSingleton<IAuditLogger>(sp =>
    new CompositeAuditLogger(new IAuditLogger[]
    {
        sp.GetRequiredService<FileAuditLogger>(),
        sp.GetRequiredService<ConsoleAuditLogger>()
    }));
```

**Production takeaway:** Multiple `Add*` for same interface does not create a chain — debrief `IEnumerable<T>` vs single `T` applied to audit logging.

---

#### Q10. (R) Review lifetimes for a read-heavy catalog API. Intermittent wrong prices and stale inventory appear after deploy. Identify lifetime mismatches.

**Answer:** `CatalogService` is singleton but depends on scoped `IProductRepository` (captive dependency) and holds a process-wide mutable cache — stale/wrong data and disposed-context races under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Singleton `CatalogService` → scoped `IProductRepository` | Captive `DbContext`; disposed or stale tracker |
| State | Singleton `CatalogCache` with mutable `Dictionary` | Never evicts; wrong prices after updates |
| Concurrency | Unsynchronized dictionary writes | Corruption under parallel requests |
| Scale-out | In-memory cache per instance | Inconsistent catalog across pods |

**Fix (priority order):**

1. Make `ICatalogService` **scoped** (or singleton with `IDbContextFactory` + no scoped repo injection).
2. Replace static dictionary with `IMemoryCache` (TTL) or distributed cache for shared invalidation.
3. Enable `ValidateScopes` to catch singleton→scoped at startup.

```csharp
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// Remove singleton CatalogService + captive cache field pattern
```

**Production takeaway:** Read-heavy APIs often "optimize" with singleton caches and create **lifetime + staleness** bugs — Karat stacks DI and caching defects in one snippet.

---
