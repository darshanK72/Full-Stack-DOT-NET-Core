# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/10. Authentication & Authorization in APIs`

---

#### Q1. (R) Review `Program.cs` and a protected controller. Tokens validate in jwt.io but every API call returns 401 in staging.

**Answer:** JWT validation is misconfigured: **`ValidIssuer` contradicts the token issuer** (`https://login.company.com` vs `https://wrong-issuer.example.com`), and **`ValidateAudience = false`** while the IdP emits `aud: order-api` means you are not enforcing audience even when tokens are otherwise valid — combined with wrong issuer, authentication always fails before authorization runs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| JWT config | Wrong `ValidIssuer` | All bearer tokens fail validation → 401 |
| Security | `ValidateAudience = false` | Any token from same authority could access API if issuer were fixed — wrong audience accepted |
| Pipeline | `UseAuthorization()` without `UseAuthentication()` (see Q2) | Even fixed tokens may not populate `HttpContext.User` |

**Fix (priority order):**

1. Set `options.Authority = "https://login.company.com"` and let OpenID metadata drive issuer validation — or set `ValidIssuer` to match token `iss`.
2. Set `ValidAudience = "order-api"` (or `options.Audience = "order-api"`) and **`ValidateAudience = true`**.
3. Add `app.UseAuthentication()` before `UseAuthorization()`.
4. Test with a real staging token; compare claims to `TokenValidationParameters`.

**Production takeaway:** jwt.io "valid signature" ≠ ASP.NET validation — issuer, audience, clock skew, and signing keys must match configuration.

---

#### Q2. (R) After fixing JWT validation, `[Authorize]` endpoints still return 401 while anonymous health checks work.

**Answer:** **`UseAuthentication()` is missing** from the pipeline. Authorization middleware runs but no authentication handler executes, so `HttpContext.User` stays anonymous and `[Authorize]` fails even with a valid bearer token in the header.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline order | No `UseAuthentication()` | JWT bearer handler never runs |
| Symptom | Health anonymous works; API 401 | Misleading — looks like token problem, is middleware order |
| Security | Authorization without authentication | Endpoints with `[AllowAnonymous]` work; protected ones always deny |

**Fix (priority order):**

1. Insert `app.UseAuthentication()` **after** `UseRouting()` and **before** `UseAuthorization()`.
2. Standard order: `UseRouting` → `UseAuthentication` → `UseAuthorization` → `MapControllers`.
3. Verify `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` default scheme matches `[Authorize]` expectations.
4. Add integration test: send `Authorization: Bearer {valid}` → expect 200 on protected route.

**Production takeaway:** `[Authorize]` depends on authentication middleware populating the principal — a classic Karat pipeline trap.

---

#### Q3. (P) When should you use `[Authorize(Roles = "Admin")]` vs `[Authorize(Policy = "CanManageOrders")]`?

**Answer:** Use **roles** for coarse, stable identity groupings aligned with token role claims; use **policies** when authorization combines roles, scopes, custom requirements, or resource checks — the scalable pattern for production APIs.

- **Roles:** `[Authorize(Roles = "Admin")]` — simple, maps to `ClaimTypes.Role` / `"role"` claim; breaks down when rules multiply ("Admin OR owner AND not suspended").
- **Policies:** Register `options.AddPolicy("CanManageOrders", p => p.RequireRole("Admin").RequireClaim("scope", "orders.write"))` — composable, testable, named in OpenAPI docs.
- **What breaks with roles-only:** Fine-grained API access encoded as dozens of roles; tenant isolation; scope-based OAuth clients; changing rules requires redeploying attribute strings vs central policy registration.
- **Web API pattern:** Default deny — `builder.Services.AddAuthorization(o => o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());` then explicit `[AllowAnonymous]` where needed.

**Production takeaway:** Policies are the extension point for **scope vs role** and custom `IAuthorizationHandler` logic; roles alone do not scale for partner APIs.

---

