# Azure Blob Storage — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [What is Azure Blob Storage, and what types of workloads is it designed for?](#q1)
2. [Explain the Azure Storage hierarchy: storage account, container, and blob.](#q2)
3. [What are the four Azure Storage services, and when would you choose Blob Storage…](#q3)
4. [What is the difference between a block blob, a page blob, and an append blob?](#q4)
5. [What redundancy options exist for a storage account that hosts blob data?](#q5)
6. [What is a blob container, and what naming rules apply to containers and blob nam…](#q6)
7. [What are the three blob container public access levels, and what does each allow…](#q7)
8. [What is the difference between Blob (anonymous read access for blobs only) and C…](#q8)
9. [What is blob-level public access, and how does it relate to container access set…](#q9)
10. [How do you create a container programmatically with the .NET SDK, and when shoul…](#q10)
11. [What NuGet package provides the modern Azure Blob Storage client for .NET, and w…](#q11)
12. [How do you register `BlobServiceClient` in ASP.NET Core dependency injection, as…](#q12)
13. [What is the difference between `BlobServiceClient`, `BlobContainerClient`, and `…](#q13)
14. [How do you authenticate to Blob Storage from a .NET application without embeddin…](#q14)
15. [What components make up a blob storage connection string, and what does each par…](#q15)
16. [How does `UploadAsync` work in this module's `FileService`, and what are its lim…](#q16)
17. [What is the difference between a simple `UploadAsync` call and a staged block-bl…](#q17)
18. [How do you download a blob as a stream without loading the entire file into memo…](#q18)
19. [Why are block blobs the default choice for streaming uploads and large files in …](#q19)
20. [How would you set Content-Type and blob metadata when uploading from an ASP.NET …](#q20)
21. [What is a Shared Access Signature (SAS) token, and what blob permissions can it …](#q21)
22. [What is the difference between an account SAS, a service SAS, and a user delegat…](#q22)
23. [When would you return a SAS URL to a client instead of proxying the file through…](#q23)
24. [What are best practices for storing storage account keys and connection strings …](#q24)
25. [How does Azure Role-Based Access Control (RBAC) complement or replace SAS tokens…](#q25)
26. [What are the Hot, Cool, and Archive access tiers, and how do they affect cost an…](#q26)
27. [What is blob lifecycle management, and give an example policy for tiering or del…](#q27)
28. [What are blob soft delete and blob versioning, and why enable them in production…](#q28)

---

## Q1. What is Azure Blob Storage, and what types of workloads is it designed for?

What is Azure Blob Storage, and what types of workloads is it designed for?

**Answer:** Azure Blob Storage is Microsoft's object storage service for unstructured data — files such as images, videos, documents, backups, and log archives — that you access over HTTP or HTTPS using a flat namespace of named blobs rather than a hierarchical file system.

- It is optimized for storing large binary objects that many clients or services read and write independently, which is why web apps, mobile backends, and data lakes use it for user uploads, static assets, and analytics landing zones.
- Blobs scale out to petabytes per storage account and are billed primarily on stored capacity, transactions, and egress, making it cost-effective for infrequently accessed archives as well as hot content delivery.
- Unlike relational databases, blob storage does not enforce schemas or support queries over blob contents; you store opaque bytes and retrieve them by name, optionally with metadata and tags for organization.

---

## Q2. Explain the Azure Storage hierarchy: storage account, container, and blob.

Explain the Azure Storage hierarchy: storage account, container, and blob.

**Answer:** A storage account is the top-level Azure resource that holds your data and exposes endpoints; inside it, containers group related blobs the way folders group files, and each blob is an individual stored object identified by name within exactly one container.

- The storage account name is globally unique across Azure and determines the base URL (for example `https://myaccount.blob.core.windows.net`).
- Containers live directly under the account and cannot nest inside other containers — the hierarchy is strictly account → container → blob, though blob names can include `/` characters to simulate virtual folders.
- Each blob has a type (block, page, or append), optional metadata, optional tags, an access tier, and a unique URL formed from the account endpoint, container name, and blob name.

---

## Q3. What are the four Azure Storage services, and when would you choose Blob Storage over the others?

What are the four Azure Storage services, and when would you choose Blob Storage over the others?

**Answer:** Azure Storage provides four data services — Blob (object storage), File (SMB/NFS shares), Queue (message queues), and Table (NoSQL key-value) — and you choose Blob Storage when you need scalable HTTP-accessible object storage for unstructured binary or text files.

| Service | Primary use | Typical access pattern |
|---|---|---|
| Blob | Images, videos, backups, data lake files | REST/SDK; flat namespace of objects |
| File | Lift-and-shift file shares, legacy apps needing SMB | Mount as network drive |
| Queue | Decouple producers and consumers with messages | Enqueue/dequeue short messages |
| Table | Structured NoSQL entities keyed by partition + row | CRUD on small structured records |

- Use Blob when clients upload or download whole files by name, when you need tiering (Hot/Cool/Archive), or when you integrate with CDN, Azure Functions triggers, or analytics pipelines over large files.
- Use File when applications expect a traditional shared folder; use Queue for asynchronous work dispatch; use Table for lightweight structured lookups — not for multi-megabyte payloads.

---

## Q4. What is the difference between a block blob, a page blob, and an append blob?

What is the difference between a block blob, a page blob, and an append blob?

**Answer:** Block blobs store data as blocks and are the default for general files; page blobs store fixed 512-byte pages for random read/write and back Azure virtual machine disks; append blobs are optimized for append-only writes such as logging.

- **Block blob:** Data is split into blocks (up to 4000 blocks of up to 4000 MiB each), uploaded individually or in one call, and committed as a blob — ideal for images, documents, and large uploads via streaming.
- **Page blob:** Random-access read/write at page granularity; used internally for Azure VM VHDs and some high-I/O scenarios, not typical for simple file upload APIs.
- **Append blob:** Only supports adding new data at the end; existing content cannot be modified in place — suited for audit logs and telemetry where writers append and readers scan sequentially.

In ASP.NET Core file-upload scenarios such as this module's `FileService`, you almost always work with block blobs through `BlobClient.UploadAsync`.

---

## Q5. What redundancy options exist for a storage account that hosts blob data?

What redundancy options exist for a storage account that hosts blob data?

**Answer:** Azure Storage offers locally redundant storage (LRS), zone-redundant storage (ZRS), geo-redundant storage (GRS), geo-zone-redundant storage (GZRS), and read-access geo-redundant variants (RA-GRS, RA-GZRS), each trading cost against durability and availability during regional failures.

| Option | Copies | Survives |
|---|---|---|
| LRS | 3 copies in one datacenter | Rack/server failure |
| ZRS | 3 copies across availability zones in one region | Zone outage |
| GRS | 6 copies across two regions (secondary async) | Regional disaster (failover required) |
| RA-GRS / RA-GZRS | Same as GRS/GZRS plus read from secondary | Regional disaster with read availability on secondary |

- LRS is the lowest-cost default for dev/test; production systems that must survive zone loss choose ZRS or GZRS within a compliance boundary.
- Geo-redundant options replicate asynchronously to a paired region — failover to the secondary is a deliberate administrative action unless you configure object replication or use features like RA-GRS for read-only secondary access.

---

## Chapter 2 — Containers & Access Levels

---

## Q6. What is a blob container, and what naming rules apply to containers and blob names?

What is a blob container, and what naming rules apply to containers and blob names?

**Answer:** A blob container is a named partition within a storage account that holds blobs and can carry its own access policy and metadata; container names must be lowercase, 3–63 characters, start with a letter or number, and contain only letters, numbers, and hyphens.

- Blob names can be up to 1024 characters and may include any URL-safe characters; many teams use `/` in blob names (for example `invoices/2024/inv-001.pdf`) to organize content without true nested folders.
- Container names must be DNS-compliant because they appear in the URL path (`https://account.blob.core.windows.net/my-container/blob-name`).
- Blob names are case-sensitive within a container, so `Photo.jpg` and `photo.jpg` are distinct objects — inconsistent casing causes duplicate or "missing" file bugs in applications.

---

## Q7. What are the three blob container public access levels, and what does each allow?

What are the three blob container public access levels, and what does each allow?

**Answer:** Container public access levels are Private (no anonymous access), Blob (anonymous read for blobs only if you know the full blob URL), and Container (anonymous list of blobs in the container plus anonymous blob read) — with Private being the secure default for application data.

| Level | Anonymous list container? | Anonymous read blob? |
|---|---|---|
| Private | No | No |
| Blob | No | Yes (with exact blob URL) |
| Container | Yes | Yes |

- Private requires authenticated requests via account key, Shared Access Signature (SAS), or Microsoft Entra ID (formerly Azure AD) credentials for every operation.
- Public levels are appropriate only for truly public static assets (marketing images, open downloads); user uploads, invoices, and PII should remain Private and be served through authenticated APIs or time-limited SAS URLs.

---

## Q8. What is the difference between Blob (anonymous read access for blobs only) and Container (anonymous read access for container and blobs) public access?

What is the difference between Blob (anonymous read access for blobs only) and Container (anonymous read access for container and blobs) public access?

**Answer:** With Blob-level public access, anonymous users can download a blob only if they already know its exact URL, but they cannot enumerate container contents; with Container-level public access, anyone can list all blob names in the container and read any blob without authentication.

- Blob-level access behaves like an unlisted link — useful for embedding a known image URL on a public website without exposing the full inventory of files.
- Container-level access exposes your entire file listing, which is a serious information disclosure risk if blob names are predictable or sensitive.
- Microsoft recommends disabling anonymous public access at the storage account level unless you explicitly need it, and serving private content through SAS or your API instead.

---

## Q9. What is blob-level public access, and how does it relate to container access settings?

What is blob-level public access, and how does it relate to container access settings?

**Answer:** Blob-level public access is controlled by the container's public access level — there is no separate per-blob "public flag" in the classic model; a blob is anonymously readable only when its container is set to Blob or Container access and the storage account allows public access.

- If the container is Private, every blob requires authentication regardless of how you construct links in your application.
- Container setting Blob means anonymous GET works for direct blob URLs but LIST on the container fails; Container setting allows both LIST and GET anonymously.
- Modern security guidance treats all blobs as private by default and uses user delegation SAS or CDN origin authentication for controlled public delivery.

---

## Q10. How do you create a container programmatically with the .NET SDK, and when should you call `CreateIfNotExistsAsync`?

How do you create a container programmatically with the .NET SDK, and when should you call `CreateIfNotExistsAsync`?

**Answer:** You obtain a `BlobContainerClient` from `BlobServiceClient.GetBlobContainerClient(name)` and call `CreateIfNotExistsAsync` (optionally passing a `PublicAccessType`) to create the container only if it does not already exist, which makes startup and deployment scripts idempotent.

- Call `CreateIfNotExistsAsync` during application startup, infrastructure provisioning, or the first upload — not on every request — to avoid unnecessary management API calls and race conditions under load.
- This module's sample assumes the container `sacontainer46310114` already exists; production code typically creates it once or relies on Infrastructure as Code (Bicep, Terraform, ARM) to provision containers ahead of runtime.
- You can also set metadata, default access tier, or immutability policies on the container at creation time when compliance requirements apply.

```csharp
var container = blobServiceClient.GetBlobContainerClient("uploads");
await container.CreateIfNotExistsAsync(PublicAccessType.None);
```

---

## Chapter 3 — .NET SDK & ASP.NET Core Integration

---

## Q11. What NuGet package provides the modern Azure Blob Storage client for .NET, and what are the three main client types?

What NuGet package provides the modern Azure Blob Storage client for .NET, and what are the three main client types?

**Answer:** The `Azure.Storage.Blobs` NuGet package (part of the Azure SDK for .NET v12+) provides the current client library, and the three primary types are `BlobServiceClient` (account-level), `BlobContainerClient` (container-level), and `BlobClient` (single blob).

- This replaces the legacy `WindowsAzure.Storage` package, which used types like `CloudBlobClient` and is no longer recommended for new development.
- All v12 clients are thread-safe and designed to be registered as singletons in dependency injection because they manage connection pooling internally.
- Related packages include `Azure.Storage.Blobs.Batch` for batch delete/set-tier operations and `Azure.Storage.Blobs.Models` for options types such as `BlobUploadOptions` and `BlobHttpHeaders`.

---

## Q12. How do you register `BlobServiceClient` in ASP.NET Core dependency injection, as shown in this module's `Program.cs`?

How do you register `BlobServiceClient` in ASP.NET Core dependency injection, as shown in this module's `Program.cs`?

**Answer:** Register a factory that constructs `BlobServiceClient` from configuration — typically `builder.Configuration.GetConnectionString("AzureBlobStorage")` — and add it to the service collection with an appropriate lifetime, usually scoped or singleton, then inject it into services like `FileService`.

- This module registers `BlobServiceClient` as scoped and passes the connection string from `appsettings.json` under `ConnectionStrings:AzureBlobStorage`.
- `IFileService` / `FileService` receives `BlobServiceClient` via constructor injection and resolves container and blob clients per operation.
- For production, prefer `DefaultAzureCredential` with a managed identity instead of a connection string containing an account key (see Q14).

```csharp
builder.Services.AddSingleton(_ =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobStorage")));
builder.Services.AddScoped<IFileService, FileService>();
```

---

## Q13. What is the difference between `BlobServiceClient`, `BlobContainerClient`, and `BlobClient`?

What is the difference between `BlobServiceClient`, `BlobContainerClient`, and `BlobClient`?

**Answer:** `BlobServiceClient` operates at the storage account level (list containers, get account properties), `BlobContainerClient` operates on one container (list blobs, set container metadata, create container), and `BlobClient` operates on one named blob (upload, download, delete, set tier).

- You usually create one `BlobServiceClient` per application and derive narrower clients: `GetBlobContainerClient("uploads")` then `GetBlobClient("photo.png")`.
- Narrower clients do not duplicate connections — they share the same HTTP pipeline and credentials from the parent service client.
- This module's `FileService` follows the pattern: inject `BlobServiceClient`, get the container client, then get the blob client before calling `UploadAsync` or `DownloadAsync`.

---

## Q14. How do you authenticate to Blob Storage from a .NET application without embedding account keys in configuration?

How do you authenticate to Blob Storage from a .NET application without embedding account keys in configuration?

**Answer:** Use Microsoft Entra ID authentication via `DefaultAzureCredential` or a managed identity assigned to your App Service, Azure Functions, or AKS workload, constructing `BlobServiceClient` with the storage account URI and credential object instead of a connection string that contains an account key.

- `DefaultAzureCredential` tries environment variables, managed identity, Visual Studio / Azure CLI login locally, and other sources in order — one code path for dev and production.
- Assign RBAC roles such as **Storage Blob Data Contributor** to the managed identity so it can read and write blobs without shared keys.
- Account keys grant full control over the entire storage account; compromising one key exposes everything, whereas managed identity credentials are short-lived tokens scoped to Entra permissions.

```csharp
var client = new BlobServiceClient(
    new Uri("https://myaccount.blob.core.windows.net"),
    new DefaultAzureCredential());
```

---

## Q15. What components make up a blob storage connection string, and what does each part do?

What components make up a blob storage connection string, and what does each part do?

**Answer:** A typical connection string includes `DefaultEndpointsProtocol`, `AccountName`, `AccountKey` (or `SharedAccessSignature`), and `EndpointSuffix`, which together tell the SDK whether to use HTTPS, which account to target, how to authenticate, and which Azure cloud endpoint to use.

- `DefaultEndpointsProtocol=https` forces TLS for all requests.
- `AccountName` identifies the globally unique storage account; `AccountKey` is the base64-encoded shared key that proves account-level authority.
- `EndpointSuffix=core.windows.net` is the public Azure commercial cloud; sovereign or custom endpoints use different suffixes.
- Alternatively, a connection string can carry a SAS token instead of an account key, limiting scope to specific permissions and expiry — still less ideal than managed identity for server-side apps.

---

## Chapter 4 — Upload, Download & Streaming

---

## Q16. How does `UploadAsync` work in this module's `FileService`, and what are its limitations for large files?

How does `UploadAsync` work in this module's `FileService`, and what are its limitations for large files?

**Answer:** `FileService` opens the uploaded `IFormFile` as a stream and passes it directly to `BlobClient.UploadAsync`, which sends the bytes to Azure as a block blob in one operation — simple and correct for small files but problematic for very large uploads or unreliable networks.

- The sample hard-codes container name `sacontainer46310114` and uses the uploaded file's original name as the blob name, with no overwrite policy, content-type headers, or virus scanning.
- A single `UploadAsync` call buffers and uploads the entire stream; for files approaching hundreds of megabytes or gigabytes, you should use staged block uploads with parallel `StageBlockAsync` calls and a final `CommitBlockListAsync`.
- There is no retry of partial progress on failure mid-upload — a staged upload can resume from the last committed block instead of restarting from zero.

---

## Q17. What is the difference between a simple `UploadAsync` call and a staged block-blob upload?

What is the difference between a simple `UploadAsync` call and a staged block-blob upload?

**Answer:** A simple `UploadAsync` uploads the entire blob in one request (or an internal single-shot block commit), while a staged upload splits the file into blocks, uploads each block independently with `StageBlockAsync`, and commits the ordered block list with `CommitBlockListAsync` — enabling parallelism, resume, and support for files larger than a single request limit.

- Staged uploads let you upload blocks in parallel across threads, dramatically improving throughput on large files.
- Each block has an ID; if upload fails partway, already-staged blocks remain valid until the block list expires (typically 7 days), so clients can retry only missing blocks.
- The SDK's `BlobClient.UploadAsync` automatically chooses single-shot or multipart behavior based on size thresholds, but explicit block staging gives you finer control for custom clients or mobile apps uploading directly to blob storage.

---

## Q18. How do you download a blob as a stream without loading the entire file into memory?

How do you download a blob as a stream without loading the entire file into memory?

**Answer:** Call `BlobClient.DownloadAsync()` or `DownloadStreamingAsync()` and use the returned `BlobDownloadStreamingResult` or `Response<BlobDownloadInfo>` content stream directly, piping it to the HTTP response or disk without reading all bytes into a byte array first.

- This module's `Download` method returns `downloadContent.Value.Content`, which is a readable stream suitable for ASP.NET Core's `File(stream, contentType, fileName)` result.
- Streaming keeps memory flat regardless of file size — important when serving multi-gigabyte exports or video from an API.
- Always dispose streams when done, and set appropriate `Content-Type` and `Content-Disposition` headers on the HTTP response so browsers handle the download correctly.

---

## Q19. Why are block blobs the default choice for streaming uploads and large files in web applications?

Why are block blobs the default choice for streaming uploads and large files in web applications?

**Answer:** Block blobs are designed for storing large objects as a sequence of independently uploaded blocks that Azure assembles into one blob, which matches how browsers and servers stream multipart file uploads without needing random-access page semantics or append-only constraints.

- The block model supports efficient parallel uploads and commits, so a web API can accept an `IFormFile` stream and forward it to blob storage with minimal memory buffering.
- Page blobs target 512-byte aligned random I/O (virtual disks), and append blobs forbid in-place edits — neither fits general user file uploads.
- Azure's SDK defaults new uploads to block blobs unless you explicitly create page or append blob clients.

---

## Q20. How would you set Content-Type and blob metadata when uploading from an ASP.NET Core `IFormFile`?

How would you set Content-Type and blob metadata when uploading from an ASP.NET Core `IFormFile`?

**Answer:** Pass `BlobUploadOptions` to `UploadAsync`, setting `HttpHeaders.ContentType` from the file's content type and optional `Metadata` dictionary for application-specific key-value pairs that travel with the blob but are not part of the file body.

- Correct `Content-Type` ensures browsers and CDNs render or download the file appropriately instead of guessing from the filename extension alone.
- Metadata keys must be valid HTTP header names (ASCII letters, digits, and specific symbols); values are returned on HEAD/GET responses and searchable in some tooling.
- This module's upload omits both — production code should set `ContentType = fileModel.formFile.ContentType` and sanitize blob names to prevent path traversal or overwrite collisions.

```csharp
var options = new BlobUploadOptions
{
    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType },
    Metadata = { ["UploadedBy"] = userId }
};
await blobClient.UploadAsync(file.OpenReadStream(), options);
```

---

## Chapter 5 — Security, SAS & Authorization

---

## Q21. What is a Shared Access Signature (SAS) token, and what blob permissions can it grant?

What is a Shared Access Signature (SAS) token, and what blob permissions can it grant?

**Answer:** A Shared Access Signature (SAS) is a signed query string appended to a blob URL that grants time-limited, fine-grained permissions — such as read, write, delete, list, add, create, or update — without giving the caller the storage account key.

- Service SAS scopes permissions to a specific blob or container; account SAS can span multiple services within the account.
- Permissions are abbreviated in the signature (for example `r` read, `w` write, `d` delete, `l` list) and always include start and expiry times.
- SAS URLs can be revoked early by rotating the account key or invalidating the signing credential used to create user delegation SAS tokens.

---

## Q22. What is the difference between an account SAS, a service SAS, and a user delegation SAS?

What is the difference between an account SAS, a service SAS, and a user delegation SAS?

**Answer:** An account SAS is signed with the storage account key and applies across blob, file, queue, and table services; a service SAS is also account-key-signed but scoped to one service (for example blob only) and often one resource; a user delegation SAS is signed with Microsoft Entra credentials and inherits RBAC-aligned identity boundaries.

| SAS type | Signed with | Scope |
|---|---|---|
| Account SAS | Account key | Multiple services / containers |
| Service SAS | Account key | One service, specific resource |
| User delegation SAS | Entra user or managed identity | Blob service; revocable via RBAC |

- User delegation SAS is preferred for production because it does not require embedding the account key in signing code and can be tied to a managed identity with **Storage Blob Delegator** role.
- Account and service SAS remain common for quick scripts and legacy integrations but expose broader blast radius if the signing key leaks.

---

## Q23. When would you return a SAS URL to a client instead of proxying the file through your Web API?

When would you return a SAS URL to a client instead of proxying the file through your Web API?

**Answer:** Return a short-lived SAS URL when you want the client to download or upload directly from blob storage, offloading bandwidth and compute from your API while still controlling access through expiry and permission scope.

- Direct download improves performance for large files because bytes flow from Azure Storage to the client without passing through your App Service instance, reducing latency and egress double-charging on the app tier.
- Mobile and SPA clients can upload large files straight to blob storage using a write SAS, avoiding request size limits and timeout constraints on your API gateway.
- Keep the SAS expiry short (minutes to hours), grant minimum permissions (`r` only for download), and generate SAS server-side after your API authenticates the user — never expose the account key to clients.

---

## Q24. What are best practices for storing storage account keys and connection strings in ASP.NET Core?

What are best practices for storing storage account keys and connection strings in ASP.NET Core?

**Answer:** Avoid committing keys to source control; store secrets in Azure Key Vault, App Service configuration settings, or user secrets locally, and prefer managed identity with RBAC so no shared key exists in configuration at all.

- This module's `appsettings.json` pattern (connection string in configuration) is acceptable for learning but should use Key Vault references or App Service application settings in production, with keys rotated if ever exposed.
- Enable soft delete and disable shared key access on the storage account when all clients can use Entra ID authentication, forcing attackers to obtain valid tokens instead of a static key.
- Separate storage accounts for production and non-production so a leaked dev key cannot access production user data.

---

## Q25. How does Azure Role-Based Access Control (RBAC) complement or replace SAS tokens for blob access?

How does Azure Role-Based Access Control (RBAC) complement or replace SAS tokens for blob access?

**Answer:** Azure RBAC assigns Entra-backed roles such as **Storage Blob Data Reader** or **Storage Blob Data Contributor** to users, groups, or managed identities at subscription, resource group, or storage account scope, providing persistent authorization without generating per-URL SAS tokens.

- Server-side applications using `DefaultAzureCredential` rely on RBAC — the identity must hold the right role before any blob SDK call succeeds.
- SAS is better for delegating temporary access to external clients that cannot authenticate with Entra (anonymous browsers, third-party tools) because RBAC requires an Entra principal.
- User delegation SAS bridges both models: your API (authenticated via RBAC) mints a time-limited SAS for the external client, keeping long-lived authority in RBAC and short-lived access in SAS.

---

## Chapter 6 — Tiers, Lifecycle & Operations

---

## Q26. What are the Hot, Cool, and Archive access tiers, and how do they affect cost and retrieval latency?

What are the Hot, Cool, and Archive access tiers, and how do they affect cost and retrieval latency?

**Answer:** Hot tier optimizes for frequent read/write with higher storage cost and lower access cost; Cool tier suits infrequently accessed data with lower storage cost and higher per-access charges; Archive tier is for rarely accessed retention with the lowest storage cost but hours of retrieval latency and a rehydration step before read.

| Tier | Access pattern | Storage cost | Read cost / latency |
|---|---|---|---|
| Hot | Frequent | Highest | Lowest / immediate |
| Cool | Monthly or less | Lower | Higher / immediate |
| Archive | Rarely (compliance) | Lowest | Rehydrate first (hours) |

- You can set a default tier on upload and change tier later with `SetAccessTierAsync`; moving to Archive requires planning because online reads are not instant.
- This module's sample does not set tiers — uploads land on the account or container default, usually Hot.

---

## Q27. What is blob lifecycle management, and give an example policy for tiering or deleting old blobs?

What is blob lifecycle management, and give an example policy for tiering or deleting old blobs?

**Answer:** Blob lifecycle management applies rule-based policies that automatically transition blobs to Cool or Archive tiers or delete them after a defined age, reducing manual cleanup and storage spend on logs, backups, and temporary uploads.

- Rules filter by prefix (for example `logs/`) or blob index tags and specify actions such as "move to Cool after 30 days" or "delete after 365 days".
- Example: user-generated thumbnails under `temp/` delete after 7 days; invoice PDFs under `invoices/` move to Cool after 90 days and Archive after one year.
- Lifecycle rules run once per day at the storage account level — they complement application logic but do not replace legal hold or immutability requirements for regulated data.

---

## Q28. What are blob soft delete and blob versioning, and why enable them in production?

What are blob soft delete and blob versioning, and why enable them in production?

**Answer:** Blob soft delete retains deleted blobs and snapshots for a configurable retention period so you can undelete accidental removals, and blob versioning automatically keeps prior versions when a blob is overwritten — together they protect against application bugs, operator mistakes, and ransomware-style mass deletion.

- Soft delete applies a retention window (1–365 days) during which deleted blobs appear as recoverable; permanent deletion happens only after retention expires.
- Versioning creates a new version ID on each overwrite or delete marker, letting you restore an earlier file state without restoring the entire container from backup.
- Enable both on production accounts holding user uploads or compliance data; combine with immutability policies when regulations require WORM (write once, read many) retention that even administrators cannot shorten.

---
