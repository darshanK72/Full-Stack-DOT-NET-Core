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

## Gotchas — File Upload & Streaming Responses (Interview Traps)

---

#### Gotcha 1. `IFormFile` buffers entire file in memory or disk before action runs

**Concepts**
- `IFormFile` — default behavior buffers file to temp disk, but `OpenReadStream()` re-reads it
- Large files with `IFormFile` — exceeds Kestrel's default 30 MB request body limit
- `[DisableRequestSizeLimit]` + streaming via `Request.Body` for large files
- `MultipartReader` for streaming multipart without buffering

**Answer**

`IFormFile` buffers the uploaded file to disk (temp file) before the action runs, which means the entire file is written to disk before any application code executes. For very large files (video, database dumps) the default Kestrel request body size limit of 30 MB causes a `413 Request Entity Too Large` before the action is ever reached. For large file streaming I use `[DisableRequestSizeLimit]` with `[RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]` and read `Request.Body` directly with `MultipartReader`, processing the stream without buffering the entire file into memory.

---

#### Gotcha 2. `[RequestSizeLimit]` vs Kestrel `MaxRequestBodySize` — which wins

**Concepts**
- Kestrel `MaxRequestBodySize` — infrastructure-level limit, enforced by the server
- `[RequestSizeLimit]` — attribute-level limit, enforced by ASP.NET Core middleware
- Kestrel's limit is a hard ceiling — `[RequestSizeLimit]` cannot exceed it
- Setting `MaxRequestBodySize = null` on Kestrel endpoint to allow unlimited

**Answer**

`[RequestSizeLimit(100_000_000)]` sets a 100 MB limit at the framework level, but if Kestrel's `MaxRequestBodySize` (default 30 MB) is not also increased, Kestrel terminates the connection before the attribute is ever consulted. Both limits must be configured in harmony: I increase `MaxRequestBodySize` at the Kestrel endpoint level to at least the maximum expected file size, and use `[RequestSizeLimit]` to enforce per-action limits within the application layer. Setting `MaxRequestBodySize = null` at the Kestrel level disables the infrastructure limit and delegates control entirely to `[RequestSizeLimit]` or `[DisableRequestSizeLimit]`.

---

#### Gotcha 3. Multipart form binding — mixing `IFormFile` with complex DTO

**Concepts**
- `[FromForm]` on DTO + `IFormFile` on file parameter — both bind from multipart
- `[FromBody]` and `IFormFile` in the same action — body claimed twice, one binds null
- `[ApiController]` infers complex type as `[FromBody]`, conflicting with `IFormFile`
- Explicit `[FromForm]` attribute required on the DTO when combined with file

**Answer**

When a multipart form upload contains both a file and metadata fields, the action must use `[FromForm]` on the metadata DTO — `[ApiController]`'s binding source inference would infer `[FromBody]` on a complex type, which conflicts with `IFormFile` because both attempt to read the request body. I explicitly annotate `([FromForm] CreateDocumentDto metadata, IFormFile file)` to direct both parameters to the multipart form binding source. Clients must send the request with `Content-Type: multipart/form-data` and place the JSON fields as individual form fields, not as a JSON blob within the multipart.

---

#### Gotcha 4. Not disposing `IFormFile` stream after processing

**Concepts**
- `IFormFile` backed by a `Stream` pointing to a temp file on disk
- Forgetting to dispose the stream from `IFormFile.OpenReadStream()` — file handle leaked
- `using` block around the stream from `IFormFile.OpenReadStream()`
- `IFormFile` itself implements `IDisposable` in some implementations

**Answer**

`IFormFile.OpenReadStream()` returns a `Stream` that must be disposed after use — it may point to a temp file handle or a memory stream depending on the file size threshold. Forgetting to dispose leaks file handles, which under load exhausts available file descriptors and causes `IOException` on subsequent requests. I always open the stream in a `using` block: `using var stream = formFile.OpenReadStream();` and process within it, ensuring the handle is released when the block exits whether or not an exception occurs.

---

#### Gotcha 5. `FileStreamResult` vs `FileContentResult` for large file downloads

**Concepts**
- `FileContentResult` — reads entire file into `byte[]` in memory before sending
- `FileStreamResult` — streams the file without fully buffering in application memory
- `PhysicalFileResult` — serves directly from disk without reading into memory
- `EnableRangeProcessing = true` — supports `Range` header for partial downloads (resume support)

**Answer**

`FileContentResult(File.ReadAllBytes(path), "application/octet-stream")` loads the entire file into a `byte[]` before sending the response, which doubles memory usage for large files. `FileStreamResult(File.OpenRead(path), "application/octet-stream")` streams the file directly from disk to the response body without buffering. `PhysicalFileResult(path, "application/octet-stream")` is even more efficient as it delegates to the underlying file serving infrastructure. I use `PhysicalFileResult` for files on the local filesystem and set `EnableRangeProcessing = true` to support resumable downloads via the `Range` request header.

