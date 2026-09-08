# 03. Entity Framework Core — Cross-Topic Interview Q&A
> Back to [.NET Data Access](../README.md)

## Subfolders

| # | Topic | Questions |
|---|-------|-----------|
| 01 | [Introduction to EF Core](01.%20Introduction%20to%20Entity%20Framework%20Core/INTERVIEW_QA.md) | What EF Core is, ORM concepts, code-first vs database-first |
| 02 | [DbContext & DbSet](02.%20DbContext%20%26%20DbSet/INTERVIEW_QA.md) | Lifetime, DI registration, scoped vs singleton |
| 03 | [Code-First Models & Migrations](03.%20Code-First%20Models%20%26%20Migrations/INTERVIEW_QA.md) | Entity classes, `Add-Migration`, `Update-Database` |
| 04 | [Database-First & Reverse Engineering](04.%20Database-First%20%26%20Reverse%20Engineering/INTERVIEW_QA.md) | `dotnet ef dbcontext scaffold`, keeping generated code |
| 05 | [CRUD Operations & SaveChanges](05.%20CRUD%20Operations%20%26%20SaveChanges/INTERVIEW_QA.md) | Add/Update/Remove, `SaveChangesAsync`, concurrency |
| 06 | [Relationships & Navigation Properties](06.%20Relationships%20%26%20Navigation%20Properties/INTERVIEW_QA.md) | One-to-many, many-to-many, cascade delete |
| 07 | [Fluent API & Data Annotations](07.%20Fluent%20API%20%26%20Data%20Annotations/INTERVIEW_QA.md) | `OnModelCreating`, `[Required]`, `[MaxLength]`, owned types |
| 08 | [LINQ to Entities & Query Patterns](08.%20LINQ%20to%20Entities%20%26%20Query%20Patterns/INTERVIEW_QA.md) | Query translation, `AsNoTracking`, projections, compiled queries |
| 09 | [Loading Related Data](09.%20Loading%20Related%20Data/INTERVIEW_QA.md) | Eager (`Include`), explicit, lazy loading, N+1 |
| 10 | [Raw SQL & Stored Procedures](10.%20Raw%20SQL%20%26%20Stored%20Procedures/INTERVIEW_QA.md) | `FromSqlRaw`, `ExecuteSqlRaw`, `SqlQuery<T>` |
| 11 | [Change Tracking, Async & Transactions](11.%20Change%20Tracking%2C%20Async%20%26%20Transactions/INTERVIEW_QA.md) | Tracker states, `SaveChangesAsync`, `IDbContextTransaction` |

---

## Table of Contents

