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

**Concepts**
- Unified read-only abstraction over all configuration sources
- Colon-delimited hierarchical key notation
- `GetSection()` returning a subtree without copying data
- Singleton registration reflecting provider updates on reload

**Answer**

`IConfiguration` is the unified read-only abstraction over all configuration sources merged into a key-value hierarchy. Application code and the framework use it to read settings from JSON files, environment variables, command-line arguments, and optional providers such as Azure Key Vault. Keys use colon notation for nesting (`ConnectionStrings:DefaultConnection`) regardless of the underlying source format, and `GetSection("Payment")` returns an `IConfigurationSection` subtree without copying provider data. `IConfiguration` is registered as a singleton; the merged view reflects provider updates when a source supports reload. Prefer binding to strongly typed options classes for application settings instead of scattering string key lookups across services.

---

## Q2. What configuration sources does ASP.NET Core load by default?

**Concepts**
- Default provider chain and its fixed order
- Last-provider-wins for duplicate keys
- `ASPNETCORE_ENVIRONMENT` selecting the environment-specific JSON file
- `launchSettings.json` as development-only and not part of `IConfiguration`

**Answer**

`WebApplication.CreateBuilder` configures a default set of configuration providers in a fixed order, and later providers override earlier ones when the same key exists, so environment-specific and deployment-time values can replace base file settings. The typical default chain is `appsettings.json`, `appsettings.{Environment}.json`, User Secrets (Development only), environment variables, and command-line arguments. `ASPNETCORE_ENVIRONMENT` selects which environment-specific JSON file loads — for example `appsettings.Development.json`. Additional providers such as Azure App Configuration, Key Vault, custom INI/XML are added explicitly in `Program.cs` or host configuration. `launchSettings.json` is not part of `IConfiguration` for deployed applications; it only affects local launch profiles.

---

## Q3. How does configuration key precedence work when the same key exists in multiple sources?

**Concepts**
- Last-registered provider wins for a duplicate key path
- Environment variable double-underscore hierarchy mapping
- Command-line arguments registered last overriding everything
- Cached values at startup not seeing later overrides without change tokens

**Answer**

Configuration providers are layered in registration order, and the last registered provider wins for a duplicate key path, which lets deployment environments override committed defaults without editing source files. If `Logging:LogLevel:Default` is `Information` in `appsettings.json` and `Warning` in an environment variable, the environment variable value is used. Environment variables map hierarchical keys with double underscores (`Logging__LogLevel__Default=Warning`) or colon on some platforms. Command-line arguments registered last override environment variables, which is useful in containers and CI scripts. Precedence applies at read time through the merged configuration tree; code that caches values at startup will not see later overrides unless it listens for reload via change tokens or `IOptionsMonitor<T>`.

---

## Q4. What is the Options pattern?

**Concepts**
- Strongly typed POCO bound to a configuration section
- `IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>` as injection wrappers
- Named options for multiple configurations of the same type
- `ValidateOnStart` for fail-fast validation

**Answer**

The Options pattern binds a configuration section to a strongly typed POCO class and injects it through `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>` instead of raw string lookups. It centralizes settings shape, enables validation, and separates configuration structure from secret storage mechanics. Register with `builder.Services.Configure<MySettings>(builder.Configuration.GetSection("MySettings"))`, and consumers depend on `IOptions<MySettings>` or snapshot/monitor and read `.Value` or subscribe to changes. Named options support multiple configurations of the same type (`Configure<StorageOptions>("aws", ...)`), and validation attributes combined with `ValidateOnStart` catch misconfiguration before the app accepts traffic.

---

## Q5. What is the difference between `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>`?

**Concepts**
- `IOptions<T>` — singleton, fixed snapshot at first resolution
- `IOptionsSnapshot<T>` — scoped, re-reads config each request
- `IOptionsMonitor<T>` — singleton, current value plus `OnChange` notifications
- Captive dependency when snapshot is injected into a singleton

**Answer**

All three expose the same underlying `TOptions` type registered with `Configure<T>`, but they differ in DI lifetime and whether they reflect configuration reloads after startup. Choosing the wrong wrapper causes stale settings or captive dependency errors. Use `IOptions<T>` when settings are static for the process lifetime. Use `IOptionsSnapshot<T>` in scoped components (controllers, per-request services) that should pick up JSON reload on the next request, since snapshot is recomputed per scope. Use `IOptionsMonitor<T>` in singletons or background services that must react to live configuration changes without scoped dependencies, since monitor is singleton-safe and exposes `CurrentValue` plus an `OnChange` callback.

