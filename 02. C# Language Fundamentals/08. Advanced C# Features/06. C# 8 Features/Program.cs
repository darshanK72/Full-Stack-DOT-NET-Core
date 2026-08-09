/*
 * =============================================================================
 * 06. C# 8 FEATURES — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: C# 8 language additions — nullable reference types, async streams,
 *        pattern-matching upgrades, using declarations, indices/ranges, and
 *        other productivity features shipped with .NET Core 3 / C# 8.
 *
 * WHY IT MATTERS:
 *   C# 8 is the release that made null-safety annotations first-class, added
 *   IAsyncEnumerable for streaming data, and expanded pattern matching into
 *   everyday business logic. Modern .NET codebases rely on these features for
 *   safer APIs, cleaner resource disposal, and expressive conditionals. This
 *   chapter walks a single document-archive workflow so every feature appears
 *   in realistic context rather than isolated snippets.
 *
 * WHAT YOU WILL LEARN:
 *   1.  C# 8 overview — what shipped and how it fits the language timeline
 *   2.  Readonly structs — immutable value types without defensive copies
 *   3.  Default interface methods — shared behavior on interfaces
 *   4.  Pattern matching enhancements — switch expressions, property patterns
 *   5.  Using declarations — scoped disposal without a nested block
 *   6.  Static local functions — helpers that cannot capture outer state
 *   7.  Disposable ref structs — stack-only types that implement IDisposable
 *   8.  Nullable reference types — compile-time null tracking for references
 *   9.  IAsyncEnumerable / async streams — async foreach over sequences
 *  10.  IAsyncDisposable — async cleanup with await using
 *  11.  Indices and ranges — ^n indexing and start..end slicing
 *  12.  Null-coalescing assignment ??= — assign only when null
 *  13.  Unmanaged constructed types (preview)
 *  14.  stackalloc in nested expressions (preview)
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp8Features;

/*
 * =========================================================================
 * SECTION 1 (supporting types): DOCUMENT KIND — ROUTING DISCRIMINATOR
 * =========================================================================
 *
 * DocumentKind classifies archive items for pattern-matching demos in Section 4.
 * Enums are value types; each batch row carries one Kind alongside metadata.
 * -------------------------------------------------------------------------
 */
public enum DocumentKind
{
    Text,
    Markup,
    Binary
}

/*
 * =========================================================================
 * SECTION 8: NULLABLE REFERENCE TYPES — METADATA WITH OPTIONAL NOTES
 * =========================================================================
 *
 * With <Nullable>enable</Nullable> in the .csproj, reference types are
 * non-nullable by default. Append ? to allow null; the compiler warns on
 * possible null dereferences.
 *
 * Annotation examples on this type:
 *
 *   string Id            — expected never null (non-nullable default)
 *   string? Notes        — may be null (explicit nullable annotation)
 *
 * Common suppressions (use deliberately, not to silence real bugs):
 *   • Null-forgiving operator: value!  — "I know this is not null here"
 *   • [NotNullWhen(true)] on Try-pattern out parameters (library code)
 *
 * Compile warnings you may see while learning NRT:
 *   CS8600 — converting null literal to non-nullable type
 *   CS8602 — dereference of a possibly null reference
 *   CS8618 — non-nullable field not initialized in constructor
 *
 * #nullable disable / #nullable restore — opt a file region out of NRT
 * (use sparingly when migrating legacy code).
 * -------------------------------------------------------------------------
 */
public sealed class DocumentMetadata
{
    public string Id { get; set; } = string.Empty; // non-nullable — must have a value
    public DocumentKind Kind { get; set; }
    public int PageCount { get; set; }
    public string? Notes { get; set; }             // nullable — optional footnote per document
}

/*
 * =========================================================================
 * SECTION 2: READONLY STRUCTS
 * =========================================================================
 *
 * readonly struct (C# 7.2 field rule, C# 8 ecosystem push) marks every
 * instance field as read-only. The compiler blocks mutating members and
 * avoids silent copies when you pass the struct with in/ref readonly.
 *
 * Use when: small immutable value types (coordinates, spans, money tuples).
 *
 *   readonly struct PageSpan
 *   {
 *       public int Start { get; }   // implicitly readonly
 *       public int Length { get; }
 *   }
 * -------------------------------------------------------------------------
 */
