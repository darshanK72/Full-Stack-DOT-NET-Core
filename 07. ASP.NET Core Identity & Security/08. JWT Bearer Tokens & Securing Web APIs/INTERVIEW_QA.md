# JWT Bearer Tokens & Securing Web APIs — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a JSON Web Token (JWT), and what are its three structural parts?](#q1-what-is-a-json-web-token-jwt-and-what-are-its-three-structural-parts)
2. [Q2. What information is stored in the JWT header, and why does the algorithm choice matter?](#q2-what-information-is-stored-in-the-jwt-header-and-why-does-the-algorithm-choice-matter)
3. [Q3. What are the standard claims in a JWT payload (sub, iss, aud, exp, iat), and what does each represent?](#q3-what-are-the-standard-claims-in-a-jwt-payload-sub-iss-aud-exp-iat-and-what-does-each-represent)
4. [Q4. What is the difference between signing and encrypting a JWT? Is the payload visible to anyone who holds the token?](#q4-what-is-the-difference-between-signing-and-encrypting-a-jwt-is-the-payload-visible-to-anyone-who-holds-the-token)
5. [Q5. How does JWT signature verification work, and what does it guarantee?](#q5-how-does-jwt-signature-verification-work-and-what-does-it-guarantee)
6. [Q6. Compare HS256, RS256, and ES256 signing algorithms. When would you choose each?](#q6-compare-hs256-rs256-and-es256-signing-algorithms-when-would-you-choose-each)
7. [Q7. What are the advantages of RS256 over HS256 in a microservices architecture?](#q7-what-are-the-advantages-of-rs256-over-hs256-in-a-microservices-architecture)
8. [Q8. How do you add JWT bearer authentication to an ASP.NET Core application?](#q8-how-do-you-add-jwt-bearer-authentication-to-an-aspnet-core-application)
9. [Q9. What is `JwtBearerOptions`, and which properties do you configure most often?](#q9-what-is-jwtbeareroptions-and-which-properties-do-you-configure-most-often)
10. [Q10. What is `TokenValidationParameters`, and which validation flags are critical for production?](#q10-what-is-tokenvalidationparameters-and-which-validation-flags-are-critical-for-production)
11. [Q11. Why is `ValidateLifetime` important, and what happens if you disable it?](#q11-why-is-validatelifetime-important-and-what-happens-if-you-disable-it)
12. [Q12. Why must both `ValidateIssuer` and `ValidateAudience` be set to `true` in production?](#q12-why-must-both-validateissuer-and-validateaudience-be-set-to-true-in-production)
13. [Q13. How do you generate a JWT in .NET using `JwtSecurityTokenHandler`?](#q13-how-do-you-generate-a-jwt-in-net-using-jwtsecuritytokenhandler)
14. [Q14. What claims should you include in a JWT for an ASP.NET Core Identity-backed API?](#q14-what-claims-should-you-include-in-a-jwt-for-an-aspnet-core-identity-backed-api)
15. [Q15. How does ASP.NET Core Identity integrate with JWT generation — what role does `UserManager` play?](#q15-how-does-aspnet-core-identity-integrate-with-jwt-generation-what-role-does-usermanager-play)
16. [Q16. What is the refresh token pattern, and why are access tokens intentionally short-lived?](#q16-what-is-the-refresh-token-pattern-and-why-are-access-tokens-intentionally-short-lived)
17. [Q17. How should refresh tokens be stored and validated on the server side?](#q17-how-should-refresh-tokens-be-stored-and-validated-on-the-server-side)
18. [Q18. What is the token revocation problem with JWTs, and what strategies exist to mitigate it?](#q18-what-is-the-token-revocation-problem-with-jwts-and-what-strategies-exist-to-mitigate-it)
19. [Q19. Should JWTs be stored in `localStorage` or an `HttpOnly` cookie? What are the security trade-offs?](#q19-should-jwts-be-stored-in-localstorage-or-an-httponly-cookie-what-are-the-security-trade-offs)
20. [Q20. How does the `[Authorize]` attribute work with bearer tokens in ASP.NET Core?](#q20-how-does-the-authorize-attribute-work-with-bearer-tokens-in-aspnet-core)
21. [Q21. How do you define and enforce policy-based authorization alongside JWT bearer authentication?](#q21-how-do-you-define-and-enforce-policy-based-authorization-alongside-jwt-bearer-authentication)
22. [Q22. What role does CORS (Cross-Origin Resource Sharing) play in API security when using JWT?](#q22-what-role-does-cors-cross-origin-resource-sharing-play-in-api-security-when-using-jwt)
23. [Q23. How do you configure CORS in ASP.NET Core so that the `Authorization` header is allowed?](#q23-how-do-you-configure-cors-in-aspnet-core-so-that-the-authorization-header-is-allowed)
24. [Q24. How do you pass custom claims (e.g., roles, tenant ID) in a JWT and read them inside a controller?](#q24-how-do-you-pass-custom-claims-eg-roles-tenant-id-in-a-jwt-and-read-them-inside-a-controller)
25. [Q25. What happens when a JWT expires mid-request? How can clients detect and handle this transparently?](#q25-what-happens-when-a-jwt-expires-mid-request-how-can-clients-detect-and-handle-this-transparently)
26. [Q26. How do you validate a JWT manually without the ASP.NET Core middleware pipeline?](#q26-how-do-you-validate-a-jwt-manually-without-the-aspnet-core-middleware-pipeline)
27. [Q27. What is a JWK Set (JWKS) endpoint, and how does it enable key rotation without redeploying services?](#q27-what-is-a-jwk-set-jwks-endpoint-and-how-does-it-enable-key-rotation-without-redeploying-services)
28. [Q28. What are the most common security mistakes developers make when implementing JWT authentication?](#q28-what-are-the-most-common-security-mistakes-developers-make-when-implementing-jwt-authentication)

---

## Q1. What is a JSON Web Token (JWT), and what are its three structural parts?

**Concepts**
- Self-contained token — no server session lookup needed
- Base64URL encoding vs encryption
- Three-part dot-separated structure: header, payload, signature
- Signature as the sole integrity and authenticity guarantee

**Answer**

A JWT is a compact, URL-safe string that carries all the information needed for verification within the token itself, which is why servers can authenticate a request by inspecting the token alone rather than querying a session store on every call. The three parts are separated by dots and each is Base64URL-encoded. The header declares the token type and the signing algorithm used. The payload carries the claims — pieces of information about the subject, issuer, expiry, and any custom application data. The signature is produced by running the encoded header and payload through the signing algorithm with a secret or private key, which is what allows the receiver to verify authenticity and detect any tampering after issuance.

---

## Q2. What information is stored in the JWT header, and why does the algorithm choice matter?

**Concepts**
- `alg` and `typ` header fields
- Symmetric HS256 — any verifier can also forge
- Asymmetric RS256 — public key sufficient to verify, private key stays with issuer
- "none" algorithm attack and algorithm pinning in TokenValidationParameters

**Answer**

The JWT header is a JSON object containing `alg`, which specifies the signing algorithm such as `HS256` or `RS256`, and `typ`, which is always `JWT`. The algorithm choice matters because it determines the cryptographic model for verification. A symmetric algorithm like `HS256` uses the same shared secret to both sign and verify — any party that can verify can also forge tokens, which is a problem when multiple services need to validate tokens independently. An asymmetric algorithm like `RS256` uses a private key to sign and a public key to verify, so resource servers can validate without holding the signing secret. Historically, some libraries trusted the `alg` field in the token header, leading to the "none" algorithm attack where a crafted token with `"alg":"none"` could bypass signature verification; modern libraries require acceptable algorithms to be declared explicitly in `TokenValidationParameters` rather than reading the value from the token itself.

---

## Q3. What are the standard claims in a JWT payload (sub, iss, aud, exp, iat), and what does each represent?

**Concepts**
- RFC 7519 registered claim names
- `exp` as primary token theft mitigation
- `aud` and `iss` together preventing audience confusion attacks
- `jti` for per-token revocation tracking

**Answer**

JWT defines a set of registered claim names in RFC 7519 that carry semantically meaningful information about the token and its subject, enabling interoperability across standard validation libraries.

| Claim | Full Name        | Meaning |
|-------|------------------|---------|
| `sub` | Subject          | Unique identifier of the principal the token represents (e.g., user ID) |
| `iss` | Issuer           | URI or name of the server that created and signed the token |
| `aud` | Audience         | Intended recipient(s) — the service(s) that should accept this token |
| `exp` | Expiration Time  | Unix timestamp after which the token must be rejected |
| `iat` | Issued At        | Unix timestamp when the token was created |
| `nbf` | Not Before       | Unix timestamp before which the token is not yet valid |
| `jti` | JWT ID           | Unique identifier for this token; used for revocation tracking |

The `exp` claim is the primary defence against token theft — a short expiry window limits the damage an attacker can do with a stolen token. The `iss` and `aud` claims together ensure a token issued for Service A cannot be replayed against Service B, which is the audience confusion attack.

---

## Q4. What is the difference between signing and encrypting a JWT? Is the payload visible to anyone who holds the token?

**Concepts**
- JWS (signed) vs JWE (encrypted)
- Base64URL as reversible encoding with no security properties
- Signing guarantees integrity but not confidentiality
- PII and sensitive data requiring JWE rather than JWS

**Answer**

Signing a JWT produces a JWS (JSON Web Signature), which guarantees integrity and authenticity — the receiver can confirm the token has not been altered and was created by the expected party. Encrypting a JWT produces a JWE (JSON Web Encryption), which protects confidentiality so the payload is unreadable to anyone without the decryption key. By default, JWTs in practice are only signed, not encrypted, and the payload is Base64URL-encoded rather than encrypted. Base64URL is a reversible text encoding with no security properties — anyone who intercepts the token can decode and read every claim instantly without any key. Sensitive information such as passwords, payment details, or PII must therefore never be placed in a standard JWT payload unless the token is also encrypted. The signature does prevent tampering: if an attacker modifies any claim, verification will fail, so the data is integrity-protected even though it is not confidential.

---

## Q5. How does JWT signature verification work, and what does it guarantee?

**Concepts**
- Re-signing header+payload and comparing to embedded signature
- Integrity and authenticity guaranteed — token validity checks are separate
- HMAC shared-secret risk vs RSA/ECDSA public-key distribution model
- Expiry and audience checks as independent steps

**Answer**

When a receiver gets a JWT, it takes the encoded header and payload, re-runs the same signing algorithm using the verification key, and compares the result to the signature embedded in the token. If the two match, the token has not been tampered with since it was issued. Verification guarantees integrity and authenticity, but it does not guarantee the token is still valid for use — expiry and audience checks are separate steps that must also pass. For HMAC-based algorithms, any party with the shared secret can both sign and verify, so leaking the secret allows attackers to mint arbitrary tokens. For RSA and ECDSA algorithms, only the private key holder can sign while any party with the public key can verify, which allows wide distribution of the verification key without compromising signing capability.

---

## Q6. Compare HS256, RS256, and ES256 signing algorithms. When would you choose each?

**Concepts**
- Symmetric HS256 — one key for sign and verify
- Asymmetric RS256/ES256 — private key signs, public key verifies
- RS256 JWKS endpoint for key distribution and rotation
- ES256 smaller key and signature size over RS256

**Answer**

The three algorithms differ in their cryptographic family, key model, and operational fit. HS256 uses a single shared secret, while RS256 and ES256 use asymmetric key pairs, which changes both the security model and deployment complexity.

| Algorithm | Family | Key Type | Signature Size | Best Used When |
|-----------|--------|----------|----------------|----------------|
| HS256 | HMAC-SHA256 | Shared secret | 32 bytes | Single-service or trusted internal system |
| RS256 | RSA-SHA256 | Public/Private key pair | ~256 bytes | Multiple services needing to verify; key rotation via JWKS |
| ES256 | ECDSA-SHA256 | Elliptic Curve key pair | 64 bytes | High-performance systems needing asymmetric security with smaller keys |

I would choose HS256 only in a single-service system where the secret never leaves the service boundary, since any verifier can also forge tokens. RS256 is the most common choice in enterprise and OIDC scenarios because public keys can be published at a JWKS endpoint and rotated without coordination. ES256 provides the same asymmetric security as RS256 but with significantly smaller keys and faster operations, making it the better default for new high-throughput APIs.

---

## Q7. What are the advantages of RS256 over HS256 in a microservices architecture?

**Concepts**
- Private key isolation at the identity server
- Public key distribution via JWKS without secret sprawl
- HS256 rotation requiring simultaneous updates across all services
- RS256 old-key retention during rotation transition window

**Answer**

In a microservices architecture, RS256 allows the identity server to sign tokens with its private key while each downstream service holds only the public key for verification — the private signing key never leaves the identity server. With HS256, every service that validates tokens must also hold the shared secret, which means a breach of any single service compromises the entire system's trust model. Adding a new downstream service with RS256 requires no secret re-sharing; the service fetches the public key from the JWKS endpoint, reducing operational overhead. Key rotation is also seamless: the identity server publishes new public keys at the JWKS endpoint, old keys stay in the set until all tokens signed under them expire, and new tokens are signed with the new private key — each token's `kid` header directs the verifier to the correct key automatically. HS256 rotation is disruptive by contrast, since all services must update simultaneously or verification breaks during the transition window.

---

## Q8. How do you add JWT bearer authentication to an ASP.NET Core application?

**Concepts**
- AddAuthentication + AddJwtBearer registration pattern
- TokenValidationParameters as core validation configuration
- UseAuthentication before UseAuthorization middleware order
- Authority for OpenID Connect automatic JWKS key discovery

**Answer**

JWT bearer authentication is added in `Program.cs` by calling `AddAuthentication` with `JwtBearerDefaults.AuthenticationScheme` as the default scheme, then chaining `AddJwtBearer` to configure validation parameters. `UseAuthentication()` and `UseAuthorization()` must then appear in the middleware pipeline in that order, before any endpoint mapping.

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

`UseAuthentication()` must come before `UseAuthorization()` because the pipeline is ordered — reversing them means the identity is never populated from the JWT before authorization decisions are made. The `Authorization: Bearer <token>` header is automatically extracted and validated by the middleware, so individual endpoints do not need to parse it manually. For RS256, replace `IssuerSigningKey` with an `RsaSecurityKey` constructed from the public key, or configure `options.Authority` to point to the OpenID Connect discovery document so keys are fetched and cached automatically.

---

## Q9. What is `JwtBearerOptions`, and which properties do you configure most often?

**Concepts**
- TokenValidationParameters as the core validation config
- SaveToken for raw JWT retrieval within the request
- Events hooks — OnTokenValidated, OnAuthenticationFailed, OnChallenge
- Authority triggering OpenID Connect automatic key discovery

**Answer**

`JwtBearerOptions` is the configuration object passed to `AddJwtBearer` that controls how the JWT middleware behaves — how it finds the token in the request, validates it, and what events it fires during the process. The most critical inner property is `TokenValidationParameters`, which carries all validation flags and determines what constitutes a valid token. Setting `SaveToken = true` stores the raw JWT string in `HttpContext.Authentication` after successful validation, allowing retrieval of the raw token string later in the request without re-parsing. The `Events` property exposes hooks like `OnTokenValidated`, `OnAuthenticationFailed`, and `OnChallenge` where custom logic can be added — for example, logging failures, reading tokens from cookies, or customizing the 401 response body. Setting `Authority` to an OpenID Connect issuer URL causes the middleware to automatically download and cache signing keys from the discovery document at `/.well-known/openid-configuration`, which simplifies key rotation management significantly.

---

## Q10. What is `TokenValidationParameters`, and which validation flags are critical for production?

**Concepts**
- ValidateIssuerSigningKey defaults to false — signature unchecked without it
- ClockSkew tolerance for server clock drift
- Four critical validation flags and their security purpose
- Default-false trap allowing any structurally valid JWT

**Answer**

`TokenValidationParameters` is a class from `Microsoft.IdentityModel.Tokens` that instructs the JWT middleware on which aspects of a token to validate before accepting it. Disabling any critical flag weakens the security contract and can allow replayed, expired, or misdirected tokens to be accepted.

| Property | Default | Production Value | Why It Matters |
|----------|---------|-----------------|----------------|
| `ValidateIssuer` | `true` | `true` | Prevents tokens from foreign issuers being accepted |
| `ValidateAudience` | `true` | `true` | Prevents tokens intended for another service from being accepted |
| `ValidateLifetime` | `true` | `true` | Rejects expired tokens |
| `ValidateIssuerSigningKey` | `false` | `true` | Verifies the signature with the expected key |
| `ClockSkew` | 5 min | `TimeSpan.Zero` or small value | Tightens expiry enforcement |

`ValidateIssuerSigningKey` being `false` by default is a common trap — without it, the signature is not verified against the configured key, so any structurally valid JWT — even one signed with a completely different key — is accepted. `ClockSkew` defaults to five minutes to tolerate clock drift; in a well-synchronized environment, reducing it to zero or thirty seconds tightens expiry enforcement.

---

## Q11. Why is `ValidateLifetime` important, and what happens if you disable it?

**Concepts**
- exp and nbf claim checking against current time
- Disabled lifetime validation making stolen tokens permanently valid
- Development vs production carryover risk
- ClockSkew as companion tolerance setting

**Answer**

`ValidateLifetime` instructs the token handler to check the `exp` (expiration) and `nbf` (not before) claims against the current time. A short token lifetime is one of the primary mitigations against token theft — even if an attacker obtains a valid token, it stops working after the expiry window closes. With `ValidateLifetime` disabled, a stolen token becomes permanently valid as long as the signing key is unchanged, transforming a temporary compromise into a long-term one. Disabling it is sometimes done in development to avoid token expiry interrupting debugging sessions, but this setting must never reach production. The `ClockSkew` property works alongside `ValidateLifetime` by adding tolerance for clock drift between the issuing and validating servers — the token is accepted for up to `ClockSkew` seconds after its `exp` timestamp.

---

## Q12. Why must both `ValidateIssuer` and `ValidateAudience` be set to `true` in production?

**Concepts**
- Issuer validation preventing foreign-authority token acceptance
- Audience confusion attack — replaying a token across services
- `aud` as the primary inter-service isolation boundary
- Both validations as complementary, not redundant, controls

**Answer**

Validating the issuer ensures only tokens created by the trusted identity server are accepted, preventing tokens from an unrelated or malicious authority from being used. Validating the audience ensures a token intended for a specific service cannot be replayed against a different service that shares the same signing key. Both validations are necessary because a token that passes issuer validation alone could still be misused across services. The audience confusion attack works when Service A and Service B both trust the same identity server but do not validate audience — a token issued for Service A can be presented to Service B and accepted, granting unintended access. The `iss` claim is equally important in federated scenarios with multiple identity providers, since without issuer validation a token from a less-secure issuer could be accepted by a service intended to trust only a high-security authority.

---

## Q13. How do you generate a JWT in .NET using `JwtSecurityTokenHandler`?

**Concepts**
- SecurityTokenDescriptor builder pattern
- CreateEncodedJwt vs CreateToken + WriteToken
- DateTime.UtcNow for expiry correctness
- SigningCredentials pairing key material with algorithm

**Answer**

Token generation uses `JwtSecurityTokenHandler` from `System.IdentityModel.Tokens.Jwt`. I construct a `SecurityTokenDescriptor` that carries the claims, lifetime, signing credentials, and issuer/audience, then call `CreateEncodedJwt` to get the serialized string.

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

Always use `DateTime.UtcNow` for `Expires` — using local time causes expiry miscalculations when the server's timezone differs from UTC. `CreateEncodedJwt` directly returns the Base64URL-encoded string; `CreateToken` returns a `SecurityToken` object that you then pass to `WriteToken` if you need the string form separately. The `SigningCredentials` object pairs the key material with the algorithm identifier, which ensures the header's `alg` field is set correctly.

---

## Q14. What claims should you include in a JWT for an ASP.NET Core Identity-backed API?

**Concepts**
- `sub` as stable user identifier rather than mutable username
- Role claims from GetRolesAsync avoiding per-request database queries
- `jti` for per-token revocation tracking
- Excluding sensitive or volatile data from the unencrypted payload

**Answer**

For an API backed by ASP.NET Core Identity, the JWT should carry claims that identify the user uniquely and convey enough authorization context so downstream services do not need to call the identity store on every request. The standard registered claims (`sub`, `jti`, `iat`, `exp`) should always be present. `sub` should be set to the user's stable unique identifier — typically `user.Id` from `IdentityUser` — not the username, because usernames can change. Role claims should be added from `UserManager.GetRolesAsync(user)` so that `[Authorize(Roles = "Admin")]` works without a database query per request. A `jti` claim generated with `Guid.NewGuid()` enables per-token revocation tracking if you implement a token denylist. Sensitive or volatile data such as email or phone number should be kept out of the payload, since the token is signed but not encrypted and claims become stale as soon as the user updates their profile without re-issuing a token.

---

## Q15. How does ASP.NET Core Identity integrate with JWT generation — what role does `UserManager` play?

**Concepts**
- UserManager as data access layer, not token generator
- GetRolesAsync and GetClaimsAsync for embedding Identity data in the token
- CheckPasswordAsync as the authentication entry point
- Cookie auth and JWT bearer as independent, coexisting mechanisms

**Answer**

ASP.NET Core Identity's `UserManager<TUser>` provides the data access layer for user information and does not generate JWTs itself — token generation is implemented in application code, typically in a service or controller that combines Identity data retrieval with `JwtSecurityTokenHandler`. The typical flow is: authenticate the user's credentials via `UserManager.CheckPasswordAsync`, retrieve roles and claims from the Identity store, then build and sign the token. `GetRolesAsync(user)` returns the user's roles from the database, which are translated into role claims added to the token payload. `GetClaimsAsync(user)` retrieves any custom claims stored in the `AspNetUserClaims` table, allowing fine-grained permissions to be embedded without an additional database call per request. Identity's built-in cookie authentication and JWT bearer authentication are entirely separate mechanisms — adding JWT bearer auth does not replace cookie auth, and both can coexist in the same application serving different endpoint groups.

---

## Q16. What is the refresh token pattern, and why are access tokens intentionally short-lived?

**Concepts**
- Short-lived access tokens vs long-lived refresh tokens
- Stateless JWT revocation impossibility — short expiry as primary mitigation
- Refresh token rotation for stolen-token detection
- Server-side refresh token storage as the only stateful part

**Answer**

The refresh token pattern separates token concerns into two pieces: a short-lived access token (typically 5–15 minutes) that gates API requests, and a long-lived refresh token (hours to days) that can be exchanged for a new access token without requiring the user to re-enter credentials. Access tokens are short-lived because JWTs are stateless and cannot be individually revoked once issued — a short lifetime limits the damage window if a token is stolen. When the access token expires, the client sends the refresh token to a dedicated `/refresh` endpoint; the server validates it against a database record, issues a new access token, and optionally rotates the refresh token to invalidate the previous one. Refresh token rotation means each token is used only once — after exchange it is marked consumed and a new one is issued, so a stolen refresh token is detected the next time the legitimate client tries to use it, since the server sees both a used and an active token, indicating theft. Unlike access tokens, refresh tokens must be stored server-side so they can be revoked — this is the one part of JWT-based auth that requires server state.

---

## Q17. How should refresh tokens be stored and validated on the server side?

**Concepts**
- Hashed refresh token storage — analogous to password hashing
- Family ID for reuse attack detection and whole-family revocation
- Device fingerprint as a soft anomaly signal
- Atomic record deletion on use to enforce single-use semantics

**Answer**

Refresh tokens should be stored in the database as hashed values (not plaintext), associated with the user ID, the device or session that issued them, an expiry timestamp, and a revocation flag. When a client presents a refresh token, the server hashes the incoming value, queries for a matching record, checks that it has not expired or been revoked, then issues a new access token. Storing the raw token in the database is analogous to storing plaintext passwords — if the table is leaked, all refresh tokens are immediately usable; hashing with SHA-256 limits the exposure significantly. Each refresh token record should include a family ID to detect refresh token reuse attacks: if a consumed token from a family is presented, the entire family should be revoked as a breach response, since the server sees evidence that both the attacker and the legitimate user hold tokens from that family. The database record can also track the client IP or device fingerprint as a soft signal — a refresh from a completely different geography can trigger step-up authentication even if the token itself is structurally valid.

---

## Q18. What is the token revocation problem with JWTs, and what strategies exist to mitigate it?

**Concepts**
- Stateless design — no built-in per-token invalidation
- Short expiry as the simplest mitigation
- Denylist with `jti` — reintroducing server state for revocability
- Token version on user record for immediate global revocation

**Answer**

JWTs are stateless by design — once issued, the server has no built-in mechanism to invalidate a specific token before it expires naturally, so if a user logs out, changes their password, or has their account suspended, existing access tokens remain valid until expiry. Short expiry is the simplest mitigation: if access tokens expire in five minutes, the window of unauthorized access after a credential change is at most five minutes without any additional infrastructure. A denylist approach stores revoked token identifiers (`jti` claims) in a fast store like Redis; on every request, the middleware checks the `jti` against the denylist before accepting the token, which reintroduces server state but allows immediate revocation. A token version on the user record offers another approach: a `tokenVersion` integer is embedded in the JWT, and on every request the API checks that it matches the current value in the database — incrementing the version on logout immediately invalidates all outstanding tokens for that user without a growing denylist. Each strategy trades some of the stateless scalability of JWTs for revocability; the right choice depends on how critical immediate revocation is for the application's threat model.

---

## Q19. Should JWTs be stored in `localStorage` or an `HttpOnly` cookie? What are the security trade-offs?

**Concepts**
- localStorage XSS risk — token readable by any script
- HttpOnly cookie CSRF risk — mitigated with SameSite and anti-forgery tokens
- SameSite=Strict/Lax as first-line CSRF defence
- Platform secure keychain for native mobile clients

**Answer**

Storing JWTs in `localStorage` makes them accessible to JavaScript running on the page, which means a Cross-Site Scripting attack can exfiltrate the token directly. Storing JWTs in `HttpOnly` cookies makes them inaccessible to JavaScript, mitigating XSS-based token theft, but introduces Cross-Site Request Forgery risk because browsers automatically send cookies with cross-origin requests.

| Storage | XSS Risk | CSRF Risk | Access from JS | Notes |
|---------|----------|-----------|----------------|-------|
| `localStorage` | High — token readable by any script | Low | Yes | Simple for SPAs, dangerous if any XSS exists |
| `sessionStorage` | High — same as localStorage | Low | Yes | Cleared on tab close, still JS-accessible |
| `HttpOnly` Cookie | Low — JS cannot read it | High — mitigated with SameSite/CSRF tokens | No | Recommended for web apps |
| Memory (JS variable) | Medium — lost on refresh | Low | Yes | Best XSS profile but poor UX |

An `HttpOnly` cookie combined with `SameSite=Strict` or `SameSite=Lax` and a CSRF token provides the strongest protection for browser-based clients. For native mobile and desktop clients, storage in the platform's secure keychain (iOS Keychain, Android Keystore) is the equivalent of `HttpOnly` cookies — isolated from other apps. Content Security Policy headers significantly reduce XSS risk even when `localStorage` is used, but they do not eliminate it — defence in depth requires both.

---

## Q20. How does the `[Authorize]` attribute work with bearer tokens in ASP.NET Core?

**Concepts**
- HttpContext.User populated by JWT middleware before authorization runs
- 401 Unauthenticated vs 403 Unauthorized distinction
- [AllowAnonymous] overriding controller-level [Authorize]
- Authentication before authorization as a required pipeline order

**Answer**

When a request reaches a controller or action marked with `[Authorize]`, the ASP.NET Core authorization middleware checks whether the current `HttpContext.User` principal is authenticated. The JWT bearer middleware, which runs earlier in the pipeline, has already extracted the `Authorization: Bearer <token>` header, validated the token, and populated `HttpContext.User` with a `ClaimsPrincipal` from the token's claims. If the principal is not authenticated, the middleware returns a 401 Unauthorized response; if authenticated but not authorized — for example, missing a required role — it returns 403 Forbidden. The `[Authorize]` attribute with no parameters requires only that the user is authenticated, so any valid JWT passes this check. `[Authorize(Roles = "Admin")]` additionally checks that the principal has a claim of type `ClaimTypes.Role` with the value `"Admin"`, which the JWT middleware populates from the role claims in the token payload. `[AllowAnonymous]` on an action overrides `[Authorize]` at the controller level, allowing unauthenticated access to that specific action regardless of the controller-level policy.

---

## Q21. How do you define and enforce policy-based authorization alongside JWT bearer authentication?

**Concepts**
- Named authorization policies in AddAuthorization
- IAuthorizationRequirement and AuthorizationHandler pair
- JWT claims as policy data source — no database query per request
- FallbackPolicy for deny-by-default posture

**Answer**

Policy-based authorization allows complex access rules beyond simple role checks by defining named policies in `AddAuthorization` and referencing them with `[Authorize(Policy = "PolicyName")]`. Policies are composed of one or more requirements, each fulfilled by a corresponding `AuthorizationHandler`, and JWT claims serve as the data source for requirement evaluation.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SeniorEmployee", policy =>
        policy.RequireClaim("department", "Engineering")
              .RequireClaim("yearsOfService", "5", "6", "7", "8", "9", "10"));
});
```

Policy requirements can access any claim in `HttpContext.User`, meaning any claim embedded in the JWT — such as `tenant_id`, `plan`, or `clearance_level` — can drive authorization decisions without a database query. Custom `IAuthorizationRequirement` and `AuthorizationHandler<T>` pairs allow arbitrary logic, including calling a database or external service, when claim-based rules alone are insufficient. Policies declared with `FallbackPolicy` apply to all endpoints that have no explicit authorization attribute, providing a deny-by-default posture without decorating every action individually.

---

## Q22. What role does CORS (Cross-Origin Resource Sharing) play in API security when using JWT?

**Concepts**
- CORS as browser-only enforcement — irrelevant to non-browser clients
- Authorization header requiring explicit CORS allowlist entry
- AllowAnyOrigin + AllowCredentials prohibition
- CORS misconfiguration as authentication bypass for browser clients

**Answer**

CORS is a browser security feature that restricts which origins can make cross-origin HTTP requests to your API. When a SPA on `https://app.example.com` calls an API at `https://api.example.com`, the browser sends a preflight `OPTIONS` request to check whether the API allows cross-origin requests from that origin; without proper CORS configuration the browser blocks the actual request. CORS does not protect against server-to-server requests or non-browser clients — it is a browser enforcement mechanism only. The CORS policy must explicitly allow the `Authorization` header; if it is not listed in `AllowedHeaders`, the browser strips it from the cross-origin request and the JWT never reaches the API. Using `AllowAnyOrigin()` combined with `AllowCredentials()` is explicitly forbidden by the CORS specification — you must name specific origins when credentials or authorization headers are included. CORS misconfiguration that reflects the request's `Origin` header without restriction effectively disables same-origin protection, allowing malicious sites to make authenticated API calls on behalf of logged-in users.

---

## Q23. How do you configure CORS in ASP.NET Core so that the `Authorization` header is allowed?

**Concepts**
- Named CORS policy with WithHeaders for Authorization
- UseCors placement before UseAuthentication
- WithExposedHeaders for client to read Authorization response header
- AllowAnyOrigin + AllowCredentials InvalidOperationException at startup

**Answer**

CORS is configured by registering a named policy in `AddCors` and applying it either globally with `UseCors` or per-endpoint with `[EnableCors]`. To allow the `Authorization` header, call `WithHeaders` with `HeaderNames.Authorization` or `AllowAnyHeader` within the policy definition.

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

`UseCors` must be placed before `UseAuthentication` and `UseAuthorization` so that CORS headers are added to the response before the authentication middleware produces a 401 — otherwise preflight requests fail with no CORS headers on the error response. For browsers to read the `Authorization` response header (e.g., a token returned in a response header), you must also call `WithExposedHeaders(HeaderNames.Authorization)`.

---

## Q24. How do you pass custom claims (e.g., roles, tenant ID) in a JWT and read them inside a controller?

**Concepts**
- Custom Claim objects in SecurityTokenDescriptor Subject
- User.FindFirstValue and User.HasClaim for claim retrieval
- Short claim type strings for compact, interoperable tokens
- HTTP header size limits constraining large claim sets

**Answer**

Custom claims are added to the token payload at generation time by including additional `Claim` objects in the `Subject` of the `SecurityTokenDescriptor`. Once the JWT middleware validates the token, all claims are available on `HttpContext.User` and can be accessed in a controller via `User.FindFirstValue(claimType)` or `User.Claims`.

```csharp
// At token generation
new Claim("tenant_id", tenantId),
new Claim("plan", "Enterprise")

// In a controller action
string tenantId = User.FindFirstValue("tenant_id");
bool isEnterprise = User.HasClaim("plan", "Enterprise");
```

Claim type strings are arbitrary, but using registered JWT claim names — lowercase, no namespace — keeps the token compact and interoperable with other tooling. Role claims added with `ClaimTypes.Role` (the long XML-namespaced name) are mapped to the short `role` key in the JWT payload by the default handler; the `RoleClaimType` property on `TokenValidationParameters` can override this mapping. Large numbers of claims increase token size, which matters because browsers typically cap total header size around 8 KB — avoid embedding large lists in the JWT and instead prefer referencing them by ID and fetching from cache.

---

## Q25. What happens when a JWT expires mid-request? How can clients detect and handle this transparently?

**Concepts**
- 401 with WWW-Authenticate Bearer error="invalid_token" as the detection signal
- ClockSkew grace period after exp timestamp
- Client-side refresh interceptor pattern
- Concurrent request queuing to avoid multiple simultaneous refreshes

**Answer**

If a JWT expires before the server processes a request, the JWT bearer middleware rejects the token and returns a 401 Unauthorized response with a `WWW-Authenticate: Bearer error="invalid_token"` header. Clients that implement transparent token refresh intercept 401 responses, use the refresh token to obtain a new access token, and replay the original request with the new token — all without the user noticing. The 401 status combined with the `invalid_token` error distinguishes an expired token from a missing-credentials 401, allowing the client to attempt refresh rather than redirect to login. `ClockSkew` on `TokenValidationParameters` adds a grace period after the `exp` timestamp during which the server still accepts the token, preventing failures from minor clock drift between client and server. Libraries like `axios` support request interceptors that automatically queue failed requests, refresh the token once, and retry all queued requests — avoiding the race condition where multiple concurrent requests all attempt to refresh simultaneously.

---

## Q26. How do you validate a JWT manually without the ASP.NET Core middleware pipeline?

**Concepts**
- JwtSecurityTokenHandler.ValidateToken outside middleware
- TokenValidationParameters reuse
- `out SecurityToken` for post-validation claim inspection
- SecurityTokenException subclass hierarchy for distinct failure modes

**Answer**

You can validate a JWT outside the middleware pipeline using `JwtSecurityTokenHandler.ValidateToken`, passing the raw token string and a `TokenValidationParameters` instance. The method returns a `ClaimsPrincipal` if validation succeeds or throws a `SecurityTokenException` subclass describing the failure.

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

This pattern is useful in background services, message consumers, or inter-service calls where an HTTP pipeline is not present and you need to validate a token carried in a message header or queue payload. The `out SecurityToken validated` parameter gives access to the parsed token object (cast to `JwtSecurityToken`) so you can inspect individual claims or the token's `ValidTo` property without a second parse. Wrap the call in a try/catch for `SecurityTokenExpiredException`, `SecurityTokenInvalidSignatureException`, and the base `SecurityTokenException` to handle different failure modes distinctly.

---

## Q27. What is a JWK Set (JWKS) endpoint, and how does it enable key rotation without redeploying services?

**Concepts**
- JWKS document structure and `kid` claim matching
- Old key retention during rotation transition window
- Authority-based automatic JWKS discovery and caching in ASP.NET Core
- Key rotation without configuration change or redeployment

**Answer**

A JSON Web Key Set (JWKS) is a JSON document published at a well-known URL — typically `/.well-known/jwks.json` — that contains the public keys the identity server uses to sign tokens, expressed in JSON Web Key format. Verifying services periodically fetch this document to obtain the current public keys, so when the identity server rotates its signing key pair, services automatically pick up the new public key without any configuration change or redeployment. Each key in the JWKS document carries a `kid` (key ID) field; the JWT header also includes a `kid` that tells the verifier which key in the set to use, allowing multiple keys to coexist during a rotation window. Key rotation works gracefully because the old public key is kept in the JWKS document until all tokens signed under it have expired, and new tokens are signed with the new private key — each token's `kid` header directs the verifier to the correct key automatically. ASP.NET Core's JWT bearer middleware discovers and caches keys from the JWKS endpoint when you configure `options.Authority` to point to an OpenID Connect-compliant issuer, and it periodically refreshes the cache to handle key rotation.

---

## Q28. What are the most common security mistakes developers make when implementing JWT authentication?

**Concepts**
- Disabled validation flags carried from development to production
- Payload confidentiality misconception — Base64URL is not encryption
- Weak or hardcoded HS256 signing secrets
- Long access token expiry negating the short-lifetime security model
- Missing revocation for high-risk operations

**Answer**

JWT authentication has several well-known pitfalls that regularly appear in security audits. Disabling `ValidateIssuer`, `ValidateAudience`, or `ValidateLifetime` for development convenience and deploying with those settings disabled is one of the most frequent and dangerous mistakes — it turns JWTs from a secure mechanism into a trivially forgeable one. Assuming the JWT payload is secret because it is "encoded" and storing sensitive data such as passwords, PII, or internal system identifiers in the payload is another common error; Base64URL decoding requires no key and is reversible by anyone. Using a weak or short signing secret for HS256, or hard-coding the secret in source code where it can be leaked via version control, undermines the cryptographic guarantee — secrets should be stored in environment variables or a secrets manager and rotated regularly. Setting very long access token expiry times (days or weeks) to avoid implementing refresh tokens negates the primary security benefit of short-lived tokens and extends the breach window indefinitely. Not implementing token revocation for high-risk operations such as password changes, role changes, or account suspension means a compromised token remains valid long after the conditions that justified it have changed.

---

## Gotchas — JWT & API Security (Interview Traps)

---

## Gotcha 1. JWT payload is Base64URL-encoded, not encrypted

**Concepts**
- Base64URL as reversible encoding with no security properties
- JWS (signed) vs JWE (encrypted) distinction
- Sensitive data exposure in standard JWT payloads

**Answer**

A common misconception is that because a JWT looks like random characters, its contents are secret. The payload is Base64URL-encoded — a reversible text encoding requiring no key — so any party who holds the token can decode and read every claim instantly. Treating JWT as confidential and storing sensitive data such as passwords, SSNs, or internal system paths in the payload exposes that data to anyone who intercepts or is handed the token. If payload confidentiality is required, use JWE (JSON Web Encryption) rather than JWS (JSON Web Signature), or move sensitive data server-side and reference it by a non-guessable identifier.

---

## Gotcha 2. `ValidateIssuerSigningKey` defaults to `false`

**Concepts**
- ValidateIssuerSigningKey default-false trap
- Configuring a key is not the same as activating key verification
- Any structurally valid JWT accepted when key verification is off

**Answer**

`TokenValidationParameters.ValidateIssuerSigningKey` defaults to `false`, meaning that without explicitly setting it to `true`, the middleware verifies the token's structure but does not confirm the signature against the configured key — any structurally valid JWT, even one signed with a completely different key, would be accepted. Setting `IssuerSigningKey` alone is not sufficient; `ValidateIssuerSigningKey = true` must also be set to activate key verification. This default was a source of multiple security vulnerabilities in production systems where developers configured the key but did not realize signature verification was still disabled.

---

## Gotcha 3. `UseAuthentication` must come before `UseAuthorization` in the middleware pipeline

**Concepts**
- Ordered sequential middleware pipeline
- HttpContext.User unpopulated when authorization runs before authentication
- Silent runtime failure — no compile-time error or startup exception
- Correct order: UseRouting → UseCors → UseAuthentication → UseAuthorization

**Answer**

ASP.NET Core's middleware pipeline is ordered and sequential — each middleware runs in the order it is registered. If `UseAuthorization` runs before `UseAuthentication`, the `HttpContext.User` principal has not yet been populated from the JWT, so authorization decisions are always made against an anonymous principal and `[Authorize]` always returns 401. The correct order is `UseRouting` → `UseCors` → `UseAuthentication` → `UseAuthorization` → endpoint mapping. This is a runtime behaviour bug that does not produce a compile-time error or startup exception, making it easy to miss until integration testing reveals that all protected endpoints return 401 even with a valid token.

---

## Gotcha 4. JWTs cannot be revoked without additional infrastructure

**Concepts**
- Stateless validation — no built-in per-token invalidation
- Client-side deletion vs server-side session invalidation
- Denylist, token version, or short lifetime as revocation strategies

**Answer**

Because JWT validation is stateless — the server checks only the signature, claims, and lifetime without querying a database — there is no built-in mechanism to invalidate a specific token before it expires. Developers sometimes assume that "logging out" by deleting the token from the client is equivalent to server-side session invalidation, but any copy of the token that still exists remains fully valid. True revocation requires server state: either a denylist of revoked `jti` values, a per-user token version number, or enforcing very short token lifetimes so the effective revocation window is acceptably small. This fundamental limitation is the most important trade-off to understand when choosing between JWT-based stateless authentication and server-side sessions.

---

## Gotcha 5. CORS `AllowAnyOrigin` and `AllowCredentials` cannot be combined

**Concepts**
- CORS specification prohibition on wildcard origin with credentials
- ASP.NET Core InvalidOperationException at startup
- Manual header override as an insecure workaround pattern

**Answer**

The CORS specification explicitly prohibits responding with `Access-Control-Allow-Origin: *` when the request includes credentials. ASP.NET Core enforces this at runtime — calling `AllowAnyOrigin().AllowCredentials()` together throws an `InvalidOperationException` at startup. The correct approach when credentials are required is to use `WithOrigins("https://app.example.com")` to name specific allowed origins rather than using the wildcard. Developers sometimes work around the startup exception by setting the `Access-Control-Allow-Origin` header manually in middleware, bypassing the policy system entirely — this is an insecure pattern that can inadvertently reflect arbitrary origins.
