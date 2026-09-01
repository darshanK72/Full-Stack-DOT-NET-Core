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

What does `AddCookie` register in the ASP.NET Core dependency injection container, and what is the role of `CookieAuthenticationOptions`?

**Answer:** `AddCookie` registers a named cookie authentication handler with the dependency injection (DI) container and binds a `CookieAuthenticationOptions` instance to that scheme name. The options object is the central configuration point for every aspect of how the cookie is created, validated, and expired.

- `AddCookie` is a shorthand for `AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>`, which wires the handler type and its options into the authentication builder.
- `CookieAuthenticationOptions` exposes properties such as `Cookie.Name`, `Cookie.HttpOnly`, `Cookie.SameSite`, `Cookie.SecurePolicy`, `LoginPath`, `LogoutPath`, `AccessDeniedPath`, `ExpireTimeSpan`, `SlidingExpiration`, and `Events`.
- The options instance can be supplied inline via a lambda or through `services.Configure<CookieAuthenticationOptions>()`, allowing environment-specific overrides without touching the handler itself.
- If multiple cookie schemes are needed — for example, a user-facing cookie and an admin-facing cookie — each call to `AddCookie` takes a distinct scheme name and its own `CookieAuthenticationOptions` instance.

---

## Q2. What is the difference between `AddAuthentication` and `UseAuthentication`, and what happens if you call one without the other?

What is the difference between `AddAuthentication` and `UseAuthentication`, and what happens if you call one without the other?

**Answer:** `AddAuthentication` is a DI registration call that tells the container which authentication schemes exist and what their options are; `UseAuthentication` is a middleware registration call that inserts the authentication middleware into the request pipeline so those schemes are actually invoked on each request. The two concerns are deliberately separated: service registration versus pipeline execution.

- Without `AddAuthentication`, there are no registered handlers, so `UseAuthentication` has nothing to invoke and `HttpContext.User` will never be populated beyond an anonymous principal.
- Without `UseAuthentication`, the handlers are registered but the middleware that calls them is absent, so even with a valid cookie on the request, `HttpContext.User` will be the default unauthenticated principal throughout the request lifecycle.
- Middleware order also matters: `UseAuthentication` must be placed before `UseAuthorization` in `Program.cs` so that the identity is established before authorization policies are evaluated.
- `AddAuthentication` optionally accepts a default scheme name; this name is used when a method like `HttpContext.AuthenticateAsync()` is called without an explicit scheme argument.

---

## Q3. Walk through how ASP.NET Core issues an authentication cookie after a successful login — from the call to `HttpContext.SignInAsync` to the `Set-Cookie` header on the response.

Walk through how ASP.NET Core issues an authentication cookie after a successful login — from the call to `HttpContext.SignInAsync` to the `Set-Cookie` header on the response.

**Answer:** When the application calls `HttpContext.SignInAsync(scheme, claimsPrincipal, authProperties)`, the cookie authentication handler serializes the principal into a protected ticket, then appends a `Set-Cookie` response header containing that ticket. The browser stores the cookie and sends it back on every subsequent request.

- The handler creates an `AuthenticationTicket` wrapping the `ClaimsPrincipal` and `AuthenticationProperties`; the ticket is serialized to bytes using `TicketSerializer`.
- The resulting byte array is encrypted and signed using the ASP.NET Core Data Protection API via `IDataProtector`, producing a tamper-proof token string that is URL-safe Base64 encoded.
- The handler sets the `Set-Cookie` header with the configured name, value (the protected token), and attributes such as `HttpOnly`, `Secure`, `SameSite`, `Path`, and `Expires` derived from `CookieAuthenticationOptions` and `AuthenticationProperties`.
- If `AuthenticationProperties.IsPersistent` is `true`, an explicit `Expires` attribute is written matching `ExpireTimeSpan`; if `false`, no `Expires` is written, producing a session cookie that the browser discards when closed.
- On subsequent requests, the `CookieAuthenticationHandler.HandleAuthenticateAsync` method reads this cookie, unprotects the token, deserializes the ticket, and populates `HttpContext.User`.

---

## Q4. What is a `ClaimsIdentity`, what is a `ClaimsPrincipal`, and how do they relate to each other?

What is a `ClaimsIdentity`, what is a `ClaimsPrincipal`, and how do they relate to each other?

**Answer:** A `ClaimsIdentity` represents a single authenticated identity — a collection of `Claim` objects plus metadata such as `AuthenticationType` and `Name` — while a `ClaimsPrincipal` is a container that can hold one or more `ClaimsIdentity` instances, representing a subject that may have been authenticated in multiple ways simultaneously. The `ClaimsPrincipal` is what ASP.NET Core stores as `HttpContext.User`.

- A `Claim` is a name-value pair (plus an optional issuer) that asserts a fact about the user, such as `ClaimTypes.NameIdentifier` for the user ID or `ClaimTypes.Email` for the email address.
- A single `ClaimsPrincipal` can hold multiple identities; for example, a user authenticated via cookie who also passed a bearer token check would have two `ClaimsIdentity` instances with different `AuthenticationType` values.
- Methods on `ClaimsPrincipal` such as `FindFirst`, `HasClaim`, and `IsInRole` search across all contained identities, so authorization code does not need to know which identity carries a given claim.
- The `IsInRole` method checks for claims of type `ClaimTypes.Role` by default, but the role claim type is configurable per identity.

---

## Q5. How is a `ClaimsPrincipal` serialized into the authentication cookie, and how is it deserialized on subsequent requests?

