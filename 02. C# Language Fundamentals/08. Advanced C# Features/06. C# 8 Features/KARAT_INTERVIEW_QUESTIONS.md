# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/06. C# 8 Features/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) After enabling `<Nullable>enable</Nullable>` on the document-ingest API, QA reports intermittent `NullReferenceException` on documents with no footnotes. Review the service:

```csharp
public sealed class DocumentIngestService
{
    public string BuildSummary(DocumentMetadata metadata)
    {
        string header = metadata.Id!.Trim();
        string note = metadata.Notes.ToUpperInvariant();
        return $"{header}: {note}";
    }

    public DocumentMetadata LoadFromJson(string json)
    {
        var doc = JsonSerializer.Deserialize<DocumentMetadata>(json);
        doc!.Id = doc.Id ?? "UNKNOWN";
        return doc;
    }
}

public sealed class DocumentMetadata
{
    public string Id { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
```

What problems remain after the developer "fixed" compiler warnings with `!`, and how do you harden this for production?

---

#### Q2. (R) A background worker streams archive pages to blob storage. Under deploy cancellation, the job keeps running for minutes and sometimes OOMs. Review the consumer and producer:

```csharp
public async Task ArchivePagesAsync(CancellationToken stoppingToken)
{
    var allPages = _documentStream.ReadPagesAsync()
        .ToListAsync(stoppingToken)
        .GetAwaiter()
        .GetResult();

    foreach (string page in allPages)
    {
        await _blobWriter.UploadPageAsync(page);
    }
}

public async IAsyncEnumerable<string> ReadPagesAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    foreach (string page in _pages)
    {
        await Task.Delay(200, cancellationToken);
        yield return page;
    }
}
```

Identify stacked compile-time, runtime, and scalability issues. What is the correct streaming pattern?

---

#### Q3. (R) A routing microservice parses document IDs with C# 8 ranges after a format change. Production throws `ArgumentOutOfRangeException` on valid-looking IDs. Review:

```csharp
public string ExtractYear(string documentId)
{
    return documentId[4..8];
}

public string ExtractSequence(string documentId)
{
    return documentId[^3..];
}

public DocumentMetadata[] TakeTail(DocumentMetadata[] batch, int tailCount)
{
    return batch[^tailCount..];
}

public void ValidateId(string documentId)
{
    if (documentId.StartsWith("DOC-"))
    {
        string year = ExtractYear(documentId);
        _metrics.RecordYear(year);
    }
}
```

Expected format: `DOC-YYYY-NNN` (minimum 12 characters). What assumptions break in production, and how do you fix parsing defensively?

---

#### Q4. (R) A teammate refactors chunk parsing to overlap I/O with processing. The build fails and code review finds async/ref-struct mixing. Review:

```csharp
public async Task<int> ProcessHeaderAsync(string headerLine)
{
    ReadOnlySpan<char> idSpan = headerLine.AsSpan();

    using DocumentChunkReader chunkReader = new DocumentChunkReader(idSpan);

    await Task.Delay(10);

    return chunkReader.VisibleLength;
}

public ref struct DocumentChunkReader
{
    private ReadOnlySpan<char> _buffer;

    public DocumentChunkReader(ReadOnlySpan<char> buffer) => _buffer = buffer;

    public int VisibleLength => _buffer.Length;

    public void Dispose() => _buffer = ReadOnlySpan<char>.Empty;
}
```

What C# 8 rules are violated, and what refactor preserves both async I/O and span-based parsing?

---

#### Q5. (P) Your team ships `IDocumentProcessor` as a shared NuGet package. Version 1 has `Process` and `ProcessorName`. Version 2 needs a `Describe()` helper without forcing every consumer to recompile. A consumer already implements both `IDocumentProcessor` and `IArchiveReporter`, each adding a default `Describe()`. How do you evolve the interface using C# 8 default interface methods, and what breaks if you ignore diamond ambiguity?

---

#### Q6. (M) A nightly batch opens thousands of small files under a shared directory. After migrating to `using var`, ops reports "too many open files" and memory climbs until the job finishes. Review the loop:

```csharp
public BatchResult IngestFolder(string rootPath)
{
    var result = new BatchResult();
    IEnumerable<string> paths = Directory.EnumerateFiles(rootPath, "*.meta");

    foreach (string path in paths)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);

        DocumentMetadata? meta = JsonSerializer.Deserialize<DocumentMetadata>(reader.ReadToEnd());
        if (meta is not null)
        {
            result.Accept(meta);
        }
    }

    return result;
}
```

The developer expected each file handle to close after each iteration. What does C# 8 `using var` actually do here, and how do you fix it?

---

#### Q7. (D) A 400-project solution enables nullable reference types repo-wide. CI surfaces 8,000 warnings; developers blanket `#nullable disable` on touched files and sprinkle `!` to merge PRs. As tech lead, what migration strategy do you recommend for a document-archive domain with heavy `string` metadata (IDs, paths, optional notes), and where do `string?`, null checks, and `[NotNullWhen]` belong versus suppressions?
