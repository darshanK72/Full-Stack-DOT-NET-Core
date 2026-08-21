# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/06. Abstract Classes & Interfaces`

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

**Answer:** Keep the **IS-A** document hierarchy on an abstract `Document` base for shared state and rendering contract, then add **CAN-DO** interfaces only for optional capabilities — `IExportable` for export, a narrow `IAuditable` (or similar) for audit — instead of one fat document interface.

- **Abstract `Document`:** `Title`, `CreatedOn`, protected constructor, abstract `DocumentKind` and `RenderContent()`, plus concrete `GetSummary()` — matches **Program.cs** Sections 1 and 3; `SpreadsheetDocument : Document` reuses helpers without duplicating fields.
- **`IExportable`:** Export is a cross-cutting capability; invoices, reports, and spreadsheets can implement it without forcing audit on types that do not need it.
- **`IAuditable` (small interface):** Only types that write audit entries implement `WriteAuditEntry`; reports that never audit are not forced to stub empty methods.
- **Why not one interface:** Duplicates state across unrelated "documents," blocks multiple inheritance of implementation, and violates Interface Segregation — consumers that only export must know about audit members.
- **Both together:** `class SpreadsheetDocument : Document, IExportable, IAuditable` — single class hierarchy, multiple optional behaviors, same pattern as `InvoiceDocument : Document, IExportable, IPrintable, INamedDocument`.

**Production takeaway:** Abstract class for shared identity and partial implementation; interfaces for capabilities that cut across hierarchies. See this chapter's **Document** + **IExportable** split and foundation **Abstract class vs interface** table.

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

**Answer:** The storage service calls the **public** `GetName()` (display label with spaces and punctuation), not the **explicit** `INamedDocument.GetName()` (file-safe slug) — two invoices with the same title collide on disk because paths are not unique or filesystem-safe.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ResolveFileName` uses public `GetName()` instead of `INamedDocument` contract | Duplicate paths; overwrites; invalid characters on some OSes |
| API surface | Explicit implementation is invisible on concrete type | Callers assume one `GetName()` — easy to pick the wrong one |
| Design | Storage depends on concrete `InvoiceDocument` | Harder to test; wrong abstraction for "file naming" capability |

**Fix (priority order):**

1. Resolve names through the interface: `((INamedDocument)invoice).GetName()` or accept `INamedDocument` / `IFileNaming` in `ResolveFileName`.
2. Add uniqueness: append document id or hash if titles can repeat — explicit slug alone may still collide.
3. Rename public method to `GetDisplayName()` if both names must coexist on the type — reduces accidental misuse.
4. Unit-test storage with two invoices sharing a title; assert distinct file paths.

```csharp
public string ResolveFileName(INamedDocument named)
{
    return named.GetName(); // explicit implementation invoked via interface
}
```

**Production takeaway:** Explicit interface implementation exists precisely when the public API and contract differ — services must depend on the **interface variable**, as **Program.cs** Section 5 demonstrates with `namedContract.GetName()` vs `invoice.GetName()`.

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

**Answer:** `IDocumentCapabilities` is a **fat interface** that violates the **Interface Segregation Principle** — every implementer must stub or throw for unrelated members, and callers cannot express minimal dependencies.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design (ISP) | One interface bundles export, print, PDF, signing, naming | `ReportDocument` forced to implement `SignWithCertificate` with `NotSupportedException` |
| Maintainability | New capability added to interface breaks all implementers | Package version churn; empty stubs multiply |
| Testing | Fakes must implement six methods to test export-only orchestrator | Bloated test doubles; brittle mocks |
| API clarity | `ExportOrchestrator` only needs `Export` but depends on mega-contract | Misleading type bounds; hides true requirements |

**Fix (priority order):**

1. Split into focused interfaces — `IExportable`, `IPrintable`, `ISignable`, `INamedDocument` — matching this chapter's pattern.
2. Change orchestrator signature to `IEnumerable<IExportable>` (as **ExportService.ExportAll** does).
3. Compose at call site: pass types that implement multiple interfaces; use pattern matching or separate services for signing/printing steps.
4. If a facade is needed for DI registration, use a small adapter per document type — not a monolithic interface.

**Production takeaway:** Prefer several small interfaces over one "capabilities" blob — callers depend on what they use, implementers only provide what they support. See **Program.cs** Section 4 (`InvoiceDocument` implements three interfaces, not one fat type).

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

**Answer:** Default interface methods require **C# 8+** and a runtime that supports them — `netstandard2.0` consumers may **fail to compile** or cannot override defaults the same way; even on modern runtimes, dispatch through the interface vs concrete type can surprise callers who expect polymorphic override behavior.

- **Build / TFM:** Default interface members are not available on older language/runtime combinations targeting pre-C#-8 projects — the package bump may block the legacy service until it retargets or the new member is moved to an extension method or separate `IExportableV2`.
- **Binary compatibility:** Adding a default method is often safer than adding a **required** abstract member (which breaks all implementers), but implementers on C# 8+ can override — document which types customize metadata vs inherit default.
- **Dispatch nuance:** Calling `ExportWithMetadata` on `IExportable` uses the most specific override on the implementing type; calling on concrete class without override uses default — tests must use the same reference type production uses.
- **Testing:** Fakes implementing `IExportable` inherit the default unless they override — unit tests may accidentally assert timestamp behavior from the default implementation instead of domain logic.
- **Alternative for wide compatibility:** Extension method `ExportWithMetadata(this IExportable e, ...)` or compositional wrapper — works on `netstandard2.0` without DIM.

**Production takeaway:** Default interface methods help evolve shared contracts with optional behavior (**Program.cs** Section 7 preview), but package authors must treat TFMs, override rules, and test doubles as part of the public API — not every consumer upgrades language version with the package.

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

**Answer:** The processor **news up** a concrete `PdfRenderer`, accepts only `InvoiceDocument`, and mixes summary building with PDF rendering — no injection point, so tests hit disk and cannot substitute a fake.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Testability | `new PdfRenderer()` inside the class | Tests require filesystem templates; slow, flaky CI |
| Abstraction | Parameter is `InvoiceDocument` not `Document` | Cannot reuse batch logic for reports; breaks polymorphism |
| SRP / design | Summary builder coupled to PDF rendering | Changing render strategy forces retesting batch orchestration |
| DI | Hidden dependency | ASP.NET Core cannot register or swap renderer per environment |

**Fix (priority order):**

1. Extract rendering behind an interface — `IDocumentRenderer` or reuse `IExportable` / a narrow `IRenderable` with `string Render()` — inject via constructor.
2. Accept abstractions on the base type: `IReadOnlyList<Document>` for polymorphic summaries (**Program.cs** Section 6).
3. Register `PdfRenderer` in DI for production; register `FakeRenderer` in tests returning fixed strings.
4. Keep orchestration thin — `BuildBatchSummary` calls `document.GetSummary()` and `document.RenderContent()` on the abstract base where possible; PDF-specific work lives in export/render services.

```csharp
public sealed class DocumentProcessor
{
    private readonly IDocumentRenderer _renderer;

