# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/07. Set Operations`

---

#### Q1. (R) A catalog sync job builds a master SKU list by merging Web and Marketplace feeds. QA reports duplicate SKUs in the export even though both feeds were loaded into `HashSet<CatalogItem>` instances constructed with `CatalogItemBySkuComparer`. Review:

**Answer:** LINQ `Union` on `IEnumerable<CatalogItem>` uses **default sequence equality** (`EqualityComparer<CatalogItem>.Default` → **reference equality** for classes), not the comparer baked into the `HashSet` instances. Two different `CatalogItem` objects with the same SKU remain distinct in the union unless you pass `bySku` explicitly to `Union`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `HashSet.Union` still invokes `Enumerable.Union` | HashSet's internal comparer does not flow to LINQ |
| API confusion | Assumed construction comparer applies to set ops | Duplicate SKUs in master export — Web and Market rows both kept |
| Data quality | `CatalogItem` has no value equality | Same business identity, different object references |

**Fix (priority order):**

1. Pass the comparer to LINQ: `webSet.Union(marketSet, bySku)` or `webFeed.Union(marketFeed, bySku)` directly on the sequences.
2. Alternatively use `UnionBy(marketFeed, item => item.Sku)` (.NET 6+) when identity is a single key field.
3. When materializing, do not assume prior `HashSet` construction fixed equality for downstream LINQ.

```csharp
IEnumerable<CatalogItem> master = webFeed.Union(marketFeed, bySku);
// or: webFeed.UnionBy(marketFeed, item => item.Sku);
```

**Production takeaway:** LINQ set operators are **comparer-agnostic unless you pass one** — see **Program.cs** Section 6 and quick reference. Same trap as HashSet → LINQ Union in the HashSet chapter.

---

#### Q2. (R) An ops dashboard deduplicates a noisy Web import before pricing review. The developer expects one row per SKU. Review:

**Answer:** `Distinct()` without a comparer uses `EqualityComparer<CatalogItem>.Default`, which for a plain class means **reference equality** — every `new CatalogItem(...)` is unique even when `Sku`, `Name`, and price match. The three SKU-300 import rows all survive.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Default reference equality on reference type | Duplicate SKUs in dashboard and downstream pricing |
| API misuse | `Distinct()` assumed business-key dedup | Silent data-quality bug — count looks "distinct" but isn't by SKU |
| Design | No `IEquatable<T>` or comparer supplied | Same lesson as **Program.cs** Section 4d — intentional plain class |

**Fix (priority order):**

1. Pass `CatalogItemBySkuComparer`: `webFeed.Distinct(bySku)`.
2. Prefer `DistinctBy(item => item.Sku)` when only one key defines identity (.NET 6+).
3. Long-term: immutable record or `IEquatable<CatalogItem>` if value equality is the default for the type.

```csharp
int uniqueCount = webFeed.Distinct(bySku).Count();           // 4
// or: webFeed.DistinctBy(item => item.Sku).Count();
```

**Production takeaway:** Karat tests whether you know **default equality is reference-based for classes** — Distinct does not infer SKU from property values. See **Program.cs** Sections 1 and 4d.

---

#### Q3. (R) A nightly ETL appends marketing tags from two channels into a single analytics table. The pipeline owner insists "we only need one copy of each tag." Review:

