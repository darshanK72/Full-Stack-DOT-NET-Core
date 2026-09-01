# JWT Bearer Tokens & Securing Web APIs — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [What is a JSON Web Token (JWT), and what are its three structural parts?](#q1)
2. [What information is stored in the JWT header, and why does the algorithm choice …](#q2)
3. [What are the standard claims in a JWT payload (sub, iss, aud, exp, iat), and wha…](#q3)
4. [What is the difference between signing and encrypting a JWT? Is the payload visi…](#q4)
5. [How does JWT signature verification work, and what does it guarantee?](#q5)
6. [Compare HS256, RS256, and ES256 signing algorithms. When would you choose each?](#q6)
7. [What are the advantages of RS256 over HS256 in a microservices architecture?](#q7)
8. [How do you add JWT bearer authentication to an ASP.NET Core application?](#q8)
9. [What is `JwtBearerOptions`, and which properties do you configure most often?](#q9)
10. [What is `TokenValidationParameters`, and which validation flags are critical for…](#q10)
11. [Why is `ValidateLifetime` important, and what happens if you disable it?](#q11)
12. [Why must both `ValidateIssuer` and `ValidateAudience` be set to `true` in produc…](#q12)
13. [How do you generate a JWT in .NET using `JwtSecurityTokenHandler`?](#q13)
14. [What claims should you include in a JWT for an ASP.NET Core Identity-backed API?](#q14)
15. [How does ASP.NET Core Identity integrate with JWT generation — what role does `U…](#q15)
16. [What is the refresh token pattern, and why are access tokens intentionally short…](#q16)
17. [How should refresh tokens be stored and validated on the server side?](#q17)
18. [What is the token revocation problem with JWTs, and what strategies exist to mit…](#q18)
19. [Should JWTs be stored in `localStorage` or an `HttpOnly` cookie? What are the se…](#q19)
20. [How does the `[Authorize]` attribute work with bearer tokens in ASP.NET Core?](#q20)
21. [How do you define and enforce policy-based authorization alongside JWT bearer au…](#q21)
22. [What role does CORS (Cross-Origin Resource Sharing) play in API security when us…](#q22)
23. [How do you configure CORS in ASP.NET Core so that the `Authorization` header is …](#q23)
24. [How do you pass custom claims (e.g., roles, tenant ID) in a JWT and read them in…](#q24)
25. [What happens when a JWT expires mid-request? How can clients detect and handle t…](#q25)
26. [How do you validate a JWT manually without the ASP.NET Core middleware pipeline?](#q26)
27. [What is a JWK Set (JWKS) endpoint, and how does it enable key rotation without r…](#q27)
28. [What are the most common security mistakes developers make when implementing JWT…](#q28)

---

## Q1. What is a JSON Web Token (JWT), and what are its three structural parts?

What is a JSON Web Token (JWT), and what are its three structural parts?

**Answer:** A JSON Web Token (JWT) is a compact, URL-safe string used to securely transmit claims between two parties as a JSON object. It is self-contained in that the token itself carries all information needed for verification, eliminating the need for the server to query a session store on every request. The three parts are separated by dots and each part is Base64URL-encoded.

- The **header** describes the token type (`JWT`) and the signing algorithm used (e.g., `HS256` or `RS256`).
- The **payload** carries the claims — pieces of information about the subject, issuer, expiry, and any custom application data.
- The **signature** is produced by running the encoded header and payload through the signing algorithm with a secret or private key; it allows the receiver to verify authenticity and detect tampering.

---

## Q2. What information is stored in the JWT header, and why does the algorithm choice matter?

What information is stored in the JWT header, and why does the algorithm choice matter?

**Answer:** The JWT header is a JSON object that contains two fields: `alg`, which specifies the signing algorithm (such as `HS256`, `RS256`, or `ES256`), and `typ`, which identifies the token type and is always `JWT`. The algorithm choice matters because it determines the cryptographic guarantees offered and the operational model required to verify the token.

- A symmetric algorithm like HMAC-SHA256 (`HS256`) uses the same secret key to both sign and verify — any party that can verify can also forge tokens, which is a problem when multiple services need to validate tokens.
- An asymmetric algorithm like `RS256` uses a private key to sign and a public key to verify; resource servers can validate tokens without ever holding the signing secret.
- The `alg` field in the header was historically trusted by some libraries, leading to the "none" algorithm attack — modern libraries require you to explicitly declare acceptable algorithms in `TokenValidationParameters` rather than reading the value from the token itself.

---

## Q3. What are the standard claims in a JWT payload (sub, iss, aud, exp, iat), and what does each represent?

What are the standard claims in a JWT payload (sub, iss, aud, exp, iat), and what does each represent?

**Answer:** JWT defines a set of registered claim names in RFC 7519 that carry semantically meaningful information about the token and its subject. These are not mandatory, but using them enables interoperability and supports standard validation logic in libraries like `System.IdentityModel.Tokens.Jwt`.

| Claim | Full Name        | Meaning |
|-------|------------------|---------|
| `sub` | Subject          | Unique identifier of the principal the token represents (e.g., user ID) |
| `iss` | Issuer           | URI or name of the server that created and signed the token |
| `aud` | Audience         | Intended recipient(s) — the service(s) that should accept this token |
| `exp` | Expiration Time  | Unix timestamp after which the token must be rejected |
| `iat` | Issued At        | Unix timestamp when the token was created |
| `nbf` | Not Before       | Unix timestamp before which the token is not yet valid |
| `jti` | JWT ID           | Unique identifier for this token; used for revocation tracking |

- The `exp` claim is the primary defence against token theft — a short expiry window limits the damage an attacker can do with a stolen token.
- The `iss` and `aud` claims together ensure a token issued for Service A cannot be replayed against Service B, which is the audience confusion attack.

---

## Q4. What is the difference between signing and encrypting a JWT? Is the payload visible to anyone who holds the token?

What is the difference between signing and encrypting a JWT? Is the payload visible to anyone who holds the token?

**Answer:** Signing a JWT (producing a JWS — JSON Web Signature) guarantees integrity and authenticity: the receiver can confirm the token has not been altered and that it was created by the expected party. Encrypting a JWT (producing a JWE — JSON Web Encryption) protects confidentiality: the payload is unreadable to anyone who does not hold the decryption key. By default, JWTs used in practice are only signed, not encrypted — the payload is Base64URL-encoded, not encrypted, and anyone who intercepts the token can decode and read it.

- Base64URL encoding is a reversible text encoding with no security properties; decoding the payload requires no key and can be done instantly in a browser or with any online decoder.
- Sensitive information such as passwords, payment details, or Personally Identifiable Information (PII) must never be placed in a standard JWT payload unless the token is also encrypted (JWE).
- The signature does prevent tampering — if an attacker modifies any claim in the payload, signature verification will fail, so the data is protected for integrity even though it is not confidential.

---

## Q5. How does JWT signature verification work, and what does it guarantee?

How does JWT signature verification work, and what does it guarantee?

**Answer:** When a receiver gets a JWT, it takes the encoded header and encoded payload from the token, re-runs the same signing algorithm using the verification key (the shared secret for HMAC, or the public key for RSA/ECDSA), and compares the result to the signature embedded in the token. If the two match, the token has not been tampered with since it was issued by the holder of the signing key. If they differ, the token is rejected.

- Verification guarantees integrity (the payload has not changed) and authenticity (the token was signed by a party holding the correct key), but it does not guarantee the token is still valid for use — expiry and audience checks are separate steps.
- For HMAC-based algorithms, any party with the shared secret can both sign and verify, so leaking the secret allows attackers to mint arbitrary tokens.
- For RSA/ECDSA-based algorithms, only the private key holder can sign, while any party with the public key can verify — this allows wide distribution of the verification key without compromising signing capability.

---

## Q6. Compare HS256, RS256, and ES256 signing algorithms. When would you choose each?

Compare HS256, RS256, and ES256 signing algorithms. When would you choose each?

**Answer:** The three algorithms differ in their cryptographic family, key model, and operational characteristics. HS256 uses a single shared secret, while RS256 and ES256 use asymmetric key pairs, which changes both the security model and the deployment complexity.

| Algorithm | Family | Key Type | Signature Size | Best Used When |
|-----------|--------|----------|----------------|----------------|
| HS256 | HMAC-SHA256 | Shared secret | 32 bytes | Single-service or trusted internal system |
| RS256 | RSA-SHA256 | Public/Private key pair | ~256 bytes | Multiple services needing to verify; key rotation via JWKS |
| ES256 | ECDSA-SHA256 | Elliptic Curve key pair | 64 bytes | High-performance systems needing asymmetric security with smaller keys |

- HS256 is simple to set up but requires every verifying service to securely share the secret, making it a poor fit for microservices where token verification is distributed.
- RS256 is the most common choice in enterprise and OAuth 2.0 / OpenID Connect (OIDC) scenarios because public keys can be published at a JWKS endpoint and rotated without coordination.
- ES256 provides the same asymmetric security as RS256 but with significantly smaller keys and faster operations, making it preferred for mobile-facing or high-throughput APIs.

---

## Q7. What are the advantages of RS256 over HS256 in a microservices architecture?

What are the advantages of RS256 over HS256 in a microservices architecture?

**Answer:** In a microservices architecture, many independent services may need to validate tokens issued by a central identity server. RS256 allows the identity server to sign tokens with its private key while each downstream service holds only the public key for verification — the private signing key never leaves the identity server. With HS256, every service that validates tokens must also hold the shared secret, meaning that a breach of any single service compromises the entire system's ability to trust tokens.

- With RS256, adding a new downstream service does not require re-sharing a secret — the service fetches the public key from a JWKS endpoint, reducing operational overhead and secret sprawl.
- Key rotation is straightforward with RS256: the identity server publishes new public keys at the JWKS endpoint, existing tokens signed with the old key remain valid until they expire (old keys stay in the set temporarily), and new tokens are signed with the new private key.
- HS256 key rotation is disruptive because all services holding the old secret must be updated simultaneously or tokens issued before and after rotation become unverifiable during the transition window.

---

## Q8. How do you add JWT bearer authentication to an ASP.NET Core application?

How do you add JWT bearer authentication to an ASP.NET Core application?

**Answer:** JWT bearer authentication is added in `Program.cs` (or `Startup.cs` in older projects) by calling `AddAuthentication` with a default scheme of `JwtBearerDefaults.AuthenticationScheme` and then chaining `AddJwtBearer` to configure validation parameters. You must also call `UseAuthentication()` and `UseAuthorization()` in the middleware pipeline, in that order, before any endpoint mapping.

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = "https://auth.example.com",
            ValidateAudience         = true,
            ValidAudience            = "https://api.example.com",
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        };
    });
```

- `UseAuthentication()` must come before `UseAuthorization()` in the pipeline; reversing them means the identity is never populated before authorization decisions are made.
- The `Authorization: Bearer <token>` header is automatically extracted and validated by the middleware — individual endpoints do not need to parse the header manually.
- For RS256, replace `IssuerSigningKey` with a `RsaSecurityKey` constructed from the public key, or configure `options.Authority` to point to the OpenID Connect discovery document so the middleware fetches keys automatically.

---

## Q9. What is `JwtBearerOptions`, and which properties do you configure most often?

What is `JwtBearerOptions`, and which properties do you configure most often?

**Answer:** `JwtBearerOptions` is the configuration object passed to `AddJwtBearer` that controls how the JWT middleware behaves — how it finds the token in the request, how it validates it, and what events it fires during the process. The most critical inner property is `TokenValidationParameters`, but `JwtBearerOptions` itself also controls token extraction and error reporting.

- `TokenValidationParameters` is the core property; it carries all validation flags (issuer, audience, lifetime, signing key) and determines what constitutes a valid token.
- `SaveToken` controls whether the raw JWT string is stored in `HttpContext.Authentication` after successful validation; setting it to `true` allows you to retrieve the raw token string later in the request without re-parsing it.
- `Events` exposes hooks like `OnTokenValidated`, `OnAuthenticationFailed`, and `OnChallenge` where you can add custom logic such as logging failures, reading tokens from cookies, or customizing the 401 response body.
- `Authority` can be set to an OpenID Connect issuer URL, causing the middleware to automatically download and cache signing keys from the discovery document (`/.well-known/openid-configuration`), which simplifies key rotation management.

---

## Q10. What is `TokenValidationParameters`, and which validation flags are critical for production?

What is `TokenValidationParameters`, and which validation flags are critical for production?

**Answer:** `TokenValidationParameters` is a class from the `Microsoft.IdentityModel.Tokens` namespace that instructs the JWT middleware (or `JwtSecurityTokenHandler`) on which aspects of a token to validate before accepting it. Disabling any of the critical flags weakens the security contract of the token and can allow replayed, expired, or misdirected tokens to be accepted.

| Property | Default | Production Value | Why It Matters |
|----------|---------|-----------------|----------------|
| `ValidateIssuer` | `true` | `true` | Prevents tokens from foreign issuers being accepted |
| `ValidateAudience` | `true` | `true` | Prevents tokens intended for another service from being accepted |
| `ValidateLifetime` | `true` | `true` | Rejects expired tokens |
| `ValidateIssuerSigningKey` | `false` | `true` | Verifies the signature with the expected key |
| `ClockSkew` | 5 min | `TimeSpan.Zero` or small value | Tightens expiry enforcement |

- `ValidateIssuerSigningKey` being `false` by default is a common trap — without it, the signature is not verified against the key you configured, so any token with a structurally valid signature is accepted.
- `ClockSkew` defaults to five minutes to tolerate clock drift between servers; in a well-synchronized environment, reducing it to zero or thirty seconds tightens expiry enforcement.

---

## Q11. Why is `ValidateLifetime` important, and what happens if you disable it?

Why is `ValidateLifetime` important, and what happens if you disable it?

**Answer:** `ValidateLifetime` instructs the token handler to check the `exp` (expiration) and `nbf` (not before) claims against the current time; if disabled, the middleware will accept tokens regardless of when they were issued or whether they have expired. A short token lifetime is one of the primary mitigations against token theft — even if an attacker obtains a valid token, it will stop working after the expiry window.

- With `ValidateLifetime` disabled, a stolen token becomes permanently valid as long as the signing key is unchanged, transforming a temporary compromise into a long-term one.
- Disabling lifetime validation is sometimes done in development to avoid token expiry interrupting debugging sessions, but this setting must never reach production.
- The `ClockSkew` property works alongside `ValidateLifetime` to add tolerance for clock drift between the issuing server and the validating server; the token is accepted for up to `ClockSkew` seconds after its `exp` timestamp.

---

## Q12. Why must both `ValidateIssuer` and `ValidateAudience` be set to `true` in production?

Why must both `ValidateIssuer` and `ValidateAudience` be set to `true` in production?

**Answer:** Validating the issuer ensures that only tokens created by the trusted identity server are accepted, preventing a token from an unrelated or malicious authority from being used. Validating the audience ensures that a token intended for a specific service cannot be replayed against a different service that shares the same signing key. Both validations are necessary because a token that passes issuer validation alone could still be misused across services.

- The audience confusion attack works like this: if Service A and Service B both trust the same identity server but do not validate audience, a token issued for Service A can be presented to Service B and accepted, granting access that was never intended.
- In multi-tenant or multi-service systems, the `aud` claim is the primary boundary that isolates tokens between services, so disabling audience validation collapses that boundary.
- The `iss` claim is equally important in federated scenarios where multiple identity providers exist — without issuer validation, a token from a less-secure issuer could be accepted by a service intended to trust only a high-security issuer.

---

## Q13. How do you generate a JWT in .NET using `JwtSecurityTokenHandler`?

How do you generate a JWT in .NET using `JwtSecurityTokenHandler`?

**Answer:** Token generation uses `JwtSecurityTokenHandler` from the `System.IdentityModel.Tokens.Jwt` package. You construct a `SecurityTokenDescriptor` that carries the claims, lifetime, signing credentials, and issuer/audience, then call `CreateToken` and serialize it to a string.

```csharp
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var descriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId),
        new Claim(ClaimTypes.Role, "Admin")
    }),
    Expires   = DateTime.UtcNow.AddMinutes(15),
    Issuer    = "https://auth.example.com",
    Audience  = "https://api.example.com",
    SigningCredentials = creds
};

var handler = new JwtSecurityTokenHandler();
string token = handler.CreateEncodedJwt(descriptor);
```

- Always use `DateTime.UtcNow` for the `Expires` field; using local time causes expiry miscalculations when the server's timezone differs from UTC.
- `CreateEncodedJwt` directly returns the Base64URL-encoded string; `CreateToken` returns a `SecurityToken` object that you then pass to `WriteToken` if you need the string form separately.
- The `SigningCredentials` object pairs the key material with the algorithm identifier; using `SecurityAlgorithms.HmacSha256` ensures the header's `alg` field is set to `HS256`.

---

## Q14. What claims should you include in a JWT for an ASP.NET Core Identity-backed API?

What claims should you include in a JWT for an ASP.NET Core Identity-backed API?

**Answer:** For an API backed by ASP.NET Core Identity, the JWT should carry claims that identify the user uniquely and convey enough authorization context so that downstream services do not need to call the identity store on every request. The standard registered claims (`sub`, `jti`, `iat`, `exp`) should always be present; Identity-specific information is added as additional claims.

- `sub` should be set to the user's stable unique identifier (typically `user.Id` from `IdentityUser`), not the username, because usernames can change.
- Role claims (`ClaimTypes.Role` or `role`) should be added from `UserManager.GetRolesAsync(user)` so that `[Authorize(Roles = "Admin")]` works without a database query per request.
- A `jti` (JWT ID) claim generated with `Guid.NewGuid()` enables per-token revocation tracking if you choose to implement a token denylist.
- Avoid including sensitive or volatile data (email, phone) in the token payload because the token is signed but not encrypted, and claims become stale as soon as the user updates their profile without re-issuing a token.

---

## Q15. How does ASP.NET Core Identity integrate with JWT generation — what role does `UserManager` play?

How does ASP.NET Core Identity integrate with JWT generation — what role does `UserManager` play?

**Answer:** ASP.NET Core Identity's `UserManager<TUser>` provides the data access layer for user information and does not generate JWTs itself — JWT generation is implemented in application code, typically in a service or controller that combines Identity data retrieval with `JwtSecurityTokenHandler`. The typical flow is: authenticate the user's credentials via `UserManager.CheckPasswordAsync`, retrieve roles and claims from the Identity store, then build and sign the token.

- `UserManager.GetRolesAsync(user)` returns the user's roles stored in the Identity database, which are then translated into role claims added to the token payload.
- `UserManager.GetClaimsAsync(user)` retrieves any custom claims stored in the `AspNetUserClaims` table, allowing fine-grained permissions to be embedded in the token without an additional database call per request.
- Identity's built-in cookie authentication and JWT authentication are entirely separate mechanisms — adding JWT bearer auth does not replace cookie auth; both can coexist in the same application serving different endpoint groups.

---

## Q16. What is the refresh token pattern, and why are access tokens intentionally short-lived?

What is the refresh token pattern, and why are access tokens intentionally short-lived?

**Answer:** The refresh token pattern separates token concerns into two complementary pieces: a short-lived access token (typically 5–15 minutes) that gates API requests, and a long-lived refresh token (hours to days) that can be exchanged for a new access token without requiring the user to re-enter credentials. Access tokens are short-lived because JWTs are stateless and cannot be individually revoked once issued — a short lifetime limits the damage window if a token is stolen.

- When the access token expires, the client sends the refresh token to a dedicated `/refresh` endpoint; the server validates the refresh token against a database record, issues a new access token, and optionally rotates the refresh token to invalidate the previous one.
- Refresh token rotation means each refresh token is used only once — after exchange it is marked consumed and a new one is issued, so a stolen refresh token is detected the next time the legitimate client tries to use it (the server sees both a used and an active token, indicating a theft).
- Unlike access tokens, refresh tokens must be stored server-side so they can be revoked — this is the one part of JWT-based auth that requires server state.

---

## Q17. How should refresh tokens be stored and validated on the server side?

How should refresh tokens be stored and validated on the server side?

**Answer:** Refresh tokens should be stored in the database as hashed values (not plaintext), associated with the user ID, the device or session that issued them, an expiry timestamp, and a revocation flag. When a client presents a refresh token, the server hashes the incoming value, queries the database for a matching record, checks that it has not expired or been revoked, then issues a new access token.

- Storing the raw refresh token in the database is analogous to storing plaintext passwords — if the table is leaked, all refresh tokens are immediately usable; hashing with SHA-256 or a similar algorithm limits exposure.
- Each refresh token record should include a family ID to detect refresh token reuse attacks: if a consumed token from a family is presented, the entire family (all sessions for that user) should be revoked as a breach response.
- The database record should also track the client IP or device fingerprint as a soft signal — a refresh from a completely different geography can trigger step-up authentication even if the token itself is valid.

---

## Q18. What is the token revocation problem with JWTs, and what strategies exist to mitigate it?

What is the token revocation problem with JWTs, and what strategies exist to mitigate it?

**Answer:** JWTs are stateless by design — once issued, the server has no built-in mechanism to invalidate a specific token before it expires naturally. This means that if a user logs out, changes their password, or has their account suspended, existing access tokens remain valid until expiry. This is the token revocation problem, and it is a fundamental trade-off of stateless tokens.

- **Short expiry** is the simplest mitigation: if access tokens expire in five minutes, the window of unauthorized access after a credential change is at most five minutes without any additional infrastructure.
- **Token denylist (blocklist):** The server maintains a store (typically Redis for performance) of revoked token identifiers (`jti` claims); on every request, the middleware checks the `jti` against the denylist before accepting the token. This reintroduces server state but allows immediate revocation.
- **Token version on user record:** A `tokenVersion` integer is stored in the user record and embedded in the JWT; on every request, the API checks that the token's version matches the current value in the database. Incrementing the version on logout immediately invalidates all outstanding tokens for that user.
- Each mitigation trades some of the stateless scalability of JWTs for revocability; choose based on how critical immediate revocation is for the application's threat model.

---

## Q19. Should JWTs be stored in `localStorage` or an `HttpOnly` cookie? What are the security trade-offs?

Should JWTs be stored in `localStorage` or an `HttpOnly` cookie? What are the security trade-offs?

**Answer:** Storing JWTs in `localStorage` makes them accessible to JavaScript running on the page, which means a Cross-Site Scripting (XSS) attack can exfiltrate the token directly. Storing JWTs in `HttpOnly` cookies makes them inaccessible to JavaScript, mitigating XSS-based token theft, but introduces Cross-Site Request Forgery (CSRF) risk because browsers automatically send cookies with cross-origin requests.

| Storage | XSS Risk | CSRF Risk | Access from JS | Notes |
|---------|----------|-----------|----------------|-------|
| `localStorage` | High — token readable by any script | Low | Yes | Simple for SPAs, dangerous if any XSS exists |
| `sessionStorage` | High — same as localStorage | Low | Yes | Cleared on tab close, still JS-accessible |
| `HttpOnly` Cookie | Low — JS cannot read it | High — mitigated with SameSite/CSRF tokens | No | Recommended for web apps |
| Memory (JS variable) | Medium — lost on refresh | Low | Yes | Best XSS profile but poor UX |

- An `HttpOnly` cookie combined with `SameSite=Strict` or `SameSite=Lax` and a CSRF token provides the strongest protection for browser-based clients.
- For native mobile and desktop clients, storage in the platform's secure keychain (iOS Keychain, Android Keystore) is the equivalent of `HttpOnly` cookies — isolated from other apps.
- Content Security Policy (CSP) headers significantly reduce XSS risk even when `localStorage` is used, but they do not eliminate it; defence in depth requires both.

---

## Q20. How does the `[Authorize]` attribute work with bearer tokens in ASP.NET Core?

How does the `[Authorize]` attribute work with bearer tokens in ASP.NET Core?

**Answer:** When a request reaches a controller or action marked with `[Authorize]`, the ASP.NET Core authorization middleware checks whether the current `HttpContext.User` principal is authenticated. The JWT bearer middleware, which runs earlier in the pipeline, has already extracted the `Authorization: Bearer <token>` header, validated the token, and populated `HttpContext.User` with a `ClaimsPrincipal` from the token's claims. If the principal is not authenticated, the middleware returns a 401 Unauthorized response; if authenticated but not authorized (e.g., missing a required role), it returns 403 Forbidden.

- The `[Authorize]` attribute with no parameters requires only that the user is authenticated — any valid JWT passes this check.
- `[Authorize(Roles = "Admin")]` additionally checks that the principal has a claim of type `ClaimTypes.Role` with the value `"Admin"`, which the JWT middleware populates from the role claims in the token payload.
- `[AllowAnonymous]` on an action overrides `[Authorize]` at the controller level, allowing unauthenticated access to that specific action regardless of the controller-level policy.

---

## Q21. How do you define and enforce policy-based authorization alongside JWT bearer authentication?

How do you define and enforce policy-based authorization alongside JWT bearer authentication?

**Answer:** Policy-based authorization in ASP.NET Core allows you to express complex access rules beyond simple role checks by defining named policies in `AddAuthorization` and referencing them with `[Authorize(Policy = "PolicyName")]`. Policies are composed of one or more requirements, each fulfilled by a corresponding `AuthorizationHandler`. JWT claims serve as the data source for requirement evaluation.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SeniorEmployee", policy =>
        policy.RequireClaim("department", "Engineering")
              .RequireClaim("yearsOfService", "5", "6", "7", "8", "9", "10"));
});
```

- Policy requirements can access any claim in `HttpContext.User`, meaning any claim embedded in the JWT (such as `tenant_id`, `plan`, or `clearance_level`) can drive authorization decisions without a database query.
- Custom `IAuthorizationRequirement` and `AuthorizationHandler<T>` pairs allow arbitrary logic, including calling a database or external service, when claim-based rules alone are insufficient.
- Policies declared with `FallbackPolicy` apply to all endpoints that have no explicit authorization attribute, providing a deny-by-default posture without decorating every action individually.

---

## Q22. What role does CORS (Cross-Origin Resource Sharing) play in API security when using JWT?

What role does CORS (Cross-Origin Resource Sharing) play in API security when using JWT?

**Answer:** CORS (Cross-Origin Resource Sharing) is a browser security feature that restricts which origins (domains, protocols, ports) can make cross-origin HTTP requests to your API. When a Single-Page Application (SPA) on `https://app.example.com` calls an API at `https://api.example.com`, the browser sends a preflight `OPTIONS` request to check whether the API allows cross-origin requests from that origin; without proper CORS configuration the browser blocks the actual request. CORS does not protect against server-to-server requests or non-browser clients — it is a browser enforcement mechanism only.

- CORS policy must explicitly allow the `Authorization` header; if it is not listed in `AllowedHeaders`, the browser strips it from the cross-origin request and the JWT never reaches the API.
- Using `AllowAnyOrigin()` combined with `AllowCredentials()` is explicitly forbidden by the CORS specification; you must name specific origins when credentials (cookies or authorization headers) are included.
- CORS misconfiguration (reflecting the request's `Origin` header without restriction) effectively disables same-origin protection, allowing malicious sites to make authenticated API calls on behalf of logged-in users.

---

## Q23. How do you configure CORS in ASP.NET Core so that the `Authorization` header is allowed?

How do you configure CORS in ASP.NET Core so that the `Authorization` header is allowed?

**Answer:** CORS is configured by registering a named policy in `AddCors` and applying it either globally with `UseCors` or per-endpoint with the `[EnableCors]` attribute. To allow the `Authorization` header, you must call `WithExposedHeaders` or `AllowAnyHeader` (or list `Authorization` explicitly in `WithHeaders`) within the policy definition.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCors", policy =>
        policy.WithOrigins("https://app.example.com")
              .WithHeaders(HeaderNames.Authorization, HeaderNames.ContentType)
              .WithMethods("GET", "POST", "PUT", "DELETE"));
});

app.UseCors("ApiCors"); // must come before UseAuthentication and UseAuthorization
```

- `UseCors` must be placed before `UseAuthentication` and `UseAuthorization` in the pipeline so that the CORS headers are added to the response before the authentication middleware produces a 401, otherwise preflight requests fail with no CORS headers on the error response.
- For browsers to read the `Authorization` response header (e.g., a token returned in a response header), you must also call `WithExposedHeaders(HeaderNames.Authorization)`.

---

## Q24. How do you pass custom claims (e.g., roles, tenant ID) in a JWT and read them inside a controller?

How do you pass custom claims (e.g., roles, tenant ID) in a JWT and read them inside a controller?

**Answer:** Custom claims are added to the token payload at generation time by including additional `Claim` objects in the `Subject` of the `SecurityTokenDescriptor`. Once the JWT middleware validates the token, all claims are available on `HttpContext.User` as a `ClaimsPrincipal`, and can be accessed in a controller via `User.FindFirstValue(claimType)` or `User.Claims`.

```csharp
// At token generation
new Claim("tenant_id", tenantId),
new Claim("plan", "Enterprise")

// In a controller action
string tenantId = User.FindFirstValue("tenant_id");
bool isEnterprise = User.HasClaim("plan", "Enterprise");
```

- Claim type strings are arbitrary, but using registered JWT claim names (lowercase, no namespace) keeps the token compact and interoperable with other tooling.
- Role claims added with `ClaimTypes.Role` (the long XML-namespaced name) are mapped to the short `role` key in the JWT payload by the default handler; the `RoleClaimType` property on `TokenValidationParameters` can override this mapping.
- Large numbers of claims increase token size, which matters for HTTP headers — browsers typically cap total header size around 8 KB, so avoid embedding large lists in the JWT; prefer referencing them by ID and fetching from cache.

---

## Q25. What happens when a JWT expires mid-request? How can clients detect and handle this transparently?

What happens when a JWT expires mid-request? How can clients detect and handle this transparently?

**Answer:** If a JWT expires before the server processes a request, the JWT bearer middleware rejects the token and returns a 401 Unauthorized response with a `WWW-Authenticate: Bearer error="invalid_token"` header. The request does not proceed to the controller. Clients that implement transparent token refresh intercept 401 responses, use the refresh token to obtain a new access token, and replay the original request with the new token — all without the user noticing.

- The 401 response status combined with the `invalid_token` error in the `WWW-Authenticate` header is the signal that distinguishes an expired token from a missing-credentials 401, allowing the client to attempt refresh rather than redirect to login.
- `ClockSkew` on `TokenValidationParameters` adds a grace period (default five minutes) after the `exp` timestamp during which the server still accepts the token; this prevents failures caused by minor clock drift between client and server.
- Libraries like `axios` (for JavaScript SPAs) support request interceptors that automatically queue failed requests, refresh the token once, and retry all queued requests, avoiding the race condition where multiple concurrent requests all attempt to refresh simultaneously.

---

## Q26. How do you validate a JWT manually without the ASP.NET Core middleware pipeline?

How do you validate a JWT manually without the ASP.NET Core middleware pipeline?

**Answer:** You can validate a JWT outside of the middleware pipeline by using `JwtSecurityTokenHandler.ValidateToken`, passing the raw token string and a `TokenValidationParameters` instance. The method returns a `ClaimsPrincipal` if validation succeeds, or throws a `SecurityTokenException` subclass describing the failure.

```csharp
var handler = new JwtSecurityTokenHandler();
var parameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = signingKey,
    ValidateIssuer   = true, ValidIssuer   = "https://auth.example.com",
    ValidateAudience = true, ValidAudience = "https://api.example.com",
    ValidateLifetime = true
};

ClaimsPrincipal principal = handler.ValidateToken(rawToken, parameters, out SecurityToken validated);
```

- This pattern is useful in background services, message consumers, or inter-service calls where an HTTP pipeline is not present and you need to validate a token carried in a message header or queue payload.
- The `out SecurityToken validated` parameter gives you access to the parsed token object (cast to `JwtSecurityToken`) so you can inspect individual claims or the token's `ValidTo` property without a second parse.
- Wrap the call in a try/catch for `SecurityTokenExpiredException`, `SecurityTokenInvalidSignatureException`, and the base `SecurityTokenException` to handle different failure modes distinctly.

---

## Q27. What is a JWK Set (JWKS) endpoint, and how does it enable key rotation without redeploying services?

What is a JWK Set (JWKS) endpoint, and how does it enable key rotation without redeploying services?

**Answer:** A JSON Web Key Set (JWKS) is a JSON document published at a well-known URL (typically `/.well-known/jwks.json`) that contains the public keys the identity server uses to sign tokens, expressed in the JSON Web Key (JWK) format. Verifying services periodically fetch this document to obtain the current public keys, so when the identity server rotates its signing key pair, services automatically pick up the new public key without any configuration change or redeployment.

- Each key in the JWKS document carries a `kid` (key ID) field; the JWT header also includes a `kid` that tells the verifier which key in the set to use for verification, allowing multiple keys to coexist during a rotation window.
- Key rotation works gracefully because the old public key is kept in the JWKS document until all tokens signed with the old private key have expired; new tokens are signed with the new private key, and each token's `kid` header directs the verifier to the correct key.
- ASP.NET Core's JWT bearer middleware automatically discovers and caches keys from the JWKS endpoint when you configure `options.Authority` to point to an OpenID Connect-compliant issuer, and it periodically refreshes the cache to handle key rotation.

---

## Q28. What are the most common security mistakes developers make when implementing JWT authentication?

What are the most common security mistakes developers make when implementing JWT authentication?

**Answer:** JWT authentication has several well-known pitfalls that regularly appear in security audits. The most impactful mistakes involve disabling validation flags during development and forgetting to re-enable them, misunderstanding the confidentiality guarantees of JWT, and insufficient token lifecycle management.

- Disabling `ValidateIssuer`, `ValidateAudience`, or `ValidateLifetime` for development convenience and deploying with those settings disabled is one of the most frequent and dangerous mistakes — it turns JWTs from a secure mechanism into a trivially forgeable one.
- Assuming the JWT payload is secret because it is "encoded" and storing sensitive data (passwords, PII, internal system identifiers) in the payload; Base64URL decoding requires no key and is reversible by anyone.
- Using a weak or short signing secret for HS256, or hard-coding the secret in source code where it can be leaked via version control; secrets should be stored in environment variables or a secrets manager and rotated regularly.
- Setting very long access token expiry times (days or weeks) to avoid implementing refresh tokens; this negates the primary security benefit of short-lived tokens and extends the breach window indefinitely.
- Not implementing token revocation (or at minimum short lifetimes) for high-risk operations such as password changes, role changes, or account suspension, meaning a compromised token remains valid long after the underlying conditions that justified it have changed.

---

## Gotchas — JWT & API Security (Interview Traps)

---

#### Gotcha 1. JWT payload is Base64URL-encoded, not encrypted

**Answer:** A common misconception is that because a JWT looks like random characters, its contents are secret. In reality, the payload is Base64URL-encoded, which is a reversible text encoding requiring no key — any party who holds the token can decode and read every claim instantly.

- Treating JWT as confidential and storing sensitive data (passwords, SSNs, internal system paths) in the payload exposes that data to anyone who intercepts or is handed the token.
- If payload confidentiality is required, use JWE (JSON Web Encryption) rather than JWS (JSON Web Signature), or move sensitive data server-side and reference it by a non-guessable identifier.

---

#### Gotcha 2. `ValidateIssuerSigningKey` defaults to `false`

**Answer:** The `TokenValidationParameters.ValidateIssuerSigningKey` property defaults to `false` in the .NET libraries, meaning that without explicitly setting it to `true`, the middleware verifies the token's structure but does not confirm the signature against your configured key. This means any structurally valid JWT — even one signed with a completely different key — would be accepted.

- Setting `IssuerSigningKey` alone is not sufficient; you must also set `ValidateIssuerSigningKey = true` to activate key verification.
- This default was a source of multiple security vulnerabilities in production systems where developers configured the key but did not realize signature verification was still disabled.

---

#### Gotcha 3. `UseAuthentication` must come before `UseAuthorization` in the middleware pipeline

**Answer:** ASP.NET Core's middleware pipeline is ordered and sequential — each middleware runs in the order it is registered. If `UseAuthorization` runs before `UseAuthentication`, the `HttpContext.User` principal has not yet been populated from the JWT, so authorization decisions are always made against an anonymous (unauthenticated) principal and `[Authorize]` always returns 401.

- The correct order is `UseRouting` → `UseCors` → `UseAuthentication` → `UseAuthorization` → `MapControllers` (or endpoint mapping).
- This is a runtime behaviour bug that does not produce a compile-time error or startup exception, making it easy to miss until integration testing reveals that all protected endpoints return 401 even with a valid token.

---

#### Gotcha 4. JWTs cannot be revoked without additional infrastructure

**Answer:** Because JWT validation is stateless — the server checks only the signature, claims, and lifetime without querying a database — there is no built-in mechanism to invalidate a specific token before it expires. Developers sometimes assume that "logging out" by deleting the token from the client is equivalent to server-side session invalidation, but any copy of the token that still exists (in a log, in an attacker's possession) remains fully valid.

- True revocation requires server state: either a denylist of revoked `jti` values, a per-user token version number, or enforcing very short token lifetimes so the effective revocation window is acceptably small.
- This fundamental limitation is the most important trade-off to understand when choosing between JWT-based stateless authentication and server-side sessions.

---

#### Gotcha 5. CORS `AllowAnyOrigin` and `AllowCredentials` cannot be combined

**Answer:** The CORS specification explicitly prohibits responding with `Access-Control-Allow-Origin: *` when the request includes credentials (cookies or `Authorization` headers). ASP.NET Core enforces this at runtime — calling `AllowAnyOrigin().AllowCredentials()` together throws an `InvalidOperationException` at startup.

- The correct approach when credentials are required is to use `WithOrigins("https://app.example.com")` to name specific allowed origins rather than using the wildcard.
- Developers sometimes work around the startup exception by setting the `Access-Control-Allow-Origin` header manually in a middleware, bypassing the policy system entirely; this is an insecure pattern that can inadvertently reflect arbitrary origins.

---
