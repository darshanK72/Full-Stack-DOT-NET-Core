# Authentication & Authorization in APIs — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 10. Authentication & Authorization in APIs](#chapter-10-authentication-authorization-in-apis)
  - [Q1. What is JWT Bearer authentication for Web APIs?](#chapter-10-authentication-authorization-in-apis-q1)
  - [Q2. What is the difference between authentication and authorizat…](#chapter-10-authentication-authorization-in-apis-q2)
  - [Q3. What is an API key and when is it used?](#chapter-10-authentication-authorization-in-apis-q3)
  - [Q4. What is the difference between Bearer tokens and API keys?](#chapter-10-authentication-authorization-in-apis-q4)
  - [Q5. What does `[Authorize]` do on an API controller?](#chapter-10-authentication-authorization-in-apis-q5)
  - [Q6. What does `[AllowAnonymous]` do?](#chapter-10-authentication-authorization-in-apis-q6)
  - [Q7. What is the difference between role-based and policy-based a…](#chapter-10-authentication-authorization-in-apis-q7)
  - [Q8. What are OAuth2 scopes vs role claims?](#chapter-10-authentication-authorization-in-apis-q8)
  - [Q9. What is `JwtBearerDefaults.AuthenticationScheme`?](#chapter-10-authentication-authorization-in-apis-q9)
  - [Q10. What HTTP header carries JWT tokens?](#chapter-10-authentication-authorization-in-apis-q10)
  - [Q11. What is the difference between 401 Unauthorized and 403 Forb…](#chapter-10-authentication-authorization-in-apis-q11)
  - [Q12. What is `TokenValidationParameters`?](#chapter-10-authentication-authorization-in-apis-q12)
  - [Q13. What is a custom `AuthenticationHandler` for API keys?](#chapter-10-authentication-authorization-in-apis-q13)
  - [Q14. What is `[Authorize(AuthenticationSchemes = "...")]`?](#chapter-10-authentication-authorization-in-apis-q14)
  - [Q15. What is resource-based authorization in APIs?](#chapter-10-authentication-authorization-in-apis-q15)
  - [Q16. What is multi-tenant authorization for Web APIs?](#chapter-10-authentication-authorization-in-apis-q16)
  - [Q17. How do mobile apps typically authenticate to REST APIs?](#chapter-10-authentication-authorization-in-apis-q17)
  - [Q18. What is the difference between cookie auth and Bearer token …](#chapter-10-authentication-authorization-in-apis-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 10. Authentication & Authorization in APIs

### Q1. What is JWT Bearer authentication for Web APIs? {#chapter-10-authentication-authorization-in-apis-q1}

What is JWT Bearer authentication for Web APIs?

**Answer:** JWT Bearer authentication validates JSON Web Tokens sent in the `Authorization: Bearer <token>` header. ASP.NET Core registers the JWT Bearer handler, validates signature, issuer, audience, and lifetime, then builds a `ClaimsPrincipal` on `HttpContext.User`.

- Tokens are self-contained — claims (user id, roles, scopes) travel in the payload without server-side session lookup.
- Configure with `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` and authority/metadata URL or explicit `TokenValidationParameters`.
- Common for mobile apps, SPAs with token endpoints, and microservice-to-service calls through an identity provider.
- Stateless — API scales horizontally without shared session store, but revocation requires short lifetimes or token blocklists.
- Pair with `UseAuthentication()` before `UseAuthorization()` in the middleware pipeline.

---

### Q2. What is the difference between authentication and authorization in APIs? {#chapter-10-authentication-authorization-in-apis-q2}

What is the difference between authentication and authorization in APIs?

**Answer:** **Authentication** establishes who the caller is — validating credentials or tokens and populating `HttpContext.User` with claims. **Authorization** decides whether that authenticated identity is permitted to perform the requested action on the requested resource.

- Authentication runs first via `UseAuthentication()` and authentication handlers (JWT, API key, cookies).
- Authorization runs via `UseAuthorization()` and evaluates `[Authorize]`, policies, and roles against the principal.
- A request can be authenticated but forbidden (403) — valid token, insufficient permission.
- A request can be unauthenticated (401) — missing, expired, or invalid token before authorization is evaluated.
- Web APIs typically use Bearer tokens for authentication and policies/scopes for authorization.

---

### Q3. What is an API key and when is it used? {#chapter-10-authentication-authorization-in-apis-q3}

What is an API key and when is it used?

**Answer:** An API key is a static secret issued to a client (partner integration, batch job, internal service) that identifies and authenticates the caller. In ASP.NET Core it is usually sent in a header like `X-Api-Key` and validated by a custom `AuthenticationHandler`.

- Used for server-to-server integrations where OAuth browser flows are impractical.
- Simpler than JWT but no built-in expiry/claims unless you embed metadata in your key store.
- Must be transmitted in headers, never query strings, to avoid logging and referrer leakage.
- Rotate keys on compromise; store hashed keys server-side like passwords when possible.
- Often combined with JWT for user-facing mobile apps while batch endpoints use API keys only.

---

### Q4. What is the difference between Bearer tokens and API keys? {#chapter-10-authentication-authorization-in-apis-q4}

What is the difference between Bearer tokens and API keys?

**Answer:** Bearer tokens (typically JWTs) are time-limited, signed credentials carrying claims about the user and client, validated cryptographically against an authority. API keys are opaque static secrets looked up in a store — simpler but without standard claims, expiry, or refresh flows unless you build them.

- JWTs include issuer, audience, expiration, and scopes; validation uses signing keys from metadata.
- API keys require database or cache lookup to map key → tenant/permissions.
- Bearer tokens suit user-delegated OAuth flows; API keys suit long-lived machine integrations.
- Both use the `Authorization` header format differently: `Bearer <jwt>` vs custom `ApiKey` scheme or `X-Api-Key`.
- ASP.NET Core treats them as separate authentication schemes selected by `[Authorize(AuthenticationSchemes = "...")]`.

---

### Q5. What does `[Authorize]` do on an API controller? {#chapter-10-authentication-authorization-in-apis-q5}

What does `[Authorize]` do on an API controller?

**Answer:** `[Authorize]` on a controller or action requires an authenticated user before the action executes. If `HttpContext.User` is not authenticated, the pipeline returns **401 Unauthorized** (or challenges with `WWW-Authenticate` for Bearer).

- Applies to all actions on the class unless overridden with `[AllowAnonymous]` on specific actions.
- Can specify `Roles = "Admin"` or `Policy = "CanManageOrders"` for finer checks beyond mere authentication.
- Requires `UseAuthentication()` and `UseAuthorization()` in the pipeline — authorization alone is insufficient.
- Without a fallback policy, controllers missing `[Authorize]` remain publicly accessible.
- Works on minimal API endpoints via `.RequireAuthorization()` with the same underlying middleware.

---

### Q6. What does `[AllowAnonymous]` do? {#chapter-10-authentication-authorization-in-apis-q6}

What does `[AllowAnonymous]` do?

**Answer:** `[AllowAnonymous]` bypasses authorization for the decorated endpoint even when a class-level `[Authorize]` or a global fallback policy requires authentication. Use it for login, registration, health checks, and webhooks that authenticate via other means.

- Does not disable authentication middleware — it only skips the authorization requirement for that endpoint.
- Webhooks often use `[AllowAnonymous]` plus HMAC signature verification inside the action.
- Must be applied explicitly; it does not inherit in reverse — class `[Authorize]` + action `[AllowAnonymous]` opens that action only.
- Overuse creates accidental public data exposure — audit endpoints without auth regularly.
- Pair with network controls (IP allowlist, API gateway rules) for sensitive anonymous endpoints when possible.

---

### Q7. What is the difference between role-based and policy-based authorization in APIs? {#chapter-10-authentication-authorization-in-apis-q7}

What is the difference between role-based and policy-based authorization in APIs?

**Answer:** **Role-based** authorization checks if the user is in a named role via `[Authorize(Roles = "Admin")]`. **Policy-based** authorization uses named policies registered in `AddAuthorization` that can combine roles, claims, scopes, and custom requirements via `IAuthorizationHandler`.

- Roles map directly to role claims in the JWT — simple for coarse admin/user splits.
- Policies compose rules: `RequireRole("Admin").RequireClaim("scope", "orders.write")`.
- Policies scale better for multi-tenant, resource-level, and OAuth scope enforcement.
- Custom policies use `IAuthorizationHandler` for logic that does not fit attributes alone.
- Production APIs favor policies as the central extension point; roles are one input to policies.

---

### Q8. What are OAuth2 scopes vs role claims? {#chapter-10-authentication-authorization-in-apis-q8}

What are OAuth2 scopes vs role claims?

**Answer:** **Roles** describe who the user is (group membership — Admin, Clerk). **Scopes** describe what the client application is permitted to do on behalf of the user (delegated permissions — `orders.read`, `orders.write`). They are different claims and require separate enforcement.

- Scopes appear in JWT as `scope` (space-delimited) or `scp` (Azure AD) claims.
- Roles appear as `role` or `ClaimTypes.Role` claims.
- A user may have role `OrderClerk` but the client app may lack `orders.write` scope for POST.
- Machine-to-machine clients often have scopes only, no roles — role-only APIs reject valid integration tokens.
- Map scopes to ASP.NET policies with `RequireClaim("scope", "orders.read")` or custom scope-parsing handlers.

---

### Q9. What is `JwtBearerDefaults.AuthenticationScheme`? {#chapter-10-authentication-authorization-in-apis-q9}

What is `JwtBearerDefaults.AuthenticationScheme`?

**Answer:** `JwtBearerDefaults.AuthenticationScheme` is the string constant `"Bearer"` — the default scheme name for JWT Bearer authentication in ASP.NET Core. Pass it to `AddAuthentication(...)` as the default scheme and reference it in `[Authorize(AuthenticationSchemes = ...)]`.

- Registers the JWT Bearer handler that reads and validates the `Authorization: Bearer` header.
- When set as default, `[Authorize]` without explicit schemes uses JWT validation automatically.
- Multiple schemes require explicit scheme names on endpoints that should not use the default.
- Challenge responses include `WWW-Authenticate: Bearer` for 401 responses.
- Configuration lives in `AddJwtBearer(options => { options.Authority = ...; })` or manual `TokenValidationParameters`.

---

### Q10. What HTTP header carries JWT tokens? {#chapter-10-authentication-authorization-in-apis-q10}

What HTTP header carries JWT tokens?

**Answer:** JWT access tokens are sent in the **`Authorization`** header with the scheme **`Bearer`**: `Authorization: Bearer eyJhbGciOiJIUzI1NiIs...`. ASP.NET Core JWT Bearer middleware reads this header exclusively — not cookies or query strings for standard API auth.

- The token is the credential — treat it like a password in transit (HTTPS only in Production).
- Never log the full Authorization header — tokens remain valid until expiry and can be replayed.
- Some legacy systems use query or cookie tokens; ASP.NET Core JWT Bearer handler does not read those by default.
- Refresh tokens use separate endpoints and storage — not sent on every API call in the Authorization header.
- API keys may use `X-Api-Key` or a custom Authorization scheme instead of Bearer.

---

### Q11. What is the difference between 401 Unauthorized and 403 Forbidden? {#chapter-10-authentication-authorization-in-apis-q11}

What is the difference between 401 Unauthorized and 403 Forbidden?

**Answer:** **401 Unauthorized** means authentication failed or is missing — no valid identity was established. **403 Forbidden** means the caller is authenticated but not permitted to perform this action on this resource.

- 401: missing/expired/invalid token, wrong signing key, failed API key lookup.
- 403: valid token but insufficient role, scope, or resource-based authorization failure.
- 401 often includes `WWW-Authenticate: Bearer` challenge header; 403 typically does not.
- Returning 401 for authorization failures misleads clients into refreshing tokens unnecessarily.
- Returning 403 for invalid tokens misleads clients into thinking permissions are the issue, not credentials.

---

### Q12. What is `TokenValidationParameters`? {#chapter-10-authentication-authorization-in-apis-q12}

What is `TokenValidationParameters`?

**Answer:** `TokenValidationParameters` configures how the JWT Bearer handler validates incoming tokens — issuer, audience, signing keys, clock skew, and which standard checks to enforce (`ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, etc.).

- Set explicitly in `AddJwtBearer` or derived automatically from `Authority` OpenID Connect metadata.
- `ValidIssuer` and `ValidAudience` must match token `iss` and `aud` claims or validation fails with 401.
- `ValidateAudience = false` accepts any audience — dangerous in multi-API environments.
- Clock skew allows small time drift between token issuer and API server for `nbf`/`exp` validation.
- Misconfiguration is a common cause of "token works in jwt.io but API returns 401."

---

### Q13. What is a custom `AuthenticationHandler` for API keys? {#chapter-10-authentication-authorization-in-apis-q13}

What is a custom `AuthenticationHandler` for API keys?

**Answer:** A custom `AuthenticationHandler<TOptions>` reads the API key from a request header, validates it against a store, and returns `AuthenticateResult.Success` with a `ClaimsPrincipal` (tenant id, client name) or `AuthenticateResult.Fail` for 401.

- Register with `AddScheme<ApiKeyOptions, ApiKeyHandler>("ApiKey", ...)` alongside JWT Bearer.
- Implement `HandleAuthenticateAsync` — parse header, lookup key, build claims identity.
- Return `AuthenticateResult.NoResult()` when the header is absent so other schemes can try if configured.
- Use `[Authorize(AuthenticationSchemes = "ApiKey")]` on integration endpoints.
- Never read API keys from query strings — header transport only.

---

### Q14. What is `[Authorize(AuthenticationSchemes = "...")]`? {#chapter-10-authentication-authorization-in-apis-q14}

What is `[Authorize(AuthenticationSchemes = "...")]`?

**Answer:** This attribute specifies which registered authentication schemes must run for the endpoint — e.g., `"Bearer"` for JWT-only or `"ApiKey"` for batch integrations. When omitted, the default scheme from `AddAuthentication(defaultScheme)` applies.

- Multiple schemes: `[Authorize(AuthenticationSchemes = "Bearer,ApiKey")]` — handler selection depends on configuration.
- Prevents JWT handler from challenging API-key-only routes with confusing Bearer errors.
- Each scheme registers via `AddJwtBearer`, `AddScheme`, etc. with a unique name.
- Authentication schemes are separate from authorization policies — both may be required.
- Hybrid APIs (mobile JWT + partner API key) rely on explicit scheme attributes per controller or route group.

---

### Q15. What is resource-based authorization in APIs? {#chapter-10-authentication-authorization-in-apis-q15}

What is resource-based authorization in APIs?

**Answer:** Resource-based authorization evaluates permissions against the specific resource being accessed — e.g., "does this user own order 123?" — using `IAuthorizationService.AuthorizeAsync(user, order, "EditPolicy")` or an `IAuthorizationHandler` with the resource instance.

- Role checks alone cannot express row-level security (same role, different tenant data).
- Handler compares route id to `tenant_id` or `sub` claim from the JWT.
- Runs after authentication; often invoked inside the action or via a filter after loading the entity.
- `AuthorizationHandler<OperationAuthorizationRequirement, Order>` receives the `Order` resource for evaluation.
- Essential for multi-tenant Web APIs where two users share a role but must not see each other's data.

---

### Q16. What is multi-tenant authorization for Web APIs? {#chapter-10-authentication-authorization-in-apis-q16}

What is multi-tenant authorization for Web APIs?

**Answer:** Multi-tenant authorization ensures each request operates only within the caller's tenant — validating tenant id from JWT claims or API keys against the tenant associated with the requested resource, often via policies and resource handlers rather than roles alone.

- Embed `tenant_id` claim in JWT or map API key to tenant in the authentication handler.
- Policy `BelongsToTenant` compares route/body tenant to claim — Admin role may bypass for support tools.
- Data queries must filter by tenant at the repository/DbContext level — authorization is not enough alone.
- Cross-tenant data leakage is a critical finding — test with tokens from tenant A accessing tenant B ids.
- Global query filters in EF Core (`HasQueryFilter`) complement authorization handlers for defense in depth.

---

### Q17. How do mobile apps typically authenticate to REST APIs? {#chapter-10-authentication-authorization-in-apis-q17}

How do mobile apps typically authenticate to REST APIs?

**Answer:** Mobile apps commonly use OAuth 2.0 / OpenID Connect — redirect or embedded web view for user login, receive access and refresh tokens, then send the access token as `Authorization: Bearer` on API calls. PKCE is required for public mobile clients without client secrets.

- Access tokens are short-lived JWTs; refresh tokens renew access without re-login.
- Store tokens in secure platform storage (iOS Keychain, Android Keystore) — never plain SharedPreferences.
- Certificate pinning optional for high-security apps to mitigate MITM on token endpoints.
- Biometric unlock gates access to stored tokens locally — not a substitute for server authentication.
- ASP.NET Core API validates JWT via `AddJwtBearer` with authority pointing to Azure AD, Auth0, IdentityServer, etc.

---

### Q18. What is the difference between cookie auth and Bearer token auth for APIs? {#chapter-10-authentication-authorization-in-apis-q18}

What is the difference between cookie auth and Bearer token auth for APIs?

**Answer:** **Cookie authentication** stores the session id in an HttpOnly cookie sent automatically by browsers — suited for same-site server-rendered apps. **Bearer token authentication** sends a token in the Authorization header — suited for SPAs, mobile apps, and cross-origin API clients that do not rely on automatic cookie submission.

- Cookies require CSRF protection for state-changing browser requests; Bearer APIs typically do not use cookies.
- Bearer tokens work cleanly with CORS and non-browser clients; cookies complicate cross-origin SPA setups.
- Cookie auth enables server-side session revocation; JWT Bearer is stateless unless paired with introspection or short lifetimes.
- SPAs often use Bearer tokens from OAuth token endpoint; MVC apps use cookie auth with anti-forgery tokens.
- ASP.NET Core Web APIs default to Bearer/JWT for machine and mobile clients; cookies remain for Blazor Server or hybrid BFF patterns.

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

#### Q1. (R) Review `Program.cs` and a protected controller. Tokens validate in jwt.io but every API call returns 401 in staging.

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.company.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidIssuer = "https://wrong-issuer.example.com"
        };
    });

builder.Services.AddAuthorization();
// ...

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok(_repo.All());
}
```

Identity provider issues JWTs with `iss: https://login.company.com` and `aud: order-api`.

---

**Answer:**

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

---

#### Q2. (R) Same API as Q1 — after fixing JWT validation, `[Authorize]` endpoints still return 401 while anonymous health checks work. Review middleware order and what's missing.

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
```

---

**Answer:**

_Answer not found._

---

#### Q3. (P) When should you use **`[Authorize(Roles = "Admin")]`** vs **`[Authorize(Policy = "CanManageOrders")]`** on Web API controllers? How do you register policies, and what breaks when you only use roles for fine-grained API access?

---

**Answer:**

_Answer not found._

---

#### Q4. (M) An API accepts Azure AD JWTs. One endpoint must allow users with **`scope: orders.read`**; another requires **`scope: orders.write`**. A token has role `OrderClerk` but no scopes. Explain how **scope claims** differ from **role claims**, and how you enforce scopes in ASP.NET Core authorization.

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review this API key authentication handler. Security audit flags credential leakage in access logs and browser history.

```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Query.TryGetValue("apiKey", out var key))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (key == _options.ApiKey)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "api-client") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
        return Task.FromResult(AuthenticateResult.Fail("Invalid key"));
    }
}
```

---

**Answer:**

_Answer not found._

---

#### Q6. (R) Penetration test found sensitive data on "public" endpoints. Review this controller — which actions are anonymously reachable and why?

```csharp
[Authorize]
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(int id) => Ok(_service.Get(id));

    [HttpGet("export")]
    public IActionResult ExportAll() => Ok(_service.ExportAll());

    [HttpPost]
    [AllowAnonymous]
    public IActionResult Register([FromBody] RegisterDto dto) => Ok(_service.Register(dto));
}

[ApiController]
[Route("api/internal/metrics")]
public class MetricsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(_metrics.Snapshot());
}
```

Global policy: no fallback authorization policy configured.

---

**Answer:**

_Answer not found._

---

#### Q7. (R) Review this diagnostic middleware deployed to Production. Compliance finds bearer tokens in log storage.

```csharp
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    var authHeader = context.Request.Headers.Authorization.ToString();
    _logger.LogInformation("Request {Method} {Path} Auth={Auth}",
        context.Request.Method, context.Request.Path, authHeader);

    if (context.Request.QueryString.HasValue)
        _logger.LogInformation("Query={Query}", context.Request.QueryString.Value);

    await next(context);
}
```

Clients send `Authorization: Bearer eyJhbG...`.

---

**Answer:**

_Answer not found._

---

#### Q8. (P) Your API supports **JWT Bearer** for mobile apps and **API Key** header for batch jobs. Both schemes are registered. Explain **authentication scheme order**, default scheme selection, and how **`[Authorize(AuthenticationSchemes = "...")]`** prevents the wrong handler from running first.

---

**Answer:**

_Answer not found._

---

#### Q9. (D) Design authorization for a multi-tenant orders API: tenants must only read their own data; admins can read all; partner integrations use API keys scoped to one tenant. Compare role-only, policy + requirements, and resource-based authorization — what would you implement and where?

---

**Answer:**

_Answer not found._

---

#### Q10. (M) A controller has class-level `[Authorize]` and one webhook action marked `[AllowAnonymous]`. The webhook validates an HMAC signature in the action body. Review risks — when is `[AllowAnonymous]` appropriate on APIs, and what must still protect the endpoint?

```csharp
[Authorize]
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];
        if (!_stripe.Verify(json, signature)) return BadRequest();
        await _handler.Process(json);
        return Ok();
    }
}
```

**Answer:**

_Answer not found._

---
