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

What are health checks in ASP.NET Core Web API?

**Answer:** Health checks are endpoints that report whether the application and its dependencies are functioning. ASP.NET Core registers checks with `AddHealthChecks`, maps URLs with `MapHealthChecks`, and returns `Healthy`, `Degraded`, or `Unhealthy` status for orchestrators and load balancers.

- Built-in and third-party checks cover SQL, Redis, URLs, disk space, and custom logic.
- Used by Kubernetes, Azure App Service, and load balancers to route or restart traffic.
- Separate from business API endpoints — lightweight, fast, and minimal response bodies in Production.
- Register checks in DI; map multiple endpoints with different predicates for live vs ready probes.
- Custom `IHealthCheck` implementations encapsulate domain-specific readiness logic.

---

## Q2. What is the difference between liveness and readiness probes?

What is the difference between liveness and readiness probes?

**Answer:** **Liveness** asks "should this process be restarted?" — checks only that the app process is responsive, not external dependencies. **Readiness** asks "should this instance receive traffic?" — includes dependency checks like database and cache availability.

- Liveness failure → orchestrator **restarts** the pod/container.
- Readiness failure → instance removed from load balancer **without restart**.
- Putting SQL checks on liveness causes restart loops during database maintenance.
- Readiness should fail when dependencies are down so clients are not sent to broken instances.
- Map separate URLs (`/health/live`, `/health/ready`) with tag-filtered predicates in ASP.NET Core 8.

---

## Q3. What is `AddHealthChecks` used for?

What is `AddHealthChecks` used for?

**Answer:** `AddHealthChecks()` registers the health check service in DI and returns `IHealthChecksBuilder` to add individual checks — built-in `AddDbContextCheck`, `AddRedis`, `AddUrlGroup`, or custom `IHealthCheck` implementations with optional tags and timeouts.

- Call `builder.Services.AddHealthChecks().AddCheck(...).AddDbContextCheck<TContext>(...)` in `Program.cs`.
- Tags classify checks for filtering on different mapped endpoints.
- Timeout per check prevents one slow dependency from hanging the entire health response.
- Checks run when a health endpoint is hit — not continuously in background unless using HealthChecksUI polling.
- Degraded status allows optional dependencies to warn without marking fully Unhealthy.

---

## Q4. What is `MapHealthChecks`?

What is `MapHealthChecks`?

**Answer:** `MapHealthChecks(path, options)` maps a URL endpoint that executes registered health checks and writes the aggregated result as HTTP response — typically 200 for Healthy/Degraded and 503 for Unhealthy.

- Example: `app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = ... })`.
- `HealthCheckOptions` configures which checks run, response writer format, and result caching.
- Can chain `.RequireAuthorization()` for detailed internal health routes.
- Multiple mappings share one registration but filter different check subsets via `Predicate`.
- Place after routing setup; health endpoints bypass most business middleware when mapped simply.

---

## Q5. What is `HealthCheckOptions.Predicate`?

What is `HealthCheckOptions.Predicate`?

**Answer:** `Predicate` is a filter function `Func<HealthCheckRegistration, bool>` that selects which registered checks run for a specific mapped endpoint. Use it to run only `live`-tagged checks on liveness and `ready`-tagged checks on readiness.

- `Predicate = c => c.Tags.Contains("ready")` runs readiness checks only.
- `Predicate = _ => false` runs no checks — endpoint returns Healthy if process responds (liveness pattern).
- Null predicate runs **all** registered checks — convenient for dev, wrong for split K8s probes.
- Enables one registration set to power `/health/live`, `/health/ready`, and `/health/db` without duplication.
- Combine with tags assigned at `AddCheck(..., tags: new[] { "ready" })` registration time.

---

## Q6. What are health check tags?

What are health check tags?

**Answer:** Tags are string labels attached to health check registrations at setup time. They classify checks (e.g., `"live"`, `"ready"`, `"db"`) so `MapHealthChecks` predicates include or exclude them per endpoint.

- One check can have multiple tags — runs on any endpoint whose predicate matches any tag.
- `AddCheck("self", () => Healthy(), tags: new[] { "live" })` for process-only liveness.
- `AddDbContextCheck<T>(tags: new[] { "ready", "db" })` for dependency checks.
- Untagged checks run on default `/health` when predicate is null (all checks).
- Tags are the primary mechanism for Kubernetes live/ready endpoint separation in ASP.NET Core 8.

---

