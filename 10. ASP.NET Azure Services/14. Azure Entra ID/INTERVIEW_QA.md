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

What is Microsoft Entra ID (formerly Azure Active Directory), and how does it differ from on-premises Active Directory?

**Answer:** Microsoft Entra ID is Microsoft's cloud-based identity and access management (IAM) service that authenticates users, issues tokens, and enforces access policies for Microsoft 365, Azure resources, and custom applications. It is not a lift-and-shift of on-premises Active Directory (AD) — it is a purpose-built cloud directory designed for internet-facing protocols like OAuth 2.0 and OpenID Connect (OIDC), not for traditional Kerberos and LDAP domain joins alone.

- On-premises AD is optimized for Windows domain-joined machines inside a corporate network, using Kerberos tickets and Group Policy for workstation management.
- Entra ID centers on user accounts, groups, app registrations, and conditional access policies that apply across cloud and hybrid scenarios without requiring every client to sit on the corporate LAN.
- Many organizations run both: Entra Connect synchronizes on-premises AD users and groups into Entra ID so the same identities can sign in to cloud apps while legacy on-premises apps continue using domain controllers.
- Entra ID also adds cloud-native capabilities — multi-factor authentication (MFA), passwordless sign-in, and risk-based Conditional Access — that are not part of classic AD by default.

---

## Q2. What is an Azure Entra tenant, and what core identity objects does it contain?

What is an Azure Entra tenant, and what core identity objects does it contain?

**Answer:** An Azure Entra tenant is an isolated instance of Microsoft Entra ID tied to one organization (or one Microsoft cloud subscription boundary), identified by a unique Directory (tenant) ID and a default domain such as `contoso.onmicrosoft.com`. Every user, application, and policy you manage in Entra ID lives inside exactly one tenant unless you explicitly configure cross-tenant or multi-tenant access.

- **Users** represent people (employees, guests) who authenticate and receive tokens; guest users from other tenants are invited via B2B collaboration.
- **Groups** collect users for role assignment, license assignment, and app access — both security groups and Microsoft 365 groups are common.
- **App registrations** define how applications authenticate and what permissions they request; each registration has a corresponding **service principal** in the tenant directory.
- **Enterprise applications** are the tenant-local instance of a registered app, where administrators grant consent and assign users or groups to the app.
- **Conditional Access policies** attach to users, apps, and signals (location, device compliance, risk level) to allow, block, or require step-up authentication.

---

## Q3. What is the relationship between OAuth 2.0 and OpenID Connect (OIDC) in Azure Entra ID authentication?

What is the relationship between OAuth 2.0 and OpenID Connect (OIDC) in Azure Entra ID authentication?

**Answer:** OAuth 2.0 is an authorization framework that lets a client obtain an access token to call a protected API on behalf of a user or itself, without sharing the user's password with that client. OpenID Connect (OIDC) is an identity layer built on top of OAuth 2.0 that adds an ID token — a signed JWT (JSON Web Token) carrying authentication claims such as who signed in and when — so applications can establish a login session, not just API access.

- In Entra ID, the same token endpoint can return both an access token (OAuth 2.0 — for calling Microsoft Graph or your own API) and an ID token (OIDC — for sign-in).
- OAuth 2.0 alone does not standardize user identity claims; OIDC fills that gap with standard claims like `sub`, `iss`, and `aud` on the ID token.
- ASP.NET Core apps that "sign users in with Microsoft" use OIDC middleware (`AddOpenIdConnect`) to validate the ID token and create a `ClaimsPrincipal`; API protection middleware (`AddJwtBearer`) validates access tokens.
- Microsoft Entra ID implements OAuth 2.0 and OIDC per the Microsoft identity platform v2.0 endpoint (`login.microsoftonline.com/{tenant}/oauth2/v2.0/...`).

---

## Q4. What are delegated permissions versus application permissions in Azure Entra ID?

What are delegated permissions versus application permissions in Azure Entra ID?

**Answer:** Delegated permissions allow an application to act on behalf of a signed-in user, limited to what that user is allowed to do in the target resource. Application permissions allow the application itself to authenticate (no user context) and access data or operations granted to the app identity, typically used by background services or daemons.

