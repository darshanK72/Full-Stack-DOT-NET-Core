# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/01. File & Directory Operations`

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

**Answer:** This is a classic TOCTOU (time-of-check to time-of-use) race: `File.Exists` and `File.Create` are not atomic, so two threads can both pass the check and one `File.Create` wins while the other throws, or one thread creates the file after another passed `Exists` and the second call skips writing entirely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Check-then-act gap between `Exists` and `Create` | Duplicate create attempts → `IOException`; skipped writes when file appears between check and branch |
| Concurrency | No synchronization or exclusive-create semantics | Intermittent failures under load — passes in single-threaded dev |
| Design | `Exists` + `Create` mimics "create if missing" without atomicity | Wrong abstraction for idempotent report generation |

**Fix (priority order):**

1. Use an exclusive create that fails fast if the file already exists — `new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)` — and catch `IOException` to treat "already exists" as expected, or return a conflict result.
2. If multiple writers must coordinate, add an app-level lock keyed by path, or use a database/object-store claim — filesystem races do not scale across pods without external coordination.
3. For idempotent content, prefer write-to-temp-then-atomic-rename (see Q2/Q5) instead of "create only if missing."
4. Remove the silent skip path — if the file exists but is empty or stale, `Exists` returning true hides a partial write from a crashed peer.

```csharp
try
{
    using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    var bytes = Encoding.UTF8.GetBytes(header);
    stream.Write(bytes, 0, bytes.Length);
}
catch (IOException) when (File.Exists(path))
{
    // Another writer won the race — handle idempotently or surface conflict
}
```

**Production takeaway:** Karat uses `File.Exists` + `File.Create` to test whether you know TOCTOU — the fix is atomic open semantics (`CreateNew`) or external locking, not a tighter `if`. See **Program.cs** Section 8 — guard before read is not the same as atomic create.

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

**Answer:** The temp-then-move pattern is right in spirit, but this version leaks temp files on failure, may write temps to an unwritable or ephemeral location in containers, and still has TOCTOU races on the final path — plus `File.Move` is not atomic across volumes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource cleanup | No `try/finally` or `try/catch` to delete `tempPath` on failure | Orphaned `.bin` files fill `%TEMP%` / container overlay — see Q7 |
| Deployment | `Path.GetTempPath()` + `finalPath` on read-only app dir | `CopyToAsync` or `Move` throws in Kubernetes/App Service when dest is not writable |
| Concurrency | `Exists` → `Delete` → `Move` on shared `finalPath` | Two pods interleave deletes/moves — corrupt or missing final file |
| Cross-volume | `File.Move` between temp dir and data volume | Becomes copy+delete — not atomic; crash mid-flight leaves duplicates or partial files |

**Fix (priority order):**

1. Wrap temp lifecycle in `try/finally` (or a `TempFile`/`TempWorkspace` disposable) that deletes the temp path on any exception.
2. Stage temp files in the **same directory** as `finalPath` (e.g. `finalPath + ".tmp"`) so `File.Move` is a same-volume rename — atomic on POSIX and NTFS for same directory.
3. Write to `finalPath.tmp`, flush/fsync if durability matters, then `File.Move(tmp, finalPath, overwrite: true)` (.NET 5+) — avoid separate delete step.
4. In containers, mount a writable volume for uploads (`/app/data` or blob storage); never assume `AppContext.BaseDirectory` or root FS is writable.
5. For multi-instance writes to one key, use object storage (S3/Azure Blob) with etag preconditions or a DB row — not shared filesystem without locking.

```csharp
string dir = Path.GetDirectoryName(finalPath)!;
string tempPath = Path.Combine(dir, $".{Guid.NewGuid():N}.tmp");
try
{
    await using (var temp = File.Create(tempPath))
        await upload.CopyToAsync(temp, ct);

    File.Move(tempPath, finalPath, overwrite: true);
    tempPath = null; // success — do not delete in finally
}
finally
{
    if (tempPath is not null && File.Exists(tempPath))
        File.Delete(tempPath);
}
```

**Production takeaway:** Temp-file staging is a production pattern only when cleanup, same-directory rename, and writable volume paths are handled — Karat stacks failure cleanup with container filesystem constraints.

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

