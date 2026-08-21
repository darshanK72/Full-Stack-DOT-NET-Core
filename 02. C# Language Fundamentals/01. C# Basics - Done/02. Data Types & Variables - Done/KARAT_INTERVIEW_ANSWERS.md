# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/02. Data Types & Variables - Done`

---

#### Q1. (R) Finance QA reports order totals off by one cent on some invoices. Review this pricing helper copied from a prototype:

```csharp
public decimal CalculateOrderTotal(double unitPrice, int quantity)
{
    double subtotal = unitPrice * quantity;
    double tax = subtotal * 0.18;
    return (decimal)(subtotal + tax);
}
```

A developer says casting the final result to `decimal` fixes binary rounding. What is wrong, and what would you change?

**Answer:** All arithmetic runs in `double` (binary floating-point), so rounding errors appear **before** the final cast — converting to `decimal` at the end only changes the display type, not the already-corrupted intermediate values. Money paths should use `decimal` (and `decimal` literals with `m`) from the first operand.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `double` used for unit price, subtotal, and tax | Cent-level drift on totals (e.g., `0.1 + 0.2` binary artifacts) |
| Design | Final `(decimal)` cast masks bad intermediate math | QA sees intermittent penny mismatches vs ledger |
| API contract | `double unitPrice` invites callers to pass JSON `number` doubles | Errors propagate from deserialization through billing |

**Fix (priority order):**

1. Change parameters and locals to `decimal` — e.g., `decimal unitPrice`, `decimal subtotal = unitPrice * quantity`, `const decimal TaxRate = 0.18m`.
2. Ensure literals use `m` suffix (`0.18m`, not `0.18`) so the compiler picks `decimal` arithmetic.
3. Parse inbound prices with `decimal.Parse` / `GetDecimal()` — never `GetDouble()` for currency fields.
4. Add regression tests for known edge cases (`0.1m + 0.2m`, quantities × repeating decimals).

```csharp
public decimal CalculateOrderTotal(decimal unitPrice, int quantity)
{
    decimal subtotal = unitPrice * quantity;
    decimal tax = subtotal * 0.18m;
    return subtotal + tax;
}
```

**Production takeaway:** This chapter's Section 11 shows `(double)0.1 + (double)0.2` vs `0.1m + 0.2m` — Karat tests whether you catch **where** the type matters, not just whether you can name `decimal`. See foundation **Data Types** — float/double vs decimal gotcha.

---

#### Q2. (R) A loyalty API returns `int?` for optional points. After deploy, `NullReferenceException` and `InvalidOperationException` appear in logs. Review:

```csharp
public int ComputeBonus(int? loyaltyPoints, string? tierCode)
{
    var bonus = loyaltyPoints.Value * 2;
    if (tierCode.Equals("Gold", StringComparison.OrdinalIgnoreCase))
        bonus += 50;
    return bonus;
}
```

What breaks in production, and how would you harden this method?

**Answer:** `loyaltyPoints.Value` throws `InvalidOperationException` when the nullable has no value, and `tierCode.Equals(...)` throws `NullReferenceException` when `tierCode` is null — both are realistic for optional API fields that clients omit or send as JSON `null`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Unguarded `.Value` on `int?` | `InvalidOperationException` — "Nullable object must have a value" |
| Runtime | Instance call on nullable `string?` | `NullReferenceException` when tier is absent |
| API / NRT | Ignores nullable annotations at boundaries | Crashes instead of treating missing data as zero bonus / non-Gold |

**Fix (priority order):**

1. Replace `.Value` with null-coalescing or `HasValue` check — e.g., `int points = loyaltyPoints ?? 0;`.
2. Use null-safe comparison — `string.Equals(tierCode, "Gold", StringComparison.OrdinalIgnoreCase)` (static overload handles null tier as non-match).
3. Enable `<Nullable>enable</Nullable>` and fix CS8602 warnings at compile time rather than suppressing with `!`.
4. Document contract: null points → 0 bonus; null/unknown tier → base bonus only.

