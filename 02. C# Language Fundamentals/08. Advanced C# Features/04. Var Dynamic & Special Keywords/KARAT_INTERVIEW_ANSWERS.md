# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/04. Var Dynamic & Special Keywords/`

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

**Answer:** The typo `Qantity` compiles because `dynamic` skips member checking, then throws `RuntimeBinderException` at runtime on the first row that lacks that misspelled member — QA samples may never hit the path. `var` locals inherit `dynamic` when the initializer is dynamic, so the entire expression chain stays late-bound with no compile-time safety.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Typo `row.Qantity` — no CS1061 | `RuntimeBinderException` aborts batch mid-import |
| Design | `dynamic` end-to-end for known CSV columns | Zero IntelliSense/refactor safety; schema drift undetected until prod |
| Correctness | No validation when members missing or wrong type | `qty * price` may bind wrong overload or fail on string values from CSV |
| Maintainability | `var` masks that locals are dynamically typed | Reviewers assume compile-time checking |

**Fix (priority order):**

1. Map to a strongly typed DTO (`InventoryImportRow`) with explicit property names and parse/validate at the boundary — typos become compile errors.
2. If shape is truly unknown, use `JsonDocument` / `JsonElement` or a dictionary with explicit key lookup and guard clauses instead of dot-syntax on `dynamic`.
3. Wrap per-row processing in try/catch for `RuntimeBinderException` only at the boundary if you must keep dynamic interop — log row index/SKU and continue or dead-letter, do not fail the whole batch silently.
4. Add contract tests with production-like malformed rows (missing columns, string `"12.5"` for quantity).

**Production takeaway:** `dynamic` trades compile-time safety for flexibility — Karat expects you to confine it to interop seams (COM, legacy plugins) and validate before business logic. See **Program.cs** Sections 3–4 — `RuntimeBinderException` on typos and **QUICK REFERENCE** — "dynamic typo → RuntimeBinderException."

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

**Answer:** `lowStock` is deferred `IEnumerable<InventoryItem>` — the `Where` predicate runs at enumeration time, after restock mutates quantities. SKUs that were low when the query was *defined* may no longer qualify (or vice versa), so alerts no longer match the "snapshot" ops thought they captured.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Deferred LINQ + mutation before `foreach` | Alert list reflects post-restock state, not state at query creation |
| Design | `var` hides `IEnumerable` deferral | Readers assume eager list; `var` inferred type is not `List<>` |
| Business logic | Restock before alert send in same method | Wrong ordering — alerts should fire on pre-restock snapshot or restock should run after |

**Fix (priority order):**

1. Materialize before mutation: `var lowStock = stock.Where(...).OrderBy(...).ToList();` then call `ApplyEmergencyRestock`.
2. Reorder operations: send alerts first, then restock — if business rules require alerting on original levels.
3. Use explicit type or comment when deferral matters: `IEnumerable<InventoryItem> lowStockQuery = ...` to signal lazy evaluation.
4. For reporting snapshots, project to immutable DTOs at capture time so later mutations cannot change alert content.

**Production takeaway:** `var` does not change LINQ semantics — deferred execution still bites when the underlying collection mutates. See **Program.cs** Section 2 — `var` with LINQ infers `IEnumerable<T>`, not a snapshot.

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

**Answer:** `nameof(T)` inside a generic method returns the type parameter name (`"T"`), not the closed type (`"InventoryDelta"`). `nameof(List<InventoryDelta>)` yields `"List"`, not a useful storage key. `default` on a struct zeroes all fields — `Sku` is `null`, `Amount` is `0` — so `_audit` records garbage without throwing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `nameof(List<InventoryDelta>)` → `"List"` | Wrong dictionary key; collisions and missing audit entries |
| Correctness | `nameof(T)` in generic method → `"T"` | Exception messages and metadata keys useless in logs |
| Data | `default` struct used as "unset" business object | Silent null SKU and zero amount posted to audit |
| Design | `where T : struct` + `default` conflates "missing" with valid zero delta | Cannot distinguish unset from intentional `Amount = 0` |

**Fix (priority order):**

1. Use `typeof(T).Name` or `nameof(InventoryDelta)` at call sites for keys — never `nameof(T)` when you need the closed type name (see **Program.cs** Section 7 — `nameof(List<int>)` → `"List"` pitfall).
2. Replace `CreateUnset()` with explicit factory or `Nullable<T>` / `Optional<InventoryDelta>` if "unset" is a business state.
3. Validate before audit: `if (string.IsNullOrEmpty(delta.Sku)) throw ...` — do not propagate zeroed structs.
4. For reflection keys, use `nameof(InventoryDelta.Sku)` with `GetProperty` — matches **Program.cs** dynamic vs reflection demo.