## Q7. Why should readiness checks include dependencies like SQL?

Why should readiness checks include dependencies like SQL?

**Answer:** Readiness determines whether the instance should receive traffic. If SQL is unreachable, the API cannot serve requests correctly — marking the instance not ready removes it from the load balancer pool without killing the process, giving the dependency time to recover.

- Prevents clients from hitting instances that will return 500 on every database call.
- Readiness failure is temporary — instance re-enters pool when checks pass again.
- Include critical dependencies only — optional third-party APIs may use Degraded instead of Unhealthy.
- Align check timeout with orchestrator probe timeout to avoid false flapping.
- Do not include the same heavy checks on liveness — restart does not fix external SQL outage.

---

## Q8. Why should liveness checks avoid external dependencies?

Why should liveness checks avoid external dependencies?

**Answer:** Liveness failure triggers a process restart. If liveness includes SQL or Redis, a brief dependency outage kills and restarts pods that cannot fix the external problem — causing restart storms, cascading failures, and unnecessary downtime.

- Liveness should answer: "Is the ASP.NET Core process hung or deadlocked?"
- A simple `() => HealthCheckResult.Healthy()` self-check or HTTP response suffices.
- External dependency failures belong on readiness — traffic drain, not restart.
- Restarting during DB maintenance removes all replicas simultaneously if liveness includes DB.
- K8s best practice: `/health/live` with no external deps; `/health/ready` with SQL/Redis/message bus.

---

## Q9. What is `AddDbContextCheck`?

What is `AddDbContextCheck`?

**Answer:** `AddDbContextCheck<TContext>()` registers a health check that verifies EF Core can connect to the database — typically executing a lightweight query or connection test against the configured DbContext.

- Package: `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`.
- Tag with `"ready"` — not `"live"`.
- Configure timeout: `.AddDbContextCheck<OrdersDbContext>(name: "sql", timeout: TimeSpan.FromSeconds(3))`.
- Uses connection pooling — ensure check query is cheap (`CanConnect` / `SELECT 1`), not full migrations scan.
- False failures under load often indicate probe timeout mismatch, not actual DB outage.

---

## Q10. What happens if liveness and readiness use the same failing check?

What happens if liveness and readiness use the same failing check?

**Answer:** When a dependency like SQL fails, **both** probes fail simultaneously — Kubernetes restarts the pod (liveness) **and** removes it from service endpoints (readiness). Restarts do not fix SQL outages, so pods enter a crash loop while traffic is also drained — the worst combined outcome from one misconfiguration.

- Brief SQL blip causes unnecessary pod kills across the fleet.
- During DB recovery, restarting pods adds startup load and delays readiness recovery.
- Fix by separating endpoints: liveness = self only, readiness = SQL/Redis.
- Same URL for both probes duplicates the failure mode — use distinct paths.
- Monitor restart counts separately from readiness failures to detect this misconfiguration.

---

## Q11. What is HealthChecksUI?

What is HealthChecksUI?

**Answer:** HealthChecksUI is a NuGet package that provides a web dashboard polling health endpoints and displaying history, status, and detailed dependency data. It uses verbose response writers and is intended for operators — not public Production exposure.

- Register with `AddHealthChecksUI()` and map `MapHealthChecksUI()`.
- Polls configured health URLs on an interval (e.g., every 2 seconds).
- Stores history in memory or SQL — useful for ops teams on internal networks.
- Public exposure leaks dependency names, failure messages, and infrastructure topology.
- Restrict to VPN, admin auth, or disable entirely in Production public APIs.

---

## Q12. Should health endpoints be publicly accessible?

Should health endpoints be publicly accessible?

**Answer:** Minimal liveness/readiness endpoints are often unauthenticated so orchestrators can probe them, but responses must contain **minimal information** — status only, no connection strings or exception details. Detailed health and HealthChecksUI should require authentication and internal network access.

- Public `/health/ready` with `{ "status": "Healthy" }` is acceptable.
- Public verbose JSON with SQL errors, Redis keys, and migration state is an information disclosure risk.
- Gate `/health/db` and UI behind `[Authorize]` or IP allowlists.
- Rate-limit health endpoints at the gateway to prevent abuse-driven load.
- Custom `ResponseWriter` strips the `data` dictionary for external-facing routes.

---

## Q13. What is a `ResponseWriter` in health checks?

What is a `ResponseWriter` in health checks?

