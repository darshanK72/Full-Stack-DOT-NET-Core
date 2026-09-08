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

## Gotchas — Introduction to ASP.NET Core (Interview Traps)

---

#### Gotcha 1. Kestrel is an application server, not a full edge gateway

**Concepts**
- Kestrel as cross-platform application web server
- Reverse proxy pattern with nginx, IIS, Azure Front Door
- TLS termination and WAF at the proxy layer
- `UseForwardedHeaders` for accurate client IP and scheme behind a proxy

**Answer**

Kestrel is ASP.NET Core's built-in web server and runs in every deployment, but it is not designed to be the sole public internet endpoint. Production deployments almost always place a reverse proxy — nginx, IIS, Azure Front Door, or an AWS ALB — in front to handle TLS certificate management, WAF protection, rate limiting, and edge caching. Kestrel binds to an internal port while the proxy terminates HTTPS and forwards HTTP internally. When running behind a proxy, `UseForwardedHeaders()` must be called early in the pipeline so `Request.Scheme` reads `https` and `Connection.RemoteIpAddress` reflects the real client IP rather than the proxy address.

---

#### Gotcha 2. `ASPNETCORE_ENVIRONMENT` is case-sensitive on Linux

**Concepts**
- Linux environment variable case sensitivity vs Windows insensitivity
- `IHostEnvironment.IsProduction()` matching exact string
- `appsettings.{Environment}.json` filename resolution by exact name
- Silent fallback to base settings when variable casing is wrong

**Answer**

`ASPNETCORE_ENVIRONMENT=production` on a Linux host does not match `"Production"` — `IHostEnvironment.IsProduction()` returns false and `appsettings.Production.json` is never loaded, so the application silently runs with base or Development settings. This is one of the most common causes of "works locally but behaves wrong in Docker" bugs. The value must exactly match the casing used in `appsettings.{Environment}.json` filenames and the `IsDevelopment()` / `IsProduction()` helper checks. Always validate the environment name as the very first diagnostic step when environment-specific settings are not applying in a deployed environment.

---

#### Gotcha 3. `builder.Build()` finalizes the DI container — services cannot be added after it

**Concepts**
- `builder.Services` for registration before `Build()`
- `app.Services` for resolution only, not registration
- `InvalidOperationException` thrown when registering after build
- Two-phase builder pattern in the minimal hosting model

**Answer**

The `WebApplicationBuilder` separates configuration into two phases: registration on `builder.Services` before calling `builder.Build()`, and resolution via `app.Services` after build. Once `Build()` is called, the DI container is compiled and sealed — any attempt to register new services throws `InvalidOperationException` or is silently ignored. Teams migrating from ASP.NET Framework sometimes try to add services inside middleware lambdas or late-running extension methods, but those registrations happen too late. All service registrations must occur before `builder.Build()` is called in `Program.cs`.

---

#### Gotcha 4. `HttpContext.Current` does not exist in ASP.NET Core

**Concepts**
- `HttpContext.Current` removed — thread-static incompatible with async/await
- `IHttpContextAccessor` as the correct replacement
- `AsyncLocal<T>` flowing context through awaited continuations
- Null `HttpContext` outside of an active HTTP request scope

**Answer**

`HttpContext.Current` was a thread-static field in ASP.NET Framework that broke under `async/await` because continuations run on any thread. ASP.NET Core eliminated it entirely. The correct replacement is injecting `IHttpContextAccessor` and reading `.HttpContext`. `IHttpContextAccessor` uses `AsyncLocal<T>` internally, which flows correctly through awaited continuations. Outside an active HTTP request — in background services, console hosts, or test harnesses — `HttpContextAccessor.HttpContext` returns `null`. Code that accesses `HttpContext` must be null-checked or restructured to accept it as a method parameter rather than reaching for it globally.

---

#### Gotcha 5. `UseHttpsRedirection` causes redirect loops behind a TLS-terminating proxy

**Concepts**
- HTTPS redirection checking `Request.Scheme` to decide when to redirect
- Proxy terminating TLS — Kestrel sees `http` internally
- `UseForwardedHeaders` must precede `UseHttpsRedirection`
- Disabling `UseHttpsRedirection` inside containers where ingress handles TLS

**Answer**

`UseHttpsRedirection` redirects any request where `Request.Scheme` is `http` to the HTTPS equivalent. Behind a TLS-terminating proxy, Kestrel receives plain HTTP internally so `Request.Scheme` is always `http`, causing every request to be redirected in an infinite loop. The fix is to call `UseForwardedHeaders()` before `UseHttpsRedirection()` so the scheme is populated from `X-Forwarded-Proto` before the redirect check runs. In Kubernetes or Azure Container Apps where TLS is handled at the ingress layer, it is often correct to disable `UseHttpsRedirection` entirely inside the pod and let the ingress controller enforce HTTPS.

