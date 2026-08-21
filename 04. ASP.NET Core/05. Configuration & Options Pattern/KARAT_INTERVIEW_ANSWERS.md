# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/05. Configuration & Options Pattern`

---

#### Q1. (M) A service reads `configuration["Payment:ApiKey"]` at startup and caches it in a field. Ops rotates the key via environment variable override in Kubernetes without redeploying, but the app keeps using the old key. Explain how `IConfiguration` providers and precedence work, and why this pattern fails for hot reload.

**Answer:** `IConfiguration` merges providers in registration order — later providers override earlier keys for the same path — but values read once into a field are frozen; they do not automatically refresh when a provider reloads unless you subscribe to change tokens or use `IOptionsMonitor<T>`.

- Default host order (simplified): `appsettings.json` → `appsettings.{Environment}.json` → User Secrets (Development) → environment variables → command-line args — **last wins** for duplicate keys (`Payment__ApiKey` in env overrides JSON).
- `IConfiguration` is a read facade over the merged tree; `GetSection("Payment")["ApiKey"]` returns the effective value at read time only.
- Caching in a constructor field bypasses reload even when the JSON file has `reloadOnChange: true` — the provider updates internally, but your field does not.
- Fix: inject `IOptionsMonitor<PaymentSettings>` and read `CurrentValue`, or register `IOptionsMonitor` with `OnChange` to refresh clients; for secrets rotated via env var, most deployments still require pod restart unless using a provider that pushes updates (Key Vault refresh, App Configuration).
- Do not confuse **precedence at startup** with **live reload** — env vars injected at pod start are static until the pod is recreated.

**Production takeaway:** Karat tests whether you know that "we changed the ConfigMap" does not update an eagerly cached string field.

---

#### Q2. (M) Three consumers need settings from the same `appsettings.json` section: a singleton cache warmer, an MVC controller, and a background `IHostedService` that must react when `ReloadOnChange` updates the file. Which of `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` belongs in each, and what breaks if you swap them?

**Answer:** Use `IOptions<T>` for the cache warmer if settings are fixed for process lifetime, `IOptionsSnapshot<T>` in the scoped controller per request, and `IOptionsMonitor<T>` in the hosted service with `OnChange` — swapping snapshot into a singleton causes captive dependency errors; swapping monitor-only into controllers works but is heavier than needed.

| Consumer | Abstraction | Why |
|---|---|---|
| Singleton cache warmer | `IOptions<T>` | One snapshot at first resolve; no per-request scope needed if reload is not required |
| MVC/API controller | `IOptionsSnapshot<T>` | Scoped per request; picks up config reload on next request without manual listeners |
| `IHostedService` reacting to file change | `IOptionsMonitor<T>` | Singleton-safe; `OnChange` callback updates background state without scoped services |

- Injecting `IOptionsSnapshot<T>` into a singleton throws when `ValidateScopes` is enabled — classic captive dependency.
- Using `IOptions<T>` in middleware or hosted services that must react to hot reload leaves stale values until restart.
- All three share `services.Configure<MySettings>(section)` — the registration is one; the wrapper interface chooses reload semantics.

**Production takeaway:** The question is DI lifetime plus freshness, not memorizing interface names.

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

**Answer:** A singleton service cannot depend on `IOptionsSnapshot<T>` because snapshot is **scoped** — the app fails at startup (with scope validation) or creates undefined lifetime behavior without validation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `IOptionsSnapshot<T>` injected into singleton | `InvalidOperationException` at startup when `ValidateScopes` is true; captive dependency |
| Options reload | Settings copied once in constructor | Even if it ran, `_settings` would not refresh on config change without monitor |
| Design | Singleton holds per-request abstraction | Violates ASP.NET Core options lifetime contract |

**Fix (priority order):**

