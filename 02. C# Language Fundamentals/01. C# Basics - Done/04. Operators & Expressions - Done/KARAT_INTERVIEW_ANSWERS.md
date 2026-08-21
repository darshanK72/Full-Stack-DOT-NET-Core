# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/04. Operators & Expressions - Done`

---

#### Q1. (R) QA reports that bulk-order discounts are too high on large orders. Review this pricing helper used in checkout:

```csharp
public static decimal ComputeOrderTotal(decimal lineSubtotal, decimal discountRate)
{
    // discountRate is 0.05m for 5% off
    return lineSubtotal - lineSubtotal * discountRate;
}

public static decimal ComputeOrderTotalWithCap(decimal lineSubtotal, decimal discountRate, decimal maxDiscount)
{
    decimal discount = lineSubtotal * discountRate;
    return lineSubtotal - discount > maxDiscount
        ? lineSubtotal - maxDiscount
        : lineSubtotal - lineSubtotal * discountRate;
}
```

What precedence or grouping bugs do you see, and how would you fix them?

**Answer:** `ComputeOrderTotal` is correct because `*` binds tighter than `-`, but `ComputeOrderTotalWithCap` compares `lineSubtotal - discount` before applying the cap logic incorrectly — when the uncapped discount exceeds `maxDiscount`, it returns `lineSubtotal - maxDiscount` as if `maxDiscount` were a final total, not a discount ceiling.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `lineSubtotal - discount > maxDiscount ? lineSubtotal - maxDiscount : …` treats `maxDiscount` as a **total cap**, not a max **discount amount** | Customers charged wrong totals when discount should be capped (over- or under-charging) |
| Maintainability | Duplicate `lineSubtotal * discountRate` in ternary branches | Drift risk — one branch updated, other not |
| Precedence | `-` vs `>` grouping is legal but obscures intent without parentheses | Reviewers misread intended math (related to **PrecedenceExamples.DiscountedSubtotal** in this chapter) |

**Fix (priority order):**

1. Compute discount once: `decimal discount = lineSubtotal * discountRate;`
2. Cap the discount, not the total: `discount = Math.Min(discount, maxDiscount);` then `return lineSubtotal - discount;`
3. Add parentheses when mixing `-`, `*`, and comparisons even if precedence is technically correct — `(lineSubtotal - discount)` makes audits faster.
4. Add unit tests for boundary cases: 0%, 5%, rate that exceeds cap, and zero subtotal.

```csharp
public static decimal ComputeOrderTotalWithCap(decimal lineSubtotal, decimal discountRate, decimal maxDiscount)
{
    decimal discount = Math.Min(lineSubtotal * discountRate, maxDiscount);
    return lineSubtotal - discount;
}
```

**Production takeaway:** Precedence makes `a - b * c` mean `a - (b * c)` — Karat tests whether you spot **business-logic grouping** errors, not just missing semicolons. See **PricingRules.ComputeDiscountedTotal** and **PrecedenceExamples** — multiplication before subtraction is correct; ternary + cap semantics often are not.

---

#### Q2. (R) A warehouse API returns `404` when a SKU code is missing from the request, but the team expected a default length of `1`. Review:

```csharp
public int ResolveMaxPickSlots(string? skuCode, int warehouseCapacity)
{
    int slots = skuCode?.Length ?? 0 + 1;
    return slots > warehouseCapacity ? warehouseCapacity : slots;
}
```

Callers pass `skuCode: null` and expect `1` slot. What is wrong, and what would you change?

**Answer:** `??` binds **looser** than `+`, so the expression parses as `skuCode?.Length ?? (0 + 1)` — when `skuCode` is null, `slots` becomes `1`. When `skuCode` is non-null, `slots` is the string length with **no** `+ 1`. The bug is inconsistent intent: developers often read `?? 0 + 1` as `(?? 0) + 1`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Null path yields `1`; non-null path yields `Length` without increment | Off-by-one pick slots for valid SKUs — wrong capacity errors or over-picking |
| Precedence | `??` lower than additive `+` (see **PrecedenceExamples.NullCoalesceBeforeAddition**) | Silent misread during code review — matches tutorial gotcha exactly |
| API contract | Method name implies "max slots" but magic `+ 1` is undocumented | Callers cannot predict behavior from signature alone |

**Fix (priority order):**

