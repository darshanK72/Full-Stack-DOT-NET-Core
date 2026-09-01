# 05. Working with CSV and Text Files — Interview Q&A

> Back to [Module Index](../INTERVIEW_QA.md)

---

## Q1. Why does `string.Split(',')` fail for CSV fields that contain commas?

**Concepts**
- `Split` splits on every comma, regardless of quoting
- RFC 4180: fields containing commas must be enclosed in double quotes
- Three tokens produced from `"Acme, Inc",42` instead of two
- Naïve split requires all fields to be free of commas — too restrictive for real data
- Quote-aware scanner needed for general-purpose CSV parsing

**Answer**

`string.Split(',')` splits a line on every comma character unconditionally — it has no concept of quoted fields. When a CSV field itself contains a comma, the RFC 4180 specification requires the field to be wrapped in double quotes: `"Acme, Inc",42`. Split sees this as three tokens — `"Acme"`, `" Inc"`, and `"42"` — instead of the correct two. This column count mismatch causes the downstream parser to map values to the wrong fields and ultimately produce a row error or, worse, silently accept garbage data. A quote-aware scan must toggle a flag when it encounters a double-quote character and ignore commas while the flag is set, correctly treating `"Acme, Inc"` as one field. The minimal quote-aware implementation handles commas inside quotes and doubled quotes (`""` → `"`) but not multi-line fields or alternate delimiters, which covers most real-world exports.

---

## Q2. What are the RFC 4180 field escaping rules for CSV?

**Concepts**
- Fields containing comma, double-quote, or newline must be quoted
- Internal double-quote characters doubled: `"` → `""`
- Fields without special characters written as-is (no quotes required)
- BOM-less UTF-8 as the standard encoding for interchange
- Trailing comma on a row adds an empty extra column

**Answer**

RFC 4180 defines three escaping rules: first, a field that contains a comma, a double-quote, or a newline must be enclosed in double-quote delimiters. Second, any double-quote character inside a quoted field must be doubled — `12" wrench` becomes `"12"" wrench"` — so the reader can distinguish a closing quote from an embedded one. Third, fields with no special characters are written as-is without quoting, though quoting them is also valid. A correct `EscapeField` implementation checks the field for any of the three special characters (`,`, `"`, `\r`, `\n`), wraps the whole value in double quotes if any are found, and replaces each internal `"` with `""`. A trailing comma at the end of a row adds an empty field — `"a,b,"` has three columns, not two — which is technically valid CSV but frequently accidental and causes column count validation failures in strict importers.

---

## Q3. Why use `StringBuilder` to assemble a CSV row instead of string concatenation?

**Concepts**
- String concatenation creates a new `string` object per field
- `StringBuilder` uses a resizable internal buffer — one allocation per row
- Performance: `N` string concatenations allocate `O(N²)` bytes total
- `string.Join(',', fields.Select(EscapeField))` as a readable alternative for short rows
- `StringBuilder` preferred when `EscapeField` is called inside a hot path loop

**Answer**

String concatenation in C# is immutable — each `+` or `+=` operation allocates a new `string` object containing the combined result, then the old strings become garbage. For a 20-column CSV row, building the row character-by-character via concatenation allocates 20 intermediate strings before the final result, totaling O(N²) bytes across the loop. `StringBuilder` maintains a single resizable internal buffer: each `Append` call writes into that buffer without creating intermediate strings, and a single final `ToString()` materializes the result. For 100,000-row exports, the difference between concatenation and `StringBuilder` is measurable — concatenation allocates hundreds of megabytes of intermediate strings that GC must collect, while `StringBuilder` allocates once per row. `string.Join(',', ...)` is a readable one-liner that internally uses `StringBuilder`, acceptable for rows assembled from an already-escaped array. Use `StringBuilder` directly when the escape logic is applied field-by-field inside the assembly loop.

---

## Q4. What is the line-by-line `StreamReader` import pattern, and why does it keep memory bounded?

