# ASP.NET Core — Interview Answers

Answers for [INTERVIEW_QUESTIONS.md](./INTERVIEW_QUESTIONS.md).  
Written for clarity and recall — concepts only, no interview coaching.

> **Scope:** All chapters

---

## Chapter 01. Introduction to ASP.NET Core

#### Q1. What is ASP.NET Core?

**Answer:** ASP.NET Core is the cross-platform web framework for building HTTP services, web APIs, MVC sites, and real-time apps on modern .NET. It runs on Kestrel (or behind IIS/nginx), uses a composable middleware pipeline, and integrates dependency injection, configuration, and logging by default.

- It is the successor to ASP.NET on .NET Framework — redesigned for Linux, containers, and cloud deployment rather than IIS-only hosting.
- Applications are bootstrapped through `WebApplication.CreateBuilder` and configured in `Program.cs` with explicit middleware and endpoint registration.
- It supports both controller-based MVC/Razor and Minimal APIs in the same host, sharing routing, auth, and serialization infrastructure.
- ASP.NET Core 8 continues the unified .NET release line with LTS support, performance improvements, and first-class Minimal API and OpenAPI features.

---

#### Q2. How does ASP.NET Core differ from ASP.NET Framework?

**Answer:** ASP.NET Core is cross-platform, modular, and side-by-side deployable, while ASP.NET Framework is Windows-only, monolithic, and tightly coupled to IIS and `System.Web`. Core replaces the old pipeline with composable middleware, built-in DI, and Kestrel as the default server.

- Framework apps depend on machine-wide GAC installs and `web.config`; Core apps ship self-contained or framework-dependent with `appsettings.json` and environment variables.
- `System.Web` handled request lifecycle implicitly; Core makes every stage explicit — routing, auth, endpoints, and static files are registered in code.
- Framework Web Forms, WCF in-process, and classic MVC 5 patterns do not port directly; Core uses middleware, endpoint routing, and modern authentication handlers.
- Microsoft maintains Framework 4.8 in maintenance mode; new web development targets ASP.NET Core on .NET 8.

---

#### Q3. What is Kestrel, and what role does it play in ASP.NET Core?

**Answer:** Kestrel is the cross-platform web server built into ASP.NET Core that listens for HTTP/HTTPS connections and forwards requests into the middleware pipeline. It is the default in-process server for all platforms and is optimized for throughput and low allocation.

- Kestrel terminates TCP connections, parses HTTP, and invokes the `RequestDelegate` chain starting at the first registered middleware.
- On Windows it can run standalone or as the backend behind IIS through the ASP.NET Core Module (ANCM).
- Kestrel supports HTTP/1.x, HTTP/2, HTTP/3 (where configured), WebSockets, and configurable limits on body size, headers, and concurrent connections.
- In production, Kestrel often listens on an internal port while a reverse proxy handles TLS and edge concerns at the network boundary.

---

#### Q4. Why do production deployments often place nginx or IIS in front of Kestrel?

**Answer:** A reverse proxy in front of Kestrel handles TLS termination, load balancing, rate limiting, static-file caching, and request filtering at the network edge. Kestrel is optimized as an application server, not as the sole public-facing internet gateway.

- The proxy presents a single certificate and hostname to clients while Kestrel runs on loopback or an internal network with plain HTTP or mTLS.
- Edge servers can serve cached static assets, gzip/brotli responses, and shield the app from slowloris or oversized payloads before they reach .NET.
- Multiple Kestrel instances behind nginx or IIS Application Request Routing distribute load and enable zero-downtime deployments.
- `UseForwardedHeaders` must be configured so Kestrel sees the original client IP and scheme (`https`) from proxy headers.

---

#### Q5. Who terminates TLS in a typical reverse-proxy deployment?

**Answer:** In a typical nginx-or-IIS-in-front-of-Kestrel setup, the reverse proxy terminates TLS — it holds the public certificate and decrypts HTTPS before forwarding to Kestrel over HTTP (often on localhost). Kestrel receives already-decrypted HTTP unless you configure end-to-end TLS separately.

- Clients negotiate TLS with nginx or IIS; Kestrel sees `X-Forwarded-Proto: https` when forwarded headers are configured correctly.
- Kestrel can terminate TLS directly when exposed without a proxy, using `ListenOptions.UseHttps` or certificate configuration in `appsettings.json`.
- Some deployments use TLS at both layers (TLS to proxy, mTLS or TLS to Kestrel) for defense in depth inside a private network.
- Misconfigured forwarded headers cause redirects and cookie `Secure` flags to assume `http` even though clients used HTTPS.

---

#### Q6. What is the unified hosting model introduced by `WebApplication.CreateBuilder`?

**Answer:** `WebApplication.CreateBuilder` merges the generic host and web host into a single bootstrap API that configures logging, configuration, DI, Kestrel, and middleware in one `Program.cs` flow. It replaces the separate `Host.CreateDefaultBuilder` + `ConfigureWebHostDefaults` split from early ASP.NET Core versions.

- The builder returns a `WebApplicationBuilder` with `Services`, `Configuration`, `Environment`, and `Logging` pre-wired with the same defaults Visual Studio templates use.
- Calling `builder.Build()` produces a `WebApplication` that is both the host and the pipeline configurator — no separate `IWebHost` and `IHost` coordination.
- Minimal hosting supports top-level statements, reducing ceremony while keeping full access to MVC, Razor, SignalR, and background services.
- The unified model is the standard entry point in ASP.NET Core 6+ and ASP.NET Core 8 project templates.

---

#### Q7. What is the difference between the old `Startup.cs` pattern and the modern minimal hosting model?

**Answer:** The `Startup.cs` pattern split configuration into `ConfigureServices` (DI registration) and `Configure` (middleware pipeline) classes referenced from `Program.cs`. The minimal hosting model colocates builder setup, service registration, middleware, and endpoint mapping in a single top-level `Program.cs` without a separate `Startup` class.

- Functionally equivalent: `builder.Services.AddControllers()` replaces `ConfigureServices`, and `app.UseRouting(); app.MapControllers()` replaces pipeline setup in `Configure`.
- `Startup.cs` remains supported for teams preferring separation; you can still call `builder.Services.AddControllers()` and use a `Startup` class via `builder.Host.ConfigureWebHostDefaults`.
- Minimal hosting reduces boilerplate and aligns with C# top-level statements introduced in modern templates.
- Both patterns compile to the same underlying `WebApplication` host in ASP.NET Core 8.

---

#### Q8. Walk through the major stages of an HTTP request in ASP.NET Core (high level).

**Answer:** An HTTP request enters Kestrel, passes through the middleware pipeline in registration order, matches an endpoint via routing, runs auth/authz, executes the endpoint (controller action or Minimal API delegate), and returns a response back through middleware in reverse order.

- Kestrel parses the request and creates an `HttpContext` with `Request`, `Response`, and `RequestServices` (request-scoped DI).
- Early middleware may handle exceptions, HTTPS redirection, static files, or forwarded headers before routing runs.
- Endpoint routing selects a handler; authorization middleware evaluates policies against the matched endpoint metadata.
- The endpoint produces an `IActionResult`, `IResult`, or raw response; result filters and response compression middleware may modify output on the way out.

---

#### Q9. When would you choose Minimal APIs over MVC controllers?

**Answer:** Minimal APIs suit small to medium HTTP services, microservices, and prototypes where a single file or few files define routes without needing views, complex filter pipelines, or convention-based folder structure. Controllers remain better for large APIs with many actions, Razor integration, and mature MVC conventions.

- Minimal APIs reduce ceremony: `app.MapGet("/api/items", () => ...)` with parameter binding and DI without controller classes.
- Choose controllers when you need `[ApiController]` conventions at scale, area routing, view results, or extensive use of action/resource/result filters.
- Both share the same host, authentication, OpenAPI, and validation infrastructure in ASP.NET Core 8.
- Minimal APIs support `MapGroup`, endpoint filters, and typed results (`TypedResults`) for production-grade APIs.

---

#### Q10. What does `ASPNETCORE_ENVIRONMENT` control?

**Answer:** `ASPNETCORE_ENVIRONMENT` sets the hosting environment name (commonly `Development`, `Staging`, or `Production`) that drives configuration loading, logging verbosity, and conditional middleware such as the developer exception page. It is read at startup into `IWebHostEnvironment.EnvironmentName`.

- `appsettings.{Environment}.json` overlays base `appsettings.json` when the environment name matches.
- `builder.Environment.IsDevelopment()` gates developer-only features like `UseDeveloperExceptionPage` and EF Core sensitive data logging.
- Production hosts set this via environment variables or launch configuration — it should never default to Development in deployed environments.
- Missetting Production to Development exposes stack traces and detailed errors to clients.

---

#### Q11. What makes ASP.NET Core suitable for Linux containers and cloud deployment?

**Answer:** ASP.NET Core runs natively on Linux and macOS, ships as self-contained or framework-dependent deployments, and starts quickly with a small footprint suitable for containers. Kestrel, configurable via environment variables, fits twelve-factor app patterns used on Kubernetes, Azure App Service, and AWS.

- Docker images based on `mcr.microsoft.com/dotnet/aspnet:8.0` provide a minimal runtime layer; SDK images are used only at build time.
- Configuration through environment variables and secrets mounts avoids baking settings into images.
- Health check endpoints (`MapHealthChecks`) integrate with Kubernetes liveness and readiness probes.
- Side-by-side runtime versions allow independent container image updates without machine-wide installs.

---

#### Q12. What is the ASP.NET Core request pipeline?

**Answer:** The request pipeline is an ordered chain of middleware components, each implementing a `RequestDelegate` that can run logic before and after calling the next delegate. Every HTTP request traverses this chain twice in concept — inward on the way to the endpoint and outward when the response returns.

- Middleware is registered with `app.Use...`, `app.Run`, or `app.Map` in `Program.cs`; order determines behavior.
- The pipeline is built once at startup when `app.Run()` is called (after all `Use` registrations).
- Terminal middleware or endpoint execution produces the response; earlier middleware can modify headers or body on the return path.
- Unlike `System.Web` modules, the pipeline is fully explicit and testable with `WebApplicationFactory` in integration tests.

---

#### Q13. What is endpoint routing?

**Answer:** Endpoint routing matches incoming requests to a specific endpoint — a controller action, Minimal API delegate, Razor page, or hub — using route templates and HTTP methods registered at startup. It decouples route matching from endpoint execution and enables endpoint-aware middleware such as authorization.

- `UseRouting` marks where route matching occurs; `UseEndpoints` or `Map*` methods register endpoints in ASP.NET Core 3+ unified routing.
- In minimal hosting, `MapControllers()`, `MapGet()`, and similar calls register endpoints directly on `WebApplication`.
- Matched endpoints carry metadata (`IAuthorizeData`, `ProducesResponseType`, etc.) consumed by middleware and OpenAPI generators.
- Endpoint routing replaced legacy IRouter-based middleware that separated routing from the rest of the pipeline awkwardly.

---

#### Q14. What are the main components registered in `Program.cs`?

**Answer:** `Program.cs` registers application services in DI during the builder phase and configures the HTTP pipeline and endpoints on the built `WebApplication`. Typical registrations include controllers, authentication, authorization, EF Core, HttpClient, options, and health checks.

- Builder phase: `builder.Services.AddControllers()`, `AddAuthentication()`, `AddDbContext<>()`, `AddEndpointsApiExplorer()`, `AddSwaggerGen()`.
- Pipeline phase: `UseHttpsRedirection()`, `UseAuthentication()`, `UseAuthorization()`, `MapControllers()`, `MapHealthChecks()`.
- Configuration and logging are configured automatically by `WebApplication.CreateBuilder` but can be extended with `builder.Configuration` and `builder.Logging`.
- Hosted services (`AddHostedService`) and Kestrel limits are also commonly registered here in ASP.NET Core 8 apps.

---

#### Q15. What is cross-platform hosting in the context of ASP.NET Core?

**Answer:** Cross-platform hosting means the same ASP.NET Core application code can run on Windows, Linux, and macOS without recompilation, using Kestrel or platform-specific reverse proxies. The runtime and framework are portable; only deployment packaging and path conventions differ.

- Development on Windows with deployment to Linux containers is a standard workflow — no separate codebase per OS.
- Path separators, line endings, and case sensitivity for static files require attention on Linux file systems.
- IIS is Windows-specific; Linux production typically uses Kestrel behind nginx, Apache, or a cloud load balancer.
- `dotnet publish` produces a runnable output for the chosen runtime identifier (RID) such as `linux-x64`.

---

#### Q16. How does ASP.NET Core handle dependency injection by default?

**Answer:** ASP.NET Core includes a built-in DI container configured through `builder.Services` with Singleton, Scoped, and Transient lifetimes. Controllers, Minimal API handlers, middleware constructed via factory, and framework services are resolved from this container automatically.

- Constructor injection is the default pattern — the container resolves parameters when activating a type.
- A new DI scope is created per HTTP request; scoped services (e.g., `DbContext`) live for that request only.
- Framework services (`ILogger<T>`, `IConfiguration`, `IHttpContextAccessor`) are pre-registered by `WebApplication.CreateBuilder`.
- Third-party containers (Autofac, etc.) can replace the default provider via `Host.UseServiceProviderFactory` if needed.

---

#### Q17. What is the difference between in-process and out-of-process IIS hosting?

**Answer:** In-process hosting runs the ASP.NET Core app inside the IIS worker process (`w3wp.exe`) with Kestrel bypassed for HTTP handling through IIS's native pipeline. Out-of-process hosting runs Kestrel in a separate `dotnet.exe` child process while IIS forwards requests via the ASP.NET Core Module.

- In-process is the default on IIS for better performance and simpler debugging on Windows; `AspNetCoreModuleV2` loads the Core CLR inside IIS.
- Out-of-process isolates crashes — if the app process dies, IIS can restart it without recycling the entire worker process in some configurations.
- Both modes use `web.config` to point IIS at the published app and configure the hosting model via `hostingModel="inprocess"` or `outofprocess`.
- Linux deployments do not use IIS; this distinction applies to Windows IIS hosting only.

---

#### Q18. What architectural shifts are required when porting a .NET Framework Web API to ASP.NET Core?

**Answer:** Porting requires replacing `System.Web` and `Global.asax` with `Program.cs`, middleware, and endpoint routing; migrating `Web.config` to `appsettings.json` and environment variables; and adopting built-in DI instead of third-party containers wired manually. Authentication moves from OWIN/IIS modules to ASP.NET Core authentication handlers and policies.

- `HttpContext` API changed — `Request`, `Response`, and middleware patterns differ from `HttpApplication` events.
- WCF, Web Forms, and `System.Web.Http` (Web API 2) have no direct equivalents; use ASP.NET Core controllers or Minimal APIs and gRPC or REST clients.
- EF6 can run on Core with compatibility packages, but EF Core is the recommended data stack for new ports.
- Static file handling, bundling, and session state APIs moved to middleware and distributed cache abstractions.

---

## Chapter 02. Project Structure & Program.cs

#### Q1. What is the purpose of `Program.cs` in an ASP.NET Core application?

**Answer:** `Program.cs` is the application entry point that creates the host, registers services, configures middleware, maps endpoints, and starts listening for requests via `app.Run()`. It is the single place where startup behavior is defined in modern templates.

- Top-level statements or a `Main` method bootstrap `WebApplication.CreateBuilder(args)`.
- All cross-cutting concerns — auth, CORS, Swagger, rate limiting — are wired here or in extension methods called from here.
- Unlike Framework's `Global.asax`, nothing runs implicitly; every feature must be registered.
- ASP.NET Core 8 templates use minimal `Program.cs` with optional partial classes or extension methods for larger apps.

---

#### Q2. What is the difference between the `builder` phase and the `app` phase in `Program.cs`?

**Answer:** The builder phase (`var builder = WebApplication.CreateBuilder(args)`) configures services, configuration sources, and logging before the host exists. The app phase (`var app = builder.Build()`) configures middleware and endpoints on the built `WebApplication`, then starts the server with `app.Run()`.

- During the builder phase you call `builder.Services.Add*` — registrations go into the service collection but nothing is resolved yet.
- `builder.Build()` validates DI (optionally), constructs the middleware pipeline factory, and returns the runnable application.
- After `Build()`, you call `app.Use*` and `app.Map*` — middleware order is fixed at this point.
- Code that needs a built service provider must run after `Build()` or inside request/endpoints, not during service registration in most cases.

---

#### Q3. What does `WebApplication.CreateBuilder(args)` return and configure?

**Answer:** It returns a `WebApplicationBuilder` that pre-configures Kestrel, content root, configuration (JSON, env vars, command line), logging, and the default service collection. Passing `args` enables command-line and launch-profile configuration overrides.

- Sets content root to the project directory and web root to `wwwroot` by default.
- Adds `appsettings.json`, `appsettings.{Environment}.json`, environment variables, and user secrets (in Development) to configuration.
- Registers framework services including routing, authentication infrastructure, and `IHostApplicationLifetime`.
- Equivalent to the older `Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(...)` chain in one call.

---

#### Q4. What happens when you call `builder.Build()`?

**Answer:** `Build()` constructs the `WebApplication` host, finalizes the service provider, and prepares the middleware pipeline for configuration. It is the boundary between service registration and pipeline setup.

- The generic host starts internal services (configuration reload, lifetime notifications) but does not yet listen for HTTP traffic.
- If `ValidateOnBuild` is enabled, invalid DI graphs (e.g., unable to resolve a registered service) throw at this point.
- Returned `WebApplication` implements `IEndpointRouteBuilder` so `MapGet`, `MapControllers`, etc. can register endpoints.
- No middleware runs until `app.Run()` or `app.StartAsync()` is invoked.

---

#### Q5. What happens when you call `app.Run()`?

**Answer:** `app.Run()` registers a terminal middleware that blocks the main thread and starts the host, causing Kestrel to bind to configured URLs and accept connections. It does not return until the application shuts down.

- Before blocking, it finalizes the middleware pipeline built from all prior `Use` and `Map` calls.
- The host listens on URLs from `ASPNETCORE_URLS`, Kestrel configuration, or `launchSettings.json` when using a profile.
- Graceful shutdown is triggered by Ctrl+C, SIGTERM, or `IHostApplicationLifetime.StopApplication()`.
- `await app.RunAsync()` is the async equivalent preferred in some templates to avoid blocking patterns.

---

#### Q6. What is `launchSettings.json`, and does it apply in production?

**Answer:** `launchSettings.json` in `Properties/` defines Visual Studio and `dotnet run` launch profiles — application URL, environment name, and browser launch settings. It is a development-only artifact and is not published or read in production deployments.

- Profiles set `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS` via `applicationUrl` and `environmentVariables`.
- IIS Express and Project profiles configure different local ports and hosting models for debugging.
- Production relies on environment variables, Kestrel endpoint configuration, or reverse-proxy bindings instead.
- Treating launch settings as production config is a common mistake — deployed apps ignore this file.

---

#### Q7. How does ASP.NET Core load `appsettings.json` and environment-specific overrides?

**Answer:** `WebApplication.CreateBuilder` adds JSON configuration providers for `appsettings.json` and `appsettings.{Environment}.json`, with the environment-specific file loaded later so its keys override the base file. Environment name comes from `ASPNETCORE_ENVIRONMENT` or defaults to `Production`.

- Missing environment files are skipped silently — only present files contribute keys.
- `optional: true` on the base file allows apps without `appsettings.json` to start.
- `reloadOnChange: true` enables hot reload of configuration when the JSON file changes on disk.
- Additional files can be added with `builder.Configuration.AddJsonFile("custom.json", optional: true)`.

---

#### Q8. What is the default configuration provider precedence order?

**Answer:** Later providers override earlier ones for the same key. The default order (lowest to highest precedence) is typically: `appsettings.json`, then `appsettings.{Environment}.json`, then user secrets in Development, then environment variables, then command-line arguments.

- Environment variables use double-underscore nesting: `Logging__LogLevel__Default` maps to `Logging:LogLevel:Default`.
- Command-line args passed to `CreateBuilder(args)` win over all default sources when keys conflict.
- Custom providers added with `Add*` run in registration order — last registration wins on duplicate keys.
- Knowing precedence is essential when debugging "wrong config value" issues across environments.

---

#### Q9. What is the difference between registering services and registering middleware?

**Answer:** Service registration (`builder.Services.Add*`) adds types to the DI container for later resolution during requests or at startup. Middleware registration (`app.Use*`) adds components to the HTTP pipeline that process every (or branched) request in order.

- Services are resolved when needed — controllers, repositories, options, HttpClient factories.
- Middleware wraps request handling — authentication, routing, exception handling, static files.
- Middleware can be registered with `UseMiddleware<T>()` where `T` is activated from DI per pipeline build.
- Confusing the two leads to calling `app.Use*` before `Build()` or trying to `Add*` after the app is built.

---

#### Q10. Why must `MapControllers()` or `MapGet()` be called for endpoints to work?

**Answer:** Endpoint mapping registers routes with the routing system and connects them to executable delegates or controller actions. Without `MapControllers()`, `MapGet()`, or equivalent calls, the pipeline has no endpoints and returns 404 for all matched routes.

- `AddControllers()` only registers MVC services in DI — it does not expose HTTP routes by itself.
- `Map*` methods attach endpoint metadata used by authorization, OpenAPI, and link generation.
- In minimal hosting, each route is explicit: `app.MapGet("/health", () => Results.Ok())`.
- Forgetting mapping after upgrading templates is a frequent cause of "all routes 404" bugs.

---

#### Q11. What is `ASPNETCORE_URLS`, and how does it relate to Kestrel binding?

**Answer:** `ASPNETCORE_URLS` is an environment variable that sets the addresses Kestrel listens on, such as `http://localhost:5000` or `http://*:8080`. It overrides default URLs from `launchSettings.json` when set in the process environment.

- Multiple URLs can be semicolon-separated: `http://0.0.0.0:8080;https://0.0.0.0:8443`.
- Kestrel endpoint configuration in code or `appsettings.json` can supersede or complement `ASPNETCORE_URLS` depending on setup order.
- Container deployments often set `ASPNETCORE_URLS=http://+:8080` to bind all interfaces on port 8080.
- IIS in-process hosting ignores Kestrel URL binding because IIS owns the HTTP port.

---

#### Q12. What is the purpose of `Properties/launchSettings.json` profiles?

**Answer:** Launch profiles group settings used when starting the app from an IDE or `dotnet run --launch-profile ProfileName`. Each profile can specify URLs, environment, command-line args, and whether to launch a browser.

- The `http` and `https` profiles in ASP.NET Core 8 templates set different ports and TLS behavior for local dev.
- `dotnet run --launch-profile https` applies that profile's environment variables for the session.
- Profiles simplify team consistency for local development without affecting published output.
- They are not security boundaries — secrets belong in user secrets or vaults, not committed launch profiles.

---

#### Q13. How do you organize a growing `Program.cs` without losing clarity?

**Answer:** Extract service registration into static extension methods such as `AddApplicationServices(this IServiceCollection services)` and pipeline setup into `UseApplicationMiddleware(this WebApplication app)`. Optionally split across partial `Program` classes or feature modules per bounded context.

- `builder.Services.AddInfrastructure(config)` and `AddWebApi()` keep `Program.cs` readable.
- Assembly scanning for `IHostedService` or validators reduces manual registration lines.
- Avoid moving middleware order across files without documenting order dependencies — auth must still follow routing rules.
- For very large apps, vertical slice or clean architecture projects hold implementations while `Program.cs` only orchestrates.

---

#### Q14. What is the difference between `Startup.cs` and putting everything in `Program.cs`?

**Answer:** Both approaches configure the same host; `Startup.cs` separates `ConfigureServices` and `Configure` methods into a dedicated class for organizational preference. `Program.cs`-only minimal hosting inlines the same logic without a separate type.

- `Startup.cs` can be easier for teams migrating from ASP.NET Core 2.x patterns or wanting xUnit-style test hooks on `Startup`.
- Minimal `Program.cs` reduces files and aligns with current templates and top-level statements.
- `WebApplication.CreateBuilder` eliminates the need for `Startup` unless you explicitly configure it via `ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())`.
- Neither choice changes runtime behavior when configured equivalently.

