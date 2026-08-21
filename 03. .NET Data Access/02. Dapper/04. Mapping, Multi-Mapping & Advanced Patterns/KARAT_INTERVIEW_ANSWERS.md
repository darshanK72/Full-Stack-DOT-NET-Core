# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/02. Dapper/04. Mapping, Multi-Mapping & Advanced Patterns`

---

#### Q1. (R) A teammate ships a "fix" to `GetOrdersWithCustomers` that removes the explicit `splitOn` because both sides have an `Id`-like key. Review the change:

```csharp
public IReadOnlyList<Order> GetOrdersWithCustomers()
{
    const string sql = """
        SELECT
            o.OrderId,
            o.OrderDate,
            o.TotalAmount,
            o.CustomerId,
            c.CustomerId,
            c.Name,
            c.Email
        FROM dbo.Orders o
        INNER JOIN dbo.Customers c ON o.CustomerId = c.CustomerId
        ORDER BY o.OrderId;
        """;

    using SqlConnection connection = new SqlConnection(_connectionString);

    return connection.Query<Order, Customer, Order>(
        sql,
        (order, customer) =>
        {
            order.Customer = customer;
            return order;
        }).ToList(); // splitOn removed — Dapper defaults to "Id"
}
```

Orders load in QA, but `Customer.Name` is null and `Customer.CustomerId` equals `Order.OrderId` on several rows. What is wrong, and how do you fix it with minimal SQL change?

**Answer:** Dapper splits at the **first column whose name matches `splitOn`** — default `"Id"` is not present, so the next ambiguous match is the **first `CustomerId`**, which belongs to `Order`, not `Customer`. Everything after that column is mapped into `Customer`, so `Name`/`Email` never bind and `Customer.CustomerId` receives the wrong slice of the row.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / mapping | Omitted `splitOn` with duplicate `CustomerId` columns | Split point lands on Order's FK, not Customer's columns |
| Correctness | `Customer` object partially or wrongly populated | Silent data corruption in API responses |
| Maintainability | Implicit default `"Id"` on JOIN queries | Breaks when schema adds/removes `Id`-named columns |

**Fix (priority order):**

1. Restore an explicit `splitOn` on a **Customer-only** column — as in **MultiMapRepository.cs**: `splitOn: "Name"` (Orders have no `Name` column).
2. Alternatively, alias the second key: `c.CustomerId AS CustCustomerId` and use `splitOn: "CustCustomerId"`.
3. Keep column order: all `Order` columns first, then all `Customer` columns — never rely on default `splitOn` for JOINs.

**Production takeaway:** `splitOn` is not optional on real JOINs; duplicate column names are the most common multi-map production bug. Prefer a unique TSecond column or SQL alias over guessing defaults.

---

#### Q2. (R) Another developer models order lines without the dictionary lookup from this chapter:

```csharp
public Order? GetOrderWithLines(int orderId)
{
    const string sql = """
        SELECT o.OrderId, o.CustomerId, o.OrderDate, o.TotalAmount,
               ol.OrderLineId, ol.OrderId, ol.ProductId, ol.Quantity, ol.LineTotal
        FROM dbo.Orders o
        INNER JOIN dbo.OrderLines ol ON o.OrderId = ol.OrderId
        WHERE o.OrderId = @orderId
        ORDER BY ol.OrderLineId;
        """;

    using SqlConnection connection = new SqlConnection(_connectionString);

    List<Order> orders = connection.Query<Order, OrderLine, Order>(
        sql,
        (order, line) =>
        {
            order.Lines.Add(line);
            return order;
        },
        new { orderId },
        splitOn: "OrderLineId").ToList();

    return orders.FirstOrDefault();
}
```

The API returns three lines for order 1, but `TotalAmount` and `Lines.Count` disagree depending on which list element the caller uses. Diagnose the bug and describe the production-safe aggregation pattern.

**Answer:** Dapper's map delegate runs **once per JOIN row**. Each row materializes a **new** `Order` instance; mutating `order.Lines` on one row does not merge siblings. `.ToList()` therefore returns **one partial `Order` per line** — `FirstOrDefault()` keeps only the first line's parent shell.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | No `Dictionary<int, Order>` collapse | Duplicate parent objects; fragmented `Lines` collections |
| API contract | `FirstOrDefault()` hides duplicates | Callers see 1 line while DB has N |
| Design | Treating multi-map like single `Query<Order>` | One-to-many JOIN semantics misunderstood |

