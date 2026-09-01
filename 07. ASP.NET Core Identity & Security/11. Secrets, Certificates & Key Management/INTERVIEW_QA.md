# Secrets, Certificates & Key Management — Interview Q&A
> 27 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. Why should secrets such as connection strings, API keys, and signing keys never be committed to source control?](#q1-why-should-secrets-such-as-connection-strings-api-keys-and-signing-keys-never-be-committed-to-source-control)
2. [Q2. What is the ASP.NET Core Secret Manager, and what problem does it solve during development?](#q2-what-is-the-aspnet-core-secret-manager-and-what-problem-does-it-solve-during-development)
3. [Q3. Where does the Secret Manager physically store user secrets on disk, and are those secrets encrypted?](#q3-where-does-the-secret-manager-physically-store-user-secrets-on-disk-and-are-those-secrets-encrypted)
4. [Q4. What is the ASP.NET Core configuration provider hierarchy, and in what order are providers loaded?](#q4-what-is-the-aspnet-core-configuration-provider-hierarchy-and-in-what-order-are-providers-loaded)
5. [Q5. How do environment variables override appsettings.json values, and what naming convention maps nested JSON keys to environment variable names?](#q5-how-do-environment-variables-override-appsettingsjson-values-and-what-naming-convention-maps-nested-json-keys-to-environment-variable-names)
6. [Q6. How do you integrate Azure Key Vault into an ASP.NET Core application's configuration pipeline?](#q6-how-do-you-integrate-azure-key-vault-into-an-aspnet-core-applications-configuration-pipeline)
7. [Q7. What is managed identity authentication, and why is it preferred over connection strings or client secrets when connecting to Azure Key Vault?](#q7-what-is-managed-identity-authentication-and-why-is-it-preferred-over-connection-strings-or-client-secrets-when-connecting-to-azure-key-vault)
8. [Q8. What is the ASP.NET Core Data Protection API, and what is its primary purpose?](#q8-what-is-the-aspnet-core-data-protection-api-and-what-is-its-primary-purpose)
9. [Q9. What is a Data Protection key ring, and how does key lifetime affect the ability to unprotect previously protected payloads?](#q9-what-is-a-data-protection-key-ring-and-how-does-key-lifetime-affect-the-ability-to-unprotect-previously-protected-payloads)
10. [Q10. What are the key storage locations available for Data Protection keys, and when would you choose each one?](#q10-what-are-the-key-storage-locations-available-for-data-protection-keys-and-when-would-you-choose-each-one)
11. [Q11. How do you encrypt Data Protection keys at rest, and what three mechanisms does ASP.NET Core offer?](#q11-how-do-you-encrypt-data-protection-keys-at-rest-and-what-three-mechanisms-does-aspnet-core-offer)
12. [Q12. What is the ProtectKeysWithAzureKeyVault method, and how does it differ from simply storing keys in Azure Key Vault?](#q12-what-is-the-protectkeyswithazurekeyvault-method-and-how-does-it-differ-from-simply-storing-keys-in-azure-key-vault)
13. [Q13. What is an X.509 certificate, and how is it used for signing JSON Web Tokens (JWTs) in ASP.NET Core?](#q13-what-is-an-x509-certificate-and-how-is-it-used-for-signing-json-web-tokens-jwts-in-aspnet-core)
14. [Q14. What is the difference between using a certificate stored in the Windows certificate store versus a file-based .pfx or .pem file?](#q14-what-is-the-difference-between-using-a-certificate-stored-in-the-windows-certificate-store-versus-a-file-based-pfx-or-pem-file)
15. [Q15. How does ASP.NET Core Kestrel load an HTTPS certificate, and what configuration options are available?](#q15-how-does-aspnet-core-kestrel-load-an-https-certificate-and-what-configuration-options-are-available)
16. [Q16. What is the dotnet dev-certs command, and what does it do to enable HTTPS during local development?](#q16-what-is-the-dotnet-dev-certs-command-and-what-does-it-do-to-enable-https-during-local-development)
17. [Q17. How are secrets managed in Docker containers, and what are the risks of baking secrets into a Docker image?](#q17-how-are-secrets-managed-in-docker-containers-and-what-are-the-risks-of-baking-secrets-into-a-docker-image)
18. [Q18. How does Kubernetes manage sensitive configuration data, and what are the security limitations of Kubernetes Secrets?](#q18-how-does-kubernetes-manage-sensitive-configuration-data-and-what-are-the-security-limitations-of-kubernetes-secrets)
19. [Q19. How do you store and consume secrets in a GitHub Actions workflow without hardcoding them in the YAML file?](#q19-how-do-you-store-and-consume-secrets-in-a-github-actions-workflow-without-hardcoding-them-in-the-yaml-file)
20. [Q20. How do Azure DevOps variable groups work, and how do you link them to Azure Key Vault for secret management?](#q20-how-do-azure-devops-variable-groups-work-and-how-do-you-link-them-to-azure-key-vault-for-secret-management)
21. [Q21. How do you rotate a secret — such as a database connection string — in a live production application without causing downtime?](#q21-how-do-you-rotate-a-secret-such-as-a-database-connection-string-in-a-live-production-application-without-causing-downtime)
22. [Q22. How do you rotate Data Protection keys without invalidating all existing protected payloads?](#q22-how-do-you-rotate-data-protection-keys-without-invalidating-all-existing-protected-payloads)
23. [Q23. What is the difference between symmetric and asymmetric key usage in the context of JWT signing, and when would you choose each approach?](#q23-what-is-the-difference-between-symmetric-and-asymmetric-key-usage-in-the-context-of-jwt-signing-and-when-would-you-choose-each-approach)
24. [Q24. What happens if you lose the Data Protection key ring in a production application, and how should you prevent this?](#q24-what-happens-if-you-lose-the-data-protection-key-ring-in-a-production-application-and-how-should-you-prevent-this)
25. [Q25. How does certificate-based authentication differ from token-based authentication, and when is a certificate the better choice?](#q25-how-does-certificate-based-authentication-differ-from-token-based-authentication-and-when-is-a-certificate-the-better-choice)
26. [Q26. What are the security risks of using environment variables to pass secrets in containers, and what mitigations exist?](#q26-what-are-the-security-risks-of-using-environment-variables-to-pass-secrets-in-containers-and-what-mitigations-exist)
27. [Q27. What common interview mistake do developers make when describing the security of ASP.NET Core user secrets?](#q27-what-common-interview-mistake-do-developers-make-when-describing-the-security-of-aspnet-core-user-secrets)

---

## Q1. Why should secrets such as connection strings, API keys, and signing keys never be committed to source control?

**Concepts**
- Git history permanence — deleted file secrets remain in all prior commits
- Automated scanner harvesting within seconds of a push
- Single repo compromise exposing all dependent system credentials
- Twelve-Factor App methodology — configuration injected from environment

**Answer**

Source control history is permanent and widely accessible — once a secret enters a repository, every contributor, every clone, and every CI system that ever pulled that repo has potentially seen it, even if the secret is later deleted in a subsequent commit. Automated scanners constantly monitor public repositories on GitHub and similar platforms, meaning a leaked key can be harvested and abused within seconds of a push. Secrets in source control create a single point of compromise: stealing the repository gives an attacker the credentials to every dependent system simultaneously. Rotating a secret that was committed requires updating all deployed environments and every historical reference in tooling, documentation, and CI pipelines — operational work that could have been avoided entirely. Configuration values change between environments (development, staging, production), so embedding them in code also breaks the principle of environment parity and forces branch-specific config files that diverge over time. Industry standards such as the Twelve-Factor App methodology explicitly mandate that configuration — anything that differs between environments — must be injected from the environment, not stored in the codebase.

---

## Q2. What is the ASP.NET Core Secret Manager, and what problem does it solve during development?

**Concepts**
- dotnet user-secrets init adding UserSecretsId to .csproj
- Secrets stored outside project directory — accidental git add prevention
- AddUserSecrets loading automatically in Development environment
- Hierarchical keys using colon separator mapping to JSON structure

**Answer**

The Secret Manager is a development-time tool built into the .NET CLI that stores sensitive configuration values outside the project directory so they are never accidentally checked into source control. It is activated by running `dotnet user-secrets init` in a project, which adds a `UserSecretsId` (a GUID) to the project file, and secrets are added via `dotnet user-secrets set "Key" "Value"`. The tool solves the common developer problem of needing real credentials locally without putting them in `appsettings.json`. The Secret Manager is explicitly designed for development only — it is not a secrets vault and provides no encryption; it is a convenience tool to keep working credentials out of the repo. Secrets are keyed by the `UserSecretsId` GUID, which means two developers working on the same project each maintain their own independent secret values even if they share the same codebase. In `Program.cs`, calling `builder.Configuration.AddUserSecrets<Program>()` (or using `CreateBuilder` in Development mode, which does this automatically) loads the secrets into the configuration pipeline with higher precedence than `appsettings.json`. The tool supports hierarchical keys using the colon separator: `dotnet user-secrets set "ConnectionStrings:Default" "..."` maps directly to the nested JSON structure used in `appsettings.json`.

---

## Q3. Where does the Secret Manager physically store user secrets on disk, and are those secrets encrypted?

**Concepts**
- %APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json — plain-text
- No encryption — only security guarantee is location outside repo root
- UserSecretsId GUID safe to commit — a lookup key only
- Production must use encrypted mechanisms

**Answer**

The Secret Manager stores all secrets in a plain-text JSON file located in the user's profile directory, outside the project tree, at `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json` on Windows and `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json` on macOS and Linux. The file is plain JSON — no encryption is applied at rest. The security guarantee is only that the file lives outside the repository root, preventing accidental `git add .` inclusion. Because the file is unencrypted, anyone with read access to the user profile directory (other local users, malware with user-level privileges) can read all stored secrets. This is a deliberate design trade-off: the Secret Manager targets developer convenience during local development, where the alternative is much worse (secrets in the repo). The `UserSecretsId` GUID stored in the `.csproj` file is safe to commit — it is only a lookup key and contains no sensitive data itself. On a clean machine or CI server, the secrets file does not exist, which means the application will fall back to other configuration providers; secrets that are genuinely required for the application to start must be provided through another mechanism in those environments.

---

## Q4. What is the ASP.NET Core configuration provider hierarchy, and in what order are providers loaded?

**Concepts**
- Last-wins rule — later providers override earlier ones for the same key
- Default ordering: appsettings.json → appsettings.{Env}.json → user secrets → env vars → Azure Key Vault → CLI args
- AddAzureKeyVault added last for highest precedence
- Debugging configuration by identifying which provider "won"

**Answer**

ASP.NET Core configuration is built from a layered stack of providers, where each provider added later in the chain can override values set by earlier providers. The last-wins rule means a value in an environment variable always overrides the same key in `appsettings.json`, which makes environment variables the natural mechanism for injecting environment-specific or sensitive configuration in deployed systems. The default order established by `WebApplication.CreateBuilder` is: `appsettings.json`, then `appsettings.{Environment}.json`, then environment variables, then command-line arguments. In the Development environment, user secrets are inserted after `appsettings.{Environment}.json` and before environment variables. Azure Key Vault is typically added last in the chain (by calling `AddAzureKeyVault` explicitly in `Program.cs`) so that vault-stored secrets take the highest precedence and override any placeholder values in JSON files. Understanding this hierarchy is critical for diagnosing why a configuration value is not what you expect: the correct debugging approach is to determine which provider "won" for a given key.

| Order | Provider | Typical Use |
|-------|----------|-------------|
| 1 | `appsettings.json` | Defaults, non-sensitive config |
| 2 | `appsettings.{Env}.json` | Environment-specific non-sensitive overrides |
| 3 | User Secrets | Development credentials (Dev only) |
| 4 | Environment variables | Container/server runtime config |
| 5 | Azure Key Vault | Production secrets (highest precedence) |
| 6 | Command-line args | One-off overrides |

---

## Q5. How do environment variables override appsettings.json values, and what naming convention maps nested JSON keys to environment variable names?

**Concepts**
- Double-underscore __ as cross-platform hierarchy separator
- Single colon valid on Windows only — not portable to Linux
- Prefix filter with AddEnvironmentVariables("PREFIX_") scoping
- IConfiguration case-insensitive key matching

**Answer**

The environment variables configuration provider reads all process environment variables and maps them into the configuration key-value store using a double-underscore (`__`) as the hierarchy separator, because a single colon (`:`) is not valid in environment variable names on Linux. A JSON structure like `{ "ConnectionStrings": { "Default": "..." } }` is overridden by an environment variable named `ConnectionStrings__Default`. On Windows, a single colon also works, but double underscore is the portable cross-platform convention. When running in a container, environment variables are the primary mechanism for injecting configuration because Dockerfiles and Kubernetes manifests both support setting them per deployment without modifying the image. A useful prefix filter can be applied: `AddEnvironmentVariables("MYAPP_")` causes the provider to strip the prefix and only import variables that begin with `MYAPP_`, preventing accidental reads of unrelated system variables. Overriding works because the `IConfiguration` abstraction flattens all providers into a single logical key space; whichever provider registered last for a given key wins. Care is needed with case sensitivity: on Linux, environment variable names are case-sensitive, whereas `IConfiguration` keys are case-insensitive by design, so `CONNECTIONSTRINGS__DEFAULT` and `ConnectionStrings__Default` resolve to the same configuration key.

---

## Q6. How do you integrate Azure Key Vault into an ASP.NET Core application's configuration pipeline?

**Concepts**
- Azure.Extensions.AspNetCore.Configuration.Secrets NuGet package
- AddAzureKeyVault with vault URI and DefaultAzureCredential
- Hyphen-to-colon key translation for nested configuration
- Read-only at startup — no automatic runtime secret refresh

**Answer**

Azure Key Vault is integrated by installing the `Azure.Extensions.AspNetCore.Configuration.Secrets` NuGet package and calling `builder.Configuration.AddAzureKeyVault(vaultUri, credential)` in `Program.cs`, where `vaultUri` is the vault's URI and `credential` is a `TokenCredential` instance such as `DefaultAzureCredential`. This call adds Azure Key Vault as the highest-priority configuration provider, so any secret stored in the vault with a matching name overrides values from `appsettings.json` or environment variables. Key Vault secret names use hyphens as the hierarchy separator because Key Vault does not allow colons or double underscores in secret names; the provider automatically translates hyphens to colons when mapping into `IConfiguration`, so a secret named `ConnectionStrings--Default` is accessible as `config["ConnectionStrings:Default"]`. The integration is read-only at startup by default: secrets are fetched once when the application starts and cached in the configuration system; if a secret is rotated in the vault, the running application does not automatically refresh unless additional polling or secret versioning logic is implemented. `DefaultAzureCredential` tries multiple authentication methods in order (managed identity, environment variables, Visual Studio credential, Azure CLI) and is the recommended choice because the same code works locally (via Azure CLI login) and in production (via managed identity) without any code changes. Secrets that are disabled or soft-deleted in Key Vault are silently skipped by the provider, which can cause missing-configuration errors at runtime if a secret is accidentally disabled.

---

## Q7. What is managed identity authentication, and why is it preferred over connection strings or client secrets when connecting to Azure Key Vault?

**Concepts**
- Azure AD service principal managed by the platform — no credentials in config
- Breaking the circular problem of "a secret to fetch secrets"
- System-assigned vs user-assigned managed identity trade-offs
- Key Vault Secrets User RBAC role for least-privilege access

**Answer**

Managed identity is an Azure Active Directory (AAD) feature that assigns an automatically managed service principal to an Azure resource — such as an App Service, Azure Function, or AKS pod — eliminating the need to store or rotate credentials manually. When an application running on that resource calls Azure Key Vault using `ManagedIdentityCredential` or `DefaultAzureCredential`, the Azure platform handles the token exchange internally; no password, certificate, or client secret appears in any configuration file. With a traditional client secret approach, the secret used to authenticate to Key Vault must itself be stored somewhere, creating a circular problem: you need a secret to fetch secrets. Managed identity breaks this cycle because the credential is handled at the infrastructure level. System-assigned managed identities have a one-to-one relationship with a specific Azure resource and are deleted automatically when the resource is deleted, reducing the orphaned-credential problem. User-assigned managed identities can be shared across multiple resources, which is useful when a fleet of services all need identical Key Vault access permissions. Azure Key Vault access policies or Azure RBAC roles such as `Key Vault Secrets User` must explicitly grant the managed identity permission to read secrets; granting the identity does not automatically expose all secrets, providing least-privilege access control.

---

## Q8. What is the ASP.NET Core Data Protection API, and what is its primary purpose?

**Concepts**
- IDataProtector.Protect / Unprotect — symmetric encrypt-then-decrypt
- Purpose strings providing application-level key isolation
- Not for long-term storage — designed for bounded-lifetime data
- AddDataProtection registering the service

**Answer**

The Data Protection API provides a simple, high-level interface for protecting and unprotecting arbitrary byte arrays or strings using symmetric cryptography. Its primary purpose is to secure short-lived data that the application itself will later unprotect — such as authentication cookies, CSRF tokens, and "remember me" tokens — without requiring developers to manage raw cryptographic primitives. The API is used internally by ASP.NET Core's cookie authentication middleware. The API exposes two main operations: `protector.Protect(plaintext)` produces an opaque encrypted blob, and `protector.Unprotect(ciphertext)` reverses it; both operations require the same logical key ring. Purpose strings (passed at protector creation time) provide application-level isolation: data protected under one purpose string cannot be unprotected using a protector created under a different purpose, even if they share the same underlying key ring. This isolation ensures that a cookie authentication protector cannot accidentally unprotect a CSRF token, even though both use the same key ring. Data Protection is not a general-purpose encryption library for long-term data storage; it is designed for data that needs to be protected for a bounded period and will be unprotected by the same application. The `IDataProtectionProvider` service is registered by calling `builder.Services.AddDataProtection()` in `Program.cs`, after which it can be injected anywhere in the application.

---

## Q9. What is a Data Protection key ring, and how does key lifetime affect the ability to unprotect previously protected payloads?

**Concepts**
- Key ring — active + retired + revoked keys persisted together
- 90-day default key lifetime — new key generated on expiry
- Retired keys retained for decryption of old payloads
- Multi-server deployments requiring shared key ring storage

**Answer**

The key ring is the collection of all Data Protection keys — active, retired, and revoked — that the application maintains across its lifetime. When a new payload is protected, it is encrypted using the current active key; when a payload is unprotected, the API inspects the key identifier embedded in the ciphertext and looks up the corresponding key in the ring, meaning old keys must be retained to decrypt old data. The default key lifetime is 90 days, after which a new key is generated and becomes active, but old keys remain in the ring. Keys are never deleted automatically by the runtime; they remain in the ring in a retired state so that previously protected payloads created under those keys can still be unprotected after the key has aged out. If the key ring is lost — for example, because it was stored only in the application's temporary file system and the server was replaced — all previously protected payloads become permanently unreadable, including active authentication cookies, which logs out all users. The key ring is also used to propagate new keys across all instances in a multi-server deployment; all instances must share the same key ring storage location (such as Azure Blob Storage or a shared file path) or they will fail to unprotect each other's payloads. Explicitly revoking a key prevents new payloads from being protected under it but also prevents existing payloads from being unprotected, so revocation should be used only when the key is believed to be compromised.

---

## Q10. What are the key storage locations available for Data Protection keys, and when would you choose each one?

**Concepts**
- File system default — sufficient for single-server, must persist across restarts
- Azure Blob Storage for horizontally scaled Azure deployments
- Redis for multi-instance deployments with an existing Redis cluster
- Entity Framework Core for apps with an existing relational database

**Answer**

ASP.NET Core Data Protection supports several key storage back-ends: the local file system (default), Azure Blob Storage, Redis, a database via Entity Framework Core, and the Windows registry. The choice depends on the deployment topology and the need for key sharing across multiple application instances.

| Storage | Use Case |
|---------|----------|
| File system | Single-server deployments, development |
| Azure Blob Storage | Multi-instance Azure App Service or AKS with Azure infrastructure |
| Redis | Multi-instance deployments already using Redis for distributed caching |
| Entity Framework Core | Applications with an existing relational database wanting key durability |
| Windows Registry (DPAPI) | Windows-only single-machine scenarios needing OS-level protection |

In a single-server deployment, the default file system storage is sufficient, but the directory must persist across application restarts. In any horizontally scaled deployment, all instances must point to the same external storage location; failure to do so causes intermittent decryption failures because an instance that did not create a particular key cannot unprotect data encrypted by it. Azure Blob Storage is configured with `PersistKeysToAzureBlobStorage(blobClient)` and is the natural choice for applications already running on Azure because it is durable, replicated, and integrates cleanly with managed identity. Redis storage via `PersistKeysToStackExchangeRedis` is a good option when latency is critical and a Redis cluster is already present in the architecture.

---

## Q11. How do you encrypt Data Protection keys at rest, and what three mechanisms does ASP.NET Core offer?

**Concepts**
- Windows DPAPI — OS-bound, machine or user account, Windows only
- X.509 certificate — portable across machines, requires private key distribution
- Azure Key Vault key wrapping — root key never leaves Azure
- Key storage (where) and key encryption (how) as orthogonal concerns

**Answer**

Data Protection keys are stored in XML files by default, and without additional configuration those files contain the key material in a form that can be exploited if the storage location is compromised. Encrypting keys at rest means wrapping the cryptographic key material inside each XML file using a higher-level key so that compromising the key storage location does not immediately expose the underlying keys. ASP.NET Core offers three built-in mechanisms: Windows DPAPI, X.509 certificates, and Azure Key Vault. `ProtectKeysWithDpapi()` uses Windows DPAPI to encrypt key material using the current machine or user account's identity; it requires no certificate management but is Windows-only and machine-bound, making it unsuitable for multi-server or container deployments. `ProtectKeysWithCertificate(certificate)` uses an X.509 certificate's public key to encrypt the Data Protection keys; any instance that has the certificate's private key can decrypt them, making this mechanism portable across machines and operating systems. `ProtectKeysWithAzureKeyVault(keyIdentifier, credential)` uses an Azure Key Vault key (not a secret) to wrap the Data Protection key material using the Key Vault's key wrapping APIs; the actual wrapping and unwrapping happens inside the vault so the root key never leaves Azure, providing the strongest security posture. These mechanisms protect only the key storage — the per-payload encryption is still performed locally using the Data Protection key; Azure Key Vault is not involved in every protect/unprotect call, only during key ring loading.

---

## Q12. What is the ProtectKeysWithAzureKeyVault method, and how does it differ from simply storing keys in Azure Key Vault?

**Concepts**
- Key Vault key (KEK) wrapping XML at rest vs storing XML as a Key Vault secret
- PersistKeysToAzureBlobStorage + ProtectKeysWithAzureKeyVault as canonical Azure pattern
- Key Vault Crypto User role for key wrapping operations
- Root key material never leaving Azure's control plane

**Answer**

`ProtectKeysWithAzureKeyVault` configures the Data Protection system to use an Azure Key Vault key (an RSA or EC cryptographic key, not a Key Vault secret) to wrap (encrypt) the XML key ring files at rest, while the key ring files themselves are stored elsewhere — typically in Azure Blob Storage. This is different from storing the keys in Key Vault as secrets: the key ring XML is never placed inside Key Vault; instead, Key Vault acts solely as a key encryption key (KEK) service. The distinction matters because Key Vault secrets are arbitrary text values, whereas Key Vault keys are managed cryptographic objects with hardware security module (HSM) backing options; using a Key Vault key for wrapping means the root key material never leaves Azure's control plane. When a new Data Protection key is generated, the application calls Key Vault's `wrapKey` API to encrypt the raw key bytes; when loading the ring, it calls `unwrapKey`. The application's own code never holds the unwrapped KEK. Combining `PersistKeysToAzureBlobStorage` with `ProtectKeysWithAzureKeyVault` is the canonical Azure deployment pattern: Blob Storage provides durable shared key ring storage, and Key Vault provides encryption-at-rest for those key files. Access to the Key Vault key for unwrapping requires that the application's managed identity hold the `Key Vault Crypto User` role (or equivalent key permissions), separate from the `Key Vault Secrets User` role used for configuration secrets.

---

## Q13. What is an X.509 certificate, and how is it used for signing JSON Web Tokens (JWTs) in ASP.NET Core?

**Concepts**
- Certificate binding public key to identity, signed by CA
- Private key signing / public key verification — asymmetric model
- JWKS endpoint publishing the public key for verifiers
- Asymmetric signing preferred over HS256 in multi-service systems

**Answer**

An X.509 certificate is a digital document that binds a public key to an identity (such as a server name or organization) and is signed by a Certificate Authority (CA) to prove its authenticity. In the context of JWT signing, the certificate's private key is used to sign the token (creating an RS256 or ES256 signature), and the certificate's public key allows any relying party to verify the signature without possessing the private key. In ASP.NET Core, an identity provider such as Duende IdentityServer uses `AddSigningCredential(certificate)` to configure the RSA private key derived from the certificate for token signing; the corresponding public key is published at the JWKS (JSON Web Key Set) endpoint. Consumers of the JWT configure `TokenValidationParameters` with the certificate's public key or with the JWKS URI so that the JWT middleware can verify signatures on incoming tokens. Asymmetric signing (certificate-based RS256) is strongly preferred over symmetric signing (HMAC-SHA256) in systems with multiple services, because the private key stays with the identity provider and the public key can be freely distributed to any number of verifying services. The certificate used for JWT signing must be kept secure on the identity provider server; if the private key is compromised, an attacker can forge tokens for any identity in the system.

---

## Q14. What is the difference between using a certificate stored in the Windows certificate store versus a file-based .pfx or .pem file?

**Concepts**
- Windows certificate store — OS-managed, non-exportable private key option
- .pfx (PKCS#12) bundling certificate and private key in a password-protected file
- Linux / container environments requiring file-based formats
- Non-exportable keys preventing raw key byte extraction even with code execution

**Answer**

The Windows certificate store (accessed via `StoreName` and `StoreLocation` enumerations) is a system-managed repository where certificates and their associated private keys are stored with OS-level access controls; private keys can be marked as non-exportable, meaning the raw key bytes can never be read out of the store even by an administrator. File-based formats like `.pfx` (PKCS#12, which bundles certificate and private key) and `.pem` (base-64 encoded DER data, sometimes split into separate cert and key files) store the key material directly on disk, often protected only by a password.

| Aspect | Windows Certificate Store | .pfx / .pem File |
|--------|--------------------------|------------------|
| Platform | Windows only | Cross-platform |
| Private key protection | OS-managed, non-exportable option | Password-protected file |
| Access control | Windows ACLs per key | File system permissions |
| Container use | Not available in Linux containers | Standard approach |

In ASP.NET Core, a certificate is loaded from the Windows store using `X509Store` and queried by thumbprint or subject name; a `.pfx` file is loaded with `new X509Certificate2(path, password)`. On Linux and in containers, the Windows certificate store does not exist, so `.pfx` or `.pem` files are the only options; the password protecting the `.pfx` must itself be managed as a secret (e.g., via an environment variable or Key Vault). The Windows certificate store's non-exportable key protection is a meaningful security control: even if an attacker gains code execution on the machine, they cannot extract the raw private key bytes — they can only use the key through the Windows CryptAPI.

---

## Q15. How does ASP.NET Core Kestrel load an HTTPS certificate, and what configuration options are available?

**Concepts**
- ConfigureHttpsDefaults and per-endpoint UseHttps accepting X509Certificate2
- Kestrel:Endpoints appsettings.json certificate configuration
- Certificate password as a secret — injected through env vars or Key Vault
- SNI support for multiple certificates per port

**Answer**

Kestrel, ASP.NET Core's built-in web server, loads HTTPS certificates through `KestrelServerOptions.ConfigureHttpsDefaults` or per-endpoint `UseHttps` configuration, which can accept a certificate directly as an `X509Certificate2`, a path to a `.pfx` file with a password, or a reference to the certificate subject in the Windows certificate store. Kestrel can also pick up certificate configuration from `appsettings.json` under the `Kestrel:Endpoints` section, allowing certificate paths and passwords to be set without code changes. The `appsettings.json` approach is the most flexible for deployment: the `Certificate` subsection accepts `Path` and `Password` keys (or `Subject` and `Store` for certificate store lookups), allowing different certificates per environment without recompilation. Certificate passwords referenced in `appsettings.json` are themselves secrets and must be injected through environment variables or Key Vault rather than hardcoded in the file. Kestrel supports SNI (Server Name Indication), allowing different certificates to be served to different hostnames on the same port by configuring multiple named endpoints with their own certificate settings. When `ASPNETCORE_URLS` specifies an `https://` binding but no certificate is configured, Kestrel falls back to the ASP.NET Core development certificate if it exists; in production this fallback should be disabled to prevent accidental use of an untrusted certificate.

---

## Q16. What is the dotnet dev-certs command, and what does it do to enable HTTPS during local development?

**Concepts**
- Self-signed certificate generated and trusted into OS trust store
- --trust flag installing to browser-trusted keychain
- Never to be used outside localhost — not CA-signed
- Linux requiring manual steps for Firefox's separate trust store

**Answer**

`dotnet dev-certs https` generates a self-signed X.509 certificate specifically for local HTTPS development and installs it into the current user's certificate trust store so that browsers and the .NET HTTPS client trust it without security warnings. Running `dotnet dev-certs https --trust` performs both generation and trust installation. This is the standard way to enable `https://localhost` on a development machine without purchasing a CA-signed certificate. The development certificate is stored at a well-known location and picked up automatically by Kestrel in the Development environment; no explicit Kestrel certificate configuration is needed for local HTTPS to work. The certificate is self-signed — it is not issued by a recognized Certificate Authority and is therefore not trusted by browsers or operating systems by default; `--trust` adds it to the OS trust store to resolve this. On macOS and Linux, `--trust` modifies the system keychain or NSS database; on Linux the process may require manual steps because Firefox maintains its own trust store and dotnet cannot update it automatically. The development certificate should never be used in any environment beyond the developer's local machine; CI servers and staging/production environments must be configured with real CA-signed certificates or certificates from Let's Encrypt.

---

## Q17. How are secrets managed in Docker containers, and what are the risks of baking secrets into a Docker image?

**Concepts**
- Docker image layer immutability — deleted secrets recoverable from history
- Environment variables as the simplest runtime injection mechanism
- Docker Secrets (Swarm mode) mounting secrets at /run/secrets/<name>
- Image scanning tools (Trivy, Snyk) detecting hardcoded secrets in layers

**Answer**

In Docker, secrets are typically injected at container startup through environment variables, Docker Secrets (for Docker Swarm), or mounted volume files — none of which require the secret to be in the image itself. Baking a secret into a Docker image by embedding it in a `Dockerfile` (via `ENV`, `COPY`, or `RUN` instructions) is dangerous because Docker image layers are immutable and cumulative; even if a subsequent layer appears to overwrite or delete a secret, the original layer containing the secret remains in the image's history and can be extracted with `docker history` or `docker save`. Environment variables are the simplest injection mechanism: `docker run -e ConnectionStrings__Default="..." myimage` passes the value to the container without embedding it in the image, and the value is readable from `IConfiguration` via the standard environment variables provider. Docker Secrets (available in Swarm mode) mount secrets as files at `/run/secrets/<name>` inside the container; the application reads the file contents rather than an environment variable, which avoids secret exposure via `docker inspect`. In production, secrets should never be passed directly in `docker run` commands typed in a shell (they appear in shell history); they should come from an orchestration platform such as Kubernetes, which can inject them from a secrets store. Scanning tools such as Trivy, Snyk, and Docker Scout can detect hardcoded secrets in image layers, making it practical to enforce a no-secrets-in-images policy in CI pipelines.

---

## Q18. How does Kubernetes manage sensitive configuration data, and what are the security limitations of Kubernetes Secrets?

**Concepts**
- Kubernetes Secret resource — base-64 encoded, not encrypted by default
- etcd encryption at rest as a separate cluster-level configuration
- Secrets Store CSI Driver for external vault integration
- RBAC gating access to Secret resources

**Answer**

Kubernetes provides a `Secret` resource type that stores base-64-encoded key-value pairs and can be injected into pods as environment variables or mounted as files. The Kubernetes API server gates access to Secret resources via RBAC, so not every service account in the cluster can read every secret. Despite the resource name, Kubernetes Secrets are not encrypted by default — they are stored as base-64-encoded plain text in etcd unless etcd encryption at rest is explicitly configured on the cluster. Base-64 encoding is encoding, not encryption; any process that can access the etcd data store (or use `kubectl get secret -o yaml`) can trivially decode a Kubernetes Secret's value with a base-64 decoder. Enabling etcd encryption at rest using an `EncryptionConfig` manifest secures secrets on disk but does not prevent a cluster administrator with `kubectl` access from reading them; encryption at rest protects against physical storage compromise, not insider threat. The recommended production pattern is to use an external secrets manager such as HashiCorp Vault, AWS Secrets Manager, or Azure Key Vault and use the Secrets Store CSI Driver or an operator to project vault-stored secrets into pods at runtime, so the actual secret values never reside in etcd.

---

## Q19. How do you store and consume secrets in a GitHub Actions workflow without hardcoding them in the YAML file?

**Concepts**
- Repository or organization secrets set in GitHub Settings — write-once
- ${{ secrets.SECRET_NAME }} syntax injecting values as environment variables
- Automatic log masking preventing accidental secret exposure
- Environment-scoped secrets with approval gates for production deployments

**Answer**

GitHub Actions secrets are stored in the repository's or organization's Settings under "Secrets and variables" and are injected into workflow jobs as environment variables using the `${{ secrets.SECRET_NAME }}` expression syntax. They are never echoed in workflow logs — GitHub automatically masks any output that matches the value of a configured secret. Secrets are set once through the GitHub UI or via the GitHub CLI (`gh secret set`) and are never visible after creation — not even to repository admins — which enforces a write-once secret model. In a workflow YAML, a secret is referenced as an environment variable: `env: MY_KEY: ${{ secrets.MY_KEY }}`, and the value is then accessible to the job's steps using the standard environment variable syntax. The secrets are available only to workflows triggered from within the repository and are not exposed to pull requests from forked repositories by default. Environment-scoped secrets (configured under Environments such as "production" or "staging") add an additional approval gate: a workflow deploying to the "production" environment must receive a manual approval from a reviewer before the environment's secrets are released to the job. For secrets that rotate frequently or must be centrally managed across many repositories, linking GitHub Actions to Azure Key Vault via OIDC (OpenID Connect) federated identity is a better pattern than duplicating the secret in every repository.

---

## Q20. How do Azure DevOps variable groups work, and how do you link them to Azure Key Vault for secret management?

**Concepts**
- Variable groups as shared named collections across multiple pipelines
- Key Vault linking — secrets fetched at pipeline run time, not stored in DevOps
- Secret refresh on each pipeline run via vault link
- Environment-scoped variable groups with approval gates

**Answer**

Azure DevOps variable groups are named collections of pipeline variables that can be shared across multiple pipelines in a project, avoiding duplication of values in every pipeline definition. When a variable group is linked to an Azure Key Vault, Azure DevOps fetches the specified secrets from the vault at pipeline run time and exposes them as pipeline variables; the secrets are never stored in Azure DevOps itself, only referenced. Variables marked as secret in a group are masked in pipeline logs. To link a variable group to Azure Key Vault, an Azure service connection with appropriate Key Vault access must first be configured; then the variable group is created with the "Link secrets from an Azure key vault" option, and individual secrets are selected from the vault for inclusion. The secrets are refreshed from Key Vault at each pipeline run, meaning a secret rotation in Key Vault is automatically picked up the next time the pipeline executes without any pipeline configuration change. Pipeline variables (including variable group values) are accessible in YAML pipelines using `$(VARIABLE_NAME)` syntax; they can also be mapped to environment variables for scripts using the `env:` block. Variable groups scoped to an environment (used in deployment jobs) can be combined with approval gates, requiring a human reviewer to authorize a deployment before the pipeline variables are materialized in the job context.

---

## Q21. How do you rotate a secret — such as a database connection string — in a live production application without causing downtime?

**Concepts**
- Three-phase rotation: add new credentials, migrate app config, revoke old credentials
- Old and new credentials valid simultaneously during overlap window
- Azure Key Vault secret versioning enabling rollback
- IOptionsMonitor or IConfigurationRoot.Reload for runtime config refresh

**Answer**

Zero-downtime secret rotation requires that both the old and new secret values are valid simultaneously for a brief overlap period, so that instances running the old configuration can continue operating while new instances pick up the new value. For a database password, this means creating the new password on the database server first, then updating the secret in the configuration store (Key Vault, environment variable, etc.), then allowing all application instances to reload their configuration before removing the old password from the database. The standard pattern involves three phases: "add new" (create new credentials alongside old), "migrate" (update the application config to use new credentials and wait for all instances to reload), and "remove old" (revoke the old credentials after confirming no instances are using them). Azure Key Vault supports secret versioning natively: each time a secret is updated a new version is created and the previous version remains accessible by version ID; this allows rollback if a rotation reveals a problem. Applications using `IConfiguration` with Azure Key Vault do not automatically reload secrets after startup; to support runtime refresh without restart, the `IOptionsMonitor<T>` pattern or a periodic `IConfigurationRoot.Reload()` call must be implemented. Blue-green deployments can simplify rotation: traffic is shifted to the new deployment (which uses the new secret) only after it is confirmed healthy, and the old deployment is torn down only after all traffic has drained.

---

## Q22. How do you rotate Data Protection keys without invalidating all existing protected payloads?

**Concepts**
- Automatic rotation at key expiry — no manual intervention required
- Retired keys retained in ring for decryption of old payloads
- IKeyManager.RevokeKey — destructive, for compromised keys only
- SecurityStamp as a safer per-user session invalidation mechanism

**Answer**

Data Protection key rotation is handled automatically by the framework — when the active key reaches its expiration (90 days by default), a new key is generated and becomes active for new protect operations, while the old key is retained in the key ring in a retired state so that existing payloads can still be unprotected. This means normal key rotation requires no manual intervention and causes no downtime. Intentional early rotation can be triggered by calling `IKeyManager.CreateNewKey` and setting the current active key's expiration. The key ring's XML files always contain both active and retired keys; as long as these files are preserved, any payload encrypted under any past key can be unprotected indefinitely. Revoking a key (via `IKeyManager.RevokeKey`) is a destructive operation distinct from expiration: a revoked key is flagged as untrusted and the framework will refuse to unprotect payloads created under it, immediately invalidating all sessions and tokens tied to that key — use revocation only when a key is believed to be compromised. To force all users to re-authenticate after a security incident without full key revocation, cookie authentication's `SecurityStamp` mechanism is a better tool: changing the `SecurityStamp` invalidates all cookies for a specific user without touching the key ring. Very short key lifetimes (minutes) are impractical because key ring refresh across a multi-server fleet may lag behind the key expiration.

---

## Q23. What is the difference between symmetric and asymmetric key usage in the context of JWT signing, and when would you choose each approach?

**Concepts**
- HS256 — shared secret, any verifier can also forge tokens
- RS256 / ES256 — private key signs, public key distributed freely
- JWKS endpoint enabling automatic public key discovery
- ES256 preferred over RS256 — shorter signatures, smaller key sizes

**Answer**

Symmetric JWT signing (HS256 — HMAC with SHA-256) uses a single shared secret key for both signing and verification; any party that can verify a token can also create a valid token, because they possess the same key. Asymmetric signing (RS256 — RSA with SHA-256, or ES256 — ECDSA with SHA-256) uses a key pair: the private key signs the token, and the public key — which can be freely distributed — verifies it. Only the holder of the private key can produce valid tokens.

| Aspect | Symmetric (HS256) | Asymmetric (RS256 / ES256) |
|--------|-------------------|---------------------------|
| Key sharing | All verifiers need the secret | Public key only needed to verify |
| Token forgery risk | Any verifier can forge tokens | Only private key holder can forge |
| Operational complexity | Simple, one key | Certificate/key pair management |
| Best for | Single-service, trusted parties | Multi-service, public verifiers |

Symmetric signing is appropriate for a single application where the same service both issues and verifies tokens, because the secret never leaves the service boundary. Asymmetric signing is the correct choice in any system where multiple independent services verify tokens but should not be able to issue them — for example, a centralized identity provider issuing tokens that microservices verify without coordinating with the identity provider on every request. The public key can be published at a standard JWKS (JSON Web Key Set) URI, allowing any consumer to discover and cache the verification key without pre-configuration. ES256 (ECDSA) is generally preferred over RS256 (RSA) for new systems because it produces shorter signatures and offers equivalent security at smaller key sizes.

---

## Q24. What happens if you lose the Data Protection key ring in a production application, and how should you prevent this?

**Concepts**
- Permanent payload unrecoverability — all cookies, CSRF tokens, remember-me tokens invalidated
- Ephemeral container storage as common cause of key ring loss
- PersistKeysTo* extension method required for durability
- Durability (where) and encryption-at-rest (how) as orthogonal concerns

**Answer**

If the key ring is permanently lost, all previously protected payloads are irrecoverable because the symmetric keys used to encrypt them no longer exist. In practice this means all active authentication cookies immediately become invalid (all users are logged out), any "remember me" tokens are worthless, CSRF tokens cannot be unprotected, and any application data that was encrypted using the Data Protection API is permanently corrupted. The application itself will continue to function — it generates a new key ring from scratch — but the data protected under the old ring is gone. The most common cause of key ring loss in cloud environments is ephemeral storage: by default, if no explicit `PersistKeysTo*` call is made, Kestrel writes keys to a temporary directory that is wiped when the container or app service instance is recycled. Prevention requires persisting the key ring to durable external storage (Azure Blob, Redis, a database) using the appropriate `PersistKeysTo*` extension method in `Program.cs`; this must be done before the first deployment to production. Encrypting keys at rest with `ProtectKeysWithAzureKeyVault` or `ProtectKeysWithCertificate` is important but does not address durability; durability (where keys are stored) and encryption at rest (how keys are protected while stored) are orthogonal concerns that must both be configured. Regular backups of the key ring storage location provide an additional recovery path in case of accidental deletion.

---

## Q25. How does certificate-based authentication differ from token-based authentication, and when is a certificate the better choice?

**Concepts**
- mTLS — client certificate presented during TLS handshake
- Bearer token — short-lived signed string presented in HTTP header
- mTLS replay prevention — private key bound to TLS connection, not portable
- AddCertificate() for ASP.NET Core mutual TLS authentication

**Answer**

Certificate-based (mutual TLS — mTLS) authentication requires the client to present a valid X.509 certificate during the TLS handshake; the server verifies the certificate chain and optionally the client's identity through the certificate's subject or thumbprint. Token-based authentication (JWT or OAuth 2.0) uses a short-lived, signed bearer token presented in an HTTP header; no client-side cryptographic material is required beyond knowledge of the token string. Certificates provide stronger non-repudiation because the private key never leaves the client, whereas a bearer token can be used by anyone who intercepts it. Certificate-based authentication is the preferred choice in machine-to-machine (service mesh, API gateway) scenarios where both parties are infrastructure components that can be provisioned with certificates at deployment time — this is the basis of service meshes like Istio. Bearer token authentication is generally simpler to implement for user-facing applications because issuing and refreshing tokens does not require the client to manage a private key. mTLS provides inherent replay protection because the TLS handshake ties the certificate to a specific connection; a stolen token can be replayed from any network location until it expires. In ASP.NET Core, certificate authentication is configured with `AddCertificate()` in the authentication pipeline and requires that the server's TLS termination point (Kestrel or a reverse proxy) is configured to request and forward client certificates.

---

## Q26. What are the security risks of using environment variables to pass secrets in containers, and what mitigations exist?

**Concepts**
- /proc/<pid>/environ exposing env vars to other processes in the container
- docker inspect revealing environment variables to anyone with Docker socket access
- File-based secret mounting (volume mounts) as a more defensible alternative
- Secrets Store CSI Driver for external vault-projected secrets in Kubernetes

**Answer**

While environment variables avoid baking secrets into the image, they introduce their own risks: environment variables are visible to any process running inside the container, appear in crash dumps, can be exfiltrated via `docker inspect` by users with Docker socket access, and are often logged by poorly configured application frameworks or monitoring agents. In Kubernetes, environment variable values sourced from Secrets are visible in the pod spec to anyone with sufficient RBAC permissions. A process with access to `/proc/<pid>/environ` on Linux can read all environment variables of any process it has permission to inspect; within a container all processes run in the same namespace, so a compromised sidecar or init container could read the main process's environment. Mitigation options include using file-based secrets (mounted volumes) instead of environment variables, using a secrets sidecar (such as Vault Agent) that writes secrets to a memory-mapped file rather than an environment variable, and using the Kubernetes Secrets Store CSI Driver to mount secrets directly from an external vault. Ensuring that application logging frameworks are configured to redact or exclude environment variables from structured log output prevents secrets from reaching log aggregation systems. Limiting Docker socket access (`/var/run/docker.sock`) and restricting the `kubectl get secret` RBAC permission to only privileged service accounts reduces the blast radius if a host or cluster is partially compromised.

---

## Q27. What common interview mistake do developers make when describing the security of ASP.NET Core user secrets?

**Concepts**
- "Secret Manager" name implying encryption — misconception to avoid
- Plain-text JSON file accessible to any process with user profile access
- Convenience tool, not a security control
- Production equivalents: Azure Key Vault, AWS Secrets Manager, HashiCorp Vault

**Answer**

The most common mistake is claiming that user secrets are encrypted or secure for general use. Developers hear "Secret Manager" and assume it functions like a secrets vault with encryption, access control, and audit logging. In reality, user secrets are stored as plain-text JSON in the user profile directory; the only security property they provide is keeping secrets out of the source repository. They offer no protection against other processes on the same machine reading the file. The correct mental model is that the Secret Manager is a developer convenience tool, not a security control: it solves the accidental commit problem, not the unauthorized access problem. Stating that user secrets "are not encrypted" directly in an answer demonstrates accurate understanding of the trade-offs and avoids the common overstatement that interviewers specifically listen for. A complete answer also notes that the file is stored per-user-secrets-ID in the user profile, outside the project directory, which is why it cannot accidentally be included in a `git add .` operation. Production equivalents that do provide encryption and access control are Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, and Windows DPAPI — candidates who name these alternatives alongside the limitation of user secrets show a clear understanding of the full secrets management landscape.

---

## Gotchas — Secrets & Key Management (Interview Traps)

---

## Gotcha 1. User Secrets Are Not Encrypted

**Concepts**
- "Secret Manager" name suggesting encryption — incorrect assumption
- Plain-text JSON on disk, accessible to local processes
- Development convenience only — not production-safe

**Answer**

Developers often conflate the name "Secret Manager" with encrypted storage, but user secrets are stored as plain-text JSON on disk; the only protection is that the file lives outside the project directory, preventing accidental source control inclusion. The wrong mental model leads to statements like "user secrets are safe to use in production" — they are not; the file can be read by any process with access to the user profile. The correct answer is that user secrets are a development convenience only, and production secrets must use an encrypted, access-controlled store such as Azure Key Vault.

---

## Gotcha 2. Deleting a Data Protection Key Does Not Decrypt Old Payloads

**Concepts**
- Key revocation making old ciphertext permanently unreadable
- Expired keys retained in ring — natural rotation without data loss
- Revocation reserved for compromised keys only

**Answer**

Developers sometimes think that revoking or deleting a Data Protection key is a way to "clean up" the key ring, not realizing that any payload encrypted under that key — including live authentication cookies — immediately becomes permanently unreadable. Revoking a key is analogous to deleting an encryption password: the ciphertext does not become plaintext, it becomes unrecoverable garbage. The correct cleanup approach is to let old keys expire naturally; expired (not revoked) keys remain in the ring and continue to allow unprotection of old data while no longer being used for new protect operations.

---

## Gotcha 3. Kubernetes Secrets Are Not Encrypted by Default

**Concepts**
- Base-64 encoding vs encryption — easily decoded without a key
- etcd encryption at rest as a separate opt-in cluster configuration
- Secrets Store CSI Driver as the production-grade alternative

**Answer**

The resource is named "Secret," which implies protection, but Kubernetes Secrets are stored as base-64-encoded plain text in etcd unless etcd encryption at rest is explicitly enabled on the cluster. Base-64 is an encoding scheme, not encryption — `echo "cGFzc3dvcmQ=" | base64 -d` immediately reveals the value to anyone with read access to the Secret resource. The correct answer notes that real protection requires either etcd encryption at rest or using an external secrets manager (Vault, Key Vault) with the Secrets Store CSI Driver.

---

## Gotcha 4. Environment Variable Secrets Are Visible to All Processes in the Container

**Concepts**
- /proc/<pid>/environ readable by processes in the same container namespace
- Sidecar or init container compromise exposing environment variables
- File-based mounting with restricted permissions as a more defensive pattern

**Answer**

Passing secrets via environment variables is safer than baking them into the image, but environment variables are not private within the container; any process running inside the same container namespace can read them from `/proc/<pid>/environ`. This matters in containers with multiple processes (init systems, supervisord) or sidecar injection patterns: a compromised secondary process can read the main application's secrets. File-based secret mounting (Docker Secrets or Kubernetes volume mounts from a CSI driver) is a more defensible pattern because file permissions can restrict access to only the specific process user.

---

## Gotcha 5. ProtectKeysWithAzureKeyVault Does Not Store the Key Ring in Key Vault

**Concepts**
- Key Vault as KEK provider only — XML key ring stored externally
- PersistKeysToAzureBlobStorage required for key ring durability
- Both calls required together for complete production Data Protection setup

**Answer**

Developers frequently assume that calling `ProtectKeysWithAzureKeyVault` stores the Data Protection key ring inside Azure Key Vault, but it only uses a Key Vault key to encrypt (wrap) the key ring XML; the XML itself must be stored separately, typically in Azure Blob Storage via `PersistKeysToAzureBlobStorage`. Omitting `PersistKeysToAzureBlobStorage` means the key ring is written to the default local file system path and is lost when the container or app instance is recycled — the key wrapping configuration is irrelevant if the wrapped keys themselves are not persisted durably. The complete production configuration combines both calls: `PersistKeysToAzureBlobStorage` for durability and `ProtectKeysWithAzureKeyVault` for encryption at rest, and both require appropriate managed identity permissions.
