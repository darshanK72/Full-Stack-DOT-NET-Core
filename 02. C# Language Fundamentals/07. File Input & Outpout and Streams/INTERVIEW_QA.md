# File Input/Output and Streams — Interview Q&A Index

Cross-topic index covering file system operations, stream types, path handling, encoding, and text/binary data processing in .NET 10.

---

## Table of Contents

| # | Topic | File |
|---|-------|------|
| 01 | File & Directory Operations | [INTERVIEW_QA.md](01.%20File%20%26%20Directory%20Operations/INTERVIEW_QA.md) |
| 02 | StreamReader & StreamWriter | [INTERVIEW_QA.md](02.%20StreamReader%20%26%20StreamWriter/INTERVIEW_QA.md) |
| 03 | FileStream & Binary Files | [INTERVIEW_QA.md](03.%20FileStream%20%26%20Binary%20Files/INTERVIEW_QA.md) |
| 04 | Path & Environment Classes | [INTERVIEW_QA.md](04.%20Path%20%26%20Environment%20Classes/INTERVIEW_QA.md) |
| 05 | Working with CSV and Text Files | [INTERVIEW_QA.md](05.%20Working%20with%20CSV%20and%20Text%20Files/INTERVIEW_QA.md) |

---

## Cross-Cutting Questions

---

## CQ1. When should you use `FileStream` directly versus wrapping it in a `StreamReader` or `StreamWriter`?

**Concepts**
- `FileStream` — raw byte-level access
- `StreamReader`/`StreamWriter` — text-oriented wrappers over any stream
- seek and random access requirements
- encoding abstraction layer
- performance: buffering differences
- binary vs. text data distinction

**Answer**

`FileStream` is the lowest-level .NET abstraction over an OS file handle. It operates on raw bytes and gives you full control over seek position, file share modes, and buffer size — making it the right choice whenever you are working with binary data (images, PDFs, serialized formats, custom protocols) or need to read at arbitrary byte offsets within a large file. Because it exposes `ReadAsync` and `WriteAsync` at the byte level, it also pairs naturally with `BinaryReader` and `BinaryWriter` for structured binary formats.

`StreamReader` and `StreamWriter` sit one layer above `FileStream`. They own encoding/decoding logic and expose line-oriented APIs (`ReadLine`, `ReadToEnd`, `WriteLine`) that are meaningless for binary content but enormously convenient for text. In practice you almost always create them with a `FileStream` underneath — either by passing the path (which implicitly creates a `FileStream`) or by wrapping one you already have, which lets you share or reuse the underlying handle.

The decision rule is: if the data is textual and you process it line by line or as whole strings, use `StreamReader`/`StreamWriter`. If the data is binary, if you need byte-accurate seeks, or if you are building a higher-level reader on top (like a `BinaryReader`), work directly with `FileStream`. For CSV or delimiter-separated text at large scale, a `StreamReader` with a `while (reader.ReadLine() != null)` loop is preferred over loading the full file with `File.ReadAllText`, because it streams content without holding everything in memory simultaneously.

---

## CQ2. How do `Path` and `Environment` classes underpin correct `File` and `Directory` operations across platforms and deployment environments?

**Concepts**
- `Path.Combine` — OS-aware path construction
- `Environment.GetFolderPath` — special folder resolution
- `Environment.CurrentDirectory` vs. `AppContext.BaseDirectory`
- path separator differences (Windows vs. Linux containers)
- `Path.GetTempPath`, `Path.GetFullPath` — normalization
- relative-to-absolute path pitfalls in web vs. console hosts

**Answer**