**Answer:** `Concat` **appends** both sequences and **keeps every element**, including within-sequence duplicates (`"hardware"` twice, `"linq"` twice) and cross-sequence repeats. The stakeholder wanted **set merge** semantics — use `Union`, which yields each unique tag once with first-seen order from the first sequence, then new items from the second.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Concat` used where set uniqueness required | Duplicate rows in analytics DB; inflated counts |
| Operator confusion | Concat = append; Union = unique merge | Wrong operator choice — see **Program.cs** Section 9 |
| Data quality | 11 rows vs 8 unique tags | Reporting and billing on tag volume skewed |

**Fix (priority order):**

1. Replace with `webTags.Union(marketTags)` for case-sensitive default, or pass `StringComparer.OrdinalIgnoreCase` if case should not split tags.
2. Use `Concat` only when every row must be preserved (audit trail, ordered append).
3. Document operator choice in ETL specs — "append" vs "unique membership."

```csharp
IEnumerable<string> combined = webTags.Union(marketTags, StringComparer.OrdinalIgnoreCase);
// Union count: 8 unique tags (hardware, FastShip, linq, api, azure, fastship, docker)
```

**Production takeaway:** **Concat keeps duplicates; Union removes them** — one of the most common LINQ set-operation mistakes in ETL. See **Program.cs** Section 9 comparison table.

---

#### Q4. (R) After a "fix typo in SKU" feature ships, the Web-only listing report returns fewer rows than inventory expects. Review:

**Answer:** `GetHashCode` was computed from `Sku` at enumeration time and placed each item in a hash bucket keyed to that value. Mutating `Sku` after the first `Except` enumeration leaves the object in the **wrong bucket** for any **re-executed** deferred query — `Contains`-style membership fails even though the in-memory list still holds the reference. Mutable fields used in `Equals`/`GetHashCode` break the hash contract for all LINQ set operators (Distinct, Union, Intersect, Except).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable `Sku` participates in comparer hash/equality | Re-querying `Except` misses updated rows |
| Hash contract | Hash at first enumeration ≠ hash after mutation | Orphaned logical membership — same trap as HashSet mutable keys |
| Pipeline | Deferred execution re-runs set logic on mutated state | Inconsistent counts between materialized list and fresh LINQ |

**Fix (priority order):**

1. Make identity immutable — `public string Sku { get; }` via constructor, matching **Program.cs** `CatalogItem`.
2. If SKU must change, treat it as **remove old + add new** (or rebuild the feed), never in-place edit on objects already used in set pipelines.
3. Materialize with `ToList()` once and avoid re-enumerating deferred queries after mutating compared fields — but immutability is the real fix.

```csharp
public sealed class CatalogItem
{
    public string Sku { get; }  // init-only identity
    // ...
}
```

**Production takeaway:** Set operators use hash buckets internally — **mutable equality fields cause silent lookup failures**, not exceptions. Same rule as Dictionary keys and HashSet elements.

---

#### Q5. (R) A data-quality check compares two tag pipelines with `SequenceEqual` after a refactor. One pipeline uses `Union` with `StringComparer.OrdinalIgnoreCase`; the other calls `Union` with no comparer. Review:

**Answer:** Parameterless `Union` for `string` uses `EqualityComparer<string>.Default`, which is **Ordinal, case-sensitive**. `"FastShip"` (from Web, first occurrence) and `"fastship"` (from Market) are **different** elements, so pipeline B can yield **more** distinct tags than pipeline A when case variants exist across feeds.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Default Ordinal equality on user-facing tags | Case variants treated as separate tags in pipeline B |
| Consistency | One pipeline case-insensitive, one case-sensitive | `SequenceEqual` false; analytics drift between environments |
| Design | Assumed strings dedupe "logically" without comparer | `"API"` vs `"api"` split unless comparer specified |

**Fix (priority order):**

1. Use the **same comparer in both pipelines**: `webTags.Union(marketTags, StringComparer.OrdinalIgnoreCase)`.
2. Align with business rule — marketing tags usually ignore case; SKU codes often use `Ordinal`.
3. When validating pipelines, pass the comparer to `SequenceEqual` too: `pipelineA.SequenceEqual(pipelineB, comparer)`.

```csharp
var comparer = StringComparer.OrdinalIgnoreCase;
IEnumerable<string> pipelineA = webTags.Union(marketTags, comparer);
IEnumerable<string> pipelineB = webTags.Union(marketTags, comparer);
bool pipelinesMatch = pipelineA.SequenceEqual(pipelineB, comparer);
```

**Production takeaway:** Default string equality is **case-sensitive Ordinal** — see **Program.cs** Sections 4c and 6 (`Union` with `StringComparer.OrdinalIgnoreCase`). Karat pairs Union comparer mismatch with Distinct/Except defaults on the same feeds.

---

#### Q6. (R) A custom SKU comparer passes review but `Distinct` and `Union` intermittently keep duplicate SKUs. Review:

**Answer:** `Equals` compares SKU with **OrdinalIgnoreCase** but `GetHashCode` hashes with **Ordinal** (case-sensitive). Two items equal by comparer (`"SKU-100"` vs `"sku-100"`) can land in **different hash buckets**, so Distinct/Union fail to collapse them — the same contract violation that breaks `HashSet<T>` and `Dictionary<TKey,TValue>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `GetHashCode`/`Equals` use different case rules | Duplicate SKUs survive Distinct and Union |
| Hash contract | Equal objects must share hash code | Intermittent — only fails when casing differs |
| Code review | Easy to miss when `Equals` and `GetHashCode` look "similar" | Silent data-quality bug in catalog sync |

**Fix (priority order):**

1. Derive hash from the **same fields and same comparison** as `Equals`.
2. Add contract tests: if `Equals(a,b)` then `GetHashCode(a) == GetHashCode(b)`.
3. Match **Program.cs** `CatalogItemBySkuComparer` — both use `StringComparison.Ordinal` / `StringComparer.Ordinal` consistently (or both ignore case if business requires).

```csharp
public int GetHashCode(CatalogItem obj) =>
    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Sku);
// If using Ordinal in Equals, use StringComparer.Ordinal.GetHashCode(obj.Sku) — must match
```

**Production takeaway:** LINQ set operators **do not throw** on bad comparers — they return wrong membership. See **Program.cs** Section 2 contract and quick reference *Common mistakes* table.