1. Parenthesize intent explicitly: `int slots = (skuCode?.Length ?? 0) + 1;`
2. If default should be `1` when null **and** length when present, document whether `+ 1` applies to both paths.
3. Consider `Math.Max(1, skuCode?.Length ?? 0)` if minimum one slot is a business rule.
4. Add tests: `null → 1`, `"AB" → 3` (or `2` if no increment), and capacity clamp edge cases.

**Production takeaway:** Null-coalescing chains (`preferred ?? backup ?? fallback`) are safe; mixing `??` with `+`, `-`, or comparisons without parentheses is a top Karat trap. See **LabelDefaults.ResolveDisplayLabel** for correct chaining vs **PrecedenceExamples.NullCoalesceBeforeAddition** for the precedence pitfall.

---

#### Q3. (R) After a deploy, inventory audit logs show stock decrements even when orders are rejected for insufficient credit. Review the guard:

```csharp
public bool TryReserveAndShip(Order order, InventoryService inventory, CreditService credit)
{
    if (credit.IsApproved(order.CustomerId) & inventory.TryReserve(order.Sku, order.Quantity))
    {
        order.MarkShipped();
        return true;
    }
    return false;
}
```

`CreditService.IsApproved` and `InventoryService.TryReserve` both write audit rows. What breaks compared to `&&`, and how do you fix it?

**Answer:** Bitwise/logical `&` on `bool` operands does **not** short-circuit — both `IsApproved` and `TryReserve` always run, so inventory is reserved even when credit fails. Production code must use `&&` unless both sides must execute.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `&` evaluates both operands always | Stock reserved on failed credit checks — oversell, reconciliation nightmares |
| Side effects | `TryReserve` mutates inventory and audit log unconditionally | Violates "fail fast" — credit gate is cosmetic |
| Operability | Audit trail shows reserve attempts for rejected orders | On-call cannot trust logs for fraud or inventory forensics |

**Fix (priority order):**

1. Replace `&` with `&&` so `TryReserve` runs only when credit is approved.
2. Order cheap, read-only checks first: `if (!credit.IsApproved(...)) return false;` then reserve — or keep single `&&` with approval on the left.
3. Split **validate → mutate → commit** phases; do not combine side-effecting calls inside a boolean expression.
4. Add integration test: credit denied → inventory unchanged, zero reserve audit entries.

**Production takeaway:** `&&`/`||` short-circuit; `&`/`|` on bool do not — **ShippingRules.NonShortCircuitDemo** in this chapter demonstrates the counter difference. Karat embeds the typo (`&` vs `&&`) in realistic service code; always ask whether the right-hand side has side effects.

---

#### Q4. (R) A nightly batch job silently wraps negative stock counts after a bad import. Review:

```csharp
public int ComputeRemainingUnits(int shippedCases, int unitsPerCase, int startingUnits)
{
    int totalShipped = shippedCases * unitsPerCase;
    return startingUnits - totalShipped;
}

// Called from batch:
int remaining = ComputeRemainingUnits(
    shippedCases: 500_000,
    unitsPerCase: 10_000,
    startingUnits: 1_000_000);
```

On a 32-bit host the value becomes positive when it should be deeply negative. What operator/context issue is this, and how would you harden it?

**Answer:** Integer multiplication and subtraction run in the default **unchecked** context — when `shippedCases * unitsPerCase` exceeds `int.MaxValue`, the product wraps; subtracting a wrapped value from `startingUnits` yields a plausible-looking positive `remaining` instead of signaling overflow.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Unchecked `int` overflow on `*` | Wrong remaining stock — silent data corruption in batch reports |
| Context | No `checked` block or `checked` project setting | Overflow throws nowhere; negative economics look like surplus |
| Domain | `int` may be too narrow for warehouse-scale counts | Latent bug appears only under production volumes |

**Fix (priority order):**

1. Use `checked` for the multiply (or enable `<CheckForOverflowUnderflow>true</CheckForOverflowUnderflow>` in debug builds): `int totalShipped = checked(shippedCases * unitsPerCase);`
2. Promote to `long` or `decimal` for intermediate math: `(long)shippedCases * unitsPerCase` before compare/subtract.
3. Validate inputs up front — reject negative or absurd magnitudes before arithmetic.
4. Fail the batch on overflow (`OverflowException`) rather than persisting wrapped values; alert ops.

```csharp
public int ComputeRemainingUnits(int shippedCases, int unitsPerCase, int startingUnits)
{
    long totalShipped = (long)shippedCases * unitsPerCase;
    long remaining = startingUnits - totalShipped;
    if (remaining > int.MaxValue || remaining < int.MinValue)
        throw new OverflowException("Remaining units out of int range.");
    return (int)remaining;
}
```

