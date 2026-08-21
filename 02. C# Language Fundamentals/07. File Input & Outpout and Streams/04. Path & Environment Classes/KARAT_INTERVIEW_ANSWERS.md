# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/04. Path & Environment Classes`

---

#### Q1. (R) A report exporter works on Windows dev machines but fails on Linux CI with "Could not find a part of the path." Review this path builder:

```csharp
public string BuildExportPath(string customerId, string fileName)
{
    string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    return baseDir + "\\Reports\\" + customerId + "\\" + fileName;
}

// Called from a nightly job:
var path = BuildExportPath("CUST-42", "summary.csv");
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
await File.WriteAllTextAsync(path, csvContent);
```

What is wrong, and how do you fix it for cross-platform deployment?

**Answer:** The method hard-codes Windows backslashes and assumes a user Documents folder exists on a headless CI agent — on Linux the concatenated path is invalid and `MyDocuments` may be empty or unsuitable for a server job.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cross-platform | `"\\"` string concatenation | Linux treats `\` as a valid filename character, not a separator — path does not resolve |
| Design | `SpecialFolder.MyDocuments` on a server/CI worker | No interactive user profile; base path may be empty or wrong |
| Maintainability | Manual join instead of `Path.Combine` | Every new segment repeats the separator mistake |

**Fix (priority order):**

1. Replace concatenation with `Path.Combine(baseDir, "Reports", customerId, fileName)`.
2. Do not use `MyDocuments` for server exports — read an configured output root from `IConfiguration` / environment variable (e.g. `/var/app/exports` or a mounted volume).
3. Validate `baseDir` is non-empty before `CreateDirectory`; fail fast with a clear configuration error in CI.
4. Sanitize `customerId` and `fileName` — reject path separators and `..` segments before combining.

```csharp
public string BuildExportPath(string outputRoot, string customerId, string fileName)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
    return Path.Combine(outputRoot, "Reports", customerId, fileName);
}
```

**Production takeaway:** Hard-coded backslashes pass on Windows dev boxes and fail immediately in Linux containers — Karat expects `Path.Combine` plus an explicit, configurable root instead of desktop assumptions. See **Program.cs** Section 1 and Section 8 — cross-platform rules.

---

#### Q2. (R) An internal admin API accepts a `fileName` query parameter and serves files from a fixed folder. Review the handler:

```csharp
private readonly string _storageRoot = Path.Combine(AppContext.BaseDirectory, "uploads");

public IResult Download(string fileName)
{
    string requested = Path.GetFullPath(Path.Combine(_storageRoot, fileName));
    if (!File.Exists(requested))
        return Results.NotFound();

    return Results.File(requested);
}

// Request: GET /download?fileName=..\..\appsettings.Production.json
```

What security and correctness issues exist, and what is the prioritized fix?

**Answer:** `Path.GetFullPath` resolves `..` segments against `_storageRoot`, so a malicious `fileName` can escape the uploads folder and read arbitrary files on the server — the existence check does not confine access to the intended directory.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No verification that resolved path stays under `_storageRoot` | Path traversal — read secrets, configs, other tenants' files |
| Input | Unsanitized user-controlled `fileName` | `..`, absolute paths, alternate separators bypass intent |
| Correctness | `GetFullPath` alone is not a sandbox boundary | Developer assumes normalization equals authorization |

**Fix (priority order):**

1. Reject rooted paths and any segment containing `..` before combining — or use `Path.GetFileName(fileName)` if only flat files are allowed.
2. After resolving, verify the full path is prefixed by the normalized storage root (case-aware on Linux):

```csharp
string storageRoot = Path.GetFullPath(_storageRoot);
string requested = Path.GetFullPath(Path.Combine(storageRoot, fileName));

if (!requested.StartsWith(storageRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal)
    && !requested.Equals(storageRoot, StringComparison.Ordinal))
    return Results.BadRequest();

if (!File.Exists(requested))
    return Results.NotFound();