---

#### Q15. When do misconfigured DI registrations typically surface — at build, startup, or first request?

**Answer:** Missing or invalid registrations may surface at `builder.Build()` if validate-on-build is enabled, at first resolution during startup for hosted services, or lazily on the first HTTP request that needs the missing service. Many apps default to failing on first request when a controller cannot be constructed.

- `ValidateOnBuild` and `ValidateScopes` (in Development) catch captive dependencies and missing registrations earlier.
- Singleton constructors that require scoped services fail at first resolution, often on first request to that code path.
- Typos in `AddScoped<IInterface, Implementation>()` produce `InvalidOperationException: Unable to resolve service for type...`.
- Background services resolving DI at `StartAsync` can fail immediately at app start rather than on HTTP traffic.

---

#### Q16. What is the `WebApplication` type?

**Answer:** `WebApplication` is the unified host type returned by `builder.Build()` that implements `IHost`, `IApplicationBuilder`, and `IEndpointRouteBuilder`. It is used to register middleware, map endpoints, and run the server.

- Combines generic host lifetime (`Run`, `StopAsync`) with web-specific pipeline configuration.
- `MapGet`, `MapControllers`, `MapRazorPages`, and `MapHub` are extension methods on `WebApplication`.
- It replaces the older pattern of separately managing `IHost` and `IWebHost`.
- Access `app.Services` for the root service provider; use `HttpContext.RequestServices` for request scope.

---

#### Q17. How does `builder.Environment` differ from reading config manually?

**Answer:** `builder.Environment` (`IWebHostEnvironment`) exposes hosting context — environment name, content root path, web root path, and helpers like `IsDevelopment()`. Configuration (`builder.Configuration`) exposes key-value settings from all providers; the two complement each other.

- Environment name drives which `appsettings.{Environment}.json` loads and conditional code paths.
- `ContentRootPath` locates `appsettings.json`; `WebRootPath` points to `wwwroot` for static files.
- Reading `configuration["Environment"]` is not equivalent — the canonical env name is on `IWebHostEnvironment`.
- File provider and physical file watchers use paths from `Environment`, not from arbitrary config keys.

---

#### Q18. What files are typically part of a new ASP.NET Core Web API project structure?

**Answer:** A new Web API project includes `Program.cs`, `appsettings.json`, `appsettings.Development.json`, the `.csproj` file, `Properties/launchSettings.json`, and optionally `Controllers/` folder with an example controller. `wwwroot` may be absent in API-only templates.

- The `.csproj` targets `net8.0` with `Microsoft.AspNetCore.OpenApi` or Swashbuckle packages depending on template.
- `.http` files in newer templates provide REST Client sample requests.
- `GlobalUsings.cs` and implicit usings reduce boilerplate in SDK-style projects.
- Docker, test, and domain projects are added by the team — not required in the default template.

---

## Chapter 03. Middleware Pipeline

#### Q1. What is middleware in ASP.NET Core?

**Answer:** Middleware is a component in the HTTP pipeline that receives an `HttpContext` and either passes control to the next component via `_next(context)` or terminates the request. Each middleware can execute logic before and after the next component runs.

- Built-in middleware includes exception handling, HTTPS redirection, static files, routing, authentication, and authorization.
- Custom middleware is a class with `RequestDelegate _next` in its constructor and an `Invoke` or `InvokeAsync` method.
- Middleware is registered in order in `Program.cs`; the first registered runs first on the inbound request.
- ASP.NET Core 8 ships additional middleware such as rate limiting (`UseRateLimiter`).

---

#### Q2. How does the middleware pipeline process an HTTP request?

**Answer:** Kestrel hands the request to the first middleware's delegate; each component may inspect or modify the request, call the next delegate, then inspect or modify the response on the way back. The chain continues until an endpoint executes or middleware short-circuits without calling next.

- The pipeline is a linked list of `RequestDelegate` functions built at startup.
- Endpoint middleware runs the matched route handler as the innermost logic.
- Response status and headers can be set by any middleware until the response starts streaming.
- Async middleware should return `Task` from `InvokeAsync` to avoid thread pool blocking under load.

---

#### Q3. What is a `RequestDelegate`?

**Answer:** A `RequestDelegate` is a delegate type — `Task RequestDelegate(HttpContext context)` — representing the signature of middleware and the pipeline entry point. Each middleware holds a reference to the next `RequestDelegate` in the chain.

- The terminal delegate after all `Use` calls either runs the endpoint or returns 404.
- `app.Run(async context => { ... })` replaces the remaining pipeline with a single terminal delegate.
- Custom middleware invokes `_next(context)` to call the next delegate in the chain.
- Understanding `RequestDelegate` clarifies how `Use`, `Run`, and `Map` compose the pipeline.

---

#### Q4. What does calling `_next(context)` do in custom middleware?

**Answer:** Calling `_next(context)` passes the current `HttpContext` to the next middleware in the pipeline, allowing downstream components and ultimately the endpoint to run. Code after `await _next(context)` executes during the response phase after inner middleware returns.

- Omitting `await _next(context)` prevents later middleware and endpoints from running — a short-circuit.
- Exceptions thrown after `_next` can still be caught by outer exception-handling middleware if the response has not started.
- Middleware often branches: call `_next` only when a condition passes (e.g., API key valid).
- Always pass the same `HttpContext` instance unless intentionally replacing it (rare).

---

#### Q5. What happens when middleware returns without calling `_next`?

**Answer:** The pipeline short-circuits — no subsequent middleware or endpoint runs for that request. The current middleware is responsible for setting status code, headers, and body before returning.

- Common for authentication failures returning 401, rate limiting returning 429, or custom guards.
- If no response is written, clients may see empty or error responses depending on server defaults.
- Short-circuiting skips authorization and endpoint logic — useful for health checks handled early.
- Over-short-circuiting (buggy middleware) causes mysterious 404/401 for routes that should exist downstream.

---

#### Q6. Why does middleware order matter?

**Answer:** Middleware runs in registration order inbound and reverse order outbound; each component only sees requests that prior middleware forwarded with `_next`. Wrong order breaks routing, auth, exception handling, and static file serving.

- Exception handling should wrap outermost (registered early) to catch exceptions from inner middleware.
- `UseRouting` before `UseAuthentication` and `UseAuthorization` ensures endpoint metadata is available for auth.
- `UseStaticFiles` before routing can serve files without hitting controllers; after routing allows endpoint precedence rules.
- `UseForwardedHeaders` must run before middleware that reads scheme or client IP.

---

#### Q7. What is the recommended order for routing, authentication, and authorization middleware?

**Answer:** Register `UseRouting` (or rely on implicit routing in minimal templates), then `UseAuthentication`, then `UseAuthorization`, then endpoint mapping (`MapControllers`, etc.). In ASP.NET Core 6+ minimal hosting, the template order is `UseAuthentication`, `UseAuthorization`, then `Map*` after routing middleware.

- Authentication populates `HttpContext.User` from cookies, JWT, or other handlers.
- Authorization evaluates policies against the matched endpoint's `[Authorize]` metadata or Minimal API `.RequireAuthorization()`.
- Placing auth before routing (older mistake) breaks endpoint-aware authorization in modern apps.
- CORS middleware (`UseCors`) has its own placement rules — typically after routing, before auth.

---

#### Q8. What is the difference between `Use`, `Run`, and `Map`?

**Answer:** `Use` adds middleware that can call the next component; `Run` adds terminal middleware that never calls next and replaces the remainder of the branch; `Map` branches the pipeline by path prefix and builds a sub-pipeline for matching requests.

- `app.Use(async (context, next) => { ... await next(); })` is inline middleware with next.
- `app.Run(async context => { await context.Response.WriteAsync("Done"); })` terminates — nothing after it runs for that branch.
- `app.Map("/admin", adminApp => { adminApp.Use... })` only runs the sub-pipeline for paths starting with `/admin`.
- `MapWhen` branches on arbitrary predicates instead of path prefix alone.

---

#### Q9. What does `Map("/path", ...)` do to the pipeline?

**Answer:** `Map` creates a pipeline branch — requests with paths matching the prefix execute only the middleware registered inside the branch; non-matching requests skip the branch entirely and continue on the main pipeline.

- Matching is prefix-based by default: `/api` matches `/api/users` and `/api`.
- The branch can have its own terminal middleware or call `MapControllers` for isolated admin APIs.
- Each branch builds a separate middleware chain linked at the map point.
- Useful for serving different auth or rate limits on `/api` vs `/internal` paths.

---

#### Q10. What is `MapWhen`, and when would you use it?

**Answer:** `MapWhen` branches the pipeline based on a custom predicate on `HttpContext`, not just path prefix. Use it when branching logic depends on headers, query strings, or content type rather than URL path alone.

- Example: run special middleware when `context.Request.Headers.ContainsKey("X-Debug")`.
- Non-matching requests bypass the branch without executing its middleware.
- Combines with `UseWhen` for conditional middleware on the main pipeline without full branching.
- Prefer `Map` for path-based splits — `MapWhen` for conditional cross-cutting behavior.

---

#### Q11. How do you register custom middleware in `Program.cs`?

**Answer:** Use `app.UseMiddleware<MyMiddleware>()` where the class has a constructor accepting `RequestDelegate` and optional DI services, plus `InvokeAsync(HttpContext)`. Alternatively use inline `app.Use(async (context, next) => { ... })`.

- `UseMiddleware<T>` resolves `T` from DI per application lifetime with `RequestDelegate` injected.
- Convention-based class name `MyMiddleware` with method `InvokeAsync` or `Invoke` is required.
- Register order relative to other middleware determines execution order.
- Extension method `app.UseCustomMiddleware()` wrapping `UseMiddleware` improves readability in larger apps.

---

#### Q12. What is the difference between middleware and MVC filters?

**Answer:** Middleware runs for every request that reaches it in the pipeline, before MVC selects an action. MVC filters run only within the MVC pipeline around controller actions, Razor pages, or view execution — after routing has chosen an MVC endpoint.

- Middleware is global unless branched with `Map`; filters can be scoped to controllers, actions, or globally registered in MVC.
- Authentication middleware establishes identity; authorization filters can enforce roles on specific actions.
- Exception middleware catches all unhandled exceptions; exception filters only see MVC action exceptions.
- Minimal APIs use endpoint filters instead of MVC filters — middleware still applies to Minimal API requests.

---

#### Q13. Where should exception-handling middleware be placed in the pipeline?

**Answer:** Register exception-handling middleware early — among the first `Use` calls — so it wraps subsequent middleware and endpoints and can catch their exceptions. `UseExceptionHandler` or `UseDeveloperExceptionPage` belong near the top of the pipeline configuration.

- Outer placement ensures exceptions from auth, routing, and controllers are handled consistently.
- Developer exception page should only run in Development; production uses `UseExceptionHandler` with ProblemDetails.
- Middleware registered after exception handler will not have its exceptions caught by that handler.
- Response must not have started before exception middleware writes an alternate response body.

---

#### Q14. What is `UseForwardedHeaders`, and why must it run early?

**Answer:** `UseForwardedHeaders` reads `X-Forwarded-For`, `X-Forwarded-Proto`, and related headers from a reverse proxy and updates `HttpContext.Connection.RemoteIpAddress` and `Request.Scheme` accordingly. It must run before middleware that redirects to HTTPS, generates absolute URLs, or logs client IPs.

- Without it, apps behind nginx or IIS ARR see the proxy IP and `http` scheme.
- Configure `ForwardedHeadersOptions` to trust known proxy IP ranges — trusting all proxies is a security risk.
- Known networks and proxies must be explicitly configured in production.
- ASP.NET Core 8 still requires explicit `app.UseForwardedHeaders()` — it is not enabled by default.

---

#### Q15. What is built-in rate limiting middleware, and where does it belong in the pipeline?

**Answer:** ASP.NET Core 7+ includes rate limiting via `AddRateLimiter` services and `UseRateLimiter` middleware, enforcing fixed-window, sliding-window, token-bucket, or concurrency limits per partition key. Place `UseRateLimiter` after routing (so policies can use endpoint metadata) and before endpoints execute — commonly after `UseAuthentication` if limits vary by user.

- Policies are registered with `builder.Services.AddRateLimiter(options => ...)` in ASP.NET Core 8.
- Rejected requests receive 429 status by default with optional `Retry-After` headers.
- Partition keys can be IP, user identity, or API key header values.
- Rate limiting at the app layer complements edge rate limiting on nginx or API gateways.

---

#### Q16. What does "short-circuiting the pipeline" mean?

**Answer:** Short-circuiting means a middleware completes the response without invoking `_next`, so later middleware and the endpoint never run for that request. It is intentional for auth failures, cached responses, or early returns.

- Differs from an endpoint completing normally after all prior middleware called `_next`.
- Static file middleware short-circuits when a file is found and served.
- Accidental short-circuit bugs occur when middleware forgets to call `_next` for valid requests.
- Short-circuit middleware must not call `_next` after writing the response — double execution causes errors.

---

#### Q17. How does middleware differ from an endpoint filter?

**Answer:** Middleware operates globally on the HTTP pipeline before endpoint invocation is finalized; endpoint filters (Minimal APIs) run immediately around a specific route handler after the endpoint is matched. Middleware cannot easily access route-handler-specific metadata without routing; endpoint filters are per-endpoint and support typed arguments.

- Endpoint filters implement `IEndpointFilter` and chain like middleware but only for one Minimal API route or group.
- MVC action filters are the controller equivalent — scoped to MVC actions, not raw middleware.
- Use middleware for cross-cutting concerns affecting many unrelated endpoints (logging, correlation IDs).
- Use endpoint filters for validation, auditing, or transformation tied to specific Minimal API handlers.

---

#### Q18. Can middleware access DI-registered services? How?

**Answer:** Yes — middleware activated via `UseMiddleware<T>()` can inject any service registered in DI through its constructor, except scoped services should not be injected into singleton middleware constructors. Per-request scoped services are obtained from `HttpContext.RequestServices` inside `InvokeAsync`.

- `RequestDelegate` is always injected as the first constructor parameter for convention-based middleware.
- Injecting `DbContext` into middleware constructor causes captive dependency if middleware is singleton.
- Resolve scoped services with `context.RequestServices.GetRequiredService<T>()` within `InvokeAsync`.
- Inline lambda middleware can use constructor injection only via `UseMiddleware` or factory patterns.

---

---

## Chapter 04. Dependency Injection & Service Lifetimes

#### Q1. What is dependency injection in ASP.NET Core?

**Answer:** Dependency injection (DI) is a design pattern where a class receives its collaborators through constructor parameters (or properties) rather than creating them with `new`. ASP.NET Core ships with a built-in DI container that registers services in `Program.cs` and resolves them automatically for controllers, middleware, Minimal API handlers, and your own types.

- The framework promotes loose coupling: application code depends on abstractions (`IOrderRepository`) while concrete types (`SqlOrderRepository`) are wired at composition root.
- Registration happens during the builder phase (`builder.Services.Add...`); resolution happens when a type is first needed at runtime.
- Constructor injection is the default and recommended style because required dependencies are explicit and the object is always in a valid state after construction.
- ASP.NET Core also supports method injection in Minimal APIs and `[FromServices]` for action parameters when constructor injection is awkward.

---

#### Q2. What is the built-in DI container in ASP.NET Core?

**Answer:** ASP.NET Core includes a lightweight, built-in service provider (`Microsoft.Extensions.DependencyInjection`) that implements `IServiceProvider` and `IServiceScopeFactory`. It is not a full-featured container like Autofac, but it covers constructor injection, lifetimes, open generics, factory delegates, keyed services (.NET 8+), and `IEnumerable<T>` multi-registration.

- Services are described as `ServiceDescriptor` entries in an `IServiceCollection`; `builder.Build()` materializes the root `IServiceProvider`.
- The default provider is replaceable via `Host.UseServiceProviderFactory` if you need advanced features such as property injection or conditional registration.
- Most ASP.NET Core framework services (`ILogger<T>`, `IConfiguration`, `DbContext`, `IHttpClientFactory`) are registered against this same container.
- Third-party containers integrate by adapting their factory to populate the same `IServiceCollection` during host setup.

---

#### Q3. What are the three service lifetimes in ASP.NET Core DI?

**Answer:** The built-in container supports three registration lifetimes: **Transient**, **Scoped**, and **Singleton**. Each lifetime controls how often the container creates a new instance when resolving a given service type.

- **Transient** — a new instance is created every time the service is resolved from a scope or the root provider.
- **Scoped** — one instance per DI scope; in web apps the request scope is the common case.
- **Singleton** — one instance for the lifetime of the root service provider (typically the entire application process).
- Choosing the wrong lifetime is a common source of bugs involving shared mutable state, disposed dependencies, or thread-safety issues.

---

#### Q4. What is the difference between Singleton, Scoped, and Transient?

**Answer:** The difference is how long an instance lives and how many instances exist when the same service is resolved multiple times. Singleton shares one instance process-wide, Scoped shares one instance per scope (usually one HTTP request), and Transient never shares instances across resolutions.

| Lifetime | Instance sharing | Typical use |
|---|---|---|
| **Singleton** | One per application | Caches, configuration facades, `IHttpClientFactory`, stateless services |
| **Scoped** | One per scope (request) | `DbContext`, unit-of-work repositories, per-user request state |
| **Transient** | New every resolve | Lightweight stateless helpers, mappers, small strategy objects |

- A Singleton must not hold mutable per-request state because concurrent requests share the same object.
- Scoped services align with Entity Framework Core's `DbContext`, which is designed to be short-lived and not thread-safe across requests.
- Transient is safest when the type is cheap to construct and carries no shared state, though excessive transient resolution in hot paths can add allocation pressure.

---

#### Q5. When should you register a service as Scoped?

**Answer:** Register a service as Scoped when it should live for the duration of a single unit of work — most often one HTTP request — and must not be shared across concurrent requests or the entire process. This is the default choice for data access, business services that use `DbContext`, and any type that holds request-specific state.

- Entity Framework Core `DbContext` is registered scoped because it tracks entities for one operation and is not thread-safe.
- Repositories and application services that depend on scoped `DbContext` should also be scoped so they share the same context instance within a request.
- Per-request user context (`ICurrentUserService`) belongs in scoped lifetime so each caller sees the correct identity without cross-user leakage.
- Do not register mutable scoped services as Singleton unless you deliberately create a new scope per operation with `IServiceScopeFactory`.

---

#### Q6. What is a "captive dependency," and why is it a problem?

**Answer:** A captive dependency occurs when a longer-lived service (typically a Singleton) holds a reference to a shorter-lived service (typically Scoped or Transient) beyond that dependency's intended lifetime. The long-lived consumer "captures" an instance that may be disposed, stale, or shared incorrectly across unrelated work units.

- Classic example: injecting scoped `AppDbContext` into a singleton `BackgroundService` — the context outlives its scope and throws `ObjectDisposedException` under load.
- Captive dependencies also cause subtle data bugs when a scoped object retains state from a previous request because it was resolved once at the root provider.
- ASP.NET Core can detect some cases when `ValidateScopes` is enabled on the default service provider, throwing at startup instead of failing in production.
- The fix is to match lifetimes correctly or create an explicit scope per operation with `IServiceScopeFactory.CreateScope()`.

---

#### Q7. How does ASP.NET Core create a scope per HTTP request?

**Answer:** When a request enters the pipeline, the host creates a request `IServiceScope` that wraps the root service provider. Middleware, endpoint routing, controllers, and Minimal API handlers resolve scoped services from this scope so each request gets its own instances.

- The scope is established early in the pipeline and disposed when the request completes, which triggers disposal of scoped `IDisposable`/`IAsyncDisposable` services such as `DbContext`.
- `HttpContext.RequestServices` exposes the scoped provider for the current request; framework components resolve from there automatically.
- Singleton services resolved during a request still come from the root provider, but if they incorrectly depend on scoped services, the captive dependency problem appears.
- Background work triggered during a request must not capture scoped services in fire-and-forget tasks — create a new scope inside the background work instead.

---

#### Q8. What happens when you register the same interface twice?

**Answer:** Multiple registrations for the same service type append descriptors to the container rather than replacing earlier entries. Resolving the interface directly returns the **last** registered implementation, while resolving `IEnumerable<TInterface>` injects all registered implementations in registration order.

- `builder.Services.AddSingleton<IMailer, SmtpMailer>();` followed by `AddSingleton<IMailer, SendGridMailer>()` means a single `IMailer` resolve yields `SendGridMailer`.
- `IEnumerable<IMailer>` activates both implementations — useful for composite handlers, plugin pipelines, or strategy chains.
- For named implementations in .NET 8+, keyed services (`AddKeyedSingleton`) avoid ambiguity when you need a specific implementation by key.
- Duplicate registration surprises teams that expect last registration to be the only one visible to `IEnumerable<T>` — both remain unless removed explicitly.

---

#### Q9. What is `IHttpClientFactory`, and why should you use it instead of `new HttpClient()`?

**Answer:** `IHttpClientFactory` is a framework service that creates and manages `HttpClient` instances with correctly pooled `HttpMessageHandler` lifetimes. Using `new HttpClient()` in long-lived services causes socket exhaustion because handlers are not recycled; disposing a shared static `HttpClient` causes a different set of DNS and handler staleness problems.

- Register named or typed clients with `AddHttpClient("payments", c => c.BaseAddress = ...)` and inject `IHttpClientFactory` or a typed client interface.
- Handlers rotate on a timer (default two minutes), which avoids stale DNS entries while still reusing connections efficiently.
- Typed clients (`AddHttpClient<IPaymentGateway, PaymentGateway>()`) combine DI with sensible defaults for base address, headers, and resilience policies.
- `IHttpClientFactory` integrates with Polly for retries, circuit breakers, and timeouts without manual handler lifecycle management.

---

#### Q10. What are keyed services in .NET 8?

**Answer:** Keyed services let you register multiple implementations of the same interface distinguished by an object key (string or other type) and resolve the correct one with `[FromKeyedServices("key")]` or `GetRequiredKeyedService<T>(key)`. They replace fragile "register last wins" patterns when you need named providers such as multiple payment gateways or storage backends.

- Register with `builder.Services.AddKeyedScoped<IPaymentProcessor, StripeProcessor>("stripe")` and `AddKeyedScoped<IPaymentProcessor, PayPalProcessor>("paypal")`.
- Inject in Minimal APIs or controllers: `([FromKeyedServices("stripe")] IPaymentProcessor processor)`.
- Keys are resolved from the active scope's provider, so keyed scoped services follow the same request scope rules as ordinary scoped services.
- Keyed services reduce the need for separate marker interfaces or factory classes when the only difference is which implementation to select at runtime.

---

#### Q11. What is `IServiceScopeFactory`, and when do you need it?

**Answer:** `IServiceScopeFactory` creates new DI scopes on demand from code that runs outside an existing request scope, such as singleton hosted services, queue workers, or manual background tasks. It is registered as a singleton and is the supported way to resolve scoped services from long-lived components.

- Call `using var scope = _scopeFactory.CreateScope();` then resolve scoped services from `scope.ServiceProvider`.
- Typical pattern in `BackgroundService`: one scope per queued message or timer tick so each unit of work gets a fresh `DbContext`.
- Without a scope factory, injecting scoped services into singletons creates captive dependencies or startup validation failures.
- Always dispose the scope (`using`) so scoped disposables are cleaned up promptly after the work unit finishes.

---

#### Q12. What is `IDbContextFactory<TContext>`, and when is it preferred over injecting `DbContext` directly?

**Answer:** `IDbContextFactory<TContext>` creates short-lived `DbContext` instances explicitly, which is ideal when work is not tied to a single HTTP request scope — for example, parallel operations in a singleton service, Blazor Server circuits, or background workers. Injecting `DbContext` directly remains correct for typical request-scoped web API code.

- Register with `AddDbContextFactory<AppDbContext>(options => ...)` alongside or instead of scoped `AddDbContext` depending on consumption patterns.
- Each `CreateDbContext()` (or `await CreateDbContextAsync()`) yields a context you own and must dispose, usually wrapped in `using`.
- Prefer the factory when multiple contexts are needed concurrently in one logical operation — a single scoped `DbContext` is not thread-safe for parallel queries.
- For ordinary controller actions, scoped `DbContext` injection is simpler and aligns with one context per request.