```csharp
public int ComputeBonus(int? loyaltyPoints, string? tierCode)
{
    var bonus = (loyaltyPoints ?? 0) * 2;
    if (string.Equals(tierCode, "Gold", StringComparison.OrdinalIgnoreCase))
        bonus += 50;
    return bonus;
}
```

**Production takeaway:** Section 19–20 cover `int?`, `??`, and `string?` — production code must treat null as data, not as exceptional. See foundation **Nullable value types** and **NRT** gotchas.

---

#### Q3. (R) A metrics exporter builds a snapshot list for a dashboard. Under load, Gen2 collections spike. Review:

```csharp
public IReadOnlyList<object> BuildDailyCounts(IEnumerable<int> orderCounts)
{
    var snapshot = new List<object>();
    foreach (var count in orderCounts)
        snapshot.Add(count);
    return snapshot;
}
```

What is the performance issue, and what type change fixes it without changing call-site semantics?

**Answer:** Each `int` added to `List<object>` is **boxed** — a heap allocation and copy wrapper per value — which creates massive allocation pressure when `orderCounts` is large, driving frequent GC Gen2 collections under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Implicit boxing `int` → `object` on every `Add` | O(n) heap allocations; GC pauses in exporters |
| Design | `List<object>` where homogeneous ints suffice | Hides type intent; invites further boxing/unboxing downstream |
| Runtime | Unbox casts if consumers read values back | Extra CPU + `InvalidCastException` risk on bad casts |

**Fix (priority order):**

1. Change to `List<int>` (or `int[]` if size is known) — no boxing for value-type elements.
2. Return `IReadOnlyList<int>` so callers get a typed, allocation-efficient snapshot.
3. If polymorphism is truly required, document why and pool/reuse buffers; prefer generics over `object`.
4. Profile with `dotnet-counters` (GC heap size, allocation rate) before/after.

```csharp
public IReadOnlyList<int> BuildDailyCounts(IEnumerable<int> orderCounts)
{
    return orderCounts.ToList(); // List<int> — no boxing
}
```

**Production takeaway:** Section 18 previews boxing — Karat extends it to **hot-path allocation** in services. See foundation **Boxing/unboxing** — avoid `object`/`ArrayList` patterns for primitives.

---

#### Q4. (P) A warehouse service increments a 32-bit `int transactionId` inside a tight loop processing bulk imports. In staging (small files) IDs look fine; in production one job reports duplicate IDs and negative values after a long run. The team says "C# integers don't overflow in normal use." Explain what happened and what you would use instead.

**Answer:** Integer arithmetic in C# is **unchecked by default**, so when `transactionId` passes `int.MaxValue` it silently wraps to `int.MinValue` (two's complement) — duplicates and negative IDs follow. Staging never reached the boundary; production volume did.

- **Mechanism:** `transactionId++` at `2,147,483,647` becomes `-2,147,483,648` without throwing — see Section 11a `unchecked` wrap behavior.
- **Why staging missed it:** Overflow is volume-dependent; small test files never cross `int.MaxValue`.
- **Fix — type:** Use `long` (or `ulong`) for monotonic counters and IDs expected to grow without bound; SQL `BIGINT` alignment.
- **Fix — guard:** Wrap critical increments in `checked` if overflow must abort the job (`OverflowException`) rather than wrap — appropriate for financial counters or sequence integrity.
- **Fix — architecture:** Prefer database sequences / `Guid` / snowflake IDs for distributed uniqueness instead of in-process `int` counters.
- **Detection:** Add metrics/alerts when IDs approach `int.MaxValue - margin`; integration tests that simulate boundary (not always feasible in CI, but document the limit).

**Production takeaway:** "Normal use" is not a type strategy — choose `long`/`decimal`/nullable types based on domain bounds. See Section 11a **checked/unchecked** and integer range table in this chapter's quick reference.

---

#### Q5. (M) A developer models store configuration like the chapter's `StoreConfig` but tries to share a tax rate across all instances from appsettings loaded at startup:

```csharp
public class PricingOptions
{
    public const decimal StandardTaxRate = LoadTaxRateFromConfiguration();

    private static decimal LoadTaxRateFromConfiguration() =>
        decimal.Parse(Environment.GetEnvironmentVariable("TAX_RATE") ?? "0.18");
}

```

