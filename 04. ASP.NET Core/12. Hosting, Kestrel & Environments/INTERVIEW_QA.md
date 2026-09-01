# Hosting, Kestrel & Environments — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 12. Hosting, Kestrel & Environments](#chapter-12-hosting-kestrel-environments)
  - [Q1. What is Kestrel?](#chapter-12-hosting-kestrel-environments-q1)
  - [Q2. What is the role of IIS in hosting ASP.NET Core on Windows?](#chapter-12-hosting-kestrel-environments-q2)
  - [Q3. What is a reverse proxy, and why use one with ASP.NET Core?](#chapter-12-hosting-kestrel-environments-q3)
  - [Q4. Who terminates TLS in a typical nginx + Kestrel deployment?](#chapter-12-hosting-kestrel-environments-q4)
  - [Q5. What is `ASPNETCORE_ENVIRONMENT`?](#chapter-12-hosting-kestrel-environments-q5)
  - [Q6. What is `IHostEnvironment` / `IWebHostEnvironment`?](#chapter-12-hosting-kestrel-environments-q6)
  - [Q7. How does the environment name affect application behavior?](#chapter-12-hosting-kestrel-environments-q7)
  - [Q8. What is `ASPNETCORE_URLS`?](#chapter-12-hosting-kestrel-environments-q8)
  - [Q9. How do you configure Kestrel to listen on a specific port?](#chapter-12-hosting-kestrel-environments-q9)
  - [Q10. What is the difference between Kestrel endpoint configuratio…](#chapter-12-hosting-kestrel-environments-q10)
  - [Q11. What are forwarded headers (`X-Forwarded-For`, `X-Forwarded-…](#chapter-12-hosting-kestrel-environments-q11)
  - [Q12. What does `UseForwardedHeaders()` do, and why must it run ea…](#chapter-12-hosting-kestrel-environments-q12)
  - [Q13. What is the difference between in-process and out-of-process…](#chapter-12-hosting-kestrel-environments-q13)
  - [Q14. What are health checks in ASP.NET Core?](#chapter-12-hosting-kestrel-environments-q14)
  - [Q15. What is the difference between a liveness probe and a readin…](#chapter-12-hosting-kestrel-environments-q15)
  - [Q16. How do container `EXPOSE` directives relate to Kestrel liste…](#chapter-12-hosting-kestrel-environments-q16)
  - [Q17. What breaks if Production is accidentally set to Development…](#chapter-12-hosting-kestrel-environments-q17)
  - [Q18. What belongs in `appsettings.Development.json` vs environmen…](#chapter-12-hosting-kestrel-environments-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 12. Hosting, Kestrel & Environments

### Q1. What is Kestrel? {#chapter-12-hosting-kestrel-environments-q1}

What is Kestrel?

**Answer:** Kestrel is the cross-platform, high-performance web server built into ASP.NET Core that listens for HTTP/HTTPS connections and drives the request pipeline.

- It runs on Windows, Linux, and macOS — the default server for `dotnet run`, containers, and cloud deployments.
- Kestrel handles connection management, HTTP parsing, TLS (when configured), and passes requests to middleware/endpoints.
- It is optimized for async I/O and integrates tightly with the .NET thread pool and DI container.
- Production deployments often place a reverse proxy in front of Kestrel for TLS termination and edge policies.

---

### Q2. What is the role of IIS in hosting ASP.NET Core on Windows? {#chapter-12-hosting-kestrel-environments-q2}

What is the role of IIS in hosting ASP.NET Core on Windows?

**Answer:** IIS acts as a reverse proxy and process manager for ASP.NET Core on Windows via the ASP.NET Core Module (ANCM) — handling Windows integration, certificate binding, and worker process lifecycle while Kestrel executes application code.

- **In-process:** the app runs inside the IIS worker process (`w3wp.exe`) for lowest latency and shared Windows Auth.
- **Out-of-process:** IIS forwards requests to a separate Kestrel `dotnet` process — better crash isolation.
- IIS provides app pool recycling, request filtering, centralized logging, and familiar Windows admin tooling.
- IIS does not replace Kestrel — it forwards to or hosts the Kestrel-based ASP.NET Core runtime.

---

### Q3. What is a reverse proxy, and why use one with ASP.NET Core? {#chapter-12-hosting-kestrel-environments-q3}

What is a reverse proxy, and why use one with ASP.NET Core?

**Answer:** A reverse proxy sits in front of Kestrel, receives client traffic, and forwards it to backend app instances — handling TLS, load balancing, compression, caching, WAF, and rate limiting at the edge.

- Examples: nginx, IIS, YARP, Azure Front Door, AWS ALB, Cloudflare.
- Kestrel focuses on running .NET efficiently; the proxy handles infrastructure concerns at scale.
- Proxies enable zero-downtime deployments, multiple instances, and SSL certificate centralization.
- Without a proxy, Kestrel must handle all edge concerns directly — workable but less common in Production.

---

### Q4. Who terminates TLS in a typical nginx + Kestrel deployment? {#chapter-12-hosting-kestrel-environments-q4}

Who terminates TLS in a typical nginx + Kestrel deployment?

**Answer:** nginx terminates TLS at the edge — clients connect via HTTPS to nginx, which decrypts traffic and forwards plain HTTP to Kestrel on an internal port (e.g., 5000 or 8080).

- SSL certificates are installed on nginx, not necessarily on each Kestrel instance.
- Kestrel may still be configured for HTTPS internally, but the common pattern is HTTP behind the proxy on a trusted network.
- `X-Forwarded-Proto: https` tells Kestrel the original scheme so redirects and link generation use HTTPS.
- Terminating TLS at the edge simplifies certificate rotation and cipher policy management.

---

### Q5. What is `ASPNETCORE_ENVIRONMENT`? {#chapter-12-hosting-kestrel-environments-q5}

What is `ASPNETCORE_ENVIRONMENT`?

**Answer:** `ASPNETCORE_ENVIRONMENT` is an environment variable that sets the hosting environment name (e.g., `Development`, `Staging`, `Production`) consumed at startup by the generic host.

- Read via `IHostEnvironment.EnvironmentName` or `IWebHostEnvironment` in application code.
- Controls which `appsettings.{Environment}.json` file merges into configuration.
- Gates conditional startup logic — developer exception page, Swagger UI, verbose logging.
- Must be set explicitly in Production deployments — it defaults to `Production` when unset in many hosts but should never be left ambiguous.

---

### Q6. What is `IHostEnvironment` / `IWebHostEnvironment`? {#chapter-12-hosting-kestrel-environments-q6}

What is `IHostEnvironment` / `IWebHostEnvironment`?

**Answer:** These DI-injected abstractions expose hosting context — environment name, content root path, application name, and (for web) web root path — without reading environment variables directly.

- `IHostEnvironment` is available in any .NET host; `IWebHostEnvironment` adds `WebRootPath` for web apps.
- `IsDevelopment()`, `IsStaging()`, `IsProduction()` are convenience checks against `EnvironmentName`.
- `ContentRootPath` is the application base directory (where `appsettings.json` lives); `WebRootPath` points to `wwwroot`.
- Prefer injecting these interfaces over hard-coding environment checks against raw strings.

---

### Q7. How does the environment name affect application behavior? {#chapter-12-hosting-kestrel-environments-q7}

How does the environment name affect application behavior?

**Answer:** The environment name selects configuration files, logging verbosity, middleware branches, and feature toggles registered conditionally at startup.

- `appsettings.Development.json` overrides base settings when `EnvironmentName` is `Development`.
- Templates enable Swagger, developer exception pages, and detailed errors only in Development.
- `ValidateOnStart`, EF migrations, and seed data may run differently per environment.
- Production should disable diagnostic endpoints, minimize log noise, and enforce strict exception handling.

---

### Q8. What is `ASPNETCORE_URLS`? {#chapter-12-hosting-kestrel-environments-q8}

What is `ASPNETCORE_URLS`?

**Answer:** `ASPNETCORE_URLS` is an environment variable that sets the addresses Kestrel listens on, using semicolon-separated URLs such as `http://0.0.0.0:8080` or `https://localhost:5001;http://localhost:5000`.

- Overrides default `launchSettings.json` URLs when set — common in Docker and Kubernetes.
- Read at startup by Kestrel endpoint configuration before the first request.
- In containers, typically set to `http://+:8080` to bind all interfaces on port 8080.
- Distinct from reverse proxy external ports — `ASPNETCORE_URLS` controls Kestrel's bind addresses inside the container or process.

---

### Q9. How do you configure Kestrel to listen on a specific port? {#chapter-12-hosting-kestrel-environments-q9}

How do you configure Kestrel to listen on a specific port?

**Answer:** Configure endpoints via `ASPNETCORE_URLS`, `launchSettings.json`, `appsettings.json` (`Kestrel:Endpoints`), or programmatic `builder.WebHost.ConfigureKestrel()`.

- Environment variable: `ASPNETCORE_URLS=http://0.0.0.0:5000`.
- `appsettings.json`: `"Kestrel": { "Endpoints": { "Http": { "Url": "http://localhost:5050" } } }`.
- Code: `builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(8080))` for explicit control including HTTPS certificates.
- Docker maps container port via `EXPOSE`/`docker run -p` — Kestrel must listen on the container's internal port.

---

### Q10. What is the difference between Kestrel endpoint configuration and IIS bindings? {#chapter-12-hosting-kestrel-environments-q10}

What is the difference between Kestrel endpoint configuration and IIS bindings?

**Answer:** Kestrel endpoint configuration (`ASPNETCORE_URLS`, `Kestrel:Endpoints`, `ConfigureKestrel`) defines which addresses the .NET process listens on; IIS bindings define which URLs IIS accepts on the machine and how it forwards to Kestrel.

- IIS bindings include hostname, port, and certificate at the IIS site level — Kestrel may never see HTTPS directly.
- Out-of-process IIS forwards to a localhost port configured by ANCM (e.g., random high port or `aspnetcore` settings).
- In-process hosting merges IIS and Kestrel — IIS bindings are authoritative for incoming traffic.
- Misaligned ports between IIS forwarding and Kestrel listening cause 502.5 process failures.

---

### Q11. What are forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`)? {#chapter-12-hosting-kestrel-environments-q11}

What are forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`)?

**Answer:** Forwarded headers are HTTP headers set by reverse proxies to pass the original client IP, scheme, and host to the backend app, which otherwise sees only the proxy's connection details.

- `X-Forwarded-For` — original client IP address chain.
- `X-Forwarded-Proto` — original scheme (`http` or `https`).
- `X-Forwarded-Host` — original Host header from the client request.
- Without them, `HttpContext.Connection.RemoteIpAddress` is the proxy IP and `Request.Scheme` is `http` even for HTTPS clients.

---

### Q12. What does `UseForwardedHeaders()` do, and why must it run early? {#chapter-12-hosting-kestrel-environments-q12}

What does `UseForwardedHeaders()` do, and why must it run early?

**Answer:** `UseForwardedHeaders()` reads `X-Forwarded-*` headers from trusted proxies and updates `HttpContext.Connection.RemoteIpAddress`, `Request.Scheme`, and `Request.Host` before downstream middleware uses them.

- Must run **first** (immediately after `Build()`) — before HTTPS redirection, authentication, link generation, and logging.
- Configure `ForwardedHeadersOptions.KnownProxies` or `KnownNetworks` so untrusted clients cannot spoof headers.
- Required behind nginx, IIS as reverse proxy, Azure App Service, and load balancers for correct URLs and audit logs.
- Late placement causes auth cookies, redirects, and `CreatedAtAction` URLs to use wrong scheme/IP.

---

### Q13. What is the difference between in-process and out-of-process IIS hosting? {#chapter-12-hosting-kestrel-environments-q13}

What is the difference between in-process and out-of-process IIS hosting?

**Answer:** In-process runs the ASP.NET Core app inside the IIS worker process; out-of-process runs Kestrel in a separate `dotnet` process with IIS proxying requests to it.

- **In-process:** lower latency, shared application pool lifecycle, app runs as IIS native module — Windows-only.
- **Out-of-process:** Kestrel is a separate process — IIS restarts independently; crash in app does not necessarily recycle IIS worker.
- Both use ANCM; the hosting model is set in the `.csproj` via `<AspNetCoreHostingModel>InProcess|OutOfProcess</AspNetCoreHostingModel>`.
- Containers and Linux always use Kestrel directly — IIS hosting is Windows-specific.

---

### Q14. What are health checks in ASP.NET Core? {#chapter-12-hosting-kestrel-environments-q14}

What are health checks in ASP.NET Core?

**Answer:** Health checks are registered endpoints that report application and dependency readiness — database connectivity, disk space, downstream APIs — for orchestrators and load balancers to make routing decisions.

- Register with `builder.Services.AddHealthChecks()` and map via `app.MapHealthChecks("/health")`.
- Return `Healthy`, `Degraded`, or `Unhealthy` with optional detailed JSON via `UIResponseWriter` or custom writers.
- Kubernetes uses them for liveness and readiness probes; Azure App Service uses them for load balancer routing.
- Separate checks for "is process alive" vs "can serve traffic" map to liveness vs readiness probes.

---

### Q15. What is the difference between a liveness probe and a readiness probe? {#chapter-12-hosting-kestrel-environments-q15}

What is the difference between a liveness probe and a readiness probe?

**Answer:** A liveness probe asks whether the process is alive and should be restarted if failing; a readiness probe asks whether the instance is ready to receive traffic and should be removed from the load balancer if failing.

- **Liveness:** basic self-check — hung deadlocks may fail liveness and trigger pod restart.
- **Readiness:** dependency checks (DB, cache, migrations complete) — failing readiness removes the pod from service without restarting it.
- Map liveness to a lightweight `/health/live` and readiness to `/health/ready` with dependency checks in Kubernetes.
- Do not include slow external calls in liveness — false restarts cause cascading outages.

---

### Q16. How do container `EXPOSE` directives relate to Kestrel listening ports? {#chapter-12-hosting-kestrel-environments-q16}

How do container `EXPOSE` directives relate to Kestrel listening ports?

**Answer:** `EXPOSE` documents which port the container listens on; Kestrel must actually bind to that port via `ASPNETCORE_URLS` — `EXPOSE` alone does not open or configure the listener.

- Example Dockerfile: `ENV ASPNETCORE_URLS=http://+:8080` + `EXPOSE 8080` + `docker run -p 8080:8080`.
- Mismatch between `ASPNETCORE_URLS` and `-p` mapping causes connection refused or wrong-port routing.
- Kubernetes `containerPort` must align with Kestrel's bind port inside the pod.
- The host maps external ports — Kestrel typically binds `0.0.0.0` inside the container, not `localhost` only.

---

### Q17. What breaks if Production is accidentally set to Development? {#chapter-12-hosting-kestrel-environments-q17}

What breaks if Production is accidentally set to Development?

**Answer:** The app exposes developer exception pages with stack traces, may enable Swagger UI publicly, uses Development configuration overrides, and applies verbose logging — creating security vulnerabilities and performance overhead.

- Sensitive configuration from `appsettings.Development.json` may merge into runtime settings.
- Attackers receive detailed error responses revealing code paths, dependencies, and connection hints.
- CORS, auth, and feature flags tied to Development may allow unintended access.
- Monitoring alerts spike from verbose logs; compliance audits flag exposed diagnostic endpoints.

---

### Q18. What belongs in `appsettings.Development.json` vs environment variables in Production? {#chapter-12-hosting-kestrel-environments-q18}

What belongs in `appsettings.Development.json` vs environment variables in Production?

**Answer:** `appsettings.Development.json` holds local-only settings — detailed logging, local connection strings, Swagger toggles, and seed flags safe for developer machines; Production secrets and environment-specific values belong in environment variables or a secret manager, not committed JSON files.

- Development file: `"Logging:LogLevel:Default": "Debug"`, localhost DB strings, `EnableSensitiveDataLogging`.
- Production: connection strings, API keys, and certificates via environment variables, Azure Key Vault, AWS Secrets Manager, or Kubernetes secrets.
- Never commit Production secrets to source control — inject at deploy time.
- Non-secret Production tuning (log levels, feature flags) can use `appsettings.Production.json` deployed without secrets or environment-variable overrides with higher precedence.

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

#### Q1. (M) Explain the roles of Kestrel, IIS, and a reverse proxy (nginx, YARP, Azure Front Door) in an ASP.NET Core deployment. Who terminates TLS, who runs your application code, and why do you often use more than one layer?

---

**Answer:**

**Answer:** Kestrel is the cross-platform web server that runs your ASP.NET Core application and executes request handling code; IIS and reverse proxies sit in front to terminate TLS, route traffic, and apply platform policies before requests reach Kestrel.

- **Kestrel** listens for HTTP connections and drives the ASP.NET Core pipeline — controllers, middleware, and DI all run inside the Kestrel-hosted process.
- **IIS** (Windows) acts as reverse proxy (out-of-process) or hosts in-process via the ASP.NET Core Module; it provides Windows integration, process recycling, certificate binding, and request filtering without replacing Kestrel as the runtime host.
- A **reverse proxy** (nginx, YARP, Azure Front Door) typically terminates TLS at the edge, load-balances across instances, enforces WAF/rate limits, and forwards HTTP to Kestrel on an internal port.
- TLS termination usually happens at IIS or the edge proxy — Kestrel often receives plain HTTP internally even when clients use HTTPS.
- Multiple layers exist because each solves different problems: edge = scale/security, IIS = Windows ops, Kestrel = run .NET efficiently.

**Production takeaway:** Confusing who terminates TLS and who runs app code leads to broken redirects, wrong client IPs, and certificates on the wrong layer.

---

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

---

**Answer:**

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

**Answer:** Forwarded headers run too late and trusted proxy configuration is missing — auth and URL generation execute before `X-Forwarded-*` is applied, so `Request.Scheme` stays `http` and `RemoteIpAddress` remains the proxy IP.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline order | `UseForwardedHeaders()` after `UseAuthentication()` | Auth, cookies, and redirects use wrong scheme/IP |
| Security | No `KnownProxies` / `KnownNetworks` | Untrusted clients can spoof forwarded headers |
| Hosting | Missing early forwarded headers | `LinkGenerator`, `CreatedAtAction`, audit logs use `http://` and proxy IP |

**Fix (priority order):**

1. Move `app.UseForwardedHeaders()` **immediately after** `Build()`, before authentication, HTTPS redirection, and routing.
2. Configure trusted proxies: `options.KnownProxies.Add(IPAddress.Parse("10.0.0.10"));` — never clear known networks in Production without isolation.
3. Ensure nginx sends `X-Forwarded-For` and `X-Forwarded-Proto`; optionally `ForwardedHeaders.XForwardedHost`.

```csharp
var app = builder.Build();
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Production takeaway:** "Works on localhost" hides missing forwarded headers — fix is both trusted proxy config and pipeline order, not enabling HTTPS on Kestrel alone.

---

---

#### Q3. (M) When would you run Kestrel directly vs host in IIS in-process vs out-of-process? What problems does IIS still solve for Windows deployments even when Kestrel executes the .NET code?

---

**Answer:**

**Answer:** Run Kestrel directly for containers and Linux; use IIS in-process for tight Windows integration and lowest latency; use IIS out-of-process for process isolation — IIS still provides recycling, Windows Auth integration, and centralized cert binding.

- **Kestrel direct:** Docker/Kubernetes, cross-platform, dev — you manage TLS, restarts, and reverse proxy yourself.
- **IIS in-process:** App runs inside IIS worker process via ANCM — lower overhead, shared Windows auth pipeline on traditional Windows servers.
- **IIS out-of-process:** Kestrel in separate dotnet process behind IIS — crash isolation, consistent with nginx patterns.
- IIS provides automatic app pool recycling, Windows Authentication, request filtering, and familiar ops tooling even when Kestrel executes .NET code.
- Choose by target: containers → Kestrel + edge proxy; legacy Windows farm → IIS + ANCM.

**Production takeaway:** Hosting model affects recycling, crash blast radius, and Windows auth — not just "which executable listens on port 80."

---

---

#### Q4. (P) How does `ASPNETCORE_ENVIRONMENT` (and `IHostEnvironment.EnvironmentName`) drive behavior at startup and per request? What breaks if Production is mis-set to Development on a public server?

---

**Answer:**

**Answer:** `ASPNETCORE_ENVIRONMENT` sets `IHostEnvironment.EnvironmentName`, which controls conditional middleware (`DeveloperExceptionPage`), configuration loading (`appsettings.{Environment}.json`), logging levels, and EF migration behavior — mis-setting Production to Development exposes stack traces and verbose errors publicly.

- Read via `builder.Environment` / `app.Environment` — branch pipeline: `if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();`
- Loads `appsettings.Development.json` overrides when env is Development — may point to local DB or disable auth features.
- **Mis-set to Development in Production:** detailed exception pages, potentially swagger exposed, relaxed CORS from dev config, sensitive data in logs.
- Set via environment variable on the host (Azure App Setting, K8s manifest, Docker `-e`) — not only `launchSettings.json` (dev machine only).
- `IsProduction()`, `IsStaging()`, `IsEnvironment("Custom")` for custom deployment slots.

**Production takeaway:** Environment name is a security and reliability switch — treat `ASPNETCORE_ENVIRONMENT=Production` as mandatory checklist item for go-live.

---

---

#### Q5. (M) Configure Kestrel to listen on specific URLs and ports — `ASPNETCORE_URLS`, `ListenAnyIP`, HTTPS certificate binding. How does this differ from IIS binding and from container `EXPOSE` / Kubernetes `containerPort`?

---

**Answer:**

**Answer:** Kestrel binding is configured via `ASPNETCORE_URLS`, `builder.WebHost.UseUrls`, or `ConfigureKestrel` with `Listen`/`ListenAnyIP`; IIS bindings are separate Windows HTTP.sys/ANCM configuration; container `EXPOSE` and K8s `containerPort` only declare ports — the app must actually listen on the same port inside the container.

- **`ASPNETCORE_URLS`:** `http://+:8080` or `https://+:443` — common in containers; `+` = all interfaces.
- **`ConfigureKestrel`:** explicit control — `options.ListenAnyIP(8080);` or `Listen(IPAddress.Loopback, 5001, o => o.UseHttps(cert));`
- **IIS binding:** site bindings in IIS Manager (443 + cert) — ANCM forwards to Kestrel backend port; different config surface from Kestrel when out-of-process.
- **Docker `EXPOSE 8080`:** documentation only unless paired with `-p`; app must bind `8080` via `ASPNETCORE_URLS=http://+:8080`.
- **Kubernetes:** `containerPort: 8080` on pod spec + probe targeting same port; Service maps 80 → 8080.

```csharp
builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenAnyIP(8080);
});
```

**Production takeaway:** Port mismatch between probe, Service, Dockerfile, and `ASPNETCORE_URLS` is the #1 container health-check failure — all layers must agree on the listening port.

---

---

#### Q6. (P) TLS is terminated at nginx; Kestrel receives plain HTTP on port 8080. What must be true in Kestrel, forwarded headers, and cookie/`SameSite` settings so redirects, HSTS, and secure cookies still behave correctly?

---

**Answer:**

**Answer:** nginx must send `X-Forwarded-Proto: https`, ASP.NET Core must apply forwarded headers early with trusted proxy config, and cookie auth must mark cookies `Secure` based on perceived scheme — optionally disable Kestrel HTTPS redirection when TLS is entirely at the edge.

- Configure nginx: `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;` and `X-Forwarded-Proto $scheme;`
- `UseForwardedHeaders()` first with `ForwardedHeaders.XForwardedProto` so `Request.IsHttps` is true inside the app.
- `UseHttpsRedirection()` may still redirect HTTP clients hitting Kestrel directly on internal network — often disabled or restricted when only nginx is public.
- Auth cookies: `CookieSecurePolicy.Always` or `SameAsRequest` — with forwarded proto, `SameAsRequest` correctly marks Secure when client used HTTPS.
- HSTS is typically set at nginx/CDN; if set in ASP.NET, ensure middleware sees HTTPS via forwarded headers.
- Do not configure Kestrel HTTPS cert if TLS is fully offloaded — plain HTTP on internal port is normal.

**Production takeaway:** TLS offload without forwarded proto breaks cookie auth and generated URLs — the app must trust the edge's view of the client scheme.

---

---

#### Q7. (P) Implement host-level health checks for Kubernetes liveness vs readiness — `/health/live` vs `/health/ready` with DB dependency. Where do you register checks, and why should the liveness probe stay lightweight?

---

**Answer:**

**Answer:** Register checks with `AddHealthChecks()`, map separate endpoints with different predicates — liveness only verifies process responsiveness; readiness includes dependencies like SQL so traffic stops when the app cannot serve.

- Register: `builder.Services.AddHealthChecks().AddSqlServer(connectionString, name: "db");`
- Map: `app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });` — always healthy if process runs (or empty predicate returning true for trivial check).
- Readiness: `app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });` — tag DB check with `.AddCheck(..., tags: new[] { "ready" })`.
- **Liveness lightweight:** failing liveness **restarts the pod** — expensive checks cause restart loops during transient DB blips; readiness failure only removes pod from Service load balancer.
- K8s manifest: `livenessProbe.httpGet.path: /health/live`, `readinessProbe.httpGet.path: /health/ready`, ports matching Kestrel bind.

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(cs, tags: new[] { "ready" });
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") });
```

**Production takeaway:** Putting DB checks on liveness causes unnecessary pod kills — readiness gates traffic, liveness gates process death.

---

---

#### Q8. (P) A Docker image for your API fails health checks: the container listens on 8080 but orchestrator probes port 80. Trace configuration from `ASPNETCORE_URLS`, `WebApplication.Urls`, Kestrel `ListenOptions`, and the Dockerfile `EXPOSE` directive.

---

**Answer:**

**Answer:** Align all four layers on the same port — set `ASPNETCORE_URLS=http://+:8080` (or Kestrel `ListenAnyIP(8080)`), `EXPOSE 8080` in Dockerfile, and configure probes/Services to target 8080, not 80.

- Default ASP.NET container images may listen on 8080 (.NET 8+) while older samples use 80 — verify the base image and template.
- `EXPOSE` alone does not bind — the runtime must listen: `ENV ASPNETCORE_URLS=http://+:8080`.
- Kubernetes `livenessProbe.httpGet.port: 8080` must match; Service `targetPort: 8080` maps external 80 → container 8080.
- `WebApplication.Urls` or `launchSettings` do not apply in container unless env vars set — Dockerfile `ENV` or K8s env required.
- Diagnostic: `docker exec` + `curl localhost:8080/health` inside container; check `dotnet` listening with `netstat`.

```dockerfile
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
```

**Production takeaway:** Port 80 vs 8080 mismatches produce "app works in curl from exec but probe fails" — trace env var → Kestrel bind → probe port as one chain.

---

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

---

**Answer:**

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

**Answer:** Missing forwarded headers before HTTPS redirection, no exception handler for Production, HTTPS redirection may misbehave behind TLS-offloading nginx, and health checks are not split for orchestrator liveness/readiness.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Hosting | No `UseForwardedHeaders()` | Wrong scheme/IP; broken URLs and audit logs |
| Exception handling | No `UseExceptionHandler` / `IExceptionHandler` in Production | Raw 500 responses; inconsistent API errors |
| Hosting | `UseHttpsRedirection()` behind nginx TLS offload | May redirect incorrectly on internal HTTP |
| Operations | Single `/health` with no readiness/liveness split | K8s cannot distinguish process vs dependency failure |
| Container | Hard-coded port 5000 vs image standard 8080 | Probe/port mapping mismatch if env not aligned |

**Fix (priority order):**

1. Add forwarded headers first with trusted nginx proxy addresses.
2. Add `UseExceptionHandler()` + `IExceptionHandler` and `AddProblemDetails()` for Production error shape.
3. Re-evaluate HTTPS redirection — often disabled or conditioned when TLS terminates at nginx.
4. Split health endpoints; tag dependency checks for readiness only.
5. Align Kestrel port with `ASPNETCORE_URLS` and container orchestration (8080 convention).

```csharp
app.UseForwardedHeaders();
app.UseExceptionHandler();
// Skip or conditionally apply HTTPS redirection behind TLS-offloading proxy
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") });
```

**Production takeaway:** Production hosting checklist is forwarded headers + exception handling + health probe design + port alignment — HTTPS redirection alone does not fix reverse-proxy deployment.

---

---

#### Q10. (D) Compare Development vs Production hosting configuration: hot reload, detailed errors, HTTPS dev certificates, logging verbosity, and forwarded headers trust. What belongs in `appsettings.Development.json` vs environment variables vs platform config?



**Answer:**

**Answer:** Development optimizes developer feedback (detailed errors, dev certs, verbose logs); Production optimizes security and operability (sanitized errors, platform env vars, trusted proxy lists, structured logging) — never rely on `launchSettings.json` outside local machines.

- **Development-only:** `UseDeveloperExceptionPage`, hot reload (`dotnet watch`), user secrets, relaxed CORS, local HTTPS dev certificate trust, verbose logging in `appsettings.Development.json`.
- **Production:** `ASPNETCORE_ENVIRONMENT=Production`, exception handler + ProblemDetails, secrets from env/Key Vault (not JSON files), forwarded headers with explicit `KnownProxies`, Information/Warning log levels.
- **`appsettings.Development.json`:** local connection strings, feature flags for dev — never copied to Production publish output with secrets.
- **Environment variables / platform config:** connection strings, API keys, `ASPNETCORE_URLS`, proxy trust IPs — override JSON in Production per twelve-factor practice.
- **Forwarded headers:** never `KnownProxies.Clear()` in Production; Development behind local proxy may use narrower test config.

**Production takeaway:** Split config by environment at the host boundary — Production behavior must not depend on files that only exist on developer laptops.

---
