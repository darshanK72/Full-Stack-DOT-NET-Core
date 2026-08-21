# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/12. Health Checks`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [05. ASP.NET Core/12. Hosting, Kestrel & Environments](../../05.%20ASP.NET%20Core/12.%20Hosting%2C%20Kestrel%20%26%20Environments/KARAT_INTERVIEW_QUESTIONS.md) (health endpoints in hosting context)

---

#### Q1. (P) Your orders Web API runs on Kubernetes. Explain **liveness** vs **readiness** probes, which dependencies belong in each, and what happens if you point both probes at the same `/health` endpoint that includes SQL and Redis checks.

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

#### Q3. (D) Security review: `/health` returns JSON listing connection string server name, migration errors, and Redis key counts. Should health endpoints be public? Compare **`ResponseWriter`** minimal output vs **`HealthChecksUI`** detailed dashboard — what do you expose externally vs keep internal?

---

#### Q4. (P) Map ASP.NET Core health checks to **Kubernetes probes** — provide example paths, `Predicate`/tags for liveness vs readiness, and recommended `failureThreshold`, `periodSeconds`, and `timeoutSeconds` values for a database-backed API.

---

#### Q5. (M) You register multiple checks with tags `ready`, `live`, and `db`. Explain how **`HealthCheckOptions.Predicate`** filters which checks run per endpoint, and write registration for three routes: `/health/live`, `/health/ready`, `/health/db` (db-only, internal).

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddRedis(redisConn, name: "redis", tags: new[] { "ready", "db" })
    .AddDbContextCheck<AppDbContext>(tags: new[] { "ready", "db" });
```

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

#### Q7. (D) Team debates one **`/health`** endpoint vs split **`/health/live`** and **`/health/ready`**. Trade-offs for Web APIs behind K8s, load balancers, and Azure App Service — when does a single combined check cause outages or false positives?
