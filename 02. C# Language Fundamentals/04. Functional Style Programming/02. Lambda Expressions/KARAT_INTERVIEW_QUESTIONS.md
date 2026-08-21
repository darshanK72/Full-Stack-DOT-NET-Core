# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/02. Lambda Expressions`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A pricing microservice builds per-SKU discount rules at startup and applies them later during checkout. QA reports every SKU gets the same discount as the last item in the catalog. Review this registration code:

```csharp
public sealed class DiscountRuleRegistry
{
    private readonly List<Func<decimal, decimal>> _rules = new();

    public void RegisterRules(IEnumerable<(string Sku, decimal Rate)> catalog)
    {
        foreach (var item in catalog)
        {
            _rules.Add(price => price * (1m - item.Rate));
        }
    }

    public decimal ApplyAll(decimal price) =>
        _rules.Aggregate(price, (current, rule) => rule(current));
}
```

What is wrong with the lambdas, and how do you fix it without changing the public API shape?

---

#### Q2. (R) A teammate refactors price validation from a statement lambda to an "expression" lambda for readability. The project fails to compile. Review the change:

```csharp
PriceFilter isValidUnitPrice = price =>
{
    if (price <= 0m) return false;
    if (price > 999_999m) return false;
    return true;
};

// Refactor attempt:
PriceFilter isValidUnitPrice = price => { price > 0m && price <= 999_999m };
```

What compile errors or design mistakes appear, and when should you keep a statement (block) lambda instead of forcing an expression form?

---

#### Q3. (R) An order API caches a `PriceTransform` delegate per tenant so repeated requests skip rebuilding markup logic. Review the scoped service:

```csharp
public sealed class TenantPricingService
{
    private Func<decimal, decimal>? _cachedTransform;

    public decimal GetMarkedUpPrice(decimal basePrice, decimal tenantMarkupPercent)
    {
        _cachedTransform ??= price => price * (1m + tenantMarkupPercent);
        return _cachedTransform(basePrice);
    }
}
```

The service is registered **Scoped**, but finance reports wrong markups when tenants change rates at runtime. What closure/capture bug is embedded here, and what is the correct fix?

---

#### Q4. (P) An EF Core repository exposes two overloads for filtering products. In production, one path translates to SQL; the other loads the entire table into memory. Review:

```csharp
public async Task<List<Product>> GetExpensiveAsync(
    decimal minPrice,
    CancellationToken ct)
{
    Expression<Func<Product, bool>> predicate =
        p => p.UnitPrice >= minPrice;

    return await _db.Products
        .Where(predicate)
        .ToListAsync(ct);
}

public async Task<List<Product>> GetExpensiveInMemoryAsync(
    decimal minPrice,
    CancellationToken ct)
{
    Func<Product, bool> predicate =
        p => p.UnitPrice >= minPrice;

    return await _db.Products
        .Where(predicate)
        .ToListAsync(ct);
}
```

Explain why `Expression<Func<T, bool>>` vs `Func<T, bool>` matters for EF Core, and what breaks if you standardize on `Func` everywhere "because lambdas look the same."

---

#### Q5. (R) A background price-sync job fires work with `Task.Run` and an async lambda. Failures never reach Application Insights. Review:

```csharp
public void ScheduleCatalogRefresh(IEnumerable<string> skus)
{
    foreach (var sku in skus)
    {
        Task.Run(async () =>
        {
            var price = await _gateway.FetchPriceAsync(sku);
            _cache.Set(sku, price);
        });
    }
}
```

What async/lambda issues stack here (including the classic loop capture), and how do you fix observability and correctness?

---

#### Q6. (M) A developer chains LINQ over an in-memory price list and assumes the filter runs once at definition time. Review:

```csharp
decimal minPromoPrice = 25m;
var promoSkus = catalog
    .Where(p => p >= minPromoPrice)
    .Select(p => p * 0.90m);

Console.WriteLine($"Eligible count: {promoSkus.Count()}");

minPromoPrice = 50m;

foreach (var price in promoSkus)
{
    Console.WriteLine(price);
}
```

What does deferred execution plus closure capture imply for the printed count vs the foreach output? How would you make the pipeline deterministic for a report snapshot?

---

#### Q7. (D) A hot-path checkout endpoint transforms thousands of line items per second. The team debates three filter styles:

```csharp
// A — expression lambda inline
var result = PricePipeline.ApplyToAll(prices, p => p * 1.08m);

// B — method group
var result = PricePipeline.ApplyToAll(prices, ApplyTax);

// C — static lambda (C# 9+)
var result = PricePipeline.ApplyToAll(prices, static p => p * 1.08m);
```

When would you choose A vs B vs C for production throughput and maintainability, and what allocation/closure trade-offs should you mention in a design review?
