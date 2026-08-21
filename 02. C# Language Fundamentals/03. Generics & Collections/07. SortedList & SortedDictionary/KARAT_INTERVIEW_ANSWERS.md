# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/07. SortedList & SortedDictionary`

---

#### Q1. (R) A warehouse dashboard prints the lowest and highest SKU from a live price map. Review:

**Answer:** `SortedDictionary<TKey,TValue>.Keys` is a read-only `ICollection<TKey>` with **no indexer** — only `SortedList` exposes `Keys[i]` and `Values[i]` for O(1) access by sorted rank. Keep `SortedDictionary` and walk `Keys` once (or track min/max while loading) rather than dropping to `Dictionary` and resorting on every request.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `Keys[0]` on `SortedDictionary` | CS0021 — indexer not defined on Keys view |
| API confusion | Assumed both sorted types support rank indexing | Wrong type chosen for dashboard endpoint |
| Over-correction | Sort-on-read with `Dictionary` + LINQ | O(n log n) per page load when O(n) scan or `SortedList` index suffices |

**Fix (priority order):**

1. If random access by sorted rank is required (`Keys[0]`, `Values[i]`), use `SortedList<string, decimal>` — see **Program.cs** Section 3.
2. If the map stays a `SortedDictionary`, iterate `Keys` once to capture first and last (Section 6 pattern) — O(n) but simple for 12k keys on a dashboard.
3. Do **not** switch to `Dictionary` solely to fix the compile error when sorted iteration is a product requirement.

```csharp
string? lowest = null, highest = null;
foreach (string sku in skuPrices.Keys)
{
    lowest ??= sku;
    highest = sku;
}
```

**Production takeaway:** Karat tests whether you know **index by rank** is `SortedList`-only — `SortedDictionary` gives sorted foreach and O(log n) key lookup, not `Keys[i]`.

---

#### Q2. (R) An inventory sync service upserts pallet counts every few seconds. Review the hot path:

**Answer:** `SortedList` insert and remove shift parallel key/value arrays — **O(n)** per upsert when the collection is full-sized. Frequent mixed inserts/updates on ~500 zones make array shifting dominate even though `TryGetValue` remains O(log n). Swap to `SortedDictionary<int, int>` for tree-backed O(log n) insert/remove while preserving sorted foreach.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `SortedList.Add` / shift on new keys | Latency grows with zone count under steady churn |
| Idiom | `ContainsKey` + indexer instead of one path | Extra O(log n) lookup; minor vs shifting cost |
| Collection choice | `SortedList` for write-heavy sync | Wrong tool when entries churn — Section 8 guidance |

**Fix (priority order):**

1. Replace backing field with `SortedDictionary<int, int>`.
2. Collapse upsert to indexer assignment — adds or updates in one call: `_countsByZone[zoneId] = palletCount`.
3. Reserve `SortedList` for small, mostly static maps (config tables, reorder reports in **Program.cs** Section 11).

```csharp
private readonly SortedDictionary<int, int> _countsByZone = new();

public void UpsertZoneCount(int zoneId, int palletCount) =>
    _countsByZone[zoneId] = palletCount;
```

**Production takeaway:** "Lookups are fast" is a Karat trap — **insert/remove cost** separates `SortedList` from `SortedDictionary`. See **Program.cs** Section 9 — insert O(n) vs O(log n).

---

#### Q3. (D) You expose a `/regions/sales` JSON endpoint. Product wants keys returned alphabetically by region code. Two proposals:

**Answer:** For ~30 regions, hourly refresh, and read-heavy traffic, **`SortedDictionary<string, int>` (B)** is the better default: sorted order is built into the structure, foreach needs no extra sort, and n is tiny so O(log n) lookup cost is irrelevant. **`Dictionary` + `OrderBy` (A)** adds allocation and O(n log n) work on every response unless you cache the sorted projection — acceptable only if you already hold a `Dictionary` for O(1) hot lookups elsewhere and sort rarely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance (A) | Sort 30 keys per request × 2k rpm | Avoidable CPU and GC from repeated `OrderBy` |
| Correctness (B wrong use) | `SortedDictionary` for millions of writes | Tree rebalancing still O(log n) — fine here, bad at huge churn |
| Design | Picking `Dictionary` "because it's faster" | Ignores that 30-key sort-on-read duplicates work the BCL already provides |

**Decision:**

| Factor | Prefer |
|---|---|
| Small n, sorted output every read | `SortedDictionary` |
| Huge n, order irrelevant, rare sorted export | `Dictionary` + sort once when exporting |
| Need `Keys[i]` by rank | `SortedList` (even smaller static maps) |
| Fastest lookup, no order | Plain `Dictionary` — Section 1 |

**Production takeaway:** Karat expects **size and access pattern** reasoning — 30 hourly regions is the sweet spot for sorted maps; see **Program.cs** Section 8 pick-list.

---