1. Change constructor to `IOptions<RateLimitSettings>` if limits are static for the process, **or** `IOptionsMonitor<RateLimitSettings>` if reload is required — subscribe in constructor with `monitor.OnChange`.
2. If per-request limits are truly needed, register `RateLimitService` as **scoped**, not singleton.
3. Enable `ValidateScopes` in development to catch this class of bug before deploy.

**Production takeaway:** Options interfaces encode lifetimes — snapshot in singleton is one of the most common Karat DI traps.

---

#### Q4. (P) A developer commits `appsettings.Production.json` containing a Stripe secret key and enables User Secrets locally. Explain what is wrong for production secrets management and what you would use instead while still binding through the Options pattern.

**Answer:** Production secrets must never live in source-controlled JSON; User Secrets are Development-only and are not deployed — production should load secrets from a secret store or platform injection while still binding to a typed options class.

- Remove secrets from all committed `appsettings*.json`; use placeholders or Key Vault references only.
- User Secrets (`UserSecretsId` in `.csproj`) apply on a developer machine — they do not exist on Azure App Service, Kubernetes, or IIS unless manually duplicated.
- Production patterns: Azure Key Vault provider (`AddAzureKeyVault`), AWS Secrets Manager, Kubernetes Secrets mounted as env vars (`Stripe__SecretKey`), or CI/CD secret injection at deploy time.
- Keep consumption typed: `services.Configure<StripeOptions>(configuration.GetSection("Stripe"))` — the **source** changes, not the service code.
- Rotate keys in the vault; restart pods or use Key Vault refresh provider — do not rely on git history to "remove" a leaked key.

**Production takeaway:** Karat checks whether you separate **configuration shape** (Options) from **secret storage** (never in repo).

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

**Answer:** `Configure<T>` binds without validation by default — empty strings and zero timeouts are valid CLR values — so invalid business rules slip through until runtime unless you add `IValidateOptions<T>` or DataAnnotations validation at startup.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Validation | No `[Required]` / `IValidateOptions` | Empty `MerchantId` and `TimeoutSeconds: 0` bind successfully |
| HTTP client | `TimeSpan.FromSeconds(0)` | Immediate timeout on every call — flaky failures under load |
| Fail-fast | Invalid config not rejected at startup | Staging deploy looks healthy; first payment fails in production |

**Fix (priority order):**

1. Add `services.AddOptions<PaymentOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart()` (or custom `IValidateOptions<PaymentOptions>`).
2. Mark `MerchantId` with `[Required]` and `TimeoutSeconds` with `[Range(1, 300)]`.
3. Register `IValidateOptions` implementations for cross-field rules (e.g., merchant ID format).

```csharp
builder.Services.AddOptions<PaymentOptions>()
    .Bind(builder.Configuration.GetSection("Payment"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

**Production takeaway:** Options binding is not validation — Karat expects you to fail fast at startup in staging.

---

#### Q6. (P) The app connects to two SQL databases — primary and read replica — each with its own connection string section. How do named options (`IOptionsSnapshot<DbConnectionOptions>` with `Configure<T>(name, ...)`) keep the registrations separate, and how does a repository resolve the correct one?

**Answer:** Register two named configurations against the same options type, then inject `IOptionsSnapshot<DbConnectionOptions>` and call `Get("Primary")` or `Get("Replica")` — or use `IOptionsFactory<T>` / `[Options("Primary")]` attribute on the consuming parameter in minimal hosting scenarios.

```csharp
builder.Services.Configure<DbConnectionOptions>("Primary",
    builder.Configuration.GetSection("ConnectionStrings:Primary"));
builder.Services.Configure<DbConnectionOptions>("Replica",
    builder.Configuration.GetSection("ConnectionStrings:Replica"));

public class OrderRepository
{
    private readonly DbConnectionOptions _primary;

