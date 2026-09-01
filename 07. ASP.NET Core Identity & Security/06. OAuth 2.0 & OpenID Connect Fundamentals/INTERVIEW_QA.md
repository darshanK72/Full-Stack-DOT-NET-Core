# OAuth 2.0 & OpenID Connect Fundamentals — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What problem does OAuth 2.0 solve, and why is it described as a delegated authorization framework rather than an authentication framework?](#q1-what-problem-does-oauth-20-solve-and-why-is-it-described-as-a-delegated-authorization-framework-rather-than-an-authentication-framework)
2. [Q2. What are the four roles defined in OAuth 2.0, and what is each role responsible for?](#q2-what-are-the-four-roles-defined-in-oauth-20-and-what-is-each-role-responsible-for)
3. [Q3. What is the difference between the Authorization Server and the Resource Server in OAuth 2.0?](#q3-what-is-the-difference-between-the-authorization-server-and-the-resource-server-in-oauth-20)
4. [Q4. What is the Authorization Code grant type, and when should it be used?](#q4-what-is-the-authorization-code-grant-type-and-when-should-it-be-used)
5. [Q5. Walk through the Authorization Code flow step by step, including every redirect and request.](#q5-walk-through-the-authorization-code-flow-step-by-step-including-every-redirect-and-request)
6. [Q6. What is PKCE (Proof Key for Code Exchange), and what attack does it prevent?](#q6-what-is-pkce-proof-key-for-code-exchange-and-what-attack-does-it-prevent)
7. [Q7. How does PKCE work mechanically — what is the code verifier, the code challenge, and how are they used?](#q7-how-does-pkce-work-mechanically-what-is-the-code-verifier-the-code-challenge-and-how-are-they-used)
8. [Q8. What is the Client Credentials grant type, and when is it the appropriate choice?](#q8-what-is-the-client-credentials-grant-type-and-when-is-it-the-appropriate-choice)
9. [Q9. What is the Device Code grant type, and what problem does it address?](#q9-what-is-the-device-code-grant-type-and-what-problem-does-it-address)
10. [Q10. Why are the Implicit grant and Resource Owner Password Credentials (ROPC) grant considered legacy or deprecated?](#q10-why-are-the-implicit-grant-and-resource-owner-password-credentials-ropc-grant-considered-legacy-or-deprecated)
11. [Q11. What is an Access Token, and what information does it convey to the Resource Server?](#q11-what-is-an-access-token-and-what-information-does-it-convey-to-the-resource-server)
12. [Q12. What is a Refresh Token, and how does it differ from an Access Token in lifetime and purpose?](#q12-what-is-a-refresh-token-and-how-does-it-differ-from-an-access-token-in-lifetime-and-purpose)
13. [Q13. What is an ID Token, and why does OpenID Connect introduce it when OAuth 2.0 already has access tokens?](#q13-what-is-an-id-token-and-why-does-openid-connect-introduce-it-when-oauth-20-already-has-access-tokens)
14. [Q14. What is the structure of a JSON Web Token (JWT), and what does each part contain?](#q14-what-is-the-structure-of-a-json-web-token-jwt-and-what-does-each-part-contain)
15. [Q15. How is a JWT signed, and how does a Resource Server or client verify that signature?](#q15-how-is-a-jwt-signed-and-how-does-a-resource-server-or-client-verify-that-signature)
16. [Q16. What is OpenID Connect (OIDC), and how does it relate to OAuth 2.0?](#q16-what-is-openid-connect-oidc-and-how-does-it-relate-to-oauth-20)
17. [Q17. What is the fundamental difference between OAuth 2.0 and OpenID Connect in terms of what each one provides?](#q17-what-is-the-fundamental-difference-between-oauth-20-and-openid-connect-in-terms-of-what-each-one-provides)
18. [Q18. What OIDC scopes are defined by the specification, and what claims does each scope expose?](#q18-what-oidc-scopes-are-defined-by-the-specification-and-what-claims-does-each-scope-expose)
19. [Q19. What is the UserInfo endpoint in OIDC, and when would a client call it instead of reading the ID Token?](#q19-what-is-the-userinfo-endpoint-in-oidc-and-when-would-a-client-call-it-instead-of-reading-the-id-token)
20. [Q20. What is the OpenID Connect discovery document, where is it found, and what information does it contain?](#q20-what-is-the-openid-connect-discovery-document-where-is-it-found-and-what-information-does-it-contain)
21. [Q21. What validations must a client perform on a received ID Token before trusting it?](#q21-what-validations-must-a-client-perform-on-a-received-id-token-before-trusting-it)
22. [Q22. What validations should a Resource Server perform on an Access Token (JWT format) on every request?](#q22-what-validations-should-a-resource-server-perform-on-an-access-token-jwt-format-on-every-request)
23. [Q23. What is the nonce parameter in OIDC, and what attack does it prevent?](#q23-what-is-the-nonce-parameter-in-oidc-and-what-attack-does-it-prevent)
24. [Q24. What is token introspection, and when is it used instead of local JWT validation?](#q24-what-is-token-introspection-and-when-is-it-used-instead-of-local-jwt-validation)
25. [Q25. What is token revocation, and why does it matter for short-lived versus long-lived tokens?](#q25-what-is-token-revocation-and-why-does-it-matter-for-short-lived-versus-long-lived-tokens)
26. [Q26. What is the difference between the front channel and the back channel in an OAuth/OIDC flow, and why does the distinction matter for security?](#q26-what-is-the-difference-between-the-front-channel-and-the-back-channel-in-an-oauthoidc-flow-and-why-does-the-distinction-matter-for-security)
27. [Q27. How does an Authorization Code flow with PKCE differ from the original Authorization Code flow with a client secret, and which is preferred for public clients?](#q27-how-does-an-authorization-code-flow-with-pkce-differ-from-the-original-authorization-code-flow-with-a-client-secret-and-which-is-preferred-for-public-clients)
28. [Q28. Can an OAuth 2.0 Access Token alone be used to authenticate a user — that is, to prove who the user is? Why or why not?](#q28-can-an-oauth-20-access-token-alone-be-used-to-authenticate-a-user-that-is-to-prove-who-the-user-is-why-or-why-not)

---

## Q1. What problem does OAuth 2.0 solve, and why is it described as a delegated authorization framework rather than an authentication framework?

**Concepts**
- Delegated authorization — scoped, revocable permission without credential sharing
- Access token as capability, not identity assertion
- Pre-OAuth credential-sharing anti-pattern
- Scope — limits the delegated permission to a named set of operations

**Answer**

OAuth 2.0 solves the problem of allowing a third-party application to access resources on behalf of a user without the user handing their credentials to that third party. Before OAuth 2.0, the typical pattern required third-party apps to ask for a user's username and password directly, which gave full access, could not be scoped, and could not be revoked without changing the password. OAuth 2.0 introduces scopes so a user can grant read-only access to photos without also granting access to contacts, and the authorization can be revoked at any time without changing the user's password. It is called a delegated authorization framework because the user delegates a scoped, revocable permission to the client application, and the authorization server enforces those limits. It is not an authentication framework because the access token it produces tells the Resource Server what the client is allowed to do, not who the user actually is. The access token is a bearer credential — any party possessing it can use it — so OAuth 2.0 says nothing about how the token was obtained or which user consented. Confusing OAuth 2.0 with authentication is a common mistake; an application that uses an access token to look up a profile is performing authentication through a side effect, not through a protocol guarantee.

---

## Q2. What are the four roles defined in OAuth 2.0, and what is each role responsible for?

**Concepts**
- Resource Owner — the entity that grants access, typically the end user
- Client — the application requesting delegated access
- Authorization Server — issues tokens and is the trust anchor
- Resource Server — hosts protected resources, validates tokens

**Answer**

OAuth 2.0 defines exactly four roles: Resource Owner, Client, Authorization Server, and Resource Server. The Resource Owner is the entity that can grant access to a protected resource; in most cases this is the end user who owns the data. The Client is the application requesting access on behalf of the Resource Owner; it can be a web app, mobile app, single-page app, or a backend service. The Authorization Server (AS) issues tokens after authenticating the Resource Owner and obtaining their consent; it is the trust anchor of the entire system, since all parties must trust what it issues. The Resource Server (RS) hosts the protected resources and accepts access tokens to serve API requests; it trusts the Authorization Server that issued those tokens. In practice, the Authorization Server and Resource Server are often operated by the same organization, but the protocol treats them as separate, which allows them to be deployed independently and allows a single Authorization Server to protect multiple Resource Servers.

---

## Q3. What is the difference between the Authorization Server and the Resource Server in OAuth 2.0?

**Concepts**
- Authorization Server — authenticates users, issues tokens
- Resource Server — validates tokens, serves protected data
- `aud` claim — binds a token to a specific Resource Server
- Token introspection — RS-to-AS real-time validity check

**Answer**

The Authorization Server authenticates the user, obtains consent, and issues tokens. The Resource Server hosts the actual protected data or API and validates those tokens to serve requests. The two servers have entirely different jobs and can be operated by different teams or vendors. A single Authorization Server can protect multiple Resource Servers by issuing tokens with an `aud` (audience) claim that names which RS the token is valid for; the RS checks the `aud` claim on every request and rejects tokens intended for a different audience. The Resource Server never sees the user's credentials; it only ever receives an access token and decides whether to honor the request based on that token's scopes and claims. When the Authorization Server and Resource Server are combined, the separation still exists logically even if not physically. Token introspection is the protocol that lets a Resource Server ask the Authorization Server in real time whether a token is still valid, bridging the gap when the RS cannot validate the token locally.

---

## Q4. What is the Authorization Code grant type, and when should it be used?

**Concepts**
- Authorization Code grant — short-lived code exchanged server-to-server
- Back-channel token delivery — tokens never exposed in browser URL
- Client secret or PKCE verifier — required to complete the exchange
- Single-use, short-lived code — interception alone is insufficient

**Answer**

The Authorization Code grant is the primary OAuth 2.0 flow for applications that can either keep a secret or use PKCE, and that have a back-channel component to exchange the authorization code for tokens. The user is redirected to the Authorization Server, authenticates and consents there, and a short-lived code is returned to the application, which exchanges it for tokens using its client secret or PKCE code verifier in a direct server-to-server request. The code is single-use and short-lived (typically 60 seconds or less), so intercepting it alone does not grant access without the client secret or PKCE verifier. This grant is appropriate for traditional server-rendered web apps, APIs acting on behalf of a user, and mobile or single-page apps when combined with PKCE. The tokens are delivered over the back channel — a direct HTTPS request from the client's server to the AS's token endpoint — so they are never exposed in the browser URL bar or history, which is the key security advantage over the now-deprecated Implicit grant.

---

## Q5. Walk through the Authorization Code flow step by step, including every redirect and request.

**Concepts**
- `state` parameter — CSRF protection, bound to the browser session
- `response_type=code` — signals the Authorization Code grant
- Back-channel token exchange — `grant_type=authorization_code` POST to `/token`
- Bearer token usage — `Authorization: Bearer {token}` header on Resource Server calls

**Answer**

The Authorization Code flow involves six distinct steps: the client builds an authorization URL and redirects the browser, the user authenticates and consents, the Authorization Server redirects back with a code, the client validates the state and exchanges the code for tokens over the back channel, and the client uses the access token to call the Resource Server. Every sensitive token exchange happens server-to-server, not through the browser.

1. The client redirects the browser to the Authorization Server's `/authorize` endpoint with parameters: `response_type=code`, `client_id`, `redirect_uri`, `scope`, and `state` (a random value to prevent CSRF).
2. The Authorization Server presents a login and consent screen; the user authenticates and approves the requested scopes.
3. The Authorization Server redirects the browser back to the client's `redirect_uri` with a short-lived `code` and the original `state` value appended as query parameters.
4. The client validates the `state`, then sends a back-channel POST to the `/token` endpoint with `grant_type=authorization_code`, `code`, `redirect_uri`, `client_id`, and `client_secret` (or PKCE verifier).
5. The Authorization Server validates everything and responds with an access token, a refresh token, and (if OIDC is used) an ID Token.
6. The client uses the access token as a Bearer token in the `Authorization` header of requests to the Resource Server.

---

## Q6. What is PKCE (Proof Key for Code Exchange), and what attack does it prevent?

**Concepts**
- Authorization code interception attack — malicious app intercepts code via redirect URI
- PKCE — cryptographic per-request proof that only the initiator can complete the exchange
- RFC 7636 — original PKCE specification
- RFC 9700 — recommends PKCE for all clients, including confidential

**Answer**

PKCE (pronounced "pixie") is an extension to the Authorization Code grant that prevents authorization code interception attacks. An authorization code interception attack occurs when a malicious app on the same device registers the same redirect URI scheme and intercepts the code before the legitimate app receives it; without a client secret or PKCE, the attacker can take the intercepted code directly to the token endpoint and obtain tokens. PKCE closes this gap by requiring the client to prove, at token exchange time, that it created the original authorization request — something only the legitimate initiating client can do, since the proof is derived from a secret the client generated locally at the start of the flow. PKCE was originally designed for mobile apps that cannot securely store a static client secret, but RFC 9700 (OAuth 2.0 Security Best Current Practice) now recommends PKCE for all Authorization Code flows, including server-side web apps.

---

## Q7. How does PKCE work mechanically — what is the code verifier, the code challenge, and how are they used?

**Concepts**
- Code verifier — cryptographically random 43–128 character string, never sent over front channel
- Code challenge — `BASE64URL(SHA-256(verifier))`, sent in the authorization request
- `S256` method — the only method that should be used; `plain` offers no real protection
- AS stores the challenge — compares against re-derived hash at token exchange

**Answer**

PKCE works by having the client generate a random secret called the code verifier before starting the flow, then sending only a hashed version of it (the code challenge) to the Authorization Server at the start, and proving knowledge of the original secret at the token exchange step. The code verifier is a cryptographically random string of 43–128 characters, generated fresh for each authorization request and never sent over the front channel. The code challenge is computed as `BASE64URL(SHA-256(ASCII(code_verifier)))` when using the `S256` method, which is the only method that should be used in practice since the `plain` method offers no real protection. The client sends `code_challenge` and `code_challenge_method=S256` in the initial `/authorize` request; the AS stores these alongside the issued code. At token exchange, the client sends the `code_verifier`; the AS hashes it with SHA-256, base64url-encodes it, and confirms it matches the stored challenge before issuing tokens. An attacker who intercepted the code and the code challenge cannot reverse SHA-256 to produce the verifier, so the exchange fails.

```
// Pseudocode
verifier = base64url(random_bytes(32))
challenge = base64url(sha256(verifier))

// Step 1 — authorization request
GET /authorize?code_challenge={challenge}&code_challenge_method=S256&...

// Step 2 — token exchange
POST /token  body: code={code}&code_verifier={verifier}&...
```

---

## Q8. What is the Client Credentials grant type, and when is it the appropriate choice?

**Concepts**
- Client Credentials — machine-to-machine flow, no user involved
- Token `sub` — identifies the client, not a human user
- Client secret as sole credential — secure storage is mandatory
- Not suitable for browsers or mobile — no secure client secret storage

**Answer**

The Client Credentials grant is a machine-to-machine flow where the client authenticates directly to the Authorization Server using its own credentials — a client ID and client secret, or a private key JWT — and receives an access token that represents the client itself rather than any user. There is no user redirect or consent screen because there is no user involved. This grant is appropriate for background jobs, microservices calling other microservices, scheduled tasks, and any server-side process that operates without a human user's involvement. The access token's `sub` claim, if present, identifies the client application rather than a human user, which means scopes and permissions must be configured at the client level in the Authorization Server rather than being granted per user. Because the client secret is the sole credential, it must be stored securely — in an environment variable, a secrets manager, or a managed identity — and rotated regularly. This grant should never be used in a browser or mobile app because there is no secure way to store a static client secret on a device controlled by the user.

---

## Q9. What is the Device Code grant type, and what problem does it address?

**Concepts**
- Device Code grant (RFC 8628) — decoupled authorization via a second device
- Device polling — `authorization_pending`, `slow_down`, then token response
- Input-constrained devices — smart TVs, CLI tools, IoT devices

**Answer**

The Device Code grant (defined in RFC 8628) allows a device with limited input capability — a smart TV, a CLI tool, a game console — to obtain tokens by having the user complete authorization on a separate device with a full browser. The device displays a short code and a URL; the user visits that URL on their phone or computer, enters the code, authenticates, and consents; the device polls the token endpoint until it receives the tokens. This grant solves the problem of devices that have no keyboard or where typing a URL and credentials is impractical. The device polls the AS at a controlled interval (typically 5 seconds) using `grant_type=urn:ietf:params:oauth:grant-type:device_code` until the user completes authentication or the device code expires. The AS returns `authorization_pending` while the user has not yet acted, `slow_down` if the device is polling too fast, or the token response once the user completes the flow. Because authorization happens on a trusted device the user controls, this grant can produce a fully authenticated session including an ID Token when OIDC scopes are requested.

---

## Q10. Why are the Implicit grant and Resource Owner Password Credentials (ROPC) grant considered legacy or deprecated?

**Concepts**
- Implicit grant — access token exposed in URL fragment, no refresh token, no PKCE support
- ROPC grant — client receives user's password directly, no MFA possible
- OAuth 2.1 — formally deprecates both grants
- Authorization Code + PKCE — recommended replacement for both

**Answer**

Both grants were deprecated in OAuth 2.1 because they carry inherent security weaknesses that cannot be fixed through configuration. The Implicit grant exposes tokens in the browser URL fragment, where they are visible to scripts and browser history; it also cannot be used with PKCE and does not support refresh tokens. The Implicit grant was originally designed for JavaScript apps before CORS existed, but modern browsers support CORS so the Authorization Code + PKCE flow works equally well for single-page apps without the security trade-offs. The ROPC grant defeats the core purpose of OAuth 2.0 by requiring the user to hand their credentials directly to the client application; it cannot support multi-factor authentication or federated login because the client intercepts the credential, and any breach of the client application exposes actual user passwords rather than just tokens.

| Grant | Core Problem | Recommended Replacement |
|---|---|---|
| Implicit | Access token returned in URL fragment; no refresh token; no PKCE support | Authorization Code + PKCE |
| ROPC | Client receives user's password directly; no MFA possible; breaks delegation model | Authorization Code + PKCE |

The ROPC grant is still seen in legacy enterprise SSO migrations but should be treated as a temporary bridge; any system still using ROPC is also a single point of compromise since a breach of the client exposes actual user passwords.

---

## Q11. What is an Access Token, and what information does it convey to the Resource Server?

**Concepts**
- Access token — scoped capability credential, short-lived bearer token
- JWT access token — self-contained, locally verifiable by the RS
- Opaque access token — requires introspection for claim discovery
- Scope enforcement — RS responsibility, not implied by token validity

**Answer**

An access token is a credential that represents the authorization granted to a client to access specific resources on behalf of a Resource Owner. It conveys the scopes (what the client is allowed to do), optionally the subject (who granted the access), and has a defined expiry — it tells the Resource Server what operations are permitted, not who the calling user is in an identity sense. Access tokens are intentionally short-lived (minutes to hours) to limit the exposure window if they are leaked; refresh tokens handle renewal without user interaction. A JWT access token can be validated locally by the Resource Server by verifying the signature, issuer, audience, and expiry without a network call to the Authorization Server, which is why JWT tokens are preferred in high-throughput APIs. An opaque access token is just a random string; the Resource Server must call the AS token introspection endpoint to learn its claims and validity status, which adds latency. The scopes in the access token tell the Resource Server what actions to permit, and the RS must enforce those scope checks itself — a valid, unexpired token with the wrong scopes must be rejected with 403, not accepted.

---

## Q12. What is a Refresh Token, and how does it differ from an Access Token in lifetime and purpose?

**Concepts**
- Refresh token — long-lived, sent only to the Authorization Server
- Refresh token rotation — new token issued on each use, family invalidated on reuse
- Storage security — HTTP-only Secure cookie or server-side session required
- Short access token lifetime — limits blast radius of token theft

**Answer**

A refresh token is a long-lived credential that a client can use to obtain a new access token from the Authorization Server without requiring the user to log in again. Its sole purpose is token renewal; it is never sent to the Resource Server. This separation of lifetime and purpose is intentional: short-lived access tokens limit blast radius if leaked, while long-lived refresh tokens maintain session continuity by silently renewing access.

| Property | Access Token | Refresh Token |
|---|---|---|
| Lifetime | Minutes to hours (short) | Hours to days or longer |
| Sent to | Resource Server | Authorization Server only |
| Purpose | Authorize API calls | Obtain new access tokens |
| Rotation | Replaced on expiry | Often rotated on each use |

Refresh tokens must be stored securely because they are long-lived; in web apps they should be stored server-side or in an HTTP-only, Secure, SameSite cookie, never in browser `localStorage` where XSS attacks can reach them. Refresh token rotation is a security practice where the AS issues a new refresh token each time the old one is used and invalidates all refresh tokens in the family if an old one is presented, since reuse of an already-used refresh token indicates potential theft. Confidential clients can hold refresh tokens more safely than public clients; for public clients, short-lived refresh tokens with rotation are essential.

---

## Q13. What is an ID Token, and why does OpenID Connect introduce it when OAuth 2.0 already has access tokens?

**Concepts**
- ID Token — JWT asserting who authenticated and when, for the client
- `aud` bound to the client_id — not intended for Resource Servers
- Standard claims: `sub`, `iss`, `aud`, `iat`, `exp`, `nonce`
- Using access token as identity — protocol anti-pattern OIDC fixes

**Answer**

An ID Token is a JWT issued by an OpenID Connect Authorization Server that contains claims about the authenticated user — their identity, when they authenticated, and how. OAuth 2.0 access tokens say "the client is allowed to do X"; the ID Token says "this specific user authenticated at this time and the assertion is for this specific client." The ID Token is consumed by the client application to establish a session, not by the Resource Server. Access tokens are designed for Resource Servers; their format and claims are not standardized for identity purposes and the access token may be an opaque string. The ID Token's audience (`aud`) is always the client that requested it, which means the client can validate the ID Token locally and be certain the token was meant specifically for it — not a token issued for a different client's API. Key claims in an ID Token include `sub` (the user's stable identifier), `iss` (the issuer), `aud` (the client ID), `iat` (issued at), `exp` (expiry), and `nonce` (replay protection). Using an access token as proof of user identity is a security mistake; only the ID Token provides that guarantee within the OIDC protocol.

---

## Q14. What is the structure of a JSON Web Token (JWT), and what does each part contain?

**Concepts**
- Three-part structure: header, payload, signature — dot-separated Base64URL
- Base64URL encoding — not encryption, payload is readable by anyone
- Registered claims: `iss`, `sub`, `aud`, `exp`, `iat`, `nbf`
- JWE vs JWS — encryption (confidentiality) vs signing (integrity)

**Answer**

A JSON Web Token consists of three Base64URL-encoded parts separated by dots: a header, a payload, and a signature. The header describes the token type and signing algorithm, the payload contains the claims (key-value pairs about the subject and the token itself), and the signature allows the recipient to verify the token has not been tampered with.

```
header.payload.signature

Example:
eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9   <- header
.eyJzdWIiOiIxMjMiLCJpc3MiOiJodHRwczovL2FzLmV4YW1wbGUuY29tIn0  <- payload
.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c  <- signature
```

The header typically contains `alg` (the signing algorithm, such as RS256 or ES256) and `typ` (always "JWT"). The payload contains registered claims like `iss`, `sub`, `aud`, `exp`, `iat`, and `nbf`, as well as any custom claims added by the Authorization Server. Base64URL encoding is not encryption — anyone can decode the header and payload and read all the claims in plain text; the signature only guarantees integrity, not confidentiality. The signature is computed over `BASE64URL(header) + "." + BASE64URL(payload)` using the key identified in the header, so any change to header or payload invalidates the signature. Sensitive data should never be placed in a JWT payload unless the token is also encrypted using JWE (JSON Web Encryption), which is a different standard from a plain signed JWT (JWS — JSON Web Signature).

---

## Q15. How is a JWT signed, and how does a Resource Server or client verify that signature?

**Concepts**
- RS256 — asymmetric, private key signs, public JWKS verifies
- HS256 — symmetric shared secret, only appropriate when AS and RS are the same system
- JWKS endpoint — `jwks_uri` from discovery document, cached with `kid` refresh
- Validation beyond signature — `iss`, `aud`, `exp` must all be checked

**Answer**

A JWT is signed using either a symmetric algorithm (HS256, which uses a shared secret) or an asymmetric algorithm (RS256 or ES256, which use a private key to sign and a public key to verify). The Authorization Server signs the JWT with its private key; recipients verify the signature using the corresponding public key, which the AS publishes at a well-known JWKS endpoint. With RS256, the RS fetches the AS's JSON Web Key Set from the discovery document's `jwks_uri` and validates the signature locally using the public key whose `kid` (key ID) matches the token header. With HS256, both the signer and verifier share the same secret; this is only appropriate when the AS and RS are the same system, because distributing the shared secret to every RS is a security risk. The JWKS endpoint returns the AS's public keys as JSON; clients typically cache these keys and refresh them when they encounter a `kid` they do not recognize, which handles key rotation gracefully. Signature verification alone is not sufficient; the verifier must also check that the `iss` claim matches the expected issuer, the `aud` claim includes the expected recipient, and the `exp` claim is in the future.

---

## Q16. What is OpenID Connect (OIDC), and how does it relate to OAuth 2.0?

**Concepts**
- OIDC — identity layer on top of OAuth 2.0, not a replacement
- `openid` scope — the signal that requests OIDC behavior
- ID Token, UserInfo endpoint, discovery document — three additions OIDC makes
- OIDC flows map to OAuth 2.0 grant types

**Answer**

OpenID Connect is an identity layer built directly on top of OAuth 2.0. It reuses OAuth 2.0's authorization flows and adds a standardized way to authenticate users and return identity information. Where OAuth 2.0 only defines how to obtain authorization to access resources, OIDC defines how to obtain a verified identity assertion in the form of an ID Token. OIDC is not a replacement for OAuth 2.0 but an extension; an OIDC-compliant server is also a fully compliant OAuth 2.0 server. OIDC adds three main things to OAuth 2.0: the ID Token (a JWT containing identity claims), the UserInfo endpoint (for fetching additional user claims), and a discovery document (for clients to auto-configure themselves). A client signals it wants OIDC behavior by including `openid` in the `scope` parameter; without that scope, the AS returns only an access token as per plain OAuth 2.0. OIDC defines standard flows that map to OAuth 2.0 grant types: the Authorization Code Flow (most common), the Implicit Flow (deprecated), and the Hybrid Flow (rarely used in practice).

---

## Q17. What is the fundamental difference between OAuth 2.0 and OpenID Connect in terms of what each one provides?

**Concepts**
- OAuth 2.0 — delegated authorization, answers "is this client allowed to do X?"
- OIDC — authentication, answers "who is this user and did they just authenticate?"
- ID Token — the mechanism that makes OIDC an authentication protocol
- "Social login with access token" anti-pattern — implementation coincidence, not specification

**Answer**

OAuth 2.0 provides delegated authorization — a way to grant a client permission to access resources on behalf of a user. OpenID Connect provides authentication — a way to verify who the user is and get a standardized identity assertion. OAuth 2.0 answers "is this client allowed to do X?"; OIDC answers "who is this user, and did they really just authenticate?".

| Dimension | OAuth 2.0 | OpenID Connect |
|---|---|---|
| Purpose | Authorization (access control) | Authentication (identity) |
| Token issued | Access token, Refresh token | ID Token (plus OAuth 2.0 tokens) |
| User identity | Not guaranteed | Guaranteed via ID Token |
| Standard claims | Not defined | Defined by OIDC specification |
| Discovery | Not specified | Defined at `/.well-known/openid-configuration` |

The practical consequence is that an OAuth 2.0 access token alone cannot safely log a user into an application; the OIDC ID Token is needed for that purpose because its `aud` is the client, its `nonce` ties it to a specific login request, and its signature proves the AS issued it. Many "Sign in with X" implementations use only the OAuth 2.0 access token to call a profile API and treat the response as authentication; this works by coincidence in practice (because major providers return consistent user IDs) but is incorrect by protocol — it relies on implementation behavior, not specification guarantees.

---

## Q18. What OIDC scopes are defined by the specification, and what claims does each scope expose?

**Concepts**
- `openid` scope — mandatory for any OIDC request
- Standard scopes: `profile`, `email`, `address`, `phone`
- Claims not guaranteed to be populated — AS returns only what it has
- Custom scopes — authorization servers may define their own claim sets

**Answer**

The OIDC specification defines five standard scopes: the mandatory `openid` scope plus `profile`, `email`, `address`, and `phone`. The `openid` scope is required for any OIDC request; omitting it results in plain OAuth 2.0 behavior and no ID Token. Each additional scope corresponds to a set of standard claims the Authorization Server may include in the ID Token or return from the UserInfo endpoint.

| Scope | Claims Returned |
|---|---|
| `openid` | `sub` (required), `iss`, `aud`, `exp`, `iat` |
| `profile` | `name`, `given_name`, `family_name`, `nickname`, `picture`, `website`, `locale`, `zoneinfo`, `updated_at` |
| `email` | `email`, `email_verified` |
| `address` | `address` (structured JSON with street, city, country, etc.) |
| `phone` | `phone_number`, `phone_number_verified` |

Not all claims are guaranteed to be populated; the AS returns only the claims it has for that user, so a client should handle absent claims defensively. Authorization Servers can define custom scopes and claims beyond these five — for example, an enterprise AS might define a `roles` scope that returns application role assignments. Claims requested via scope may be returned either in the ID Token directly or only from the UserInfo endpoint depending on AS configuration, so clients should be prepared to handle both delivery mechanisms.

---

## Q19. What is the UserInfo endpoint in OIDC, and when would a client call it instead of reading the ID Token?

**Concepts**
- UserInfo endpoint — protected OAuth 2.0 resource returning current user claims
- `sub` consistency check — `sub` in UserInfo must match `sub` in ID Token
- Fresh data — UserInfo reflects current state, not state at ID Token issuance
- ID Token size consideration — large claim sets moved to UserInfo

**Answer**

The UserInfo endpoint is a protected OAuth 2.0 resource endpoint that returns claims about the authenticated user. A client calls it by presenting the access token as a Bearer token; the AS verifies the token and returns the user's claims as a JSON object. It is an alternative to embedding all claims in the ID Token, which is useful when the claim set would make the ID Token too large since ID Tokens may travel as URL parameters in some flows. Claims returned by the UserInfo endpoint must have a `sub` that matches the `sub` in the ID Token; if they do not match, the client must reject the response as it may indicate a token substitution attack. Fetching from UserInfo is always a fresh network call to the AS, so it returns the most current profile data, while claims embedded in the ID Token reflect the state at issuance time — making UserInfo the right choice when claims like email or profile data change frequently. Some implementations always return the same claims in both places; others put only `sub` and timestamps in the ID Token and push everything else to UserInfo, so the client must handle both configurations.

---

## Q20. What is the OpenID Connect discovery document, where is it found, and what information does it contain?

**Concepts**
- Discovery document — JSON metadata at `{issuer}/.well-known/openid-configuration`
- Auto-configuration — clients fetch once at startup, no manual endpoint configuration
- `jwks_uri` — location of the AS's public key set for signature verification
- Public, unauthenticated endpoint — contains no secrets, only metadata

**Answer**

The OIDC discovery document is a JSON metadata document that describes an Authorization Server's capabilities and endpoint URLs. It is always located at `{issuer}/.well-known/openid-configuration`, as defined by RFC 8414. Clients fetch this document at startup to configure themselves automatically rather than requiring manual endpoint configuration. Key fields include `issuer` (the AS's canonical URL), `authorization_endpoint`, `token_endpoint`, `userinfo_endpoint`, `jwks_uri`, `registration_endpoint`, and lists of supported `response_types`, `grant_types`, `scopes`, `claims`, and signing algorithms. The `jwks_uri` field tells clients where to fetch the AS's public key set for signature verification; most libraries cache the JWKS and refresh it when they encounter an unknown `kid`. Libraries like `Microsoft.AspNetCore.Authentication.OpenIdConnect` fetch the discovery document automatically and keep the endpoint URLs and keys up to date without requiring the developer to hard-code individual URLs. The discovery document is a public, unauthenticated endpoint; it contains no secrets, only metadata about the AS's configuration, so it can safely be accessed by any client.

---

## Q21. What validations must a client perform on a received ID Token before trusting it?

**Concepts**
- Signature validation — JWKS lookup by `kid`
- Issuer and audience validation — `iss` must match expected, `aud` must include `client_id`
- Expiry and not-before — clock skew tolerance of 5 minutes or less
- Nonce validation — mismatch must reject the token, not degrade gracefully
- `alg: none` rejection — algorithm confusion attack vector

**Answer**

A client must validate seven properties of an ID Token before trusting it: the signature, the issuer, the audience, the expiry, the issued-at time, the nonce, and the algorithm. Skipping any of these checks opens the client to spoofing, replay, or token substitution attacks. Signature validation requires fetching the AS's JWKS and verifying the signature using the public key whose `kid` matches the token's header. Issuer validation means the `iss` claim must exactly match the expected issuer URL; even a subtle mismatch must cause rejection, since this prevents tokens from a different AS being accepted. Audience validation means the `aud` claim must include the client's own `client_id`; a token issued for a different client must be rejected even if everything else is valid. Expiry and not-before validation requires the current time to be between `nbf` and `exp` with a small tolerance for clock skew, typically five minutes or less. Nonce validation means the `nonce` claim in the ID Token must match the nonce the client generated for this specific authorization request; a mismatch indicates a replay attack and the token must be rejected rather than accepted with a warning. Algorithm validation means the client should only accept the algorithms it is explicitly configured to trust — RS256, ES256 — and must unconditionally reject tokens with `alg: none` regardless of the signature field.

---

## Q22. What validations should a Resource Server perform on an Access Token (JWT format) on every request?

**Concepts**
- Signature verification with cached JWKS — no network call per request
- `iss` and `aud` validation — issuer and audience must both match configured values
- Scope enforcement — 403 for insufficient scope, 401 for invalid token
- No per-request introspection for JWT — self-contained tokens defeat that purpose

**Answer**

A Resource Server must validate a JWT access token on every incoming request by verifying the signature, confirming the issuer, confirming the audience, checking the expiry, and enforcing the scopes required for the requested operation. Signature verification uses the AS's public JWKS, cached locally and refreshed when an unknown `kid` is encountered; caching means a network call is not needed on every request. The `iss` claim is compared against the configured trusted issuer; tokens from any other issuer are rejected with HTTP 401. The `aud` claim must include the RS's own identifier; tokens issued for a different resource server are rejected even if the signature is valid, since a valid signature alone only proves the token was issued by the AS — it does not prove the token was meant for this RS. Scope enforcement is the RS's responsibility: if the token's `scope` claim does not include the scope required for the API endpoint being called, the RS returns HTTP 403, not 401, because the token itself is valid but insufficient for this operation. The RS should not call the AS introspection endpoint on every request for JWT tokens since that defeats the purpose of self-contained tokens; introspection is reserved for opaque tokens or when immediate revocation detection is required.

---

## Q23. What is the nonce parameter in OIDC, and what attack does it prevent?

**Concepts**
- Nonce — random per-request value bound into the ID Token
- ID Token replay attack — intercepted token used on a different session
- Client storage — nonce stored in session or signed cookie until validation
- Mandatory nonce check — mismatch must reject, never degrade gracefully

**Answer**

The nonce is a random value the client includes in the authorization request; the Authorization Server binds it to the ID Token it issues by embedding the same value in the `nonce` claim. When the client receives the ID Token, it verifies that the nonce in the token matches the one it generated, which prevents ID Token replay attacks. Without the nonce, an attacker who intercepts an ID Token in transit could present it to the same or a different client and establish a session as the victim, because the token would pass all other validations. The client must generate a new cryptographically random nonce for each authorization request and store it — typically in a server-side session or a signed, HTTP-only cookie — until it can be compared against the token's `nonce` claim after the redirect returns. The nonce is mandatory in the OIDC Implicit Flow and strongly recommended in the Authorization Code Flow; most OIDC client libraries handle nonce generation and validation automatically. A failed nonce check must result in the ID Token being rejected entirely; the client should not degrade gracefully on a nonce mismatch since doing so eliminates the replay protection the nonce was designed to provide.

---

## Q24. What is token introspection, and when is it used instead of local JWT validation?

**Concepts**
- Token introspection (RFC 7662) — RS asks AS for real-time token status
- Opaque tokens — require introspection; no local decoding possible
- Immediate revocation detection — introspection catches revoked JWT before expiry
- Performance trade-off — synchronous HTTP round-trip vs local CPU validation

**Answer**

Token introspection (RFC 7662) is a protocol by which a Resource Server asks the Authorization Server in real time whether a given token is currently active and what its claims are. The RS sends the token to the AS's introspection endpoint with its own credentials, and the AS responds with a JSON object including an `active` boolean and the token's claims if it is active. Introspection is used for opaque (non-JWT) tokens where the RS cannot decode the token locally; it is the only way to learn the token's claims in that case. Introspection is also appropriate when immediate revocation detection is required — for example, after a user logs out or an account is suspended; a locally cached JWT may still appear valid by signature until it expires, but introspection will return `active: false` immediately after revocation. The performance trade-off is significant: introspection adds a synchronous HTTP round-trip to every API call, whereas local JWT validation is a pure CPU operation with no network dependency. A common hybrid approach caches introspection results for a short period (30 seconds to a minute) to reduce the load on the AS while still catching recently revoked tokens faster than waiting for JWT expiry.

---

## Q25. What is token revocation, and why does it matter for short-lived versus long-lived tokens?

**Concepts**
- Token revocation (RFC 7009) — client or AS declares token invalid before expiry
- Refresh token revocation — cascade must invalidate the entire token family
- JWT access token revocation gap — RS validates locally, cannot detect revocation
- Short access token lifetime — primary mitigation for the revocation gap

**Answer**

Token revocation (RFC 7009) is a mechanism that allows a client or Authorization Server to declare a token invalid before its natural expiry. A client calls the revocation endpoint when a user explicitly logs out; the AS marks the token as revoked in its store. Revocation matters most for long-lived tokens — specifically refresh tokens — because a short-lived access token will expire quickly on its own, but a long-lived refresh token that is leaked or stolen can be used for days or weeks unless explicitly revoked. For JWT access tokens, revocation has limited practical effect unless the Resource Server actively checks a revocation list or uses introspection, because the RS validates JWTs locally and cannot know the token has been revoked; this is the fundamental revocation gap in the JWT model. This asymmetry is why access tokens should have short lifetimes (5–15 minutes is common) — even if revocation is not enforced at the RS, a short-lived token limits the damage window significantly. Refresh token revocation must cascade: revoking a refresh token should also invalidate any access tokens derived from it, and the AS should invalidate the entire token family if refresh token reuse is detected, since reuse signals potential theft.

---

## Q26. What is the difference between the front channel and the back channel in an OAuth/OIDC flow, and why does the distinction matter for security?

**Concepts**
- Front channel — browser redirects, URL parameters, visible to logs and scripts
- Back channel — direct server-to-server HTTPS, bypasses browser entirely
- Authorization code — front-channel transport, short-lived single-use by design
- Token exchange — back-channel operation, keeps actual tokens out of the browser

**Answer**

The front channel refers to communication that passes through the user's browser — typically HTTP redirects and URL parameters — where the data is visible in browser history, referrer headers, and network logs. The back channel refers to direct server-to-server HTTPS calls that bypass the browser entirely. The distinction matters because front-channel communication is inherently less secure: values passed in URLs can be intercepted, logged, or leaked by browser extensions, intermediary proxies, or server-side logging. In the Authorization Code flow, the authorization code travels over the front channel (in a redirect URL query parameter), which is why it is short-lived and single-use — if it is intercepted from a log, it provides only a narrow window of opportunity and cannot be redeemed without the client secret or PKCE verifier. The token exchange — where the code is swapped for access and refresh tokens — happens over the back channel (a direct POST from the client's server to the AS's token endpoint), keeping actual tokens away from the browser entirely. Deprecated grants like Implicit exposed the access token over the front channel in a URL fragment, which is one of the primary reasons they are considered insecure. PKCE adds front-channel security by ensuring that even if the code is intercepted from the front channel, it cannot be redeemed without the back-channel code verifier.

---

## Q27. How does an Authorization Code flow with PKCE differ from the original Authorization Code flow with a client secret, and which is preferred for public clients?

**Concepts**
- Client secret — static, shared, server-side only
- PKCE code verifier — dynamic, per-request, never stored
- Public client — no secure client secret storage possible
- RFC 9700 — recommends PKCE for all clients, including confidential

**Answer**

The original Authorization Code flow uses a static client secret to prove the client's identity at the token endpoint; PKCE replaces (or supplements) that with a cryptographic challenge generated per-request. For public clients — single-page apps, mobile apps, desktop apps — PKCE is mandatory because there is no secure way to store a static client secret on a device controlled by the user; a mobile app that embeds a client secret is effectively a public secret since anyone can decompile the app and extract the value.

| Property | Client Secret | PKCE |
|---|---|---|
| Secret type | Static, shared | Dynamic, per-request |
| Storage | Server-side only (secure) | Not stored anywhere (derived) |
| Suitable for | Confidential (server-side) clients | Public clients and all clients |
| Replay protection | By secret | By one-time code verifier |

PKCE provides equivalent protection to a client secret without requiring a secret to exist at all; the code verifier is derived fresh for each flow and is never stored anywhere. For confidential (server-side) clients, using both a client secret and PKCE provides defense in depth: an attacker who steals the authorization code cannot use it even if they also know the client ID, since they have neither the secret nor the code verifier. RFC 9700 (OAuth 2.0 Security Best Current Practice) recommends PKCE for all clients, including confidential ones, as of 2023, making the distinction primarily about whether a client secret is also used rather than whether PKCE is used.

---

## Q28. Can an OAuth 2.0 Access Token alone be used to authenticate a user — that is, to prove who the user is? Why or why not?

**Concepts**
- Access token audience — Resource Server, not the client
- No session binding — access token not tied to the current authentication event
- Token substitution attack — valid access token reused across different contexts
- OIDC ID Token — the correct mechanism for client-side authentication assertions

**Answer**

An OAuth 2.0 access token alone cannot be used to authenticate a user because OAuth 2.0 does not define a standard format for access tokens, does not guarantee that the token contains user identity information, and does not bind the token to a specific user authentication event in a way the client can verify. The access token's audience is the Resource Server, not the client; even if the RS returns profile data when called with the token, the client has no way to verify that the token was issued as a result of the current user's authentication event rather than some earlier session. An attacker could obtain an access token for a different purpose — such as accessing a file storage API — and replay it against a profile endpoint to impersonate a user in a poorly designed login flow, since the access token carries no client-bound nonce or session binding. OpenID Connect solves this precisely: the ID Token's `aud` is the client itself, the `nonce` ties the token to a specific login request, and the signature proves the AS issued it as a deliberate authentication assertion — together these form an authentication guarantee the client can safely trust. Many "Sign in with X" implementations that use only the OAuth 2.0 access token to call a profile API are technically correct in practice because major providers return consistent user IDs, but incorrect by protocol; they are relying on implementation coincidence, not specification guarantees, which means the security depends on the provider's behavior remaining stable rather than on any enforceable protocol property.

---

## Gotchas — OAuth 2.0 & OIDC (Interview Traps)

---

#### Gotcha 1. OAuth 2.0 Is Not Authentication

**Concepts**
- Access token — authorization credential, not an identity assertion
- Session binding absent — no tie to the current authentication event
- OIDC ID Token — the protocol-correct authentication mechanism

**Answer**

OAuth 2.0 is a delegated authorization protocol, not an authentication protocol. Using an OAuth 2.0 access token to "log in" a user by calling a profile endpoint is relying on a side effect, not a protocol guarantee. The access token does not contain any binding to the current browser session or the current login event, so it cannot prove that a user just authenticated; it only proves that someone, at some point, authorized a client to call a specific API. An attacker who obtains a valid access token from any session can call the same profile endpoint and receive the same user data, which means the "authentication" succeeds even for a stolen token. OpenID Connect exists specifically to fill this gap by adding the ID Token, which is an authentication assertion with audience, nonce, and expiry guarantees that make it safe for the client to use as proof of authentication.

---

#### Gotcha 2. JWT Payloads Are Not Encrypted by Default

**Concepts**
- JWS — JSON Web Signature, provides integrity but no confidentiality
- Base64URL — encoding, not encryption; payload is readable
- JWE — JSON Web Encryption, required for confidentiality

**Answer**

A standard signed JWT (JWS — JSON Web Signature) is Base64URL-encoded, not encrypted. Anyone who intercepts the token can decode the header and payload and read all the claims in plain text; no key is required to decode the content, only to verify the signature. The signature prevents tampering but provides no confidentiality, so sensitive personal data — email addresses, phone numbers, internal user IDs, permissions — placed in a JWT payload is readable by anyone who can obtain the token, including the user themselves, browser extensions, or network intermediaries. Never put passwords, private keys, or sensitive PII in a JWT payload unless the token is also encrypted using JWE (JSON Web Encryption), which is a distinct standard that wraps the signed JWT in an encrypted envelope. This is a common source of data exposure when developers see Base64URL encoding and assume the encoded format implies secrecy.

---

#### Gotcha 3. Validating the Signature Is Not Enough

**Concepts**
- Signature — proves the token was signed by the key holder, nothing more
- `aud` validation — required to prevent token substitution across clients
- `iss` validation — required to prevent tokens from rogue issuers being accepted

**Answer**

Verifying a JWT's signature confirms the token was signed by a holder of the private key, but it does not confirm the token was meant for the current recipient, is still valid, or came from the expected issuer. An attacker could take a valid JWT issued by the same AS for a different client and present it to a Resource Server; signature validation alone would accept it because the signature is genuinely valid. Full validation requires checking `iss` (the issuer must match the trusted AS), `aud` (the audience must include this client or RS's identifier), `exp` and `nbf` (the token must be currently valid), and `nonce` (for ID Tokens in OIDC flows) in addition to the signature. Skipping `aud` validation is one of the most common JWT security mistakes in production systems and the one most often exploited in token substitution attacks.

---

#### Gotcha 4. The Refresh Token Is More Sensitive Than the Access Token

**Concepts**
- Refresh token longevity — hours to days vs minutes for access tokens
- XSS exposure via localStorage — JavaScript can read and exfiltrate it
- Refresh token rotation with family invalidation — reuse detection mechanism

**Answer**

Because a refresh token is long-lived and can be used to obtain new access tokens indefinitely, compromising a refresh token is far more damaging than compromising a short-lived access token. The short lifetime of the access token is part of the security design — a leaked access token expires in minutes, limiting the damage window — but a leaked refresh token can maintain access for days until explicitly revoked. Storing a refresh token in browser `localStorage` or a non-HTTP-only cookie exposes it to Cross-Site Scripting (XSS) attacks since JavaScript running in the page context can read and exfiltrate it; it should be stored in an HTTP-only, Secure, SameSite cookie or a server-side session. Refresh token rotation with family invalidation is the defense against stolen refresh tokens: when a previously used refresh token is presented, the AS invalidates the entire token family on the assumption that theft occurred, forcing the user to re-authenticate — this detects theft at the cost of occasionally terminating legitimate sessions.

---

#### Gotcha 5. `state` and `nonce` Serve Different Purposes

**Concepts**
- `state` — CSRF protection, binds response to browser session
- `nonce` — ID Token replay protection, binds token to specific login request
- Not interchangeable — both should be used in OIDC Authorization Code flows

**Answer**

The `state` parameter protects the authorization request against Cross-Site Request Forgery by binding the redirect response to a specific browser session. The `nonce` protects the ID Token against replay attacks by binding the token to a specific login request. They are not interchangeable and both should be used in OIDC flows because they protect against different attack vectors. The `state` is echoed back in the redirect response query parameters and validated by the client before processing the authorization code; a mismatch means a CSRF attempt is in progress — someone else initiated the authorization request on behalf of this user. The `nonce` is embedded in the ID Token by the AS and validated by the client after receiving and decoding the token; a mismatch means the token was replayed from a different session or a different login request. Omitting `state` opens the application to CSRF; omitting `nonce` opens it to ID Token replay; most OIDC client libraries generate and validate both automatically, so the gotcha is when developers implement the flow manually and overlook one or both parameters.

---
