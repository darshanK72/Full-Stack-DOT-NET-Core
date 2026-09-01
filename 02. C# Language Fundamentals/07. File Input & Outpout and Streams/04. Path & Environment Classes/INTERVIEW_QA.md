# 04. Path & Environment Classes — Interview Q&A

> Back to [Module Index](../INTERVIEW_QA.md)

---

## Q1. Why should you use `Path.Combine` instead of string concatenation with `"\\"` or `"/"`?

**Concepts**
- OS-correct directory separator automatically chosen
- Handles trailing separator on the first segment gracefully
- Cross-platform portability (Windows `\`, Linux/macOS `/`)
- Prevents double-separator bugs (`"base\\" + "\\file"`)
- Rooted second segment resets to that root (documented behavior)

**Answer**

`Path.Combine` inserts the directory separator character appropriate for the current OS — `\` on Windows, `/` on Linux and macOS — so the same code produces valid paths on any platform without conditional logic. String concatenation with `"\\"` hardcodes the Windows separator, breaking on Linux and macOS; concatenation with `"/"` works on both OS types for path content but is incorrect on Windows for drive-letter paths like `C:\`. `Path.Combine` also handles edge cases: if the first segment already ends with a separator, it does not add a duplicate one, and if a later segment is an absolute (rooted) path, Combine correctly returns that segment as the result rather than prepending the prior segments. The rule is: always use `Path.Combine` for every segment join, never manual concatenation.

---

## Q2. What happens when a segment passed to `Path.Combine` is already a rooted (absolute) path?

**Concepts**
- Rooted segment silently discards all preceding segments
- `Path.IsPathRooted(segment)` to detect before combining
- Behavior matches the way shells resolve `cd /absolute/path`
- Security concern: user-supplied rooted input can escape intended base
- Documented behavior — not a bug, but a common surprise

**Answer**

When any segment in `Path.Combine` is an absolute (rooted) path, `Combine` discards all preceding segments and starts fresh from that absolute segment. For example, `Path.Combine("exports", "2026", "/etc/passwd")` returns `/etc/passwd` on Linux, completely ignoring `"exports"` and `"2026"`. This mirrors the behavior of most shell `cd` commands and is documented, but it is a common security concern in web applications where user input is accepted as a path component: a user submitting `/etc/passwd` as a filename can escape the intended base directory. The defensive pattern is to validate that user-supplied segments are not rooted (`Path.IsPathRooted(userInput)` returns `false`) and do not contain `..` sequences before combining them. `Path.GetFullPath(combined, basePath)` available since .NET Core 2.1 validates that the final resolved path stays within the intended base.

---

## Q3. What do `Path.GetFileName`, `Path.GetFileNameWithoutExtension`, and `Path.GetExtension` return?

**Concepts**
- `GetFileName` — last segment of the path (may be a directory name if no file extension)
- `GetFileNameWithoutExtension` — filename minus the last extension
- `GetExtension` — last extension including the dot, or empty string
- None of these methods touch the filesystem — purely string operations
- Multi-dot filenames: only the last dot is the extension boundary

**Answer**

`Path.GetFileName(path)` returns everything after the last directory separator — for `"reports/2026/sales.csv"` it returns `"sales.csv"`, and for a directory path `"reports/2026/"` with a trailing separator it returns an empty string. `Path.GetFileNameWithoutExtension(path)` strips the last extension: `"sales.csv"` → `"sales"`, and `"archive.tar.gz"` → `"archive.tar"` (only the last `.gz` is removed). `Path.GetExtension(path)` returns the extension including the leading dot: `".csv"`, `".gz"`, or an empty string for paths with no dot in the filename. All three are purely string operations with no filesystem access, so they work on paths that do not exist on disk and return results based entirely on the string content of the path argument.

---

## Q4. What does `Path.GetDirectoryName` return, and what is its edge case with null?

**Concepts**
- Returns the parent directory path string without the filename
- Returns `null` when the path has no parent (root paths and bare filenames)
- Trailing separator handling differs between paths
- Used with `Directory.CreateDirectory(Path.GetDirectoryName(filePath)!)` pattern
- Null-forgiving operator required when chained with methods that don't accept null

**Answer**

`Path.GetDirectoryName("reports/2026/sales.csv")` returns `"reports/2026"` — everything before the last separator. For a path with no parent — a bare filename like `"sales.csv"` or a root path like `"C:\\"` on Windows — it returns `null`. This null return is the source of the common `null!` null-forgiving operator in `Directory.CreateDirectory(Path.GetDirectoryName(filePath)!)`: the developer knows the path is not a bare filename and asserts non-null. A common pattern is to ensure the directory for a new file exists before writing: `Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!)`, which is safe as long as `outputPath` contains a directory component. Always validate user-supplied paths for this edge case before calling `GetDirectoryName` and passing the result to methods that do not accept null.

---

## Q5. How does `Path.ChangeExtension` differ from manual string slicing?

**Concepts**
- `ChangeExtension` handles no-extension files correctly
- Handles multi-part extensions — only the last part is replaced
- `null` argument keeps current extension; empty string removes it
- Manual `path[..^4]` fails on filenames shorter than 4 chars or wrong extension length
- Purely a string operation — no filesystem access

**Answer**

`Path.ChangeExtension("invoice.pdf", ".archive.pdf")` correctly replaces the last extension, returning `"invoice.archive.pdf"`, while a manual slice like `path[..^4]` hardcodes the assumption that the extension is exactly 4 characters (including the dot). For a file without an extension, `ChangeExtension("Makefile", ".bak")` appends `.bak`, while the manual slice would corrupt the filename. Passing `null` as the new extension preserves the current one (a no-op useful in conditional expression), and passing an empty string or `""` removes the extension and the dot entirely, returning `"invoice"`. `Path.ChangeExtension` is also safe for paths with no dot in the filename — it adds the new extension without misinterpreting a separator character. Always prefer `ChangeExtension` over manual index arithmetic for extension replacement.

---

## Q6. What does `Path.GetFullPath` do, and what are its pitfalls?

**Concepts**
- Resolves relative path against `Environment.CurrentDirectory`
- Returns absolute path string (no filesystem check)
- Throws `ArgumentException` for invalid path characters
- `CurrentDirectory` at runtime may differ from developer expectation
- `.NET Core 2.1+`: `GetFullPath(path, basePath)` overload for explicit base

**Answer**

`Path.GetFullPath(relativePath)` resolves the relative path against `Environment.CurrentDirectory` and returns an absolute path string, normalizing `.` and `..` segments and removing redundant separators. It does not check whether the path exists — it is purely a string transformation. The pitfall is that `CurrentDirectory` is the process working directory set by the host, not necessarily the folder containing the assembly; in a unit test runner, `CurrentDirectory` may be a temp folder, and in a systemd service it may be `/`. Code that calls `GetFullPath("data/config.json")` in development and production may resolve to completely different absolute paths. The `.NET Core 2.1+` overload `Path.GetFullPath(path, basePath)` accepts an explicit base directory, which is much safer: `Path.GetFullPath("data/config.json", AppContext.BaseDirectory)` always resolves relative to the assembly folder regardless of the process working directory.

---

## Q7. What is the difference between `Path.IsPathRooted` and `Path.GetPathRoot`?

**Concepts**
- `IsPathRooted` — returns `bool` indicating whether path is absolute
- `GetPathRoot` — returns the root component string (`"C:\\"`, `"/"`, or empty)
- Both are pure string operations — no filesystem access
- Rooted path starts with drive letter, UNC `\\server\share`, or `/` on Unix
- Use `IsPathRooted` for validation; use `GetPathRoot` to extract the root component

**Answer**

`Path.IsPathRooted(path)` returns `true` if the path is absolute — starting with a drive letter on Windows (`"C:\"`) or a forward slash on Linux (`"/home/..."`) or a UNC prefix (`"\\server\share"`). `Path.GetPathRoot(path)` returns the root string itself: `"C:\\"`, `"/"`, `"\\\\server\\share\\"`, or an empty string for relative paths. `IsPathRooted` is useful as a validation guard before combining user-supplied segments: reject any segment where `IsPathRooted` returns `true` to prevent directory traversal out of the intended base. `GetPathRoot` is useful when the code needs to strip the root to create a portable relative subpath, or to detect whether a path is on a specific drive. Neither method checks the filesystem — they examine only the string content of the path argument.

