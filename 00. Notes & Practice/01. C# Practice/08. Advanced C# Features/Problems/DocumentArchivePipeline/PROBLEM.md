---
module: 08. Advanced C# Features
difficulty: Hard
chapters: 06 C# 8 Features
domain: DocumentArchive
---

# Document Archive Pipeline

Build a **.NET 8 console application from scratch** that processes document metadata using C# 8 nullable reference types, switch expressions, indices/ranges, using declarations, and `??=`.

## Business context

A legal archive ingests document batches. Each item has optional footnotes, a kind discriminator, and a file name. The pipeline classifies storage paths, slices file extensions with ranges, writes summary lines to a temp file with a using declaration, and lazily initializes a cache dictionary.

## Definitions

**Enum `DocumentKind`**

- `Text`, `Markup`, `Binary`

**Class `DocumentMetadata`**

- `Id` (string, non-nullable)
- `Kind` (`DocumentKind`)
- `FileName` (string, non-nullable)
- `Notes` (string?, nullable optional footnote)

**Readonly struct `PageSpan`**

- `Start`, `Length` (int); `End => Start + Length`

**Class `ArchiveClassifier`**

- `string GetStorageFolder(DocumentMetadata doc)` — **switch expression** on `doc.Kind`: Text → `"text/"`, Markup → `"markup/"`, Binary → `"binary/"`, _ → `"other/"`
- `string GetExtension(string fileName)` — use **range** `fileName[(fileName.LastIndexOf('.') + 1)..]` when dot present; else empty string
- `string BuildSummaryLine(DocumentMetadata doc)` — include Id and Kind; append `" | "` + Notes only when Notes is not null/whitespace

**Class `ArchivePipeline`**

- Private `Dictionary<string, string>? _folderCache`
- `string ResolveFolderCached(DocumentMetadata doc)` — use `??=` to assign `_folderCache = new()` on first use; cache `doc.Id → folder` from classifier
- `void WriteSummaries(IReadOnlyList<DocumentMetadata> batch, string outputPath)` — **using declaration** `using StreamWriter writer = new(...)`; write one summary line per doc; auto-dispose at end of method scope

## Demo Main

1. Create batch of 3 documents (one with null Notes, mixed kinds).
2. Print storage folder and extension for each.
3. Call `WriteSummaries` to temp file path; read back and print lines.
4. Call `ResolveFolderCached` twice for same id; verify cache hit (same reference or print cached value).

## Constraints

- net8, explicit usings, `<Nullable>enable</Nullable>`
- No `#nullable disable` in student code
- Switch expression required (not classic switch statement) for folder mapping

## Non-goals

IAsyncEnumerable, default interface methods, ref struct disposables

## Evaluation

[EVALUATION.md](EVALUATION.md)