**Answer:** `ResponseWriter` is a delegate on `HealthCheckOptions` that controls the HTTP response body format when health checks complete. Replace the default verbose JSON with minimal output for Production public endpoints.

- Signature: `Func<HttpContext, HealthReport, Task>`.
- Default writer includes per-check duration, status, and exception messages.
- Custom writer: `await context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() })`.
- HealthChecksUI uses `UIResponseWriter.WriteHealthCheckUIResponse` — verbose, internal only.
- Keep public writers stable — orchestrators parse simple status, not full HealthReport schema.

---

## Q14. How do Kubernetes probes use health check endpoints?

How do Kubernetes probes use health check endpoints?

**Answer:** Kubernetes configures `livenessProbe`, `readinessProbe`, and optionally `startupProbe` as HTTP GET requests to mapped ASP.NET Core health URLs. Failed liveness restarts the container; failed readiness removes the pod from Service endpoints.

- `httpGet.path: /health/live` for liveness; `/health/ready` for readiness.
- Configure `initialDelaySeconds`, `periodSeconds`, `timeoutSeconds`, and `failureThreshold` to match check latency.
- `timeoutSeconds` must exceed worst-case dependency check duration to prevent false failures.
- Startup probe allows slow EF migration boot before liveness/readiness take over.
- ASP.NET Core maps matching endpoints with tag predicates aligned to probe purpose.

---

## Q15. What is `HealthStatus` (Healthy, Degraded, Unhealthy)?

What is `HealthStatus` (Healthy, Degraded, Unhealthy)?

**Answer:** `HealthStatus` is the enum result of each health check — **Healthy** (all good), **Degraded** (partial impairment, still operational), or **Unhealthy** (failure). The aggregate report status reflects the worst individual check unless configured otherwise.

- HTTP mapping: Healthy/Degraded typically return 200; Unhealthy returns 503.
- Use Degraded for optional dependencies (cache miss fallback) vs Unhealthy for critical SQL failure on readiness.
- Custom checks return `HealthCheckResult.Degraded("Cache slow")` with descriptive messages for ops.
- Orchestrators may treat Degraded differently — document team convention for load balancer behavior.
- All checks contribute to aggregate — one Unhealthy check marks the endpoint Unhealthy.

---

## Q16. What is the difference between `/health/live` and `/health/ready`?

What is the difference between `/health/live` and `/health/ready`?

**Answer:** `/health/live` runs liveness checks — process self-check only, no external dependencies — telling Kubernetes whether to restart the container. `/health/ready` runs readiness checks including SQL, Redis, and other dependencies — telling the load balancer whether to send traffic.

- Different `MapHealthChecks` calls with different `Predicate` filters on tags.
- Live fails → pod restart. Ready fails → pod stays running but removed from service pool.
- Live should respond in milliseconds; ready may take 1–3 seconds for DB ping.
- Never point both K8s probes at the same URL with dependency checks included.
- Document both URLs in deployment runbooks and align probe timeouts per endpoint latency.

---

## Q17. What timeout considerations apply to health checks under load?

What timeout considerations apply to health checks under load?

**Answer:** Under traffic spikes, dependency checks (especially DB) slow down. If health probe timeout is shorter than check latency, readiness flaps — pods leave and re-enter the pool repeatedly. Health checks themselves also add query load during spikes, worsening the problem.

- Set K8s `timeoutSeconds` ≥ P99 dependency latency (often 3–5s for readiness, 1–2s for live).
- Configure per-check timeout in `AddDbContextCheck(..., timeout: TimeSpan.FromSeconds(3))`.
- Increase `periodSeconds` and `failureThreshold` to absorb brief blips without immediate removal.
- Avoid expensive queries in health checks — use connection test, not full table scan.
- Consider caching last successful readiness for a few seconds via custom wrapper to reduce DB hammering.

---

## Q18. What information should external health endpoints expose?

What information should external health endpoints expose?

**Answer:** External-facing health endpoints should expose only aggregate status (`Healthy` / `Unhealthy`), optional non-sensitive version or build id, and perhaps timestamp — never connection strings, stack traces, internal hostnames, migration errors, or per-dependency exception messages.

- Internal ops endpoints may expose dependency names and durations behind authentication.
- Custom minimal `ResponseWriter` for public routes; verbose writer for internal routes only.
- `traceId` is unnecessary on health endpoints — keep payloads tiny for fast probes.
- HealthChecksUI and `UIResponseWriter` output are never appropriate for public internet exposure.
- Treat verbose health like debug endpoints — same security review and access controls apply.