**Concepts**
- `while ((line = reader.ReadLine()) != null)` — correct EOF detection
- One line string in memory at a time
- Memory proportional to the longest single line, not the file size
- Skip blank lines and comment lines without loading the rest of the file
- `ReadLineAsync` for async pipelines

**Answer**

The standard pattern opens a `StreamReader` and calls `ReadLine()` in a loop, checking for `null` (EOF) as the loop condition. Each call returns one line string, processes it, and allows the prior line to be garbage collected before the next call. This means memory footprint is bounded by the longest single line in the file — typically a few hundred bytes — regardless of whether the file is 1 KB or 1 GB. Contrast this with `File.ReadAllLines`, which allocates a `string[]` containing every line simultaneously. The pattern also enables early exit (stop after finding the first matching row), skipping lines without allocating them (check `IsIgnorableLine` before doing any further processing), and reporting row numbers in error messages (increment a counter per `ReadLine` call). In ASP.NET Core endpoints, `await reader.ReadLineAsync(ct)` frees the thread during each disk read, preventing thread-pool starvation under concurrent imports.

---

## Q5. Why accept `TextReader` in a CSV parser instead of `StreamReader` specifically?

**Concepts**
- `TextReader` as the abstract text-I/O interface
- `StringReader` implements `TextReader` for in-memory strings
- `StreamReader` implements `TextReader` for file- or stream-backed text
- Testability: pass `new StringReader(csvContent)` without disk access
- Same parser code handles files, HTTP responses, and in-memory strings

**Answer**

`StreamReader` is a concrete implementation of the `TextReader` abstract class. When a parser method accepts `TextReader`, callers pass any implementation: `new StreamReader(filePath)` for a file, `new StreamReader(httpResponse.GetResponseStream())` for an HTTP response body, or `new StringReader("header\nrow1\nrow2")` for an in-memory string in a unit test. The parsing logic itself — splitting lines, checking column count, parsing typed fields — is identical regardless of the source. Accepting `StreamReader` directly would force all unit tests to create temp files, adding filesystem dependency to every test and making the test suite fragile and slow. The `TextReader` parameter also communicates intent: this method reads characters sequentially and makes no assumptions about seeking or file-specific operations.

---

## Q6. How should a CSV importer handle the header row?

**Concepts**
- Header row consumed first after skipping blank and comment lines
- Not imported as a data record — just consumed and discarded (or used for column mapping)
- `headerConsumed` flag tracking whether the first data line has been seen
- Column-name-to-index mapping for flexible column ordering
- Exact column count validation against expected schema

**Answer**

The standard approach uses a `bool headerConsumed` flag initialized to `false`. The main loop skips blank lines and comment lines (lines starting with `#`) unconditionally. When the first non-ignorable line is encountered and `headerConsumed` is false, set it to `true` and continue — this line is consumed as the header without producing a data record. For importers that must handle flexible column ordering (columns can appear in any order from different partners), parse the header into a column-name-to-index dictionary, then look up each field's position by name rather than hardcoding index `columns[3]` for price. For simple fixed-schema imports where the column order is contractually guaranteed, simply consuming the header without parsing its content is sufficient. Always validate that the header row's column count matches the expected schema before processing data rows.

---

## Q7. Why must `int.TryParse` and `decimal.TryParse` use `CultureInfo.InvariantCulture` in CSV parsing?

**Concepts**
- `TryParse` without culture uses current thread culture
- German de-DE culture: decimal comma `12,99`, thousands dot `1.000`
- InvariantCulture: decimal point `12.99`, thousands comma `1,000`
- CSV files from partners typically use InvariantCulture (English) format
- `NumberStyles.Number` to allow thousands separators; `NumberStyles.Integer` for whole numbers only

**Answer**