---

#### Q13. What do `ValidateOnBuild` and `ValidateScopes` do?

**Answer:** These are optional checks on the default service provider that fail fast at startup instead of at runtime under load. `ValidateOnBuild` attempts to resolve the entire service graph when the provider is built, and `ValidateScopes` throws when a singleton (or root scope) tries to capture a scoped service directly.

- Enable in ASP.NET Core 8 with `builder.Host.UseDefaultServiceProvider((context, options) => { options.ValidateOnBuild = true; options.ValidateScopes = true; })`.
- `ValidateOnBuild` catches missing registrations such as an unregistered `IOptions<SmtpSettings>` before traffic arrives.
- `ValidateScopes` exposes captive dependencies like scoped `DbContext` injected into a singleton `CacheService` at root resolution.
- They add a small startup cost but are valuable in CI, staging, and local development to prevent DI lifetime mistakes from reaching production.

---

#### Q14. How do you register an interface with its implementation?

**Answer:** Use extension methods on `IServiceCollection` that pair the abstraction with the concrete type and lifetime: `builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>()`. The container resolves `IOrderRepository` to `SqlOrderRepository` wherever that interface is requested.

- Choose lifetime explicitly: `AddSingleton`, `AddScoped`, or `AddTransient` as appropriate for the implementation's state and dependencies.
- You can also register the concrete type alone (`AddScoped<OrderService>()`) when no interface is needed, though interface-based registration improves testability.
- Factory registration `AddScoped<IRepo>(sp => new SqlRepo(sp.GetRequiredService<...>()))` is used when construction logic is non-trivial.
- Open generic registration (`AddScoped(typeof(IRepository<>), typeof(EfRepository<>))`) supports generic repository patterns without per-entity boilerplate.

---

#### Q15. What is constructor injection?

**Answer:** Constructor injection supplies a class's dependencies through its public constructor parameters, and the DI container chooses an constructor and fills each parameter with registered services. It is the primary injection style in ASP.NET Core because dependencies are required, visible, and immutable after construction.

- ASP.NET Core activates controllers, Minimal API handlers, middleware, and your services by reflecting on the constructor with the most resolvable parameters.
- Multiple constructors are supported, but only one will be used — ambiguous or unsatisfiable constructors cause activation failures at runtime (or at build with validation).
- Optional dependencies can use default parameter values or `IOptions<T>` with defaults, but required services should not be resolved manually inside the constructor body.
- Property injection exists in some third-party containers but is rarely needed in ASP.NET Core's built-in provider.

---

#### Q16. Can you inject a Scoped service into a Singleton? What happens?

**Answer:** You should not inject a scoped service directly into a singleton constructor because the singleton outlives any request scope. With `ValidateScopes` enabled, the application throws at startup; without validation, the scoped instance may be created from the root provider, leading to `ObjectDisposedException`, stale state, or cross-request data leakage.

- The container treats resolving a scoped service from the root provider as a captive dependency when validation is on.
- Correct patterns: make the consumer scoped, inject `IServiceScopeFactory` and create a scope per operation, or use `IDbContextFactory<T>` for database access from singleton workers.
- Middleware is scoped per request and can safely consume scoped services; singleton hosted services cannot without explicit scoping.
- This rule applies equally to your own services and to framework abstractions such as `IOptionsSnapshot<T>` that are registered scoped.

---

#### Q17. What is the difference between `AddSingleton`, `AddScoped`, and `AddTransient`?

**Answer:** These three methods register the same abstraction-to-implementation mapping but assign different lifetimes to the created instance. The method name states how long the container caches and reuses the instance when the service is resolved repeatedly.

- `AddSingleton<TService, TImplementation>()` — one shared instance for the application process; thread-safe stateless services only unless you synchronize mutable state deliberately.
- `AddScoped<TService, TImplementation>()` — one instance per DI scope; in web apps this aligns with one HTTP request.
- `AddTransient<TService, TImplementation>()` — a fresh instance on every resolution, even multiple times within the same request.
- All three support overloads for instances, factories, and open generics; lifetime is the only behavioral difference among them.

---

#### Q18. How does DI work in Minimal API route handlers?

**Answer:** Minimal API route handlers participate in the same DI container as controllers: parameters whose types are registered services are resolved automatically from the request scope when the endpoint executes. You declare dependencies as handler parameters alongside route, query, and body bindings.

- Example: `app.MapGet("/orders/{id}", async (int id, IOrderService orders) => ...)` injects `IOrderService` from `HttpContext.RequestServices`.
- Use `[FromKeyedServices("key")]` for keyed services and `[FromServices]` when disambiguation is needed with other binding sources.
- Scoped services resolve correctly because each request creates a scope before the handler runs.
- `TypedResults`, validation filters, and endpoint filters can also receive injected services through constructor injection on filter types registered in DI.

---

## Chapter 05. Configuration & Options Pattern

#### Q1. What is `IConfiguration` in ASP.NET Core?

**Answer:** `IConfiguration` is the unified read-only abstraction over all configuration sources merged into a key-value hierarchy. Application code and the framework use it to read settings from JSON files, environment variables, command-line arguments, and optional providers such as Azure Key Vault.

- Keys use colon notation for nesting (`ConnectionStrings:DefaultConnection`) regardless of the underlying source format.
- `GetSection("Payment")` returns an `IConfigurationSection` subtree without copying provider data.
- `IConfiguration` is registered as a singleton; the merged view reflects provider updates when a source supports reload.
- Prefer binding to strongly typed options classes for application settings instead of scattering string key lookups across services.

---

#### Q2. What configuration sources does ASP.NET Core load by default?

**Answer:** `WebApplication.CreateBuilder` configures a default set of configuration providers in a fixed order. Later providers override earlier ones when the same key exists, so environment-specific and deployment-time values can replace base file settings.

- Typical default chain: `appsettings.json`, `appsettings.{Environment}.json`, User Secrets (Development only), environment variables, and command-line arguments.
- `ASPNETCORE_ENVIRONMENT` selects which environment-specific JSON file loads (for example, `appsettings.Development.json`).
- Additional providers — Azure App Configuration, Key Vault, custom INI/XML — are added explicitly in `Program.cs` or host configuration.
- `launchSettings.json` is not part of `IConfiguration` for deployed applications; it only affects local launch profiles.

---

#### Q3. How does configuration key precedence work when the same key exists in multiple sources?

**Answer:** Configuration providers are layered in registration order, and **the last registered provider wins** for a duplicate key path. This lets deployment environments override committed defaults without editing source files.

- If `Logging:LogLevel:Default` is `Information` in `appsettings.json` and `Warning` in an environment variable, the environment variable value is used.
- Environment variables map hierarchical keys with double underscores (`Logging__LogLevel__Default=Warning`) or colon on some platforms.
- Command-line arguments registered last override environment variables, which is useful in containers and CI scripts.
- Precedence applies at read time through the merged configuration tree; code that caches values at startup will not see later overrides unless it listens for reload.

---

#### Q4. What is the Options pattern?

**Answer:** The Options pattern binds a configuration section to a strongly typed POCO class and injects it through `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>` instead of raw string lookups. It centralizes settings shape, enables validation, and separates configuration structure from secret storage mechanics.

- Register with `builder.Services.Configure<MySettings>(builder.Configuration.GetSection("MySettings"))`.
- Consumers depend on `IOptions<MySettings>` (or snapshot/monitor) and read `.Value` or subscribe to changes.
- Named options support multiple configurations of the same type (`Configure<StorageOptions>("aws", ...)`).
- Validation attributes and `ValidateOnStart` catch misconfiguration before the app accepts traffic.

---

#### Q5. What is the difference between `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>`?

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

#### Q6. When would you use `IOptionsMonitor<T>` over `IOptions<T>`?

**Answer:** Use `IOptionsMonitor<T>` when a singleton or long-lived component must observe current configuration values after startup, especially when JSON files use `reloadOnChange: true` or an external provider pushes updates. `IOptions<T>` captures `.Value` once and ignores subsequent provider reloads unless the service itself is recreated.

- Subscribe with `monitor.OnChange(settings => { ... })` to refresh caches, HTTP client policies, or feature flags when settings change.
- `CurrentValue` always returns the latest merged configuration for that options type.
- Background workers and middleware registered as singletons should use monitor (or re-read `IConfiguration` with change tokens), not snapshot.
- If settings never change at runtime, `IOptions<T>` is simpler and avoids change-callback complexity.

---

#### Q7. How do you bind a configuration section to a strongly typed class?

**Answer:** Define a POCO with properties matching configuration keys, then bind the section during service registration or manually with the configuration binder. The binder maps hierarchical keys to nested properties and supports arrays and dictionaries.

- Preferred registration: `builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"))`.
- Manual bind: `var settings = new EmailSettings(); configuration.GetSection("Email").Bind(settings);`
- Property names are case-insensitive by default; `[Required]` and other data annotations participate when validation is enabled.
- Complex types, lists, and dictionary sections bind when key naming follows documented conventions (`Items:0:Name`, `Headers:Accept`).

---

#### Q8. What does `Configure<TOptions>(configuration.GetSection("..."))` do?

**Answer:** `Configure<TOptions>` registers an `IConfigureOptions<TOptions>` setup that binds the specified configuration section onto a new `TOptions` instance each time options are computed. It connects `IConfiguration` data to the options pipeline consumed by `IOptions<T>`, snapshot, and monitor wrappers.

- Multiple `Configure<TOptions>` calls merge delegates in registration order (later delegates can overwrite earlier property values).
- The section root maps to the options type; missing keys leave default property values intact.
- Binding runs through `OptionsBuilder` and respects culture-invariant conversion for primitives and enums.
- Pair with `ValidateDataAnnotations()` or custom `IValidateOptions<T>` for fail-fast validation at startup or first resolve.

---

#### Q9. What are named options, and when are they needed?

**Answer:** Named options let you register and resolve multiple independent configurations of the same options type, distinguished by a string name. They are needed when one class shape describes several logical profiles — for example, two blob storage backends or multiple JWT bearer schemes.

- Register: `services.Configure<StorageOptions>("aws", config.GetSection("Storage:Aws"));` and similarly for `"azure"`.
- Resolve with `IOptionsSnapshot<StorageOptions>` or `IOptionsMonitor<StorageOptions>` using `.Get("aws")` or inject via `IOptionsFactory<TOptions>`.
- Unnamed `Configure<T>` registers the default name (`Options.DefaultName`).
- Named options avoid duplicating nearly identical POCO types when only configuration values differ.

---

#### Q10. How do environment variables map to configuration keys?

**Answer:** Environment variables become flat keys in the configuration provider, with hierarchy represented by double underscores (`__`) or, on some systems, colons. The provider runs late in the default chain, so env vars commonly override JSON settings in containers and cloud hosts.

- `ConnectionStrings__DefaultConnection` maps to `ConnectionStrings:DefaultConnection`.
- ASP.NET Core also recognizes conventional env vars such as `ASPNETCORE_URLS` and `ASPNETCORE_ENVIRONMENT` through dedicated hosting configuration.
- Kubernetes ConfigMaps and Secrets mounted as env vars follow the same mapping rules without code changes.
- Prefix filters (`AddEnvironmentVariables("MYAPP_")`) limit which variables enter configuration in multi-tenant hosts.

---

#### Q11. What is `ReloadOnChange` on JSON configuration files?

**Answer:** When `appsettings.json` is added with `reloadOnChange: true` (the default in the generic host), the physical file watcher reloads that provider when the file changes on disk. The merged `IConfiguration` updates, and options monitors or change tokens can react without restarting the process.

- Reload affects the configuration provider layer; services that cached values in fields at startup still hold stale data until they use monitor or re-read configuration.
- `IOptionsSnapshot<T>` picks up changes on the next request scope after reload.
- File reload is suitable for non-secret tuning (feature flags, timeouts); secret rotation often still requires pod restart or a vault provider with its own refresh semantics.
- Heavy reload churn on network-mounted config files can cause frequent recomputation — validate operational impact in production.

---

#### Q12. What is the purpose of User Secrets in development?

**Answer:** User Secrets store developer-specific sensitive values outside the project tree on the local machine, loaded only when `DOTNET_ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` is Development. They prevent committing connection strings, API keys, and tokens to source control while keeping the same `IConfiguration` key paths as production.

- Enabled with `UserSecretsId` in the `.csproj` and accessed via `dotnet user-secrets set "Stripe:SecretKey" "..."`.
- Secrets are stored under the user profile, not deployed with the published application.
- They integrate as a configuration provider after JSON files in Development, overriding local `appsettings.Development.json` values.
- User Secrets are not a production mechanism — they complement, rather than replace, vault or platform secret injection.

---

#### Q13. How should production secrets be managed?

**Answer:** Production secrets should live in a dedicated secret store or platform injection mechanism, never in committed configuration files or repository history. Application code continues to consume secrets through `IConfiguration` and the Options pattern while deployment supplies values via environment variables, Azure Key Vault, AWS Secrets Manager, or Kubernetes Secrets.

- Remove secrets from `appsettings.Production.json`; keep only non-sensitive defaults and structure.
- Use managed identity or workload identity when connecting to Azure Key Vault (`AddAzureKeyVault`) rather than embedding vault credentials.
- Mount secrets as environment variables in containers (`Payment__ApiKey`) so the same binding code works across environments.
- Rotate compromised keys in the vault and redeploy or restart workloads; removing a secret from git does not invalidate leaked history.

---

#### Q14. What is options validation (`ValidateDataAnnotations`, `ValidateOnStart`)?

**Answer:** Options validation runs registered validators against bound options instances to ensure required fields, ranges, and custom rules are satisfied before the application relies on them. `ValidateDataAnnotations()` applies attribute-based rules; `ValidateOnStart()` fails application startup if validation fails instead of deferring failure to the first consumer.

- Example: `builder.Services.AddOptions<PaymentOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart();`
- Implement `IValidateOptions<TOptions>` for cross-property rules that annotations cannot express.
- `ValidateOnStart` is especially valuable for API keys, connection strings, and feature toggles whose absence would cause obscure runtime errors.
- Validation runs when options are first built at startup (with `ValidateOnStart`) or on each options factory invocation depending on configuration.

---

#### Q15. What is the difference between reading `configuration["Key"]` and injecting `IOptions<T>`?

**Answer:** Direct `IConfiguration` indexing reads a single string key at the call site, while `IOptions<T>` supplies a typed, bound object whose shape is validated and registered once in DI. Options encourage testability and centralized settings models; raw configuration suits one-off framework setup in `Program.cs`.

- `configuration["Payment:ApiKey"]` returns a string or null with no compile-time property checking.
- `IOptions<PaymentOptions>.Value.ApiKey` is strongly typed and can carry validation attributes and default values on the POCO.
- Options support reload wrappers (snapshot/monitor); manual `configuration["Key"]` reads current merged value but does not notify consumers automatically.
- Use `IConfiguration` in startup/bootstrap code; prefer options in application services and domain layers.

---

#### Q16. How does `appsettings.{Environment}.json` override base settings?

**Answer:** The host loads `appsettings.json` first, then loads `appsettings.{Environment}.json` when the file exists, with the environment name taken from `ASPNETCORE_ENVIRONMENT`. Keys present in the environment file replace matching keys from the base file while unspecified keys inherit base values.

- Setting `ASPNETCORE_ENVIRONMENT=Development` merges `appsettings.Development.json` over the base file.
- Override is key-by-key at the configuration provider level, not a full document replacement — nested sections merge recursively.
- Production deployments typically set environment via hosting platform variables rather than editing files on disk.
- Missing environment files are skipped silently; only the base `appsettings.json` applies when no override file exists.

---

#### Q17. What is `IConfigureOptions<T>`?

**Answer:** `IConfigureOptions<T>` is the extensibility hook that mutates a `TOptions` instance after binding and before consumers read it. Multiple implementations run in registration order, enabling modular libraries to contribute defaults or post-bind adjustments without a single monolithic `Configure` call.

- Implement `Configure(TOptions options)` or `IConfigureNamedOptions<T>` for named variants.
- Framework and libraries register configurators internally — for example, `JwtBearerOptions` setup from authentication extensions.
- Application code usually calls `services.Configure<T>(configuration.GetSection(...))`, which registers an internal `IConfigureOptions<T>` under the hood.
- Post-configure with `IPostConfigureOptions<T>` when values must be adjusted after all `IConfigureOptions` delegates run.

---

#### Q18. Can singleton services safely use `IOptionsSnapshot<T>`? Why or why not?

**Answer:** No — `IOptionsSnapshot<T>` is registered with a scoped lifetime because it is recomputed per scope to reflect configuration reloads on each request. Injecting it into a singleton creates a captive dependency; with `ValidateScopes` enabled, startup fails with an invalid scope error.

- Singleton services should use `IOptions<T>` for static settings or `IOptionsMonitor<T>` when live reload is required.
- Controllers and scoped services are the intended consumers of `IOptionsSnapshot<T>`.
- If a singleton mistakenly resolves snapshot from the root provider without validation, behavior is undefined and may appear to work until scopes dispose.
- The options interfaces encode lifetime contracts — matching consumer lifetime to the correct options wrapper prevents subtle stale-config bugs.

---

## Chapter 06. Logging & Diagnostics

#### Q1. What is `ILogger<T>` in ASP.NET Core?

**Answer:** `ILogger<T>` is a generic logging abstraction registered in DI where the type parameter `T` sets the log category (typically the consuming class name). It wraps the underlying `Microsoft.Extensions.Logging` pipeline and is the standard way application and framework code emit structured log entries.

- Category names appear in filters (`Logging:LogLevel:YourApp.Services.OrderService`) for per-namespace level control.
- Methods include `LogInformation`, `LogWarning`, `LogError`, and overloads that accept exceptions and structured parameters.
- `ILogger<T>` is registered as a singleton factory; each `T` gets a category-specific logger without manual registration.
- Prefer injecting `ILogger<MyService>` over non-generic `ILogger` so categories are precise in production log queries.

---

#### Q2. How is logging configured in ASP.NET Core?

**Answer:** Logging is configured through the `Logging` section of configuration (commonly `appsettings.json`) and optional code-based provider registration in `Program.cs`. The host adds default providers and reads minimum levels and category overrides from configuration at startup.

- Set default and per-namespace levels: `"Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } }`.
- Add providers explicitly with `builder.Logging.AddConsole()`, `AddDebug()`, or third-party sinks (Serilog, Application Insights).
- `ClearProviders()` removes defaults when you want full control — typical in tests or custom Serilog bootstrap.
- Log level filters apply before messages reach providers, reducing overhead for noisy framework categories.

---

#### Q3. What are the standard log levels in .NET logging?

**Answer:** .NET defines six log levels ordered by severity: Trace, Debug, Information, Warning, Error, and Critical, plus None to disable logging. A log call is emitted only when its level is greater than or equal to the effective minimum level for that logger category.

- **Trace** — detailed diagnostic flow, rarely enabled in production.
- **Debug** — developer-oriented diagnostic information useful during local troubleshooting.
- **Information** — general application flow events (request handled, order created).
- **Warning** — unexpected but recoverable situations (retry, deprecated API use).
- **Error** — failures in the current operation that were handled or returned to the caller.
- **Critical** — unrecoverable application or system failures requiring immediate attention.

---

#### Q4. What is structured logging?

**Answer:** Structured logging records events as typed name-value properties attached to a message template rather than as a single formatted string. Log sinks (Seq, Application Insights, Elasticsearch) can index and query on `OrderId`, `UserId`, or `ElapsedMs` without fragile regular expressions.

- Use templates: `_logger.LogInformation("Order {OrderId} shipped in {ElapsedMs} ms", orderId, elapsed);`
- Properties are stored as fields in the logging backend, enabling filters like `OrderId == 'abc'` across all log levels.
- Structured data supports aggregation, alerting, and correlation across services in centralized logging systems.
- Exception objects passed to logging overloads capture type, message, and stack trace as structured exception details when the provider supports it.

---

#### Q5. Why should you use message templates instead of string interpolation in log calls?

**Answer:** Message templates defer formatting to the logging provider and preserve parameter names as structured properties, while string interpolation (`$"Order {id}"`) always allocates the final string even when the log level is disabled. Templates also enable consistent property indexing in Application Insights and Seq.

- `_logger.LogDebug("Processing {OrderId}", orderId)` skips string building when Debug is disabled for that category.
- `$"Processing {orderId}"` evaluates immediately, wasting CPU and allocations on hot paths at Information default levels.
- Templates keep property names explicit for queries (`OrderId`) instead of parsing a concatenated sentence.
- Some providers use template hashing for high-performance logging (`LoggerMessage` source generators build on this model).

---

#### Q6. What is the difference between `_logger.LogError(ex.Message)` and `_logger.LogError(ex, "...")`?

**Answer:** Passing only `ex.Message` logs a plain string with no exception object attached, so sinks lose stack trace, inner exceptions, and structured exception metadata. The overload `_logger.LogError(exception, messageTemplate, ...)` records the full exception for diagnostics and still supports structured message properties.

- `LogError(ex.Message)` uses a string overload — Application Insights and Seq show text without exception details.
- `LogError(ex, "Payment failed for {OrderId}", orderId)` stores the exception type, stack, and custom properties together.
- Support teams searching by exception type or stack frame cannot triage effectively when only `.Message` was logged.
- Always pass the exception instance as the first argument to error/critical logging overloads when an exception exists.

---

#### Q7. What is the difference between `throw;` and `throw ex;` in a catch block?

**Answer:** `throw;` rethrows the caught exception while preserving the original stack trace, so logs and debuggers point to the true failure site. `throw ex;` throws the same exception object but resets the stack trace to the current catch line, hiding where the error originally occurred.

- Use `throw;` when logging and rethrowing, or when a catch block cannot handle the error and must propagate it.
- `throw ex;` makes production incidents look like the bug is in middleware or a generic handler instead of the root cause.
- Filtering and alerting on stack traces become misleading when `throw ex` is used in shared exception handling code.
- If you do not need to catch, omit the try/catch and let global exception middleware log once with a preserved stack.

---

#### Q8. What is `ILogger.BeginScope`, and what is it used for?

**Answer:** `BeginScope` adds contextual properties to all log entries written within a `using` block on that logger, until the scope is disposed. It is used to attach correlation IDs, tenant IDs, or operation names so related log lines share queryable fields across layers.

- Example: `using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id })) { ... }`
- ASP.NET Core request logging scopes often include `RequestId`, `TraceIdentifier`, or path information automatically in some providers.
- Scopes nest; inner scopes add properties without removing outer scope values unless keys collide.
- Middleware that establishes a correlation scope at request start ties controller, service, and repository logs to one customer action.

---

#### Q9. How does ASP.NET Core assign a `TraceIdentifier` to each request?

**Answer:** When a request is received, the host generates a unique string and stores it on `HttpContext.TraceIdentifier` for the lifetime of that request. Loggers, diagnostics, and developer exception pages include this value so operators can correlate all activity for a single HTTP transaction.

- The identifier is available before middleware runs and remains constant until the request completes.
- Built-in request logging and many templates include `{TraceIdentifier}` or map it to logging scope properties.
- It is local to one server instance — distributed systems propagate a separate correlation or trace ID across outbound calls.
- Clients can supply their own correlation header; middleware may copy or complement it but `TraceIdentifier` always exists server-side.

---

#### Q10. What is a correlation ID, and where is it typically set?

**Answer:** A correlation ID is an application-wide identifier carried through a logical operation — often spanning multiple services — so all related logs and traces can be filtered together. It is typically set in edge middleware from an incoming header (for example, `X-Correlation-ID`) or generated when absent, then added to logging scopes and outbound HTTP headers.

- Incoming gateway or API middleware reads or creates the ID at the start of the pipeline.
- `BeginScope` pushes the correlation ID onto all logs for the request and downstream work triggered synchronously within that scope.
- Outgoing `HttpClient` calls should forward the same header via a delegating handler or OpenTelemetry propagation.
- Correlation IDs differ from W3C `traceparent` trace IDs but serve a similar operational purpose; many systems align them in OpenTelemetry setups.

