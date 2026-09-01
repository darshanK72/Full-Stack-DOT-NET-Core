# Hosting, Kestrel & Environments — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Kestrel?](#q1-what-is-kestrel)
2. [Q2. What is the role of IIS in hosting ASP.NET Core on Windows?](#q2-what-is-the-role-of-iis-in-hosting-aspnet-core-on-windows)
3. [Q3. What is a reverse proxy, and why use one with ASP.NET Core?](#q3-what-is-a-reverse-proxy-and-why-use-one-with-aspnet-core)
4. [Q4. Who terminates TLS in a typical nginx + Kestrel deployment?](#q4-who-terminates-tls-in-a-typical-nginx-kestrel-deployment)
5. [Q5. What is `ASPNETCORE_ENVIRONMENT`?](#q5-what-is-aspnetcore_environment)
6. [Q6. What is `IHostEnvironment` / `IWebHostEnvironment`?](#q6-what-is-ihostenvironment-iwebhostenvironment)
7. [Q7. How does the environment name affect application behavior?](#q7-how-does-the-environment-name-affect-application-behavior)
8. [Q8. What is `ASPNETCORE_URLS`?](#q8-what-is-aspnetcore_urls)
9. [Q9. How do you configure Kestrel to listen on a specific port?](#q9-how-do-you-configure-kestrel-to-listen-on-a-specific-port)
10. [Q10. What is the difference between Kestrel endpoint configuration and IIS bindings?](#q10-what-is-the-difference-between-kestrel-endpoint-configuration-and-iis-bindings)
11. [Q11. What are forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`)?](#q11-what-are-forwarded-headers-x-forwarded-for-x-forwarded-proto)
12. [Q12. What does `UseForwardedHeaders()` do, and why must it run early?](#q12-what-does-useforwardedheaders-do-and-why-must-it-run-early)
13. [Q13. What is the difference between in-process and out-of-process IIS hosting?](#q13-what-is-the-difference-between-in-process-and-out-of-process-iis-hosting)
14. [Q14. What are health checks in ASP.NET Core?](#q14-what-are-health-checks-in-aspnet-core)
15. [Q15. What is the difference between a liveness probe and a readiness probe?](#q15-what-is-the-difference-between-a-liveness-probe-and-a-readiness-probe)
16. [Q16. How do container `EXPOSE` directives relate to Kestrel listening ports?](#q16-how-do-container-expose-directives-relate-to-kestrel-listening-ports)
17. [Q17. What breaks if Production is accidentally set to Development?](#q17-what-breaks-if-production-is-accidentally-set-to-development)
18. [Q18. What belongs in `appsettings.Development.json` vs environment variables in Production?](#q18-what-belongs-in-appsettingsdevelopmentjson-vs-environment-variables-in-production)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is Kestrel?

**Concepts**
- Cross-platform web server built into ASP.NET Core
- Async I/O and .NET thread pool integration
- Production-grade application server not a full edge gateway
- Reverse proxy commonly placed in front for TLS and edge policies

**Answer**

Kestrel is the cross-platform, high-performance web server built into ASP.NET Core that listens for HTTP/HTTPS connections and drives the request pipeline. It runs on Windows, Linux, and macOS — the default server for `dotnet run`, containers, and cloud deployments — and handles connection management, HTTP parsing, TLS (when configured), and passes requests to middleware and endpoints. Kestrel is optimized for async I/O and integrates tightly with the .NET thread pool and DI container. Production deployments often place a reverse proxy in front of Kestrel for TLS termination, WAF policies, and load balancing, since Kestrel is an excellent application server but not a full edge gateway.

---

## Q2. What is the role of IIS in hosting ASP.NET Core on Windows?

**Concepts**
- ASP.NET Core Module (ANCM) as the bridge
- In-process vs out-of-process hosting modes
- Windows-specific features — app pool recycling, Windows Auth
- IIS as reverse proxy not a replacement for Kestrel

**Answer**

IIS acts as a reverse proxy and process manager for ASP.NET Core on Windows via the ASP.NET Core Module (ANCM) — handling Windows integration, certificate binding, and worker process lifecycle while Kestrel executes application code. In in-process mode, the app runs inside the IIS worker process (`w3wp.exe`) for lowest latency and shared Windows authentication. In out-of-process mode, IIS forwards requests to a separate Kestrel `dotnet` process for better crash isolation. IIS provides app pool recycling, request filtering, centralized logging, and familiar Windows admin tooling. IIS does not replace Kestrel — it either forwards to or hosts the Kestrel-based ASP.NET Core runtime.

---

## Q3. What is a reverse proxy, and why use one with ASP.NET Core?

**Concepts**
- Reverse proxy handling TLS, load balancing, and edge policies
- Kestrel focused on running .NET efficiently behind the proxy
- Zero-downtime deployments and SSL certificate centralization
- Common examples — nginx, IIS, YARP, Azure Front Door, AWS ALB

**Answer**

A reverse proxy sits in front of Kestrel, receives client traffic, and forwards it to backend app instances — handling TLS, load balancing, compression, caching, WAF, and rate limiting at the edge. Kestrel focuses on running .NET efficiently while the proxy handles infrastructure concerns at scale. Proxies enable zero-downtime deployments, multiple instances, and SSL certificate centralization without requiring each Kestrel instance to manage certificates. Without a proxy, Kestrel must handle all edge concerns directly — workable but less common in production deployments where nginx, IIS, YARP, Azure Front Door, or AWS ALB commonly sit in front.

---

## Q4. Who terminates TLS in a typical nginx + Kestrel deployment?

**Concepts**
- nginx terminating TLS at the edge
- Kestrel receiving plain HTTP on an internal port
- X-Forwarded-Proto telling Kestrel the original scheme
- Certificate management and rotation at the edge layer

**Answer**

nginx terminates TLS at the edge — clients connect via HTTPS to nginx, which decrypts traffic and forwards plain HTTP to Kestrel on an internal port such as 5000 or 8080. SSL certificates are installed on nginx rather than on each Kestrel instance, which simplifies certificate rotation and cipher policy management. Kestrel may still be configured for HTTPS internally for service-to-service scenarios, but the common pattern is plain HTTP behind the proxy on a trusted internal network. `X-Forwarded-Proto: https` tells Kestrel the original client scheme so that `Request.IsHttps` returns `true` inside the app, which is required for HTTPS redirects and secure cookie flags to behave correctly.

---

## Q5. What is `ASPNETCORE_ENVIRONMENT`?

**Concepts**
- Environment variable setting the hosting environment name
- Controls appsettings.{Environment}.json merging
- Gates conditional startup logic — developer exception page, Swagger
- Defaults to Production when unset in many hosts

**Answer**

`ASPNETCORE_ENVIRONMENT` is an environment variable that sets the hosting environment name — `Development`, `Staging`, `Production`, or any custom name — consumed at startup by the generic host. It is read via `IHostEnvironment.EnvironmentName` in application code and controls which `appsettings.{Environment}.json` file merges into configuration at startup. It also gates conditional startup logic such as the developer exception page, Swagger UI, verbose logging, and EF seed data. The value should be set explicitly in production deployments — it defaults to `Production` when unset in many hosts, but relying on that default rather than setting it explicitly is ambiguous.

---

## Q6. What is `IHostEnvironment` / `IWebHostEnvironment`?

**Concepts**
- DI abstractions for environment name, content root, and web root
- IsDevelopment(), IsStaging(), IsProduction() convenience methods
- ContentRootPath vs WebRootPath
- Prefer interface injection over raw environment variable access

**Answer**

These DI-injected abstractions expose hosting context — environment name, content root path, application name, and (for web apps) web root path — without reading environment variables directly. `IHostEnvironment` is available in any .NET host; `IWebHostEnvironment` adds `WebRootPath` for web apps. `IsDevelopment()`, `IsStaging()`, and `IsProduction()` are convenience methods against `EnvironmentName`. `ContentRootPath` is the application base directory where `appsettings.json` lives; `WebRootPath` points to `wwwroot`. Prefer injecting these interfaces over hard-coding environment checks against raw strings for testability and clarity.

---

## Q7. How does the environment name affect application behavior?

**Concepts**
- appsettings.{Environment}.json configuration override
- Conditional middleware — Swagger and developer exception page
- ValidateOnStart and EF seed data per environment
- Production disabling diagnostic endpoints and minimizing log noise

**Answer**

The environment name selects configuration files, logging verbosity, middleware branches, and feature toggles registered conditionally at startup. `appsettings.Development.json` overrides base settings when `EnvironmentName` is `Development`, and templates enable Swagger, developer exception pages, and detailed errors only in that environment. `ValidateOnStart` for service validation, EF migrations, and seed data may run differently per environment — seed data often runs only in Development or Staging. Production should disable diagnostic endpoints, minimize log noise, and enforce strict exception handling, since those behaviors have security and performance costs that are unacceptable in a public-facing deployment.

---

## Q8. What is `ASPNETCORE_URLS`?

**Concepts**
- Semicolon-separated listen addresses for Kestrel
- Overrides launchSettings.json when set in environment
- Common container pattern — http://+:8080
- Distinct from reverse proxy external ports

**Answer**

`ASPNETCORE_URLS` is an environment variable that sets the addresses Kestrel listens on, using semicolon-separated URLs such as `http://0.0.0.0:8080` or `https://localhost:5001;http://localhost:5000`. It overrides default `launchSettings.json` URLs when set, which makes it the correct mechanism for Docker and Kubernetes where `launchSettings.json` is never loaded. In containers, it is typically set to `http://+:8080` to bind all interfaces on port 8080. This is distinct from reverse proxy external ports — `ASPNETCORE_URLS` controls Kestrel's bind addresses inside the container or process, not what the ingress or load balancer exposes externally.

---

## Q9. How do you configure Kestrel to listen on a specific port?

**Concepts**
- ASPNETCORE_URLS environment variable for container deployments
- Kestrel:Endpoints in appsettings.json for configuration-driven binding
- ConfigureKestrel() for programmatic control including HTTPS certificates
- Docker EXPOSE documenting the port Kestrel must actually bind

**Answer**

Configure Kestrel's listening port via `ASPNETCORE_URLS`, `launchSettings.json` (development only), `appsettings.json` under `Kestrel:Endpoints`, or programmatic `builder.WebHost.ConfigureKestrel()`. In containers, `ASPNETCORE_URLS=http://0.0.0.0:5000` is the most common approach. In `appsettings.json`: `"Kestrel": { "Endpoints": { "Http": { "Url": "http://localhost:5050" } } }`. For explicit control including HTTPS certificate binding, use `builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(8080))`. Docker maps container ports via `EXPOSE` and `docker run -p`, but Kestrel must listen on the container's internal port — `EXPOSE` alone does not configure the listener.

---

## Q10. What is the difference between Kestrel endpoint configuration and IIS bindings?

**Concepts**
- Kestrel configuration — which addresses the .NET process listens on
- IIS bindings — which URLs IIS accepts and forwards to Kestrel
- Misaligned ports causing 502.5 process startup failures
- In-process hosting — IIS bindings authoritative for incoming traffic

**Answer**

Kestrel endpoint configuration — `ASPNETCORE_URLS`, `Kestrel:Endpoints`, `ConfigureKestrel` — defines which addresses the .NET process binds to. IIS bindings define which URLs IIS accepts at the machine level and how it forwards to Kestrel. In out-of-process hosting, IIS receives external traffic on its configured hostname and port, then forwards to a localhost port configured by ANCM. In in-process hosting, the app runs inside IIS worker and IIS bindings are authoritative for incoming traffic. Misaligned ports between the IIS forwarding target and Kestrel's listening address cause 502.5 process startup failures, since IIS cannot connect to the backend.

---

## Q11. What are forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`)?

**Concepts**
- X-Forwarded-For — original client IP address chain
- X-Forwarded-Proto — original connection scheme (http/https)
- X-Forwarded-Host — original Host header from client
- Required because Kestrel only sees the proxy connection

**Answer**

Forwarded headers are HTTP headers set by reverse proxies to pass the original client IP, scheme, and host to the backend application, which otherwise sees only the proxy's connection details. `X-Forwarded-For` carries the original client IP address chain; `X-Forwarded-Proto` carries the original scheme (`http` or `https`); `X-Forwarded-Host` carries the original Host header from the client request. Without them, `HttpContext.Connection.RemoteIpAddress` is the proxy IP and `Request.Scheme` is always `http` even for HTTPS clients — breaking HTTPS redirects, cookie secure flags, link generation, and audit logs.

---

## Q12. What does `UseForwardedHeaders()` do, and why must it run early?

**Concepts**
- Reads X-Forwarded-* and updates HttpContext.Connection and Request
- Must run before HTTPS redirection, authentication, and link generation
- ForwardedHeadersOptions.KnownProxies for trusted network restriction
- Late placement causing wrong scheme and IP downstream

**Answer**

`UseForwardedHeaders()` reads `X-Forwarded-*` headers from trusted proxies and updates `HttpContext.Connection.RemoteIpAddress`, `Request.Scheme`, and `Request.Host` before downstream middleware uses them. It must run as the first middleware after `Build()` — before HTTPS redirection, authentication, link generation, and rate limiting by IP — because all those downstream components read the properties it corrects. Configure `ForwardedHeadersOptions.KnownProxies` or `KnownNetworks` so untrusted clients cannot spoof headers by sending their own `X-Forwarded-*` values. Late placement causes auth cookies, redirects, `CreatedAtAction` URLs, and audit logs to use the wrong scheme or the proxy IP instead of the real client address.

---

## Q13. What is the difference between in-process and out-of-process IIS hosting?

**Concepts**
- In-process — app in IIS worker process w3wp.exe, lowest latency
- Out-of-process — Kestrel in separate dotnet process, crash isolation
- AspNetCoreHostingModel project property controlling the mode
- Containers and Linux always use Kestrel directly

**Answer**

In-process runs the ASP.NET Core app inside the IIS worker process (`w3wp.exe`) via the ASP.NET Core Module — this provides the lowest latency and access to the shared Windows authentication pipeline, making it the preferred mode for traditional Windows deployments. Out-of-process runs Kestrel in a separate `dotnet` process with IIS proxying requests to it — a crash in the application does not necessarily recycle the IIS worker, providing better crash isolation. Both modes use ANCM; the hosting model is set in `.csproj` via `<AspNetCoreHostingModel>InProcess|OutOfProcess</AspNetCoreHostingModel>`. Containers and Linux always use Kestrel directly, since IIS hosting is Windows-specific.

---

## Q14. What are health checks in ASP.NET Core?

**Concepts**
- Registered endpoints reporting Healthy/Degraded/Unhealthy
- AddHealthChecks() and MapHealthChecks() wiring
- Kubernetes liveness and readiness probes consuming them
- Separate checks for process liveness vs dependency readiness

**Answer**

Health checks are registered endpoints that report application and dependency readiness — database connectivity, disk space, downstream API reachability — for orchestrators and load balancers to make routing decisions. Register with `builder.Services.AddHealthChecks()` and map via `app.MapHealthChecks("/health")`. They return `Healthy`, `Degraded`, or `Unhealthy` with optional detailed JSON via `UIResponseWriter` or custom writers. Kubernetes uses them for liveness and readiness probes; Azure App Service uses them for load balancer routing decisions. Since the concerns differ, separate checks for "is the process alive" and "can the app serve traffic" should map to different endpoints corresponding to liveness versus readiness probes.

---

## Q15. What is the difference between a liveness probe and a readiness probe?

**Concepts**
- Liveness — process alive check, failure triggers pod restart
- Readiness — dependency check, failure removes pod from load balancer
- Liveness must stay lightweight to avoid false restarts
- Kubernetes probe paths and tag-based filtering

**Answer**

A liveness probe asks whether the process is alive and should be restarted if failing; a readiness probe asks whether the instance is ready to receive traffic and should be removed from the load balancer if failing — without restarting. The critical distinction is consequence: a failing liveness probe triggers a pod restart, while a failing readiness probe only stops new traffic from routing to that pod. Liveness must be a lightweight self-check — a simple HTTP 200 — because including slow dependency checks causes unnecessary pod restarts during transient database blips. Readiness should include dependency checks such as database connectivity and migration completion, so traffic stops flowing to an instance that cannot actually handle requests.

---

## Q16. How do container `EXPOSE` directives relate to Kestrel listening ports?

**Concepts**
- EXPOSE documents the port — does not open or configure it
- ASPNETCORE_URLS must match the exposed port
- Kubernetes containerPort must align with Kestrel bind port
- Kestrel binding 0.0.0.0 inside containers not localhost only

**Answer**

`EXPOSE` in a Dockerfile documents which port the container listens on but does not actually open or configure the listener — Kestrel must bind to that port via `ASPNETCORE_URLS` for connections to be accepted. A mismatch between `ASPNETCORE_URLS` and the port mapped in `docker run -p` or the Kubernetes Service's `targetPort` causes connection refused. Kestrel should bind `0.0.0.0` (or `+`) inside containers rather than `localhost` only, since probes and inter-container traffic come from outside the loopback. Kubernetes `containerPort` on the pod spec must match Kestrel's bind port, and the Service `targetPort` maps the external service port to that container port.

---

## Q17. What breaks if Production is accidentally set to Development?

**Concepts**
- Developer exception pages exposing stack traces publicly
- Swagger UI potentially enabled for external clients
- Development configuration overrides merging into runtime
- Compliance risks from verbose logging and relaxed CORS

**Answer**

The app exposes developer exception pages with stack traces, exception types, and internal file paths to any client that triggers an error — a direct information disclosure vulnerability. Swagger UI may be enabled publicly depending on conditional middleware, allowing API exploration by unauthorized parties. `appsettings.Development.json` overrides merge into runtime settings, which may point to wrong databases, disable authentication features, or change CORS rules. Verbose logging generates noise that spikes monitoring alerts, and compliance audits flag exposed diagnostic endpoints. The entire surface is controlled by a single environment variable, which is why `ASPNETCORE_ENVIRONMENT=Production` must be a mandatory go-live checklist item.

---

## Q18. What belongs in `appsettings.Development.json` vs environment variables in Production?

**Concepts**
- appsettings.Development.json for local-only non-secret settings
- Environment variables and secret managers for production secrets
- Source control safety — development file committed, production secrets not
- Non-secret production tuning in appsettings.Production.json

**Answer**

`appsettings.Development.json` holds local-only settings safe for developer machines and source control — detailed logging levels, localhost connection strings, Swagger toggles, `EnableSensitiveDataLogging`, and seed flags. Production secrets and environment-specific sensitive values such as connection strings, API keys, and certificates belong in environment variables, Azure Key Vault, AWS Secrets Manager, or Kubernetes secrets — never in committed JSON files. Non-secret production tuning such as log levels and feature flags can use `appsettings.Production.json` deployed without secrets, but values with higher precedence via environment variables override them at runtime. Never commit production secrets to source control — inject them at deploy time through the hosting platform.

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- UseRouting must precede UseAuthentication and UseAuthorization
- Endpoint metadata not selected before routing runs
- Recommended pipeline order for ASP.NET Core 8

**Answer**

In ASP.NET Core endpoint routing, `UseAuthentication` and `UseAuthorization` must run after `UseRouting` so the auth middleware can read endpoint metadata — if auth runs before routing, the endpoint has not been selected yet and policy resolution for `[Authorize]` and `RequireAuthorization()` cannot inspect the correct attributes. The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints. Symptoms of wrong order include anonymous access to protected endpoints and 401 challenges that fire without correctly applying per-endpoint allow-anonymous overrides.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive dependency lifetime violation
- EF DbContext stale change tracker accumulation
- ValidateScopes detecting the problem at startup
- IServiceScopeFactory as the correct fix

**Answer**

A scoped service injected into a singleton is held for the entire application lifetime, long after the scope that created it was disposed. The most common case is `DbContext`: the change tracker accumulates entities from unrelated requests, and after the scope is torn down any access throws `ObjectDisposedException`. Enable `ValidateScopes = true` in Development and staging to catch these combinations at startup rather than under production load. The fix is to inject `IServiceScopeFactory` and create a scope per unit of work, or use `IDbContextFactory<T>` to get a short-lived context per operation.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- HttpMessageHandler lifetime and socket exhaustion
- IHttpClientFactory managed handler recycling
- Named and typed client registration pattern

**Answer**

Instantiating `HttpClient` with `new` in a long-lived singleton prevents socket reuse because each instance holds its own `HttpMessageHandler` and the underlying TCP connections are not returned to a pool until garbage collection. Under load this causes socket exhaustion — `SocketException` and timeout errors that do not appear in local testing with low concurrency. `IHttpClientFactory` manages handler lifetimes and recycles connections correctly, so the fix is to register named or typed clients via `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()` and inject them rather than constructing `HttpClient` directly.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- IOptions<T> frozen snapshot at first resolution
- IOptionsSnapshot<T> recalculates per request scope
- IOptionsMonitor<T> live change notifications for singletons
- Silent staleness until process restart

**Answer**

`IOptions<T>` resolves once and caches the configuration snapshot for the service's lifetime, so a singleton that reads `.Value` in its constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled. `IOptionsSnapshot<T>` recalculates per request scope but is only usable in scoped services. `IOptionsMonitor<T>` supports change notifications via `OnChange` and works correctly in singletons. The failure mode is silent — misconfiguration persists until process restart because `.Value` was captured at construction.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET semantics and safe/idempotent URL parameters
- Proxies and caches stripping GET request bodies
- [FromQuery] with [AsParameters] for complex filter criteria
- Silent failures in CDN and proxy layers

**Answer**

`[FromBody]` on a GET endpoint is an anti-pattern because HTTP GET is defined as safe and idempotent with parameters in the URL — many clients, CDNs, and caching proxies strip or ignore request bodies on GET requests, so binding fails silently in production while "Try it out" in Swagger may appear to work. Use `[FromQuery]` with separate parameter names or `[AsParameters]` on a record type to aggregate complex filter criteria into a single clean parameter object.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- JsonNamingPolicy.CamelCase as ASP.NET Core default
- Silent binding producing default values instead of errors
- PropertyNameCaseInsensitive as a mitigation
- Validation attributes turning silent failure into 400 responses

**Answer**

ASP.NET Core Web API serializes JSON with `JsonNamingPolicy.CamelCase` by default, which means incoming JSON with PascalCase keys like `"CustomerName"` does not match the property — the model binds successfully but properties silently hold default values (null, zero, false). The preferred fix is standardizing all clients on camelCase and enforcing it through OpenAPI contracts. As a mitigation, `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` relaxes matching. Add required validation attributes so silent binding failures produce 400 responses rather than corrupt data silently stored to the database.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- throw; preserving original stack trace
- throw ex; resetting stack trace to the catch site
- InnerException preservation when intentionally wrapping
- APM and structured logging dependency on accurate stack traces

**Answer**

Rethrowing with `throw ex` resets the stack trace to the catch block line, which means Application Insights, Serilog, and `IExceptionHandler` all point at the handler rather than the code that actually failed. Bare `throw;` preserves the full original stack trace. Use `throw;` when logging and delegating upward; wrap with a new exception type only when adding context — `throw new OrderProcessingException("...", ex)` — so the original failure is preserved in `InnerException`. This rule applies identically in async code after `await`.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel as application server vs edge gateway
- TLS termination and certificate management at the reverse proxy
- WAF, rate limiting, and static file caching at the edge
- UseForwardedHeaders required for client IP logging

**Answer**

Kestrel is a production-grade application server optimized for running .NET efficiently, but directly exposing it to the internet skips TLS certificate centralization, WAF filtering, centralized rate limiting, and efficient static-file caching that reverse proxies handle. nginx, IIS, Azure Front Door, or AWS ALB typically sit in front so certificates are managed at the proxy layer with automatic renewal. If Kestrel is exposed directly, client IP logging requires `UseForwardedHeaders` configuration, and containers typically bind Kestrel to an internal port while the ingress controller handles external HTTPS.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- launchSettings.json applies only to dotnet run and IDE launch
- ASPNETCORE_URLS and ASPNETCORE_ENVIRONMENT as production env vars
- appsettings.Production.json for non-secret production tuning

**Answer**

`Properties/launchSettings.json` contains URLs, environment variables, and launch profiles that are read only by `dotnet run`, Visual Studio, and VS Code — the file is not deployed to production hosts and has no effect on them. Relying on it for environment name or URL configuration leads to wrong `ASPNETCORE_ENVIRONMENT` or binding address in deployed environments. Production URLs and environment come from host-level environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- default(false) for missing JSON field
- Nullable bool? for tri-state intent
- PATCH semantics requiring omitted-vs-false distinction
- Update DTO design for partial updates

**Answer**

A non-nullable `bool` property in a PATCH DTO cannot distinguish "field omitted from JSON" from "explicitly set to false" because `System.Text.Json` deserializes missing properties to `default(false)`, which corrupts partial-update semantics — a client updating only an email address accidentally resets a consent flag to false. PATCH endpoints need `bool?`, separate update DTOs that only include fields being modified, or tri-state enums like `Unspecified | OptIn | OptOut` to represent intent explicitly. Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host headers
- ForwardedHeadersOptions.KnownProxies for trusted network restriction
- Pipeline position — must run before HTTPS redirection and auth
- Header spoofing risk when trusting all proxies

**Answer**

Without `UseForwardedHeaders()` configured with known proxy IPs, `HttpContext.Request.Scheme` stays `http` even when clients used HTTPS, `Request.Host` reflects the internal address, and the client IP is the proxy — breaking HTTPS redirects, secure cookie flags, and audit logs. Call `UseForwardedHeaders()` as early as possible, before HTTPS redirection, authentication, link generation, and rate limiting by IP. Configure `ForwardedHeadersOptions` to trust only your specific reverse proxy network rather than all proxies, since trusting all enables header spoofing by any client.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- UseStaticFiles() serving without authentication
- wwwroot as a public CDN root
- Secrets management via environment variables and Key Vault
- Build pipeline verification of publish output

**Answer**

Every file in `wwwroot` is served to unauthenticated anonymous clients by `UseStaticFiles()` — there is no authentication gate by default. Placing `.env` files, `appsettings.Production.json`, private keys, or backup configs there makes them directly downloadable via their URL path. Only public assets such as CSS, JavaScript, images, and public PDFs belong in `wwwroot`. Sensitive configuration must live in environment variables, Azure Key Vault, or similar secret managers, and build pipelines should verify that publish output does not include secrets in the web root.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback order relative to API endpoint mapping
- /api/* returning index.html with HTTP 200 as a silent failure
- Endpoint-first ordering in Program.cs

**Answer**

Registering `MapFallbackToFile("index.html")` before API endpoint mapping causes any unmatched API route — including valid 404s — to return `index.html` with HTTP 200, which breaks JSON parsers on clients and masks the real failure. The correct order is to map API routes with `MapControllers()` or `MapGroup("/api")` first, then static files, then the SPA fallback last. Symptoms include CORS errors appearing as HTML responses and Swagger fetch failures in production SPA hosting.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- BackgroundService singleton lifetime
- Scoped service constructor injection causing disposal errors
- IServiceScopeFactory.CreateAsyncScope() per background job
- ValidateScopes detecting this at startup

**Answer**

A singleton `BackgroundService` cannot constructor-inject scoped services like `DbContext` because hosted services live for the application lifetime while scoped instances are disposed after their first scope ends, causing `ObjectDisposedException` or scope validation errors at startup. The fix is to inject `IServiceScopeFactory`, then inside each background job call `await using var scope = factory.CreateAsyncScope()`, resolve the scoped service from `scope.ServiceProvider`, and dispose the scope when the job finishes. Enable `ValidateScopes` in Development to catch this before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR broadcast scope — single server instance only
- Redis or Azure Service Bus backplane for multi-instance routing
- Sticky sessions vs backplane trade-offs
- Azure SignalR Service as a managed alternative

**Answer**

SignalR tracks connected clients per server instance, so a broadcast from one instance reaches only the clients connected to that instance. With multiple instances behind a load balancer, users on different nodes never receive events raised on other nodes — a critical failure for real-time chat or notifications. Sticky sessions keep one client on one node but do not route server-side events across nodes. The solution is a Redis or Azure Service Bus backplane registered with `AddSignalR().AddStackExchangeRedis(...)`, or the managed Azure SignalR Service. Test scale-out with at least two instances before launch.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) Explain the roles of Kestrel, IIS, and a reverse proxy (nginx, YARP, Azure Front Door) in an ASP.NET Core deployment. Who terminates TLS, who runs your application code, and why do you often use more than one layer?

**Concepts**
- Kestrel as the .NET application runtime host
- IIS in-process vs out-of-process via ANCM
- Reverse proxy terminating TLS and handling edge policies
- Multiple layers solving different operational problems

**Answer**

Kestrel is the cross-platform web server that runs ASP.NET Core application code — controllers, middleware, and DI all execute inside the Kestrel-hosted process. A reverse proxy such as nginx, YARP, or Azure Front Door typically terminates TLS at the edge, decrypts client traffic, and forwards plain HTTP to Kestrel on an internal port, since certificate management and cipher policy are easier to centralize there than to distribute across every Kestrel instance.

IIS on Windows adds a third role: it acts as reverse proxy in out-of-process mode (forwarding to a separate Kestrel `dotnet` process) or hosts in-process (running the app inside `w3wp.exe` via ANCM) — providing Windows authentication integration, app pool recycling, request filtering, and familiar Windows operations tooling without replacing Kestrel as the runtime. Multiple layers exist because each solves different problems: the edge proxy handles scale and security, IIS handles Windows operational concerns, and Kestrel handles running .NET efficiently. Confusing who terminates TLS and who runs app code leads to broken redirects, wrong client IPs, and certificates placed on the wrong layer.

---

#### Q2. (P) An API works locally over HTTPS but generates `http://` links and logs the wrong client IP when deployed behind nginx. Review this configuration — what is missing, and why does middleware order matter?

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
var app = builder.Build();
app.UseAuthentication();
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.MapControllers();
```

**Concepts**
- UseForwardedHeaders must run before auth and HTTPS redirection
- KnownProxies or KnownNetworks for trusted proxy restriction
- Request.Scheme and RemoteIpAddress set by forwarded headers middleware
- Auth and link generation consuming wrong scheme before headers are applied

**Answer**

`UseForwardedHeaders()` is called after `UseAuthentication()`, which means authentication, HTTPS redirection, and link generation all run before `Request.Scheme` is updated from `X-Forwarded-Proto`. The result is that `Request.Scheme` stays `http`, `RemoteIpAddress` remains the nginx IP, and any URL generated by `LinkGenerator` or `CreatedAtAction` uses the wrong scheme. Trusted proxy configuration is also missing — without `KnownProxies` or `KnownNetworks`, the middleware is not restricted to your nginx IP, allowing any client to spoof forwarded headers.

The fix is to move `app.UseForwardedHeaders()` as the very first middleware after `Build()`, before authentication, HTTPS redirection, and routing. Then configure trusted proxy addresses in `ForwardedHeadersOptions` to prevent header spoofing.

```csharp
var app = builder.Build();
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

#### Q3. (M) When would you run Kestrel directly vs host in IIS in-process vs out-of-process? What problems does IIS still solve for Windows deployments even when Kestrel executes the .NET code?

**Concepts**
- Kestrel direct for containers and cross-platform deployments
- IIS in-process for lowest latency and Windows auth integration
- IIS out-of-process for crash isolation
- IIS app pool recycling, request filtering, and Windows operations tooling

**Answer**

Run Kestrel directly for Docker/Kubernetes, cross-platform deployments, and development — you manage TLS, restarts, and the reverse proxy yourself, but you gain maximum portability. Use IIS in-process when deploying to traditional Windows servers where lowest latency and access to the Windows authentication pipeline matter — the app runs inside `w3wp.exe` via ANCM with the least indirection. Use IIS out-of-process when crash isolation is a priority — a Kestrel process crash does not necessarily recycle the IIS worker, so IIS can restart the dotnet process independently.

IIS still provides value even when Kestrel executes .NET code: automatic app pool recycling on unhealthy processes, Windows Authentication integration, centralized request filtering and logging, and familiar Windows ops tooling — all without needing a separate nginx or reverse proxy deployment on the Windows platform. The choice is primarily driven by target environment: containers and Linux point to Kestrel with an edge proxy; legacy Windows server farms point to IIS with ANCM.

---

#### Q4. (P) How does `ASPNETCORE_ENVIRONMENT` (and `IHostEnvironment.EnvironmentName`) drive behavior at startup and per request? What breaks if Production is mis-set to Development on a public server?

**Concepts**
- ASPNETCORE_ENVIRONMENT controlling conditional middleware and config loading
- appsettings.{Environment}.json override merging at startup
- Developer exception page and Swagger exposed when mis-set
- launchSettings.json as development-only not production configuration

**Answer**

`ASPNETCORE_ENVIRONMENT` sets `IHostEnvironment.EnvironmentName`, which is read at startup to branch pipeline configuration, merge `appsettings.{Environment}.json` overrides, set logging verbosity, and gate features like Swagger, developer exception pages, and EF seed data. Access it in `Program.cs` via `builder.Environment` before `Build()` or via `app.Environment` after — branch with `if (app.Environment.IsDevelopment()) { … }` rather than `#if DEBUG` so the check is deployment-time not compile-time.

Mis-setting Production to Development exposes detailed exception pages with stack traces and internal paths to any client that triggers an error. Swagger UI may be enabled, allowing API exploration by unauthorized parties. `appsettings.Development.json` overrides merge and may point to wrong databases, disable auth features, or relax CORS. Verbose logging spikes monitoring and compliance audits flag diagnostic endpoints. Set `ASPNETCORE_ENVIRONMENT=Production` via host-level environment variables — Azure App Setting, Kubernetes manifest, Docker `-e` — not via `launchSettings.json`, which exists only on developer machines.

---

#### Q5. (M) Configure Kestrel to listen on specific URLs and ports — `ASPNETCORE_URLS`, `ListenAnyIP`, HTTPS certificate binding. How does this differ from IIS binding and from container `EXPOSE` / Kubernetes `containerPort`?

**Concepts**
- ASPNETCORE_URLS — semicolon-separated listen addresses
- ConfigureKestrel ListenAnyIP for explicit port and certificate binding
- IIS bindings as a separate Windows configuration surface
- EXPOSE and containerPort as documentation not listeners

**Answer**

Kestrel binding is configured via `ASPNETCORE_URLS` for environment-driven setup (`http://+:8080`), or `builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(8080))` for explicit programmatic control including HTTPS certificate binding with `o.Listen(IPAddress.Loopback, 5001, o => o.UseHttps(cert))`. The `+` wildcard binds all interfaces inside a container, which is required for probes and ingress traffic that come from outside the loopback.

IIS bindings are a separate configuration surface — they define site bindings in IIS Manager including hostname, port, and certificate — and ANCM forwards incoming requests to Kestrel's backend port. In out-of-process hosting these are two independent port configurations that must align. Docker `EXPOSE 8080` and Kubernetes `containerPort: 8080` are documentation only — they do not open ports or configure listeners. Kestrel must actually bind the same port via `ASPNETCORE_URLS` or `ConfigureKestrel` for connections to succeed. A Kubernetes Service then maps external port 80 to `targetPort: 8080` inside the pod.

```csharp
builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenAnyIP(8080);
});
```

---

#### Q6. (P) TLS is terminated at nginx; Kestrel receives plain HTTP on port 8080. What must be true in Kestrel, forwarded headers, and cookie/`SameSite` settings so redirects, HSTS, and secure cookies still behave correctly?

**Concepts**
- nginx sending X-Forwarded-Proto: https
- UseForwardedHeaders updating Request.IsHttps
- Cookie secure policy using perceived scheme
- HTTPS redirection often disabled when TLS fully offloaded

**Answer**

nginx must send `X-Forwarded-Proto: https` and `X-Forwarded-For` in every request to the backend. `UseForwardedHeaders()` must run first in the ASP.NET Core pipeline with `ForwardedHeaders.XForwardedProto` enabled and nginx's IP in `KnownProxies`, so `Request.IsHttps` returns `true` inside the application even though Kestrel received plain HTTP.

`UseHttpsRedirection()` may still redirect HTTP clients hitting Kestrel's internal port — this is often disabled or restricted when only nginx is public-facing, since Kestrel on port 8080 is not reachable externally. Auth cookie policy should use `CookieSecurePolicy.Always` or `SameAsRequest` — with forwarded proto applied, `SameAsRequest` correctly marks cookies `Secure` when the client used HTTPS. HSTS is typically set at nginx or the CDN; if also set in ASP.NET Core, the app must see the correct scheme via forwarded headers for the `Strict-Transport-Security` header to emit correctly. Do not configure a Kestrel HTTPS certificate if TLS is fully offloaded — plain HTTP on the internal port is the normal and correct pattern.

---

#### Q7. (P) Implement host-level health checks for Kubernetes liveness vs readiness — `/health/live` vs `/health/ready` with DB dependency. Where do you register checks, and why should the liveness probe stay lightweight?

**Concepts**
- AddHealthChecks().AddSqlServer() with readiness tag
- Separate MapHealthChecks paths with Predicate filtering
- Liveness failure triggering pod restart — must not include slow checks
- Readiness failure removing pod from Service load balancer

**Answer**

Register checks with `builder.Services.AddHealthChecks()` tagging dependency checks with `"ready"` so they appear only on the readiness endpoint. Map two separate endpoints using `HealthCheckOptions.Predicate` to filter which checks run per path.

Liveness — `/health/live` — should use an empty predicate or a check that merely verifies the process can respond, since a failing liveness probe triggers a pod restart in Kubernetes. Including a database connectivity check on liveness causes unnecessary pod kills during transient connection blips. Readiness — `/health/ready` — includes the tagged dependency checks, so the pod is removed from the Service load balancer when the database is unreachable without restarting the container. Kubernetes probe configuration targets these paths at the same port Kestrel binds.

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(cs, tags: new[] { "ready" });
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") });
```

---

#### Q8. (P) A Docker image for your API fails health checks: the container listens on 8080 but orchestrator probes port 80. Trace configuration from `ASPNETCORE_URLS`, `WebApplication.Urls`, Kestrel `ListenOptions`, and the Dockerfile `EXPOSE` directive.

**Concepts**
- ASPNETCORE_URLS as the authoritative Kestrel bind address in containers
- EXPOSE as documentation only — does not open ports
- Kubernetes containerPort and probe port must match Kestrel bind
- Docker -p or Kubernetes Service targetPort mapping external to container

**Answer**

The container listens on 8080 because `ASPNETCORE_URLS=http://+:8080` (or `ListenAnyIP(8080)`) configures Kestrel's bind address. The orchestrator probes port 80 because the Kubernetes Service, probe config, or Docker port mapping targets 80 — that mismatch causes connection refused. `EXPOSE 8080` in the Dockerfile and `containerPort: 8080` in the Kubernetes pod spec are documentation only; they do not open ports or change what Kestrel binds.

Trace the chain: Kestrel bind address from `ASPNETCORE_URLS` env var (`http://+:8080`) → Dockerfile `EXPOSE 8080` declaring the port → Kubernetes pod spec `containerPort: 8080` → probe `httpGet.port: 8080` → Service `targetPort: 8080`. All four must agree. `WebApplication.Urls` set in code applies only if no `ASPNETCORE_URLS` env var is set; `launchSettings.json` never applies in containers. Diagnose with `docker exec <container> curl localhost:8080/health` to verify Kestrel is actually listening before investigating the orchestrator.

```dockerfile
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
```

---

#### Q9. (R) Review this production `Program.cs` hosting setup. What would you fix before go-live?

```csharp
builder.WebHost.UseKestrel(o => o.ListenAnyIP(5000));
builder.Services.AddHealthChecks();
var app = builder.Build();
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
```

*(Assume deploy: Linux container behind nginx terminating TLS, `ASPNETCORE_ENVIRONMENT=Production`.)*

**Concepts**
- Missing UseForwardedHeaders before HTTPS redirection
- No Production exception handler producing ProblemDetails
- HTTPS redirection problematic behind nginx TLS offload
- Single health endpoint not split for liveness vs readiness

**Answer**

There are four issues before go-live. First, `UseForwardedHeaders()` is missing — nginx sends `X-Forwarded-Proto` and `X-Forwarded-For`, but without forwarded headers middleware these are never applied, so `Request.Scheme` stays `http`, client IPs are nginx's address, and generated URLs use the wrong scheme.

Second, there is no exception handler for Production. The `if (IsDevelopment())` branch only gates the developer exception page but provides no `UseExceptionHandler()` or `IExceptionHandler` for the non-development path — unhandled exceptions return raw 500 responses rather than consistent `ProblemDetails`.

Third, `UseHttpsRedirection()` behind nginx TLS offload may redirect requests incorrectly. Since nginx forwards plain HTTP to Kestrel on port 5000, the `Request.Scheme` is `http` (unless forwarded headers are applied first) — adding forwarded headers fixes this, but if nginx is the sole public entry point, HTTPS redirection on the internal HTTP port is often disabled entirely.

Fourth, the single `/health` endpoint does not distinguish liveness from readiness, so Kubernetes cannot independently control pod restarts and traffic routing.

```csharp
app.UseForwardedHeaders();
app.UseExceptionHandler();
// Conditionally apply or skip HTTPS redirection behind TLS-offloading proxy
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") });
```

---

#### Q10. (D) Compare Development vs Production hosting configuration: hot reload, detailed errors, HTTPS dev certificates, logging verbosity, and forwarded headers trust. What belongs in `appsettings.Development.json` vs environment variables vs platform config?

**Concepts**
- Development — hot reload, dev certs, verbose logs, detailed errors
- Production — sanitized errors, env var secrets, explicit KnownProxies
- appsettings.Development.json for local non-secrets committed to source
- Platform config — K8s manifests, Azure App Settings for production values

**Answer**

Development optimizes developer feedback: `dotnet watch` hot reload, `ASPNETCORE_HTTPS_PORTS` dev certificate trust, `UseDeveloperExceptionPage()`, verbose logging in `appsettings.Development.json`, user secrets for local API keys, and relaxed CORS. These belong in `appsettings.Development.json` or `Properties/launchSettings.json` since they are committed to source and safe for developer machines.

Production optimizes security and operability: `ASPNETCORE_ENVIRONMENT=Production` via host platform config, exception handler producing `ProblemDetails` with only `traceId` in the response body, Information/Warning log levels, secrets from environment variables or Key Vault — never committed JSON files. Forwarded headers trust must specify explicit `KnownProxies` in production rather than clearing `KnownNetworks` which would trust everyone.

The configuration hierarchy in order of precedence is: `appsettings.json` (base) → `appsettings.{Environment}.json` (environment overlay) → environment variables (platform secrets, higher precedence) → command-line arguments. Production values that override base config belong in environment variables or platform-managed secret stores, not in JSON files deployed with the application.
