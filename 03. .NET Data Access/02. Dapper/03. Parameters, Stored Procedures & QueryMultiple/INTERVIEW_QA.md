# Chapter 03. Parameters, Stored Procedures & QueryMultiple — Interview Q&A
> Back to [Dapper Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Chapter 03. Parameters, Stored Procedures & QueryMultiple](#chapter-03-parameters-stored-procedures--querymultiple)
  - [Q1. How do you pass parameters to a Dapper query?](#q1-how-do-you-pass-parameters-to-a-dapper-query)
  - [Q2. How does Dapper prevent SQL injection?](#q2-how-does-dapper-prevent-sql-injection)
  - [Q3. How do you call a stored procedure with Dapper?](#q3-how-do-you-call-a-stored-procedure-with-dapper)
  - [Q4. What is `QueryMultiple`, and when is it used?](#q4-what-is-querymultiple-and-when-is-it-used)
  - [Q5. How do you read multiple result sets from `QueryMultiple`?](#q5-how-do-you-read-multiple-result-sets-from-querymultiple)
  - [Q6. When would you prefer `QueryMultiple` over separate round-trips?](#q6-when-would-you-prefer-querymultiple-over-separate-round-trips)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 03. Parameters, Stored Procedures & QueryMultiple

---

## Q1. How do you pass parameters to a Dapper query?

**Concepts**
- anonymous object parameter binding
- DynamicParameters for output and typed params
- case-insensitive name matching
- @-prefixed SQL placeholders
- property-to-parameter alignment

**Answer**

I pass parameters as the second argument to any Dapper method using an anonymous object, a `DynamicParameters` bag, a `Dictionary<string, object>`, or any object whose public properties match `@Name` placeholders in the SQL. Dapper maps property names to SQL parameter names case-insensitively. An anonymous object like `new { Id = 42, Name = "Widget" }` binds `@Id` and `@Name` without any additional configuration. `DynamicParameters` supports output parameters, table-valued parameters, and explicit database types for cases where the anonymous object pattern is insufficient. Property names must align with the SQL placeholder names exactly — `UserId` maps to `@UserId`, not `@user_id`, unless you alias the column or use a custom convention.

---

## Q2. How does Dapper prevent SQL injection?

**Concepts**
- ADO.NET parameterized execution
- user values separated from SQL text
- @-placeholder binding safety
- string interpolation as injection vector
- DynamicParameters also parameterizes

**Answer**

Dapper prevents SQL injection by sending user values as ADO.NET parameters separate from the SQL text, so input is never interpreted as executable SQL syntax by the database engine. As long as you use `@placeholders` with a parameter object and do not concatenate user input into the SQL string, injection is prevented — `"SELECT * FROM Users WHERE Email = @Email", new { Email = userInput }` is safe. The unsafe pattern is `$"SELECT * FROM Users WHERE Email = '{userInput}'"` — Dapper cannot protect against injected strings because it never sees user input embedded that way; it only parameterizes the values you pass in the param argument. `DynamicParameters.Add("Email", value)` still produces a proper parameter even when building SQL dynamically with a fixed structure. Stored procedure names should always be fixed string literals; only parameter values should come from user input.

---

## Q3. How do you call a stored procedure with Dapper?

**Concepts**
- CommandType.StoredProcedure flag
- procedure name as SQL argument
- DynamicParameters for output and return values
- Execute for non-query procedures
- result type matching from procedure output

**Answer**

I set `commandType: CommandType.StoredProcedure` and pass the procedure name as the SQL argument, with parameters bound the same way as ad hoc queries. For example: `connection.Query<Product>("usp_GetProductsByCategory", new { CategoryId = 5 }, commandType: CommandType.StoredProcedure)`. Output and return-value parameters require `DynamicParameters` with `ParameterDirection.Output` or `ParameterDirection.ReturnValue`. `Execute` and `ExecuteAsync` work for non-query procedures that do not return rowsets. The result shape must still match the mapped type's properties — Dapper uses the column names from the procedure's result set to bind, so the usual naming rules apply.

---

## Q4. What is `QueryMultiple`, and when is it used?

**Concepts**
- multi-result-set batch execution
- GridReader sequential consumption
- single round-trip for related datasets
- network latency reduction
- prompt GridReader disposal

**Answer**

`QueryMultiple` executes one batch or stored procedure that returns multiple result grids and exposes them through a `GridReader`. I use it when a single round-trip should return related datasets — for example, a header row plus detail lines — rather than two separate queries. It reduces network latency by combining multiple `SELECT` statements or a multi-result procedure into one database call. It returns `SqlMapper.GridReader`, which I hold open with `using var multi = connection.QueryMultiple(...)` while reading each set in order. Result sets are consumed sequentially — you cannot skip ahead — and I dispose the `GridReader` promptly to release the underlying reader and make the connection available for reuse.

---

## Q5. How do you read multiple result sets from `QueryMultiple`?

**Concepts**
- sequential Read calls per result set
- forward-only GridReader consumption
- result-set order dependency
- materialization before GridReader disposal
- ReadAsync equivalent

**Answer**

After `QueryMultiple`, I call `Read<T>()`, `ReadFirst<T>()`, or `ReadFirstOrDefault<T>()` on the `GridReader` once per result set, in the order the database returns them. The first `multi.Read<Order>()` consumes the first result set; the second `multi.Read<OrderLine>()` consumes the second. A mismatch between read order and SQL result order causes wrong-type mapping or empty sequences — the reader is forward-only and there is no way to rewind. I materialize each `Read` with `.ToList()` if I need the data after disposing the `GridReader`. The async equivalent is `QueryMultipleAsync` followed by `ReadAsync<T>()`.

---

## Q6. When would you prefer `QueryMultiple` over separate round-trips?

**Concepts**
- latency amplification on high-latency links
- always-needed related datasets
- parallel independent queries as alternative
- large second result set streaming consideration
- optional vs required data trade-off

**Answer**

I prefer `QueryMultiple` when two or more result sets are always needed together and combining them saves measurable latency, especially over high-latency network links to cloud databases. Dashboard endpoints that always load a summary alongside detail rows in one stored procedure are a natural fit. Separate round-trips are simpler when result sets are optional, independently cacheable, or can be fetched in parallel on different connections when the database supports concurrent execution. Very large second result sets may be better streamed in a dedicated query rather than held behind the first grid reader while it is being processed.

---

## Gotchas — Dapper Parameters, Stored Procedures & QueryMultiple (Interview Traps)

---

#### Gotcha 1. String interpolation in SQL bypasses Dapper parameterization

**Concepts**
- Dapper accepts raw SQL strings — no interpolation safety
- `$"LIKE '%{term}%'"` passed to `QueryAsync` is injection vulnerability
- `@Pattern` with C#-side wildcard composition as safe alternative
- `DynamicParameters` for programmatic parameter construction
- code review must flag all interpolated SQL strings as blocking defects

**Answer**

Dapper passes the SQL string directly to the database without inspecting or rewriting it — interpolating a user-supplied value into the string creates a SQL injection vulnerability identical to raw ADO.NET concatenation. The safe pattern for a LIKE predicate is to compose the wildcard in C# and pass it as a parameter: `new { Pattern = $"%{term}%" }` with `WHERE ProductName LIKE @Pattern`. Always build SQL with static `@placeholder` strings and supply values through Dapper's parameter object, never through string interpolation.

---

#### Gotcha 2. Stored procedure called without `CommandType.StoredProcedure` — treated as text

**Concepts**
- Dapper default command type is `CommandType.Text`
- SP name as text is sent as `EXEC dbo.ProcName` ad-hoc batch
- `CommandType.StoredProcedure` required for named SP binding
- `commandType: CommandType.StoredProcedure` Dapper parameter
- plan reuse: RPC vs ad-hoc batch plan cache difference

**Answer**

Dapper's default command type is `CommandType.Text`. Passing a stored procedure name to `QueryAsync` without specifying `commandType: CommandType.StoredProcedure` causes Dapper to send the name as a literal SQL string, which SQL Server interprets as an invalid statement rather than an RPC call. Always pass `commandType: CommandType.StoredProcedure` when calling stored procedures: `conn.QueryAsync<Product>("dbo.usp_GetProducts", param, commandType: CommandType.StoredProcedure)`. Omitting it causes a `SqlException: syntax error near 'dbo'`.

---

#### Gotcha 3. Output parameter value not available until after `Execute` completes

**Concepts**
- `DynamicParameters.Get<T>` must be called after execution
- value is `default(T)` or `DBNull` before execution completes
- async methods must be `await`ed before reading output values
- `ParameterDirection.Output` vs `ParameterDirection.ReturnValue`
- output parameter size must be set for string types

**Answer**

When using `DynamicParameters` for stored procedure output parameters, the output value is populated only after the Dapper execution call completes. Reading `dp.Get<int>("@OutParam")` before `await conn.ExecuteAsync(...)` returns the initial default, not the SP-assigned value. Always `await` the execution call fully before reading any output values via `dp.Get<T>()`. Also remember that RETURN code values require `ParameterDirection.ReturnValue`, not `Output`, and string outputs require an explicit `size` argument to prevent truncation at zero length.

---

#### Gotcha 4. `QueryMultiple` grid consumed in wrong order — wrong type mapped to wrong rows

**Concepts**
- `GridReader.Read<T>()` advances forward through result sets in SP order
- calling `Read<Orders>()` when next set is customers maps wrong rows
- grid is forward-only — cannot rewind to re-read a consumed set
- result set order in SP must match `Read<T>()` call order in C#
- no error thrown for type mismatch — silent wrong mapping

**Answer**

`GridReader` from `QueryMultiple` is strictly forward-only — each `Read<T>()` call consumes the next result set in the order the stored procedure emitted them. If the procedure emits customers first and orders second, but the C# code calls `grid.Read<Order>()` then `grid.Read<Customer>()`, customer rows are silently mapped to `Order` and order rows to `Customer` with no exception. Always document the SP result set sequence and write the `Read<T>()` calls in the exact matching order.

---

#### Gotcha 5. `QueryMultiple` connection must stay open until all grids consumed

**Concepts**
- `GridReader` reads lazily from the underlying connection
- disposing connection before consuming all grids throws
- `using` connection scope must wrap entire `QueryMultiple` call and grid consumption
- `GridReader` itself must be disposed after all grids consumed
- nested `using` pattern: outer connection, inner grid reader

**Answer**

`GridReader` reads rows from the live database connection lazily — the connection must remain open until all grids are fully consumed. Disposing the connection before calling `Read<T>()` on all result sets throws `ObjectDisposedException` or "connection is closed." The correct pattern is a nested `using`: the outer `using` wraps the connection, and an inner `using` wraps the `GridReader`. The grid reader must be disposed explicitly (or via `using`) to close the result set and release any server-side resources.

---

#### Gotcha 6. `DynamicParameters.Add` with wrong `DbType` — implicit conversion and plan pollution

**Concepts**
- `DbType` inferred from value when not specified
- `DateTime` inferred as `DbType.DateTime` — may truncate time on `date` columns
- `decimal` inferred without precision/scale — rounding on money columns
- explicit `dbType` argument for type-sensitive parameters
- plan cache pollution from mismatched parameter types

**Answer**

When using `DynamicParameters.Add("@StartDate", dateValue)` without specifying `dbType`, Dapper infers the SQL type from the CLR value, which may not match the column type exactly. A `DateTime` column may be a SQL `date` (date-only) or `datetime2` — wrong inference causes implicit conversion that prevents an index seek and truncates or rounds the value. Always pass the explicit `dbType: DbType.Date` or `dbType: DbType.Decimal` with appropriate precision to match the column definition and avoid type mismatch plan pollution.

---

#### Gotcha 7. `DynamicParameters` size not set for string output parameters — truncation at 0

**Concepts**
- output string parameter defaults to size 0 without explicit `size`
- truncation: output value populated but trimmed to empty string
- `size` argument in `DynamicParameters.Add` for string outputs
- no error thrown for size mismatch — silent empty value
- always specify `size` matching SP's declared parameter length

**Answer**

`DynamicParameters.Add("@ResultName", direction: ParameterDirection.Output)` without a `size` argument defaults the parameter size to 0 for string types, causing the output value to arrive truncated to an empty string. SQL Server writes the full value to the parameter, but the client-side buffer is too small to receive it. Always specify `size: 200` (or the actual SP parameter length) for any output or input/output string parameter: `dp.Add("@ResultName", dbType: DbType.String, direction: ParameterDirection.Output, size: 200)`.

---

#### Gotcha 8. Calling stored procedure with `NOCOUNT ON` — `Execute` returns -1 for rows affected

**Concepts**
- `SET NOCOUNT ON` suppresses row-count TDS messages
- Dapper `Execute`/`ExecuteAsync` returns -1 when NOCOUNT active
- rows-affected check for missing-row detection breaks
- output parameter as reliable row count alternative
- common DBA practice breaking application row-count logic

**Answer**

Many stored procedures begin with `SET NOCOUNT ON` to reduce TDS traffic. With `NOCOUNT ON`, Dapper's `ExecuteAsync` returns `-1` regardless of how many rows were affected. Application code that checks `if (affected == 0) throw new NotFoundException()` will never fire for such procedures, silently treating a successful insert or update as a success even when the WHERE clause matched zero rows. Use an output parameter explicitly set to `@@ROWCOUNT` inside the procedure to get a reliable row count when `NOCOUNT ON` is active.

---

#### Gotcha 9. Passing `IEnumerable<T>` list parameter — parameter count limit for large lists

**Concepts**
- Dapper expands `IEnumerable<T>` to individual `@p0, @p1, ...` parameters
- SQL Server 2100-parameter limit per batch
- large `IN` clause exceeds parameter limit — `SqlException`
- table-valued parameter (TVP) or temp-table for large lists
- Dapper does not warn at compile time about list size

**Answer**

When an anonymous parameter object has an `IEnumerable<T>` property (e.g., `new { Ids = idList }`), Dapper expands it to individual numbered parameters in an `IN (@p0, @p1, ...)` clause. SQL Server has a hard limit of 2,100 parameters per batch — exceeding it throws `SqlException: The query has too many parameters`. For lists larger than a few hundred items, use a table-valued parameter (TVP) with a user-defined table type, a temporary table, or break the list into batches of safe size before calling Dapper.

---

#### Gotcha 10. Reusing `DynamicParameters` object across multiple calls — stale parameters accumulate

**Concepts**
- `DynamicParameters` instance is mutable and accumulates added parameters
- reusing across calls adds duplicate parameter names — throws or uses wrong value
- create new `DynamicParameters` per execution
- parameter name collision silent on re-add with same name
- DynamicParameters as a per-call object, not a session-level object

**Answer**

`DynamicParameters` is a mutable object that accumulates all parameters added to it across multiple `Add` calls. If the same instance is reused across two stored procedure calls, the second call inherits all parameters from the first, causing duplicate parameter names or stale values being passed to the wrong procedure. Always create a new `DynamicParameters` instance per execution: `var dp = new DynamicParameters(); dp.Add(...)` just before the Dapper call, and never reuse a `DynamicParameters` instance from a previous call.

---

## Scenario-Based Questions (Karat Format)

---

## Q1. (R) A junior developer ships a product search endpoint. Security review flags it before deploy. What is wrong, and how do you fix it without changing the public method signature?

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

**Concepts**
- LIKE clause SQL injection via interpolation
- @Pattern parameter for LIKE predicate
- C#-side wildcard composition
- deferred IEnumerable without materialization
- parameter value vs SQL structure distinction

**Answer**

The SQL is built with C# string interpolation, so `searchTerm` is concatenated into the command text. An attacker can inject arbitrary SQL — `'; DROP TABLE dbo.Products; --` — to read, modify, or destroy data. Dapper parameterizes only what you pass through its param argument; interpolated strings bypass the parameter channel entirely. The method also returns a deferred `IEnumerable<Product>` from inside a `using` block, which will fail if the caller enumerates after disposal.

The fix uses a `@Pattern` placeholder and composes the wildcard in C# before passing it as a parameter value:

```csharp
public IReadOnlyList<Product> SearchByName(string searchTerm)
{
    using var connection = new SqlConnection(_connectionString);
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName LIKE @Pattern
        ORDER BY ProductName;
        """;
    return connection.Query<Product>(sql, new { Pattern = $"%{searchTerm}%" }).ToList();
}
```

The wildcard characters `%` are part of the parameter value, not SQL syntax, so they are fully parameterized. Input length validation at the API layer adds defense-in-depth but is not a substitute.

---

## Q2. (R) Code review on a stored-procedure insert path. The author claims it works in a one-off SSMS test. What breaks at runtime or in production?

```csharp
public int InsertProduct(string productName, decimal unitPrice, int stockQuantity)
{
    using var connection = new SqlConnection(_connectionString);
    var dp = new DynamicParameters();
    dp.Add("@ProductName", productName);
    dp.Add("@UnitPrice", unitPrice);
    dp.Add("@StockQuantity", stockQuantity);
    dp.Add("@NewProductId", direction: ParameterDirection.Output);

    int newId = dp.Get<int>("@NewProductId"); // read before execute

    connection.Execute("dbo.usp_InsertProduct", dp,
        commandType: CommandType.StoredProcedure);
    return newId;
}
```

**Concepts**
- OUTPUT parameter populated after execute
- DynamicParameters.Get read timing
- explicit DbType for output parameters
- zero-default return propagating as real ID
- ADO.NET execute-then-read semantics

**Answer**

OUTPUT parameters are populated by the database engine after the command executes — reading `@NewProductId` before calling `Execute` always returns the CLR default `0`. The method returns `0` as the new product ID on every call, regardless of what the stored procedure actually inserts. Any downstream code that uses this ID to build foreign key relationships will silently create corrupt data.

The `Add` call also omits `dbType: DbType.Int32`, which can cause provider inference issues for Output parameters on some configurations.

The correct order is to declare the output parameter with an explicit type, execute, then read:

```csharp
dp.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_InsertProduct", dp, commandType: CommandType.StoredProcedure);
return dp.Get<int>("@NewProductId");
```

SSMS manual runs hide this ordering bug because SSMS does not read output parameters before the batch executes — the ADO.NET model is explicit about the execute-then-read contract.

---

## Q3. (R) A dashboard API calls `usp_GetProductDashboard` but returns wrong totals after a DBA reorders the SELECT statements inside the procedure. What fails silently vs throws, and how do you harden it?

```csharp
public DashboardDto GetDashboard()
{
    using var connection = new SqlConnection(_connectionString);
    using var multi = connection.QueryMultiple(
        "dbo.usp_GetProductDashboard", commandType: CommandType.StoredProcedure);

    int totalCount = multi.Read<int>().Single();
    var products = multi.Read<Product>().AsList();
    string topName = multi.ReadFirst<string>();
    return new DashboardDto(products, totalCount, topName);
}
```

**Concepts**
- GridReader forward-only order dependency
- result-set reorder silent wrong mapping
- CLR type mismatch on wrong set throwing
- proc-consumer contract versioning
- documented read-order integration test

**Answer**

`GridReader.Read<T>()` is forward-only and order-dependent — it always consumes the next result set in sequence. When the DBA reorders the stored procedure's SELECT statements, the consumer reads a product-shaped result set where it expects a count integer, and vice versa. Some type mismatches throw at read time; others produce garbage values silently — if the first set happens to be mappable to `int` by accident, `totalCount` gets a nonsense value and the dashboard shows wrong numbers without any exception.

To harden this, align the consumer read order with the documented procedure contract and never leave that contract implicit. Add an integration test that asserts column shapes and values for each `Read` call. Document the expected result-set order in the procedure's header comment, and treat any change to that order as a breaking API change requiring a coordinated consumer update. For dashboards where the order is likely to change, consider a single result set with fixed column aliases, separate endpoints, or a JSON-typed return value from the procedure.

---

## Q4. (M) A filter query returns every row instead of the intended price band. The SQL and anonymous object look correct at a glance. What is the binding bug?

```csharp
return connection.Query<Product>(sql, new { MinimumPrice = minPrice, MaximumPrice = maxPrice });
// SQL: WHERE UnitPrice >= @MinPrice AND UnitPrice <= @MaxPrice
```

**Concepts**
- property-to-placeholder exact name alignment
- case-insensitive but exact-name matching
- unbound parameters treated as unconstrained
- full table scan from missing predicate
- anonymous object property naming discipline

**Answer**

Dapper binds parameters by name: the SQL tokens `@MinPrice` and `@MaxPrice` require an object with properties named `MinPrice` and `MaxPrice`. The anonymous object uses `MinimumPrice` and `MaximumPrice`, so those parameters are never supplied to the SQL command. SQL Server may treat the unbound predicates as always-true, returning all rows — a full table scan with wrong business data and no error.

The fix is to rename the properties to match the SQL placeholders exactly: `new { MinPrice = minPrice, MaxPrice = maxPrice }`. Alternatively, rename the SQL tokens to `@MinimumPrice` and `@MaximumPrice` to match the object. An integration test with known seed data asserting the exact row count within a price band is the most reliable way to catch this class of name-alignment bug.

---

## Q5. (P) An ASP.NET Core endpoint wraps a long-running report stored procedure. The controller passes `HttpContext.RequestAborted` as cancellation. What is missing for timeout and cooperative cancellation?

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

**Concepts**
- CommandDefinition as Dapper cancellation carrier
- command timeout explicit override
- CancellationToken propagation to ADO.NET
- thread and connection exhaustion under slow procs
- OperationCanceledException mapping

**Answer**

The cancellation token is passed to `OpenAsync` but not to the `QueryAsync` call itself. If the client disconnects or the request is aborted after the connection opens, the SQL command continues running until the default connection timeout elapses. Under load, slow report procedures hold pool connections open and eventually exhaust the pool. `CommandDefinition` is the Dapper hook for supplying both a command timeout and a cancellation token to a single call:

```csharp
var cmd = new CommandDefinition(
    "dbo.usp_SlowProductReport",
    parameters: null,
    commandTimeout: 120,
    commandType: CommandType.StoredProcedure,
    cancellationToken: cancellationToken);
return (await connection.QueryAsync<Product>(cmd)).AsList();
```

Set the timeout to the report's measured SLA rather than relying on the connection default. Map `OperationCanceledException` and SQL timeout exceptions (number `-2`) to appropriate HTTP status codes (499 or 504) with structured logging. For very heavy reports, an async job-plus-polling pattern is preferable to stretching the synchronous timeout indefinitely.

---

## Q6. (D) A teammate refactors `GetProductCountViaReturnValue` to use `ExecuteScalar` and another method tries to read a T-SQL `RETURN` value via an `OUTPUT` parameter. Explain what each approach gets wrong about `RETURN` vs `OUTPUT`, and what Dapper API is correct for each.

```csharp
// Attempt 1 — proc uses RETURN, not SELECT
var count = connection.ExecuteScalar<int>("dbo.usp_GetProductCount",
    commandType: CommandType.StoredProcedure);

// Attempt 2 — proc uses RETURN 0/RETURN 1, not OUTPUT @Status
var dp = new DynamicParameters(new { ProductId = id });
dp.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_TryGetProductName", dp, commandType: CommandType.StoredProcedure);
int status = dp.Get<int>("@Status");
```

**Concepts**
- T-SQL RETURN value as ReturnValue parameter
- OUTPUT parameter distinct from RETURN
- ExecuteScalar reads first result set column only
- DynamicParameters ParameterDirection.ReturnValue
- anonymous object Input-only limitation

**Answer**

T-SQL `RETURN` sends an integer through a dedicated ReturnValue channel in ADO.NET — it does not go through result sets or arbitrary OUTPUT parameters. `ExecuteScalar` reads the first column of the first row of a result set; a procedure that only executes `RETURN COUNT(*)` with no SELECT produces no result set, so `ExecuteScalar` returns `null` or zero rather than the count.

Attempt 2 adds an `@Status` OUTPUT parameter, but the procedure uses `RETURN 0` and `RETURN 1` — those values flow through ReturnValue, not through any OUTPUT parameter. `dp.Get<int>("@Status")` always returns zero because nothing ever sets that output.

The correct Dapper approach for `RETURN` values is a `DynamicParameters` entry with `ParameterDirection.ReturnValue`:

```csharp
var dp = new DynamicParameters();
dp.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
connection.Execute("dbo.usp_TryGetProductName", dp, commandType: CommandType.StoredProcedure);
int status = dp.Get<int>("@ReturnValue");
```

For a procedure that SELECTs a single value, use `ExecuteScalar`. For OUTPUT parameters, use `ParameterDirection.Output` with an explicit `DbType` and size for strings. Anonymous objects can only supply Input parameters — directional parameters always require `DynamicParameters`.

---

## Q7. (R) A batch import service loads a dashboard in one round trip but intermittently throws `InvalidOperationException` under load. Identify lifetime and reader issues and the fix.

```csharp
public (IEnumerable<Product> Products, int Total, string TopName) GetDashboardLazy()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    var multi = connection.QueryMultiple(sql);
    var products = multi.Read<Product>(); // deferred
    int total = multi.Read<int>().Single();
    string top = multi.ReadFirst<string>();
    return (products, total, top);
}
```

**Concepts**
- GridReader forward-only deferred sequence
- connection and GridReader undisposed
- deferred IEnumerable outliving reader
- materialization inside using scope
- IReadOnlyList as safe return type

**Answer**

The method returns a deferred `IEnumerable<Product>` from `multi.Read<Product>()`. When the caller enumerates `products`, the underlying `GridReader` and connection may already be in use by other concurrent calls or may have been released — causing "invalid operation" errors. The connection is also never disposed, leaking a pool slot on every invocation.

The three-part fix is to wrap both the connection and GridReader in `using` blocks, materialize every `Read` call before leaving the `using` scope, and return materialized collections:

```csharp
public (IReadOnlyList<Product> Products, int Total, string TopName) GetDashboardMaterialized()
{
    using var connection = new SqlConnection(_connectionString);
    using var multi = connection.QueryMultiple(sql);
    var products = multi.Read<Product>().AsList();
    int total = multi.Read<int>().Single();
    string top = multi.ReadFirst<string>();
    return (products, total, top);
}
```

Complete all `Read` calls in order before the `using` block exits — the forward-only `GridReader` mirrors `SqlDataReader.NextResult` semantics, and leaving it mid-way puts the reader in an undefined state.