How is a `ClaimsPrincipal` serialized into the authentication cookie, and how is it deserialized on subsequent requests?

**Answer:** ASP.NET Core uses `TicketSerializer` to convert the `AuthenticationTicket` (which wraps the `ClaimsPrincipal` and `AuthenticationProperties`) into a compact binary format, and then the Data Protection API encrypts and authenticates that binary payload before it is Base64-encoded into the cookie value. Deserialization reverses this process on each request.

- `TicketSerializer.Default.Serialize` iterates over each `ClaimsIdentity`, writing its `AuthenticationType`, `NameClaimType`, `RoleClaimType`, and every `Claim` (type, value, value type, issuer, original issuer) into a `BinaryWriter`.
- The serialized bytes are passed to `IDataProtector.Protect`, which applies AES-256-CBC encryption and HMACSHA256 authentication by default, ensuring both confidentiality and tamper-detection.
- On an inbound request, the handler calls `IDataProtector.Unprotect`; if the payload has been modified, a `CryptographicException` is thrown and the cookie is treated as invalid, resulting in an unauthenticated request rather than an exception propagating to the user.
- Because the full principal is in the cookie, the server stores no session state by default — the cookie itself is the session, which has horizontal scaling benefits but means revoking a specific cookie requires additional server-side state.

---

## Q6. What is the `AuthenticationType` property on `ClaimsIdentity` and why does it matter for `IsAuthenticated`?

What is the `AuthenticationType` property on `ClaimsIdentity` and why does it matter for `IsAuthenticated`?

**Answer:** `AuthenticationType` is a string on `ClaimsIdentity` that names the mechanism used to authenticate the identity, such as `"Cookies"` or `"Bearer"`; the `IsAuthenticated` property returns `true` if and only if `AuthenticationType` is a non-null, non-empty string. This design means an identity with no authentication type is explicitly anonymous.

- An identity constructed with `new ClaimsIdentity()` and no authentication type argument has `IsAuthenticated == false` even if it carries claims, which is intentional — claims alone do not imply authentication.
- Authorization middleware checks `HttpContext.User.Identity.IsAuthenticated` as the first gate; if this is `false`, the request is treated as unauthenticated regardless of what claims are present.
- When ASP.NET Core Identity creates the `ClaimsIdentity` during sign-in, it passes `IdentityConstants.ApplicationScheme` (or the scheme name) as the `AuthenticationType`, so `IsAuthenticated` is always `true` for a successfully signed-in user's identity.
- A common bug occurs when developers construct a `ClaimsPrincipal` manually for testing and forget to supply the `AuthenticationType`, causing `[Authorize]` to reject the principal even in unit tests.

---

## Q7. What does `Secure` mean on an authentication cookie, what risk does omitting it introduce, and when is it safe to omit?

What does `Secure` mean on an authentication cookie, what risk does omitting it introduce, and when is it safe to omit?

**Answer:** The `Secure` attribute on a cookie instructs the browser to transmit that cookie only over HTTPS connections; without it, the browser will send the cookie over plain HTTP as well. Omitting `Secure` exposes the cookie to network-level interception, known as session hijacking or cookie theft via a man-in-the-middle attack.

- When an attacker can observe HTTP traffic on the same network — a public Wi-Fi scenario is the classic example — they can read the cookie value and replay it to impersonate the victim.
- ASP.NET Core sets `CookieSecurePolicy.SameAsRequest` by default, which means the `Secure` flag is added only when the request itself arrived over HTTPS; in production this behaves correctly but in HTTP-only development environments the flag is absent.
- `CookieSecurePolicy.Always` forces the `Secure` flag unconditionally and is the correct production setting; `CookieSecurePolicy.None` omits it unconditionally and should never be used in production.
- Omitting `Secure` is only acceptable in a fully internal environment where all traffic is on a private network with no HTTP interception risk, and even then it is considered poor practice.

---

## Q8. What does `HttpOnly` mean on an authentication cookie, and why is it the default in ASP.NET Core Identity?

What does `HttpOnly` mean on an authentication cookie, and why is it the default in ASP.NET Core Identity?

**Answer:** The `HttpOnly` attribute instructs the browser to withhold the cookie from JavaScript's `document.cookie` API and from `XMLHttpRequest`/`fetch` requests that originate from script; the browser still sends it automatically on regular HTTP requests. ASP.NET Core Identity sets `HttpOnly = true` by default to defend against Cross-Site Scripting (XSS) attacks that attempt to steal the session cookie.

- When a page is vulnerable to XSS, an injected script can read `document.cookie` and transmit all visible cookies to an attacker's server; `HttpOnly` prevents the authentication cookie from being readable by any script, even script running on the legitimate origin.
- `HttpOnly` does not prevent the cookie from being sent with requests — the browser still attaches it automatically — so it does not interfere with normal authenticated navigation.
- `HttpOnly` does not protect against Cross-Site Request Forgery (CSRF); that requires anti-forgery tokens or `SameSite` restrictions, because the browser sends `HttpOnly` cookies on cross-site form submissions just as on same-site ones.
- Setting `Cookie.HttpOnly = false` in `CookieAuthenticationOptions` would expose the cookie to JavaScript, which is occasionally needed for single-page application frameworks that inspect cookies, though the far safer alternative is to use a separate non-authentication cookie for any client-side metadata.

---

## Q9. Explain the three `SameSite` values — `Strict`, `Lax`, and `None` — and when you would choose each for a login cookie.

Explain the three `SameSite` values — `Strict`, `Lax`, and `None` — and when you would choose each for a login cookie.

