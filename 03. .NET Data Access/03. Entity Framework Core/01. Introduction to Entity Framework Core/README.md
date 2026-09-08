# 01. Introduction to Entity Framework Core — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q1. What is Entity Framework Core?](#q1-what-is-entity-framework-core)
- [Q2. What is an ORM, and how does EF Core fit that definition?](#q2-what-is-an-orm-and-how-does-ef-core-fit-that-definition)
- [Q3. What is the difference between EF Core and EF6 (Entity Framework Classic)?](#q3-what-is-the-difference-between-ef-core-and-ef6-entity-framework-classic)
- [Q4. What is code-first versus database-first in EF Core?](#q4-what-is-code-first-versus-database-first-in-ef-core)
- [Q5. When would you choose EF Core over ADO.NET or Dapper?](#q5-when-would-you-choose-ef-core-over-adonet-or-dapper)
- [Q6. What are the trade-offs of using EF Core?](#q6-what-are-the-trade-offs-of-using-ef-core)
- [Q7. How does EF Core translate C# queries into SQL?](#q7-how-does-ef-core-translate-c-queries-into-sql)
- [Q8. How does EF Core handle schema evolution?](#q8-how-does-ef-core-handle-schema-evolution)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 01. Introduction to Entity Framework Core

---

## Q1. What is Entity Framework Core?

**Concepts**
- cross-platform ORM for .NET
- LINQ-to-SQL translation pipeline
- database provider model
- NuGet package architecture
- change tracking and migrations support

**Answer**

Entity Framework Core (EF Core) is Microsoft's modern, cross-platform Object-Relational Mapper (ORM) for .NET. It maps C# entity classes to database tables and lets you query and persist data using LINQ, with provider plugins for SQL Server, PostgreSQL, SQLite, and other databases. EF Core 8 is the current long-term support (LTS) line aligned with .NET 8; Replaces Entity Framework 6 for new development on .NET Core and modern .NET. Ships as NuGet packages (`Microsoft.EntityFrameworkCore`, provider packages, design-time tools). Supports LINQ queries, change tracking, migrations, raw SQL, and compiled models for production tuning.

---

## Q2. What is an ORM, and how does EF Core fit that definition?

**Concepts**
- object-relational impedance mismatch
- class-to-table and property-to-column mapping
- navigation property and FK relationship model
- ORM layer over ADO.NET

**Answer**

An Object-Relational Mapper bridges the impedance mismatch between object-oriented C# code and relational database tables by mapping classes to tables, properties to columns, and object references to foreign keys. EF Core fits that definition by translating LINQ to SQL, materializing rows as entities, and persisting changes through a unit of work. You work with `Product` and `Category` objects instead of manual `INSERT` and `JOIN` strings for routine CRUD; Relationships express as navigation properties; EF Core generates join SQL when you query or include them. Schema can evolve from C# model changes through the migrations pipeline. The ORM layer sits between application code and ADO.NET, which still executes the final commands.

---

## Q3. What is the difference between EF Core and EF6 (Entity Framework Classic)?

**Concepts**
- cross-platform vs Windows-only runtime
- modular provider model in EF Core
- EF6 maintenance vs EF Core new development
- feature parity timeline

**Answer**

EF Core is a ground-up rewrite for cross-platform .NET with a modular provider model, while EF6 is the Windows-only, .NET Framework stack built on `System.Data.Entity`. EF Core prioritizes performance and cloud deployment; EF6 remains in maintenance for legacy applications. EF Core runs on Linux, macOS, and containers; EF6 targets .NET Framework and Windows-centric hosting; EF Core uses `DbContext` with fluent configuration and lightweight dependencies; EF6 uses `ObjectContext`/`DbContext` with EDMX and heavier design-time tooling. Some EF6 features arrived in EF Core over releases — stored procedure mapping, richer SQL Server types, and bulk operations in EF Core 7+. New projects on .NET 8 use EF Core; EF6 is for maintaining existing .NET Framework codebases.

---

## Q4. What is code-first versus database-first in EF Core?

**Concepts**
- code-first model ownership via C# classes
- database-first reverse engineering workflow
- migrations as schema evolution mechanism
- schema authority trade-off

**Answer**

Code-first starts from C# entity classes and uses migrations to create or update the database schema from the model. Database-first starts from an existing database and scaffolds entity classes and a `DbContext` from live tables using reverse engineering. Code-first: developers own the model in source control; `dotnet ef migrations add` generates schema scripts; Database-first: the database is the schema authority; `dotnet ef dbcontext scaffold` generates C# from tables. Both approaches use the same runtime EF Core APIs once the model exists. Hybrid workflows scaffold once then hand-edit partial classes, but the model and database must stay synchronized.

---

## Q5. When would you choose EF Core over ADO.NET or Dapper?

**Concepts**
- LINQ composability for complex queries
- automatic change tracking benefit
- migrations and repeatable schema deployment
- team productivity vs raw SQL control

**Answer**

Choose EF Core when application logic benefits from LINQ composability, automatic change tracking, migrations, and relationship management rather than hand-written SQL for every operation. It fits domain-driven CRUD services, admin applications, and teams that want the database schema driven from C# types. Rapid development of standard create-read-update-delete APIs with validation and concurrency built in; Complex querying where LINQ composes filters, sorting, and paging without string-building SQL. Applications that need migration history, seed data, and repeatable schema deployment across environments. When relationship graphs, global query filters, and interceptors provide more value than raw SQL control.

---

## Q6. What are the trade-offs of using EF Core?

**Concepts**
- productivity vs performance-predictability trade-off
- SQL transparency cost
- LINQ client-evaluation risk
- Dapper and ADO.NET escape hatches

**Answer**

EF Core trades some performance predictability and SQL transparency for productivity, abstraction, and cross-cutting features like migrations and change tracking. Misused LINQ can generate inefficient SQL; correctly used, it reduces boilerplate substantially. Advantage: less manual mapping code, unified model for reads and writes, strong tooling in Visual Studio and CLI; Disadvantage: learning curve for tracking behavior, query translation limits, and migration merge conflicts on teams. Heavy includes or untracked-vs-tracked confusion cause production performance issues if not understood. Raw SQL and Dapper remain valid escape hatches inside EF Core projects for hot paths.

---

## Q7. How does EF Core translate C# queries into SQL?

**Concepts**
- expression tree building from LINQ
- provider query compiler translation
- deferred execution trigger
- client evaluation prevention in EF Core 3+

**Answer**

EF Core builds an expression tree from LINQ methods chained on `IQueryable<T>`, passes it through a database provider's query compiler, and generates provider-specific SQL at execution time. The provider translates supported expression nodes; unsupported nodes cause client evaluation errors or exceptions in EF Core 3+. `Where`, `Select`, `OrderBy`, `Join`, and provider-specific functions like `EF.Functions.Like` translate when possible; Calling `ToList()`, `First()`, or `Count()` on the query triggers execution and SQL generation. EF Core logs generated SQL through `LogTo`, `EnableSensitiveDataLogging`, or interceptors for debugging. Compiled queries and EF Core 8 compiled models reduce repeated translation overhead in high-throughput apps.

---

## Q8. How does EF Core handle schema evolution?

**Concepts**
- versioned migration files with Up and Down methods
- model snapshot diffing
- EnsureCreated limitation vs migrations
- dotnet ef database update deployment

**Answer**

EF Core uses migrations — versioned C# files with `Up` and `Down` methods that apply incremental schema changes to the database based on differences from the current model snapshot. Teams apply migrations with `dotnet ef database update` or automated deployment pipelines. Each migration captures model changes as operations: add column, create index, alter relationship, and similar; A model snapshot records the full current model for diffing the next migration. Alternative `EnsureCreated()` creates the database once from the model but does not support migration history or upgrades. Production deployments typically run migrations in controlled release steps rather than at arbitrary application startup.

---

## Gotchas — Introduction to Entity Framework Core (Interview Traps)

---

#### Gotcha 1. Scoped DbContext captured in a singleton service

**Concepts**
- `DbContext` is scoped per HTTP request in ASP.NET Core
- singleton service holding a scoped `DbContext` — captive dependency
- context disposed while singleton still alive — `ObjectDisposedException`
- tracked state leaking across requests from different users
- `IDbContextFactory<TContext>` as the correct singleton injection pattern

**Answer**

Registering a singleton service that stores a scoped `DbContext` in a field creates a captive dependency — the context may be disposed at the end of the first request while the singleton continues to live across all subsequent requests. Symptoms are `ObjectDisposedException: Cannot access a disposed context` and intermittent cross-user data contamination in tracked entities. When a singleton genuinely needs database access, inject `IDbContextFactory<TContext>` instead and create/dispose a short-lived context instance per operation.

---

#### Gotcha 2. DbContext is not thread-safe — never share across threads

**Concepts**
- `DbContext` internal state not thread-safe
- concurrent queries on same context cause undefined behavior
- `Parallel.ForEach` with shared context corrupts change tracker
- separate context instance per thread/task via `IDbContextFactory`
- ASP.NET Core scopes one context per request on one thread

**Answer**

`DbContext` is not thread-safe — running concurrent operations (parallel queries, parallel `SaveChanges`) on the same instance corrupts the change tracker and produces unpredictable exceptions. ASP.NET Core's DI system scopes one context per request, and requests are handled on a single thread, so normal request handlers are safe. The problem arises in background services or `Parallel.ForEach` — each parallel branch must have its own `DbContext` instance obtained from `IDbContextFactory<TContext>`.

---

#### Gotcha 3. N+1 queries from accessing navigation properties in a loop

**Concepts**
- navigation property access on unloaded entity fires hidden SQL
- lazy loading proxy triggers per-row database roundtrip
- N parent entities → N separate SQL queries for children
- `Include`/`ThenInclude` for eager loading
- EF Core command logging to detect N+1 pattern

**Answer**

Accessing a navigation property in a loop over unloaded entities fires one SQL query per parent entity — the classic N+1 performance collapse. With lazy loading enabled, this happens silently without any visible query call in the source code. EF Core command logging revealing many identical query templates with different ID parameter values is the diagnostic signal. Fix with `.Include(o => o.Lines)` for eager loading, `.AsSplitQuery()` when multiple collections cause Cartesian explosion, or `.Select()` projection when only a subset of data is needed.

---

#### Gotcha 4. Client-side LINQ evaluation — full table loaded into memory

**Concepts**
- `AsEnumerable()` or `ToList()` before filter causes full table scan
- EF Core 3+ throws `InvalidOperationException` for untranslatable predicates
- non-translatable C# method in `Where()` forces client evaluation
- `EF.Functions` for database-side string/date functions
- raw SQL or stored procedure for complex untranslatable logic

**Answer**

Calling `AsEnumerable()` or `ToList()` before applying a `Where` filter switches EF Core to LINQ-to-Objects mode, pulling the full table into application memory before filtering. EF Core 3+ throws for untranslatable predicates in `Where` rather than silently downloading entire tables as earlier versions did. When a `Where` predicate uses a C# method that EF Core cannot translate, use `EF.Functions` equivalents, rewrite as translatable expressions, or use raw SQL. Never call `AsEnumerable()` before `Where` on large tables.

---

#### Gotcha 5. `AsNoTracking` omitted on read-only queries — change tracking overhead

**Concepts**
- EF Core snapshots every entity's original values for change detection
- tracking overhead: CPU and memory per materialized entity
- GET-only endpoints with no `SaveChanges` waste tracking resources
- `AsNoTracking()` disables snapshots for read-only queries
- global `QueryTrackingBehavior.NoTracking` with per-query opt-in

**Answer**

By default, EF Core tracks every entity it materializes — storing original and current values per property for change detection at `SaveChanges` time. On read-only GET endpoints that never call `SaveChanges`, this tracking is pure overhead. Add `AsNoTracking()` to any query that only reads and returns data, or configure `QueryTrackingBehavior.NoTracking` globally and add explicit tracking only on queries that feed `SaveChanges`. The memory and CPU savings are measurable for large result sets.

---

#### Gotcha 6. `SaveChanges` commits all tracked changes, not just the intended entity

**Concepts**
- `SaveChanges` flushes every `Added`/`Modified`/`Deleted` tracked entity
- unintended changes on other tracked entities committed together
- multi-step service methods accumulating unrelated changes
- one context per unit-of-work as the design guideline
- `ChangeTracker.Clear()` to discard unintended pending changes

**Answer**

`SaveChanges` commits every entity currently tracked by the context in an `Added`, `Modified`, or `Deleted` state — not just the entity you intended to save. If another service method earlier in the request modified a different entity on the same scoped context, that change is also committed. Use a short-lived context scoped to one unit of work, or call `context.ChangeTracker.Clear()` to discard unintended tracked state before the intended save. EF Core 7+ `ExecuteUpdate`/`ExecuteDelete` bypass the change tracker entirely for targeted bulk operations.

---

#### Gotcha 7. In-Memory provider does not enforce relational constraints

**Concepts**
- In-Memory provider is an in-process dictionary, not a SQL engine
- no FK constraints, cascade rules, or uniqueness enforcement
- tests passing on In-Memory fail on SQL Server in staging
- `Testcontainers` or SQL Server LocalDB for relational fidelity tests
- shared In-Memory database name causes cross-test pollution

**Answer**

EF Core's In-Memory provider stores entities in memory as an in-process dictionary — it does not enforce foreign key constraints, cascade delete rules, unique indexes, or any SQL Server-specific behavior. Tests that pass on In-Memory (including cascade and FK tests) frequently fail on SQL Server in staging. Use a real provider (LocalDB or Testcontainers SQL Server) for tests that assert relational behavior. If you use In-Memory, give each test a unique database name via `Guid.NewGuid().ToString()` to avoid cross-test state pollution.

---

#### Gotcha 8. Lazy loading after DbContext disposed — serializer-triggered hidden queries fail

**Concepts**
- lazy loading proxy accesses navigation property post-disposal
- JSON serializer touches all public properties including navigations
- `ObjectDisposedException` thrown mid-serialization
- `Include` or DTO projection before context scope ends
- request-scoped context disposed at end of pipeline

**Answer**

If lazy loading is enabled and an entity with unloaded navigation properties is returned from a controller action, the JSON serializer may access those navigation properties while serializing the response — but the scoped `DbContext` may already be disposed at that point in the request pipeline, causing `ObjectDisposedException`. Always load all required navigation properties with `Include` before the context scope ends, or project to a DTO that contains only the data the response needs, eliminating any navigation property access outside the context lifetime.

---

#### Gotcha 9. EF Core provider swap is not zero-cost — LINQ differences and migration regeneration

**Concepts**
- LINQ translates differently between providers (SQL Server vs PostgreSQL)
- `UseSqlServer` vs `UseNpgsql` — connection string format change
- migration history must be regenerated for new provider
- provider-specific types (`rowversion`, `nvarchar`) not portable
- SQL Server-specific LINQ (`FromSqlRaw` with T-SQL) may not translate

**Answer**

Swapping EF Core's database provider is a NuGet and `Use*` extension change at the surface, but the SQL generated for the same LINQ query differs between providers — queries that work on SQL Server may not translate correctly on PostgreSQL or SQLite. Migration history is specific to each provider and must be regenerated after a swap. Provider-specific types like `rowversion`, spatial types, and JSON columns require different Fluent API configuration per provider. After a provider swap, regenerate migrations, run the full test suite against the target provider, and verify execution plans on hot queries.

---

#### Gotcha 10. `context.Database.EnsureCreated()` vs `MigrateAsync()` — no migration history recorded

**Concepts**
- `EnsureCreated` creates schema if absent but records no migration history
- `MigrateAsync` applies pending migrations and records them in `__EFMigrationsHistory`
- mixing `EnsureCreated` with migrations causes migration failure on first run
- `EnsureCreated` appropriate only for ephemeral test databases
- production always uses `MigrateAsync` or migration scripts

**Answer**

`context.Database.EnsureCreated()` creates the database schema directly from the current model if the database does not exist, but it does not write any entries to `__EFMigrationsHistory`. If you then run `dotnet ef database update`, EF Core sees no applied migrations and tries to apply all of them — including `InitialCreate`, which conflicts with the schema already created by `EnsureCreated`. Use `EnsureCreated` only for test databases that are created and destroyed per test run; use `MigrateAsync` or migration scripts for all production and staging environments.

---

## Scenario-Based Questions (Karat Format)

---

## Q97. (D) Your team owns a product catalog microservice: CRUD on `Product` with relationships, schema owned via Code-First migrations, and a separate admin dashboard that runs one hand-tuned SQL report per screen (aggregations, window functions, 500k+ rows, read-only). They want to pick **one** data-access stack for everything. What do you recommend for each path — EF Core, Dapper, or ADO.NET — and why would forcing a single choice hurt?

**Concepts**
- EF Core vs Dapper vs ADO.NET selection
- mixed access pattern per workload
- CRUD vs reporting stack split
- hybrid data-access architecture

**Answer**

Use EF Core for the catalog CRUD and schema evolution path, and Dapper (or targeted raw SQL through EF's escape hatches) for the heavy read-only reports — not ADO.NET unless you need provider-specific streaming APIs Dapper cannot cover. Forcing one stack either sacrifices migration/change-tracking productivity on writes or accepts poor SQL and overhead on report queries. - Catalog CRUD + relationships + migrations: EF Core — entities map to your domain, `SaveChanges` handles unit-of-work, Code-First migrations keep schema aligned (this chapter's stack comparison; detail in ch03). - Admin dashboards / large aggregations: Dapper — you keep hand-tuned SQL, `Query<T>` maps rows with minimal overhead, no change tracker on 500k rows. - ADO.NET: Reserve for max-control scenarios (manual `SqlDataReader` streaming, bulk copy APIs) when neither EF nor Dapper fits. - Hybrid is normal: Same database, two access paths — EF for writes/domain services, Dapper for read-optimized report endpoints — with clear boundaries so teams do not duplicate business rules in SQL strings. - Forcing EF everywhere: LINQ translation may produce suboptimal plans; materializing large graphs wastes memory; no change tracking needed on read-only reports. - Forcing Dapper everywhere: You reimplement relationship graphs, migration tooling, and optimistic concurrency manually on the CRUD side. Production takeaway: The intro chapter's "when to use" table is a decision guide, not a loyalty oath — senior judgment is picking the right tool per workload while sharing one connection string and schema.

---

## Q98. (R) CI passes with this "integration" test setup, but staging against SQL Server fails on the same assertions. Review the test harness:

```csharp
public class ProductRepositoryTests : IDisposable
{
    private readonly AppDbContext _db;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ProductTests") // same name for every test class
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public void DeleteParent_cascades_to_child_rows()
    {
        // seed Category + Products, delete Category, assert children removed
    }
}
```

**Concepts**
- In-Memory provider vs SQL Server fidelity
- cascade and FK behavior absence
- shared test database isolation failure
- Testcontainers for real-provider tests

**Answer**

The In-Memory provider is an in-process dictionary, not a SQL engine — it does not enforce relational constraints, cascade rules, or T-SQL semantics the way SQL Server does. Sharing one database name across parallel tests also causes cross-test pollution, so green CI does not predict staging behavior.

1. Use a **unique** In-Memory name per test (`Guid.NewGuid().ToString()`) if you only need fast unit tests of LINQ against `DbContext` — as in this chapter's `InMemoryProviderPreview` (`"EfCoreIntroCh01"`).
2. Move cascade/FK/delete-behavior tests to **SQL Server integration tests** — LocalDB, Testcontainers, or a dedicated CI database — with migrations applied (`Database.Migrate()` or `dotnet ef database update`).
3. Keep In-Memory for pure logic tests (query filters, mapping) where SQL translation differences are irrelevant; never assert provider-specific SQL behavior against In-Memory.
4. Dispose/`using` the context per test and avoid static shared stores.

---

## Q99. (P) A developer adds `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.SqlServer` to the web project, writes `AppDbContext` and entities, then runs `dotnet ef migrations add InitialCreate` from the solution root. The tool reports that no `DbContext` was found or that design-time services cannot be resolved. Explain **design-time vs runtime** EF Core packages and list the fixes in priority order.

**Concepts**
- design-time vs runtime package separation
- dotnet-ef tool installation requirement
- IDesignTimeDbContextFactory for CLI
- project and startup-project flags

**Answer**

Runtime packages (`Microsoft.EntityFrameworkCore`, provider packages) power your app at execution time; design-time tooling needs `Microsoft.EntityFrameworkCore.Design` (and often `dotnet-ef` as a local tool) to construct `DbContext` outside the running host, discover options, and generate migrations. Missing design packages or wrong startup project causes the CLI to fail even when the app runs fine. - Runtime (app host): `Microsoft.EntityFrameworkCore` + `Microsoft.EntityFrameworkCore.SqlServer` (or Npgsql, etc.) — loaded when the API runs; `AddDbContext` and queries use these. - Design-time (CLI / VS Package Manager): `Microsoft.EntityFrameworkCore.Design` — referenced with `PrivateAssets="All"` in the project that owns `DbContext`; supplies `IDesignTimeDbContextFactory` resolution, MSBuild targets for `dotnet ef`. - Tooling: Install `dotnet-ef` as a local tool (`dotnet tool install dotnet-ef`) or use PMC `Add-Migration` with Design package present — the global SDK does not include EF commands by default. - Startup project: Run from the project containing `DbContext`, or pass `--project` (migrations project) and `--startup-project` (web app with `Program.cs` and connection string). - `IDesignTimeDbContextFactory<TContext>`: Add when DI-only configuration (no parameterless ctor) prevents the tools from building options — factory builds `DbContextOptions` the same way as `Program.cs`.

1. Add `Microsoft.EntityFrameworkCore.Design` to the `DbContext` project (matches version of runtime packages — e.g. 8.0.11 in this chapter's `.csproj`).
2. Install/restore `dotnet-ef` local tool; run `dotnet ef migrations add InitialCreate --project <ContextProject> --startup-project <WebProject>`.
3. Ensure `AppDbContext` is public and in an assembly referenced by the startup project; pass explicit `--context AppDbContext` if multiple contexts exist.
4. If options come only from DI, implement `IDesignTimeDbContextFactory<AppDbContext>` reading `appsettings.json` or env vars — mirrors production registration.

---

## Q100. (M) Production moves from on-prem SQL Server to Azure Database for PostgreSQL. The codebase still references only `Microsoft.EntityFrameworkCore.SqlServer` and calls `UseSqlServer(connectionString)` inside `OnConfiguring`. Entity classes and LINQ queries are unchanged. What must change for **provider selection**, and what still requires manual verification after the swap?

**Concepts**
- provider NuGet package swap procedure
- UseNpgsql vs UseSqlServer configuration
- migration history regeneration after provider change
- dialect difference verification

**Answer**

Swap the NuGet provider package and the `Use*` extension — replace `Microsoft.EntityFrameworkCore.SqlServer` + `UseSqlServer` with `Npgsql.EntityFrameworkCore.PostgreSQL` + `UseNpgsql` — and update connection strings. LINQ and entities largely stay the same, but dialect differences, migrations, and provider-specific types must be revalidated; you cannot copy SQL Server migration history verbatim. - Packages: Remove `Microsoft.EntityFrameworkCore.SqlServer`; add `Npgsql.EntityFrameworkCore.PostgreSQL` (version aligned with EF Core 8.x). - Registration: Centralize in `AddDbContext` / `DbContextOptionsBuilder` — `options.UseNpgsql(configuration.GetConnectionString("Default"))`; remove hardcoded `UseSqlServer` from `OnConfiguring` when using DI (see Q6). - Connection string: PostgreSQL format (`Host=...;Database=...;Username=...;Password=...`) — often from Azure Key Vault or App Configuration, not LocalDB. - Migrations: Regenerate or baseline a new migration history for PostgreSQL — SQL Server–specific annotations (clustered indexes, `nvarchar`, sequences) do not port automatically. - Manual verification: Raw SQL (ch10), `decimal` precision, `DateTime` kind handling, string collation, JSON/hierarchy columns, and query plans for hot LINQ — providers translate differently (this chapter's provider table: same DbContext surface, different dialect). - Tests: Replace In-Memory or SQL Server–only CI with PostgreSQL Testcontainers or Azure flexible server integration tests before cutover. Production takeaway: Provider swap is a NuGet + extension + connection change at the surface, but production readiness means re-running migrations and performance tests against the target dialect — not assuming LINQ is byte-identical SQL.
