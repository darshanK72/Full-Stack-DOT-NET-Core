# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/04. Grouping`

---

#### Q1. (R) A support dashboard builds assignee buckets once at startup, then mutates shared `Department` objects when tickets are reassigned. Review this grouping code. What breaks after reassignment, and how do you fix it?

**Answer:** `ToLookup` indexes by the key object's hash code and equality at build time. Mutating `Dept.Code` after the lookup is built corrupts the internal dictionary — the bucket no longer matches the mutated key, so counts and indexer queries return stale or empty results even though the same object instance is reused.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Key design | Mutable reference type (`Department`) used as `TKey` | Changing `Code` changes hash/equality after insertion |
| Data model | Multiple tickets share one `Department` instance | One in-place edit affects every ticket referencing it |
| Caching | `ILookup` built once, keys mutated later | Dashboard shows wrong headcount; reassignment appears lost |
| Correctness | Query uses pre-mutation `sharedHwDept` reference | Indexer may miss rows that logically moved to `"NET"` |

**Fix (priority order):**

1. **Do not mutate keys** after grouping — treat keys as immutable value snapshots (`string Code`, `record`, or `ValueTuple`).
2. If department can change, **rebuild the lookup** after reassignment (`board.ToLookup(...)`) or update a domain store keyed by stable id, not mutable objects.
3. Prefer **value-type or string keys**: `ToLookup(t => t.Dept.Code)` or `ToLookup(t => t.DeptId)` instead of the whole `Department` object.
4. If shared mutable graphs are required, use **`Select` to project an immutable key** at grouping time: `GroupBy(t => t.Dept.Code)` — the string snapshot won't change when the object mutates later (but existing buckets still won't auto-move rows; rebuild or use ids).

```csharp
// Immutable key at partition time
ILookup<string, Ticket> byDeptCode = board.ToLookup(t => t.Dept.Code);

// Reassignment: change ticket's dept id/code, then rebuild lookup
byDeptCode = board.ToLookup(t => t.Dept.Code);
```

**Production takeaway:** `GroupBy`/`ToLookup` assume stable keys for the lifetime of the result — Karat tests whether you treat mutable reference keys like dictionary keys (never mutate after insert). See **Program.cs** Section 8 — composite/value keys; Section 12 — `ToLookup` immediate indexing.

---

#### Q2. (R) A nightly report caches `GroupBy` results in a field so the web tier can reuse them all day. Review this service. What is wrong with treating `IEnumerable<IGrouping<…>>` as a snapshot, and how do you materialize correctly?

**Answer:** `GroupBy` is **deferred** — storing `IEnumerable<IGrouping<…>>` only caches the query definition, not the partition. Each enumeration re-walks the **live** source sequence, so mutations to `liveBoard` after `Refresh` change counts, and multiple calls are inconsistent if the list changes between them.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Deferred execution | `_byPriority` is lazy `IEnumerable` | No snapshot — `GroupBy` re-runs against current `board` |
| Source coupling | `Refresh` captured `board` reference implicitly via closure in `GroupBy` iterator | `liveBoard.Add(...)` visible on next `Count()` |
| Cache semantics | Field named/labeled as cache but not materialized | Non-deterministic report numbers under concurrent ticket updates |
| API design | `GetCount` mutates `liveBoard` as side effect | Hidden coupling between caller and cache freshness |

**Fix (priority order):**

1. **Materialize** when you need a stable snapshot — outer and inner:

```csharp
private IReadOnlyList<(string Priority, List<Ticket> Tickets)>? _byPriority;

public void Refresh(IEnumerable<Ticket> board)
{
    _byPriority = board
        .GroupBy(t => t.Priority)
        .Select(g => (g.Key, g.ToList()))
        .ToList();
}
```