public readonly struct PageSpan
{
    public int Start { get; }
    public int Length { get; }

    public PageSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public int End => Start + Length;

    public override string ToString() => $"pages [{Start}..{End})";
}

/*
 * =========================================================================
 * SECTION 3: DEFAULT INTERFACE METHODS
 * =========================================================================
 *
 * Interfaces can now carry method bodies. Implementers inherit the default
 * unless they declare their own version — similar to Java 8 default methods.
 *
 * Benefits:
 *   • Add behavior to existing interfaces without breaking implementers
 *   • Share utility code at the abstraction boundary
 *
 * Limitations:
 *   • Implementers must be public when the interface method is public
 *   • Diamond ambiguity if two interfaces define the same default
 * -------------------------------------------------------------------------
 */
public interface IDocumentProcessor
{
    string ProcessorName { get; }

    // Default interface method — concrete behavior on an interface (C# 8).
    string Describe() => $"{ProcessorName} processor (default Describe)";

    string Process(string content);
}

public sealed class TextDocumentProcessor : IDocumentProcessor
{
    public string ProcessorName => "Text";

    public string Process(string content) => content.Trim().ToUpperInvariant();
}

public sealed class MarkupDocumentProcessor : IDocumentProcessor
{
    public string ProcessorName => "Markup";

    // Optional override of the default interface method.
    public string Describe() => $"{ProcessorName} processor (custom Describe)";

    public string Process(string content) =>
        content.Replace("<p>", string.Empty).Replace("</p>", "\n");
}

/*
 * =========================================================================
 * SECTION 7: DISPOSABLE REF STRUCTS
 * =========================================================================
 *
 * ref struct types are stack-only (Span<T>, ReadOnlySpan<T>). C# 8 allows
 * them to implement IDisposable so you can write:
 *
 *   using var reader = new DocumentChunkReader(span);
 *
 * Restrictions recap:
 *   • Cannot be boxed, stored in object fields, or implement arbitrary interfaces
 *   • Cannot be used across await boundaries (async state machine restriction)
 *   • Dispose order follows using declarations at end of scope
 * -------------------------------------------------------------------------
 */
public ref struct DocumentChunkReader
{
    private ReadOnlySpan<char> _buffer;
    private bool _disposed;

    public DocumentChunkReader(ReadOnlySpan<char> buffer)
    {
        _buffer = buffer;
        _disposed = false;
    }

    public int VisibleLength => _disposed ? 0 : _buffer.Length;

    public ReadOnlySpan<char> Peek() => _disposed ? ReadOnlySpan<char>.Empty : _buffer;

    public void Dispose()
    {
        _disposed = true;
        _buffer = ReadOnlySpan<char>.Empty;
    }
}

/*
 * =========================================================================
 * SECTIONS 9–10: IAsyncEnumerable AND IAsyncDisposable
 * =========================================================================
 *
 * IAsyncEnumerable<T> lets an async method yield items with await between
 * them. Consumers use await foreach.
 *
 * IAsyncDisposable complements IDisposable for async cleanup:
 *   await using var resource = …;
 *
 * [EnumeratorCancellation] forwards a CancellationToken into the async
 * iterator when the consumer passes one to WithCancellation().
 * -------------------------------------------------------------------------
 */
public sealed class AsyncDocumentStream : IAsyncDisposable
{
    private readonly IReadOnlyList<string> _pages;
    private bool _disposed;

    public AsyncDocumentStream(IReadOnlyList<string> pages)
    {
        _pages = pages;
    }

    public async IAsyncEnumerable<string> ReadPagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (string page in _pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(5, cancellationToken);
            yield return page;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await Task.Delay(5);
    }
}

