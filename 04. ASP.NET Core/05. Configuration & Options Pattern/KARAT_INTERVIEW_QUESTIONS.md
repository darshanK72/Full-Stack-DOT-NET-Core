# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/05. Configuration & Options Pattern`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (M) A service reads `configuration["Payment:ApiKey"]` at startup and caches it in a field. Ops rotates the key via environment variable override in Kubernetes without redeploying, but the app keeps using the old key. Explain how `IConfiguration` providers and precedence work, and why this pattern fails for hot reload.

---

#### Q2. (M) Three consumers need settings from the same `appsettings.json` section: a singleton cache warmer, an MVC controller, and a background `IHostedService` that must react when `ReloadOnChange` updates the file. Which of `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` belongs in each, and what breaks if you swap them?

---

#### Q3. (R) Review this startup registration and service. What fails at runtime or under config reload?

```csharp
// Program.cs
builder.Services.Configure<RateLimitSettings>(
    builder.Configuration.GetSection("RateLimit"));

// RateLimitService.cs — registered as Singleton
public class RateLimitService
{
    private readonly RateLimitSettings _settings;

    public RateLimitService(IOptionsSnapshot<RateLimitSettings> options)
        => _settings = options.Value;
}
```

---

#### Q4. (P) A developer commits `appsettings.Production.json` containing a Stripe secret key and enables User Secrets locally. Explain what is wrong for production secrets management and what you would use instead while still binding through the Options pattern.

---

#### Q5. (R) Review configuration binding. The app starts in staging with invalid settings and only fails when the first payment runs.

```csharp
public class PaymentOptions
{
    public string MerchantId { get; set; } = "";
    public int TimeoutSeconds { get; set; }
}

builder.Services.Configure<PaymentOptions>(
    builder.Configuration.GetSection("Payment"));

// PaymentGateway.cs
public PaymentGateway(IOptions<PaymentOptions> options)
{
    _options = options.Value;
    _client = new HttpClient { Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds) };
}
```

Staging `appsettings` has `"TimeoutSeconds": 0` and an empty `MerchantId`. No exception at startup.

---

#### Q6. (P) The app connects to two SQL databases — primary and read replica — each with its own connection string section. How do named options (`IOptionsSnapshot<DbConnectionOptions>` with `Configure<T>(name, ...)`) keep the registrations separate, and how does a repository resolve the correct one?

---

#### Q7. (R) A team uses `IOptionsMonitor<T>` to refresh an in-memory rate-limit cache when `appsettings.json` changes. After editing the file, some pods pick up the new limit and others do not until restart. Review the listener code.

```csharp
public class RateLimitCache : IHostedService
{
    private readonly IOptionsMonitor<RateLimitSettings> _monitor;
    private int _currentLimit;

    public RateLimitCache(IOptionsMonitor<RateLimitSettings> monitor)
    {
        _monitor = monitor;
        _currentLimit = monitor.CurrentValue.MaxRequests;
        monitor.OnChange(settings => _currentLimit = settings.MaxRequests);
    }

    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    public bool AllowRequest() => /* uses _currentLimit */;
}
```

`appsettings.json` has `"ReloadOnChange": false` on the JSON provider (default in some templates). ConfigMap updates propagate to the file on disk.

---

#### Q8. (R) Review this middleware and options usage. Operators change `FeatureFlags:EnableBeta` in Azure App Configuration; beta users still see the old behavior until process recycle.

```csharp
public class FeatureGateMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _betaEnabled;

    public FeatureGateMiddleware(RequestDelegate next, IOptions<FeatureFlags> flags)
    {
        _next = next;
        _betaEnabled = flags.Value.EnableBeta; // captured once
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/beta") && !_betaEnabled)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        await _next(context);
    }
}
```

---

#### Q9. (D) A junior developer injects `IConfiguration` directly into every service instead of `IOptions<T>`. When is direct `IConfiguration` acceptable, and when should you enforce the Options pattern with validation and named sections?
