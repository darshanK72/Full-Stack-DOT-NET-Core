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

What is `IFormFile` in ASP.NET Core Web API?

**Answer:** `IFormFile` represents an uploaded file sent as part of a `multipart/form-data` request. It exposes the file name, content type, length, and `OpenReadStream()` for reading bytes without loading the entire file into memory upfront when configured correctly.

- Bind as an action parameter on POST/PUT endpoints accepting multipart form data.
- Use `CopyToAsync` to stream to disk, blob storage, or virus scanner — avoid reading all bytes for large files.
- Validate extension, content type, and size before persisting — never trust client-provided `FileName` for paths.
- Available on `[ApiController]` actions with `[FromForm]` or implicit form binding for file parameters.
- For JSON-only APIs, file upload requires switching to multipart — `IFormFile` does not bind from raw JSON bodies.

---

## Q2. What is `multipart/form-data`?

What is `multipart/form-data`?

**Answer:** `multipart/form-data` is an HTTP content type for requests containing a mix of files and form fields, each sent as a separate part with its own headers. Browsers and HTTP clients use it for file uploads; ASP.NET Core model binding maps parts to `IFormFile` and form properties.

- `Content-Type: multipart/form-data; boundary=----WebKitFormBoundary...` delimits parts in the body.
- Each part can have a name matching the parameter (`file`, `title`, `metadata`).
- Required for binary file upload — JSON cannot efficiently embed large binary payloads.
- ASP.NET Core parses multipart via form readers with configurable size and memory thresholds.
- OpenAPI documents multipart endpoints with `requestBody.content.multipart/form-data` schema.

---

## Q3. What does `[FromForm]` do for file uploads?

What does `[FromForm]` do for file uploads?

**Answer:** `[FromForm]` tells model binding to read the parameter from form fields in a multipart or URL-encoded request body rather than from route, query, or JSON body. Apply it to `IFormFile` and companion metadata properties in mixed upload actions.

- Without correct binding source, complex upload actions may fail to bind file or metadata.
- Combine `IFormFile file` and `[FromForm] DocumentMetadata metadata` in one multipart request.
- JSON nested in a single multipart part is not auto-deserialized into POCOs — flatten fields or deserialize manually.
- `[ApiController]` infers `[FromForm]` for simple types in some cases but explicit attributes clarify intent.
- Not used for raw binary POST bodies — those require manual stream reading or custom formatters.

---

## Q4. What is the `[RequestSizeLimit]` attribute?

What is the `[RequestSizeLimit]` attribute?

**Answer:** `[RequestSizeLimit(bytes)]` raises or lowers the maximum allowed request body size for a specific action or endpoint, overriding the global Kestrel default (~30 MB). It must align with Kestrel, `FormOptions`, and reverse proxy limits or uploads still fail with 413.

- Apply to the action handling large uploads: `[RequestSizeLimit(1_073_741_824)]` for 1 GB.
- Works with `IHttpMaxRequestBodySizeFeature` for per-request overrides in minimal APIs.
- Does not alone fix multipart limits — pair with `[RequestFormLimits(MultipartBodyLengthLimit = ...)]`.
- Setting a limit is also a security control — reject unexpectedly large bodies early.
- Global default remains conservative; opt-in large limits only on import endpoints.

---

## Q5. What is the difference between buffering and streaming a file download?

What is the difference between buffering and streaming a file download?

**Answer:** **Buffering** reads the entire file into memory (e.g., `byte[]` or `ReadAllBytesAsync`) before sending the response — simple but causes OOM under concurrent large downloads. **Streaming** sends bytes as they are read from disk or blob storage via `FileStreamResult` — constant memory regardless of file size.

- `return File(bytes, contentType)` buffers; `return File(stream, contentType)` streams.
- Streaming improves time-to-first-byte and supports large files (GB+) safely.
- Streaming enables range requests when `enableRangeProcessing: true` for seek/resume.
- Always dispose streams — `FileStreamResult` handles disposal after response completes.
- Cloud downloads should pipe blob `OpenReadAsync` directly to the response body.

---

## Q6. What is the `Content-Disposition` header?

What is the `Content-Disposition` header?

**Answer:** `Content-Disposition` tells the client how to handle the response body — typically as an attachment to download or inline to display in the browser. For file downloads it includes `filename` or `filename*` (UTF-8 encoded name).

