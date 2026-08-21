# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/03. Ordering`

---

#### Q1. (R) A warehouse pick-list API should sort by **Priority descending**, then **PlacedAt ascending** within the same priority. QA reports rush (`Priority == 3`) lines appear in random date order. Review the query. What is wrong, and what would you change?

**Answer:** A second `OrderBy` **replaces** the entire sort — it does not add a secondary key. After `.OrderBy(line => line.PlacedAt)`, only `PlacedAt` determines order; the earlier `OrderByDescending(Priority)` is discarded.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Second `OrderBy` instead of `ThenBy` | Rush orders no longer grouped above lower priorities |
| Domain logic | Pick-list rule needs multi-key sort | Warehouse walks aisles in wrong sequence within priority bands |
| API contract | Callers expect priority-first ordering | QA sees "random" dates among `Priority == 3` rows |

**Fix (priority order):**

1. Replace the second `OrderBy` with `ThenBy`: `.OrderByDescending(l => l.Priority).ThenBy(l => l.PlacedAt)`.
2. Type the intermediate result as `IOrderedEnumerable<FulfillmentLine>` when chaining so `ThenBy` stays visible in IntelliSense.
3. Add an integration test that asserts priority-3 rows sort by `PlacedAt` ascending among themselves.
4. Materialize with `.ToList()` at the API boundary if the sorted snapshot must not change between response serialization steps.

```csharp
return openLines
    .Where(line => line.Priority >= 2)
    .OrderByDescending(line => line.Priority)
    .ThenBy(line => line.PlacedAt)
    .Select(line => new FulfillmentLineDto(line.OrderId, line.Priority, line.PlacedAt, line.Zone));
```

**Production takeaway:** Multi-key sorts are one `ThenBy` chain — a second `OrderBy` is one of the most common LINQ ordering bugs in reporting APIs. See **Program.cs** Section 6–7 and Quick Reference "second OrderBy instead of ThenBy."

---

#### Q2. (M) A developer unit-tests in-memory LINQ and ships this EF Core query. They assert priority-1 rows keep the same relative order as the import file when only `OrderBy(Priority)` is used — no `ThenBy`. What assumption fails in production, and how would you make ordering deterministic for the database?

**Answer:** **LINQ to Objects** `OrderBy` is a **stable** sort — equal keys keep source order — but **EF Core → SQL** does not guarantee the same tie behavior. Without an explicit secondary `ORDER BY` column, the database may return priority-1 rows in any order, and that order can change between executions or after index changes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Stability assumed across providers | In-memory test passes; production order differs |
| Testing | Test validates accidental source order, not business rule | False confidence — flaky or wrong pick sequences |
| Operability | No named tie-break column in SQL | Support cannot reproduce "which order came first" |

**Fix (priority order):**

1. Add an explicit business tie-break: `.OrderBy(l => l.Priority).ThenBy(l => l.PlacedAt)` (or `.ThenBy(l => l.OrderId)` for a unique key).
2. Rewrite tests to assert **key order**, not incidental import-file order — unless import order is a documented rule, encode it in `ThenBy`.
3. For pagination or cursor APIs, always include a unique final key so pages are stable.
4. Document in API specs: "sorted by Priority asc, then PlacedAt asc" — not "stable sort preserves import order."

```csharp
return await _db.FulfillmentLines
    .Where(line => line.Priority == 1)
    .OrderBy(line => line.Priority)
    .ThenBy(line => line.PlacedAt)
    .Select(line => line.OrderId)
    .ToListAsync(ct);
```

**Production takeaway:** Stability is a **LINQ to Objects** implementation detail — never rely on it for SQL, EF Core, or PLINQ without explicit `ThenBy` / `ORDER BY` columns. See **Program.cs** Section 8 — "Do not assume stability when ordering is translated to a database."

---