| | Delegated | Application |
|---|---|---|
| User context | Required — a user must sign in | None — app uses its own identity |
| Typical flow | Authorization code, OBO | Client credentials |
| Consent | User or admin grants scopes like `User.Read` | Admin consent required for app-only access |
| Risk if misused | Limited by user's own rights | Can access all data the permission allows across the tenant |

- Delegated permissions appear as scopes in access tokens (for example `scp` or `scope` claim); application permissions appear as roles (the `roles` claim on the token).
- Admin consent is often required for both types in enterprise tenants, but application permissions always require an administrator because there is no user boundary to limit abuse.
- Choosing the wrong type — requesting application permissions when a user-delegated call suffices — is a common over-permissioning mistake.

---

## Chapter 2 — App Registrations & Service Principals

---

## Q5. What is an app registration in Azure Entra ID, and why do you create one?

What is an app registration in Azure Entra ID, and why do you create one?

**Answer:** An app registration is a global definition in Entra ID that describes your application's identity — its name, redirect URIs, supported OAuth flows, requested permissions, and credentials (secrets or certificates). You create one so Entra ID knows which client is requesting tokens, which URLs are allowed to receive authorization codes, and which APIs the app may access.

- Every registered app receives an Application (client) ID — a GUID that clients include in token requests to identify themselves.
- Redirect URIs are a critical security control: Entra ID only returns authorization codes to URIs listed on the registration, preventing code interception by arbitrary sites.
- For a custom Web API, the registration also defines exposed scopes (delegated) and app roles (application) that other apps request during consent.
- Registrations are created in the Entra admin center under **App registrations**, via Azure CLI, or through infrastructure-as-code tools like Bicep or Terraform.

---

## Q6. What is a service principal, and how does it relate to an app registration?

What is a service principal, and how does it relate to an app registration?

**Answer:** A service principal is the local representation of an application inside a specific Entra tenant — it is the security identity that actually receives tokens, holds assigned roles, and appears in audit logs for that tenant. An app registration is the blueprint; when the app is used in a tenant (including its home tenant), Entra ID creates a service principal object that links back to that registration.

- One app registration can have service principals in many tenants — this is how multi-tenant SaaS apps work: one registration, many enterprise instances.
- Administrators assign users and groups to the service principal (Enterprise Application blade) to control who may use the app.
- Admin consent for API permissions is recorded on the service principal, not merely on the registration object.
- Managed identities are a special type of service principal created and managed automatically by Azure for Azure resources — no manual secret management on the registration.

---

## Q7. What is the difference between the Application (client) ID and the Directory (tenant) ID?

What is the difference between the Application (client) ID and the Directory (tenant) ID?

**Answer:** The Application (client) ID uniquely identifies your app registration across the Microsoft identity platform — every token request, MSAL configuration, and `Audience`/`ClientId` setting references this GUID. The Directory (tenant) ID uniquely identifies the Entra ID organization (tenant) that issued the token or hosts the user accounts you authenticate against.

- Client ID answers "which application is calling?" — it is the same in every tenant where your multi-tenant app is installed.
- Tenant ID answers "which organization's directory issued this token?" — it appears in authority URLs like `https://login.microsoftonline.com/{tenantId}` and in the `tid` claim inside tokens.
- Single-tenant apps hard-code their own tenant ID in the authority so only users from that organization can sign in.
- Multi-tenant or "any organizational account" apps use `common` or `organizations` instead of a specific tenant ID in the authority, and validate the `tid` claim at runtime if they need to restrict access.

---

## Q8. How do client secrets differ from certificates for app authentication, and when would you choose each?

How do client secrets differ from certificates for app authentication, and when would you choose each?

**Answer:** Both client secrets and certificates prove the application's identity when it requests tokens without a user (client credentials flow) or when exchanging codes at the token endpoint. A client secret is a shared string stored by the app; a certificate uses public-key cryptography where Entra ID holds the public key and the app holds the private key to sign authentication requests.

