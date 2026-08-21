# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/10. Conversion Operations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse sync service materializes inventory before filtering low-stock alerts. Review this method when `catalog` is an EF Core `IQueryable<InventoryItem>` from `_db.Inventory`:

```csharp
public List<InventoryItem> GetLowStockAlerts(IQueryable<InventoryItem> catalog)
{
    var snapshot = catalog.ToList(); // ensure we have a list
    return snapshot
        .Where(item => item.StockQty > 0 && item.StockQty <= 10)
        .OrderBy(item => item.StockQty)
        .ToList();
}
```

What is wrong with calling `ToList()` this early, and how would you fix it?

---

#### Q2. (R) An API endpoint reports low-stock metrics by reusing one deferred query three times:

```csharp
IEnumerable<InventoryItem> lowStock = liveCatalog.Where(item =>
{
    _logger.LogDebug("Filtering {Sku}", item.Sku);
    return item.StockQty > 0 && item.StockQty <= 10;
});

int alertCount = lowStock.Count();
List<string> alertSkus = lowStock.Select(item => item.Sku).ToList();
decimal alertValue = lowStock.Sum(item => item.UnitPrice * item.StockQty);
```

Under load the endpoint is slow and logs show the filter running many times per request. What went wrong, and where should `ToList()` appear?

---

#### Q3. (R) After a bulk import, a developer builds a SKU lookup map directly from the raw feed (duplicate SKU rows are common in imports):

```csharp
InventoryItem[] importedRows = await _importReader.ReadAllAsync();

Dictionary<string, InventoryItem> skuLookup =
    importedRows.ToDictionary(row => row.Sku);

// later: validate order lines with skuLookup.TryGetValue(...)
```

Production throws `ArgumentException: An item with the same key has already been added.` What failed, and how do you build a safe lookup?

---

#### Q4. (R) A legacy COM import returns `IEnumerable` (non-generic) with mixed runtime types. Two teammates propose different approaches:

```csharp
// Teammate A — strict typing
foreach (InventoryItem item in legacyFeed.Cast<InventoryItem>())
{
    ProcessRow(item);
}

// Teammate B — tolerant extraction
List<InventoryItem> items = legacyFeed.OfType<InventoryItem>().ToList();
foreach (InventoryItem item in items)
{
    ProcessRow(item);
}
```

The feed occasionally contains corrupt string rows like `"CORRUPT-ROW-NOT-AN-ITEM"`. Which approach fits production import validation, and what breaks if you choose the other?

---

#### Q5. (R) A catalog search endpoint tries to apply a custom C# helper inside an EF Core query:

```csharp
public async Task<List<InventoryItem>> SearchExpensiveAsync(CancellationToken ct)
{
    return await _db.Inventory
        .AsEnumerable()
        .Where(item => MatchesPricingPolicy(item)) // instance method — not translatable to SQL
        .OrderBy(item => item.UnitPrice)
        .Take(20)
        .ToListAsync(ct);
}
```

The query compiles but loads the entire `Inventory` table into memory on every search. What happened, and how do you fix it without abandoning EF translation?

---

#### Q6. (M) A pricing job snapshots equipment rows, then mutates live catalog prices while reporting uses the snapshot:

```csharp
List<InventoryItem> equipmentSnapshot =
    liveCatalog
        .Where(item => item.Category == ItemCategory.Equipment)
        .OrderBy(item => item.UnitPrice)
        .ToList();

// ... hours later, batch job updates UnitPrice on liveCatalog items ...

decimal reportedTotal = equipmentSnapshot.Sum(item => item.UnitPrice);
```

The report total changes even though `equipmentSnapshot.Count` is unchanged. Explain the behavior and what a production snapshot must guarantee if finance needs immutable prices.
