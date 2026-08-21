# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/02. Project Structure & Program.cs`

---

#### Q1. (R) Review this top-level `Program.cs`. The app compiles but returns 404 for all controller routes and Swagger shows no endpoints. What structural mistake was made?

**Answer:** Services and Swagger are registered, but the pipeline never maps controllers or calls `UseRouting` — authorization middleware runs with no endpoints, so every controller route returns 404.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline | Missing `app.MapControllers()` (and typically `UseRouting`) | No routes registered — perpetual 404 |
| Structure | `UseAuthorization()` without authentication or endpoints | Misleading security setup; no `[Authorize]` enforcement path |
| Swagger | OpenAPI has no discovered endpoints | Empty Swagger UI despite "working" compile |
| Design | `app.Run()` not used but implicit — pipeline ends without terminal route | Requests fall through unmatched |

**Fix (priority order):**

1. Add routing and endpoint mapping:

```csharp
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

2. Add `UseAuthentication()` if JWT/cookies are used — authorization alone is insufficient.

**Production takeaway:** Top-level `Program.cs` merges startup phases — **Build() is not enough**; missing `Map*` calls are the most common "empty API" bug.

---

#### Q2. (R) A developer registers middleware inside `builder.Services` and services inside the `app` pipeline block. What fails and how should `Program.cs` separate the two phases?

**Answer:** Middleware extension methods belong on `WebApplication` after `Build()`; service registration belongs on `IServiceCollection` before `Build()` — swapping them causes compile errors or no-op pipeline configuration.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `builder.Services.UseHttpsRedirection()` — not an `IServiceCollection` extension | Build failure |
| Compile | `app.AddControllers()` — not a valid pipeline method | Build failure |
| Conceptual | Services vs middleware phases confused | Team repeats mistake across microservices |
| Runtime | Even if shimmed, middleware never runs | Security headers, HTTPS redirect absent |

**Fix (priority order):**

1. **Before `Build()`:** `builder.Services.Add*()` — DI registrations only.
2. **After `Build()`:** `app.Use*()` / `app.Map*()` — middleware and endpoints.

```csharp
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddControllers();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/ping", () => "pong");
app.Run();
```

**Production takeaway:** `Program.cs` structure is a contract — **Services = composition root**, **App = request pipeline**.

---

#### Q3. (P) `launchSettings.json` sets `"applicationUrl": "https://localhost:7101;http://localhost:5101"` and `"ASPNETCORE_ENVIRONMENT": "Development"`. What does **not** carry over to production, and what must replace it in Azure App Service or Kubernetes?

**Answer:** `launchSettings.json` is IDE-local and never deployed — production needs environment variables or platform config for URLs, environment name, secrets, and connection strings.

- **Does not deploy:** `launchSettings.json` is not published with the app; its `applicationUrl` and dev environment override apply only to F5 / `dotnet run` from Visual Studio or CLI with launch profile.
- **Replace URLs:** Set `ASPNETCORE_URLS` or platform binding (App Service → Configuration; K8s → container port + Service/Ingress).
- **Replace environment:** Set `ASPNETCORE_ENVIRONMENT=Production` (or Staging) via App Service settings, K8s manifest, or container env — not launch profile.
- **Replace secrets:** User secrets and dev `appsettings.Development.json` give way to Key Vault, App Service secrets, or K8s secrets mounted as env vars.
- **HTTPS dev cert:** Development certificate trust does not exist in prod — use platform-managed certs or cert-manager.

**Production takeaway:** Junior devs often debug "works locally" prod failures because **`launchSettings` is mistaken for runtime config**.

---

#### Q4. (R) Review environment-specific configuration loading. Production connects to the dev SQL database after deploy. What is wrong with this setup?

**Answer:** `appsettings.Development.json` is loaded unconditionally, so its connection string overrides production settings whenever keys collide — regardless of `ASPNETCORE_ENVIRONMENT=Production`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Configuration | Manual `AddJsonFile("appsettings.Development.json")` without environment guard | Dev connection string wins in Production |
| Security | Dev database credentials in prod process | Data leak, compliance violation |
| Design | Duplicates `CreateDefaultBuilder` behavior incorrectly | Unpredictable config precedence |
| Operations | Silent wrong-environment connect — app "works" until data mismatch | Corrupt prod with test data |

**Fix (priority order):**

1. Remove unconditional Development file load — use `WebApplication.CreateBuilder(args)` which loads environment-specific files automatically.
2. If customizing: `AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)` **after** base `appsettings.json`.
3. Store production connection strings in env vars or secret store — highest precedence.

**Production takeaway:** Configuration provider **order and conditional loading** belong in Project Structure literacy — not only the Options chapter.

---

#### Q5. (M) Explain what happens at `var app = builder.Build()` vs `app.Run()` in the modern hosting model — what gets validated, what is still lazy, and when do misconfigured services first surface?

**Answer:** `Build()` constructs the `WebApplication`, builds the service provider (optionally validating registrations), and configures the middleware pipeline blueprint; `Run()` starts Kestrel and blocks until shutdown — many scoped services are not resolved until the first request unless `ValidateOnBuild` is enabled.

- **`Build()`:** Freezes `IServiceCollection` into root `IServiceProvider`; with `ValidateOnBuild`, DI attempts to resolve all registered services — catches missing dependencies at startup.
- **`Build()`:** Middleware components are instantiated when first wired — order is fixed but delegates are not executed yet.
- **`Run()`:** Starts host lifetime, binds Kestrel to configured URLs, begins accepting connections.
- **Lazy resolution:** Scoped services (e.g., `DbContext`) resolve per request — captive singleton/scoped bugs may not appear until traffic unless `ValidateScopes` is on.
- **First request:** Routing, auth, and endpoint execution may throw if middleware order or options are wrong — even when `Build()` succeeded.

**Production takeaway:** "It compiled and started" ≠ "DI and pipeline are correct" — enable scope/build validation in CI/staging.

---

#### Q6. (R) Preview routing middleware order: controllers return 404, but minimal `/health` works. Identify the ordering bug.

**Answer:** `UseRouting()` is registered **after** `MapControllers()`, so endpoint routing for controllers was never established in the correct order — `/health` works because `MapGet` creates an endpoint, but controller endpoint metadata is not wired through a proper routing middleware sequence.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline order | `UseRouting()` after `MapControllers()` | Controller endpoints not matched — 404 |
| Pipeline order | Auth middleware before routing without endpoint-aware configuration | Authorization may not apply as expected |
| Design | Mixed minimal + controller endpoints with inconsistent routing setup | Health works while business API appears "down" |
| Operations | Misleading partial availability in probes | Load balancer sends traffic to broken API |

**Fix (priority order):**

1. Standard order:

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok());
```

