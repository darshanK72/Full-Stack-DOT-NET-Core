# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/03. List`

---

#### Q1. (R) A nightly import job loads 500,000 shipment SKUs into a `List<string>` by calling `Add` one at a time in a loop. Memory profiling shows repeated large allocations and GC pressure. Review the pattern below. What is happening internally, and how would you fix it?

```csharp
public static List<string> LoadSkusFromFeed(IEnumerable<string> feedLines)
{
    var skus = new List<string>();
    foreach (var line in feedLines)
    {
        skus.Add(line.Trim());
    }
    return skus;
}
```

**Answer:** Each time `Count` exceeds `Capacity`, `List<T>` allocates a new backing array (typically double the previous size), copies every existing element, and discards the old array — so repeated growth on a half-million-item load causes many intermediate large allocations and full copies before the final size is reached.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | No initial capacity hint | Repeated resize + copy: O(n) per growth step → O(n²) total copy work for n adds |
| Memory | Discarded backing arrays until GC | GC pressure spikes during bulk import; LOH pressure for large string lists |
| Operability | `Clear()` later keeps high `Capacity` | Memory retained after import if list is reused without `TrimExcess()` |

**Fix (priority order):**

1. Pre-size when count is known or estimable: `new List<string>(capacity: 500_000)` or `new List<string>(feedLines as ICollection<string> ?? feedLines.ToList())` when the source exposes count.
2. Prefer `AddRange` over per-item `Add` when inserting a batch — one resize check for the whole range.
3. After bulk deletes, call `TrimExcess()` if the list will stay small long-term to release unused backing array memory.
4. For truly massive feeds, consider streaming processing instead of materializing everything into one list.

```csharp
public static List<string> LoadSkusFromFeed(IReadOnlyCollection<string> feedLines)
{
    var skus = new List<string>(feedLines.Count);
    foreach (var line in feedLines)
    {
        skus.Add(line.Trim());
    }
    return skus;
}
```

**Production takeaway:** `List<T>` growth is amortized O(1) per `Add`, but only if you avoid pathological resize storms — Karat tests whether you know `Capacity` doubles (0 → 4 → 8 → 16 …) and that pre-sizing is a one-line production win. See **Program.cs** Section 3 — Count / Capacity.

---

#### Q2. (R) A warehouse service removes cancelled dock labels during iteration. In staging it throws intermittently. Review this method — what breaks, and what is the correct fix?

```csharp
public void PurgeCancelledLabels(List<string> dockLabels, HashSet<string> cancelled)
{
    foreach (string label in dockLabels)
    {
        if (cancelled.Contains(label))
        {
            dockLabels.Remove(label);
        }
    }
}
```

**Answer:** Modifying a `List<T>` while iterating it with `foreach` invalidates the enumerator — the runtime throws `InvalidOperationException` ("Collection was modified") as soon as `Remove` shifts elements and bumps the list's version.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Remove` inside `foreach` | `InvalidOperationException` — job fails mid-purge |
| Correctness | Only first match removed per value anyway | Even if it didn't throw, partial removal + skipped items after shift |
| API choice | `Remove(object)` scans from start each call | O(n²) for many cancellations on a large list |

**Fix (priority order):**

1. **Iterate backwards by index** when removing in-place: `for (int i = dockLabels.Count - 1; i >= 0; i--)` then `RemoveAt(i)` — backward removal avoids index skips.
2. **Prefer `RemoveAll`** for predicate-based bulk delete: `dockLabels.RemoveAll(label => cancelled.Contains(label))` — single pass, no enumerator invalidation.
3. **Rebuild** if most items are removed: `dockLabels.RemoveAll(...)` or filter to a new list and replace reference.
4. Never call `Add`, `Insert`, `Remove`, `Clear`, or `Sort` on a collection during `foreach` on that same collection.

```csharp
public void PurgeCancelledLabels(List<string> dockLabels, HashSet<string> cancelled)
{
    dockLabels.RemoveAll(label => cancelled.Contains(label));
}
```

**Production takeaway:** This is one of the most common collection bugs in production services — Karat embeds it in realistic warehouse code to see if you diagnose enumerator invalidation, not just "don't modify while looping." See **Program.cs** Section 2 — CRUD / RemoveAll.

---

