---
module: 07. File Input & Outpout and Streams
difficulty: Medium
chapters: 01 File & Directory Operations, 03 FileStream & Binary Files
domain: Security / Media
---

# File Signature Scanner

Build a **.NET 8 console application from scratch** that detects file types by magic bytes at the start (and optional trailer) of files on disk.

## Business context

Upload screening must flag files whose extension says `.png` but header bytes do not match the PNG signature. Scan walks a folder tree without loading entire files.

## Definitions

**Static class `FileSignatureScanner`**

Known signatures (first bytes):

| Label | Bytes (hex) |
|-------|-------------|
| PNG | 89 50 4E 47 |
| PDF | 25 50 44 46 |

- `bool MatchesSignature(string filePath, byte[] signature)` — open `FileStream` read-only; read `signature.Length` bytes; compare using loop or `SequenceEqual`; return false if file shorter than signature
- `string? DetectType(string filePath)` — try PNG then PDF signatures; return label or null
- `IReadOnlyList<string> ScanFolder(string folderPath)` — `Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)`; for each file, if `DetectType` not null, add formatted string `"{relativePath} => {label}"` where relative path is file name only for demo simplicity

No `Console` in scanner.

## Demo Main

1. Create scan folder with: valid PNG header file (write bytes), valid PDF header file, random `.txt`
2. Print `DetectType` for each
3. Print `ScanFolder` results (expect 2 matches)
4. Demonstrate `MatchesSignature` false on text file
5. Delete scan folder

## Constraints

- net8
- Use `Read` return value — do not assume full buffer filled incorrectly

## Non-goals

Full MIME database, async scanning

## Evaluation

[EVALUATION.md](EVALUATION.md)