---

#### Q11. How do you configure log levels per namespace in `appsettings.json`?

**Answer:** Under `Logging:LogLevel`, add entries whose keys are category names (usually namespace prefixes) and whose values are minimum level names. The longest matching prefix wins for a given logger category, with `Default` as the fallback when no specific rule matches.

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
    "YourApp.Payments": "Debug"
  }
}
```

- Framework namespaces (`Microsoft.*`) are commonly elevated to Warning to reduce noise while keeping application code at Information.
- Temporary Debug on a subtree (`YourApp.Payments`) aids targeted investigation without enabling Debug globally in production.
- Changes take effect on restart unless using a dynamic logging level provider or configuration reload integrated with logging.

---

#### Q12. What logging providers ship with ASP.NET Core by default?

**Answer:** The generic host registers a small set of built-in providers suitable for local development and simple deployments. Additional sinks are added via NuGet packages or `ILoggerProvider` implementations.

- **Console** — writes formatted log output to stdout/stderr (default in ASP.NET Core 8 templates for development visibility).
- **Debug** — writes to the debugger output window when running under Visual Studio or attached debuggers.
- **EventSource** — emits ETW/EventSource events for low-overhead diagnostics on Windows and tooling integration.
- **EventLog** — available on Windows for writing to the Windows Event Log when configured.
- Production APIs commonly add Application Insights, Serilog, or OpenTelemetry exporters rather than relying on console alone.

---

#### Q13. What is OpenTelemetry, and how does it relate to ASP.NET Core?

**Answer:** OpenTelemetry (OTel) is a vendor-neutral standard and SDK for collecting traces, metrics, and logs from applications and exporting them to observability backends. ASP.NET Core 8 integrates OTel through `OpenTelemetry.Extensions.Hosting` and instrumentation packages that automatically record HTTP requests, outbound calls, and custom spans.

- Add with `builder.Services.AddOpenTelemetry()` and configure tracing, metrics, and logging exporters (OTLP, Azure Monitor, Prometheus).
- ASP.NET Core instrumentation creates spans for incoming requests with method, route, status code, and duration.
- OTel unifies correlation with W3C trace context (`traceparent`) propagated across services.
- It complements — and increasingly replaces — ad hoc Application Insights SDK wiring for portable observability across clouds.

---

#### Q14. What is distributed tracing?

**Answer:** Distributed tracing records a tree of spans representing work across multiple services and machines for one logical operation, linked by shared trace and span IDs. Each service creates child spans for database calls, HTTP outbound requests, and queue processing so operators can see end-to-end latency and failure points.

- A trace ID ties together all spans from the initial API call through downstream microservices.
- Parent-child span relationships show which dependency slowed or failed the overall request.
- Propagation headers (`traceparent`, `tracestate`) carry context on HTTP and messaging calls between services.
- ASP.NET Core with OpenTelemetry or Application Insights produces distributed traces automatically for incoming and outgoing HTTP when instrumentation is enabled.

---

#### Q15. What should you never log in a production application?

**Answer:** Never log secrets, credentials, full payment card data, authentication tokens, passwords, or other regulated personal data at levels that reach persistent sinks. Production logs are replicated, indexed, and retained — treating them as secure storage causes compliance violations and expands breach impact when log systems are compromised.

- Avoid logging raw JWTs, API keys, connection strings, or session cookies even in Error logs during incidents.
- Minimize personally identifiable information (PII) such as full email, government IDs, or health data — prefer opaque internal IDs.
- Request/response body logging at Information or Debug often captures sensitive payloads unintentionally.
- When debugging auth issues, log outcome and correlation ID, not the secret or full credential material.

---

#### Q16. What is the difference between logging and diagnostics?

**Answer:** Logging records discrete timestamped events with messages and severity intended for human investigation and alerting, while diagnostics encompasses broader telemetry — metrics, traces, health checks, counters, and performance counters — that describe system behavior quantitatively over time.

- Logs answer "what happened on this request at 14:32:05?" with narrative detail.
- Metrics answer "what is the error rate and p95 latency this hour?" as aggregated time series.
- Traces answer "which service call in the chain added 800 ms?" with span hierarchies.
- ASP.NET Core uses `ILogger` for logging and OpenTelemetry / `System.Diagnostics.Activity` for tracing; both feed observability platforms but serve different query patterns.

---

#### Q17. How does Application Insights integrate with ASP.NET Core logging?

**Answer:** The `Microsoft.ApplicationInsights.AspNetCore` package registers an `ILoggerProvider` that forwards log entries to Azure Application Insights alongside automatic request, dependency, and exception telemetry. Log levels and sampling settings control volume and cost while preserving Error and Critical signals.

- Add with `builder.Services.AddApplicationInsightsTelemetry()` and configure `APPLICATIONINSIGHTS_CONNECTION_STRING` or `InstrumentationKey` in the environment.
- `ILogger` messages appear as trace telemetry with severity, custom properties, and linked operation ID when a request is active.
- Exception logging with the exception overload creates exception telemetry with stack traces searchable in the portal.
- Adaptive sampling reduces ingestion for high-volume Information logs while keeping statistically representative error data.

---

#### Q18. What is `LoggerMessage` source generators, and why use them?

**Answer:** `LoggerMessage` attributes and source generators (in `Microsoft.Extensions.Logging.Abstractions`) compile high-performance logging methods that cache delegate instances and avoid boxing and string formatting when a log level is disabled. They are used in hot paths and framework code where logging overhead must stay minimal.

- Define partial methods with `[LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} created")]` on a partial static class.
- The generator emits strongly typed `LogOrderCreated(ILogger logger, int orderId)` methods with optimal `IsEnabled` checks.
- Parameter names in the template become structured properties identically to hand-written template calls.
- Use when profiling shows logging allocations matter — ordinary `ILogger` template calls are sufficient for most business code.

---

## Chapter 07. Routing & Endpoints

#### Q1. What is routing in ASP.NET Core?

**Answer:** Routing is the mechanism that maps an incoming HTTP request URL and method to the code that handles it — a controller action, a minimal API delegate, or another endpoint. In ASP.NET Core 8, routing runs early in the pipeline via endpoint routing middleware and produces an `Endpoint` with metadata (HTTP methods, authorization, constraints) before most request processing continues.

- The router compares the request path and verb against registered route templates stored in an `EndpointDataSource`.
- Matched endpoints carry metadata used later for authorization, CORS, rate limiting, and OpenAPI generation.
- Routing applies equally to MVC controllers, Razor Pages, minimal APIs, and SignalR hubs registered with `Map*`.
- If no endpoint matches, the pipeline continues without a selected endpoint and typically returns 404.

---

#### Q2. What is endpoint routing?

**Answer:** Endpoint routing is the unified routing model introduced in ASP.NET Core 3.0 where all endpoints are collected at startup and matched in one pass before the rest of the pipeline executes endpoint-specific logic. ASP.NET Core 8 uses endpoint routing by default — `UseRouting()` selects the endpoint, and `UseEndpoints()` / `Map*` executes it.

- At startup, `MapControllers()`, `MapGet()`, and similar calls register routes into a shared endpoint table.
- `UseRouting()` runs the `EndpointRoutingMiddleware`, which sets `HttpContext.GetEndpoint()` for downstream middleware.
- Authorization middleware reads endpoint metadata (`[Authorize]`, `RequireAuthorization()`) after the endpoint is known.
- This replaces the legacy two-stage routing model where routing and dispatch were separate middleware passes.

---

#### Q3. What is the difference between attribute routing and conventional routing?

**Answer:** Attribute routing declares routes directly on controllers and actions with `[Route]` and `[HttpGet]` attributes, while conventional routing uses centralized route templates like `{controller=Home}/{action=Index}/{id?}` in `Program.cs`. ASP.NET Core 8 REST APIs almost always use attribute routing or minimal API `MapGroup` for explicit, version-friendly URLs.

- Attribute routing gives per-action control — `[Route("api/v1/[controller]")]` plus `[HttpGet("{id:int}")]` defines exact templates.
- Conventional routing maps URL segments to controller and action names by naming convention without per-action attributes.
- Both styles register into the same endpoint table; attribute routes typically provide more specific templates for APIs.
- Greenfield Web APIs should prefer attribute routing or `MapGroup` and omit unused conventional MVC patterns.

---

#### Q4. How does `[Route("api/[controller]")]` work?

**Answer:** The `[controller]` token is a route parameter replaced at startup with the controller class name minus the `Controller` suffix — `ProductsController` becomes `products` by default (case depends on URL generation settings). Combined with `[HttpGet("{id:int}")]`, a `ProductsController` exposes `GET /api/products/{id}`.

- Route tokens like `[controller]` and `[action]` are resolved from type and method names when endpoints are built.
- The replacement uses the controller name without the `Controller` suffix: `OrdersController` → `orders`.
- Token replacement happens at endpoint registration time, not per request.
- You can override casing via route options or use explicit literals (`[Route("api/products")]`) when the URL must not follow class naming.

---

#### Q5. What are route constraints, and why use them?

**Answer:** Route constraints restrict what values a route parameter may accept — for example `{id:int}` only matches integers, `{slug:alpha}` only letters. They prevent ambiguous matches, reject malformed URLs with 404 instead of binding errors, and improve route precedence during matching.

- Built-in constraints include `:int`, `:guid`, `:decimal`, `:datetime`, `:regex(...)`, and `:minlength(n)`.
- A request to `/users/not-a-guid` against `{id:guid}` fails route matching (404) rather than reaching the action.
- Constraints make one template more specific than another, helping the matcher choose the correct endpoint.
- Custom constraints implement `IRouteConstraint` and register via `RouteOptions.ConstraintMap`.

---

#### Q6. What is the difference between `{id}` and `{id:int}` in a route template?

**Answer:** `{id}` accepts any non-empty string segment, while `{id:int}` accepts only integer values that fit `int`. Without `:int`, the parameter binds as a string and the framework may attempt type conversion in model binding, which can throw or produce unexpected 500 errors on invalid input.

- `{id}` matches `abc`, `123`, and any single path segment — less selective during route matching.
- `{id:int}` rejects non-integer segments at routing time, typically returning 404 before the action runs.
- Constrained routes rank higher in precedence than unconstrained parameter routes with the same shape.
- Use the narrowest constraint that matches your domain type (`:guid` for UUIDs, `:int` for numeric keys).

---

#### Q7. How does ASP.NET Core decide which endpoint handles a request?

**Answer:** The endpoint matcher evaluates all registered endpoints for the request HTTP method and path, filters to those whose templates match, then selects the best match by precedence rules. The selected endpoint is stored on `HttpContext` and executed after authorization and other endpoint-aware middleware.

- Only endpoints whose HTTP method constraints include the request verb (GET, POST, etc.) are candidates.
- Template matching compares literal segments exactly and binds parameter segments to values.
- Precedence favors literal segments over parameters, constrained parameters over unconstrained, and longer templates over shorter ones.
- If two endpoints are equally specific, ASP.NET Core 8 throws `AmbiguousMatchException` at runtime.

---

#### Q8. What happens when two routes match the same request?

**Answer:** When two endpoints have equal precedence for the same method and path, the runtime throws `AmbiguousMatchException` — this is a configuration error that must be fixed before production. When one route is more specific (literal vs parameter, constraint vs open), the more specific route wins without error.

- Common causes include duplicate `[HttpGet("{id}")]` on two actions or overlapping minimal API and controller routes.
- Fix by renaming routes, adding constraints (`{id:int}` vs `{slug}`), or removing duplicate registrations.
- Use `EndpointDataSource` inspection or Development endpoint debugging to list collisions at startup or in CI.
- Never rely on declaration order alone when templates are equally specific.

---

#### Q9. What is `MapControllers()`?

**Answer:** `MapControllers()` is an extension on `WebApplication` that registers endpoint routes for all controller actions decorated with attribute routing. It must be called in `Program.cs` for controller-based APIs to respond — without it, controller types exist but no HTTP endpoints are mapped.

- It scans the DI-registered `ControllerActionEndpointDataSource` and adds action endpoints to the route table.
- Works with `[ApiController]` Web APIs and traditional MVC controllers that use `[Route]` attributes.
- Does not register conventional `{controller}/{action}` routes — those require separate `MapControllerRoute` calls.
- Typically placed after `UseRouting()`, `UseAuthentication()`, and `UseAuthorization()` in the pipeline.

---

#### Q10. What is `MapGroup()` in Minimal APIs?

**Answer:** `MapGroup()` creates a route prefix shared by a set of minimal API endpoints, reducing repetition and enabling group-level metadata. Calling `app.MapGroup("/api/v1/customers")` and then `.MapGet("/{id:int}", ...)` registers `GET /api/v1/customers/{id}`.

- Group prefixes compose when nested — `MapGroup("/api").MapGroup("/v1")` adds both segments.
- Groups support `.WithTags()`, `.RequireAuthorization()`, `.WithOpenApi()`, and `.AddEndpointFilter()` for all child routes.
- They replace verbose repeated path strings and mirror controller `[Route]` prefix patterns.
- Each mapped delegate in the group inherits the combined prefix automatically.

---

#### Q11. What is link generation, and why does it matter for `CreatedAtAction`?

**Answer:** Link generation builds URLs from route names, controller/action identifiers, and route values rather than hard-coded strings. `CreatedAtAction` uses link generation to populate the `Location` header on 201 Created responses with the canonical URI of the new resource.

- `CreatedAtAction(nameof(GetById), new { id = order.Id }, order)` resolves the URL from the routing table.
- Hard-coded URLs break when route templates, versioning prefixes, or host paths change.
- Link generation reads `HttpContext.Request.Scheme`, `Host`, and `PathBase` — forwarded headers must be correct behind proxies.
- Minimal APIs use named routes (`WithName("GetOrderById")`) with `Results.CreatedAtRoute` for the same behavior.

---

#### Q12. How do you return HTTP 201 Created with a `Location` header?

**Answer:** Return status 201 with a `Location` header pointing to the new resource URI and include the created representation in the body. In MVC, use `CreatedAtAction`, `CreatedAtRoute`, or `Created`; in minimal APIs, use `Results.Created` or `Results.CreatedAtRoute`.

- MVC: `return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);`
- Minimal API: `return Results.Created($"/api/orders/{order.Id}", order);` or named route variant.
- The body should contain the created DTO so clients need not immediately re-fetch.
- Status code must be 201, not 200 — HTTP caches and REST clients depend on correct semantics.

---

#### Q13. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?

**Answer:** `CreatedAtAction` generates a URL from a controller action name and route values, while `CreatedAtRoute` generates a URL from a named route registered in the routing table. Both return 201 with a `Location` header and response body.

- `CreatedAtAction(nameof(GetById), new { id = 5 }, dto)` targets an action on the current or specified controller.
- `CreatedAtRoute("GetOrderById", new { id = 5 }, dto)` requires a route name from `[HttpGet(Name = "...")]` or `WithName(...)`.
- Named routes decouple link generation from controller type names — useful after refactoring or with minimal APIs.
- Both rely on correct `HttpContext` scheme and host for absolute URLs in the `Location` header.

---

#### Q14. What role do forwarded headers play in URL generation behind a reverse proxy?

**Answer:** When TLS terminates at nginx, IIS, or Cloudflare, Kestrel sees HTTP and an internal hostname unless forwarded headers are applied. `UseForwardedHeaders()` reads `X-Forwarded-Proto`, `X-Forwarded-Host`, and `X-Forwarded-For` so `HttpContext.Request.Scheme` and `Host` reflect the client-facing URL used in link generation.

- Without forwarded headers, `CreatedAtAction` and redirects emit `http://` and internal hostnames.
- `UseForwardedHeaders()` must run early — before HTTPS redirection, authentication, and endpoint execution.
- Configure `ForwardedHeadersOptions.KnownProxies` or `KnownNetworks` to trust only your edge proxies.
- Set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` in container templates as a reminder to enable processing.

---

#### Q15. What is route order / route precedence?

**Answer:** Route precedence determines which endpoint wins when multiple templates could match, based on specificity rules rather than source file order alone. ASP.NET Core 8 ranks literal segments above parameters, constrained parameters above unconstrained, and longer templates above shorter ones.

- `[HttpGet("sale")]` beats `[HttpGet("{category}")]` because a literal segment is more specific than a parameter.
- `{id:guid}` is more specific than `{id}` because of the constraint.
- Equal-specificity duplicates cause `AmbiguousMatchException` — order does not break ties.
- Design APIs with distinct literal paths or constraints to avoid accidental shadowing (e.g., `/products/sale` vs `/products/{category}`).

---

#### Q16. How do HTTP methods map to controller actions or minimal API endpoints?

**Answer:** HTTP verbs map through method constraints — `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`, and `[HttpPatch]` on actions, or `MapGet`, `MapPost`, etc. on minimal APIs. A single route template can support multiple methods via separate endpoints or `[HttpGet]` / `[HttpPost]` on the same path with different actions.

- The matcher requires the request method to match the endpoint HTTP method metadata.
- `[HttpGet("{id}")]` and `[HttpPut("{id}")]` on the same controller share a path but differ by verb.
- `[AcceptVerbs("GET", "HEAD")]` or `[Route(..., Order = ...)]` handle less common combinations.
- Minimal APIs map one delegate per verb — `MapGet("/items", ...)` and `MapPost("/items", ...)` are distinct endpoints.

---

#### Q17. What is the difference between endpoint routing and legacy routing middleware?

**Answer:** Legacy routing (pre-3.0) used `UseMvc()` with routing middleware that did not integrate endpoint metadata into authorization and other middleware. Endpoint routing separates selection (`UseRouting`) from execution (`MapControllers` / terminal middleware), letting authorization, CORS, and rate limiting inspect the matched endpoint before the handler runs.

- Legacy model ran route matching inside MVC middleware without a unified endpoint abstraction.
- Endpoint routing exposes `Endpoint` metadata to the entire pipeline via `HttpContext.GetEndpoint()`.
- ASP.NET Core 8 applications use endpoint routing exclusively — legacy patterns are obsolete.
- `UseEndpoints` was merged into `Map*` calls on `WebApplication` in the minimal hosting model.

---

#### Q18. What is `AmbiguousMatchException`, and what causes it?

**Answer:** `AmbiguousMatchException` is thrown when two or more endpoints have identical precedence for the same HTTP method and URL path, so the matcher cannot choose a single handler. It indicates a routing configuration bug that must be resolved by deduplicating or disambiguating routes.

- Typical causes: duplicate `[HttpGet("users")]` on two actions, or minimal API plus controller sharing the same template.
- Fix by adding constraints, renaming paths, merging handlers, or removing duplicate registrations.
- Detect early by enumerating `EndpointDataSource` endpoints in a startup test or CI check.
- Unlike shadowing (where a more specific route wins silently), ambiguity is always a hard failure.

---

## Chapter 08. Model Binding & Validation

#### Q1. What is model binding in ASP.NET Core?

**Answer:** Model binding is the process that maps HTTP request data — route values, query strings, form fields, and JSON bodies — to action method parameters and complex types. ASP.NET Core 8 uses built-in model binders selected by `[FromRoute]`, `[FromQuery]`, `[FromBody]`, and binding source inference rules.

- Binding runs after routing selects an endpoint and before validation and the action executes.
- Simple types bind from route and query by default; complex types from body typically require POST/PUT/PATCH.
- Binding failures add entries to `ModelState` with error messages per property.
- `[ApiController]` applies opinionated binding conventions (e.g., infer `[FromBody]` for complex types on actions).

---

#### Q2. How does ASP.NET Core bind route values, query strings, and request bodies to action parameters?

**Answer:** The framework selects a binding source per parameter: route templates supply `[FromRoute]` values, query keys supply `[FromQuery]`, and the JSON/form body supplies `[FromBody]`. For each source, the appropriate `IModelBinder` converts strings or JSON tokens to CLR types and populates `ModelState` on failure.

- Route values come from matched endpoint parameters — `{id:int}` binds to an `int id` parameter.
- Query binding uses key names matching property names (case-insensitive by default) — `?page=2&sort=name`.
- Body binding uses `System.Text.Json` (or Newtonsoft if configured) for `[FromBody]` complex types.
- `[BindRequired]` and binding source attributes override defaults when multiple sources exist.

---

#### Q3. What is the difference between `[FromBody]`, `[FromQuery]`, and `[FromRoute]`?

**Answer:** These attributes tell model binding which part of the HTTP request supplies a parameter's value. `[FromRoute]` reads URI template values, `[FromQuery]` reads the query string, and `[FromBody]` reads the request body (typically JSON).

- `[FromRoute]` maps `{orderId}` in the template to a method parameter.
- `[FromQuery]` maps `?status=active` to a parameter or complex filter object properties.
- `[FromBody]` deserializes JSON — only one `[FromBody]` parameter is allowed per action by default.
- Using the wrong source silently produces default values (empty object, 0, null) rather than obvious errors.

---

#### Q4. Can a GET request have a body, and should you use `[FromBody]` on GET?

**Answer:** HTTP does not forbid a body on GET, but clients, proxies, and caches commonly ignore or strip GET bodies, and ASP.NET Core model binders do not bind `[FromBody]` on GET by default. Using `[FromBody]` on GET is an anti-pattern that fails silently in production.

- Search and filter parameters on GET should use `[FromQuery]` or a complex type bound from the query string.
- Heavy or sensitive filters belong on `POST /search` with `[FromBody]` if the payload is large.
- `[ApiController]` inference will not treat GET body parameters as reliable input.
- Integration tests against localhost may mask the issue that mobile clients and CDNs drop GET bodies.

---

#### Q5. What does the `[ApiController]` attribute change about model validation?

**Answer:** `[ApiController]` enables automatic HTTP 400 responses with `ValidationProblemDetails` when model binding or validation fails, without manual `ModelState.IsValid` checks. It also applies binding source inference — complex types from body, simple types from query/route.

- Invalid `ModelState` short-circuits before the action body runs, returning RFC 7807-compatible JSON.
- Attribute routing is required — `[ApiController]` assumes attribute-routed API controllers.
- Binding source inference reduces boilerplate `[FromQuery]` on simple GET parameters.
- Customize behavior via `ConfigureApiBehaviorOptions` (e.g., suppress automatic 400 — discouraged for public APIs).

---

#### Q6. What is `ModelState`, and how is it used?

**Answer:** `ModelState` is a dictionary-like structure on `ControllerBase` that records binding and validation errors keyed by property name. Actions and filters inspect `ModelState.IsValid` to decide whether to proceed, though `[ApiController]` handles this automatically.

- Each key (e.g., `Email`) holds `ModelError` entries with error messages.
- DataAnnotations, `IValidatableObject`, and FluentValidation all contribute errors to `ModelState`.
- `ValidationProblem(ModelState)` returns a 400 ProblemDetails payload with an `errors` extension.
- Binding failures (type conversion) and validation failures (business rules) both appear in the same structure.

---

#### Q7. What are data annotation attributes for validation?

**Answer:** Data annotations are declarative attributes on model properties — `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[RegularExpression]` — evaluated by the validation system during model validation. They run automatically when `[ApiController]` is present and the model is bound.

- `[Required]` ensures a value was supplied — critical for non-nullable reference types with NRT enabled.
- `[Range(1, 100)]` validates numeric bounds; `[StringLength(200)]` limits string length.
- Validation runs after binding — annotations validate the bound object, not raw JSON syntax.
- Cross-field rules need `[IValidatableObject]` or FluentValidation because annotations are property-scoped.

---

#### Q8. What HTTP status code does `[ApiController]` return for validation failures by default?

**Answer:** `[ApiController]` returns **400 Bad Request** with a `ValidationProblemDetails` body (`application/problem+json`) when model binding or validation fails. The response includes an `errors` dictionary mapping property names to message arrays.

- No action code runs when automatic validation fails — the filter returns before the method body.
- Malformed JSON (syntax errors) also typically yields 400, though with a different problem type than field validation.
- Clients should parse the `errors` object for per-field form feedback.
- Override only with strong justification — inconsistent status codes break generated SDKs and SPAs.

---

#### Q9. What is RFC 7807 ProblemDetails?

**Answer:** RFC 7807 defines a standard JSON format for HTTP API error responses with fields like `type`, `title`, `status`, `detail`, and `instance`. ASP.NET Core 8 uses `ProblemDetails` and `ValidationProblemDetails` as the default error shape for API controllers and exception handlers.

- `ValidationProblemDetails` extends `ProblemDetails` with an `errors` dictionary for field-level failures.
- Content type is `application/problem+json` (or `application/problem+xml`).
- `type` is often a URI identifying the error category; `title` is a short human-readable summary.
- Consistent ProblemDetails let frontends and API gateways handle errors uniformly across endpoints.

---

#### Q10. How do you enable ProblemDetails responses in ASP.NET Core?

**Answer:** Register ProblemDetails services and rely on `[ApiController]` automatic validation, or call `Results.Problem()` / `TypedResults.Problem()` in minimal APIs. In ASP.NET Core 8, `builder.Services.AddProblemDetails()` configures customization, and `IExceptionHandler` implementations return ProblemDetails for unhandled exceptions.

- `[ApiController]` + invalid `ModelState` automatically produces `ValidationProblemDetails`.
- `builder.Services.AddProblemDetails(options => { ... })` customizes default titles and types.
- Exception handling: implement `IExceptionHandler` and register with `AddExceptionHandler<T>()`.
- Use `ProducesResponseType(typeof(ProblemDetails), 400)` in OpenAPI metadata for client generation.

---

#### Q11. What is the default JSON property naming policy in ASP.NET Core Web APIs?

**Answer:** ASP.NET Core 8 Web APIs default to **camelCase** JSON property names via `JsonNamingPolicy.CamelCase` in `System.Text.Json`. A C# property `CustomerName` serializes as `"customerName"` and expects the same on deserialization.

- Configured in `AddControllers().AddJsonOptions(...)` or `ConfigureHttpJsonOptions` for minimal APIs.
- PascalCase JSON keys from some clients do not bind unless `PropertyNameCaseInsensitive = true` is set.
- Prefer standardizing clients on camelCase rather than relying on case-insensitive matching in production.
- OpenAPI documents should reflect camelCase to match actual wire format.

---

#### Q12. How do you configure camelCase JSON serialization?

**Answer:** ASP.NET Core 8 uses camelCase by default — explicit configuration is only needed when changing or confirming the policy. Set `JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase` in `AddJsonOptions` for controllers or `ConfigureHttpJsonOptions` for minimal APIs.

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
```

