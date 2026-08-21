# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/07. Methods - Done`

---

#### Q1. (R) A warehouse API helper is supposed to bump packed quantity in place before case-splitting. QA reports the count never changes. Review the call site and method — what is wrong, and how do you fix it?

**Answer:** `AdjustQuantity` is declared with `ref`, but the call site omits `ref` — that is a compile error (CS1615). If the team "fixed" it by removing `ref` from the signature instead, the method receives a copy of `packedUnits` and the caller stays at 36, matching the by-value trap in **Program.cs** Section 6.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile / API | Missing `ref` at call site when parameter is `ref int` | CS1615 — build blocked until corrected |
| Correctness | If `ref` was dropped from the signature to silence the error | Silent bug — quantity never updates; downstream `TrySplitCases` uses stale value |
| Design | Using `ref` for a single updated integer | Works, but returning `(int newUnits, …)` or assigning a return value is clearer for most callers |

**Fix (priority order):**

1. Add `ref` at the call site: `AdjustQuantity(ref packedUnits, 12);` — both declaration and call must use `ref`.
2. If the team avoids `ref` for readability, change the method to return the new value: `packedUnits = BumpQuantity(packedUnits, 12);`.
3. Add a unit test asserting `packedUnits` changes before `TrySplitCases` runs — catches by-value regressions in CI.

**Production takeaway:** `ref`/`out`/`in` are part of the method signature — callers must match exactly. Karat pairs this with the tutorial's `TryBumpByValue` vs `AdjustQuantity(ref …)` demo. See foundation **Methods** — ref requires initialization and keyword at both sites.

---

#### Q2. (R) A pricing service wraps a Try-pattern helper. Under some inputs the process throws instead of returning `false`. Review the method:

```csharp
public static bool TryApplyVolumeDiscount(
    decimal amount,
    int tier,
    out decimal discounted,
    out string reason)
{
    if (amount <= 0m)
    {
        reason = "Amount must be positive.";
        return false;   // early exit
    }

    discounted = amount * (1m - tier * 0.05m);
    return discounted >= amount * 0.5m;
}
```

What breaks at runtime, and what would you change?

**Answer:** On the failure path the method assigns `reason` but never assigns `out decimal discounted` before returning — the C# compiler enforces definite assignment for `out` parameters (CS0177 at compile time in strict builds; if `discounted` were partially fixed, any missed path still violates the contract). Callers using `discounted` after `false` would read an unassigned variable.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile / contract | `discounted` not assigned on the `amount <= 0` return path | CS0177 — build failure, or undefined output if analyzer rules differ |
| API semantics | Try-pattern implies all `out` values are defined on both success and failure | Callers may use `discounted` in logging even when `false` — wrong values or analyzer warnings |
| Maintainability | Future branch added without assigning every `out` | Easy regression — each new early return must assign all outs |

**Fix (priority order):**

1. Assign every `out` parameter on **every** return path: `discounted = 0m;` (or `amount`) before `return false` on the validation branch.
2. Consider reducing `out` count — return `(bool ok, decimal discounted, string? reason)` or a small result record for clarity.
3. Add tests for failure paths asserting `discounted` and `reason` have expected sentinel values.

```csharp
if (amount <= 0m)
{
    discounted = 0m;
    reason = "Amount must be positive.";
    return false;
}
```

**Production takeaway:** `out` means the callee **must** assign before any return — unlike `ref`, the caller does not initialize. Matches **Program.cs** Section 7 and the `TrySplitCases` pattern. See foundation **Methods** — CS0177 gotcha.

---

#### Q3. (R) A developer adds a flexible shipping-fee helper and the project fails to compile. Review the signatures and one call site:

```csharp
public static decimal AddFees(decimal subtotal, params decimal[] surcharges) =>
    subtotal + surcharges.Sum();

public static decimal AddFees(decimal subtotal, params decimal[] surcharges, decimal taxRate) =>
    (subtotal + surcharges.Sum()) * (1m + taxRate);

var total = AddFees(100m, 5m, 2.50m, taxRate: 0.08m);
```

What is wrong, and how would you redesign this API?

**Answer:** `params` must be the **last** parameter in the parameter list — placing `decimal taxRate` after `params decimal[] surcharges` is illegal (CS0231). Even if reordered, two `params` overloads would be invalid (only one `params` per method).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Parameter after `params` array | CS0231 — build blocked |
| API design | Two overloads both using `params` for the same position | CS0229 if both existed — only one params parameter allowed per method |
| Overload resolution | `AddFees(100m, 5m, 2.50m, taxRate: 0.08m)` — compiler must decide whether `0.08m` is another surcharge or tax | Ambiguous or surprising binding if signatures were "fixed" without care |