#### Q3. (M) A shipment validator checks whether each incoming pallet's SKU already exists in a queue of 50,000 items by calling `IndexOf` inside a loop. What is the performance problem, and what structure would you use instead?

```csharp
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    foreach (var item in incoming)
    {
        if (queue.IndexOf(item) < 0)
        {
            return false;
        }
    }
    return true;
}
```

**Answer:** `IndexOf` performs a linear scan O(n) over the entire list for every incoming item, giving O(n × m) behavior — and because `ShipmentItem` uses reference equality by default, the check may not even match logically equal SKUs unless `Equals` is overridden.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `IndexOf` in outer loop | 50k × incoming count comparisons — timeouts under peak load |
| Correctness | Default reference equality on `ShipmentItem` | Two objects with same SKU may not compare equal — false negatives |
| Design | List is ordered sequence, not lookup index | Wrong tool for membership-by-key checks |

**Fix (priority order):**

1. Build a **`HashSet<string>`** (or `Dictionary<string, ShipmentItem>`) of queued SKUs once — O(1) average lookup per incoming item.
2. If order must be preserved **and** you need key lookup, maintain **both**: `List<ShipmentItem>` for order + `HashSet<string>` for membership (common production pattern).
3. Override **`Equals`/`GetHashCode`** on `ShipmentItem` by SKU if set semantics should match domain identity — required for `IndexOf`/`Contains` to work by value.
4. Use **`Exists(predicate)`** only for single checks — still O(n) per call; does not fix the nested-loop cost.

```csharp
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    var queuedSkus = new HashSet<string>(queue.Select(q => q.Sku));
    return incoming.All(item => queuedSkus.Contains(item.Sku));
}
```

**Production takeaway:** `List<T>` search helpers (`IndexOf`, `Contains`, `Find`) are fine for small lists or rare checks — Karat uses scale (50k items) to force the jump to hash-based lookup. See **Program.cs** Section 12 — Dictionary preview vs List scan.

---

#### Q4. (D) Two developers search a pallet-count list for the first value over 20. One uses `List.Find`; the other uses LINQ `FirstOrDefault`. When would you prefer each, and what subtle difference matters for value types?

```csharp
List<int> palletCounts = GetPalletCounts();

int a = palletCounts.Find(n => n > 20);
int b = palletCounts.FirstOrDefault(n => n > 20);
```

**Answer:** For `List<int>`, both scan from index 0 and stop at the first match — behavior is equivalent here — but `Find` avoids LINQ's iterator allocation and is the idiomatic in-place search on `List<T>`; the important trap is that both return **`default(T)`** when nothing matches (`0` for `int`, not "no result").

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `default(int)` is `0` when no match | Cannot distinguish "found zero" from "not found" without `Exists` or nullable |
| Performance | LINQ adds delegate + enumerator overhead | Negligible on small lists; matters in hot loops on large lists |
| Consistency | Mixing styles across codebase | Team readability — pick one pattern per layer |

**When to prefer each:**

- **`List.Find` / `Exists` / `FindAll`:** Hot paths on materialized `List<T>` already in memory; mutating-list APIs (`FindAll` returns new list); no extra `using System.Linq`.
- **LINQ (`FirstOrDefault`, `Where`, `Any`):** Composing over `IEnumerable<T>`, deferred pipelines, or when the source may not be a list — keeps query chains uniform.
- **Neither alone for "maybe absent" value types:** Use `int? result = palletCounts.Cast<int?>().FirstOrDefault(n => n > 20)` or check `Exists` first, or return a tuple/bool+value.

**Production takeaway:** Karat tests API semantics, not LINQ religion — `Find` vs `FirstOrDefault` on a `List<int>` is a wash for performance, but **`default(T)` ambiguity** on value types breaks business rules silently. See **Program.cs** Section 4 — Find / Exists.

---

#### Q5. (R) A `ShipmentQueueService` exposes its internal lane list directly to API callers. Review the property and usage — what can go wrong in production, and how would you expose the data safely?

```csharp
public class ShipmentQueueService
{
    private readonly List<string> _lanes = new() { "Lane-1", "Lane-2" };

    public List<string> Lanes => _lanes;

    public void Reassign(string sku, string lane)
    {
        _lanes.Add(lane);
    }
}

// Controller
var lanes = _queueService.Lanes;
lanes.Clear();
lanes.Add("Hijacked-Lane");
```