2. Or use **`ToLookup`** for keyed random access with immediate build: `board.ToLookup(t => t.Priority)`.
3. Pass **`IReadOnlyList<Ticket>`** into `Refresh` and do not mutate the same list afterward — copy if the live board continues to change: `board.ToList()` before grouping.
4. Remove side effects from `GetCount` — counting should not `Add` to the source list.

**Production takeaway:** Caching `GroupBy` without `ToList`/`ToLookup` is a common production bug — the partition is not frozen. See **Program.cs** Section 2 — deferred `IEnumerable<IGrouping<…>>`; Section 12 — `ToLookup` runs immediately.

---

#### Q3. (P) An API endpoint receives 50k tickets and must answer "how many tickets per assignee?" for **each** of 200 assignee names in a loop (authorization filter). A developer uses deferred `GroupBy` inside the loop. Review the pattern and choose the correct LINQ operator for production.

**Answer:** Calling `GroupBy` inside the loop repartitions the entire 50k sequence **200 times** — roughly O(assignees × n) with repeated hash bucketing. Build **`ToLookup` once** (or `GroupBy` once then index) and read each assignee in O(1) per key.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `GroupBy` per loop iteration | ~200 full scans/partitions of 50k rows |
| Algorithm | `First(g => g.Key == assignee)` linear search per iteration | Adds O(groups) on top of repeated GroupBy |
| Scalability | Acceptable in dev with 12 tickets; fails under Karat-scale data | Timeouts, thread-pool pressure on API |
| Operator choice | Deferred `GroupBy` used for repeated random access by key | Wrong tool — `ILookup` exists for this |

**Fix (priority order):**

1. Build lookup **once**:

```csharp
ILookup<string, Ticket> byAssignee = board.ToLookup(t => t.Assignee);

foreach (string assignee in assignees)
    counts[assignee] = byAssignee[assignee].Count();
```

2. Or single `GroupBy` + dictionary: `board.GroupBy(t => t.Assignee).ToDictionary(g => g.Key, g => g.Count())`.
3. If you only need counts, project in one pass with `GroupBy` + result selector (Section 6) — no inner loop over assignees list required.
4. For very large payloads, consider DB-side `GROUP BY` instead of in-memory LINQ.

**Production takeaway:** `GroupBy` = one sequential walk when you enumerate; `ToLookup` = one eager pass + O(1) key access — Karat pairs them to test operator selection, not syntax recall. See **Program.cs** Section 12 — `ToLookup` vs `GroupBy` table.

---

#### Q4. (R) A tree-view UI renders Category → Priority → tickets using nested `GroupBy`. Product later complains the page times out on a 120k-row export. Review the nesting approach vs a flat composite key. What is inefficient here, and how would you refactor?

**Answer:** Nested `GroupBy` is **correct** but performs two partition passes and allocates intermediate `IGrouping` hierarchies. For large flat exports, a **single** `GroupBy` with a composite key `(Category, Priority)` is one pass, simpler to sort, and easier to paginate — matching Section 8 and the chapter note that deep nesting should stay shallow.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Outer `GroupBy` + inner `GroupBy` per category | Two full partitioning passes over the data |
| Allocations | Many short-lived `IGrouping` iterators and `List<PriorityNode>` | GC pressure on 120k rows |
| Maintainability | Tree built in nested loops | Harder to stream/page than flat `(Category, Priority)` groups |
| Ordering | `.OrderBy` on each nesting level | Repeated sort work; flat key sorts once |

**Fix (priority order):**

1. **Flat composite key** when UI can derive hierarchy from two fields:

```csharp
var flat = board
    .GroupBy(t => (t.Category, t.Priority))
    .OrderBy(g => g.Key.Category)
    .ThenBy(g => g.Key.Priority);
```

2. If tree shape is required, build from flat groups in one projection rather than re-partitioning members.
3. **Stream/paginate** — don't `ToList()` every title list for full export; project counts or page keys first.
4. Keep nested `GroupBy` for **small** in-memory boards (demo size in **Program.cs** Section 11) — not large exports.