**Fix (priority order):**

1. Move non-params parameters **before** the `params` array: `AddFees(decimal subtotal, decimal taxRate, params decimal[] surcharges)`.
2. Prefer explicit overloads over `params` for production fee APIs — e.g., `AddFees(subtotal, IEnumerable<decimal> surcharges, decimal taxRate = 0m)` — clearer for JSON/API callers and unit tests.
3. Drop duplicate `params` overloads; use optional `taxRate` on a single method or named arguments: `AddFees(subtotal, taxRate: 0.08m, surcharges: new[] { 5m, 2.50m })`.

**Production takeaway:** `params` is syntactic sugar for callers, not a general "variadic tail" — it must be last and appears once. Fixed-arity overloads (like `CountTokens(string, string)` vs `params` in **Program.cs** Section 5) win overload resolution when they match exactly.

---

#### Q4. (P) Your team ships `OrderFormatting.dll` v1.0 with this public API:

```csharp
public static string FormatMoney(decimal amount, string currency = "USD")
    => $"{currency} {amount:N2}";
```

In v1.1 you change the default to `"USD "` (trailing space) for alignment. Existing microservices reference the new DLL but were **not recompiled**. What do callers observe, and how should you version optional-parameter defaults in shared libraries?

**Answer:** Optional parameter defaults are embedded at the **call site at compile time**, not resolved at runtime from the callee's metadata — services compiled against v1.0 keep passing `"USD"` implicitly even when v1.1 DLL is deployed, so behavior diverges from source-only callers who recompiled.

- **Observed split:** Recompiled callers get `"USD 123.45"` (with trailing space in default); non-recompiled callers still emit `"USD123.45"` — formatting inconsistencies across services, failed snapshot tests, and confused support tickets.
- **Why:** The compiler emits the default literal into each calling assembly's IL; swapping the DLL alone does not rewrite those call sites. See **Program.cs** Section 10 — "baked into the call site at compile time."
- **Safe patterns:** Treat default changes as **breaking** — bump major version, require rebuild, or avoid optional params on public library surfaces; use overloads (`FormatMoney(amount)` → calls `FormatMoney(amount, "USD")`) so defaults live in one method body inside the library.
- **Alternative:** Explicit arguments at all call sites in production code (`FormatMoney(x, "USD")`) — no implicit default dependency.
- **Documentation:** Release notes must say "recompile required" when any optional default changes; CI should rebuild all consumers on shared library updates.

**Production takeaway:** Optional parameters are convenient in tutorials but fragile for shared binaries — prefer overloads or mandatory parameters for stable public APIs. Karat tests whether you know DLL swap ≠ behavior change for optional defaults.

---

#### Q5. (R) A catalog service computes pallet arrangements recursively. In production, large orders crash the worker. Review:

```csharp
public static long CountArrangements(int levels)
{
    if (levels == 0)
        return 1;

    return levels * CountArrangements(levels - 1);   // factorial-style
}

// Called from batch job with user-supplied depth:
long ways = CountArrangements(requestedDepth);   // requestedDepth can be 50_000+
```

What fails, why does it surface only under load, and what fix do you prioritize?

**Answer:** Deep recursion allocates one stack frame per call; `requestedDepth` in the tens of thousands exhausts the thread stack and throws `StackOverflowException` — an unhandled crash that kills the worker process. Small test values (e.g., 5 like **Program.cs** `Factorial` demo) pass QA.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Unbounded recursive depth on user input | `StackOverflowException` — process termination, no graceful error body |
| Correctness | Missing validation for negative `levels` | Infinite recursion toward negative depths until stack overflow |
| Operability | Crash only with large production inputs | Passes unit tests; fails under real catalog batch sizes |

**Fix (priority order):**

1. Replace recursion with an iterative loop for factorial-style math — O(1) stack, O(n) time — same result, no stack growth.
2. Validate input: reject `levels < 0` (`ArgumentOutOfRangeException`) and cap `levels` to a business maximum before computing — return 400/problem details at API boundary.
3. If recursion is required (tree structures), document max depth and use tail-recursion-friendly designs where applicable; monitor stack size in load tests.
4. Return typed errors to callers instead of allowing process crash — wrap computation in bounded worker with timeout for untrusted input.

```csharp
public static long CountArrangements(int levels)
{
    if (levels < 0) throw new ArgumentOutOfRangeException(nameof(levels));
    long result = 1;
    for (int i = 2; i <= levels; i++) result *= i;
    return result;
}
```