---

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

#### Gotcha 1. POST returning 200 instead of 201

**Answer:** A successful resource creation with POST should return HTTP 201 Created and tell the client where the new resource lives — returning 200 OK omits that contract and breaks REST clients that rely on status codes and the Location header.

- Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with a Location header pointing at the new resource URL.
- Include the created representation or a minimal payload in the response body when clients need immediate data without a follow-up GET.
- Returning 200 for create operations hides the new resource URL from standard HTTP client libraries and OpenAPI-generated SDKs.

---

#### Gotcha 2. GET that mutates state

**Answer:** GET must be safe and idempotent — performing deletes or updates on GET violates HTTP semantics, breaks caching proxies, and creates security holes when URLs are prefetched, logged, or opened in email clients.

- Browsers, CDNs, and link-preview crawlers may invoke GET URLs without user intent, so side effects run unintentionally.
- Cached GET responses can replay destructive operations or stale mutations across clients.
- Use POST, PUT, PATCH, or DELETE for state changes and keep GET read-only.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Answer:** Business failures must map to appropriate 4xx or 5xx status codes — a 200 response with an error flag forces every client to parse the body instead of using standard HTTP semantics, retries, and monitoring.

- Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures and 404, 409, or 422 for domain errors.
- HTTP status codes drive client retry logic, API gateways, and APM alerting; a 200 masks failures in dashboards.
- Envelope patterns like `{ success: false }` require custom handling in every consumer and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Answer:** EF Core entities expose navigation properties, shadow fields, and circular references that are not meant for public contracts — serialize DTOs with explicit shapes and never leak database schema to clients.

- Lazy-loaded navigations trigger N+1 queries during serialization and can pull entire object graphs into the response.
- Circular references between entities cause JSON serializer loops or require fragile reference-handling settings.
- DTOs decouple the API contract from schema migrations and let you expose only the fields clients need.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Answer:** ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json` — PascalCase property names from some clients bind as missing properties, leaving model properties at default values and causing silent data loss on POST and PUT.

- `[JsonPropertyName("PropertyName")]` or a custom `PropertyNamingPolicy` aligns server expectations with legacy client payloads.
- Enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)` when you must accept mixed casing.
- Silent binding failures produce 201/204 success responses with partially saved data and no validation error.

---

#### Gotcha 6. GET with `[FromBody]`

**Answer:** Many HTTP clients, proxies, and caches ignore or strip GET request bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8 Web API.

- Model binding for `[FromBody]` on GET is not reliably supported across the HTTP ecosystem.
- Use query strings with `[FromQuery]` for simple filters or POST to a dedicated search endpoint for complex filter objects.
- OpenAPI tools and browser fetch also discourage or block GET bodies, making the pattern fragile in production.

---

#### Gotcha 7. CORS as server security

**Answer:** CORS is enforced by browsers only — it does not stop curl, Postman, server-to-server calls, or direct API requests; authentication and authorization still protect the API.

- CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they do not authenticate callers.
- A public API without auth remains fully accessible to any non-browser client regardless of CORS policy.
- Register `AddCors` and `UseCors` for browser SPA access, and enforce JWT, cookies, or API keys separately for real security.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Answer:** Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers — you must specify explicit origins with `WithOrigins` and call `AllowCredentials`.

