# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/12. Health Checks`

---

#### Q1. (P) Liveness vs readiness on Kubernetes — same `/health` with SQL and Redis.

**Answer:** **Liveness** asks "should the container be restarted?" — should be a trivial self-check only. **Readiness** asks "should this instance receive traffic?" — includes dependencies like SQL and Redis. Using one combined endpoint for both causes **cascading restarts and traffic drain** when a dependency blips.

- **Liveness failure:** kubelet **kills and restarts** the pod — must not depend on external DB or pod restart loops during DB maintenance.
- **Readiness failure:** pod removed from Service endpoints — no restart; correct when DB is down so clients aren't sent to a broken instance.
- **Same `/health` with SQL:** Brief SQL timeout fails liveness → unnecessary restarts; also fails readiness → traffic removed — **double penalty** from one slow check.
- **Fix:** `/health/live` — `Predicate = _ => false` or tagged `live` self-check only; `/health/ready` — SQL + Redis with `ready` tag.

**Production takeaway:** **Liveness vs readiness** separation prevents dependency slowness from restarting healthy processes — standard K8s Web API pattern.

---

#### Q2. (R) Readiness flaps — `TaskCanceledException` on DbContext check, K8s timeout 1s.

**Answer:** **`AddDbContextCheck` runs a DB query with default health check timeout (~30s)** but K8s **`timeoutSeconds: 1`** cancels the probe before SQL responds under load — intermittent `TaskCanceledException` and flapping readiness.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Timeout mismatch | Probe 1s vs DB latency 2–3s | False unhealthy; pod removed from load balancer |
| Load | Health check hits DB every probe interval | Adds query load during traffic spikes |
| Configuration | No explicit `HealthCheckOptions` timeout on registration | Hard to align with K8s |

**Fix (priority order):**

1. Increase K8s **`readinessProbe.timeoutSeconds`** to 3–5 (and `periodSeconds` 10+) — align with P99 DB latency.
2. Configure check timeout: `.AddDbContextCheck<OrdersDbContext>(..., timeout: TimeSpan.FromSeconds(3))`.
3. Use lightweight query — EF check executes `CanConnect`/`SELECT 1`; ensure connection pooling warm, not full migration check.
4. Consider **caching last successful readiness** briefly to avoid hammering DB on every probe (custom `IHealthCheck` wrapper).
5. Do **not** include heavy DB check on liveness endpoint.

**Production takeaway:** **DbContext health check timeout** must match orchestrator probe timeout — 1s probes against 2s SQL is a classic flap configuration.

---

#### Q3. (D) `/health` exposes server names, migration errors, Redis counts — public or not?

**Answer:** Public health endpoints should return **minimal status** (Healthy/Degraded/Unhealthy + optional version/build) — never connection strings, stack traces, or internal topology. Detailed JSON and **Health Checks UI** belong on internal networks with authentication.

| Exposure | Acceptable | Never expose publicly |
|---|---|---|
| Minimal `ResponseWriter` | `{ "status": "Healthy" }` or RFC-style short JSON | Connection string hostnames, exception messages |
| Readiness detail | Optional dependency name without credentials | Migration pending lists with schema internals |
| Health Checks UI | VPN / admin auth only | Open `/health-ui` polling production every 2s |

- **Production public:** `MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = ..., ResponseWriter = WriteMinimal })` — custom writer strips `data` dictionary entries.
- **HealthChecksUI:** Uses verbose `UIResponseWriter.WriteHealthCheckUIResponse` — **internal only**; stores history in memory/SQL — attack surface + info disclosure.
- **Auth:** `[Authorize]` on `/health/db` and UI; keep `/health/live` anonymous for K8s without auth if required — still minimal body.

**Production takeaway:** **Exposing details publicly** helps attackers map infrastructure — treat verbose health like debug endpoints.

---

#### Q4. (P) Map health checks to Kubernetes probes — paths, tags, timing.

