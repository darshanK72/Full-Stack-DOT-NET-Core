# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/10. Exception Handling - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A teammate adds logging around order validation before rethrowing. Review this method — what would you change and why?

```csharp
public void ValidateAndCharge(Order order, decimal walletBalance, ILogger logger)
{
    try
    {
        order.Validate();
        ProcessPayment(walletBalance, order.Total);
    }
    catch (Exception ex)
    {
        logger.LogError(ex.Message);
        throw ex;
    }
}
```

---

#### Q2. (R) A nightly reconciliation job wraps payment-gateway calls like this. Support reports "job succeeded" but ledger rows are missing after gateway timeouts. What is wrong?

```csharp
public int ReconcilePendingOrders(IEnumerable<Order> orders)
{
    int processed = 0;

    foreach (Order order in orders)
    {
        try
        {
            ChargeViaSimulatedGateway(order.Total);
            processed++;
        }
        catch (Exception)
        {
            // gateway blips are common — keep going
        }
    }

    return processed;
}
```

---

#### Q3. (R) Audit entries must always be closed, even when `WriteEntry` throws. A junior developer refactors Section 15 without `using`. Review:

```csharp
public static string WriteAuditTrail(string orderId)
{
    AuditLogWriter audit = new AuditLogWriter();
    audit.WriteEntry($"Payment attempt for {orderId}");
    string trail = audit.LastEntry;
    audit.Dispose();
    return trail;
}
```

What can go wrong in production, and how would you fix it?

---

#### Q4. (D) A team introduces `InvalidOrderException`, `InsufficientFundsException`, and `CustomerNotFoundException` for every validation failure — including null method parameters and missing optional query filters. When is a custom domain exception the right choice vs `ArgumentException`, a result type, or no throw at all?

---

#### Q5. (R) Two implementations look up a product by SKU. One is in code review. Which approach would you approve for a catalog service called millions of times per day, and why?

```csharp
// A
public Product GetProduct(string sku)
{
    if (!_catalog.TryGetValue(sku, out Product? product))
        throw new KeyNotFoundException($"SKU {sku} not found.");
    return product;
}

// B
public bool TryGetProduct(string sku, out Product? product) =>
    _catalog.TryGetValue(sku, out product);
```

---

#### Q6. (P) This console chapter lets `InvalidOrderException` bubble out of `Main` when validation fails. In an ASP.NET Core API, the same unhandled domain exception currently returns a raw 500 HTML page. What centralized pattern replaces scattered try/catch in every controller, and what must differ between Development and Production responses?

---