`TryParse` called without an explicit culture argument uses `Thread.CurrentThread.CurrentCulture`, which is the OS locale of the machine running the code. On a German-configured server, `decimal.TryParse("12.99")` may interpret `"12.99"` as 1,299 (treating `.` as the thousands separator) and return `12.99` truncated, or simply fail — because German InvariantCulture uses `,` as the decimal separator. CSV files exchanged between systems typically use `CultureInfo.InvariantCulture` (English: `.` decimal separator, `,` thousands) regardless of where they were generated, which is the format that Excel exports when configured for English locale. Passing `CultureInfo.InvariantCulture` explicitly to `TryParse` makes the parsing behavior identical on all machines regardless of OS locale. Use `NumberStyles.Integer` for whole-number fields to reject decimal-formatted integers, and `NumberStyles.Number` for decimal fields to allow optional thousands separators.

---

## Q8. How do you handle an optional `DateTime` field in a CSV row that may be blank?

**Concepts**
- Blank field represented as empty string `""` after split
- `DateTime?` (nullable) to distinguish "no date" from a specific date
- `DateTime.TryParse` with `CultureInfo.InvariantCulture` and `DateTimeStyles.None`
- ISO 8601 `yyyy-MM-dd` format unambiguous across locales
- On parse failure: add error to error list and `continue` to next row

**Answer**

An optional date field in CSV should be represented as `DateTime?` (nullable). After splitting the row, trim the column string: if it is empty or whitespace, set the field to `null` without attempting to parse. If the string is non-empty, call `DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)` and check the return value. On `true`, set the nullable to `parsedDate`. On `false`, add a descriptive error to the error collection — `"Line {n}: invalid date '{text}'"` — and `continue` to the next row rather than aborting the entire import. For unambiguous interchange, require partners to use ISO 8601 format `yyyy-MM-dd`: the string `"01/02/2026"` is January 2 in the US locale and February 1 in the UK locale, but `"2026-01-02"` is always January 2.

---

## Q9. What is the partial import strategy with error collection?

**Concepts**
- `List<string> errors` accumulating error messages with line numbers
- `continue` after adding error to skip the bad row and process the rest
- Never `throw` on row-level validation failure — let the loop continue
- Return both the accepted items and the error list
- Batch rejection threshold: abort if error rate exceeds N% of rows

**Answer**

A partial import accumulates errors without aborting: for each invalid row — wrong column count, unparseable number, missing required field — a descriptive error message including the line number is added to a `List<string>` and the loop continues with the next row. After the loop, the method returns both the successfully parsed items and the error list, letting the caller decide whether to proceed (the 3 bad rows out of 50,000 are acceptable) or reject the batch (5% error rate is too high). This is far more useful than throwing on the first bad row, which stops the import at row 12 with no information about the remaining 49,988 rows. The caller can surface the error list in a validation report, log it for the data supplier to correct, and import the valid rows immediately. A practical enhancement is a configurable error threshold — abort and reject the entire file if more than N% of rows fail, preventing half-processed imports when the file is systematically malformed.

---

## Q10. How do you write a CSV export with `StreamWriter` and proper escaping?

**Concepts**
- UTF-8 no-BOM encoding for portable interchange
- `StreamWriter` wrapping `FileStream` for full `FileShare` control
- `Flush()` after all rows written, before `Dispose`
- Escape each field before appending to the row
- Numbers formatted with `InvariantCulture` to avoid locale-dependent separators

**Answer**

A correct CSV export opens a `StreamWriter` with `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` to produce UTF-8 without a BOM, which is the standard for CSV interchange — BOM confuses many parsers. Write the header row first with `writer.WriteLine("Sku,Name,Quantity,UnitPrice,RestockedOn")`. For each record, build the row using an `EscapeField` helper for every string field and format numeric fields with `InvariantCulture`: `item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture)`. Use `StringBuilder` and append a comma between fields rather than building one large interpolated string. Call `writer.Flush()` after the last row to push any buffered content before `Dispose` closes the handle. Wrapping the writer in `using` ensures `Flush()` and handle release happen on all paths including exceptions.

---

## Q11. What is the trailing comma problem in CSV, and how do you guard against it?