/*
 * =========================================================================
 * SECTION 13: UNMANAGED CONSTRUCTED TYPES (PREVIEW)
 * =========================================================================
 *
 * C# 8 lets unmanaged types include generic instantiations where every
 * type argument is also unmanaged (e.g. Span<GridPoint>, pointers to T).
 *
 * COVERED IN DETAIL LATER → low-level memory / unsafe interop topics
 *   (headline concepts only: unmanaged constraint, sizeof, stackalloc T[n],
 *    Span<T> over structs, ref struct rules)
 * -------------------------------------------------------------------------
 */
public struct GridPoint
{
    public int X { get; set; }
    public int Y { get; set; }

    public override string ToString() => $"({X},{Y})";
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 1: C# 8 OVERVIEW — DOCUMENT ARCHIVE BATCH
     * =========================================================================
     *
     * C# 8 shipped with .NET Core 3.0 (2019). Major themes:
     *
     *   Theme                    | Representative features
     *   -------------------------|--------------------------------------------
     *   Null safety              | Nullable reference types (NRT)
     *   Async iteration          | IAsyncEnumerable, IAsyncDisposable
     *   Pattern matching         | Switch expressions, property/tuple patterns
     *   Resource management      | using declarations, disposable ref structs
     *   Value-type ergonomics    | Readonly structs, indices/ranges
     *   Interface evolution      | Default interface methods
     *   Low-level performance    | Unmanaged constructed types, stackalloc
     *
     * This program simulates an archive service ingesting three documents.
     * Each later section applies one C# 8 feature to the same batch.
     * -------------------------------------------------------------------------
     */

    public static async Task Main(string[] args)
    {
        DocumentMetadata[] batch = new DocumentMetadata[]
        {
            new DocumentMetadata
            {
                Id = "DOC-2024-001",
                Kind = DocumentKind.Text,
                PageCount = 120,
                Notes = "Annual report"
            },
            new DocumentMetadata
            {
                Id = "DOC-2024-002",
                Kind = DocumentKind.Markup,
                PageCount = 8,
                Notes = null
            },
            new DocumentMetadata
            {
                Id = "DOC-2024-003",
                Kind = DocumentKind.Binary,
                PageCount = 1,
                Notes = "Firmware image"
            },
        };

        Console.WriteLine("=== C# 8 Features — Document Archive Batch ===");
        foreach (DocumentMetadata doc in batch)
        {
            Console.WriteLine($"  {doc.Id} | {doc.Kind} | {doc.PageCount} pages");
        }

        DemonstrateReadonlyStruct();
        DemonstrateDefaultInterfaceMethods();
        DemonstratePatternMatching(batch);
        DemonstrateUsingDeclarations(batch);
        DemonstrateStaticLocalFunctions(batch);
        DemonstrateDisposableRefStruct(batch[0].Id);
        DemonstrateNullableReferenceTypes(batch);
        await DemonstrateAsyncStreamsAsync();
        DemonstrateIndicesAndRanges(batch);
        DemonstrateNullCoalescingAssignment(batch);
        DemonstrateLowLevelPreview();

        Console.WriteLine();
        Console.WriteLine("=== Batch processing complete ===");
    }

    private static void DemonstrateReadonlyStruct()
    {
        PageSpan introduction = new PageSpan(start: 0, length: 3);
        PageSpan appendix = new PageSpan(start: 117, length: 3);

        Console.WriteLine();
        Console.WriteLine("--- Readonly struct: PageSpan ---");
        Console.WriteLine($"  Introduction: {introduction}");
        Console.WriteLine($"  Appendix:     {appendix}");
        Console.WriteLine($"  Combined end: {appendix.End}");
    }

    private static void DemonstrateDefaultInterfaceMethods()
    {
        IDocumentProcessor textProcessor = new TextDocumentProcessor();
        IDocumentProcessor markupProcessor = new MarkupDocumentProcessor();

        Console.WriteLine();
        Console.WriteLine("--- Default interface methods ---");
        Console.WriteLine($"  {textProcessor.Describe()}");
        Console.WriteLine($"  {markupProcessor.Describe()}");
        Console.WriteLine($"  Processed markup: {markupProcessor.Process("<p>Hello</p>")}".Trim());
    }