#### Q3. (R) A nightly export job logs pick-list metrics, then writes every sorted line. Under load the job slows and occasionally logs a different "first order" between steps. Review the method. What does deferred `OrderBy` do here, and how would you fix it?

**Answer:** `OrderBy` / `ThenBy` are **deferred** — each terminal operator (`Count`, `foreach`, `First`) **re-enumerates and re-sorts** the source. This method runs the full sort three times, and if `openLines` is a live or expensive sequence, work multiplies and ordering can diverge if the underlying data changes between passes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Three enumerations of `sorted` | Triple sort cost on large fulfillment feeds |
| Correctness | Source may mutate between `Count`, `foreach`, and `First` | "First order" metric may not match rows written in the loop |
| Observability | `Count()` then `First()` on deferred pipeline | Misleading metrics under concurrent updates |

**Fix (priority order):**

1. Materialize once after ordering: `var sorted = openLines.OrderByDescending(...).ThenBy(...).ToList();`
2. Use `sorted.Count`, `foreach`, and `sorted[0]` / `sorted.First()` on the same list snapshot.
3. If the source is `IQueryable`, push ordering to SQL with one `ToListAsync` — still one materialization point.
4. Avoid calling `Count()` on a deferred ordered sequence when you will enumerate again — use the list count.

```csharp
List<FulfillmentLine> sorted = openLines
    .OrderByDescending(line => line.Priority)
    .ThenBy(line => line.PlacedAt)
    .ToList();

_logger.LogInformation("Export row count: {Count}", sorted.Count);

foreach (FulfillmentLine line in sorted)
    writer.WriteLine($"{line.OrderId},{line.Priority},{line.PlacedAt:O}");

_metrics.RecordFirstOrder(sorted[0].OrderId);
```

**Production takeaway:** Treat deferred ordering like deferred filtering — **one materialization** when multiple passes are needed. See **Program.cs** Section 3 — deferred execution until `foreach` / `ToList`; Section 13 — enumeration triggers the sort.

---

#### Q4. (R) A customer directory endpoint returns lines sorted alphabetically by `Customer`. Sort order matches on a developer laptop but differs on the Linux API host; support tickets mention `"Acme Corp"` and `"acme corp"` appearing far apart. Review the handler. What comparison rules apply by default, and what would you change for a stable API contract?

**Answer:** `OrderBy(line => line.Customer)` uses `Comparer<string>.Default`, which is **culture-sensitive** and can differ by server locale. Default string ordering also treats casing ordinally within the culture rules — so `"Acme Corp"`, `"acme corp"`, and `"beta llc"` / `"Beta LLC"` may not group the way product or support expects.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Culture-dependent string sort | Different order on Windows dev box vs Linux container |
| UX / support | Case variants treated as separate clusters | Duplicate-looking customers scattered in the directory |
| API contract | Sort semantics undocumented | Clients cannot reproduce ordering offline |

**Fix (priority order):**

1. Pass an explicit comparer: `.OrderBy(line => line.Customer, StringComparer.OrdinalIgnoreCase)` for case-insensitive ASCII-safe API sorting.
2. If locale-aware sorting is required (e.g., Swedish `å`), set `CultureInfo` explicitly in startup and document it — do not rely on server default.
3. For display grouping, consider normalizing a sort key column in the database rather than sorting raw user-entered text.
4. Add API docs: "Customer sort: ordinal, case-insensitive" (or named culture).

```csharp
return openLines
    .OrderBy(line => line.Customer, StringComparer.OrdinalIgnoreCase)
    .Select(line => new CustomerDirectoryRow(line.OrderId, line.Customer))
    .ToList();
```

**Production takeaway:** **Never ship string `OrderBy` without naming the comparer** in public APIs — culture and casing are environment-dependent. See **Program.cs** Section 9a — `StringComparer.OrdinalIgnoreCase`; Quick Reference IComparer tips.

---

