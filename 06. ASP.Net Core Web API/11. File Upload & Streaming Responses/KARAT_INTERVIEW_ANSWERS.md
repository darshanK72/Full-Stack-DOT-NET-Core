# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/11. File Upload & Streaming Responses`

---

#### Q1. (R) QA uploads 200 MB in test; Production returns 413. `[RequestSizeLimit]` on action.

**Answer:** Upload limits stack at multiple layers — **`[RequestSizeLimit]`** only raises the ASP.NET Core request limit for that action; **Kestrel's `MaxRequestBodySize`**, reverse proxy body limits, and **`FormOptions.MultipartBodyLengthLimit`** may still reject the body before model binding reaches the action.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Hosting | Default Kestrel ~30 MB body limit | 413 before action executes — attribute never consulted |
| Multipart | Default `MultipartBodyLengthLimit` 128 MB | Can block below 200 MB even if Kestrel raised |
| Security | `Path.Combine("/uploads", file.FileName)` | Path traversal via malicious filename |
| Ops | 30s proxy timeout | Large upload may abort mid-stream unrelated to 413 |

**Fix (priority order):**

1. Configure Kestrel: `builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = ...)` or per-endpoint with `IHttpMaxRequestBodySizeFeature`.
2. Raise **`FormOptions.MultipartBodyLengthLimit`** for multipart endpoints (or globally if justified).
3. Align nginx/IIS **`client_max_body_size`** / `maxAllowedContentLength` with API limits.
4. Keep `[RequestSizeLimit]` as endpoint-specific cap — lowest layer must allow it first.
5. Sanitize stored filename — use generated id, not client `FileName`.

**Production takeaway:** **IFormFile limits** are never a single attribute — Kestrel, form options, and proxy must align.

---

#### Q2. (P) Configure limits for 1 GB CSV import on one endpoint; 30 MB elsewhere.

**Answer:** Set global conservative defaults, then opt-in the import route with Kestrel/feature limits, `FormOptions`, and `[RequestSizeLimit]` together — document all three in runbooks.

- **Global Kestrel:** Leave default ~30 MB or set explicitly on `Limits.MaxRequestBodySize`.
- **Import endpoint:** `[RequestSizeLimit(1_073_741_824)]` on action **and** `app.MapPost(...).DisableAntiforgery()` if minimal — configure `IHttpMaxRequestBodySizeFeature` for that route if using Kestrel per-request override.
- **`FormOptions`:** Default `MultipartBodyLengthLimit = 30 * 1024 * 1024`; for import controller only, use filter or:
  ```csharp
  services.Configure<FormOptions>(options => { /* default 30MB */ });
  // Import: [RequestFormLimits(MultipartBodyLengthLimit = 1_073_741_824)]
  ```
- **`[RequestFormLimits(MultipartBodyLengthLimit = ...)]`** pairs with `[RequestSizeLimit]` for multipart uploads.
- **Proxy:** Increase load balancer timeout and body size for `/api/import` path only if possible.

**Production takeaway:** Per-endpoint upload size is **`[RequestSizeLimit]` + `[RequestFormLimits]` + Kestrel + proxy** — missing any layer reproduces 413 in prod only.

---

#### Q3. (R) Download loads entire file into byte[] — OOM under load.

**Answer:** **`File.ReadAllBytesAsync` buffers the entire file in memory** before sending; concurrent 4 GB downloads multiply memory use and trigger OOM kills regardless of "streaming" intent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | Full file in `byte[]` | OOM; GC pressure scales with concurrency × file size |
| Scalability | No true streaming | Thread and socket held until full read completes |
| UX | No range support | PDF/video clients cannot seek |

**Fix (priority order):**

1. Return **`PhysicalFileResult` / `FileStreamResult`** with `FileStream` opened read-only — `return PhysicalFile(path, contentType, fileName, enableRangeProcessing: true);`
2. Prefer **`Results.Stream()`** in minimal APIs for delegate-based streaming.
3. Set **`Content-Length`** when known, or chunked transfer; add **`Content-Disposition: attachment`**.
4. For cloud storage, stream from blob SDK (`OpenReadAsync`) through to response body — never `ReadAllBytes`.
5. Offload very large static assets to CDN/signed URLs when appropriate.

**Production takeaway:** **`File(bytes, ...)` is not streaming** — use stream-based results for large files.

---

#### Q4. (M) Multipart binding — `Metadata` null, `File` populated.

**Answer:** **`IFormFile` binds from form file parts** by name match (`file`). Complex **`DocumentMetadata` requires explicit `[FromForm]`** on the property or a custom binder — JSON in a multipart part is **not** automatically deserialized into nested objects without `[ModelBinder(BinderType = typeof(...))]` or sending metadata fields as separate form fields.

- Default model binder maps simple form fields to properties; JSON blob in one part needs **`[FromForm]`** + JSON input formatter configuration or manual read from `Request.Form["metadata"]`.
- **Fix options:** (1) Flatten — `Title`, `Category` as separate form fields; (2) `[FromForm] string metadataJson` then `JsonSerializer.Deserialize<DocumentMetadata>`; (3) custom **`IFormCollection`** parsing.
- **Client contract:** Either `multipart/form-data` with discrete fields, or `application/json` metadata part with custom binder — document in OpenAPI.

