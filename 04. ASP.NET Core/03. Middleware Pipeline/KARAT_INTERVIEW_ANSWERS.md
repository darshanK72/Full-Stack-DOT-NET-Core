# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/03. Middleware Pipeline`

---

#### Q1. (P) How do you limit incoming client requests within a time window using ASP.NET Core's built-in rate limiting middleware? What must you register in services, how do you attach policies to endpoints, and where should `UseRateLimiter()` sit relative to routing and authentication?

**Answer:** Register policies with `builder.Services.AddRateLimiter(...)`, call `app.UseRateLimiter()` after routing (and typically after authentication when limits are per-user), and attach policies globally, via `[EnableRateLimiting("policy")]`, or `RequireRateLimiting` on minimal routes.

- Define policies — e.g., `AddFixedWindowLimiter("api", o => { o.Window = TimeSpan.FromMinutes(1); o.PermitLimit = 100; })`, or sliding/token-bucket for burst control.
- Rejected requests return **429 Too Many Requests**; configure `OnRejected` to log partition key and emit `Retry-After`.
- **Order:** `UseRouting()` → `UseAuthentication()` (if limits use claims) → `UseRateLimiter()` → `MapControllers()` / minimal endpoints.
- Anonymous abuse protection can use IP partition keys earlier; authenticated per-user limits need `User` populated first.
- Complement edge proxy rate limits — in-app limits enable per-route and per-identity granularity.

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("api", w => { w.Window = TimeSpan.FromMinutes(1); w.PermitLimit = 60; });
});
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("api");
```

**Production takeaway:** Rate limiting is middleware + service registration — wrong order means wrong partition keys or limits applied before the endpoint is known.

---

#### Q2. (R) Review this API key middleware. What is wrong with behavior, headers, and security — and what happens to downstream middleware when the key is missing?

**Answer:** The middleware correctly skips `_next` on failure (downstream never runs) but produces an incomplete 401 — no body, weak header validation — so clients and gateways cannot parse errors and empty keys bypass auth.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP / API | 401 with no body or `Content-Type` | API clients receive empty responses |
| Runtime / logic | `ContainsKey` only — empty header passes | Auth bypass with `X-Api-Key: ` |
| Security | No comparison to configured secret; not constant-time | Key validation is cosmetic |
| Observability | No logging or `ProblemDetails` | Cannot audit rejected calls |

**Fix (priority order):**

1. Use `TryGetValue` and validate against `IOptions<ApiKeyOptions>` or secret store.
2. Write `ProblemDetails` JSON and set status before return.
3. Log correlation id; avoid distinguishing missing vs invalid if policy requires.

```csharp
if (!context.Request.Headers.TryGetValue("X-Api-Key", out var key)
    || !_validator.IsValid(key))
{
    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = 401, Title = "Unauthorized", Detail = "Invalid or missing API key."
    });
    return;
}
await _next(context);
```

**Production takeaway:** Short-circuit without `_next` is correct for 401; **incomplete response** is the production bug.

---

#### Q3. (M) An authentication middleware sets `Response.StatusCode = 401` and returns without calling `_next`. Trace one request through the pipeline — which components still run, which are skipped, and why is omitting `_next` intentional rather than a bug?

**Answer:** Middleware registered **before** auth still ran on the way in; once auth sets 401 and returns without `_next`, all subsequent middleware, routing, and endpoints are skipped — only earlier middleware's "after `_next`" code (if any) on the inbound leg does not run for skipped components; the response travels back through the server.

- **Already executed:** Middleware earlier in `Program.cs` (e.g., forwarded headers, logging start) ran before auth invoked `_next`.
- **Skipped:** Everything registered after auth — authorization, endpoints, MVC filters, business logic.
- **Intentional:** Fail-closed security — unauthenticated traffic must not hit handlers that assume identity.
- **Response path:** Kestrel sends status/headers/body to client; no further pipeline stages add headers unless terminal middleware mishandles started response.
- Auth should set complete response (status, optional `WWW-Authenticate`, JSON body) before return.

**Production takeaway:** 401 short-circuit is a **deliberate pipeline termination** — Karat tests whether you confuse it with an exception or missing `await _next`.

---

#### Q4. (R) After deploy behind nginx, HTTPS redirects loop and `[Authorize]` sees anonymous users. Review middleware order — what is wrong?

**Answer:** `UseForwardedHeaders()` runs **after** HTTPS redirection and authentication, so `Request.Scheme` stays `http` and client IP stays wrong — redirects loop and auth cookies/schemes mismatch production TLS termination.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline order | Forwarded headers after `UseHttpsRedirection` | Infinite redirect loops behind TLS-offloading proxy |
| Security | Auth middleware sees wrong scheme | Cookie `Secure` policy and redirect URI validation fail |
| Observability | Wrong `RemoteIpAddress` | Rate limits and audit logs show proxy IP only |
| Configuration | Missing `KnownProxies`/`KnownNetworks` | Spoofed `X-Forwarded-*` headers possible |

**Fix (priority order):**

1. Move `UseForwardedHeaders()` **first** (after exception handler if any).
2. Configure trusted proxy networks in `ForwardedHeadersOptions`.
3. Then `UseHttpsRedirection`, routing, authentication, authorization.

```csharp
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Production takeaway:** Middleware order is not cosmetic behind reverse proxies — forwarded headers are a **prerequisite** for correct scheme-aware middleware.

