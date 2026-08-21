# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/02. StreamReader & StreamWriter`

---

#### Q1. (R) A nightly audit job throws on bad rows and operators report the log file stays locked until the worker restarts. Review this helper:

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");

    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected — fix upstream feed.");

    writer.Dispose();
}
```

What keeps the file locked, and how do you fix it without losing the rejected row on disk?

**Answer:** When validation throws, `Dispose()` never runs, so the `StreamWriter` keeps the underlying file handle open — on Windows the log stays locked until GC finalizes the writer. Wrap the writer in `using` (or `try/finally`) so the handle is released even on the exception path; the invalid row is already on disk because `WriteLine` ran before the throw.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource lifetime | No `using` / `finally`; `Dispose()` only on happy path | File handle leak; "file in use" on next append |
| Ordering | Validate after write | Rejected rows still persisted — may be intended, but callers must know |
| Platform | Undisposed `StreamWriter` on Windows | Lock persists until process exit or finalizer — common ops incident |

**Fix (priority order):**

1. Use `using (var writer = new StreamWriter(logPath, append: true)) { … }` so dispose runs on all exit paths.
2. If invalid rows must not be written, validate **before** `WriteLine`, or write to a staging file and commit on success.
3. For long-lived services, prefer `await using StreamWriter` with async writes if the call chain is async end-to-end.
4. Monitor for handle leaks — repeated failures should not require worker restart to unlock the log.

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected — fix upstream feed.");

    using StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");
}
```

**Production takeaway:** Karat pairs exception flow with I/O cleanup — `StreamWriter` is not magic; undisposed writers are production file locks. See **Program.cs** Section 6 — `using` / `Dispose` and QUICK REFERENCE — "Forgetting using / Dispose → file locked until GC."

---

#### Q2. (R) A CSV export looks correct on the developer's Windows machine but the first column header fails validation after deploy to Linux containers. Review write vs read:

```csharp
// Export service (built and tested on Windows)
using (var writer = new StreamWriter(exportPath, append: false, Encoding.Default))
{
    writer.WriteLine("id,name,amount");
    writer.WriteLine("1,Alpha,42.50");
}

// Import service (Linux container — default StreamReader ctor)
using (var reader = new StreamReader(importPath))
{
    string? header = reader.ReadLine();
    if (header != "id,name,amount")
        throw new InvalidDataException($"Unexpected header: '{header}'");
}
```

What fails in production, and how do you make the round-trip deterministic across OS boundaries?

**Answer:** `Encoding.Default` is the **system code page** — Windows-1252 on Windows, often UTF-8 on modern Linux — so bytes on disk differ by environment, and the reader's default detection may decode the same bytes differently than the writer encoded them. Pin both sides to explicit `Encoding.UTF8` (typically `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` for no BOM) and compare headers after `ReadLine()`, which already returns decoded characters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encoding | `Encoding.Default` on write | Different byte sequences per OS — mojibake or subtle header mismatch |
| Encoding | Implicit reader encoding on read | Guessing wrong code page — first line may include stray BOM or wrong chars |
| Contract | String equality on header | Fails even when visually "correct" in a GUI editor |

**Fix (priority order):**

1. Replace `Encoding.Default` with explicit UTF-8 on **both** writer and reader constructors.
2. Document encoding in the file format contract; reject files whose BOM/bytes do not match.
3. For CSV consumed by Excel on Windows, decide deliberately on UTF-8 BOM vs no BOM — do not rely on defaults.
4. Add an integration test that round-trips on Linux CI, not only on the developer's Windows box.

```csharp
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

using (var writer = new StreamWriter(exportPath, append: false, utf8))
    writer.WriteLine("id,name,amount");

using (var reader = new StreamReader(importPath, utf8))
{
    string? header = reader.ReadLine();
    // ...
}
```

**Production takeaway:** "Works on my machine" for text files is almost always an encoding default mismatch — Karat expects you to name `UTF8Encoding` and match reader/writer. See **Program.cs** Section 4 — pass the same `Encoding` to matching ctors.

---

#### Q3. (R) A support dashboard calls this to show the tail of a customer log. Under load the worker process recycles with `OutOfMemoryException`. Review:

```csharp
public string LoadCustomerLogForSupport(string logPath)
{
    if (!File.Exists(logPath))
        return string.Empty;

    using StreamReader reader = new StreamReader(logPath);
    return reader.ReadToEnd();
}
```

Customer logs can exceed 10 GB. What is wrong, and what pattern replaces `ReadToEnd` for this use case?

