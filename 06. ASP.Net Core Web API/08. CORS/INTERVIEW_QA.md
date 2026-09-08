# CORS — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is CORS?](#q1-what-is-cors)
2. [Q2. Why do browsers enforce CORS for Web APIs?](#q2-why-do-browsers-enforce-cors-for-web-apis)
3. [Q3. What is a cross-origin request?](#q3-what-is-a-cross-origin-request)
4. [Q4. What is a CORS preflight request?](#q4-what-is-a-cors-preflight-request)
5. [Q5. When does a browser send an OPTIONS preflight?](#q5-when-does-a-browser-send-an-options-preflight)
6. [Q6. What is the `Access-Control-Allow-Origin` header?](#q6-what-is-the-access-control-allow-origin-header)
7. [Q7. What is the difference between `AllowAnyOrigin` and `WithOrigins`?](#q7-what-is-the-difference-between-allowanyorigin-and-withorigins)
8. [Q8. Why can't `AllowAnyOrigin` be used with `AllowCredentials`?](#q8-why-cant-allowanyorigin-be-used-with-allowcredentials)
9. [Q9. What does `AllowHeaders` configure?](#q9-what-does-allowheaders-configure)
10. [Q10. What does `WithExposedHeaders` do?](#q10-what-does-withexposedheaders-do)
11. [Q11. What is the correct middleware order for `UseCors` in a Web API?](#q11-what-is-the-correct-middleware-order-for-usecors-in-a-web-api)
12. [Q12. What is the difference between simple and non-simple CORS requests?](#q12-what-is-the-difference-between-simple-and-non-simple-cors-requests)
13. [Q13. Does CORS protect the API server from unauthorized access?](#q13-does-cors-protect-the-api-server-from-unauthorized-access)
14. [Q14. What is `Access-Control-Allow-Credentials`?](#q14-what-is-access-control-allow-credentials)
15. [Q15. What is the difference between CORS errors and 401 Unauthorized?](#q15-what-is-the-difference-between-cors-errors-and-401-unauthorized)
16. [Q16. What does `Access-Control-Allow-Methods` specify?](#q16-what-does-access-control-allow-methods-specify)
17. [Q17. When should CORS be configured at the API vs API gateway?](#q17-when-should-cors-be-configured-at-the-api-vs-api-gateway)
18. [Q18. What is a CORS policy in ASP.NET Core?](#q18-what-is-a-cors-policy-in-aspnet-core)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is CORS?

**Concepts**
- CORS — browser security mechanism controlling cross-origin JavaScript access
- Same-Origin Policy — the browser rule CORS relaxes
- `Access-Control-Allow-Origin` — server opt-in header
- Browser-only enforcement — non-browser clients unaffected
- `AddCors()` / `UseCors()` — ASP.NET Core registration and middleware

**Answer**

Cross-Origin Resource Sharing is a browser security mechanism that controls whether JavaScript running on one origin can access resources from a different origin. Without CORS headers, the browser's Same-Origin Policy blocks JavaScript from reading cross-origin API responses, protecting users from malicious websites that would otherwise read their authenticated data from other services. The server opts into cross-origin access by responding with `Access-Control-Allow-Origin` and related headers that tell the browser which origins, methods, and headers are permitted. CORS is enforced entirely by the browser — curl, Postman, and server-to-server HTTP clients ignore CORS headers completely, which means CORS is not a security mechanism for the API itself. In ASP.NET Core 8 I configure CORS with `AddCors()` in service registration and `UseCors()` middleware in the pipeline.

---

## Q2. Why do browsers enforce CORS for Web APIs?

**Concepts**
- Same-Origin Policy — prevents cross-origin response reading by default
- User credential protection — cookies and sessions on the user's behalf
- Server opt-in model — server explicitly permits cross-origin access
- Same-origin requests — no CORS check when SPA and API share host/port

**Answer**

Browsers enforce the Same-Origin Policy to prevent malicious websites from reading responses from other origins using the user's browser credentials. Without this protection, any website could execute JavaScript that calls your API as the logged-in user — reading account data, initiating transactions, or extracting sensitive information — without the user's knowledge. CORS provides a controlled exception: the server declares which origins may access its resources, so a legitimate SPA at `https://app.example.com` can be permitted to call `https://api.example.com` while arbitrary malicious sites cannot. Same-origin requests — where the SPA and API share the same scheme, host, and port — never trigger CORS checks since there is no cross-origin boundary to cross.

---

## Q3. What is a cross-origin request?

**Concepts**
- Origin — scheme + host + port combination
- Cross-origin — any difference in scheme, host, or port
- `Origin` request header — sent by browser on cross-origin requests
- `Access-Control-Allow-Origin` — server echoes permitted origin
- Port difference — `localhost:3000` vs `localhost:5000` is cross-origin

**Answer**

A cross-origin request occurs when the JavaScript origin — the combination of scheme, host, and port of the web page — differs in any dimension from the origin of the API being called. `https://app.example.com` calling `https://api.example.com` is cross-origin due to the different host. `http://localhost:3000` calling `http://localhost:5000` is cross-origin due to the different port, even though both are localhost. The browser sends an `Origin` header on cross-origin requests containing the calling page's origin, and the server must respond with `Access-Control-Allow-Origin` echoing either that origin or a wildcard for the browser to allow JavaScript access to the response. If the server's CORS headers do not match the request origin, the browser blocks JavaScript from reading the response — the request reaches the server and a response comes back, but JavaScript cannot see it.

---

## Q4. What is a CORS preflight request?

**Concepts**
- Preflight — automatic `OPTIONS` request before the actual request
- Non-simple requests — trigger preflight (JSON Content-Type, custom headers, non-safe methods)
- `Access-Control-Request-Method` / `Access-Control-Request-Headers` — preflight metadata
- Preflight response must be 2xx — without requiring authentication
- Failed preflight — blocks actual request, browser reports CORS error

**Answer**

A CORS preflight is an automatic `OPTIONS` HTTP request that the browser sends before the actual request when the cross-origin call is non-simple. The browser sends the preflight with `Access-Control-Request-Method` indicating the intended method and `Access-Control-Request-Headers` listing the custom headers the actual request will include. The server must respond to the `OPTIONS` request with the appropriate `Access-Control-Allow-*` headers and a 2xx status code — without requiring authentication — to signal that the actual request is permitted. Only after a successful preflight does the browser send the real GET, POST, PUT, or DELETE. A failed preflight blocks the actual request entirely, and the browser reports a CORS error in DevTools rather than the real HTTP status that the server would have returned.

---

## Q5. When does a browser send an OPTIONS preflight?

**Concepts**
- Non-simple request — triggers preflight
- Simple request criteria — GET/HEAD/POST with safelisted headers and Content-Types
- `Content-Type: application/json` — always triggers preflight
- `Authorization` header — triggers preflight
- Custom headers like `X-Request-Id` — trigger preflight

**Answer**

The browser sends an OPTIONS preflight for any cross-origin request that does not meet the "simple request" criteria. A request is simple if it uses GET, HEAD, or POST, uses only CORS-safelisted request headers, and has a Content-Type of `text/plain`, `application/x-www-form-urlencoded`, or `multipart/form-data`. In practice, almost every API request triggers preflight because JSON POST with `Content-Type: application/json` is non-simple, any request with `Authorization: Bearer <token>` is non-simple since Authorization is not in the safelist, and any custom header like `X-Request-Id` or `X-Api-Version` is non-simple. GET requests without custom headers skip preflight and go directly to the server, which is why simple GET endpoints appear to work from Postman but fail from a browser with a missing CORS policy.

---

## Q6. What is the `Access-Control-Allow-Origin` header?

**Concepts**
- `Access-Control-Allow-Origin` — server permission for a specific origin
- Single value per header — cannot list multiple origins
- `*` wildcard — all origins, incompatible with credentials
- Browser blocking — missing or mismatched header prevents JavaScript access
- ASP.NET Core `WithOrigins()` — echoes the matched origin

**Answer**

`Access-Control-Allow-Origin` is the response header that tells the browser which origin is permitted to read the response via JavaScript. The header accepts either a specific origin like `https://app.example.com` or the wildcard `*` for all origins, but cannot contain a list of multiple origins in a single header value — the server must dynamically echo the requesting origin from a configured allowlist rather than returning all permitted origins at once. ASP.NET Core's `WithOrigins("https://app.example.com")` handles this dynamic echo automatically when the requesting `Origin` matches an entry in the allowlist. If the header is missing, set to a different origin than the requesting one, or set to `*` when credentials are involved, the browser blocks JavaScript from reading the response even though the response arrived successfully.

---

## Q7. What is the difference between `AllowAnyOrigin` and `WithOrigins`?

**Concepts**
- `AllowAnyOrigin()` — sets `Access-Control-Allow-Origin: *`
- `WithOrigins()` — echoes specific allowed origins, required for credentials
- Wildcard — acceptable for public read-only APIs without authentication
- Explicit origins — required for authenticated cross-origin requests
- No wildcard subdomain support — each subdomain must be listed explicitly

**Answer**

`AllowAnyOrigin()` sets `Access-Control-Allow-Origin: *` which permits any origin to read the response, while `WithOrigins("https://app.example.com")` restricts access to explicitly listed origins and dynamically echoes the requesting origin in the response header when it matches. `AllowAnyOrigin()` is acceptable for public, completely read-only APIs in development where no authentication is involved — a public catalog API or health check endpoint. For any endpoint that handles authentication, cookies, or sensitive data, `WithOrigins()` with an explicit allowlist is required because `AllowAnyOrigin()` cannot be combined with `AllowCredentials()`. ASP.NET Core does not natively support wildcard subdomain patterns in `WithOrigins`, so I must list each subdomain explicitly — `"https://app.example.com"`, `"https://admin.example.com"` — or implement a custom `ICorsPolicyProvider` for subdomain pattern matching.

---

## Q8. Why can't `AllowAnyOrigin` be used with `AllowCredentials`?

**Concepts**
- CORS specification — forbids `* + credentials` combination
- Browser enforcement — rejects this combination
- Credentialed cross-origin request — cookies, HTTP auth, client certificates
- `WithOrigins().AllowCredentials()` — correct pattern
- Bearer tokens in `Authorization` header — credentials mode consideration

**Answer**

The CORS specification explicitly forbids combining `Access-Control-Allow-Origin: *` with `Access-Control-Allow-Credentials: true` because allowing credentials from any arbitrary origin would let any malicious website make authenticated requests on behalf of the user — the very attack that the Same-Origin Policy was designed to prevent. Browsers reject this combination and refuse to expose the response to JavaScript. ASP.NET Core throws at startup or emits an invalid CORS response when both are configured together. When a SPA uses cookie-based authentication or sends `credentials: 'include'` in fetch calls, I replace `AllowAnyOrigin()` with `WithOrigins("https://app.example.com")` and chain `AllowCredentials()`. For Bearer tokens passed in the `Authorization` header, `AllowCredentials()` may not be strictly needed unless the browser fetch is configured with `credentials: 'include'`, but explicit origins are still best practice for security.

---

## Q9. What does `AllowHeaders` configure?

**Concepts**
- `AllowHeaders` / `WithHeaders` / `AllowAnyHeader` — configures `Access-Control-Allow-Headers`
- `Authorization` — must be allowed for JWT Bearer requests
- `Content-Type` — must be allowed for JSON POST requests
- Preflight response — headers negotiated during OPTIONS
- Missing header permissions — preflight fails, actual request blocked

**Answer**

`AllowHeaders` (or `AllowAnyHeader()` for all headers) configures which request headers the browser is permitted to include on cross-origin requests. The server echoes the allowed headers in `Access-Control-Allow-Headers` on the preflight OPTIONS response. I must include `Authorization` for JWT Bearer token requests, `Content-Type` for JSON POST requests, and any custom headers like `X-Request-Id` or `X-Api-Version`. Missing a header in the allowlist causes the preflight to succeed but the actual request header to be rejected — the browser reports a CORS error before authentication middleware runs, which is why a 401 sometimes appears as a CORS error and developers spend time debugging the wrong layer. Using `AllowAnyHeader()` removes the header management overhead during development but I narrow it to explicitly needed headers in production policies.

---

## Q10. What does `WithExposedHeaders` do?

**Concepts**
- `WithExposedHeaders` — configures `Access-Control-Expose-Headers`
- CORS-safelisted response headers — only these readable by default
- `Content-Disposition` — file download filename, requires exposure
- `X-Total-Count` / `X-Pagination` — pagination metadata headers
- Alternative — return metadata in JSON body to avoid header exposure

**Answer**

By default, cross-origin JavaScript can only read a small set of CORS-safelisted response headers — `Content-Type`, `Cache-Control`, `Expires`, `Last-Modified`, and `Pragma`. Any other response header — `Content-Disposition`, `X-Total-Count`, `X-Pagination`, `X-RateLimit-Remaining` — is invisible to JavaScript unless the server explicitly permits access via `Access-Control-Expose-Headers`. I configure this with `.WithExposedHeaders("Content-Disposition", "X-Total-Count")` in the CORS policy. Without it, a file download endpoint that sets `Content-Disposition: attachment; filename="report.pdf"` works from Postman but the JavaScript client cannot read the filename — it sees an empty string from `response.headers.get("Content-Disposition")`. For simple pagination counts I sometimes return the total in the JSON body instead to avoid the exposure configuration entirely, since that approach is simpler and works without CORS changes.

---

## Q11. What is the correct middleware order for `UseCors` in a Web API?

**Concepts**
- Middleware order — `UseRouting` → `UseCors` → `UseAuthentication` → `UseAuthorization`
- CORS before auth — preflight OPTIONS must succeed without credentials
- CORS headers on error responses — 401/403 need CORS headers too
- `MapControllers()` — must come after `UseCors`
- Named policy — `app.UseCors("PolicyName")`

**Answer**

CORS middleware must run after routing and before authentication and authorization in the ASP.NET Core pipeline. The correct order is `UseRouting()` → `UseCors()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`. CORS must precede authentication because preflight OPTIONS requests must succeed without authentication — if auth middleware runs first, it may return 401 before CORS headers are added, and the browser reports a CORS error rather than the 401. CORS headers must also be added to error responses — when a valid cross-origin request reaches an endpoint and returns 401 or 403, the CORS headers must be present so JavaScript can read the error status and response body. If `UseCors` runs after auth, error responses from auth middleware lack CORS headers and the browser hides the real error from JavaScript, making debugging significantly harder.

---

## Q12. What is the difference between simple and non-simple CORS requests?

**Concepts**
- Simple request — skips preflight, proceeds directly
- Non-simple request — triggers OPTIONS preflight
- Simple criteria — GET/HEAD/POST with safelisted headers and Content-Types
- `Content-Type: application/json` — always non-simple
- Preflight latency — one extra round trip before the actual request

**Answer**

Simple requests skip the preflight and go directly to the server — the browser only checks the `Access-Control-Allow-Origin` header on the response. A request is simple when it uses GET, HEAD, or POST, uses only safelisted headers, and has a Content-Type of `text/plain`, `application/x-www-form-urlencoded`, or `multipart/form-data`. Non-simple requests trigger an OPTIONS preflight before the actual request — this adds one round trip of latency and requires the server to handle the OPTIONS method on all versioned and authenticated endpoints. Nearly all Web API requests are non-simple since JSON Content-Type and Authorization headers disqualify them. This is why every production CORS policy needs `Access-Control-Allow-Headers` to include `Authorization` and `Content-Type`, and the server's OPTIONS response must be fast, return 200, and not require authentication.

---

## Q13. Does CORS protect the API server from unauthorized access?

**Concepts**
- CORS — browser-only enforcement mechanism
- Non-browser clients — completely bypass CORS
- Authentication and authorization — actual server protection
- CORS as UX/browser feature — not a security boundary for the API

**Answer**

No. CORS is enforced by the browser only — it controls whether JavaScript on one origin can read responses from another origin. Curl, Postman, server-to-server HTTP clients, and any script running outside a browser are completely unaffected by CORS headers. A public API without authentication is accessible to anyone regardless of CORS configuration. I treat CORS as a browser UX and user protection feature: it prevents malicious websites from reading API responses via a user's browser credentials. The API itself is protected by authentication middleware — JWT Bearer tokens, cookies, or API keys — and authorization policies, which enforce access control for all callers regardless of whether they are a browser, a server, or a command-line tool.

---

## Q14. What is `Access-Control-Allow-Credentials`?

**Concepts**
- `Access-Control-Allow-Credentials: true` — permits browser to include credentials
- Credentials mode — cookies, HTTP auth, client certificates
- Client-side requirement — `credentials: 'include'` in fetch
- Requires specific origin — not compatible with wildcard
- Bearer tokens — may not require credentials mode depending on fetch config

**Answer**

`Access-Control-Allow-Credentials: true` tells the browser that it may include credentials — cookies, HTTP authentication headers, and client certificates — in cross-origin requests and expose the authenticated response to JavaScript. Without this header, cross-origin requests in credentials mode are rejected by the browser even if the server responds successfully. I configure it in ASP.NET Core with `.AllowCredentials()` on the CORS policy, which requires a specific origin rather than a wildcard. The client must also configure credentials mode explicitly — `fetch(url, { credentials: 'include' })` or `xhr.withCredentials = true` — since browsers default to omitting credentials on cross-origin requests. Cookie-based authentication in cross-origin SPAs requires both `AllowCredentials()` on the server and `credentials: 'include'` on the client. Bearer tokens passed in the `Authorization` header do not necessarily require credentials mode unless cookies are also involved.

---

## Q15. What is the difference between CORS errors and 401 Unauthorized?

**Concepts**
- CORS error — browser blocks JavaScript from reading response due to missing CORS headers
- 401 Unauthorized — authentication failure returned by server
- 401 without CORS headers — browser reports as CORS error, masking real cause
- Middleware order — `UseCors` before `UseAuthentication` fixes the masking
- DevTools — "blocked by CORS policy" vs actual 401 status in Network tab

**Answer**

A CORS error occurs when the browser blocks JavaScript from reading a cross-origin response because the server's CORS headers are missing or incorrect — the response may be 200 or 401, but JavaScript cannot see the status or body. A 401 Unauthorized is the server's authentication failure response that JavaScript can read normally when CORS headers are present and correct. The confusion arises when CORS headers are missing from a 401 response: the browser sees a cross-origin response without `Access-Control-Allow-Origin`, blocks JavaScript from reading it, and reports "blocked by CORS policy" in the DevTools console rather than the real 401 status. The fix is middleware order — `UseCors()` must run before `UseAuthentication()` so that CORS headers are added to 401 responses before they reach the browser. In DevTools Network tab, even a CORS-blocked response shows the real HTTP status — developers should check the actual status code before assuming CORS configuration is wrong.

---

## Q16. What does `Access-Control-Allow-Methods` specify?

**Concepts**
- `Access-Control-Allow-Methods` — permitted HTTP methods for cross-origin requests
- Preflight negotiation — browser sends `Access-Control-Request-Method`, server responds
- `AllowAnyMethod()` — permits all methods
- `WithMethods(...)` — restricts to listed methods
- Method restriction in production — avoid `AllowAnyMethod` when only specific verbs needed

**Answer**

`Access-Control-Allow-Methods` lists the HTTP methods the browser is permitted to use on cross-origin requests, appearing in the preflight OPTIONS response. The browser's preflight includes `Access-Control-Request-Method: POST` and the server must respond with `Access-Control-Allow-Methods` containing POST for the actual request to proceed. I configure this with `.WithMethods("GET", "POST", "PUT", "DELETE")` or `.AllowAnyMethod()` in the CORS policy. The method in the actual request must be listed — preflight can succeed for OPTIONS while the actual PUT request is blocked if PUT is not in `Access-Control-Allow-Methods`. In production I restrict to the methods the API actually uses rather than using `AllowAnyMethod()`, since the principle of least privilege applies to CORS policies just as it does to authorization policies.

---

## Q17. When should CORS be configured at the API vs API gateway?

**Concepts**
- API gateway CORS — central policy, consistent allowlist across microservices
- ASP.NET Core CORS — per-app, required for direct public exposure
- Duplicate CORS headers — both gateway and app produce headers, browser rejects
- Single CORS owner — documented in platform runbook
- Development — app-level CORS for local dev without gateway

**Answer**

I configure CORS at the API gateway or reverse proxy when the API sits behind a centralized entry point serving multiple microservices — the gateway owns the origin allowlist, handles preflight caching, and ensures consistent CORS policy across all backend services without each service needing its own configuration. I configure CORS in the ASP.NET Core application when the app is directly exposed to the internet without a gateway, for local development where no gateway exists, or for internal services that SPA clients access directly without going through the edge. The critical mistake to avoid is having both the gateway and the app emit CORS headers simultaneously — browsers reject responses with multiple `Access-Control-Allow-Origin` headers and report a CORS error even when both values are correct. I document CORS ownership clearly in the platform runbook so on-call engineers know which layer to update when an origin allowlist changes.

---

## Q18. What is a CORS policy in ASP.NET Core?

**Concepts**
- Named CORS policy — registered in `AddCors()`, applied by name
- Policy configuration — origins, methods, headers, exposed headers, credentials
- Global application — `app.UseCors("PolicyName")` in pipeline
- Per-controller/action — `[EnableCors("PolicyName")]` attribute
- Environment-specific origins — loaded from `appsettings.{Env}.json`

**Answer**

A CORS policy in ASP.NET Core is a named set of rules registered in `AddCors()` that defines the complete cross-origin access configuration — allowed origins, methods, request headers, exposed response headers, preflight max age, and credential support. I register it with `builder.Services.AddCors(options => options.AddPolicy("Default", builder => builder.WithOrigins(...).AllowAnyHeader().AllowAnyMethod()))` and apply it globally with `app.UseCors("Default")` in the middleware pipeline. Multiple policies can coexist — an `"AdminPolicy"` with stricter origins for the admin controller and a `"PublicPolicy"` with wider access for read-only catalog endpoints, applied per-controller with `[EnableCors("AdminPolicy")]`. I load the allowed origins from `appsettings.{Environment}.json` via `IOptions` so development, staging, and production environments each have their appropriate origin allowlists without changing code.

---

## Gotchas — CORS (Interview Traps)

---

#### Gotcha 1. `AllowAnyOrigin()` with `AllowCredentials()` browser rejection

**Concepts**
- CORS spec forbids `Access-Control-Allow-Origin: *` with `Access-Control-Allow-Credentials: true`
- Browsers reject the credentialed response — SPA call fails with CORS error
- `WithOrigins(...)` + `AllowCredentials()` — correct combination for credentialed requests
- ASP.NET Core throws or emits invalid CORS headers when both are combined

**Answer**

The CORS specification explicitly forbids combining a wildcard origin (`*`) with credential allowance — browsers reject any response that includes both `Access-Control-Allow-Origin: *` and `Access-Control-Allow-Credentials: true`. ASP.NET Core honors this by throwing an `InvalidOperationException` or silently omitting the credential header when both are configured. For SPA clients that send cookies or `Authorization` headers, I must list each allowed origin explicitly with `WithOrigins("https://app.example.com")` and chain `AllowCredentials()`, loading allowed origins from environment configuration to separate local development URLs from production domains.

---

#### Gotcha 2. Preflight OPTIONS request not reaching the API

**Concepts**
- Browser sends OPTIONS preflight before cross-origin requests with custom headers or non-simple methods
- `UseCors()` must handle OPTIONS before `UseAuthentication()` / `UseAuthorization()`
- OPTIONS request with no CORS middleware — returns 404, browser reports CORS error
- `app.UseCors()` placement — before endpoint routing to intercept OPTIONS

**Answer**

A browser's preflight OPTIONS request carries no authentication credentials. If authentication middleware runs before CORS middleware, the OPTIONS request receives a `401 Unauthorized` before CORS headers are ever added — the browser interprets the missing `Access-Control-Allow-Origin` as a CORS failure and blocks the actual request. `UseCors()` must be placed before `UseAuthentication()` and `UseAuthorization()` in the middleware pipeline so the CORS headers are added to the OPTIONS response before any auth check runs, allowing the preflight to succeed.

---

#### Gotcha 3. CORS headers stripped by reverse proxy between client and Kestrel

**Concepts**
- Reverse proxy (nginx, YARP, Azure APIM) may remove or overwrite CORS response headers
- Proxy adding its own `Access-Control-Allow-Origin: *` after Kestrel emits specific origin
- Browser receiving contradicting CORS headers from proxy
- CORS handled at proxy layer vs app layer — pick one, not both

**Answer**

A reverse proxy sitting between the browser and Kestrel may strip, overwrite, or add its own CORS headers, causing the browser to see a different `Access-Control-Allow-Origin` value than what ASP.NET Core emitted. Configuring CORS both in the app and in the proxy produces contradicting headers. I decide at architecture level which layer owns CORS and configure it in only one place — typically the API gateway or the reverse proxy for unified policy management across services, or the application for fine-grained per-endpoint policy control. I validate the actual CORS headers the browser receives using devtools, not just the Kestrel response headers.

---

#### Gotcha 4. Named CORS policy declared but not applied

**Concepts**
- `builder.Services.AddCors(options => options.AddPolicy("MyPolicy", ...)` — declares the policy
- `app.UseCors("MyPolicy")` — applies it globally in the middleware pipeline
- `[EnableCors("MyPolicy")]` — applies it per controller or action
- `UseCors()` without a policy name — applies the default policy, not the named one

**Answer**

Calling `AddCors` and defining a named policy does nothing until the policy is applied either globally via `app.UseCors("MyPolicy")` or per-endpoint via `[EnableCors("MyPolicy")]`. Calling `app.UseCors()` without a policy name applies the default policy (registered with `AddDefaultPolicy`), not the named policy, silently giving all endpoints the wrong CORS configuration. I always explicitly name both the policy registration and the `UseCors` call, and verify with integration tests that the `Access-Control-Allow-Origin` header matches the expected allowed origin.

---

#### Gotcha 5. Missing `Access-Control-Expose-Headers` for custom response headers

**Concepts**
- Browser restricts access to response headers from cross-origin requests by default
- `Access-Control-Expose-Headers` — allowlist of additional headers the browser may read
- Custom headers (`X-Total-Count`, `X-Request-Id`, `ETag`) not accessible without explicit exposure
- `WithExposedHeaders("X-Total-Count", "ETag")` in CORS policy

**Answer**

Browsers only allow JavaScript to read a small set of "safe" response headers from cross-origin requests — all other headers are filtered out even when CORS is correctly configured. Custom headers like `X-Total-Count` for pagination, `X-Request-Id` for tracing, or `ETag` for caching are invisible to the SPA client unless explicitly listed in `Access-Control-Expose-Headers`. I add `.WithExposedHeaders("X-Total-Count", "X-Request-Id", "ETag")` to the CORS policy for endpoints that return custom headers, verified by a JavaScript `response.headers.get("X-Total-Count")` call returning a non-null value in integration tests.

---

#### Gotcha 6. CORS as a security mechanism — not stopping non-browser clients

**Concepts**
- CORS — browser-only Same-Origin Policy enforcement mechanism
- curl, Postman, server-to-server HTTP clients — completely unaffected by CORS
- Malicious script in attacker-controlled browser — prevented by CORS if origin not listed
- Authentication and authorization — real security for all client types

**Answer**

CORS does not protect the API from unauthorized access — it controls whether JavaScript in a browser tab from another origin may read the response. curl, Postman, mobile apps, and server-to-server calls bypass CORS entirely because they are not subject to the Same-Origin Policy. A widely-open CORS policy on a public API is a UX concern for browser clients but not a security backdoor — the real security is authentication and authorization, which I enforce regardless of CORS configuration. Treating CORS as security leads to a false sense of protection where the real attack surface remains fully exposed.

---

#### Gotcha 7. Allowing any header with `AllowAnyHeader()` alongside credentials

**Concepts**
- `AllowAnyHeader()` — emits `Access-Control-Allow-Headers: *` on preflight
- `Access-Control-Allow-Headers: *` with credentials — browser may reject the combination
- Browser behavior varies: some reject wildcard + credentials for headers as well as origins
- Explicit header listing: `.WithHeaders("Content-Type", "Authorization")` for credentialed requests

**Answer**

The same spec restriction that forbids `AllowAnyOrigin` + `AllowCredentials` also applies to `AllowAnyHeader` + `AllowCredentials` in some browser implementations — a preflight with `Access-Control-Allow-Headers: *` and `Access-Control-Allow-Credentials: true` may be rejected. When a CORS policy uses `AllowCredentials()`, I replace `AllowAnyHeader()` with an explicit `WithHeaders("Content-Type", "Authorization", "X-Requested-With")` listing the headers the SPA actually needs, which keeps the preflight safe across all browsers and avoids accidentally permitting unexpected headers.

---

#### Gotcha 8. Environment-specific origin lists hardcoded

**Concepts**
- Dev origin `http://localhost:4200` hardcoded in production policy
- Production origin `https://app.example.com` missing from dev policy
- Configuration-driven allowed origins — `appsettings.{Environment}.json`
- Localhost wildcard — not a valid CORS origin value

**Answer**

Hardcoding `http://localhost:4200` as an allowed origin in the production CORS policy gives any developer's local machine cross-origin access to the production API from any browser tab. Conversely, missing the production SPA domain in the production policy causes a CORS failure that only manifests in production. I load allowed origins from `appsettings.{Environment}.json` under a `Cors:AllowedOrigins` array, which is empty by default and populated with the correct domain per environment. Localhost is excluded from production configuration by policy, verified in code review.

---

#### Gotcha 9. CORS middleware order — placed after authentication

**Concepts**
- `UseCors()` must run before the request reaches authentication middleware
- Middleware order: `UseRouting` then `UseCors` then `UseAuthentication` then `UseAuthorization`
- In ASP.NET Core 7+ minimal hosting model — `app.UseCors()` before `app.UseAuthentication()`
- Placing `UseCors` after `UseAuthorization` — OPTIONS preflight blocked by auth

**Answer**

`UseCors()` must be placed before `UseAuthentication()` and `UseAuthorization()` in the middleware pipeline. A common mistake in the minimal hosting model is adding `UseCors()` after `UseAuthorization()` because it "works for normal requests" — authenticated requests with matching origins do work, but OPTIONS preflights (which carry no credentials) fail at the auth middleware before CORS runs. The browser then reports a CORS error even though the actual policy is correct. The canonical order is: `UseRouting()` then `UseCors()` then `UseAuthentication()` then `UseAuthorization()` then `MapControllers()`.

---

#### Gotcha 10. gRPC-Web requiring separate CORS configuration alongside gRPC

**Concepts**
- gRPC uses HTTP/2 binary framing — browsers need gRPC-Web middleware
- gRPC-Web preflight headers differ from standard REST CORS headers
- `AddGrpcWeb()` + `EnableGrpcWeb()` required on each mapped gRPC service
- CORS policy must allow gRPC-Web-specific headers: `grpc-status`, `grpc-message`, `content-type`

**Answer**

Browser-based gRPC clients using gRPC-Web generate preflight requests with gRPC-specific headers (`grpc-status`, `grpc-message`, `grpc-encoding`) that the CORS policy must explicitly allow. A REST-oriented CORS policy allowing only `Content-Type` and `Authorization` blocks gRPC-Web preflights even when the origin is allowed. I add the gRPC headers to `WithHeaders(...)` in the CORS policy, call `app.UseGrpcWeb()` after `UseCors()` but before `MapGrpcService()`, and call `.EnableGrpcWeb()` on each mapped service. The gRPC-Web CORS requirements are distinct from REST and must be configured alongside, not instead of, any existing REST CORS policy.

---

## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- `AllowAnyOrigin()` + `AllowCredentials()` — CORS specification violation
- ASP.NET Core throws at startup or emits invalid response
- `WithOrigins("https://app.example.com")` — required for credentialed requests
- `credentials: 'include'` — browser sends cookies, requires specific origin on server

**Answer**

The CORS specification forbids combining `Access-Control-Allow-Origin: *` with `Access-Control-Allow-Credentials: true`. ASP.NET Core detects this combination and either throws at startup or emits an invalid CORS response — the browser then rejects it, which is why the console shows a CORS error despite the policy appearing permissive. The policy is internally contradictory: `AllowAnyOrigin()` wants a wildcard, but `AllowCredentials()` requires a specific origin in the response.

The fix is to replace `AllowAnyOrigin()` with `WithOrigins("https://app.example.com")`:

```csharp
options.AddPolicy("SpaPolicy", policy =>
    policy.WithOrigins("https://app.example.com")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials());
```

If the app also runs in local development I add the local dev URL: `.WithOrigins("https://app.example.com", "http://localhost:3000")`. Origins should be loaded from `appsettings.{Environment}.json` in production so the development and production origins are configured per environment without code changes.

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

**Concepts**
- Missing `AllowHeaders` — no `Access-Control-Allow-Headers` in preflight response
- `Authorization`, `Content-Type`, `X-Request-Id` — all require explicit allowance
- `AllowAnyHeader()` — simplest fix, or list specific headers
- Postman bypasses CORS — does not send preflight
- Preflight failure masks the actual controller error

**Answer**

The policy is missing `AllowHeaders` or `AllowAnyHeader()`, so the preflight OPTIONS response does not include `Access-Control-Allow-Headers`. When the browser's preflight sends `Access-Control-Request-Headers: content-type, authorization, x-request-id`, the server's response does not list any of them, causing the preflight to fail. The browser blocks the actual POST before it reaches the controller, which is why Postman works — Postman does not send a preflight since it is not a browser.

The fix is to add `.AllowAnyHeader()` or be explicit:

```csharp
policy.WithOrigins("https://portal.example.com")
      .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization, "X-Request-Id")
      .AllowAnyMethod();
```

I also note that `AllowAnyMethod()` without `AllowAnyHeader()` is an unusual combination — typically both are either permissive or explicitly restricted together. In production I use specific headers rather than `AllowAnyHeader()` to apply least privilege, listing only the headers the SPA actually sends.

---

#### Q3. (M) A JWT-protected API returns 401 on cross-origin requests; developers say "CORS is broken" because the response lacks `Access-Control-Allow-Origin`. Walk through middleware order — `UseCors`, `UseAuthentication`, `UseAuthorization`, endpoint — and explain when CORS headers appear on error responses vs when auth fails first.

---

**Concepts**
- Middleware order — determines which headers appear on error responses
- `UseCors` before `UseAuthentication` — CORS headers added before auth can return 401
- `UseCors` after `UseAuthentication` — 401 from auth middleware lacks CORS headers
- Browser CORS error vs 401 — browser hides the 401 if CORS headers missing
- Network tab — actual HTTP status visible even when CORS-blocked

**Answer**

The CORS middleware adds `Access-Control-Allow-Origin` and related headers to every response that passes through it — including error responses like 401 and 403. The critical constraint is that CORS middleware must run before authentication middleware so that when auth returns 401, the CORS headers have already been added to the response before it reaches the browser.

If the pipeline is ordered `UseAuthentication()` → `UseCors()`, authentication runs first and returns a 401 response. The CORS middleware then has no opportunity to add `Access-Control-Allow-Origin` because the response is already committed or the middleware chain short-circuited before reaching `UseCors`. The browser receives a 401 without CORS headers, applies its CORS check, finds no valid `Access-Control-Allow-Origin`, and blocks JavaScript from reading the response — reporting "blocked by CORS policy" rather than the real 401.

The correct order is `UseRouting()` → `UseCors()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`. With this order, CORS adds its headers first, then auth runs and may return 401, but that 401 already has the CORS headers so the browser can read the status code and response body. JavaScript on the SPA can then see the 401 and redirect the user to login rather than showing a confusing CORS error. Developers can confirm this by checking the Network tab in DevTools — even a CORS-blocked response shows the real HTTP status — so a 401 in the Network tab alongside "CORS error" in the console is the telltale sign of wrong middleware order.

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

**Concepts**
- `WithOrigins` — no native wildcard subdomain support
- `ICorsPolicyProvider` — custom implementation for wildcard subdomain matching
- Environment-specific origins — `appsettings.{Environment}.json` via `IOptions`
- `AllowAnyOrigin()` risk — acceptable only for public APIs without authentication
- Origin allowlist from configuration — avoids hardcoded origins in code

**Answer**

The configuration approach is correct for environment-specific origins — loading from `appsettings.{Environment}.json` via `IOptions<CorsOptions>` or a dedicated `CorsSettings` class means development, staging, and production each have their appropriate allowlists without code changes between deployments. `AllowAnyOrigin()` is appropriate only for fully public, unauthenticated read-only APIs; for authenticated endpoints it must be replaced with an explicit allowlist.

The wildcard `"https://*.staging.example.com"` is the problem. ASP.NET Core's `WithOrigins` does not support wildcard subdomain patterns and will treat the literal string as the only allowed origin, so `https://tenant1.staging.example.com` will not match. There are three solutions. The simplest is to list each staging subdomain explicitly — `"https://tenant1.staging.example.com"`, `"https://tenant2.staging.example.com"` — which is verbose but requires no custom code. For dynamic tenant subdomains I implement a custom `ICorsPolicyProvider` that uses `string.Contains(".staging.example.com")` or a regex to match the origin against the subdomain pattern at request time. The third option is to use a single staging domain with path-based tenant routing, eliminating the subdomain requirement entirely.

I also ensure that the `CorsSettings` section is not treated as a secret — origin names are not sensitive — so they can live in committed `appsettings.{Environment}.json` files without concern.

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

**Concepts**
- CORS-safelisted response headers — only a few readable by JavaScript by default
- `Content-Disposition` — not safelisted, requires `WithExposedHeaders`
- `X-Total-Count` — custom header, requires explicit exposure
- `Access-Control-Expose-Headers` — set by `WithExposedHeaders`
- Alternative — return metadata in JSON body to avoid header exposure

**Answer**

The issue is that `Content-Disposition` and `X-Total-Count` are not in the CORS-safelisted response headers, so browsers block JavaScript from reading them even though the 200 response is received successfully. The policy allows the request and adds `Access-Control-Allow-Origin`, but without `Access-Control-Expose-Headers` listing these headers, the JavaScript `response.headers.get("Content-Disposition")` returns null and `response.headers.get("X-Total-Count")` returns null.

The fix is to add `WithExposedHeaders`:

```csharp
policy.WithOrigins("https://app.example.com")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .WithExposedHeaders("Content-Disposition", "X-Total-Count");
```

For the file download specifically, this allows the JavaScript client to read the filename from `Content-Disposition` and set it on the download. For pagination totals, I would also consider returning `X-Total-Count` as a field in a JSON response wrapper instead — `{ "total": 1500, "items": [...] }` — since it avoids the CORS exposure configuration and works in all HTTP clients without special handling.

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

**Concepts**
- `UseCors` after `UseAuthentication` — 401 responses lack CORS headers
- Preflight OPTIONS — hits authentication middleware before CORS
- Middleware short-circuit — auth returns 401 before CORS headers added
- Anonymous endpoints — bypass auth so CORS runs for them, but auth endpoints fail

**Answer**

CORS is placed after authentication and authorization in the pipeline. For anonymous endpoints, auth middleware passes through without returning an error, so the request reaches `UseCors` which adds the CORS headers — this is why anonymous routes work. For authenticated endpoints, the OPTIONS preflight arrives without a Bearer token, authentication middleware returns 401 before `UseCors` runs, and the 401 response lacks `Access-Control-Allow-Origin`. The browser sees a CORS policy failure on the preflight and blocks the actual authenticated request.

The fix is to move `UseCors` before auth:

```csharp
app.UseRouting();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

With this order, every request — including OPTIONS preflights and 401 responses — passes through `UseCors` first and receives the CORS headers before auth can short-circuit. The OPTIONS preflight gets a 200 with CORS headers, the browser proceeds with the actual authenticated request, and any 401 from authentication still carries CORS headers so JavaScript can read and handle the error.

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

**Concepts**
- Middleware after `MapControllers` — does not run for matched endpoint requests
- Middleware order — endpoint middleware short-circuits before reaching `UseCors`
- IIS reverse proxy — different domain triggers CORS check
- Localhost direct access — bypasses IIS proxy, Kestrel serves directly

**Answer**

`UseCors` is registered after `MapControllers()`, which means endpoint routing has already matched and handled the request before CORS middleware runs. Middleware placed after `MapControllers()` does not execute for requests matched by the endpoint routing, so `UseCors` is effectively dead code for all mapped controller routes. Local development works when hitting Kestrel directly because some local requests may bypass the endpoint routing short-circuit, and without a cross-origin boundary there is no CORS check to fail. Behind the IIS reverse proxy with different domains, the SPA at `https://spa.internal.corp` calling `https://api.internal.corp` triggers a cross-origin browser check, and since CORS headers are never added the browser blocks the response.

The fix is to move `UseCors` before `MapControllers()`:

```csharp
app.UseRouting();
app.UseCors("DevPolicy");
app.MapControllers();
```

In production the policy should also be more restrictive — `AllowAnyOrigin` in a non-development environment and an internal corporate network is not a security risk in the same way as public internet exposure, but I would use `WithOrigins("https://spa.internal.corp")` to be explicit and prevent the policy from accidentally applying to unintended origins if the application is ever moved to a different environment.

---

#### Q8. (D) API team owns ASP.NET Core CORS; platform team adds Azure API Management in front with its own CORS policy. Browser sees duplicate or conflicting `Access-Control-Allow-Origin` headers. Who should own CORS in production, and how do you avoid double-application?

---

**Concepts**
- Duplicate `Access-Control-Allow-Origin` — browser rejects multiple values
- APIM CORS policy — gateway-level CORS handling
- ASP.NET Core CORS — app-level CORS handling
- Single CORS owner — prevent dual-header emission
- `cors-passthrough` or disabled CORS at one layer

**Answer**

Browsers reject a response that contains multiple `Access-Control-Allow-Origin` header values, treating the ambiguity as a CORS policy failure even when both values would individually be valid. The duplicate headers occur because APIM adds its own `Access-Control-Allow-Origin` in the gateway's CORS policy and ASP.NET Core adds another in the app middleware, resulting in a response with two separate `Access-Control-Allow-Origin` headers.

The correct approach is to designate one layer as the CORS owner. For a production multi-tenant API behind APIM, I prefer making APIM the CORS owner because it provides a centralized allowlist across all backend microservices, eliminates per-service CORS configuration drift, and can handle preflight caching at the gateway layer to reduce latency. When APIM owns CORS, I disable CORS in the ASP.NET Core app entirely — remove `AddCors()`, `UseCors()`, and all CORS policy registrations — so only one layer emits headers.

When ASP.NET Core owns CORS — for example when the API is directly exposed without a gateway in some environments — I disable the APIM CORS policy via `<cors allow-credentials="true">` set to passthrough or by simply not configuring a CORS policy in APIM. I document the CORS ownership in the platform runbook with a clear rule: exactly one layer handles CORS, the other is disabled, and all origin allowlist changes go through the owning layer's configuration.
