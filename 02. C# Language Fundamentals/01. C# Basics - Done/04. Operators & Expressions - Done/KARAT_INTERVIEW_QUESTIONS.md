# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/04. Operators & Expressions - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

---

#### Q6. (P) Your team ships pricing, inventory, and permission rules that mix arithmetic, `??`, `&&`, and `|=` in single expressions. What review checklist would you use in PRs to catch operator bugs before they reach production?

---
