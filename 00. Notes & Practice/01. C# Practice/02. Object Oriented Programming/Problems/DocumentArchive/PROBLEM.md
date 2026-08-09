---
module: 02. Object Oriented Programming
difficulty: Hard
chapters: 06 Abstract Classes, 06 Interfaces, 06 Explicit Interface, 09 Document Example
domain: LegalArchive
---

# Document Archive

Build a **.NET 8 console application from scratch** for a legal document archive using abstract base classes and multiple interfaces.

## Business context

Compliance stores PDFs and text memos. Some entries export to bytes; all support keyword search. Archive service processes mixed document types through shared abstractions.

## Definitions

**Abstract class `Document`**

- `Title` (string, non-empty), `CreatedUtc` (DateTime)
- Protected constructor validates title
- Abstract `string DocumentType { get; }`
- Abstract `string RenderPreview(int maxChars)` — truncate with `"..."` if longer than maxChars
- Virtual `int WordCount` → split `RenderPreview(int.MaxValue)` on whitespace; empty → 0

**`PdfDocument : Document`**

- `PageCount` (int > 0)
- `DocumentType` → `"PDF"`
- Preview → `"PDF:{Title} ({PageCount} pages)"` then truncate per rules

**`TextMemo : Document`**

- `Body` (string)
- `DocumentType` → `"MEMO"`
- Preview → first line of body or `"(empty)"` if whitespace; truncate per rules

**Interface `IExportable`**

- `byte[] Export()`

**Interface `ISearchable`**

- `bool ContainsKeyword(string keyword)` — case-insensitive ordinal contains on title + type-specific text

**`PdfDocument : Document, IExportable, ISearchable`**

- `Export()` → UTF-8 bytes of preview (max 200 chars)
- Search title + `"PDF"`

**`TextMemo : Document, IExportable, ISearchable`**

- Search title + body

**Explicit interface conflict (required)**

Add interface `IIndexedItem` with `string Title { get; }` meaning storage key (`"DOC-{CreatedUtc:yyyyMMdd}-{hash}"` style — any deterministic key format you document).

Implement explicitly on both concrete types so public `Title` remains human title.

**Class `ArchiveService`**

- `Add(Document doc)` — false if another doc with same public title exists (case-insensitive)
- `IReadOnlyList<Document> All`
- `List<byte[]> ExportAll(IExportable filter)` — export every stored doc that implements `IExportable` (cast safely)
- `List<Document> Search(string keyword)` — docs implementing `ISearchable` where `ContainsKeyword` true

## Demo Main

Add one PDF and one memo; search, export, print previews and explicit `IIndexedItem.Title` via cast.

## Constraints

- net8, explicit usings
- At least one explicit interface member for `IIndexedItem.Title`

## Non-goals

Real PDF libraries, database

## Evaluation

[EVALUATION.md](EVALUATION.md)