**Fix (priority order):**

1. Use the **lookup dictionary idiom** from **MultiMapRepository.GetOrderWithLines** — `TryGetValue(order.OrderId, …)`, initialize `Lines` once, `Add(line)` on the tracked instance.
2. Return `lookup.TryGetValue(orderId, out var result) ? result : null` — not the raw `IEnumerable` from `Query`.
3. Keep `splitOn: "OrderLineId"` — first column that starts `OrderLine`.

**Production takeaway:** Multi-map solves column splitting, not aggregation; one-to-many always needs explicit parent deduplication (dictionary, `Lookup`, or `GroupBy` after materialization).

---

#### Q3. (R) A new microservice copies `ColumnMappedProduct` and `DateOnlyTypeHandler` into a Web API project. Integration tests fail intermittently:

```csharp
// Program.cs — no RegisterDapperExtensions call
var app = builder.Build();
app.MapGet("/products/{id}", async (int id, IProductRepo repo) =>
    await repo.GetByIdAsync(id));

// ProductRepository.cs
public async Task<ColumnMappedProduct?> GetByIdAsync(int id)
{
    const string sql = "SELECT ProductId, ProductName, UnitPrice FROM dbo.Products WHERE ProductId = @id;";
    await using var conn = new SqlConnection(_cs);
    return await conn.QuerySingleOrDefaultAsync<ColumnMappedProduct>(sql, new { id });
}

// OrderSummaryRepository.cs — added for a new endpoint
public async Task<OrderSummaryDto> GetSummaryAsync(int orderId)
{
    const string sql = "SELECT OrderDate FROM dbo.Orders WHERE OrderId = @orderId;";
    await using var conn = new SqlConnection(_cs);
    var date = await conn.QuerySingleAsync<DateOnly>(sql, new { orderId });
    return new OrderSummaryDto { OrderDate = date };
}
```

`Price` is always `0` in some test runs; `QuerySingleAsync<DateOnly>` throws `DataException` in others. What is missing, where should it live, and why does test order matter?

**Answer:** Dapper extension registration (`SqlMapper.AddTypeHandler`, `SqlMapper.SetTypeMap`) is **global static state** and must run **once before any query** using those types. Without **RegisterDapperExtensions** from **Program.cs**, `UnitPrice` never maps to `Price` (stays default `0`), and `DateTime` from SQL cannot convert to `DateOnly` without **DateOnlyTypeHandler**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Missing `ColumnAttributeTypeMap.RegisterOnce()` | `[Column("UnitPrice")]` ignored — silent zero prices |
| Runtime | Missing `SqlMapper.AddTypeHandler(new DateOnlyTypeHandler())` | `Parse` fails on `DateTime` from `DATE` columns |
| Startup / tests | Registration not in host startup | Tests pass only if another test registered handlers first — order-dependent flakiness |
| Design | Per-request registration absent but also not centralized | Easy to forget when copying repository code |

**Fix (priority order):**

1. Call a single `RegisterDapperExtensions()` at application startup — mirror **Program.cs** lines 67 and 93–97: TypeHandler first, then TypeMap.
2. In tests, invoke the same registration in a **one-time fixture** (`IClassFixture`, `[ModuleInitializer]`, or `WebApplicationFactory` host build) — not per test method.
3. Prefer **SQL aliases** (`UnitPrice AS Price`) for simple renames when you want zero global config; keep TypeHandlers for true type mismatches (`DateOnly`, enums, JSON).

**Production takeaway:** Global Dapper configuration belongs next to composition root setup (`Program.cs` / host builder), shared by production and tests — never assume a tutorial's `Main` ran.

---

#### Q4. (P) Production needs `DiscontinuedDate` mapped to `DateOnly?` on `ProductRow` — the column is often `NULL`. A junior registers this handler:

```csharp
public sealed class NullableDateOnlyHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value) =>
        value is DateTime dt ? DateOnly.FromDateTime(dt) : null;

    public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
        parameter.Value = value?.ToDateTime(TimeOnly.MinValue);
}
```

Under load, some rows with `DiscontinuedDate = NULL` still throw; parameterized inserts sometimes send `DBNull` incorrectly. What should `Parse` and `SetValue` handle, and when do you prefer a TypeHandler over a SQL `CAST`/`CONVERT` alias in the SELECT?

