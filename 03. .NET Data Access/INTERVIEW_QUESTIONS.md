# .NET Data Access — Interview Questions

Organized by curriculum chapters under `04. .NET Data Access` (ADO.NET, Dapper, Entity Framework Core).  
Overlapping questions are deduplicated; each topic appears once in its best-fit chapter.  
**Gotchas** at the end are real interview traps — patterns candidates commonly miss.

> **Purpose:** Interview-focused recall bank — high-yield questions only. Depth where interviewers actually probe, not exhaustive API coverage.

---

## ADO.NET

## Chapter 01. Introduction to ADO.NET

1. What is ADO.NET?
2. What is the difference between connected and disconnected data access in ADO.NET?
3. What are the core building blocks of ADO.NET (connection, command, reader, adapter)?
4. What is the difference between ADO.NET and an ORM like Entity Framework Core?
5. When would you choose ADO.NET over Dapper or EF Core?
6. What is the connected model, and which ADO.NET types does it primarily use?
7. What is the disconnected model, and which ADO.NET types does it primarily use?
8. What are the trade-offs of hand-written SQL versus a higher-level ORM?

---

## Chapter 02. SqlConnection & Connection Strings

1. What is connection pooling in ADO.NET?
2. Does creating `new SqlConnection()` every time open a new physical database connection?
3. Why should you use `using` or `await using` with connections?
4. How do you store connection strings securely in ASP.NET Core?
5. What is the difference between `Microsoft.Data.SqlClient` and `System.Data.SqlClient`?
6. What is a pool exhaustion error, and what typically causes it?
7. What symptoms indicate a misconfigured or exhausted connection pool?
8. How do unclosed connections affect pool availability?

---

## Chapter 03. SqlCommand & Parameters

1. What is the difference between `ExecuteReader`, `ExecuteNonQuery`, and `ExecuteScalar`?
2. What is a parameterized query, and why is it preferred over string concatenation?
3. What is SQL injection, and how do parameters prevent it?
4. What is the difference between `AddWithValue` and explicitly typed `SqlParameter`?
5. What is the difference between `Text` and `StoredProcedure` command types?
6. When would you use `ExecuteScalar` instead of `ExecuteReader`?

---

## Chapter 04. SqlDataReader

1. Why is `SqlDataReader` described as a forward-only, read-only cursor?
2. What is the difference between connected streaming reads and loading everything into memory?
3. Why must a `SqlDataReader` be closed or disposed before running another command on the same connection (without MARS)?
4. When is `SqlDataReader` the best choice for large result sets?
5. How do you handle NULL database values when reading from a data reader?
6. What performance advantage does a reader have over filling a `DataTable`?
7. What happens if you do not dispose a data reader?

---

## Chapter 05. DataSet, DataTable & SqlDataAdapter

1. What is the disconnected model that `DataSet`/`DataTable` support?
2. What is the difference between `DataReader` streaming and `DataAdapter.Fill`?
3. When would you still use `DataSet`/`DataTable` in modern .NET applications?
4. What are the memory implications of filling a large table into a `DataSet`?
5. Why are `DataSet`/`DataTable` less common in ASP.NET Core APIs than in older WinForms apps?

---

## Chapter 06. Transactions & Connection Pooling

1. What are the ACID properties of a transaction?
2. How do you begin, commit, and rollback a transaction in ADO.NET?
3. Why must all commands in a transaction share the same connection?
4. What is `TransactionScope`, and how does it differ from `SqlTransaction`?
5. What is a pool exhaustion error, and what typically causes it?
6. What isolation levels exist, and why do they matter?
7. What is the correct pattern for rollback in a `try/catch` around ADO.NET transactions?

---

## Chapter 07. Stored Procedures & Output Parameters

1. What is a stored procedure, and why use one from ADO.NET?
2. How do you execute a stored procedure with `SqlCommand`?
3. What is the difference between output parameters and return values (`ReturnValue`)?
4. When are stored procedures preferred over inline SQL in ADO.NET?
5. What are the trade-offs of putting business logic in stored procedures versus C#?

---

## Chapter 08. Async ADO.NET

1. Why should database I/O be async in ASP.NET Core request handlers?
2. What does `await using` provide when working with connections and readers?
3. What problems arise from calling `.Result` or `.Wait()` on async ADO.NET operations?
4. What is `CancellationToken` support in async ADO.NET methods?
5. What is the recommended async pattern for opening a connection, executing a command, and reading results?
6. When is synchronous ADO.NET still acceptable?

---

## Dapper

## Chapter 01. Introduction to Dapper

1. What is Dapper?
2. What type of library is Dapper (ORM, micro-ORM, or something else)?
3. What problem does Dapper solve compared to raw ADO.NET?
4. What problem does Dapper solve compared to Entity Framework Core?
5. When would you choose Dapper over EF Core?
6. When would you choose EF Core over Dapper?
7. What are the main advantages and limitations of Dapper?
8. Does Dapper generate SQL for you?

