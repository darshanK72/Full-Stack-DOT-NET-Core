# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/07. Methods - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse API helper is supposed to bump packed quantity in place before case-splitting. QA reports the count never changes. Review the call site and method — what is wrong, and how do you fix it?

```csharp
public static void AdjustQuantity(ref int units, int extra) => units += extra;

// InventoryService.cs
int packedUnits = 36;
AdjustQuantity(packedUnits, 12);   // caller expects packedUnits == 48
bool ok = TrySplitCases(packedUnits, 12, out int cases, out int loose);
```

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

---

#### Q4. (P) Your team ships `OrderFormatting.dll` v1.0 with this public API:

```csharp
public static string FormatMoney(decimal amount, string currency = "USD")
    => $"{currency} {amount:N2}";
```

In v1.1 you change the default to `"USD "` (trailing space) for alignment. Existing microservices reference the new DLL but were **not recompiled**. What do callers observe, and how should you version optional-parameter defaults in shared libraries?

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