    /*
     * =========================================================================
     * SECTION 4: PATTERN MATCHING ENHANCEMENTS
     * =========================================================================
     *
     * C# 8 adds switch expressions and richer patterns on top of C# 7 matching.
     *
     * New in C# 8:
     *   • Switch expressions       value switch { … => … }
     *   • Property patterns        { Kind: DocumentKind.Text, PageCount: > 100 }
     *   • Tuple patterns           (DocumentKind.Text, _) => …
     *   • Positional patterns      (var x, var y) — with deconstruction
     *   • Recursive patterns       nested property patterns
     *
     * --- 4a. Switch expression with property patterns ---
     * --- 4b. Tuple pattern on (Kind, PageCount) ---
     * -------------------------------------------------------------------------
     */
    private static void DemonstratePatternMatching(DocumentMetadata[] batch)
    {
        Console.WriteLine();
        Console.WriteLine("--- Pattern matching: routing rules ---");

        foreach (DocumentMetadata doc in batch)
        {
            string route = doc switch
            {
                { Kind: DocumentKind.Text, PageCount: > 100 } => "heavy-text-queue",
                { Kind: DocumentKind.Markup } => "markup-queue",
                { Kind: DocumentKind.Binary, PageCount: 1 } => "binary-fast-lane",
                _ => "default-queue"
            };

            Console.WriteLine($"  {doc.Id} → {route}");
        }

        DocumentMetadata sample = batch[1];
        string priority = (sample.Kind, sample.PageCount) switch
        {
            (DocumentKind.Markup, var count) when count <= 10 => "low",
            (DocumentKind.Text, var count) when count > 50 => "high",
            _ => "normal"
        };

        Console.WriteLine($"  Tuple pattern priority for {sample.Id}: {priority}");
    }

