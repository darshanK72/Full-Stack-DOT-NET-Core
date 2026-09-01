# Azure Blob Storage — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure Blob Storage, and what types of workloads is it designed for?](#q1-what-is-azure-blob-storage-and-what-types-of-workloads-is-it-designed-for)
2. [Q2. Explain the Azure Storage hierarchy: storage account, container, and blob.](#q2-explain-the-azure-storage-hierarchy-storage-account-container-and-blob)
3. [Q3. What are the four Azure Storage services, and when would you choose Blob Storage over the others?](#q3-what-are-the-four-azure-storage-services-and-when-would-you-choose-blob-storage-over-the-others)
4. [Q4. What is the difference between a block blob, a page blob, and an append blob?](#q4-what-is-the-difference-between-a-block-blob-a-page-blob-and-an-append-blob)
5. [Q5. What redundancy options exist for a storage account that hosts blob data?](#q5-what-redundancy-options-exist-for-a-storage-account-that-hosts-blob-data)
6. [Q6. What is a blob container, and what naming rules apply to containers and blob names?](#q6-what-is-a-blob-container-and-what-naming-rules-apply-to-containers-and-blob-names)
7. [Q7. What are the three blob container public access levels, and what does each allow?](#q7-what-are-the-three-blob-container-public-access-levels-and-what-does-each-allow)
8. [Q8. What is the difference between Blob (anonymous read access for blobs only) and Container (anonymous read access for container and blobs) public access?](#q8-what-is-the-difference-between-blob-anonymous-read-access-for-blobs-only-and-container-anonymous-read-access-for-container-and-blobs-public-access)
9. [Q9. What is blob-level public access, and how does it relate to container access settings?](#q9-what-is-blob-level-public-access-and-how-does-it-relate-to-container-access-settings)
10. [Q10. How do you create a container programmatically with the .NET SDK, and when should you call `CreateIfNotExistsAsync`?](#q10-how-do-you-create-a-container-programmatically-with-the-net-sdk-and-when-should-you-call-createifnotexistsasync)
11. [Q11. What NuGet package provides the modern Azure Blob Storage client for .NET, and what are the three main client types?](#q11-what-nuget-package-provides-the-modern-azure-blob-storage-client-for-net-and-what-are-the-three-main-client-types)
12. [Q12. How do you register `BlobServiceClient` in ASP.NET Core dependency injection, as shown in this module's `Program.cs`?](#q12-how-do-you-register-blobserviceclient-in-aspnet-core-dependency-injection-as-shown-in-this-modules-programcs)
13. [Q13. What is the difference between `BlobServiceClient`, `BlobContainerClient`, and `BlobClient`?](#q13-what-is-the-difference-between-blobserviceclient-blobcontainerclient-and-blobclient)
14. [Q14. How do you authenticate to Blob Storage from a .NET application without embedding account keys in configuration?](#q14-how-do-you-authenticate-to-blob-storage-from-a-net-application-without-embedding-account-keys-in-configuration)
15. [Q15. What components make up a blob storage connection string, and what does each part do?](#q15-what-components-make-up-a-blob-storage-connection-string-and-what-does-each-part-do)
16. [Q16. How does `UploadAsync` work in this module's `FileService`, and what are its limitations for large files?](#q16-how-does-uploadasync-work-in-this-modules-fileservice-and-what-are-its-limitations-for-large-files)
17. [Q17. What is the difference between a simple `UploadAsync` call and a staged block-blob upload?](#q17-what-is-the-difference-between-a-simple-uploadasync-call-and-a-staged-block-blob-upload)
18. [Q18. How do you download a blob as a stream without loading the entire file into memory?](#q18-how-do-you-download-a-blob-as-a-stream-without-loading-the-entire-file-into-memory)
19. [Q19. Why are block blobs the default choice for streaming uploads and large files in web applications?](#q19-why-are-block-blobs-the-default-choice-for-streaming-uploads-and-large-files-in-web-applications)
20. [Q20. How would you set Content-Type and blob metadata when uploading from an ASP.NET Core `IFormFile`?](#q20-how-would-you-set-content-type-and-blob-metadata-when-uploading-from-an-aspnet-core-iformfile)
21. [Q21. What is a Shared Access Signature (SAS) token, and what blob permissions can it grant?](#q21-what-is-a-shared-access-signature-sas-token-and-what-blob-permissions-can-it-grant)
22. [Q22. What is the difference between an account SAS, a service SAS, and a user delegation SAS?](#q22-what-is-the-difference-between-an-account-sas-a-service-sas-and-a-user-delegation-sas)
23. [Q23. When would you return a SAS URL to a client instead of proxying the file through your Web API?](#q23-when-would-you-return-a-sas-url-to-a-client-instead-of-proxying-the-file-through-your-web-api)
24. [Q24. What are best practices for storing storage account keys and connection strings in ASP.NET Core?](#q24-what-are-best-practices-for-storing-storage-account-keys-and-connection-strings-in-aspnet-core)
25. [Q25. How does Azure Role-Based Access Control (RBAC) complement or replace SAS tokens for blob access?](#q25-how-does-azure-role-based-access-control-rbac-complement-or-replace-sas-tokens-for-blob-access)
26. [Q26. What are the Hot, Cool, and Archive access tiers, and how do they affect cost and retrieval latency?](#q26-what-are-the-hot-cool-and-archive-access-tiers-and-how-do-they-affect-cost-and-retrieval-latency)
27. [Q27. What is blob lifecycle management, and give an example policy for tiering or deleting old blobs?](#q27-what-is-blob-lifecycle-management-and-give-an-example-policy-for-tiering-or-deleting-old-blobs)
28. [Q28. What are blob soft delete and blob versioning, and why enable them in production?](#q28-what-are-blob-soft-delete-and-blob-versioning-and-why-enable-them-in-production)

---

## Q1. What is Azure Blob Storage, and what types of workloads is it designed for?

**Concepts**
- Object storage for unstructured binary and text data
- Flat namespace of named blobs accessed over HTTP or HTTPS
- Petabyte-scale with cost tiers for hot, infrequent, and archive access
- No schema or query capability — retrieve by name with optional metadata

**Answer**

Azure Blob Storage is Microsoft's object storage service for unstructured data — files such as images, videos, documents, backups, and log archives — accessed over HTTP or HTTPS using a flat namespace of named blobs rather than a hierarchical file system. It is optimized for storing large binary objects that many clients or services read and write independently, which is why web apps, mobile backends, and data lakes use it for user uploads, static assets, and analytics landing zones. Blobs scale to petabytes per storage account and are billed primarily on stored capacity, transactions, and egress, making it cost-effective for both infrequently accessed archives and frequently served content. Unlike relational databases, blob storage does not enforce schemas or support queries over blob contents — you store opaque bytes and retrieve them by name, optionally with metadata and tags for organization.

---

## Q2. Explain the Azure Storage hierarchy: storage account, container, and blob.

**Concepts**
- Storage account — globally unique top-level Azure resource exposing endpoints
- Container — named partition grouping blobs, no nesting
- Blob — individual stored object with type, metadata, tier, and unique URL
- Virtual folder simulation via forward-slash characters in blob names

**Answer**

A storage account is the top-level Azure resource that holds all data and exposes service endpoints, and its name is globally unique across Azure, forming the base URL like `https://myaccount.blob.core.windows.net`. Inside it, containers group related blobs the way folders group files, but the hierarchy is strictly account → container → blob — containers cannot nest inside other containers. Each blob is identified by name within exactly one container, has a type (block, page, or append), optional metadata, optional tags, an access tier, and a unique URL composed of the account endpoint, container name, and blob name. Blob names can include `/` characters to simulate virtual folders — for example `invoices/2024/inv-001.pdf` — but these are just characters in the name rather than actual nested directory structures, since the namespace is flat.

---

## Q3. What are the four Azure Storage services, and when would you choose Blob Storage over the others?

**Concepts**
- Blob — HTTP-accessible object storage for binary and text files
- File — SMB/NFS shares for lift-and-shift or legacy file-sharing scenarios
- Queue — decoupled async message passing between producers and consumers
- Table — NoSQL key-value structured entity storage

**Answer**

Azure Storage provides four data services: Blob (object storage), File (SMB/NFS shares), Queue (message queues), and Table (NoSQL key-value). I choose Blob Storage when clients upload or download whole files by name, when I need tiering (Hot/Cool/Archive), or when integrating with CDN, Azure Functions triggers, or analytics pipelines over large files — because it is the only service designed for scalable HTTP-accessible object storage of unstructured binary or text data. I use File when applications expect a traditional shared folder and need to be mounted as a network drive, since Blob's flat namespace does not map well to applications that rely on directory traversal semantics. I use Queue for asynchronous work dispatch between producers and consumers, and Table for lightweight structured lookups keyed by partition and row — not for multi-megabyte payloads where Blob is far more cost-effective.

---

## Q4. What is the difference between a block blob, a page blob, and an append blob?

**Concepts**
- Block blob — segmented upload model for general binary files
- Page blob — 512-byte aligned random read/write for virtual machine disks
- Append blob — append-only writes for audit logs and telemetry
- BlobClient.UploadAsync defaulting to block blobs in web applications

**Answer**

Block blobs store data as independently uploaded blocks that Azure assembles into the final blob, making them the default for general files — images, documents, and large uploads via streaming. Page blobs provide fixed 512-byte-page random-access read/write, used internally for Azure VM VHDs and high-I/O scenarios, but they are not appropriate for simple file upload APIs since their random-write model adds unnecessary complexity. Append blobs only support adding new data at the end; existing content cannot be modified in place, which makes them suited for audit logs and telemetry where writers append and readers scan sequentially. In ASP.NET Core file-upload scenarios, block blobs through `BlobClient.UploadAsync` are almost always the right choice because the block model supports efficient parallel upload, resume on failure, and serves any content type from the browser download perspective.

---

## Q5. What redundancy options exist for a storage account that hosts blob data?

**Concepts**
- LRS — three copies in one datacenter, lowest cost
- ZRS — three copies across availability zones in one region
- GRS and GZRS — six copies across two regions with async secondary
- RA-GRS and RA-GZRS — read-access to secondary during regional outage

**Answer**

Azure Storage offers locally redundant storage (LRS), zone-redundant storage (ZRS), geo-redundant storage (GRS), geo-zone-redundant storage (GZRS), and read-access geo-redundant variants (RA-GRS and RA-GZRS), each trading cost against durability and availability during regional failures. LRS maintains three copies in one datacenter and is the lowest-cost default for dev/test; it protects against rack and server failures but not zone or regional outages. ZRS places three copies across availability zones in one region, surviving a zone outage, which makes it the appropriate choice for production systems within a compliance boundary that cannot replicate across regions. GRS and GZRS replicate asynchronously to a paired region, providing durability against regional disasters, but failover to the secondary is a deliberate administrative action rather than automatic unless you configure object replication. RA-GRS and RA-GZRS add a read-only secondary endpoint so applications can serve read traffic from the secondary region even before a failover is triggered, which is useful for read-heavy workloads tolerating eventual consistency.

---

## Q6. What is a blob container, and what naming rules apply to containers and blob names?

**Concepts**
- Container as a named partition with its own access policy and metadata
- Container naming rules — lowercase, 3–63 characters, DNS-compliant
- Blob name case sensitivity within a container
- Virtual folder simulation via forward-slash in blob names

**Answer**

A blob container is a named partition within a storage account that holds blobs and can carry its own access policy and metadata. Container names must be lowercase, 3–63 characters, start with a letter or number, and contain only letters, numbers, and hyphens — because they appear in the URL path as `https://account.blob.core.windows.net/my-container/blob-name` and must be DNS-compliant. Blob names can be up to 1024 characters and may include any URL-safe characters; many teams use `/` in blob names like `invoices/2024/inv-001.pdf` to organize content without true nested folders. Blob names are case-sensitive within a container, so `Photo.jpg` and `photo.jpg` are distinct objects — inconsistent casing causes duplicate or missing-file bugs in applications that generate blob names from user-supplied filenames.

---

## Q7. What are the three blob container public access levels, and what does each allow?

**Concepts**
- Private — all requests require authentication
- Blob — anonymous read for individual blobs with exact URL
- Container — anonymous blob listing plus anonymous blob read
- Private as the secure default for application data

**Answer**

Container public access levels are Private (no anonymous access), Blob (anonymous read for blobs only if the caller knows the exact blob URL), and Container (anonymous listing of blobs in the container plus anonymous blob read). Private requires authenticated requests via account key, SAS, or Microsoft Entra ID credentials for every operation and is the secure default for application data. Blob-level access is appropriate for truly public static assets like marketing images where you want direct URL access without exposing the inventory. Container-level access exposes the full listing of blob names to anyone, which is a serious information disclosure risk if blob names are predictable or sensitive — an unauthenticated caller can enumerate everything in the container. Public levels should be used only for genuinely public static content; user uploads, invoices, and PII should remain Private and be served through authenticated APIs or time-limited SAS URLs.

---

## Q8. What is the difference between Blob (anonymous read access for blobs only) and Container (anonymous read access for container and blobs) public access?

**Concepts**
- Blob-level access — anonymous download requires knowing the exact URL
- Container-level access — anonymous listing exposes full blob inventory
- Information disclosure risk from Container-level public access
- Microsoft recommendation to disable anonymous access account-wide

**Answer**

With Blob-level public access, anonymous users can download a blob only if they already know its exact URL but cannot enumerate container contents, which behaves like an unlisted link and is useful for embedding a known image URL on a public website without exposing the full file inventory. With Container-level public access, anyone can list all blob names in the container and read any blob without authentication, so the entire file listing is public — this is a serious information disclosure risk if blob names are predictable or encode sensitive information. I use Blob-level access for intentionally public static assets where discoverability is not a concern, and I avoid Container-level access entirely for data containing user files or structured names. Microsoft recommends disabling anonymous public access at the storage account level unless explicitly required, and serving private content through SAS URLs or API endpoints instead.

---

## Q9. What is blob-level public access, and how does it relate to container access settings?

**Concepts**
- No separate per-blob public flag — access controlled at container level
- Container Private setting — all blobs require authentication regardless of links
- Storage account-level allow/block public access override
- User delegation SAS as the preferred controlled public delivery mechanism

**Answer**

Blob-level public access is controlled by the container's public access level — there is no separate per-blob public flag in the classic model. A blob is anonymously readable only when its container is set to Blob or Container access and the storage account's "Allow Blob public access" setting is enabled. If the container is Private, every blob requires authentication regardless of how links are constructed in the application. Container setting Blob means anonymous GET works for direct blob URLs but LIST on the container fails; Container setting allows both LIST and GET anonymously. Modern security guidance treats all blobs as private by default and uses user delegation SAS or CDN origin authentication for controlled public delivery rather than relying on container-level anonymous access settings.

---

## Q10. How do you create a container programmatically with the .NET SDK, and when should you call `CreateIfNotExistsAsync`?

**Concepts**
- BlobContainerClient obtained from BlobServiceClient.GetBlobContainerClient
- CreateIfNotExistsAsync for idempotent container provisioning
- Calling once at startup or provisioning, not on every request
- PublicAccessType parameter setting container access level at creation

**Answer**

I obtain a `BlobContainerClient` from `BlobServiceClient.GetBlobContainerClient(name)` and call `CreateIfNotExistsAsync` to create the container only if it does not already exist, which makes startup and deployment scripts idempotent.

```csharp
var container = blobServiceClient.GetBlobContainerClient("uploads");
await container.CreateIfNotExistsAsync(PublicAccessType.None);
```

I call `CreateIfNotExistsAsync` during application startup, infrastructure provisioning, or the first upload — not on every request — to avoid unnecessary management API calls and race conditions under load. Production code typically creates containers once or relies on Infrastructure as Code like Bicep or Terraform to provision containers ahead of runtime, since container creation is an infrastructure concern rather than a per-request operation. You can also set metadata, default access tier, or immutability policies on the container at creation time when compliance requirements apply.

---

## Q11. What NuGet package provides the modern Azure Blob Storage client for .NET, and what are the three main client types?

**Concepts**
- Azure.Storage.Blobs NuGet package (Azure SDK for .NET v12+)
- BlobServiceClient, BlobContainerClient, BlobClient as the three-tier hierarchy
- Thread-safe singleton-appropriate clients sharing HTTP pipeline
- Legacy WindowsAzure.Storage package superseded

**Answer**

The `Azure.Storage.Blobs` NuGet package — part of the Azure SDK for .NET v12+ — provides the current client library, and the three primary types are `BlobServiceClient` (account-level), `BlobContainerClient` (container-level), and `BlobClient` (single blob). This replaces the legacy `WindowsAzure.Storage` package, which used types like `CloudBlobClient` and is no longer recommended for new development. All v12 clients are thread-safe and designed to be registered as singletons in dependency injection because they manage connection pooling internally, so creating a new instance per request is unnecessary and wasteful. Related packages include `Azure.Storage.Blobs.Batch` for batch delete and set-tier operations, and `Azure.Storage.Blobs.Models` for options types like `BlobUploadOptions` and `BlobHttpHeaders`.

---

## Q12. How do you register `BlobServiceClient` in ASP.NET Core dependency injection, as shown in this module's `Program.cs`?

**Concepts**
- AddSingleton for BlobServiceClient since the client manages its own pooling
- Connection string from IConfiguration sourcing credentials
- IFileService / FileService receiving BlobServiceClient via constructor injection
- DefaultAzureCredential as the production-preferred alternative to connection strings

**Answer**

I register a `BlobServiceClient` as a singleton constructed from the connection string in configuration, then inject it into services like `FileService` that need blob operations.

```csharp
builder.Services.AddSingleton(_ =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobStorage")));
builder.Services.AddScoped<IFileService, FileService>();
```

The connection string is sourced from `appsettings.json` under `ConnectionStrings:AzureBlobStorage` for local development, or from App Service configuration settings in production. `IFileService` / `FileService` receives `BlobServiceClient` via constructor injection and resolves container and blob clients per operation using the narrow client methods. For production, I prefer `DefaultAzureCredential` with a managed identity instead of a connection string containing an account key, since managed identity tokens are short-lived and eliminate a static secret from configuration.

---

## Q13. What is the difference between `BlobServiceClient`, `BlobContainerClient`, and `BlobClient`?

**Concepts**
- BlobServiceClient — account-level operations (list containers, account properties)
- BlobContainerClient — container-level operations (list blobs, set metadata)
- BlobClient — single-blob operations (upload, download, delete, set tier)
- Derived clients sharing the same HTTP pipeline and credentials

**Answer**

`BlobServiceClient` operates at the storage account level — listing containers and getting account properties — and is the root from which narrower clients are derived. `BlobContainerClient` operates on one specific container, providing methods to list blobs, set container metadata, and create the container. `BlobClient` operates on one named blob, offering upload, download, delete, and access tier operations. I usually create one `BlobServiceClient` per application registered as a singleton and derive narrower clients from it: `GetBlobContainerClient("uploads")` then `GetBlobClient("photo.png")`. Narrower clients do not duplicate connections — they share the same HTTP pipeline and credentials from the parent service client, which is why creating them on demand per operation is the correct pattern rather than caching `BlobClient` instances.

---

## Q14. How do you authenticate to Blob Storage from a .NET application without embedding account keys in configuration?

**Concepts**
- DefaultAzureCredential trying managed identity, Azure CLI, Visual Studio in sequence
- Storage Blob Data Contributor RBAC role for read and write access
- BlobServiceClient constructed with URI and credential, not connection string
- Account key blast radius — full account control if leaked

**Answer**

I use Microsoft Entra ID authentication via `DefaultAzureCredential` or a managed identity assigned to the App Service, constructing `BlobServiceClient` with the storage account URI and credential object instead of a connection string containing an account key.

```csharp
var client = new BlobServiceClient(
    new Uri("https://myaccount.blob.core.windows.net"),
    new DefaultAzureCredential());
```

`DefaultAzureCredential` tries environment variables, managed identity, Visual Studio, Azure CLI login locally, and other sources in order — one code path for dev and production. I assign RBAC roles such as Storage Blob Data Contributor to the managed identity so it can read and write blobs without shared keys. Account keys grant full control over the entire storage account, so compromising one key exposes everything; managed identity credentials are short-lived tokens scoped to Entra permissions, which dramatically limits the blast radius of a credential leak.

---

## Q15. What components make up a blob storage connection string, and what does each part do?

**Concepts**
- DefaultEndpointsProtocol enforcing HTTPS for all requests
- AccountName identifying the globally unique storage account
- AccountKey as base64-encoded shared key for account-level authority
- EndpointSuffix differentiating commercial Azure from sovereign clouds

**Answer**

A typical connection string includes `DefaultEndpointsProtocol`, `AccountName`, `AccountKey` (or `SharedAccessSignature`), and `EndpointSuffix`, which together tell the SDK whether to use HTTPS, which account to target, how to authenticate, and which Azure cloud endpoint to use. `DefaultEndpointsProtocol=https` forces TLS for all requests. `AccountName` identifies the globally unique storage account; `AccountKey` is the base64-encoded shared key that proves account-level authority. `EndpointSuffix=core.windows.net` is the public Azure commercial cloud; sovereign or custom endpoints use different suffixes. Alternatively, a connection string can carry a SAS token instead of an account key, limiting scope to specific permissions and expiry — still less ideal than managed identity for server-side apps since any static string in configuration is a credential that can leak.

---

## Q16. How does `UploadAsync` work in this module's `FileService`, and what are its limitations for large files?

**Concepts**
- UploadAsync sending the full stream in one operation
- Single-shot upload limitations for large files and unreliable networks
- No partial-upload resume on failure with simple UploadAsync
- Staged block upload required for files approaching hundreds of megabytes

**Answer**

`FileService` opens the uploaded `IFormFile` as a stream and passes it directly to `BlobClient.UploadAsync`, which sends the bytes to Azure as a block blob in one operation. This is simple and correct for small files, but for files approaching hundreds of megabytes or gigabytes, the single-shot approach is problematic because the entire stream is buffered and uploaded without the ability to resume if the connection drops mid-transfer. The sample hard-codes the container name and uses the uploaded file's original name as the blob name, with no overwrite policy, Content-Type headers, or virus scanning, so it should be considered a starting point rather than production-ready code. For large files, the correct approach is staged block uploads using parallel `StageBlockAsync` calls and a final `CommitBlockListAsync`, which allows the upload to resume from the last committed block on failure rather than restarting from zero.

---

## Q17. What is the difference between a simple `UploadAsync` call and a staged block-blob upload?

**Concepts**
- Simple UploadAsync — full stream in one request or internal single-shot commit
- StageBlockAsync — independent parallel block uploads by block ID
- CommitBlockListAsync — ordered block list committed as the final blob
- Resume capability — uncommitted blocks valid for approximately seven days

**Answer**

A simple `UploadAsync` uploads the entire blob in one request or an internal single-shot block commit, while a staged upload splits the file into blocks, uploads each block independently with `StageBlockAsync`, and commits the ordered block list with `CommitBlockListAsync`. Staged uploads let you upload blocks in parallel across threads, dramatically improving throughput on large files since multiple 4 MiB blocks travel simultaneously rather than sequentially. Each block has an ID; if the upload fails partway through, already-staged blocks remain valid for approximately 7 days, so the client can retry only missing blocks rather than restarting the entire file transfer. The SDK's `BlobClient.UploadAsync` automatically chooses single-shot or multipart behavior based on size thresholds, but explicit block staging gives finer control for custom clients or mobile apps that upload directly to blob storage from unreliable connections.

---

## Q18. How do you download a blob as a stream without loading the entire file into memory?

**Concepts**
- DownloadStreamingAsync returning a readable stream from blob storage
- ASP.NET Core File(stream, contentType) result piping stream to HTTP response
- Memory-flat streaming regardless of blob size
- Content-Type and Content-Disposition headers for correct browser handling

**Answer**

I call `BlobClient.DownloadStreamingAsync()` and use the returned content stream directly, piping it to the HTTP response or disk without reading all bytes into a byte array first. This keeps memory consumption flat regardless of file size, which is important when serving multi-gigabyte exports or video from an API. The download stream can be passed directly to ASP.NET Core's `File(stream, contentType, fileName)` result, which writes bytes to the response as they arrive from blob storage rather than buffering the entire file on the server. I always set appropriate `Content-Type` and `Content-Disposition` headers on the HTTP response so browsers handle the download correctly — a missing `Content-Disposition: attachment` header can cause browsers to render the file inline rather than downloading it, and a wrong `Content-Type` can prevent images from displaying correctly.

---

## Q19. Why are block blobs the default choice for streaming uploads and large files in web applications?

**Concepts**
- Block model supporting parallel independent block uploads
- IFormFile stream forwarded to blob storage with minimal memory buffering
- Page blob random-access and append blob constraints ruling them out
- SDK defaulting new uploads to block blobs

**Answer**

Block blobs are designed for storing large objects as a sequence of independently uploaded blocks that Azure assembles into one blob, which matches how browsers and servers stream multipart file uploads without needing random-access page semantics or append-only constraints. The block model supports efficient parallel uploads and commits, so a web API can accept an `IFormFile` stream and forward it to blob storage with minimal memory buffering, since the SDK can send blocks concurrently without loading the full file into memory. Page blobs target 512-byte aligned random I/O for virtual disks, and append blobs forbid in-place edits — neither fits general user file uploads where you write the file once and serve it many times. Azure's SDK defaults new uploads to block blobs unless you explicitly create page or append blob clients, which is the correct default for web application file handling.

---

## Q20. How would you set Content-Type and blob metadata when uploading from an ASP.NET Core `IFormFile`?

**Concepts**
- BlobUploadOptions carrying BlobHttpHeaders and Metadata dictionary
- ContentType from IFormFile.ContentType for correct browser and CDN behavior
- Metadata as key-value pairs traveling with the blob on HEAD and GET responses
- Blob name sanitization preventing path traversal or overwrite collisions

**Answer**

I pass `BlobUploadOptions` to `UploadAsync`, setting `HttpHeaders.ContentType` from the file's content type and optionally populating the `Metadata` dictionary with application-specific key-value pairs that travel with the blob but are not part of the file body.

```csharp
var options = new BlobUploadOptions
{
    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType },
    Metadata = { ["UploadedBy"] = userId }
};
await blobClient.UploadAsync(file.OpenReadStream(), options);
```

Correct `Content-Type` ensures browsers and CDNs render or download the file appropriately instead of guessing from the filename extension alone — a missing content type causes browsers to download `.png` files instead of displaying them inline. Metadata keys must be valid HTTP header names and are returned on HEAD/GET responses, making them useful for filtering and auditing without downloading the blob body. I also sanitize blob names to prevent path traversal or overwrite collisions, since using the user-supplied original filename directly can allow an attacker to overwrite existing blobs or escape the intended naming convention.

---

## Q21. What is a Shared Access Signature (SAS) token, and what blob permissions can it grant?

**Concepts**
- SAS — signed query string granting time-limited fine-grained permissions
- Permission abbreviations — r (read), w (write), d (delete), l (list), c (create)
- Start and expiry times required on every SAS
- Early revocation via account key rotation or user delegation key invalidation

**Answer**

A Shared Access Signature is a signed query string appended to a blob URL that grants time-limited, fine-grained permissions — such as read, write, delete, list, add, create, or update — without giving the caller the storage account key. Permissions are abbreviated in the signature — `r` read, `w` write, `d` delete, `l` list — and every SAS must include start and expiry times, so the token becomes invalid after the window passes. Service SAS scopes permissions to a specific blob or container; account SAS can span multiple services within the account. SAS URLs can be revoked early by rotating the account key (which invalidates account-key-signed SAS) or by invalidating the signing credential for user delegation SAS tokens. The server generates the SAS after authenticating the user; clients receive only the time-limited URL, never the account key.

---

## Q22. What is the difference between an account SAS, a service SAS, and a user delegation SAS?

**Concepts**
- Account SAS — signed with account key, spanning multiple services
- Service SAS — account-key-signed, scoped to one service and resource
- User delegation SAS — Entra-signed with RBAC-aligned identity, revocable
- User delegation SAS preferred for production security

**Answer**

An account SAS is signed with the storage account key and applies across blob, file, queue, and table services; a service SAS is also account-key-signed but scoped to one service and often one specific resource; a user delegation SAS is signed with Microsoft Entra credentials and inherits RBAC-aligned identity boundaries. User delegation SAS is preferred for production because it does not require embedding the account key in signing code — the signing authority is a managed identity with Storage Blob Delegator role — and it can be revoked by revoking the underlying RBAC assignment rather than rotating the account key. Account and service SAS remain common for quick scripts and legacy integrations, but they expose a broader blast radius if the signing key leaks since the key grants account-wide authority. The mechanism is the same — a signed query string appended to the URL — but the trust root and revocation path differ significantly.

---

## Q23. When would you return a SAS URL to a client instead of proxying the file through your Web API?

**Concepts**
- Direct client-to-storage download offloading bandwidth from the API tier
- Write SAS for client-side direct upload bypassing API request size limits
- Short expiry and minimum-permission SAS for security
- Server-side SAS generation after user authentication

**Answer**

I return a short-lived SAS URL when I want the client to download or upload directly from blob storage, offloading bandwidth and compute from my API while still controlling access through expiry and permission scope. Direct download improves performance for large files because bytes flow from Azure Storage to the client without passing through my App Service instance, reducing latency and avoiding double-charging egress on the app tier. Mobile and SPA clients can upload large files straight to blob storage using a write SAS, bypassing request size limits and timeout constraints on the API gateway entirely. I keep the SAS expiry short — minutes to hours — grant minimum permissions like `r` only for download, and generate SAS server-side after my API authenticates the user, so clients never see the account key. The client receives a URL that works for the duration of the token, after which they must request a new URL from my API.

---

## Q24. What are best practices for storing storage account keys and connection strings in ASP.NET Core?

**Concepts**
- Excluding keys from source control and committed configuration files
- Azure Key Vault references or App Service settings for production
- Managed identity with RBAC eliminating shared keys entirely
- Separate storage accounts for production and non-production environments

**Answer**

I avoid committing keys to source control and store secrets in Azure Key Vault, App Service configuration settings, or user secrets locally, preferring managed identity with RBAC so no shared key exists in configuration at all. The learning-oriented `appsettings.json` pattern with a connection string is acceptable for getting started but should use Key Vault references or App Service application settings in production, with keys rotated if ever exposed. I enable soft delete and consider disabling shared key access on the storage account when all clients can use Entra ID authentication, since disabling shared keys forces attackers to obtain valid short-lived tokens rather than a static credential. Separate storage accounts for production and non-production ensure that a leaked dev key cannot access production user data, since account keys grant authority over the entire account.

---

## Q25. How does Azure Role-Based Access Control (RBAC) complement or replace SAS tokens for blob access?

**Concepts**
- Storage Blob Data Reader and Storage Blob Data Contributor roles
- RBAC for persistent server-side authorization without per-URL token generation
- SAS for delegating temporary access to external clients without Entra identity
- User delegation SAS bridging RBAC authority and external client access

**Answer**

Azure RBAC assigns Entra-backed roles like Storage Blob Data Reader or Storage Blob Data Contributor to users, groups, or managed identities, providing persistent authorization without generating per-URL SAS tokens. Server-side applications using `DefaultAzureCredential` rely on RBAC — the identity must hold the right role before any blob SDK call succeeds, and there are no tokens to generate or expire on a per-request basis. SAS is better for delegating temporary access to external clients that cannot authenticate with Entra, such as anonymous browsers or third-party tools, because RBAC requires an Entra principal. User delegation SAS bridges both models: my API authenticates via RBAC and mints a time-limited SAS for the external client, keeping long-lived authority in RBAC and short-lived access in SAS. This combination avoids storing account keys anywhere while still enabling controlled external access.

---

## Q26. What are the Hot, Cool, and Archive access tiers, and how do they affect cost and retrieval latency?

**Concepts**
- Hot tier — highest storage cost, lowest access cost, immediate retrieval
- Cool tier — lower storage cost, higher per-access charges, immediate retrieval
- Archive tier — lowest storage cost, rehydration required before read (hours)
- SetAccessTierAsync changing tier after upload

**Answer**

Hot tier optimizes for frequent read/write with higher storage cost and lower access cost per transaction. Cool tier suits infrequently accessed data — accessed monthly or less — with lower storage cost and higher per-access charges, but still immediate retrieval. Archive tier is for rarely accessed retention with the lowest storage cost but hours of retrieval latency, since data must be rehydrated from offline storage before it can be read. Moving a blob to Archive is a commitment to that cost model: online reads are not available until rehydration completes, which can take hours depending on the priority chosen. I set a default tier on upload and change tier later with `SetAccessTierAsync`; moving frequently accessed content to Cool or Archive by mistake results in unexpectedly high access costs and latency. The module's sample does not set tiers, so uploads land on the account or container default, which is usually Hot.

---

## Q27. What is blob lifecycle management, and give an example policy for tiering or deleting old blobs?

**Concepts**
- Rule-based policies automatically tiering or deleting blobs by age
- Prefix or blob index tag filtering targeting specific blob subsets
- Daily policy execution at storage account level
- Complement to legal hold and immutability for regulated data

**Answer**

Blob lifecycle management applies rule-based policies that automatically transition blobs to Cool or Archive tiers or delete them after a defined age, reducing manual cleanup and storage spend on logs, backups, and temporary uploads. Rules filter by prefix — for example `logs/` — or blob index tags, and specify actions like "move to Cool after 30 days" or "delete after 365 days." As an example, user-generated thumbnails under `temp/` can delete after 7 days while invoice PDFs under `invoices/` move to Cool after 90 days and Archive after one year, with no code changes needed in the application. Lifecycle rules run once per day at the storage account level, so they complement application logic but do not replace legal hold or immutability requirements for regulated data where policies must be enforced with tamper-proof records.

---

## Q28. What are blob soft delete and blob versioning, and why enable them in production?

**Concepts**
- Blob soft delete — recoverable retention period for deleted blobs and snapshots
- Blob versioning — automatic version ID on each overwrite or delete marker
- Combined protection against accidental deletion and ransomware mass-delete
- WORM compliance using immutability policies alongside soft delete

**Answer**

Blob soft delete retains deleted blobs and snapshots for a configurable retention period — 1 to 365 days — so accidental removals can be recovered through the portal or SDK, and permanent deletion only happens after retention expires. Blob versioning automatically keeps prior versions when a blob is overwritten, assigning a new version ID on each write, so earlier file states can be restored without restoring the entire container from backup. Together they protect against application bugs, operator mistakes, and ransomware-style mass deletion where an attacker or misconfigured script deletes or overwrites large numbers of blobs. I enable both on production accounts holding user uploads or compliance data; soft delete provides a safety net for individual deletions while versioning provides a point-in-time history for overwrites. For regulations requiring write-once-read-many (WORM) retention that even administrators cannot shorten, I combine these with immutability policies at the container level.

---
