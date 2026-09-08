# 02. Dapper — Cross-Topic Interview Q&A
> Back to [.NET Data Access](../README.md)

## Subfolders

| # | Topic | Questions |
|---|-------|-----------|
| 01 | [Introduction to Dapper](01.%20Introduction%20to%20Dapper/INTERVIEW_QA.md) | What Dapper is, micro-ORM vs ORM, when to choose |
| 02 | [Queries, Execute & Async Methods](02.%20Queries%2C%20Execute%20%26%20Async%20Methods/INTERVIEW_QA.md) | `Query<T>`, `Execute`, `QueryFirstOrDefault`, async variants |
| 03 | [Parameters, Stored Procedures & QueryMultiple](03.%20Parameters%2C%20Stored%20Procedures%20%26%20QueryMultiple/INTERVIEW_QA.md) | Anonymous/DynamicParameters, sprocs, multiple result sets |
| 04 | [Mapping, Multi-Mapping & Advanced Patterns](04.%20Mapping%2C%20Multi-Mapping%20%26%20Advanced%20Patterns/INTERVIEW_QA.md) | `splitOn`, custom type handlers, buffered vs unbuffered |

---

## Table of Contents

- [CQ1. How Dapper sits on top of ADO.NET and what it adds.](#cq1-how-dapper-sits-on-top-of-adonet-and-what-it-adds)
- [CQ2. Parameterization in Dapper: anonymous objects, DynamicParameters, and SQL injection safety.](#cq2-parameterization-in-dapper-anonymous-objects-dynamicparameters-and-sql-injection-safety)
- [CQ3. Multi-mapping with `splitOn`: joining related entities in a single query.](#cq3-multi-mapping-with-spliton-joining-related-entities-in-a-single-query)
- [CQ4. Async Dapper across Query, Execute, and QueryMultiple.](#cq4-async-dapper-across-query-execute-and-querymultiple)

---

## CQ1. How Dapper sits on top of ADO.NET and what it adds

**Concepts**
- extension methods on `IDbConnection`
- IL-emitted mapping from `IDataReader` columns to POCO properties
- no connection lifecycle management — caller owns the connection
- no SQL generation — caller writes SQL
- negligible overhead vs hand-written ADO.NET mapping

**Answer**

Dapper is a NuGet package that adds extension methods (`Query<T>`, `Execute`, `QueryFirst`, etc.) directly to `IDbConnection`, so it works with any ADO.NET-compatible driver — SQL Server, PostgreSQL, SQLite, MySQL. When you call `conn.Query<Product>("SELECT ...")`, Dapper internally creates a `DbCommand`, sets the parameters, opens a `SqlDataReader`, and uses IL-emitted mapping code to project each row onto a `Product` instance. The IL emission is done once per query shape and cached, making subsequent calls as fast as hand-written column-by-column reader code. Dapper adds nothing around connection management: you open the connection (or let Dapper auto-open and auto-close it for a single query), write the SQL, and receive the typed result. There is no change tracking, no LINQ-to-SQL translation, and no migration tooling. The value proposition is removing the ADO.NET mapping boilerplate — `while (reader.Read()) { new Product { Id = reader.GetInt32(0), ... } }` — without sacrificing control over the SQL.

---

## CQ2. Parameterization in Dapper: anonymous objects, `DynamicParameters`, and SQL injection safety

**Concepts**
- anonymous object maps property names to `@param` names
- DynamicParameters for output/return params and dynamic query building
- Dapper always creates parameterized `DbParameter` objects — never string concatenation
- `IN` clause requires `WHERE id IN @ids` with an IEnumerable value
- custom type handler for non-primitive property types

**Answer**

Dapper accepts parameters as an anonymous object whose property names map to `@paramName` placeholders in the SQL: `conn.Query<Order>("SELECT * FROM Orders WHERE CustomerId = @customerId", new { customerId = id })`. Dapper converts this into a `DbParameter` added to the command — it never concatenates values into the SQL string, so it is inherently safe from SQL injection as long as you do not build the SQL itself via string concatenation. `DynamicParameters` is needed for output parameters, return values from stored procedures, and dynamic query building where the set of parameters is determined at runtime: add each parameter with `dp.Add("@name", value)` or `dp.Add("@outVal", dbType: DbType.Int32, direction: ParameterDirection.Output)`, then read back `dp.Get<int>("@outVal")` after `Execute`. For `IN` clauses, Dapper handles the expansion automatically: `WHERE Id IN @ids` with `new { ids = new[] { 1, 2, 3 } }` expands to `WHERE Id IN (@ids1, @ids2, @ids3)` — no manual join required, and the expansion is still parameterized.

---

## CQ3. Multi-mapping with `splitOn`: joining related entities in a single query

**Concepts**
- `Query<T1, T2, TReturn>` overload with mapping function
- `splitOn` names the column where the second entity starts
- N+1 avoidance: one query instead of a query per parent row
- manual deduplication when the parent appears on multiple child rows
- `QueryMultiple` as the alternative for separate result sets

**Answer**

`Query<Order, Customer, Order>` with `splitOn: "CustomerId"` tells Dapper to split the reader columns at the `CustomerId` column — columns before it map to `Order`, columns from `CustomerId` onward map to `Customer`. The mapping function receives the two populated objects and returns the composed result: `(order, customer) => { order.Customer = customer; return order; }`. This is the core N+1 avoidance pattern: one SQL `JOIN` fetches all orders with their customers in a single round-trip, instead of one query per order. The gotcha is parent deduplication — a one-to-many join produces one row per child, so each parent `Order` appears multiple times with a different child. You must group the results with a dictionary: keep a `Dictionary<int, Order>` indexed by order id, and for each row either add the order if new or append the child to the existing order's collection. `QueryMultiple` is the alternative when you prefer separate `SELECT` statements (no join): one `sql` string with two semicolon-separated queries, then `grid.Read<Order>()` and `grid.Read<OrderLine>()` to consume each result set in sequence.

---

## CQ4. Async Dapper across `Query`, `Execute`, and `QueryMultiple`

**Concepts**
- `QueryAsync<T>`, `ExecuteAsync`, `QueryFirstOrDefaultAsync`
- all async methods accept `CancellationToken`
- buffered (default) vs unbuffered async streaming
- `await using` on the `GridReader` from `QueryMultipleAsync`
- same thread-release benefit as raw async ADO.NET

**Answer**

Every Dapper method has an async counterpart: `QueryAsync<T>`, `ExecuteAsync`, `ExecuteScalarAsync`, `QueryFirstOrDefaultAsync`, and `QueryMultipleAsync`. Pass a `CancellationToken` to propagate request cancellation: `await conn.QueryAsync<Product>(sql, param, cancellationToken: ct)`. By default Dapper buffers the entire result set into a `List<T>` before returning, which means the connection is closed promptly but all rows are in memory. Pass `buffered: false` to get an `IEnumerable<T>` that streams rows lazily — the connection stays open until the enumeration is complete, so pair this with immediate consumption (`await foreach` or `.ToList()`) rather than storing the lazy sequence for later. `QueryMultipleAsync` returns a `GridReader` that implements `IAsyncDisposable`; use `await using var grid = await conn.QueryMultipleAsync(sql, param)` so the underlying reader and connection resources are released when you exit the block.