**Production takeaway:** C# arithmetic overflows wrap by default — unlike SQL or decimal math. See foundation **Operators** gotcha on integer division; Karat extends to **checked** context for inventory and financial quantities. Prefer `decimal` for money (**OrderArithmetic.ComputeLineSubtotal**); use `checked` or wider types for counts that can exceed 2³¹.

---

#### Q5. (R) A code review flags this permission-update endpoint copied from an internal admin tool. Identify the compound-assignment and operator issues:

```csharp
public int UpdatePickerRole(int currentRole, bool grantAdmin, bool revokeWrite)
{
    if (grantAdmin)
        currentRole |= PermissionFlags.Admin;

    if (revokeWrite)
        currentRole ^= PermissionFlags.Write;  // "remove write" per ticket

    currentRole /= 2;  // "normalize" role mask after shift-left in legacy import

    return currentRole;
}
```

What would you change before merging?

**Answer:** Three operator mistakes stack: `^=` **toggles** Write (adds it if absent), it does not reliably remove; `|=` without validating existing flags can grant Admin on corrupted masks; and `/=` performs **integer division** on a bit mask, destroying unrelated permission bits instead of "normalizing" a shift.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `role ^= Write` toggles Write bit — wrong for "revoke write" | Users lose or gain Write unpredictably depending on current state |
| Correctness | `currentRole /= 2` is integer division, not "undo << 1" on flags | Arbitrary permission bits cleared — privilege escalation or lockout |
| Design | Compound bitwise ops (`|=`, `&= ~Flag`) require domain knowledge | Copy-paste from **PermissionFlags** tutorial without **RemoveWriteViaAndNot** pattern |
| Maintainability | Magic `/= 2` comment references legacy import | Future devs cannot reason about resulting mask |

**Fix (priority order):**

1. Revoke with AND-NOT: `currentRole &= ~PermissionFlags.Write;` (see **PermissionFlags.RemoveWriteViaAndNot**).
2. Grant with OR: keep `|=` for Admin only after validating `currentRole` is a known baseline mask.
3. Replace `/= 2` with explicit unshift if legacy data was shifted: `currentRole >>= 1` **only** if product owner confirms all roles were uniformly shifted — otherwise migrate data offline, do not "fix" in request path.
4. Return new mask from pure function; log before/after for audit; unit-test grant/revoke combinations.

```csharp
if (revokeWrite)
    currentRole &= ~PermissionFlags.Write;
// Remove currentRole /= 2 unless data migration explicitly requires >>= 1
```

**Production takeaway:** Compound assignment (`|=`, `&=`, `^=`, `/=`) is concise but encodes irreversible in-place mutations — Karat tests whether you know **XOR toggle vs AND-NOT clear** and that `/=` on flags is almost never what you want. See **PermissionFlags.ApplyCompoundAssignments** for intentional demo code vs production-safe **RemoveWriteViaAndNot**.

---

#### Q6. (P) Your team ships pricing, inventory, and permission rules that mix arithmetic, `??`, `&&`, and `|=` in single expressions. What review checklist would you use in PRs to catch operator bugs before they reach production?

**Answer:** Treat every multi-operator expression as a liability — require explicit parentheses, short-circuit logical operators for guards with side effects, and separate mutation from conditionals unless the idiom is a well-known flag pattern.

- **Precedence:** Flag any expression mixing `??` with `+`, `-`, `<`, or `?:` without parentheses; require `(a ?? b) + c` style or split into named locals (see **PrecedenceExamples**).
- **Short-circuit:** Side effects (DB calls, reserve/decrement, logging) must use `&&`/`||`, never `&`/`|` on bool; put failing checks on the left.
- **Arithmetic domain:** Money paths use `decimal` literals (`49.99m`); count paths use `checked` or `long` when products can overflow `int`; never rely on integer `/` for fractional business rules (**OrderArithmetic.CompareDivision**).
- **Compound assignment:** Bitwise `^=` is toggle, not remove; `/=` and `%=` on counts need explicit comment or rejection; prefer `role = (role & ~Flag)` for revocations.
- **Readability rule:** If an expression needs a comment to explain evaluation order, extract to two or three statements with descriptive names — reviewers and Karat both reward clarity over one-liners.

**Production takeaway:** Layer 1 teaches operator tables; Layer 2 expects a **PR nose** for precedence, short-circuit, overflow, and compound-assignment footguns — the same families demonstrated in this folder's **Program.cs** warehouse scenario.

---
