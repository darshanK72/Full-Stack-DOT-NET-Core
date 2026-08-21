# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/08. CORS`

---

#### Q1. (R) Review CORS policy — `AllowAnyOrigin()` + `AllowCredentials()` for SPA with cookies.

**Answer:** Browsers forbid `Access-Control-Allow-Origin: *` (or any origin wildcard) combined with `AllowCredentials()` — the CORS spec requires a specific origin echo when credentials are included; this configuration is invalid and the browser blocks the response.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| CORS spec | `AllowAnyOrigin()` + `AllowCredentials()` | Invalid combo — browser rejects preflight/response |
| Security | Appears permissive but fails at runtime | Developers confuse with "CORS broken" |
| Auth | Cookie/JWT credential mode requires explicit origin | SPA cannot authenticate cross-origin |

**Fix (priority order):**

1. Replace `AllowAnyOrigin()` with explicit origins: `.WithOrigins("https://app.example.com")`.
2. Keep `.AllowCredentials()` only when using cookies — for bearer tokens in `Authorization` header, credentials may not be needed (but origin still must be explicit if credentials used).
3. Use environment-specific origin lists from configuration — never wildcard with credentials.

```csharp
policy.WithOrigins("https://app.example.com")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

**Production takeaway:** The AllowAnyOrigin + credentials trap is one of the most common Karat CORS questions — mutually exclusive by browser spec.

---

#### Q2. (R) Review preflight failures — custom headers not allowed.

**Answer:** Preflight `OPTIONS` requests require the server to echo allowed methods **and** headers — `AllowAnyMethod()` without `AllowAnyHeader()` or explicit header list does not permit `Authorization` or custom `X-Request-Id`, so the browser blocks the actual POST before it reaches authentication.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Preflight | Missing `AllowHeaders` for `Authorization`, `X-Request-Id` | Browser blocks cross-origin POST |
| Testing | Postman/curl skip preflight | "Works in Postman, fails in browser" |
| Auth | JWT in Authorization header triggers preflight | All authenticated API calls fail from SPA |

**Fix (priority order):**

1. Add `.AllowAnyHeader()` for dev, or explicit `.WithHeaders("Authorization", "Content-Type", "X-Request-Id")` in production.
2. Ensure `OPTIONS` is handled — CORS middleware responds to preflight before auth when placed correctly.
3. Document required headers in API integration guide.

**Production takeaway:** Simple GET may work without preflight; JSON POST with Authorization always preflights — header allowlist must match client requests.

---

#### Q3. (M) JWT 401 on cross-origin — CORS vs auth middleware order and error response headers.

**Answer:** CORS middleware must run early enough to add headers to **all** responses including 401/403 — if authentication fails before CORS runs, browsers hide the response body and report a CORS error instead of showing 401, masking the real auth failure.

- Correct order: `UseRouting()` → `UseCors()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`.
- CORS middleware adds `Access-Control-Allow-Origin` on short-circuit responses when configured — including failed auth, if CORS ran first.
- A 401 without CORS headers looks like a CORS bug in DevTools — check Network tab for actual status vs console message.
- `[EnableCors]` on controllers applies policy per-endpoint — global policy still needs correct pipeline placement.
- Preflight `OPTIONS` must succeed **without** requiring authentication — CORS handles OPTIONS before auth challenge.

**Production takeaway:** "CORS error on 401" usually means middleware order or missing CORS on error responses — fix pipeline, not JWT settings first.

---

#### Q4. (P) Production origin allowlist vs `AllowAnyOrigin()` — wildcards and environment config.

**Answer:** Production must use explicit origin allowlists from configuration — `AllowAnyOrigin()` disables credential support and reflects any origin on anonymous reads, which is unsafe for authenticated APIs; ASP.NET Core does not support `https://*.staging.example.com` wildcards in `WithOrigins` natively.

- Load origins from `IOptions<CorsOptions>` bound to `appsettings.{Environment}.json`.
- Staging subdomains: list each origin explicitly, or implement custom `ICorsPolicyProvider` matching suffix rules server-side.
- Never use `AllowAnyOrigin()` in production for APIs with sensitive data — use gateway allowlist + app-level policy as defense in depth.
- Separate policies: `DevPolicy` (localhost ports) vs `ProductionPolicy` (explicit HTTPS origins only).
- Validate origin strings include scheme and no trailing slash — `https://app.example.com` not `app.example.com`.