- `ConfigureHttpJsonOptions` applies the same policy to minimal API JSON responses and binding.
- Use `PropertyNameCaseInsensitive = true` only as a compatibility bridge, not the primary contract.
- `[JsonPropertyName("customKey")]` overrides naming for individual properties.
- Newtonsoft.Json requires separate configuration via `AddNewtonsoftJson` if used instead of System.Text.Json.

---

#### Q13. What is `IValidatableObject`?

**Answer:** `IValidatableObject` is an interface implemented on a model class to perform cross-property validation in a `Validate(ValidationContext)` method. It runs during the same validation pass as DataAnnotations and adds errors to `ModelState`.

- Use when one property's validity depends on another — e.g., `EndDate >= StartDate`.
- Return `ValidationResult` instances with optional member names to attach errors to specific properties.
- Errors surface in `ValidationProblemDetails.errors` identically to `[Required]` failures.
- Keep rules focused — large rule sets belong in FluentValidation validators instead.

---

#### Q14. What is FluentValidation, and how does it differ from data annotations?

**Answer:** FluentValidation is a library that defines validation rules in separate `AbstractValidator<T>` classes with a fluent API, rather than attributes on DTOs. Register it with `AddFluentValidationAutoValidation()` to integrate with ASP.NET Core 8 model validation and `ModelState`.

- Rules live in validator classes — DTOs stay clean and free of validation attributes.
- Supports complex conditional logic, async rules, and reusable rule sets more comfortably than annotations.
- Errors still merge into `ModelState` and return as `ValidationProblemDetails` when `[ApiController]` is enabled.
- Do not duplicate the same rule in both DataAnnotations and FluentValidation.

---

#### Q15. How does complex type binding from query strings work (e.g., nested objects)?

**Answer:** Complex types bind from query strings using prefix notation — property names become query keys like `sort.field=name&sort.descending=true` for a nested `Sort` object. ASP.NET Core 8 binds public settable properties case-insensitively by default for `[FromQuery]` types.

- Prefix binding uses dot notation for nesting — `filter.minPrice=10&filter.category=books`.
- Some clients send bracket notation (`sort[field]`) which may require custom model binders or documentation alignment.
- Deep nesting in query strings is fragile — flatten to `sortField` and `sortDescending` for public APIs.
- Test binding with integration tests — silent null nested objects are a common production bug.

---

#### Q16. What is the difference between model binding errors and validation errors?

**Answer:** Model binding errors occur when the framework cannot convert request data to the target type — invalid JSON shape, type conversion failure, or missing required binding source. Validation errors occur when binding succeeds but DataAnnotations, `IValidatableObject`, or FluentValidation rules reject the values.

- Binding error example: `"abc"` for an `int` route parameter when no `:int` constraint exists.
- Validation error example: empty string on a property marked `[Required]` after successful bind.
- Both appear in `ModelState` but binding errors may prevent validation from running on unbound properties.
- Route constraints push some binding failures to 404 before `ModelState` is populated.

---

#### Q17. What is `[ValidateNever]` used for?

**Answer:** `[ValidateNever]` excludes a property from validation — useful when a navigation property or large object graph should not be validated during binding. It prevents `[Required]` and other validators from running on that property and its children.

- Apply to EF Core navigation properties accidentally exposed on API DTOs.
- Prevents over-validation of nested graphs on PATCH endpoints that bind partial updates.
- Does not skip model binding — the property still binds if present in the request.
- Use deliberately — excluding properties can hide missing validation on sensitive fields.

---

#### Q18. How does ASP.NET Core handle invalid JSON in the request body?

**Answer:** When the JSON payload is syntactically invalid or cannot be deserialized to the target type, ASP.NET Core 8 returns **400 Bad Request** with a ProblemDetails response before the action executes. This is distinct from validation failures on a successfully deserialized object.

- Malformed JSON (trailing comma, wrong token type) fails during input formatting, not DataAnnotations.
- `[ApiController]` maps formatter exceptions to 400 automatically for API controllers.
- Type mismatches (`"text"` for an `int` property) may bind as model errors or deserialization failures depending on configuration.
- Log deserialization failures server-side; return generic messages to clients without exposing internal parser details.

---

## Chapter 09. Filters

#### Q1. What are filters in ASP.NET Core MVC?

**Answer:** Filters are attributes or DI-registered components that run before and after MVC action execution, providing cross-cutting logic at the controller/action level. ASP.NET Core 8 supports authorization, resource, action, exception, and result filters — distinct from middleware, which runs for the entire pipeline.

- Filters operate only after routing selects an MVC controller action (or Razor Page handler).
- They access MVC-specific context — `ActionDescriptor`, route values, bound arguments, and `IActionResult`.
- Register globally in `AddControllers(options => ...)`, via `[ServiceFilter]`, or `[TypeFilter]`.
- Minimal APIs use endpoint filters instead of MVC filters for the same cross-cutting concerns.

---

#### Q2. What is the MVC filter pipeline execution order?

**Answer:** MVC filters run in a fixed order: authorization filters, resource filters, action filters, exception filters (on failure), then result filters. Within each type, global filters run before controller filters, which run before action filters — `IOrderedFilter.Order` can fine-tune ordering.

- Authorization filters run first — fail-fast 401/403 before expensive work.
- Resource filters wrap the remainder of the pipeline including model binding and action execution.
- Action filters wrap the action method itself — `OnActionExecuting` before, `OnActionExecuted` after.
- Result filters wrap `IActionResult` execution (view rendering, JSON serialization).

---

#### Q3. What is an authorization filter?

**Answer:** An authorization filter implements `IAuthorizationFilter` or `IAsyncAuthorizationFilter` and runs before the action to verify the caller is allowed to execute it. The `[Authorize]` attribute is implemented as an authorization filter that evaluates policies via `IAuthorizationService`.

- Set `context.Result` to `UnauthorizedResult` or `ForbidResult` to short-circuit before the action runs.
- Runs after authentication middleware populates `HttpContext.User`.
- Custom filters can enforce API keys or custom claims, but prefer policy-based authorization for maintainability.
- Use `IAsyncAuthorizationFilter` when authorization logic performs async I/O — never block with `.Result`.

---

#### Q4. What is an action filter?

**Answer:** An action filter implements `IActionFilter` or `IAsyncActionFilter` and runs immediately before and after the action method executes. It is ideal for action-scoped concerns — audit logging, timing, mutating bound arguments, or validating models after binding.

- `OnActionExecuting` / before `await next()`: can set `context.Result` to skip the action.
- `OnActionExecuted` / after `await next()`: inspect outcome, exceptions, or elapsed time.
- Prefer `IAsyncActionFilter` when the filter performs I/O or async work.
- Action filters see bound parameters — middleware does not have this MVC context.

---

#### Q5. What is a resource filter?

**Answer:** A resource filter implements `IResourceFilter` or `IAsyncResourceFilter` and wraps execution of the entire remainder of the filter pipeline plus the action. It runs after authorization but before model binding and action filters on the way in, and after result execution on the way out.

- Use for request-scoped caching in `HttpContext.Items` or short-circuiting with a cached `IActionResult`.
- `OnResourceExecuting` can set `context.Result` to bypass the action entirely.
- Not intended for cross-request caching — use `IMemoryCache` with TTL for that.
- Runs earlier than action filters — suitable for decisions that skip model binding cost.

---

#### Q6. What is a result filter?

**Answer:** A result filter implements `IResultFilter` or `IAsyncResultFilter` and runs before and after the `IActionResult` is executed — rendering a view, writing JSON, or returning a file. Use it to modify headers, wrap responses, or log final output status.

- `OnResultExecuting` runs before the result executes — can cancel or replace `context.Result`.
- `OnResultExecuted` runs after — inspect the response status code and log completion.
- Exception filters handle action exceptions; result filters handle result execution wrapping.
- For uniform response shaping across APIs, middleware or endpoint filters may be simpler.

---

#### Q7. What is an exception filter?

**Answer:** An exception filter implements `IExceptionFilter` or `IAsyncExceptionFilter` and runs when an unhandled exception occurs in the action or earlier MVC filter stages, before the result is committed. It can mark the exception handled and set `context.Result` to an error response.

- Only catches exceptions from the MVC pipeline — not middleware failures or minimal API exceptions.
- Runs before result execution — useful for converting domain exceptions to specific MVC views.
- ASP.NET Core 8 APIs prefer centralized `IExceptionHandler` middleware for uniform ProblemDetails.
- Exception filters do not replace logging — always log the full exception object server-side.

---

#### Q8. What is the difference between middleware and filters?

**Answer:** Middleware runs for every request matching its branch in the pipeline, before or after routing, and applies to all endpoint types. Filters run only for MVC/Razor Page requests after an action is selected, with access to MVC-specific context like `ActionDescriptor` and bound arguments.

- Middleware order is global and explicit in `Program.cs` — filters follow MVC's fixed pipeline order.
- Middleware is the right layer for authentication, forwarded headers, rate limiting, and exception handling across all endpoints.
- Filters suit per-controller or per-action logic — audit trails, action timing, MVC-specific authorization checks.
- Minimal APIs and middleware-only pipelines never execute MVC filters.

---

#### Q9. When would you use a filter instead of middleware?

**Answer:** Use a filter when the logic needs MVC context — action name, bound parameters, or controller-level policies — or when the behavior should apply only to specific controllers/actions rather than every request. Use middleware for transport-wide concerns that must run before routing or across minimal APIs and MVC alike.

- Action-specific audit logging with access to bound DTOs → action filter.
- Per-controller authorization beyond standard `[Authorize]` policies → authorization filter.
- Rejecting requests before expensive DB middleware on all paths including health checks → middleware.
- API key validation on all routes including minimal APIs → middleware, not controller filters.

---

#### Q10. How do you register a global filter?

**Answer:** Add the filter type in `AddControllers` options or register it as a scoped service and use `AddService`. Global filters apply to every controller action unless excluded.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllers(options =>
    options.Filters.AddService<AuditActionFilter>());