---

## Chapter 02. Queries, Execute & Async Methods

1. What is the difference between Dapper's `Query` and `Execute` methods?
2. When do you use `Query<T>` versus `QueryFirstOrDefault<T>`?
3. What does `Execute` return, and when is it used?
4. Does Dapper open the connection if it is closed when you call `Query`?
5. What is the difference between buffered and unbuffered queries in Dapper?
6. What async methods does Dapper provide (`QueryAsync`, `ExecuteAsync`, etc.)?

---

## Chapter 03. Parameters, Stored Procedures & QueryMultiple

1. How do you pass parameters to a Dapper query?
2. How does Dapper prevent SQL injection?
3. How do you call a stored procedure with Dapper?
4. What is `QueryMultiple`, and when is it used?
5. How do you read multiple result sets from `QueryMultiple`?
6. When would you prefer `QueryMultiple` over separate round-trips?

---

## Chapter 04. Mapping, Multi-Mapping & Advanced Patterns

1. How does Dapper map column names to property names by default?
2. What happens when column names do not match property names?
3. What is multi-mapping in Dapper?
4. What is the `splitOn` parameter in multi-mapping?
5. How does Dapper handle nested object graphs compared to EF Core `Include`?

---

## Entity Framework Core

## Chapter 01. Introduction to Entity Framework Core

1. What is Entity Framework Core?
2. What is an ORM, and how does EF Core fit that definition?
3. What is the difference between EF Core and EF6 (Entity Framework Classic)?
4. What is code-first versus database-first in EF Core?
5. When would you choose EF Core over ADO.NET or Dapper?
6. What are the trade-offs of using EF Core?
7. How does EF Core translate C# queries into SQL?
8. How does EF Core handle schema evolution?

---

## Chapter 02. DbContext & DbSet

1. What is a `DbContext` in EF Core?
2. What is a `DbSet<T>`?
3. How do you register `DbContext` in ASP.NET Core dependency injection?
4. Why is `DbContext` typically registered as scoped?
5. What is the difference between injecting `DbContext` and using `IDbContextFactory<TContext>`?
6. What does `OnModelCreating` do in a `DbContext`?
7. What is `EnsureCreated`, and how does it differ from migrations?
8. Can you reuse one `DbContext` across multiple threads?

---

## Chapter 03. Code-First Models & Migrations

1. What is code-first in EF Core?
2. What is a migration in EF Core?
3. How do you create and apply migrations from the CLI?
4. What is the difference between `Up` and `Down` in a migration?
5. What is a model snapshot in EF Core migrations?
6. What happens if you change a model without creating a migration?
7. What is the difference between `EnsureCreated` and migrations-based schema creation?
8. When should migrations run automatically in production?

---

## Chapter 04. Database-First & Reverse Engineering

1. What is database-first in EF Core?
2. How do you scaffold a `DbContext` from an existing database?
3. When is database-first preferred over code-first?
4. What are the limitations of reverse-engineered models?
5. How do you refresh a scaffolded model after database schema changes?
6. Can you combine scaffolded models with manual partial classes?

---

## Chapter 05. CRUD Operations & SaveChanges

1. How do you insert, update, and delete entities with EF Core?
2. What does `SaveChanges()` do?
3. What is the difference between `Add`, `Update`, and `Remove` on a `DbSet`?
4. What is attach versus add when working with disconnected entities?
5. What is `ExecuteUpdate` / `ExecuteDelete` in EF Core 7+?
6. What is the unit-of-work pattern in relation to `DbContext`?
7. How do you perform a bulk update without loading entities into memory?

---

## Chapter 06. Relationships & Navigation Properties

1. What is a navigation property in EF Core?
2. What is the difference between one-to-many and many-to-many relationships?
3. How do you configure a one-to-many relationship in code-first?
4. What is a foreign key property in EF Core?
5. What is cascade delete in EF Core?
6. When would you disable cascade delete?
7. What is a many-to-many relationship in EF Core 5+?

---

## Chapter 07. Fluent API & Data Annotations

1. What is the Fluent API in EF Core?
2. When should you prefer Fluent API over data annotations?
3. What is `IEntityTypeConfiguration<T>`?
4. How do Fluent API and annotations interact when both configure the same property?
5. How do you configure indexes with Fluent API?

---

## Chapter 08. LINQ to Entities & Query Patterns

1. What is the difference between LINQ to Objects and LINQ to Entities?
2. What is `IQueryable<T>` versus `IEnumerable<T>` in EF Core queries?
3. When does query execution actually occur (deferred execution)?
4. What is the N+1 query problem?
5. What causes client evaluation warnings or errors in EF Core?
6. How do you filter, project, sort, and paginate with EF Core LINQ?
7. What is the difference between `Select` projection and loading full entities?
8. What is a global query filter in EF Core?
9. How do you debug the SQL generated by EF Core?

