# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/08. Projection Operations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) An order-summary API is slow under load. SQL Profiler shows one query for all orders, then one query per order for lines. Review this EF Core service method. What causes the N+1 pattern, and how do you fix it?

```csharp
public async Task<IEnumerable<string>> GetOrderLabelsAsync(DateOnly cutoff)
{
    var orders = await _db.Orders
        .Where(o => o.PlacedOn >= cutoff)
        .ToListAsync();

    return orders.Select(o =>
    {
        int lineCount = o.Lines.Count; // navigation property
        return $"{o.OrderId} ({o.Customer}): {lineCount} line(s), total {o.OrderTotal:C}";
    });
}
```

---

#### Q2. (R) A warehouse pick-list report shows the wrong row count and nested loops in code review. Review this projection. What is wrong with the LINQ, and what is the correct fix?

```csharp
public int CountPickRows(IEnumerable<Order> orders)
{
    IEnumerable<IReadOnlyList<OrderLine>> nested = orders.Select(o => o.Lines);
    return nested.Count(); // used in dashboard KPI
}

public IEnumerable<string> BuildPickLabels(IEnumerable<Order> orders)
{
    return orders
        .Select(o => o.Lines)
        .Select(lines => lines.First().Sku); // assumes one list per "row"
}
```

---

#### Q3. (R) A shared reporting library exposes order headers to a Web API project. The API project fails to compile after the refactor. Review both sides. What breaks at the assembly boundary, and what projection target should replace it?

```csharp
// OrderReportingLib (class library)
public static class OrderReportQueries
{
    public static IEnumerable<object> GetHighValueHeaders(IEnumerable<Order> orders, decimal minimum)
    {
        return orders
            .Where(o => o.OrderTotal >= minimum)
            .Select(o => new { o.OrderId, o.Customer, o.OrderTotal });
    }
}

// OrderApi (Web project) — does not compile
public IActionResult GetHighValue(decimal min)
{
    var rows = OrderReportQueries.GetHighValueHeaders(_orders, min);
    foreach (var row in rows)
    {
        _logger.LogInformation("{OrderId} {Total}", row.OrderId, row.OrderTotal); // CS1061
    }
    return Ok(rows);
}
```

---

#### Q4. (R) A paginated orders endpoint returns quickly in dev (small DB) but transfers megabytes per page in production. Review the repository. What is over-fetched, and how should projection change the SQL?

```csharp
public async Task<IPage<OrderHeaderDto>> GetRecentOrdersAsync(int page, int pageSize)
{
    var orders = await _db.Orders
        .Include(o => o.Lines)
        .OrderByDescending(o => o.PlacedOn)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync();

    var dtos = orders.Select(o => new OrderHeaderDto(
        o.OrderId,
        o.Customer,
        o.PlacedOn,
        o.OrderTotal));

    return new Page<OrderHeaderDto>(dtos, page, pageSize);
}
```

---

#### Q5. (R) Flattening order lines for a shipping-label printer loses parent context — labels print without OrderId. Review this SelectMany usage. What is missing, and what does the three-parameter overload fix?

```csharp
public IEnumerable<ShippingLabelRow> BuildLabelRows(IEnumerable<Order> orders)
{
    return orders.SelectMany(o => o.Lines)
        .Select(line => new ShippingLabelRow(
            line.Sku,
            line.ProductName,
            line.Quantity)); // ShippingLabelRow expects OrderId + Customer
}
```

---

#### Q6. (D) Your team ships three endpoints that all project orders: a JSON API, a CSV export, and an internal admin grid. One developer wants anonymous types everywhere "because LINQ is shorter." Another wants `(string Id, decimal Total)` tuples in the contracts assembly. A third wants `OrderLineSummary` records. What would you standardize for each boundary, and why?

---