    /*
     * =========================================================================
     * SECTION 5: USING DECLARATIONS
     * =========================================================================
     *
     * using var / using Type name = … disposes at the end of the enclosing
     * scope (usually the method), not at the end of a nested block.
     *
     * Before (C# 7):
     *
     *   using (var writer = new StringWriter())
     *   {
     *       writer.WriteLine("…");
     *       return writer.ToString();
     *   }
     *
     * After (C# 8):
     *
     *   using var writer = new StringWriter();
     *   writer.WriteLine("…");
     *   return writer.ToString();   // dispose runs here, before return
     *
     * Dispose order: reverse declaration order (last declared, first disposed).
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateUsingDeclarations(DocumentMetadata[] batch)
    {
        Console.WriteLine();
        Console.WriteLine("--- Using declarations ---");

        using var logWriter = new StringWriter();
        logWriter.WriteLine("Archive ingest log");
        logWriter.WriteLine($"Batch size: {batch.Length}");

        using var buffer = new MemoryStream();
        byte[] payload = Encoding.UTF8.GetBytes(logWriter.ToString() ?? string.Empty);
        buffer.Write(payload, 0, payload.Length);

        Console.WriteLine($"  Log bytes buffered: {buffer.Length}");
        // logWriter and buffer disposed at end of this method (reverse order)
    }

    /*
     * =========================================================================
     * SECTION 6: STATIC LOCAL FUNCTIONS
     * =========================================================================
     *
     * Local functions can be marked static so they cannot capture variables
     * from the enclosing method. That prevents accidental closure allocations
     * and makes data flow explicit through parameters.
     *
     * Without static: local function sees batch, args, etc. implicitly.
     * With static:    must pass everything in — clearer and often faster.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateStaticLocalFunctions(DocumentMetadata[] batch)
    {
        Console.WriteLine();
        Console.WriteLine("--- Static local functions ---");

        foreach (DocumentMetadata doc in batch)
        {
            if (!IsValidArchiveId(doc.Id))
            {
                Console.WriteLine($"  Rejected invalid id: {doc.Id}");
            }
        }

        Console.WriteLine("  All batch IDs passed validation.");

        static bool IsValidArchiveId(string id)
        {
            return id.Length >= 10 && id.StartsWith("DOC-", StringComparison.Ordinal);
        }
    }

    private static void DemonstrateDisposableRefStruct(string headerLine)
    {
        Console.WriteLine();
        Console.WriteLine("--- Disposable ref struct ---");

        ReadOnlySpan<char> idSpan = headerLine.AsSpan();

        using (DocumentChunkReader chunkReader = new DocumentChunkReader(idSpan))
        {
            ReadOnlySpan<char> peek = chunkReader.Peek();
            Console.WriteLine($"  Chunk length: {chunkReader.VisibleLength}");
            Console.WriteLine($"  Chunk text:   {peek.ToString()}");
        }
    }

    private static void DemonstrateNullableReferenceTypes(DocumentMetadata[] batch)
    {
        Console.WriteLine();
        Console.WriteLine("--- Nullable reference types ---");

        foreach (DocumentMetadata doc in batch)
        {
            string displayNote = doc.Notes ?? "(no notes)";
            Console.WriteLine($"  {doc.Id}: {displayNote}");
        }

        string? optionalTag = FindTag(batch[0]);
        if (optionalTag != null)
        {
            Console.WriteLine($"  Tag for first doc: {optionalTag}");
        }
    }

    private static async Task DemonstrateAsyncStreamsAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- IAsyncEnumerable / async streams ---");

        string[] pages = new[] { "Page 1: Summary", "Page 2: Details", "Page 3: Appendix" };
        await using AsyncDocumentStream documentStream = new AsyncDocumentStream(pages);

        await foreach (string page in documentStream.ReadPagesAsync())
        {
            Console.WriteLine($"  Streamed: {page}");
        }

        Console.WriteLine();
        Console.WriteLine("--- IAsyncDisposable ---");
        Console.WriteLine("  AsyncDocumentStream disposed via await using above.");
    }

    /*
     * =========================================================================
     * SECTION 11: INDICES AND RANGES
     * =========================================================================
     *
     * System.Index and System.Range (C# 8) enable slice syntax:
     *
     *   Index from start:  array[2]     array[^1] is last element
     *   Range slice:      array[1..4]  elements at 1,2,3 (end exclusive)
     *                     array[..3]   start through index 2
     *                     array[2..]   index 2 through end
     *                     array[^3..]  third-from-last through end
     *
     * Works on arrays, spans, and strings. Range end is exclusive (like loops).
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateIndicesAndRanges(DocumentMetadata[] batch)
    {
        Console.WriteLine();
        Console.WriteLine("--- Indices and ranges ---");

        string firstId = batch[0].Id;
        string yearPart = firstId[4..8];
        string suffix = firstId[^3..];
        Index lastCharIndex = ^1;

        Console.WriteLine($"  Full id:     {firstId}");
        Console.WriteLine($"  Year slice:  {yearPart}");
        Console.WriteLine($"  Last 3 chars:{suffix}");
        Console.WriteLine($"  Last char:   {firstId[lastCharIndex]}");

        int[] pageNumbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        int[] middlePages = pageNumbers[3..7];

        Console.WriteLine($"  Middle pages: {string.Join(", ", middlePages)}");
    }

    /*
     * =========================================================================
     * SECTION 12: NULL-COALESCING ASSIGNMENT ??=
     * =========================================================================
     *
     * x ??= y assigns y to x only when x is null. Equivalent to:
     *
     *   if (x is null) x = y;
     *
     * Typical use: lazy initialization of caches, options, or collections.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateNullCoalescingAssignment(DocumentMetadata[] batch)
    {
        Console.WriteLine();
        Console.WriteLine("--- Null-coalescing assignment ??= ---");

        Dictionary<string, int>? pageCountCache = null;

        foreach (DocumentMetadata doc in batch)
        {
            pageCountCache ??= new Dictionary<string, int>(StringComparer.Ordinal);
            pageCountCache[doc.Id] = doc.PageCount;
        }

        Console.WriteLine($"  Cache entries: {pageCountCache!.Count}");
        Console.WriteLine($"  DOC-2024-001 pages: {pageCountCache["DOC-2024-001"]}");
    }

    /*
     * =========================================================================
     * SECTION 14: stackalloc IN NESTED EXPRESSIONS (PREVIEW)
     * =========================================================================
     *
     * Before C# 8, stackalloc had to be a standalone statement. C# 8 allows
     * stackalloc inside expressions — arguments, initializers, returns.
     *
     * COVERED IN DETAIL LATER → performance / Span-focused chapters
     *   (headline concepts only: stack-only allocation, Span bridge, limit size)
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateLowLevelPreview()
    {
        Console.WriteLine();
        Console.WriteLine("--- Preview: unmanaged constructed types ---");

        Span<GridPoint> corners = stackalloc GridPoint[4];
        corners[0] = new GridPoint { X = 0, Y = 0 };
        corners[1] = new GridPoint { X = 100, Y = 0 };
        corners[2] = new GridPoint { X = 100, Y = 200 };
        corners[3] = new GridPoint { X = 0, Y = 200 };

        Console.WriteLine($"  Bounding box corner A: {corners[0]}");
        Console.WriteLine($"  Bounding box corner C: {corners[2]}");

        Console.WriteLine();
        Console.WriteLine("--- Preview: stackalloc in nested expressions ---");

        int byteSum = SumBytes(stackalloc byte[] { 10, 20, 30, 40 });
        Console.WriteLine($"  Byte checksum: {byteSum}");
    }

    /*
     * Nullable return — caller must check for null before dereferencing.
     */
    private static string? FindTag(DocumentMetadata doc)
    {
        return doc.Notes != null ? doc.Notes.ToUpperInvariant() : null;
    }

