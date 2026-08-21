# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/04. Path & Environment Classes`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

---

#### Q3. (P) A worker service loads `config/settings.json` with a relative path. It passes locally from Visual Studio but fails in production when started as a Windows Service or from a systemd unit. The startup code:

```csharp
var settingsPath = Path.GetFullPath("config/settings.json");
var json = await File.ReadAllTextAsync(settingsPath);
```

Logs show `Environment.CurrentDirectory` is `C:\Windows\System32` on the server but the project folder when debugging. What is happening, and what anchor should production code use instead?

---

#### Q4. (P) A containerized API writes large PDF exports using `Path.GetTempFileName()` and never deletes them. After a few days in Kubernetes, pods hit `No space left on device`. The temp folder path is `/tmp` inside the container. What breaks in this pattern, and what production approach replaces `GetTempFileName`?

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

---

#### Q7. (D) Two services exchange file paths over a message queue. Service A (Windows) sends `D:\data\invoices\inv-001.pdf`. Service B (Linux) tries to open it and also needs a relative path for an audit log entry. A developer writes:

```csharp
string incoming = message.FilePath; // from Windows producer
string relative = Path.GetRelativePath(AppContext.BaseDirectory, incoming);
await File.ReadAllTextAsync(incoming);
```

What breaks on Linux, and what contract should replace raw absolute paths between services?

---

#### Q8. (P) A build pipeline archives deeply nested test output on Windows agents. One test creates a folder tree exceeding 260 characters. Locally it works when long-path support is enabled; on a Linux agent the same code runs but a Windows-only integration test fails with `PathTooLongException`. What explains the platform difference, and what mitigations belong in the path-building code?
