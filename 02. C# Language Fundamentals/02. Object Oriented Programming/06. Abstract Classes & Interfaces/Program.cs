/*
 * =============================================================================
 * 06. ABSTRACT CLASSES AND INTERFACES IN C#
 * =============================================================================
 *
 * TOPIC: Abstraction mechanisms — abstract base classes that share partial
 *        implementation, and interfaces that define cross-cutting contracts.
 *
 * WHY IT MATTERS:
 *   Large applications mix shared behavior (a Document base with common fields)
 *   with optional capabilities (export, print, audit). Abstract classes and
 *   interfaces let you model both without duplicating code or locking types into
 *   a single inheritance chain. You program against abstractions so services
 *   stay testable and open to new document types.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Abstract classes, abstract methods, and abstract properties
 *   2.  Interface declaration and implicit implementation
 *   3.  When to choose abstract class vs interface
 *   4.  Implementing multiple interfaces on one class
 *   5.  Explicit interface implementation
 *   6.  Using abstractions in application code (processor and export services)
 *   7.  Preview: default interface methods (C# 8)
 *   8.  Preview: real-world interface patterns
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AbstractClassesAndInterfaces;

/*
 * =========================================================================
 * SECTION 1: ABSTRACT CLASSES AND ABSTRACT METHODS
 * =========================================================================
 *
 * An ABSTRACT CLASS is a base type that cannot be instantiated directly.
 * It may contain:
 *   • Concrete members (fields, properties, methods with bodies)
 *   • ABSTRACT members — declared without a body; subclasses MUST override
 *
 * Syntax:
 *   public abstract class Document
 *   {
 *       public abstract string RenderContent();     // abstract method
 *       public abstract string DocumentKind { get; } // abstract property
 *       public string GetSummary() { ... }          // concrete helper
 *   }
 *
 * Subclass obligation:
 *   public class InvoiceDocument : Document
 *   {
 *       public override string RenderContent() { ... }
 *       public override string DocumentKind => "Invoice";
 *   }
 *
 * Compile errors:
 *   new Document("x", DateTime.Now)     → CS0144 cannot create abstract type
 *   class Bad : Document { }            → CS0534 must implement abstract members
 *
 * Scenario: a document archive stores invoices and reports. Both share Title
 * and CreatedOn but render body text differently — perfect for an abstract
 * Document base with a protected constructor subclasses call via : base(...).
 * -------------------------------------------------------------------------
 */
public abstract class Document
{
    public string Title { get; }
    public DateTime CreatedOn { get; }

    protected Document(string title, DateTime createdOn)
    {
        Title = title;
        CreatedOn = createdOn;
    }

    public abstract string DocumentKind { get; }

    public abstract string RenderContent();

    public string GetSummary()
    {
        return $"{Title} ({DocumentKind}, created {CreatedOn:yyyy-MM-dd})";
    }
}

/*
 * =========================================================================
 * SECTION 2: INTERFACE DECLARATION AND IMPLEMENTATION
 * =========================================================================
 *
 * An INTERFACE is a contract — a list of members implementing types must
 * provide. Before C# 8, interfaces did not hold instance state.
 *
 * Declaration:
 *   public interface IExportable
 *   {
 *       string Export(string format);
 *   }
 *
 * Implementation (class or struct):
 *   public class InvoiceDocument : Document, IExportable
 *   {
 *       public string Export(string format) { ... }
 *   }
 *
 * A type implements an interface implicitly when members are public and
 * signatures match. Call through the interface for capability-based APIs:
 *
 *   IExportable exportable = invoice;
 *   exportable.Export("pdf");
 *
 * Compile error if a member is missing:
 *   class Incomplete : IExportable { }  → CS0535 does not implement Export
 *
 * --- 2a. Interface naming ---
 *
 * Convention: prefix with I (IExportable, IPrintable). Interfaces describe
 * CAN-DO capabilities that may cut across unrelated class hierarchies.
 * -------------------------------------------------------------------------
 */
public interface IExportable
{
    string Export(string format);
}

public interface IPrintable
{
    string Print();
}

/*
 * =========================================================================
 * SECTION 3: ABSTRACT CLASS VS INTERFACE — WHEN TO USE WHICH
 * =========================================================================
 *
 * | Choose…           | When…                                              |
 * |-------------------|----------------------------------------------------|
 * | Abstract class    | IS-A relationship + shared state or concrete helpers |
 * | Interface         | CAN-DO capability; unrelated types share a contract |
 * | Both              | Base class for common data; interfaces for extras   |
 *
 * Rules of thumb:
 *   • C# allows ONE base class but MANY interfaces.
 *   • Prefer abstract class when subclasses truly extend one hierarchy
 *     (Document → InvoiceDocument, ReportDocument).
 *   • Prefer interface when capability cuts across hierarchies (anything
 *     IExportable, even outside Document).
 *   • Abstract class can have protected members; interfaces expose public
 *     contract members (explicit impl can hide them — Section 5).
 *
 * This chapter uses BOTH: Document base for shared title/date + rendering,
 * IExportable/IPrintable for optional cross-cutting features.
 *
 * COVERED IN DETAIL LATER → 05. Inheritance and Polymorphism (virtual/override,
 *   runtime polymorphism on concrete base classes)
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 4: IMPLEMENTING MULTIPLE INTERFACES
 * =========================================================================
 *
 * A class lists one base class (optional) then comma-separated interfaces:
 *
 *   public class InvoiceDocument : Document, IExportable, IPrintable, INamedDocument
 *
 * Each interface adds obligations. The type satisfies all contracts simultaneously.
 * This models MULTIPLE INHERITANCE OF BEHAVIOR (not of implementation state —
 * C# still has single inheritance for classes).
 *
 * --- 4a. INamedDocument — optional naming contract ---
 * --- 4b. InvoiceDocument — three interfaces plus abstract base ---
 * --- 4c. ReportDocument — two interfaces, no INamedDocument ---
 * -------------------------------------------------------------------------
 */
public interface INamedDocument
{
    string GetName();
}

public class InvoiceDocument : Document, IExportable, IPrintable, INamedDocument
{
    public decimal Amount { get; }
    public string Customer { get; }

    public InvoiceDocument(string title, DateTime createdOn, decimal amount, string customer)
        : base(title, createdOn)
    {
        Amount = amount;
        Customer = customer;
    }

    public override string DocumentKind => "Invoice";

    public override string RenderContent()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "Invoice {0} — customer {1}, amount {2:C}",
            Title,
            Customer,
            Amount);
    }

    public string Export(string format)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "[{0}] {1} | {2:C} | {3}",
            format.ToUpperInvariant(),
            Title,
            Amount,
            Customer);
    }

    public string Print()
    {
        return "PRINT >> " + RenderContent();
    }

    public string GetName()
    {
        return "Invoice: " + Title;
    }

    /*
     * =========================================================================
     * SECTION 5: EXPLICIT INTERFACE IMPLEMENTATION
     * =========================================================================
     *
     * Normally interface members are public on the class. EXPLICIT implementation
     * prefixes the member with the interface name:
     *
     *   string INamedDocument.GetName() { return "file-safe-name.inv"; }
     *
     * The method is callable ONLY through the interface (or a cast):
     *
     *   invoice.GetName()                     → public display name
     *   ((INamedDocument)invoice).GetName()   → storage file name
     *
     * Use explicit implementation when:
     *   • Two interfaces declare the same signature (name clash)
     *   • The interface member should not pollute the public surface API
     *   • The interface contract differs subtly from the class's public method
     *
     * GetName() above returns a human label; INamedDocument.GetName() below
     * returns a file-safe name — both coexist because the interface member is explicit.
     * -------------------------------------------------------------------------
     */
    string INamedDocument.GetName()
    {
        return Title.Replace(' ', '-').ToLowerInvariant() + ".inv";
    }
}

