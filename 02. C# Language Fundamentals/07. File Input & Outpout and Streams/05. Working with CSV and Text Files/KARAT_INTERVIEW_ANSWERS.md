# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/05. Working with CSV and Text Files`

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

#### Q2. (R) An export job writes inventory CSV on a German Windows server; a US warehouse tool rejects half the rows. Review the export path:

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

#### Q4. (P) A drop-folder service uses `FileSystemWatcher` to import CSV as soon as a file appears in `\\share\inbound`. Operators report random "column count" errors and duplicate SKU rows. The handler:

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

#### Q6. (D) Your team must ingest partner CSV feeds with quoted commas, optional date columns, and occasional header renames (`SKU` vs `Sku`). One engineer proposes `CsvHelper`; another wants to extend the hand-rolled `SplitQuotedLine` from this chapter. When do you reach for each, and what are the trade-offs for a long-lived warehouse integration?

**Answer:** Extend the hand-rolled parser when the format is narrow, stable, and you need zero dependencies and full control for learning or a single internal export shape — `SplitQuotedLine` plus invariant `TryParse` covers quoted commas and typed columns for fixed five-column inventory. Reach for **CsvHelper** when feeds vary (renamed headers, optional columns, class maps, multiple delimiters) and you want header binding, validation attributes, and RFC 4180 edge cases without maintaining parser state yourself.

- **Hand-rolled (this chapter):** Fixed schema (`InventoryColumnCount`), quote-aware scan, explicit row errors — minimal surface, easy to unit-test with `StringReader`, no package churn; you own multiline fields, alternate encodings, and every new partner quirk.
- **CsvHelper:** `[Name("SKU")]`, `ClassMap`, `MissingFieldFound`, culture options, async enumeration — faster to onboard new feeds; adds dependency and team must learn mapping API; still need staging, transactions, and idempotency around it.
- **Header drift:** Hand-rolled code often assumes first line equals `InventoryHeader` string — brittle. CsvHelper can map by index or name with case-insensitive matching; either way, validate required columns up front and fail with a clear "missing column SKU" message.
- **Hybrid:** CsvHelper for deserialization into DTOs, then domain validation (`sku` required, qty ≥ 0) in a service layer — keeps parser concerns separate from warehouse rules (**Program.cs** `InventoryItem` pattern).
- **When not to hand-roll:** Multiple partners, embedded newlines in fields, tab/pipe delimiters, or frequent spec changes — maintenance cost exceeds CsvHelper's learning curve.

**Production takeaway:** Parser choice is an integration lifecycle bet — fixed internal format favors transparent hand-rolled code; multi-partner feeds favor a library plus staging and idempotent merge either way.

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