**Concepts**
- Trailing comma after the last field adds an empty column
- Column count from split is one more than expected
- Strict validation: reject rows where column count does not match schema
- Some exporters intentionally include trailing commas — lenient mode skips empty final field
- `TrimEnd(',')` is not a general fix — may trim a legitimately empty last field

**Answer**

A row ending with a comma like `"W-100,Widget,50,12.99,"` produces six tokens when split on comma, one more than the expected five columns — the sixth is an empty string representing the "trailing" field. A strict importer that checks `columns.Count != expectedCount` correctly rejects this row. However, some legacy exporters intentionally write trailing commas as part of their format, producing files that always have one extra empty column. For such partners, the validation should be `columns.Count != expectedCount` where `expectedCount` is six (header has five named columns plus one empty), or the importer should trim trailing empty columns before counting. Do not use `line.TrimEnd(',')` as a general fix — if the last legitimate column is intentionally empty (a blank optional date), trimming the comma removes that empty value and shifts all subsequent parsing. The correct approach is to document the partner's format and handle it explicitly in a partner-specific importer configuration.

---

## Q12. How do you handle empty required fields in a CSV row?

**Concepts**
- `columns[0].Trim()` to get the field value after split
- Empty string as a valid field in CSV — not the same as missing
- Required field validation: empty value is an error, not a null to ignore
- Error message with line number: `"Line {n}: SKU is required (empty first column)"`
- `continue` to keep processing subsequent rows

**Answer**

After splitting and trimming a row's columns, required fields must be validated for non-emptiness explicitly — an empty string after trim means the column is present but blank. For a SKU field that must not be empty: `if (sku.Length == 0) { errors.Add($"Line {lineNumber}: SKU is required."); continue; }`. This is distinct from a missing column (wrong column count), which is validated earlier in the pipeline. The empty-required-field check must run before any downstream parsing that depends on the field value, and must add a descriptive error with the line number so data suppliers can correct the specific row. Never treat an empty required field as a valid null — it is a data quality error that should be recorded and reported. Optional fields (like a date) are handled differently: empty is acceptable and mapped to `null` without adding an error.

---

## Q13. How does culture affect decimal parsing, and what is the "12,99 vs 12.99" problem?

**Concepts**
- German `de-DE` culture: decimal separator is `,`; thousands separator is `.`
- `InvariantCulture`: decimal separator is `.`; thousands separator is `,`
- `"12,99"` parsed with InvariantCulture: either fails or parses as 1,299 (thousands)
- `"12.99"` parsed with `de-DE` culture: either fails or parses as 12 (whole) ignoring `.99`
- Always use InvariantCulture for CSV interchange; document the format contract

**Answer**

The decimal parsing problem arises because different cultures assign different meanings to the comma and period characters in numeric strings. In German (`de-DE`), the decimal separator is `,` and the thousands separator is `.` — `"12,99"` means 12.99 and `"1.000"` means 1000. In `CultureInfo.InvariantCulture` (English), it is the reverse: `"12.99"` means 12.99 and `"12,99"` is ambiguous or would be interpreted as 1299 if thousands-separator parsing is enabled. A CSV file generated on a German-locale machine using `decimal.ToString()` (without specifying culture) writes `"12,99"`, and an InvariantCulture importer then either rejects it or silently imports 12 instead of 12.99, depending on `NumberStyles`. The fix is to contractually specify InvariantCulture in the format specification and enforce it on both writer and reader: `price.ToString("F2", CultureInfo.InvariantCulture)` on export, `decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out d)` on import.

---

## Q14. What is the `IsIgnorableLine` pattern, and what does it enable?

**Concepts**
- Skip empty lines (blank rows in the file)
- Skip comment lines (lines starting with `#`)
- Prevent empty lines and comments from being counted as data rows
- Line counter increments for every physical line including ignored ones
- Allows file metadata, batch IDs, and instructions in the file without breaking the parser

**Answer**