- [CQ1. DbContext lifetime, change tracking, and why scoped registration matters.](#cq1-dbcontext-lifetime-change-tracking-and-why-scoped-registration-matters)
- [CQ2. N+1 query problem: how it arises and how Include, projections, and compiled queries fix it.](#cq2-n1-query-problem-how-it-arises-and-how-include-projections-and-compiled-queries-fix-it)
- [CQ3. Migrations, Fluent API, and the code-first workflow end-to-end.](#cq3-migrations-fluent-api-and-the-code-first-workflow-end-to-end)
- [CQ4. Transactions across SaveChanges, raw SQL, and Dapper on a shared connection.](#cq4-transactions-across-savechanges-raw-sql-and-dapper-on-a-shared-connection)
- [CQ5. AsNoTracking, projections, and compiled queries: read-performance toolkit.](#cq5-asnotracking-projections-and-compiled-queries-read-performance-toolkit)

---

## CQ1. `DbContext` lifetime, change tracking, and why scoped registration matters

**Concepts**
- `DbContext` is not thread-safe: one instance per request
- change tracker holds in-memory entity snapshots
- scoped DI lifetime: one `DbContext` per HTTP request
- singleton `DbContext`: shared tracker across requests — data races
- transient `DbContext`: no cross-method `SaveChanges` — incomplete units of work

**Answer**

`DbContext` maintains an in-memory identity map and a change tracker that records the original snapshot of every entity it loaded. On `SaveChanges`, it computes a diff between the snapshot and the current state to generate `UPDATE` statements. This tracking state is mutable and is not thread-safe — two concurrent requests sharing one instance would corrupt each other's change sets. Registering `DbContext` as **scoped** in ASP.NET Core (`services.AddDbContext<AppDbContext>(...)`) creates one instance per HTTP request, which is correct: a single request reads entities, mutates them, and calls `SaveChanges` as a logical unit of work, then the instance is disposed. Registering as **singleton** is a bug: the tracker accumulates entities across all requests, leaks memory, and causes concurrency exceptions. Registering as **transient** creates a fresh instance per injection site: a service and its repository get different `DbContext` instances, so entities tracked by one are invisible to the other's `SaveChanges` — the unit of work is broken across the request. Scoped is the only correct lifetime for web applications.

---

## CQ2. N+1 query problem: how it arises and how `Include`, projections, and compiled queries fix it

**Concepts**
- N+1: one query for N parents + N queries for their children
- `Include` / `ThenInclude` issues a JOIN or split-query to load navigation properties
- `AsSplitQuery` for large result sets avoiding Cartesian explosion
- `Select` projection avoids loading unreferenced columns
- compiled queries eliminate repeated LINQ expression tree compilation

**Answer**

N+1 arises when code iterates a collection of entities and accesses a navigation property that was not loaded: `foreach (var order in db.Orders.ToList()) { Console.WriteLine(order.Customer.Name); }`. If `Customer` is not included, EF Core issues one `SELECT` for all orders and then one `SELECT` per order to lazy-load the customer — 1 + N database round-trips. `Include(o => o.Customer)` fixes this by generating a single `JOIN` that loads both in one round-trip. `ThenInclude` chains deeper: `.Include(o => o.Lines).ThenInclude(l => l.Product)`. For collections that produce large Cartesian products (order with 100 lines × 50 products), `AsSplitQuery()` issues separate SQL queries and EF Core stitches them in memory — trading round-trips for Cartesian explosion. `Select(o => new OrderDto { ... })` is better still for read-only scenarios: only the columns named in the projection are fetched, navigation properties are not needed, and no change tracking overhead is incurred. `EF.CompileQuery` caches the compiled expression tree so repeated calls skip the translation step, which is measurable on high-frequency queries.

---

## CQ3. Migrations, Fluent API, and the code-first workflow end-to-end

**Concepts**
- entity classes → `OnModelCreating` Fluent API → migration snapshot
- `dotnet ef migrations add <Name>` generates `Up` / `Down` methods
- `dotnet ef database update` applies pending migrations
- migration snapshot tracks the current model state, not the database
- Fluent API vs Data Annotations: precedence and separation of concerns

**Answer**

The code-first workflow starts with plain C# entity classes. EF Core infers a default schema (table name from `DbSet` property, PK from `Id` or `<TypeName>Id`, foreign keys from navigation property naming conventions). You override those defaults in `OnModelCreating` with the Fluent API: `modelBuilder.Entity<Order>().HasKey(o => o.OrderId)`, `.Property(o => o.Total).HasColumnType("decimal(18,2)")`, `.HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict)`. Data Annotations (`[Required]`, `[MaxLength(200)]`, `[Column("order_total")]`) are a lighter alternative but mix infrastructure concerns into domain entities; Fluent API keeps that in `AppDbContext`. `dotnet ef migrations add AddOrderTotal` snapshots the current model, diffs it against the prior migration snapshot, and generates an `Up()` with `AddColumn` and a `Down()` with `DropColumn`. `dotnet ef database update` runs all pending `Up()` methods in order. Never hand-edit the migration snapshot file — EF Core owns it.

---

## CQ4. Transactions across `SaveChanges`, raw SQL, and Dapper on a shared connection

**Concepts**
- `SaveChanges` wraps all changes in an implicit transaction
- `IDbContextTransaction` for explicit multi-`SaveChanges` transactions
- `Database.GetDbConnection()` + `Database.CurrentTransaction.GetDbTransaction()` for Dapper
- `TransactionScope` for cross-context distributed transactions
- rollback on exception; never swallow the exception before rollback

**Answer**

`SaveChanges` automatically wraps all pending changes in a single database transaction — if any SQL fails, the entire batch rolls back. For multi-step workflows where you need to call `SaveChanges` more than once in the same atomic unit, use an explicit transaction: `await using var tx = await db.Database.BeginTransactionAsync(ct)`, perform work, `await db.SaveChangesAsync(ct)`, do more work, `await db.SaveChangesAsync(ct)`, then `await tx.CommitAsync(ct)` — a `try/catch` should call `await tx.RollbackAsync(ct)` before rethrowing. To include Dapper commands in the same transaction, get the underlying ADO.NET objects: `var conn = db.Database.GetDbConnection(); var dbTx = tx.GetDbTransaction(); await conn.ExecuteAsync(sql, param, transaction: dbTx)`. Both EF Core `SaveChanges` and Dapper `Execute` then participate in the same database transaction. Avoid `TransactionScope` with async code unless you target .NET Core 2.0+ and understand the thread-affinity requirements; prefer explicit `IDbContextTransaction` for EF Core async flows.

---

## CQ5. `AsNoTracking`, projections, and compiled queries: read-performance toolkit

**Concepts**
- `AsNoTracking`: skip snapshot allocation and identity-map lookup — read-only queries
- `AsNoTrackingWithIdentityResolution`: deduplicate without tracking overhead
- `Select` projection: only requested columns travel the wire
- `EF.CompileQuery`: skip LINQ expression-tree translation on hot paths
- when NOT to use AsNoTracking: entities you intend to update in the same request

**Answer**

`AsNoTracking()` tells EF Core not to add returned entities to the change tracker: no snapshot is stored and the identity map is not consulted. This is the single highest-impact optimisation for read-only queries — change tracking overhead is measurable when loading hundreds of entities per request. Add it to any query where you will not call `SaveChanges` on the results: `db.Products.AsNoTracking().Where(p => p.Active).ToListAsync(ct)`. `AsNoTrackingWithIdentityResolution()` is a middle ground: deduplicates entities that appear multiple times in a join result (so navigations are wired correctly) without the full tracking overhead. `Select(p => new ProductDto { ... })` reduces both query and allocation cost: only projected columns travel the wire, EF Core does not instantiate full entity objects, and no tracking is needed because DTOs are not entities. `EF.CompileQuery` pre-compiles the LINQ expression tree to a delegate, eliminating the ~0.1–0.5 ms translation cost on every call: `static readonly Func<AppDb, int, Product?> ById = EF.CompileQuery((AppDb db, int id) => db.Products.FirstOrDefault(p => p.Id == id))`. Use compiled queries for endpoints called thousands of times per second where the translation overhead is measurable in profiling.
