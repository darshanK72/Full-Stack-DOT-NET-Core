# Introduction to ASP.NET Core — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is ASP.NET Core?](#q1-what-is-aspnet-core)
2. [Q2. How does ASP.NET Core differ from ASP.NET Framework?](#q2-how-does-aspnet-core-differ-from-aspnet-framework)
3. [Q3. What is Kestrel, and what role does it play in ASP.NET Core?](#q3-what-is-kestrel-and-what-role-does-it-play-in-aspnet-core)
4. [Q4. Why do production deployments often place nginx or IIS in front of Kestrel?](#q4-why-do-production-deployments-often-place-nginx-or-iis-in-front-of-kestrel)
5. [Q5. Who terminates TLS in a typical reverse-proxy deployment?](#q5-who-terminates-tls-in-a-typical-reverse-proxy-deployment)
6. [Q6. What is the unified hosting model introduced by `WebApplication.CreateBuilder`?](#q6-what-is-the-unified-hosting-model-introduced-by-webapplicationcreatebuilder)
7. [Q7. What is the difference between the old `Startup.cs` pattern and the modern minimal hosting model?](#q7-what-is-the-difference-between-the-old-startupcs-pattern-and-the-modern-minimal-hosting-model)
8. [Q8. Walk through the major stages of an HTTP request in ASP.NET Core (high level).](#q8-walk-through-the-major-stages-of-an-http-request-in-aspnet-core-high-level)
9. [Q9. When would you choose Minimal APIs over MVC controllers?](#q9-when-would-you-choose-minimal-apis-over-mvc-controllers)
10. [Q10. What does `ASPNETCORE_ENVIRONMENT` control?](#q10-what-does-aspnetcore_environment-control)
11. [Q11. What makes ASP.NET Core suitable for Linux containers and cloud deployment?](#q11-what-makes-aspnet-core-suitable-for-linux-containers-and-cloud-deployment)
12. [Q12. What is the ASP.NET Core request pipeline?](#q12-what-is-the-aspnet-core-request-pipeline)
13. [Q13. What is endpoint routing?](#q13-what-is-endpoint-routing)
14. [Q14. What are the main components registered in `Program.cs`?](#q14-what-are-the-main-components-registered-in-programcs)
15. [Q15. What is cross-platform hosting in the context of ASP.NET Core?](#q15-what-is-cross-platform-hosting-in-the-context-of-aspnet-core)
16. [Q16. How does ASP.NET Core handle dependency injection by default?](#q16-how-does-aspnet-core-handle-dependency-injection-by-default)
17. [Q17. What is the difference between in-process and out-of-process IIS hosting?](#q17-what-is-the-difference-between-in-process-and-out-of-process-iis-hosting)
18. [Q18. What architectural shifts are required when porting a .NET Framework Web API to ASP.NET Core?](#q18-what-architectural-shifts-are-required-when-porting-a-net-framework-web-api-to-aspnet-core)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is ASP.NET Core?

**Concepts**
- Cross-platform, modular successor to ASP.NET Framework
- Kestrel as the built-in web server
- Composable middleware pipeline
- `WebApplication.CreateBuilder` unified hosting entry point
- MVC, Razor, and Minimal APIs sharing one host

**Answer**

ASP.NET Core is the cross-platform web framework for building HTTP services, APIs, MVC sites, and real-time apps on modern .NET. Its core shift from ASP.NET Framework is that every concern — routing, authentication, serialization, static files — is opt-in middleware rather than implicitly loaded by IIS, which is why the same application binary runs on Linux and Windows without modification. The entry point is `WebApplication.CreateBuilder`, which configures DI, Kestrel, configuration, and logging in one `Program.cs` flow, and the resulting host supports both controller-based MVC and Minimal APIs side-by-side sharing the same routing, auth, and pipeline infrastructure.

---

## Q2. How does ASP.NET Core differ from ASP.NET Framework?

**Concepts**
- `System.Web` vs explicit middleware pipeline
- Windows-only IIS dependency vs cross-platform Kestrel
- GAC/machine-wide installs vs self-contained deployment
- Implicit request lifecycle vs explicit service registration
- `appsettings.json` + environment variables vs `web.config`

**Answer**

The fundamental difference is that ASP.NET Framework was built on `System.Web` and IIS, where request lifecycle events fired implicitly through HTTP modules and handlers tied to Windows. ASP.NET Core replaced that with an explicit middleware pipeline where every stage — routing, auth, endpoints — is registered in code, which means the application is self-describing and portable. Deployment changed from requiring a machine-wide GAC install and `web.config` to shipping either self-contained or framework-dependent executables with `appsettings.json` and environment variables for configuration. Microsoft keeps Framework 4.8 in maintenance mode, so new web development targets ASP.NET Core; the patterns that don't port directly — Web Forms, WCF, and `System.Web.Http` Web API 2 — have no equivalents and require architectural replacement.

---

## Q3. What is Kestrel, and what role does it play in ASP.NET Core?

**Concepts**
- Cross-platform web server built into ASP.NET Core
- TCP connection termination and HTTP parsing
- `RequestDelegate` chain entry point
- HTTP/1.x, HTTP/2, HTTP/3, and WebSocket support
- ASP.NET Core Module (ANCM) for IIS backend

**Answer**

Kestrel is the built-in cross-platform web server that listens for TCP connections, parses HTTP, creates an `HttpContext`, and invokes the middleware pipeline. It is not optional — the pipeline requires a server to supply the request context, and Kestrel is that server on all platforms. On Windows, it can run standalone or as the backend behind IIS through the ASP.NET Core Module (ANCM), where IIS forwards requests to Kestrel rather than running ASP.NET natively. Kestrel handles HTTP/1.x, HTTP/2, HTTP/3 (when configured), and WebSockets, and exposes configurable limits on connection count, request body size, and header count. In production it typically listens on an internal port while a reverse proxy handles TLS at the public edge.

---

## Q4. Why do production deployments often place nginx or IIS in front of Kestrel?

**Concepts**
- Reverse proxy for TLS termination and edge concerns
- Static-file caching and compression at the proxy layer
- Load balancing and zero-downtime deploys
- `UseForwardedHeaders` for client IP and scheme
- Slowloris and oversized payload filtering before .NET

**Answer**

Kestrel is optimized as an application server, not as an internet-facing edge gateway, so a reverse proxy handles the operational concerns that belong at the network boundary. The proxy holds the public TLS certificate, terminates HTTPS, and forwards plain HTTP to Kestrel on a loopback or internal interface, which means certificate rotation, cipher policy, and HTTP/2 edge features are managed once at the proxy rather than on every app instance. Edge servers can also serve cached static assets, apply gzip/brotli compression, rate-limit connections, and drop malformed or oversized requests before they reach the .NET process. When multiple Kestrel instances run behind nginx or IIS Application Request Routing, the proxy distributes load and enables zero-downtime rolling restarts. The one configuration consequence is that `UseForwardedHeaders` must be registered early so Kestrel reads `X-Forwarded-Proto` and `X-Forwarded-For` correctly instead of seeing `http` and the proxy's IP.

---

## Q5. Who terminates TLS in a typical reverse-proxy deployment?

**Concepts**
- TLS termination at the reverse proxy layer
- `X-Forwarded-Proto` header and `UseForwardedHeaders`
- Kestrel direct TLS via `ListenOptions.UseHttps`
- End-to-end TLS (proxy-to-Kestrel mTLS) for defense in depth
- Broken `Secure` flag and redirect behavior from misconfigured headers

**Answer**

In a standard nginx-in-front-of-Kestrel or IIS-in-front-of-Kestrel setup, the reverse proxy terminates TLS — it holds the public certificate, negotiates HTTPS with the client, and forwards decrypted HTTP to Kestrel, which means Kestrel sees `http` on the internal connection unless end-to-end TLS is configured separately. The application learns the original scheme through `X-Forwarded-Proto: https` only when `UseForwardedHeaders` is configured to trust the proxy network; without it, `HttpContext.Request.Scheme` reads `http` even though clients used HTTPS, which breaks HTTPS redirects, `Secure` cookie flags, and generated links. Kestrel can terminate TLS directly using `ListenOptions.UseHttps` when deployed without a proxy, and some hardened deployments use TLS at both layers — proxy to client and mTLS to Kestrel — for defense in depth inside a private network.

---

## Q6. What is the unified hosting model introduced by `WebApplication.CreateBuilder`?

**Concepts**
- `WebApplicationBuilder` merging generic host and web host
- Pre-wired defaults for logging, configuration, DI, and Kestrel
- `WebApplication` as both host and pipeline configurator
- Top-level statements reducing `Program.cs` ceremony
- Standard entry point in ASP.NET Core 6+

**Answer**

`WebApplication.CreateBuilder` merges the generic host and web host into a single bootstrap API so that logging, configuration, DI, and Kestrel are all wired with production-sensible defaults before you write a single service registration. The builder exposes `Services`, `Configuration`, `Environment`, and `Logging` directly, which means you no longer coordinate `Host.CreateDefaultBuilder` with `ConfigureWebHostDefaults` as the older two-method split required. Calling `builder.Build()` produces a `WebApplication` that is simultaneously the host lifecycle manager and the pipeline configurator — there is no separate `IWebHost` or `IHost` to juggle. The result supports top-level statements and minimal boilerplate while still giving full access to MVC, Razor, SignalR, and hosted services, and it has been the standard ASP.NET Core project template entry point since version 6.

---

## Q7. What is the difference between the old `Startup.cs` pattern and the modern minimal hosting model?

**Concepts**
- `ConfigureServices` vs `builder.Services` for DI registration
- `Configure` method vs in-place middleware registration on `WebApplication`
- Functional equivalence of both patterns at runtime
- `Startup` class still supported via `builder.Host.ConfigureWebHostDefaults`
- C# top-level statements reducing boilerplate

**Answer**

The `Startup.cs` pattern split DI registration and pipeline configuration into two methods on a separate class: `ConfigureServices` wired services and `Configure` registered middleware. The minimal hosting model collapses both into a single top-level `Program.cs` where builder phase (`builder.Services.Add...`) and pipeline phase (`app.Use...`, `app.Map...`) are interleaved without a `Startup` class. Functionally they are identical — `builder.Services.AddControllers()` replaces `ConfigureServices`, and `app.UseRouting(); app.MapControllers()` replaces pipeline setup in `Configure`. Teams that prefer the explicit separation can still use a `Startup` class via `builder.Host.ConfigureWebHostDefaults`, but the minimal model reduces ceremony and aligns with C# top-level statements in modern project templates. Both compile to the same underlying `WebApplication` host at runtime.

---

## Q8. Walk through the major stages of an HTTP request in ASP.NET Core (high level).

**Concepts**
- Kestrel TCP parsing and `HttpContext` creation
- Middleware pipeline traversal in registration order
- Endpoint routing selecting the handler
- Authorization evaluating endpoint metadata
- `IActionResult`/`IResult` execution and response return path

**Answer**

An HTTP request enters Kestrel, which parses the TCP stream, builds an `HttpContext` with `Request`, `Response`, and a request-scoped `RequestServices` container, and passes it to the first middleware in the pipeline. Middleware runs in registration order — exception handling, HTTPS redirection, forwarded headers, static files — until routing runs and selects a matching endpoint. Authorization middleware then evaluates `[Authorize]` metadata on the matched endpoint, which is why routing must precede auth; a 401 or 403 response can short-circuit here without reaching the handler. If authorization passes, the endpoint delegate or controller action executes: DI resolves scoped dependencies, the handler runs, and produces an `IActionResult`, `IResult`, or writes directly to the response. Result execution serializes the body, and the response propagates back through the middleware chain in reverse, where compression or response-modification middleware can act on the outbound stream before Kestrel flushes bytes to the client.

---

## Q9. When would you choose Minimal APIs over MVC controllers?

**Concepts**
- Minimal APIs reducing ceremony for small service surfaces
- Controllers for complex filter pipelines and MVC conventions
- `MapGroup`, endpoint filters, and `TypedResults` in Minimal APIs
- `[ApiController]` attribute conventions at scale
- Shared host infrastructure for both approaches

**Answer**

I'd reach for Minimal APIs when the service is small — a microservice, an internal tool, or a prototype with fewer than roughly ten endpoints — because `app.MapGet("/items", handler)` with inline DI and parameter binding avoids the controller class, attribute routing, and DI constructor that add indirection for little payoff at that scale. Controllers pay back their ceremony when the API grows: `[ApiController]` conventions handle model validation responses automatically, resource-level base classes share filters, `ProblemDetails` formatting is consistent, and versioning libraries integrate cleanly with controller metadata. Both approaches share the same host, authentication, authorization, OpenAPI, and serialization infrastructure in ASP.NET Core, so the choice is about growth path and convention density, not capability. The point where I'd regret starting with Minimal APIs is when handlers grow fat inline lambdas, validation duplicates across routes, and endpoint grouping creates its own boilerplate that starts resembling a controller anyway.

---

## Q10. What does `ASPNETCORE_ENVIRONMENT` control?

**Concepts**
- Hosting environment name read into `IWebHostEnvironment`
- `appsettings.{Environment}.json` layered configuration
- `IsDevelopment()` gating developer-only middleware
- Environment variables as the production configuration mechanism
- Developer exception page and sensitive data exposure risk

**Answer**

`ASPNETCORE_ENVIRONMENT` sets the hosting environment name — typically `Development`, `Staging`, or `Production` — which is read at startup into `IWebHostEnvironment.EnvironmentName` and drives several automatic behaviors. Configuration loading layers `appsettings.{Environment}.json` on top of the base `appsettings.json` so staging can override connection strings without touching the base file. `builder.Environment.IsDevelopment()` gates developer-only middleware like `UseDeveloperExceptionPage()` and EF Core's sensitive-data logging, which means the value in production matters for security: a production host running with `Development` accidentally exposes full stack traces and query parameters to clients. The variable is set via the host's environment — container orchestrators, IIS site settings, or deployment pipelines — never from `appsettings.json`, since configuration itself depends on it loading first.

---

## Q11. What makes ASP.NET Core suitable for Linux containers and cloud deployment?

**Concepts**
- Native Linux/macOS runtime without Windows dependency
- Self-contained vs framework-dependent deployment packaging
- Twelve-factor app configuration via environment variables
- Health check endpoints for Kubernetes probes
- Side-by-side runtime versioning without machine-wide installs

**Answer**

ASP.NET Core runs natively on Linux and macOS and ships either self-contained (embedding the runtime) or framework-dependent (using a pre-installed runtime layer), both of which produce a single executable deployable without a Windows server or IIS. Kestrel binds to ports configured through environment variables (`ASPNETCORE_URLS`), which aligns with twelve-factor app patterns used by Kubernetes, Azure App Service, and AWS — no machine-wide configuration files need changing between environments. Docker images use `mcr.microsoft.com/dotnet/aspnet:10.0` as the runtime layer, keeping final images small since the SDK image is only needed at build time. Health check endpoints via `MapHealthChecks` plug directly into Kubernetes liveness and readiness probes, and because runtimes install side-by-side, one container image can target a specific .NET version without affecting other workloads on the same node.

---

## Q12. What is the ASP.NET Core request pipeline?

**Concepts**
- Ordered chain of `RequestDelegate` components
- `app.Use` / `app.Run` / `app.Map` registration
- Pipeline built once at startup when `app.Run()` is called
- Terminal middleware ending the chain
- `WebApplicationFactory` for integration test coverage

**Answer**

The request pipeline is an ordered chain of middleware components, each receiving an `HttpContext` and a reference to the next delegate in the chain, which it can call to continue or skip to short-circuit. Middleware is registered in `Program.cs` with `app.Use...`, `app.Map`, or `app.Run`; order is the only thing that determines behavior because the pipeline is linear and each component sees the same `HttpContext` on the way in and on the return path. The chain is built exactly once when `app.Run()` is called, so all `Use` registrations must be complete before that call. Terminal middleware — or an endpoint — produces the response and does not call next; earlier middleware in the chain may then modify headers or the body stream on the response return path. Unlike `System.Web` modules, which fired events implicitly, the Core pipeline is fully explicit and can be exercised end-to-end in integration tests with `WebApplicationFactory` without deploying a server.

---

## Q13. What is endpoint routing?

**Concepts**
- Route matching decoupled from endpoint execution
- `UseRouting` for matching, `Map*` for endpoint registration
- Endpoint metadata consumed by auth and OpenAPI middleware
- `IRouter`-based legacy routing replaced in ASP.NET Core 3+
- `MapControllers`, `MapGet`, `MapRazorPages` as registration methods

**Answer**

Endpoint routing separates the act of matching an incoming request to a route template from the act of executing the matched handler, which is the key architectural difference from the older `IRouter`-based system where routing and execution were coupled. `UseRouting` runs the matching logic and attaches the selected endpoint to `HttpContext.GetEndpoint()`; middleware registered after it — authentication, authorization, rate limiting — can inspect the endpoint's metadata before the handler runs. Endpoints are registered with `MapControllers()`, `MapGet()`, `MapRazorPages()`, and similar methods that attach route templates, HTTP methods, and typed metadata like `[Authorize]` policies or `ProducesResponseType` annotations. In minimal hosting, `Map*` calls on `WebApplication` handle both routing registration and endpoint registration in one call. OpenAPI generators and authorization middleware both read the endpoint metadata tree at startup, which is why endpoint routing enables accurate policy application and schema generation that the legacy `IRouter` model could not provide.

---

## Q14. What are the main components registered in `Program.cs`?

**Concepts**
- Builder phase (`Services`) vs pipeline phase (`WebApplication`)
- `AddControllers`, `AddAuthentication`, `AddDbContext` as service registrations
- `UseHttpsRedirection`, `UseAuthentication`, `UseAuthorization` middleware order
- `AddHostedService` for background work
- Kestrel limits and configuration extension points

**Answer**

`Program.cs` has two distinct phases: the builder phase where services are registered into the DI container, and the pipeline phase where middleware and endpoints are added to the built `WebApplication`. During the builder phase, typical registrations include `AddControllers()`, `AddAuthentication()` with a scheme, `AddAuthorization()` with policies, `AddDbContext<TContext>()`, `AddHttpClient()`, `AddEndpointsApiExplorer()`, and `AddSwaggerGen()`. `WebApplication.CreateBuilder` pre-registers framework services like `ILogger<T>`, `IConfiguration`, and `IWebHostEnvironment` automatically. The pipeline phase follows: `UseHttpsRedirection()`, `UseAuthentication()`, `UseAuthorization()`, then endpoint mapping like `MapControllers()` and `MapHealthChecks()`. Hosted services go into the builder phase via `AddHostedService<T>()`, and Kestrel limits or socket configuration extend `builder.WebHost`. The ordering within the pipeline phase is what matters for correctness — auth middleware must come after routing.

---

## Q15. What is cross-platform hosting in the context of ASP.NET Core?

**Concepts**
- Same binary running on Windows, Linux, macOS without recompilation
- Kestrel as the cross-platform server vs IIS (Windows-only)
- Runtime identifier (RID) for platform-targeted publish output
- Linux case-sensitivity for static files and paths
- nginx/Apache as the Linux reverse proxy equivalent of IIS

**Answer**

Cross-platform hosting means the same `Program.cs`, middleware configuration, and business logic compile once and run on Windows, Linux, and macOS without a platform-specific code path, since ASP.NET Core and Kestrel abstract the OS socket and threading APIs. Developing on Windows and deploying to Linux containers is a standard workflow — `dotnet publish -r linux-x64` produces a self-contained or framework-dependent output runnable on any compatible Linux distribution. The main practical differences are at the infrastructure layer: IIS is Windows-only, so Linux production environments use Kestrel behind nginx, Apache, or a cloud load balancer; and Linux file systems are case-sensitive, which surfaces path and static-file bugs that Windows development hides. Path separator handling and line-ending conventions also require attention when file paths appear in configuration or routing logic.

---

## Q16. How does ASP.NET Core handle dependency injection by default?

**Concepts**
- Built-in DI container configured through `builder.Services`
- Singleton, Scoped, and Transient lifetime registrations
- Constructor injection as the default resolution pattern
- Request-scoped DI scope per HTTP request
- Pre-registered framework services (`ILogger`, `IConfiguration`)

**Answer**

ASP.NET Core includes a built-in DI container that is configured through `builder.Services` during the builder phase and made available for the lifetime of the application. Services are registered with one of three lifetimes: Singleton (one instance for the app lifetime), Scoped (one per HTTP request), or Transient (new instance each time). Constructor injection is the default pattern — the container inspects constructor parameters and resolves registered types when activating controllers, middleware factories, and handlers. Each HTTP request automatically creates and disposes a DI scope, so Scoped services like `DbContext` are safe to inject into controllers without manual lifecycle management. `WebApplication.CreateBuilder` pre-registers framework services including `ILogger<T>`, `IConfiguration`, `IWebHostEnvironment`, and `IHttpContextAccessor`. Third-party containers like Autofac can replace the default provider via `Host.UseServiceProviderFactory` if the built-in container's feature set is insufficient.

---

## Q17. What is the difference between in-process and out-of-process IIS hosting?

**Concepts**
- In-process hosting running Core CLR inside `w3wp.exe`
- Out-of-process forwarding to a Kestrel child process
- ASP.NET Core Module (ANCM) v2 enabling both modes
- `hostingModel` attribute in `web.config`
- Crash isolation difference between modes

**Answer**

In-process hosting loads the Core CLR into the IIS worker process (`w3wp.exe`) through ASP.NET Core Module v2, so requests flow directly from IIS's native pipeline to the ASP.NET Core middleware without a network hop — this is the default on IIS for better throughput and simpler debugging on Windows. Out-of-process hosting runs Kestrel in a separate `dotnet.exe` child process, and ANCM forwards HTTP requests to it over a localhost connection; if the ASP.NET Core process crashes, IIS can restart it independently without recycling the entire IIS worker process. The hosting model is selected per-application via the `hostingModel` attribute in `web.config` (`inprocess` or `outofprocess`). Neither mode applies to Linux; this is purely an IIS-on-Windows concern, and container deployments run Kestrel directly.

---

## Q18. What architectural shifts are required when porting a .NET Framework Web API to ASP.NET Core?

**Concepts**
- Replacing `System.Web` and `Global.asax` with `Program.cs` and middleware
- `web.config` to `appsettings.json` and environment variable migration
- Built-in DI replacing manual or third-party container wiring
- OWIN/IIS auth modules to ASP.NET Core authentication handlers
- `HttpContext` API differences between Framework and Core

**Answer**

Porting a .NET Framework Web API to ASP.NET Core requires replacing the entire hosting infrastructure, not just updating namespaces. `System.Web`, `Global.asax`, and the `HttpApplication` event model have no equivalents — they must be replaced with `Program.cs`, middleware, and endpoint routing. `Web.config` app settings migrate to `appsettings.json` and environment variables, with `IConfiguration` as the runtime abstraction. Dependency injection, which was often a third-party container wired manually in Framework, becomes the built-in DI container configured through `builder.Services`, so service registrations need to be moved. Authentication moves from OWIN middleware or IIS Windows Auth modules to ASP.NET Core authentication handlers and `[Authorize]` policies. The `HttpContext` API surface changed — `Request`, `Response`, and session APIs differ, and `HttpContext.Current` does not exist in Core since requests are async and not thread-affined. Libraries that targeted `System.Web.Http` Web API 2, WCF, or Web Forms have no direct ports and require architectural replacement with Core controllers, Minimal APIs, or gRPC.

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- `UseRouting` must precede `UseAuthentication` and `UseAuthorization`
- Endpoint metadata read by auth middleware after routing selects endpoint
- Authorization policy evaluation against matched endpoint metadata
- Recommended order: exceptions → forwarded headers → routing → auth → endpoints

**Answer**

The ASP.NET Core middleware pipeline runs in registration order, so authorization middleware can only read the matched endpoint's metadata — including `[Authorize]` attributes and policy names — if routing has already run. When `UseAuthentication` or `UseAuthorization` is registered before `UseRouting`, the endpoint has not been selected yet, so endpoint-aware authorization is silently skipped and protected routes become accessible anonymously. The correct order is: exception handling, forwarded headers, routing, authentication, authorization, then endpoint mapping (`MapControllers`, `MapGet`). Symptoms of this mistake are 401 responses without proper WWW-Authenticate challenges or, more dangerously, anonymous access to endpoints that carry `[Authorize]` metadata.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive dependency anti-pattern (scoped inside singleton)
- `DbContext` change tracker accumulating entities across requests
- `ValidateScopes` startup validation catching illegal combinations
- `IServiceScopeFactory` for creating explicit per-operation scopes
- `IDbContextFactory<T>` for singleton operations on `DbContext`

**Answer**

When a singleton service captures a scoped dependency like `DbContext` in its constructor, the scoped instance is created once and held for the application lifetime instead of being created and disposed per request. The result is that EF Core's change tracker accumulates entities from every request, thread safety is violated, and eventually a disposed-object exception surfaces when the original scope ends while the singleton's reference persists. The fix is to inject `IServiceScopeFactory` into the singleton and create a scope per operation — `using var scope = factory.CreateScope()` — or use `IDbContextFactory<T>`, which handles scoping internally. Enable `ValidateScopes: true` in Development and Staging environments so the container catches illegal scope combinations at startup, where they're easy to diagnose, rather than in production under load.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- `HttpMessageHandler` socket pool lifetime
- Socket exhaustion from per-use `HttpClient` instantiation
- `IHttpClientFactory` managing handler lifetimes and recycling
- Typed and named client registration pattern

**Answer**

Instantiating `HttpClient` with `new` inside a singleton — or wrapping it in a `using` block — defeats the connection pool because each `HttpClient` instance owns its own `HttpMessageHandler` and holds its sockets open until garbage collected, not until `Dispose` is called. Under real traffic this produces socket exhaustion that surfaces only in production, typically as `SocketException` or connection timeout errors that don't appear in local testing. `IHttpClientFactory` solves this by managing `HttpMessageHandler` lifetimes separately from `HttpClient` lifetimes: handlers are pooled and recycled on a timer so DNS changes propagate, while `HttpClient` instances are lightweight wrappers that can be created per-use. Register typed clients with `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()` and inject `IExternalApi` into your services rather than `HttpClient` directly.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- `IOptions<T>` snapshot fixed at first resolution
- `IOptionsSnapshot<T>` recalculating per request scope
- `IOptionsMonitor<T>` live change notifications via `OnChange`
- Singleton services requiring `IOptionsMonitor` for live updates
- `reloadOnChange` in configuration without corresponding options interface

**Answer**

`IOptions<T>` resolves and caches the configuration snapshot at the first call to `.Value`, which means a singleton that reads `IOptions<T>.Value` at construction time will never see updated values even if `appsettings.json` reloads with `reloadOnChange: true`. `IOptionsSnapshot<T>` recalculates per request scope and works correctly for scoped or transient services, but singleton services must use `IOptionsMonitor<T>`, which exposes an `OnChange` callback and always returns current values from `.CurrentValue`. The failure mode is silent: the process keeps running with stale configuration until it restarts, which makes this easy to miss in testing where configuration rarely changes mid-run. For singleton services that need live feature flags or connection string updates, `IOptionsMonitor<T>` is the correct interface.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET semantics discouraging request bodies
- Proxy and CDN stripping of GET bodies
- `[FromQuery]` with `[AsParameters]` for complex GET filter objects
- Safe and idempotent GET expectations in REST conventions

**Answer**

`[FromBody]` on a GET handler tells the model binder to deserialize the request body, but many HTTP clients, CDNs, caching proxies, and browsers strip or ignore bodies on GET requests by convention, so the bound parameter silently receives its default value rather than the intended data. The failure typically appears only in production or behind specific infrastructure, not in Swagger's "Try it out" during development where the body is sent verbatim. Complex filter objects on GET endpoints should use `[FromQuery]` with either individual parameters or `[AsParameters]` on a DTO class, which maps query string keys to properties without needing a body. This also keeps GET endpoints safe, idempotent, and cacheable — properties that body-bearing GETs sacrifice.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- Default `JsonNamingPolicy.CamelCase` serialization in ASP.NET Core
- Silent binding failure leaving properties at default values
- `PropertyNameCaseInsensitive` as an opt-in deserialization setting
- Standardizing client contracts on camelCase

**Answer**

ASP.NET Core serializes JSON with camelCase property names by default via `System.Text.Json` and `JsonNamingPolicy.CamelCase`, so when mobile or legacy clients POST JSON with PascalCase keys — `"CustomerName"` instead of `"customerName"` — model binding silently leaves the matching C# property at its default value rather than throwing a 400. The binding failure is invisible in logs because the deserialization itself succeeds without error. Standardizing clients on camelCase and documenting the contract in OpenAPI is the cleanest fix; if backward compatibility requires accepting PascalCase, `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` enables case-insensitive matching, though this loosens the contract for all endpoints. Adding `[Required]` or `Range` validation attributes on key properties turns silent binding failures into explicit 400 responses, making the bug visible during development rather than at runtime.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- Stack trace reset by `throw ex` vs preservation by bare `throw`
- `InnerException` preservation when wrapping exceptions
- Exception filters and Application Insights relying on accurate stack traces

**Answer**

`throw ex` reassigns the exception's stack trace to the current catch block line, so exception filters, structured logging, and Application Insights show the re-throw site rather than the original failure location, hiding root causes behind infrastructure code. Bare `throw;` preserves the complete original stack trace through the catch block. When rethrowing after logging or cleanup, always use `throw;`. The only valid reason to wrap is when you need to add domain context: `throw new OrderProcessingException("Failed to process order", ex)` preserves the original as `InnerException` while surfacing a domain-meaningful type to callers. This trap appears in both application code and background worker error handlers where developers assume rethrowing under a new type requires `throw ex`.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel as application server vs full edge gateway
- TLS certificate management complexity per instance
- WAF, rate limiting, and static-file caching at the proxy layer
- `UseForwardedHeaders` for client IP when behind a proxy
- Ingress controller handling HTTPS externally in containers

**Answer**

Kestrel is production-grade for running ASP.NET Core application logic, but using it as the sole public internet endpoint means every concern that production deployments typically handle at the network edge — TLS certificate rotation, cipher policy, rate limiting, WAF rules, static-file caching, and multi-instance load balancing — must be handled either inside the .NET process or not at all. Placing nginx, IIS, Azure Front Door, or an AWS ALB in front centralizes these concerns and lets Kestrel focus on application code. Container deployments typically bind Kestrel to port 8080 on the pod network, with the ingress controller handling HTTPS externally. If Kestrel is genuinely the only layer, client IP logging and link generation still work correctly, but the operational complexity is concentrated in the app process with no separation of concerns.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- `launchSettings.json` applies only to `dotnet run` and Visual Studio launch
- `ASPNETCORE_URLS` and `ASPNETCORE_ENVIRONMENT` as production configuration
- `appsettings.Production.json` and host-level env vars for deployed environments
- Development ergonomics vs runtime configuration

**Answer**

`Properties/launchSettings.json` is a development ergonomics file read only by `dotnet run`, Visual Studio, and VS Code launch profiles — it is never deployed to production servers or containers and has no effect on hosted environments. Settings there, including `applicationUrl`, `ASPNETCORE_ENVIRONMENT`, and any custom environment variables, do not carry forward to IIS, Docker, Kubernetes, or Azure App Service. Production URLs and environment names must come from the host's environment variable system — `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`, or orchestrator config maps — and application settings from `appsettings.Production.json` or a secrets manager. Assuming `launchSettings.json` sets production behavior is how teams deploy with the wrong environment name or binding and then can't reproduce the problem locally.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- `System.Text.Json` deserializing absent JSON fields to `default(bool) = false`
- Tri-state semantics requiring `bool?` or an enum
- PATCH vs PUT semantics for partial updates
- Nullable model properties in OpenAPI client generation

**Answer**

A non-nullable `bool` on a PATCH DTO cannot distinguish "field omitted from the JSON body" from "field explicitly set to false" because `System.Text.Json` deserializes missing JSON properties to `default(bool)`, which is `false`. This means a PATCH request that omits `IsActive` silently sets `IsActive = false` rather than leaving the field unchanged — a data corruption bug that's particularly dangerous for marketing consent fields, feature flags, and access control properties. The fix is `bool?` on the DTO: a `null` value means "not provided, keep existing," while `true` or `false` means "explicitly set." Alternatively, an enum with an `Unspecified` state makes intent explicit in code and in generated OpenAPI clients, which represent optional updates correctly when properties are nullable.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- `X-Forwarded-For`, `X-Forwarded-Proto`, `X-Forwarded-Host` headers
- `HttpContext.Request.Scheme` showing `http` instead of `https` without middleware
- `ForwardedHeadersOptions.KnownProxies`/`KnownNetworks` for trust scope
- HTTPS redirection and `Secure` cookie flag requiring correct scheme
- Proxy header spoofing risk from untrusted `ForwardedHeaders` config

**Answer**

Without `UseForwardedHeaders`, `HttpContext.Request.Scheme` reads `http` because Kestrel sees an HTTP connection from the proxy, even though the client used HTTPS. This breaks HTTPS redirect middleware (which redirects in a loop or does nothing), cookie `Secure` flag generation, OAuth redirect URIs, and any link generation that uses the request scheme. `UseForwardedHeaders()` must be registered early in the pipeline — before HTTPS redirection and any middleware that reads scheme, host, or client IP. `ForwardedHeadersOptions` must be configured with `KnownProxies` or `KnownNetworks` set to your actual reverse proxy addresses because accepting forwarded headers from untrusted sources allows header injection that spoofs client IP and scheme. Local development without a proxy does not need this middleware; production behind nginx, IIS, or a cloud load balancer always does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- `UseStaticFiles` serving `wwwroot` to unauthenticated clients
- Web root boundary and unauthorized file exposure risk
- `IConfiguration` and environment variables for secrets outside web root
- Build pipeline verification of web root contents

**Answer**

`UseStaticFiles` serves every file under `wwwroot` to any client, authenticated or not, by default — there is no authorization check on static file requests. Any secret, configuration backup, or private key that ends up in `wwwroot` is immediately accessible over HTTP with a direct path request. Only public assets — CSS, JavaScript, images, and public PDFs — belong there. Sensitive configuration must be loaded through `IConfiguration`, environment variables, or a secrets manager from outside the web root entirely. The highest-risk scenario is a CI pipeline that copies `appsettings.Production.json` into the project root and a subsequent publish step that incorrectly includes it in `wwwroot`; adding a build-time check that validates web root contents before deploy catches this before it reaches production.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback returning `index.html` for unmatched paths
- Endpoint mapping order determining fallback behavior
- API 404s masked as 200 HTML responses
- Conditional fallback excluding `/api` prefix paths

**Answer**

`MapFallbackToFile("index.html")` matches any unresolved request, including `GET /api/unknown`, and returns the SPA shell with a 200 status. When this is registered before API endpoint mapping, legitimate API 404 responses become unexpected 200 HTML responses, which breaks JSON clients that check status codes, corrupts error handling in JavaScript fetch calls, and masks CORS errors as malformed HTML. The fix is to register API endpoints — `MapControllers()`, Minimal API groups — before `MapFallbackToFile`, so 404s from unmatched API paths propagate correctly rather than being swallowed by the fallback. If the SPA and API share the same host, a conditional fallback that excludes paths starting with `/api` is more explicit and resilient to future endpoint additions.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- `BackgroundService` singleton lifetime vs scoped service lifetime
- `IServiceScopeFactory` for creating per-job scopes
- `IAsyncDisposable` scope disposal pattern
- `ValidateScopes` detecting illegal constructor injection at startup

**Answer**

`BackgroundService` is a singleton — it lives for the application lifetime — so injecting scoped services like `DbContext` or repository classes directly into its constructor violates the DI scope rules. If `ValidateScopes` is enabled, the host throws at startup; if it isn't, the scoped service is created once and held indefinitely, causing EF change trackers to accumulate stale data and eventually throwing `ObjectDisposedException` when the original scope ends. The correct pattern is to inject `IServiceScopeFactory`, then inside each background iteration create `await using var scope = factory.CreateAsyncScope()`, resolve the scoped dependency from `scope.ServiceProvider`, do the work, and let the scope dispose at the end of the `await using` block. This same rule applies to timer callbacks and `Task.Run` loops started from any singleton.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR hub broadcast reaching only locally connected clients
- Redis backplane via `AddStackExchangeRedis` for cross-instance messaging
- Azure SignalR Service as a managed backplane alternative
- Sticky sessions retaining connection but not routing cross-node events
- Consistent channel prefix per application in the backplane config

**Answer**

SignalR hub methods that broadcast to all clients — `Clients.All.SendAsync(...)` — only reach clients connected to the same server instance. When multiple instances run behind a load balancer, a user on instance A never receives a message sent from instance B unless a backplane routes the event across all instances. Sticky sessions keep each client on the same node, which prevents disconnects, but they don't route server-side broadcasts across nodes — they're a connection stability measure, not a messaging solution. The fix is a Redis backplane via `AddSignalR().AddStackExchangeRedis(connectionString)` with a consistent channel prefix, or Azure SignalR Service, which manages the backplane entirely. Testing this requires at minimum two instances with load balancing before launch; single-node staging environments hide this problem completely.

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

**Concepts**
- `ApiController` (`System.Web.Http`) not existing in ASP.NET Core
- `HttpContext.Current` removed in favor of async request-scoped `HttpContext`
- Static repository pattern bypassing DI and test seams
- `ControllerBase` and `[ApiController]` as Core equivalents
- Constructor injection replacing static data access

**Answer**

This code does not compile on ASP.NET Core because `ApiController` is from `System.Web.Http`, which doesn't exist in the Core runtime, and `HttpContext.Current` was a thread-static on the Framework HTTP pipeline that has no equivalent in ASP.NET Core's async, non-thread-affined request model — even if the types were shimmed, `HttpContext.Current` would return null in async request flows. The controller should inherit `ControllerBase` with `[ApiController]` and `[Route("api/[controller]")]`, and the current user is available through `User.Identity.Name` on the controller base class, which ASP.NET Core populates from the authenticated principal. `OrderRepository.Find(id)` as a static call bypasses DI entirely, making the controller impossible to unit test and hiding its dependencies from the container; an `IOrderRepository` scoped service should be injected through the constructor instead. The action should return `ActionResult<OrderDto>` with explicit DTO mapping rather than an entity graph through `Json()`, which uses Framework-era serialization defaults.

---

#### Q2. (P) Your team deploys the same ASP.NET Core API to Linux containers behind nginx and to Windows with IIS. Explain who runs application code, who terminates TLS, and what stays the same in `Program.cs` across both targets.

**Concepts**
- Kestrel running `Program.cs` and middleware pipeline on all platforms
- nginx/IIS as TLS-terminating reverse proxies
- IIS in-process vs out-of-process and their Kestrel relationship
- `WebApplication.CreateBuilder` identical across deployment targets
- Environment-specific configuration via `ASPNETCORE_URLS` and `ForwardedHeaders`

**Answer**

Kestrel always runs the ASP.NET Core application code — `Program.cs`, the middleware pipeline, and endpoint handlers — on both Linux and Windows. On Linux behind nginx, Kestrel listens on an internal port while nginx handles TLS termination and forwards decrypted HTTP. On Windows behind IIS, the ASP.NET Core Module either loads the Core CLR into the IIS worker process (in-process) or forwards HTTP requests to a Kestrel subprocess (out-of-process) — in both cases the application code runs in ASP.NET Core, not in IIS's native pipeline. `Program.cs` stays identical across both targets because middleware registration, DI, and routing are the same; what differs is operational configuration delivered through environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), `UseForwardedHeaders` to read the correct scheme and client IP from the proxy, and how TLS certificates are provisioned. Health checks and structured logging should not hard-code Windows paths or assume IIS module availability.

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

**Concepts**
- Hard-coded `app.Run(url)` overriding `ASPNETCORE_URLS` environment variable
- Swagger enabled unconditionally vs gated to Development
- Missing `UseHttpsRedirection` for TLS-expected environments
- Missing health check endpoints for orchestrator probes
- Environment-aware pipeline configuration

**Answer**

`app.Run("http://0.0.0.0:5000")` overrides `ASPNETCORE_URLS` and any Kestrel configuration section, forcing the process to bind to a fixed address regardless of what the container orchestrator sets — when the orchestrator injects `ASPNETCORE_URLS=http://+:8080`, Kestrel ignores it and binds to 5000, so health probes that target 8080 time out and the pod is marked unhealthy. Swagger is registered unconditionally, so Staging and production expose the full API schema and Swagger UI to public clients since no environment check gates it. `UseHttpsRedirection` is missing, which means credentials and tokens may traverse plaintext if edge TLS misconfigures or is bypassed. There are no health check endpoints, so load balancers and readiness probes have no valid path to check. The fix is to remove `app.Run(url)` and let configuration control bindings, gate Swagger with `if (app.Environment.IsDevelopment())` or secure it with authentication in Staging, add `app.UseHttpsRedirection()` where TLS is expected, and register `MapHealthChecks("/health")`.

---

#### Q4. (M) A request hits `GET /api/orders/42` on a deployed ASP.NET Core app. Walk through the major stages from Kestrel accepting the socket through to the JSON response leaving the process — name the layers, not every middleware.

**Concepts**
- Kestrel TCP parsing and `HttpContext` creation with request-scoped DI
- Middleware pipeline with short-circuit capability
- Endpoint routing selecting controller action
- DI scope resolution and action execution
- `System.Text.Json` serialization and response flush

**Answer**

The request enters Kestrel, which accepts the TCP connection, parses the HTTP request line and headers, and creates an `HttpContext` with `Request`, `Response`, `Connection`, and a request-scoped `RequestServices` container linked to the application DI container. The middleware pipeline then runs in registration order — forwarded headers translate proxy-set scheme and IP, exception handling wraps the remainder for structured error responses, routing runs its pattern-matching logic and attaches `OrdersController.Get(42)` to `HttpContext.GetEndpoint()`, authentication validates the credential and populates `HttpContext.User`, and authorization evaluates the endpoint's `[Authorize]` metadata against the populated principal. If authorization passes, the controller is activated by DI with its scoped repository, `id = 42` is bound from the route, the action runs, and returns an `IActionResult` or `ActionResult<OrderDto>`. The result executor invokes `System.Text.Json` to serialize the DTO to the response body as `application/json`. After the handler returns, middleware that registered post-processing logic runs in reverse order — response compression, CORS headers — and Kestrel flushes the completed response through the connection back to the reverse proxy and to the client.

---

#### Q5. (D) Product wants a small internal tool: one POST endpoint, one GET endpoint, no MVC views, team knows C# well. When would you choose **Minimal APIs** vs **controllers**, and what would make you regret Minimal APIs six months later?

**Concepts**
- Minimal API suitability for small, cohesive endpoint surfaces
- Controller conventions at scale with `[ApiController]` and filters
- `MapGroup` and endpoint filters in Minimal APIs for production patterns
- Growth path risk with fat inline lambdas and duplicated validation
- Hybrid approach combining Minimal APIs and controllers in one host

**Answer**

For a small internal tool with one POST and one GET endpoint, Minimal APIs are the right starting point — two `app.Map*` calls with typed parameters and DI injection cost almost nothing to write and nothing to understand, whereas a controller class adds an activation step, attribute routing, and a constructor just to wrap two methods. The decision changes if the tool grows: once handlers accumulate validation logic, start sharing authorization policies across routes, need per-endpoint filters, or the team begins searching for a convention to group related code, the friction of Minimal APIs rises while the ceremony of controllers starts earning its keep. The specific triggers I'd watch for are validation duplicated across three or more routes, inline lambda bodies exceeding twenty lines, or testing requiring reflection over endpoint metadata instead of instantiating a controller class with a test DI container. A hybrid is valid and underused — Minimal APIs for health checks and internal ops endpoints, controllers for the growing public domain API, sharing one `WebApplication` host, authentication stack, and OpenAPI generator.

---

#### Q6. (R) A consultant claims "ASP.NET Core is just Kestrel — you don't need IIS or nginx." Review their deployment diagram assumptions. What production gaps appear when Kestrel is the only layer in front of your app?

**Concepts**
- Kestrel as application server vs full edge gateway
- TLS certificate rotation complexity per instance
- WAF, rate limiting, and IP filtering at the edge
- Load balancing and rolling deploy across Kestrel instances
- `ForwardedHeaders` necessity when a proxy is reintroduced

**Answer**

Kestrel runs ASP.NET Core correctly as the sole process, and for internal services or containers behind a cloud load balancer where the LB handles TLS, it is often fine. The gaps appear when Kestrel is the only layer facing the public internet: TLS certificate rotation must happen per instance rather than at one proxy, cipher negotiation policy is managed in each `appsettings.json` instead of one nginx config, and there is no WAF or rate limiting before requests reach the .NET process — malformed requests or abuse patterns hit application code rather than being dropped at the edge. Static-file serving works through `UseStaticFiles` but lacks the caching and compression efficiency of nginx for high-traffic assets. Load balancing across multiple Kestrel instances requires another mechanism — DNS, cloud LB, YARP — to replace what a reverse proxy provides for free. `ForwardedHeaders` becomes unnecessary if Kestrel is the edge since clients connect directly, but the moment a proxy is added, correct client IP and scheme require it. The consultant's claim is defensible for specific environments but generalizes poorly to externally-facing services with compliance or operational requirements.

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

**Concepts**
- Endpoint mapping before `UseRouting` causing 404 on all routes
- Auth middleware running before routing losing endpoint metadata
- `WebApplicationFactory<Program>` for integration test bootstrapping
- `public partial class Program` required for `WebApplicationFactory` access
- Pipeline order regression from copy-paste `Startup.Configure` migration

**Answer**

`MapControllers()` is called before `UseRouting()`, which means endpoints are registered in the endpoint table but route matching never runs because routing middleware was added after the endpoint registration call — every request returns 404. The auth middleware runs before routing, so it cannot inspect matched endpoint metadata and `[Authorize]` policies on controllers may not apply correctly, depending on the auth scheme. The integration tests boot through `new HostBuilder().ConfigureWebHost(b => b.UseStartup<Startup>())`, which constructs a completely different pipeline from what `Program.cs` defines, so tests exercise the old `Startup.cs` behavior and will not catch the 404 or auth regression introduced in the migration. The correct pipeline order is `UseRouting()`, then `UseAuthentication()`, then `UseAuthorization()`, then `MapControllers()`. For integration tests, the file should expose `public partial class Program { }` so `WebApplicationFactory<Program>` can boot the real pipeline. Any duplicate configuration providers carried over from the old `Startup` constructor should be removed in favor of `WebApplication.CreateBuilder(args)` defaults.

```csharp
var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

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

**Concepts**
- `UseUrls` overriding `ASPNETCORE_URLS` environment variable
- `localhost` binding unreachable from container probe networks
- `EXPOSE` in Dockerfile vs Kestrel listen address alignment
- Configuration-driven URLs vs developer-machine defaults in images

**Answer**

`builder.WebHost.UseUrls("http://localhost:5000")` overrides `ASPNETCORE_URLS` entirely — environment variables set by the orchestrator are ignored, so Kestrel binds to `localhost:5000` regardless of what the CI pipeline injects. Inside a Docker container, `localhost` refers to the container's loopback interface, which is unreachable from the Docker host, Kubernetes liveness probe network, or load balancer health checks, so `/health` probes time out. The Dockerfile `EXPOSE 8080` documents the expected port but has no effect on what Kestrel actually binds, creating a misleading discrepancy. On Windows CI agents, the port conflict between `localhost:5000` and an existing process can cause "address already in use" errors; on Linux the binding succeeds but the port is inaccessible from outside the container. The fix is to remove `UseUrls` entirely and let Kestrel read `ASPNETCORE_URLS` from the environment, which the CI pipeline sets to `http://+:8080` — the `+` meaning all interfaces including the one the probe reaches.

```csharp
var builder = WebApplication.CreateBuilder(args);
// URLs from env/config only — no UseUrls in container images
var app = builder.Build();
app.MapGet("/health", () => Results.Ok("healthy"));
app.Run();
```

---