`IsIgnorableLine` is a small helper that returns `true` for lines that should be skipped without incrementing the data-row counter or triggering any validation. Typically it returns `true` for empty lines (`line.Length == 0`) and lines starting with `#` (the comment convention). Placing this check at the top of the main loop ensures that blank separator lines between batches, header comments containing generation timestamps, and instruction lines are silently consumed. The physical line counter must still increment for every line including ignored ones — so error messages like `"Line 14: invalid quantity"` point to the correct physical line in the file, making it easy for the data supplier to open the file in any editor and jump to that line. Without this pattern, blank lines produce column-count validation errors and the import fails on any file that includes natural whitespace.

---

## Q15. How does `StringReader` enable testing a CSV parser without touching the filesystem?

**Concepts**
- `StringReader` implements `TextReader` backed by a `string`
- `new StringReader(csvContent)` requires no file, no directory, no OS permissions
- Same `Import(TextReader reader)` method used in test and production
- Tests run fast and in parallel without file-locking concerns
- Enables testing of every edge case (bad rows, encoding, empty file) inline in the test

**Answer**

When the parser method accepts `TextReader`, a unit test can pass `new StringReader(csvString)` where `csvString` is the complete inline CSV content as a C# string literal. No temporary files are created, no directories are needed, and the test runs without any OS permissions for filesystem access. The same `InventoryCsv.Import(reader)` method call processes the in-memory string through exactly the same code path as a production `StreamReader` over a real file, because `StringReader` implements every `TextReader` method identically. This allows testing every edge case — a row with wrong column count, an invalid price format, a date-format mismatch, an empty SKU, a file with only a header and no data rows — as fast, isolated, in-process tests that run in milliseconds. The alternative (creating a temp file per test) is slow, requires cleanup, can fail on read-only CI filesystems, and adds test infrastructure complexity for no benefit.

---

## Q16. What are the limitations of a minimal quote-aware CSV scanner vs a full RFC 4180 parser?

**Concepts**
- Minimal scanner handles commas inside quotes and doubled-quote escaping
- Does not handle multi-line quoted fields (newlines inside a quoted field spanning rows)
- No alternate delimiter support (tab, semicolon)
- No Unicode normalization or BOM stripping
- Third-party libraries (CsvHelper) for full RFC 4180 compliance including multi-line fields

**Answer**

A minimal quote-aware scanner — a char-by-char loop that toggles an `inQuotes` flag on `"` and treats commas inside quotes as part of the field — correctly handles the two most common cases: fields containing commas and fields containing doubled internal quotes. It does not handle fields containing literal newlines, which are valid in RFC 4180 but require the parser to accumulate characters across multiple `ReadLine()` calls before ending the record. It also does not handle alternate delimiters (tab-separated, semicolon-separated), BOM stripping, Unicode normalization, or the edge case of a quote appearing mid-field that is not a doubled escape. For production CSV exchange with external partners whose format is not fully controlled, a tested third-party library like `CsvHelper` is preferable because it handles all RFC 4180 edge cases, provides column mapping, handles encoding detection, and has a large community that has surfaced and fixed these edge cases over years of use.

---

## Q17. (Gotcha) What happens when `decimal.Parse` is used instead of `decimal.TryParse`?

**Concepts**
- `Parse` throws `FormatException` on any invalid format string
- One bad row aborts the entire import loop
- `TryParse` returns `false` without throwing — allows error collection and `continue`
- Production imports must never stop on one bad row when thousands of good rows follow
- `FormatException` at row 12 leaves rows 13–100,000 unprocessed

**Answer**

`decimal.Parse("not-a-price")` throws `FormatException` immediately, which propagates up the call stack unless caught. In a loop that imports 100,000 rows, a single malformed price field at row 12 throws and exits the loop, leaving 99,988 rows unprocessed. If the exception is not caught, the entire import transaction may roll back, discarding the 11 rows already accepted. `decimal.TryParse("not-a-price", ..., out decimal d)` returns `false` and sets `d` to 0 without throwing — the caller checks the return value, adds an error message with the line number to the error list, and calls `continue` to process the next row. This partial-import pattern is the only acceptable approach for high-volume imports where a handful of bad rows are a normal quality issue, not a reason to discard an entire batch.