---

#### Gotcha 6. Returning `IAsyncEnumerable<T>` — status code cannot change after first write

**Concepts**
- `IAsyncEnumerable<T>` from controller action — `System.Text.Json` streams JSON array progressively
- Response headers already sent after first row — exceptions mid-stream cannot change status code
- Connection cancellation not handled — `CancellationToken` on action parameter
- Client receives partial JSON array on mid-stream error — looks like a 200 with corrupt JSON

**Answer**

Returning `IAsyncEnumerable<T>` from a controller action causes `System.Text.Json` to write JSON array elements to the response stream progressively as each element is produced. Once the first byte is written, the HTTP status code and headers are already sent — if a database error occurs mid-stream, the response cannot change to `500`. The client receives a partial JSON array followed by a stream close, which looks like a successful 200 with corrupt JSON. I accept a `CancellationToken` parameter on the action to handle client disconnection, and only use streaming for read-only queries where exceptions mid-stream are unexpected.

---

#### Gotcha 7. `Content-Disposition` header for inline vs attachment downloads

**Concepts**
- `Content-Disposition: attachment; filename="report.pdf"` — browser prompts download dialog
- `Content-Disposition: inline; filename="image.png"` — browser renders in tab
- Missing `Content-Disposition` — browser decides based on `Content-Type` alone
- `FileDownloadName` on `FileStreamResult` sets `Content-Disposition: attachment`

**Answer**

Without an explicit `Content-Disposition` header, the browser uses heuristics based on `Content-Type` to decide whether to display or download the file. `FileStreamResult` sets `Content-Disposition: attachment` automatically when `FileDownloadName` is provided. For images or PDFs that should open in the browser rather than downloading, I construct the `FileStreamResult` without `FileDownloadName` and manually set `Response.Headers["Content-Disposition"] = "inline"`, which gives the browser the explicit instruction rather than relying on content-type heuristics.

---

#### Gotcha 8. Chunked transfer encoding buffered by reverse proxy

**Concepts**
- Kestrel sends streaming responses as chunked transfer encoding by HTTP/1.1
- Reverse proxy with response buffering enabled — buffers the full stream before forwarding
- nginx `proxy_buffering off` — required for true streaming to browser
- `X-Accel-Buffering: no` header disabling nginx buffering per response

**Answer**

A streaming response from Kestrel is chunked by HTTP/1.1 semantics, but a reverse proxy sitting between Kestrel and the browser may buffer the full response before forwarding it — negating the streaming benefit and causing the browser to wait for the entire file before receiving any bytes. nginx's `proxy_buffering` is on by default, which buffers streaming API responses entirely. I add `Response.Headers["X-Accel-Buffering"] = "no"` to streaming endpoints or configure `proxy_buffering off` in the nginx location block for streaming paths. This is especially important for Server-Sent Events and `IAsyncEnumerable` API endpoints.

---

#### Gotcha 9. Antiforgery token validation blocking multipart file upload from SPA

**Concepts**
- `ValidateAntiForgeryToken` applied globally or to controllers — blocks cross-origin forms
- SPA sending multipart without antiforgery token header — 400 error
- Antiforgery appropriate for browser form submissions, not token-authenticated API calls
- `[IgnoreAntiforgeryToken]` on file upload actions using JWT authentication

**Answer**

If antiforgery validation is enabled globally (for example, via a base controller filter), a React or Angular SPA uploading a file via `FormData` and `fetch()` will receive a `400 Bad Request` unless it also sends the antiforgery token cookie and header. SPAs using JWT bearer authentication do not benefit from antiforgery protection because the token is already a credential proof — antiforgery protects against cross-site request forgery for cookie-authenticated sessions. I apply `[IgnoreAntiforgeryToken]` to file upload actions in token-authenticated APIs, reserving antiforgery for cookie-authenticated MVC controllers.

---

#### Gotcha 10. Missing `multipart/form-data` per-part size limit for multiple files

**Concepts**
- Multiple `IFormFile` uploads — total size exceeds single-file limit
- `[RequestFormLimits]` controlling `MultipartBodyLengthLimit` and `MultipartHeadersLengthLimit`
- Kestrel limit applies to total request body, not individual files
- `MultipartBoundaryLengthLimit` default 128 bytes — custom boundaries may exceed it

**Answer**

`[RequestSizeLimit]` applies to the total request body, but for multipart form data the `MultipartBodyLengthLimit` in `RequestFormLimits` controls the maximum size of each individual part (file). Uploading five 20 MB files requires a total request size limit of at least 100 MB and a `MultipartBodyLengthLimit` of at least 20 MB per part. I use `[RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]` to set the per-part limit alongside `[RequestSizeLimit(250_000_000)]` for the total body. I also check `MultipartHeadersLengthLimit` for requests with very long metadata headers, since the default 16 KB limit can be exceeded by Base64-encoded inline content in headers.

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