**Answer:** `SameSite` is a cookie attribute that controls whether the browser includes a cookie on cross-site requests; its three values define progressively permissive policies. For an authentication cookie, the right value depends on whether the application needs to receive authenticated cross-site POST requests or top-level navigations from external sites.

| Value | Sent on same-site requests | Sent on top-level cross-site navigations (GET) | Sent on cross-site POST / sub-resource |
|---|---|---|---|
| `Strict` | Yes | No | No |
| `Lax` | Yes | Yes | No |
| `None` | Yes | Yes | Yes (requires `Secure`) |

- `Strict` is the most restrictive: the cookie is never sent on any cross-site request, including a user clicking a link from an external site; this breaks flows where users arrive from email links or external OAuth callbacks.
- `Lax` is the modern browser default and a sensible choice for most login cookies: the cookie is withheld from cross-site sub-resource loads and POST submissions (preventing CSRF on state-changing endpoints) but is included when the user navigates top-level to the site.
- `None` must be paired with `Secure` and is required when the cookie must be sent in cross-site contexts — for example, an embedded iframe or a third-party API call where the cookie identifies the user; without `SameSite=None; Secure`, browsers block the cookie in third-party contexts entirely.
- ASP.NET Core defaults to `SameSiteMode.Lax` for the application cookie, which provides good CSRF protection without breaking external navigation flows.

---

## Q10. What is the difference between sliding expiration and absolute expiration on a cookie, and how do you configure each in `CookieAuthenticationOptions`?

What is the difference between sliding expiration and absolute expiration on a cookie, and how do you configure each in `CookieAuthenticationOptions`?

**Answer:** Absolute expiration sets a fixed point in time after which the cookie is always invalid regardless of activity, while sliding expiration resets the expiry clock each time the user makes a request, keeping active users logged in indefinitely as long as they keep using the application. Both use `ExpireTimeSpan` as the base duration; sliding expiration is enabled by setting `SlidingExpiration = true`.

- With `SlidingExpiration = false` (the default), a cookie issued with `ExpireTimeSpan` of 30 minutes expires exactly 30 minutes after issuance; a user who makes a request at 29 minutes gets a response but their cookie expires in one minute regardless.
- With `SlidingExpiration = true`, the handler re-issues the cookie (updating the `Expires` header) if more than half of `ExpireTimeSpan` has elapsed since the cookie was last issued; this halving threshold prevents a cookie refresh on every single request, reducing overhead.
- Sliding expiration alone does not cap total session length; to enforce an absolute maximum session duration you must store the original issuance time as a claim and validate it in `OnValidatePrincipal`.
- `IsPersistent` on `AuthenticationProperties` controls whether the cookie survives browser closure; if `IsPersistent = false`, the cookie is a session cookie and the browser discards it regardless of `ExpireTimeSpan` when the browser closes, but sliding expiration still applies within that browser session.

---

## Q11. What is `IsPersistent` on `AuthenticationProperties`, how does it affect the cookie's lifetime, and what happens when it is `false`?

What is `IsPersistent` on `AuthenticationProperties`, how does it affect the cookie's lifetime, and what happens when it is `false`?

**Answer:** `IsPersistent` is a boolean on `AuthenticationProperties` that determines whether the authentication cookie is issued with an explicit `Expires` attribute, making it a persistent cookie that survives browser restarts, or without one, making it a session cookie that the browser removes when closed. When `IsPersistent` is `false`, the `Expires` attribute is omitted from the `Set-Cookie` header.

- A persistent cookie (`IsPersistent = true`) has its `Expires` set to `DateTime.UtcNow + ExpireTimeSpan`; the browser stores it to disk and retains it across browser sessions until that timestamp passes.
- A session cookie (`IsPersistent = false`) has no `Expires` attribute; browsers treat this as "delete when the tab or browser closes," which is the safer default for shared or public computers.
- The "Remember me" checkbox in a typical login form maps directly to `IsPersistent`: checking the box passes `true` so the user stays logged in across browser restarts, while leaving it unchecked passes `false` for a single-session login.
- `IsPersistent` interacts with `SlidingExpiration`: for a non-persistent cookie, ASP.NET Core still refreshes the ticket internally (updating the in-memory expiry), but the `Expires` attribute is never written to the response, so the browser's actual removal trigger remains browser-close.

---

## Q12. Walk through `SignInManager.PasswordSignInAsync` end to end — every step from receiving credentials to returning a `SignInResult`.

Walk through `SignInManager.PasswordSignInAsync` end to end — every step from receiving credentials to returning a `SignInResult`.

**Answer:** `PasswordSignInAsync` is a high-level orchestration method on `SignInManager<TUser>` that validates credentials, enforces lockout policy, checks two-factor authentication (2FA) requirements, and, on success, calls `SignInAsync` to issue the authentication cookie. It returns a `SignInResult` value object describing the outcome.

- The method first calls `UserManager.FindByNameAsync` (or `FindByEmailAsync` if configured) to locate the user record; if no user is found, it returns `SignInResult.Failed` without revealing whether the username exists.
- It checks whether the account requires email confirmation (`RequireConfirmedAccount` option) and returns `SignInResult.NotAllowed` if the account has not been confirmed.
- It calls `UserManager.CheckPasswordAsync`, which hashes the submitted password and compares it to the stored hash using the configured `IPasswordHasher<TUser>`; on failure it increments the access failed count and may trigger lockout.
- Lockout is checked via `IsLockedOut`; if the account is locked, `SignInResult.LockedOut` is returned and the caller is expected to redirect to a lockout page.
- If the user has 2FA enabled (`TwoFactorEnabled` and a verified authenticator or phone), the method stores a partial sign-in cookie and returns `SignInResult.TwoFactorRequired`, redirecting the user to the 2FA challenge page.
- On full success, `SignInAsync` is called, which creates the `ClaimsPrincipal` via `CreateUserPrincipalAsync` (populating claims from the user record) and calls `HttpContext.SignInAsync` to issue the cookie.

