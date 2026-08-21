# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/02. ArrayList`

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

**Answer:** The export loop assumes every `ArrayList` element is a `Product`, but `Add("RUSH-PICK")` stores a `string` — the cast `(Product)entry` throws `InvalidCastException` at runtime because `ArrayList.Add` accepts any `object` with no compile-time type check.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Type safety | `ArrayList` allows heterogeneous `Add` | Wrong runtime type slips in; compile succeeds |
| Runtime | `(Product)entry` on a `string` | `InvalidCastException` — nightly job fails |
| Design | Mixed domain types in one bag | Same fragility as **Program.cs** Section 5 CRUD demo (`Add(250)` beside `Product`) |
| Maintainability | Implicit contract "all items are Product" | No compiler enforcement; code review must catch bad `Add` |

**Fix (priority order):**

1. Remove the string from the product list — store rush flags on `Product` or use a separate collection.
2. Migrate `warehouseLines` to `List<Product>` so `Add("RUSH-PICK")` fails at compile time.
3. Short-term guard: use pattern matching (`entry is Product p`) and log/skip invalid rows instead of blind cast — stops the crash but hides data quality issues.
4. Add an integration test that runs the export against a fixture mirroring legacy mixed data.

**Production takeaway:** `ArrayList` defers type errors to production — Karat uses this to test whether you connect "it compiled" with "nothing checked the element type." See **Program.cs** Sections 2 and 7 — indexer and `foreach` return `object`.

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

**Answer:** Each `Add(i)` boxes the `int` onto the heap, creating 10,000 extra allocations and GC pressure; the read then uses `(long)` on a boxed `int`, which throws `InvalidCastException` because unboxing requires the exact original type — you cannot unbox a boxed `int` directly to `long`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Boxing every `int` on `Add` | Heap allocations, GC churn under load |
| Runtime | `(long)readings[0]` unboxes boxed `int` as `long` | `InvalidCastException` — intermittent pod crash |
| API misuse | `ArrayList` for homogeneous numeric telemetry | Wrong tool when all elements are `int` |
| "Fix" regression | Widening cast on unbox | Confuses numeric widening with unboxing rules |

**Fix (priority order):**

1. Replace with `List<int>` — no boxing for value-type elements; indexer returns `int` directly.
2. Correct read: `int peak = (int)readings[0]!` only if staying on `ArrayList`; prefer `List<int>` so no cast is needed.
3. If values can exceed `int`, use `List<long>` from the start — store the wider type without boxing.
4. Profile Gen0/Gen1 collections after migration to confirm allocation drop.

**Production takeaway:** Boxing is invisible in small demos but measurable in hot loops — Karat pairs GC symptoms with the `Add(object)` signature. See **Program.cs** Section 9 — boxing on `Add(42)` and correct `(int)` unbox.

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

**Answer:** Index 1 holds a boxed `int` (250), not a `Product` — `((Product)catalog[i]!)` throws `InvalidCastException` on the second iteration. The method compiles because `IList` indexer returns `object?` and the cast is explicit; no compile-time guarantee exists that callers populated the list homogeneously.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Contract | `IList` accepts any element type | Callers can pass legacy mixed `ArrayList` |
| Runtime | Cast `(Product)` on boxed `int` | API call fails mid-loop |
| API design | Non-generic `IList` parameter | Hides intended element type from callers and reviewers |
| Migration debt | Legacy row shape (`250` inline) coexists with `Product` | Data migration incomplete but new code assumes completion |

**Fix (priority order):**

1. Change signature to `IReadOnlyList<Product>` or `List<Product>` — mixed `Add` fails at compile time on the caller side when they migrate.
2. Add a dedicated DTO mapper at the legacy boundary that converts raw rows to `Product` before calling business logic — never pass raw `ArrayList` into domain code.
3. Interim: validate with `catalog[i] is Product` and throw a descriptive error listing index and runtime type.
4. Deprecate `GetCatalogTotal(IList)` once batch jobs are updated; track call sites.

**Production takeaway:** Programming against non-generic `IList`/`ICollection` was necessary pre-generics — modern code should not treat it as a typed list. See **Program.cs** Section 8 — `IList` polymorphism and Section 10 — catalog with manual casts.

---

#### Q4. (P) Your team is migrating a .NET Framework inventory module that uses `ArrayList` for product catalogs, `Hashtable` for SKU→bin lookup, and manual `(Product)` casts in every loop. What is your migration plan to modern generic collections, and what do you change first to stop runtime cast failures?

**Answer:** Migrate at the boundaries first — replace internal storage with `List<Product>` and `Dictionary<string, string>` (or appropriate typed keys/values), then narrow public APIs from `IList`/`Hashtable` to generic interfaces so new code cannot inject wrong types; leave thin adapter shims for external callers until call sites are updated.