Hardcoded path strings are one of the most common sources of cross-environment bugs in .NET applications. `Path.Combine` solves the separator problem: it inserts `\` on Windows and `/` on Linux automatically, and it correctly handles trailing-slash edge cases that simple string concatenation misses. Paired with `Path.GetFullPath`, you can normalize a path built from relative segments into a canonical absolute path that works regardless of the calling process's working directory.

`Environment.GetFolderPath(Environment.SpecialFolder.*)` is the correct way to locate well-known OS directories (Desktop, LocalApplicationData, CommonApplicationData) rather than assuming `C:\Users\...` exists. This matters even on Windows when running in a container or as a service account with a non-standard profile.

In ASP.NET Core and hosted services `Environment.CurrentDirectory` may not point where you expect, because the OS sets it to the process launch directory which can differ between `dotnet run`, IIS hosting, and Docker. `AppContext.BaseDirectory` is more reliable for locating files deployed beside the binary. When your code consumes user-supplied paths, always validate with `Path.GetFullPath` and check that the resolved path starts within the intended root directory — this prevents path traversal attacks where `../../etc/passwd`-style inputs escape your intended directory scope.

---

## CQ3. How do encoding decisions ripple across `StreamReader`, `StreamWriter`, `File` helper methods, and CSV/text processing?

**Concepts**
- UTF-8 as .NET 10 default (no BOM)
- BOM detection and `detectEncodingFromByteOrderMarks` parameter
- `Encoding.UTF8` vs. `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)`
- legacy encodings via `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)`
- `StreamWriter` flushing and `AutoFlush`
- encoding mismatch symptoms: mojibake, truncated characters

**Answer**

Encoding is the silent agreement between writer and reader about how bytes map to characters. In .NET 10 the no-argument `StreamReader` and `StreamWriter` constructors default to UTF-8 without a BOM, which is the correct choice for new files and APIs. When reading files produced by external systems — legacy databases, Excel CSV exports, SAP outputs — you must explicitly specify the encoding, because Windows-1252 and ISO-8859-1 are still common in enterprise integrations. Passing the wrong encoding produces mojibake: characters with code points above 127 become garbage.

`StreamReader` has a `detectEncodingFromByteOrderMarks: true` overload that sniffs the first bytes of the file to detect UTF-8, UTF-16 LE, and UTF-16 BE BOMs automatically. This is useful for files of unknown origin but should never be the default in code that produces files — emit a BOM only when downstream consumers require it (notably older Excel versions for CSV).

For `StreamWriter`, `AutoFlush = false` (the default) buffers writes for performance, but you must either call `Flush()` explicitly or rely on `Dispose()` to flush the remaining buffer. A `using` block guarantees `Dispose` is called and therefore that the last write is never silently lost — making the `using` pattern not just a memory hygiene choice but a correctness requirement for file writing. When processing CSV or delimiter-separated files, encoding decisions compound with newline differences (`\r\n` vs. `\n`) and must be tested against both Windows-produced and Unix-produced input files.

---

## CQ4. How does the `IDisposable` + `using` pattern apply consistently across every stream and file type, and what happens if you skip it?

**Concepts**
- `IDisposable.Dispose` — deterministic resource release
- `using` declaration vs. `using` statement (C# 8+)
- OS file handle leak
- `StreamReader` disposing underlying `FileStream` by default
- `leaveOpen: true` parameter for shared stream ownership
- `await using` for `IAsyncDisposable` streams

**Answer**

Every type in the `System.IO` namespace that wraps an OS resource — `FileStream`, `StreamReader`, `StreamWriter`, `BinaryReader`, `BinaryWriter` — implements `IDisposable`. The OS file handle is a finite, process-global resource. If you neglect `Dispose`, the handle stays open until the GC finalizes the object, which is non-deterministic and may never happen in a long-running service. Symptoms include `IOException: The process cannot access the file because it is being used by another process`, handle exhaustion under load, and files that appear empty because the writer's buffer was never flushed.

The modern C# 8 `using` declaration (`using var reader = new StreamReader(path);`) scopes disposal to the enclosing block without the extra indentation of the traditional `using` statement — both compile to identical try/finally IL. For async I/O in .NET 10, use `await using` with types that implement `IAsyncDisposable` such as `StreamWriter`, allowing the final async flush to complete before the handle is released.

A subtle ownership issue arises when you stack wrappers: by default, disposing a `StreamReader` also disposes the underlying `FileStream`. If you need to keep the `FileStream` alive after the reader is done — for example, to hand it to a second reader or to seek back to the start — pass `leaveOpen: true` to the `StreamReader` constructor and manage the `FileStream` lifetime explicitly with its own `using` block. Getting ownership semantics right prevents both handle leaks and use-after-dispose exceptions.
