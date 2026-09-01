# File Upload & Streaming Responses — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is `IFormFile` in ASP.NET Core Web API?](#q1-what-is-iformfile-in-aspnet-core-web-api)
2. [Q2. What is `multipart/form-data`?](#q2-what-is-multipartform-data)
3. [Q3. What does `[FromForm]` do for file uploads?](#q3-what-does-fromform-do-for-file-uploads)
4. [Q4. What is the `[RequestSizeLimit]` attribute?](#q4-what-is-the-requestsizelimit-attribute)
5. [Q5. What is the difference between buffering and streaming a file download?](#q5-what-is-the-difference-between-buffering-and-streaming-a-file-download)
6. [Q6. What is the `Content-Disposition` header?](#q6-what-is-the-content-disposition-header)
7. [Q7. What is the difference between attachment and inline Content-Disposition?](#q7-what-is-the-difference-between-attachment-and-inline-content-disposition)
8. [Q8. What HTTP status does 413 Payload Too Large indicate?](#q8-what-http-status-does-413-payload-too-large-indicate)
9. [Q9. What are `FormOptions` in ASP.NET Core?](#q9-what-are-formoptions-in-aspnet-core)
10. [Q10. What role do Kestrel limits play in request body size?](#q10-what-role-do-kestrel-limits-play-in-request-body-size)
11. [Q11. What is `IAsyncEnumerable` streaming for API responses?](#q11-what-is-iasyncenumerable-streaming-for-api-responses)
12. [Q12. What is the difference between `File()` and `PhysicalFileResult`?](#q12-what-is-the-difference-between-file-and-physicalfileresult)
13. [Q13. What is a streaming response in Web APIs?](#q13-what-is-a-streaming-response-in-web-apis)
14. [Q14. What is `[DisableFormValueModelBinding]`?](#q14-what-is-disableformvaluemodelbinding)
15. [Q15. What is the difference between uploading via JSON vs multipart?](#q15-what-is-the-difference-between-uploading-via-json-vs-multipart)
16. [Q16. What is range request support for large files?](#q16-what-is-range-request-support-for-large-files)
17. [Q17. What is `MemoryBufferThreshold` in FormOptions?](#q17-what-is-memorybufferthreshold-in-formoptions)
18. [Q18. How does a reverse proxy affect large file uploads?](#q18-how-does-a-reverse-proxy-affect-large-file-uploads)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is `IFormFile` in ASP.NET Core Web API?

**Concepts**
- IFormFile representing an uploaded file from a multipart/form-data request
- OpenReadStream for reading bytes without full in-memory buffering
- CopyToAsync for streaming to disk or blob storage
- File name, content type, and length validation before persistence
- [FromForm] or implicit form binding required to activate IFormFile binding

**Answer**

`IFormFile` represents an uploaded file sent as part of a `multipart/form-data` request and exposes the file name, content type, length, and `OpenReadStream()` for reading bytes without loading the entire file into memory upfront. Bind it as an action parameter on POST/PUT endpoints and use `CopyToAsync` to stream directly to disk, blob storage, or a virus scanner pipeline rather than accumulating all bytes in a `byte[]`. Validate extension, content type, and size before persisting — never trust the client-provided `FileName` for constructing file system paths since it can contain path traversal sequences like `../../`. For JSON-only APIs, switching to file upload requires changing to multipart since `IFormFile` does not bind from raw JSON bodies.

---

## Q2. What is `multipart/form-data`?

**Concepts**
- multipart/form-data content type for mixed file and field requests
- Boundary delimiter separating each part in the request body
- ASP.NET Core mapping parts to IFormFile and form properties
- Binary file upload requiring multipart rather than JSON
- OpenAPI requestBody.content.multipart/form-data schema documentation

**Answer**

`multipart/form-data` is an HTTP content type for requests containing a mix of files and form fields, each sent as a separate part with its own headers — the `Content-Type` header includes a `boundary` string that delimits where one part ends and the next begins. Browsers and HTTP clients use it for file uploads; ASP.NET Core model binding maps parts to `IFormFile` parameters and form properties by matching part names to parameter names. It is required for binary file upload because JSON cannot efficiently or safely embed large binary payloads. ASP.NET Core parses multipart via form readers with configurable size and memory thresholds, and OpenAPI documents multipart endpoints with a `requestBody.content.multipart/form-data` schema including both the file part and any metadata fields.

---

## Q3. What does `[FromForm]` do for file uploads?

**Concepts**
- [FromForm] directing model binding to read from form fields rather than JSON body
- Combining IFormFile and metadata POCO in one multipart request
- JSON nested in a multipart part not auto-deserialized into POCOs
- [ApiController] inferring [FromForm] for IFormFile parameters in some cases
- Explicit binding source attributes clarifying intent for complex upload actions

**Answer**

`[FromForm]` tells model binding to read the parameter from form fields in a multipart or URL-encoded request body rather than from route, query, or JSON body. Apply it to both `IFormFile` parameters and companion metadata properties in mixed upload actions — for example, `IFormFile file` and `[FromForm] DocumentMetadata metadata` in one multipart request. A JSON string sent as a single multipart part is not automatically deserialized into a POCO; the model binder reads it as a raw string form field, so nested metadata must be either flattened into individual form fields or deserialized manually from the string value. `[ApiController]` infers `[FromForm]` for `IFormFile` parameters in some configurations, but explicit attributes clarify intent and prevent confusion when actions mix file and non-file parameters.

---

## Q4. What is the `[RequestSizeLimit]` attribute?

**Concepts**
- [RequestSizeLimit] overriding the action-level request body size
- Alignment required with Kestrel global limit and FormOptions
- IHttpMaxRequestBodySizeFeature for per-request programmatic override
- Security purpose of conservative global defaults with opt-in large limits
- [RequestFormLimits] as companion for multipart section limits

**Answer**

`[RequestSizeLimit(bytes)]` raises or lowers the maximum allowed request body size for a specific action or endpoint, overriding the global Kestrel default of approximately 30 MB. It must be paired with Kestrel, `FormOptions`, and reverse proxy configuration — setting only the attribute while leaving Kestrel at its default means the request is rejected at the server level before the action attribute is evaluated. Apply to the specific action handling large uploads: `[RequestSizeLimit(1_073_741_824)]` for 1 GB. For multipart uploads, also set `[RequestFormLimits(MultipartBodyLengthLimit = 1_073_741_824)]` since `FormOptions.MultipartBodyLengthLimit` defaults to 128 MB independently of `MaxRequestBodySize`. Setting a large limit is also a security decision — leave the global conservative and opt-in per import endpoint only.

---

## Q5. What is the difference between buffering and streaming a file download?

**Concepts**
- Buffering loading entire file into memory before the first byte is sent
- FileStreamResult sending bytes incrementally from a stream
- OOM risk from concurrent buffered large file downloads
- enableRangeProcessing enabling HTTP range requests for seek and resume
- Cloud blob OpenReadAsync piped directly to the response body

**Answer**

**Buffering** reads the entire file into memory — via `File.ReadAllBytesAsync` or similar — before writing the first byte to the response, which is simple but causes OOM under concurrent large downloads as each request holds the full file in heap memory simultaneously. **Streaming** sends bytes as they are read from disk or blob storage via `FileStreamResult`, meaning memory usage is constant regardless of file size since bytes are piped through a fixed-size buffer. Use `return File(stream, contentType)` rather than `return File(bytes, contentType)` for any file above a few hundred kilobytes. Streaming also supports range requests when `enableRangeProcessing: true` is set, enabling resume and seek for video or large PDFs. Cloud downloads should pipe the blob SDK's `OpenReadAsync` stream directly to the response body rather than downloading to a local temp file first.

---

## Q6. What is the `Content-Disposition` header?

**Concepts**
- Content-Disposition instructing the client how to handle the response body
- attachment disposition prompting a save dialog
- inline disposition opening content in the browser
- filename* (RFC 5987) for non-ASCII file name encoding
- ControllerBase.File fileDownloadName parameter setting the header

**Answer**

`Content-Disposition` tells the client how to handle the response body — typically as an attachment to download or inline to display in the browser. For file downloads it includes `filename` or the RFC 5987 `filename*` (UTF-8 encoded name) to suggest the save name. Set it via `ControllerBase.File(..., fileDownloadName: "report.pdf")` which automatically emits `Content-Disposition: attachment; filename="report.pdf"`, or construct a `ContentDispositionHeaderValue` manually in minimal APIs. JSON responses do not use Content-Disposition since they are not file downloads; it is meaningful only on binary, text, or octet-stream responses where the client needs guidance on how to present the bytes.

---

## Q7. What is the difference between attachment and inline Content-Disposition?

**Concepts**
- attachment instructing the client to save the file
- inline instructing the browser to render the content in the viewport
- Same bytes — only client handling behavior differs
- Content-Type determining whether inline rendering is possible
- attachment as safe default for unknown or sensitive file types

**Answer**

**`attachment`** instructs the client to save the file locally or open it with an external application, producing a save dialog in browsers. **`inline`** instructs the client to display the content within the browser window when the content type is renderable — PDFs, images, and plain text open directly in the browser tab. The underlying bytes are identical; only the disposition and client behavior differ. Use attachment for exports, installers, and sensitive documents users should not preview in-browser, and inline for PDFs or images intended for in-page viewing. Mobile clients may treat both similarly. `ControllerBase.File()` with a `fileDownloadName` argument typically sets attachment disposition; returning `File(stream, contentType)` without a download name omits the header, and the browser decides based on content type alone.

---

## Q8. What HTTP status does 413 Payload Too Large indicate?

**Concepts**
- 413 Payload Too Large from server size limit enforcement
- Kestrel, FormOptions, [RequestSizeLimit], and proxy limits as layered gates
- Action code never running — 413 occurs before model binding
- Presigned direct-to-blob upload as alternative for very large files
- Aligning limits across all layers to prevent unexpected 413

**Answer**

**413 Payload Too Large** means the server refused to process the request because the body exceeds configured size limits — and the action code never runs since the rejection happens during request body processing before model binding. The limits are layered: Kestrel `MaxRequestBodySize` (~30 MB default) is the server-level gate, `FormOptions.MultipartBodyLengthLimit` (128 MB default) is the form parser gate, `[RequestSizeLimit]` is the action-level override, and reverse proxy body limits (nginx `client_max_body_size`, IIS `maxAllowedContentLength`) are the infrastructure gate before Kestrel. A 413 from Kestrel means `[RequestSizeLimit]` on the action was never evaluated — you must raise the Kestrel limit as well. Return a friendly ProblemDetails message from custom middleware if you intercept size violations. For very large uploads (hundreds of MB to GB), consider presigned URLs for direct client-to-blob uploads so the API never handles the binary body.

---

## Q9. What are `FormOptions` in ASP.NET Core?

**Concepts**
- FormOptions configuring multipart parsing limits in ASP.NET Core
- MultipartBodyLengthLimit defaulting to 128 MB independently of Kestrel
- ValueLengthLimit and KeyLengthLimit capping individual form field sizes
- MultipartHeadersCountLimit preventing header explosion attacks
- Alignment with [RequestSizeLimit] and Kestrel for consistent upload behavior

**Answer**

`FormOptions` configures how ASP.NET Core parses form and multipart requests — limits on body length, number of headers, individual multipart section size, and the memory buffering threshold before spilling to disk temp files. Configure via `services.Configure<FormOptions>(options => { ... })` globally or `[RequestFormLimits(...)]` per action. `MultipartBodyLengthLimit` defaults to 128 MB, which can block uploads below the Kestrel limit and cause confusing 413s that appear to be an action-level problem. `ValueLengthLimit` and `KeyLengthLimit` cap individual form field sizes to prevent maliciously large field values. `MultipartHeadersCountLimit` limits the number of headers per part to prevent header explosion attacks. All these limits must be aligned with `[RequestSizeLimit]` and the Kestrel `MaxRequestBodySize` for uploads to behave consistently across all layers.

---

## Q10. What role do Kestrel limits play in request body size?

**Concepts**
- Kestrel MaxRequestBodySize as the server-level gate before all middleware
- Global default of ~30 MB enforced before action attributes are evaluated
- IHttpMaxRequestBodySizeFeature for per-request programmatic override
- Setting MaxRequestBodySize to null disabling the limit entirely — risky
- Proxy limits equally critical alongside Kestrel in production

**Answer**

Kestrel enforces `Limits.MaxRequestBodySize` (default approximately 30 MB) at the server level before ASP.NET Core middleware, model binding, or action attributes see the body. No action-level `[RequestSizeLimit]` attribute can allow a larger upload unless Kestrel (or `IHttpMaxRequestBodySizeFeature`) first permits it — a 413 from Kestrel never reaches the attribute. Configure globally with `builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = ...)` or per-endpoint via `context.Features.Get<IHttpMaxRequestBodySizeFeature>()?.MaxRequestBodySize = ...` inside the action. Setting it to `null` disables the limit entirely, which is dangerous on public endpoints without other guards. In production the reverse proxy — nginx, Azure Front Door, or Cloudflare — imposes its own body limit that must also be aligned; a 413 from the proxy never even reaches Kestrel.

---

## Q11. What is `IAsyncEnumerable` streaming for API responses?

**Concepts**
- IAsyncEnumerable<T> producing items incrementally rather than materializing a full list
- EF Core AsAsyncEnumerable() returning items as they are fetched
- System.Text.Json serializing async enumerables to chunked JSON arrays
- [EnumeratorCancellation] CancellationToken for client-disconnect cancellation
- Reverse proxy buffering breaking streaming if not disabled at the gateway

**Answer**

Returning `IAsyncEnumerable<T>` from a controller or minimal API lets ASP.NET Core serialize items incrementally as they are produced — chunked JSON array output instead of materializing millions of records in a `List<T>` first. The EF Core pattern is to return the query as `AsAsyncEnumerable()` rather than calling `ToListAsync()`, which means rows flow from the database cursor to the HTTP response body without the entire result set ever existing in heap memory simultaneously. Use `[EnumeratorCancellation]` on the `CancellationToken` parameter to cancel enumeration when the client disconnects, preventing the database query from continuing to run for a client that has already left. System.Text.Json in ASP.NET Core 8 supports async enumeration natively. Reverse proxies may buffer entire responses — disable response buffering on streaming endpoints at the gateway to ensure the incremental benefit reaches the client.

---

## Q12. What is the difference between `File()` and `PhysicalFileResult`?

**Concepts**
- PhysicalFileResult reading from a file system path on disk
- File(stream) accepting any readable stream including cloud SDK streams
- File(bytes) loading everything into memory — avoid for large files
- enableRangeProcessing parameter for HTTP Range request support on both
- VirtualFileResult serving from embedded resources or IFileProvider roots

**Answer**

Both stream file content to the client, but they differ in source. `PhysicalFileResult` (returned by `PhysicalFile(path, contentType, fileDownloadName)`) reads from a file path on disk, which is convenient when the file already exists on the server file system. `File(stream, ...)` accepts any open readable stream — including blob SDK streams, memory streams, or network streams — making it more flexible for cloud-stored files. `File(bytes, ...)` loads everything into memory before sending and should be avoided for any file above a few kilobytes. Both overloads accept `enableRangeProcessing: true` to support HTTP Range requests for resume and seek. `VirtualFileResult` serves from embedded resources or `IFileProvider` content roots, useful for static assets registered in the DI container.

---

## Q13. What is a streaming response in Web APIs?

**Concepts**
- Streaming response sending body bytes incrementally as data is produced
- Chunked transfer encoding when Content-Length is unknown
- CancellationToken stopping production when the client disconnects
- Proxy and load balancer buffering breaking end-to-end streaming
- Upload streaming and download streaming as separate concerns

**Answer**

A streaming response sends the HTTP body incrementally as data is generated or read, rather than buffering the full payload before the first byte. Examples include `FileStreamResult` for file downloads, `IAsyncEnumerable<T>` JSON serialization, NDJSON line streams for real-time event feeds, and Server-Sent Events. When `Content-Length` is unknown at the start of the response, ASP.NET Core uses chunked transfer encoding so the client knows when each chunk ends without waiting for the full body. The `CancellationToken` passed to the streaming operation propagates client-disconnect events so production stops rather than continuing to generate data for a disconnected client. Proxies and load balancers must not buffer entire responses for streaming to work end-to-end — configure `proxy_buffering off` in nginx for streaming endpoints or use a streaming-aware gateway.

---

## Q14. What is `[DisableFormValueModelBinding]`?

**Concepts**
- [DisableFormValueModelBinding] preventing automatic form model binding
- IFormFile parameters being null when the filter is applied
- MultipartReader for section-by-section manual streaming upload processing
- Mutually exclusive patterns: IFormFile binding vs manual multipart parsing
- Section-by-section processing for virus scan pipelines

**Answer**

`[DisableFormValueModelBinding]` is a filter that disables automatic form model binding for an action so the developer can manually parse multipart data with `MultipartReader` for true streaming uploads without first buffering the entire body. When applied, `IFormFile` parameters will not bind — they remain null — because the form reader that populates them is disabled. Use this pattern in virus-scan pipelines that must process the stream section-by-section and pass each chunk to the scanner before persisting. Mixing the two patterns incorrectly — applying `[DisableFormValueModelBinding]` while still expecting `IFormFile` parameters to be populated — produces null `IFormFile` with no error, which is the most common misapplication seen in code copied from streaming upload samples. Choose one pattern per endpoint: either standard `IFormFile` binding for simple uploads, or `[DisableFormValueModelBinding]` with `MultipartReader` for streaming processing.

---

## Q15. What is the difference between uploading via JSON vs multipart?

**Concepts**
- JSON base64 encoding adding ~33% size overhead for binary data
- multipart/form-data sending binary parts natively without encoding overhead
- ASP.NET Core IFormFile binding from multipart parts
- Presigned URL upload as a third pattern bypassing the API body entirely
- OpenAPI schema differences between JSON and multipart endpoints

**Answer**

**JSON** (`application/json`) suits metadata and small text payloads but is inefficient and impractical for large binary files — base64 encoding inflates size by approximately 33% and requires the entire JSON body to be parsed before any bytes can be processed. **Multipart** (`multipart/form-data`) sends binary file parts as native binary alongside text form fields, which is the standard HTTP pattern for file uploads. ASP.NET Core binds multipart parts to `IFormFile` parameters and can stream file parts to their destination without full in-memory buffering. For very large files (gigabytes), a presigned URL pattern is a third option: the API generates a short-lived direct-to-blob upload URL and returns it to the client, which then uploads directly to blob storage without the API handling the binary body at all. OpenAPI documents multipart endpoints with `requestBody.content.multipart/form-data` schema and JSON endpoints with `application/json` schema — mixed APIs often use multipart for create-with-file and JSON for metadata-only updates.

---

## Q16. What is range request support for large files?

**Concepts**
- HTTP Range header requesting byte subsets for resume and seek
- 206 Partial Content response with Content-Range header
- enableRangeProcessing: true on FileResult activating range support
- Accept-Ranges: bytes header advertising capability to clients
- Seeking requirement on the underlying stream for range requests

**Answer**

HTTP Range requests let clients request byte subsets of a resource (`Range: bytes=0-1023`) for resume, seek, and partial downloads — essential for video players, PDF viewers, and download managers that resume interrupted transfers. Enable with `enableRangeProcessing: true` on `FileResult`; ASP.NET Core then responds with **206 Partial Content** and a `Content-Range` header when a range header is present in the request, or the full 200 response when no range is requested. The `Accept-Ranges: bytes` header in the response advertises the capability to clients so they know to attempt range requests after a failed download. The underlying stream or file must support seeking — cloud blob streams from SDK `OpenReadAsync` typically support seek, but network streams from HTTP responses do not. Without range support, clients re-download entire multi-GB files after connection drops, which is unacceptable for video or large document download scenarios.

---

## Q17. What is `MemoryBufferThreshold` in FormOptions?

**Concepts**
- MemoryBufferThreshold controlling per-section in-memory buffer before disk spill
- Default 64 KB spilling to temp file for larger sections
- int.MaxValue forcing full in-memory buffering — OOM risk under concurrency
- Per-section buffering independent for file parts and form text fields
- Tuning for performance vs memory trade-off on high-concurrency upload servers

**Answer**

`MemoryBufferThreshold` (default 64 KB) controls how much of each multipart section is held in memory before ASP.NET Core spills overflow to a temporary disk file. This means a 10 MB file upload uses at most 64 KB of heap per request for the file section, with the remainder written to a temp file that `IFormFile.OpenReadStream()` then reads from. Setting it to `int.MaxValue` forces full in-memory buffering — simple but dangerous under concurrent large file uploads since every concurrent request holds the entire file in heap, which exhausts memory quickly. The threshold applies per multipart section independently, so a multipart request with a 100 MB file and a 1 KB metadata field buffers the field in memory and spills the file to disk. Tuning lower values reduces RAM at the cost of disk I/O; the default 64 KB is appropriate for most scenarios.

---

## Q18. How does a reverse proxy affect large file uploads?

**Concepts**
- Proxy body size limits enforced before Kestrel sees the request
- nginx client_max_body_size and IIS maxAllowedContentLength
- Proxy read and send timeouts for slow-network large file uploads
- Response buffering on proxy breaking IAsyncEnumerable streaming downloads
- Aligning all layers: proxy, Kestrel, FormOptions, and action attributes

**Answer**

Reverse proxies (nginx, IIS ARR, Azure Application Gateway, Cloudflare) impose their own body size limits, timeouts, and buffering behavior that can reject or truncate uploads before Kestrel sees them — causing 413, 502, or silent timeouts in production even when all ASP.NET Core limits are correctly configured. nginx's `client_max_body_size` must meet or exceed the API's `[RequestSizeLimit]`; IIS requires `maxAllowedContentLength` in `web.config`. Proxy read and send timeouts must exceed the worst-case upload duration for large files on slow networks — a 30-second proxy timeout is insufficient for a 1 GB upload over a slow mobile connection. Response buffering on the proxy can also break streaming downloads and `IAsyncEnumerable` JSON exports by waiting for the complete body before forwarding to the client. Align limits and timeouts across proxy, Kestrel, and FormOptions, and always test large uploads through a production-like path that includes the proxy layer, not just directly against Kestrel.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

#### Gotcha 1. POST returning 200 instead of 201

**Concepts**
- HTTP 201 Created with Location header as REST create contract
- CreatedAtAction / CreatedAtRoute for correct response
- Resource discovery via Location header
- Status code semantics for OpenAPI-generated clients

**Answer**

A successful resource creation with POST should return HTTP 201 Created and a `Location` header pointing at the new resource URL, because 200 OK carries no hint that a new resource was created or where to find it. Standard HTTP clients, API gateways, and OpenAPI-generated SDKs all look at the status code first — returning 200 means the response body is the only way to discover the new resource id, and clients that skip parsing the body miss it entirely. Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with the Location header, and include the created representation or a minimal payload in the body when clients need immediate data without a follow-up GET.

---

#### Gotcha 2. GET that mutates state

**Concepts**
- GET as safe and idempotent per HTTP specification
- Prefetch and crawler risks from side-effecting GETs
- Caching proxy behavior replaying GET responses
- Correct HTTP verbs for state-changing operations

**Answer**

GET must be safe and idempotent per HTTP semantics — performing deletes or updates in a GET handler violates the specification, breaks caching proxies that may replay GET responses, and creates security holes when URLs are prefetched by browsers, link-preview crawlers, or email clients. The problem is that these callers invoke GET URLs without user intent, so a delete fires without anyone clicking anything. Cached GET responses can replay destructive operations across clients since the proxy treats the response as a normal cacheable resource. Use POST, PUT, PATCH, or DELETE for any operation that changes state and reserve GET strictly for reads.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Concepts**
- HTTP status code as the universal success vs failure contract
- 200 with error flag defeating monitoring, retries, and API gateways
- ProblemDetails for consistent structured failure responses
- APM alerting and circuit breakers depending on HTTP status

**Answer**

Business failures must map to appropriate 4xx or 5xx status codes because HTTP status is the universal contract that drives client retry logic, API gateway circuit breakers, and APM alerting thresholds — a 200 response with `success: false` in the body masks every failure from every system that does not parse the body. API gateways route and throttle on status code; if every response is 200, failed calls look healthy in dashboards and no alert fires. Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures, 404 for missing resources, 409 for conflicts, and 422 for semantic rejections. Envelope patterns like `{ success: false }` require every consumer to implement a custom parser and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Concepts**
- EF entity navigation properties not suitable for public HTTP contracts
- Lazy-loading N+1 triggered during JSON serialization
- Circular reference serializer loops
- DTO decoupling API contract from persistence schema

**Answer**

EF Core entities carry navigation properties, change-tracker state, and database-internal fields that were never meant to be a public HTTP contract, so serializing them directly leaks schema details and invites circular reference errors. Lazy-loaded navigations trigger N+1 queries during serialization when the JSON serializer walks the object graph — each navigation fires a new SQL query, exhausting the connection pool under load. Circular references between related entities cause the JSON serializer to loop indefinitely or require fragile `ReferenceHandler.IgnoreCycles` settings that hide design problems. Map entities to DTOs with explicit shapes in the service layer or via EF projection so the API contract evolves independently of table schema changes.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Concepts**
- System.Text.Json defaulting to camelCase serialization in ASP.NET Core 8
- Silent binding failure from PascalCase client payloads
- JsonPropertyName and PropertyNamingPolicy as alignment tools
- PropertyNameCaseInsensitive for legacy mixed-casing clients

**Answer**

ASP.NET Core 8 defaults to camelCase JSON serialization via `System.Text.Json`, so PascalCase property names from legacy clients bind as missing properties because the case does not match — the model properties default to `null` or `0` rather than the values the client sent. The failure is silent: the request returns 201 or 204 with no validation error, but the persisted record has default values instead of the submitted data. Fix with `[JsonPropertyName("PropertyName")]` attributes on DTO properties or a custom `PropertyNamingPolicy` to align server expectations with legacy payloads. When accepting mixed casing from various clients, enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)`.

---

#### Gotcha 6. GET with `[FromBody]`

**Concepts**
- GET request body not reliably supported across the HTTP ecosystem
- [FromBody] on GET failing silently through proxies and caches
- [FromQuery] for simple filters as the correct alternative
- OpenAPI tools and browser fetch blocking GET bodies

**Answer**

Many HTTP clients, proxies, CDNs, and caches ignore or strip GET request bodies because the HTTP specification does not define semantics for GET bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8. Model binding for `[FromBody]` on GET is therefore unreliable across the full HTTP ecosystem even if it works in direct testing. Use query strings with `[FromQuery]` for simple filter parameters, or POST to a dedicated search endpoint for complex filter objects that do not fit in a URL. Browser fetch API and OpenAPI tooling also discourage or block GET bodies, making the pattern fragile in any production environment where the full request path includes a proxy.

---

#### Gotcha 7. CORS as server security

**Concepts**
- CORS as browser-only enforcement — not server-side authentication
- Non-browser clients unaffected by CORS headers
- Authentication and authorization as actual server protection
- CORS enabling SPA browser access alongside real auth

**Answer**

CORS is enforced by browsers only — it prevents JavaScript on one origin from reading cross-origin responses, but it does nothing to stop curl, Postman, server-to-server calls, or any direct API request. The `Access-Control-Allow-Origin` header is a signal browsers check after receiving the response; a non-browser client simply ignores it and reads the data. A public API without authentication is fully accessible to any non-browser caller regardless of CORS policy, so CORS is never a substitute for JWT, API keys, or cookies. Register `AddCors` and `UseCors` to enable browser SPA access on cross-origin calls, and enforce actual authentication and authorization separately for real protection.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Concepts**
- Browser rejection of wildcard origin on credentialed requests
- AllowAnyOrigin and AllowCredentials as mutually exclusive
- WithOrigins for explicit trusted frontend origins
- Access-Control-Allow-Credentials header requirement

**Answer**

Browsers reject a response with `Access-Control-Allow-Origin: *` when the request includes cookies or an `Authorization` header, because the CORS specification explicitly forbids wildcard origins on credentialed cross-origin requests. `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined — ASP.NET Core will not emit a valid CORS response for credentialed requests when both are set. Instead, use `WithOrigins("https://app.example.com", "https://localhost:3000")` to list every trusted frontend origin explicitly, including local development URLs and all production domains. The browser also requires `Access-Control-Allow-Credentials: true` in the response, which `AllowCredentials()` handles.

---

#### Gotcha 9. Swagger UI exposed in Production

**Concepts**
- Swagger UI disclosing full API surface and schema to public internet
- Environment checks wrapping MapSwagger and UseSwaggerUI
- OpenAPI document exposure revealing endpoint names and enum values
- Authentication or IP allowlist gating for API documentation

**Answer**

Public Swagger UI discloses the full API surface, all schemas, enum values, and try-it-out access to anyone who finds the URL — giving potential attackers a complete map of your endpoints and data structures without any effort. Gate `MapSwagger` and `UseSwaggerUI` in `Program.cs` behind environment checks so they run only in Development and Staging, or require authentication middleware before the Swagger middleware. Production APIs should serve OpenAPI documents only to authenticated developers or internal tooling, not the public internet. Exposed OpenAPI documents reveal internal endpoint names, field names, and request schemas that are directly useful for targeted reconnaissance.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Concepts**
- [ApiController] enabling automatic ModelStateInvalidFilter
- Binding source inference for complex types
- Mixed controllers producing inconsistent error contracts
- Assembly-level [ApiController] for uniform behavior

**Answer**

Without `[ApiController]`, automatic 400 `ValidationProblemDetails` responses, binding source inference for complex types, and attribute routing enforcement all differ from controllers that have the attribute — so mixed controllers in the same API produce inconsistent error shapes that break partner integrations. A controller missing `[ApiController]` may return 200 OK with a partially bound model when model validation fails, because `ModelStateInvalidFilter` does not run, and `[FromBody]` is not inferred for complex parameters. Apply `[ApiController]` at the controller or assembly level using `[assembly: ApiController]` in an attribute file so every endpoint shares the same conventions without per-class annotation.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Concepts**
- Sync-over-async causing thread-pool starvation under load
- Deadlock when synchronization context is held during blocking call
- async Task<IActionResult> propagating await through service layer
- Kestrel throughput reduction from blocked request threads

**Answer**

Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation under load because the calling thread is blocked waiting for I/O to complete while no thread is available to process the continuation. Deadlocks also occur in environments with a synchronization context when the blocked thread holds the context that the async continuation needs to resume on — the task never completes because the thread it needs is the thread that is waiting for it. Always `await` async service and database calls in controller actions, which means the action signature is `async Task<IActionResult>` and the `await` propagates through the entire service and repository layer. Kestrel processes many concurrent requests efficiently precisely because async I/O frees threads while waiting — sync-over-async defeats this design entirely.

---

#### Gotcha 12. Liveness probe includes SQL check

**Concepts**
- Liveness as process restart signal — unrelated to external dependency recovery
- Readiness as traffic drain signal for dependency failures
- Kubernetes restart loop from liveness including external checks
- Tag-based separation of liveness and readiness health checks

**Answer**

If the liveness probe includes SQL and the database goes down for maintenance, Kubernetes kills and restarts pods even though restarting cannot fix a database outage — creating a restart loop that adds startup overhead and delays recovery. Liveness answers whether the ASP.NET Core process is alive and responsive; it should return healthy as long as the process can handle an HTTP request, independent of downstream dependencies. Readiness answers whether the instance should receive traffic; SQL, Redis, and message bus checks belong here because a failing dependency means the instance will return errors. Map `/health/live` with a tag predicate selecting only the self-check and `/health/ready` with the predicate selecting `AddDbContextCheck` and other dependency checks.

---

#### Gotcha 13. N+1 queries in list endpoints

**Concepts**
- N+1 pattern: one parent query plus N child queries per row
- Lazy loading triggering extra SQL during serialization
- EF projection with Select fetching only required columns
- Include/ThenInclude for explicit eager loading in one round trip

**Answer**

N+1 occurs when a list endpoint loads a parent collection and then each item triggers an additional query for a related navigation — one query for 100 orders plus 100 queries for each order's customer. The most common cause in APIs is serializing entity objects with lazy-loaded navigation properties: the JSON serializer accesses a navigation, EF fires a SELECT, and this repeats once per row. Fix with a single translated query: project directly to DTOs using `.Select(o => new OrderDto { CustomerName = o.Customer.Name })` so EF generates one SQL JOIN, or use explicit `.Include(o => o.Customer)` before materialization. Validate with EF logging or APM to confirm list endpoints produce a fixed small number of SQL round trips regardless of result set size.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Concepts**
- Offset pagination page drift from concurrent inserts and deletes
- Skip/Take without stable OrderBy producing undefined row order
- Keyset pagination anchored to a stable indexed key
- Large OFFSET performance cost scanning and discarding preceding rows

**Answer**

Concurrent inserts and deletes shift row positions in the dataset while a client walks pages — a new row inserted at page 1 pushes all subsequent rows one position, so page 2 either repeats the last row of page 1 or skips a row entirely. `Skip((page - 1) * pageSize).Take(pageSize)` also requires the database to count and discard all preceding rows, which becomes expensive on large offsets. Keyset pagination avoids both problems by using `WHERE id > @lastSeenId ORDER BY id LIMIT @pageSize` with the last key from the previous response — no scanning skipped rows and no drift because the filter is anchored to a specific key rather than a count. Offset pagination remains acceptable for small mostly-static tables; expose cursor tokens in link headers or response metadata for high-churn datasets.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Concepts**
- Field resolvers executing one database query per parent row
- DataLoader batching concurrent field resolutions into a single query
- 101 queries for a 100-row list without batching
- Root-level eager loading as alternative for static parent-child fields

**Answer**

Field resolvers in HotChocolate or other GraphQL servers execute independently per parent row — resolving `books` for each of 100 authors runs 100 separate queries plus the initial author query, totaling 101 round trips. DataLoader batches concurrent field resolutions within a single request: all 100 `books` resolver calls accumulate the author ids during the execution tick, then DataLoader fires one grouped query for all of them at once. Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips automatically. For fields the client almost always requests together with the parent, eager-load or project at the root query level rather than using DataLoader.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Concepts**
- Native gRPC HTTP/2 binary framing not accessible to browser JavaScript
- gRPC-Web protocol as browser-compatible translation layer
- AddGrpcWeb and EnableGrpcWeb for middleware setup
- CORS configuration required alongside gRPC-Web for cross-origin calls

**Answer**

Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript APIs — browsers cannot control trailers or binary framing at the level gRPC requires, so `@grpc/grpc-js` in the browser fails. Browser clients need the gRPC-Web protocol, which translates between the browser-accessible HTTP/1.1 or HTTP/2 fetch API and the native gRPC binary format via ASP.NET Core middleware. Add `AddGrpcWeb()` to services and call `.EnableGrpcWeb()` on each mapped gRPC service to activate the translation layer. CORS must also be configured for the browser origin because cross-origin browser calls still enforce CORS preflight and response header checks regardless of gRPC-Web. Standard .NET or Node gRPC clients communicating server-to-server continue using native gRPC without gRPC-Web.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Review this document upload endpoint. QA uploads a 200 MB PDF successfully in test; Production Kestrel returns **413 Payload Too Large** for the same file. Developers say "we set `[RequestSizeLimit]` on the action."

```csharp
[HttpPost("upload")]
[RequestSizeLimit(500 * 1024 * 1024)]
public async Task<IActionResult> Upload(IFormFile file)
{
    await using var stream = file.OpenReadStream();
    var path = Path.Combine("/uploads", file.FileName);
    await using var fs = File.Create(path);
    await stream.CopyToAsync(fs);
    return Ok(new { file.FileName, file.Length });
}
```

`Program.cs` has no Kestrel or multipart limit configuration. Reverse proxy timeout is 30 seconds.

**Concepts**
- [RequestSizeLimit] overriding action-level limit but Kestrel global default still gates first
- Kestrel MaxRequestBodySize ~30 MB evaluated before the action attribute
- FormOptions.MultipartBodyLengthLimit at 128 MB as a separate limit
- Path.Combine with client FileName enabling path traversal vulnerability
- Proxy timeout insufficient for large file upload duration

**Answer**

The 413 comes from Kestrel, not from the action attribute — `[RequestSizeLimit(500MB)]` overrides the per-action limit, but Kestrel's global `MaxRequestBodySize` of approximately 30 MB is enforced at the server level before any action attribute is evaluated. The action attribute is never reached for a 200 MB upload. The fix requires also configuring Kestrel: `builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 500L * 1024 * 1024)` globally, or using `IHttpMaxRequestBodySizeFeature` per-request. Beyond the 413, `FormOptions.MultipartBodyLengthLimit` defaults to 128 MB independently and must also be raised via `[RequestFormLimits(MultipartBodyLengthLimit = 500L * 1024 * 1024)]` on the action. The 30-second proxy timeout is also a problem — a 200 MB upload on a slow connection takes longer than 30 seconds and will be cut off with a 502. There is also a security issue: `Path.Combine("/uploads", file.FileName)` is vulnerable to path traversal if `FileName` contains `../` sequences — always use `Path.GetFileName(file.FileName)` to strip directory components before constructing the destination path.

---

#### Q2. (P) Configure **request body size limits** for a Web API that accepts CSV imports up to 1 GB via `IFormFile` while keeping default 30 MB limits on all other endpoints. What do you set in Kestrel, `FormOptions`, and action attributes?

**Concepts**
- Global Kestrel MaxRequestBodySize raised for upload endpoint via IHttpMaxRequestBodySizeFeature
- [RequestSizeLimit] and [RequestFormLimits] on the specific import action
- FormOptions global default preserved for all other endpoints
- Per-request Kestrel override pattern to avoid raising global default
- Proxy limit alignment alongside application-level limits

**Answer**

The correct approach avoids raising the global Kestrel limit — which would expose all endpoints to 1 GB bodies — and instead raises it only for the import endpoint. In `Program.cs`, keep the Kestrel global at the default (approximately 30 MB) or lower. On the import action, apply `[RequestSizeLimit(1L * 1024 * 1024 * 1024)]` and `[RequestFormLimits(MultipartBodyLengthLimit = 1L * 1024 * 1024 * 1024)]` to override both the action-level and form-parser limits. To also override the Kestrel limit for that specific request without changing the global, access `IHttpMaxRequestBodySizeFeature` inside the action before model binding runs — this requires a minimal API or an action filter rather than the action body itself, since model binding runs before the action body. Alternatively, raise the Kestrel global in `Program.cs` and rely on `[RequestSizeLimit(30 * 1024 * 1024)]` on all other endpoints to cap them down — less clean but simpler. In both cases, align the reverse proxy (`nginx: client_max_body_size 1g;`, IIS `maxAllowedContentLength`) and extend the proxy timeout to cover the upload duration.

---

#### Q3. (R) Review this "streaming download" endpoint. Memory spikes to file size on concurrent downloads and the pod OOMs.

```csharp
[HttpGet("{id}/download")]
public async Task<IActionResult> Download(int id)
{
    var path = _store.GetPath(id);
    var bytes = await File.ReadAllBytesAsync(path);
    return File(bytes, "application/octet-stream", Path.GetFileName(path));
}
```

Files range from 50 MB to 4 GB.

**Concepts**
- File.ReadAllBytesAsync loading entire file into memory before the first byte is sent
- File(bytes) buffering the full content in heap before response starts
- FileStreamResult sending bytes incrementally from a stream
- OOM under concurrent large downloads from multiple in-memory full-file copies
- PhysicalFile as the simplest fix for server-disk files

**Answer**

`File.ReadAllBytesAsync` loads the entire file into a `byte[]` in heap memory before a single byte is sent to the client, and `File(bytes, ...)` holds that entire array until the response completes. For a 500 MB file, every concurrent download consumes 500 MB of heap — 10 concurrent downloads use 5 GB, which easily OOMs a container. The name "streaming download" is misleading: this is fully buffered. The fix is to stream from the file directly: `return PhysicalFile(path, "application/octet-stream", Path.GetFileName(path))` reads from disk incrementally through a fixed internal buffer so memory usage is constant regardless of file size. For files not on local disk (blob storage), use `return File(await _blobClient.OpenReadAsync(), "application/octet-stream", fileName)` — the SDK's `OpenReadAsync` returns a seekable stream that ASP.NET Core can pipe directly to the response. Add `enableRangeProcessing: true` to the `PhysicalFile` or `File` call to support resume and seek for large files.

---

#### Q4. (M) A client sends `multipart/form-data` with fields `metadata` (JSON) and `file` (binary). Review binding — why is `Metadata` null and `File` populated?

```csharp
public class UploadRequest
{
    public IFormFile File { get; set; }
    public DocumentMetadata Metadata { get; set; }
}

[HttpPost("documents")]
public IActionResult Upload([FromForm] UploadRequest request)
{
    return Ok(new { request.Metadata?.Title, request.File?.FileName });
}
```

Client sends `metadata` part as `Content-Type: application/json` in the multipart body without `[FromForm]` on nested properties.

**Concepts**
- Multipart form binder treating all parts as string form fields regardless of part Content-Type
- ASP.NET Core not auto-deserializing JSON parts within multipart as POCOs
- Manual JSON deserialization from string form field value as the fix
- Flattening metadata fields as individual form fields as an alternative
- [FromForm] binding to flat string values only — not nested JSON

**Answer**

`Metadata` is null because ASP.NET Core's form model binder does not auto-deserialize JSON-encoded multipart parts into POCOs — it treats every part as a string form field regardless of the part's `Content-Type: application/json` header. The binder looks for a form field named `metadata` and finds a string, but `DocumentMetadata` is a complex type that cannot be bound from a string without explicit deserialization. `IFormFile` is populated because the binder has built-in support for mapping binary multipart parts to `IFormFile` parameters by matching on the part name and treating the part as a file rather than a string. There are two fixes. The simpler fix: read the metadata part as a string form field (`var json = Request.Form["metadata"]`) and deserialize manually (`var metadata = JsonSerializer.Deserialize<DocumentMetadata>(json)`). The more REST-idiomatic fix for simple metadata: flatten `DocumentMetadata` into individual form fields (`title`, `description`) so the binder can populate them directly without JSON parsing. If the metadata is complex, the manual deserialization approach is cleaner than flattening many fields.

---

#### Q5. (P) Implement a **streaming file download** that sets correct **`Content-Disposition`** (attachment vs inline), supports range requests for large PDFs, and avoids loading the entire file into memory. What ASP.NET Core types and headers do you use?

**Concepts**
- PhysicalFile or File(stream) for streaming without full in-memory load
- enableRangeProcessing: true for HTTP Range request support
- Content-Disposition attachment vs inline based on intended client behavior
- Accept-Ranges: bytes header emitted automatically with enableRangeProcessing
- Content-Type matching the file format for correct browser handling

**Answer**

I use `PhysicalFile` for server-disk files since it reads incrementally through a fixed-size internal buffer without loading the file into memory. For attachment downloads where the browser should prompt a save dialog: `return PhysicalFile(path, "application/pdf", Path.GetFileName(path), enableRangeProcessing: true)` — the third argument is `fileDownloadName`, which sets `Content-Disposition: attachment; filename="..."` automatically. For inline display where the browser should render the PDF directly: `return PhysicalFile(path, "application/pdf", enableRangeProcessing: true)` without the download name argument, which omits the `Content-Disposition` header so the browser renders based on content type. The `enableRangeProcessing: true` parameter emits `Accept-Ranges: bytes` in the response and handles `Range:` request headers with **206 Partial Content** responses including `Content-Range` headers automatically. For files stored in cloud blob storage rather than local disk: `var stream = await _blob.OpenReadAsync(); return File(stream, "application/pdf", "report.pdf", enableRangeProcessing: true)` — the blob SDK stream is seekable so range processing works correctly. Always use `Path.GetFileName` on server-side paths to prevent path traversal issues, and set `Content-Type` to match the actual file format rather than `application/octet-stream` for formats the browser can render.

---

#### Q6. (D) Design an upload pipeline: receive `IFormFile` → virus scan → store in blob storage → return 201. Where does scanning run (middleware vs action vs background queue), how do you avoid **memory buffering** the whole file before scan, and what HTTP status do you return if scan times out?

**Concepts**
- Streaming upload avoiding full in-memory file before scan
- [DisableFormValueModelBinding] and MultipartReader for section-by-section streaming
- Synchronous inline scan blocking the response vs background queue returning 202 Accepted
- Scan timeout returning 503 Service Unavailable or 504 Gateway Timeout
- Virus scan pipeline using CopyToAsync piping stream section to scanner

**Answer**

There are two viable pipeline designs depending on whether the client must know the scan result synchronously. For synchronous scanning: disable form model binding on the action with `[DisableFormValueModelBinding]`, read the request body section-by-section with `MultipartReader`, and pipe each file section's stream to the virus scanner and then to blob storage in a single pass — this avoids loading the file into memory because `MultipartReader` exposes a raw section stream that you `CopyTo` through the scanner pipeline to the destination. The action blocks until scanning and upload complete, then returns 201 Created with the blob location. If the scan times out (scanner unresponsive), return **503 Service Unavailable** with a `Retry-After` header rather than 500, since the scanner being unavailable is an upstream dependency failure, not an application error. For asynchronous scanning: accept the upload with standard `IFormFile`, stream to blob storage, enqueue a background scan job, and immediately return **202 Accepted** with a `Location` header pointing at a status-check endpoint. The client polls or subscribes to know when the file is approved. The asynchronous pattern is better for large files and slow scanners since it does not tie up an HTTP connection for the scan duration. Never run virus scan in global middleware for every request — it only makes sense on upload endpoints.

---

#### Q7. (P) An API streams millions of log records to a data warehouse client. Implement **`IAsyncEnumerable<T>`** JSON streaming from a minimal API or controller. What formatter/signature do you use, and what breaks if you return `List<T>` instead?

**Concepts**
- IAsyncEnumerable<T> return type enabling incremental JSON array serialization
- EF Core AsAsyncEnumerable() avoiding full materialization
- System.Text.Json serializing IAsyncEnumerable to chunked output natively
- [EnumeratorCancellation] CancellationToken stopping enumeration on disconnect
- List<T> return type causing full materialization before the first byte is sent

**Answer**

I return `IAsyncEnumerable<LogRecord>` from the action and EF Core provides the source with `.AsAsyncEnumerable()` on the query rather than `.ToListAsync()`. System.Text.Json in ASP.NET Core 8 serializes `IAsyncEnumerable<T>` natively to a JSON array, writing each element to the response body as it is produced rather than buffering the full array. The `CancellationToken` with `[EnumeratorCancellation]` attribute stops enumeration when the client disconnects, so the database query stops processing rather than running to completion for a dead client. If I return `List<T>` instead, EF Core must first execute `ToListAsync()` which materializes all millions of records into a `List<LogRecord>` in heap memory, then System.Text.Json serializes the full list at once — memory usage scales with record count rather than staying constant, causing OOM under concurrent requests. The incremental benefit also requires proxy configuration: the reverse proxy must not buffer the entire response before forwarding it to the data warehouse client, otherwise the client receives all bytes at once when the proxy finishes buffering, defeating the streaming benefit. Set `proxy_buffering off` in nginx or equivalent for the streaming endpoint.

---

#### Q8. (R) Review this upload + validation code. Binding works for small files; large uploads fail silently with empty `IFormFile` and no error body.

```csharp
[HttpPost("archive")]
[DisableFormValueModelBinding] // custom filter — misunderstood
public async Task<IActionResult> UploadArchive(IFormFile archive)
{
    if (archive is null || archive.Length == 0)
        return BadRequest("No file");

    await _storage.SaveAsync(archive.OpenReadStream());
    return Accepted();
}

// Startup — global limit
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = long.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});
```

The custom filter was copied from a streaming sample that parses multipart manually.

**Concepts**
- [DisableFormValueModelBinding] preventing IFormFile binding — parameter is always null
- MemoryBufferThreshold = int.MaxValue forcing all uploads into heap memory
- Mutually exclusive patterns: IFormFile binding vs manual MultipartReader
- Silent null IFormFile with no validation error when binding is disabled
- int.MaxValue buffer threshold causing OOM under concurrent large uploads

**Answer**

`[DisableFormValueModelBinding]` disables the form reader that populates `IFormFile` parameters — `archive` is always null regardless of file size, which is why the action returns `BadRequest("No file")` silently for every upload. The filter was copied from a sample that uses `MultipartReader` to parse parts manually, where `[DisableFormValueModelBinding]` is required to prevent double-reading the request body. The two patterns are mutually exclusive: if you want `IFormFile` binding, remove `[DisableFormValueModelBinding]`; if you need streaming section-by-section processing, keep the filter but remove the `IFormFile` parameter and use `MultipartReader` in the action body instead. The second issue is `MemoryBufferThreshold = int.MaxValue` in the global `FormOptions` — this forces every multipart section to be buffered entirely in memory rather than spilling to a temp file at 64 KB. For large concurrent uploads, every request holds the full file in heap memory simultaneously, which causes OOM. Set `MemoryBufferThreshold` back to the default (65536) or a small value appropriate for your expected metadata parts, and let file parts spill to disk temp files as intended.