| | Client secret | Certificate |
|---|---|---|
| Format | Random string | X.509 cert with private key |
| Rotation | Manual; easy to leak in config | Supports auto-rotation via Key Vault |
| Proof mechanism | Send secret in token request | Sign JWT assertion with private key |
| Typical use | Dev/test, quick prototypes | Production, CI/CD, long-lived daemons |

- Secrets appear in source control leaks and pipeline logs more often than private keys stored in Azure Key Vault, so production guidance favors certificates or managed identities.
- Certificate-based client authentication uses a signed client assertion (`client_assertion` parameter) instead of sending a raw secret over the wire.
- Managed identities eliminate both options for apps running on Azure — Azure provisions the identity and handles credential rotation automatically (see Q16).

---

## Q9. What is an Enterprise Application in Azure Entra ID, and how does it differ from an App Registration?

What is an Enterprise Application in Azure Entra ID, and how does it differ from an App Registration?

**Answer:** An Enterprise Application is the tenant-specific instance of an application that administrators manage for access control, single sign-on (SSO) configuration, and consent — it corresponds to the service principal in the directory. An App Registration is the developer-facing definition of the app's identity, redirect URIs, credentials, and API definitions — it may exist once while Enterprise Applications appear in every tenant where the app is used.

- Developers own app registrations (client ID, secrets, exposed API scopes); IT admins own enterprise applications (assign users, grant tenant-wide consent, configure Conditional Access).
- When you "add an app" from the gallery (for example Salesforce or a third-party SaaS), you get an enterprise application and service principal but you may not have access to the publisher's app registration.
- For your own API, you create the app registration first, then assign users to its enterprise application entry if you want to restrict who can obtain tokens.
- The Enterprise Applications blade is also where you find sign-in logs and audit activity for that app in your tenant.

---

## Chapter 3 — OAuth 2.0 & OpenID Connect Flows

---

## Q10. Describe the authorization code flow with PKCE and why it is the recommended flow for web and mobile apps.

Describe the authorization code flow with PKCE and why it is the recommended flow for web and mobile apps.

**Answer:** The authorization code flow redirects the user to Entra ID to sign in; after successful authentication, Entra ID redirects back to the app with a short-lived authorization code that the app exchanges at the token endpoint for access and ID tokens. PKCE (Proof Key for Code Exchange) adds a dynamically generated `code_verifier` and hashed `code_challenge` so that even if an attacker intercepts the authorization code, they cannot exchange it without the original verifier — which is essential for public clients that cannot store a client secret.

1. The client generates a random `code_verifier` and sends its SHA-256 hash as `code_challenge` when starting the authorize request.
2. The user authenticates at Entra ID and is redirected to the registered redirect URI with an authorization `code`.
3. The client POSTs to the token endpoint with the `code`, `code_verifier`, client ID, and redirect URI.
4. Entra ID validates the verifier against the original challenge and returns access and ID tokens.
5. The web app establishes a session (often a cookie) and uses the access token to call downstream APIs.

- Confidential server-side web apps may also use a client secret in step 3, but PKCE is still recommended for defense in depth.
- Native and single-page applications (SPAs) must use PKCE because they cannot securely embed a client secret.
- MSAL handles PKCE generation and the code exchange automatically when you call `AcquireTokenInteractive` or equivalent methods.

---

## Q11. What is the client credentials flow, and when should a .NET background service use it?

What is the client credentials flow, and when should a .NET background service use it?

**Answer:** The client credentials flow lets an application authenticate as itself — using its client ID plus a secret, certificate, or managed identity — and receive an access token with application permissions, with no signed-in user involved. A .NET background service such as a worker, Azure Function, or scheduled job should use this flow when it must call Microsoft Graph, a custom API, or another protected resource on a timer or message trigger without human interaction.

