# 03. FileStream & Binary Files — Interview Q&A


## Table of Contents

1. [Q1. What is the difference between the `File` static class, the `Stream` abstract base, and `FileStream`?](#q1-what-is-the-difference-between-the-file-static-class-the-stream-abstract-base-and-filestream)
2. [Q2. Explain each `FileMode` value and when to use it.](#q2-explain-each-filemode-value-and-when-to-use-it)
3. [Q3. What do `FileAccess` and `FileShare` control, and how do they interact between concurrent openers?](#q3-what-do-fileaccess-and-fileshare-control-and-how-do-they-interact-between-concurrent-openers)
4. [Q4. Why does the default `FileShare.None` cause sharing violations for concurrent processes?](#q4-why-does-the-default-filesharenone-cause-sharing-violations-for-concurrent-processes)
5. [Q5. What are `FileStream.Position`, `Seek`, and `Length`?](#q5-what-are-filestreamposition-seek-and-length)
6. [Q6. Explain `SeekOrigin.Begin`, `Current`, and `End` with a concrete binary-file scenario.](#q6-explain-seekoriginbegin-current-and-end-with-a-concrete-binary-file-scenario)
7. [Q7. What happens when you call `Seek` on a non-seekable stream?](#q7-what-happens-when-you-call-seek-on-a-non-seekable-stream)
8. [Q8. Why must the return value of `Stream.Read()` always be checked?](#q8-why-must-the-return-value-of-streamread-always-be-checked)
9. [Q9. When should you call `FileStream.Flush()` explicitly vs relying on `Dispose`?](#q9-when-should-you-call-filestreamflush-explicitly-vs-relying-on-dispose)
10. [Q10. What do `BinaryReader` and `BinaryWriter` add over raw `FileStream` byte operations?](#q10-what-do-binaryreader-and-binarywriter-add-over-raw-filestream-byte-operations)
11. [Q11. What is the on-disk format of `BinaryWriter.Write(string)`, and why is it .NET-specific?](#q11-what-is-the-on-disk-format-of-binarywriterwritestring-and-why-is-it-net-specific)
12. [Q12. Why must the read order in `BinaryReader` exactly match the write order in `BinaryWriter`?](#q12-why-must-the-read-order-in-binaryreader-exactly-match-the-write-order-in-binarywriter)
13. [Q13. What are file signatures (magic bytes), and why is the file extension an unreliable indicator of file type?](#q13-what-are-file-signatures-magic-bytes-and-why-is-the-file-extension-an-unreliable-indicator-of-file-type)
14. [Q14. When should you use binary files instead of text files?](#q14-when-should-you-use-binary-files-instead-of-text-files)
15. [Q15. What is the difference between `FileMode.Truncate` and `FileMode.Create`?](#q15-what-is-the-difference-between-filemodetruncate-and-filemodecreate)
16. [Q16. When should you use `MemoryStream` instead of `FileStream`?](#q16-when-should-you-use-memorystream-instead-of-filestream)
17. [Q17. What is buffered I/O, and how does `FileStream`'s buffer size affect performance?](#q17-what-is-buffered-io-and-how-does-filestreams-buffer-size-affect-performance)
18. [Q18. (Gotcha) Why does `BinaryReader.ReadBytes(n)` return a short array on truncated files without throwing?](#q18-gotcha-why-does-binaryreaderreadbytesn-return-a-short-array-on-truncated-files-without-throwing)
19. [Q19. (Gotcha) Why does checking `FileInfo.Length` immediately after a `BinaryWriter.Write` call return a stale size?](#q19-gotcha-why-does-checking-fileinfolength-immediately-after-a-binarywriterwrite-call-return-a-stale-size)
20. [Q20. (Gotcha) What does `leaveOpen: false` (the default) on `BinaryReader`/`BinaryWriter` do unexpectedly?](#q20-gotcha-what-does-leaveopen-false-the-default-on-binaryreaderbinarywriter-do-unexpectedly)
21. [Q21. (Scenario R) A telemetry service reads a 4-byte file signature but short files produce a garbage comparison result without throwing. What is wrong?](#q21-scenario-r-a-telemetry-service-reads-a-4-byte-file-signature-but-short-files-produce-a-garbage-comparison-result-without-throwing-what-is-wrong)
22. [Q22. (Scenario R) A background job appends binary audit records with `FileShare.None`; a dashboard reader gets `IOException`. What locking mismatch causes the failure?](#q22-scenario-r-a-background-job-appends-binary-audit-records-with-filesharenone-a-dashboard-reader-gets-ioexception-what-locking-mismatch-causes-the-failure)
23. [Q23. (Scenario R) A teammate ports `inventory.bin` to another language and swaps the field read order — prices and names are nonsense after the first record. What breaks?](#q23-scenario-r-a-teammate-ports-inventorybin-to-another-language-and-swaps-the-field-read-order-prices-and-names-are-nonsense-after-the-first-record-what-breaks)
24. [Q24. (Scenario P) A service reads a 2 GB binary sensor archive with `File.ReadAllBytes` on every API request — OOM under load. What is wrong?](#q24-scenario-p-a-service-reads-a-2-gb-binary-sensor-archive-with-filereadallbytes-on-every-api-request-oom-under-load-what-is-wrong)
25. [Q25. (Scenario D) A new field is added to an existing binary record format — how do you version a binary file format to avoid breaking old readers?](#q25-scenario-d-a-new-field-is-added-to-an-existing-binary-record-format-how-do-you-version-a-binary-file-format-to-avoid-breaking-old-readers)

---
> Back to [Module Index](../INTERVIEW_QA.md)

---

## Q1. What is the difference between the `File` static class, the `Stream` abstract base, and `FileStream`?

**Concepts**
- `File` — static utility class, one-shot operations, wraps `FileStream` internally
- `Stream` — abstract byte-I/O contract: `Read`, `Write`, `Seek`, `Flush`, `Position`, `Length`
- `FileStream` — concrete `Stream` backed by an OS file handle
- Lifecycle: `File` open-op-close per call; `FileStream` held open across operations
- `FileStream` provides `FileMode`, `FileAccess`, `FileShare` control

**Answer**

`File` is a purely static helper — every method (`ReadAllText`, `Create`, `Copy`) opens a `FileStream` internally, performs the operation, and closes it, with no persistent state in the caller. `Stream` is the abstract class that defines the byte-level contract shared by `FileStream`, `MemoryStream`, `NetworkStream`, and `CryptoStream`. `FileStream` is the concrete implementation that wraps an OS file handle, exposes `Position`, `Length`, `Seek`, and the core `Read`/`Write` methods, and keeps the handle open for as long as the object lives. I use `File` static methods for one-shot operations where simplicity matters and `FileStream` when I need the handle open across multiple operations, need precise `FileMode`/`FileAccess`/`FileShare` flags, need to seek between reads and writes, or need to pass the stream to `StreamReader`, `BinaryReader`, or a compression wrapper.

---

## Q2. Explain each `FileMode` value and when to use it.

**Concepts**
- `CreateNew` — atomic exclusive create; throws `IOException` if path exists
- `Create` — truncate if exists, create if not
- `Open` — open existing; throws `FileNotFoundException` if missing
- `OpenOrCreate` — open if present, create empty if not
- `Truncate` — open existing and set length to zero immediately
- `Append` — open or create, seek to EOF before each write

**Answer**

`CreateNew` is the right mode for idempotency guards — it atomically fails if the path already exists, letting callers catch `IOException` as the "already exists" case without a prior `File.Exists` check that would introduce a TOCTOU race. `Create` truncates an existing file or creates a new one; use it when old content must be discarded, as `File.Create` and `File.WriteAllText` do internally. `Open` opens an existing file and throws `FileNotFoundException` if absent, suitable when the file's prior existence is a precondition. `OpenOrCreate` opens if present or creates empty if not, positioned at byte zero. `Truncate` opens an existing file and immediately sets its length to zero while keeping the path and handle intact — use it to wipe and rewrite in place without deleting the file, preserving ACLs and alternate data streams. `Append` opens or creates and positions every write at EOF, making it safe for log files that must grow without manual seeking.

---

## Q3. What do `FileAccess` and `FileShare` control, and how do they interact between concurrent openers?

**Concepts**
- `FileAccess` — what operations this handle may perform (Read, Write, ReadWrite)
- `FileShare` — what concurrent openers are permitted (None, Read, Write, ReadWrite, Delete)
- OS enforces the intersection of all active handles' share declarations
- Mismatch between opener's requested access and holder's share → sharing violation
- `FileShare.None` is the exclusive lock — blocks all concurrent openers

**Answer**

`FileAccess` is a per-handle capability flag: `Read` permits only reads, `Write` permits only writes, and `ReadWrite` permits both — attempting an operation not covered throws at runtime. `FileShare` declares what the caller tolerates from other openers while its handle is open: `None` is a full exclusive lock, `Read` allows concurrent read-only opens, `Write` allows concurrent write opens, `ReadWrite` allows any combination, and `Delete` permits the file to be deleted (marked for deferred deletion on Windows) while the handle is open. The OS enforces the most-restrictive combination: if a first opener holds `FileShare.Read`, a second opener requesting `FileAccess.Write` is rejected with a sharing violation because the first side did not include `Write` in its share. Every sharing violation means at least one side's requested access conflicts with the other side's declared share.

---

## Q4. Why does the default `FileShare.None` cause sharing violations for concurrent processes?

**Concepts**
- Default `FileShare` is `None` when the parameter is omitted
- `FileShare.None` blocks all concurrent openers regardless of their access mode
- Production writer should use `FileShare.Read` to allow simultaneous readers
- Path-based `File.*` methods use internal defaults callers cannot see
- Explicit `FileStream` constructor required to control sharing

**Answer**

When a `FileStream` is constructed using a two-argument overload that omits `FileShare`, it defaults to `FileShare.None`, which instructs the OS to deny every other open on that path — including read-only opens. This means a background writer that opens with the default causes every reader, logger monitor, and health-check that tries to open the same file to receive `IOException: sharing violation`. The fix is to specify `FileShare.Read` on any writer that does not require write exclusivity: `new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read)`. For cases where multiple concurrent writers are legitimate — distributed append logs — all openers must use `FileShare.ReadWrite`, with application-level synchronization ensuring write ordering, because `FileShare` only resolves the open conflict, not the write-interleave problem.

---

## Q5. What are `FileStream.Position`, `Seek`, and `Length`?

**Concepts**
- `Position` — byte offset of the next read or write
- `Seek(offset, SeekOrigin)` — reposition without reading or writing
- `Length` — current file size in bytes
- `CanSeek` — false for non-seekable streams (NetworkStream, pipe streams)
- Setting `Position` directly is equivalent to `Seek(value, SeekOrigin.Begin)`

**Answer**

`Position` is the byte offset at which the next `Read` or `Write` operation will occur; read and write calls advance it automatically by the number of bytes transferred. `Seek(offset, origin)` repositions the pointer using one of three origins: `SeekOrigin.Begin` for an absolute offset from the start of the file, `SeekOrigin.Current` for an offset relative to the current position, and `SeekOrigin.End` for an offset measured backward from the end (typically a negative value). `Length` returns the total size of the file in bytes and may differ from `Position` — a file can be extended by seeking past EOF and writing, which fills the gap with zeros. Seeking is only valid when `CanSeek` returns `true`; calling `Seek` on a `NetworkStream`, pipe stream, or `CryptoStream` in certain modes throws `NotSupportedException`.

---

## Q6. Explain `SeekOrigin.Begin`, `Current`, and `End` with a concrete binary-file scenario.

**Concepts**
- `SeekOrigin.Begin` — absolute from file start for indexed record access
- `SeekOrigin.Current` — relative skip for parsing variable-length sections
- `SeekOrigin.End` — tail-read for footer validation
- Seek without reading/writing — pure position change
- Record index as offset table for random access

**Answer**

In a binary product catalog file, `SeekOrigin.Begin` is used when an index at byte 0 stores each record's byte offset from the start — `stream.Seek(recordOffsets[i], SeekOrigin.Begin)` jumps directly to record `i` without scanning. `SeekOrigin.Current` is used after reading a variable-length name field to skip a reserved padding block of known size: `stream.Seek(paddingBytes, SeekOrigin.Current)` advances past the padding without reading it. `SeekOrigin.End` is used to validate a file footer magic value: `stream.Seek(-8, SeekOrigin.End)` positions 8 bytes before EOF and `ReadBytes(8)` retrieves the footer — confirming file integrity without reading the entire body. Together these three origins cover indexed lookup, sequential parsing, and tail inspection, which are the three most common random-access patterns in binary file formats.

---

## Q7. What happens when you call `Seek` on a non-seekable stream?

**Concepts**
- `CanSeek` property indicating whether seeking is supported
- `NotSupportedException` thrown on seek attempt when `CanSeek` is false
- `NetworkStream`, pipe streams, `CryptoStream` in some modes as non-seekable
- `MemoryStream` as a seekable in-process workaround
- `CanRead`, `CanWrite`, `CanSeek` should be checked before use

**Answer**

Calling `Seek` or setting `Position` on a stream where `CanSeek` is `false` throws `NotSupportedException` immediately. `NetworkStream` is non-seekable because TCP delivers a strictly forward-only byte sequence with no mechanism to rewind. Pipe streams and `DeflateStream` used for decompression are also non-seekable in most configurations. When code receives a `Stream` parameter and the algorithm requires seeking — for example, reading a length prefix to allocate a buffer then seeking back to re-read — I check `stream.CanSeek` first and, if false, copy the relevant bytes into a `MemoryStream` which is always seekable. Using `CanRead`, `CanWrite`, and `CanSeek` as guards rather than catching `NotSupportedException` is the idiomatic check-then-act pattern for stream capabilities.

---

## Q8. Why must the return value of `Stream.Read()` always be checked?

**Concepts**
- `Stream.Read` may return fewer bytes than requested — legitimate short read
- Near EOF the available bytes may be less than the requested count
- Network and pipe streams deliver data in OS-sized chunks
- Ignoring the return count leads to partial buffer with zero-padded tail
- `BinaryReader.ReadBytes(n)` vs `Stream.Read` — different short-read semantics

**Answer**

`Stream.Read(buffer, offset, count)` is contractually permitted to return fewer bytes than `count` — this is not an error but a normal part of the API. For file streams opened on a local disk, a single `Read` call usually returns the requested count unless the stream is near EOF, but for network streams and certain OS configurations, partial reads are routine. Code that ignores the return value and assumes the buffer is fully populated reads garbage from the zero-initialized tail of the array, which causes silent data corruption in binary formats that rely on exact field widths. The correct pattern is a read loop that accumulates until `count` bytes have been received or the stream returns 0 (EOF). `BinaryReader.ReadBytes(n)` handles this internally and returns a shorter array if EOF is reached, while `ReadExactly` (available since .NET 7) throws `EndOfStreamException` on a short read — both eliminate the need for a manual accumulation loop.

---

## Q9. When should you call `FileStream.Flush()` explicitly vs relying on `Dispose`?

**Concepts**
- `Dispose` calls `Flush()` then releases the OS file handle
- `Flush()` pushes buffered bytes to the OS buffer cache without closing
- `FileOptions.WriteThrough` bypasses OS cache for crash-durability
- `Flush()` before checking `FileInfo.Length` from another code path
- Two-level flushing: writer's buffer then FileStream's buffer

**Answer**

`Dispose` (triggered at the end of a `using` block) calls `Flush()` internally before releasing the handle, so in a complete, exception-free execution path every write is persisted without an explicit `Flush()`. Explicit `Flush()` is necessary when the file handle must remain open but another piece of code — a poller, a dashboard reader, or a length check via `FileInfo` — needs to see the current content before the writer closes. The most common case is checking `stream.Length` from within the same block after a series of writes: the OS may show a smaller length while bytes sit in the internal buffer, so calling `Flush()` first ensures the reported size reflects all written data. `BinaryWriter` has its own buffer above `FileStream`'s buffer; for double-buffered chains, flush both: `writer.Flush(); stream.Flush()`. `FileOptions.WriteThrough` bypasses the OS write cache entirely for crash-durability, but at the cost of higher per-write latency.

---

## Q10. What do `BinaryReader` and `BinaryWriter` add over raw `FileStream` byte operations?

**Concepts**
- Typed read methods: `ReadInt32`, `ReadDouble`, `ReadBoolean`, `ReadString`, `ReadBytes`
- Typed write methods: `Write(int)`, `Write(double)`, `Write(bool)`, `Write(string)`, `Write(byte[])`
- Length-prefixed UTF-8 string handling (`ReadString` / `Write(string)`)
- `EndOfStreamException` on `ReadBytes` short read vs silent partial buffer from raw `Read`
- `BaseStream` property exposing the underlying stream

**Answer**

`BinaryReader` and `BinaryWriter` layer typed primitive serialization on top of any `Stream`, handling byte ordering, type sizes, and read loops internally. Instead of reading 4 bytes, calling `BitConverter.ToInt32(buffer, 0)`, and handling the little-endian assumption manually, I call `reader.ReadInt32()` and all of that is encapsulated. `BinaryWriter.Write(string)` encodes the string with a 7-bit variable-length length prefix followed by UTF-8 bytes; `ReadString()` reads it back — but since this format is .NET-specific, cross-language consumers cannot use it directly. `BinaryReader.ReadBytes(count)` is safer than a raw `stream.Read(buffer, 0, count)` call because it returns a new byte array and throws `EndOfStreamException` if the stream ends before `count` bytes are read, making short-read errors explicit rather than hidden in zero-padded buffers. `BaseStream` exposes the underlying stream so Position and Length can be checked without leaving the reader/writer wrapper.

---

## Q11. What is the on-disk format of `BinaryWriter.Write(string)`, and why is it .NET-specific?

**Concepts**
- 7-bit encoded variable-length integer as length prefix
- UTF-8 string bytes following the prefix
- 1-byte prefix for strings 0–127 chars; 2-byte for 128–16383 chars
- No standard equivalent in C, Python, Go, Java
- Cross-language interop requires fixed-size length prefix (2 or 4 bytes)

**Answer**

`BinaryWriter.Write(string)` serializes the string length using a 7-bit encoded variable-length integer — a format where each byte contributes 7 bits of the length value, with the high bit indicating whether another byte follows. Strings up to 127 bytes use a 1-byte prefix; up to 16,383 bytes use 2 bytes, and so on. The length is followed by the UTF-8 encoded string bytes. `BinaryReader.ReadString()` decodes this format correctly, but no standard library in C, Python, Go, Java, or JavaScript implements 7-bit encoded length prefixes. A cross-language consumer that attempts to read the prefix as a 1-byte or 4-byte integer will misparse the length, then misalign every subsequent field in the record. For binary files consumed outside .NET, I write strings with a fixed-size prefix — typically a 2-byte or 4-byte little-endian length followed by raw UTF-8 bytes — and document the layout in the file specification.

---

## Q12. Why must the read order in `BinaryReader` exactly match the write order in `BinaryWriter`?

**Concepts**
- `BinaryReader` advances `Position` on every typed read
- Each read consumes exactly the bytes that the matching write produced
- Wrong read type misaligns all subsequent field reads
- No field names or delimiters in binary format — layout is the schema
- Format specification or versioned header required for readers written separately

**Answer**

A binary file has no field names or delimiters — the layout is entirely defined by the sequence of write calls, and `BinaryReader` advances `Position` by the exact byte count each typed read consumes. If the writer writes `int` (4 bytes) followed by `double` (8 bytes), but the reader calls `ReadDouble()` first, it reads the 4-byte int as the first 8 bytes of the combined sequence, producing a garbage double, and then the subsequent `ReadInt32()` reads 4 bytes starting 8 bytes in — also garbage. The desynchronization propagates through every remaining field in the record. There is no error thrown because the bytes are valid for the types being read — the data is wrong but structurally parseable. The fix is to treat the write order as a formal contract: document it in a format specification, write a file-magic validation at the start of `Read`, and version the format header so new fields can be added without breaking old readers.

---

## Q13. What are file signatures (magic bytes), and why is the file extension an unreliable indicator of file type?

**Concepts**
- Magic bytes: fixed byte sequence at a known offset in the file
- Extension as a user-controlled, renameable string
- `BinaryReader.ReadBytes(n)` + `SequenceEqual` for signature validation
- Reject before parsing format-specific content to prevent malformed-data exploits
- Common signatures: PNG `89 50 4E 47`, ZIP `50 4B 03 04`, PDF `25 50 44 46`

**Answer**

A file signature is a fixed sequence of bytes at a known offset — typically the first few bytes — that identifies the file format independently of its name or extension. PNG files begin with `89 50 4E 47 0D 0A 1A 0A`, ZIP archives with `50 4B 03 04`, and PDF with `25 50 44 46`. The file extension is a string controlled entirely by the user or the upstream system; renaming `malware.exe` to `upload.png` changes the extension while the content remains identical. Validating by reading the expected bytes with `BinaryReader.ReadBytes(signatureLength)` and comparing with `SequenceEqual` against the known signature array rejects malformed or malicious files before any format-specific parsing begins — preventing exploits that depend on a parser consuming unexpected binary layouts. I always validate the signature as the first operation in a binary file reader, and I throw a descriptive `InvalidDataException` on mismatch rather than continuing with the wrong parser.

---

## Q14. When should you use binary files instead of text files?

**Concepts**
- Binary: compact storage, typed layout, random access by fixed record offsets
- Text: human-readable, line-oriented, toolable (grep, tail, Excel)
- Binary for: images, audio, custom protocols, game saves, sensor data archives
- Text for: logs, configuration, CSV, JSON, XML
- Mixed approach: text envelope with binary payload (e.g., HTTP with body)

**Answer**

Binary file handling reads and writes raw bytes with no encoding transformation — a 32-bit integer is always exactly 4 bytes on disk regardless of its value, while its text representation takes 1 to 10 bytes. This compactness and fixed field size enables random access via `Seek` to any record at a computed offset without scanning, which is impossible with text files because line lengths vary. Binary is the right choice for images, audio, custom network protocol frames, game save files, sensor archives, and any format where compact storage, interoperability with non-.NET code expecting a specific byte layout, or random access to records matters. Text is the right choice when the file must be inspectable with standard tools (`tail`, `grep`, Excel), when the format is configuration or log data consumed by multiple teams, or when the content changes are tracked in version control where diffs must be human-readable.

---

## Q15. What is the difference between `FileMode.Truncate` and `FileMode.Create`?

**Concepts**
- `Create` — creates new file or truncates existing; works whether file exists or not
- `Truncate` — requires file to exist; throws `FileNotFoundException` if missing
- Both set file length to zero immediately
- `Truncate` preserves ACLs, creation time, and the file's inode identity
- `Create` may change creation time on some OS configurations

**Answer**

Both `FileMode.Create` and `FileMode.Truncate` result in a zero-length file open for writing, but they differ on what must be true first. `Create` is unconditional — it creates the file if it does not exist or overwrites it to zero bytes if it does. `Truncate` requires the file to already exist and throws `FileNotFoundException` if it does not; it then sets the length to zero while keeping the file's ACLs, creation timestamp, inode identity, and alternate data streams intact. The practical difference matters when other components track the file by path identity: a process watching the path via a file-system watcher may see a delete-and-create event from `Create` but only a modify event from `Truncate`, depending on the OS implementation. Use `Truncate` when in-place wiping is required to preserve file identity; use `Create` when the file may or may not exist and you always want a clean start.

---

## Q16. When should you use `MemoryStream` instead of `FileStream`?

**Concepts**
- `MemoryStream` — in-process seekable byte buffer, no disk I/O
- Unit testing: feed pre-built bytes through code that accepts `Stream`
- Intermediate accumulation before sending to network or compressor
- `ToArray()` for snapshot; `GetBuffer()` for zero-copy access to internal buffer
- Large payloads: `MemoryStream` grows internal array on heap (LOH risk)

**Answer**

I use `MemoryStream` when I need a seekable, in-memory byte buffer without touching the filesystem — for unit-testing binary serialization code that accepts a `Stream`, for accumulating bytes from multiple writes before sending them to a network socket or compression stream in one shot, or for decoding a byte array into typed values via `BinaryReader` without creating a temp file. Passing `new MemoryStream(bytes)` wraps an existing byte array as a readable stream, and `stream.Position = 0` after writing resets it for reading — the standard test-serializer-then-deserialize pattern. For large payloads, `MemoryStream` is not appropriate because it doubles its internal buffer on each resize, leading to large LOH allocations and compaction pressure; for multi-megabyte intermediate data, prefer a temp file on the destination volume or an `ArrayPool<byte>`-backed custom stream.

---

## Q17. What is buffered I/O, and how does `FileStream`'s buffer size affect performance?

**Concepts**
- Managed buffer reducing syscall frequency for small reads/writes
- Default 4096-byte internal buffer on `FileStream`
- Larger buffer (64 KB) improving throughput for sequential large-file access
- `FileOptions.SequentialScan` hinting OS to prefetch pages ahead
- `FileOptions.WriteThrough` bypassing OS write cache for durability

**Answer**

`FileStream` maintains an internal managed byte buffer — defaulting to 4096 bytes — that batches small reads and writes to reduce the number of kernel transitions. One thousand 4-byte writes to an unbuffered stream generate 1000 syscalls; with a 4096-byte buffer they generate roughly one syscall per 1024 writes, dramatically reducing overhead. Increasing the buffer to 65,536 bytes benefits sequential large-file reads and writes by aligning better with disk block sizes and reducing context-switch frequency, but wastes memory for random-access patterns where the buffer is rarely filled. `FileOptions.SequentialScan` hints the OS to eagerly prefetch pages ahead of the current position, improving throughput for single-pass large-file reads. `FileOptions.WriteThrough` bypasses the OS write cache and writes directly to stable storage, increasing per-write latency but ensuring data survives a process crash — appropriate for write-ahead logs and status files that must be durable.

---

## Q18. (Gotcha) Why does `BinaryReader.ReadBytes(n)` return a short array on truncated files without throwing?

**Concepts**
- `ReadBytes(n)` reads up to `n` bytes and returns whatever is available
- Returns an array shorter than `n` at EOF — no exception
- `EndOfStreamException` only thrown by `Read*` typed methods when EOF is hit mid-type
- Explicit `length == expectedLength` check required before using the result
- `.NET 7` `ReadExactly(n)` throws `EndOfStreamException` on short read

**Answer**

`BinaryReader.ReadBytes(n)` reads up to `n` bytes from the stream and returns the bytes actually read — if the stream is at or near EOF, it returns a shorter array without throwing any exception. This is by design: the method is documented as returning `byte[]` of length 0 to `n`. Code that calls `SequenceEqual` on the returned array against a 4-byte signature without checking that `readBytes.Length == 4` first will receive `false` for a file that is only 2 bytes long — the file is rejected, but for the wrong reason with no actionable diagnostic. The fix is to check the returned array length explicitly and throw a descriptive exception when it is shorter than expected: `if (magic.Length < 4) throw new InvalidDataException("File too short to contain a valid signature")`. On .NET 7+, `reader.ReadExactly(buffer, 0, 4)` throws `EndOfStreamException` when the stream ends before 4 bytes are read, eliminating the length check.

---

## Q19. (Gotcha) Why does checking `FileInfo.Length` immediately after a `BinaryWriter.Write` call return a stale size?

**Concepts**
- `BinaryWriter` has an internal buffer above the `FileStream`
- `FileStream` has its own internal buffer above the OS file cache
- `FileInfo.Length` reads from disk-visible size, not the in-process buffer
- `writer.Flush(); stream.Flush()` required before `FileInfo.Length` reflects all writes
- `new FileInfo(path).Length` vs `stream.Length` — stream's own Length is authoritative

**Answer**

`BinaryWriter` maintains its own internal buffer, and `FileStream` maintains another buffer below it. When a write call fills neither buffer completely, the bytes sit in the writer's buffer and `stream.Length` reflects only what has been committed through both buffers to the OS. `FileInfo.Length` queries the OS for the file's metadata size, which matches only what has been flushed through `FileStream` to the kernel — typically the last full block boundary. Calling `writer.Flush()` pushes the writer's buffer into the `FileStream`, and `stream.Flush()` then pushes `FileStream`'s buffer to the OS. After both calls, `stream.Length` and a freshly constructed `new FileInfo(path).Length` both return the expected size. The simplest check is `stream.Length` from within the `using` block after both flushes rather than constructing a `FileInfo` externally.

---

## Q20. (Gotcha) What does `leaveOpen: false` (the default) on `BinaryReader`/`BinaryWriter` do unexpectedly?

**Concepts**
- `leaveOpen: false` — disposing the reader/writer also disposes the underlying stream
- Common bug: wrapping a single `FileStream` in sequential reader and writer
- `ObjectDisposedException` on subsequent access after first wrapper disposes
- `leaveOpen: true` required when the stream must survive the wrapper's disposal

**Answer**

`BinaryReader` and `BinaryWriter` constructors default to `leaveOpen: false`, which means when the reader or writer is disposed, it calls `Dispose()` on the underlying stream as well. This is correct when the wrapper is the sole owner of the stream, but it becomes a bug when a single `FileStream` is passed to sequential wrappers — for example, writing a header with a `BinaryWriter`, then reading back part of the file with a `BinaryReader` on the same stream. Disposing the writer after the header write closes the `FileStream`, so the subsequent `BinaryReader` construction or read throws `ObjectDisposedException`. The fix is to pass `leaveOpen: true` to both wrappers and dispose the `FileStream` explicitly after both are done. This pattern is common in format-patching utilities and test fixtures that reuse a single `MemoryStream` through multiple serialization passes.

---

## Q21. (Scenario R) A telemetry service reads a 4-byte file signature but short files produce a garbage comparison result without throwing. What is wrong?

**Defective code:**

```csharp
public static bool ValidateSignature(string path)
{
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    using var reader = new BinaryReader(stream);
    byte[] magic = reader.ReadBytes(4);                    // BUG: returns short array on truncated file
    return magic.SequenceEqual(new byte[] { 0x49, 0x4E, 0x56, 0x31 }); // "INV1"
    // 2-byte file returns 2-element array; SequenceEqual returns false, not an exception
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ReadBytes(4)` returns a 2-element array for a 2-byte file; `SequenceEqual` against 4 bytes silently returns false | File rejected without a diagnostic message about why (file too short vs wrong format) |
| Diagnostics | Caller sees `false` return — indistinguishable from a wrong-format file vs a corrupted/truncated file | No actionable error for operators debugging an ingestion failure |
| Security | Truncated malicious files bypass format-specific validation silently | Downstream parsing may attempt to read further fields from a dangerously short file |

**Fix priority**

1. Check `magic.Length == 4` before calling `SequenceEqual` and throw `InvalidDataException("File too short")` when shorter.
2. Or use `reader.Read(buffer, 0, 4)` in a loop / `.NET 7` `ReadExactly` which throws `EndOfStreamException` on short read.
3. Add a separate file-length pre-check: if `new FileInfo(path).Length < minimumExpectedSize` throw immediately.

```csharp
public static void ValidateSignature(string path)
{
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    using var reader = new BinaryReader(stream);
    byte[] magic = reader.ReadBytes(4);
    if (magic.Length < 4)
        throw new InvalidDataException("File is too short to contain a valid signature.");
    if (!magic.SequenceEqual(new byte[] { 0x49, 0x4E, 0x56, 0x31 }))
        throw new InvalidDataException("File signature mismatch — not a valid inventory file.");
}
```

---

## Q22. (Scenario R) A background job appends binary audit records with `FileShare.None`; a dashboard reader gets `IOException`. What locking mismatch causes the failure?

**Defective code:**

```csharp
// Background job (writer):
using var stream = new FileStream(auditPath, FileMode.Append,
    FileAccess.Write);                              // BUG: FileShare defaults to None — exclusive lock

// Dashboard reader (concurrent):
using var readStream = new FileStream(auditPath, FileMode.Open,
    FileAccess.Read, FileShare.Read);               // FAILS: writer holds exclusive lock
using var reader = new BinaryReader(readStream);
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Locking | Omitting `FileShare` defaults to `FileShare.None` — no concurrent openers permitted | Dashboard reader throws `IOException: sharing violation` whenever the writer is active |
| Availability | Dashboard is unavailable during audit write windows, which may be long-running | Monitoring blind spots during the most active write periods |
| Subtle default | `FileShare` omission is invisible in the constructor call — looks like a valid open | Hard to diagnose without knowing the default value |

**Fix priority**

1. Change the writer to `FileShare.Read` — it retains exclusive write access but allows concurrent readers.
2. If multiple concurrent writers are required, both sides must use `FileShare.ReadWrite` with application-level write serialization.
3. Use `FileShare.ReadWrite` on the dashboard reader as a defensive default to tolerate writers regardless of their share configuration.

---

## Q23. (Scenario R) A teammate ports `inventory.bin` to another language and swaps the field read order — prices and names are nonsense after the first record. What breaks?

**Concepts**
- Binary format as a strict positional byte contract
- `BinaryReader` advancing `Position` on every typed read
- Field-order mismatch desynchronizing all subsequent field reads
- Format specification required for cross-language implementations
- Magic-byte version header for format evolution

**Answer**

A binary file has no delimiters, no field names, and no schema embedded in the data — the layout is entirely the sequence of write calls. When the inventory file writes `Int32 id`, `String name`, `Double price`, `Boolean inStock`, the bytes on disk for one record are exactly `[4 bytes][variable name bytes][8 bytes][1 byte]` in that order. A reader written in another language that calls `ReadDouble` before `ReadString` reads the variable-length name bytes as if they were a double, producing a nonsense floating-point value, and every subsequent read is misaligned by however many bytes the name occupied. The values are not corrupted in the sense of being invalid bytes — they are valid doubles and ints, just the wrong bytes interpreted as the wrong types. The fix is a formal binary format specification document that precisely lists each field's position, type, byte count, and endianness, along with a file-header version byte so future format changes are detectable before parsing.

---

## Q24. (Scenario P) A service reads a 2 GB binary sensor archive with `File.ReadAllBytes` on every API request — OOM under load. What is wrong?

**Defective code:**

```csharp
app.MapGet("/sensors/{deviceId}/archive", async (string deviceId, IArchiveStore store) =>
{
    string path = store.GetArchivePath(deviceId);
    byte[] all = File.ReadAllBytes(path);           // BUG: 2 GB byte[] on LOH per request
    var records = SensorCodec.DecodeAll(all);       // BUG: decodes from full byte[] in memory
    return Results.Ok(records.TakeLast(1000));
});
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Memory | `File.ReadAllBytes` allocates the entire 2 GB file as a single LOH `byte[]` per request | OOM under concurrent requests; GC pressure even under light load |
| Threading | Synchronous `File.ReadAllBytes` blocks a thread-pool thread | Thread-pool starvation at moderate concurrency |
| Waste | Full file read when only the last 1000 records are needed | 2 GB disk I/O to return ~8 KB of records |

**Fix priority**

1. Seek near EOF — compute the byte offset of the last 1000 records from the record size and use `stream.Seek(-recordBytes, SeekOrigin.End)`.
2. Use streaming decode: open `FileStream` with `FileOptions.Asynchronous`, wrap in `BinaryReader`, and decode records in a loop with `await` calls.
3. Cache the last-N-records result if freshness latency permits.

---

## Q25. (Scenario D) A new field is added to an existing binary record format — how do you version a binary file format to avoid breaking old readers?

**Concepts**
- File-level version field in the header (magic bytes + version byte/int)
- Old readers check version and reject or skip unknown versions
- Forward compatibility: old reader skips unknown fields using length-prefix or offset-table
- Backward compatibility: new reader handles old version records
- Separate codec classes per version vs a single codec with version branching

**Answer**

The standard approach is to embed a version number in the file header immediately after the magic bytes — typically a `uint16` or `uint32` that identifies the format version. Old readers that encounter a version number higher than they understand throw a descriptive `InvalidDataException("Unsupported format version")` rather than attempting to parse fields they do not know about. For forward compatibility — allowing old readers to skip new fields — each record's variable section is preceded by a length field so old readers can `Seek` past the section without needing to know its structure. New readers check the version and branch to the appropriate deserializer: a `switch (version)` that delegates to `ReadV1`, `ReadV2`, etc., keeping each version's logic isolated. For files that must support incremental field additions without a full version bump, a tag-length-value (TLV) encoding within each record allows unknown tags to be skipped by length without a version increment.

## Gotchas — FileStream & Binary Files (Interview Traps)

---

#### Gotcha 1. `FileStream.Position` Advances After Every Read or Write — Seek to Re-Read

**Concepts**
- Every `Read` and `Write` call advances `Position` by the number of bytes transferred
- To re-read from the beginning after writing, call `stream.Seek(0, SeekOrigin.Begin)` or set `stream.Position = 0`
- Forgetting to seek is the most common binary file bug when both reading and writing in the same session
- Not all streams are seekable; check `stream.CanSeek` before calling `Seek`

**Answer**

After `fileStream.Write(buffer, 0, buffer.Length)`, `fileStream.Position` equals the number of bytes written. A subsequent `fileStream.Read(readBuffer, 0, readBuffer.Length)` reads from that advanced position, not from the start of the file. For patterns that write then read back (verification, test helpers, in-memory round-trips), call `fileStream.Seek(0, SeekOrigin.Begin)` or assign `fileStream.Position = 0` between the write and the read.

---

#### Gotcha 2. `BinaryWriter` Writes Little-Endian — Not Portable Across Platforms Without Conversion

**Concepts**
- `BinaryWriter.Write(int)` writes a 4-byte integer in little-endian byte order on all .NET platforms
- Big-endian systems and network protocols (TCP/IP) use big-endian byte order
- Use `IPAddress.HostToNetworkOrder(value)` to convert to big-endian before writing to a network buffer
- For cross-language binary files, document the byte order explicitly in the format specification

**Answer**

`binaryWriter.Write(0x01020304)` writes bytes `04 03 02 01` in the file — little-endian. On a big-endian consumer (a Java application or a network protocol parser expecting `01 02 03 04`), the value is read as `0x04030201`. For network protocols, use `IPAddress.HostToNetworkOrder` before writing. For file formats shared with other languages, use `BinaryPrimitives.WriteInt32BigEndian(span, value)` from `System.Buffers.Binary` for explicit, documented byte-order control.

---

#### Gotcha 3. `FileStream` with `FileShare.None` — Exclusive Lock Blocks Other Processes

**Concepts**
- `FileShare.None` prevents any other process (or thread in another `FileStream`) from opening the file
- While the stream is open, other processes receive `IOException: The process cannot access the file`
- `FileShare.Read` allows concurrent readers but blocks writers
- `FileShare.ReadWrite` allows all access; callers must handle concurrent modification themselves

**Answer**

`new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)` places an exclusive lock on the file. Any other process (backup tool, antivirus, deployment script) that tries to open the file receives an access-denied exception for the duration. This is appropriate for write-critical operations like database file access, but for most application files `FileShare.Read` is a better default — it allows concurrent readers without risking corruption.

---

#### Gotcha 4. `FileStream.Read` May Return Fewer Bytes Than Requested — Always Loop

**Concepts**
- `Read(buffer, offset, count)` returns the number of bytes actually read, which may be less than `count`
- A single call to `Read` is not guaranteed to fill the buffer, especially on network streams or at end of file
- Must loop until the desired byte count is accumulated or EOF is detected
- `ReadExactly(buffer, offset, count)` (.NET 7+) handles the loop internally and throws if EOF is reached

**Answer**

`stream.Read(buffer, 0, 100)` may return any value from 0 to 100 — a partial read is valid and common, especially on network streams. Code that assumes the return value equals `count` will silently process incomplete data. The correct pattern is a loop: `int totalRead = 0; while (totalRead < count) { int n = stream.Read(buffer, totalRead, count - totalRead); if (n == 0) break; totalRead += n; }`. In .NET 7+, `stream.ReadExactly(buffer, 0, count)` implements this loop and throws `EndOfStreamException` if the stream ends before `count` bytes are read.

---

#### Gotcha 5. `BinaryReader.ReadString()` Uses a .NET-Specific Length-Prefix Format

**Concepts**
- `ReadString()` reads a 7-bit encoded integer length prefix, then reads that many UTF-8 bytes
- This format is specific to .NET's `BinaryWriter`/`BinaryReader` pair and is not interoperable with other languages
- A file written with `BinaryWriter.Write(string)` cannot be read correctly by Java, Python, or C without understanding the .NET length encoding
- For cross-platform binary formats, encode string length as a fixed-size integer and document the encoding

**Answer**

`binaryWriter.Write("hello")` writes a 7-bit variable-length integer for the byte count (1 byte for `hello` = 5) followed by the UTF-8 bytes. This length-prefix scheme is not documented in any cross-platform standard. A Python script trying to read the file with a fixed 4-byte length prefix will misparse the data. When writing binary files that must be read by other languages, use a fixed-width length field (`BinaryPrimitives.WriteInt32LittleEndian`) and document the encoding and endianness explicitly.

---

#### Gotcha 6. `FileMode.Append` Appends on Open but Is Not Suitable for Random-Access Writes

**Concepts**
- `FileMode.Append` opens or creates the file and sets the initial position to the end of the file
- On Windows, `FileAccess.Write` is required with `FileMode.Append`; `FileAccess.ReadWrite` throws
- Seeking to a position before the start of the appended content is not allowed — `SeekOrigin.Begin` throws
- For true random-access write-and-read, use `FileMode.Open` or `FileMode.OpenOrCreate` with `FileAccess.ReadWrite`

**Answer**

`new FileStream(path, FileMode.Append)` is designed for sequential tail-writes only. Attempting to `Seek` to a position earlier than the append offset throws `IOException: Seek to a negative position is not allowed`, because the OS restricts append-mode file descriptors. For a pattern where you open a file, write new data, and then re-read some of it, use `FileMode.OpenOrCreate` with `FileAccess.ReadWrite` and manage the position manually.

---

#### Gotcha 7. `FileStream` Async — `useAsync: true` Required for True Async I/O on Windows

**Concepts**
- On Windows, file I/O is synchronous by default even when using `await ReadAsync()`
- Without `FileOptions.Asynchronous` (or `useAsync: true`), async methods run synchronously on a thread-pool thread
- `new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true)` enables overlapped I/O
- Alternatively, `FileOptions.Asynchronous` is the named flag version of `useAsync: true`

**Answer**

`await fileStream.ReadAsync(buffer, 0, buffer.Length)` on a `FileStream` opened without `FileOptions.Asynchronous` completes synchronously on Windows — the thread-pool thread blocks during the I/O operation, negating the async benefit. To get true non-blocking I/O on Windows, open the stream with `new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous)`. On Linux and macOS, file I/O behaves differently at the OS level, but specifying `Asynchronous` is still the correct portable practice.

---

#### Gotcha 8. Disposing `BinaryWriter` Disposes the Underlying Stream — Use `leaveOpen: true` to Prevent This

**Concepts**
- `BinaryWriter.Dispose()` calls `Dispose()` on the underlying `Stream` by default
- After disposing the `BinaryWriter`, any reference to the original stream is also closed
- Pass `leaveOpen: true` to the constructor to keep the stream open after the writer is disposed
- Same pattern applies to `BinaryReader`, `StreamReader`, `StreamWriter`, and `GZipStream`

**Answer**

`using var writer = new BinaryWriter(memoryStream);` disposes `memoryStream` when the `using` block exits. Any code that tries to read from `memoryStream` after the block will get `ObjectDisposedException`. The fix is `new BinaryWriter(memoryStream, Encoding.UTF8, leaveOpen: true)` — the stream remains open after the writer is disposed, and you can seek to position 0 and read it back. This is the standard pattern for pipelines that pass one stream through multiple wrappers.

---

#### Gotcha 9. `FileStream.Length` Can Be Expensive on Some File Systems — Cache If Called Frequently

**Concepts**
- `FileStream.Length` queries the file system for the current file size on each call
- On network file systems (NFS, SMB) or virtual file systems, repeated calls can incur round-trip latency
- Cache the length in a local variable when it is used repeatedly in a loop
- For files being actively written by another process, the cached length may be stale — weigh freshness vs performance

**Answer**

`for (int i = 0; i < stream.Length; i++)` calls `stream.Length` on every iteration — potentially a file system stat call each time on network or virtual file systems. Cache it: `long length = stream.Length; for (long i = 0; i < length; i++)`. For local SSD-backed streams the difference is negligible, but for network-mounted storage it can be significant. When the file size changes during processing (e.g., another writer is appending), decide whether a stale cached value is acceptable or a fresh read is needed.

---

#### Gotcha 10. `File.OpenRead()` Is Equivalent to `new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)`

**Concepts**
- `File.OpenRead(path)` is a convenience factory for a read-only, file-share-read `FileStream`
- The default `FileShare.Read` allows other processes to read the file simultaneously
- It does not allow writing; `FileAccess.Read` means any attempt to call `Write()` throws `NotSupportedException`
- For write access or a different sharing mode, construct `FileStream` directly with the desired flags

**Answer**

`File.OpenRead(path)` is exactly `new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)` — a read-only stream with shared read access. It is the clearest way to express "I am opening this file only to read it, and other readers are welcome." When you need to write, append, or use exclusive access, construct a `FileStream` directly with the explicit `FileAccess` and `FileShare` flags that match your intent, rather than calling a convenience factory and then being surprised by `NotSupportedException`.

---