- `Content-Disposition: attachment; filename="report.pdf"` prompts save dialog.
- `Content-Disposition: inline; filename="report.pdf"` opens in browser if content type supports it.
- Set via `ControllerBase.File(..., fileDownloadName)` or `ContentDispositionHeaderValue` in minimal APIs.
- Important for APIs returning binary — JSON responses do not use Content-Disposition.
- RFC 5987 `filename*` supports non-ASCII filenames.

---

## Q7. What is the difference between attachment and inline Content-Disposition?

What is the difference between attachment and inline Content-Disposition?

**Answer:** **`attachment`** instructs the client to save the file locally or open with an external application. **`inline`** instructs the client to display the content within the browser window when the content type is renderable (PDF, images).

- Use attachment for exports, installers, and sensitive documents users should not preview in-browser.
- Use inline for PDFs or images meant to be viewed directly in a tab or embedded viewer.
- Same file bytes — only the disposition and client behavior differ.
- Mobile clients may treat both similarly but attachment is safer default for unknown file types.
- ASP.NET Core `fileDownloadName` parameter on `File()` results typically sets attachment disposition.

---

## Q8. What HTTP status does 413 Payload Too Large indicate?

What HTTP status does 413 Payload Too Large indicate?

**Answer:** **413 Payload Too Large** means the server refused to process the request because the body exceeds configured size limits — Kestrel `MaxRequestBodySize`, `FormOptions.MultipartBodyLengthLimit`, `[RequestSizeLimit]`, or reverse proxy body limits.

- Occurs before model binding completes — action code never runs.
- Distinct from 400 validation failure — the body was not accepted at all due to size.
- Fix by aligning limits at Kestrel, form options, attribute, and nginx/IIS `client_max_body_size`.
- Return ProblemDetails from custom middleware if you intercept size violations with a friendly message.
- Clients should chunk large uploads or use presigned direct-to-blob upload URLs when 413 persists.

---

## Q9. What are `FormOptions` in ASP.NET Core?

What are `FormOptions` in ASP.NET Core?

**Answer:** `FormOptions` configures how ASP.NET Core parses form and multipart requests — limits on body length, number of headers, individual multipart section size, and memory buffering threshold before spilling to disk temp files.

- Configure via `services.Configure<FormOptions>(options => { ... })` or `[RequestFormLimits(...)]` per action.
- `MultipartBodyLengthLimit` defaults to 128 MB — can block uploads below Kestrel limit.
- `ValueLengthLimit` and `KeyLengthLimit` cap individual form field sizes.
- `MultipartHeadersCountLimit` prevents header explosion attacks in multipart requests.
- Must align with `[RequestSizeLimit]` and Kestrel for consistent upload behavior.

---

## Q10. What role do Kestrel limits play in request body size?

What role do Kestrel limits play in request body size?

**Answer:** Kestrel enforces `Limits.MaxRequestBodySize` (default ~30 MB) at the server level before ASP.NET Core middleware and model binding see the body. No action-level attribute can allow larger uploads unless Kestrel (or `IHttpMaxRequestBodySizeFeature`) permits it first.

- Configure globally: `builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = ...)`.
- Set to `null` to disable limit (not recommended publicly without other guards).
- Per-endpoint override via `IHttpMaxRequestBodySizeFeature.DisableMaxRequestBodySize` or max size feature on HttpContext.
- First gate in the stack — proxy limits (nginx, Azure Front Door) are equally critical.
- 413 from Kestrel never reaches `[RequestSizeLimit]` logic — configure both layers explicitly.

---

## Q11. What is `IAsyncEnumerable` streaming for API responses?

What is `IAsyncEnumerable` streaming for API responses?

**Answer:** Returning `IAsyncEnumerable<T>` from a controller or minimal API lets ASP.NET Core serialize items incrementally as they are produced — chunked JSON array output instead of materializing millions of records in a `List<T>` first.

- EF Core: return query as `AsAsyncEnumerable()` — do not call `ToListAsync()` before returning.
- Improves time-to-first-byte and bounds memory for large read-only exports (logs, events).
- Use `[EnumeratorCancellation]` on `CancellationToken` parameter to cancel enumeration when client disconnects.
- System.Text.Json in ASP.NET Core 8 supports async enumeration for JSON responses.
- Reverse proxies may buffer responses — disable buffering on streaming endpoints at the gateway.

---

## Q12. What is the difference between `File()` and `PhysicalFileResult`?

What is the difference between `File()` and `PhysicalFileResult`?

**Answer:** Both stream file content to the client. `PhysicalFileResult` (returned by `PhysicalFile(path, contentType, fileDownloadName)`) reads from a file path on disk. `File(stream, ...)` or `File(bytes, ...)` accepts an open stream or byte array — use stream overload for streaming, bytes for small files only.