    private static int SumBytes(ReadOnlySpan<byte> data)
    {
        int total = 0;
        foreach (byte b in data)
        {
            total += b;
        }

        return total;
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — C# 8 FEATURES
 * =============================================================================
 *
 * --- Nullable reference types ---
 *
 *   string name;       // non-nullable by default (with Nullable enable)
 *   string? note;      // may be null
 *   value!             // null-forgiving (suppress warning)
 *
 * --- Switch expression ---
 *
 *   var label = x switch { 1 => "one", 2 => "two", _ => "other" };
 *
 * --- Property / tuple patterns ---
 *
 *   doc switch { { Kind: Text, PageCount: > 10 } => … }
 *   (kind, count) switch { (Text, var n) when n > 10 => … }
 *
 * --- using declaration ---
 *
 *   using var stream = File.OpenRead(path);
 *   // disposed at end of scope, reverse order for multiple usings
 *
 * --- await using (IAsyncDisposable) ---
 *
 *   await using var conn = OpenAsync();
 *   await foreach (var item in conn.Stream()) { … }
 *
 * --- Indices / ranges ---
 *
 *   array[^1]      last element
 *   array[1..4]    slice [1], [2], [3]
 *   array[..3]     first three elements
 *
 * --- ??= ---
 *
 *   cache ??= new Dictionary<string, int>();
 *
 * --- readonly struct ---
 *
 *   public readonly struct Point { public int X { get; } … }
 *
 * --- default interface method ---
 *
 *   interface IWorker { void Report() => Console.WriteLine("ok"); }
 *
 * --- static local function ---
 *
 *   static int Add(int a, int b) => a + b;
 *
 * --- disposable ref struct ---
 *
 *   public ref struct Reader : IDisposable { public void Dispose() { … } }
 *
 * --- IAsyncEnumerable ---
 *
 *   async IAsyncEnumerable<T> GetAsync() { yield return x; await Task.Yield(); }
 *
 * --- Related chapters ---
 *
 *   05. C# 7 Features              pattern matching foundations, tuples, local functions
 *   07. C# 9+ Features (future)  records, init-only setters, top-level statements
 *   Span / Memory topics           deep dive on stackalloc and unmanaged constraints
 *
 * =============================================================================
 */
