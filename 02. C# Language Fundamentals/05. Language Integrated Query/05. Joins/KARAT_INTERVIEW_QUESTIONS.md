# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/05. Joins`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A revenue dashboard reports "active customers with orders" using an inner join. Product asks why Harbor Supplies (C004) never appears and why totals do not match the orders table. Review this query against the chapter seed shape (`Customer`, `Order`, `Shipment`):

```csharp
var revenueByCustomer =
    from order in orders
    join customer in customers
        on order.CustomerId equals customer.CustomerId
    select new { customer.CustomerId, customer.Name, order.Total };

decimal dashboardTotal = revenueByCustomer.Sum(r => r.Total);
int distinctCustomers = revenueByCustomer.Select(r => r.CustomerId).Distinct().Count();
// distinctCustomers == customers.Length  →  false in QA
```

What rows are silently dropped, and how would you change the query depending on whether the report needs **all customers** vs **only customers with at least one order**?

---

#### Q2. (R) A developer ports the chapter's left-outer-join pattern but production throws `NullReferenceException` on customers with no orders. Review:

```csharp
var customerOrderLines =
    from customer in customers
    join order in orders
        on customer.CustomerId equals order.CustomerId
        into orderGroup
    from order in orderGroup.DefaultIfEmpty()
    select new
    {
        customer.Name,
        OrderId = order.OrderId,           // line flagged in review
        LineTotal = order.Total * 1.08m,   // tax on every row
    };
```

What is wrong with the left-join shape and the projection, and what is the correct method-syntax equivalent?

---

#### Q3. (R) A catalog team wants "every customer paired with every carrier they *could* use" for a shipping-options matrix. A junior dev copies a join snippet but gets 60 rows instead of 12 for 4 customers × 3 carriers. Review:

```csharp
string[] carriers = ["FedEx", "UPS", "DHL"];

var options =
    from customer in customers
    from carrier in carriers
    join order in orders
        on customer.CustomerId equals order.CustomerId
    select new { customer.Name, carrier, order.OrderId };

Console.WriteLine(options.Count()); // 60, not 12
```

What pattern caused the explosion, what row count should a true cross join produce, and how do you write the cartesian product correctly?

---

#### Q4. (R) Warehouse replenishment joins stock to reorder rows on `(Sku, WarehouseCode)`. QA reports SKU-200 @ WH-A never matches a reorder row that clearly exists in the CSV (`sku-200`, `wh-a`). Review:

```csharp
var replenishment =
    stock.Join(
        reorders,
        s => (s.Sku, s.WarehouseCode),
        r => (r.Sku, r.WarehouseCode),
        (s, r) => new { s.Sku, s.WarehouseCode, s.OnHand, r.ReorderQty });

// Separate attempt — filter active warehouses with default equality:
var active = new[] { "wh-a", "WH-B" };
var covered = active.Join(
    stock,
    code => code,
    row => row.WarehouseCode,
    (code, row) => code);
```

Why do case-mismatched keys fail to join, and when must you pass `IEqualityComparer<TKey>`?

---

#### Q5. (M) An EF Core API loads customers with optional orders using the idiomatic LINQ left-join pattern:

```csharp
var rows = await db.Customers
    .GroupJoin(
        db.Orders,
        c => c.CustomerId,
        o => o.CustomerId,
        (c, orderGroup) => new { c, orderGroup })
    .SelectMany(
        x => x.orderGroup.DefaultIfEmpty(),
        (x, o) => new CustomerOrderDto
        {
            Name = x.c.Name,
            OrderId = o != null ? o.OrderId : (int?)null,
            Total = o != null ? o.Total : null,
        })
    .ToListAsync();
```

What SQL shape does EF Core typically emit for this, and what changes if you replace `DefaultIfEmpty()` with `.SelectMany(o => o)` or move `.Where(o => o != null)` before the flatten step?

---

#### Q6. (R) Operations sees "duplicate" fulfillment lines for Order 101 in a shipped-orders report and opens a data-quality ticket. Review the chained inner joins:

```csharp
var shippedLines =
    from order in orders
    join customer in customers on order.CustomerId equals customer.CustomerId
    join shipment in shipments on order.OrderId equals shipment.OrderId
    select new { order.OrderId, customer.Name, shipment.Carrier, order.Total };

int lineCount = shippedLines.Count();
int distinctOrders = shippedLines.Select(l => l.OrderId).Distinct().Count();
// lineCount > distinctOrders — reported as duplicates
```

Is this a join bug or expected join semantics? How do row counts differ from `GroupJoin` on the same keys, and how would you aggregate without double-counting `order.Total`?

---

#### Q7. (D) You are choosing a pattern for a nightly export: **(A)** flat inner join of customers × orders, **(B)** `GroupJoin` keeping nested order lists per customer, **(C)** left join flattened with `DefaultIfEmpty`. Harbor Supplies has zero orders; Cascade Foods has two. Which pattern for (1) a CSV with one row per order, (2) a JSON file with one object per customer and an `orders` array, and (3) a master list that must include customers with zero orders?