**Answer:** Register tagged checks; map **liveness** to process-only, **readiness** to dependency checks; set probe timeouts greater than worst-case dependency latency.

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<OrdersDbContext>(tags: new[] { "ready" }, timeout: TimeSpan.FromSeconds(3))
    .AddRedis(redisConn, tags: new[] { "ready" });

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready")
});
```

**K8s example:**

```yaml
livenessProbe:
  httpGet: { path: /health/live, port: 8080 }
  initialDelaySeconds: 15
  periodSeconds: 20
  timeoutSeconds: 2
  failureThreshold: 3
readinessProbe:
  httpGet: { path: /health/ready, port: 8080 }
  initialDelaySeconds: 5
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3
```

- **Liveness:** longer period, short timeout, no DB.
- **Readiness:** shorter period, timeout ≥ DB P99, failureThreshold > 1 to absorb blips.
- **Startup probe:** optional for slow EF migration on boot.

**Production takeaway:** **K8s probes mapping** is tag-filtered endpoints + aligned timeouts — not one `/health` for everything.

---

#### Q5. (M) Multiple tags — `Predicate` filters checks per endpoint.

**Answer:** Each mapped endpoint runs only checks whose registration matches the **`Predicate`** — typically **`check.Tags.Contains("tag")`**. Untagged checks run on default `/health` when predicate is null (all checks).

```csharp
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready")
});

app.MapHealthChecks("/health/db", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("db")
}).RequireAuthorization("InternalOpsOnly");
```

- **`self` check:** tags `live` only — runs on live endpoint.
- **Redis + DbContext:** tags `ready` + `db` — run on both `/health/ready` and `/health/db`; ready endpoint may also include lighter checks without `db` tag if you split further.
- **Empty predicate:** `Predicate = _ => false` on live — always returns Healthy with no checks (process up) — alternative pattern.

**Production takeaway:** **Tagged checks** + predicates are how one registration powers multiple probe URLs without duplicating DB queries on liveness.

---

#### Q6. (R) Health Checks UI public on `/health-ui` — security issues.

**Answer:** **`MapHealthChecksUI` without auth** exposes an admin dashboard listing dependency names, failure history, and detailed exception data from **`UIResponseWriter`** — plus polls production every 2s amplifying load and leaking infrastructure state to the internet.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Public `/health-ui` | Attackers enumerate failing dependencies, timings, versions |
| Disclosure | `UIResponseWriter` on public `/health` | Verbose JSON with exception messages |
| Abuse | UI polls HTTPS every 2s | Self-DOS; credential-less monitoring of internal state |
| Storage | InMemory storage in Production | Lost history on restart; not the main risk — exposure is |

**Fix (priority order):**

1. Remove public UI — restrict to internal network or disable in Production.
2. If needed: **`RequireAuthorization`**, IP allowlist, or separate internal admin host.
3. Replace public `/health` writer with **minimal custom `ResponseWriter`** — not `UIResponseWriter`.
4. Configure UI to poll internal cluster URL, not public `https://api.company.com`.
5. Rate-limit health endpoints at gateway.

**Production takeaway:** **Health UI security** — UI is an operator tool, not a public API; verbose writers belong behind auth.

---

#### Q7. (D) Single `/health` vs split live/ready — trade-offs.

**Answer:** Single combined check is acceptable for **simple App Service / IIS** deployments with one dependency and no K8s liveness restarts; **split endpoints are required for K8s** and any environment where dependency failure must not restart the process.

| Approach | Pros | Cons |
|---|---|---|
| Single `/health` | Simple; one URL for load balancer | K8s liveness restarts on DB blip; LB may keep sending traffic if only "shallow" check |
| Split live/ready | Correct K8s semantics; isolate dependency failures | More routes to secure and document |
| Combined + all deps | Easy demo | Worst of both — restart + traffic drain on same failure |

- **Azure App Service:** Health check path can use `/health/ready`; no liveness restart — combined less dangerous but still exposes detail.
- **Load balancer:** Point pool health to **readiness** (dependencies), not liveness.
- **False positives:** Single endpoint including Redis when Redis optional → unnecessary unhealthy; use **Degraded** vs Unhealthy or tag optional deps.
- **Web API guidance:** Always split for K8s; use minimal live + dependency ready for production APIs.

**Production takeaway:** One **`/health`** including DbContext is a **demo pattern** — production Web APIs on K8s use **tagged split endpoints** to avoid restart loops and probe timeout mismatches.