- The service requests a token from `https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token` with `grant_type=client_credentials` and the target scope (for example `https://graph.microsoft.com/.default`).
- The returned access token carries application roles, not delegated user scopes, so the API must authorize based on app roles and the service must be granted only the minimum application permissions required.
- Prefer managed identity over client secrets when the service runs on Azure App Service, Azure Functions, Azure Container Apps, or similar — MSAL's `DefaultAzureCredential` or `ManagedIdentityCredential` acquires tokens without stored secrets.
- Do not use client credentials when the operation should respect the current user's permissions — use delegated flows or on-behalf-of instead.

---

## Q12. What is the on-behalf-of (OBO) flow, and what problem does it solve in multi-tier APIs?

What is the on-behalf-of (OBO) flow, and what problem does it solve in multi-tier APIs?

**Answer:** The on-behalf-of (OBO) flow allows a middle-tier API that receives a user's access token to exchange it for a new access token to call a downstream API, still acting as that same user. It solves the problem where a web API cannot simply forward the incoming bearer token to another API — downstream APIs often require a token issued specifically for their audience, and forwarding tokens violates least-privilege and audience validation rules.

1. The client obtains a user access token for the middle API (for example scope `api://middle-api/access`).
2. The middle API receives the request, validates the incoming token, and calls the Entra token endpoint with the OBO grant, passing the user's token as the `assertion`.
3. Entra ID returns a new access token scoped for the downstream API (for example `api://downstream-api/read`).
4. The middle API calls the downstream API with the new token.

- Both APIs must be registered in Entra ID, and the middle API needs delegated permission to the downstream API with admin consent.
- MSAL provides `AcquireTokenOnBehalfOf` in `Microsoft.Identity.Web` and `IConfidentialClientApplication` for this exchange.
- OBO preserves user context and auditing — downstream actions appear as the user, not as the middle tier's application identity.

---

## Q13. What are scopes, and how do they relate to API permissions in app registrations?

What are scopes, and how do they relate to API permissions in app registrations?

**Answer:** A scope is a string identifier that names a delegated permission to perform a specific action on a resource — for example `User.Read` on Microsoft Graph or a custom scope like `api://my-api/orders.read` on your own API. In app registrations, you define scopes under **Expose an API**; consuming apps request those scopes in the `scope` parameter during login or token acquisition, and administrators grant them during consent.

- Full scope format for custom APIs is typically `api://{client-id}/{scope-name}` or a configured Application ID URI plus the scope name.
- The `.default` scope is special: for client credentials it means "all application permissions granted to this app"; for delegated flows it can mean the static permissions configured on the registration.
- Granted scopes appear in the access token as the `scp` claim (v2 tokens) or in the `roles` claim for application permissions.
- APIs should validate that the token contains the scope or role required for each endpoint rather than accepting any valid token from the tenant.

---

## Q14. What is the `/.well-known/openid-configuration` endpoint, and what information does it expose?

What is the `/.well-known/openid-configuration` endpoint, and what information does it expose?

**Answer:** The OpenID Connect discovery document is a JSON metadata file published by Entra ID at `https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration` that tells clients and APIs where to send authentication and token requests and where to fetch signing keys. ASP.NET Core and MSAL use it to auto-configure issuer validation, authorization endpoints, and JSON Web Key Set (JWKS) URLs without hard-coding every URL per environment.

- Key fields include `issuer`, `authorization_endpoint`, `token_endpoint`, `jwks_uri`, and supported response types and scopes.
- The `jwks_uri` points to the public keys used to validate token signatures — APIs fetch this to support key rotation without redeployment.
- Using discovery is preferred over manually setting `Authority` and `MetadataAddress` inconsistently across services.
- For multi-tenant APIs, the issuer and signing keys must be validated against the tenant that issued the token, which is why production APIs often use `ValidateIssuer` with an issuer validator that accepts known tenant issuers.

---

## Q15. What is token caching, and why is it important when using MSAL in production?

What is token caching, and why is it important when using MSAL in production?

**Answer:** Token caching is MSAL's mechanism for storing access and refresh tokens (and associated metadata) so the library can return a still-valid access token from memory or persistent storage instead of calling Entra ID on every HTTP request. Without caching, every API call would trigger a network round trip to the token endpoint, quickly hit throttling limits, and add unnecessary latency.

