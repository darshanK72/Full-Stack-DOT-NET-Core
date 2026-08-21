# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/02. Dapper/01. Introduction to Dapper/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A junior ports the chapter's `GetActiveProducts` into an ASP.NET Core API. Review the repository method:

```csharp
public IEnumerable<Product> GetActiveProducts()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL;
        """;
    return connection.Query<Product>(sql);
}
```

The controller calls `return Ok(_repo.GetActiveProducts());` and integration tests pass locally. What breaks under load or refactoring, and how would you fix it to match the pattern in this chapter's `ProductRepository`?

---

#### Q2. (R) A search endpoint accepts a product name from the query string. Review:

```csharp
public Product? FindByName(string productName)
{
    using IDbConnection db = new SqlConnection(_connectionString);
    string sql = $"""
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName = '{productName}';
        """;
    return db.QueryFirstOrDefault<Product>(sql);
}
```

What are the problems (security, correctness, maintainability), and what is the Dapper-native fix this chapter previews for `GetById`?

---

#### Q3. (M) Two developers debate connection handling before a code review. Developer A writes every repository method with `using IDbConnection db = new SqlConnection(_cs);` and never calls `Open()`. Developer B opens the connection once in a unit-of-work class and passes the open `IDbConnection` into repository methods that run inside a transaction. Both patterns appear in this chapter's demos (`ConnectionBehaviorDemo`). Under what conditions is each correct, and what state is the connection in after `Query<T>` completes when it started closed?

---

#### Q4. (D) Your team is building an order microservice. Writes (create order, reserve inventory) will use EF Core with migrations. A product catalog read endpoint must return hand-tuned SQL with specific indexes and no change-tracker overhead. A teammate proposes "just use EF Core everywhere for consistency." What do you recommend, how would Dapper fit, and what breaks if you force EF Core on the read path?

---

#### Q5. (R) A legacy `dbo.Products` table uses snake_case column names. A new Dapper DTO maps cleanly in tests against LocalDB seed data, but staging returns rows with empty names and zero prices:

```csharp
public sealed class ProductListItem
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
}

public IReadOnlyList<ProductListItem> GetCatalog()
{
    using IDbConnection db = new SqlConnection(_connectionString);
    const string sql = """
        SELECT product_id, product_name, unit_price
        FROM dbo.Products
        WHERE discontinued_date IS NULL;
        """;
    return db.Query<ProductListItem>(sql).ToList();
}
```

No exceptions are thrown. Diagnose the failure mode and give two production-safe fixes (one SQL-side, one mapping-side for later chapters).

---

#### Q6. (P) You register data access in ASP.NET Core DI for a Dapper-based `ProductRepository` like the one in this chapter. A teammate submits:

```csharp
// Program.cs
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("AdoNetTutorial")!));

// ProductRepository.cs — refactored constructor
public ProductRepository(IDbConnection connection) => _connection = connection;
```

What fails at startup or under concurrent traffic, and how should connection strings, repository lifetime, and `IDbConnection` be wired instead?

---

#### Q7. (R) A developer coming from EF Core "fixes" update logic using Dapper. Review:

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    var product = GetById(productId);          // QueryFirstOrDefault<Product>
    if (product is null) return;
    product = product with { UnitPrice = newPrice };  // record copy — Product is not a record here
    // "Dapper tracks changes like EF" — no Execute call
}

public Product? GetById(int id)
{
    IDbConnection db = new SqlConnection(_connectionString);
    return db.QueryFirstOrDefault<Product>(
        "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products WHERE ProductId = @id",
        new { id });
}
```

Identify compile/runtime/DI issues and explain why the price never persists — tying back to what Dapper actually is in this chapter.