---

## Q13. Walk through the user registration flow in ASP.NET Core Identity — from form submission to the user record being persisted.

Walk through the user registration flow in ASP.NET Core Identity — from form submission to the user record being persisted.

**Answer:** Registration begins with the controller or Razor Page receiving a validated view model, then delegating to `UserManager.CreateAsync` to hash the password, validate the user object, and persist the record; if successful, the application typically signs the user in immediately. Error handling at each step is critical to producing a good user experience.

- The incoming form fields are bound to a view model and validated with Data Annotations or `FluentValidation`; model state errors (missing fields, format violations) are caught before any Identity call is made.
- A new `ApplicationUser` (or `IdentityUser`) instance is constructed from the validated input; `UserManager.CreateAsync(user, password)` is called, passing the plain-text password separately so that hashing happens inside Identity.
- `CreateAsync` runs the registered `IUserValidator<TUser>` instances (checking username uniqueness, email format, and any custom rules) and `IPasswordValidator<TUser>` instances (minimum length, required characters) before touching the store.
- If validators reject the input, `CreateAsync` returns an `IdentityResult` with `Succeeded = false` and a list of `IdentityError` objects; the controller adds these errors to `ModelState` and re-renders the form.
- On success, the application may call `SignInManager.SignInAsync(user, isPersistent: false)` to immediately authenticate the new user, or send a confirmation email first and require the user to verify before signing in.
- The `IUserStore<TUser>` implementation (typically `UserStore` backed by Entity Framework Core) persists the user, normalised username, normalised email, and password hash to the `AspNetUsers` table.

---

## Q14. What validation does `UserManager.CreateAsync` perform before inserting a user record, and how do you return validation errors to the UI?

What validation does `UserManager.CreateAsync` perform before inserting a user record, and how do you return validation errors to the UI?

**Answer:** `UserManager.CreateAsync` runs all registered `IUserValidator<TUser>` and `IPasswordValidator<TUser>` implementations synchronously before touching the data store; it aggregates every error from every validator into a single `IdentityResult` and returns without persisting if any validator fails. The UI receives these errors through the `IdentityResult.Errors` collection.

- The default `UserValidator<TUser>` checks that the username is not empty, that it matches the configured `AllowedUserNameCharacters` pattern, and that the email address is unique (if `RequireUniqueEmail` is set).
- The default `PasswordValidator<TUser>` enforces `PasswordOptions` rules: minimum length, required digit, required uppercase letter, required lowercase letter, and required non-alphanumeric character — each of which is independently configurable in `AddIdentity`.
- The pattern for returning errors to the UI is to iterate `result.Errors` and call `ModelState.AddModelError(string.Empty, error.Description)` for each, then return the view with the current model so the user sees all failures at once.
- Custom validators are registered by implementing `IUserValidator<TUser>` or `IPasswordValidator<TUser>` and adding them via `services.AddTransient<IUserValidator<ApplicationUser>, MyCustomUserValidator>()`, after which they participate in the same aggregation pass automatically.

---

## Q15. How does ASP.NET Core Identity handle account lockout during login, and what options control lockout behavior?

How does ASP.NET Core Identity handle account lockout during login, and what options control lockout behavior?

**Answer:** When a login attempt fails credential verification, `UserManager` calls `AccessFailedAsync` to increment the failed-attempt counter for that user; once the counter reaches `MaxFailedAccessAttempts`, `LockoutEnd` is set to a future timestamp and subsequent login attempts return `SignInResult.LockedOut` without checking the password. Lockout is configured via `LockoutOptions` inside `AddIdentity`.

- Key options are `MaxFailedAccessAttempts` (default 5), `DefaultLockoutTimeSpan` (default 5 minutes), and `AllowedForNewUsers` (default `true`, meaning new accounts participate in lockout from creation).
- Lockout must be requested explicitly: `SignInManager.PasswordSignInAsync` has a `lockoutOnFailure` parameter; passing `false` disables lockout tracking for that call, which is useful when a trusted internal system is verifying credentials.
- After a successful login, `UserManager.ResetAccessFailedCountAsync` is called automatically to reset the failure counter back to zero, preventing a user who eventually logs in correctly from carrying a near-lockout count into their session.
- An administrator can unlock an account by calling `UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow)`, which clears the lockout immediately.
- Lockout is stored in the `LockoutEnd` and `AccessFailedCount` columns of the `AspNetUsers` table; a `null` `LockoutEnd` or a past timestamp means the account is not locked.

---

## Q16. How does `PasswordSignInAsync` handle a user who has two-factor authentication (2FA) enabled?

How does `PasswordSignInAsync` handle a user who has two-factor authentication (2FA) enabled?

**Answer:** When credentials are valid but the user has 2FA enabled, `PasswordSignInAsync` stores a short-lived partial authentication cookie and returns `SignInResult.TwoFactorRequired` instead of issuing the full application cookie; the partial cookie carries the user ID so the 2FA verification page can identify the user without re-submitting credentials. Full authentication only completes after the second factor is verified.

