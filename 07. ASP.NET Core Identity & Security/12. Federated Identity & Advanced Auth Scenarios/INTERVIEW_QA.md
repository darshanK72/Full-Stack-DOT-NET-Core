# Federated Identity & Advanced Auth Scenarios — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is federated identity, and why is it useful in enterprise environments?](#q1-what-is-federated-identity-and-why-is-it-useful-in-enterprise-environments)
2. [Q2. What is the difference between an Identity Provider (IdP) and a Service Provider (SP) in a federated identity system?](#q2-what-is-the-difference-between-an-identity-provider-idp-and-a-service-provider-sp-in-a-federated-identity-system)
3. [Q3. How does SAML 2.0 differ from OpenID Connect (OIDC) / OAuth 2.0 at a high level, and when would you choose one over the other?](#q3-how-does-saml-20-differ-from-openid-connect-oidc-oauth-20-at-a-high-level-and-when-would-you-choose-one-over-the-other)
4. [Q4. What role does Azure Active Directory (now Microsoft Entra ID) play as an IdP in an ASP.NET Core application?](#q4-what-role-does-azure-active-directory-now-microsoft-entra-id-play-as-an-idp-in-an-aspnet-core-application)
5. [Q5. How does the `Microsoft.Identity.Web` library simplify integrating Azure AD authentication in ASP.NET Core?](#q5-how-does-the-microsoftidentityweb-library-simplify-integrating-azure-ad-authentication-in-aspnet-core)
6. [Q6. What is a multi-tenant application in Azure AD, and how does token validation differ from a single-tenant application?](#q6-what-is-a-multi-tenant-application-in-azure-ad-and-how-does-token-validation-differ-from-a-single-tenant-application)
7. [Q7. How do you validate tokens from multiple tenants safely in ASP.NET Core without hardcoding a single tenant issuer?](#q7-how-do-you-validate-tokens-from-multiple-tenants-safely-in-aspnet-core-without-hardcoding-a-single-tenant-issuer)
8. [Q8. What is the On-Behalf-Of (OBO) flow, and when would you use it in a microservices architecture?](#q8-what-is-the-on-behalf-of-obo-flow-and-when-would-you-use-it-in-a-microservices-architecture)
9. [Q9. How does the Client Credentials flow work, and when is it appropriate for machine-to-machine (M2M) API access?](#q9-how-does-the-client-credentials-flow-work-and-when-is-it-appropriate-for-machine-to-machine-m2m-api-access)
10. [Q10. What is token chaining in a microservices architecture, and what risks does it introduce?](#q10-what-is-token-chaining-in-a-microservices-architecture-and-what-risks-does-it-introduce)
11. [Q11. What is the Backend for Frontend (BFF) pattern, and why is it used to secure Single-Page Applications (SPAs)?](#q11-what-is-the-backend-for-frontend-bff-pattern-and-why-is-it-used-to-secure-single-page-applications-spas)
12. [Q12. What is Duende IdentityServer, and when would you self-host an authorization server instead of using a cloud IdP?](#q12-what-is-duende-identityserver-and-when-would-you-self-host-an-authorization-server-instead-of-using-a-cloud-idp)
13. [Q13. What is OpenIddict, and how does it compare to Duende IdentityServer as a self-hosted authorization server option?](#q13-what-is-openiddict-and-how-does-it-compare-to-duende-identityserver-as-a-self-hosted-authorization-server-option)
14. [Q14. What is dynamic client registration (DCR) in OAuth 2.0, and when is it useful?](#q14-what-is-dynamic-client-registration-dcr-in-oauth-20-and-when-is-it-useful)
15. [Q15. How do scopes work in a microservices architecture with per-service audiences?](#q15-how-do-scopes-work-in-a-microservices-architecture-with-per-service-audiences)
16. [Q16. What is the difference between the `aud` (audience) claim and the `scope` claim in a JWT access token?](#q16-what-is-the-difference-between-the-aud-audience-claim-and-the-scope-claim-in-a-jwt-access-token)
17. [Q17. What is silent token refresh in a SPA, and how does it work with OIDC?](#q17-what-is-silent-token-refresh-in-a-spa-and-how-does-it-work-with-oidc)
18. [Q18. What is OIDC Session Management, and how does it differ from silent token refresh?](#q18-what-is-oidc-session-management-and-how-does-it-differ-from-silent-token-refresh)
19. [Q19. What is the OAuth 2.0 token revocation endpoint, and how does it work?](#q19-what-is-the-oauth-20-token-revocation-endpoint-and-how-does-it-work)
20. [Q20. What is the token introspection endpoint, and when should you use it instead of local JWT validation?](#q20-what-is-the-token-introspection-endpoint-and-when-should-you-use-it-instead-of-local-jwt-validation)
21. [Q21. What are the security considerations for using the implicit grant flow versus the Authorization Code + PKCE flow in SPAs?](#q21-what-are-the-security-considerations-for-using-the-implicit-grant-flow-versus-the-authorization-code-pkce-flow-in-spas)
22. [Q22. What is the difference between delegated permissions and application permissions in Azure AD?](#q22-what-is-the-difference-between-delegated-permissions-and-application-permissions-in-azure-ad)
23. [Q23. How do you achieve tenant isolation in a multi-tenant application to prevent cross-tenant data leakage?](#q23-how-do-you-achieve-tenant-isolation-in-a-multi-tenant-application-to-prevent-cross-tenant-data-leakage)
24. [Q24. What is PKCE (Proof Key for Code Exchange), and why was it introduced in OAuth 2.0?](#q24-what-is-pkce-proof-key-for-code-exchange-and-why-was-it-introduced-in-oauth-20)
25. [Q25. What is a federated credential, and how is it used in Azure Workload Identity or Managed Identity scenarios?](#q25-what-is-a-federated-credential-and-how-is-it-used-in-azure-workload-identity-or-managed-identity-scenarios)
26. [Q26. What are the common security risks in token forwarding (token chaining) across microservices?](#q26-what-are-the-common-security-risks-in-token-forwarding-token-chaining-across-microservices)
27. [Q27. What is claims transformation in ASP.NET Core, and when is it useful in federated identity scenarios?](#q27-what-is-claims-transformation-in-aspnet-core-and-when-is-it-useful-in-federated-identity-scenarios)
28. [Q28. How does the BFF pattern compare to storing tokens in browser-accessible storage for securing SPAs?](#q28-how-does-the-bff-pattern-compare-to-storing-tokens-in-browser-accessible-storage-for-securing-spas)

---

## Q1. What is federated identity, and why is it useful in enterprise environments?

What is federated identity, and why is it useful in enterprise environments?

**Answer:** Federated identity is a system that allows a user's identity, authenticated by a trusted Identity Provider (IdP), to be accepted by one or more separate Service Providers (SPs) without requiring the user to authenticate again at each service. It enables single sign-on (SSO) across organizational and application boundaries by establishing a trust relationship between domains, so users carry their identity rather than re-proving it at every destination.

- Federated identity is particularly valuable in enterprises where users access systems from multiple vendors — Salesforce, GitHub, an internal HR portal — all using the same corporate credentials, eliminating separate logins for each system.
- The trust relationship between an IdP and SP is established through metadata exchange (in SAML 2.0) or discovery documents (in OpenID Connect / OIDC), which contain public keys and endpoint URLs so each party can verify the other's assertions.
- It eliminates the need for users to maintain separate credentials per application, reducing password fatigue and the risk of credential stuffing attacks across siloed systems.
- From an operations perspective, centralized identity management means that revoking a user's access at the IdP propagates immediately to all federated SPs, without requiring administrators to touch each system individually.

---

## Q2. What is the difference between an Identity Provider (IdP) and a Service Provider (SP) in a federated identity system?

What is the difference between an Identity Provider (IdP) and a Service Provider (SP) in a federated identity system?

**Answer:** An Identity Provider (IdP) is the system that authenticates the user and issues signed security assertions or tokens attesting to the user's identity and attributes. A Service Provider (SP) is the application that receives and trusts the IdP's assertion to grant the user access to its resources, without performing its own credential verification.

- The IdP holds the user's credentials and is solely responsible for the authentication ceremony — password check, multi-factor authentication (MFA) prompt, biometric verification, etc.
- The SP trusts the IdP's assertion by validating its digital signature against the IdP's known public key; it never sees the user's raw credentials and does not authenticate the user independently.
- In a typical ASP.NET Core deployment, Azure Active Directory (Azure AD) acts as the IdP, and the ASP.NET Core API registered in Azure AD acts as the SP (also called the resource server in OAuth 2.0 terminology).
- The critical risk in this separation is misconfigured trust: an SP must only accept assertions signed by a specific, known IdP, otherwise an attacker could present forged assertions from a rogue IdP and gain unauthorized access.

---

## Q3. How does SAML 2.0 differ from OpenID Connect (OIDC) / OAuth 2.0 at a high level, and when would you choose one over the other?

How does SAML 2.0 differ from OpenID Connect (OIDC) / OAuth 2.0 at a high level, and when would you choose one over the other?

**Answer:** Security Assertion Markup Language (SAML) 2.0 is an XML-based federation protocol designed for enterprise single sign-on between web applications, while OpenID Connect (OIDC) is a modern identity layer built on top of OAuth 2.0 that uses JSON Web Tokens (JWTs) and is better suited for REST APIs, SPAs, and mobile applications. Both protocols achieve federated SSO, but they differ substantially in token format, transport mechanism, and ecosystem fit.

| Dimension | SAML 2.0 | OIDC / OAuth 2.0 |
|---|---|---|
| Token format | XML assertion | JSON Web Token (JWT) |
| Transport | Browser redirects (POST / Redirect bindings) | HTTP redirects + JSON API calls |
| Primary use case | Enterprise SSO between web apps | API authorization, mobile, SPA |
| Discovery | XML metadata exchange | `.well-known/openid-configuration` endpoint |
| Mobile / API friendly | No — browser-centric design | Yes |
| Native OAuth delegation | No | Built-in |

- Choose SAML 2.0 when integrating with legacy enterprise systems such as on-premises Active Directory Federation Services (ADFS), Okta enterprise SSO, or any system that only exposes a SAML endpoint.
- Choose OIDC / OAuth 2.0 for modern applications: REST APIs, SPAs, mobile apps, and microservices, where JSON tokens and HTTP API endpoints are the natural fit.
- Many cloud IdPs, including Azure AD and Okta, support both protocols simultaneously; the choice often depends on the consuming application's capabilities rather than the IdP's limitations.

---

## Q4. What role does Azure Active Directory (now Microsoft Entra ID) play as an IdP in an ASP.NET Core application?

What role does Azure Active Directory (now Microsoft Entra ID) play as an IdP in an ASP.NET Core application?

**Answer:** Azure Active Directory, now rebranded as Microsoft Entra ID, acts as a cloud-based Identity Provider that issues OIDC ID tokens and OAuth 2.0 access tokens for applications registered within an Entra ID tenant. An ASP.NET Core application registers as an app registration in Entra ID and validates incoming tokens issued by Entra ID's token endpoint using the tenant's published public signing keys.

- Each tenant has its own issuer URL in the format `https://login.microsoftonline.com/{tenantId}/v2.0`, and tokens contain tenant-specific claims such as `tid` (tenant ID) and `oid` (object ID unique within the tenant).
- The ASP.NET Core JWT Bearer middleware retrieves the public signing keys automatically from the OIDC discovery document at `https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration`, validating both the signature and the `aud` (audience) claim on every request.
- Entra ID supports both delegated flows (Authorization Code with PKCE, On-Behalf-Of) and application flows (Client Credentials), making it the IdP for both interactive user-facing applications and headless service-to-service scenarios.
- App roles and security groups defined in Entra ID appear as claims in the access token, enabling role-based authorization decisions directly from the IdP without a separate permission lookup.

---

## Q5. How does the `Microsoft.Identity.Web` library simplify integrating Azure AD authentication in ASP.NET Core?

How does the `Microsoft.Identity.Web` library simplify integrating Azure AD authentication in ASP.NET Core?

**Answer:** `Microsoft.Identity.Web` is a NuGet library that wraps the lower-level Microsoft Authentication Library (MSAL) and ASP.NET Core's JWT Bearer middleware, providing a streamlined integration with Azure AD / Entra ID that handles token validation, token caching, and the On-Behalf-Of (OBO) flow with minimal boilerplate configuration.

- Calling `builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)` reads the `AzureAd` section from `appsettings.json` and configures JWT Bearer validation with the correct issuer, audience, and signing key retrieval automatically — no manual `TokenValidationParameters` wiring required.
- For web apps that call downstream APIs, combining `AddMicrosoftIdentityWebApp` with `.EnableTokenAcquisitionToCallDownstreamApi()` wires up a token cache and makes `ITokenAcquisition` injectable in controllers, allowing `GetAccessTokenForUserAsync(scopes)` to handle the OBO exchange transparently.
- The library also handles multi-tenant scenarios correctly by providing a built-in multi-tenant issuer validator when `"TenantId": "common"` is configured, which is a common source of misconfiguration when done manually with the raw middleware.

```csharp
// Program.cs — minimal Web API with Entra ID
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");
// appsettings.json: "AzureAd": { "Instance": "...", "TenantId": "common", "ClientId": "..." }
```

---

## Q6. What is a multi-tenant application in Azure AD, and how does token validation differ from a single-tenant application?

What is a multi-tenant application in Azure AD, and how does token validation differ from a single-tenant application?

**Answer:** A multi-tenant Azure AD application is one registered to accept sign-ins from users across any Azure AD tenant (or a defined set of tenants), not only the tenant where the application is registered. Because each tenant has its own issuer URL, the `iss` (issuer) claim in the JWT varies per user, causing standard single-issuer validation to reject tokens from external tenants.

- In a single-tenant app, the `iss` claim is always `https://login.microsoftonline.com/{myTenantId}/v2.0`, and the JWT Bearer middleware can validate against this fixed string.
- In a multi-tenant app, the `iss` claim reflects the authenticating user's home tenant, so configuring a single fixed issuer causes valid tokens from other tenants to fail validation with a `SecurityTokenInvalidIssuerException`.
- The safe approach is a custom `IssuerValidator` that derives the expected issuer from the token's `tid` (tenant ID) claim and checks it against a list of approved tenants, rather than disabling issuer validation outright.
- The `tid` claim is the primary handle for multi-tenancy at the application layer — it identifies which tenant the user belongs to and must be propagated to all data access and logging operations for isolation and audit purposes.

---

## Q7. How do you validate tokens from multiple tenants safely in ASP.NET Core without hardcoding a single tenant issuer?

How do you validate tokens from multiple tenants safely in ASP.NET Core without hardcoding a single tenant issuer?

**Answer:** Safe multi-tenant token validation requires replacing the default single-issuer check with a custom `IssuerValidator` that reconstructs the expected issuer URL from the token's `tid` claim, compares it against the actual issuer in the token, and optionally checks the tenant against an allowlist of permitted tenants. Setting `ValidateIssuer = false` alone is insufficient and dangerous.

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = false, // disable static string check
    IssuerValidator = (issuer, token, parameters) =>
    {
        var jwt = token as JwtSecurityToken;
        var tid = jwt?.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;
        var expected = $"https://login.microsoftonline.com/{tid}/v2.0";
        if (issuer != expected || !IsAllowedTenant(tid))
            throw new SecurityTokenInvalidIssuerException("Untrusted issuer");
        return issuer;
    }
};
```

- Setting `ValidateIssuer = false` without a replacement validator means any token from any Azure AD tenant will pass issuer validation, which is an open-door vulnerability for apps intended to serve only specific tenants.
- The OIDC signing keys are tenant-specific and fetched from the per-tenant JWKS endpoint; the middleware retrieves them using the token's `iss` value, so cryptographic signature validation remains intact even when issuer string validation is customized.
- `Microsoft.Identity.Web` handles this automatically when `"TenantId": "common"` is set in configuration, using its built-in multi-tenant issuer validator that performs the same pattern-matching logic.

---

## Q8. What is the On-Behalf-Of (OBO) flow, and when would you use it in a microservices architecture?

What is the On-Behalf-Of (OBO) flow, and when would you use it in a microservices architecture?

**Answer:** The On-Behalf-Of (OBO) flow is an OAuth 2.0 extension that allows a middle-tier API to exchange the access token it received from a client for a new access token scoped for a downstream API, while preserving the original user's identity throughout the chain. It is used when a downstream service needs to know who the original user is, not simply which service is calling.

- Without OBO, a middle-tier service calling a downstream API using Client Credentials would completely lose the user's identity — the downstream API would only see the middle-tier's application identity in the token, making user-level authorization and audit logging impossible.
- In OBO, the middle-tier sends its received access token to the token endpoint with `grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer` along with its own client credentials; the authorization server responds with a new token carrying the original user's claims but scoped to the downstream API's audience.
- The downstream API validates the OBO-derived token normally — checking the signature, `aud`, and `exp` claims — with no special handling required to detect that it arrived via OBO.
- OBO requires that the original access token include the necessary delegated scopes for the downstream API; the client must have consented to those scopes when it first requested the token, otherwise the OBO exchange is rejected.

---

## Q9. How does the Client Credentials flow work, and when is it appropriate for machine-to-machine (M2M) API access?

How does the Client Credentials flow work, and when is it appropriate for machine-to-machine (M2M) API access?

**Answer:** The Client Credentials flow is an OAuth 2.0 grant type where an application authenticates directly to the authorization server using its own client ID and a client secret or certificate, then receives an access token that represents the application's identity rather than any user's identity. It is designed for daemon services, background workers, and server-to-server communication where no human user is involved in the request.

- The token is obtained by posting `grant_type=client_credentials`, `client_id`, `client_secret` (or a signed JWT assertion for certificate-based auth), and `scope` to the token endpoint; no browser redirect or user consent interaction is required.
- The resulting access token contains application-level claims such as `appid` and `roles` (if application roles are assigned in Azure AD), but no user claims like `sub`, `name`, or `scp`, because no user authenticated.
- In Azure AD, you grant the calling application access via "Application permissions" rather than "Delegated permissions," and a tenant administrator must grant consent because there is no user who can consent on their own behalf.
- This flow should only be used for truly automated machine-to-machine scenarios; using it to impersonate users by forwarding their data within the system bypasses the security guarantees of delegated flows and is an architectural anti-pattern.

---

## Q10. What is token chaining in a microservices architecture, and what risks does it introduce?

What is token chaining in a microservices architecture, and what risks does it introduce?

**Answer:** Token chaining (also called token forwarding) refers to passing an access token received by one microservice directly to a downstream microservice as the authorization credential, rather than obtaining a new token scoped for that downstream call. While simple to implement, it introduces security and operational risks that the On-Behalf-Of (OBO) flow is designed to address.

- Raw token forwarding violates the principle of least privilege because the downstream service receives a token with the full scope set granted to the original caller, which may be far broader than what the downstream operation requires.
- If a downstream service is compromised, it retains a token that may be valid for other services in the chain; with properly scoped OBO tokens, the blast radius is limited to the specific downstream service's audience.
- Token forwarding makes audit trails ambiguous: all downstream services see only the original client's identity in the token, not the intermediate service that actually made the call, complicating incident response and compliance logging.
- Within a tightly controlled internal trust boundary (e.g., services running in the same Kubernetes namespace with network policies), token forwarding is sometimes accepted as a pragmatic trade-off, but it should be avoided when services cross security domains or have different trust levels.

---

## Q11. What is the Backend for Frontend (BFF) pattern, and why is it used to secure Single-Page Applications (SPAs)?

What is the Backend for Frontend (BFF) pattern, and why is it used to secure Single-Page Applications (SPAs)?

**Answer:** The Backend for Frontend (BFF) pattern places a server-side component between a Single-Page Application (SPA) and backend APIs; the BFF owns the entire OAuth 2.0 / OIDC authentication flow, stores tokens in a secure server-side session, and proxies API calls on behalf of the SPA. This architecture removes the need for the browser to handle or store access tokens at any point.

- The core motivation is that browsers cannot safely store tokens: `localStorage` is fully accessible to injected scripts (Cross-Site Scripting / XSS risk), and while `HttpOnly` cookies are script-inaccessible, the browser cannot directly attach a JWT cookie to an arbitrary API call — it needs the BFF intermediary.
- With BFF, the SPA communicates with the BFF using `HttpOnly` session cookies; the BFF attaches the stored access token to outgoing backend API requests server-side, so the token never appears in the browser's JavaScript execution environment.
- The BFF handles token refresh, re-authentication redirects, and propagating user identity to downstream APIs via OBO or token forwarding within the server-side trust boundary, completely hiding OAuth complexity from the frontend code.
- Frameworks like Duende BFF and `Microsoft.Identity.Web` for Razor Pages-based frontends provide built-in BFF support with session cookie management, CSRF protection, and transparent token renewal.

---

## Q12. What is Duende IdentityServer, and when would you self-host an authorization server instead of using a cloud IdP?

What is Duende IdentityServer, and when would you self-host an authorization server instead of using a cloud IdP?

**Answer:** Duende IdentityServer is a commercial OpenID Connect and OAuth 2.0 authorization server framework for ASP.NET Core that lets you self-host a fully standards-compliant authorization server within your own infrastructure. You would choose to self-host when you need full control over token issuance, custom grant types, on-premises deployment requirements, or business constraints that prevent using a cloud IdP such as Azure AD.

- Self-hosting is appropriate when compliance requirements mandate data sovereignty — for instance, regulations that prohibit sending authentication requests or user data to a cloud provider's servers in another jurisdiction.
- Duende IdentityServer supports all standard OAuth 2.0 grant types, OIDC flows, dynamic client registration, PKCE, and token introspection, making it a complete drop-in replacement for a cloud IdP.
- The trade-off is operational ownership: you are responsible for availability, patching, signing key rotation, and security hardening of the authorization server, whereas a cloud IdP like Entra ID handles all of that automatically with enterprise SLAs.
- Common self-hosting scenarios include Independent Software Vendor (ISV) products shipped to on-premises customer environments, B2C platforms requiring custom claim issuance logic or branding, and organizations with on-premises user stores (LDAP, SQL) they are not migrating to a cloud directory.

---

## Q13. What is OpenIddict, and how does it compare to Duende IdentityServer as a self-hosted authorization server option?

What is OpenIddict, and how does it compare to Duende IdentityServer as a self-hosted authorization server option?

**Answer:** OpenIddict is a fully open-source (Apache 2.0 license) OIDC and OAuth 2.0 server framework for ASP.NET Core that integrates directly with Entity Framework Core for persisting clients, tokens, and authorizations. It is the primary alternative to Duende IdentityServer when licensing cost is a decision factor, since Duende requires a commercial license for production deployments above the community revenue threshold.

- OpenIddict is built around a store abstraction with first-class support for Entity Framework Core and MongoDB, making it natural to embed inside an existing application that already uses those data access layers.
- Duende IdentityServer has a more polished ecosystem: richer official documentation, the QuickStart UI for rapid prototyping, the Duende BFF library, and a broader set of official samples — advantages that accelerate onboarding for teams less familiar with OAuth 2.0 internals.
- Both libraries implement the same core specifications (OIDC Core, OAuth 2.0 RFC 6749, PKCE RFC 7636, Token Introspection RFC 7662, etc.) and produce interoperable tokens; the choice is driven by licensing model and ecosystem preference rather than protocol capability.
- OpenIddict's design philosophy embeds the authorization server directly into the host ASP.NET Core application as middleware, reducing infrastructure footprint compared to running a standalone server, which suits smaller deployments well.

---

## Q14. What is dynamic client registration (DCR) in OAuth 2.0, and when is it useful?

What is dynamic client registration (DCR) in OAuth 2.0, and when is it useful?

**Answer:** Dynamic Client Registration (DCR), defined in RFC 7591, is an OAuth 2.0 protocol that allows client applications to register themselves with an authorization server programmatically at runtime by POSTing registration metadata to a registration endpoint, rather than requiring an administrator to manually pre-register each client. The authorization server responds with a `client_id` and optionally a `client_secret`.

- DCR is useful when clients are created dynamically at scale, such as a SaaS platform that provisions a dedicated OAuth client for each new customer tenant, or IoT deployments where devices self-register upon first activation without manual provisioning.
- The registration endpoint typically requires an initial access token — a bootstrap credential issued out-of-band by the authorization server administrator — to prevent anonymous, unauthenticated registration abuse.
- After registration, clients may use the client management endpoint (RFC 7592) to read, update, or delete their own registration, enabling self-service credential rotation without administrator involvement.
- Enabling DCR without strict initial access token requirements is a significant security risk, because it allows any party to register arbitrary clients and potentially request broad scopes; authorization server operators should always require bootstrap credentials.

---

## Q15. How do scopes work in a microservices architecture with per-service audiences?

How do scopes work in a microservices architecture with per-service audiences?

**Answer:** In a microservices architecture, each service defines its own OAuth 2.0 resource with a distinct `aud` (audience) value and exposes fine-grained scopes representing specific operations or data access levels. A client requests only the scopes it needs for a particular downstream call, and each service independently validates that the incoming token's `aud` matches its own identifier and that the required scope is present in the token.

- Defining per-service audiences ensures that a token issued for Service A cannot be replayed against Service B: Service B rejects any token where its own identifier is absent from the `aud` claim, providing a cryptographic binding between token and target service.
- Scopes express authorization — what the token holder is permitted to do — while the audience expresses scope of validity — which service(s) accept the token. Both checks are necessary; audience alone does not constrain what operation is allowed.
- A well-designed scope hierarchy might look like `orders.read`, `orders.write`, and `orders.admin`; a read-only integration client requests only `orders.read` even if broader scopes exist, applying the principle of least privilege.
- In Azure AD, APIs expose scopes as "Delegated permissions" (validated via the `scp` claim in the token) and application-level permissions as "Application permissions" (validated via the `roles` claim); downstream APIs must check the appropriate claim based on whether the caller used a delegated or application flow.

---

## Q16. What is the difference between the `aud` (audience) claim and the `scope` claim in a JWT access token?

What is the difference between the `aud` (audience) claim and the `scope` claim in a JWT access token?

**Answer:** The `aud` (audience) claim identifies which resource server or servers the token is valid for; a resource server must reject any token where its own identifier is not present in `aud`. The `scope` claim (or `scp` in Azure AD tokens) describes what the token holder is authorized to do within that audience — it is an authorization control layered on top of the audience binding.

- `aud` is a routing and replay-prevention control: it ensures a token obtained for one API cannot be presented to a different API, because each service only accepts tokens that explicitly name it as the intended audience.
- `scope` is an authorization granularity control within an accepted token: even when a valid, correctly-addressed token is presented, the service checks the scope claim to determine which specific operations are permitted.
- A token may contain multiple values in the `aud` claim (as a JSON array) if it is intended for multiple related services, though this reduces the isolation benefit of per-service audiences because one compromised service holds a token valid elsewhere.
- In ASP.NET Core with JWT Bearer middleware, the `aud` claim is validated automatically when `options.TokenValidationParameters.ValidAudience` is set; scope checks require explicit policy configuration, for example using `RequireScope("orders.read")` from `Microsoft.Identity.Web`.

---

## Q17. What is silent token refresh in a SPA, and how does it work with OIDC?

What is silent token refresh in a SPA, and how does it work with OIDC?

**Answer:** Silent token refresh is the technique where a Single-Page Application (SPA) obtains a new access token before the current one expires, without requiring the user to go through an interactive sign-in redirect. In the traditional iFrame-based approach, the SPA opens a hidden iFrame pointing to the authorization server's authorization endpoint with `prompt=none`, which causes the server to return a new token using the user's existing session cookie without displaying any UI.

- The `prompt=none` parameter instructs the IdP to return a token immediately if the user has an active session, or an error code (`login_required` or `interaction_required`) if no session exists, signaling that an interactive login is needed.
- Modern browsers are progressively blocking third-party cookies — Safari's Intelligent Tracking Prevention and Chrome's Privacy Sandbox — which breaks the iFrame approach because the hidden iFrame cannot access the IdP's session cookie when the SPA and IdP are on different domains.
- The preferred modern alternative is issuing a refresh token with the `offline_access` scope using the PKCE-protected Authorization Code flow; the SPA stores the refresh token in memory (never in `localStorage`) and exchanges it for new access tokens as needed without an iFrame.
- The Backend for Frontend (BFF) pattern sidesteps the silent refresh problem entirely by moving all token management to the server side, where cookies and sessions behave predictably regardless of browser restrictions.

---

## Q18. What is OIDC Session Management, and how does it differ from silent token refresh?

What is OIDC Session Management, and how does it differ from silent token refresh?

**Answer:** OpenID Connect (OIDC) Session Management is a specification that defines how a Relying Party (RP — the client application) can detect and react to changes in the user's authentication session at the Identity Provider (IdP), such as the user signing out from another application that shares the same IdP session. Silent token refresh addresses token expiry; OIDC Session Management addresses external session termination events.

- OIDC Session Management uses a polling mechanism where the RP opens a hidden iFrame that sends periodic `postMessage` signals to an IdP frame; a changed session state signal from the IdP frame indicates the user's session has changed and the RP should re-validate.
- OIDC Front-Channel Logout delivers logout notifications to the RP by having the IdP load the RP's logout URL in a browser iFrame when another client logs out; it depends on the user's browser being open and reachable.
- OIDC Back-Channel Logout delivers logout notifications server-to-server via an HTTP POST from the IdP directly to the RP's registered back-channel logout endpoint, making it reliable even when the user's browser is closed or their browser tab is not active.
- In enterprise SSO scenarios, Back-Channel Logout is the more robust choice; Front-Channel Logout is simple but fragile, and the iFrame-based Session Management polling has the same third-party cookie limitations as iFrame-based silent refresh.

---

## Q19. What is the OAuth 2.0 token revocation endpoint, and how does it work?

What is the OAuth 2.0 token revocation endpoint, and how does it work?

**Answer:** The OAuth 2.0 Token Revocation endpoint (RFC 7009) is an authorization server endpoint where a client notifies the server that a specific access token or refresh token should be considered invalid and discard immediately. It is the primary mechanism for implementing logout that invalidates tokens on the server side rather than just clearing them from the client.

- The client sends an HTTP POST to the revocation endpoint with the token value and optionally a `token_type_hint` (`refresh_token` or `access_token`) to help the server locate the token efficiently; the server responds with `200 OK` regardless of whether the token was found.
- Revoking a refresh token is immediately effective: the client can no longer exchange it for new access tokens, so once the current short-lived access token expires naturally, the session is fully terminated.
- Revoking a JWT access token has limited practical effect for resource servers that perform local validation, because the JWT remains cryptographically valid until its `exp` (expiration) timestamp — the resource server does not contact the authorization server during local validation and will not know about the revocation.
- For true immediate access token revocation to take effect, the resource server must call the introspection endpoint on every request to get the live token state, rather than relying on local JWT signature and expiry verification.

---

## Q20. What is the token introspection endpoint, and when should you use it instead of local JWT validation?

What is the token introspection endpoint, and when should you use it instead of local JWT validation?

**Answer:** The OAuth 2.0 Token Introspection endpoint (RFC 7662) allows a resource server to query the authorization server at request time to determine whether a presented token is currently active — meaning not expired and not revoked — and to retrieve the token's claims in a normalized JSON format. It is an alternative to local JWT validation that provides real-time token state rather than relying solely on the token's embedded expiry.

- Local JWT validation is fast: the resource server verifies the cryptographic signature and checks the `exp` claim without any network call, but it cannot detect a token that was revoked mid-lifetime because the JWT remains structurally valid until `exp`.
- Introspection adds a synchronous network call to every API request, introducing latency and a dependency on the authorization server's availability, but it is the correct approach when the security model requires that revocation take effect immediately rather than waiting for token expiry.
- A resource server authenticates to the introspection endpoint with its own client credentials (client ID and secret), preventing unauthorized parties from using the endpoint to enumerate token validity.
- A practical optimization is a hybrid approach: validate the JWT signature locally to cheaply reject structurally invalid tokens, then call introspection only for sensitive or high-privilege operations where revocation latency is not acceptable.

---

## Q21. What are the security considerations for using the implicit grant flow versus the Authorization Code + PKCE flow in SPAs?

What are the security considerations for using the implicit grant flow versus the Authorization Code + PKCE flow in SPAs?

**Answer:** The OAuth 2.0 implicit grant flow returns access tokens directly in the redirect URI fragment (the `#` portion of the URL), making them visible in browser history, referrer headers logged by servers, and accessible to any JavaScript executing on the page. The Authorization Code flow with Proof Key for Code Exchange (PKCE) exchanges a short-lived code for tokens via a back-channel API call, keeping tokens out of the URL entirely and out of browser history.

| Dimension | Implicit Flow | Auth Code + PKCE |
|---|---|---|
| Token location | URL fragment (`#access_token=...`) | Back-channel JSON response |
| Visible in browser history | Yes | No |
| Refresh tokens | Not supported | Supported |
| Code interception protection | None | PKCE verifier/challenge binding |
| Currently recommended | No (deprecated by OAuth 2.0 Security BCP) | Yes |

- The URL fragment is readable by any JavaScript on the page, including injected scripts, and may be captured by browser extensions, proxies, or server access logs if a redirect occurs after the fragment is set.
- PKCE prevents authorization code interception attacks: the client generates a `code_verifier`, sends a `code_challenge` (its SHA-256 hash) with the authorization request, and must present the original `code_verifier` during the code exchange — only the originating client can complete the exchange even if the code is intercepted.
- The OAuth 2.0 Security Best Current Practice (BCP, RFC 9700) explicitly deprecates the implicit flow and the resource-owner password credentials flow for all new implementations, recommending Authorization Code + PKCE universally.

---

## Q22. What is the difference between delegated permissions and application permissions in Azure AD?

What is the difference between delegated permissions and application permissions in Azure AD?

**Answer:** In Azure AD, delegated permissions represent access that an application exercises on behalf of a signed-in user; the effective access is the intersection of what the user is permitted to do and what the application is authorized to request. Application permissions represent access that an application exercises with its own identity, independent of any user, and always require explicit administrator consent.

- Delegated permissions are used in flows where a user is present (Authorization Code, On-Behalf-Of); the resulting access token contains the `scp` claim listing the consented delegated scopes, and the resource API can combine scope checks with user identity checks.
- Application permissions are used in the Client Credentials flow where no user is involved; the resulting token contains the `roles` claim listing the granted application roles, and there is no `scp` claim because no user delegated any permission.
- A common least-privilege mistake is granting an application a broad application permission (e.g., `Mail.ReadWrite` covering all mailboxes in the tenant) when the application only needs delegated access to a single user's mailbox — a principle of least privilege violation with significant blast radius.
- Azure AD conditional access policies can enforce user-level conditions (require MFA, compliant device) on delegated permission tokens, but generally cannot apply user-level conditions to application permission tokens because no user context exists in those tokens.

---

## Q23. How do you achieve tenant isolation in a multi-tenant application to prevent cross-tenant data leakage?

How do you achieve tenant isolation in a multi-tenant application to prevent cross-tenant data leakage?

**Answer:** Tenant isolation in a multi-tenant application means ensuring that a user or application authenticated from Tenant A cannot read, modify, or delete data belonging to Tenant B, even if both users make requests to the same API. The primary mechanism is extracting the `tid` (tenant ID) claim from the validated access token and treating it as a mandatory filter on all data access operations.

- Every database query, cache lookup, and storage operation should include the tenant ID as a non-optional predicate; a query that returns records without filtering by tenant is a data leakage vulnerability, regardless of the authentication layer's correctness.
- Row-Level Security (RLS) in databases such as SQL Server, using `SESSION_CONTEXT` combined with a security policy predicate, enforces tenant filtering at the database engine level, providing a defense-in-depth backstop against application-layer bugs that omit the tenant filter.
- During token validation, always verify the `tid` claim against an allowlist of known tenants if you operate a closed multi-tenant system (not open to all Azure AD tenants); accepting tokens from any tenant creates an unauthenticated registration vector if any Azure AD user can request access.
- Logging and audit trails must include the tenant ID on every log entry to enable per-tenant activity analysis, billing, and security incident response without risk of one tenant's logs contaminating another's.

---

## Q24. What is PKCE (Proof Key for Code Exchange), and why was it introduced in OAuth 2.0?

What is PKCE (Proof Key for Code Exchange), and why was it introduced in OAuth 2.0?

**Answer:** Proof Key for Code Exchange (PKCE, pronounced "pixie"), defined in RFC 7636, is an OAuth 2.0 extension that prevents authorization code interception attacks by cryptographically binding the initial authorization request to the subsequent token exchange request. Without PKCE, a malicious application on the same device could intercept the authorization code delivered to the redirect URI and exchange it for tokens before the legitimate client does.

- The client generates a random, high-entropy `code_verifier` string, computes a `code_challenge` (the Base64URL-encoded SHA-256 hash of the verifier), and sends the challenge with the authorization request; the server stores it.
- During the code exchange, the client sends the original `code_verifier`; the authorization server hashes it and compares it against the stored `code_challenge` — only the client that initiated the authorization request possesses the verifier, so intercepted codes are useless without it.
- PKCE was initially designed for public clients (mobile apps, desktop apps, SPAs) that cannot safely store a client secret, but RFC 9700 recommends PKCE for all authorization code flows, including confidential clients, as an additional security layer.
- In ASP.NET Core, the OIDC middleware (`AddOpenIdConnect`) automatically enables PKCE when `options.ResponseType = OpenIdConnectResponseType.Code` is configured; no extra code is required to generate or verify the `code_verifier`.

---

## Q25. What is a federated credential, and how is it used in Azure Workload Identity or Managed Identity scenarios?

What is a federated credential, and how is it used in Azure Workload Identity or Managed Identity scenarios?

**Answer:** A federated credential in Azure AD is a trust relationship that allows an external identity token — such as a GitHub Actions OIDC token or a Kubernetes service account token — to be exchanged for an Azure AD access token without using a client secret or certificate. Azure Managed Identity is a related but distinct mechanism where the Azure platform assigns and manages a service principal for an Azure resource (VM, container, function app) so that the workload can authenticate to Azure AD without any application-managed credentials in configuration files.

- Federated credentials eliminate the need to store and rotate client secrets in CI/CD pipelines and container workloads; the pipeline proves its identity through an OIDC token issued by the hosting platform (GitHub, Azure Kubernetes Service, GitLab), which Azure AD trusts and exchanges for its own token.
- With GitHub Actions, you configure an Azure AD app registration to trust tokens from `https://token.actions.githubusercontent.com` for a specific repository and branch; the workflow exchanges its GitHub OIDC token for an Azure AD access token at runtime using the `azure/login` action with `with: client-id` and no secret.
- Managed Identity is specific to Azure resources: the platform injects short-lived credentials into the compute environment, and the application retrieves them from the Instance Metadata Service (IMDS) endpoint at `169.254.169.254`; `DefaultAzureCredential` from the Azure SDK handles this automatically in any environment.
- Both approaches implement the no-long-lived-secrets principle: federated credentials expire with the workflow run, and managed identity credentials are rotated automatically by the platform, eliminating the credential hygiene burden from developers.

---

## Q26. What are the common security risks in token forwarding (token chaining) across microservices?

What are the common security risks in token forwarding (token chaining) across microservices?

**Answer:** Token forwarding — passing an access token received by one service to a downstream service as the authorization credential — introduces several security risks: the downstream service receives a token with the original caller's full scope set (often broader than the downstream call needs), token exposure spreads to more services, and audit trails become ambiguous because intermediate services are invisible in the token's claim history.

- If a downstream service is compromised, it holds a token valid for the entire scope set of the original calling client, not just the scopes needed for its own function; the On-Behalf-Of (OBO) flow limits this by producing a token scoped specifically to the downstream API's audience.
- Token forwarding makes forensic analysis of unauthorized access harder: downstream service logs show the original client's identity from the token, not the identity of the intermediate service that actually forwarded it, making it difficult to trace the call path during incident response.
- In mutual TLS (mTLS) architectures, service-to-service calls can be authenticated by client certificates at the transport layer, providing a service-specific identity assertion that does not depend on forwarded user tokens and is not confused with user-level authorization.
- Token forwarding is sometimes accepted within a tightly controlled internal network with strong perimeter controls and consistent trust levels, but it should always be avoided when services cross security domains, have different data access levels, or are operated by different teams with different security standards.

---

## Q27. What is claims transformation in ASP.NET Core, and when is it useful in federated identity scenarios?

What is claims transformation in ASP.NET Core, and when is it useful in federated identity scenarios?

**Answer:** Claims transformation in ASP.NET Core is the process of modifying or augmenting the `ClaimsPrincipal` after the incoming token is validated but before the request handler executes, by implementing the `IClaimsTransformation` interface. It is useful in federated identity scenarios where the external IdP's token contains only generic identifiers, and the application needs to enrich them with application-specific attributes such as internal roles, permissions, or tenant metadata.

- A common federated identity use case is mapping Azure AD group object IDs (GUIDs that appear in the token's `groups` claim) to human-readable application role names; claims transformation resolves each GUID to a named role by querying a local database or Microsoft Graph at request time.
- `IClaimsTransformation.TransformAsync` is invoked on every request after authentication succeeds, so implementations must perform fast, cache-friendly work; a database call on every request without caching will degrade API throughput significantly.
- Transformed claims exist only in memory for the duration of the current request; they are not written back to the JWT or persisted in the user's session automatically, so the same transformation runs on every request unless the results are cached by the implementation.
- Claims transformation should not introduce security-relevant logic that silently defaults to granting access if it fails or returns empty results; a failed transformation should produce minimal claims rather than elevated ones, and any cache invalidation strategy must account for mid-session permission changes.

---

## Q28. How does the BFF pattern compare to storing tokens in browser-accessible storage for securing SPAs?

How does the BFF pattern compare to storing tokens in browser-accessible storage for securing SPAs?

**Answer:** In the Backend for Frontend (BFF) pattern, tokens are stored in a server-side session and the browser communicates with the BFF using `HttpOnly` cookies that JavaScript cannot read. In the browser storage approach, the SPA stores tokens in memory, `sessionStorage`, or `localStorage` and attaches them directly to API requests — a simpler architecture that places security responsibility entirely in the browser environment.

| Dimension | BFF (server-side tokens) | Browser memory | localStorage |
|---|---|---|---|
| XSS token theft risk | Tokens never reach browser JS | Low — cleared on tab close | High — persists across sessions |
| CSRF risk | Yes — mitigated by SameSite + CSRF token | None | None |
| Requires server component | Yes | No | No |
| Refresh token support | Yes, server-side | Yes, but in browser memory | Yes, but persists insecurely |
| 3rd-party cookie deprecation impact | None | iFrame silent refresh broken | iFrame silent refresh broken |

- The BFF pattern provides the strongest security posture: even a successful XSS attack cannot extract tokens from `HttpOnly` cookies or a server-side session, because those values are never transmitted to the browser's JavaScript execution environment.
- Browser in-memory storage is the best available option for SPAs that cannot adopt a BFF: tokens are cleared when the page is closed, limiting the window of exposure, but an XSS attack within the same tab lifetime can still read in-memory tokens.
- `localStorage` should not be used to store access tokens or refresh tokens in production applications; it persists indefinitely across sessions, is accessible to any JavaScript on the same origin including injected scripts, and survives browser restarts.
- The BFF pattern requires careful Cross-Site Request Forgery (CSRF) mitigation because it relies on cookies; using `SameSite=Strict` or `SameSite=Lax` on session cookies combined with a CSRF token header check on state-changing requests is the standard defense.

---

## Gotchas — Federated Identity & Advanced Auth (Interview Traps)

#### Gotcha 1. Setting `ValidateIssuer = false` without a replacement validator in multi-tenant apps

**Answer:** A common shortcut for multi-tenant apps is setting `ValidateIssuer = false` in `TokenValidationParameters`, believing this is sufficient to accept tokens from any tenant. In reality, disabling the issuer check without installing a custom `IssuerValidator` means the middleware will accept a valid JWT from any Azure AD tenant in the world — including tenants belonging to attackers who can register their own apps and obtain tokens signed by Azure AD's keys.

- The correct approach is to pair `ValidateIssuer = false` with a custom `IssuerValidator` that reconstructs the expected issuer from the token's `tid` claim and checks an allowlist of permitted tenants, so only known tenants are accepted.
- `Microsoft.Identity.Web` handles this correctly by default when `"TenantId": "common"` is configured; the problem occurs when developers configure the raw `AddJwtBearer` middleware manually and only flip the boolean.

---

#### Gotcha 2. Confusing Client Credentials with OBO — losing user identity in the middle tier

**Answer:** A common design mistake is having a middle-tier API call a downstream API using Client Credentials because "it's simpler," when the downstream API actually needs to know who the original user is for authorization or audit purposes. Client Credentials produces a token with the middle tier's application identity only; the user context is completely absent.

- The On-Behalf-Of (OBO) flow exists precisely for this situation: the middle tier exchanges its received user-delegated token for a new token that carries the original user's identity and is scoped to the downstream API's audience.
- If the downstream API relies on user identity for row-level authorization (e.g., "a user can only see their own records"), using Client Credentials bypasses that gate entirely because the downstream API has no user identity to filter on.

---

#### Gotcha 3. Assuming access token revocation works immediately with local JWT validation

**Answer:** Many developers call the revocation endpoint to implement logout and assume the access token is immediately invalid across all resource servers. For JWTs validated locally (signature check + `exp` check), the revocation endpoint has no immediate effect — the token remains cryptographically valid and will pass local validation until its `exp` timestamp is reached.

- Revoking a refresh token is the more impactful action: it prevents the client from obtaining new access tokens, so once the current short-lived access token expires naturally, the user's session ends.
- For scenarios requiring true immediate access token invalidation — for example, an emergency account suspension — resource servers must use the introspection endpoint on every request to check live token state, which adds a network round trip per API call.

---

#### Gotcha 4. Using the implicit flow for new SPA implementations

**Answer:** The OAuth 2.0 implicit flow was the standard recommendation for SPAs for many years, and some older tutorials and courses still describe it as the correct approach. It is explicitly deprecated by the OAuth 2.0 Security Best Current Practice (RFC 9700) and should not be used in any new implementation.

- The implicit flow returns access tokens in the URL fragment, making them visible in browser history, server access logs on redirect targets, and accessible to any JavaScript on the page — a significant attack surface that the Authorization Code + PKCE flow eliminates.
- All modern OIDC libraries (MSAL.js, oidc-client-ts, Auth0 SPA SDK) default to Authorization Code + PKCE; using the implicit flow in a new project typically requires explicitly opting into a deprecated mode.

---

#### Gotcha 5. Checking `scp` for application tokens or `roles` for delegated tokens

**Answer:** Azure AD issues access tokens with different claim structures depending on the grant type: delegated tokens (Authorization Code, OBO) carry the `scp` claim listing consented scopes, while application tokens (Client Credentials) carry the `roles` claim listing assigned application roles. Checking only `scp` in a resource API that accepts both token types means application-identity callers will always fail the scope check even when properly authorized.

- A robust API validates the claim appropriate to the token's context: check `scp` for delegated (user) tokens and `roles` for application tokens; the `idtyp` claim (`user` vs `app`) distinguishes the two types in newer Azure AD tokens.
- `Microsoft.Identity.Web`'s `RequireScope` extension validates the `scp` claim and will reject application tokens; for APIs that accept both patterns, explicit policy logic or the `AcceptedScope` combined with `AcceptedAppPermission` attributes are the correct tools.

---