- **Phase 1 — stop the bleeding:** Identify hot paths throwing `InvalidCastException` (catalog totals, export loops). Convert those `ArrayList` instances to `List<Product>` at the point of creation; map legacy rows in one factory method rather than scattering casts.
- **Phase 2 — keyed lookup:** Replace `Hashtable skuToBin` with `Dictionary<string, string>` — eliminates boxing on value types and `as`/cast on values. See **Program.cs** Section 12.
- **Phase 3 — API surface:** Change method parameters from `IList` to `IReadOnlyList<Product>` or `IEnumerable<Product>`; keep obsolete overloads that copy into `List<Product>` with validation for remaining legacy callers.
- **Phase 4 — satellite types:** Migrate `Stack`/`Queue`/`SortedList` usages to `Stack<T>`, `Queue<T>`, `SortedList<TKey,TValue>` as touched (Section 13 preview).
- **Testing:** Characterization tests with production-like mixed `ArrayList` fixtures; assert migrated code either rejects bad rows or maps them explicitly — never silent cast.
- **Do not big-bang** every file — migrate by vertical slice (catalog service end-to-end) so each PR is deployable.

**Production takeaway:** Migration priority is runtime cast failures and public boundaries, not alphabetical file renames — Karat tests whether you know *where* generics buy safety first.

---

#### Q5. (M) Two implementations compute the same warehouse capacity check. One uses `ArrayList`, one uses `List<int>`. A performance test shows the `ArrayList` version allocates more and runs slower on .NET 8. Explain the mechanism — what happens on each `Add` for value types, and why does `List<int>` avoid it?

**Answer:** `ArrayList.Add` takes `object`, so each `int` is boxed into a separate heap object stored in the internal `object[]`; `List<int>` stores ints directly in its `T[]` backing array with no boxing because the generic type parameter is known at compile time.

- **`ArrayList.Add(i)`:** `int` → boxed `object` (heap allocation + copy) → reference stored in `object[]`. 50,000 iterations ⇒ 50,000 box allocations plus array resizing copies.
- **`List<int>.Add(i)`:** `int` written inline into `int[]` — same amortized growth strategy as `ArrayList`, but no per-element heap wrapper.
- **Read path:** `ArrayList` indexer returns `object` → unbox cast; `List<int>` indexer returns `int` — fewer instructions, no unbox.
- **GC:** Boxed objects are short-lived Gen0 garbage; high-frequency adds inflate collection frequency and cache pressure — matches the pod/GC story in Q2.
- **Capacity hint:** Both honor initial capacity (`new ArrayList(50_000)` / `new List<int>(50_000)`) to reduce resize copies — boxing cost remains unique to `ArrayList` for value types.

**Production takeaway:** Same Big-O for `Add`, different constant factors and allocation profile — Karat expects you to name boxing/unboxing, not just "generics are faster." See **Program.cs** Sections 4 and 11 — capacity behavior and `ArrayList` vs `List<T>` comparison table.

---

#### Q6. (D) A monolith has 40 call sites passing `ArrayList` into methods typed as `IList`. Full rewrite to `List<T>` is blocked for two sprints. What incremental strategy reduces `InvalidCastException` risk without a big-bang change, and where do you draw the line on leaving `ArrayList` in place?

**Answer:** Introduce typed wrappers and validated adapters at the edges — new code accepts `IReadOnlyList<T>`; legacy `ArrayList` flows through a single conversion layer that validates or maps elements — and freeze new `ArrayList` usage via analyzer or review rule while migrating call sites by module.

- **Immediate guardrails:** Ban new `ArrayList`/`new ArrayList()` in product code (Roslyn analyzer or `.editorconfig` convention); allow only in the compatibility adapter project.
- **Adapter pattern:** `static List<Product> ToProductList(IList legacy)` — foreach with `is Product` check; throw `InvalidOperationException` with index and type on first bad element (fail fast at boundary, not deep in business logic).
- **Strangler order:** Migrate leaf utilities with no downstream `IList` exports first; then services; public API last — each sprint removes a cluster of call sites, not random files.
- **Interface bridge:** Obsolete `void Process(IList items)` → add `Process(IReadOnlyList<Product> items)`; old overload converts via adapter and logs `[Obsolete]` warning to track remaining callers.
- **Draw the line — keep `ArrayList` temporarily only:** inside isolated interop with external legacy binaries you cannot change, or serialized blobs you have not migrated yet — never in new domain logic.
- **Do not** half-migrate by sprinkling `(Product)` casts — that preserves runtime risk; centralize casts once.

**Production takeaway:** Incremental migration is about *typed boundaries* and *fail-fast validation*, not leaving 40 unchecked cast sites — Karat tests pragmatic legacy strategy, not "rewrite everything day one."

---