---

## Q18. (Gotcha) Why does reading a 500 MB CSV with `File.ReadAllText` for line counting cause `OutOfMemoryException`?

**Concepts**
- `ReadAllText` materializes the entire file as one `string` on the LOH
- 500 MB string = one LOH allocation that cannot be compacted by GC
- Multiple concurrent requests hold multiple 500 MB allocations simultaneously
- `StreamReader.ReadLine()` loop counts lines with O(1) memory per line
- `File.ReadLines(path).Count()` as a clean one-liner alternative with lazy reading

**Answer**

`File.ReadAllText` reads the entire file into a single `string` object. A 500 MB CSV allocates a 500 MB object on the Large Object Heap, which is not compacted by the standard GC. Under concurrent load with three requests each reading a different 500 MB file, the total LOH allocation exceeds 1.5 GB, and `OutOfMemoryException` terminates the request — or the entire worker process. For line counting, the correct approach is a `StreamReader` in a `while ((line = reader.ReadLine()) != null) lineCount++;` loop, which holds at most one line string in memory at a time. Alternatively, `File.ReadLines(path).Count()` uses a lazy iterator that streams one line at a time and terminates the file handle after counting. The total memory usage is bounded by the longest single line — typically a few hundred bytes — regardless of file size.

---

## Q19. (Scenario R) Inventory import uses `string.Split(',')` — a product name `"Bolt, M6x1.0"` splits into 3 tokens, the wrong field lands in quantity, and `decimal.Parse` throws, aborting the entire import.

**Defective code:**