| Abstraction | Lifetime | Reload behavior |
|---|---|---|
| `IOptions<T>` | Singleton | Fixed snapshot at first resolution |
| `IOptionsSnapshot<T>` | Scoped (per request) | Re-reads config each scope/request |
| `IOptionsMonitor<T>` | Singleton | Current value + `OnChange` notifications |

---

## Q6. When would you use `IOptionsMonitor<T>` over `IOptions<T>`?

**Concepts**
- Singleton or long-lived component needing live configuration values
- `reloadOnChange: true` on JSON files or external provider pushing updates
- `OnChange` callback for cache or policy refresh
- `CurrentValue` always returning the latest merged configuration

**Answer**

Use `IOptionsMonitor<T>` when a singleton or long-lived component must observe current configuration values after startup, especially when JSON files use `reloadOnChange: true` or an external provider pushes updates. `IOptions<T>` captures `.Value` once and ignores subsequent provider reloads unless the service itself is recreated, so any value cached in a field at construction is stale after the next reload. Subscribe with `monitor.OnChange(settings => { ... })` to refresh caches, HTTP client policies, or feature flags when settings change, and use `CurrentValue` to always read the latest merged configuration for that options type. Background workers and middleware registered as singletons should use monitor or re-read `IConfiguration` with change tokens, not snapshot. If settings never change at runtime, `IOptions<T>` is simpler and avoids change-callback complexity.

---

## Q7. How do you bind a configuration section to a strongly typed class?

**Concepts**
- POCO with properties matching configuration keys
- `services.Configure<T>(section)` as the preferred registration
- `Bind()` for manual one-off binding
- Case-insensitive property matching and array/dictionary support

**Answer**

Define a POCO with properties matching configuration keys, then bind the section during service registration or manually with the configuration binder. The binder maps hierarchical keys to nested properties and supports arrays and dictionaries. The preferred registration is `builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"))`, while manual bind uses `var settings = new EmailSettings(); configuration.GetSection("Email").Bind(settings)`. Property names are case-insensitive by default, and `[Required]` and other data annotations participate when validation is enabled. Complex types, lists, and dictionary sections bind when key naming follows documented conventions (`Items:0:Name`, `Headers:Accept`).

---

## Q8. What does `Configure<TOptions>(configuration.GetSection("..."))` do?

**Concepts**
- `IConfigureOptions<TOptions>` registered to bind the section
- Multiple `Configure<T>` calls merging in registration order
- `OptionsBuilder` pipeline respecting culture-invariant conversion
- Pairing with `ValidateDataAnnotations()` for fail-fast validation

**Answer**

`Configure<TOptions>` registers an `IConfigureOptions<TOptions>` setup that binds the specified configuration section onto a new `TOptions` instance each time options are computed. It connects `IConfiguration` data to the options pipeline consumed by `IOptions<T>`, snapshot, and monitor wrappers. Multiple `Configure<TOptions>` calls merge delegates in registration order — later delegates can overwrite earlier property values — and the section root maps to the options type while missing keys leave default property values intact. Binding runs through `OptionsBuilder` and respects culture-invariant conversion for primitives and enums. Pair with `ValidateDataAnnotations()` or custom `IValidateOptions<T>` for fail-fast validation at startup or first resolve.

---

## Q9. What are named options, and when are they needed?

**Concepts**
- Named options for multiple independent configurations of the same POCO type
- `services.Configure<T>("name", section)` registration
- `IOptionsSnapshot<T>.Get("name")` or `IOptionsMonitor<T>.Get("name")` resolution
- Avoiding duplicate POCO types when only configuration values differ

**Answer**

Named options let you register and resolve multiple independent configurations of the same options type, distinguished by a string name. They are needed when one class shape describes several logical profiles — for example, two blob storage backends or multiple JWT bearer schemes. Register with `services.Configure<StorageOptions>("aws", config.GetSection("Storage:Aws"))` and similarly for `"azure"`. Resolve with `IOptionsSnapshot<StorageOptions>` or `IOptionsMonitor<StorageOptions>` using `.Get("aws")` or inject via `IOptionsFactory<TOptions>`. Unnamed `Configure<T>` registers the default name (`Options.DefaultName`), so named options avoid duplicating nearly identical POCO types when only configuration values differ.