public class ReportDocument : Document, IExportable, IPrintable
{
    public string Department { get; }
    public int PageCount { get; }

    public ReportDocument(string title, DateTime createdOn, string department, int pageCount)
        : base(title, createdOn)
    {
        Department = department;
        PageCount = pageCount;
    }

    public override string DocumentKind => "Report";

    public override string RenderContent()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "Report {0} — {1} department, {2} pages",
            Title,
            Department,
            PageCount);
    }

    public string Export(string format)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "[{0}] {1} | dept={2} | pages={3}",
            format.ToUpperInvariant(),
            Title,
            Department,
            PageCount);
    }

    public string Print()
    {
        return "PRINT >> " + RenderContent();
    }
}

/*
 * =========================================================================
 * SECTION 6: USING ABSTRACTIONS IN APPLICATION DEVELOPMENT
 * =========================================================================
 *
 * Production code depends on abstractions, not concrete types:
 *
 *   • DocumentProcessor accepts IReadOnlyList<Document> — any subclass works
 *   • ExportService accepts IEnumerable<IExportable> — only export capability matters
 *
 * Benefits:
 *   • Add new Document subclasses without rewriting processors
 *   • Unit tests inject fakes that implement the same interfaces
 *   • Services stay focused — ExportService does not need invoice fields
 * -------------------------------------------------------------------------
 */