---

#### Q5. (R) Review `Run` vs `Map` branching. `/admin` returns 404 and static files leak on `/admin/config.json`. Explain pipeline behavior and fix order.

**Answer:** `app.Run` terminates the pipeline for all requests that reach it — because it is registered before `MapControllers()`, main app controllers never run; the `/admin` branch lacks static file handling control, and global static files serve sensitive paths.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline | `Run` before `MapControllers()` | Terminal delegate catches requests — controllers unreachable |
| Branching | `/admin` branch missing auth/static rules | 404 on admin controllers; unintended file exposure |
| Static files | Global `UseStaticFiles()` serves `/admin/config.json` | Sensitive config downloadable |
| Design | Fallback `Run` too early | Entire API appears broken except branch routes |

**Fix (priority order):**

1. Register `MapControllers()` **before** any terminal `Run`.
2. Use `Map`/`MapWhen` for branches; avoid global terminal `Run` except true fallback at end.
3. Restrict static files with `RequestPath` or exclude admin paths; protect admin branch with auth middleware.

```csharp
app.UseStaticFiles(); // or scoped StaticFileOptions
app.Map("/admin", admin =>
{
    admin.UseAuthentication();
    admin.UseAuthorization();
    admin.MapControllers();
});
app.MapControllers();
app.Run(async ctx => { ctx.Response.StatusCode = 404; }); // last only
```

**Production takeaway:** `Run` is **terminal** — placement ends the pipeline for matching traffic; `Map` creates sub-pipelines without stopping the main app registration order.

---

#### Q6. (P) Where should global exception-handling middleware sit relative to routing, authentication, and `UseDeveloperExceptionPage`? What breaks if exception middleware is registered too early or too late?

**Answer:** Register developer-facing exception pages early in Development; production exception handler (`UseExceptionHandler` or `IExceptionHandler` middleware) should wrap routing and endpoints so action and endpoint exceptions convert to uniform responses — but still inside the outermost logging/correlation middleware.

- **Development:** `UseDeveloperExceptionPage()` near top (after forwarded headers) — shows stack traces for all downstream exceptions.
- **Production:** `UseExceptionHandler("/error")` or `AddExceptionHandler<GlobalHandler>()` + `UseExceptionHandler()` early enough to catch endpoint exceptions, **before** response starts.
- **Too early:** Handler may not have endpoint metadata for ProblemDetails context — still catches exceptions.
- **Too late (after endpoints only):** Some middleware exceptions missed; worse if placed after terminal middleware.
- **Auth exceptions:** Authentication failures are often not exceptions — they short-circuit with 401/403; do not rely on exception middleware for normal auth denial.
- Never expose developer page in Production.

```csharp
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler();
app.UseForwardedHeaders();
app.UseRouting();
app.UseAuthentication();
app.MapControllers();
```

**Production takeaway:** Exception middleware is the **outer envelope** for unhandled faults — auth short-circuits and validation ProblemDetails are separate paths.

---

#### Q7. (R) This middleware tries to short-circuit banned clients but clients still receive full response bodies and logs show "Headers already sent." What went wrong?

**Answer:** The middleware calls `_next` **before** checking the ban — the endpoint executes and starts the response; afterward status and body cannot be rewritten, causing "headers already sent" and defeated blocking intent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline logic | Post-check after `await _next(context)` | Ban runs too late — full response already generated |
| HTTP | `StatusCode`/`WriteAsJsonAsync` after response started | Runtime error or ignored status change |
| Security | Banned IPs receive full payloads | Data leak to blocked clients |
| Performance | Expensive work runs for banned clients | Defeats purpose of early rejection |

