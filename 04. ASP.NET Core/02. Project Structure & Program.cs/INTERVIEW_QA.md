# Project Structure & Program.cs — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is the purpose of `Program.cs` in an ASP.NET Core application?](#q1-what-is-the-purpose-of-programcs-in-an-aspnet-core-application)
2. [Q2. What are the two phases of an ASP.NET Core startup — builder phase and app phase?](#q2-what-are-the-two-phases-of-an-aspnet-core-startup--builder-phase-and-app-phase)
3. [Q3. What does `WebApplication.CreateBuilder(args)` return and configure?](#q3-what-does-webapplicationcreatebuilderargs-return-and-configure)
4. [Q4. What happens when you call `builder.Build()`?](#q4-what-happens-when-you-call-builderbuild)
5. [Q5. What happens when you call `app.Run()`?](#q5-what-happens-when-you-call-apprun)
6. [Q6. What is `launchSettings.json`, and does it apply in production?](#q6-what-is-launchsettingsjson-and-does-it-apply-in-production)
7. [Q7. How does ASP.NET Core load `appsettings.json` and environment-specific overrides?](#q7-how-does-aspnet-core-load-appsettingsjson-and-environment-specific-overrides)
8. [Q8. What is the default configuration provider precedence order?](#q8-what-is-the-default-configuration-provider-precedence-order)
9. [Q9. What is the difference between registering services and registering middleware?](#q9-what-is-the-difference-between-registering-services-and-registering-middleware)
10. [Q10. Why must `MapControllers()` or `MapGet()` be called for endpoints to work?](#q10-why-must-mapcontrollers-or-mapget-be-called-for-endpoints-to-work)
11. [Q11. What is `ASPNETCORE_URLS`, and how does it relate to Kestrel binding?](#q11-what-is-aspnetcore_urls-and-how-does-it-relate-to-kestrel-binding)
12. [Q12. What is the purpose of `Properties/launchSettings.json` profiles?](#q12-what-is-the-purpose-of-propertieslaunchsettingsjson-profiles)
13. [Q13. How do you organize a growing `Program.cs` without losing clarity?](#q13-how-do-you-organize-a-growing-programcs-without-losing-clarity)
14. [Q14. What is the difference between `Startup.cs` and putting everything in `Program.cs`?](#q14-what-is-the-difference-between-startupcs-and-putting-everything-in-programcs)
15. [Q15. When do misconfigured DI registrations typically surface?](#q15-when-do-misconfigured-di-registrations-typically-surface)
16. [Q16. What is the `WebApplication` type?](#q16-what-is-the-webapplication-type)
17. [Q17. How does `builder.Environment` differ from reading config manually?](#q17-how-does-builderenvironment-differ-from-reading-config-manually)
18. [Q18. What files are typically part of a new ASP.NET Core Web API project structure?](#q18-what-files-are-typically-part-of-a-new-aspnet-core-web-api-project-structure)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is the purpose of `Program.cs` in an ASP.NET Core application?

**Concepts**
- `Program.cs` as single composition root — all startup wiring in one place
- Service registration vs middleware pipeline phases
- Top-level statements eliminating `Main` boilerplate in .NET 6+
- Explicit feature registration — no implicit behavior unlike `Global.asax`

**Answer**

The reason `Program.cs` is the application entry point is that modern ASP.NET Core 6+ unified host setup into a single top-level file using top-level statements, so every cross-cutting concern — services, configuration, middleware, and endpoints — is registered explicitly in one place. I call `WebApplication.CreateBuilder(args)` to start configuring services, then `builder.Build()` to get a runnable `WebApplication`, then chain `Use*` and `Map*` calls to shape the HTTP pipeline, and finally `app.Run()` to block and accept connections. Unlike the old `Global.asax`, nothing activates implicitly — if I forget to call `MapControllers()`, routes simply don't exist.

---

## Q2. What are the two phases of an ASP.NET Core startup — builder phase and app phase?

**Concepts**
- `WebApplicationBuilder` — service registration before service provider exists
- `builder.Build()` finalizing `IServiceProvider` and returning `WebApplication`
- Middleware registration on `WebApplication` after `Build()`
- `ValidateOnBuild` — fail-fast DI graph validation at startup

**Answer**

The builder phase and the app phase are two distinct moments separated by `builder.Build()`. During the builder phase I call `builder.Services.Add*` to register dependencies into the DI container — nothing is resolved yet, the host doesn't exist, and the pipeline has no shape. Calling `builder.Build()` freezes the service collection into a root `IServiceProvider`, optionally validates the DI graph if `ValidateOnBuild` is enabled, and returns a `WebApplication`. After that I enter the app phase and call `app.Use*` for middleware and `app.Map*` for endpoints — order here is fixed and determines execution sequence. Any code that needs a built service provider must run in the app phase or inside a request handler, not during service registration.

---

## Q3. What does `WebApplication.CreateBuilder(args)` return and configure?

**Concepts**
- `WebApplicationBuilder` return type
- Default configuration source loading — `appsettings.json`, env-specific, env vars, args
- Content root and web root defaults from project directory
- Core framework services auto-registered before any manual additions

**Answer**

Calling `WebApplication.CreateBuilder(args)` returns a `WebApplicationBuilder` that has already pre-wired Kestrel, set the content root to the project directory, loaded `appsettings.json` and `appsettings.{Environment}.json`, added environment variables and command-line arguments to configuration, and registered core framework services including routing and logging. Passing `args` matters because it enables command-line overrides and launch-profile settings to reach the configuration pipeline. It replaces the older, more verbose `Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(...)` chain from .NET 3–5.

---

## Q4. What happens when you call `builder.Build()`?

**Concepts**
- Service collection frozen into root `IServiceProvider`
- `ValidateOnBuild` — missing registration detection at startup
- `WebApplication` implementing `IHost`, `IApplicationBuilder`, `IEndpointRouteBuilder`
- `Map*` extension methods available on `WebApplication` after build

**Answer**

Calling `builder.Build()` constructs the `WebApplication` host, converts the `IServiceCollection` into a root `IServiceProvider`, and prepares the middleware pipeline for configuration. The key boundary is that after this call the service collection is frozen — registering more services afterward has no effect. If `ValidateOnBuild` is enabled (via `builder.Host.UseDefaultServiceProvider(o => o.ValidateOnBuild = true)`), the runtime tries to resolve every registered service at build time and throws immediately on missing dependencies rather than waiting for the first request. The returned `WebApplication` implements `IEndpointRouteBuilder`, which is why `MapGet`, `MapControllers`, and `MapHub` work on it directly.

---

## Q5. What happens when you call `app.Run()`?

**Concepts**
- `app.Run()` blocking the main thread until shutdown
- Kestrel URL binding from `ASPNETCORE_URLS` and configuration
- Graceful shutdown via SIGTERM, Ctrl+C, or `IHostApplicationLifetime`
- `app.RunAsync()` as non-blocking equivalent

**Answer**

Calling `app.Run()` finalizes the middleware pipeline, causes Kestrel to bind to configured URLs, and blocks the main thread until the application shuts down. Before blocking it locks in the sequence of all prior `Use*` and `Map*` registrations, so no further pipeline changes are possible after this call. The URLs come from `ASPNETCORE_URLS`, Kestrel configuration in `appsettings.json`, or `launchSettings.json` profiles when running locally. Shutdown is triggered by Ctrl+C, SIGTERM from a container orchestrator, or programmatic `IHostApplicationLifetime.StopApplication()`, and Kestrel drains in-flight requests during the configured graceful shutdown window before the process exits.

---

## Q6. What is `launchSettings.json`, and does it apply in production?

**Concepts**
- `launchSettings.json` — IDE and `dotnet run` artifact only, never deployed
- Per-profile `applicationUrl`, `environmentVariables`, and browser launch settings
- `ASPNETCORE_URLS` and `ASPNETCORE_ENVIRONMENT` as production replacements
- Development HTTPS certificate not present in deployed environments

**Answer**

The key misunderstanding about `launchSettings.json` is that developers often treat it as runtime configuration when it is purely a development convenience file that Visual Studio, VS Code, and `dotnet run` consume — it never ships to production. It stores named profiles that set environment variables like `ASPNETCORE_ENVIRONMENT` and the local URL, which is why I can switch between http and https profiles for debugging. Deployed applications get their environment name from `ASPNETCORE_ENVIRONMENT` set in the container manifest or App Service configuration, and their URLs from `ASPNETCORE_URLS` or Kestrel endpoint settings — not from this file.

---

## Q7. How does ASP.NET Core load `appsettings.json` and environment-specific overrides?

**Concepts**
- Configuration provider stacking — later providers override earlier ones for same key
- `appsettings.{Environment}.json` conditional loading based on `ASPNETCORE_ENVIRONMENT`
- `reloadOnChange: true` — hot config reload without restart
- Optional environment-specific files — silent skip if absent

**Answer**

The configuration system in ASP.NET Core is a stack of providers where later providers override earlier ones for matching keys. `WebApplication.CreateBuilder` adds `appsettings.json` first, then `appsettings.{Environment}.json` — so whatever `ASPNETCORE_ENVIRONMENT` is set to determines which override file loads. Environment-specific files are optional and silently skipped if absent, so a missing `appsettings.Production.json` doesn't cause a startup failure. The `reloadOnChange: true` default means a file change on disk triggers a config reload without restarting the process, which is useful for log level adjustments and feature flags I don't want to require a redeploy to change.

---

## Q8. What is the default configuration provider precedence order?

**Concepts**
- Provider stack — lowest to highest: JSON files → user secrets → env vars → command-line args
- Double-underscore env var convention mapping to colon-separated keys
- User secrets available in Development environment only
- Command-line args as highest-priority override for CI and container deployments

**Answer**

Configuration precedence runs lowest to highest: `appsettings.json`, then `appsettings.{Environment}.json`, then user secrets in Development, then environment variables, then command-line arguments. This means environment variables always win over JSON files, which is why production deployments set `ConnectionStrings__Default` as an env var to override the placeholder in `appsettings.json`. The double-underscore in env var names maps to the colon separator in configuration keys, so `Logging__LogLevel__Default` resolves the same path as `Logging:LogLevel:Default`. When debugging a wrong config value I work backwards from command-line args to env vars before checking JSON files.

---

## Q9. What is the difference between registering services and registering middleware?

**Concepts**
- `builder.Services.Add*` — DI container registration, before `Build()`
- `app.Use*` — HTTP pipeline registration, after `Build()`
- Extension method namespaces — `IServiceCollection` vs `IApplicationBuilder`/`WebApplication`
- Middleware consuming DI-registered services at request time

**Answer**

Service registration and middleware registration happen in separate phases and use entirely different extension surfaces. Service registration happens before `builder.Build()` by calling extension methods on `builder.Services`, which is an `IServiceCollection` — I use it to register controllers, database contexts, typed HTTP clients, and everything the application needs injected. Middleware registration happens on the `WebApplication` instance after `Build()`, using `app.Use*` methods that add components to the HTTP pipeline in order. A middleware component like authentication can itself depend on DI services, but it is registered on the pipeline, not in the container. Confusing the two causes compiler errors because `IServiceCollection` and `WebApplication` have distinct extension method namespaces.

---

## Q10. Why must `MapControllers()` or `MapGet()` be called for endpoints to work?

**Concepts**
- `AddControllers()` — DI registration only, no HTTP routes exposed
- `MapControllers()` — connecting controller routes to the routing and endpoint system
- Endpoint routing — routes as rich metadata objects, not just URL patterns
- Missing `Map*` as root cause of all-404 APIs

**Answer**

Calling `AddControllers()` only registers MVC services in the DI container — it does not expose any HTTP routes. The routes become active only when `MapControllers()` (or the minimal API equivalent `MapGet`, `MapPost`, etc.) is called on the `WebApplication`, because that step attaches endpoint metadata to the routing system. The reason this distinction matters is that endpoint routing treats routes as rich metadata objects that authorization middleware, OpenAPI generators, and link generators all read from — so the mapping step is what makes endpoints visible to the entire pipeline, not just to URL matching. Forgetting `MapControllers()` produces a server that accepts connections, runs all middleware, and silently returns 404 for every request.

---

## Q11. What is `ASPNETCORE_URLS`, and how does it relate to Kestrel binding?

**Concepts**
- `ASPNETCORE_URLS` — environment variable overriding Kestrel addresses
- Semicolon-separated multiple address binding
- IIS in-process hosting ignoring Kestrel URL binding
- `http://+:PORT` for container deployments with external TLS termination

**Answer**

Setting `ASPNETCORE_URLS` is the standard way to tell Kestrel which addresses to bind without changing application code. I use semicolons to bind multiple addresses: `http://0.0.0.0:8080;https://0.0.0.0:8443`, which is the pattern container deployments use when the ingress controller handles TLS termination while the app listens on plain HTTP internally. This environment variable overrides the `applicationUrl` from `launchSettings.json` in development, and Kestrel endpoint configuration in `appsettings.json` can further override it depending on configuration load order. IIS in-process hosting is the exception — IIS owns the HTTP port and Kestrel URL binding is ignored there.

---

## Q12. What is the purpose of `Properties/launchSettings.json` profiles?

**Concepts**
- Launch profiles — grouped settings for IDE and `dotnet run` startup
- Per-profile `applicationUrl`, `environmentVariables`, and browser launch behavior
- `dotnet run --launch-profile` for profile selection at CLI
- Profiles as dev ergonomics, not security boundaries or runtime configuration

**Answer**

Launch profiles exist purely to make local development consistent across a team's machines and IDEs. Each profile in `launchSettings.json` specifies which URLs to listen on, which environment name to use, and which environment variables to inject — so switching between an https profile with dev certs and an http profile without them is a matter of picking a profile rather than editing code. Running `dotnet run --launch-profile https` applies that profile for the session. Profiles are not security boundaries and should never contain secrets since the file is committed to source control and visible to everyone with repository access.

---

## Q13. How do you organize a growing `Program.cs` without losing clarity?

**Concepts**
- Extension methods for DI grouping — `Add*` per layer or feature
- Preserving middleware order visibility in `Program.cs` or a single block
- Feature modules or vertical slice DI organization
- Hidden ordering as a security and correctness risk

**Answer**

The goal when `Program.cs` grows is to keep middleware order visible while moving DI boilerplate into well-named extension methods. I extract service registration groups into static methods like `builder.Services.AddInfrastructure(config)`, `AddApplicationServices()`, and `AddWebApiServices()` — each lives in the layer that owns those registrations. For the pipeline I'm more conservative: I keep the `Use*` calls in `Program.cs` itself or in a single `UseApplicationPipeline(app)` method whose body is one ordered block, because reordering auth relative to routing across file boundaries is a common source of production incidents. Assembly scanning for validators, background services, and option configurations reduces manual registration lines without obscuring the pipeline order that reviewers need to audit.

---

## Q14. What is the difference between `Startup.cs` and putting everything in `Program.cs`?

**Concepts**
- `Startup.cs` separating `ConfigureServices` and `Configure` into named methods
- Minimal hosting `Program.cs` — both phases in one file using top-level statements
- `WebApplicationFactory<Program>` for integration test setup in minimal hosting
- Neither choice changes runtime behavior when configured equivalently

**Answer**

Both `Startup.cs` and the minimal `Program.cs` approach configure exactly the same host — the difference is organizational. `Startup.cs` separates service registration (`ConfigureServices`) from middleware configuration (`Configure`) into a dedicated class, which some teams prefer because it maps naturally to the two-phase mental model and allows test harnesses to substitute or wrap the startup class. Modern templates use minimal `Program.cs` with top-level statements, which is less boilerplate and uses `WebApplicationFactory<Program>` for integration testing rather than a startup class reference. I default to minimal `Program.cs` for new projects and reach for `Startup.cs` only when the team has an established pattern or when the integration test fixture needs to override startup logic by subclassing.

---

## Q15. When do misconfigured DI registrations typically surface?

**Concepts**
- Lazy DI resolution — errors on first request by default, not at startup
- `ValidateOnBuild` — forcing resolution at `Build()` for early failure
- `ValidateScopes` — captive dependency detection in Development
- Background service errors at `StartAsync`, not on HTTP request traffic

**Answer**

By default, DI registration errors surface lazily — they throw on the first HTTP request that tries to construct a controller or service with a missing or mis-wired dependency. This means a misconfigured registration that isn't hit in CI tests can reach production silently. Enabling `ValidateOnBuild` in `builder.Host.UseDefaultServiceProvider(o => o.ValidateOnBuild = true)` forces the runtime to attempt resolving every registered service at `Build()` time, converting startup failures into immediate crashes rather than intermittent per-request errors. `ValidateScopes` catches captive dependency bugs where a scoped service like `DbContext` is injected into a singleton — that surfaces either at build time with validation on, or on the first request that reaches that singleton without it.

---

## Q16. What is the `WebApplication` type?

**Concepts**
- `WebApplication` implementing `IHost`, `IApplicationBuilder`, `IEndpointRouteBuilder`
- Unified host replacing separate `IHost` and `IWebHost` from earlier versions
- `app.Services` providing root service provider access after `Build()`
- `Map*` methods as extension methods on `IEndpointRouteBuilder`

**Answer**

The `WebApplication` type is ASP.NET Core 6's unified host that collapses what were previously two separate objects — `IHost` for generic host lifetime and `IWebHost` for the HTTP pipeline — into one. Because it implements `IApplicationBuilder`, I can call `app.Use*` middleware methods on it directly, and because it implements `IEndpointRouteBuilder`, `MapGet`, `MapControllers`, and `MapHub` work on the same object without ceremony. After `builder.Build()` returns a `WebApplication`, I access `app.Services` for the root service provider to run any setup code that needs a resolved service, then call `app.Run()` to start Kestrel.

---

## Q17. How does `builder.Environment` differ from reading config manually?

**Concepts**
- `IWebHostEnvironment` — hosting context: environment name, content root, web root paths
- `IsDevelopment()`, `IsProduction()` — environment-name helper methods
- `IConfiguration` — key-value settings from files, env vars, and other sources
- Environment driving file providers for `appsettings` and `wwwroot` location

**Answer**

`builder.Environment` (`IWebHostEnvironment`) exposes hosting context — the environment name, physical paths for content root and web root, and helper methods like `IsDevelopment()`. Configuration (`builder.Configuration`) exposes key-value settings loaded from files, environment variables, and other sources. Reading `configuration["ASPNETCORE_ENVIRONMENT"]` is not equivalent to `builder.Environment.EnvironmentName` because the canonical environment name is set by the runtime at host initialization, while it may or may not be represented consistently as a plain config key depending on the setup. The environment object also provides the file providers that locate `appsettings.json` and `wwwroot`, so physical path resolution depends on `Environment`, not on arbitrary configuration values.

---

## Q18. What files are typically part of a new ASP.NET Core Web API project structure?

**Concepts**
- Default template files: `Program.cs`, `appsettings*.json`, `.csproj`, `launchSettings.json`
- `.http` file for inline REST Client requests in .NET 8+ templates
- `GlobalUsings.cs` and implicit usings reducing boilerplate using statements
- `Controllers/` folder vs minimal API approach — both scaffolded by different templates

**Answer**

A new Web API project from `dotnet new webapi` includes `Program.cs`, `appsettings.json`, `appsettings.Development.json`, the `.csproj` targeting `net10.0`, `Properties/launchSettings.json`, and either a sample controller or a minimal API endpoint. Recent templates add a `.http` file giving the team REST Client sample requests alongside the code so endpoints can be tested without opening Swagger. `GlobalUsings.cs` and the SDK's implicit usings eliminate most boilerplate `using` statements, so new files need fewer explicit imports. The template deliberately excludes docker files, test projects, and domain layers — those are added as the project evolves rather than scaffolded by default.

---

## Gotchas — Project Structure & Program.cs (Interview Traps)

---

#### Gotcha 1. Services registered after `builder.Build()` are lost or throw

**Concepts**
- DI container sealed at `builder.Build()` — registrations must precede it
- `app.Services` for resolution only, not registration
- `InvalidOperationException` or silent no-op when adding services post-build
- Extension methods on `IServiceCollection` for organized pre-build registration

**Answer**

`builder.Build()` compiles the DI container into an immutable `IServiceProvider`. Any call to `builder.Services.Add*()` after `Build()` either throws `InvalidOperationException` or registers into a collection that is never used. The `app` variable exposes `app.Services` only for resolving services. Teams organizing large applications should use `IServiceCollection` extension methods — `builder.Services.AddOrderModule()` — that all run before `Build()`. Middleware factories or conditional registrations that add services too late are a common source of confusing `InvalidOperationException: Unable to resolve service` errors that appear only under specific request paths.

---

#### Gotcha 2. Middleware registration order in `Program.cs` directly controls execution order

**Concepts**
- `app.Use*()` calls forming an ordered delegate chain
- Exception handler must be outermost (registered first)
- `UseRouting` before `UseAuthentication` and `UseAuthorization`
- `UseStaticFiles` before routing to prevent static paths being matched as API routes

**Answer**

In the minimal hosting model, the order of `app.Use*()` calls in `Program.cs` determines execution order for every HTTP request. Exception handling middleware must be registered first so it wraps the entire pipeline and catches exceptions from any subsequent step. Authentication must follow routing so the matched endpoint's metadata is available for policy evaluation. Registering `UseAuthorization()` before `UseRouting()` silently breaks endpoint-based authorization — the endpoint hasn't been selected yet when the authorization check runs. Review middleware order in `Program.cs` during every code review; it is the single most impactful architectural decision in the file.

---

#### Gotcha 3. Top-level statements create an implicit `Program` class that must be made public for tests

**Concepts**
- Top-level statement compilation producing implicit `internal Program` class
- `WebApplicationFactory<Program>` failing to reference `internal` type
- `public partial class Program { }` at bottom of `Program.cs` as the fix
- Partial class merging with the compiler-generated implicit class

**Answer**

When `Program.cs` uses top-level statements, the C# compiler generates an implicit `Program` class that is `internal` by default. Integration tests using `WebApplicationFactory<Program>` fail with a compile error because the test project cannot reference an `internal` type in another assembly. The fix is to add `public partial class Program { }` at the bottom of `Program.cs`, which makes the class public and partial, allowing the compiler to merge it with the generated class and giving test projects visibility. Forgetting this single line is the most common setup issue when creating integration tests for minimal hosting model projects.

---

#### Gotcha 4. `AddControllers()` registers services but `MapControllers()` is required to activate routes

**Concepts**
- `AddControllers()` — registers MVC infrastructure in DI
- `MapControllers()` — scans controllers and registers endpoint routing
- 404 response with no error when `MapControllers()` is omitted
- Split between registration and activation intentional in minimal model

**Answer**

`builder.Services.AddControllers()` registers the MVC infrastructure — formatters, model binding, validation, filters — in the DI container, but it does not create any routes. Routes are activated by calling `app.MapControllers()` in the middleware pipeline, which scans controller classes for `[Route]`, `[HttpGet]`, and `[ApiController]` attributes and registers endpoint metadata. Omitting `MapControllers()` causes every request to return 404 with no log entry explaining why. This explicit separation between registration and activation is intentional in the minimal hosting model, but regularly trips up developers migrating from the conventional `Startup.Configure` model where both steps were more tightly coupled.

---

#### Gotcha 5. `AddControllers()` vs `AddControllersWithViews()` vs `AddMvc()` — pick the smallest footprint

**Concepts**
- `AddControllers()` — API only, no Razor view engine
- `AddControllersWithViews()` — controllers plus Razor view rendering
- `AddMvc()` — controllers, views, and Razor Pages
- Unnecessary Razor view engine overhead in API-only projects

**Answer**

`AddControllers()` registers only the services needed for Web API controllers — `IActionResult` responses, model binding, validation, and formatters — without the Razor view engine. `AddControllersWithViews()` adds Razor view rendering. `AddMvc()` includes everything, including Razor Pages, which is unnecessary overhead when the project only serves JSON. Using `AddMvc()` in an API-only project increases startup time and the surface area of infrastructure that activates. Pick `AddControllers()` for API projects, `AddControllersWithViews()` for MVC applications that also expose APIs, and `AddMvc()` only when Razor Pages are genuinely used.

---

#### Gotcha 6. `app.Services.CreateScope()` at startup must be disposed — root-scope resolution causes captive dependency

**Concepts**
- Root `IServiceProvider` as singleton lifetime container
- Scoped services resolved from root becoming lifetime singletons
- `await using var scope = app.Services.CreateAsyncScope()` pattern
- EF Core `DbContext` migration and seeding at startup

**Answer**

Running database migrations or seeding at startup requires a scoped `DbContext`, but the root `IServiceProvider` (`app.Services`) is effectively a singleton container — resolving a scoped service directly from it creates a captive dependency held for the application lifetime. The correct pattern is `await using var scope = app.Services.CreateAsyncScope()`, then `scope.ServiceProvider.GetRequiredService<AppDbContext>()`. Failing to dispose the scope means the `DbContext` and all tracked entities remain alive until process exit, which is a resource leak when repeated or used in tests. Always use the `await using` block so disposal is guaranteed even when the seeding logic throws.

---

#### Gotcha 7. `appsettings.{Environment}.json` overrides — arrays are replaced, not merged

**Concepts**
- JSON configuration layering — key-level override, not file replacement
- Array sections replaced entirely by environment-specific file
- Logging providers, CORS origins, and AllowedHosts stored in arrays
- `IConfiguration.GetSection` returning merged scalar values

**Answer**

`appsettings.Production.json` is layered on top of `appsettings.json` at the key level — it does not replace the entire base file. Scalar keys in the environment file override matching base keys; all other base keys remain active. For arrays, the behavior is different: a JSON array in `appsettings.Production.json` replaces the entire array from `appsettings.json` for that section, not merges element-by-element. Teams that store logging providers, CORS origins, or `AllowedHosts` as arrays are surprised to find the production file wipes the base array rather than extending it. Understanding the full precedence order — base → environment → environment variables → command-line — prevents unexpected configuration behavior.

---

#### Gotcha 8. `Properties/launchSettings.json` is never deployed and has no effect in production

**Concepts**
- `launchSettings.json` consumed by `dotnet run`, Visual Studio, VS Code only
- Excluded from `dotnet publish` output — not present on deployed host
- Production URLs via `ASPNETCORE_URLS` environment variable
- `ASPNETCORE_ENVIRONMENT` set at the host or container level

**Answer**

`Properties/launchSettings.json` stores launch profiles for local development — `applicationUrl`, environment variable overrides, and Swagger launch settings — and is explicitly excluded by `dotnet publish`. Deployed applications never read it. Production URLs must be configured via `ASPNETCORE_URLS` or Kestrel options in `appsettings.json`; the environment name must be set through the host's environment variable system (container manifest, App Service configuration, or systemd unit file). Discovering this at first deployment — when the app starts on a different port or in the wrong environment — is common and avoidable by validating host configuration separately from launch profiles.

---

#### Gotcha 9. Reading `IConfiguration` directly bypasses validation — prefer `IOptions<T>` with `ValidateOnStart`

**Concepts**
- `IConfiguration["Key"]` returning `null` silently when key is absent
- `IOptions<T>` binding with type conversion and data annotation validation
- `ValidateDataAnnotations()` and `ValidateOnStart()` for eager startup validation
- Missing configuration surfaced at startup, not at the first runtime use

**Answer**

Reading configuration via `IConfiguration["ConnectionStrings:Default"]` returns `null` with no error when the key is absent, so a missing connection string only surfaces as `NullReferenceException` when the first database operation runs. Binding configuration to `IOptions<T>` classes and calling `ValidateDataAnnotations().ValidateOnStart()` causes the host to throw at startup if required configuration is absent or malformed, surfacing the error before any request is served. `ValidateOnStart()` is the key addition — without it, `IOptions<T>` validates lazily on first access rather than at startup. This is the preferred pattern for all application configuration in production services.

---

#### Gotcha 10. `builder.Configuration` vs `app.Configuration` — both work but serve different phases

**Concepts**
- `builder.Configuration` — mutable `IConfigurationBuilder` for adding sources
- `app.Configuration` — resolved `IConfiguration` for reading values
- Adding sources after `Build()` has no effect on the built configuration
- `AddEnvironmentVariables` and `AddCommandLine` added automatically by default builder

**Answer**

`WebApplicationBuilder` exposes `builder.Configuration` as a mutable `IConfigurationBuilder` where additional sources — custom JSON files, Azure Key Vault, AWS Parameter Store — can be added before `Build()`. After `Build()` is called, `app.Configuration` is the built, immutable `IConfiguration` for reading values. Calling `app.Configuration.AddJsonFile(...)` after build does nothing because the `IConfiguration` object is already finalized. The `WebApplication.CreateBuilder` defaults already include `appsettings.json`, `appsettings.{env}.json`, environment variables, and command-line arguments — custom sources should be additive, and `builder.Configuration.Sources.Clear()` is available to reset them if needed.

---

## Scenario-Based Questions (Karat Format)

---

## Q1. (R) You add a new API controller with `[ApiController]`, add `builder.Services.AddControllers()`, configure Swagger, then `app.Run()`. Every request returns 404. What did you forget, and how does that affect authorization behavior too?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.Run();
```

**Concepts**
- `AddControllers()` — DI registration only, no routes exposed
- `MapControllers()` — required to connect controller routes to routing system
- `UseAuthorization` without routing context — endpoint metadata unavailable
- `UseAuthentication` missing before `UseAuthorization`

**Answer**

Services and Swagger are registered but the pipeline never calls `app.MapControllers()`, which means the routing system has no controller endpoint data sources and every request falls through unmatched with 404. The `UseAuthorization()` call is also problematic because there's no `UseAuthentication()` call before it and no routing to provide endpoint metadata, so authorization middleware has no principal to evaluate and no policy metadata to read. The fix is to add explicit routing, authentication, and endpoint mapping in the correct order:

```csharp
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## Q2. (R) A junior developer tries to call `app.UseHttpsRedirection()` on `builder.Services` and `builder.Services.AddControllers()` on `app`. The code doesn't compile. What is wrong conceptually?

```csharp
builder.Services.UseHttpsRedirection();
builder.Services.UseAuthentication();
builder.Services.AddControllers();
var app = builder.Build();
app.AddControllers();
app.UseHttpsRedirection();
```

**Concepts**
- `IServiceCollection.Use*` — compile error: extension methods live on wrong type
- `WebApplication.Add*` — compile error: extension methods live on wrong type
- Builder phase vs app phase separation
- Extension method surface area as the enforced type contract

**Answer**

The code puts middleware registration calls on `builder.Services` (an `IServiceCollection`) and tries to call `AddControllers()` on `app` (a `WebApplication`). Neither compiles because extension methods live on different types: `UseHttpsRedirection` and `UseAuthentication` extend `IApplicationBuilder`/`WebApplication`, while `AddControllers` extends `IServiceCollection`. Moving each call to the correct phase fixes both issues — all `Add*` registrations go before `builder.Build()`, all `Use*` and `Map*` calls go on the returned `WebApplication`:

```csharp
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddControllers();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## Q3. (P) Your team discovers the production app ignores all the URL and environment settings from `launchSettings.json`. Why? How do you set these correctly for a containerized deployment?

**Concepts**
- `launchSettings.json` excluded from `dotnet publish` output
- `ASPNETCORE_URLS` replacing `applicationUrl` in production
- `ASPNETCORE_ENVIRONMENT` from container environment variable
- Development HTTPS cert and user secrets not available in container

**Answer**

Nothing in `launchSettings.json` reaches a production deployment because the file is excluded from `dotnet publish` output by MSBuild. The `applicationUrl` does not set Kestrel bindings in production — I replace it with `ASPNETCORE_URLS=http://+:8080` in the container manifest or platform environment configuration. The `ASPNETCORE_ENVIRONMENT=Development` from the launch profile becomes `ASPNETCORE_ENVIRONMENT=Production` or `Staging` set at the host level. The development HTTPS certificate trusted on developer machines isn't present in a container image — TLS is handled at the ingress or proxy layer. Secrets set locally via `dotnet user-secrets` also don't deploy; in Azure I use App Service configuration settings or Key Vault references instead.

---

## Q4. (R) Review this `Program.cs`. The app always uses the development database connection string in production, even after setting `ASPNETCORE_ENVIRONMENT=Production`.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile("appsettings.Development.json");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

**Concepts**
- `AddJsonFile` loading unconditionally — overrides production config regardless of environment
- Configuration provider order — later providers override earlier ones
- `WebApplication.CreateBuilder` built-in conditional environment-aware loading
- `optional: true` parameter for environment-specific files

**Answer**

The root problem is calling `builder.Configuration.AddJsonFile("appsettings.Development.json")` unconditionally — since it's added after `appsettings.json`, it overrides any matching keys including the connection string regardless of what `ASPNETCORE_ENVIRONMENT` is set to. So in a production deployment where `ASPNETCORE_ENVIRONMENT=Production`, the dev connection string still wins because the dev file was loaded last. The fix is to remove the manual `AddJsonFile` calls entirely and rely on `WebApplication.CreateBuilder`'s built-in behavior, which loads `appsettings.{EnvironmentName}.json` conditionally based on the actual environment. If customization is needed I use the environment name explicitly:

```csharp
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true, reloadOnChange: true);
```

---

## Q5. (M) A colleague asks: "What exactly does `builder.Build()` do — does it start the server? When do services actually get resolved? What does `app.Run()` do that `Build()` doesn't?" Walk through both.

**Concepts**
- `Build()` — service provider construction and finalization
- `ValidateOnBuild` and `ValidateScopes` — optional fail-fast at build time
- Lazy DI resolution — most services first resolved on the initial request
- `Run()` — Kestrel binding and main-thread blocking until shutdown

**Answer**

`Build()` and `Run()` represent distinct phases of the application lifecycle. `Build()` finalizes the service collection into a root `IServiceProvider` and optionally performs graph validation — if `ValidateOnBuild` is enabled, the runtime attempts to resolve every registered service right there, so missing dependencies throw immediately rather than waiting for the first request. After `Build()`, middleware components are declared on the pipeline blueprint, but their delegates haven't executed and no HTTP ports are bound. `Run()` starts the host, causes Kestrel to bind to configured URLs, and blocks the main thread. Many things remain lazy past `Build()` — scoped services resolve per request, and captive dependency bugs between singleton and scoped services don't surface until either the first request hits the singleton, or `ValidateScopes` catches them at build time.

---

## Q6. (R) Your team reports that `/api/weather` returns 200 but `GET /api/users` returns 404, even though both controllers exist. Review the pipeline — what is wrong?

```csharp
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRouting();
app.MapGet("/health", () => Results.Ok());
app.Run();
```

**Concepts**
- `UseRouting` after `MapControllers` — conflicting implicit vs explicit routing contexts
- Auth middleware before routing — endpoint metadata unavailable
- Explicit `UseRouting` governing which `Map*` calls it covers
- Routing middleware position determining endpoint metadata availability for auth

**Answer**

The explicit `UseRouting()` call appears after `MapControllers()`, which creates a conflict between the implicit routing context that `MapControllers` inserted at position 3 and the explicit routing at position 4. Authentication and authorization at positions 1 and 2 run before any routing context exists, so `[Authorize]` metadata on controllers can't be evaluated. The `/health` minimal API may work differently because it's registered after the explicit `UseRouting()` and is governed by that routing context, while controllers may be governed by the implicit routing inserted earlier — leading to inconsistent behavior. The fix is to place `UseRouting()` before authentication so endpoint metadata is available to auth, and ensure all `Map*` calls follow:

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok());
app.Run();
```

---

## Q7. (D) Your `Program.cs` is now 200+ lines. Where do you draw the line between what stays in `Program.cs` and what moves to extension methods or feature modules?

**Concepts**
- Extension methods for DI grouping — safe to extract without behavior change
- Middleware order visibility — keep in one auditable block
- `Startup.cs` vs single `Program.cs` — team convention trade-off
- Hidden ordering across assemblies as security and correctness risk

**Answer**

The key principle is that middleware order must be visible to whoever reads `Program.cs` — extracting DI registrations into `AddInfrastructure()` or `AddApplicationServices()` is safe because registration order doesn't affect correctness. What I preserve in `Program.cs` is the exact `UseForwardedHeaders` → `UseHttpsRedirection` → `UseAuthentication` → `UseRateLimiter` → `MapControllers` sequence, either inline or in a single `UseApiPipeline(app)` method whose body is one ordered block. The danger of splitting middleware setup across multiple assemblies is that reviewers lose the ability to audit order at a glance — a misplaced `UseAuthentication` relative to `UseAuthorization` is a security bug, not just an aesthetic issue. For very large teams that use `Startup.cs` specifically to support integration test fixture substitution, that trade-off is reasonable; for everything else, a thin `Program.cs` with named DI extension methods is the right balance.

---

## Q8. (R) CI fails with an unresolved dependency error for `SmtpOrderNotifier` that requires `IOptions<SmtpOptions>`. The developer says "it ran fine on my machine." Diagnose and fix.

```csharp
builder.Services.AddScoped<IOrderNotifier, SmtpOrderNotifier>();
var app = builder.Build();
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

**Concepts**
- `IOptions<T>` dependency — `Configure<TOptions>` binding required before use
- `ValidateOnBuild` exposing missing DI registrations at startup in CI
- Missing `Smtp` configuration section in CI `appsettings.json`
- `UseExceptionHandler("/error")` requiring a mapped `/error` endpoint

**Answer**

The startup failure occurs because `SmtpOrderNotifier` requires `IOptions<SmtpOptions>` in its constructor, but `SmtpOptions` was never bound to a configuration section with `builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"))`. The developer's machine worked because their local configuration or user secrets contained the `Smtp` section and the service wasn't hit on the specific test paths they ran, while CI runs with `ValidateOnBuild` enabled or the service is resolved during startup via a background service or warm-up. The fix is to add the options binding before registering the notifier, and to ensure the CI appsettings contain a `Smtp` section:

```csharp
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IOrderNotifier, SmtpOrderNotifier>();
```

The `UseExceptionHandler("/error")` call also requires a corresponding mapped `/error` route or it may produce a 404 on the error path — I'd add `app.MapGet("/error", () => Results.Problem())` or switch to the `IExceptionHandler` service approach in .NET 8 instead.