- `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined; ASP.NET Core will not emit a valid CORS response for credentialed requests.
- List every trusted frontend origin explicitly, including local dev URLs and production domains.
- Credentialed cross-origin calls require both matching origins and `Access-Control-Allow-Credentials: true`.

---

#### Gotcha 9. Swagger UI exposed in Production

**Answer:** Public Swagger UI discloses the full API surface, schemas, and try-it-out access — gate it behind authentication or disable it outside Development and Staging in ASP.NET Core 8.

- `MapSwagger` and `UseSwaggerUI` in `Program.cs` should be wrapped in environment checks or authorization middleware.
- Exposed OpenAPI documents reveal internal endpoints, field names, and enum values useful for reconnaissance.
- Production APIs typically serve OpenAPI only to authenticated developers or internal tooling, not the public internet.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Answer:** Without `[ApiController]`, automatic 400 `ValidationProblemDetails`, binding source inference, and attribute routing behaviors differ — mixed controllers in the same Web API produce inconsistent error contracts.

- `[ApiController]` enables automatic model-state validation responses and `[FromBody]` inference for complex types.
- Controllers missing the attribute may return 200 with invalid models or require manual `ModelState` checks.
- Apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Answer:** Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation and deadlocks under load — always `await` async service and database calls in ASP.NET Core 8.

- Sync-over-async ties up request threads while I/O completes, reducing throughput on Kestrel under concurrent load.
- Deadlocks occur when the blocked thread holds a synchronization context the continuation needs to resume.
- Mark controller actions `async Task<IActionResult>` and propagate `await` through the service layer to EF Core and HTTP clients.

---

#### Gotcha 12. Liveness probe includes SQL check

**Answer:** If the liveness probe fails when SQL is down, Kubernetes restarts pods that cannot fix the dependency — put SQL, Redis, and external service checks on readiness only.

- Liveness answers whether the process should be killed and restarted; a down database is not healed by restarting the app.
- Readiness removes the pod from the load balancer until dependencies recover without unnecessary restarts.
- Map `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck` or custom dependency tags.

---

#### Gotcha 13. N+1 queries in list endpoints

**Answer:** Returning entities with lazy-loaded navigation properties triggers one SQL query per row — use projection with `Select`, explicit `Include`, or DTO mapping to fetch list data in a bounded number of queries.

- Serializing a list of `Order` entities with `Customer` navigation can execute 1 + N queries under default lazy loading.
- Project directly to DTOs in LINQ so EF Core generates a single query with only the columns needed.
- For graphs that must be included, use `Include`/`ThenInclude` or split queries deliberately rather than relying on lazy load during JSON output.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Answer:** Concurrent inserts and deletes between offset pages cause duplicate or skipped rows — use keyset or cursor pagination ordered by a stable, indexed key for large datasets in Web API list endpoints.

- `Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are added or removed between requests.
- Keyset pagination uses `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key from the previous response.
- Offset pagination remains acceptable for small, mostly static tables; expose cursor tokens in link headers or response metadata for high-churn data.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Answer:** Field resolvers in HotChocolate or other GraphQL servers that query the database per parent row explode SQL under load — batch related loads with DataLoader or resolve joins at the root query.

- A list of 100 authors each resolving `books` individually executes 101 queries instead of one batched query.
- Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips.
- Eager-load or project at the root query when the client always requests nested fields together.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Answer:** Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript — browser clients need gRPC-Web middleware plus CORS configuration in ASP.NET Core 8.

- Standard `@grpc/grpc-js` in Node or .NET clients works server-to-server; Blazor WASM and SPA browsers require the gRPC-Web protocol.
- Add `AddGrpcWeb()` and `EnableGrpcWeb()` on mapped gRPC services to translate between gRPC-Web and native gRPC.
- Configure CORS for the browser origin alongside gRPC-Web, since cross-origin browser calls still enforce CORS on preflight and response headers.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (P) Your orders Web API runs on Kubernetes. Explain **liveness** vs **readiness** probes, which dependencies belong in each, and what happens if you point both probes at the same `/health` endpoint that includes SQL and Redis checks.

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

---

#### Q3. (D) Security review: `/health` returns JSON listing connection string server name, migration errors, and Redis key counts. Should health endpoints be public? Compare **`ResponseWriter`** minimal output vs **`HealthChecksUI`** detailed dashboard — what do you expose externally vs keep internal?

---

**Answer:**

_Answer not found._

---

#### Q4. (P) Map ASP.NET Core health checks to **Kubernetes probes** — provide example paths, `Predicate`/tags for liveness vs readiness, and recommended `failureThreshold`, `periodSeconds`, and `timeoutSeconds` values for a database-backed API.

---

**Answer:**

_Answer not found._

---

#### Q5. (M) You register multiple checks with tags `ready`, `live`, and `db`. Explain how **`HealthCheckOptions.Predicate`** filters which checks run per endpoint, and write registration for three routes: `/health/live`, `/health/ready`, `/health/db` (db-only, internal).

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddRedis(redisConn, name: "redis", tags: new[] { "ready", "db" })
    .AddDbContextCheck<AppDbContext>(tags: new[] { "ready", "db" });
```

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

---

#### Q7. (D) Team debates one **`/health`** endpoint vs split **`/health/live`** and **`/health/ready`**. Trade-offs for Web APIs behind K8s, load balancers, and Azure App Service — when does a single combined check cause outages or false positives?



**Answer:**

_Answer not found._

---