```

3. Prefer an opaque file id mapped server-side to a stored name instead of accepting raw path fragments from the client.
4. Log traversal attempts; return 400/404 without leaking whether the target file exists outside uploads.

**Production takeaway:** `GetFullPath` normalizes strings — it does not enforce trust boundaries. Always anchor to a known root and verify containment after resolution. See **Program.cs** Section 3 — GetFullPath pitfalls.

---

#### Q3. (P) A worker service loads `config/settings.json` with a relative path. It passes locally from Visual Studio but fails in production when started as a Windows Service or from a systemd unit. The startup code:

```csharp
var settingsPath = Path.GetFullPath("config/settings.json");
var json = await File.ReadAllTextAsync(settingsPath);
```

Logs show `Environment.CurrentDirectory` is `C:\Windows\System32` on the server but the project folder when debugging. What is happening, and what anchor should production code use instead?

**Answer:** Relative paths passed to `Path.GetFullPath` resolve against `Environment.CurrentDirectory`, which follows the process working directory set by the shell, service wrapper, or scheduler — not the folder containing the published assembly.

- Locally, the IDE sets CWD to the project directory, so `config/settings.json` is found next to source layout.
- Installed services and systemd units often start with CWD `/` or `System32`, so the same relative string points at the wrong tree.
- Production code should anchor content-relative assets to `AppContext.BaseDirectory` (or `IHostEnvironment.ContentRootPath` in ASP.NET Core), which tracks the deployed app folder.

```csharp
var settingsPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "config", "settings.json"));
```

- If settings live outside the publish folder (common for secrets), read an absolute path from configuration rather than assuming a relative layout.
- Document and test startup from a non-project CWD in CI to catch this class of bug early.

**Production takeaway:** Never assume `CurrentDirectory` equals the app install location — Karat pairs this with deployment context. See **Program.cs** Section 7 — CurrentDirectory vs BaseDirectory.

---

#### Q4. (P) A containerized API writes large PDF exports using `Path.GetTempFileName()` and never deletes them. After a few days in Kubernetes, pods hit `No space left on device`. The temp folder path is `/tmp` inside the container. What breaks in this pattern, and what production approach replaces `GetTempFileName`?

**Answer:** `GetTempFileName` creates a zero-byte file immediately and returns its path, but the API replaces it with a large PDF without deleting the original or subsequent temps — ephemeral container `/tmp` (often a small `emptyDir` volume) fills up because nothing cleans up and each request adds another file.

- In containers, `Path.GetTempPath()` maps to `/tmp` unless overridden by `TMPDIR` — shared across requests in the same pod with no guaranteed recycle until the pod restarts.
- `GetTempFileName` is poor for large artifacts: it creates an extra file, uses predictable patterns, and encourages orphan leaks under load.
- Prefer streaming the response directly to the client, writing to a configured persistent volume, or using `IBlobStorage` / object storage for exports.
- If scratch space is required, combine `Path.GetTempPath()` with a unique name (`Guid`), wrap writes in `try/finally`, and delete in `finally`; consider `TemporaryFileStream` patterns or bounded pools.
- Set `TMPDIR` / `TEMP` / `TMP` explicitly in the deployment manifest to a sized volume when scratch I/O is unavoidable.
- Add disk-usage metrics and liveness checks — `/tmp` exhaustion kills all endpoints in the pod.

**Production takeaway:** Temp directories in containers are small and shared — treat them as bounded scratch space with explicit cleanup, not an export archive. See **Program.cs** Section 5 — GetTempPath / GetTempFileName cleanup note.

---

#### Q5. (R) A desktop-style feature is ported to a headless Linux server without changes:

```csharp
public string GetDefaultExportFolder()
{
    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    return Path.Combine(desktop, "MyApp", "Exports");
}