---

## Q10. How do environment variables map to configuration keys?

**Concepts**
- Double-underscore (`__`) as the hierarchy separator for env vars
- Late-registered env var provider overriding JSON settings
- Kubernetes ConfigMaps and Secrets following the same mapping rules
- Prefix filters limiting which variables enter configuration

**Answer**

Environment variables become flat keys in the configuration provider, with hierarchy represented by double underscores (`__`) or, on some systems, colons. The provider runs late in the default chain, so env vars commonly override JSON settings in containers and cloud hosts. `ConnectionStrings__DefaultConnection` maps to `ConnectionStrings:DefaultConnection`, and ASP.NET Core also recognizes conventional env vars such as `ASPNETCORE_URLS` and `ASPNETCORE_ENVIRONMENT` through dedicated hosting configuration. Kubernetes ConfigMaps and Secrets mounted as env vars follow the same mapping rules without code changes, and prefix filters (`AddEnvironmentVariables("MYAPP_")`) limit which variables enter configuration in multi-tenant hosts.

---

## Q11. What is `ReloadOnChange` on JSON configuration files?

**Concepts**
- Physical file watcher reloading the provider on file change
- `IConfiguration` updating while field-cached values stay stale
- `IOptionsSnapshot<T>` picking up changes on the next request scope
- Reload unsuitable for secret rotation that requires process restart

**Answer**

When `appsettings.json` is added with `reloadOnChange: true` (the default in the generic host), the physical file watcher reloads that provider when the file changes on disk. The merged `IConfiguration` updates, and options monitors or change tokens can react without restarting the process. Reload affects the configuration provider layer; services that cached values in fields at startup still hold stale data until they use monitor or re-read configuration, since the field holds a copy, not a reference to the provider. `IOptionsSnapshot<T>` picks up changes on the next request scope after reload. File reload is suitable for non-secret tuning such as feature flags and timeouts; secret rotation often still requires pod restart or a vault provider with its own refresh semantics. Heavy reload churn on network-mounted config files can cause frequent recomputation, so validate operational impact in production.

---

## Q12. What is the purpose of User Secrets in development?

**Concepts**
- Secrets stored outside the project tree, not committed to source control
- Loaded only in Development via `UserSecretsId` in `.csproj`
- Same `IConfiguration` key paths as production
- Not a production mechanism — complements vault or platform injection

**Answer**

User Secrets store developer-specific sensitive values outside the project tree on the local machine, loaded only when `DOTNET_ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` is Development. They prevent committing connection strings, API keys, and tokens to source control while keeping the same `IConfiguration` key paths as production, so no code changes are needed when moving values to a vault. Enable with `UserSecretsId` in the `.csproj` and access via `dotnet user-secrets set "Stripe:SecretKey" "..."`. Secrets are stored under the user profile, not deployed with the published application. They integrate as a configuration provider after JSON files in Development, overriding local `appsettings.Development.json` values. User Secrets are not a production mechanism — they complement, rather than replace, vault or platform secret injection.

---

## Q13. How should production secrets be managed?

**Concepts**
- Secrets in a dedicated secret store, never in committed configuration files
- Azure Key Vault, AWS Secrets Manager, Kubernetes Secrets as options
- Managed identity for vault access without embedded credentials
- Secret rotation requiring redeploy or restart, not git removal

**Answer**

Production secrets should live in a dedicated secret store or platform injection mechanism, never in committed configuration files or repository history. Application code continues to consume secrets through `IConfiguration` and the Options pattern while deployment supplies values via environment variables, Azure Key Vault, AWS Secrets Manager, or Kubernetes Secrets. Remove secrets from `appsettings.Production.json` and keep only non-sensitive defaults and structure. Use managed identity or workload identity when connecting to Azure Key Vault (`AddAzureKeyVault`) rather than embedding vault credentials. Mount secrets as environment variables in containers (`Payment__ApiKey`) so the same binding code works across environments. Rotate compromised keys in the vault and redeploy or restart workloads; removing a secret from git does not invalidate leaked history.

---