- The partial cookie is issued via `SignInManager.SignInWithClaimsAsync` using the `IdentityConstants.TwoFactorUserIdScheme`, a separate cookie scheme with a short expiry (five minutes by default) intended only to bridge the gap between the password step and the 2FA code step.
- The 2FA page calls `SignInManager.GetTwoFactorAuthenticationUserAsync()`, which reads the partial cookie to retrieve the pending user, then presents the authenticator-app or SMS code input.
- Once the user submits a valid time-based one-time password (TOTP) or SMS code, `SignInManager.TwoFactorAuthenticatorSignInAsync` or `TwoFactorSignInAsync` validates the code, deletes the partial cookie, and issues the full application cookie.
- If the 2FA step times out or the partial cookie expires before the code is submitted, the user must start from the password step again; there is no implicit fallback to single-factor authentication.

---

## Q17. What does `SignOutAsync` do at the HTTP level, and what is required to fully destroy the session from the server side?

What does `SignOutAsync` do at the HTTP level, and what is required to fully destroy the session from the server side?

**Answer:** `SignOutAsync` calls the cookie authentication handler, which appends a `Set-Cookie` response header that overwrites the authentication cookie with an expired, empty value — effectively instructing the browser to delete the cookie. Because ASP.NET Core's default cookie authentication is stateless (no server-side session), there is nothing server-side to destroy unless an external session store is in use.

- The cookie deletion is achieved by setting the cookie value to an empty string and `Expires` to a past date (typically `DateTimeOffset.UnixEpoch`), which browsers interpret as an instruction to remove the cookie.
- The server never stores the cookie value itself in the default setup, so there is no server-side record to revoke; once the browser discards the cookie, the authentication ticket is gone.
- If the application needs true server-side revocation — for example, to invalidate a cookie immediately after a password change or account ban — it must store a revocation flag or a "last changed" timestamp server-side and check it in `OnValidatePrincipal` on each request.
- After calling `SignOutAsync`, the application should redirect the user rather than rendering a page, because if the response is cached by the browser the user might see the cached authenticated page while the cookie has actually been deleted.

---

## Q18. What is the `ReturnUrl` pattern, why is it a security risk if not validated, and how does ASP.NET Core prevent open redirects?

What is the `ReturnUrl` pattern, why is it a security risk if not validated, and how does ASP.NET Core prevent open redirects?

**Answer:** `ReturnUrl` is a query string parameter appended by the authentication middleware when it redirects an unauthenticated user to the `LoginPath`; after a successful login, the application redirects back to this URL so the user arrives at the page they originally requested. If the application blindly redirects to any `ReturnUrl` value, an attacker can craft a link that sends users to a malicious external site after they log in — an open redirect vulnerability.

- An open redirect attack works by an attacker sending a user a link such as `/login?ReturnUrl=https://evil.example.com/phishing`; after the user logs in on the legitimate site, they are bounced to the attacker's page, which may mimic the original site to capture additional data.
- ASP.NET Core provides `LocalRedirect` and `Url.IsLocalUrl` on controllers and Razor Pages; `LocalRedirect` throws an `InvalidOperationException` if the URL is not local, while `Url.IsLocalUrl` returns `false` for absolute URLs with an external host, allowing the application to fall back to a safe default.
- A URL is considered local if it starts with `/` (but not `//`, which is a protocol-relative URL pointing to an external host) or starts with `~`; absolute URLs with a scheme (`http://`, `https://`) are never local.
- The correct pattern is: after login, check `Url.IsLocalUrl(returnUrl)` and if it is `true` redirect to `returnUrl`, otherwise redirect to `/` or another safe default; never redirect to an unvalidated URL.

---

## Q19. What are anti-forgery tokens (XSRF/CSRF tokens), and how do they protect login and registration forms?

What are anti-forgery tokens (XSRF/CSRF tokens), and how do they protect login and registration forms?

**Answer:** Anti-forgery tokens, also called Cross-Site Request Forgery (CSRF) or XSRF tokens, are cryptographically random values that ASP.NET Core embeds in HTML forms and simultaneously stores in a cookie; when a form is submitted, the server verifies that both copies match, ensuring the submission originated from a page the server rendered rather than from a cross-site attacker. This protects state-changing endpoints like login and registration from CSRF attacks.

- A CSRF attack against a login form (login CSRF) allows an attacker to log a victim into the attacker's account on a target site; the victim then uses the site while authenticated as the attacker, causing them to unknowingly store sensitive data in the attacker's account.
- ASP.NET Core Razor Pages validates anti-forgery tokens on all POST handlers by default; MVC controllers require either the `[ValidateAntiForgeryToken]` attribute on the action or `[AutoValidateAntiforgeryToken]` on the controller.
- The `<form>` tag helper (`asp-action`) automatically renders a hidden `__RequestVerificationToken` field; the `@Html.AntiForgeryToken()` helper does the same in classic Razor syntax.
- The token cookie is set with `HttpOnly = false` by default (because the framework needs to read it in JavaScript-heavy scenarios) and `SameSite = Strict`; it is separate from the authentication cookie and has a shorter lifetime.
- The Data Protection API backs the anti-forgery token generation and validation, meaning the token is tied to the specific server key material; in a web farm, all nodes must share the same Data Protection keys.

---

## Q20. How does the ASP.NET Core Data Protection API protect the authentication cookie, and what key management concerns should you be aware of in a web farm?

How does the ASP.NET Core Data Protection API protect the authentication cookie, and what key management concerns should you be aware of in a web farm?

