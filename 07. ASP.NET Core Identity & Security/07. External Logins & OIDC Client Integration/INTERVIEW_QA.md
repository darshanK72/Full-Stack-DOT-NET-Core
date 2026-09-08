# External Logins & OIDC Client Integration — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is the purpose of `AddAuthentication` and how do you wire up external login providers such as Google or Microsoft in an ASP.NET Core application?](#q1-what-is-the-purpose-of-addauthentication-and-how-do-you-wire-up-external-login-providers-such-as-google-or-microsoft-in-an-aspnet-core-application)
2. [Q2. How does the ExternalLoginCallback flow work from the moment a user clicks "Login with Google" to the moment they are signed into your application?](#q2-how-does-the-externallogincallback-flow-work-from-the-moment-a-user-clicks-login-with-google-to-the-moment-they-are-signed-into-your-application)
3. [Q3. What is the role of `UserManager.AddLoginAsync` and the `AspNetUserLogins` table in an external login flow?](#q3-what-is-the-role-of-usermanageraddloginasync-and-the-aspnetuserlogins-table-in-an-external-login-flow)
4. [Q4. What does `SignInManager.ExternalLoginSignInAsync` return and what must your application do when that return value indicates no existing link was found?](#q4-what-does-signinmanagerexternalloginsigninasync-return-and-what-must-your-application-do-when-that-return-value-indicates-no-existing-link-was-found)
5. [Q5. How do you extract claims such as email, display name, and profile picture from an external provider inside the ExternalLoginCallback action?](#q5-how-do-you-extract-claims-such-as-email-display-name-and-profile-picture-from-an-external-provider-inside-the-externallogincallback-action)
6. [Q6. What is `SignInManager.GetExternalLoginInfoAsync` and why must it be called before processing an external login?](#q6-what-is-signinmanagergetexternallogininfoasync-and-why-must-it-be-called-before-processing-an-external-login)
7. [Q7. What configuration options are mandatory when calling `AddOpenIdConnect`, and what does each one represent?](#q7-what-configuration-options-are-mandatory-when-calling-addopenidconnect-and-what-does-each-one-represent)
8. [Q8. What is the `Authority` option in `AddOpenIdConnect` and how does the middleware use it to discover provider endpoints?](#q8-what-is-the-authority-option-in-addopenidconnect-and-how-does-the-middleware-use-it-to-discover-provider-endpoints)
9. [Q9. What `ResponseType` values can you specify in an OIDC configuration and how does each affect what tokens or codes are returned?](#q9-what-responsetype-values-can-you-specify-in-an-oidc-configuration-and-how-does-each-affect-what-tokens-or-codes-are-returned)
10. [Q10. What `Scope` values are typically requested in an OIDC flow, and why does requesting the `offline_access` scope matter?](#q10-what-scope-values-are-typically-requested-in-an-oidc-flow-and-why-does-requesting-the-offline_access-scope-matter)
11. [Q11. What is the `CallbackPath` option in `AddOpenIdConnect` and how does ASP.NET Core use it to complete the login round-trip?](#q11-what-is-the-callbackpath-option-in-addopenidconnect-and-how-does-aspnet-core-use-it-to-complete-the-login-round-trip)
12. [Q12. What does the `SaveTokens` option do in `AddOpenIdConnect`, and how do you later retrieve the stored tokens from the HTTP context?](#q12-what-does-the-savetokens-option-do-in-addopenidconnect-and-how-do-you-later-retrieve-the-stored-tokens-from-the-http-context)
13. [Q13. What is the difference between backchannel token validation and front-channel token delivery in OIDC?](#q13-what-is-the-difference-between-backchannel-token-validation-and-front-channel-token-delivery-in-oidc)
14. [Q14. What is the `state` parameter in an OAuth 2.0 authorization request and how does it protect against Cross-Site Request Forgery (CSRF) attacks?](#q14-what-is-the-state-parameter-in-an-oauth-20-authorization-request-and-how-does-it-protect-against-cross-site-request-forgery-csrf-attacks)
15. [Q15. What are correlation cookies in ASP.NET Core OAuth middleware, and how do they tie together the outgoing request and the incoming callback?](#q15-what-are-correlation-cookies-in-aspnet-core-oauth-middleware-and-how-do-they-tie-together-the-outgoing-request-and-the-incoming-callback)
16. [Q16. What is the OIDC nonce, where is it stored, and how does the middleware use it to prevent token replay attacks?](#q16-what-is-the-oidc-nonce-where-is-it-stored-and-how-does-the-middleware-use-it-to-prevent-token-replay-attacks)
17. [Q17. How do you configure multiple external providers — for example Google, Microsoft, and a custom OIDC provider — in a single ASP.NET Core application?](#q17-how-do-you-configure-multiple-external-providers-for-example-google-microsoft-and-a-custom-oidc-provider-in-a-single-aspnet-core-application)
18. [Q18. How does account linking work when a user who already has a local account wants to connect an external provider, and what code is involved?](#q18-how-does-account-linking-work-when-a-user-who-already-has-a-local-account-wants-to-connect-an-external-provider-and-what-code-is-involved)
19. [Q19. What is the purpose of `IdentityConstants.ExternalScheme`, why is the cookie it creates short-lived, and how does it differ from the main application cookie?](#q19-what-is-the-purpose-of-identityconstantsexternalscheme-why-is-the-cookie-it-creates-short-lived-and-how-does-it-differ-from-the-main-application-cookie)
20. [Q20. What GDPR considerations arise when your application receives and stores personal data from an external provider, and how do you address them?](#q20-what-gdpr-considerations-arise-when-your-application-receives-and-stores-personal-data-from-an-external-provider-and-how-do-you-address-them)
21. [Q21. How should `ClientId` and `ClientSecret` be managed in ASP.NET Core applications, and what are the risks of storing them in source control?](#q21-how-should-clientid-and-clientsecret-be-managed-in-aspnet-core-applications-and-what-are-the-risks-of-storing-them-in-source-control)
22. [Q22. What is the difference between OAuth 2.0 and OpenID Connect (OIDC), and when would you use each?](#q22-what-is-the-difference-between-oauth-20-and-openid-connect-oidc-and-when-would-you-use-each)
23. [Q23. What is Proof Key for Code Exchange (PKCE), when is it required in an OIDC flow, and how does ASP.NET Core support it?](#q23-what-is-proof-key-for-code-exchange-pkce-when-is-it-required-in-an-oidc-flow-and-how-does-aspnet-core-support-it)
24. [Q24. How do you handle the `RemoteFailure` event that can occur during an external login attempt?](#q24-how-do-you-handle-the-remotefailure-event-that-can-occur-during-an-external-login-attempt)
25. [Q25. How do you apply claims transformation after receiving the external provider's identity so that your application's internal claims are enriched or normalized?](#q25-how-do-you-apply-claims-transformation-after-receiving-the-external-providers-identity-so-that-your-applications-internal-claims-are-enriched-or-normalized)
26. [Q26. How does signing out of an external provider (federated sign-out) work in an OIDC integration, and what ASP.NET Core mechanisms support it?](#q26-how-does-signing-out-of-an-external-provider-federated-sign-out-work-in-an-oidc-integration-and-what-aspnet-core-mechanisms-support-it)
27. [Q27. What risks arise if an application blindly trusts the email claim from an external provider without additional verification, and how do you mitigate them?](#q27-what-risks-arise-if-an-application-blindly-trusts-the-email-claim-from-an-external-provider-without-additional-verification-and-how-do-you-mitigate-them)
28. [Q28. How do you handle token expiry and refresh when `SaveTokens` is enabled and the access token stored in the cookie expires?](#q28-how-do-you-handle-token-expiry-and-refresh-when-savetokens-is-enabled-and-the-access-token-stored-in-the-cookie-expires)

---

## Q1. What is the purpose of `AddAuthentication` and how do you wire up external login providers such as Google or Microsoft in an ASP.NET Core application?

**Concepts**
- `AddAuthentication` — registers authentication services and sets the default scheme
- `RemoteAuthenticationHandler` — base for all external provider handlers
- `CallbackPath` — scheme-specific path the middleware intercepts on return
- Provider-specific extensions: `AddGoogle`, `AddMicrosoftAccount`, `AddOpenIdConnect`

**Answer**

`AddAuthentication` registers the authentication services in the DI container and establishes the default scheme used to sign in, challenge, and sign out users. It accepts a default scheme name — commonly `CookieAuthenticationDefaults.AuthenticationScheme` — so the framework knows where to persist the identity after a successful login. External providers are added by chaining provider-specific extensions after the `AddAuthentication` call; each registers a `RemoteAuthenticationHandler` that handles the redirect to the provider and the callback back to the application. Each external handler is independent and identified by its scheme name, so any number of providers can be registered simultaneously; each handler responds only to requests whose path matches its own `CallbackPath`, which means they do not interfere with one another. The `GoogleOptions` type accepted by `AddGoogle` exposes `ClientId`, `ClientSecret`, and optional `Scope` settings, making the wiring declarative.

```csharp
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddGoogle(o => { o.ClientId = "..."; o.ClientSecret = "..."; });
```

---

## Q2. How does the ExternalLoginCallback flow work from the moment a user clicks "Login with Google" to the moment they are signed into your application?

**Concepts**
- Challenge to external scheme — triggers redirect to provider's authorization endpoint
- Back-channel token exchange — middleware exchanges code for tokens server-to-server
- `IdentityConstants.ExternalScheme` cookie — transient bridge from callback to action
- `ExternalLoginSignInAsync` — looks up local account, issues application cookie

**Answer**

When the user clicks the external login button, a challenge to the Google scheme is issued, which redirects the browser to Google's authorization endpoint with a crafted URL including the application's callback address. The middleware constructs the authorization URL using the registered `ClientId`, a random `state` parameter for CSRF protection, and the `CallbackPath` as the redirect URI. Google authenticates the user and redirects back to the `CallbackPath` with an authorization code; the middleware validates the `state`, then exchanges the code for an ID token and access token via a direct server-to-server HTTPS call to Google's token endpoint, never exposing the tokens through the browser. After that back-channel exchange succeeds, the middleware extracts claims from the ID token and stores them in a short-lived external authentication cookie using the `IdentityConstants.ExternalScheme` scheme. The `ExternalLoginCallback` action then calls `SignInManager.GetExternalLoginInfoAsync()` to read that cookie, calls `SignInManager.ExternalLoginSignInAsync()` to find a matching local account, and if one is found, issues the application session cookie and clears the external cookie. If no matching local account exists, the action redirects to a registration page where the user confirms their email and `UserManager.AddLoginAsync` is called to create the link.

---

## Q3. What is the role of `UserManager.AddLoginAsync` and the `AspNetUserLogins` table in an external login flow?

**Concepts**
- `AspNetUserLogins` — composite primary key of `LoginProvider` + `ProviderKey`
- `ProviderKey` — stable `sub` claim, not the email address
- Single user, multiple login rows — supports linking multiple external accounts
- `AddLoginAsync` idempotency — returns `IdentityResult` failure, not an exception, on duplicate

**Answer**

`UserManager.AddLoginAsync` records the association between a local ASP.NET Core Identity user and a specific external provider account by inserting a row into the `AspNetUserLogins` table. This row contains the provider name, the provider-assigned user key (subject identifier), and the display name, forming a durable link so future logins from the same external account are recognized without requiring the user to go through registration again. The `AspNetUserLogins` table has a composite primary key of `LoginProvider` and `ProviderKey`, ensuring a user cannot accidentally add the same external account twice. The `ProviderKey` is the stable identifier issued by the external provider — for Google this is the `sub` claim — rather than an email address, because emails can change while the `sub` remains constant. `AddLoginAsync` should be called only after confirming that no existing link exists; calling it on an already-linked account returns an `IdentityResult` failure rather than throwing an exception. A single local user can have many rows in `AspNetUserLogins`, meaning the same account can be linked to Google, Microsoft, and GitHub simultaneously.

---

## Q4. What does `SignInManager.ExternalLoginSignInAsync` return and what must your application do when that return value indicates no existing link was found?

**Concepts**
- `SignInResult` — discriminated result type with four states
- `SignInResult.Failed` in external context — provider key not yet linked
- `SignInResult.IsLockedOut` — account exists but is locked
- `SignInResult.IsNotAllowed` — email or phone confirmation required

**Answer**

`ExternalLoginSignInAsync` returns a `SignInResult` value that tells the application whether the sign-in succeeded, failed due to lockout, or failed because no matching `AspNetUserLogins` row exists for the provided provider and key. When the result indicates no link was found, the application must redirect the user through a registration or account-linking flow before a session can be established. `SignInResult.Succeeded` means a matching local account was found, the account is not locked out, and a session cookie has been issued; the external authentication cookie is automatically cleared at this point. `SignInResult.Failed` in the external login context typically means the provider key is not yet associated with any local user, so the application should redirect to a page where the user can create a new account or link to an existing one. `SignInResult.IsLockedOut` means the local account exists but is currently locked, and the application should show a lockout message rather than offering registration. `SignInResult.IsNotAllowed` means the local account exists but `EmailConfirmed` or `PhoneNumberConfirmed` restrictions prevent login, so the user must be guided to confirm their contact method first.

---

## Q5. How do you extract claims such as email, display name, and profile picture from an external provider inside the ExternalLoginCallback action?

**Concepts**
- `ExternalLoginInfo.Principal.Claims` — holds all claims from the ID token and user-info response
- Scope dependency — claims are absent unless the corresponding scope was requested
- `ClaimActions.MapJsonKey` — maps user-info JSON fields to named claim types
- Transient claims — must be explicitly persisted if needed beyond the current request

**Answer**

After calling `SignInManager.GetExternalLoginInfoAsync()`, the returned `ExternalLoginInfo` object contains a `Principal` property whose `Claims` collection holds everything the provider included in the ID token or user-info endpoint response. Specific claims are read using standard `ClaimTypes` constants or the provider's own claim URI — common types include `ClaimTypes.Email`, `ClaimTypes.Name`, and `ClaimTypes.GivenName`; Google also includes a picture URL under the non-standard URI `"picture"` from the user-info endpoint. The claims available depend on the scopes requested: the `profile` scope brings name and picture, and the `email` scope brings the email address, so without the appropriate scope the claim is simply absent from the collection. Additional claims from provider-specific APIs can be mapped using `ClaimActions.MapJsonKey` on `GoogleOptions`, which maps a JSON field from the user-info response to a named claim type without requiring custom code. Claims extracted here are transient — they exist only for the duration of the external cookie; if they need to be persisted to the local user record, they must be explicitly copied into `ApplicationUser` properties or inserted into the `AspNetUserClaims` table.

---

## Q6. What is `SignInManager.GetExternalLoginInfoAsync` and why must it be called before processing an external login?

**Concepts**
- Reads and validates `IdentityConstants.ExternalScheme` cookie
- Returns `null` on expiry — always null-check the result
- Consumes the cookie — single-read semantics
- `LoginProvider` string — matches the scheme name registered with `AddGoogle`

**Answer**

`GetExternalLoginInfoAsync` reads and validates the short-lived external authentication cookie (scheme `IdentityConstants.ExternalScheme`) that the OAuth or OIDC middleware wrote when it completed the provider callback, returning the provider name, provider key, and the external `ClaimsPrincipal`. It must be called first because the entire external identity is stored only in that transient cookie; without reading it, the callback action has no knowledge of who authenticated or with which provider. If the external cookie has expired — its default lifetime is typically a few minutes — `GetExternalLoginInfoAsync` returns `null`, so the result must always be null-checked and a redirect back to the login page issued when it is missing. The method also deletes the external cookie internally once the `ExternalLoginInfo` has been extracted, ensuring the short-lived credential is consumed exactly once. The returned `ExternalLoginInfo.LoginProvider` string matches the scheme name registered with `AddGoogle` or similar, which is the value stored in `AspNetUserLogins.LoginProvider` when the association is created.

---

## Q7. What configuration options are mandatory when calling `AddOpenIdConnect`, and what does each one represent?

**Concepts**
- `Authority` — base URL for discovery document fetch
- `ClientId` and `ClientSecret` — application credentials for token exchange
- `ResponseType` — defaults to `"code"` for Authorization Code flow
- `CallbackPath` — must match redirect URI registered at the identity provider

**Answer**

The three essential options are `Authority`, `ClientId`, and `ClientSecret`. `Authority` points the middleware to the identity provider's base URL so it can perform discovery by fetching `{Authority}/.well-known/openid-configuration`, which contains the authorization, token, and JWKS endpoint URLs automatically. `ClientId` identifies the application to the provider and is included in every authorization request; `ClientSecret` is the shared secret used to authenticate the application during the back-channel token exchange and must be treated like a password. `ResponseType` defaults to `"code"` for the Authorization Code flow, which is the recommended flow for server-side applications because tokens are never exposed in the browser URL. `CallbackPath` is optional but important: it defaults to `/signin-oidc` and must exactly match the redirect URI registered with the identity provider — a mismatch causes the provider to reject the redirect before the user can authenticate. `SaveTokens` defaults to `false` and should only be set to `true` when access or refresh tokens need to be retrieved for downstream API calls.

---

## Q8. What is the `Authority` option in `AddOpenIdConnect` and how does the middleware use it to discover provider endpoints?

**Concepts**
- `Authority` — base URL appended with `/.well-known/openid-configuration`
- Discovery document cached at startup — restart re-fetches
- `jwks_uri` — location of public keys for ID token signature verification
- Disabling discovery — `MetadataAddress` or `Configuration` for non-OIDC providers

**Answer**

`Authority` is the base URL of the OpenID Connect identity provider; the middleware appends `/.well-known/openid-configuration` to it on startup to fetch the discovery document, a JSON file that advertises all the provider's endpoint URLs and supported algorithms. Because the middleware resolves all endpoint addresses from the discovery document, the developer does not have to configure authorization, token, or JWKS endpoints manually; the middleware handles all of this at runtime. The discovery document is fetched during the first authentication request and cached for the lifetime of the middleware; restarting the application causes it to be re-fetched, which handles provider key rotation automatically when a new `kid` appears in the JWKS. If the provider does not support OIDC discovery, automatic discovery can be disabled by setting `MetadataAddress` explicitly to point to a custom metadata endpoint, or by setting `Configuration` directly with an `OpenIdConnectConfiguration` object containing the endpoint URLs. For Azure Active Directory, the authority follows the pattern `https://login.microsoftonline.com/{tenantId}/v2.0`; for Auth0 it is `https://{domain}/`.

---

## Q9. What `ResponseType` values can you specify in an OIDC configuration and how does each affect what tokens or codes are returned?

**Concepts**
- `"code"` — Authorization Code flow, back-channel token exchange, recommended
- `"id_token"` / `"token"` — Implicit flows, tokens in redirect URL, deprecated
- `"code id_token"` — Hybrid flow, partial back-channel
- OAuth 2.1 deprecation of Implicit flows

**Answer**

The most common `ResponseType` values are `"code"` for the Authorization Code flow, `"id_token"` for the implicit ID token flow, `"token"` for the implicit access token flow, and `"code id_token"` for the hybrid flow. Modern best practices mandate the Authorization Code flow with PKCE and strongly discourage implicit flows because implicit flows expose tokens in the browser URL fragment, making them visible in browser history and server logs.

| ResponseType | Flow | Tokens in redirect URL | Back-channel exchange |
|---|---|---|---|
| `code` | Authorization Code | No | Yes — code exchanged for tokens server-side |
| `id_token` | Implicit | Yes (ID token) | No |
| `token` | Implicit | Yes (access token) | No |
| `code id_token` | Hybrid | Yes (ID token) | Yes (code exchanged for access/refresh tokens) |

In the Authorization Code flow, the browser receives only an opaque short-lived code; the actual tokens are fetched over a direct HTTPS call from the server to the token endpoint, never passing through the browser. Implicit flows were originally designed for JavaScript apps before CORS existed, but modern browsers support CORS so the Authorization Code + PKCE flow works equally well for single-page apps without the security trade-offs. ASP.NET Core's OIDC middleware defaults to `"code"` and handles the back-channel exchange automatically.

---

## Q10. What `Scope` values are typically requested in an OIDC flow, and why does requesting the `offline_access` scope matter?

**Concepts**
- `openid` scope — mandatory, signals OIDC behavior, required for ID token
- `offline_access` — requests a refresh token for long-lived API access
- Scopes are additive — each grants a set of claims or permissions
- Provider consent — some providers prompt the user to consent per scope

**Answer**

The `openid` scope is mandatory in any OIDC flow because it signals to the provider that an ID token is expected rather than just an access token; without it, the flow is plain OAuth 2.0 and no ID token is issued. Common additional scopes are `profile` (name, picture, locale), `email` (email address and verified status), and `offline_access`, which instructs the provider to issue a refresh token alongside the access token. `offline_access` is important for applications that need to call APIs on behalf of the user beyond the short lifetime of an access token — typically one hour — without requiring the user to re-authenticate; without it, the access token expires and there is no programmatic way to renew it. `offline_access` is not universally supported; some providers require the user to grant explicit consent for refresh tokens, and others enable them only for confidential clients with a client secret. Scopes are additive — each one grants access to a set of claims or API permissions, and the provider may prompt the user to consent on first login. ASP.NET Core's OIDC middleware defaults to requesting `openid` and `profile`; additional scopes are added via `options.Scope.Add("email")` or `options.Scope.Add("offline_access")`.

---

## Q11. What is the `CallbackPath` option in `AddOpenIdConnect` and how does ASP.NET Core use it to complete the login round-trip?

**Concepts**
- `CallbackPath` — relative path the OIDC middleware intercepts exclusively
- Must match redirect URI registered at the identity provider exactly
- Full URI constructed at runtime: `{scheme}://{host}{CallbackPath}`
- Multiple providers — each needs a distinct `CallbackPath`

**Answer**

`CallbackPath` is the relative path on the server to which the identity provider redirects the user after authentication; it defaults to `/signin-oidc`. The OIDC middleware intercepts requests to this path, validates the `state` and authorization code response, performs the back-channel token exchange, and completes the authentication before handing control to the rest of the pipeline. Unlike a normal MVC route, the callback path is handled exclusively by the middleware and never reaches a controller action, so no matching action method is needed. The value must be registered as an allowed redirect URI in the application's registration at the identity provider; the full URI sent during authorization is constructed at runtime as `{request.Scheme}://{request.Host}{CallbackPath}`, which is why the scheme and host must be correctly configured in production, especially behind a reverse proxy. Multiple OIDC providers in one application must each have a different `CallbackPath` — for example `/signin-google` for Google and `/signin-azure` for Azure AD — since two handlers sharing the same path means only one will ever intercept the callback correctly.

---

## Q12. What does the `SaveTokens` option do in `AddOpenIdConnect`, and how do you later retrieve the stored tokens from the HTTP context?

**Concepts**
- `SaveTokens = true` — serializes tokens into the authentication cookie's `Properties`
- `HttpContext.GetTokenAsync` — reads a named token from the cookie properties
- Cookie size growth — access tokens from some providers can approach the 4 KB browser limit
- Default is `false` — only enable when downstream API calls require the token

**Answer**

When `SaveTokens` is set to `true`, the OIDC middleware serializes the access token, ID token, refresh token (if present), and token expiry time into the authentication cookie's `Properties` dictionary after a successful login. Tokens are retrieved later by calling `HttpContext.GetTokenAsync("access_token")`, an extension method from `Microsoft.AspNetCore.Authentication`; token names that can be passed include `"access_token"`, `"id_token"`, `"refresh_token"`, `"token_type"`, and `"expires_at"`. Because the tokens are stored inside the authentication cookie, the cookie size grows significantly — an access token from Azure AD can be several kilobytes — and cookies have a browser-imposed limit of around 4 KB per domain, so chunked cookies or server-side session storage may be necessary in production. The stored access token can be used to call downstream APIs on behalf of the user by attaching it as a Bearer token to an `HttpClient` request. `SaveTokens` defaults to `false` because storing tokens in cookies has both security and size implications; it should be enabled only when server-side API calls on behalf of the signed-in user are required.

---

## Q13. What is the difference between backchannel token validation and front-channel token delivery in OIDC?

**Concepts**
- Front channel — tokens or codes travel through the browser URL
- Back channel — direct server-to-server HTTPS, tokens never touch the browser
- Authorization Code flow — only the code is front-channel; token exchange is back-channel
- JWKS validation — signature verification performed over the back channel

**Answer**

Front-channel delivery means tokens or authorization codes travel through the user's browser — either in the URL fragment for implicit flows or in the query string for hybrid flows — making them potentially visible in browser history, referrer headers, and server logs. Back-channel validation means the server communicates directly with the identity provider over a server-to-server HTTPS call, keeping tokens entirely out of the browser.

| Aspect | Front-channel | Backchannel |
|---|---|---|
| Token path | Browser URL/fragment | Direct server-to-server HTTPS |
| Exposure risk | Browser history, logs, referrer | Minimal — TLS-protected only |
| Flow | Implicit, hybrid (partial) | Authorization Code, hybrid (code exchange) |
| CSRF risk | Higher | Lower (state/nonce mitigate) |

In the Authorization Code flow, only the code travels through the browser (front-channel); the actual tokens are fetched by the server calling the token endpoint directly (back-channel), which is why this flow is far more secure than implicit alternatives. Back-channel token validation also applies to ID token signature verification: the middleware downloads the provider's public keys from the JWKS endpoint server-to-server and verifies the token's cryptographic signature locally without browser involvement. ASP.NET Core's OIDC middleware always performs the token exchange and signature validation over the back-channel; application code does not need to implement this.

---

## Q14. What is the `state` parameter in an OAuth 2.0 authorization request and how does it protect against Cross-Site Request Forgery (CSRF) attacks?

**Concepts**
- `state` — random opaque value embedded in both authorization request and callback
- CSRF prevention — proves the callback originated from a redirect this application initiated
- `CorrelationException` — thrown when `state` values do not match
- ASP.NET Core generates and validates `state` automatically

**Answer**

The `state` parameter is an opaque value the application generates before redirecting to the authorization server and includes in the authorization URL; the provider echoes it back unchanged in the callback. The application then verifies that the returned `state` matches the one it generated, proving the callback originated from a redirect it initiated. Without the `state` check, an attacker could craft a callback URL with a valid authorization code obtained from a different session and trick the server into associating that code — and the attacker's external identity — with the victim's session, a classic OAuth CSRF attack. ASP.NET Core's OAuth and OIDC middleware generates a cryptographically random `state` value automatically, stores it in a correlation cookie, and validates the echo on the callback; application developers do not need to implement this logic manually. The `state` value can also carry application-specific data such as a `returnUrl`; ASP.NET Core encodes this alongside the correlation identifier in a Base64 string. If the `state` values do not match on the callback, the middleware throws a `CorrelationException` and rejects the request before any code exchange occurs.

---

## Q15. What are correlation cookies in ASP.NET Core OAuth middleware, and how do they tie together the outgoing request and the incoming callback?

**Concepts**
- Correlation cookie — scheme-specific, short-lived, HTTP-only, contains correlation ID
- Cookie name pattern: `.AspNetCore.Correlation.{schemeName}.{correlationId}`
- Single-use — deleted immediately after the correlation check passes
- Absence on callback — indicates forgery or expired browser session, request rejected

**Answer**

A correlation cookie is a short-lived, HTTP-only cookie that the middleware writes to the browser just before redirecting to the authorization server; it contains the same unique correlation identifier that is embedded in the `state` parameter. When the callback arrives, the middleware reads the cookie, extracts its value, and compares it with the `state` returned by the provider to confirm the callback belongs to the browser's original request. The cookie is named with a prefix like `.AspNetCore.Correlation.{schemeName}.{correlationId}`, making it scheme-specific so multiple concurrent provider logins do not collide. Because the cookie is `HttpOnly` and `SameSite=None; Secure`, it cannot be read by JavaScript and is scoped to the same-site redirect flow, limiting its exposure window. After the correlation check passes, the middleware immediately deletes the correlation cookie so it cannot be reused; a second use of the same callback URL will fail with a correlation error. If the correlation cookie is absent on the callback — because the user's browser deleted cookies or because the callback is a forged request from another origin — the middleware rejects the request outright.

---

## Q16. What is the OIDC nonce, where is it stored, and how does the middleware use it to prevent token replay attacks?

**Concepts**
- Nonce — random per-request value embedded in the authorization request and in the ID token
- Stored in a short-lived HTTP-only cookie — no server-side session required
- Token replay prevention — captured ID token cannot be injected into a different session
- `state` vs nonce — CSRF protection vs ID token replay protection

**Answer**

The nonce (number used once) is a unique random value that the application includes in the authorization request; the identity provider embeds it into the ID token it issues. When the middleware receives the ID token, it checks that the nonce in the token matches the one it sent, confirming the token was issued specifically in response to this login flow and preventing an attacker who captures a valid ID token from replaying it against a different session. The nonce is stored in a separate, short-lived HTTP-only cookie before the redirect, so the middleware can retrieve it when the callback arrives without relying on server-side session state — this makes the OIDC middleware stateless. The nonce is a mandatory validation step in the OIDC specification; if the ID token does not contain a nonce that matches the stored value, the middleware rejects the token and the login fails. Unlike the `state` parameter — which protects against CSRF at the OAuth layer by binding the response to the browser session — the nonce protects at the token layer, ensuring an ID token cannot be stolen from one OIDC flow and injected into another. ASP.NET Core's OIDC middleware generates, stores, and validates the nonce automatically; application code does not need to implement nonce logic.

---

## Q17. How do you configure multiple external providers — for example Google, Microsoft, and a custom OIDC provider — in a single ASP.NET Core application?

**Concepts**
- Unique scheme name per provider — passed as first argument to `AddOpenIdConnect`
- Unique `CallbackPath` per provider — prevents handler collision
- `SignInManager.GetExternalAuthenticationSchemesAsync` — dynamic login button rendering
- Independent secrets per provider — managed separately in Secret Manager or Key Vault

**Answer**

Multiple external providers are configured by chaining additional calls to `AddGoogle`, `AddMicrosoftAccount`, and `AddOpenIdConnect` after `AddAuthentication`, giving each handler a unique scheme name, a unique `CallbackPath`, and its own credentials. Each call to `AddOpenIdConnect` accepts a scheme name as its first argument — for example `"MyCompanySSO"` — which is later used in `Challenge("MyCompanySSO")` to trigger that specific provider. Every provider must have a different `CallbackPath`; if two providers share the same path, only one handler will intercept the callback and the other will always fail, which is a common and frustrating misconfiguration. The list of registered external providers can be retrieved at runtime via `SignInManager.GetExternalAuthenticationSchemesAsync()` to dynamically render login buttons without hardcoding provider names in the view. Secrets for each provider must be managed independently — the Secret Manager tool locally and Key Vault or environment variables in production — since a single leaked secret compromises only one provider rather than all of them.

---

## Q18. How does account linking work when a user who already has a local account wants to connect an external provider, and what code is involved?

**Concepts**
- `UserManager.GetUserAsync(User)` — retrieves existing user from active session
- `AddLoginAsync` after confirming no duplicate link exists
- Preventing account takeover — verify the external key is not already linked to another user
- `RemoveLoginAsync` — reverse operation, must guard against removing all authentication methods

**Answer**

Account linking adds an `AspNetUserLogins` row for an external provider to a user account that already exists locally, usually initiated from a profile management page while the user is already signed in. The user triggers a challenge to the external provider, the callback extracts the `ExternalLoginInfo`, and then `UserManager.AddLoginAsync(existingUser, loginInfo.LoginInfo)` is called to create the association. The key difference from the registration flow is that the local `ApplicationUser` already exists; it is retrieved via `UserManager.GetUserAsync(User)` using the existing session, rather than being created new. Before calling `AddLoginAsync`, the code should verify that the external provider key is not already linked to a different local account; allowing that would let an attacker who controls an external identity link it to a victim's local account and subsequently sign in as the victim. After the link is created, future logins via that provider will resolve to this user through `SignInManager.ExternalLoginSignInAsync` without requiring an additional step. The reverse operation — removing a linked provider — uses `UserManager.RemoveLoginAsync`, and the application should ensure a user cannot remove all authentication methods at once, since that would lock them out of their own account.

---

## Q19. What is the purpose of `IdentityConstants.ExternalScheme`, why is the cookie it creates short-lived, and how does it differ from the main application cookie?

**Concepts**
- `IdentityConstants.ExternalScheme` (`"Identity.External"`) — transient bridge between callback and action
- Short lifetime — prevents indefinite availability of the external identity
- `Identity.Application` — durable session cookie issued after the application accepts the login
- Explicit sign-in required — external cookie does not satisfy `[Authorize]`

**Answer**

`IdentityConstants.ExternalScheme` is the authentication scheme name (`"Identity.External"`) under which the OIDC or OAuth middleware writes a temporary cookie containing the external provider's claims immediately after the callback succeeds. It exists solely to bridge the gap between the provider callback and the `ExternalLoginCallback` action; once that action reads the cookie via `GetExternalLoginInfoAsync`, the external identity should be discarded and replaced by an application session if the login is accepted. The cookie's short lifetime — configurable, but typically a few minutes — ensures that if the user abandons the callback flow, the external identity does not remain readable indefinitely as a potential avenue for session fixation. The main application cookie (scheme `Identity.Application`) is the durable session cookie issued by `SignInManager.SignInAsync` after the application has verified and accepted the login; it has a much longer lifetime and carries the local `ClaimsPrincipal` constructed from the application's own user store. Having two separate cookies means the external identity is never automatically elevated to an application session; the application code must explicitly decide to accept it — and at that decision point, lockout status, email confirmation requirements, and existing link checks are enforced. If the callback action returns without signing the user into the application scheme, only the external cookie remains, and requests to `[Authorize]`-protected resources will keep redirecting to the login page because only `Identity.Application` satisfies the default authentication challenge.

---

## Q20. What GDPR considerations arise when your application receives and stores personal data from an external provider, and how do you address them?

**Concepts**
- Application as independent data controller — once data is stored, its origin is irrelevant
- Right to erasure — must delete all related tables: `AspNetUsers`, `AspNetUserLogins`, `AspNetUserClaims`, `AspNetUserRoles`
- Data minimization — store only claims the application actively uses
- Avoid logging tokens or full claim sets — log retention outlasts intended data retention

**Answer**

GDPR requires that personal data — including name, email, and profile picture received from an external provider — is processed only for a stated, legitimate purpose, stored only as long as necessary, and protected appropriately. The application becomes an independent data controller the moment it stores or uses external provider data, regardless of where it came from, so the provider's own privacy policy does not cover the application's use of that data. The privacy policy must disclose that personal data is received from external providers and describe what is stored; relying solely on the provider's policy is insufficient. Data minimization applies: if only the email address is needed to create an account, the profile picture URL should not be stored unless the application actively uses it. Users have the right to erasure, which means deleting the `AspNetUsers` row and all related `AspNetUserLogins`, `AspNetUserClaims`, and `AspNetUserRoles` rows when a deletion is requested; a soft-delete pattern does not satisfy this requirement unless personal identifiers are also zeroed out. External provider tokens and full claim sets should never be written to application logs because log retention periods typically far exceed the intended data retention period for personal information, creating an inadvertent long-lived store of personal data.

---

## Q21. How should `ClientId` and `ClientSecret` be managed in ASP.NET Core applications, and what are the risks of storing them in source control?

**Concepts**
- Secret Manager (`dotnet user-secrets`) — development-time storage outside the project tree
- Environment variables with double underscores — map to `IConfiguration` hierarchy in production
- Leaked `ClientSecret` — attacker can impersonate the application at the token endpoint
- `ClientId` vs `ClientSecret` risk asymmetry — ID is public by design, secret is a credential

**Answer**

`ClientId` and `ClientSecret` must never be stored in `appsettings.json` or any file committed to source control; instead, the .NET Secret Manager tool is used during development and environment variables, Azure Key Vault, or a secrets management service such as HashiCorp Vault are used in production. Exposing a `ClientSecret` in source control means anyone with repository access — including all historical commits even after the file is updated — can impersonate the application against the identity provider. The Secret Manager (`dotnet user-secrets set "Authentication:Google:ClientSecret" "..."`) stores values in a user-profile directory outside the project tree, so they are never accidentally committed. In production, `IConfiguration` reads from environment variables automatically; naming them with double underscores for hierarchy separation — `Authentication__Google__ClientSecret` — maps to the same configuration key as `Authentication:Google:ClientSecret` in JSON. A leaked `ClientSecret` allows an attacker to register fraudulent redirect URIs with the identity provider and intercept authorization codes for the application's users; the secret must be rotated immediately at the identity provider upon suspected exposure. `ClientId` is generally lower risk because it is public by design in the OAuth flow, but `ClientSecret` is the credential that proves the server's identity to the token endpoint and must be treated with the same care as a database password.

---

## Q22. What is the difference between OAuth 2.0 and OpenID Connect (OIDC), and when would you use each?

**Concepts**
- OAuth 2.0 — authorization framework, answers "can this client do X?"
- OIDC — identity layer, answers "who is this user?"
- ID token — OIDC's authentication assertion, always a JWT
- `AddOpenIdConnect` vs `AddGoogle` — full OIDC vs OAuth + proprietary user-info

**Answer**

OAuth 2.0 is an authorization framework that allows an application to obtain limited access to a resource on behalf of a user, but it says nothing about who the user is. OpenID Connect is an identity layer built on top of OAuth 2.0 that adds a standardized way to authenticate the user and communicate their identity via a signed ID token in JWT format.

| Aspect | OAuth 2.0 | OIDC |
|---|---|---|
| Purpose | Authorization (access to resources) | Authentication (who the user is) |
| Token issued | Access token (opaque or JWT) | ID token (always JWT) + access token |
| Standard user info | None | `sub`, `email`, `name`, `picture` claims |
| Discovery | Not standardized | `/.well-known/openid-configuration` |

OAuth 2.0 alone is the right choice when the application needs to call an API on behalf of the user — such as the Google Calendar API — but does not need to know who the user is within its own system. OIDC is the right choice when authenticating the user and establishing a local session based on their external identity, which is the scenario `AddOpenIdConnect` is designed for. In practice, `AddGoogle` and `AddMicrosoftAccount` use OAuth 2.0 plus provider-specific user-info endpoints rather than the full OIDC standard, while `AddOpenIdConnect` targets providers that implement OIDC and issue proper ID tokens.

---

## Q23. What is Proof Key for Code Exchange (PKCE), when is it required in an OIDC flow, and how does ASP.NET Core support it?

**Concepts**
- Code verifier and code challenge — SHA-256 hash, `S256` method
- Mandatory for public clients — no secure client secret storage on device
- `UsePkce = true` — enabled by default in ASP.NET Core 5+
- Defense-in-depth for confidential clients — PKCE + client secret

**Answer**

PKCE is an extension to the Authorization Code flow that prevents authorization code interception attacks by requiring the client to prove it generated the original authorization request. Before the redirect, the client creates a random `code_verifier`, derives a `code_challenge` from it by computing `BASE64URL(SHA-256(code_verifier))`, sends the challenge with the authorization request, and then sends the original `code_verifier` during the token exchange; the server hashes the verifier and compares it against the stored challenge to verify they match. PKCE was originally designed for public clients — mobile and single-page applications — that cannot keep a `ClientSecret`, since an intercepted authorization code without a `code_verifier` cannot be exchanged for tokens and a mobile app that embeds a client secret is effectively a public secret. PKCE is mandatory for native and single-page applications, and OAuth 2.1 recommends it for all clients including confidential ones. ASP.NET Core 5 and later enable PKCE for `AddOpenIdConnect` by default via `UsePkce = true`; it can be disabled for providers that do not support it by setting `options.UsePkce = false`. Even for server-side apps with a `ClientSecret`, using PKCE adds defense-in-depth since an intercepted code is useless without the verifier, which never leaves the server.

---

## Q24. How do you handle the `RemoteFailure` event that can occur during an external login attempt?

**Concepts**
- `OnRemoteFailure` event — fires when the provider returns an error in the callback
- `context.HandleResponse()` — suppresses the default exception behavior
- `error=access_denied` — user cancelled on the provider's consent screen
- Redirect to friendly error page rather than surfacing raw exception

**Answer**

The `RemoteFailure` event fires when the external provider returns an error in the callback — the user denies consent, the `state` is invalid, or the token exchange fails — and by default the middleware throws an exception that produces a 500 error page. The event is handled by subscribing to `options.Events.OnRemoteFailure` and redirecting the user to a friendly error or login page instead. Inside the event handler, `context.Failure` contains the underlying exception and should be logged for diagnostics; calling `context.HandleResponse()` suppresses the default exception behavior and hands control to the handler's own redirect. Common failure reasons include the user clicking "Cancel" on the provider's consent screen (which sends `error=access_denied`), an expired or tampered `state` parameter, and network failures during the back-channel token exchange. The difference between user cancellation and other errors can be detected by checking `context.Failure.Message` or by inspecting the callback query string for `error=access_denied`; user cancellation is not an error worth alarming over, while other failures may warrant more prominent logging.

```csharp
options.Events.OnRemoteFailure = ctx => {
    ctx.HandleResponse();
    ctx.Response.Redirect("/Account/Login?error=external");
    return Task.CompletedTask;
};
```

---

## Q25. How do you apply claims transformation after receiving the external provider's identity so that your application's internal claims are enriched or normalized?

**Concepts**
- `IClaimsTransformation` — per-request enrichment after authentication
- One-time enrichment via `UserManager.AddClaimAsync` — persisted in `AspNetUserClaims`
- `options.ClaimActions.MapJsonKey` — lighter-weight per-provider claim mapping
- Caching inside `TransformAsync` — required to avoid per-request database calls

**Answer**

Claims transformation allows adding, removing, or modifying claims on the `ClaimsPrincipal` after authentication completes but before the principal is used by authorization policies. Implementing the `IClaimsTransformation` interface and registering it in DI causes the framework to call `TransformAsync` on every request where the authentication cookie is resolved; a common use case is mapping the provider's `sub` or `email` claim to an internal user ID claim, or adding role claims fetched from a database. Because `IClaimsTransformation` runs on every request, not just at login time, it must be efficient; database calls should be avoided unless the result is cached, since the transformer runs on every authenticated page load. One-time enrichment is often a better fit: adding claims to `AspNetUserClaims` via `UserManager.AddClaimAsync` at registration time persists them in the database and includes them automatically in future sessions without requiring per-request work. For lighter-weight scenarios where only provider-specific JSON fields need to be renamed or included, `options.ClaimActions.MapJsonKey` in `AddGoogle` or `AddOpenIdConnect` maps provider-specific JSON fields to named claims before the principal is created, which is preferable to a full claims transformation for simple mapping tasks.

---

## Q26. How does signing out of an external provider (federated sign-out) work in an OIDC integration, and what ASP.NET Core mechanisms support it?

**Concepts**
- Federated sign-out — outbound (application signs out at provider) and inbound (provider notifies application)
- `end_session_endpoint` + `id_token_hint` — outbound logout redirect
- Front-channel vs back-channel logout — iframe vs POST notification from provider
- Missing federated sign-out — SSO silently re-authenticates the user

**Answer**

Federated sign-out means that when a user signs out of the application, they are also signed out of the identity provider, and vice versa. In ASP.NET Core, outbound federated sign-out is triggered by calling `SignOutAsync` with both the cookie scheme and the OIDC scheme; the OIDC middleware redirects to the provider's `end_session_endpoint` with a `post_logout_redirect_uri` and an `id_token_hint` that helps the provider identify the session to terminate without requiring the user to re-enter credentials. Inbound federated sign-out — the provider notifying the application when the user signs out elsewhere — uses either the OIDC Back-Channel Logout specification (a POST to a registered endpoint on the application) or the Front-Channel Logout specification (loading an iframe); ASP.NET Core's OIDC middleware handles front-channel logout via `SignedOutCallbackPath`. If federated sign-out is not implemented, a user who clicks "Sign Out" on the application still has an active session at the identity provider, and clicking "Login" again immediately signs them back in silently via SSO, which is confusing and may be a security concern in shared-computer scenarios. To sign out of both the application cookie and the OIDC provider, call `HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)` and `HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme)` together; omitting either one produces an incomplete sign-out.

---

## Q27. What risks arise if an application blindly trusts the email claim from an external provider without additional verification, and how do you mitigate them?

**Concepts**
- `email_verified` claim — must be `true` before email is used as a trusted identifier
- `ProviderKey` (`sub` claim) — stable, authoritative identifier, preferred for account linking
- Email-based account takeover — attacker controls a provider with unverified email claims
- Confirmation step required before `AddLoginAsync` when linking by email

**Answer**

If an application uses the email claim from an external provider as the sole identifier for linking to an existing local account, an attacker who controls a provider that issues unverified email claims can impersonate any user whose email address they know. Providers vary in how rigorously they verify email ownership before including it in tokens, and some allow users to set arbitrary email addresses without verification. Google includes an `email_verified` claim alongside `email`; this must be checked and confirmed `true` before using the email to link accounts, because an unverified email from Google means the user claimed the address but Google has not confirmed ownership. The safer primary lookup key is the `ProviderKey` (the `sub` claim), which is a stable, provider-assigned identifier that never belongs to another user on the same provider, so using it for account linking is safe regardless of whether the email is verified. When linking an external login to an existing local account found by email match, requiring the user to also enter their current local password or a verification code before calling `AddLoginAsync` proves they control that local account independently of the external provider's claims. The risk is highest for providers that do not enforce email verification and for applications that auto-link based on email without a confirmation step.

---

## Q28. How do you handle token expiry and refresh when `SaveTokens` is enabled and the access token stored in the cookie expires?

**Concepts**
- `expires_at` in cookie properties — checked before using the access token
- Manual refresh via `grant_type=refresh_token` POST — update cookie with `UpdateTokenValue`
- `IdentityModel.AspNetCore` — automates the refresh pattern
- No refresh token available — redirect to re-authenticate if `offline_access` not requested

**Answer**

When `SaveTokens` is enabled, the access token is stored in the authentication cookie with its expiry time as the `expires_at` property. The cookie itself does not expire when the access token expires, so the application must check token validity before using it and initiate a refresh if a refresh token is available. Expiry is checked by calling `HttpContext.GetTokenAsync("expires_at")`, parsing the value as a `DateTimeOffset`, and comparing it to `DateTimeOffset.UtcNow` with a small buffer — typically 5 minutes — to avoid using a token that will expire mid-request. If the access token has expired and a refresh token is available (retrieved via `GetTokenAsync("refresh_token")`), a direct HTTP POST is made to the provider's token endpoint with `grant_type=refresh_token` to obtain a new access token; the cookie must then be updated by re-reading the current `AuthenticateResult`, calling `Properties.UpdateTokenValue(...)` for the new access token and expiry, and re-issuing the cookie via `HttpContext.SignInAsync`. Failing to update the cookie leaves the stale token in place for all subsequent requests, which means every call to a downstream API will fail with a 401. Libraries such as `IdentityModel.AspNetCore` automate this refresh pattern by exposing a `GetAccessTokenAsync` method that transparently refreshes when needed, so developers do not need to write the renewal logic manually. If no refresh token is available because `offline_access` was not requested or the provider did not grant one, the only option when the access token expires is to redirect the user to re-authenticate.

---

## Gotchas — External Logins & OIDC (Interview Traps)

---

#### Gotcha 1. `ClientSecret` stored in `appsettings.json` committed to Git

**Concepts**
- Git history is permanent — removing the file in a later commit does not remove the secret
- Immediate rotation required — assume the secret is compromised once it appears in history
- Secret Manager and Key Vault — the correct development and production alternatives

**Answer**

A `ClientSecret` in a committed configuration file is immediately accessible to anyone with repository access, including all historical commits even after the file is updated. Once a secret appears in Git history it must be treated as permanently compromised; removing the file from the latest commit does not help because the secret remains in every earlier commit and any fork, clone, or CI cache created since then. The correct model is to treat `ClientSecret` identically to a database password: use the Secret Manager (`dotnet user-secrets`) locally and Key Vault or environment variables in production. The leaked secret must be rotated at the identity provider immediately upon discovery; the window of unauthorized use begins the moment the commit is pushed, not the moment the exposure is discovered.

---

#### Gotcha 2. Confusing the external cookie scheme with the application cookie scheme

**Concepts**
- `IdentityConstants.ExternalScheme` — written by middleware, not an application session
- `[Authorize]` satisfied only by `Identity.Application` — external cookie does not grant access
- Explicit `SignInAsync` required — application code is the gating point

**Answer**

Many developers assume that because the OIDC callback succeeded and a cookie was written, the user is now signed into the application; but the cookie written at that point uses `IdentityConstants.ExternalScheme`, not `Identity.Application`, and does not grant access to `[Authorize]`-protected resources. The user is not signed in until the `ExternalLoginCallback` action explicitly calls `SignInManager.SignInAsync` or `ExternalLoginSignInAsync`. Skipping the callback action — or returning from it without signing in — leaves the user with only the short-lived external cookie, which expires in minutes and does not satisfy authorization requirements; protected pages will keep redirecting to the login page with no obvious explanation. The separation is intentional: it gives application code the opportunity to run checks such as lockout detection, email confirmation enforcement, and new-user registration before a durable session is established.

---

#### Gotcha 3. Using the email claim as the account-linking key without checking `email_verified`

**Concepts**
- `email_verified = false` — provider has not confirmed the user owns the address
- `sub` claim as primary link key — always authoritative, immune to email verification gap
- Confirmation step for high-value linking — user proves control of local account independently

**Answer**

Linking an external login to a local account based solely on the `email` claim assumes the provider has verified that the user owns that email address, which is not always true. Providers such as Google include `email_verified: true` when the address is confirmed; a value of `false` or the claim's absence means the user claimed the address but the provider has not confirmed ownership — trusting it for account linking at that point could allow an attacker to register a matching email at a permissive provider and link to the victim's account. The safe primary link key is the `ProviderKey` (the `sub` claim), which is always authoritative within the provider's namespace and is immune to the email verification gap entirely. When linking by email to an existing local account found by email match, requiring the user to also enter their current local password or a one-time verification code before calling `AddLoginAsync` proves they control that local account independently of the external provider's claims.

---

#### Gotcha 4. Forgetting to match `CallbackPath` with the redirect URI registered at the provider

**Concepts**
- `redirect_uri_mismatch` error — provider rejects the authorization request before user sees anything
- Full URI includes scheme, host, port, and path — all must match exactly
- Reverse proxy — scheme and host must be correctly forwarded or explicitly configured

**Answer**

If the `CallbackPath` in `AddOpenIdConnect` options does not exactly match the redirect URI registered in the identity provider's application portal, the provider will reject the authorization request with a `redirect_uri_mismatch` error before the user can authenticate. The middleware constructs the full redirect URI as `{scheme}://{host}{CallbackPath}`, so the registered URI must include the correct scheme, host, port (if non-standard), and path — a trailing slash difference is enough to cause a mismatch. A common mistake is registering `https://myapp.com/signin-oidc` at the provider but running locally on `http://localhost:5000/signin-oidc`; these are treated as different URIs and both must be registered if used in different environments. Behind a reverse proxy, the scheme and host seen by the middleware may differ from those seen by the provider unless `ForwardedHeaders` middleware is configured or the `CallbackPath` is set alongside explicit scheme/host overrides — this is a common source of mysterious `redirect_uri_mismatch` errors in production.

---

#### Gotcha 5. Assuming `SaveTokens = true` is always safe for production

**Concepts**
- Cookie size limit — 4 KB per domain, Azure AD tokens approach this limit
- Truncated cookie — `AuthenticateResult.Failure` on every request, user locked out
- Server-side token store — preferred pattern for production token persistence

**Answer**

`SaveTokens = true` serializes access tokens, refresh tokens, and ID tokens into the authentication cookie, which is sent with every request; because cookies have a 4 KB browser-imposed size limit and access tokens from providers like Azure AD can be several kilobytes, this can cause cookie overflow errors or silently truncated cookies that fail to deserialize. A truncated authentication cookie causes `AuthenticateResult.Failure` on every subsequent request, effectively locking the user out of the application with no obvious error message — the user appears to be authenticated (they have a cookie) but every protected page fails. The correct production model for applications that need tokens is to store them server-side, in a distributed cache or database keyed by a session identifier, and pass only the session key in the cookie. If `SaveTokens` must be used, enabling cookie chunking via `ChunkingCookieManager` splits large cookies across multiple cookie headers, though this is a workaround rather than a proper solution and still has a practical upper limit based on the number of headers the browser will accept.

---

#### Gotcha 6. Correlation Cookie Expires Quickly — Long Auth Flows Fail With a Correlation Error

**Concepts**
- Correlation cookie storing the state/nonce pair, expiring in ~15 minutes by default
- User leaving the browser idle before completing provider consent — correlation error on return
- `CorrelationExpiry` and `RemoteAuthenticationOptions.CorrelationCookie` tuning

**Answer**

When `AddOpenIdConnect` redirects the user to the external provider, it stores a short-lived correlation cookie that holds the state and nonce for the pending request. If the user takes too long to complete consent at the provider — for instance, they are prompted to create an account, verify their email, or simply leave the browser idle — the correlation cookie expires before the provider redirects back, and the middleware returns "Correlation failed" with no token. This is not a provider error; it is a client-side timeout. The expiry is controlled by `RemoteAuthenticationOptions.CorrelationCookie.Expiration` (default ~15 minutes). For flows involving multi-step provider onboarding, extending this window reduces the failure rate; but the window should not be made too long, as a valid state/nonce pair could be replayed within the window.

---

#### Gotcha 7. `GetExternalLoginInfoAsync` Returns `null` When Called Outside the Callback Path

**Concepts**
- `GetExternalLoginInfoAsync` reading the temporary `Identity.External` cookie
- External cookie only present immediately after the provider redirects back to `CallbackPath`
- Calling after a redirect away from the callback URL loses the cookie

**Answer**

`SignInManager.GetExternalLoginInfoAsync()` reads the temporary `Identity.External` cookie that is written immediately after the provider's redirect and deleted once read. This method must be called within the same HTTP request (or an immediate post-redirect-get) that handles the external provider callback at `CallbackPath`. If the application redirects to another page before calling `GetExternalLoginInfoAsync` — for example, to display a registration form — the cookie is lost and the method returns `null`. The pattern for new-user registration flows is to store the `ExternalLoginInfo` in `TempData` or to pass the provider, key, and any required claims as form fields into the registration page, because the external cookie will not survive an additional redirect.

---

#### Gotcha 8. Multiple OIDC Providers Sharing the Same `SignedOutCallbackPath` Causes Sign-Out Conflicts

**Concepts**
- `SignedOutCallbackPath` must be unique per OIDC scheme
- Last registered handler claiming the callback and silently dropping other providers' post-logout redirects
- `options.SignedOutCallbackPath = "/signout-callback-<provider>"` as the correct pattern

**Answer**

When multiple `AddOpenIdConnect` registrations are present in the same application, each handler's `SignedOutCallbackPath` defaults to `/signout-callback-oidc`. The first handler that matches the path claims the callback; other providers' post-logout redirects are processed by the wrong handler and silently fail — the user is redirected to an unexpected page, or the sign-out sequence loops. The fix is to set a unique `SignedOutCallbackPath` for each OIDC provider: for example, `/signout-callback-aad` for Azure AD and `/signout-callback-github` for GitHub. The same uniqueness rule applies to `CallbackPath` and `RemoteSignOutPath` when multiple providers are registered.

---

#### Gotcha 9. `ClaimActions.MapUniqueJsonKey` Silently Drops Duplicate Claim Types

**Concepts**
- `MapUniqueJsonKey` mapping a JSON property to a claim type only if the claim type is not already present
- First claim wins — subsequent providers or mappers for the same type have no effect
- `MapJsonKey` (non-unique) adding duplicate claims — both overloads exist for different use cases

**Answer**

`ClaimActions.MapUniqueJsonKey` is the default mapping method in most OIDC provider configurations — it adds the claim only if the claim type is not already present in the identity. This means that if the identity already carries a `name` claim from a prior mapping step (for example, from the standard OIDC claims), a custom `MapUniqueJsonKey("name", "display_name")` will silently have no effect, leaving the original value intact. Developers who expect their custom mapping to override an existing claim value must use `MapJsonKey` (which always adds, potentially duplicating the claim) combined with a prior `DeleteClaim` call to remove the old value, or they must restructure the mapping order so the desired source is mapped first.

---

#### Gotcha 10. Nonce Validation Fails in Browsers That Block Third-Party Cookies (ITP / Safari)

**Concepts**
- Nonce stored in a same-site cookie — blocked by ITP in cross-site redirect scenarios
- OIDC library throwing "nonce could not be validated" with no obvious browser link
- `OpenIdConnectOptions.ProtocolValidator.RequireNonce = false` as a workaround with trade-offs

**Answer**

The ASP.NET Core OIDC middleware stores the nonce value in a short-lived correlation cookie before redirecting to the authorization server. When the provider redirects back, the middleware reads the cookie to validate the nonce embedded in the ID Token. In browsers with Intelligent Tracking Prevention (Safari ITP, Firefox Enhanced Tracking Protection) or when the callback URL is considered cross-site, the correlation cookie may be blocked, causing nonce validation to fail with a cryptic error. This is most common in iFrame-embedded flows, pop-up flows, and scenarios where the provider redirect crosses a subdomain boundary. Mitigations include using the same registered domain for the application and the callback, switching to a server-side PKCE state store instead of cookies, or (accepting reduced replay protection) setting `ProtocolValidator.RequireNonce = false` when the browser environment cannot reliably persist first-party cookies through a redirect.

---
