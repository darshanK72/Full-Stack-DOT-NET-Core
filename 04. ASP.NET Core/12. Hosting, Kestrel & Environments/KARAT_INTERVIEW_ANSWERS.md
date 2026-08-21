# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/12. Hosting, Kestrel & Environments`

---

#### Q1. (M) Explain the roles of Kestrel, IIS, and a reverse proxy (nginx, YARP, Azure Front Door) in an ASP.NET Core deployment. Who terminates TLS, who runs your application code, and why do you often use more than one layer?

**Answer:** Kestrel is the cross-platform web server that runs your ASP.NET Core application and executes request handling code; IIS and reverse proxies sit in front to terminate TLS, route traffic, and apply platform policies before requests reach Kestrel.

- **Kestrel** listens for HTTP connections and drives the ASP.NET Core pipeline — controllers, middleware, and DI all run inside the Kestrel-hosted process.
- **IIS** (Windows) acts as reverse proxy (out-of-process) or hosts in-process via the ASP.NET Core Module; it provides Windows integration, process recycling, certificate binding, and request filtering without replacing Kestrel as the runtime host.
- A **reverse proxy** (nginx, YARP, Azure Front Door) typically terminates TLS at the edge, load-balances across instances, enforces WAF/rate limits, and forwards HTTP to Kestrel on an internal port.
- TLS termination usually happens at IIS or the edge proxy — Kestrel often receives plain HTTP internally even when clients use HTTPS.
- Multiple layers exist because each solves different problems: edge = scale/security, IIS = Windows ops, Kestrel = run .NET efficiently.

**Production takeaway:** Confusing who terminates TLS and who runs app code leads to broken redirects, wrong client IPs, and certificates on the wrong layer.

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

#### Q3. (M) When would you run Kestrel directly vs host in IIS in-process vs out-of-process? What problems does IIS still solve for Windows deployments even when Kestrel executes the .NET code?

**Answer:** Run Kestrel directly for containers and Linux; use IIS in-process for tight Windows integration and lowest latency; use IIS out-of-process for process isolation — IIS still provides recycling, Windows Auth integration, and centralized cert binding.

- **Kestrel direct:** Docker/Kubernetes, cross-platform, dev — you manage TLS, restarts, and reverse proxy yourself.
- **IIS in-process:** App runs inside IIS worker process via ANCM — lower overhead, shared Windows auth pipeline on traditional Windows servers.
- **IIS out-of-process:** Kestrel in separate dotnet process behind IIS — crash isolation, consistent with nginx patterns.
- IIS provides automatic app pool recycling, Windows Authentication, request filtering, and familiar ops tooling even when Kestrel executes .NET code.
- Choose by target: containers → Kestrel + edge proxy; legacy Windows farm → IIS + ANCM.

**Production takeaway:** Hosting model affects recycling, crash blast radius, and Windows auth — not just "which executable listens on port 80."

---

#### Q4. (P) How does `ASPNETCORE_ENVIRONMENT` (and `IHostEnvironment.EnvironmentName`) drive behavior at startup and per request? What breaks if Production is mis-set to Development on a public server?

**Answer:** `ASPNETCORE_ENVIRONMENT` sets `IHostEnvironment.EnvironmentName`, which controls conditional middleware (`DeveloperExceptionPage`), configuration loading (`appsettings.{Environment}.json`), logging levels, and EF migration behavior — mis-setting Production to Development exposes stack traces and verbose errors publicly.

- Read via `builder.Environment` / `app.Environment` — branch pipeline: `if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();`
- Loads `appsettings.Development.json` overrides when env is Development — may point to local DB or disable auth features.
- **Mis-set to Development in Production:** detailed exception pages, potentially swagger exposed, relaxed CORS from dev config, sensitive data in logs.
- Set via environment variable on the host (Azure App Setting, K8s manifest, Docker `-e`) — not only `launchSettings.json` (dev machine only).
- `IsProduction()`, `IsStaging()`, `IsEnvironment("Custom")` for custom deployment slots.

