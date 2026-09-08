# Azure Key Vault — Interview Q&A
> 27 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure Key Vault, and what three categories of cryptographic material does it store?](#q1-what-is-azure-key-vault-and-what-three-categories-of-cryptographic-material-does-it-store)
2. [Q2. What is the difference between a Key Vault secret, a Key Vault key, and a Key Vault certificate?](#q2-what-is-the-difference-between-a-key-vault-secret-a-key-vault-key-and-a-key-vault-certificate)
3. [Q3. Why should production ASP.NET Core applications store sensitive configuration in Azure Key Vault instead of in `appsettings.json` or plain environment variables alone?](#q3-why-should-production-aspnet-core-applications-store-sensitive-configuration-in-azure-key-vault-instead-of-in-appsettingsjson-or-plain-environment-variables-alone)
4. [Q4. What are soft delete and purge protection in Azure Key Vault, and why do they matter for production vaults?](#q4-what-are-soft-delete-and-purge-protection-in-azure-key-vault-and-why-do-they-matter-for-production-vaults)
5. [Q5. What is the difference between a standard Key Vault and a Key Vault with a Hardware Security Module (HSM) backing?](#q5-what-is-the-difference-between-a-standard-key-vault-and-a-key-vault-with-a-hardware-security-module-hsm-backing)
6. [Q6. How do you integrate Azure Key Vault into an ASP.NET Core application's configuration pipeline?](#q6-how-do-you-integrate-azure-key-vault-into-an-aspnet-core-applications-configuration-pipeline)
7. [Q7. Which NuGet packages are required to use Azure Key Vault as a configuration provider in ASP.NET Core?](#q7-which-nuget-packages-are-required-to-use-azure-key-vault-as-a-configuration-provider-in-aspnet-core)
8. [Q8. How are Key Vault secret names mapped to `IConfiguration` keys, and what naming convention should you use for nested settings such as `ConnectionStrings:Default`?](#q8-how-are-key-vault-secret-names-mapped-to-iconfiguration-keys-and-what-naming-convention-should-you-use-for-nested-settings-such-as-connectionstringsdefault)
9. [Q9. When `AddAzureKeyVault` is called in `Program.cs`, at what point are secrets loaded, and does the running application automatically pick up rotated secrets?](#q9-when-addazurekeyvault-is-called-in-programcs-at-what-point-are-secrets-loaded-and-does-the-running-application-automatically-pick-up-rotated-secrets)
10. [Q10. How does Azure Key Vault fit into the ASP.NET Core configuration provider hierarchy relative to `appsettings.json`, user secrets, and environment variables?](#q10-how-does-azure-key-vault-fit-into-the-aspnet-core-configuration-provider-hierarchy-relative-to-appsettingsjson-user-secrets-and-environment-variables)
11. [Q11. What is Azure managed identity, and how does an ASP.NET Core app running on Azure App Service authenticate to Key Vault without storing credentials?](#q11-what-is-azure-managed-identity-and-how-does-an-aspnet-core-app-running-on-azure-app-service-authenticate-to-key-vault-without-storing-credentials)
12. [Q12. What is `DefaultAzureCredential`, and in what order does it attempt authentication methods?](#q12-what-is-defaultazurecredential-and-in-what-order-does-it-attempt-authentication-methods)
13. [Q13. What is the difference between a system-assigned managed identity and a user-assigned managed identity when granting Key Vault access?](#q13-what-is-the-difference-between-a-system-assigned-managed-identity-and-a-user-assigned-managed-identity-when-granting-key-vault-access)
14. [Q14. Why is storing a Key Vault client secret in `appsettings.json` or environment variables considered an anti-pattern?](#q14-why-is-storing-a-key-vault-client-secret-in-appsettingsjson-or-environment-variables-considered-an-anti-pattern)
15. [Q15. What are Key Vault access policies, and what permissions can they grant on secrets, keys, and certificates?](#q15-what-are-key-vault-access-policies-and-what-permissions-can-they-grant-on-secrets-keys-and-certificates)
16. [Q16. What is Azure Role-Based Access Control (RBAC) for Key Vault, and how does the permission model differ from legacy access policies?](#q16-what-is-azure-role-based-access-control-rbac-for-key-vault-and-how-does-the-permission-model-differ-from-legacy-access-policies)
17. [Q17. Which authorization model does Microsoft recommend for new Key Vault deployments, and can both models be active on the same vault?](#q17-which-authorization-model-does-microsoft-recommend-for-new-key-vault-deployments-and-can-both-models-be-active-on-the-same-vault)
18. [Q18. Which built-in Azure RBAC roles are commonly assigned to an ASP.NET Core application's managed identity for read-only secret access?](#q18-which-built-in-azure-rbac-roles-are-commonly-assigned-to-an-aspnet-core-applications-managed-identity-for-read-only-secret-access)
19. [Q19. What does least-privilege access mean in the context of Key Vault, and why should an app identity not receive blanket `get` permission on all secrets?](#q19-what-does-least-privilege-access-mean-in-the-context-of-key-vault-and-why-should-an-app-identity-not-receive-blanket-get-permission-on-all-secrets)
20. [Q20. How does secret versioning work in Azure Key Vault, and how does versioning support zero-downtime rotation?](#q20-how-does-secret-versioning-work-in-azure-key-vault-and-how-does-versioning-support-zero-downtime-rotation)
21. [Q21. What strategies can you use to rotate a Key Vault secret consumed by a running ASP.NET Core application without restarting every instance?](#q21-what-strategies-can-you-use-to-rotate-a-key-vault-secret-consumed-by-a-running-aspnet-core-application-without-restarting-every-instance)
22. [Q22. How do you load an HTTPS certificate from Azure Key Vault for Kestrel in ASP.NET Core?](#q22-how-do-you-load-an-https-certificate-from-azure-key-vault-for-kestrel-in-aspnet-core)
23. [Q23. What is `ProtectKeysWithAzureKeyVault` in the Data Protection API, and how does it differ from storing application settings as Key Vault secrets?](#q23-what-is-protectkeyswithazurekeyvault-in-the-data-protection-api-and-how-does-it-differ-from-storing-application-settings-as-key-vault-secrets)
24. [Q24. How should developers authenticate to Key Vault during local development on a workstation?](#q24-how-should-developers-authenticate-to-key-vault-during-local-development-on-a-workstation)
25. [Q25. How do Azure DevOps variable groups integrate with Azure Key Vault for pipeline secret injection?](#q25-how-do-azure-devops-variable-groups-integrate-with-azure-key-vault-for-pipeline-secret-injection)
26. [Q26. What monitoring and audit capabilities does Azure Key Vault provide, and why are they important for compliance?](#q26-what-monitoring-and-audit-capabilities-does-azure-key-vault-provide-and-why-are-they-important-for-compliance)
27. [Q27. What common mistake do developers make when they assume `AddAzureKeyVault` alone makes their entire configuration secure?](#q27-what-common-mistake-do-developers-make-when-they-assume-addazurekeyvault-alone-makes-their-entire-configuration-secure)

---

## Q1. What is Azure Key Vault, and what three categories of cryptographic material does it store?

**Concepts**
- Secrets — arbitrary name-value pairs such as connection strings and API keys
- Keys — managed cryptographic material for sign, encrypt, and wrap operations
- Certificates — X.509 documents for TLS, client auth, and code signing
- Unified access control, soft-delete lifecycle, and audit logging across all three

**Answer**

Azure Key Vault is a managed cloud service that centralizes storage and access control for secrets, cryptographic keys, and certificates used by applications and services. Instead of scattering passwords and keys across configuration files, deployment scripts, and developer machines, teams store them in a vault where Azure enforces authentication, authorization, auditing, and optional hardware-backed protection. Secrets are arbitrary name-value pairs — connection strings, API keys, OAuth client secrets — retrieved at runtime by applications through the Key Vault REST API or the ASP.NET Core configuration provider. Keys are cryptographic key material used for signing, verification, encryption, and decryption operations; Key Vault can perform crypto operations inside the service so private key bytes never leave the vault boundary. Certificates are X.509 certificates that Key Vault can import, store, renew automatically, and expose for TLS termination or client authentication. All three object types share the same vault URI, access control model, soft-delete lifecycle, and diagnostic logging, giving operations teams one place to govern sensitive material across an estate of ASP.NET Core applications.

---

## Q2. What is the difference between a Key Vault secret, a Key Vault key, and a Key Vault certificate?

**Concepts**
- Secret — stored string value retrieved by the application directly
- Key — managed RSA or EC key pair used for sign/encrypt without exporting
- Certificate — full X.509 document with managed renewal policy
- Object type determines whether the app reads a value or delegates crypto operations

**Answer**

A secret is a simple stored string value the application reads directly — the vault does not interpret the payload. A key is managed cryptographic material that Key Vault can use to perform sign, verify, wrapKey, and unwrapKey operations, which lets private key material stay inside the vault even when the app performs crypto at scale, since the app sends data to the vault for signing rather than receiving the key bytes. A certificate is a full X.509 identity document with a private key that Key Vault can import or generate, renew on a schedule, and surface for TLS termination or signing scenarios. The right object type depends on whether the application needs to read a value (secret), delegate a cryptographic operation (key), or present an identity document for transport security (certificate). Choosing the wrong type — storing a JWT signing key as a plain secret instead of a Key Vault key — gives the app access to raw key bytes it could inadvertently expose rather than having Key Vault perform the operation without exporting the material.

---

## Q3. Why should production ASP.NET Core applications store sensitive configuration in Azure Key Vault instead of in `appsettings.json` or plain environment variables alone?

**Concepts**
- appsettings.json committed to source control and embedded in deployment artifacts
- Environment variables — plain text at rest on host, visible to portal users
- Key Vault — encryption at rest, fine-grained RBAC, versioning, and audit trail
- Managed identity token acquisition replacing secrets in configuration entirely

**Answer**

`appsettings.json` is usually committed to source control and copied with every deployment artifact, so placing production secrets there creates a high risk of accidental exposure and makes rotation painful since every copy must be updated. Environment variables improve separation from code but are still plain text at rest on the host, often visible to anyone with shell or portal access, and difficult to audit centrally. Azure Key Vault adds encryption at rest, fine-grained access control, versioning, rotation support, and an immutable audit trail while ASP.NET Core still reads values through the same `IConfiguration` abstraction. Key Vault secrets are never embedded in container images, Git history, or build logs when managed identity is used at runtime, since the application fetches them only after it starts with an authenticated identity. Centralizing secrets in a vault means rotation updates one authoritative object rather than redeploying new environment variables across every App Service slot, pod spec, and CI pipeline variable, and compliance frameworks requiring demonstrable controls over secret access are satisfied by Key Vault diagnostic logs in a way flat configuration files cannot match.

---

## Q4. What are soft delete and purge protection in Azure Key Vault, and why do they matter for production vaults?

**Concepts**
- Soft delete — recoverable retention period before permanent destruction
- Purge protection — administrator-proof lock preventing early permanent deletion
- Retention window of 7 to 90 days configurable at vault level
- One-way decision — purge protection cannot be disabled after enablement

**Answer**

Soft delete means that when a secret, key, certificate, or entire vault is deleted, Azure retains it in a recoverable state for a configurable retention period of 7 to 90 days instead of destroying it immediately, allowing operators to restore deleted objects through the portal, CLI, or REST API. Purge protection is an additional vault-level setting that prevents even a privileged administrator from permanently purging soft-deleted objects until the retention window expires, guarding against malicious or mistaken permanent deletion. Without soft delete, a single erroneous `az keyvault secret delete` or misconfigured automation script could instantly remove credentials that production ASP.NET Core instances depend on, causing widespread outages. Purge protection is especially important because compromised admin credentials could otherwise be used to permanently erase secrets and cover tracks; with it enabled, attackers must wait out the retention window before permanent removal, giving the security team time to respond. Enabling purge protection on the vault is a one-time architectural decision that cannot be undone without recreating the vault, so it should be made before the vault holds production material.

---

## Q5. What is the difference between a standard Key Vault and a Key Vault with a Hardware Security Module (HSM) backing?

**Concepts**
- Standard tier — software-protected keys, suitable for secrets and TLS certs
- Premium tier — FIPS 140-2 Level 2 HSM-backed keys for regulated workloads
- Managed HSM — dedicated single-tenant Level 3 HSM pool
- Tier matching regulatory requirement rather than defaulting to HSM

**Answer**

A standard Key Vault stores and processes keys in software-protected storage managed by Microsoft, which satisfies most application-secret and TLS certificate scenarios. An Azure Key Vault Premium tier stores keys in FIPS 140-2 Level 2 validated hardware security modules, meaning private key material is generated and used inside tamper-resistant hardware and cannot be exported in plaintext. Managed HSM is a separate service with FIPS 140-2 Level 3 validated dedicated hardware for organizations needing single-tenant HSM isolation rather than multi-tenant vault sharing, chosen when high-assurance crypto, payment, or government requirements demand it. Standard vaults fully support secrets and certificates plus software-protected keys, and most ASP.NET Core applications storing connection strings and HTTPS certificates use this tier. Cost and operational complexity increase with HSM tiers, so I match the tier to regulatory requirements rather than defaulting to HSM for every secret string.

---

## Q6. How do you integrate Azure Key Vault into an ASP.NET Core application's configuration pipeline?

**Concepts**
- AddAzureKeyVault extension method on IConfigurationBuilder
- DefaultAzureCredential as the TokenCredential for vault authentication
- Vault URI following the https://{vault-name}.vault.azure.net/ pattern
- Provider placement determining whether vault values override or are overridden

**Answer**

I install the `Azure.Extensions.AspNetCore.Configuration.Secrets` package, register a `TokenCredential` like `DefaultAzureCredential`, and call `builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), credential)` in `Program.cs` before services that depend on configuration are built. That single call adds Key Vault as a configuration provider, so secrets in the vault surface through `IConfiguration` and the Options pattern exactly like values from JSON files.

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());
```

The vault URI follows the pattern `https://{vault-name}.vault.azure.net/` and must match the vault deployed in the same Entra ID tenant as the credential. I place the `AddAzureKeyVault` call after default JSON providers so vault values override `appsettings.json`, or adjust order deliberately when vault secrets should act as defaults only. Failures to authenticate or missing permissions surface at startup when the provider first loads secrets, which is preferable to lazy failures on the first database connection attempt since it causes a clear startup error rather than a mid-request exception.

---

## Q7. Which NuGet packages are required to use Azure Key Vault as a configuration provider in ASP.NET Core?

**Concepts**
- Azure.Extensions.AspNetCore.Configuration.Secrets — AddAzureKeyVault extension
- Azure.Identity — DefaultAzureCredential and other TokenCredential implementations
- Azure.Security.KeyVault.Secrets/Certificates/Keys for direct SDK access
- Modern Azure.* SDK track replacing deprecated Microsoft.Azure.KeyVault

**Answer**

The primary package is `Azure.Extensions.AspNetCore.Configuration.Secrets`, which contains the `AddAzureKeyVault` extension method for `IConfigurationBuilder` and maps vault secret names into the configuration key space. Authentication is handled through `Azure.Identity`, which provides `DefaultAzureCredential`, `ManagedIdentityCredential`, and `ClientSecretCredential` implementations that acquire Entra ID tokens for Key Vault REST calls without requiring OAuth boilerplate. For direct SDK access outside configuration — for example loading a certificate for Kestrel — I additionally reference `Azure.Security.KeyVault.Certificates`, `Azure.Security.KeyVault.Secrets`, or `Azure.Security.KeyVault.Keys` depending on the object type. Package versions should align with the application's target framework; modern .NET 10 apps use the current stable `Azure.*` SDK track, not the deprecated `Microsoft.Azure.KeyVault` legacy client.

---

## Q8. How are Key Vault secret names mapped to `IConfiguration` keys, and what naming convention should you use for nested settings such as `ConnectionStrings:Default`?

**Concepts**
- Double-hyphen separator in secret names mapping to colon in IConfiguration
- Key Vault naming rules — alphanumeric and hyphens only, no underscores or colons
- Case-insensitive secret names and IConfiguration keys
- Labels and versions separate from the secret name

**Answer**

Key Vault secret names may contain only alphanumeric characters and hyphens, so hierarchical configuration paths use double hyphens as segment separators. The configuration provider translates `--` in a secret name to `:` in `IConfiguration`, meaning a secret named `ConnectionStrings--Default` is read as `configuration["ConnectionStrings:Default"]`. This mirrors the environment variable convention using `__` but uses `--` because Key Vault naming rules reject underscores and colons. A secret named `Stripe--SecretKey` binds to `Stripe:SecretKey` and works with `GetSection("Stripe")` without any code changes, since the mapping is handled entirely by the provider. Secret names are case-insensitive in Key Vault and `IConfiguration` key lookups are also case-insensitive by default in ASP.NET Core, but I standardize on PascalCase segments for consistency with JSON files. Labels and versions are separate from the name — the provider loads the latest enabled version unless custom reload logic pins a specific version.

---

## Q9. When `AddAzureKeyVault` is called in `Program.cs`, at what point are secrets loaded, and does the running application automatically pick up rotated secrets?

**Concepts**
- Startup-time loading — secrets fetched during configuration builder execution
- In-memory cache — values fixed for the process lifetime by default
- Rotation without restart requires reload logic or App Configuration refresh
- Secret versioning retaining old version until provider is refreshed

**Answer**

By default, the Key Vault configuration provider fetches all secrets during application startup when the configuration builder executes, then caches the returned values in memory for the process lifetime. Rotating a secret in the vault does not automatically change what an already-running ASP.NET Core instance reads until the process restarts or explicit reload behavior is added. Startup loading means the first failed `get` permission or missing secret prevents the app from building configuration correctly, which typically fails fast during boot rather than mid-request — a desirable fail-fast behavior. The default provider does not poll Key Vault on an interval; long-lived App Service instances continue using the value retrieved at cold start until recycled. To support rotation without full restarts, I use `Azure.Extensions.AspNetCore.Configuration.Secrets` reload options when configured, switch to Azure App Configuration with Key Vault references and refresh middleware, or implement `IOptionsMonitor` with a custom polling secret provider. Secret versioning in Key Vault always retains old versions, so the provider without reload logic holds the old version's value until refreshed.

---

## Q10. How does Azure Key Vault fit into the ASP.NET Core configuration provider hierarchy relative to `appsettings.json`, user secrets, and environment variables?

**Concepts**
- Configuration provider registration order determining precedence
- Key Vault added last for highest precedence in production
- Environment variables as break-glass override when placed after Key Vault
- Debugging wrong values by identifying which provider supplied them

**Answer**

Configuration providers registered later override earlier ones for the same key. In a typical production setup, `appsettings.json` and `appsettings.{Environment}.json` supply non-sensitive defaults, user secrets apply only in Development, environment variables inject container or platform settings, and Key Vault is added last so vault secrets win when names collide. Adding Key Vault last ensures a connection string in the vault overrides a placeholder in JSON even if an environment variable was also set incorrectly during a migration. Some teams intentionally place environment variables after Key Vault for emergency break-glass overrides, which is valid but should be documented since it inverts the more common production pattern. Understanding provider order is essential when debugging "wrong connection string" incidents: inspecting which provider supplied the winning value before assuming Key Vault is misconfigured reveals whether the override is coming from an environment variable set during a previous deployment.

---

## Q11. What is Azure managed identity, and how does an ASP.NET Core app running on Azure App Service authenticate to Key Vault without storing credentials?

**Concepts**
- Managed identity — platform-managed Entra ID service principal for Azure resources
- System-assigned and user-assigned identity types
- DefaultAzureCredential or ManagedIdentityCredential acquiring bearer token at runtime
- No client secret in configuration since the platform proves identity

**Answer**

Managed identity is a Microsoft Entra ID feature that gives an Azure resource — such as App Service, Azure Functions, or AKS — an automatically managed service principal. The platform obtains access tokens for Azure resources on behalf of the app, so no client secret or certificate credential appears in configuration. On App Service I enable system-assigned or user-assigned managed identity in the portal or infrastructure-as-code template, then use the identity's object ID or client ID in Key Vault access policies or RBAC assignments. Application code uses `new DefaultAzureCredential()` or `new ManagedIdentityCredential()` when calling `AddAzureKeyVault`; at runtime on Azure, the credential exchanges the platform-managed identity for a bearer token scoped to `https://vault.azure.net/.default`. Key Vault validates the token, checks RBAC or access policy permissions, and returns only the secrets that identity is allowed to read. Because there is no secret in `appsettings.json`, compromising the repository does not grant Key Vault access unless attackers also gain control of the Azure resource itself.

---

## Q12. What is `DefaultAzureCredential`, and in what order does it attempt authentication methods?

**Concepts**
- Chained credential trying multiple sources in sequence until one succeeds
- Managed identity succeeding in Azure without any stored password
- Azure CLI and Visual Studio as developer fallbacks for local development
- AuthenticationFailedException when all sources in the chain fail

**Answer**

`DefaultAzureCredential` is a chained credential from the `Azure.Identity` library that tries a sequence of authentication sources until one succeeds, allowing the same code path to work on a developer workstation and in Azure production without conditional compilation. The typical chain (subject to SDK version) is: environment variables (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_CLIENT_SECRET`) for service principal scenarios, then Workload Identity on AKS, then Managed Identity on App Service or Functions, then Visual Studio signed-in account, then Azure CLI login session, then Azure PowerShell, then Azure Developer CLI. Locally, developers usually authenticate through Azure CLI or Visual Studio; in Azure, managed identity succeeds without any stored password. Explicitly using `ManagedIdentityCredential` instead of `DefaultAzureCredential` is appropriate in production-hardened deployments that want to forbid fallback to developer credentials. Failed attempts in the chain are swallowed until all options fail, at which point an `AuthenticationFailedException` explains which sources were tried — a common local-dev error when `az login` has expired.

---

## Q13. What is the difference between a system-assigned managed identity and a user-assigned managed identity when granting Key Vault access?

**Concepts**
- System-assigned — tied to one resource, deleted when the resource is deleted
- User-assigned — standalone resource shared across multiple apps
- AZURE_CLIENT_ID required for DefaultAzureCredential to select the correct identity
- Fleet access pattern favoring user-assigned for uniform vault permissions

**Answer**

A system-assigned managed identity is created automatically for one Azure resource, shares that resource's lifecycle, and is deleted when the resource is deleted. A user-assigned managed identity is created as its own Azure resource, can be attached to multiple resources simultaneously, and persists independently of any single app. System-assigned is simpler for one web app that owns its own Key Vault permissions and should lose access automatically when the app is torn down. User-assigned fits microservice estates where ten APIs need identical read access to a shared set of secrets — I grant the identity once and attach it to each App Service plan instance, rather than configuring ten separate RBAC assignments. ASP.NET Core code is identical for both; only the Azure resource configuration and the object ID used in RBAC assignments differ. When using user-assigned identity with `DefaultAzureCredential`, I set `AZURE_CLIENT_ID` to the user-assigned identity's client ID so the credential selects the correct identity among several that may be attached to the host.

---

## Q14. Why is storing a Key Vault client secret in `appsettings.json` or environment variables considered an anti-pattern?

**Concepts**
- Secret zero problem — a credential needed to fetch all other credentials
- Client secret expiry causing production startup failures on missed rotation
- Secret sprawl — copies in environment variables, crash dumps, and portal blades
- Managed identity eliminating bootstrap credentials entirely

**Answer**

Using a client secret to authenticate to Key Vault recreates the very problem Key Vault is meant to solve: a long-lived credential that unlocks all other credentials. If that bootstrap secret is committed, logged, or copied into every deployment slot, an attacker who obtains it can read every secret the service principal can access in the vault, which is called the "secret zero" problem — and managed identity eliminates it by having Azure prove the app's identity at the platform layer. Client secrets expire and require rotation; missing a rotation breaks production startup until every copy of the secret is updated, whereas managed identity tokens are short-lived and acquired automatically by the platform. Service principal secrets in environment variables appear in crash dumps, diagnostic exports, and portal configuration blades, expanding the blast radius beyond Key Vault audit logs. Acceptable exceptions include local development without Azure CLI, legacy CI pipelines not yet migrated to workload identity federation, and third-party hosts that cannot use managed identity — in those cases secrets should live in the CI secret store, not source-controlled JSON.

---

## Q15. What are Key Vault access policies, and what permissions can they grant on secrets, keys, and certificates?

**Concepts**
- Access policy as per-vault ACL listing explicit permissions per principal
- Secret permissions — get, list, set, delete, backup, restore, recover, purge
- Key permissions adding cryptographic operations like sign, encrypt, wrapKey
- Granularity without RBAC tooling — harder to audit at enterprise scale

**Answer**

Access policies are the original Key Vault authorization model where for each Entra ID security principal, an administrator lists explicit data-plane permissions on secrets, keys, and certificates separately. An ASP.NET Core app's managed identity typically receives only `get` and sometimes `list` on secrets — not `set`, `delete`, or `purge` — to enforce least privilege. Secret permissions include `get`, `list`, `set`, `delete`, `backup`, `restore`, `recover`, and `purge`; production read-only apps need only `get`. Key permissions add cryptographic operations like `sign`, `verify`, `encrypt`, `decrypt`, `wrapKey`, and `unwrapKey` for applications using Key Vault's cryptographic APIs or Data Protection key encryption. Certificate permissions cover `get`, `list`, `create`, `import`, and `manageissuers` for TLS and signing certificate workflows. Access policies are vault-local and ignore Azure resource group boundaries, which was flexible early on but becomes hard to audit at enterprise scale compared with subscription-wide RBAC assignments, which is why Microsoft recommends RBAC for new vaults.

---

## Q16. What is Azure Role-Based Access Control (RBAC) for Key Vault, and how does the permission model differ from legacy access policies?

**Concepts**
- Key Vault RBAC — Azure role assignments at vault, resource group, or subscription scope
- Key Vault Secrets User and other built-in roles replacing custom permission flags
- Subscription-wide tooling, ARM templates, and Azure Policy managing RBAC assignments
- RBAC-only vault mode preventing access policy evaluation

**Answer**

Key Vault RBAC assigns familiar Azure roles — such as `Key Vault Secrets User` — at vault, resource group, or subscription scope using the same Entra ID role system as Storage or SQL. Permissions are bundled into roles rather than hand-picked per object type, and assignments appear in standard Azure access reviews and Privileged Identity Management workflows. The model uses Azure's data actions under `"Microsoft.KeyVault/vaults/*"`, so roles like `Key Vault Administrator` are for operators and not appropriate for application identities. Access policies and RBAC cannot both authorize the same caller on vaults configured for RBAC-only mode; new vaults should enable RBAC authorization from creation so all assignments use the consistent enterprise identity model. Migration paths exist to convert policy-based vaults to RBAC without recreating secrets, but planning is required so production apps do not lose access mid-cutover.

---

## Q17. Which authorization model does Microsoft recommend for new Key Vault deployments, and can both models be active on the same vault?

**Concepts**
- Azure RBAC as the recommended model for new vaults
- Single authorization mode per vault — RBAC or access policies, not both
- Control-plane operations always governed by RBAC regardless of vault mode
- Key Vault Secrets User role for application runtime access

**Answer**

Microsoft recommends Azure RBAC for new Key Vault instances because it integrates with enterprise identity governance, supports assignment at scale via ARM templates and Azure Policy, and aligns with other Azure services. Vaults created with RBAC as the authorization model do not use access policies for data-plane permission; callers need appropriate RBAC role assignments like `Key Vault Secrets User` on that vault scope. Legacy vaults may still rely entirely on access policies and continue to work, but should be migrated during security modernization efforts. A vault is in either RBAC mode or access-policy mode for data-plane authorization — both models are not evaluated simultaneously for the same caller on the same vault. Control-plane operations like creating the vault and changing network rules always use Azure RBAC separately via roles like `Key Vault Contributor`, independent of whether the data plane uses RBAC or access policies. Greenfield ASP.NET Core deployments should grant the app's managed identity `Key Vault Secrets User` on the specific vault rather than copying broad access-policy templates with unnecessary permissions.

---

## Q18. Which built-in Azure RBAC roles are commonly assigned to an ASP.NET Core application's managed identity for read-only secret access?

**Concepts**
- Key Vault Secrets User — read secret properties and values without write rights
- Key Vault Certificate User — read certificate and private key for Kestrel TLS
- Key Vault Crypto User — signing and Data Protection key wrap/unwrap
- Narrowest scope assignment — individual vault resource, not subscription-wide

**Answer**

The role used most often for production web APIs is Key Vault Secrets User, which allows reading secret properties and values (`get`) and enumeration (`list`) without granting write or delete rights. If the app loads TLS certificates from the vault, Key Vault Certificate User provides read access to certificates and the private key material needed for Kestrel. Key Vault Secrets Officer includes write operations and is appropriate for deployment pipelines or admin tools, not long-running App Service instances. Key Vault Crypto User is assigned when the app uses Key Vault keys for signing or Data Protection without exporting key bytes. Key Vault Reader exposes metadata only and not secret values, making it insufficient for configuration providers that must read secret payloads. I assign roles at the narrowest scope that works — usually the individual vault resource — rather than subscription-wide Key Vault Administrator, which violates least privilege and grants far more than an application needs.

---

## Q19. What does least-privilege access mean in the context of Key Vault, and why should an app identity not receive blanket `get` permission on all secrets?

**Concepts**
- Least privilege limiting each identity to only required secrets and permissions
- Shared vault blast radius when one identity can read all secrets
- Separate vaults per environment and per sensitivity boundary
- Periodic access review removing orphaned identities

**Answer**

Least privilege means each managed identity receives only the minimum Key Vault permissions and secret scope required for its job function, so a compromised web tier cannot exfiltrate unrelated database credentials or third-party API keys stored in the same vault. Granting an identity access to every secret in a shared vault multiplies blast radius because all ASP.NET Core apps often share one vault for convenience, meaning compromising one app's identity exposes credentials for every app in the estate. I prefer separate vaults per environment (development, staging, production) and per sensitivity boundary so a staging app identity physically cannot read production secrets even if misconfigured. Within one vault, RBAC with Azure ABAC conditions or splitting secrets across vaults provides finer isolation than a single `Key Vault Secrets User` assignment. Deployment pipelines receive `set` permission on specific secrets while runtime App Service identities receive only `get` on the subset they consume, and periodic access reviews remove identities tied to decommissioned App Service slots since orphaned principals with `get` remain valid until explicitly revoked.

---

## Q20. How does secret versioning work in Azure Key Vault, and how does versioning support zero-downtime rotation?

**Concepts**
- New version ID assigned on each create or update operation
- Latest version resolved at startup without pinning
- Dual-credential rotation window allowing staggered app restarts
- Disabling current version before apps refresh as a common rotation mistake

**Answer**

Every time I create or update a secret, Key Vault assigns a new version identifier while retaining previous versions until I delete them. Applications can read the latest version by omitting a version in the `get` call, or pin a specific version when deterministic behavior is required during a controlled rollout. Rotation begins by adding a new version with the updated connection string or API key while the old version remains valid for in-flight connections still using the previous value. Database and downstream systems often accept both old and new credentials briefly during a rotation window, allowing staggered App Service restarts without a hard cutover failure. The ASP.NET Core default configuration provider loads the latest enabled version at startup; without reload, instances started before rotation keep the old value until recycled, which is why blue-green deployment or slot swaps accompany many rotation runbooks. Accidentally disabling the current version before all apps refresh is a common operational mistake during rushed rotations, since disabled versions are skipped by the provider.

---

## Q21. What strategies can you use to rotate a Key Vault secret consumed by a running ASP.NET Core application without restarting every instance?

**Concepts**
- Staggered restart or slot swap loading new secret version at startup
- Azure App Configuration with Key Vault references and refresh middleware
- Custom IOptionsMonitor polling provider for in-process refresh
- Dual-credential downstream acceptance during the rotation window

**Answer**

Because the default `AddAzureKeyVault` provider caches secrets at startup, zero-downtime rotation requires either accepting staggered natural recycling, forcing controlled restarts, or adding a refresh mechanism. The simplest approach is updating the secret version in Key Vault and then restarting App Service instances or swapping deployment slots so each process reloads configuration — reliable when brief mixed-version traffic is acceptable since both old and new instances run correctly during the overlap. Azure App Configuration with Key Vault references provides middleware that can poll for changes and reload `IOptionsMonitor` without a full process restart, which suits long-running services where even a rolling restart takes too long. A custom `IOptionsMonitor` polling background service that periodically fetches the latest secret version via `SecretClient` gives full control over refresh intervals and failure handling but requires more code. For databases, creating two SQL logins during rotation — updating Key Vault, refreshing apps, then retiring the old login — means the app restart timing matters less because both credentials work concurrently during the transition window.

---

## Q22. How do you load an HTTPS certificate from Azure Key Vault for Kestrel in ASP.NET Core?

**Concepts**
- CertificateClient downloading X509Certificate2 including private key
- ConfigureKestrel configuring HTTPS defaults with the downloaded certificate
- Key Vault Certificate User or certificate get permission required
- Platform-side TLS termination as an alternative to in-process binding

**Answer**

I use the `Azure.Security.KeyVault.Certificates` client with `DefaultAzureCredential` to download the certificate as an `X509Certificate2` instance including the private key when permitted, then configure Kestrel in `Program.cs` with `ConfigureKestrel` and `ListenOptions.UseHttps(certificate)`.

```csharp
var client = new CertificateClient(vaultUri, new DefaultAzureCredential());
var cert = await client.DownloadCertificateAsync("MyTlsCert");
builder.WebHost.ConfigureKestrel(o =>
    o.ConfigureHttpsDefaults(h => h.ServerCertificate = cert.Value));
```

The managed identity needs Key Vault Certificate User RBAC or certificate `get` permission to retrieve the private key material. Certificates imported into Key Vault as `.pfx` must be marked exportable if the app requires the private key locally on Kestrel; platform binding through App Service or Azure Container Apps avoids exporting the key entirely since TLS terminates at the front-end load balancer. Automatic certificate renewal in Key Vault reduces expiry outages, but apps must still restart or implement periodic refresh to pick up renewed cert bytes since the certificate object is loaded once at startup.

---

## Q23. What is `ProtectKeysWithAzureKeyVault` in the Data Protection API, and how does it differ from storing application settings as Key Vault secrets?

**Concepts**
- ProtectKeysWithAzureKeyVault encrypting the key ring at rest using a Key Vault key
- Key wrap/unwrap operations — Key Vault Crypto User role required
- Data Protection keys still persisted separately — Key Vault only wraps them
- Losing the wrapping key invalidating all authentication cookies

**Answer**

`ProtectKeysWithAzureKeyVault` configures ASP.NET Core Data Protection to encrypt the key ring at rest using a Key Vault key, so persisted Data Protection keys in blob storage or Redis remain useless ciphertext without Key Vault decrypt permission. Storing a connection string as a Key Vault secret via `AddAzureKeyVault` is unrelated — that supplies configuration values, whereas `ProtectKeysWithAzureKeyVault` wraps the cryptographic keys used to protect cookies and antiforgery tokens. Data Protection keys are still persisted to a separate store like `PersistKeysToAzureBlobStorage`, SQL, or Redis; Key Vault only encrypts those key files, not replaces the store. The Key Vault key object performs `wrap`/`unwrap` operations, so the app identity needs Key Vault Crypto User or key `unwrapKey` permission, not merely secret read access. Losing access to the wrapping key makes all existing authentication cookies invalid — equivalent to losing the key ring — so operational ownership of the Key Vault key must match the cookie session lifetime expectations for the application.

---

## Q24. How should developers authenticate to Key Vault during local development on a workstation?

**Concepts**
- az login or Visual Studio Azure account as developer credential source
- DefaultAzureCredential resolving developer identity without embedded secrets
- Dedicated development vault isolating experimental secrets from production
- User secrets as a simpler alternative for local-only connection strings

**Answer**

Developers should sign in with Azure CLI (`az login`) or Visual Studio Azure account credentials and use `DefaultAzureCredential` in local `Program.cs`, which picks up the developer identity without embedding a client secret in user secrets. Their Entra ID user or a dedicated developer group needs RBAC like `Key Vault Secrets User` on a non-production vault — never production secrets on individual laptops unless policy explicitly allows break-glass access. A dedicated development Key Vault isolates experimental secrets from production and allows looser developer group assignments without risking production data-plane access. An alternative approach is to keep using ASP.NET Core user secrets locally for connection strings and reserve Key Vault integration testing for shared development vaults or CI environments, since that avoids requiring every developer to have Azure credentials configured on their machine. If `DefaultAzureCredential` fails locally, the error usually means no login session, the wrong tenant, or missing vault RBAC — not a missing NuGet package.

---

## Q25. How do Azure DevOps variable groups integrate with Azure Key Vault for pipeline secret injection?

**Concepts**
- Variable group linking to Key Vault for pipeline-time secret retrieval
- Selected secrets imported as pipeline variables without repository storage
- Service connection or workload identity federation authenticating to the vault
- Pipeline variable groups supplementing not replacing runtime managed identity

**Answer**

Azure DevOps variable groups can link directly to an Azure Key Vault; selected secrets appear as pipeline variables — marked secret and masked in logs — available to YAML or classic release pipelines without storing values in the repository. The pipeline service connection or workload identity federation authenticates to Key Vault at queue time, retrieves current secret values, and injects them into tasks like deployment or integration tests. Linking is configured in Pipelines → Library → Variable groups → "Link secrets from an Azure key vault as variables," and only secrets explicitly selected are imported rather than the entire vault. Secret values sync when the pipeline runs, so updating Key Vault does not require editing YAML — the next pipeline run picks up new values automatically. Pipelines that deploy ASP.NET Core to App Service still prefer runtime managed identity for the running app; variable groups bootstrap deployment-time secrets like publish profiles or one-time tokens rather than replacing in-app Key Vault configuration. GitHub Actions achieves a similar outcome with `azure/login` and OIDC federation instead of long-lived service principal passwords in repository secrets.

---

## Q26. What monitoring and audit capabilities does Azure Key Vault provide, and why are they important for compliance?

**Concepts**
- AuditEvent diagnostic logs — caller IP, object ID, operation, and result
- Azure Activity Log for control-plane events like vault creation and policy changes
- Throttling metrics detecting aggressive polling during misconfigured refresh
- Integration with Azure Sentinel and Defender for Cloud for incident response

**Answer**

Key Vault emits diagnostic logs to Log Analytics, Azure Monitor, or Event Hub recording authentication attempts, secret get/set/delete operations, key usage, and policy changes, producing an auditable trail of who accessed which secret and when. Azure Activity Log captures control-plane events such as vault creation and network rule updates. `AuditEvent` logs include caller IP, object ID, and operation result, which are essential for investigating suspected credential theft from a compromised managed identity — I can determine exactly which secrets were read, by which identity, from which IP, and when. Metrics such as saturation and availability expose throttling when applications poll secrets too aggressively during misconfigured refresh loops. Compliance regimes like SOC 2, PCI-DSS, and HIPAA-aligned workloads require demonstrating access controls and reviewable logs; Key Vault centralizes that evidence for all ASP.NET Core consumers in one diagnostic stream. Alert rules on unusual `SecretGet` volume or failed authentication spikes integrate with Azure Sentinel or Defender for Cloud for automated incident response.

---

## Q27. What common mistake do developers make when they assume `AddAzureKeyVault` alone makes their entire configuration secure?

**Concepts**
- AddAzureKeyVault supplying additional key-value pairs, not encrypting existing ones
- Plaintext secrets in committed JSON remaining visible despite vault integration
- Verbose configuration logging printing secret values to Application Insights
- Complete model — vault storage, no-secrets JSON, managed identity, and rotation

**Answer**

Developers often believe that calling `AddAzureKeyVault` encrypts or sanitizes all configuration, when in reality it only supplies additional key-value pairs from the vault — any secret still present in committed `appsettings.json`, logged environment variables, or verbose exception messages remains exposed. `AddAzureKeyVault` does not delete or override secrets that exist only in JSON unless the same key is also defined in the vault with higher provider precedence, so placeholders in Git are still visible to anyone with repository access. A second common mistake is startup-level configuration logging: if developers indiscriminately log `IConfiguration` keys at `Debug` level, secret values can be printed to Application Insights traces. Local development often still loads user secrets and JSON that are shipped to production without review, reintroducing plaintext credentials beside the vault integration. A complete security model stores authoritative values in Key Vault, keeps JSON free of secrets, uses managed identity at runtime, audits vault access, and rotates versions on a schedule — `AddAzureKeyVault` is one step in that chain, not the whole solution.

---

## Gotchas — Azure Key Vault (Interview Traps)

---

#### Gotcha 1. Managed identity not assigned to the resource — DefaultAzureCredential loops through all credential sources and gives a confusing error

**Concepts**
- `DefaultAzureCredential` tries multiple credential sources in order before failing
- Missing managed identity assignment causes all sources to fail with individual errors
- The aggregated exception message lists every source that was tried, which obscures the root cause
- `ManagedIdentityCredential` directly is faster to diagnose

**Answer**

When a managed identity is not enabled or not assigned to an App Service or Azure Function, `DefaultAzureCredential` cycles through all built-in credential sources (environment variables, workload identity, managed identity, Visual Studio, Azure CLI, etc.) before raising an `AuthenticationFailedException` that lists every source and its individual failure reason. The actual root cause — managed identity not assigned — is buried inside a long multi-paragraph error message. Using `ManagedIdentityCredential` directly in production accelerates diagnosis because it fails immediately with a single message instead of exhausting all alternatives first.

---

#### Gotcha 2. Key Vault soft-delete cannot be disabled — a vault deleted by mistake is not gone and must be purged explicitly

**Concepts**
- Soft-delete retains deleted Key Vaults in a recoverable state for 7–90 days
- Soft-delete has been mandatory since 2021 and cannot be disabled
- A deleted vault with the same name blocks re-creation until purged
- Purge protection, if enabled, prevents even manual purge for the retention period

**Answer**

Azure Key Vault soft-delete is now enforced on all vaults and cannot be disabled. When a Key Vault is deleted it enters a soft-deleted state and is retained for the configured retention period (7–90 days). If you try to create a new vault with the same name in the same region and subscription, the creation fails because the soft-deleted vault still occupies the name. The fix is to explicitly purge the soft-deleted vault using `az keyvault purge` before recreating with the same name. If purge protection is also enabled, you cannot purge the vault at all during the retention period, which means the name is locked for weeks.

---

#### Gotcha 3. Key Vault reference syntax in App Service requires Key Vault Secrets User role on the specific secret — vault-level role is not enough

**Concepts**
- App Service Key Vault references use the format `@Microsoft.KeyVault(SecretUri=...)`
- The managed identity needs "Key Vault Secrets User" role
- RBAC on the vault alone may not be sufficient; role must include the secret path
- Reference resolution failure shows as `@Microsoft.KeyVault(...)` in the app setting value at runtime

**Answer**

App Service Key Vault references resolve at startup using the app's managed identity. For the reference to resolve successfully, the identity needs the "Key Vault Secrets User" role. This role can be assigned at the vault level (granting access to all secrets) or at the individual secret level for least-privilege access. A common trap is assigning a vault-level role such as "Reader" that does not include the `get` permission on secrets, which causes the reference to fail to resolve. At runtime an unresolved Key Vault reference is visible as the literal `@Microsoft.KeyVault(...)` string in the application setting value, not an error message.

---

#### Gotcha 4. Pinning a specific secret version in the Key Vault reference URI prevents automatic rotation

**Concepts**
- Key Vault reference URIs can include a version GUID (`/secrets/MySecret/<version>`) or omit it for the latest
- A version-pinned reference always returns that specific version regardless of rotation
- Omitting the version returns the current latest secret, enabling automatic rotation
- Re-deploying with a new version URI is required if the version is pinned

**Answer**

A Key Vault secret URI has the form `https://<vault>.vault.azure.net/secrets/<name>/<version>`. When a Key Vault reference in App Service includes the version GUID, the reference is pinned to that exact secret version and does not pick up new versions created during secret rotation. Teams that implement secret rotation by creating a new secret version discover that the App Service continues reading the old pinned version until the application is redeployed with the updated URI. Omitting the version from the reference URI (`/secrets/<name>` with no trailing version) causes the reference to always resolve to the latest version and picks up rotation automatically after a brief cache refresh.

---

#### Gotcha 5. Key Vault Firewall enabled without VNet Service Endpoint blocks App Service connections

**Concepts**
- Key Vault firewall can be restricted to specific IP ranges or VNet Service Endpoints
- App Service does not have a static outbound IP by default; IPs change during scale events
- "Allow trusted Microsoft services" does not cover App Service by default
- VNet integration with a VNet Service Endpoint on Key Vault is the production pattern

**Answer**

Enabling the Key Vault firewall and specifying allowed IP ranges requires knowing the App Service's outbound IP addresses, which are stable within an App Service Plan but change if the plan is migrated or if the app is moved to a different plan. Adding individual IPs is fragile; a scale event or plan migration changes the IPs and breaks connectivity. The "Allow trusted Microsoft services" toggle does not cover App Service in all scenarios. The production pattern is to enable VNet integration on the App Service and configure a VNet Service Endpoint on Key Vault so that traffic from the App Service's delegated subnet is permitted without relying on static IPs.

---

#### Gotcha 6. AddAzureKeyVault loads all secrets at startup — large vaults with hundreds of secrets cause slow cold starts

**Concepts**
- `AddAzureKeyVault()` in `Program.cs` pages through all secrets in the vault on startup
- A vault with 500+ secrets causes several seconds of additional startup delay
- Throttling on Key Vault (2,000 requests per 10 seconds) can fail startup if multiple instances start simultaneously
- Secret prefix filtering or a custom `AzureKeyVaultConfigurationOptions` loads only needed secrets

**Answer**

`builder.Configuration.AddAzureKeyVault()` enumerates every secret in the vault at application startup and loads them all into the configuration system. For a vault that holds secrets for multiple teams or applications across hundreds of entries, this causes noticeable cold start delay and risks hitting Key Vault's request throttle limit (2,000 requests per 10 seconds) when multiple App Service instances start simultaneously during scale-out. The `AzureKeyVaultConfigurationOptions.Manager` property accepts a custom `KeyVaultSecretManager` that filters secrets by prefix so only the secrets belonging to the current application are loaded, dramatically reducing startup time and request volume.

---

#### Gotcha 7. Key Vault secret names use hyphens but IConfiguration maps them with double underscore — hierarchical keys don't round-trip

**Concepts**
- Key Vault secret names can contain hyphens but not colons or double underscores
- `AddAzureKeyVault` default manager replaces `--` (double hyphen) with `:` for hierarchical keys
- `MyApp--Database--ConnectionString` maps to `MyApp:Database:ConnectionString`
- A secret named `MyApp:Database:ConnectionString` is invalid in Key Vault

**Answer**

Azure Key Vault secret names may contain letters, numbers, and hyphens, but not colons. The default Key Vault configuration provider for .NET (`KeyVaultSecretManager`) translates double hyphens (`--`) to colon separators (`:`) when loading secrets into IConfiguration. A secret named `MyApp--Database--ConnectionString` becomes `Configuration["MyApp:Database:ConnectionString"]`. Developers who name secrets with single hyphens (`MyApp-Database-ConnectionString`) and then try to read them as hierarchical configuration keys (`Configuration["MyApp:Database:ConnectionString"]`) get null because single hyphens are not translated. Only double hyphens trigger the section-separator mapping.

---

#### Gotcha 8. Key Vault access policies and RBAC are separate authorization models — mixing them causes unexpected access denials

**Concepts**
- Legacy Key Vault access policies (vault policy model) are separate from Azure RBAC
- A vault set to "Vault access policy" model ignores RBAC role assignments on the vault resource
- A vault set to "Azure role-based access control" model ignores vault access policies
- Migrating from access policies to RBAC requires explicit migration and removes all existing policies

**Answer**

Key Vault supports two authorization models: the legacy "vault access policy" model and the newer "Azure role-based access control" model. These are mutually exclusive per vault; the active model is set in the vault's Properties. A managed identity granted "Key Vault Secrets User" via Azure RBAC on a vault that is still using the vault access policy model has no effective access, because RBAC assignments are ignored. Conversely, a vault access policy entry has no effect on a vault switched to RBAC model. When troubleshooting Key Vault access denials, the first thing to check is which authorization model the vault is using and whether the identity's permissions match that model.

---

#### Gotcha 9. Key Vault SDK throttles at 2,000 requests per 10 seconds — applications that fetch secrets per request hit the limit

**Concepts**
- Key Vault has service-level throttling at 2,000 GET requests per 10 seconds per vault
- Applications that call `GetSecretAsync` on every HTTP request exceed this limit under moderate load
- `SecretClient` caches are not built-in; the caller must implement caching
- `AddAzureKeyVault` in Program.cs loads at startup and respects the cache — per-request calls do not

**Answer**

Azure Key Vault enforces a throttling limit of 2,000 GET requests per 10 seconds per vault. An application that calls `SecretClient.GetSecretAsync("connectionString")` inside a controller action or service method on every incoming HTTP request will hit this limit under moderate production traffic (200 requests per second to the app means 200 Key Vault calls per second). The `SecretClient` does not cache results internally; callers must implement their own caching using `IMemoryCache` or the `AddAzureKeyVault` configuration provider which loads at startup. Key Vault is designed for startup configuration loading and secret rotation, not as a per-request secrets store.

---

#### Gotcha 10. Certificates stored as Key Vault secrets return the full PEM chain — reading only the first certificate misses intermediate CA certificates

**Concepts**
- Key Vault certificates stored as secrets return base64-encoded PFX or PEM including the full chain
- `X509Certificate2` loaded from the full chain must use the correct constructor flag for chain inclusion
- Missing intermediate CA certificates causes TLS handshake failures in strict clients
- The Key Vault certificate object and the secret representation have different access paths

**Answer**

When you store a TLS certificate in Key Vault, it is accessible both as a Key Vault Certificate object and as a Secret that returns the full certificate chain in PEM or PFX format. Code that reads the secret and loads only the leaf certificate (the first certificate in the chain) without including intermediate CA certificates can cause TLS handshake failures with clients that do strict chain validation. The correct approach is to read the full PFX bytes and use `new X509Certificate2(bytes, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable)` which preserves the complete chain. Also note the Key Vault Certificate API and the Key Vault Secret API return different formats, and code that uses one when the other is expected produces decoding errors.

---