public static class DocumentProcessor
{
    public static string BuildBatchSummary(IReadOnlyList<Document> documents)
    {
        StringBuilder builder = new StringBuilder();

        foreach (Document document in documents)
        {
            builder.AppendLine(document.GetSummary());
            builder.AppendLine(document.RenderContent());
        }

        return builder.ToString().TrimEnd();
    }
}

public static class ExportService
{
    public static string ExportAll(IEnumerable<IExportable> exportables, string format)
    {
        StringBuilder builder = new StringBuilder();

        foreach (IExportable exportable in exportables)
        {
            builder.AppendLine(exportable.Export(format));
        }

        return builder.ToString().TrimEnd();
    }
}

/*
 * =========================================================================
 * SECTION 7: PREVIEW — DEFAULT INTERFACE METHODS (C# 8)
 * =========================================================================
 *
 * COVERED IN DETAIL LATER → 08. Advanced C# Features / 06. C# 8 Features
 *   (headline concepts only: interfaces may define method bodies; implementers
 *    inherit the default unless they provide their own override)
 *
 * Before C# 8 every interface member had to be implemented by the type.
 * Default interface methods reduce boilerplate for optional behaviors.
 * -------------------------------------------------------------------------
 */
internal interface IPreviewDefaultBehavior
{
    string Describe()
    {
        return "Default interface method (C# 8+) — override optional.";
    }
}

internal sealed class PreviewExporter : IPreviewDefaultBehavior
{
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 8: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Fixed values so the program runs without keyboard input. Each block
     * exercises the sections above: abstract rendering, interface export/print,
     * type checks, multiple interfaces, explicit GetName, and service classes.
     *
     * COVERED IN DETAIL LATER → 09. OOP Real-World Examples
     *   (INotificationSender, payment processors, repository patterns)
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        DateTime runDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        InvoiceDocument invoice = new InvoiceDocument(
            "March Hosting",
            runDate,
            249.99m,
            "Contoso Ltd");

        ReportDocument report = new ReportDocument(
            "Q1 Sales",
            runDate,
            "Sales",
            42);

        Document[] archive = { invoice, report };

        string invoiceRender = invoice.RenderContent();
        string reportRender = report.RenderContent();
        string invoiceSummary = invoice.GetSummary();

        IExportable exportableInvoice = invoice;
        IPrintable printableReport = report;

        string pdfExport = exportableInvoice.Export("pdf");
        string printLine = printableReport.Print();

        bool invoiceIsDocument = invoice is Document;
        bool reportIsExportable = report is IExportable;
        bool reportIsReportDocument = report is ReportDocument;

        bool hasExport = invoice is IExportable;
        bool hasPrint = invoice is IPrintable;
        bool hasNamed = invoice is INamedDocument;
        bool reportHasNamed = report is INamedDocument;

        List<IExportable> exportQueue = new List<IExportable>();
        exportQueue.Add(invoice);
        exportQueue.Add(report);

        string publicDisplayName = invoice.GetName();
        INamedDocument namedContract = invoice;
        string storageFileName = namedContract.GetName();

