---
module: 07. File Input & Outpout and Streams
difficulty: Hard
chapters: 05 Working with CSV and Text Files
domain: Warehouse
---

# Warehouse CSV Pipeline

Build a **.NET 8 console application from scratch** for quote-aware CSV import and escaped export of inventory rows.

## Business context

Warehouse receives nightly SKU feeds from vendors. Names may contain commas; prices must use invariant decimals. Bad rows should report line numbers while good rows still import.

## Definitions

**Class `StockLine`**

- `string Sku`, `string Name`, `int Quantity`, `decimal UnitPrice` — init properties

**Static class `CsvFormatting`**

- `string EscapeField(string value)` — quote-wrap when field contains `,` `"` `\r` or `\n`; double internal quotes
- `string BuildRow(params string[] fields)` — join with commas using `StringBuilder`

**Static class `CsvParsing`**

- `const int StockColumnCount = 4`
- `const string StockHeader = "Sku,Name,Quantity,UnitPrice"`
- `List<string> SplitQuotedLine(string line)` — minimal quote-aware scanner
- `bool IsIgnorableLine(string line)` — empty or starts with `#`

**Static class `StockCsvPipeline`**

- `(List<StockLine> Items, List<string> Errors) Import(TextReader reader)` —
  - Skip ignorable lines
  - First non-ignorable line = header (not imported)
  - Validate column count; empty SKU error; `TryParse` quantity/price with `InvariantCulture`; negative quantity error; collect errors with line numbers; continue on row failure
- `void Export(string path, IEnumerable<StockLine> items)` — UTF-8 no BOM `StreamWriter`, header row, invariant numeric formatting (`UnitPrice` as `F2`)

## Demo Main

1. Import clean CSV from temp file including one row `"Acme, Inc"` name with comma
2. Print imported SKUs and error count
3. Import mixed file with one bad quantity row — print errors, show partial import count
4. Export items to second file; read first data line back with StreamReader
5. Import same CSV via `StringReader` inline (one valid row)
6. Cleanup temp folder

## Constraints

- net8
- No third-party CSV libraries

## Non-goals

RFC 4180 multiline fields, Excel

## Evaluation

[EVALUATION.md](EVALUATION.md)
