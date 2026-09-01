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

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

#### Gotcha 1. POST returning 200 instead of 201

**Concepts**
- HTTP 201 Created with Location header as REST create contract
- CreatedAtAction / CreatedAtRoute for correct response
- Resource discovery via Location header
- Status code semantics for OpenAPI-generated clients

**Answer**

A successful resource creation with POST should return HTTP 201 Created and a `Location` header pointing at the new resource URL, because 200 OK carries no hint that a new resource was created or where to find it. Standard HTTP clients, API gateways, and OpenAPI-generated SDKs all look at the status code first — returning 200 means the response body is the only way to discover the new resource id, and clients that skip parsing the body miss it entirely. Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with the Location header, and include the created representation or a minimal payload in the body when clients need immediate data without a follow-up GET.

---

#### Gotcha 2. GET that mutates state

**Concepts**
- GET as safe and idempotent per HTTP specification
- Prefetch and crawler risks from side-effecting GETs
- Caching proxy behavior replaying GET responses
- Correct HTTP verbs for state-changing operations

**Answer**

GET must be safe and idempotent per HTTP semantics — performing deletes or updates in a GET handler violates the specification, breaks caching proxies that may replay GET responses, and creates security holes when URLs are prefetched by browsers, link-preview crawlers, or email clients. The problem is that these callers invoke GET URLs without user intent, so a delete fires without anyone clicking anything. Cached GET responses can replay destructive operations across clients since the proxy treats the response as a normal cacheable resource. Use POST, PUT, PATCH, or DELETE for any operation that changes state and reserve GET strictly for reads.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Concepts**
- HTTP status code as the universal success vs failure contract
- 200 with error flag defeating monitoring, retries, and API gateways
- ProblemDetails for consistent structured failure responses
- APM alerting and circuit breakers depending on HTTP status

**Answer**

Business failures must map to appropriate 4xx or 5xx status codes because HTTP status is the universal contract that drives client retry logic, API gateway circuit breakers, and APM alerting thresholds — a 200 response with `success: false` in the body masks every failure from every system that does not parse the body. API gateways route and throttle on status code; if every response is 200, failed calls look healthy in dashboards and no alert fires. Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures, 404 for missing resources, 409 for conflicts, and 422 for semantic rejections. Envelope patterns like `{ success: false }` require every consumer to implement a custom parser and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Concepts**
- EF entity navigation properties not suitable for public HTTP contracts
- Lazy-loading N+1 triggered during JSON serialization
- Circular reference serializer loops
- DTO decoupling API contract from persistence schema

**Answer**

EF Core entities carry navigation properties, change-tracker state, and database-internal fields that were never meant to be a public HTTP contract, so serializing them directly leaks schema details and invites circular reference errors. Lazy-loaded navigations trigger N+1 queries during serialization when the JSON serializer walks the object graph — each navigation fires a new SQL query, exhausting the connection pool under load. Circular references between related entities cause the JSON serializer to loop indefinitely or require fragile `ReferenceHandler.IgnoreCycles` settings that hide design problems. Map entities to DTOs with explicit shapes in the service layer or via EF projection so the API contract evolves independently of table schema changes.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Concepts**
- System.Text.Json defaulting to camelCase serialization in ASP.NET Core 8
- Silent binding failure from PascalCase client payloads
- JsonPropertyName and PropertyNamingPolicy as alignment tools
- PropertyNameCaseInsensitive for legacy mixed-casing clients

**Answer**