#### Q4. (M) Token has role `OrderClerk` but no scopes. Explain scope vs role and enforcement.

**Answer:** **Roles** describe who the user is (group membership); **scopes** describe what the client application is permitted to do on behalf of the user (OAuth delegated permissions). A role without `scope: orders.read` should not access a scope-protected read endpoint even if the user is an OrderClerk.

- JWT carries scopes in `scope` claim (space-delimited) or `scp` (Azure AD) — not interchangeable with `role`.
- Policy example: `options.AddPolicy("OrdersRead", p => p.RequireClaim("scope", "orders.read"));` or custom requirement parsing space-separated scopes.
- **Enforcement:** `[Authorize(Policy = "OrdersRead")]` on GET; `[Authorize(Policy = "OrdersWrite")]` on POST — authorization middleware runs after authentication populated claims.
- **Client credentials flow:** Machine clients often have scopes only, no roles — role-only APIs reject valid integration tokens.

**Production takeaway:** Karat tests whether you map **OAuth scopes** to ASP.NET policies separately from role attributes.

---

#### Q5. (R) Review API key in query string. Security audit flags credential leakage.

**Answer:** Accepting **`apiKey` in the query string** exposes secrets in URL access logs (server, CDN, gateway), browser history, and referrer headers. Authentication succeeds functionally but violates credential transport best practices.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | API key in query string | Keys in IIS/nginx/load balancer logs permanently |
| Observability | Query logged by Q7-style middleware | Compliance violation — secrets in log aggregation |
| Design | Query visible in shared links and Referer | Key leakage to third-party analytics |

**Fix (priority order):**

1. Require **`X-Api-Key` header** (or `Authorization: ApiKey ...`) — read from headers in handler, never query.
2. Reject query keys even if header missing — fail closed with 401.
3. Redact sensitive headers in logging middleware.
4. Rotate keys found in historical logs; treat query keys as compromised.

**Production takeaway:** API keys belong in **headers** — query string keys are a debrief-level finding in production audits.

---

#### Q6. (R) Penetration test found sensitive data on "public" endpoints.

**Answer:** **`MetricsController` is entirely anonymous** — no class-level `[Authorize]`. **`ExportAll`** inherits `[Authorize]` but may lack an explicit role/policy check beyond "any authenticated user," exposing all customers to any valid token. Only `Register` is intentionally anonymous via `[AllowAnonymous]`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Anonymous leak | `MetricsController` — no `[Authorize]` | Internal metrics publicly readable |
| Authorization gap | `ExportAll` — authenticated but not authorized | Any logged-in user exports all PII |
| Misunderstanding | `[AllowAnonymous]` only on `Register` | Other actions still require auth — but metrics controller bypasses entirely |

**Fix (priority order):**

1. Add **`[Authorize]`** to `MetricsController` or map `/api/internal/*` with endpoint metadata + fallback policy.
2. Configure **`FallbackPolicy`** requiring authenticated user globally — explicit `[AllowAnonymous]` on register/webhooks only.
3. Protect `ExportAll` with **`[Authorize(Policy = "CanExportCustomers")]`** or admin role.
4. Audit with integration tests scanning OpenAPI for endpoints without security requirements.

**Production takeaway:** Without **fallback authorization policy**, any controller missing `[Authorize]` is a public API — common merge/regression failure.

---

#### Q7. (R) Diagnostic middleware logs bearer tokens in Production.

**Answer:** Logging the full **`Authorization` header** persists bearer JWTs and API keys in log stores — tokens remain valid until expiry and can be replayed. Logging **`QueryString`** also captures any query-based keys from Q5.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Raw `Authorization` in logs | Token replay; compliance (PCI, SOC2) failure |
| Operations | Query string logged | API keys and PII in search indexes |
| Design | Information-level logging in Production | High volume + permanent secret retention |

**Fix (priority order):**

