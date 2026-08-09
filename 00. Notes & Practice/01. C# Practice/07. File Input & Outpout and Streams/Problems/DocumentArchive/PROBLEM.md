---
module: 07. File Input & Outpout and Streams
difficulty: Medium
chapters: 01 File & Directory Operations
domain: Legal / Document Management
---

# Document Archive

Build a **.NET 8 console application from scratch** that archives contract files into a dated folder tree using file-system APIs.

## Business context

Legal receives signed contract text files in an inbox folder. Each night they must land in `archive\{yyyy}\{MM}\` without overwriting existing archives, expose metadata for auditing, and support cleanup of empty staging folders.

## Definitions

**Class `DocumentArchiveService`**

- Constructor `(string inboxRoot, string archiveRoot)` — store roots (no I/O in ctor)
- `void EnsureInbox()` — `Directory.CreateDirectory` on inbox if missing
- `bool ArchiveDocument(string fileName)` —
  - Source: `Path.Combine(inboxRoot, fileName)`
  - If source missing → return `false`
  - Destination folder: `archiveRoot\{UtcNow:yyyy}\{UtcNow:MM}\`
  - Destination file: same file name; `File.Copy(..., overwrite: false)` — return `false` if destination already exists
  - On success return `true`
- `IReadOnlyList<string> ListInboxFiles()` — `Directory.GetFiles(inboxRoot)` returning file names only (`Path.GetFileName`)
- `long GetArchivedSize(string fileName)` — find newest copy under `archiveRoot` via `DirectoryInfo.EnumerateFiles(fileName, SearchOption.AllDirectories)`, return `FileInfo.Length` of first match or `-1` if none
- `void PurgeEmptyInbox()` — if inbox exists and has zero files and zero subdirectories, `Directory.Delete(inboxRoot, recursive: false)`

No `Console` calls in this class.

## Demo Main

1. Create temp inbox/archive under `AppContext.BaseDirectory`
2. Write two inbox files with `File.WriteAllText`
3. Archive each; attempt duplicate archive of same name → second returns false
4. Print inbox list and archived size for one file
5. Delete inbox files, call `PurgeEmptyInbox`, print whether inbox folder still exists
6. Delete archive tree recursively

## Constraints

- net8
- Use static `File`/`Directory` for one-off ops; `DirectoryInfo`/`FileInfo` where enumeration/metadata reused

## Non-goals

Database index, PDF parsing, cloud storage

## Evaluation

[EVALUATION.md](EVALUATION.md)
