# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/01. Introduction to ASP.NET Core`

---

#### Q1. (R) A developer ports a .NET Framework Web API to ASP.NET Core and keeps this controller pattern. What breaks at compile time and at runtime, and what architectural shift does ASP.NET Core require instead?

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

#### Q2. (P) Your team deploys the same ASP.NET Core API to Linux containers behind nginx and to Windows with IIS. Explain who runs application code, who terminates TLS, and what stays the same in `Program.cs` across both targets.

**Answer:** Kestrel always executes your ASP.NET Core application code and middleware pipeline; nginx or IIS typically terminates TLS and reverse-proxies to Kestrel, while `Program.cs` stays the same — only hosting configuration (URLs, forwarded headers, certificates) changes per environment.

- **Kestrel** is the cross-platform web server built into ASP.NET Core — it runs `Program.cs`, the middleware pipeline, and endpoint handlers.
- **nginx (Linux)** or **IIS (Windows)** often sits in front: handles TLS certificates, HTTP/2 edge features, rate limits, WAF rules, and load balancing across replicas.
- **IIS in-process** hosts the Core app inside the IIS worker process; **out-of-process** IIS forwards to a Kestrel listener — in both cases your app logic is still ASP.NET Core.
- `WebApplication.CreateBuilder` and middleware registration are identical; deployment differs via `ASPNETCORE_URLS`, `ForwardedHeaders`, certificate binding, and container `EXPOSE` ports.
- Health checks and structured logging should not assume Windows-specific paths or IIS modules.

**Production takeaway:** "Cross-platform" means the **same codebase** ships everywhere; ops chooses the edge server — Kestrel is not optional for running your app.

---

#### Q3. (R) Review this `Program.cs` from a tutorial copied into a staging environment. The app starts but health checks fail and Swagger is exposed publicly. What is wrong?

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

#### Q4. (M) A request hits `GET /api/orders/42` on a deployed ASP.NET Core app. Walk through the major stages from Kestrel accepting the socket through to the JSON response leaving the process — name the layers, not every middleware.

**Answer:** Kestrel parses HTTP and builds `HttpContext`, the middleware pipeline runs (possibly short-circuiting), routing selects the endpoint, the controller/minimal handler executes through DI, serialization writes the response, and Kestrel flushes bytes back through the reverse proxy to the client.

1. **Reverse proxy → Kestrel:** TLS may terminate at nginx/IIS; Kestrel receives HTTP, creates `HttpContext` with connection, request, and response features.
2. **Middleware chain:** Forwarded headers, exception handling, HTTPS redirect, routing, authentication, authorization — each can inspect or short-circuit before the endpoint.
3. **Endpoint routing:** Matcher selects `OrdersController.Get(42)` or a minimal route delegate based on method + path template.
4. **Handler execution:** DI resolves scoped services (e.g., repository), action runs, returns `IActionResult` or typed result.
5. **Result execution / serialization:** `System.Text.Json` (or configured formatter) writes JSON to the response body; middleware post-processing runs on the way out.
6. **Kestrel → client:** Response headers and body stream to the connection; proxy may add its own headers.

**Production takeaway:** Interviews test whether you see ASP.NET Core as a **composed pipeline ending in an endpoint** — not "controller first" as in classic System.Web.

---

#### Q5. (D) Product wants a small internal tool: one POST endpoint, one GET endpoint, no MVC views, team knows C# well. When would you choose **Minimal APIs** vs **controllers**, and what would make you regret Minimal APIs six months later?

**Answer:** Minimal APIs fit a handful of cohesive endpoints with lightweight DTOs and few cross-cutting MVC concerns; controllers pay off when validation, filters, versioning, OpenAPI grouping, and complex binding grow and you need established MVC conventions.

- **Choose Minimal APIs** when the service stays small (≤ ~10 endpoints), handlers are thin, team prefers colocated `Program.cs` routes, and you do not need action filters or complex model-binding scenarios.
- **Choose controllers** when you expect `[Authorize]` policies per action, validation filters, `ProblemDetails` conventions, API versioning, or multiple related resources sharing base behavior.
- **Regret triggers for Minimal APIs:** duplicated validation across routes, fat inline lambdas, testing friction without clear class boundaries, and OpenAPI/grouping clutter as endpoints multiply.
- **Either way** use DI, options pattern, and middleware for cross-cutting concerns — Minimal APIs are not "no architecture."
- Hybrid is valid: Minimal for health/internal ops, controllers for the public domain API.

**Production takeaway:** The decision is about **growth path and team conventions**, not raw line count — Karat wants trade-off reasoning, not "always controllers."

---

#### Q6. (R) A consultant claims "ASP.NET Core is just Kestrel — you don't need IIS or nginx." Review their deployment diagram assumptions. What production gaps appear when Kestrel is the only layer in front of your app?

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

#### Q7. (R) The unified hosting model (`WebApplication.CreateBuilder`) replaced `IWebHost` / `Startup.cs` for most new projects. Review this partial migration — what breaks middleware order, integration tests, and endpoint discovery?

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

#### Q8. (R) Review this cross-platform CI script comment and the accompanying `Program.cs` change. Builds pass on Windows agents but the container crashes on Linux with `Address already in use`. Diagnose and fix.

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