#### Q5. (D) A paginated fulfillment grid calls this repository method. Users report rows "jumping" between pages when they refresh — especially among lines that share the same priority. What ordering guarantee is missing, and how would you fix pagination?

**Answer:** `OrderByDescending(Priority)` alone leaves **ties unordered** at the database level. Among many `Priority == 2` rows, SQL may return them in any order — so `Skip` / `Take` page boundaries shift between requests when the engine picks a different tie order.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No secondary sort key for ties | Rows move between pages on refresh |
| UX | Unstable pagination | Users lose scroll position; duplicate/missing rows across pages |
| Design | Single-key sort treated as total order | Shared priority values are common in fulfillment data |

**Fix (priority order):**

1. Add deterministic tie-breakers: `.OrderByDescending(l => l.Priority).ThenBy(l => l.PlacedAt).ThenBy(l => l.OrderId)`.
2. Prefer a **unique** final key (`OrderId`) so every row has a fixed position in the total ordering.
3. For keyset/cursor pagination, encode the full sort key tuple in the cursor — not just priority.
4. Match UI copy to implementation: "Sorted by priority, then oldest first, then order id."

```csharp
var items = await _db.FulfillmentLines
    .OrderByDescending(line => line.Priority)
    .ThenBy(line => line.PlacedAt)
    .ThenBy(line => line.OrderId)
    .Skip(page * pageSize)
    .Take(pageSize)
    .Select(line => new FulfillmentLineDto(line.OrderId, line.Priority, line.PlacedAt))
    .ToListAsync(ct);
```

**Production takeaway:** Pagination requires a **total order** — primary `OrderBy` plus explicit `ThenBy` keys, ending with a unique column. Stability from in-memory LINQ tests does not fix SQL tie behavior. See **Program.cs** Section 8 — explicit `ThenBy` preferred over implicit stability.

---

#### Q6. (R) A teammate splits sorting across two private helpers to keep methods small. The project no longer builds. Review the chain. What type broke the `ThenBy` call, and how would you structure multi-key sorts in production code?

**Answer:** `OrderByDescending` returns `IOrderedEnumerable<T>`, but `SortByPriority` exposes `IEnumerable<FulfillmentLine>`. `ThenBy` exists only on `IOrderedEnumerable<T>` — so `AddPlacedAtTieBreak` cannot compile (CS1061).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `ThenBy` on `IEnumerable<T>` | Build blocked — CS1061 |
| Design | Sort chain split without preserving ordered type | Refactor accidentally drops secondary-key capability |
| Maintainability | Hidden requirement that callers pass already-ordered sequence | Future helpers may repeat the mistake |

**Fix (priority order):**

1. Return `IOrderedEnumerable<FulfillmentLine>` from the first sort step — or keep the full chain in one method / one expression.
2. Apply `ThenBy` in the same pipeline immediately after `OrderBy*`, matching **Program.cs** Section 7.
3. If helpers are needed, pass `IOrderedEnumerable<FulfillmentLine>` into the tie-break helper — do not widen to `IEnumerable` until the chain is complete.
4. For reusable sort profiles, use a static extension or named method that returns the full ordered query in one call.

```csharp
private static IOrderedEnumerable<FulfillmentLine> SortByPriority(IEnumerable<FulfillmentLine> lines) =>
    lines.OrderByDescending(line => line.Priority);

public IEnumerable<FulfillmentLineDto> GetPickList(IEnumerable<FulfillmentLine> openLines)
{
    return SortByPriority(openLines)
        .ThenBy(line => line.PlacedAt)
        .Select(line => new FulfillmentLineDto(line.OrderId, line.Priority, line.PlacedAt));
}
```

**Production takeaway:** `IOrderedEnumerable<T>` is not cosmetic — widening to `IEnumerable<T>` too early is how teams "forget" `ThenBy` at compile time. Keep multi-key sorts as one chained expression or typed ordered steps. See **Program.cs** Section 7 — return type table and pitfall example.