**Answer:** The Data Protection API (DPAPI in the ASP.NET Core context, not Windows DPAPI) provides authenticated encryption: it encrypts the cookie payload with AES-256-CBC and signs it with HMACSHA256, so any tampering invalidates the signature and the cookie is rejected. Key management — where keys are stored and how they are rotated — is the primary operational concern, especially when multiple server instances must decrypt the same cookie.

- By default, Data Protection generates a master key and stores it in a platform-specific location: `%APPDATA%\Microsoft\UserSecrets` on Windows developer machines, or a file under `~/.aspnet/DataProtection-Keys` on Linux; in production this must be changed to a shared location.
- In a web farm or container environment, all instances must use the same key ring; if each instance generates its own keys, a cookie encrypted on server A cannot be decrypted on server B, causing random authentication failures as requests are load-balanced across nodes.
- Key sharing is configured with `PersistKeysToFileSystem`, `PersistKeysToAzureBlobStorage`, `PersistKeysToDbContext` (via `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`), or other storage providers; keys should also be protected at rest with `ProtectKeysWith...` using Azure Key Vault or a certificate.
- Keys auto-rotate every 90 days by default; the framework keeps old keys to decrypt cookies issued before rotation, so users are not logged out on key rotation as long as the old keys remain in the ring.
- If Data Protection keys are lost (for example, an ephemeral container is destroyed), all outstanding authentication tickets become unreadable and every user is effectively logged out.

---

## Q21. What is `CookieAuthenticationEvents.OnValidatePrincipal`, when does it execute, and what is a typical use case?

What is `CookieAuthenticationEvents.OnValidatePrincipal`, when does it execute, and what is a typical use case?

**Answer:** `OnValidatePrincipal` is a callback on `CookieAuthenticationEvents` that the cookie handler invokes every time it successfully deserializes the authentication ticket from the cookie — that is, on every authenticated request before the principal is assigned to `HttpContext.User`. It provides a hook to perform server-side validation or enrichment of the principal that the cookie alone cannot guarantee.

- Because the cookie is self-contained and stateless, it cannot reflect server-side changes such as a password reset, a role change, or an account ban that occurred after the cookie was issued; `OnValidatePrincipal` is where you check whether the user's security stamp (or another versioning value) still matches the database.
- ASP.NET Core Identity wires up its own `OnValidatePrincipal` implementation (`SecurityStampValidator`) by default when you call `AddIdentity`; it compares the `SecurityStamp` claim in the cookie against the current stamp in the database and rejects the principal if they differ, forcing re-authentication.
- The callback receives a `CookieValidatePrincipalContext`; calling `context.RejectPrincipal()` followed by `CookieAuthenticationDefaults` instructs the middleware to treat the current cookie as invalid and redirect to the login page.
- Custom implementations can throttle the frequency of database checks by storing a `LastCheck` timestamp in the cookie and only hitting the database when a configurable interval has passed, balancing security with database load.

---

## Q22. What happens when `OnValidatePrincipal` calls `context.RejectPrincipal()`?

What happens when `OnValidatePrincipal` calls `context.RejectPrincipal()`?

**Answer:** Calling `context.RejectPrincipal()` instructs the cookie authentication middleware to discard the deserialized ticket and set `HttpContext.User` to an unauthenticated principal for the current request; subsequent middleware and endpoints see an anonymous user. To also delete the cookie from the browser, the handler must additionally call `await context.HttpContext.SignOutAsync()` inside the callback.

- Without the explicit `SignOutAsync` call, `RejectPrincipal` only nullifies the principal for the current request; the cookie remains in the browser and would be sent again on the next request, triggering `OnValidatePrincipal` again and again.
- The common pattern inside a security-stamp check is: call `context.RejectPrincipal()` and `await context.HttpContext.SignOutAsync(scheme)` so the browser's cookie is deleted and the user is redirected to the login page on the next navigation.
- `RejectPrincipal` does not throw or short-circuit the middleware pipeline; the request continues through subsequent middleware with the unauthenticated principal, which is why `UseAuthorization` must come after `UseAuthentication` to enforce access control.
- The `SecurityStampValidator` in ASP.NET Core Identity uses this exact pattern combined with a `ValidationInterval` (default 30 minutes) to limit how often the security stamp database check runs.

---

## Q23. What claims does ASP.NET Core Identity include in the cookie by default, and how do you add custom claims?

What claims does ASP.NET Core Identity include in the cookie by default, and how do you add custom claims?

**Answer:** When `SignInManager.SignInAsync` builds the `ClaimsPrincipal`, it calls `UserClaimsPrincipalFactory.CreateAsync`, which adds a fixed set of claims derived from the user record; the exact claims are `NameIdentifier` (user ID), `Name` (username), `Email` (if present), `SecurityStamp`, and all claims stored in the `AspNetUserClaims` table. Custom claims are added by overriding `UserClaimsPrincipalFactory` or by implementing `IUserClaimsPrincipalFactory<TUser>`.

- `ClaimTypes.NameIdentifier` maps to `user.Id`, `ClaimTypes.Name` maps to `user.UserName`, and role claims (`ClaimTypes.Role`) are added for each role in `AspNetUserRoles` when roles are enabled.
- The `SecurityStamp` claim is stored as a custom claim type (`AspNet.Identity.SecurityStamp`) and is used by `SecurityStampValidator` to detect invalidated sessions; it is not a standard JWT claim type.
- To add custom claims, create a class inheriting `UserClaimsPrincipalFactory<ApplicationUser>`, override `GenerateClaimsAsync`, call `await base.GenerateClaimsAsync(user)` to get the default identity, then add additional claims to it.
- Register the custom factory in DI: `services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, MyClaimsPrincipalFactory>()`, ensuring it is registered after `AddIdentity` so it overrides the default.
- Because claims are serialized into the cookie, large claim sets increase cookie size; browsers enforce a 4 KB limit per cookie, so claims that represent large datasets should be looked up from the database per request rather than embedded.

