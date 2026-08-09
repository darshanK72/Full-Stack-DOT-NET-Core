/*
 * =============================================================================
 * 03. FILESTREAM AND BINARY FILES — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Low-level byte I/O with FileStream, plus typed binary serialization
 *        through BinaryReader and BinaryWriter for structured binary files.
 *
 * WHY IT MATTERS:
 *   Text files are human-readable but inefficient for numeric arrays, images,
 *   game saves, or custom protocols. FileStream reads and writes raw bytes with
 *   precise control over position and sharing. BinaryReader and BinaryWriter
 *   add ReadInt32, WriteString, and other typed helpers on top of any Stream
 *   without manual bit shifting or length-prefix math.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Binary vs text files — when each format fits
 *   2.  FileStream — FileMode, FileAccess, FileShare, Position, Seek, Length
 *   3.  Byte-level Read and Write — buffers, partial reads, ReadByte/WriteByte
 *   4.  using blocks, Flush, and Dispose — releasing handles and pushing buffers
 *   5.  BinaryWriter — Write primitives, strings, and byte arrays
 *   6.  BinaryReader — ReadInt32, ReadString, ReadBytes, and matching order
 *   7.  Structured binary record files — header plus repeating records
 *   8.  FileMode.OpenOrCreate and Truncate — cache and rewrite patterns
 *
 * =============================================================================
 */

using System;
using System.IO;
using System.Text;

namespace FileStreamAndBinaryFiles;

/*
 * =========================================================================
 * SECTION 1: BINARY VS TEXT FILES — WHEN TO USE WHICH
 * =========================================================================
 *
 * A file on disk is always bytes. What changes is how you interpret them:
 *
 *   Aspect            | Text file                    | Binary file
 *   ------------------|------------------------------|-----------------------------
 *   Human readable    | Yes (Notepad, VS Code)       | No — hex editor or your app
 *   Encoding          | UTF-8, ASCII, etc.           | App-defined layout (no "lines")
 *   Numbers           | Characters "42" (2+ bytes)   | int32 = exactly 4 bytes
 *   Random access     | Line-based; seek by scanning | Fixed field sizes → fast Seek
 *   Typical APIs      | StreamReader/StreamWriter    | FileStream + BinaryReader/Writer
 *   Best for          | Logs, config, CSV, JSON      | Images, audio, saves, protocols
 *
 * Use text when people or line-oriented tools must read the file. Use binary when
 * you need compact storage, fixed layouts, or interoperability with non-.NET code
 * that expects a byte protocol.
 *
 * Text layer (StreamReader/StreamWriter, encodings, ReadLine) is covered in detail
 * in chapter 02. File static helpers (ReadAllText, Copy, Delete) live in chapter 01.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: STRUCTURED BINARY RECORD — DOMAIN TYPE FOR THE DEMO
 * =========================================================================
 *
 * A "record" is one logical row stored as a fixed sequence of Write calls.
 * ProductRecord is an in-memory shape; on disk we serialize Id, Name, Price,
 * and InStock in that exact order inside inventory.bin (see SECTION 7).
 * -------------------------------------------------------------------------
 */
public readonly record struct ProductRecord(int Id, string Name, double Price, bool InStock);

/*
 * =========================================================================
 * SECTION 3: BINARY INVENTORY CODEC — WRITE AND READ A RECORD FILE
 * =========================================================================
 *
 * inventory.bin layout:
 *
 *   Offset (conceptual)   Field           Type / notes
 *   -------------------   --------------- ---------------------------------
 *   0                     magic           4 ASCII bytes "INV1" (file signature)
 *   4                     recordCount     int32
 *   8+                    record[0..n-1]  repeat recordCount times:
 *                         productId       int32
 *                         name            string (7-bit length prefix + UTF-8)
 *                         unitPrice       double (8 bytes)
 *                         inStock         bool (1 byte)
 *
 * BinaryWriter.Write(string) stores a length-prefixed UTF-8 string (.NET 7-bit
 * length encoding). BinaryReader.ReadString() must use the same Encoding.
 * -------------------------------------------------------------------------
 */
public static class BinaryInventoryCodec
{
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("INV1");

    public static void Write(string path, ProductRecord[] products)
    {
        using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);

        writer.Write(Magic);                  // 4-byte signature so readers reject wrong files
        writer.Write(products.Length);          // header: how many records follow

        foreach (ProductRecord product in products)
        {
            writer.Write(product.Id);           // int32 — 4 bytes, little-endian on typical x64
            writer.Write(product.Name);         // length-prefixed UTF-8 string
            writer.Write(product.Price);        // double — 8 bytes IEEE 754
            writer.Write(product.InStock);      // bool — single byte (0 or 1)
        }