1. **Never log** `Authorization`, cookies, or `apiKey` query values — redact or omit.
2. Log **`TraceIdentifier`**, path, method, status, user id claim (non-secret) only.
3. Use structured logging filters / Serilog destructuring policies to mask known sensitive fields.
4. Restrict diagnostic middleware to Development or sample at debug level behind feature flag.

**Production takeaway:** **Token in logs** is equivalent to password in logs — rotate exposed tokens and scrub log retention.

---

#### Q8. (P) JWT Bearer + API Key — scheme order and `[Authorize(AuthenticationSchemes = "...")]`.

**Answer:** Register multiple schemes with **`AddAuthentication().AddJwtBearer(...).AddScheme<,...>("ApiKey", ...)`**; set **default authenticate/challenge scheme** to JwtBearer for mobile. Batch endpoints specify **`[Authorize(AuthenticationSchemes = "ApiKey")]`** so only the API key handler runs — otherwise the default JWT handler may challenge incorrectly.

- **Registration:** `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...).AddScheme<ApiKeyOptions, ApiKeyHandler>("ApiKey", ...);`
- **Default scheme:** First parameter to `AddAuthentication(...)` — used when `[Authorize]` does not specify schemes.
- **Order:** Authentication handlers are selected by **scheme name**, not registration order — but each handler's `HandleAuthenticateAsync` runs for the schemes listed on the endpoint.
- **Explicit schemes:** `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` on mobile controllers; `ApiKey` on `/api/batch/*`.
- **Anti-pattern:** Letting JWT handler fail first on API-key-only routes → confusing `WWW-Authenticate: Bearer` responses.

**Production takeaway:** Multiple schemes require **explicit scheme on endpoints** — default scheme alone is insufficient for hybrid APIs.

---

#### Q9. (D) Multi-tenant orders API — role-only vs policy vs resource-based authorization.

**Answer:** Combine **fallback authenticate policy**, **tenant-scoped claims on API keys**, and **resource-based handlers** for row-level checks — roles alone cannot express "same tenant as order id."

- **API keys:** Issue key with embedded tenant id claim; policy `RequireClaim("tenant_id")` on integration routes.
- **JWT users:** Policy `BelongsToTenant` + `IAuthorizationHandler` comparing route `tenantId` to `tenant_id` claim.
- **Admin bypass:** `context.User.IsInRole("Admin")` inside handler or separate policy with `RequireRole("Admin")`.
- **Roles-only failure:** `OrderClerk` in tenant A could pass role check and read tenant B order if handler does not compare resource tenant.
- **Implementation location:** Policies in `Program.cs`; resource handler in `AuthorizationHandler<OperateOnOrderRequirement>` using `IHttpContextAccessor` or injected `Order` from route.

**Production takeaway:** Web API authorization at scale is **policy + resource handlers** — `[Authorize(Roles=...)]` on controller is the outer gate only.

---

#### Q10. (M) `[AllowAnonymous]` webhook with HMAC validation — risks and requirements.

**Answer:** `[AllowAnonymous]` is appropriate for **third-party callbacks that cannot send your JWT**, but the endpoint must still authenticate via **signature verification**, idempotency, and replay protection — not remain open to arbitrary POST bodies.

- **Why AllowAnonymous:** Stripe/server webhooks do not use your bearer tokens; class-level `[Authorize]` would block them without this override.
- **Required protections:** Verify HMAC (`Stripe-Signature`) **before** processing; use raw body (enable buffering); reject stale timestamps; store processed event ids.
- **Risks:** Weak or skipped verification → forged payments; reading body twice breaks signature if model binding consumed stream — use `[FromBody]` carefully or `Request.Body` with `EnableBuffering()`.
- **Not appropriate for:** "Public" CRUD because auth is hard — use client credentials or API keys instead.

**Production takeaway:** **Anonymous endpoint ≠ unauthenticated** — webhooks trade JWT for cryptographic request verification; pen testers flag missing HMAC, not `[AllowAnonymous]` alone.