---

## Q24. What is the difference between `UserManager<TUser>` and `SignInManager<TUser>` in terms of responsibilities?

What is the difference between `UserManager<TUser>` and `SignInManager<TUser>` in terms of responsibilities?

**Answer:** `UserManager<TUser>` is concerned with user data — creating, updating, deleting, finding, and validating user records and their associated credentials, claims, roles, and tokens; `SignInManager<TUser>` is concerned with authentication — coordinating the sign-in process, issuing cookies, handling lockout, 2FA, and external login flows. `UserManager` knows nothing about the HTTP context; `SignInManager` wraps `UserManager` and adds the HTTP and cookie layer.

| Concern | `UserManager<TUser>` | `SignInManager<TUser>` |
|---|---|---|
| Creating users | `CreateAsync` | — |
| Password hashing | `CheckPasswordAsync`, `ChangePasswordAsync` | calls `UserManager` internally |
| Cookie issuance | — | `SignInAsync`, `PasswordSignInAsync` |
| Lockout enforcement | `AccessFailedAsync`, `IsLockedOutAsync` | calls `UserManager` internally |
| External login | `AddLoginAsync`, `FindByLoginAsync` | `ExternalLoginSignInAsync` |
| 2FA tokens | `GenerateTwoFactorTokenAsync` | `TwoFactorAuthenticatorSignInAsync` |
| HTTP context access | None | Yes (via `IHttpContextAccessor`) |

- `UserManager` can be used in background services, console applications, and tests without a running HTTP server; `SignInManager` depends on `IHttpContextAccessor` and is meaningful only within a request context.
- When writing custom identity logic — such as a bulk import tool or an admin service — use `UserManager` directly; `SignInManager` is for authentication endpoints that issue cookies.

---

## Q25. What is `SignInResult` and what are its possible outcomes after calling `PasswordSignInAsync`?

What is `SignInResult` and what are its possible outcomes after calling `PasswordSignInAsync`?

**Answer:** `SignInResult` is an immutable value object returned by `PasswordSignInAsync` that communicates the outcome of the sign-in attempt as a set of boolean properties; it does not throw exceptions for normal failure cases, keeping control flow predictable. The caller branches on these properties to decide what page to redirect to.

- `SignInResult.Succeeded` is `true` when credentials are correct, the account is not locked, email confirmation is satisfied, and no additional factor is required; the full application cookie has been issued.
- `SignInResult.IsLockedOut` is `true` when the account's `LockoutEnd` is in the future; the caller should redirect to a lockout information page and not reveal the lockout duration to the user in detail.
- `SignInResult.IsNotAllowed` is `true` when the account exists and the password is correct but sign-in is blocked for another reason — most commonly because email confirmation is required and has not been completed.
- `SignInResult.RequiresTwoFactor` is `true` when credentials are valid but the user has 2FA enabled; a partial cookie has been issued and the caller should redirect to the 2FA code entry page.
- When all four boolean properties are `false` and `Succeeded` is `false`, the credentials were simply incorrect; the caller should add a generic error message to the model and re-render the login form without indicating whether the username or password was wrong.

---

## Q26. How does the `TicketDataFormat` and `IDataProtector` chain protect against cookie forging and replay attacks?

How does the `TicketDataFormat` and `IDataProtector` chain protect against cookie forging and replay attacks?

**Answer:** `TicketDataFormat` wraps an `IDataProtector` and is responsible for converting an `AuthenticationTicket` to a protected string and back; the `IDataProtector` applies authenticated encryption (AES-256-CBC with HMACSHA256) so the ciphertext cannot be decrypted or modified without the server's key material. Any alteration to the cookie value — even a single bit — causes `Unprotect` to throw a `CryptographicException`, and the middleware treats the result as an invalid, unauthenticated request.

- The `IDataProtector` is scoped to a purpose string (e.g., `"Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware"` plus the scheme name), meaning a data protector created for cookies cannot be reused to unprotect tokens from a different purpose, preventing cross-scheme attacks.
- The ticket includes an expiry timestamp inside the encrypted payload; `HandleAuthenticateAsync` compares this embedded expiry against `DateTime.UtcNow` and rejects expired tickets even if the encryption is valid — closing replay attack windows where an attacker captures a valid cookie and attempts to use it after the legitimate user has logged out.
- Key rotation (every 90 days by default) means that even if an attacker somehow captured the key material from a past period, the window of compromise is bounded; old keys are retained in the ring for decryption but new cookies are encrypted with the newest key.
- The protection is end-to-end at the server: neither the cookie value's plaintext nor the user's claims are visible in transit if `Secure` is enforced, and they are never readable by JavaScript if `HttpOnly` is set.

---

## Q27. What is the relationship between the cookie authentication scheme's `LoginPath`, `AccessDeniedPath`, and `ReturnUrl` in the challenge and forbid flow?

What is the relationship between the cookie authentication scheme's `LoginPath`, `AccessDeniedPath`, and `ReturnUrl` in the challenge and forbid flow?

**Answer:** When an unauthenticated request hits a protected endpoint, the cookie handler issues an HTTP 302 redirect to `LoginPath` with the original URL appended as a `ReturnUrl` query parameter — this is the challenge flow; when an authenticated user lacks the required permission, the handler redirects to `AccessDeniedPath` — this is the forbid flow. Both paths and the `ReturnUrl` parameter name are configurable in `CookieAuthenticationOptions`.