---

## Chapter 09. Loading Related Data

1. What is the difference between eager loading, lazy loading, and explicit loading?
2. How do you use `Include` and `ThenInclude` for eager loading?
3. What is the N+1 problem in the context of loading related data?
4. Why should you avoid lazy loading in ASP.NET Core applications?
5. What is a cartesian explosion when including multiple collections?
6. What is `AsSplitQuery`, and when should you use it?
7. How do you choose between eager loading, explicit loading, and projection?

---

## Chapter 10. Raw SQL & Stored Procedures

1. What is `FromSqlRaw` versus `FromSqlInterpolated`?
2. Why is `FromSqlInterpolated` preferred over string interpolation for raw SQL?
3. When should you use raw SQL instead of LINQ in EF Core?
4. What are the security considerations for raw SQL in EF Core?
5. How do you call stored procedures with EF Core?
6. How do you execute non-query raw SQL (`ExecuteSqlRaw`)?

---

## Chapter 11. Change Tracking, Async & Transactions

1. What is change tracking in EF Core?
2. What entity states does EF Core track (`Added`, `Modified`, `Deleted`, `Unchanged`)?
3. What does `AsNoTracking` do, and when should you use it?
4. What is the performance impact of change tracking on read-heavy queries?
5. Why use async EF Core methods in ASP.NET Core?
6. How do you begin and commit a transaction in EF Core?
7. How does EF Core detect concurrency conflicts?
8. What is a concurrency token or row version column?
9. How do you handle `DbUpdateConcurrencyException`?

---

## Gotchas — .NET Data Access (Interview Traps)

#### Gotcha 1. String concatenation instead of parameters

Building SQL with `$"WHERE Id = {id}"` or `+ userInput +` in ADO.NET or Dapper bypasses parameterization and opens SQL injection even when the rest of the app "uses an ORM."

#### Gotcha 2. Open DataReader blocks second command

Running another `SqlCommand` on the same connection while a `SqlDataReader` is open fails unless MARS is enabled — a common bug when loading header then detail rows in ADO.NET.

#### Gotcha 3. `AddWithValue` and wrong SQL types

`SqlParameter.AddWithValue` infers types that may not match the column (e.g., `nvarchar` vs `varchar`, oversized strings), causing index scans, implicit conversion, or unexpected plan cache behavior.

#### Gotcha 4. Leaked connections exhaust the pool

Not disposing `SqlConnection`, `SqlDataReader`, or returning before `using` completes leaks pool slots until timeout — under load the app throws "timeout expired obtaining connection from pool."

#### Gotcha 5. Transaction started after first command

Beginning a `SqlTransaction` only after the first statement already committed implicitly leaves multi-step operations non-atomic under concurrency.

#### Gotcha 6. Dapper `Query` without `using` on connection

Returning `IEnumerable<T>` from Dapper before disposing the connection defers enumeration and fails at runtime or holds connections open until GC — materialize with `.ToList()` inside the `using` block.

#### Gotcha 7. `QuerySingle` when zero or many rows exist

Using `QuerySingle` on optional lookups throws where `QueryFirstOrDefault` is appropriate — interview code often mishandles empty or duplicate result sets.

#### Gotcha 8. Multi-map `splitOn` wrong column

Dapper multi-mapping with an incorrect `splitOn` silently maps NULL or wrong types into nested objects, producing subtle data corruption instead of an obvious error.

#### Gotcha 9. Scoped `DbContext` captured in a singleton

Caching a repository or service that holds a scoped `DbContext` in a singleton creates disposed or cross-request state — same captive-dependency trap as other ASP.NET Core services.

#### Gotcha 10. Lazy loading after the context is disposed

Enabling lazy loading then serializing entities outside the request scope triggers "Cannot access a disposed context" or hidden N+1 queries on every property access.

#### Gotcha 11. N+1 from lazy load or missing Include

Listing parents and accessing navigation properties in a loop without eager loading or projection generates one query per row — classic EF Core performance failure in APIs.

#### Gotcha 12. Cartesian explosion with multiple Includes

Eager-loading two collections on one query multiplies rows in SQL; memory and network spike until `AsSplitQuery` or separate queries are used.

#### Gotcha 13. Client-side evaluation of LINQ

Calling `.ToList()` before filtering or using non-translatable methods like custom C# logic in `Where` pulls entire tables into memory — works in dev, fails at scale.

#### Gotcha 14. Tracking overhead on read-only queries

Omitting `AsNoTracking()` on large read-only lists makes EF Core snapshot every entity unnecessarily, increasing memory and CPU for no benefit.

#### Gotcha 15. `SaveChanges` without a transaction for multi-step updates

Multiple `SaveChanges` calls or separate operations that must succeed together allow partial commits unless wrapped in an explicit transaction or single unit of work.