- MSAL caches access tokens until near expiry (based on the `exp` claim) and uses refresh tokens for silent renewal in confidential and public client flows where refresh tokens are issued.
- For ASP.NET Core web apps, `Microsoft.Identity.Web` integrates distributed caches (Redis, SQL Server) so multiple server instances share the same token cache for a user session.
- Client credentials tokens are also cached in memory keyed by tenant and scope — worker services benefit significantly from this default behavior.
- Cache serialization must be encrypted at rest when refresh tokens are stored, because a stolen refresh token allows prolonged access as the user or application.

---

## Chapter 4 — Managed Identities

---

## Q16. What is a managed identity in Azure, and what problem does it eliminate?

What is a managed identity in Azure, and what problem does it eliminate?

**Answer:** A managed identity is an Entra ID service principal automatically created and lifecycle-managed by Azure for a specific Azure resource, giving that resource an identity it can use to authenticate to other Azure services and Microsoft APIs without storing client secrets or certificates in configuration. It eliminates secret sprawl, rotation toil, and the risk of credentials leaking from `appsettings.json`, environment variables, or source control.

- Azure handles creation, renewal, and deletion of the identity credentials — your code only requests tokens via the local identity endpoint or Azure SDK credential types.
- The identity appears as an enterprise application/service principal in Entra ID, where administrators assign roles (for example Storage Blob Data Contributor) or API permissions like any other app.
- Managed identities can authenticate to Azure Resource Manager, Azure Key Vault, Azure Storage, Azure SQL, Microsoft Graph (with assigned permissions), and custom APIs that accept Entra tokens.
- They apply to Azure-hosted workloads only — local development typically uses Azure CLI or Visual Studio credentials via `DefaultAzureCredential`, not a managed identity.

---

## Q17. What is the difference between system-assigned and user-assigned managed identities?

What is the difference between system-assigned and user-assigned managed identities?

**Answer:** A system-assigned managed identity is tied to exactly one Azure resource — it is created when you enable identity on that resource and is deleted when the resource is deleted. A user-assigned managed identity is created as a standalone Azure resource that you can attach to zero, one, or many other resources, and it persists independently of any single compute instance.

| | System-assigned | User-assigned |
|---|---|---|
| Lifecycle | Bound to one resource | Independent resource |
| Sharing | Cannot share across resources | Can attach to multiple VMs, App Services, etc. |
| Use case | Single app on one host | Multiple scalesets sharing one identity |
| Naming | Azure-generated | You choose the name |

- User-assigned identities are useful when several App Service slots, VM scale set instances, or containers must present the same identity to Key Vault or a database.
- Both types use the same token acquisition endpoint on the host (`IDENTITY_ENDPOINT` / `IMDS` at `169.254.169.254` on VMs).
- In .NET, `DefaultAzureCredential` resolves managed identity in Azure automatically; you specify a user-assigned client ID when multiple identities are available on the same resource.

---

## Q18. How does a .NET application running on Azure App Service acquire a token using a managed identity?

How does a .NET application running on Azure App Service acquire a token using a managed identity?

**Answer:** When managed identity is enabled on App Service, the platform exposes a local REST endpoint and environment variables that the Azure Identity library uses to obtain Entra ID tokens without any secret in your code. In .NET you typically use `DefaultAzureCredential` or `ManagedIdentityCredential` from the `Azure.Identity` package and call `GetTokenAsync` with the target resource scope.

```csharp
var credential = new DefaultAzureCredential();
var token = await credential.GetTokenAsync(
    new TokenRequestContext(["https://vault.azure.net/.default"]));
// Use token.Token as Bearer token for Key Vault or HTTP clients
```

- For user-assigned identity, pass `ManagedIdentityClientId` in `DefaultAzureCredentialOptions` or set `AZURE_CLIENT_ID` in App Service configuration.
- Under the hood, App Service calls the managed identity endpoint with an `X-IDENTITY-HEADER` and receives an access token for the requested audience.
- `Microsoft.Identity.Web` also supports `ClientCredentials` with managed identity when calling downstream APIs from an ASP.NET Core app hosted on Azure.
- Local development does not use the App Service endpoint — `DefaultAzureCredential` falls back to Visual Studio, Azure CLI, or environment-based service principal credentials.

