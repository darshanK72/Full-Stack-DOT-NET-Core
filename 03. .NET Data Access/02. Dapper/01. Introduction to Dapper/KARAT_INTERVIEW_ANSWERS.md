# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/02. Dapper/01. Introduction to Dapper/`

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

**Answer:** The method returns a deferred `IEnumerable<Product>` tied to a connection that is never disposed and will eventually be closed or garbage-collected while enumeration may still be pending. That causes connection leaks, pool exhaustion under load, and intermittent `ObjectDisposedException` when the serializer enumerates after the connection is gone.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Connection lifetime | Connection opened but never disposed (`using` missing) | Leaked connections; SQL connection pool starvation under traffic |
| Runtime / correctness | Returns lazy `IEnumerable` without materializing inside the connection scope | Enumeration after dispose/close throws or returns partial data |
| Design | Caller owns enumeration timing | Fragile across controllers, middleware, and JSON serialization order |
| Scalability | One leaked connection per request | Production outages when pool max is hit; passes in low-concurrency local tests |

**Fix (priority order):**

1. Wrap the connection in `using IDbConnection connection = new SqlConnection(_connectionString);` so it returns to the pool reliably.
2. Materialize before return: `return connection.Query<Product>(sql).ToList();` — same pattern as this chapter's `ProductRepository.GetActiveProducts`.
3. Change the return type to `IReadOnlyList<Product>` so callers cannot accidentally defer enumeration past the connection lifetime.
4. Do not call `Open()` manually unless sharing an open connection inside a transaction; Dapper opens a closed connection automatically.

**Production takeaway:** Dapper's default `Query<T>` is buffered when you call `ToList()`, which is why the chapter materializes inside the `using` block. Returning raw `IEnumerable<T>` from a disposed connection is one of the most common Dapper production failures — local tests hide it because enumeration often happens immediately on the same thread.

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

**Answer:** Interpolating user input into SQL is a critical SQL-injection vulnerability; Dapper's value is parameterized SQL with anonymous objects (or `DynamicParameters` in later chapters), exactly like `GetById`'s `new { productId }` pattern.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | String interpolation embeds `productName` in SQL | SQL injection — attacker can read, modify, or drop data |
| Correctness | Unescaped quotes in legitimate names (`O'Brien`) break the query | Syntax errors or wrong matches for valid input |
| Maintainability | Dynamic SQL string built by concatenation | Hard to audit, test, and reuse; bypasses Dapper's parameter binding |
| Design | Uses `$"""...'{productName}'..."""` despite Dapper parameter support | Negates a core reason to use Dapper over raw ADO.NET |

**Fix (priority order):**

1. Replace interpolation with a parameter placeholder and anonymous object:

```csharp
const string sql = """
    SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
    FROM dbo.Products
    WHERE ProductName = @productName;
    """;
return db.QueryFirstOrDefault<Product>(sql, new { productName });
```

2. Validate or length-limit `productName` at the API boundary if business rules require it — validation is not a substitute for parameterization.
3. Log and map "not found" at the service layer rather than treating SQL exceptions as control flow.

**Production takeaway:** Dapper does not prevent injection if you inject strings yourself — parameters are mandatory for any user-supplied value. The chapter's `GetById` already shows the correct pattern; search endpoints are the usual place teams regress to concatenation.

---

#### Q3. (M) Two developers debate connection handling before a code review. Developer A writes every repository method with `using IDbConnection db = new SqlConnection(_cs);` and never calls `Open()`. Developer B opens the connection once in a unit-of-work class and passes the open `IDbConnection` into repository methods that run inside a transaction. Both patterns appear in this chapter's demos (`ConnectionBehaviorDemo`). Under what conditions is each correct, and what state is the connection in after `Query<T>` completes when it started closed?

**Answer:** Developer A's pattern is the default for standalone reads and writes — one short-lived connection per operation, disposed via `using`, which is pool-friendly and matches this chapter's `ProductRepository`. Developer B's pattern is correct when multiple commands must share one connection and one transaction; repositories accept the already-open `IDbConnection` instead of creating their own.