**Production takeaway:** Environment name is a security and reliability switch — treat `ASPNETCORE_ENVIRONMENT=Production` as mandatory checklist item for go-live.

---

#### Q5. (M) Configure Kestrel to listen on specific URLs and ports — `ASPNETCORE_URLS`, `ListenAnyIP`, HTTPS certificate binding. How does this differ from IIS binding and from container `EXPOSE` / Kubernetes `containerPort`?

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

#### Q6. (P) TLS is terminated at nginx; Kestrel receives plain HTTP on port 8080. What must be true in Kestrel, forwarded headers, and cookie/`SameSite` settings so redirects, HSTS, and secure cookies still behave correctly?

**Answer:** nginx must send `X-Forwarded-Proto: https`, ASP.NET Core must apply forwarded headers early with trusted proxy config, and cookie auth must mark cookies `Secure` based on perceived scheme — optionally disable Kestrel HTTPS redirection when TLS is entirely at the edge.

- Configure nginx: `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;` and `X-Forwarded-Proto $scheme;`
- `UseForwardedHeaders()` first with `ForwardedHeaders.XForwardedProto` so `Request.IsHttps` is true inside the app.
- `UseHttpsRedirection()` may still redirect HTTP clients hitting Kestrel directly on internal network — often disabled or restricted when only nginx is public.
- Auth cookies: `CookieSecurePolicy.Always` or `SameAsRequest` — with forwarded proto, `SameAsRequest` correctly marks Secure when client used HTTPS.
- HSTS is typically set at nginx/CDN; if set in ASP.NET, ensure middleware sees HTTPS via forwarded headers.
- Do not configure Kestrel HTTPS cert if TLS is fully offloaded — plain HTTP on internal port is normal.

**Production takeaway:** TLS offload without forwarded proto breaks cookie auth and generated URLs — the app must trust the edge's view of the client scheme.

---

#### Q7. (P) Implement host-level health checks for Kubernetes liveness vs readiness — `/health/live` vs `/health/ready` with DB dependency. Where do you register checks, and why should the liveness probe stay lightweight?

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

#### Q8. (P) A Docker image for your API fails health checks: the container listens on 8080 but orchestrator probes port 80. Trace configuration from `ASPNETCORE_URLS`, `WebApplication.Urls`, Kestrel `ListenOptions`, and the Dockerfile `EXPOSE` directive.

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

#### Q10. (D) Compare Development vs Production hosting configuration: hot reload, detailed errors, HTTPS dev certificates, logging verbosity, and forwarded headers trust. What belongs in `appsettings.Development.json` vs environment variables vs platform config?

**Answer:** Development optimizes developer feedback (detailed errors, dev certs, verbose logs); Production optimizes security and operability (sanitized errors, platform env vars, trusted proxy lists, structured logging) — never rely on `launchSettings.json` outside local machines.

- **Development-only:** `UseDeveloperExceptionPage`, hot reload (`dotnet watch`), user secrets, relaxed CORS, local HTTPS dev certificate trust, verbose logging in `appsettings.Development.json`.
- **Production:** `ASPNETCORE_ENVIRONMENT=Production`, exception handler + ProblemDetails, secrets from env/Key Vault (not JSON files), forwarded headers with explicit `KnownProxies`, Information/Warning log levels.
- **`appsettings.Development.json`:** local connection strings, feature flags for dev — never copied to Production publish output with secrets.
- **Environment variables / platform config:** connection strings, API keys, `ASPNETCORE_URLS`, proxy trust IPs — override JSON in Production per twelve-factor practice.
- **Forwarded headers:** never `KnownProxies.Clear()` in Production; Development behind local proxy may use narrower test config.

**Production takeaway:** Split config by environment at the host boundary — Production behavior must not depend on files that only exist on developer laptops.
