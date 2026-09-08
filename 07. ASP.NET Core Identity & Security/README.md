# 07. ASP.NET Core Identity & Security

Authentication, authorization, Identity, JWT, OAuth 2.0, OIDC, 2FA, secrets management.

## Topics

| # | Topic | Questions | Q&A File |
|---|-------|-----------|----------|
| 01 | Authentication & Authorization Fundamentals | 28 | [README.md](./01.%20Authentication%20%26%20Authorization%20Fundamentals/README.md) |
| 02 | ASP.NET Core Identity — Setup & User Management | 28 | [README.md](./02.%20ASP.NET%20Core%20Identity%20-%20Setup%20%26%20User%20Management/README.md) |
| 03 | Registration, Login & Cookie Authentication | 28 | [README.md](./03.%20Registration,%20Login%20%26%20Cookie%20Authentication/README.md) |
| 04 | Password Security, Lockout & Data Protection | 27 | [README.md](./04.%20Password%20Security,%20Lockout%20%26%20Data%20Protection/README.md) |
| 05 | Claims, Roles & Policy-Based Authorization | 28 | [README.md](./05.%20Claims,%20Roles%20%26%20Policy-Based%20Authorization/README.md) |
| 06 | OAuth 2.0 & OpenID Connect Fundamentals | 28 | [README.md](./06.%20OAuth%202.0%20%26%20OpenID%20Connect%20Fundamentals/README.md) |
| 07 | External Logins & OIDC Client Integration | 28 | [README.md](./07.%20External%20Logins%20%26%20OIDC%20Client%20Integration/README.md) |
| 08 | JWT Bearer Tokens & Securing Web APIs | 28 | [README.md](./08.%20JWT%20Bearer%20Tokens%20%26%20Securing%20Web%20APIs/README.md) |
| 09 | Two-Factor Authentication & Identity Extensibility | 28 | [README.md](./09.%20Two-Factor%20Authentication%20%26%20Identity%20Extensibility/README.md) |
| 10 | Web Application Security | 28 | [README.md](./10.%20Web%20Application%20Security/README.md) |
| 11 | Secrets, Certificates & Key Management | 27 | [README.md](./11.%20Secrets,%20Certificates%20%26%20Key%20Management/README.md) |
| 12 | Federated Identity & Advanced Auth Scenarios | 28 | [README.md](./12.%20Federated%20Identity%20%26%20Advanced%20Auth%20Scenarios/README.md) |

---

> Each Q&A file has a **Table of Contents** at the top linking to every question.
> Files that include scenario-based Karat questions have a **Scenario-Based Questions** section at the bottom.

---

# 07. ASP.NET Core Identity & Security — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [Authentication & Authorization Fundamentals](01.%20Authentication%20%26%20Authorization%20Fundamentals/README.md) | Core AuthN vs AuthZ concepts, middleware pipeline, and scheme selection |
| 02 | [ASP.NET Core Identity - Setup & User Management](02.%20ASP.NET%20Core%20Identity%20-%20Setup%20%26%20User%20Management/README.md) | UserManager, RoleManager, IdentityUser customization, and EF Core stores |
| 03 | [Registration, Login & Cookie Authentication](03.%20Registration%2C%20Login%20%26%20Cookie%20Authentication/README.md) | Cookie issuance, SignInManager, sliding expiration, and cookie security flags |
| 04 | [Password Security, Lockout & Data Protection](04.%20Password%20Security%2C%20Lockout%20%26%20Data%20Protection/README.md) | Password hashing, lockout policy, IPasswordHasher, and the Data Protection API |
| 05 | [Claims, Roles & Policy-Based Authorization](05.%20Claims%2C%20Roles%20%26%20Policy-Based%20Authorization/README.md) | ClaimsPrincipal, role-based vs policy-based AuthZ, and IAuthorizationHandler |
| 06 | [OAuth 2.0 & OpenID Connect Fundamentals](06.%20OAuth%202.0%20%26%20OpenID%20Connect%20Fundamentals/README.md) | OAuth grant types, OIDC ID token structure, and authorization server roles |
| 07 | [External Logins & OIDC Client Integration](07.%20External%20Logins%20%26%20OIDC%20Client%20Integration/README.md) | AddOpenIdConnect, external login callbacks, claim mapping, and account linking |
| 08 | [JWT Bearer Tokens & Securing Web APIs](08.%20JWT%20Bearer%20Tokens%20%26%20Securing%20Web%20APIs/README.md) | JWT structure, signing algorithms, TokenValidationParameters, and refresh tokens |
| 09 | [Two-Factor Authentication & Identity Extensibility](09.%20Two-Factor%20Authentication%20%26%20Identity%20Extensibility/README.md) | TOTP, SMS 2FA, TwoFactorSignInAsync, and custom token providers |
| 10 | [Web Application Security](10.%20Web%20Application%20Security/README.md) | CSRF, XSS, Content Security Policy, HSTS, and security headers |
| 11 | [Secrets, Certificates & Key Management](11.%20Secrets%2C%20Certificates%20%26%20Key%20Management/README.md) | User Secrets, environment variables, Azure Key Vault, and key rotation |
| 12 | [Federated Identity & Advanced Auth Scenarios](12.%20Federated%20Identity%20%26%20Advanced%20Auth%20Scenarios/README.md) | WS-Federation, SAML, multi-tenant auth, and claims transformation |

---

