# 02. StreamReader & StreamWriter — Interview Q&A


## Table of Contents

1. [Q1. What are `TextReader` and `TextWriter`, and why do APIs accept these abstractions instead of `StreamReader`/`StreamWriter`?](#q1-what-are-textreader-and-textwriter-and-why-do-apis-accept-these-abstractions-instead-of-streamreaderstreamwriter)
2. [Q2. What are the key `StreamWriter` constructor overloads, and what does each imply?](#q2-what-are-the-key-streamwriter-constructor-overloads-and-what-does-each-imply)
3. [Q3. What are the key `StreamReader` constructor overloads?](#q3-what-are-the-key-streamreader-constructor-overloads)
4. [Q4. How do `ReadLine`, `ReadToEnd`, and `ReadBlock` differ for large files?](#q4-how-do-readline-readtoend-and-readblock-differ-for-large-files)
5. [Q5. What is the default encoding of `StreamReader` and `StreamWriter`, and when does `Encoding.Default` cause mojibake?](#q5-what-is-the-default-encoding-of-streamreader-and-streamwriter-and-when-does-encodingdefault-cause-mojibake)
6. [Q6. What is the difference between `Encoding.UTF8` and `new UTF8Encoding(false)`?](#q6-what-is-the-difference-between-encodingutf8-and-new-utf8encodingfalse)
7. [Q7. What is `StreamWriter.AutoFlush`, and what is the performance trade-off vs explicit `Flush()`?](#q7-what-is-streamwriterautoflush-and-what-is-the-performance-trade-off-vs-explicit-flush)
8. [Q8. When must you call `Flush()` explicitly, and when is `Dispose` enough?](#q8-when-must-you-call-flush-explicitly-and-when-is-dispose-enough)
9. [Q9. What happens when a `StreamWriter` is not disposed?](#q9-what-happens-when-a-streamwriter-is-not-disposed)
10. [Q10. What are `StringReader` and `StringWriter`, and when should you use them?](#q10-what-are-stringreader-and-stringwriter-and-when-should-you-use-them)
11. [Q11. What is the correct loop pattern for `ReadLine()`, and what is the common mistake with empty-line detection?](#q11-what-is-the-correct-loop-pattern-for-readline-and-what-is-the-common-mistake-with-empty-line-detection)
12. [Q12. When is `StreamReader.EndOfStream` available, and when should you use `ReadLine() == null` instead?](#q12-when-is-streamreaderendofstream-available-and-when-should-you-use-readline-null-instead)
13. [Q13. What does `leaveOpen: true` do in `StreamReader`/`StreamWriter` constructors?](#q13-what-does-leaveopen-true-do-in-streamreaderstreamwriter-constructors)
14. [Q14. How do `ReadLineAsync`, `WriteLineAsync`, and `ReadToEndAsync` free thread-pool threads?](#q14-how-do-readlineasync-writelineasync-and-readtoendasync-free-thread-pool-threads)
15. [Q15. What does `StreamReader.Peek()` do, and when is it useful?](#q15-what-does-streamreaderpeek-do-and-when-is-it-useful)
16. [Q16. What is the difference between wrapping a `FileStream` in `StreamWriter` vs using the path constructor?](#q16-what-is-the-difference-between-wrapping-a-filestream-in-streamwriter-vs-using-the-path-constructor)
17. [Q17. (Gotcha) Why does `ReadToEnd()` on a 10 GB customer log cause `OutOfMemoryException`?](#q17-gotcha-why-does-readtoend-on-a-10-gb-customer-log-cause-outofmemoryexception)
18. [Q18. (Gotcha) How does an encoding mismatch between writer and reader cause mojibake silently?](#q18-gotcha-how-does-an-encoding-mismatch-between-writer-and-reader-cause-mojibake-silently)
19. [Q19. (Gotcha) Why does `while (line != string.Empty)` terminate early on blank lines?](#q19-gotcha-why-does-while-line-stringempty-terminate-early-on-blank-lines)
20. [Q20. (Gotcha) What is the performance cost of `AutoFlush = true` in a high-throughput logger?](#q20-gotcha-what-is-the-performance-cost-of-autoflush-true-in-a-high-throughput-logger)
21. [Q21. (Scenario R) A nightly audit job throws on bad rows and the log file stays locked until the worker restarts. What is wrong?](#q21-scenario-r-a-nightly-audit-job-throws-on-bad-rows-and-the-log-file-stays-locked-until-the-worker-restarts-what-is-wrong)
22. [Q22. (Scenario R) A CSV export written on Windows with `Encoding.Default` fails header validation on Linux containers. What is the root cause?](#q22-scenario-r-a-csv-export-written-on-windows-with-encodingdefault-fails-header-validation-on-linux-containers-what-is-the-root-cause)
23. [Q23. (Scenario R) A support dashboard calls `ReadToEnd()` on customer logs up to 10 GB — `OutOfMemoryException` under concurrent load. What is the replacement?](#q23-scenario-r-a-support-dashboard-calls-readtoend-on-customer-logs-up-to-10-gb-outofmemoryexception-under-concurrent-load-what-is-the-replacement)
24. [Q24. (Scenario R) A status file writer sets `IN_PROGRESS` then `COMPLETE`, but operators see `IN_PROGRESS` forever after a crash. What causes it?](#q24-scenario-r-a-status-file-writer-sets-inprogress-then-complete-but-operators-see-inprogress-forever-after-a-crash-what-causes-it)
25. [Q25. (Scenario R) A log tailer and log writer in the same app produce intermittent `IOException: sharing violation`. What sharing rule is missing?](#q25-scenario-r-a-log-tailer-and-log-writer-in-the-same-app-produce-intermittent-ioexception-sharing-violation-what-sharing-rule-is-missing)
26. [Q26. (Scenario P) An ASP.NET Core hosted service calls synchronous `ReadLine()` inside an async loop. Thread-pool queue depth grows under load. What is the problem?](#q26-scenario-p-an-aspnet-core-hosted-service-calls-synchronous-readline-inside-an-async-loop-thread-pool-queue-depth-grows-under-load-what-is-the-problem)

---
> Back to [Module Index](../INTERVIEW_QA.md)

---

## Q1. What are `TextReader` and `TextWriter`, and why do APIs accept these abstractions instead of `StreamReader`/`StreamWriter`?

**Concepts**
- `TextReader`/`TextWriter` as abstract character-I/O contracts
- `StreamReader`/`StreamWriter` as stream-backed implementations
- `StringReader`/`StringWriter` as in-memory implementations
- Testability: inject `StringReader` without disk I/O
- Polymorphic parsers reusable with files and in-memory strings

**Answer**

`TextReader` defines a character-level reading contract — `Read`, `ReadLine`, `ReadToEnd`, `ReadBlock`, `Peek` — without specifying where the characters come from. `TextWriter` provides the writing contract — `Write`, `WriteLine`, `Flush`, `Encoding`. `StreamReader` and `StreamWriter` implement these abstractions backed by a `Stream`; `StringReader` and `StringWriter` implement them backed by a `string` or `StringBuilder`. When a parser method accepts `TextReader` rather than `StreamReader`, passing `new StringReader("csv,data,here")` in a unit test eliminates filesystem dependency entirely, and the production code passes a `new StreamReader(filePath)` without any change. APIs like `XmlReader.Create(TextReader)` and `JsonSerializer` follow this pattern for the same reason: the parsing logic is completely decoupled from the I/O source.

---

## Q2. What are the key `StreamWriter` constructor overloads, and what does each imply?

**Concepts**
- `new StreamWriter(path)` — create/overwrite, UTF-8 no-BOM default
- `new StreamWriter(path, append: true)` — open or create, seek to EOF
- `new StreamWriter(path, append, encoding)` — explicit encoding
- `new StreamWriter(stream)` / `new StreamWriter(stream, encoding)` — wraps any `Stream`
- `leaveOpen` parameter controlling whether `Dispose` closes the underlying stream

**Answer**

The simplest path constructor `new StreamWriter(path)` creates or overwrites the file and uses UTF-8 without a BOM, which is the right default for most modern interchange. `new StreamWriter(path, append: true)` opens with `FileMode.Append` and positions writes at end-of-file, suitable for growing log files; internally it still uses the default UTF-8 encoding unless a third argument is provided. The explicit encoding overload `new StreamWriter(path, append, encoding)` is necessary when writing files for legacy Windows applications expecting `Encoding.GetEncoding(1252)` or for partners requiring a specific BOM. When wrapping an existing `FileStream` — useful when precise `FileMode`, `FileAccess`, and `FileShare` flags are needed — pass the stream to `new StreamWriter(stream, encoding, bufferSize, leaveOpen)`, where `leaveOpen: true` prevents the writer from closing the underlying stream when it disposes.

---

## Q3. What are the key `StreamReader` constructor overloads?

**Concepts**
- `new StreamReader(path)` — default UTF-8 with BOM detection enabled
- `new StreamReader(path, encoding)` — explicit encoding, overrides BOM detection
- `detectEncodingFromByteOrderMarks` parameter — auto-detects UTF-8/UTF-16 BOM
- `new StreamReader(stream)` / `new StreamReader(stream, encoding)` — wraps any `Stream`
- `leaveOpen` to prevent closing the wrapped stream

**Answer**

The path constructor `new StreamReader(path)` defaults to UTF-8 with `detectEncodingFromByteOrderMarks: true`, which means it switches to the BOM-indicated encoding if a BOM is present at the start. This auto-detection is convenient but can surprise callers: a file with a UTF-16 LE BOM will be read as UTF-16 even if the caller expected UTF-8. When the encoding is known and guaranteed by the format contract, pass it explicitly — `new StreamReader(path, new UTF8Encoding(false))` — and set `detectEncodingFromByteOrderMarks: false` if available in the overload. Wrapping a `Stream` is used when the `FileStream` is opened with specific `FileMode`/`FileShare` flags that the path constructor does not expose. Pass `leaveOpen: true` when the `Stream` must remain open after the reader is disposed — for example, when the same `MemoryStream` is reused for reading and the reader should not close it.

---

## Q4. How do `ReadLine`, `ReadToEnd`, and `ReadBlock` differ for large files?

**Concepts**
- `ReadLine` — one line per call, memory bounded by longest line
- `ReadToEnd` — entire remaining content as one `string`, LOH risk
- `ReadBlock` — fills caller-supplied `char[]` buffer, predictable allocation
- Use case selection based on file size and processing pattern
- `ReadLineAsync` for async pipelines

**Answer**

`ReadLine` reads until the next line terminator and returns one `string`, keeping heap allocation proportional to the longest line regardless of file size — the correct pattern for line-oriented large-file processing. `ReadToEnd` reads everything from the current position to EOF into a single `string`, which is fine for configuration files under a few kilobytes but allocates a potentially enormous LOH object for multi-megabyte files and blocks the thread for the full read duration. `ReadBlock` fills a caller-supplied `char[]` buffer up to a specified count, returning the actual number of characters read — suited for fixed-chunk processing or piping into another writer with minimal allocation since the buffer is reused across iterations. For anything over a few megabytes, `ReadLine` in a loop or `ReadLineAsync` in an async context is the standard approach; `ReadToEnd` is only appropriate for small, bounded content.

---

## Q5. What is the default encoding of `StreamReader` and `StreamWriter`, and when does `Encoding.Default` cause mojibake?

**Concepts**
- `StreamWriter` path constructor default: UTF-8 without BOM
- `StreamReader` path constructor default: UTF-8 with BOM detection
- `Encoding.Default` resolves to system code page (Windows-1252 on Windows, UTF-8 on Linux)
- Encoding mismatch between writer and reader produces garbled characters (mojibake)
- Cross-platform deployments especially vulnerable to `Encoding.Default` differences

**Answer**

The no-argument `StreamWriter` path constructor defaults to UTF-8 without a BOM; `StreamReader` defaults to UTF-8 with BOM detection. These defaults match on a same-OS round-trip, but `Encoding.Default` introduces a platform-dependent code page: on most Windows machines it is Windows-1252, while on Linux it is UTF-8. Code that writes with `Encoding.Default` on a Windows build server produces Windows-1252 bytes, and a Linux container reading the same file with its default UTF-8 decodes the non-ASCII bytes differently, producing garbled characters — mojibake — that silently corrupt headers, names, and any field with accented characters. The fix is to pin both writer and reader to the same explicit encoding, typically `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)`, and document the encoding in the format contract.

---

## Q6. What is the difference between `Encoding.UTF8` and `new UTF8Encoding(false)`?

**Concepts**
- `Encoding.UTF8` — static property, UTF-8 with BOM emitter
- `new UTF8Encoding(false)` — UTF-8 without BOM
- BOM bytes `EF BB BF` written at the start of the file by `Encoding.UTF8`
- Excel on Windows expects a BOM; most web APIs and Unix tools do not
- BOM breaks downstream `string.StartsWith` checks on the header

**Answer**

`Encoding.UTF8` is a static property that returns a `UTF8Encoding` configured to emit a byte-order mark (`EF BB BF`) at the start of the stream — its full constructor equivalent is `new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)`. The BOM helps Excel on Windows correctly detect the encoding without configuration, but it causes problems for Unix tools, HTTP headers, and downstream parsers that use `string.StartsWith("id,name")` to validate the header row, because the first characters are invisible BOM bytes, not "i". `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` produces identical content without the BOM prefix, which is the standard choice for API outputs, inter-service file exchange, and any consumer that is already configured to expect UTF-8. Choose `Encoding.UTF8` only when the file is destined for a BOM-aware tool like Excel and document that decision in the format contract.

---

## Q7. What is `StreamWriter.AutoFlush`, and what is the performance trade-off vs explicit `Flush()`?

**Concepts**
- Internal character buffer accumulating writes until buffer full or explicit flush
- `AutoFlush = true` issuing a flush after every `Write`/`WriteLine` call
- Performance impact: each write becomes a syscall instead of batched
- Appropriate for log files and status files polled by other processes
- Explicit `Flush()` as a targeted alternative for periodic flushing

**Answer**

`StreamWriter` accumulates writes in an internal character buffer and only pushes content to the underlying stream when the buffer fills, when `Flush()` is called, or when the writer is disposed. Setting `AutoFlush = true` changes this so every `Write` or `WriteLine` flushes the buffer to the underlying stream immediately, guaranteeing that each line is visible to other processes without waiting for buffer capacity or dispose. The cost is that every write becomes at least one syscall rather than being batched, which is noticeable in tight write loops — a logger writing 10,000 lines with `AutoFlush = true` issues 10,000 flushes instead of a handful. The right choice is `AutoFlush = true` for infrequent status tokens or audit lines that must be visible immediately, and explicit periodic `Flush()` for high-throughput writers that flush on a time or count boundary.

---

## Q8. When must you call `Flush()` explicitly, and when is `Dispose` enough?

**Concepts**
- `Dispose` triggers a final flush before releasing the handle
- `Flush()` pushes buffer without closing — for intermediate visibility
- Use `Flush()` when a downstream process polls the file before the writer closes
- Signal file pattern: write token, flush, let reader detect the token

**Answer**

`Dispose` (triggered at the end of a `using` block) calls `Flush()` internally before releasing the file handle, so every write in a complete, exception-free execution path is always persisted. Explicit `Flush()` is needed when the writer remains open and another process, thread, or file watcher must see the written content before the writer closes — for example, writing a `READY` token to a signal file that a subprocess polls before proceeding. Without an explicit `Flush()` call in this scenario, the `READY` token might sit in the internal buffer for an indefinite period. The same applies to shared log files: if a log viewer tails the file while the writer is still active, calling `Flush()` at the end of each logical batch ensures the viewer sees fresh entries without restarting the writer.

---

## Q9. What happens when a `StreamWriter` is not disposed?

**Concepts**
- Internal buffer not flushed — last writes silently lost
- OS file handle held until GC finalizer runs
- Windows file lock preventing concurrent open/delete/rename
- GC finalizer timing is non-deterministic
- `using` or `await using` as the mandatory pattern

**Answer**

Failing to dispose a `StreamWriter` has two consequences: the internal buffer may never be flushed to the underlying stream, so writes that have not yet filled the buffer are silently discarded even on normal process exit, and the underlying `FileStream` handle remains open until the garbage collector runs the finalizer. On Windows, an undisposed `StreamWriter` imposes a file lock — other processes cannot delete, rename, or exclusively open the file, causing `IOException: The process cannot access the file because it is being used by another process`. Because GC timing is non-deterministic, these errors appear intermittently under load and are difficult to reproduce reliably. The fix is always `using var writer = new StreamWriter(...)` or `await using var writer = ...` for async writers, which makes the compiler generate a `try/finally` that calls `Dispose` on every exit path including exceptions.

---

## Q10. What are `StringReader` and `StringWriter`, and when should you use them?

**Concepts**
- `StringWriter` appends to an internal `StringBuilder` — no disk I/O
- `StringReader` reads from an existing `string`
- Both implement `TextReader`/`TextWriter` — usable anywhere those interfaces are accepted
- Unit testing parsers without creating temp files
- Building multi-segment text in memory before a single write to disk

**Answer**

`StringWriter` implements `TextWriter` backed by an internal `StringBuilder`, making it the right tool for assembling a multi-line text document in memory before writing it to disk, a network socket, or a test assertion. `GetStringBuilder()` returns the underlying buffer without a copy. `StringReader` implements `TextReader` backed by a `string`, letting a caller step through an in-memory string using the same `ReadLine`/`ReadToEnd` API that a `StreamReader` would expose for a file. The main use case for `StringReader` is unit testing: a parser that accepts `TextReader` can be fed `new StringReader("header\nrow1\nrow2")` in a test with no disk access, while the same code in production reads from a `StreamReader` over a real file. Both types are lightweight, require no cleanup for resource-holding purposes (though `Dispose` should still be called for consistency), and can be created without any filesystem permissions.

---

## Q11. What is the correct loop pattern for `ReadLine()`, and what is the common mistake with empty-line detection?

**Concepts**
- `ReadLine()` returns `null` at EOF, not an empty string
- `while ((line = reader.ReadLine()) != null)` — correct EOF detection
- `while (line != "")` — breaks at first empty line, not EOF
- Empty lines as valid data in CSV and config files
- Null check is the only EOF-safe termination condition

**Answer**

`ReadLine()` returns `null` when the stream reaches end-of-file, not an empty string. The correct loop is `while ((line = reader.ReadLine()) != null) { }`, which terminates only at EOF and correctly processes empty lines as content. The common mistake is `while ((line = reader.ReadLine()) != "")` or checking `if (line == string.Empty) break`, which breaks at the first blank line — any separator row in a CSV, any blank comment line in a config file, or the standard blank line between sections terminates the loop prematurely, silently discarding all subsequent content. A secondary mistake is `while (!reader.EndOfStream)` without reading: `EndOfStream` only becomes `true` after a read attempt past EOF, and it is only available on `StreamReader`, not on the `TextReader` base class.

---

## Q12. When is `StreamReader.EndOfStream` available, and when should you use `ReadLine() == null` instead?

**Concepts**
- `EndOfStream` is a `StreamReader`-specific property
- Not available on `TextReader` base — compile error if parameter is typed as `TextReader`
- `EndOfStream` becomes `true` only after a read reaches EOF
- `ReadLine() == null` works on any `TextReader` implementation
- `EndOfStream` appropriate only when `StreamReader` is known at the call site

**Answer**

`EndOfStream` is a property on `StreamReader` specifically — it has no counterpart on the `TextReader` abstract class. Code typed as `TextReader` cannot call `EndOfStream` at compile time. Even when the concrete type is `StreamReader`, `EndOfStream` is `true` only after the internal buffer is exhausted and a read has already attempted to fetch more bytes past EOF; calling it before any read on a non-empty file returns `false` correctly, but on an empty file it returns `true` immediately after the buffer is set up — which is reliable but subtly different from checking after each line. The `ReadLine() == null` idiom is more portable, works on any `TextReader` implementation including `StringReader`, and is the conventional pattern used in all documentation examples. Reserve `EndOfStream` for the rare case where you need to peek at EOF state without attempting a read.

---

## Q13. What does `leaveOpen: true` do in `StreamReader`/`StreamWriter` constructors?

**Concepts**
- Default `leaveOpen: false` — Dispose on the wrapper closes the underlying stream
- `leaveOpen: true` — wrapper disposes without closing the base stream
- Necessary when multiple wrappers share one `Stream`
- Necessary when the `Stream` must be read from after the wrapper is disposed
- LIFO disposal order of nested `using` statements

**Answer**

When `StreamReader` or `StreamWriter` is constructed with `leaveOpen: false` (the default), disposing the wrapper also calls `Close()` on the underlying `Stream`, releasing the file handle. This is usually correct but becomes a problem when multiple wrappers share one stream — for example, a `StreamWriter` that writes a header and then a `StreamReader` that reads the same `MemoryStream` back. If the `StreamWriter` disposes with `leaveOpen: false`, the `MemoryStream` is closed and the subsequent `StreamReader` throws `ObjectDisposedException`. Setting `leaveOpen: true` on both wrappers lets them act as views over the shared stream without claiming ownership, and the `Stream` itself must then be disposed explicitly after all wrappers are done. The pattern is common in unit tests that pass a `MemoryStream` through multiple wrappers sequentially.

---

## Q14. How do `ReadLineAsync`, `WriteLineAsync`, and `ReadToEndAsync` free thread-pool threads?

**Concepts**
- Synchronous variants block a thread during I/O wait
- Async variants issue I/O and return the thread to the pool during wait
- `CancellationToken` support on .NET 7+ overloads
- `await using` for async disposal without blocking flush
- Required for ASP.NET Core endpoints and hosted services

**Answer**

`ReadLine()` blocks the calling thread for the entire disk read duration — on a spinning disk or network share that is milliseconds of CPU-idle waiting, which starves the thread pool under concurrent requests. `ReadLineAsync()` issues the underlying I/O to the OS and `await`s completion, releasing the thread back to the thread pool to serve other requests during the wait. On .NET 7+, all async text methods accept a `CancellationToken` so operations can be cancelled cleanly on request shutdown or timeout. `StreamWriter` is `IAsyncDisposable` since .NET 5, so `await using var writer = new StreamWriter(...)` flushes and disposes asynchronously without blocking. In an ASP.NET Core endpoint that processes file content on every request, the difference between sync and async I/O can determine whether the service sustains 10 concurrent requests or 10,000, since each blocked thread occupies a thread-pool slot for the full I/O duration.

---

## Q15. What does `StreamReader.Peek()` do, and when is it useful?

**Concepts**
- Returns next character as `int` without consuming it from the stream
- Returns `-1` at EOF without throwing
- Inherited from `TextReader` — available on any implementation
- Use case: detect encoding signature or section marker without advancing
- Not commonly needed for line-by-line file processing

**Answer**

`Peek()` returns the next character in the stream as an `int` — cast to `char` for comparison — without advancing the read position. Calling `ReadLine()` or `Read()` after `Peek()` will see the same character again. `Peek()` returns `-1` when the stream is at EOF, matching the same convention as `Read()`. Practical uses include sniffing the first character of a line to decide whether it is a comment marker (`#`) without consuming the line, or detecting whether the next line is an XML declaration versus data content. For most line-by-line file processing, `ReadLine()` followed by a null check is clearer and sufficient; `Peek()` adds value only when the decision to read a line depends on what the next character is, and consuming it unconditionally would require pushing it back.

---

## Q16. What is the difference between wrapping a `FileStream` in `StreamWriter` vs using the path constructor?

**Concepts**
- Path constructor: `StreamWriter` creates its own internal `FileStream` with defaults
- Stream constructor: caller controls `FileMode`, `FileAccess`, `FileShare`
- `FileShare` not configurable from the path constructor
- Explicit `FileStream` allows `FileOptions.Asynchronous` for true async I/O
- `leaveOpen` parameter only available in the stream constructor overload

**Answer**

The `StreamWriter(path)` path constructor creates an internal `FileStream` using `FileMode.Create`, `FileAccess.Write`, and `FileShare.Read` with default buffer settings — the caller has no control over these flags. When the writer must coexist with concurrent readers or writers in a specific sharing configuration, the `FileStream` must be constructed explicitly: `new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)` and then wrapped `new StreamWriter(fileStream)`. The explicit `FileStream` constructor also exposes `FileOptions.Asynchronous`, which enables true OS-level async I/O on Windows — without it, `WriteLineAsync` on Windows runs synchronous I/O on a thread-pool thread, negating the async benefit. When the same `FileStream` is shared between a reader and a writer for a patching operation, `leaveOpen: true` on both wrappers prevents either from closing the shared handle.

---

## Q17. (Gotcha) Why does `ReadToEnd()` on a 10 GB customer log cause `OutOfMemoryException`?

**Concepts**
- `ReadToEnd` allocates entire remaining content as one `string`
- Multi-GB LOH allocation exhausts managed heap under concurrent requests
- String internment and LOH pinning compounding the problem
- Tail-read pattern: Seek near EOF and read a bounded chunk instead

**Answer**

`ReadToEnd()` reads every byte from the current stream position to EOF and decodes them into a single `string` — a 10 GB log file produces a 10 GB managed string that must fit contiguously on the Large Object Heap. Under concurrent dashboard requests, each call holds a 10 GB allocation simultaneously, and the CLR cannot compact LOH, so heap exhaustion triggers `OutOfMemoryException` within a few simultaneous requests. The correct pattern for a "show recent lines" view is to open a `FileStream`, seek to a position near EOF with `Seek(-windowBytes, SeekOrigin.End)`, wrap in a `StreamReader`, and call `ReadLine()` in a loop until the desired line count is satisfied — keeping allocation proportional to the viewing window rather than the whole file. For full sequential scanning, replace `ReadToEnd` with `while (await reader.ReadLineAsync(ct) is { } line)` with a cap on maximum lines returned.

---

## Q18. (Gotcha) How does an encoding mismatch between writer and reader cause mojibake silently?

**Concepts**
- Writer encodes `char` → bytes using encoding A
- Reader decodes bytes → `char` using encoding B
- Non-ASCII characters map to different byte sequences per encoding
- No exception thrown — bytes are valid under the wrong encoding, just different characters
- Visible only in locale-specific characters (accents, currency symbols, CJK)

**Answer**

Encoding mismatch is silent because both encoder and decoder complete successfully without errors — the problem is that the byte sequences for non-ASCII characters differ between encodings. A German product name `Büro` written with Windows-1252 produces bytes `42 FC 72 6F`; a UTF-8 reader decodes `FC` as an invalid sequence and substitutes a replacement character `?` or `�`, producing `B?ro` with no exception thrown. For Latin-1 text decoded with Windows-1250 (Czech), accented letters silently map to different accented letters, and the error is not apparent unless someone notices that Prague's "Praha" lost its correct diacritics. The diagnostic approach is to check `reader.CurrentEncoding.WebName` after a read — it will confirm what the reader actually used after BOM detection. The prevention is to explicitly construct both writer and reader with the same `Encoding` instance and turn off `detectEncodingFromByteOrderMarks` when the encoding is contractually guaranteed.

---

## Q19. (Gotcha) Why does `while (line != string.Empty)` terminate early on blank lines?

**Concepts**
- `ReadLine()` returns empty string `""` for a blank line, not `null`
- `null` means EOF; `""` means an empty line in the file
- Early termination skips all content after the first blank line
- Correct check: `while ((line = reader.ReadLine()) != null)`

**Answer**

A blank line in the file — a line containing only a newline character — causes `ReadLine()` to return `""` (an empty string), not `null`. EOF is signalled by `null`. A loop written as `while ((line = reader.ReadLine()) != "")` terminates at the first blank line, silently dropping everything after it. In CSV files this means all rows after the first blank separator are lost; in INI config files it means all sections after the first blank line are silently ignored. The standard null-check loop `while ((line = reader.ReadLine()) != null)` correctly processes empty lines as content and only exits at EOF. If blank lines need to be skipped as a processing concern, add `if (line.Length == 0) continue;` inside the loop body rather than encoding that into the termination condition.

---

## Q20. (Gotcha) What is the performance cost of `AutoFlush = true` in a high-throughput logger?

**Concepts**
- Each `WriteLine` with `AutoFlush = true` issues a syscall
- Without AutoFlush: writes batch into internal buffer, one syscall per ~4 KB
- 10,000 log lines with AutoFlush: ~10,000 syscalls vs ~40 without
- I/O-bound bottleneck in tight loop — writer becomes the slowest component
- Solution: batch flush on a time or count boundary

**Answer**

`AutoFlush = true` makes `StreamWriter` flush its internal character buffer after every `Write` or `WriteLine` call, which means each write translates into at minimum one system call to push bytes to the OS buffer cache. In a tight loop writing 10,000 log entries, `AutoFlush = true` generates approximately 10,000 flush operations versus the roughly 40 that would occur naturally as the 4 KB internal buffer fills. On spinning disks, each flush can be a disk write if the OS write cache is also bypassed; on SSDs and with OS write caching enabled, the cost is the syscall overhead rather than actual disk I/O, but the difference still shows at high throughput. The production pattern for a high-throughput logger is to keep `AutoFlush = false` and flush explicitly on a time boundary (every 500 ms) or a line count boundary (every 100 lines), so most writes are batched but entries are still visible promptly without per-line overhead.

---

## Q21. (Scenario R) A nightly audit job throws on bad rows and the log file stays locked until the worker restarts. What is wrong?

**Defective code:**

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    StreamWriter writer = new StreamWriter(logPath, append: true);  // BUG: no using
    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected.");  // BUG: Dispose never called
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");
    writer.Dispose();  // only reached on happy path
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | `Dispose` only called on happy path — exception skips it | File handle held open until GC; file locked on Windows for indefinite time |
| Data loss | Buffer may not be flushed before the exception escapes | Any partial write buffered in `StreamWriter` is silently discarded |
| Availability | Subsequent appends by other callers throw `IOException: file in use` | Audit log stops recording until the process is restarted |

**Fix priority**

1. Wrap `StreamWriter` in `using` to guarantee `Dispose` on all exit paths.
2. Validate `entry` before constructing the writer so no file handle is opened for a rejected row.
3. Consider flushing explicitly after each write if the file is polled by another process between calls.

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected.");
    using StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");
}
```

---

## Q22. (Scenario R) A CSV export written on Windows with `Encoding.Default` fails header validation on Linux containers. What is the root cause?

**Defective code:**

```csharp
// Writer on Windows build server:
using var writer = new StreamWriter(exportPath, append: false, Encoding.Default); // Windows-1252
writer.WriteLine("id,name,price");

// Reader on Linux container:
using var reader = new StreamReader(importPath, Encoding.Default); // UTF-8 on Linux
string header = reader.ReadLine();
if (header != "id,name,price")                                     // fails silently or with BOM
    throw new FormatException("Invalid CSV header.");
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Platform divergence | `Encoding.Default` is Windows-1252 on Windows, UTF-8 on Linux — different bytes for same text | Garbled non-ASCII fields; header check fails when BOM is written |
| Portability | No explicit encoding documented in the format contract | Behavior changes silently when either side is redeployed |
| Debugging | No exception for the encoding mismatch — silent wrong-character substitution | Root cause is invisible without checking `reader.CurrentEncoding.WebName` at runtime |

**Fix priority**

1. Replace `Encoding.Default` with `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` on both writer and reader.
2. Document the encoding choice in the format specification.
3. Add a cross-platform integration test that writes on one OS and reads on another (or simulates both encodings in a unit test with `StringReader`).

---

## Q23. (Scenario R) A support dashboard calls `ReadToEnd()` on customer logs up to 10 GB — `OutOfMemoryException` under concurrent load. What is the replacement?

**Defective code:**

```csharp
app.MapGet("/logs/{customerId}/tail", async (string customerId, ILogStore store) =>
{
    string path = store.GetLogPath(customerId);
    using var reader = new StreamReader(path);
    string content = reader.ReadToEnd();            // BUG: 10 GB string allocation per request
    var lines = content.Split('\n')
                       .TakeLast(200)
                       .ToArray();
    return Results.Ok(lines);
});
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Memory | `ReadToEnd` allocates the full file as a single LOH string | OOM under concurrent requests; 3 simultaneous calls = 30 GB allocation |
| Threading | No async read — synchronous `ReadToEnd` blocks a thread-pool thread | Thread-pool starvation at moderate concurrent load |
| Inefficiency | Full file read when only last 200 lines are needed | Reads 10 GB to return ~10 KB of data |

**Fix priority**

1. Seek near EOF and read a bounded window: `stream.Seek(-windowBytes, SeekOrigin.End)`, then `ReadLine` up to 200 lines.
2. Replace sync `ReadToEnd` with `await reader.ReadLineAsync(ct)` in a loop with a line count cap.
3. Cache the tail in memory or on a fast store if this endpoint is called frequently.

---

## Q24. (Scenario R) A status file writer sets `IN_PROGRESS` then `COMPLETE`, but operators see `IN_PROGRESS` forever after a crash. What causes it?

**Concepts**
- `StreamWriter` buffer holds content until `Flush()` or `Dispose`
- Process crash before `Dispose` discards buffered content
- `AutoFlush = true` ensuring each status token reaches disk immediately
- `using` statement ensuring `FAILED` is written in `catch`/`finally`

**Answer**

`StreamWriter` buffers writes internally — if the writer is not disposed before the crash, `IN_PROGRESS` may be sitting in the internal buffer and never written to disk, leaving the status file empty. Even if `IN_PROGRESS` is written and flushed, the writer may still be holding `COMPLETE` in the buffer when the crash occurs, so the final state is never recorded. The two-part fix: set `AutoFlush = true` on the status writer so each `WriteLine` is immediately pushed to the OS buffer, and wrap the export logic in a `using` block with a `try/catch` that writes a `FAILED` status in the `catch` branch. For stronger crash safety — surviving a hard power loss — write the terminal status to a temp file and `File.Move` it atomically over the status file, so the reader never sees a truncated or transitional state.

---

## Q25. (Scenario R) A log tailer and log writer in the same app produce intermittent `IOException: sharing violation`. What sharing rule is missing?

**Concepts**
- Tailer opens with default `FileShare` — may exclude writer
- Writer opens with default `FileShare` — excludes other openers
- Both must declare compatible `FileShare` to coexist
- Path constructors on `StreamReader`/`StreamWriter` hide `FileShare` defaults
- Explicit `FileStream` with `FileShare.ReadWrite` required for both

**Answer**

The path-based `StreamReader` constructor creates its own internal `FileStream` with `FileShare.Read`, meaning it allows other readers but excludes any writer — so when the writer tries to open the file, it conflicts with the tailer's exclusive write denial. Similarly, `StreamWriter(path)` defaults to `FileShare.Read`, blocking additional writers from opening the same file. Neither side can see these flags without reading the source. The fix is to construct both `FileStream` instances explicitly with compatible sharing flags: the tailer opens with `FileShare.ReadWrite` so the writer is not blocked, and the writer opens with `FileShare.Read` at minimum. Pass these explicit `FileStream` instances to `new StreamReader(fs)` and `new StreamWriter(fs)`. For highest correctness in an append-only log, both sides should use `FileShare.ReadWrite` and the application should serialize writes with a lock.

---

## Q26. (Scenario P) An ASP.NET Core hosted service calls synchronous `ReadLine()` inside an async loop. Thread-pool queue depth grows under load. What is the problem?

**Concepts**
- Synchronous `ReadLine()` blocking a thread-pool thread during disk I/O
- Thread-pool starvation — pool fills with blocked threads, new work cannot start
- `ReadLineAsync(CancellationToken)` freeing thread during wait
- `FileShare.ReadWrite` needed if the file is written by another process concurrently

**Answer**

`StreamReader.ReadLine()` is synchronous — the calling thread blocks for the entire disk read duration, doing no useful work while waiting for the OS to return data. In an ASP.NET Core hosted service that processes many files concurrently, each concurrent execution holds a thread-pool thread captive for the I/O wait, which is the same class of starvation as calling `.Result` on a `Task`. As the pool fills with blocked threads, new work items queue up and latency spikes even though CPU utilization is low — a classic sign of I/O-bound thread-pool starvation. The fix is `await reader.ReadLineAsync(stoppingToken)` throughout the loop, releasing the thread to serve other work during each disk read. The `CancellationToken` overload available since .NET 7 also enables clean shutdown when the host signals `stoppingToken`. Ensure the `FileStream` is opened with `FileOptions.Asynchronous` for true OS-level async I/O on Windows.

## Gotchas — StreamReader & StreamWriter (Interview Traps)

---

#### Gotcha 1. `StreamWriter` Buffers by Default — Data May Not Reach the File Until Flush

**Concepts**
- `StreamWriter` maintains an internal character buffer that is flushed to the underlying stream periodically
- Forgetting to flush or close a `StreamWriter` can result in partially written or empty files
- The `using` statement disposes the writer, which calls `Flush()` and then `Close()` automatically
- `AutoFlush = true` disables buffering and flushes after every write, at the cost of throughput

**Answer**

`var writer = new StreamWriter("log.txt"); writer.WriteLine("Hello");` does not guarantee the line reaches disk until `writer.Flush()` or `writer.Dispose()` is called. If the program exits or crashes before disposal, the buffered content is lost. Always use a `using` statement or `await using` for async writers to ensure the buffer is flushed on every exit path including exceptions. Use `AutoFlush = true` only when immediate visibility is more important than write throughput, such as for interactive console output or real-time log tailing.

---

#### Gotcha 2. `StreamReader.EndOfStream` Reads Ahead — Can Block on Network Streams

**Concepts**
- `EndOfStream` reads ahead into the buffer to determine whether more data is available
- On a `NetworkStream` or a pipe where data arrives incrementally, checking `EndOfStream` can block waiting for the next chunk
- On a `FileStream`, `EndOfStream` is reliable because file size is known
- For network or pipe streams, check whether bytes were returned from the read operation instead

**Answer**

`while (!reader.EndOfStream) { var line = reader.ReadLine(); … }` works correctly for files where the stream length is known in advance. On a `NetworkStream`, `EndOfStream` blocks until data arrives or the connection closes, because there is no known end position — the property peeks ahead into the buffer. For network or pipe scenarios, check the read result directly: `string? line; while ((line = reader.ReadLine()) != null) { … }` which handles EOF without the blocking peek.

---

#### Gotcha 3. `StreamWriter(path)` Creates or Overwrites — Pass `append: true` to Append

**Concepts**
- `new StreamWriter(path)` truncates an existing file or creates a new one
- `new StreamWriter(path, append: true)` opens an existing file and positions at the end, or creates a new file
- Silently overwriting is a common source of data loss in logging and audit trail code
- Equivalent to `File.OpenWrite` (overwrites) vs `File.AppendText` (appends) in terms of intent

**Answer**

`new StreamWriter("audit.log")` destroys all existing content in `audit.log` without warning. To add entries to an existing log file, pass `append: true`: `new StreamWriter("audit.log", append: true)`. For production logging, prefer a logging framework that manages file rotation and appending, but when using `StreamWriter` directly, always be explicit about whether the intent is to create-or-overwrite or to append.

---

#### Gotcha 4. `StreamReader.ReadToEnd()` Loads the Entire Stream Into a String — Not Suitable for Large Files

**Concepts**
- `ReadToEnd()` reads from the current position to the end of the stream and returns the result as a single string
- For a 1 GB file, this allocates a 1 GB string on the heap
- Should be replaced with line-by-line reading (`ReadLine`) or chunk reading (`ReadBlock`) for large inputs
- Fine for small configuration files, API responses, or in-memory streams with known small content

**Answer**

`reader.ReadToEnd()` is convenient but scales poorly — it allocates a string of the same size as the remaining stream content. For large log files, CSV exports, or binary-as-text data, use `ReadLine()` in a loop to process content incrementally without materialising the whole file. Reserve `ReadToEnd()` for cases where the full content must be manipulated as a string and the size is bounded and small, such as reading a configuration file or a small API payload.

---

#### Gotcha 5. `StreamReader` Default Encoding Is UTF-8 — Specify Encoding for Other Formats

**Concepts**
- `new StreamReader(stream)` defaults to UTF-8 with BOM detection
- ANSI, Latin-1, or custom-encoded streams are silently misread or throw `DecoderFallbackException`
- Always specify the encoding explicitly when the source format is known
- `Encoding.ASCII` is a strict 7-bit encoding; characters above 127 are replaced with `?` by default

**Answer**

`new StreamReader(fileStream)` reads UTF-8 by default. Reading a Windows-1252 ANSI file containing `é` or `ü` produces garbled characters because the byte values for those characters differ between encodings. Use `new StreamReader(fileStream, Encoding.GetEncoding(1252))` for ANSI, or `Encoding.Latin1` as a transparent single-byte encoding that preserves all byte values. When the encoding is unknown, use the BOM-detecting constructor and provide a fallback: `new StreamReader(stream, detectEncodingFromByteOrderMarks: true, leaveOpen: false)`.

---

#### Gotcha 6. Mixing `StreamReader.ReadLine()` and `Stream.Read()` on the Same Stream

**Concepts**
- `StreamReader` maintains its own internal buffer, independent of the underlying `Stream`'s position
- Calling `stream.Read(buffer, 0, n)` after a `StreamReader` has buffered ahead will skip already-buffered bytes
- The stream position and the reader's logical position diverge once the reader has read-ahead data
- Never mix `StreamReader` reads and direct `Stream` reads on the same stream instance

**Answer**

When `StreamReader` reads a chunk of data into its internal buffer, the underlying `Stream.Position` advances by the buffer size, not by the number of characters you have consumed via `ReadLine()`. Calling `stream.Read(...)` after `reader.ReadLine()` skips over the bytes already in the reader's buffer. If you need to read both text (via `StreamReader`) and binary data (via `Stream.Read`) from the same source, read all text first and then switch, or use a single abstraction that handles both.

---

#### Gotcha 7. `AutoFlush = true` on `StreamWriter` — Useful for Logging but Reduces Throughput

**Concepts**
- `AutoFlush = true` causes a flush to the underlying stream after every `Write` or `WriteLine` call
- Eliminates buffering benefits; each call maps to a direct OS write
- Appropriate when real-time visibility matters more than throughput (log tailing, interactive consoles)
- Inappropriate for high-frequency writes such as processing millions of CSV rows — batch with manual `Flush()`

**Answer**

Setting `writer.AutoFlush = true` makes every `writer.WriteLine(...)` call immediately flush to the underlying stream. This is useful when the file is being tailed by a monitoring tool or when the process may be killed and partial log entries must be preserved up to the last line. However, for batch write-heavy code (writing thousands of CSV rows), `AutoFlush = true` can reduce throughput by 10–100x compared to buffered writing with periodic manual flushes. Choose based on the observability vs performance trade-off.

---

#### Gotcha 8. `StreamWriter` Over a `MemoryStream` — Seek to 0 Before Reading Back

**Concepts**
- After writing to a `StreamWriter` backed by a `MemoryStream`, the stream position is at the end
- `memStream.ToArray()` always returns all bytes from position 0 regardless of current position
- `memStream.GetBuffer()` returns the internal buffer including over-allocated unused bytes — not what you want
- Reading the stream back with a `StreamReader` requires `memStream.Seek(0, SeekOrigin.Begin)` first

**Answer**

After `writer.Write("content"); writer.Flush();` the `MemoryStream.Position` is at the end of the written data. Attempting to read back via `new StreamReader(memStream).ReadToEnd()` returns an empty string because there is nothing after the current position. Either call `memStream.Seek(0, SeekOrigin.Begin)` before reading, or use `memStream.ToArray()` which copies from position 0 regardless of the current position. Call `Flush()` on the `StreamWriter` before seeking to ensure all buffered content has been pushed to the `MemoryStream`.

---

#### Gotcha 9. `using` Statement Disposal Order — Inner Reader Closes the Stream

**Concepts**
- `StreamReader` by default disposes (closes) the underlying stream when it is disposed
- Wrapping both `FileStream` and `StreamReader` in `using` blocks results in the stream being closed twice
- The second close is harmless but redundant; passing `leaveOpen: true` to `StreamReader` prevents it from closing the stream
- Use `leaveOpen: true` when you want to read the same stream with multiple readers or continue using it after the reader is done

**Answer**

`using var fs = new FileStream(path, FileMode.Open); using var reader = new StreamReader(fs);` disposes both the `StreamReader` and the `FileStream`. When `reader` is disposed first (inner `using`), it closes `fs`; then when `fs` is disposed by the outer `using`, it is closed again — which is safe because `FileStream.Dispose` checks if already closed. To prevent the reader from closing the stream, use `new StreamReader(fs, Encoding.UTF8, detectBOM: true, bufferSize: 4096, leaveOpen: true)`.

---

#### Gotcha 10. `StreamReader.Peek()` Returns -1 at EOF and Does Not Advance Position

**Concepts**
- `Peek()` returns the next character's integer code without consuming it, or -1 at end of stream
- The position is not advanced; the next `Read()` or `ReadLine()` still returns that same character
- Useful for lookahead parsing to decide which code path to take without consuming the character
- Calling `Peek()` may trigger a buffer fill, which advances the underlying stream position without advancing the reader's logical position

**Answer**

`reader.Peek()` looks at the next character without consuming it, returning -1 if the stream is at end. The character is still available for the next `Read()` or `ReadLine()` call — `Peek()` is truly non-destructive from the caller's perspective. This is useful for peeking at the first character of an XML document to detect the encoding or for disambiguating parser branches. Checking `Peek() == -1` is a reliable EOF check that does not trigger the read-ahead block that `EndOfStream` can cause on network streams.

---