#### Q4. (R) A catalog search feature stores product tags in a case-insensitive sorted map. QA reports duplicate logical tags after a Turkish-locale server deploy. Review:

**Answer:** `StringComparer.CurrentCulture` uses locale-sensitive rules — casing and ordering can differ by server culture (Turkish **I/i** is the classic trap). For stable product-tag identity, use **`StringComparer.OrdinalIgnoreCase`** (chapter Section 7) so `"CSharp"` and `"csharp"` are the same key and indexer assignment updates rather than duplicates.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Culture-sensitive comparer on logical keys | Duplicate or missing keys when culture changes across environments |
| Environment | Turkish locale vs en-US dev box | QA-only failures after regional deploy |
| API | Indexer update assumes comparer treats keys as equal | Two entries when comparer says keys differ |

**Fix (priority order):**

1. Construct with `StringComparer.OrdinalIgnoreCase` — matches **Program.cs** `DemoCustomComparer`.
2. Use `Add` only when you want duplicate detection to throw; use indexer when upserting counts.
3. Reserve `CurrentCulture` for **display sort** (UI lists), not for canonical tag keys in services.

```csharp
var tagsByCount = new SortedDictionary<string, int>(
    StringComparer.OrdinalIgnoreCase)
{
    ["dotnet"] = 12,
    ["CSharp"] = 8,
    ["LINQ"] = 5
};

tagsByCount["csharp"] = 99; // updates single "CSharp"/"csharp" entry
```

**Production takeaway:** Sorted collections sort by **`IComparer<TKey>`**, not culture by default — explicit `OrdinalIgnoreCase` avoids locale drift between dev and production. See foundation **Strings** — culture vs ordinal.

---

#### Q5. (M) A pricing microservice benchmarks three shapes for a nightly job that inserts 50_000 random SKUs once, then performs 500_000 lookups:

**Answer:** `SortedList` backs keys and values with **parallel arrays**. Each insert finds the slot with binary search then **shifts** remaining elements — **O(n)** per insert, so 50_000 random inserts approach **O(n²)** total work. `Dictionary` averages **O(1)** insert; `SortedDictionary` uses a red-black tree at **O(log n)** per insert with no shifting — which is why `SortedList` dominates the load phase despite similar O(log n) lookup afterward.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Array shift on every `SortedList.Add` | Load phase orders of magnitude slower at 50k entries |
| Benchmark misread | Blaming "sorted" generically | Wrong fix — might avoid all sorted types instead of swapping list vs tree |
| Pattern | `ContainsKey` guard before every add | Extra lookup; still dwarfed by shift cost on `SortedList` |

**Rule of thumb (Section 9):**

| Need | Pick |
|---|---|
| Fastest lookup, order irrelevant | `Dictionary` |
| Sorted keys + access by rank (`Keys[i]`) | `SortedList` — small / mostly static |
| Sorted keys + frequent inserts/removes | `SortedDictionary` |
| Membership only, no values | `HashSet` |

**Production takeaway:** Karat pairs benchmark numbers with **mechanism** — array shift vs tree rebalance — not just "sorted is slower." Nightly bulk load + heavy lookup → `Dictionary` or `SortedDictionary`; `SortedList` only if you need index-by-rank on a small final map.

---

#### Q6. (R) A developer ports a `Dictionary` helper to sorted collections but copies the wrong comparer interface. Review:

**Answer:** `SortedDictionary` and `SortedList` constructors take **`IComparer<TKey>`**, not **`IEqualityComparer<TKey>`**. The snippet fails at compile time (`SkuIgnoreCaseEquality` does not implement `IComparer<string>`). If coerced, the collection would still sort/compare by the wrong contract — hash-based equality does not define sort order. Pass `StringComparer.OrdinalIgnoreCase` (implements `IComparer<string>`) or a custom `IComparer<string>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `IEqualityComparer` passed to sorted ctor | CS1503 — type mismatch |
| Conceptual | Confused hash equality with sort order | Dictionary/HashSet vs sorted maps — Section 7 comment |
| Runtime (if bypassed) | Wrong or default ordering | Duplicate logical keys or unexpected sort sequence |

**Fix (priority order):**

1. Use built-in comparer: `new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase)`.
2. For custom rules, implement **`IComparer<TKey>`** (like `ScoreDescendingComparer` in **Program.cs**), not `IEqualityComparer`.
3. Keep `SkuIgnoreCaseEquality` for `Dictionary<string, T>` / `HashSet<string>` only.

```csharp
var reorderQty = new SortedDictionary<string, int>(
    StringComparer.OrdinalIgnoreCase)
{
    ["ZEBRA-CLIP"] = 40,
    ["ALPHA-PAD"] = 120
};

reorderQty["alpha-pad"] = 200; // updates existing key
```

**Production takeaway:** Karat stacks **interface confusion** with collection choice — `IEqualityComparer` for hash tables, `IComparer` for sorted types. See **Program.cs** QUICK REFERENCE — "Not IEqualityComparer."
