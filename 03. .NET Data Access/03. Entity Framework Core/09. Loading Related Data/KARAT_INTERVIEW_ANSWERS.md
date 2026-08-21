# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/09. Loading Related Data`

---

#### Q1. (R) A list endpoint only needs order id, date, and customer name, but the repository loads the full graph before mapping. Review the method — what is the performance problem, and how do you fix it while keeping a single SQL round-trip?

**Answer:** The query over-fetches — it JOINs and materializes every `OrderLine` and `Product` row for 50 orders even though the DTO only needs scalar order fields and the customer name, wasting SQL I/O, network bandwidth, and heap memory before the mapping discards the graph.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Data loading | `Include` + `ThenInclude` on `Lines` and `Product` for a summary list | Pulls entire child graph into memory |
| API design | Entity graph loaded then projected in memory | ORM materializes full entities; GC pressure under concurrent list traffic |
| SQL shape | Wide JOIN across Orders → OrderLines → Products | Larger result set and slower query plan than needed |

**Fix (priority order):**

1. Replace eager Include with a **server-side projection** — EF translates `Select` to SQL that returns only required columns in one command:

```csharp
return await context.Orders
    .AsNoTracking()
    .OrderByDescending(o => o.OrderDate)
    .Take(50)
    .Select(o => new OrderListItemDto(o.OrderId, o.OrderDate, o.Customer!.Name))
    .ToListAsync(ct);
```

2. If you must stay on entities temporarily, drop `.Include(o => o.Lines).ThenInclude(l => l.Product)` — keep only `.Include(o => o.Customer)` when customer name is needed and lines are not.
3. Add query logging (`LogTo` or `QueryDiagnostics` from this chapter) in staging to compare row counts before/after.

**Production takeaway:** Include is for when you **use** the navigation graph; list endpoints that only need a flat shape should project in `IQueryable` — the same lesson as **OrderLoadingService.GetOrdersWithEagerLoading** vs a summary DTO. See foundation **ch.08 IQueryable** — deferred execution lets `Select` shape SQL.

---

#### Q2. (R) After eager-loading changes, the order-detail page shows line quantities but every `ProductName` is null. Review the query — what is wrong with the Include chain, and what SQL shape do you expect after the fix?

**Answer:** The query includes `Order.Lines` but never continues the chain with `ThenInclude(l => l.Product)`, so line entities materialize without their product navigation — `line.Product` stays null even though `Quantity` comes from the included line rows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Loading strategy | Missing `ThenInclude` after collection `Include` | Nested reference navigation not joined |
| Runtime | UI shows blank product names | Functional bug mistaken for mapping/serialization issue |
| ThenInclude depth | Chain stops at one level | Common Karat trap — `Include(o => o.Lines)` does not imply `Product` |

**Fix (priority order):**

1. Extend the chain to match **OrderLoadingService.GetOrdersWithEagerLoading**:

```csharp
return context.Orders
    .AsNoTracking()
    .Include(o => o.Customer)
    .Include(o => o.Lines)
        .ThenInclude(l => l.Product)
    .FirstOrDefault(o => o.OrderId == orderId);
```

2. Each new `Include` from the root starts a **new branch** — after `.Include(o => o.Customer)`, the next `.Include(o => o.Lines)` is another root branch, and `.ThenInclude(l => l.Product)` attaches to the **Lines** branch, not Customer.
3. Expect SQL with JOINs from `Orders` → `OrderLines` → `Products` (plus `Customers`), or two statements if split query is enabled.

**Production takeaway:** ThenInclude depth must match the object graph you dereference in the view — one missing level fails silently with null navigations. See **Program.cs** quick reference — eager loading chain example.

---

#### Q3. (M) A reporting job loads customers with all their orders and every order line. Under load, SQL row counts explode and memory spikes, but the team insists "it's one query so it must be efficient." Review the query — explain the cartesian product, and when you would use `AsSplitQuery()`.