ASP.NET Core 8 defaults to camelCase JSON serialization via `System.Text.Json`, so PascalCase property names from legacy clients bind as missing properties because the case does not match — the model properties default to `null` or `0` rather than the values the client sent. The failure is silent: the request returns 201 or 204 with no validation error, but the persisted record has default values instead of the submitted data. Fix with `[JsonPropertyName("PropertyName")]` attributes on DTO properties or a custom `PropertyNamingPolicy` to align server expectations with legacy payloads. When accepting mixed casing from various clients, enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)`.

---

#### Gotcha 6. GET with `[FromBody]`

**Concepts**
- GET request body not reliably supported across the HTTP ecosystem
- [FromBody] on GET failing silently through proxies and caches
- [FromQuery] for simple filters as the correct alternative
- OpenAPI tools and browser fetch blocking GET bodies

**Answer**

Many HTTP clients, proxies, CDNs, and caches ignore or strip GET request bodies because the HTTP specification does not define semantics for GET bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8. Model binding for `[FromBody]` on GET is therefore unreliable across the full HTTP ecosystem even if it works in direct testing. Use query strings with `[FromQuery]` for simple filter parameters, or POST to a dedicated search endpoint for complex filter objects that do not fit in a URL. Browser fetch API and OpenAPI tooling also discourage or block GET bodies, making the pattern fragile in any production environment where the full request path includes a proxy.

---

#### Gotcha 7. CORS as server security

**Concepts**
- CORS as browser-only enforcement — not server-side authentication
- Non-browser clients unaffected by CORS headers
- Authentication and authorization as actual server protection
- CORS enabling SPA browser access alongside real auth

**Answer**

CORS is enforced by browsers only — it prevents JavaScript on one origin from reading cross-origin responses, but it does nothing to stop curl, Postman, server-to-server calls, or any direct API request. The `Access-Control-Allow-Origin` header is a signal browsers check after receiving the response; a non-browser client simply ignores it and reads the data. A public API without authentication is fully accessible to any non-browser caller regardless of CORS policy, so CORS is never a substitute for JWT, API keys, or cookies. Register `AddCors` and `UseCors` to enable browser SPA access on cross-origin calls, and enforce actual authentication and authorization separately for real protection.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Concepts**
- Browser rejection of wildcard origin on credentialed requests
- AllowAnyOrigin and AllowCredentials as mutually exclusive
- WithOrigins for explicit trusted frontend origins
- Access-Control-Allow-Credentials header requirement

**Answer**

Browsers reject a response with `Access-Control-Allow-Origin: *` when the request includes cookies or an `Authorization` header, because the CORS specification explicitly forbids wildcard origins on credentialed cross-origin requests. `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined — ASP.NET Core will not emit a valid CORS response for credentialed requests when both are set. Instead, use `WithOrigins("https://app.example.com", "https://localhost:3000")` to list every trusted frontend origin explicitly, including local development URLs and all production domains. The browser also requires `Access-Control-Allow-Credentials: true` in the response, which `AllowCredentials()` handles.

---

#### Gotcha 9. Swagger UI exposed in Production

**Concepts**
- Swagger UI disclosing full API surface and schema to public internet
- Environment checks wrapping MapSwagger and UseSwaggerUI
- OpenAPI document exposure revealing endpoint names and enum values
- Authentication or IP allowlist gating for API documentation

**Answer**

Public Swagger UI discloses the full API surface, all schemas, enum values, and try-it-out access to anyone who finds the URL — giving potential attackers a complete map of your endpoints and data structures without any effort. Gate `MapSwagger` and `UseSwaggerUI` in `Program.cs` behind environment checks so they run only in Development and Staging, or require authentication middleware before the Swagger middleware. Production APIs should serve OpenAPI documents only to authenticated developers or internal tooling, not the public internet. Exposed OpenAPI documents reveal internal endpoint names, field names, and request schemas that are directly useful for targeted reconnaissance.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Concepts**
- [ApiController] enabling automatic ModelStateInvalidFilter
- Binding source inference for complex types
- Mixed controllers producing inconsistent error contracts
- Assembly-level [ApiController] for uniform behavior

**Answer**

Without `[ApiController]`, automatic 400 `ValidationProblemDetails` responses, binding source inference for complex types, and attribute routing enforcement all differ from controllers that have the attribute — so mixed controllers in the same API produce inconsistent error shapes that break partner integrations. A controller missing `[ApiController]` may return 200 OK with a partially bound model when model validation fails, because `ModelStateInvalidFilter` does not run, and `[FromBody]` is not inferred for complex parameters. Apply `[ApiController]` at the controller or assembly level using `[assembly: ApiController]` in an attribute file so every endpoint shares the same conventions without per-class annotation.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Concepts**
- Sync-over-async causing thread-pool starvation under load
- Deadlock when synchronization context is held during blocking call
- async Task<IActionResult> propagating await through service layer
- Kestrel throughput reduction from blocked request threads

**Answer**

Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation under load because the calling thread is blocked waiting for I/O to complete while no thread is available to process the continuation. Deadlocks also occur in environments with a synchronization context when the blocked thread holds the context that the async continuation needs to resume on — the task never completes because the thread it needs is the thread that is waiting for it. Always `await` async service and database calls in controller actions, which means the action signature is `async Task<IActionResult>` and the `await` propagates through the entire service and repository layer. Kestrel processes many concurrent requests efficiently precisely because async I/O frees threads while waiting — sync-over-async defeats this design entirely.

---