**Answer:** `ReadToEnd()` allocates a single `string` for the entire remaining file — with 10 GB logs that forces a multi-gigabyte LOH allocation and typically terminates the process with `OutOfMemoryException`. Stream line-by-line with `ReadLine()` or `ReadLineAsync()`, seek to a tail window with `FileStream` + bounded `ReadBlock`, or use external tail tools — never materialize the whole file for a "show last lines" feature.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | `ReadToEnd()` on multi-GB file | `OutOfMemoryException`; worker recycle under concurrent support requests |
| API misuse | Support "tail" implemented as full load | Latency and memory scale with file size, not UI need |
| Scalability | Sync read of entire blob | Thread blocked for duration of huge I/O |

**Fix (priority order):**

1. For tail UI: open `FileStream` with `FileShare.ReadWrite`, seek near EOF, read last N KB/chars, split lines locally.
2. For scanning: `while ((line = await reader.ReadLineAsync(ct)) != null)` — bounded memory regardless of file size.
3. Cap response size returned to the dashboard (e.g., last 500 lines or 256 KB).
4. Move huge log analytics to indexed storage — files on disk are not a query engine.

```csharp
public async Task<IReadOnlyList<string>> ReadLastLinesAsync(string logPath, int maxLines, CancellationToken ct)
{
    var lines = new Queue<string>(maxLines);
    await using StreamReader reader = new StreamReader(logPath);
    while (await reader.ReadLineAsync(ct) is { } line)
    {
        if (lines.Count == maxLines) lines.Dequeue();
        lines.Enqueue(line);
    }
    return lines.ToArray();
}
```

**Production takeaway:** `ReadToEnd()` is for small files only — Karat uses log scale to test whether you know **Program.cs** Section 3 (`ReadLine` / `ReadBlock`) vs Section 7 (`File.ReadAllText` trap). Same mistake as `File.ReadAllText` on huge files.

---

#### Q4. (R) A long-running export writes a status file so another process can poll completion. Operators see `IN_PROGRESS` forever after a crash mid-run. Review:

```csharp
string statusPath = Path.Combine(outputDir, "export.status");
StreamWriter writer = new StreamWriter(statusPath, append: false);
writer.WriteLine("IN_PROGRESS");

RunHeavyExport(); // may take 20+ minutes; process sometimes killed by OOM killer

writer.WriteLine("COMPLETE");
writer.Dispose();

// Poller (separate process):
using StreamReader poller = new StreamReader(statusPath);
string lastLine = poller.ReadToEnd().TrimEnd().Split('\n').Last();
bool done = lastLine == "COMPLETE";
```

What causes false "stuck" exports and incomplete status files, and how do you harden write + detection?

**Answer:** `StreamWriter` buffers output — `IN_PROGRESS` may not hit disk until `Flush()` or `Dispose`, so a poller can see an empty or stale file early. If the process dies mid-export, `COMPLETE` is never written and the poller correctly sees stuck state, but the writer also lacks atomic replace semantics — partial flushes can leave truncated files. Use `AutoFlush` or explicit `Flush()` after status transitions, write-temp-then-`File.Move` for atomic status, and treat absence of `COMPLETE` plus process exit as failure with timeout alerting.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Buffering | No `Flush` / `AutoFlush` after `IN_PROGRESS` | Poller reads empty or old content; false negatives |
| Durability | Single file overwritten in place | Crash mid-write → truncated or blank status file |
| Detection | `ReadToEnd().Split('\n').Last()` on in-progress write | May read partial buffer; race with writer |
| Lifecycle | No `using` on writer if exception in `RunHeavyExport` | Handle leak + final status never written |

**Fix (priority order):**

1. Enable `writer.AutoFlush = true` or call `Flush()` immediately after each status line.
2. Write status to a temp file and atomically replace: `File.WriteAllText(temp, status); File.Move(temp, statusPath, overwrite: true);` — or use `StreamWriter` on temp then move.
3. Poller: check file length stability, last-write time, and explicit `FAILED`/`COMPLETE` tokens; add SLA timeout.
4. Wrap writer in `using` and set `FAILED` in `catch`/`finally` when export aborts.

```csharp
using StreamWriter writer = new StreamWriter(statusPath, append: false) { AutoFlush = true };
writer.WriteLine("IN_PROGRESS");
try
{
    RunHeavyExport();
    writer.WriteLine("COMPLETE");
}
catch
{
    writer.WriteLine("FAILED");
    throw;
}
```

**Production takeaway:** Karat stacks buffering + crash recovery — operators care about **observable** state on disk, not in-process buffers. See **Program.cs** Section 6 — `Flush`, `AutoFlush`, and dispose flushes remaining buffer.

---

#### Q5. (R) A log tailer and a log writer run in the same app. The tailer intermittently throws `IOException: The process cannot access the file because it is being used by another process`. Review:

