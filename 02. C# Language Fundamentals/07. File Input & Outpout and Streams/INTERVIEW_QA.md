# 07. File Input & Outpout and Streams — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. File & Directory Operations](#01-file-directory-operations)
  - [Q1. Explain file handling in C# and the role of the `System.IO` namespace.](#q1-explain-file-handling-in-c-and-the-role-of-the-systemio-namespace)
  - [Q2. What is the difference between the static `File`/`Directory` classes and the instance `FileInfo`/`DirectoryInfo` classes?](#q2-what-is-the-difference-between-the-static-filedirectory-classes-and-the-instance-fileinfodirectoryinfo-classes)
  - [Q3. When would you prefer `FileInfo` over repeated `File.*` static calls on the same path?](#q3-when-would-you-prefer-fileinfo-over-repeated-file-static-calls-on-the-same-path)
  - [Q4. How do `Directory.GetFiles`, `Directory.GetDirectories`, and their `Enumerate*` counterparts differ in memory behavior?](#q4-how-do-directorygetfiles-directorygetdirectories-and-their-enumerate-counterparts-differ-in-memory-behavior)
  - [Q5. What does `Directory.CreateDirectory` do when intermediate folders already exist?](#q5-what-does-directorycreatedirectory-do-when-intermediate-folders-already-exist)
  - [Q6. What is the difference between `File.Copy` with `overwrite: false` vs `overwrite: true`, and what exception indicates a conflict?](#q6-what-is-the-difference-between-filecopy-with-overwrite-false-vs-overwrite-true-and-what-exception-indicates-a-conflict)
  - [Q7. How does `File.Move` differ from copy-then-delete, and what happens to metadata and hard links?](#q7-how-does-filemove-differ-from-copy-then-delete-and-what-happens-to-metadata-and-hard-links)
  - [Q8. What is `File.Replace`, and when is it preferable to manual backup-and-overwrite?](#q8-what-is-filereplace-and-when-is-it-preferable-to-manual-backup-and-overwrite)
  - [Q9. How do you safely delete a directory tree using `Directory.Delete(path, recursive: true)`?](#q9-how-do-you-safely-delete-a-directory-tree-using-directorydeletepath-recursive-true)
  - [Q10. What file metadata can you read via `File` static methods vs `FileInfo` instance properties?](#q10-what-file-metadata-can-you-read-via-file-static-methods-vs-fileinfo-instance-properties)
  - [Q11. Explain `File.GetCreationTime`, `GetLastWriteTime`, and `GetLastAccessTime` — and their `Utc` variants.](#q11-explain-filegetcreationtime-getlastwritetime-and-getlastaccesstime-and-their-utc-variants)
  - [Q12. How do you set creation, last-write, and last-access timestamps programmatically?](#q12-how-do-you-set-creation-last-write-and-last-access-timestamps-programmatically)
  - [Q13. What are `FileAttributes` (ReadOnly, Hidden, System, Archive)? How do you read and modify them?](#q13-what-are-fileattributes-readonly-hidden-system-archive-how-do-you-read-and-modify-them)
  - [Q14. What is the difference between `File.Exists` and attempting to open a file that may be deleted concurrently?](#q14-what-is-the-difference-between-fileexists-and-attempting-to-open-a-file-that-may-be-deleted-concurrently)
  - [Q15. What exceptions should you expect during file operations (`FileNotFoundException`, `DirectoryNotFoundException`, `IOException`, `UnauthorizedAccessException`)?](#q15-what-exceptions-should-you-expect-during-file-operations-filenotfoundexception-directorynotfoundexception-ioexception-unauthorizedaccessexception)
  - [Q16. How does `File.AppendAllText` differ from opening with `FileMode.Append`?](#q16-how-does-fileappendalltext-differ-from-opening-with-filemodeappend)
  - [Q17. When is `File.ReadAllBytes` / `File.WriteAllBytes` appropriate vs stream-based APIs?](#q17-when-is-filereadallbytes-filewriteallbytes-appropriate-vs-stream-based-apis)
  - [Q18. Explain `File.Create`, `File.Open`, `File.OpenRead`, and `File.OpenWrite` — what modes and access do they imply?](#q18-explain-filecreate-fileopen-fileopenread-and-fileopenwrite-what-modes-and-access-do-they-imply)
  - [Q19. How do you handle TOCTOU (time-of-check-time-of-use) races when checking existence before read/write?](#q19-how-do-you-handle-toctou-time-of-check-time-of-use-races-when-checking-existence-before-readwrite)
  - [Q20. What is the difference between deleting a file and clearing its contents while keeping the path?](#q20-what-is-the-difference-between-deleting-a-file-and-clearing-its-contents-while-keeping-the-path)

- [02. StreamReader & StreamWriter](#02-streamreader-streamwriter)
  - [Q1. Explain the `Stream` base class hierarchy and where `StreamReader`/`StreamWriter` fit.](#q1-explain-the-stream-base-class-hierarchy-and-where-streamreaderstreamwriter-fit)
  - [Q2. What is the difference between `File.ReadAllText`, `File.ReadAllLines`, and `File.ReadLines`?](#q2-what-is-the-difference-between-filereadalltext-filereadalllines-and-filereadlines)
  - [Q3. Why can `File.ReadLines` hold a file lock until enumeration completes?](#q3-why-can-filereadlines-hold-a-file-lock-until-enumeration-completes)
  - [Q4. How do `StreamReader.ReadLine`, `ReadToEnd`, and `ReadBlock` differ for large files?](#q4-how-do-streamreaderreadline-readtoend-and-readblock-differ-for-large-files)
  - [Q5. What is the default encoding for `StreamReader` and `StreamWriter`, and why can that cause mojibake?](#q5-what-is-the-default-encoding-for-streamreader-and-streamwriter-and-why-can-that-cause-mojibake)
  - [Q6. How do you specify `Encoding.UTF8`, UTF-8 with BOM, and legacy encodings (`Encoding.GetEncoding`)?](#q6-how-do-you-specify-encodingutf8-utf-8-with-bom-and-legacy-encodings-encodinggetencoding)
  - [Q7. What does `StreamReader.DetectEncodingFromByteOrderMarks` control?](#q7-what-does-streamreaderdetectencodingfrombyteordermarks-control)
  - [Q8. Explain async read/write methods on `StreamReader`/`StreamWriter` (`ReadLineAsync`, `WriteLineAsync`, `ReadToEndAsync`).](#q8-explain-async-readwrite-methods-on-streamreaderstreamwriter-readlineasync-writelineasync-readtoendasync)
  - [Q9. What is `StreamWriter.AutoFlush`, and when should you call `Flush()` explicitly?](#q9-what-is-streamwriterautoflush-and-when-should-you-call-flush-explicitly)
  - [Q10. How do you append text to an existing file with `StreamWriter` (constructor overload with `append: true`)?](#q10-how-do-you-append-text-to-an-existing-file-with-streamwriter-constructor-overload-with-append-true)
  - [Q11. What happens if you forget to dispose a `StreamWriter` — especially on Windows file locking?](#q11-what-happens-if-you-forget-to-dispose-a-streamwriter-especially-on-windows-file-locking)
  - [Q12. Can you use `StreamReader`/`StreamWriter` with non-file streams (memory, network)? Give examples.](#q12-can-you-use-streamreaderstreamwriter-with-non-file-streams-memory-network-give-examples)
  - [Q13. What is the difference between `using` blocks and C# 8 `using` declarations for stream cleanup?](#q13-what-is-the-difference-between-using-blocks-and-c-8-using-declarations-for-stream-cleanup)
  - [Q14. How do you read a file line-by-line without loading it entirely into memory?](#q14-how-do-you-read-a-file-line-by-line-without-loading-it-entirely-into-memory)
  - [Q15. What is `TextReader`/`TextWriter`, and why do APIs often accept these abstractions?](#q15-what-is-textreadertextwriter-and-why-do-apis-often-accept-these-abstractions)

- [03. FileStream & Binary Files](#03-filestream-binary-files)
  - [Q1. What is the difference between `File`, `Stream`, and `FileStream`?](#q1-what-is-the-difference-between-file-stream-and-filestream)
  - [Q2. Explain `FileMode` (`CreateNew`, `Create`, `Open`, `OpenOrCreate`, `Truncate`, `Append`) — when use each?](#q2-explain-filemode-createnew-create-open-openorcreate-truncate-append-when-use-each)
  - [Q3. Explain `FileAccess` (`Read`, `Write`, `ReadWrite`) and `FileShare` (`None`, `Read`, `Write`, `ReadWrite`, `Delete`).](#q3-explain-fileaccess-read-write-readwrite-and-fileshare-none-read-write-readwrite-delete)
  - [Q4. Why does default `FileShare.None` cause sharing violations when another process needs read access?](#q4-why-does-default-filesharenone-cause-sharing-violations-when-another-process-needs-read-access)
  - [Q5. What are `FileStream.Position`, `Seek`, and `Length` — and when is seeking valid?](#q5-what-are-filestreamposition-seek-and-length-and-when-is-seeking-valid)
  - [Q6. Explain `SeekOrigin` (`Begin`, `Current`, `End`) with a concrete read-modify-write scenario.](#q6-explain-seekorigin-begin-current-end-with-a-concrete-read-modify-write-scenario)
  - [Q7. What happens if you seek on a non-seekable stream (e.g., some network streams)?](#q7-what-happens-if-you-seek-on-a-non-seekable-stream-eg-some-network-streams)
  - [Q8. What is the difference between `FileStream.Read`/`Write` and `ReadAsync`/`WriteAsync`?](#q8-what-is-the-difference-between-filestreamreadwrite-and-readasyncwriteasync)
  - [Q9. What do `BinaryReader` and `BinaryWriter` add over raw `FileStream` byte operations?](#q9-what-do-binaryreader-and-binarywriter-add-over-raw-filestream-byte-operations)
  - [Q10. How does `BinaryReader` handle endianness and primitive types (`ReadInt32`, `ReadDouble`, `ReadString`)?](#q10-how-does-binaryreader-handle-endianness-and-primitive-types-readint32-readdouble-readstring)
  - [Q11. What is the on-disk format of `BinaryWriter.Write(string)` — and why does it matter for cross-platform files?](#q11-what-is-the-on-disk-format-of-binarywriterwritestring-and-why-does-it-matter-for-cross-platform-files)
  - [Q12. What is the difference between text and binary file handling in C#?](#q12-what-is-the-difference-between-text-and-binary-file-handling-in-c)
  - [Q13. When should you use `MemoryStream` instead of `FileStream`?](#q13-when-should-you-use-memorystream-instead-of-filestream)
  - [Q14. What is buffered I/O, and how do `FileStream` buffer size options affect performance?](#q14-what-is-buffered-io-and-how-do-filestream-buffer-size-options-affect-performance)
  - [Q15. How do you read a fixed header followed by variable-length records from a binary file?](#q15-how-do-you-read-a-fixed-header-followed-by-variable-length-records-from-a-binary-file)
  - [Q16. What is a file signature (magic bytes), and how do you validate one without trusting the extension?](#q16-what-is-a-file-signature-magic-bytes-and-how-do-you-validate-one-without-trusting-the-extension)

- [04. Path & Environment Classes](#04-path-environment-classes)
  - [Q1. What is the `Path` class, and why should you never hard-code `\` or `/` separators?](#q1-what-is-the-path-class-and-why-should-you-never-hard-code-or-separators)
  - [Q2. How does `Path.Combine` behave with trailing slashes, rooted segments, and empty segments?](#q2-how-does-pathcombine-behave-with-trailing-slashes-rooted-segments-and-empty-segments)
  - [Q3. What is the difference between `Path.GetFullPath` and passing a relative path directly to `File.Open`?](#q3-what-is-the-difference-between-pathgetfullpath-and-passing-a-relative-path-directly-to-fileopen)
  - [Q4. Explain `Path.GetDirectoryName`, `GetFileName`, `GetFileNameWithoutExtension`, and `GetExtension`.](#q4-explain-pathgetdirectoryname-getfilename-getfilenamewithoutextension-and-getextension)
  - [Q5. What do `Path.GetTempPath`, `Path.GetTempFileName`, and `Path.GetRandomFileName` return — and what are the security implications of `GetTempFileName`?](#q5-what-do-pathgettemppath-pathgettempfilename-and-pathgetrandomfilename-return-and-what-are-the-security-implications-of-gettempfilename)
  - [Q6. How do `Path.IsPathRooted`, `HasExtension`, and `ChangeExtension` work?](#q6-how-do-pathispathrooted-hasextension-and-changeextension-work)
  - [Q7. What invalid path characters does `Path.GetInvalidPathChars` / `GetInvalidFileNameChars` expose?](#q7-what-invalid-path-characters-does-pathgetinvalidpathchars-getinvalidfilenamechars-expose)
  - [Q8. What is `Environment.SpecialFolder`, and how do you resolve `MyDocuments`, `ApplicationData`, and `LocalApplicationData`?](#q8-what-is-environmentspecialfolder-and-how-do-you-resolve-mydocuments-applicationdata-and-localapplicationdata)
  - [Q9. How does `Environment.GetFolderPath` differ from hard-coding `C:\Users\...`?](#q9-how-does-environmentgetfolderpath-differ-from-hard-coding-cusers)
  - [Q10. What is `Environment.CurrentDirectory`, and how can it differ from the executable's location?](#q10-what-is-environmentcurrentdirectory-and-how-can-it-differ-from-the-executables-location)
  - [Q11. How do you get the application base directory in modern .NET (`AppContext.BaseDirectory`, `AppDomain.CurrentDomain.BaseDirectory`)?](#q11-how-do-you-get-the-application-base-directory-in-modern-net-appcontextbasedirectory-appdomaincurrentdomainbasedirectory)
  - [Q12. What is the difference between absolute and relative paths in console apps vs ASP.NET Core?](#q12-what-is-the-difference-between-absolute-and-relative-paths-in-console-apps-vs-aspnet-core)
  - [Q13. How do UNC paths (`\\server\share`) interact with `Path.Combine` and `Path.GetFullPath`?](#q13-how-do-unc-paths-servershare-interact-with-pathcombine-and-pathgetfullpath)
  - [Q14. What cross-platform path differences matter when deploying the same code on Windows and Linux?](#q14-what-cross-platform-path-differences-matter-when-deploying-the-same-code-on-windows-and-linux)

- [05. Working with CSV and Text Files](#05-working-with-csv-and-text-files)
  - [Q1. Why is there no built-in CSV parser in the BCL, and what libraries are commonly used?](#q1-why-is-there-no-built-in-csv-parser-in-the-bcl-and-what-libraries-are-commonly-used)
  - [Q2. What are RFC 4180 rules for CSV fields, delimiters, and record terminators?](#q2-what-are-rfc-4180-rules-for-csv-fields-delimiters-and-record-terminators)
  - [Q3. When must a CSV field be wrapped in double quotes?](#q3-when-must-a-csv-field-be-wrapped-in-double-quotes)
  - [Q4. How do you escape a literal double quote inside a quoted CSV field?](#q4-how-do-you-escape-a-literal-double-quote-inside-a-quoted-csv-field)
  - [Q5. What goes wrong if you split CSV lines on `Split(',')` without a proper parser?](#q5-what-goes-wrong-if-you-split-csv-lines-on-split-without-a-proper-parser)
  - [Q6. How do you handle embedded newlines inside quoted CSV fields?](#q6-how-do-you-handle-embedded-newlines-inside-quoted-csv-fields)
  - [Q7. What issues arise with culture-specific decimal separators in CSV numeric columns?](#q7-what-issues-arise-with-culture-specific-decimal-separators-in-csv-numeric-columns)
  - [Q8. How do you write CSV headers and ensure stable column ordering for downstream consumers?](#q8-how-do-you-write-csv-headers-and-ensure-stable-column-ordering-for-downstream-consumers)
  - [Q9. What is the difference between `\n`, `\r\n`, and `Environment.NewLine` for text file line endings?](#q9-what-is-the-difference-between-n-rn-and-environmentnewline-for-text-file-line-endings)
  - [Q10. How do you normalize line endings when reading files produced on Windows vs Linux?](#q10-how-do-you-normalize-line-endings-when-reading-files-produced-on-windows-vs-linux)
  - [Q11. What are best practices for large CSV ingestion (streaming vs loading all rows)?](#q11-what-are-best-practices-for-large-csv-ingestion-streaming-vs-loading-all-rows)
  - [Q12. How do you validate CSV row shape (column count) before deserializing to objects?](#q12-how-do-you-validate-csv-row-shape-column-count-before-deserializing-to-objects)
  - [Q13. When should you use fixed-width text formats instead of CSV?](#q13-when-should-you-use-fixed-width-text-formats-instead-of-csv)
  - [Q14. How do you properly dispose file resources across layered readers (`FileStream` → `StreamReader`)?](#q14-how-do-you-properly-dispose-file-resources-across-layered-readers-filestream-streamreader)
  - [Q15. What logging and rotation patterns apply when appending to text log files over time?](#q15-what-logging-and-rotation-patterns-apply-when-appending-to-text-log-files-over-time)
  - [Q16. **`ReadAllLines` vs `ReadLines`** — `ReadAllLines` loads the entire file into a `string[]`; `ReadLines` is lazy but keeps the file open until enumeration finishes or is disposed.](#q16-readalllines-vs-readlines-readalllines-loads-the-entire-file-into-a-string-readlines-is-lazy-but-keeps-the-file-open-until-enumeration-finishes-or-is-disposed)
  - [Q17. **Undisposed streams lock files on Windows** — A finalized-but-not-disposed `FileStream`/`StreamWriter` can block deletes, renames, and antivirus scans until GC runs.](#q17-undisposed-streams-lock-files-on-windows-a-finalized-but-not-disposed-filestreamstreamwriter-can-block-deletes-renames-and-antivirus-scans-until-gc-runs)
  - [Q18. **`FileShare` defaults to exclusive access** — Opening without `FileShare.Read` prevents other processes from reading concurrently.](#q18-fileshare-defaults-to-exclusive-access-opening-without-fileshareread-prevents-other-processes-from-reading-concurrently)
  - [Q19. **Hard-coded path separators break cross-platform** — `"folder\\file.txt"` fails on Linux; always use `Path.Combine`.](#q19-hard-coded-path-separators-break-cross-platform-folderfiletxt-fails-on-linux-always-use-pathcombine)
  - [Q20. **Relative paths depend on `CurrentDirectory`** — A path valid in Visual Studio may fail as a Windows Service or cron job where CWD differs.](#q20-relative-paths-depend-on-currentdirectory-a-path-valid-in-visual-studio-may-fail-as-a-windows-service-or-cron-job-where-cwd-differs)
  - [Q21. **`Path.Combine` with an absolute second segment discards earlier parts** — `Path.Combine("C:\\a", "D:\\b")` yields `D:\b`, which surprises many candidates.](#q21-pathcombine-with-an-absolute-second-segment-discards-earlier-parts-pathcombineca-db-yields-db-which-surprises-many-candidates)
  - [Q22. **Encoding mismatch silently corrupts text** — Default UTF-8 assumptions break on Windows-1252 or UTF-16 LE files; specify `Encoding` explicitly.](#q22-encoding-mismatch-silently-corrupts-text-default-utf-8-assumptions-break-on-windows-1252-or-utf-16-le-files-specify-encoding-explicitly)
  - [Q23. **Seeking past EOF then writing extends the file with undefined gap bytes** — Understand sparse/hole behavior when patching binary files in place.](#q23-seeking-past-eof-then-writing-extends-the-file-with-undefined-gap-bytes-understand-sparsehole-behavior-when-patching-binary-files-in-place)
  - [Q24. **CSV `Split(',')` breaks on quoted commas** — `"Smith, Jr.",42` becomes three fields; use a real parser or state machine.](#q24-csv-split-breaks-on-quoted-commas-smith-jr42-becomes-three-fields-use-a-real-parser-or-state-machine)
  - [Q25. **Double-quote escaping in CSV is `""` not `\"`** — Getting escape rules wrong produces columns that shift on import.](#q25-double-quote-escaping-in-csv-is-not-getting-escape-rules-wrong-produces-columns-that-shift-on-import)
  - [Q1. (R) A report export service must create a file only if it does not already exist. Review this helper used under concurrent load:](#q1-r-a-report-export-service-must-create-a-file-only-if-it-does-not-already-exist-review-this-helper-used-under-concurrent-load)
  - [Q2. (R) A teammate refactors upload processing to write through a temp file, then move into place. Review the method:](#q2-r-a-teammate-refactors-upload-processing-to-write-through-a-temp-file-then-move-into-place-review-the-method)
  - [Q3. (R) A nightly cleanup job removes old workspace folders. Review:](#q3-r-a-nightly-cleanup-job-removes-old-workspace-folders-review)
  - [Q4. (P) A multi-process log aggregator appends audit lines from several worker threads. One worker uses `File.AppendAllText`; another opens with default sharing:](#q4-p-a-multi-process-log-aggregator-appends-audit-lines-from-several-worker-threads-one-worker-uses-fileappendalltext-another-opens-with-default-sharing)
  - [Q5. (P) An export job stages files under `%TEMP%` on Windows, then calls `File.Move(source, dest)` into a network share. On developer laptops it works; in Azure App Service (Linux) and when crossing drive letters it fails with `IOException` or leaves duplicate files. What is happening at the OS level, and what pattern replaces naive `File.Move`?](#q5-p-an-export-job-stages-files-under-temp-on-windows-then-calls-filemovesource-dest-into-a-network-share-on-developer-laptops-it-works-in-azure-app-service-linux-and-when-crossing-drive-letters-it-fails-with-ioexception-or-leaves-duplicate-files-what-is-happening-at-the-os-level-and-what-pattern-replaces-naive-filemove)
  - [Q6. (M) An ASP.NET Core endpoint reads a 200 MB CSV from disk on every request:](#q6-m-an-aspnet-core-endpoint-reads-a-200-mb-csv-from-disk-on-every-request)
  - [Q7. (D) A containerized API creates per-request scratch directories under `Path.GetTempPath()` but never deletes them when handlers throw. Disk on the node fills over days; restarting the pod "fixes" it until the next deploy. Compare three cleanup strategies — `try/finally`, `IDisposable` workspace helper, and OS temp with periodic janitor — for production container deployments. What is your default and why?](#q7-d-a-containerized-api-creates-per-request-scratch-directories-under-pathgettemppath-but-never-deletes-them-when-handlers-throw-disk-on-the-node-fills-over-days-restarting-the-pod-fixes-it-until-the-next-deploy-compare-three-cleanup-strategies-tryfinally-idisposable-workspace-helper-and-os-temp-with-periodic-janitor-for-production-container-deployments-what-is-your-default-and-why)

- [02. StreamReader & StreamWriter](#02-streamreader-streamwriter-1)

- [02. StreamReader & StreamWriter](#02-streamreader-streamwriter-2)
  - [Q1. (R) A nightly audit job throws on bad rows and operators report the log file stays locked until the worker restarts. Review this helper:](#q1-r-a-nightly-audit-job-throws-on-bad-rows-and-operators-report-the-log-file-stays-locked-until-the-worker-restarts-review-this-helper)
  - [Q2. (R) A CSV export looks correct on the developer's Windows machine but the first column header fails validation after deploy to Linux containers. Review write vs read:](#q2-r-a-csv-export-looks-correct-on-the-developers-windows-machine-but-the-first-column-header-fails-validation-after-deploy-to-linux-containers-review-write-vs-read)
  - [Q3. (R) A support dashboard calls this to show the tail of a customer log. Under load the worker process recycles with `OutOfMemoryException`. Review:](#q3-r-a-support-dashboard-calls-this-to-show-the-tail-of-a-customer-log-under-load-the-worker-process-recycles-with-outofmemoryexception-review)
  - [Q4. (R) A long-running export writes a status file so another process can poll completion. Operators see `IN_PROGRESS` forever after a crash mid-run. Review:](#q4-r-a-long-running-export-writes-a-status-file-so-another-process-can-poll-completion-operators-see-in_progress-forever-after-a-crash-mid-run-review)
  - [Q5. (R) A log tailer and a log writer run in the same app. The tailer intermittently throws `IOException: The process cannot access the file because it is being used by another process`. Review:](#q5-r-a-log-tailer-and-a-log-writer-run-in-the-same-app-the-tailer-intermittently-throws-ioexception-the-process-cannot-access-the-file-because-it-is-being-used-by-another-process-review)
  - [Q6. (P) An ASP.NET Core hosted service ingests a growing feed file every few seconds. A developer keeps sync I/O "because the file is local":](#q6-p-an-aspnet-core-hosted-service-ingests-a-growing-feed-file-every-few-seconds-a-developer-keeps-sync-io-because-the-file-is-local)
  - [Q7. (R) A tool rewrites the first line of a config file in place, then reads the remainder. After a refactor it throws `ObjectDisposedException`. Review:](#q7-r-a-tool-rewrites-the-first-line-of-a-config-file-in-place-then-reads-the-remainder-after-a-refactor-it-throws-objectdisposedexception-review)
  - [Q8. (M) A cross-platform app parses `.env`-style files written on mixed developer machines (Windows CRLF, macOS/Linux LF). Review ingestion:](#q8-m-a-cross-platform-app-parses-env-style-files-written-on-mixed-developer-machines-windows-crlf-macoslinux-lf-review-ingestion)

- [03. FileStream & Binary Files](#03-filestream-binary-files-1)

- [03. FileStream & Binary Files](#03-filestream-binary-files-2)
  - [Q1. (R) A telemetry service reads a fixed 4-byte file signature from `signature.bin`. In production, short files produce garbage signatures without throwing. Review the reader:](#q1-r-a-telemetry-service-reads-a-fixed-4-byte-file-signature-from-signaturebin-in-production-short-files-produce-garbage-signatures-without-throwing-review-the-reader)
  - [Q2. (R) A background job appends binary audit records while a dashboard process tries to read the same file. The writer opens like this; the reader gets `IOException: The process cannot access the file`:](#q2-r-a-background-job-appends-binary-audit-records-while-a-dashboard-process-tries-to-read-the-same-file-the-writer-opens-like-this-the-reader-gets-ioexception-the-process-cannot-access-the-file)
  - [Q3. (R) A teammate ports `inventory.bin` readers from another language and swaps field order on one record type. The file opens fine but prices and names are nonsense after the first record. Review:](#q3-r-a-teammate-ports-inventorybin-readers-from-another-language-and-swaps-field-order-on-one-record-type-the-file-opens-fine-but-prices-and-names-are-nonsense-after-the-first-record-review)
  - [Q4. (R) A log-rotation utility reads the last 8 bytes of a growing file to verify a footer magic. It intermittently returns wrong bytes under load. Review:](#q4-r-a-log-rotation-utility-reads-the-last-8-bytes-of-a-growing-file-to-verify-a-footer-magic-it-intermittently-returns-wrong-bytes-under-load-review)
  - [Q5. (P) Your .NET service writes `metrics.bin` consumed by a Linux C tool on big-endian ARM. A developer uses default `BinaryWriter`/`BinaryReader` for `int` and `double` fields. Locally on x64 Windows everything works; in staging the C tool reads garbage. What is the root cause, and how do you design a cross-platform binary layout?](#q5-p-your-net-service-writes-metricsbin-consumed-by-a-linux-c-tool-on-big-endian-arm-a-developer-uses-default-binarywriterbinaryreader-for-int-and-double-fields-locally-on-x64-windows-everything-works-in-staging-the-c-tool-reads-garbage-what-is-the-root-cause-and-how-do-you-design-a-cross-platform-binary-layout)
  - [Q6. (D) A data pipeline must scan a 60 GB append-only binary archive for records matching a key — random access by fixed record index, not full sequential parse every time. A junior proposes `FileStream` + `Seek` per lookup; a senior suggests `MemoryMappedFile`. What are the trade-offs, and when would you still choose streaming?](#q6-d-a-data-pipeline-must-scan-a-60-gb-append-only-binary-archive-for-records-matching-a-key-random-access-by-fixed-record-index-not-full-sequential-parse-every-time-a-junior-proposes-filestream-seek-per-lookup-a-senior-suggests-memorymappedfile-what-are-the-trade-offs-and-when-would-you-still-choose-streaming)
  - [Q7. (R) An export worker writes large binary batches with async I/O, then signals a downstream processor via a message queue. The processor often reads zero-length or incomplete files. Review:](#q7-r-an-export-worker-writes-large-binary-batches-with-async-io-then-signals-a-downstream-processor-via-a-message-queue-the-processor-often-reads-zero-length-or-incomplete-files-review)
  - [Q8. (R) A cache service tries to wipe and rewrite `cache.bin` in one handle. It throws at runtime despite the path existing. Review both open attempts:](#q8-r-a-cache-service-tries-to-wipe-and-rewrite-cachebin-in-one-handle-it-throws-at-runtime-despite-the-path-existing-review-both-open-attempts)

- [04. Path & Environment Classes](#04-path-environment-classes-1)

- [04. Path & Environment Classes](#04-path-environment-classes-2)
  - [Q1. (R) A report exporter works on Windows dev machines but fails on Linux CI with "Could not find a part of the path." Review this path builder:](#q1-r-a-report-exporter-works-on-windows-dev-machines-but-fails-on-linux-ci-with-could-not-find-a-part-of-the-path-review-this-path-builder)
  - [Q2. (R) An internal admin API accepts a `fileName` query parameter and serves files from a fixed folder. Review the handler:](#q2-r-an-internal-admin-api-accepts-a-filename-query-parameter-and-serves-files-from-a-fixed-folder-review-the-handler)
  - [Q3. (P) A worker service loads `config/settings.json` with a relative path. It passes locally from Visual Studio but fails in production when started as a Windows Service or from a systemd unit. The startup code:](#q3-p-a-worker-service-loads-configsettingsjson-with-a-relative-path-it-passes-locally-from-visual-studio-but-fails-in-production-when-started-as-a-windows-service-or-from-a-systemd-unit-the-startup-code)
  - [Q4. (P) A containerized API writes large PDF exports using `Path.GetTempFileName()` and never deletes them. After a few days in Kubernetes, pods hit `No space left on device`. The temp folder path is `/tmp` inside the container. What breaks in this pattern, and what production approach replaces `GetTempFileName`?](#q4-p-a-containerized-api-writes-large-pdf-exports-using-pathgettempfilename-and-never-deletes-them-after-a-few-days-in-kubernetes-pods-hit-no-space-left-on-device-the-temp-folder-path-is-tmp-inside-the-container-what-breaks-in-this-pattern-and-what-production-approach-replaces-gettempfilename)
  - [Q5. (R) A desktop-style feature is ported to a headless Linux server without changes:](#q5-r-a-desktop-style-feature-is-ported-to-a-headless-linux-server-without-changes)
  - [Q6. (M) A path helper builds log file locations from configuration segments. Review this method called on both Windows and Linux:](#q6-m-a-path-helper-builds-log-file-locations-from-configuration-segments-review-this-method-called-on-both-windows-and-linux)
  - [Q7. (D) Two services exchange file paths over a message queue. Service A (Windows) sends `D:\data\invoices\inv-001.pdf`. Service B (Linux) tries to open it and also needs a relative path for an audit log entry. A developer writes:](#q7-d-two-services-exchange-file-paths-over-a-message-queue-service-a-windows-sends-ddatainvoicesinv-001pdf-service-b-linux-tries-to-open-it-and-also-needs-a-relative-path-for-an-audit-log-entry-a-developer-writes)
  - [Q8. (P) A build pipeline archives deeply nested test output on Windows agents. One test creates a folder tree exceeding 260 characters. Locally it works when long-path support is enabled; on a Linux agent the same code runs but a Windows-only integration test fails with `PathTooLongException`. What explains the platform difference, and what mitigations belong in the path-building code?](#q8-p-a-build-pipeline-archives-deeply-nested-test-output-on-windows-agents-one-test-creates-a-folder-tree-exceeding-260-characters-locally-it-works-when-long-path-support-is-enabled-on-a-linux-agent-the-same-code-runs-but-a-windows-only-integration-test-fails-with-pathtoolongexception-what-explains-the-platform-difference-and-what-mitigations-belong-in-the-path-building-code)

- [05. Working with CSV and Text Files](#05-working-with-csv-and-text-files-1)

- [05. Working with CSV and Text Files](#05-working-with-csv-and-text-files-2)
  - [Q1. (R) A partner feed import worked in QA but mis-maps vendor names in production. Review this row parser used for every data line after the header:](#q1-r-a-partner-feed-import-worked-in-qa-but-mis-maps-vendor-names-in-production-review-this-row-parser-used-for-every-data-line-after-the-header)
  - [Q2. (R) An export job writes inventory CSV on a German Windows server; a US warehouse tool rejects half the rows. Product names with accents arrive as `MÃ¼ller` when the US tool opens the file. Review the export path:](#q2-r-an-export-job-writes-inventory-csv-on-a-german-windows-server-a-us-warehouse-tool-rejects-half-the-rows-product-names-with-accents-arrive-as-mã¼ller-when-the-us-tool-opens-the-file-review-the-export-path)
  - [Q3. (R) A nightly import job loads a 400 MB ERP export. Review the service method:](#q3-r-a-nightly-import-job-loads-a-400-mb-erp-export-review-the-service-method)
  - [Q4. (P) A drop-folder service uses `FileSystemWatcher` to import CSV as soon as a file appears in `\\share\inbound`. Operators report random "column count" errors and duplicate SKU rows. The handler:](#q4-p-a-drop-folder-service-uses-filesystemwatcher-to-import-csv-as-soon-as-a-file-appears-in-shareinbound-operators-report-random-column-count-errors-and-duplicate-sku-rows-the-handler)
  - [Q5. (P) Re-running the same inbound file after a network blip must not double inventory counts. A developer adds a guard:](#q5-p-re-running-the-same-inbound-file-after-a-network-blip-must-not-double-inventory-counts-a-developer-adds-a-guard)
  - [Q6. (D) Your team must ingest partner CSV feeds with quoted commas, optional date columns, and occasional header renames (`SKU` vs `Sku`). One engineer proposes `CsvHelper`; another wants to extend the hand-rolled `SplitQuotedLine` from this chapter. When do you reach for each, and what are the trade-offs for a long-lived warehouse integration?](#q6-d-your-team-must-ingest-partner-csv-feeds-with-quoted-commas-optional-date-columns-and-occasional-header-renames-sku-vs-sku-one-engineer-proposes-csvhelper-another-wants-to-extend-the-hand-rolled-splitquotedline-from-this-chapter-when-do-you-reach-for-each-and-what-are-the-trade-offs-for-a-long-lived-warehouse-integration)
  - [Q7. (R) Two import workers occasionally corrupt the same nightly file. Review the concurrent access pattern:](#q7-r-two-import-workers-occasionally-corrupt-the-same-nightly-file-review-the-concurrent-access-pattern)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. File & Directory Operations

#### Q1. Explain file handling in C# and the role of the `System.IO` namespace.

(R) A report export service must create a file only if it does not already exist. Review this helper used under concurrent load:

```csharp
public static void EnsureReportFile(string path, string header)
{
    if (!File.Exists(path))
    {
        using var stream = File.Create(path);
        var bytes = Encoding.UTF8.GetBytes(header);
        stream.Write(bytes, 0, bytes.Length);
    }
}
```

Two requests for the same path occasionally throw `IOException: file already exists`, and sometimes one request silently skips writing. What is wrong, and how do you fix it for production?

**Answer:** This is a classic TOCTOU (time-of-check to time-of-use) race: `File.Exists` and `File.Create` are not atomic, so two threads can both pass the check and one `File.Create` wins while the other throws, or one thread creates the file after another passed `Exists` and the second call skips writing entirely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Check-then-act gap between `Exists` and `Create` | Duplicate create attempts → `IOException`; skipped writes when file appears between check and branch |
| Concurrency | No synchronization or exclusive-create semantics | Intermittent failures under load — passes in single-threaded dev |
| Design | `Exists` + `Create` mimics "create if missing" without atomicity | Wrong abstraction for idempotent report generation |

**Fix (priority order):**

1. Use an exclusive create that fails fast if the file already exists — `new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)` — and catch `IOException` to treat "already exists" as expected, or return a conflict result.
2. If multiple writers must coordinate, add an app-level lock keyed by path, or use a database/object-store claim — filesystem races do not scale across pods without external coordination.
3. For idempotent content, prefer write-to-temp-then-atomic-rename (see Q2/Q5) instead of "create only if missing."
4. Remove the silent skip path — if the file exists but is empty or stale, `Exists` returning true hides a partial write from a crashed peer.

```csharp
try
{
    using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    var bytes = Encoding.UTF8.GetBytes(header);
    stream.Write(bytes, 0, bytes.Length);
}
catch (IOException) when (File.Exists(path))
{
    // Another writer won the race — handle idempotently or surface conflict
}
```

**Production takeaway:** Karat uses `File.Exists` + `File.Create` to test whether you know TOCTOU — the fix is atomic open semantics (`CreateNew`) or external locking, not a tighter `if`. See **Program.cs** Section 8 — guard before read is not the same as atomic create.

---

#### Q2. What is the difference between the static `File`/`Directory` classes and the instance `FileInfo`/`DirectoryInfo` classes?

(R) A teammate refactors upload processing to write through a temp file, then move into place. Review the method:

```csharp
public async Task SaveUploadAsync(IFormFile upload, string finalPath, CancellationToken ct)
{
    string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
    await using (var temp = File.Create(tempPath))
    {
        await upload.CopyToAsync(temp, ct);
    }

    if (File.Exists(finalPath))
        File.Delete(finalPath);

    File.Move(tempPath, finalPath);
}
```

What breaks when `CopyToAsync` throws, when the app runs in a Linux container with a read-only root filesystem, and when two pods write the same `finalPath`?

**Answer:** The temp-then-move pattern is right in spirit, but this version leaks temp files on failure, may write temps to an unwritable or ephemeral location in containers, and still has TOCTOU races on the final path — plus `File.Move` is not atomic across volumes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource cleanup | No `try/finally` or `try/catch` to delete `tempPath` on failure | Orphaned `.bin` files fill `%TEMP%` / container overlay — see Q7 |
| Deployment | `Path.GetTempPath()` + `finalPath` on read-only app dir | `CopyToAsync` or `Move` throws in Kubernetes/App Service when dest is not writable |
| Concurrency | `Exists` → `Delete` → `Move` on shared `finalPath` | Two pods interleave deletes/moves — corrupt or missing final file |
| Cross-volume | `File.Move` between temp dir and data volume | Becomes copy+delete — not atomic; crash mid-flight leaves duplicates or partial files |

**Fix (priority order):**

1. Wrap temp lifecycle in `try/finally` (or a `TempFile`/`TempWorkspace` disposable) that deletes the temp path on any exception.
2. Stage temp files in the **same directory** as `finalPath` (e.g. `finalPath + ".tmp"`) so `File.Move` is a same-volume rename — atomic on POSIX and NTFS for same directory.
3. Write to `finalPath.tmp`, flush/fsync if durability matters, then `File.Move(tmp, finalPath, overwrite: true)` (.NET 5+) — avoid separate delete step.
4. In containers, mount a writable volume for uploads (`/app/data` or blob storage); never assume `AppContext.BaseDirectory` or root FS is writable.
5. For multi-instance writes to one key, use object storage (S3/Azure Blob) with etag preconditions or a DB row — not shared filesystem without locking.

```csharp
string dir = Path.GetDirectoryName(finalPath)!;
string tempPath = Path.Combine(dir, $".{Guid.NewGuid():N}.tmp");
try
{
    await using (var temp = File.Create(tempPath))
        await upload.CopyToAsync(temp, ct);

    File.Move(tempPath, finalPath, overwrite: true);
    tempPath = null; // success — do not delete in finally
}
finally
{
    if (tempPath is not null && File.Exists(tempPath))
        File.Delete(tempPath);
}
```

**Production takeaway:** Temp-file staging is a production pattern only when cleanup, same-directory rename, and writable volume paths are handled — Karat stacks failure cleanup with container filesystem constraints.

---

#### Q3. When would you prefer `FileInfo` over repeated `File.*` static calls on the same path?

(R) A nightly cleanup job removes old workspace folders. Review:

```csharp
public void PurgeWorkspace(string workspaceRoot)
{
    foreach (string file in Directory.GetFiles(workspaceRoot, "*", SearchOption.AllDirectories))
    {
        File.SetAttributes(file, FileAttributes.Normal);
        File.Delete(file);
    }

    Directory.Delete(workspaceRoot, recursive: false);
}
```

Locally it works on small trees; in production it throws `IOException` on non-empty directories or `UnauthorizedAccessException` on hidden/system files. What is wrong with this approach, and what should you use instead?

**Answer:** Manual file-by-file deletion before a non-recursive `Directory.Delete` is slower, still fails on nested subdirectories, and fights read-only/hidden attributes — while leaving the tree inconsistent if any step throws mid-loop. The API already supports recursive delete in one call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API misuse | `Directory.Delete(..., recursive: false)` after only deleting **files** | Subfolders remain → `IOException: directory not empty` |
| Scalability | `GetFiles(..., AllDirectories)` loads entire tree into memory | Large workspaces → memory pressure; long lock while iterating |
| Reliability | Partial loop then exception | Some files deleted, folder half-purged — harder to retry idempotently |
| Permissions | `SetAttributes(Normal)` on every file | May still fail on locked files (open handles) or ACL/UAC denied paths |

**Fix (priority order):**

1. Use `Directory.Delete(workspaceRoot, recursive: true)` — one call removes files and nested folders (see **Program.cs** Section 9).
2. Before delete, ensure no open `FileStream`/`StreamReader` handles — dispose all streams first (Section 7 — sharing violation on Windows).
3. For large trees, prefer `DirectoryInfo.EnumerateFiles` with lazy enumeration if you must pre-process, but still finish with recursive delete — do not hand-roll tree walking unless you need selective retention.
4. On permission errors, fix ACLs or run under a service account with rights to the data directory — attribute clearing is not a substitute for proper permissions in prod.
5. Wrap in retry for transient sharing violations if antivirus/indexer holds brief locks.

**Production takeaway:** `Directory.Delete` without `recursive: true` on a non-empty folder is a common tutorial pitfall scaled to production — Karat expects you to know when recursive delete is correct and when open handles block it.

---

#### Q4. How do `Directory.GetFiles`, `Directory.GetDirectories`, and their `Enumerate*` counterparts differ in memory behavior?

(P) A multi-process log aggregator appends audit lines from several worker threads. One worker uses `File.AppendAllText`; another opens with default sharing:

```csharp
// Worker A
File.AppendAllText(logPath, line + Environment.NewLine);

// Worker B
using var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write);
using var writer = new StreamWriter(fs);
writer.WriteLine(line);
```

Under load you see `IOException: sharing violation` and occasionally interleaved garbage bytes. Explain `FileShare` behavior here and show a production-safe append pattern.

**Answer:** Default `FileStream` constructors use `FileShare.Read`, which excludes other writers — concurrent appenders block each other with sharing violations. Even when opens succeed, unsynchronized multi-writer appends interleave bytes at the OS level without line atomicity.

- `File.AppendAllText` opens, appends, and closes per call — high overhead and still races with other writers using incompatible share flags.
- For multiple writers on one file, open with `FileShare.ReadWrite` so other handles can coexist: `new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)`.
- Line atomicity is **not** guaranteed by `FileShare` — two threads can still interleave mid-line; use a `Mutex`/`SemaphoreSlim` named by path, a single dedicated writer thread/channel, or one process owning the log file.
- Better production pattern: append to stdout and let the platform aggregate (Kubernetes logging, Azure Monitor), or write per-process log files and merge — avoid many writers to one file on Windows especially.
- If you must share one file, wrap append in a process-wide lock and flush after each line; consider `StreamWriter` with `AutoFlush = true`.

```csharp
private static readonly SemaphoreSlim _logLock = new(1, 1);

await _logLock.WaitAsync(ct);
try
{
    await File.AppendAllTextAsync(logPath, line + Environment.NewLine, ct);
}
finally
{
    _logLock.Release();
}
```

**Production takeaway:** Sharing violations mean incompatible `FileShare` flags or an undisposed handle — Karat ties **Program.cs** Section 7 (open handles block delete/write) to concurrent append design, not just "use a lock" memorization.

---

#### Q5. What does `Directory.CreateDirectory` do when intermediate folders already exist?

(P) An export job stages files under `%TEMP%` on Windows, then calls `File.Move(source, dest)` into a network share. On developer laptops it works; in Azure App Service (Linux) and when crossing drive letters it fails with `IOException` or leaves duplicate files. What is happening at the OS level, and what pattern replaces naive `File.Move`?

**Answer:** `File.Move` is only a cheap atomic rename when source and destination are on the **same file system/volume**. Cross-volume or temp-to-network-share moves degrade to copy-then-delete — slow, non-atomic, and vulnerable to partial failure — and Linux container temp paths often live on a different mount than persisted data volumes.

- Windows: moving from `C:\Users\...\Temp` to `D:\` or `\\server\share` triggers copy+delete, not rename — crash after copy leaves both files or neither in expected state.
- Linux containers: `/tmp` may be tmpfs while `/app/data` is a mounted volume — `File.Move` cannot rename across mounts; errno `EXDEV` → .NET wraps as `IOException`.
- Hidden cost: large files copied twice consume disk and time; antivirus on network paths adds locks.
- Production pattern: stage temp file in the **destination directory** (hidden `.part` suffix), fsync if required, then same-directory `File.Move` to final name — atomic replace on same volume.
- For cross-machine delivery, skip filesystem move entirely — stream to blob storage (S3/Azure Blob) with server-side commit, or use a message queue with object key — not SMB paths from app servers.
- Use `Path.GetPathRoot` or compare `Directory.GetDirectoryRoot` of source and dest in diagnostics; if roots differ, plan copy+verify+delete explicitly with checksum validation.

**Production takeaway:** **Program.cs** Section 3 notes Move is "atomic rename on same volume" — Karat tests whether you apply that caveat when `%TEMP%` and upload folders diverge in cloud deploys.

---

#### Q6. What is the difference between `File.Copy` with `overwrite: false` vs `overwrite: true`, and what exception indicates a conflict?

(M) An ASP.NET Core endpoint reads a 200 MB CSV from disk on every request:

```csharp
app.MapGet("/reports/{id}", (string id, IReportStore store) =>
{
    string path = store.GetPath(id);
    if (!File.Exists(path))
        return Results.NotFound();

    string csv = File.ReadAllText(path);
    return Results.Content(csv, "text/csv");
});
```

Latency spikes under concurrent traffic and thread-pool queue depth grows, even though CPU stays low. What mechanism is blocking, and what file APIs would you use instead?

**Answer:** `File.ReadAllText` synchronously reads the entire 200 MB into a single `string` on a thread-pool thread — blocking async I/O throughput and allocating a huge LOH object — so concurrent requests queue behind blocked threads even though the work is I/O-bound.

- The minimal API delegate is synchronous; each request ties up a thread for the full disk read — classic thread-pool starvation under load (same class of problem as `.Result` on async I/O).
- `ReadAllText` doubles memory (file bytes + UTF-16 string) — 200 MB file can mean 400 MB+ per request peak.
- Prefer `return Results.File(path, "text/csv", enableRangeProcessing: true)` — streams from disk with `SendFileAsync` / efficient OS sendfile where available, no full buffering in managed memory.
- If transformation is required: `async Task<IResult>` with `await File.ReadAllTextAsync(path, ct)` or better `File.OpenRead` + `StreamReader` / pipe through `Results.Stream`.
- Add caching (`IMemoryCache` with size limits), CDN, or object storage pre-signed URLs for large static exports — disk read per request does not scale.
- Pass `CancellationToken` from `HttpContext.RequestAborted` so clients disconnecting abort the read.

```csharp
app.MapGet("/reports/{id}", (string id, IReportStore store) =>
{
    string path = store.GetPath(id);
    return File.Exists(path)
        ? Results.File(path, "text/csv", fileDownloadName: $"{id}.csv", enableRangeProcessing: true)
        : Results.NotFound();
});
```

**Production takeaway:** Sync all-at-once file helpers from **Program.cs** Section 3 (`ReadAllText`) are fine for small demo files — in web apps they block the thread pool; Karat expects streaming async APIs for large I/O.

---

#### Q7. How does `File.Move` differ from copy-then-delete, and what happens to metadata and hard links?

(D) A containerized API creates per-request scratch directories under `Path.GetTempPath()` but never deletes them when handlers throw. Disk on the node fills over days; restarting the pod "fixes" it until the next deploy. Compare three cleanup strategies — `try/finally`, `IDisposable` workspace helper, and OS temp with periodic janitor — for production container deployments. What is your default and why?

**Answer:** Orphaned scratch dirs are a deployment lifecycle bug, not a filesystem API quirk — the default should be deterministic per-operation cleanup via an `IDisposable` workspace scoped to the request, with a periodic janitor as a safety net in long-lived pods.

| Strategy | Strengths | Weaknesses |
|---|---|---|
| **`try/finally` inline** | Simple; guaranteed on exit from one method | Easy to forget when logic branches across helpers; duplicated across endpoints |
| **`IDisposable` workspace (`using var ws = new TempWorkspace(...)`)** | Centralizes create/delete; composes with `await using`; testable | Requires discipline to always `using`; nested scopes must not double-delete |
| **OS temp + periodic janitor** | Catches leaks from third-party libs and crash kills; good backstop in K8s | Not sufficient alone — unbounded growth between sweeps fills emptyDir/volume; race if janitor deletes active dirs |

- Default: **`IDisposable`/`IAsyncDisposable` temp workspace** created at request entry, deleted in `Dispose` even on exceptions — matches **Program.cs** Main's clean-slate pattern (`Directory.Delete` before recreate) but scoped per operation.
- Implement janitor as secondary: delete directories under temp older than N hours **only if** naming includes GUID and heartbeat file — never blanket `Delete` on entire `GetTempPath()` while app runs.
- In containers, mount scratch space with size limits (`emptyDir` sizeLimit) so leaks fail fast instead of evicting neighbors; prefer streaming to blob storage over large local scratch.
- Log workspace path on creation at Debug level; metric `temp_workspace_bytes` for observability.
- Avoid relying on pod restart as cleanup policy — violates 12-factor; masks handler bugs.

**Production takeaway:** Karat uses container disk fill to test whether you connect **Program.cs** cleanup demos to request-scoped `using` and deployment volume limits — restart is not a cleanup strategy.

---

#### Q8. What is `File.Replace`, and when is it preferable to manual backup-and-overwrite?

_Answer not found._

---

#### Q9. How do you safely delete a directory tree using `Directory.Delete(path, recursive: true)`?

_Answer not found._

---

#### Q10. What file metadata can you read via `File` static methods vs `FileInfo` instance properties?

_Answer not found._

---

#### Q11. Explain `File.GetCreationTime`, `GetLastWriteTime`, and `GetLastAccessTime` — and their `Utc` variants.

_Answer not found._

---

#### Q12. How do you set creation, last-write, and last-access timestamps programmatically?

_Answer not found._

---

#### Q13. What are `FileAttributes` (ReadOnly, Hidden, System, Archive)? How do you read and modify them?

_Answer not found._

---

#### Q14. What is the difference between `File.Exists` and attempting to open a file that may be deleted concurrently?

_Answer not found._

---

#### Q15. What exceptions should you expect during file operations (`FileNotFoundException`, `DirectoryNotFoundException`, `IOException`, `UnauthorizedAccessException`)?

_Answer not found._

---

#### Q16. How does `File.AppendAllText` differ from opening with `FileMode.Append`?

_Answer not found._

---

#### Q17. When is `File.ReadAllBytes` / `File.WriteAllBytes` appropriate vs stream-based APIs?

_Answer not found._

---

#### Q18. Explain `File.Create`, `File.Open`, `File.OpenRead`, and `File.OpenWrite` — what modes and access do they imply?

_Answer not found._

---

#### Q19. How do you handle TOCTOU (time-of-check-time-of-use) races when checking existence before read/write?

_Answer not found._

---

#### Q20. What is the difference between deleting a file and clearing its contents while keeping the path?

_Answer not found._

---

### 02. StreamReader & StreamWriter

#### Q1. Explain the `Stream` base class hierarchy and where `StreamReader`/`StreamWriter` fit.

(R) A nightly audit job throws on bad rows and operators report the log file stays locked until the worker restarts. Review this helper:

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");

    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected — fix upstream feed.");

    writer.Dispose();
}
```

What keeps the file locked, and how do you fix it without losing the rejected row on disk?

**Answer:** When validation throws, `Dispose()` never runs, so the `StreamWriter` keeps the underlying file handle open — on Windows the log stays locked until GC finalizes the writer. Wrap the writer in `using` (or `try/finally`) so the handle is released even on the exception path; the invalid row is already on disk because `WriteLine` ran before the throw.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource lifetime | No `using` / `finally`; `Dispose()` only on happy path | File handle leak; "file in use" on next append |
| Ordering | Validate after write | Rejected rows still persisted — may be intended, but callers must know |
| Platform | Undisposed `StreamWriter` on Windows | Lock persists until process exit or finalizer — common ops incident |

**Fix (priority order):**

1. Use `using (var writer = new StreamWriter(logPath, append: true)) { … }` so dispose runs on all exit paths.
2. If invalid rows must not be written, validate **before** `WriteLine`, or write to a staging file and commit on success.
3. For long-lived services, prefer `await using StreamWriter` with async writes if the call chain is async end-to-end.
4. Monitor for handle leaks — repeated failures should not require worker restart to unlock the log.

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected — fix upstream feed.");

    using StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");
}
```

**Production takeaway:** Karat pairs exception flow with I/O cleanup — `StreamWriter` is not magic; undisposed writers are production file locks. See **Program.cs** Section 6 — `using` / `Dispose` and QUICK REFERENCE — "Forgetting using / Dispose → file locked until GC."

---

#### Q2. What is the difference between `File.ReadAllText`, `File.ReadAllLines`, and `File.ReadLines`?

(R) A CSV export looks correct on the developer's Windows machine but the first column header fails validation after deploy to Linux containers. Review write vs read:

```csharp
// Export service (built and tested on Windows)
using (var writer = new StreamWriter(exportPath, append: false, Encoding.Default))
{
    writer.WriteLine("id,name,amount");
    writer.WriteLine("1,Alpha,42.50");
}

// Import service (Linux container — default StreamReader ctor)
using (var reader = new StreamReader(importPath))
{
    string? header = reader.ReadLine();
    if (header != "id,name,amount")
        throw new InvalidDataException($"Unexpected header: '{header}'");
}
```

What fails in production, and how do you make the round-trip deterministic across OS boundaries?

**Answer:** `Encoding.Default` is the **system code page** — Windows-1252 on Windows, often UTF-8 on modern Linux — so bytes on disk differ by environment, and the reader's default detection may decode the same bytes differently than the writer encoded them. Pin both sides to explicit `Encoding.UTF8` (typically `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` for no BOM) and compare headers after `ReadLine()`, which already returns decoded characters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encoding | `Encoding.Default` on write | Different byte sequences per OS — mojibake or subtle header mismatch |
| Encoding | Implicit reader encoding on read | Guessing wrong code page — first line may include stray BOM or wrong chars |
| Contract | String equality on header | Fails even when visually "correct" in a GUI editor |

**Fix (priority order):**

1. Replace `Encoding.Default` with explicit UTF-8 on **both** writer and reader constructors.
2. Document encoding in the file format contract; reject files whose BOM/bytes do not match.
3. For CSV consumed by Excel on Windows, decide deliberately on UTF-8 BOM vs no BOM — do not rely on defaults.
4. Add an integration test that round-trips on Linux CI, not only on the developer's Windows box.

```csharp
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

using (var writer = new StreamWriter(exportPath, append: false, utf8))
    writer.WriteLine("id,name,amount");

using (var reader = new StreamReader(importPath, utf8))
{
    string? header = reader.ReadLine();
    // ...
}
```

**Production takeaway:** "Works on my machine" for text files is almost always an encoding default mismatch — Karat expects you to name `UTF8Encoding` and match reader/writer. See **Program.cs** Section 4 — pass the same `Encoding` to matching ctors.

---

#### Q3. Why can `File.ReadLines` hold a file lock until enumeration completes?

(R) A support dashboard calls this to show the tail of a customer log. Under load the worker process recycles with `OutOfMemoryException`. Review:

```csharp
public string LoadCustomerLogForSupport(string logPath)
{
    if (!File.Exists(logPath))
        return string.Empty;

    using StreamReader reader = new StreamReader(logPath);
    return reader.ReadToEnd();
}
```

Customer logs can exceed 10 GB. What is wrong, and what pattern replaces `ReadToEnd` for this use case?

**Answer:** `ReadToEnd()` allocates a single `string` for the entire remaining file — with 10 GB logs that forces a multi-gigabyte LOH allocation and typically terminates the process with `OutOfMemoryException`. Stream line-by-line with `ReadLine()` or `ReadLineAsync()`, seek to a tail window with `FileStream` + bounded `ReadBlock`, or use external tail tools — never materialize the whole file for a "show last lines" feature.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | `ReadToEnd()` on multi-GB file | `OutOfMemoryException`; worker recycle under concurrent support requests |
| API misuse | Support "tail" implemented as full load | Latency and memory scale with file size, not UI need |
| Scalability | Sync read of entire blob | Thread blocked for duration of huge I/O |

**Fix (priority order):**

1. For tail UI: open `FileStream` with `FileShare.ReadWrite`, seek near EOF, read last N KB/chars, split lines locally.
2. For scanning: `while ((line = await reader.ReadLineAsync(ct)) != null)` — bounded memory regardless of file size.
3. Cap response size returned to the dashboard (e.g., last 500 lines or 256 KB).
4. Move huge log analytics to indexed storage — files on disk are not a query engine.

```csharp
public async Task<IReadOnlyList<string>> ReadLastLinesAsync(string logPath, int maxLines, CancellationToken ct)
{
    var lines = new Queue<string>(maxLines);
    await using StreamReader reader = new StreamReader(logPath);
    while (await reader.ReadLineAsync(ct) is { } line)
    {
        if (lines.Count == maxLines) lines.Dequeue();
        lines.Enqueue(line);
    }
    return lines.ToArray();
}
```

**Production takeaway:** `ReadToEnd()` is for small files only — Karat uses log scale to test whether you know **Program.cs** Section 3 (`ReadLine` / `ReadBlock`) vs Section 7 (`File.ReadAllText` trap). Same mistake as `File.ReadAllText` on huge files.

---

#### Q4. How do `StreamReader.ReadLine`, `ReadToEnd`, and `ReadBlock` differ for large files?

(R) A long-running export writes a status file so another process can poll completion. Operators see `IN_PROGRESS` forever after a crash mid-run. Review:

```csharp
string statusPath = Path.Combine(outputDir, "export.status");
StreamWriter writer = new StreamWriter(statusPath, append: false);
writer.WriteLine("IN_PROGRESS");

RunHeavyExport(); // may take 20+ minutes; process sometimes killed by OOM killer

writer.WriteLine("COMPLETE");
writer.Dispose();

// Poller (separate process):
using StreamReader poller = new StreamReader(statusPath);
string lastLine = poller.ReadToEnd().TrimEnd().Split('\n').Last();
bool done = lastLine == "COMPLETE";
```

What causes false "stuck" exports and incomplete status files, and how do you harden write + detection?

**Answer:** `StreamWriter` buffers output — `IN_PROGRESS` may not hit disk until `Flush()` or `Dispose`, so a poller can see an empty or stale file early. If the process dies mid-export, `COMPLETE` is never written and the poller correctly sees stuck state, but the writer also lacks atomic replace semantics — partial flushes can leave truncated files. Use `AutoFlush` or explicit `Flush()` after status transitions, write-temp-then-`File.Move` for atomic status, and treat absence of `COMPLETE` plus process exit as failure with timeout alerting.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Buffering | No `Flush` / `AutoFlush` after `IN_PROGRESS` | Poller reads empty or old content; false negatives |
| Durability | Single file overwritten in place | Crash mid-write → truncated or blank status file |
| Detection | `ReadToEnd().Split('\n').Last()` on in-progress write | May read partial buffer; race with writer |
| Lifecycle | No `using` on writer if exception in `RunHeavyExport` | Handle leak + final status never written |

**Fix (priority order):**

1. Enable `writer.AutoFlush = true` or call `Flush()` immediately after each status line.
2. Write status to a temp file and atomically replace: `File.WriteAllText(temp, status); File.Move(temp, statusPath, overwrite: true);` — or use `StreamWriter` on temp then move.
3. Poller: check file length stability, last-write time, and explicit `FAILED`/`COMPLETE` tokens; add SLA timeout.
4. Wrap writer in `using` and set `FAILED` in `catch`/`finally` when export aborts.

```csharp
using StreamWriter writer = new StreamWriter(statusPath, append: false) { AutoFlush = true };
writer.WriteLine("IN_PROGRESS");
try
{
    RunHeavyExport();
    writer.WriteLine("COMPLETE");
}
catch
{
    writer.WriteLine("FAILED");
    throw;
}
```

**Production takeaway:** Karat stacks buffering + crash recovery — operators care about **observable** state on disk, not in-process buffers. See **Program.cs** Section 6 — `Flush`, `AutoFlush`, and dispose flushes remaining buffer.

---

#### Q5. What is the default encoding for `StreamReader` and `StreamWriter`, and why can that cause mojibake?

(R) A log tailer and a log writer run in the same app. The tailer intermittently throws `IOException: The process cannot access the file because it is being used by another process`. Review:

```csharp
public IEnumerable<string> TailLines(string logPath)
{
    using FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read);
    using StreamReader reader = new StreamReader(fs);

    while (!reader.EndOfStream)
    {
        string? line = reader.ReadLine();
        if (line != null)
            yield return line;
    }
}

// Elsewhere, on another thread:
using StreamWriter writer = new StreamWriter(logPath, append: true);
writer.WriteLine($"{DateTime.UtcNow:o} INFO  heartbeat");
```

What sharing rule is missing, and why does `StreamReader`/`StreamWriter` path constructors hide it?

**Answer:** Opening `FileStream` with default `FileShare.Read` grants exclusive write access — a concurrent `StreamWriter` on the same path cannot open for append. Open the tailer's stream with `FileShare.ReadWrite` (and usually `FileMode.Open`, `FileAccess.Read`) so writers can append while you read. Path-based `StreamReader`/`StreamWriter` ctors create their own `FileStream` with sharing defaults you do not see — for tail-follow scenarios, construct `FileStream` explicitly, then wrap with `leaveOpen: true`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| File sharing | Default `FileShare.Read` on read stream | `IOException` when appender opens same log |
| API visibility | `new StreamReader(path)` hides share flags | Developers miss sharing until production concurrency |
| Iterator | `yield return` holds stream open for enumeration lifetime | Writer blocked for entire foreach duration |

**Fix (priority order):**

1. Tailer: `new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)`.
2. Writer: `new StreamWriter(new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite), appendEncoding) { AutoFlush = true }` or equivalent append pattern.
3. For live tail, re-open or seek-from-end patterns with periodic reopen on `IOException` — logs rotate.
4. Prefer structured logging sinks (Serilog file sink with shared flag) instead of hand-rolled tail+append.

```csharp
using FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
using StreamReader reader = new StreamReader(fs, leaveOpen: false);
```

**Production takeaway:** Text wrappers do not remove OS file-lock rules — Karat tests whether you know when to bypass path ctors and configure `FileShare`. Preview **Program.cs** Section 2d / 3f — `FileStream` then `StreamReader`/`StreamWriter` chain.

---

#### Q6. How do you specify `Encoding.UTF8`, UTF-8 with BOM, and legacy encodings (`Encoding.GetEncoding`)?

(P) An ASP.NET Core hosted service ingests a growing feed file every few seconds. A developer keeps sync I/O "because the file is local":

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using StreamReader reader = new StreamReader(_feedPath);

    while (!stoppingToken.IsCancellationRequested)
    {
        string? line = reader.ReadLine(); // blocks thread pool thread
        if (line == null)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            continue;
        }

        await _processor.HandleLineAsync(line, stoppingToken);
    }
}
```

What breaks under hosting pressure, and what is the production-grade read loop?

**Answer:** `ReadLine()` is synchronous — each call blocks a thread pool thread while waiting on disk I/O, which defeats the async hosting model and can contribute to thread-pool starvation when many background services do the same. Use `ReadLineAsync(stoppingToken)` (or `WaitToReadAsync` patterns on pipes) inside an async loop, combine with `FileShare.ReadWrite` if producers append, and reopen or track position when reaching EOF on a growing file.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | Sync `ReadLine()` in async host | Thread pool blocked during I/O waits |
| EOF handling | `null` line then `Delay` on static reader | Misses new lines appending at EOF unless reposition/reopen |
| Cancellation | Sync read ignores `stoppingToken` during block | Slow shutdown under load |

**Fix (priority order):**

1. Replace with `await reader.ReadLineAsync(stoppingToken)` in the loop.
2. When at EOF on a growing feed, flush writer side, optionally reopen file or track `_lastPosition` with `FileStream.Position`.
3. Mark the hosted service async end-to-end; avoid `.Result` on any related tasks.
4. Add metrics for lag (lines behind) and backoff when file is temporarily locked.

```csharp
await using StreamReader reader = new StreamReader(_feedPath);

while (!stoppingToken.IsCancellationRequested)
{
    string? line = await reader.ReadLineAsync(stoppingToken);
    if (line is null)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        continue;
    }

    await _processor.HandleLineAsync(line, stoppingToken);
}
```

**Production takeaway:** Local disk does not make I/O free — sync-over-async in hosted services is the same Karat trap as `.Result` in controllers. Prefer async stream APIs even for file reads.

---

#### Q7. What does `StreamReader.DetectEncodingFromByteOrderMarks` control?

(R) A tool rewrites the first line of a config file in place, then reads the remainder. After a refactor it throws `ObjectDisposedException`. Review:

```csharp
public void PatchConfigHeader(string path, string newHeader)
{
    using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
    using StreamWriter writer = new StreamWriter(fs, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
    using StreamReader reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);

    writer.WriteLine(newHeader);
    writer.Flush();

    fs.Seek(0, SeekOrigin.Begin);
    string remainder = reader.ReadToEnd();
}
```

Which dispose/ownership choices are wrong, and what is the correct pattern when wrapping the same `FileStream`?

**Answer:** `StreamReader` is constructed with `leaveOpen: false` (the default), so disposing the reader **closes the shared `FileStream`** when the `using` block ends — before `Seek`/`ReadToEnd` if ordering were wrong, and any later use throws `ObjectDisposedException`. Both reader and writer must use `leaveOpen: true` when sharing one stream; dispose order should flush the writer, then dispose reader, then writer, then the stream — or avoid dual wrappers on one stream and read/write in separate phases with explicit positioning.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Ownership | `StreamReader(..., leaveOpen: false)` on shared `fs` | Reader dispose closes `fs` for everyone |
| Concurrency | Simultaneous reader/writer on same stream without clear protocol | Undefined buffering; corrupted reads |
| Design | In-place header patch via read/write same stream | Easy to truncate file if rewrite shorter than original |

**Fix (priority order):**

1. Set `leaveOpen: true` on **both** `StreamWriter` and `StreamReader`; dispose `fs` last explicitly.
2. Safer: read full content first, patch in memory, write to temp file, atomic replace — avoids length mismatch corrupting tail.
3. After writing header, `writer.Flush()` before reading; reset position with `fs.Seek` and optionally discard reader buffer (new reader instance).
4. Document that `StreamWriter` path ctor owns the stream unless you pass your own `FileStream`.

```csharp
using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8, leaveOpen: true))
{
    writer.WriteLine(newHeader);
    writer.Flush();
}
fs.Seek(0, SeekOrigin.Begin);
using (StreamReader reader = new StreamReader(fs, Encoding.UTF8, leaveOpen: true))
{
    _ = reader.ReadLine();
    string remainder = reader.ReadToEnd();
}
```

**Production takeaway:** Default `leaveOpen: false` means disposing the text wrapper closes the base stream — Karat tests layered I/O ownership called out in **Program.cs** Section 2d/3f FileStream chains.

---

#### Q8. Explain async read/write methods on `StreamReader`/`StreamWriter` (`ReadLineAsync`, `WriteLineAsync`, `ReadToEndAsync`).

(M) A cross-platform app parses `.env`-style files written on mixed developer machines (Windows CRLF, macOS/Linux LF). Review ingestion:

```csharp
public Dictionary<string, string> ParseEnvFile(string path)
{
    var map = new Dictionary<string, string>();
    using StreamReader reader = new StreamReader(path);

    string content = reader.ReadToEnd();
    foreach (string rawLine in content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
    {
        int eq = rawLine.IndexOf('=');
        if (eq <= 0) continue;
        string key = rawLine[..eq].Trim();
        string value = rawLine[(eq + 1)..].Trim();
        map[key] = value;
    }
    return map;
}
```

What breaks when files use CRLF or when keys are compared across environments, and how should line-based parsing use `StreamReader` instead?

**Answer:** Splitting on `'\n'` alone leaves a trailing `'\r'` on keys when the file uses CRLF — `map["HOST"]` misses lookups for `"HOST\r"`. `ReadToEnd()` also reintroduces the large-file memory trap. Loop with `ReadLine()`, which strips platform newlines (`\r\n` or `\n`) uniformly, trim keys/values defensively, and use ordinal key comparison; skip comments with `#` per line instead of splitting the whole file.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Newlines | `Split('\n')` on CRLF content | Keys include `\r` — silent config misses in Linux-deployed apps |
| Memory | `ReadToEnd()` for config | Unbounded allocation if `.env` grows or includes generated blocks |
| Parsing | `RemoveEmptyEntries` | Skips intentional blank lines vs comments — may hide format errors |

**Fix (priority order):**

1. Replace bulk split with `while ((line = reader.ReadLine()) != null)` — `ReadLine` removes `\r\n`/`\n` per **Program.cs** Section 3.
2. `key = key.Trim('\r', ' ', '\t')` defensively if content comes from external tools.
3. Use `StringComparer.OrdinalIgnoreCase` only if spec requires case-insensitivity — document choice.
4. For deployment, normalize line endings in repo via `.gitattributes` — but runtime parsing must still tolerate CRLF.

```csharp
while (reader.ReadLine() is { } line)
{
    line = line.Trim();
    if (line.Length == 0 || line.StartsWith('#')) continue;
    int eq = line.IndexOf('=');
    if (eq <= 0) continue;
    map[line[..eq].Trim()] = line[(eq + 1)..].Trim();
}
```

**Production takeaway:** Cross-platform text bugs often show up as `\r`-poisoned keys, not mojibake — Karat expects `ReadLine` semantics vs manual split. See **Program.cs** QUICK REFERENCE — prefer line loop over whole-file helpers for scalable parsing.

---

#### Q9. What is `StreamWriter.AutoFlush`, and when should you call `Flush()` explicitly?

_Answer not found._

---

#### Q10. How do you append text to an existing file with `StreamWriter` (constructor overload with `append: true`)?

_Answer not found._

---

#### Q11. What happens if you forget to dispose a `StreamWriter` — especially on Windows file locking?

_Answer not found._

---

#### Q12. Can you use `StreamReader`/`StreamWriter` with non-file streams (memory, network)? Give examples.

_Answer not found._

---

#### Q13. What is the difference between `using` blocks and C# 8 `using` declarations for stream cleanup?

_Answer not found._

---

#### Q14. How do you read a file line-by-line without loading it entirely into memory?

_Answer not found._

---

#### Q15. What is `TextReader`/`TextWriter`, and why do APIs often accept these abstractions?

_Answer not found._

---

### 03. FileStream & Binary Files

#### Q1. What is the difference between `File`, `Stream`, and `FileStream`?

(R) A telemetry service reads a fixed 4-byte file signature from `signature.bin`. In production, short files produce garbage signatures without throwing. Review the reader:

```csharp
public static string ReadFileSignature(string path)
{
    byte[] buffer = new byte[4];

    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    stream.Read(buffer, 0, buffer.Length); // signature must be exactly 4 bytes

    return Encoding.ASCII.GetString(buffer);
}
```

What is wrong, and how would you harden this for truncated or partially written files?

**Answer:** `FileStream.Read` may return fewer bytes than requested — especially near EOF or on a file still being written — but the code ignores the return value and decodes the entire buffer, padding with `\0` or stale bytes. You must loop until you have 4 bytes or confirm EOF, and treat short reads as corrupt input.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Return value of `Read` ignored | Partial buffer decoded as full signature |
| Runtime | No EOF / short-file handling | Silent garbage strings instead of explicit failure |
| Concurrency | `FileShare.Read` while writer may still flush | Reader sees pre-flush or truncated file |

**Fix (priority order):**

1. Loop reads until `totalRead == 4` or `Read` returns 0 — throw `InvalidDataException` if fewer than 4 bytes after EOF.
2. Prefer `BinaryReader.ReadBytes(4)` when you need an exact count — it throws `EndOfStreamException` on short input (see **Program.cs** Section 9).
3. If another process writes the file, coordinate with `FileShare.ReadWrite` on the writer and validate magic before trusting content.
4. Decode only the bytes actually read: `Encoding.ASCII.GetString(buffer, 0, totalRead)`.

```csharp
int totalRead = 0;
while (totalRead < buffer.Length)
{
    int n = stream.Read(buffer, totalRead, buffer.Length - totalRead);
    if (n == 0) break;
    totalRead += n;
}
if (totalRead != buffer.Length)
    throw new InvalidDataException($"Expected 4 signature bytes, got {totalRead}.");
```

**Production takeaway:** Karat embeds the **Program.cs** Section 5 rule — always check bytes returned — inside realistic signature-reading code. A single `Read` call is not a contract for a full buffer.

---

#### Q2. Explain `FileMode` (`CreateNew`, `Create`, `Open`, `OpenOrCreate`, `Truncate`, `Append`) — when use each?

(R) A background job appends binary audit records while a dashboard process tries to read the same file. The writer opens like this; the reader gets `IOException: The process cannot access the file`:

```csharp
// Writer (audit service)
using var stream = new FileStream(
    auditPath, FileMode.Append, FileAccess.Write, FileShare.None);

// Reader (dashboard — runs concurrently)
using var readStream = new FileStream(
    auditPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
```

What locking mismatch causes the failure, and what `FileShare` flags should each side use?

**Answer:** The writer holds an exclusive lock with `FileShare.None`, so no other process can open the file — even for read — until the handle is disposed. For concurrent append + read, the writer must allow shared read access (`FileShare.Read`) while the reader opens with `FileShare.ReadWrite` so both can coexist.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Locking | Writer uses `FileShare.None` | Exclusive lock blocks dashboard reader |
| Design | Long-lived writer handle (service loop) | File stays locked for entire process lifetime |
| Correctness | Reader assumes `ReadWrite` share fixes writer side | Share flags must match on **both** open calls |

**Fix (priority order):**

1. Change writer to `FileShare.Read` (or `FileShare.ReadWrite` if multiple writers are coordinated): `new FileStream(auditPath, FileMode.Append, FileAccess.Write, FileShare.Read)`.
2. Keep reader as `FileMode.Open, FileAccess.Read, FileShare.ReadWrite`.
3. Ensure writer `Flush()` / dispose runs periodically if readers need fresh tail bytes — buffered appends may not be visible until flush.
4. For high-concurrency append, consider one writer process or a queue instead of many exclusive handles.

**Production takeaway:** `FileShare` is negotiated at open time — the most restrictive combination wins. **Program.cs** Section 4 shows `FileShare.None` for exclusive writes and `FileShare.Read` for concurrent readers; production append+tail-read patterns need the writer to grant share.

---

#### Q3. Explain `FileAccess` (`Read`, `Write`, `ReadWrite`) and `FileShare` (`None`, `Read`, `Write`, `ReadWrite`, `Delete`).

(R) A teammate ports `inventory.bin` readers from another language and swaps field order on one record type. The file opens fine but prices and names are nonsense after the first record. Review:

```csharp
for (int i = 0; i < recordCount; i++)
{
    int id = reader.ReadInt32();
    double price = reader.ReadDouble();   // was written as int32 + string + bool
    string name = reader.ReadString();
    bool inStock = reader.ReadBoolean();
    results[i] = new ProductRecord(id, name, price, inStock);
}
```

What breaks, why does corruption spread to later records, and how do you detect or recover safely?

**Answer:** Binary files have no field names — the reader consumes bytes in strict write order. Reading `double` where a length-prefixed `string` was written misaligns the stream pointer, so every subsequent field and record parses garbage until `EndOfStreamException` or absurd values appear.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Read order ≠ write order (`id`, `name`, `price`, `bool`) | First record wrong; cursor permanently offset |
| Serialization | Treating on-disk layout like struct memory layout | Language ports assume field order matches CLR struct |
| Recovery | No per-record checksum or length guard | One mismatch corrupts entire remainder of file |

**Fix (priority order):**

1. Restore exact write order from **Program.cs** `BinaryInventoryCodec`: `ReadInt32` → `ReadString` → `ReadDouble` → `ReadBoolean`.
2. Validate magic header and `recordCount` before the loop; cap `recordCount` against `stream.Length` to reject absurd headers (corrupt/truncated files).
3. Add optional per-record length prefix or CRC if you need partial recovery — without it, fail fast on first parse anomaly.
4. Never use `StructLayout` / `Marshal.StructureToPtr` interchangeably with `BinaryWriter` unless you explicitly define packing and endianness.

**Production takeaway:** BinaryReader mismatches are not localized bugs — one wrong primitive shifts the cursor for all following data. Magic bytes (**Program.cs** Section 3) catch wrong file types; they do not catch wrong field order within the right file.

---

#### Q4. Why does default `FileShare.None` cause sharing violations when another process needs read access?

(R) A log-rotation utility reads the last 8 bytes of a growing file to verify a footer magic. It intermittently returns wrong bytes under load. Review:

```csharp
public static byte[] ReadFooter(string path)
{
    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    byte[] footer = new byte[8];
    stream.Seek(0, SeekOrigin.End);           // jump to end
    stream.Read(footer, 0, footer.Length);
    return footer;
}
```

What Position/Seek mistakes are here, and what else should you validate before trusting the footer?

**Answer:** `Seek(0, SeekOrigin.End)` moves to EOF — **after** the last byte — not to the start of an 8-byte footer. The subsequent `Read` then pulls bytes from beyond the file (zeros/partial read) or fails silently depending on length. You need a negative offset from the end, e.g. `Seek(-8, SeekOrigin.End)`, and must handle files shorter than 8 bytes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Seek(0, End)` ≠ "last N bytes" | Reads past EOF; footer never matches |
| Edge case | No `Length < 8` guard | Short files produce partial/garbage footers |
| Concurrency | `FileShare.Read` while appender grows file | Footer position shifts between Seek and Read |

**Fix (priority order):**

1. Replace with `stream.Seek(-footer.Length, SeekOrigin.End)` — pattern from **Program.cs** `ReadLastTwoBytes` (`Seek(-2, SeekOrigin.End)`).
2. If `stream.Length < footer.Length`, throw or return a explicit failure — do not read.
3. Check `Read` return value or use `ReadBytes(8)` when you require exactly 8 bytes.
4. If the file is actively appended, re-read or use a stable snapshot (copy, or open with coordinated share + retry).

**Production takeaway:** `SeekOrigin.End` offsets are relative to EOF — zero means "after the last byte," not "the last byte." Karat tests whether you can translate "read tail" into signed Seek math.

---

#### Q5. What are `FileStream.Position`, `Seek`, and `Length` — and when is seeking valid?

(P) Your .NET service writes `metrics.bin` consumed by a Linux C tool on big-endian ARM. A developer uses default `BinaryWriter`/`BinaryReader` for `int` and `double` fields. Locally on x64 Windows everything works; in staging the C tool reads garbage. What is the root cause, and how do you design a cross-platform binary layout?

**Answer:** `BinaryWriter`/`BinaryReader` use the platform's native little-endian layout for multi-byte primitives on typical x64 Windows — the ARM C consumer expects big-endian (network) byte order, so numeric fields decode incorrectly even when field order and sizes match.

- Document an explicit wire format: field order, fixed sizes, and **endianness** (usually big-endian for cross-language files).
- Write primitives with explicit byte reversal (`BinaryPrimitives.WriteInt32BigEndian`) or a known serializer (Protocol Buffers, MessagePack) instead of assuming CLR defaults match C `struct` memory.
- Do not confuse **struct memory layout** (`StructLayout`, padding, alignment) with **BinaryWriter** layout — they are unrelated unless you carefully marshal.
- Add a version byte and magic header; integration-test round-trip with the C reader in CI on both endian platforms.

**Production takeaway:** "Same language on dev machine" hides endianness and padding issues until the first cross-platform consumer. **Program.cs** notes little-endian on typical x64 — that is an assumption, not a portable protocol.

---

#### Q6. Explain `SeekOrigin` (`Begin`, `Current`, `End`) with a concrete read-modify-write scenario.

(D) A data pipeline must scan a 60 GB append-only binary archive for records matching a key — random access by fixed record index, not full sequential parse every time. A junior proposes `FileStream` + `Seek` per lookup; a senior suggests `MemoryMappedFile`. What are the trade-offs, and when would you still choose streaming?

**Answer:** `MemoryMappedFile` maps file pages into virtual memory — excellent for repeated random access on large read-mostly files without loading 60 GB into a `byte[]`, and the OS caches hot regions efficiently. Pure `FileStream` + `Seek` per lookup works but pays more syscall overhead and does not leverage page cache as naturally for scattered access patterns.

- **Memory-mapped pros:** Fast indexed jumps when records are fixed-size or you maintain an offset index; multiple processes can share mapped views read-only; no manual buffer management for random reads.
- **Memory-mapped cons:** Less ideal for concurrent **writes** / append while mapped; address-space limits on 32-bit; careful handling of torn reads if writer appends without coordination; not a drop-in for variable-length records without an index.
- **Streaming pros:** Simpler lifecycle with `using`; natural for sequential export/import; better when records are variable-length and you must parse forward anyway; async `ReadAsync` pipelines for ETL.
- **Hybrid:** Build a sidecar index file (offset table) + mmap or seek; for one-pass full scan, sequential `FileStream` may be faster than millions of random seeks.

**Production takeaway:** Karat tests design judgment — mmap is not "always faster," but for **large, repeatedly indexed, read-heavy** binary archives it often beats naive per-lookup `Seek` on spinning disks and NVMe alike when record boundaries are known.

---

#### Q7. What happens if you seek on a non-seekable stream (e.g., some network streams)?

(R) An export worker writes large binary batches with async I/O, then signals a downstream processor via a message queue. The processor often reads zero-length or incomplete files. Review:

```csharp
public async Task ExportBatchAsync(string path, byte[] payload, CancellationToken ct)
{
    await using FileStream stream = new FileStream(
        path, FileMode.Create, FileAccess.Write, FileShare.Read);

    await stream.WriteAsync(payload, ct);
    // message published immediately after WriteAsync returns
    await _queue.PublishAsync(new BatchReadyMessage(path), ct);
}
```

What async/flush timing issue causes incomplete reads, and how do you fix it before publishing?

**Answer:** `WriteAsync` returning means data reached the `FileStream` buffer — not necessarily the OS disk cache or a stable on-disk length visible to another process. Publishing immediately races the consumer, which may open the file before flush/dispose completes and see zero or partial content.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No `FlushAsync` / dispose before signal | Consumer reads truncated file |
| Timing | Message queue is faster than disk visibility | Intermittent "empty file" failures |
| API | `FileShare.Read` allows concurrent open | Reader succeeds but gets stale length |

**Fix (priority order):**

1. `await stream.FlushAsync(ct)` before publishing — mirrors **Program.cs** Section 7 (`writer.Flush(); stream.Flush()`).
2. Prefer `await using` scope: dispose (close handle) before enqueue so OS metadata reflects final length.
3. Optionally write to a temp path and atomically `File.Move` to the final path, then publish — consumer never sees a half-written target.
4. Consumer should retry with backoff on short reads, but the producer must not rely on that alone.

```csharp
await stream.WriteAsync(payload, ct);
await stream.FlushAsync(ct);
// await using dispose runs here — then publish
await _queue.PublishAsync(new BatchReadyMessage(path), ct);
```

**Production takeaway:** Async I/O does not remove flush semantics — **Program.cs** warns that `FileInfo.Length` may be stale until flush/dispose. Queue-based pipelines need flush + atomic rename, not just `WriteAsync`.

---

#### Q8. What is the difference between `FileStream.Read`/`Write` and `ReadAsync`/`WriteAsync`?

(R) A cache service tries to wipe and rewrite `cache.bin` in one handle. It throws at runtime despite the path existing. Review both open attempts:

```csharp
// Attempt A — "open existing and overwrite first byte"
using var readOnly = new FileStream(cachePath, FileMode.Open, FileAccess.Read);
readOnly.WriteByte(0xFF);

// Attempt B — "create fresh file but only pass Read access"
using var creator = new FileStream(cachePath, FileMode.Create, FileAccess.Read);
```

What `FileMode`/`FileAccess` mismatches cause each failure, and what is the correct combination for in-place rewrite (**Program.cs** Section 10 — Truncate pattern)?

**Answer:** `FileAccess` must authorize every operation you perform — `Read` forbids `WriteByte`, and `FileMode.Create` with `FileAccess.Read` is an invalid combination that throws `ArgumentException` at construction. For in-place rewrite, open with write-capable access and use `FileMode.Truncate` or `ReadWrite` + explicit length reset.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Attempt A: `FileAccess.Read` + `WriteByte` | `NotSupportedException` at first write |
| API contract | Attempt B: `FileMode.Create` + `FileAccess.Read` | `ArgumentException` — Create requires Write or ReadWrite |
| Design | Using `Open` + write when file should be cleared first | Old bytes remain if you only overwrite first byte without truncating |

**Fix (priority order):**

1. For wipe-and-rewrite in place: `new FileStream(cachePath, FileMode.Truncate, FileAccess.Write, FileShare.None)` — **Program.cs** Section 10 sets length to 0 then writes.
2. If you need read-then-write in one session: `FileMode.OpenOrCreate` or `Open` with `FileAccess.ReadWrite`, then `SetLength(0)` or `Truncate` semantics before writing.
3. Match `FileMode` to intent: `Create` truncates existing path; `Append` seeks to end; do not pair write modes with read-only access.
4. Always pair with `using` / dispose so locks release after rewrite.

```csharp
using var stream = new FileStream(
    cachePath, FileMode.Truncate, FileAccess.Write, FileShare.None);
stream.WriteByte(0xFF);
stream.Flush();
```

**Production takeaway:** `FileMode` chooses **how the OS opens the path**; `FileAccess` gates **what this handle may do** — Karat stacks both in one snippet to see if you diagnose constructor vs first-write failures separately.

---

#### Q9. What do `BinaryReader` and `BinaryWriter` add over raw `FileStream` byte operations?

_Answer not found._

---

#### Q10. How does `BinaryReader` handle endianness and primitive types (`ReadInt32`, `ReadDouble`, `ReadString`)?

_Answer not found._

---

#### Q11. What is the on-disk format of `BinaryWriter.Write(string)` — and why does it matter for cross-platform files?

_Answer not found._

---

#### Q12. What is the difference between text and binary file handling in C#?

_Answer not found._

---

#### Q13. When should you use `MemoryStream` instead of `FileStream`?

_Answer not found._

---

#### Q14. What is buffered I/O, and how do `FileStream` buffer size options affect performance?

_Answer not found._

---

#### Q15. How do you read a fixed header followed by variable-length records from a binary file?

_Answer not found._

---

#### Q16. What is a file signature (magic bytes), and how do you validate one without trusting the extension?

_Answer not found._

---

### 04. Path & Environment Classes

#### Q1. What is the `Path` class, and why should you never hard-code `\` or `/` separators?

(R) A report exporter works on Windows dev machines but fails on Linux CI with "Could not find a part of the path." Review this path builder:

```csharp
public string BuildExportPath(string customerId, string fileName)
{
    string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    return baseDir + "\\Reports\\" + customerId + "\\" + fileName;
}

// Called from a nightly job:
var path = BuildExportPath("CUST-42", "summary.csv");
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
await File.WriteAllTextAsync(path, csvContent);
```

What is wrong, and how do you fix it for cross-platform deployment?

**Answer:** The method hard-codes Windows backslashes and assumes a user Documents folder exists on a headless CI agent — on Linux the concatenated path is invalid and `MyDocuments` may be empty or unsuitable for a server job.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cross-platform | `"\\"` string concatenation | Linux treats `\` as a valid filename character, not a separator — path does not resolve |
| Design | `SpecialFolder.MyDocuments` on a server/CI worker | No interactive user profile; base path may be empty or wrong |
| Maintainability | Manual join instead of `Path.Combine` | Every new segment repeats the separator mistake |

**Fix (priority order):**

1. Replace concatenation with `Path.Combine(baseDir, "Reports", customerId, fileName)`.
2. Do not use `MyDocuments` for server exports — read an configured output root from `IConfiguration` / environment variable (e.g. `/var/app/exports` or a mounted volume).
3. Validate `baseDir` is non-empty before `CreateDirectory`; fail fast with a clear configuration error in CI.
4. Sanitize `customerId` and `fileName` — reject path separators and `..` segments before combining.

```csharp
public string BuildExportPath(string outputRoot, string customerId, string fileName)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
    return Path.Combine(outputRoot, "Reports", customerId, fileName);
}
```

**Production takeaway:** Hard-coded backslashes pass on Windows dev boxes and fail immediately in Linux containers — Karat expects `Path.Combine` plus an explicit, configurable root instead of desktop assumptions. See **Program.cs** Section 1 and Section 8 — cross-platform rules.

---

#### Q2. How does `Path.Combine` behave with trailing slashes, rooted segments, and empty segments?

(R) An internal admin API accepts a `fileName` query parameter and serves files from a fixed folder. Review the handler:

```csharp
private readonly string _storageRoot = Path.Combine(AppContext.BaseDirectory, "uploads");

public IResult Download(string fileName)
{
    string requested = Path.GetFullPath(Path.Combine(_storageRoot, fileName));
    if (!File.Exists(requested))
        return Results.NotFound();

    return Results.File(requested);
}

// Request: GET /download?fileName=..\..\appsettings.Production.json
```

What security and correctness issues exist, and what is the prioritized fix?

**Answer:** `Path.GetFullPath` resolves `..` segments against `_storageRoot`, so a malicious `fileName` can escape the uploads folder and read arbitrary files on the server — the existence check does not confine access to the intended directory.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No verification that resolved path stays under `_storageRoot` | Path traversal — read secrets, configs, other tenants' files |
| Input | Unsanitized user-controlled `fileName` | `..`, absolute paths, alternate separators bypass intent |
| Correctness | `GetFullPath` alone is not a sandbox boundary | Developer assumes normalization equals authorization |

**Fix (priority order):**

1. Reject rooted paths and any segment containing `..` before combining — or use `Path.GetFileName(fileName)` if only flat files are allowed.
2. After resolving, verify the full path is prefixed by the normalized storage root (case-aware on Linux):

```csharp
string storageRoot = Path.GetFullPath(_storageRoot);
string requested = Path.GetFullPath(Path.Combine(storageRoot, fileName));

if (!requested.StartsWith(storageRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal)
    && !requested.Equals(storageRoot, StringComparison.Ordinal))
    return Results.BadRequest();

if (!File.Exists(requested))
    return Results.NotFound();
```

3. Prefer an opaque file id mapped server-side to a stored name instead of accepting raw path fragments from the client.
4. Log traversal attempts; return 400/404 without leaking whether the target file exists outside uploads.

**Production takeaway:** `GetFullPath` normalizes strings — it does not enforce trust boundaries. Always anchor to a known root and verify containment after resolution. See **Program.cs** Section 3 — GetFullPath pitfalls.

---

#### Q3. What is the difference between `Path.GetFullPath` and passing a relative path directly to `File.Open`?

(P) A worker service loads `config/settings.json` with a relative path. It passes locally from Visual Studio but fails in production when started as a Windows Service or from a systemd unit. The startup code:

```csharp
var settingsPath = Path.GetFullPath("config/settings.json");
var json = await File.ReadAllTextAsync(settingsPath);
```

Logs show `Environment.CurrentDirectory` is `C:\Windows\System32` on the server but the project folder when debugging. What is happening, and what anchor should production code use instead?

**Answer:** Relative paths passed to `Path.GetFullPath` resolve against `Environment.CurrentDirectory`, which follows the process working directory set by the shell, service wrapper, or scheduler — not the folder containing the published assembly.

- Locally, the IDE sets CWD to the project directory, so `config/settings.json` is found next to source layout.
- Installed services and systemd units often start with CWD `/` or `System32`, so the same relative string points at the wrong tree.
- Production code should anchor content-relative assets to `AppContext.BaseDirectory` (or `IHostEnvironment.ContentRootPath` in ASP.NET Core), which tracks the deployed app folder.

```csharp
var settingsPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "config", "settings.json"));
```

- If settings live outside the publish folder (common for secrets), read an absolute path from configuration rather than assuming a relative layout.
- Document and test startup from a non-project CWD in CI to catch this class of bug early.

**Production takeaway:** Never assume `CurrentDirectory` equals the app install location — Karat pairs this with deployment context. See **Program.cs** Section 7 — CurrentDirectory vs BaseDirectory.

---

#### Q4. Explain `Path.GetDirectoryName`, `GetFileName`, `GetFileNameWithoutExtension`, and `GetExtension`.

(P) A containerized API writes large PDF exports using `Path.GetTempFileName()` and never deletes them. After a few days in Kubernetes, pods hit `No space left on device`. The temp folder path is `/tmp` inside the container. What breaks in this pattern, and what production approach replaces `GetTempFileName`?

**Answer:** `GetTempFileName` creates a zero-byte file immediately and returns its path, but the API replaces it with a large PDF without deleting the original or subsequent temps — ephemeral container `/tmp` (often a small `emptyDir` volume) fills up because nothing cleans up and each request adds another file.

- In containers, `Path.GetTempPath()` maps to `/tmp` unless overridden by `TMPDIR` — shared across requests in the same pod with no guaranteed recycle until the pod restarts.
- `GetTempFileName` is poor for large artifacts: it creates an extra file, uses predictable patterns, and encourages orphan leaks under load.
- Prefer streaming the response directly to the client, writing to a configured persistent volume, or using `IBlobStorage` / object storage for exports.
- If scratch space is required, combine `Path.GetTempPath()` with a unique name (`Guid`), wrap writes in `try/finally`, and delete in `finally`; consider `TemporaryFileStream` patterns or bounded pools.
- Set `TMPDIR` / `TEMP` / `TMP` explicitly in the deployment manifest to a sized volume when scratch I/O is unavoidable.
- Add disk-usage metrics and liveness checks — `/tmp` exhaustion kills all endpoints in the pod.

**Production takeaway:** Temp directories in containers are small and shared — treat them as bounded scratch space with explicit cleanup, not an export archive. See **Program.cs** Section 5 — GetTempPath / GetTempFileName cleanup note.

---

#### Q5. What do `Path.GetTempPath`, `Path.GetTempFileName`, and `Path.GetRandomFileName` return — and what are the security implications of `GetTempFileName`?

(R) A desktop-style feature is ported to a headless Linux server without changes:

```csharp
public string GetDefaultExportFolder()
{
    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    return Path.Combine(desktop, "MyApp", "Exports");
}

// Startup ensures folder exists:
Directory.CreateDirectory(GetDefaultExportFolder());
```

What fails on a server or container, and how should export location be chosen for server-side processing?

**Answer:** `SpecialFolder.Desktop` assumes an interactive user profile with a Desktop directory — on headless Linux servers or minimal container images the path is often empty or points under a non-writable home directory, causing `CreateDirectory` or later writes to fail.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Platform | Desktop folder on server/container | Path empty, missing, or not writable |
| Design | UI-centric SpecialFolder on backend | Wrong abstraction for batch/API exports |
| Operations | Silent reliance on user profile layout | Worked on developer workstation; fails in prod |

**Fix (priority order):**

1. Replace Desktop with a configured server path — environment variable, `appsettings`, or mounted volume (`/app/data/exports`).
2. For multi-tenant SaaS, use tenant-scoped storage (database blob, S3, Azure Blob) rather than local filesystem folders.
3. If user-specific exports are required on a desktop app, keep `SpecialFolder` — but gate server code paths separately.
4. Validate the chosen root exists and is writable at startup; surface a clear configuration error instead of failing mid-request.

```csharp
public string GetDefaultExportFolder(IConfiguration config)
{
    var root = config["Export:RootPath"]
        ?? Path.Combine(AppContext.BaseDirectory, "exports");
    return Path.Combine(root, "MyApp", "Exports");
}
```

**Production takeaway:** `SpecialFolder` values encode OS/user UI conventions — server workloads need explicit configuration, not Desktop. See **Program.cs** Section 6 — SpecialFolder and Section 8 — do not assume drive letters or desktop layout.

---

#### Q6. How do `Path.IsPathRooted`, `HasExtension`, and `ChangeExtension` work?

(M) A path helper builds log file locations from configuration segments. Review this method called on both Windows and Linux:

```csharp
public static string BuildLogPath(string configuredRoot, string appName, string logFile)
{
    // configuredRoot might be "/var/log", "logs", or "C:\\Logs" from appsettings
    return Path.Combine("ignored", "prefix", configuredRoot, appName, logFile);
}
```

What surprising result occurs when `configuredRoot` is an absolute Unix path (`/var/log`) or a Windows drive root (`C:\Logs`), and how should callers structure segments?

**Answer:** When any segment after the first is rooted (starts with `/` on Unix or a drive/root on Windows), `Path.Combine` discards all prior segments — `"ignored"` and `"prefix"` are dropped, and the result resets to the rooted segment plus the remainder.

- `Path.Combine("ignored", "prefix", "/var/log", "MyApp", "app.log")` → `/var/log/MyApp/app.log` on Linux.
- `Path.Combine("ignored", "prefix", @"C:\Logs", "MyApp", "app.log")` → `C:\Logs\MyApp\app.log` on Windows.
- Developers expect `"ignored/prefix"` to prefix configured roots — it silently does not when the config value is absolute.
- Pass either all-relative segments under a known base, or treat an absolute configured root as the sole first argument: `Path.Combine(configuredRoot, appName, logFile)` without dummy prefixes.
- Document in configuration schema whether `LogRoot` must be relative (to `BaseDirectory`) or absolute — do not mix assumptions in one Combine chain.

**Production takeaway:** Rooted segments in `Path.Combine` reset the path — a common misconfiguration when appsettings contains absolute paths. See **Program.cs** Section 1 — Combine with rooted segment demo.

---

#### Q7. What invalid path characters does `Path.GetInvalidPathChars` / `GetInvalidFileNameChars` expose?

(D) Two services exchange file paths over a message queue. Service A (Windows) sends `D:\data\invoices\inv-001.pdf`. Service B (Linux) tries to open it and also needs a relative path for an audit log entry. A developer writes:

```csharp
string incoming = message.FilePath; // from Windows producer
string relative = Path.GetRelativePath(AppContext.BaseDirectory, incoming);
await File.ReadAllTextAsync(incoming);
```

What breaks on Linux, and what contract should replace raw absolute paths between services?

**Answer:** Windows absolute paths are meaningless on Linux — `File.ReadAllTextAsync` fails because `D:\...` is not a valid path on Unix, and `Path.GetRelativePath` cannot produce a meaningful relative path across different roots or machines.

- `GetRelativePath` requires both paths to share a common base on the same machine; cross-OS absolute paths have no shared root.
- Message contracts should carry stable identifiers (blob URI, S3 key, file id, share-relative path) — not producer-local absolute paths.
- If both services mount the same network share, agree on a **share-relative** path (`invoices/inv-001.pdf`) and each service combines with its locally configured mount point via `Path.Combine(mountRoot, relativeKey)`.
- For audit logs, store the logical key or URI, not `GetRelativePath` output from foreign paths.
- Use object storage (HTTPS URL + auth) for cross-platform handoff; consumers download to their own temp scratch if local file access is required.

**Production takeaway:** File paths are not portable across OS or hosts — exchange logical keys or URIs and resolve locally. See **Program.cs** Section 8 — Windows drive roots vs Unix single-root layout.

---

#### Q8. What is `Environment.SpecialFolder`, and how do you resolve `MyDocuments`, `ApplicationData`, and `LocalApplicationData`?

(P) A build pipeline archives deeply nested test output on Windows agents. One test creates a folder tree exceeding 260 characters. Locally it works when long-path support is enabled; on a Linux agent the same code runs but a Windows-only integration test fails with `PathTooLongException`. What explains the platform difference, and what mitigations belong in the path-building code?

**Answer:** Windows historically enforced `MAX_PATH` (260 characters) unless long-path awareness is enabled at OS and application level; Linux paths are typically limited by `PATH_MAX` (often 4096 bytes) and are far more permissive — so the same deeply nested tree exceeds Windows limits while Linux succeeds.

- Developer machines with Windows 10+ long-path policy and .NET long-path support may hide the bug until a default-config CI Windows agent runs.
- Linux CI passing does not prove Windows deployment safety when paths are built from repeated `Path.Combine` of user names, guids, and nested fixture folders.
- Mitigations: shorten segment names, hash long identifiers (`SHA256` folder name instead of full title), flatten output layout, use `\\?\` prefix only as a last resort on Windows with explicit long-path enablement.
- Read remaining length budget before creating nested dirs; fail early with a clear test message instead of `PathTooLongException` mid-run.
- Keep artifact roots shallow — `artifacts/{buildId}/{suite}/file.ext` rather than mirroring full source tree depth.
- In CI, run at least one Windows job without long-path overrides to match conservative production environments.

**Production takeaway:** Path length limits are OS- and policy-dependent — design folder layouts for the shortest common denominator (default Windows), not the most permissive agent. See **Program.cs** Section 8 — cross-platform comparison table.

---

#### Q9. How does `Environment.GetFolderPath` differ from hard-coding `C:\Users\...`?

_Answer not found._

---

#### Q10. What is `Environment.CurrentDirectory`, and how can it differ from the executable's location?

_Answer not found._

---

#### Q11. How do you get the application base directory in modern .NET (`AppContext.BaseDirectory`, `AppDomain.CurrentDomain.BaseDirectory`)?

_Answer not found._

---

#### Q12. What is the difference between absolute and relative paths in console apps vs ASP.NET Core?

_Answer not found._

---

#### Q13. How do UNC paths (`\\server\share`) interact with `Path.Combine` and `Path.GetFullPath`?

_Answer not found._

---

#### Q14. What cross-platform path differences matter when deploying the same code on Windows and Linux?

_Answer not found._

---

### 05. Working with CSV and Text Files

#### Q1. Why is there no built-in CSV parser in the BCL, and what libraries are commonly used?

(R) A partner feed import worked in QA but mis-maps vendor names in production. Review this row parser used for every data line after the header:

```csharp
public static InventoryItem? ParseRow(string line, int lineNumber)
{
    string[] cols = line.Split(',', StringSplitOptions.TrimEntries);

    if (cols.Length != 5)
        return null;

    return new InventoryItem
    {
        Sku = cols[0],
        Name = cols[1],
        Quantity = int.Parse(cols[2]),
        UnitPrice = decimal.Parse(cols[3]),
        RestockedOn = string.IsNullOrEmpty(cols[4]) ? null : DateTime.Parse(cols[4]),
    };
}
```

Sample production row: `"Acme, Inc",Widget,10,9.99,2026-01-15`

What fails, and what is the prioritized fix?

**Answer:** `Split(',')` treats the comma inside `"Acme, Inc"` as a delimiter, yielding six columns instead of five — SKU shifts into the name column and downstream fields mis-map silently when the count check is skipped or relaxed. Replace naive split with quote-aware parsing, use `TryParse` with `CultureInfo.InvariantCulture`, and return row-level errors with line numbers instead of throwing or returning null without context.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Parsing | `Split(',')` on quoted fields | Wrong column count and shifted SKU/name/qty mapping |
| Correctness | `int.Parse` / `decimal.Parse` / `DateTime.Parse` | One bad cell aborts the row via exception — or worse, if wrapped in catch-all, loses line context |
| Validation | `return null` on count mismatch | Silent drop with no operator-visible error for the `"Acme, Inc"` shape |
| Culture | Default-culture `Parse` | Locale-dependent decimal/date interpretation across servers |

**Fix (priority order):**

1. Parse with a quote-aware scanner (`SplitQuotedLine` from **Program.cs** Section 3) — commas inside double quotes stay in one field.
2. Validate `columns.Count == InventoryColumnCount` and emit `Line {n}: expected 5 columns, found {m}` — matches **Program.cs** Section 4.
3. Replace `Parse` with `TryParse(..., CultureInfo.InvariantCulture, ...)` for quantity, price, and optional date.
4. Keep required-field checks (`sku.Length == 0`) before building `InventoryItem`.

```csharp
var columns = CsvParsing.SplitQuotedLine(line);
if (columns.Count != CsvParsing.InventoryColumnCount)
{
    errors.Add($"Line {lineNumber}: expected 5 columns, found {columns.Count}.");
    continue;
}
```

**Production takeaway:** QA files without quoted commas hide the bug; partner exports with `"Acme, Inc"` expose it immediately — Karat tests whether you know Split is not CSV parsing.

---

#### Q2. What are RFC 4180 rules for CSV fields, delimiters, and record terminators?

(R) An export job writes inventory CSV on a German Windows server; a US warehouse tool rejects half the rows. Review the export path:

```csharp
public static void ExportInventory(string path, IEnumerable<InventoryItem> items)
{
    using var writer = new StreamWriter(path);
    writer.WriteLine("Sku,Name,Quantity,UnitPrice,RestockedOn");

    foreach (var item in items)
    {
        writer.WriteLine(string.Join(",",
            item.Sku,
            item.Name,
            item.Quantity.ToString(),
            item.UnitPrice.ToString("F2"),
            item.RestockedOn?.ToString("yyyy-MM-dd") ?? ""));
    }
}
```

Partner file snippet: `W-400,Acme spare,5,19,95,2026-02-15`  
Accent field on disk (UTF-8): `Müller` displays as `MÃ¼ller` in Excel on a Latin-1 assumption.

What breaks across environments, and how do you make the file portable?

**Answer:** `decimal.ToString("F2")` and bare `string.Join` use the current thread culture — on `de-DE`, `19.95` becomes `19,95`, which naive US parsers read as two columns (`19` and `95`). Default `StreamWriter` encoding varies by platform, so UTF-8 bytes misread as Windows-1252 produce mojibake (`MÃ¼ller`). Unescaped names containing commas or quotes also break column boundaries. Format numbers and dates with `CultureInfo.InvariantCulture`, escape fields with `CsvFormatting.BuildRow`, and write UTF-8 explicitly — document encoding for partners or emit UTF-8 BOM when Excel must auto-detect.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Culture | `ToString()` / `ToString("F2")` without invariant culture | Decimal comma splits one price into two CSV columns |
| Escaping | Raw `string.Join` — no quote wrapping | Commas or `"` in `Name` corrupt row shape |
| Encoding | Default `StreamWriter` encoding | UTF-8 read as Latin-1 → mojibake; Excel opens wrong code page |
| Interchange | No fixed date format culture | Ambiguous date strings if culture leaks into format |

**Fix (priority order):**

1. Build each row with `CsvFormatting.BuildRow` — doubles internal quotes and wraps fields containing commas (**Program.cs** Sections 2–3).
2. Format numbers with `ToString(CultureInfo.InvariantCulture)` or `"F2"` + invariant — same as **Program.cs** Section 4a export.
3. Use explicit UTF-8: `new StreamWriter(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))` — matches **Program.cs** Section 4a; add BOM only if Excel auto-detect is required.
4. On import, open with matching encoding (`new StreamReader(path, Encoding.UTF8)`) — avoid `Encoding.Default`; for unknown feeds, sniff BOM or validate a known header line before bulk load (full charset detection belongs in **02. StreamReader & StreamWriter**).
5. Parse inbound numbers with the same invariant rules — `decimal.TryParse(..., CultureInfo.InvariantCulture, ...)`.

```csharp
writer.WriteLine(CsvFormatting.BuildRow(
    item.Sku,
    item.Name,
    item.Quantity.ToString(CultureInfo.InvariantCulture),
    item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture),
    item.RestockedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? ""));
```

**Production takeaway:** CSV interchange is a wire format — treat culture as fixed (Invariant) on both read and write; never inherit the server's regional settings.

---

#### Q3. When must a CSV field be wrapped in double quotes?

(R) A nightly import job loads a 400 MB ERP export. Review the service method:

```csharp
public ImportResult Import(string path)
{
    string[] lines = File.ReadAllLines(path);
    var items = new List<InventoryItem>();
    var errors = new List<string>();

    for (int i = 1; i < lines.Length; i++)  // skip header at index 0
    {
        try
        {
            items.Add(ParseRow(lines[i]));
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
        }
    }

    return new ImportResult(items, errors);
}
```

What production problems appear at scale, and what pattern from this chapter replaces `ReadAllLines`?

**Answer:** `ReadAllLines` allocates a string for every row plus a large array — on a 400 MB file that spikes memory, increases GC pressure, and can OOM a constrained worker. It also assumes line index 0 is always the header, skipping comment rows and blank lines incorrectly, and `catch (Exception)` drops line numbers from error messages. Stream line-by-line with `StreamReader`/`TextReader`, skip ignorable lines, consume the first data header explicitly, and collect per-line errors while continuing the import.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | `File.ReadAllLines` loads entire file | High heap use; OOM on large partner feeds |
| Correctness | `i = 1` hard-coded header skip | `#` comment or blank first lines shift every row mapping |
| Observability | `errors.Add(ex.Message)` | Operators cannot locate row 47,832 without line numbers |
| Resilience | Exception per row inside generic catch | Partial imports OK, but `Parse` throws stop row detail unless TryParse used |

**Fix (priority order):**

1. Replace with `using StreamReader reader = new StreamReader(path)` and `while ((line = reader.ReadLine()) != null)` — flat memory (**Program.cs** Section 4, QUICK REFERENCE).
2. Reuse `InventoryCsv.Import(TextReader)` logic: skip blanks/comments, consume header once, increment `lineNumber` for each physical line.
3. Use `TryParse` + structured errors (`Line {lineNumber}: invalid quantity '{text}'`) instead of exceptions for expected bad data.
4. Accept `TextReader` in the parser so the same code reads files, streams, and `StringReader` tests.

**Production takeaway:** `ReadAllLines` is fine for demos; production imports of multi-megabyte feeds should stream — Karat pairs this with row-level error reporting from the chapter's partial-import pattern.

---

#### Q4. How do you escape a literal double quote inside a quoted CSV field?

(P) A drop-folder service uses `FileSystemWatcher` to import CSV as soon as a file appears in `\\share\inbound`. Operators report random "column count" errors and duplicate SKU rows. The handler:

```csharp
watcher.Created += (_, e) =>
{
    var (items, errors) = InventoryCsv.Import(new StreamReader(e.FullPath));
    _repository.UpsertAll(items);
};
```

What race conditions happen with partial writes, and how do you harden the watcher pipeline?

**Answer:** `Created` fires when the file is first allocated, often before the upstream copy finishes — `StreamReader` then reads a truncated file, producing short rows and column-count errors. Retries on the same growing file can also insert partial batches before the full file lands. Wait until the file size stabilizes, open with shared-read exclusion or move to a processing folder under an exclusive lock, then import once idempotently.

- **Stabilize before read:** Poll `FileInfo.Length` until unchanged for N seconds, or use a "ready" sentinel file (`inventory.csv.ready`) written after the main file completes.
- **Exclusive processing:** On pickup, `File.Move` to `processing\{guid}.csv` so no second watcher event imports the same path mid-copy.
- **Locking:** Open with `FileShare.Read` only after stable size; avoid writers still flushing — `StreamWriter` on the exporter should `Flush()` before rename (**Program.cs** Section 4a).
- **Idempotency:** Key imports by file hash + batch id so a duplicate `Created` event does not double-count SKUs (see Q5).
- **Error shape:** Surface parse errors with line numbers; do not call `UpsertAll` on a batch with critical structural failures unless business rules allow partial loads.

**Production takeaway:** FileSystemWatcher notifies early — production pipelines treat "file exists" as "file ready," never as equivalent.

---

#### Q5. What goes wrong if you split CSV lines on `Split(',')` without a proper parser?

(P) Re-running the same inbound file after a network blip must not double inventory counts. A developer adds a guard:

```csharp
public void ImportFile(string path)
{
    if (_repository.AnyImportedFrom(path))
        return;

    var (items, errors) = InventoryCsv.Import(new StreamReader(path));
    _repository.InsertAll(items);
    _repository.MarkImported(path);
}
```

Halfway through a 50k-row file the database throws; the operator fixes the DB and re-runs. What goes wrong, and how do you make the import idempotent with rollback?

**Answer:** If `MarkImported` runs only after full success, a mid-batch failure leaves no mark but may have inserted thousands of rows — a re-run duplicates them. If `MarkImported` runs before verification, a failed run blocks forever. Wrap the database work in a transaction, stage rows in a import-batch table keyed by file hash, and commit only when all rows validate — or delete-by-batch-id on failure before retry.

- **Transactional bulk insert:** `BEGIN TRANSACTION` → insert all items with `ImportBatchId` → commit; on any failure, rollback so re-run starts clean.
- **Two-phase mark:** Record batch as `Processing` before insert, flip to `Completed` on commit — retries detect `Processing`/`Failed` and either resume or rollback-by-batch-id first.
- **Idempotency key:** Hash file contents (`SHA256`) — `AnyImportedFrom` should check hash, not path, so renamed re-drops are detected.
- **Partial parse policy:** If `errors` is non-empty, decide upfront: fail entire batch (rollback) vs import valid rows — document which; do not silently mix without operator ack.
- **Line-level staging:** Insert into `StagingInventory` via streaming parser; merge into live table in one set-based statement inside the transaction.

```csharp
await using var tx = await _db.Database.BeginTransactionAsync(ct);
var batchId = await _staging.LoadAsync(items, fileHash, ct);
if (errors.Count > 0) { await tx.RollbackAsync(ct); return; }
await _repository.MergeFromStagingAsync(batchId, ct);
await _repository.CompleteBatchAsync(fileHash, ct);
await tx.CommitAsync(ct);
```

**Production takeaway:** Idempotent import means safe retry — both "no duplicate rows" and "failed run leaves no footprint"; a path-only guard satisfies neither after a partial failure.

---

#### Q6. How do you handle embedded newlines inside quoted CSV fields?

(D) Your team must ingest partner CSV feeds with quoted commas, optional date columns, and occasional header renames (`SKU` vs `Sku`). One engineer proposes `CsvHelper`; another wants to extend the hand-rolled `SplitQuotedLine` from this chapter. When do you reach for each, and what are the trade-offs for a long-lived warehouse integration?

**Answer:** Extend the hand-rolled parser when the format is narrow, stable, and you need zero dependencies and full control for learning or a single internal export shape — `SplitQuotedLine` plus invariant `TryParse` covers quoted commas and typed columns for fixed five-column inventory. Reach for **CsvHelper** when feeds vary (renamed headers, optional columns, class maps, multiple delimiters) and you want header binding, validation attributes, and RFC 4180 edge cases without maintaining parser state yourself.

- **Hand-rolled (this chapter):** Fixed schema (`InventoryColumnCount`), quote-aware scan, explicit row errors — minimal surface, easy to unit-test with `StringReader`, no package churn; you own multiline fields, alternate encodings, and every new partner quirk.
- **CsvHelper:** `[Name("SKU")]`, `ClassMap`, `MissingFieldFound`, culture options, async enumeration — faster to onboard new feeds; adds dependency and team must learn mapping API; still need staging, transactions, and idempotency around it.
- **Header drift:** Hand-rolled code often assumes first line equals `InventoryHeader` string — brittle. CsvHelper can map by index or name with case-insensitive matching; either way, validate required columns up front and fail with a clear "missing column SKU" message.
- **Hybrid:** CsvHelper for deserialization into DTOs, then domain validation (`sku` required, qty ≥ 0) in a service layer — keeps parser concerns separate from warehouse rules (**Program.cs** `InventoryItem` pattern).
- **When not to hand-roll:** Multiple partners, embedded newlines in fields, tab/pipe delimiters, or frequent spec changes — maintenance cost exceeds CsvHelper's learning curve.

**Production takeaway:** Parser choice is an integration lifecycle bet — fixed internal format favors transparent hand-rolled code; multi-partner feeds favor a library plus staging and idempotent merge either way.

---

#### Q7. What issues arise with culture-specific decimal separators in CSV numeric columns?

(R) Two import workers occasionally corrupt the same nightly file. Review the concurrent access pattern:

```csharp
public async Task ImportAsync(string path, CancellationToken ct)
{
    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    using var reader = new StreamReader(stream);
    var (items, errors) = InventoryCsv.Import(reader);
    await _repository.BulkInsertAsync(items, ct);
}

// Hosted service starts two overlapping imports when backlog > 1:
_ = ImportAsync(latestFile, ct);
_ = ImportAsync(latestFile, ct);
```

Separately, a new feed omits the header row entirely. The parser assumes the first non-blank line is always the header. What fails under concurrency and header drift, and how do you fix both?

**Answer:** Two workers reading the same file concurrently duplicate database inserts unless the import is idempotent — and if either process also opens with write sharing, interleaved reads can see inconsistent snapshots on some OS/network shares. Header-less feeds mis-map the first data row as column names, shifting every subsequent field. Serialize per-file processing, move files exclusively before import, and detect headers by column signature rather than blind "first line skip."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Two `ImportAsync` on same path | Duplicate SKU rows or race on `_repository` |
| File I/O | Default `FileStream` share mode | Writer/exporter may still hold lock; reader fails or sees partial content |
| Correctness | First non-blank line = header | Header-less feed imports garbage first row; silent wrong types |
| Orchestration | Fire-and-forget duplicate tasks | No single-owner guarantee for nightly drop |

**Fix (priority order):**

1. **Single consumer:** Queue file paths; one worker processes each file — or `File.Move` to `processing\{id}.csv` atomically so only one worker owns the path.
2. **Open read-only with explicit share:** `new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)` — document that exporters must finish before drop.
3. **Header detection:** If first row matches header pattern (`Sku,Name,...` case-insensitive) or column count/types fail (`int.TryParse` on col 2 fails), treat line as data and use known column order — or reject with "header row missing."
4. **Idempotency:** Combine with Q5 — file hash + batch id so a duplicate worker run does not double insert.
5. **Optional:** `FileShare.None` during move-then-import pipeline on local disk; on SMB shares, prefer copy-to-local-temp then import.

```csharp
if (!headerConsumed && LooksLikeHeader(line))
{
    headerConsumed = true;
    continue;
}
// if no header seen after policy check, use DefaultInventoryColumns map
```

**Production takeaway:** Concurrent import is a workflow bug first and a parsing bug second — exclusive file ownership plus header validation prevents both duplicate rows and shifted columns.

---

#### Q8. How do you write CSV headers and ensure stable column ordering for downstream consumers?

_Answer not found._

---

#### Q9. What is the difference between `\n`, `\r\n`, and `Environment.NewLine` for text file line endings?

_Answer not found._

---

#### Q10. How do you normalize line endings when reading files produced on Windows vs Linux?

_Answer not found._

---

#### Q11. What are best practices for large CSV ingestion (streaming vs loading all rows)?

_Answer not found._

---

#### Q12. How do you validate CSV row shape (column count) before deserializing to objects?

_Answer not found._

---

#### Q13. When should you use fixed-width text formats instead of CSV?

_Answer not found._

---

#### Q14. How do you properly dispose file resources across layered readers (`FileStream` → `StreamReader`)?

_Answer not found._

---

#### Q15. What logging and rotation patterns apply when appending to text log files over time?

_Answer not found._

---

#### Q16. **`ReadAllLines` vs `ReadLines`** — `ReadAllLines` loads the entire file into a `string[]`; `ReadLines` is lazy but keeps the file open until enumeration finishes or is disposed.

_Answer not found._

---

#### Q17. **Undisposed streams lock files on Windows** — A finalized-but-not-disposed `FileStream`/`StreamWriter` can block deletes, renames, and antivirus scans until GC runs.

_Answer not found._

---

#### Q18. **`FileShare` defaults to exclusive access** — Opening without `FileShare.Read` prevents other processes from reading concurrently.

_Answer not found._

---

#### Q19. **Hard-coded path separators break cross-platform** — `"folder\\file.txt"` fails on Linux; always use `Path.Combine`.

_Answer not found._

---

#### Q20. **Relative paths depend on `CurrentDirectory`** — A path valid in Visual Studio may fail as a Windows Service or cron job where CWD differs.

_Answer not found._

---

#### Q21. **`Path.Combine` with an absolute second segment discards earlier parts** — `Path.Combine("C:\\a", "D:\\b")` yields `D:\b`, which surprises many candidates.

_Answer not found._

---

#### Q22. **Encoding mismatch silently corrupts text** — Default UTF-8 assumptions break on Windows-1252 or UTF-16 LE files; specify `Encoding` explicitly.

_Answer not found._

---

#### Q23. **Seeking past EOF then writing extends the file with undefined gap bytes** — Understand sparse/hole behavior when patching binary files in place.

_Answer not found._

---

#### Q24. **CSV `Split(',')` breaks on quoted commas** — `"Smith, Jr.",42` becomes three fields; use a real parser or state machine.

_Answer not found._

---

#### Q25. **Double-quote escaping in CSV is `""` not `\"`** — Getting escape rules wrong produces columns that shift on import.

_Answer not found._

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A report export service must create a file only if it does not already exist. Review this helper used under concurrent load:

```csharp
public static void EnsureReportFile(string path, string header)
{
    if (!File.Exists(path))
    {
        using var stream = File.Create(path);
        var bytes = Encoding.UTF8.GetBytes(header);
        stream.Write(bytes, 0, bytes.Length);
    }
}
```

Two requests for the same path occasionally throw `IOException: file already exists`, and sometimes one request silently skips writing. What is wrong, and how do you fix it for production?

---

**Answer:**

```csharp
public static void EnsureReportFile(string path, string header)
{
    if (!File.Exists(path))
    {
        using var stream = File.Create(path);
        var bytes = Encoding.UTF8.GetBytes(header);
        stream.Write(bytes, 0, bytes.Length);
    }
}
```

Two requests for the same path occasionally throw `IOException: file already exists`, and sometimes one request silently skips writing. What is wrong, and how do you fix it for production?

**Answer:** This is a classic TOCTOU (time-of-check to time-of-use) race: `File.Exists` and `File.Create` are not atomic, so two threads can both pass the check and one `File.Create` wins while the other throws, or one thread creates the file after another passed `Exists` and the second call skips writing entirely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Check-then-act gap between `Exists` and `Create` | Duplicate create attempts → `IOException`; skipped writes when file appears between check and branch |
| Concurrency | No synchronization or exclusive-create semantics | Intermittent failures under load — passes in single-threaded dev |
| Design | `Exists` + `Create` mimics "create if missing" without atomicity | Wrong abstraction for idempotent report generation |

**Fix (priority order):**

1. Use an exclusive create that fails fast if the file already exists — `new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)` — and catch `IOException` to treat "already exists" as expected, or return a conflict result.
2. If multiple writers must coordinate, add an app-level lock keyed by path, or use a database/object-store claim — filesystem races do not scale across pods without external coordination.
3. For idempotent content, prefer write-to-temp-then-atomic-rename (see Q2/Q5) instead of "create only if missing."
4. Remove the silent skip path — if the file exists but is empty or stale, `Exists` returning true hides a partial write from a crashed peer.

```csharp
try
{
    using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    var bytes = Encoding.UTF8.GetBytes(header);
    stream.Write(bytes, 0, bytes.Length);
}
catch (IOException) when (File.Exists(path))
{
    // Another writer won the race — handle idempotently or surface conflict
}
```

**Production takeaway:** Karat uses `File.Exists` + `File.Create` to test whether you know TOCTOU — the fix is atomic open semantics (`CreateNew`) or external locking, not a tighter `if`. See **Program.cs** Section 8 — guard before read is not the same as atomic create.

---

---

#### Q2. (R) A teammate refactors upload processing to write through a temp file, then move into place. Review the method:

```csharp
public async Task SaveUploadAsync(IFormFile upload, string finalPath, CancellationToken ct)
{
    string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
    await using (var temp = File.Create(tempPath))
    {
        await upload.CopyToAsync(temp, ct);
    }

    if (File.Exists(finalPath))
        File.Delete(finalPath);

    File.Move(tempPath, finalPath);
}
```

What breaks when `CopyToAsync` throws, when the app runs in a Linux container with a read-only root filesystem, and when two pods write the same `finalPath`?

---

**Answer:**

```csharp
public async Task SaveUploadAsync(IFormFile upload, string finalPath, CancellationToken ct)
{
    string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
    await using (var temp = File.Create(tempPath))
    {
        await upload.CopyToAsync(temp, ct);
    }

    if (File.Exists(finalPath))
        File.Delete(finalPath);

    File.Move(tempPath, finalPath);
}
```

What breaks when `CopyToAsync` throws, when the app runs in a Linux container with a read-only root filesystem, and when two pods write the same `finalPath`?

**Answer:** The temp-then-move pattern is right in spirit, but this version leaks temp files on failure, may write temps to an unwritable or ephemeral location in containers, and still has TOCTOU races on the final path — plus `File.Move` is not atomic across volumes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource cleanup | No `try/finally` or `try/catch` to delete `tempPath` on failure | Orphaned `.bin` files fill `%TEMP%` / container overlay — see Q7 |
| Deployment | `Path.GetTempPath()` + `finalPath` on read-only app dir | `CopyToAsync` or `Move` throws in Kubernetes/App Service when dest is not writable |
| Concurrency | `Exists` → `Delete` → `Move` on shared `finalPath` | Two pods interleave deletes/moves — corrupt or missing final file |
| Cross-volume | `File.Move` between temp dir and data volume | Becomes copy+delete — not atomic; crash mid-flight leaves duplicates or partial files |

**Fix (priority order):**

1. Wrap temp lifecycle in `try/finally` (or a `TempFile`/`TempWorkspace` disposable) that deletes the temp path on any exception.
2. Stage temp files in the **same directory** as `finalPath` (e.g. `finalPath + ".tmp"`) so `File.Move` is a same-volume rename — atomic on POSIX and NTFS for same directory.
3. Write to `finalPath.tmp`, flush/fsync if durability matters, then `File.Move(tmp, finalPath, overwrite: true)` (.NET 5+) — avoid separate delete step.
4. In containers, mount a writable volume for uploads (`/app/data` or blob storage); never assume `AppContext.BaseDirectory` or root FS is writable.
5. For multi-instance writes to one key, use object storage (S3/Azure Blob) with etag preconditions or a DB row — not shared filesystem without locking.

```csharp
string dir = Path.GetDirectoryName(finalPath)!;
string tempPath = Path.Combine(dir, $".{Guid.NewGuid():N}.tmp");
try
{
    await using (var temp = File.Create(tempPath))
        await upload.CopyToAsync(temp, ct);

    File.Move(tempPath, finalPath, overwrite: true);
    tempPath = null; // success — do not delete in finally
}
finally
{
    if (tempPath is not null && File.Exists(tempPath))
        File.Delete(tempPath);
}
```

**Production takeaway:** Temp-file staging is a production pattern only when cleanup, same-directory rename, and writable volume paths are handled — Karat stacks failure cleanup with container filesystem constraints.

---

---

#### Q3. (R) A nightly cleanup job removes old workspace folders. Review:

```csharp
public void PurgeWorkspace(string workspaceRoot)
{
    foreach (string file in Directory.GetFiles(workspaceRoot, "*", SearchOption.AllDirectories))
    {
        File.SetAttributes(file, FileAttributes.Normal);
        File.Delete(file);
    }

    Directory.Delete(workspaceRoot, recursive: false);
}
```

Locally it works on small trees; in production it throws `IOException` on non-empty directories or `UnauthorizedAccessException` on hidden/system files. What is wrong with this approach, and what should you use instead?

---

**Answer:**

```csharp
public void PurgeWorkspace(string workspaceRoot)
{
    foreach (string file in Directory.GetFiles(workspaceRoot, "*", SearchOption.AllDirectories))
    {
        File.SetAttributes(file, FileAttributes.Normal);
        File.Delete(file);
    }

    Directory.Delete(workspaceRoot, recursive: false);
}
```

Locally it works on small trees; in production it throws `IOException` on non-empty directories or `UnauthorizedAccessException` on hidden/system files. What is wrong with this approach, and what should you use instead?

**Answer:** Manual file-by-file deletion before a non-recursive `Directory.Delete` is slower, still fails on nested subdirectories, and fights read-only/hidden attributes — while leaving the tree inconsistent if any step throws mid-loop. The API already supports recursive delete in one call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API misuse | `Directory.Delete(..., recursive: false)` after only deleting **files** | Subfolders remain → `IOException: directory not empty` |
| Scalability | `GetFiles(..., AllDirectories)` loads entire tree into memory | Large workspaces → memory pressure; long lock while iterating |
| Reliability | Partial loop then exception | Some files deleted, folder half-purged — harder to retry idempotently |
| Permissions | `SetAttributes(Normal)` on every file | May still fail on locked files (open handles) or ACL/UAC denied paths |

**Fix (priority order):**

1. Use `Directory.Delete(workspaceRoot, recursive: true)` — one call removes files and nested folders (see **Program.cs** Section 9).
2. Before delete, ensure no open `FileStream`/`StreamReader` handles — dispose all streams first (Section 7 — sharing violation on Windows).
3. For large trees, prefer `DirectoryInfo.EnumerateFiles` with lazy enumeration if you must pre-process, but still finish with recursive delete — do not hand-roll tree walking unless you need selective retention.
4. On permission errors, fix ACLs or run under a service account with rights to the data directory — attribute clearing is not a substitute for proper permissions in prod.
5. Wrap in retry for transient sharing violations if antivirus/indexer holds brief locks.

**Production takeaway:** `Directory.Delete` without `recursive: true` on a non-empty folder is a common tutorial pitfall scaled to production — Karat expects you to know when recursive delete is correct and when open handles block it.

---

---

#### Q4. (P) A multi-process log aggregator appends audit lines from several worker threads. One worker uses `File.AppendAllText`; another opens with default sharing:

```csharp
// Worker A
File.AppendAllText(logPath, line + Environment.NewLine);

// Worker B
using var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write);
using var writer = new StreamWriter(fs);
writer.WriteLine(line);
```

Under load you see `IOException: sharing violation` and occasionally interleaved garbage bytes. Explain `FileShare` behavior here and show a production-safe append pattern.

---

**Answer:**

```csharp
// Worker A
File.AppendAllText(logPath, line + Environment.NewLine);

// Worker B
using var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write);
using var writer = new StreamWriter(fs);
writer.WriteLine(line);
```

Under load you see `IOException: sharing violation` and occasionally interleaved garbage bytes. Explain `FileShare` behavior here and show a production-safe append pattern.

**Answer:** Default `FileStream` constructors use `FileShare.Read`, which excludes other writers — concurrent appenders block each other with sharing violations. Even when opens succeed, unsynchronized multi-writer appends interleave bytes at the OS level without line atomicity.

- `File.AppendAllText` opens, appends, and closes per call — high overhead and still races with other writers using incompatible share flags.
- For multiple writers on one file, open with `FileShare.ReadWrite` so other handles can coexist: `new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)`.
- Line atomicity is **not** guaranteed by `FileShare` — two threads can still interleave mid-line; use a `Mutex`/`SemaphoreSlim` named by path, a single dedicated writer thread/channel, or one process owning the log file.
- Better production pattern: append to stdout and let the platform aggregate (Kubernetes logging, Azure Monitor), or write per-process log files and merge — avoid many writers to one file on Windows especially.
- If you must share one file, wrap append in a process-wide lock and flush after each line; consider `StreamWriter` with `AutoFlush = true`.

```csharp
private static readonly SemaphoreSlim _logLock = new(1, 1);

await _logLock.WaitAsync(ct);
try
{
    await File.AppendAllTextAsync(logPath, line + Environment.NewLine, ct);
}
finally
{
    _logLock.Release();
}
```

**Production takeaway:** Sharing violations mean incompatible `FileShare` flags or an undisposed handle — Karat ties **Program.cs** Section 7 (open handles block delete/write) to concurrent append design, not just "use a lock" memorization.

---

---

#### Q5. (P) An export job stages files under `%TEMP%` on Windows, then calls `File.Move(source, dest)` into a network share. On developer laptops it works; in Azure App Service (Linux) and when crossing drive letters it fails with `IOException` or leaves duplicate files. What is happening at the OS level, and what pattern replaces naive `File.Move`?

---

**Answer:**

**Answer:** `File.Move` is only a cheap atomic rename when source and destination are on the **same file system/volume**. Cross-volume or temp-to-network-share moves degrade to copy-then-delete — slow, non-atomic, and vulnerable to partial failure — and Linux container temp paths often live on a different mount than persisted data volumes.

- Windows: moving from `C:\Users\...\Temp` to `D:\` or `\\server\share` triggers copy+delete, not rename — crash after copy leaves both files or neither in expected state.
- Linux containers: `/tmp` may be tmpfs while `/app/data` is a mounted volume — `File.Move` cannot rename across mounts; errno `EXDEV` → .NET wraps as `IOException`.
- Hidden cost: large files copied twice consume disk and time; antivirus on network paths adds locks.
- Production pattern: stage temp file in the **destination directory** (hidden `.part` suffix), fsync if required, then same-directory `File.Move` to final name — atomic replace on same volume.
- For cross-machine delivery, skip filesystem move entirely — stream to blob storage (S3/Azure Blob) with server-side commit, or use a message queue with object key — not SMB paths from app servers.
- Use `Path.GetPathRoot` or compare `Directory.GetDirectoryRoot` of source and dest in diagnostics; if roots differ, plan copy+verify+delete explicitly with checksum validation.

**Production takeaway:** **Program.cs** Section 3 notes Move is "atomic rename on same volume" — Karat tests whether you apply that caveat when `%TEMP%` and upload folders diverge in cloud deploys.

---

---

#### Q6. (M) An ASP.NET Core endpoint reads a 200 MB CSV from disk on every request:

```csharp
app.MapGet("/reports/{id}", (string id, IReportStore store) =>
{
    string path = store.GetPath(id);
    if (!File.Exists(path))
        return Results.NotFound();

    string csv = File.ReadAllText(path);
    return Results.Content(csv, "text/csv");
});
```

Latency spikes under concurrent traffic and thread-pool queue depth grows, even though CPU stays low. What mechanism is blocking, and what file APIs would you use instead?

---

**Answer:**

```csharp
app.MapGet("/reports/{id}", (string id, IReportStore store) =>
{
    string path = store.GetPath(id);
    if (!File.Exists(path))
        return Results.NotFound();

    string csv = File.ReadAllText(path);
    return Results.Content(csv, "text/csv");
});
```

Latency spikes under concurrent traffic and thread-pool queue depth grows, even though CPU stays low. What mechanism is blocking, and what file APIs would you use instead?

**Answer:** `File.ReadAllText` synchronously reads the entire 200 MB into a single `string` on a thread-pool thread — blocking async I/O throughput and allocating a huge LOH object — so concurrent requests queue behind blocked threads even though the work is I/O-bound.

- The minimal API delegate is synchronous; each request ties up a thread for the full disk read — classic thread-pool starvation under load (same class of problem as `.Result` on async I/O).
- `ReadAllText` doubles memory (file bytes + UTF-16 string) — 200 MB file can mean 400 MB+ per request peak.
- Prefer `return Results.File(path, "text/csv", enableRangeProcessing: true)` — streams from disk with `SendFileAsync` / efficient OS sendfile where available, no full buffering in managed memory.
- If transformation is required: `async Task<IResult>` with `await File.ReadAllTextAsync(path, ct)` or better `File.OpenRead` + `StreamReader` / pipe through `Results.Stream`.
- Add caching (`IMemoryCache` with size limits), CDN, or object storage pre-signed URLs for large static exports — disk read per request does not scale.
- Pass `CancellationToken` from `HttpContext.RequestAborted` so clients disconnecting abort the read.

```csharp
app.MapGet("/reports/{id}", (string id, IReportStore store) =>
{
    string path = store.GetPath(id);
    return File.Exists(path)
        ? Results.File(path, "text/csv", fileDownloadName: $"{id}.csv", enableRangeProcessing: true)
        : Results.NotFound();
});
```

**Production takeaway:** Sync all-at-once file helpers from **Program.cs** Section 3 (`ReadAllText`) are fine for small demo files — in web apps they block the thread pool; Karat expects streaming async APIs for large I/O.

---

---

#### Q7. (D) A containerized API creates per-request scratch directories under `Path.GetTempPath()` but never deletes them when handlers throw. Disk on the node fills over days; restarting the pod "fixes" it until the next deploy. Compare three cleanup strategies — `try/finally`, `IDisposable` workspace helper, and OS temp with periodic janitor — for production container deployments. What is your default and why?

---

### 02. StreamReader & StreamWriter

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/02. StreamReader & StreamWriter`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Orphaned scratch dirs are a deployment lifecycle bug, not a filesystem API quirk — the default should be deterministic per-operation cleanup via an `IDisposable` workspace scoped to the request, with a periodic janitor as a safety net in long-lived pods.

| Strategy | Strengths | Weaknesses |
|---|---|---|
| **`try/finally` inline** | Simple; guaranteed on exit from one method | Easy to forget when logic branches across helpers; duplicated across endpoints |
| **`IDisposable` workspace (`using var ws = new TempWorkspace(...)`)** | Centralizes create/delete; composes with `await using`; testable | Requires discipline to always `using`; nested scopes must not double-delete |
| **OS temp + periodic janitor** | Catches leaks from third-party libs and crash kills; good backstop in K8s | Not sufficient alone — unbounded growth between sweeps fills emptyDir/volume; race if janitor deletes active dirs |

- Default: **`IDisposable`/`IAsyncDisposable` temp workspace** created at request entry, deleted in `Dispose` even on exceptions — matches **Program.cs** Main's clean-slate pattern (`Directory.Delete` before recreate) but scoped per operation.
- Implement janitor as secondary: delete directories under temp older than N hours **only if** naming includes GUID and heartbeat file — never blanket `Delete` on entire `GetTempPath()` while app runs.
- In containers, mount scratch space with size limits (`emptyDir` sizeLimit) so leaks fail fast instead of evicting neighbors; prefer streaming to blob storage over large local scratch.
- Log workspace path on creation at Debug level; metric `temp_workspace_bytes` for observability.
- Avoid relying on pod restart as cleanup policy — violates 12-factor; masks handler bugs.

**Production takeaway:** Karat uses container disk fill to test whether you connect **Program.cs** cleanup demos to request-scoped `using` and deployment volume limits — restart is not a cleanup strategy.

---

### 02. StreamReader & StreamWriter

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/02. StreamReader & StreamWriter`

---

---

#### Q1. (R) A nightly audit job throws on bad rows and operators report the log file stays locked until the worker restarts. Review this helper:

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");

    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected — fix upstream feed.");

    writer.Dispose();
}
```

What keeps the file locked, and how do you fix it without losing the rejected row on disk?

---

**Answer:**

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");

    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected — fix upstream feed.");

    writer.Dispose();
}
```

What keeps the file locked, and how do you fix it without losing the rejected row on disk?

**Answer:** When validation throws, `Dispose()` never runs, so the `StreamWriter` keeps the underlying file handle open — on Windows the log stays locked until GC finalizes the writer. Wrap the writer in `using` (or `try/finally`) so the handle is released even on the exception path; the invalid row is already on disk because `WriteLine` ran before the throw.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource lifetime | No `using` / `finally`; `Dispose()` only on happy path | File handle leak; "file in use" on next append |
| Ordering | Validate after write | Rejected rows still persisted — may be intended, but callers must know |
| Platform | Undisposed `StreamWriter` on Windows | Lock persists until process exit or finalizer — common ops incident |

**Fix (priority order):**

1. Use `using (var writer = new StreamWriter(logPath, append: true)) { … }` so dispose runs on all exit paths.
2. If invalid rows must not be written, validate **before** `WriteLine`, or write to a staging file and commit on success.
3. For long-lived services, prefer `await using StreamWriter` with async writes if the call chain is async end-to-end.
4. Monitor for handle leaks — repeated failures should not require worker restart to unlock the log.

```csharp
public void AppendAuditEntry(string logPath, string entry)
{
    if (entry.Contains("INVALID", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Audit row rejected — fix upstream feed.");

    using StreamWriter writer = new StreamWriter(logPath, append: true);
    writer.WriteLine($"{DateTime.UtcNow:o} {entry}");
}
```

**Production takeaway:** Karat pairs exception flow with I/O cleanup — `StreamWriter` is not magic; undisposed writers are production file locks. See **Program.cs** Section 6 — `using` / `Dispose` and QUICK REFERENCE — "Forgetting using / Dispose → file locked until GC."

---

---

#### Q2. (R) A CSV export looks correct on the developer's Windows machine but the first column header fails validation after deploy to Linux containers. Review write vs read:

```csharp
// Export service (built and tested on Windows)
using (var writer = new StreamWriter(exportPath, append: false, Encoding.Default))
{
    writer.WriteLine("id,name,amount");
    writer.WriteLine("1,Alpha,42.50");
}

// Import service (Linux container — default StreamReader ctor)
using (var reader = new StreamReader(importPath))
{
    string? header = reader.ReadLine();
    if (header != "id,name,amount")
        throw new InvalidDataException($"Unexpected header: '{header}'");
}
```

What fails in production, and how do you make the round-trip deterministic across OS boundaries?

---

**Answer:**

```csharp
// Export service (built and tested on Windows)
using (var writer = new StreamWriter(exportPath, append: false, Encoding.Default))
{
    writer.WriteLine("id,name,amount");
    writer.WriteLine("1,Alpha,42.50");
}

// Import service (Linux container — default StreamReader ctor)
using (var reader = new StreamReader(importPath))
{
    string? header = reader.ReadLine();
    if (header != "id,name,amount")
        throw new InvalidDataException($"Unexpected header: '{header}'");
}
```

What fails in production, and how do you make the round-trip deterministic across OS boundaries?

**Answer:** `Encoding.Default` is the **system code page** — Windows-1252 on Windows, often UTF-8 on modern Linux — so bytes on disk differ by environment, and the reader's default detection may decode the same bytes differently than the writer encoded them. Pin both sides to explicit `Encoding.UTF8` (typically `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` for no BOM) and compare headers after `ReadLine()`, which already returns decoded characters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encoding | `Encoding.Default` on write | Different byte sequences per OS — mojibake or subtle header mismatch |
| Encoding | Implicit reader encoding on read | Guessing wrong code page — first line may include stray BOM or wrong chars |
| Contract | String equality on header | Fails even when visually "correct" in a GUI editor |

**Fix (priority order):**

1. Replace `Encoding.Default` with explicit UTF-8 on **both** writer and reader constructors.
2. Document encoding in the file format contract; reject files whose BOM/bytes do not match.
3. For CSV consumed by Excel on Windows, decide deliberately on UTF-8 BOM vs no BOM — do not rely on defaults.
4. Add an integration test that round-trips on Linux CI, not only on the developer's Windows box.

```csharp
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

using (var writer = new StreamWriter(exportPath, append: false, utf8))
    writer.WriteLine("id,name,amount");

using (var reader = new StreamReader(importPath, utf8))
{
    string? header = reader.ReadLine();
    // ...
}
```

**Production takeaway:** "Works on my machine" for text files is almost always an encoding default mismatch — Karat expects you to name `UTF8Encoding` and match reader/writer. See **Program.cs** Section 4 — pass the same `Encoding` to matching ctors.

---

---

#### Q3. (R) A support dashboard calls this to show the tail of a customer log. Under load the worker process recycles with `OutOfMemoryException`. Review:

```csharp
public string LoadCustomerLogForSupport(string logPath)
{
    if (!File.Exists(logPath))
        return string.Empty;

    using StreamReader reader = new StreamReader(logPath);
    return reader.ReadToEnd();
}
```

Customer logs can exceed 10 GB. What is wrong, and what pattern replaces `ReadToEnd` for this use case?

---

**Answer:**

```csharp
public string LoadCustomerLogForSupport(string logPath)
{
    if (!File.Exists(logPath))
        return string.Empty;

    using StreamReader reader = new StreamReader(logPath);
    return reader.ReadToEnd();
}
```

Customer logs can exceed 10 GB. What is wrong, and what pattern replaces `ReadToEnd` for this use case?

**Answer:** `ReadToEnd()` allocates a single `string` for the entire remaining file — with 10 GB logs that forces a multi-gigabyte LOH allocation and typically terminates the process with `OutOfMemoryException`. Stream line-by-line with `ReadLine()` or `ReadLineAsync()`, seek to a tail window with `FileStream` + bounded `ReadBlock`, or use external tail tools — never materialize the whole file for a "show last lines" feature.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | `ReadToEnd()` on multi-GB file | `OutOfMemoryException`; worker recycle under concurrent support requests |
| API misuse | Support "tail" implemented as full load | Latency and memory scale with file size, not UI need |
| Scalability | Sync read of entire blob | Thread blocked for duration of huge I/O |

**Fix (priority order):**

1. For tail UI: open `FileStream` with `FileShare.ReadWrite`, seek near EOF, read last N KB/chars, split lines locally.
2. For scanning: `while ((line = await reader.ReadLineAsync(ct)) != null)` — bounded memory regardless of file size.
3. Cap response size returned to the dashboard (e.g., last 500 lines or 256 KB).
4. Move huge log analytics to indexed storage — files on disk are not a query engine.

```csharp
public async Task<IReadOnlyList<string>> ReadLastLinesAsync(string logPath, int maxLines, CancellationToken ct)
{
    var lines = new Queue<string>(maxLines);
    await using StreamReader reader = new StreamReader(logPath);
    while (await reader.ReadLineAsync(ct) is { } line)
    {
        if (lines.Count == maxLines) lines.Dequeue();
        lines.Enqueue(line);
    }
    return lines.ToArray();
}
```

**Production takeaway:** `ReadToEnd()` is for small files only — Karat uses log scale to test whether you know **Program.cs** Section 3 (`ReadLine` / `ReadBlock`) vs Section 7 (`File.ReadAllText` trap). Same mistake as `File.ReadAllText` on huge files.

---

---

#### Q4. (R) A long-running export writes a status file so another process can poll completion. Operators see `IN_PROGRESS` forever after a crash mid-run. Review:

```csharp
string statusPath = Path.Combine(outputDir, "export.status");
StreamWriter writer = new StreamWriter(statusPath, append: false);
writer.WriteLine("IN_PROGRESS");

RunHeavyExport(); // may take 20+ minutes; process sometimes killed by OOM killer

writer.WriteLine("COMPLETE");
writer.Dispose();

// Poller (separate process):
using StreamReader poller = new StreamReader(statusPath);
string lastLine = poller.ReadToEnd().TrimEnd().Split('\n').Last();
bool done = lastLine == "COMPLETE";
```

What causes false "stuck" exports and incomplete status files, and how do you harden write + detection?

---

**Answer:**

```csharp
string statusPath = Path.Combine(outputDir, "export.status");
StreamWriter writer = new StreamWriter(statusPath, append: false);
writer.WriteLine("IN_PROGRESS");

RunHeavyExport(); // may take 20+ minutes; process sometimes killed by OOM killer

writer.WriteLine("COMPLETE");
writer.Dispose();

// Poller (separate process):
using StreamReader poller = new StreamReader(statusPath);
string lastLine = poller.ReadToEnd().TrimEnd().Split('\n').Last();
bool done = lastLine == "COMPLETE";
```

What causes false "stuck" exports and incomplete status files, and how do you harden write + detection?

**Answer:** `StreamWriter` buffers output — `IN_PROGRESS` may not hit disk until `Flush()` or `Dispose`, so a poller can see an empty or stale file early. If the process dies mid-export, `COMPLETE` is never written and the poller correctly sees stuck state, but the writer also lacks atomic replace semantics — partial flushes can leave truncated files. Use `AutoFlush` or explicit `Flush()` after status transitions, write-temp-then-`File.Move` for atomic status, and treat absence of `COMPLETE` plus process exit as failure with timeout alerting.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Buffering | No `Flush` / `AutoFlush` after `IN_PROGRESS` | Poller reads empty or old content; false negatives |
| Durability | Single file overwritten in place | Crash mid-write → truncated or blank status file |
| Detection | `ReadToEnd().Split('\n').Last()` on in-progress write | May read partial buffer; race with writer |
| Lifecycle | No `using` on writer if exception in `RunHeavyExport` | Handle leak + final status never written |

**Fix (priority order):**

1. Enable `writer.AutoFlush = true` or call `Flush()` immediately after each status line.
2. Write status to a temp file and atomically replace: `File.WriteAllText(temp, status); File.Move(temp, statusPath, overwrite: true);` — or use `StreamWriter` on temp then move.
3. Poller: check file length stability, last-write time, and explicit `FAILED`/`COMPLETE` tokens; add SLA timeout.
4. Wrap writer in `using` and set `FAILED` in `catch`/`finally` when export aborts.

```csharp
using StreamWriter writer = new StreamWriter(statusPath, append: false) { AutoFlush = true };
writer.WriteLine("IN_PROGRESS");
try
{
    RunHeavyExport();
    writer.WriteLine("COMPLETE");
}
catch
{
    writer.WriteLine("FAILED");
    throw;
}
```

**Production takeaway:** Karat stacks buffering + crash recovery — operators care about **observable** state on disk, not in-process buffers. See **Program.cs** Section 6 — `Flush`, `AutoFlush`, and dispose flushes remaining buffer.

---

---

#### Q5. (R) A log tailer and a log writer run in the same app. The tailer intermittently throws `IOException: The process cannot access the file because it is being used by another process`. Review:

```csharp
public IEnumerable<string> TailLines(string logPath)
{
    using FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read);
    using StreamReader reader = new StreamReader(fs);

    while (!reader.EndOfStream)
    {
        string? line = reader.ReadLine();
        if (line != null)
            yield return line;
    }
}

// Elsewhere, on another thread:
using StreamWriter writer = new StreamWriter(logPath, append: true);
writer.WriteLine($"{DateTime.UtcNow:o} INFO  heartbeat");
```

What sharing rule is missing, and why does `StreamReader`/`StreamWriter` path constructors hide it?

---

**Answer:**

```csharp
public IEnumerable<string> TailLines(string logPath)
{
    using FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read);
    using StreamReader reader = new StreamReader(fs);

    while (!reader.EndOfStream)
    {
        string? line = reader.ReadLine();
        if (line != null)
            yield return line;
    }
}

// Elsewhere, on another thread:
using StreamWriter writer = new StreamWriter(logPath, append: true);
writer.WriteLine($"{DateTime.UtcNow:o} INFO  heartbeat");
```

What sharing rule is missing, and why does `StreamReader`/`StreamWriter` path constructors hide it?

**Answer:** Opening `FileStream` with default `FileShare.Read` grants exclusive write access — a concurrent `StreamWriter` on the same path cannot open for append. Open the tailer's stream with `FileShare.ReadWrite` (and usually `FileMode.Open`, `FileAccess.Read`) so writers can append while you read. Path-based `StreamReader`/`StreamWriter` ctors create their own `FileStream` with sharing defaults you do not see — for tail-follow scenarios, construct `FileStream` explicitly, then wrap with `leaveOpen: true`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| File sharing | Default `FileShare.Read` on read stream | `IOException` when appender opens same log |
| API visibility | `new StreamReader(path)` hides share flags | Developers miss sharing until production concurrency |
| Iterator | `yield return` holds stream open for enumeration lifetime | Writer blocked for entire foreach duration |

**Fix (priority order):**

1. Tailer: `new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)`.
2. Writer: `new StreamWriter(new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite), appendEncoding) { AutoFlush = true }` or equivalent append pattern.
3. For live tail, re-open or seek-from-end patterns with periodic reopen on `IOException` — logs rotate.
4. Prefer structured logging sinks (Serilog file sink with shared flag) instead of hand-rolled tail+append.

```csharp
using FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
using StreamReader reader = new StreamReader(fs, leaveOpen: false);
```

**Production takeaway:** Text wrappers do not remove OS file-lock rules — Karat tests whether you know when to bypass path ctors and configure `FileShare`. Preview **Program.cs** Section 2d / 3f — `FileStream` then `StreamReader`/`StreamWriter` chain.

---

---

#### Q6. (P) An ASP.NET Core hosted service ingests a growing feed file every few seconds. A developer keeps sync I/O "because the file is local":

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using StreamReader reader = new StreamReader(_feedPath);

    while (!stoppingToken.IsCancellationRequested)
    {
        string? line = reader.ReadLine(); // blocks thread pool thread
        if (line == null)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            continue;
        }

        await _processor.HandleLineAsync(line, stoppingToken);
    }
}
```

What breaks under hosting pressure, and what is the production-grade read loop?

---

**Answer:**

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using StreamReader reader = new StreamReader(_feedPath);

    while (!stoppingToken.IsCancellationRequested)
    {
        string? line = reader.ReadLine(); // blocks thread pool thread
        if (line == null)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            continue;
        }

        await _processor.HandleLineAsync(line, stoppingToken);
    }
}
```

What breaks under hosting pressure, and what is the production-grade read loop?

**Answer:** `ReadLine()` is synchronous — each call blocks a thread pool thread while waiting on disk I/O, which defeats the async hosting model and can contribute to thread-pool starvation when many background services do the same. Use `ReadLineAsync(stoppingToken)` (or `WaitToReadAsync` patterns on pipes) inside an async loop, combine with `FileShare.ReadWrite` if producers append, and reopen or track position when reaching EOF on a growing file.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | Sync `ReadLine()` in async host | Thread pool blocked during I/O waits |
| EOF handling | `null` line then `Delay` on static reader | Misses new lines appending at EOF unless reposition/reopen |
| Cancellation | Sync read ignores `stoppingToken` during block | Slow shutdown under load |

**Fix (priority order):**

1. Replace with `await reader.ReadLineAsync(stoppingToken)` in the loop.
2. When at EOF on a growing feed, flush writer side, optionally reopen file or track `_lastPosition` with `FileStream.Position`.
3. Mark the hosted service async end-to-end; avoid `.Result` on any related tasks.
4. Add metrics for lag (lines behind) and backoff when file is temporarily locked.

```csharp
await using StreamReader reader = new StreamReader(_feedPath);

while (!stoppingToken.IsCancellationRequested)
{
    string? line = await reader.ReadLineAsync(stoppingToken);
    if (line is null)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        continue;
    }

    await _processor.HandleLineAsync(line, stoppingToken);
}
```

**Production takeaway:** Local disk does not make I/O free — sync-over-async in hosted services is the same Karat trap as `.Result` in controllers. Prefer async stream APIs even for file reads.

---

---

#### Q7. (R) A tool rewrites the first line of a config file in place, then reads the remainder. After a refactor it throws `ObjectDisposedException`. Review:

```csharp
public void PatchConfigHeader(string path, string newHeader)
{
    using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
    using StreamWriter writer = new StreamWriter(fs, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
    using StreamReader reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);

    writer.WriteLine(newHeader);
    writer.Flush();

    fs.Seek(0, SeekOrigin.Begin);
    string remainder = reader.ReadToEnd();
}
```

Which dispose/ownership choices are wrong, and what is the correct pattern when wrapping the same `FileStream`?

---

**Answer:**

```csharp
public void PatchConfigHeader(string path, string newHeader)
{
    using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
    using StreamWriter writer = new StreamWriter(fs, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
    using StreamReader reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);

    writer.WriteLine(newHeader);
    writer.Flush();

    fs.Seek(0, SeekOrigin.Begin);
    string remainder = reader.ReadToEnd();
}
```

Which dispose/ownership choices are wrong, and what is the correct pattern when wrapping the same `FileStream`?

**Answer:** `StreamReader` is constructed with `leaveOpen: false` (the default), so disposing the reader **closes the shared `FileStream`** when the `using` block ends — before `Seek`/`ReadToEnd` if ordering were wrong, and any later use throws `ObjectDisposedException`. Both reader and writer must use `leaveOpen: true` when sharing one stream; dispose order should flush the writer, then dispose reader, then writer, then the stream — or avoid dual wrappers on one stream and read/write in separate phases with explicit positioning.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Ownership | `StreamReader(..., leaveOpen: false)` on shared `fs` | Reader dispose closes `fs` for everyone |
| Concurrency | Simultaneous reader/writer on same stream without clear protocol | Undefined buffering; corrupted reads |
| Design | In-place header patch via read/write same stream | Easy to truncate file if rewrite shorter than original |

**Fix (priority order):**

1. Set `leaveOpen: true` on **both** `StreamWriter` and `StreamReader`; dispose `fs` last explicitly.
2. Safer: read full content first, patch in memory, write to temp file, atomic replace — avoids length mismatch corrupting tail.
3. After writing header, `writer.Flush()` before reading; reset position with `fs.Seek` and optionally discard reader buffer (new reader instance).
4. Document that `StreamWriter` path ctor owns the stream unless you pass your own `FileStream`.

```csharp
using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8, leaveOpen: true))
{
    writer.WriteLine(newHeader);
    writer.Flush();
}
fs.Seek(0, SeekOrigin.Begin);
using (StreamReader reader = new StreamReader(fs, Encoding.UTF8, leaveOpen: true))
{
    _ = reader.ReadLine();
    string remainder = reader.ReadToEnd();
}
```

**Production takeaway:** Default `leaveOpen: false` means disposing the text wrapper closes the base stream — Karat tests layered I/O ownership called out in **Program.cs** Section 2d/3f FileStream chains.

---

---

#### Q8. (M) A cross-platform app parses `.env`-style files written on mixed developer machines (Windows CRLF, macOS/Linux LF). Review ingestion:

```csharp
public Dictionary<string, string> ParseEnvFile(string path)
{
    var map = new Dictionary<string, string>();
    using StreamReader reader = new StreamReader(path);

    string content = reader.ReadToEnd();
    foreach (string rawLine in content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
    {
        int eq = rawLine.IndexOf('=');
        if (eq <= 0) continue;
        string key = rawLine[..eq].Trim();
        string value = rawLine[(eq + 1)..].Trim();
        map[key] = value;
    }
    return map;
}
```

What breaks when files use CRLF or when keys are compared across environments, and how should line-based parsing use `StreamReader` instead?

---

### 03. FileStream & Binary Files

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/03. FileStream & Binary Files`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public Dictionary<string, string> ParseEnvFile(string path)
{
    var map = new Dictionary<string, string>();
    using StreamReader reader = new StreamReader(path);

    string content = reader.ReadToEnd();
    foreach (string rawLine in content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
    {
        int eq = rawLine.IndexOf('=');
        if (eq <= 0) continue;
        string key = rawLine[..eq].Trim();
        string value = rawLine[(eq + 1)..].Trim();
        map[key] = value;
    }
    return map;
}
```

What breaks when files use CRLF or when keys are compared across environments, and how should line-based parsing use `StreamReader` instead?

**Answer:** Splitting on `'\n'` alone leaves a trailing `'\r'` on keys when the file uses CRLF — `map["HOST"]` misses lookups for `"HOST\r"`. `ReadToEnd()` also reintroduces the large-file memory trap. Loop with `ReadLine()`, which strips platform newlines (`\r\n` or `\n`) uniformly, trim keys/values defensively, and use ordinal key comparison; skip comments with `#` per line instead of splitting the whole file.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Newlines | `Split('\n')` on CRLF content | Keys include `\r` — silent config misses in Linux-deployed apps |
| Memory | `ReadToEnd()` for config | Unbounded allocation if `.env` grows or includes generated blocks |
| Parsing | `RemoveEmptyEntries` | Skips intentional blank lines vs comments — may hide format errors |

**Fix (priority order):**

1. Replace bulk split with `while ((line = reader.ReadLine()) != null)` — `ReadLine` removes `\r\n`/`\n` per **Program.cs** Section 3.
2. `key = key.Trim('\r', ' ', '\t')` defensively if content comes from external tools.
3. Use `StringComparer.OrdinalIgnoreCase` only if spec requires case-insensitivity — document choice.
4. For deployment, normalize line endings in repo via `.gitattributes` — but runtime parsing must still tolerate CRLF.

```csharp
while (reader.ReadLine() is { } line)
{
    line = line.Trim();
    if (line.Length == 0 || line.StartsWith('#')) continue;
    int eq = line.IndexOf('=');
    if (eq <= 0) continue;
    map[line[..eq].Trim()] = line[(eq + 1)..].Trim();
}
```

**Production takeaway:** Cross-platform text bugs often show up as `\r`-poisoned keys, not mojibake — Karat expects `ReadLine` semantics vs manual split. See **Program.cs** QUICK REFERENCE — prefer line loop over whole-file helpers for scalable parsing.

---

### 03. FileStream & Binary Files

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/03. FileStream & Binary Files`

---

---

#### Q1. (R) A telemetry service reads a fixed 4-byte file signature from `signature.bin`. In production, short files produce garbage signatures without throwing. Review the reader:

```csharp
public static string ReadFileSignature(string path)
{
    byte[] buffer = new byte[4];

    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    stream.Read(buffer, 0, buffer.Length); // signature must be exactly 4 bytes

    return Encoding.ASCII.GetString(buffer);
}
```

What is wrong, and how would you harden this for truncated or partially written files?

---

**Answer:**

```csharp
public static string ReadFileSignature(string path)
{
    byte[] buffer = new byte[4];

    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    stream.Read(buffer, 0, buffer.Length); // signature must be exactly 4 bytes

    return Encoding.ASCII.GetString(buffer);
}
```

What is wrong, and how would you harden this for truncated or partially written files?

**Answer:** `FileStream.Read` may return fewer bytes than requested — especially near EOF or on a file still being written — but the code ignores the return value and decodes the entire buffer, padding with `\0` or stale bytes. You must loop until you have 4 bytes or confirm EOF, and treat short reads as corrupt input.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Return value of `Read` ignored | Partial buffer decoded as full signature |
| Runtime | No EOF / short-file handling | Silent garbage strings instead of explicit failure |
| Concurrency | `FileShare.Read` while writer may still flush | Reader sees pre-flush or truncated file |

**Fix (priority order):**

1. Loop reads until `totalRead == 4` or `Read` returns 0 — throw `InvalidDataException` if fewer than 4 bytes after EOF.
2. Prefer `BinaryReader.ReadBytes(4)` when you need an exact count — it throws `EndOfStreamException` on short input (see **Program.cs** Section 9).
3. If another process writes the file, coordinate with `FileShare.ReadWrite` on the writer and validate magic before trusting content.
4. Decode only the bytes actually read: `Encoding.ASCII.GetString(buffer, 0, totalRead)`.

```csharp
int totalRead = 0;
while (totalRead < buffer.Length)
{
    int n = stream.Read(buffer, totalRead, buffer.Length - totalRead);
    if (n == 0) break;
    totalRead += n;
}
if (totalRead != buffer.Length)
    throw new InvalidDataException($"Expected 4 signature bytes, got {totalRead}.");
```

**Production takeaway:** Karat embeds the **Program.cs** Section 5 rule — always check bytes returned — inside realistic signature-reading code. A single `Read` call is not a contract for a full buffer.

---

---

#### Q2. (R) A background job appends binary audit records while a dashboard process tries to read the same file. The writer opens like this; the reader gets `IOException: The process cannot access the file`:

```csharp
// Writer (audit service)
using var stream = new FileStream(
    auditPath, FileMode.Append, FileAccess.Write, FileShare.None);

// Reader (dashboard — runs concurrently)
using var readStream = new FileStream(
    auditPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
```

What locking mismatch causes the failure, and what `FileShare` flags should each side use?

---

**Answer:**

```csharp
// Writer (audit service)
using var stream = new FileStream(
    auditPath, FileMode.Append, FileAccess.Write, FileShare.None);

// Reader (dashboard — runs concurrently)
using var readStream = new FileStream(
    auditPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
```

What locking mismatch causes the failure, and what `FileShare` flags should each side use?

**Answer:** The writer holds an exclusive lock with `FileShare.None`, so no other process can open the file — even for read — until the handle is disposed. For concurrent append + read, the writer must allow shared read access (`FileShare.Read`) while the reader opens with `FileShare.ReadWrite` so both can coexist.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Locking | Writer uses `FileShare.None` | Exclusive lock blocks dashboard reader |
| Design | Long-lived writer handle (service loop) | File stays locked for entire process lifetime |
| Correctness | Reader assumes `ReadWrite` share fixes writer side | Share flags must match on **both** open calls |

**Fix (priority order):**

1. Change writer to `FileShare.Read` (or `FileShare.ReadWrite` if multiple writers are coordinated): `new FileStream(auditPath, FileMode.Append, FileAccess.Write, FileShare.Read)`.
2. Keep reader as `FileMode.Open, FileAccess.Read, FileShare.ReadWrite`.
3. Ensure writer `Flush()` / dispose runs periodically if readers need fresh tail bytes — buffered appends may not be visible until flush.
4. For high-concurrency append, consider one writer process or a queue instead of many exclusive handles.

**Production takeaway:** `FileShare` is negotiated at open time — the most restrictive combination wins. **Program.cs** Section 4 shows `FileShare.None` for exclusive writes and `FileShare.Read` for concurrent readers; production append+tail-read patterns need the writer to grant share.

---

---

#### Q3. (R) A teammate ports `inventory.bin` readers from another language and swaps field order on one record type. The file opens fine but prices and names are nonsense after the first record. Review:

```csharp
for (int i = 0; i < recordCount; i++)
{
    int id = reader.ReadInt32();
    double price = reader.ReadDouble();   // was written as int32 + string + double + bool
    string name = reader.ReadString();
    bool inStock = reader.ReadBoolean();
    results[i] = new ProductRecord(id, name, price, inStock);
}
```

What breaks, why does corruption spread to later records, and how do you detect or recover safely?

---

**Answer:**

```csharp
for (int i = 0; i < recordCount; i++)
{
    int id = reader.ReadInt32();
    double price = reader.ReadDouble();   // was written as int32 + string + bool
    string name = reader.ReadString();
    bool inStock = reader.ReadBoolean();
    results[i] = new ProductRecord(id, name, price, inStock);
}
```

What breaks, why does corruption spread to later records, and how do you detect or recover safely?

**Answer:** Binary files have no field names — the reader consumes bytes in strict write order. Reading `double` where a length-prefixed `string` was written misaligns the stream pointer, so every subsequent field and record parses garbage until `EndOfStreamException` or absurd values appear.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Read order ≠ write order (`id`, `name`, `price`, `bool`) | First record wrong; cursor permanently offset |
| Serialization | Treating on-disk layout like struct memory layout | Language ports assume field order matches CLR struct |
| Recovery | No per-record checksum or length guard | One mismatch corrupts entire remainder of file |

**Fix (priority order):**

1. Restore exact write order from **Program.cs** `BinaryInventoryCodec`: `ReadInt32` → `ReadString` → `ReadDouble` → `ReadBoolean`.
2. Validate magic header and `recordCount` before the loop; cap `recordCount` against `stream.Length` to reject absurd headers (corrupt/truncated files).
3. Add optional per-record length prefix or CRC if you need partial recovery — without it, fail fast on first parse anomaly.
4. Never use `StructLayout` / `Marshal.StructureToPtr` interchangeably with `BinaryWriter` unless you explicitly define packing and endianness.

**Production takeaway:** BinaryReader mismatches are not localized bugs — one wrong primitive shifts the cursor for all following data. Magic bytes (**Program.cs** Section 3) catch wrong file types; they do not catch wrong field order within the right file.

---

---

#### Q4. (R) A log-rotation utility reads the last 8 bytes of a growing file to verify a footer magic. It intermittently returns wrong bytes under load. Review:

```csharp
public static byte[] ReadFooter(string path)
{
    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    stream.Seek(0, SeekOrigin.End);           // jump to end
    byte[] footer = new byte[8];
    stream.Read(footer, 0, footer.Length);
    return footer;
}
```

What Position/Seek mistakes are here, and what else should you validate before trusting the footer?

---

**Answer:**

```csharp
public static byte[] ReadFooter(string path)
{
    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    byte[] footer = new byte[8];
    stream.Seek(0, SeekOrigin.End);           // jump to end
    stream.Read(footer, 0, footer.Length);
    return footer;
}
```

What Position/Seek mistakes are here, and what else should you validate before trusting the footer?

**Answer:** `Seek(0, SeekOrigin.End)` moves to EOF — **after** the last byte — not to the start of an 8-byte footer. The subsequent `Read` then pulls bytes from beyond the file (zeros/partial read) or fails silently depending on length. You need a negative offset from the end, e.g. `Seek(-8, SeekOrigin.End)`, and must handle files shorter than 8 bytes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Seek(0, End)` ≠ "last N bytes" | Reads past EOF; footer never matches |
| Edge case | No `Length < 8` guard | Short files produce partial/garbage footers |
| Concurrency | `FileShare.Read` while appender grows file | Footer position shifts between Seek and Read |

**Fix (priority order):**

1. Replace with `stream.Seek(-footer.Length, SeekOrigin.End)` — pattern from **Program.cs** `ReadLastTwoBytes` (`Seek(-2, SeekOrigin.End)`).
2. If `stream.Length < footer.Length`, throw or return a explicit failure — do not read.
3. Check `Read` return value or use `ReadBytes(8)` when you require exactly 8 bytes.
4. If the file is actively appended, re-read or use a stable snapshot (copy, or open with coordinated share + retry).

**Production takeaway:** `SeekOrigin.End` offsets are relative to EOF — zero means "after the last byte," not "the last byte." Karat tests whether you can translate "read tail" into signed Seek math.

---

---

#### Q5. (P) Your .NET service writes `metrics.bin` consumed by a Linux C tool on big-endian ARM. A developer uses default `BinaryWriter`/`BinaryReader` for `int` and `double` fields. Locally on x64 Windows everything works; in staging the C tool reads garbage. What is the root cause, and how do you design a cross-platform binary layout?

---

**Answer:**

**Answer:** `BinaryWriter`/`BinaryReader` use the platform's native little-endian layout for multi-byte primitives on typical x64 Windows — the ARM C consumer expects big-endian (network) byte order, so numeric fields decode incorrectly even when field order and sizes match.

- Document an explicit wire format: field order, fixed sizes, and **endianness** (usually big-endian for cross-language files).
- Write primitives with explicit byte reversal (`BinaryPrimitives.WriteInt32BigEndian`) or a known serializer (Protocol Buffers, MessagePack) instead of assuming CLR defaults match C `struct` memory.
- Do not confuse **struct memory layout** (`StructLayout`, padding, alignment) with **BinaryWriter** layout — they are unrelated unless you carefully marshal.
- Add a version byte and magic header; integration-test round-trip with the C reader in CI on both endian platforms.

**Production takeaway:** "Same language on dev machine" hides endianness and padding issues until the first cross-platform consumer. **Program.cs** notes little-endian on typical x64 — that is an assumption, not a portable protocol.

---

---

#### Q6. (D) A data pipeline must scan a 60 GB append-only binary archive for records matching a key — random access by fixed record index, not full sequential parse every time. A junior proposes `FileStream` + `Seek` per lookup; a senior suggests `MemoryMappedFile`. What are the trade-offs, and when would you still choose streaming?

---

**Answer:**

**Answer:** `MemoryMappedFile` maps file pages into virtual memory — excellent for repeated random access on large read-mostly files without loading 60 GB into a `byte[]`, and the OS caches hot regions efficiently. Pure `FileStream` + `Seek` per lookup works but pays more syscall overhead and does not leverage page cache as naturally for scattered access patterns.

- **Memory-mapped pros:** Fast indexed jumps when records are fixed-size or you maintain an offset index; multiple processes can share mapped views read-only; no manual buffer management for random reads.
- **Memory-mapped cons:** Less ideal for concurrent **writes** / append while mapped; address-space limits on 32-bit; careful handling of torn reads if writer appends without coordination; not a drop-in for variable-length records without an index.
- **Streaming pros:** Simpler lifecycle with `using`; natural for sequential export/import; better when records are variable-length and you must parse forward anyway; async `ReadAsync` pipelines for ETL.
- **Hybrid:** Build a sidecar index file (offset table) + mmap or seek; for one-pass full scan, sequential `FileStream` may be faster than millions of random seeks.

**Production takeaway:** Karat tests design judgment — mmap is not "always faster," but for **large, repeatedly indexed, read-heavy** binary archives it often beats naive per-lookup `Seek` on spinning disks and NVMe alike when record boundaries are known.

---

---

#### Q7. (R) An export worker writes large binary batches with async I/O, then signals a downstream processor via a message queue. The processor often reads zero-length or incomplete files. Review:

```csharp
public async Task ExportBatchAsync(string path, byte[] payload, CancellationToken ct)
{
    await using FileStream stream = new FileStream(
        path, FileMode.Create, FileAccess.Write, FileShare.Read);

    await stream.WriteAsync(payload, ct);
    // message published immediately after WriteAsync returns
    await _queue.PublishAsync(new BatchReadyMessage(path), ct);
}
```

What async/flush timing issue causes incomplete reads, and how do you fix it before publishing?

---

**Answer:**

```csharp
public async Task ExportBatchAsync(string path, byte[] payload, CancellationToken ct)
{
    await using FileStream stream = new FileStream(
        path, FileMode.Create, FileAccess.Write, FileShare.Read);

    await stream.WriteAsync(payload, ct);
    // message published immediately after WriteAsync returns
    await _queue.PublishAsync(new BatchReadyMessage(path), ct);
}
```

What async/flush timing issue causes incomplete reads, and how do you fix it before publishing?

**Answer:** `WriteAsync` returning means data reached the `FileStream` buffer — not necessarily the OS disk cache or a stable on-disk length visible to another process. Publishing immediately races the consumer, which may open the file before flush/dispose completes and see zero or partial content.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No `FlushAsync` / dispose before signal | Consumer reads truncated file |
| Timing | Message queue is faster than disk visibility | Intermittent "empty file" failures |
| API | `FileShare.Read` allows concurrent open | Reader succeeds but gets stale length |

**Fix (priority order):**

1. `await stream.FlushAsync(ct)` before publishing — mirrors **Program.cs** Section 7 (`writer.Flush(); stream.Flush()`).
2. Prefer `await using` scope: dispose (close handle) before enqueue so OS metadata reflects final length.
3. Optionally write to a temp path and atomically `File.Move` to the final path, then publish — consumer never sees a half-written target.
4. Consumer should retry with backoff on short reads, but the producer must not rely on that alone.

```csharp
await stream.WriteAsync(payload, ct);
await stream.FlushAsync(ct);
// await using dispose runs here — then publish
await _queue.PublishAsync(new BatchReadyMessage(path), ct);
```

**Production takeaway:** Async I/O does not remove flush semantics — **Program.cs** warns that `FileInfo.Length` may be stale until flush/dispose. Queue-based pipelines need flush + atomic rename, not just `WriteAsync`.

---

---

#### Q8. (R) A cache service tries to wipe and rewrite `cache.bin` in one handle. It throws at runtime despite the path existing. Review both open attempts:

```csharp
// Attempt A — "open existing and overwrite first byte"
using var readOnly = new FileStream(cachePath, FileMode.Open, FileAccess.Read);
readOnly.WriteByte(0xFF);

// Attempt B — "create fresh file but only pass Read access"
using var creator = new FileStream(cachePath, FileMode.Create, FileAccess.Read);
```

What `FileMode`/`FileAccess` mismatches cause each failure, and what is the correct combination for in-place rewrite (**Program.cs** Section 10 — Truncate pattern)?

---

### 04. Path & Environment Classes

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/04. Path & Environment Classes`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
// Attempt A — "open existing and overwrite first byte"
using var readOnly = new FileStream(cachePath, FileMode.Open, FileAccess.Read);
readOnly.WriteByte(0xFF);

// Attempt B — "create fresh file but only pass Read access"
using var creator = new FileStream(cachePath, FileMode.Create, FileAccess.Read);
```

What `FileMode`/`FileAccess` mismatches cause each failure, and what is the correct combination for in-place rewrite (**Program.cs** Section 10 — Truncate pattern)?

**Answer:** `FileAccess` must authorize every operation you perform — `Read` forbids `WriteByte`, and `FileMode.Create` with `FileAccess.Read` is an invalid combination that throws `ArgumentException` at construction. For in-place rewrite, open with write-capable access and use `FileMode.Truncate` or `ReadWrite` + explicit length reset.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Attempt A: `FileAccess.Read` + `WriteByte` | `NotSupportedException` at first write |
| API contract | Attempt B: `FileMode.Create` + `FileAccess.Read` | `ArgumentException` — Create requires Write or ReadWrite |
| Design | Using `Open` + write when file should be cleared first | Old bytes remain if you only overwrite first byte without truncating |

**Fix (priority order):**

1. For wipe-and-rewrite in place: `new FileStream(cachePath, FileMode.Truncate, FileAccess.Write, FileShare.None)` — **Program.cs** Section 10 sets length to 0 then writes.
2. If you need read-then-write in one session: `FileMode.OpenOrCreate` or `Open` with `FileAccess.ReadWrite`, then `SetLength(0)` or `Truncate` semantics before writing.
3. Match `FileMode` to intent: `Create` truncates existing path; `Append` seeks to end; do not pair write modes with read-only access.
4. Always pair with `using` / dispose so locks release after rewrite.

```csharp
using var stream = new FileStream(
    cachePath, FileMode.Truncate, FileAccess.Write, FileShare.None);
stream.WriteByte(0xFF);
stream.Flush();
```

**Production takeaway:** `FileMode` chooses **how the OS opens the path**; `FileAccess` gates **what this handle may do** — Karat stacks both in one snippet to see if you diagnose constructor vs first-write failures separately.

---

### 04. Path & Environment Classes

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/04. Path & Environment Classes`

---

---

#### Q1. (R) A report exporter works on Windows dev machines but fails on Linux CI with "Could not find a part of the path." Review this path builder:

```csharp
public string BuildExportPath(string customerId, string fileName)
{
    string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    return baseDir + "\\Reports\\" + customerId + "\\" + fileName;
}

// Called from a nightly job:
var path = BuildExportPath("CUST-42", "summary.csv");
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
await File.WriteAllTextAsync(path, csvContent);
```

What is wrong, and how do you fix it for cross-platform deployment?

---

**Answer:**

```csharp
public string BuildExportPath(string customerId, string fileName)
{
    string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    return baseDir + "\\Reports\\" + customerId + "\\" + fileName;
}

// Called from a nightly job:
var path = BuildExportPath("CUST-42", "summary.csv");
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
await File.WriteAllTextAsync(path, csvContent);
```

What is wrong, and how do you fix it for cross-platform deployment?

**Answer:** The method hard-codes Windows backslashes and assumes a user Documents folder exists on a headless CI agent — on Linux the concatenated path is invalid and `MyDocuments` may be empty or unsuitable for a server job.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cross-platform | `"\\"` string concatenation | Linux treats `\` as a valid filename character, not a separator — path does not resolve |
| Design | `SpecialFolder.MyDocuments` on a server/CI worker | No interactive user profile; base path may be empty or wrong |
| Maintainability | Manual join instead of `Path.Combine` | Every new segment repeats the separator mistake |

**Fix (priority order):**

1. Replace concatenation with `Path.Combine(baseDir, "Reports", customerId, fileName)`.
2. Do not use `MyDocuments` for server exports — read an configured output root from `IConfiguration` / environment variable (e.g. `/var/app/exports` or a mounted volume).
3. Validate `baseDir` is non-empty before `CreateDirectory`; fail fast with a clear configuration error in CI.
4. Sanitize `customerId` and `fileName` — reject path separators and `..` segments before combining.

```csharp
public string BuildExportPath(string outputRoot, string customerId, string fileName)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
    return Path.Combine(outputRoot, "Reports", customerId, fileName);
}
```

**Production takeaway:** Hard-coded backslashes pass on Windows dev boxes and fail immediately in Linux containers — Karat expects `Path.Combine` plus an explicit, configurable root instead of desktop assumptions. See **Program.cs** Section 1 and Section 8 — cross-platform rules.

---

---

#### Q2. (R) An internal admin API accepts a `fileName` query parameter and serves files from a fixed folder. Review the handler:

```csharp
private readonly string _storageRoot = Path.Combine(AppContext.BaseDirectory, "uploads");

public IResult Download(string fileName)
{
    string requested = Path.GetFullPath(Path.Combine(_storageRoot, fileName));
    if (!File.Exists(requested))
        return Results.NotFound();

    return Results.File(requested);
}

// Request: GET /download?fileName=..\..\appsettings.Production.json
```

What security and correctness issues exist, and what is the prioritized fix?

---

**Answer:**

```csharp
private readonly string _storageRoot = Path.Combine(AppContext.BaseDirectory, "uploads");

public IResult Download(string fileName)
{
    string requested = Path.GetFullPath(Path.Combine(_storageRoot, fileName));
    if (!File.Exists(requested))
        return Results.NotFound();

    return Results.File(requested);
}

// Request: GET /download?fileName=..\..\appsettings.Production.json
```

What security and correctness issues exist, and what is the prioritized fix?

**Answer:** `Path.GetFullPath` resolves `..` segments against `_storageRoot`, so a malicious `fileName` can escape the uploads folder and read arbitrary files on the server — the existence check does not confine access to the intended directory.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No verification that resolved path stays under `_storageRoot` | Path traversal — read secrets, configs, other tenants' files |
| Input | Unsanitized user-controlled `fileName` | `..`, absolute paths, alternate separators bypass intent |
| Correctness | `GetFullPath` alone is not a sandbox boundary | Developer assumes normalization equals authorization |

**Fix (priority order):**

1. Reject rooted paths and any segment containing `..` before combining — or use `Path.GetFileName(fileName)` if only flat files are allowed.
2. After resolving, verify the full path is prefixed by the normalized storage root (case-aware on Linux):

```csharp
string storageRoot = Path.GetFullPath(_storageRoot);
string requested = Path.GetFullPath(Path.Combine(storageRoot, fileName));

if (!requested.StartsWith(storageRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal)
    && !requested.Equals(storageRoot, StringComparison.Ordinal))
    return Results.BadRequest();

if (!File.Exists(requested))
    return Results.NotFound();
```

3. Prefer an opaque file id mapped server-side to a stored name instead of accepting raw path fragments from the client.
4. Log traversal attempts; return 400/404 without leaking whether the target file exists outside uploads.

**Production takeaway:** `GetFullPath` normalizes strings — it does not enforce trust boundaries. Always anchor to a known root and verify containment after resolution. See **Program.cs** Section 3 — GetFullPath pitfalls.

---

---

#### Q3. (P) A worker service loads `config/settings.json` with a relative path. It passes locally from Visual Studio but fails in production when started as a Windows Service or from a systemd unit. The startup code:

```csharp
var settingsPath = Path.GetFullPath("config/settings.json");
var json = await File.ReadAllTextAsync(settingsPath);
```

Logs show `Environment.CurrentDirectory` is `C:\Windows\System32` on the server but the project folder when debugging. What is happening, and what anchor should production code use instead?

---

**Answer:**

```csharp
var settingsPath = Path.GetFullPath("config/settings.json");
var json = await File.ReadAllTextAsync(settingsPath);
```

Logs show `Environment.CurrentDirectory` is `C:\Windows\System32` on the server but the project folder when debugging. What is happening, and what anchor should production code use instead?

**Answer:** Relative paths passed to `Path.GetFullPath` resolve against `Environment.CurrentDirectory`, which follows the process working directory set by the shell, service wrapper, or scheduler — not the folder containing the published assembly.

- Locally, the IDE sets CWD to the project directory, so `config/settings.json` is found next to source layout.
- Installed services and systemd units often start with CWD `/` or `System32`, so the same relative string points at the wrong tree.
- Production code should anchor content-relative assets to `AppContext.BaseDirectory` (or `IHostEnvironment.ContentRootPath` in ASP.NET Core), which tracks the deployed app folder.

```csharp
var settingsPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "config", "settings.json"));
```

- If settings live outside the publish folder (common for secrets), read an absolute path from configuration rather than assuming a relative layout.
- Document and test startup from a non-project CWD in CI to catch this class of bug early.

**Production takeaway:** Never assume `CurrentDirectory` equals the app install location — Karat pairs this with deployment context. See **Program.cs** Section 7 — CurrentDirectory vs BaseDirectory.

---

---

#### Q4. (P) A containerized API writes large PDF exports using `Path.GetTempFileName()` and never deletes them. After a few days in Kubernetes, pods hit `No space left on device`. The temp folder path is `/tmp` inside the container. What breaks in this pattern, and what production approach replaces `GetTempFileName`?

---

**Answer:**

**Answer:** `GetTempFileName` creates a zero-byte file immediately and returns its path, but the API replaces it with a large PDF without deleting the original or subsequent temps — ephemeral container `/tmp` (often a small `emptyDir` volume) fills up because nothing cleans up and each request adds another file.

- In containers, `Path.GetTempPath()` maps to `/tmp` unless overridden by `TMPDIR` — shared across requests in the same pod with no guaranteed recycle until the pod restarts.
- `GetTempFileName` is poor for large artifacts: it creates an extra file, uses predictable patterns, and encourages orphan leaks under load.
- Prefer streaming the response directly to the client, writing to a configured persistent volume, or using `IBlobStorage` / object storage for exports.
- If scratch space is required, combine `Path.GetTempPath()` with a unique name (`Guid`), wrap writes in `try/finally`, and delete in `finally`; consider `TemporaryFileStream` patterns or bounded pools.
- Set `TMPDIR` / `TEMP` / `TMP` explicitly in the deployment manifest to a sized volume when scratch I/O is unavoidable.
- Add disk-usage metrics and liveness checks — `/tmp` exhaustion kills all endpoints in the pod.

**Production takeaway:** Temp directories in containers are small and shared — treat them as bounded scratch space with explicit cleanup, not an export archive. See **Program.cs** Section 5 — GetTempPath / GetTempFileName cleanup note.

---

---

#### Q5. (R) A desktop-style feature is ported to a headless Linux server without changes:

```csharp
public string GetDefaultExportFolder()
{
    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    return Path.Combine(desktop, "MyApp", "Exports");
}

// Startup ensures folder exists:
Directory.CreateDirectory(GetDefaultExportFolder());
```

What fails on a server or container, and how should export location be chosen for server-side processing?

---

**Answer:**

```csharp
public string GetDefaultExportFolder()
{
    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    return Path.Combine(desktop, "MyApp", "Exports");
}

// Startup ensures folder exists:
Directory.CreateDirectory(GetDefaultExportFolder());
```

What fails on a server or container, and how should export location be chosen for server-side processing?

**Answer:** `SpecialFolder.Desktop` assumes an interactive user profile with a Desktop directory — on headless Linux servers or minimal container images the path is often empty or points under a non-writable home directory, causing `CreateDirectory` or later writes to fail.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Platform | Desktop folder on server/container | Path empty, missing, or not writable |
| Design | UI-centric SpecialFolder on backend | Wrong abstraction for batch/API exports |
| Operations | Silent reliance on user profile layout | Worked on developer workstation; fails in prod |

**Fix (priority order):**

1. Replace Desktop with a configured server path — environment variable, `appsettings`, or mounted volume (`/app/data/exports`).
2. For multi-tenant SaaS, use tenant-scoped storage (database blob, S3, Azure Blob) rather than local filesystem folders.
3. If user-specific exports are required on a desktop app, keep `SpecialFolder` — but gate server code paths separately.
4. Validate the chosen root exists and is writable at startup; surface a clear configuration error instead of failing mid-request.

```csharp
public string GetDefaultExportFolder(IConfiguration config)
{
    var root = config["Export:RootPath"]
        ?? Path.Combine(AppContext.BaseDirectory, "exports");
    return Path.Combine(root, "MyApp", "Exports");
}
```

**Production takeaway:** `SpecialFolder` values encode OS/user UI conventions — server workloads need explicit configuration, not Desktop. See **Program.cs** Section 6 — SpecialFolder and Section 8 — do not assume drive letters or desktop layout.

---

---

#### Q6. (M) A path helper builds log file locations from configuration segments. Review this method called on both Windows and Linux:

```csharp
public static string BuildLogPath(string configuredRoot, string appName, string logFile)
{
    // configuredRoot might be "/var/log", "logs", or "C:\\Logs" from appsettings
    return Path.Combine("ignored", "prefix", configuredRoot, appName, logFile);
}
```

What surprising result occurs when `configuredRoot` is an absolute Unix path (`/var/log`) or a Windows drive root (`C:\Logs`), and how should callers structure segments?

---

**Answer:**

```csharp
public static string BuildLogPath(string configuredRoot, string appName, string logFile)
{
    // configuredRoot might be "/var/log", "logs", or "C:\\Logs" from appsettings
    return Path.Combine("ignored", "prefix", configuredRoot, appName, logFile);
}
```

What surprising result occurs when `configuredRoot` is an absolute Unix path (`/var/log`) or a Windows drive root (`C:\Logs`), and how should callers structure segments?

**Answer:** When any segment after the first is rooted (starts with `/` on Unix or a drive/root on Windows), `Path.Combine` discards all prior segments — `"ignored"` and `"prefix"` are dropped, and the result resets to the rooted segment plus the remainder.

- `Path.Combine("ignored", "prefix", "/var/log", "MyApp", "app.log")` → `/var/log/MyApp/app.log` on Linux.
- `Path.Combine("ignored", "prefix", @"C:\Logs", "MyApp", "app.log")` → `C:\Logs\MyApp\app.log` on Windows.
- Developers expect `"ignored/prefix"` to prefix configured roots — it silently does not when the config value is absolute.
- Pass either all-relative segments under a known base, or treat an absolute configured root as the sole first argument: `Path.Combine(configuredRoot, appName, logFile)` without dummy prefixes.
- Document in configuration schema whether `LogRoot` must be relative (to `BaseDirectory`) or absolute — do not mix assumptions in one Combine chain.

**Production takeaway:** Rooted segments in `Path.Combine` reset the path — a common misconfiguration when appsettings contains absolute paths. See **Program.cs** Section 1 — Combine with rooted segment demo.

---

---

#### Q7. (D) Two services exchange file paths over a message queue. Service A (Windows) sends `D:\data\invoices\inv-001.pdf`. Service B (Linux) tries to open it and also needs a relative path for an audit log entry. A developer writes:

```csharp
string incoming = message.FilePath; // from Windows producer
string relative = Path.GetRelativePath(AppContext.BaseDirectory, incoming);
await File.ReadAllTextAsync(incoming);
```

What breaks on Linux, and what contract should replace raw absolute paths between services?

---

**Answer:**

```csharp
string incoming = message.FilePath; // from Windows producer
string relative = Path.GetRelativePath(AppContext.BaseDirectory, incoming);
await File.ReadAllTextAsync(incoming);
```

What breaks on Linux, and what contract should replace raw absolute paths between services?

**Answer:** Windows absolute paths are meaningless on Linux — `File.ReadAllTextAsync` fails because `D:\...` is not a valid path on Unix, and `Path.GetRelativePath` cannot produce a meaningful relative path across different roots or machines.

- `GetRelativePath` requires both paths to share a common base on the same machine; cross-OS absolute paths have no shared root.
- Message contracts should carry stable identifiers (blob URI, S3 key, file id, share-relative path) — not producer-local absolute paths.
- If both services mount the same network share, agree on a **share-relative** path (`invoices/inv-001.pdf`) and each service combines with its locally configured mount point via `Path.Combine(mountRoot, relativeKey)`.
- For audit logs, store the logical key or URI, not `GetRelativePath` output from foreign paths.
- Use object storage (HTTPS URL + auth) for cross-platform handoff; consumers download to their own temp scratch if local file access is required.

**Production takeaway:** File paths are not portable across OS or hosts — exchange logical keys or URIs and resolve locally. See **Program.cs** Section 8 — Windows drive roots vs Unix single-root layout.

---

---

#### Q8. (P) A build pipeline archives deeply nested test output on Windows agents. One test creates a folder tree exceeding 260 characters. Locally it works when long-path support is enabled; on a Linux agent the same code runs but a Windows-only integration test fails with `PathTooLongException`. What explains the platform difference, and what mitigations belong in the path-building code?

---

### 05. Working with CSV and Text Files

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/05. Working with CSV and Text Files`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Windows historically enforced `MAX_PATH` (260 characters) unless long-path awareness is enabled at OS and application level; Linux paths are typically limited by `PATH_MAX` (often 4096 bytes) and are far more permissive — so the same deeply nested tree exceeds Windows limits while Linux succeeds.

- Developer machines with Windows 10+ long-path policy and .NET long-path support may hide the bug until a default-config CI Windows agent runs.
- Linux CI passing does not prove Windows deployment safety when paths are built from repeated `Path.Combine` of user names, guids, and nested fixture folders.
- Mitigations: shorten segment names, hash long identifiers (`SHA256` folder name instead of full title), flatten output layout, use `\\?\` prefix only as a last resort on Windows with explicit long-path enablement.
- Read remaining length budget before creating nested dirs; fail early with a clear test message instead of `PathTooLongException` mid-run.
- Keep artifact roots shallow — `artifacts/{buildId}/{suite}/file.ext` rather than mirroring full source tree depth.
- In CI, run at least one Windows job without long-path overrides to match conservative production environments.

**Production takeaway:** Path length limits are OS- and policy-dependent — design folder layouts for the shortest common denominator (default Windows), not the most permissive agent. See **Program.cs** Section 8 — cross-platform comparison table.

---

### 05. Working with CSV and Text Files

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/05. Working with CSV and Text Files`

---

---

#### Q1. (R) A partner feed import worked in QA but mis-maps vendor names in production. Review this row parser used for every data line after the header:

```csharp
public static InventoryItem? ParseRow(string line, int lineNumber)
{
    string[] cols = line.Split(',', StringSplitOptions.TrimEntries);

    if (cols.Length != 5)
        return null;

    return new InventoryItem
    {
        Sku = cols[0],
        Name = cols[1],
        Quantity = int.Parse(cols[2]),
        UnitPrice = decimal.Parse(cols[3]),
        RestockedOn = string.IsNullOrEmpty(cols[4]) ? null : DateTime.Parse(cols[4]),
    };
}
```

Sample production row: `"Acme, Inc",Widget,10,9.99,2026-01-15`

What fails, and what is the prioritized fix?

---

**Answer:**

```csharp
public static InventoryItem? ParseRow(string line, int lineNumber)
{
    string[] cols = line.Split(',', StringSplitOptions.TrimEntries);

    if (cols.Length != 5)
        return null;

    return new InventoryItem
    {
        Sku = cols[0],
        Name = cols[1],
        Quantity = int.Parse(cols[2]),
        UnitPrice = decimal.Parse(cols[3]),
        RestockedOn = string.IsNullOrEmpty(cols[4]) ? null : DateTime.Parse(cols[4]),
    };
}
```

Sample production row: `"Acme, Inc",Widget,10,9.99,2026-01-15`

What fails, and what is the prioritized fix?

**Answer:** `Split(',')` treats the comma inside `"Acme, Inc"` as a delimiter, yielding six columns instead of five — SKU shifts into the name column and downstream fields mis-map silently when the count check is skipped or relaxed. Replace naive split with quote-aware parsing, use `TryParse` with `CultureInfo.InvariantCulture`, and return row-level errors with line numbers instead of throwing or returning null without context.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Parsing | `Split(',')` on quoted fields | Wrong column count and shifted SKU/name/qty mapping |
| Correctness | `int.Parse` / `decimal.Parse` / `DateTime.Parse` | One bad cell aborts the row via exception — or worse, if wrapped in catch-all, loses line context |
| Validation | `return null` on count mismatch | Silent drop with no operator-visible error for the `"Acme, Inc"` shape |
| Culture | Default-culture `Parse` | Locale-dependent decimal/date interpretation across servers |

**Fix (priority order):**

1. Parse with a quote-aware scanner (`SplitQuotedLine` from **Program.cs** Section 3) — commas inside double quotes stay in one field.
2. Validate `columns.Count == InventoryColumnCount` and emit `Line {n}: expected 5 columns, found {m}` — matches **Program.cs** Section 4.
3. Replace `Parse` with `TryParse(..., CultureInfo.InvariantCulture, ...)` for quantity, price, and optional date.
4. Keep required-field checks (`sku.Length == 0`) before building `InventoryItem`.

```csharp
var columns = CsvParsing.SplitQuotedLine(line);
if (columns.Count != CsvParsing.InventoryColumnCount)
{
    errors.Add($"Line {lineNumber}: expected 5 columns, found {columns.Count}.");
    continue;
}
```

**Production takeaway:** QA files without quoted commas hide the bug; partner exports with `"Acme, Inc"` expose it immediately — Karat tests whether you know Split is not CSV parsing.

---

---

#### Q2. (R) An export job writes inventory CSV on a German Windows server; a US warehouse tool rejects half the rows. Product names with accents arrive as `MÃ¼ller` when the US tool opens the file. Review the export path:

```csharp
public static void ExportInventory(string path, IEnumerable<InventoryItem> items)
{
    using var writer = new StreamWriter(path);
    writer.WriteLine("Sku,Name,Quantity,UnitPrice,RestockedOn");

    foreach (var item in items)
    {
        writer.WriteLine(string.Join(",",
            item.Sku,
            item.Name,
            item.Quantity.ToString(),
            item.UnitPrice.ToString("F2"),
            item.RestockedOn?.ToString("yyyy-MM-dd") ?? ""));
    }
}
```

Partner file snippet: `W-400,Acme spare,5,19,95,2026-02-15`

What breaks across environments, and how do you make the file portable?

---

**Answer:**

_Answer not found._

---

#### Q3. (R) A nightly import job loads a 400 MB ERP export. Review the service method:

```csharp
public ImportResult Import(string path)
{
    string[] lines = File.ReadAllLines(path);
    var items = new List<InventoryItem>();
    var errors = new List<string>();

    for (int i = 1; i < lines.Length; i++)  // skip header at index 0
    {
        try
        {
            items.Add(ParseRow(lines[i]));
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
        }
    }

    return new ImportResult(items, errors);
}
```

What production problems appear at scale, and what pattern from this chapter replaces `ReadAllLines`?

---

**Answer:**

```csharp
public ImportResult Import(string path)
{
    string[] lines = File.ReadAllLines(path);
    var items = new List<InventoryItem>();
    var errors = new List<string>();

    for (int i = 1; i < lines.Length; i++)  // skip header at index 0
    {
        try
        {
            items.Add(ParseRow(lines[i]));
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
        }
    }

    return new ImportResult(items, errors);
}
```

What production problems appear at scale, and what pattern from this chapter replaces `ReadAllLines`?

**Answer:** `ReadAllLines` allocates a string for every row plus a large array — on a 400 MB file that spikes memory, increases GC pressure, and can OOM a constrained worker. It also assumes line index 0 is always the header, skipping comment rows and blank lines incorrectly, and `catch (Exception)` drops line numbers from error messages. Stream line-by-line with `StreamReader`/`TextReader`, skip ignorable lines, consume the first data header explicitly, and collect per-line errors while continuing the import.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | `File.ReadAllLines` loads entire file | High heap use; OOM on large partner feeds |
| Correctness | `i = 1` hard-coded header skip | `#` comment or blank first lines shift every row mapping |
| Observability | `errors.Add(ex.Message)` | Operators cannot locate row 47,832 without line numbers |
| Resilience | Exception per row inside generic catch | Partial imports OK, but `Parse` throws stop row detail unless TryParse used |

**Fix (priority order):**

1. Replace with `using StreamReader reader = new StreamReader(path)` and `while ((line = reader.ReadLine()) != null)` — flat memory (**Program.cs** Section 4, QUICK REFERENCE).
2. Reuse `InventoryCsv.Import(TextReader)` logic: skip blanks/comments, consume header once, increment `lineNumber` for each physical line.
3. Use `TryParse` + structured errors (`Line {lineNumber}: invalid quantity '{text}'`) instead of exceptions for expected bad data.
4. Accept `TextReader` in the parser so the same code reads files, streams, and `StringReader` tests.

**Production takeaway:** `ReadAllLines` is fine for demos; production imports of multi-megabyte feeds should stream — Karat pairs this with row-level error reporting from the chapter's partial-import pattern.

---

---

#### Q4. (P) A drop-folder service uses `FileSystemWatcher` to import CSV as soon as a file appears in `\\share\inbound`. Operators report random "column count" errors and duplicate SKU rows. The handler:

```csharp
watcher.Created += (_, e) =>
{
    var (items, errors) = InventoryCsv.Import(new StreamReader(e.FullPath));
    _repository.UpsertAll(items);
};
```

What race conditions happen with partial writes, and how do you harden the watcher pipeline?

---

**Answer:**

```csharp
watcher.Created += (_, e) =>
{
    var (items, errors) = InventoryCsv.Import(new StreamReader(e.FullPath));
    _repository.UpsertAll(items);
};
```

What race conditions happen with partial writes, and how do you harden the watcher pipeline?

**Answer:** `Created` fires when the file is first allocated, often before the upstream copy finishes — `StreamReader` then reads a truncated file, producing short rows and column-count errors. Retries on the same growing file can also insert partial batches before the full file lands. Wait until the file size stabilizes, open with shared-read exclusion or move to a processing folder under an exclusive lock, then import once idempotently.

- **Stabilize before read:** Poll `FileInfo.Length` until unchanged for N seconds, or use a "ready" sentinel file (`inventory.csv.ready`) written after the main file completes.
- **Exclusive processing:** On pickup, `File.Move` to `processing\{guid}.csv` so no second watcher event imports the same path mid-copy.
- **Locking:** Open with `FileShare.Read` only after stable size; avoid writers still flushing — `StreamWriter` on the exporter should `Flush()` before rename (**Program.cs** Section 4a).
- **Idempotency:** Key imports by file hash + batch id so a duplicate `Created` event does not double-count SKUs (see Q5).
- **Error shape:** Surface parse errors with line numbers; do not call `UpsertAll` on a batch with critical structural failures unless business rules allow partial loads.

**Production takeaway:** FileSystemWatcher notifies early — production pipelines treat "file exists" as "file ready," never as equivalent.

---

---

#### Q5. (P) Re-running the same inbound file after a network blip must not double inventory counts. A developer adds a guard:

```csharp
public void ImportFile(string path)
{
    if (_repository.AnyImportedFrom(path))
        return;

    var (items, errors) = InventoryCsv.Import(new StreamReader(path));
    _repository.InsertAll(items);
    _repository.MarkImported(path);
}
```

Halfway through a 50k-row file the database throws; the operator fixes the DB and re-runs. What goes wrong, and how do you make the import idempotent with rollback?

---

**Answer:**

```csharp
public void ImportFile(string path)
{
    if (_repository.AnyImportedFrom(path))
        return;

    var (items, errors) = InventoryCsv.Import(new StreamReader(path));
    _repository.InsertAll(items);
    _repository.MarkImported(path);
}
```

Halfway through a 50k-row file the database throws; the operator fixes the DB and re-runs. What goes wrong, and how do you make the import idempotent with rollback?

**Answer:** If `MarkImported` runs only after full success, a mid-batch failure leaves no mark but may have inserted thousands of rows — a re-run duplicates them. If `MarkImported` runs before verification, a failed run blocks forever. Wrap the database work in a transaction, stage rows in a import-batch table keyed by file hash, and commit only when all rows validate — or delete-by-batch-id on failure before retry.

- **Transactional bulk insert:** `BEGIN TRANSACTION` → insert all items with `ImportBatchId` → commit; on any failure, rollback so re-run starts clean.
- **Two-phase mark:** Record batch as `Processing` before insert, flip to `Completed` on commit — retries detect `Processing`/`Failed` and either resume or rollback-by-batch-id first.
- **Idempotency key:** Hash file contents (`SHA256`) — `AnyImportedFrom` should check hash, not path, so renamed re-drops are detected.
- **Partial parse policy:** If `errors` is non-empty, decide upfront: fail entire batch (rollback) vs import valid rows — document which; do not silently mix without operator ack.
- **Line-level staging:** Insert into `StagingInventory` via streaming parser; merge into live table in one set-based statement inside the transaction.

```csharp
await using var tx = await _db.Database.BeginTransactionAsync(ct);
var batchId = await _staging.LoadAsync(items, fileHash, ct);
if (errors.Count > 0) { await tx.RollbackAsync(ct); return; }
await _repository.MergeFromStagingAsync(batchId, ct);
await _repository.CompleteBatchAsync(fileHash, ct);
await tx.CommitAsync(ct);
```

**Production takeaway:** Idempotent import means safe retry — both "no duplicate rows" and "failed run leaves no footprint"; a path-only guard satisfies neither after a partial failure.

---

---

#### Q6. (D) Your team must ingest partner CSV feeds with quoted commas, optional date columns, and occasional header renames (`SKU` vs `Sku`). One engineer proposes `CsvHelper`; another wants to extend the hand-rolled `SplitQuotedLine` from this chapter. When do you reach for each, and what are the trade-offs for a long-lived warehouse integration?

---

**Answer:**

**Answer:** Extend the hand-rolled parser when the format is narrow, stable, and you need zero dependencies and full control for learning or a single internal export shape — `SplitQuotedLine` plus invariant `TryParse` covers quoted commas and typed columns for fixed five-column inventory. Reach for **CsvHelper** when feeds vary (renamed headers, optional columns, class maps, multiple delimiters) and you want header binding, validation attributes, and RFC 4180 edge cases without maintaining parser state yourself.

- **Hand-rolled (this chapter):** Fixed schema (`InventoryColumnCount`), quote-aware scan, explicit row errors — minimal surface, easy to unit-test with `StringReader`, no package churn; you own multiline fields, alternate encodings, and every new partner quirk.
- **CsvHelper:** `[Name("SKU")]`, `ClassMap`, `MissingFieldFound`, culture options, async enumeration — faster to onboard new feeds; adds dependency and team must learn mapping API; still need staging, transactions, and idempotency around it.
- **Header drift:** Hand-rolled code often assumes first line equals `InventoryHeader` string — brittle. CsvHelper can map by index or name with case-insensitive matching; either way, validate required columns up front and fail with a clear "missing column SKU" message.
- **Hybrid:** CsvHelper for deserialization into DTOs, then domain validation (`sku` required, qty ≥ 0) in a service layer — keeps parser concerns separate from warehouse rules (**Program.cs** `InventoryItem` pattern).
- **When not to hand-roll:** Multiple partners, embedded newlines in fields, tab/pipe delimiters, or frequent spec changes — maintenance cost exceeds CsvHelper's learning curve.

**Production takeaway:** Parser choice is an integration lifecycle bet — fixed internal format favors transparent hand-rolled code; multi-partner feeds favor a library plus staging and idempotent merge either way.

---

---

#### Q7. (R) Two import workers occasionally corrupt the same nightly file. Review the concurrent access pattern:

```csharp
public async Task ImportAsync(string path, CancellationToken ct)
{
    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    using var reader = new StreamReader(stream);
    var (items, errors) = InventoryCsv.Import(reader);
    await _repository.BulkInsertAsync(items, ct);
}

// Hosted service starts two overlapping imports when backlog > 1:
_ = ImportAsync(latestFile, ct);
_ = ImportAsync(latestFile, ct);
```

Separately, a new feed omits the header row entirely. The parser assumes the first non-blank line is always the header. What fails under concurrency and header drift, and how do you fix both?

**Answer:**

```csharp
public async Task ImportAsync(string path, CancellationToken ct)
{
    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
    using var reader = new StreamReader(stream);
    var (items, errors) = InventoryCsv.Import(reader);
    await _repository.BulkInsertAsync(items, ct);
}

// Hosted service starts two overlapping imports when backlog > 1:
_ = ImportAsync(latestFile, ct);
_ = ImportAsync(latestFile, ct);
```

Separately, a new feed omits the header row entirely. The parser assumes the first non-blank line is always the header. What fails under concurrency and header drift, and how do you fix both?

**Answer:** Two workers reading the same file concurrently duplicate database inserts unless the import is idempotent — and if either process also opens with write sharing, interleaved reads can see inconsistent snapshots on some OS/network shares. Header-less feeds mis-map the first data row as column names, shifting every subsequent field. Serialize per-file processing, move files exclusively before import, and detect headers by column signature rather than blind "first line skip."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Two `ImportAsync` on same path | Duplicate SKU rows or race on `_repository` |
| File I/O | Default `FileStream` share mode | Writer/exporter may still hold lock; reader fails or sees partial content |
| Correctness | First non-blank line = header | Header-less feed imports garbage first row; silent wrong types |
| Orchestration | Fire-and-forget duplicate tasks | No single-owner guarantee for nightly drop |

**Fix (priority order):**

1. **Single consumer:** Queue file paths; one worker processes each file — or `File.Move` to `processing\{id}.csv` atomically so only one worker owns the path.
2. **Open read-only with explicit share:** `new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)` — document that exporters must finish before drop.
3. **Header detection:** If first row matches header pattern (`Sku,Name,...` case-insensitive) or column count/types fail (`int.TryParse` on col 2 fails), treat line as data and use known column order — or reject with "header row missing."
4. **Idempotency:** Combine with Q5 — file hash + batch id so a duplicate worker run does not double insert.
5. **Optional:** `FileShare.None` during move-then-import pipeline on local disk; on SMB shares, prefer copy-to-local-temp then import.

```csharp
if (!headerConsumed && LooksLikeHeader(line))
{
    headerConsumed = true;
    continue;
}
// if no header seen after policy check, use DefaultInventoryColumns map
```

**Production takeaway:** Concurrent import is a workflow bug first and a parsing bug second — exclusive file ownership plus header validation prevents both duplicate rows and shifted columns.

---
