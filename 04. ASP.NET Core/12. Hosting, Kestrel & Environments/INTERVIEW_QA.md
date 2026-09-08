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

## Gotchas — Hosting, Kestrel & Environments (Interview Traps)

---

#### Gotcha 1. `ASPNETCORE_ENVIRONMENT` is case-sensitive on Linux — `production` is not `Production`

**Concepts**
- Linux environment variable case sensitivity
- `IHostEnvironment.IsProduction()` matching the exact string `"Production"`
- `appsettings.{Environment}.json` file lookup by exact name
- Silent fallback to base settings when the variable is wrong

**Answer**

`ASPNETCORE_ENVIRONMENT=production` (lowercase) on a Linux host does not match `"Production"` — `IHostEnvironment.IsProduction()` returns false, `appsettings.Production.json` is not loaded, and the app runs with base or Development settings. This is one of the most common "works in Docker locally, behaves wrong in production" bugs. The value must exactly match the casing used in the `appsettings.{Environment}.json` filename. Always validate the environment name is set correctly at the infrastructure level — container manifest, App Service config, Kubernetes ConfigMap — as the first diagnostic step when environment-specific configuration is not applying.

---

#### Gotcha 2. Kestrel `MaxRequestBodySize` defaults to 30 MB — file upload endpoints need explicit increase

**Concepts**
- `KestrelServerOptions.Limits.MaxRequestBodySize` default 30 MB
- Per-endpoint override via `[RequestSizeLimit]` attribute
- `[DisableRequestSizeLimit]` for removing the limit on specific endpoints
- IIS in-process hosting having its own `maxAllowedContentLength` setting

**Answer**

Kestrel enforces a maximum request body size of 30 MB by default. Endpoints that accept file uploads or large batch payloads receive a 413 Request Entity Too Large response for oversized requests without a useful error message. Increase the limit globally via `builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 100_000_000)` or per-endpoint with `[RequestSizeLimit(100_000_000)]` on the action. When running behind IIS in-process, IIS also enforces `maxAllowedContentLength` in its own configuration — both limits must be raised independently. `[DisableRequestSizeLimit]` removes the Kestrel limit entirely and should be used only for streaming endpoints with their own length validation.

---

#### Gotcha 3. `UseHttpsRedirection` loops behind a TLS-terminating proxy — requires `UseForwardedHeaders` first

**Concepts**
- Kestrel receiving HTTP from proxy even when client used HTTPS
- `Request.Scheme` always `http` without `UseForwardedHeaders`
- Infinite redirect loop when `UseHttpsRedirection` runs without correct scheme
- `UseForwardedHeaders` before `UseHttpsRedirection` in middleware order

**Answer**

Behind a TLS-terminating proxy, Kestrel receives plain HTTP on the internal network, so `Request.Scheme` is always `http`. `UseHttpsRedirection` redirects any request where `Request.Scheme` is `http`, creating an infinite redirect loop. The fix is to call `UseForwardedHeaders()` before `UseHttpsRedirection()` so the `X-Forwarded-Proto` header updates `Request.Scheme` to `https` before the redirect check runs. In Kubernetes or container deployments where TLS is handled exclusively at the ingress layer, it is often correct to disable `UseHttpsRedirection` inside the pod entirely rather than relying on forwarded header configuration.

---

#### Gotcha 4. In-process vs out-of-process IIS hosting — different behavior for request handling and environment variables

**Concepts**
- In-process: `w3wp.exe` hosts the .NET runtime — lower latency, single process
- Out-of-process: Kestrel runs separately, IIS as reverse proxy
- `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` not available in in-process
- `web.config` `processPath` and `arguments` different between modes

**Answer**

ASP.NET Core applications hosted on IIS can run in-process (inside the `w3wp.exe` worker process) or out-of-process (IIS acts as a reverse proxy to a Kestrel subprocess). In-process has lower latency and simpler process management but means the app shares its lifetime with the IIS worker process — an app crash can affect the IIS worker process and other sites it hosts. Out-of-process is more isolated. Environment variables are configured differently in `web.config` for each mode. `ASPNETCORE_ENVIRONMENT` set in IIS application pool environment variables works for in-process; out-of-process sets it in the `web.config` `<aspNetCore>` `<environmentVariables>` section.

---

#### Gotcha 5. `launchSettings.json` is never deployed — production configuration must come from host environment

**Concepts**
- `launchSettings.json` read only by `dotnet run`, VS, and VS Code
- Excluded from `dotnet publish` output
- Production `ASPNETCORE_URLS` via environment variable or `appsettings.json`
- Development HTTPS certificate not available in deployed environments

**Answer**

`Properties/launchSettings.json` stores development launch profiles and is explicitly excluded from `dotnet publish`. Every setting in it — `applicationUrl`, `ASPNETCORE_ENVIRONMENT`, environment variable overrides — has no effect in deployed environments. Production URL bindings must be configured via `ASPNETCORE_URLS` environment variable or Kestrel's `Endpoints` section in `appsettings.json`. The `ASPNETCORE_ENVIRONMENT=Production` value must be set through the hosting platform's environment variable mechanism (App Service config, container manifest, systemd unit file). Discovering this at first deployment — when the app starts on a wrong port or in Development mode — is common.

---