---

## Q19. Which Azure services commonly use managed identities instead of client secrets?

Which Azure services commonly use managed identities instead of client secrets?

**Answer:** Any Azure compute or integration service that needs to call other Azure APIs or Entra-protected resources commonly supports managed identity as the recommended authentication path, replacing stored connection strings with passwordless access. The pattern appears across PaaS hosting, data services, and automation.

- **Azure App Service and Azure Functions** — access Key Vault secrets, Storage, SQL, Service Bus, and custom APIs.
- **Azure Virtual Machines and VM Scale Sets** — extension-based IMDS token retrieval for automation scripts and apps.
- **Azure Kubernetes Service (AKS)** — workload identity federates Kubernetes service accounts to Entra service principals.
- **Azure Logic Apps, Data Factory, and Automation** — connect to connectors and databases without embedded credentials.
- **Azure Container Apps and App Service containers** — same model as App Service for microservices calling Key Vault or internal APIs.

- Data plane access (reading blobs, querying SQL with Entra auth) still requires RBAC role assignment or database user mapping for the managed identity — enabling identity alone is not sufficient.

---

## Chapter 5 — MSAL & ASP.NET Core Integration

---

## Q20. What is MSAL (Microsoft Authentication Library), and how does it differ from the legacy ADAL?

What is MSAL (Microsoft Authentication Library), and how does it differ from the legacy ADAL?

**Answer:** MSAL (Microsoft Authentication Library) is the current Microsoft-supported library family for acquiring Entra ID tokens from .NET, JavaScript, Python, and other platforms, replacing the deprecated Active Directory Authentication Library (ADAL). MSAL targets the Microsoft identity platform v2.0 endpoint, supports incremental consent, PKCE, and modern flows, and integrates with `Microsoft.Identity.Web` for ASP.NET Core.

- ADAL used the v1 endpoint (`login.microsoftonline.com/{tenant}/oauth2/authorize`) and is no longer recommended; MSAL uses v2 (`/oauth2/v2.0/authorize`) with scope-based permissions instead of resource URIs alone.
- MSAL provides unified public and confidential client types (`IPublicClientApplication`, `IConfidentialClientApplication`) with built-in token caching and authority customization for multi-tenant scenarios.
- `Microsoft.Identity.Web` wraps MSAL for ASP.NET Core with one-line configuration for sign-in, token acquisition to call APIs, and downstream API invocation.
- New projects should never add ADAL packages — migration guides map ADAL's `AcquireTokenAsync` patterns to MSAL's account-centric cache model.

---

## Q21. How do you configure an ASP.NET Core web app to sign users in with Microsoft Entra ID using Microsoft.Identity.Web?

How do you configure an ASP.NET Core web app to sign users in with Microsoft Entra ID using Microsoft.Identity.Web?

**Answer:** You register the web app in Entra ID with a redirect URI, add the `Microsoft.Identity.Web` NuGet package, and bind an `AzureAd` section from configuration to `AddAuthentication().AddMicrosoftIdentityWebApp()`, which wires OpenID Connect for sign-in and token validation. The middleware creates a cookie session after validating the ID token from Entra ID.

```csharp
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();
// Pipeline: UseAuthentication(); UseAuthorization();
```

- `appsettings.json` stores `TenantId`, `ClientId`, `ClientSecret` (for confidential web apps), and `CallbackPath` matching the registration.
- Enable ID tokens and configure redirect URIs in the portal (for example `https://localhost:5001/signin-oidc`).
- To call Microsoft Graph or a custom API on behalf of the signed-in user, add `.EnableTokenAcquisitionToCallDownstreamApi()` and `.AddMicrosoftGraph()` or `.AddDownstreamApi()`.
- For Blazor WebAssembly, use the WASM-specific packages; server-side Blazor uses the same middleware pattern as MVC or Razor Pages.

---

## Q22. How does the `[Authorize]` attribute behave when Microsoft Entra ID is configured as the authentication scheme?