- **Developer A (closed start, per-method `using`):** Correct for independent queries with no shared transaction. Dapper opens a closed connection before executing. After `Query<T>` completes, the connection is **Open** (same ADO.NET command behavior noted in `ConnectionBehaviorDemo`). The `using` dispose closes it and returns it to the pool.
- **Developer B (caller-opened, shared connection):** Correct inside `IDbTransaction` or a unit-of-work where several `Execute`/`Query` calls must commit or roll back together. Repositories must **not** dispose a connection they did not create — only the owner disposes.
- **Wrong hybrid:** Opening per method inside a transaction scope but disposing between calls — breaks atomicity. Creating a new connection per call inside a transaction block — each call may auto-commit separately unless enlisted correctly.

**Production takeaway:** `ConnectionBehaviorDemo` shows both states are valid entry points; the decision is transactional scope, not performance superstition. Per-request `using` + `ToList()` scales cleanly in ASP.NET Core; shared open connections belong inside explicit transaction boundaries (covered further in ADO.NET ch06 and Dapper ch02 async/transaction patterns).

---

#### Q4. (D) Your team is building an order microservice. Writes (create order, reserve inventory) will use EF Core with migrations. A product catalog read endpoint must return hand-tuned SQL with specific indexes and no change-tracker overhead. A teammate proposes "just use EF Core everywhere for consistency." What do you recommend, how would Dapper fit, and what breaks if you force EF Core on the read path?

**Answer:** Use EF Core for the write model and schema evolution; add Dapper for the catalog read path with explicit SQL — the hybrid pattern described in `DapperConcepts.ExplainStackComparison`. Forcing EF Core on hand-tuned reads sacrifices control and adds change-tracker overhead without benefit.

- **EF Core for writes:** Migrations, relationships, unit-of-work, and change tracking simplify order creation and inventory reservation — the team's existing choice is sound.
- **Dapper for catalog reads:** Inject the connection string (or factory), implement a thin repository like this chapter's `ProductRepository`, and keep SQL with the exact indexes and projections the read SLA requires. No tracker, minimal allocations — Dapper's micro-ORM sweet spot.
- **Shared infrastructure:** Same database, same connection string configuration; optional read replica connection string for reporting/catalog if scale demands it.
- **What breaks with EF-only reads:** LINQ may emit suboptimal SQL; `AsNoTracking` helps but you still lack fine-grained control over hints, covering indexes, and projections; teams often fight N+1 or over-fetching on hot paths.
- **Consistency argument:** Consistency belongs in boundaries (DTO contracts, error shape, logging), not in forcing one ORM for every access pattern. Document when to reach for EF vs Dapper in the repo README or ADR.

**Production takeaway:** Many production systems combine EF Core and Dapper on one database — this chapter's comparison table is the decision framework Karat expects, not a single-tool mandate.

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

**Answer:** Dapper maps by column name to property name (case-insensitive by default). `product_id` may map to `ProductId`, but `product_name` and `unit_price` do not match `ProductName` and `UnitPrice`, so those properties stay at default values — silent data loss with no exception, exactly the convention-mapping gotcha in `Models/Product.cs` and the chapter quick reference.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Mapping / correctness | Snake_case columns vs PascalCase properties | `ProductName` empty, `UnitPrice` 0 — API returns garbage data |
| Observability | No exception on mismatch | Bug reaches production; hard to spot without assertion tests |
| Environment | LocalDB seed used PascalCase columns; staging legacy schema differs | "Works on my machine" — schema drift between environments |
| Design | Assumes convention mapping without verifying column names | Fragile when integrating legacy databases |

**Fix (priority order):**

1. **SQL-side (preferred for reads):** Alias columns to match properties:

```sql
SELECT product_id AS ProductId,
       product_name AS ProductName,
       unit_price AS UnitPrice
FROM dbo.Products
WHERE discontinued_date IS NULL;
```