**Answer:** Returning the live `List<string>` breaks encapsulation — any caller can mutate, clear, or replace elements in the service's internal state without going through `Reassign`, causing invariant violations and race conditions if the service is shared.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Public mutable collection escape | Callers bypass validation; `Clear()` wipes production lanes |
| Encapsulation | `List<T>` exposes `Add`/`Remove`/`Sort` | Cannot audit or log changes; hard to evolve to new rules |
| Concurrency | Shared list reference across requests | One request mutates while another reads — corrupt state under load |
| API contract | Return type promises mutability | Consumers depend on side-effecting the service internals |

**Fix (priority order):**

1. Expose **`IReadOnlyList<string>`** backed by **`AsReadOnly()`** or return **`_lanes.ToArray()`** / **`[.. _lanes]`** snapshot when callers must not see live mutations.
2. Prefer **`IReadOnlyList<string> Lanes => _lanes.AsReadOnly()`** for a live read-only view — note underlying list changes still appear (Section 8 behavior).
3. For strict immutability from outside, return a **copy**: `return _lanes.ToList()` or `return (IReadOnlyList<string>)_lanes.ToArray()` — higher allocation, safest for public APIs.
4. Route all mutations through **methods** on the service (`AddLane`, `RemoveLane`) that enforce rules and logging.

```csharp
public IReadOnlyList<string> Lanes => _lanes.AsReadOnly();

public void AddLane(string lane)
{
    if (string.IsNullOrWhiteSpace(lane)) throw new ArgumentException(nameof(lane));
    _lanes.Add(lane);
}
```

**Production takeaway:** `AsReadOnly()` prevents mutation through the wrapper but not through leaked `List<T>` references — Karat stacks encapsulation + API surface design. See **Program.cs** Section 8 — AsReadOnly.

---

#### Q6. (P) A singleton background worker and several API threads share one static `List<ShipmentItem>` for the live shipment queue. Under load, counts become wrong and the process occasionally throws. Explain why `List<T>` is unsafe here and what pattern you would use instead.

```csharp
public static class ShipmentHub
{
    public static readonly List<ShipmentItem> LiveQueue = new();

    public static void Enqueue(ShipmentItem item) => LiveQueue.Add(item);

    public static void ProcessNext()
    {
        if (LiveQueue.Count > 0)
        {
            var next = LiveQueue[0];
            LiveQueue.RemoveAt(0);
            Ship(next);
        }
    }
}
```

**Answer:** `List<T>` is not thread-safe — concurrent `Add`, `RemoveAt`, and reads can corrupt internal array state, lose elements, throw `ArgumentOutOfRangeException`, or throw during enumeration because another thread resized or removed items mid-operation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Unsynchronized `Add` + `RemoveAt` | Lost updates, torn reads, occasional exceptions |
| Correctness | `Count > 0` then `[0]` is not atomic | Another thread can dequeue between check and index — race |
| Architecture | Static mutable shared state | Cannot scale out across processes; hidden global coupling |
| Observability | Intermittent failures under load | Passes locally; fails in production peak traffic |

**Fix (priority order):**

1. **`lock` around all queue operations** on a private list if you must share in-process state — simplest fix, limits throughput.
2. Prefer **`ConcurrentQueue<ShipmentItem>`** or **`Channel<ShipmentItem>`** for producer/consumer patterns — designed for concurrent enqueue/dequeue.
3. Remove **static mutable** queues from business logic — inject a **scoped or singleton service** with explicit thread-safe storage; use database/message broker for multi-instance deployments.
4. Never expose the raw list publicly (see Q5) — wrap in a thread-safe API.

```csharp
private static readonly object Gate = new();
private static readonly List<ShipmentItem> LiveQueue = new();

public static void Enqueue(ShipmentItem item)
{
    lock (Gate) { LiveQueue.Add(item); }
}

public static bool TryDequeue(out ShipmentItem? item)
{
    lock (Gate)
    {
        if (LiveQueue.Count == 0) { item = null; return false; }
        item = LiveQueue[0];
        LiveQueue.RemoveAt(0);
        return true;
    }
}
```

**Production takeaway:** `List<T>` documentation explicitly states it is not thread-safe — Karat pairs this with singleton/static patterns to test whether you reach for synchronization or the right concurrent collection. For new code, `Channel<T>` or `ConcurrentQueue<T>` beats hand-rolled locks on `List<T>`.