**Answer:** Manual file-by-file deletion before a non-recursive `Directory.Delete` is slower, still fails on nested subdirectories, and fights read-only/hidden attributes — while leaving the tree inconsistent if any step throws mid-loop. The API already supports recursive delete in one call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API misuse | `Directory.Delete(..., recursive: false)` after only deleting **files** | Subfolders remain → `IOException: directory not empty` |
| Scalability | `GetFiles(..., AllDirectories)` loads entire tree into memory | Large workspaces → memory pressure; long lock while iterating |
| Reliability | Partial loop then exception | Some files deleted, folder half-purged — harder to retry idempotently |
| Permissions | `SetAttributes(Normal)` on every file | May still fail on locked files (open handles) or ACL/UAC denied paths |

**Fix (priority order):**

1. Use `Directory.Delete(workspaceRoot, recursive: true)` — one call removes files and nested folders (see **Program.cs** Section 9).
2. Before delete, ensure no open `FileStream`/`StreamReader` handles — dispose all streams first (Section 7 — sharing violation on Windows).
3. For large trees, prefer `DirectoryInfo.EnumerateFiles` with lazy enumeration if you must pre-process, but still finish with recursive delete — do not hand-roll tree walking unless you need selective retention.
4. On permission errors, fix ACLs or run under a service account with rights to the data directory — attribute clearing is not a substitute for proper permissions in prod.
5. Wrap in retry for transient sharing violations if antivirus/indexer holds brief locks.

**Production takeaway:** `Directory.Delete` without `recursive: true` on a non-empty folder is a common tutorial pitfall scaled to production — Karat expects you to know when recursive delete is correct and when open handles block it.

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

**Answer:** Default `FileStream` constructors use `FileShare.Read`, which excludes other writers — concurrent appenders block each other with sharing violations. Even when opens succeed, unsynchronized multi-writer appends interleave bytes at the OS level without line atomicity.

- `File.AppendAllText` opens, appends, and closes per call — high overhead and still races with other writers using incompatible share flags.
- For multiple writers on one file, open with `FileShare.ReadWrite` so other handles can coexist: `new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)`.
- Line atomicity is **not** guaranteed by `FileShare` — two threads can still interleave mid-line; use a `Mutex`/`SemaphoreSlim` named by path, a single dedicated writer thread/channel, or one process owning the log file.
- Better production pattern: append to stdout and let the platform aggregate (Kubernetes logging, Azure Monitor), or write per-process log files and merge — avoid many writers to one file on Windows especially.
- If you must share one file, wrap append in a process-wide lock and flush after each line; consider `StreamWriter` with `AutoFlush = true`.

```csharp
private static readonly SemaphoreSlim _logLock = new(1, 1);

await _logLock.WaitAsync(ct);
try
{
    await File.AppendAllTextAsync(logPath, line + Environment.NewLine, ct);
}
finally
{
    _logLock.Release();
}
```

**Production takeaway:** Sharing violations mean incompatible `FileShare` flags or an undisposed handle — Karat ties **Program.cs** Section 7 (open handles block delete/write) to concurrent append design, not just "use a lock" memorization.

---

#### Q5. (P) An export job stages files under `%TEMP%` on Windows, then calls `File.Move(source, dest)` into a network share. On developer laptops it works; in Azure App Service (Linux) and when crossing drive letters it fails with `IOException` or leaves duplicate files. What is happening at the OS level, and what pattern replaces naive `File.Move`?

**Answer:** `File.Move` is only a cheap atomic rename when source and destination are on the **same file system/volume**. Cross-volume or temp-to-network-share moves degrade to copy-then-delete — slow, non-atomic, and vulnerable to partial failure — and Linux container temp paths often live on a different mount than persisted data volumes.