// Startup ensures folder exists:
Directory.CreateDirectory(GetDefaultExportFolder());
```

What fails on a server or container, and how should export location be chosen for server-side processing?

**Answer:** `SpecialFolder.Desktop` assumes an interactive user profile with a Desktop directory — on headless Linux servers or minimal container images the path is often empty or points under a non-writable home directory, causing `CreateDirectory` or later writes to fail.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Platform | Desktop folder on server/container | Path empty, missing, or not writable |
| Design | UI-centric SpecialFolder on backend | Wrong abstraction for batch/API exports |
| Operations | Silent reliance on user profile layout | Worked on developer workstation; fails in prod |

**Fix (priority order):**

1. Replace Desktop with a configured server path — environment variable, `appsettings`, or mounted volume (`/app/data/exports`).
2. For multi-tenant SaaS, use tenant-scoped storage (database blob, S3, Azure Blob) rather than local filesystem folders.
3. If user-specific exports are required on a desktop app, keep `SpecialFolder` — but gate server code paths separately.
4. Validate the chosen root exists and is writable at startup; surface a clear configuration error instead of failing mid-request.

```csharp
public string GetDefaultExportFolder(IConfiguration config)
{
    var root = config["Export:RootPath"]
        ?? Path.Combine(AppContext.BaseDirectory, "exports");
    return Path.Combine(root, "MyApp", "Exports");
}
```

**Production takeaway:** `SpecialFolder` values encode OS/user UI conventions — server workloads need explicit configuration, not Desktop. See **Program.cs** Section 6 — SpecialFolder and Section 8 — do not assume drive letters or desktop layout.

---

#### Q6. (M) A path helper builds log file locations from configuration segments. Review this method called on both Windows and Linux:

```csharp
public static string BuildLogPath(string configuredRoot, string appName, string logFile)
{
    // configuredRoot might be "/var/log", "logs", or "C:\\Logs" from appsettings
    return Path.Combine("ignored", "prefix", configuredRoot, appName, logFile);
}
```

What surprising result occurs when `configuredRoot` is an absolute Unix path (`/var/log`) or a Windows drive root (`C:\Logs`), and how should callers structure segments?

**Answer:** When any segment after the first is rooted (starts with `/` on Unix or a drive/root on Windows), `Path.Combine` discards all prior segments — `"ignored"` and `"prefix"` are dropped, and the result resets to the rooted segment plus the remainder.

- `Path.Combine("ignored", "prefix", "/var/log", "MyApp", "app.log")` → `/var/log/MyApp/app.log` on Linux.
- `Path.Combine("ignored", "prefix", @"C:\Logs", "MyApp", "app.log")` → `C:\Logs\MyApp\app.log` on Windows.
- Developers expect `"ignored/prefix"` to prefix configured roots — it silently does not when the config value is absolute.
- Pass either all-relative segments under a known base, or treat an absolute configured root as the sole first argument: `Path.Combine(configuredRoot, appName, logFile)` without dummy prefixes.
- Document in configuration schema whether `LogRoot` must be relative (to `BaseDirectory`) or absolute — do not mix assumptions in one Combine chain.

**Production takeaway:** Rooted segments in `Path.Combine` reset the path — a common misconfiguration when appsettings contains absolute paths. See **Program.cs** Section 1 — Combine with rooted segment demo.

---

#### Q7. (D) Two services exchange file paths over a message queue. Service A (Windows) sends `D:\data\invoices\inv-001.pdf`. Service B (Linux) tries to open it and also needs a relative path for an audit log entry. A developer writes:

```csharp
string incoming = message.FilePath; // from Windows producer
string relative = Path.GetRelativePath(AppContext.BaseDirectory, incoming);
await File.ReadAllTextAsync(incoming);
```

What breaks on Linux, and what contract should replace raw absolute paths between services?

**Answer:** Windows absolute paths are meaningless on Linux — `File.ReadAllTextAsync` fails because `D:\...` is not a valid path on Unix, and `Path.GetRelativePath` cannot produce a meaningful relative path across different roots or machines.

- `GetRelativePath` requires both paths to share a common base on the same machine; cross-OS absolute paths have no shared root.
- Message contracts should carry stable identifiers (blob URI, S3 key, file id, share-relative path) — not producer-local absolute paths.
- If both services mount the same network share, agree on a **share-relative** path (`invoices/inv-001.pdf`) and each service combines with its locally configured mount point via `Path.Combine(mountRoot, relativeKey)`.
- For audit logs, store the logical key or URI, not `GetRelativePath` output from foreign paths.
- Use object storage (HTTPS URL + auth) for cross-platform handoff; consumers download to their own temp scratch if local file access is required.

**Production takeaway:** File paths are not portable across OS or hosts — exchange logical keys or URIs and resolve locally. See **Program.cs** Section 8 — Windows drive roots vs Unix single-root layout.

---

#### Q8. (P) A build pipeline archives deeply nested test output on Windows agents. One test creates a folder tree exceeding 260 characters. Locally it works when long-path support is enabled; on a Linux agent the same code runs but a Windows-only integration test fails with `PathTooLongException`. What explains the platform difference, and what mitigations belong in the path-building code?

**Answer:** Windows historically enforced `MAX_PATH` (260 characters) unless long-path awareness is enabled at OS and application level; Linux paths are typically limited by `PATH_MAX` (often 4096 bytes) and are far more permissive — so the same deeply nested tree exceeds Windows limits while Linux succeeds.

- Developer machines with Windows 10+ long-path policy and .NET long-path support may hide the bug until a default-config CI Windows agent runs.
- Linux CI passing does not prove Windows deployment safety when paths are built from repeated `Path.Combine` of user names, guids, and nested fixture folders.
- Mitigations: shorten segment names, hash long identifiers (`SHA256` folder name instead of full title), flatten output layout, use `\\?\` prefix only as a last resort on Windows with explicit long-path enablement.
- Read remaining length budget before creating nested dirs; fail early with a clear test message instead of `PathTooLongException` mid-run.
- Keep artifact roots shallow — `artifacts/{buildId}/{suite}/file.ext` rather than mirroring full source tree depth.
- In CI, run at least one Windows job without long-path overrides to match conservative production environments.

**Production takeaway:** Path length limits are OS- and policy-dependent — design folder layouts for the shortest common denominator (default Windows), not the most permissive agent. See **Program.cs** Section 8 — cross-platform comparison table.