2. **Mapping-side (later chapter):** Custom column maps or underscore matching rules in Dapper ch04 — register a map so `product_name` → `ProductName` globally for legacy schemas.
3. Add integration tests against a schema that mirrors staging, asserting non-default `ProductName` and `UnitPrice` for seeded rows.
4. Optionally project to a dynamic or intermediate type during migration — short-term bridge only.

**Production takeaway:** Dapper's silent default on unmapped columns is worse than a thrown exception — always verify column/property alignment when connecting to legacy schemas. SQL aliases are the fastest production fix; custom maps pay off when many queries hit the same legacy naming.

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

**Answer:** A singleton `IDbConnection` is not thread-safe for concurrent requests — multiple threads executing Dapper calls on one shared `SqlConnection` cause undefined behavior, timeouts, and corrupted reads. Even if thread safety were solved, a long-lived connection bypasses pooling best practices and breaks per-operation dispose semantics this chapter teaches.

- **Singleton `SqlConnection`:** SQL Server connections are not designed for concurrent multi-threaded use on one instance. Under parallel API requests, queries interleave on shared state — intermittent failures that load tests expose immediately.
- **Singleton repository holding connection:** Same captive dependency problem — repository lifetime extends connection lifetime across all users and requests.
- **Missing dispose:** A singleton connection is never disposed until shutdown; if it drops, the whole app shares one broken connection.
- **Correct lifetime:** Register `ProductRepository` as **scoped** (per request) or **transient**; do **not** register `IDbConnection` in DI for typical per-query usage.
- **Correct wiring:** Inject `IConfiguration` or `IOptions<ConnectionStrings>` (or a small `IDbConnectionFactory`) and create `using IDbConnection db = new SqlConnection(_cs)` inside each method — identical to this chapter's repository. For tests, inject a connection string pointing at LocalDB or accept `IDbConnection` only in test doubles.

```csharp
builder.Services.AddScoped<ProductRepository>();

public ProductRepository(IOptions<DatabaseOptions> options)
{
    _connectionString = options.Value.AdoNetTutorial
        ?? throw new InvalidOperationException("Connection string missing.");
}
```

**Production takeaway:** Dapper's simplicity is "create connection, query, dispose per operation." DI should inject **configuration**, not a shared connection — the chapter's constructor-stores-connection-string pattern scales correctly into ASP.NET Core.

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

**Answer:** Dapper is a stateless micro-ORM — it maps query rows to POCOs and executes explicit commands; there is no change tracker, so modifying an in-memory object without an `Execute`/`UPDATE` never touches the database. The snippet also fails to compile (`with` on a non-record) and leaks connections in `GetById`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `product with { ... }` on `Product` sealed class (not `record`) | CS8858 — does not build |
| Correctness | No `Execute`/`UPDATE` after in-memory mutation | Price never persisted — functional bug invisible in unit tests that mock the repo |
| Design | Assumes EF-style change tracking | Violates Dapper's model — queries are read-only projections unless followed by explicit SQL |
| Connection lifetime | `GetById` creates `SqlConnection` without `using` | Connection leak on every read |
| Maintainability | Read-modify-write without transaction | Race conditions under concurrent price updates even after adding `Execute` |

**Fix (priority order):**

1. Replace the EF mental model with an explicit update:

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    using IDbConnection db = new SqlConnection(_connectionString);
    const string sql = "UPDATE dbo.Products SET UnitPrice = @newPrice WHERE ProductId = @productId;";
    db.Execute(sql, new { productId, newPrice });
}
```

2. Fix `GetById` with `using IDbConnection` — same as this chapter's implementation.
3. If read-modify-write is required (validation rules on current row), wrap read + update in a transaction on one open connection.
4. On the mutation path, mutate properties directly if needed (`product.UnitPrice = newPrice` won't compile with `init` — another signal the EF pattern was copied blindly); prefer parameter-only updates without loading the entity when possible.

**Production takeaway:** The chapter quick reference lists "Expect EF-style change tracking" as a common mistake — Dapper returns disconnected POCOs. Updates require explicit `Execute` SQL; that is a feature (predictable SQL, no hidden round-trips), not a missing feature.
