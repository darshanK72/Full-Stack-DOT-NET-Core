# Registration, Login & Cookie Authentication — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What does `AddCookie` register in the ASP.NET Core dependency injection container, and what is the role of `CookieAuthenticationOptions`?](#q1-what-does-addcookie-register-in-the-aspnet-core-dependency-injection-container-and-what-is-the-role-of-cookieauthenticationoptions)
2. [Q2. What is the difference between `AddAuthentication` and `UseAuthentication`, and what happens if you call one without the other?](#q2-what-is-the-difference-between-addauthentication-and-useauthentication-and-what-happens-if-you-call-one-without-the-other)
3. [Q3. Walk through how ASP.NET Core issues an authentication cookie after a successful login — from the call to `HttpContext.SignInAsync` to the `Set-Cookie` header on the response.](#q3-walk-through-how-aspnet-core-issues-an-authentication-cookie-after-a-successful-login-from-the-call-to-httpcontextsigninasync-to-the-set-cookie-header-on-the-response)
4. [Q4. What is a `ClaimsIdentity`, what is a `ClaimsPrincipal`, and how do they relate to each other?](#q4-what-is-a-claimsidentity-what-is-a-claimsprincipal-and-how-do-they-relate-to-each-other)
5. [Q5. How is a `ClaimsPrincipal` serialized into the authentication cookie, and how is it deserialized on subsequent requests?](#q5-how-is-a-claimsprincipal-serialized-into-the-authentication-cookie-and-how-is-it-deserialized-on-subsequent-requests)
6. [Q6. What is the `AuthenticationType` property on `ClaimsIdentity` and why does it matter for `IsAuthenticated`?](#q6-what-is-the-authenticationtype-property-on-claimsidentity-and-why-does-it-matter-for-isauthenticated)
7. [Q7. What does `Secure` mean on an authentication cookie, what risk does omitting it introduce, and when is it safe to omit?](#q7-what-does-secure-mean-on-an-authentication-cookie-what-risk-does-omitting-it-introduce-and-when-is-it-safe-to-omit)
8. [Q8. What does `HttpOnly` mean on an authentication cookie, and why is it the default in ASP.NET Core Identity?](#q8-what-does-httponly-mean-on-an-authentication-cookie-and-why-is-it-the-default-in-aspnet-core-identity)
9. [Q9. Explain the three `SameSite` values — `Strict`, `Lax`, and `None` — and when you would choose each for a login cookie.](#q9-explain-the-three-samesite-values-strict-lax-and-none-and-when-you-would-choose-each-for-a-login-cookie)
10. [Q10. What is the difference between sliding expiration and absolute expiration on a cookie, and how do you configure each in `CookieAuthenticationOptions`?](#q10-what-is-the-difference-between-sliding-expiration-and-absolute-expiration-on-a-cookie-and-how-do-you-configure-each-in-cookieauthenticationoptions)
11. [Q11. What is `IsPersistent` on `AuthenticationProperties`, how does it affect the cookie's lifetime, and what happens when it is `false`?](#q11-what-is-ispersistent-on-authenticationproperties-how-does-it-affect-the-cookies-lifetime-and-what-happens-when-it-is-false)
12. [Q12. Walk through `SignInManager.PasswordSignInAsync` end to end — every step from receiving credentials to returning a `SignInResult`.](#q12-walk-through-signinmanagerpasswordsigninasync-end-to-end-every-step-from-receiving-credentials-to-returning-a-signinresult)
13. [Q13. Walk through the user registration flow in ASP.NET Core Identity — from form submission to the user record being persisted.](#q13-walk-through-the-user-registration-flow-in-aspnet-core-identity-from-form-submission-to-the-user-record-being-persisted)
14. [Q14. What validation does `UserManager.CreateAsync` perform before inserting a user record, and how do you return validation errors to the UI?](#q14-what-validation-does-usermanagercreateasync-perform-before-inserting-a-user-record-and-how-do-you-return-validation-errors-to-the-ui)
15. [Q15. How does ASP.NET Core Identity handle account lockout during login, and what options control lockout behavior?](#q15-how-does-aspnet-core-identity-handle-account-lockout-during-login-and-what-options-control-lockout-behavior)
16. [Q16. How does `PasswordSignInAsync` handle a user who has two-factor authentication (2FA) enabled?](#q16-how-does-passwordsigninasync-handle-a-user-who-has-two-factor-authentication-2fa-enabled)
17. [Q17. What does `SignOutAsync` do at the HTTP level, and what is required to fully destroy the session from the server side?](#q17-what-does-signoutasync-do-at-the-http-level-and-what-is-required-to-fully-destroy-the-session-from-the-server-side)
18. [Q18. What is the `ReturnUrl` pattern, why is it a security risk if not validated, and how does ASP.NET Core prevent open redirects?](#q18-what-is-the-returnurl-pattern-why-is-it-a-security-risk-if-not-validated-and-how-does-aspnet-core-prevent-open-redirects)
19. [Q19. What are anti-forgery tokens (XSRF/CSRF tokens), and how do they protect login and registration forms?](#q19-what-are-anti-forgery-tokens-xsrfcsrf-tokens-and-how-do-they-protect-login-and-registration-forms)
20. [Q20. How does the ASP.NET Core Data Protection API protect the authentication cookie, and what key management concerns should you be aware of in a web farm?](#q20-how-does-the-aspnet-core-data-protection-api-protect-the-authentication-cookie-and-what-key-management-concerns-should-you-be-aware-of-in-a-web-farm)
21. [Q21. What is `CookieAuthenticationEvents.OnValidatePrincipal`, when does it execute, and what is a typical use case?](#q21-what-is-cookieauthenticationeventsonvalidateprincipal-when-does-it-execute-and-what-is-a-typical-use-case)
22. [Q22. What happens when `OnValidatePrincipal` calls `context.RejectPrincipal()`?](#q22-what-happens-when-onvalidateprincipal-calls-contextrejectprincipal)
23. [Q23. What claims does ASP.NET Core Identity include in the cookie by default, and how do you add custom claims?](#q23-what-claims-does-aspnet-core-identity-include-in-the-cookie-by-default-and-how-do-you-add-custom-claims)
24. [Q24. What is the difference between `UserManager<TUser>` and `SignInManager<TUser>` in terms of responsibilities?](#q24-what-is-the-difference-between-usermanagertuser-and-signinmanagertuser-in-terms-of-responsibilities)
25. [Q25. What is `SignInResult` and what are its possible outcomes after calling `PasswordSignInAsync`?](#q25-what-is-signinresult-and-what-are-its-possible-outcomes-after-calling-passwordsigninasync)
26. [Q26. How does the `TicketDataFormat` and `IDataProtector` chain protect against cookie forging and replay attacks?](#q26-how-does-the-ticketdataformat-and-idataprotector-chain-protect-against-cookie-forging-and-replay-attacks)
27. [Q27. What is the relationship between the cookie authentication scheme's `LoginPath`, `AccessDeniedPath`, and `ReturnUrl` in the challenge and forbid flow?](#q27-what-is-the-relationship-between-the-cookie-authentication-schemes-loginpath-accessdeniedpath-and-returnurl-in-the-challenge-and-forbid-flow)
28. [Q28. Describe three common mistakes developers make with cookie authentication in ASP.NET Core (interview gotchas).](#q28-describe-three-common-mistakes-developers-make-with-cookie-authentication-in-aspnet-core-interview-gotchas)

---

## Q1. What does `AddCookie` register in the ASP.NET Core dependency injection container, and what is the role of `CookieAuthenticationOptions`?

**Concepts**
- `AddCookie` as `AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>` shorthand
- `CookieAuthenticationOptions` as the configuration root for a named scheme
- Multiple named cookie schemes with independent options
- Inline lambda vs `services.Configure` registration

**Answer**

`AddCookie` registers a named cookie authentication handler with the DI container and is shorthand for `AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>`, which wires the handler type and its options into the authentication builder. The bound `CookieAuthenticationOptions` instance becomes the central configuration point for every aspect of how the cookie is created, validated, and expired — it exposes properties such as `Cookie.Name`, `Cookie.HttpOnly`, `Cookie.SameSite`, `Cookie.SecurePolicy`, `LoginPath`, `LogoutPath`, `AccessDeniedPath`, `ExpireTimeSpan`, `SlidingExpiration`, and `Events`. The options can be supplied inline via a lambda or through `services.Configure<CookieAuthenticationOptions>()`, which allows environment-specific overrides without touching the handler itself. When multiple cookie schemes are needed — for example, a user-facing cookie and an admin-facing cookie — each call to `AddCookie` takes a distinct scheme name and its own independent `CookieAuthenticationOptions` instance.

---

## Q2. What is the difference between `AddAuthentication` and `UseAuthentication`, and what happens if you call one without the other?

**Concepts**
- DI service registration vs middleware pipeline execution
- `HttpContext.User` population — requires both to be present
- Middleware ordering — `UseAuthentication` before `UseAuthorization`
- Default scheme resolution for scheme-less `AuthenticateAsync` calls

**Answer**

`AddAuthentication` is a DI registration call that tells the container which authentication schemes exist and what their options are, while `UseAuthentication` is a middleware registration call that inserts the authentication middleware into the request pipeline so those schemes are actually invoked on each request. The two concerns are deliberately separated — service registration versus pipeline execution — so omitting either has a distinct effect. Without `AddAuthentication`, there are no registered handlers, so `UseAuthentication` has nothing to invoke and `HttpContext.User` will never be populated beyond an anonymous principal. Without `UseAuthentication`, the handlers are registered but the middleware that calls them is absent, which means even a valid cookie on the request is never read and `HttpContext.User` remains the default unauthenticated principal throughout the request lifecycle. Middleware order also matters: `UseAuthentication` must be placed before `UseAuthorization` in `Program.cs` so the identity is established before authorization policies are evaluated. `AddAuthentication` optionally accepts a default scheme name, which is used when `HttpContext.AuthenticateAsync()` is called without an explicit scheme argument.

---

## Q3. Walk through how ASP.NET Core issues an authentication cookie after a successful login — from the call to `HttpContext.SignInAsync` to the `Set-Cookie` header on the response.

**Concepts**
- `AuthenticationTicket` — principal wrapped with properties
- `TicketSerializer` binary serialization
- Data Protection API — AES-256-CBC + HMACSHA256 authenticated encryption
- `Set-Cookie` attributes derived from options and `AuthenticationProperties`
- `IsPersistent` controlling the presence of the `Expires` attribute

**Answer**

When the application calls `HttpContext.SignInAsync(scheme, claimsPrincipal, authProperties)`, the cookie authentication handler serializes the principal into a protected ticket, then appends a `Set-Cookie` response header containing that ticket, and the browser stores the cookie and sends it back on every subsequent request. The handler creates an `AuthenticationTicket` wrapping the `ClaimsPrincipal` and `AuthenticationProperties`, then uses `TicketSerializer` to convert it to a compact byte array, writing each `ClaimsIdentity`'s authentication type, claim types, values, and issuers. That byte array is passed to `IDataProtector.Protect`, which applies AES-256-CBC encryption and HMACSHA256 authentication by default, producing a tamper-proof URL-safe Base64-encoded token. The `Set-Cookie` header is then written with the configured name, that protected value, and attributes such as `HttpOnly`, `Secure`, `SameSite`, `Path`, and `Expires` derived from `CookieAuthenticationOptions` and `AuthenticationProperties`. If `AuthenticationProperties.IsPersistent` is `true`, an explicit `Expires` attribute is written matching `ExpireTimeSpan`; if `false`, no `Expires` is written, producing a session cookie that the browser discards when closed. On subsequent requests, `CookieAuthenticationHandler.HandleAuthenticateAsync` reads this cookie, calls `Unprotect` on the token, deserializes the ticket, and populates `HttpContext.User`.

---

## Q4. What is a `ClaimsIdentity`, what is a `ClaimsPrincipal`, and how do they relate to each other?

**Concepts**
- `ClaimsIdentity` — single authenticated identity with a collection of claims
- `ClaimsPrincipal` — container for one or more `ClaimsIdentity` instances
- `Claim` — name-value assertion with an optional issuer
- `IsInRole` and `FindFirst` — cross-identity aggregation on the principal

**Answer**

A `ClaimsIdentity` represents a single authenticated identity — a collection of `Claim` objects plus metadata such as `AuthenticationType` and `Name` — while a `ClaimsPrincipal` is a container that can hold one or more `ClaimsIdentity` instances, representing a subject that may have been authenticated in multiple ways simultaneously. The `ClaimsPrincipal` is what ASP.NET Core stores as `HttpContext.User`. A `Claim` is a name-value pair, plus an optional issuer, that asserts a fact about the user, such as `ClaimTypes.NameIdentifier` for the user ID or `ClaimTypes.Email` for the email address. A single `ClaimsPrincipal` can hold multiple identities — for example, a user authenticated via cookie who also passed a bearer token check would have two `ClaimsIdentity` instances with different `AuthenticationType` values. Methods on `ClaimsPrincipal` such as `FindFirst`, `HasClaim`, and `IsInRole` search across all contained identities, so authorization code does not need to know which identity carries a given claim. The `IsInRole` method checks for claims of type `ClaimTypes.Role` by default, but the role claim type is configurable per identity.

---

## Q5. How is a `ClaimsPrincipal` serialized into the authentication cookie, and how is it deserialized on subsequent requests?

**Concepts**
- `TicketSerializer` compact binary format
- `IDataProtector.Protect` — AES-256-CBC + HMACSHA256
- `CryptographicException` on tamper detection — request treated as unauthenticated
- Stateless self-contained session — no server-side session record

**Answer**

ASP.NET Core uses `TicketSerializer` to convert the `AuthenticationTicket` — which wraps the `ClaimsPrincipal` and `AuthenticationProperties` — into a compact binary format, and then the Data Protection API encrypts and authenticates that binary payload before it is Base64-encoded into the cookie value. `TicketSerializer.Default.Serialize` iterates over each `ClaimsIdentity`, writing its `AuthenticationType`, `NameClaimType`, `RoleClaimType`, and every `Claim` (type, value, value type, issuer, original issuer) into a `BinaryWriter`. The serialized bytes are passed to `IDataProtector.Protect`, which applies AES-256-CBC encryption and HMACSHA256 authentication, ensuring both confidentiality and tamper detection. On an inbound request, the handler calls `IDataProtector.Unprotect`; if the payload has been modified even slightly, a `CryptographicException` is thrown and the cookie is treated as invalid, resulting in an unauthenticated request rather than an exception propagating to the user. Because the full principal is embedded in the cookie, the server stores no session state by default, which has horizontal scaling benefits but means revoking a specific cookie requires additional server-side state if you need immediate invalidation.

---

## Q6. What is the `AuthenticationType` property on `ClaimsIdentity` and why does it matter for `IsAuthenticated`?

**Concepts**
- `AuthenticationType` as the `IsAuthenticated` gate
- Anonymous vs authenticated identity — the null/empty string distinction
- `IsAuthenticated` computed, not stored
- Common bug — missing `AuthenticationType` in manually constructed principals

**Answer**

`AuthenticationType` is a string on `ClaimsIdentity` that names the mechanism used to authenticate the identity, such as `"Cookies"` or `"Bearer"`, and `IsAuthenticated` returns `true` if and only if `AuthenticationType` is a non-null, non-empty string. This design means an identity with no authentication type is explicitly anonymous, because claims alone do not imply authentication — an identity constructed with `new ClaimsIdentity()` and no authentication type argument has `IsAuthenticated == false` even if it carries claims. Authorization middleware checks `HttpContext.User.Identity.IsAuthenticated` as the first gate; if this is `false`, the request is treated as unauthenticated regardless of what claims are present. When ASP.NET Core Identity creates the `ClaimsIdentity` during sign-in, it passes `IdentityConstants.ApplicationScheme` (or the scheme name) as the `AuthenticationType`, so `IsAuthenticated` is always `true` for a successfully signed-in user's identity. A common bug occurs when developers construct a `ClaimsPrincipal` manually for testing and forget to supply the `AuthenticationType`, causing `[Authorize]` to reject the principal even in unit tests where the intent is clearly to represent an authenticated user.

---

## Q7. What does `Secure` mean on an authentication cookie, what risk does omitting it introduce, and when is it safe to omit?

**Concepts**
- `Secure` flag — HTTPS-only transmission instruction to the browser
- Session hijacking via cookie theft on unencrypted HTTP
- `CookieSecurePolicy` — Always, SameAsRequest, None
- Development vs production behavior difference

**Answer**

The `Secure` attribute on a cookie instructs the browser to transmit that cookie only over HTTPS connections; without it, the browser will send the cookie over plain HTTP as well, which exposes the cookie to network-level interception in a session hijacking or man-in-the-middle attack. When an attacker can observe HTTP traffic on the same network — a public Wi-Fi scenario is the classic example — they can read the cookie value and replay it to impersonate the victim. ASP.NET Core sets `CookieSecurePolicy.SameAsRequest` by default, which means the `Secure` flag is added only when the request itself arrived over HTTPS; in production this behaves correctly, but in HTTP-only development environments the flag is absent. `CookieSecurePolicy.Always` forces the `Secure` flag unconditionally and is the correct production setting, while `CookieSecurePolicy.None` omits it unconditionally and should never be used in production. Omitting `Secure` is only acceptable in a fully internal environment where all traffic is on a private network with no HTTP interception risk, and even then it is considered poor practice.

---

## Q8. What does `HttpOnly` mean on an authentication cookie, and why is it the default in ASP.NET Core Identity?

**Concepts**
- `HttpOnly` — JavaScript `document.cookie` inaccessibility
- XSS-based session cookie theft defense
- `HttpOnly` vs CSRF — orthogonal attack surfaces
- SPA `HttpOnly = false` trade-off

**Answer**

The `HttpOnly` attribute instructs the browser to withhold the cookie from JavaScript's `document.cookie` API and from `XMLHttpRequest`/`fetch` requests that originate from script; the browser still sends it automatically on regular HTTP requests. ASP.NET Core Identity sets `HttpOnly = true` by default to defend against Cross-Site Scripting (XSS) attacks that attempt to steal the session cookie, since when a page is vulnerable to XSS, an injected script can read `document.cookie` and transmit all visible cookies to an attacker's server. `HttpOnly` does not prevent the cookie from being sent with requests — the browser still attaches it automatically — so it does not interfere with normal authenticated navigation. Importantly, `HttpOnly` does not protect against Cross-Site Request Forgery (CSRF); the browser sends `HttpOnly` cookies on cross-site form submissions just as on same-site ones, so CSRF requires anti-forgery tokens or `SameSite` restrictions as separate defenses. Setting `Cookie.HttpOnly = false` in `CookieAuthenticationOptions` would expose the cookie to JavaScript, which is occasionally needed for single-page application frameworks that inspect cookies, though the far safer alternative is to use a separate non-authentication cookie for any client-side metadata.

---

## Q9. Explain the three `SameSite` values — `Strict`, `Lax`, and `None` — and when you would choose each for a login cookie.

**Concepts**
- `SameSite` — cross-site request cookie policy
- `Strict` — top-level cross-site navigation blocked
- `Lax` — modern browser default, blocks cross-site POSTs
- `SameSite=None; Secure` pairing requirement for third-party contexts

**Answer**

`SameSite` is a cookie attribute that controls whether the browser includes a cookie on cross-site requests; its three values define progressively permissive policies, and the right value depends on whether the application needs to receive authenticated cross-site POST requests or top-level navigations from external sites.

| Value | Sent on same-site requests | Sent on top-level cross-site navigations (GET) | Sent on cross-site POST / sub-resource |
|---|---|---|---|
| `Strict` | Yes | No | No |
| `Lax` | Yes | Yes | No |
| `None` | Yes | Yes | Yes (requires `Secure`) |

`Strict` is the most restrictive — the cookie is never sent on any cross-site request, including when a user clicks a link from an external site — which breaks flows where users arrive from email links or external OAuth callbacks. `Lax` is the modern browser default and a sensible choice for most login cookies because the cookie is withheld from cross-site sub-resource loads and POST submissions, preventing CSRF on state-changing endpoints, while still being included when the user navigates top-level to the site. `None` must be paired with `Secure` and is required when the cookie must be sent in cross-site contexts such as an embedded iframe or a third-party API call; without `SameSite=None; Secure`, browsers block the cookie in third-party contexts entirely. ASP.NET Core defaults to `SameSiteMode.Lax` for the application cookie, which provides good CSRF protection without breaking external navigation flows.

---

## Q10. What is the difference between sliding expiration and absolute expiration on a cookie, and how do you configure each in `CookieAuthenticationOptions`?

**Concepts**
- Absolute expiration — fixed deadline regardless of activity
- Sliding expiration — half-elapsed threshold triggers re-issue
- `SlidingExpiration = true` with `ExpireTimeSpan`
- Maximum session duration enforcement via `OnValidatePrincipal`

**Answer**

Absolute expiration sets a fixed point in time after which the cookie is always invalid regardless of activity, while sliding expiration resets the expiry clock each time the user makes a request, keeping active users logged in indefinitely as long as they keep using the application. Both use `ExpireTimeSpan` as the base duration; sliding expiration is enabled by setting `SlidingExpiration = true`. With `SlidingExpiration = false` (the default), a cookie issued with an `ExpireTimeSpan` of 30 minutes expires exactly 30 minutes after issuance — a user who makes a request at 29 minutes gets a response but their cookie expires in one minute regardless. With `SlidingExpiration = true`, the handler re-issues the cookie when more than half of `ExpireTimeSpan` has elapsed since the cookie was last issued; this halving threshold prevents a cookie refresh on every single request, reducing overhead. Sliding expiration alone does not cap total session length, so to enforce an absolute maximum session duration you must store the original issuance time as a claim and validate it in `OnValidatePrincipal`. `IsPersistent` on `AuthenticationProperties` controls whether the cookie survives browser closure; if `IsPersistent = false`, the browser discards the cookie regardless of `ExpireTimeSpan` when the browser closes, but sliding expiration still applies within that browser session.

---

## Q11. What is `IsPersistent` on `AuthenticationProperties`, how does it affect the cookie's lifetime, and what happens when it is `false`?

**Concepts**
- `IsPersistent` — persistent vs session cookie distinction
- `Expires` attribute presence controlled by `IsPersistent`
- "Remember me" checkbox mapping to `IsPersistent`
- Interaction with `SlidingExpiration` on non-persistent cookies

**Answer**

`IsPersistent` is a boolean on `AuthenticationProperties` that determines whether the authentication cookie is issued with an explicit `Expires` attribute, making it a persistent cookie that survives browser restarts, or without one, making it a session cookie that the browser removes when closed. A persistent cookie (`IsPersistent = true`) has its `Expires` set to `DateTime.UtcNow + ExpireTimeSpan`; the browser stores it to disk and retains it across browser sessions until that timestamp passes. A session cookie (`IsPersistent = false`) has no `Expires` attribute; browsers treat this as "delete when the browser closes," which is the safer default for shared or public computers. The "Remember me" checkbox in a typical login form maps directly to `IsPersistent`: checking the box passes `true` so the user stays logged in across browser restarts, while leaving it unchecked passes `false` for a single-session login. `IsPersistent` interacts with `SlidingExpiration` in that for a non-persistent cookie, ASP.NET Core still refreshes the ticket internally, updating the in-memory expiry, but the `Expires` attribute is never written to the response, so the browser's actual removal trigger remains browser-close.

---

## Q12. Walk through `SignInManager.PasswordSignInAsync` end to end — every step from receiving credentials to returning a `SignInResult`.

**Concepts**
- `PasswordSignInAsync` as high-level orchestration over `UserManager`
- `FindByNameAsync` — no username-existence disclosure on failure
- `IPasswordHasher.CheckPasswordAsync` and access-failed increment
- `SignInResult.TwoFactorRequired` and `TwoFactorUserIdScheme` partial cookie
- `CreateUserPrincipalAsync` — claim population before `HttpContext.SignInAsync`

**Answer**

`PasswordSignInAsync` is a high-level orchestration method on `SignInManager<TUser>` that validates credentials, enforces lockout policy, checks two-factor authentication requirements, and on success calls `SignInAsync` to issue the authentication cookie, returning a `SignInResult` value object describing the outcome. The method first calls `UserManager.FindByNameAsync` (or `FindByEmailAsync` if configured) to locate the user record; if no user is found, it returns `SignInResult.Failed` without revealing whether the username exists. It then checks whether the account requires email confirmation via `RequireConfirmedAccount` and returns `SignInResult.NotAllowed` if confirmation has not been completed. Next it calls `UserManager.CheckPasswordAsync`, which hashes the submitted password and compares it to the stored hash using the configured `IPasswordHasher<TUser>`; on failure it increments the access-failed count and may trigger lockout. Lockout is checked via `IsLockedOutAsync`; if the account is locked, `SignInResult.LockedOut` is returned and the caller is expected to redirect to a lockout page. If the user has 2FA enabled, the method stores a partial sign-in cookie under `IdentityConstants.TwoFactorUserIdScheme` and returns `SignInResult.TwoFactorRequired`, redirecting the user to the 2FA challenge page. On full success, `SignInAsync` is called, which creates the `ClaimsPrincipal` via `CreateUserPrincipalAsync` — populating claims from the user record — and calls `HttpContext.SignInAsync` to issue the cookie.

---

## Q13. Walk through the user registration flow in ASP.NET Core Identity — from form submission to the user record being persisted.

**Concepts**
- Model validation before any Identity call
- `UserManager.CreateAsync` — validator pipeline aggregation
- `IUserValidator` and `IPasswordValidator` — pluggable validation
- `IdentityResult.Errors` → `ModelState.AddModelError` pattern
- `IUserStore<TUser>` — EF Core persistence into `AspNetUsers`

**Answer**

Registration begins with the controller or Razor Page receiving a validated view model, then delegating to `UserManager.CreateAsync` to hash the password, validate the user object, and persist the record. The incoming form fields are bound to a view model and validated with Data Annotations or FluentValidation; model state errors are caught before any Identity call is made. A new `ApplicationUser` instance is constructed from the validated input and passed to `UserManager.CreateAsync(user, password)` with the plain-text password supplied separately so hashing happens inside Identity. `CreateAsync` runs the registered `IUserValidator<TUser>` instances — checking username uniqueness, email format, and any custom rules — and `IPasswordValidator<TUser>` instances — minimum length, required characters — before touching the store. If validators reject the input, `CreateAsync` returns an `IdentityResult` with `Succeeded = false` and a list of `IdentityError` objects; the controller adds these to `ModelState` via `ModelState.AddModelError(string.Empty, error.Description)` for each and re-renders the form so the user sees all failures at once. On success, the application typically calls `SignInManager.SignInAsync(user, isPersistent: false)` to immediately authenticate the new user, or sends a confirmation email first and requires verification before signing in. The `IUserStore<TUser>` implementation backed by Entity Framework Core persists the user, normalised username, normalised email, and password hash to the `AspNetUsers` table.

---

## Q14. What validation does `UserManager.CreateAsync` perform before inserting a user record, and how do you return validation errors to the UI?

**Concepts**
- `UserValidator<TUser>` — username characters and email uniqueness
- `PasswordValidator<TUser>` — `PasswordOptions` enforcement
- `IdentityResult` aggregation — all errors collected before returning
- Custom validator registration via `services.AddTransient<IUserValidator<...>, ...>()`

**Answer**

`UserManager.CreateAsync` runs all registered `IUserValidator<TUser>` and `IPasswordValidator<TUser>` implementations synchronously before touching the data store, aggregates every error from every validator into a single `IdentityResult`, and returns without persisting if any validator fails. The default `UserValidator<TUser>` checks that the username is not empty, that it matches the configured `AllowedUserNameCharacters` pattern, and that the email address is unique if `RequireUniqueEmail` is set. The default `PasswordValidator<TUser>` enforces `PasswordOptions` rules — minimum length, required digit, required uppercase letter, required lowercase letter, and required non-alphanumeric character — each of which is independently configurable in `AddIdentity`. The pattern for returning errors to the UI is to iterate `result.Errors` and call `ModelState.AddModelError(string.Empty, error.Description)` for each, then return the view with the current model so the user sees all failures simultaneously. Custom validators are registered by implementing `IUserValidator<TUser>` or `IPasswordValidator<TUser>` and adding them via `services.AddTransient<IUserValidator<ApplicationUser>, MyCustomUserValidator>()`, after which they participate in the same aggregation pass automatically.

---

## Q15. How does ASP.NET Core Identity handle account lockout during login, and what options control lockout behavior?

**Concepts**
- `AccessFailedAsync` — failure counter increment and lockout trigger
- `LockoutOptions` — MaxFailedAccessAttempts, DefaultLockoutTimeSpan, AllowedForNewUsers
- `lockoutOnFailure` parameter in `PasswordSignInAsync`
- `ResetAccessFailedCountAsync` on successful login
- Admin unlock via `SetLockoutEndDateAsync`

**Answer**

When a login attempt fails credential verification, `UserManager` calls `AccessFailedAsync` to increment the failed-attempt counter for that user; once the counter reaches `MaxFailedAccessAttempts`, `LockoutEnd` is set to a future timestamp and subsequent login attempts return `SignInResult.LockedOut` without checking the password. Lockout is configured via `LockoutOptions` inside `AddIdentity`, with the key options being `MaxFailedAccessAttempts` (default 5), `DefaultLockoutTimeSpan` (default 5 minutes), and `AllowedForNewUsers` (default `true`, meaning new accounts participate in lockout from creation). Lockout must be requested explicitly: `PasswordSignInAsync` has a `lockoutOnFailure` parameter, and passing `false` disables lockout tracking for that call, which is useful when a trusted internal system is verifying credentials. After a successful login, `UserManager.ResetAccessFailedCountAsync` is called automatically to reset the failure counter back to zero, preventing a user who eventually logs in correctly from carrying a near-lockout count into their session. An administrator can unlock an account by calling `UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow)`, which clears the lockout immediately; lockout is stored in the `LockoutEnd` and `AccessFailedCount` columns of the `AspNetUsers` table, so it survives application restarts and is enforced consistently across multiple server instances.

---

## Q16. How does `PasswordSignInAsync` handle a user who has two-factor authentication (2FA) enabled?

**Concepts**
- `TwoFactorUserIdScheme` partial cookie bridging password and 2FA steps
- `SignInResult.TwoFactorRequired` outcome
- `GetTwoFactorAuthenticationUserAsync` reading the partial cookie
- TOTP / SMS second-factor validation before full cookie issuance

**Answer**

When credentials are valid but the user has 2FA enabled, `PasswordSignInAsync` stores a short-lived partial authentication cookie under `IdentityConstants.TwoFactorUserIdScheme` and returns `SignInResult.TwoFactorRequired` instead of issuing the full application cookie; the partial cookie carries the user ID so the 2FA verification page can identify the user without re-submitting credentials. The 2FA page calls `SignInManager.GetTwoFactorAuthenticationUserAsync()`, which reads the partial cookie to retrieve the pending user, then presents the authenticator-app or SMS code input. Once the user submits a valid time-based one-time password (TOTP) or SMS code, `SignInManager.TwoFactorAuthenticatorSignInAsync` or `TwoFactorSignInAsync` validates the code, deletes the partial cookie, and issues the full application cookie. The partial cookie scheme has a short expiry — five minutes by default — intended only to bridge the gap between the password step and the 2FA code step, so if the 2FA step times out before the code is submitted, the user must start from the password step again with no implicit fallback to single-factor authentication.

---

## Q17. What does `SignOutAsync` do at the HTTP level, and what is required to fully destroy the session from the server side?

**Concepts**
- `SignOutAsync` — expired `Set-Cookie` header instructs browser deletion
- Stateless cookie — no server-side record to revoke by default
- Security stamp update for true cross-device invalidation
- Post-signout redirect to avoid serving cached authenticated pages

**Answer**

`SignOutAsync` calls the cookie authentication handler, which appends a `Set-Cookie` response header that overwrites the authentication cookie with an expired, empty value — effectively instructing the browser to delete the cookie. The cookie deletion is achieved by setting the cookie value to an empty string and `Expires` to a past date, which browsers interpret as an instruction to remove the cookie. Because ASP.NET Core's default cookie authentication is stateless, the server never stores the cookie value itself, so there is no server-side record to revoke; once the browser discards the cookie, the authentication ticket is gone. If the application needs true server-side revocation — for example, to invalidate a cookie immediately after a password change or account ban — it must store a revocation flag or a "last changed" timestamp server-side and check it in `OnValidatePrincipal` on each request, which is exactly how the security stamp mechanism works. After calling `SignOutAsync`, the application should redirect the user rather than rendering a page, because if the response is cached by the browser the user might see the cached authenticated page while the cookie has actually been deleted.

---

## Q18. What is the `ReturnUrl` pattern, why is it a security risk if not validated, and how does ASP.NET Core prevent open redirects?

**Concepts**
- `ReturnUrl` — appended by authentication middleware on challenge redirect
- Open redirect vulnerability — attacker-controlled external URL post-login
- `Url.IsLocalUrl` and `LocalRedirect` — local URL validation
- Protocol-relative URL `//` as a non-local URL that bypasses naive checks

**Answer**

`ReturnUrl` is a query string parameter appended by the authentication middleware when it redirects an unauthenticated user to the `LoginPath`; after a successful login, the application redirects back to this URL so the user arrives at the page they originally requested. If the application blindly redirects to any `ReturnUrl` value, an attacker can craft a link such as `/login?ReturnUrl=https://evil.example.com/phishing` that sends users to a malicious external site after they log in — an open redirect vulnerability. ASP.NET Core provides `LocalRedirect` and `Url.IsLocalUrl` on controllers and Razor Pages; `LocalRedirect` throws an `InvalidOperationException` if the URL is not local, while `Url.IsLocalUrl` returns `false` for absolute URLs with an external host, allowing the application to fall back to a safe default. A URL is considered local if it starts with `/` but not `//`, since a protocol-relative URL like `//evil.example.com` points to an external host and would be sent over the current scheme — absolute URLs with a scheme are also rejected. The correct pattern is to check `Url.IsLocalUrl(returnUrl)` after login and redirect to `returnUrl` if it is local, otherwise redirect to `/` or another safe default; never redirect to an unvalidated URL.

---

## Q19. What are anti-forgery tokens (XSRF/CSRF tokens), and how do they protect login and registration forms?

**Concepts**
- Anti-forgery token — double-submit cookie pattern
- Login CSRF — attacker logging victim into attacker's account
- `[ValidateAntiForgeryToken]` and tag helper auto-rendering
- Data Protection API backing token generation
- Web farm — shared Data Protection keys required

**Answer**

Anti-forgery tokens are cryptographically random values that ASP.NET Core embeds in HTML forms and simultaneously stores in a cookie; when a form is submitted, the server verifies that both copies match, ensuring the submission originated from a page the server rendered rather than from a cross-site attacker. A CSRF attack against a login form allows an attacker to log a victim into the attacker's account on a target site, causing them to unknowingly store sensitive data in the attacker's account — so protecting the login form is just as important as protecting state-changing endpoints. ASP.NET Core Razor Pages validates anti-forgery tokens on all POST handlers by default; MVC controllers require either the `[ValidateAntiForgeryToken]` attribute on the action or `[AutoValidateAntiforgeryToken]` on the controller. The `<form>` tag helper with `asp-action` automatically renders a hidden `__RequestVerificationToken` field, and the Data Protection API backs the token generation and validation, meaning the token is tied to the server's key material. In a web farm, all nodes must share the same Data Protection keys, since a token generated by one server must be verifiable by any other server handling the subsequent POST.

---

## Q20. How does the ASP.NET Core Data Protection API protect the authentication cookie, and what key management concerns should you be aware of in a web farm?

**Concepts**
- Data Protection API — AES-256-CBC + HMACSHA256 authenticated encryption
- Key ring — shared across all web farm instances
- Key auto-rotation — 90-day default, backward-compatible
- `PersistKeysToFileSystem` / `PersistKeysToAzureBlobStorage` for shared storage
- `ProtectKeysWith...` — key encryption at rest

**Answer**

The Data Protection API encrypts the cookie payload with AES-256-CBC and signs it with HMACSHA256, so any tampering invalidates the signature and the cookie is rejected. Key management — where keys are stored and how they are rotated — is the primary operational concern, especially when multiple server instances must decrypt the same cookie. By default, Data Protection generates a master key and stores it in a platform-specific location: `%APPDATA%\Microsoft\UserSecrets` on Windows developer machines, or a file under `~/.aspnet/DataProtection-Keys` on Linux; in production this must be changed to a shared location. In a web farm or container environment, all instances must use the same key ring; if each instance generates its own keys, a cookie encrypted on one server cannot be decrypted by another, causing random authentication failures as requests are load-balanced across nodes. Key sharing is configured with `PersistKeysToFileSystem`, `PersistKeysToAzureBlobStorage`, `PersistKeysToDbContext`, or other storage providers, and keys should also be protected at rest with `ProtectKeysWith...` using Azure Key Vault or a certificate. Keys auto-rotate every 90 days by default, and the framework keeps old keys to decrypt cookies issued before rotation, so users are not logged out on key rotation as long as the old keys remain in the ring; however, if Data Protection keys are lost entirely — for example when an ephemeral container is destroyed — all outstanding authentication tickets become unreadable and every user is effectively logged out.

---

## Q21. What is `CookieAuthenticationEvents.OnValidatePrincipal`, when does it execute, and what is a typical use case?

**Concepts**
- `OnValidatePrincipal` — per-request hook after successful ticket deserialization
- Security stamp validation — detecting post-issuance server-side changes
- `SecurityStampValidator` built-in implementation
- Throttled database check via timestamp claim

**Answer**

`OnValidatePrincipal` is a callback on `CookieAuthenticationEvents` that the cookie handler invokes every time it successfully deserializes the authentication ticket from the cookie — that is, on every authenticated request before the principal is assigned to `HttpContext.User`. Because the cookie is self-contained and stateless, it cannot reflect server-side changes such as a password reset, a role change, or an account ban that occurred after the cookie was issued, so `OnValidatePrincipal` is where you check whether the user's security stamp or another versioning value still matches the database. ASP.NET Core Identity wires up its own `OnValidatePrincipal` implementation (`SecurityStampValidator`) by default when you call `AddIdentity`; it compares the `SecurityStamp` claim in the cookie against the current stamp in the database and rejects the principal if they differ, forcing re-authentication. The callback receives a `CookieValidatePrincipalContext`; calling `context.RejectPrincipal()` followed by `SignOutAsync` instructs the middleware to treat the current cookie as invalid and redirect to the login page. Custom implementations can throttle the frequency of database checks by storing a `LastCheck` timestamp in the cookie and only hitting the database when a configurable interval has passed, balancing security with database load.

---

## Q22. What happens when `OnValidatePrincipal` calls `context.RejectPrincipal()`?

**Concepts**
- `RejectPrincipal` — current-request anonymization only
- `SignOutAsync` required to delete the browser cookie
- Re-entry loop without explicit `SignOutAsync`
- `UseAuthorization` must follow `UseAuthentication` for the rejection to have effect

**Answer**

Calling `context.RejectPrincipal()` instructs the cookie authentication middleware to discard the deserialized ticket and set `HttpContext.User` to an unauthenticated principal for the current request; subsequent middleware and endpoints see an anonymous user. Without the explicit `SignOutAsync` call, `RejectPrincipal` only nullifies the principal for the current request — the cookie remains in the browser and would be sent again on the next request, triggering `OnValidatePrincipal` again and again. The common pattern inside a security-stamp check is therefore to call both `context.RejectPrincipal()` and `await context.HttpContext.SignOutAsync(scheme)`, so the browser's cookie is deleted and the user is redirected to the login page on the next navigation. `RejectPrincipal` does not throw or short-circuit the middleware pipeline; the request continues through subsequent middleware with the unauthenticated principal, which is why `UseAuthorization` must come after `UseAuthentication` to enforce access control on the now-anonymous request. The `SecurityStampValidator` in ASP.NET Core Identity uses this exact pattern combined with a `ValidationInterval` (default 30 minutes) to limit how often the security stamp database check runs.

---

## Q23. What claims does ASP.NET Core Identity include in the cookie by default, and how do you add custom claims?

**Concepts**
- `UserClaimsPrincipalFactory` — default claim population
- `SecurityStamp` claim type — used by `SecurityStampValidator`
- Custom claims via `UserClaimsPrincipalFactory<TUser>` override
- Cookie size constraint — 4 KB browser limit

**Answer**

When `SignInManager.SignInAsync` builds the `ClaimsPrincipal`, it calls `UserClaimsPrincipalFactory.CreateAsync`, which adds a fixed set of claims from the user record: `ClaimTypes.NameIdentifier` for the user ID, `ClaimTypes.Name` for the username, `ClaimTypes.Email` if present, the `SecurityStamp` under a custom claim type, and all claims stored in the `AspNetUserClaims` table. Role claims (`ClaimTypes.Role`) are also added for each role in `AspNetUserRoles` when roles are enabled. The `SecurityStamp` claim is stored as `AspNet.Identity.SecurityStamp` and is used by `SecurityStampValidator` to detect invalidated sessions; it is not a standard JWT claim type. To add custom claims, I create a class inheriting `UserClaimsPrincipalFactory<ApplicationUser>`, override `GenerateClaimsAsync`, call `await base.GenerateClaimsAsync(user)` to get the default identity, and then add additional claims to it. The custom factory is registered in DI via `services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, MyClaimsPrincipalFactory>()`, ensuring it is registered after `AddIdentity` so it overrides the default. Because claims are serialized into the cookie, large claim sets increase cookie size; browsers enforce a 4 KB limit per cookie, so claims representing large datasets should be looked up from the database per request rather than embedded.

---

## Q24. What is the difference between `UserManager<TUser>` and `SignInManager<TUser>` in terms of responsibilities?

**Concepts**
- `UserManager` — user data operations with no HTTP dependency
- `SignInManager` — authentication orchestration requiring HTTP context
- Background service and CLI use cases for `UserManager` alone
- `IHttpContextAccessor` dependency in `SignInManager`

**Answer**

`UserManager<TUser>` is concerned with user data — creating, updating, deleting, finding, and validating user records and their associated credentials, claims, roles, and tokens — while `SignInManager<TUser>` is concerned with authentication, coordinating the sign-in process, issuing cookies, handling lockout, 2FA, and external login flows. The key architectural distinction is that `UserManager` knows nothing about the HTTP context, while `SignInManager` wraps `UserManager` and adds the HTTP and cookie layer.

| Concern | `UserManager<TUser>` | `SignInManager<TUser>` |
|---|---|---|
| Creating users | `CreateAsync` | — |
| Password hashing | `CheckPasswordAsync`, `ChangePasswordAsync` | calls `UserManager` internally |
| Cookie issuance | — | `SignInAsync`, `PasswordSignInAsync` |
| Lockout enforcement | `AccessFailedAsync`, `IsLockedOutAsync` | calls `UserManager` internally |
| External login | `AddLoginAsync`, `FindByLoginAsync` | `ExternalLoginSignInAsync` |
| 2FA tokens | `GenerateTwoFactorTokenAsync` | `TwoFactorAuthenticatorSignInAsync` |
| HTTP context access | None | Yes (via `IHttpContextAccessor`) |

`UserManager` can be used in background services, console applications, and tests without a running HTTP server since it has no HTTP dependency; `SignInManager` depends on `IHttpContextAccessor` and is meaningful only within a request context. When writing custom identity logic — such as a bulk import tool or an admin service — I use `UserManager` directly; `SignInManager` is for authentication endpoints that issue cookies.

---

## Q25. What is `SignInResult` and what are its possible outcomes after calling `PasswordSignInAsync`?

**Concepts**
- `SignInResult` — immutable discriminated outcome object
- Four boolean outcome properties — no exceptions for normal failures
- Generic "credentials incorrect" fallback — avoid disclosing which field failed
- `RequiresTwoFactor` — partial cookie already issued

**Answer**

`SignInResult` is an immutable value object returned by `PasswordSignInAsync` that communicates the outcome as a set of boolean properties; it does not throw exceptions for normal failure cases, keeping control flow predictable, and the caller branches on these properties to decide what page to redirect to. `SignInResult.Succeeded` is `true` when credentials are correct, the account is not locked, email confirmation is satisfied, and no additional factor is required — the full application cookie has been issued. `SignInResult.IsLockedOut` is `true` when the account's `LockoutEnd` is in the future; the caller should redirect to a lockout information page and not reveal the lockout duration in detail. `SignInResult.IsNotAllowed` is `true` when the account exists and the password is correct but sign-in is blocked for another reason, most commonly because email confirmation has not been completed. `SignInResult.RequiresTwoFactor` is `true` when credentials are valid but the user has 2FA enabled; a partial cookie has been issued and the caller should redirect to the 2FA code entry page. When all four boolean properties are `false` and `Succeeded` is `false`, the credentials were simply incorrect; the caller should add a generic error message and re-render the login form without indicating whether the username or password was wrong.

---

## Q26. How does the `TicketDataFormat` and `IDataProtector` chain protect against cookie forging and replay attacks?

**Concepts**
- `TicketDataFormat` — ticket-to-string serialization wrapping `IDataProtector`
- Purpose-scoped `IDataProtector` — cross-scheme attack prevention
- Embedded expiry timestamp inside the encrypted payload — replay prevention
- Key rotation — bounded compromise window

**Answer**

`TicketDataFormat` wraps an `IDataProtector` and is responsible for converting an `AuthenticationTicket` to a protected string and back; the `IDataProtector` applies authenticated encryption so the ciphertext cannot be decrypted or modified without the server's key material. Any alteration to the cookie value — even a single bit — causes `Unprotect` to throw a `CryptographicException`, and the middleware treats the result as an invalid, unauthenticated request. The `IDataProtector` is scoped to a purpose string (the scheme name), meaning a data protector created for cookies cannot be reused to unprotect tokens from a different purpose, preventing cross-scheme attacks where a token from one subsystem is replayed against another. The ticket includes an expiry timestamp inside the encrypted payload; `HandleAuthenticateAsync` compares this embedded expiry against `DateTime.UtcNow` and rejects expired tickets even if the encryption is valid, which closes replay attack windows where an attacker captures a valid cookie and attempts to use it after the legitimate user has logged out. Key rotation every 90 days by default means that even if key material from a past period were somehow compromised, the window of exposure is bounded; old keys are retained in the ring for decryption but new cookies are encrypted with the newest key.

---

## Q27. What is the relationship between the cookie authentication scheme's `LoginPath`, `AccessDeniedPath`, and `ReturnUrl` in the challenge and forbid flow?

**Concepts**
- Challenge flow — 302 to `LoginPath` with `ReturnUrl` parameter
- Forbid flow — 302 to `AccessDeniedPath` without `ReturnUrl`
- `ReturnUrlParameter` — configurable query string key name
- `LoginPath` / `AccessDeniedPath` absent — falls back to raw 401 / 403

**Answer**

When an unauthenticated request hits a protected endpoint, the cookie handler issues an HTTP 302 redirect to `LoginPath` with the original URL appended as a `ReturnUrl` query parameter — this is the challenge flow. When an authenticated user lacks the required permission, the handler redirects to `AccessDeniedPath` — this is the forbid flow. The challenge flow is triggered by `IAuthenticationService.ChallengeAsync`, which for cookie authentication redirects to `LoginPath?ReturnUrl=%2Fprotected-page`; after successful login, the application should redirect to the validated `ReturnUrl` to complete the round trip. The forbid flow is triggered by `IAuthenticationService.ForbidAsync` when the user is authenticated but fails an authorization policy; the redirect goes to `AccessDeniedPath` without a `ReturnUrl` because sending the user back to a page they are not allowed to see would be meaningless. If `LoginPath` or `AccessDeniedPath` is not configured, the handler falls back to returning a raw HTTP 401 or HTTP 403 status code respectively, which is the correct behavior for API endpoints that are not browser-navigated. The `ReturnUrlParameter` property (default `"ReturnUrl"`) controls the query string key; changing it requires updating all login page logic that reads and redirects to it.

---

## Q28. Describe three common mistakes developers make with cookie authentication in ASP.NET Core (interview gotchas).

**Concepts**
- `HttpOnly` vs CSRF — orthogonal attack surfaces
- Stateless cookie revocation misconception after `SignOutAsync`
- Data Protection key sharing in web farms and containers
- `SameSite=None` without `Secure` — silent browser rejection
- `ClaimsIdentity` without `AuthenticationType` — `IsAuthenticated` always false

**Answer**

The first common mistake is confusing `HttpOnly` with protection against CSRF. `HttpOnly` prevents JavaScript from reading the cookie but has no effect on the browser's automatic attachment of that cookie to cross-site requests, which is precisely what CSRF exploits. The correct defenses against CSRF are anti-forgery tokens and `SameSite=Lax` or `SameSite=Strict` cookie policy, not `HttpOnly`. The second mistake is assuming a cookie is revoked when `SignOutAsync` is called. `SignOutAsync` instructs the browser to delete the cookie by sending an expired `Set-Cookie` header, but the authentication ticket inside that cookie remains cryptographically valid until its embedded expiry timestamp; an attacker who captured the cookie value before sign-out can continue using it. The correct solution is to update the user's `SecurityStamp` via `UserManager.UpdateSecurityStampAsync` whenever credentials or permissions change, so the `SecurityStampValidator` running in `OnValidatePrincipal` detects the mismatch and rejects the stale cookie. A third common mistake is not sharing Data Protection keys in a web farm or container deployment. When each instance generates its own key ring, a cookie encrypted on one server cannot be decrypted by another, causing random authentication failures that depend on which server handles each request; the symptom is users being randomly redirected to the login page, and the fix is to configure a shared key store such as `PersistKeysToAzureBlobStorage`. A fourth gotcha is setting `SameSiteMode.None` without also setting `CookieSecurePolicy.Always`; modern browsers silently reject a `SameSite=None` cookie that lacks the `Secure` attribute, breaking cross-site authentication flows with no visible error in application logs. Finally, constructing a `ClaimsIdentity` without supplying an `AuthenticationType` argument causes `IsAuthenticated` to return `false` regardless of what claims the identity carries, which is a frequent source of mysterious `[Authorize]` failures in unit tests.

---

## Gotchas — Cookie Authentication (Interview Traps)

---

#### Gotcha 1. Confusing `HttpOnly` with protection against CSRF

**Concepts**
- `HttpOnly` — JS read prevention only, not transmission prevention
- CSRF — exploits automatic cookie attachment to any request to the domain
- `SameSite` and anti-forgery tokens as the actual CSRF defenses

**Answer**

`HttpOnly` prevents JavaScript from reading the cookie, but it does not prevent the browser from automatically sending the cookie on cross-site form submissions, and Cross-Site Request Forgery exploits the automatic sending behavior, not the reading behavior. CSRF works because the browser attaches the authentication cookie to any request to the target domain, whether that request was initiated by the target site or by a malicious external page; `HttpOnly` has no bearing on this automatic attachment. The correct defenses against CSRF are anti-forgery tokens, which a cross-site attacker cannot read or replicate since they must match both a hidden form field and a separate cookie value, and `SameSite=Lax` or `SameSite=Strict` cookie policy, which instructs modern browsers to withhold the cookie on cross-site POSTs.

---

#### Gotcha 2. Assuming the cookie is revoked when `SignOutAsync` is called

**Concepts**
- `SignOutAsync` — client-side browser delete via expired `Set-Cookie`
- Stateless cookie — valid until embedded expiry, server has no record
- Security stamp update as the mechanism for true server-side invalidation

**Answer**

`SignOutAsync` instructs the browser to delete the cookie by sending an expired `Set-Cookie` header, but if the browser ignores this instruction or the cookie value was copied before sign-out, the authentication ticket remains cryptographically valid until its embedded expiry timestamp. Without server-side state, there is no way for the server to reject a previously valid ticket on its own. This matters most after a password reset or account compromise: calling `SignOutAsync` does not invalidate outstanding cookies held on other devices, so an attacker who copied the cookie before sign-out can continue to use it. The correct solution is to update the user's `SecurityStamp` via `UserManager.UpdateSecurityStampAsync` whenever credentials or permissions change; the `SecurityStampValidator` running in `OnValidatePrincipal` detects the stamp mismatch on the next request and rejects the stale cookie.

---

#### Gotcha 3. Not sharing Data Protection keys in a web farm

**Concepts**
- Per-instance key ring — cross-server decryption failure
- Random auth failures correlated with load balancer routing
- `PersistKeysToFileSystem` / `PersistKeysToAzureBlobStorage` as the fix

**Answer**

When multiple server instances each generate their own Data Protection key ring, a cookie encrypted by one server cannot be decrypted by another, causing random authentication failures that are difficult to diagnose because they depend on which server the load balancer routes each request to. The symptom is users being randomly redirected to the login page even though they recently logged in, with no obvious error in the application logs because the cookie unprotect failure is silently handled as "invalid ticket." The fix is to configure `PersistKeysToFileSystem`, `PersistKeysToAzureBlobStorage`, or another shared store so all instances share the same key ring, and optionally protect the keys with `ProtectKeysWith...` so key material at rest is also encrypted.

---

#### Gotcha 4. Forgetting that `SameSite=None` requires `Secure`

**Concepts**
- Chrome 80+ `SameSite=None` enforcement requiring `Secure`
- Silent browser drop — no server-side warning or error
- Third-party and embedded iframe contexts requiring `None`

**Answer**

Setting `SameSiteMode.None` in `CookieAuthenticationOptions` without also setting `CookieSecurePolicy.Always` produces a cookie that modern browsers reject entirely in third-party contexts. The `SameSite=None` attribute is only honoured by browsers when paired with `Secure`; browsers that implement the 2020 SameSite default changes — Chrome 80+, Firefox, Safari — drop a `SameSite=None` cookie that lacks `Secure`, treating it as if the cookie was never sent. The failure surfaces only as authentication not working in embedded or cross-origin scenarios; the application logs show no warning because the rejection is enforced at the browser level, not by ASP.NET Core. The correct fix is to pair `SameSiteMode.None` with `CookieSecurePolicy.Always`.

---

#### Gotcha 5. Setting `ClaimsIdentity` without an `AuthenticationType` and expecting `IsAuthenticated` to be `true`

**Concepts**
- `IsAuthenticated` computed from `AuthenticationType` — cannot be set directly
- Common test setup pitfall — manually constructed principal rejected by `[Authorize]`
- `new ClaimsIdentity(claims, scheme)` as the correct pattern

**Answer**

`ClaimsIdentity.IsAuthenticated` is `true` if and only if the `AuthenticationType` property is a non-null, non-empty string; constructing an identity with only claims but no `AuthenticationType` produces a principal that carries claims but is treated as unauthenticated by the authorization middleware. The property is computed, not stored — `public bool IsAuthenticated => !string.IsNullOrEmpty(AuthenticationType)` — so there is no way to set `IsAuthenticated` directly, which surprises developers who expect it to behave like a settable flag. This is a frequent mistake in unit tests and custom sign-in flows where a `ClaimsPrincipal` is constructed manually. The fix is always to pass the scheme name or a descriptive string as the `authenticationType` argument when constructing `ClaimsIdentity`: `new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)`.

---

#### Gotcha 6. Sliding Expiration Rewrites the Cookie on Every Request — Expensive at Scale

**Concepts**
- `SlidingExpiration = true` issuing a new `Set-Cookie` header on every qualifying request
- Response inflation from cookie rewriting on high-traffic endpoints
- `ExpireTimeSpan` renewal threshold (half-life) controlling how often rewriting occurs

**Answer**

When `CookieAuthenticationOptions.SlidingExpiration` is `true`, the middleware checks on every authenticated request whether more than half the `ExpireTimeSpan` has elapsed since the cookie was last issued. If so, it rewrites the cookie with a new expiry — adding a `Set-Cookie` header to the response. On high-traffic applications this means every authenticated user's session cookie is silently rewritten multiple times a day, adding response overhead and causing cache-invalidation issues for responses that vary by `Set-Cookie`. Developers often enable sliding expiration for user-experience reasons without understanding its implementation cost. For APIs that receive many authenticated calls per minute, the renewal threshold should be tuned or sliding expiration should be disabled entirely in favor of an explicit session refresh endpoint.

---

#### Gotcha 7. `IsPersistent = true` Only Extends Browser-Side Lifetime — Security Stamp Validation Still Fires

**Concepts**
- `IsPersistent = true` setting an absolute expiry on the cookie — not a server-side session
- `ValidatePrincipal` security stamp check running every `ValidationInterval` regardless of persistence
- Security stamp change invalidating the persistent cookie mid-session

**Answer**

Setting `AuthenticationProperties.IsPersistent = true` in `SignInAsync` writes the cookie with an absolute expiry date so it survives the browser session, giving users a "remember me" experience. However, it does not disable the security stamp validator — if `CookieAuthenticationOptions.Events.OnValidatePrincipal` is wired with `SecurityStampValidator.ValidatePrincipalAsync`, the middleware re-checks the security stamp on every request that falls within the configured `ValidationInterval`. Any operation that updates the security stamp (password change, role change, `UpdateSecurityStampAsync`) immediately invalidates all persistent cookies for that user, ending all "remembered" sessions simultaneously. This is the intended security behavior, but developers who add security stamp validation after deployment without documenting the side effect confuse users whose persistent sessions unexpectedly expire.

---

#### Gotcha 8. Email Confirmation Token Is Single-Use — Refreshing the Confirmation Page Invalidates It

**Concepts**
- `ConfirmEmailAsync` consuming the token on first successful call
- Second call with the same token returning `IdentityResult` failure — not a security error
- Token URL in emails being forwarded or pre-fetched by security tools

**Answer**

The email confirmation token generated by `UserManager.GenerateEmailConfirmationTokenAsync` is single-use by design. Once `ConfirmEmailAsync` is called successfully, the token is consumed; calling `ConfirmEmailAsync` again with the same link — for example, because the user bookmarked it or clicked it twice — returns a failed `IdentityResult` with an "Invalid token" error. This causes confusion because users who open a confirmation email on a device where a security scanner pre-fetches links will arrive at a "link already used" error page having never clicked the link themselves. Mitigation strategies include using one-time codes instead of tokenized links, adding a confirmation landing page that requires an additional user action before consuming the token, or displaying a helpful "already confirmed — please log in" page when the token fails.

---

#### Gotcha 9. `ExternalLoginSignInAsync` Succeeds Even When `IsNotAllowed` Is the Underlying State

**Concepts**
- `IsNotAllowed` — account locked by admin or email not confirmed — blocking local login only
- `ExternalLoginSignInAsync` bypassing `IsNotAllowed` check in some Identity versions
- Explicit `CanSignInAsync` check required before processing external login

**Answer**

`SignInManager.ExternalLoginSignInAsync` links an external provider login to an existing user account and issues the application cookie. In some versions of ASP.NET Core Identity the method does not re-run all the same checks as `PasswordSignInAsync` — specifically, `IsNotAllowed` (set when email confirmation is required but not completed, or when an admin has locked the account) may not be enforced. A user whose local login returns `SignInResult.NotAllowed` can bypass that restriction by using an external provider. The safe pattern is to call `SignInManager.CanSignInAsync(user)` explicitly after retrieving the user from the external login info, and to return a `NotAllowed` response before calling `ExternalLoginSignInAsync` if the check fails.

---

#### Gotcha 10. LoginPath Redirect Loop When the Login Page Requires Authentication

**Concepts**
- `CookieAuthenticationOptions.LoginPath` redirecting unauthenticated requests to the login page
- Login page protected by `[Authorize]` creating an infinite redirect loop
- `ReturnUrl` depth growing on each redirect — eventual 404 or max-URL-length error

**Answer**

Cookie authentication redirects unauthenticated requests to the configured `LoginPath`. If the login page itself is decorated with `[Authorize]` — which can happen through a global authorization filter or a parent controller — every redirect to the login page is itself challenged, causing an infinite redirect loop. The loop is self-evidencing: the URL in the browser's address bar grows with each redirect as `ReturnUrl` is appended recursively, eventually exceeding the maximum URL length and returning a 400 or 404 error with no clear indication of the underlying cause. The fix is to decorate the login action (and any page it depends on, such as the layout partial) with `[AllowAnonymous]`, or to ensure the login route is explicitly excluded from any global authorization requirement.

---