        writer.Flush();                         // push writer buffer to underlying FileStream
        stream.Flush();                         // push FileStream buffer to the OS / disk
    }

    public static ProductRecord[] Read(string path)
    {
        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);

        byte[] magic = reader.ReadBytes(Magic.Length); // read exact byte count — not ReadString
        if (!magic.AsSpan().SequenceEqual(Magic))
        {
            throw new InvalidDataException("Not an inventory file — magic bytes mismatch.");
        }

        int recordCount = reader.ReadInt32();
        ProductRecord[] results = new ProductRecord[recordCount];

        for (int i = 0; i < recordCount; i++)
        {
            int id = reader.ReadInt32();
            string name = reader.ReadString();       // must match prior Write(name) order
            double price = reader.ReadDouble();
            bool inStock = reader.ReadBoolean();
            results[i] = new ProductRecord(id, name, price, inStock);
        }

        return results;
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 4: FILESTREAM — FileMode, FileAccess, FileShare
     * =========================================================================
     *
     * FileStream is a Stream backed by a file on disk. Common constructor:
     *
     *   new FileStream(path, FileMode, FileAccess, FileShare)
     *
     * FileMode (how the OS opens the file):
     *
     *   Mode          | Behavior
     *   --------------|--------------------------------------------------------
     *   Create        | Create new; truncate if path already exists
     *   Open          | Open existing; FileNotFoundException if missing
     *   OpenOrCreate  | Open if present, else create empty file
     *   Append        | Open or create; seek to end before every write
     *   Truncate      | Open existing and set length to 0 immediately
     *
     * FileAccess: Read, Write, ReadWrite — what this handle may do.
     *
     * FileShare: None, Read, Write, ReadWrite — what other processes may do
     *             while this handle is open (None = exclusive lock).
     *
     * Key members: Length, Position, Read, Write, Seek, Flush, ReadByte, WriteByte.
     * -------------------------------------------------------------------------
     */
    public static void WriteFileSignature(string path)
    {
        byte[] signature = Encoding.ASCII.GetBytes("INV1");

        using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        stream.Write(signature, 0, signature.Length); // copy 4 bytes from buffer into the file
        Console.WriteLine($"Wrote {signature.Length} signature bytes. Stream length: {stream.Length}");
    }

    /*
     * =========================================================================
     * SECTION 5: READ AND WRITE RAW BYTES
     * =========================================================================
     *
     * Write(byte[] buffer, int offset, int count) copies count bytes from the
     * buffer starting at offset into the file stream at the current Position.
     *
     * Read(byte[] buffer, int offset, int count) fills the buffer and returns
     * the number of bytes actually read (0 at end-of-file). Always check the
     * return value — a single Read call may return fewer than count bytes when
     * the stream is near EOF or the OS delivers a partial chunk.
     *
     * ReadByte() returns one byte (0–255) or -1 at EOF. WriteByte writes one byte.
     * -------------------------------------------------------------------------
     */
    public static string ReadFileSignature(string path)
    {
        byte[] readBuffer = new byte[4];

        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length); // may be < buffer.Length at EOF
        string decoded = Encoding.ASCII.GetString(readBuffer, 0, bytesRead);
        Console.WriteLine($"Read {bytesRead} bytes: {decoded}");
        Console.WriteLine($"Position after read: {stream.Position} / {stream.Length}");
        return decoded;
    }

    /*
     * =========================================================================
     * SECTION 6: Position, Seek, AND Length — RANDOM ACCESS
     * =========================================================================
     *
     * Position is the next byte offset for Read or Write. Length is total file
     * size in bytes. Seek moves Position without reading/writing:
     *
     *   Seek(offset, SeekOrigin.Begin)   → absolute from start (offset 0 = first byte)
     *   Seek(offset, SeekOrigin.Current) → relative to current Position
     *   Seek(offset, SeekOrigin.End)     → relative to end (often negative offset)
     *
     * After Seek(-2, SeekOrigin.End), Read 2 bytes yields the last two bytes only.
     * -------------------------------------------------------------------------
     */
    public static string ReadLastTwoBytes(string path)
    {
        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(-2, SeekOrigin.End);              // jump to 2 bytes before EOF
        byte[] tail = new byte[2];
        int tailRead = stream.Read(tail, 0, tail.Length);
        string tailText = Encoding.ASCII.GetString(tail, 0, tailRead);
        Console.WriteLine($"Last {tailRead} bytes via Seek: {tailText} (Position now {stream.Position})");
        return tailText;
    }

    /*
     * =========================================================================
     * SECTION 7: Append MODE, Flush, AND Dispose
     * =========================================================================
     *
     * FileMode.Append opens (or creates) and positions at end-of-file before
     * each write — ideal for log-style growth without manual Seek.
     *
     * Flush pushes buffered bytes to the underlying stream / OS. Dispose (end of
     * using) closes the handle and releases the file lock. Always wrap FileStream
     * in using — forgetting Dispose leaves the file locked until GC runs.
     *
     * FileInfo.Length read from disk may be stale until Flush/dispose completes.
     * -------------------------------------------------------------------------
     */
    public static long AppendBytesAndReportLength(string path, byte[] extra)
    {
        using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
        {
            stream.Write(extra, 0, extra.Length);
            stream.Flush(); // ensure appended bytes are visible before we inspect length
            Console.WriteLine($"After append, stream length: {stream.Length} bytes");
            return stream.Length;
        } // Dispose closes handle — another process can open the file now
    }

    /*
     * =========================================================================
     * SECTION 8: BinaryWriter — TYPED PRIMITIVE WRITES
     * =========================================================================
     *
     * BinaryWriter wraps any Stream (usually FileStream) and writes CLR types in
     * a fixed binary layout. Common methods:
     *
     *   Write(bool)  Write(byte)  Write(byte[])  Write(char)  Write(decimal)
     *   Write(double) Write(float) Write(int)     Write(long)  Write(short)
     *   Write(string) Write(uint) Write(ulong)   Write(ushort)
     *
     * Constructor: new BinaryWriter(stream, encoding, leaveOpen)
     *   leaveOpen: false (default) — disposing the writer also disposes the stream.
     *
     * BaseStream property exposes the underlying Stream (e.g. to check Position).
     * -------------------------------------------------------------------------
     */
    public static void WritePrimitiveSnapshot(string path)
    {
        using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);

        writer.Write(42);                              // int32
        writer.Write(3.14);                            // double
        writer.Write(true);                            // bool
        writer.Write("snapshot");                      // length-prefixed string
        writer.Write(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }); // raw byte blob
        writer.Write(9.99m);                           // decimal — 16-byte block

        writer.Flush();
        Console.WriteLine($"Primitive snapshot: {writer.BaseStream.Length} bytes on disk");
    }

    /*
     * =========================================================================
     * SECTION 9: BinaryReader — TYPED READS AND ReadBytes
     * =========================================================================
     *
     * BinaryReader mirrors BinaryWriter — read types in the same order they were
     * written. Mismatch (ReadInt32 when a string was written) corrupts the rest
     * of the file and yields garbage or EndOfStreamException.
     *
     * ReadBytes(count) reads exactly count bytes or throws EndOfStreamException
     * if fewer remain — unlike Read on Stream, which returns a partial count.
     *
     * Pass the same Encoding to BinaryReader that you used on BinaryWriter.
     * -------------------------------------------------------------------------
     */
    public static void ReadPrimitiveSnapshot(string path)
    {
        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);

        int number = reader.ReadInt32();
        double pi = reader.ReadDouble();
        bool flag = reader.ReadBoolean();
        string label = reader.ReadString();
        byte[] marker = reader.ReadBytes(4);         // fixed-width byte block
        decimal price = reader.ReadDecimal();

        Console.WriteLine($"  int={number}, double={pi}, bool={flag}, string={label}");
        Console.WriteLine($"  bytes={BitConverter.ToString(marker)}, decimal={price}");
        Console.WriteLine($"  Reader Position at EOF: {reader.BaseStream.Position} / {reader.BaseStream.Length}");
    }

    /*
     * =========================================================================
     * SECTION 10: FileMode.OpenOrCreate AND Truncate
     * =========================================================================
     *
     * OpenOrCreate is useful for cache or state files: open if present, else
     * create an empty file ready for ReadWrite.
     *
     * Truncate opens an existing file and immediately sets Length to 0 while
     * keeping the handle open — rewrite in place without deleting the path.
     * -------------------------------------------------------------------------
     */
    public static void DemonstrateOpenOrCreateAndTruncate(string cachePath)
    {
        using (FileStream openOrCreate = new FileStream(
            cachePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
        {
            Console.WriteLine($"OpenOrCreate initial length: {openOrCreate.Length}");
        }

        File.WriteAllBytes(cachePath, new byte[] { 1, 2, 3, 4, 5 });

        using (FileStream truncate = new FileStream(
            cachePath, FileMode.Truncate, FileAccess.Write, FileShare.None))
        {
            truncate.WriteByte(0xFF); // single byte after wiping previous 5 bytes
            Console.WriteLine($"After Truncate + WriteByte, length: {truncate.Length}");
        }
    }

    /*
     * =========================================================================
     * SECTION 11: DEMONSTRATION — Main ORCHESTRATES THE CHAPTER
     * =========================================================================
     */
    public static void Main(string[] args)
    {
        string dataFolder = Path.Combine(AppContext.BaseDirectory, "binary-demo");
        if (Directory.Exists(dataFolder))
        {
            Directory.Delete(dataFolder, recursive: true);
        }
        Directory.CreateDirectory(dataFolder);

        string signaturePath = Path.Combine(dataFolder, "signature.bin");
        string snapshotPath = Path.Combine(dataFolder, "primitives.bin");
        string inventoryPath = Path.Combine(dataFolder, "inventory.bin");
        string cachePath = Path.Combine(dataFolder, "cache.bin");

        Console.WriteLine("=== FileStream raw bytes ===");
        WriteFileSignature(signaturePath);
        ReadFileSignature(signaturePath);
        ReadLastTwoBytes(signaturePath);
        AppendBytesAndReportLength(signaturePath, Encoding.ASCII.GetBytes("OK"));

        Console.WriteLine();
        Console.WriteLine("=== BinaryWriter / BinaryReader primitives ===");
        WritePrimitiveSnapshot(snapshotPath);
        ReadPrimitiveSnapshot(snapshotPath);

        Console.WriteLine();
        Console.WriteLine("=== Structured inventory.bin ===");
        ProductRecord[] catalog =
        [
            new ProductRecord(101, "Keyboard", 79.99, true),
            new ProductRecord(102, "Mouse", 29.50, true),
            new ProductRecord(103, "Monitor", 349.00, false),
        ];

        BinaryInventoryCodec.Write(inventoryPath, catalog);
        Console.WriteLine($"Inventory file size: {new FileInfo(inventoryPath).Length} bytes");

        ProductRecord[] loaded = BinaryInventoryCodec.Read(inventoryPath);
        double catalogValue = 0;
        foreach (ProductRecord item in loaded)
        {
            catalogValue += item.Price;
            string stock = item.InStock ? "in stock" : "out";
            Console.WriteLine($"  #{item.Id} {item.Name,-10} ${item.Price:F2} ({stock})");
        }
        Console.WriteLine($"Catalog value (one unit each): ${catalogValue:F2}");

        Console.WriteLine();
        Console.WriteLine("=== OpenOrCreate and Truncate ===");
        DemonstrateOpenOrCreateAndTruncate(cachePath);

        Directory.Delete(dataFolder, recursive: true);
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — FILESTREAM AND BINARY FILES
 * =============================================================================
 *
 * --- Binary vs text ---
 *
 *   Text  → StreamReader/StreamWriter (ch.02), human-readable lines
 *   Binary → FileStream + BinaryReader/Writer, compact typed layout
 *
 * --- FileStream constructor ---
 *
 *   new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
 *   new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
 *
 * --- FileMode ---
 *
 *   Create | Open | OpenOrCreate | Append | Truncate
 *
 * --- Byte I/O ---
 *
 *   stream.Write(buffer, offset, count)
 *   int n = stream.Read(buffer, offset, count)   // n may be < count
 *   stream.WriteByte(0xFF);  int b = stream.ReadByte(); // -1 at EOF
 *   stream.Seek(offset, SeekOrigin.Begin | Current | End)
 *   stream.Position, stream.Length, stream.Flush()
 *
 * --- using / Dispose ---
 *
 *   using FileStream fs = …;  // Dispose at end of block — releases file lock
 *   writer.Flush(); stream.Flush();  // push buffers before external length checks
 *
 * --- BinaryWriter / BinaryReader ---
 *
 *   using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
 *   using var w = new BinaryWriter(fs, Encoding.UTF8);
 *   w.Write(42); w.Write("name"); w.Write(3.14); w.Write(new byte[] { … }); w.Flush();
 *
 *   using var r = new BinaryReader(fs, Encoding.UTF8);
 *   int n = r.ReadInt32(); string s = r.ReadString(); byte[] blob = r.ReadBytes(4);
 *
 *   Read in exact write order; same Encoding on both sides.
 *
 * --- Structured binary file pattern ---
 *
 *   [magic bytes / version] [header counts]
 *   [record][record]…  each record = fixed sequence of Write calls
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Read return value ignored            | Partial buffer; garbage at tail
 *  Wrong Read order vs Write order      | Corrupt parse; EndOfStreamException
 *  FileInfo.Length before Flush         | Stale on-disk size while buffered
 *  Mismatched BinaryReader Encoding     | Wrong string lengths / mojibake
 *  Forgetting using / Dispose           | File locked until GC
 *
 * --- Related chapters ---
 *
 *   01. File and Directory Operations   File.Exists, copy, delete, enumerate
 *   02. StreamReader and StreamWriter   text I/O (not raw bytes)
 *   04. Path and Environment            Path.Combine, AppContext.BaseDirectory
 *
 * =============================================================================
 */
