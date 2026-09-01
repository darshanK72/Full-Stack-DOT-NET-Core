# Azure Entra ID — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Microsoft Entra ID (formerly Azure Active Directory), and how does it differ from on-premises Active Directory?](#q1-what-is-microsoft-entra-id-formerly-azure-active-directory-and-how-does-it-differ-from-on-premises-active-directory)
2. [Q2. What is an Azure Entra tenant, and what core identity objects does it contain?](#q2-what-is-an-azure-entra-tenant-and-what-core-identity-objects-does-it-contain)
3. [Q3. What is the relationship between OAuth 2.0 and OpenID Connect (OIDC) in Azure Entra ID authentication?](#q3-what-is-the-relationship-between-oauth-20-and-openid-connect-oidc-in-azure-entra-id-authentication)
4. [Q4. What are delegated permissions versus application permissions in Azure Entra ID?](#q4-what-are-delegated-permissions-versus-application-permissions-in-azure-entra-id)
5. [Q5. What is an app registration in Azure Entra ID, and why do you create one?](#q5-what-is-an-app-registration-in-azure-entra-id-and-why-do-you-create-one)
6. [Q6. What is a service principal, and how does it relate to an app registration?](#q6-what-is-a-service-principal-and-how-does-it-relate-to-an-app-registration)
7. [Q7. What is the difference between the Application (client) ID and the Directory (tenant) ID?](#q7-what-is-the-difference-between-the-application-client-id-and-the-directory-tenant-id)
8. [Q8. How do client secrets differ from certificates for app authentication, and when would you choose each?](#q8-how-do-client-secrets-differ-from-certificates-for-app-authentication-and-when-would-you-choose-each)
9. [Q9. What is an Enterprise Application in Azure Entra ID, and how does it differ from an App Registration?](#q9-what-is-an-enterprise-application-in-azure-entra-id-and-how-does-it-differ-from-an-app-registration)
10. [Q10. Describe the authorization code flow with PKCE and why it is the recommended flow for web and mobile apps.](#q10-describe-the-authorization-code-flow-with-pkce-and-why-it-is-the-recommended-flow-for-web-and-mobile-apps)
11. [Q11. What is the client credentials flow, and when should a .NET background service use it?](#q11-what-is-the-client-credentials-flow-and-when-should-a-net-background-service-use-it)
12. [Q12. What is the on-behalf-of (OBO) flow, and what problem does it solve in multi-tier APIs?](#q12-what-is-the-on-behalf-of-obo-flow-and-what-problem-does-it-solve-in-multi-tier-apis)
13. [Q13. What are scopes, and how do they relate to API permissions in app registrations?](#q13-what-are-scopes-and-how-do-they-relate-to-api-permissions-in-app-registrations)
14. [Q14. What is the `/.well-known/openid-configuration` endpoint, and what information does it expose?](#q14-what-is-the-well-knownopenid-configuration-endpoint-and-what-information-does-it-expose)
15. [Q15. What is token caching, and why is it important when using MSAL in production?](#q15-what-is-token-caching-and-why-is-it-important-when-using-msal-in-production)
16. [Q16. What is a managed identity in Azure, and what problem does it eliminate?](#q16-what-is-a-managed-identity-in-azure-and-what-problem-does-it-eliminate)
17. [Q17. What is the difference between system-assigned and user-assigned managed identities?](#q17-what-is-the-difference-between-system-assigned-and-user-assigned-managed-identities)
18. [Q18. How does a .NET application running on Azure App Service acquire a token using a managed identity?](#q18-how-does-a-net-application-running-on-azure-app-service-acquire-a-token-using-a-managed-identity)
19. [Q19. Which Azure services commonly use managed identities instead of client secrets?](#q19-which-azure-services-commonly-use-managed-identities-instead-of-client-secrets)
20. [Q20. What is MSAL (Microsoft Authentication Library), and how does it differ from the legacy ADAL?](#q20-what-is-msal-microsoft-authentication-library-and-how-does-it-differ-from-the-legacy-adal)
21. [Q21. How do you configure an ASP.NET Core web app to sign users in with Microsoft Entra ID using Microsoft.Identity.Web?](#q21-how-do-you-configure-an-aspnet-core-web-app-to-sign-users-in-with-microsoft-entra-id-using-microsoftidentityweb)
22. [Q22. How does the `[Authorize]` attribute behave when Microsoft Entra ID is configured as the authentication scheme?](#q22-how-does-the-authorize-attribute-behave-when-microsoft-entra-id-is-configured-as-the-authentication-scheme)
23. [Q23. How do you read user claims (roles, groups, `oid`, `tid`) from an Entra ID token in ASP.NET Core?](#q23-how-do-you-read-user-claims-roles-groups-oid-tid-from-an-entra-id-token-in-aspnet-core)
24. [Q24. How do you protect an ASP.NET Core Web API with Azure Entra ID using JWT bearer validation?](#q24-how-do-you-protect-an-aspnet-core-web-api-with-azure-entra-id-using-jwt-bearer-validation)
25. [Q25. What is an App Role versus a group-based authorization check in Entra ID?](#q25-what-is-an-app-role-versus-a-group-based-authorization-check-in-entra-id)
26. [Q26. How do you expose a custom API scope in an app registration and assign it to a calling application?](#q26-how-do-you-expose-a-custom-api-scope-in-an-app-registration-and-assign-it-to-a-calling-application)
27. [Q27. What is the audience (`aud`) claim in an Entra ID access token, and why must your API validate it?](#q27-what-is-the-audience-aud-claim-in-an-entra-id-access-token-and-why-must-your-api-validate-it)
28. [Q28. What is a multi-tenant Azure Entra application, and what changes when you support accounts from any organization?](#q28-what-is-a-multi-tenant-azure-entra-application-and-what-changes-when-you-support-accounts-from-any-organization)

---

## Q1. What is Microsoft Entra ID (formerly Azure Active Directory), and how does it differ from on-premises Active Directory?

**Concepts**
- Cloud-based identity and access management service
- OAuth 2.0 and OIDC — internet-facing protocols vs Kerberos and LDAP
- On-premises AD — domain-joined machines, Group Policy, corporate LAN
- Entra Connect — synchronizes on-premises AD users into Entra ID
- Cloud-native capabilities — MFA, passwordless, risk-based Conditional Access

**Answer**

Microsoft Entra ID is Microsoft's cloud-based identity and access management service that authenticates users, issues tokens, and enforces access policies for Microsoft 365, Azure resources, and custom applications. It is not a lift-and-shift of on-premises Active Directory — it is a purpose-built cloud directory designed for internet-facing protocols like OAuth 2.0 and OpenID Connect rather than traditional Kerberos and LDAP domain joins. On-premises AD is optimized for Windows domain-joined machines inside a corporate network, using Kerberos tickets and Group Policy for workstation management. Entra ID centers on user accounts, groups, app registrations, and conditional access policies that apply across cloud and hybrid scenarios without requiring every client to sit on the corporate LAN. Many organizations run both: Entra Connect synchronizes on-premises AD users and groups into Entra ID so the same identities can sign in to cloud apps while legacy on-premises apps continue using domain controllers. Entra ID also adds cloud-native capabilities — multi-factor authentication, passwordless sign-in, and risk-based Conditional Access — that are not part of classic AD by default.

---

## Q2. What is an Azure Entra tenant, and what core identity objects does it contain?

**Concepts**
- Tenant — isolated Entra ID instance tied to one organization
- Users — people who authenticate and receive tokens
- Groups — user collections for role assignment and license assignment
- App registrations — application identity definitions with client IDs
- Enterprise applications — tenant-local service principal instances with consent and user assignments
- Conditional Access policies — signal-based allow/block/step-up rules

**Answer**

An Azure Entra tenant is an isolated instance of Microsoft Entra ID tied to one organization, identified by a unique Directory (tenant) ID and a default domain such as `contoso.onmicrosoft.com`. Every user, application, and policy you manage in Entra ID lives inside exactly one tenant unless you explicitly configure cross-tenant or multi-tenant access. Users represent people — employees, guests — who authenticate and receive tokens; guest users from other tenants are invited via B2B collaboration. Groups collect users for role assignment, license assignment, and app access. App registrations define how applications authenticate and what permissions they request; each registration has a corresponding service principal in the tenant directory. Enterprise applications are the tenant-local instance of a registered app, where administrators grant consent and assign users or groups to the app. Conditional Access policies attach to users, apps, and signals — location, device compliance, risk level — to allow, block, or require step-up authentication.

---

## Q3. What is the relationship between OAuth 2.0 and OpenID Connect (OIDC) in Azure Entra ID authentication?

**Concepts**
- OAuth 2.0 — authorization framework for access tokens to call protected APIs
- OIDC — identity layer on top of OAuth 2.0 adding ID tokens for sign-in
- Access token — authorizes API calls; ID token — establishes login session
- `AddOpenIdConnect` for MVC sign-in vs `AddJwtBearer` for API protection
- Microsoft identity platform v2.0 endpoint

**Answer**

OAuth 2.0 is an authorization framework that lets a client obtain an access token to call a protected API on behalf of a user or itself, without sharing the user's password with that client. OpenID Connect is an identity layer built on top of OAuth 2.0 that adds an ID token — a signed JWT carrying authentication claims such as who signed in and when — so applications can establish a login session, not just API access. In Entra ID, the same token endpoint can return both an access token for calling Microsoft Graph or your own API and an ID token for sign-in. OAuth 2.0 alone does not standardize user identity claims; OIDC fills that gap with standard claims like `sub`, `iss`, and `aud` on the ID token. ASP.NET Core apps that sign users in with Microsoft use OIDC middleware via `AddOpenIdConnect` to validate the ID token and create a `ClaimsPrincipal`; API protection middleware via `AddJwtBearer` validates access tokens. Microsoft Entra ID implements both per the Microsoft identity platform v2.0 endpoint at `login.microsoftonline.com/{tenant}/oauth2/v2.0/...`.

---

## Q4. What are delegated permissions versus application permissions in Azure Entra ID?

**Concepts**
- Delegated permissions — app acts on behalf of a signed-in user, bounded by user's rights
- Application permissions — app acts as itself with no user context
- `scp` claim — delegated scopes in access token
- `roles` claim — application permissions in access token
- Admin consent required for application permissions — no user boundary

**Answer**

Delegated permissions allow an application to act on behalf of a signed-in user, limited to what that user is allowed to do in the target resource. Application permissions allow the application itself to authenticate with no user context and access data or operations granted to the app identity, typically used by background services or daemons. Delegated permissions appear as scopes in access tokens via the `scp` or `scope` claim; application permissions appear as roles via the `roles` claim on the token. Admin consent is often required for both types in enterprise tenants, but application permissions always require an administrator because there is no user boundary to limit abuse. Choosing the wrong type — requesting application permissions when a user-delegated call suffices — is a common over-permissioning mistake.

| | Delegated | Application |
|---|---|---|
| User context | Required — a user must sign in | None — app uses its own identity |
| Typical flow | Authorization code, OBO | Client credentials |
| Consent | User or admin grants scopes | Admin consent required |
| Risk if misused | Limited by user's own rights | Can access all data the permission allows |

---

## Q5. What is an app registration in Azure Entra ID, and why do you create one?

**Concepts**
- App registration — global definition of application identity in Entra ID
- Application (client) ID — GUID identifying the app in token requests
- Redirect URIs — security control preventing authorization code interception
- Exposed scopes and app roles — for APIs other apps request during consent
- Creation via Entra admin center, Azure CLI, or Bicep/Terraform

**Answer**

An app registration is a global definition in Entra ID that describes your application's identity — its name, redirect URIs, supported OAuth flows, requested permissions, and credentials. You create one so Entra ID knows which client is requesting tokens, which URLs are allowed to receive authorization codes, and which APIs the app may access. Every registered app receives an Application (client) ID — a GUID that clients include in token requests to identify themselves. Redirect URIs are a critical security control: Entra ID only returns authorization codes to URIs listed on the registration, preventing code interception by arbitrary sites. For a custom Web API, the registration also defines exposed scopes for delegated access and app roles for application access that other apps request during consent.

---

## Q6. What is a service principal, and how does it relate to an app registration?

**Concepts**
- Service principal — tenant-local security identity receiving tokens and holding role assignments
- App registration — blueprint; service principal — tenant instance
- Multi-tenant SaaS — one registration, service principals in many tenants
- Admin consent recorded on service principal, not registration object
- Managed identity — special auto-managed service principal type

**Answer**

A service principal is the local representation of an application inside a specific Entra tenant — it is the security identity that actually receives tokens, holds assigned roles, and appears in audit logs for that tenant. An app registration is the blueprint; when the app is used in a tenant (including its home tenant), Entra ID creates a service principal object that links back to that registration. One app registration can have service principals in many tenants — this is how multi-tenant SaaS apps work: one registration, many enterprise instances. Administrators assign users and groups to the service principal (Enterprise Application blade) to control who may use the app, and admin consent for API permissions is recorded on the service principal rather than merely on the registration object. Managed identities are a special type of service principal created and managed automatically by Azure for Azure resources — no manual secret management on the registration.

---

## Q7. What is the difference between the Application (client) ID and the Directory (tenant) ID?

**Concepts**
- Application (client) ID — identifies the app registration across the Microsoft identity platform
- Directory (tenant) ID — identifies the Entra organization that issued the token
- Authority URL — `https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/...`
- `tid` claim in tokens — identifies the issuing tenant
- Single-tenant vs multi-tenant authority configuration

**Answer**

The Application (client) ID uniquely identifies your app registration across the Microsoft identity platform — every token request, MSAL configuration, and `Audience`/`ClientId` setting references this GUID. The Directory (tenant) ID uniquely identifies the Entra ID organization that issued the token or hosts the user accounts you authenticate against. Client ID answers "which application is calling?" — it is the same in every tenant where your multi-tenant app is installed. Tenant ID answers "which organization's directory issued this token?" — it appears in authority URLs like `https://login.microsoftonline.com/{tenantId}` and in the `tid` claim inside tokens. Single-tenant apps hard-code their own tenant ID in the authority so only users from that organization can sign in. Multi-tenant or "any organizational account" apps use `common` or `organizations` instead of a specific tenant ID in the authority, and validate the `tid` claim at runtime if they need to restrict access.

---

## Q8. How do client secrets differ from certificates for app authentication, and when would you choose each?

**Concepts**
- Client secret — shared string, leaks easily in logs and config files
- Certificate — public-key proof via signed client assertion, supports Key Vault auto-rotation
- Managed identity — eliminates both options for Azure-hosted workloads
- `client_assertion` parameter — certificate-based token request
- Production guidance — prefer certificates or managed identities over secrets

**Answer**

Both client secrets and certificates prove the application's identity when it requests tokens without a user (client credentials flow) or when exchanging codes at the token endpoint. A client secret is a random string stored by the app; a certificate uses public-key cryptography where Entra ID holds the public key and the app holds the private key to sign authentication requests. Secrets appear in source control leaks and pipeline logs more often than private keys stored in Azure Key Vault, so production guidance favors certificates or managed identities. Certificate-based client authentication uses a signed client assertion (`client_assertion` parameter) instead of sending a raw secret over the wire and supports auto-rotation via Key Vault. Managed identities eliminate both options for apps running on Azure since Azure provisions the identity and handles credential rotation automatically.

| | Client secret | Certificate |
|---|---|---|
| Format | Random string | X.509 cert with private key |
| Rotation | Manual; easy to leak | Supports auto-rotation via Key Vault |
| Proof mechanism | Send secret in token request | Sign JWT assertion with private key |
| Typical use | Dev/test, quick prototypes | Production, CI/CD, long-lived daemons |

---

## Q9. What is an Enterprise Application in Azure Entra ID, and how does it differ from an App Registration?

**Concepts**
- Enterprise Application — tenant-specific instance, IT admin-managed
- App Registration — developer-owned global definition of app identity
- One registration per app; Enterprise Application per tenant where it is used
- Gallery SaaS apps — Enterprise Application without developer access to registration
- Sign-in logs and audit activity visible in Enterprise Applications blade

**Answer**

An Enterprise Application is the tenant-specific instance of an application that administrators manage for access control, single sign-on configuration, and consent — it corresponds to the service principal in the directory. An App Registration is the developer-facing definition of the app's identity, redirect URIs, credentials, and API definitions — it may exist once while Enterprise Applications appear in every tenant where the app is used. Developers own app registrations — client ID, secrets, exposed API scopes; IT admins own enterprise applications — assign users, grant tenant-wide consent, configure Conditional Access. When you add an app from the gallery such as Salesforce or a third-party SaaS, you get an enterprise application and service principal but may not have access to the publisher's app registration. For your own API, you create the app registration first, then assign users to its enterprise application entry if you want to restrict who can obtain tokens. The Enterprise Applications blade is also where you find sign-in logs and audit activity for that app in your tenant.

---

## Q10. Describe the authorization code flow with PKCE and why it is the recommended flow for web and mobile apps.

**Concepts**
- Authorization code flow — redirect to Entra, receive code, exchange for tokens
- PKCE — `code_verifier` and `code_challenge` prevent code interception by attackers
- Public client requirement — SPAs and native apps cannot store a client secret
- MSAL handles PKCE generation automatically
- Confidential server-side apps — also benefit from PKCE for defense in depth

**Answer**

The authorization code flow redirects the user to Entra ID to sign in; after successful authentication, Entra ID redirects back to the app with a short-lived authorization code that the app exchanges at the token endpoint for access and ID tokens. PKCE (Proof Key for Code Exchange) adds a dynamically generated `code_verifier` and hashed `code_challenge` so that even if an attacker intercepts the authorization code, they cannot exchange it without the original verifier — which is essential for public clients that cannot store a client secret. The client generates a random `code_verifier` and sends its SHA-256 hash as `code_challenge` when starting the authorize request; the user authenticates at Entra ID and is redirected to the registered redirect URI with an authorization code; the client then POSTs to the token endpoint with the code, `code_verifier`, client ID, and redirect URI; Entra ID validates the verifier against the original challenge and returns tokens. Native and single-page applications must use PKCE since they cannot securely embed a client secret, and MSAL handles PKCE generation and the code exchange automatically when you call `AcquireTokenInteractive` or equivalent methods.

---

## Q11. What is the client credentials flow, and when should a .NET background service use it?

**Concepts**
- Client credentials flow — app authenticates as itself, no signed-in user
- `grant_type=client_credentials` with `.default` scope
- Application permissions — not delegated user scopes
- Managed identity preferred over client secrets on Azure compute
- Not for user-scoped operations — use delegated or OBO instead

**Answer**

The client credentials flow lets an application authenticate as itself — using its client ID plus a secret, certificate, or managed identity — and receive an access token with application permissions, with no signed-in user involved. A .NET background service such as a worker, Azure Function, or scheduled job should use this flow when it must call Microsoft Graph, a custom API, or another protected resource on a timer or message trigger without human interaction. The service requests a token from the Entra token endpoint with `grant_type=client_credentials` and the target scope such as `https://graph.microsoft.com/.default`; the returned access token carries application roles rather than delegated user scopes, so the API must authorize based on app roles and the service must be granted only the minimum application permissions required. Prefer managed identity over client secrets when the service runs on Azure App Service, Azure Functions, or similar — `DefaultAzureCredential` or `ManagedIdentityCredential` acquires tokens without stored secrets. Do not use client credentials when the operation should respect the current user's permissions — use delegated flows or on-behalf-of instead.

---

## Q12. What is the on-behalf-of (OBO) flow, and what problem does it solve in multi-tier APIs?

**Concepts**
- OBO flow — middle API exchanges user's incoming token for a new token to a downstream API
- Token audience violation — forwarding the same bearer token breaks downstream audience validation
- `AcquireTokenOnBehalfOf` in MSAL / `Microsoft.Identity.Web`
- User context preserved — downstream actions appear as the original user in audit logs
- Both APIs must be registered; middle API needs delegated permission to downstream API

**Answer**

The on-behalf-of flow allows a middle-tier API that receives a user's access token to exchange it for a new access token to call a downstream API, still acting as that same user. It solves the problem where a web API cannot simply forward the incoming bearer token to another API — downstream APIs require a token issued specifically for their audience, and forwarding tokens violates least-privilege and audience validation rules. The client obtains a user access token for the middle API; the middle API receives the request, validates the incoming token, and calls the Entra token endpoint with the OBO grant passing the user's token as the `assertion`; Entra ID returns a new access token scoped for the downstream API. Both APIs must be registered in Entra ID, and the middle API needs delegated permission to the downstream API with admin consent. OBO preserves user context and auditing — downstream actions appear as the user rather than as the middle tier's application identity. MSAL provides `AcquireTokenOnBehalfOf` in `Microsoft.Identity.Web` and `IConfidentialClientApplication` for this exchange.

---

## Q13. What are scopes, and how do they relate to API permissions in app registrations?

**Concepts**
- Scope — string identifier naming a delegated permission on a resource
- `api://{client-id}/{scope-name}` — full scope format for custom APIs
- `.default` scope — all statically consented permissions for the app
- `scp` claim in v2 access token — contains granted delegated scopes
- APIs must validate `scp` or `roles` per endpoint, not just accept any valid token

**Answer**

A scope is a string identifier that names a delegated permission to perform a specific action on a resource — for example `User.Read` on Microsoft Graph or a custom scope like `api://my-api/orders.read` on your own API. In app registrations, you define scopes under Expose an API; consuming apps request those scopes in the `scope` parameter during login or token acquisition, and administrators grant them during consent. The full scope format for custom APIs is typically `api://{client-id}/{scope-name}` or a configured Application ID URI plus the scope name. The `.default` scope is special: for client credentials it means all application permissions granted to this app; for delegated flows it can mean the static permissions configured on the registration. Granted scopes appear in the access token as the `scp` claim in v2 tokens or in the `roles` claim for application permissions. APIs should validate that the token contains the scope or role required for each endpoint rather than accepting any valid token from the tenant.

---

## Q14. What is the `/.well-known/openid-configuration` endpoint, and what information does it expose?

**Concepts**
- OpenID Connect discovery document — published JSON metadata by Entra ID
- Key fields — `issuer`, `authorization_endpoint`, `token_endpoint`, `jwks_uri`
- `jwks_uri` — public signing keys for token signature validation without redeployment
- Auto-configuration — MSAL and ASP.NET Core read this instead of hard-coding URLs
- Multi-tenant APIs — validate issuer against known tenant issuers via discovery

**Answer**

The OpenID Connect discovery document is a JSON metadata file published by Entra ID at `https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration` that tells clients and APIs where to send authentication and token requests and where to fetch signing keys. ASP.NET Core and MSAL use it to auto-configure issuer validation, authorization endpoints, and JSON Web Key Set (JWKS) URLs without hard-coding every URL per environment. Key fields include `issuer`, `authorization_endpoint`, `token_endpoint`, `jwks_uri`, and supported response types and scopes. The `jwks_uri` points to the public keys used to validate token signatures — APIs fetch this to support key rotation without redeployment. Using discovery is preferred over manually setting `Authority` and `MetadataAddress` inconsistently across services. For multi-tenant APIs, the issuer and signing keys must be validated against the tenant that issued the token, which is why production APIs often use `ValidateIssuer` with an issuer validator that accepts known tenant issuers.

---

## Q15. What is token caching, and why is it important when using MSAL in production?

**Concepts**
- Token caching — MSAL stores tokens and reuses valid ones instead of calling Entra on every request
- Access token cache — keyed by tenant, scope, account; avoids round trips
- Distributed cache for ASP.NET Core — Redis or SQL Server for multi-instance session sharing
- Refresh token renewal — MSAL uses refresh tokens for silent renewal without user interaction
- Encrypted at rest requirement — refresh tokens allow prolonged access if stolen

**Answer**

Token caching is MSAL's mechanism for storing access and refresh tokens so the library can return a still-valid access token from memory or persistent storage instead of calling Entra ID on every HTTP request. Without caching, every API call would trigger a network round trip to the token endpoint, quickly hit throttling limits, and add unnecessary latency. MSAL caches access tokens until near expiry based on the `exp` claim and uses refresh tokens for silent renewal in confidential and public client flows where refresh tokens are issued. For ASP.NET Core web apps, `Microsoft.Identity.Web` integrates distributed caches — Redis, SQL Server — so multiple server instances share the same token cache for a user session. Client credentials tokens are also cached in memory keyed by tenant and scope, which means worker services benefit significantly from this default behavior. Cache serialization must be encrypted at rest when refresh tokens are stored, because a stolen refresh token allows prolonged access as the user or application.

---

## Q16. What is a managed identity in Azure, and what problem does it eliminate?

**Concepts**
- Managed identity — Entra ID service principal automatically managed by Azure for a resource
- Eliminates secret sprawl — no client secrets or certificates in configuration
- Azure handles credential creation, renewal, and deletion
- Applies to Azure-hosted workloads only — local development uses fallback credentials
- Role assignment required — RBAC on the target service for data plane access

**Answer**

A managed identity is an Entra ID service principal automatically created and lifecycle-managed by Azure for a specific Azure resource, giving that resource an identity it can use to authenticate to other Azure services and Microsoft APIs without storing client secrets or certificates in configuration. It eliminates secret sprawl, rotation toil, and the risk of credentials leaking from `appsettings.json`, environment variables, or source control. Azure handles creation, renewal, and deletion of the identity credentials — your code only requests tokens via the local identity endpoint or Azure SDK credential types. The identity appears as an enterprise application/service principal in Entra ID, where administrators assign roles such as Storage Blob Data Contributor or API permissions like any other app. Managed identities can authenticate to Azure Resource Manager, Azure Key Vault, Azure Storage, Azure SQL, Microsoft Graph with assigned permissions, and custom APIs that accept Entra tokens. They apply to Azure-hosted workloads only — local development typically uses Azure CLI or Visual Studio credentials via `DefaultAzureCredential`, not a managed identity.

---

## Q17. What is the difference between system-assigned and user-assigned managed identities?

**Concepts**
- System-assigned — lifecycle bound to one resource, deleted when resource is deleted
- User-assigned — standalone Azure resource attached to multiple compute instances
- Sharing — user-assigned enables multiple App Service slots sharing one identity
- Naming — system-assigned is Azure-generated; user-assigned has a developer-chosen name
- `DefaultAzureCredential` with user-assigned — specify client ID via options or `AZURE_CLIENT_ID`

**Answer**

A system-assigned managed identity is tied to exactly one Azure resource — it is created when you enable identity on that resource and is deleted when the resource is deleted. A user-assigned managed identity is created as a standalone Azure resource that you can attach to zero, one, or many other resources, and it persists independently of any single compute instance. System-assigned identities suit a single app on one host where no sharing is needed. User-assigned identities are useful when several App Service slots, VM scale set instances, or containers must present the same identity to Key Vault or a database — you attach one user-assigned identity to all of them instead of granting identical permissions to multiple system-assigned identities. In .NET, `DefaultAzureCredential` resolves managed identity in Azure automatically; specify a user-assigned client ID via `DefaultAzureCredentialOptions.ManagedIdentityClientId` or the `AZURE_CLIENT_ID` environment variable when multiple identities are available on the same resource.

| | System-assigned | User-assigned |
|---|---|---|
| Lifecycle | Bound to one resource | Independent resource |
| Sharing | Cannot share across resources | Attach to multiple instances |
| Use case | Single app on one host | Multiple scale set instances sharing one identity |

---

## Q18. How does a .NET application running on Azure App Service acquire a token using a managed identity?

**Concepts**
- App Service identity endpoint — local REST endpoint injected by the platform
- `DefaultAzureCredential` or `ManagedIdentityCredential` from `Azure.Identity`
- `TokenRequestContext` with resource scope — target service audience
- User-assigned identity — `AZURE_CLIENT_ID` env var or `ManagedIdentityClientId` option
- Local development fallback — Azure CLI, Visual Studio, or environment-based service principal

**Answer**

When managed identity is enabled on App Service, the platform exposes a local REST endpoint and environment variables that the Azure Identity library uses to obtain Entra ID tokens without any secret in your code. In .NET you use `DefaultAzureCredential` or `ManagedIdentityCredential` from the `Azure.Identity` package and call `GetTokenAsync` with the target resource scope. For user-assigned identity, pass `ManagedIdentityClientId` in `DefaultAzureCredentialOptions` or set `AZURE_CLIENT_ID` in App Service configuration. Under the hood, App Service calls the managed identity endpoint with an `X-IDENTITY-HEADER` and receives an access token for the requested audience. `Microsoft.Identity.Web` also supports `ClientCredentials` with managed identity when calling downstream APIs from an ASP.NET Core app hosted on Azure. Local development does not use the App Service endpoint — `DefaultAzureCredential` falls back to Visual Studio, Azure CLI, or environment-based service principal credentials.

```csharp
var credential = new DefaultAzureCredential();
var token = await credential.GetTokenAsync(
    new TokenRequestContext(["https://vault.azure.net/.default"]));
// Use token.Token as Bearer token for Key Vault or HTTP clients
```

---

## Q19. Which Azure services commonly use managed identities instead of client secrets?

**Concepts**
- App Service and Azure Functions — access Key Vault, Storage, SQL, Service Bus
- AKS workload identity — federated Kubernetes service accounts to Entra principals
- Azure Automation, Logic Apps, Data Factory — connectors without embedded credentials
- Container Apps and App Service containers — same model as App Service
- Data plane access — RBAC role assignment required in addition to enabling identity

**Answer**

Any Azure compute or integration service that needs to call other Azure APIs or Entra-protected resources commonly supports managed identity as the recommended authentication path, replacing stored connection strings with passwordless access. Azure App Service and Azure Functions use managed identity to access Key Vault secrets, Storage, SQL, Service Bus, and custom APIs. Azure Virtual Machines and VM Scale Sets use extension-based IMDS token retrieval for automation scripts and apps. Azure Kubernetes Service uses workload identity to federate Kubernetes service accounts to Entra service principals. Azure Logic Apps, Data Factory, and Automation connect to connectors and databases without embedded credentials. Azure Container Apps and App Service containers follow the same model for microservices calling Key Vault or internal APIs. Data plane access — reading blobs, querying SQL with Entra auth — still requires RBAC role assignment or database user mapping for the managed identity; enabling identity alone is not sufficient.

---

## Q20. What is MSAL (Microsoft Authentication Library), and how does it differ from the legacy ADAL?

**Concepts**
- MSAL — current Microsoft token acquisition library targeting v2.0 endpoint
- ADAL — deprecated library targeting v1.0 endpoint with resource URIs
- `IPublicClientApplication` and `IConfidentialClientApplication` — MSAL client types
- `Microsoft.Identity.Web` — ASP.NET Core wrapper for MSAL with one-line configuration
- New projects — never add ADAL; migrate existing apps using Microsoft migration guides

**Answer**

MSAL (Microsoft Authentication Library) is the current Microsoft-supported library family for acquiring Entra ID tokens, replacing the deprecated Active Directory Authentication Library (ADAL). MSAL targets the Microsoft identity platform v2.0 endpoint, supports incremental consent, PKCE, and modern flows, and integrates with `Microsoft.Identity.Web` for ASP.NET Core. ADAL used the v1 endpoint and is no longer recommended; MSAL uses v2 with scope-based permissions instead of resource URIs alone. MSAL provides unified public and confidential client types — `IPublicClientApplication` for native/SPA apps and `IConfidentialClientApplication` for server-side apps — with built-in token caching and authority customization for multi-tenant scenarios. `Microsoft.Identity.Web` wraps MSAL for ASP.NET Core with one-line configuration for sign-in, token acquisition, and downstream API invocation. New projects should never add ADAL packages since migration guides map ADAL's `AcquireTokenAsync` patterns to MSAL's account-centric cache model.

---

## Q21. How do you configure an ASP.NET Core web app to sign users in with Microsoft Entra ID using Microsoft.Identity.Web?

**Concepts**
- `Microsoft.Identity.Web` NuGet package
- `AddAuthentication().AddMicrosoftIdentityWebApp()` — wires OIDC and cookie session
- `AzureAd` configuration section — `TenantId`, `ClientId`, `ClientSecret`, `CallbackPath`
- Redirect URI registration in Entra portal matching `CallbackPath`
- `.EnableTokenAcquisitionToCallDownstreamApi()` for calling Graph or custom APIs

**Answer**

Register the web app in Entra ID with a redirect URI, add the `Microsoft.Identity.Web` NuGet package, and bind an `AzureAd` section from configuration to `AddAuthentication().AddMicrosoftIdentityWebApp()`, which wires OpenID Connect for sign-in and token validation. The middleware creates a cookie session after validating the ID token from Entra ID. Store `TenantId`, `ClientId`, `ClientSecret` for confidential web apps, and `CallbackPath` matching the registration in `appsettings.json`. Enable ID tokens and configure redirect URIs in the portal — for example `https://localhost:5001/signin-oidc`. To call Microsoft Graph or a custom API on behalf of the signed-in user, add `.EnableTokenAcquisitionToCallDownstreamApi()` and `.AddMicrosoftGraph()` or `.AddDownstreamApi()`.

```csharp
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();
// Pipeline: UseAuthentication(); UseAuthorization();
```

---

## Q22. How does the `[Authorize]` attribute behave when Microsoft Entra ID is configured as the authentication scheme?

**Concepts**
- MVC/Razor Pages — `[Authorize]` checks authentication cookie, redirects to Entra on miss
- Web API — `[Authorize]` requires valid `Authorization: Bearer` token, returns 401 on miss
- Policy-based authorization — evaluates claims after authentication succeeds
- `[AllowAnonymous]` — bypasses authorization for health checks or public pages
- JWT bearer vs OIDC cookie — different scheme behaviors on the same attribute

**Answer**

When Entra ID sign-in is configured through OpenID Connect, the `[Authorize]` attribute checks whether the current HTTP request has an authenticated `ClaimsPrincipal` — typically established via the authentication cookie created after OIDC sign-in — and returns a challenge redirect to Entra ID if the user is anonymous. For MVC or Razor Pages, unauthenticated users are redirected to `/MicrosoftIdentity/Account/SignIn` or the configured login path. For Web APIs protected with JWT bearer, `[Authorize]` requires a valid `Authorization: Bearer` access token and returns HTTP 401 if the token is missing or invalid since API clients do not support browser redirects. Policy-based authorization via `[Authorize(Policy = "RequireAdminRole")]` evaluates claims from the ID token or access token — such as app roles or group claims — after authentication succeeds. `[AllowAnonymous]` bypasses authorization for specific endpoints such as health checks or public landing pages.

---

## Q23. How do you read user claims (roles, groups, `oid`, `tid`) from an Entra ID token in ASP.NET Core?

**Concepts**
- `HttpContext.User` as `ClaimsPrincipal` — populated after authentication middleware runs
- `oid` — immutable user object ID, prefer as database key over display name or email
- `tid` — tenant identifier, essential in multi-tenant apps for data scoping
- `roles` claim — app roles assigned on enterprise application
- `groups` overage — too many groups for token triggers Graph query for full membership

**Answer**

After authentication, Entra ID claims are available on `HttpContext.User` as a `ClaimsPrincipal`; controller actions inject `ClaimsPrincipal user` or access `User.FindFirst(ClaimTypes.NameIdentifier)` to read individual claim types. The `oid` claim is the immutable user object ID within the tenant — prefer this over display name or email for database keys since email addresses can change. The `tid` claim identifies the tenant that issued the token and is essential in multi-tenant apps to scope data per organization. The `roles` claim contains app roles assigned on the enterprise application and appears when App roles are defined and assigned to users or groups. Group membership claims can be emitted in the token or retrieved from Microsoft Graph if group overage occurs when the user belongs to too many groups for the token size limit. Configure `TokenValidationParameters.RoleClaimType` if roles use a custom claim type; `Microsoft.Identity.Web` maps common claim types automatically.

```csharp
var userId = User.FindFirst("oid")?.Value;
var tenantId = User.FindFirst("tid")?.Value;
var isAdmin = User.IsInRole("Admin");
```

---

## Q24. How do you protect an ASP.NET Core Web API with Azure Entra ID using JWT bearer validation?

**Concepts**
- `AddMicrosoftIdentityWebApi` — configures JWT bearer with Entra discovery
- `AzureAd:Audience` — must match `aud` claim on incoming tokens
- Signing key rotation — downloaded automatically from `jwks_uri` via discovery
- `UseAuthentication()` before `UseAuthorization()` in pipeline
- Multi-tenant APIs — custom issuer validator accepting tokens from trusted tenants

**Answer**

Register the API in Entra ID, expose at least one scope or app role, then configure ASP.NET Core with JWT bearer authentication pointing the authority to your Entra tenant so the middleware downloads signing keys and validates issuer, audience, lifetime, and signature on every request. Controllers decorated with `[Authorize]` reject anonymous calls automatically. Set `AzureAd:Audience` to the Application ID URI or client ID of the API registration — this must match the `aud` claim on incoming tokens. Enable Access tokens in the API registration's Implicit grant and hybrid flows only when needed; modern SPAs use authorization code with PKCE instead of implicit flow. Call `UseAuthentication()` before `UseAuthorization()` in the pipeline. For multi-tenant APIs, configure issuer validation to accept tokens from any tenant you trust, or validate `tid` against an allow list in custom middleware.

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();
```

---

## Q25. What is an App Role versus a group-based authorization check in Entra ID?

**Concepts**
- App roles — API-defined role names in app registration, appear as `roles` claim
- Group claims — Entra security group membership in token `groups` claim or via Graph
- Token size overage — groups claim can exceed limits, triggering Graph query
- App roles preferred for application authorization — explicit, stable, no token bloat
- Groups suited for reusing existing corporate role structures

**Answer**

App roles are application-defined role names such as `Reader` or `Admin` assigned to users or groups on the enterprise application; they appear in the `roles` claim of access tokens issued to that API. Group-based authorization relies on Entra security group membership appearing as `groups` claims in the token, or on checking group membership via Microsoft Graph at runtime when group claims exceed token size limits and an overage claim is emitted. App roles are preferred for application authorization because they are explicit, stable, and do not bloat tokens when users belong to many groups. Group claims can exceed token size limits — Entra emits a `groups` overage claim and expects the API to query Graph for full membership. In ASP.NET Core, map app roles to policies with `options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"))`. Groups suit reusing existing corporate role structures; app roles suit fine-grained, API-specific permissions.

| | App roles | Groups |
|---|---|---|
| Defined by | Your API's app registration | Entra ID directory |
| Token claim | `roles` | `groups` (or overage via Graph) |
| Portability | Specific to your application | Reusable across many apps |
| Admin experience | Assign on enterprise app | Assign users to AD groups |

---

## Q26. How do you expose a custom API scope in an app registration and assign it to a calling application?

**Concepts**
- Application ID URI — base URI for scope names in the API registration
- Expose an API — add scope; consuming app adds API permission referencing it
- Admin consent — required before Entra issues tokens containing the scope
- Full scope string — `api://{api-client-id}/scope-name` in authorize and token requests
- `scp` claim validation — API must confirm expected scope before executing logic

**Answer**

On the API's app registration, set an Application ID URI under Expose an API, add one or more scopes for delegated access or app roles for application access, then on the client app's registration add API permissions referencing those scopes and complete admin consent. Only after consent will Entra ID issue tokens containing the granted scope to that client. The client requests the full scope string such as `api://{api-client-id}/access_as_user` in the authorize and token requests; the API validates the `scp` claim contains the expected scope before executing protected logic. The calling app must use the v2 endpoint and request the full scope string, not just the short name. Integration tests often use a separate test client registration with the same permission grants. Application permissions follow the parallel path under Application permissions with admin consent and the client credentials flow.

---

## Q27. What is the audience (`aud`) claim in an Entra ID access token, and why must your API validate it?

**Concepts**
- `aud` claim — identifies the intended recipient of the access token
- Confused-deputy vulnerability — accepting a token issued for another API
- `TokenValidationParameters.ValidAudience` — server-side enforcement
- ID token `aud` identifies the client app, not the API — do not use for API authorization
- Misconfigured audience — common cause of 401 errors after successful Entra sign-in

**Answer**

The `aud` claim identifies the intended recipient of the access token — typically the Application ID URI or client ID of the API that should accept the token. Your API must validate that `aud` matches its own registered identifier because a token issued for Microsoft Graph or another API in the same tenant is cryptographically valid but not authorized for your endpoints — accepting it would be a confused-deputy vulnerability. Entra ID sets `aud` when the token is minted; you cannot trust the client to send the "right" audience since validation happens server-side in `TokenValidationParameters.ValidAudience`. A token valid for `api://app-a` must be rejected by `api://app-b` even if both APIs share the same tenant and signing keys. ID tokens also carry `aud`, but it identifies the client application that requested sign-in rather than the API — do not use ID tokens to authorize API calls. Misconfigured audience — wrong Application ID URI in API config — is one of the most common causes of 401 errors after otherwise successful Entra sign-in.

---

## Q28. What is a multi-tenant Azure Entra application, and what changes when you support accounts from any organization?

**Concepts**
- Multi-tenant registration — configured to accept sign-in from any Entra organization
- Service principal per customer tenant — created on first admin consent
- `common` or `organizations` authority — replaces single tenant ID in sign-in URL
- `tid` claim validation — developer must scope data by tenant ID
- Admin consent required per customer tenant — SaaS onboarding flow

**Answer**

A multi-tenant application is registered once in your home tenant but configured to accept sign-in from users in any Entra ID organization, and each customer tenant gets its own service principal when an admin consents. Tokens include a `tid` claim identifying which tenant the user belongs to so your app can partition data per organization. The authority URL uses `common` or `organizations` instead of a fixed tenant ID during sign-in so users from any tenant can authenticate. Your API must validate issuers from multiple tenants — or use a custom validator — and must never trust tokens without checking `tid` against your customer registry. Admin consent in each customer tenant is required before that organization's users can use the app; SaaS onboarding flows often redirect customer admins through a consent URL. Data isolation becomes your responsibility: store `tid` and `oid` with every record and filter queries by tenant, since Entra ID does not segregate your application's database.

---
