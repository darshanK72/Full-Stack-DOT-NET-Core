# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/06. Abstract Classes & Interfaces`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (D) Your team is adding a `SpreadsheetDocument` to the document archive. It shares `Title` and `CreatedOn` with invoices and reports, but also needs optional CSV export and a separate audit trail that other document types may never use. A junior dev proposes making everything an interface:

```csharp
public interface ISpreadsheetDocument
{
    string Title { get; }
    DateTime CreatedOn { get; }
    string RenderContent();
    string Export(string format);
    void WriteAuditEntry(string action);
}
```

How would you model this using abstract classes and interfaces (as in this chapter), and why?

---

#### Q2. (R) A storage service saves file names for archived documents. After deployment, some invoices overwrite each other on disk. Review:

```csharp
public sealed class InvoiceStorageService
{
    public string ResolveFileName(InvoiceDocument invoice)
    {
        // Human-readable label for UI and logs
        return invoice.GetName();
    }

    public void Save(InvoiceDocument invoice, Stream content)
    {
        string path = Path.Combine(_root, ResolveFileName(invoice));
        using var file = File.Create(path);
        content.CopyTo(file);
    }
}
```

`InvoiceDocument` implements `INamedDocument` with explicit `string INamedDocument.GetName()` returning a file-safe name, and a public `GetName()` returning `"Invoice: " + Title`. What is wrong, and how do you fix it?

---

#### Q3. (R) A PR introduces a "kitchen sink" capability interface for the export pipeline. Review:

```csharp
public interface IDocumentCapabilities
{
    string Export(string format);
    string Print();
    string GetName();
    byte[] RenderPdf();
    void SendToPrinter(string queueName);
    string SignWithCertificate(string thumbprint);
}

public class ExportOrchestrator
{
    public void RunBatch(IEnumerable<IDocumentCapabilities> items, string format)
    {
        foreach (var item in items)
        {
            _logger.LogInformation(item.Export(format));
        }
    }
}
```

Only invoices need signing; reports only export. What design problems do you see, and how would you refactor?

---

#### Q4. (M) The team ships a NuGet package with `IExportable` consumed by ten internal services. To add optional metadata without breaking implementers, they add a C# 8 default method:

```csharp
public interface IExportable
{
    string Export(string format);

    string ExportWithMetadata(string format)
    {
        return Export(format) + " | exported=" + DateTime.UtcNow.ToString("O");
    }
}
```

An older service still targets `netstandard2.0` and references the updated package. A newer ASP.NET Core service on `net8.0` overrides `ExportWithMetadata` in one document type. What breaks or surprises you in build, runtime, and testing — and what would you document for consumers?

---

#### Q5. (R) Unit tests for `DocumentProcessor` are slow and require real PDF files on disk because production code was wired to concrete types. Review:

```csharp
public sealed class DocumentProcessor
{
    private readonly PdfRenderer _renderer = new PdfRenderer(); // reads templates from disk

    public string BuildBatchSummary(IReadOnlyList<InvoiceDocument> documents)
    {
        var builder = new StringBuilder();
        foreach (var doc in documents)
        {
            builder.AppendLine(doc.GetSummary());
            builder.AppendLine(_renderer.Render(doc)); // not on Document base
        }
        return builder.ToString();
    }
}
```

The chapter's `DocumentProcessor` accepts `IReadOnlyList<Document>` and `ExportService` accepts `IEnumerable<IExportable>`. What is wrong here, and how would you introduce test seams?

---

#### Q6. (D) Code review: two approaches for a payment-notification feature.

**Option A — one abstract base:**

```csharp
public abstract class NotifierBase
{
    public abstract void Send(string recipient, string message);
    protected void LogAttempt(string recipient) { /* shared */ }
}
```

**Option B — interface only:**

```csharp
public interface INotificationSender
{
    void Send(string recipient, string message);
}
```

Some notifiers are `EmailNotifier : NotifierBase`; others are `SmsNotifier : INotificationSender` with no shared base. When do you pick abstract class, interface, or both — and what is your decision rule for this codebase?

---