**Answer:** A single SQL statement with JOINs across two collections (`Customer.Orders` and each order's `Lines`) produces a cartesian product — each customer row repeats for every combination of order × line in the result set, so EF must de-duplicate in memory and the database ships far more rows than entities returned.

- **Mechanism:** One `Include` on a collection plus `ThenInclude` into another collection multiplies rows in the flat JOIN result. Ten orders with five lines each can yield ~50 joined rows **per customer** before deduplication.
- **Symptoms:** Slow reports, high SQL `logical reads`, memory spikes, "one query" that transfers megabytes — worsens as order history grows.
- **`AsSplitQuery()`:** EF issues multiple SQL commands (e.g., customers, then orders, then lines) that avoid the wide JOIN cartesian product while still building the same graph — usually 2–4 round-trips vs one giant result.
- **When to use:** Multiple collection navigations or deep Include trees on reporting/read-only paths where a single JOIN would explode — as noted in **OrderLoadingService.cs** Section 4 preview.
- **Trade-off:** Split queries add round-trips; on latency-sensitive small graphs a single JOIN may be fine — profile with command logging.

**Production takeaway:** "One query" ≠ "one efficient query" — cartesian explosion is a classic EF production incident; split queries or projection are the fix. See **QueryDiagnostics** in this chapter to compare command count vs row volume.

---

#### Q4. (R) A developer refactors N+1 line loading to "explicit loading" but production still shows hundreds of SQL commands per request. Review the method — identify every issue and prioritize fixes.

**Answer:** Explicit loading never runs correctly here — `AsNoTracking()` detaches entities so `Entry(order).Load()` cannot attach and load navigations, and even if tracked, calling `Load()` twice per order in a loop reproduces an N+1 (1 + 2×N commands) instead of a batched Include or projection.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Tracking | `AsNoTracking()` then `Entry(order).Load()` | Load is a no-op or throws depending on version/state; navigations stay null |
| Performance | `Load()` inside `foreach` over N orders | 1 + 2N SQL commands (customer + lines each iteration) |
| Misapplied pattern | Explicit load used for bulk read | Explicit loading suits **conditional** navigations on a **tracked** root, not batch header lists |

**Fix (priority order):**

1. For bulk headers, use one query with Include or projection — not per-row Load:

```csharp
return await context.Orders
    .AsNoTracking()
    .Where(o => orderIds.Contains(o.OrderId))
    .Select(o => new OrderHeaderDto(o.OrderId, o.Customer!.Name, o.Lines.Count))
    .ToListAsync();
```

2. If explicit loading is required (conditional navigations after business logic), remove `AsNoTracking()` on the root query so the entity is **tracked**, and batch where possible — still prefer Include when you always need Customer + Lines.
3. Follow **OrderLoadingService.GetOrderWithExplicitLoading** — single tracked root, `Reference().Load()` and `Collection().Query().Include(...).Load()` when the context is open.

**Production takeaway:** Explicit loading requires tracking and still costs per Load call — swapping N+1 manual queries for N+1 `Load()` in a loop does not fix scalability. See **Program.cs** Section 7 and N+1 demo in Section 5.

---

#### Q5. (R) A fulfillment API should return only orders that have at least one line with `Quantity >= 2`, and each order should expose only those qualifying lines. The developer uses filtered Include but QA reports wrong orders in the response. Review the code — what is misunderstood about filtered Include, and how do you fix the business rule?

**Answer:** Filtered `Include` only filters **which child rows populate the collection** — it does **not** filter the root `Order` entities, so orders with no qualifying lines still appear with an empty `Lines` collection unless you add a root `Where`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | Treating filtered Include as root filter | Orders with zero bulk lines still returned |
| Business logic | Callers check `order.Lines.Count` expecting exclusion | Wrong orders enter fulfillment queue |
| API contract | Partial line graph without root predicate | Matches **GetOrdersWithFilteredLines** demo shape, not "orders that qualify" |

**Fix (priority order):**

1. Filter the root entity when the business rule applies to **which orders** are returned:

```csharp
return context.Orders
    .AsNoTracking()
    .Where(o => o.Lines.Any(l => l.Quantity >= minQuantity))
    .Include(o => o.Lines.Where(l => l.Quantity >= minQuantity))
    .OrderBy(o => o.OrderId)
    .ToList();
```

2. Keep filtered Include to avoid loading non-qualifying lines into the graph — both predicates should align on the same rule.
3. Document that filtered Include (EF Core 5+) translates to SQL on the collection subquery, not client-side filtering after a full load — see **OrderLoadingService** Section 8.

**Production takeaway:** Filtered Include answers "which related rows to attach," not "which parents match" — Karat tests whether you add `Where` on the root separately. See **Program.cs** quick reference — filtered Include example.

---

#### Q6. (D) An order API has three read paths: (A) list view — id/date/customer name only; (B) detail view — customer + lines + product names; (C) background job — load customer only when an order fails validation. For each path, choose **projection**, **Include/ThenInclude**, or **explicit Load** — and name one production failure mode if you pick the wrong strategy.

**Answer:** Match loading strategy to how much of the graph you dereference — project flat list data, Include the full detail graph in one or split queries, and explicit Load only for conditional navigations on a tracked entity.

| Path | Strategy | Why | Wrong choice → production failure |
|---|---|---|---|
| **(A) List view** | **Projection** (`Select` to DTO) | Only three scalars; no need for entity graph or change tracking | Include + ThenInclude → over-fetch (Q1): memory and SQL bloat on every list request |
| **(B) Detail view** | **Include / ThenInclude** (optionally `AsSplitQuery` if cartesian risk) | Always need Customer, Lines, Product — same as **GetOrdersWithEagerLoading** | Projection-only without planning → multiple round-trips or N+1 if UI walks navigations later |
| **(C) Validation job** | **Explicit Load** on tracked root after initial query | Customer needed only for failed orders — avoid loading customer for every order up front | Include Customer on bulk query → wasted JOINs when 95% pass validation; Load in loop without tracking → null Customer (Q4) |

- **(A)** Keep `AsNoTracking`; never Include lines on list endpoints.
- **(B)** Chain `.Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product)`; add `AsSplitQuery()` if metrics show cartesian row explosion (Q3).
- **(C)** Query orders tracked (no `AsNoTracking`), run validation, then `Entry(order).Reference(o => o.Customer).Load()` only in the failure branch — context must stay open until Load completes.

**Production takeaway:** The three EF loading modes from this chapter — eager, explicit, projection — are not interchangeable; Karat expects you to tie each HTTP/job shape to a strategy and name the failure mode (over-fetch, N+1, cartesian, null navigations) when you mismatch.

---