- `PhysicalFile` is convenient when the file already exists on server filesystem.
- `FileStreamResult` from `File(stream, ...)` works for any readable stream including blob SDK streams.
- Both support `enableRangeProcessing` for HTTP Range requests.
- `VirtualFileResult` serves from embedded resources or IFileProvider content roots.
- Avoid `File(byte[])` for large files — use stream-based overloads exclusively in Production.

---

## Q13. What is a streaming response in Web APIs?

What is a streaming response in Web APIs?

**Answer:** A streaming response sends the HTTP body incrementally as data is generated or read, rather than buffering the full payload before the first byte. Examples include `FileStreamResult`, `IAsyncEnumerable<T>` JSON, NDJSON line streams, and SSE.

- Reduces memory pressure and latency for large downloads and exports.
- Client receives chunked transfer encoding when Content-Length is unknown.
- CancellationToken propagates to stop production when the client disconnects.
- Proxies and load balancers must not buffer entire responses for streaming to work end-to-end.
- Distinct from upload streaming — both directions benefit from avoiding full-body buffering.

---

## Q14. What is `[DisableFormValueModelBinding]`?

What is `[DisableFormValueModelBinding]`?

**Answer:** `[DisableFormValueModelBinding]` is a filter that disables automatic form model binding for an action so the developer can manually parse multipart data with `MultipartReader` for true streaming uploads without buffering the entire body in memory first.

- When applied, `IFormFile` parameters will **not** bind — they remain null.
- Used in advanced streaming upload samples — not compatible with standard `IFormFile` actions on the same endpoint.
- Choose one pattern: standard `IFormFile` binding **or** manual multipart parsing, not both mixed incorrectly.
- Pair with reading `Request.Body` directly and section-by-section processing for virus scan pipelines.
- Removing the attribute restores normal form binding if you switch back to standard upload handling.

---

## Q15. What is the difference between uploading via JSON vs multipart?

What is the difference between uploading via JSON vs multipart?

**Answer:** **JSON** (`application/json`) suits metadata and small base64-encoded payloads but is inefficient and impractical for large binary files. **Multipart** (`multipart/form-data`) sends binary files as native binary parts alongside form fields — the standard for file uploads in HTTP APIs.

- JSON base64 inflates size ~33% and requires full body parsing in memory.
- Multipart streams file parts with boundaries; ASP.NET binds to `IFormFile`.
- OpenAPI: JSON endpoints use `application/json` schema; upload endpoints document `multipart/form-data`.
- Mixed APIs often use multipart for create-with-file and JSON for metadata-only updates.
- Presigned URL upload to blob storage is a third pattern — client uploads directly to storage, API receives JSON notification only.

---

## Q16. What is range request support for large files?

What is range request support for large files?

**Answer:** HTTP Range requests let clients request byte subsets of a resource (`Range: bytes=0-1023`) for resume, seek, and partial downloads. Enable with `enableRangeProcessing: true` on `FileResult` — ASP.NET Core responds with **206 Partial Content** and `Content-Range` header.

- Essential for video, PDF viewers, and download managers that resume interrupted transfers.
- Server must support seeking on the underlying stream or file.
- `Accept-Ranges: bytes` header advertises capability to clients.
- Without range support, clients re-download entire multi-GB files after connection drops.
- `PhysicalFile(path, contentType, name, enableRangeProcessing: true)` is the typical Web API pattern.

---

## Q17. What is `MemoryBufferThreshold` in FormOptions?

What is `MemoryBufferThreshold` in FormOptions?

**Answer:** `MemoryBufferThreshold` (default 64 KB) controls how much of each multipart section is buffered in memory before ASP.NET Core spills overflow to a temporary disk file. Lower values reduce RAM use; setting it to `int.MaxValue` forces full in-memory buffering.

- Large uploads should spill to disk temp — do not raise threshold to max unless you accept OOM risk.
- Works per multipart section — file parts and form fields each have independent buffering behavior.
- Tuning affects performance on high-concurrency upload servers.
- Related to `MultipartBodyLengthLimit` — threshold is per-section memory, limit is total multipart size.
- Streaming upload tutorials manipulate this setting — misconfiguration breaks expected memory profile.

---

## Q18. How does a reverse proxy affect large file uploads?

How does a reverse proxy affect large file uploads?

