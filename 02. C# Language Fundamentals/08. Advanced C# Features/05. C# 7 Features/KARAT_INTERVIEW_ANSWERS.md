# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/05. C# 7 Features/`

---

#### Q1. (R) Express VIP orders are routed to the standard express lane in production. Review this C# 7 switch with `when` guards (mirrors the warehouse routing demo):

**Answer:** The `Express` case without a `when` guard matches every express order first, so the `Express when order.IsHighValue` branch is unreachable dead code — high-value express orders never reach VIP routing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Unconditional `case OrderPriority.Express` precedes guarded case | VIP lane never selected; SLA breach on high-value express |
| Pattern matching | Misunderstanding first-match-wins switch semantics | Silent logic bug — compiles and runs |
| Testing | Tests that only assert "express → some express lane" pass | Mixed-batch production traffic exposes wrong lane |

**Fix (priority order):**

1. Reorder cases so **more specific patterns come first** — mirror the tutorial's `RouteOrder` in **Program.cs** Section 5:

```csharp
switch (order.Priority)
{
    case OrderPriority.Critical:
        return "CRITICAL-LANE";
    case OrderPriority.Express when order.IsHighValue:
        return "EXPRESS-VIP";
    case OrderPriority.Express:
        return "EXPRESS-STANDARD";
    case OrderPriority.Standard when order.Quantity > 100:
        return "BULK-STANDARD";
    case OrderPriority.Standard:
        return "STANDARD";
    default:
        return "UNKNOWN";
}
```

2. Add a test matrix: `(Express, high-value)`, `(Express, low-value)`, `(Standard, qty>100)` — assert distinct lanes.
3. Consider a C# 8+ switch expression later for exhaustiveness; C# 7 switch still requires manual ordering discipline.

**Production takeaway:** C# 7 switch patterns behave like ordered rule lists, not `if/else if` auto-reordering — Karat embeds this as a routing bug that compiles cleanly. See **Program.cs** QUICK REFERENCE — "Pattern case order wrong → Wrong branch taken."

---

#### Q2. (R) A product lookup was optimized with `ValueTask<string>` for cache hits. Under retry logic, intermittent `InvalidOperationException` appears. Review:

**Answer:** A consumed `ValueTask` must not be awaited twice unless it wraps a `Task` or `IValueTaskSource` — the retry path re-awaits the same instance after the cache-hit path already completed it synchronously, causing undefined behavior or `InvalidOperationException`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async / ValueTask | Double `await` on same `ValueTask` instance | Intermittent crash on cache-hit + empty-name retry |
| API contract | Caller stores and reuses `ValueTask` like `Task` | Violates ValueTask consumption rules |
| Design | Retry treats empty string as fetch failure without re-invoking | Masks catalog bugs; triggers invalid reuse |

**Fix (priority order):**

1. **Do not cache the `ValueTask`** — call `GetProductNameAsync` again on retry, or await once into a `string`:

```csharp
order.ProductName = await _lookup.GetProductNameAsync(order.ProductId);

if (string.IsNullOrEmpty(order.ProductName))
    order.ProductName = await _lookup.GetProductNameAsync(order.ProductId);
```

2. Better: return `Task<string>` on public boundaries unless profiling proves allocation pressure; keep `ValueTask` internal to hot paths documented as single-consumption.
3. Fix empty-name handling at source — distinguish "not found" from empty string instead of blind retry.
4. Add analyzer discipline: treat `ValueTask`/`ValueTask<T>` like `IDisposable` — one consumer.

**Production takeaway:** `ValueTask` wins on cache hits (**Program.cs** Section 9) but fails when callers treat it like a reusable `Task` — a common Karat stack of optimization + retry logic.

---

#### Q3. (R) A developer refactors inventory reservation to use C# 7 ref returns for in-place updates. The build fails; after a workaround it crashes in QA. Review:

**Answer:** `ref` locals and `ref` returns cannot appear in `async` methods (state machine restriction), and `ref` returns must target stable storage — a `List<T>` indexer returns a temporary ref in many contexts, not a durable slot alias safe across growth/reallocation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `ref` local in `async Task ReserveAsync` | CS8170 / CS4012 — async methods cannot use ref locals |
| Runtime / API | `ref counts[index]` on `List<int>` | Ref may not track list reallocation; corrupt or invalid updates |
| Design | Sync ref mutation mixed with awaited I/O | Reservation not atomic with commit; race if ever made to compile |

**Fix (priority order):**

1. Remove `ref` from async paths — compute new value after `await`, assign by index:

```csharp
public async Task ReserveAsync(Order order, int[] inventory, int skuIndex)
{
    await Task.Delay(10);
    inventory[skuIndex] -= order.Quantity;
}
```

2. Use **arrays** (as in **Program.cs** `FindInventorySlot`) for in-place ref mutation in synchronous hot paths only.
3. For `List<T>`, mutate via index assignment or lock — do not expose `ref` return on list indexer.
4. Keep `FindSlot` synchronous; if reservation needs I/O, read/modify/write as explicit steps with concurrency control (`lock`, `Interlocked`, or DB transaction).

**Production takeaway:** Ref returns suit fixed buffers (**Program.cs** Section 8); pairing them with `async` or `List<T>` is a stacked compile + lifetime trap Karat uses to test feature boundaries.

---

#### Q4. (R) A CSV import pipeline uses C# 7 out variables. Finance sees rows with quantity `0` marked as successfully imported. Review:

**Answer:** `int.TryParse` failure on quantity is ignored — `out var qty` defaults to `0` and import still returns `true` because only `orderId` failure short-circuits; invalid quantity silently becomes zero.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Quantity parse result not checked | Bad CSV → qty 0 orders marked successful |
| Out variables | `out var` makes parse easy to "fire and forget" | C# 7 ergonomics hide missing validation |
| Data integrity | `return true` after partial parsing | Downstream fulfillment and billing act on garbage |

**Fix (priority order):**

1. Check every `TryParse` return value — C# 7 inline `out` does not remove validation duty:

```csharp
if (!int.TryParse(columns[2], out var qty) || qty <= 0)
    return false;

row.Quantity = qty;
```

2. Validate SKU with explicit failure when required — `TryGetValue` fallback to raw string may be intentional, but document it.
3. Consider `RequirePositiveQuantity` pattern from **Program.cs** Section 11 (throw expression) for internal APIs vs `Try*` for import boundaries.
4. Log row index and raw columns on `false` — ops needs rejected rows, not silent zeros.

**Production takeaway:** Out variables reduce boilerplate (**Program.cs** Section 3) but Karat tests whether you still branch on the `bool` — `out var` without checking is a production data bug.

---

#### Q5. (P) A warehouse fulfillment microservice returns `(bool CanFulfill, string Note)` tuples from `CheckFulfillment` — the same shape as the tutorial's tuple demo. The team debates replacing tuples with a `FulfillmentResult` record before exposing the method on a public NuGet contract. When is the tuple idiomatic, and when does it break production maintainability?

**Answer:** Named tuples are fine for private or internal helpers with stable, obvious element semantics; public NuGet contracts need a named type so additions, serialization, and versioning do not break consumers silently.

- **Keep tuples** for internal methods with two or three tightly coupled values and no evolution expected — e.g. private `CheckFulfillment` inside one service class, same assembly as **Program.cs** Section 6 demo.
- **Use a record/class** when the shape crosses assembly boundaries, gains fields (`ReservedQuantity`, `BackorderSku`), needs JSON/XML mapping, or appears in logs/metrics dashboards — `(bool, string)` element names are not part of the runtime contract.
- **Inferred tuple names (C# 7.1)** help readability at the return site but do not replace API documentation for external callers.
- **Tuple equality (C# 7.3)** is useful for tests comparing snapshots; not a substitute for domain identity on persisted entities.

**Production takeaway:** Tuples are a local convenience feature — Karat asks you to draw the line at package/public API surfaces where contract evolution and tooling (OpenAPI, analyzers) require named types.

---

#### Q6. (M) A batch job uses a local function with captured outer state to retry flaky lane assignments. Ops reports duplicate reservations on the same SKU after parallel batch splits. Review:

**Answer:** The local function captures `reservationFailures` and mutates `inventory` through `ref` aliases while `Parallel.ForEach` runs handlers concurrently — non-atomic read-modify-write on shared array slots and non-interlocked increments corrupt counts under race.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | `Parallel.ForEach` + unsynchronized `slot -= order.Quantity` | Lost updates → overselling inventory |
| Local functions | Closure over `reservationFailures++` | Race on int increment; inaccurate metrics |
| Ref locals | `ref int slot = ref inventory[skuIndex]` in parallel bodies | Each thread aliases same storage without lock |

**Fix (priority order):**

1. Do not parallelize in-place mutation on a shared array without synchronization — use `lock` per SKU, `ConcurrentDictionary`, or partition work by SKU.
2. Replace `reservationFailures++` with `Interlocked.Increment(ref reservationFailures)` if you keep shared counters.
3. Prefer **static local functions (C# 8+)** or private methods that take explicit parameters — makes captured state visible in review; see **Program.cs** Section 7 note on static locals.
4. For warehouse scale-out, reservation belongs in a transactional store (DB row lock), not a shared in-memory array across parallel threads.

**Production takeaway:** Local functions + ref locals are synchronous, single-flow tools from C# 7 — Karat stacks them with `Parallel.ForEach` to test whether you recognize closure and alias concurrency, not just syntax.

---

#### Q7. (D) Two teammates implement guard clauses for order validation. Which approach do you standardize on for a shared domain library, and why?

**Answer:** Standardize on **Option B (classic blocks)** for shared domain validation libraries, and allow **Option A (throw expressions)** only for thin one-liners where exception detail stays minimal — not as the default for public APIs that operators debug from logs.

- **Throw expressions** pair well with expression-bodied members (**Program.cs** Sections 10–11) for null-guard properties and private helpers — `order ?? throw new ArgumentNullException(nameof(order))` is clear and concise.
- **Classic blocks** win when you need overloads with `(paramName, actualValue, message)` on `ArgumentOutOfRangeException`, multiple guards, or XML doc that describes thrown types — Option B's qty check carries the offending value; Option A's ternary does not.
- **Refactor safety:** Expression-bodied throw chains are harder to breakpoint and step through in production debugging than block bodies.
- **Consistency:** Mixed styles across a NuGet domain library frustrate code review — pick block guards for public methods, throw expressions for small internal `Require*` helpers like **Program.cs** `RequireOrder` / `RequirePositiveQuantity`.

**Production takeaway:** C# 7 throw expressions are idiomatic guards, not a wholesale replacement for validation methods — Karat tests judgment on expression-bodied brevity vs operability in a shared library.
