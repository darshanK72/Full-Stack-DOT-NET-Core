# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/05. Joins`

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

**Answer:** Inner `join` keeps only key matches — customers with no orders (Harbor Supplies) and any unmatched inner-side rows vanish without error, so `distinctCustomers` reflects order-holding customers only, not `customers.Length`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Inner join drops unmatched **outer** rows when starting from orders; starting from customers with inner join to orders also drops customers with zero orders | Harbor Supplies missing from any "all customers" report |
| Expectations | Comparing `distinctCustomers` to `customers.Length` assumes left/full coverage | False QA failure; product thinks data is corrupt |
| Semantics | Inner join is correct only when the business rule is "customers **with** at least one order" | Wrong operator if zero-order customers must appear |

**Fix (priority order):**

1. **Clarify the requirement** — "customers with orders" → inner join from `orders` (or `customers` inner join `orders`) is correct; document that zero-order customers are intentionally excluded.
2. **All customers, optional order data** → left outer join: `GroupJoin` + `SelectMany` + `DefaultIfEmpty()` (Section 11 pattern), starting from `customers` as the outer sequence.
3. **All customers listed even with zero orders, one row per customer** → `GroupJoin` without flattening, or left join then `GroupBy` customer if multiple order rows are acceptable.
4. Do not "fix" missing Harbor by switching to cross join — that invents pairings without a key.

**Production takeaway:** Inner join data loss is silent — the #1 join production bug is using `Join` when stakeholders expect every parent row. See **Program.cs** Section 4 — Harbor Supplies deliberately absent from inner join; Section 11 keeps them.

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

**Answer:** The `GroupJoin` + `DefaultIfEmpty()` shape is correct for a left join, but when `orderGroup` is empty, `DefaultIfEmpty()` yields `null` for reference-type `Order` — dereferencing `order.OrderId` or `order.Total` throws. Use null-conditional or explicit null checks in the projection.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `order.OrderId` / `order.Total` without null guard | `NullReferenceException` for Harbor Supplies (zero orders) |
| Type choice | `Order` is a reference-type record — empty group → `null`, not `default(Order)` with safe fields | Value-type inners would yield `default(T)` (often misleading zeros) |
| Pattern | Left join requires **flatten** step — `into` alone is `GroupJoin` (nested), not flat left join | Skipping `from … DefaultIfEmpty()` drops unmatched outers entirely |

**Fix (priority order):**

1. Null-safe projection: `OrderId = order != null ? order.OrderId : (int?)null`, `LineTotal = order != null ? order.Total * 1.08m : null`.
2. Or: `order?.OrderId`, `order?.Total * 1.08m` with nullable result types as needed.
3. Method-syntax equivalent (matches **Program.cs** Section 11):

```csharp
customers.GroupJoin(
        orders,
        c => c.CustomerId,
        o => o.CustomerId,
        (c, orderGroup) => new { c, orderGroup })
    .SelectMany(
        x => x.orderGroup.DefaultIfEmpty(),
        (x, order) => new
        {
            x.c.Name,
            OrderId = order != null ? order.OrderId : (int?)null,
            LineTotal = order != null ? order.Total * 1.08m : (decimal?)null,
        });
```

4. For one-to-many outers (Cascade Foods → two orders), left join flatten produces **two** rows — expected; do not assume one row per customer unless you `GroupJoin` without flattening.

**Production takeaway:** Left join in LINQ to Objects is always **GroupJoin + SelectMany + DefaultIfEmpty** — the `into` clause alone is not enough. See **Program.cs** Sections 9–11 and quick reference "Common mistakes — Null inner after left join".

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

**Answer:** Mixing a nested `from` (cross product) with a key-based `join` multiplies customers × carriers × matching orders — not a pure cartesian product. A true cross join of 4 customers × 3 carriers yields **12** rows; here you get roughly |customers| × |carriers| × (orders per customer).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `from carrier in carriers` cross-multiplies **before** the join filter on orders | Row count driven by order multiplicity, not |A| × |B| |
| Performance | Accidental cartesian + join on large sequences | Memory/CPU explosion in prod (e.g., 10k × 10k × matches) |
| Operator confusion | `Join` needs keys; cross join has **no** `on` clause | Wrong mental model — "join all the things" |