How does the `[Authorize]` attribute behave when Microsoft Entra ID is configured as the authentication scheme?

**Answer:** When Entra ID sign-in is configured through OpenID Connect, the `[Authorize]` attribute checks whether the current HTTP request has an authenticated `ClaimsPrincipal` — typically established via the authentication cookie created after OIDC sign-in — and returns a challenge redirect to Entra ID if the user is anonymous. It does not by itself validate a bearer access token on API endpoints unless JWT bearer is the configured scheme for that controller or policy.

- For MVC or Razor Pages, `[Authorize]` relies on cookie authentication backed by the OIDC middleware; unauthenticated users are redirected to `/MicrosoftIdentity/Account/SignIn` or the configured login path.
- For Web APIs protected with JWT bearer, `[Authorize]` requires a valid `Authorization: Bearer` access token and returns HTTP 401 if the token is missing or invalid.
- Policy-based authorization (`[Authorize(Policy = "RequireAdminRole")]`) evaluates claims from the ID token or access token — for example app roles or group claims — after authentication succeeds.
- `[AllowAnonymous]` bypasses authorization for specific endpoints such as health checks or public landing pages.

---

## Q23. How do you read user claims (roles, groups, `oid`, `tid`) from an Entra ID token in ASP.NET Core?

How do you read user claims (roles, groups, `oid`, `tid`) from an Entra ID token in ASP.NET Core?

**Answer:** After authentication, Entra ID claims are available on `HttpContext.User` as a `ClaimsPrincipal`; controller actions inject `ClaimsPrincipal user` or access `User.FindFirst(ClaimTypes.NameIdentifier)` to read individual claim types. The object ID (`oid`) identifies the user in Entra ID, the tenant ID (`tid`) identifies their organization, and roles or groups determine authorization.

- `oid` — immutable user object ID within the tenant; prefer this over display name or email for database keys.
- `tid` — tenant that issued the token; essential in multi-tenant apps to scope data per organization.
- `roles` — app roles assigned on the enterprise application; appear when **App roles** are defined and assigned to users or groups.
- `groups` — group membership claims can be emitted in the token or retrieved from Microsoft Graph if group overage occurs (too many groups for the token).
- Configure `TokenValidationParameters.RoleClaimType` if roles use a custom claim type; `Microsoft.Identity.Web` maps common claim types automatically.

```csharp
var userId = User.FindFirst("oid")?.Value;
var tenantId = User.FindFirst("tid")?.Value;
var isAdmin = User.IsInRole("Admin");
```

---

## Chapter 6 — Protecting APIs

---

## Q24. How do you protect an ASP.NET Core Web API with Azure Entra ID using JWT bearer validation?

How do you protect an ASP.NET Core Web API with Azure Entra ID using JWT bearer validation?