```csharp
public IEnumerable<InventoryItem> Import(string path)
{
    var items = new List<InventoryItem>();
    foreach (string line in File.ReadLines(path).Skip(1))  // skip header
    {
        string[] cols = line.Split(',');                    // BUG 1: splits on commas inside quotes
        string sku = cols[0];
        string name = cols[1];                              // BUG 2: wrong field if name has comma
        int qty = int.Parse(cols[2]);                       // BUG 3: throws if wrong column landed here
        decimal price = decimal.Parse(cols[3]);             // BUG 4: throws; aborts loop entirely
        items.Add(new InventoryItem { Sku = sku, Name = name, Quantity = qty, UnitPrice = price });
    }
    return items;
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Parsing | `Split(',')` breaks on quoted names containing commas — wrong column count | `cols[2]` gets `" M6x1.0"` instead of quantity; all subsequent fields shift |
| Error handling | `int.Parse` and `decimal.Parse` throw on any format error — no try/catch | One bad row aborts the entire import; all subsequent rows are lost |
| Resilience | No error collection — caller has no information about which rows failed | Entire import appears to succeed or fail as a whole; no partial import |

**Fix priority**

1. Replace `Split(',')` with a quote-aware `SplitQuotedLine` implementation.
2. Replace `int.Parse`/`decimal.Parse` with `TryParse` + `CultureInfo.InvariantCulture`.
3. Collect errors with line numbers and return both successful items and error list.
4. Validate column count before accessing specific indices.

```csharp
public (List<InventoryItem> Items, List<string> Errors) Import(TextReader reader)
{
    var items = new List<InventoryItem>();
    var errors = new List<string>();
    bool headerConsumed = false;
    int lineNumber = 0;
    string? line;
    while ((line = reader.ReadLine()) != null)
    {
        lineNumber++;
        if (line.Length == 0) continue;
        if (!headerConsumed) { headerConsumed = true; continue; }
        var cols = SplitQuotedLine(line);
        if (cols.Count != 4) { errors.Add($"Line {lineNumber}: expected 4 columns, got {cols.Count}."); continue; }
        if (!int.TryParse(cols[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int qty))
        { errors.Add($"Line {lineNumber}: invalid quantity '{cols[2]}'."); continue; }
        if (!decimal.TryParse(cols[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
        { errors.Add($"Line {lineNumber}: invalid price '{cols[3]}'."); continue; }
        items.Add(new InventoryItem { Sku = cols[0].Trim(), Name = cols[1].Trim(), Quantity = qty, UnitPrice = price });
    }
    return (items, errors);
}
```

---

## Q20. (Scenario R) Export service writes decimal prices with current thread culture (de-DE) — commas instead of dots break the downstream InvariantCulture parser.

**Defective code:**

```csharp
public void ExportInventory(string path, IEnumerable<InventoryItem> items)
{
    using var writer = new StreamWriter(path);
    writer.WriteLine("Sku,Name,Quantity,UnitPrice");
    foreach (var item in items)
    {
        // BUG: ToString() uses Thread.CurrentCulture — "12,99" on de-DE machine
        writer.WriteLine($"{item.Sku},{item.Name},{item.Quantity},{item.UnitPrice}");
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Culture | `item.UnitPrice` in string interpolation uses `Thread.CurrentCulture` | German OS writes `"12,99"` — downstream InvariantCulture importer fails on every price |
| Quoting | No field escaping — if `item.Name` contains a comma, the row is malformed | Column count mismatch for any product with a comma in the name |
| Encoding | `new StreamWriter(path)` defaults to UTF-8 without BOM — acceptable, but undocumented | Silent cross-platform encoding risk if the encoding contract is ever changed |

**Fix priority**

1. Format all numeric fields with `InvariantCulture`: `item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture)`.
2. Escape every string field with `EscapeField` before writing.
3. Document the encoding choice explicitly (UTF-8 no-BOM) in the format specification.

```csharp
using var writer = new StreamWriter(path, append: false,
    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
writer.WriteLine("Sku,Name,Quantity,UnitPrice");
foreach (var item in items)
{
    writer.WriteLine(string.Join(',',
        EscapeField(item.Sku),
        EscapeField(item.Name),
        item.Quantity.ToString(CultureInfo.InvariantCulture),
        item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture)));
}
```

---

## Q21. (Scenario R) CSV importer calls `decimal.Parse` (not `TryParse`) on price column — one bad row throws and aborts the entire 100,000-row import.

**Concepts**
- `decimal.Parse` throws `FormatException` on the first malformed price
- All remaining rows after the exception are never processed
- `TryParse` returns `false` and allows `continue` to the next row
- Error collection with line numbers enables partial import and supplier feedback

**Answer**

In a 100,000-row inventory import, calling `decimal.Parse(columns[3])` on every row means a single malformed value — `"N/A"`, `"TBD"`, or a culturally formatted `"12,99"` — throws `FormatException` and exits the method entirely. The 99,999 valid rows that follow are never processed. The fix replaces `decimal.Parse` with `decimal.TryParse(columns[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price)`, checks the return value, and on `false` adds a descriptive error to the errors collection: `"Line {n}: invalid unit price '{columns[3]}'"`. Then `continue` proceeds to the next row. After the loop, the caller receives both the successfully parsed items and the complete list of row-level errors, can log them for supplier correction, and can apply a threshold policy — reject the batch if more than 1% of rows fail, otherwise accept the partial import.

---

## Q22. (Scenario M) Scheduled report reads entire 500 MB CSV export with `File.ReadAllText` for line counting — OOM under concurrent requests.

**Defective code:**

```csharp
app.MapGet("/admin/reports/{id}/linecount", async (string id, IReportStore store) =>
{
    string path = store.GetCsvPath(id);
    string all = File.ReadAllText(path);               // BUG: 500 MB string per request
    int count = all.Split('\n').Length;                // BUG: another string[] allocation
    return Results.Ok(new { lineCount = count });
});
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Memory | `File.ReadAllText` allocates 500 MB LOH string per request | OOM under concurrent requests; GC pressure even under light load |
| Waste | Full file materialized to count lines — only newlines needed | 500 MB read + 500 MB split array for a single integer result |
| Threading | Synchronous `File.ReadAllText` blocks thread-pool thread | Thread-pool starvation under moderate concurrency |

**Fix priority**

1. Replace with `File.ReadLines(path).Count()` — lazy, O(1) memory per line.
2. Or use `StreamReader.ReadLine()` loop with `await ReadLineAsync(ct)` for async.
3. Cache the line count result if the file is not modified frequently.

```csharp
app.MapGet("/admin/reports/{id}/linecount", async (string id, IReportStore store, CancellationToken ct) =>
{
    string path = store.GetCsvPath(id);
    int count = 0;
    using var reader = new StreamReader(path);
    while (await reader.ReadLineAsync(ct) != null) count++;
    return Results.Ok(new { lineCount = count });
});
```

---

## Q23. (Scenario D) Multi-tenant CSV import must accept both InvariantCulture `"12.99"` and `de-DE` `"12,99"` price formats from different partners. How do you design a culture-aware import pipeline?

**Concepts**
- Per-tenant `CultureInfo` configuration stored in partner onboarding data
- Culture instance resolved before import begins — not from `Thread.CurrentCulture`
- `decimal.TryParse(text, NumberStyles.Number, partnerCulture, out decimal d)`
- Validation: reject files where the format does not match the registered culture
- Fallback strategy: try InvariantCulture first, then partner culture, report ambiguity

**Answer**

The correct design stores a `CultureInfo` (or its IETF language tag) in each partner's configuration record, resolved once at the start of each import job rather than sampled from the thread at parse time. The import pipeline loads the partner record, instantiates `CultureInfo partnerCulture = CultureInfo.GetCultureInfo(partner.NumericCultureTag)`, and passes that culture to every `TryParse` call throughout the session. For partners on InvariantCulture, `CultureInfo.InvariantCulture` is used directly. This makes the culture an explicit, auditable parameter of the import — it appears in logs and is included in import error reports. The fallback strategy for ambiguous values (where `"12,99"` could be InvariantCulture 1299 or de-DE 12.99) is to reject ambiguous rows and ask the partner to clarify the format, rather than silently choosing one interpretation. A smoke-test on the first 10 rows with both cultures and checking for plausibility (negative prices, implausibly large numbers) can catch misconfigured culture settings before processing the full file.

---

## Q24. (Scenario R) Export produces unquoted fields — product name `Widget "Pro" Edition` corrupts the CSV because internal quotes are not doubled.

**Defective code:**

```csharp
public string BuildCsvRow(InventoryItem item)
{
    // BUG: no escaping — quotes in name will break the CSV format
    return $"{item.Sku},{item.Name},{item.Quantity},{item.UnitPrice:F2}";
}
// item.Name = 'Widget "Pro" Edition' produces:
// SKU-1,Widget "Pro" Edition,50,12.99
// Readers see: SKU-1 | Widget "Pro" Edition (unclosed quote starts a new field)
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Format corruption | Unescaped `"` in a field starts a quoted section mid-value | CSV parser reads incorrect field boundaries — downstream parsing is corrupted |
| Silent failure | No exception thrown — corrupt row is written silently | Data corruption discovered only when the import fails on the reader side |
| Inconsistency | Some rows correct, some corrupted — depends on whether field values contain special chars | Intermittent failures that are hard to reproduce without the specific product name |

**Fix priority**

1. Apply `EscapeField` to every string field: wrap in double quotes if it contains `,`, `"`, `\r`, or `\n`; double any internal `"`.
2. Use `StringBuilder` + `EscapeField` per field rather than string interpolation.
3. Add a round-trip test: write a row with special characters, parse it back, verify the values match.

```csharp
public static string EscapeField(string value)
{
    if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    return value;
}

public string BuildCsvRow(InventoryItem item)
{
    return string.Join(',',
        EscapeField(item.Sku),
        EscapeField(item.Name),
        item.Quantity.ToString(CultureInfo.InvariantCulture),
        item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture));
}
```