2. In .NET 7+, many templates rely on implicit routing — still verify auth runs after routing when using endpoint metadata.

**Production takeaway:** Middleware order preview in `Program.cs` is high-frequency Karat material — **routing before endpoints, auth after routing**.

---

#### Q7. (D) `Program.cs` in a microservice has grown to 200 lines. The team debates `Program.cs` vs `Startup.cs` vs extension methods (`AddInfrastructure()`, `UseApiPipeline()`). What criteria decide the split without hiding middleware order?

**Answer:** Keep a thin, readable `Program.cs` that shows middleware order explicitly; move DI registration groups into `Add*` extension methods and keep `Use*` pipeline extensions small and ordered — avoid `Startup.cs` unless the team already standardizes on it for testability.

- **Stay in `Program.cs`:** The exact sequence of `UseForwardedHeaders`, `UseAuthentication`, `UseRateLimiter`, `MapControllers` — order must be visible in one scroll or one `UseApiPipeline()` whose body is ordered top-to-bottom.
- **Extract to `AddInfrastructure()`:** DbContext, repositories, HTTP clients, options binding — no middleware.
- **Extract to `UseApiPipeline()`:** Only if the method body preserves order verbatim — document "do not reorder."
- **Avoid:** Multiple hidden `Use*` calls across assemblies that make order non-obvious — production incidents from duplicated `UseAuthentication`.
- **`Startup.cs`:** Valid for large teams wanting `ConfigureServices`/`Configure` test doubles — not required in modern templates.

**Production takeaway:** Structure is for **human readability of pipeline order**, not arbitrary line-count reduction.

---

#### Q8. (R) Review this production-hardening attempt. The app throws at startup in CI but worked on a developer laptop. Prioritize fixes.

**Answer:** `ValidateOnBuild` (or first controller resolution) fails because `SmtpOrderNotifier` depends on `IOptions<SmtpOptions>` that was never registered — the developer machine may have had a partial manual registration or did not run the same startup path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | Missing `Configure<SmtpOptions>` / `AddOptions` binding | Startup failure resolving `IOrderNotifier` |
| Configuration | SMTP settings not loaded from config section | Notifier cannot send; options default/null |
| Environment | Swagger omitted in prod (good) but exception handler unmapped | `/error` route may 404 unless endpoint exists |
| Testing | CI enables full graph validation; dev used hot reload without hitting notifier | Bug escapes locally |

**Fix (priority order):**

1. Register options: `builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));`
2. Ensure `appsettings.json` / secrets contain `Smtp` section for CI.
3. Map exception handler endpoint or switch to `IExceptionHandler` (.NET 8+) with proper middleware.

```csharp
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IOrderNotifier, SmtpOrderNotifier>();
```

**Production takeaway:** `Program.cs` service registration must form a **complete graph** — partial local testing hides missing options bindings until CI `ValidateOnBuild`.

---