```csharp
public IEnumerable<string> TailLines(string logPath)
{
    using FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read);
    using StreamReader reader = new StreamReader(fs);

    while (!reader.EndOfStream)
    {
        string? line = reader.ReadLine();
        if (line != null)
            yield return line;
    }
}

// Elsewhere, on another thread:
using StreamWriter writer = new StreamWriter(logPath, append: true);
writer.WriteLine($"{DateTime.UtcNow:o} INFO  heartbeat");
```

What sharing rule is missing, and why does `StreamReader`/`StreamWriter` path constructors hide it?

**Answer:** Opening `FileStream` with default `FileShare.Read` grants exclusive write access — a concurrent `StreamWriter` on the same path cannot open for append. Open the tailer's stream with `FileShare.ReadWrite` (and usually `FileMode.Open`, `FileAccess.Read`) so writers can append while you read. Path-based `StreamReader`/`StreamWriter` ctors create their own `FileStream` with sharing defaults you do not see — for tail-follow scenarios, construct `FileStream` explicitly, then wrap with `leaveOpen: true`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| File sharing | Default `FileShare.Read` on read stream | `IOException` when appender opens same log |
| API visibility | `new StreamReader(path)` hides share flags | Developers miss sharing until production concurrency |
| Iterator | `yield return` holds stream open for enumeration lifetime | Writer blocked for entire foreach duration |

**Fix (priority order):**

1. Tailer: `new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)`.
2. Writer: `new StreamWriter(new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite), appendEncoding) { AutoFlush = true }` or equivalent append pattern.
3. For live tail, re-open or seek-from-end patterns with periodic reopen on `IOException` — logs rotate.
4. Prefer structured logging sinks (Serilog file sink with shared flag) instead of hand-rolled tail+append.

```csharp
using FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
using StreamReader reader = new StreamReader(fs, leaveOpen: false);
```

**Production takeaway:** Text wrappers do not remove OS file-lock rules — Karat tests whether you know when to bypass path ctors and configure `FileShare`. Preview **Program.cs** Section 2d / 3f — `FileStream` then `StreamReader`/`StreamWriter` chain.

---

#### Q6. (P) An ASP.NET Core hosted service ingests a growing feed file every few seconds. A developer keeps sync I/O "because the file is local":

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using StreamReader reader = new StreamReader(_feedPath);

    while (!stoppingToken.IsCancellationRequested)
    {
        string? line = reader.ReadLine(); // blocks thread pool thread
        if (line == null)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            continue;
        }

        await _processor.HandleLineAsync(line, stoppingToken);
    }
}
```

What breaks under hosting pressure, and what is the production-grade read loop?

**Answer:** `ReadLine()` is synchronous — each call blocks a thread pool thread while waiting on disk I/O, which defeats the async hosting model and can contribute to thread-pool starvation when many background services do the same. Use `ReadLineAsync(stoppingToken)` (or `WaitToReadAsync` patterns on pipes) inside an async loop, combine with `FileShare.ReadWrite` if producers append, and reopen or track position when reaching EOF on a growing file.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | Sync `ReadLine()` in async host | Thread pool blocked during I/O waits |
| EOF handling | `null` line then `Delay` on static reader | Misses new lines appending at EOF unless reposition/reopen |
| Cancellation | Sync read ignores `stoppingToken` during block | Slow shutdown under load |

**Fix (priority order):**

1. Replace with `await reader.ReadLineAsync(stoppingToken)` in the loop.
2. When at EOF on a growing feed, flush writer side, optionally reopen file or track `_lastPosition` with `FileStream.Position`.
3. Mark the hosted service async end-to-end; avoid `.Result` on any related tasks.
4. Add metrics for lag (lines behind) and backoff when file is temporarily locked.

```csharp
await using StreamReader reader = new StreamReader(_feedPath);