    public OrderRepository(IOptionsSnapshot<DbConnectionOptions> options)
        => _primary = options.Get("Primary");
}
```

- Without names, the last `Configure<DbConnectionOptions>` wins — both repositories would share one connection string.
- Named options also support `services.ConfigureAll<T>()` for shared defaults plus per-name overrides.
- Validation: `services.AddOptions<DbConnectionOptions>("Primary").Bind(...).ValidateOnStart()`.

**Production takeaway:** Named options are the standard pattern for multi-tenant connection strings, SMTP profiles, and dual-database read/write splitting.

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
    // ...
}
```

`appsettings.json` has `"ReloadOnChange": false` on the JSON provider (default in some templates). ConfigMap updates propagate to the file on disk.

**Answer:** The `OnChange` wiring is correct, but reload never fires if the JSON configuration source was not registered with `reloadOnChange: true` — and in Kubernetes, not every pod sees the file update atomically at the same instant without a shared configuration provider.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Config provider | `ReloadOnChange: false` (or missing) | File edits on disk do not trigger `IChangeToken` — `OnChange` never runs |
| Deployment | Per-pod local `appsettings.json` from ConfigMap | Rolling update timing means pods briefly disagree on limits |
| Thread safety | `_currentLimit` updated from callback without `Interlocked` | Rare torn reads under concurrent requests (minor vs reload issue) |

**Fix (priority order):**

1. Ensure JSON source reload: `builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)`.
2. For cluster-wide dynamic config, prefer **Azure App Configuration**, **Consul**, or env-based limits with a control plane — file reload on mounted volumes is fragile.
3. Make `_currentLimit` updates thread-safe: `Interlocked.Exchange(ref _currentLimit, settings.MaxRequests)`.

**Production takeaway:** `IOptionsMonitor` only reacts when the underlying `IConfiguration` provider signals change — no signal, no reload.

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
    // ...
}
```

**Answer:** Middleware is constructed once per application lifetime; capturing `IOptions<T>.Value` in the constructor freezes the flag at startup — use `IOptionsMonitor<FeatureFlags>` and read `CurrentValue` per request (or subscribe to `OnChange`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Options reload | `IOptions<T>` snapshot in ctor field | Feature flag changes in App Configuration have no effect until restart |
| Middleware lifetime | Single middleware instance for app | Constructor runs once — not per request |
| Operations | Hot flag toggle expected | Ops believes config push worked; users still blocked or exposed |

**Fix (priority order):**

1. Inject `IOptionsMonitor<FeatureFlags>` and read `_flags.CurrentValue.EnableBeta` inside `InvokeAsync`.
2. Alternatively use Microsoft.FeatureManagement (`IFeatureManager`) which integrates with App Configuration refresh.
3. Document which flags require recycle vs dynamic toggle in runbooks.

**Production takeaway:** Middleware plus `IOptions<T>` is a classic "we toggled the flag in Azure but nothing changed" postmortem.

---

#### Q9. (D) A junior developer injects `IConfiguration` directly into every service instead of `IOptions<T>`. When is direct `IConfiguration` acceptable, and when should you enforce the Options pattern with validation and named sections?

**Answer:** Direct `IConfiguration` is fine for one-off keys in infrastructure glue (e.g., reading a single connection string in `Program.cs`) — domain services should use validated `IOptions<T>` or `IOptionsMonitor<T>` so settings are typed, testable, and fail-fast.

- **Acceptable:** Startup/bootstrap code, custom `IConfigurationSource` authoring, rare dynamic key lookups where section names are not known at compile time.
- **Prefer Options:** Any setting used in business logic, anything requiring validation, multi-section binding, named registrations, or reload semantics.
- **Testing:** `IOptions<T>` is trivial to mock with `Options.Create(new MySettings { ... })`; `IConfiguration` requires building an in-memory configuration tree.
- **Team rule:** Services take `IOptionsSnapshot<T>` or monitor — only the composition root reads raw `IConfiguration` to call `Configure<T>()`.
- Avoid `configuration["Payment:ApiKey"]` string typos in hot paths — refactor to properties with compile-time names.

**Production takeaway:** Options pattern is not ceremony — it is typed contracts plus validation at startup, which Karat treats as production hygiene.

---