**Production takeaway:** `nameof` is compile-time safe for *syntax* but not semantically magic — generic arity and type-parameter names trip audit and ORM code. `default` on structs is a valid zero value, not "empty optional."

---

#### Q4. (P) Your team ingests nightly plugin config from a legacy host that exposes JSON whose shape changes per warehouse (extra keys, missing booleans, numeric strings). A junior dev proposes `dynamic` + `ExpandoObject` for the entire pipeline; another proposes strongly typed records + `System.Text.Json` with `[JsonExtensionData]`. When is `dynamic` justified here, and what production risks push you toward typed or semi-typed models?

**Answer:** Use `dynamic` only at the thin interop boundary where you truly cannot describe the contract (COM, embedded scripting, some legacy APIs). For JSON plugin config with known core fields and variable extensions, prefer typed records with `[JsonExtensionData] Dictionary<string, JsonElement>` or a dedicated options class — you keep compile checks on `WarehouseId`, `MaxSkus`, etc., while absorbing extra keys.

- **`dynamic` risks:** `RuntimeBinderException` in production on typos; no refactor support; harder unit tests; DLR overhead on hot paths; `ExpandoObject` members are not normal CLR properties — reflection returns null (**Program.cs** Section 6).
- **Typed + extension data:** Core settings validated at deserialize time; unknown keys preserved for forward compatibility; schema changes caught in CI with golden JSON fixtures.
- **When `dynamic` wins:** One-off script host, JScript/COM object, or third-party DLL that only exposes late-bound objects — wrap in an adapter and map to typed models immediately inside the adapter.
- **Middle ground:** Deserialize to `JsonDocument`, query required nodes explicitly, validate types before mapping — no DLR, explicit errors.

**Production takeaway:** Karat tests judgment on *where* to stop using `dynamic` — one adapter method, not the whole pipeline. Match **Program.cs** warehouse plugin scenario: ExpandoObject for demo config, but production code maps to validated types before inventory rules run.

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

**Answer:** `_stopRequested` must be `volatile` (or guarded by `Interlocked`/lock) so the worker thread observes the UI thread's write promptly. Without `volatile`, the JIT may cache the field in a register and the loop may never exit — intermittent and load-dependent, which is why dev often passes.

- **Simple boolean stop flag:** `private volatile bool _stopRequested;` — matches **Program.cs** `PriceRefreshSignal` (Section 4). Documents intent: cross-thread visibility, not atomic compound updates.
- **Counter / statistics:** use `Interlocked.Increment` / `Interlocked.Read` — `volatile` alone does not make read-modify-write atomic.
- **Why local dev hides it:** single-core, short runs, or debugger flushes memory; production multi-core reordering exposes the bug.
- **Alternative:** `CancellationToken` from `CancellationTokenSource` — idiomatic for .NET worker services and ASP.NET hosted services; propagates through async calls better than a raw flag.

**Production takeaway:** `volatile` is field-only and not a lock — pair with simple flags only. See **Program.cs** QUICK REFERENCE — "volatile on local → CS0106" and threading depth in **06. Multithreading & Async**.

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

**Answer:** Unqualified `Math.Round` resolves to `WarehouseAnalytics.Pricing.Math` in that namespace — which has no `Round(decimal, int)` overload, so you get **CS0117** at compile time (not a silent wrong answer). The shadowing is still a maintenance trap: every new developer repeats the failure until they learn the local type exists.

- **Immediate fix:** `global::System.Math.Round(gross, 2)` or `System.Math.Round` with a file-level alias `using BclMath = System.Math;`.
- **Policy:** Ban type names that shadow BCL types (`Math`, `Thread`, `Task`, `Environment`) in shared namespaces — rename to `PricingMath`, `MoneyRounding`, etc. **Program.cs** Section 9 explicitly warns never ship shadow names in production.
- **Linting:** Enable analyzer rules or code review checklist for BCL name collisions; namespace-per-feature reduces accidental local `Math` helpers in global pricing code.
- **Design:** Domain rounding belongs on a clearly named static class (`CurrencyRounding.ToNickel`) so call sites document intent and never compete with `System.Math` lookup.

**Production takeaway:** `global::` fixes resolution but does not fix team confusion — Karat pairs mechanism (`global::System.Math`) with design policy (do not shadow BCL). See **Program.cs** Sections 6 and 9 — local `Math` vs `global::System.Math.PI`.