---

## Q8. What are `Path.DirectorySeparatorChar` and `Path.AltDirectorySeparatorChar`?

**Concepts**
- `DirectorySeparatorChar` — primary separator (`\` on Windows, `/` on Linux/macOS)
- `AltDirectorySeparatorChar` — alternate separator (often `/` on Windows too)
- Use `Path.Combine` as the preferred approach — avoid inserting chars manually
- Manual insertion needed for custom format strings or HTTP-style virtual paths
- Never hard-code `'\\'` or `'/'` in OS-sensitive path code

**Answer**

`Path.DirectorySeparatorChar` is the primary directory separator for the current OS: `\` on Windows and `/` on Linux and macOS. `Path.AltDirectorySeparatorChar` is the alternate separator, which on Windows is `/` (both are accepted by the Windows kernel), and on Linux is also `/` (there is no true alternate). These constants are used when code must insert exactly one separator character manually — for example, to build a display string or trim a trailing separator — rather than constructing a path segment. `Path.Combine` is always preferable over manual separator insertion for path construction because it handles edge cases automatically. Never hardcode `'\\'` or `'/'` in path construction; always use these constants or, better, `Path.Combine`.

---

## Q9. What is `Path.GetTempPath()` and when should you use it?

**Concepts**
- Returns the OS temp directory path (Windows: `%TEMP%`, Linux: `/tmp`)
- Use for short-lived scratch files that do not need a permanent location
- Does not create files — just returns the directory string
- Combine with `Path.Combine` and a Guid-based name for unique scratch paths
- Files persist until explicitly deleted or OS cleanup

**Answer**

`Path.GetTempPath()` returns the path to the OS-managed temporary file directory — `C:\Users\username\AppData\Local\Temp` on Windows, `/tmp` on most Linux distributions. It is appropriate for staging files that will be deleted within the same request, operation, or process lifetime: image uploads being processed, intermediate computation results, and staging areas for file moves. The method returns only a directory path string with no file creation — the caller must create files within it. For unique scratch file names without creating a file immediately, use `Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tmp")`. For containerized environments, `/tmp` is often a `tmpfs` mount separate from the data volume, which means `File.Move` from temp to the data volume crosses filesystems and degrades to non-atomic copy-then-delete — stage temp files in the destination directory instead.

---

## Q10. What does `Path.GetTempFileName()` do differently from `Path.GetTempPath()`?

**Concepts**
- `GetTempFileName` creates an actual empty file on disk and returns its path
- Returns a unique path in the OS temp directory
- File remains on disk until explicitly deleted — no automatic cleanup
- Useful for ensuring a unique path without a race condition on creation
- Maximum of 65,535 unique temp file names on Windows before `IOException`

**Answer**

`Path.GetTempFileName()` both creates an empty zero-byte file in the OS temp directory and returns its absolute path — unlike `GetTempPath()`, which only returns the directory. The file is guaranteed unique at creation time because the OS atomically creates it, which is useful when the name must be reserved before the actual content is written. The critical gotcha is that the file persists until `File.Delete` is called — there is no automatic cleanup, so forgetting to delete in a `finally` block causes temp file accumulation. On Windows, the underlying API exhausts after 65,535 outstanding temp files (across all processes since the last reboot) and throws `IOException`, which has caused production outages in services that leak temp files for days. The defensive pattern is always: create the temp file, remember its path, wrap the work in `try/finally` with `File.Delete(tempPath)` in the `finally`.

---

## Q11. What is the difference between `Environment.CurrentDirectory` and `AppContext.BaseDirectory`?

**Concepts**
- `CurrentDirectory` — process working directory set by the host at launch
- `BaseDirectory` — folder containing the running assembly (`*.dll` or `*.exe`)
- `CurrentDirectory` can change at runtime (`Environment.CurrentDirectory = ...`)
- `BaseDirectory` is read-only and stable for the process lifetime
- Deployed apps should anchor file paths to `BaseDirectory`, not `CurrentDirectory`

**Answer**

`Environment.CurrentDirectory` is the process working directory — the path the OS shell was in when the process was launched, or whatever the host process set it to. In a Visual Studio debug run it is typically `bin\Debug\net10.0`; in an MSTest runner it might be a temp folder; in a `systemd` service it is usually `/`. It can also be changed at runtime by calling `Environment.CurrentDirectory = newPath`, which affects all relative path resolution for the remainder of the process. `AppContext.BaseDirectory` is the folder containing the currently executing assembly and is fixed for the lifetime of the process — it is where `MyApp.dll` lives. For configuration files, data files, and embedded resources shipped alongside the assembly, `AppContext.BaseDirectory` is the correct anchor. `CurrentDirectory` is appropriate only when the intent is explicitly to operate relative to wherever the user invoked the tool.

---

## Q12. What is `Environment.GetFolderPath(SpecialFolder)`, and why is it preferable to hardcoded paths?

**Concepts**
- Returns OS-standard paths for well-known user and system folders
- Works on Windows, Linux, and macOS with the same API
- No drive letter assumption — correct for D: drives, network profiles
- Handles per-user vs per-machine vs roaming vs local data separation
- Always combine the result with `Path.Combine` — never assume subfolder structure

**Answer**

`Environment.GetFolderPath(Environment.SpecialFolder.X)` returns the OS-managed path for well-known folders, correctly resolving the location on the current machine and user session without any hardcoded strings. On Windows it correctly handles user profiles on non-C: drives, corporate environments with redirected folders, and different OS versions. On Linux, it returns XDG-compliant paths (when available) like `$HOME/.local/share` for `LocalApplicationData`. The hardcoded alternative — `"C:\\Users\\" + username + "\\AppData\\Roaming\\" + appName` — fails on D: drive installs, breaks in Azure App Service and containers, and is wrong on Linux. Always combine the result with `Path.Combine` and the application-specific subfolder: `Path.Combine(appData, "MyCompany", "MyApp", "settings.json")` creates a clean path without any assumptions about the returned string's trailing separator or content.

---

## Q13. What is the difference between `SpecialFolder.ApplicationData` (roaming) and `SpecialFolder.LocalApplicationData` (local)?

**Concepts**
- `ApplicationData` — roaming profile, synced across PCs in domain environments
- `LocalApplicationData` — local machine only, not synced
- Roaming for user preferences; local for machine-specific cache and large files
- Group Policy can disable or redirect roaming profiles
- Container environments: roaming may not be available; local data volume is the correct choice

**Answer**

`SpecialFolder.ApplicationData` resolves to the roaming application data folder — in Windows domain environments, this folder is synchronized to the domain profile server and follows the user when they log in from any machine. It is appropriate for user preferences, license keys, and application settings that should be the same regardless of which machine the user works on. `SpecialFolder.LocalApplicationData` resolves to a machine-local folder that is never synchronized — appropriate for caches, downloaded indices, and large binary assets that would be expensive to sync or are machine-specific (like hardware-calibrated settings). In container environments, roaming profiles are typically unavailable and even local app data may sit on a read-only overlay; the correct location in a container is a mounted persistent volume, and `LocalApplicationData` maps to that mount if the container is configured correctly.

---

## Q14. What is `SpecialFolder.UserProfile`, and when do you use it instead of `MyDocuments`?

**Concepts**
- `UserProfile` — home directory root (`C:\Users\username` on Windows, `/home/username` on Linux)
- `MyDocuments` / `Personal` — the Documents subfolder within the user profile
- `UserProfile` for tools that need the home root (SSH config, dotfiles)
- `MyDocuments` for end-user documents that should appear in file explorers
- Linux: `UserProfile` maps to `$HOME`; `MyDocuments` maps to `$HOME` (no standard subfolder)

**Answer**

`SpecialFolder.UserProfile` returns the user's home directory root — `C:\Users\dakhairnar` on Windows, `/home/dakhairnar` on Linux. It is used when an application needs to read or write to the root of the user's profile: SSH stores its `~/.ssh` directory there, `.gitconfig` lives there, and shell dotfiles live there on Linux. `SpecialFolder.MyDocuments` (alias: `SpecialFolder.Personal`) returns the Documents subfolder, which on Windows is typically `C:\Users\dakhairnar\Documents` and is the conventional location for user-created documents that should appear in File Explorer under Documents. Use `MyDocuments` for exported reports and files users are expected to work with in a file manager; use `UserProfile` for hidden configuration or tool-specific state that should not clutter the visible Documents folder.

---

## Q15. How do `Environment.MachineName` and `Environment.UserName` help build stable log or data paths?

**Concepts**
- `MachineName` — NetBIOS name of the host machine
- `UserName` — login name of the currently logged-in user
- Useful as disambiguation segments in shared-storage paths
- Include in log directory hierarchy for multi-machine aggregation
- May contain characters that are invalid in paths on some OSes — use in combination with `Path.Combine`

**Answer**

`Environment.MachineName` returns the NetBIOS or hostname of the machine, and `Environment.UserName` returns the current interactive user's login name. Both are useful for constructing disambiguation segments in shared log or data directories — a log root of `Path.Combine(baseDir, "logs", Environment.MachineName, Environment.UserName)` ensures that concurrent writes from different machines or different users on the same machine do not collide on the same path. In containerized environments, `MachineName` returns the pod name (set by Kubernetes from `hostname`), making it useful for per-pod log directories in a shared persistent volume. Always use these values in `Path.Combine` calls and be aware that `MachineName` on Windows can contain characters that are valid in environment variables but might be unusual in filenames (dashes and periods are safe; forward slashes would break path parsing).

---

## Q16. What are the cross-platform path differences between Windows and Linux/macOS?

**Concepts**
- Windows: drive roots (`C:\`), `\` primary separator, usually case-insensitive
- Linux/macOS: single root (`/`), `/` only separator, case-sensitive
- `"C:"` on Linux is a valid relative folder name, not a drive root
- File equality checks must be case-insensitive on Windows, case-sensitive on Linux
- `Path.DirectorySeparatorChar` gives the correct separator for the current OS

**Answer**

Windows uses drive-letter roots (`C:\`, `D:\`), backslash as the primary separator, and typically case-insensitive path comparison — `"Report.pdf"` and `"report.pdf"` refer to the same file. Linux and macOS use a single hierarchical root `/`, forward slash as the only separator, and case-sensitive path comparison — `"Report.pdf"` and `"report.pdf"` are different files. The practical consequences: hardcoded `"C:\\"` paths fail on Linux entirely (`"C:"` becomes a relative folder name); hardcoded `"\\"` separators produce invalid paths on Linux; case-insensitive file equality tests that work on Windows silently fail to find files on Linux when the case differs. Cross-platform code must use `Path.Combine` for all path construction, `OperatingSystem.IsWindows()` / `OperatingSystem.IsLinux()` for conditional behavior, and `StringComparison.OrdinalIgnoreCase` on Windows vs `Ordinal` on Linux when comparing filenames.

---

## Q17. (Gotcha) Why does `Path.GetFullPath` in a unit test resolve to a completely different path than in production?

**Concepts**
- `GetFullPath` resolves against `Environment.CurrentDirectory`
- Test runner sets `CurrentDirectory` to a temp or output folder
- Production service sets `CurrentDirectory` to a different working directory
- `AppContext.BaseDirectory` as the stable anchor for deployed-relative paths
- `GetFullPath(path, basePath)` overload bypasses `CurrentDirectory` entirely

**Answer**

`Path.GetFullPath("data/config.json")` calls the OS to resolve the relative path against `Environment.CurrentDirectory` — the process working directory. In MSTest or xUnit, the test runner may set `CurrentDirectory` to the test output directory, the repository root, or a randomly generated temp folder. In the production service, `CurrentDirectory` may be `/` (systemd default) or a different path set by the host. The same relative string produces two completely different absolute paths in the two environments, so the unit test passes (finds the file in the test output) while production fails (file not found at `/data/config.json`). The fix is to use `Path.GetFullPath("data/config.json", AppContext.BaseDirectory)`, which resolves relative to the assembly folder in both environments, or to use `Path.Combine(AppContext.BaseDirectory, "data", "config.json")` which is clearer about the anchor.

---

## Q18. (Gotcha) What are the consequences of calling `Path.GetTempFileName()` in a request handler without cleanup?

**Concepts**
- `GetTempFileName` creates a file immediately — not just a name
- No automatic cleanup — file persists until explicitly deleted
- Windows: exhaustion after ~65,535 concurrent temp files → `IOException`
- Container overlay filesystem: temp accumulation fills the container disk
- `try/finally` with `File.Delete(tempPath)` as the mandatory pattern

**Answer**

Each call to `Path.GetTempFileName()` creates an actual zero-byte file in the OS temp directory. Without a `try/finally` that deletes the file on all exit paths — including exceptions — every request that throws leaves an orphaned file. Over hours of production load these accumulate: on a service handling 1,000 requests per hour with a 1% error rate, 10 orphan files per hour accumulates 240 files per day. In container environments where `/tmp` is a `tmpfs` mount with a fixed size limit, filling it causes new `GetTempFileName` calls to throw `IOException: no space left on device`. On Windows, the internal counter for temp file names resets per reboot and exhausts after 65,535 outstanding files across all processes since the last reboot. The mandatory pattern is to store the path, wrap all work in `try/finally`, and call `File.Delete(tempPath)` in the finally block — setting the path variable to `null` after a successful operation is a common sentinel pattern to avoid double-deleting.

---

## Q19. (Gotcha) Why does `Path.Combine("ignored", "/etc/hosts")` silently discard "ignored" on Linux?

**Concepts**
- Rooted second segment resets to that root, discarding all prior segments
- `Path.IsPathRooted("/etc/hosts")` returns `true`
- Security: user-supplied path can escape intended base directory
- `Path.GetFullPath(combined, allowedBase)` for path traversal validation
- Directory traversal also possible with `"../"` sequences in non-rooted segments

**Answer**

`Path.Combine("ignored", "/etc/hosts")` returns `"/etc/hosts"` on Linux — the `"ignored"` segment is completely discarded because the second segment is an absolute path. This is documented behavior that mirrors shell path resolution, but it is a security vulnerability in code that concatenates a user-supplied filename into a base path: an attacker passes `"/etc/shadow"` as the filename and the application reads a system file outside its intended scope. The same attack works with `../../../etc/shadow` using `..` traversal. The defensive fix is to validate that user input is neither rooted (`Path.IsPathRooted(input)`) nor contains `".."` sequences before combining. For complete protection, use `Path.GetFullPath(Path.Combine(baseDir, userInput))` and then assert that the result starts with `baseDir + Path.DirectorySeparatorChar` to confirm the final path is still within the allowed directory.

---

## Q20. (Scenario R) A service builds a config path by string concatenation — fails on Linux and when `basePath` ends with a separator.

**Defective code:**

```csharp
public string GetConfigPath(string basePath, string configFileName)
{
    // BUG 1: hard-coded Windows backslash
    // BUG 2: double separator if basePath already ends with '\'
    // BUG 3: forward slash in configFileName breaks on Windows 95/legacy (minor)
    return basePath + "\\" + configFileName;
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Platform | `"\\"` is the Windows separator — produces invalid path on Linux/macOS | All file operations using this path fail on non-Windows deployments |
| Double separator | If `basePath` ends with `"\"`, result is `"base\\file"` (minor on modern Windows but wrong) | Path resolution may fail on strict path validators or legacy tools |
| Maintainability | Hard-coded separator scattered across codebase — requires find-and-replace for cross-platform work | High cost of future cross-platform migration |

**Fix priority**

1. Replace with `Path.Combine(basePath, configFileName)` — handles separator, trailing slash, and OS differences.
2. Validate that `configFileName` does not contain `..` or rooted path components if it comes from user input.

```csharp
public string GetConfigPath(string basePath, string configFileName)
{
    if (Path.IsPathRooted(configFileName) || configFileName.Contains(".."))
        throw new ArgumentException("Invalid config file name.", nameof(configFileName));
    return Path.Combine(basePath, configFileName);
}
```

---

## Q21. (Scenario R) An integration test sets `CurrentDirectory` to a test folder; production code uses `Path.GetFullPath("data/config.json")` and resolves the wrong path in CI.

**Defective code:**

```csharp
public class ConfigLoader
{
    public string GetConfigPath()
    {
        // BUG: resolves against CurrentDirectory, not assembly location
        return Path.GetFullPath("data/config.json");
    }
}

// Test:
[TestInitialize]
public void Setup()
{
    Environment.CurrentDirectory = @"C:\TestFixtures";  // test sets a fake CWD
}

[TestMethod]
public void GetConfigPath_ReturnsExpectedPath()
{
    var loader = new ConfigLoader();
    string path = loader.GetConfigPath();
    // Returns "C:\TestFixtures\data\config.json" in test but "/data/config.json" in CI
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Environment sensitivity | `GetFullPath` uses `CurrentDirectory` which differs between local dev, CI, and production | Path resolves to different locations in different environments; test passes, production fails |
| Testability | Production code cannot be unit-tested without manipulating `CurrentDirectory` | Test setup is fragile and may affect other tests run in the same process |
| Portability | Relative path with no explicit base is undefined behavior in deployed services | Service startup failures when systemd or Docker sets CWD to `/` |

**Fix priority**

1. Replace `Path.GetFullPath("data/config.json")` with `Path.Combine(AppContext.BaseDirectory, "data", "config.json")`.
2. Or use the two-argument `Path.GetFullPath("data/config.json", AppContext.BaseDirectory)`.
3. Never manipulate `Environment.CurrentDirectory` in tests — it is a global mutable state that affects all concurrent tests.

---

## Q22. (Scenario R) Application stores data under `"C:\\ProgramData\\MyApp"` hardcoded — fails in Azure, containers, and D-drive installs.

**Defective code:**

```csharp
public class AppDataService
{
    private static readonly string DataRoot = @"C:\ProgramData\MyApp";   // BUG: hardcoded path

    public void SaveSettings(string json)
    {
        Directory.CreateDirectory(DataRoot);
        File.WriteAllText(Path.Combine(DataRoot, "settings.json"), json);
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Platform | `"C:\\"` does not exist on Linux or macOS — `Directory.CreateDirectory` throws | All data operations fail in containerized or non-Windows deployments |
| Environment | `ProgramData` on Windows may be on a D: drive if Windows is installed there | `Directory.CreateDirectory` on `C:\ProgramData` fails when Windows is on D: |
| Portability | Hard-coded path cannot be overridden by configuration or environment variables | No way to redirect data path for multi-tenant or cloud environments |

**Fix priority**

1. Replace with `Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)` for machine-wide shared data.
2. Use `SpecialFolder.LocalApplicationData` for per-user, non-roaming data.
3. Combine with `Path.Combine(rootPath, "MyCompany", "MyApp")` for proper app namespacing.

```csharp
private static readonly string DataRoot = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyCompany", "MyApp");
```

---

## Q23. (Scenario M) An API handler calls `Path.GetTempFileName()` per request but never deletes the file — disk fills over time.

**Defective code:**

```csharp
app.MapPost("/process", async (IFormFile file, IProcessor processor) =>
{
    string tempPath = Path.GetTempFileName();       // BUG: creates file; never cleaned up on error
    await using var temp = File.Open(tempPath, FileMode.Open);
    await file.CopyToAsync(temp);
    temp.Position = 0;
    var result = await processor.ProcessAsync(temp);
    File.Delete(tempPath);                          // BUG: only reached on success path
    return Results.Ok(result);
});
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | `File.Delete` only on success — exceptions leave orphan temp files | Disk fills over hours of production load; Windows exhausts 65,535 temp names |
| Inefficiency | `GetTempFileName` creates the file, then code re-opens it — two operations where one suffices | Extra syscall per request |
| Volume | On Linux containers, `/tmp` is `tmpfs` with a size limit — fills quickly under load | `IOException: no space left on device` causes all subsequent requests to fail |

**Fix priority**

1. Wrap in `try/finally` with `File.Delete(tempPath)` in the `finally` block.
2. Or use a Guid-based name in the destination directory to stay on the same volume.
3. Consider not writing to disk at all if the upload stream can be processed in-memory or piped directly.

```csharp
string tempPath = Path.GetTempFileName();
try
{
    await using var temp = File.Open(tempPath, FileMode.Open);
    await file.CopyToAsync(temp);
    temp.Position = 0;
    var result = await processor.ProcessAsync(temp);
    return Results.Ok(result);
}
finally
{
    if (File.Exists(tempPath)) File.Delete(tempPath);
}
```

---

## Q24. (Scenario D) A cross-platform CLI tool uses `Path.Combine("C:", "exports", "output.csv")` — on Linux "C:" is treated as a relative folder name. What is the correct approach?

**Concepts**
- `"C:"` on Linux is a valid relative directory name, not a drive root
- `Path.IsPathRooted("C:\\exports")` returns `true` on Windows but `Path.IsPathRooted("C:")` returns `false` on Linux
- Cross-platform tools should not assume drive letters exist
- `AppContext.BaseDirectory` or `SpecialFolder` as the correct portable anchor
- `--output` CLI flag allowing users to override the path portably

**Answer**

On Windows, `Path.Combine("C:", "exports", "output.csv")` produces `"C:exports\\output.csv"` — a path relative to the current directory on drive C: — which is already not the intended `"C:\\exports\\output.csv"` (note the missing backslash after `C:`). On Linux, `"C:"` is a valid relative folder name, so the combined result is `"C:/exports/output.csv"` which creates a subdirectory named `"C:"` in the current working directory. Neither is correct. The right approach for a CLI tool is to default outputs to a path relative to `AppContext.BaseDirectory` (for outputs shipped with the app) or to use `Environment.GetFolderPath(SpecialFolder.Desktop)` / `SpecialFolder.MyDocuments` for user-facing exports. Expose an `--output` flag so users can specify an absolute path on any OS: `Path.GetFullPath(outputArg)` resolves the user-provided path correctly on both platforms.