    public DocumentProcessor(IDocumentRenderer renderer) => _renderer = renderer;

    public string BuildBatchSummary(IReadOnlyList<Document> documents)
    {
        var builder = new StringBuilder();
        foreach (var doc in documents)
        {
            builder.AppendLine(doc.GetSummary());
            builder.AppendLine(_renderer.Render(doc));
        }
        return builder.ToString();
    }
}
```

**Production takeaway:** Interfaces are test seams — depend on abstractions, inject implementations. The chapter's static `DocumentProcessor` / `ExportService` illustrate the **dependency direction**; production services add constructor injection and fakes for fast tests.

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

**Answer:** Use an **abstract base** when notifiers truly share state or concrete helpers (logging, retry policy, template loading); use an **interface** when the only contract is "can send" across unrelated types; use **both** when shared infrastructure belongs in a base but multiple channels must also be substitutable in DI and tests.

**Decision rule (aligned with this chapter):**

| Signal | Choose |
|---|---|
| Shared fields, protected helpers, single IS-A hierarchy | Abstract class (`NotifierBase`) |
| Unrelated types (email, SMS, webhook) must be swappable | `INotificationSender` interface |
| Shared logging/retry **and** need multiple inheritance of behavior | Base class for shared code + `INotificationSender` implemented by base or subclasses |
| Only some notifiers support attachments/signing | Separate small interfaces — do not bloated base |

**For this codebase:**

- **`INotificationSender`** for DI registration, controllers, and unit tests — same role as `IExportable` in **ExportService**.
- **`NotifierBase`** only if most channels share `LogAttempt`, correlation id, or configuration — avoid forcing SMS through an email-centric hierarchy.
- **`SmsNotifier : INotificationSender`** without base is valid when there is nothing to share — do not invent an abstract class for one method.

**Production takeaway:** Abstract class answers "what are they in common?" Interface answers "what can they do for me?" The chapter's **Document** + **IExportable** combination is the template — base for identity, interfaces for pluggable capabilities and test doubles.

---
