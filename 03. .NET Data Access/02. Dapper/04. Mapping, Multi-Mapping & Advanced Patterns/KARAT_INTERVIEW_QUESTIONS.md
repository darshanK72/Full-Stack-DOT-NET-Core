# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/02. Dapper/04. Mapping, Multi-Mapping & Advanced Patterns`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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
