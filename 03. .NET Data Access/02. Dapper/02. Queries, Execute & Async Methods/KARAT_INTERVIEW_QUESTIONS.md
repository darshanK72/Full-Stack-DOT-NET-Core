# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/02. Dapper/02. Queries, Execute & Async Methods`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A catalog API exposes "get product by category." A teammate ships this repository method. Review it — what breaks in production as the catalog grows, and what Dapper API would you use instead?

```csharp
public Product GetByCategory(SqlConnection connection, int categoryId)
{
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE CategoryId = @categoryId;
        """;

    return connection.QuerySingle<Product>(sql, new { categoryId });
}
```

---

#### Q2. (R) An export endpoint needs to stream millions of product rows with minimal memory. A developer refactors the repository like this. Review the full path — what fails at runtime, and why does the bug pass unit tests that mock `IEnumerable<Product>`?

```csharp
public IEnumerable<Product> StreamAllActive()
{
    using var connection = new SqlConnection(_connectionString);
    connection.Open();
    return connection.Query<Product>(
        """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL;
        """,
        buffered: false);
}

// Controller
public IActionResult Export()
{
    var rows = _repository.StreamAllActive();
    return Ok(rows.Select(p => MapToDto(p))); // deferred LINQ over deferred Dapper query
}
```

---

#### Q3. (R) A health-check endpoint reports inventory count. Review this action — identify compile-time, runtime, and scalability issues:

```csharp
[HttpGet("inventory/count")]
public IActionResult GetActiveProductCount()
{
    using var connection = new SqlConnection(_configuration.GetConnectionString("AdoNetTutorial"));
    connection.Open();
    int count = connection.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM dbo.Products WHERE DiscontinuedDate IS NULL;").Result;
    return Ok(new { count });
}
```

---

#### Q4. (M) Two junior developers argue about the right Dapper call for these operations. For each snippet, name the **correct** Dapper method (`Query`, `QueryFirst`, `Execute`, or `ExecuteScalar`) and what goes wrong if they ship as written:

```csharp
// A — needs rows affected after UPDATE
int changed = (int)connection.ExecuteScalar(
    "UPDATE dbo.Products SET StockQuantity = @qty WHERE ProductId = @id;",
    new { id = 1, qty = 10 });

// B — needs a single aggregate value
decimal avg = connection.Query<decimal>(
    "SELECT AVG(UnitPrice) FROM dbo.Products WHERE DiscontinuedDate IS NULL;")
    .First();

// C — needs to confirm INSERT succeeded (1 row)
var result = connection.Query<int>(
    """
    INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity)
    VALUES (@id, @name, @price, @stock);
    """,
    new { id = 99, name = "Cable", price = 9.99m, stock = 50 });
bool inserted = result.Any();
```

---

#### Q5. (P) Your ASP.NET Core API uses scoped `SqlConnection` per request. A repository method mirrors the tutorial's async list load but returns `IEnumerable<Product>` directly from `QueryAsync`. Under load, callers intermittently see `InvalidOperationException` ("There is already an open DataReader…"). Explain the mechanism and show the production-safe pattern (including when to materialize vs stream).

```csharp
public async Task<IEnumerable<Product>> GetAllActiveAsync(SqlConnection connection)
{
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL
        ORDER BY ProductName;
        """;

    return await connection.QueryAsync<Product>(sql);
}
```

---

#### Q6. (D) The tutorial's `InsertProductReturningId` uses `MAX(ProductId) + 1` inside a batch and returns the new id via `ExecuteScalar<int>`. A teammate says "works in dev, ship it." Two API instances insert products concurrently under load. What breaks, and what pattern replaces both the id generation **and** the Dapper call sequence?

```csharp
const string sql = """
    DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
    INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
    VALUES (@NextId, @productName, @unitPrice, @stockQuantity, NULL);
    SELECT @NextId;
    """;
int newId = connection.ExecuteScalar<int>(sql, new { productName, unitPrice, stockQuantity });
```

---

#### Q7. (P) A batch job must insert a product **and** read the new `ProductId` in one round trip, then update a related audit row — all-or-nothing. The developer runs two separate Dapper calls on the same open connection without an explicit transaction. When does that silently corrupt data, and how do you wire `Execute` / `ExecuteScalar` correctly with `IDbTransaction`?