**Answer:** Reverse proxies (nginx, IIS ARR, Azure Application Gateway, Cloudflare) impose their own body size limits, timeouts, and buffering behavior that can reject or truncate uploads before Kestrel sees them — causing 413, 502, or silent timeouts in Production only.

- nginx: `client_max_body_size` must meet or exceed API `[RequestSizeLimit]`.
- IIS: `maxAllowedContentLength` in web.config.
- Proxy read/send timeouts must exceed worst-case upload duration for large files on slow networks.
- Response buffering on proxy can break streaming downloads and `IAsyncEnumerable` JSON exports.
- Align limits and timeouts across proxy, Kestrel, and FormOptions; test large uploads through Production-like path, not just direct Kestrel.

---

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

#### Gotcha 1. POST returning 200 instead of 201

**Answer:** A successful resource creation with POST should return HTTP 201 Created and tell the client where the new resource lives — returning 200 OK omits that contract and breaks REST clients that rely on status codes and the Location header.

- Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with a Location header pointing at the new resource URL.
- Include the created representation or a minimal payload in the response body when clients need immediate data without a follow-up GET.
- Returning 200 for create operations hides the new resource URL from standard HTTP client libraries and OpenAPI-generated SDKs.

---

#### Gotcha 2. GET that mutates state

**Answer:** GET must be safe and idempotent — performing deletes or updates on GET violates HTTP semantics, breaks caching proxies, and creates security holes when URLs are prefetched, logged, or opened in email clients.

- Browsers, CDNs, and link-preview crawlers may invoke GET URLs without user intent, so side effects run unintentionally.
- Cached GET responses can replay destructive operations or stale mutations across clients.
- Use POST, PUT, PATCH, or DELETE for state changes and keep GET read-only.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Answer:** Business failures must map to appropriate 4xx or 5xx status codes — a 200 response with an error flag forces every client to parse the body instead of using standard HTTP semantics, retries, and monitoring.

- Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures and 404, 409, or 422 for domain errors.
- HTTP status codes drive client retry logic, API gateways, and APM alerting; a 200 masks failures in dashboards.
- Envelope patterns like `{ success: false }` require custom handling in every consumer and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Answer:** EF Core entities expose navigation properties, shadow fields, and circular references that are not meant for public contracts — serialize DTOs with explicit shapes and never leak database schema to clients.

- Lazy-loaded navigations trigger N+1 queries during serialization and can pull entire object graphs into the response.
- Circular references between entities cause JSON serializer loops or require fragile reference-handling settings.
- DTOs decouple the API contract from schema migrations and let you expose only the fields clients need.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Answer:** ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json` — PascalCase property names from some clients bind as missing properties, leaving model properties at default values and causing silent data loss on POST and PUT.

- `[JsonPropertyName("PropertyName")]` or a custom `PropertyNamingPolicy` aligns server expectations with legacy client payloads.
- Enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)` when you must accept mixed casing.
- Silent binding failures produce 201/204 success responses with partially saved data and no validation error.

---

#### Gotcha 6. GET with `[FromBody]`

**Answer:** Many HTTP clients, proxies, and caches ignore or strip GET request bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8 Web API.

- Model binding for `[FromBody]` on GET is not reliably supported across the HTTP ecosystem.
- Use query strings with `[FromQuery]` for simple filters or POST to a dedicated search endpoint for complex filter objects.
- OpenAPI tools and browser fetch also discourage or block GET bodies, making the pattern fragile in production.

---

#### Gotcha 7. CORS as server security

**Answer:** CORS is enforced by browsers only — it does not stop curl, Postman, server-to-server calls, or direct API requests; authentication and authorization still protect the API.

- CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they do not authenticate callers.
- A public API without auth remains fully accessible to any non-browser client regardless of CORS policy.
- Register `AddCors` and `UseCors` for browser SPA access, and enforce JWT, cookies, or API keys separately for real security.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Answer:** Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers — you must specify explicit origins with `WithOrigins` and call `AllowCredentials`.

