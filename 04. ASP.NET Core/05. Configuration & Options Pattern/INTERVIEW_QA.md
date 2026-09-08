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

## Gotchas — Configuration & Options Pattern (Interview Traps)

---

#### Gotcha 1. `IOptions<T>` vs reload

**Concepts**
- `IOptions<T>` — fixed snapshot at first resolution
- `IOptionsSnapshot<T>` — per-request recalculation, scoped
- `IOptionsMonitor<T>` — singleton-safe with change notifications
- Stale configuration when `.Value` is cached in a constructor field

**Answer**

`IOptions<T>` captures a configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled, because the wrapper holds the computed value without subscribing to change tokens. `IOptionsSnapshot<T>` recalculates per request scope so a singleton cannot inject it without creating a captive dependency. `IOptionsMonitor<T>` is the singleton-safe wrapper that supports change notifications via `OnChange` and exposes `CurrentValue` for the latest merged configuration. Misconfiguration persists silently until process restart when `.Value` was cached at construction, so singleton services that need live updates must use `IOptionsMonitor<T>`.

---

#### Gotcha 2. Production secrets committed to source-controlled `appsettings` files

**Concepts**

- Git history retaining committed secrets permanently even after deletion
- User Secrets as development-only, not deployed to production
- Key Vault, Secrets Manager, or env var injection for production values
- Options pattern shape unchanged regardless of secret source

**Answer**

Connection strings, API keys, and tokens committed to `appsettings.Production.json` or any JSON file in source control become permanently accessible in git history even after a subsequent commit removes them — the key is leaked and must be rotated immediately. User Secrets (`dotnet user-secrets`) are a development-only mechanism; they live under the developer's user profile and are never deployed, so they cannot substitute for production secret management. The fix is to remove secrets from all committed JSON files, store placeholders or Key Vault references instead, and supply real values via Azure Key Vault (`AddAzureKeyVault`), AWS Secrets Manager, Kubernetes Secrets mounted as env vars (`Stripe__SecretKey`), or CI/CD pipeline secret injection. The consuming code is unchanged — `services.Configure<StripeOptions>(configuration.GetSection("Stripe"))` works identically regardless of whether the value comes from a file or a vault provider.

---

#### Gotcha 3. `launchSettings.json` is not deployed — it does not set production behavior

**Concepts**

- `launchSettings.json` as development-only launch configuration
- Published application not including or reading the file
- `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS` as production runtime configuration

**Answer**

Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts and the published application does not read the file at all. Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings. Relying on `launchSettings.json` for production behavior leads to wrong environment selection or wrong port binding in deployed environments. Use `appsettings.Production.json` for non-secret defaults and host-level environment variables for environment selection and URL binding.

---

#### Gotcha 4. No `ValidateOnStart` — invalid configuration silently reaches the first customer call

**Concepts**

- `Configure<T>()` binding without validation by default
- Empty string and 0 as valid CLR values that pass binding silently
- `ValidateDataAnnotations()` applying `[Required]`, `[Range]`, `[Url]` attributes
- `ValidateOnStart()` failing startup rather than deferring to first consumer

**Answer**

`Configure<T>()` binds configuration without running any validation — empty strings, zero timeouts, and out-of-range values are accepted as valid CLR defaults, so an `appsettings` typo or a missing required key causes no startup error. The application launches, traffic arrives, and only the first payment, email, or database call throws, often with a misleading error that does not point to missing configuration. Pairing `AddOptions<T>().Bind(section).ValidateDataAnnotations().ValidateOnStart()` causes the host to fail at startup with a clear validation error that names the offending properties, surfacing misconfiguration in staging before it reaches production. Use `[Required]` on keys that must be present, `[Range]` on numeric bounds, and `IValidateOptions<T>` for cross-property rules that attribute annotations cannot express.

---

#### Gotcha 5. `IOptionsSnapshot<T>` injected into a singleton — captive dependency

**Concepts**

- `IOptionsSnapshot<T>` registered as scoped, recomputed per scope
- Captive dependency when snapshot injected into a singleton constructor
- `ValidateScopes` throwing at startup on this combination
- `IOptionsMonitor<T>` as the singleton-safe live-reload alternative

**Answer**

`IOptionsSnapshot<T>` is registered as a scoped service because it is designed to be recomputed per request scope to reflect configuration reloads. Injecting it into a singleton creates a captive dependency — the singleton outlives the scope, and with `ValidateScopes` enabled the application fails at startup. Without validation, the scoped snapshot is resolved from the root provider once and is never refreshed, silently defeating its reload semantics. Singleton services should use `IOptions<T>` for static settings or `IOptionsMonitor<T>` when live reload is required; `IOptionsMonitor<T>` is singleton-safe and exposes `CurrentValue` plus an `OnChange` callback without depending on a request scope.

---

#### Gotcha 6. `GetSection("Key")` returns an empty section — never null — for missing keys

**Concepts**

- `GetSection` returning a non-null empty `IConfigurationSection` when key is absent
- `.Exists()` as the correct presence check
- `GetRequiredSection` throwing when section is absent
- Section name typo silently leaving all properties at CLR defaults

