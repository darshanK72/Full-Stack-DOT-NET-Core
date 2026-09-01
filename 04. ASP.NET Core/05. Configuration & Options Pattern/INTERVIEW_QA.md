# Configuration & Options Pattern — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is `IConfiguration` in ASP.NET Core?](#q1-what-is-iconfiguration-in-aspnet-core)
2. [Q2. What configuration sources does ASP.NET Core load by default?](#q2-what-configuration-sources-does-aspnet-core-load-by-default)
3. [Q3. How does configuration key precedence work when the same key exists in multiple sources?](#q3-how-does-configuration-key-precedence-work-when-the-same-key-exists-in-multiple-sources)
4. [Q4. What is the Options pattern?](#q4-what-is-the-options-pattern)
5. [Q5. What is the difference between `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>`?](#q5-what-is-the-difference-between-ioptionst-ioptionssnapshott-and-ioptionsmonitort)
6. [Q6. When would you use `IOptionsMonitor<T>` over `IOptions<T>`?](#q6-when-would-you-use-ioptionsmonitort-over-ioptionst)
7. [Q7. How do you bind a configuration section to a strongly typed class?](#q7-how-do-you-bind-a-configuration-section-to-a-strongly-typed-class)
8. [Q8. What does `Configure<TOptions>(configuration.GetSection("..."))` do?](#q8-what-does-configuretoptionsconfigurationgetsection-do)
9. [Q9. What are named options, and when are they needed?](#q9-what-are-named-options-and-when-are-they-needed)
10. [Q10. How do environment variables map to configuration keys?](#q10-how-do-environment-variables-map-to-configuration-keys)
11. [Q11. What is `ReloadOnChange` on JSON configuration files?](#q11-what-is-reloadonchange-on-json-configuration-files)
12. [Q12. What is the purpose of User Secrets in development?](#q12-what-is-the-purpose-of-user-secrets-in-development)
13. [Q13. How should production secrets be managed?](#q13-how-should-production-secrets-be-managed)
14. [Q14. What is options validation (`ValidateDataAnnotations`, `ValidateOnStart`)?](#q14-what-is-options-validation-validatedataannotations-validateonstart)
15. [Q15. What is the difference between reading `configuration["Key"]` and injecting `IOptions<T>`?](#q15-what-is-the-difference-between-reading-configurationkey-and-injecting-ioptionst)
16. [Q16. How does `appsettings.{Environment}.json` override base settings?](#q16-how-does-appsettingsenvironmentjson-override-base-settings)
17. [Q17. What is `IConfigureOptions<T>`?](#q17-what-is-iconfigureoptionst)
18. [Q18. Can singleton services safely use `IOptionsSnapshot<T>`? Why or why not?](#q18-can-singleton-services-safely-use-ioptionssnapshott-why-or-why-not)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is `IConfiguration` in ASP.NET Core?

What is `IConfiguration` in ASP.NET Core?

**Answer:** `IConfiguration` is the unified read-only abstraction over all configuration sources merged into a key-value hierarchy. Application code and the framework use it to read settings from JSON files, environment variables, command-line arguments, and optional providers such as Azure Key Vault.

- Keys use colon notation for nesting (`ConnectionStrings:DefaultConnection`) regardless of the underlying source format.
- `GetSection("Payment")` returns an `IConfigurationSection` subtree without copying provider data.
- `IConfiguration` is registered as a singleton; the merged view reflects provider updates when a source supports reload.
- Prefer binding to strongly typed options classes for application settings instead of scattering string key lookups across services.

---

## Q2. What configuration sources does ASP.NET Core load by default?

What configuration sources does ASP.NET Core load by default?

**Answer:** `WebApplication.CreateBuilder` configures a default set of configuration providers in a fixed order. Later providers override earlier ones when the same key exists, so environment-specific and deployment-time values can replace base file settings.

- Typical default chain: `appsettings.json`, `appsettings.{Environment}.json`, User Secrets (Development only), environment variables, and command-line arguments.
- `ASPNETCORE_ENVIRONMENT` selects which environment-specific JSON file loads (for example, `appsettings.Development.json`).
- Additional providers — Azure App Configuration, Key Vault, custom INI/XML — are added explicitly in `Program.cs` or host configuration.
- `launchSettings.json` is not part of `IConfiguration` for deployed applications; it only affects local launch profiles.

---

## Q3. How does configuration key precedence work when the same key exists in multiple sources?

How does configuration key precedence work when the same key exists in multiple sources?

**Answer:** Configuration providers are layered in registration order, and **the last registered provider wins** for a duplicate key path. This lets deployment environments override committed defaults without editing source files.

- If `Logging:LogLevel:Default` is `Information` in `appsettings.json` and `Warning` in an environment variable, the environment variable value is used.
- Environment variables map hierarchical keys with double underscores (`Logging__LogLevel__Default=Warning`) or colon on some platforms.
- Command-line arguments registered last override environment variables, which is useful in containers and CI scripts.
- Precedence applies at read time through the merged configuration tree; code that caches values at startup will not see later overrides unless it listens for reload.

---

## Q4. What is the Options pattern?

What is the Options pattern?

**Answer:** The Options pattern binds a configuration section to a strongly typed POCO class and injects it through `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>` instead of raw string lookups. It centralizes settings shape, enables validation, and separates configuration structure from secret storage mechanics.

- Register with `builder.Services.Configure<MySettings>(builder.Configuration.GetSection("MySettings"))`.
- Consumers depend on `IOptions<MySettings>` (or snapshot/monitor) and read `.Value` or subscribe to changes.
- Named options support multiple configurations of the same type (`Configure<StorageOptions>("aws", ...)`).
- Validation attributes and `ValidateOnStart` catch misconfiguration before the app accepts traffic.

---

## Q5. What is the difference between `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>`?

What is the difference between `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>`?

**Answer:** All three expose the same underlying `TOptions` type registered with `Configure<T>`, but they differ in DI lifetime and whether they reflect configuration reloads after startup. Choosing the wrong wrapper causes stale settings or captive dependency errors.

| Abstraction | Lifetime | Reload behavior |
|---|---|---|
| `IOptions<T>` | Singleton | Fixed snapshot at first resolution |
| `IOptionsSnapshot<T>` | Scoped (per request) | Re-reads config each scope/request |
| `IOptionsMonitor<T>` | Singleton | Current value + `OnChange` notifications |

- Use `IOptions<T>` when settings are static for the process lifetime.
- Use `IOptionsSnapshot<T>` in scoped components (controllers, per-request services) that should pick up JSON reload on the next request.
- Use `IOptionsMonitor<T>` in singletons or background services that must react to live configuration changes without scoped dependencies.

---

## Q6. When would you use `IOptionsMonitor<T>` over `IOptions<T>`?

When would you use `IOptionsMonitor<T>` over `IOptions<T>`?

**Answer:** Use `IOptionsMonitor<T>` when a singleton or long-lived component must observe current configuration values after startup, especially when JSON files use `reloadOnChange: true` or an external provider pushes updates. `IOptions<T>` captures `.Value` once and ignores subsequent provider reloads unless the service itself is recreated.

- Subscribe with `monitor.OnChange(settings => { ... })` to refresh caches, HTTP client policies, or feature flags when settings change.
- `CurrentValue` always returns the latest merged configuration for that options type.
- Background workers and middleware registered as singletons should use monitor (or re-read `IConfiguration` with change tokens), not snapshot.
- If settings never change at runtime, `IOptions<T>` is simpler and avoids change-callback complexity.

---

## Q7. How do you bind a configuration section to a strongly typed class?

How do you bind a configuration section to a strongly typed class?

**Answer:** Define a POCO with properties matching configuration keys, then bind the section during service registration or manually with the configuration binder. The binder maps hierarchical keys to nested properties and supports arrays and dictionaries.

- Preferred registration: `builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"))`.
- Manual bind: `var settings = new EmailSettings(); configuration.GetSection("Email").Bind(settings);`
- Property names are case-insensitive by default; `[Required]` and other data annotations participate when validation is enabled.
- Complex types, lists, and dictionary sections bind when key naming follows documented conventions (`Items:0:Name`, `Headers:Accept`).

---

## Q8. What does `Configure<TOptions>(configuration.GetSection("..."))` do?

What does `Configure<TOptions>(configuration.GetSection("..."))` do?

**Answer:** `Configure<TOptions>` registers an `IConfigureOptions<TOptions>` setup that binds the specified configuration section onto a new `TOptions` instance each time options are computed. It connects `IConfiguration` data to the options pipeline consumed by `IOptions<T>`, snapshot, and monitor wrappers.

- Multiple `Configure<TOptions>` calls merge delegates in registration order (later delegates can overwrite earlier property values).
- The section root maps to the options type; missing keys leave default property values intact.
- Binding runs through `OptionsBuilder` and respects culture-invariant conversion for primitives and enums.
- Pair with `ValidateDataAnnotations()` or custom `IValidateOptions<T>` for fail-fast validation at startup or first resolve.

---

## Q9. What are named options, and when are they needed?

What are named options, and when are they needed?

**Answer:** Named options let you register and resolve multiple independent configurations of the same options type, distinguished by a string name. They are needed when one class shape describes several logical profiles — for example, two blob storage backends or multiple JWT bearer schemes.

- Register: `services.Configure<StorageOptions>("aws", config.GetSection("Storage:Aws"));` and similarly for `"azure"`.
- Resolve with `IOptionsSnapshot<StorageOptions>` or `IOptionsMonitor<StorageOptions>` using `.Get("aws")` or inject via `IOptionsFactory<TOptions>`.
- Unnamed `Configure<T>` registers the default name (`Options.DefaultName`).
- Named options avoid duplicating nearly identical POCO types when only configuration values differ.

---

## Q10. How do environment variables map to configuration keys?

How do environment variables map to configuration keys?

**Answer:** Environment variables become flat keys in the configuration provider, with hierarchy represented by double underscores (`__`) or, on some systems, colons. The provider runs late in the default chain, so env vars commonly override JSON settings in containers and cloud hosts.

- `ConnectionStrings__DefaultConnection` maps to `ConnectionStrings:DefaultConnection`.
- ASP.NET Core also recognizes conventional env vars such as `ASPNETCORE_URLS` and `ASPNETCORE_ENVIRONMENT` through dedicated hosting configuration.
- Kubernetes ConfigMaps and Secrets mounted as env vars follow the same mapping rules without code changes.
- Prefix filters (`AddEnvironmentVariables("MYAPP_")`) limit which variables enter configuration in multi-tenant hosts.

---

## Q11. What is `ReloadOnChange` on JSON configuration files?

What is `ReloadOnChange` on JSON configuration files?

**Answer:** When `appsettings.json` is added with `reloadOnChange: true` (the default in the generic host), the physical file watcher reloads that provider when the file changes on disk. The merged `IConfiguration` updates, and options monitors or change tokens can react without restarting the process.

- Reload affects the configuration provider layer; services that cached values in fields at startup still hold stale data until they use monitor or re-read configuration.
- `IOptionsSnapshot<T>` picks up changes on the next request scope after reload.
- File reload is suitable for non-secret tuning (feature flags, timeouts); secret rotation often still requires pod restart or a vault provider with its own refresh semantics.
- Heavy reload churn on network-mounted config files can cause frequent recomputation — validate operational impact in production.

---

## Q12. What is the purpose of User Secrets in development?

What is the purpose of User Secrets in development?

**Answer:** User Secrets store developer-specific sensitive values outside the project tree on the local machine, loaded only when `DOTNET_ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` is Development. They prevent committing connection strings, API keys, and tokens to source control while keeping the same `IConfiguration` key paths as production.

- Enabled with `UserSecretsId` in the `.csproj` and accessed via `dotnet user-secrets set "Stripe:SecretKey" "..."`.
- Secrets are stored under the user profile, not deployed with the published application.
- They integrate as a configuration provider after JSON files in Development, overriding local `appsettings.Development.json` values.
- User Secrets are not a production mechanism — they complement, rather than replace, vault or platform secret injection.

---

## Q13. How should production secrets be managed?

How should production secrets be managed?

**Answer:** Production secrets should live in a dedicated secret store or platform injection mechanism, never in committed configuration files or repository history. Application code continues to consume secrets through `IConfiguration` and the Options pattern while deployment supplies values via environment variables, Azure Key Vault, AWS Secrets Manager, or Kubernetes Secrets.

- Remove secrets from `appsettings.Production.json`; keep only non-sensitive defaults and structure.
- Use managed identity or workload identity when connecting to Azure Key Vault (`AddAzureKeyVault`) rather than embedding vault credentials.
- Mount secrets as environment variables in containers (`Payment__ApiKey`) so the same binding code works across environments.
- Rotate compromised keys in the vault and redeploy or restart workloads; removing a secret from git does not invalidate leaked history.

---

## Q14. What is options validation (`ValidateDataAnnotations`, `ValidateOnStart`)?

What is options validation (`ValidateDataAnnotations`, `ValidateOnStart`)?

**Answer:** Options validation runs registered validators against bound options instances to ensure required fields, ranges, and custom rules are satisfied before the application relies on them. `ValidateDataAnnotations()` applies attribute-based rules; `ValidateOnStart()` fails application startup if validation fails instead of deferring failure to the first consumer.

- Example: `builder.Services.AddOptions<PaymentOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart();`
- Implement `IValidateOptions<TOptions>` for cross-property rules that annotations cannot express.
- `ValidateOnStart` is especially valuable for API keys, connection strings, and feature toggles whose absence would cause obscure runtime errors.
- Validation runs when options are first built at startup (with `ValidateOnStart`) or on each options factory invocation depending on configuration.

---

## Q15. What is the difference between reading `configuration["Key"]` and injecting `IOptions<T>`?

What is the difference between reading `configuration["Key"]` and injecting `IOptions<T>`?

**Answer:** Direct `IConfiguration` indexing reads a single string key at the call site, while `IOptions<T>` supplies a typed, bound object whose shape is validated and registered once in DI. Options encourage testability and centralized settings models; raw configuration suits one-off framework setup in `Program.cs`.

- `configuration["Payment:ApiKey"]` returns a string or null with no compile-time property checking.
- `IOptions<PaymentOptions>.Value.ApiKey` is strongly typed and can carry validation attributes and default values on the POCO.
- Options support reload wrappers (snapshot/monitor); manual `configuration["Key"]` reads current merged value but does not notify consumers automatically.
- Use `IConfiguration` in startup/bootstrap code; prefer options in application services and domain layers.

---

## Q16. How does `appsettings.{Environment}.json` override base settings?

How does `appsettings.{Environment}.json` override base settings?

**Answer:** The host loads `appsettings.json` first, then loads `appsettings.{Environment}.json` when the file exists, with the environment name taken from `ASPNETCORE_ENVIRONMENT`. Keys present in the environment file replace matching keys from the base file while unspecified keys inherit base values.

- Setting `ASPNETCORE_ENVIRONMENT=Development` merges `appsettings.Development.json` over the base file.
- Override is key-by-key at the configuration provider level, not a full document replacement — nested sections merge recursively.
- Production deployments typically set environment via hosting platform variables rather than editing files on disk.
- Missing environment files are skipped silently; only the base `appsettings.json` applies when no override file exists.

---

## Q17. What is `IConfigureOptions<T>`?

What is `IConfigureOptions<T>`?

**Answer:** `IConfigureOptions<T>` is the extensibility hook that mutates a `TOptions` instance after binding and before consumers read it. Multiple implementations run in registration order, enabling modular libraries to contribute defaults or post-bind adjustments without a single monolithic `Configure` call.

- Implement `Configure(TOptions options)` or `IConfigureNamedOptions<T>` for named variants.
- Framework and libraries register configurators internally — for example, `JwtBearerOptions` setup from authentication extensions.
- Application code usually calls `services.Configure<T>(configuration.GetSection(...))`, which registers an internal `IConfigureOptions<T>` under the hood.
- Post-configure with `IPostConfigureOptions<T>` when values must be adjusted after all `IConfigureOptions` delegates run.

---

## Q18. Can singleton services safely use `IOptionsSnapshot<T>`? Why or why not?

Can singleton services safely use `IOptionsSnapshot<T>`? Why or why not?

**Answer:** No — `IOptionsSnapshot<T>` is registered with a scoped lifetime because it is recomputed per scope to reflect configuration reloads on each request. Injecting it into a singleton creates a captive dependency; with `ValidateScopes` enabled, startup fails with an invalid scope error.

- Singleton services should use `IOptions<T>` for static settings or `IOptionsMonitor<T>` when live reload is required.
- Controllers and scoped services are the intended consumers of `IOptionsSnapshot<T>`.
- If a singleton mistakenly resolves snapshot from the root provider without validation, behavior is undefined and may appear to work until scopes dispose.
- The options interfaces encode lifetime contracts — matching consumer lifetime to the correct options wrapper prevents subtle stale-config bugs.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Answer:** In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing breaks endpoint-aware authorization and policy resolution.

- The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`).
- When auth runs before routing, the endpoint has not been selected yet and `[Authorize]` metadata on minimal routes or controllers may not apply correctly.
- Symptoms include anonymous access to protected endpoints or 401 responses without proper challenge behavior.
- Always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Answer:** Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`.

- The singleton holds one scoped instance forever instead of one per request — EF change trackers accumulate unrelated entities.
- Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup.
- Fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.
- This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Answer:** Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected.

- `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` in a singleton is an anti-pattern.
- `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly.
- Register named or typed clients: `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>();`
- Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Answer:** `IOptions<T>` captures configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled.

- `IOptionsSnapshot<T>` recalculates per request scope; `IOptionsMonitor<T>` supports change notifications via `OnChange`.
- Singleton services must use `IOptionsMonitor<T>` or read options inside scoped operations if they need live updates.
- Misconfiguration persists silently until process restart when `.Value` was cached at construction.
- See Chapter 05 for the full options lifetime comparison.

---

#### Gotcha 5. GET with `[FromBody]`

**Answer:** Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production.

- Query strings and route values are the correct binding sources for GET requests.
- Complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys.
- Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development.
- REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Answer:** ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled.

- Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero).
- Prefer standardizing clients on camelCase and documenting the contract in OpenAPI.
- Optional mitigation: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — but explicit camelCase contracts are cleaner.
- Add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Answer:** Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown.

- Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis.
- Always use `throw;` when rethrowing after logging or cleanup in a catch block.
- Wrap in a new exception only when adding context: `throw new OrderProcessingException("...", ex)` to preserve `InnerException`.
- This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Answer:** Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require.

- Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front.
- TLS certificates are easier to manage at the proxy layer with automatic renewal.
- Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.
- Containers often bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Answer:** Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts.

- Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.
- Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments.
- The file is development ergonomics, not runtime configuration.
- Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Answer:** A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics.

- PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent.
- Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.
- Create DTOs may use non-nullable bool when explicit values are always required on insert.
- Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Answer:** Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs.

- Call `UseForwardedHeaders()` early, before middleware that reads scheme or host (HTTPS redirection, link generation, rate limiting by IP).
- Configure `ForwardedHeadersOptions` to trust only your reverse proxy network — trusting all proxies enables header spoofing.
- Headers include `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`.
- Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Answer:** Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default — placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP.

- Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`.
- Sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers.
- Accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident.
- Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Answer:** SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers.

- Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`.
- Scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes.
- Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting.
- Order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Answer:** A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration.

- Hosted services live for the application lifetime — scoped dependencies must not be constructor-injected.
- Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes.
- Same rule applies to timers and `Task.Run` loops started from singletons.
- Enable `ValidateScopes` to catch this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Answer:** SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events.

- Sticky sessions keep one client on one node but do not route events raised on other nodes to that client.
- Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application.
- Raw WebSocket apps need equivalent custom pub/sub — SignalR's backplane is the built-in solution.
- Test scale-out with at least two instances before launch, not single-node staging alone.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

## Gotchas — ASP.NET Core (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) A service reads `configuration["Payment:ApiKey"]` at startup and caches it in a field. Ops rotates the key via environment variable override in Kubernetes without redeploying, but the app keeps using the old key. Explain how `IConfiguration` providers and precedence work, and why this pattern fails for hot reload.

---

**Answer:**

**Answer:** `IConfiguration` merges providers in registration order — later providers override earlier keys for the same path — but values read once into a field are frozen; they do not automatically refresh when a provider reloads unless you subscribe to change tokens or use `IOptionsMonitor<T>`.

- Default host order (simplified): `appsettings.json` → `appsettings.{Environment}.json` → User Secrets (Development) → environment variables → command-line args — **last wins** for duplicate keys (`Payment__ApiKey` in env overrides JSON).
- `IConfiguration` is a read facade over the merged tree; `GetSection("Payment")["ApiKey"]` returns the effective value at read time only.
- Caching in a constructor field bypasses reload even when the JSON file has `reloadOnChange: true` — the provider updates internally, but your field does not.
- Fix: inject `IOptionsMonitor<PaymentSettings>` and read `CurrentValue`, or register `IOptionsMonitor` with `OnChange` to refresh clients; for secrets rotated via env var, most deployments still require pod restart unless using a provider that pushes updates (Key Vault refresh, App Configuration).
- Do not confuse **precedence at startup** with **live reload** — env vars injected at pod start are static until the pod is recreated.

**Production takeaway:** Karat tests whether you know that "we changed the ConfigMap" does not update an eagerly cached string field.

---

---

#### Q2. (M) Three consumers need settings from the same `appsettings.json` section: a singleton cache warmer, an MVC controller, and a background `IHostedService` that must react when `ReloadOnChange` updates the file. Which of `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` belongs in each, and what breaks if you swap them?

---

**Answer:**

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

**Answer:**

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

---

#### Q4. (P) A developer commits `appsettings.Production.json` containing a Stripe secret key and enables User Secrets locally. Explain what is wrong for production secrets management and what you would use instead while still binding through the Options pattern.

---

**Answer:**

**Answer:** Production secrets must never live in source-controlled JSON; User Secrets are Development-only and are not deployed — production should load secrets from a secret store or platform injection while still binding to a typed options class.

- Remove secrets from all committed `appsettings*.json`; use placeholders or Key Vault references only.
- User Secrets (`UserSecretsId` in `.csproj`) apply on a developer machine — they do not exist on Azure App Service, Kubernetes, or IIS unless manually duplicated.
- Production patterns: Azure Key Vault provider (`AddAzureKeyVault`), AWS Secrets Manager, Kubernetes Secrets mounted as env vars (`Stripe__SecretKey`), or CI/CD secret injection at deploy time.
- Keep consumption typed: `services.Configure<StripeOptions>(configuration.GetSection("Stripe"))` — the **source** changes, not the service code.
- Rotate keys in the vault; restart pods or use Key Vault refresh provider — do not rely on git history to "remove" a leaked key.

**Production takeaway:** Karat checks whether you separate **configuration shape** (Options) from **secret storage** (never in repo).

---

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

**Answer:**

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

---

#### Q6. (P) The app connects to two SQL databases — primary and read replica — each with its own connection string section. How do named options (`IOptionsSnapshot<DbConnectionOptions>` with `Configure<T>(name, ...)`) keep the registrations separate, and how does a repository resolve the correct one?

---

**Answer:**

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

**Answer:**

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

**Answer:**

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

---

#### Q9. (D) A junior developer injects `IConfiguration` directly into every service instead of `IOptions<T>`. When is direct `IConfiguration` acceptable, and when should you enforce the Options pattern with validation and named sections?



**Answer:**

**Answer:** Direct `IConfiguration` is fine for one-off keys in infrastructure glue (e.g., reading a single connection string in `Program.cs`) — domain services should use validated `IOptions<T>` or `IOptionsMonitor<T>` so settings are typed, testable, and fail-fast.

- **Acceptable:** Startup/bootstrap code, custom `IConfigurationSource` authoring, rare dynamic key lookups where section names are not known at compile time.
- **Prefer Options:** Any setting used in business logic, anything requiring validation, multi-section binding, named registrations, or reload semantics.
- **Testing:** `IOptions<T>` is trivial to mock with `Options.Create(new MySettings { ... })`; `IConfiguration` requires building an in-memory configuration tree.
- **Team rule:** Services take `IOptionsSnapshot<T>` or monitor — only the composition root reads raw `IConfiguration` to call `Configure<T>()`.
- Avoid `configuration["Payment:ApiKey"]` string typos in hot paths — refactor to properties with compile-time names.

**Production takeaway:** Options pattern is not ceremony — it is typed contracts plus validation at startup, which Karat treats as production hygiene.

---

---
