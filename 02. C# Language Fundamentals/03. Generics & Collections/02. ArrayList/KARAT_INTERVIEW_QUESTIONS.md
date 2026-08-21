# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/02. ArrayList`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A legacy warehouse service stores pick lines in an `ArrayList`. After a refactor, production throws `InvalidCastException` during the nightly export. Review the code — what failed, and why did it compile?

```csharp
ArrayList warehouseLines = LoadLinesFromDatabase(); // returns mixed legacy rows

decimal totalValue = 0m;
foreach (object entry in warehouseLines)
{
    Product product = (Product)entry;
    totalValue += product.ProductPrice;
}
```

A teammate added this line to support rush SKUs before the export job runs:

```csharp
warehouseLines.Add("RUSH-PICK");
```

---

#### Q2. (R) A sensor-ingestion job stores telemetry in an `ArrayList` and unboxes on read. Under load, GC pressure spikes and one pod crashes intermittently. Review the hot path — what is wrong at the storage layer and on read?

```csharp
ArrayList readings = new ArrayList(capacity: 10_000);

for (int i = 0; i < 10_000; i++)
{
    readings.Add(i); // sensor count snapshot
}

int peak = (long)readings[0]; // "fix" after a code review comment
```

---

#### Q3. (R) A catalog API still exposes `IList` for backward compatibility. New code assumes every element is a `Product`. Review this controller helper — what breaks at runtime, and what compile-time safety is missing?

```csharp
public decimal GetCatalogTotal(IList catalog)
{
    decimal total = 0m;
    for (int i = 0; i < catalog.Count; i++)
    {
        total += ((Product)catalog[i]!).ProductPrice;
    }
    return total;
}

// Caller from legacy batch job:
IList legacyCatalog = new ArrayList
{
    new Product { ProductNo = 10, ProductName = "Scanner", ProductPrice = 89.50m },
    250 // legacy quantity field stored inline before Product migration
};
GetCatalogTotal(legacyCatalog);
```

---

#### Q4. (P) Your team is migrating a .NET Framework inventory module that uses `ArrayList` for product catalogs, `Hashtable` for SKU→bin lookup, and manual `(Product)` casts in every loop. What is your migration plan to modern generic collections, and what do you change first to stop runtime cast failures?

---

#### Q5. (M) Two implementations compute the same warehouse capacity check. One uses `ArrayList`, one uses `List<int>`. A performance test shows the `ArrayList` version allocates more and runs slower on .NET 8. Explain the mechanism — what happens on each `Add` for value types, and why does `List<int>` avoid it?

```csharp
// Version A — legacy
ArrayList slots = new ArrayList(50_000);
for (int i = 0; i < 50_000; i++)
    slots.Add(i);

// Version B — migrated
List<int> slots = new List<int>(50_000);
for (int i = 0; i < 50_000; i++)
    slots.Add(i);
```

---

#### Q6. (D) A monolith has 40 call sites passing `ArrayList` into methods typed as `IList`. Full rewrite to `List<T>` is blocked for two sprints. What incremental strategy reduces `InvalidCastException` risk without a big-bang change, and where do you draw the line on leaving `ArrayList` in place?

---
