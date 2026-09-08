# Health Checks — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are health checks in ASP.NET Core Web API?](#q1-what-are-health-checks-in-aspnet-core-web-api)
2. [Q2. What is the difference between liveness and readiness probes?](#q2-what-is-the-difference-between-liveness-and-readiness-probes)
3. [Q3. What is `AddHealthChecks` used for?](#q3-what-is-addhealthchecks-used-for)
4. [Q4. What is `MapHealthChecks`?](#q4-what-is-maphealthchecks)
5. [Q5. What is `HealthCheckOptions.Predicate`?](#q5-what-is-healthcheckoptionspredicate)
6. [Q6. What are health check tags?](#q6-what-are-health-check-tags)
7. [Q7. Why should readiness checks include dependencies like SQL?](#q7-why-should-readiness-checks-include-dependencies-like-sql)
8. [Q8. Why should liveness checks avoid external dependencies?](#q8-why-should-liveness-checks-avoid-external-dependencies)
9. [Q9. What is `AddDbContextCheck`?](#q9-what-is-adddbcontextcheck)
10. [Q10. What happens if liveness and readiness use the same failing check?](#q10-what-happens-if-liveness-and-readiness-use-the-same-failing-check)
11. [Q11. What is HealthChecksUI?](#q11-what-is-healthchecksui)
12. [Q12. Should health endpoints be publicly accessible?](#q12-should-health-endpoints-be-publicly-accessible)
13. [Q13. What is a `ResponseWriter` in health checks?](#q13-what-is-a-responsewriter-in-health-checks)
14. [Q14. How do Kubernetes probes use health check endpoints?](#q14-how-do-kubernetes-probes-use-health-check-endpoints)
15. [Q15. What is `HealthStatus` (Healthy, Degraded, Unhealthy)?](#q15-what-is-healthstatus-healthy-degraded-unhealthy)
16. [Q16. What is the difference between `/health/live` and `/health/ready`?](#q16-what-is-the-difference-between-healthlive-and-healthready)
17. [Q17. What timeout considerations apply to health checks under load?](#q17-what-timeout-considerations-apply-to-health-checks-under-load)
18. [Q18. What information should external health endpoints expose?](#q18-what-information-should-external-health-endpoints-expose)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are health checks in ASP.NET Core Web API?

**Concepts**
- Health check endpoints reporting Healthy, Degraded, or Unhealthy status
- AddHealthChecks registering checks and MapHealthChecks mapping URLs
- Orchestrator and load balancer integration for routing and restart decisions
- IHealthCheck interface for custom domain-specific readiness logic
- Separate from business API endpoints — lightweight and fast

**Answer**

Health checks are endpoints that report whether the application and its dependencies are functioning. ASP.NET Core registers checks with `AddHealthChecks` in DI, maps probe URLs with `MapHealthChecks`, and returns `Healthy`, `Degraded`, or `Unhealthy` status for orchestrators and load balancers to act on. Built-in and third-party checks cover SQL, Redis, URLs, disk space, and custom logic via the `IHealthCheck` interface. Kubernetes, Azure App Service, and load balancers use these endpoints to decide whether to route traffic to or restart a pod. Health endpoints are separate from business API endpoints — they should respond quickly with minimal logic so they do not consume significant resources on probing intervals. Register checks in DI with tags classifying which probe endpoint they belong to, then map multiple endpoints with different predicates for live vs ready separation.

---

## Q2. What is the difference between liveness and readiness probes?

**Concepts**
- Liveness probe asking whether the process should be restarted
- Readiness probe asking whether the instance should receive traffic
- Liveness failure triggering pod restart in Kubernetes
- Readiness failure removing pod from load balancer without restart
- SQL checks on liveness causing restart loops during database maintenance

**Answer**

**Liveness** asks "should this process be restarted?" — it checks only that the app process is responsive and not deadlocked, not whether external dependencies are available. **Readiness** asks "should this instance receive traffic?" — it includes dependency checks like database and cache availability to ensure the instance can actually serve requests successfully. Liveness failure tells the orchestrator to kill and restart the container; readiness failure removes the instance from the load balancer pool without killing it. Putting SQL checks on liveness causes restart loops during database maintenance because the process is healthy but the dependency is down — restarting the pod does nothing to fix a database outage. Map separate URLs (`/health/live`, `/health/ready`) with tag-filtered predicates in ASP.NET Core 8 so the two probe types run different check subsets.

---

## Q3. What is `AddHealthChecks` used for?

**Concepts**
- AddHealthChecks registering the health check DI service and returning IHealthChecksBuilder
- Built-in AddDbContextCheck, AddRedis, AddUrlGroup registrations
- Tags classifying checks for predicate filtering on mapped endpoints
- Per-check timeout preventing one slow dependency from blocking the probe response
- Degraded status for optional dependencies that should warn but not fail readiness

**Answer**

`AddHealthChecks()` registers the health check service in DI and returns `IHealthChecksBuilder` to chain individual check registrations — built-in `AddDbContextCheck`, `AddRedis`, `AddUrlGroup`, or custom `IHealthCheck` implementations. Call `builder.Services.AddHealthChecks().AddCheck(...).AddDbContextCheck<TContext>(...)` in `Program.cs`. Tags classify checks so `MapHealthChecks` predicates can include or exclude them per endpoint — a check tagged `"ready"` runs on readiness probes and a check tagged `"live"` runs on liveness probes. Configure timeout per check to prevent one slow dependency from hanging the entire health response. Checks run when a health endpoint receives a request; they do not run continuously in the background unless using HealthChecksUI polling. Use `Degraded` status for optional dependencies that should warn without marking the instance fully `Unhealthy` and removing it from the load balancer.

---

## Q4. What is `MapHealthChecks`?

**Concepts**
- MapHealthChecks mapping a URL endpoint executing registered health checks
- 200 for Healthy/Degraded and 503 for Unhealthy HTTP status mapping
- HealthCheckOptions configuring which checks run and response writer format
- RequireAuthorization chained for detailed internal health routes
- Multiple mappings sharing one registration but filtering different check subsets

**Answer**

`MapHealthChecks(path, options)` maps a URL endpoint that executes registered health checks and writes the aggregated result as an HTTP response — typically returning 200 for Healthy and Degraded statuses and 503 for Unhealthy. Configure the checks that run on each endpoint via `HealthCheckOptions.Predicate`, the response body format via `ResponseWriter`, and access control via `.RequireAuthorization()` chained after the mapping. Multiple `MapHealthChecks` calls share the same registered check set but filter different subsets: `app.MapHealthChecks("/health/live", ...)` and `app.MapHealthChecks("/health/ready", ...)` each select their own subset via tag predicates. Place `MapHealthChecks` after routing setup; health endpoints bypass most business middleware when mapped simply, giving them the fast response time orchestrators need.

---

## Q5. What is `HealthCheckOptions.Predicate`?

**Concepts**
- Predicate as Func<HealthCheckRegistration, bool> selecting which checks run per endpoint
- Predicate = _ => false running no checks — process-only liveness pattern
- Null predicate running all registered checks — wrong for split K8s probes
- Tag-based predicate filtering enabling one registration set to power multiple endpoints
- Tags assigned at AddCheck registration time combined with Predicate at mapping time

**Answer**

`Predicate` is a `Func<HealthCheckRegistration, bool>` that filters which registered checks run for a specific mapped endpoint. Use `Predicate = c => c.Tags.Contains("ready")` to run only readiness checks on the readiness endpoint, and `Predicate = c => c.Tags.Contains("live")` for liveness. Setting `Predicate = _ => false` runs no checks — the endpoint returns Healthy as long as the process can respond to an HTTP request, which is the correct liveness pattern (any check would be redundant). A null predicate runs all registered checks, which is convenient for a combined dev dashboard but wrong for split Kubernetes probes because it causes both live and ready to fail simultaneously when any dependency is down. Tags are assigned at `AddCheck(..., tags: new[] { "ready" })` registration time and matched at mapping time, enabling one set of registrations to power `/health/live`, `/health/ready`, and `/health/db` without duplicating registrations.

---

## Q6. What are health check tags?

**Concepts**
- Tags as string labels attached to check registrations at setup time
- Classifying checks as "live", "ready", "db" for predicate filtering
- One check having multiple tags matching multiple endpoint predicates
- Untagged checks running on default /health with null predicate
- Tags as the primary liveness/readiness separation mechanism

**Answer**

Tags are string labels attached to health check registrations at setup time that classify checks for filtering on different mapped endpoints. One check can carry multiple tags — a Redis check tagged both `"ready"` and `"db"` runs on any endpoint whose predicate matches either tag. `AddCheck("self", () => Healthy(), tags: new[] { "live" })` creates a process-only self-check for liveness, while `AddDbContextCheck<T>(tags: new[] { "ready", "db" })` creates a dependency check for readiness and the detailed db endpoint. Untagged checks run on a default `/health` endpoint when the predicate is null. Tags are the primary mechanism for Kubernetes live/ready endpoint separation in ASP.NET Core 8 because they decouple the concern of "which checks exist" from "which endpoint executes which checks."

---

## Q7. Why should readiness checks include dependencies like SQL?

**Concepts**
- Readiness determining whether to send traffic to this instance
- SQL unavailability causing every API request to fail — correct to drain traffic
- Readiness failure as temporary — instance re-enters pool when checks pass
- Critical vs optional dependencies: Unhealthy vs Degraded status
- Probe timeout alignment with check latency to avoid false flapping

**Answer**

Readiness determines whether the instance should receive traffic. If SQL is unreachable, the API cannot serve database-backed requests correctly — every request will fail with a 500, so marking the instance not ready removes it from the load balancer pool without killing the process, giving the dependency time to recover. Readiness failure is temporary: the instance re-enters the pool automatically when the checks pass again, which is the correct behavior for a dependency outage. Include only critical dependencies — optional third-party APIs whose failure the app handles gracefully should use `Degraded` rather than `Unhealthy` so the instance stays in the pool with a warning rather than being drained. Align the Kubernetes probe `timeoutSeconds` with the actual check latency to avoid false flapping during normal load spikes; a timeout shorter than the P99 check latency causes intermittent false failures.

---

## Q8. Why should liveness checks avoid external dependencies?

**Concepts**
- Liveness failure triggering process restart — does not fix external dependencies
- Restart loop when liveness includes a dependency that is externally down
- Self-check sufficient for liveness — is the ASP.NET Core process responsive?
- External dependency failures belonging on readiness probes
- Simultaneous DB maintenance removing all replicas if liveness includes DB

**Answer**

Liveness failure triggers a process restart. If liveness includes SQL or Redis and a dependency goes down briefly for maintenance, Kubernetes kills and restarts pods that cannot fix the external problem — creating restart loops, cascading failures, and unnecessary downtime. Liveness should answer only: "Is the ASP.NET Core process hung or deadlocked?" A simple `() => HealthCheckResult.Healthy()` self-check or a lightweight HTTP response test suffices. External dependency failures belong on readiness — traffic drain without restart is the correct response. If all pods include the same external dependency on liveness and the DB goes into maintenance, all pods restart simultaneously, removing the entire fleet from service rather than just draining traffic until the dependency recovers.

---

## Q9. What is `AddDbContextCheck`?

**Concepts**
- AddDbContextCheck registering an EF Core database connectivity check
- EntityFrameworkCore health checks NuGet package
- Tagging with "ready" not "live" for correct probe placement
- Per-check timeout preventing slow DB probes from hanging the health response
- Lightweight CanConnect query rather than migration scan for check query

**Answer**

`AddDbContextCheck<TContext>()` registers a health check that verifies EF Core can connect to the database — typically executing a lightweight connection test or `CanConnect` query against the configured `DbContext`. Add the package `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` and call `.AddDbContextCheck<OrdersDbContext>(name: "sql", tags: new[] { "ready" })` — tag it `"ready"` not `"live"` so it runs only on the readiness probe. Configure a timeout: `timeout: TimeSpan.FromSeconds(3)` to prevent a slow database from keeping the health probe open for tens of seconds under load. The check uses the configured connection string and pooling settings, so ensure the probe query is cheap — `CanConnect` or `SELECT 1` — not a full migrations scan. False failures under load often indicate a probe timeout mismatch with the Kubernetes `timeoutSeconds` rather than an actual database problem.

---

## Q10. What happens if liveness and readiness use the same failing check?

**Concepts**
- Both probes failing simultaneously causing restart and traffic drain together
- Kubernetes killing pods that cannot fix an external dependency outage
- Restart adding startup load during dependency recovery — delaying readiness
- Separate endpoints as the required solution
- Restart count monitoring separately from readiness failure count

**Answer**

When a dependency like SQL fails and both probes point at the same endpoint containing SQL checks, Kubernetes restarts the pod (liveness failure) **and** removes it from service endpoints (readiness failure) simultaneously. Restarts do not fix SQL outages, so pods enter a crash loop while traffic is also drained — the worst combined outcome from one misconfiguration. Brief SQL blips kill pods across the entire fleet, and during recovery each pod must restart and pass its startup probe before readiness can pass, adding several seconds of startup latency to the recovery path. Fix by separating endpoints: liveness runs only a self-check tagged `"live"`, readiness runs dependency checks tagged `"ready"`. Monitor restart counts separately from readiness failure counts to detect this misconfiguration in production — unusual restart counts with simultaneous readiness failures is the signature.

---

## Q11. What is HealthChecksUI?

**Concepts**
- HealthChecksUI providing a web dashboard polling health endpoints
- Verbose UIResponseWriter output intended for internal operator use
- AddHealthChecksUI and MapHealthChecksUI registration
- Polling interval creating continuous query load on health endpoints
- Internal network or authenticated access required — not for public exposure

**Answer**

HealthChecksUI is a NuGet package that provides a web dashboard which polls registered health endpoints and displays history, status, and per-dependency data over time. Register with `AddHealthChecksUI()` and `MapHealthChecksUI()`, configure the health URLs to poll and the polling interval (e.g., every 2 seconds), and choose an in-memory or SQL backend for history storage. The dashboard is intended for operators on internal networks — it uses `UIResponseWriter.WriteHealthCheckUIResponse` which produces verbose JSON including dependency names, failure messages, exception details, and duration. Public exposure of HealthChecksUI leaks dependency names, failure messages, and infrastructure topology. Restrict it to VPN access, admin authentication, or disable it entirely in production public APIs. The polling interval also adds continuous query load to dependency checks, which is significant if health checks hit real SQL.

---

## Q12. Should health endpoints be publicly accessible?

**Concepts**
- Minimal liveness/readiness endpoints unauthenticated for orchestrator access
- Verbose health JSON with failure details as information disclosure risk
- Custom minimal ResponseWriter for external-facing routes
- Rate limiting health endpoints at the gateway to prevent abuse
- Detailed /health/db and HealthChecksUI requiring authentication and internal network

**Answer**

Minimal liveness and readiness endpoints are often unauthenticated so orchestrators and load balancers can probe them without tokens, but responses must contain only aggregate status — `{ "status": "Healthy" }` — not connection strings, exception details, or per-dependency failure messages. Detailed health and HealthChecksUI must require authentication and internal network access. A public `/health/ready` with only the aggregate status string is acceptable; a public verbose JSON body listing SQL error messages, Redis key counts, and migration state is an information disclosure risk. Use a custom `ResponseWriter` on external-facing routes that strips the `data` dictionary and returns only `{ "status": "..." }`. Rate-limit health endpoints at the gateway to prevent abuse-driven load — health probes hitting SQL on every request become a denial-of-service vector if an attacker hits the endpoint rapidly.

---

## Q13. What is a `ResponseWriter` in health checks?

**Concepts**
- ResponseWriter as Func<HttpContext, HealthReport, Task> controlling the response body
- Default writer including per-check duration, status, and exception messages
- Custom minimal writer for external public routes
- UIResponseWriter.WriteHealthCheckUIResponse for HealthChecksUI dashboard
- Stable simple output for orchestrators that parse only aggregate status

**Answer**

`ResponseWriter` is a delegate on `HealthCheckOptions` with signature `Func<HttpContext, HealthReport, Task>` that controls the HTTP response body format when health checks complete. The default writer includes per-check name, duration, status, and exception messages — verbose enough for internal diagnostics but too much for public exposure. Replace it for external-facing routes with a minimal writer: `async (ctx, report) => await ctx.Response.WriteAsJsonAsync(new { status = report.Status.ToString() })`. HealthChecksUI uses `UIResponseWriter.WriteHealthCheckUIResponse` which produces detailed structured JSON — appropriate only for the internal dashboard. Keep public writers stable and simple since orchestrators parse only the aggregate status or the HTTP status code rather than the full HealthReport schema. The HTTP status code itself (200 vs 503) is the primary signal; the body is supplementary.

---

## Q14. How do Kubernetes probes use health check endpoints?

**Concepts**
- livenessProbe and readinessProbe as HTTP GET requests to health URLs
- Failed liveness restarting the container, failed readiness removing from Service endpoints
- initialDelaySeconds, periodSeconds, timeoutSeconds, failureThreshold configuration
- Startup probe allowing slow EF migration boot before live/ready take over
- timeoutSeconds exceeding worst-case check latency to prevent false failures

**Answer**

Kubernetes configures `livenessProbe` and `readinessProbe` (and optionally `startupProbe`) as HTTP GET requests to mapped ASP.NET Core health URLs. Failed liveness causes Kubernetes to kill and restart the container; failed readiness removes the pod from Service endpoints without restarting. Map `httpGet.path: /health/live` for liveness and `/health/ready` for readiness in the pod spec. Configure `initialDelaySeconds` to give the app time to start before probing begins, `periodSeconds` to set the probe frequency, `timeoutSeconds` to set the maximum wait per probe (must exceed P99 dependency latency), and `failureThreshold` to control how many consecutive failures trigger action. A startup probe allows slow EF Core migration runs at boot before liveness and readiness probes take over — the pod is not killed during migrations. `timeoutSeconds` is the single most important setting to tune correctly; set it to exceed the 99th percentile health check latency under load to prevent false failures during traffic spikes.

---

## Q15. What is `HealthStatus` (Healthy, Degraded, Unhealthy)?

**Concepts**
- Healthy, Degraded, Unhealthy as the three HealthCheckResult values
- Aggregate report reflecting the worst individual check status
- Degraded for optional dependencies — warning without removing from load balancer
- Unhealthy for critical dependencies — removes instance from service pool
- HTTP 200 for Healthy/Degraded and 503 for Unhealthy by default

**Answer**

`HealthStatus` is the enum result of each health check — **Healthy** (everything working), **Degraded** (partial impairment but still operational), or **Unhealthy** (failure). The aggregate `HealthReport.Status` reflects the worst individual check: one `Unhealthy` check makes the whole report `Unhealthy`. HTTP mapping is 200 for Healthy and Degraded statuses and 503 for Unhealthy by default, though you can customize this in `HealthCheckOptions.ResultStatusCodes`. Use `Degraded` for optional dependencies — a cache miss fallback or a slow secondary service — where the instance can still handle requests without the dependency. Use `Unhealthy` only for critical dependencies whose absence makes the instance unable to serve requests, since `Unhealthy` on a readiness probe removes the pod from the load balancer. Document your team's convention for load balancer behavior on Degraded status, as some infrastructure treats it as 200 (pass) and some as 503 (fail).

---

## Q16. What is the difference between `/health/live` and `/health/ready`?

**Concepts**
- /health/live running self-check only — pod restart signal
- /health/ready running dependency checks — traffic routing signal
- Different MapHealthChecks calls with different Predicate tag filters
- Live response in milliseconds vs ready allowing 1–3 seconds for DB ping
- Never pointing both K8s probes at the same URL containing dependency checks

**Answer**

`/health/live` runs liveness checks — process self-check only, no external dependencies — telling Kubernetes whether to restart the container. `/health/ready` runs readiness checks including SQL, Redis, and other dependencies — telling the load balancer whether to send traffic. They are separate `MapHealthChecks` registrations with different `Predicate` tag filters: live selects only `"live"`-tagged checks and ready selects only `"ready"`-tagged checks. Live should respond in milliseconds since it has no network calls; ready may take 1–3 seconds for a database ping. Never point both Kubernetes probes at the same URL that includes dependency checks — when the dependency fails, both probes fail simultaneously, causing pods to be restarted and drained at the same time rather than just drained. Document both URLs in deployment runbooks and align `timeoutSeconds` in the pod spec to match each endpoint's expected latency.

---

## Q17. What timeout considerations apply to health checks under load?

**Concepts**
- P99 dependency latency under load often exceeding default probe timeouts
- K8s timeoutSeconds must exceed worst-case check latency to prevent false flapping
- Per-check timeout in AddDbContextCheck preventing one slow check blocking the probe
- Increasing periodSeconds and failureThreshold to absorb brief blips
- Expensive health check queries adding load during traffic spikes — use lightweight queries

**Answer**

Under traffic spikes, dependency checks especially SQL slow down. If the Kubernetes probe `timeoutSeconds` is shorter than the check latency at P99 under load, readiness flaps — pods leave and re-enter the load balancer pool repeatedly, amplifying the load spike. Set `timeoutSeconds` to at least the P99 dependency latency, often 3–5 seconds for readiness and 1–2 seconds for liveness. Configure a per-check timeout in `AddDbContextCheck(..., timeout: TimeSpan.FromSeconds(3))` so a slow check fails predictably rather than blocking the probe response indefinitely. Increase `failureThreshold` to 3–5 to absorb brief blips without immediate removal — a single slow probe should not drain the pod. Health check queries themselves add to the SQL load during spikes, so use the lightest possible query (connection test, `SELECT 1`) rather than table scans or migration status queries. Consider caching the last successful readiness result for a few seconds via a wrapper check to reduce SQL probing frequency under high probe load.

---

## Q18. What information should external health endpoints expose?

**Concepts**
- Aggregate status only: Healthy/Unhealthy/Degraded and optional build version
- Never exposing connection strings, stack traces, or internal hostnames externally
- Custom minimal ResponseWriter for external routes vs verbose for internal
- HealthChecksUI and UIResponseWriter as internal-only outputs
- traceId unnecessary on health endpoints — keep payload tiny for fast probes

**Answer**

External-facing health endpoints should expose only aggregate status (`Healthy` / `Unhealthy`), an optional non-sensitive version or build id useful for deployment verification, and perhaps a timestamp — never connection strings, stack traces, internal hostnames, migration errors, Redis key counts, or per-dependency exception messages. Internal ops endpoints may expose dependency names, durations, and failure messages behind authentication and internal network access. Use a custom minimal `ResponseWriter` for public routes that returns only `{ "status": "Healthy", "version": "1.2.3" }` and the verbose `UIResponseWriter` only for the internal health dashboard. `traceId` is unnecessary on health endpoints since probes are automated rather than user-initiated; keep payloads tiny for fast probes. Apply the same security review to verbose health endpoints as to debug endpoints — they reveal infrastructure topology and failure modes that inform attack planning.

---

## Gotchas — Health Checks (Interview Traps)

---

#### Gotcha 1. SQL health check in liveness probe causing restart cascade

**Concepts**
- Liveness probe — Kubernetes kills and restarts the pod on failure
- Readiness probe — removes pod from load balancer rotation until ready
- SQL outage triggering liveness failure — all pods restart, cannot fix external dependency
- `/health/live` (in-process only) vs `/health/ready` (SQL, Redis, dependencies)

**Answer**

A liveness probe answers whether the process itself should be killed and restarted — it should only check things a restart can fix, like a deadlock or a corrupted in-process state. Adding a SQL connectivity check to liveness means every pod restarts when the database goes down, producing a restart loop that exhausts pod restart budgets without making any progress. SQL, Redis, and external HTTP dependency checks belong on the readiness probe, which removes the pod from load balancer rotation until the dependency recovers. Restarting the pod cannot fix a database outage.

---

#### Gotcha 2. Health endpoint requiring authentication — blocking Kubernetes probes

**Concepts**
- Kubernetes liveness/readiness probes — unauthenticated HTTP requests from kubelet
- `[Authorize]` on health controller or global auth filter blocking 200 response
- `RequireAuthorization()` on `MapHealthChecks` — must use explicit anonymous access
- Separate anonymous health path vs authenticated detailed health path

**Answer**

Kubernetes health probes are unauthenticated HTTP requests from the kubelet — they cannot carry a JWT or cookie. If a global authentication policy or `[Authorize]` attribute is applied to the health check endpoints, probes receive `401 Unauthorized` and the pod is killed or removed from rotation. I configure `MapHealthChecks("/health/live")` without `RequireAuthorization()` for basic probes, and provide a separate authenticated endpoint (e.g. `/health/detailed`) for internal monitoring tools that need the full check output with sensitive details about individual service states.

---

#### Gotcha 3. Slow health check degrading API response time

**Concepts**
- Synchronous health check blocking the thread pool
- Health check timeout — default no timeout; slow DB query blocks forever
- `HealthCheckContext.CancellationToken` — respect cancellation from the probe deadline
- Caching health check results — `MapHealthChecks` with `CacheDuration` option

**Answer**

A health check that executes a full-table SQL query or waits for a slow external HTTP call can take seconds per probe. Under high-frequency probing combined with concurrent requests, health checks consume thread pool capacity and degrade API throughput. I use lightweight connectivity checks (`SELECT 1` rather than business queries), respect the `CancellationToken` in `CheckHealthAsync` to honor probe timeouts, and configure a cache duration on `MapHealthChecks` options so repeated rapid polls return the cached result rather than re-running every check on each probe call.

---

#### Gotcha 4. `HealthStatus` mapping to non-standard HTTP status codes

**Concepts**
- Default: `Healthy` and `Degraded` both return HTTP 200; `Unhealthy` returns 503
- `Degraded` returning 200 — load balancer treats pod as fully healthy despite degradation
- Custom `ResultStatusCodes` mapping `Degraded` to 207 or 503 for stricter routing
- Load balancer health check — only respects HTTP 200/non-200, not JSON body content

**Answer**

By default, `Degraded` status maps to HTTP 200, so a load balancer health check sees a degraded pod as fully healthy and continues sending traffic. Depending on what "degraded" means (one secondary dependency unavailable vs primary cache miss), returning 200 may be correct or may send traffic to a pod that cannot serve it correctly. I configure `ResultStatusCodes` in `MapHealthChecks` to map `Degraded` to `207 Multi-Status` (distinguishable from success) or `503 Service Unavailable` for strict setups where any degradation should remove the pod from rotation.

---

#### Gotcha 5. `AddDbContextCheck` exhausting the connection pool

**Concepts**
- `AddDbContextCheck<TContext>()` — opens a connection and executes `CanConnectAsync()`
- Probe frequency times pod count times connection pool size — pool exhaustion under high-frequency probing
- Short-lived connection per health check — connections returned to pool, but rapid probing opens many
- Single dedicated low-pool-size DbContext for health checks vs sharing with request pool

**Answer**

`AddDbContextCheck<AppDbContext>()` opens a database connection on every probe to verify connectivity. With a 10-second probe interval and 20 pods, that is 2 connections per second just for health checks. In combination with request traffic, this can exhaust the connection pool, causing health checks to fail not because the database is down but because the pool is exhausted — a self-inflicted false negative that removes all pods from rotation. I register a separate minimal `DbContext` with a pool size of 2 dedicated to health checks, isolated from the request-serving connection pool.

---

#### Gotcha 6. Startup probe vs liveness probe — pod killed before app warms up

**Concepts**
- Startup probe — disables liveness and readiness until startup succeeds; prevents kill during warmup
- Without startup probe — liveness fails during slow startup (migrations, warm-up), pod restarted repeatedly
- `failureThreshold` times `periodSeconds` — startup probe timeout window
- `MapHealthChecks("/health/startup")` for a dedicated startup-complete check

**Answer**

On slow-starting applications — those that run database migrations, warm up caches, or compile Roslyn expressions at startup — liveness probes may fire before the app is ready to respond, causing the pod to be killed and restarted in a loop. The startup probe prevents this by disabling liveness and readiness checks until the startup probe succeeds, effectively giving the app an extended grace period. I map a `/health/startup` endpoint that returns `Healthy` only after all startup tasks complete, configure Kubernetes with `startupProbe` using a long timeout, and switch to the standard liveness probe thereafter.

---

#### Gotcha 7. Health check tags not used for readiness-only filtering

**Concepts**
- `AddCheck<T>("check", tags: new[] { "ready" })` — tag categorizes the check
- `MapHealthChecks("/health/ready", opts => opts.Predicate = c => c.Tags.Contains("ready"))` — filters by tag
- Without predicate — all checks run on all endpoints regardless of probe type
- `live` tag for lightweight checks; `ready` tag for dependency checks

**Answer**

Without filtering by tags, every registered health check runs on every health endpoint — liveness, readiness, and startup probes all execute SQL, Redis, and external HTTP checks. This negates the liveness vs readiness separation because the liveness probe still fails when SQL is down even if it only runs the SQL check because all checks run on all paths. I tag checks as `"ready"` or `"live"` and use `Predicate = c => c.Tags.Contains("ready")` on `MapHealthChecks("/health/ready")` so each endpoint runs only the checks appropriate for its probe type.

---

#### Gotcha 8. Health check response exposing sensitive internal details to public

**Concepts**
- Default JSON response — includes check names, descriptions, exception messages
- Exception message revealing internal topology (server names, connection strings)
- `Predicate` and `ResponseWriter` customization — control what is exposed
- Authenticated detailed endpoint vs anonymous summary endpoint

**Answer**

The default `HealthCheckOptions.ResponseWriter` serializes the full check result including exception messages, which may include the database server name, connection string fragments, or internal network hostnames. This is exposed to any caller who can reach the health endpoint — including internet-facing probes on a public API. I use a custom `ResponseWriter` on the public health endpoint that returns only `{"status":"Healthy"}` or `{"status":"Unhealthy"}` without any detail, and provide a separate authenticated endpoint with full JSON output for internal use by operations teams.

---

#### Gotcha 9. Non-representative health check — database accessible but app logic broken

**Concepts**
- `SELECT 1` confirms connectivity but not that business queries work
- Health check passing while migrations are missing or schema is wrong
- Business-critical query as health check — lightweight SELECT from key table
- "Health" from the user's perspective vs infrastructure perspective

**Answer**

A health check that only verifies connectivity (`SELECT 1` or `CanConnectAsync()`) returns `Healthy` even when the database is missing required tables (due to missing migrations), or query indexes are dropped causing timeouts. A pod can pass all health checks while being completely unable to serve business requests. I add a simple business-representative query to the readiness check — a fast `COUNT(1)` from the most-used table — which validates that the schema is correct and queries execute within a reasonable time, not just that the TCP connection is open.

---

#### Gotcha 10. `IHealthCheck` returning `Unhealthy` on transient failure without retry

**Concepts**
- Single transient network hiccup — check returns `Unhealthy`, pod removed from rotation
- Retry logic or tolerance in health check vs instant failure
- `Degraded` for marginal but functional state vs `Unhealthy` for fully non-functional
- `HealthCheckOptions.Predicate` and Kubernetes `failureThreshold` for tolerance

**Answer**

A health check that calls an external HTTP API and immediately returns `Unhealthy` on a single timeout may cause the pod to be removed from rotation due to a 500ms network glitch that the application would have retried successfully in normal request processing. I build tolerance into the health check by retrying once with a short delay before returning `Unhealthy`, or by returning `Degraded` for a single failure and `Unhealthy` only after two consecutive failures. Kubernetes `failureThreshold: 3` on the readiness probe adds a second layer of tolerance, keeping the pod in rotation through transient network instability.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (P) Your orders Web API runs on Kubernetes. Explain **liveness** vs **readiness** probes, which dependencies belong in each, and what happens if you point both probes at the same `/health` endpoint that includes SQL and Redis checks.

**Concepts**
- Liveness — restart signal for a hung or deadlocked process
- Readiness — traffic drain signal when dependencies are unavailable
- SQL and Redis as readiness-only dependencies
- Combined endpoint causing simultaneous restart and drain during outages
- Separate /health/live and /health/ready with tag predicates

**Answer**

Liveness answers "is this process hung or deadlocked?" — Kubernetes restarts the container when it fails. Readiness answers "is this instance ready to serve traffic?" — Kubernetes removes the pod from Service endpoints without restarting when it fails. SQL and Redis belong on readiness only: if they go down, the process is still healthy (liveness passes, no restart) but unable to serve requests correctly (readiness fails, pod drained from the pool until dependencies recover). If both probes point at the same `/health` endpoint that includes SQL and Redis checks, a database maintenance window causes both probes to fail simultaneously — Kubernetes restarts every pod (liveness failure) while also draining them (readiness failure). Restarting does nothing to fix the database, so pods enter crash loops: they restart, hit the liveness probe which fails again immediately because SQL is still down, and restart again. This cascades across the entire fleet. The correct setup is `app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = c => c.Tags.Contains("live") })` running a lightweight self-check, and `app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") })` running SQL and Redis checks.

---

#### Q2. (R) Review health check registration. After deploy, pods flap: readiness fails intermittently, logs show `TaskCanceledException` during EF check. SQL is healthy but latency spikes to 2–3 seconds under load.

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>(
        name: "sql",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" });

builder.Services.AddDbContext<OrdersDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

No custom timeout configured. K8s `readinessProbe.timeoutSeconds: 1`.

**Concepts**
- K8s timeoutSeconds: 1 shorter than DB check latency under load causing false failures
- AddDbContextCheck default timeout matching or exceeding K8s timeout
- TaskCanceledException from probe timeout cancelling the DB check in-flight
- periodSeconds and failureThreshold absorbing brief latency spikes
- False readiness flapping draining pods unnecessarily during traffic spikes

**Answer**

The Kubernetes `readinessProbe.timeoutSeconds: 1` is shorter than the SQL check latency under load (2–3 seconds). When the probe fires during a traffic spike, Kubernetes sends an HTTP GET to `/health/ready` and cancels the request after 1 second; the `AddDbContextCheck` is still in-flight waiting for SQL, and the cancellation propagates as a `TaskCanceledException` which the health check framework reports as `Unhealthy` rather than a true SQL failure. The pod is then removed from the load balancer even though SQL is healthy and the app is serving traffic correctly — only the probe timed out. The registration has no custom timeout configured, so it uses the health check default which can exceed Kubernetes's 1-second deadline. There are two fixes to apply together: first, add `timeout: TimeSpan.FromMilliseconds(800)` to the `AddDbContextCheck` registration so the check fails fast before Kubernetes cancels the probe (giving 200 ms margin); second, raise `readinessProbe.timeoutSeconds` in the K8s deployment to at least 4–5 seconds to accommodate P99 DB latency under load. Also increase `failureThreshold` to 3 so a single slow probe does not immediately drain the pod.

---

#### Q3. (D) Security review: `/health` returns JSON listing connection string server name, migration errors, and Redis key counts. Should health endpoints be public? Compare **`ResponseWriter`** minimal output vs **`HealthChecksUI`** detailed dashboard — what do you expose externally vs keep internal?

**Concepts**
- Verbose health JSON disclosing infrastructure details to external attackers
- Custom minimal ResponseWriter for public routes stripping sensitive data
- UIResponseWriter as internal-only output for operator dashboards
- Defense-in-depth: minimal external output plus authenticated internal endpoint
- Information disclosure from migration errors and connection strings

**Answer**

The current `/health` response is an information disclosure vulnerability — the connection string server name, migration error details, and Redis key counts tell an attacker the database host, schema state, and cache sizing without any authentication. Health endpoints should not expose this data externally. The right design has two endpoints. The first is a public minimal endpoint: `app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = async (ctx, report) => await ctx.Response.WriteAsJsonAsync(new { status = report.Status.ToString() }) })` — returns only `{ "status": "Healthy" }` or `{ "status": "Unhealthy" }` with no per-dependency data, suitable for external monitoring and load balancer probes. The second is an authenticated internal endpoint: `app.MapHealthChecks("/health/internal", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }).RequireAuthorization("InternalOnly")` — returns the full verbose JSON with dependency names, durations, and errors, accessible only from internal network or with an admin token. The HealthChecksUI dashboard (`/health-ui`) should also require authentication and be restricted to internal network access. Migration error details in particular should never reach external logs or responses since they reveal table structure and schema version.

---

#### Q4. (P) Map ASP.NET Core health checks to **Kubernetes probes** — provide example paths, `Predicate`/tags for liveness vs readiness, and recommended `failureThreshold`, `periodSeconds`, and `timeoutSeconds` values for a database-backed API.

**Concepts**
- /health/live with "live" tag predicate for liveness probe
- /health/ready with "ready" tag predicate for readiness probe
- failureThreshold absorbing brief blips before action
- timeoutSeconds exceeding P99 health check latency under load
- initialDelaySeconds allowing startup before probing begins

**Answer**

Register checks with tags in `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<AppDbContext>(name: "sql", timeout: TimeSpan.FromSeconds(4), tags: new[] { "ready" })
    .AddRedis(redisConn, name: "redis", timeout: TimeSpan.FromSeconds(3), tags: new[] { "ready" });

app.MapHealthChecks("/health/live", new HealthCheckOptions
    { Predicate = c => c.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
    { Predicate = c => c.Tags.Contains("ready") });
```

In the Kubernetes deployment spec:

```yaml
livenessProbe:
  httpGet: { path: /health/live, port: 8080 }
  initialDelaySeconds: 10
  periodSeconds: 10
  timeoutSeconds: 2
  failureThreshold: 3

readinessProbe:
  httpGet: { path: /health/ready, port: 8080 }
  initialDelaySeconds: 15
  periodSeconds: 15
  timeoutSeconds: 5
  failureThreshold: 3
```

`timeoutSeconds: 2` for live is safe since the self-check has no network calls. `timeoutSeconds: 5` for ready accommodates P99 DB latency under load; the per-check `timeout: TimeSpan.FromSeconds(4)` ensures the check fails fast before Kubernetes cancels the probe. `failureThreshold: 3` means 3 consecutive failures before action, absorbing brief spikes. `initialDelaySeconds: 15` on readiness allows EF Core startup work to complete before probing begins.

---

#### Q5. (M) You register multiple checks with tags `ready`, `live`, and `db`. Explain how **`HealthCheckOptions.Predicate`** filters which checks run per endpoint, and write registration for three routes: `/health/live`, `/health/ready`, `/health/db` (db-only, internal).

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddRedis(redisConn, name: "redis", tags: new[] { "ready", "db" })
    .AddDbContextCheck<AppDbContext>(tags: new[] { "ready", "db" });
```

**Concepts**
- Predicate as Func<HealthCheckRegistration, bool> running per registered check
- Tags.Contains() matching to include specific checks per endpoint
- One check with multiple tags running on multiple endpoints
- /health/db running only "db"-tagged checks for detailed internal diagnostics
- RequireAuthorization chained for the internal db endpoint

**Answer**

`HealthCheckOptions.Predicate` is a `Func<HealthCheckRegistration, bool>` evaluated for each registered check when an endpoint is hit — if it returns `true` for a registration, that check runs; if `false`, it is skipped. Given the registrations above: `"self"` has tag `"live"`, `"redis"` has tags `"ready"` and `"db"`, and the DbContext check has tags `"ready"` and `"db"`. The three endpoint mappings are:

```csharp
// /health/live — only "self" check runs
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("live")
});

// /health/ready — "redis" and DbContext checks run; "self" skipped
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready")
});

// /health/db — only "redis" and DbContext checks run (same as ready here)
// internal, requires auth
app.MapHealthChecks("/health/db", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).RequireAuthorization("InternalOnly");
```

`/health/live` returns Healthy quickly from the self-check alone. `/health/ready` runs both dependency checks. `/health/db` runs the same dependency checks but with verbose `UIResponseWriter` output for operator diagnostics, protected by authorization so it is never publicly accessible.

---

#### Q6. (R) Review Production `Program.cs`. Health Checks UI is reachable at `/health-ui` without authentication and polls `/health` every 2 seconds from the public internet.

```csharp
builder.Services.AddHealthChecksUI(setup =>
{
    setup.AddHealthCheckEndpoint("https://api.company.com/health", "Production API");
}).AddInMemoryStorage();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
```

**Concepts**
- UIResponseWriter exposing dependency names, errors, and infrastructure details publicly
- /health-ui accessible without authentication disclosing internal topology
- 2-second poll from public internet adding continuous load on health checks and SQL
- Information disclosure through verbose health JSON to unauthenticated callers
- Environment check or RequireAuthorization required for both /health verbose and /health-ui

**Answer**

There are three distinct issues. First, `/health` uses `UIResponseWriter.WriteHealthCheckUIResponse` which produces verbose JSON including dependency names, failure exception messages, and per-check durations — any unauthenticated caller can read SQL server names, Redis connection errors, or migration failure messages from the public internet. Second, `/health-ui` is mapped without `RequireAuthorization()`, so the full HealthChecksUI dashboard is publicly accessible and exposes a visual history of infrastructure failures including dependency names and error messages. Third, `setup.AddHealthCheckEndpoint("https://api.company.com/health", "Production API")` polls the production URL every 2 seconds from wherever HealthChecksUI runs — if this runs on the same server as the API, that is a `/health` request with a DB ping every 2 seconds on the production database, adding continuous background load. The fixes: change the `/health` `ResponseWriter` to minimal output for public access; add `.RequireAuthorization("OpsTeam")` to the `MapHealthChecksUI` call; restrict the HealthChecksUI poll target to an internal URL rather than the public API domain; wrap both in an environment check so they do not run in Production public deployments at all; and increase the poll interval to at least 30 seconds if kept.

---

#### Q7. (D) Team debates one **`/health`** endpoint vs split **`/health/live`** and **`/health/ready`**. Trade-offs for Web APIs behind K8s, load balancers, and Azure App Service — when does a single combined check cause outages or false positives?

**Concepts**
- Combined endpoint causing simultaneous restart and drain during dependency outages
- Azure App Service warmup probe vs Kubernetes liveness/readiness distinction
- False liveness failure from dependency latency causing unnecessary restarts
- Separate endpoints enabling independent probe timeout tuning
- Single endpoint appropriate for simple non-orchestrated deployments

**Answer**

A single combined `/health` endpoint is appropriate for simple deployments behind a basic load balancer that only cares about "is the app up?" — the load balancer removes the instance on failure and restores it on recovery. For Kubernetes, a single combined endpoint with dependency checks causes the failure described in Q1: when SQL is unavailable, liveness and readiness fail simultaneously, triggering restarts that cannot fix the dependency and adding startup overhead to the recovery path. For Azure App Service, the health check endpoint is used for instance removal from the load balancer (similar to readiness) but not for restarts, so a combined check is less dangerous — but SQL failures still cause unnecessary instance removal from scale-out groups during brief spikes. The case for split endpoints: independent timeout tuning (liveness needs 2s; readiness needs 5s for DB), clear semantic separation (is the process hung vs is it ready to serve), ability to add new dependency checks to readiness without risking liveness-triggered restarts, and explicit documentation of what each probe checks. The case for a single combined endpoint: simpler setup, fewer URLs to maintain, appropriate for non-orchestrated deployments without container restart semantics. For any Kubernetes-deployed API with SQL or Redis dependencies, split endpoints are the correct choice — the cost is two `MapHealthChecks` calls and two entries in the pod spec.