**Production takeaway:** Recursion needs a base case **and** a bounded depth — **Program.cs** Section 12 warns that deep chains cause `StackOverflowException`. Prefer loops for linear factorial-style work in services.

---

#### Q6. (R) A base reporting type and a derived export type disagree at runtime. The derived XML docs say it "overrides" discount logic, but callers through a base reference see the old behavior. Review:

```csharp
public class OrderReport
{
    public virtual decimal ApplyDiscount(decimal amount) => amount * 0.95m;
}

public class WholesaleReport : OrderReport
{
    /// <summary>Overrides ApplyDiscount to use wholesale rate.</summary>
    public decimal ApplyDiscount(decimal amount) => amount * 0.80m;   // note: no override keyword
}

OrderReport report = new WholesaleReport();
decimal result = report.ApplyDiscount(100m);   // team expects 80.00m
```

What is wrong, how does this differ from a true override, and what would you change?

**Answer:** The derived method **hides** the base member (`new` is implied when signatures match without `override`) instead of overriding it — dispatch through `OrderReport report` binds to `OrderReport.ApplyDiscount`, so `result` is **95.00m**, not 80.00m. XML comments describe intent the runtime does not honor.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Missing `override` on `virtual` method — method hiding instead | Wholesale discount never applied when referenced as base type — revenue loss or wrong reports |
| Documentation | XML says "Overrides" but code uses hiding semantics | Misleading for IDE tooltips, Swagger generators, and code reviewers |
| Polymorphism | Factory returns `OrderReport` references | Entire inheritance chain silently uses retail rate — hard-to-spot production bug |

**Fix (priority order):**

1. Add `public override decimal ApplyDiscount(decimal amount) => amount * 0.80m;` — enables virtual dispatch through base references.
2. If hiding was intentional (rare), use explicit `public new decimal ApplyDiscount(...)` and fix XML to say "Hides" — never store as base type when hidden behavior is required.
3. Add polymorphic test: `OrderReport r = new WholesaleReport(); Assert.Equal(80m, r.ApplyDiscount(100m));` — fails with hiding, passes with override.

**Production takeaway:** Method **overloading** (same chapter, Section 4) is compile-time name + signature resolution; **override vs hide** is inheritance — different chapter but Karat stacks them because teams confuse "same method name" with polymorphic replacement. Deep coverage → OOP module; here the trap is `virtual`/`override` vs accidental hiding.

---

#### Q7. (R) Static analysis flags a contract mismatch between XML documentation and implementation. Review:

```csharp
/// <summary>
/// Splits <paramref name="units"/> into full cases and remainder.
/// Returns false when units is negative; <paramref name="cases"/> and
/// <paramref name="remainder"/> are zero on failure.
/// </summary>
public static bool TrySplitCases(int units, int unitsPerCase, out int cases, out int remainder)
{
    cases = units / unitsPerCase;
    remainder = units % unitsPerCase;
    return remainder == 0;
}
```

What behavior does the docs promise that the code does not deliver, and what breaks for callers that trust the XML contract?

**Answer:** The XML claims the method returns `false` when `units` is negative and zeros the `out` values — the implementation never checks for negative `units` and defines success only as "even split" (`remainder == 0`). Callers handling `false` as a validation failure will mis-classify valid uneven splits, and negative inputs produce wrong `cases`/`remainder` without signaling failure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Contract | Docs: `false` on negative input with zeroed outs; code: `false` only when remainder ≠ 0 | Callers' error handling for invalid input never runs — bad data propagates |
| Correctness | Negative `units` with positive `unitsPerCase` yields negative `cases` in C# integer division | Downstream inventory counts wrong; no exception |
| Maintainability | IDE / DocFX / Swagger consumers surface the XML as truth | Onboarding devs implement against documentation, not behavior — persistent integration bugs |

**Fix (priority order):**

1. Align code with docs **or** fix docs to match code — pick one source of truth; prefer code + tests as authority, then update XML.
2. If Try-pattern for validation: `if (units < 0 \|\| unitsPerCase <= 0) { cases = 0; remainder = 0; return false; }` then compute split.
3. If success means "even split" only (as in **Program.cs** `TrySplitCases` demo), rewrite XML: "Returns true when units divides evenly into cases; otherwise false with computed cases and remainder."
4. Enable XML doc warnings (CS1591 / custom analyzers) in CI and add tests for negative input and uneven splits.

**Production takeaway:** XML documentation is a public contract when you ship libraries — drift is as harmful as a breaking signature change. The tutorial's `TrySplitCases` uses the bool for "even split," not negativity — Karat tests reading docs **and** the method body together.
