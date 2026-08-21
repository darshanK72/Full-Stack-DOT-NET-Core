# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/02. Data Types & Variables - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Finance QA reports order totals off by one cent on some invoices. Review this pricing helper copied from a prototype:

```csharp
public decimal CalculateOrderTotal(double unitPrice, int quantity)
{
    double subtotal = unitPrice * quantity;
    double tax = subtotal * 0.18;
    return (decimal)(subtotal + tax);
}
```

A developer says casting the final result to `decimal` fixes binary rounding. What is wrong, and what would you change?

---

#### Q2. (R) A loyalty API returns `int?` for optional points. After deploy, `NullReferenceException` and `InvalidOperationException` appear in logs. Review:

```csharp
public int ComputeBonus(int? loyaltyPoints, string? tierCode)
{
    var bonus = loyaltyPoints.Value * 2;
    if (tierCode.Equals("Gold", StringComparison.OrdinalIgnoreCase))
        bonus += 50;
    return bonus;
}
```

What breaks in production, and how would you harden this method?

---

#### Q3. (R) A metrics exporter builds a snapshot list for a dashboard. Under load, Gen2 collections spike. Review:

```csharp
public IReadOnlyList<object> BuildDailyCounts(IEnumerable<int> orderCounts)
{
    var snapshot = new List<object>();
    foreach (var count in orderCounts)
        snapshot.Add(count);
    return snapshot;
}
```

What is the performance issue, and what type change fixes it without changing call-site semantics?

---

#### Q4. (P) A warehouse service increments a 32-bit `int transactionId` inside a tight loop processing bulk imports. In staging (small files) IDs look fine; in production one job reports duplicate IDs and negative values after a long run. The team says "C# integers don't overflow in normal use." Explain what happened and what you would use instead.

---

#### Q5. (M) A developer models store configuration like the chapter's `StoreConfig` but tries to share a tax rate across all instances from appsettings loaded at startup:

```csharp
public class PricingOptions
{
    public const decimal StandardTaxRate = LoadTaxRateFromConfiguration();

    private static decimal LoadTaxRateFromConfiguration() =>
        decimal.Parse(Environment.GetEnvironmentVariable("TAX_RATE") ?? "0.18");
}
```

Build fails with CS0133. They propose replacing `const` with `static readonly` assigned from a static constructor. Is that sufficient for production DI, and what pattern would you recommend?

---

#### Q6. (D) Your API team debates `var` vs explicit types in service-layer code. Two snippets assign the same JSON field:

```csharp
var total = json.GetProperty("orderTotal").GetDecimal();
decimal total = json.GetProperty("orderTotal").GetDecimal();

var orderId = json.GetProperty("orderId").GetInt64();
long orderId = json.GetProperty("orderId").GetInt64();
```

When would you require explicit `decimal` and `long` (as in this chapter's money and `orderId` examples), and when is `var` acceptable?

---