**Production takeaway:** Nested grouping reads well for tutorials; production reports at scale favor one composite `GroupBy` unless the inner dimension is tiny. See **Program.cs** Section 11 — nested basics + "keep nesting shallow"; Section 8 — `(Category, Priority)` tuple keys.

---

#### Q5. (M) Imported tickets allow `Category` to be null when the CSV field is blank. A developer groups and then tries to fetch the "Hardware" bucket with `First`. Review behavior for null keys and the lookup below.

**Answer:** `GroupBy`/`ToLookup` allow **null keys** — all null categories land in **one** bucket. `lookup[null]` returns that bucket (empty sequence if none). `First(g => g.Key == "Hardware")` throws **`InvalidOperationException`** if no Hardware group exists; null-key tickets are **not** in the Hardware group.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Null keys | `string?` key selector produces one null bucket | Uncategorized rows grouped together — easy to overlook |
| API misuse | `First` without `FirstOrDefault` | Runtime throw when "Hardware" absent from import batch |
| Lookup semantics | `lookup[null]` does not throw | Safe count for uncategorized — unlike `Dictionary` duplicate concerns |
| Comparison | `g.Key == "Hardware"` | Null keys never match; use explicit null handling for uncategorized |

**Fix (priority order):**

1. Use **`FirstOrDefault`** or **`TryGet`-style** access:

```csharp
var hardware = byCategory.FirstOrDefault(g => g.Key == "Hardware");
int hwCount = hardware?.Count() ?? 0;
```

2. For null bucket: `int uncategorized = lookup[null].Count();` — valid; `lookup.Contains(null)` is `true` only if at least one null key existed at build time.
3. Normalize at import: `Category = raw?.Trim() ?? "Uncategorized"` if business rules reject null keys.
4. Document that **null is a valid key** — distinct from missing key in `ILookup` (missing non-null key → empty sequence, not throw).

**Production takeaway:** Null keys group correctly but surprise teams expecting SQL `GROUP BY` null handling in reports — always handle the null bucket explicitly. See **Program.cs** Section 12 — missing key returns empty sequence; grouping keys can be any type including null.

---

#### Q6. (D) You are designing a ticket-routing service. Two paths are proposed:

- **Path A:** `board.GroupBy(t => t.Assignee)` — deferred, walk groups when building each route batch.
- **Path B:** `board.ToLookup(t => t.Assignee)` — built once after each poll from the queue.

When would you choose each in production (single-pass report vs repeated random access by assignee), and what are the trade-offs for memory, staleness, and missing keys?

**Answer:** Use **`GroupBy` (Path A)** when you will enumerate every group **once** in order (summary report, export) — deferred execution avoids building hash tables you won't use. Use **`ToLookup` (Path B)** when the same partitioned data serves **many random lookups by assignee** (routing, SLA checks per agent) — one O(n) build, then O(1) per key.

- **Memory:** `ToLookup` allocates the full multi-map up front; `GroupBy` holds iterator state until enumeration — lower peak memory if you never materialize all groups.
- **Staleness:** Both reflect the source at enumeration/build time. After queue poll N+1, rebuild `ToLookup`; a stored `GroupBy` without materialization will see live changes on re-enumeration (same staleness rules as Q2).
- **Missing keys:** `ILookup[name]` returns **empty sequence** (no throw); `GroupBy` requires scan/`FirstOrDefault` to find a key. Prefer `Contains(key)` before assuming assignee exists.
- **Single-pass report:** `GroupBy` + `Select` result selector (Section 6) emits one row per assignee without indexer — no lookup table needed.
- **Repeated access:** `ToLookup` matches **Program.cs** Section 12 — `map["Ada"]`, `Contains`, missing → empty.

**Production takeaway:** Operator choice is about access pattern, not syntax — deferred walk vs immediate index is the Karat judgment call. See **Program.cs** Section 12 comparison table and Quick Reference — `GroupBy` deferred / `ToLookup` immediate.