---

#### Gotcha 6. Code placed after `app.Run()` is unreachable during normal execution

**Concepts**
- `app.Run()` blocking until application shutdown
- `app.RunAsync()` for non-blocking host composition
- `IHostApplicationLifetime` for shutdown callbacks
- Startup vs post-shutdown cleanup patterns

**Answer**

`app.Run()` starts Kestrel and blocks the calling thread until the application shuts down. Any code placed after `app.Run()` in `Program.cs` is unreachable during normal operation and executes only after shutdown — which is almost never the intended behavior. Developers who want to run code at shutdown should register `IHostApplicationLifetime.ApplicationStopping` or `ApplicationStopped` callbacks before the `Run()` call. `app.RunAsync()` is available when composing multiple hosts, but in the standard single-app minimal hosting pattern `app.Run()` is both correct and idiomatic.

---

#### Gotcha 7. `IWebHostEnvironment` vs `IHostEnvironment` — `WebRootPath` is only on the web variant

**Concepts**
- `IHostEnvironment` — base interface with `ContentRootPath`
- `IWebHostEnvironment` — adds `WebRootPath` for the `wwwroot` folder
- Injecting the wrong interface compiles but loses `WebRootPath`
- `ContentRootPath` for non-web assets vs `WebRootPath` for served files

**Answer**

`IHostEnvironment` exposes `EnvironmentName`, `ApplicationName`, and `ContentRootPath` and is available in any hosted application. `IWebHostEnvironment` extends it with `WebRootPath`, the path to the `wwwroot` folder where static files are served from. Middleware or services that need to read or serve files from `wwwroot` must inject `IWebHostEnvironment`, not `IHostEnvironment` — injecting the base interface compiles successfully but `WebRootPath` is unavailable. This distinction matters when building middleware that reads static assets or configuration files from the web root at runtime.

---

#### Gotcha 8. Static file paths are case-sensitive on Linux — `Logo.png` is not `logo.png`

**Concepts**
- Linux file system case sensitivity vs Windows case insensitivity
- Static file URL casing must match file system casing on Linux
- Docker image builds copying files with mismatched casing
- Standardizing file names to lowercase as a prevention strategy

**Answer**

On Windows, `wwwroot/images/Logo.png` and `wwwroot/images/logo.png` refer to the same file. On Linux — including Docker containers — they are distinct paths. A URL requesting `/images/logo.png` returns 404 when the file is named `Logo.png`. Applications developed on Windows and deployed to Linux containers regularly hit this problem in CI after working fine on developer machines. The fix is to standardize all static file names and HTML references to lowercase and enforce consistent casing in the CI pipeline, since the Windows development environment hides the problem entirely.

---

#### Gotcha 9. Partial migration to minimal hosting — `Startup.cs` methods are never called

**Concepts**
- Top-level statements replacing `Startup.cs` in minimal hosting model
- `ConfigureServices` and `Configure` not called without `UseStartup<T>()`
- Silent DI registration failure — services missing at runtime
- Full migration vs `builder.Host.UseStartup<Startup>()` bridge

**Answer**

The minimal hosting model uses top-level statements in `Program.cs` and eliminates `Startup.cs`. Projects that partially migrate — using `WebApplicationBuilder` while keeping a `Startup` class — find that `ConfigureServices` and `Configure` are never called, because the minimal model does not look for them automatically. DI registrations in `Startup.ConfigureServices` are silently skipped, leading to `InvalidOperationException` when services are resolved. Either migrate fully to the minimal model, or use `builder.Host.UseStartup<Startup>()` to keep the conventional pattern — mixing the two without that bridge does not work.

---

#### Gotcha 10. Default `WebApplication.CreateBuilder` loads `appsettings.json` automatically — duplicate registration causes double loading

**Concepts**
- `WebApplication.CreateBuilder` loading `appsettings.json` and `appsettings.{env}.json` by default
- Manually adding JSON providers duplicating configuration sources
- `IConfiguration.GetSection` returning merged values from all providers
- `builder.Configuration.Sources.Clear()` to reset default sources

**Answer**

`WebApplication.CreateBuilder` automatically registers `appsettings.json`, `appsettings.{EnvironmentName}.json`, environment variables, and command-line arguments as configuration sources. Developers who manually call `builder.Configuration.AddJsonFile("appsettings.json")` in `Program.cs` are adding a duplicate source — the file is read twice, which is harmless but confusing in diagnostics. If a project needs to reset all configuration sources and start from scratch, call `builder.Configuration.Sources.Clear()` before adding custom providers. The default sources are almost always appropriate; adding custom JSON files should be done for supplementary files, not as a replacement for the defaults.

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