## Q14. What is options validation (`ValidateDataAnnotations`, `ValidateOnStart`)?

**Concepts**
- `ValidateDataAnnotations()` applying attribute-based rules at options compute time
- `ValidateOnStart()` failing application startup on invalid options
- `IValidateOptions<TOptions>` for cross-property rules
- Fail-fast at startup preventing obscure runtime errors

**Answer**

Options validation runs registered validators against bound options instances to ensure required fields, ranges, and custom rules are satisfied before the application relies on them. `ValidateDataAnnotations()` applies attribute-based rules; `ValidateOnStart()` fails application startup if validation fails instead of deferring failure to the first consumer. Register with `builder.Services.AddOptions<PaymentOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart()`. Implement `IValidateOptions<TOptions>` for cross-property rules that annotations cannot express. `ValidateOnStart` is especially valuable for API keys, connection strings, and feature toggles whose absence would cause obscure runtime errors, since missing configuration is surfaced immediately rather than failing the first payment or database call in production.

---

## Q15. What is the difference between reading `configuration["Key"]` and injecting `IOptions<T>`?

**Concepts**
- `configuration["Key"]` — single string key, no compile-time checking
- `IOptions<T>` — typed, validated, registered once in DI
- Options supporting reload wrappers; raw indexer reading current merged value
- Raw `IConfiguration` appropriate in startup/bootstrap code only

**Answer**

Direct `IConfiguration` indexing reads a single string key at the call site, while `IOptions<T>` supplies a typed, bound object whose shape is validated and registered once in DI. `configuration["Payment:ApiKey"]` returns a string or null with no compile-time property checking, while `IOptions<PaymentOptions>.Value.ApiKey` is strongly typed and can carry validation attributes and default values on the POCO. Options support reload wrappers (snapshot/monitor); manual `configuration["Key"]` reads the current merged value but does not notify consumers automatically. Use `IConfiguration` in startup/bootstrap code; prefer options in application services and domain layers since `IOptions<T>` is trivial to mock with `Options.Create(new MySettings { ... })` while `IConfiguration` requires building an in-memory configuration tree.

---

## Q16. How does `appsettings.{Environment}.json` override base settings?

**Concepts**
- Environment file loaded after base file, keys overriding by last-provider-wins
- `ASPNETCORE_ENVIRONMENT` selecting the environment name
- Key-by-key recursive merge, not full document replacement
- Missing environment files skipped silently

**Answer**

The host loads `appsettings.json` first, then loads `appsettings.{Environment}.json` when the file exists, with the environment name taken from `ASPNETCORE_ENVIRONMENT`. Keys present in the environment file replace matching keys from the base file, while unspecified keys inherit base values since the override is key-by-key at the configuration provider level rather than a full document replacement — nested sections merge recursively. Setting `ASPNETCORE_ENVIRONMENT=Development` merges `appsettings.Development.json` over the base file. Production deployments typically set environment via hosting platform variables rather than editing files on disk. Missing environment files are skipped silently; only the base `appsettings.json` applies when no override file exists.

---

## Q17. What is `IConfigureOptions<T>`?

**Concepts**
- `IConfigureOptions<T>` mutating options after binding before consumers read
- Multiple implementations running in registration order
- `IConfigureNamedOptions<T>` for named variants
- `IPostConfigureOptions<T>` for adjustments after all configure delegates run

**Answer**

`IConfigureOptions<T>` is the extensibility hook that mutates a `TOptions` instance after binding and before consumers read it. Multiple implementations run in registration order, enabling modular libraries to contribute defaults or post-bind adjustments without a single monolithic `Configure` call. Implement `Configure(TOptions options)` or `IConfigureNamedOptions<T>` for named variants. Framework and libraries register configurators internally — for example, `JwtBearerOptions` setup from authentication extensions — so you rarely implement this interface directly in application code. Application code usually calls `services.Configure<T>(configuration.GetSection(...))`, which registers an internal `IConfigureOptions<T>` under the hood. Use `IPostConfigureOptions<T>` when values must be adjusted after all `IConfigureOptions` delegates run.

---

## Q18. Can singleton services safely use `IOptionsSnapshot<T>`? Why or why not?