- `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined; ASP.NET Core will not emit a valid CORS response for credentialed requests.
- List every trusted frontend origin explicitly, including local dev URLs and production domains.
- Credentialed cross-origin calls require both matching origins and `Access-Control-Allow-Credentials: true`.

---

#### Gotcha 9. Swagger UI exposed in Production

**Answer:** Public Swagger UI discloses the full API surface, schemas, and try-it-out access — gate it behind authentication or disable it outside Development and Staging in ASP.NET Core 8.

- `MapSwagger` and `UseSwaggerUI` in `Program.cs` should be wrapped in environment checks or authorization middleware.
- Exposed OpenAPI documents reveal internal endpoints, field names, and enum values useful for reconnaissance.
- Production APIs typically serve OpenAPI only to authenticated developers or internal tooling, not the public internet.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Answer:** Without `[ApiController]`, automatic 400 `ValidationProblemDetails`, binding source inference, and attribute routing behaviors differ — mixed controllers in the same Web API produce inconsistent error contracts.

- `[ApiController]` enables automatic model-state validation responses and `[FromBody]` inference for complex types.
- Controllers missing the attribute may return 200 with invalid models or require manual `ModelState` checks.
- Apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Answer:** Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation and deadlocks under load — always `await` async service and database calls in ASP.NET Core 8.

- Sync-over-async ties up request threads while I/O completes, reducing throughput on Kestrel under concurrent load.
- Deadlocks occur when the blocked thread holds a synchronization context the continuation needs to resume.
- Mark controller actions `async Task<IActionResult>` and propagate `await` through the service layer to EF Core and HTTP clients.

---

#### Gotcha 12. Liveness probe includes SQL check

**Answer:** If the liveness probe fails when SQL is down, Kubernetes restarts pods that cannot fix the dependency — put SQL, Redis, and external service checks on readiness only.

- Liveness answers whether the process should be killed and restarted; a down database is not healed by restarting the app.
- Readiness removes the pod from the load balancer until dependencies recover without unnecessary restarts.
- Map `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck` or custom dependency tags.

---

#### Gotcha 13. N+1 queries in list endpoints

**Answer:** Returning entities with lazy-loaded navigation properties triggers one SQL query per row — use projection with `Select`, explicit `Include`, or DTO mapping to fetch list data in a bounded number of queries.

- Serializing a list of `Order` entities with `Customer` navigation can execute 1 + N queries under default lazy loading.
- Project directly to DTOs in LINQ so EF Core generates a single query with only the columns needed.
- For graphs that must be included, use `Include`/`ThenInclude` or split queries deliberately rather than relying on lazy load during JSON output.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Answer:** Concurrent inserts and deletes between offset pages cause duplicate or skipped rows — use keyset or cursor pagination ordered by a stable, indexed key for large datasets in Web API list endpoints.

- `Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are added or removed between requests.
- Keyset pagination uses `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key from the previous response.
- Offset pagination remains acceptable for small, mostly static tables; expose cursor tokens in link headers or response metadata for high-churn data.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Answer:** Field resolvers in HotChocolate or other GraphQL servers that query the database per parent row explode SQL under load — batch related loads with DataLoader or resolve joins at the root query.

- A list of 100 authors each resolving `books` individually executes 101 queries instead of one batched query.
- Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips.
- Eager-load or project at the root query when the client always requests nested fields together.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Answer:** Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript — browser clients need gRPC-Web middleware plus CORS configuration in ASP.NET Core 8.

- Standard `@grpc/grpc-js` in Node or .NET clients works server-to-server; Blazor WASM and SPA browsers require the gRPC-Web protocol.
- Add `AddGrpcWeb()` and `EnableGrpcWeb()` on mapped gRPC services to translate between gRPC-Web and native gRPC.
- Configure CORS for the browser origin alongside gRPC-Web, since cross-origin browser calls still enforce CORS on preflight and response headers.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

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

---

**Answer:**

_Answer not found._

---

#### Q2. (P) Configure **request body size limits** for a Web API that accepts CSV imports up to 1 GB via `IFormFile` while keeping default 30 MB limits on all other endpoints. What do you set in Kestrel, `FormOptions`, and action attributes?

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

---

#### Q5. (P) Implement a **streaming file download** that sets correct **`Content-Disposition`** (attachment vs inline), supports range requests for large PDFs, and avoids loading the entire file into memory. What ASP.NET Core types and headers do you use?

---

**Answer:**

_Answer not found._

---

#### Q6. (D) Design an upload pipeline: receive `IFormFile` → virus scan → store in blob storage → return 201. Where does scanning run (middleware vs action vs background queue), how do you avoid **memory buffering** the whole file before scan, and what HTTP status do you return if scan times out?

---

**Answer:**

_Answer not found._

---

#### Q7. (P) An API streams millions of log records to a data warehouse client. Implement **`IAsyncEnumerable<T>`** JSON streaming from a minimal API or controller. What formatter/signature do you use, and what breaks if you return `List<T>` instead?

---

**Answer:**

_Answer not found._

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

**Answer:**

_Answer not found._

---