**Answer:** Register the API in Entra ID, expose at least one scope or app role, then configure ASP.NET Core with JWT bearer authentication pointing the authority to your Entra tenant so the middleware downloads signing keys and validates issuer, audience, lifetime, and signature on every request. Controllers decorated with `[Authorize]` reject anonymous calls automatically.

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();
```

- Set `AzureAd:Audience` to the Application ID URI or client ID of the API registration — this must match the `aud` claim on incoming tokens.
- Enable **Access tokens** in the API registration's **Implicit grant and hybrid flows** only when needed; modern SPAs use authorization code with PKCE instead of implicit flow.
- Call `UseAuthentication()` before `UseAuthorization()` in the pipeline.
- For multi-tenant APIs, configure issuer validation to accept tokens from any tenant you trust, or validate `tid` against an allow list in custom middleware.

---

## Q25. What is an App Role versus a group-based authorization check in Entra ID?

What is an App Role versus a group-based authorization check in Entra ID?

**Answer:** App roles are application-defined role names (such as `Reader` or `Admin`) assigned to users or groups on the enterprise application; they appear in the `roles` claim of access tokens issued to that API. Group-based authorization relies on Entra security group membership appearing as `groups` claims in the token, or on checking group membership via Microsoft Graph at runtime.

| | App roles | Groups |
|---|---|---|
| Defined by | Your API's app registration | Entra ID directory |
| Token claim | `roles` | `groups` (or overage via Graph) |
| Portability | Specific to your application | Reusable across many apps |
| Admin experience | Assign on enterprise app | Assign users to AD groups |

- App roles are preferred for application authorization because they are explicit, stable, and do not bloat tokens when users belong to many groups.
- Group claims can exceed token size limits — Entra emits a `groups` overage claim and expects the API to query Graph for full membership.
- In ASP.NET Core, map app roles to policies: `options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));`.
- Groups suit reusing existing corporate role structures; app roles suit fine-grained, API-specific permissions.

---

## Q26. How do you expose a custom API scope in an app registration and assign it to a calling application?

How do you expose a custom API scope in an app registration and assign it to a calling application?

**Answer:** On the API's app registration, set an Application ID URI under **Expose an API**, add one or more scopes (for delegated access) or app roles (for application access), then on the client app's registration add API permissions referencing those scopes and complete admin consent. Only after consent will Entra ID issue tokens containing the granted scope to that client.

1. API registration → **Expose an API** → set `api://{client-id}` or custom URI → **Add a scope** (for example `access_as_user`).
2. Client registration → **API permissions** → **Add a permission** → **My APIs** → select the API → choose delegated scope.
3. Grant admin consent for the tenant (button on API permissions or via PowerShell/Graph).
4. Client requests `scope=api://{api-client-id}/access_as_user` in the authorize and token requests.
5. API validates the `scp` claim contains the expected scope before executing protected logic.

- The calling app must use the v2 endpoint and request the full scope string, not just the short name.
- Integration tests often use a separate test client registration with the same permission grants.
- Application permissions follow the parallel path under **Application permissions** with admin consent and the client credentials flow.

---

## Q27. What is the audience (`aud`) claim in an Entra ID access token, and why must your API validate it?

What is the audience (`aud`) claim in an Entra ID access token, and why must your API validate it?

**Answer:** The `aud` claim identifies the intended recipient of the access token — typically the Application ID URI or client ID of the API that should accept the token. Your API must validate that `aud` matches its own registered identifier because a token issued for Microsoft Graph or another API in the same tenant is cryptographically valid but not authorized for your endpoints — accepting it would be a confused-deputy vulnerability.

- Entra ID sets `aud` when the token is minted; you cannot trust the client to send the "right" audience — validation happens server-side in `TokenValidationParameters.ValidAudience`.
- A token valid for `api://app-a` must be rejected by `api://app-b` even if both APIs share the same tenant and signing keys.
- ID tokens also carry `aud`, but it identifies the client application that requested sign-in, not the API — do not use ID tokens to authorize API calls.
- Misconfigured audience (wrong Application ID URI in API config) is one of the most common causes of 401 errors after otherwise successful Entra sign-in.

---

## Chapter 7 — Multi-Tenant & Common Mistakes

---

## Q28. What is a multi-tenant Azure Entra application, and what changes when you support accounts from any organization?

What is a multi-tenant Azure Entra application, and what changes when you support accounts from any organization?

**Answer:** A multi-tenant application is registered once in your home tenant but configured to accept sign-in from users in any Entra ID organization (or optionally any Microsoft account, depending on supported account types). Each customer tenant gets its own service principal when an admin consents, and tokens include a `tid` claim identifying which tenant the user belongs to so your app can partition data per organization.

- Supported account types are set on the app registration: single tenant, any organizational directory, or personal Microsoft accounts plus work accounts.
- Authority URL uses `common` or `organizations` instead of a fixed tenant ID during sign-in so users from any tenant can authenticate.
- Your API must validate issuers from multiple tenants (or use a custom validator) and must never trust tokens without checking `tid` against your customer registry.
- Admin consent in each customer tenant is required before that organization's users can use the app — SaaS onboarding flows often redirect customer admins through a consent URL.
- Data isolation becomes your responsibility: store `tid` (and `oid`) with every record and filter queries by tenant; Entra ID does not segregate your application's database.

---
