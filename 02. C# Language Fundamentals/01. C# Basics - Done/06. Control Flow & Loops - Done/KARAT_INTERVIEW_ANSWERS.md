# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/06. Control Flow & Loops - Done`

---

#### Q1. (R) A developer ports a C-style fulfillment router into C#. The build fails with CS0163. Review the switch — what is wrong, and how would you fix it while preserving the shared "in transit" behavior?

**Answer:** C# does not allow fall-through between cases that contain executable statements — the `"Packed"` case assigns to `message` but never exits with `break`/`return`, triggering CS0163. Shared labels work only when multiple `case` labels precede a single block with one exit point.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `"Packed"` case has statements but no `break`/`return`/`throw` | CS0163 — build blocked |
| Logic | Undeclared `message` variable (if compile were forced) | Would not compile (CS0103) |
| Port mistake | Assumed C/C++ fall-through semantics | Classic switch migration trap |

**Fix (priority order):**

1. Use shared labels the C# way — multiple cases, one block, one exit:

```csharp
case "Packed":
case "Shipped":
case "Delivered":
    return "In transit pipeline";
```

2. Or give `"Packed"` its own `return` if the message must differ: `return "Ready for carrier";`
3. Prefer `return` per case (as in this chapter's `GetStatusMessage`) — eliminates missing-`break` bugs entirely.
4. For value-returning routing, consider a switch expression (`status switch { "Pending" => ..., _ => ... }`) — no fall-through surface area.

**Production takeaway:** Fulfillment status routing is a common Karat snippet — know that C# only allows fall-through via **empty** stacked labels, not between bodies. See **Program.cs** Section 4 — classic switch `break` rules.

---

#### Q2. (R) A nightly batch job counts warehouse slots for billing. QA reports the invoice is one slot short for every aisle. Review the nested loop:

```csharp
public static int CountBillableSlots(int aisles, int shelvesPerAisle)
{
    int count = 0;
    for (int aisle = 1; aisle < aisles; aisle++)
    {
        for (int shelf = 1; shelf <= shelvesPerAisle; shelf++)
        {
            count++;
        }
    }
    return count;
}
// Called with CountBillableSlots(5, 10) — ops expects 50 slots.
```

What is wrong, and what would you change?

**Answer:** The outer loop uses `aisle < aisles` with a 1-based start, so it runs for aisles 1–4 instead of 1–5 — an off-by-one error that drops one entire aisle (10 slots) from the count.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Outer bound `aisle < aisles` with 1-based indexing | Misses last aisle — billing under-count |
| Consistency | Inner loop uses `<= shelvesPerAisle`, outer uses `< aisles` | Mixed inclusive/exclusive bounds — easy to miss in review |
| Testing | `(5, 10)` returns 40, not 50 | Silent revenue loss until reconciliation |

**Fix (priority order):**

1. Match the chapter's inclusive pattern: `for (int aisle = 1; aisle <= aisles; aisle++)` — mirrors `CountWarehouseSlots` in **Program.cs** Section 12.
2. Alternatively use 0-based indexing consistently: `for (int aisle = 0; aisle < aisles; aisle++)` — document which convention the API uses.
3. Add a unit test asserting `CountBillableSlots(5, 10) == 50` and edge cases (`0` aisles, `1` shelf).
4. Name parameters or add XML docs clarifying whether counts are 1-based inclusive ranges.

**Production takeaway:** Off-by-one is the most common loop bug — Karat pairs `<` vs `<=` with a business consequence (billing). Always trace first/last iteration against expected cardinality.

---

#### Q3. (R) A gate-controller service hangs in staging after a config change. Review the retry loop:

```csharp
public static bool WaitForGateOpen(int maxAttempts)
{
    int attempt = 0;
    while (attempt < maxAttempts)
    {
        if (TryOpenGate())
        {
            return true;
        }
        // forgot to increment attempt
    }
    return false;
}
```

Another team member "fixes" it with `while (true)` and a `break` inside `TryOpenGate()` that only runs on success — but `maxAttempts` is never checked. What breaks in each version, and what is the safe bounded-retry pattern?

**Answer:** Version one never increments `attempt`, so `attempt < maxAttempts` stays true forever when `TryOpenGate()` fails — an infinite loop that hangs the worker. Version two removes the upper bound entirely, so repeated failures spin forever unless an external timeout kills the process.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Missing `attempt++` after failed try | Infinite loop — thread/process hung, health checks fail |
| Runtime | `while (true)` without attempt cap | Unbounded retry — CPU burn, no graceful degradation |
| Operability | No logging between attempts | On-call cannot tell stuck vs slow vs failing gate |

**Fix (priority order):**

1. Use the bounded retry pattern from **Program.cs** Section 16:

```csharp
int attempt = 0;
while (attempt < maxAttempts)
{
    attempt++;
    if (TryOpenGate())
    {
        return true;
    }
}
return false;
```

2. Increment **before or after** the try, but always on every iteration — never only on success.
3. If using `while (true)`, enforce `if (++attempt >= maxAttempts) return false;` inside the body — still prefer `while (attempt < maxAttempts)` for readability.
4. Add delay/backoff between attempts for I/O gates; log attempt count and failure reason.

**Production takeaway:** `while (true)` is valid only when a guaranteed exit path exists (`break`, `return`, `throw`). Production services need bounded retries plus observability — see **TryOpenGate** in this chapter.

---

#### Q4. (R) A pick-list optimizer searches a 2D bin grid for the first high-priority SKU. It finds the SKU but keeps scanning every remaining aisle. Review:

```csharp
public static (int row, int col)? FindPrioritySku(string[,] grid, string target)
{
    for (int row = 0; row < grid.GetLength(0); row++)
    {
        for (int col = 0; col < grid.GetLength(1); col++)
        {
            if (grid[row, col] == target)
            {
                continue; // found — move to next cell
            }
        }
    }
    return null;
}
```

What is wrong with `continue` here, and how would you fix it for early exit?

**Answer:** `continue` skips to the **next inner-loop iteration** — it does not exit either loop or return coordinates. Even when the SKU is found, scanning continues; the method always returns `null`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `continue` instead of `return`/`break` on match | Never returns found position — pick optimizer always fails |
| Performance | Full grid scan on every call | O(rows × cols) even after early match — wasted warehouse API time |
| API design | Nullable return never populated | Callers cannot route pickers to the bin |

**Fix (priority order):**

1. Return immediately on match: `return (row, col);` — clearest fix (matches `TryFindSku` using early `return true` in Section 14).
2. If only the inner loop should stop, use `break` plus a found flag — but `return` is simpler for a search helper.
3. To exit **both** nested loops without `return`, use a labeled `break` or extract search into a method that returns on first hit.
4. Add a test: 3×3 grid with target at `[0,0]` — assert scan stops and coordinates match.

**Production takeaway:** `break` exits the innermost loop/switch; `continue` advances to the next iteration of the **innermost** loop only. Karat uses nested loops to test whether you confuse the two with `return`.

---

#### Q5. (R) A pricing API uses pattern matching on order payloads. Support tickets report zero-quantity lines labeled as "positive." Review:

```csharp
public static string ClassifyLine(object line)
{
    switch (line)
    {
        case int qty:
            return qty > 0 ? $"Active: {qty} units" : $"Zero qty: {qty}";
        case int units when units > 0:
            return $"Positive quantity: {units}";
        case string sku when sku.Length > 0:
            return $"SKU: {sku}";
        default:
            return "Unsupported";
    }
}
// ClassifyLine(0) returns "Active: 0 units" — product owner expected "Zero qty: 0".
```

What is wrong, and how do case order and `when` guards interact?

**Answer:** Switch cases are evaluated **top to first match** — the unguarded `case int qty:` matches every `int`, including zero, before the `when units > 0` arm is ever considered. The guarded case is unreachable dead code for the intended positive-only path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Broad `case int qty:` listed before guarded `case int when units > 0` | Zero/negative ints never reach the guarded arm |
| Maintainability | Duplicate `int` handling with conflicting messages | Product confusion — "Active" label on zero qty |
| Dead code | Second `int` case appears to handle positives | Misleading during review — looks correct but never runs for matched type |

**Fix (priority order):**

1. Order from **most specific to least** — guarded cases first:

```csharp
case int units when units > 0:
    return $"Positive quantity: {units}";
case int units:
    return $"Non-positive quantity: {units}";
```

2. Mirror **Program.cs** `DescribePayload` — `when units > 0` before bare `case int units`.
3. Remove redundant ternary in the first arm if cases are split cleanly.
4. Add tests for `0`, `-1`, positive int, empty string, and non-int payload.

**Production takeaway:** Pattern matching is first-match-wins, not best-match-wins — `when` guards do not override an earlier matching type pattern. See foundation **Control Flow** pattern switch and Section 6 in this chapter.

---

#### Q6. (R) A barcode scan worker sums active line quantities but under-reports totals. Review:

```csharp
public static int SumActiveLines(int[] quantities)
{
    int total = 0;
    foreach (int qty in quantities)
    {
        if (qty <= 0)
            break; // skip bad lines
        total += qty;
    }
    return total;
}
// Input: { 2, 0, 5, 3 } — expected 10, actual 2.
```

What is wrong, and what is the difference between `break` and `continue` in this loop?

**Answer:** `break` **exits the entire loop** on the first non-positive quantity — processing stops at `0` and never reaches `5` or `3`. The developer meant to skip bad lines and keep summing, which requires `continue`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `break` on `qty <= 0` instead of `continue` | Stops at first zero/negative — total 2 instead of 10 |
| Semantics | Comment says "skip bad lines" but code aborts | Comment/code mismatch — classic review trap |
| Data integrity | Inventory totals wrong when discontinued lines appear mid-batch | Shipping and stock reconciliation errors |

**Fix (priority order):**

1. Replace `break` with `continue` — matches `SumActiveQuantities` in **Program.cs** Section 15.
2. Optionally track `skippedLines` for audit logging when qty <= 0.
3. Clarify comment: `continue` skips **this iteration**; `break` ends the **whole** foreach.
4. Unit test mixed arrays: `{ 2, 0, 5, -1, 3 }` → total `10`, skipped `2`.

**Production takeaway:** `break` = stop looping entirely; `continue` = skip to next element. Karat embeds the bug in a foreach that looks like the chapter's correct `continue` example — read the jump statement, not the comment alone.

---