**Fix (priority order):**

1. **Pure cross join** — remove the `join` clause entirely:

```csharp
var shippingMatrix =
    from customer in customers
    from carrier in carriers
    select new { customer.Name, carrier };

// or: customers.SelectMany(c => carriers, (c, carrier) => new { c.Name, carrier });
```

2. **Customer × carrier only for customers who have orders** — filter customers first, **then** cross with carriers (still 12 max if all 4 have orders — but Harbor has none, so 3 × 3 = 9 if filtered).
3. **Customer × order × carrier** — intentional three-way expansion; document expected count and aggregate carefully.
4. In SQL/EF, cross join is `from a in A from b in B` with no `join`; guard against accidental nested `from` when a keyed `Join` was intended.

**Production takeaway:** Cross join row count is always |outer| × |inner| — if counts look like multiples of order volume, you stacked cross product with key join. See **Program.cs** Section 13 — 4 × 3 = 12 vs true join match count 5.

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
var covered = active.Join(
    stock,
    code => code,
    row => row.WarehouseCode,
    (code, row) => code);
```

Why do case-mismatched keys fail to join, and when must you pass `IEqualityComparer<TKey>`?

**Answer:** Default join equality uses `EqualityComparer<TKey>.Default` — for strings that is **ordinal, case-sensitive**, so `"SKU-200"` ≠ `"sku-200"` and `"WH-A"` ≠ `"wh-a"`. Pass `StringComparer.OrdinalIgnoreCase` (or a custom comparer for composite keys) when keys are logically equal but differ by culture/casing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Case-sensitive tuple/string keys miss valid matches | SKU-200 replenishment row absent — understock false negative |
| Data integration | CSV feeds often vary casing; DB collations may differ from in-memory LINQ | Works in SQL with CI collation, fails in LINQ to Objects |
| Null keys | `null` equals `null` in join keys, but null SKU/warehouse usually means "unknown" — often excluded from both sides | Silent non-match or unintended matches depending on data |

**Fix (priority order):**

1. Normalize keys at ingest: `Sku = sku.Trim().ToUpperInvariant()` on both sequences before join (consistent for batch jobs).
2. Or use comparer overload:

```csharp
stock.Join(reorders,
    s => s.Sku, r => r.Sku,
    (s, r) => …,
    StringComparer.OrdinalIgnoreCase);
