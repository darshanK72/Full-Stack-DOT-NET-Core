# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/02. Dapper/03. Parameters, Stored Procedures & QueryMultiple`
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A junior developer ships a product search endpoint backed by this Dapper repository method. Security review flags it before deploy. What is wrong, and how do you fix it without changing the public method signature?

```csharp
public IEnumerable<Product> SearchByName(string searchTerm)
{
    using var connection = new SqlConnection(_connectionString);
    var sql = $"""
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName LIKE '%{searchTerm}%'
        ORDER BY ProductName;
        """;
    return connection.Query<Product>(sql);
}
```

---

#### Q2. (R) Code review on a stored-procedure insert path that mirrors `DynamicParameterRepository.InsertProduct`. The author claims it works in a one-off SSMS test. What breaks at runtime or in production, and in what order do you fix it?

```csharp
public int InsertProduct(string productName, decimal unitPrice, int stockQuantity)
{
    using var connection = new SqlConnection(_connectionString);
    var dp = new DynamicParameters();
    dp.Add("@ProductName", productName);
    dp.Add("@UnitPrice", unitPrice);
    dp.Add("@StockQuantity", stockQuantity);
    dp.Add("@NewProductId", direction: ParameterDirection.Output);

    int newId = dp.Get<int>("@NewProductId"); // read identity before execute

    connection.Execute(
        "dbo.usp_InsertProduct",
        dp,
        commandType: CommandType.StoredProcedure);

    return newId;
}
```

---

#### Q3. (R) A dashboard API calls `usp_GetProductDashboard` but returns wrong totals after a DBA reorders the SELECT statements inside the procedure. Review this consumer — what fails silently vs throws, and how do you harden it?

```csharp
public DashboardDto GetDashboard()
{
    using var connection = new SqlConnection(_connectionString);
    using var multi = connection.QueryMultiple(
        "dbo.usp_GetProductDashboard",
        commandType: CommandType.StoredProcedure);

    int totalCount = multi.Read<int>().Single();           // expects COUNT(*) first
    var products = multi.Read<Product>().AsList();         // expects product rows second
    string topName = multi.ReadFirst<string>();            // expects TOP 1 name third

    return new DashboardDto(products, totalCount, topName);
}
```

---

#### Q4. (M) A filter query returns every row instead of the intended price band. The SQL and anonymous object look correct at a glance. What is the binding bug?

```csharp
const string sql = """
    SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
    FROM dbo.Products
    WHERE UnitPrice >= @MinPrice AND UnitPrice <= @MaxPrice
    ORDER BY UnitPrice;
    """;

using var connection = new SqlConnection(_connectionString);
return connection.Query<Product>(sql, new { MinimumPrice = minPrice, MaximumPrice = maxPrice });
```

---

#### Q5. (P) An ASP.NET Core endpoint wraps a long-running report stored procedure. The controller passes `HttpContext.RequestAborted` as cancellation. Review the repository — what is missing for timeout and cooperative cancellation, and how would you wire `CommandDefinition` correctly?

```csharp
public async Task<IReadOnlyList<Product>> GetSlowReportAsync(CancellationToken cancellationToken)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    return (await connection.QueryAsync<Product>(
        "dbo.usp_SlowProductReport",
        commandType: CommandType.StoredProcedure)).AsList();
}
```

---

#### Q6. (D) A teammate refactors `GetProductCountViaReturnValue` to use an anonymous object because "ReturnValue is just another int param." They change the proc caller to:

```csharp
var count = connection.ExecuteScalar<int>(
    "dbo.usp_GetProductCount",
    commandType: CommandType.StoredProcedure);
```

Separately, another method tries to read a T-SQL `RETURN` with:

```csharp
var dp = new DynamicParameters(new { ProductId = id });
dp.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_TryGetProductName", dp, commandType: CommandType.StoredProcedure);
int status = dp.Get<int>("@Status"); // proc uses RETURN 0 / RETURN 1, not OUTPUT @Status
```

Explain what each approach gets wrong about T-SQL `RETURN` vs `OUTPUT`, and what Dapper API you use for each.

---

#### Q7. (R) A batch import service loads a dashboard in one round trip but intermittently throws `InvalidOperationException` under load. Review the method — identify lifetime/reader issues and the fix.

```csharp
public (IEnumerable<Product> Products, int Total, string TopName) GetDashboardLazy()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();

    var multi = connection.QueryMultiple(
        """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products;
        SELECT COUNT(*) FROM dbo.Products;
        SELECT TOP 1 ProductName FROM dbo.Products ORDER BY UnitPrice DESC;
        """);

    var products = multi.Read<Product>();  // deferred IEnumerable — connection still open
    int total = multi.Read<int>().Single();
    string top = multi.ReadFirst<string>();

    return (products, total, top); // caller enumerates products after method returns
}
```