**Concepts**
- `IOptionsSnapshot<T>` registered as scoped, recomputed per scope
- Captive dependency when injected into a singleton
- `ValidateScopes` throwing at startup on this combination
- `IOptionsMonitor<T>` as the singleton-safe alternative for live reload

**Answer**

No — `IOptionsSnapshot<T>` is registered with a scoped lifetime because it is recomputed per scope to reflect configuration reloads on each request. Injecting it into a singleton creates a captive dependency; with `ValidateScopes` enabled, startup fails with an invalid scope error. Singleton services should use `IOptions<T>` for static settings or `IOptionsMonitor<T>` when live reload is required. Controllers and scoped services are the intended consumers of `IOptionsSnapshot<T>`. If a singleton mistakenly resolves snapshot from the root provider without validation, behavior is undefined and may appear to work until the scope is disposed — at which point `.Value` throws. The options interfaces encode lifetime contracts — matching consumer lifetime to the correct options wrapper prevents subtle stale-config bugs.

---

## Gotchas — ASP.NET Core (Interview Traps)

---

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- `UseRouting` before `UseAuthentication` and `UseAuthorization`
- Endpoint metadata availability for auth middleware
- Correct pipeline order in `Program.cs`

**Answer**

In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing means the endpoint has not been selected yet, which breaks endpoint-aware authorization and policy resolution. The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`). Symptoms of wrong order include anonymous access to protected endpoints or 401 responses without proper challenge behavior, so always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive `DbContext` living past its scope
- Stale EF change tracker accumulating unrelated entities
- `ValidateScopes` as the detection mechanism

**Answer**

Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`. The singleton holds one scoped instance forever instead of one per request, so EF change trackers accumulate unrelated entities across requests. Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup, and fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation. This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- Socket exhaustion from per-use `HttpClient` instantiation
- `HttpMessageHandler` lifecycle managed by `IHttpClientFactory`
- Named and typed client registration pattern

**Answer**

Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected. `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` is an anti-pattern because the OS connection handle is held by the handler, not the client object. `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly; register named or typed clients with `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()`. Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- `IOptions<T>` — fixed snapshot at first resolution
- `IOptionsSnapshot<T>` — per-request recalculation, scoped
- `IOptionsMonitor<T>` — singleton-safe with change notifications
- Stale configuration when `.Value` is cached in a constructor field

**Answer**

`IOptions<T>` captures a configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled, because the wrapper holds the computed value without subscribing to change tokens. `IOptionsSnapshot<T>` recalculates per request scope so a singleton cannot inject it without creating a captive dependency. `IOptionsMonitor<T>` is the singleton-safe wrapper that supports change notifications via `OnChange` and exposes `CurrentValue` for the latest merged configuration. Misconfiguration persists silently until process restart when `.Value` was cached at construction, so singleton services that need live updates must use `IOptionsMonitor<T>`.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET body stripped by clients, proxies, and CDNs
- `[FromQuery]` with `[AsParameters]` for complex GET filters
- Silent binding failure rather than explicit error

**Answer**

Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production. Query strings and route values are the correct binding sources for GET requests, and complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys. Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development, since Swagger sends directly to local Kestrel without intermediate proxies. REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- Default `JsonNamingPolicy.CamelCase` in ASP.NET Core 8
- Silent binding to default values on case mismatch
- `PropertyNameCaseInsensitive` as a compatibility bridge

**Answer**

ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled. Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero) since System.Text.Json's default matching is exact-case on deserialization. Prefer standardizing clients on camelCase and documenting the contract in OpenAPI, and add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- `throw ex` resetting the stack trace to the catch block
- `throw;` preserving the original stack trace
- `InnerException` preservation when wrapping in a new exception

**Answer**

Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown. Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis, so always use `throw;` when rethrowing after logging or cleanup in a catch block. Wrap in a new exception only when adding context — `throw new OrderProcessingException("...", ex)` — to preserve `InnerException`. This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel as application server vs full edge gateway
- TLS termination, WAF, and rate limiting at the reverse proxy
- `UseForwardedHeaders` required for accurate client IP and scheme

**Answer**

Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require. Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front since TLS certificates are easier to manage at the proxy layer with automatic renewal. Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy, and containers typically bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- `launchSettings.json` as development-only launch configuration
- Production host using environment variables, not launch profiles
- `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS` as runtime configuration

**Answer**

Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts. Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings. Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments since the published application does not include or read the file. Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- Non-nullable `bool` defaulting to `false` on JSON omission
- Three-state intent: unspecified, opt-in, opt-out
- `bool?` or enum tri-state for partial-update DTOs

**Answer**

A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics. PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent, since a user omitting `sendNewsletter` should mean "leave as is" rather than "opt out". Marketing consent and feature flags are common domains where this bug causes compliance or logic errors, and nullable fields should be documented in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- `X-Forwarded-Proto` and `X-Forwarded-For` headers
- Wrong scheme causing broken HTTPS redirects and cookie secure flags
- `KnownProxies` configuration to prevent header spoofing

**Answer**

Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs. Call `UseForwardedHeaders()` early, before middleware that reads scheme or host such as HTTPS redirection, link generation, or rate limiting by IP. Configure `ForwardedHeadersOptions` to trust only your reverse proxy network since trusting all proxies enables header spoofing. Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- `UseStaticFiles()` serving all `wwwroot` contents unauthenticated
- Secrets and config files must stay outside the web root
- `appsettings.Production.json` in `wwwroot` as a critical security incident

**Answer**

Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default, so placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP. Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`, while sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers. An accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident since the file is served as a plain-text download. Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback returning `index.html` for unmatched routes including `/api/*`
- API endpoint registration ordering before fallback
- CORS and Swagger failures masked by HTML responses