#### Gotcha 12. Liveness probe includes SQL check

**Concepts**
- Liveness as process restart signal — unrelated to external dependency recovery
- Readiness as traffic drain signal for dependency failures
- Kubernetes restart loop from liveness including external checks
- Tag-based separation of liveness and readiness health checks

**Answer**

If the liveness probe includes SQL and the database goes down for maintenance, Kubernetes kills and restarts pods even though restarting cannot fix a database outage — creating a restart loop that adds startup overhead and delays recovery. Liveness answers whether the ASP.NET Core process is alive and responsive; it should return healthy as long as the process can handle an HTTP request, independent of downstream dependencies. Readiness answers whether the instance should receive traffic; SQL, Redis, and message bus checks belong here because a failing dependency means the instance will return errors. Map `/health/live` with a tag predicate selecting only the self-check and `/health/ready` with the predicate selecting `AddDbContextCheck` and other dependency checks.

---

#### Gotcha 13. N+1 queries in list endpoints

**Concepts**
- N+1 pattern: one parent query plus N child queries per row
- Lazy loading triggering extra SQL during serialization
- EF projection with Select fetching only required columns
- Include/ThenInclude for explicit eager loading in one round trip

**Answer**

N+1 occurs when a list endpoint loads a parent collection and then each item triggers an additional query for a related navigation — one query for 100 orders plus 100 queries for each order's customer. The most common cause in APIs is serializing entity objects with lazy-loaded navigation properties: the JSON serializer accesses a navigation, EF fires a SELECT, and this repeats once per row. Fix with a single translated query: project directly to DTOs using `.Select(o => new OrderDto { CustomerName = o.Customer.Name })` so EF generates one SQL JOIN, or use explicit `.Include(o => o.Customer)` before materialization. Validate with EF logging or APM to confirm list endpoints produce a fixed small number of SQL round trips regardless of result set size.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Concepts**
- Offset pagination page drift from concurrent inserts and deletes
- Skip/Take without stable OrderBy producing undefined row order
- Keyset pagination anchored to a stable indexed key
- Large OFFSET performance cost scanning and discarding preceding rows

**Answer**

Concurrent inserts and deletes shift row positions in the dataset while a client walks pages — a new row inserted at page 1 pushes all subsequent rows one position, so page 2 either repeats the last row of page 1 or skips a row entirely. `Skip((page - 1) * pageSize).Take(pageSize)` also requires the database to count and discard all preceding rows, which becomes expensive on large offsets. Keyset pagination avoids both problems by using `WHERE id > @lastSeenId ORDER BY id LIMIT @pageSize` with the last key from the previous response — no scanning skipped rows and no drift because the filter is anchored to a specific key rather than a count. Offset pagination remains acceptable for small mostly-static tables; expose cursor tokens in link headers or response metadata for high-churn datasets.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Concepts**
- Field resolvers executing one database query per parent row
- DataLoader batching concurrent field resolutions into a single query
- 101 queries for a 100-row list without batching
- Root-level eager loading as alternative for static parent-child fields

**Answer**

Field resolvers in HotChocolate or other GraphQL servers execute independently per parent row — resolving `books` for each of 100 authors runs 100 separate queries plus the initial author query, totaling 101 round trips. DataLoader batches concurrent field resolutions within a single request: all 100 `books` resolver calls accumulate the author ids during the execution tick, then DataLoader fires one grouped query for all of them at once. Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips automatically. For fields the client almost always requests together with the parent, eager-load or project at the root query level rather than using DataLoader.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Concepts**
- Native gRPC HTTP/2 binary framing not accessible to browser JavaScript
- gRPC-Web protocol as browser-compatible translation layer
- AddGrpcWeb and EnableGrpcWeb for middleware setup
- CORS configuration required alongside gRPC-Web for cross-origin calls

**Answer**

Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript APIs — browsers cannot control trailers or binary framing at the level gRPC requires, so `@grpc/grpc-js` in the browser fails. Browser clients need the gRPC-Web protocol, which translates between the browser-accessible HTTP/1.1 or HTTP/2 fetch API and the native gRPC binary format via ASP.NET Core middleware. Add `AddGrpcWeb()` to services and call `.EnableGrpcWeb()` on each mapped gRPC service to activate the translation layer. CORS must also be configured for the browser origin because cross-origin browser calls still enforce CORS preflight and response header checks regardless of gRPC-Web. Standard .NET or Node gRPC clients communicating server-to-server continue using native gRPC without gRPC-Web.

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
