# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/05. Working with CSV and Text Files`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

#### Q6. (D) Your team must ingest partner CSV feeds with quoted commas, optional date columns, and occasional header renames (`SKU` vs `Sku`). One engineer proposes `CsvHelper`; another wants to extend the hand-rolled `SplitQuotedLine` from this chapter. When do you reach for each, and what are the trade-offs for a long-lived warehouse integration?

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
