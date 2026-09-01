# Web Application Security — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is the OWASP Top 10, and which categories are most relevant to ASP.NET Core applications?](#q1-what-is-the-owasp-top-10-and-which-categories-are-most-relevant-to-aspnet-core-applications)
2. [Q2. What is Cross-Site Scripting (XSS), and what are the three main types?](#q2-what-is-cross-site-scripting-xss-and-what-are-the-three-main-types)
3. [Q3. How does ASP.NET Core protect against XSS by default, and when does that protection not apply?](#q3-how-does-aspnet-core-protect-against-xss-by-default-and-when-does-that-protection-not-apply)
4. [Q4. What is Cross-Site Request Forgery (CSRF), and how does an attacker exploit it?](#q4-what-is-cross-site-request-forgery-csrf-and-how-does-an-attacker-exploit-it)
5. [Q5. How do anti-forgery tokens work in ASP.NET Core, and what are the two primary APIs for configuring them?](#q5-how-do-anti-forgery-tokens-work-in-aspnet-core-and-what-are-the-two-primary-apis-for-configuring-them)
6. [Q6. What is SQL injection, and how does EF Core protect against it by default?](#q6-what-is-sql-injection-and-how-does-ef-core-protect-against-it-by-default)
7. [Q7. When using EF Core, can raw SQL queries still be vulnerable to SQL injection? How do you write them safely?](#q7-when-using-ef-core-can-raw-sql-queries-still-be-vulnerable-to-sql-injection-how-do-you-write-them-safely)
8. [Q8. What is an Insecure Direct Object Reference (IDOR), and how does proper authorization prevent it?](#q8-what-is-an-insecure-direct-object-reference-idor-and-how-does-proper-authorization-prevent-it)
9. [Q9. What is Content Security Policy (CSP), and how do you add it to an ASP.NET Core response?](#q9-what-is-content-security-policy-csp-and-how-do-you-add-it-to-an-aspnet-core-response)
10. [Q10. What HTTP security headers should every ASP.NET Core application include, and what does each one do?](#q10-what-http-security-headers-should-every-aspnet-core-application-include-and-what-does-each-one-do)
11. [Q11. What is HTTP Strict Transport Security (HSTS), and how do you enable it in ASP.NET Core?](#q11-what-is-http-strict-transport-security-hsts-and-how-do-you-enable-it-in-aspnet-core)
12. [Q12. What is CORS (Cross-Origin Resource Sharing), and why does the browser enforce it?](#q12-what-is-cors-cross-origin-resource-sharing-and-why-does-the-browser-enforce-it)
13. [Q13. How do you configure a CORS policy in ASP.NET Core, and what is the difference between a named policy and the default policy?](#q13-how-do-you-configure-a-cors-policy-in-aspnet-core-and-what-is-the-difference-between-a-named-policy-and-the-default-policy)
14. [Q14. What is a CORS preflight request, and when does the browser send one?](#q14-what-is-a-cors-preflight-request-and-when-does-the-browser-send-one)
15. [Q15. Why is CORS not a server-side security boundary?](#q15-why-is-cors-not-a-server-side-security-boundary)
16. [Q16. What is an open redirect vulnerability, and how do ASP.NET Core's `LocalRedirect` and `IsLocalUrl` helpers prevent it?](#q16-what-is-an-open-redirect-vulnerability-and-how-do-aspnet-cores-localredirect-and-islocalurl-helpers-prevent-it)
17. [Q17. What are the `Secure`, `HttpOnly`, and `SameSite` cookie attributes, and what threat does each mitigate?](#q17-what-are-the-secure-httponly-and-samesite-cookie-attributes-and-what-threat-does-each-mitigate)
18. [Q18. What are the valid values of the `SameSite` cookie attribute, and how does each affect cross-site cookie sending?](#q18-what-are-the-valid-values-of-the-samesite-cookie-attribute-and-how-does-each-affect-cross-site-cookie-sending)
19. [Q19. What is the `SameSite=None` gotcha, and when does it apply?](#q19-what-is-the-samesitenone-gotcha-and-when-does-it-apply)
20. [Q20. How does ASP.NET Core's cookie authentication middleware configure cookies securely by default?](#q20-how-does-aspnet-cores-cookie-authentication-middleware-configure-cookies-securely-by-default)
21. [Q21. What is rate limiting, and how do you add it to an ASP.NET Core application (.NET 7+)?](#q21-what-is-rate-limiting-and-how-do-you-add-it-to-an-aspnet-core-application-net-7)
22. [Q22. What rate-limiting algorithms are built into `Microsoft.AspNetCore.RateLimiting`, and when would you choose each?](#q22-what-rate-limiting-algorithms-are-built-into-microsoftaspnetcoreratelimiting-and-when-would-you-choose-each)
23. [Q23. What is input validation, and how does ASP.NET Core model binding contribute to security?](#q23-what-is-input-validation-and-how-does-aspnet-core-model-binding-contribute-to-security)
24. [Q24. What is the difference between client-side and server-side input validation, and which one is a security control?](#q24-what-is-the-difference-between-client-side-and-server-side-input-validation-and-which-one-is-a-security-control)
25. [Q25. What is a mass assignment vulnerability, and how do `[Bind]` and dedicated DTOs (Data Transfer Objects) prevent it?](#q25-what-is-a-mass-assignment-vulnerability-and-how-do-bind-and-dedicated-dtos-data-transfer-objects-prevent-it)
26. [Q26. What are stored XSS and reflected XSS, and which is more dangerous and why?](#q26-what-are-stored-xss-and-reflected-xss-and-which-is-more-dangerous-and-why)
27. [Q27. How does DOM-based XSS differ from server-side XSS, and what mitigation applies?](#q27-how-does-dom-based-xss-differ-from-server-side-xss-and-what-mitigation-applies)
28. [Q28. What is the difference between authentication and authorization, and why does confusing them lead to IDOR vulnerabilities?](#q28-what-is-the-difference-between-authentication-and-authorization-and-why-does-confusing-them-lead-to-idor-vulnerabilities)

---

## Q1. What is the OWASP Top 10, and which categories are most relevant to ASP.NET Core applications?

What is the OWASP Top 10, and which categories are most relevant to ASP.NET Core applications?

**Answer:** The OWASP (Open Web Application Security Project) Top 10 is a regularly updated list of the most critical web application security risks, based on real-world vulnerability data. It is widely used as a baseline security standard in the industry. The categories most relevant to ASP.NET Core developers are Injection (A03), Broken Access Control (A01), Security Misconfiguration (A05), and Identification and Authentication Failures (A07).

- Broken Access Control (A01) covers IDOR (Insecure Direct Object Reference) and missing authorization checks, which are easy to introduce when `[Authorize]` attributes or resource-level checks are omitted.
- Injection (A03) includes SQL injection, which EF Core largely prevents through parameterized queries, but raw SQL escapes that protection.
- Security Misconfiguration (A05) covers missing HTTPS enforcement, absent security headers, permissive CORS policies, and exposed stack traces in production.
- Identification and Authentication Failures (A07) map directly to weaknesses in ASP.NET Core Identity configuration such as short token lifetimes, weak password policies, or missing multi-factor authentication.
- Cryptographic Failures (A02) are relevant when developers store passwords using reversible encryption or weak hashing instead of ASP.NET Core Identity's bcrypt-based password hasher.

---

## Q2. What is Cross-Site Scripting (XSS), and what are the three main types?

What is Cross-Site Scripting (XSS), and what are the three main types?

**Answer:** Cross-Site Scripting (XSS) is an injection attack where an attacker embeds malicious scripts into content that is later rendered by another user's browser, causing those scripts to execute in the victim's security context. The three main types are reflected XSS, stored XSS, and DOM-based XSS. All three exploit a failure to separate untrusted data from executable code.

- Reflected XSS occurs when user-supplied input (typically from a query string) is immediately echoed back in the server's HTML response without encoding, and the victim is tricked into clicking a crafted URL.
- Stored XSS occurs when malicious input is persisted to a database and later rendered to other users; it is more dangerous because no crafted link is required to trigger it.
- DOM-based XSS occurs entirely in the browser: client-side JavaScript reads from an attacker-controlled source (such as `location.hash`) and writes to an executable sink (such as `innerHTML`) without the data ever reaching the server.
- All three allow attackers to steal session cookies, perform actions on behalf of the victim, or redirect the user to a phishing page.

---

## Q3. How does ASP.NET Core protect against XSS by default, and when does that protection not apply?

How does ASP.NET Core protect against XSS by default, and when does that protection not apply?

**Answer:** ASP.NET Core's Razor view engine HTML-encodes all values rendered with the `@` syntax by default, converting characters like `<`, `>`, and `"` into safe HTML entities before they reach the browser. This means that even if an attacker injects `<script>alert(1)</script>` into a database field, Razor renders it as harmless text rather than executable code. The protection does not apply in several important scenarios.

- When developers explicitly bypass encoding with `@Html.Raw(value)`, the raw string is written directly into the page, re-introducing the XSS risk.
- The protection applies only to server-rendered HTML; it does not cover JavaScript contexts where a value is dynamically written into a `<script>` block or used as an event handler.
- DOM-based XSS is entirely client-side, so server-side encoding cannot prevent it — client-side sanitization libraries or a strict Content Security Policy are required instead.
- Rendering user input into HTML attributes that accept URLs (such as `href` or `src`) requires URL encoding in addition to HTML encoding, and Razor's default `@` encoding is sometimes insufficient for those contexts.
- When returning JSON from API endpoints, the serializer does not apply HTML encoding; APIs that embed JSON directly in script tags need explicit encoding.

---

## Q4. What is Cross-Site Request Forgery (CSRF), and how does an attacker exploit it?

What is Cross-Site Request Forgery (CSRF), and how does an attacker exploit it?

**Answer:** Cross-Site Request Forgery (CSRF, also written XSRF) is an attack where a malicious website tricks an authenticated user's browser into sending a state-changing request to a target site, because the browser automatically attaches the user's session cookies to every matching request. The attack works because the server cannot distinguish a request the user intentionally submitted from one the attacker forged. CSRF is only relevant for browser-based sessions backed by cookies; token-based APIs using Authorization headers are not vulnerable.

- A classic scenario: the victim is logged into their bank while also visiting a malicious page that contains a hidden form posting to `bank.example/transfer?to=attacker&amount=1000`. The browser sends the bank's session cookie automatically, and the transfer succeeds.
- CSRF does not let the attacker read the response; it only lets the attacker trigger actions the authenticated user is permitted to perform.
- The same-origin policy prevents the attacker from reading the anti-forgery token from the legitimate site, which is why that token is an effective defense.
- Modern `SameSite=Lax` cookie defaults in browsers reduce CSRF risk significantly, but they are a browser-level mitigation and not a substitute for server-side validation.

---

## Q5. How do anti-forgery tokens work in ASP.NET Core, and what are the two primary APIs for configuring them?

How do anti-forgery tokens work in ASP.NET Core, and what are the two primary APIs for configuring them?

**Answer:** Anti-forgery tokens work by embedding a cryptographically random secret into the form (as a hidden field) and also storing it in a cookie; when the form is submitted, the server validates that both tokens match. An attacker's page cannot read the token from the legitimate site due to the same-origin policy, so it cannot forge a valid request. The two primary ASP.NET Core APIs are `[ValidateAntiForgeryToken]` applied to controller actions and the `IAntiforgery` service for manual control.

- In Razor-based MVC apps, `@Html.AntiForgeryToken()` or the `<form>` tag helper emits the hidden input field, and `[ValidateAntiForgeryToken]` on the action validates both the field and cookie on POST.
- The `[AutoValidateAntiforgeryToken]` filter can be applied globally to validate all unsafe-method (POST, PUT, DELETE, PATCH) requests site-wide without decorating every action.
- The `IAntiforgery` service is used for scenarios outside Razor forms — for example, generating a token to embed in a JavaScript variable that a single-page application sends as a request header.
- ASP.NET Core validates the `X-XSRF-TOKEN` or `RequestVerificationToken` header when a token is sent that way, allowing SPAs (Single-Page Applications) to participate in CSRF protection without hidden form fields.

```csharp
// Global filter in Program.cs
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
```

---

## Q6. What is SQL injection, and how does EF Core protect against it by default?

What is SQL injection, and how does EF Core protect against it by default?

**Answer:** SQL (Structured Query Language) injection is an attack where user-supplied input is concatenated directly into a SQL query string, allowing the attacker to alter the query's logic, extract unauthorized data, or destroy data. EF (Entity Framework) Core protects against SQL injection by default because all LINQ queries are translated into parameterized SQL statements, where user values are passed as typed parameters rather than embedded in the query text.

- In a parameterized query, the database engine treats the user's input as a literal value, not as SQL syntax, so even a string like `'; DROP TABLE Users; --` is stored or compared as plain text.
- EF Core's change tracking, navigation properties, and LINQ expression tree compilation all produce parameterized SQL without requiring developers to think about escaping.
- This protection is automatic for all standard LINQ operations such as `Where`, `FirstOrDefault`, `OrderBy`, and `Include`.
- The database driver (e.g., Microsoft.Data.SqlClient) enforces the parameter boundary at the protocol level, making it impossible for user data to break out into the SQL syntax even if it contains quotes or SQL keywords.

---

## Q7. When using EF Core, can raw SQL queries still be vulnerable to SQL injection? How do you write them safely?

When using EF Core, can raw SQL queries still be vulnerable to SQL injection? How do you write them safely?

**Answer:** Yes, EF Core's protection does not apply when developers use raw SQL APIs and concatenate user input directly into the query string. The unsafe APIs are `FromSqlRaw` and `ExecuteSqlRaw` when called with string interpolation or concatenation. The safe alternatives are `FromSqlInterpolated` and `ExecuteSqlInterpolated`, which convert C# interpolated string holes into proper SQL parameters automatically.

- Calling `context.Users.FromSqlRaw($"SELECT * FROM Users WHERE Name = '{name}'")` is vulnerable because the value of `name` is embedded as SQL text; an attacker can supply `' OR '1'='1` to return all rows.
- Calling `context.Users.FromSqlInterpolated($"SELECT * FROM Users WHERE Name = {name}")` is safe because EF Core recognizes the interpolation and converts `name` into a `DbParameter`.
- When writing stored procedure calls or complex raw queries, `DbParameter` objects (e.g., `new SqlParameter("@name", name)`) can be passed to `FromSqlRaw` explicitly, which is also safe.
- Code reviews should flag any raw SQL method that accepts a plain `string` variable built outside the method call, as this is the typical injection pattern.

```csharp
// Unsafe
var users = ctx.Users.FromSqlRaw("SELECT * FROM Users WHERE Name = '" + name + "'");

// Safe
var users = ctx.Users.FromSqlInterpolated($"SELECT * FROM Users WHERE Name = {name}");
```

---

## Q8. What is an Insecure Direct Object Reference (IDOR), and how does proper authorization prevent it?

What is an Insecure Direct Object Reference (IDOR), and how does proper authorization prevent it?

**Answer:** An Insecure Direct Object Reference (IDOR) is a vulnerability where an application uses a user-controlled identifier (such as a database row ID in a URL) to access a resource without verifying that the requesting user is authorized to access that specific resource. For example, a user who is authorized to view `/invoices/101` can simply change the URL to `/invoices/102` and access another user's invoice if the server performs no ownership check. Preventing IDOR requires combining authentication (knowing who the user is) with resource-level authorization (verifying the user owns or is permitted to access the specific record).

- The fix is to always check the resource's owner or role permissions after fetching it, not just before entering the controller action.
- ASP.NET Core's `IAuthorizationService` with resource-based authorization policies is the idiomatic way to perform per-resource checks.
- Using GUIDs (Globally Unique Identifiers) instead of sequential integers as public identifiers makes IDs harder to guess but does not eliminate the vulnerability; the authorization check must still exist.
- IDOR applies to any type of identifier: numeric IDs, file names in download URLs, and even account numbers passed in request bodies.

---

## Q9. What is Content Security Policy (CSP), and how do you add it to an ASP.NET Core response?

What is Content Security Policy (CSP), and how do you add it to an ASP.NET Core response?

**Answer:** Content Security Policy (CSP) is an HTTP response header that instructs the browser to only load resources — scripts, stylesheets, images, fonts, and frames — from origins that the server explicitly permits. It is a defense-in-depth measure that limits the damage of XSS attacks by preventing injected scripts from executing or loading external payloads. ASP.NET Core does not include a built-in CSP helper, so the header is added via middleware or the `UseHeaders` approach.

- A strict CSP can block inline scripts entirely with `script-src 'self'`, meaning even a successfully injected `<script>` tag will be refused by the browser because it is not from an allowed origin.
- The `Content-Security-Policy-Report-Only` header allows a site to test a CSP policy without enforcing it, logging violations to a report URI instead of blocking resources.
- Because CSP operates in the browser, it does not prevent server-side attacks; it is a client-side mitigation layered on top of server-side output encoding.
- A minimal CSP header can be added in ASP.NET Core middleware: `context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'")`.
- CSP must be balanced carefully in practice: libraries loaded from CDNs (Content Delivery Networks), inline styles, and `eval()` usage all require explicit exceptions that weaken the policy.

---

## Q10. What HTTP security headers should every ASP.NET Core application include, and what does each one do?

What HTTP security headers should every ASP.NET Core application include, and what does each one do?

**Answer:** Every ASP.NET Core application should include a standard set of HTTP response headers that instruct browsers to behave safely with the application's content. These headers are not enabled by default in ASP.NET Core and must be configured explicitly, typically in middleware.

| Header | Purpose |
|---|---|
| `Strict-Transport-Security` | Forces browsers to use HTTPS for a specified duration; prevents protocol-downgrade attacks. |
| `X-Content-Type-Options: nosniff` | Prevents browsers from MIME-sniffing a response's content type, blocking certain content-injection attacks. |
| `X-Frame-Options: DENY` or `SAMEORIGIN` | Controls whether the page can be rendered in an `<iframe>`, defending against clickjacking. |
| `Referrer-Policy` | Controls how much referrer information is included in outgoing requests, protecting sensitive URL data. |
| `Content-Security-Policy` | Restricts which origins can supply scripts, styles, and other resources. |
| `Permissions-Policy` | Restricts access to browser features (camera, geolocation, etc.) from the page or embedded frames. |

- `X-Content-Type-Options: nosniff` prevents Internet Explorer and older Chrome from treating a plain-text response as HTML or JavaScript, which could turn an uploaded file into an XSS vector.
- `X-Frame-Options` was the predecessor to CSP's `frame-ancestors` directive; both can be used together for maximum compatibility.
- `Referrer-Policy: strict-origin-when-cross-origin` is a sensible default that prevents leaking path-level URL information to third-party sites while preserving referrer data for same-site navigation.

---

## Q11. What is HTTP Strict Transport Security (HSTS), and how do you enable it in ASP.NET Core?

What is HTTP Strict Transport Security (HSTS), and how do you enable it in ASP.NET Core?

**Answer:** HTTP Strict Transport Security (HSTS) is a security mechanism where the server tells browsers that all future requests to that domain must use HTTPS, even if the user types `http://`. The browser caches this directive for the duration of `max-age` and refuses to make plain HTTP connections to that origin, preventing SSL-stripping attacks. In ASP.NET Core, HSTS is enabled with `AddHsts` and `UseHsts`.

- The `max-age` directive (commonly set to one year, i.e., 31536000 seconds) tells the browser how long to remember the HTTPS-only requirement.
- The `includeSubdomains` flag extends the requirement to all subdomains, which can cause problems if any subdomain does not yet have a valid certificate.
- The `preload` flag allows a site to be submitted to browser HSTS preload lists, meaning the browser enforces HTTPS even on the very first visit, before it has received the header.
- HSTS should not be enabled in development because it can lock `localhost` into HTTPS and break HTTP-only dev servers; ASP.NET Core's default templates guard `UseHsts` with `app.Environment.IsProduction()`.
- `UseHttpsRedirection` complements HSTS by redirecting the first HTTP request to HTTPS at the server level; HSTS then prevents that initial plaintext request from happening on subsequent visits.

```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

app.UseHttpsRedirection();
app.UseHsts();
```

---

## Q12. What is CORS (Cross-Origin Resource Sharing), and why does the browser enforce it?

What is CORS (Cross-Origin Resource Sharing), and why does the browser enforce it?

**Answer:** Cross-Origin Resource Sharing (CORS) is a browser security mechanism that controls which cross-origin HTTP requests a web page is allowed to make. By default, the browser's same-origin policy blocks JavaScript from reading responses from a different origin (scheme + host + port); CORS is the protocol by which a server can relax this restriction for specific trusted origins. The browser enforces it to prevent a malicious website from using a victim's credentials to silently read data from another site.

- CORS is enforced entirely by the browser — a server has no way to force a browser to apply or ignore CORS rules — and it is irrelevant to non-browser clients such as curl, Postman, or server-to-server HTTP calls.
- The server signals allowed origins by including `Access-Control-Allow-Origin` in its response; if the header is absent or mismatched, the browser blocks JavaScript from reading the response body.
- CORS does not prevent the request from reaching the server for simple requests; it only prevents the browser from giving the response back to the requesting JavaScript.
- This is a critical distinction: CORS is about protecting the client's data and cookies from being read by a foreign origin, not about blocking requests from reaching the server.

---

## Q13. How do you configure a CORS policy in ASP.NET Core, and what is the difference between a named policy and the default policy?

How do you configure a CORS policy in ASP.NET Core, and what is the difference between a named policy and the default policy?

**Answer:** CORS policies are configured with `builder.Services.AddCors` and applied with `app.UseCors`. A named policy is defined with a string key and applied selectively (per-endpoint or per-controller), while the default policy is applied globally to all endpoints without naming it. Named policies give fine-grained control in APIs that serve multiple clients with different trust levels.

- Named policies are referenced by passing the policy name to `[EnableCors("PolicyName")]` on a controller or action, or to `app.UseCors("PolicyName")` in the middleware pipeline.
- A single global policy can be set as the default by passing a builder delegate (not a name) to `AddDefaultPolicy`; it then applies without any attribute or named reference.
- `WithOrigins`, `WithMethods`, `WithHeaders`, and `AllowCredentials` are the key builder methods: `WithOrigins` restricts which origins can make requests, `AllowCredentials` permits the browser to send cookies cross-origin, and `AllowAnyOrigin` combined with `AllowCredentials` is explicitly disallowed by the spec.
- The `UseCors` middleware must be placed after `UseRouting` but before `UseAuthorization` in the pipeline to function correctly with endpoint-level CORS attributes.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("TrustedSpa", policy =>
        policy.WithOrigins("https://app.example.com")
              .WithMethods("GET", "POST")
              .AllowCredentials());
});

app.UseCors("TrustedSpa");
```

---

## Q14. What is a CORS preflight request, and when does the browser send one?

What is a CORS preflight request, and when does the browser send one?

**Answer:** A CORS preflight request is an HTTP `OPTIONS` request the browser automatically sends before a cross-origin request to check whether the server permits the actual request. The browser sends a preflight when the request uses a method other than GET, HEAD, or POST, when the Content-Type is not one of the three simple types, or when custom headers are included. The server must respond with the appropriate `Access-Control-Allow-*` headers before the browser will send the actual request.

- Simple requests (GET or POST with `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`) bypass the preflight and are sent directly, but the response is still withheld if the origin is not allowed.
- A preflight response includes headers such as `Access-Control-Allow-Origin`, `Access-Control-Allow-Methods`, and `Access-Control-Allow-Headers`; missing any of these causes the browser to abort the actual request.
- `Access-Control-Max-Age` tells the browser how long (in seconds) to cache the preflight result, reducing round-trips for repeated API calls.
- ASP.NET Core's CORS middleware handles `OPTIONS` requests automatically when a CORS policy is configured; no explicit OPTIONS endpoint is needed.

---

## Q15. Why is CORS not a server-side security boundary?

Why is CORS not a server-side security boundary?

**Answer:** CORS is enforced by the browser, not the server, meaning a server cannot rely on it to protect against unauthorized access from non-browser clients. Any HTTP client that is not a browser — including curl, Postman, server-side code, or a modified browser — can omit the `Origin` header or ignore CORS restrictions entirely and read the response without restriction. CORS's only purpose is to control which cross-origin browser scripts can read a response; it does not replace authentication or authorization.

- An API that omits authentication tokens but relies on CORS to restrict access can be freely accessed by any server-to-server call, mobile app, or script running outside a browser.
- The correct mental model: CORS protects the authenticated user's data from being silently exfiltrated by a malicious web page in the user's browser; it does not protect the server's data from attackers using other tools.
- Even within the browser, the same-origin policy only blocks JavaScript from reading the response; the request still reaches the server, which means state-changing actions (POST, DELETE) can still be triggered by simple cross-origin requests.
- Authorization (checking who can access what) must be enforced server-side regardless of what CORS policy is in place.

---

## Q16. What is an open redirect vulnerability, and how do ASP.NET Core's `LocalRedirect` and `IsLocalUrl` helpers prevent it?

What is an open redirect vulnerability, and how do ASP.NET Core's `LocalRedirect` and `IsLocalUrl` helpers prevent it?

**Answer:** An open redirect vulnerability occurs when an application accepts a URL from user input (typically a query string parameter like `?returnUrl=...`) and redirects the user to it without verifying that the destination is a safe, local URL. An attacker can craft a link like `https://legit.example.com/login?returnUrl=https://attacker.com` to redirect victims to a phishing site after they authenticate, exploiting trust in the legitimate domain. ASP.NET Core addresses this with `LocalRedirect` and `IsLocalUrl`, both of which reject absolute URLs pointing to external origins.

- `LocalRedirect(returnUrl)` on the controller base class throws an `InvalidOperationException` if `returnUrl` is not a local (relative) URL, preventing the redirect entirely rather than silently redirecting to a dangerous location.
- `IUrlHelper.IsLocalUrl(url)` returns `false` for absolute URLs, URLs with protocol-relative schemes (`//attacker.com`), or any URL that would leave the application's origin, allowing the developer to branch safely.
- Attackers often bypass naive checks that only look for `http://` by using protocol-relative URLs (`//evil.com`) or Unicode tricks; `IsLocalUrl` handles these cases by inspecting the URL structure rather than just checking for a prefix.
- The fix is simple: always validate `returnUrl` with `IsLocalUrl` or use `LocalRedirect` and never use `Redirect(returnUrl)` directly with user-supplied input.

---

## Q17. What are the `Secure`, `HttpOnly`, and `SameSite` cookie attributes, and what threat does each mitigate?

What are the `Secure`, `HttpOnly`, and `SameSite` cookie attributes, and what threat does each mitigate?

**Answer:** The `Secure`, `HttpOnly`, and `SameSite` attributes are flags that restrict how browsers transmit and expose cookies, each addressing a distinct attack vector. These attributes are applied when the server sets a cookie via the `Set-Cookie` response header and are the primary mechanism for hardening session and authentication cookies.

| Attribute | What it does | Threat mitigated |
|---|---|---|
| `Secure` | Cookie is only sent over HTTPS connections | Cookie theft over plaintext HTTP (man-in-the-middle) |
| `HttpOnly` | Cookie is inaccessible to JavaScript (`document.cookie`) | XSS-based session cookie theft |
| `SameSite=Strict` | Cookie is never sent on cross-site requests | CSRF |
| `SameSite=Lax` | Cookie is sent on top-level navigation GETs but not sub-resource cross-site requests | CSRF (balanced with usability) |
| `SameSite=None` | Cookie is always sent cross-site (requires `Secure`) | Intentional cross-site cookie sharing |

- `HttpOnly` does not prevent XSS from occurring, but it ensures a successful XSS attack cannot steal the session cookie via `document.cookie`, limiting the attacker to session-lifetime actions rather than persistent credential theft.
- `Secure` must be set on any cookie containing sensitive data; without it, the cookie is transmitted in plaintext whenever the user visits an HTTP URL, even if the site uses HTTPS.
- All three attributes should be combined for authentication cookies; ASP.NET Core Identity sets `Secure`, `HttpOnly`, and `SameSite=Lax` by default.

---

## Q18. What are the valid values of the `SameSite` cookie attribute, and how does each affect cross-site cookie sending?

What are the valid values of the `SameSite` cookie attribute, and how does each affect cross-site cookie sending?

**Answer:** The `SameSite` attribute has three values — `Strict`, `Lax`, and `None` — that control whether a cookie is included in cross-site requests. Each represents a different trade-off between CSRF protection and functionality for legitimate cross-site use cases such as OAuth flows and embedded content.

- `SameSite=Strict` prevents the cookie from being sent on any cross-site request, including navigations initiated by clicking a link from another site. This gives maximum CSRF protection but can break flows where users arrive from another origin already expecting to be logged in.
- `SameSite=Lax` is the browser default for cookies that do not specify `SameSite`. It allows the cookie to be sent on cross-site top-level navigations using safe HTTP methods (GET, HEAD), such as clicking a link, but blocks it on cross-site sub-resource requests (images, fetch calls, form POSTs). This covers most CSRF scenarios while preserving usability.
- `SameSite=None` disables the restriction entirely and sends the cookie in all cross-site contexts; it is required for third-party authentication cookies, embedded widgets, and cross-site API calls from SPAs. It must be paired with `Secure` — browsers reject a `SameSite=None` cookie over HTTP.
- Modern browsers default to `Lax` when `SameSite` is unspecified, which substantially reduces CSRF risk even without explicit configuration, though server-side anti-forgery validation remains best practice.

---

## Q19. What is the `SameSite=None` gotcha, and when does it apply?

What is the `SameSite=None` gotcha, and when does it apply?

**Answer:** The `SameSite=None` gotcha is that setting `SameSite=None` without also setting `Secure` causes browsers (starting with Chrome 80 in 2020) to reject or ignore the cookie entirely, as if the `Set-Cookie` header was never sent. This breaks cross-site scenarios such as OAuth callbacks, cross-origin API calls with credentials, and embedded iframes that depend on third-party cookies. The fix is always to pair `SameSite=None` with `Secure`, which also means the cookie requires HTTPS.

- A common manifestation is an OAuth or OpenID Connect (OIDC) login flow that works in development (HTTP) for some browsers but silently fails in others, because the correlation or state cookie is dropped due to missing `Secure` on an HTTP endpoint.
- ASP.NET Core's `CookieOptions` class requires explicitly setting both `SameSite = SameSiteMode.None` and `Secure = true` together; setting only `SameSite=None` in configuration is insufficient.
- Older browsers (particularly Safari on iOS 12 and certain enterprise IE versions) treated `SameSite=None` as `SameSite=Strict` when they first encountered the new attribute, causing the opposite problem of cookies being blocked by browsers that were supposed to send them.
- When writing cookie middleware for cross-site scenarios, it is good practice to check the `User-Agent` and apply a browser-compatibility workaround, which ASP.NET Core's `SameSiteCookiePolicy` middleware can handle.

---

## Q20. How does ASP.NET Core's cookie authentication middleware configure cookies securely by default?

How does ASP.NET Core's cookie authentication middleware configure cookies securely by default?

**Answer:** ASP.NET Core's `AddCookie` authentication handler applies a set of secure defaults to the authentication cookie: `HttpOnly=true`, `Secure=SameAsRequest` (which becomes HTTPS-only in production), and `SameSite=Lax`. These defaults mean that the cookie is inaccessible to JavaScript, is transmitted over HTTPS in production, and is not sent on most cross-site requests. Developers can override these settings in the `CookieAuthenticationOptions` during service registration.

- `Secure=SameAsRequest` is a dynamic setting that matches the security level of the request that established the session; this is sensible for development where HTTPS may not be configured, but production deployments should explicitly set `Secure=true`.
- The `ExpireTimeSpan` property controls the sliding window for the authentication ticket; setting `SlidingExpiration=false` creates an absolute expiration, which is more secure for sensitive applications.
- The `Cookie.Name` property defaults to `.AspNetCore.Cookies`; renaming it is a minor security-through-obscurity measure but more importantly avoids conflicts in multi-application deployments.
- The `LoginPath`, `LogoutPath`, and `AccessDeniedPath` properties control redirect behavior and should always point to local paths to avoid open redirect opportunities.

---

## Q21. What is rate limiting, and how do you add it to an ASP.NET Core application (.NET 7+)?

What is rate limiting, and how do you add it to an ASP.NET Core application (.NET 7+)?

**Answer:** Rate limiting is a technique that caps how many requests a client can make within a time window, protecting the application from brute-force attacks, credential stuffing, denial-of-service attempts, and API abuse. ASP.NET Core .NET 7 introduced built-in rate limiting via the `Microsoft.AspNetCore.RateLimiting` namespace, configured with `AddRateLimiter` and applied with `UseRateLimiter`. Limits can be applied globally or per endpoint using the `[EnableRateLimiting("PolicyName")]` attribute.

- `AddRateLimiter` accepts a configuration delegate where named policies are defined using algorithms like `AddFixedWindowLimiter`, `AddSlidingWindowLimiter`, `AddTokenBucketLimiter`, or `AddConcurrencyLimiter`.
- The `OnRejected` callback on the limiter options lets developers customize the response when a limit is hit, typically returning a `429 Too Many Requests` status code with a `Retry-After` header.
- A partition key function tells the limiter how to identify clients; common choices include the user's IP address (`HttpContext.Connection.RemoteIpAddress`) or authenticated user ID (`HttpContext.User.Identity.Name`).
- Rate limiting should be applied to sensitive endpoints first: login, password reset, registration, and API endpoints that perform expensive operations or access personal data.

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

app.UseRateLimiter();
```

---

## Q22. What rate-limiting algorithms are built into `Microsoft.AspNetCore.RateLimiting`, and when would you choose each?

What rate-limiting algorithms are built into `Microsoft.AspNetCore.RateLimiting`, and when would you choose each?

**Answer:** ASP.NET Core .NET 7+ ships four built-in rate-limiting algorithms: Fixed Window, Sliding Window, Token Bucket, and Concurrency limiter. Each has different characteristics suited to different traffic patterns and protection goals.

| Algorithm | How it works | Best used for |
|---|---|---|
| Fixed Window | Allows N requests per fixed time window (e.g., 100 per minute); resets at window boundary | Simple brute-force protection where precise timing is not critical |
| Sliding Window | Tracks requests across a rolling time window using multiple smaller segments | Smoother enforcement without boundary bursts; API quotas |
| Token Bucket | Issues tokens at a steady rate; requests consume tokens; unused tokens accumulate up to a maximum | Handling burst traffic gracefully while enforcing long-run average rates |
| Concurrency | Limits the number of requests processed simultaneously (not per time) | Protecting expensive operations (DB-heavy, I/O-heavy) from overload |

- Fixed Window has a well-known weakness: a client can send N requests at the very end of one window and N more at the very start of the next, achieving double the limit in a short period. Sliding Window eliminates this burst by spreading the window.
- Token Bucket is the best choice when short bursts of traffic are legitimate (a user refreshing rapidly) but sustained high rates need to be capped.
- Concurrency limiting is orthogonal to time-based limits; it prevents the server from processing too many concurrent long-running requests regardless of their arrival rate.

---

## Q23. What is input validation, and how does ASP.NET Core model binding contribute to security?

What is input validation, and how does ASP.NET Core model binding contribute to security?

**Answer:** Input validation is the process of verifying that data received from the user conforms to the expected format, type, range, and business rules before the application processes it. ASP.NET Core model binding automatically maps incoming request data (from forms, query strings, route values, and JSON bodies) to strongly-typed .NET objects, and data annotation attributes like `[Required]`, `[StringLength]`, `[Range]`, and `[RegularExpression]` are evaluated during binding to populate `ModelState`. Checking `ModelState.IsValid` before processing is the first line of defense against malformed input.

- Model binding enforces type safety: a parameter declared as `int id` cannot be bound from a string like `'; DROP TABLE--` because the binding will fail and return a `400 Bad Request` before the action executes.
- Data annotations combined with `ModelState.IsValid` checks prevent business-logic processing of obviously invalid data, but they do not prevent injection attacks — that requires parameterized queries and encoding at the point of use.
- `[ApiController]` automatically returns `400 Bad Request` with a validation error response when `ModelState` is invalid, so API controllers do not need an explicit `if (!ModelState.IsValid)` check.
- Custom validation attributes (inheriting `ValidationAttribute`) and `IValidatableObject` allow complex cross-field or business-rule validation to be expressed declaratively.

---

## Q24. What is the difference between client-side and server-side input validation, and which one is a security control?

What is the difference between client-side and server-side input validation, and which one is a security control?

**Answer:** Client-side validation runs in the browser using JavaScript or HTML5 attributes (`required`, `maxlength`, `pattern`) and provides immediate user feedback without a round-trip to the server. Server-side validation runs on the server after the request arrives and is the only true security control because any client-side validation can be bypassed by an attacker who uses a tool like curl, Postman, or a modified browser. Server-side validation must always be present; client-side validation is a UX (User Experience) enhancement only.

- An attacker can disable JavaScript, use browser developer tools, or send raw HTTP requests to bypass all client-side validation entirely. The server cannot assume that any client-side check was applied.
- Server-side validation in ASP.NET Core is performed by data annotations evaluated during model binding and checked via `ModelState.IsValid`, as well as by custom validation logic in the action or service layer.
- Having both layers serves different purposes: client-side reduces unnecessary server round-trips and improves user experience, while server-side ensures correctness and security regardless of the client's behavior.
- A system that relies solely on client-side validation is equivalent to having no validation from a security standpoint.

---

## Q25. What is a mass assignment vulnerability, and how do `[Bind]` and dedicated DTOs (Data Transfer Objects) prevent it?

What is a mass assignment vulnerability, and how do `[Bind]` and dedicated DTOs (Data Transfer Objects) prevent it?

**Answer:** A mass assignment vulnerability occurs when a model binding framework automatically maps all request properties to an object that contains sensitive fields the user should not be able to set, such as `IsAdmin`, `Role`, or `Balance`. If a model with a `Role` property is bound directly from a POST body, an attacker can add `"Role": "Admin"` to the JSON and the framework will set it. ASP.NET Core's model binding performs mass assignment by default, so developers must explicitly restrict which fields can be bound.

- The `[Bind("Name,Email")]` attribute on an action parameter tells the model binder to set only the listed properties and ignore everything else in the request.
- The preferred approach is to use a dedicated DTO (Data Transfer Object) — a separate class that only contains the fields the user is allowed to supply — and then map the DTO to the domain entity manually or using AutoMapper.
- DTOs are superior to `[Bind]` because they express intent through the type system: the DTO class itself cannot contain a `Role` property if it was never added, whereas `[Bind]` requires remembering to add the include list every time an action is created.
- EF Core's change tracking partially mitigates this in some patterns (because you fetch the entity, apply allowed changes, and save), but this only works if the attach/update pattern is followed correctly and never if the entity is constructed from scratch from user input.

---

## Q26. What are stored XSS and reflected XSS, and which is more dangerous and why?

What are stored XSS and reflected XSS, and which is more dangerous and why?

**Answer:** Stored XSS (also called persistent XSS) occurs when malicious script is saved to the server's database and later served to other users who visit the affected page. Reflected XSS occurs when the malicious script is embedded in a URL, sent to the server, and immediately reflected in the response to the same request. Stored XSS is generally considered more dangerous because it can affect every user who visits the page without requiring the attacker to trick each individual victim into clicking a crafted link.

- In stored XSS, the attacker submits a comment, profile field, or message containing a script; every user who subsequently views that content has the script executed in their browser, potentially exposing all their session cookies simultaneously.
- In reflected XSS, the attacker must persuade each victim to click a malicious link — for example via phishing email — which limits scale but can still be highly effective against targeted individuals.
- Both types are prevented server-side by HTML-encoding output at the point of rendering, not at the point of storage; storing raw user input in the database is acceptable as long as encoding is applied when the data is displayed.
- Stored XSS is particularly severe in administrative interfaces or high-traffic pages where the injected script could capture credentials or session tokens from many users simultaneously.

---

## Q27. How does DOM-based XSS differ from server-side XSS, and what mitigation applies?

How does DOM-based XSS differ from server-side XSS, and what mitigation applies?

**Answer:** DOM-based XSS (Document Object Model-based XSS) differs from reflected and stored XSS in that the malicious payload never reaches the server; it flows entirely within the browser, from an attacker-controlled source to a dangerous JavaScript sink. Common sources include `location.hash`, `location.search`, `document.referrer`, and `window.name`; common sinks include `innerHTML`, `document.write`, `eval`, and `setTimeout` with a string argument. Because the server never sees the payload, server-side output encoding provides no protection.

- The root cause is JavaScript code that reads from a user-controlled source and writes to a DOM sink without sanitizing the value; for example, `document.getElementById('msg').innerHTML = location.hash.slice(1)` is vulnerable.
- The correct mitigation is to use safe DOM APIs: prefer `textContent` over `innerHTML` when inserting text, use `createElement` and `setAttribute` when building HTML dynamically, and avoid passing untrusted strings to `eval` or `Function()`.
- A strict Content Security Policy with `script-src 'self'` and without `'unsafe-inline'` or `'unsafe-eval'` blocks many DOM-based XSS payloads even if the vulnerable code path is triggered.
- Frameworks like Angular and React use safe DOM abstraction by default (Angular's template binding encodes, React's JSX escapes) and reduce DOM XSS risk, but unsafe escapes like Angular's `bypassSecurityTrustHtml` or React's `dangerouslySetInnerHTML` reintroduce it.

---

## Q28. What is the difference between authentication and authorization, and why does confusing them lead to IDOR vulnerabilities?

What is the difference between authentication and authorization, and why does confusing them lead to IDOR vulnerabilities?

**Answer:** Authentication is the process of verifying who a user is — establishing identity, typically by validating credentials or a token. Authorization is the process of verifying what that identified user is allowed to do or access. Authentication answers "Are you who you claim to be?" while authorization answers "Are you allowed to do this specific thing?" Confusing them leads to IDOR because developers often apply authentication (require login) but omit resource-level authorization (verify the logged-in user owns this specific record).

- A typical IDOR pattern: an action method is decorated with `[Authorize]`, ensuring only logged-in users can call it, but the action fetches a record by ID from the request without checking that the record belongs to the current user — any authenticated user can access any record.
- The fix requires an additional authorization step: after fetching the record, compare its owner identifier to `User.FindFirstValue(ClaimTypes.NameIdentifier)` and return `403 Forbidden` if they do not match.
- ASP.NET Core's resource-based authorization (`IAuthorizationService.AuthorizeAsync(user, resource, policy)`) is the idiomatic pattern for expressing per-resource ownership rules as reusable policies.
- The `[Authorize]` attribute alone is an authentication gate, not a full authorization check; real-world APIs need both the attribute and explicit resource-ownership verification inside the action.

---

## Gotchas — Web Application Security (Interview Traps)

#### Gotcha 1. CORS is not a security boundary on the server

**Answer:** A common misconception is that configuring a restrictive CORS policy prevents unauthorized clients from accessing the server's data. CORS only restricts which cross-origin browser scripts can read the response; it cannot prevent non-browser clients, server-to-server calls, or any tool outside a standard browser from making requests and reading responses freely.

- A developer who removes authentication and relies on CORS to protect an endpoint has no protection at all against curl, Postman, or any server-side attacker.
- The correct rule: CORS protects authenticated users' data from being silently exfiltrated by malicious web pages; authentication and authorization protect the server's resources from everyone.

---

#### Gotcha 2. SameSite=None requires Secure — omitting it silently drops the cookie

**Answer:** Setting `SameSite=None` without the `Secure` attribute causes modern browsers (Chrome 80+, Firefox, Edge) to reject the cookie silently, as though the `Set-Cookie` header were never sent. This breaks OAuth flows, cross-site embeds, and any third-party cookie scenario without producing an obvious error.

- The failure is silent: no JavaScript exception is thrown, no network error appears in the console — the cookie simply does not arrive on subsequent requests, which manifests as mysteriously failed logins or missing authentication state.
- The fix is always to set both `SameSite = SameSiteMode.None` and `Secure = true` together in `CookieOptions`, and to ensure the endpoint is only accessible over HTTPS.

---

#### Gotcha 3. Server-side output encoding, not input sanitization, is the correct XSS defense

**Answer:** A frequent misconception is that stripping or rejecting HTML characters at input time (sanitizing on write) is the right defense against XSS. In reality, the authoritative defense is encoding at output time — encoding the data in the correct context when it is rendered into HTML, JavaScript, CSS, or a URL.

- Sanitizing input can corrupt legitimate data: a user who types `5 < 10` in a comment field should have that stored as-is; encoding it to `5 &lt; 10` only when displaying it preserves the original meaning.
- Input sanitization is appropriate for specific cases like rich-text editors where users intentionally supply HTML (using a library like HtmlSanitizer), but even then output encoding is still the final safety net.
- Razor's `@` syntax provides automatic HTML encoding at render time, which is the correct default; `@Html.Raw` bypasses this and should be used only when the value has already been validated or sanitized.

---

#### Gotcha 4. AllowAnyOrigin() and AllowCredentials() cannot be combined

**Answer:** ASP.NET Core's CORS middleware explicitly throws an `InvalidOperationException` at startup if a policy specifies both `AllowAnyOrigin()` and `AllowCredentials()`. The CORS specification itself forbids sending `Access-Control-Allow-Origin: *` alongside `Access-Control-Allow-Credentials: true`, because doing so would let any website make authenticated cross-origin requests using the visitor's cookies.

- An attempt to work around this by setting `WithOrigins("*")` instead of `AllowAnyOrigin()` does not bypass the restriction; the middleware detects wildcard origins in either form.
- The correct approach when credentials must be allowed is to enumerate the trusted origins explicitly with `WithOrigins("https://app.example.com")`, giving a meaningful security boundary.
- This restriction is enforced both by the specification and by browsers, which refuse to expose a credentialed response with a wildcard `Allow-Origin` header regardless of what the server sends.

---

#### Gotcha 5. [Authorize] is an authentication check, not an authorization check

**Answer:** The `[Authorize]` attribute without a policy or role parameter only verifies that the user is authenticated (logged in), not that the user has permission to access a specific resource. Developers often assume that placing `[Authorize]` on a controller action means the user's access is fully secured, but this leaves resource-level authorization gaps that lead to IDOR vulnerabilities.

- An authenticated user who accesses `/orders/9999` with a bare `[Authorize]` attribute can retrieve order 9999 even if it belongs to a different customer, because no check verifies ownership.
- True resource-level security requires an additional check inside the action body — comparing the resource's owner to the current user's identity — or using `IAuthorizationService` with a resource-based policy that encodes the ownership rule.

---
