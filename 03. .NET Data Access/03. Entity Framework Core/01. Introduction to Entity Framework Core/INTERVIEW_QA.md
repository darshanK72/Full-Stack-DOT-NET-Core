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

## Gotchas

---

## Gotcha 9. Scoped `DbContext` captured in a singleton

**Concepts**
- scoped DbContext captured in singleton field
- captive dependency anti-pattern
- IDbContextFactory for singleton database access

**Answer**

Registering a singleton service that holds a scoped `DbContext` creates a captive dependency — the context may be disposed while the singleton lives, or state leaks across HTTP requests. `DbContext` is scoped per request in ASP.NET Core — singletons must not store it in fields; Inject `IDbContextFactory<TContext>` into singletons when long-lived services need occasional database access. Symptoms include "Cannot access a disposed context" or cross-user data contamination in tracked entities.

---

## Gotcha 11. N+1 from lazy load or missing Include

**Concepts**
- N+1 from missing Include in loop
- one query per parent row multiplication
- eager loading and projection as fixes

**Answer**

Listing parent entities then accessing navigation properties in a loop without eager loading or projection fires one SQL query per parent row — classic N+1 performance collapse in EF Core APIs. One query for N orders plus N queries for each order's lines equals N+1 round-trips per request; Fix with `Include`/`ThenInclude`, split queries, or `Select` projections that join needed data in one statement. EF Core command logging revealing identical query templates with different IDs signals N+1 immediately.

---

## Gotcha 13. Client-side evaluation of LINQ

**Concepts**
- ToList before filter forces full table load
- EF Core 3+ exception for accidental client evaluation
- EF.Functions and translatable expression rewrites

**Answer**

Calling `ToList()` before filtering or using non-translatable C# logic in `Where` forces EF Core to pull entire tables into application memory — acceptable in development with small seeds, catastrophic in production at scale. EF Core 3+ throws on many accidental client evaluations instead of silently downloading whole tables; `AsEnumerable()` explicitly switches to LINQ to Objects — any following `Where` runs in memory. Rewrite with translatable expressions, `EF.Functions`, database-side filtering, or raw SQL for unsupported logic.

---

## Gotcha 14. Tracking overhead on read-only queries

**Concepts**
- change tracking snapshot overhead on read-only queries
- AsNoTracking omission on GET endpoints
- global QueryTrackingBehavior.NoTracking setting

**Answer**

Omitting `AsNoTracking()` on large read-only lists makes EF Core snapshot every entity for change detection that will never run, wasting memory and CPU on GET endpoints. Tracking stores original and current values per property for each row materialized; ASP.NET Core read services should default to `AsNoTracking()` plus DTO projection. Global `QueryTrackingBehavior.NoTracking` with explicit tracking on command paths prevents accidental overhead.

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