**Production takeaway:** **Multipart binding** treats file and JSON metadata differently — `IFormFile` "just works"; POCOs need explicit strategy.

---

#### Q5. (P) Streaming download with Content-Disposition and range support.

**Answer:** Use **`PhysicalFileResult`** or **`FileStreamResult`** with `enableRangeProcessing: true`; set **`Content-Disposition`** via `ContentDispositionHeaderValue` or the `fileDownloadName` parameter on `Results.File` / `ControllerBase.File`.

```csharp
[HttpGet("{id}/download")]
public IActionResult Download(int id)
{
    var meta = _store.GetMeta(id);
    var stream = _store.OpenRead(id); // FileStream or blob stream — caller owns disposal via result

    return File(
        stream,
        meta.ContentType,
        meta.FileName,
        enableRangeProcessing: true); // sets Accept-Ranges, handles Range: bytes=
}
```

- **Attachment vs inline:** `Content-Disposition: attachment; filename="report.pdf"` forces download; `inline` for browser PDF viewing — `File(..., fileDownloadName)` defaults to attachment.
- **Minimal API:** `Results.File(stream, contentType, fileDownloadName, enableRangeProcessing: true)`.
- **Headers:** Do not buffer to compute length if unknown — omit length or use chunked encoding when stream length unavailable.
- **Async:** Prefer async stream copy via result execution — avoid reading stream to byte[] first.

**Production takeaway:** **`enableRangeProcessing`** + stream result is the production pattern for large PDF/video downloads.

---

#### Q6. (D) Upload pipeline — virus scan, blob storage, avoid full memory buffer.

**Answer:** Stream the upload **directly to temp blob or disk** while computing hash, queue scan job, return **202 Accepted** with status URL — scan synchronously only for small files with strict SLA; never hold entire multi-GB file in RAM.

- **Flow:** Action reads **`IFormFile.OpenReadStream()`** → copy to staging blob in chunks → enqueue scan message (path, hash) → return **202** with `{ id, statusUrl }`.
- **Scan location:** **Background worker** (Azure Function, queue consumer) for production scale; synchronous scan in action only for ≤ N MB with timeout.
- **Avoid buffering:** Do not `CopyToAsync(MemoryStream)` for full file; set **`FormOptions.MemoryBufferThreshold`** so large parts spill to disk temp (default behavior — do not set threshold to `int.MaxValue` unless you understand RAM cost).
- **Scan timeout:** Return **503** or **408** with ProblemDetails — client polls status URL; do not return 201 until scan passes.
- **Failure after upload:** Delete staging blob on scan fail; return **422** or **400** with safe message.

**Production takeaway:** Virus scan is **async pipeline stage** — `[DisableFormValueModelBinding]` + manual multipart parsing when you need true streaming before any full-body buffer.

---

#### Q7. (P) Stream millions of records via `IAsyncEnumerable<T>` JSON.

**Answer:** Return **`IAsyncEnumerable<LogRecord>`** from controller or minimal API — ASP.NET Core serializes with **`System.Text.Json`** incremental async enumeration ( .NET 7+ improved support); client receives chunked JSON array stream instead of one giant payload.

```csharp
[HttpGet("logs/export")]
public IAsyncEnumerable<LogRecord> ExportLogs(CancellationToken ct)
    => _repo.StreamLogsAsync(ct);

// Minimal API
app.MapGet("/logs/export", (ILogRepo repo, CancellationToken ct) => repo.StreamLogsAsync(ct));
```

- **What breaks with `List<T>`:** Entire result materialized in memory; long time-to-first-byte; client timeout; OOM on large exports.
- **Requirements:** `[EnumeratorCancellation]` on token parameter; ensure EF uses **`AsAsyncEnumerable()`** — not `ToListAsync()` first.
- **Content-Type:** `application/json`; newline-delimited JSON (NDJSON) if clients prefer line-by-line via custom formatter.
- **Reverse proxy:** Disable response buffering on gateway for streaming endpoints.

**Production takeaway:** **`IAsyncEnumerable` streaming JSON** is the Web API pattern for large read-only exports — `List<T>` defeats HTTP streaming.

---

#### Q8. (R) `[DisableFormValueModelBinding]` — large uploads fail with null `IFormFile`.

**Answer:** The filter **disables form model binding** so the app can parse multipart manually in streaming samples — with it enabled, **`IFormFile` parameter binding never runs**, so `archive` is always null with no validation ProblemDetails.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding | `[DisableFormValueModelBinding]` on standard upload | IFormFile never bound — silent null |
| Configuration | `MemoryBufferThreshold = int.MaxValue` | Forces entire multipart into memory — OOM on large files opposite of intent |
| Error UX | `BadRequest("No file")` string | Inconsistent with ProblemDetails API |

**Fix (priority order):**

1. **Remove** `[DisableFormValueModelBinding]` unless action manually reads `MultipartReader` — use streaming sample pattern end-to-end or standard `IFormFile` binding, not both.
2. Restore sensible **`MemoryBufferThreshold`** (default 64 KB) so large parts spill to disk temp files.
3. Return **`ValidationProblemDetails`** when file missing.
4. If true streaming required: drop `IFormFile` parameter; implement manual multipart section loop from debrief streaming docs.

**Production takeaway:** **Memory buffering** settings and disable-form filters are paired — copying half a streaming tutorial breaks normal `IFormFile` uploads.
