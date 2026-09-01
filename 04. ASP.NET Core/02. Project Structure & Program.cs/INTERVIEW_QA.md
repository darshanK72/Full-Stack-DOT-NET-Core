# Project Structure & Program.cs — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 02. Project Structure & Program.cs](#chapter-02-project-structure-programcs)
  - [Q1. What is the purpose of `Program.cs` in an ASP.NET Core appli…](#chapter-02-project-structure-programcs-q1)
  - [Q2. What is the difference between the `builder` phase and the `…](#chapter-02-project-structure-programcs-q2)
  - [Q3. What does `WebApplication.CreateBuilder(args)` return and co…](#chapter-02-project-structure-programcs-q3)
  - [Q4. What happens when you call `builder.Build()`?](#chapter-02-project-structure-programcs-q4)
  - [Q5. What happens when you call `app.Run()`?](#chapter-02-project-structure-programcs-q5)
  - [Q6. What is `launchSettings.json`, and does it apply in producti…](#chapter-02-project-structure-programcs-q6)
  - [Q7. How does ASP.NET Core load `appsettings.json` and environmen…](#chapter-02-project-structure-programcs-q7)
  - [Q8. What is the default configuration provider precedence order?](#chapter-02-project-structure-programcs-q8)
  - [Q9. What is the difference between registering services and regi…](#chapter-02-project-structure-programcs-q9)
  - [Q10. Why must `MapControllers()` or `MapGet()` be called for endp…](#chapter-02-project-structure-programcs-q10)
  - [Q11. What is `ASPNETCORE_URLS`, and how does it relate to Kestrel…](#chapter-02-project-structure-programcs-q11)
  - [Q12. What is the purpose of `Properties/launchSettings.json` prof…](#chapter-02-project-structure-programcs-q12)
  - [Q13. How do you organize a growing `Program.cs` without losing cl…](#chapter-02-project-structure-programcs-q13)
  - [Q14. What is the difference between `Startup.cs` and putting ever…](#chapter-02-project-structure-programcs-q14)
  - [Q15. When do misconfigured DI registrations typically surface — a…](#chapter-02-project-structure-programcs-q15)
  - [Q16. What is the `WebApplication` type?](#chapter-02-project-structure-programcs-q16)
  - [Q17. How does `builder.Environment` differ from reading config ma…](#chapter-02-project-structure-programcs-q17)
  - [Q18. What files are typically part of a new ASP.NET Core Web API …](#chapter-02-project-structure-programcs-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 02. Project Structure & Program.cs

### Q1. What is the purpose of `Program.cs` in an ASP.NET Core application? {#chapter-02-project-structure-programcs-q1}

What is the purpose of `Program.cs` in an ASP.NET Core application?

**Answer:** `Program.cs` is the application entry point that creates the host, registers services, configures middleware, maps endpoints, and starts listening for requests via `app.Run()`. It is the single place where startup behavior is defined in modern templates.

- Top-level statements or a `Main` method bootstrap `WebApplication.CreateBuilder(args)`.
- All cross-cutting concerns — auth, CORS, Swagger, rate limiting — are wired here or in extension methods called from here.
- Unlike Framework's `Global.asax`, nothing runs implicitly; every feature must be registered.
- ASP.NET Core 8 templates use minimal `Program.cs` with optional partial classes or extension methods for larger apps.

---

### Q2. What is the difference between the `builder` phase and the `app` phase in `Program.cs`? {#chapter-02-project-structure-programcs-q2}

What is the difference between the `builder` phase and the `app` phase in `Program.cs`?

**Answer:** The builder phase (`var builder = WebApplication.CreateBuilder(args)`) configures services, configuration sources, and logging before the host exists. The app phase (`var app = builder.Build()`) configures middleware and endpoints on the built `WebApplication`, then starts the server with `app.Run()`.

- During the builder phase you call `builder.Services.Add*` — registrations go into the service collection but nothing is resolved yet.
- `builder.Build()` validates DI (optionally), constructs the middleware pipeline factory, and returns the runnable application.
- After `Build()`, you call `app.Use*` and `app.Map*` — middleware order is fixed at this point.
- Code that needs a built service provider must run after `Build()` or inside request/endpoints, not during service registration in most cases.

---

### Q3. What does `WebApplication.CreateBuilder(args)` return and configure? {#chapter-02-project-structure-programcs-q3}

What does `WebApplication.CreateBuilder(args)` return and configure?

**Answer:** It returns a `WebApplicationBuilder` that pre-configures Kestrel, content root, configuration (JSON, env vars, command line), logging, and the default service collection. Passing `args` enables command-line and launch-profile configuration overrides.

- Sets content root to the project directory and web root to `wwwroot` by default.
- Adds `appsettings.json`, `appsettings.{Environment}.json`, environment variables, and user secrets (in Development) to configuration.
- Registers framework services including routing, authentication infrastructure, and `IHostApplicationLifetime`.
- Equivalent to the older `Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(...)` chain in one call.

---

### Q4. What happens when you call `builder.Build()`? {#chapter-02-project-structure-programcs-q4}

What happens when you call `builder.Build()`?

**Answer:** `Build()` constructs the `WebApplication` host, finalizes the service provider, and prepares the middleware pipeline for configuration. It is the boundary between service registration and pipeline setup.

- The generic host starts internal services (configuration reload, lifetime notifications) but does not yet listen for HTTP traffic.
- If `ValidateOnBuild` is enabled, invalid DI graphs (e.g., unable to resolve a registered service) throw at this point.
- Returned `WebApplication` implements `IEndpointRouteBuilder` so `MapGet`, `MapControllers`, etc. can register endpoints.
- No middleware runs until `app.Run()` or `app.StartAsync()` is invoked.

---

### Q5. What happens when you call `app.Run()`? {#chapter-02-project-structure-programcs-q5}

What happens when you call `app.Run()`?

**Answer:** `app.Run()` registers a terminal middleware that blocks the main thread and starts the host, causing Kestrel to bind to configured URLs and accept connections. It does not return until the application shuts down.

- Before blocking, it finalizes the middleware pipeline built from all prior `Use` and `Map` calls.
- The host listens on URLs from `ASPNETCORE_URLS`, Kestrel configuration, or `launchSettings.json` when using a profile.
- Graceful shutdown is triggered by Ctrl+C, SIGTERM, or `IHostApplicationLifetime.StopApplication()`.
- `await app.RunAsync()` is the async equivalent preferred in some templates to avoid blocking patterns.

---

### Q6. What is `launchSettings.json`, and does it apply in production? {#chapter-02-project-structure-programcs-q6}

What is `launchSettings.json`, and does it apply in production?

**Answer:** `launchSettings.json` in `Properties/` defines Visual Studio and `dotnet run` launch profiles — application URL, environment name, and browser launch settings. It is a development-only artifact and is not published or read in production deployments.

- Profiles set `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS` via `applicationUrl` and `environmentVariables`.
- IIS Express and Project profiles configure different local ports and hosting models for debugging.
- Production relies on environment variables, Kestrel endpoint configuration, or reverse-proxy bindings instead.
- Treating launch settings as production config is a common mistake — deployed apps ignore this file.

---

### Q7. How does ASP.NET Core load `appsettings.json` and environment-specific overrides? {#chapter-02-project-structure-programcs-q7}

How does ASP.NET Core load `appsettings.json` and environment-specific overrides?

**Answer:** `WebApplication.CreateBuilder` adds JSON configuration providers for `appsettings.json` and `appsettings.{Environment}.json`, with the environment-specific file loaded later so its keys override the base file. Environment name comes from `ASPNETCORE_ENVIRONMENT` or defaults to `Production`.

- Missing environment files are skipped silently — only present files contribute keys.
- `optional: true` on the base file allows apps without `appsettings.json` to start.
- `reloadOnChange: true` enables hot reload of configuration when the JSON file changes on disk.
- Additional files can be added with `builder.Configuration.AddJsonFile("custom.json", optional: true)`.

---

### Q8. What is the default configuration provider precedence order? {#chapter-02-project-structure-programcs-q8}

What is the default configuration provider precedence order?

**Answer:** Later providers override earlier ones for the same key. The default order (lowest to highest precedence) is typically: `appsettings.json`, then `appsettings.{Environment}.json`, then user secrets in Development, then environment variables, then command-line arguments.

- Environment variables use double-underscore nesting: `Logging__LogLevel__Default` maps to `Logging:LogLevel:Default`.
- Command-line args passed to `CreateBuilder(args)` win over all default sources when keys conflict.
- Custom providers added with `Add*` run in registration order — last registration wins on duplicate keys.
- Knowing precedence is essential when debugging "wrong config value" issues across environments.

---

### Q9. What is the difference between registering services and registering middleware? {#chapter-02-project-structure-programcs-q9}

What is the difference between registering services and registering middleware?

**Answer:** Service registration (`builder.Services.Add*`) adds types to the DI container for later resolution during requests or at startup. Middleware registration (`app.Use*`) adds components to the HTTP pipeline that process every (or branched) request in order.

- Services are resolved when needed — controllers, repositories, options, HttpClient factories.
- Middleware wraps request handling — authentication, routing, exception handling, static files.
- Middleware can be registered with `UseMiddleware<T>()` where `T` is activated from DI per pipeline build.
- Confusing the two leads to calling `app.Use*` before `Build()` or trying to `Add*` after the app is built.

---

### Q10. Why must `MapControllers()` or `MapGet()` be called for endpoints to work? {#chapter-02-project-structure-programcs-q10}

Why must `MapControllers()` or `MapGet()` be called for endpoints to work?

**Answer:** Endpoint mapping registers routes with the routing system and connects them to executable delegates or controller actions. Without `MapControllers()`, `MapGet()`, or equivalent calls, the pipeline has no endpoints and returns 404 for all matched routes.

- `AddControllers()` only registers MVC services in DI — it does not expose HTTP routes by itself.
- `Map*` methods attach endpoint metadata used by authorization, OpenAPI, and link generation.
- In minimal hosting, each route is explicit: `app.MapGet("/health", () => Results.Ok())`.
- Forgetting mapping after upgrading templates is a frequent cause of "all routes 404" bugs.

---

### Q11. What is `ASPNETCORE_URLS`, and how does it relate to Kestrel binding? {#chapter-02-project-structure-programcs-q11}

What is `ASPNETCORE_URLS`, and how does it relate to Kestrel binding?

**Answer:** `ASPNETCORE_URLS` is an environment variable that sets the addresses Kestrel listens on, such as `http://localhost:5000` or `http://*:8080`. It overrides default URLs from `launchSettings.json` when set in the process environment.

- Multiple URLs can be semicolon-separated: `http://0.0.0.0:8080;https://0.0.0.0:8443`.
- Kestrel endpoint configuration in code or `appsettings.json` can supersede or complement `ASPNETCORE_URLS` depending on setup order.
- Container deployments often set `ASPNETCORE_URLS=http://+:8080` to bind all interfaces on port 8080.
- IIS in-process hosting ignores Kestrel URL binding because IIS owns the HTTP port.

---

### Q12. What is the purpose of `Properties/launchSettings.json` profiles? {#chapter-02-project-structure-programcs-q12}

What is the purpose of `Properties/launchSettings.json` profiles?

**Answer:** Launch profiles group settings used when starting the app from an IDE or `dotnet run --launch-profile ProfileName`. Each profile can specify URLs, environment, command-line args, and whether to launch a browser.

- The `http` and `https` profiles in ASP.NET Core 8 templates set different ports and TLS behavior for local dev.
- `dotnet run --launch-profile https` applies that profile's environment variables for the session.
- Profiles simplify team consistency for local development without affecting published output.
- They are not security boundaries — secrets belong in user secrets or vaults, not committed launch profiles.

---

### Q13. How do you organize a growing `Program.cs` without losing clarity? {#chapter-02-project-structure-programcs-q13}

How do you organize a growing `Program.cs` without losing clarity?

**Answer:** Extract service registration into static extension methods such as `AddApplicationServices(this IServiceCollection services)` and pipeline setup into `UseApplicationMiddleware(this WebApplication app)`. Optionally split across partial `Program` classes or feature modules per bounded context.

- `builder.Services.AddInfrastructure(config)` and `AddWebApi()` keep `Program.cs` readable.
- Assembly scanning for `IHostedService` or validators reduces manual registration lines.
- Avoid moving middleware order across files without documenting order dependencies — auth must still follow routing rules.
- For very large apps, vertical slice or clean architecture projects hold implementations while `Program.cs` only orchestrates.

---

### Q14. What is the difference between `Startup.cs` and putting everything in `Program.cs`? {#chapter-02-project-structure-programcs-q14}

What is the difference between `Startup.cs` and putting everything in `Program.cs`?

**Answer:** Both approaches configure the same host; `Startup.cs` separates `ConfigureServices` and `Configure` methods into a dedicated class for organizational preference. `Program.cs`-only minimal hosting inlines the same logic without a separate type.

- `Startup.cs` can be easier for teams migrating from ASP.NET Core 2.x patterns or wanting xUnit-style test hooks on `Startup`.
- Minimal `Program.cs` reduces files and aligns with current templates and top-level statements.
- `WebApplication.CreateBuilder` eliminates the need for `Startup` unless you explicitly configure it via `ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())`.
- Neither choice changes runtime behavior when configured equivalently.

---

### Q15. When do misconfigured DI registrations typically surface — at build, startup, or first request? {#chapter-02-project-structure-programcs-q15}

When do misconfigured DI registrations typically surface — at build, startup, or first request?

**Answer:** Missing or invalid registrations may surface at `builder.Build()` if validate-on-build is enabled, at first resolution during startup for hosted services, or lazily on the first HTTP request that needs the missing service. Many apps default to failing on first request when a controller cannot be constructed.

- `ValidateOnBuild` and `ValidateScopes` (in Development) catch captive dependencies and missing registrations earlier.
- Singleton constructors that require scoped services fail at first resolution, often on first request to that code path.
- Typos in `AddScoped<IInterface, Implementation>()` produce `InvalidOperationException: Unable to resolve service for type...`.
- Background services resolving DI at `StartAsync` can fail immediately at app start rather than on HTTP traffic.

---

### Q16. What is the `WebApplication` type? {#chapter-02-project-structure-programcs-q16}

What is the `WebApplication` type?

**Answer:** `WebApplication` is the unified host type returned by `builder.Build()` that implements `IHost`, `IApplicationBuilder`, and `IEndpointRouteBuilder`. It is used to register middleware, map endpoints, and run the server.

- Combines generic host lifetime (`Run`, `StopAsync`) with web-specific pipeline configuration.
- `MapGet`, `MapControllers`, `MapRazorPages`, and `MapHub` are extension methods on `WebApplication`.
- It replaces the older pattern of separately managing `IHost` and `IWebHost`.
- Access `app.Services` for the root service provider; use `HttpContext.RequestServices` for request scope.

---

### Q17. How does `builder.Environment` differ from reading config manually? {#chapter-02-project-structure-programcs-q17}

How does `builder.Environment` differ from reading config manually?

**Answer:** `builder.Environment` (`IWebHostEnvironment`) exposes hosting context — environment name, content root path, web root path, and helpers like `IsDevelopment()`. Configuration (`builder.Configuration`) exposes key-value settings from all providers; the two complement each other.

- Environment name drives which `appsettings.{Environment}.json` loads and conditional code paths.
- `ContentRootPath` locates `appsettings.json`; `WebRootPath` points to `wwwroot` for static files.
- Reading `configuration["Environment"]` is not equivalent — the canonical env name is on `IWebHostEnvironment`.
- File provider and physical file watchers use paths from `Environment`, not from arbitrary config keys.

---

### Q18. What files are typically part of a new ASP.NET Core Web API project structure? {#chapter-02-project-structure-programcs-q18}

What files are typically part of a new ASP.NET Core Web API project structure?

**Answer:** A new Web API project includes `Program.cs`, `appsettings.json`, `appsettings.Development.json`, the `.csproj` file, `Properties/launchSettings.json`, and optionally `Controllers/` folder with an example controller. `wwwroot` may be absent in API-only templates.

- The `.csproj` targets `net8.0` with `Microsoft.AspNetCore.OpenApi` or Swashbuckle packages depending on template.
- `.http` files in newer templates provide REST Client sample requests.
- `GlobalUsings.cs` and implicit usings reduce boilerplate in SDK-style projects.
- Docker, test, and domain projects are added by the team — not required in the default template.

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

#### Q1. (R) Review this top-level `Program.cs`. The app compiles but returns 404 for all controller routes and Swagger shows no endpoints. What structural mistake was made?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.Run();
```

---

**Answer:**

**Answer:** Services and Swagger are registered, but the pipeline never maps controllers or calls `UseRouting` — authorization middleware runs with no endpoints, so every controller route returns 404.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline | Missing `app.MapControllers()` (and typically `UseRouting`) | No routes registered — perpetual 404 |
| Structure | `UseAuthorization()` without authentication or endpoints | Misleading security setup; no `[Authorize]` enforcement path |
| Swagger | OpenAPI has no discovered endpoints | Empty Swagger UI despite "working" compile |
| Design | `app.Run()` not used but implicit — pipeline ends without terminal route | Requests fall through unmatched |

**Fix (priority order):**

1. Add routing and endpoint mapping:

```csharp
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

2. Add `UseAuthentication()` if JWT/cookies are used — authorization alone is insufficient.

**Production takeaway:** Top-level `Program.cs` merges startup phases — **Build() is not enough**; missing `Map*` calls are the most common "empty API" bug.

---

---

#### Q2. (R) A developer registers middleware inside `builder.Services` and services inside the `app` pipeline block. What fails and how should `Program.cs` separate the two phases?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.UseHttpsRedirection();
builder.Services.AddControllers();

var app = builder.Build();
app.AddControllers();
app.MapGet("/ping", () => "pong");
app.Run();
```

---

**Answer:**

**Answer:** Middleware extension methods belong on `WebApplication` after `Build()`; service registration belongs on `IServiceCollection` before `Build()` — swapping them causes compile errors or no-op pipeline configuration.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `builder.Services.UseHttpsRedirection()` — not an `IServiceCollection` extension | Build failure |
| Compile | `app.AddControllers()` — not a valid pipeline method | Build failure |
| Conceptual | Services vs middleware phases confused | Team repeats mistake across microservices |
| Runtime | Even if shimmed, middleware never runs | Security headers, HTTPS redirect absent |

**Fix (priority order):**

1. **Before `Build()`:** `builder.Services.Add*()` — DI registrations only.
2. **After `Build()`:** `app.Use*()` / `app.Map*()` — middleware and endpoints.

```csharp
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddControllers();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/ping", () => "pong");
app.Run();
```

**Production takeaway:** `Program.cs` structure is a contract — **Services = composition root**, **App = request pipeline**.

---

---

#### Q3. (P) `launchSettings.json` sets `"applicationUrl": "https://localhost:7101;http://localhost:5101"` and `"ASPNETCORE_ENVIRONMENT": "Development"`. What does **not** carry over to production, and what must replace it in Azure App Service or Kubernetes?

---

**Answer:**

**Answer:** `launchSettings.json` is IDE-local and never deployed — production needs environment variables or platform config for URLs, environment name, secrets, and connection strings.

- **Does not deploy:** `launchSettings.json` is not published with the app; its `applicationUrl` and dev environment override apply only to F5 / `dotnet run` from Visual Studio or CLI with launch profile.
- **Replace URLs:** Set `ASPNETCORE_URLS` or platform binding (App Service → Configuration; K8s → container port + Service/Ingress).
- **Replace environment:** Set `ASPNETCORE_ENVIRONMENT=Production` (or Staging) via App Service settings, K8s manifest, or container env — not launch profile.
- **Replace secrets:** User secrets and dev `appsettings.Development.json` give way to Key Vault, App Service secrets, or K8s secrets mounted as env vars.
- **HTTPS dev cert:** Development certificate trust does not exist in prod — use platform-managed certs or cert-manager.

**Production takeaway:** Junior devs often debug "works locally" prod failures because **`launchSettings` is mistaken for runtime config**.

---

---

#### Q4. (R) Review environment-specific configuration loading. Production connects to the dev SQL database after deploy. What is wrong with this setup?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile("appsettings.Development.json");
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
var app = builder.Build();
app.MapControllers();
app.Run();
```

*(Deploy sets `ASPNETCORE_ENVIRONMENT=Production`.)*

---

**Answer:**

**Answer:** `appsettings.Development.json` is loaded unconditionally, so its connection string overrides production settings whenever keys collide — regardless of `ASPNETCORE_ENVIRONMENT=Production`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Configuration | Manual `AddJsonFile("appsettings.Development.json")` without environment guard | Dev connection string wins in Production |
| Security | Dev database credentials in prod process | Data leak, compliance violation |
| Design | Duplicates `CreateDefaultBuilder` behavior incorrectly | Unpredictable config precedence |
| Operations | Silent wrong-environment connect — app "works" until data mismatch | Corrupt prod with test data |

**Fix (priority order):**

1. Remove unconditional Development file load — use `WebApplication.CreateBuilder(args)` which loads environment-specific files automatically.
2. If customizing: `AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)` **after** base `appsettings.json`.
3. Store production connection strings in env vars or secret store — highest precedence.

**Production takeaway:** Configuration provider **order and conditional loading** belong in Project Structure literacy — not only the Options chapter.

---

---

#### Q5. (M) Explain what happens at `var app = builder.Build()` vs `app.Run()` in the modern hosting model — what gets validated, what is still lazy, and when do misconfigured services first surface?

---

**Answer:**

**Answer:** `Build()` constructs the `WebApplication`, builds the service provider (optionally validating registrations), and configures the middleware pipeline blueprint; `Run()` starts Kestrel and blocks until shutdown — many scoped services are not resolved until the first request unless `ValidateOnBuild` is enabled.

- **`Build()`:** Freezes `IServiceCollection` into root `IServiceProvider`; with `ValidateOnBuild`, DI attempts to resolve all registered services — catches missing dependencies at startup.
- **`Build()`:** Middleware components are instantiated when first wired — order is fixed but delegates are not executed yet.
- **`Run()`:** Starts host lifetime, binds Kestrel to configured URLs, begins accepting connections.
- **Lazy resolution:** Scoped services (e.g., `DbContext`) resolve per request — captive singleton/scoped bugs may not appear until traffic unless `ValidateScopes` is on.
- **First request:** Routing, auth, and endpoint execution may throw if middleware order or options are wrong — even when `Build()` succeeded.

**Production takeaway:** "It compiled and started" ≠ "DI and pipeline are correct" — enable scope/build validation in CI/staging.

---

---

#### Q6. (R) Preview routing middleware order before the Routing chapter: controllers return 404, but minimal `/health` works. Identify the ordering bug.

```csharp
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRouting();
app.MapGet("/health", () => Results.Ok());
app.Run();
```

---

**Answer:**

_Answer not found._

---

#### Q7. (D) `Program.cs` in a microservice has grown to 200 lines. The team debates `Program.cs` vs `Startup.cs` vs extension methods (`AddInfrastructure()`, `UseApiPipeline()`). What criteria decide the split without hiding middleware order?

---

**Answer:**

**Answer:** Keep a thin, readable `Program.cs` that shows middleware order explicitly; move DI registration groups into `Add*` extension methods and keep `Use*` pipeline extensions small and ordered — avoid `Startup.cs` unless the team already standardizes on it for testability.

- **Stay in `Program.cs`:** The exact sequence of `UseForwardedHeaders`, `UseAuthentication`, `UseRateLimiter`, `MapControllers` — order must be visible in one scroll or one `UseApiPipeline()` whose body is ordered top-to-bottom.
- **Extract to `AddInfrastructure()`:** DbContext, repositories, HTTP clients, options binding — no middleware.
- **Extract to `UseApiPipeline()`:** Only if the method body preserves order verbatim — document "do not reorder."
- **Avoid:** Multiple hidden `Use*` calls across assemblies that make order non-obvious — production incidents from duplicated `UseAuthentication`.
- **`Startup.cs`:** Valid for large teams wanting `ConfigureServices`/`Configure` test doubles — not required in modern templates.

**Production takeaway:** Structure is for **human readability of pipeline order**, not arbitrary line-count reduction.

---

---

#### Q8. (R) Review this production-hardening attempt. The app throws at startup in CI but worked on a developer laptop. Prioritize fixes.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IOrderNotifier, SmtpOrderNotifier>();
builder.Services.AddHttpClient<IExternalPricingClient, ExternalPricingClient>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseExceptionHandler("/error");
app.MapControllers();
app.Run();
```

*(CI log: `Unable to resolve service for type 'SmtpOrderNotifier' while attempting to activate 'IOrderNotifier'` — `SmtpOrderNotifier` constructor requires `IOptions<SmtpOptions>` never registered.)*

**Answer:**

**Answer:** `ValidateOnBuild` (or first controller resolution) fails because `SmtpOrderNotifier` depends on `IOptions<SmtpOptions>` that was never registered — the developer machine may have had a partial manual registration or did not run the same startup path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | Missing `Configure<SmtpOptions>` / `AddOptions` binding | Startup failure resolving `IOrderNotifier` |
| Configuration | SMTP settings not loaded from config section | Notifier cannot send; options default/null |
| Environment | Swagger omitted in prod (good) but exception handler unmapped | `/error` route may 404 unless endpoint exists |
| Testing | CI enables full graph validation; dev used hot reload without hitting notifier | Bug escapes locally |

**Fix (priority order):**

1. Register options: `builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));`
2. Ensure `appsettings.json` / secrets contain `Smtp` section for CI.
3. Map exception handler endpoint or switch to `IExceptionHandler` (.NET 8+) with proper middleware.

```csharp
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IOrderNotifier, SmtpOrderNotifier>();
```

**Production takeaway:** `Program.cs` service registration must form a **complete graph** — partial local testing hides missing options bindings until CI `ValidateOnBuild`.

---

---