**Answer**

`configuration.GetSection("Payment")` always returns a non-null `IConfigurationSection` regardless of whether the section exists. When the section is missing, `Exists()` returns `false` and `Bind(myObject)` leaves all properties at their CLR defaults without any error, so a typo in the section name (`"Payemnt"`) passes binding silently and every setting ends up at zero, empty, or false. Check existence with `section.Exists()` when the section is optional, use `GetRequiredSection("Payment")` (throws `InvalidOperationException` when absent) for mandatory sections, or rely on `ValidateDataAnnotations().ValidateOnStart()` to surface the default-value consequence as a validation failure at startup.

---

#### Gotcha 7. `reloadOnChange: false` on the JSON provider — `IOptionsMonitor.OnChange` never fires

**Concepts**

- `reloadOnChange: true` required to trigger `IChangeToken` on file edit
- `IOptionsMonitor.OnChange` wired correctly but never triggered without file watcher
- Custom-added providers not inheriting the default host `reloadOnChange` setting
- Vault providers with their own refresh mechanism independent of file watchers

**Answer**

If the `appsettings.json` provider is registered without `reloadOnChange: true`, file edits on disk never trigger an `IChangeToken` notification, so `IOptionsMonitor<T>.OnChange` callbacks and `IOptionsSnapshot<T>` reload on next request both silently stop working even though the wiring looks correct. The default `WebApplication.CreateBuilder` enables `reloadOnChange: true`, but custom template configurations or providers added with `builder.Configuration.AddJsonFile(path)` without the flag omit it. Verify configuration sources during debugging; enable `reloadOnChange: true` deliberately and document which flags require a pod restart versus dynamic toggle so operators have accurate expectations when pushing a configuration change.

---

#### Gotcha 8. Environment variable hierarchy separator — double underscore on Linux, not colon

**Concepts**

- Double underscore (`__`) as the cross-platform hierarchy separator for env vars
- Colon (`:`) accepted on Windows but not valid in Linux environment variable names
- Silent binding failure when key is not mapped to the correct hierarchy path
- Kubernetes ConfigMaps and CI environment blocks must use `__`

**Answer**

Hierarchical configuration keys in environment variables must use double underscores (`ConnectionStrings__DefaultConnection`) not colons because the colon character is not a valid environment variable name character on Linux. The colon works on Windows where the env var provider accepts it, so teams that develop on Windows and deploy to Linux containers find that `ConnectionStrings:DefaultConnection` binds correctly in local testing but silently fails in production — the variable is treated as a flat non-hierarchical key that matches nothing in the configuration tree. Always use `__` in Kubernetes ConfigMaps, GitHub Actions `env:` blocks, Docker Compose environment sections, and any cross-platform CI script for hierarchical configuration keys.

---

#### Gotcha 9. Named options `.Value` resolves the default name — `.Get("name")` required for named registrations

**Concepts**

- `Options.DefaultName` (empty string) as the key for unnamed `Configure<T>()` registrations
- `.Value` always resolving the default name regardless of named registrations
- `.Get("name")` required to resolve a named options registration
- Last-wins for multiple unnamed `Configure<T>()` calls on the same type

**Answer**

`IOptionsSnapshot<T>.Value` and `IOptions<T>.Value` always resolve the options registered under `Options.DefaultName` (the empty string), not any named registration. When you register two named options — `Configure<T>("primary", ...)` and `Configure<T>("secondary", ...)` — and then read `.Value`, you get an unbound instance with all properties at CLR defaults because no unnamed registration was made. Named options must always be resolved with `.Get("primary")` or `.Get("secondary")`. Additionally, multiple unnamed `Configure<T>()` calls accumulate in registration order and the factory merges all of them; a second unnamed registration overwrites the same property values in order, so last one wins — which surprises teams expecting only one to apply.

---

#### Gotcha 10. `ASPNETCORE_ENVIRONMENT` vs `DOTNET_ENVIRONMENT` — wrong variable for worker services

**Concepts**

- `DOTNET_ENVIRONMENT` read by the generic host and worker services
- `ASPNETCORE_ENVIRONMENT` read by `WebApplication` and ASP.NET Core web host
- Worker services silently staying in Production when `ASPNETCORE_ENVIRONMENT` is set instead
- `WebApplication.CreateBuilder` reading both, with `ASPNETCORE_ENVIRONMENT` taking precedence

**Answer**

The generic host (worker services, console apps) reads `DOTNET_ENVIRONMENT` to select the environment, while the ASP.NET Core web host reads `ASPNETCORE_ENVIRONMENT`. When deploying a worker service and setting `ASPNETCORE_ENVIRONMENT=Development`, the host ignores it — the environment remains Production, `appsettings.Development.json` does not load, and User Secrets are not applied. This is a common copy-paste mistake from web app deployment scripts applied to worker service containers. Use `DOTNET_ENVIRONMENT` for generic host workloads; `WebApplication.CreateBuilder` reads both and `ASPNETCORE_ENVIRONMENT` takes precedence when both are set, so using `DOTNET_ENVIRONMENT` universally is the safest cross-project approach.

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