- The challenge flow is triggered by `IAuthenticationService.ChallengeAsync`, which in cookie authentication means redirecting to `LoginPath?ReturnUrl=%2Fprotected-page`; after successful login, the application should redirect to the validated `ReturnUrl` to complete the round trip.
- The forbid flow is triggered by `IAuthenticationService.ForbidAsync` when the user is authenticated but fails an authorization policy; the redirect goes to `AccessDeniedPath` without a `ReturnUrl` because sending the user back to a page they are not allowed to see would be meaningless.
- If `LoginPath` or `AccessDeniedPath` is not configured, the handler falls back to returning a raw HTTP 401 or HTTP 403 status code respectively, which is the correct behavior for API endpoints that are not browser-navigated.
- The `ReturnUrlParameter` property (default `"ReturnUrl"`) controls the query string key; changing it requires updating all login page logic that reads and redirects to it.

---

## Q28. Describe three common mistakes developers make with cookie authentication in ASP.NET Core (interview gotchas).

Describe three common mistakes developers make with cookie authentication in ASP.NET Core (interview gotchas).

**Answer:** See the Gotchas section below.

---

## Gotchas — Cookie Authentication (Interview Traps)

---

#### Gotcha 1. Confusing `HttpOnly` with protection against CSRF

**Answer:** `HttpOnly` prevents JavaScript from reading the cookie, but it does not prevent the browser from automatically sending the cookie on cross-site form submissions; Cross-Site Request Forgery (CSRF) exploits the automatic sending behavior, not the reading behavior. A site that relies solely on `HttpOnly` to prevent CSRF is still vulnerable.

- CSRF works because the browser attaches the authentication cookie to any request to the target domain, whether that request was initiated by the target site or by a malicious external page; `HttpOnly` has no bearing on this automatic attachment.
- The correct defenses against CSRF are anti-forgery tokens (which a cross-site attacker cannot read or replicate) and `SameSite=Lax` or `SameSite=Strict` cookie policy (which instructs modern browsers to withhold the cookie on cross-site POSTs).

---

#### Gotcha 2. Assuming the cookie is revoked when `SignOutAsync` is called

**Answer:** `SignOutAsync` instructs the browser to delete the cookie by sending an expired `Set-Cookie` header, but if the browser ignores this instruction or the cookie value was copied before sign-out, the authentication ticket remains cryptographically valid until its embedded expiry timestamp. Without server-side state, there is no way for the server to reject a previously valid ticket on its own.

- This matters most for security-sensitive flows: after a password reset or account compromise, calling `SignOutAsync` does not invalidate outstanding cookies held on other devices; an attacker who copied the cookie before sign-out can continue to use it.
- The correct solution is to update the user's `SecurityStamp` (via `UserManager.UpdateSecurityStampAsync`) whenever credentials or permissions change; the `SecurityStampValidator` running in `OnValidatePrincipal` will detect the stamp mismatch on the next request and reject the stale cookie.

---

#### Gotcha 3. Not sharing Data Protection keys in a web farm

**Answer:** When multiple server instances each generate their own Data Protection key ring, a cookie encrypted by one server cannot be decrypted by another, causing random authentication failures that are difficult to diagnose because they depend on which server the load balancer routes each request to. This is a common production issue with containerised deployments.

- The symptom is users being randomly redirected to the login page even though they recently logged in, with no obvious error in the application logs because the cookie unprotect failure is silently handled as "invalid ticket."
- The fix is to configure `PersistKeysToFileSystem`, `PersistKeysToAzureBlobStorage`, or another shared store and optionally protect the keys with `ProtectKeysWith...` so all instances share both the key ring and its encryption.

---

#### Gotcha 4. Forgetting that `SameSite=None` requires `Secure`

**Answer:** Setting `SameSiteMode.None` in `CookieAuthenticationOptions` without also setting `CookieSecurePolicy.Always` (or `SecurePolicy.SameAsRequest` on an HTTPS host) produces a cookie that modern browsers reject entirely in third-party contexts, silently breaking cross-site authentication flows. The `SameSite=None` attribute is only honoured by browsers when paired with `Secure`.

- Browsers that implement the 2020 SameSite default changes (Chrome 80+, Firefox, Safari) will drop a `SameSite=None` cookie that lacks `Secure`, treating it as if the cookie was never sent; the application sees an unauthenticated request with no visible error.
- This pairing requirement is enforced at the browser level, not by ASP.NET Core, so the application logs will not show a warning; the failure surfaces only as authentication not working in embedded or cross-origin scenarios.

---

#### Gotcha 5. Setting `ClaimsIdentity` without an `AuthenticationType` and expecting `IsAuthenticated` to be `true`

**Answer:** `ClaimsIdentity.IsAuthenticated` is `true` if and only if the `AuthenticationType` property is a non-null, non-empty string; constructing an identity with only claims but no `AuthenticationType` produces a principal that carries claims but is treated as unauthenticated by the authorization middleware. This is a frequent mistake in unit tests and custom sign-in flows.

- The property is computed, not stored: `public bool IsAuthenticated => !string.IsNullOrEmpty(AuthenticationType);` — there is no way to set `IsAuthenticated` directly, which surprises developers who expect it to behave like a flag.
- The fix is always to pass the scheme name or a descriptive string as the `authenticationType` argument when constructing `ClaimsIdentity`: `new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)`.

---