**Answer**

SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers. Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`, and scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes. Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting, so the correct order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- Singleton `BackgroundService` incompatible with constructor-injected scoped services
- `CreateAsyncScope()` per job to create a fresh scope
- `ValidateScopes` catching this defect at startup

**Answer**

A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration, because hosted services live for the application lifetime and scoped dependencies must not be constructor-injected. Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes. The same rule applies to timers and `Task.Run` loops started from singletons, and enabling `ValidateScopes` catches this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR hub broadcasting to connected clients on the same instance only
- Redis or Azure Service Bus backplane for multi-node event routing
- Sticky sessions insufficient without a backplane

**Answer**

SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events. Sticky sessions keep one client on one node but do not route events raised on other nodes to that client, so adding a second instance without a backplane means events silently disappear for users on the wrong node. Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application, and test scale-out with at least two instances before launch rather than single-node staging alone.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (M) A service reads `configuration["Payment:ApiKey"]` at startup and caches it in a field. Ops rotates the key via environment variable override in Kubernetes without redeploying, but the app keeps using the old key. Explain how `IConfiguration` providers and precedence work, and why this pattern fails for hot reload.

**Concepts**
- Configuration provider chain with last-provider-wins precedence
- Field-cached value bypassing provider reload
- `IOptionsMonitor<T>` as the singleton-safe live-reload alternative
- Env var override static at pod start, not dynamic

**Answer**

`IConfiguration` merges providers in registration order — later providers override earlier keys for the same path — but values read once into a field are frozen because the field holds a copy, not a live reference to the provider. The default host order is `appsettings.json` → `appsettings.{Environment}.json` → User Secrets → environment variables → command-line args, so `Payment__ApiKey` in an env var overrides JSON correctly at startup. However, Kubernetes ConfigMap changes do not dynamically update env vars in running pods; the pod must be restarted to pick up the new value. Even if the provider did reload (as with a mounted file and `reloadOnChange: true`), a field cached in a constructor is not re-read. The fix is to inject `IOptionsMonitor<PaymentSettings>` and read `CurrentValue` at call time, or register `OnChange` to refresh outbound clients when configuration changes, though for secrets rotated via env var most deployments still require pod restart.

---

#### Q2. (M) Three consumers need settings from the same `appsettings.json` section: a singleton cache warmer, an MVC controller, and a background `IHostedService` that must react when `ReloadOnChange` updates the file. Which of `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` belongs in each, and what breaks if you swap them?

**Concepts**
- `IOptions<T>` for singleton consumers with static settings
- `IOptionsSnapshot<T>` for scoped controllers picking up reload per request
- `IOptionsMonitor<T>` for singleton hosted services reacting to file changes
- Captive dependency from swapping snapshot into a singleton

**Answer**

Use `IOptions<T>` for the cache warmer if settings are fixed for process lifetime, `IOptionsSnapshot<T>` in the scoped controller per request, and `IOptionsMonitor<T>` in the hosted service with `OnChange`. Injecting `IOptionsSnapshot<T>` into a singleton throws when `ValidateScopes` is enabled — classic captive dependency — since snapshot is a scoped service. Using `IOptions<T>` in the hosted service that must react to hot reload leaves stale values until restart since `IOptions<T>` captures once. Using `IOptionsMonitor<T>` in the controller works but is heavier than needed since controllers get a fresh scope per request anyway.

| Consumer | Abstraction | Why |
|---|---|---|
| Singleton cache warmer | `IOptions<T>` | One snapshot at first resolve; no per-request scope needed if reload is not required |
| MVC/API controller | `IOptionsSnapshot<T>` | Scoped per request; picks up config reload on next request without manual listeners |
| `IHostedService` reacting to file change | `IOptionsMonitor<T>` | Singleton-safe; `OnChange` callback updates background state without scoped services |

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

**Concepts**
- `IOptionsSnapshot<T>` scoped lifetime incompatible with singleton constructor injection
- `InvalidOperationException` at startup with `ValidateScopes` enabled
- Settings copied once in constructor — no reload even if snapshot worked
- `IOptionsMonitor<T>` as the correct singleton-safe fix

**Answer**

A singleton service cannot depend on `IOptionsSnapshot<T>` because snapshot is scoped — the app fails at startup with scope validation or creates a captive dependency without validation. Even if the constructor ran without throwing, `_settings` captures a copy of `.Value` at construction time so it would never reflect config reload, since the field is a value copy, not a live reference. The fix is to change the constructor to `IOptions<RateLimitSettings>` if limits are static, or `IOptionsMonitor<RateLimitSettings>` if reload is required — subscribing with `monitor.OnChange` to update the cached value. If per-request limits are truly needed, register `RateLimitService` as scoped, not singleton, and use `IOptionsSnapshot<T>` correctly.

---

#### Q4. (P) A developer commits `appsettings.Production.json` containing a Stripe secret key and enables User Secrets locally. Explain what is wrong for production secrets management and what you would use instead while still binding through the Options pattern.

**Concepts**
- Production secrets never in source-controlled JSON files
- User Secrets as Development-only, not deployed
- Azure Key Vault, AWS Secrets Manager, or env var injection for production
- Options pattern shape unchanged regardless of secret source

**Answer**

Production secrets must never live in source-controlled JSON — the key is now leaked in git history, which remains even after the file is edited. User Secrets apply only on a developer's machine (loaded only in Development via `UserSecretsId`) and do not exist on Azure App Service, Kubernetes, or IIS unless manually duplicated, which means they cannot substitute for production secret management. The fix is to remove secrets from all committed `appsettings*.json`, use placeholders or Key Vault references only, and supply real values via Azure Key Vault (`AddAzureKeyVault`), AWS Secrets Manager, Kubernetes Secrets mounted as env vars (`Stripe__SecretKey`), or CI/CD secret injection at deploy time. The consumption code is unchanged: `services.Configure<StripeOptions>(configuration.GetSection("Stripe"))` works regardless of whether the source is a file or a vault provider. Rotate the compromised key in the vault and restart pods; removing a secret from git does not invalidate the leaked value in history.

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

**Concepts**
- `Configure<T>` binding without validation — empty string and zero are valid CLR values
- `TimeSpan.FromSeconds(0)` causing immediate timeout on every call
- `ValidateDataAnnotations().ValidateOnStart()` as the fail-fast fix
- `[Required]` and `[Range]` attributes preventing invalid business values

**Answer**

`Configure<T>` binds without validation by default — empty strings and zero timeouts are valid CLR values — so invalid business rules slip through until runtime unless you add validation. `TimeSpan.FromSeconds(0)` on an `HttpClient` means every outgoing payment call times out immediately, appearing as flaky failures only under load rather than at startup. The fix is `AddOptions<PaymentOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart()`, combined with `[Required]` on `MerchantId` and `[Range(1, 300)]` on `TimeoutSeconds`:

```csharp
builder.Services.AddOptions<PaymentOptions>()
    .Bind(builder.Configuration.GetSection("Payment"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

This causes the application to fail at startup in staging with a clear validation error rather than failing on the first payment transaction in production.

---

#### Q6. (P) The app connects to two SQL databases — primary and read replica — each with its own connection string section. How do named options (`IOptionsSnapshot<DbConnectionOptions>` with `Configure<T>(name, ...)`) keep the registrations separate, and how does a repository resolve the correct one?

**Concepts**
- Named options preventing last-wins overwrite for same POCO type
- `Configure<T>("name", section)` for separate named registrations
- `IOptionsSnapshot<T>.Get("name")` resolving the correct named configuration
- `ValidateOnStart` per named registration for fail-fast validation

**Answer**

Register two named configurations against the same options type, then inject `IOptionsSnapshot<DbConnectionOptions>` and call `Get("Primary")` or `Get("Replica")` to obtain the correct connection string. Without names, the last `Configure<DbConnectionOptions>` wins — both repositories would share one connection string.

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

Named options also support `services.ConfigureAll<T>()` for shared defaults plus per-name overrides, and you can add `services.AddOptions<DbConnectionOptions>("Primary").Bind(...).ValidateOnStart()` to catch missing connection strings at startup rather than at the first database call.

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

**Concepts**
- `reloadOnChange: false` preventing `IChangeToken` from firing
- `OnChange` wiring correct but never triggered without file watcher
- Per-pod local `appsettings.json` causing rolling update timing skew
- `Interlocked.Exchange` for thread-safe `_currentLimit` updates

**Answer**

The `OnChange` wiring is correct but reload never fires because the JSON configuration source was not registered with `reloadOnChange: true` — without that flag, file edits on disk do not trigger the `IChangeToken` the monitor listens for, so `OnChange` never runs. In Kubernetes, not every pod sees the ConfigMap file update atomically at the same instant, which means pods briefly disagree on limits during rolling updates. The `_currentLimit` field is also updated from a background callback without `Interlocked`, which can cause torn reads under concurrent requests. The fix is to ensure `reloadOnChange: true` on the JSON source, use a cluster-wide dynamic config provider (Azure App Configuration, Consul) for consistent cross-pod limits, and make the update thread-safe with `Interlocked.Exchange(ref _currentLimit, settings.MaxRequests)`.

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

**Concepts**
- Middleware constructed once per application lifetime
- `IOptions<T>` capturing flag at construction, not per request
- `IOptionsMonitor<T>.CurrentValue` read inside `InvokeAsync` as the fix
- `Microsoft.FeatureManagement` as a fuller alternative

**Answer**

Middleware is constructed once per application lifetime; capturing `IOptions<T>.Value` in the constructor freezes the flag at startup, so changes in Azure App Configuration have no effect until the process restarts. The fix is to inject `IOptionsMonitor<FeatureFlags>` and read `_flags.CurrentValue.EnableBeta` inside `InvokeAsync` so the flag is evaluated per request against the latest merged configuration. Alternatively, use `Microsoft.FeatureManagement` (`IFeatureManager`) which integrates with App Configuration refresh and adds richer targeting rules. Any runbook for this service should document which flags require process recycle vs which support dynamic toggle, so operators have accurate expectations when pushing a configuration change.

---

#### Q9. (D) A junior developer injects `IConfiguration` directly into every service instead of `IOptions<T>`. When is direct `IConfiguration` acceptable, and when should you enforce the Options pattern with validation and named sections?

**Concepts**
- Direct `IConfiguration` acceptable in startup/bootstrap code
- `IOptions<T>` for typed, validated, testable settings in services
- `IConfiguration` requiring in-memory tree to mock vs `Options.Create(new T())`
- Composition root as the boundary for raw configuration access

**Answer**

Direct `IConfiguration` is fine for one-off keys in infrastructure glue — reading a single connection string in `Program.cs`, authoring a custom `IConfigurationSource`, or rare dynamic key lookups where section names are not known at compile time. Domain services should use validated `IOptions<T>` or `IOptionsMonitor<T>` so settings are typed, testable, and fail-fast. The key practical difference is testing: `IOptions<T>` is trivial to mock with `Options.Create(new MySettings { ... })`, while `IConfiguration` requires building an in-memory configuration tree. A useful team rule is that services take `IOptionsSnapshot<T>` or monitor, and only the composition root reads raw `IConfiguration` to call `Configure<T>()`. Avoid `configuration["Payment:ApiKey"]` string lookups in hot paths — a property typo silently returns null while a POCO property has a compile-time name.