**Answer:** The handler must treat **`DBNull.Value` and `null` explicitly** on read, and assign **`DBNull.Value`** (not C# `null`) to `IDbDataParameter.Value` when writing SQL NULLs. The ternary `value is DateTime` branch never runs for NULL database values if ADO.NET surfaces them as `DBNull`, and `parameter.Value = value?.…` leaves the parameter unset when `value` is null.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Parse` ignores `DBNull` | InvalidCastException or DataException on nullable columns |
| Runtime | `SetValue` uses C# null for parameter | SQL INSERT/UPDATE may omit NULL binding correctly only with `DBNull.Value` |
| Design | Handler registered for `DateOnly?` but entity uses non-nullable `DateOnly` elsewhere | Wrong handler generic — register matching open/nullable type |

**Fix (priority order):**

1. **Parse:** `if (value is null or DBNull) return null;` then handle `DateTime` / `DateOnly` like **DateOnlyTypeHandler.cs**.
2. **SetValue:** `parameter.Value = value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;`
3. Register with `SqlMapper.AddTypeHandler(new NullableDateOnlyHandler())` before queries — same startup rule as Section 6.

**When to use TypeHandler vs SQL alias:**

- **TypeHandler:** reuse across many queries, both directions (read/write), domain types (`DateOnly`, value objects, bool as char).
- **SQL alias/CAST:** one-off reporting query, read-only DTO, no insert path — e.g. `SELECT CAST(DiscontinuedDate AS date) AS DiscontinuedDate` into a `DateTime?` property without custom handler.

**Production takeaway:** Nullable value-type mapping fails on `DBNull` edge cases more often than on happy-path dates — mirror ADO.NET null semantics explicitly.

---

#### Q5. (D) An order-details endpoint currently loads 200 orders, then loops:

```csharp
foreach (var order in orders)
{
    order.Customer = await _repo.GetCustomerAsync(order.CustomerId); // N round-trips
    order.Lines = (await _repo.GetLinesAsync(order.OrderId)).ToList();
}
```

A proposal replaces the loop with one JOIN query using the chapter's `Query<Order, Customer, Order>` plus a second `GetOrderWithLines`-style multi-map. Compare **N+1 queries**, **two JOIN queries** (header + lines), and **one wide JOIN** with dictionary aggregation. What would you ship for a paginated admin grid vs a single-order detail page?

**Answer:** N+1 explodes latency and connection use under load; JOIN + multi-map collapses round-trips but trades **wider rows and mapping complexity**. The right shape depends on **payload depth** and **pagination**, not a universal rule.

- **Paginated admin grid (200 orders, customer name only):** Ship **one JOIN** — `Query<Order, Customer, Order>` with explicit `splitOn` (see **GetOrdersWithCustomers**). Skip lines entirely; do not multi-map children you will not display. Avoid N+1; also avoid loading all lines for every row.
- **Single-order detail page:** Ship **two queries** — (1) order + customer JOIN, (2) lines multi-map with dictionary for that `orderId` — or **one** order+lines JOIN if line count is bounded (tens, not thousands). Two queries keeps mapping simpler and avoids a cartesian explosion when lines grow.
- **One wide JOIN (order + customer + lines):** Acceptable for **one order id**; for lists it duplicates order/customer columns per line and increases memory — use dictionary aggregation and never return raw duplicated rows to the client.
- **N+1:** Reserve for small graphs, background jobs, or when cache hits dominate — not for hot list endpoints.

**Production takeaway:** Multi-map eliminates N+1 **when you fetch the graph in one SQL round-trip**; it does not replace pagination discipline or choosing how deep each endpoint hydrates.

---

#### Q6. (R) A developer migrates manual INSERT SQL to Dapper.Contrib on the same `Products` table (`ProductId` is **not** IDENTITY — keys are assigned manually):

```csharp
[Table("Products")]
public sealed class ProductEntity
{
    [Key] // was [ExplicitKey] in the tutorial sample
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}

// Insert path
entity.ProductId = await conn.ExecuteScalarAsync<int>(
    "SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products;");
await conn.InsertAsync(entity);
var loaded = await conn.GetAsync<ProductEntity>(entity.ProductId); // null
```

Insert appears to succeed but `Get` returns null; SQL Profiler shows an INSERT without `ProductId`. What went wrong with Contrib attributes, and what concurrency risk remains even after fixing the attribute?

**Answer:** `[Key]` tells Contrib the column is an **identity** key — Contrib **excludes it from INSERT** and expects the database to generate it. Manual keys require **`[ExplicitKey]`** as in **ContribProduct.cs** so assigned `ProductId` is included in the INSERT statement.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `[Key]` on non-identity column | INSERT omits `ProductId`; row may fail constraints or get wrong key |
| Runtime | `Get<T>(id)` after bad insert | Returns null — key mismatch or no row |
| Concurrency | `MAX(ProductId) + 1` without transaction/sequence | Duplicate keys under concurrent inserts — race even with correct attribute |

**Fix (priority order):**

1. Change to `[ExplicitKey]` on `ProductId` — matches **ContribPreviewRepository** pattern.
2. Replace `MAX+1` with **`SEQUENCE`**, **`IDENTITY`**, or insert inside a **transaction** with **`UPDLOCK, HOLDLOCK`** on the key range — or use a dedicated key table.
3. Verify generated SQL in Profiler: INSERT must list `ProductId` when manually assigned.

**Production takeaway:** Contrib's `[Key]` vs `[ExplicitKey]` distinction is not cosmetic — it controls INSERT shape. Manual key generation still needs database-level uniqueness strategy.

---

#### Q7. (R) A search API builds product filters with string concatenation instead of the chapter's SqlBuilder preview pattern:

```csharp
public IReadOnlyList<ProductRow> SearchProducts(string? category, decimal? minPrice, string sortColumn)
{
    var sql = "SELECT ProductId, ProductName, UnitPrice, StockQuantity FROM dbo.Products WHERE 1=1";
    if (!string.IsNullOrEmpty(category))
        sql += $" AND Category = '{category}'";
    if (minPrice.HasValue)
        sql += $" AND UnitPrice >= {minPrice.Value}";
    sql += $" ORDER BY {sortColumn}";

    using var conn = new SqlConnection(_connectionString);
    return conn.Query<ProductRow>(sql).ToList();
}
```

Security review flags SQL injection on `category` and `sortColumn`. Refactor toward **Dapper.SqlBuilder** (or equivalent) with parameterized fragments. What must never be passed as a raw interpolated string, and how do whitelist + parameters split responsibility?

**Answer:** **User-supplied values** (`category`, prices, ids) must always flow through **Dapper parameters** (`@category`, `@minPrice`). **Identifiers** (column names, sort direction, table names) cannot be parameterized in T-SQL — `sortColumn` must be chosen from a **fixed whitelist**, not concatenated from the request string.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `$" AND Category = '{category}'"` | Classic SQL injection on filter values |
| Security | `$" ORDER BY {sortColumn}"` | Injection via sort field — attacker can inject subqueries or stack statements |
| Maintainability | Ad-hoc string building | Optional clauses error-prone; hard to audit |

**Fix (priority order):**

1. Replace value filters with parameterized fragments — pattern from **SqlBuilderPreview.cs**:

```csharp
var builder = new SqlBuilder();
var template = builder.AddTemplate(
    "SELECT /**select**/ FROM dbo.Products /**where**/ /**orderby**/",
    new { Category = category, MinPrice = minPrice });

builder.Select("ProductId, ProductName, UnitPrice, StockQuantity");
if (!string.IsNullOrEmpty(category))
    builder.Where("Category = @Category", new { Category = category });
if (minPrice.HasValue)
    builder.Where("UnitPrice >= @MinPrice", new { MinPrice = minPrice });

var sort = sortColumn switch
{
    "Name" => "ProductName",
    "Price" => "UnitPrice",
    _ => "ProductId"
};
builder.OrderBy(sort);

return conn.Query<ProductRow>(template.RawSql, template.Parameters).ToList();
```

2. Never whitelist-sort with user raw strings — map `"price"` → `"UnitPrice"` internally.
3. Keep `minPrice` as a parameter even though decimal is less exploitable — plan cache and typing consistency.

**Production takeaway:** Dapper parameters secure **values**; SqlBuilder (or manual fragment lists) secure **optional SQL structure**. Identifier injection is still injection — whitelist columns, parameterize everything else.
