# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/01. File & Directory Operations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A report export service must create a file only if it does not already exist. Review this helper used under concurrent load:

```csharp
public static void EnsureReportFile(string path, string header)
{
    if (!File.Exists(path))
    {
        using var stream = File.Create(path);
        var bytes = Encoding.UTF8.GetBytes(header);
        stream.Write(bytes, 0, bytes.Length);
    }
}
```

Two requests for the same path occasionally throw `IOException: file already exists`, and sometimes one request silently skips writing. What is wrong, and how do you fix it for production?

---

#### Q2. (R) A teammate refactors upload processing to write through a temp file, then move into place. Review the method:

```csharp
public async Task SaveUploadAsync(IFormFile upload, string finalPath, CancellationToken ct)
{
    string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
    await using (var temp = File.Create(tempPath))
    {
        await upload.CopyToAsync(temp, ct);
    }

    if (File.Exists(finalPath))
        File.Delete(finalPath);

    File.Move(tempPath, finalPath);
}
```

What breaks when `CopyToAsync` throws, when the app runs in a Linux container with a read-only root filesystem, and when two pods write the same `finalPath`?

---

#### Q3. (R) A nightly cleanup job removes old workspace folders. Review:

```csharp
public void PurgeWorkspace(string workspaceRoot)
{
    foreach (string file in Directory.GetFiles(workspaceRoot, "*", SearchOption.AllDirectories))
    {
        File.SetAttributes(file, FileAttributes.Normal);
        File.Delete(file);
    }

    Directory.Delete(workspaceRoot, recursive: false);
}
```

Locally it works on small trees; in production it throws `IOException` on non-empty directories or `UnauthorizedAccessException` on hidden/system files. What is wrong with this approach, and what should you use instead?

---

#### Q4. (P) A multi-process log aggregator appends audit lines from several worker threads. One worker uses `File.AppendAllText`; another opens with default sharing:

```csharp
// Worker A
File.AppendAllText(logPath, line + Environment.NewLine);

// Worker B
using var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write);
using var writer = new StreamWriter(fs);
writer.WriteLine(line);
```

Under load you see `IOException: sharing violation` and occasionally interleaved garbage bytes. Explain `FileShare` behavior here and show a production-safe append pattern.

---

#### Q5. (P) An export job stages files under `%TEMP%` on Windows, then calls `File.Move(source, dest)` into a network share. On developer laptops it works; in Azure App Service (Linux) and when crossing drive letters it fails with `IOException` or leaves duplicate files. What is happening at the OS level, and what pattern replaces naive `File.Move`?

---

#### Q6. (M) An ASP.NET Core endpoint reads a 200 MB CSV from disk on every request:

```csharp
app.MapGet("/reports/{id}", (string id, IReportStore store) =>
{
    string path = store.GetPath(id);
    if (!File.Exists(path))
        return Results.NotFound();

    string csv = File.ReadAllText(path);
    return Results.Content(csv, "text/csv");
});
```

Latency spikes under concurrent traffic and thread-pool queue depth grows, even though CPU stays low. What mechanism is blocking, and what file APIs would you use instead?

---

#### Q7. (D) A containerized API creates per-request scratch directories under `Path.GetTempPath()` but never deletes them when handlers throw. Disk on the node fills over days; restarting the pod "fixes" it until the next deploy. Compare three cleanup strategies — `try/finally`, `IDisposable` workspace helper, and OS temp with periodic janitor — for production container deployments. What is your default and why?