```

3. For composite keys with mixed case, project a normalized key or implement `IEqualityComparer<(string Sku, string Wh)>`.
4. **Query syntax** composite keys: anonymous types on both sides of `equals` — property **names and order** must align; `"Sku"` vs `"SKU"` on one side breaks matching.
5. Align with EF/SQL: push casing rules to the database (`LOWER()`, CI collation) so translated SQL matches business rules.

**Production takeaway:** Join keys are compared in memory unless EF translates them — never assume CSV casing matches entity properties. See **Program.cs** Section 7 — `StringComparer.OrdinalIgnoreCase` overload and `sku-200` / `wh-a` demo row.

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

**Answer:** EF Core translates `GroupJoin` + `SelectMany` + `DefaultIfEmpty()` to a **LEFT JOIN** (or equivalent OUTER APPLY) in SQL — customers without orders appear with NULL order columns. Removing `DefaultIfEmpty()` turns it into an inner join; filtering nulls before flatten also drops unmatched customers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Translation | Pattern maps to SQL `LEFT JOIN` + selected columns | Correct server-side left join when fully translatable |
| Pitfall | `.SelectMany(x => x.orderGroup)` without `DefaultIfEmpty()` | Inner join semantics — customers with zero orders disappear |
| Pitfall | `.Where(o => o != null)` on the group before `SelectMany` | Equivalent to inner join filter — same data loss as Q1 |
| Client eval | Complex result selectors or non-translatable lambdas after join | EF may client-evaluate part of the tree — N+1 or memory load |

**Fix (priority order):**

1. Keep `DefaultIfEmpty()` for optional related data; verify generated SQL with `ToQueryString()` (EF Core 5+) or logging.
2. Prefer explicit shape when readable: some teams use `from c in db.Customers join o in db.Orders … into g from o in g.DefaultIfEmpty()` — same translation.
3. **Include / projection:** For simple "customer + orders collection", `Include(c => c.Orders)` or a grouped projection may be clearer than manual left join.
4. Avoid `.SelectMany(o => o)` thinking it "flattens" — that skips the default row for empty groups.
5. Watch **cartesian explosion** when left-joining multiple collections in one query — EF Core 8+ documents split queries / `AsSplitQuery()` for one-to-many includes.

**Production takeaway:** The LINQ left-join recipe exists precisely because there is no `LeftJoin` operator — EF maps it to SQL OUTER JOIN when keys are translatable. See **Program.cs** Section 11 method-syntax pattern; test SQL, not just in-memory parity.

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

**Answer:** This is expected **one-to-many** inner join behavior — Order 101 has two shipments, so it correctly appears twice. `Join` emits one row per **key match**, not one row per order; summing `order.Total` on the flat result double-counts.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Expectations | Treating `lineCount > distinctOrders` as duplicate data | Wasted data-quality investigation |
| Aggregation | `Sum(l => l.Total)` on flat join | Revenue inflated by shipment multiplicity |
| Alternative shape | Need one row per order with nested shipments | `GroupJoin` keeps one order with `IEnumerable<Shipment>` |

**Fix (priority order):**

1. **Shipment-level report** — current query is correct; count lines, not distinct orders; do not sum order total per line without deduping.
2. **Order-level revenue** — aggregate on orders first, or `DistinctBy(l => l.OrderId)` before sum, or join orders to customers only and attach shipment count separately.
3. **Nested view** — `orders.GroupJoin(shipments, …)` → one element per order, O101 group size 2 (**Program.cs** Section 9b).
4. **Orders without shipments** — chained **inner** join to shipments drops O105; use left join on shipments if unshipped orders must appear.

```csharp
// Order-level total — do not sum on shipment-expanded rows:
decimal orderRevenue = orders.Sum(o => o.Total);

// Or shipment lines without double-counting order fields in rollups:
var byOrder = shippedLines.GroupBy(l => l.OrderId)
    .Select(g => new { OrderId = g.Key, Total = g.First().Total, Shipments = g.Count() });
```

**Production takeaway:** Inner join multiplies on one-to-many relationships — the fulfillment report in **Program.cs** Section 8 intentionally shows two lines for O101. Use `GroupJoin` when the consumer needs one outer row with nested inners.

---

#### Q7. (D) You are choosing a pattern for a nightly export: **(A)** flat inner join of customers × orders, **(B)** `GroupJoin` keeping nested order lists per customer, **(C)** left join flattened with `DefaultIfEmpty`. Harbor Supplies has zero orders; Cascade Foods has two. Which pattern for (1) a CSV with one row per order, (2) a JSON file with one object per customer and an `orders` array, and (3) a master list that must include customers with zero orders?

**Answer:** Match the join shape to the output grain — flat inner join for order rows only, `GroupJoin` for nested per-customer documents, left join flatten for a flat file that must list every customer including those with zero orders.

- **(1) CSV — one row per order:** **(A) Inner join** (`customers` join `orders` or start from `orders` join `customers`). Harbor Supplies omitted (no orders); Cascade Foods produces **two** rows. Correct when the file is "order fact" data, not a customer census.
- **(2) JSON — one object per customer with `orders` array:** **(B) `GroupJoin`**. Each customer is one outer element; Cascade Foods gets `orders: [103, 104]`; Harbor gets `orders: []`. Maps cleanly to serialization without a second grouping pass. Row count = `customers.Length` (4).
- **(3) Master list including zero-order customers:** **(C) Left join flattened** if the CSV must list every customer on every row (Harbor → one row with null order columns), or **(B)** if you generate customer headers then emit child rows — but for a **flat** master with optional order columns, **GroupJoin + SelectMany + DefaultIfEmpty** (6 rows here: 5 order rows + 1 Harbor row with nulls). Choose **(B)** if the deliverable is hierarchical; **(C)** if downstream tools require a single flat table.

**Production takeaway:** Join vs GroupJoin vs left join is a **reporting grain** decision — see **Program.cs** Section 10 comparison table and Section 14 row-count summary. Pick operator first from "what is one output record?", not from SQL habit alone.
