/*
 * =============================================================================
 * 02. STREAMREADER AND STREAMWRITER — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Character-based text I/O through TextReader/TextWriter abstractions
 *        and their concrete implementations — StreamReader/StreamWriter for files,
 *        StringReader/StringWriter for in-memory text, plus encoding and buffering.
 *
 * WHY IT MATTERS:
 *   Loading a multi-gigabyte log with File.ReadAllText (ch.01) allocates the
 *   entire file into one string. StreamReader reads line-by-line or in chunks so
 *   memory stays bounded. StreamWriter streams output incrementally. TextReader/
 *   TextWriter let you swap file vs string backends without rewriting parsers.
 *
 * WHAT YOU WILL LEARN:
 *   1.  TextReader / TextWriter — abstract text I/O contract and polymorphism
 *   2.  StreamWriter — Write, WriteLine, Flush, AutoFlush, append, encoding ctors
 *   3.  StreamReader — ReadLine, ReadToEnd, ReadBlock, Peek, EndOfStream, ctors
 *   4.  Encoding — UTF-8, system default, matching reader/writer encodings
 *   5.  StringReader / StringWriter — build or parse text without touching disk
 *   6.  using / Dispose — release file handles; internal buffering behavior
 *   7.  Streams vs File.ReadAllText — when one-shot helpers win (preview ch.01)
 *
 * =============================================================================
 */

using System;
using System.IO;
using System.Text;

namespace StreamReaderAndStreamWriter;

/*
 * =========================================================================
 * SECTION 1: TextReader AND TextWriter — ABSTRACT BASE CLASSES
 * =========================================================================
 *
 * System.IO defines two abstract roots for character-based I/O:
 *
 *   TextWriter  → Write, WriteLine, Flush, Encoding property
 *   TextReader  → Read, ReadLine, ReadToEnd, ReadBlock, Peek
 *
 * Concrete types inherit these contracts:
 *
 *   Source / destination     Writer              Reader
 *   --------------------     ----------------    ----------------
 *   File on disk             StreamWriter        StreamReader
 *   In-memory string         StringWriter        StringReader
 *
 * Methods that accept TextReader (custom parsers, CSV loaders) work with files
 * OR strings — swap the implementation without rewriting the algorithm.
 *
 * EndOfStream is StreamReader-only; TextReader base has no EndOfStream property.
 * -------------------------------------------------------------------------
 */
public static class TextIoHelpers
{
    public static void WriteBanner(TextWriter writer)
    {
        writer.WriteLine("=== Stream I/O demo ==="); // WriteLine appends platform newline
    }

    public static int CountNonEmptyLines(TextReader reader)
    {
        int count = 0;
        string? line;
        while ((line = reader.ReadLine()) != null) // null means end of stream
        {
            if (line.Length > 0)
            {
                count++;
            }
        }
        return count;
    }

    public static string ParseFirstColumn(TextReader reader)
    {
        string? header = reader.ReadLine(); // consume header row
        string? firstData = reader.ReadLine();
        if (header == null || firstData == null)
        {
            return string.Empty;
        }

        int comma = firstData.IndexOf(',');
        return comma >= 0 ? firstData.Substring(0, comma) : firstData;
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 8: MAIN — ORCHESTRATES THE CHAPTER DEMO
     * =========================================================================
     *
     * Each step calls into the sections above. Demo files live under a temp
     * folder recreated each run (Directory helpers are ch.01; Path.Combine
     * preview — full coverage in ch.04 Path and Environment).
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        string demoFolder = Path.Combine(AppContext.BaseDirectory, "stream-demo");
        if (Directory.Exists(demoFolder))
        {
            Directory.Delete(demoFolder, recursive: true); // clean slate from prior run
        }
        Directory.CreateDirectory(demoFolder);

        string logPath = Path.Combine(demoFolder, "app.log");
        string unicodePath = Path.Combine(demoFolder, "unicode-notes.txt");

        Console.WriteLine("=== 02. StreamReader and StreamWriter ===");
        Console.WriteLine();

        DemonstrateTextReaderWriterPolymorphism();
        int lineCount = DemonstrateStreamWriter(logPath);
        DemonstrateStreamReader(logPath);
        DemonstrateEncoding(unicodePath);
        DemonstrateStringReaderWriter(logPath, lineCount);
        DemonstrateDisposeAndBuffering(logPath);
        DemonstrateStreamsVsReadAllText(logPath);

        Directory.Delete(demoFolder, recursive: true); // tidy up demo artifacts
        Console.WriteLine();
        Console.WriteLine("Demo folder removed.");
    }