```

- `options.Filters.Add<AuditActionFilter>()` resolves the filter from DI when needed.
- Global filters with scoped dependencies must use `AddService<T>()` with `AddScoped<T>()`.
- `[AllowAnonymous]` bypasses authorization filters but not other filter types.
- Order global filters with `IOrderedFilter.Order` when sequence matters.

---

#### Q11. How do you apply a filter to a single action or controller?

**Answer:** Apply filter attributes directly on the controller class or action method — `[ServiceFilter(typeof(AuditActionFilter))]`, `[TypeFilter(typeof(AuditActionFilter))]`, or built-in attributes like `[Authorize]`. Controller-level filters inherit to all actions unless overridden.

- `[ServiceFilter]` resolves the filter from DI — supports constructor injection with proper lifetimes.
- `[TypeFilter]` instantiates the filter with specified constructor arguments.
- Multiple filters on one action run in declaration order within the same filter stage.
- Action-level `[Authorize(Roles = "Admin")]` overrides or narrows controller-level authorization.

---

#### Q12. What is `IAsyncActionFilter`, and how does it differ from `IActionFilter`?

**Answer:** `IAsyncActionFilter` defines a single `OnActionExecutionAsync` method with an `ActionExecutionDelegate next`, enabling async/await around action execution. `IActionFilter` uses synchronous `OnActionExecuting` and `OnActionExecuted` callbacks — blocking async I/O in sync filters causes thread-pool starvation.

- Async filter: `var executed = await next();` then inspect `executed.Result` or `executed.Exception`.
- Sync filter: implement both `OnActionExecuting` and `OnActionExecuted` — no async support.
- Always prefer `IAsyncActionFilter` when the filter awaits databases, HTTP calls, or file I/O.
- Never use `.Result` or `.Wait()` on tasks inside synchronous filter methods.

---

#### Q13. How does `[Authorize]` relate to authorization filters?

**Answer:** `[Authorize]` is implemented as an authorization filter (`AuthorizeFilter`) that evaluates the configured authorization policy against `HttpContext.User` via `IAuthorizationService`. It sets `context.Result` to 401 or 403 when the policy fails, before the action or model binding proceeds.

- Policy names, roles, and schemes come from the attribute properties — `[Authorize(Policy = "CanEdit")]`.
- `[AllowAnonymous]` on an action skips authorization filter enforcement for that endpoint.
- The same policy system backs both `[Authorize]` and minimal API `.RequireAuthorization()`.
- Authentication must run first via `UseAuthentication()` — filters authorize an already populated principal.

---

#### Q14. What is the difference between authentication middleware and authorization filters?

**Answer:** Authentication middleware (`UseAuthentication()`) validates credentials and constructs `HttpContext.User` — it runs for all requests before authorization. Authorization filters run later in the MVC pipeline and decide whether the authenticated (or anonymous) user may execute a specific action.

- Authentication answers "who is this caller?" — JWT, cookies, API keys via authentication handlers.
- Authorization answers "may this caller perform this action?" — roles, policies, claims.
- `UseAuthorization()` middleware enforces endpoint metadata including minimal API routes.
- Authorization filters are the MVC integration point for `[Authorize]` on controllers — both use the same policy infrastructure.

---

#### Q15. Do Minimal APIs use MVC filters?

**Answer:** No — minimal API endpoints do not execute the MVC filter pipeline (`IActionFilter`, `IAuthorizationFilter`, etc.). Cross-cutting logic for minimal APIs uses endpoint filters (`IEndpointFilter`), authorization middleware, or custom middleware instead.

- `[Authorize]` on controllers does not protect minimal API routes registered separately.
- Apply `.RequireAuthorization()` on minimal API endpoints or groups for policy enforcement.
- Use `.AddEndpointFilter<T>()` for validation, logging, and timing around minimal handlers.
- Mixing controllers and minimal APIs requires configuring both filter/middleware and endpoint metadata consistently.

---

#### Q16. What are endpoint filters in Minimal APIs?

**Answer:** Endpoint filters implement `IEndpointFilter` and wrap minimal API route handlers — the closest equivalent to MVC action filters. Register with `.AddEndpointFilter<T>()` on a route or group to run logic before and after the handler delegate.

- Signature: `InvokeAsync(HttpContext, EndpointFilterInvocationContext next)`.
- Can validate arguments in `EndpointFilterInvocationContext.Arguments` before the handler runs.
- Return `Results.Problem()` or other `IResult` to short-circuit without calling the handler.
- Group-level filters apply to every endpoint in a `MapGroup` — useful for shared validation or timing.

---

#### Q17. How does DI work with filters?

**Answer:** Filters are resolved from the DI container when registered via `[ServiceFilter]`, `AddService<T>()`, or `TypeFilter`. Constructor injection works, but filter lifetimes must align with their dependencies — scoped filters for scoped services like `DbContext`.

- Register: `builder.Services.AddScoped<MyActionFilter>()` then `[ServiceFilter(typeof(MyActionFilter))]`.
- Global `options.Filters.Add<MyActionFilter>()` with scoped dependencies requires `AddService<MyActionFilter>()`.
- Injecting scoped `DbContext` into a singleton-cached filter causes captive dependency errors in Production with `ValidateScopes`.
- Use `IServiceScopeFactory` or `IDbContextFactory<T>` inside singleton filters when a scope per invocation is needed.

---

#### Q18. Can a filter short-circuit a request? How?

**Answer:** Yes — filters short-circuit by assigning `context.Result` to an `IActionResult` (authorization, resource, action filters) or returning an `IResult` without calling `next()` (endpoint filters). Subsequent filters and the action are skipped once a result is set.

- Authorization filter: `context.Result = new UnauthorizedResult();` before the action runs.
- Action filter: set `context.Result` in `OnActionExecuting` or before `await next()` in async filters.
- Resource filter: set `context.Result` in `OnResourceExecuting` to skip model binding and the action entirely.
- Endpoint filter: return `Results.BadRequest(...)` without invoking `await next()` to skip the handler.

---

## Chapter 10. Exception Handling

#### Q1. How does ASP.NET Core handle unhandled exceptions by default?

**Answer:** Unhandled exceptions propagate up the middleware pipeline; if nothing catches them, Kestrel returns a generic HTTP 500 response with no useful body for API clients, while Development hosting may show richer diagnostics when configured.

- In **Development**, the default template enables `UseDeveloperExceptionPage()`, which returns an HTML page with exception type, message, and stack trace.
- In **Production**, without custom handling, clients typically receive a blank or minimal 500 response — no stack trace and no structured error shape.
- Exceptions thrown in middleware, filters, or endpoint code all bubble upward until exception-handling middleware, an exception filter, or the host catches them.
- ASP.NET Core 8 encourages `AddProblemDetails()` plus `UseExceptionHandler()` or `IExceptionHandler` for consistent API error responses instead of relying on defaults.

---

#### Q2. What is `UseExceptionHandler` middleware?

**Answer:** `UseExceptionHandler` wraps downstream middleware and endpoints in a try/catch; when an unhandled exception occurs, it re-executes the pipeline on a configured error path or invokes a registered `IExceptionHandler` to produce a safe response.

- Call `app.UseExceptionHandler()` early in the pipeline — typically right after `Build()` — so routing, auth, and endpoints are covered.
- You can pass a path (`UseExceptionHandler("/error")`) or rely on .NET 8's `AddExceptionHandler<T>()` registration for programmatic handling.
- The middleware clears the response, sets an appropriate status code, and writes the error payload without rethrowing to the client.
- It does not replace logging — always log the full exception before returning the sanitized response.

---

#### Q3. What is `DeveloperExceptionPage`, and when is it enabled?

**Answer:** `DeveloperExceptionPage` is middleware that renders a detailed HTML diagnostic page for unhandled exceptions, intended only for local development debugging.

- Enable with `app.UseDeveloperExceptionPage()` when `app.Environment.IsDevelopment()` is true.
- The page shows exception type, message, stack trace, query string, cookies, headers, and routing data — information that must never reach external users.
- The default Web API template enables it only in Development; Production uses `UseExceptionHandler` instead.
- It returns HTML, not JSON — unsuitable as the sole error handler for API-only applications even in Development if clients expect `ProblemDetails`.

---

#### Q4. What is the difference between Development and Production exception behavior?

**Answer:** Development prioritizes developer visibility (stack traces, detailed pages); Production prioritizes security and stable client contracts (generic messages, structured errors, full server-side logging).

- **Development:** `UseDeveloperExceptionPage()` or enriched `ProblemDetails` with `Extensions` containing exception details for local debugging.
- **Production:** `UseExceptionHandler` / `IExceptionHandler` returns RFC 7807 `ProblemDetails` with safe titles and details; stack traces stay in logs only.
- Environment is determined by `ASPNETCORE_ENVIRONMENT` and checked via `IHostEnvironment.IsDevelopment()`.
- Misconfiguring Production as Development exposes internal implementation details and is a common security incident.

---

#### Q5. What is RFC 7807 ProblemDetails?

**Answer:** RFC 7807 defines a standard machine-readable format for HTTP API error responses, using fields such as `type`, `title`, `status`, `detail`, and `instance` to describe problems consistently.

- ASP.NET Core maps `ProblemDetails` to JSON with `Content-Type: application/problem+json`.
- The `status` field mirrors the HTTP status code; `title` gives a short human-readable summary; `detail` explains the specific failure.
- `type` is typically a URI identifying the error category; `instance` identifies the specific request (often the path or trace ID).
- Using ProblemDetails keeps OpenAPI/Swagger, client deserializers, and global handlers aligned on one error contract.

---

#### Q6. How do you return ProblemDetails from an API?

**Answer:** Register ProblemDetails services, then return them from controllers, minimal APIs, or global exception handlers using built-in helpers or explicit `Results.Problem()` / `TypedResults.Problem()` calls.

- Call `builder.Services.AddProblemDetails()` in .NET 8 to enable customization hooks and consistent serialization.
- In controllers: `return Problem(detail: "…", statusCode: 404)` or `return NotFound(new ProblemDetails { … })`.
- In Minimal APIs: `Results.Problem(statusCode: 409, title: "Conflict")` or `TypedResults.Problem(...)` for compile-time typing.
- Global handlers implement `IExceptionHandler.TryHandleAsync` and write `ProblemDetails` via `IProblemDetailsService`.

---

#### Q7. What is `IExceptionHandler` in .NET 8+?

**Answer:** `IExceptionHandler` is a DI-registered service invoked by exception-handling middleware to centralize exception-to-response mapping in a testable, single-responsibility class.

- Register with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and enable via `app.UseExceptionHandler()`.
- Implement `ValueTask<bool> TryHandleAsync(HttpContext, Exception, CancellationToken)` — return `true` when the exception is handled.
- Multiple handlers can be registered; the pipeline tries them in registration order until one returns `true`.
- Prefer `IExceptionHandler` over inline lambda middleware for mapping domain exceptions to status codes and ProblemDetails shapes.

---

#### Q8. What is the difference between `throw;` and `throw ex;`?

**Answer:** `throw;` rethrows the current exception preserving the original stack trace; `throw ex;` rethrows the same exception object but resets the stack trace to the catch block, hiding the true failure site.

- Always use `throw;` when logging and delegating an unhandled failure upward without wrapping.
- Use `throw new DomainException("message", ex)` when intentionally wrapping — the inner exception preserves the original stack in `InnerException`.
- `throw ex;` makes Application Insights, Serilog, and `IExceptionHandler` logs point at the catch block instead of the root cause.
- This applies equally in async code — the stack-trace rule is unchanged after `await`.

---

#### Q9. Why should you log the full exception object, not just `ex.Message`?

**Answer:** Logging only `ex.Message` drops the stack trace, inner exceptions, and structured logging scopes that production diagnostics depend on to find root cause quickly.

- Pass the exception as the first parameter: `_logger.LogError(ex, "Payment failed for {OrderId}", orderId)`.
- Structured logging providers capture exception type, stack, and nested `InnerException` chains automatically.
- Message-only logs cannot distinguish identical messages from different failure locations or underlying causes.
- Global exception handlers should log once at the boundary with full context, then return sanitized ProblemDetails to the client.

---

#### Q10. Where should global exception handling middleware be placed in the pipeline?

**Answer:** Place `UseExceptionHandler()` as early as possible after `Build()`, before routing, authentication, HTTPS redirection, and endpoint middleware, so it wraps the entire downstream pipeline.

- Recommended order start: `UseExceptionHandler()` → `UseForwardedHeaders()` → `UseHttpsRedirection()` → `UseRouting()` → auth → endpoints.
- If placed too late, exceptions thrown in early middleware (e.g., forwarded headers, auth) bypass the handler.
- Exception filters run within MVC's filter pipeline and do not catch middleware exceptions — middleware placement still matters for non-controller code.
- Only one primary exception handler should write the final response; avoid duplicate catch/log/write layers.

---

#### Q11. What is an exception filter, and how does it differ from exception middleware?

**Answer:** An exception filter (`IExceptionFilter` / `IAsyncExceptionFilter`) runs inside the MVC filter pipeline for controller actions; exception middleware wraps the entire ASP.NET Core pipeline including middleware and Minimal APIs.

- Exception filters only apply to MVC controller actions and Razor Pages — not to raw middleware or Minimal API endpoints (unless using endpoint-specific handling).
- Middleware catches exceptions from any downstream component — authentication, custom middleware, minimal routes, and controllers.
- Filters can access `ActionContext`, model state, and action metadata; middleware only sees `HttpContext`.
- Use middleware for API-wide policy; use exception filters only when action-specific context is required and middleware is insufficient.

---

#### Q12. What information should never be exposed to external API clients in error responses?

**Answer:** Never return stack traces, internal file paths, connection strings, SQL queries, server hostnames, dependency versions, or raw exception messages that reveal implementation details.

- Production ProblemDetails should use generic `title`/`detail` text; map sensitive internals to safe, user-facing messages.
- Include a `traceId` (from `HttpContext.TraceIdentifier`) for support correlation without exposing internals.
- Log full exception details server-side with correlation IDs tied to the same trace identifier.
- Validation errors may include field-level messages; unexpected 500 errors should not echo exception types from third-party libraries.

---

#### Q13. How do you map domain exceptions to HTTP status codes centrally?

**Answer:** Implement a single `IExceptionHandler` (or exception middleware) with a switch or dictionary that maps known domain exception types to HTTP status codes and ProblemDetails payloads.

- Define domain exceptions such as `NotFoundException` → 404, `ValidationException` → 400, `ConflictException` → 409, `ForbiddenException` → 403.
- Unmapped exceptions default to 500 with a generic message; log them as errors with full detail.
- Keep controllers thin — throw domain exceptions and let the handler translate; avoid per-action try/catch for predictable failures.
- Register the handler in DI so mapping logic is unit-testable independent of HTTP plumbing.

---

#### Q14. What is `AddProblemDetails()`?

**Answer:** `AddProblemDetails()` registers services that configure and write RFC 7807 ProblemDetails responses, including customization delegates and integration with exception handling in .NET 8.

- Call `builder.Services.AddProblemDetails(options => { … })` to set default `type`, customize `ProblemDetails` per status code, or add `Extensions`.
- Works with `IProblemDetailsService` to customize output in `IExceptionHandler.TryHandleAsync`.
- Replaces older ad-hoc JSON error shapes with a consistent, OpenAPI-friendly format across controllers and Minimal APIs.
- Can add `traceId`, validation errors, and environment-specific detail through the customization callback.

---

#### Q15. What happens when an exception is thrown in middleware vs in a controller action?

**Answer:** Middleware exceptions propagate up until caught by `UseExceptionHandler` or the host; controller exceptions are first seen by MVC exception filters (if registered), then bubble to the same global middleware if unhandled.

- Middleware has no MVC filter pipeline — only global exception middleware or the host handles it.
- Controller exceptions trigger `IExceptionFilter` before reaching exception middleware, allowing action-context-aware handling.
- Both paths should converge on the same ProblemDetails contract to avoid inconsistent API error shapes.
- Minimal API exceptions skip MVC filters entirely — only endpoint filters and global middleware apply.

---

#### Q16. How do you customize error responses per exception type?

**Answer:** In `IExceptionHandler`, pattern-match on exception type (or base types), set `ProblemDetails.Status`, `Title`, `Detail`, and optional `Extensions`, then write the response via `IProblemDetailsService`.

- Use `exception switch` or a dictionary of `Type` → handler delegate for maintainable mapping tables.
- Add custom extension fields (e.g., `errorCode`, `fieldErrors`) through `ProblemDetails.Extensions`.
- Configure status-specific defaults in `AddProblemDetails` for errors not tied to a specific exception type.
- Avoid exposing different response shapes per endpoint — one consistent schema simplifies client error handling.

---

#### Q17. What is the difference between client errors (4xx) and server errors (5xx)?

**Answer:** 4xx indicates the client sent a bad or unauthorized request and can often fix it; 5xx indicates the server failed to fulfill a valid request and the client should retry or contact support.

- **4xx examples:** 400 validation failure, 401 unauthenticated, 403 forbidden, 404 not found, 409 conflict, 422 semantic validation.
- **5xx examples:** 500 unhandled exception, 502 bad gateway, 503 service unavailable — the server or dependency failed unexpectedly.
- Map predictable business-rule violations to 4xx — returning 500 for "not found" breaks monitoring and client retry logic.
- Log 4xx at Warning/Information when expected; log 5xx at Error with full exception detail.

---

#### Q18. How does `[ApiController]` affect exception handling for validation failures?

**Answer:** `[ApiController]` automatically returns HTTP 400 with a ValidationProblemDetails body when model validation fails — before the action runs — without throwing an exception or requiring manual `ModelState` checks.

- Invalid models short-circuit via the automatic `[ApiController]` filter; no exception is thrown for annotation validation failures.
- The response shape is `ValidationProblemDetails` (a ProblemDetails subtype) with an `errors` dictionary keyed by field name.
- This is distinct from unhandled exceptions — validation failures are expected client errors, not 500 server errors.
- Custom validation can still throw domain exceptions that global handlers map separately from automatic 400 responses.

---

## Chapter 11. Static Files & Request Pipeline

#### Q1. What is the purpose of the `wwwroot` folder?

**Answer:** `wwwroot` is the default web root directory whose files are served as public static content — HTML, CSS, JavaScript, images, and fonts — directly over HTTP without hitting controller logic.

- It maps to `IWebHostEnvironment.WebRootPath` and is the default root for `UseStaticFiles()`.
- Files in `wwwroot` are included in publish output and deployed to the server as-is.
- Treat everything under `wwwroot` as publicly accessible to anonymous clients — no authentication gate by default.
- SPA builds (React, Angular, Blazor WASM) typically place `index.html` and bundled assets here.

---

#### Q2. What does `UseStaticFiles()` do?

**Answer:** `UseStaticFiles()` registers middleware that intercepts HTTP requests, maps the URL path to a file under the configured web root, and returns the file with the correct content type and caching headers if the file exists.

- If no matching file is found, the request passes to the next middleware (routing, endpoints).
- Default configuration serves from `wwwroot` with standard MIME type detection via `FileExtensionContentTypeProvider`.
- Customize behavior through `StaticFileOptions` — custom roots, content types, response headers, or request-path prefixes.
- Static file middleware does not execute application code — it is optimized for direct file serving from disk.

---

#### Q3. What is the difference between `UseDefaultFiles()` and `UseStaticFiles()`?

**Answer:** `UseDefaultFiles()` rewrites directory requests (e.g., `/` or `/docs/`) to a default file name like `index.html` without serving content itself; `UseStaticFiles()` actually reads and returns the file bytes.

- Call `UseDefaultFiles()` **before** `UseStaticFiles()` so the rewrite happens before the file lookup.
- Default file names include `index.html`, `index.htm`, `default.html` — configurable via `DefaultFilesOptions`.
- `UseDefaultFiles()` alone does not serve files — it only changes `Path` on the request for the next middleware.
- Together they enable `GET /` to return `wwwroot/index.html` without exposing `/index.html` in the URL.

---

#### Q4. What security risk does `UseDirectoryBrowser()` pose?

**Answer:** `UseDirectoryBrowser()` enables HTML directory listings, exposing file and folder names to anyone who can reach the URL — a serious information disclosure vulnerability on public sites.

- Attackers enumerate backup files, source maps, upload folders, and forgotten assets without guessing paths.
- It is acceptable only in controlled Development environments — never enable on Production public hosts.
- Prefer explicit file serving via `UseStaticFiles()` with known file names; hide directory structure entirely.
- Combine with misconfigured upload directories and it accelerates discovery of sensitive or executable files.

---

#### Q5. Where should static file middleware be placed in the pipeline?

**Answer:** Place `UseDefaultFiles()` and `UseStaticFiles()` after routing is established but before endpoint execution — typically after `UseRouting()` and alongside or before `UseAuthentication()`, depending on whether static files need auth (usually they do not).

- Map API endpoints (`MapControllers`, `MapGroup`) before SPA fallback so `/api/*` routes are not swallowed by static handling.
- Common order: exception handler → forwarded headers → HTTPS → routing → auth → endpoints → default files → static files → SPA fallback.
- Static middleware short-circuits when a file matches — order relative to routing affects whether endpoint or file wins.
- For SPAs, fallback to `index.html` must come **after** API endpoint mapping.

---

#### Q6. How do you serve a Single Page Application (SPA) with ASP.NET Core?

**Answer:** Publish the SPA build output to `wwwroot`, serve static assets with `UseStaticFiles()`, and add a fallback route that returns `index.html` for client-side routes not matched by API endpoints or static files.

- Build the SPA (npm/vite/webpack) and copy `dist/` contents into `wwwroot`.
- Use `UseDefaultFiles()` + `UseStaticFiles()` for root and asset paths.
- Add `app.MapFallbackToFile("index.html")` after API routes so deep links like `/orders/123` work on refresh.
- In Production, many teams serve static assets from a CDN while Kestrel handles API and fallback only.

---

#### Q7. What is SPA fallback routing, and why is it needed?

**Answer:** SPA fallback routing returns `index.html` for URLs that do not match a physical file or server endpoint, allowing the client-side router to handle navigation for deep links and browser refreshes.

- Without fallback, refreshing `/dashboard/settings` returns 404 because no server file exists at that path.
- Fallback runs only when no endpoint matched and no static file was found — it is a last-resort catch-all.
- The client router (React Router, Vue Router) reads the URL and renders the correct view after `index.html` loads.
- Fallback must not intercept valid API 404 responses unless intentionally unified — order API mapping first.

---

#### Q8. How do you prevent SPA fallback from intercepting API routes?

**Answer:** Register all API endpoints before `MapFallbackToFile`, or scope fallback to non-API paths using conditional mapping such as `MapFallbackToFile("index.html").Add(builder => !builder.Request.Path.StartsWithSegments("/api"))`.

- `MapControllers()` or `MapGroup("/api")` must be registered before the fallback delegate runs.
- If fallback is registered first, `/api/unknown` returns `index.html` with HTTP 200 — breaking API clients silently.
- Use path prefixes consistently — `/api` for backend, everything else for SPA.
- Test with `curl` against unknown API paths to confirm JSON 404, not HTML 200.

---

#### Q9. What are MIME types, and how does ASP.NET Core determine them for static files?

**Answer:** MIME types (Content-Type values) tell browsers how to interpret file bytes; ASP.NET Core maps file extensions to MIME types via `FileExtensionContentTypeProvider` when serving static files.

- `.html` → `text/html`, `.css` → `text/css`, `.js` → `application/javascript`, `.png` → `image/png`.
- Wrong MIME types cause browsers to block scripts (CSP) or misrender downloads instead of displaying pages.
- Customize mappings in `StaticFileOptions.ContentTypeProvider` for uncommon extensions (`.wasm`, `.webp`, custom fonts).
- `ServeUnknownFileTypes` falls back to `application/octet-stream` when enabled — use cautiously.

---

#### Q10. What is `StaticFileOptions`?

**Answer:** `StaticFileOptions` configures static file middleware behavior — file provider root, request path prefix, content type mappings, default content type, response headers, and whether unknown file types are served.

- Pass to `app.UseStaticFiles(new StaticFileOptions { … })` or configure via `IOptions<StaticFileOptions>`.
- `FileProvider` + `RequestPath` serve files from a non-`wwwroot` folder at a URL subpath (e.g., `/images`).
- `OnPrepareResponse` adds cache-control, security headers, or auth checks per file response.
- Enables serving uploaded content from a controlled directory without exposing the entire project tree.

---

#### Q11. What is `FileExtensionContentTypeProvider`?

**Answer:** `FileExtensionContentTypeProvider` is a dictionary-backed lookup that maps file extensions (e.g., `.json`, `.svg`) to IANA MIME type strings for the `Content-Type` response header.

- Default provider covers common web extensions; extend with `provider.Mappings[".myext"] = "application/x-custom"`.
- Used internally by static files middleware and available for manual content-type resolution in downloads.
- Missing mappings cause 404 or `application/octet-stream` depending on `ServeUnknownFileTypes` setting.
- Keep mappings explicit for security-sensitive types — do not serve `.config` or `.cs` with guessed types.

---

#### Q12. What does `ServeUnknownFileTypes` do, and when is it risky?

**Answer:** When `ServeUnknownFileTypes` is `true`, static files middleware serves files whose extension has no MIME mapping using `DefaultContentType` (typically `application/octet-stream`) instead of returning 404.

- Risky on public sites — unknown extensions like `.config`, `.bak`, or `.env` may become downloadable if placed in the served root.
- Browsers may sniff content and execute HTML/JS served with wrong types, enabling XSS in edge cases.
- Prefer explicitly registering needed extensions in `ContentTypeProvider` rather than allowing all unknown types.
- Default is `false` — unknown extensions are not served, which is the safer Production default.

---

#### Q13. What is a path traversal attack in the context of file serving?

**Answer:** Path traversal exploits insufficient path sanitization so attackers request URLs containing `../` sequences to read files outside the intended directory — such as `GET /files/../../appsettings.json`.

- Occurs when custom file-serving code concatenates user input into file paths without normalization and boundary checks.
- Built-in `UseStaticFiles()` normalizes paths and restricts lookups to the configured `FileProvider` root — generally safe when not misconfigured.
- Custom download endpoints must call `Path.GetFullPath` and verify the resolved path starts with the allowed base directory.
- Never serve files based on raw user-supplied paths without validation.

---

#### Q14. How do you set cache-control headers for static assets?

**Answer:** Use `StaticFileOptions.OnPrepareResponse` to append `Cache-Control`, `ETag`, and `Expires` headers, or configure caching at the reverse proxy/CDN layer for Production static assets.

- Example: in `OnPrepareResponse`, set `ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable"` for fingerprinted assets.
- Kestrel static files support conditional requests via `Last-Modified` and `ETag` automatically for cache validation.
- For `index.html`, set `no-cache` or short `max-age` so deployments propagate quickly to clients.
- CDN edge caching is preferred for global scale; Kestrel headers still matter for direct-host scenarios.

---

#### Q15. Why should hashed JS/CSS files be cached aggressively but `index.html` should not?

**Answer:** Fingerprinted assets (e.g., `main.a1b2c3.js`) have unique URLs that change on every build — long cache lifetimes are safe because a new deployment uses new file names; `index.html` references those names and must be fetched fresh to pick up new bundles.

- Aggressive caching (`max-age=31536000, immutable`) for hashed files maximizes CDN and browser performance.
- Caching `index.html` aggressively causes clients to load stale bundle references after deployments, breaking the app.
- Set `Cache-Control: no-cache` on `index.html` so browsers revalidate and get updated script references.
- This cache-busting strategy is standard for Vite, Webpack, and Angular CLI production builds.

---

#### Q16. What is the difference between serving files from `wwwroot` vs a custom folder?

**Answer:** `wwwroot` is the conventional, publish-included web root served by default; a custom folder requires explicit `StaticFileOptions` with a `PhysicalFileProvider` and optional `RequestPath` prefix.

- `wwwroot` files deploy automatically with `dotnet publish` — no extra configuration.
- Custom folders (e.g., `Uploads/`, `/var/app/assets`) need explicit middleware registration and correct publish/copy rules.
- `RequestPath` maps a URL prefix (`/downloads`) to a physical directory outside `wwwroot`.
- Custom roots are useful for user-generated content but require stronger path validation and access control.

---

#### Q17. Can static files be served without placing them in `wwwroot`?

**Answer:** Yes — configure `UseStaticFiles` with `StaticFileOptions.FileProvider = new PhysicalFileProvider(path)` and optionally `RequestPath` to expose any directory the process can read.

- Multiple `UseStaticFiles` calls can serve different roots at different URL prefixes in the same app.
- Files outside `wwwroot` are not published by default — ensure deployment copies or generates them on the server.
- Razor Class Libraries can embed static assets served via `_content/{LibraryName}/` without copying to `wwwroot`.
- Every additional served root expands the attack surface — restrict to necessary directories only.

---

#### Q18. What happens if sensitive files (e.g., `.env`, `appsettings.Production.json`) are placed in `wwwroot`?

**Answer:** They become anonymously downloadable at their URL path — `GET /appsettings.Production.json` returns secrets including connection strings, API keys, and credentials to any visitor.

- Static file middleware has no authentication — everything in the served root is public.
- Secrets belong in environment variables, Azure Key Vault, or secret managers — never in `wwwroot` or any publicly mapped folder.
- CI/CD should validate publish output does not copy config or `.env` files into web roots.
- Source maps (`.js.map`) in `wwwroot` similarly expose original source structure to attackers.

---

## Chapter 12. Hosting, Kestrel & Environments

#### Q1. What is Kestrel?

**Answer:** Kestrel is the cross-platform, high-performance web server built into ASP.NET Core that listens for HTTP/HTTPS connections and drives the request pipeline.

- It runs on Windows, Linux, and macOS — the default server for `dotnet run`, containers, and cloud deployments.
- Kestrel handles connection management, HTTP parsing, TLS (when configured), and passes requests to middleware/endpoints.
- It is optimized for async I/O and integrates tightly with the .NET thread pool and DI container.
- Production deployments often place a reverse proxy in front of Kestrel for TLS termination and edge policies.

---

#### Q2. What is the role of IIS in hosting ASP.NET Core on Windows?

**Answer:** IIS acts as a reverse proxy and process manager for ASP.NET Core on Windows via the ASP.NET Core Module (ANCM) — handling Windows integration, certificate binding, and worker process lifecycle while Kestrel executes application code.

- **In-process:** the app runs inside the IIS worker process (`w3wp.exe`) for lowest latency and shared Windows Auth.
- **Out-of-process:** IIS forwards requests to a separate Kestrel `dotnet` process — better crash isolation.
- IIS provides app pool recycling, request filtering, centralized logging, and familiar Windows admin tooling.
- IIS does not replace Kestrel — it forwards to or hosts the Kestrel-based ASP.NET Core runtime.

---

#### Q3. What is a reverse proxy, and why use one with ASP.NET Core?

**Answer:** A reverse proxy sits in front of Kestrel, receives client traffic, and forwards it to backend app instances — handling TLS, load balancing, compression, caching, WAF, and rate limiting at the edge.

- Examples: nginx, IIS, YARP, Azure Front Door, AWS ALB, Cloudflare.
- Kestrel focuses on running .NET efficiently; the proxy handles infrastructure concerns at scale.
- Proxies enable zero-downtime deployments, multiple instances, and SSL certificate centralization.
- Without a proxy, Kestrel must handle all edge concerns directly — workable but less common in Production.

---

#### Q4. Who terminates TLS in a typical nginx + Kestrel deployment?

**Answer:** nginx terminates TLS at the edge — clients connect via HTTPS to nginx, which decrypts traffic and forwards plain HTTP to Kestrel on an internal port (e.g., 5000 or 8080).

- SSL certificates are installed on nginx, not necessarily on each Kestrel instance.
- Kestrel may still be configured for HTTPS internally, but the common pattern is HTTP behind the proxy on a trusted network.
- `X-Forwarded-Proto: https` tells Kestrel the original scheme so redirects and link generation use HTTPS.
- Terminating TLS at the edge simplifies certificate rotation and cipher policy management.

---

#### Q5. What is `ASPNETCORE_ENVIRONMENT`?

**Answer:** `ASPNETCORE_ENVIRONMENT` is an environment variable that sets the hosting environment name (e.g., `Development`, `Staging`, `Production`) consumed at startup by the generic host.

- Read via `IHostEnvironment.EnvironmentName` or `IWebHostEnvironment` in application code.
- Controls which `appsettings.{Environment}.json` file merges into configuration.
- Gates conditional startup logic — developer exception page, Swagger UI, verbose logging.
- Must be set explicitly in Production deployments — it defaults to `Production` when unset in many hosts but should never be left ambiguous.

---

#### Q6. What is `IHostEnvironment` / `IWebHostEnvironment`?

**Answer:** These DI-injected abstractions expose hosting context — environment name, content root path, application name, and (for web) web root path — without reading environment variables directly.

- `IHostEnvironment` is available in any .NET host; `IWebHostEnvironment` adds `WebRootPath` for web apps.
- `IsDevelopment()`, `IsStaging()`, `IsProduction()` are convenience checks against `EnvironmentName`.
- `ContentRootPath` is the application base directory (where `appsettings.json` lives); `WebRootPath` points to `wwwroot`.
- Prefer injecting these interfaces over hard-coding environment checks against raw strings.

---

#### Q7. How does the environment name affect application behavior?

**Answer:** The environment name selects configuration files, logging verbosity, middleware branches, and feature toggles registered conditionally at startup.

- `appsettings.Development.json` overrides base settings when `EnvironmentName` is `Development`.
- Templates enable Swagger, developer exception pages, and detailed errors only in Development.
- `ValidateOnStart`, EF migrations, and seed data may run differently per environment.
- Production should disable diagnostic endpoints, minimize log noise, and enforce strict exception handling.

---

#### Q8. What is `ASPNETCORE_URLS`?

**Answer:** `ASPNETCORE_URLS` is an environment variable that sets the addresses Kestrel listens on, using semicolon-separated URLs such as `http://0.0.0.0:8080` or `https://localhost:5001;http://localhost:5000`.

- Overrides default `launchSettings.json` URLs when set — common in Docker and Kubernetes.
- Read at startup by Kestrel endpoint configuration before the first request.
- In containers, typically set to `http://+:8080` to bind all interfaces on port 8080.
- Distinct from reverse proxy external ports — `ASPNETCORE_URLS` controls Kestrel's bind addresses inside the container or process.

---

#### Q9. How do you configure Kestrel to listen on a specific port?

**Answer:** Configure endpoints via `ASPNETCORE_URLS`, `launchSettings.json`, `appsettings.json` (`Kestrel:Endpoints`), or programmatic `builder.WebHost.ConfigureKestrel()`.

- Environment variable: `ASPNETCORE_URLS=http://0.0.0.0:5000`.
- `appsettings.json`: `"Kestrel": { "Endpoints": { "Http": { "Url": "http://localhost:5050" } } }`.
- Code: `builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(8080))` for explicit control including HTTPS certificates.
- Docker maps container port via `EXPOSE`/`docker run -p` — Kestrel must listen on the container's internal port.

---

#### Q10. What is the difference between Kestrel endpoint configuration and IIS bindings?

**Answer:** Kestrel endpoint configuration (`ASPNETCORE_URLS`, `Kestrel:Endpoints`, `ConfigureKestrel`) defines which addresses the .NET process listens on; IIS bindings define which URLs IIS accepts on the machine and how it forwards to Kestrel.

- IIS bindings include hostname, port, and certificate at the IIS site level — Kestrel may never see HTTPS directly.
- Out-of-process IIS forwards to a localhost port configured by ANCM (e.g., random high port or `aspnetcore` settings).
- In-process hosting merges IIS and Kestrel — IIS bindings are authoritative for incoming traffic.
- Misaligned ports between IIS forwarding and Kestrel listening cause 502.5 process failures.

---

#### Q11. What are forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`)?

**Answer:** Forwarded headers are HTTP headers set by reverse proxies to pass the original client IP, scheme, and host to the backend app, which otherwise sees only the proxy's connection details.

- `X-Forwarded-For` — original client IP address chain.
- `X-Forwarded-Proto` — original scheme (`http` or `https`).
- `X-Forwarded-Host` — original Host header from the client request.
- Without them, `HttpContext.Connection.RemoteIpAddress` is the proxy IP and `Request.Scheme` is `http` even for HTTPS clients.

---

#### Q12. What does `UseForwardedHeaders()` do, and why must it run early?

**Answer:** `UseForwardedHeaders()` reads `X-Forwarded-*` headers from trusted proxies and updates `HttpContext.Connection.RemoteIpAddress`, `Request.Scheme`, and `Request.Host` before downstream middleware uses them.

- Must run **first** (immediately after `Build()`) — before HTTPS redirection, authentication, link generation, and logging.
- Configure `ForwardedHeadersOptions.KnownProxies` or `KnownNetworks` so untrusted clients cannot spoof headers.
- Required behind nginx, IIS as reverse proxy, Azure App Service, and load balancers for correct URLs and audit logs.
- Late placement causes auth cookies, redirects, and `CreatedAtAction` URLs to use wrong scheme/IP.

---

#### Q13. What is the difference between in-process and out-of-process IIS hosting?

**Answer:** In-process runs the ASP.NET Core app inside the IIS worker process; out-of-process runs Kestrel in a separate `dotnet` process with IIS proxying requests to it.

- **In-process:** lower latency, shared application pool lifecycle, app runs as IIS native module — Windows-only.
- **Out-of-process:** Kestrel is a separate process — IIS restarts independently; crash in app does not necessarily recycle IIS worker.
- Both use ANCM; the hosting model is set in the `.csproj` via `<AspNetCoreHostingModel>InProcess|OutOfProcess</AspNetCoreHostingModel>`.
- Containers and Linux always use Kestrel directly — IIS hosting is Windows-specific.

---

#### Q14. What are health checks in ASP.NET Core?

**Answer:** Health checks are registered endpoints that report application and dependency readiness — database connectivity, disk space, downstream APIs — for orchestrators and load balancers to make routing decisions.

- Register with `builder.Services.AddHealthChecks()` and map via `app.MapHealthChecks("/health")`.
- Return `Healthy`, `Degraded`, or `Unhealthy` with optional detailed JSON via `UIResponseWriter` or custom writers.
- Kubernetes uses them for liveness and readiness probes; Azure App Service uses them for load balancer routing.
- Separate checks for "is process alive" vs "can serve traffic" map to liveness vs readiness probes.

---

#### Q15. What is the difference between a liveness probe and a readiness probe?

**Answer:** A liveness probe asks whether the process is alive and should be restarted if failing; a readiness probe asks whether the instance is ready to receive traffic and should be removed from the load balancer if failing.

- **Liveness:** basic self-check — hung deadlocks may fail liveness and trigger pod restart.
- **Readiness:** dependency checks (DB, cache, migrations complete) — failing readiness removes the pod from service without restarting it.
- Map liveness to a lightweight `/health/live` and readiness to `/health/ready` with dependency checks in Kubernetes.
- Do not include slow external calls in liveness — false restarts cause cascading outages.

---

#### Q16. How do container `EXPOSE` directives relate to Kestrel listening ports?

**Answer:** `EXPOSE` documents which port the container listens on; Kestrel must actually bind to that port via `ASPNETCORE_URLS` — `EXPOSE` alone does not open or configure the listener.

- Example Dockerfile: `ENV ASPNETCORE_URLS=http://+:8080` + `EXPOSE 8080` + `docker run -p 8080:8080`.
- Mismatch between `ASPNETCORE_URLS` and `-p` mapping causes connection refused or wrong-port routing.
- Kubernetes `containerPort` must align with Kestrel's bind port inside the pod.
- The host maps external ports — Kestrel typically binds `0.0.0.0` inside the container, not `localhost` only.

---

#### Q17. What breaks if Production is accidentally set to Development?

**Answer:** The app exposes developer exception pages with stack traces, may enable Swagger UI publicly, uses Development configuration overrides, and applies verbose logging — creating security vulnerabilities and performance overhead.

- Sensitive configuration from `appsettings.Development.json` may merge into runtime settings.
- Attackers receive detailed error responses revealing code paths, dependencies, and connection hints.
- CORS, auth, and feature flags tied to Development may allow unintended access.
- Monitoring alerts spike from verbose logs; compliance audits flag exposed diagnostic endpoints.

---

#### Q18. What belongs in `appsettings.Development.json` vs environment variables in Production?

**Answer:** `appsettings.Development.json` holds local-only settings — detailed logging, local connection strings, Swagger toggles, and seed flags safe for developer machines; Production secrets and environment-specific values belong in environment variables or a secret manager, not committed JSON files.

- Development file: `"Logging:LogLevel:Default": "Debug"`, localhost DB strings, `EnableSensitiveDataLogging`.
- Production: connection strings, API keys, and certificates via environment variables, Azure Key Vault, AWS Secrets Manager, or Kubernetes secrets.
- Never commit Production secrets to source control — inject at deploy time.
- Non-secret Production tuning (log levels, feature flags) can use `appsettings.Production.json` deployed without secrets or environment-variable overrides with higher precedence.

---

## Chapter 13. Minimal APIs

#### Q1. What are Minimal APIs in ASP.NET Core?

**Answer:** Minimal APIs are a lightweight way to define HTTP endpoints directly on the application builder without creating MVC controller classes, using `MapGet`, `MapPost`, and related extension methods to bind routes to delegates or methods in `Program.cs` or extension classes.

- They were introduced to reduce ceremony for small services, microservices, and prototypes while still running on the same Kestrel host, middleware pipeline, and dependency injection container as controller-based apps.
- A minimal endpoint is still a first-class routed endpoint in ASP.NET Core 8 — it participates in endpoint routing, authorization metadata, OpenAPI generation, and endpoint filters.
- Handlers can be lambda expressions, local functions, or static/instance methods registered through extension methods such as `TodoEndpoints.Map(app)`.
- They compile to the same hosting model as `WebApplication.CreateBuilder` — there is no separate runtime; only the API surface area is smaller.

---

#### Q2. How do Minimal APIs differ from controller-based APIs?

**Answer:** Controller-based APIs organize endpoints in classes inheriting `ControllerBase` with action methods and `[ApiController]` conventions, while Minimal APIs map routes to functions with explicit metadata and opt-in validation rather than inheriting MVC's opinionated defaults.

- Controllers get automatic model validation responses (400 ProblemDetails) through `[ApiController]`; Minimal APIs require endpoint filters or manual validation unless you add those behaviors explicitly.
- MVC uses action filters, authorization filters, and result filters; Minimal APIs use endpoint filters and middleware instead — there is no `[Authorize]` attribute unless you apply `.RequireAuthorization()` on the route.
- Controllers return `IActionResult`/`ActionResult<T>` executed by MVC infrastructure; Minimal APIs prefer `IResult`/`TypedResults`, which write responses directly without invoking MVC result executors.
- Controllers scale well for large teams with established folder conventions (`Controllers/`, `Services/`); Minimal APIs scale when you enforce the same separation through extension methods and avoid a monolithic `Program.cs`.

---

#### Q3. How do you define a GET endpoint with Minimal APIs?

**Answer:** Call `app.MapGet` with a route template and a handler delegate that returns a response type or `IResult`, registering the endpoint during application configuration after `builder.Build()`.

```csharp
app.MapGet("/weather", () => new[] { "Sunny", "Cloudy" });

app.MapGet("/items/{id:int}", (int id, IItemService items) =>
    items.Find(id) is { } item ? Results.Ok(item) : Results.NotFound());
```

- Route parameters bind by name to handler parameters when types are compatible (`{id:int}` binds to `int id`).
- Services such as `IItemService` resolve from the request scope automatically when listed as handler parameters.
- Returning a plain object implicitly produces `200 OK` with JSON serialization using the configured `System.Text.Json` options.
- Named endpoints (`.WithName("GetItem")`) support link generation for `Created`/`Accepted` responses and OpenAPI operation IDs.

---

#### Q4. What is `MapGet`, `MapPost`, `MapPut`, `MapDelete`?

**Answer:** These are extension methods on `IEndpointRouteBuilder` (typically `WebApplication`) that register HTTP endpoints for a specific verb and path template, connecting them to a handler delegate and optional metadata.

- `MapGet` handles GET, `MapPost` handles POST, `MapPut` handles PUT, and `MapDelete` handles DELETE — each rejects requests using other verbs for that route unless you chain additional maps.
- They return `RouteHandlerBuilder`, which supports fluent configuration: `.Produces<T>()`, `.RequireAuthorization()`, `.WithTags()`, `.AddEndpointFilter<T>()`, and `.WithName()`.
- `MapMethods` and `Map` provide lower-level control when you need custom verbs or non-standard routing branches.
- Under endpoint routing, these calls add `Endpoint` objects to the route table consumed by the routing middleware at request time.

---

#### Q5. How does parameter binding work in Minimal API route handlers?

**Answer:** The minimal hosting binder resolves handler parameters from route values, query strings, headers, the request body, dependency injection, and framework types such as `HttpContext` and `CancellationToken`, using attributes to disambiguate when multiple sources could apply.

- Route template values bind by parameter name (`/users/{userId}` → `Guid userId`); built-in route constraints (`:int`, `:guid`) validate format before the handler runs.
- `[FromBody]` binds JSON once per request; `[FromQuery]` and `[FromHeader]` select alternate sources; `[FromServices]` forces DI resolution even when a name collision exists.
- Types registered in DI (`DbContext`, repositories, `IOptions<T>`) bind from `HttpContext.RequestServices` using the per-request scope.
- `[AsParameters]` on a record or class aggregates multiple bindable properties (route + query + body) into one parameter object with predictable property-level binding.

---

#### Q6. What is the difference between `Results.Ok()` and `TypedResults.Ok()`?

**Answer:** Both return an `IResult` that writes a 200 OK response, but `TypedResults.Ok<T>(T value)` preserves the generic response type at compile time so OpenAPI tools and source generators can infer accurate response schemas.

- `Results.Ok(dto)` works at runtime but often appears as an untyped or loosely typed schema in Swagger/OpenAPI documents.
- `TypedResults.Ok<OrderDto>(order)` emits metadata that `AddOpenApi` and Swashbuckle use to document the response body shape and status code explicitly.
- Both avoid allocating MVC `ObjectResult` infrastructure — the minimal pipeline executes `IResult.ExecuteAsync` directly.
- Prefer `TypedResults` when client code generation, contract testing, or Native AOT trimming depends on strongly typed endpoint metadata.

---

#### Q7. What is `IResult`, and why use it?

**Answer:** `IResult` is the interface implemented by built-in minimal API response helpers (`Results`, `TypedResults`) that encapsulates how to write an HTTP response, including status code, headers, and body, without going through MVC result executors.

- Implementations such as `Ok<T>`, `NotFound`, `ValidationProblem`, and `Redirect` know how to serialize themselves through `ExecuteAsync(HttpContext)`.
- Returning `IResult` makes alternate response shapes explicit in the handler signature (`Task<IResult>`), which improves readability compared to magic implicit status codes.
- Chaining `.Produces<T>(StatusCodes.Status200OK)` on the route adds metadata even when the handler returns a custom `IResult` implementation.
- Custom types can implement `IResult` for specialized response formatting while staying compatible with the minimal hosting pipeline.

---

#### Q8. What are endpoint filters in Minimal APIs?

**Answer:** Endpoint filters are hooks that run immediately before and after a minimal API route handler, similar to action filters in MVC but scoped to a single endpoint or group, enabling validation, logging, and short-circuiting without global middleware.

- Register with `.AddEndpointFilter<ValidationFilter>()` on a route or `MapGroup`, or register a global filter through DI as `IEndpointFilter`.
- Filters receive `EndpointFilterInvocationContext` with bound arguments and can return a result early (for example `Results.ValidationProblem`) without calling the next delegate.
- They run after routing and model binding but before the handler executes, making them the idiomatic place for request validation in Minimal APIs.
- Unlike middleware, endpoint filters see the specific bound parameters for that route and can access endpoint metadata such as authorization requirements.

---

#### Q9. How do you add validation to a Minimal API endpoint?

**Answer:** Minimal APIs do not automatically validate models the way `[ApiController]` does, so you add validation through endpoint filters, third-party libraries such as FluentValidation, or built-in validation extensions that inspect bound parameters before the handler runs.

- An endpoint filter can iterate `context.Arguments`, run `Validator.TryValidateObject` or FluentValidation's `ValidateAsync`, and return `Results.ValidationProblem(errors)` on failure.
- Data annotations on DTOs work when you explicitly invoke validation — they are not enforced unless a filter or helper calls a validator.
- ASP.NET Core 8 templates may include `.AddValidation()` extensions that wire common validation behavior for minimal endpoints.
- Combine validation filters with `[AsParameters]` record types so route, query, and body fields validate as one cohesive request object.

---

#### Q10. How do you organize Minimal APIs with `MapGroup`?

**Answer:** `MapGroup` creates a route prefix and shared configuration for related endpoints, letting you apply tags, authorization, filters, and OpenAPI metadata once instead of repeating it on every route.

```csharp
var todos = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .RequireAuthorization();

todos.MapGet("/", GetAll);
todos.MapPost("/", Create);
todos.MapGet("/{id:int}", GetById);
```

- Groups inherit fluent metadata — `.RequireAuthorization("PolicyName")` on the group protects all mapped child routes unless a route calls `.AllowAnonymous()`.
- Nested groups compose for versioning (`/api/v1`, `/api/v2`) or domain boundaries (`/api/billing`, `/api/inventory`).
- Extract group registration into static extension methods (`TodoEndpoints.Map(app)`) to keep `Program.cs` readable as the API grows.
- `MapGroup` organizes URLs and cross-cutting concerns; it does not version contracts — separate DTO types per major version remain necessary.

---

#### Q11. How do you apply authorization to Minimal API endpoints?

**Answer:** Register authentication and authorization services, place `UseAuthentication()` and `UseAuthorization()` in the middleware pipeline, define policies with `AddAuthorizationBuilder()`, and call `.RequireAuthorization()` or `.RequireAuthorization("PolicyName")` on routes or groups.

- JWT bearer example: `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` plus `AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("Admin"))`.
- Apply per route: `app.MapDelete("/admin/users/{id}", Handler).RequireAuthorization("AdminOnly");`
- Public endpoints omit authorization metadata or call `.AllowAnonymous()` when a fallback policy requires authentication globally.
- Middleware order matters: authentication and authorization must run after routing (`UseRouting`) and before the endpoint executes in ASP.NET Core 8.

---

#### Q12. How does OpenAPI/Swagger discover Minimal API endpoints?

**Answer:** Register `AddEndpointsApiExplorer()` (for Swashbuckle) or `AddOpenApi()` (built-in .NET 8 OpenAPI support), map endpoints with discoverable metadata, and use `TypedResults` plus `.Produces<T>()` so schema generators infer request and response types.

- `WithName`, `WithTags`, `WithSummary`, and `WithDescription` attach documentation metadata consumed by OpenAPI generators.
- Returning opaque types or untyped `object` yields empty or generic schemas — `TypedResults.Ok<CustomerDto>(dto)` fixes inference for client code generation.
- Swashbuckle requires `AddSwaggerGen()` and middleware (`UseSwagger`, `UseSwaggerUI`); .NET 8's `MapOpenApi()` serves the document without Swashbuckle if configured.
- Internal routes can opt out with `.ExcludeFromDescription()` so they do not appear in the public API document.

---

#### Q13. What is `ExcludeFromDescription()` used for?

**Answer:** `ExcludeFromDescription()` marks a minimal API endpoint so OpenAPI/Swagger generators omit it from the published API description, which is useful for internal diagnostics, health probes, or admin-only operations you do not want in client-facing contracts.

- The endpoint remains fully callable at runtime — exclusion affects documentation only, not routing or authorization.
- Typical uses include `/internal/reload-cache`, operational hooks, or duplicate routes kept for backward compatibility during migration.
- Combine with proper authorization on sensitive routes; excluding an endpoint from Swagger does not secure it.
- Controllers have analogous mechanisms (`[ApiExplorerSettings(IgnoreApi = true)]`); minimal APIs use the fluent `.ExcludeFromDescription()` method.

---

#### Q14. When would you choose Minimal APIs over controllers?

**Answer:** Choose Minimal APIs when the service is small, the team wants minimal ceremony, or you need fast iteration on a focused HTTP surface — microservices, internal tools, prototypes, and simple CRUD APIs are common fits.

- Fewer files and no controller base-class hierarchy reduce boilerplate when endpoint count is low and cross-cutting rules are simple.
- Cloud-native and containerized workloads benefit from a compact startup path and straightforward `Program.cs` when paired with extension-method organization.
- Performance characteristics are comparable to controllers for typical JSON APIs — the choice is primarily about structure and team conventions, not raw throughput.
- Minimal APIs still support DI, middleware, auth, validation filters, and OpenAPI when configured explicitly.

---

#### Q15. When would Minimal APIs become a poor long-term choice?

**Answer:** Minimal APIs become harder to maintain when the API grows large, many cross-cutting concerns stack up, or the team relies on MVC conventions for consistency — a sprawling `Program.cs` with inline lambdas and no test seams is the common failure mode.

- Large teams often prefer controllers for discoverability (`Controllers/OrdersController.cs`), consistent filter usage, and established code-review patterns.
- Complex validation, versioning, and hypermedia requirements need disciplined extension methods, filters, and DTO separation — without that discipline, minimal APIs accumulate "god file" debt faster than controllers.
- If the project already standardizes on MVC patterns (areas, view results, complex filter pipelines), forcing Minimal APIs splits conventions across services.
- Minimal APIs are not wrong at scale, but they require the same architectural boundaries as controllers — handlers in dedicated classes, filters for validation, and DI for all infrastructure.

---

#### Q16. How is DI used in Minimal API handlers?

**Answer:** Handler parameters are resolved from the per-request `IServiceProvider` (`HttpContext.RequestServices`), so any service registered in DI can be injected by listing its type as a parameter — constructor injection applies to named methods, not inline lambdas that capture no service provider.

- Scoped services (`DbContext`, repositories) work correctly when resolved per invocation because each request creates a scope.
- Singleton services resolve from the root provider; avoid capturing scoped services in closures created at startup (captive dependency).
- `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` bind like any other service; snapshot refreshes per request when configuration reloads.
- For lambdas that need many dependencies, prefer static handler methods or class-based handlers registered through DI (`AddScoped<OrderHandler>()` + method group reference).

---

#### Q17. What is `AddEndpointsApiExplorer()`?

**Answer:** `AddEndpointsApiExplorer()` registers the API explorer services that collect endpoint metadata (HTTP methods, routes, parameters, response types) from minimal endpoints and controllers, enabling Swashbuckle and other tools to generate OpenAPI documents.

- It implements `IApiDescriptionGroupCollectionProvider`, which Swashbuckle's `SwaggerGen` consumes to build schemas and operation lists.
- Call it in the service configuration phase: `builder.Services.AddEndpointsApiExplorer();` alongside `AddSwaggerGen()` when using Swashbuckle.
- .NET 8's built-in `AddOpenApi()` provides an alternative pipeline that also relies on endpoint metadata being present and correctly typed.
- Without an API explorer and without explicit `.Produces<T>()` metadata, minimal endpoints may appear in Swagger with incomplete or generic schemas.

---

#### Q18. How do you return HTTP 201 Created from a Minimal API?

**Answer:** Return `Results.Created(uri, value)` or `TypedResults.Created<Uri, T>(uri, value)` from the handler, supplying the URI of the newly created resource for the `Location` header and the response body DTO.

```csharp
app.MapPost("/orders", async (CreateOrderRequest req, IOrderService svc) =>
{
    var order = await svc.CreateAsync(req);
    return TypedResults.Created($"/orders/{order.Id}", order);
});
```

- The first argument is the resource URI (string or `Uri`) placed in the `Location` header — it should identify the new entity, not the collection URL.
- The response body typically contains the created representation or a subset; status code is 201 automatically.
- Use `.WithName("CreateOrder")` and link generation helpers when the URI should be generated from route names rather than string interpolation.
- `Results.CreatedAtRoute` is available when named routes are defined, analogous to MVC's `CreatedAtAction`.

---

## Chapter 14. Background & Hosted Services

#### Q1. What is a hosted service in ASP.NET Core?

**Answer:** A hosted service is a class registered with the generic host that runs background work tied to the application lifetime, starting when the host starts and stopping gracefully when the host shuts down.

- Hosted services integrate with ASP.NET Core's `IHost` so background loops, queue consumers, and startup initialization participate in coordinated startup and shutdown rather than running as untracked threads.
- They are registered as singletons via `AddHostedService<T>()` and implement `IHostedService` or inherit `BackgroundService`.
- Examples include cache warmers, periodic sync jobs, message publishers, and in-process queue consumers.
- The same hosting model applies to worker services (`Worker` template) and web apps — background work can live alongside Kestrel in one process.

---

#### Q2. What is `IHostedService`?

**Answer:** `IHostedService` is the interface defining `StartAsync(CancellationToken)` and `StopAsync(CancellationToken)` hooks that the host calls to begin and end background components during application startup and shutdown.

- `StartAsync` should complete quickly — the host awaits all hosted services' `StartAsync` before marking startup finished and accepting traffic.
- `StopAsync` runs during shutdown and should release resources, flush buffers, and signal long-running work to exit.
- Any class can implement `IHostedService` directly for short-lived startup/shutdown tasks without a long-running loop.
- The host manages ordering: all services start, then the web server listens; on shutdown, `StopAsync` is called with a linked cancellation token.

---

#### Q3. What is `BackgroundService`, and how does it differ from `IHostedService`?

**Answer:** `BackgroundService` is an abstract base class implementing `IHostedService` that schedules a long-running `ExecuteAsync(CancellationToken)` loop on a background thread while returning promptly from `StartAsync`, which is the pattern most queue consumers and polling workers need.

- Raw `IHostedService` requires you to manage your own background thread or timer inside `StartAsync`/`StopAsync`.
- `BackgroundService.StartAsync` queues `ExecuteAsync` and returns immediately, so Kestrel can start listening while the loop runs.
- Override `StopAsync` to cancel the token and optionally wait for `ExecuteAsync` to observe shutdown before the process exits.
- Choose raw `IHostedService` for quick initialization hooks; choose `BackgroundService` for continuous processing until cancellation.

---

#### Q4. What is the difference between `StartAsync` and `ExecuteAsync`?

**Answer:** `StartAsync` is the host's startup hook that must finish quickly so the application can become ready, while `ExecuteAsync` (on `BackgroundService`) is where long-running loops, queue consumption, and periodic work belong.

- Blocking `StartAsync` with synchronous I/O or `.Wait()` on long tasks delays port binding and causes health/readiness probe failures in orchestrators.
- `ExecuteAsync` receives a `stoppingToken` linked to host shutdown — exit loops when cancellation is requested or when catching `OperationCanceledException` during shutdown.
- One-time initialization that takes seconds may run in `StartAsync` if bounded with a timeout, or defer to `ExecuteAsync` with a readiness gate if partial startup is acceptable.
- `IHostedService` has no `ExecuteAsync` — only `BackgroundService` provides that template method.

---

#### Q5. How do you register a hosted service in DI?

**Answer:** Call `builder.Services.AddHostedService<T>()` (or the overload accepting a factory) during service configuration, which registers the implementation as a singleton implementing both `T` and `IHostedService`.

```csharp
builder.Services.AddHostedService<EmailDispatchWorker>();
// or with factory:
builder.Services.AddHostedService(sp => new OutboxPublisher(sp.GetRequiredService<IServiceScopeFactory>()));
```

- The host resolves all `IHostedService` registrations and invokes their lifecycle methods automatically — no manual `new Thread()` is required.
- Hosted services are singletons; do not inject scoped services directly into the constructor (see Q6).
- Multiple hosted services can coexist (outbox publisher, metrics reporter, cache refresher) — each runs independently with shared shutdown semantics.
- Register supporting singletons (channels, queues) separately and inject them into the hosted service constructor.

---

#### Q6. Why can't you inject a Scoped service directly into a Singleton hosted service?

**Answer:** Hosted services register as singletons and live for the entire application lifetime, while scoped services such as `DbContext` are created and disposed per request or per scope — injecting scoped into singleton creates a captive dependency that outlives its scope and causes `ObjectDisposedException` or stale state.

- The DI container may allow the registration at build time unless `ValidateScopes` is enabled, but the bug surfaces at runtime when the scoped instance is disposed after the first scope ends.
- EF Core `DbContext` is scoped because it tracks changes for one unit of work — reusing one instance across background jobs corrupts the change tracker and connection pooling.
- ASP.NET Core 8 can validate scope violations at startup when `ValidateScopes` and `ValidateOnBuild` are enabled in Development or staging.
- The fix is never to change `DbContext` to singleton — always open a new scope per work item (see Q7).

---

#### Q7. What is `IServiceScopeFactory`, and how is it used in background work?

**Answer:** `IServiceScopeFactory` creates new dependency injection scopes on demand, allowing singleton hosted services to resolve scoped services safely by opening a scope per background job and disposing it when the job completes.

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.SaveChangesAsync(stoppingToken);
```

- Inject `IServiceScopeFactory` (singleton-safe) into the hosted service constructor, not the scoped service itself.
- Create one scope per queue message, file, or batch unit — not one scope for the entire application lifetime.
- `CreateAsyncScope()` supports async disposal patterns and is preferred over `CreateScope()` in async workers.
- `IDbContextFactory<TContext>` is an alternative when the primary need is creating short-lived DbContext instances without a full scope.

---

#### Q8. What is `Channel<T>`, and how is it used for in-process queuing?

**Answer:** `System.Threading.Channels.Channel<T>` is a thread-safe producer-consumer queue built into .NET that decouples API request threads from background processing by letting endpoints write work items and hosted services read them asynchronously.

- API handlers write to `ChannelWriter<T>` and return `202 Accepted` immediately; a `BackgroundService` reads from `ChannelReader<T>` with `ReadAllAsync(stoppingToken)`.
- `Channel.CreateBounded<T>(capacity)` applies backpressure when the queue fills — producers await space instead of growing memory without limit.
- Register the channel or a wrapper interface as a singleton shared between the endpoint and the consumer hosted service.
- Channels are in-process and not durable — restart loses unbounded in-memory items unless you persist job state in a database first.

---

#### Q9. How does graceful shutdown work for hosted services?

**Answer:** When the host receives a shutdown signal, it cancels a linked `CancellationToken` passed to hosted services, calls `StopAsync` on each `IHostedService`, and waits up to `HostOptions.ShutdownTimeout` for background work to finish before the process exits.

- `BackgroundService` links cancellation to `ExecuteAsync` — loops should check `stoppingToken.IsCancellationRequested` and pass the token to I/O calls.
- Configure `builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(25))` to align with platform grace periods (for example Kubernetes `terminationGracePeriodSeconds`).
- `StopAsync` should stop accepting new work, drain in-flight items within the timeout, or persist incomplete jobs back to a durable store.
- Swallowing `OperationCanceledException` during shutdown is correct when the service is exiting cleanly; do not treat shutdown cancellation as an error.

---

#### Q10. What is the role of `CancellationToken` in `BackgroundService`?

**Answer:** The `stoppingToken` passed to `ExecuteAsync` is cancelled when the host begins shutdown, giving the background loop a signal to exit gracefully instead of being killed mid-operation.

- Pass the same token to `Task.Delay`, `PeriodicTimer.WaitForNextTickAsync`, database calls, and HTTP requests so blocked operations wake promptly on shutdown.
- When the token fires, finish the current work unit if possible, then exit the loop — do not start new long operations after cancellation.
- `HttpContext.RequestAborted` is the per-request equivalent; `stoppingToken` is the application-lifetime equivalent for background work.
- Ignoring the token causes Kubernetes SIGKILL after the grace period, often mid-transaction, leading to duplicate charges or corrupted state on restart.

---

#### Q11. What happens when Kubernetes sends SIGTERM to a pod?

**Answer:** Kubernetes marks the pod for termination, removes it from service endpoints, sends SIGTERM to the container process, waits for the configured grace period (default 30 seconds), then sends SIGKILL if the process is still running.

- ASP.NET Core's host translates shutdown into cancellation of hosted service tokens and `StopAsync` calls — workers must observe this to drain gracefully.
- Readiness probes fail immediately so the load balancer stops sending new traffic, but in-flight HTTP requests and background jobs may still be running.
- Set `HostOptions.ShutdownTimeout` slightly below `terminationGracePeriodSeconds` so the app exits cleanly before SIGKILL.
- Long-running jobs should checkpoint progress in durable storage so SIGKILL mid-job can resume idempotently on restart.

---

#### Q12. What is `PeriodicTimer`, and when would you use it in a hosted service?

**Answer:** `PeriodicTimer` (.NET 6+) is an async-friendly timer that exposes `WaitForNextTickAsync(CancellationToken)`, which is ideal for polling intervals in `BackgroundService` because it integrates cleanly with shutdown cancellation unlike `Task.Delay` loops alone.

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
while (await timer.WaitForNextTickAsync(stoppingToken))
{
    await ProcessOverdueInvoicesAsync(stoppingToken);
}
```

- Prefer it over `while(true) { await Task.Delay(...); }` because `WaitForNextTickAsync` respects timer drift and cancellation in one call.
- Use for reconciliation sweeps, cache refreshes, and health checks that poll on a fixed interval.
- Combine with idempotent database updates so overlapping ticks or restarts do not double-process records.
- For event-driven workloads with low latency requirements, `Channel<T>` or external message brokers reduce unnecessary polling load.

---

#### Q13. What is the difference between polling and event-driven background processing?

**Answer:** Polling repeatedly queries a data store or timer on a fixed interval to find work, while event-driven processing reacts to messages or API events as they occur, typically through channels or external brokers.

- Polling is simpler to implement and self-healing (missed events are picked up on the next tick) but generates steady database load even when idle.
- Event-driven processing has lower latency and less idle load but requires reliable enqueue paths and reconciliation for missed events.
- Polling suits small fleets and low-frequency tasks; event-driven suits high-throughput notifications and billing triggers.
- Many production systems use a hybrid: channels or webhooks for hot paths plus a nightly reconciliation timer for drift detection.

---

#### Q14. When should background work stay in-process vs move to an external queue/broker?

**Answer:** Keep work in-process when volume is low, durability requirements are modest, and a single instance handles the load; move to an external queue (RabbitMQ, Azure Service Bus, Hangfire with shared storage) when you need cross-instance distribution, durable retries, or isolation from the web process.

- In-process `Channel<T>` plus a hosted service is fine for fire-and-forget emails or report generation on a single node with persisted job rows.
- Multi-instance deployments need a shared queue or outbox table so any instance can pick up work — in-memory channels are invisible to other pods.
- CPU-heavy or long-running jobs should not share the web app's thread pool — offload to worker services or dedicated consumers.
- External brokers add operational complexity but provide poison-message handling, dead-letter queues, and at-least-once delivery guarantees.

---

#### Q15. What is the outbox pattern?

**Answer:** The outbox pattern writes outbound messages or integration events to a database table in the same transaction as the business state change, then a separate publisher process reads unpublished rows and sends them to a broker, guaranteeing consistency between the database and downstream systems.

- Business code updates entities and inserts an outbox row atomically — no "database committed but message lost" race.
- A hosted service polls or leases outbox rows, publishes to the message broker, and marks rows published only after confirmed delivery (or uses a state machine: Pending → Processing → Published).
- Multi-instance publishers require row locking (`FOR UPDATE SKIP LOCKED`, `UPDLOCK`) so two workers do not publish the same message.
- Consumers must be idempotent because at-least-once delivery can produce duplicates after crashes between publish and mark-published steps.

---

#### Q16. What problems arise from unbounded parallelism in a background worker?

**Answer:** Unbounded parallelism — for example `Task.WhenAll` over thousands of files or messages without a concurrency cap — exhausts thread pool threads, database connection pools, and memory, and increases duplicate processing and partial-failure corruption under load.

- Each parallel task may open a scoped `DbContext` or HTTP connection simultaneously, hitting pool limits and causing timeouts across the entire application.
- Without per-item error isolation, one failure's semantics become unclear when mixed with concurrent successes.
- Limit concurrency with `SemaphoreSlim`, `Parallel.ForEachAsync` with `MaxDegreeOfParallelism`, or bounded channel readers.
- Combine bounded parallelism with idempotency keys so retries after partial failure do not double-charge or duplicate side effects.

---

#### Q17. How does a hosted service relate to the ASP.NET Core application lifetime?

**Answer:** Hosted services start after the host builds the service provider and complete their `StartAsync` before the application is considered started, and they receive shutdown signals through the same `IHost` lifetime that stops Kestrel and disposes the root service provider.

- In a web app, Kestrel and hosted services share one process — background workers run concurrently with HTTP request handling.
- `IHostApplicationLifetime` exposes `ApplicationStarted`, `ApplicationStopping`, and `ApplicationStopped` for registering callbacks around hosted service work.
- When the root provider disposes at shutdown, singleton dependencies are disposed — another reason scoped services must not be captured for the app's entire lifetime.
- Worker Service templates use the same `IHost` without Kestrel, demonstrating that hosted services are not web-specific.

---

#### Q18. What is the difference between `IHostedService` and a `Task.Run` fire-and-forget call?

**Answer:** `IHostedService` is registered with the host, participates in coordinated startup and shutdown, and receives cancellation when the application stops, whereas `Task.Run` fire-and-forget work is untracked, may outlive intended scope, and is aborted abruptly on process exit without cleanup.

- Fire-and-forget tasks started from a controller or middleware are not awaited by the host — exceptions may go unobserved and shutdown will not wait for them.
- Hosted services run under the host's exception logging and lifecycle management — `StopAsync` provides a hook to flush state.
- `Task.Run` inside a hosted service for parallel work is acceptable when bounded and awaited within `ExecuteAsync`, but not as a substitute for registering the worker itself.
- For background work in ASP.NET Core 8, always prefer `AddHostedService<T>()` over `_ = Task.Run(...)` from request handlers.

---

## Chapter 15. WebSockets & Real-Time Transport

#### Q1. What are WebSockets, and how do they differ from regular HTTP requests?

**Answer:** WebSockets provide a full-duplex, persistent connection between client and server after an initial HTTP upgrade handshake, allowing both sides to send messages at any time without the request-response overhead of standard HTTP.

- Regular HTTP is stateless and typically one request yields one response, then the connection may close; WebSockets keep the TCP connection open for bidirectional framed messages.
- The upgrade begins as HTTP GET with `Connection: Upgrade` and `Upgrade: websocket` headers — once accepted, the protocol switches from HTTP to the WebSocket framing protocol.
- WebSockets suit live dashboards, chat, gaming, and tick feeds where server push latency matters.
- They consume server resources for the connection duration — unlike short HTTP requests that release resources immediately after the response.

---

#### Q2. How do you enable WebSockets in ASP.NET Core?

**Answer:** Call `app.UseWebSockets()` (optionally with `WebSocketOptions`) in the middleware pipeline and handle upgrade requests in an endpoint that checks `context.WebSockets.IsWebSocketRequest` before calling `AcceptWebSocketAsync`.

```csharp
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    // receive/send loop
});
```

- `UseWebSockets` adds middleware that detects upgrade requests and enables the WebSocket subsystem in Kestrel.
- Without this middleware, upgrade attempts fail or behave as normal HTTP requests.
- SignalR enables WebSockets internally when you call `AddSignalR()` and map hubs — raw WebSocket use requires explicit middleware and handler code.

---

#### Q3. Where must `UseWebSockets()` be placed in the middleware pipeline?

**Answer:** Place `UseWebSockets()` early in the pipeline — after exception handling and forwarded headers, but before the branch that handles the upgrade and before terminal middleware that would short-circuit the request.

- Authentication and authorization for the upgrade request must run before `AcceptWebSocketAsync` because WebSocket messages after upgrade no longer pass through standard HTTP middleware on each frame.
- If placed after a terminal middleware or missing entirely, upgrade requests return 404 or fail to switch protocols.
- Behind a reverse proxy, ensure the proxy forwards `Upgrade` and `Connection` headers and that path-base configuration matches the mapped WebSocket route.
- SignalR's `MapHub` still requires `UseWebSockets()` (or equivalent) in the pipeline for the WebSocket transport.

---

#### Q4. What happens during a WebSocket upgrade request?

**Answer:** The client sends an HTTP GET with `Connection: Upgrade`, `Upgrade: websocket`, `Sec-WebSocket-Key`, and related headers; the server validates the request, responds with `101 Switching Protocols` and a computed `Sec-WebSocket-Accept` value, and the connection becomes a WebSocket with framed bidirectional messaging.

- Until the server accepts, the request is normal HTTP — cookies, JWT bearer tokens, and authorization policies apply at this stage.
- After `AcceptWebSocketAsync`, further communication uses WebSocket frames (`ReceiveAsync`/`SendAsync`), not HTTP request/response pairs.
- Failed upgrades return HTTP error status codes (400, 401, 404) before any protocol switch occurs.
- Load balancers must support connection upgrade and often require sticky sessions or shared backplane for subsequent message routing in multi-instance setups.

---

#### Q5. What server resources are consumed by an open WebSocket connection?

**Answer:** Each open WebSocket holds a TCP connection, a `WebSocket` object, send/receive buffers, and any application-level registry state (connection dictionaries, group memberships) until the client closes or the server terminates the connection.

- Thousands of idle tabs multiply memory for buffers and tracking structures — unbounded static dictionaries of connections are a common memory leak.
- Thread pool continuations from `ReceiveAsync`/`SendAsync` add CPU overhead under high message rates.
- Slow clients that cannot read fast enough accumulate outbound queues unless the server applies backpressure or drops them.
- Monitor active connection count, bytes in/out, and process memory — alert when connections grow without matching active user sessions.

---

#### Q6. What is SignalR?

**Answer:** SignalR is an ASP.NET Core library that provides a high-level real-time messaging abstraction over WebSockets, Server-Sent Events, and long polling, with hubs, connection IDs, groups, and automatic client reconnection support.

- Developers define hub classes with methods callable from clients and server push methods such as `Clients.Group("room").SendAsync(...)`.
- SignalR negotiates the best available transport — WebSockets when supported, falling back to SSE or long polling through firewalls and proxies.
- It integrates with authentication (`[Authorize]` on hubs), dependency injection, and scale-out backplanes (Redis, Azure Service Bus).
- SignalR is the default choice for ASP.NET Core real-time features unless you need a fully custom binary protocol.

---

#### Q7. What is the difference between SignalR and raw WebSockets?

**Answer:** Raw WebSockets give you a low-level framed connection where you define message format, routing, reconnection, and scale-out yourself; SignalR provides hubs, groups, connection management, transport fallback, and built-in scale-out hooks on top of WebSockets or alternate transports.

| Aspect | Raw WebSocket | SignalR |
|---|---|---|
| Protocol | You define JSON/binary framing | Hub methods, JSON/MessagePack |
| Reconnection | Manual backoff and resubscribe | Client SDK auto-reconnect |
| Scale-out | Custom pub/sub + registry | Redis/Azure backplane built-in |
| Fallback transports | WebSocket only | WebSocket, SSE, long polling |
| Auth | Manual at upgrade time | `[Authorize]`, JWT on negotiate |

- Raw WebSockets fit custom protocols, non-.NET clients with strict wire formats, or minimal overhead binary streams.
- SignalR fits typical notify/broadcast scenarios (order status, chat, live dashboards) with faster team delivery.

---

#### Q8. When would you choose SignalR over raw WebSockets?

**Answer:** Choose SignalR when you need group broadcast, automatic transport fallback, client reconnection, and multi-instance scale-out without building that infrastructure yourself — typical business real-time notifications are the sweet spot.

- Order status updates to browser and mobile clients mapped to user groups (`Clients.User(id)`) are simpler with SignalR than maintaining per-connection dictionaries manually.
- Environments where WebSockets are blocked by proxies benefit from SignalR's SSE/long-polling fallback without separate client code paths.
- Teams without dedicated real-time protocol expertise ship faster with hub-based APIs and official JavaScript/.NET clients.
- Choose raw WebSockets when SignalR's overhead, negotiate handshake, or opinionated hub model does not fit (embedded devices, third-party binary protocols, extreme latency tuning).

---

#### Q9. What is a SignalR backplane, and why is it needed?

**Answer:** A SignalR backplane is a shared pub/sub message bus (Redis, Azure Service Bus, etc.) that synchronizes hub messages across all server instances so a broadcast from one node reaches clients connected to other nodes.

- Without a backplane, each instance only knows about its local connections — `Clients.All.SendAsync` on instance A does not reach sockets on instance B.
- Register with `AddSignalR().AddStackExchangeRedis(connectionString, options => ...)` or the Azure SignalR Service integration.
- Sticky sessions alone keep one client on one node but do not solve cross-instance fan-out when events originate on arbitrary nodes.
- Azure SignalR Service is a managed alternative that offloads connection management and scaling entirely from your web servers.

---

#### Q10. How do you scale WebSocket/SignalR applications across multiple server instances?

**Answer:** Combine a SignalR backplane or Azure SignalR Service for message fan-out, enforce authentication at connection time, configure proxy WebSocket timeouts, and optionally use sticky sessions for connection affinity while relying on the backplane for cross-node broadcasts.

- Redis backplane: all instances subscribe to a channel prefix; hub messages publish once and every node delivers to its local connections.
- Raw WebSocket scale-out requires a custom connection registry in Redis and pub/sub routing — each instance subscribes and forwards to local sockets.
- Configure nginx/ALB idle timeouts longer than your heartbeat interval to prevent proxy-side disconnects.
- Load-test connection count and broadcast fan-out separately — 2,000 idle connections behave differently from broadcasting to 2,000 clients every second.

---

#### Q11. How is authentication handled for WebSocket connections?

**Answer:** Authentication occurs during the HTTP upgrade request before the protocol switches — validate JWT bearer tokens, cookies, or API keys in middleware or endpoint authorization, because individual WebSocket frames do not re-run the full HTTP auth pipeline.

- For JWT, browsers often pass the token as a query parameter or `Authorization` header on the upgrade GET because WebSocket API header support varies.
- SignalR's negotiate endpoint accepts `[Authorize]` and standard authentication handlers before establishing the transport.
- After upgrade, derive user identity from `context.User` claims established at upgrade — never trust a client-sent user ID in the first WebSocket message.
- Anonymous upgrades should be rejected explicitly (`401`) before `AcceptWebSocketAsync` for protected resources.

---

#### Q12. What are WebSocket message size limits in ASP.NET Core/Kestrel?

**Answer:** Kestrel limits the initial HTTP upgrade request body via `MaxRequestBodySize`, but per-message limits for WebSocket frames require application-level enforcement because `ReceiveAsync` returns one frame chunk at a time and large logical messages span multiple frames.

- Default receive buffers are often 4 KB per call — reassemble with a `MemoryStream` until `EndOfMessage` is true, counting total bytes against a cap.
- SignalR exposes `MaximumReceiveMessageSize` in hub options; raw handlers must implement equivalent guards and close with status `1009` (Message Too Big) when exceeded.
- Without a reassembly cap, a malicious client sending infinite partial frames causes out-of-memory failures.
- Prefer HTTP upload endpoints for large blobs; use WebSockets for notifications and small control messages.

---

#### Q13. What is WebSocket backpressure, and why does it matter for broadcasts?

**Answer:** Backpressure occurs when a producer sends messages faster than a slow consumer can read them, causing outbound queues to grow — in naive broadcast loops that `await SendAsync` sequentially to every client, one slow peer blocks delivery to all others (head-of-line blocking).

- Serialize the payload once and fan out with bounded parallelism or per-client outbound queues instead of awaiting every send inside one client's receive loop.
- Remove dead sockets from registries when `WebSocketState` is not `Open` or when sends throw — stale entries amplify blocking.
- SignalR handles much of this internally; raw WebSocket broadcast code needs explicit queue caps and drop policies for slow clients.
- Decouple inbound messages (publish to a channel/bus) from outbound fan-out so one client's read loop does not drive global broadcast timing.

---

#### Q14. How do you detect and clean up stale WebSocket connections?

**Answer:** Use protocol-level keep-alives plus application heartbeats, enforce idle timeouts, and always remove connections from registries in a `finally` block when the receive loop exits or the token is cancelled.

- Set `WebSocketOptions.KeepAliveInterval` so Kestrel sends control frames — this helps but may not traverse all proxies without application-level pings.
- Send periodic heartbeat messages and close the connection if no response arrives within the configured interval.
- Link `CancellationToken` to `HttpContext.RequestAborted` and host shutdown so deploys do not leave ghost entries in static dictionaries.
- Cap connections per authenticated user at accept time to prevent one account from opening unbounded tabs and exhausting server memory.

---

#### Q15. What is the difference between WebSocket and Server-Sent Events (SSE)?

**Answer:** WebSockets are bidirectional — either side can send at any time — while Server-Sent Events (SSE) are a one-way HTTP-based stream from server to client over a long-lived `text/event-stream` response.

- SSE works over standard HTTP/1.1 or HTTP/2 without an upgrade handshake, traversing some proxies and firewalls more easily than WebSockets.
- SSE is suitable for live feeds, progress updates, and notifications that only need server push; client-to-server updates still use regular HTTP requests.
- WebSockets fit chat, collaborative editing, and gaming where low-latency client messages are frequent.
- SignalR can fall back to SSE automatically when WebSockets are unavailable, hiding transport details from application code.

---

#### Q16. What is long polling, and how does it compare to WebSockets?

**Answer:** Long polling is a technique where the client sends repeated HTTP requests and the server holds each request open until new data arrives or a timeout occurs, then the client immediately opens another request — it simulates push over plain HTTP.

- It has higher latency and overhead than WebSockets because each message cycle may require new HTTP headers and connection setup.
- It works everywhere HTTP works, including restrictive proxies — SignalR uses it as a last-resort fallback transport.
- WebSockets maintain one persistent connection with lower per-message overhead after the upgrade.
- Long polling consumes server threads or async waits per waiting client — at scale it is less efficient than WebSockets or SSE for continuous streams.

---

#### Q17. What is a SignalR Hub?

**Answer:** A SignalR Hub is a server-side class that defines methods clients can invoke and provides `Clients`, `Groups`, and `Context` properties for pushing messages to connected clients, groups, or specific connection IDs.

```csharp
public class OrderHub : Hub
{
    public async Task JoinOrderGroup(string orderId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");

    public async Task OrderStatusChanged(string orderId, string status) =>
        await Clients.Group($"order-{orderId}").SendAsync("StatusUpdated", status);
}
```

- Map with `app.MapHub<OrderHub>("/hubs/orders");` — clients connect via the SignalR JavaScript or .NET client SDK.
- Hub methods run in the context of a connected client; server-side code injects services via constructor DI.
- `[Authorize]` on the hub class or methods restricts who can connect and invoke operations.
- Hubs abstract connection lifetime — the framework tracks connection IDs and group membership across reconnections when designed with durable user identifiers.

---

#### Q18. What security risks exist when clients self-identify via the first WebSocket message?

**Answer:** If the server accepts a client-supplied user ID or tenant ID in the first WebSocket frame instead of binding identity from authenticated claims on the upgrade request, any anonymous connection can impersonate another user and subscribe to their private channels.

- WebSocket auth must be established at HTTP upgrade time — trusting post-upgrade JSON payloads is equivalent to skipping authentication on REST endpoints.
- Attackers connect to `/ws/orders`, send `{ "userId": "victim-guid" }`, and receive events intended for the victim.
- Derive identity from `context.User.FindFirst(ClaimTypes.NameIdentifier)` after validating JWT or cookies during upgrade.
- Combine authenticated upgrades with authorization checks on group subscription — users should only join groups matching their claims.

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
