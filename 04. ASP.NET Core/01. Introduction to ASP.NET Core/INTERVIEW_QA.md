# Introduction to ASP.NET Core — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 01. Introduction to ASP.NET Core](#chapter-01-introduction-to-aspnet-core)
  - [Q1. What is ASP.NET Core?](#chapter-01-introduction-to-aspnet-core-q1)
  - [Q2. How does ASP.NET Core differ from ASP.NET Framework?](#chapter-01-introduction-to-aspnet-core-q2)
  - [Q3. What is Kestrel, and what role does it play in ASP.NET Core?](#chapter-01-introduction-to-aspnet-core-q3)
  - [Q4. Why do production deployments often place nginx or IIS in fr…](#chapter-01-introduction-to-aspnet-core-q4)
  - [Q5. Who terminates TLS in a typical reverse-proxy deployment?](#chapter-01-introduction-to-aspnet-core-q5)
  - [Q6. What is the unified hosting model introduced by `WebApplicat…](#chapter-01-introduction-to-aspnet-core-q6)
  - [Q7. What is the difference between the old `Startup.cs` pattern …](#chapter-01-introduction-to-aspnet-core-q7)
  - [Q8. Walk through the major stages of an HTTP request in ASP.NET …](#chapter-01-introduction-to-aspnet-core-q8)
  - [Q9. When would you choose Minimal APIs over MVC controllers?](#chapter-01-introduction-to-aspnet-core-q9)
  - [Q10. What does `ASPNETCORE_ENVIRONMENT` control?](#chapter-01-introduction-to-aspnet-core-q10)
  - [Q11. What makes ASP.NET Core suitable for Linux containers and cl…](#chapter-01-introduction-to-aspnet-core-q11)
  - [Q12. What is the ASP.NET Core request pipeline?](#chapter-01-introduction-to-aspnet-core-q12)
  - [Q13. What is endpoint routing?](#chapter-01-introduction-to-aspnet-core-q13)
  - [Q14. What are the main components registered in `Program.cs`?](#chapter-01-introduction-to-aspnet-core-q14)
  - [Q15. What is cross-platform hosting in the context of ASP.NET Cor…](#chapter-01-introduction-to-aspnet-core-q15)
  - [Q16. How does ASP.NET Core handle dependency injection by default…](#chapter-01-introduction-to-aspnet-core-q16)
  - [Q17. What is the difference between in-process and out-of-process…](#chapter-01-introduction-to-aspnet-core-q17)
  - [Q18. What architectural shifts are required when porting a .NET F…](#chapter-01-introduction-to-aspnet-core-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 01. Introduction to ASP.NET Core

### Q1. What is ASP.NET Core? {#chapter-01-introduction-to-aspnet-core-q1}

What is ASP.NET Core?

**Answer:** ASP.NET Core is the cross-platform web framework for building HTTP services, web APIs, MVC sites, and real-time apps on modern .NET. It runs on Kestrel (or behind IIS/nginx), uses a composable middleware pipeline, and integrates dependency injection, configuration, and logging by default.

- It is the successor to ASP.NET on .NET Framework — redesigned for Linux, containers, and cloud deployment rather than IIS-only hosting.
- Applications are bootstrapped through `WebApplication.CreateBuilder` and configured in `Program.cs` with explicit middleware and endpoint registration.
- It supports both controller-based MVC/Razor and Minimal APIs in the same host, sharing routing, auth, and serialization infrastructure.
- ASP.NET Core 8 continues the unified .NET release line with LTS support, performance improvements, and first-class Minimal API and OpenAPI features.

---

### Q2. How does ASP.NET Core differ from ASP.NET Framework? {#chapter-01-introduction-to-aspnet-core-q2}

How does ASP.NET Core differ from ASP.NET Framework?

**Answer:** ASP.NET Core is cross-platform, modular, and side-by-side deployable, while ASP.NET Framework is Windows-only, monolithic, and tightly coupled to IIS and `System.Web`. Core replaces the old pipeline with composable middleware, built-in DI, and Kestrel as the default server.

- Framework apps depend on machine-wide GAC installs and `web.config`; Core apps ship self-contained or framework-dependent with `appsettings.json` and environment variables.
- `System.Web` handled request lifecycle implicitly; Core makes every stage explicit — routing, auth, endpoints, and static files are registered in code.
- Framework Web Forms, WCF in-process, and classic MVC 5 patterns do not port directly; Core uses middleware, endpoint routing, and modern authentication handlers.
- Microsoft maintains Framework 4.8 in maintenance mode; new web development targets ASP.NET Core on .NET 8.

---

### Q3. What is Kestrel, and what role does it play in ASP.NET Core? {#chapter-01-introduction-to-aspnet-core-q3}

What is Kestrel, and what role does it play in ASP.NET Core?

**Answer:** Kestrel is the cross-platform web server built into ASP.NET Core that listens for HTTP/HTTPS connections and forwards requests into the middleware pipeline. It is the default in-process server for all platforms and is optimized for throughput and low allocation.

- Kestrel terminates TCP connections, parses HTTP, and invokes the `RequestDelegate` chain starting at the first registered middleware.
- On Windows it can run standalone or as the backend behind IIS through the ASP.NET Core Module (ANCM).
- Kestrel supports HTTP/1.x, HTTP/2, HTTP/3 (where configured), WebSockets, and configurable limits on body size, headers, and concurrent connections.
- In production, Kestrel often listens on an internal port while a reverse proxy handles TLS and edge concerns at the network boundary.

---

### Q4. Why do production deployments often place nginx or IIS in front of Kestrel? {#chapter-01-introduction-to-aspnet-core-q4}

Why do production deployments often place nginx or IIS in front of Kestrel?

**Answer:** A reverse proxy in front of Kestrel handles TLS termination, load balancing, rate limiting, static-file caching, and request filtering at the network edge. Kestrel is optimized as an application server, not as the sole public-facing internet gateway.

- The proxy presents a single certificate and hostname to clients while Kestrel runs on loopback or an internal network with plain HTTP or mTLS.
- Edge servers can serve cached static assets, gzip/brotli responses, and shield the app from slowloris or oversized payloads before they reach .NET.
- Multiple Kestrel instances behind nginx or IIS Application Request Routing distribute load and enable zero-downtime deployments.
- `UseForwardedHeaders` must be configured so Kestrel sees the original client IP and scheme (`https`) from proxy headers.

---

### Q5. Who terminates TLS in a typical reverse-proxy deployment? {#chapter-01-introduction-to-aspnet-core-q5}

Who terminates TLS in a typical reverse-proxy deployment?

**Answer:** In a typical nginx-or-IIS-in-front-of-Kestrel setup, the reverse proxy terminates TLS — it holds the public certificate and decrypts HTTPS before forwarding to Kestrel over HTTP (often on localhost). Kestrel receives already-decrypted HTTP unless you configure end-to-end TLS separately.

- Clients negotiate TLS with nginx or IIS; Kestrel sees `X-Forwarded-Proto: https` when forwarded headers are configured correctly.
- Kestrel can terminate TLS directly when exposed without a proxy, using `ListenOptions.UseHttps` or certificate configuration in `appsettings.json`.
- Some deployments use TLS at both layers (TLS to proxy, mTLS or TLS to Kestrel) for defense in depth inside a private network.
- Misconfigured forwarded headers cause redirects and cookie `Secure` flags to assume `http` even though clients used HTTPS.

---

### Q6. What is the unified hosting model introduced by `WebApplication.CreateBuilder`? {#chapter-01-introduction-to-aspnet-core-q6}

What is the unified hosting model introduced by `WebApplication.CreateBuilder`?

**Answer:** `WebApplication.CreateBuilder` merges the generic host and web host into a single bootstrap API that configures logging, configuration, DI, Kestrel, and middleware in one `Program.cs` flow. It replaces the separate `Host.CreateDefaultBuilder` + `ConfigureWebHostDefaults` split from early ASP.NET Core versions.

- The builder returns a `WebApplicationBuilder` with `Services`, `Configuration`, `Environment`, and `Logging` pre-wired with the same defaults Visual Studio templates use.
- Calling `builder.Build()` produces a `WebApplication` that is both the host and the pipeline configurator — no separate `IWebHost` and `IHost` coordination.
- Minimal hosting supports top-level statements, reducing ceremony while keeping full access to MVC, Razor, SignalR, and background services.
- The unified model is the standard entry point in ASP.NET Core 6+ and ASP.NET Core 8 project templates.

---

### Q7. What is the difference between the old `Startup.cs` pattern and the modern minimal hosting model? {#chapter-01-introduction-to-aspnet-core-q7}

What is the difference between the old `Startup.cs` pattern and the modern minimal hosting model?

**Answer:** The `Startup.cs` pattern split configuration into `ConfigureServices` (DI registration) and `Configure` (middleware pipeline) classes referenced from `Program.cs`. The minimal hosting model colocates builder setup, service registration, middleware, and endpoint mapping in a single top-level `Program.cs` without a separate `Startup` class.

- Functionally equivalent: `builder.Services.AddControllers()` replaces `ConfigureServices`, and `app.UseRouting(); app.MapControllers()` replaces pipeline setup in `Configure`.
- `Startup.cs` remains supported for teams preferring separation; you can still call `builder.Services.AddControllers()` and use a `Startup` class via `builder.Host.ConfigureWebHostDefaults`.
- Minimal hosting reduces boilerplate and aligns with C# top-level statements introduced in modern templates.
- Both patterns compile to the same underlying `WebApplication` host in ASP.NET Core 8.

---

### Q8. Walk through the major stages of an HTTP request in ASP.NET Core (high level). {#chapter-01-introduction-to-aspnet-core-q8}

Walk through the major stages of an HTTP request in ASP.NET Core (high level).

**Answer:** An HTTP request enters Kestrel, passes through the middleware pipeline in registration order, matches an endpoint via routing, runs auth/authz, executes the endpoint (controller action or Minimal API delegate), and returns a response back through middleware in reverse order.

- Kestrel parses the request and creates an `HttpContext` with `Request`, `Response`, and `RequestServices` (request-scoped DI).
- Early middleware may handle exceptions, HTTPS redirection, static files, or forwarded headers before routing runs.
- Endpoint routing selects a handler; authorization middleware evaluates policies against the matched endpoint metadata.
- The endpoint produces an `IActionResult`, `IResult`, or raw response; result filters and response compression middleware may modify output on the way out.

---

### Q9. When would you choose Minimal APIs over MVC controllers? {#chapter-01-introduction-to-aspnet-core-q9}

When would you choose Minimal APIs over MVC controllers?

**Answer:** Minimal APIs suit small to medium HTTP services, microservices, and prototypes where a single file or few files define routes without needing views, complex filter pipelines, or convention-based folder structure. Controllers remain better for large APIs with many actions, Razor integration, and mature MVC conventions.

- Minimal APIs reduce ceremony: `app.MapGet("/api/items", () => ...)` with parameter binding and DI without controller classes.
- Choose controllers when you need `[ApiController]` conventions at scale, area routing, view results, or extensive use of action/resource/result filters.
- Both share the same host, authentication, OpenAPI, and validation infrastructure in ASP.NET Core 8.
- Minimal APIs support `MapGroup`, endpoint filters, and typed results (`TypedResults`) for production-grade APIs.

---

### Q10. What does `ASPNETCORE_ENVIRONMENT` control? {#chapter-01-introduction-to-aspnet-core-q10}

What does `ASPNETCORE_ENVIRONMENT` control?

**Answer:** `ASPNETCORE_ENVIRONMENT` sets the hosting environment name (commonly `Development`, `Staging`, or `Production`) that drives configuration loading, logging verbosity, and conditional middleware such as the developer exception page. It is read at startup into `IWebHostEnvironment.EnvironmentName`.

- `appsettings.{Environment}.json` overlays base `appsettings.json` when the environment name matches.
- `builder.Environment.IsDevelopment()` gates developer-only features like `UseDeveloperExceptionPage` and EF Core sensitive data logging.
- Production hosts set this via environment variables or launch configuration — it should never default to Development in deployed environments.
- Missetting Production to Development exposes stack traces and detailed errors to clients.

---

### Q11. What makes ASP.NET Core suitable for Linux containers and cloud deployment? {#chapter-01-introduction-to-aspnet-core-q11}

What makes ASP.NET Core suitable for Linux containers and cloud deployment?

**Answer:** ASP.NET Core runs natively on Linux and macOS, ships as self-contained or framework-dependent deployments, and starts quickly with a small footprint suitable for containers. Kestrel, configurable via environment variables, fits twelve-factor app patterns used on Kubernetes, Azure App Service, and AWS.

- Docker images based on `mcr.microsoft.com/dotnet/aspnet:8.0` provide a minimal runtime layer; SDK images are used only at build time.
- Configuration through environment variables and secrets mounts avoids baking settings into images.
- Health check endpoints (`MapHealthChecks`) integrate with Kubernetes liveness and readiness probes.
- Side-by-side runtime versions allow independent container image updates without machine-wide installs.

---

### Q12. What is the ASP.NET Core request pipeline? {#chapter-01-introduction-to-aspnet-core-q12}

What is the ASP.NET Core request pipeline?

**Answer:** The request pipeline is an ordered chain of middleware components, each implementing a `RequestDelegate` that can run logic before and after calling the next delegate. Every HTTP request traverses this chain twice in concept — inward on the way to the endpoint and outward when the response returns.

- Middleware is registered with `app.Use...`, `app.Run`, or `app.Map` in `Program.cs`; order determines behavior.
- The pipeline is built once at startup when `app.Run()` is called (after all `Use` registrations).
- Terminal middleware or endpoint execution produces the response; earlier middleware can modify headers or body on the return path.
- Unlike `System.Web` modules, the pipeline is fully explicit and testable with `WebApplicationFactory` in integration tests.

---

### Q13. What is endpoint routing? {#chapter-01-introduction-to-aspnet-core-q13}

What is endpoint routing?

**Answer:** Endpoint routing matches incoming requests to a specific endpoint — a controller action, Minimal API delegate, Razor page, or hub — using route templates and HTTP methods registered at startup. It decouples route matching from endpoint execution and enables endpoint-aware middleware such as authorization.

- `UseRouting` marks where route matching occurs; `UseEndpoints` or `Map*` methods register endpoints in ASP.NET Core 3+ unified routing.
- In minimal hosting, `MapControllers()`, `MapGet()`, and similar calls register endpoints directly on `WebApplication`.
- Matched endpoints carry metadata (`IAuthorizeData`, `ProducesResponseType`, etc.) consumed by middleware and OpenAPI generators.
- Endpoint routing replaced legacy IRouter-based middleware that separated routing from the rest of the pipeline awkwardly.

---

### Q14. What are the main components registered in `Program.cs`? {#chapter-01-introduction-to-aspnet-core-q14}

What are the main components registered in `Program.cs`?

**Answer:** `Program.cs` registers application services in DI during the builder phase and configures the HTTP pipeline and endpoints on the built `WebApplication`. Typical registrations include controllers, authentication, authorization, EF Core, HttpClient, options, and health checks.

- Builder phase: `builder.Services.AddControllers()`, `AddAuthentication()`, `AddDbContext<>()`, `AddEndpointsApiExplorer()`, `AddSwaggerGen()`.
- Pipeline phase: `UseHttpsRedirection()`, `UseAuthentication()`, `UseAuthorization()`, `MapControllers()`, `MapHealthChecks()`.
- Configuration and logging are configured automatically by `WebApplication.CreateBuilder` but can be extended with `builder.Configuration` and `builder.Logging`.
- Hosted services (`AddHostedService`) and Kestrel limits are also commonly registered here in ASP.NET Core 8 apps.

---

### Q15. What is cross-platform hosting in the context of ASP.NET Core? {#chapter-01-introduction-to-aspnet-core-q15}

What is cross-platform hosting in the context of ASP.NET Core?

**Answer:** Cross-platform hosting means the same ASP.NET Core application code can run on Windows, Linux, and macOS without recompilation, using Kestrel or platform-specific reverse proxies. The runtime and framework are portable; only deployment packaging and path conventions differ.

- Development on Windows with deployment to Linux containers is a standard workflow — no separate codebase per OS.
- Path separators, line endings, and case sensitivity for static files require attention on Linux file systems.
- IIS is Windows-specific; Linux production typically uses Kestrel behind nginx, Apache, or a cloud load balancer.
- `dotnet publish` produces a runnable output for the chosen runtime identifier (RID) such as `linux-x64`.

---

### Q16. How does ASP.NET Core handle dependency injection by default? {#chapter-01-introduction-to-aspnet-core-q16}

How does ASP.NET Core handle dependency injection by default?

**Answer:** ASP.NET Core includes a built-in DI container configured through `builder.Services` with Singleton, Scoped, and Transient lifetimes. Controllers, Minimal API handlers, middleware constructed via factory, and framework services are resolved from this container automatically.

- Constructor injection is the default pattern — the container resolves parameters when activating a type.
- A new DI scope is created per HTTP request; scoped services (e.g., `DbContext`) live for that request only.
- Framework services (`ILogger<T>`, `IConfiguration`, `IHttpContextAccessor`) are pre-registered by `WebApplication.CreateBuilder`.
- Third-party containers (Autofac, etc.) can replace the default provider via `Host.UseServiceProviderFactory` if needed.

---

### Q17. What is the difference between in-process and out-of-process IIS hosting? {#chapter-01-introduction-to-aspnet-core-q17}

What is the difference between in-process and out-of-process IIS hosting?

**Answer:** In-process hosting runs the ASP.NET Core app inside the IIS worker process (`w3wp.exe`) with Kestrel bypassed for HTTP handling through IIS's native pipeline. Out-of-process hosting runs Kestrel in a separate `dotnet.exe` child process while IIS forwards requests via the ASP.NET Core Module.

- In-process is the default on IIS for better performance and simpler debugging on Windows; `AspNetCoreModuleV2` loads the Core CLR inside IIS.
- Out-of-process isolates crashes — if the app process dies, IIS can restart it without recycling the entire worker process in some configurations.
- Both modes use `web.config` to point IIS at the published app and configure the hosting model via `hostingModel="inprocess"` or `outofprocess`.
- Linux deployments do not use IIS; this distinction applies to Windows IIS hosting only.

---

### Q18. What architectural shifts are required when porting a .NET Framework Web API to ASP.NET Core? {#chapter-01-introduction-to-aspnet-core-q18}

What architectural shifts are required when porting a .NET Framework Web API to ASP.NET Core?

**Answer:** Porting requires replacing `System.Web` and `Global.asax` with `Program.cs`, middleware, and endpoint routing; migrating `Web.config` to `appsettings.json` and environment variables; and adopting built-in DI instead of third-party containers wired manually. Authentication moves from OWIN/IIS modules to ASP.NET Core authentication handlers and policies.

- `HttpContext` API changed — `Request`, `Response`, and middleware patterns differ from `HttpApplication` events.
- WCF, Web Forms, and `System.Web.Http` (Web API 2) have no direct equivalents; use ASP.NET Core controllers or Minimal APIs and gRPC or REST clients.
- EF6 can run on Core with compatibility packages, but EF Core is the recommended data stack for new ports.
- Static file handling, bundling, and session state APIs moved to middleware and distributed cache abstractions.

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

#### Q1. (R) A developer ports a .NET Framework Web API to ASP.NET Core and keeps this controller pattern. What breaks at compile time and at runtime, and what architectural shift does ASP.NET Core require instead?

```csharp
public class OrdersController : ApiController
{
    public IActionResult Get(int id)
    {
        var user = HttpContext.Current.User.Identity.Name;
        var order = OrderRepository.Find(id); // static data access
        return Json(order);
    }
}
```

---

**Answer:**

**Answer:** This code targets the .NET Framework `System.Web` stack — it will not compile on ASP.NET Core without wholesale replacement of base types, `HttpContext` access, and data access wiring through constructor injection instead of static repositories.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `ApiController` (System.Web.Http) and `HttpContext.Current` do not exist in ASP.NET Core | Project does not build |
| Architecture | Static `OrderRepository.Find` bypasses DI and test seams | Untestable, hidden dependencies |
| Runtime / security | `HttpContext.Current` was thread-affined in Framework; Core uses async request-scoped `HttpContext` via injection | Wrong identity model even if shimmed |
| API | `Json(order)` without explicit serialization policy | Inconsistent contracts vs `System.Text.Json` defaults |

**Fix (priority order):**

1. Inherit `ControllerBase`, use `[ApiController]` and attribute routing — `[Route("api/[controller]")]`.
2. Inject `IOrderRepository` (scoped) and access user via `User.Identity` on the request `HttpContext` (injected into controller).
3. Return `ActionResult<OrderDto>` or `Ok(dto)` with explicit DTO mapping — not entity graphs directly.

**Production takeaway:** Karat uses Framework carryover snippets to test whether you recognize ASP.NET Core as a **new hosting + DI + middleware** model, not an in-place API rename.

---

---

#### Q2. (P) Your team deploys the same ASP.NET Core API to Linux containers behind nginx and to Windows with IIS. Explain who runs application code, who terminates TLS, and what stays the same in `Program.cs` across both targets.

---

**Answer:**

**Answer:** Kestrel always executes your ASP.NET Core application code and middleware pipeline; nginx or IIS typically terminates TLS and reverse-proxies to Kestrel, while `Program.cs` stays the same — only hosting configuration (URLs, forwarded headers, certificates) changes per environment.

- **Kestrel** is the cross-platform web server built into ASP.NET Core — it runs `Program.cs`, the middleware pipeline, and endpoint handlers.
- **nginx (Linux)** or **IIS (Windows)** often sits in front: handles TLS certificates, HTTP/2 edge features, rate limits, WAF rules, and load balancing across replicas.
- **IIS in-process** hosts the Core app inside the IIS worker process; **out-of-process** IIS forwards to a Kestrel listener — in both cases your app logic is still ASP.NET Core.
- `WebApplication.CreateBuilder` and middleware registration are identical; deployment differs via `ASPNETCORE_URLS`, `ForwardedHeaders`, certificate binding, and container `EXPOSE` ports.
- Health checks and structured logging should not assume Windows-specific paths or IIS modules.

**Production takeaway:** "Cross-platform" means the **same codebase** ships everywhere; ops chooses the edge server — Kestrel is not optional for running your app.

---

---

#### Q3. (R) Review this `Program.cs` from a tutorial copied into a staging environment. The app starts but health checks fail and Swagger is exposed publicly. What is wrong?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run("http://0.0.0.0:5000");
```

*(Assume `ASPNETCORE_ENVIRONMENT=Staging` and the team expects HTTPS-only, no public API docs.)*

---

**Answer:**

**Answer:** The app hard-codes Kestrel to HTTP on all interfaces, always enables Swagger regardless of environment, and omits HTTPS redirection and environment guards — fine for local tutorials, unsafe for staging/production exposure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Hosting | `app.Run("http://0.0.0.0:5000")` overrides configured URLs | Conflicts with container/orchestrator port binding; may ignore `ASPNETCORE_URLS` |
| Security | Swagger/UI enabled unconditionally | Public API surface and schema disclosure in Staging |
| Security | No HTTPS redirection or HSTS | Credentials and tokens traverse plaintext if edge TLS misconfigured |
| Operations | No `/health` or readiness endpoint mapped | Load balancer marks pod unhealthy or probes wrong path |

**Fix (priority order):**

1. Remove hard-coded `Run(url)` — let configuration (`ASPNETCORE_URLS`, Kestrel config) define bindings.
2. Gate Swagger: `if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }` or secure with auth in Staging.
3. Add `app.UseHttpsRedirection()` when TLS is expected; map explicit health endpoints for probes.

**Production takeaway:** Tutorial `Program.cs` files are a common source of "works locally, fails security scan in staging" — environment-aware pipeline setup is part of introduction-level production readiness.

---

---

#### Q4. (M) A request hits `GET /api/orders/42` on a deployed ASP.NET Core app. Walk through the major stages from Kestrel accepting the socket through to the JSON response leaving the process — name the layers, not every middleware.

---

**Answer:**

**Answer:** Kestrel parses HTTP and builds `HttpContext`, the middleware pipeline runs (possibly short-circuiting), routing selects the endpoint, the controller/minimal handler executes through DI, serialization writes the response, and Kestrel flushes bytes back through the reverse proxy to the client.

1. **Reverse proxy → Kestrel:** TLS may terminate at nginx/IIS; Kestrel receives HTTP, creates `HttpContext` with connection, request, and response features.
2. **Middleware chain:** Forwarded headers, exception handling, HTTPS redirect, routing, authentication, authorization — each can inspect or short-circuit before the endpoint.
3. **Endpoint routing:** Matcher selects `OrdersController.Get(42)` or a minimal route delegate based on method + path template.
4. **Handler execution:** DI resolves scoped services (e.g., repository), action runs, returns `IActionResult` or typed result.
5. **Result execution / serialization:** `System.Text.Json` (or configured formatter) writes JSON to the response body; middleware post-processing runs on the way out.
6. **Kestrel → client:** Response headers and body stream to the connection; proxy may add its own headers.

**Production takeaway:** Interviews test whether you see ASP.NET Core as a **composed pipeline ending in an endpoint** — not "controller first" as in classic System.Web.

---

---

#### Q5. (D) Product wants a small internal tool: one POST endpoint, one GET endpoint, no MVC views, team knows C# well. When would you choose **Minimal APIs** vs **controllers**, and what would make you regret Minimal APIs six months later?

---

**Answer:**

**Answer:** Minimal APIs fit a handful of cohesive endpoints with lightweight DTOs and few cross-cutting MVC concerns; controllers pay off when validation, filters, versioning, OpenAPI grouping, and complex binding grow and you need established MVC conventions.

- **Choose Minimal APIs** when the service stays small (≤ ~10 endpoints), handlers are thin, team prefers colocated `Program.cs` routes, and you do not need action filters or complex model-binding scenarios.
- **Choose controllers** when you expect `[Authorize]` policies per action, validation filters, `ProblemDetails` conventions, API versioning, or multiple related resources sharing base behavior.
- **Regret triggers for Minimal APIs:** duplicated validation across routes, fat inline lambdas, testing friction without clear class boundaries, and OpenAPI/grouping clutter as endpoints multiply.
- **Either way** use DI, options pattern, and middleware for cross-cutting concerns — Minimal APIs are not "no architecture."
- Hybrid is valid: Minimal for health/internal ops, controllers for the public domain API.

**Production takeaway:** The decision is about **growth path and team conventions**, not raw line count — Karat wants trade-off reasoning, not "always controllers."

---

---

#### Q6. (R) A consultant claims "ASP.NET Core is just Kestrel — you don't need IIS or nginx." Review their deployment diagram assumptions. What production gaps appear when Kestrel is the only layer in front of your app?

---

**Answer:**

**Answer:** Kestrel alone can serve traffic but lacks many edge capabilities teams rely on in production — centralized TLS management, load balancing, request buffering, WAF, and graceful multi-process management — so exposing Kestrel directly is usually a deliberate simplification, not enterprise default.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No WAF, rate limiting at edge, or IP allow lists | Vulnerability to abuse before app middleware runs |
| TLS / ops | Certificate rotation and cipher policy on every app instance | Operational burden; misconfiguration risk |
| Scale | Single-process Kestrel vs load-balanced fleet behind proxy | Uneven connection handling; harder rolling deploys |
| Observability | Missing proxy access logs and request IDs at edge | Harder incident triage |
| Platform | Windows IIS integration (app pool recycle, Windows auth) bypassed | Loses familiar ops tooling where required |

**Fix (priority order):**

1. Place nginx, IIS, YARP, or cloud load balancer in front for TLS termination and balancing.
2. Configure `ForwardedHeaders` so the app sees correct scheme and client IP.
3. Keep Kestrel as the process running `Program.cs` — not as the sole public internet face unless threat model allows.

**Production takeaway:** Kestrel **runs** ASP.NET Core; it is not a full replacement for reverse-proxy operational features — the intro chapter sets up why deployments are layered.

---

---

#### Q7. (R) The unified hosting model (`WebApplication.CreateBuilder`) replaced `IWebHost` / `Startup.cs` for most new projects. Review this partial migration — what breaks middleware order, integration tests, and endpoint discovery?

```csharp
// Migrated from Startup.Configure — "should be equivalent"
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRouting();
app.Run();

// Integration tests still use:
// new HostBuilder().ConfigureWebHost(b => b.UseStartup<Startup>()).Build();
```

---

**Answer:**

**Answer:** This migration copies `Startup.Configure` lines out of order, registers endpoints before routing, and leaves tests on `IWebHost` — controllers 404, auth misapplies, and `WebApplicationFactory` cannot boot the app.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline order | `MapControllers()` before `UseRouting()` | Endpoint matching fails — 404 on all routes |
| Migration | `UseEndpoints` removed but routing split incorrectly | Subtle differences from old template |
| Testing | Tests still use `IWebHost` / generic host boot | CI fails or tests wrong pipeline |
| Configuration | Duplicate manual `appsettings` loads from old `Startup` | Wrong config precedence |

**Fix (priority order):**

1. Map `Startup.Configure` middleware order 1:1 — routing before endpoints, auth after routing.
2. Expose `public partial class Program { }` for `WebApplicationFactory<Program>`.
3. Remove duplicate configuration providers; rely on `WebApplication.CreateBuilder(args)`.

```csharp
var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Production takeaway:** Unified hosting reduces ceremony but migrations fail when teams treat it as rename-only — **pipeline order and test bootstrapping** must be revalidated.

---

---

#### Q8. (R) Review this cross-platform CI script comment and the accompanying `Program.cs` change. Builds pass on Windows agents but the container crashes on Linux with `Address already in use`. Diagnose and fix.

```csharp
// Dockerfile EXPOSE 8080 — CI sets ASPNETCORE_URLS=http://+:8080
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000"); // force dev URL
var app = builder.Build();
app.MapGet("/health", () => Results.Ok("healthy"));
app.Run();
```

**Answer:**

**Answer:** `UseUrls("http://localhost:5000")` forces a fixed localhost binding that conflicts with container orchestration expecting port 8080 on `0.0.0.0`, and may collide when the platform already sets `ASPNETCORE_URLS`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Hosting | Hard-coded `UseUrls` overrides `ASPNETCORE_URLS=http://+:8080` | Container listens on wrong port — health probes fail |
| Cross-platform | `localhost` binding inside container is not reachable from probe network | Kubernetes/Docker health checks time out |
| Config | Dockerfile `EXPOSE 8080` disagrees with Kestrel bind | "Address already in use" if supervisor also binds 8080 |
| Dev/prod parity | Windows agent may mask port conflict | Linux deploy fails intermittently |

**Fix (priority order):**

1. Remove `builder.WebHost.UseUrls(...)` — rely on `ASPNETCORE_URLS`, `Kestrel` config section, or `appsettings`.
2. Bind `http://+:8080` (all interfaces) in container environments, not `localhost`.
3. Align probe URLs in orchestrator with the configured listen port.

```csharp
var builder = WebApplication.CreateBuilder(args);
// URLs from env/config only — no UseUrls in container images
var app = builder.Build();
app.MapGet("/health", () => Results.Ok("healthy"));
app.Run();
```

**Production takeaway:** Cross-platform deploy success depends on **configuration-driven URLs**, not developer-machine defaults baked into `Program.cs`.

---

---
