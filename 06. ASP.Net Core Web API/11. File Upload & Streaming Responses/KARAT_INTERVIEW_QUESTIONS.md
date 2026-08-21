# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/11. File Upload & Streaming Responses`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [06. ASP.NET Core MVC/06. Model Binding in MVC](../../06.%20ASP.NET%20Core%20MVC/06.%20Model%20Binding%20in%20MVC/KARAT_INTERVIEW_QUESTIONS.md) (multipart enctype on forms)

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

---

#### Q2. (P) Configure **request body size limits** for a Web API that accepts CSV imports up to 1 GB via `IFormFile` while keeping default 30 MB limits on all other endpoints. What do you set in Kestrel, `FormOptions`, and action attributes?

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

#### Q5. (P) Implement a **streaming file download** that sets correct **`Content-Disposition`** (attachment vs inline), supports range requests for large PDFs, and avoids loading the entire file into memory. What ASP.NET Core types and headers do you use?

---

#### Q6. (D) Design an upload pipeline: receive `IFormFile` → virus scan → store in blob storage → return 201. Where does scanning run (middleware vs action vs background queue), how do you avoid **memory buffering** the whole file before scan, and what HTTP status do you return if scan times out?

---

#### Q7. (P) An API streams millions of log records to a data warehouse client. Implement **`IAsyncEnumerable<T>`** JSON streaming from a minimal API or controller. What formatter/signature do you use, and what breaks if you return `List<T>` instead?

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