- Windows: moving from `C:\Users\...\Temp` to `D:\` or `\\server\share` triggers copy+delete, not rename — crash after copy leaves both files or neither in expected state.
- Linux containers: `/tmp` may be tmpfs while `/app/data` is a mounted volume — `File.Move` cannot rename across mounts; errno `EXDEV` → .NET wraps as `IOException`.
- Hidden cost: large files copied twice consume disk and time; antivirus on network paths adds locks.
- Production pattern: stage temp file in the **destination directory** (hidden `.part` suffix), fsync if required, then same-directory `File.Move` to final name — atomic replace on same volume.
- For cross-machine delivery, skip filesystem move entirely — stream to blob storage (S3/Azure Blob) with server-side commit, or use a message queue with object key — not SMB paths from app servers.
- Use `Path.GetPathRoot` or compare `Directory.GetDirectoryRoot` of source and dest in diagnostics; if roots differ, plan copy+verify+delete explicitly with checksum validation.

**Production takeaway:** **Program.cs** Section 3 notes Move is "atomic rename on same volume" — Karat tests whether you apply that caveat when `%TEMP%` and upload folders diverge in cloud deploys.

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

**Answer:** `File.ReadAllText` synchronously reads the entire 200 MB into a single `string` on a thread-pool thread — blocking async I/O throughput and allocating a huge LOH object — so concurrent requests queue behind blocked threads even though the work is I/O-bound.

- The minimal API delegate is synchronous; each request ties up a thread for the full disk read — classic thread-pool starvation under load (same class of problem as `.Result` on async I/O).
- `ReadAllText` doubles memory (file bytes + UTF-16 string) — 200 MB file can mean 400 MB+ per request peak.
- Prefer `return Results.File(path, "text/csv", enableRangeProcessing: true)` — streams from disk with `SendFileAsync` / efficient OS sendfile where available, no full buffering in managed memory.
- If transformation is required: `async Task<IResult>` with `await File.ReadAllTextAsync(path, ct)` or better `File.OpenRead` + `StreamReader` / pipe through `Results.Stream`.
- Add caching (`IMemoryCache` with size limits), CDN, or object storage pre-signed URLs for large static exports — disk read per request does not scale.
- Pass `CancellationToken` from `HttpContext.RequestAborted` so clients disconnecting abort the read.

```csharp
app.MapGet("/reports/{id}", (string id, IReportStore store) =>
{
    string path = store.GetPath(id);
    return File.Exists(path)
        ? Results.File(path, "text/csv", fileDownloadName: $"{id}.csv", enableRangeProcessing: true)
        : Results.NotFound();
});
```

**Production takeaway:** Sync all-at-once file helpers from **Program.cs** Section 3 (`ReadAllText`) are fine for small demo files — in web apps they block the thread pool; Karat expects streaming async APIs for large I/O.

---

#### Q7. (D) A containerized API creates per-request scratch directories under `Path.GetTempPath()` but never deletes them when handlers throw. Disk on the node fills over days; restarting the pod "fixes" it until the next deploy. Compare three cleanup strategies — `try/finally`, `IDisposable` workspace helper, and OS temp with periodic janitor — for production container deployments. What is your default and why?

**Answer:** Orphaned scratch dirs are a deployment lifecycle bug, not a filesystem API quirk — the default should be deterministic per-operation cleanup via an `IDisposable` workspace scoped to the request, with a periodic janitor as a safety net in long-lived pods.

| Strategy | Strengths | Weaknesses |
|---|---|---|
| **`try/finally` inline** | Simple; guaranteed on exit from one method | Easy to forget when logic branches across helpers; duplicated across endpoints |
| **`IDisposable` workspace (`using var ws = new TempWorkspace(...)`)** | Centralizes create/delete; composes with `await using`; testable | Requires discipline to always `using`; nested scopes must not double-delete |
| **OS temp + periodic janitor** | Catches leaks from third-party libs and crash kills; good backstop in K8s | Not sufficient alone — unbounded growth between sweeps fills emptyDir/volume; race if janitor deletes active dirs |

- Default: **`IDisposable`/`IAsyncDisposable` temp workspace** created at request entry, deleted in `Dispose` even on exceptions — matches **Program.cs** Main's clean-slate pattern (`Directory.Delete` before recreate) but scoped per operation.
- Implement janitor as secondary: delete directories under temp older than N hours **only if** naming includes GUID and heartbeat file — never blanket `Delete` on entire `GetTempPath()` while app runs.
- In containers, mount scratch space with size limits (`emptyDir` sizeLimit) so leaks fail fast instead of evicting neighbors; prefer streaming to blob storage over large local scratch.
- Log workspace path on creation at Debug level; metric `temp_workspace_bytes` for observability.
- Avoid relying on pod restart as cleanup policy — violates 12-factor; masks handler bugs.

**Production takeaway:** Karat uses container disk fill to test whether you connect **Program.cs** cleanup demos to request-scoped `using` and deployment volume limits — restart is not a cleanup strategy.
