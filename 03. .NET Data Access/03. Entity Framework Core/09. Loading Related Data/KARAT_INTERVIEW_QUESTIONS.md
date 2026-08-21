# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/09. Loading Related Data`
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A list endpoint only needs order id, date, and customer name, but the repository loads the full graph before mapping. Review the method — what is the performance problem, and how do you fix it while keeping a single SQL round-trip?

```csharp
public async Task<IReadOnlyList<OrderListItemDto>> GetRecentOrdersAsync(CancellationToken ct)
{
    using ShopDbContext context = CreateContext();
    List<Order> orders = await context.Orders
        .AsNoTracking()
        .Include(o => o.Customer)
        .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
        .OrderByDescending(o => o.OrderDate)
        .Take(50)
        .ToListAsync(ct);

    return orders
        .Select(o => new OrderListItemDto(o.OrderId, o.OrderDate, o.Customer!.Name))
        .ToList();
}
```

---

#### Q2. (R) After eager-loading changes, the order-detail page shows line quantities but every `ProductName` is null. Review the query — what is wrong with the Include chain, and what SQL shape do you expect after the fix?

```csharp
public Order? GetOrderDetail(int orderId)
{
    using ShopDbContext context = CreateContext();
    return context.Orders
        .AsNoTracking()
        .Include(o => o.Customer)
        .Include(o => o.Lines)
        .FirstOrDefault(o => o.OrderId == orderId);
}
```

---

#### Q3. (M) A reporting job loads customers with all their orders and every order line. Under load, SQL row counts explode and memory spikes, but the team insists "it's one query so it must be efficient." Review the query — explain the cartesian product, and when you would use `AsSplitQuery()`.

```csharp
public IList<Customer> GetCustomersWithOrderHistory()
{
    using ShopDbContext context = CreateContext();
    return context.Customers
        .AsNoTracking()
        .Include(c => c.Orders)
            .ThenInclude(o => o.Lines)
                .ThenInclude(l => l.Product)
        .OrderBy(c => c.CustomerId)
        .ToList();
}
```

---

#### Q4. (R) A developer refactors N+1 line loading to "explicit loading" but production still shows hundreds of SQL commands per request. Review the method — identify every issue and prioritize fixes.

```csharp
public IList<OrderHeaderDto> GetOrderHeaders(IReadOnlyList<int> orderIds)
{
    using ShopDbContext context = CreateContext();
    List<Order> orders = context.Orders
        .AsNoTracking()
        .Where(o => orderIds.Contains(o.OrderId))
        .ToList();

    foreach (Order order in orders)
    {
        context.Entry(order).Reference(o => o.Customer).Load();
        context.Entry(order).Collection(o => o.Lines).Load();
    }

    return orders.Select(o => new OrderHeaderDto(
        o.OrderId,
        o.Customer!.Name,
        o.Lines.Count)).ToList();
}
```

---

#### Q5. (R) A fulfillment API should return only orders that have at least one line with `Quantity >= 2`, and each order should expose only those qualifying lines. The developer uses filtered Include but QA reports wrong orders in the response. Review the code — what is misunderstood about filtered Include, and how do you fix the business rule?

```csharp
public IList<Order> GetBulkShipCandidates(int minQuantity)
{
    using ShopDbContext context = CreateContext();
    return context.Orders
        .AsNoTracking()
        .Include(o => o.Lines.Where(l => l.Quantity >= minQuantity))
        .OrderBy(o => o.OrderId)
        .ToList();
}
```

---

#### Q6. (D) An order API has three read paths: (A) list view — id/date/customer name only; (B) detail view — customer + lines + product names; (C) background job — load customer only when an order fails validation. For each path, choose **projection**, **Include/ThenInclude**, or **explicit Load** — and name one production failure mode if you pick the wrong strategy.

---
