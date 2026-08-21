# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/04. Var Dynamic & Special Keywords/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse integration service parses third-party CSV rows into `dynamic` bags before posting to inventory. It passes QA with two sample files but throws in production on the first malformed row. Review the mapper:

```csharp
public sealed class DynamicImportMapper
{
    public decimal ComputeLineTotal(dynamic row)
    {
        var sku = row.Sku;
        var qty = row.Qantity;          // vendor column mapped at runtime
        var price = row.UnitPrice;
        return qty * price;
    }

    public void ImportBatch(IEnumerable<dynamic> rows)
    {
        foreach (dynamic row in rows)
        {
            var total = ComputeLineTotal(row);
            _ledger.Post(row.Sku, total);
        }
    }
}
```

What fails, when, and how would you harden this for production?

---

#### Q2. (R) A pricing dashboard uses `var` with LINQ and mutates the source collection between query definition and enumeration. Review:

```csharp
public void PrintLowStockAlerts(InventoryItem[] stock)
{
    var lowStock = stock.Where(i => i.Quantity < 15).OrderBy(i => i.Sku);

    ApplyEmergencyRestock(stock);   // bumps quantities on several SKUs

    foreach (var item in lowStock)
    {
        _alerts.Send($"{item.Sku} critically low: {item.Quantity}");
    }
}

private static void ApplyEmergencyRestock(InventoryItem[] stock)
{
    foreach (var item in stock.Where(i => i.Quantity < 5))
        item.Quantity += 50;
}
```

What behavior do stakeholders see versus what they expect, and what would you change?

---

#### Q3. (R) A generic repository uses `nameof` and `default` for reflection-based updates. After a refactor, updates silently stop working for value-type columns. Review:

```csharp
public class GenericPatchHelper<T> where T : struct
{
    public static void EnsureColumnExists(string columnName)
    {
        var prop = typeof(T).GetProperty(columnName);
        if (prop is null)
            throw new InvalidOperationException($"Missing {columnName} on {nameof(T)}");
    }

    public static T CreateUnset()
    {
        return default;
    }
}

// Caller (InventoryDelta patch path):
string key = nameof(List<InventoryDelta>);   // used as dictionary / column key
var delta = GenericPatchHelper<InventoryDelta>.CreateUnset();
_audit[key] = delta.Amount;   // delta.Sku is null, Amount is 0
```

Identify the defects (compile-time, runtime, and data correctness) and prioritize fixes.

---

#### Q4. (P) Your team ingests nightly plugin config from a legacy host that exposes JSON whose shape changes per warehouse (extra keys, missing booleans, numeric strings). A junior dev proposes `dynamic` + `ExpandoObject` for the entire pipeline; another proposes strongly typed records + `System.Text.Json` with `[JsonExtensionData]`. When is `dynamic` justified here, and what production risks push you toward typed or semi-typed models?

---

#### Q5. (M) A background price-refresh worker should stop within seconds when ops clicks "Cancel" in the admin UI. The flag works in dev (single core, low load) but the worker occasionally runs for minutes in production. Review:

```csharp
public sealed class PriceRefreshWorker
{
    private bool _stopRequested;

    public void RequestStop() => _stopRequested = true;

    public void RunLoop()
    {
        while (!_stopRequested)
        {
            RefreshNextSku();
            Thread.Sleep(10);
        }
    }
}
```

What mechanism is missing, why does it pass locally, and what would you use instead for a simple stop flag versus a counter you increment?

---

#### Q6. (D) Two teams share a `WarehouseAnalytics` namespace. Team A added a helper type named `Math` for domain-specific rounding; Team B assumed BCL `System.Math` in unqualified calls. Review the pricing snippet:

```csharp
namespace WarehouseAnalytics.Pricing;

public static class Math
{
    public static decimal RoundToNickel(decimal value)
        => global::System.Math.Round(value / 0.05m) * 0.05m;
}

public sealed class LineTotalCalculator
{
    public decimal ApplyTax(decimal net, decimal rate)
    {
        decimal gross = net * (1 + rate);
        return Math.Round(gross, 2);   // which Math?
    }
}
```

What breaks at compile time or runtime, and what naming or qualification policy prevents this in a shared codebase?