```csharp
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddPolicy("Production", p =>
    p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
```

**Production takeaway:** CORS is an allowlist, not a denylist — wildcards in config files still need code to interpret them; framework won't do subdomain globbing for you.

---

#### Q5. (R) Review exposed headers — client cannot read `Content-Disposition` or `X-Total-Count`.

**Answer:** By default, browsers expose only CORS-safelisted response headers to JavaScript — custom headers like `X-Total-Count` and `Content-Disposition` require `WithExposedHeaders()` or the client cannot read them from `fetch`/XHR.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| CORS | Missing `Access-Control-Expose-Headers` | JS cannot read pagination total or filename |
| UX | Download filename ignored | Blob downloads get generic names |
| API design | Pagination metadata in headers invisible to SPA | Client shows wrong counts |

**Fix (priority order):**

1. Add `.WithExposedHeaders("Content-Disposition", "X-Total-Count")` to the policy.
2. Alternatively return metadata in JSON body — headers optional for CORS simplicity.
3. Document exposed headers in OpenAPI extension or integration guide.

**Production takeaway:** Successful 200 with CORS does not mean JS can read all response headers — exposure is a separate explicit allowlist.

---

#### Q6. (P) Review middleware placement — CORS after auth breaks preflight.

**Answer:** CORS must run before authentication and authorization — placing `UseCors` after `UseAuthentication`/`UseAuthorization` causes preflight OPTIONS requests (which carry no JWT) to fail auth before CORS headers are applied.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Middleware order | `UseCors` after auth | OPTIONS 401 — preflight fails |
| Anonymous endpoints | May work on simple GET | POST with Authorization fails cross-origin |
| Environment | Intermittent failures | Depends on endpoint auth requirements |

**Fix (priority order):**

1. Reorder: `UseRouting(); UseCors("Default"); UseAuthentication(); UseAuthorization(); MapControllers();`
2. Ensure `[AllowAnonymous]` on health endpoints does not substitute for CORS order fix.
3. Verify OPTIONS returns 204/200 with CORS headers in browser Network tab.

**Production takeaway:** CORS is a pipeline concern before auth — auth middleware should not run on failed preflight before CORS responds.

---

#### Q7. (R) Review CORS middleware placed after `MapControllers`.

**Answer:** Middleware registered after `MapControllers()` does not run for matched endpoints — CORS never executes for controller routes, causing browser failures even when direct Kestrel tests or misordered pipelines appear to work for some paths.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Middleware order | `UseCors` after endpoint mapping | CORS skipped for mapped routes |
| IIS/proxy | Reverse proxy adds complexity | Failures visible only cross-origin in browser |
| DevPolicy | `AllowAnyOrigin` without credentials | Masks some issues locally on same-origin tests |

**Fix (priority order):**

1. Move `UseCors` before `UseAuthentication`, `UseAuthorization`, and `MapControllers`.
2. Standard pipeline: routing → CORS → auth → authz → endpoints.
3. Test from actual SPA origin, not Swagger same-origin or Postman.

**Production takeaway:** Middleware after `Map*` is dead code for those endpoints — CORS placement is structural, not a policy string tweak.

---

#### Q8. (D) CORS at API vs API Management — duplicate `Access-Control-Allow-Origin` headers.

**Answer:** Only one layer should emit CORS headers in production — duplicate `Access-Control-Allow-Origin` values (API + APIM both adding headers) violate browser rules and cause unpredictable failures; typically the edge gateway owns CORS for public APIs, and the origin app disables CORS or uses internal-only policies.

| Owner | When |
|---|---|
| **API Management / CDN / reverse proxy** | Public multi-tenant APIs; central allowlist; consistent policy across microservices |
| **ASP.NET Core app** | Direct public exposure without gateway; local dev; internal SPAs hitting app directly |
| **Both (anti-pattern)** | Duplicate headers — browser may reject response |

- If APIM handles CORS, disable `UseCors` in production or restrict to Development environment.
- Align allowlists — APIM and app must not contradict (different origins echoed).
- APIM preflight caching can mask app misconfiguration until bypass paths hit origin directly.
- Document single owner in platform runbook — on-call fixes one place, not two.

**Production takeaway:** CORS is edge policy for many enterprises — double-application is a architecture smell, not a "belt and suspenders" safety measure.
