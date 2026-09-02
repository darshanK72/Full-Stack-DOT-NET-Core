# 01. File & Directory Operations — Interview Q&A


## Table of Contents

1. [Q1. When should you choose `File`/`Directory` (static) over `FileInfo`/`DirectoryInfo` (instance) classes?](#q1-when-should-you-choose-filedirectory-static-over-fileinfodirectoryinfo-instance-classes)
2. [Q2. What does `Directory.CreateDirectory` do when intermediate folders already exist?](#q2-what-does-directorycreatedirectory-do-when-intermediate-folders-already-exist)
3. [Q3. What is the memory difference between `Directory.GetFiles` and `Directory.EnumerateFiles`?](#q3-what-is-the-memory-difference-between-directorygetfiles-and-directoryenumeratefiles)
4. [Q4. How do `File.ReadAllLines`, `File.ReadAllText`, and `File.ReadLines` differ for large files?](#q4-how-do-filereadalllines-filereadalltext-and-filereadlines-differ-for-large-files)
5. [Q5. What is the difference between `File.Copy` with `overwrite: false` vs `overwrite: true`?](#q5-what-is-the-difference-between-filecopy-with-overwrite-false-vs-overwrite-true)
6. [Q6. How does `File.Move` differ from a copy-then-delete sequence?](#q6-how-does-filemove-differ-from-a-copy-then-delete-sequence)
7. [Q7. What happens on Windows when you call `File.Delete` on a file with open handles?](#q7-what-happens-on-windows-when-you-call-filedelete-on-a-file-with-open-handles)
8. [Q8. How do `FileAttributes` flags work, and how do you set and clear individual flags?](#q8-how-do-fileattributes-flags-work-and-how-do-you-set-and-clear-individual-flags)
9. [Q9. Explain `File.GetCreationTime`, `GetLastWriteTime`, and `GetLastAccessTime` — why prefer UTC variants?](#q9-explain-filegetcreationtime-getlastwritetime-and-getlastaccesstime-why-prefer-utc-variants)
10. [Q10. Why is `File.Exists` insufficient as a guard before open or delete? What is the TOCTOU risk?](#q10-why-is-fileexists-insufficient-as-a-guard-before-open-or-delete-what-is-the-toctou-risk)
11. [Q11. What is the difference between `Directory.Delete(path)` and `Directory.Delete(path, recursive: true)`?](#q11-what-is-the-difference-between-directorydeletepath-and-directorydeletepath-recursive-true)
12. [Q12. What metadata does `FileInfo` cache, and when must you call `Refresh()`?](#q12-what-metadata-does-fileinfo-cache-and-when-must-you-call-refresh)
13. [Q13. What do `File.Create`, `File.OpenRead`, `File.OpenWrite`, and `File.Open` imply about mode and access?](#q13-what-do-filecreate-fileopenread-fileopenwrite-and-fileopen-imply-about-mode-and-access)
14. [Q14. What is the difference between `SearchOption.TopDirectoryOnly` and `SearchOption.AllDirectories`?](#q14-what-is-the-difference-between-searchoptiontopdirectoryonly-and-searchoptionalldirectories)
15. [Q15. How does `File.AppendAllText` differ from holding a `FileStream` open with `FileMode.Append`?](#q15-how-does-fileappendalltext-differ-from-holding-a-filestream-open-with-filemodeappend)
16. [Q16. What exceptions should you handle for typical file operations?](#q16-what-exceptions-should-you-handle-for-typical-file-operations)
17. [Q17. When is `File.ReadAllBytes` / `File.WriteAllBytes` appropriate vs a `FileStream`?](#q17-when-is-filereadallbytes-filewriteallbytes-appropriate-vs-a-filestream)
18. [Q18. (Gotcha) Why does `File.ReadAllLines` on a 2 GB log file throw `OutOfMemoryException`?](#q18-gotcha-why-does-filereadalllines-on-a-2-gb-log-file-throw-outofmemoryexception)
19. [Q19. (Gotcha) Why does `Directory.Delete(path)` throw even after deleting all visible files?](#q19-gotcha-why-does-directorydeletepath-throw-even-after-deleting-all-visible-files)
20. [Q20. (Gotcha) What does `FileInfo` metadata staleness mean in practice?](#q20-gotcha-what-does-fileinfo-metadata-staleness-mean-in-practice)
21. [Q21. (Gotcha) Why does `File.OpenWrite` leave stale tail bytes when used to update file content?](#q21-gotcha-why-does-fileopenwrite-leave-stale-tail-bytes-when-used-to-update-file-content)
22. [Q22. (Gotcha) Why can relative paths produce `FileNotFoundException` in unexpected locations?](#q22-gotcha-why-can-relative-paths-produce-filenotfoundexception-in-unexpected-locations)
23. [Q23. (Scenario R) A report export service uses `File.Exists` + `File.Create` to guard concurrent writes — two threads occasionally get `IOException` or silently skip writing. What is wrong and how do you fix it?](#q23-scenario-r-a-report-export-service-uses-fileexists-filecreate-to-guard-concurrent-writes-two-threads-occasionally-get-ioexception-or-silently-skip-writing-what-is-wrong-and-how-do-you-fix-it)
24. [Q24. (Scenario R) An upload service stages files in `Path.GetTempPath()` then calls `File.Move` to a different volume. It leaks temp files on exception and fails on Linux containers. What is wrong?](#q24-scenario-r-an-upload-service-stages-files-in-pathgettemppath-then-calls-filemove-to-a-different-volume-it-leaks-temp-files-on-exception-and-fails-on-linux-containers-what-is-wrong)
25. [Q25. (Scenario R) A cleanup job calls `Directory.Delete(root, recursive: false)` after a file-loop — throws in production on non-empty directories. What is wrong?](#q25-scenario-r-a-cleanup-job-calls-directorydeleteroot-recursive-false-after-a-file-loop-throws-in-production-on-non-empty-directories-what-is-wrong)
26. [Q26. (Scenario M) An ASP.NET Core endpoint reads a 200 MB CSV with `File.ReadAllText` on every request — thread-pool starvation and LOH pressure under load. What is wrong?](#q26-scenario-m-an-aspnet-core-endpoint-reads-a-200-mb-csv-with-filereadalltext-on-every-request-thread-pool-starvation-and-loh-pressure-under-load-what-is-wrong)
27. [Q27. (Scenario D) A containerized API creates per-request scratch directories under `Path.GetTempPath()` but never cleans them up on exception. Compare three strategies.](#q27-scenario-d-a-containerized-api-creates-per-request-scratch-directories-under-pathgettemppath-but-never-cleans-them-up-on-exception-compare-three-strategies)

---
> Back to [Module Index](../INTERVIEW_QA.md)

---

## Q1. When should you choose `File`/`Directory` (static) over `FileInfo`/`DirectoryInfo` (instance) classes?

**Concepts**
- Static classes: no state, one security check per call
- Instance classes: security check amortized at construction
- Metadata caching on `FileInfo`/`DirectoryInfo` until `Refresh()`
- Instance methods mirror static equivalents (`CopyTo`, `MoveTo`, `Delete`)
- Appropriate choice based on number of operations on the same path

**Answer**

The static `File` and `Directory` classes perform one security check and one OS round-trip per method call, making them ideal for single, isolated operations such as `File.Delete(path)` or `File.ReadAllText(path)`. The instance classes `FileInfo` and `DirectoryInfo` amortize the security demand at construction and cache metadata properties — `Length`, `LastWriteTimeUtc`, `Attributes` — so reading several properties for the same path triggers only one kernel round-trip rather than one per property. The rule of thumb: use static methods when you perform one operation per path, and use instance wrappers when you read multiple metadata values or chain several operations (exists check, copy, then delete) on the same path within the same scope. Calling `new FileInfo(path)` just to call a single method on it adds construction overhead with no benefit.

---

## Q2. What does `Directory.CreateDirectory` do when intermediate folders already exist?

**Concepts**
- Idempotent directory creation
- Full path chain created in one call
- No exception on pre-existing segments
- TOCTOU-safe alternative to check-then-create pattern
- Returns `DirectoryInfo` for the deepest created folder

**Answer**

`Directory.CreateDirectory` creates the entire path chain — including any missing intermediate segments — and does not throw if some or all of the segments already exist. This makes it safe to call unconditionally before writing a file, eliminating the `if (!Directory.Exists(path)) Directory.CreateDirectory(path)` pattern that introduces a TOCTOU race: another process could create the directory between the check and the create, causing the second call to fail. The only exceptions it throws are for invalid characters in the path, paths exceeding the OS length limit, or a path that resolves to an existing file rather than a directory. It returns a `DirectoryInfo` pointing at the leaf folder, which can be used immediately without a second lookup.

---

## Q3. What is the memory difference between `Directory.GetFiles` and `Directory.EnumerateFiles`?

**Concepts**
- `GetFiles` eager materialization into `string[]`
- `EnumerateFiles` lazy `IEnumerable<string>` backed by iterator
- Allocation proportional to directory size for `Get*` variants
- Early termination efficiency with lazy enumeration
- Kernel handle held open during lazy enumeration

**Answer**

`Directory.GetFiles` scans the entire directory and returns a fully allocated `string[]` before the caller sees the first entry; for a directory with 200,000 files, that is a 200,000-element array on the heap. `Directory.EnumerateFiles` returns an `IEnumerable<string>` backed by a lazy iterator that yields one path at a time, keeping memory proportional to one string regardless of how many files exist. This matters for LINQ operations like `.FirstOrDefault(f => f.EndsWith(".log"))` — with `EnumerateFiles` the scan stops at the first match, while `GetFiles` materializes everything first. The trade-off is that the OS directory handle stays open for the duration of the enumeration, so enumerators should be consumed promptly and not stored long-term.

---

## Q4. How do `File.ReadAllLines`, `File.ReadAllText`, and `File.ReadLines` differ for large files?

**Concepts**
- `ReadAllText` — entire file as one `string`, eager LOH allocation
- `ReadAllLines` — entire file as `string[]`, eager allocation
- `ReadLines` — lazy `IEnumerable<string>`, file handle open during iteration
- OutOfMemoryException risk for multi-gigabyte files with eager variants
- Use `StreamReader` for maximum control over chunk size

**Answer**

`File.ReadAllText` loads the entire file into a single `string` before returning — on a 500 MB file that is a 500 MB LOH allocation that triggers a Gen2 GC collection. `File.ReadAllLines` does the same but splits on line endings and allocates both the individual line strings and the array holding them. `File.ReadLines` is lazy — it returns an `IEnumerable<string>` that reads one line at a time and keeps memory proportional to the longest single line, which is the correct choice for large files. The catch is that `ReadLines` holds the underlying file handle open until the enumerator is disposed, so breaking a `foreach` early or calling LINQ terminal operations disposes the enumerator and releases the handle cleanly. For very large files where even `ReadLines` overhead matters, a `StreamReader` in a manual `while` loop with a reused buffer is the most efficient approach.

---

## Q5. What is the difference between `File.Copy` with `overwrite: false` vs `overwrite: true`?

**Concepts**
- `overwrite: false` (default) throws `IOException` on destination collision
- `overwrite: true` replaces destination file atomically on NTFS
- `IOException` as the collision exception type
- Metadata (creation time) may differ between source and copy
- Not a fully atomic operation under all concurrent access patterns

**Answer**

With `overwrite: false`, `File.Copy` throws `System.IO.IOException` if the destination path already exists, which is the safe default that prevents silent data loss when called carelessly. With `overwrite: true`, the existing destination is replaced — on Windows NTFS, `CopyFileEx` handles the replacement as a single kernel operation, making it resistant to partial-file visibility, though it is not fully atomic under all concurrent write scenarios. File content is always copied, but metadata such as creation time is set to the time of the copy rather than the source's creation time; callers that require timestamp preservation must call `File.SetCreationTimeUtc` after the copy. When the destination must be updated safely with no moment of absence for concurrent readers, `File.Replace` is preferable because it preserves the destination's inode and ACLs.

---

## Q6. How does `File.Move` differ from a copy-then-delete sequence?

**Concepts**
- Atomic rename within the same volume (no bytes moved)
- Cross-volume fallback to copy-then-delete (non-atomic)
- Metadata and hard-link preservation on same-volume rename
- `overwrite: true` overload available since .NET 5
- `IOException` on cross-volume atomic-rename failure

**Answer**

When source and destination share the same filesystem volume, `File.Move` is a single atomic rename system call — the directory entry is updated with no bytes copied on disk, completing in constant time regardless of file size, and all metadata (creation time, last-write time, attributes, hard links) is fully preserved. When source and destination span different volumes or network shares, .NET falls back to copy-then-delete, which is not atomic: a crash after the copy but before the delete leaves both copies on disk. For same-volume moves where the destination may already exist, passing `overwrite: true` (added in .NET 5) combines the replace and delete into one call, closing the window where a separate `File.Delete` could fail. Cross-volume atomic replacement is not achievable with `File.Move`; stage the file on the destination volume and then rename within it.

---

## Q7. What happens on Windows when you call `File.Delete` on a file with open handles?

**Concepts**
- Windows deferred delete — file marked for deletion when last handle closes
- File path still accessible to handle holders after delete call
- `IOException` thrown by other openers after the deferred delete mark
- `FileShare.Delete` required for a concurrent opener to allow the deletion
- Linux unlink semantics differ — inode remains accessible until handle count reaches zero

**Answer**

On Windows, `File.Delete` succeeds (no exception) even when another process or thread has the file open, provided all open handles were created with `FileShare.Delete`. The OS marks the file for deferred deletion: the path disappears from directory listings immediately, but the underlying data remains accessible to the handle holders until the last handle closes. Any opener that did not include `FileShare.Delete` causes `File.Delete` to throw `IOException: The process cannot access the file because it is being used by another process`. On Linux the behavior is different — `unlink` immediately removes the directory entry regardless of open handles, and the data persists until the last file descriptor referencing the inode is closed. This means cross-platform file deletion code must not rely on either platform's specific behavior when handles may be concurrently open.

---

## Q8. How do `FileAttributes` flags work, and how do you set and clear individual flags?

**Concepts**
- `FileAttributes` as a combinable `[Flags]` enum
- `File.GetAttributes(path)` returns the combined flag set
- `File.SetAttributes(path, value)` replaces the entire attribute set
- Bitwise OR to add a flag, AND-NOT to remove a flag
- `ReadOnly`, `Hidden`, `Archive`, `System` as the common flags

**Answer**

`FileAttributes` is a `[Flags]` enum whose members — `ReadOnly`, `Hidden`, `System`, `Archive`, `Directory`, `Compressed`, `Encrypted` — can be combined bitwise. `File.GetAttributes(path)` returns the full combined value, and `File.SetAttributes(path, newValue)` replaces the complete set, so you must read the current value first or risk clearing unrelated flags. To add a flag: `attrs | FileAttributes.Hidden`. To remove a flag: `attrs & ~FileAttributes.ReadOnly`. Setting attributes requires write permission on the file's metadata — restricted service accounts can receive `UnauthorizedAccessException` on protected system files. On Linux, `ReadOnly` maps to write-permission bits and most Windows-specific flags are silently ignored, so attribute code must be tested on the target platform.

---

## Q9. Explain `File.GetCreationTime`, `GetLastWriteTime`, and `GetLastAccessTime` — why prefer UTC variants?

**Concepts**
- Three distinct filesystem timestamps (created, last written, last accessed)
- Local-time variants subject to DST ambiguity
- UTC variants avoid DST double-map on clock fallback
- NTFS stores UTC internally; local variants involve conversion on read
- `GetLastAccessTime` unreliable on performance-tuned systems

**Answer**

`GetCreationTime` returns when the directory entry was created, `GetLastWriteTime` when content was last modified, and `GetLastAccessTime` when the file was last opened for reading. The non-suffixed variants return local `DateTime` values, which are ambiguous during DST transitions when two different UTC instants map to the same local representation during the hour clocks fall back. The `Utc` variants (`GetCreationTimeUtc`, etc.) return UTC and avoid that ambiguity entirely — use them for logging, comparisons, and storage. On Windows, NTFS stores timestamps as UTC at 100-nanosecond precision and converts on read, so UTC variants also skip one conversion step. `GetLastAccessTime` is frequently disabled on performance-tuned NTFS volumes and on Linux ext4 with `noatime`, making it an unreliable proxy for recent reads.

---

## Q10. Why is `File.Exists` insufficient as a guard before open or delete? What is the TOCTOU risk?

**Concepts**
- TOCTOU race between check and use
- `File.Exists` as a point-in-time snapshot
- `FileMode.CreateNew` for atomic exclusive-create
- `FileMode.Open` + `FileNotFoundException` for atomic open-if-exists
- `File.Exists` appropriate only for user-facing pre-validation

**Answer**

`File.Exists` is a point-in-time snapshot — another process can delete or create the file in the window between the check returning `true` and the subsequent open or delete call. This TOCTOU race causes intermittent failures under concurrent load and is not detectable by inspecting any single execution in isolation. The race-free alternative encodes intent in `FileMode`: use `FileMode.CreateNew` to atomically fail if the path already exists (throwing `IOException` which you catch as the "already exists" case), and use `FileMode.Open` to atomically fail if the path does not exist (catching `FileNotFoundException` as the "missing" case). `File.Exists` is appropriate for user-facing validation messages where a subsequent race is acceptable, such as showing "file not found" in a UI before a user re-enters a path, but must never be the sole guard before a security-sensitive or correctness-critical operation.

---

## Q11. What is the difference between `Directory.Delete(path)` and `Directory.Delete(path, recursive: true)`?

**Concepts**
- Non-recursive delete requires empty directory — throws `IOException` if any content exists
- `recursive: true` removes all files and subdirectories in one call
- Open handles inside the tree block file deletion on Windows
- Read-only files need attribute clearing before recursive delete
- `UnauthorizedAccessException` on hidden/system files under recursive delete

**Answer**

`Directory.Delete(path)` without the `recursive` flag throws `IOException: The directory is not empty` if any file or subdirectory exists inside the path, even a single hidden file. This catches many developers off guard when the directory was expected to be empty but contains OS-created metadata files like `Thumbs.db`. Passing `recursive: true` removes all files and subdirectories in one call, which is faster and simpler than a manual loop. Before calling recursive delete, all `FileStream`, `StreamReader`, and `StreamWriter` instances opened against paths inside the tree must be disposed because Windows defers deletion of open files, which then blocks the containing directory's removal. Files marked `ReadOnly` throw `UnauthorizedAccessException` under recursive delete; clear them with `File.SetAttributes(path, FileAttributes.Normal)` first.

---

## Q12. What metadata does `FileInfo` cache, and when must you call `Refresh()`?

**Concepts**
- `FileInfo` caches `Length`, timestamps, `Attributes`, `Exists` at construction or last refresh
- External changes not reflected until `Refresh()` is called
- `FileInfo(path)` does not throw if path does not exist
- `Refresh()` re-issues the kernel metadata query
- `Length` throws `FileNotFoundException` via `Refresh()` if file deleted after construction

**Answer**

`FileInfo` populates its properties — `Length`, `CreationTimeUtc`, `LastWriteTimeUtc`, `LastAccessTimeUtc`, `Attributes`, `Exists` — lazily on first access and then caches the results until `Refresh()` is explicitly called. This means if another process writes to the file and changes its `Length` after your `FileInfo` was constructed, reading `Length` again without calling `Refresh()` returns the stale cached value. Constructing `new FileInfo(path)` does not throw even if the file does not exist; `Exists` simply returns `false` and `Length` throws `FileNotFoundException` on access. Call `Refresh()` whenever you need a fresh metadata snapshot after an external operation, such as verifying a file's size after a background job finished writing to it.

---

## Q13. What do `File.Create`, `File.OpenRead`, `File.OpenWrite`, and `File.Open` imply about mode and access?

**Concepts**
- `File.Create` — `FileMode.Create` + `FileAccess.ReadWrite`, truncates existing content
- `File.OpenRead` — `FileMode.Open` + `FileAccess.Read`, throws if path missing
- `File.OpenWrite` — `FileMode.OpenOrCreate` + `FileAccess.Write`, does NOT truncate
- `File.Open` — explicit `FileMode`, `FileAccess`, optional `FileShare` for full control
- All return `FileStream` requiring `using` for handle release

**Answer**

`File.Create(path)` opens with `FileMode.Create` and `FileAccess.ReadWrite` — it creates the file if absent or truncates it to zero bytes if present, which surprises callers expecting to append to an existing file. `File.OpenRead(path)` is shorthand for `FileMode.Open` with `FileAccess.Read` and `FileShare.Read`; it throws `FileNotFoundException` if the path does not exist. `File.OpenWrite(path)` opens with `FileMode.OpenOrCreate` and `FileAccess.Write` and positions at byte zero without truncating — content beyond what you write remains on disk, a subtle trap when replacing shorter content. `File.Open(path, mode, access, share)` is the general-purpose overload that should be used whenever the implicit defaults of the convenience methods do not match the caller's intent, particularly when `FileShare` must be set explicitly for concurrent access.

---

## Q14. What is the difference between `SearchOption.TopDirectoryOnly` and `SearchOption.AllDirectories`?

**Concepts**
- `TopDirectoryOnly` — immediate children only, no recursion
- `AllDirectories` — recursive scan of all nested subdirectories
- `AllDirectories` traverses symbolic links on some platforms
- Use with `EnumerateFiles` for lazy deep traversal
- `SearchOption` applies to both `Get*` and `Enumerate*` overloads

**Answer**

`SearchOption.TopDirectoryOnly` restricts enumeration to immediate children of the specified directory, returning no files or folders nested deeper. `SearchOption.AllDirectories` recursively descends every subdirectory and returns all matching entries at any depth. For directory trees with thousands of nested files, pairing `AllDirectories` with `EnumerateFiles` — the lazy variant — keeps memory bounded because entries stream one at a time rather than being loaded into a complete array. On Linux and macOS, recursive enumeration with `AllDirectories` follows symbolic links, which can lead to infinite loops if a symlink creates a directory cycle; Windows does not follow junction point loops in most cases. Prefer `AllDirectories` with `EnumerateFiles` over `GetFiles` for large or unknown-depth trees.

---

## Q15. How does `File.AppendAllText` differ from holding a `FileStream` open with `FileMode.Append`?

**Concepts**
- `AppendAllText` — open, append, close in one call (no lingering handle)
- `FileMode.Append` on held `FileStream` — efficient for tight append loop
- Per-call overhead of open/close vs amortized handle for frequent appends
- `FileShare` defaults differ between convenience method and explicit constructor
- Thread safety without a held handle vs explicit synchronization with held handle

**Answer**

`File.AppendAllText` is a one-shot operation: it opens the file, writes the content, and immediately closes the handle — no lock persists between calls, so each call is independent and other processes can open the file freely in between. Opening a `FileStream` with `FileMode.Append` and keeping it open is more efficient for a tight loop that appends many entries because the OS seek-to-EOF overhead is paid once at open time rather than on every call, and the handle avoids repeated open/close system calls. The open handle carries a lock governed by the `FileShare` flag in the `FileStream` constructor; `AppendAllText` chooses its own share flags internally and closes before returning. For sporadic one-off appends where no handle should linger, `AppendAllText` is simpler; for a long-running log writer, a held handle with explicit `FileShare` control is more efficient.

---

## Q16. What exceptions should you handle for typical file operations?

**Concepts**
- `FileNotFoundException` — path missing when `FileMode.Open` used
- `DirectoryNotFoundException` — intermediate directory segment missing
- `IOException` — base for disk-full, sharing violation, locked file errors
- `UnauthorizedAccessException` — insufficient permissions
- `PathTooLongException` (surfaced as `IOException` on .NET 5+) — path exceeds OS limit

**Answer**

`FileNotFoundException` is the exception for attempting to open a non-existent file with `FileMode.Open`, and `DirectoryNotFoundException` fires when an intermediate directory segment of the path does not exist — both derive from `IOException`. `IOException` itself is the broad base for disk-full errors, sharing violations (`The process cannot access the file`), and locked-file errors, all of which have the same base type even though their causes differ. `UnauthorizedAccessException` signals a permissions problem: writing to a read-only file, trying to open a path the account lacks rights to, or attempting to open a directory as a regular file. The correct handler structure catches the most specific exception first with a precise user message and falls back to `IOException` as a general handler, never catching root `Exception` and swallowing I/O failures silently.

---

## Q17. When is `File.ReadAllBytes` / `File.WriteAllBytes` appropriate vs a `FileStream`?

**Concepts**
- `ReadAllBytes` allocates the entire file as `byte[]` on the heap
- LOH allocation risk for files larger than 85 KB
- Simplicity for small, bounded binary payloads
- Stream-based chunked reads for large files
- No `async` support on `ReadAllBytes`/`WriteAllBytes`

**Answer**

`File.ReadAllBytes` is appropriate when the file is small (ideally under a few megabytes), processing always requires the full buffer at once, and simplicity matters more than memory efficiency. For files larger than about 85 KB, the `byte[]` is allocated on the Large Object Heap and increases GC pressure; a 200 MB file produces a 200 MB LOH object that can cause `OutOfMemoryException` under concurrent load. Stream-based APIs read in chunks that stay in the OS page cache rather than materializing the full content in managed memory, and they support `ReadAsync`/`WriteAsync` natively, which frees thread-pool threads during the I/O wait. For large writes that must not leave a partial file on crash, stream to a temp path and `File.Move` into place after the stream is closed and flushed.

---

## Q18. (Gotcha) Why does `File.ReadAllLines` on a 2 GB log file throw `OutOfMemoryException`?

**Concepts**
- Eager materialization of entire file into `string[]`
- Combined allocation: individual line strings plus the container array
- Large Object Heap exhaustion under concurrent requests
- `File.ReadLines` or `StreamReader.ReadLine` loop as bounded alternatives

**Answer**

`File.ReadAllLines` reads the entire file, splits on line endings, and returns every line as an element of a `string[]`. For a 2 GB log, this allocates the full file content into memory before returning the first line — not just one array but potentially millions of individual `string` allocations plus the `string[]` holding them. When multiple requests call this concurrently, each holds a 2 GB allocation simultaneously, exhausting the available managed heap and triggering `OutOfMemoryException`. The fix is `File.ReadLines(path)` for simple LINQ scenarios, which yields one line at a time, or a `StreamReader` in a `while ((line = reader.ReadLine()) != null)` loop for maximum control. Both patterns keep memory proportional to the longest single line regardless of file size.

---

## Q19. (Gotcha) Why does `Directory.Delete(path)` throw even after deleting all visible files?

**Concepts**
- Hidden OS files (`Thumbs.db`, `.DS_Store`) that `GetFiles` without `SearchOption` may skip
- Subdirectories remaining when only files are deleted in the loop
- Non-recursive delete failing on any remaining content
- `recursive: true` as the robust replacement for the manual loop

**Answer**

`Directory.Delete(path)` without `recursive: true` throws `IOException: The directory is not empty` if any file, subdirectory, or hidden OS-created file remains. Developers often delete all files via `Directory.GetFiles(path)` in a loop and then call non-recursive delete, but this fails because subdirectories are not touched, and hidden system files like `Thumbs.db` or `.DS_Store` may exist. Even if the loop is extended to handle subdirectories, hand-rolling tree deletion is slower and more fragile than the built-in recursive path. The robust fix is `Directory.Delete(path, recursive: true)`, which handles all content, hidden or not, in one kernel-guided operation. Dispose all open handles inside the tree first on Windows, because an open handle prevents that file's deletion and then blocks the containing directory.

---

## Q20. (Gotcha) What does `FileInfo` metadata staleness mean in practice?

**Concepts**
- `FileInfo` metadata cached at construction or last `Refresh()`
- Stale `Length` after external write to the same file
- `Refresh()` required to re-query the kernel
- `FileInfo.Exists` can return `true` for a deleted file before refresh

**Answer**

When you construct `new FileInfo(path)` and then another process writes to that file, subsequent reads of `Length` or `LastWriteTimeUtc` on the same `FileInfo` object return the cached values from construction time, not the current on-disk state. This is by design — caching amortizes the kernel round-trip — but it becomes a bug when the code assumes the metadata is live. The fix is to call `Refresh()` before accessing any metadata property that must reflect the current file state. A particularly insidious case is calling `Refresh()` after the file is deleted: `Length` throws `FileNotFoundException`, and `Exists` returns `false` after refresh even though it was `true` immediately after construction. Always call `Refresh()` after any operation — by this process or another — that could have changed the file.

---

## Q21. (Gotcha) Why does `File.OpenWrite` leave stale tail bytes when used to update file content?

**Concepts**
- `File.OpenWrite` opens with `FileMode.OpenOrCreate`, no truncation
- New write shorter than old content leaves old bytes past the write position
- `FileMode.Create` or `stream.SetLength(0)` to truncate before writing
- `File.WriteAllText` as the safe one-shot replacement

**Answer**

`File.OpenWrite(path)` opens with `FileMode.OpenOrCreate` positioned at byte zero but does NOT truncate the file. If the existing file is 500 bytes and you write 200 bytes, the first 200 bytes are your new content and bytes 200–499 still contain the old data. A subsequent reader sees a 500-byte file whose tail is stale. This is the correct semantic for streaming appends to a pre-allocated region, but wrong for full-content replacement. The fix is `File.Create(path)` (which uses `FileMode.Create` and does truncate), opening a `FileStream` with `FileMode.Truncate` if you need to keep the handle open, or using `File.WriteAllText`/`File.WriteAllBytes` for small files where one-shot replacement is the intent.

---

## Q22. (Gotcha) Why can relative paths produce `FileNotFoundException` in unexpected locations?

**Concepts**
- Relative paths resolved against `Environment.CurrentDirectory`
- `CurrentDirectory` is the process working directory, not the `.exe` folder
- IDE debug sessions, services, and unit test runners set different `CurrentDirectory` values
- `AppContext.BaseDirectory` as a stable anchor for deployed-relative paths
- `Path.GetFullPath` to diagnose the resolved path at runtime

**Answer**

Relative paths are resolved against `Environment.CurrentDirectory` — the process working directory — which is set by the shell or host that launched the process, not necessarily the folder containing the `.exe` or `.dll`. In Visual Studio, `CurrentDirectory` during a debug run is typically `bin\Debug\net10.0`; in a unit test runner it may be the test output folder or the solution root; in a systemd service it may be `/`. Hardcoding a relative path like `"data/config.json"` works in one environment and silently reads the wrong file or throws in another. The reliable fix is to anchor file paths to `AppContext.BaseDirectory` using `Path.Combine(AppContext.BaseDirectory, "data", "config.json")` for resources shipped alongside the assembly, or to `Environment.GetFolderPath(SpecialFolder.LocalApplicationData)` for user-specific data.

---

## Q23. (Scenario R) A report export service uses `File.Exists` + `File.Create` to guard concurrent writes — two threads occasionally get `IOException` or silently skip writing. What is wrong and how do you fix it?

**Defective code:**

```csharp
public static void EnsureReportFile(string path, string header)
{
    if (!File.Exists(path))                          // BUG 1: TOCTOU check
    {
        using var stream = File.Create(path);        // BUG 2: no FileMode.CreateNew
        var bytes = Encoding.UTF8.GetBytes(header);
        stream.Write(bytes, 0, bytes.Length);
        // BUG 3: no FileShare specified — exclusive by default
    }
    // BUG 4: second thread silently skips writing because Exists returned true
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | `File.Exists` and `File.Create` are non-atomic — another thread creates the file between them | Intermittent `IOException` or lost writes under load |
| Correctness | Second thread that sees `Exists == true` silently skips the header write | Report file may be created empty by the winner and never populated by the loser |
| Sharing | No `FileShare` specified — default `FileShare.None` blocks concurrent readers during write | Readers of the file during the write window get `IOException` |

**Fix priority**

1. Replace `File.Exists` check with `FileMode.CreateNew` — let the OS enforce atomicity.
2. Catch `IOException` and verify `File.Exists(path)` in the exception filter to distinguish "file already exists" from a genuine I/O error.
3. Specify `FileShare.Read` so concurrent readers can open the file while the header is being written.

```csharp
public static void EnsureReportFile(string path, string header)
{
    try
    {
        using var stream = new FileStream(path, FileMode.CreateNew,
                                          FileAccess.Write, FileShare.Read);
        var bytes = Encoding.UTF8.GetBytes(header);
        stream.Write(bytes, 0, bytes.Length);
    }
    catch (IOException) when (File.Exists(path))
    {
        // Another writer won the race — treat as idempotent success
    }
}
```

---

## Q24. (Scenario R) An upload service stages files in `Path.GetTempPath()` then calls `File.Move` to a different volume. It leaks temp files on exception and fails on Linux containers. What is wrong?

**Defective code:**

```csharp
public async Task SaveUploadAsync(IFormFile upload, string finalPath, CancellationToken ct)
{
    // BUG 1: temp on different volume than finalPath
    string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
    await using (var temp = File.Create(tempPath))
        await upload.CopyToAsync(temp, ct);         // BUG 2: no try/finally for temp cleanup

    if (File.Exists(finalPath))
        File.Delete(finalPath);                     // BUG 3: non-atomic delete-then-move

    File.Move(tempPath, finalPath);                 // cross-volume: copy-then-delete, not atomic
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | No `try/finally` — exception from `CopyToAsync` leaves orphaned `.bin` in temp | Temp disk fills over time; container restarts appear to "fix" it |
| Atomicity | Cross-volume `File.Move` degrades to copy-then-delete — crash between steps leaves partial or duplicate files | Corrupt final file or both old and new copies on disk |
| Platform | Linux `/tmp` is a separate `tmpfs` mount from the data volume — `File.Move` throws `IOException (EXDEV)` | All uploads fail in containerized deployments |

**Fix priority**

1. Stage the temp file in the same directory as `finalPath` (same volume = atomic rename).
2. Wrap the entire operation in `try/finally` to delete the temp on any exit path.
3. Use `File.Move(tmp, finalPath, overwrite: true)` (.NET 5+) to eliminate the separate delete step.

```csharp
public async Task SaveUploadAsync(IFormFile upload, string finalPath, CancellationToken ct)
{
    string tempPath = Path.Combine(
        Path.GetDirectoryName(finalPath)!, $".{Guid.NewGuid():N}.tmp");
    try
    {
        await using (var temp = File.Create(tempPath))
            await upload.CopyToAsync(temp, ct);
        File.Move(tempPath, finalPath, overwrite: true);
        tempPath = null!; // signal success so finally doesn't delete
    }
    finally
    {
        if (tempPath is not null && File.Exists(tempPath))
            File.Delete(tempPath);
    }
}
```

---

## Q25. (Scenario R) A cleanup job calls `Directory.Delete(root, recursive: false)` after a file-loop — throws in production on non-empty directories. What is wrong?

**Defective code:**

```csharp
public void PurgeWorkspace(string workspaceRoot)
{
    // BUG 1: GetFiles with no recursion misses nested subdirectories
    foreach (string file in Directory.GetFiles(workspaceRoot, "*",
                                               SearchOption.AllDirectories))
    {
        File.SetAttributes(file, FileAttributes.Normal);
        File.Delete(file);
    }
    // BUG 2: recursive:false fails when subdirectories still exist
    Directory.Delete(workspaceRoot, recursive: false);
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Deleting only files leaves subdirectories; non-recursive delete throws `IOException` | Cleanup fails in production; workspace accumulates |
| Memory | `Directory.GetFiles(..., AllDirectories)` eagerly loads entire path list into `string[]` | Memory spike proportional to workspace tree size |
| Incomplete | Does not clear `ReadOnly` attribute on subdirectories | Some subdirectory deletes throw `UnauthorizedAccessException` |

**Fix priority**

1. Replace the file loop + non-recursive delete with a single `Directory.Delete(workspaceRoot, recursive: true)`.
2. Clear `ReadOnly` attributes on all files and directories if `UnauthorizedAccessException` is possible.
3. Dispose all open handles inside the tree before calling delete.

```csharp
public void PurgeWorkspace(string workspaceRoot)
{
    foreach (string file in Directory.EnumerateFiles(
        workspaceRoot, "*", SearchOption.AllDirectories))
    {
        File.SetAttributes(file, FileAttributes.Normal);
    }
    Directory.Delete(workspaceRoot, recursive: true);
}
```

---

## Q26. (Scenario M) An ASP.NET Core endpoint reads a 200 MB CSV with `File.ReadAllText` on every request — thread-pool starvation and LOH pressure under load. What is wrong?

**Defective code:**

```csharp
app.MapGet("/reports/{id}", (string id, IReportStore store) =>
{
    string path = store.GetPath(id);
    if (!File.Exists(path))
        return Results.NotFound();

    string csv = File.ReadAllText(path);    // BUG 1: sync, blocks thread; BUG 2: 200 MB LOH string
    return Results.Content(csv, "text/csv");
});
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Thread-pool | Synchronous `File.ReadAllText` blocks a thread-pool thread for the entire disk read | Thread-pool starvation under concurrency; CPU idle while I/O waits |
| Memory | 200 MB `string` allocated on LOH per request | GC pressure, Gen2 collections, potential OOM under concurrent requests |
| TOCTOU | `File.Exists` check before read introduces race condition | File deleted between check and read causes `FileNotFoundException` instead of 404 |

**Fix priority**

1. Replace with `Results.File(path, "text/csv")` which uses `SendFileAsync` / kernel sendfile for zero-copy streaming.
2. If transformation is needed, use `Results.Stream(File.OpenRead(path), "text/csv")` to stream asynchronously.
3. Remove the `File.Exists` check and handle `FileNotFoundException` to eliminate the TOCTOU race.

---

## Q27. (Scenario D) A containerized API creates per-request scratch directories under `Path.GetTempPath()` but never cleans them up on exception. Compare three strategies.

**Concepts**
- `try/finally` scattering cleanup across every handler
- `IDisposable` temp workspace scoped to request lifetime
- Periodic janitor as secondary defense, not primary
- `emptyDir` Kubernetes volume with size limit as backstop

**Answer**

The default choice should be an `IDisposable` (or `IAsyncDisposable`) temp workspace object created at request entry: its constructor creates the directory under a Guid-named path, and its `Dispose` method calls `Directory.Delete(path, recursive: true)`. Using this object in a `using` statement in the handler means the compiler generates a `try/finally` that disposes on every exit path including unhandled exceptions — no cleanup logic is duplicated across endpoints. Inline `try/finally` in each handler works but forces each developer to write the same pattern and makes it easy to forget in a new endpoint. A periodic janitor that deletes directories older than N hours is valuable as a secondary defense — catching leaks from third-party library code and out-of-process kills — but must never be the primary mechanism because it allows unbounded growth between sweeps and can race against active scratch directories if naming is not unique. In Kubernetes, mount scratch space with `emptyDir: {sizeLimit: "500Mi"}` so container-level disk exhaustion fails fast with an eviction rather than silently filling the node's filesystem.