while (!stoppingToken.IsCancellationRequested)
{
    string? line = await reader.ReadLineAsync(stoppingToken);
    if (line is null)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        continue;
    }

    await _processor.HandleLineAsync(line, stoppingToken);
}
```

**Production takeaway:** Local disk does not make I/O free — sync-over-async in hosted services is the same Karat trap as `.Result` in controllers. Prefer async stream APIs even for file reads.

---

#### Q7. (R) A tool rewrites the first line of a config file in place, then reads the remainder. After a refactor it throws `ObjectDisposedException`. Review:

```csharp
public void PatchConfigHeader(string path, string newHeader)
{
    using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
    using StreamWriter writer = new StreamWriter(fs, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
    using StreamReader reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);

    writer.WriteLine(newHeader);
    writer.Flush();

    fs.Seek(0, SeekOrigin.Begin);
    string remainder = reader.ReadToEnd();
}
```

Which dispose/ownership choices are wrong, and what is the correct pattern when wrapping the same `FileStream`?

**Answer:** `StreamReader` is constructed with `leaveOpen: false` (the default), so disposing the reader **closes the shared `FileStream`** when the `using` block ends — before `Seek`/`ReadToEnd` if ordering were wrong, and any later use throws `ObjectDisposedException`. Both reader and writer must use `leaveOpen: true` when sharing one stream; dispose order should flush the writer, then dispose reader, then writer, then the stream — or avoid dual wrappers on one stream and read/write in separate phases with explicit positioning.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Ownership | `StreamReader(..., leaveOpen: false)` on shared `fs` | Reader dispose closes `fs` for everyone |
| Concurrency | Simultaneous reader/writer on same stream without clear protocol | Undefined buffering; corrupted reads |
| Design | In-place header patch via read/write same stream | Easy to truncate file if rewrite shorter than original |

**Fix (priority order):**

1. Set `leaveOpen: true` on **both** `StreamWriter` and `StreamReader`; dispose `fs` last explicitly.
2. Safer: read full content first, patch in memory, write to temp file, atomic replace — avoids length mismatch corrupting tail.
3. After writing header, `writer.Flush()` before reading; reset position with `fs.Seek` and optionally discard reader buffer (new reader instance).
4. Document that `StreamWriter` path ctor owns the stream unless you pass your own `FileStream`.

```csharp
using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8, leaveOpen: true))
{
    writer.WriteLine(newHeader);
    writer.Flush();
}
fs.Seek(0, SeekOrigin.Begin);
using (StreamReader reader = new StreamReader(fs, Encoding.UTF8, leaveOpen: true))
{
    _ = reader.ReadLine();
    string remainder = reader.ReadToEnd();
}
```

**Production takeaway:** Default `leaveOpen: false` means disposing the text wrapper closes the base stream — Karat tests layered I/O ownership called out in **Program.cs** Section 2d/3f FileStream chains.

---

#### Q8. (M) A cross-platform app parses `.env`-style files written on mixed developer machines (Windows CRLF, macOS/Linux LF). Review ingestion:

```csharp
public Dictionary<string, string> ParseEnvFile(string path)
{
    var map = new Dictionary<string, string>();
    using StreamReader reader = new StreamReader(path);

    string content = reader.ReadToEnd();
    foreach (string rawLine in content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
    {
        int eq = rawLine.IndexOf('=');
        if (eq <= 0) continue;
        string key = rawLine[..eq].Trim();
        string value = rawLine[(eq + 1)..].Trim();
        map[key] = value;
    }
    return map;
}
```

What breaks when files use CRLF or when keys are compared across environments, and how should line-based parsing use `StreamReader` instead?

**Answer:** Splitting on `'\n'` alone leaves a trailing `'\r'` on keys when the file uses CRLF — `map["HOST"]` misses lookups for `"HOST\r"`. `ReadToEnd()` also reintroduces the large-file memory trap. Loop with `ReadLine()`, which strips platform newlines (`\r\n` or `\n`) uniformly, trim keys/values defensively, and use ordinal key comparison; skip comments with `#` per line instead of splitting the whole file.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Newlines | `Split('\n')` on CRLF content | Keys include `\r` — silent config misses in Linux-deployed apps |
| Memory | `ReadToEnd()` for config | Unbounded allocation if `.env` grows or includes generated blocks |
| Parsing | `RemoveEmptyEntries` | Skips intentional blank lines vs comments — may hide format errors |

**Fix (priority order):**

1. Replace bulk split with `while ((line = reader.ReadLine()) != null)` — `ReadLine` removes `\r\n`/`\n` per **Program.cs** Section 3.
2. `key = key.Trim('\r', ' ', '\t')` defensively if content comes from external tools.
3. Use `StringComparer.OrdinalIgnoreCase` only if spec requires case-insensitivity — document choice.
4. For deployment, normalize line endings in repo via `.gitattributes` — but runtime parsing must still tolerate CRLF.

```csharp
while (reader.ReadLine() is { } line)
{
    line = line.Trim();
    if (line.Length == 0 || line.StartsWith('#')) continue;
    int eq = line.IndexOf('=');
    if (eq <= 0) continue;
    map[line[..eq].Trim()] = line[(eq + 1)..].Trim();
}
```

**Production takeaway:** Cross-platform text bugs often show up as `\r`-poisoned keys, not mojibake — Karat expects `ReadLine` semantics vs manual split. See **Program.cs** QUICK REFERENCE — prefer line loop over whole-file helpers for scalable parsing.
