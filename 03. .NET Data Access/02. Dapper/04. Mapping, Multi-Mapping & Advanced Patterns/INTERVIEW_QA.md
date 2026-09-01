# Chapter 04. Mapping, Multi-Mapping & Advanced Patterns — Interview Q&A
> Back to [Dapper Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Chapter 04. Mapping, Multi-Mapping & Advanced Patterns](#chapter-04-mapping-multi-mapping--advanced-patterns)
  - [Q1. How does Dapper map column names to property names by default?](#q1-how-does-dapper-map-column-names-to-property-names-by-default)
  - [Q2. What happens when column names do not match property names?](#q2-what-happens-when-column-names-do-not-match-property-names)
  - [Q3. What is multi-mapping in Dapper?](#q3-what-is-multi-mapping-in-dapper)
  - [Q4. What is the `splitOn` parameter in multi-mapping?](#q4-what-is-the-spliton-parameter-in-multi-mapping)
  - [Q5. How does Dapper handle nested object graphs compared to EF Core `Include`?](#q5-how-does-dapper-handle-nested-object-graphs-compared-to-ef-core-include)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 04. Mapping, Multi-Mapping & Advanced Patterns

---

## Q1. How does Dapper map column names to property names by default?

**Concepts**
- case-insensitive name equality matching
- public property and field targeting
- column order independence
- underscore naming not auto-mapped
- nullable value type NULL handling

**Answer**

Dapper matches result column names to public properties and fields on the target type using case-insensitive name equality. The first column value maps to the property with the same name regardless of column order in the SELECT list, so `ProductName` and `productname` both map to a `ProductName` property. Underscore naming such as `product_name` does not auto-map to `ProductName` without SQL aliases or custom type maps — the names must be equal after case normalization, not just similar. Value types map directly; nullable value types accept database NULL as `null`. No attributes are required unless you add a custom type map or use a mapping extension.

---

## Q2. What happens when column names do not match property names?

**Concepts**
- silent partial object population
- unmatched columns ignored
- unmatched properties at default values
- SQL alias as fix
- SetTypeMap custom mapping

**Answer**

Unmatched columns are silently ignored, and unmatched properties remain at their default values — `null`, `0`, or `false` — without any exception. This silent partial population is a common source of bugs because Dapper never throws for missing mappings; you get back objects that look valid but are missing data. The straightforward fix is a SQL alias: `SELECT FirstName AS GivenName` to match a `GivenName` property. For more systematic mapping, `SqlMapper.SetTypeMap` accepts a custom type map, or you can implement `ITypeHandler` for special conversions. Integration tests that assert specific property values are the most reliable way to catch name mismatches before production.

---

## Q3. What is multi-mapping in Dapper?

**Concepts**
- single-row multi-type decomposition
- join result graph assembly delegate
- Query generic type overloads
- splitOn boundary column
- N+1 avoidance pattern

**Answer**

Multi-mapping lets a single row with columns from a JOIN decompose into multiple nested objects in one `Query` call, using a delegate to assemble the parent-child graph. Overloads like `Query<Order, Customer, Order>` accept two or more type parameters plus a `Func` that combines them into the return type. A typical pattern is `SELECT o.*, c.* FROM Orders o JOIN Customers c ...` mapped to an `Order` instance with a nested `Customer` property. The split point between object types is controlled by the `splitOn` parameter. Dapper invokes the mapping function once per row, so you decide how to attach the child object to the parent. This approach avoids N+1 queries without forcing you to load flat DTOs and then manually group by parent ID in memory.

---

## Q4. What is the `splitOn` parameter in multi-mapping?

**Concepts**
- column boundary for object type split
- default "Id" splitOn behavior
- duplicate Id column aliasing requirement
- silent wrong-value risk on incorrect splitOn
- SELECT column order alignment

**Answer**

`splitOn` names the column in the result row where the next object type's properties begin. Dapper maps all columns before that column name to the first generic type, and all columns from that name onward to the second type, repeating for additional type parameters. The default is `"Id"` — this works when the second entity's first mapped column is literally named `Id` and appears in the SELECT after the first entity's columns. Duplicate `Id` columns in a join require explicit SQL aliases and a matching `splitOn` value such as `"CustomerId"` to point at the right split boundary. An incorrect `splitOn` silently maps NULL or wrong values into nested objects rather than throwing, making integration tests that assert nested property values essential.

---

## Q5. How does Dapper handle nested object graphs compared to EF Core `Include`?

**Concepts**
- explicit SQL join requirement
- absence of navigation properties
- multi-mapping vs Include/ThenInclude
- no lazy loading
- precise control vs declarative relationship traversal

**Answer**

Dapper has no navigation properties or automatic graph loading — I shape graphs explicitly with SQL joins, multi-mapping overloads, or multiple queries. EF Core's `Include` and `ThenInclude` translate declared relationship metadata into join or split queries automatically, with the change tracker deduplicating parent instances during fix-up. With Dapper, I write the join, choose whether to use multi-map or separate queries, and merge duplicate parent rows in code when a one-parent-to-many-children join produces repeated parent columns. Dapper does not lazy-load — if columns are absent from the SELECT, the properties simply stay at default values with no hidden round-trips. EF Core's cartesian explosion from loading multiple collection includes has no Dapper equivalent unless I write the wide join myself, so Dapper gives me precise control over both what data is fetched and how it is assembled.

---

## Gotchas

---

## Gotcha 8. Multi-map `splitOn` wrong column

**Concepts**
- forward-only splitOn matching
- default "Id" ambiguity in JOINs
- silent NULL or wrong value mapping
- SELECT column order requirement
- integration test assertion necessity

**Answer**

Dapper multi-mapping uses `splitOn` to name the column where the next object type begins. An incorrect column causes the split to land at the wrong boundary, silently mapping NULL or wrong values into nested objects without throwing an exception. The `splitOn` default of `"Id"` requires that the second type's first mapped column is literally named `Id` — duplicate column names in SELECT lists require explicit aliases and matching `splitOn` values. Column order in the SELECT must align with the generic type order in `Query<TFirst, TSecond, TReturn>`. Integration tests that assert nested property values are the reliable way to catch split errors.

---

## Gotcha 9. Scoped `DbContext` captured in a singleton

**Concepts**
- captive dependency anti-pattern
- singleton service lifetime
- scoped DbContext per HTTP request
- IDbContextFactory for long-lived services
- disposed context cross-request contamination

**Answer**

Registering a singleton service that holds a scoped `DbContext` creates a captive dependency — the context may be disposed while the singleton remains alive, or its tracked state leaks across HTTP requests from different users. `DbContext` is scoped per request in ASP.NET Core, so singletons must never store it in fields. When a long-lived service genuinely needs database access, inject `IDbContextFactory<TContext>` instead, which lets the singleton create and dispose short-lived context instances on demand. Symptoms include "Cannot access a disposed context" exceptions or cross-user data contamination in tracked entities.

---

## Gotcha 11. N+1 from lazy load or missing Include

**Concepts**
- N+1 query pattern
- navigation property loop access
- eager loading with Include/ThenInclude
- split queries
- EF Core command logging detection

**Answer**

Listing parent entities and then accessing navigation properties in a loop without eager loading fires one SQL query per parent row — the classic N+1 performance collapse. One query for N orders plus N queries for each order's lines equals N+1 round-trips per request, which compounds into catastrophic latency and database load at scale. The fix is `Include`/`ThenInclude`, split query mode, or `Select` projections that join the needed data in one statement. EF Core command logging revealing identical query templates with different ID parameters is the diagnostic signal that N+1 is occurring.

---

## Gotcha 12. Cartesian explosion with multiple Includes

**Concepts**
- cross-join row multiplication
- multiple collection include inflation
- AsSplitQuery separation
- EF Core change tracker deduplication
- DTO projection as avoidance strategy

**Answer**

Eager-loading two or more collection navigations in one SQL query multiplies result rows by the product of collection sizes — an order with 10 lines and 5 notes produces 50 rows, spiking memory and network use even though the parent entity count is modest. EF Core deduplicates parent instances during fix-up, but SQL Server has already transmitted the inflated rowset across the wire. The fix is `AsSplitQuery()`, which fetches collections with separate SELECT statements that each return only the rows they need. Projecting to DTOs is another avoidance strategy when only summaries or counts are needed rather than full collection graphs.

---

## Scenario-Based Questions (Karat Format)

---

## Q1. (R) A teammate removes the explicit `splitOn` from `GetOrdersWithCustomers` because "both sides have an Id-like key." Orders load in QA but `Customer.Name` is null and `Customer.CustomerId` equals `Order.OrderId`. What is wrong and how do you fix it?

```csharp
return connection.Query<Order, Customer, Order>(
    sql,
    (order, customer) => { order.Customer = customer; return order; }
).ToList(); // splitOn removed — defaults to "Id"
```

**Concepts**
- splitOn default "Id" column matching
- duplicate CustomerId boundary ambiguity
- wrong split silently mismapping columns
- explicit splitOn on unique second-type column
- SQL alias as alternative disambiguation

**Answer**

Dapper's default `splitOn` is `"Id"` — it splits at the first column matching that name. In the JOIN query, `CustomerId` appears twice: once as `o.CustomerId` (a foreign key on Order) and once as `c.CustomerId` (the primary key on Customer). Without an explicit `splitOn`, Dapper finds the first `CustomerId` column — which belongs to Order — and starts mapping Customer from there. Everything after Order's `CustomerId` maps into the Customer type, but the Customer primary key never appears at the correct boundary, so `Customer.Name` and `Customer.Email` never bind.

The fix is an explicit `splitOn` pointing to a column unique to the Customer portion of the row — for example, `splitOn: "Name"` if Orders have no `Name` column, or a SQL alias like `c.CustomerId AS CustCustomerId` with `splitOn: "CustCustomerId"`. Column order must be maintained: all Order columns first, then all Customer columns. Never rely on default `splitOn` for real JOIN queries with shared column name patterns.

---

## Q2. (R) Another developer models order lines without the dictionary lookup. The API returns three lines for order 1 but `TotalAmount` and `Lines.Count` disagree depending on which element the caller uses. Diagnose and describe the production-safe aggregation pattern.

```csharp
List<Order> orders = connection.Query<Order, OrderLine, Order>(
    sql,
    (order, line) => { order.Lines.Add(line); return order; },
    new { orderId }, splitOn: "OrderLineId").ToList();
return orders.FirstOrDefault();
```

**Concepts**
- delegate called once per JOIN row
- new Order instance per row in map delegate
- dictionary deduplication for parent aggregation
- one-to-many multi-map pattern
- FirstOrDefault hiding fragmentation

**Answer**

Dapper's map delegate runs once per row returned by the JOIN. Each invocation receives a freshly materialized `Order` and `OrderLine` — they are not the same `Order` instance across rows. Calling `order.Lines.Add(line)` modifies a brand-new `Order` object each time, so the list ultimately contains one partial `Order` per line rather than one aggregated Order with all lines. `FirstOrDefault()` returns the first of these partial objects, which has exactly one line regardless of how many lines the order has.

The production-safe pattern is a dictionary that tracks `Order` instances by primary key:

```csharp
var lookup = new Dictionary<int, Order>();
connection.Query<Order, OrderLine, Order>(
    sql,
    (order, line) =>
    {
        if (!lookup.TryGetValue(order.OrderId, out var existing))
            lookup[order.OrderId] = existing = order;
        existing.Lines.Add(line);
        return existing;
    },
    new { orderId }, splitOn: "OrderLineId");
return lookup.TryGetValue(orderId, out var result) ? result : null;
```

Multi-mapping handles column splitting, not aggregation — one-to-many relationships always need explicit parent deduplication.

---

## Q3. (R) A new microservice copies `ColumnMappedProduct` and `DateOnlyTypeHandler` but omits the `RegisterDapperExtensions` call. `Price` is always `0` in some test runs; `QuerySingleAsync<DateOnly>` throws `DataException` in others. What is missing and why does test order matter?

**Concepts**
- Dapper global static type registration
- SqlMapper.AddTypeHandler startup requirement
- SqlMapper.SetTypeMap column attribute mapping
- test fixture one-time registration
- test-order-dependent flakiness from shared static state

**Answer**

`SqlMapper.AddTypeHandler` and `SqlMapper.SetTypeMap` modify global static dictionaries in Dapper. They must be called once before any query that uses those types. Without the registration call in `Program.cs`, the `[Column("UnitPrice")]` attribute is never applied so `Price` stays at its default zero, and `DateTime` from a `DATE` column cannot convert to `DateOnly` without the type handler — causing `DataException` at read time.

Test order matters because Dapper's static state persists across tests in the same process. If a test that indirectly triggers registration runs first — perhaps through a `WebApplicationFactory` — the handler is already registered and the test passes. Run the tests in a different order and registration never happens, so the test fails. This is the classic shared-static-state flakiness pattern.

The fix is to call the registration in a single guaranteed-to-run location: `Program.cs` for production, and a shared `IClassFixture` or `[ModuleInitializer]` for tests. Where SQL aliases can cover the rename (`UnitPrice AS Price`), prefer them over global type maps to keep configuration local and explicit.

---

## Q4. (P) Production needs `DiscontinuedDate` mapped to `DateOnly?`. A junior registers a type handler, but under load some NULL rows throw and parameterized inserts sometimes fail. What should `Parse` and `SetValue` handle?

```csharp
public override DateOnly? Parse(object value) =>
    value is DateTime dt ? DateOnly.FromDateTime(dt) : null;

public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
    parameter.Value = value?.ToDateTime(TimeOnly.MinValue);
```

**Concepts**
- DBNull.Value distinct from C# null in ADO.NET
- Parse DBNull explicit handling
- SetValue DBNull.Value for SQL NULL
- IDbDataParameter.Value null vs DBNull
- TypeHandler vs SQL CAST/CONVERT trade-off

**Answer**

ADO.NET surfaces database NULL values as `DBNull.Value`, not as C# `null`. The `Parse` method's `value is DateTime dt` pattern never matches `DBNull.Value`, so NULL database columns can fall through and throw `InvalidCastException` in some provider implementations. The explicit safe guard is:

```csharp
public override DateOnly? Parse(object value)
{
    if (value is null or DBNull) return null;
    return value is DateTime dt ? DateOnly.FromDateTime(dt) : DateOnly.Parse(value.ToString()!);
}
```

For `SetValue`, assigning `parameter.Value = null` (from `value?.ToDateTime(...)` when `value` is null) may not properly signal SQL NULL to the provider — `IDbDataParameter.Value` must be set to `DBNull.Value` explicitly:

```csharp
public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
    parameter.Value = value.HasValue ? (object)value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
```

Use a TypeHandler when the conversion recurs across many queries or has both read and write directions. For a one-off read-only DTO against a single query, a SQL `CAST(DiscontinuedDate AS date)` alias with a `DateTime?` property avoids the registration overhead entirely.

---

## Q5. (D) An order-details endpoint loops over 200 orders fetching customer and lines per order. A proposal replaces the loop with JOIN queries using multi-mapping. Compare N+1 queries, two JOIN queries, and one wide JOIN with dictionary aggregation. What would you ship for a paginated admin grid vs a single-order detail page?

**Concepts**
- N+1 latency and connection scaling
- JOIN multi-map eliminating round-trips
- cartesian row duplication in wide JOINs
- paginated grid shallow hydration pattern
- single-detail deep hydration pattern

**Answer**

N+1 is wrong for any hot endpoint: 200 orders triggering 400 additional queries produces 401 round-trips per request and collapses under concurrent load. The question is which JOIN strategy fits which page.

For a paginated admin grid showing 200 orders with customer name only, I ship one JOIN — `Query<Order, Customer, Order>` with explicit `splitOn` — and skip loading lines entirely. The grid never displays lines, so loading them wastes memory and widens rows unnecessarily.

For a single-order detail page, I ship two queries: one JOIN for order plus customer, and one multi-map with dictionary aggregation for that order's lines. Two queries keeps mapping simple, avoids duplicating order and customer columns per line row, and remains fast because the dataset is narrow. A single wide JOIN of order plus customer plus lines also works when line count is bounded — use the dictionary aggregation pattern and never return the duplicated flat rows directly to the client.

One wide JOIN for a list of 200 orders with lines is inappropriate: it duplicates order and customer columns once per line, multiplying data transfer and memory. Multi-mapping eliminates N+1 round-trips but does not replace pagination discipline — always constrain how deep each endpoint hydrates.

---

## Q6. (R) A developer migrates INSERT logic to Dapper.Contrib. `ProductId` is not IDENTITY — keys are assigned manually. Insert appears to succeed but `Get` returns null. SQL Profiler shows an INSERT without `ProductId`. What went wrong with Contrib attributes?

```csharp
[Table("Products")]
public sealed class ProductEntity
{
    [Key] // should be [ExplicitKey]
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
```

**Concepts**
- Contrib [Key] vs [ExplicitKey] distinction
- IDENTITY column assumption in [Key]
- INSERT column exclusion for auto-generated keys
- [ExplicitKey] for manually assigned keys
- MAX+1 concurrency risk regardless of attribute

**Answer**

`[Key]` tells Dapper.Contrib that `ProductId` is an identity column — the database generates its value, so Contrib excludes it from the INSERT statement. When the application assigns the ID manually and marks it `[Key]`, Contrib emits an INSERT with no `ProductId` column. The insert either fails on a NOT NULL constraint, inserts a zero or default value, or inserts with the wrong key — hence `GetAsync` returning null because the row is not findable by the expected ID.

The fix is to change `[Key]` to `[ExplicitKey]`, which tells Contrib that the application supplies the key value and it must be included in the INSERT. Verify the generated SQL in SQL Profiler after the change to confirm `ProductId` appears in the INSERT.

The `MAX(ProductId) + 1` approach for ID generation remains a concurrency risk even after fixing the attribute: two concurrent transactions can compute the same next ID and race to insert, producing a primary key violation. For production, delegate ID generation to a database `SEQUENCE` or `IDENTITY` column, or wrap the max-computation and insert in a serialized transaction with appropriate locking.

---

## Q7. (R) A search API builds product filters with string concatenation. Security review flags SQL injection on `category` and `sortColumn`. Refactor toward Dapper.SqlBuilder with parameterized fragments.

```csharp
var sql = "SELECT ... WHERE 1=1";
if (!string.IsNullOrEmpty(category))
    sql += $" AND Category = '{category}'";
if (minPrice.HasValue)
    sql += $" AND UnitPrice >= {minPrice.Value}";
sql += $" ORDER BY {sortColumn}";
```

**Concepts**
- user value injection via filter concatenation
- sort column identifier injection
- SqlBuilder parameterized WHERE fragments
- column name whitelist for ORDER BY
- values vs identifiers in SQL injection model

**Answer**

Both `category` and `sortColumn` are injected directly into SQL as string literals. For `category`, an attacker can close the string with a quote and append arbitrary SQL. For `sortColumn`, identifier injection is equally dangerous — `'; DROP TABLE dbo.Products; --` appended to `ORDER BY` can execute arbitrary statements.

The solution splits responsibility between parameters and whitelisting. User-supplied values like `category` and `minPrice` flow through Dapper parameters; column names cannot be parameterized in T-SQL and must come from a fixed whitelist:

```csharp
var builder = new SqlBuilder();
var template = builder.AddTemplate(
    "SELECT ProductId, ProductName, UnitPrice FROM dbo.Products /**where**/ /**orderby**/");

if (!string.IsNullOrEmpty(category))
    builder.Where("Category = @Category", new { Category = category });
if (minPrice.HasValue)
    builder.Where("UnitPrice >= @MinPrice", new { MinPrice = minPrice });

var sortCol = sortColumn switch
{
    "Name" => "ProductName",
    "Price" => "UnitPrice",
    _ => "ProductId"
};
builder.OrderBy(sortCol);

return conn.Query<ProductRow>(template.RawSql, template.Parameters).ToList();
```

The whitelist maps the user's sort request to a literal column name chosen by the application — an attacker who passes an unsupported sort key gets the default `ProductId` ordering, never their injected SQL. Parameters protect value predicates; the whitelist protects identifier-position values.