    /*
     * --- 1a. Polymorphism — same helper, different TextWriter backend ---
     */
    private static void DemonstrateTextReaderWriterPolymorphism()
    {
        using (StringWriter bannerWriter = new StringWriter())
        {
            TextIoHelpers.WriteBanner(bannerWriter); // TextWriter parameter — not StringWriter-specific
            Console.WriteLine($"TextWriter polymorphism: {bannerWriter.ToString().Trim()}");
        }

        string inlineCsv = "id,name\n1,Alpha\n2,Beta\n";
        using (StringReader csvReader = new StringReader(inlineCsv))
        {
            string firstId = TextIoHelpers.ParseFirstColumn(csvReader); // TextReader parameter
            Console.WriteLine($"TextReader polymorphism — first id: {firstId}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 2: StreamWriter — WRITE TEXT TO A FILE OR STREAM
     * =========================================================================
     *
     * StreamWriter wraps a file path or an underlying Stream for text output.
     *
     * Common constructors:
     *
     *   new StreamWriter(path)                         create/overwrite, UTF-8 no BOM
     *   new StreamWriter(path, append: true)           append to existing file
     *   new StreamWriter(path, append, encoding)       explicit Encoding
     *   new StreamWriter(stream)                       wrap FileStream (ch.03 preview)
     *   new StreamWriter(stream, encoding)             stream + encoding
     *
     * Members:
     *
     *   Write(text)       characters with no trailing newline
     *   WriteLine(text)   line + Environment.NewLine
     *   Flush()           push internal buffer to underlying stream / disk
     *   AutoFlush         when true, Flush after every Write/WriteLine
     *   .Encoding         Encoding used for byte conversion
     *
     * Always wrap in using so handles close even when an exception escapes.
     * -------------------------------------------------------------------------
     */
    private static int DemonstrateStreamWriter(string logPath)
    {
        Console.WriteLine("--- StreamWriter ---");

        // --- 2a. Path constructor — create/overwrite with default UTF-8 (no BOM) ---
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.Write("2026-08-09 09:00:00 "); // partial line — no newline yet
            writer.WriteLine("INFO  Application started.");
            writer.WriteLine("2026-08-09 09:00:01 INFO  Loading configuration.");
            writer.WriteLine("2026-08-09 09:00:02 WARN  Cache folder missing — using defaults.");
            writer.Flush(); // ensure buffer reaches disk before closing
            Console.WriteLine($"  Default encoding: {writer.Encoding.WebName}");
        }

        Console.WriteLine($"  Log written: {logPath} ({new FileInfo(logPath).Length} bytes)");

        // --- 2b. Append mode ---
        using (StreamWriter appendWriter = new StreamWriter(logPath, append: true))
        {
            appendWriter.WriteLine("2026-08-09 09:05:00 INFO  User session opened.");
        }

        Console.WriteLine($"  After append: {new FileInfo(logPath).Length} bytes");

        // --- 2c. AutoFlush — no manual Flush needed after each line ---
        string auditPath = Path.Combine(Path.GetDirectoryName(logPath)!, "audit.log");
        using (StreamWriter autoFlushWriter = new StreamWriter(auditPath))
        {
            autoFlushWriter.AutoFlush = true; // each Write/WriteLine flushes immediately
            autoFlushWriter.WriteLine("AUDIT  Settings saved.");
            // another process could read this line immediately — no explicit Flush required
        }

        Console.WriteLine($"  AutoFlush audit file: {auditPath}");

        // --- 2d. FileStream constructor (byte stream internals → ch.03 FileStream) ---
        string streamBackedPath = Path.Combine(Path.GetDirectoryName(logPath)!, "stream-backed.log");
        using (FileStream fileStream = new FileStream(streamBackedPath, FileMode.Create, FileAccess.Write))
        using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
        {
            streamWriter.WriteLine("Written through FileStream → StreamWriter chain.");
        }

        Console.WriteLine($"  FileStream-backed writer: {streamBackedPath}");
        Console.WriteLine();

        // unicodePath is written in Section 4 — return line count for StringWriter demo
        return 5; // four initial lines + one append line
    }

    /*
     * =========================================================================
     * SECTION 3: StreamReader — READ TEXT FROM A FILE OR STREAM
     * =========================================================================
     *
     * StreamReader reads decoded characters from a file path or underlying Stream.
     *
     * Common constructors:
     *
     *   new StreamReader(path)                    default encoding detection
     *   new StreamReader(path, encoding)          explicit Encoding (must match writer)
     *   new StreamReader(path, detectEncodingFromByteOrderMarks: true)
     *   new StreamReader(stream)                  wrap FileStream (ch.03 preview)
     *   new StreamReader(stream, encoding)
     *
     * Read members:
     *
     *   ReadLine()     next line without newline chars; null at end of file
     *   ReadToEnd()    entire remaining content as one string (fine for small files)
     *   ReadBlock()    fill char[] buffer; returns count of chars read (large files)
     *   Peek()         next char without consuming (-1 at EOF) — inherited from TextReader
     *   EndOfStream    StreamReader property — true when positioned at EOF
     *
     * Typical log processing: while ((line = reader.ReadLine()) != null) { … }
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateStreamReader(string logPath)
    {
        Console.WriteLine("--- StreamReader ---");

        // --- 3a. ReadLine — line-by-line (bounded memory for large files) ---
        Console.WriteLine("  ReadLine:");
        int lineNumber = 0;
        using (StreamReader lineReader = new StreamReader(logPath))
        {
            string? line;
            while ((line = lineReader.ReadLine()) != null)
            {
                lineNumber++;
                Console.WriteLine($"    [{lineNumber}] {line}");
            }
        }

        // --- 3b. ReadToEnd — entire file as one string (small files only) ---
        using (StreamReader fullReader = new StreamReader(logPath))
        {
            string entireLog = fullReader.ReadToEnd();
            Console.WriteLine($"  ReadToEnd: {entireLog.Length} characters total.");
        }

        // --- 3c. ReadBlock — read into a fixed char buffer (chunk processing) ---
        char[] buffer = new char[64];
        using (StreamReader blockReader = new StreamReader(logPath))
        {
            int charsRead = blockReader.ReadBlock(buffer, 0, buffer.Length); // may read fewer than buffer.Length
            string preview = new string(buffer, 0, charsRead);
            Console.WriteLine($"  ReadBlock preview ({charsRead} chars): {preview.Replace("\r\n", " ").Replace('\n', ' ')}...");
        }

        // --- 3d. Peek — look ahead without consuming ---
        using (StringReader peekReader = new StringReader("First line\nSecond line"))
        {
            int nextChar = peekReader.Peek(); // returns 'F' without advancing
            Console.WriteLine($"  Peek (non-consuming): '{(char)nextChar}'");
            Console.WriteLine($"  ReadLine after Peek: {peekReader.ReadLine()}");
        }

        // --- 3e. EndOfStream — StreamReader-only EOF indicator ---
        using (StreamReader eofReader = new StreamReader(logPath))
        {
            eofReader.ReadToEnd(); // advance to end
            Console.WriteLine($"  EndOfStream after ReadToEnd: {eofReader.EndOfStream}");
        }

        // --- 3f. FileStream constructor — same chain as StreamWriter demo ---
        string streamBackedPath = Path.Combine(Path.GetDirectoryName(logPath)!, "stream-backed.log");
        using (FileStream fileStream = new FileStream(streamBackedPath, FileMode.Open, FileAccess.Read))
        using (StreamReader streamReader = new StreamReader(fileStream))
        {
            string firstLine = streamReader.ReadLine() ?? string.Empty;
            Console.WriteLine($"  FileStream-backed ReadLine: {firstLine}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 4: ENCODING — HOW BYTES BECOME CHARACTERS
     * =========================================================================
     *
     * Text files are bytes on disk; Encoding maps bytes ↔ chars.
     *
     *   Encoding.UTF8              universal; default for new StreamWriter(path)
     *   Encoding.Default           system ANSI code page (legacy Windows apps)
     *   new UTF8Encoding(false)    UTF-8 without byte-order mark (BOM)
     *
     * Mismatch (write UTF-8, read as Default) produces mojibake (garbled text).
     * Pass the SAME Encoding to matching StreamWriter and StreamReader ctors.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateEncoding(string unicodePath)
    {
        Console.WriteLine("--- Encoding ---");

        UTF8Encoding utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        using (StreamWriter utf8Writer = new StreamWriter(unicodePath, append: false, utf8NoBom))
        {
            utf8Writer.WriteLine("Price: €42 — café résumé"); // non-ASCII needs consistent encoding
        }

        using (StreamReader utf8Reader = new StreamReader(unicodePath, utf8NoBom))
        {
            string unicodeLine = utf8Reader.ReadLine() ?? string.Empty;
            Console.WriteLine($"  UTF-8 round-trip: {unicodeLine}");
            Console.WriteLine($"  Reader CurrentEncoding: {utf8Reader.CurrentEncoding.WebName}");
        }

        // Encoding.Default — system code page; avoid for cross-platform text unless required
        string legacyPath = Path.Combine(Path.GetDirectoryName(unicodePath)!, "legacy-ansi.txt");
        using (StreamWriter defaultWriter = new StreamWriter(legacyPath, append: false, Encoding.Default))
        {
            defaultWriter.WriteLine("Legacy system encoding sample.");
        }

        using (StreamReader defaultReader = new StreamReader(legacyPath, Encoding.Default))
        {
            string legacyLine = defaultReader.ReadLine() ?? string.Empty;
            Console.WriteLine($"  Encoding.Default round-trip: {legacyLine}");
            Console.WriteLine($"  Default code page: {Encoding.Default.WebName}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 5: StringWriter AND StringReader — IN-MEMORY TEXT
     * =========================================================================
     *
     * StringWriter : TextWriter → appends to an internal StringBuilder
     *   ToString() / GetStringBuilder() retrieve built text
     *
     * StringReader : TextReader → reads from an existing string (immutable source)
     *
     * Use cases:
     *   • Build CSV/report in memory before saving (StringWriter)
     *   • Unit-test parsers without touching disk (StringReader)
     *   • Reuse a method that accepts TextReader / TextWriter
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateStringReaderWriter(string logPath, int lineCount)
    {
        Console.WriteLine("--- StringWriter / StringReader ---");

        using (StringWriter reportWriter = new StringWriter())
        {
            reportWriter.WriteLine("=== Daily summary ===");
            reportWriter.WriteLine($"Lines in log: {lineCount}");
            reportWriter.WriteLine($"Log path: {logPath}");

            StringBuilder builder = reportWriter.GetStringBuilder(); // same buffer as ToString()
            Console.WriteLine($"  StringBuilder length: {builder.Length}");
            Console.WriteLine(reportWriter.ToString());
        }

        string inlineCsv = "id,name\n1,Alpha\n2,Beta\n";
        using (StringReader csvReader = new StringReader(inlineCsv))
        {
            int totalLines = TextIoHelpers.CountNonEmptyLines(csvReader);
            Console.WriteLine($"  Non-empty lines in in-memory CSV: {totalLines}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 6: using / DISPOSE AND INTERNAL BUFFERING
     * =========================================================================
     *
     * StreamReader and StreamWriter hold unmanaged file handles and internal
     * char buffers. Dispose (via using) closes the handle promptly.
     *
     *   using (StreamReader r = new StreamReader(path)) { … }  // preferred
     *   reader.Dispose()                                        // explicit; same effect
     *
     * StreamWriter buffering:
     *   • Writes accumulate in an internal buffer for performance
     *   • Flush() pushes buffer to disk without closing
     *   • AutoFlush = true flushes after every Write/WriteLine
     *   • Dispose/close also flushes remaining buffer
     *
     * StreamReader buffering:
     *   • ReadLine/ReadToEnd use an internal read buffer automatically
     *   • ReadBlock lets YOU supply a char[] for custom chunk sizes
     *
     * Forgetting Dispose leaves files locked until GC finalizes — common bug on Windows.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateDisposeAndBuffering(string logPath)
    {
        Console.WriteLine("--- Dispose and buffering ---");

        string flushDemoPath = Path.Combine(Path.GetDirectoryName(logPath)!, "flush-demo.txt");

        StreamWriter unflushedWriter = new StreamWriter(flushDemoPath);
        unflushedWriter.Write("Buffered until Flush or Dispose.");
        // Data sits in StreamWriter's internal buffer — not necessarily on disk yet

        long sizeBeforeFlush = new FileInfo(flushDemoPath).Length; // check size without opening a second handle
        Console.WriteLine($"  File length before Flush: {sizeBeforeFlush} bytes (buffer may be empty on disk)");

        unflushedWriter.Flush(); // push internal buffer to the underlying file
        long sizeAfterFlush = new FileInfo(flushDemoPath).Length;
        Console.WriteLine($"  File length after Flush: {sizeAfterFlush} bytes");

        unflushedWriter.Dispose(); // final flush + release file handle

        using (StreamReader afterDisposeReader = new StreamReader(flushDemoPath))
        {
            string content = afterDisposeReader.ReadToEnd();
            Console.WriteLine($"  After Dispose, ReadToEnd: '{content}'");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 7: STREAMS VS File.ReadAllText — WHEN TO USE WHICH
     * =========================================================================
     *
     * File static helpers (ch.01 File and Directory Operations) load entire
     * files into memory:
     *
     *   File.ReadAllText(path)     → one string (entire file)
     *   File.ReadAllLines(path)    → string[] (entire file)
     *   File.ReadLines(path)       → lazy IEnumerable<string> (better, still ch.01)
     *
     * Prefer StreamReader when:
     *   • File may be large (logs, exports, data feeds)
     *   • You process line-by-line or in fixed-size chunks
     *   • You need explicit Encoding control on the stream
     *
     * Prefer File.ReadAllText / ReadAllLines when:
     *   • File is small and you need the whole content at once
     *   • Simplicity beats streaming control
     *
     * COVERED IN DETAIL LATER → 01. File and Directory Operations
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateStreamsVsReadAllText(string logPath)
    {
        Console.WriteLine("--- Streams vs File.ReadAllText (preview) ---");

        // Small file — ReadAllText is acceptable (full API in ch.01)
        string allAtOnce = File.ReadAllText(logPath);
        Console.WriteLine($"  File.ReadAllText: {allAtOnce.Length} chars loaded at once.");

        // Same file — StreamReader line loop uses bounded memory regardless of size
        int streamedLines = 0;
        using (StreamReader streamReader = new StreamReader(logPath))
        {
            while (streamReader.ReadLine() != null)
            {
                streamedLines++;
            }
        }

        Console.WriteLine($"  StreamReader line loop: {streamedLines} lines without loading entire file.");
        Console.WriteLine("  For large files, prefer StreamReader or File.ReadLines (ch.01).");
        Console.WriteLine();
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — STREAMREADER AND STREAMWRITER
 * =============================================================================
 *
 * --- Abstract bases ---
 *
 *   TextWriter    Write, WriteLine, Flush, Encoding
 *   TextReader    Read, ReadLine, ReadToEnd, ReadBlock, Peek
 *
 * --- StreamWriter ---
 *
 *   new StreamWriter(path)
 *   new StreamWriter(path, append: true)
 *   new StreamWriter(path, append, encoding)
 *   new StreamWriter(stream) / new StreamWriter(stream, encoding)
 *
 *   writer.Write("partial");  writer.WriteLine("full line");
 *   writer.Flush();           writer.AutoFlush = true;
 *
 * --- StreamReader ---
 *
 *   new StreamReader(path)
 *   new StreamReader(path, encoding)
 *   new StreamReader(stream) / new StreamReader(stream, encoding)
 *
 *   ReadLine()     → string? (null = EOF)
 *   ReadToEnd()    → remaining text as one string
 *   ReadBlock(buf, index, count) → chars read into buffer
 *   Peek()         → next char without advancing (-1 at EOF)
 *   EndOfStream    → StreamReader only; true at EOF
 *
 * --- In-memory ---
 *
 *   StringWriter   → ToString() / GetStringBuilder()
 *   StringReader   → wraps existing string; same ReadLine contract
 *
 * --- Encoding ---
 *
 *   Encoding.UTF8                       default for new StreamWriter(path)
 *   Encoding.Default                    system code page (legacy)
 *   new UTF8Encoding(false)             UTF-8 without BOM
 *
 *   Pass the same Encoding to matching StreamWriter and StreamReader.
 *
 * --- Dispose / buffering ---
 *
 *   using (var w = new StreamWriter(path)) { … }   releases handle + final flush
 *   Flush()                                         push writer buffer without close
 *   AutoFlush = true                                flush after each write
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  ReadAllText on huge files            | OutOfMemoryException
 *  Mismatched write/read encoding       | Mojibake (garbled characters)
 *  Forgetting using / Dispose           | File locked until GC
 *  while (ReadLine() != "")             | Empty lines skipped unintentionally
 *  Assuming TextReader has EndOfStream  | Compile error — use StreamReader
 *
 * --- Related chapters ---
 *
 *   01. File and Directory Operations   File.ReadAllText, ReadLines, Exists
 *   03. FileStream and Binary Files     FileMode, byte-level stream control
 *   04. Path and Environment            Path.Combine, AppContext.BaseDirectory
 *
 * =============================================================================
 */