## Table of Contents
- [CQ1. Cookie Auth vs JWT — Token Delivery Models and the Hybrid SPA Pattern](#cq1-cookie-auth-vs-jwt--token-delivery-models-and-the-hybrid-spa-pattern)
- [CQ2. The Claims Pipeline from OIDC Identity Token to ClaimsPrincipal](#cq2-the-claims-pipeline-from-oidc-identity-token-to-claimsprincipal)
- [CQ3. How 2FA Interacts with Account Lockout](#cq3-how-2fa-interacts-with-account-lockout)
- [CQ4. CSRF Protection — Why Cookie Auth Needs Antiforgery Tokens but JWT Auth Does Not](#cq4-csrf-protection--why-cookie-auth-needs-antiforgery-tokens-but-jwt-auth-does-not)
- [CQ5. Storing Client Secrets and JWT Signing Keys Safely in Production](#cq5-storing-client-secrets-and-jwt-signing-keys-safely-in-production)
- [CQ6. Policy-Based Authorization Against External IdP Claims](#cq6-policy-based-authorization-against-external-idp-claims)
- [CQ7. Custom User Properties vs Claims — IUserClaimsPrincipalFactory vs On-Demand DB Lookup](#cq7-custom-user-properties-vs-claims--iuserclaimsprincipalfactory-vs-on-demand-db-lookup)
- [CQ8. PBKDF2 Iteration Count and Hash Migration on Next Login](#cq8-pbkdf2-iteration-count-and-hash-migration-on-next-login)
- [CQ9. How the Auth Cookie Is Encrypted and the Distributed Key-Ring Gotcha](#cq9-how-the-auth-cookie-is-encrypted-and-the-distributed-key-ring-gotcha)
- [CQ10. Authorization Code Flow with PKCE for SPAs and Why Implicit Flow Is Deprecated](#cq10-authorization-code-flow-with-pkce-for-spas-and-why-implicit-flow-is-deprecated)
- [CQ11. Multi-Tenant Azure Entra ID — Validating the tid Claim and the Common-Endpoint Issuer Gotcha](#cq11-multi-tenant-azure-entra-id--validating-the-tid-claim-and-the-common-endpoint-issuer-gotcha)
- [CQ12. Role Checks with Cookie Auth vs JWT — Why \[Authorize(Roles)\] Can Silently Fail](#cq12-role-checks-with-cookie-auth-vs-jwt--why-authorizeroles-can-silently-fail)
- [CQ13. First-Time 2FA Setup — TOTP Secret, QR Code, Confirmation, and Recovery Codes](#cq13-first-time-2fa-setup--totp-secret-qr-code-confirmation-and-recovery-codes)
- [CQ14. JWT Storage — localStorage vs HttpOnly Cookie vs BFF Pattern](#cq14-jwt-storage--localstorage-vs-httponly-cookie-vs-bff-pattern)
- [CQ15. Refresh Token Rotation in ASP.NET Core Identity](#cq15-refresh-token-rotation-in-aspnet-core-identity)
- [CQ16. Resource-Based Authorization — IAuthorizationService vs the \[Authorize\] Attribute](#cq16-resource-based-authorization--iauthorizationservice-vs-the-authorize-attribute)
- [CQ17. Lockout and External Provider Logins — The Unified Lockout Gap](#cq17-lockout-and-external-provider-logins--the-unified-lockout-gap)

---

> These questions cut across two or more of the twelve subtopics above.
> Each question identifies its source topics in the header line.
> For deep-dive questions on a single topic, open that subfolder's `README.md`.

---

## CQ1. Cookie Auth vs JWT — Token Delivery Models and the Hybrid SPA Pattern
> Topics: [03 — Cookie Authentication], [08 — JWT Bearer Tokens]

**Concepts**
- The browser attaches an authentication cookie automatically on every same-origin request, without any JavaScript involvement
- A JWT bearer token must be set explicitly in the `Authorization: Bearer` header by application code — the browser never sends it automatically
- Cookie auth is vulnerable to CSRF because automatic attachment means cross-site pages can trigger authenticated requests without reading the cookie
- JWT auth is vulnerable to XSS if the token is stored in `localStorage`, where malicious script can read and exfiltrate it; storing the JWT in an `HttpOnly` cookie eliminates that vector but reintroduces CSRF territory
- The hybrid SPA pattern: browser-rendered pages authenticate with a cookie; the same application's public API accepts JWT bearer tokens, giving each surface its natural token type

**Answer**
Cookie authentication and JWT bearer authentication differ most fundamentally in how the credential travels to the server. When a user logs in with cookie auth, the browser receives a `Set-Cookie` response and then attaches that cookie header to every matching request thereafter — automatically, with no JavaScript involvement. This convenience is also the attack surface: a malicious page on any origin can trigger a state-changing request to your site, and the browser will include the cookie. That is Cross-Site Request Forgery. ASP.NET Core counters it with the synchronizer token pattern: `[ValidateAntiForgeryToken]` compares a hidden form field value against a cookie value, and a cross-site page cannot read the cookie to reproduce the field value.

JWT bearer tokens work differently. The `Authorization: Bearer <token>` header is never sent automatically by the browser — JavaScript must attach it. This makes CSRF impossible for JWT-protected APIs, because a cross-site HTML form has no mechanism to set custom request headers. The risk shifts to XSS: if the JWT is stored in `localStorage`, any injected script can read and exfiltrate it. The mitigation is to store the JWT in an `HttpOnly` cookie — invisible to script — but that circles back to CSRF, so the API also needs anti-forgery protection or must rely on the `SameSite` cookie attribute.

The hybrid SPA pattern resolves the tension by assigning each token type to its natural surface. The browser application (Razor Pages, Blazor, or an MVC front end) authenticates with a cookie because the browser handles that efficiently and CSRF protection via `[ValidateAntiForgeryToken]` is straightforward. A separate public API — consumed by mobile apps, third-party integrations, or the same SPA using `fetch` — accepts only JWT bearer tokens, because those callers cannot share a cookie jar. In .NET 10, `AddAuthentication` supports multiple schemes simultaneously; `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` or a named policy selects the active scheme per endpoint.

---

## CQ2. The Claims Pipeline from OIDC Identity Token to ClaimsPrincipal
> Topics: [05 — Claims & Policy Authorization], [06 — OAuth 2.0 & OIDC], [07 — External Logins & OIDC Client], [08 — JWT Bearer Tokens]

**Concepts**
- An OIDC identity token (`id_token`) is a JWT issued by the authorization server carrying the user's identity claims (`sub`, `email`, `name`, custom attributes)
- `AddOpenIdConnect` validates the `id_token` signature, issuer, and audience, then runs a `ClaimActions` pipeline to map token claims into the local `ClaimsPrincipal`
- `MapUniqueJsonKey` copies standard claims but `DeleteClaim` strips any source claim whose JSON key collides with a known ASP.NET mapping — the raw `sub` claim is remapped to the long URI `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` and then removed
- Claims absent from the principal after mapping are absent from any downstream JWT the application re-issues; they do not appear automatically
- The fix: call `options.ClaimActions.MapJsonKey("sub", "sub")` explicitly in `AddOpenIdConnect`, or look up missing claims via `UserManager.GetClaimsAsync` at token-issuance time

**Answer**
When a user authenticates via an external OIDC provider — Azure AD, Google, Auth0, Okta — the authorization server issues an `id_token` JWT that carries identity claims such as `sub`, `email`, `name`, and any custom attributes configured on the provider. ASP.NET Core's `AddOpenIdConnect` middleware validates this token and then runs a `ClaimActions` pipeline that selects which claims to copy into the in-process `ClaimsPrincipal`. The default mapping calls `MapUniqueJsonKey` for a fixed list of well-known claims, but it also runs `DeleteClaim` for any raw JSON key it remaps to a long-form URI. The `sub` claim, for example, is remapped to `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` and the original short name is removed. A developer who calls `User.FindFirst("sub")` inside the application will find nothing, even though the claim arrived in the token.

```csharp
// Program.cs — preserve short claim names alongside the mapped URI
builder.Services.AddAuthentication()
    .AddOpenIdConnect(options =>
    {
        options.ClaimActions.MapJsonKey("sub", "sub");
        options.ClaimActions.MapJsonKey("email", "email");
        options.ClaimActions.MapJsonKey("department", "extension_department"); // custom AAD attribute
    });
```

Once the mapped `ClaimsPrincipal` is stored in the cookie, the token-issuance endpoint reads claims from `HttpContext.User` or calls `UserManager.GetClaimsAsync` to hydrate any claims that are stored in the Identity database rather than the principal. Only the claims that reach this point flow into the JWT bearer token sent to the API. If a required claim is missing there, it will not appear in the bearer token, and any policy or resource filter that checks for it will silently fail. Auditing the full pipeline — IdP token → `ClaimActions` → cookie principal → `UserManager` claims → JWT payload — is the right debugging strategy when authorization behaves unexpectedly after adding a new external provider.

---

## CQ3. How 2FA Interacts with Account Lockout
> Topics: [02 — Identity Setup], [04 — Password Security & Lockout], [09 — Two-Factor Authentication]

**Concepts**
- `IdentityOptions.Lockout` configures `MaxFailedAccessAttempts`, `DefaultLockoutTimeSpan`, and `AllowedForNewUsers`
- `PasswordSignInAsync` increments the failure counter on a bad password (when `lockoutOnFailure: true`) and resets it on a correct password
- After a correct password with 2FA enabled, `PasswordSignInAsync` returns `SignInResult.TwoFactorRequired` — the failure counter is neither incremented nor reset at this point
- `TwoFactorSignInAsync` does **not** call `UserManager.AccessFailedAsync` internally — repeated 2FA failures do not trigger lockout by default
- The security gap: an attacker with a stolen valid password can brute-force the TOTP window indefinitely without hitting the lockout threshold

**Answer**
Account lockout in ASP.NET Core Identity counts consecutive failed authentication attempts and temporarily disables the account after reaching the configured threshold (`MaxFailedAccessAttempts`). The built-in `PasswordSignInAsync` handles this automatically: every failed password check calls `UserManager.AccessFailedAsync` to increment the counter (when `lockoutOnFailure` is `true`), and a successful password check calls `UserManager.ResetAccessFailedCountAsync` to clear it. The complication arises at the 2FA boundary.

When a user enters the correct password but 2FA is enabled, `PasswordSignInAsync` returns `SignInResult.TwoFactorRequired` without touching the lockout counter. The password was valid, so no failure is recorded, and no reset is issued either. The user is redirected to the 2FA challenge page, where `TwoFactorSignInAsync` validates the one-time code. This method does not call `AccessFailedAsync` on failure — the counter stays wherever it was when the password succeeded, and repeated bad 2FA codes leave no trace in the lockout state.

The mitigation is to add explicit lockout logic in the 2FA controller action:

```csharp
var result = await _signInManager.TwoFactorSignInAsync(
    provider, code, isPersistent: false, rememberClient: false);

if (!result.Succeeded)
{
    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
    if (user is not null)
    {
        await _userManager.AccessFailedAsync(user);
        if (await _userManager.IsLockedOutAsync(user))
            return RedirectToPage("./Lockout");
    }
    ModelState.AddModelError(string.Empty, "Invalid verification code.");
}
```

With this in place, lockout protects both authentication stages. Configure `LockoutOptions` in `AddIdentity` to tune the threshold and duration; set `AllowedForNewUsers = true` if you want new accounts to be lockout-eligible from the moment of registration.

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers      = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
```

---

## CQ4. CSRF Protection — Why Cookie Auth Needs Antiforgery Tokens but JWT Auth Does Not
> Topics: [03 — Cookie Authentication], [08 — JWT Bearer Tokens], [10 — Web Application Security]

**Concepts**
- CSRF exploits the browser's automatic cookie attachment to forge authenticated requests from a page on a different origin
- The synchronizer token pattern embeds a random hidden field token (tied to the Data Protection key ring) that a cross-site page cannot read
- `[ValidateAntiForgeryToken]` compares the hidden form field value with the anti-forgery cookie value; a mismatch rejects the request with 400
- JWT bearer auth is structurally immune to CSRF — the `Authorization` header is never sent automatically by the browser
- The double-submit cookie pattern is a stateless CSRF mitigation: a readable (non-`HttpOnly`) cookie holds the CSRF token; the client must also send it in a header; the server verifies both values match

**Answer**
Cross-Site Request Forgery attacks work because browsers attach cookies to every matching request regardless of which page initiated the request. A malicious page on `evil.com` can embed a form that POSTs to `mybank.com/transfer`, and the browser will include the `mybank.com` authentication cookie — the server sees an authenticated request it never intended to allow. ASP.NET Core defends against this using the synchronizer token pattern. A cryptographically random anti-forgery token is embedded in the form as a hidden field (`__RequestVerificationToken`) and stored separately in a cookie. When the form is submitted, `[ValidateAntiForgeryToken]` (or the automatic validation built into Razor Pages) verifies that both tokens match. Because same-origin policy prevents a cross-site script from reading the cookie, `evil.com` cannot replicate the hidden field value, and the forged request is rejected.

JWT-authenticated APIs do not need this protection. The `Authorization: Bearer <token>` header is never sent automatically by the browser — application JavaScript must explicitly include it on each request. An HTML form on `evil.com` cannot set custom request headers, so a cross-site form submission to your API will arrive without an `Authorization` header and be rejected as unauthenticated. This is a structural property of JWT bearer auth, not a configuration choice.

The double-submit cookie pattern is a useful alternative when server-side session state is not available. A CSRF token is placed in a readable (non-`HttpOnly`) cookie alongside the session cookie. The browser JavaScript reads this readable cookie and echoes it in a custom request header (such as `X-CSRF-Token`). The server compares the two. An attacker on `evil.com` cannot read the cookie via script (same-origin policy blocks it), so they cannot construct the matching header. The approach is stateless because the server only needs to compare two values rather than look up a stored token.

A quick reference for how the two protection models look in .NET 10:

```csharp
// Cookie-authenticated form endpoint — requires antiforgery token
[HttpPost, ValidateAntiForgeryToken]
public IActionResult Transfer([FromForm] TransferModel model) { /* ... */ }

// JWT-authenticated API endpoint — no antiforgery needed; CORS policy instead
[HttpPost("api/transfer")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public IActionResult ApiTransfer([FromBody] TransferModel model) { /* ... */ }
```

Razor Pages automatically validates the anti-forgery token on every POST handler — there is no `[ValidateAntiForgeryToken]` attribute needed there. For Minimal API endpoints with cookie auth, call `app.UseAntiforgery()` and annotate the route with `WithRequestTimeout` or `RequireAuthorization` as appropriate; the `IAntiforgery` service is available via DI for manual validation.

---

## CQ5. Storing Client Secrets and JWT Signing Keys Safely in Production
> Topics: [07 — External Logins & OIDC Client], [08 — JWT Bearer Tokens], [11 — Secrets, Certificates & Key Management]

**Concepts**
- `client_secret` values and symmetric JWT signing keys must never appear in `appsettings.json` committed to source control
- Development: `dotnet user-secrets` stores key-value pairs in a per-user file outside the project directory, outside any repository
- Production: environment variables injected by the hosting platform, or Azure Key Vault accessed via `AddAzureKeyVault` + Managed Identity (no credential needed to retrieve the credential)
- Prefer asymmetric RS256/ES256 for JWT signing — only the issuer holds the private key; consuming APIs verify tokens using the public key from the JWKS endpoint
- Key rotation gotcha: replacing the active signing key immediately invalidates all tokens signed with the old key, including long-lived refresh tokens; a key overlap window (publish new → keep accepting old for max token lifetime → retire old) prevents a hard logout for all users

**Answer**
Credentials such as OAuth `client_secret` values and symmetric JWT signing keys are high-value targets — a leaked `client_secret` lets an attacker impersonate your application to the authorization server; a leaked HS256 signing key lets an attacker forge arbitrary JWT bearer tokens. The first rule is to keep them out of source control entirely. In development, `dotnet user-secrets init` and `dotnet user-secrets set "Jwt:SigningKey" "<value>"` store values in a per-user JSON file under `%APPDATA%\Microsoft\UserSecrets`, which the configuration system merges with `appsettings.json` at startup without any repository exposure.

In production the minimum acceptable practice is an environment variable injected at runtime by the hosting platform (an App Service Application Setting, a Kubernetes Secret mounted as an env var, or a Docker `--env` flag). A more robust solution is Azure Key Vault with `builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential())` — the application authenticates to Key Vault using its Managed Identity, so there is no credential needed to retrieve the credential.

```csharp
// Program.cs — Key Vault via Managed Identity, no secrets in code
var kvUri = builder.Configuration["KeyVaultUri"]!;
builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());

// JWT signing with RS256 — private key from Key Vault certificate
var rsa = RSA.Create();
rsa.ImportFromPem(builder.Configuration["JwtSigningKey:PrivatePem"]);
var signingKey = new RsaSecurityKey(rsa) { KeyId = "v2" };
```

For JWT signing, prefer RS256 or ES256 over HS256. With HS256 every service that validates tokens must share the same secret, multiplying the exposure surface. With RS256 only the issuer holds the private key; API consumers download the public key from the JWKS endpoint and cache it. Key rotation is safe only with an overlap window: publish the new key to the JWKS endpoint and start signing new tokens with it, but continue accepting tokens signed by the old key for at least as long as the maximum access or refresh token lifetime. Only after that window closes should the old key be retired. Rotating without the overlap window causes a hard logout for every user who holds a token signed by the old key.

---

## CQ6. Policy-Based Authorization Against External IdP Claims
> Topics: [05 — Claims & Policy Authorization], [12 — Federated Identity & Advanced Auth Scenarios]

**Concepts**
- External IdPs issue claims under their own naming conventions — Azure AD may issue `extension_department` or a long-form namespace URI instead of the local `department` claim your policy expects
- `IAuthorizationRequirement` + `IAuthorizationHandler` is the extension point; the handler calls `context.User.HasClaim(...)` and succeeds or fails the requirement
- `IClaimsTransformation` is the cross-cutting normalization point — it runs after every authentication event and can rename or add claims before any authorization handler sees the principal
- `JwtBearerOptions.MapInboundClaims` (default `false` in .NET 10) controls whether the JWT middleware renames short claim names to long-form URI equivalents — mixing behaviors across versions breaks policies that reference one form or the other
- When an external claim name differs from the ASP.NET Core expected type, `HasClaim` silently returns false, `[Authorize(Policy = "...")]` silently denies access, and there is no runtime error pointing to the mismatch

**Answer**
Federated authentication introduces a claim name mismatch problem: your policy is written against one claim naming convention, but the external IdP issues the same data under a different key. An `[Authorize(Policy = "HROnly")]` policy that calls `context.User.HasClaim("department", "HR")` will silently deny access to a user whose Azure AD token carries `extension_department = HR`, because the string comparison fails and the authorization handler returns `context.Fail()` — or, worse, simply does not call `context.Succeed()`.

The right place to fix this depends on scope. If the mismatch is provider-specific, handle it in the OIDC middleware using `ClaimActions.MapJsonKey`:

```csharp
options.ClaimActions.MapJsonKey("department", "extension_department");
```

This maps the external JSON key to the internal claim type before the principal is serialized into the cookie, so every downstream handler and policy sees the normalized name. If the normalization needs to apply across multiple schemes — cookie auth, JWT bearer, and external logins — register an `IClaimsTransformation` implementation instead:

```csharp
public class ClaimNormalizer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity!;
        var raw = identity.FindFirst("extension_department") ?? identity.FindFirst("dept");
        if (raw is not null && !identity.HasClaim("department", raw.Value))
            identity.AddClaim(new Claim("department", raw.Value));
        return Task.FromResult(principal);
    }
}
```

A separate .NET 10 gotcha is `JwtBearerOptions.MapInboundClaims`. In older .NET versions this defaulted to `true`, causing the JWT middleware to rename short claim names such as `sub` to the long URI `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`. In .NET 10 it defaults to `false`, preserving the original short names from the token. An application migrated from an earlier version that writes policies or `FindFirst` calls against the long-form URI will silently break, because the claim now arrives under `sub`, not the URI. Auditing this flag on upgrade — and choosing one canonical form for all claim references across the codebase — prevents hard-to-diagnose authorization failures.

---

## CQ7. Custom User Properties vs Claims — IUserClaimsPrincipalFactory vs On-Demand DB Lookup
> Topics: [02 — ASP.NET Core Identity Setup & User Management], [05 — Claims, Roles & Policy-Based Authorization]

**Concepts**
- A property added to `ApplicationUser` (e.g., `Tier`, `DepartmentId`) is persisted to `AspNetUsers` but is NOT automatically projected into `ClaimsPrincipal`
- `IUserClaimsPrincipalFactory<TUser>` is called once at login to build the principal that is serialized into the auth cookie
- Override `GenerateClaimsAsync` to map custom properties to claims at login time — the resulting cookie carries all needed claims without a DB round-trip per request
- On-demand DB lookup inside a handler or middleware can provide fresher data but adds latency and requires careful caching
- The gotcha: a property added to `IdentityUser` but not included in the claims factory is invisible to `ClaimsPrincipal`, to policy handlers, and to every `[Authorize]` check

**Answer**

ASP.NET Core Identity stores the user record in the `AspNetUsers` table, but when a user logs in, what actually travels through the request pipeline is a `ClaimsPrincipal` — a serialized snapshot of the identity baked into the auth cookie. These two representations are not kept in sync automatically. If you add a `Tier` property to `ApplicationUser` and run a migration, that column exists in the database, but `User.FindFirst("tier")` returns null because nothing ever copied it into the principal.

The correct extension point is `IUserClaimsPrincipalFactory<TUser>`. Override `GenerateClaimsAsync` and add the property as a claim:

```csharp
public class AppClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(
        userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(
        ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("tier", user.Tier ?? "free"));
        identity.AddClaim(new Claim("dept_id", user.DepartmentId.ToString()));
        return identity;
    }
}
```

Register with `builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>()`. After the next login, the cookie contains the new claims and all authorization policies can inspect them without a database round-trip.

The on-demand DB lookup alternative — reading the user record inside middleware or a handler on every request — provides fresher data (useful when `Tier` can change while the user is logged in) but adds a database call per request. A middle path is to keep the claim in the cookie but call `SignInManager.RefreshSignInAsync(user)` whenever the property changes, which issues a new cookie with the updated claim without requiring the user to log out.

---

## CQ8. PBKDF2 Iteration Count and Hash Migration on Next Login
> Topics: [02 — ASP.NET Core Identity Setup & User Management], [04 — Password Security, Lockout & Data Protection]

**Concepts**
- ASP.NET Core Identity V3 hashing uses PBKDF2-SHA256 with a configurable iteration count (default 100,000 in .NET 10)
- `PasswordHasherOptions.IterationCount` sets the count; `CompatibilityMode` switches between V2 (10,000 iterations, SHA-1) and V3
- `IPasswordHasher<TUser>.VerifyHashedPassword` returns `PasswordVerificationResult.SuccessRehashNeeded` when the stored hash was produced with outdated parameters
- `SignInManager.PasswordSignInAsync` detects `SuccessRehashNeeded` and automatically re-hashes the password with the current settings before updating the stored hash — transparent to the user
- The legacy-system gotcha: bcrypt or Argon2 hashes imported from another system are not understood by the built-in hasher; a custom `IPasswordHasher<TUser>` must handle both formats during the migration window

**Answer**

ASP.NET Core Identity hashes passwords using PBKDF2 with HMAC-SHA256 (format version 3 by default). The iteration count — how many rounds of the key-derivation function are applied — trades login latency for brute-force resistance. In .NET 10 the default is 100,000 iterations, and you can raise it further:

```csharp
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    options.IterationCount    = 350_000;
});
```

When `VerifyHashedPassword` is called at login, it decodes the hash header to find the iteration count used when the hash was created. If it is lower than the current `IterationCount`, the method returns `PasswordVerificationResult.SuccessRehashNeeded` instead of `PasswordVerificationResult.Success`. The `PasswordSignInAsync` implementation inside `SignInManager` checks for this result and silently updates the stored hash to the new count — migration is gradual and transparent: old hashes are upgraded one login at a time with no downtime or forced password reset.

The critical gap arises when migrating from a system that used bcrypt, Argon2, or a custom scheme. The built-in hasher does not understand those formats and returns `Failed` for every bcrypt hash presented. The solution is a composite `IPasswordHasher<TUser>`:

```csharp
public class MigratingPasswordHasher(
    PasswordHasher<ApplicationUser> modern) : IPasswordHasher<ApplicationUser>
{
    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user, string hashedPassword, string providedPassword)
    {
        if (hashedPassword.StartsWith("$2"))   // bcrypt sentinel
        {
            bool ok = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            return ok ? PasswordVerificationResult.SuccessRehashNeeded
                      : PasswordVerificationResult.Failed;
        }
        return modern.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }

    public string HashPassword(ApplicationUser user, string password)
        => modern.HashPassword(user, password);
}
```

Returning `SuccessRehashNeeded` for a bcrypt match causes `SignInManager` to immediately re-hash the password with PBKDF2, so legacy hashes are replaced one login at a time without a forced password reset. Register the composite hasher with `builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, MigratingPasswordHasher>()`.

---

## CQ9. How the Auth Cookie Is Encrypted and the Distributed Key-Ring Gotcha
> Topics: [03 — Registration, Login & Cookie Authentication], [04 — Password Security, Lockout & Data Protection]

**Concepts**
- The ASP.NET Core Data Protection API protects the auth cookie using AES-256-CBC encryption + HMAC-SHA256 authentication
- By default each application instance generates its own key ring stored in a local per-machine directory or in memory
- In a scale-out deployment (App Service scale-out, Kubernetes), Instance A's key ring differs from Instance B's — cookies issued by A cannot be decrypted by B, causing 403 errors or redirect loops for load-balanced users
- Solution: shared key storage (`PersistKeysToAzureBlobStorage`, `PersistKeysToStackExchangeRedis`, or `PersistKeysToDbContext`) + shared key encryption (`ProtectKeysWithAzureKeyVault` or a certificate)
- `SetApplicationName` must be identical across all instances; mismatched app names produce keys that cannot be used interchangeably even when stored in the same location

**Answer**

When ASP.NET Core serializes a `ClaimsPrincipal` into an auth cookie, it uses the Data Protection API rather than a raw encryption call. Data Protection applies AES-256-CBC to encrypt the cookie payload and HMAC-SHA256 to authenticate it, using a key from a managed key ring. The key ring rotates automatically — a new key every 90 days by default, with old keys retained for decryption until they age out. In development, the key ring lives in `%LOCALAPPDATA%\ASP.NET\DataProtection-Keys` and everything works because one instance holds all keys.

The problem surfaces in production when the application scales out. Each App Service instance, Kubernetes pod, or container starts with an independent local key ring. A user authenticated by Instance A receives a cookie encrypted under Instance A's key. The next request, routed to Instance B by the load balancer, finds the cookie unreadable — Instance B does not have Instance A's key — and redirects the user to the login page. This is intermittent, difficult to reproduce in single-instance staging, and often misdiagnosed as a session configuration issue.

The fix is a shared key ring with at-rest encryption:

```csharp
// Program.cs — shared key ring for multi-instance deployments
builder.Services.AddDataProtection()
    .SetApplicationName("MyApp")
    .PersistKeysToAzureBlobStorage(
        new Uri(builder.Configuration["DataProtection:BlobUri"]!),
        new DefaultAzureCredential())
    .ProtectKeysWithAzureKeyVault(
        new Uri(builder.Configuration["DataProtection:KeyVaultKeyId"]!),
        new DefaultAzureCredential());
```

`SetApplicationName` is important: two applications sharing the same Blob container but with different names produce keys in separate namespaces — each app can only read its own cookies. Without `ProtectKeysWithAzureKeyVault` (or a certificate equivalent), the key XML files sit in plaintext in Blob Storage. Redis (`PersistKeysToStackExchangeRedis`) and SQL Server (`PersistKeysToDbContext<T>`) are alternatives to Azure Blob when those infrastructure dependencies are preferred.

---

## CQ10. Authorization Code Flow with PKCE for SPAs and Why Implicit Flow Is Deprecated
> Topics: [06 — OAuth 2.0 & OpenID Connect Fundamentals], [08 — JWT Bearer Tokens & Securing Web APIs]

**Concepts**
- Authorization code flow: the client receives a short-lived `code` in the redirect URI, then exchanges it for tokens in a back-channel POST — tokens never appear in the URL
- PKCE (Proof Key for Code Exchange): the client generates a random `code_verifier`, sends `code_challenge = BASE64URL(SHA256(code_verifier))` in the authorization request, and sends the raw `code_verifier` in the token exchange; the server verifies the relationship, binding the exchange to the originating client
- Implicit flow (deprecated): the `access_token` is returned directly in the URL fragment, which appears in browser history, server access logs, and `Referer` headers sent to third-party scripts
- ASP.NET Core `AddOpenIdConnect` sends PKCE automatically when `ResponseType = "code"` (the default in .NET 10); `UsePkce = true` is the explicit opt-in flag
- For browser-only SPAs without a .NET backend, MSAL.js and similar standards-compliant OIDC libraries handle PKCE natively; the BFF pattern is the highest-security alternative

**Answer**

The implicit flow was designed for public JavaScript clients that could not securely store a `client_secret`. The authorization server returned the access token directly in the redirect URI's fragment (`#access_token=…`), skipping the back-channel code exchange. The problem is that anything in the URL can leak: browsers store the full URL in history, include it in `Referer` headers sent to third-party analytics scripts, and write it to server access logs. An access token in the URL has a high exfiltration surface even if the user never copies it.

Authorization code flow with PKCE eliminates this exposure for public clients without requiring a client secret. The SPA generates a cryptographically random `code_verifier` (43–128 characters), computes `code_challenge = BASE64URL(SHA256(code_verifier))`, and sends only the challenge in the authorization request. The authorization server returns a short-lived, single-use authorization code in the redirect URI. The SPA then POSTs the code plus the original `code_verifier` to the token endpoint. The server re-hashes the verifier and compares it to the stored challenge — if they match, tokens are issued. An attacker who intercepts the authorization code cannot exchange it without knowing the verifier.

```csharp
// Program.cs — AddOpenIdConnect uses PKCE by default for code flow
builder.Services.AddAuthentication()
    .AddOpenIdConnect("oidc", options =>
    {
        options.ResponseType = "code";   // authorization code flow
        options.UsePkce      = true;     // default; explicit for clarity
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("api");
        options.SaveTokens = true;
    });
```

The OAuth 2.1 draft formally removes implicit flow and requires PKCE for all authorization code requests, including confidential clients. Migrating an existing SPA from implicit to PKCE-code typically involves changing `response_type` from `token` to `code` and adding the PKCE parameters — most modern authorization servers support both during the transition period. For ASP.NET Core server-rendered applications, `AddOpenIdConnect` handles the entire PKCE ceremony automatically with no additional code.

---

## CQ11. Multi-Tenant Azure Entra ID — Validating the tid Claim and the Common-Endpoint Issuer Gotcha
> Topics: [07 — External Logins & OIDC Client Integration], [12 — Federated Identity & Advanced Auth Scenarios]

**Concepts**
- The `/common` endpoint accepts users from any Entra ID tenant; each token's `iss` claim is tenant-specific: `https://login.microsoftonline.com/{tenantId}/v2.0`
- `TokenValidationParameters.ValidateIssuer = true` with a single hard-coded issuer rejects tokens from all other tenants — which is effectively all users for a multi-tenant app
- Setting `ValidateIssuer = false` without additional checks allows any Entra tenant in the world to authenticate into the application
- Correct approach: `ValidateIssuer = false` plus an `OnTokenValidated` handler that checks the `tid` claim against a configured allowed-tenant list
- `Microsoft.Identity.Web` (`AddMicrosoftIdentityWebAppAuthentication`) encapsulates this pattern behind an `AllowedTenants` configuration property

**Answer**

Single-tenant Entra ID applications use a tenant-specific endpoint (`https://login.microsoftonline.com/{tenantId}/v2.0`). Tokens carry an `iss` claim that matches that endpoint, so `ValidateIssuer = true` with the matching value works exactly as expected. Multi-tenant applications use the `/common` endpoint so that users from any Entra tenant can sign in. The complication is that each Entra tenant issues tokens with a unique `iss` value: `https://login.microsoftonline.com/72f988bf-xxxx/v2.0` for one tenant, a different GUID for every other tenant.

Setting `ValidateIssuer = true` with a single string causes the JWT middleware to reject tokens from every tenant whose GUID differs from the one hard-coded in configuration — which is all users for a multi-tenant app. The naive fix is `ValidateIssuer = false`, but without additional validation that accepts any Entra tenant in the world, including directories belonging to unrelated organizations.

The correct pattern is `ValidateIssuer = false` combined with an explicit `tid` claim check:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/common/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer   = false,
            ValidateAudience = true,
            ValidAudience    = builder.Configuration["AzureAd:ClientId"]
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                var tid = ctx.Principal!.FindFirstValue("tid");
                var allowed = builder.Configuration
                    .GetSection("AzureAd:AllowedTenants")
                    .Get<string[]>()!;
                if (!allowed.Contains(tid))
                    ctx.Fail("Tenant not authorized.");
                return Task.CompletedTask;
            }
        };
    });
```

`Microsoft.Identity.Web` wraps this pattern behind `AddMicrosoftIdentityWebAppAuthentication` with an `AllowedTenants` list in `appsettings.json`, which is the preferred approach for new projects. Note also that the `/common` metadata document does not serve a fixed JWKS — signing key retrieval happens per-token by reading the `tid` from the token header and constructing a tenant-specific key endpoint, so key caching behavior differs from single-tenant deployments.

---

## CQ12. Role Checks with Cookie Auth vs JWT — Why [Authorize(Roles)] Can Silently Fail
> Topics: [01 — Authentication & Authorization Fundamentals], [05 — Claims, Roles & Policy-Based Authorization], [08 — JWT Bearer Tokens & Securing Web APIs]

**Concepts**
- `[Authorize(Roles = "Admin")]` internally calls `ClaimsPrincipal.IsInRole`, which searches for a claim whose type equals `ClaimsIdentity.RoleClaimType`
- Cookie auth (via `SignInManager`): roles are stored under `ClaimTypes.Role` (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`), the default `RoleClaimType` — role checks work without configuration
- JWT auth with `MapInboundClaims = false` (the .NET 10 default): the `"roles"` claim stays as the short string `"roles"`, but `RoleClaimType` still defaults to the long URI — the type comparison fails silently, producing a 403 with no error message
- Fix: set `TokenValidationParameters.RoleClaimType = "roles"` to match the actual claim type in the JWT
- Alternative: `MapInboundClaims = true` renames `"roles"` to the long URI, but also renames `"sub"`, `"iss"`, and other short-name claims, breaking any code that references them by short name

**Answer**

`[Authorize(Roles = "Admin")]` is syntactic sugar that translates to a policy checking `ClaimsPrincipal.IsInRole("Admin")`. Internally, `IsInRole` iterates the `ClaimsIdentity` objects on the principal, searching for a claim whose `Type` equals `identity.RoleClaimType` and whose `Value` equals `"Admin"`. The gotcha is that `RoleClaimType` is set when the identity is constructed and its default value is the long URI `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`.

When you sign in with cookie authentication, `SignInManager.SignInWithClaimsAsync` serializes role claims under exactly that long-form type. `IsInRole("Admin")` finds the claim, and `[Authorize(Roles = "Admin")]` succeeds. When you validate a JWT bearer token in .NET 10, however, `MapInboundClaims` defaults to `false`, so the `"roles"` claim from the token payload remains as the short string `"roles"`. The `ClaimsIdentity` created by the JWT middleware uses `ClaimTypes.Role` (the long URI) as its `RoleClaimType`, so `IsInRole` searches under the long URI, finds nothing, and the authorization silently fails with a 403 — no exception, no log entry explaining the type mismatch.

```csharp
// Fix: align RoleClaimType with the actual claim name in the JWT
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType    = "roles",
            NameClaimType    = "name",
            ValidateIssuer   = true,
            ValidIssuer      = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience    = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = /* ... */
        };
    });
```

An equally valid fix is to emit the role claim under the long URI at token-issuance time, so the JWT payload already uses `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` as the claim type. Either approach works as long as `RoleClaimType` and the claim type in the token agree. Choose one canonical form across the codebase and enforce it, because mismatches are invisible at startup and only manifest as silent runtime 403 responses.

---

## CQ13. First-Time 2FA Setup — TOTP Secret, QR Code, Confirmation, and Recovery Codes
> Topics: [02 — ASP.NET Core Identity Setup & User Management], [09 — Two-Factor Authentication & Identity Extensibility]

**Concepts**
- `UserManager.GetAuthenticatorKeyAsync` returns the user's TOTP shared secret (Base32-encoded), generating and persisting a new one if none exists yet
- The secret is formatted as an `otpauth://totp/{issuer}:{email}?secret={base32secret}&issuer={issuer}` URI and rendered as a QR code client-side
- `UserManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code)` confirms the 6-digit code before 2FA is enabled
- `UserManager.SetTwoFactorEnabledAsync(user, true)` enables the feature after successful verification
- `UserManager.GenerateNewTwoFactorRecoveryCodesAsync` produces single-use recovery codes hashed and stored in `AspNetUserTokens`; calling it again immediately invalidates all previously issued codes

**Answer**

Setting up TOTP-based 2FA for the first time involves a small ceremony: generate a shared secret, let the user scan it into an authenticator app, confirm the app is producing valid codes, enable 2FA, and then issue recovery codes. ASP.NET Core Identity provides all the building blocks.

`GetAuthenticatorKeyAsync` returns the user's existing TOTP secret or generates and persists a new one. The secret arrives as a Base32 string. Authenticator apps such as Google Authenticator and Microsoft Authenticator expect an `otpauth://totp/` URI, which you build server-side and render as a QR code client-side:

```csharp
var key = await _userManager.GetAuthenticatorKeyAsync(user);
if (key is null)
{
    await _userManager.ResetAuthenticatorKeyAsync(user);
    key = await _userManager.GetAuthenticatorKeyAsync(user);
}

var uri = $"otpauth://totp/{Uri.EscapeDataString("MyApp")}:" +
          $"{Uri.EscapeDataString(user.Email!)}?" +
          $"secret={key}&issuer={Uri.EscapeDataString("MyApp")}&digits=6";
// Pass uri to the view; render as QR code using a JavaScript library
```

Before enabling 2FA, verify the code the user enters from their app to confirm they scanned the secret correctly:

```csharp
bool valid = await _userManager.VerifyTwoFactorTokenAsync(
    user, TokenOptions.DefaultAuthenticatorProvider, model.Code);

if (!valid)
{
    ModelState.AddModelError("Code", "Invalid verification code.");
    return Page();
}

await _userManager.SetTwoFactorEnabledAsync(user, true);
var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
// Display recoveryCodes once — they are hashed in the DB and cannot be retrieved later
```

Recovery codes are hashed single-use tokens stored in `AspNetUserTokens`. When a user redeems one via `TwoFactorRecoveryCodeSignInAsync`, it is consumed and cannot be reused. Calling `GenerateNewTwoFactorRecoveryCodesAsync` again issues a fresh batch and immediately invalidates all prior codes, so users who regenerate must be warned that any previously printed codes are now worthless. Always prompt users to save their recovery codes before leaving the setup page, as there is no mechanism to display them a second time.

---

## CQ14. JWT Storage — localStorage vs HttpOnly Cookie vs BFF Pattern
> Topics: [03 — Registration, Login & Cookie Authentication], [08 — JWT Bearer Tokens & Securing Web APIs], [10 — Web Application Security]

**Concepts**
- `localStorage` is readable by any JavaScript on the same origin — an XSS vulnerability can silently exfiltrate a stored JWT to an attacker-controlled server
- `HttpOnly` cookies are invisible to JavaScript — XSS cannot read the token — but cookie auto-attachment reintroduces CSRF risk; mitigate with `SameSite=Strict` or `SameSite=Lax` plus antiforgery tokens
- `SameSite=Strict` prevents the cookie from being sent on any cross-site request (including top-level navigations); `SameSite=Lax` allows safe top-level GET navigations; `SameSite=None` requires `Secure` and is needed for legitimate cross-site embedding
- The BFF (Backend-for-Frontend) pattern: the SPA authenticates against its own same-origin backend, which stores tokens server-side and uses an `HttpOnly` session cookie for the SPA; the BFF proxies API calls attaching the bearer token server-side — the token never reaches the browser
- BFF is the recommended architecture for SPAs handling sensitive data; YARP and Duende BFF are common .NET 10 implementation choices

**Answer**

Where to store a JWT in a browser application is fundamentally a choice between XSS risk and CSRF risk. `localStorage` is convenient — JavaScript can read it and attach it in `Authorization` headers on every `fetch` call — but it is fully accessible to any script running on the page, including scripts injected via an XSS vulnerability. A single XSS vector can drain the stored token to an external server, achieving the same effect as stealing a password. The token remains valid until expiry regardless of what the victim does.

Storing the JWT in an `HttpOnly` cookie removes it from JavaScript's reach entirely. XSS can no longer read the token. The trade-off is that the browser attaches the cookie automatically, reintroducing CSRF exposure. The `SameSite` attribute is the primary mitigation: `SameSite=Strict` causes the browser to omit the cookie from all cross-site requests, making CSRF essentially impossible. `SameSite=Lax` (the modern browser default) allows safe top-level GET navigations but blocks cross-site POST requests — sufficient for most CSRF scenarios. Applications using `SameSite=None` for legitimate cross-site embedding still need antiforgery token validation.

The BFF pattern eliminates the JWT from the browser entirely. The SPA authenticates against a same-origin backend that stores OAuth tokens in a server-side session. The SPA authenticates with that backend using a short-lived `HttpOnly` session cookie. When the SPA needs data from a downstream API, it calls a BFF endpoint, which attaches the stored bearer token server-side before forwarding the request. The browser never holds a raw JWT. An XSS attacker can call BFF endpoints using the session cookie, but they cannot export the token to replay it elsewhere — the blast radius is bounded to the current authenticated session. For .NET 10, YARP with cookie-authenticated reverse-proxy routes or the Duende BFF library are the most practical implementations.

---

## CQ15. Refresh Token Rotation in ASP.NET Core Identity
> Topics: [06 — OAuth 2.0 & OpenID Connect Fundamentals], [08 — JWT Bearer Tokens & Securing Web APIs], [02 — ASP.NET Core Identity Setup & User Management]

**Concepts**
- Refresh tokens are long-lived credentials; in ASP.NET Core Identity they are stored in the `AspNetUserTokens` table via `UserManager.SetAuthenticationTokenAsync`
- Single-use rotation: on every use the old refresh token is deleted and a new one is issued atomically; a stolen token that is used by an attacker triggers the rotation and immediately invalidates the original
- Reuse detection: a client presenting a refresh token that no longer exists in the database signals a possible compromise — the correct response is to revoke all refresh tokens for that user
- The delete-before-issue order is critical; issuing the new token before deleting the old one creates a window where two concurrent requests can each claim the same refresh token
- `RemoveAuthenticationTokenAsync` followed by `SetAuthenticationTokenAsync` implements rotation; wrap in a serialized operation to prevent races

**Answer**

A refresh token is a long-lived credential that lets a client obtain new access tokens without re-authenticating the user. Because it is long-lived, its compromise is more damaging than leaking a short-lived access token. Refresh token rotation limits this damage: each use produces a new refresh token and the consumed one is immediately invalidated. An attacker who steals a refresh token and uses it triggers the rotation, making the victim's copy invalid — the first party to notice they can no longer refresh is the signal that a compromise occurred.

In ASP.NET Core Identity, refresh tokens are stored in `AspNetUserTokens` via `UserManager.SetAuthenticationTokenAsync`. The rotation flow is:

```csharp
public async Task<TokenResponse> RotateRefreshTokenAsync(
    string incoming, ApplicationUser user)
{
    var stored = await _userManager.GetAuthenticationTokenAsync(
        user, "MyApp", "RefreshToken");

    if (stored is null || stored != incoming)
    {
        // Token already consumed or never existed — treat as compromise
        await _userManager.RemoveAuthenticationTokenAsync(
            user, "MyApp", "RefreshToken");   // revoke all for this user
        throw new SecurityTokenException("Refresh token reuse detected.");
    }

    // Delete first, then issue — prevents two concurrent callers from both succeeding
    await _userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshToken");

    var newRefresh = GenerateSecureToken();   // cryptographically random, opaque
    await _userManager.SetAuthenticationTokenAsync(
        user, "MyApp", "RefreshToken", newRefresh);

    var newAccess = _jwtService.Issue(user);
    return new TokenResponse(newAccess, newRefresh);
}
```

The delete-before-issue order is intentional. If the new token is stored first and the response is lost before the old token is deleted, two valid refresh tokens coexist — exactly what rotation prevents. Detecting reuse (incoming token not found) should revoke all tokens for the affected user, forcing re-authentication. For high-security scenarios, storing a token family ID allows tracing and fully revoking all branches of a reuse chain across multiple devices.

---

## CQ16. Resource-Based Authorization — IAuthorizationService vs the [Authorize] Attribute
> Topics: [01 — Authentication & Authorization Fundamentals], [05 — Claims, Roles & Policy-Based Authorization]

**Concepts**
- `[Authorize(Policy = "...")]` runs as middleware before the action executes — the resource has not yet been loaded from the database
- Resource-based authorization requires the actual resource instance to make an access decision (e.g., is this user the owner of *this specific* document?)
- `IAuthorizationService.AuthorizeAsync(User, resource, requirement)` is called inside the action body after loading the resource
- `AuthorizationHandler<TRequirement, TResource>` receives both the `AuthorizationHandlerContext` and the typed resource instance, enabling per-object ownership rules
- The two mechanisms are complementary: `[Authorize]` guards the endpoint (authentication + broad policy); `IAuthorizationService` guards the specific resource instance (ownership, state-based rules)

**Answer**

`[Authorize]` attributes evaluate before the action method executes. This is correct for broad gate checks — is the user authenticated? Do they hold the `Editor` role? Does the `SubscriptionActive` policy pass? — because those checks require no data from the database. The attribute cannot answer "can this user edit this specific document?" because the document has not been loaded yet when the attribute evaluates.

Resource-based authorization moves the check inside the action, after the resource is retrieved:

```csharp
[HttpGet("{id}")]
[Authorize]   // ensures the user is authenticated before touching the DB
public async Task<IActionResult> Edit(int id)
{
    var doc = await _db.Documents.FindAsync(id);
    if (doc is null) return NotFound();

    var result = await _authService.AuthorizeAsync(User, doc, "DocumentEditPolicy");
    if (!result.Succeeded) return Forbid();

    return View(doc);
}
```

The `"DocumentEditPolicy"` is backed by a typed handler:

```csharp
public class DocumentEditHandler
    : AuthorizationHandler<DocumentEditRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DocumentEditRequirement requirement,
        Document resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (resource.OwnerId == userId || context.User.IsInRole("Admin"))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```

Register the handler: `builder.Services.AddSingleton<IAuthorizationHandler, DocumentEditHandler>()`. The `[Authorize]` attribute on the action still runs first, rejecting unauthenticated requests before any database call, keeping the pipeline efficient. `IAuthorizationService` then makes the fine-grained per-instance ownership decision. Neither mechanism replaces the other — applying only `[Authorize]` without resource-based checks means any authenticated user can access any document by guessing its ID; applying only `IAuthorizationService` without `[Authorize]` means unauthenticated callers reach the database before being rejected.

---

## CQ17. Lockout and External Provider Logins — The Unified Lockout Gap
> Topics: [02 — ASP.NET Core Identity Setup & User Management], [04 — Password Security, Lockout & Data Protection], [07 — External Logins & OIDC Client Integration]

**Concepts**
- `PasswordSignInAsync` calls `UserManager.AccessFailedAsync` on a failed password attempt, incrementing the lockout counter
- `ExternalLoginSignInAsync` does not involve a password check and never calls `AccessFailedAsync` — the lockout counter is entirely bypassed for external-provider logins
- A user who authenticates via Google, Microsoft, or any external provider is structurally immune to the built-in password lockout mechanism
- The gap: repeated failed TOTP attempts following an external login do not trigger lockout; suspicious external-login patterns leave no trace in the lockout state
- Mitigation: manually call `AccessFailedAsync` in the external login callback and in the 2FA challenge handler; or override `SignInManager<TUser>.ExternalLoginSignInAsync` to add custom suspicious-activity tracking

**Answer**

ASP.NET Core Identity's lockout mechanism is tightly coupled to the password sign-in path. When a user enters a wrong password, `PasswordSignInAsync` calls `UserManager.AccessFailedAsync`, which increments a counter in `AspNetUsers`. Once the counter reaches `MaxFailedAccessAttempts`, `IsLockedOutAsync` returns `true` and subsequent logins are rejected until the lockout period expires. This is a solid defence against credential-stuffing and brute-force attacks on password-based accounts.

`ExternalLoginSignInAsync` takes a different path entirely. It locates the user by the external provider's `(provider, providerKey)` pair and calls `SignInOrTwoFactorAsync` internally — there is no credential to validate, so `AccessFailedAsync` is never invoked. The lockout counter stays at zero regardless of how many times the external login flow is attempted or how many times the subsequent 2FA challenge fails (see CQ3). An attacker who compromises a user's Google account and repeatedly attempts to log into your application faces no automated throttling from ASP.NET Core Identity.

Mitigation requires explicit calls at the external login callback:

```csharp
// ExternalLoginCallback action — after ExternalLoginSignInAsync
if (!result.Succeeded && !result.RequiresTwoFactor)
{
    var info = await _signInManager.GetExternalLoginInfoAsync();
    var user = await _userManager.FindByLoginAsync(
        info!.LoginProvider, info.ProviderKey);
    if (user is not null)
    {
        await _userManager.AccessFailedAsync(user);
        if (await _userManager.IsLockedOutAsync(user))
            return RedirectToPage("./Lockout");
    }
}
```

For broader monitoring — detecting patterns such as many failed logins before a successful one, or the same provider account attempting to log into multiple local accounts — override `SignInManager<TUser>.ExternalLoginSignInAsync` and publish security events to a logging or SIEM pipeline. A custom `IUserClaimsPrincipalFactory` can add audit-trail claims (e.g., `last_external_login_ip`) to the principal at login time, but it cannot enforce lockout on its own because it runs only after the sign-in decision has already been made.

---

## Cross-Reference Quick Map

| CQ | Primary Tension | Subtopics Involved |
|----|-----------------|-------------------|
| CQ1 | Cookie vs JWT token delivery | 03, 08 |
| CQ2 | Claims lost between OIDC → principal → bearer token | 05, 06, 07, 08 |
| CQ3 | 2FA failures bypass lockout counter | 02, 04, 09 |
| CQ4 | CSRF protection: cookie endpoints vs JWT APIs | 03, 08, 10 |
| CQ5 | Secret and signing key lifecycle in production | 07, 08, 11 |
| CQ6 | External claim name mismatch silently breaks policies | 05, 12 |
| CQ7 | Custom IdentityUser property invisible to ClaimsPrincipal | 02, 05 |
| CQ8 | PBKDF2 iteration count migration and legacy hash formats | 02, 04 |
| CQ9 | Distributed key-ring breaks auth cookies at scale-out | 03, 04 |
| CQ10 | PKCE replaces implicit flow for SPA token safety | 06, 08 |
| CQ11 | /common endpoint issuer validation breaks multi-tenant auth | 07, 12 |
| CQ12 | RoleClaimType mismatch causes silent [Authorize(Roles)] failure | 01, 05, 08 |
| CQ13 | TOTP 2FA setup ceremony and recovery code lifecycle | 02, 09 |
| CQ14 | JWT browser storage: XSS vs CSRF vs BFF trade-offs | 03, 08, 10 |
| CQ15 | Refresh token rotation and reuse-detection revocation | 02, 06, 08 |
| CQ16 | Resource-based authorization needs IAuthorizationService, not [Authorize] | 01, 05 |
| CQ17 | External provider logins bypass the password lockout counter | 02, 04, 07 |

---

# 07. ASP.NET Core Identity & Security — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [Authentication & Authorization Fundamentals](01.%20Authentication%20%26%20Authorization%20Fundamentals/INTERVIEW_QA.md) | Core AuthN vs AuthZ concepts, middleware pipeline, and scheme selection |
| 02 | [ASP.NET Core Identity - Setup & User Management](02.%20ASP.NET%20Core%20Identity%20-%20Setup%20%26%20User%20Management/INTERVIEW_QA.md) | UserManager, RoleManager, IdentityUser customization, and EF Core stores |
| 03 | [Registration, Login & Cookie Authentication](03.%20Registration%2C%20Login%20%26%20Cookie%20Authentication/INTERVIEW_QA.md) | Cookie issuance, SignInManager, sliding expiration, and cookie security flags |
| 04 | [Password Security, Lockout & Data Protection](04.%20Password%20Security%2C%20Lockout%20%26%20Data%20Protection/INTERVIEW_QA.md) | Password hashing, lockout policy, IPasswordHasher, and the Data Protection API |
| 05 | [Claims, Roles & Policy-Based Authorization](05.%20Claims%2C%20Roles%20%26%20Policy-Based%20Authorization/INTERVIEW_QA.md) | ClaimsPrincipal, role-based vs policy-based AuthZ, and IAuthorizationHandler |
| 06 | [OAuth 2.0 & OpenID Connect Fundamentals](06.%20OAuth%202.0%20%26%20OpenID%20Connect%20Fundamentals/INTERVIEW_QA.md) | OAuth grant types, OIDC ID token structure, and authorization server roles |
| 07 | [External Logins & OIDC Client Integration](07.%20External%20Logins%20%26%20OIDC%20Client%20Integration/INTERVIEW_QA.md) | AddOpenIdConnect, external login callbacks, claim mapping, and account linking |
| 08 | [JWT Bearer Tokens & Securing Web APIs](08.%20JWT%20Bearer%20Tokens%20%26%20Securing%20Web%20APIs/INTERVIEW_QA.md) | JWT structure, signing algorithms, TokenValidationParameters, and refresh tokens |
| 09 | [Two-Factor Authentication & Identity Extensibility](09.%20Two-Factor%20Authentication%20%26%20Identity%20Extensibility/INTERVIEW_QA.md) | TOTP, SMS 2FA, TwoFactorSignInAsync, and custom token providers |
| 10 | [Web Application Security](10.%20Web%20Application%20Security/INTERVIEW_QA.md) | CSRF, XSS, Content Security Policy, HSTS, and security headers |
| 11 | [Secrets, Certificates & Key Management](11.%20Secrets%2C%20Certificates%20%26%20Key%20Management/INTERVIEW_QA.md) | User Secrets, environment variables, Azure Key Vault, and key rotation |
| 12 | [Federated Identity & Advanced Auth Scenarios](12.%20Federated%20Identity%20%26%20Advanced%20Auth%20Scenarios/INTERVIEW_QA.md) | WS-Federation, SAML, multi-tenant auth, and claims transformation |

---

## Table of Contents
- [CQ1. Cookie Auth vs JWT — Token Delivery Models and the Hybrid SPA Pattern](#cq1-cookie-auth-vs-jwt--token-delivery-models-and-the-hybrid-spa-pattern)
- [CQ2. The Claims Pipeline from OIDC Identity Token to ClaimsPrincipal](#cq2-the-claims-pipeline-from-oidc-identity-token-to-claimsprincipal)
- [CQ3. How 2FA Interacts with Account Lockout](#cq3-how-2fa-interacts-with-account-lockout)
- [CQ4. CSRF Protection — Why Cookie Auth Needs Antiforgery Tokens but JWT Auth Does Not](#cq4-csrf-protection--why-cookie-auth-needs-antiforgery-tokens-but-jwt-auth-does-not)
- [CQ5. Storing Client Secrets and JWT Signing Keys Safely in Production](#cq5-storing-client-secrets-and-jwt-signing-keys-safely-in-production)
- [CQ6. Policy-Based Authorization Against External IdP Claims](#cq6-policy-based-authorization-against-external-idp-claims)
- [CQ7. Custom User Properties vs Claims — IUserClaimsPrincipalFactory vs On-Demand DB Lookup](#cq7-custom-user-properties-vs-claims--iuserclaimsprincipalfactory-vs-on-demand-db-lookup)
- [CQ8. PBKDF2 Iteration Count and Hash Migration on Next Login](#cq8-pbkdf2-iteration-count-and-hash-migration-on-next-login)
- [CQ9. How the Auth Cookie Is Encrypted and the Distributed Key-Ring Gotcha](#cq9-how-the-auth-cookie-is-encrypted-and-the-distributed-key-ring-gotcha)
- [CQ10. Authorization Code Flow with PKCE for SPAs and Why Implicit Flow Is Deprecated](#cq10-authorization-code-flow-with-pkce-for-spas-and-why-implicit-flow-is-deprecated)
- [CQ11. Multi-Tenant Azure Entra ID — Validating the tid Claim and the Common-Endpoint Issuer Gotcha](#cq11-multi-tenant-azure-entra-id--validating-the-tid-claim-and-the-common-endpoint-issuer-gotcha)
- [CQ12. Role Checks with Cookie Auth vs JWT — Why \[Authorize(Roles)\] Can Silently Fail](#cq12-role-checks-with-cookie-auth-vs-jwt--why-authorizeroles-can-silently-fail)
- [CQ13. First-Time 2FA Setup — TOTP Secret, QR Code, Confirmation, and Recovery Codes](#cq13-first-time-2fa-setup--totp-secret-qr-code-confirmation-and-recovery-codes)
- [CQ14. JWT Storage — localStorage vs HttpOnly Cookie vs BFF Pattern](#cq14-jwt-storage--localstorage-vs-httponly-cookie-vs-bff-pattern)
- [CQ15. Refresh Token Rotation in ASP.NET Core Identity](#cq15-refresh-token-rotation-in-aspnet-core-identity)
- [CQ16. Resource-Based Authorization — IAuthorizationService vs the \[Authorize\] Attribute](#cq16-resource-based-authorization--iauthorizationservice-vs-the-authorize-attribute)
- [CQ17. Lockout and External Provider Logins — The Unified Lockout Gap](#cq17-lockout-and-external-provider-logins--the-unified-lockout-gap)

---

> These questions cut across two or more of the twelve subtopics above.
> Each question identifies its source topics in the header line.
> For deep-dive questions on a single topic, open that subfolder's `INTERVIEW_QA.md`.

---

## CQ1. Cookie Auth vs JWT — Token Delivery Models and the Hybrid SPA Pattern
> Topics: [03 — Cookie Authentication], [08 — JWT Bearer Tokens]

**Concepts**
- The browser attaches an authentication cookie automatically on every same-origin request, without any JavaScript involvement
- A JWT bearer token must be set explicitly in the `Authorization: Bearer` header by application code — the browser never sends it automatically
- Cookie auth is vulnerable to CSRF because automatic attachment means cross-site pages can trigger authenticated requests without reading the cookie
- JWT auth is vulnerable to XSS if the token is stored in `localStorage`, where malicious script can read and exfiltrate it; storing the JWT in an `HttpOnly` cookie eliminates that vector but reintroduces CSRF territory
- The hybrid SPA pattern: browser-rendered pages authenticate with a cookie; the same application's public API accepts JWT bearer tokens, giving each surface its natural token type

**Answer**
Cookie authentication and JWT bearer authentication differ most fundamentally in how the credential travels to the server. When a user logs in with cookie auth, the browser receives a `Set-Cookie` response and then attaches that cookie header to every matching request thereafter — automatically, with no JavaScript involvement. This convenience is also the attack surface: a malicious page on any origin can trigger a state-changing request to your site, and the browser will include the cookie. That is Cross-Site Request Forgery. ASP.NET Core counters it with the synchronizer token pattern: `[ValidateAntiForgeryToken]` compares a hidden form field value against a cookie value, and a cross-site page cannot read the cookie to reproduce the field value.

JWT bearer tokens work differently. The `Authorization: Bearer <token>` header is never sent automatically by the browser — JavaScript must attach it. This makes CSRF impossible for JWT-protected APIs, because a cross-site HTML form has no mechanism to set custom request headers. The risk shifts to XSS: if the JWT is stored in `localStorage`, any injected script can read and exfiltrate it. The mitigation is to store the JWT in an `HttpOnly` cookie — invisible to script — but that circles back to CSRF, so the API also needs anti-forgery protection or must rely on the `SameSite` cookie attribute.

The hybrid SPA pattern resolves the tension by assigning each token type to its natural surface. The browser application (Razor Pages, Blazor, or an MVC front end) authenticates with a cookie because the browser handles that efficiently and CSRF protection via `[ValidateAntiForgeryToken]` is straightforward. A separate public API — consumed by mobile apps, third-party integrations, or the same SPA using `fetch` — accepts only JWT bearer tokens, because those callers cannot share a cookie jar. In .NET 10, `AddAuthentication` supports multiple schemes simultaneously; `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` or a named policy selects the active scheme per endpoint.

---

## CQ2. The Claims Pipeline from OIDC Identity Token to ClaimsPrincipal
> Topics: [05 — Claims & Policy Authorization], [06 — OAuth 2.0 & OIDC], [07 — External Logins & OIDC Client], [08 — JWT Bearer Tokens]

**Concepts**
- An OIDC identity token (`id_token`) is a JWT issued by the authorization server carrying the user's identity claims (`sub`, `email`, `name`, custom attributes)
- `AddOpenIdConnect` validates the `id_token` signature, issuer, and audience, then runs a `ClaimActions` pipeline to map token claims into the local `ClaimsPrincipal`
- `MapUniqueJsonKey` copies standard claims but `DeleteClaim` strips any source claim whose JSON key collides with a known ASP.NET mapping — the raw `sub` claim is remapped to the long URI `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` and then removed
- Claims absent from the principal after mapping are absent from any downstream JWT the application re-issues; they do not appear automatically
- The fix: call `options.ClaimActions.MapJsonKey("sub", "sub")` explicitly in `AddOpenIdConnect`, or look up missing claims via `UserManager.GetClaimsAsync` at token-issuance time

**Answer**
When a user authenticates via an external OIDC provider — Azure AD, Google, Auth0, Okta — the authorization server issues an `id_token` JWT that carries identity claims such as `sub`, `email`, `name`, and any custom attributes configured on the provider. ASP.NET Core's `AddOpenIdConnect` middleware validates this token and then runs a `ClaimActions` pipeline that selects which claims to copy into the in-process `ClaimsPrincipal`. The default mapping calls `MapUniqueJsonKey` for a fixed list of well-known claims, but it also runs `DeleteClaim` for any raw JSON key it remaps to a long-form URI. The `sub` claim, for example, is remapped to `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` and the original short name is removed. A developer who calls `User.FindFirst("sub")` inside the application will find nothing, even though the claim arrived in the token.

```csharp
// Program.cs — preserve short claim names alongside the mapped URI
builder.Services.AddAuthentication()
    .AddOpenIdConnect(options =>
    {
        options.ClaimActions.MapJsonKey("sub", "sub");
        options.ClaimActions.MapJsonKey("email", "email");
        options.ClaimActions.MapJsonKey("department", "extension_department"); // custom AAD attribute
    });
```

Once the mapped `ClaimsPrincipal` is stored in the cookie, the token-issuance endpoint reads claims from `HttpContext.User` or calls `UserManager.GetClaimsAsync` to hydrate any claims that are stored in the Identity database rather than the principal. Only the claims that reach this point flow into the JWT bearer token sent to the API. If a required claim is missing there, it will not appear in the bearer token, and any policy or resource filter that checks for it will silently fail. Auditing the full pipeline — IdP token → `ClaimActions` → cookie principal → `UserManager` claims → JWT payload — is the right debugging strategy when authorization behaves unexpectedly after adding a new external provider.

---

## CQ3. How 2FA Interacts with Account Lockout
> Topics: [02 — Identity Setup], [04 — Password Security & Lockout], [09 — Two-Factor Authentication]

**Concepts**
- `IdentityOptions.Lockout` configures `MaxFailedAccessAttempts`, `DefaultLockoutTimeSpan`, and `AllowedForNewUsers`
- `PasswordSignInAsync` increments the failure counter on a bad password (when `lockoutOnFailure: true`) and resets it on a correct password
- After a correct password with 2FA enabled, `PasswordSignInAsync` returns `SignInResult.TwoFactorRequired` — the failure counter is neither incremented nor reset at this point
- `TwoFactorSignInAsync` does **not** call `UserManager.AccessFailedAsync` internally — repeated 2FA failures do not trigger lockout by default
- The security gap: an attacker with a stolen valid password can brute-force the TOTP window indefinitely without hitting the lockout threshold

**Answer**
Account lockout in ASP.NET Core Identity counts consecutive failed authentication attempts and temporarily disables the account after reaching the configured threshold (`MaxFailedAccessAttempts`). The built-in `PasswordSignInAsync` handles this automatically: every failed password check calls `UserManager.AccessFailedAsync` to increment the counter (when `lockoutOnFailure` is `true`), and a successful password check calls `UserManager.ResetAccessFailedCountAsync` to clear it. The complication arises at the 2FA boundary.

When a user enters the correct password but 2FA is enabled, `PasswordSignInAsync` returns `SignInResult.TwoFactorRequired` without touching the lockout counter. The password was valid, so no failure is recorded, and no reset is issued either. The user is redirected to the 2FA challenge page, where `TwoFactorSignInAsync` validates the one-time code. This method does not call `AccessFailedAsync` on failure — the counter stays wherever it was when the password succeeded, and repeated bad 2FA codes leave no trace in the lockout state.

The mitigation is to add explicit lockout logic in the 2FA controller action:

```csharp
var result = await _signInManager.TwoFactorSignInAsync(
    provider, code, isPersistent: false, rememberClient: false);

if (!result.Succeeded)
{
    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
    if (user is not null)
    {
        await _userManager.AccessFailedAsync(user);
        if (await _userManager.IsLockedOutAsync(user))
            return RedirectToPage("./Lockout");
    }
    ModelState.AddModelError(string.Empty, "Invalid verification code.");
}
```

With this in place, lockout protects both authentication stages. Configure `LockoutOptions` in `AddIdentity` to tune the threshold and duration; set `AllowedForNewUsers = true` if you want new accounts to be lockout-eligible from the moment of registration.

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers      = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
```

---

## CQ4. CSRF Protection — Why Cookie Auth Needs Antiforgery Tokens but JWT Auth Does Not
> Topics: [03 — Cookie Authentication], [08 — JWT Bearer Tokens], [10 — Web Application Security]

**Concepts**
- CSRF exploits the browser's automatic cookie attachment to forge authenticated requests from a page on a different origin
- The synchronizer token pattern embeds a random hidden field token (tied to the Data Protection key ring) that a cross-site page cannot read
- `[ValidateAntiForgeryToken]` compares the hidden form field value with the anti-forgery cookie value; a mismatch rejects the request with 400
- JWT bearer auth is structurally immune to CSRF — the `Authorization` header is never sent automatically by the browser
- The double-submit cookie pattern is a stateless CSRF mitigation: a readable (non-`HttpOnly`) cookie holds the CSRF token; the client must also send it in a header; the server verifies both values match

**Answer**
Cross-Site Request Forgery attacks work because browsers attach cookies to every matching request regardless of which page initiated the request. A malicious page on `evil.com` can embed a form that POSTs to `mybank.com/transfer`, and the browser will include the `mybank.com` authentication cookie — the server sees an authenticated request it never intended to allow. ASP.NET Core defends against this using the synchronizer token pattern. A cryptographically random anti-forgery token is embedded in the form as a hidden field (`__RequestVerificationToken`) and stored separately in a cookie. When the form is submitted, `[ValidateAntiForgeryToken]` (or the automatic validation built into Razor Pages) verifies that both tokens match. Because same-origin policy prevents a cross-site script from reading the cookie, `evil.com` cannot replicate the hidden field value, and the forged request is rejected.

JWT-authenticated APIs do not need this protection. The `Authorization: Bearer <token>` header is never sent automatically by the browser — application JavaScript must explicitly include it on each request. An HTML form on `evil.com` cannot set custom request headers, so a cross-site form submission to your API will arrive without an `Authorization` header and be rejected as unauthenticated. This is a structural property of JWT bearer auth, not a configuration choice.

The double-submit cookie pattern is a useful alternative when server-side session state is not available. A CSRF token is placed in a readable (non-`HttpOnly`) cookie alongside the session cookie. The browser JavaScript reads this readable cookie and echoes it in a custom request header (such as `X-CSRF-Token`). The server compares the two. An attacker on `evil.com` cannot read the cookie via script (same-origin policy blocks it), so they cannot construct the matching header. The approach is stateless because the server only needs to compare two values rather than look up a stored token.

A quick reference for how the two protection models look in .NET 10:

```csharp
// Cookie-authenticated form endpoint — requires antiforgery token
[HttpPost, ValidateAntiForgeryToken]
public IActionResult Transfer([FromForm] TransferModel model) { /* ... */ }

// JWT-authenticated API endpoint — no antiforgery needed; CORS policy instead
[HttpPost("api/transfer")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public IActionResult ApiTransfer([FromBody] TransferModel model) { /* ... */ }
```

Razor Pages automatically validates the anti-forgery token on every POST handler — there is no `[ValidateAntiForgeryToken]` attribute needed there. For Minimal API endpoints with cookie auth, call `app.UseAntiforgery()` and annotate the route with `WithRequestTimeout` or `RequireAuthorization` as appropriate; the `IAntiforgery` service is available via DI for manual validation.

---

## CQ5. Storing Client Secrets and JWT Signing Keys Safely in Production
> Topics: [07 — External Logins & OIDC Client], [08 — JWT Bearer Tokens], [11 — Secrets, Certificates & Key Management]

**Concepts**
- `client_secret` values and symmetric JWT signing keys must never appear in `appsettings.json` committed to source control
- Development: `dotnet user-secrets` stores key-value pairs in a per-user file outside the project directory, outside any repository
- Production: environment variables injected by the hosting platform, or Azure Key Vault accessed via `AddAzureKeyVault` + Managed Identity (no credential needed to retrieve the credential)
- Prefer asymmetric RS256/ES256 for JWT signing — only the issuer holds the private key; consuming APIs verify tokens using the public key from the JWKS endpoint
- Key rotation gotcha: replacing the active signing key immediately invalidates all tokens signed with the old key, including long-lived refresh tokens; a key overlap window (publish new → keep accepting old for max token lifetime → retire old) prevents a hard logout for all users

**Answer**
Credentials such as OAuth `client_secret` values and symmetric JWT signing keys are high-value targets — a leaked `client_secret` lets an attacker impersonate your application to the authorization server; a leaked HS256 signing key lets an attacker forge arbitrary JWT bearer tokens. The first rule is to keep them out of source control entirely. In development, `dotnet user-secrets init` and `dotnet user-secrets set "Jwt:SigningKey" "<value>"` store values in a per-user JSON file under `%APPDATA%\Microsoft\UserSecrets`, which the configuration system merges with `appsettings.json` at startup without any repository exposure.

In production the minimum acceptable practice is an environment variable injected at runtime by the hosting platform (an App Service Application Setting, a Kubernetes Secret mounted as an env var, or a Docker `--env` flag). A more robust solution is Azure Key Vault with `builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential())` — the application authenticates to Key Vault using its Managed Identity, so there is no credential needed to retrieve the credential.

```csharp
// Program.cs — Key Vault via Managed Identity, no secrets in code
var kvUri = builder.Configuration["KeyVaultUri"]!;
builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());

// JWT signing with RS256 — private key from Key Vault certificate
var rsa = RSA.Create();
rsa.ImportFromPem(builder.Configuration["JwtSigningKey:PrivatePem"]);
var signingKey = new RsaSecurityKey(rsa) { KeyId = "v2" };
```

For JWT signing, prefer RS256 or ES256 over HS256. With HS256 every service that validates tokens must share the same secret, multiplying the exposure surface. With RS256 only the issuer holds the private key; API consumers download the public key from the JWKS endpoint and cache it. Key rotation is safe only with an overlap window: publish the new key to the JWKS endpoint and start signing new tokens with it, but continue accepting tokens signed by the old key for at least as long as the maximum access or refresh token lifetime. Only after that window closes should the old key be retired. Rotating without the overlap window causes a hard logout for every user who holds a token signed by the old key.

---

## CQ6. Policy-Based Authorization Against External IdP Claims
> Topics: [05 — Claims & Policy Authorization], [12 — Federated Identity & Advanced Auth Scenarios]

**Concepts**
- External IdPs issue claims under their own naming conventions — Azure AD may issue `extension_department` or a long-form namespace URI instead of the local `department` claim your policy expects
- `IAuthorizationRequirement` + `IAuthorizationHandler` is the extension point; the handler calls `context.User.HasClaim(...)` and succeeds or fails the requirement
- `IClaimsTransformation` is the cross-cutting normalization point — it runs after every authentication event and can rename or add claims before any authorization handler sees the principal
- `JwtBearerOptions.MapInboundClaims` (default `false` in .NET 10) controls whether the JWT middleware renames short claim names to long-form URI equivalents — mixing behaviors across versions breaks policies that reference one form or the other
- When an external claim name differs from the ASP.NET Core expected type, `HasClaim` silently returns false, `[Authorize(Policy = "...")]` silently denies access, and there is no runtime error pointing to the mismatch

**Answer**
Federated authentication introduces a claim name mismatch problem: your policy is written against one claim naming convention, but the external IdP issues the same data under a different key. An `[Authorize(Policy = "HROnly")]` policy that calls `context.User.HasClaim("department", "HR")` will silently deny access to a user whose Azure AD token carries `extension_department = HR`, because the string comparison fails and the authorization handler returns `context.Fail()` — or, worse, simply does not call `context.Succeed()`.

The right place to fix this depends on scope. If the mismatch is provider-specific, handle it in the OIDC middleware using `ClaimActions.MapJsonKey`:

```csharp
options.ClaimActions.MapJsonKey("department", "extension_department");
```

This maps the external JSON key to the internal claim type before the principal is serialized into the cookie, so every downstream handler and policy sees the normalized name. If the normalization needs to apply across multiple schemes — cookie auth, JWT bearer, and external logins — register an `IClaimsTransformation` implementation instead:

```csharp
public class ClaimNormalizer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity!;
        var raw = identity.FindFirst("extension_department") ?? identity.FindFirst("dept");
        if (raw is not null && !identity.HasClaim("department", raw.Value))
            identity.AddClaim(new Claim("department", raw.Value));
        return Task.FromResult(principal);
    }
}
```

A separate .NET 10 gotcha is `JwtBearerOptions.MapInboundClaims`. In older .NET versions this defaulted to `true`, causing the JWT middleware to rename short claim names such as `sub` to the long URI `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`. In .NET 10 it defaults to `false`, preserving the original short names from the token. An application migrated from an earlier version that writes policies or `FindFirst` calls against the long-form URI will silently break, because the claim now arrives under `sub`, not the URI. Auditing this flag on upgrade — and choosing one canonical form for all claim references across the codebase — prevents hard-to-diagnose authorization failures.

---

## CQ7. Custom User Properties vs Claims — IUserClaimsPrincipalFactory vs On-Demand DB Lookup
> Topics: [02 — ASP.NET Core Identity Setup & User Management], [05 — Claims, Roles & Policy-Based Authorization]

**Concepts**
- A property added to `ApplicationUser` (e.g., `Tier`, `DepartmentId`) is persisted to `AspNetUsers` but is NOT automatically projected into `ClaimsPrincipal`
- `IUserClaimsPrincipalFactory<TUser>` is called once at login to build the principal that is serialized into the auth cookie
- Override `GenerateClaimsAsync` to map custom properties to claims at login time — the resulting cookie carries all needed claims without a DB round-trip per request
- On-demand DB lookup inside a handler or middleware can provide fresher data but adds latency and requires careful caching
- The gotcha: a property added to `IdentityUser` but not included in the claims factory is invisible to `ClaimsPrincipal`, to policy handlers, and to every `[Authorize]` check

**Answer**

ASP.NET Core Identity stores the user record in the `AspNetUsers` table, but when a user logs in, what actually travels through the request pipeline is a `ClaimsPrincipal` — a serialized snapshot of the identity baked into the auth cookie. These two representations are not kept in sync automatically. If you add a `Tier` property to `ApplicationUser` and run a migration, that column exists in the database, but `User.FindFirst("tier")` returns null because nothing ever copied it into the principal.

The correct extension point is `IUserClaimsPrincipalFactory<TUser>`. Override `GenerateClaimsAsync` and add the property as a claim:

```csharp
public class AppClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(
        userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(
        ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("tier", user.Tier ?? "free"));
        identity.AddClaim(new Claim("dept_id", user.DepartmentId.ToString()));
        return identity;
    }
}
```

Register with `builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>()`. After the next login, the cookie contains the new claims and all authorization policies can inspect them without a database round-trip.

The on-demand DB lookup alternative — reading the user record inside middleware or a handler on every request — provides fresher data (useful when `Tier` can change while the user is logged in) but adds a database call per request. A middle path is to keep the claim in the cookie but call `SignInManager.RefreshSignInAsync(user)` whenever the property changes, which issues a new cookie with the updated claim without requiring the user to log out.

---

## CQ8. PBKDF2 Iteration Count and Hash Migration on Next Login
> Topics: [02 — ASP.NET Core Identity Setup & User Management], [04 — Password Security, Lockout & Data Protection]

**Concepts**
- ASP.NET Core Identity V3 hashing uses PBKDF2-SHA256 with a configurable iteration count (default 100,000 in .NET 10)
- `PasswordHasherOptions.IterationCount` sets the count; `CompatibilityMode` switches between V2 (10,000 iterations, SHA-1) and V3
- `IPasswordHasher<TUser>.VerifyHashedPassword` returns `PasswordVerificationResult.SuccessRehashNeeded` when the stored hash was produced with outdated parameters
- `SignInManager.PasswordSignInAsync` detects `SuccessRehashNeeded` and automatically re-hashes the password with the current settings before updating the stored hash — transparent to the user
- The legacy-system gotcha: bcrypt or Argon2 hashes imported from another system are not understood by the built-in hasher; a custom `IPasswordHasher<TUser>` must handle both formats during the migration window

**Answer**

ASP.NET Core Identity hashes passwords using PBKDF2 with HMAC-SHA256 (format version 3 by default). The iteration count — how many rounds of the key-derivation function are applied — trades login latency for brute-force resistance. In .NET 10 the default is 100,000 iterations, and you can raise it further:

```csharp
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    options.IterationCount    = 350_000;
});
```

When `VerifyHashedPassword` is called at login, it decodes the hash header to find the iteration count used when the hash was created. If it is lower than the current `IterationCount`, the method returns `PasswordVerificationResult.SuccessRehashNeeded` instead of `PasswordVerificationResult.Success`. The `PasswordSignInAsync` implementation inside `SignInManager` checks for this result and silently updates the stored hash to the new count — migration is gradual and transparent: old hashes are upgraded one login at a time with no downtime or forced password reset.

The critical gap arises when migrating from a system that used bcrypt, Argon2, or a custom scheme. The built-in hasher does not understand those formats and returns `Failed` for every bcrypt hash presented. The solution is a composite `IPasswordHasher<TUser>`:

```csharp
public class MigratingPasswordHasher(
    PasswordHasher<ApplicationUser> modern) : IPasswordHasher<ApplicationUser>
{
    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user, string hashedPassword, string providedPassword)
    {
        if (hashedPassword.StartsWith("$2"))   // bcrypt sentinel
        {
            bool ok = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            return ok ? PasswordVerificationResult.SuccessRehashNeeded
                      : PasswordVerificationResult.Failed;
        }
        return modern.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }

    public string HashPassword(ApplicationUser user, string password)
        => modern.HashPassword(user, password);
}
```

Returning `SuccessRehashNeeded` for a bcrypt match causes `SignInManager` to immediately re-hash the password with PBKDF2, so legacy hashes are replaced one login at a time without a forced password reset. Register the composite hasher with `builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, MigratingPasswordHasher>()`.

---

## CQ9. How the Auth Cookie Is Encrypted and the Distributed Key-Ring Gotcha
> Topics: [03 — Registration, Login & Cookie Authentication], [04 — Password Security, Lockout & Data Protection]

**Concepts**
- The ASP.NET Core Data Protection API protects the auth cookie using AES-256-CBC encryption + HMAC-SHA256 authentication
- By default each application instance generates its own key ring stored in a local per-machine directory or in memory
- In a scale-out deployment (App Service scale-out, Kubernetes), Instance A's key ring differs from Instance B's — cookies issued by A cannot be decrypted by B, causing 403 errors or redirect loops for load-balanced users
- Solution: shared key storage (`PersistKeysToAzureBlobStorage`, `PersistKeysToStackExchangeRedis`, or `PersistKeysToDbContext`) + shared key encryption (`ProtectKeysWithAzureKeyVault` or a certificate)
- `SetApplicationName` must be identical across all instances; mismatched app names produce keys that cannot be used interchangeably even when stored in the same location

**Answer**

When ASP.NET Core serializes a `ClaimsPrincipal` into an auth cookie, it uses the Data Protection API rather than a raw encryption call. Data Protection applies AES-256-CBC to encrypt the cookie payload and HMAC-SHA256 to authenticate it, using a key from a managed key ring. The key ring rotates automatically — a new key every 90 days by default, with old keys retained for decryption until they age out. In development, the key ring lives in `%LOCALAPPDATA%\ASP.NET\DataProtection-Keys` and everything works because one instance holds all keys.

The problem surfaces in production when the application scales out. Each App Service instance, Kubernetes pod, or container starts with an independent local key ring. A user authenticated by Instance A receives a cookie encrypted under Instance A's key. The next request, routed to Instance B by the load balancer, finds the cookie unreadable — Instance B does not have Instance A's key — and redirects the user to the login page. This is intermittent, difficult to reproduce in single-instance staging, and often misdiagnosed as a session configuration issue.

The fix is a shared key ring with at-rest encryption:

```csharp
// Program.cs — shared key ring for multi-instance deployments
builder.Services.AddDataProtection()
    .SetApplicationName("MyApp")
    .PersistKeysToAzureBlobStorage(
        new Uri(builder.Configuration["DataProtection:BlobUri"]!),
        new DefaultAzureCredential())
    .ProtectKeysWithAzureKeyVault(
        new Uri(builder.Configuration["DataProtection:KeyVaultKeyId"]!),
        new DefaultAzureCredential());
```

`SetApplicationName` is important: two applications sharing the same Blob container but with different names produce keys in separate namespaces — each app can only read its own cookies. Without `ProtectKeysWithAzureKeyVault` (or a certificate equivalent), the key XML files sit in plaintext in Blob Storage. Redis (`PersistKeysToStackExchangeRedis`) and SQL Server (`PersistKeysToDbContext<T>`) are alternatives to Azure Blob when those infrastructure dependencies are preferred.

---

## CQ10. Authorization Code Flow with PKCE for SPAs and Why Implicit Flow Is Deprecated
> Topics: [06 — OAuth 2.0 & OpenID Connect Fundamentals], [08 — JWT Bearer Tokens & Securing Web APIs]

**Concepts**
- Authorization code flow: the client receives a short-lived `code` in the redirect URI, then exchanges it for tokens in a back-channel POST — tokens never appear in the URL
- PKCE (Proof Key for Code Exchange): the client generates a random `code_verifier`, sends `code_challenge = BASE64URL(SHA256(code_verifier))` in the authorization request, and sends the raw `code_verifier` in the token exchange; the server verifies the relationship, binding the exchange to the originating client
- Implicit flow (deprecated): the `access_token` is returned directly in the URL fragment, which appears in browser history, server access logs, and `Referer` headers sent to third-party scripts
- ASP.NET Core `AddOpenIdConnect` sends PKCE automatically when `ResponseType = "code"` (the default in .NET 10); `UsePkce = true` is the explicit opt-in flag
- For browser-only SPAs without a .NET backend, MSAL.js and similar standards-compliant OIDC libraries handle PKCE natively; the BFF pattern is the highest-security alternative

**Answer**

The implicit flow was designed for public JavaScript clients that could not securely store a `client_secret`. The authorization server returned the access token directly in the redirect URI's fragment (`#access_token=…`), skipping the back-channel code exchange. The problem is that anything in the URL can leak: browsers store the full URL in history, include it in `Referer` headers sent to third-party analytics scripts, and write it to server access logs. An access token in the URL has a high exfiltration surface even if the user never copies it.

Authorization code flow with PKCE eliminates this exposure for public clients without requiring a client secret. The SPA generates a cryptographically random `code_verifier` (43–128 characters), computes `code_challenge = BASE64URL(SHA256(code_verifier))`, and sends only the challenge in the authorization request. The authorization server returns a short-lived, single-use authorization code in the redirect URI. The SPA then POSTs the code plus the original `code_verifier` to the token endpoint. The server re-hashes the verifier and compares it to the stored challenge — if they match, tokens are issued. An attacker who intercepts the authorization code cannot exchange it without knowing the verifier.

```csharp
// Program.cs — AddOpenIdConnect uses PKCE by default for code flow
builder.Services.AddAuthentication()
    .AddOpenIdConnect("oidc", options =>
    {
        options.ResponseType = "code";   // authorization code flow
        options.UsePkce      = true;     // default; explicit for clarity
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("api");
        options.SaveTokens = true;
    });
```

The OAuth 2.1 draft formally removes implicit flow and requires PKCE for all authorization code requests, including confidential clients. Migrating an existing SPA from implicit to PKCE-code typically involves changing `response_type` from `token` to `code` and adding the PKCE parameters — most modern authorization servers support both during the transition period. For ASP.NET Core server-rendered applications, `AddOpenIdConnect` handles the entire PKCE ceremony automatically with no additional code.

---

## CQ11. Multi-Tenant Azure Entra ID — Validating the tid Claim and the Common-Endpoint Issuer Gotcha
> Topics: [07 — External Logins & OIDC Client Integration], [12 — Federated Identity & Advanced Auth Scenarios]

**Concepts**
- The `/common` endpoint accepts users from any Entra ID tenant; each token's `iss` claim is tenant-specific: `https://login.microsoftonline.com/{tenantId}/v2.0`
- `TokenValidationParameters.ValidateIssuer = true` with a single hard-coded issuer rejects tokens from all other tenants — which is effectively all users for a multi-tenant app
- Setting `ValidateIssuer = false` without additional checks allows any Entra tenant in the world to authenticate into the application
- Correct approach: `ValidateIssuer = false` plus an `OnTokenValidated` handler that checks the `tid` claim against a configured allowed-tenant list
- `Microsoft.Identity.Web` (`AddMicrosoftIdentityWebAppAuthentication`) encapsulates this pattern behind an `AllowedTenants` configuration property

**Answer**

Single-tenant Entra ID applications use a tenant-specific endpoint (`https://login.microsoftonline.com/{tenantId}/v2.0`). Tokens carry an `iss` claim that matches that endpoint, so `ValidateIssuer = true` with the matching value works exactly as expected. Multi-tenant applications use the `/common` endpoint so that users from any Entra tenant can sign in. The complication is that each Entra tenant issues tokens with a unique `iss` value: `https://login.microsoftonline.com/72f988bf-xxxx/v2.0` for one tenant, a different GUID for every other tenant.

Setting `ValidateIssuer = true` with a single string causes the JWT middleware to reject tokens from every tenant whose GUID differs from the one hard-coded in configuration — which is all users for a multi-tenant app. The naive fix is `ValidateIssuer = false`, but without additional validation that accepts any Entra tenant in the world, including directories belonging to unrelated organizations.

The correct pattern is `ValidateIssuer = false` combined with an explicit `tid` claim check:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/common/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer   = false,
            ValidateAudience = true,
            ValidAudience    = builder.Configuration["AzureAd:ClientId"]
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                var tid = ctx.Principal!.FindFirstValue("tid");
                var allowed = builder.Configuration
                    .GetSection("AzureAd:AllowedTenants")
                    .Get<string[]>()!;
                if (!allowed.Contains(tid))
                    ctx.Fail("Tenant not authorized.");
                return Task.CompletedTask;
            }
        };
    });
```

`Microsoft.Identity.Web` wraps this pattern behind `AddMicrosoftIdentityWebAppAuthentication` with an `AllowedTenants` list in `appsettings.json`, which is the preferred approach for new projects. Note also that the `/common` metadata document does not serve a fixed JWKS — signing key retrieval happens per-token by reading the `tid` from the token header and constructing a tenant-specific key endpoint, so key caching behavior differs from single-tenant deployments.

---

## CQ12. Role Checks with Cookie Auth vs JWT — Why [Authorize(Roles)] Can Silently Fail
> Topics: [01 — Authentication & Authorization Fundamentals], [05 — Claims, Roles & Policy-Based Authorization], [08 — JWT Bearer Tokens & Securing Web APIs]

**Concepts**
- `[Authorize(Roles = "Admin")]` internally calls `ClaimsPrincipal.IsInRole`, which searches for a claim whose type equals `ClaimsIdentity.RoleClaimType`
- Cookie auth (via `SignInManager`): roles are stored under `ClaimTypes.Role` (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`), the default `RoleClaimType` — role checks work without configuration
- JWT auth with `MapInboundClaims = false` (the .NET 10 default): the `"roles"` claim stays as the short string `"roles"`, but `RoleClaimType` still defaults to the long URI — the type comparison fails silently, producing a 403 with no error message
- Fix: set `TokenValidationParameters.RoleClaimType = "roles"` to match the actual claim type in the JWT
- Alternative: `MapInboundClaims = true` renames `"roles"` to the long URI, but also renames `"sub"`, `"iss"`, and other short-name claims, breaking any code that references them by short name

**Answer**

`[Authorize(Roles = "Admin")]` is syntactic sugar that translates to a policy checking `ClaimsPrincipal.IsInRole("Admin")`. Internally, `IsInRole` iterates the `ClaimsIdentity` objects on the principal, searching for a claim whose `Type` equals `identity.RoleClaimType` and whose `Value` equals `"Admin"`. The gotcha is that `RoleClaimType` is set when the identity is constructed and its default value is the long URI `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`.

When you sign in with cookie authentication, `SignInManager.SignInWithClaimsAsync` serializes role claims under exactly that long-form type. `IsInRole("Admin")` finds the claim, and `[Authorize(Roles = "Admin")]` succeeds. When you validate a JWT bearer token in .NET 10, however, `MapInboundClaims` defaults to `false`, so the `"roles"` claim from the token payload remains as the short string `"roles"`. The `ClaimsIdentity` created by the JWT middleware uses `ClaimTypes.Role` (the long URI) as its `RoleClaimType`, so `IsInRole` searches under the long URI, finds nothing, and the authorization silently fails with a 403 — no exception, no log entry explaining the type mismatch.

```csharp
// Fix: align RoleClaimType with the actual claim name in the JWT
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType    = "roles",
            NameClaimType    = "name",
            ValidateIssuer   = true,
            ValidIssuer      = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience    = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = /* ... */
        };
    });
```

An equally valid fix is to emit the role claim under the long URI at token-issuance time, so the JWT payload already uses `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` as the claim type. Either approach works as long as `RoleClaimType` and the claim type in the token agree. Choose one canonical form across the codebase and enforce it, because mismatches are invisible at startup and only manifest as silent runtime 403 responses.

---

## CQ13. First-Time 2FA Setup — TOTP Secret, QR Code, Confirmation, and Recovery Codes
> Topics: [02 — ASP.NET Core Identity Setup & User Management], [09 — Two-Factor Authentication & Identity Extensibility]

**Concepts**
- `UserManager.GetAuthenticatorKeyAsync` returns the user's TOTP shared secret (Base32-encoded), generating and persisting a new one if none exists yet
- The secret is formatted as an `otpauth://totp/{issuer}:{email}?secret={base32secret}&issuer={issuer}` URI and rendered as a QR code client-side
- `UserManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code)` confirms the 6-digit code before 2FA is enabled
- `UserManager.SetTwoFactorEnabledAsync(user, true)` enables the feature after successful verification
- `UserManager.GenerateNewTwoFactorRecoveryCodesAsync` produces single-use recovery codes hashed and stored in `AspNetUserTokens`; calling it again immediately invalidates all previously issued codes

**Answer**

Setting up TOTP-based 2FA for the first time involves a small ceremony: generate a shared secret, let the user scan it into an authenticator app, confirm the app is producing valid codes, enable 2FA, and then issue recovery codes. ASP.NET Core Identity provides all the building blocks.

`GetAuthenticatorKeyAsync` returns the user's existing TOTP secret or generates and persists a new one. The secret arrives as a Base32 string. Authenticator apps such as Google Authenticator and Microsoft Authenticator expect an `otpauth://totp/` URI, which you build server-side and render as a QR code client-side:

```csharp
var key = await _userManager.GetAuthenticatorKeyAsync(user);
if (key is null)
{
    await _userManager.ResetAuthenticatorKeyAsync(user);
    key = await _userManager.GetAuthenticatorKeyAsync(user);
}

var uri = $"otpauth://totp/{Uri.EscapeDataString("MyApp")}:" +
          $"{Uri.EscapeDataString(user.Email!)}?" +
          $"secret={key}&issuer={Uri.EscapeDataString("MyApp")}&digits=6";
// Pass uri to the view; render as QR code using a JavaScript library
```

Before enabling 2FA, verify the code the user enters from their app to confirm they scanned the secret correctly:

```csharp
bool valid = await _userManager.VerifyTwoFactorTokenAsync(
    user, TokenOptions.DefaultAuthenticatorProvider, model.Code);

if (!valid)
{
    ModelState.AddModelError("Code", "Invalid verification code.");
    return Page();
}

await _userManager.SetTwoFactorEnabledAsync(user, true);
var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
// Display recoveryCodes once — they are hashed in the DB and cannot be retrieved later
```

Recovery codes are hashed single-use tokens stored in `AspNetUserTokens`. When a user redeems one via `TwoFactorRecoveryCodeSignInAsync`, it is consumed and cannot be reused. Calling `GenerateNewTwoFactorRecoveryCodesAsync` again issues a fresh batch and immediately invalidates all prior codes, so users who regenerate must be warned that any previously printed codes are now worthless. Always prompt users to save their recovery codes before leaving the setup page, as there is no mechanism to display them a second time.

---

## CQ14. JWT Storage — localStorage vs HttpOnly Cookie vs BFF Pattern
> Topics: [03 — Registration, Login & Cookie Authentication], [08 — JWT Bearer Tokens & Securing Web APIs], [10 — Web Application Security]

**Concepts**
- `localStorage` is readable by any JavaScript on the same origin — an XSS vulnerability can silently exfiltrate a stored JWT to an attacker-controlled server
- `HttpOnly` cookies are invisible to JavaScript — XSS cannot read the token — but cookie auto-attachment reintroduces CSRF risk; mitigate with `SameSite=Strict` or `SameSite=Lax` plus antiforgery tokens
- `SameSite=Strict` prevents the cookie from being sent on any cross-site request (including top-level navigations); `SameSite=Lax` allows safe top-level GET navigations; `SameSite=None` requires `Secure` and is needed for legitimate cross-site embedding
- The BFF (Backend-for-Frontend) pattern: the SPA authenticates against its own same-origin backend, which stores tokens server-side and uses an `HttpOnly` session cookie for the SPA; the BFF proxies API calls attaching the bearer token server-side — the token never reaches the browser
- BFF is the recommended architecture for SPAs handling sensitive data; YARP and Duende BFF are common .NET 10 implementation choices

**Answer**

Where to store a JWT in a browser application is fundamentally a choice between XSS risk and CSRF risk. `localStorage` is convenient — JavaScript can read it and attach it in `Authorization` headers on every `fetch` call — but it is fully accessible to any script running on the page, including scripts injected via an XSS vulnerability. A single XSS vector can drain the stored token to an external server, achieving the same effect as stealing a password. The token remains valid until expiry regardless of what the victim does.

Storing the JWT in an `HttpOnly` cookie removes it from JavaScript's reach entirely. XSS can no longer read the token. The trade-off is that the browser attaches the cookie automatically, reintroducing CSRF exposure. The `SameSite` attribute is the primary mitigation: `SameSite=Strict` causes the browser to omit the cookie from all cross-site requests, making CSRF essentially impossible. `SameSite=Lax` (the modern browser default) allows safe top-level GET navigations but blocks cross-site POST requests — sufficient for most CSRF scenarios. Applications using `SameSite=None` for legitimate cross-site embedding still need antiforgery token validation.

The BFF pattern eliminates the JWT from the browser entirely. The SPA authenticates against a same-origin backend that stores OAuth tokens in a server-side session. The SPA authenticates with that backend using a short-lived `HttpOnly` session cookie. When the SPA needs data from a downstream API, it calls a BFF endpoint, which attaches the stored bearer token server-side before forwarding the request. The browser never holds a raw JWT. An XSS attacker can call BFF endpoints using the session cookie, but they cannot export the token to replay it elsewhere — the blast radius is bounded to the current authenticated session. For .NET 10, YARP with cookie-authenticated reverse-proxy routes or the Duende BFF library are the most practical implementations.

---

## CQ15. Refresh Token Rotation in ASP.NET Core Identity
> Topics: [06 — OAuth 2.0 & OpenID Connect Fundamentals], [08 — JWT Bearer Tokens & Securing Web APIs], [02 — ASP.NET Core Identity Setup & User Management]

**Concepts**
- Refresh tokens are long-lived credentials; in ASP.NET Core Identity they are stored in the `AspNetUserTokens` table via `UserManager.SetAuthenticationTokenAsync`
- Single-use rotation: on every use the old refresh token is deleted and a new one is issued atomically; a stolen token that is used by an attacker triggers the rotation and immediately invalidates the original
- Reuse detection: a client presenting a refresh token that no longer exists in the database signals a possible compromise — the correct response is to revoke all refresh tokens for that user
- The delete-before-issue order is critical; issuing the new token before deleting the old one creates a window where two concurrent requests can each claim the same refresh token
- `RemoveAuthenticationTokenAsync` followed by `SetAuthenticationTokenAsync` implements rotation; wrap in a serialized operation to prevent races

**Answer**

A refresh token is a long-lived credential that lets a client obtain new access tokens without re-authenticating the user. Because it is long-lived, its compromise is more damaging than leaking a short-lived access token. Refresh token rotation limits this damage: each use produces a new refresh token and the consumed one is immediately invalidated. An attacker who steals a refresh token and uses it triggers the rotation, making the victim's copy invalid — the first party to notice they can no longer refresh is the signal that a compromise occurred.

In ASP.NET Core Identity, refresh tokens are stored in `AspNetUserTokens` via `UserManager.SetAuthenticationTokenAsync`. The rotation flow is:

```csharp
public async Task<TokenResponse> RotateRefreshTokenAsync(
    string incoming, ApplicationUser user)
{
    var stored = await _userManager.GetAuthenticationTokenAsync(
        user, "MyApp", "RefreshToken");

    if (stored is null || stored != incoming)
    {
        // Token already consumed or never existed — treat as compromise
        await _userManager.RemoveAuthenticationTokenAsync(
            user, "MyApp", "RefreshToken");   // revoke all for this user
        throw new SecurityTokenException("Refresh token reuse detected.");
    }

    // Delete first, then issue — prevents two concurrent callers from both succeeding
    await _userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshToken");

    var newRefresh = GenerateSecureToken();   // cryptographically random, opaque
    await _userManager.SetAuthenticationTokenAsync(
        user, "MyApp", "RefreshToken", newRefresh);

    var newAccess = _jwtService.Issue(user);
    return new TokenResponse(newAccess, newRefresh);
}
```

The delete-before-issue order is intentional. If the new token is stored first and the response is lost before the old token is deleted, two valid refresh tokens coexist — exactly what rotation prevents. Detecting reuse (incoming token not found) should revoke all tokens for the affected user, forcing re-authentication. For high-security scenarios, storing a token family ID allows tracing and fully revoking all branches of a reuse chain across multiple devices.

---

## CQ16. Resource-Based Authorization — IAuthorizationService vs the [Authorize] Attribute
> Topics: [01 — Authentication & Authorization Fundamentals], [05 — Claims, Roles & Policy-Based Authorization]

**Concepts**
- `[Authorize(Policy = "...")]` runs as middleware before the action executes — the resource has not yet been loaded from the database
- Resource-based authorization requires the actual resource instance to make an access decision (e.g., is this user the owner of *this specific* document?)
- `IAuthorizationService.AuthorizeAsync(User, resource, requirement)` is called inside the action body after loading the resource
- `AuthorizationHandler<TRequirement, TResource>` receives both the `AuthorizationHandlerContext` and the typed resource instance, enabling per-object ownership rules
- The two mechanisms are complementary: `[Authorize]` guards the endpoint (authentication + broad policy); `IAuthorizationService` guards the specific resource instance (ownership, state-based rules)

**Answer**

`[Authorize]` attributes evaluate before the action method executes. This is correct for broad gate checks — is the user authenticated? Do they hold the `Editor` role? Does the `SubscriptionActive` policy pass? — because those checks require no data from the database. The attribute cannot answer "can this user edit this specific document?" because the document has not been loaded yet when the attribute evaluates.

Resource-based authorization moves the check inside the action, after the resource is retrieved:

```csharp
[HttpGet("{id}")]
[Authorize]   // ensures the user is authenticated before touching the DB
public async Task<IActionResult> Edit(int id)
{
    var doc = await _db.Documents.FindAsync(id);
    if (doc is null) return NotFound();

    var result = await _authService.AuthorizeAsync(User, doc, "DocumentEditPolicy");
    if (!result.Succeeded) return Forbid();

    return View(doc);
}
```

The `"DocumentEditPolicy"` is backed by a typed handler:

```csharp
public class DocumentEditHandler
    : AuthorizationHandler<DocumentEditRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DocumentEditRequirement requirement,
        Document resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (resource.OwnerId == userId || context.User.IsInRole("Admin"))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```

Register the handler: `builder.Services.AddSingleton<IAuthorizationHandler, DocumentEditHandler>()`. The `[Authorize]` attribute on the action still runs first, rejecting unauthenticated requests before any database call, keeping the pipeline efficient. `IAuthorizationService` then makes the fine-grained per-instance ownership decision. Neither mechanism replaces the other — applying only `[Authorize]` without resource-based checks means any authenticated user can access any document by guessing its ID; applying only `IAuthorizationService` without `[Authorize]` means unauthenticated callers reach the database before being rejected.

---

## CQ17. Lockout and External Provider Logins — The Unified Lockout Gap
> Topics: [02 — ASP.NET Core Identity Setup & User Management], [04 — Password Security, Lockout & Data Protection], [07 — External Logins & OIDC Client Integration]

**Concepts**
- `PasswordSignInAsync` calls `UserManager.AccessFailedAsync` on a failed password attempt, incrementing the lockout counter
- `ExternalLoginSignInAsync` does not involve a password check and never calls `AccessFailedAsync` — the lockout counter is entirely bypassed for external-provider logins
- A user who authenticates via Google, Microsoft, or any external provider is structurally immune to the built-in password lockout mechanism
- The gap: repeated failed TOTP attempts following an external login do not trigger lockout; suspicious external-login patterns leave no trace in the lockout state
- Mitigation: manually call `AccessFailedAsync` in the external login callback and in the 2FA challenge handler; or override `SignInManager<TUser>.ExternalLoginSignInAsync` to add custom suspicious-activity tracking

**Answer**

ASP.NET Core Identity's lockout mechanism is tightly coupled to the password sign-in path. When a user enters a wrong password, `PasswordSignInAsync` calls `UserManager.AccessFailedAsync`, which increments a counter in `AspNetUsers`. Once the counter reaches `MaxFailedAccessAttempts`, `IsLockedOutAsync` returns `true` and subsequent logins are rejected until the lockout period expires. This is a solid defence against credential-stuffing and brute-force attacks on password-based accounts.

`ExternalLoginSignInAsync` takes a different path entirely. It locates the user by the external provider's `(provider, providerKey)` pair and calls `SignInOrTwoFactorAsync` internally — there is no credential to validate, so `AccessFailedAsync` is never invoked. The lockout counter stays at zero regardless of how many times the external login flow is attempted or how many times the subsequent 2FA challenge fails (see CQ3). An attacker who compromises a user's Google account and repeatedly attempts to log into your application faces no automated throttling from ASP.NET Core Identity.

Mitigation requires explicit calls at the external login callback:

```csharp
// ExternalLoginCallback action — after ExternalLoginSignInAsync
if (!result.Succeeded && !result.RequiresTwoFactor)
{
    var info = await _signInManager.GetExternalLoginInfoAsync();
    var user = await _userManager.FindByLoginAsync(
        info!.LoginProvider, info.ProviderKey);
    if (user is not null)
    {
        await _userManager.AccessFailedAsync(user);
        if (await _userManager.IsLockedOutAsync(user))
            return RedirectToPage("./Lockout");
    }
}
```

For broader monitoring — detecting patterns such as many failed logins before a successful one, or the same provider account attempting to log into multiple local accounts — override `SignInManager<TUser>.ExternalLoginSignInAsync` and publish security events to a logging or SIEM pipeline. A custom `IUserClaimsPrincipalFactory` can add audit-trail claims (e.g., `last_external_login_ip`) to the principal at login time, but it cannot enforce lockout on its own because it runs only after the sign-in decision has already been made.

---

## Cross-Reference Quick Map

| CQ | Primary Tension | Subtopics Involved |
|----|-----------------|-------------------|
| CQ1 | Cookie vs JWT token delivery | 03, 08 |
| CQ2 | Claims lost between OIDC → principal → bearer token | 05, 06, 07, 08 |
| CQ3 | 2FA failures bypass lockout counter | 02, 04, 09 |
| CQ4 | CSRF protection: cookie endpoints vs JWT APIs | 03, 08, 10 |
| CQ5 | Secret and signing key lifecycle in production | 07, 08, 11 |
| CQ6 | External claim name mismatch silently breaks policies | 05, 12 |
| CQ7 | Custom IdentityUser property invisible to ClaimsPrincipal | 02, 05 |
| CQ8 | PBKDF2 iteration count migration and legacy hash formats | 02, 04 |
| CQ9 | Distributed key-ring breaks auth cookies at scale-out | 03, 04 |
| CQ10 | PKCE replaces implicit flow for SPA token safety | 06, 08 |
| CQ11 | /common endpoint issuer validation breaks multi-tenant auth | 07, 12 |
| CQ12 | RoleClaimType mismatch causes silent [Authorize(Roles)] failure | 01, 05, 08 |
| CQ13 | TOTP 2FA setup ceremony and recovery code lifecycle | 02, 09 |
| CQ14 | JWT browser storage: XSS vs CSRF vs BFF trade-offs | 03, 08, 10 |
| CQ15 | Refresh token rotation and reuse-detection revocation | 02, 06, 08 |
| CQ16 | Resource-based authorization needs IAuthorizationService, not [Authorize] | 01, 05 |
| CQ17 | External provider logins bypass the password lockout counter | 02, 04, 07 |