#### Gotcha 6. Kestrel HTTP/2 requires TLS in most configurations — plaintext HTTP/2 (`h2c`) limited support

**Concepts**
- HTTP/2 requiring TLS for browser-to-server connections
- `h2c` (cleartext HTTP/2) supported by Kestrel but not most browsers
- gRPC requiring HTTP/2 — TLS or explicit `h2c` configuration needed
- Kestrel `Protocols.Http1AndHttp2` for enabling both versions

**Answer**

HTTP/2 connections from browsers always require TLS — browsers do not support HTTP/2 over cleartext (h2c). Kestrel supports h2c for non-browser clients, but enabling it requires explicit configuration: `KestrelServerOptions.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2)` with the HTTP endpoint. gRPC requires HTTP/2 — gRPC services fail with confusing errors when HTTP/2 is not enabled or when TLS is missing between client and server. In production containers, gRPC services typically run Kestrel on HTTP (h2c) on port 8080 while the ingress controller handles TLS externally and forwards h2c to the pod.

---

#### Gotcha 7. `UseUrls()` is overridden by Kestrel endpoint configuration — last-write wins ordering confusion

**Concepts**
- `WebApplication.CreateBuilder` loading Kestrel configuration from `appsettings.json`
- `UseUrls()` vs `KestrelServerOptions.Configure()` precedence
- `ASPNETCORE_URLS` environment variable taking highest precedence
- `listenOptions.UseHttps()` for per-endpoint TLS

**Answer**

`UseUrls("http://localhost:5000")` in code is overridden by Kestrel endpoint configuration in `appsettings.json` (`Kestrel:Endpoints`), which is in turn overridden by the `ASPNETCORE_URLS` environment variable. Developers who set `UseUrls()` expecting it to always control binding are surprised when a `Kestrel:Endpoints` section in `appsettings.json` silently takes precedence. The precedence order is: `KestrelServerOptions.Configure()` in code → `appsettings.json` Kestrel section → `ASPNETCORE_URLS` environment variable. Environment variables have the highest precedence, making them the reliable override mechanism for deployment environments. Avoid mixing `UseUrls()` with Kestrel configuration as the interaction is non-obvious.

---

#### Gotcha 8. `ASPNETCORE_ENVIRONMENT` not set defaults to `Production` — unexpected in new deployments

**Concepts**
- Default environment name `"Production"` when variable is absent
- `IsDevelopment()` returning false in unset environments
- Missing Swagger UI in new deployments because environment is not `Development`
- Secrets manager and Key Vault not configured — startup failures in Production mode

**Answer**

When `ASPNETCORE_ENVIRONMENT` is not set, ASP.NET Core defaults to `"Production"`. A new deployment that omits this variable runs in Production mode without explicitly intending to — `appsettings.Production.json` is loaded, `IsDevelopment()` returns false, Swagger UI is hidden if gated to Development, and any startup code conditioned on the environment name runs the production path. This is typically correct behavior, but teams that develop features with Staging-specific configuration and forget to set `ASPNETCORE_ENVIRONMENT=Staging` on the staging host get Production behavior silently. Always explicitly set the environment variable in every deployment environment rather than relying on the absence-equals-Production default.

---

#### Gotcha 9. Kestrel connection limits are not set by default — unbounded connections cause resource exhaustion

**Concepts**
- `KestrelServerOptions.Limits.MaxConcurrentConnections` default is `null` (unlimited)
- `MaxConcurrentUpgradedConnections` for WebSocket and HTTP upgrade limits
- Thread pool and socket exhaustion under sustained attack or traffic spike
- Rate limiting middleware as the application-layer complement to Kestrel limits

**Answer**

Kestrel does not impose connection limits by default — `MaxConcurrentConnections` is `null`, meaning the server accepts as many simultaneous connections as system resources allow. Under a sustained connection flood or a slow-loris attack, this causes socket and thread pool exhaustion. Set a reasonable limit: `builder.WebHost.ConfigureKestrel(o => o.Limits.MaxConcurrentConnections = 10000)`. `MaxConcurrentUpgradedConnections` governs WebSocket and HTTP upgrade connections separately and should also be bounded. Kestrel-level limits are a first line of defense at the connection level; the rate limiting middleware (`AddRateLimiter`) operates at the request level and complements connection limits.

---

#### Gotcha 10. Generic Host vs `WebApplication.CreateBuilder` — mixing patterns causes duplicate service registrations

**Concepts**
- `WebApplication.CreateBuilder` as the minimal hosting API for ASP.NET Core 6+
- `Host.CreateDefaultBuilder().ConfigureWebHostDefaults()` as the Generic Host pattern
- Both patterns adding default services — mixing adds them twice
- Migration from Generic Host to minimal hosting as a one-time refactor

**Answer**

`WebApplication.CreateBuilder` is the preferred entry point for ASP.NET Core 6 and later and internally uses the generic host. The older `Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => ...)` pattern is still valid but verbose. Mixing the two — using `WebApplication.CreateBuilder` and then also calling `builder.Host.ConfigureWebHostDefaults(...)` — can register default services twice and produces confusing DI registration logs. Migration from the older pattern is a straightforward one-time refactor that simplifies `Program.cs` significantly. Worker services and console applications that do not serve HTTP requests should still use `Host.CreateDefaultBuilder` directly without the web host extension.

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
