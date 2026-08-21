# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/02. StreamReader & StreamWriter`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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
