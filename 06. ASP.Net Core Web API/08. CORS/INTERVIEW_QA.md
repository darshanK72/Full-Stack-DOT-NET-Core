# CORS — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 08. CORS](#chapter-08-cors)
  - [Q1. What is CORS?](#chapter-08-cors-q1)
  - [Q2. Why do browsers enforce CORS for Web APIs?](#chapter-08-cors-q2)
  - [Q3. What is a cross-origin request?](#chapter-08-cors-q3)
  - [Q4. What is a CORS preflight request?](#chapter-08-cors-q4)
  - [Q5. When does a browser send an OPTIONS preflight?](#chapter-08-cors-q5)
  - [Q6. What is the `Access-Control-Allow-Origin` header?](#chapter-08-cors-q6)
  - [Q7. What is the difference between `AllowAnyOrigin` and `WithOri…](#chapter-08-cors-q7)
  - [Q8. Why can't `AllowAnyOrigin` be used with `AllowCredentials`?](#chapter-08-cors-q8)
  - [Q9. What does `AllowHeaders` configure?](#chapter-08-cors-q9)
  - [Q10. What does `WithExposedHeaders` do?](#chapter-08-cors-q10)
  - [Q11. What is the correct middleware order for `UseCors` in a Web …](#chapter-08-cors-q11)
  - [Q12. What is the difference between simple and non-simple CORS re…](#chapter-08-cors-q12)
  - [Q13. Does CORS protect the API server from unauthorized access?](#chapter-08-cors-q13)
  - [Q14. What is `Access-Control-Allow-Credentials`?](#chapter-08-cors-q14)
  - [Q15. What is the difference between CORS errors and 401 Unauthori…](#chapter-08-cors-q15)
  - [Q16. What does `Access-Control-Allow-Methods` specify?](#chapter-08-cors-q16)
  - [Q17. When should CORS be configured at the API vs API gateway?](#chapter-08-cors-q17)
  - [Q18. What is a CORS policy in ASP.NET Core?](#chapter-08-cors-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 08. CORS

### Q1. What is CORS? {#chapter-08-cors-q1}

What is CORS?

**Answer:** Cross-Origin Resource Sharing (CORS) is a browser security mechanism that controls whether a web page from one origin (scheme + host + port) can access resources from a different origin via JavaScript. ASP.NET Core implements CORS through middleware and policy configuration.

- CORS is enforced by browsers only — it does not affect server-to-server calls, curl, or Postman.
- The server responds with CORS headers (`Access-Control-Allow-Origin`, etc.) telling the browser whether to expose the response to JavaScript.
- ASP.NET Core configures CORS via `AddCors()` service registration and `UseCors()` middleware with named policies.
- CORS is not a substitute for authentication — it controls browser access, not API authorization.

---

### Q2. Why do browsers enforce CORS for Web APIs? {#chapter-08-cors-q2}

Why do browsers enforce CORS for Web APIs?

**Answer:** Browsers enforce the Same-Origin Policy to prevent malicious websites from reading responses from other origins using the user's credentials. CORS provides a controlled exception — the server explicitly permits specific origins to access its resources via JavaScript.

- Without CORS, any website could call your API from the user's browser and read sensitive response data.
- CORS headers are the server's way of opting in to cross-origin browser access.
- Same-origin requests (SPA and API on the same host/port) do not trigger CORS checks.
- CORS protects users browsing the web — it does not protect the API from direct non-browser access.

---

### Q3. What is a cross-origin request? {#chapter-08-cors-q3}

What is a cross-origin request?

**Answer:** A cross-origin request occurs when the JavaScript origin (scheme, host, and port) of the web page differs from the origin of the API being called. For example, a SPA at `https://app.example.com` calling an API at `https://api.example.com` is cross-origin.

- `https://app.example.com:443` vs `https://api.example.com:443` — different host, cross-origin.
- `http://localhost:3000` vs `http://localhost:5000` — different port, cross-origin.
- `https://example.com` vs `https://example.com` — same origin, no CORS check.
- The browser sends an `Origin` header on cross-origin requests; the server must echo it in `Access-Control-Allow-Origin`.

---

### Q4. What is a CORS preflight request? {#chapter-08-cors-q4}

What is a CORS preflight request?

**Answer:** A CORS preflight is an automatic `OPTIONS` request sent by the browser before the actual request when the request is "non-simple" — for example, JSON POST with `Content-Type: application/json` or requests with custom headers like `Authorization`.

- The browser sends `OPTIONS` with `Access-Control-Request-Method` and `Access-Control-Request-Headers`.
- The server must respond with appropriate `Access-Control-Allow-*` headers and a 2xx status without requiring authentication.
- Only after a successful preflight does the browser send the actual GET, POST, PUT, or DELETE request.
- Failed preflights block the actual request — the browser reports a CORS error, not the underlying HTTP status.

---

### Q5. When does a browser send an OPTIONS preflight? {#chapter-08-cors-q5}

When does a browser send an OPTIONS preflight?

**Answer:** Browsers send an OPTIONS preflight for non-simple cross-origin requests — those using methods other than GET/HEAD/POST, custom headers beyond the CORS-safelist, or `Content-Type` values other than `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`.

- JSON POST with `Content-Type: application/json` always triggers preflight.
- Requests with `Authorization` header (Bearer JWT) trigger preflight.
- Custom headers like `X-Request-Id` or `X-Api-Key` trigger preflight.
- Simple GET requests without custom headers typically do not preflight.

---

### Q6. What is the `Access-Control-Allow-Origin` header? {#chapter-08-cors-q6}

What is the `Access-Control-Allow-Origin` header?

**Answer:** `Access-Control-Allow-Origin` tells the browser which origin is permitted to read the response via JavaScript. The server echoes the requesting origin or a specific allowed origin — never a list of multiple origins in a single header value.

- Example: `Access-Control-Allow-Origin: https://app.example.com`.
- `Access-Control-Allow-Origin: *` allows any origin but cannot be combined with credentials.
- ASP.NET Core's `WithOrigins("https://app.example.com")` sets this header for matching requests.
- The browser blocks JavaScript access to the response if this header is missing or does not match the requesting origin.

---

### Q7. What is the difference between `AllowAnyOrigin` and `WithOrigins`? {#chapter-08-cors-q7}

What is the difference between `AllowAnyOrigin` and `WithOrigins`?

**Answer:** `AllowAnyOrigin()` sets `Access-Control-Allow-Origin: *` for all origins — permissive but incompatible with credentials. `WithOrigins("https://app.example.com")` sets the header to specific allowed origins, required when cookies or credentials are involved.

- `AllowAnyOrigin()` is acceptable for public read-only APIs in development without authentication.
- `WithOrigins()` requires listing each allowed origin explicitly — load from configuration per environment.
- ASP.NET Core does not support wildcard subdomains in `WithOrigins` natively — list each subdomain or implement custom `ICorsPolicyProvider`.
- Production APIs with authenticated users must use explicit origin allowlists.

---

### Q8. Why can't `AllowAnyOrigin` be used with `AllowCredentials`? {#chapter-08-cors-q8}

Why can't `AllowAnyOrigin` be used with `AllowCredentials`?

**Answer:** The CORS specification forbids combining `Access-Control-Allow-Origin: *` with `Access-Control-Allow-Credentials: true`. Browsers reject this combination because wildcard origin with credentials would allow any site to access authenticated responses.

- When credentials (cookies, client certificates) are included, the server must echo a specific origin.
- ASP.NET Core throws at startup or the browser blocks the response if both are configured together.
- For Bearer tokens in the `Authorization` header, `AllowCredentials()` may not be needed — but explicit origins are still required if credentials mode is enabled.
- Fix: replace `AllowAnyOrigin()` with `WithOrigins("https://app.example.com").AllowCredentials()`.

---

### Q9. What does `AllowHeaders` configure? {#chapter-08-cors-q9}

What does `AllowHeaders` configure?

**Answer:** `AllowHeaders` (or `WithHeaders` / `AllowAnyHeader`) configures which request headers the browser may send on cross-origin requests. The server echoes allowed headers in `Access-Control-Allow-Headers` on preflight responses.

- Must include `Authorization` for JWT Bearer token requests — otherwise preflight fails.
- Must include `Content-Type` for JSON POST requests with `application/json`.
- Custom headers like `X-Request-Id` or `X-Api-Version` must be explicitly allowed or use `AllowAnyHeader()`.
- Missing header permissions cause preflight failure — the browser blocks the actual request before it reaches authentication.

---

### Q10. What does `WithExposedHeaders` do? {#chapter-08-cors-q10}

What does `WithExposedHeaders` do?

**Answer:** `WithExposedHeaders` configures which response headers JavaScript can read from cross-origin responses via `fetch` or XHR. By default, browsers expose only CORS-safelisted response headers — custom headers require explicit exposure.

- Sets `Access-Control-Expose-Headers` — for example, `Content-Disposition`, `X-Total-Count`, `X-Pagination`.
- Without exposure, JavaScript cannot read pagination totals or download filenames from response headers.
- Example: `.WithExposedHeaders("Content-Disposition", "X-Total-Count")`.
- Alternatively, return metadata in the JSON body to avoid CORS header exposure complexity.

---

### Q11. What is the correct middleware order for `UseCors` in a Web API? {#chapter-08-cors-q11}

What is the correct middleware order for `UseCors` in a Web API?

**Answer:** CORS middleware must run after routing and before authentication and authorization — typically: `UseRouting()` → `UseCors()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`.

- CORS must execute before auth so preflight OPTIONS requests succeed without authentication.
- CORS headers must be added to error responses (401, 403) — if auth runs before CORS, browsers report CORS errors instead of auth failures.
- Middleware registered after `MapControllers()` does not run for matched endpoints — CORS must precede endpoint mapping.
- Use a named policy: `app.UseCors("DefaultPolicy")` matching the policy registered in `AddCors()`.

---

### Q12. What is the difference between simple and non-simple CORS requests? {#chapter-08-cors-q12}

What is the difference between simple and non-simple CORS requests?

**Answer:** Simple requests skip preflight and proceed directly — GET/HEAD/POST with safelisted headers and Content-Type values (`text/plain`, `application/x-www-form-urlencoded`, `multipart/form-data`). Non-simple requests trigger an OPTIONS preflight before the actual request.

- Simple: `GET /api/products` with no custom headers from a cross-origin SPA — no preflight.
- Non-simple: `POST /api/orders` with `Content-Type: application/json` and `Authorization: Bearer ...` — preflight required.
- Non-simple: PUT, PATCH, DELETE methods always trigger preflight.
- Preflight adds latency — one extra round trip before the actual request executes.

---

### Q13. Does CORS protect the API server from unauthorized access? {#chapter-08-cors-q13}

Does CORS protect the API server from unauthorized access?

**Answer:** No. CORS is a browser-enforced policy that prevents JavaScript on unauthorized websites from reading API responses. It does not block direct HTTP requests from curl, Postman, server-to-server calls, or malicious scripts running outside a browser context.

- Authentication and authorization middleware protect the API from unauthorized access.
- CORS only controls which browser origins can read responses via JavaScript.
- A public API without auth is accessible to anyone regardless of CORS configuration.
- Treat CORS as a browser UX/security feature, not an API security boundary.

---

### Q14. What is `Access-Control-Allow-Credentials`? {#chapter-08-cors-q14}

What is `Access-Control-Allow-Credentials`?

**Answer:** `Access-Control-Allow-Credentials: true` tells the browser it may include credentials (cookies, HTTP authentication, client certificates) in cross-origin requests and expose the authenticated response to JavaScript. Requires a specific origin in `Access-Control-Allow-Origin`, not a wildcard.

- ASP.NET Core: `.AllowCredentials()` on the CORS policy sets this header.
- The client must also set `credentials: 'include'` in fetch or `withCredentials: true` in XHR.
- Required for cookie-based authentication in cross-origin SPAs.
- Bearer tokens in the `Authorization` header do not require credentials mode unless cookies are also sent.

---

### Q15. What is the difference between CORS errors and 401 Unauthorized? {#chapter-08-cors-q15}

What is the difference between CORS errors and 401 Unauthorized?

**Answer:** A CORS error occurs when the browser blocks JavaScript from reading a response due to missing or incorrect CORS headers — the actual HTTP status may be 200 or 401, but JavaScript cannot see it. A 401 Unauthorized is an authentication failure returned by the server that JavaScript can read if CORS headers are present.

- CORS error in DevTools console: "blocked by CORS policy" — often caused by middleware order or missing CORS on error responses.
- 401 with proper CORS headers: JavaScript can read the status and response body — the client handles re-authentication.
- 401 without CORS headers on a cross-origin request: browser reports a CORS error, masking the real auth failure.
- Fix middleware order first when diagnosing "CORS error on 401" — ensure `UseCors()` runs before `UseAuthentication()`.

---

### Q16. What does `Access-Control-Allow-Methods` specify? {#chapter-08-cors-q16}

What does `Access-Control-Allow-Methods` specify?

**Answer:** `Access-Control-Allow-Methods` lists the HTTP methods the browser may use on cross-origin requests. It appears in preflight OPTIONS responses, echoing permitted methods such as GET, POST, PUT, PATCH, and DELETE.

- ASP.NET Core: `.WithMethods("GET", "POST", "PUT", "DELETE")` or `.AllowAnyMethod()`.
- Must include the method used by the actual request — otherwise preflight succeeds but the real request method is blocked.
- Preflight sends `Access-Control-Request-Method: POST`; server responds with `Access-Control-Allow-Methods: POST`.
- Restrict methods in production policies — avoid `AllowAnyMethod()` when only GET and POST are needed.

---

### Q17. When should CORS be configured at the API vs API gateway? {#chapter-08-cors-q17}

When should CORS be configured at the API vs API gateway?

**Answer:** Configure CORS at the API gateway or reverse proxy for public multi-tenant APIs with centralized origin allowlists across microservices. Configure CORS in the ASP.NET Core app when the app is directly exposed without a gateway, or for development and internal SPAs hitting the app directly.

- Gateway/APIM: central policy, consistent allowlist, preflight caching — one place to manage origins for all backend services.
- ASP.NET Core app: direct public exposure, local development, internal services without an edge gateway.
- Avoid duplicate CORS headers from both gateway and app — browsers reject responses with multiple `Access-Control-Allow-Origin` values.
- Document a single CORS owner in the platform runbook — on-call fixes one layer, not two.

---

### Q18. What is a CORS policy in ASP.NET Core? {#chapter-08-cors-q18}

What is a CORS policy in ASP.NET Core?

**Answer:** A CORS policy is a named set of rules registered in `AddCors()` defining allowed origins, methods, headers, exposed headers, and credential support. The policy is applied via `UseCors("PolicyName")` middleware or `[EnableCors("PolicyName")]` on controllers and actions.

- Register: `builder.Services.AddCors(options => options.AddPolicy("Default", builder => builder.WithOrigins(...).AllowAnyHeader().AllowAnyMethod()))`.
- Apply globally: `app.UseCors("Default")` in the middleware pipeline.
- Apply per-controller: `[EnableCors("AdminPolicy")]` for different rules on different endpoint groups.
- Load allowed origins from `appsettings.{Environment}.json` via `IOptions` for environment-specific configuration.

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

#### Q1. (R) Review this CORS policy for a SPA that sends JWT cookies on cross-origin requests. Browser console shows CORS error; policy appears permissive.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaPolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();
app.UseCors("SpaPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Frontend: `https://app.example.com` calling API at `https://api.example.com` with `credentials: 'include'`.

---

**Answer:**

_Answer not found._

---

#### Q2. (R) Review preflight failures on a custom header. Simple GET works from Postman; browser POST with `X-Request-Id` and `Authorization` fails before reaching the controller.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.WithOrigins("https://portal.example.com")
              .AllowAnyMethod());
});

app.UseRouting();
app.UseCors("Default");
app.UseAuthentication();
app.MapControllers();
```

No explicit `AllowHeaders`; client sends `Content-Type: application/json`, `Authorization: Bearer ...`, `X-Request-Id: abc`.

---

**Answer:**

_Answer not found._

---

#### Q3. (M) A JWT-protected API returns 401 on cross-origin requests; developers say "CORS is broken" because the response lacks `Access-Control-Allow-Origin`. Walk through middleware order — `UseCors`, `UseAuthentication`, `UseAuthorization`, endpoint — and explain when CORS headers appear on error responses vs when auth fails first.

---

**Answer:**

_Answer not found._

---

#### Q4. (P) Production requires an explicit origin allowlist for three frontends and a staging tenant. Review this configuration approach vs `AllowAnyOrigin()` — environment-specific policies, secrets, and wildcard subdomain pitfalls.

```csharp
// appsettings.Production.json
"Cors": {
  "AllowedOrigins": [
    "https://app.example.com",
    "https://admin.example.com",
    "https://*.staging.example.com"
  ]
}
```

ASP.NET Core `WithOrigins` does not support `*.staging.example.com` wildcards natively.

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review exposed headers for a file-download API. JavaScript client cannot read `Content-Disposition` or custom `X-Total-Count` from XHR despite successful 200.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Api", policy =>
        policy.WithOrigins("https://app.example.com")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
```

Response includes `Content-Disposition: attachment; filename="report.pdf"` and `X-Total-Count: 1500`; no `WithExposedHeaders` configured.

---

**Answer:**

_Answer not found._

---

#### Q6. (P) Review middleware placement in `Program.cs`. CORS works for anonymous endpoints but authenticated routes still fail preflight in some environments.

```csharp
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("Default");
app.MapControllers();
```

Policy registered with `WithOrigins("https://app.example.com")` and `AllowAnyMethod()`.

---

**Answer:**

_Answer not found._

---

#### Q7. (R) Review this global CORS middleware placed after endpoints. Local development works when hitting Kestrel directly; fails behind IIS reverse proxy with custom domain.

```csharp
var app = builder.Build();

app.MapControllers();
app.UseCors("DevPolicy");  // DevPolicy: AllowAnyOrigin, no credentials

app.Run();
```

IIS site binds `https://api.internal.corp`; SPA at `https://spa.internal.corp`.

---

**Answer:**

_Answer not found._

---

#### Q8. (D) API team owns ASP.NET Core CORS; platform team adds Azure API Management in front with its own CORS policy. Browser sees duplicate or conflicting `Access-Control-Allow-Origin` headers. Who should own CORS in production, and how do you avoid double-application?



**Answer:**

_Answer not found._

---