Build fails with CS0133. They propose replacing `const` with `static readonly` assigned from a static constructor. Is that sufficient for production DI, and what pattern would you recommend?

**Answer:** `const` requires a compile-time constant — runtime config cannot qualify (CS0133). `static readonly` with a static constructor can work for a singleton-like value, but it hides testability, defers failures to type-load time, and bypasses the options/DI patterns ASP.NET Core expects.

- **`const` vs `readonly` (Section 2):** `const` is baked in at compile time; `readonly` fields are set at run time (declaration or ctor) — config values are run-time facts.
- **`static readonly` + static ctor:** Loads env var once when the type is first accessed; hard to mock in tests; parse errors crash type initialization (`TypeInitializationException`).
- **Production pattern:** `IOptions<PricingOptions>` / `IOptionsMonitor<PricingOptions>` bound from `IConfiguration` at startup — reloadable, injectable, validated with `DataAnnotations` or `IValidateOptions<T>`.
- **Instance `readonly`:** Matches `StoreConfig` — per-store values set once in ctor when each store entity is created from DB/config row.
- **Validation:** Use `decimal.Parse` with `CultureInfo.InvariantCulture`; prefer `TryParse` or config binder errors over unhandled `FormatException` on startup.

```csharp
public sealed class PricingOptions
{
    public decimal StandardTaxRate { get; init; }
}

// Program.cs — services.Configure<PricingOptions>(configuration.GetSection("Pricing"));
```

**Production takeaway:** Karat pairs language keywords (`const`/`readonly`) with **how teams actually load config** — static env reads are a step up from `const`, but DI options are the shippable pattern. See **StoreConfig** in this chapter's `Program.cs` for instance-level `readonly`.

---

#### Q6. (D) Your API team debates `var` vs explicit types in service-layer code. Two snippets assign the same JSON field:

```csharp
var total = json.GetProperty("orderTotal").GetDecimal();
decimal total = json.GetProperty("orderTotal").GetDecimal();

var orderId = json.GetProperty("orderId").GetInt64();
long orderId = json.GetProperty("orderId").GetInt64();
```

When would you require explicit `decimal` and `long` (as in this chapter's money and `orderId` examples), and when is `var` acceptable?

**Answer:** Require explicit types when the type carries **domain meaning or correctness constraints** — money (`decimal`), large identifiers (`long`), flags, enums — so reviewers and maintainers see intent without inferring from the right-hand side. Use `var` when the type is obvious from a descriptive API (e.g., `var doc = JsonDocument.Parse(...)`) or when the right-hand side is verbose generic code.

**Require explicit types:**

- **Financial fields** — always `decimal total` (Section 11); never `var` that could drift if someone swaps in `GetDouble()`.
- **Large IDs and counters** — `long orderId` documents 64-bit range (Section 10 `orderId` literal with `L` suffix); prevents silent narrowing if API changes.
- **Public API surfaces** — method signatures and DTO properties must show types; no `var` in signatures.
- **Narrowing or ambiguous conversions** — when RHS involves casts, ternary mixes, or custom operators.

**`var` acceptable:**

- Right-hand side clearly names the type: `var store = new StoreConfig("PUN-01", 0.18m)` in local scope (though some teams still prefer explicit for `StoreConfig`).
- LINQ/comprehension where the static type is long (`IEnumerable<...>`) and explicit type clutters.
- `new()` target-typed scenarios where type appears on the left of assignment elsewhere in the statement.

**Team convention:** Allow `var` for locals when the type is **immediate and unambiguous**; disallow `var` for primitives that encode business rules (`decimal`, `long`, `bool` flags, enums like `ShipmentStatus`). Enforce via `.editorconfig` (IDE0008) with exceptions documented.

**Production takeaway:** Section 7 says `var` is still strongly typed — the judgment call is **readability and misuse prevention**, not compiler capability. Karat tests whether you tie type choice to money, ID range, and reviewability like this chapter's examples.

---