        string batchSummary = DocumentProcessor.BuildBatchSummary(archive);
        string batchExport = ExportService.ExportAll(exportQueue, "json");

        string polymorphicLine = string.Empty;
        foreach (Document document in archive)
        {
            polymorphicLine = document.RenderContent();
        }

        IPreviewDefaultBehavior preview = new PreviewExporter();
        string defaultBehaviorText = preview.Describe();

        Console.WriteLine("=== Abstract Classes and Interfaces — Document Archive ===");
        Console.WriteLine();
        Console.WriteLine("--- Section 1: abstract rendering ---");
        Console.WriteLine(invoiceRender);
        Console.WriteLine(reportRender);
        Console.WriteLine("Summary: " + invoiceSummary);
        Console.WriteLine();
        Console.WriteLine("--- Section 2: interface export / print ---");
        Console.WriteLine(pdfExport);
        Console.WriteLine(printLine);
        Console.WriteLine();
        Console.WriteLine("--- Section 3: type checks (abstract vs interface) ---");
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "invoice is Document={0}, report is IExportable={1}, report is ReportDocument={2}",
            invoiceIsDocument,
            reportIsExportable,
            reportIsReportDocument));
        Console.WriteLine();
        Console.WriteLine("--- Section 4: multiple interfaces ---");
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "invoice: export={0}, print={1}, named={2}; report has INamedDocument={3}; queue={4}",
            hasExport,
            hasPrint,
            hasNamed,
            reportHasNamed,
            exportQueue.Count));
        Console.WriteLine();
        Console.WriteLine("--- Section 5: explicit vs public GetName ---");
        Console.WriteLine("Public:   " + publicDisplayName);
        Console.WriteLine("Explicit: " + storageFileName);
        Console.WriteLine();
        Console.WriteLine("--- Section 6: application services ---");
        Console.WriteLine(batchSummary);
        Console.WriteLine("--- exports ---");
        Console.WriteLine(batchExport);
        Console.WriteLine("Last polymorphic render: " + polymorphicLine);
        Console.WriteLine();
        Console.WriteLine("--- Section 7: default interface method preview ---");
        Console.WriteLine(defaultBehaviorText);
        Console.WriteLine();
        Console.WriteLine("--- Section 8: real-world patterns ---");
        Console.WriteLine("See chapter 09 for INotificationSender-style examples.");
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — ABSTRACT CLASSES AND INTERFACES
 * =============================================================================
 *
 * --- Abstract class ---
 *
 *  public abstract class Base
 *  {
 *      public abstract void MustOverride();
 *      public abstract string Kind { get; }
 *      public void SharedHelper() { }
 *  }
 *  public class Derived : Base
 *  {
 *      public override void MustOverride() { }
 *      public override string Kind => "Derived";
 *  }
 *
 *  new Base()  → CS0144
 *  class X : Base { } without overrides → CS0534
 *
 * --- Interface ---
 *
 *  public interface IContract
 *  {
 *      ReturnType Member(params);
 *  }
 *  public class Worker : IContract
 *  {
 *      public ReturnType Member(params) { }
 *  }
 *
 *  Missing member → CS0535
 *
 * --- Abstract class vs interface ---
 *
 *  Abstract class     | Interface
 *  -------------------|------------------------------------------
 *  Single inheritance | Multiple interfaces per class
 *  Shared state OK    | Contract only (no instance fields pre-C# 8)
 *  IS-A hierarchy     | CAN-DO capability across types
 *
 * --- Multiple interfaces ---
 *
 *  class T : BaseClass, IOne, ITwo { ... }
 *
 * --- Explicit implementation ---
 *
 *  ReturnType IOne.Clash() { ... }   // call via (IOne)obj or interface variable
 *
 * --- Application pattern ---
 *
 *  void Process(IEnumerable<IExportable> items)  // depend on abstraction
 *  void Render(Document doc)                     // polymorphic base call
 *
 * --- Preview (later chapters) ---
 *
 *  Default interface methods  → 08. Advanced C# Features / 06. C# 8 Features
 *  Real-world examples        → 09. OOP Real-World Examples
 *
 * =============================================================================
 */
