# 07. File Input & Output and Streams — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. File & Directory Operations](#01-file--directory-operations) — Q1–Q27
- [02. StreamReader & StreamWriter](#02-streamreader--streamwriter) — Q28–Q50
- [03. FileStream & Binary Files](#03-filestream--binary-files) — Q51–Q74
- [04. Path & Environment Classes](#04-path--environment-classes) — Q75–Q96
- [05. Working with CSV and Text Files](#05-working-with-csv-and-text-files) — Q97–Q128

---

## 01. File & Directory Operations

---

## Q1. Explain file handling in C# and the role of the `System.IO` namespace.

**Concepts**
- `System.IO` as the entry point for filesystem work
- Static convenience classes (`File`, `Directory`) vs instance classes (`FileInfo`, `DirectoryInfo`)
- `Stream` hierarchy as the underlying abstraction
- `IDisposable` pattern for handle release
- Text vs binary distinction at the encoding layer

**Answer**

The `System.IO` namespace provides every type needed to read, write, and navigate the filesystem. At the top sit two layers: the static classes `File` and `Directory` for one-shot operations, and the instance classes `FileInfo` and `DirectoryInfo` for repeated work on the same path since they amortize the security check at construction. Beneath those, every I/O operation ultimately flows through the `Stream` hierarchy — `FileStream` for raw bytes, `StreamReader`/`StreamWriter` for encoded text, `BinaryReader`/`BinaryWriter` for typed primitives — which means all file access shares the same async, buffering, and disposal model. Disposal is non-negotiable: on Windows an undisposed `FileStream` holds an OS file lock until the finalizer runs, so every stream type should live inside a `using` statement.

---

## Q2. What is the difference between the static `File`/`Directory` classes and the instance `FileInfo`/`DirectoryInfo` classes?

**Concepts**
- Per-call security demand vs once-at-construction demand
- Metadata caching on instance classes
- `FileInfo.Refresh()` to re-read stale cached values
- Appropriate use: one-shot vs repeated access

**Answer**

`File` and `Directory` are purely static — each call opens its own handle, performs a fresh security check, and closes again, which is efficient for a single operation. `FileInfo` and `DirectoryInfo` perform the security demand once at construction and cache metadata properties like `Length`, `LastWriteTime`, and `Exists`, so reading several properties in a loop is faster because the kernel is not called again until `Refresh()` is invoked. When code only touches a path once, the static methods are simpler and equally fast; when the same path is inspected multiple times (checking size then checking attributes, for example), `FileInfo` avoids redundant round-trips to the OS.

---

## Q3. When would you prefer `FileInfo` over repeated `File.*` static calls on the same path?

**Concepts**
- Security check amortization at construction
- Cached property reads until `Refresh()`
- Fluent multi-property access on one object
- Object lifetime matching the operation scope

**Answer**

I prefer `FileInfo` whenever a single method needs more than one metadata value for the same path — for example, checking `Exists`, reading `Length`, and reading `LastWriteTimeUtc` in the same block. The security demand fires once at `new FileInfo(path)` rather than three times across three static calls, and the properties share the same kernel snapshot, so results are consistent. For one-off operations like `File.Delete` or `File.ReadAllText` called once, the static methods are cleaner since there is no benefit in maintaining the object.

---

## Q4. How do `Directory.GetFiles`, `Directory.GetDirectories`, and their `Enumerate*` counterparts differ in memory behavior?

**Concepts**
- Eager materialization into `string[]` vs lazy `IEnumerable<string>`
- Memory allocation proportional to directory size for `Get*` methods
- Streaming enumeration stopping early with LINQ
- Kernel handle held open during enumeration

**Answer**

`GetFiles` and `GetDirectories` scan the entire directory and return a `string[]` — the full result set is in managed memory before the caller sees the first entry, which allocates a large array on big directories. `EnumerateFiles` and `EnumerateDirectories` return an `IEnumerable<string>` backed by a lazy iterator, so items stream one at a time and the allocation stays bounded regardless of tree size. Because both implement `IEnumerable`, LINQ works on either, but `Enumerate*` pairs better with `Where`/`Take` since iteration stops as soon as enough items are found rather than loading the whole set first.

---

## Q5. What does `Directory.CreateDirectory` do when intermediate folders already exist?

**Concepts**
- Idempotent directory creation
- No exception on pre-existing path
- All missing intermediate segments created in one call
- TOCTOU avoidance vs `Directory.Exists` check-then-create

**Answer**

`Directory.CreateDirectory` creates the full path including any missing intermediate segments and does not throw if any or all of them already exist, making it safe to call unconditionally before writing a file. This idempotency eliminates the `if (!Directory.Exists(path)) Directory.CreateDirectory(path)` pattern, which introduces a TOCTOU race where the directory could be created between the check and the create. The only exceptions it throws are for invalid characters, paths exceeding the OS length limit, or a path that resolves to an existing file rather than a directory.

---

## Q6. What is the difference between `File.Copy` with `overwrite: false` vs `overwrite: true`, and what exception indicates a conflict?

**Concepts**
- `overwrite: false` as the safe default — throws on collision
- `overwrite: true` replaces destination atomically on NTFS
- `IOException` as the conflict exception type
- File content vs metadata copying behavior

**Answer**

With `overwrite: false` (the default), `File.Copy` throws `System.IO.IOException` if the destination already exists, preventing silent data loss. With `overwrite: true`, the destination is replaced — on Windows NTFS this uses `CopyFileEx` internally, which handles the replacement in one kernel operation, though it is not fully atomic under concurrent access on all platforms. File content is always copied but metadata such as creation time may differ from the source depending on the OS, so callers that need timestamp preservation should set them explicitly after the copy.

---

## Q7. How does `File.Move` differ from copy-then-delete, and what happens to metadata and hard links?

**Concepts**
- Atomic rename on same volume — no data movement
- Cross-volume fallback to copy-then-delete
- Metadata and hard-link preservation on rename
- `overwrite: true` overload available since .NET 5

**Answer**

When source and destination share the same volume, `File.Move` is a single atomic rename system call — the directory entry is updated with no bytes moved on disk, so it completes in constant time and all metadata (creation time, attributes, hard links) is fully preserved. When paths span different volumes or network shares, .NET falls back to copy-then-delete, which is not atomic: a crash between the two steps can leave both a partial copy and the original, and hard links pointing to the source inode are not reproduced. Since .NET 5, passing `overwrite: true` avoids an intermediate `File.Delete`, which reduces the race window, but cross-volume moves remain non-atomic regardless.

---

## Q8. What is `File.Replace`, and when is it preferable to manual backup-and-overwrite?

**Concepts**
- Atomic destination replacement with optional backup
- Preserves destination ACLs and security descriptors on the inode
- Single-volume requirement
- Contrast with delete-then-copy two-step

**Answer**

`File.Replace(source, destination, backupPath)` atomically replaces the destination with the source in a single kernel call on Windows NTFS, optionally moving the original destination to a backup path at the same time. Because the underlying inode of the destination survives, ACLs, security descriptors, and alternate data streams are preserved — which copy-then-delete would lose. This makes it the right choice for updating config files or reports that other processes hold handles to, since the replacement is never absent from the reader's view. All three paths must reside on the same volume; cross-volume use throws `IOException`.

---

## Q9. How do you safely delete a directory tree using `Directory.Delete(path, recursive: true)`?

**Concepts**
- `recursive: true` flag to remove files and subdirectories
- `IOException` on non-empty directory without the flag
- Open handles blocking file deletion on Windows
- Read-only attribute clearing before delete

**Answer**

`Directory.Delete(path, recursive: true)` removes the directory and all its contents in one call; without the flag it throws `IOException: directory not empty` if any file or subdirectory exists, which catches many developers off guard. The common mistake — deleting files in a loop then calling non-recursive delete — still fails when subdirectories are present and is slower than the built-in recursive path. Before calling, every `FileStream`, `StreamReader`, and `StreamWriter` opened against paths inside the tree must be disposed because on Windows an open handle prevents that file's deletion, which then blocks the containing directory. Files with read-only attributes need `File.SetAttributes(path, FileAttributes.Normal)` first, or the delete throws `UnauthorizedAccessException` on those entries.

---

## Q10. What file metadata can you read via `File` static methods vs `FileInfo` instance properties?

**Concepts**
- Static `File.GetCreationTime`, `GetLastWriteTime`, `GetAttributes` methods
- `FileInfo` properties: `Length`, `CreationTime`, `LastWriteTime`, `Attributes`
- Performance difference for multiple reads on the same path
- `FileInfo.Refresh()` to invalidate cached values

**Answer**

The `File` static class exposes `GetCreationTime`, `GetLastWriteTime`, `GetLastAccessTime`, `GetAttributes`, and `GetAccessControl` as standalone calls that each open a handle, read, and close. `FileInfo` exposes the same data as properties that are populated on first access and cached until `Refresh()` is called — meaning multiple property reads for the same file do not each go to the kernel. `File` is simpler when only one value is needed once; `FileInfo` wins whenever multiple metadata values are read for the same path because the kernel call is amortized across all reads within one refresh cycle.

---

## Q11. Explain `File.GetCreationTime`, `GetLastWriteTime`, and `GetLastAccessTime` — and their `Utc` variants.

**Concepts**
- Three distinct filesystem timestamps and their meanings
- Local vs UTC `DateTime` return values
- DST ambiguity in local-time variants
- NTFS internal UTC storage and conversion on read

**Answer**

`GetCreationTime` returns when the directory entry was created, `GetLastWriteTime` when content was last changed, and `GetLastAccessTime` when the file was last opened. Each returns a `DateTime` in local time; the `Utc` variants return UTC values and are preferable for logging and cross-timezone comparisons because local time is ambiguous during DST transitions — two different instants can have the same local representation when clocks fall back. NTFS stores all timestamps internally as UTC at 100-nanosecond precision and Windows converts on read, so the Utc variants avoid an extra conversion and are more precise. `LastAccessTime` is frequently disabled on performance-tuned Windows systems and on Linux filesystems, making it unreliable as a proxy for recent reads.

---

## Q12. How do you set creation, last-write, and last-access timestamps programmatically?

**Concepts**
- `File.SetCreationTime`, `SetLastWriteTime`, `SetLastAccessTime` static methods
- `FileInfo` property setters for the same values
- UTC overloads to avoid DST ambiguity
- Write permissions required on file metadata

**Answer**

Timestamps are set with `File.SetCreationTime(path, datetime)`, `File.SetLastWriteTime`, and `File.SetLastAccessTime`, each accepting a `DateTime` in local time; the `Utc` overloads accept UTC values directly and avoid DST conversion errors. `FileInfo` exposes the same as settable properties — `info.LastWriteTimeUtc = DateTime.UtcNow` — which is cleaner when multiple values are set on the same file object. Setting timestamps requires write permission on the file's metadata, typically held by the file owner or an administrator, so service accounts running under restricted identities may receive `UnauthorizedAccessException` on protected system files.

---

## Q13. What are `FileAttributes` (ReadOnly, Hidden, System, Archive)? How do you read and modify them?

**Concepts**
- `FileAttributes` as a combinable flags enum
- `File.GetAttributes` / `File.SetAttributes`
- Bitwise add and remove for individual flags
- Platform portability — most Windows flags are no-ops on Linux

**Answer**

`FileAttributes` is a `[Flags]` enum whose most common members are `ReadOnly`, `Hidden`, `System`, `Archive`, `Directory`, `Compressed`, and `Encrypted`. Read the current value with `File.GetAttributes(path)`, which returns the combined flags, and write it back with `File.SetAttributes(path, newValue)`, which replaces the entire set. Since it is a flags enum, add individual attributes with bitwise OR (`attrs | FileAttributes.Hidden`) and remove them with bitwise AND-NOT (`attrs & ~FileAttributes.ReadOnly`), always preserving the other flags. On Linux, `ReadOnly` maps to write-permission bits and most Windows-specific flags are silently ignored, so code that manipulates these attributes should be tested on the target platform.

---

## Q14. What is the difference between `File.Exists` and attempting to open a file that may be deleted concurrently?

**Concepts**
- TOCTOU race between check and use
- `File.Exists` as a point-in-time snapshot
- Atomic open semantics via `FileMode` + `try/catch`
- Exception-driven vs check-driven file access

**Answer**

`File.Exists` is a point-in-time snapshot — another process can delete or rename the file in the window between the check returning `true` and the subsequent open, so the check is never a reliable guard. The only race-free pattern is to attempt the open directly and let the runtime throw `FileNotFoundException` if the file is absent, catching that specific exception where absence is a legitimate outcome. Check-then-act (`if (File.Exists) File.Open`) introduces a TOCTOU window that can cause intermittent failures under concurrent load without any indication of what went wrong. `File.Exists` is appropriate for user-facing validation messages where the race is acceptable, but it must never be the sole guard before a security-sensitive or correctness-critical open.

---

## Q15. What exceptions should you expect during file operations (`FileNotFoundException`, `DirectoryNotFoundException`, `IOException`, `UnauthorizedAccessException`)?

**Concepts**
- `FileNotFoundException` — path missing when expected to exist
- `DirectoryNotFoundException` — intermediate directory missing
- `IOException` as the base for disk-full, sharing violation, locked file
- `UnauthorizedAccessException` — permissions denied
- `PathTooLongException` — path exceeds OS limit

**Answer**

`FileNotFoundException` is thrown when `FileMode.Open` is used on a missing path, and `DirectoryNotFoundException` fires when an intermediate directory segment does not exist — both derive from `IOException`, which is the broad base for disk-full errors, sharing violations, and locked-file errors. `UnauthorizedAccessException` signals a permissions problem: writing to a read-only file, accessing a path where the account lacks rights, or attempting to open a directory as a file. `PathTooLongException` (surfaced as `IOException` with a specific message on .NET 5+) fires when the path exceeds the OS character limit. The correct catch structure is to handle the most specific exception first with a precise message and fall back to `IOException` as a general handler, never catching the root `Exception` and swallowing I/O failures silently.

---

## Q16. How does `File.AppendAllText` differ from opening with `FileMode.Append`?

**Concepts**
- One-shot open-write-close vs long-lived handle
- Per-call `FileShare` defaults vs explicit sharing control
- Overhead of repeated open/close cycles
- Thread safety without a held handle

**Answer**

`File.AppendAllText` opens the file, appends the text, and immediately closes — a complete lifecycle in one call, which means no handle lingers between calls and the next opener sees no lock. Opening with `FileMode.Append` on a `FileStream` keeps the handle open and positions writes at EOF on each call, which is more efficient for a tight loop that appends many entries because the OS seek overhead is avoided. The open handle holds a lock governed by the `FileShare` flag specified, so callers must decide explicitly whether other processes can read or write concurrently; `AppendAllText` makes this decision internally and closes before returning. For sporadic one-off appends where no handle should linger, `AppendAllText` is simpler; for a long-running log writer in a single process, a held handle with `FileShare.Read` is more efficient.

---

## Q17. When is `File.ReadAllBytes` / `File.WriteAllBytes` appropriate vs stream-based APIs?

**Concepts**
- Whole-file allocation on the LOH for large files
- Simplicity for small, bounded payloads
- Stream-based chunked reads for large files
- `async` support only available via stream APIs

**Answer**

`File.ReadAllBytes` is appropriate when the file is small (typically under a few megabytes), the code processes the whole buffer anyway, and simplicity matters more than memory efficiency. For large files, it allocates the entire content on the Large Object Heap — a 200 MB file produces a 200 MB `byte[]` that triggers GC pressure and can cause `OutOfMemoryException` under concurrent load. Stream-based APIs read in chunks that stay in the OS page cache rather than materializing the full content in managed memory, and they support `ReadAsync`/`WriteAsync` natively, which prevents blocking thread-pool threads on I/O-bound work. `File.WriteAllBytes` similarly suits small payloads where atomicity via temp-file-then-rename is not needed; for large writes that must not leave a partial file, write to a temp path and `File.Move` into place.

---

## Q18. Explain `File.Create`, `File.Open`, `File.OpenRead`, and `File.OpenWrite` — what modes and access do they imply?

**Concepts**
- `File.Create` — `FileMode.Create` + `FileAccess.ReadWrite`, truncates existing
- `File.OpenRead` — `FileMode.Open` + `FileAccess.Read`, throws if missing
- `File.OpenWrite` — `FileMode.OpenOrCreate` + `FileAccess.Write`, leaves tail intact
- `File.Open` — explicit `FileMode` and `FileAccess` for full control

**Answer**

`File.Create(path)` opens with `FileMode.Create` and `FileAccess.ReadWrite`, creating the file if absent or truncating it to zero bytes if present, which surprises callers who expected to append. `File.OpenRead(path)` is a shortcut for `FileMode.Open` with `FileAccess.Read` — it throws `FileNotFoundException` if the path does not exist and uses `FileShare.Read` by default. `File.OpenWrite(path)` opens with `FileMode.OpenOrCreate` and `FileAccess.Write`, creating the file if missing and positioning at byte zero without truncating, so existing content beyond what is written remains on disk — a subtle trap when replacing file content. `File.Open(path, mode, access, share)` is the general-purpose overload and should be used whenever the defaults of the convenience methods do not match the intent.

---

## Q19. How do you handle TOCTOU (time-of-check-time-of-use) races when checking existence before read/write?

**Concepts**
- TOCTOU race class of bug
- `FileMode.CreateNew` for atomic exclusive create
- `FileMode.Open` + `FileNotFoundException` for atomic "open only if exists"
- Kernel-level atomicity of `FileMode` semantics
- `File.Exists` as user-facing validation only

**Answer**

The fix is to skip the existence check entirely and encode the intent in the `FileMode` parameter, which the kernel resolves atomically. For "create only if not exists," use `FileMode.CreateNew` — it throws `IOException` if the path already exists, which I catch and handle as the expected collision case. For "open only if exists," use `FileMode.Open` and catch `FileNotFoundException` when absence is a valid outcome. For "create or open," `FileMode.OpenOrCreate` handles both cases in one call. These `FileMode` values are resolved by the OS as single atomic operations, closing the window that exists between a `File.Exists` check and the subsequent open. `File.Exists` is appropriate only for pre-flight user messages where a subsequent race is acceptable.

---

## Q20. What is the difference between deleting a file and clearing its contents while keeping the path?

**Concepts**
- `File.Delete` removes the directory entry
- `FileMode.Truncate` or `SetLength(0)` preserves path and metadata
- Open-handle deferred delete on Windows
- ACL and inode identity preserved on truncate

**Answer**

`File.Delete` removes the directory entry — the path ceases to exist, and on Windows any process that already has the file open causes the delete to be deferred until the last handle closes, which can confuse writers that expect the path gone immediately. Clearing contents while keeping the path uses `FileStream` opened with `FileMode.Truncate` (sets length to zero) or `stream.SetLength(0)` on an existing handle. Truncation preserves the file's ACLs, inode number on POSIX systems, creation timestamp, and any alternate data streams, which matters when downstream watchers or audit logs track the path identity rather than the content. Use truncation rather than delete-and-recreate whenever the file's identity must remain stable across the operation.

---

## Q21. (Scenario R) A report export service uses `File.Exists` + `File.Create` to guard concurrent writes — two threads occasionally get `IOException` or silently skip writing. What is wrong and how do you fix it?

**Concepts**
- TOCTOU race between `File.Exists` and `File.Create`
- `FileMode.CreateNew` for atomic exclusive create
- Catching `IOException` as the "already exists" signal
- App-level lock or object-store claim for cross-pod coordination

**Answer**

The race is that `File.Exists` and `File.Create` are separate kernel calls with no atomicity guarantee between them, so two threads can both pass the check and one `File.Create` wins while the other throws, or a third process creates the file between the check and the create and the second caller skips writing silently. The fix is to remove the existence check entirely and open with `FileMode.CreateNew`, which atomically fails if the path already exists, then catch `IOException` and verify `File.Exists` in the filter to treat that outcome as an expected collision rather than an infrastructure error. For multi-instance deployments where multiple pods race on the same path, filesystem atomicity is not sufficient — use an external lock keyed by path (a database row, an object-store conditional put) because `FileMode.CreateNew` only works reliably within a single machine's filesystem.

```csharp
try
{
    using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    stream.Write(Encoding.UTF8.GetBytes(header));
}
catch (IOException) when (File.Exists(path))
{
    // another writer won the race — treat as idempotent success or surface conflict
}
```

---

## Q22. (Scenario R) A teammate writes an upload through a temp file then moves into place, but the temp leaks on exception, the container root filesystem is read-only, and two pods race on the same final path. What is wrong?

**Concepts**
- Temp file leak when `CopyToAsync` throws without `try/finally`
- Cross-volume `File.Move` degrades to non-atomic copy-then-delete
- Container read-only root filesystem vs writable mounted volume
- Pod-level race on shared final path requires object-store semantics

**Answer**

The three problems compound: without a `try/finally` wrapping the temp file path, any exception from `CopyToAsync` or `File.Move` leaves an orphaned `.bin` file in `%TEMP%` that fills the container overlay over time. Staging the temp in `Path.GetTempPath()` while moving to a data volume crosses filesystem boundaries on Linux containers, so `File.Move` degrades to copy-then-delete — not atomic — meaning a crash between the two steps leaves both a partial copy and the original. The fix stages the temp in the same directory as the final path (same volume = atomic rename), wraps the entire operation in `try/finally` to delete the temp on any exception, and uses `File.Move(tmp, finalPath, overwrite: true)` (.NET 5+) to avoid the separate delete step. For multi-pod writes to the same key, object storage with etag preconditions is the correct primitive since there is no cross-pod filesystem lock.

```csharp
string tempPath = Path.Combine(Path.GetDirectoryName(finalPath)!, $".{Guid.NewGuid():N}.tmp");
try
{
    await using (var temp = File.Create(tempPath))
        await upload.CopyToAsync(temp, ct);
    File.Move(tempPath, finalPath, overwrite: true);
    tempPath = null;
}
finally
{
    if (tempPath is not null && File.Exists(tempPath))
        File.Delete(tempPath);
}
```

---

## Q23. (Scenario R) A nightly cleanup job deletes files one-by-one then calls `Directory.Delete(root, recursive: false)`, but throws in production on non-empty directories or hidden files. What is wrong?

**Concepts**
- Non-recursive `Directory.Delete` fails on any remaining content
- `Directory.GetFiles` loads all paths eagerly into memory
- Partial-loop failure leaves tree inconsistent
- `Directory.Delete(path, recursive: true)` as the correct API

**Answer**

The fundamental mistake is calling `Directory.Delete` without `recursive: true` after only deleting files — since the loop skips subdirectories, they remain and the delete throws `IOException: directory not empty` every time a nested folder exists. Even if the loop were extended to cover directories, hand-rolling tree deletion is slower and less reliable than the built-in recursive path, and `Directory.GetFiles(..., SearchOption.AllDirectories)` materializes the entire tree into memory before the first deletion. The correct approach is `Directory.Delete(workspaceRoot, recursive: true)`, which removes files and nested folders in one call. Before calling, dispose all open handles to files inside the tree since Windows defers deletion of files with open handles, which then blocks the containing directory's deletion.

---

## Q24. (Scenario P) A multi-process log aggregator has workers using `File.AppendAllText` and `new FileStream(..., FileMode.Append, FileAccess.Write)` (default share), producing sharing violations and interleaved bytes. What is happening?

**Concepts**
- Default `FileShare.None` on `FileStream` — exclusive lock
- `FileShare.ReadWrite` required for concurrent appenders
- Line-level atomicity not guaranteed by `FileShare`
- Single-writer channel or per-process log files as production alternatives

**Answer**

The default `FileShare` on a `FileStream` constructor that omits the parameter is `FileShare.None`, which grants an exclusive lock and blocks any other process from opening the file — causing the `IOException: sharing violation`. Even when both sides use `FileShare.ReadWrite` so they can coexist, the OS does not guarantee that individual `WriteLine` calls are atomic: two writers can interleave bytes mid-line. The minimal fix is to open with `FileShare.ReadWrite` on both sides, but line integrity then requires an app-level lock — a `SemaphoreSlim(1,1)` wrapping each append call. The production pattern that avoids all of this is a single dedicated writer thread or `Channel<string>` that serializes entries, or writing per-process log files and aggregating them externally, which is how structured logging sinks like Serilog handle high-throughput scenarios.

---

## Q25. (Scenario P) An export job stages under `%TEMP%`, then calls `File.Move` to a network share. On Linux containers and across drive letters it fails with `IOException` or leaves duplicate files. What is happening?

**Concepts**
- `File.Move` atomic only on same volume — degrades to copy-then-delete cross-volume
- Linux tmpfs vs mounted data volume as separate filesystems
- `EXDEV` errno wrapped as `IOException` on cross-mount rename
- Stage-in-destination-directory pattern for guaranteed same-volume move

**Answer**

`File.Move` is a cheap atomic rename when source and destination share the same filesystem, but it degrades to copy-then-delete when they differ — which means a crash after the copy but before the delete leaves both files, and a crash during the copy leaves neither in the expected state. On Linux containers, `/tmp` is typically a separate `tmpfs` mount from the persistent data volume at `/app/data`, so the kernel returns `EXDEV` (cross-device link), which .NET surfaces as `IOException`. The production fix stages the temp file in the same directory as the final destination (a hidden `.part` file alongside the target), then calls `File.Move(tmp, final, overwrite: true)` — guaranteed same-volume and atomic on POSIX and NTFS. For cross-machine delivery to a network share, bypass filesystem moves entirely and stream to object storage (Azure Blob, S3) with a server-side commit.

---

## Q26. (Scenario M) An ASP.NET Core endpoint reads a 200 MB CSV with `File.ReadAllText` on every request. Thread-pool queue depth grows and latency spikes even though CPU is low. What is the mechanism and fix?

**Concepts**
- Synchronous `File.ReadAllText` blocking a thread-pool thread
- Thread-pool starvation under concurrent I/O-bound requests
- LOH allocation from large `string` materialization
- `Results.File` for zero-copy streaming via OS sendfile

**Answer**

`File.ReadAllText` is synchronous — each call occupies a thread-pool thread for the entire duration of the disk read, so under concurrent traffic the pool exhausts its threads waiting on I/O while CPU idles, which is the same class of starvation as calling `.Result` on an async operation. Reading 200 MB into a `string` also allocates a single Large Object Heap object per request, adding GC pressure that compounds the throughput problem. The minimal fix is `Results.File(path, "text/csv", enableRangeProcessing: true)`, which streams directly from disk using `SendFileAsync` and avoids materializing the content in managed memory at all. If transformation is required before sending, use an `async` delegate with `File.OpenRead` piped through `Results.Stream` so the thread is freed during the I/O wait.

---

## Q27. (Scenario D) A containerized API creates per-request scratch directories under `Path.GetTempPath()` but never deletes them when handlers throw. Compare `try/finally`, `IDisposable` workspace, and periodic janitor. What is the default choice?

**Concepts**
- Per-request deterministic cleanup vs background sweep
- `IAsyncDisposable` temp workspace scoped to request lifetime
- Container `emptyDir` size limit as a backstop
- Periodic janitor as secondary defense, not primary

**Answer**

The default should be an `IDisposable` (or `IAsyncDisposable`) temp workspace created at request entry whose `Dispose` method calls `Directory.Delete(path, recursive: true)`, because it cleanly handles any exit path including unhandled exceptions through the `using` statement without duplicating cleanup logic across every endpoint. Inline `try/finally` works but scatters the create-and-delete responsibility into each handler, making it easy to forget in a new code path. A periodic janitor that deletes directories older than N hours is valuable as a secondary defense — it catches leaks from third-party libraries and out-of-process kills — but must never be the primary mechanism since it allows unbounded growth between sweeps and can race against active scratch directories if naming is not unique. In Kubernetes, mount scratch space with `emptyDir: {sizeLimit: "500Mi"}` so container-level disk exhaustion fails fast rather than evicting neighbors.

---

## 02. StreamReader & StreamWriter

---

## Q28. Explain the `Stream` base class hierarchy and where `StreamReader`/`StreamWriter` fit.

**Concepts**
- `Stream` as the abstract byte-sequence base
- `FileStream`, `MemoryStream`, `NetworkStream` as concrete byte streams
- `TextReader`/`TextWriter` as the abstract text layer
- `StreamReader`/`StreamWriter` as encoding bridge between bytes and text

**Answer**

`Stream` is the abstract base that defines the byte-level contract: `Read`, `Write`, `Seek`, `Flush`, `Position`, `Length`, and `CanRead`/`CanWrite`/`CanSeek`. Concrete implementations — `FileStream`, `MemoryStream`, `NetworkStream`, `CryptoStream` — all inherit it and deal in raw bytes. `TextReader` and `TextWriter` sit above `Stream` as abstractions for character sequences; `StreamReader` and `StreamWriter` implement those interfaces by wrapping any `Stream` and applying an `Encoding` to convert between bytes and `char`/`string`. This layered design means I can swap the underlying stream — point a `StreamReader` at a `MemoryStream` for unit tests or a `NetworkStream` for socket I/O — without changing any parsing code, since the text API is the same regardless of where the bytes come from.

---

## Q29. What is the difference between `File.ReadAllText`, `File.ReadAllLines`, and `File.ReadLines`?

**Concepts**
- `ReadAllText` — whole file as one `string`, eager
- `ReadAllLines` — whole file as `string[]`, eager
- `ReadLines` — lazy `IEnumerable<string>`, file handle held open
- Memory implications for large files

**Answer**

`File.ReadAllText` reads the entire file and returns it as a single `string`, which allocates the whole content on the heap before the caller sees any characters. `File.ReadAllLines` does the same but splits on line endings and returns a `string[]`, allocating both the individual line strings and the array. `File.ReadLines` is the lazy alternative — it returns an `IEnumerable<string>` that reads one line at a time, keeping memory proportional to one line rather than the entire file, but it holds the underlying file handle open for the lifetime of the enumeration. For large files, `ReadLines` is the right choice because it avoids the LOH allocation; the trade-off is that the handle stays open until the `foreach` completes or the enumerator is disposed, so the file remains locked during iteration.

---

## Q30. Why can `File.ReadLines` hold a file lock until enumeration completes?

**Concepts**
- Lazy iterator keeping `FileStream` open until disposed
- `IEnumerator.Dispose` releasing the underlying handle
- Breaking from `foreach` early disposes the enumerator
- `using` on the enumerable not equivalent to `using` on the enumerator

**Answer**

`File.ReadLines` opens a `FileStream` and a `StreamReader` on the first call to `MoveNext()` and keeps them open to serve subsequent lines lazily. Because the underlying handle must remain open to read each line on demand, the file lock is held for the entire duration of enumeration. When a `foreach` loop runs to completion or breaks early, the C# compiler-generated code calls `IEnumerator.Dispose()` on the iterator, which closes the stream and releases the lock. If the caller stores the `IEnumerable<string>` in a variable and enumerates it partially without disposing the enumerator — for example using LINQ's `First()` without a `foreach` — the stream stays open until the garbage collector finalizes it, which on Windows keeps the file locked unexpectedly.

---

## Q31. How do `StreamReader.ReadLine`, `ReadToEnd`, and `ReadBlock` differ for large files?

**Concepts**
- `ReadLine` — one line at a time, bounded memory
- `ReadToEnd` — entire remaining content as one `string`, LOH risk
- `ReadBlock` — fixed-size char buffer, predictable allocation
- Suitable use case for each method

**Answer**

`ReadLine` reads until the next line terminator and returns one `string` at a time, keeping heap allocation proportional to the longest line regardless of file size — this is the right pattern for line-oriented parsing of large files. `ReadToEnd` reads everything from the current position to EOF into a single `string`, which is fine for small files but allocates a potentially enormous LOH object for large ones and blocks the thread for the full read duration. `ReadBlock` fills a caller-supplied `char[]` buffer up to a specified count and returns how many characters were actually read, making it suited for fixed-chunk processing or piping into another writer with minimal allocation. For anything over a few megabytes I use `ReadLine` in a loop or `ReadLineAsync` in an async context rather than `ReadToEnd`.

---

## Q32. What is the default encoding for `StreamReader` and `StreamWriter`, and why can that cause mojibake?

**Concepts**
- `StreamReader` default: UTF-8 with BOM detection enabled
- `StreamWriter` default: UTF-8 without BOM
- `Encoding.Default` resolves to system code page, not UTF-8 on Windows
- Mismatch between writer and reader encoding producing mojibake

**Answer**

The no-argument `StreamReader` constructor defaults to UTF-8 with `detectEncodingFromByteOrderMarks: true`, so it auto-detects the encoding from a leading BOM if present. The no-argument `StreamWriter` defaults to UTF-8 without a BOM. When code uses `Encoding.Default` on Windows it gets the system ANSI code page (often Windows-1252), which differs from UTF-8, so bytes written with the ANSI encoder are read back as garbage by a UTF-8 reader — this is mojibake. The fix is to pass the same explicit encoding to both writer and reader rather than relying on defaults; I use `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` for files exchanged with other systems where a BOM would be unexpected.

---

## Q33. How do you specify `Encoding.UTF8`, UTF-8 with BOM, and legacy encodings (`Encoding.GetEncoding`)?

**Concepts**
- `Encoding.UTF8` — static property, UTF-8 with BOM emitter
- `new UTF8Encoding(false)` — UTF-8 without BOM
- `Encoding.GetEncoding(1252)` for legacy Windows code pages
- BOM detection vs explicit encoding in readers

**Answer**

`Encoding.UTF8` is a static property that returns a UTF-8 encoder configured to emit a BOM on write, which is what Excel on Windows expects but what most web APIs and Unix tools do not. To write UTF-8 without a BOM I construct `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)`. Legacy encodings are accessed via `Encoding.GetEncoding(codePage)` — for example `Encoding.GetEncoding(1252)` for Windows-1252 or `Encoding.GetEncoding("iso-8859-1")` for Latin-1 — but these require the `System.Text.Encoding.CodePages` NuGet package on .NET Core since non-Unicode encodings are not registered by default. On the read side, I specify the same encoding explicitly rather than relying on BOM detection, since a missing BOM causes the reader to fall back to the constructor's default and silently produce wrong characters.

---

## Q34. What does `StreamReader.DetectEncodingFromByteOrderMarks` control?

**Concepts**
- BOM detection at stream start
- Overrides the constructor-specified encoding when a BOM is found
- Does not detect encoding without a BOM
- UTF-8, UTF-16 LE/BE, and UTF-32 BOM signatures

**Answer**

`DetectEncodingFromByteOrderMarks` (defaulting to `true`) tells `StreamReader` to inspect the first few bytes of the stream for a BOM — the byte sequences that signal UTF-8 (`EF BB BF`), UTF-16 LE (`FF FE`), UTF-16 BE (`FE FF`), or UTF-32. If a matching BOM is found, the reader switches to that encoding regardless of what was passed to the constructor. When no BOM is present, the constructor-specified encoding is used without any detection, since BOM detection cannot infer encoding from arbitrary byte patterns. I set this parameter to `false` when consuming files from partners whose encoding I have verified and documented, because a spurious BOM from a misconfigured tool would otherwise silently change the decoding and corrupt the first field.

---

## Q35. Explain async read/write methods on `StreamReader`/`StreamWriter` (`ReadLineAsync`, `WriteLineAsync`, `ReadToEndAsync`).

**Concepts**
- Async I/O freeing thread-pool threads during disk wait
- `CancellationToken` support on .NET 7+ overloads
- `await using` for async disposal of the writer
- Composition with `async Task` methods in ASP.NET Core

**Answer**

`ReadLineAsync`, `WriteLineAsync`, and `ReadToEndAsync` are the async counterparts of their synchronous versions — they issue the underlying I/O and `await` completion rather than blocking the calling thread, which frees the thread-pool thread to serve other requests during the disk wait. On .NET 7+ all three accept a `CancellationToken` so the operation can be aborted cleanly on request cancellation or shutdown. `StreamWriter` is `IAsyncDisposable` since .NET 5, so I use `await using` to flush and dispose asynchronously without blocking on the final flush. In hosted services or minimal API handlers, mixing synchronous `ReadLine` with an otherwise async pipeline ties up a thread-pool thread per in-flight request, which is the same throughput problem as `Task.Result` in a controller.

---

## Q36. What is `StreamWriter.AutoFlush`, and when should you call `Flush()` explicitly?

**Concepts**
- Internal buffer accumulating writes until explicit flush or dispose
- `AutoFlush = true` flushing after every `Write`/`WriteLine` call
- Performance trade-off: batched writes vs immediate disk visibility
- `Flush()` before signaling downstream consumers

**Answer**

`StreamWriter` maintains an internal character buffer and batches writes to reduce the number of system calls; it only writes buffered content to the underlying stream when the buffer is full, when `Flush()` is called, or when the writer is disposed. Setting `AutoFlush = true` makes the writer flush after every `Write` or `WriteLine`, which ensures disk visibility immediately at the cost of more frequent I/O — appropriate for log files and status files that other processes poll. I call `Flush()` explicitly when I need the content visible to a reader before disposing the writer, for example after writing a status token that a downstream process or poller reads on a known schedule. Without either, an un-disposed writer can leave the last buffer of content unwritten if the process terminates abnormally.

---

## Q37. How do you append text to an existing file with `StreamWriter` (constructor overload with `append: true`)?

**Concepts**
- `new StreamWriter(path, append: true)` opening with `FileMode.Append`
- Handle open for the writer's lifetime vs per-call open-close
- `FileShare` defaults when using the path constructor
- Using `StreamWriter` on a pre-opened `FileStream` for full control

**Answer**

Passing `append: true` to the `StreamWriter` path constructor opens the underlying `FileStream` with `FileMode.Append` and `FileAccess.Write`, positioning writes at the current end of file without truncating existing content. The handle stays open for the lifetime of the `StreamWriter` object, so each subsequent `WriteLine` appends efficiently without reopening. The path constructor uses `FileShare.Read` by default, which means other processes can read the file concurrently but cannot write — if concurrent writers are needed, I construct the `FileStream` explicitly with `FileShare.ReadWrite` and pass it to the `StreamWriter` constructor. Wrapping the writer in `using` is essential because `Dispose` flushes the buffer and closes the handle; without it, the last lines may never reach disk and the file lock persists on Windows.

---

## Q38. What happens if you forget to dispose a `StreamWriter` — especially on Windows file locking?

**Concepts**
- Unflushed buffer lost on process exit without dispose
- OS file handle held open until GC finalizer
- Windows file lock blocking concurrent open/delete/rename
- `using` or `await using` as the mandatory pattern

**Answer**

Forgetting to dispose a `StreamWriter` has two consequences: the internal buffer may never be flushed, so the last writes are silently lost even if the process exits normally, and the underlying `FileStream` handle remains open until the garbage collector runs the finalizer. On Windows, an open handle imposes a file lock — other processes cannot delete, rename, or exclusively open the file, which causes `IOException: process cannot access the file` errors that appear intermittent because they depend on when GC happens to run. The fix is always `using var writer = new StreamWriter(...)` or `await using` for async writers so the compiler generates a `try/finally` that calls `Dispose` on every exit path including exceptions.

---

## Q39. Can you use `StreamReader`/`StreamWriter` with non-file streams (memory, network)? Give examples.

**Concepts**
- Constructor accepting any `Stream`, not just `FileStream`
- `MemoryStream` for in-memory text processing and unit testing
- `NetworkStream` / `SslStream` for socket-based text protocols
- `leaveOpen: true` to prevent wrapper from closing the base stream

**Answer**

`StreamReader` and `StreamWriter` accept any `Stream` — the encoding bridge they provide is independent of where the bytes come from or go to. I use `new StreamReader(new MemoryStream(bytes))` in unit tests to feed a pre-built byte payload through the same parsing code that reads files in production, avoiding actual disk I/O. For HTTP or socket protocols, wrapping a `NetworkStream` in a `StreamReader` lets me call `ReadLine()` on a text-based protocol like SMTP or IRC. When the wrapper must not close the underlying stream on dispose (for example a `MemoryStream` whose bytes are read after the writer disposes), I pass `leaveOpen: true` to the constructor so the base stream remains open and its `Position` can be reset for reading.

---

## Q40. What is the difference between `using` blocks and C# 8 `using` declarations for stream cleanup?

**Concepts**
- Classic `using` block — dispose at closing brace
- C# 8 `using` declaration — dispose at end of enclosing scope
- Nested resource lifetimes and ordering
- Readability vs explicit scope control

**Answer**

The classic `using (var r = new StreamReader(...)) { }` disposes at the explicit closing brace, giving precise control over when the handle is released. The C# 8 `using var r = new StreamReader(...)` declaration disposes at the end of the enclosing scope (the method or block it lives in), which reduces nesting for simple single-resource methods. The difference matters when multiple streams share a scope: a `using var fs = new FileStream(...)` followed by `using var r = new StreamReader(fs)` disposes `r` first at scope exit (LIFO order), which flushes and closes it before `fs` is disposed — matching the safe teardown order. Classic `using` blocks are preferable whenever the dispose boundary is not at the method end, for example to release a file handle early before continuing with expensive CPU work.

---

## Q41. How do you read a file line-by-line without loading it entirely into memory?

**Concepts**
- `StreamReader.ReadLine()` returning one line per call
- Loop terminating on `null` return at EOF
- `ReadLineAsync` for async pipelines
- Memory bounded by longest single line

**Answer**

I open a `StreamReader` and call `ReadLine()` in a `while` loop, checking for `null` to detect EOF, which keeps only one line string in memory at a time regardless of file size. For async pipelines I use `await reader.ReadLineAsync(cancellationToken)` (the `CancellationToken` overload available since .NET 7) so the thread-pool thread is free during each disk read. This pattern handles files of any size within constant memory proportional to the longest line, making it the standard approach for log parsing, CSV ingestion, and config file loading in production services. `File.ReadLines` is a convenient alternative for simple LINQ queries but holds the file handle open until the enumerator is disposed, which `StreamReader` in a `using` block makes explicit and deterministic.

---

## Q42. What is `TextReader`/`TextWriter`, and why do APIs often accept these abstractions?

**Concepts**
- `TextReader`/`TextWriter` as the abstract character-sequence interface
- `StringReader`/`StringWriter` as in-memory implementations
- `StreamReader`/`StreamWriter` as stream-backed implementations
- Testability: inject `StringReader` instead of file-backed `StreamReader`

**Answer**

`TextReader` defines the character-level reading contract — `Read`, `ReadLine`, `ReadToEnd`, `Peek` — without specifying where the characters come from, and `TextWriter` defines the writing contract similarly. `StreamReader` and `StreamWriter` implement these interfaces backed by a `Stream`; `StringReader` and `StringWriter` implement them backed by a `string` or `StringBuilder`. Accepting `TextReader` in a parser method rather than `StreamReader` means I can pass a `new StringReader("csv,data,here")` in a unit test without touching the filesystem, and the same method reads from a `FileStream` in production — the parsing logic is completely decoupled from the I/O source. APIs like `XmlReader.Create(TextReader)` and `JsonSerializer.Deserialize(TextReader)` use this pattern for the same reason.

---

## Q43. (Scenario R) A nightly audit job throws on bad rows and the log file stays locked until the worker restarts. What keeps the file locked and how do you fix it?

**Concepts**
- `StreamWriter.Dispose()` only on the happy path — handle leak on exception
- OS file lock held until GC finalizes the undisposed writer
- `using` statement generating `try/finally` on all exit paths
- Validate before write vs write then validate

**Answer**

When the method throws after constructing `StreamWriter` but before the manual `Dispose()` call at the end, `Dispose` never runs — so the underlying `FileStream` handle stays open. On Windows, that open handle holds the file lock until the garbage collector finalizes the writer, which may not happen for minutes, causing every subsequent append attempt to get `IOException: file in use`. The fix is `using var writer = new StreamWriter(...)` so the compiler wraps the body in `try/finally` and calls `Dispose` on all exit paths including the exception path. If invalid rows must not be persisted, validate the entry before writing rather than after; if the rejected row should remain on disk (it already was written before the throw), that is intentional and the `using` pattern still ensures the handle releases.

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected.");
    using StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");
}
```

---

## Q44. (Scenario R) A CSV export written on Windows with `Encoding.Default` fails header validation on Linux containers. What is the root cause and fix?

**Concepts**
- `Encoding.Default` resolving to system code page (Windows-1252 on Windows, UTF-8 on Linux)
- Different byte sequences on disk per OS for the same text
- Reader default not matching writer encoding
- `new UTF8Encoding(false)` for portable no-BOM UTF-8

**Answer**

`Encoding.Default` is the system ANSI code page, which is Windows-1252 on most Windows machines and UTF-8 on modern Linux — so the bytes written on Windows differ from what the Linux reader's default expects, producing a BOM mismatch or silent character substitution that causes the string equality check on the header to fail. The fix pins both writer and reader to the same explicit encoding: `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` for UTF-8 without a BOM, passed identically to both `StreamWriter` and `StreamReader` constructors. For files consumed by Excel on Windows, I deliberately choose UTF-8 with BOM (`Encoding.UTF8`) and document that decision in the format contract rather than letting it vary by environment.

```csharp
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
using var writer = new StreamWriter(exportPath, append: false, utf8);
using var reader = new StreamReader(importPath, utf8);
```

---

## Q45. (Scenario R) A support dashboard calls `StreamReader.ReadToEnd()` on customer logs that can exceed 10 GB, causing `OutOfMemoryException`. What is wrong and what replaces it?

**Concepts**
- `ReadToEnd` materializing entire file as one `string` on the LOH
- Multi-GB LOH allocation triggering OOM under concurrent requests
- Tail-read pattern: seek near EOF and read bounded chunk
- Line-by-line streaming with bounded result set

**Answer**

`ReadToEnd` allocates a single `string` for everything from the current position to EOF — a 10 GB log produces a 10 GB managed string, which exceeds the LOH and causes `OutOfMemoryException`, often recycling the worker process mid-request. For a "show last N lines" support view, the right pattern is to open a `FileStream`, seek to a position near EOF with `Seek(-windowBytes, SeekOrigin.End)`, read a bounded block, split on newlines, and return the last N lines — keeping allocation proportional to the window size rather than the file. For full sequential scanning, replace `ReadToEnd` with a `while (await reader.ReadLineAsync(ct) is { } line)` loop that processes one line at a time, and cap the response at a fixed maximum (for example 500 lines or 256 KB) before returning.

---

## Q46. (Scenario R) A long-running export writes `IN_PROGRESS` then `COMPLETE` to a status file, but operators see `IN_PROGRESS` forever after a crash. What causes it and how do you harden it?

**Concepts**
- `StreamWriter` internal buffer not flushed until `Flush()` or `Dispose`
- `AutoFlush = true` ensuring each status token reaches disk immediately
- Lack of `using` causing handle leak and missing `COMPLETE` write on crash
- Write-to-temp-then-atomic-move for crash-consistent status

**Answer**

`StreamWriter` buffers output in memory — `IN_PROGRESS` may not reach disk until `Flush()` or `Dispose` is called, so a poller can see an empty file after the status line was written programmatically. When the process is killed mid-export, `COMPLETE` is never written and the status file stays as `IN_PROGRESS` forever because the writer was never disposed and never flushed. Setting `AutoFlush = true` on the writer ensures each `WriteLine` is immediately flushed to the OS buffer, making status tokens visible to pollers without a restart. Wrapping the writer in `using` and writing `FAILED` in a `catch`/`finally` ensures the final state is always recorded; for stronger crash safety, write the terminal status to a temp file and `File.Move` atomically, so the poller never sees a truncated or empty status file.

---

## Q47. (Scenario R) A log tailer and log writer in the same app produce intermittent `IOException: process cannot access the file`. What sharing rule is missing?

**Concepts**
- Default `FileShare.Read` on tailer's `FileStream` — exclusive write lock
- `FileShare.ReadWrite` required so both handles coexist
- Path-based `StreamReader`/`StreamWriter` constructors hiding share flags
- Explicit `FileStream` construction to control sharing

**Answer**

Opening the tailer's `FileStream` without specifying a share mode defaults to `FileShare.Read`, which grants the tailer exclusive write access — so when the writer tries to open the same path for append, the OS rejects it with a sharing violation even though the tailer is not writing. Both sides need to cooperate: the tailer opens with `FileShare.ReadWrite` so writers can append, and the writer opens with at least `FileShare.Read` so other readers can coexist. The path-based `StreamReader` and `StreamWriter` constructors create their own `FileStream` with sharing defaults the caller never sees, which is why the problem is invisible until load exposes the concurrency. The fix is to construct `FileStream` explicitly with the desired `FileShare` flag and then pass it to `StreamReader(fs)` or `StreamWriter(fs)`.

---

## Q48. (Scenario P) An ASP.NET Core hosted service calls `reader.ReadLine()` (synchronous) inside an async loop. Thread-pool queue depth grows under load. What is the problem and fix?

**Concepts**
- Synchronous `ReadLine()` blocking a thread-pool thread during disk I/O
- Thread-pool starvation identical to `.Result` in async context
- `ReadLineAsync(CancellationToken)` freeing the thread during wait
- EOF handling on a growing feed file

**Answer**

`ReadLine()` is synchronous — it blocks the calling thread-pool thread for the entire duration of the disk read, so each concurrent execution of the hosted service occupies a thread doing nothing but waiting, which starves other requests as the pool fills with blocked threads. Since the service is otherwise async, the fix is `await reader.ReadLineAsync(stoppingToken)`, which releases the thread during the I/O wait and only resumes when data is available. At EOF on a growing file, `ReadLineAsync` returns `null` just like the synchronous version, so the existing `Delay` loop for polling still works; for a file that is actively being appended by another process, I also need to ensure the reader opens with `FileShare.ReadWrite` so the writer's handle does not block the reader's open.

---

## Q49. (Scenario R) A tool patches the first line of a config file in place then reads the remainder, but throws `ObjectDisposedException` after refactoring. What is wrong?

**Concepts**
- `leaveOpen: false` default on `StreamReader` closing the shared `FileStream`
- LIFO disposal order of nested `using` statements
- Both reader and writer must use `leaveOpen: true` when sharing one stream
- Safer alternative: read-patch-write via temp file and atomic replace

**Answer**

When `StreamReader` is constructed without `leaveOpen: true` (the default is `false`), disposing the reader closes the underlying `FileStream` — which happens when the `StreamReader`'s `using` block ends, leaving the `FileStream` closed for anyone else holding a reference to it. Any subsequent call on the `FileStream` or writer that was also wrapping it throws `ObjectDisposedException`. The fix is to set `leaveOpen: true` on both the `StreamWriter` and `StreamReader` so they act as views over the shared stream without claiming ownership, then dispose the raw `FileStream` explicitly after both wrappers are done. The safer and more correct pattern for in-place header patching is to read the full content into memory, apply the edit, write to a temp file, and call `File.Replace` or `File.Move`, which avoids length-mismatch corruption when the new header is shorter than the original.

---

## Q50. (Scenario M) A cross-platform app parses `.env`-style files using `ReadToEnd` + `Split('\n')`. Keys with `\r` suffixes break lookups on Windows files. What is wrong and how should line-based parsing use `StreamReader`?

**Concepts**
- `Split('\n')` leaving `'\r'` at end of each key on CRLF files
- `StreamReader.ReadLine()` stripping both `\r\n` and `\n` uniformly
- `ReadToEnd` reintroducing large-file allocation risk
- Defensive `Trim()` on keys as belt-and-suspenders guard

**Answer**

Splitting on `'\n'` alone does not strip the `'\r'` that precedes it in Windows CRLF line endings, so keys parsed from a Windows-written file contain a trailing carriage return — `map["HOST"]` silently misses lookups for `"HOST\r"`. `StreamReader.ReadLine()` handles both `\r\n` and `\n` uniformly, stripping the terminator before returning the line, which is the correct fix. Replacing `ReadToEnd().Split('\n')` with a `while ((line = reader.ReadLine()) != null)` loop also eliminates the `ReadToEnd` LOH risk. I also add `key = key.Trim()` defensively on each parsed key since editor tools on various platforms sometimes insert invisible whitespace, and I document the encoding expectation (UTF-8) in the format contract so the reader is constructed with the matching encoding explicitly.

---

## 03. FileStream & Binary Files

---

## Q51. What is the difference between `File`, `Stream`, and `FileStream`?

**Concepts**
- `File` — static utility class, no state, wraps `FileStream` internally
- `Stream` — abstract base defining byte I/O contract
- `FileStream` — concrete `Stream` implementation backed by a filesystem file
- Lifecycle: `File` one-shot; `FileStream` held open for duration of use

**Answer**

`File` is a purely static helper — every method like `ReadAllText` or `Create` opens a `FileStream` internally, performs the operation, and closes it, with no persistent state. `Stream` is the abstract class that defines the byte-level contract (`Read`, `Write`, `Seek`, `Flush`, `Position`, `Length`) that all I/O types share. `FileStream` is the concrete `Stream` subclass backed by an OS file handle — it keeps the handle open for as long as the object lives, which gives fine-grained control over buffering, seeking, and sharing. I use `File` static methods for one-shot operations and `FileStream` when I need the handle open across multiple reads or writes, need to configure `FileMode`/`FileAccess`/`FileShare` precisely, or need to wrap the stream with `StreamReader`/`BinaryReader`.

---

## Q52. Explain `FileMode` (`CreateNew`, `Create`, `Open`, `OpenOrCreate`, `Truncate`, `Append`) — when use each?

**Concepts**
- `CreateNew` — fail if exists, exclusive atomic create
- `Create` — truncate if exists, create if not
- `Open` — fail if not exists
- `OpenOrCreate` — open existing or create new
- `Truncate` — open existing and set length to zero
- `Append` — open or create, seek to EOF

**Answer**

`CreateNew` is the atomic "create only if not exists" mode — it throws `IOException` if the path exists, making it the right choice for idempotency guards. `Create` truncates an existing file or creates a new one, so it is safe for overwrite scenarios where old content must be discarded. `Open` opens an existing file and throws `FileNotFoundException` if it does not exist, suitable when the file's prior existence is a precondition. `OpenOrCreate` opens the file if it exists or creates a new empty one if it does not, positioned at byte zero. `Truncate` opens an existing file and immediately sets its length to zero without creating a new one — used to wipe and rewrite in place. `Append` opens or creates the file and positions writes at the end-of-file, making every write an append without needing to seek.

---

## Q53. Explain `FileAccess` (`Read`, `Write`, `ReadWrite`) and `FileShare` (`None`, `Read`, `Write`, `ReadWrite`, `Delete`).

**Concepts**
- `FileAccess` gating what operations this handle may perform
- `FileShare` declaring what concurrent openers are permitted
- Most-restrictive-combination semantics when multiple handles exist
- Mismatch between two openers causing sharing violation

**Answer**

`FileAccess` is a per-handle capability flag: `Read` permits only reads, `Write` permits only writes, and `ReadWrite` permits both — attempting an operation not covered by the flag throws `NotSupportedException` or `UnauthorizedAccessException` at runtime. `FileShare` declares what the caller is willing to share with other openers of the same path: `None` is exclusive, `Read` allows other read-only openers, `Write` allows other writers, `ReadWrite` allows any combination, and `Delete` permits the file to be marked for deletion while the handle is open. The OS enforces the most-restrictive combination: if a first opener uses `FileShare.Read` and a second opener requests `FileAccess.Write`, the second open fails because the first did not allow write sharing. A sharing violation from a concurrent open always means at least one side's requested access conflicts with the other side's declared share.

---

## Q54. Why does default `FileShare.None` cause sharing violations when another process needs read access?

**Concepts**
- `FileShare.None` — exclusive lock, no concurrent openers permitted
- OS checking requested access against the holder's declared share
- Production pattern: writer with `FileShare.Read` to allow concurrent readers
- Common default trap in `FileStream` constructor overloads

**Answer**

When a `FileStream` is constructed using the overload that omits the `FileShare` parameter, it defaults to `FileShare.None`, which tells the OS that no other process or thread should be allowed to open the file at all — not even for reading — while this handle is open. Since `FileShare.None` permits zero concurrent openers, any attempt by a reader or another writer to open the same path gets `IOException: sharing violation`. The fix is to specify `FileShare.Read` on a writer that does not need write exclusivity, which allows concurrent readers while still blocking other writers. For concurrent append scenarios, `FileShare.ReadWrite` on both sides is required, though that only resolves the opening conflict and does not prevent interleaved writes without additional synchronization.

---

## Q55. What are `FileStream.Position`, `Seek`, and `Length` — and when is seeking valid?

**Concepts**
- `Position` — current byte offset from stream start
- `Seek(offset, SeekOrigin)` — reposition the stream pointer
- `Length` — total size of the file in bytes
- `CanSeek` — false on non-seekable streams like `NetworkStream`

**Answer**

`Position` is the byte offset at which the next read or write will occur; reads and writes advance it automatically. `Seek(offset, origin)` repositions the pointer using a `SeekOrigin` of `Begin`, `Current`, or `End` — for example `Seek(-8, SeekOrigin.End)` positions 8 bytes before the end of file. `Length` returns the current file size in bytes and may differ from `Position` if the stream was opened in append mode or if another process is writing. Seeking is only valid when `CanSeek` is `true`, which it is for `FileStream` backed by a regular file but not for `NetworkStream`, pipe streams, or `CryptoStream` in certain modes; calling `Seek` on a non-seekable stream throws `NotSupportedException`.

---

## Q56. Explain `SeekOrigin` (`Begin`, `Current`, `End`) with a concrete read-modify-write scenario.

**Concepts**
- `SeekOrigin.Begin` — absolute offset from file start
- `SeekOrigin.Current` — relative to current position
- `SeekOrigin.End` — offset from end of file (negative values read backwards)
- Seeking to overwrite a specific record without rewriting the whole file

**Answer**

`SeekOrigin.Begin` sets position to an absolute byte offset from the start of the file, which is the right choice when record offsets are stored in an index. `SeekOrigin.Current` moves relative to where the pointer already is, useful when skipping a known number of bytes after parsing a header. `SeekOrigin.End` is used when offsets are measured from the end — `Seek(-8, SeekOrigin.End)` positions exactly 8 bytes before EOF to read a footer magic number. A concrete read-modify-write scenario: read a fixed 16-byte header at position 0, advance through records to find the one to update, call `Seek(recordStart, SeekOrigin.Begin)`, write the modified record bytes, then call `Flush()` — the rest of the file is untouched because seeks never reallocate file content.

---

## Q57. What happens if you seek on a non-seekable stream (e.g., some network streams)?

**Concepts**
- `CanSeek` property indicating seek support
- `NotSupportedException` thrown on seek attempt
- `NetworkStream`, pipe streams, and certain `CryptoStream` modes as non-seekable
- Buffering workarounds when seek is needed on non-seekable source

**Answer**

Calling `Seek` or setting `Position` on a stream where `CanSeek` returns `false` throws `NotSupportedException` — the stream has no underlying random-access store to reposition. `NetworkStream` is non-seekable because TCP delivers a forward-only byte sequence with no ability to rewind, and pipe streams are similarly one-directional. If I need to re-read part of a non-seekable stream (for example to re-parse a header after reading it), I copy the relevant bytes into a `MemoryStream` first — which is seekable — and seek within that copy. The `CanSeek`, `CanRead`, and `CanWrite` properties exist precisely so code can check capabilities at runtime rather than catching `NotSupportedException` as control flow.

---

## Q58. What is the difference between `FileStream.Read`/`Write` and `ReadAsync`/`WriteAsync`?

**Concepts**
- Synchronous `Read`/`Write` blocking the calling thread
- `ReadAsync`/`WriteAsync` using OS async I/O and freeing the thread
- `useAsync: true` flag required on `FileStream` for true async I/O on Windows
- Performance: sync is fine for background batch work; async is required in web servers

**Answer**

`Read` and `Write` are synchronous — they block the calling thread until the OS completes the I/O, which on spinning disks or network-backed storage means the thread idles for milliseconds doing nothing useful. `ReadAsync` and `WriteAsync` issue the I/O request to the OS and `await` completion asynchronously, freeing the thread to handle other work. On Windows, true async file I/O only happens when the `FileStream` is opened with `useAsync: true` (or `FileOptions.Asynchronous`); without this flag, `ReadAsync` on Windows runs synchronous I/O on a thread-pool thread, which has the same net effect but wastes a thread. In ASP.NET Core endpoints and hosted services I always use async methods and open `FileStream` with `FileOptions.Asynchronous`; in command-line batch tools where only one stream is active, synchronous I/O is simpler and equally fast.

---

## Q59. What do `BinaryReader` and `BinaryWriter` add over raw `FileStream` byte operations?

**Concepts**
- Typed read methods (`ReadInt32`, `ReadDouble`, `ReadString`, `ReadBoolean`)
- Length-prefixed string encoding for `ReadString`/`Write(string)`
- Buffering a `Stream` argument for typed access
- `EndOfStreamException` on short reads vs silent partial buffer

**Answer**

`BinaryReader` and `BinaryWriter` layer typed primitive serialization on top of any `Stream`: instead of manually reading 4 bytes and calling `BitConverter.ToInt32`, I call `reader.ReadInt32()` and the encoding, byte order, and read-loop are handled internally. `BinaryWriter.Write(string)` encodes the string as a 7-bit-encoded length prefix followed by UTF-8 bytes, and `BinaryReader.ReadString()` reads it back — but this format is .NET-specific, so cross-language consumers cannot use it directly. `BinaryReader.ReadBytes(count)` throws `EndOfStreamException` if the stream ends before `count` bytes are read, unlike a raw `Stream.Read` that silently returns fewer bytes without throwing, which makes short-read errors explicit rather than hidden in zero-padded buffers.

---

## Q60. How does `BinaryReader` handle endianness and primitive types (`ReadInt32`, `ReadDouble`, `ReadString`)?

**Concepts**
- Little-endian byte order assumed on all platforms
- No built-in big-endian support in `BinaryReader`/`BinaryWriter`
- `BinaryPrimitives` for explicit endianness control
- Cross-language interoperability requires documented byte order

**Answer**

`BinaryReader` and `BinaryWriter` always use little-endian byte order — the least significant byte is stored first — regardless of the underlying platform's native endianness. This means files written with `BinaryWriter` on x64 Windows are readable by `BinaryReader` on any .NET platform, but a big-endian consumer written in C or Go will read numeric fields in the wrong order. When cross-language interoperability is needed, I use `BinaryPrimitives.WriteInt32BigEndian(buffer, value)` from `System.Buffers.Binary` to write in network byte order (big-endian) and document the byte order explicitly in the format specification. `ReadString` and `Write(string)` use a .NET-specific 7-bit-encoded length prefix format that has no standard equivalent in other languages, so for cross-platform binary files I write strings as a fixed-size byte length followed by raw UTF-8 bytes.

---

## Q61. What is the on-disk format of `BinaryWriter.Write(string)` — and why does it matter for cross-platform files?

**Concepts**
- 7-bit encoded variable-length integer as length prefix
- UTF-8 encoded string bytes following the length
- .NET-specific format with no standard cross-language equivalent
- Custom fixed-length-prefixed encoding for interop

**Answer**

`BinaryWriter.Write(string)` encodes the string length as a 7-bit encoded variable-length integer (1 byte for lengths 0–127, 2 bytes for 128–16383, and so on) followed by the UTF-8 encoded string bytes. `BinaryReader.ReadString()` reads this format back correctly, but no standard C, Python, or Go library knows about the 7-bit encoding scheme, so any cross-language consumer that tries to read this as a length-prefixed string will parse the prefix incorrectly and misalign every subsequent field. For binary files consumed outside .NET, I write strings with a fixed-size prefix — a 2-byte or 4-byte little-endian length in a documented byte order — followed by the raw UTF-8 bytes, avoiding the .NET-specific 7-bit encoding entirely and documenting the format in the file specification.

---

## Q62. What is the difference between text and binary file handling in C#?

**Concepts**
- Text mode: encoding conversion, line-ending normalization
- Binary mode: raw bytes, no transformation
- `StreamReader`/`StreamWriter` for text; `FileStream`/`BinaryReader` for binary
- `\r\n` vs `\n` normalization in text reads

**Answer**

Text file handling applies an encoding layer that converts bytes to characters and normalizes line endings — `StreamReader.ReadLine()` returns a `string` with the `\r\n` or `\n` stripped regardless of the original format, and `StreamWriter.WriteLine()` writes `Environment.NewLine` on the current platform. Binary file handling reads and writes raw bytes with no transformation; the caller is responsible for interpreting the byte sequence as typed values using `BinaryReader` or manual `BitConverter` calls. Mixing the two — reading binary data with a `StreamReader` — typically corrupts the content because the encoding conversion mangles bytes that are not valid character sequences in the assumed encoding. The mode choice is driven by the file format: structured text (CSV, log, config) uses text APIs; images, serialized records, archives, and protocol frames use binary APIs.

---

## Q63. When should you use `MemoryStream` instead of `FileStream`?

**Concepts**
- In-process byte buffer without disk I/O
- Testability: wrapping a `MemoryStream` in APIs that accept `Stream`
- Intermediate buffer before writing to another destination
- `ToArray()` / `GetBuffer()` for accessing accumulated bytes

**Answer**

I use `MemoryStream` when I need a seekable, in-memory byte buffer without touching the filesystem — for unit testing code that accepts a `Stream`, for accumulating bytes from multiple writes before sending them to a network socket or compressor in one shot, or for decoding a byte array into types via `BinaryReader` without creating a temp file. `MemoryStream` is also the right intermediate buffer when an API requires a `Stream` but I have a `byte[]` or `string` in hand: I wrap it with `new MemoryStream(bytes)` and pass the stream directly. For large payloads I prefer `FileStream` or a `PipeWriter` because `MemoryStream` grows its internal buffer on the heap, doubling it each time it fills, which can cause large LOH allocations and excessive GC for multi-megabyte data.

---

## Q64. What is buffered I/O, and how do `FileStream` buffer size options affect performance?

**Concepts**
- OS-level page cache as the primary buffer
- `FileStream` internal managed buffer reducing syscall frequency
- Default 4096-byte buffer
- `FileOptions.WriteThrough` and `FileOptions.SequentialScan` hints

**Answer**

Buffered I/O means that small reads and writes accumulate in an in-process buffer rather than each issuing a syscall — `FileStream`'s default 4096-byte internal buffer means 1000 one-byte writes coalesce into roughly one `Write` syscall instead of 1000, which dramatically reduces kernel transition overhead. Increasing the buffer size (for example to 65536 bytes) benefits sequential large-file reads and writes by reducing syscall frequency and aligning better with disk block sizes, but wastes memory for random-access patterns that rarely fill the buffer. `FileOptions.SequentialScan` hints the OS to pre-fetch pages ahead of the current position, which improves throughput for single-pass large file reads. `FileOptions.WriteThrough` bypasses the OS write cache and writes directly to stable storage, which increases latency per write but ensures data survives a process crash, making it appropriate for transaction logs or status files that must be durable.

---

## Q65. How do you read a fixed header followed by variable-length records from a binary file?

**Concepts**
- Fixed-length header parsed first with `BinaryReader` typed reads
- Variable-length record discovered via a length field prefix
- `Seek` to skip or re-read a record at a known offset
- EOF detection via `EndOfStreamException` or `BaseStream.Position`

**Answer**

I open the file with a `FileStream` wrapped in a `BinaryReader` and read the header fields in declaration order — `ReadInt32()` for magic number, `ReadInt16()` for version, and so on — since the header is always a fixed number of bytes at position zero. After consuming the header, I enter a loop that reads the record's length prefix (`ReadInt32()`), then reads exactly that many bytes with `ReadBytes(recordLength)` to deserialize the payload, repeating until `BaseStream.Position >= BaseStream.Length`. When I also need random access by record index, I build an in-memory offset table during the first sequential pass — storing each record's `BaseStream.Position` before the length read — so subsequent lookups use `Seek(offset, SeekOrigin.Begin)` to jump directly to any record without a full re-scan.

---

## Q66. What is a file signature (magic bytes), and how do you validate one without trusting the extension?

**Concepts**
- Magic bytes as a per-format byte sequence at a known file offset
- Extension as an unreliable hint controlled by the user
- `ReadBytes(n)` + `SequenceEqual` for header validation
- Defensive rejection before parsing format-specific content

**Answer**

A file signature is a fixed byte sequence embedded at a known offset — typically the first few bytes — that identifies the file format independently of its extension or name. PNG files start with `89 50 4E 47 0D 0A 1A 0A`, ZIP archives with `50 4B 03 04`, and PDF with `25 50 44 46`. I validate by reading the expected number of bytes with `BinaryReader.ReadBytes(n)` and comparing with `SequenceEqual` against the known signature array; if it does not match I reject the file before any parsing begins, which prevents malformed or malicious files masquerading as the expected format. Trusting the extension alone is insufficient because a user or upstream system can rename any file and the extension check passes silently even when the content is garbage or an exploit payload.

---

## Q67. (Scenario R) A telemetry service reads a fixed 4-byte file signature but short files produce garbage signatures without throwing. What is wrong?

**Concepts**
- `Stream.Read` returning fewer bytes than requested without throwing
- `BinaryReader.ReadBytes` returning a short array on EOF
- `EndOfStreamException` not thrown by `ReadBytes` on partial read
- Explicit length check on the returned byte array

**Answer**

`BinaryReader.ReadBytes(4)` does not throw when fewer than 4 bytes remain — it returns whatever bytes are available, so a 2-byte truncated file returns a 2-element array that is then compared against the 4-byte signature, and `SequenceEqual` returns `false` rather than throwing, so the rejection path is hit by accident rather than by design. The correct pattern is to check `ReadBytes(4).Length == 4` before calling `SequenceEqual`, or to use `BinaryReader.Read` in a loop via `ReadExactly` (available since .NET 7) which throws `EndOfStreamException` if the stream ends before the count is satisfied. Catching `EndOfStreamException` explicitly and mapping it to a "file too short to be a valid signature" error gives a clear diagnostic instead of a silent false-positive rejection.

---

## Q68. (Scenario R) A background job appends binary audit records with `FileShare.None`; a dashboard reader gets `IOException`. What locking mismatch causes the failure?

**Concepts**
- `FileShare.None` blocking all concurrent openers
- Dashboard reader's `FileAccess.Read` conflicting with writer's exclusive share
- `FileShare.Read` on writer permitting simultaneous readers
- Independent `FileShare.ReadWrite` for full concurrent access

**Answer**

When the background job opens the file with the default or explicit `FileShare.None`, it tells the OS to deny all other opens regardless of their requested access, so the dashboard reader's open with `FileAccess.Read` is rejected and `IOException: sharing violation` is thrown. The fix is to change the writer to use `FileShare.Read` — it still holds exclusive write access (no other writer can open) but grants read-only openers permission to coexist. If the architecture requires both concurrent reads and concurrent writes from multiple processes, all openers must agree on `FileShare.ReadWrite`, but then line-level atomicity must be enforced by the application since the OS does not serialize writes.

---

## Q69. (Scenario R) A teammate ports `inventory.bin` readers from another language and swaps field read order. Prices and names are nonsense after the first record. What breaks?

**Concepts**
- Binary format as a strict positional contract
- `BinaryReader` advancing position on every read
- Field order mismatch desynchronizing all subsequent reads
- Format documentation and magic-byte versioning

**Answer**

`BinaryReader` advances the stream position on every read call, so reading field A before field B when the file stores B before A consumes the wrong bytes for both — and because the position is now wrong by the size of the misread fields, every subsequent record reads shifted bytes that decode as garbage. The root cause is that the format contract was undocumented or not followed exactly by the port. The fix is to define the binary layout as a versioned specification — field name, type, byte order, and byte offset within each record — and add an assertion at the read site that `BaseStream.Position` matches the expected offset after each record to catch drift early. Adding a magic byte and version field at the start of the file means the reader can reject old formats fast rather than silently misinterpreting fields.

---

## Q70. (Scenario R) A log-rotation utility calls `Seek(0, SeekOrigin.End)` to read the last 8 bytes. It intermittently returns wrong bytes. What is the seek mistake?

**Concepts**
- `SeekOrigin.End` with offset zero positions after the last byte
- Reading after seeking to EOF returns zero bytes
- Negative offset required to position before EOF
- `Seek(-8, SeekOrigin.End)` for 8 bytes before end

**Answer**

`Seek(0, SeekOrigin.End)` positions the stream pointer exactly at the end of the file — one byte past the last byte — so any subsequent `Read` call immediately hits EOF and returns zero bytes without throwing. To read the last 8 bytes the call must be `Seek(-8, SeekOrigin.End)`, which positions 8 bytes before the end. The intermittent behavior comes from files shorter than 8 bytes producing a negative absolute position, which causes `Seek` to clamp or throw depending on the platform, so the caller should also guard with `if (stream.Length >= 8)` before issuing the seek.

---

## Q71. (Scenario P) A .NET service writes `metrics.bin` consumed by a Linux C tool on big-endian ARM. Default `BinaryWriter` fields decode as garbage. What is the root cause?

**Concepts**
- `BinaryWriter` always writing little-endian byte order
- Big-endian ARM expecting most-significant-byte-first layout
- `BinaryPrimitives.WriteInt32BigEndian` for explicit byte order
- Format specification documenting endianness as a contract

**Answer**

`BinaryWriter` unconditionally uses little-endian byte order on all .NET platforms — the least significant byte is written first — so an `int32` value of 1 is stored as `01 00 00 00`. A C tool on a big-endian ARM processor expects `00 00 00 01`, reads the bytes in the wrong order, and decodes a completely different number. The fix is to abandon `BinaryWriter` for the numeric fields and instead use `BinaryPrimitives.WriteInt32BigEndian(buffer, value)` from `System.Buffers.Binary`, which fills a `Span<byte>` in network byte order (big-endian), then write that buffer to the `FileStream` directly. The format specification must document the byte order explicitly so all consumers — regardless of language or platform — know what to expect.

---

## Q72. (Scenario D) A 60 GB append-only binary archive needs random access by fixed record index. Compare `FileStream` + `Seek` vs `MemoryMappedFile`. When do you choose each?

**Concepts**
- `FileStream` + `Seek` — kernel handles paging, low managed footprint
- `MemoryMappedFile` — OS virtual memory maps file pages on demand
- `MemoryMappedFile` overhead at OS level for small record reads
- Concurrent reader processes sharing the same memory-mapped region

**Answer**

`FileStream` with `Seek` computes `recordIndex * recordSize` to issue a targeted kernel read — the OS fetches only the relevant disk block into the page cache, managed memory stays minimal, and it works correctly in any process isolation model. `MemoryMappedFile` maps the file into the process's virtual address space so record access is a pointer dereference after the first page fault — it excels when the access pattern is dense and spread across the file (many random reads in a short window) because the OS reuses already-faulted pages without issuing repeat read syscalls. I choose `FileStream` + `Seek` when records are accessed infrequently or in isolation and when the process lifetime is short, to avoid the setup cost and virtual-address pressure of mapping 60 GB. I choose `MemoryMappedFile` when multiple reader processes need to share the mapped region (the OS backs them with the same physical pages) or when access patterns are dense enough to benefit from pointer-level latency.

---

## Q73. (Scenario R) An export worker calls `WriteAsync` then immediately publishes a message queue signal. The processor reads zero-length or incomplete files. What timing issue causes this?

**Concepts**
- `FileStream` write buffer not flushed to OS on `WriteAsync` completion
- `FlushAsync()` ensuring buffered bytes reach the OS file cache
- OS page cache vs stable storage distinction
- Signal-after-flush-and-close ordering guarantee

**Answer**

`WriteAsync` completes when the bytes are transferred to `FileStream`'s internal managed buffer — not necessarily when they reach the OS page cache or disk. The message queue signal fires while the bytes are still in the buffer, so when the processor opens the file immediately after receiving the signal it sees zero bytes or a partial file. The fix is to call `await stream.FlushAsync()` before publishing the signal, which transfers the buffer to the OS; if durability across a process crash is required, `FlushAsync` must be followed by opening the stream with `FileOptions.WriteThrough` or calling `SafeFileHandle.Flush`. Alternatively, disposing the `FileStream` before publishing the signal achieves the same flush — `DisposeAsync` calls `FlushAsync` internally.

---

## Q74. (Scenario R) A cache service opens `cache.bin` with `FileAccess.Read` then calls `WriteByte`, and separately tries `FileMode.Create` with `FileAccess.Read`. Both throw. What `FileMode`/`FileAccess` mismatches cause each failure?

**Concepts**
- `FileAccess.Read` restricting handle to read-only operations
- `NotSupportedException` on write attempt against read-only handle
- `FileMode.Create` implying write intent — incompatible with read-only access
- Correct pairing: `FileMode.Create` with `FileAccess.Write` or `ReadWrite`

**Answer**

Opening with `FileAccess.Read` grants a read-only handle — any call to `WriteByte`, `Write`, or `WriteAsync` on that handle throws `NotSupportedException` because the handle capability does not include write, regardless of file permissions on disk. `FileMode.Create` tells the OS to create the file or truncate the existing one, which is an inherently write operation — pairing it with `FileAccess.Read` is a logical contradiction and the OS rejects the open, throwing `ArgumentException` or `UnauthorizedAccessException` depending on the platform. The correct pairings are `FileMode.Create` with `FileAccess.Write` (write-only) or `FileAccess.ReadWrite` (read back what was written), and `FileMode.Open` with `FileAccess.Read` for read-only access to an existing file.

---

## 04. Path & Environment Classes

---

## Q75. What is the `Path` class, and why should you never hard-code `\` or `/` separators?

**Concepts**
- `Path` as a pure string-manipulation utility with no filesystem I/O
- `Path.DirectorySeparatorChar` resolving to `\` on Windows and `/` on Linux
- `Path.Combine` for safe segment joining
- Cross-platform portability via separator abstraction

**Answer**

`Path` is a static utility class that manipulates path strings without touching the filesystem — it never opens a handle or checks whether a path exists. Hard-coding `\` breaks on Linux and macOS where the separator is `/`, and hard-coding `/` produces incorrect results when a Windows path component itself starts with `/` (which is interpreted as a root). `Path.Combine` joins segments using `Path.DirectorySeparatorChar`, which .NET sets to the correct value for the current OS, so the same source code produces valid paths on all platforms. `Path.AltDirectorySeparatorChar` and `Path.VolumeSeparatorChar` cover edge cases like UNC paths and drive letters.

---

## Q76. How does `Path.Combine` behave with trailing slashes, rooted segments, and empty segments?

**Concepts**
- Rooted segment discarding all prior segments
- Trailing separator on an earlier segment absorbed correctly
- Empty string segment treated as current-directory component
- `null` segment throwing `ArgumentNullException`

**Answer**

`Path.Combine`'s most surprising behavior is that a rooted segment (one starting with `/` or a drive letter like `C:\`) discards all previously accumulated segments — `Path.Combine("a", "b", "/absolute")` returns `/absolute`, not `a/b/absolute`. This is intentional but frequently causes path injection bugs when the second segment comes from user input. Trailing slashes on an earlier segment are absorbed without doubling: `Path.Combine("a/", "b")` returns `a/b`. Empty string segments are treated as a no-op on the combination result but do not throw. Any `null` argument throws `ArgumentNullException` immediately. I always validate that user-supplied path components are not rooted (using `Path.IsPathRooted`) before passing them to `Path.Combine` to prevent path traversal vulnerabilities.

---

## Q77. What is the difference between `Path.GetFullPath` and passing a relative path directly to `File.Open`?

**Concepts**
- `Path.GetFullPath` resolving relative path against `Environment.CurrentDirectory`
- `File.Open` resolving relative path at open time — also against current directory
- Snapshot vs late-binding behavior
- Path traversal normalization (`..` resolution) by `GetFullPath`

**Answer**

`Path.GetFullPath` immediately combines the relative path with `Environment.CurrentDirectory` and normalizes all `..` and `.` segments, producing an absolute canonical string that can be logged, compared, or validated before the file is opened. Passing a relative path directly to `File.Open` defers the resolution to the OS open call, which also uses the current directory — but since `CurrentDirectory` can be changed between the call to `GetFullPath` and the call to `File.Open`, the two methods may resolve to different absolute paths under concurrent code. The more important reason to use `GetFullPath` explicitly is to normalize `..` traversals before a security check: `Path.GetFullPath` collapses `"uploads/../etc/passwd"` to `"/etc/passwd"`, allowing the containment check to work correctly before the open.

---

## Q78. Explain `Path.GetDirectoryName`, `GetFileName`, `GetFileNameWithoutExtension`, and `GetExtension`.

**Concepts**
- `GetDirectoryName` returning the parent folder string (no I/O)
- `GetFileName` returning the last segment including extension
- `GetFileNameWithoutExtension` stripping the final dot-extension
- `GetExtension` returning the dot-prefixed extension or empty string

**Answer**

All four methods are pure string operations with no filesystem access. `Path.GetDirectoryName("a/b/c.txt")` returns `"a/b"` — the parent segment without a trailing separator. `Path.GetFileName("a/b/c.txt")` returns `"c.txt"` — everything after the last separator. `Path.GetFileNameWithoutExtension("a/b/c.txt")` returns `"c"` — the last segment minus the final dot and extension. `Path.GetExtension("a/b/c.txt")` returns `".txt"` with the leading dot; it returns an empty string if there is no dot in the last segment. `GetExtension` only looks at the last segment, so `Path.GetExtension("/etc/init.d")` returns `".d"` which is sometimes unexpected — it treats the last segment's last dot as the extension boundary.

---

## Q79. What do `Path.GetTempPath`, `Path.GetTempFileName`, and `Path.GetRandomFileName` return — and what are the security implications of `GetTempFileName`?

**Concepts**
- `GetTempPath` returning OS temp directory path
- `GetTempFileName` creating a zero-byte file and returning its path — TOCTOU-free
- `GetRandomFileName` returning a random name string without creating anything
- `GetTempFileName` leaking handles and using sequential names on old Windows

**Answer**

`Path.GetTempPath()` returns the OS temp directory (`%TEMP%` on Windows, `/tmp` on Linux) as a string without creating anything. `Path.GetTempFileName()` atomically creates a zero-byte file in that directory and returns its unique path, which avoids the TOCTOU race of generating a name and separately creating the file — the file exists and is owned by the caller before the path is returned. The security concern with `GetTempFileName` is that on older Windows versions the names are sequential and predictable, making symlink attacks or name guessing feasible in shared temp directories; `Path.GetRandomFileName()` generates a cryptographically random name string (8.3 format) without creating anything on disk, which I combine with `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` and then `File.Create` to get an unpredictable path. Regardless of method, temp files must always be deleted in a `finally` block or they accumulate indefinitely.

---

## Q80. How do `Path.IsPathRooted`, `HasExtension`, and `ChangeExtension` work?

**Concepts**
- `IsPathRooted` checking for leading separator or drive letter
- `HasExtension` checking for a dot in the last segment
- `ChangeExtension` swapping or removing the extension
- All three as pure string operations, no filesystem access

**Answer**

`Path.IsPathRooted` returns `true` when the path starts with a directory separator (`/`) or a drive letter followed by `:` on Windows — it is the correct guard before passing user-supplied path components to `Path.Combine` to prevent rooted-segment injection. `Path.HasExtension` returns `true` when the last segment contains a dot that is not the first or last character — it is a heuristic rather than a filesystem check, so `"file."` returns `true` even though the extension is empty. `Path.ChangeExtension("report.csv", ".json")` returns `"report.json"` by replacing everything after the last dot; passing `null` as the extension removes it entirely; passing an extension without a leading dot adds the dot automatically.

---

## Q81. What invalid path characters does `Path.GetInvalidPathChars` / `GetInvalidFileNameChars` expose?

**Concepts**
- `GetInvalidPathChars` listing characters illegal in the full path string
- `GetInvalidFileNameChars` listing characters illegal in a single filename segment
- Platform-specific character sets — Windows larger than Linux
- Input validation before constructing paths from user data

**Answer**

`Path.GetInvalidPathChars()` returns the set of characters the OS forbids anywhere in a path string — on Windows this includes `<`, `>`, `"`, `|`, `?`, `*`, and control characters; on Linux/macOS only the null byte and `/` are truly invalid. `Path.GetInvalidFileNameChars()` returns a superset of the path-invalid characters plus the path separator itself, so it is used when validating a single file or folder name segment rather than a full path. I use these arrays to sanitize user-supplied filenames by replacing or stripping forbidden characters before constructing the path, and I use `Path.IsPathRooted` alongside them because a segment containing only valid characters can still inject an absolute path if it starts with a separator.

---

## Q82. What is `Environment.SpecialFolder`, and how do you resolve `MyDocuments`, `ApplicationData`, and `LocalApplicationData`?

**Concepts**
- `Environment.SpecialFolder` enum mapping logical folders to OS-specific paths
- `Environment.GetFolderPath(SpecialFolder.X)` resolving to the actual path
- `ApplicationData` for roaming user data (synced in domain environments)
- `LocalApplicationData` for machine-local user data (not synced)

**Answer**

`Environment.SpecialFolder` is an enum that abstracts OS-specific well-known directory locations — `MyDocuments`, `Desktop`, `ApplicationData`, `LocalApplicationData`, `ProgramFiles`, `Windows`, and so on — so code does not need to know `C:\Users\username\Documents` on Windows vs `~/Documents` on macOS. Calling `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)` returns the roaming profile path (`%APPDATA%`, typically `C:\Users\name\AppData\Roaming`) where settings that should follow a domain user across machines belong. `LocalApplicationData` resolves to `%LOCALAPPDATA%` (`C:\Users\name\AppData\Local`), which is machine-specific and not roamed — the better choice for caches, logs, and large app data. On Linux and macOS these methods resolve to XDG-compatible paths when available or fall back to the home directory subdirectories.

---

## Q83. How does `Environment.GetFolderPath` differ from hard-coding `C:\Users\...`?

**Concepts**
- Hard-coded paths encoding assumptions about username and OS version
- `GetFolderPath` delegating to the OS to resolve the actual location
- Roaming profiles, network homes, and redirected folder support
- Cross-platform: same call returns valid paths on Windows, Linux, macOS

**Answer**

Hard-coding `C:\Users\username\AppData\Roaming\MyApp` encodes the username, the drive letter, and an assumption about the operating system — none of which are stable across users, machines, or platforms. `Environment.GetFolderPath` asks the OS at runtime where that folder actually lives for the current user, which handles redirected folders (an IT policy might move `AppData` to a network share), UWP app isolation, Linux home directories, and macOS Library paths all correctly. The username does not appear in calling code at all, so the same line `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)` works identically in development on a Windows laptop and production on a Linux container.

---

## Q84. What is `Environment.CurrentDirectory`, and how can it differ from the executable's location?

**Concepts**
- `CurrentDirectory` as the process's working directory, set at launch
- Shell working directory passed to child processes at creation
- Difference from assembly location or publish output directory
- Mutable at runtime — `Environment.CurrentDirectory = newPath`

**Answer**

`Environment.CurrentDirectory` is the process's current working directory — it is the base path that the OS uses to resolve relative file paths. It is inherited from the parent process (or shell) that launched the application, so running `dotnet run` from `C:\Projects` sets `CurrentDirectory` to `C:\Projects`, not the output directory where the DLL lives. This means relative paths like `"config/settings.json"` resolve against the shell's location rather than the app's location, which causes "works on my machine" failures when running as a Windows Service (where current directory is `C:\Windows\System32`) or a systemd unit. The fix is to use `AppContext.BaseDirectory` as the anchor for app-relative paths rather than relying on `CurrentDirectory`.

---

## Q85. How do you get the application base directory in modern .NET (`AppContext.BaseDirectory`, `AppDomain.CurrentDomain.BaseDirectory`)?

**Concepts**
- `AppContext.BaseDirectory` — publish output or host directory, stable across platforms
- `AppDomain.CurrentDomain.BaseDirectory` — equivalent but older API
- `Assembly.Location` — path to the DLL (empty for single-file publish)
- Preferred anchor for app-relative content files

**Answer**

`AppContext.BaseDirectory` returns the directory where the application's output files are published — the folder containing the entry assembly — and is the recommended anchor for loading config files, templates, and static assets that are deployed alongside the executable. It is stable across all .NET hosting models including Docker containers, Windows Services, and Lambda functions. `AppDomain.CurrentDomain.BaseDirectory` returns the same value on modern .NET (the two are equivalent) but carries the older `AppDomain` API surface. `Assembly.GetExecutingAssembly().Location` also returns the DLL path in most cases, but it returns an empty string for single-file published apps where the assembly is embedded in the host executable — making it unreliable for path resolution in deployment scenarios.

---

## Q86. What is the difference between absolute and relative paths in console apps vs ASP.NET Core?

**Concepts**
- Console app `CurrentDirectory` set by the launching shell
- ASP.NET Core `IWebHostEnvironment.ContentRootPath` as the stable anchor
- `IWebHostEnvironment.WebRootPath` for `wwwroot` static files
- Avoid relative paths in both — use anchored absolute paths

**Answer**

In a console app, `Environment.CurrentDirectory` is whatever the shell was in when the process started — running `./myapp` from the home directory sets a different current directory than a service manager launching the same binary. In ASP.NET Core, Kestrel starts the host with `ContentRootPath` set to the application's publish directory (or `Directory.GetCurrentDirectory()` in development), exposed via `IWebHostEnvironment.ContentRootPath`. Resolving config or content file paths against `ContentRootPath` works consistently across local dev and production deployment. For web-accessible static files, `WebRootPath` points to the `wwwroot` folder. The safe rule for both models is to resolve every file path to an absolute path anchored on `AppContext.BaseDirectory` or the environment's content root at startup, store it, and use that absolute path throughout the application lifetime.

---

## Q87. How do UNC paths (`\\server\share`) interact with `Path.Combine` and `Path.GetFullPath`?

**Concepts**
- UNC paths treated as rooted by `Path.IsPathRooted`
- `Path.Combine` discarding previous segments when a UNC path is an argument
- `Path.GetFullPath` preserving UNC prefix without normalizing the host component
- Network latency and unavailability unlike local filesystem paths

**Answer**

UNC paths are rooted — `Path.IsPathRooted(@"\\server\share\folder")` returns `true` — so passing one as any argument to `Path.Combine` discards all preceding segments, identical to passing a drive-rooted path. `Path.GetFullPath` accepts and returns UNC paths unchanged when they are already absolute: `\\server\share\folder` normalizes any `..` segments in the local part but does not attempt to resolve the `\\server\share` prefix against any base. The key operational difference is that `File` and `Directory` operations on UNC paths incur network I/O and can fail with network-specific exceptions (`IOException` with an underlying network error code) or extremely long timeouts when the server is unreachable, so timeouts and retry policies that are unnecessary for local disk paths become important.

---

## Q88. What cross-platform path differences matter when deploying the same code on Windows and Linux?

**Concepts**
- `\` vs `/` as directory separator
- Case sensitivity: Windows case-insensitive, Linux case-sensitive
- Drive letters absent on Linux — paths start with `/`
- MAX_PATH (260) on Windows vs longer limits on Linux

**Answer**

The four differences that cause production bugs are: separator character (`\` vs `/`), case sensitivity (`Config\app.json` and `config/app.json` are the same file on Windows but different files on Linux), drive letter syntax (no `C:` on Linux — paths start with `/`), and path length limits (Windows MAX_PATH is 260 characters unless long-path support is enabled; Linux allows up to 4096). Using `Path.Combine` and `Path.DirectorySeparatorChar` avoids separator issues, but case sensitivity requires discipline in the codebase — file and directory names in source code must exactly match the case on disk. Path length problems appear silently in test frameworks and build pipelines that create deeply nested output directories, failing only on Windows with `PathTooLongException` while the same paths work on Linux CI.

---

## Q89. (Scenario R) A report exporter hard-codes `"\\"` separators and uses `SpecialFolder.MyDocuments` on Linux CI. What is wrong and how do you fix it for cross-platform deployment?

**Concepts**
- `\\` separators invalid as path separators on Linux
- `SpecialFolder.MyDocuments` resolving to home-directory subfolder on Linux but often unsuitable for server output
- `Path.Combine` for OS-agnostic joining
- Environment-variable-configured output directory for server deployments

**Answer**

Hard-coded `"\\"` separators become literal characters on Linux since the only path separator is `/`, producing a single path segment with backslashes in the name rather than a hierarchy — the file ends up in the current directory rather than the intended nested path. `SpecialFolder.MyDocuments` returns `~/Documents` on Linux (if XDG directories are set) but is a desktop concept meaningless on a headless CI server, so the resulting path is both wrong and potentially inaccessible. The fix is `Path.Combine(baseDir, "reports", $"{reportId}.csv")` using the correct separator, and for server deployments the base directory should come from an environment variable or `IConfiguration` rather than a special folder — this makes the output location configurable per environment without code changes.

---

## Q90. (Scenario R) An admin API accepts a `fileName` query parameter combined with `Path.GetFullPath` to serve files. `GET /download?fileName=..\..\appsettings.Production.json` succeeds. What is the vulnerability and fix?

**Concepts**
- Path traversal attack via `..` in user-supplied filename
- `Path.GetFullPath` normalizing the traversal to a valid absolute path
- Containment check: resolved path must start with the allowed base
- `StringComparison.OrdinalIgnoreCase` on Windows, `Ordinal` on Linux

**Answer**

`Path.GetFullPath` does exactly what causes the vulnerability here — it resolves `..` segments to their canonical absolute path, so `Path.GetFullPath(Path.Combine(serveRoot, "../../appsettings.Production.json"))` returns a path outside `serveRoot` that `File.Open` happily opens. The fix is to compute the canonical resolved path with `Path.GetFullPath(candidatePath)` and then assert that it starts with `Path.GetFullPath(serveRoot) + Path.DirectorySeparatorChar` before opening — the trailing separator prevents a `serveRoot` of `/var/app/files` from matching a resolved path of `/var/app/files-private`. Using `string.StartsWith` with `StringComparison.OrdinalIgnoreCase` on Windows (case-insensitive filesystem) and `Ordinal` on Linux (case-sensitive) ensures the comparison matches how the OS actually resolves the path.

---

## Q91. (Scenario P) A worker service loads `config/settings.json` with a relative path. It works from Visual Studio but fails as a Windows Service because `CurrentDirectory` is `C:\Windows\System32`. What is the fix?

**Concepts**
- `Environment.CurrentDirectory` set by service manager, not the binary location
- `AppContext.BaseDirectory` as the stable anchor for deployed content
- Setting `CurrentDirectory` explicitly at startup as a workaround
- `IConfiguration.AddJsonFile` with explicit base path in .NET hosted services

**Answer**

When Windows Service Control Manager starts a service, it sets the current directory to `C:\Windows\System32`, so `File.Open("config/settings.json")` looks for `C:\Windows\System32\config\settings.json` rather than the service's install directory. The correct fix is to anchor the path at `AppContext.BaseDirectory`: `Path.Combine(AppContext.BaseDirectory, "config", "settings.json")` resolves to the publish directory on all hosting models. For `IConfiguration` in a .NET generic host, `hostBuilder.ConfigureAppConfiguration((ctx, cfg) => cfg.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "config/settings.json")))` achieves the same. Setting `Environment.CurrentDirectory = AppContext.BaseDirectory` at `Main` entry is a common quick fix but is a global mutation that can break libraries that also rely on current directory, so explicit absolute paths are preferable.

---

## Q92. (Scenario P) A containerized API uses `Path.GetTempFileName()` for PDF exports and never deletes them. Pods hit `No space left on device` after days. What breaks in this pattern?

**Concepts**
- `GetTempFileName` creating a new file on every call — no automatic cleanup
- Container overlay filesystem accumulating temp files until pod eviction
- `try/finally` guarantee for temp file deletion on all code paths
- `IAsyncDisposable` temp file wrapper for deterministic cleanup

**Answer**

`Path.GetTempFileName()` creates a physical zero-byte file each call and returns its path — no cleanup is automatic, and OS-level temp file purge policies that exist on desktops typically do not run in containers. Each export request leaves a PDF-sized file in `/tmp`; over days the overlay filesystem fills until the pod hits disk pressure and is evicted, often mid-request. The fix wraps the temp file path in a `try/finally` that calls `File.Delete(tempPath)` in the `finally` block, ensuring cleanup on every exit path including unhandled exceptions. For a reusable pattern, an `IAsyncDisposable` wrapper that owns the temp path and deletes in `DisposeAsync` can be used with `await using`, expressing cleanup as a resource lifetime rather than manual bookkeeping. In Kubernetes, also mount `emptyDir` volumes with a `sizeLimit` as a safety backstop.

---

## Q93. (Scenario R) A desktop feature using `SpecialFolder.Desktop` is ported to a headless Linux server and fails. What is wrong and how should server export locations be chosen?

**Concepts**
- `SpecialFolder.Desktop` mapped to home directory subfolder — may not exist on server
- Headless Linux containers potentially having no configured home directory
- Configuration-driven output path for server deployments
- `ASPNETCORE_ENVIRONMENT` or environment variables to vary output directory

**Answer**

`Environment.GetFolderPath(Environment.SpecialFolder.Desktop)` returns an empty string on headless Linux when no desktop environment is configured or when the HOME directory is not set, so any subsequent `Path.Combine` with an empty base silently resolves to a relative path, which then resolves to the current working directory — a completely different location from the intended export destination. Desktop-oriented special folders (`Desktop`, `MyDocuments`, `SendTo`) are meaningful only in interactive user sessions; server exports should go to a path read from configuration (`IConfiguration["ExportPath"]`) or an environment variable (`EXPORT_DIR`), which can be set per environment without code changes. Mounting a named Kubernetes volume at a well-known path like `/var/app/exports` is the recommended pattern since it separates the concern of "where exports go" from the application code.

---

## Q94. (Scenario M) A path helper calls `Path.Combine("ignored", "prefix", configuredRoot, appName, logFile)` where `configuredRoot` is an absolute path like `/var/log`. What surprising result occurs?

**Concepts**
- Rooted segment discarding all prior `Path.Combine` arguments
- Configuration-supplied rooted path silently bypassing hardcoded prefixes
- Security implication: user-controlled rooted path can escape intended directory
- `Path.IsPathRooted` guard before passing user-supplied values to `Combine`

**Answer**

When `configuredRoot` is `/var/log`, `Path.Combine` discards everything before it — `"ignored"` and `"prefix"` — because a rooted segment resets the accumulated path. The result is `/var/log/appName/logFile`, completely ignoring the intended prefix. This is the specified behavior of `Path.Combine` and is correct when the caller genuinely wants an absolute override, but it is a trap when the code assumes the earlier segments always appear in the result. When `configuredRoot` comes from configuration or user input, I check `Path.IsPathRooted(configuredRoot)` before the combine — if it is rooted I use it directly as the base (no prefix prepended), and if it is relative I combine it with my known safe base directory. This makes the behavior explicit rather than depending on whether the configured value happens to be absolute or relative.

---

## Q95. (Scenario D) Two services exchange Windows absolute paths (`D:\data\invoices\inv-001.pdf`) over a message queue. Service B on Linux fails to open them. What contract should replace raw absolute paths?

**Concepts**
- Absolute OS paths as non-portable identifiers
- Logical identifier (key, relative path, object-store URI) as the portable contract
- Service-local path resolution from a configured base
- Object storage URI (`s3://`, `https://blob.core.windows.net`) as the cross-platform alternative

**Answer**

Passing raw Windows absolute paths over a message queue creates an implicit tight coupling to the source machine's drive letter, directory layout, and filesystem — the path `D:\data\invoices\inv-001.pdf` is meaningless on a Linux service that has no `D:` drive. The correct contract is a logical identifier: a relative path like `invoices/2026/inv-001.pdf` that each service resolves against its own locally configured base directory (`InvoiceBasePath`), or an object-store URI (`s3://bucket/invoices/inv-001.pdf`) that any service with storage credentials can resolve. Relative paths as message payloads decouple the services from each other's filesystem layout while remaining human-readable and allowing each service to store files wherever its deployment requires. Object-store URIs are the best choice for cross-host scenarios since they remove filesystem-sharing requirements entirely.

---

## Q96. (Scenario P) A build pipeline creates deeply nested test output exceeding 260 characters. It works on Linux CI but fails on Windows with `PathTooLongException`. What explains the platform difference and what mitigations apply?

**Concepts**
- Windows MAX_PATH limit of 260 characters (legacy Win32 APIs)
- Linux POSIX path limit of 4096 characters
- `\\?\` extended-length path prefix bypassing MAX_PATH on Windows
- `<LongPathsEnabled>true</LongPathsEnabled>` opt-in in app manifest or registry

**Answer**

Windows file APIs historically enforced a MAX_PATH limit of 260 characters, which Linux POSIX APIs do not — POSIX allows up to 4096 bytes per path. This is why deeply nested output directories fail on Windows CI while working identically on Linux. The mitigations are: enable long-path support system-wide via Group Policy (`HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled = 1`) or via the app manifest `<longPathAware>true</longPathAware>`, which allows .NET on Windows 10 version 1607+ to use paths up to about 32767 characters. The code-level fix is to prefix absolute paths with `\\?\` (the extended-length prefix) when constructing `FileStream` on Windows, though `Path.GetFullPath` does not add this prefix automatically. The structural fix is to keep test output directory names short — flatten the hierarchy or shorten component names so paths stay under 200 characters, which prevents the issue across all Windows configurations without relying on the opt-in.


---

## Q89. (Scenario R) A report exporter hard-codes `"\\"` separators and uses `SpecialFolder.MyDocuments` on Linux CI. What is wrong and how do you fix it for cross-platform deployment?

_[to be reformatted in pass 2]_

---

## Q90. (Scenario R) An admin API accepts a `fileName` query parameter combined with `Path.GetFullPath` to serve files. `GET /download?fileName=..\..\appsettings.Production.json` succeeds. What is the vulnerability and fix?

_[to be reformatted in pass 2]_

---

## Q91. (Scenario P) A worker service loads `config/settings.json` with a relative path. It works from Visual Studio but fails as a Windows Service because `CurrentDirectory` is `C:\Windows\System32`. What is the fix?

_[to be reformatted in pass 2]_

---

## Q92. (Scenario P) A containerized API uses `Path.GetTempFileName()` for PDF exports and never deletes them. Pods hit `No space left on device` after days. What breaks in this pattern?

_[to be reformatted in pass 2]_

---

## Q93. (Scenario R) A desktop feature using `SpecialFolder.Desktop` is ported to a headless Linux server and fails. What is wrong and how should server export locations be chosen?

_[to be reformatted in pass 2]_

---

## Q94. (Scenario M) A path helper calls `Path.Combine("ignored", "prefix", configuredRoot, appName, logFile)` where `configuredRoot` is an absolute path like `/var/log`. What surprising result occurs?

_[to be reformatted in pass 2]_

---

## Q95. (Scenario D) Two services exchange Windows absolute paths (`D:\data\invoices\inv-001.pdf`) over a message queue. Service B on Linux fails to open them. What contract should replace raw absolute paths?

_[to be reformatted in pass 2]_

---

## Q96. (Scenario P) A build pipeline creates deeply nested test output exceeding 260 characters. It works on Linux CI but fails on Windows with `PathTooLongException`. What explains the platform difference and what mitigations apply?

_[to be reformatted in pass 2]_

---

## 05. Working with CSV and Text Files

---

## Q97. Why is there no built-in CSV parser in the BCL, and what libraries are commonly used?

**Concepts**
- CSV as an informal format with no single authoritative specification
- RFC 4180 as a widely cited but not universally followed guideline
- `CsvHelper` and `Sylvan.Data.Csv` as production-grade libraries
- `TextFieldParser` in `Microsoft.VisualBasic` as a BCL option

**Answer**

CSV is not a formally standardized format — RFC 4180 documents common conventions but many producers deviate from it (omitting trailing CRLF, using semicolons as delimiters, varying quoting rules), which makes a single correct implementation impossible to define. The BCL chose not to ship an opinionated parser, leaving the problem to libraries that can handle real-world variation. `CsvHelper` is the most widely used and handles quoting, custom delimiters, culture-aware type conversion, and class mapping out of the box. `Sylvan.Data.Csv` is a high-performance alternative that implements `IDataReader` and integrates with `SqlBulkCopy`. For simple cases, `Microsoft.VisualBasic.FileIO.TextFieldParser` is available in the BCL and handles quoted fields correctly, though its API is procedural rather than streaming.

---

## Q98. What are RFC 4180 rules for CSV fields, delimiters, and record terminators?

**Concepts**
- Comma as the field delimiter
- CRLF (`\r\n`) as the record terminator, including after the last record
- Optional header row identical in structure to data rows
- Quoting required when a field contains comma, double-quote, or CRLF

**Answer**

RFC 4180 specifies comma as the delimiter (though many producers use tab or semicolon in practice), CRLF as the record terminator including after the final record, and an optional first row of header names that follow the same quoting rules as data fields. Fields that contain a comma, a double-quote, or a CRLF must be enclosed in double-quote characters; the enclosing quotes are not part of the field value. Fields that do not contain these characters may optionally be quoted. A double-quote within a quoted field is represented as two consecutive double-quotes (`""`), not a backslash escape. Leading and trailing spaces within an unquoted field are part of the field value, which is a common source of subtle parsing differences between producers and consumers.

---

## Q99. When must a CSV field be wrapped in double quotes?

**Concepts**
- Mandatory quoting for fields containing the delimiter character
- Mandatory quoting for fields containing double-quote characters
- Mandatory quoting for fields containing embedded newlines (CRLF or LF)
- Optional quoting for all other fields

**Answer**

Per RFC 4180, quoting is mandatory when the field value contains the delimiter (comma, or whatever delimiter the file uses), a literal double-quote character, or an embedded newline. Failing to quote a field containing a comma causes the parser to split it into multiple fields, shifting all subsequent columns. Failing to quote a field containing a newline causes the parser to treat it as a record terminator, splitting a single logical row into two. Double-quote characters inside a quoted field are escaped by doubling them (`""`), so a field containing a single `"` is written as `"""..."` with the surrounding quotes. Fields that contain none of these characters may be optionally quoted or unquoted — a robust parser accepts both forms.

---

## Q100. How do you escape a literal double quote inside a quoted CSV field?

**Concepts**
- RFC 4180 doubling rule: `""` represents one literal `"`
- No backslash escaping in standard CSV
- Parser state machine distinguishing doubled-quote from closing-quote
- Misuse of `\"` causing column-count errors in strict parsers

**Answer**

In RFC 4180 CSV, the only escaping mechanism for a literal double-quote inside a quoted field is to write two consecutive double-quote characters — `""`. So a field containing the value `say "hello"` is written as `"say ""hello"""` in the CSV file. There is no backslash escape — `\"` is not part of the CSV standard and is interpreted by some parsers as a closing quote followed by a stray backslash character, which shifts columns. A correct stateful parser transitions from "inside quotes" to "maybe end of field" when it sees `"`, then: if the next character is another `"` it emits one `"` and stays inside the field; if it is the delimiter or a newline it ends the field. Using `\"` for escaping instead of `""` is a common error in hand-rolled writers that breaks any downstream RFC-compliant reader.

---

## Q101. What goes wrong if you split CSV lines on `Split(',')` without a proper parser?

**Concepts**
- Quoted fields containing commas split incorrectly
- Quoted fields containing newlines split across lines
- Double-quote escape sequences passed through literally as `""`
- Column count variance producing wrong mappings

**Answer**

`Split(',')` is a character-level split that knows nothing about quoting, so a field value like `"Smith, John"` is split into two tokens — `"Smith` and ` John"` — instead of one, shifting every subsequent column to the wrong index. This is the most common bug in hand-rolled CSV code and produces subtle data corruption that only surfaces for records with commas in text fields. Embedded newlines inside quoted fields cause `Split(',')` on a single line read from `ReadLine()` to miss the continuation, so the record is truncated. The fix is always to use a proper CSV parser (`CsvHelper`, `Sylvan.Data.Csv`, or `TextFieldParser`) rather than string splits, because the RFC 4180 state machine cannot be reproduced correctly by a regex or a `Split` call.

---

## Q102. How do you handle embedded newlines inside quoted CSV fields?

**Concepts**
- Embedded newlines making a single logical row span multiple physical lines
- `ReadLine()` breaking at the embedded newline — wrong record boundary
- Parser tracking open-quote state to span multiple physical lines
- `CsvHelper` handling multi-line records transparently

**Answer**

A CSV record with an embedded newline in a quoted field spans two or more physical lines — a `ReadLine()` loop breaks the record at the newline inside the field, treating the continuation as a new record and producing the wrong column count. The correct approach is to use a parser that maintains a state machine: when it reads an opening quote it continues reading characters — including newline bytes — until it finds an unescaped closing quote, only then ending the field. `CsvHelper` handles this correctly by default; `TextFieldParser` also handles it. Hand-rolling a multi-line-aware parser requires reading character by character with the quote state tracked across `ReadLine` boundaries, which is why I avoid it in favor of a library.

---

## Q103. What issues arise with culture-specific decimal separators in CSV numeric columns?

**Concepts**
- Culture-sensitive `ToString` and `Parse` using system locale's decimal separator
- German locale using `,` as decimal separator — conflicting with CSV delimiter
- `CultureInfo.InvariantCulture` for portable numeric formatting
- Downstream parsing requiring matching `NumberStyles` and `IFormatProvider`

**Answer**

`double.ToString()` and `decimal.ToString()` use the current culture's number format — on a German Windows machine the decimal separator is `,` (comma), so `1.5` is formatted as `1,5`. In a comma-delimited CSV file, `1,5` parses as two separate fields rather than the decimal number `1.5`, breaking every numeric column. The fix is to always format numeric values with `CultureInfo.InvariantCulture` — `value.ToString("F2", CultureInfo.InvariantCulture)` — which uses `.` as the decimal separator regardless of the machine locale. Parsing on the read side must also use `double.Parse(field, CultureInfo.InvariantCulture)` for consistency. If the CSV is intended for Excel on a German system, the separator character itself must be documented in a format contract or a `sep=;` hint line, since that user may expect semicolons as delimiters.

---

## Q104. How do you write CSV headers and ensure stable column ordering for downstream consumers?

**Concepts**
- Explicit ordered column name array rather than property reflection
- Reflection-based ordering non-deterministic across CLR versions
- Contract-driven stable ordering in a shared format specification
- `CsvHelper.Configuration.RegisterClassMap` for explicit member ordering

**Answer**

Reflection-based header generation (iterating `typeof(T).GetProperties()`) does not guarantee stable ordering across runtime versions, JIT optimizations, or code changes that add new properties — a column added in a refactor can shift all subsequent columns and silently corrupt the downstream consumer's mappings. The correct pattern is an explicit ordered column list: define `string[] Headers = { "Id", "Name", "Price", "Currency" }` as a constant in the format contract and write the header row from that array, then write each data row by accessing fields in the same order. With `CsvHelper`, `RegisterClassMap<T>` with `Map(m => m.Field).Index(0)` declarations gives explicit, stable positions. The format contract (a shared specification or interface) is the authoritative source — the header order in code must match it exactly so any consumer written against that contract reads the correct columns regardless of how the producer's model class evolves.

---

## Q105. What is the difference between `\n`, `\r\n`, and `Environment.NewLine` for text file line endings?

**Concepts**
- `\n` (LF) — Unix and Linux line ending
- `\r\n` (CRLF) — Windows line ending
- `\r` (CR) — old Mac (pre-OS X) line ending
- `Environment.NewLine` resolving to platform default at runtime

**Answer**

`\n` (line feed, 0x0A) is the standard line ending on Unix, Linux, and macOS; `\r\n` (carriage return + line feed, 0x0D 0x0A) is the Windows standard. Writing files with hard-coded `\n` on Windows is technically cross-platform for most consumers but differs from what native Windows tools expect. `Environment.NewLine` resolves to `\r\n` on Windows and `\n` on Linux, so using it matches the platform default — appropriate for files intended only for the local system. For files exchanged between systems or uploaded to git repositories, I use `\n` explicitly or configure `StreamWriter.NewLine = "\n"` to force Unix endings, since CRLF in source control and config files causes diff noise on Linux systems. RFC 4180 specifies CRLF for CSV, so CSV writers should use `\r\n` regardless of platform.

---

## Q106. How do you normalize line endings when reading files produced on Windows vs Linux?

**Concepts**
- `StreamReader.ReadLine()` stripping both `\r\n` and `\n` uniformly
- `ReadToEnd()` + `Split('\n')` leaving `\r` on Windows-produced lines
- Explicit `Replace("\r\n", "\n")` before `Split` as a defensive step
- `File.ReadAllLines` also normalizing correctly via `StreamReader`

**Answer**

`StreamReader.ReadLine()` handles all three line ending styles — `\r\n`, `\n`, and `\r` — and returns the line content without the terminator, which is the safest way to read line-by-line across platforms. When using `ReadToEnd()` combined with `Split('\n')`, Windows-produced CRLF lines leave a trailing `\r` on each entry, which is the root cause of `key\r` mismatch bugs in config parsers. The fix when `Split` is necessary is to first normalize: `content.Replace("\r\n", "\n").Replace('\r', '\n')` reduces all variants to LF before splitting. `File.ReadAllLines` uses `StreamReader` internally and normalizes correctly, so it is preferable to `ReadToEnd().Split` for line-oriented parsing.

---

## Q107. What are best practices for large CSV ingestion (streaming vs loading all rows)?

**Concepts**
- `File.ReadAllLines` materialing full file into memory — LOH risk on large files
- `File.ReadLines` / `StreamReader.ReadLine` streaming one line at a time
- Chunked batch inserts to bound transaction size and memory
- `IAsyncEnumerable<T>` for async streaming ingestion in hosted services

**Answer**

Large CSV ingestion should stream rows rather than materializing the file — `File.ReadAllLines` on a 400 MB CSV allocates a `string[]` with potentially millions of entries that pins a large LOH segment and causes GC pauses under concurrent traffic. `File.ReadLines` or a `StreamReader` loop keeps allocation proportional to one row at a time. Deserialized rows should be batched (for example 500 at a time) and written to the database in bulk using `SqlBulkCopy` or `DbContext.BulkInsert`, rather than inserting each row in a separate transaction, which also prevents the open database transaction growing unboundedly large. For async ingest pipelines in hosted services, I expose the file rows as `IAsyncEnumerable<T>` from a `CsvHelper.GetRecordsAsync<T>` call, which lets the consumer `await foreach` and apply backpressure through a `Channel<T>` between the reader and the writer.

---

## Q108. How do you validate CSV row shape (column count) before deserializing to objects?

**Concepts**
- Column count check against the header row count
- Early rejection before expensive deserialization or DB insert
- Line number tracking for actionable error messages
- `CsvHelper` `BadDataException` and `HeaderValidationException`

**Answer**

I read the header row first, count the expected columns, then on each data row check that the actual column count matches before attempting type conversion or object construction — a mismatch means the row was incorrectly parsed (typically due to an unquoted comma) and deserializing it would write wrong values to wrong fields. The check is `if (row.Length != expectedColumnCount) throw new CsvFormatException(lineNumber, row.Length, expectedColumnCount)` so the error message names the exact line and counts, making it actionable for operations teams. With `CsvHelper`, configuring `MissingFieldFound` and `BadDataFound` delegates on `CsvConfiguration` lets me log the offending raw line and skip or reject it without aborting the entire import. A tolerance policy (skip bad rows vs abort) is a business decision that should be explicit in the import specification.

---

## Q109. When should you use fixed-width text formats instead of CSV?

**Concepts**
- Fixed-width format as delimiter-free, positional field layout
- Suitable for legacy mainframe integrations and regulatory submissions
- Avoids delimiter-in-value quoting complexity
- Character counting discipline vs parser flexibility of CSV

**Answer**

Fixed-width formats are appropriate when the consuming system is a mainframe, legacy COBOL program, or regulatory authority that specifies exact column positions in a file layout — these systems predate CSV conventions and expect each field at a known byte offset with a known length. The format avoids all quoting and delimiter ambiguity since fields are simply padded to their specified width, but it requires that field values never exceed their column width and that any truncation is acceptable. I use fixed-width when the file specification says "field Name is bytes 1–30, left-justified, space-padded" rather than "comma-separated"; for modern integrations between .NET services I prefer JSON, CSV, or Parquet depending on whether the data is document-oriented, tabular, or analytical.

---

## Q110. How do you properly dispose file resources across layered readers (`FileStream` → `StreamReader`)?

**Concepts**
- `StreamReader` disposing the underlying `FileStream` by default
- `leaveOpen: true` when the base stream has a longer lifetime
- LIFO disposal order from nested `using` statements
- `await using` for `IAsyncDisposable` writers and streams

**Answer**

When a `StreamReader` is constructed with `leaveOpen: false` (the default), disposing the reader also disposes the underlying `FileStream` — so one `using` block covers both. Nested `using` statements dispose in LIFO order, which matches the correct teardown sequence: the inner reader is disposed before the outer stream, flushing any buffered state before the handle is released. When a `FileStream` is shared between multiple wrappers (a `StreamReader` and a `BinaryReader` on the same handle, for example), I pass `leaveOpen: true` to each wrapper and dispose the raw `FileStream` explicitly after all wrappers are done. For async code, `StreamWriter` is `IAsyncDisposable` and must be used with `await using` so the final flush is awaited rather than blocking.

---

## Q111. What logging and rotation patterns apply when appending to text log files over time?

**Concepts**
- Single long-lived `StreamWriter` with `AutoFlush` for append performance
- File rotation by size, date, or sequence number
- `FileShare.Read` on the writer so log readers and shippers can coexist
- Structured logging sinks (Serilog, NLog) implementing rotation correctly

**Answer**

A long-lived append writer opened once with `FileMode.Append`, `FileShare.Read`, and `AutoFlush = true` is efficient for high-throughput logging since it avoids repeated open/close overhead and flushes each entry immediately to the OS buffer so tailing tools see entries in near real time. Rotation is triggered by checking `stream.Length` after each write (size rotation) or comparing `DateTime.UtcNow.Date` with the log file's date (daily rotation), then closing the current stream, renaming the file to include a timestamp, and opening a new stream. In production, I use Serilog or NLog's rolling file sink rather than implementing rotation manually — they handle concurrent writers, rotation-on-lock, and gzip compression of rotated files correctly across platforms, including the edge case where the file is still open in another process when rotation fires. The writer must always open with `FileShare.Read` so log shippers (Filebeat, Fluent Bit) can read the current file without a sharing violation.

---

## Q112. `ReadAllLines` vs `ReadLines` — trade-offs in memory and file lock duration.

**Concepts**
- `ReadAllLines` materializing `string[]` in memory, handle released before return
- `ReadLines` streaming lazily, handle open during enumeration
- Memory: `ReadAllLines` proportional to file size; `ReadLines` proportional to one line
- LINQ with `ReadLines` more memory-efficient for large files

**Answer**

`ReadAllLines` opens the file, reads every line into a `string[]`, closes the handle, and returns the array — the file is unlocked before the caller processes a single line, so concurrent writers can open it immediately after the call returns. The trade-off is that the entire file lives in memory simultaneously, which is fine for small files but causes LOH pressure on large ones. `ReadLines` returns a lazy enumerator that holds the file handle open until enumeration completes or the enumerator is disposed, meaning the file is locked during the full `foreach` loop. For small files where the full content must be processed, `ReadAllLines` is simpler and its early handle release is an advantage; for large files where only a subset of lines is needed (LINQ `Where`/`Take`), `ReadLines` is more efficient because iteration stops early and the unread portion is never loaded, though the handle lifetime must be managed deliberately.

---

## Q113. Undisposed streams lock files on Windows — describe the mechanism and correct pattern.

**Concepts**
- OS file handle held by undisposed `FileStream` until GC finalizer
- GC finalizer non-deterministic — handle released at GC time, not at scope exit
- Windows file lock blocking concurrent open, delete, or rename operations
- `using` / `await using` generating `try/finally` for deterministic disposal

**Answer**

When a `FileStream` is not disposed, the .NET runtime holds the underlying Win32 file handle open. The `FileStream` finalizer will eventually close it when the GC collects the object, but GC is non-deterministic — the handle might stay open for seconds or minutes after the stream is logically done, blocking any other process that tries to open the file exclusively, rename it, or delete it. This causes intermittent `IOException: process cannot access the file` errors that are hard to reproduce and appear unrelated to the code that holds the handle. The correct pattern is `using var fs = new FileStream(...)` which the compiler transforms into a `try/finally` block that calls `fs.Dispose()` on every exit path including exceptions, releasing the handle immediately at scope exit. For `IAsyncDisposable` streams (like async `FileStream` wrappers), `await using` must be used to avoid blocking on disposal.

---

## Q114. `FileShare` defaults to exclusive access — what does that mean for concurrent readers?

**Concepts**
- `FileShare.None` as the default on most `FileStream` constructor overloads
- All other processes blocked from opening the file during the exclusive hold
- Read-only concurrent access permitted by `FileShare.Read` on the writer
- `FileShare` as a declaration of tolerance, not a capability grant

**Answer**

The `FileStream` constructor overloads that accept only a path and `FileMode` default to `FileShare.None`, which grants exclusive access — no other process can open the file for reading, writing, or deletion while the handle is open. This is the correct default for write operations where intermediate state must never be seen, but it silently blocks log shippers, backup agents, and monitoring tools that try to read the file concurrently. `FileShare.Read` on the writer allows concurrent readers while still preventing other writers from opening the file, which is the right choice for log files and status files. The key mental model is that `FileShare` is a declaration of what concurrent opens the current opener is willing to tolerate — setting `FileShare.Read` does not grant readers any new capability, it simply does not block them when they request read access.

---

## Q115. Hard-coded path separators break cross-platform — why and what to use instead.

**Concepts**
- `\` as a Windows separator — not recognized as separator on Linux
- `/` universally recognized on all .NET platforms including Windows
- `Path.Combine` and `Path.DirectorySeparatorChar` for portable joining
- String interpolation with separators as the most common error location

**Answer**

On Windows the directory separator is `\`, on Linux and macOS it is `/`. Code like `var path = baseDir + "\\" + "reports" + "\\" + fileName` produces a path with backslash separators that are literal characters on Linux — the OS treats the whole string as a single path segment with backslashes in the name rather than a hierarchy. The portable fix is `Path.Combine(baseDir, "reports", fileName)`, which uses `Path.DirectorySeparatorChar` internally. Even forward slashes in string literals like `"reports/file.csv"` are cross-platform on .NET (Windows APIs accept `/`), so using `/` in string literals is acceptable as a pragmatic cross-platform default, but `Path.Combine` is still preferable because it handles edge cases like double separators and rooted segments correctly.

---

## Q116. Relative paths depend on `CurrentDirectory` — why does this cause production failures?

**Concepts**
- Relative path resolved against `Environment.CurrentDirectory` at time of open
- Service manager starting process with `CurrentDirectory` set to system directory
- `AppContext.BaseDirectory` as the stable anchor for app-relative files
- Late-binding failure not caught in unit tests or local development

**Answer**

A relative path like `"config/settings.json"` is resolved against `Environment.CurrentDirectory` at the moment the file is opened, not against where the binary lives. In local development `CurrentDirectory` is typically the project directory so the file is found. When deployed as a Windows Service, IIS worker process, or systemd unit, the service manager sets `CurrentDirectory` to a system directory (often `C:\Windows\System32` or `/`), so the relative path resolves to a completely different location and the file is not found — but the error only appears in production when the deployment environment differs from development. Anchoring all app-relative file paths to `AppContext.BaseDirectory` at startup eliminates the dependency on `CurrentDirectory` and makes the resolution deterministic regardless of how the process was launched.

---

## Q117. `Path.Combine` with an absolute second segment discards earlier parts — explain the behavior.

**Concepts**
- Absolute path segment as a complete restart, not a continuation
- All prior `Path.Combine` arguments discarded silently
- Security implication when second segment comes from configuration or user input
- `Path.IsPathRooted` guard before combining user-supplied segments

**Answer**

`Path.Combine` is specified to behave as a chained URI-like join: if any segment is absolute (rooted), it resets the accumulated path to that segment and discards everything before it. So `Path.Combine("/var/app", "/etc/passwd")` returns `"/etc/passwd"` — the first segment is completely discarded. This is intentional and correct when the caller wants the second segment to be an absolute override, but it is a silent bug when configuration or user input supplies a value that happens to be absolute. I always call `Path.IsPathRooted(segment)` before passing a configuration value to `Path.Combine`; if it is rooted I decide explicitly whether to use it as-is or reject it as invalid input rather than letting the discard happen silently.

---

## Q118. Encoding mismatch silently corrupts text — how does it happen and how is it prevented?

**Concepts**
- Writer and reader using different encodings — bytes interpreted under wrong code page
- `Encoding.Default` resolving differently per OS
- UTF-8 without BOM as the portable default
- No exception from mismatched encoding — silent character substitution or question marks

**Answer**

Encoding mismatch produces no exception — the bytes are valid under both encodings, just interpreted as different characters. A writer using Windows-1252 writes the byte `0x92` for a right single quotation mark; a reader using UTF-8 decodes `0x92` as a C1 control character (or replacement character) since `0x92` is not a valid single-byte UTF-8 sequence, producing a garbled character in the output. The mismatch goes undetected until someone reads the file on a different machine or passes it to an external tool. Prevention requires explicit encoding on both sides: pass the same `Encoding` instance to both the `StreamWriter` and `StreamReader` constructors rather than relying on defaults. `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` is the portable no-BOM UTF-8 encoding that works correctly on all platforms and does not emit a BOM that confuses consumers.

---

## Q119. Seeking past EOF then writing extends the file with undefined gap bytes — explain the behavior.

**Concepts**
- Sparse file behavior: gap bytes are not necessarily written to disk
- `\0` returned when reading the gap on most filesystems
- Filesystem-dependent: NTFS sparse files vs ext4 hole punching
- Intentional use: pre-allocating file space; unintentional use: corrupt format

**Answer**

Seeking past the current end of file with `Seek(pastEofOffset, SeekOrigin.Begin)` and then writing creates a gap in the file — the bytes between the original EOF and the new write position. On most filesystems (NTFS with sparse support, ext4 with `FALLOC_FL_KEEP_SIZE`) this gap is represented as a "hole" that is not physically allocated on disk; reading back the gap returns null bytes (`\0`). On filesystems without sparse support, the gap bytes are physically written as zeros. An application that reads back the gap expecting valid data receives null bytes without an exception, which silently corrupts deserialization if the format is not expecting nulls. In binary record formats where each record's position is computed as `index * recordSize`, seeking to a new record position before writing is intentional and correct; doing so accidentally when a seek offset computation has an error is a subtle source of format corruption.

---

## Q120. CSV `Split(',')` breaks on quoted commas — explain what goes wrong with a concrete example.

**Concepts**
- `Split(',')` as a delimiter-unaware character split
- Quoted field containing comma parsed as two tokens
- Column-shift corrupting all fields after the split point
- No fix possible with `Split` — requires stateful CSV parser

**Answer**

Consider a CSV row: `001,"Acme, Inc",19.99`. Correct parsing yields three fields: `001`, `Acme, Inc`, and `19.99`. `"001,\"Acme, Inc\",19.99".Split(',')` yields four tokens: `001`, `"Acme`, ` Inc"`, and `19.99` — the quoted comma split the vendor name into two tokens, shifted the price into column 4, and introduced a stray quote character at the start of column 2. Every downstream column index is wrong after the split point, so deserialized objects have price in the description field, description in the price field, and so on. There is no way to fix `Split(',')` for quoted fields — the parsing must be stateful, tracking whether the current character is inside a quoted field. Use `CsvHelper`, `TextFieldParser`, or `Sylvan.Data.Csv` instead.

---

## Q121. Double-quote escaping in CSV is `""` not `\"` — why does getting this wrong shift columns?

**Concepts**
- RFC 4180 specifying `""` as the only double-quote escape
- `\"` interpreted by RFC parsers as closing quote followed by stray backslash
- Premature end-of-field causing column count to increase
- Writer and reader escaping contract must match

**Answer**

In RFC 4180 CSV, the only way to embed a double-quote in a quoted field is to double it: `""`. A writer that uses `\"` instead produces `"say \"hi\""`, which an RFC parser reads as: opening `"`, content `say `, closing `"` (at the `\` boundary), then two literal characters `h` and `i` followed by an extra `"` — the field ends prematurely and the remainder is parsed as a new partial field, increasing the column count by one for that row. This shifts all subsequent fields right and causes column-count validation failures or wrong data in mapped objects. The correct writer produces `"say ""hi"""` for the value `say "hi"`. When writing CSV without a library, each field value must be processed with `field.Replace("\"", "\"\"")` before wrapping in quotes.

---

## Q122. (Scenario R) A partner feed import uses `Split(',')` and mis-maps vendor names when a field contains `"Acme, Inc"`. What fails and what is the fix?

**Concepts**
- `Split(',')` breaking quoted fields at embedded commas
- Column index off-by-one (or more) after the split point
- `CsvHelper` or `TextFieldParser` as the correct replacement
- Regression test with a fixture file containing quoted commas

**Answer**

`Split(',')` splits `001,"Acme, Inc",19.99` into four tokens instead of three, so the price index becomes 3 instead of 2 — the deserialized object gets `" Inc"` as the vendor name and `19.99` as some other field, and the price field gets the default value or a parse error. The data corruption is silent on rows without quoted commas and only surfaces on rows where the vendor name contains one, which may be rare enough to escape notice in testing. The fix is to replace `Split(',')` with `CsvHelper.GetRecords<T>()` or `TextFieldParser` which maintain quoting state. After fixing, add a regression test that reads a fixture CSV file with at least one row containing a quoted comma, a quoted newline, and a double-quoted value so future changes cannot regress the parser.

---

## Q123. (Scenario R) An export on a German Windows server formats decimal numbers with `ToString("F2")` and the US warehouse tool splits one price column into two. What breaks across environments?

**Concepts**
- `ToString("F2")` using thread's current culture for decimal separator
- German locale formatting `1.50` as `1,50` — comma treated as CSV delimiter
- `CultureInfo.InvariantCulture` for machine-to-machine numeric formatting
- US consumer's parser splitting `1,50` into `1` and `50`

**Answer**

`ToString("F2")` on a German-locale machine produces `1,50` for the decimal value 1.50, because the German `NumberFormatInfo.NumberDecimalSeparator` is `,`. In a comma-delimited CSV file this is indistinguishable from a field separator, so the US warehouse parser splits the price column into two tokens and shifts every subsequent column. The fix is `value.ToString("F2", CultureInfo.InvariantCulture)` which uses `.` as the decimal separator regardless of the server's locale. The same invariant culture must be used when parsing on the read side: `decimal.Parse(field, CultureInfo.InvariantCulture)`. If the format contract requires locale-specific formatting (for a human-readable report), the numeric columns must also be quoted so the comma is inside a quoted field, though this is a fragile design that is better avoided by using invariant formatting.

---

## Q124. (Scenario R) A nightly import job loads a 400 MB ERP CSV with `File.ReadAllLines`. What production problems appear at scale and what replaces it?

**Concepts**
- `string[]` from `ReadAllLines` allocating entire file on the LOH
- GC pressure from concurrent imports materializing multiple large arrays
- `File.ReadLines` or `CsvHelper` streaming row by row
- Chunked batch database writes to bound per-batch memory and transaction size

**Answer**

`File.ReadAllLines` materializes the entire 400 MB CSV as a `string[]` in managed memory before the first row is processed — under concurrent imports, several such arrays coexist on the LOH simultaneously, triggering frequent Gen 2 GC pauses and potentially `OutOfMemoryException` when memory pressure spikes. The fix is to stream with `CsvHelper.GetRecordsAsync<T>()` or a `File.ReadLines` loop so only one row is in memory at a time. Rows should be inserted in chunks — accumulate 500 records, execute a batch insert via `SqlBulkCopy` or EF Core `BulkInsert`, clear the batch list, and continue — so no single transaction spans the full 400 MB of data. This pattern keeps per-request memory at roughly one batch size regardless of file length and keeps database transaction durations short.

---

## Q125. (Scenario P) A `FileSystemWatcher` drop-folder service imports CSV as soon as a file appears. Operators see random column-count errors and duplicate SKU rows. What race conditions exist and how do you harden the pipeline?

**Concepts**
- `Created` event firing while producer still writing — file read mid-write
- `Changed` event firing multiple times per file — duplicate processing
- `FileShare.None` on the writing side causing read failure
- Retry-with-backoff, file-completion detection, and idempotency key as fixes

**Answer**

`FileSystemWatcher` fires the `Created` or `Changed` event as soon as the OS reports activity, which often precedes the producer finishing its write — the importer opens a file that is still being populated and reads a truncated CSV, causing column-count errors. The same file can also fire multiple `Changed` events (one per flush), causing duplicate imports. Two hardening steps address the race: first, retry the open with `FileShare.Read` in a backoff loop — if the open fails with a sharing violation, the producer still holds the file exclusively, so wait and retry; if the column count is wrong on the first complete read, the file may be partially written. Second, record each processed file by a content hash or filename + arrival timestamp in a tracking table (an idempotency key) so re-processing the same file after a failure does not produce duplicate rows — only import rows whose idempotency key has not been committed.

---

## Q126. (Scenario P) Re-running the same inbound CSV file after a network blip doubles inventory counts. A path-only guard does not prevent it after a mid-batch DB failure. How do you make the import idempotent with rollback?

**Concepts**
- Path-only guard failing after partial DB write — same path re-processes remainder
- Content hash or file fingerprint as the durable idempotency key
- Transactional import log recording completion, not just start
- Two-phase commit: all rows inserted + idempotency record in one transaction

**Answer**

A path-only guard records that processing started but not that it completed — after a mid-batch DB failure the guard is absent (it was never committed) and re-running imports the remaining rows on top of the partially-committed ones, doubling counts for the rows that succeeded before the failure. The correct pattern is a two-phase transactional import: open a single database transaction, insert all rows, insert a record in an `ImportLog` table keyed by the file's SHA-256 hash (computed before reading rows), and commit the transaction — either all rows and the idempotency record commit together or none of them do. On re-run, check the `ImportLog` first; if the hash already exists, skip the import entirely. The hash rather than the filename is the key because the same file content might be delivered under different filenames, and the same filename might be reused for a different file.

---

## Q127. (Scenario D) One engineer proposes `CsvHelper`; another wants to extend a hand-rolled `SplitQuotedLine`. When do you reach for each for a long-lived warehouse integration?

**Concepts**
- `CsvHelper` covering RFC 4180 edge cases, type conversion, and class mapping
- Hand-rolled parser accumulated tech debt from each new edge case
- Maintenance burden comparison over a multi-year integration lifetime
- `CsvHelper` configurability for non-standard formats (custom delimiter, quote char)

**Answer**

I reach for `CsvHelper` for any integration expected to run for more than a few months because it handles the full RFC 4180 surface — embedded newlines, doubled-quote escapes, custom delimiters, multi-character quotes, BOM detection, culture-aware type conversion, and attribute-driven class mapping — and the maintenance is borne by the library's maintainers rather than the team. A hand-rolled `SplitQuotedLine` starts simple but accumulates `if` branches for each new edge case discovered in production: a vendor starts including newlines in description fields, another switches to semicolons, a third wraps all fields in quotes. After a year, the hand-rolled parser is harder to reason about than `CsvHelper`'s configuration API and has likely reimplemented most of it incorrectly. I accept `CsvHelper` as a dependency whenever the file format is CSV-shaped; the only exception is an extremely performance-sensitive path where `Sylvan.Data.Csv` (which implements `IDataReader` and avoids string allocation per field) is a better fit.

---

## Q128. (Scenario R) Two import workers fire-and-forget on the same nightly file. A new feed also omits the header row. What fails under concurrency and header drift, and how do you fix both?

**Concepts**
- Concurrent workers processing the same file — duplicate rows without deduplication
- `headerRow: false` configuration in `CsvHelper` for headerless feeds
- Column-index-based mapping as a fragile but necessary fallback without headers
- Distributed lock (DB advisory lock, Redis SETNX) preventing concurrent same-file imports

**Answer**

Two workers processing the same file without coordination insert every row twice — there is no sharing violation because both open with read access, and no idempotency guard because neither checks whether the other is already processing the file. The fix is a distributed lock keyed on the file's canonical identifier (path or hash): worker A acquires the lock, checks the `ImportLog`, imports, commits, releases; worker B acquires the lock after A releases it, finds the record in `ImportLog`, and exits without re-importing. For the headerless feed, `CsvHelper` must be configured with `HasHeaderRecord = false` and the class map must use `.Index(n)` instead of `.Name("colName")` to bind columns by position — this is fragile because any column reordering in the feed breaks the mapping silently, so I request that the partner add a header row and document the column order in the integration contract as a prerequisite for the integration being considered complete.