**Fix (priority order):**

1. Check ban **before** `await _next(context)`.
2. Short-circuit with status + body and `return` without calling `_next`.
3. Place IP ban middleware early — before routing/endpoints.

```csharp
if (IsBanned(context.Connection.RemoteIpAddress))
{
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    await context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
    return;
}
await _next(context);
```

**Production takeaway:** Middleware that rejects requests must run **before** the work it prevents — post-processing cannot undo a started response.

---

#### Q8. (M) Product needs request timing in logs for every endpoint including minimal APIs, plus early rejection of oversized uploads before MVC model binding. Would you use middleware, an endpoint filter, or an action filter for each concern — and why?

**Answer:** Request timing belongs in middleware wrapping the whole pipeline (or `IHttpMetrics` / built-in logging); upload size rejection belongs in early middleware (Kestrel limits + custom middleware checking `Content-Length`) before routing/model binding — not action filters.

- **Request timing:** Middleware with `Stopwatch` around `await _next` captures minimal APIs and controllers uniformly; action filters miss middleware-only paths and other endpoints.
- **Upload size cap:** Configure `KestrelServerLimits.MaxRequestBodySize` and/or middleware inspecting `Content-Length` **before** `UseRouting` — reject 413 before buffering body into memory.
- **Action filters** suit per-action authorization, validation tweaks, or transforming `IActionResult` — they need routed MVC context.
- **Endpoint filters** (.NET 7+) apply to minimal APIs and route handlers — good for per-route validation, not global timing.
- Correlation IDs and security headers — middleware; business validation — filters or endpoint filters.

**Production takeaway:** "Before routing / before binding" almost always means **middleware or server limits**, not filters.

---

#### Q9. (R) Review custom correlation-ID middleware and registration. Some responses have two `X-Correlation-Id` headers and downstream services receive empty IDs. Diagnose.

**Answer:** Duplicate middleware registration appends the header twice, and downstream calls likely read from `Request.Headers` after the response path instead of from `HttpContext.Items` or propagated outbound handler — empty IDs when incoming header was absent on internal calls.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline | `UseMiddleware<CorrelationIdMiddleware>()` registered twice | Duplicate `X-Correlation-Id` headers confuse clients and APM |
| Design | Downstream HTTP calls not using `Items["CorrelationId"]` | Broken distributed tracing |
| HTTP | `Headers.Append` without checking existing | Violates single-value expectation |
| Observability | Logs may use different id than response header | Cannot correlate user report to logs |

**Fix (priority order):**

1. Register middleware once; use `Use` extension with guard.
2. Store id in `Items` and `HttpContext.TraceIdentifier`; set response header only if not present.
3. Use `IHttpClientFactory` delegating handler to forward header on outbound calls.

```csharp
if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
    context.Response.Headers["X-Correlation-Id"] = id;
```

**Production takeaway:** Middleware duplication is silent in compile — **double registration** produces subtle HTTP spec violations in production.

---

#### Q10. (D) You inherit a pipeline with 14 custom middleware components, some duplicated between `MapWhen` branches. How do you refactor without changing outward behavior — what belongs in middleware vs endpoint filters vs hosting reverse proxy?

**Answer:** Consolidate duplicate cross-cutting middleware into a single ordered pipeline segment; move edge concerns (TLS, WAF, coarse rate limits) to the reverse proxy; move action-specific logic to endpoint filters — preserve behavior with integration tests on status codes, headers, and body for representative routes.

| Concern | Preferred layer |
|---|---|
| TLS, L7 DDoS, geo block | Reverse proxy / CDN |
| Forwarded headers, global exception envelope, correlation ID | Middleware (once) |
| Per-route rate limits, API key on subset | Middleware `MapWhen` or endpoint metadata + policy |
| Model validation, `IActionResult` shaping | Endpoint/action filters |
| Business rules | Application services |

- Draw current pipeline order; mark duplicates; extract `UseStandardApiPipeline()` called once.
- Avoid `MapWhen` copies that re-register auth — use endpoint metadata and policies instead.
- Characterization tests through `WebApplicationFactory` before/after refactor.

**Production takeaway:** Middleware sprawl is a maintenance hazard — **one ordered pipeline** plus edge proxy beats fourteen slightly different branches.

---
