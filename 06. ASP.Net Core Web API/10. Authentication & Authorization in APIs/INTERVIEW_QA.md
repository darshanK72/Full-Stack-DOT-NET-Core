# Authentication & Authorization in APIs — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is JWT Bearer authentication for Web APIs?](#q1-what-is-jwt-bearer-authentication-for-web-apis)
2. [Q2. What is the difference between authentication and authorization in APIs?](#q2-what-is-the-difference-between-authentication-and-authorization-in-apis)
3. [Q3. What is an API key and when is it used?](#q3-what-is-an-api-key-and-when-is-it-used)
4. [Q4. What is the difference between Bearer tokens and API keys?](#q4-what-is-the-difference-between-bearer-tokens-and-api-keys)
5. [Q5. What does `[Authorize]` do on an API controller?](#q5-what-does-authorize-do-on-an-api-controller)
6. [Q6. What does `[AllowAnonymous]` do?](#q6-what-does-allowanonymous-do)
7. [Q7. What is the difference between role-based and policy-based authorization in APIs?](#q7-what-is-the-difference-between-role-based-and-policy-based-authorization-in-apis)
8. [Q8. What are OAuth2 scopes vs role claims?](#q8-what-are-oauth2-scopes-vs-role-claims)
9. [Q9. What is `JwtBearerDefaults.AuthenticationScheme`?](#q9-what-is-jwtbearerdefaultsauthenticationscheme)
10. [Q10. What HTTP header carries JWT tokens?](#q10-what-http-header-carries-jwt-tokens)
11. [Q11. What is the difference between 401 Unauthorized and 403 Forbidden?](#q11-what-is-the-difference-between-401-unauthorized-and-403-forbidden)
12. [Q12. What is `TokenValidationParameters`?](#q12-what-is-tokenvalidationparameters)
13. [Q13. What is a custom `AuthenticationHandler` for API keys?](#q13-what-is-a-custom-authenticationhandler-for-api-keys)
14. [Q14. What is `[Authorize(AuthenticationSchemes = "...")]`?](#q14-what-is-authorizeauthenticationschemes)
15. [Q15. What is resource-based authorization in APIs?](#q15-what-is-resource-based-authorization-in-apis)
16. [Q16. What is multi-tenant authorization for Web APIs?](#q16-what-is-multi-tenant-authorization-for-web-apis)
17. [Q17. How do mobile apps typically authenticate to REST APIs?](#q17-how-do-mobile-apps-typically-authenticate-to-rest-apis)
18. [Q18. What is the difference between cookie auth and Bearer token auth for APIs?](#q18-what-is-the-difference-between-cookie-auth-and-bearer-token-auth-for-apis)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is JWT Bearer authentication for Web APIs?

**Concepts**
- JWT Bearer handler validating tokens from Authorization: Bearer header
- Self-contained tokens carrying claims without server-side session lookup
- AddAuthentication + AddJwtBearer registration with authority or TokenValidationParameters
- Stateless horizontal scaling without shared session store
- UseAuthentication before UseAuthorization in the middleware pipeline

**Answer**

JWT Bearer authentication validates JSON Web Tokens sent in the `Authorization: Bearer <token>` header by reading the token, verifying the signature against the issuer's signing keys, and checking issuer, audience, and lifetime before building a `ClaimsPrincipal` on `HttpContext.User`. The key advantage is that tokens are self-contained — claims like user id, roles, and scopes travel in the payload, so the API scales horizontally without a shared session store. Configure with `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` and supply either an authority URL (which drives automatic OpenID Connect metadata discovery) or explicit `TokenValidationParameters`. Pair with `UseAuthentication()` before `UseAuthorization()` in the middleware pipeline — missing `UseAuthentication()` means the token is never read and `HttpContext.User` stays unauthenticated even with valid tokens. The stateless nature means revocation requires either short lifetimes or a token blocklist since there is no server-side session to invalidate.

---

## Q2. What is the difference between authentication and authorization in APIs?

**Concepts**
- Authentication establishing identity by validating credentials or tokens
- Authorization deciding whether the authenticated identity may perform the action
- UseAuthentication populating HttpContext.User before UseAuthorization runs
- 401 Unauthorized for failed authentication, 403 Forbidden for failed authorization
- Bearer tokens for authentication, policies and scopes for authorization

**Answer**

**Authentication** establishes who the caller is — validating credentials or tokens and populating `HttpContext.User` with claims so subsequent middleware knows the identity. **Authorization** decides whether that authenticated identity is permitted to perform the requested action on the requested resource. Authentication runs first via `UseAuthentication()` and authentication handlers; authorization runs via `UseAuthorization()` and evaluates `[Authorize]`, policies, and roles against the principal that authentication produced. A request can be authenticated but forbidden — valid token, but the user lacks the required role or scope — which returns 403. A request can be unauthenticated — missing, expired, or invalid token — which returns 401 before authorization even runs. Web APIs typically use Bearer tokens for authentication and policy-based scopes or roles for authorization.

---

## Q3. What is an API key and when is it used?

**Concepts**
- API key as a static secret identifying and authenticating a client
- Custom AuthenticationHandler reading X-Api-Key header
- Server-to-server integrations where OAuth browser flows are impractical
- Hashed key storage server-side like password storage
- Header-only transport to avoid logging and referrer leakage

**Answer**

An API key is a static secret issued to a client — a partner integration, batch job, or internal service — that identifies and authenticates the caller without an interactive login flow. In ASP.NET Core it is typically sent in a header like `X-Api-Key` and validated by a custom `AuthenticationHandler` that looks the key up in a store and maps it to a claims identity. API keys suit server-to-server integrations where OAuth browser-redirect flows are impractical — the calling service simply attaches the key to every request. They must be transmitted in headers rather than query strings to avoid logging and referrer leakage, since query strings appear in server access logs and browser history. Store hashed keys server-side like passwords rather than plain text, and rotate on compromise. API keys are often combined with JWT for user-facing mobile apps while separate batch endpoints use API keys only.

---

## Q4. What is the difference between Bearer tokens and API keys?

**Concepts**
- JWT Bearer tokens as time-limited signed credentials carrying claims
- API keys as opaque static secrets requiring database lookup
- OAuth delegated flows vs long-lived machine integration credentials
- TokenValidationParameters for cryptographic validation of JWTs
- [Authorize(AuthenticationSchemes)] selecting the correct handler per endpoint

**Answer**

Bearer tokens (typically JWTs) are time-limited, signed credentials carrying claims about the user and client, validated cryptographically against an authority's signing keys — no database lookup required since the signature proves authenticity. API keys are opaque static secrets that require a database or cache lookup to map the key to a tenant or permissions set — simpler but without standard claims, expiry, or refresh flows unless you build them yourself. JWTs suit user-delegated OAuth flows where the token represents delegated user permission; API keys suit long-lived machine-to-machine integrations. Both use the `Authorization` header but in different formats: `Bearer <jwt>` for tokens and either a custom `ApiKey` scheme or `X-Api-Key` for API keys. ASP.NET Core treats them as separate named authentication schemes, so `[Authorize(AuthenticationSchemes = "ApiKey")]` can protect integration endpoints while `[Authorize]` on user endpoints uses the default JWT Bearer scheme.

---

## Q5. What does `[Authorize]` do on an API controller?

**Concepts**
- [Authorize] requiring an authenticated principal before the action runs
- 401 Unauthorized response and WWW-Authenticate challenge when unauthenticated
- Roles and Policy parameters for finer-grained checks beyond authentication
- UseAuthentication and UseAuthorization both required in the pipeline
- RequireAuthorization() for minimal API endpoints

**Answer**

`[Authorize]` on a controller or action requires that `HttpContext.User` is authenticated before the action executes — if not, the pipeline returns **401 Unauthorized** with a `WWW-Authenticate: Bearer` challenge header for JWT Bearer schemes. Applied at the controller class level, it covers all actions unless overridden with `[AllowAnonymous]` on specific methods. You can add `Roles = "Admin"` or `Policy = "CanManageOrders"` for checks beyond mere authentication, so the action also verifies the authenticated user has the required role or satisfies the named policy. Both `UseAuthentication()` and `UseAuthorization()` must be in the pipeline — omitting `UseAuthentication()` means no handler ever validates the token and the user stays unauthenticated, so `[Authorize]` always returns 401. Minimal API endpoints use `.RequireAuthorization()` with the same underlying middleware.

---

## Q6. What does `[AllowAnonymous]` do?

**Concepts**
- [AllowAnonymous] bypassing authorization for the decorated endpoint
- Authentication middleware still running — only authorization skipped
- Overriding class-level [Authorize] for specific public endpoints
- Webhook endpoints using [AllowAnonymous] plus HMAC signature verification
- Audit risk of accidental public data exposure via [AllowAnonymous]

**Answer**

`[AllowAnonymous]` bypasses the authorization requirement for the decorated endpoint even when a class-level `[Authorize]` or a global fallback policy requires authentication. The authentication middleware still runs — it only skips the authorization check for that endpoint, so `HttpContext.User` may be populated if the request carries a valid token, but the endpoint does not require it. A common pattern is webhooks: `[AllowAnonymous]` plus HMAC signature verification inside the action body, since the payment provider signs the request rather than sending a Bearer token. Applying it to a class-level `[Authorize]` controller opens only that specific action, not the whole class. Overuse creates accidental public data exposure — audit endpoints without auth regularly, especially after merges, since `[AllowAnonymous]` on one action in a sensitive controller is easy to miss in code review.

---

## Q7. What is the difference between role-based and policy-based authorization in APIs?

**Concepts**
- Role-based authorization checking role claims via [Authorize(Roles = "...")]
- Policy-based authorization composing roles, claims, scopes, and custom requirements
- IAuthorizationHandler for logic that does not fit attribute-level rules
- Policies as the central extension point for fine-grained and multi-tenant rules
- AddAuthorization registration for named policies

**Answer**

**Role-based** authorization checks if the authenticated user is in a named role via `[Authorize(Roles = "Admin")]` — straightforward for coarse admin/user splits where the role claim exists in the JWT. **Policy-based** authorization uses named policies registered in `AddAuthorization` that can combine roles, claims, scopes, and custom requirements via `IAuthorizationHandler`, which means the rule lives in one place rather than scattered across multiple attributes. Policies scale better for multi-tenant, resource-level, and OAuth scope enforcement: `options.AddPolicy("CanWriteOrders", p => p.RequireRole("OrderClerk").RequireClaim("scope", "orders.write"))` expresses a rule that neither role nor claim alone could capture. Custom policies use `IAuthorizationHandler` for logic that requires injected services or resource context — things that do not fit attribute-only checks. Production APIs favor policies as the central extension point, using roles as one input to those policies rather than the entire authorization strategy.

---

## Q8. What are OAuth2 scopes vs role claims?

**Concepts**
- Roles describing who the user is — group membership
- Scopes describing what the client application may do — delegated permissions
- scope and scp JWT claim names for Azure AD and standard OAuth
- Machine-to-machine clients having scopes but no roles
- RequireClaim("scope", "...") in ASP.NET Core policies for scope enforcement

**Answer**

**Roles** describe who the user is — group membership like Admin or OrderClerk — and travel in the JWT as `role` or `ClaimTypes.Role` claims. **Scopes** describe what the client application is permitted to do on behalf of the user — delegated permissions like `orders.read` or `orders.write` — and travel as `scope` (space-delimited string, standard OAuth) or `scp` (Azure AD) claims. They require separate enforcement because a user may have role `OrderClerk` but the client app may lack the `orders.write` scope, which means the write endpoint must check both. Machine-to-machine clients in client credentials flows have scopes only and no user roles — role-only APIs reject valid integration tokens from service accounts. Map scopes to ASP.NET Core policies with `RequireClaim("scope", "orders.read")` or a custom scope-parsing handler that splits the space-delimited string, since claims-based scope matching varies by identity provider format.

---

## Q9. What is `JwtBearerDefaults.AuthenticationScheme`?

**Concepts**
- JwtBearerDefaults.AuthenticationScheme as the "Bearer" string constant
- Default scheme for AddAuthentication making [Authorize] use JWT automatically
- Multiple schemes requiring explicit scheme names on non-default endpoints
- WWW-Authenticate: Bearer challenge header on 401 responses
- AddJwtBearer options for authority and TokenValidationParameters

**Answer**

`JwtBearerDefaults.AuthenticationScheme` is the string constant `"Bearer"` — the conventional default scheme name for JWT Bearer authentication in ASP.NET Core. Pass it to `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` as the default scheme so that `[Authorize]` without an explicit scheme always invokes the JWT Bearer handler. When set as default, every undecorated `[Authorize]` attribute automatically uses JWT validation without needing `[Authorize(AuthenticationSchemes = "Bearer")]` on each endpoint. Challenge responses include `WWW-Authenticate: Bearer` for 401 responses, which tells clients to obtain a token. If the API supports multiple schemes — JWT for users and API keys for batch clients — only one can be the default; the others need explicit scheme names on endpoints that should use them.

---

## Q10. What HTTP header carries JWT tokens?

**Concepts**
- Authorization header with Bearer scheme for JWT access tokens
- HTTPS-only transport since the token is the credential
- Never logging the Authorization header value
- Refresh tokens on separate endpoints — not sent on every API call
- ASP.NET Core JWT Bearer handler reading only this header by default

**Answer**

JWT access tokens are sent in the **`Authorization`** header with the scheme **`Bearer`**: `Authorization: Bearer eyJhbGciOiJIUzI1NiIs...`. The ASP.NET Core JWT Bearer middleware reads this header exclusively — not cookies or query strings for standard API authentication. The token is the credential and must travel over HTTPS only in production, since anyone intercepting it can replay it until expiry. Never log the full Authorization header value — tokens remain valid until expiry and can be replayed by anyone who reads the log. Refresh tokens are separate: they are stored securely client-side and sent only to the token endpoint to obtain a new access token, not included in Authorization headers on every API call.

---

## Q11. What is the difference between 401 Unauthorized and 403 Forbidden?

**Concepts**
- 401 — authentication missing or invalid, no identity established
- 403 — authenticated but insufficient permission for this resource
- WWW-Authenticate challenge header accompanying 401 responses
- Returning 401 for authorization failures causing unnecessary token refreshes
- Returning 403 for invalid tokens confusing clients about the cause

**Answer**

**401 Unauthorized** means authentication failed or is missing — no valid identity was established, either because the token is absent, expired, has an invalid signature, or uses the wrong signing key. **403 Forbidden** means the caller is authenticated but not permitted to perform this action on this resource — the identity is valid, but the role, scope, or resource-based authorization check failed. Returning 401 for authorization failures misleads clients into refreshing their token unnecessarily, since a fresh token from the same user still lacks the required permission. Returning 403 for invalid tokens misleads clients into thinking their permissions are the issue when the real fix is to re-authenticate. 401 often includes a `WWW-Authenticate: Bearer` challenge header instructing the client on how to obtain a valid token; 403 typically does not, since the client already authenticated correctly.

---

## Q12. What is `TokenValidationParameters`?

**Concepts**
- TokenValidationParameters configuring issuer, audience, signing keys, and clock skew
- ValidIssuer and ValidAudience matching token iss and aud claims
- ValidateAudience = false accepting any audience — dangerous in multi-API environments
- Clock skew accommodating time drift between issuer and API server
- Authority-driven automatic metadata vs explicit manual parameters

**Answer**

`TokenValidationParameters` configures how the JWT Bearer handler validates incoming tokens — which issuer is accepted, which audience is required, which signing keys are trusted, how much clock skew is tolerated, and which standard checks to enforce (`ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, etc.). Set it explicitly in `AddJwtBearer` or derive it automatically by supplying an `Authority` URL, which triggers OpenID Connect metadata discovery to pull the issuer and signing keys. `ValidIssuer` and `ValidAudience` must match the token `iss` and `aud` claims or validation fails with 401, which is the most common cause of "token works in jwt.io but API returns 401" — the signing is valid but the claims do not match configuration. Setting `ValidateAudience = false` is dangerous in multi-API environments because a valid token issued for one API can authenticate against another. Clock skew allows small time drift between the token issuer and API server for `nbf` and `exp` validation.

---

## Q13. What is a custom `AuthenticationHandler` for API keys?

**Concepts**
- AuthenticationHandler<TOptions> reading and validating API keys from headers
- AuthenticateResult.Success with ClaimsPrincipal vs AuthenticateResult.Fail
- AuthenticateResult.NoResult() delegating to next scheme when header is absent
- AddScheme registration alongside JWT Bearer
- [Authorize(AuthenticationSchemes = "ApiKey")] on integration endpoints

**Answer**

A custom `AuthenticationHandler<TOptions>` reads the API key from a request header, validates it against a store or cache, and returns `AuthenticateResult.Success` with a `ClaimsPrincipal` (carrying tenant id and client name as claims) or `AuthenticateResult.Fail` for invalid keys. Implement `HandleAuthenticateAsync` — parse the header, look up the key, and build a claims identity on success. Return `AuthenticateResult.NoResult()` when the header is absent rather than `Fail`, so other registered schemes can try if the endpoint supports multiple authentication methods. Register with `AddScheme<ApiKeyOptions, ApiKeyHandler>("ApiKey", ...)` alongside `AddJwtBearer` and decorate integration endpoints with `[Authorize(AuthenticationSchemes = "ApiKey")]`. API keys must come from request headers only — never read them from query strings, since query strings appear in access logs and browser history.

---

## Q14. What is `[Authorize(AuthenticationSchemes = "...")]`?

**Concepts**
- AuthenticationSchemes specifying which registered handler runs for the endpoint
- Default scheme from AddAuthentication applying when omitted
- Multiple schemes via comma-separated string for hybrid endpoints
- Preventing JWT handler from challenging API-key-only routes
- Scheme selection separate from authorization policy selection

**Answer**

This attribute specifies which registered authentication schemes must run for the endpoint — e.g., `"Bearer"` for JWT-only routes or `"ApiKey"` for batch integration endpoints. When omitted, the default scheme from `AddAuthentication(defaultScheme)` applies, so most user-facing endpoints do not need to specify a scheme explicitly. Specifying `[Authorize(AuthenticationSchemes = "ApiKey")]` on batch endpoints prevents the JWT Bearer handler from running and issuing a confusing `WWW-Authenticate: Bearer` challenge to service clients that do not use tokens. Multiple schemes can be specified as a comma-separated string — `"Bearer,ApiKey"` — and ASP.NET Core evaluates them in registration order. Authentication schemes and authorization policies are independent: an endpoint can specify both `AuthenticationSchemes = "Bearer"` and `Policy = "CanManageOrders"`, meaning the JWT handler must authenticate and the policy must be satisfied.

---

## Q15. What is resource-based authorization in APIs?

**Concepts**
- Resource-based authorization evaluating permissions against a specific resource instance
- IAuthorizationService.AuthorizeAsync with resource and policy name
- AuthorizationHandler<TRequirement, TResource> receiving the resource for comparison
- Role checks alone insufficient for row-level ownership or tenant isolation
- Running after authentication and entity loading inside the action

**Answer**

Resource-based authorization evaluates permissions against the specific resource being accessed — "does this user own order 123?" — rather than just checking a role or claim in isolation. I call `_authService.AuthorizeAsync(User, order, "EditPolicy")` inside the action after loading the entity, and an `AuthorizationHandler<EditOrderRequirement, Order>` compares the `tenant_id` or `sub` claim from the JWT against the loaded `Order.OwnerId`. Role checks alone cannot express row-level security because two users with the same role may not access the same rows — role says what kind of user they are, but not which specific records they own. The handler receives the actual resource instance, so it can access any property for comparison without a separate database query. This pattern is essential for multi-tenant APIs where two users share a role but must not see each other's data.

---

## Q16. What is multi-tenant authorization for Web APIs?

**Concepts**
- Tenant id embedded in JWT claims or mapped from API key at authentication time
- BelongsToTenant policy comparing route/body tenant to claim
- Data queries filtered by tenant at repository or DbContext level in addition to auth
- Cross-tenant data leakage testing with tokens from different tenants
- EF Core global query filters as defense-in-depth complement to authorization

**Answer**

Multi-tenant authorization ensures each request operates only within the caller's tenant by embedding `tenant_id` in JWT claims at authentication time (or mapping API key → tenant in the authentication handler) and then enforcing it through a `BelongsToTenant` policy that compares the route or body tenant id against the claim. Authorization alone is not enough — data queries must also filter by tenant at the repository or DbContext level so that a code bug in authorization does not accidentally expose all tenants' data. EF Core global query filters (`HasQueryFilter(e => e.TenantId == _currentTenant.Id)`) provide defense-in-depth by making it structurally impossible for a query to return rows from a different tenant. Test specifically by using a token from tenant A to request tenant B resource ids — this is the most common cross-tenant leakage scenario and should fail with 404 or 403, not return tenant B's data.

---

## Q17. How do mobile apps typically authenticate to REST APIs?

**Concepts**
- OAuth 2.0 / OIDC with PKCE for public mobile clients without client secrets
- Short-lived access tokens and refresh token renewal flow
- Secure platform storage: iOS Keychain and Android Keystore
- ASP.NET Core API validating JWT via AddJwtBearer with authority
- Certificate pinning for high-security token endpoint protection

**Answer**

Mobile apps commonly use OAuth 2.0 / OpenID Connect — redirecting to a system browser or embedded web view for user login, receiving access and refresh tokens after the authorization code exchange, then sending the access token as `Authorization: Bearer` on every API call. PKCE (Proof Key for Code Exchange) is required for public mobile clients because they have no client secret to protect the token endpoint exchange. Access tokens are short-lived JWTs; refresh tokens renew access without re-login and must be stored in secure platform storage — iOS Keychain or Android Keystore — never in plain SharedPreferences or localStorage. The ASP.NET Core API validates the JWT via `AddJwtBearer` pointing its `Authority` at Azure AD, Auth0, IdentityServer, or whatever identity provider issues the tokens. Biometric unlock gates access to stored tokens locally but is not a substitute for server authentication, since it only controls whether the token can be read from the secure store, not whether it is valid.

---

## Q18. What is the difference between cookie auth and Bearer token auth for APIs?

**Concepts**
- Cookie auth storing session id in HttpOnly cookie submitted automatically by browsers
- Bearer token auth requiring explicit Authorization header — suitable for non-browser clients
- CSRF protection required for cookie auth, not needed for Bearer
- Cookie auth enabling server-side session revocation
- JWT Bearer stateless unless paired with introspection or a token blocklist

**Answer**

**Cookie authentication** stores the session id in an HttpOnly cookie that browsers submit automatically — suited for same-site server-rendered apps and Blazor Server because the browser handles credential transport. **Bearer token authentication** sends a token in the `Authorization` header, which the client code must attach explicitly — suited for SPAs, mobile apps, and cross-origin API clients that do not rely on automatic cookie submission. Cookies require CSRF protection for state-changing browser requests because a malicious page can trigger cross-site requests that carry the cookie automatically; Bearer APIs typically do not need CSRF protection since the token must be attached explicitly from JavaScript. Cookie auth enables server-side session revocation by deleting the session record; JWT Bearer is stateless and tokens remain valid until expiry unless the API maintains a token blocklist or uses short-lived tokens with introspection. ASP.NET Core Web APIs default to Bearer/JWT for machine and mobile clients; cookies remain for Blazor Server or BFF (Backend for Frontend) hybrid patterns.

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

**Concepts**
- ValidIssuer contradicting the token iss claim causing validation failure
- ValidateAudience = false accepting wrong audiences when issuer were fixed
- UseAuthentication() missing from the pipeline — HttpContext.User never populated
- jwt.io showing valid signature while ASP.NET Core validation checks issuer and audience
- Authority driving OpenID Connect metadata vs explicit TokenValidationParameters conflict

**Answer**

JWT validation is misconfigured in two ways and the pipeline is incomplete in a third. First, `ValidIssuer` is set to `"https://wrong-issuer.example.com"` but the identity provider issues tokens with `iss: https://login.company.com` — every token fails issuer validation and returns 401 regardless of signature validity, which is why jwt.io (which only checks the signature) shows the token as valid but ASP.NET Core rejects it. Second, `ValidateAudience = false` means that if the issuer were fixed, any token from this authority could access the API — the `aud: order-api` claim would never be checked. Third, `app.UseAuthentication()` is missing before `app.UseAuthorization()`, which means the JWT Bearer handler never runs to populate `HttpContext.User` even if validation were configured correctly. The fix in priority order: supply `options.Authority = "https://login.company.com"` and let OpenID Connect metadata drive both issuer validation and signing key discovery, removing the manual `TokenValidationParameters` that contradicts it — or set `ValidIssuer = "https://login.company.com"` to match the token `iss`; set `ValidAudience = "order-api"` and `ValidateAudience = true`; and add `app.UseAuthentication()` before `app.UseAuthorization()`.

---

#### Q2. (R) Same API as Q1 — after fixing JWT validation, `[Authorize]` endpoints still return 401 while anonymous health checks work. Review middleware order and what's missing.

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
```

**Concepts**
- UseAuthentication before UseAuthorization as the required pipeline order
- MapControllers and MapHealthChecks calling order relative to middleware
- [AllowAnonymous] on health checks bypassing authorization correctly
- HttpContext.User populated by UseAuthentication for UseAuthorization to evaluate

**Answer**

The middleware order shown — `UseRouting`, `UseAuthentication`, `UseAuthorization`, then `MapControllers` — is actually correct for ASP.NET Core 8. If `[Authorize]` endpoints still return 401 after fixing the JWT configuration from Q1, the most likely remaining cause is that the JWT Bearer handler is still rejecting the token at the `ValidateAudience` or `ValidIssuer` level — a misconfigured `TokenValidationParameters` silently causes 401 even when middleware order is correct. I would enable JWT Bearer event logging (`options.Events = new JwtBearerEvents { OnAuthenticationFailed = ctx => { ... } }`) to surface the exact validation failure. If the health check works anonymously while protected endpoints return 401, `UseAuthentication()` is running and reaching the handler — the failure is in the token validation itself, not the pipeline order. Confirm by testing with a token from a tool that sends the exact same claims the staging IdP would issue, and compare the `iss`, `aud`, `exp`, and signing key against `TokenValidationParameters`.

---

#### Q3. (P) When should you use **`[Authorize(Roles = "Admin")]`** vs **`[Authorize(Policy = "CanManageOrders")]`** on Web API controllers? How do you register policies, and what breaks when you only use roles for fine-grained API access?

**Concepts**
- Role-based authorization checking a single claim, policy composing multiple requirements
- AddAuthorization for named policy registration
- IAuthorizationHandler for requirements that need injected services or resource context
- Roles insufficient for OAuth scope enforcement and multi-tenant row-level rules
- Policies centralizing authorization logic rather than scattering across attributes

**Answer**

I use `[Authorize(Roles = "Admin")]` only for coarse, stable role checks where the role claim in the JWT is the complete authorization decision — for example, "only Admins can delete tenants" where there is no per-resource or scope component. I use `[Authorize(Policy = "CanManageOrders")]` when the authorization rule involves more than one condition, changes independently of the role taxonomy, or requires injected services. Register named policies in `Program.cs` with `builder.Services.AddAuthorization(options => options.AddPolicy("CanManageOrders", p => p.RequireRole("OrderManager").RequireClaim("scope", "orders.write")))`. What breaks when you only use roles for fine-grained API access: first, machine-to-machine clients using client credentials flows have no user roles, only scopes — role-only checks reject valid integration tokens; second, multi-tenant logic ("does this user manage orders for their own tenant?") requires resource context that a role claim alone cannot provide; third, rules scattered across dozens of `[Authorize(Roles = "...")]` attributes have no central location to update when the role taxonomy changes. Policies consolidate all of that into one registration that can be updated without touching controllers.

---

#### Q4. (M) An API accepts Azure AD JWTs. One endpoint must allow users with **`scope: orders.read`**; another requires **`scope: orders.write`**. A token has role `OrderClerk` but no scopes. Explain how **scope claims** differ from **role claims**, and how you enforce scopes in ASP.NET Core authorization.

**Concepts**
- Scopes as client application delegated permissions vs roles as user group membership
- scp and scope claim name differences between Azure AD and standard OAuth
- RequireClaim for scope enforcement in named policies
- Machine-to-machine client credentials tokens having scopes but no roles
- Role-only authorization rejecting valid scope-only integration tokens

**Answer**

Roles describe who the user is — `OrderClerk` is a group membership that persists across sessions and client applications. Scopes describe what this specific client application is permitted to do on behalf of the user — `orders.write` is a delegated permission the user granted to the client during the OAuth consent flow, and it may vary between the mobile app and the partner integration even for the same user. They are separate claims and require separate enforcement. Azure AD emits the `scp` claim (space-delimited string) for delegated scopes rather than the standard OAuth `scope` claim, so the policy must account for the claim name the issuer uses. A token with role `OrderClerk` but no scope claims will fail a scope policy because the user's role does not grant the client application permission — the client must have been granted that scope during registration. Register `builder.Services.AddAuthorization(options => { options.AddPolicy("ReadOrders", p => p.RequireClaim("scp", "orders.read")); options.AddPolicy("WriteOrders", p => p.RequireClaim("scp", "orders.write")); })` and apply `[Authorize(Policy = "ReadOrders")]` and `[Authorize(Policy = "WriteOrders")]` to the respective endpoints. If the API must support both Azure AD (scp) and standard OAuth (scope), write a custom `IAuthorizationHandler` that checks both claim names.

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

**Concepts**
- Query string API keys appearing in server access logs and browser history
- Header transport as the required API key delivery mechanism
- Comparison against a single hardcoded option vs per-client key store
- AuthenticateResult.Fail vs NoResult for missing vs invalid keys
- Timing-safe comparison for secret values

**Answer**

The critical issue is that `Request.Query.TryGetValue("apiKey", ...)` reads the API key from the query string — query strings are written to web server access logs, nginx logs, CDN logs, and browser history, which means the key is exposed in plain text in multiple log stores accessible to operators and potentially attackers. The fix is to read exclusively from a request header: `Request.Headers.TryGetValue("X-Api-Key", out var key)` with `return Task.FromResult(AuthenticateResult.NoResult())` when the header is absent, so other schemes can still attempt authentication on the same request. The current code also compares against a single hardcoded `_options.ApiKey` rather than looking up per-client keys in a store, which means all clients share one key and there is no way to revoke a single client without rotating the key for everyone. Replace with an async lookup against a hashed key store: `var clientId = await _keyStore.ValidateAsync(key)` and build claims from the looked-up client identity. Also use `CryptographicOperations.FixedTimeEquals` or equivalent for the comparison to prevent timing attacks.

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

**Concepts**
- [AllowAnonymous] on Register bypassing class-level [Authorize] for that action
- MetricsController missing [Authorize] and no global fallback policy
- No fallback authorization policy leaving unannotated controllers publicly accessible
- Internal sensitive endpoints requiring explicit [Authorize] or network-level controls
- Global fallback policy via AddAuthorization as defense-in-depth

**Answer**

Two endpoints are anonymously reachable. First, `POST /api/customers` (`Register`) is intentionally anonymous via `[AllowAnonymous]`, which overrides the class-level `[Authorize]` — this is presumably correct. Second, `GET /api/internal/metrics` is anonymously reachable because `MetricsController` has no `[Authorize]` attribute and there is no global fallback authorization policy configured, so the endpoint is completely unprotected and returns a metrics snapshot to anyone who discovers the URL. The path prefix `/api/internal/` provides no security — it is just a routing convention. The fix for `MetricsController` is to add `[Authorize]` (or a specific policy like `[Authorize(Policy = "InternalOnly")]`) and gate it behind an IP allowlist or network policy in the ingress. For defense-in-depth across the whole API, configure a global fallback policy with `builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())` so any endpoint missing an explicit `[Authorize]` is still protected — unannotated controllers fail closed rather than open.

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

**Concepts**
- Authorization header containing the Bearer token — a credential, not metadata
- Logging credentials violating PCI DSS and GDPR log retention requirements
- Query string logging exposing API keys or access codes if present in URLs
- Redacting or omitting Authorization header in structured logging
- Scope-limiting diagnostic middleware to Development environment only

**Answer**

The middleware logs `Auth={Auth}` which resolves to the full `Authorization: Bearer eyJhbG...` header value — the complete JWT, which is a credential valid until expiry. Anyone with read access to the log storage (operators, log aggregation vendors, monitoring dashboards) can extract the token and replay it to impersonate the user. This violates PCI DSS log data requirements and GDPR data minimization principles since JWTs often carry user ids, email, and roles. The `Query={Query}` line also captures full query strings, which may include API keys or OAuth authorization codes in some flows. The fixes in priority order: remove the Authorization header from the log entirely or replace the value with a redacted placeholder — `Auth=[redacted]` or log only the scheme (`Bearer`) without the credential; remove the raw query string log and replace with structured extraction of only non-sensitive parameters; gate this middleware behind an environment check so it only runs in Development where logs are not retained long-term or sent to external storage. If correlation context is the goal, log `TraceIdentifier` or the `X-Correlation-Id` header instead of the token.

---

#### Q8. (P) Your API supports **JWT Bearer** for mobile apps and **API Key** header for batch jobs. Both schemes are registered. Explain **authentication scheme order**, default scheme selection, and how **`[Authorize(AuthenticationSchemes = "...")]`** prevents the wrong handler from running first.

**Concepts**
- Default authentication scheme from AddAuthentication driving unannotated [Authorize]
- Authentication handler running order for multi-scheme APIs
- [Authorize(AuthenticationSchemes)] locking an endpoint to a specific scheme
- AuthenticateResult.NoResult delegating to the next scheme vs Fail stopping the chain
- WWW-Authenticate challenge header mismatch when wrong scheme runs on wrong endpoint

**Answer**

When `AddAuthentication("Bearer")` is called with `"Bearer"` as the default, every unannotated `[Authorize]` attribute invokes only the JWT Bearer handler — the API key handler does not run unless explicitly requested. Authentication handlers do not automatically fall through in a cascade; the default scheme runs alone unless the endpoint specifies `AuthenticationSchemes`. Without explicit scheme annotation on the batch endpoints, the JWT Bearer handler runs first on an API key request, finds no `Authorization: Bearer` header, and returns a `WWW-Authenticate: Bearer` challenge — the batch client receives a confusing 401 asking for a token it does not use. Apply `[Authorize(AuthenticationSchemes = "ApiKey")]` to all batch-integration controllers so the API key handler runs exclusively for those endpoints, preventing the JWT challenge. The API key handler's `HandleAuthenticateAsync` should return `AuthenticateResult.NoResult()` when the `X-Api-Key` header is absent (not `Fail`), so that on a hybrid endpoint annotated with `"Bearer,ApiKey"`, the JWT handler still gets a chance to authenticate. This NoResult vs Fail distinction is important: `NoResult` means "I didn't find my credentials, let another handler try"; `Fail` means "I found credentials but they are invalid, stop here."

---

#### Q9. (D) Design authorization for a multi-tenant orders API: tenants must only read their own data; admins can read all; partner integrations use API keys scoped to one tenant. Compare role-only, policy + requirements, and resource-based authorization — what would you implement and where?

**Concepts**
- Role-only authorization insufficient for row-level tenant isolation
- Policy + IAuthorizationRequirement for claim-level tenant matching
- Resource-based authorization for per-entity ownership checks
- EF Core global query filters as defense-in-depth at the data layer
- API key handler embedding tenant_id claim during authentication

**Answer**

Role-only authorization cannot solve this problem because two users can share the same role (`OrderViewer`) but belong to different tenants — the role says nothing about which tenant's data they may access. Policy + requirements gets closer: a `BelongsToTenantRequirement` policy compares the `tenant_id` claim in the JWT or the tenant id mapped from the API key against the tenant id in the request route or body, enforced via an `IAuthorizationHandler` registered in DI. This handles the common case but still does not prevent an attacker from crafting a request to a different tenant's order id if the handler only checks the route prefix. Resource-based authorization handles the per-entity check: after loading the `Order` from the database, call `await _authService.AuthorizeAsync(User, order, "BelongsToCurrentTenant")` inside the action — the handler compares `order.TenantId` against `User.FindFirst("tenant_id")?.Value`. I would implement all three layers: the API key handler embeds `tenant_id` as a claim during authentication; a policy + requirement enforces it at the endpoint level before the action runs; resource-based authorization enforces it at the entity level after loading; and EF Core global query filters (`HasQueryFilter(o => o.TenantId == _currentTenant.Id)`) provide defense-in-depth so even a missed authorization check cannot return cross-tenant rows. Admins get a bypass in the policy handler by checking for the `Admin` role before the tenant comparison.

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

**Concepts**
- [AllowAnonymous] bypassing ASP.NET authorization while application-level HMAC verifies identity
- HMAC signature verification using raw request body before any deserialization
- Request body buffering required to read body in middleware and then again in action
- Rate limiting and idempotency key protection for webhook endpoints
- [AllowAnonymous] appropriate when the caller authenticates via a non-Bearer mechanism

**Answer**

`[AllowAnonymous]` is appropriate here because Stripe authenticates via HMAC signature over the raw request body, not via a Bearer token — the standard ASP.NET Core authentication pipeline cannot verify this credential type. The pattern is correct in principle: bypass the Bearer authorization layer and implement application-level verification inside the action. The risks to address are: first, the action reads `Request.Body` with a `StreamReader` directly — if any middleware has already buffered or read the body, the stream position may be at the end and `json` will be empty, causing HMAC verification to fail with a silent `BadRequest`; enable `app.Use` request buffering or use `EnableBuffering()` to reset the stream if needed. Second, if `_stripe.Verify` returns false, return 400 — but also log the failure with enough context to detect probing. Third, the endpoint is publicly reachable by anyone, so rate limiting at the gateway level prevents abuse even though invalid signatures are rejected. Fourth, process webhook events idempotently using Stripe's `event.id` so retried deliveries do not double-process. `[AllowAnonymous]` is the wrong choice when there is no application-level verification at all — anonymous endpoints with no auth of any kind are exposed.
