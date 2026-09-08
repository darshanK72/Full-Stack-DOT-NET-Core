# Chapter 01. Introduction to Dapper — Interview Q&A
> Back to [Dapper Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Chapter 01. Introduction to Dapper](#chapter-01-introduction-to-dapper)
  - [Q1. What is Dapper?](#q1-what-is-dapper)
  - [Q2. What type of library is Dapper (ORM, micro-ORM, or something else)?](#q2-what-type-of-library-is-dapper-orm-micro-orm-or-something-else)
  - [Q3. What problem does Dapper solve compared to raw ADO.NET?](#q3-what-problem-does-dapper-solve-compared-to-raw-adonet)
  - [Q4. What problem does Dapper solve compared to Entity Framework Core?](#q4-what-problem-does-dapper-solve-compared-to-entity-framework-core)
  - [Q5. When would you choose Dapper over EF Core?](#q5-when-would-you-choose-dapper-over-ef-core)
  - [Q6. When would you choose EF Core over Dapper?](#q6-when-would-you-choose-ef-core-over-dapper)
  - [Q7. What are the main advantages and limitations of Dapper?](#q7-what-are-the-main-advantages-and-limitations-of-dapper)
  - [Q8. Does Dapper generate SQL for you?](#q8-does-dapper-generate-sql-for-you)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 01. Introduction to Dapper

---

## Q1. What is Dapper?

**Concepts**
- IDbConnection extension method surface
- ADO.NET abstraction layer
- result-row-to-POCO materialization
- micro-ORM positioning
- provider-agnostic design

**Answer**

Dapper is a lightweight .NET library that extends `IDbConnection` with extension methods for executing SQL and mapping result rows to plain C# objects. It sits directly on top of ADO.NET, which means it works with any ADO.NET provider — SQL Server, PostgreSQL, MySQL, SQLite, and others — without locking you into a specific database. Stack Overflow created and distributes it as the `Dapper` NuGet package. Its key design point is that you retain full control over SQL while it removes the manual `SqlDataReader` boilerplate of creating commands, indexing columns, and copying values property by property. It maps results to classes, structs, value tuples, and dynamic types with minimal configuration, and it intentionally leaves connection lifetime management to you — you open, pool, and dispose connections through the underlying provider.

---

## Q2. What type of library is Dapper (ORM, micro-ORM, or something else)?

**Concepts**
- micro-ORM classification
- object-relational mapping scope boundary
- absence of change tracking and migrations
- contrast with full ORM
- SQL visibility as a design goal

**Answer**

Dapper is a micro-ORM — it maps result rows to objects but deliberately stops short of modeling relationships, tracking entity state across a unit of work, or managing schema migrations. You write the SQL; Dapper handles parameter binding and materialization. A full ORM like Entity Framework Core goes further by generating SQL from LINQ expressions and tracking which properties changed so it can emit targeted updates. Dapper avoids those features by design, keeping its behavior fast and predictable for hand-tuned queries. Teams often describe it as an object mapper rather than a complete persistence framework, and they reach for it when SQL visibility and throughput matter more than convention-based modeling.

---

## Q3. What problem does Dapper solve compared to raw ADO.NET?

**Concepts**
- SqlDataReader column-indexing elimination
- parameter binding from anonymous objects
- command and reader boilerplate reduction
- async surface area
- full ADO.NET feature access retained

**Answer**

Raw ADO.NET requires repetitive ceremony to create commands, add parameters, open readers, and copy column values into object properties row by row. Dapper collapses all of that into a single call like `connection.Query<Product>(sql, param)` while still using your SQL and your connection. It eliminates manual `SqlDataReader` indexing and null-checking for every column, and it binds anonymous objects, `DynamicParameters`, or expando objects to SQL parameters automatically. Async methods like `QueryAsync` and `ExecuteAsync` expose the same concise surface area. Underneath, you retain full access to transactions, stored procedures, and provider-specific features — Dapper simply removes the plumbing.

---

## Q4. What problem does Dapper solve compared to Entity Framework Core?

**Concepts**
- change tracker overhead on reads
- SQL transparency for tuning
- LINQ translation cost
- read-only result set efficiency
- schema management responsibility

**Answer**

Entity Framework Core adds layers — LINQ translation, change tracking, relationship fix-up, and migration infrastructure — that are unnecessary for simple, performance-sensitive read paths. Dapper executes the exact SQL you provide and returns mapped objects with near-ADO.NET overhead. Because you always see the SQL sent to the database, tuning indexes and diagnosing slow queries is straightforward. Removing the change tracker also reduces memory use significantly on large read-only result sets, since EF Core snapshots every property for potential dirty detection. The trade-off is that you lose EF Core's automation for joins, cascade updates, and relationship navigation — every query and data-modification statement becomes your responsibility.

---

## Q5. When would you choose Dapper over EF Core?

**Concepts**
- hot-path throughput requirements
- stored procedure integration
- inefficient LINQ translation scenarios
- read-heavy DTO-only APIs
- DBA-owned SQL management

**Answer**

I choose Dapper when I need hand-written SQL, maximum read throughput, or tight integration with legacy stored procedures where EF Core's LINQ translation adds little value. Hot paths where every round-trip and every millisecond are profiled and fixed at the SQL level benefit from Dapper's minimal overhead. Teams with strong database administrators who prefer stored procedures and tuned statements in source control also tend to prefer Dapper. Scenarios where EF Core would generate inefficient joins, cartesian explosions, or simply cannot translate certain expressions are exactly where Dapper's explicit SQL shines. Read-heavy APIs that only need flat DTOs — no entity graphs, no change tracking — are another natural fit.

---

## Q6. When would you choose EF Core over Dapper?

**Concepts**
- migration-driven schema evolution
- LINQ composability
- change tracking unit of work
- relationship navigation and eager loading
- provider-agnostic query translation

**Answer**

I choose Entity Framework Core when I want convention-based mapping, LINQ composability, automated migrations, and change tracking for typical CRUD workflows. Greenfield applications where C# entity classes should drive schema evolution through migrations are exactly where EF Core excels. Complex object graphs with relationships, eager loading, and automatic fix-up are far easier to manage with EF Core's navigation properties than with hand-written joins. When `SaveChanges` unit-of-work semantics and optimistic concurrency tokens simplify the code compared to writing `UPDATE` SQL per operation, EF Core is the better choice. Provider-agnostic LINQ — writing one query that EF Core translates to SQL Server, PostgreSQL, or SQLite — also avoids maintaining database-specific SQL dialects.

---

## Q7. What are the main advantages and limitations of Dapper?

**Concepts**
- benchmark performance advantage
- minimal API surface
- absence of dirty-entity detection
- manual relationship shaping requirement
- schema drift risk

**Answer**

Dapper's main advantage is speed: it is consistently among the fastest .NET mappers in benchmarks because it avoids the heavy metadata, proxy generation, and change-tracking layers that full ORMs carry. Its API is intentionally small — you learn `Query`, `Execute`, and how to pass parameters, rather than a complete ORM stack. On the limitation side, there is no built-in mechanism to detect dirty entities or batch updates without writing SQL yourself. Multi-table object graphs require manual joins, multi-mapping overloads, or multiple queries that you assemble in code. Schema drift is also not caught at compile time the way a strongly configured EF Core model can signal mismatches through failed migrations or model validation.

---

## Q8. Does Dapper generate SQL for you?

**Concepts**
- explicit SQL requirement
- no LINQ provider or expression tree translator
- Dapper.Contrib optional CRUD helpers
- predictable SQL as a design principle
- INSERT/UPDATE/DELETE require explicit text

**Answer**

Dapper never generates SQL. You supply the complete statement as a string or the name of a stored procedure, and Dapper binds parameters, executes the command through ADO.NET, and maps the result set to your types — nothing more. Inserts, updates, and deletes require explicit `INSERT`, `UPDATE`, or `DELETE` statements. There is no LINQ provider or expression tree translator in Dapper itself. Third-party extensions like Dapper.Contrib offer optional CRUD helpers that emit fixed SQL templates, but even those do not perform dynamic LINQ translation. This is a deliberate design choice: predictable, visible SQL is a feature that distinguishes Dapper from full ORMs.

---

## Gotchas — Introduction to Dapper (Interview Traps)

---

#### Gotcha 1. String interpolation in SQL — Dapper does not prevent injection

**Concepts**
- Dapper extension methods accept raw SQL strings
- no automatic sanitization or parameterization of interpolated strings
- `@placeholder` with anonymous object as the correct pattern
- `$"WHERE Id = {id}"` passed to `QueryAsync` is a critical vulnerability
- injection risk identical to raw ADO.NET string concatenation

**Answer**

Dapper's `QueryAsync(sql, param)` accepts whatever SQL string you pass — it does not inspect, sanitize, or rewrite the string. Interpolating user input directly (`$"WHERE Name = '{name}'"`) creates a SQL injection vulnerability identical to raw ADO.NET string concatenation. Always use `@placeholder` syntax in the SQL string and pass values through the anonymous parameter object: `conn.QueryAsync<Product>("WHERE Name = @name", new { name })`. Dapper binds the object properties to named parameters automatically.

---

#### Gotcha 2. Connection not disposed — connection pool exhaustion

**Concepts**
- `IDbConnection` must be wrapped in `using` or `await using`
- Dapper opens a closed connection automatically but never closes it
- pool slot not returned to pool until Dispose
- GC finalization too slow for production concurrency
- "timeout expired obtaining connection from pool" error

**Answer**

Dapper opens a closed `IDbConnection` automatically before executing, but it never closes or disposes the connection — that is the caller's responsibility. Failing to wrap the connection in `using` or `await using` leaves pool slots checked out until garbage collection, which is too slow under concurrent API traffic. The pool exhausts silently and the error ("timeout expired obtaining connection from pool") appears only under load, passing all single-user local tests. Always use `using IDbConnection conn = new SqlConnection(_cs)` to guarantee disposal.

---

#### Gotcha 3. Returning deferred `IEnumerable<T>` after connection closed

**Concepts**
- Dapper `Query<T>` returns `IEnumerable<T>` via deferred execution
- connection disposal before enumeration causes `ObjectDisposedException`
- `ToList()` / `ToArray()` materializes inside connection scope
- `QueryAsync` same issue — `await` alone does not protect
- `IReadOnlyList<T>` return type signals materialized result

**Answer**

Dapper's buffered `Query<T>` returns an `IEnumerable<T>` that is executed and buffered immediately by default, but if called with `buffered: false` or if the `using` block exits before enumeration, SQL runs after the connection is closed, throwing `ObjectDisposedException` or "connection is closed". Always call `.ToList()` or `.ToArray()` inside the `using` block before returning, and change the return type to `IReadOnlyList<T>` to signal to callers that the sequence is fully materialized. With `QueryAsync`, the same applies — `await` the task and materialize inside the connection scope.

---

#### Gotcha 4. `QuerySingle` throws on zero or more than one row

**Concepts**
- `QuerySingle<T>` strict uniqueness assertion
- throws `InvalidOperationException` on 0 or 2+ rows
- `QueryFirstOrDefault<T>` for optional single-row lookups
- `QueryFirst<T>` for required first-row with multiple possible rows
- unique key invariant as the precondition for `QuerySingle`

**Answer**

`QuerySingle<T>` throws `InvalidOperationException` if the query returns zero rows or more than one row — it asserts exact uniqueness as a domain invariant. Use it only when a unique database constraint guarantees exactly one matching row. For optional lookups (absence is valid), use `QueryFirstOrDefault<T>` which returns `default(T)` when no rows match. For required first-row scenarios where multiple rows may exist, use `QueryFirst<T>`. Using `QuerySingle` where `QueryFirstOrDefault` is correct converts a business condition into a hard exception.

---

#### Gotcha 5. Dynamic type from Dapper — no compile-time safety, runtime `RuntimeBinderException`

**Concepts**
- `Query` without type parameter returns `IEnumerable<dynamic>`
- `dynamic` property access fails at runtime with wrong column name
- typo in column alias causes `RuntimeBinderException`
- no IntelliSense, no rename refactoring for dynamic properties
- strongly-typed `Query<T>` as the correct production pattern

**Answer**

Calling Dapper's `Query()` without a type parameter returns `IEnumerable<dynamic>` — every column becomes a property on a dynamic object. Accessing `row.ProductNme` (a typo) compiles without error but throws `RuntimeBinderException` at runtime when the column is not found. For exploratory or throw-away code this is acceptable, but all production queries should use `Query<T>` with a mapped POCO so that column-property mismatches are caught as mapping failures and typos trigger compile-time errors from the typed property.

---

#### Gotcha 6. `QueryMultiple` — consuming a grid reader twice throws

**Concepts**
- `GridReader` from `QueryMultiple` is forward-only
- each `Read<T>()` call advances to the next result set
- calling `Read<T>()` on an already-consumed grid throws
- grid must be consumed in the same order as SP result sets
- `using` on `GridReader` to dispose after all grids consumed

**Answer**

`GridReader` returned by `QueryMultiple` is forward-only — each call to `Read<T>()` or `ReadAsync<T>()` consumes one result set in order. Calling `Read<T>()` twice on the same result set index, or calling it out of order relative to the stored procedure's SELECT sequence, either throws an `ObjectDisposedException` or silently maps the wrong rows to the wrong type. Always consume grids in the same order as the procedure emits them, wrap `GridReader` in `using`, and never re-read a grid that has already been consumed.

---

#### Gotcha 7. Dapper extension methods on `IDbConnection` — calling on closed connection without auto-open

**Concepts**
- Dapper opens a closed connection automatically before executing
- Dapper does not reopen a `Broken` state connection
- manually pre-opened connection works but requires consistent state
- `connection.Open()` before Dapper calls is redundant but harmless
- broken connection from connection pool returns error on reuse

**Answer**

Dapper automatically calls `Open()` on a closed connection before executing a query and leaves it in the same open/closed state it was when the call began. However, Dapper does not attempt to reopen a connection in a `Broken` state — a connection returned from the pool that has been severed by a network interruption will fail and throw without any retry. For production code, implement a resilience policy (Polly retry) that catches transient connectivity exceptions and retries with a fresh connection rather than reusing the broken one.

---

#### Gotcha 8. Dapper maps by column name — SQL alias required when columns conflict

**Concepts**
- Dapper maps column name to property name (case-insensitive)
- JOIN producing two columns named `Id` — last value wins
- explicit alias required for ambiguous column names in multi-table JOINs
- `AS OrderId` and `AS CustomerId` disambiguation pattern
- silent wrong-value mapping when duplicate column names exist

**Answer**

Dapper maps columns to POCO properties by matching column names case-insensitively. When a query joins two tables and both have a column named `Id`, only the last one in the SELECT list survives — Dapper overwrites the property with each matching column. This produces silently wrong data rather than an exception. Always alias duplicate column names in JOIN queries: `SELECT o.Id AS OrderId, c.Id AS CustomerId, ...` so each target property in the POCO maps to exactly one distinct column name.

---

#### Gotcha 9. No schema migration support in Dapper — requires external tooling

**Concepts**
- Dapper is a micro-ORM with no migration system
- schema changes require external migration tool (DbUp, Flyway, EF migrations)
- Dapper and EF Core migrations can coexist on the same database
- stored SQL scripts as the migration mechanism with Dapper
- production schema drift from manual DBA changes not tracked

**Answer**

Dapper has no migration or schema-management system — it maps SQL results to objects and nothing else. Schema changes must be managed by an external tool such as DbUp, Flyway, or EF Core Migrations (even when Dapper is the primary query layer). Without a migration tool, schema changes applied manually by DBAs are not tracked, making environment synchronization error-prone. Choose a migration tool early in the project and commit all schema changes through it, regardless of which ORM layer runs the application queries.

---

#### Gotcha 10. `DynamicParameters` direction and output parameter value not read until after execution

**Concepts**
- `DynamicParameters.Add` with `direction: ParameterDirection.Output`
- output value not available until after `Execute`/`QueryAsync` completes
- `dp.Get<int>("@OutParam")` to read output value after execution
- forgot to call `Get<T>` and reading stale `null` from object
- `ParameterDirection.ReturnValue` for SP RETURN code

**Answer**

When using `DynamicParameters` for stored procedure output parameters, the output value is populated only after the Dapper method completes execution. Reading `dp.Get<int>("@OutputParam")` before the `ExecuteAsync` or `QueryAsync` call completes returns the default value, not the SP-assigned value. Always call `dp.Get<T>("@ParamName")` after the awaited Dapper call. Also set `direction: ParameterDirection.ReturnValue` for SP `RETURN` codes — they use a distinct direction from `Output` parameters.

---

## Scenario-Based Questions (Karat Format)

---

## Q1. (R) A junior ports `GetActiveProducts` into an ASP.NET Core API. Integration tests pass locally. What breaks under load or refactoring, and how would you fix it?

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

**Concepts**
- undisposed SqlConnection pool leak
- deferred IEnumerable outliving connection
- pool exhaustion under concurrent load
- using block disposal guarantee
- ToList() materialization inside connection scope

**Answer**

The method returns a deferred `IEnumerable<Product>` tied to a connection that is never disposed. The connection stays open and un-pooled until garbage collection reclaims it, leaking a slot from the connection pool on every request. Under concurrent load this exhausts the pool and triggers "timeout expired obtaining connection" errors — a failure that local tests never expose because enumeration happens immediately on the same thread before GC pressure builds.

The fix is to wrap the connection in `using IDbConnection connection = new SqlConnection(_connectionString)` so disposal is guaranteed on any code path, and to call `.ToList()` before returning so the result is fully materialized inside the connection's lifetime. Changing the return type to `IReadOnlyList<Product>` signals to callers that the sequence is already materialized. There is also no need to call `Open()` manually — Dapper opens a closed connection automatically before executing.

```csharp
public IReadOnlyList<Product> GetActiveProducts()
{
    using IDbConnection connection = new SqlConnection(_connectionString);
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL;
        """;
    return connection.Query<Product>(sql).ToList();
}
```

---

## Q2. (R) A search endpoint accepts a product name from the query string. What are the problems — security, correctness, maintainability — and what is the Dapper-native fix?

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

**Concepts**
- SQL injection via string interpolation
- @-placeholder parameterization pattern
- unescaped single-quote correctness failure
- Dapper anonymous object binding
- input validation as defense-in-depth only

**Answer**

Interpolating user input into the SQL string is a critical SQL injection vulnerability — an attacker can append arbitrary SQL after the closing quote to read, modify, or drop data. Beyond security, a legitimate product name containing a single quote — like `O'Brien` — breaks the query with a syntax error. The interpolation also undermines the primary reason to use Dapper, which is parameterized SQL.

The fix is to use a `@productName` placeholder and pass the value as a parameter:

```csharp
public Product? FindByName(string productName)
{
    using IDbConnection db = new SqlConnection(_connectionString);
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName = @productName;
        """;
    return db.QueryFirstOrDefault<Product>(sql, new { productName });
}
```

Dapper only parameterizes values you pass through its param argument — any string concatenated or interpolated into the SQL text before Dapper sees it is not protected. Input length validation at the API boundary adds defense-in-depth but is not a substitute for parameterization.

---

## Q3. (M) Two developers debate connection handling. Developer A uses a short-lived `using IDbConnection` per method and never calls `Open()`. Developer B opens once in a unit-of-work class and passes the open connection into repositories sharing a transaction. Under what conditions is each correct, and what is the connection state after `Query<T>` completes when it started closed?

**Concepts**
- per-operation short-lived connection pattern
- shared connection for transaction scope
- Dapper auto-open on closed connection
- connection state Open after execute
- ownership and disposal responsibility

**Answer**

Developer A's pattern is the default for standalone reads and writes — one short-lived connection per operation, disposed via `using`, which is pool-friendly and the standard pattern for independent queries. Developer B's pattern is correct when multiple commands must share one connection and one transaction; repositories accept the already-open `IDbConnection` instead of creating their own so all operations can commit or roll back together.

When Dapper executes on a closed connection it calls `Open()` before the command, and after `Query<T>` completes the connection state is `Open`. The `using` block's dispose then closes it and returns it to the pool. The wrong hybrid is opening per method inside a transaction scope but disposing between calls — that breaks atomicity. Creating a new connection per call inside a transaction block also fails because each call may autocommit separately unless explicitly enlisted. The deciding factor is transactional scope, not performance preference. Per-request `using` plus `.ToList()` scales cleanly in ASP.NET Core; a shared open connection belongs exclusively inside explicit transaction boundaries.

---

## Q4. (D) Your team builds an order microservice. Writes use EF Core with migrations. A product catalog read endpoint needs hand-tuned SQL with specific indexes and no change-tracker overhead. A teammate proposes using EF Core everywhere for consistency. What do you recommend?

**Concepts**
- hybrid EF Core and Dapper architecture
- write model vs read model tool selection
- change tracker overhead on read paths
- Dapper micro-ORM sweet spot
- consistency through boundaries not single-tool mandate

**Answer**

I recommend using EF Core for the write model and schema evolution while adding Dapper for the catalog read path with explicit SQL. EF Core simplifies order creation and inventory reservation through migrations, relationships, unit-of-work, and change tracking — all appropriate for a write model. The catalog read path has specific SQL, targeted indexes, and projections that Dapper executes with minimal overhead and no tracker allocations. Both paths share the same database and connection string configuration.

Forcing EF Core on the hand-tuned read path means fighting the LINQ translator for exact SQL, adding `AsNoTracking` everywhere, and losing fine-grained control over hints and covering index projections. Teams using EF Core on hot read paths often encounter N+1 issues or over-fetching that require progressively awkward workarounds. Consistency belongs in API contract design, error shapes, and logging conventions — not in forcing one ORM for every access pattern. Document when to reach for EF Core versus Dapper in an Architecture Decision Record so the team applies each tool intentionally.

---

## Q5. (R) A legacy `dbo.Products` table uses snake_case column names. A new Dapper DTO maps cleanly in tests against LocalDB seed data, but staging returns rows with empty names and zero prices. No exceptions are thrown. Diagnose the failure and give two production-safe fixes.

```csharp
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

**Concepts**
- case-insensitive name equality mapping
- underscore vs PascalCase mismatch
- silent default value on unmapped properties
- SQL alias fix
- custom type map for global convention

**Answer**

Dapper maps by case-insensitive name equality. `product_id` may coincidentally match `ProductId` in some environments, but `product_name` does not match `ProductName` and `unit_price` does not match `UnitPrice`, so those properties stay at their default values — empty string and zero — with no exception. The LocalDB seed used PascalCase column names, hiding the mismatch; the staging schema uses legacy snake_case. No error is thrown because Dapper never requires all columns to map.

The SQL-side fix, preferred for reads, is to alias columns to match the property names:

```sql
SELECT product_id AS ProductId,
       product_name AS ProductName,
       unit_price AS UnitPrice
FROM dbo.Products
WHERE discontinued_date IS NULL;
```

The mapping-side fix for a codebase with many queries against the same legacy schema is to register a custom type map using `SqlMapper.SetTypeMap` or a naming convention handler so `product_name` maps globally to `ProductName`. Add integration tests against a schema that mirrors staging, asserting non-default `ProductName` and `UnitPrice` values for seeded rows, so schema drift is caught before production.

---

## Q6. (P) A teammate registers a Dapper `ProductRepository` in ASP.NET Core DI. What fails at startup or under concurrent traffic, and how should connection strings, repository lifetime, and `IDbConnection` be wired instead?

```csharp
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("AdoNetTutorial")!));

public ProductRepository(IDbConnection connection) => _connection = connection;
```

**Concepts**
- singleton SqlConnection thread-safety violation
- captive dependency lifetime mismatch
- connection-string injection pattern
- scoped or transient repository lifetime
- per-method using IDbConnection creation

**Answer**

A singleton `IDbConnection` is not thread-safe for concurrent requests. Multiple threads executing Dapper calls on the same `SqlConnection` instance produce undefined behavior — interleaved commands, timeouts, and corrupted result reads. Even if thread safety were somehow handled, a long-lived connection bypasses connection pooling best practices and breaks the per-operation disposal semantics that make Dapper's pool behavior predictable.

The correct approach is to register `ProductRepository` as scoped (per request) or transient, never singleton, and to not register `IDbConnection` in DI at all for typical per-query usage. Instead, inject `IConfiguration` or an `IOptions<DatabaseOptions>` wrapper that holds the connection string, then create `using IDbConnection db = new SqlConnection(_connectionString)` inside each repository method:

```csharp
builder.Services.AddScoped<ProductRepository>();

public ProductRepository(IOptions<DatabaseOptions> options)
{
    _connectionString = options.Value.AdoNetTutorial
        ?? throw new InvalidOperationException("Connection string missing.");
}
```

For integration tests, inject a connection string pointing at LocalDB. Reserve `IDbConnection` injection only for test doubles where you need to control what the connection returns.

---

## Q7. (R) A developer coming from EF Core writes an update method. Identify compile/runtime/DI issues and explain why the price never persists.

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    var product = GetById(productId);
    if (product is null) return;
    product = product with { UnitPrice = newPrice }; // Product is not a record
    // no Execute call
}

public Product? GetById(int id)
{
    IDbConnection db = new SqlConnection(_connectionString);
    return db.QueryFirstOrDefault<Product>(
        "SELECT ... FROM dbo.Products WHERE ProductId = @id", new { id });
}
```

**Concepts**
- Dapper stateless micro-ORM design
- absence of change tracking
- with expression on non-record type compile error
- explicit UPDATE SQL requirement
- connection leak in GetById

**Answer**

The price never persists because Dapper is a stateless micro-ORM — it maps query rows to POCOs and executes explicit commands, but it has no change tracker. Modifying an in-memory object without calling `Execute` with an UPDATE statement makes no database round-trip at all. There is also a compile error: `product with { UnitPrice = newPrice }` is a `with` expression, which is only valid on `record` types — if `Product` is a sealed class, this does not build (CS8858). Additionally, `GetById` creates a `SqlConnection` without a `using` block, leaking a connection on every lookup.

The fix is to replace the EF mental model with explicit SQL:

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    using IDbConnection db = new SqlConnection(_connectionString);
    const string sql = "UPDATE dbo.Products SET UnitPrice = @newPrice WHERE ProductId = @productId;";
    db.Execute(sql, new { productId, newPrice });
}
```

Fix `GetById` with `using IDbConnection` as well. If a read-then-modify pattern is genuinely required for business logic, wrap the read and update in a transaction on a single open connection to prevent concurrent price updates from racing.
