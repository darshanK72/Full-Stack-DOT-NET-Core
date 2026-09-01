# 03. Entity Framework Core — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 01. Introduction to Entity Framework Core](#chapter-01-introduction-to-entity-framework-core)
  - [Q1. What is Entity Framework Core?](#chapter-01-introduction-to-entity-framework-core-q1)
  - [Q2. What is an ORM, and how does EF Core fit that definition?](#chapter-01-introduction-to-entity-framework-core-q2)
  - [Q3. What is the difference between EF Core and EF6 (Entity Frame…](#chapter-01-introduction-to-entity-framework-core-q3)
  - [Q4. What is code-first versus database-first in EF Core?](#chapter-01-introduction-to-entity-framework-core-q4)
  - [Q5. When would you choose EF Core over ADO.NET or Dapper?](#chapter-01-introduction-to-entity-framework-core-q5)
  - [Q6. What are the trade-offs of using EF Core?](#chapter-01-introduction-to-entity-framework-core-q6)
  - [Q7. How does EF Core translate C# queries into SQL?](#chapter-01-introduction-to-entity-framework-core-q7)
  - [Q8. How does EF Core handle schema evolution?](#chapter-01-introduction-to-entity-framework-core-q8)

- [Chapter 02. DbContext & DbSet](#chapter-02-dbcontext-dbset)
  - [Q1. What is a `DbContext` in EF Core?](#chapter-02-dbcontext-dbset-q1)
  - [Q2. What is a `DbSet<T>`?](#chapter-02-dbcontext-dbset-q2)
  - [Q3. How do you register `DbContext` in ASP.NET Core dependency i…](#chapter-02-dbcontext-dbset-q3)
  - [Q4. Why is `DbContext` typically registered as scoped?](#chapter-02-dbcontext-dbset-q4)
  - [Q5. What is the difference between injecting `DbContext` and usi…](#chapter-02-dbcontext-dbset-q5)
  - [Q6. What does `OnModelCreating` do in a `DbContext`?](#chapter-02-dbcontext-dbset-q6)
  - [Q7. What is `EnsureCreated`, and how does it differ from migrati…](#chapter-02-dbcontext-dbset-q7)
  - [Q8. Can you reuse one `DbContext` across multiple threads?](#chapter-02-dbcontext-dbset-q8)

- [Chapter 03. Code-First Models & Migrations](#chapter-03-code-first-models-migrations)
  - [Q1. What is code-first in EF Core?](#chapter-03-code-first-models-migrations-q1)
  - [Q2. What is a migration in EF Core?](#chapter-03-code-first-models-migrations-q2)
  - [Q3. How do you create and apply migrations from the CLI?](#chapter-03-code-first-models-migrations-q3)
  - [Q4. What is the difference between `Up` and `Down` in a migratio…](#chapter-03-code-first-models-migrations-q4)
  - [Q5. What is a model snapshot in EF Core migrations?](#chapter-03-code-first-models-migrations-q5)
  - [Q6. What happens if you change a model without creating a migrat…](#chapter-03-code-first-models-migrations-q6)
  - [Q7. What is the difference between `EnsureCreated` and migration…](#chapter-03-code-first-models-migrations-q7)
  - [Q8. When should migrations run automatically in production?](#chapter-03-code-first-models-migrations-q8)

- [Chapter 04. Database-First & Reverse Engineering](#chapter-04-database-first-reverse-engineering)
  - [Q1. What is database-first in EF Core?](#chapter-04-database-first-reverse-engineering-q1)
  - [Q2. How do you scaffold a `DbContext` from an existing database?](#chapter-04-database-first-reverse-engineering-q2)
  - [Q3. When is database-first preferred over code-first?](#chapter-04-database-first-reverse-engineering-q3)
  - [Q4. What are the limitations of reverse-engineered models?](#chapter-04-database-first-reverse-engineering-q4)
  - [Q5. How do you refresh a scaffolded model after database schema …](#chapter-04-database-first-reverse-engineering-q5)
  - [Q6. Can you combine scaffolded models with manual partial classe…](#chapter-04-database-first-reverse-engineering-q6)

- [Chapter 05. CRUD Operations & SaveChanges](#chapter-05-crud-operations-savechanges)
  - [Q1. How do you insert, update, and delete entities with EF Core?](#chapter-05-crud-operations-savechanges-q1)
  - [Q2. What does `SaveChanges()` do?](#chapter-05-crud-operations-savechanges-q2)
  - [Q3. What is the difference between `Add`, `Update`, and `Remove`…](#chapter-05-crud-operations-savechanges-q3)
  - [Q4. What is attach versus add when working with disconnected ent…](#chapter-05-crud-operations-savechanges-q4)
  - [Q5. What is `ExecuteUpdate` / `ExecuteDelete` in EF Core 7+?](#chapter-05-crud-operations-savechanges-q5)
  - [Q6. What is the unit-of-work pattern in relation to `DbContext`?](#chapter-05-crud-operations-savechanges-q6)
  - [Q7. How do you perform a bulk update without loading entities in…](#chapter-05-crud-operations-savechanges-q7)

- [Chapter 06. Relationships & Navigation Properties](#chapter-06-relationships-navigation-properties)
  - [Q1. What is a navigation property in EF Core?](#chapter-06-relationships-navigation-properties-q1)
  - [Q2. What is the difference between one-to-many and many-to-many …](#chapter-06-relationships-navigation-properties-q2)
  - [Q3. How do you configure a one-to-many relationship in code-firs…](#chapter-06-relationships-navigation-properties-q3)
  - [Q4. What is a foreign key property in EF Core?](#chapter-06-relationships-navigation-properties-q4)
  - [Q5. What is cascade delete in EF Core?](#chapter-06-relationships-navigation-properties-q5)
  - [Q6. When would you disable cascade delete?](#chapter-06-relationships-navigation-properties-q6)
  - [Q7. What is a many-to-many relationship in EF Core 5+?](#chapter-06-relationships-navigation-properties-q7)

- [Chapter 07. Fluent API & Data Annotations](#chapter-07-fluent-api-data-annotations)
  - [Q1. What is the Fluent API in EF Core?](#chapter-07-fluent-api-data-annotations-q1)
  - [Q2. When should you prefer Fluent API over data annotations?](#chapter-07-fluent-api-data-annotations-q2)
  - [Q3. What is `IEntityTypeConfiguration<T>`?](#chapter-07-fluent-api-data-annotations-q3)
  - [Q4. How do Fluent API and annotations interact when both configu…](#chapter-07-fluent-api-data-annotations-q4)
  - [Q5. How do you configure indexes with Fluent API?](#chapter-07-fluent-api-data-annotations-q5)

- [Chapter 08. LINQ to Entities & Query Patterns](#chapter-08-linq-to-entities-query-patterns)
  - [Q1. What is the difference between LINQ to Objects and LINQ to E…](#chapter-08-linq-to-entities-query-patterns-q1)
  - [Q2. What is `IQueryable<T>` versus `IEnumerable<T>` in EF Core q…](#chapter-08-linq-to-entities-query-patterns-q2)
  - [Q3. When does query execution actually occur (deferred execution…](#chapter-08-linq-to-entities-query-patterns-q3)
  - [Q4. What is the N+1 query problem?](#chapter-08-linq-to-entities-query-patterns-q4)
  - [Q5. What causes client evaluation warnings or errors in EF Core?](#chapter-08-linq-to-entities-query-patterns-q5)
  - [Q6. How do you filter, project, sort, and paginate with EF Core …](#chapter-08-linq-to-entities-query-patterns-q6)
  - [Q7. What is the difference between `Select` projection and loadi…](#chapter-08-linq-to-entities-query-patterns-q7)
  - [Q8. What is a global query filter in EF Core?](#chapter-08-linq-to-entities-query-patterns-q8)
  - [Q9. How do you debug the SQL generated by EF Core?](#chapter-08-linq-to-entities-query-patterns-q9)

- [Chapter 09. Loading Related Data](#chapter-09-loading-related-data)
  - [Q1. What is the difference between eager loading, lazy loading, …](#chapter-09-loading-related-data-q1)
  - [Q2. How do you use `Include` and `ThenInclude` for eager loading…](#chapter-09-loading-related-data-q2)
  - [Q3. What is the N+1 problem in the context of loading related da…](#chapter-09-loading-related-data-q3)
  - [Q4. Why should you avoid lazy loading in ASP.NET Core applicatio…](#chapter-09-loading-related-data-q4)
  - [Q5. What is a cartesian explosion when including multiple collec…](#chapter-09-loading-related-data-q5)
  - [Q6. What is `AsSplitQuery`, and when should you use it?](#chapter-09-loading-related-data-q6)
  - [Q7. How do you choose between eager loading, explicit loading, a…](#chapter-09-loading-related-data-q7)

- [Chapter 10. Raw SQL & Stored Procedures](#chapter-10-raw-sql-stored-procedures)
  - [Q1. What is `FromSqlRaw` versus `FromSqlInterpolated`?](#chapter-10-raw-sql-stored-procedures-q1)
  - [Q2. Why is `FromSqlInterpolated` preferred over string interpola…](#chapter-10-raw-sql-stored-procedures-q2)
  - [Q3. When should you use raw SQL instead of LINQ in EF Core?](#chapter-10-raw-sql-stored-procedures-q3)
  - [Q4. What are the security considerations for raw SQL in EF Core?](#chapter-10-raw-sql-stored-procedures-q4)
  - [Q5. How do you call stored procedures with EF Core?](#chapter-10-raw-sql-stored-procedures-q5)
  - [Q6. How do you execute non-query raw SQL (`ExecuteSqlRaw`)?](#chapter-10-raw-sql-stored-procedures-q6)

- [Chapter 11. Change Tracking, Async & Transactions](#chapter-11-change-tracking-async-transactions)
  - [Q1. What is change tracking in EF Core?](#chapter-11-change-tracking-async-transactions-q1)
  - [Q2. What entity states does EF Core track (`Added`, `Modified`, …](#chapter-11-change-tracking-async-transactions-q2)
  - [Q3. What does `AsNoTracking` do, and when should you use it?](#chapter-11-change-tracking-async-transactions-q3)
  - [Q4. What is the performance impact of change tracking on read-he…](#chapter-11-change-tracking-async-transactions-q4)
  - [Q5. Why use async EF Core methods in ASP.NET Core?](#chapter-11-change-tracking-async-transactions-q5)
  - [Q6. How do you begin and commit a transaction in EF Core?](#chapter-11-change-tracking-async-transactions-q6)
  - [Q7. How does EF Core detect concurrency conflicts?](#chapter-11-change-tracking-async-transactions-q7)
  - [Q8. What is a concurrency token or row version column?](#chapter-11-change-tracking-async-transactions-q8)
  - [Q9. How do you handle `DbUpdateConcurrencyException`?](#chapter-11-change-tracking-async-transactions-q9)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 01. Introduction to Entity Framework Core

### Q1. What is Entity Framework Core? {#chapter-01-introduction-to-entity-framework-core-q1}

What is Entity Framework Core?

**Answer:** Entity Framework Core (EF Core) is Microsoft's modern, cross-platform Object-Relational Mapper (ORM) for .NET. It maps C# entity classes to database tables and lets you query and persist data using LINQ, with provider plugins for SQL Server, PostgreSQL, SQLite, and other databases.

- EF Core 8 is the current long-term support (LTS) line aligned with .NET 8.
- Replaces Entity Framework 6 for new development on .NET Core and modern .NET.
- Ships as NuGet packages (`Microsoft.EntityFrameworkCore`, provider packages, design-time tools).
- Supports LINQ queries, change tracking, migrations, raw SQL, and compiled models for production tuning.

---

### Q2. What is an ORM, and how does EF Core fit that definition? {#chapter-01-introduction-to-entity-framework-core-q2}

What is an ORM, and how does EF Core fit that definition?

**Answer:** An Object-Relational Mapper bridges the impedance mismatch between object-oriented C# code and relational database tables by mapping classes to tables, properties to columns, and object references to foreign keys. EF Core fits that definition by translating LINQ to SQL, materializing rows as entities, and persisting changes through a unit of work.

- You work with `Product` and `Category` objects instead of manual `INSERT` and `JOIN` strings for routine CRUD.
- Relationships express as navigation properties; EF Core generates join SQL when you query or include them.
- Schema can evolve from C# model changes through the migrations pipeline.
- The ORM layer sits between application code and ADO.NET, which still executes the final commands.

---

### Q3. What is the difference between EF Core and EF6 (Entity Framework Classic)? {#chapter-01-introduction-to-entity-framework-core-q3}

What is the difference between EF Core and EF6 (Entity Framework Classic)?

**Answer:** EF Core is a ground-up rewrite for cross-platform .NET with a modular provider model, while EF6 is the Windows-only, .NET Framework stack built on `System.Data.Entity`. EF Core prioritizes performance and cloud deployment; EF6 remains in maintenance for legacy applications.

- EF Core runs on Linux, macOS, and containers; EF6 targets .NET Framework and Windows-centric hosting.
- EF Core uses `DbContext` with fluent configuration and lightweight dependencies; EF6 uses `ObjectContext`/`DbContext` with EDMX and heavier design-time tooling.
- Some EF6 features arrived in EF Core over releases — stored procedure mapping, richer SQL Server types, and bulk operations in EF Core 7+.
- New projects on .NET 8 use EF Core; EF6 is for maintaining existing .NET Framework codebases.

---

### Q4. What is code-first versus database-first in EF Core? {#chapter-01-introduction-to-entity-framework-core-q4}

What is code-first versus database-first in EF Core?

**Answer:** Code-first starts from C# entity classes and uses migrations to create or update the database schema from the model. Database-first starts from an existing database and scaffolds entity classes and a `DbContext` from live tables using reverse engineering.

- Code-first: developers own the model in source control; `dotnet ef migrations add` generates schema scripts.
- Database-first: the database is the schema authority; `dotnet ef dbcontext scaffold` generates C# from tables.
- Both approaches use the same runtime EF Core APIs once the model exists.
- Hybrid workflows scaffold once then hand-edit partial classes, but the model and database must stay synchronized.

---

### Q5. When would you choose EF Core over ADO.NET or Dapper? {#chapter-01-introduction-to-entity-framework-core-q5}

When would you choose EF Core over ADO.NET or Dapper?

**Answer:** Choose EF Core when application logic benefits from LINQ composability, automatic change tracking, migrations, and relationship management rather than hand-written SQL for every operation. It fits domain-driven CRUD services, admin applications, and teams that want the database schema driven from C# types.

- Rapid development of standard create-read-update-delete APIs with validation and concurrency built in.
- Complex querying where LINQ composes filters, sorting, and paging without string-building SQL.
- Applications that need migration history, seed data, and repeatable schema deployment across environments.
- When relationship graphs, global query filters, and interceptors provide more value than raw SQL control.

---

### Q6. What are the trade-offs of using EF Core? {#chapter-01-introduction-to-entity-framework-core-q6}

What are the trade-offs of using EF Core?

**Answer:** EF Core trades some performance predictability and SQL transparency for productivity, abstraction, and cross-cutting features like migrations and change tracking. Misused LINQ can generate inefficient SQL; correctly used, it reduces boilerplate substantially.

- Advantage: less manual mapping code, unified model for reads and writes, strong tooling in Visual Studio and CLI.
- Disadvantage: learning curve for tracking behavior, query translation limits, and migration merge conflicts on teams.
- Heavy includes or untracked-vs-tracked confusion cause production performance issues if not understood.
- Raw SQL and Dapper remain valid escape hatches inside EF Core projects for hot paths.

---

### Q7. How does EF Core translate C# queries into SQL? {#chapter-01-introduction-to-entity-framework-core-q7}

How does EF Core translate C# queries into SQL?

**Answer:** EF Core builds an expression tree from LINQ methods chained on `IQueryable<T>`, passes it through a database provider's query compiler, and generates provider-specific SQL at execution time. The provider translates supported expression nodes; unsupported nodes cause client evaluation errors or exceptions in EF Core 3+.

- `Where`, `Select`, `OrderBy`, `Join`, and provider-specific functions like `EF.Functions.Like` translate when possible.
- Calling `ToList()`, `First()`, or `Count()` on the query triggers execution and SQL generation.
- EF Core logs generated SQL through `LogTo`, `EnableSensitiveDataLogging`, or interceptors for debugging.
- Compiled queries and EF Core 8 compiled models reduce repeated translation overhead in high-throughput apps.

---

### Q8. How does EF Core handle schema evolution? {#chapter-01-introduction-to-entity-framework-core-q8}

How does EF Core handle schema evolution?

**Answer:** EF Core uses migrations — versioned C# files with `Up` and `Down` methods that apply incremental schema changes to the database based on differences from the current model snapshot. Teams apply migrations with `dotnet ef database update` or automated deployment pipelines.

- Each migration captures model changes as operations: add column, create index, alter relationship, and similar.
- A model snapshot records the full current model for diffing the next migration.
- Alternative `EnsureCreated()` creates the database once from the model but does not support migration history or upgrades.
- Production deployments typically run migrations in controlled release steps rather than at arbitrary application startup.

---

## Chapter 02. DbContext & DbSet

### Q1. What is a `DbContext` in EF Core? {#chapter-02-dbcontext-dbset-q1}

What is a `DbContext` in EF Core?

**Answer:** `DbContext` is the primary session with the database in EF Core — it coordinates querying, change tracking, and saving through a configured model. Each instance represents a unit of work for a logical operation such as one HTTP request.

- Exposes `DbSet<T>` properties as entry points for entity operations.
- `OnModelCreating` and `OnConfiguring` define mapping and provider options.
- `SaveChanges` and `SaveChangesAsync` persist tracked changes in a single transaction by default.
- Should be short-lived and disposed to release connections back to the pool.

---

### Q2. What is a `DbSet<T>`? {#chapter-02-dbcontext-dbset-q2}

What is a `DbSet<T>`?

**Answer:** A `DbSet<T>` represents a collection of entities of type `T` mapped to a database table or view and exposes LINQ query methods plus add, update, and remove operations. It is the typed gateway for querying and mutating a specific entity type through the context.

- `context.Products` returns `DbSet<Product>` for LINQ: `Where`, `Include`, `AsNoTracking`.
- `Add`, `AddRange`, `Update`, `Remove`, and `RemoveRange` mark entities for `SaveChanges`.
- `DbSet` implements `IQueryable<T>`, so LINQ providers translate chained methods before execution.
- Under the hood, EF Core tracks entities added or queried through the context's change tracker.

---

### Q3. How do you register `DbContext` in ASP.NET Core dependency injection? {#chapter-02-dbcontext-dbset-q3}

How do you register `DbContext` in ASP.NET Core dependency injection?

**Answer:** Call `services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString))` in `Program.cs` (or `Startup.cs` on older templates). Inject `AppDbContext` into controllers, services, or minimal API handlers through constructor injection.

- Connection string comes from configuration: `builder.Configuration.GetConnectionString("Default")`.
- Provider-specific options configure retries, split queries, and command timeouts.
- Design-time factory (`IDesignTimeDbContextFactory`) supports migrations CLI without running the web host.
- Register related interceptors and `DbContext` options in the same configuration delegate when needed.

---

### Q4. Why is `DbContext` typically registered as scoped? {#chapter-02-dbcontext-dbset-q4}

Why is `DbContext` typically registered as scoped?

**Answer:** `DbContext` is not thread-safe and caches state for one logical unit of work, so it aligns with ASP.NET Core's per-request scope. A scoped registration creates one context instance per HTTP request, shared by all services in that request, then disposes it when the request completes.

- Singleton registration would share tracked entities across requests — incorrect data and thread-safety violations.
- Transient registration works but creates multiple contexts per request, breaking a single unit of work and wasting connections.
- Scoped lifetime matches `SaveChanges` boundaries: one request, one context, one commit pattern.
- Background workers use `IDbContextFactory<TContext>` to create short-lived contexts outside request scope.

---

### Q5. What is the difference between injecting `DbContext` and using `IDbContextFactory<TContext>`? {#chapter-02-dbcontext-dbset-q5}

What is the difference between injecting `DbContext` and using `IDbContextFactory<TContext>`?

**Answer:** Injecting `DbContext` directly gives the request-scoped instance managed by dependency injection. `IDbContextFactory<TContext>` creates fresh `DbContext` instances on demand, which is required for singleton services, parallel work, or long-running background tasks.

- Register factory with `AddDbContextFactory<AppDbContext>` alongside or instead of scoped `AddDbContext`.
- Factory-created contexts must be disposed explicitly: `await using var context = await factory.CreateDbContextAsync()`.
- Avoid storing injected scoped `DbContext` in singleton fields — captive dependency anti-pattern.
- Factory supports multi-threaded scenarios where each thread needs its own isolated context.

---

### Q6. What does `OnModelCreating` do in a `DbContext`? {#chapter-02-dbcontext-dbset-q6}

What does `OnModelCreating` do in a `DbContext`?

**Answer:** `OnModelCreating` is the override where you configure the entity model using the Fluent API — keys, relationships, indexes, column types, and table names — beyond what conventions infer from property names. EF Core calls it once when building the model for that context type.

- `modelBuilder.Entity<Product>().HasKey(p => p.Sku)` sets explicit keys.
- `ApplyConfigurationsFromAssembly` loads `IEntityTypeConfiguration<T>` classes for modular mapping.
- Configuration here merges with data annotations; Fluent API wins conflicts on the same facet.
- Changes require a new migration in code-first workflows to update the database schema.

---

### Q7. What is `EnsureCreated`, and how does it differ from migrations? {#chapter-02-dbcontext-dbset-q7}

What is `EnsureCreated`, and how does it differ from migrations?

**Answer:** `Database.EnsureCreated()` creates the database and schema from the current model if they do not exist, without using the migrations history table. It is intended for prototypes and tests — it cannot upgrade an existing schema when the model changes and should not replace migrations in production.

- Migrations apply incremental, versioned changes and support rollbacks via `Down` methods.
- `EnsureCreated` skips migration history — combining both on the same database causes conflicts.
- `EnsureCreatedAsync` exists for async bootstrap in test fixtures.
- Production EF Core 8 applications rely on `dotnet ef database update` or scripted migration bundles instead.

---

### Q8. Can you reuse one `DbContext` across multiple threads? {#chapter-02-dbcontext-dbset-q8}

Can you reuse one `DbContext` across multiple threads?

**Answer:** No. `DbContext` is not thread-safe — concurrent operations on the same instance cause undefined behavior, corrupted change tracking, and intermittent exceptions. Each parallel task needs its own context instance, typically from `IDbContextFactory<TContext>`.

- ASP.NET Core handles concurrency by scoping one context per request on a single thread.
- Parallel LINQ over one `DbSet` with shared context is unsafe even for read-only queries.
- Use separate contexts and merge results in memory, or serialize database access per context.
- Thread safety applies to all context operations including `SaveChanges`, queries, and explicit loading.

---

## Chapter 03. Code-First Models & Migrations

### Q1. What is code-first in EF Core? {#chapter-03-code-first-models-migrations-q1}

What is code-first in EF Core?

**Answer:** Code-first means your C# entity classes and Fluent API configuration are the source of truth for the database schema. EF Core compares the model to the database and generates migrations to create or alter tables, columns, keys, and indexes.

- Entity classes live in the application project under `Models/` or a dedicated domain layer.
- Conventions map `Id` or `{TypeName}Id` to primary keys and `{TypeName}` navigation to foreign keys.
- Data annotations (`[Required]`, `[MaxLength]`, `[Table]`) supplement or override conventions.
- The database is created or updated by applying migrations rather than manual SQL scripts in application code.

---

### Q2. What is a migration in EF Core? {#chapter-03-code-first-models-migrations-q2}

What is a migration in EF Core?

**Answer:** A migration is a named, timestamped folder containing a `Migration` class with `Up` and `Down` methods that describe schema changes, plus an updated model snapshot. It is the version-controlled record of how the database evolved to match the entity model.

- Generated by `dotnet ef migrations add MigrationName` using the design-time model diff.
- `Up` applies changes forward; `Down` reverses them for rollback scenarios.
- Applied to databases with `dotnet ef database update` or programmatic `context.Database.Migrate()`.
- Teams commit migration files to source control alongside the entity changes that caused them.

---

### Q3. How do you create and apply migrations from the CLI? {#chapter-03-code-first-models-migrations-q3}

How do you create and apply migrations from the CLI?

**Answer:** Install the EF Core tools (`dotnet tool install --global dotnet-ef`), ensure a design-time factory or startup project is configured, then run `dotnet ef migrations add InitialCreate` to scaffold a migration and `dotnet ef database update` to apply pending migrations to the target database.

- `--project` points to the project containing the `DbContext`; `--startup-project` to the executable host.
- `dotnet ef migrations list` shows applied and pending migrations against a connection.
- `dotnet ef migrations script` generates idempotent SQL for deployment pipelines that cannot run the CLI.
- EF Core 8 supports migration bundles as self-contained executables for controlled production runs.

---

### Q4. What is the difference between `Up` and `Down` in a migration? {#chapter-03-code-first-models-migrations-q4}

What is the difference between `Up` and `Down` in a migration?

**Answer:** The `Up` method contains operations applied when moving forward to this migration — creating tables, adding columns, creating indexes. The `Down` method reverses those operations so the database can roll back to the prior migration state.

- `dotnet ef database update` runs `Up` on each pending migration in order.
- `dotnet ef database update PreviousMigrationName` runs `Down` on migrations after the target.
- Hand-editing `Up`/`Down` is acceptable for data fixes or complex transformations not inferred automatically.
- Production rollbacks often prefer forward-fix migrations over running `Down` on live data.

---

### Q5. What is a model snapshot in EF Core migrations? {#chapter-03-code-first-models-migrations-q5}

What is a model snapshot in EF Core migrations?

**Answer:** The model snapshot (`*ModelSnapshot.cs`) is a serialized picture of the entire EF Core model after all migrations to date. The tooling diffs the current entity model against this snapshot to generate the next migration's operations.

- Updated automatically every time you add a migration — do not edit manually except in merge conflict resolution.
- Without an accurate snapshot, the next migration may emit duplicate or missing operations.
- Merge conflicts in snapshot files require careful resolution so the model matches the team's intended state.
- The snapshot is design-time metadata; it is not executed at application runtime.

---

### Q6. What happens if you change a model without creating a migration? {#chapter-03-code-first-models-migrations-q6}

What happens if you change a model without creating a migration?

**Answer:** The C# model and the database schema drift apart — runtime queries may fail against missing columns, or EF Core may read wrong types from stale columns. `SaveChanges` might succeed on some properties while others silently map to nonexistent or mismatched columns until a query exposes the error.

- Development databases appear to work until a new property is queried or persisted.
- Other developers' databases remain on the old schema until they apply pending migrations.
- Production deployments without migration steps cause startup failures or data corruption.
- Always add and apply a migration when changing keys, relationships, required fields, or column types.

---

### Q7. What is the difference between `EnsureCreated` and migrations-based schema creation? {#chapter-03-code-first-models-migrations-q7}

What is the difference between `EnsureCreated` and migrations-based schema creation?

**Answer:** `EnsureCreated` builds the database from the current model in one step with no migration history table, suitable for throwaway tests. Migrations apply ordered, incremental changes tracked in `__EFMigrationsHistory`, supporting upgrades, team collaboration, and production deployment.

- `EnsureCreated` fails or behaves unpredictably if the database already exists with a different shape.
- Migrations support evolving a live database through dozens of releases without dropping data.
- Never mix `EnsureCreated` and migrations on the same database — EF Core documents this as unsupported.
- Integration tests often use `EnsureCreated` or `EnsureDeleted` plus `EnsureCreated`; production uses migrations.

---

### Q8. When should migrations run automatically in production? {#chapter-03-code-first-models-migrations-q8}

When should migrations run automatically in production?

**Answer:** Migrations should run in controlled deployment steps — pipeline job, init container, or maintenance window — not blindly on every application instance startup. Automatic startup migration risks race conditions when multiple nodes start simultaneously and makes rollbacks harder to coordinate.

- Preferred: run `dotnet ef database update`, a migration bundle, or generated SQL script once per release before or during traffic switch.
- Acceptable: single-instance admin service calls `context.Database.Migrate()` when no other writer competes.
- Avoid: every pod in a Kubernetes deployment racing to migrate on boot without coordination.
- Always back up production databases before applying migrations that drop columns or reshape data.

---

## Chapter 04. Database-First & Reverse Engineering

### Q1. What is database-first in EF Core? {#chapter-04-database-first-reverse-engineering-q1}

What is database-first in EF Core?

**Answer:** Database-first in EF Core means the relational schema already exists — often maintained by database administrators or legacy systems — and you generate C# entity classes and a `DbContext` from live tables using reverse engineering (scaffolding). The database remains the authoritative schema definition.

- Common when integrating with existing enterprise databases or stored-procedure-heavy systems.
- Scaffolding reads table metadata through the provider's information schema queries.
- Generated code reflects current columns, keys, and relationships as EF Core interprets them.
- Ongoing schema changes require re-scaffolding or manual model updates to stay aligned.

---

### Q2. How do you scaffold a `DbContext` from an existing database? {#chapter-04-database-first-reverse-engineering-q2}

How do you scaffold a `DbContext` from an existing database?

**Answer:** Run `dotnet ef dbcontext scaffold "<connection-string>" Microsoft.EntityFrameworkCore.SqlServer` (or another provider package) with options for output directory, context name, and table filters. The command generates entity classes and a `DbContext` with `DbSet` properties and Fluent configuration.

- `--output-dir Models` and `--context-dir Data` organize generated files.
- `--tables Orders,Products` limits scaffolding to specific tables; `--schema dbo` filters by schema.
- `--data-annotations` emits attributes instead of fluent calls in `OnModelCreating`.
- Use `--force` to overwrite prior scaffold output after intentional regeneration.

---

### Q3. When is database-first preferred over code-first? {#chapter-04-database-first-reverse-engineering-q3}

When is database-first preferred over code-first?

**Answer:** Database-first fits when the database predates the application, multiple clients share one schema, or organizational policy requires database administrators to own all structural changes. It also suits reporting over vendor databases where you cannot dictate schema from C#.

- Legacy modernization layers a .NET API over unchanged SQL Server schemas.
- Regulated environments where schema changes go through database change advisory boards, not application migrations alone.
- Read-heavy integration with third-party databases where scaffolding is faster than manual model authoring.
- Less ideal for greenfield apps where the team wants model-driven evolution entirely in the .NET repository.

---

### Q4. What are the limitations of reverse-engineered models? {#chapter-04-database-first-reverse-engineering-q4}

What are the limitations of reverse-engineered models?

**Answer:** Scaffolded models reflect the database at one point in time and may include naming mismatches, missing navigation ergonomics, and no domain logic. They can misinterpret views, triggers, complex keys, or provider-specific types without manual correction.

- Table and column names map literally — `cust_nm` becomes awkward property names unless renamed in partial classes.
- Not all database constructs scaffold cleanly: table-valued functions, certain composite keys, or undocumented views.
- Re-scaffolding overwrites generated files unless you isolate custom code in partial classes.
- Scaffolding does not capture business rules enforced only in triggers or check constraints as C# validation.

---

### Q5. How do you refresh a scaffolded model after database schema changes? {#chapter-04-database-first-reverse-engineering-q5}

How do you refresh a scaffolded model after database schema changes?

**Answer:** Re-run `dotnet ef dbcontext scaffold` with the same options and `--force` to regenerate entities and context, then merge any custom partial class extensions that were not overwritten. Alternatively, hand-edit the model and add a code-first migration if you have switched to owning schema from the application.

- Compare diffs carefully — regenerated files replace prior scaffold output entirely.
- Keep custom logic in `*.Partial.cs` files or separate configuration classes excluded from overwrite.
- For small changes, manual entity updates plus a migration may be less disruptive than full re-scaffold.
- Document the scaffold command in the repository so the team reproduces identical output.

---

### Q6. Can you combine scaffolded models with manual partial classes? {#chapter-04-database-first-reverse-engineering-q6}

Can you combine scaffolded models with manual partial classes?

**Answer:** Yes. EF Core scaffolding generates partial classes intentionally — you add `Product.Partial.cs` with the same `partial class Product` to attach computed properties, methods, or interfaces without touching regenerated files. Partial `DbContext` classes extend `OnModelCreating` with hand-written Fluent API.

- Regenerated scaffold files stay overwrite-safe; partials persist across re-scaffold.
- Use partials for `[NotMapped]` properties, validation helpers, and domain behavior.
- Additional `IEntityTypeConfiguration<T>` classes configure mapping without editing generated context code.
- This hybrid pattern is the standard way to maintain database-first models long term.

---

## Chapter 05. CRUD Operations & SaveChanges

### Q1. How do you insert, update, and delete entities with EF Core? {#chapter-05-crud-operations-savechanges-q1}

How do you insert, update, and delete entities with EF Core?

**Answer:** Add new entities with `context.Set<T>().Add(entity)` or `AddRange`, mark updates by modifying tracked entities or calling `Update`, remove with `Remove` or `RemoveRange`, then persist with `await context.SaveChangesAsync()`. EF Core generates insert, update, and delete statements from change tracker state.

- Insert: create object, `Add`, `SaveChanges` — identity keys populate after save when configured as store-generated.
- Update: query entity, mutate properties, `SaveChanges` — only changed columns appear in `UPDATE` when tracking detects modifications.
- Delete: `Remove(entity)` marks `Deleted`; `SaveChanges` issues `DELETE`.
- Disconnected updates from APIs attach or use `Update` with caution to avoid overwriting unchanged columns.

---

### Q2. What does `SaveChanges()` do? {#chapter-05-crud-operations-savechanges-q2}

What does `SaveChanges()` do?

**Answer:** `SaveChanges` (and `SaveChangesAsync`) commits all pending changes tracked by the context in a single database transaction by default. It generates SQL for added, modified, and deleted entities, executes it in dependency order, and updates store-generated values such as identity columns and row versions.

- Returns the number of state entries written to the database.
- Detects relationship changes and orders inserts to satisfy foreign key constraints.
- Raises `SavingChanges` and `SavedChanges` events and runs interceptors before and after persistence.
- If any statement fails, the transaction rolls back and no partial commit occurs within that `SaveChanges` call.

---

### Q3. What is the difference between `Add`, `Update`, and `Remove` on a `DbSet`? {#chapter-05-crud-operations-savechanges-q3}

What is the difference between `Add`, `Update`, and `Remove` on a `DbSet`?

**Answer:** `Add` marks an entity as `Added` for insert on save. `Update` marks every mapped property as `Modified` for a full update (or attaches disconnected graphs as modified). `Remove` marks the entity as `Deleted` for delete on save.

- `Add` on an entity with an existing key value may throw or behave as update depending on key configuration — typically use `Add` only for new keys.
- `Update` is convenient for disconnected Data Transfer Objects from HTTP PUT but can overwrite columns not sent in the payload.
- `Remove` requires the entity to be known to the context — attach first if it came from the client with only an id.
- `Attach` plus setting `EntityState` manually offers finer control than blanket `Update`.

---

### Q4. What is attach versus add when working with disconnected entities? {#chapter-05-crud-operations-savechanges-q4}

What is attach versus add when working with disconnected entities?

**Answer:** `Add` tells EF Core the entity is new and should be inserted. `Attach` registers an existing entity with the context as `Unchanged` without inserting — you then mark specific properties or the whole entity `Modified` for targeted updates.

- Web APIs often receive ids from clients — `Attach` plus `Modified` state updates without a prior query.
- `Add` on an entity with a non-zero key may attempt insert and violate primary key constraints.
- `context.Entry(entity).Property(e => e.Name).IsModified = true` updates one column after attach.
- `Update` is shorthand for attach-all-properties-as-modified on disconnected instances.

---

### Q5. What is `ExecuteUpdate` / `ExecuteDelete` in EF Core 7+? {#chapter-05-crud-operations-savechanges-q5}

What is `ExecuteUpdate` / `ExecuteDelete` in EF Core 7+?

**Answer:** `ExecuteUpdate` and `ExecuteDelete` translate LINQ filters directly into SQL `UPDATE` and `DELETE` statements without loading entities into memory or running change tracking. EF Core 8 continues these APIs for high-performance bulk mutations on `IQueryable`.

- Example: `await context.Products.Where(p => p.Discontinued).ExecuteDeleteAsync()`.
- Example: `await context.Products.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, newPrice))`.
- Bypasses the change tracker — no entity instances materialized, lower memory and CPU.
- Does not run domain logic on entities, cascade through application-level validators, or update navigation fix-up in memory.

---

### Q6. What is the unit-of-work pattern in relation to `DbContext`? {#chapter-05-crud-operations-savechanges-q6}

What is the unit-of-work pattern in relation to `DbContext`?

**Answer:** The unit-of-work pattern groups multiple repository operations into one atomic persistence boundary. In EF Core, `DbContext` implements this naturally — all tracked changes across `DbSet` operations commit together in one `SaveChanges` call inside one transaction.

- One request handler accumulates adds, updates, and deletes then calls `SaveChangesAsync` once.
- Multiple `SaveChanges` calls in one context create separate transactions unless wrapped in an explicit transaction.
- Repositories sharing one injected context participate in the same unit of work automatically.
- Failure during `SaveChanges` rolls back the entire batch, preserving consistency across related tables.

---

### Q7. How do you perform a bulk update without loading entities into memory? {#chapter-05-crud-operations-savechanges-q7}

How do you perform a bulk update without loading entities into memory?

**Answer:** Use `ExecuteUpdateAsync` (EF Core 7+) with a filtered `IQueryable` to push updates to the database in one SQL statement. For very large batches or provider-specific optimizations, raw SQL via `ExecuteSqlRawAsync` or third-party bulk extension libraries remain options.

- `ExecuteUpdateAsync` with `SetProperty` sets column values in SQL without selecting rows into the context.
- Ideal for flag updates, soft deletes, and price adjustments across many matching rows.
- Does not invoke interceptors or tracked-entity events per row the way individual updates would.
- When complex per-row logic is required in C#, batch with smaller queries or use keyset pagination instead of loading entire tables.

---

---

## Chapter 06. Relationships & Navigation Properties

### Q1. What is a navigation property in EF Core? {#chapter-06-relationships-navigation-properties-q1}

What is a navigation property in EF Core?

**Answer:** A navigation property is a CLR property on an entity that represents a relationship to one or more related entities — a reference to a single related row or a collection of related rows. EF Core uses navigation properties to traverse associations in LINQ and to load related data through includes, lazy loading, or explicit loading.

- A reference navigation points to one related entity (for example `Order.Customer`).
- A collection navigation points to many related entities (for example `Customer.Orders`).
- Navigation properties are optional in the model but enable relationship traversal without manually joining foreign keys in every query.
- EF Core infers relationships from navigation properties paired with foreign key properties or configures them explicitly via Fluent API or data annotations.

---

### Q2. What is the difference between one-to-many and many-to-many relationships? {#chapter-06-relationships-navigation-properties-q2}

What is the difference between one-to-many and many-to-many relationships?

**Answer:** A one-to-many relationship means one parent entity relates to many child entities through a foreign key on the child side. A many-to-many relationship means entities on both sides can relate to multiple rows on the other side, typically modeled with a join entity or implicit join table in EF Core 5+.

- One-to-many: the child table holds the foreign key (`Order.CustomerId` → `Customer.Id`).
- Many-to-many: neither side stores the other's key directly — EF Core 5+ can use a join entity or an implicit join table with two foreign keys.
- One-to-many navigation is usually a single reference on the many side and a collection on the one side.
- Many-to-many navigation is a collection on both sides (`Student.Courses` and `Course.Students`).

---

### Q3. How do you configure a one-to-many relationship in code-first? {#chapter-06-relationships-navigation-properties-q3}

How do you configure a one-to-many relationship in code-first?

**Answer:** EF Core can infer one-to-many from navigation and foreign key properties by convention, or you configure it explicitly with Fluent API or data annotations when conventions are insufficient. The child entity must expose a foreign key property or shadow foreign key pointing to the parent primary key.

- By convention: add `public int CustomerId { get; set; }` and `public Customer Customer { get; set; }` on `Order` — EF Core wires the relationship automatically.
- Fluent API: `modelBuilder.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);`
- Data annotations: `[ForeignKey(nameof(Customer))]` on the FK property or `[InverseProperty]` when multiple relationships exist between the same types.
- Required versus optional relationships are controlled with `.IsRequired()` or nullable foreign key types.

---

### Q4. What is a foreign key property in EF Core? {#chapter-06-relationships-navigation-properties-q4}

What is a foreign key property in EF Core?

**Answer:** A foreign key property is the scalar column on the dependent entity that stores the primary key value of the related principal entity. EF Core maps it to a database FK constraint and uses it for relationship fix-up, cascade rules, and LINQ joins.

- It can be a CLR property (`public int CustomerId`) or a shadow property configured only in the model.
- The dependent entity is the side that holds the foreign key — in one-to-many, the "many" side typically owns the FK column.
- Changing the FK value reassigns the relationship without necessarily loading the related navigation object.
- Composite foreign keys require multiple properties and explicit configuration in Fluent API.

---

### Q5. What is cascade delete in EF Core? {#chapter-06-relationships-navigation-properties-q5}

What is cascade delete in EF Core?

**Answer:** Cascade delete means when the principal (parent) entity is deleted, EF Core automatically deletes dependent (child) entities that reference it, matching `ON DELETE CASCADE` in the database. It is the default for required relationships in many configurations.

- Configured with `OnDelete(DeleteBehavior.Cascade)` in Fluent API or by convention for required FKs.
- When you call `Remove(parent)`, EF Core marks related children as `Deleted` if cascade is enabled.
- The database schema must also define the matching FK cascade rule for deletes executed outside EF Core.
- `DeleteBehavior.ClientCascade` deletes dependents in memory without relying on database cascade — used in specific scenarios.

---

### Q6. When would you disable cascade delete? {#chapter-06-relationships-navigation-properties-q6}

When would you disable cascade delete?

**Answer:** Disable cascade delete when deleting a parent must not automatically remove children — for example when child records have independent business meaning, audit requirements, or soft-delete policies. Use `DeleteBehavior.Restrict` or `NoAction` to force explicit handling of dependents before deleting the principal.

- Lookup or reference tables where child rows should block parent deletion (`Restrict` throws on delete if dependents exist).
- Soft-delete patterns where children remain linked to an archived parent instead of being physically removed.
- Legacy databases where FK constraints use `NO ACTION` and database-enforced rules differ from EF defaults.
- Explicit orphan management gives clearer error messages and audit trails than silent cascade removal.

---

### Q7. What is a many-to-many relationship in EF Core 5+? {#chapter-06-relationships-navigation-properties-q7}

What is a many-to-many relationship in EF Core 5+?

**Answer:** In EF Core 5 and later, many-to-many can be modeled with skip navigation properties on both entities and an implicit join table managed by EF Core, without requiring a dedicated join entity class. EF Core creates and maps the join table automatically based on conventions.

- Both entities expose collection navigations (`Post.Tags` and `Tag.Posts`).
- EF Core generates a hidden join table (for example `PostTag`) with composite foreign keys to both sides.
- You can expose the join entity explicitly when you need extra columns on the link (for example `AssignedDate` on a `CourseEnrollment` entity).
- Migrations create the join table schema; querying either collection loads related entities through the join.

---

## Chapter 07. Fluent API & Data Annotations

### Q1. What is the Fluent API in EF Core? {#chapter-07-fluent-api-data-annotations-q1}

What is the Fluent API in EF Core?

**Answer:** The Fluent API is a code-based configuration surface in `OnModelCreating` (or `IEntityTypeConfiguration<T>` classes) that describes the EF Core model using method chains instead of attributes on entity classes. It controls tables, keys, properties, relationships, indexes, and constraints with full expressiveness.

- Called on `ModelBuilder` — for example `modelBuilder.Entity<Product>().ToTable("Products").HasKey(p => p.Id);`
- Supports configurations that have no data annotation equivalent (composite keys, complex relationship rules, value conversions).
- Keeps persistence concerns out of domain entity classes when you prefer POCOs without attributes.
- `ApplyConfigurationsFromAssembly` discovers and applies all `IEntityTypeConfiguration<T>` implementations in an assembly.

---

### Q2. When should you prefer Fluent API over data annotations? {#chapter-07-fluent-api-data-annotations-q2}

When should you prefer Fluent API over data annotations?

**Answer:** Prefer Fluent API when configuration is complex, affects multiple types, has no annotation equivalent, or when you want to keep entity classes free of persistence attributes. Data annotations suit simple, visible constraints on individual properties.

- Composite keys, alternate keys, owned types, and detailed cascade/index tuning require Fluent API.
- Large models benefit from separate configuration classes rather than cluttering entities with `[Column]`, `[ForeignKey]`, and `[Index]` on every property.
- Cross-cutting rules (global naming, soft-delete filters, shared base configurations) belong in Fluent API or configuration classes.
- Data annotations remain fine for basic validation attributes (`[Required]`, `[MaxLength]`) that double as API validation in ASP.NET Core.

---

### Q3. What is `IEntityTypeConfiguration<T>`? {#chapter-07-fluent-api-data-annotations-q3}

What is `IEntityTypeConfiguration<T>`?

**Answer:** `IEntityTypeConfiguration<T>` is an interface for encapsulating Fluent API configuration for a single entity type in its own class, implementing `Configure(EntityTypeBuilder<T> builder)`. It keeps `OnModelCreating` clean and groups all mapping rules for one entity in one place.

- Example: `public class OrderConfiguration : IEntityTypeConfiguration<Order> { public void Configure(EntityTypeBuilder<Order> builder) { ... } }`
- Register with `modelBuilder.ApplyConfiguration(new OrderConfiguration())` or `ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)`.
- Each configuration class owns table name, keys, properties, relationships, and indexes for that entity.
- Supports testing and reuse — the same configuration applies across multiple `DbContext` types if needed.

---

### Q4. How do Fluent API and annotations interact when both configure the same property? {#chapter-07-fluent-api-data-annotations-q4}

How do Fluent API and annotations interact when both configure the same property?

**Answer:** When Fluent API and data annotations configure the same aspect of the model, Fluent API takes precedence and overrides the annotation. EF Core merges configurations but resolves conflicts in favor of explicit Fluent API calls.

- If `[MaxLength(100)]` is on a property and Fluent API sets `.HasMaxLength(200)`, the effective limit is 200.
- Avoid duplicating the same rule in both places — pick one source of truth per property to prevent confusion during maintenance.
- Some annotations (for example `[Key]`) and Fluent API can coexist when they agree; conflicting values silently favor Fluent API.
- Review generated migrations when changing configuration sources to ensure schema matches intent.

---

### Q5. How do you configure indexes with Fluent API? {#chapter-07-fluent-api-data-annotations-q5}

How do you configure indexes with Fluent API?

**Answer:** Define indexes in Fluent API with `HasIndex` on the entity builder, optionally marking them unique or naming them explicitly. Indexes speed up queries on filtered, sorted, or joined columns and enforce uniqueness at the database level.

- Single column: `builder.HasIndex(p => p.Sku);`
- Composite: `builder.HasIndex(p => new { p.LastName, p.FirstName });`
- Unique: `builder.HasIndex(p => p.Email).IsUnique();`
- Named index: `.HasDatabaseName("IX_Product_Sku")` — migrations emit `CREATE INDEX` statements accordingly.

---

## Chapter 08. LINQ to Entities & Query Patterns

### Q1. What is the difference between LINQ to Objects and LINQ to Entities? {#chapter-08-linq-to-entities-query-patterns-q1}

What is the difference between LINQ to Objects and LINQ to Entities?

**Answer:** LINQ to Entities translates query expressions into SQL executed by the database provider, while LINQ to Objects runs operators in memory against CLR collections after data is already loaded. EF Core queries against `DbSet<T>` use LINQ to Entities until the query is materialized.

- LINQ to Entities: `context.Orders.Where(o => o.Total > 100)` becomes a SQL `WHERE` clause — filtering happens on the server.
- LINQ to Objects: after `ToList()`, subsequent `.Where` with non-translatable logic runs in application memory.
- Only expression-tree-compatible operations translate; arbitrary C# methods generally do not unless mapped via `EF.Functions` or user-defined function mapping.
- Accidentally switching to LINQ to Objects (via `AsEnumerable()` or early `ToList()`) pulls more data than intended.

---

### Q2. What is `IQueryable<T>` versus `IEnumerable<T>` in EF Core queries? {#chapter-08-linq-to-entities-query-patterns-q2}

What is `IQueryable<T>` versus `IEnumerable<T>` in EF Core queries?

**Answer:** `IQueryable<T>` represents a composable, deferred query that EF Core can still translate and combine into SQL before execution. `IEnumerable<T>` represents an in-memory sequence where further LINQ operators execute locally in .NET after materialization.

- `DbSet<T>` implements `IQueryable<T>` — each `.Where`, `.Select`, or `.OrderBy` extends the expression tree sent to the provider.
- Calling `ToList()`, `ToArray()`, or iterating with `foreach` executes the query and returns `IEnumerable<T>` backed by loaded objects.
- Returning `IQueryable<T>` from a repository allows callers to add filters that still translate to SQL — returning `IEnumerable<T>` does not.
- `AsQueryable()` on an in-memory list wraps it as `IQueryable` but uses LINQ to Objects, not EF Core translation.

---

### Q3. When does query execution actually occur (deferred execution)? {#chapter-08-linq-to-entities-query-patterns-q3}

When does query execution actually occur (deferred execution)?

**Answer:** EF Core defers execution until the query is enumerated or explicitly materialized — for example by calling `ToList()`, `First()`, `Count()`, `foreach`, or async equivalents like `ToListAsync()`. Building the query with chained operators does not hit the database until that terminal operation runs.

- `var q = context.Products.Where(p => p.Active);` — no SQL yet; `q` is an unevaluated `IQueryable`.
- `await q.ToListAsync()` — EF Core generates and executes SQL at this point.
- Multiple enumerations of the same `IEnumerable` result re-query unless cached; `IQueryable` re-executes on each materialization.
- `SaveChanges` is separate from query execution — it persists tracked changes, not read queries.

---

### Q4. What is the N+1 query problem? {#chapter-08-linq-to-entities-query-patterns-q4}

What is the N+1 query problem?

**Answer:** The N+1 problem occurs when one query loads a list of N parent entities and then accessing a related navigation property triggers one additional query per parent, totaling N+1 round-trips. It devastates API latency and database load under even moderate list sizes.

- Classic pattern: load 100 orders, then loop and read `order.Lines` — 1 + 100 queries.
- Caused by lazy loading, missing `Include`, or projection that omits needed related data.
- Fix with eager loading (`Include`), split queries, or a single `Select` projection that joins required fields.
- EF Core logging showing the same SQL template repeated with different parameter IDs is the usual diagnostic signal.

---

### Q5. What causes client evaluation warnings or errors in EF Core? {#chapter-08-linq-to-entities-query-patterns-q5}

What causes client evaluation warnings or errors in EF Core?

**Answer:** Client evaluation happens when part of a LINQ expression cannot be translated to SQL and EF Core either throws (EF Core 3+) or warns and executes that portion in memory after fetching data. Non-translatable methods, custom delegates, and complex C# logic in `Where` or `Select` are common triggers.

- Calling instance methods, arbitrary lambdas, or .NET-only APIs inside queries often fails translation.
- `AsEnumerable()` before `Where` forces all prior data into memory, then filters client-side.
- EF Core 3+ throws `InvalidOperationException` for many patterns that EF6 silently client-evaluated — safer but requires query rewrites.
- Use `EF.Functions.Like`, provider-translatable methods, raw SQL, or database functions for logic the provider cannot express.

---

### Q6. How do you filter, project, sort, and paginate with EF Core LINQ? {#chapter-08-linq-to-entities-query-patterns-q6}

How do you filter, project, sort, and paginate with EF Core LINQ?

**Answer:** Compose standard LINQ operators on `IQueryable` so EF Core translates them to SQL: `Where` for filtering, `Select` for projection, `OrderBy`/`ThenBy` for sorting, and `Skip`/`Take` for pagination. Execute asynchronously in ASP.NET Core with `ToListAsync` or similar terminal methods.

- Filter: `context.Products.Where(p => p.CategoryId == id && p.Active)`
- Project: `.Select(p => new ProductDto { Id = p.Id, Name = p.Name })` — loads only needed columns.
- Sort: `.OrderBy(p => p.Name).ThenByDescending(p => p.CreatedUtc)`
- Paginate: `.Skip((page - 1) * pageSize).Take(pageSize)` — always pair with a stable `OrderBy` to avoid inconsistent pages.

---

### Q7. What is the difference between `Select` projection and loading full entities? {#chapter-08-linq-to-entities-query-patterns-q7}

What is the difference between `Select` projection and loading full entities?

**Answer:** `Select` projection translates to a SQL query that retrieves only the columns needed for the result shape, avoiding full entity materialization and change tracking. Loading full entities fetches all mapped columns and registers each row in the change tracker by default.

- Projection to DTOs or anonymous types reduces network I/O and memory — ideal for read-only API responses.
- Full entity queries return tracked `Product` instances suitable for updates via `SaveChanges`.
- Projection can flatten related data in one query: `.Select(o => new { o.Id, CustomerName = o.Customer.Name })`.
- `AsNoTracking()` on full-entity reads reduces tracking overhead but still loads all columns unlike targeted projection.

---

### Q8. What is a global query filter in EF Core? {#chapter-08-linq-to-entities-query-patterns-q8}

What is a global query filter in EF Core?

**Answer:** A global query filter is a LINQ predicate applied automatically to every query for an entity type, configured in `OnModelCreating` with `HasQueryFilter`. It enforces row-level rules such as soft delete, multi-tenancy, or active-record flags without repeating `Where` in every query.

- Example: `builder.HasQueryFilter(p => !p.IsDeleted);` — all queries exclude soft-deleted rows unless ignored.
- Tenant isolation: `builder.HasQueryFilter(o => o.TenantId == _tenantId);` when `_tenantId` is captured from a scoped service.
- Bypass with `IgnoreQueryFilters()` for admin or audit scenarios that must see all rows.
- Filters compose with user-specified `Where` clauses — both predicates appear in generated SQL.

---

### Q9. How do you debug the SQL generated by EF Core? {#chapter-08-linq-to-entities-query-patterns-q9}

How do you debug the SQL generated by EF Core?

**Answer:** Enable sensitive logging and log EF Core database commands to the console or your logging provider, or inspect queries with `ToQueryString()` on an `IQueryable` before execution. These tools show the exact SQL, parameters, and command timing during development.

- `optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)` or `EnableSensitiveDataLogging()` in development (never log parameter values in production indiscriminately).
- `var sql = query.ToQueryString();` — returns the translated SQL string for inspection without executing.
- Application Insights, Serilog, or `Microsoft.EntityFrameworkCore.Database.Command` log category capture command text at Information level.
- Tags and `TagWith("MyQuery")` annotate SQL comments so you can correlate logged statements to code locations.

---

## Chapter 09. Loading Related Data

### Q1. What is the difference between eager loading, lazy loading, and explicit loading? {#chapter-09-loading-related-data-q1}

What is the difference between eager loading, lazy loading, and explicit loading?

**Answer:** Eager loading fetches related data in the same query (or coordinated queries) up front using `Include`. Lazy loading fetches related data automatically when a navigation property is first accessed. Explicit loading runs a separate query on demand via `Entry(...).Collection(...).Load()` or `Reference(...).Load()`.

- Eager: `context.Orders.Include(o => o.Lines)` — related data available immediately, no extra queries on access.
- Lazy: requires proxies or lazy-loading proxies package; triggers SQL on first navigation access — risky after context disposal.
- Explicit: start with a stub entity or partial load, then call `Load()` when you know you need the related data.
- Eager and explicit give predictable query counts; lazy makes query count depend on code paths and property access order.

---

### Q2. How do you use `Include` and `ThenInclude` for eager loading? {#chapter-09-loading-related-data-q2}

How do you use `Include` and `ThenInclude` for eager loading?

**Answer:** Chain `Include` on the root `DbSet` query to load related entities, and use `ThenInclude` to load deeper levels along a navigation path. EF Core translates includes into SQL JOINs or split queries depending on configuration.

- One level: `context.Orders.Include(o => o.Customer)`
- Deeper: `context.Orders.Include(o => o.Lines).ThenInclude(l => l.Product)`
- Multiple branches: repeat `Include` from the root — `Include(o => o.Customer).Include(o => o.Lines)`
- Filtered includes (EF Core 5+): `.Include(o => o.Lines.Where(l => l.Active))` loads only matching related rows.

---

### Q3. What is the N+1 problem in the context of loading related data? {#chapter-09-loading-related-data-q3}

What is the N+1 problem in the context of loading related data?

**Answer:** When listing parent entities without loading related navigations, each access to a child collection or reference in a loop fires a separate SQL query — one initial query plus N per-row queries. This is the relational-data manifestation of the N+1 anti-pattern.

- Occurs with lazy loading enabled or when developers forget `Include` on list endpoints.
- A 50-row list with one navigation access per row becomes 51 database round-trips per HTTP request.
- Detect via EF Core command logging or APM tools showing repeated identical queries with different keys.
- Resolve with `Include`, `AsSplitQuery`, projection, or batch explicit loading before the loop.

---

### Q4. Why should you avoid lazy loading in ASP.NET Core applications? {#chapter-09-loading-related-data-q4}

Why should you avoid lazy loading in ASP.NET Core applications?

**Answer:** Lazy loading triggers database queries during navigation property access, which often happens during JSON serialization or view rendering after the request-scoped `DbContext` is disposed or without the developer's explicit awareness. ASP.NET Core's stateless request model makes implicit loading unpredictable and expensive.

- Serializers accessing navigations cause "Cannot access a disposed context" or hidden N+1 query storms.
- Query count becomes data-dependent — endpoints perform differently based on which properties the client touches.
- Explicit `Include` or projection makes API contracts and performance predictable and testable.
- Lazy loading suits long-lived desktop contexts with UI-driven access patterns, not short HTTP request scopes.

---

### Q5. What is a cartesian explosion when including multiple collections? {#chapter-09-loading-related-data-q5}

What is a cartesian explosion when including multiple collections?

**Answer:** Cartesian explosion happens when a single SQL query JOINs two or more collection navigations, producing a rowset whose size is roughly the product of collection cardinalities — many duplicate parent rows over the wire before EF Core deduplicates in memory.

- Example: `Include(o => o.Lines).Include(o => o.Shipments)` on orders with 20 lines and 10 shipments can emit ~200 rows per order in SQL.
- Network and memory costs spike even though the final object graph has far fewer unique entities.
- EF Core fix-up reconstructs parents correctly, but the damage is already done at the SQL transport layer.
- Mitigate with `AsSplitQuery()`, separate queries, or projection to DTOs that avoid multi-collection joins.

---

### Q6. What is `AsSplitQuery`, and when should you use it? {#chapter-09-loading-related-data-q6}

What is `AsSplitQuery`, and when should you use it?

**Answer:** `AsSplitQuery()` tells EF Core to load included related data using multiple SQL queries instead of one large JOIN, avoiding cartesian explosion when including multiple collection navigations. Each include path gets its own SELECT while EF Core still assembles the object graph.

- Use when eager-loading two or more `Include` collection branches on the same root entity.
- Slightly more round-trips than a single join but far less data transferred when collections are large.
- Can be set globally via `UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)` or per query with `.AsSplitQuery()`.
- Single-reference includes (many-to-one) rarely need split queries — the problem is primarily multi-collection joins.

---

### Q7. How do you choose between eager loading, explicit loading, and projection? {#chapter-09-loading-related-data-q7}

How do you choose between eager loading, explicit loading, and projection?

**Answer:** Choose based on how much related data you know you need at query time and whether the operation is read-only or tracked. Eager loading suits known graphs up front; explicit loading suits conditional related data; projection suits read-only API responses that need flat or partial shapes.

- Eager (`Include`): list/detail endpoints where the response always needs the same related entities.
- Explicit (`Load`): related data needed only in some branches — load when a flag or business rule triggers the need.
- Projection (`Select` to DTO): read-only APIs that never update entities — smallest payload, no tracking, no N+1.
- Avoid lazy loading in web APIs; combine split queries when eager-loading multiple collections.

---

## Chapter 10. Raw SQL & Stored Procedures

### Q1. What is `FromSqlRaw` versus `FromSqlInterpolated`? {#chapter-10-raw-sql-stored-procedures-q1}

What is `FromSqlRaw` versus `FromSqlInterpolated`?

**Answer:** Both methods query entities with raw SQL instead of LINQ, but `FromSqlRaw` accepts a plain string (with optional `SqlParameter` objects) while `FromSqlInterpolated` accepts a `FormattableString` that EF Core converts into parameterized SQL. `FromSqlInterpolated` prevents accidental injection from interpolated values.

- `FromSqlRaw("SELECT * FROM Products WHERE CategoryId = {0}", id)` — placeholders become parameters when passed correctly.
- `FromSqlInterpolated($"SELECT * FROM Products WHERE CategoryId = {id}")` — compiler-generated `FormattableString` ensures parameterization.
- Never pass an ordinary interpolated string to `FromSqlRaw` — `$"..."` evaluated before the call embeds literals unsafely.
- Both require the SQL to map to the entity shape (column names compatible with the entity type).

---

### Q2. Why is `FromSqlInterpolated` preferred over string interpolation for raw SQL? {#chapter-10-raw-sql-stored-procedures-q2}

Why is `FromSqlInterpolated` preferred over string interpolation for raw SQL?

**Answer:** `FromSqlInterpolated` treats each interpolated value as a separate SQL parameter, so user input never becomes part of the SQL text. Ordinary C# string interpolation builds a single literal string before EF Core sees it, reintroducing SQL injection risk identical to concatenation.

- `FormattableString` preserves argument boundaries — EF Core emits `WHERE Id = @p0` with bound parameters.
- `$"WHERE Name = '{name}'"` passed to `FromSqlRaw` embeds raw text — attackers can inject malicious SQL.
- Parameterization also improves plan cache reuse compared to ad hoc literal SQL per distinct value.
- Same safety model as parameterized ADO.NET commands and Dapper anonymous parameter objects.

---

### Q3. When should you use raw SQL instead of LINQ in EF Core? {#chapter-10-raw-sql-stored-procedures-q3}

When should you use raw SQL instead of LINQ in EF Core?

**Answer:** Use raw SQL when LINQ cannot express the query efficiently or at all — complex reporting queries, database-specific features, optimized hints, bulk reads of views, or legacy stored procedures. Raw SQL trades provider translation and compile-time checking for full control over the statement sent to the server.

- Window functions, recursive CTEs, or vendor-specific syntax unavailable in LINQ.
- Performance-critical queries where a hand-tuned plan outperforms generated EF SQL.
- Mapping to keyless entity types or database views for read-only reporting models.
- Calling existing stored procedures the organization already maintains in the database.

---

### Q4. What are the security considerations for raw SQL in EF Core? {#chapter-10-raw-sql-stored-procedures-q4}

What are the security considerations for raw SQL in EF Core?

**Answer:** All dynamic values must reach the database as parameters, never as concatenated SQL text. Use `FromSqlInterpolated`, parameterized `FromSqlRaw`, or `ExecuteSqlRaw` with `SqlParameter` objects — and validate that no user-controlled input defines structural SQL elements (table or column names) without strict allowlisting.

- SQL injection remains possible if raw strings embed user input directly.
- `ExecuteSqlRaw` with `{0}` placeholders parameterizes values; string building before the call does not.
- Dynamic identifiers (sort columns, table names) cannot be parameterized — map user input to a fixed allowlist.
- Logging raw SQL in production should redact or avoid sensitive parameter values even when queries are parameterized.

---

### Q5. How do you call stored procedures with EF Core? {#chapter-10-raw-sql-stored-procedures-q5}

How do you call stored procedures with EF Core?

**Answer:** Map stored procedure result sets to entity types or keyless types and invoke them with `FromSqlRaw` or `FromSqlInterpolated` using `EXEC` syntax, or use `ExecuteSqlRaw` for procedures that perform non-query work. Pass parameters as method arguments or `SqlParameter` instances.

- Query returning rows: `context.Products.FromSqlRaw("EXEC GetProductsByCategory @CategoryId = {0}", categoryId)`
- Interpolated: `context.Orders.FromSqlInterpolated($"EXEC usp_GetOrders @StartDate = {start}, @EndDate = {end}")`
- Non-query procedures: `context.Database.ExecuteSqlRaw("EXEC usp_ArchiveOrders @BeforeDate = {0}", date)`
- Result shape must match entity columns; keyless types suit procedures returning non-entity projections.

---

### Q6. How do you execute non-query raw SQL (`ExecuteSqlRaw`)? {#chapter-10-raw-sql-stored-procedures-q6}

How do you execute non-query raw SQL (`ExecuteSqlRaw`)?

**Answer:** `ExecuteSqlRaw` and `ExecuteSqlInterpolated` run INSERT, UPDATE, DELETE, or DDL statements that do not return entity rows, returning the number of rows affected. They execute outside the change tracker — tracked entities in memory are not automatically updated.

- `await context.Database.ExecuteSqlRawAsync("UPDATE Products SET Active = 0 WHERE Discontinued = 1");`
- Parameterized: `ExecuteSqlRawAsync("DELETE FROM Logs WHERE CreatedUtc < {0}", cutoffDate)`
- Prefer `ExecuteUpdate`/`ExecuteDelete` (EF Core 7+) for bulk operations that EF can translate from LINQ when raw SQL is not required.
- Wrap multi-statement raw SQL in an explicit transaction when atomicity is required.

---

## Chapter 11. Change Tracking, Async & Transactions

### Q1. What is change tracking in EF Core? {#chapter-11-change-tracking-async-transactions-q1}

What is change tracking in EF Core?

**Answer:** Change tracking is EF Core's mechanism for snapshotting entity state when loaded or attached, detecting modifications, and generating INSERT, UPDATE, and DELETE statements on `SaveChanges`. The `DbContext` maintains an entry per tracked entity with current and original values.

- Enabled by default for queries that return entity types without `AsNoTracking()`.
- The change tracker compares current property values to snapshots taken at query or attach time.
- Relationships and FK changes are tracked alongside scalar properties.
- Disabling tracking (`AsNoTracking`) skips snapshot overhead for read-only scenarios.

---

### Q2. What entity states does EF Core track (`Added`, `Modified`, `Deleted`, `Unchanged`)? {#chapter-11-change-tracking-async-transactions-q2}

What entity states does EF Core track (`Added`, `Modified`, `Deleted`, `Unchanged`)?

**Answer:** EF Core assigns each tracked entity an `EntityState` describing what `SaveChanges` should do: `Added` inserts new rows, `Modified` updates changed rows, `Deleted` removes rows, and `Unchanged` skips entities with no detected changes. `Detached` means the entity is not in the tracker.

- `Add()` marks entities `Added` — INSERT on save even if they had a key value set manually.
- `Update()` marks all mapped properties `Modified` unless configured otherwise — issues a broad UPDATE.
- `Remove()` marks `Deleted` — DELETE on save; cascade rules apply to dependents.
- `Unchanged` entities are loaded but untouched; `Attach()` with unchanged values sets `Unchanged` until properties change.

---

### Q3. What does `AsNoTracking` do, and when should you use it? {#chapter-11-change-tracking-async-transactions-q3}

What does `AsNoTracking` do, and when should you use it?

**Answer:** `AsNoTracking()` tells EF Core not to snapshot or track entities returned by a query, reducing memory and CPU for read-only operations. Use it on list endpoints, reports, and any query whose results will not be updated through the same `DbContext` instance.

- Tracked queries store original values for every property — unnecessary when you only serialize to JSON.
- `AsNoTrackingWithIdentityResolution()` deduplicates repeated instances in a single result without full change tracking.
- Set globally: `options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` for read-heavy apps.
- Omit `AsNoTracking` when you load entities specifically to modify and call `SaveChanges` on the same context.

---

### Q4. What is the performance impact of change tracking on read-heavy queries? {#chapter-11-change-tracking-async-transactions-q4}

What is the performance impact of change tracking on read-heavy queries?

**Answer:** Tracking adds memory for original/current value snapshots and CPU for fix-up and change detection on every materialized entity. On large read-only lists, this overhead can significantly increase Gen2 pressure and request latency compared to equivalent `AsNoTracking` queries.

- A 10,000-row GET that tracks entities holds two value sets per property per row in the worst case.
- Identity resolution and relationship fix-up add cost beyond flat property snapshots.
- Projection to DTOs avoids both tracking and loading unused columns — often the best read-path optimization.
- The regression is silent until load testing — results are identical, only resource use differs.

---

### Q5. Why use async EF Core methods in ASP.NET Core? {#chapter-11-change-tracking-async-transactions-q5}

Why use async EF Core methods in ASP.NET Core?

**Answer:** Async methods (`ToListAsync`, `SaveChangesAsync`, `FirstOrDefaultAsync`) release the request thread during I/O waits, improving scalability under concurrent load. ASP.NET Core thread pool threads are a shared resource — blocking them on database calls reduces throughput.

- Database latency is I/O-bound; async avoids thread starvation during waits for SQL Server responses.
- Pair with `CancellationToken` from `HttpContext.RequestAborted` so client disconnects cancel long queries.
- Never block async EF calls with `.Result` or `.Wait()` — causes deadlocks and defeats scalability benefits.
- Sync methods remain acceptable in console tools or single-threaded batch jobs with no concurrency pressure.

---

### Q6. How do you begin and commit a transaction in EF Core? {#chapter-11-change-tracking-async-transactions-q6}

How do you begin and commit a transaction in EF Core?

**Answer:** Start a transaction on the context's database facade with `BeginTransactionAsync`, perform tracked changes and raw SQL, then call `CommitAsync` — or `RollbackAsync` on failure. All operations on the same `DbContext` instance participate in the transaction until commit or rollback.

- `await using var tx = await context.Database.BeginTransactionAsync();`
- Make changes, `await context.SaveChangesAsync();`, optionally run `ExecuteSqlRaw` on the same context.
- `await tx.CommitAsync();` — dispose rolls back if commit was not called.
- `SaveChanges` alone wraps each call in an implicit transaction for its own batch, not across multiple separate calls.

---

### Q7. How does EF Core detect concurrency conflicts? {#chapter-11-change-tracking-async-transactions-q7}

How does EF Core detect concurrency conflicts?

**Answer:** EF Core compares concurrency token values in the UPDATE or DELETE WHERE clause against the values read when the entity was loaded. If zero rows match because another transaction changed the row first, EF Core throws `DbUpdateConcurrencyException` on `SaveChanges`.

- Configure a concurrency token property with `[Timestamp]` / `rowversion` or `.IsConcurrencyToken()` on a property.
- Generated SQL includes `WHERE Id = @id AND RowVersion = @originalRowVersion`.
- No token configured means last-write-wins — later saves overwrite earlier changes silently.
- Optimistic concurrency suits web apps where simultaneous edits are possible but locking is undesirable.

---

### Q8. What is a concurrency token or row version column? {#chapter-11-change-tracking-async-transactions-q8}

What is a concurrency token or row version column?

**Answer:** A concurrency token is a property mapped to a database column whose value changes whenever the row is updated, used by EF Core to detect stale writes. SQL Server `rowversion` (`[Timestamp]` in EF) is the common choice — the database auto-increments it on every update.

- Mark with `[Timestamp]` on a `byte[]` property or Fluent API `.IsRowVersion()`.
- EF Core reads the token at query time and includes the original value in UPDATE/DELETE predicates.
- Any concurrent modification changes the token, causing the next save to affect zero rows and throw.
- Application-defined tokens (for example a manual `Version` integer) also work if incremented on each update.

---

### Q9. How do you handle `DbUpdateConcurrencyException`? {#chapter-11-change-tracking-async-transactions-q9}

How do you handle `DbUpdateConcurrencyException`?

**Answer:** Catch `DbUpdateConcurrencyException`, inspect `exception.Entries` for conflicting entities, and resolve by refreshing from the database, merging user changes, or returning a conflict response to the client. Production APIs typically return HTTP 409 with a message asking the user to reload and retry.

- Reload: `await entry.ReloadAsync()` discards stale client values and re-reads current database state.
- Client wins: reapply intended values after reload and retry `SaveChanges` if business rules allow overwriting.
- Server wins: return 409 Conflict with current row data so the UI can show what changed.
- Log concurrency conflicts for monitoring — frequent conflicts may indicate UX or workflow design issues.

---

---

## Gotchas — .NET Data Access (Interview Traps)

#### Gotcha 1. String concatenation instead of parameters

**Answer:** Building SQL with `$"WHERE Id = {id}"` or string concatenation sends user input as literal SQL text, bypassing parameterization and enabling SQL injection even when the rest of the application uses an ORM or micro-ORM.

- ADO.NET and Dapper require explicit parameters — never embed raw user strings in SQL text.
- EF Core `FromSqlInterpolated` is safe; passing an ordinary interpolated string to `FromSqlRaw` is not.
- Code review should treat any dynamic SQL without placeholders as a blocking defect.

---

#### Gotcha 2. Open DataReader blocks second command

**Answer:** Running another `SqlCommand` on the same connection while a `SqlDataReader` is still open fails on SQL Server unless Multiple Active Result Sets (MARS) is enabled in the connection string.

- Always dispose or finish reading the `DataReader` before issuing the next command on that connection.
- A common bug loads a header row then tries to load detail rows on the same connection without closing the reader.
- EF Core manages readers internally, but raw ADO.NET code in the same request must respect this rule.

---

#### Gotcha 3. `AddWithValue` and wrong SQL types

**Answer:** `SqlParameter.AddWithValue` infers parameter types from CLR values, which may not match the database column type — causing implicit conversions, index scans, and poor plan cache behavior.

- Prefer explicit `SqlParameter` with `SqlDbType`, size, and precision matching the column definition.
- String inference often picks oversized `nvarchar` lengths, preventing optimal index seeks on narrower columns.
- Dapper and EF Core parameterize with more predictable typing but custom ADO.NET still needs explicit types.

---

#### Gotcha 4. Leaked connections exhaust the pool

**Answer:** Failing to dispose `SqlConnection`, `SqlDataReader`, or abandoning a `using` block early leaks connection pool slots until timeout, eventually causing "timeout expired obtaining connection from pool" errors under load.

- Always use `await using` for connections and readers so disposal runs on exceptions too.
- Symptoms appear only under concurrent load, making this a classic production-only failure mode.
- Long-lived undisposed `DbContext` instances cause the same exhaustion pattern.

---

#### Gotcha 5. Transaction started after first command

**Answer:** Beginning a `SqlTransaction` only after the first statement already executed means that statement committed under implicit autocommit, so later steps in the intended unit of work are not atomic with the first.

- Call `BeginTransaction` immediately after opening the connection, before any DML.
- EF Core `SaveChanges` without an explicit transaction auto-commits each call — wrap multi-step work explicitly.
- Integration tests with single-user data often miss this race because implicit commits appear to "work."

---

#### Gotcha 6. Dapper `Query` without `using` on connection

**Answer:** Returning deferred `IEnumerable<T>` from Dapper before disposing the connection postpones execution until enumeration, failing at runtime or holding connections open until garbage collection.

- Materialize inside the connection scope with `.ToList()` or `.ToArray()` before returning from the method.
- Deferred execution means SQL runs when the caller iterates — often after the `using` block closed the connection.
- Async variants (`QueryAsync`) still require materialization before leaving the connection lifetime.

---

#### Gotcha 7. `QuerySingle` when zero or many rows exist

**Answer:** Dapper's `QuerySingle` throws if zero rows or more than one row match, while optional lookups typically need `QueryFirstOrDefault` which returns default when empty.

- Use `QuerySingle` only when exactly one row is a domain invariant enforced by a unique key.
- Duplicate data turns `QuerySingle` into a hard failure that `QueryFirstOrDefault` would handle differently — choose based on whether duplicates indicate bugs.
- EF Core mirrors the same distinction between `SingleOrDefault` and `FirstOrDefault`.

---

#### Gotcha 8. Multi-map `splitOn` wrong column

**Answer:** Dapper multi-mapping uses `splitOn` to name the column where the next object type begins; an incorrect column splits at the wrong boundary, silently mapping NULL or wrong values into nested objects.

- `splitOn` defaults to `"Id"` — duplicate column names in SELECT lists require explicit aliases and matching `splitOn` values.
- Align SELECT column order with the generic type order in `Query<TFirst, TSecond, TReturn>`.
- Integration tests asserting nested property values catch splitOn mistakes that unit tests on flat rows miss.

---

#### Gotcha 9. Scoped `DbContext` captured in a singleton

**Answer:** Registering a singleton service that holds a scoped `DbContext` creates a captive dependency — the context may be disposed while the singleton lives, or state leaks across HTTP requests.

- `DbContext` is scoped per request in ASP.NET Core — singletons must not store it in fields.
- Inject `IDbContextFactory<TContext>` into singletons when long-lived services need occasional database access.
- Symptoms include "Cannot access a disposed context" or cross-user data contamination in tracked entities.

---

#### Gotcha 10. Lazy loading after the context is disposed

**Answer:** Lazy loading triggers SQL when navigation properties are accessed — if that happens after the request-scoped `DbContext` is disposed, EF Core throws or the serializer triggers hidden queries that fail mid-response.

- ASP.NET Core disposes scoped contexts at the end of the request pipeline — serialization often runs near that boundary.
- Prefer explicit includes or projections inside the request scope instead of returning entity graphs with unresolved lazy navigations.
- Proxy types plus disposed contexts produce intermittent failures depending on property access order.

---

#### Gotcha 11. N+1 from lazy load or missing Include

**Answer:** Listing parent entities then accessing navigation properties in a loop without eager loading or projection fires one SQL query per parent row — classic N+1 performance collapse in EF Core APIs.

- One query for N orders plus N queries for each order's lines equals N+1 round-trips per request.
- Fix with `Include`/`ThenInclude`, split queries, or `Select` projections that join needed data in one statement.
- EF Core command logging revealing identical query templates with different IDs signals N+1 immediately.

---

#### Gotcha 12. Cartesian explosion with multiple Includes

**Answer:** Eager-loading two or more collection navigations in one SQL query multiplies result rows by the product of collection sizes, spiking memory and network use even though parent entity count is modest.

- EF Core deduplicates parents during fix-up, but SQL Server already sent the inflated rowset across the wire.
- Use `AsSplitQuery()` to fetch collections with separate SELECT statements instead of one giant join.
- Projection to DTOs avoids loading full collection graphs when only counts or summaries are needed.

---

#### Gotcha 13. Client-side evaluation of LINQ

**Answer:** Calling `ToList()` before filtering or using non-translatable C# logic in `Where` forces EF Core to pull entire tables into application memory — acceptable in development with small seeds, catastrophic in production at scale.

- EF Core 3+ throws on many accidental client evaluations instead of silently downloading whole tables.
- `AsEnumerable()` explicitly switches to LINQ to Objects — any following `Where` runs in memory.
- Rewrite with translatable expressions, `EF.Functions`, database-side filtering, or raw SQL for unsupported logic.

---

#### Gotcha 14. Tracking overhead on read-only queries

**Answer:** Omitting `AsNoTracking()` on large read-only lists makes EF Core snapshot every entity for change detection that will never run, wasting memory and CPU on GET endpoints.

- Tracking stores original and current values per property for each row materialized.
- ASP.NET Core read services should default to `AsNoTracking()` plus DTO projection.
- Global `QueryTrackingBehavior.NoTracking` with explicit tracking on command paths prevents accidental overhead.

---

#### Gotcha 15. `SaveChanges` without a transaction for multi-step updates

**Answer:** Multiple `SaveChanges` calls or separate database operations that must succeed together commit independently by default, allowing partial updates that leave data in an inconsistent state when a later step fails.

- Wrap related saves and raw SQL in `BeginTransactionAsync`/`CommitAsync` on one `DbContext`.
- Prefer one `SaveChanges` per unit of work when all changes are tracked together on the same context.
- Retry logic after failure cannot assume earlier steps rolled back unless they shared a transaction boundary.

---

---

## Scenario-Based Answers (Karat Format)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A teammate registers EF Core in an ASP.NET Core API like this and injects `AppDbContext` into a singleton `ProductCacheService` that stores query results in an instance field. Review the setup. What breaks under concurrent traffic, and how do you fix it?

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")),
    ServiceLifetime.Singleton);

builder.Services.AddSingleton<ProductCacheService>();

// ProductCacheService.cs
public sealed class ProductCacheService
{
    private readonly AppDbContext _db;
    private List<Product>? _cached;

    public ProductCacheService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct) =>
        _cached ??= await _db.Products.Where(p => p.DiscontinuedDate == null).ToListAsync(ct);
}
```

---

**Answer:**

**Answer:** `DbContext` is not thread-safe and must be scoped per request (or per unit of work), not registered as a singleton. Sharing one instance across concurrent HTTP requests causes change-tracker corruption, stale data, and cross-request state leakage — and caching query results on a singleton service mixes every caller's view of the catalog.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `AddDbContext` with `ServiceLifetime.Singleton` | One `DbContext` shared by all requests — not thread-safe |
| Architecture | Singleton `ProductCacheService` holds `DbContext` + mutable `_cached` | Captive dependency; tracker state and cache bleed across users |
| Correctness | Concurrent reads/writes on same context instance | Intermittent exceptions, wrong entities attached, flaky tests |
| Scale-out | In-memory `_cached` on singleton | Stale catalog until restart; not coherent across multiple pods |

**Fix (priority order):**

1. Register with default **scoped** lifetime: `builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(...));` — omit the third parameter or pass `ServiceLifetime.Scoped`.
2. If caching is required, inject `IMemoryCache` or a distributed cache into a **singleton** service and store **DTOs**, not `DbContext` or tracked entities — load through a scoped factory or `IServiceScopeFactory.CreateScope()` per refresh.
3. Keep `ProductCacheService` singleton only if it is stateless regarding EF; resolve `AppDbContext` inside a short-lived scope when refreshing cache data.
4. Enable scope validation in development (`builder.Services.ValidateScopes = true`) to catch captive scoped dependencies early.

**Production takeaway:** Karat uses DbContext lifetime to test whether you know EF is a per-request session, not a shared connection pool wrapper. See this chapter's `AppDbContext` — constructor injection via `DbContextOptions<T>` is the pattern `AddDbContext` expects.

---

---

#### Q2. (D) Your team owns a product catalog microservice: CRUD on `Product` with relationships, schema owned via Code-First migrations, and a separate admin dashboard that runs one hand-tuned SQL report per screen (aggregations, window functions, 500k+ rows, read-only). They want to pick **one** data-access stack for everything. What do you recommend for each path — EF Core, Dapper, or ADO.NET — and why would forcing a single choice hurt?

---

**Answer:**

**Answer:** Use EF Core for the catalog CRUD and schema evolution path, and Dapper (or targeted raw SQL through EF's escape hatches) for the heavy read-only reports — not ADO.NET unless you need provider-specific streaming APIs Dapper cannot cover. Forcing one stack either sacrifices migration/change-tracking productivity on writes or accepts poor SQL and overhead on report queries.

- **Catalog CRUD + relationships + migrations:** EF Core — entities map to your domain, `SaveChanges` handles unit-of-work, Code-First migrations keep schema aligned (this chapter's stack comparison; detail in ch03).
- **Admin dashboards / large aggregations:** Dapper — you keep hand-tuned SQL, `Query<T>` maps rows with minimal overhead, no change tracker on 500k rows.
- **ADO.NET:** Reserve for max-control scenarios (manual `SqlDataReader` streaming, bulk copy APIs) when neither EF nor Dapper fits.
- **Hybrid is normal:** Same database, two access paths — EF for writes/domain services, Dapper for read-optimized report endpoints — with clear boundaries so teams do not duplicate business rules in SQL strings.
- **Forcing EF everywhere:** LINQ translation may produce suboptimal plans; materializing large graphs wastes memory; no change tracking needed on read-only reports.
- **Forcing Dapper everywhere:** You reimplement relationship graphs, migration tooling, and optimistic concurrency manually on the CRUD side.

**Production takeaway:** The intro chapter's "when to use" table is a decision guide, not a loyalty oath — senior judgment is picking the right tool per workload while sharing one connection string and schema.

---

---

#### Q3. (R) CI passes with this "integration" test setup, but staging against SQL Server fails on the same assertions. Review the test harness:

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

What is wrong with treating In-Memory as a SQL Server substitute here, and what would you change?

---

**Answer:**

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

What is wrong with treating In-Memory as a SQL Server substitute here, and what would you change?

**Answer:** The In-Memory provider is an in-process dictionary, not a SQL engine — it does not enforce relational constraints, cascade rules, or T-SQL semantics the way SQL Server does. Sharing one database name across parallel tests also causes cross-test pollution, so green CI does not predict staging behavior.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Provider fidelity | `UseInMemoryDatabase` for cascade/FK behavior | In-Memory ignores or approximates SQL constraints — false positives |
| Test isolation | Fixed name `"ProductTests"` for all classes | Parallel tests share state; order-dependent failures |
| Schema strategy | `EnsureCreated()` instead of migrations against real DB | Skips migration SQL, indexes, and provider-specific DDL |
| Mislabeling | Class named "integration" but uses In-Memory | Team trusts CI; production SQL fails on delete rules |

**Fix (priority order):**

1. Use a **unique** In-Memory name per test (`Guid.NewGuid().ToString()`) if you only need fast unit tests of LINQ against `DbContext` — as in this chapter's `InMemoryProviderPreview` (`"EfCoreIntroCh01"`).
2. Move cascade/FK/delete-behavior tests to **SQL Server integration tests** — LocalDB, Testcontainers, or a dedicated CI database — with migrations applied (`Database.Migrate()` or `dotnet ef database update`).
3. Keep In-Memory for pure logic tests (query filters, mapping) where SQL translation differences are irrelevant; never assert provider-specific SQL behavior against In-Memory.
4. Dispose/`using` the context per test and avoid static shared stores.

**Production takeaway:** This chapter explicitly warns that In-Memory is preview/demo-only — Karat tests whether you would ship based on In-Memory "integration" tests. See `Utils/InMemoryProviderPreview.cs` limitations list.

---

---

#### Q4. (P) A developer adds `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.SqlServer` to the web project, writes `AppDbContext` and entities, then runs `dotnet ef migrations add InitialCreate` from the solution root. The tool reports that no `DbContext` was found or that design-time services cannot be resolved. Explain **design-time vs runtime** EF Core packages and list the fixes in priority order.

---

**Answer:**

**Answer:** Runtime packages (`Microsoft.EntityFrameworkCore`, provider packages) power your app at execution time; design-time tooling needs `Microsoft.EntityFrameworkCore.Design` (and often `dotnet-ef` as a local tool) to construct `DbContext` outside the running host, discover options, and generate migrations. Missing design packages or wrong startup project causes the CLI to fail even when the app runs fine.

- **Runtime (app host):** `Microsoft.EntityFrameworkCore` + `Microsoft.EntityFrameworkCore.SqlServer` (or Npgsql, etc.) — loaded when the API runs; `AddDbContext` and queries use these.
- **Design-time (CLI / VS Package Manager):** `Microsoft.EntityFrameworkCore.Design` — referenced with `PrivateAssets="All"` in the project that owns `DbContext`; supplies `IDesignTimeDbContextFactory` resolution, MSBuild targets for `dotnet ef`.
- **Tooling:** Install `dotnet-ef` as a **local** tool (`dotnet tool install dotnet-ef`) or use PMC `Add-Migration` with Design package present — the global SDK does not include EF commands by default.
- **Startup project:** Run from the project containing `DbContext`, or pass `--project` (migrations project) and `--startup-project` (web app with `Program.cs` and connection string).
- **`IDesignTimeDbContextFactory<TContext>`:** Add when DI-only configuration (no parameterless ctor) prevents the tools from building options — factory builds `DbContextOptions` the same way as `Program.cs`.

**Fix (priority order):**

1. Add `Microsoft.EntityFrameworkCore.Design` to the `DbContext` project (matches version of runtime packages — e.g. 8.0.11 in this chapter's `.csproj`).
2. Install/restore `dotnet-ef` local tool; run `dotnet ef migrations add InitialCreate --project <ContextProject> --startup-project <WebProject>`.
3. Ensure `AppDbContext` is public and in an assembly referenced by the startup project; pass explicit `--context AppDbContext` if multiple contexts exist.
4. If options come only from DI, implement `IDesignTimeDbContextFactory<AppDbContext>` reading `appsettings.json` or env vars — mirrors production registration.

**Production takeaway:** "App runs, migrations fail" is a classic onboarding trap — runtime and design-time are separate deployment concerns; Design is not needed on the server, only in the repo for schema changes. See `EfCoreConcepts.ExplainNuGetSetup` — Design deferred to ch03/ch04.

---

---

#### Q5. (M) Production moves from on-prem SQL Server to Azure Database for PostgreSQL. The codebase still references only `Microsoft.EntityFrameworkCore.SqlServer` and calls `UseSqlServer(connectionString)` inside `OnConfiguring`. Entity classes and LINQ queries are unchanged. What must change for **provider selection**, and what still requires manual verification after the swap?

---

**Answer:**

**Answer:** Swap the NuGet provider package and the `Use*` extension — replace `Microsoft.EntityFrameworkCore.SqlServer` + `UseSqlServer` with `Npgsql.EntityFrameworkCore.PostgreSQL` + `UseNpgsql` — and update connection strings. LINQ and entities largely stay the same, but dialect differences, migrations, and provider-specific types must be revalidated; you cannot copy SQL Server migration history verbatim.

- **Packages:** Remove `Microsoft.EntityFrameworkCore.SqlServer`; add `Npgsql.EntityFrameworkCore.PostgreSQL` (version aligned with EF Core 8.x).
- **Registration:** Centralize in `AddDbContext` / `DbContextOptionsBuilder` — `options.UseNpgsql(configuration.GetConnectionString("Default"))`; remove hardcoded `UseSqlServer` from `OnConfiguring` when using DI (see Q6).
- **Connection string:** PostgreSQL format (`Host=...;Database=...;Username=...;Password=...`) — often from Azure Key Vault or App Configuration, not LocalDB.
- **Migrations:** Regenerate or baseline a new migration history for PostgreSQL — SQL Server–specific annotations (clustered indexes, `nvarchar`, sequences) do not port automatically.
- **Manual verification:** Raw SQL (ch10), `decimal` precision, `DateTime` kind handling, string collation, JSON/hierarchy columns, and query plans for hot LINQ — providers translate differently (this chapter's provider table: same DbContext surface, different dialect).
- **Tests:** Replace In-Memory or SQL Server–only CI with PostgreSQL Testcontainers or Azure flexible server integration tests before cutover.

**Production takeaway:** Provider swap is a NuGet + extension + connection change at the surface, but production readiness means re-running migrations and performance tests against the target dialect — not assuming LINQ is byte-identical SQL.

---

---

#### Q6. (R) Review this `AppDbContext` and `Program.cs` fragment from a web app that "works locally" but ignores environment-specific connection strings in deployed environments:

```csharp
public sealed class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Trusted_Connection=True;");
    }
}

// Program.cs
builder.Services.AddDbContext<AppDbContext>();
```

What problems does this pattern create for DI, testing, and multi-environment deployment — and what is the production-ready registration shape?

---

### 02. DbContext & DbSet

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/02. DbContext & DbSet`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public sealed class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Trusted_Connection=True;");
    }
}

// Program.cs
builder.Services.AddDbContext<AppDbContext>();
```

What problems does this pattern create for DI, testing, and multi-environment deployment — and what is the production-ready registration shape?

**Answer:** The parameterless constructor plus hardcoded LocalDB fallback in `OnConfiguring` fights DI-based configuration: design-time tools and accidental `new AppDbContext()` silently hit LocalDB, while deployed environments that expect `AddDbContext` lambda configuration may never use the intended connection string if options are not wired correctly. Production apps should use a single options-injected constructor and configure the provider only in composition root.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Configuration | Hardcoded LocalDB in `OnConfiguring` | Staging/prod ignore `appsettings.Production.json` when fallback path runs |
| DI | Parameterless ctor enables `new AppDbContext()` | Bypasses scoped registration; hidden second configuration path |
| Testability | Cannot swap to In-Memory/Npgsql without hitting `OnConfiguring` | Tests accidentally touch LocalDB or duplicate provider setup |
| Design-time | Tools may use parameterless ctor | Migrations generated against wrong server |
| Pattern | `AddDbContext<AppDbContext>()` without options lambda | Relies on `OnConfiguring` — acceptable only if all config lives there intentionally |

**Fix (priority order):**

1. Remove the parameterless constructor and hardcoded connection string from production code paths.
2. Register explicitly:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

3. Keep `AppDbContext(DbContextOptions<AppDbContext> options) : base(options)` only — matches this chapter's `Data/AppDbContext.cs` preview.
4. For `dotnet ef`, add `IDesignTimeDbContextFactory<AppDbContext>` that reads configuration — do not embed secrets in source.
5. In tests, pass `UseInMemoryDatabase(Guid.NewGuid().ToString())` or Testcontainers via `DbContextOptionsBuilder` — never depend on LocalDB fallback.

**Production takeaway:** `DbContextOptions<T>` injection at the composition root is the seam for provider selection, environment connection strings, and test doubles — `OnConfiguring` with LocalDB is tutorial-friendly but becomes a deployment footgun in web apps. Forward reference: ch02 covers `OnConfiguring` vs options injection in depth.

---

### 02. DbContext & DbSet

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/02. DbContext & DbSet`

---

---

#### Q1. (R) A teammate "optimizes" startup by registering `AppDbContext` as a **Singleton** and injecting it into controllers. The API passes smoke tests locally but corrupts data under concurrent load. Review the registration and usage — what is wrong, and how do you fix it?

```csharp
// Program.cs
builder.Services.AddSingleton<AppDbContext>(sp =>
{
    var options = sp.GetRequiredService<DbContextOptions<AppDbContext>>();
    return new AppDbContext(options);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

```csharp
// ProductController.cs
public class ProductController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetInStock()
    {
        return await _db.Products
            .Where(p => p.StockQuantity > 0)
            .ToListAsync();
    }
}
```

---

**Answer:**

**Answer:** `DbContext` is not thread-safe and must represent one unit of work per scope — registering it as a singleton shares one change tracker and connection semantics across all requests, which causes cross-request state bleed and race conditions under concurrency. The snippet also registers `AddDbContext` twice (singleton factory plus default scoped registration), creating ambiguous DI resolution.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `AddSingleton<AppDbContext>` | One context shared by all HTTP requests |
| Thread safety | Concurrent `ToListAsync` / `SaveChanges` on same instance | Corrupted change tracker; intermittent wrong reads/writes |
| Design | Duplicate registration (`AddSingleton` + `AddDbContext`) | Unpredictable which registration wins; harder to diagnose |
| Resource | Long-lived context holds DB connection / tracked entities | Connection pool pressure; memory growth on tracked graphs |

**Fix (priority order):**

1. Remove the `AddSingleton<AppDbContext>` registration entirely.
2. Keep only `services.AddDbContext<AppDbContext>(...)` — EF registers the context as **scoped** (one per HTTP request).
3. Inject `AppDbContext` into controllers/transient services within the request scope; never store it on singleton fields.
4. For background work, create an `IServiceScope` per job and resolve a fresh context inside that scope.

**Production takeaway:** Karat pairs "singleton for performance" with EF — the correct default is scoped `AddDbContext`, not shared instances. See this chapter's `DbContextServiceRegistration` Section 8 and `AppDbContext` lifetime comments.

---

---

#### Q2. (R) A background job processes a price-update queue with `Parallel.ForEachAsync`, reusing one injected `AppDbContext`. Review this worker — what breaks under concurrency, and what lifetime pattern replaces it?

```csharp
public sealed class PriceUpdateWorker : BackgroundService
{
    private readonly AppDbContext _db;
    private readonly IPriceQueue _queue;

    public PriceUpdateWorker(AppDbContext db, IPriceQueue queue)
    {
        _db = db;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Parallel.ForEachAsync(
            _queue.ReadAll(stoppingToken),
            stoppingToken,
            async (item, ct) =>
            {
                var product = await _db.Products.FindAsync(new object[] { item.ProductId }, ct);
                if (product is null) return;
                product.UnitPrice = item.NewPrice;
                await _db.SaveChangesAsync(ct);
            });
    }
}
```

---

**Answer:**

**Answer:** A single `DbContext` instance must not be used concurrently from multiple threads — `Parallel.ForEachAsync` violates that rule, so EF Core's change tracker and internal state can corrupt updates or throw inconsistent exceptions. The worker also captures a scoped context in a singleton `BackgroundService`, which is a captive dependency that may be disposed before the host shuts down.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Thread safety | Shared `_db` across parallel iterations | Undefined behavior; lost updates or tracker exceptions |
| DI lifetime | Scoped `AppDbContext` injected into singleton worker | Captive dependency; `ObjectDisposedException` after scope ends |
| Unit of work | One context tracking interleaved `SaveChanges` from parallel tasks | Conflicting `EntityState`; partial commits |
| Scalability | Serial connection reuse under fake parallelism | False sense of throughput; DB lock contention |

**Fix (priority order):**

1. Do **not** parallelize on one context — process sequentially on one `_db` **or** create one scope (and one context) **per item**.
2. Inject `IServiceScopeFactory` (or `IDbContextFactory<AppDbContext>`) instead of `AppDbContext` directly into the hosted service.
3. Per item:

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var product = await db.Products.FindAsync(new object[] { item.ProductId }, ct);
// update + SaveChangesAsync on this scope's context only
```

4. Optionally use `IDbContextFactory<AppDbContext>` for explicit `CreateDbContext()` per parallel partition when true parallelism is required.

**Production takeaway:** EF documents explicitly: do not share one `DbContext` across threads. Parallel + injected context is a classic Karat stack of thread safety and DI lifetime traps.

---

---

#### Q3. (M) After deploying to a database that already has `dbo.Products`, EF queries fail with "Invalid object name 'Product'". The entity and context match this chapter's tutorial shape. Review the context — what naming mistake caused the mismatch, and how do you fix it without renaming the SQL table?

```csharp
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Product { get; set; } = null!; // singular property name

    // no OnModelCreating override
}
```

```csharp
public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
```

---

**Answer:**

**Answer:** EF Core maps `DbSet<Product> Product` to table name **Product** (singular) by convention — pluralizing uses the **DbSet property name**, not only the entity class name. The existing database table is `Products`, so generated SQL targets the wrong object.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Model mapping | Singular DbSet property `Product` | SQL queries `dbo.Product` instead of `dbo.Products` |
| Convention | Assumed class name alone drives table name | Mismatch when property name differs from table |
| Runtime | No compile error — fails at first query | Deploy-time surprise after schema already exists |

**Fix (priority order):**

1. Rename the property to plural `Products` (matches this chapter's `AppDbContext`: `public DbSet<Product> Products => Set<Product>()`).
2. Or keep the property name and pin the table in `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().ToTable("Products");
}
```

3. Add an integration test that runs a simple `context.Products.Any()` against the real schema in CI.

**Production takeaway:** DbSet **property naming** drives default table names — Karat tests whether you know convention details beyond "class Product maps to Products." Full fluent overrides → ch.07 Fluent API & Data Annotations.

---

---

#### Q4. (R) A developer copies the tutorial's `OnConfiguring` fallback into production but removes the `IsConfigured` guard so LocalDB always works in dev. DI registration supplies the real connection string from `appsettings.json`. Review the context — what breaks in staging/prod, and what is the correct split between `OnConfiguring` and `AddDbContext`?

```csharp
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true");
    }
}
```

```csharp
// Program.cs (ASP.NET Core)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

---

**Answer:**

**Answer:** Without `if (!optionsBuilder.IsConfigured)`, `OnConfiguring` overwrites provider settings every time the context is constructed — the hard-coded LocalDB string wins over `AddDbContext`'s configuration, so staging and production connect to the wrong server (or fail entirely on Linux/containers where LocalDB does not exist).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Configuration | Unconditional `UseSqlServer(LocalDB...)` in `OnConfiguring` | Ignores `appsettings.json` / environment connection strings |
| Hosting | LocalDB connection text in deployed binaries | Startup or query failures on non-Windows hosts |
| Security / ops | Connection secrets embedded in context class | Cannot rotate credentials via configuration |
| Design | Mixing demo fallback with production DI path | Environment-specific bugs that pass on developer machines |

**Fix (priority order):**

1. Restore the guard from this chapter's `AppDbContext`:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
        optionsBuilder.UseSqlServer(ConnectionOptions.LocalDbConnectionString);
}
```

2. In ASP.NET Core, configure **only** in `AddDbContext` using `IConfiguration.GetConnectionString("Default")` — leave `OnConfiguring` empty or dev-only behind the guard.
3. Remove hard-coded connection strings from the context class; use User Secrets / environment variables for local dev when not using DI.
4. Verify with a staging deploy that `context.Database.GetConnectionString()` reflects the configured server.

**Production takeaway:** `OnConfiguring` is a fallback for parameterless construction (tutorial Section 4b), not the primary production path — DI + options constructor is. Karat embeds the missing `IsConfigured` check as the trap.

---

---

#### Q5. (R) A performance pass switches to `AddDbContextPool` but keeps request-scoped state on the context subclass. Under load, users occasionally see another user's `CurrentUserId` in audit columns. Review the design — what violates pooling rules, and how do you fix it?

```csharp
public sealed class AppDbContext : DbContext
{
    public int CurrentUserId { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<Product>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedByUserId = CurrentUserId;
        }
        return base.SaveChanges();
    }
}
```

```csharp
// Program.cs
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")),
    poolSize: 128);
```

---

**Answer:**

**Answer:** `AddDbContextPool` reuses the same context **instance** across requests after reset — instance fields like `CurrentUserId` survive between leases if not cleared, so audit logic stamps the wrong user. Pooling requires the context type to behave as if freshly constructed on every lease.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pooling contract | Mutable instance state (`CurrentUserId`) on pooled context | Cross-request data leak; wrong audit metadata |
| Correctness | `SaveChanges` reads stale `CurrentUserId` | Compliance / security incident under concurrency |
| Performance misconception | Pooling replaces scoped lifetime semantics | Faster startup, not safe for per-request fields on the context |
| Design | Business identity stored on EF context instead of call context | Hard to test; hidden coupling to HTTP user |

**Fix (priority order):**

1. Remove mutable request state from `AppDbContext` — no user id fields on the context class.
2. Pass `CurrentUserId` into repository/service methods, or inject `ICurrentUserService` and read it inside `SaveChanges` from scoped DI (not from a field set once on a pooled instance).
3. If you must set state per request, use non-pooled `AddDbContext`, or implement `IDisposable`/`Reset` patterns only as documented — prefer not pooling stateful contexts.
4. Add a load test asserting audit columns always match the authenticated user.

**Production takeaway:** `AddDbContextPool` is safe only when the context is stateless aside from EF's own reset — Karat uses audit-column bleed to test pooling misuse. Default `AddDbContext` (this chapter's preview) is the safer baseline until you understand pool rules.

---

---

#### Q6. (P) A console integration test host mirrors this chapter's `DbContextServiceRegistration` but resolves `AppDbContext` directly from the root `ServiceProvider` (no scope). With `ValidateScopes = true`, startup throws; with validation disabled, tests pass but production API calls fail intermittently. Explain both behaviors and show the correct resolution pattern for a scoped DbContext.

```csharp
public static class DbContextServiceRegistration
{
    public static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        return services.BuildServiceProvider(validateScopes: true);
    }

    public static IReadOnlyList<Product> GetProducts(ServiceProvider provider)
    {
        AppDbContext context = provider.GetRequiredService<AppDbContext>();
        return context.Products.OrderBy(p => p.Id).ToList();
    }
}
```

---

---

### 03. Code-First Models & Migrations

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/03. Code-First Models & Migrations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** `AddDbContext` registers `AppDbContext` as scoped — resolving it from the root `ServiceProvider` creates a captive singleton context when validation is off, or throws `InvalidOperationException` when `ValidateScopes` is true. Scoped services must be resolved inside an `IServiceScope` so each unit of work gets its own context that is disposed with the scope.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI scope | `provider.GetRequiredService<AppDbContext>()` at root | Violates scoped lifetime; one long-lived context |
| Validation | `validateScopes: true` catches misuse at resolve time | Test host fails fast (correct behavior) |
| Production | Validation disabled in tests hides bug | Intermittent disposed-context errors under ASP.NET load |
| Resource | Root-resolved context never disposed per operation | Connection / tracker leak in long-running hosts |

**Fix (priority order):**

1. Always create a scope before resolving — match this chapter's `GetProductsViaDependencyInjection`:

```csharp
public static IReadOnlyList<Product> GetProducts(ServiceProvider provider)
{
    using IServiceScope scope = provider.CreateScope();
    AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    return context.Products.OrderBy(p => p.Id).ToList();
}
```

2. Keep `ValidateScopes = true` in integration tests to mirror ASP.NET Core's default scope validation in Development.
3. In web apps, rely on request scope — controllers get `AppDbContext` injected automatically per request.
4. For manual units of work in console jobs, `using var scope = provider.CreateScope()` per operation.

**Production takeaway:** Same registration shape as ASP.NET Core (`AddDbContext` → scoped) but resolution **must** happen inside a scope — this chapter's Section 8b demonstrates the correct pattern; root resolution is a common Karat trap paired with `ValidateScopes`.

---

---

### 03. Code-First Models & Migrations

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/03. Code-First Models & Migrations`

---

---

#### Q1. (R) Two developers branch from the same commit where `InitialCreate` is the only migration. Alice adds `AddProductSku` on Monday; Bob adds `AddCategoryDescription` on Tuesday. Both run `dotnet ef migrations add` locally against the same snapshot. After merge, the repo has two timestamped migrations whose Designer files both list `InitialCreate` as the parent, and Git conflicted on `StoreDbContextModelSnapshot.cs`. CI runs `dotnet ef database update` on a fresh LocalDB and fails. What went wrong, and how do you resolve it without losing either schema change?

---

**Answer:**

**Answer:** Both developers generated sibling migrations from the same parent snapshot, so EF Core sees a branched migration history — only one linear chain can be applied, and the merged `StoreDbContextModelSnapshot.cs` cannot represent two divergent model states at once.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Workflow | Parallel `migrations add` from identical base | Two migrations with same parent; non-linear history |
| Source control | Conflicting `StoreDbContextModelSnapshot.cs` | Snapshot out of sync with one or both migration files |
| CI/runtime | `dotnet ef database update` on clean DB | Second sibling migration may never apply or build metadata fails |
| Team process | No "migration lock" or rebase-before-add rule | Repeatable merge pain on every parallel schema change |

**Fix (priority order):**

1. **Pick one branch's migration to keep temporarily** — apply it locally on a scratch database to preserve its DDL intent.
2. **Remove the conflicting sibling migration(s)** from `Migrations/` (both `.cs`, `.Designer.cs`, and reset snapshot if needed).
3. **Merge model changes in C# first** — combine Alice's `Sku` property and Bob's `Description` on `Product`/`Category` in one working tree.
4. Run **`dotnet ef migrations add CombinedProductAndCategoryChanges`** to produce a **single** new migration and one authoritative snapshot.
5. On databases that already applied one sibling migration (dev machines), either `dotnet ef database drop` (dev only) or manually reconcile `__EFMigrationsHistory` and schema — never do this casually in production.
6. **Prevent recurrence:** short-lived feature branches, pull latest before `migrations add`, or designate one "schema owner" per sprint; some teams add a CI check that migration timestamps are strictly increasing with a single parent chain.

**Production takeaway:** Migrations are version-controlled DDL — treat parallel adds like parallel edits to one file. The snapshot is the merge conflict surface; the fix is one linear migration chain, not hand-editing two sibling `Up()` methods. See **Program.cs** Quick Reference — "Edit applied migration Up() by hand → history out of sync."

---

---

#### Q2. (R) A developer renames a column in the chapter's `Product` entity to match API naming:

```csharp
// Before
public decimal UnitPrice { get; set; }

// After
public decimal Price { get; set; }
```

They run `dotnet ef migrations add RenameUnitPriceToPrice` and inspect the generated `Up()`:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "UnitPrice",
        table: "Products");

    migrationBuilder.AddColumn<decimal>(
        name: "Price",
        table: "Products",
        type: "decimal(18,2)",
        nullable: false,
        defaultValue: 0m);
}
```

The migration has not been applied to production yet, but staging already has 50,000 product rows. What is wrong with shipping this migration as-is, and what would you change?

---

**Answer:**

```csharp
// Before
public decimal UnitPrice { get; set; }

// After
public decimal Price { get; set; }
```

They run `dotnet ef migrations add RenameUnitPriceToPrice` and inspect the generated `Up()`:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "UnitPrice",
        table: "Products");

    migrationBuilder.AddColumn<decimal>(
        name: "Price",
        table: "Products",
        type: "decimal(18,2)",
        nullable: false,
        defaultValue: 0m);
}
```

The migration has not been applied to production yet, but staging already has 50,000 product rows. What is wrong with shipping this migration as-is, and what would you change?

**Answer:** EF Core treats an unannotated property rename as drop-then-add, which **destroys existing `UnitPrice` values** and briefly leaves every row at the `defaultValue` of `0m` — silent data loss on staging/production.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `DropColumn` + `AddColumn` instead of rename | All historical prices wiped |
| Data | `defaultValue: 0m` on non-nullable column | Every product shows $0.00 after migrate |
| Operations | Looks like a safe migration in code review | Passes CI on empty LocalDB; fails in real data environments |

**Fix (priority order):**

1. **Before applying anywhere with data**, remove or revert the bad migration (`dotnet ef migrations remove` if not yet applied to shared DBs).
2. Regenerate with an explicit rename — **Package Manager Console:** `Add-Migration RenameUnitPriceToPrice` after `[Column("UnitPrice")]` on `Price` then second migration, or prefer editing `Up()` to:
   ```csharp
   migrationBuilder.RenameColumn(
       name: "UnitPrice",
       table: "Products",
       newName: "Price");
   ```
3. For complex renames, use **`dotnet ef migrations add ...` then hand-edit `Up()`/`Down()`** before first apply to shared environments — never edit after apply.
4. Validate on a **copy of staging data**: `SELECT COUNT(*) FROM Products WHERE Price = 0` should not jump to 50,000.
5. Add a team rule: **review generated `Up()` for `DropColumn`/`AddColumn` pairs** on entity renames — flag as data-loss risk in PR template.

**Production takeaway:** Code-First renames are not rename-safe by default; the generated migration is a suggestion. Karat tests whether you read `Up()` DDL, not just the C# property name. See **Models/Product.cs** — `UnitPrice` maps by convention; renaming the property changes the column mapping unless you intervene.

---

---

#### Q3. (R) A teammate "fixes" seeding so every deploy refreshes demo catalog data. Review the change to the chapter's `SeedSampleDataIfEmpty` pattern:

```csharp
public static void SeedCatalog(StoreDbContext context)
{
    if (!context.Categories.Any(c => c.Name == "Office"))
    {
        context.Categories.Add(new Category { Name = "Office" });
    }
    if (!context.Categories.Any(c => c.Name == "Field"))
    {
        context.Categories.Add(new Category { Name = "Field" });
    }
    context.SaveChanges();

    var officeId = context.Categories.Single(c => c.Name == "Office").CategoryId;

    context.Products.AddRange(
        new Product { Name = "Notebook", UnitPrice = 4.50m, CategoryId = officeId },
        new Product { Name = "Pen Pack", UnitPrice = 6.25m, CategoryId = officeId });
    context.SaveChanges();
}
```

`Program.Main` now calls `SeedCatalog(context)` on every startup after `Database.Migrate()`. Locally it looks fine; after two restarts in staging, duplicate `"Notebook"` rows appear and integration tests fail on product counts. Diagnose the idempotency gaps and describe a production-safe seeding approach.

---

**Answer:**

```csharp
public static void SeedCatalog(StoreDbContext context)
{
    if (!context.Categories.Any(c => c.Name == "Office"))
    {
        context.Categories.Add(new Category { Name = "Office" });
    }
    if (!context.Categories.Any(c => c.Name == "Field"))
    {
        context.Categories.Add(new Category { Name = "Field" });
    }
    context.SaveChanges();

    var officeId = context.Categories.Single(c => c.Name == "Office").CategoryId;

    context.Products.AddRange(
        new Product { Name = "Notebook", UnitPrice = 4.50m, CategoryId = officeId },
        new Product { Name = "Pen Pack", UnitPrice = 6.25m, CategoryId = officeId });
    context.SaveChanges();
}
```

`Program.Main` now calls `SeedCatalog(context)` on every startup after `Database.Migrate()`. Locally it looks fine; after two restarts in staging, duplicate `"Notebook"` rows appear and integration tests fail on product counts. Diagnose the idempotency gaps and describe a production-safe seeding approach.

**Answer:** Categories are guarded by name, but **products are inserted unconditionally on every startup**, so each restart adds another `"Notebook"` and `"Pen Pack"` — the chapter's original `if (context.Products.Any()) return;` guard was removed only halfway.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No idempotency check before `Products.AddRange` | Duplicate rows every deploy/restart |
| Design | Name-based category guard but not for products | Inconsistent seed contract |
| Testing | Passes first run; fails on second | Flaky integration tests in staging |
| Production | Startup seeding with partial guards | Data pollution in long-lived environments |

**Fix (priority order):**

1. Restore a **single gate** — either `if (context.Products.Any()) return;` (chapter pattern in **MigrationDemo.SeedSampleDataIfEmpty**) or check each product by natural key before insert.
2. Use **natural-key upsert** for reference data:
   ```csharp
   if (!context.Products.Any(p => p.Name == "Notebook"))
       context.Products.Add(new Product { ... });
   ```
3. Prefer **`HasData` in `OnModelCreating`** for static lookup tables — runs through migrations, versioned with schema (see EF Core seeding docs); use `Migrate()` once, not per-startup inserts.
4. For production reference data, use **idempotent SQL scripts** or a dedicated one-time migration/`INSERT ... WHERE NOT EXISTS` — not `Main` on every pod restart.
5. Separate **demo seed** (dev only) from **production bootstrap** (migrations/SQL) — gate demo seed behind `IHostEnvironment.IsDevelopment()`.

**Production takeaway:** Idempotent seeding means "safe to run N times" — guard every entity type or use migration-embedded seed. The chapter's `SeedSampleDataIfEmpty` is intentionally empty-check-first; partial guards are worse than no seed. See **Program.cs** Main — seed runs after `Migrate()`, so it executes on every instance in a scale-out deploy.

---

---

#### Q4. (P) A developer adds `public int StockQuantity { get; set; }` to `Product`, merges to `main`, and deploys. The pipeline builds and publishes the app but **does not** run `dotnet ef database update`. Production startup calls `context.Database.Migrate()` as in this chapter's `MigrationDemo.TryApplyMigrations`. Production database last applied migration is still `InitialCreate`. What fails first — build, startup, or first query — and what is the correct deploy sequence for schema changes?

---

**Answer:**

**Answer:** **Build succeeds** — the new property is only C# until a migration exists. If no new migration was generated, **startup `Migrate()` is a no-op** (still at `InitialCreate`), and the **first query or save involving `StockQuantity` fails** at runtime with a column mismatch (`Invalid column name 'StockQuantity'`) or silent mis-mapping if the column is missing from SELECT projections.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Process | Model changed without `dotnet ef migrations add` | App expects column that DB does not have |
| Deploy | Pipeline publishes app without schema step | Runtime failure after deploy |
| Assumption | "`Migrate()` on startup fixes everything" | Only applies **checked-in** migrations — not pending model edits |

**Correct deploy sequence (priority order):**

1. **Developer:** change entity → `dotnet ef migrations add AddProductStockQuantity` → commit migration + snapshot → PR review `Up()` DDL.
2. **CI:** build + test against database updated in test job (`dotnet ef database update` on LocalDB/Testcontainers).
3. **Deploy:** run **`dotnet ef database update`** (or apply generated SQL script) **before or as first step** of release — while old app still running if migration is backward-compatible, or during maintenance window if not.
4. **Then** deploy new app bits. If using startup `Migrate()`, treat it as a **safety net**, not the primary schema pipeline — and use **single-instance migration lock** or run migrations outside the web process to avoid races.
5. **Verify:** `SELECT MigrationId FROM __EFMigrationsHistory` includes the new migration; smoke test `Products` read/write.

**Production takeaway:** Pending **model** changes ≠ pending **migrations**. `Database.Migrate()` only replays files under `Migrations/` — see **MigrationDemo.TryApplyMigrations** and **Program.cs** Quick Reference row "Model change without new migration → Runtime errors / column mismatch."

---

---

#### Q5. (R) A throwaway integration test project created the `EfCoreTutorial` database with `EnsureCreated()`. The main app (this chapter) is deployed and calls `Database.Migrate()` on startup. Startup logs:

```
SqlException: There is already an object named 'Categories' in the database.
```

Review the catch block in this chapter's `MigrationDemo.TryApplyMigrations` — what state is the database in, why does `Migrate()` fail, and what are the safe recovery options for dev vs production?

---

**Answer:**

```
SqlException: There is already an object named 'Categories' in the database.
```

Review the catch block in this chapter's `MigrationDemo.TryApplyMigrations` — what state is the database in, why does `Migrate()` fail, and what are the safe recovery options for dev vs production?

**Answer:** The database has **physical tables but no (or incomplete) `__EFMigrationsHistory`** — `EnsureCreated()` built schema without recording migrations, so `Migrate()` tries to run `InitialCreate.Up()` and hits existing `Categories`/`Products` objects.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Schema path | `EnsureCreated()` then `Migrate()` on same database | Two incompatible schema evolution paths |
| History | Missing or empty `__EFMigrationsHistory` | EF thinks no migrations applied |
| Runtime | `CreateTable` in `Up()` against existing objects | SqlException — startup fails or demo skips |

**Fix (priority order):**

**Development / local (data disposable):**

1. `dotnet ef database drop --force` then `dotnet ef database update` — cleanest path per chapter message in **MigrationDemo** catch block.
2. Or drop only conflicting tables + history table, then `Migrate()`.

**Production (data must survive — rare for this exact mistake, but same class of problem):**

1. **Do not** `drop database` if data matters.
2. **Baseline** the database: create `__EFMigrationsHistory` if missing, insert row for `InitialCreate` (and any subsequent migrations whose DDL already exists) after verifying schema matches snapshot — effectively telling EF "these are already applied."
3. Generate **`dotnet ef migrations script`** and compare to live schema; fix drift manually with DBA review.
4. **Prevent:** ban `EnsureCreated()` outside throwaway tests; use dedicated test databases; document one schema path per environment.

**Production takeaway:** `EnsureCreated()` and migrations must never share a long-lived database — **MigrationCliReference.ExplainEnsureCreatedVsMigrate** and **Program.cs** Quick Reference. The chapter's catch handles this explicitly for LocalDB demos; production recovery is baseline/history repair, not blind drop.

---

---

#### Q6. (D) Your team debates where migrations run for the `StoreDbContext` / SQL Server app: **(A)** `Database.Migrate()` in `Program.cs` on every app startup, **(B)** `dotnet ef database update` in the CI/CD pipeline before swapping traffic, or **(C)** generating idempotent SQL scripts for DBAs to run manually. Under multi-instance Kubernetes, blue/green deploys, and strict change windows, which option(s) do you recommend and why?

---

### 04. Database-First & Reverse Engineering

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/04. Database-First & Reverse Engineering`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Prefer **(B) pipeline-applied migrations** or **(C) reviewed idempotent scripts** for production; use **(A) startup `Migrate()`** only for dev, test, and small single-instance apps — not as the primary strategy when multiple pods start concurrently or DBAs own change windows.

**Option A — `Database.Migrate()` at startup:**

- **Pros:** Simple; app self-heals dev/test; matches this chapter's **MigrationDemo** demo.
- **Cons:** **Race conditions** when N pods call `Migrate()` simultaneously; migrations run inside app security context; long `Up()` blocks startup probes; hard to fit strict maintenance windows; rollback couples app + schema.

**Option B — `dotnet ef database update` in CI/CD (recommended default):**

- Run as a **dedicated deploy step/job** before traffic shift; one execution per release.
- Use migration bundles (`dotnet ef migrations bundle`) for self-contained executables without SDK on agents.
- Pair with backward-compatible migrations when doing rolling deploys (expand-contract pattern).

**Option C — idempotent SQL scripts for DBAs:**

- Best when **separation of duties** requires DBA approval, auditing, or manual rollback scripts.
- `dotnet ef migrations script --idempotent` emits `IF NOT EXISTS` guards against partial applies.
- App deploy and schema deploy decouple — fits change windows; slower feedback loop.

**Recommendation matrix:**

| Context | Choice |
|---|---|
| Local / chapter demo | `Migrate()` in `Main` — **Program.cs** |
| CI integration tests | `database update` on ephemeral DB |
| Production K8s, multi-instance | **B** or **C** — never N parallel startup migrators |
| Regulated / DBA-owned SQL Server | **C** with reviewed scripts + history table verification |

**Production takeaway:** The mechanism (`Migrate()` vs CLI vs script) matters less than **exactly-once, ordered application** before new code depends on new columns — Karat tests deploy judgment tied to **StoreDbContext** and `__EFMigrationsHistory`, not CLI trivia. See **MigrationCliReference** Section 6 — production often runs updates from CI/CD instead of app startup.

---

### 04. Database-First & Reverse Engineering

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/04. Database-First & Reverse Engineering`

---

---

#### Q1. (R) A teammate "fixes" scaffold output by adding a computed helper directly in the generated entity, then runs re-scaffold with `--force` after a DBA adds a column. Review what they changed and predict what happens on the next build.

```csharp
// Models/Product.cs  (generated — edited by hand)
public partial class Product
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public virtual Category Category { get; set; } = null!;

    // Added during code review — "keeps logic near the entity"
    public decimal InventoryValue => UnitPrice * StockQuantity;
    public string StockStatus => StockQuantity > 0 ? "Available" : "Out of stock";
}
```

They did **not** create `Product.Partial.cs`. After `dotnet ef dbcontext scaffold ... --force`, CI reports missing members where services call `product.InventoryValue`. What went wrong, and what is the correct file layout?

---

**Answer:**

**Answer:** Hand-editing `Product.cs` violates the database-first contract — `--force` replaces every generated entity file, so `InventoryValue` and `StockStatus` disappear while the new column appears, and any code referencing those members fails to compile until the logic is moved to a partial file the scaffold never touches.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Workflow | Custom members in generated `Product.cs` | Lost on every `--force` re-scaffold |
| Compile | Services/views call removed properties | CS1061 — build blocked after re-scaffold |
| Maintainability | "Fix scaffold output by hand" anti-pattern | Team repeats the same lost edits each DBA release |
| Design | Computed helpers mixed with persistence shape | Blurs generated mapping vs domain logic |

**Fix (priority order):**

1. Move `InventoryValue` and `StockStatus` to `Models/Product.Partial.cs` with the same namespace and `partial class Product` (as in this chapter's tutorial layout).
2. Re-run scaffold with `--force`; verify `Product.cs` is regenerated cleanly and `Product.Partial.cs` is untouched.
3. Add a team rule: **never edit** `Models/*.cs` or `Data/*DbContext.cs` after initial scaffold — only `*.Partial.cs` and non-generated services.
4. Document the exact scaffold command in README or `ScaffoldWorkflow.SampleReScaffoldCommand` so everyone produces identical output.

**Production takeaway:** Re-scaffold is designed to overwrite generated files — Karat tests whether you know partial classes are the survival mechanism, not Git history. See **Program.cs** Section 7 and **ScaffoldWorkflow.cs** re-scaffold checklist.

---

---

#### Q2. (R) Review this partial-class attempt. The developer wants `DisplayLabel` and a `[NotMapped]` cache field to survive re-scaffold. What fails at compile time or at runtime, and how should the files be organized?

```csharp
// Models/CategoryHelpers.cs
namespace InventoryApp.Models;

public class CategoryHelpers
{
    public static string DisplayLabel(Category c) =>
        string.IsNullOrWhiteSpace(c.Description) ? c.Name : $"{c.Name} - {c.Description}";
}
```

```csharp
// Models/Category.Partial.cs
namespace InventoryApp.Models;

public partial class Category
{
    private Dictionary<int, int>? _productCountCache;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int CachedProductCount => _productCountCache?.Count ?? 0;
}
```

```csharp
// Models/Category.cs  (scaffolded)
namespace InventoryApp.Models;

public partial class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
```

Assume scaffold ran successfully once. The app calls `category.DisplayLabel` in a Razor view. What breaks?

---

**Answer:**

**Answer:** `CategoryHelpers` is a separate static class, so `category.DisplayLabel` does not exist on the entity — views or services calling that member fail at compile time. The partial file structure is otherwise correct for `[NotMapped]` members, but the display helper must live on `partial class Category`, not a sibling helper type.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `DisplayLabel` on static helper, not on `Category` | CS1061 if views call `category.DisplayLabel` |
| API design | Extension logic disconnected from entity | Callers pass `Category` into static helper — easy to bypass and duplicate |
| Partial pattern | `Category.Partial.cs` exists but omits the desired instance member | Re-scaffold survives, but the feature is incomplete |
| Runtime | `_productCountCache` never populated in snippet | `CachedProductCount` always 0 — silent logic bug if used for UI |

**Fix (priority order):**

1. Add `DisplayLabel` as an instance property on `partial class Category` in `Category.Partial.cs` (mirror this chapter's tutorial).
2. Delete or repurpose `CategoryHelpers.cs` — keep one place for category display logic.
3. If `CachedProductCount` is real, populate `_productCountCache` in application code or remove the dead field.
4. Confirm namespace matches scaffolded `Category.cs` exactly so partial merging works.

```csharp
public partial class Category
{
    public string DisplayLabel =>
        string.IsNullOrWhiteSpace(Description) ? Name : $"{Name} - {Description}";
}
```

**Production takeaway:** Partial classes require the **same type name and namespace** — a helper class is not a partial. Karat stacks "survives re-scaffold" knowledge with whether the API shape actually compiles.

---

---

#### Q3. (P) Production integrates with a legacy SQL Server database owned by a DBA team. They deploy a script renaming `dbo.Products.UnitPrice` to `ListPrice` and changing `StockQuantity` from `INT` to `SMALLINT`. Your team runs:

```bash
dotnet ef dbcontext scaffold "<conn>" Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir Models --context-dir Data --context InventoryDbContext --force
dotnet build
```

Walk through the **re-scaffold workflow** in order: what the scaffold overwrites, what survives, what fails first in the solution, and where fixes belong (generated vs partial vs app code).

---

**Answer:**

_Answer not found._

---

#### Q4. (D) You inherit a 15-year-old ERP database: `dbo` holds ~180 operational tables, `audit` holds change-log tables, and several tables use non-plural names (`tblCustMaster`, `OrdLine`). The app only reads `dbo.Categories`, `dbo.Products`, and `dbo.OrdLine`. How do you scaffold without dragging the entire schema into the model, and what **re-scaffold drift** risks appear when a new developer scaffolds without the same filters?

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Before re-scaffold, a developer tuned delete behavior and decimal precision directly in the generated DbContext. Review the diff they intend to keep "because scaffold gets it wrong":

```csharp
// Data/InventoryDbContext.cs  (generated — hand-edited)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.ProductId);
        entity.ToTable("Products");
        entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)"); // DBA uses 4 dp
        entity.HasOne(d => d.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // legacy DB forbids cascade
    });
    // OnModelCreatingPartial not called — removed "by mistake" during edit
}
```

They run `--force` re-scaffold next sprint. What regressions appear in production behavior and compile output?

---

**Answer:**

_Answer not found._

---

#### Q6. (M) Two developers re-scaffold the same legacy database on different branches. Dev A scaffolds with `--schema dbo --no-pluralize`; Dev B runs scaffold with no filters on a default template. Both commit generated files. At merge, `InventoryDbContext.cs` and entity files conflict heavily. Explain the **mechanism** of scaffold output drift and the team process you would put in place so re-scaffold stays repeatable (command, filters, partials, CI).

---

---

### 05. CRUD Operations & SaveChanges

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/05. CRUD Operations & SaveChanges`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

_Answer not found._

---

#### Q1. (R) An API endpoint maps a JSON body to `Product`, assigns `ProductId` from the route, changes `UnitPrice`, and calls `SaveChanges`. The response is 200 but the price in SQL is unchanged. Review this service method — what is wrong and how do you fix it?

```csharp
public void PatchPrice(AppDbContext context, int productId, decimal newPrice)
{
    var stub = new Product
    {
        ProductId = productId,
        UnitPrice = newPrice
    };
    // stub built from DTO — not loaded from context
    context.SaveChanges();
}
```

---

**Answer:**

**Answer:** The stub is **detached** — `SaveChanges` only executes SQL for entities the change tracker knows about in `Added`, `Modified`, or `Deleted` state. Building a `Product` in memory and never calling `Update()`, `Attach()`, or loading it with `Find` leaves nothing pending, so zero rows are written even though the method returns successfully.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracking | Detached entity never registered with context | `SaveChanges` returns 0; UPDATE never sent |
| API contract | HTTP 200 implies success | Silent data loss — hardest class of EF bug in production |
| Pattern | Confuses in-memory mutation with tracked update | Same mistake as tutorial "forget Update() on detached stub" |

**Fix (priority order):**

1. **Preferred:** Load tracked entity — `var product = context.Products.Find(productId); product.UnitPrice = newPrice;` then `SaveChanges()` (matches `TryUpdateUnitPriceTracked` in **ProductService.cs**).
2. **When stub is required:** `context.Products.Update(stub)` or `context.Entry(stub).Property(p => p.UnitPrice).IsModified = true` after attach — only if no other instance with the same key is already tracked.
3. Check `SaveChanges()` return value or verify row state in tests — do not assume flush happened.
4. Return 404 when `Find` returns null instead of no-op save.

**Production takeaway:** EF does not diff arbitrary objects against the database — only the change tracker drives SQL. See **Program.cs** Section 5b and **ProductService.UpdateProductDetached**.

---

---

#### Q2. (R) A teammate "fixes" detached updates by always calling `DbSet.Update()`. Partial PATCH requests now wipe columns the client did not send. Review this controller helper — what breaks, and what pattern fixes it without overwriting untouched fields?

```csharp
public async Task UpdateFromDto(AppDbContext context, ProductDto dto)
{
    var entity = new Product
    {
        ProductId = dto.Id,
        ProductName = dto.Name,
        UnitPrice = dto.Price
        // StockQuantity and IsDiscontinued not in DTO — default to 0 / false
    };
    context.Products.Update(entity);
    await context.SaveChangesAsync();
}
```

---

**Answer:**

**Answer:** `DbSet.Update()` is equivalent to **Attach + mark entire entity Modified**, so every mapped property is included in the UPDATE statement. Unset properties (`StockQuantity = 0`, `IsDiscontinued = false`) overwrite existing database values — a classic partial-update bug.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `Update()` marks **all** scalar properties Modified | Columns not in DTO reset to defaults |
| Data integrity | `StockQuantity` zeroed, flags cleared | Inventory and status corrupted on PATCH |
| Design | Treating PATCH like full PUT | Works in demos with complete objects; fails with real DTOs |

**Fix (priority order):**

1. Load the tracked entity with `Find` or a query, mutate only DTO-supplied properties, then `SaveChanges()` — change tracker emits UPDATE only for changed columns (default).
2. For detached graphs: attach then mark individual properties — `context.Attach(entity); context.Entry(entity).Property(p => p.UnitPrice).IsModified = true;` (or `Entry(...).CurrentValues.SetValues(dto)` with explicit field list).
3. Use a dedicated PATCH model with nullable fields; apply only non-null members to the tracked entity.
4. Reserve `Update(entity)` for full replacement scenarios (PUT) where every column is intentionally supplied.

**Production takeaway:** Attach vs Update is not interchangeable with PATCH semantics — **Update means "replace whole row."** See **ProductService.cs** Section 5 comments on `Update()` attaching + Modified for all mapped props.

---

---

#### Q3. (M) This code mirrors **Program.cs Section 5b** but skips `ChangeTracker.Clear()`. What exception or silent failure do you expect at runtime, and what are two safe ways to update a detached stub when another instance might already be tracked?

```csharp
public void RenameProduct(AppDbContext context, int id, string newName)
{
    Product? tracked = context.Products.Find(id);
    tracked!.ProductName = "stale name from earlier step"; // still Modified in tracker

    var stub = new Product { ProductId = id, ProductName = newName, UnitPrice = 9.99m };
    context.Products.Update(stub);
    context.SaveChanges();
}
```

---

**Answer:**

**Answer:** EF Core allows **only one tracked instance per key** per context. Calling `Update(stub)` while `tracked` is still in the change tracker throws `InvalidOperationException` — "The instance of entity type 'Product' cannot be tracked because another instance with the same key value is already being tracked."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracker | Two instances, same `ProductId` | Hard failure on `Update()` |
| Workflow | Earlier mutation left entity Modified | Even Unchanged duplicate keys conflict on attach |
| Demo gap | Tutorial clears tracker before detached update | Production code copying 5b without `Clear()` breaks |

**Fix (priority order):**

1. **Best:** Reuse the tracked instance — mutate `tracked.ProductName = newName` and drop the stub entirely (no second instance).
2. Detach before attach: `context.Entry(tracked).State = EntityState.Detached` or `context.ChangeTracker.Clear()` (as in **Program.cs** line 118) then `Update(stub)` — only when you intentionally discard prior pending changes on that context.
3. Use a **new short-lived DbContext** per operation in web apps (scoped DI) so stale tracked entities from earlier steps in the same request are less common — still prefer single-instance update.
4. Never call `Update()` on a stub when you already have the entity from `Find` in the same method.

**Production takeaway:** Karat tests whether you read the tutorial's `ChangeTracker.Clear()` comment as a **symptom fix** and know the production fix is **one tracked instance per key**. Forward reference: attach patterns in ch11 Change Tracking.

---

---

#### Q4. (P) A catalog import reads 10,000 CSV rows and persists each with `Add` + `SaveChanges` inside the loop. It works on 50 rows in QA but times out in production. What is wrong with the persistence pattern, and what would you change?

```csharp
foreach (var row in csvRows)
{
    context.Products.Add(MapRow(row));
    context.SaveChanges(); // one round trip per row
}
```

---

**Answer:**

**Answer:** Each `SaveChanges()` opens a transaction, generates INSERT SQL, waits for the database, and resets change-tracker state — **10,000 round trips** instead of one batched unit of work. Throughput collapses under network latency, log growth, and lock duration even though the logic is "correct."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | SaveChanges per iteration | N transactions; import timeouts |
| Scalability | Change tracker churn | Memory and CPU on tracker between flushes |
| Operations | Long-held connections | Pool exhaustion; harder to retry atomically |

**Fix (priority order):**

1. Batch: `Add` / `AddRange` many entities, then **one** `SaveChanges()` per chunk (e.g. 500–1000 rows) — matches **Program.cs** Section 7 batch pattern.
2. For very large loads, consider `BulkInsert` extensions or raw SQL/`SqlBulkCopy` when EF change tracking is unnecessary.
3. Wrap each chunk in an explicit transaction if all-or-nothing per batch is required.
4. Use `SaveChangesAsync` with cancellation for API-hosted imports.

**Production takeaway:** `SaveChanges` is the **commit boundary**, not a per-row persist call — the chapter demo batches Add + Update + Remove before a single flush for exactly this reason. See **ProductService.cs** Section 7.

---

---

#### Q5. (P) Two admins edit the same product. Admin A loads price 10.00, Admin B loads price 10.00; B saves 12.00, then A saves 11.00. No error is thrown. The `Product` entity has no concurrency column. What data-loss scenario is this, and how would you add optimistic concurrency in EF Core?

---

**Answer:**

**Answer:** This is **last-write-wins** — EF's default UPDATE uses only the primary key in the WHERE clause, so Admin A's save overwrites B's 12.00 with 11.00 without detecting that the row changed in between. No concurrency token means no `DbUpdateConcurrencyException` and no signal to retry or merge.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Stale read persisted silently | Lost update; audit trail shows both "success" |
| Collaboration | No conflict detection | Support tickets: "my price change disappeared" |
| Schema | `Product` has no token column | EF cannot compare original vs current row version |

**Fix (priority order):**

1. Add a concurrency property — e.g. `public byte[] RowVersion { get; set; }` with `[Timestamp]` or fluent `IsRowVersion()`, or a dedicated `uint Version` with `[ConcurrencyCheck]`.
2. On `SaveChanges`, EF includes the token in the WHERE clause; mismatch throws **`DbUpdateConcurrencyException`** — catch, reload, merge or return 409 Conflict to the client.
3. Expose current token to clients (ETag header or DTO field) so updates send **If-Match** semantics.
4. For high-contention fields, combine tokens with domain rules (minimum price floors) validated before save (Q6).

**Production takeaway:** CRUD without concurrency tokens is fine for tutorials; multi-user production catalogs need optimistic concurrency or explicit locking. This chapter's `Product` entity intentionally omits tokens — migrations chapter adds schema evolution.

---

---

#### Q6. (R) A bulk pricing endpoint accepts client-supplied `UnitPrice` and `StockQuantity`, calls `AddProduct` + `SaveChanges`, and returns the new `ProductId`. Negative prices and zero stock slip through to SQL. Review this flow — what is missing before `SaveChanges`, and where should validation live in a real API?

```csharp
public Product AddProduct(string name, decimal unitPrice, int stockQuantity)
{
    var product = new Product
    {
        ProductName = name,
        UnitPrice = unitPrice,
        StockQuantity = stockQuantity
    };
    _context.Products.Add(product);
    return product;
}

// caller
var created = service.AddProduct(dto.Name, dto.UnitPrice, dto.StockQuantity);
service.SaveChanges();
return Ok(created.ProductId);
```

---

---

### 06. Relationships & Navigation Properties

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/06. Relationships & Navigation Properties`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** EF Core persists whatever the change tracker holds — it does **not** validate business rules. `AddProduct` only constructs an entity and sets `Added`; without guards, invalid scalars become valid INSERTs. Validation must run **before** `SaveChanges`, ideally before the entity enters the tracker.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Domain rules | No checks on price/stock | Negative prices, bad inventory in production DB |
| Layering | Controller trusts DTO blindly | Invalid data crosses API boundary |
| EF behavior | `SaveChanges` succeeds on invalid business data | Silent acceptance — constraints only if DB CHECK/FK exist |

**Fix (priority order):**

1. Validate in the API: Data Annotations on DTO (`[Range]`) + `ModelState`, or FluentValidation in the controller/minimal-API filter **before** calling the service.
2. Enforce invariants in the service/domain layer — throw `ArgumentOutOfRangeException` or return `Result` failures so non-HTTP callers are also protected.
3. Add database CHECK constraints as last line of defense (does not replace app validation UX).
4. Optionally implement `SaveChanges` override or `IValidatableObject` on the entity — still keep API-level validation for early 400 responses.

**Production takeaway:** **SaveChanges is not a validation gate** — it translates tracked state to SQL. Pair CRUD services (like **ProductService.AddProduct**) with explicit validation and ProblemDetails for 400/409 responses in ASP.NET Core (see ch08 Model Binding & Validation). Return meaningful errors before flush, not after bad rows land in SQL.

---

---

### 06. Relationships & Navigation Properties

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/06. Relationships & Navigation Properties`

---

---

#### Q1. (R) An admin "deactivate customer" endpoint loads a `Customer` with `Include(c => c.Orders)` and calls `context.Customers.Remove(customer)` + `SaveChanges`, expecting only the customer row to flip an `IsActive` flag. Support reports missing order history. Review the relationship setup and delete path — what actually happens in SQL, and how do you prevent accidental data loss?

```csharp
// RelationshipsDbContext — unchanged from tutorial
modelBuilder.Entity<Order>(entity =>
{
    entity.HasOne(order => order.Customer)
        .WithMany(customer => customer.Orders)
        .HasForeignKey(order => order.CustomerId)
        .OnDelete(DeleteBehavior.Cascade);
});

// AdminService.cs
public void DeactivateCustomer(RelationshipsDbContext context, int customerId)
{
    var customer = context.Customers
        .Include(c => c.Orders)
        .First(c => c.CustomerId == customerId);
    // TODO: set IsActive = false — never implemented
    context.Customers.Remove(customer);
    context.SaveChanges();
}
```

---

**Answer:**

**Answer:** `Remove(customer)` is a **physical delete**, and `DeleteBehavior.Cascade` on `Order`→`Customer` tells SQL Server to **DELETE all dependent order rows** when the customer row is deleted — not deactivate anything. The missing `IsActive` assignment means the code never implemented soft delete; it wiped the principal and cascaded to dependents exactly as **DemonstrateCascadeDelete** in **Program.cs** shows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Delete behavior | `OnDelete(Cascade)` on Customer→Orders | All order history deleted with customer |
| Intent vs API | `Remove()` used for "deactivate" | Irreversible data loss; audit/compliance failure |
| Design | No soft-delete column or Restrict | Business rule "keep orders" incompatible with cascade |

**Fix (priority order):**

1. Implement **soft delete**: add `IsActive` / `DeletedAt`, query with global filter, **never** `Remove` for deactivation — update scalar flags only.
2. If physical delete is required, change to `DeleteBehavior.Restrict` (or `NoAction`) on Customer→Orders so delete fails until orders are archived/moved explicitly.
3. Document cascade paths in `OnModelCreating` — long cascade chains (Customer→Orders→LineItems) multiply surprise deletions.
4. Return order count in admin UI before confirm; use integration tests that assert orders survive deactivation.

**Production takeaway:** EF defaults and explicit **Cascade** are convenient for graph cleanup in demos (**Program.cs** lines 200–205) but dangerous when product language says "deactivate." Prefer Restrict + explicit cleanup or soft-delete for customer/principal entities with valuable dependents.

---

---

#### Q2. (R) A teammate makes `Order.CustomerId` nullable so "orphan orders" can exist after a customer GDPR erasure, and configures `OnDelete(DeleteBehavior.SetNull)`. Review the model and fluent config — what breaks at migration/runtime, and what is the correct shape for an optional relationship?

```csharp
public sealed class Order
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }           // changed from int
    public Customer? Customer { get; set; }        // changed from Customer = null!
    // ...
}

modelBuilder.Entity<Order>(entity =>
{
    entity.HasOne(order => order.Customer)
        .WithMany(customer => customer.Orders)
        .HasForeignKey(order => order.CustomerId)
        .OnDelete(DeleteBehavior.SetNull)
        .IsRequired(false);
});
```

---

**Answer:**

**Answer:** The fluent snippet is internally consistent for an **optional** relationship — nullable FK + `IsRequired(false)` + `SetNull` is the valid combination. The surprise is twofold: (1) if any code still treats `Customer` as required (`null!`, `.Customer.Name` without null check), you get null-reference bugs after GDPR erasure; (2) if someone leaves `CustomerId` as non-nullable `int` while calling `SetNull`, migration or runtime FK constraint creation fails because SQL cannot SET NULL on a NOT NULL column.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Required vs optional | Mixing `int` FK with `SetNull` | Migration/DB error: cannot null non-nullable column |
| Semantics | Optional FK without null-safe queries | `Include` + dereference throws after customer deleted |
| GDPR | SetNull orphans orders | Correct for retention policy but breaks reports keyed by customer |

**Fix (priority order):**

1. Align all three: nullable `int? CustomerId`, nullable `Customer?` navigation, `IsRequired(false)`, `OnDelete(SetNull)` — only for truly optional associations.
2. Update LINQ and DTOs for null customer — `order.Customer?.Name ?? "Redacted"`.
3. If orders **must** always have a customer (this chapter's **Order.cs** uses required `int CustomerId`), keep required FK and use **Restrict** or archive workflow instead of SetNull.
4. Add filtered indexes and reporting views for orphan orders if GDPR requires principal deletion while retaining financial records.

**Production takeaway:** **SetNull applies only to optional FKs** — see **Program.cs** quick reference (`SetNull — optional FK only`). Required relationships default to Cascade on SQL Server; making FK nullable is a domain decision, not just a delete-behavior tweak.

---

---

#### Q3. (M) A legacy `Comment` entity has a navigation to `BlogPost` but **no** `BlogPostId` scalar. A developer writes this query and gets a compile error. Explain how EF models the FK, how to query/filter without a CLR property, and one production pitfall when inserting graphs.

```csharp
public sealed class Comment
{
    public int CommentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public BlogPost BlogPost { get; set; } = null!; // no BlogPostId property
}

// Fails to compile:
var comments = context.Comments
    .Where(c => c.BlogPostId == postId)
    .ToList();
```

---

**Answer:**

**Answer:** EF Core creates a **shadow property** — a FK column (`BlogPostId`) in the model and database without a CLR property on `Comment`. You cannot reference `c.BlogPostId` in LINQ because it does not exist on the class; filter via navigation or `EF.Property<int>(c, "BlogPostId")`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Model | No scalar FK on entity | Compile error on direct property access |
| Query | Shadow FK invisible in C# | Developers guess wrong column name |
| Graph insert | Set navigation only on one side | Usually works; setting mismatched shadow FK via entry API causes silent wrong links |

**Fix (priority order):**

1. **Preferred:** Expose FK explicitly — add `public int BlogPostId { get; set; }` paired with `BlogPost` navigation (same pattern as **Order.CustomerId** in **Models/Order.cs**).
2. **Query without CLR FK:** `context.Comments.Where(c => c.BlogPost.PostId == postId)` or `EF.Property<int>(c, "BlogPostId") == postId`.
3. **Inspect model:** `context.Model.FindEntityType(typeof(Comment))!.FindProperty("BlogPostId")` or migration snapshot to confirm shadow name.
4. **Graph insert pitfall:** Attaching a `Comment` with `BlogPost` navigation pointing to an untracked `BlogPost` stub with wrong key can fix-up incorrectly — assign FK scalar or attach principal first.

**Production takeaway:** Shadow FKs work for reverse-engineered schemas but hurt maintainability in team codebases. Explicit `{Navigation}Id` scalars match this chapter's convention and avoid "property does not exist" failures in LINQ projections and APIs.

---

---

#### Q4. (R) A catalog import links products to tags using the chapter's `ProductTag` join entity. CI passes on 10 rows but production throws on duplicate key. Review the import loop — what relationship rule is violated, and how do you fix idempotent linking when `AddedOn` payload matters?

```csharp
foreach (var row in importRows)
{
    var product = context.Products.First(p => p.Sku == row.Sku);
    var tag = context.Tags.First(t => t.Label == row.TagLabel);

    product.ProductTags.Add(new ProductTag
    {
        Tag = tag,
        AddedOn = row.AddedOn
    });
    context.SaveChanges(); // throws on re-run for same (ProductId, TagId)
}
```

---

**Answer:**

**Answer:** `ProductTag` has a **composite primary key** `(ProductId, TagId)` configured in **RelationshipsDbContext** — inserting the same pair twice violates the PK on re-run. Per-row `SaveChanges` does not cause the duplicate; re-importing the same SKU/tag pair does. This is why the chapter uses an explicit join entity instead of blind skip navigation when you need payload columns like `AddedOn`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cardinality | Duplicate `(ProductId, TagId)` | `DbUpdateException` on PK violation |
| Idempotency | No existence check before Add | Re-run imports fail in production |
| Join entity | Payload on link row | Cannot replace with naive skip M2M without losing `AddedOn` |

**Fix (priority order):**

1. Upsert pattern — query first (as **Program.cs** `DemonstrateManyToManyJoinEntity` lines 127–128): `if (!context.ProductTags.Any(...))` then add.
2. Or load existing link and update `AddedOn` only when the row exists; add when missing.
3. Batch `SaveChanges` once per chunk after deduplicating in memory — not once per row.
4. Keep explicit `ProductTag` entity when payload exists; skip navigation is for link tables **without** extra columns (**RelationshipsDbContext** Section 5b comment).

**Production takeaway:** Many-to-many join entities inherit **relational PK rules** — one row per pair. Production imports must be idempotent; the tutorial demo already guards with `linkExists` before insert.

---

---

#### Q5. (R) A checkout service builds an order graph in one `DbContext` but relationships look wrong after `SaveChanges` — wrong customer on the order, or `InvalidOperationException` about duplicate tracked instances. Review this method — diagnose the fix-up / tracking issues and show the corrected graph insert.

```csharp
public void PlaceOrder(RelationshipsDbContext context, int customerId, decimal total)
{
    var customerA = context.Customers.Find(customerId);
    var customerB = new Customer { CustomerId = customerId, Name = "cached" };

    var order = new Order
    {
        OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
        TotalAmount = total,
        CustomerId = customerId,
        Customer = customerB   // different instance than customerA
    };

    context.Orders.Add(order);
    context.SaveChanges();
}
```

---

**Answer:**

**Answer:** The method creates **two different `Customer` instances with the same key** — `customerA` from `Find` (tracked) and `customerB` (new stub). Setting `order.Customer = customerB` while also setting `CustomerId` triggers EF **relationship fix-up** and change-tracker conflicts: either duplicate-key tracking exception or fix-up replacing navigations with inconsistent state.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracker | Two instances, same `CustomerId` | `InvalidOperationException` on track/fix-up |
| Graph consistency | FK scalar + navigation point at different instances | Undefined which customer wins after SaveChanges |
| Pattern | Manual stub instead of navigation graph | Breaks **Program.cs** Section 2–3 graph-insert pattern |

**Fix (priority order):**

1. **Use one instance** — set `order.Customer = customerA` (from `Find`) and remove redundant `customerB`; or omit `CustomerId` and let EF set FK from navigation.
2. **Graph insert from principal** (tutorial pattern):

```csharp
var customer = context.Customers.Find(customerId)!;
customer.Orders.Add(new Order
{
    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
    TotalAmount = total
});
context.SaveChanges();
```

3. Never new-up a principal with an existing key in the same context unless you `Attach` with correct state intentionally.
4. If only FK is known: set `CustomerId` only, leave `Customer` null until loaded — do not assign a second stub instance.

**Production takeaway:** EF **fix-up** wires navigations and FKs bidirectionally for tracked graphs — mixed instances with the same key are a common Karat trap. Follow **DemonstrateOneToManyGraphInsert** in **Program.cs**: one tracked graph, EF sets `Order.CustomerId` on save.

---

---

#### Q6. (D) Product managers want to delete unused `Tag` rows from an admin screen. The model matches **Program.cs Section 6b** — `Product`→`ProductTag` is Cascade, `Tag`→`ProductTag` is Restrict. A delete request returns a SQL FK exception. Explain the delete behavior from the database's perspective, and compare fixing this with Restrict+cleanup vs switching Tag side to Cascade vs soft-delete on tags.

---

---

### 07. Fluent API & Data Annotations

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/07. Fluent API & Data Annotations`
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** `DeleteBehavior.Restrict` on `Tag`→`ProductTag` maps to a FK that **blocks deleting a Tag while any ProductTag row references it** — SQL Server raises a reference constraint error. Deleting a **Product** still cascades to its ProductTag rows only; shared tags used by other products remain referenced, so Tag delete correctly fails until links are removed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Delete behavior | Restrict on Tag side | Admin delete Tag fails while links exist |
| UX | UI offers "delete tag" without unlink | 500 / unhandled DbUpdateException |
| Shared tags | Tag used by many products | Physical delete affects catalog semantics |

**Fix (priority order):**

1. **Restrict + explicit cleanup (recommended for shared tags):** Delete or reassign `ProductTag` rows first, then delete Tag — matches tutorial intent (**Program.cs** lines 223–230: Product delete removes links, Tag survives).
2. **Cascade on Tag side:** Deleting Tag would remove **all** ProductTag links catalog-wide — rarely desired for shared labels like "Sale"; risks accidental mass unlinking.
3. **Soft-delete tags:** `IsActive` flag + filtered queries; keep FK integrity and history; best when tags appear in historical reports.
4. Handle `DbUpdateException` in API → 409 Conflict with "Tag in use by N products."

**Production takeaway:** Many-to-many with explicit join entity gives **independent delete rules per FK** — Product cascade vs Tag restrict is deliberate (**RelationshipsDbContext** lines 90–98). Karat tests whether you read cascade direction per relationship, not "cascade everything." Shared dimension rows (tags, categories) almost always need Restrict or soft-delete, not cascade from the lookup table.

---

---

### 07. Fluent API & Data Annotations

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/07. Fluent API & Data Annotations`

---

---

#### Q1. (R) After a schema review, QA reports that `CatalogProducts.Name` truncates at 50 characters even though the entity still shows `[MaxLength(100)]`. Review the configuration split across files. What conflicted, what actually landed in SQL Server, and how do you fix it so the team is not surprised again?

```csharp
// Models/CatalogProduct.cs
[Required(AllowEmptyStrings = false)]
[MaxLength(100)]
public string Name { get; set; } = string.Empty;

// Data/CatalogDbContext.cs — added during a hotfix
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<CatalogProduct>(entity =>
    {
        entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
    });
}
```

---

**Answer:**

**Answer:** EF Core applied **both** configuration sources, but when annotation and Fluent API disagree on the same mapping facet, **Fluent API wins** — the column is `NVARCHAR(50)`, not 100. Developers reading only `CatalogProduct.cs` will misdiagnose truncation as an application bug.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Mapping conflict | `[MaxLength(100)]` vs `HasMaxLength(50)` on same property | Schema uses 50; annotation misleads code review and API docs |
| Maintainability | Two sources of truth for one column | Future edits update only one side; silent drift repeats |
| Validation | `[StringLength]` / data-annotation validators may still use 100 in some paths | Client-side or manual validation can disagree with database cap |

**Fix (priority order):**

1. Pick **one** authoritative rule per property — remove the loser (`[MaxLength(100)]` **or** the Fluent `HasMaxLength(50)`), matching the intended business limit.
2. Regenerate or add a migration so the database, model snapshot, and entity agree; verify with `dotnet ef migrations script` or SSMS column definition.
3. Adopt team convention: simple per-property rules on the entity **or** centralized Fluent/`IEntityTypeConfiguration<T>` — not both for the same facet (matches **CatalogDbContext.cs** precedence note).
4. Add a PR checklist item: "no duplicate mapping facets across annotations and Fluent."

**Production takeaway:** Karat uses dual-configuration drift to test whether you know Fluent wins at runtime — reading the entity file alone is not enough. See this chapter's **CatalogProduct** (annotations) vs **Supplier** (Fluent-only) split.

---

---

#### Q2. (R) Finance rejects migrated totals: penny amounts drift after bulk price imports. The team points at `CatalogProduct.UnitPrice`. Review the entity and migration snippet. What is wrong with the money mapping, and what do you change before the next deploy?

```csharp
// Models/CatalogProduct.cs
public decimal UnitPrice { get; set; }

// Generated migration Up()
migrationBuilder.CreateTable(
    name: "CatalogProducts",
    columns: table => new
    {
        UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
    });
```

A separate branch added Fluent configuration:

```csharp
entity.Property(e => e.UnitPrice).HasPrecision(19, 4);
```

…but left `[Column(TypeName = "decimal(18,2)")]` on the property from an earlier scaffold.

---

**Answer:**

**Answer:** Money columns need an explicit **precision and scale** (`HasPrecision` / `[Precision]`); relying on SQL Server's default `decimal(18,2)` silently rounds values like `12.3456789m` to two decimal places at persistence. When Fluent `HasPrecision(19,4)` conflicts with `[Column(TypeName = "decimal(18,2)")]`, **Fluent API wins** — but stale attributes and old migrations still confuse reviewers and can block clean diffs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Default `decimal(18,2)` without explicit facet | Sub-cent amounts rounded on INSERT/UPDATE; finance reconciliation fails |
| Mapping conflict | `[Column(TypeName = "decimal(18,2)")]` vs `HasPrecision(19, 4)` | Team unsure which scale is live; annotation lies on scaffolded entity |
| Migration hygiene | Existing migration hard-codes `decimal(18,2)` | New precision requires a new migration; altering column may lock large tables |

**Fix (priority order):**

1. Decide business scale (e.g., `(19,4)` for unit prices) and configure **once**: `entity.Property(e => e.UnitPrice).HasPrecision(19, 4);` or `[Precision(19, 4)]` — not both conflicting.
2. Remove obsolete `[Column(TypeName = ...)]` if Fluent owns the facet; regenerate migration to `ALTER COLUMN` with correct precision/scale.
3. Validate rounding rules in the domain layer if display/storage scales differ (store 4 dp, present 2 dp in UI).
4. Add integration tests that insert boundary decimals and assert round-trip equality from SQL.

```csharp
entity.Property(e => e.UnitPrice).HasPrecision(19, 4);
```

**Production takeaway:** `decimal` in C# does not imply database scale — EF must map money explicitly. Conflicting annotation + Fluent on precision is the same "two bosses" trap as max length. See **CatalogProduct.UnitPrice** comment in this chapter (`decimal(18,2)` by convention).

---

---

#### Q3. (R) Production paging on order history is slow after go-live. The `OrderLines` table has ~2M rows. Review the relationship configuration — FK exists, but DBAs see a table scan on every lookup by product. What is missing, and how do you fix it in EF Core?

```csharp
public class OrderLine
{
    public int OrderLineId { get; set; }
    public int CatalogProductId { get; set; }
    public int Quantity { get; set; }
    public CatalogProduct Product { get; set; } = null!;
}

// CatalogDbContext.OnModelCreating
modelBuilder.Entity<OrderLine>(entity =>
{
    entity.HasOne(e => e.Product)
          .WithMany()
          .HasForeignKey(e => e.CatalogProductId)
          .OnDelete(DeleteBehavior.Restrict);
    // no index configuration
});
```

---

**Answer:**

**Answer:** EF Core creates the **foreign-key constraint** but does **not** automatically create a **nonclustered index** on the FK column. Without `HasIndex(e => e.CatalogProductId)`, queries filtering or joining on `CatalogProductId` scan the heap as row count grows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | No index on `CatalogProductId` FK column | Table scans on `WHERE CatalogProductId = @id`; slow joins and paging |
| Operations | FK present so dev assumes "indexed" | Local dev with tiny data hides issue; prod latency spikes after scale |
| Design | Only relationship configured, not access paths | Common EF misconception — constraint ≠ index |

**Fix (priority order):**

1. Add index in Fluent API: `entity.HasIndex(e => e.CatalogProductId);` (name it in migrations for clarity).
2. Generate and deploy migration; verify plan uses seek/lookup in SSMS/`SET STATISTICS IO ON`.
3. For composite queries (e.g., product + date range), consider composite index `{ CatalogProductId, CreatedUtc }` based on actual query shapes.
4. Document team rule: **every FK used in filters/joins gets an index** unless a covering clustered key already serves it.

```csharp
modelBuilder.Entity<OrderLine>(entity =>
{
    entity.HasOne(e => e.Product)
          .WithMany()
          .HasForeignKey(e => e.CatalogProductId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(e => e.CatalogProductId);
});
```

**Production takeaway:** This chapter teaches `HasIndex` for **unique** business keys (**SupplierCode**); the same API applies to FK columns — uniqueness is optional, seek performance is the goal. Contrast with **DemonstrateUniqueIndexViolation** in **Program.cs**.

---

---

#### Q4. (R) With `#nullable enable`, a developer "fixes" compiler warnings on `CatalogProduct` but SaveChanges behavior changes in staging. Review the property declarations. What do nullable reference types vs `[Required]` each control, and what would you standardize?

```csharp
#nullable enable

public class CatalogProduct
{
    public int CatalogProductId { get; set; }

    // Developer removed [Required] — "string is already non-nullable"
    public string Name { get; set; } = string.Empty;

    [Required]
    public string? Sku { get; set; }

    public string? Description { get; set; }
}
```

---

**Answer:**

**Answer:** **Nullable reference types** are a **compile-time** C# feature; **`[Required]`** is an EF Core / DataAnnotations **mapping and validation** facet. A non-nullable `string Name` without `[Required]` is still mapped **required by EF convention**, but removing `[Required(AllowEmptyStrings = false)]` allows empty string at validation. `[Required]` on `string? Sku` is contradictory — the column is required in the model while the type promises nullability to the compiler.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `string Name` without `[Required(AllowEmptyStrings = false)]` | `""` may pass where business forbids empty names (**Program.cs** demo pitfall) |
| Type/model mismatch | `[Required]` on `string? Sku` | CS8618 quieted but intent unclear; reviewers assume Sku optional |
| Layer confusion | Treating NRT as substitute for EF attributes | Compiler green; database/validation rules diverge from API contracts |

**Fix (priority order):**

1. Align each property: required value → `string` + `[Required(AllowEmptyStrings = false)]` when empty string must fail; optional → `string?` without `[Required]`.
2. Remove `[Required]` from nullable reference properties unless you intentionally require non-null at persistence (then use non-nullable `string` with initializer).
3. Standardize: **NRT for C# contracts**, **`[Required]` / Fluent `IsRequired()` for persistence and SaveChanges validation** — document on the team wiki.
4. Mirror rules in ASP.NET model binding if the same entities surface in APIs.

```csharp
[Required(AllowEmptyStrings = false)]
[MaxLength(100)]
public string Name { get; set; } = string.Empty;

[StringLength(20, MinimumLength = 3)]
public string Sku { get; set; } = string.Empty;

public string? Description { get; set; }
```

**Production takeaway:** Karat stacks NRT and EF validation — they solve different problems. See **CatalogProduct.cs** Section 1 on `[Required]` vs conventions and empty-string default.

---

---

#### Q5. (P) Your team inherits scaffolded `Supplier` rows from a legacy database and adds `[StringLength(20)]` on `SupplierCode` while `CatalogDbContext` already calls `HasIndex(e => e.SupplierCode).IsUnique()` and `HasMaxLength(20)` in Fluent API. Builds succeed; tests pass locally. What production risks remain if you leave both styles on the same property, and what convention would you enforce?

---

**Answer:**

**Answer:** When values agree, EF merges facets and tests pass, but **duplicate configuration** still creates drift risk on the next edit — someone updates the annotation during a scaffold regen and misses Fluent, or vice versa. Indexes and uniqueness belong in Fluent anyway; sprinkling annotations on a POCO designed for **OnModelCreating** (this chapter's **Supplier**) fights the intended architecture.

- **Drift risk:** Scaffold regen overwrites entity attributes; Fluent block unchanged → next migration surprises (length, required, index name).
- **Review blind spot:** Reviewers see `[StringLength(20)]` on the class and skip **CatalogDbContext** — unique index or filtered index rules stay invisible.
- **Validation duplication:** DataAnnotations validators and EF Fluent may both apply; conflicting error messages in API vs console paths.
- **Convention:** Keep **Supplier** annotation-free; all mapping in `ConfigureSupplier` or `IEntityTypeConfiguration<Supplier>`. Use annotations on **CatalogProduct**-style simple entities only when the team owns the file and won't rescaffold.
- **Production enforcement:** Analyzer or PR rule — "no `[MaxLength]` / `[Required]` on entities configured in Fluent for the same property"; single `IEntityTypeConfiguration` per aggregate in large apps.

**Production takeaway:** Builds succeeding does not mean configuration is maintainable — Karat tests judgment about **one boss per facet**, not whether EF can merge identical duplicates today.

---

---

#### Q6. (R) A pricing microservice bulk-inserts catalog rows. One bad batch passes C# compilation and EF model validation but corrupts amounts in SQL Server. Review the insert path and configuration. List the issues and prioritized fixes.

```csharp
var batch = new[]
{
    new CatalogProduct { Name = "Widget", Sku = "WDG-001", UnitPrice = 12.3456789m },
    new CatalogProduct { Name = "Gadget", Sku = "GDG", UnitPrice = 0m }
};

context.CatalogProducts.AddRange(batch);
await context.SaveChangesAsync();

// Entity — no precision facet
public decimal UnitPrice { get; set; }

// DbContext — only Supplier uses Fluent; CatalogProduct is annotation-only
```

---

---

### 08. LINQ to Entities & Query Patterns

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/08. LINQ to Entities & Query Patterns`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** The batch relies on implicit `decimal(18,2)` mapping, so high-precision literals are **rounded at the database** without throwing; short SKUs may pass or fail depending on whether `[StringLength(20, MinimumLength = 3)]` is present on the entity — and annotation-only configuration on **CatalogProduct** means no Fluent safety net for money or indexes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `UnitPrice = 12.3456789m` with default scale 2 | Stored as `12.35` — silent financial drift |
| Validation gap | `Sku = "GDG"` (3 chars) — edge of `[StringLength]` min | May pass validation but business rules may require longer SKUs; empty-name case not shown but same class of bug |
| Configuration | CatalogProduct has no `HasPrecision` / explicit money facet | Bulk import magnifies rounding across thousands of rows |
| Observability | SaveChanges succeeds | No exception; reconciliation finds discrepancies later |

**Fix (priority order):**

1. Add explicit money mapping: `HasPrecision(19, 4)` (or team standard) via Fluent or `[Precision]` on `UnitPrice`; add migration.
2. Validate in domain/service before `AddRange`: scale, min SKU length, non-empty name — do not rely on DB rounding as validation.
3. Consider `[StringLength(20, MinimumLength = 3)]` + `[Required(AllowEmptyStrings = false)]` consistently (as in chapter **CatalogProduct**); fail fast before `SaveChanges`.
4. For bulk paths, use transactions and row-level error reporting; optionally `ExecuteUpdate`/bulk extensions with explicit column types.
5. Add tests that assert inserted `UnitPrice` equals source after read-back from SQL.

**Production takeaway:** EF SaveChanges success only means constraints satisfied — not that business precision or string rules held. Pair annotations/Fluent from this chapter with **pre-save domain validation** for money and identifiers. See **DemonstrateAnnotationValidation** in **Program.cs**.

---

---

### 08. LINQ to Entities & Query Patterns

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/08. LINQ to Entities & Query Patterns`

---

---

#### Q1. (R) A catalog API endpoint is slow under load. Review this repository method from the EF Core LINQ chapter's `StoreDbContext` model. What executes on SQL Server vs in the app, and what would you change?

```csharp
public IReadOnlyList<Product> GetExpensiveElectronics(StoreDbContext context)
{
    return context.Products
        .AsEnumerable()
        .Where(p => p.Category!.Name == "Electronics" && p.UnitPrice >= 50m)
        .OrderBy(p => p.Name)
        .ToList();
}
```

---

**Answer:**

**Answer:** `AsEnumerable()` switches the query from LINQ to Entities to LINQ to Objects, so SQL Server returns every product row (respecting global filters) and the app filters by category name and price in memory — correct results, catastrophic scale.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Query translation | `AsEnumerable()` before `Where` | Provider becomes in-memory; SQL is `SELECT * FROM Products` (plus global filter) |
| Performance | Category name filter runs client-side | Full table pulled over the network; CPU and memory grow with catalog size |
| Design | Navigation `Category!.Name` used after provider switch | JOIN could run in SQL if the query stayed `IQueryable` |

**Fix (priority order):**

1. Remove `AsEnumerable()` — keep the chain on `IQueryable<Product>` until the terminal operator.
2. Filter on translatable members: `.Where(p => p.CategoryId == 1 && p.UnitPrice >= 50m)` or `.Where(p => p.Category!.Name == "Electronics" && p.UnitPrice >= 50m)` so EF generates `JOIN` + `WHERE`.
3. Prefer `.Select` to a DTO when the API only needs a subset of columns (see `ProjectionQueries.GetProductCatalogItems()`).
4. Use `ToQueryString()` or SQL logging in dev to verify predicates appear in `WHERE`, not only after materialization.

**Production takeaway:** `AsEnumerable()` / `ToList()` early in the pipeline is a classic Karat trap — same LINQ syntax, opposite execution location. See LINQ ch.01 — `IQueryable` vs `IEnumerable`; this chapter's **Program.cs** quick reference — client evaluation pitfall.

---

---

#### Q2. (R) This search compiles but throws at runtime when hit through the API. Review the query and helper. What failed to translate, and how do you fix it without loading the whole table?

```csharp
public IReadOnlyList<Product> GetPremiumInStock(StoreDbContext context)
{
    return context.Products
        .Where(p => IsPremiumSku(p.Name) && p.StockQuantity > 0)
        .OrderBy(p => p.Name)
        .ToList();
}

private static bool IsPremiumSku(string name) =>
    name.StartsWith("Pro", StringComparison.OrdinalIgnoreCase);
```

---

**Answer:**

**Answer:** EF Core cannot translate the custom `IsPremiumSku` method (and its `StringComparison` overload) into SQL, so query translation fails at execution time unless you rewrite the predicate with translatable expressions or explicitly opt into client evaluation — which you should avoid for filters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Translation | Non-mapped instance/static method in `Where` | `InvalidOperationException` — "could not be translated" at `ToList()` |
| API surface | `StringComparison.OrdinalIgnoreCase` in helper | Not a known SQL pattern for EF's translator |
| Performance risk | Workaround via `AsEnumerable()` before filter | Would load all products — unacceptable for search |

**Fix (priority order):**

1. Inline translatable string logic: `.Where(p => p.Name.StartsWith("Pro") && p.StockQuantity > 0)` — EF Core translates `StartsWith` to SQL `LIKE`.
2. For case-insensitive search on SQL Server, use `.Where(p => EF.Functions.Like(p.Name, "Pro%"))` or configure a case-insensitive collation on the column — do not pull rows to CLR for casing.
3. If logic is truly domain-specific and non-translatable, filter translatable columns in SQL first (category, price band), then apply the helper only on the reduced set — or persist a computed/stored flag (`IsPremium`) set on write.
4. Never hide untranslatable calls inside repository helpers without documenting they force client evaluation.

**Production takeaway:** Compilable LINQ ≠ translatable LINQ — Karat tests whether you diagnose translation failures vs blaming "EF is slow." See **FilteringQueries** — composable `IQueryable` filters that stay on the provider.

---

---

#### Q3. (R) An order-history endpoint works in dev with 3 seed orders but degrades badly in production. Review this service method. What query pattern causes the regression, and how do you fix it?

```csharp
public List<string> BuildOrderLineSummaries(StoreDbContext context)
{
    List<Order> orders = context.Orders
        .OrderBy(o => o.OrderDate)
        .ToList();

    return orders.Select(o =>
        $"{o.CustomerName} ({o.OrderDate:d}): " +
        string.Join(", ", o.Lines.Select(l => l.Product!.Name)))
        .ToList();
}
```

---

**Answer:**

**Answer:** Materializing orders without eager-loading related data, then touching `Lines` and `Product` in LINQ to Objects, triggers lazy-loading N+1 queries — one round-trip for orders plus one (or more) per order line and product in production volume.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Data loading | `ToList()` on orders only | Parent rows loaded; navigations unset |
| N+1 | `o.Lines` and `l.Product!.Name` in memory loop | Each access can emit `SELECT` — 1 + N + M queries |
| Scalability | `string.Join` over lazy collections | Latency grows linearly with orders × lines; connection pool pressure |

**Fix (priority order):**

1. Eager-load in one translated query: `.Include(o => o.Lines).ThenInclude(l => l.Product)` before `ToList()` (detailed in ch.09 Loading Related Data).
2. Better for read-only summaries: project in SQL with `SelectMany` (see `ProjectionQueries.GetFlatOrderLineDescriptions()`) so the database returns flat rows — no entity graph, no lazy load.
3. Use `.AsNoTracking()` on read paths to avoid change-tracker overhead when entities are not updated.
4. Enable EF Core query logging or `TagWith` in staging and assert a single SQL batch (or known small count) per API call.

**Production takeaway:** Small seed data hides N+1 — Karat pairs "works locally" with production query multiplication. See **Order** model comment — lines navigation without Include is intentional in the filtering demo, not in list endpoints.

---

---

#### Q4. (R) A junior dev refactors the product search to "keep filtering composable" by returning `IQueryable<Product>` from the repository. Under ASP.NET Core scoped `DbContext`, intermittent `ObjectDisposedException` appears in production. Review both layers. What leaked, and what boundary would you enforce?

```csharp
public sealed class ProductRepository
{
    private readonly StoreDbContext _context;

    public ProductRepository(StoreDbContext context) => _context = context;

    public IQueryable<Product> BuildSearchQuery(int? categoryId, decimal? minPrice)
    {
        IQueryable<Product> query = _context.Products;

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.UnitPrice >= minPrice.Value);

        return query;
    }
}

public sealed class CatalogService
{
    private readonly ProductRepository _repo;

    public CatalogService(ProductRepository repo) => _repo = repo;

    public IReadOnlyList<Product> GetCatalog(int? categoryId, decimal? minPrice, string sort)
    {
        IQueryable<Product> query = _repo.BuildSearchQuery(categoryId, minPrice);

        query = sort == "price"
            ? query.OrderBy(p => p.UnitPrice)
            : query.OrderBy(p => p.Name);

        return query.ToList(); // sometimes throws after the HTTP request ends
    }
}
```

---

**Answer:**

**Answer:** The repository leaks `IQueryable<Product>` tied to a scoped `StoreDbContext`; when `ToList()` runs after the scope (or after the context is disposed), EF cannot execute the deferred query — composability across layers without a shared lifetime causes intermittent failures under timing pressure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `IQueryable` escapes repository with captured `_context` | Execution deferred past valid `DbContext` lifetime |
| Architecture | Service adds `OrderBy` then materializes | Looks composable, but couples callers to EF provider and context scope |
| Correctness | Intermittent `ObjectDisposedException` | Passes unit tests that dispose synchronously; fails in prod under async middleware |

**Fix (priority order):**

1. Materialize inside the scope that owns the context: repository (or service) calls `.ToListAsync()` / `.ToList()` before returning — return `IReadOnlyList<Product>` or DTOs, not `IQueryable`.
2. If dynamic filtering is required, pass filter parameters into one repository method that builds and executes the query internally (pattern in **FilteringQueries.SearchProducts**).
3. For advanced cases (reporting, admin grids), use a dedicated query object or `IQueryable` only within a single method where context lifetime is explicit — never return it from DI-scoped repositories to controllers.
4. Add an analyzer or team rule: public repository methods must not expose `IQueryable<T>`.

**Production takeaway:** `IQueryable` is deferred execution plus a live `DbContext` — leaking it is leaking scope. Karat tests repository boundaries, not just LINQ syntax. See **Program.cs** quick reference — disposed context pitfall.

---

---

#### Q5. (P) The chapter's `ProjectionQueries.GetProductCatalogItems()` projects to `ProductListItem` in the database. A teammate returns full `Product` entities from the repository and maps to DTOs in the controller with AutoMapper. Both compile. When would you insist on server-side `Select` projection, and what breaks if you skip it?

---

**Answer:**

**Answer:** Server-side `Select` keeps column subsetting and joins in SQL; returning tracked entities and mapping in memory ships every column, hydrates navigations you may not need, and invites lazy-load surprises — acceptable only for small internal tools or true aggregate roots you will update.

- **Insist on SQL projection** for list/catalog APIs, reports, and any read-heavy endpoint where the client needs fewer fields than the entity — bandwidth, memory, and GC improve measurably.
- **Global filters and soft-delete** still apply in `Select` — discontinued products stay hidden unless `IgnoreQueryFilters()` is intentional (see **StoreDbContext** `HasQueryFilter`).
- **AutoMapper after full entity load** often triggers N+1 if DTOs reference `Category.Name` and `Category` was not included — projection folds the join into one `SELECT`.
- **Change tracking:** projected DTOs are not tracked — safer for read-only APIs; full entities carry tracker cost unless you add `AsNoTracking()`.
- **When full entities are OK:** single-entity get-by-id with update intent, or bounded admin screens with explicit `Include` plan.

**Production takeaway:** Composability of LINQ stops at the repository boundary; composability of *data shape* belongs in translatable `Select` — matches **ProjectionQueries** Section 1 and the chapter's DTO `ProductListItem`.

---

---

#### Q6. (M) A background job captures an `IQueryable<Product>` during request handling and enumerates it minutes later on a thread-pool thread. The chapter's `IQueryableDemonstrations` stresses deferred execution. Walk through what happens from query construction to `foreach`, and what materialization rule you would enforce in code review.

```csharp
public IQueryable<Product> GetPendingPriceReview(StoreDbContext context, decimal threshold)
{
    return context.Products
        .Where(p => p.UnitPrice >= threshold)
        .OrderBy(p => p.Name);
}

// elsewhere, minutes later:
foreach (Product product in GetPendingPriceReview(context, 75m))
{
    await PublishReviewEventAsync(product);
}
```

---

---

### 09. Loading Related Data

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/09. Loading Related Data`
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Assigning to `IQueryable<Product>` only builds an expression tree — no SQL runs until enumeration; deferring `foreach` until after the request-scoped `DbContext` is disposed (or on another thread without that scope) throws or behaves unpredictably because the query is bound to a dead context.

- **Construction:** `GetPendingPriceReview` chains `Where`/`OrderBy` on `_context.Products` — provider captures the context; **Program.cs** / **IQueryableDemonstrations** — no DB round-trip yet.
- **Deferred window:** Returning or storing `IQueryable` exports that capture — any later `foreach`, `ToList`, or async iteration is the first execution point.
- **Failure mode:** Request ends → scoped `StoreDbContext` disposed → background `foreach` calls into EF → `ObjectDisposedException` (or worse, reuse of a pooled context in incorrect scope if misconfigured).
- **Secondary trap:** Even with a live context, long-lived `IQueryable` reused across callers can compose unintended filters or race on shared scoped state.
- **Code-review rule:** Materialize (`ToListAsync`, etc.) before leaving the owning scope; pass `IReadOnlyList<T>`, keys, or messages to background work — never `IQueryable<T>`.

**Production takeaway:** Deferred execution is a lifetime contract, not a performance trick — same lesson as **DemonstrateDeferredExecution** in **Program.cs**, extended to async and background jobs. See LINQ ch.01 — terminal operators trigger execution.

---

---

### 09. Loading Related Data

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/09. Loading Related Data`

---

---

#### Q1. (R) A list endpoint only needs order id, date, and customer name, but the repository loads the full graph before mapping. Review the method — what is the performance problem, and how do you fix it while keeping a single SQL round-trip?

```csharp
public async Task<IReadOnlyList<OrderListItemDto>> GetRecentOrdersAsync(CancellationToken ct)
{
    using ShopDbContext context = CreateContext();
    List<Order> orders = await context.Orders
        .AsNoTracking()
        .Include(o => o.Customer)
        .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
        .OrderByDescending(o => o.OrderDate)
        .Take(50)
        .ToListAsync(ct);

    return orders
        .Select(o => new OrderListItemDto(o.OrderId, o.OrderDate, o.Customer!.Name))
        .ToList();
}
```

---

**Answer:**

**Answer:** The query over-fetches — it JOINs and materializes every `OrderLine` and `Product` row for 50 orders even though the DTO only needs scalar order fields and the customer name, wasting SQL I/O, network bandwidth, and heap memory before the mapping discards the graph.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Data loading | `Include` + `ThenInclude` on `Lines` and `Product` for a summary list | Pulls entire child graph into memory |
| API design | Entity graph loaded then projected in memory | ORM materializes full entities; GC pressure under concurrent list traffic |
| SQL shape | Wide JOIN across Orders → OrderLines → Products | Larger result set and slower query plan than needed |

**Fix (priority order):**

1. Replace eager Include with a **server-side projection** — EF translates `Select` to SQL that returns only required columns in one command:

```csharp
return await context.Orders
    .AsNoTracking()
    .OrderByDescending(o => o.OrderDate)
    .Take(50)
    .Select(o => new OrderListItemDto(o.OrderId, o.OrderDate, o.Customer!.Name))
    .ToListAsync(ct);
```

2. If you must stay on entities temporarily, drop `.Include(o => o.Lines).ThenInclude(l => l.Product)` — keep only `.Include(o => o.Customer)` when customer name is needed and lines are not.
3. Add query logging (`LogTo` or `QueryDiagnostics` from this chapter) in staging to compare row counts before/after.

**Production takeaway:** Include is for when you **use** the navigation graph; list endpoints that only need a flat shape should project in `IQueryable` — the same lesson as **OrderLoadingService.GetOrdersWithEagerLoading** vs a summary DTO. See foundation **ch.08 IQueryable** — deferred execution lets `Select` shape SQL.

---

---

#### Q2. (R) After eager-loading changes, the order-detail page shows line quantities but every `ProductName` is null. Review the query — what is wrong with the Include chain, and what SQL shape do you expect after the fix?

```csharp
public Order? GetOrderDetail(int orderId)
{
    using ShopDbContext context = CreateContext();
    return context.Orders
        .AsNoTracking()
        .Include(o => o.Customer)
        .Include(o => o.Lines)
        .FirstOrDefault(o => o.OrderId == orderId);
}
```

---

**Answer:**

**Answer:** The query includes `Order.Lines` but never continues the chain with `ThenInclude(l => l.Product)`, so line entities materialize without their product navigation — `line.Product` stays null even though `Quantity` comes from the included line rows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Loading strategy | Missing `ThenInclude` after collection `Include` | Nested reference navigation not joined |
| Runtime | UI shows blank product names | Functional bug mistaken for mapping/serialization issue |
| ThenInclude depth | Chain stops at one level | Common Karat trap — `Include(o => o.Lines)` does not imply `Product` |

**Fix (priority order):**

1. Extend the chain to match **OrderLoadingService.GetOrdersWithEagerLoading**:

```csharp
return context.Orders
    .AsNoTracking()
    .Include(o => o.Customer)
    .Include(o => o.Lines)
        .ThenInclude(l => l.Product)
    .FirstOrDefault(o => o.OrderId == orderId);
```

2. Each new `Include` from the root starts a **new branch** — after `.Include(o => o.Customer)`, the next `.Include(o => o.Lines)` is another root branch, and `.ThenInclude(l => l.Product)` attaches to the **Lines** branch, not Customer.
3. Expect SQL with JOINs from `Orders` → `OrderLines` → `Products` (plus `Customers`), or two statements if split query is enabled.

**Production takeaway:** ThenInclude depth must match the object graph you dereference in the view — one missing level fails silently with null navigations. See **Program.cs** quick reference — eager loading chain example.

---

---

#### Q3. (M) A reporting job loads customers with all their orders and every order line. Under load, SQL row counts explode and memory spikes, but the team insists "it's one query so it must be efficient." Review the query — explain the cartesian product, and when you would use `AsSplitQuery()`.

```csharp
public IList<Customer> GetCustomersWithOrderHistory()
{
    using ShopDbContext context = CreateContext();
    return context.Customers
        .AsNoTracking()
        .Include(c => c.Orders)
            .ThenInclude(o => o.Lines)
                .ThenInclude(l => l.Product)
        .OrderBy(c => c.CustomerId)
        .ToList();
}
```

---

**Answer:**

**Answer:** A single SQL statement with JOINs across two collections (`Customer.Orders` and each order's `Lines`) produces a cartesian product — each customer row repeats for every combination of order × line in the result set, so EF must de-duplicate in memory and the database ships far more rows than entities returned.

- **Mechanism:** One `Include` on a collection plus `ThenInclude` into another collection multiplies rows in the flat JOIN result. Ten orders with five lines each can yield ~50 joined rows **per customer** before deduplication.
- **Symptoms:** Slow reports, high SQL `logical reads`, memory spikes, "one query" that transfers megabytes — worsens as order history grows.
- **`AsSplitQuery()`:** EF issues multiple SQL commands (e.g., customers, then orders, then lines) that avoid the wide JOIN cartesian product while still building the same graph — usually 2–4 round-trips vs one giant result.
- **When to use:** Multiple collection navigations or deep Include trees on reporting/read-only paths where a single JOIN would explode — as noted in **OrderLoadingService.cs** Section 4 preview.
- **Trade-off:** Split queries add round-trips; on latency-sensitive small graphs a single JOIN may be fine — profile with command logging.

**Production takeaway:** "One query" ≠ "one efficient query" — cartesian explosion is a classic EF production incident; split queries or projection are the fix. See **QueryDiagnostics** in this chapter to compare command count vs row volume.

---

---

#### Q4. (R) A developer refactors N+1 line loading to "explicit loading" but production still shows hundreds of SQL commands per request. Review the method — identify every issue and prioritize fixes.

```csharp
public IList<OrderHeaderDto> GetOrderHeaders(IReadOnlyList<int> orderIds)
{
    using ShopDbContext context = CreateContext();
    List<Order> orders = context.Orders
        .AsNoTracking()
        .Where(o => orderIds.Contains(o.OrderId))
        .ToList();

    foreach (Order order in orders)
    {
        context.Entry(order).Reference(o => o.Customer).Load();
        context.Entry(order).Collection(o => o.Lines).Load();
    }

    return orders.Select(o => new OrderHeaderDto(
        o.OrderId,
        o.Customer!.Name,
        o.Lines.Count)).ToList();
}
```

---

**Answer:**

**Answer:** Explicit loading never runs correctly here — `AsNoTracking()` detaches entities so `Entry(order).Load()` cannot attach and load navigations, and even if tracked, calling `Load()` twice per order in a loop reproduces an N+1 (1 + 2×N commands) instead of a batched Include or projection.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Tracking | `AsNoTracking()` then `Entry(order).Load()` | Load is a no-op or throws depending on version/state; navigations stay null |
| Performance | `Load()` inside `foreach` over N orders | 1 + 2N SQL commands (customer + lines each iteration) |
| Misapplied pattern | Explicit load used for bulk read | Explicit loading suits **conditional** navigations on a **tracked** root, not batch header lists |

**Fix (priority order):**

1. For bulk headers, use one query with Include or projection — not per-row Load:

```csharp
return await context.Orders
    .AsNoTracking()
    .Where(o => orderIds.Contains(o.OrderId))
    .Select(o => new OrderHeaderDto(o.OrderId, o.Customer!.Name, o.Lines.Count))
    .ToListAsync();
```

2. If explicit loading is required (conditional navigations after business logic), remove `AsNoTracking()` on the root query so the entity is **tracked**, and batch where possible — still prefer Include when you always need Customer + Lines.
3. Follow **OrderLoadingService.GetOrderWithExplicitLoading** — single tracked root, `Reference().Load()` and `Collection().Query().Include(...).Load()` when the context is open.

**Production takeaway:** Explicit loading requires tracking and still costs per Load call — swapping N+1 manual queries for N+1 `Load()` in a loop does not fix scalability. See **Program.cs** Section 7 and N+1 demo in Section 5.

---

---

#### Q5. (R) A fulfillment API should return only orders that have at least one line with `Quantity >= 2`, and each order should expose only those qualifying lines. The developer uses filtered Include but QA reports wrong orders in the response. Review the code — what is misunderstood about filtered Include, and how do you fix the business rule?

```csharp
public IList<Order> GetBulkShipCandidates(int minQuantity)
{
    using ShopDbContext context = CreateContext();
    return context.Orders
        .AsNoTracking()
        .Include(o => o.Lines.Where(l => l.Quantity >= minQuantity))
        .OrderBy(o => o.OrderId)
        .ToList();
}
```

---

**Answer:**

**Answer:** Filtered `Include` only filters **which child rows populate the collection** — it does **not** filter the root `Order` entities, so orders with no qualifying lines still appear with an empty `Lines` collection unless you add a root `Where`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | Treating filtered Include as root filter | Orders with zero bulk lines still returned |
| Business logic | Callers check `order.Lines.Count` expecting exclusion | Wrong orders enter fulfillment queue |
| API contract | Partial line graph without root predicate | Matches **GetOrdersWithFilteredLines** demo shape, not "orders that qualify" |

**Fix (priority order):**

1. Filter the root entity when the business rule applies to **which orders** are returned:

```csharp
return context.Orders
    .AsNoTracking()
    .Where(o => o.Lines.Any(l => l.Quantity >= minQuantity))
    .Include(o => o.Lines.Where(l => l.Quantity >= minQuantity))
    .OrderBy(o => o.OrderId)
    .ToList();
```

2. Keep filtered Include to avoid loading non-qualifying lines into the graph — both predicates should align on the same rule.
3. Document that filtered Include (EF Core 5+) translates to SQL on the collection subquery, not client-side filtering after a full load — see **OrderLoadingService** Section 8.

**Production takeaway:** Filtered Include answers "which related rows to attach," not "which parents match" — Karat tests whether you add `Where` on the root separately. See **Program.cs** quick reference — filtered Include example.

---

---

#### Q6. (D) An order API has three read paths: (A) list view — id/date/customer name only; (B) detail view — customer + lines + product names; (C) background job — load customer only when an order fails validation. For each path, choose **projection**, **Include/ThenInclude**, or **explicit Load** — and name one production failure mode if you pick the wrong strategy.

---

---

### 10. Raw SQL & Stored Procedures

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/10. Raw SQL & Stored Procedures`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Match loading strategy to how much of the graph you dereference — project flat list data, Include the full detail graph in one or split queries, and explicit Load only for conditional navigations on a tracked entity.

| Path | Strategy | Why | Wrong choice → production failure |
|---|---|---|---|
| **(A) List view** | **Projection** (`Select` to DTO) | Only three scalars; no need for entity graph or change tracking | Include + ThenInclude → over-fetch (Q1): memory and SQL bloat on every list request |
| **(B) Detail view** | **Include / ThenInclude** (optionally `AsSplitQuery` if cartesian risk) | Always need Customer, Lines, Product — same as **GetOrdersWithEagerLoading** | Projection-only without planning → multiple round-trips or N+1 if UI walks navigations later |
| **(C) Validation job** | **Explicit Load** on tracked root after initial query | Customer needed only for failed orders — avoid loading customer for every order up front | Include Customer on bulk query → wasted JOINs when 95% pass validation; Load in loop without tracking → null Customer (Q4) |

- **(A)** Keep `AsNoTracking`; never Include lines on list endpoints.
- **(B)** Chain `.Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product)`; add `AsSplitQuery()` if metrics show cartesian row explosion (Q3).
- **(C)** Query orders tracked (no `AsNoTracking`), run validation, then `Entry(order).Reference(o => o.Customer).Load()` only in the failure branch — context must stay open until Load completes.

**Production takeaway:** The three EF loading modes from this chapter — eager, explicit, projection — are not interchangeable; Karat expects you to tie each HTTP/job shape to a strategy and name the failure mode (over-fetch, N+1, cartesian, null navigations) when you mismatch.

---

---

### 10. Raw SQL & Stored Procedures

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/10. Raw SQL & Stored Procedures`

---

---

#### Q1. (R) A junior developer adds a product search endpoint by adapting `GetProductsByNamePrefix` from **RawSqlQueryRepository.cs**. Review the repository method:

```csharp
public List<Product> SearchByName(string userInput)
{
    var sql = $"""
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName LIKE '%{userInput}%'
        ORDER BY ProductId
        """;

    return _context.Products
        .FromSqlRaw(sql)
        .AsNoTracking()
        .ToList();
}
```

They argue it is safe because they used `FromSqlRaw`, not ADO.NET string building. What is wrong, and how do you fix it?

---

**Answer:**

```csharp
public List<Product> SearchByName(string userInput)
{
    var sql = $"""
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName LIKE '%{userInput}%'
        ORDER BY ProductId
        """;

    return _context.Products
        .FromSqlRaw(sql)
        .AsNoTracking()
        .ToList();
}
```

They argue it is safe because they used `FromSqlRaw`, not ADO.NET string building. What is wrong, and how do you fix it?

**Answer:** `FromSqlRaw` only parameterizes values you pass as **separate arguments** with `{0}` placeholders — embedding `userInput` in the SQL string via C# interpolation inlines untrusted text into the command, which is classic SQL injection. The API name does not make concatenated strings safe.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `userInput` interpolated into SQL before `FromSqlRaw` | SQL injection — attacker can alter WHERE, UNION, or stack batches |
| Misconception | Assumes `FromSqlRaw` always parameterizes | `{0}` args are safe; pre-built strings with embedded values are not |
| API surface | Search exposed to HTTP query string | High-risk endpoint — injection reachable from production traffic |

**Fix (priority order):**

1. Replace with `FromSqlInterpolated` and pass the pattern as an interpolated value: `WHERE ProductName LIKE {$"%{userInput}%"}` — EF sends `userInput` as a parameter, not literal SQL text.
2. Or keep `FromSqlRaw` with `{0}`: `" ... WHERE ProductName LIKE {0}"` and pass `"%" + userInput + "%"` as the argument.
3. Validate/sanitize input length and reject obvious control characters at the API layer — defense in depth, not a substitute for parameters.
4. Never build SQL with `$"..."` or `+` when any segment comes from users — same rule as ADO.NET ch.03 and Dapper ch.01.

**Production takeaway:** Karat pairs EF Core APIs with injection traps — `FromSqlRaw` is safe **only** when user data never touches the SQL string literal. See **Program.cs** QUICK REFERENCE — "never concatenate user input."

---

---

#### Q2. (M) A teammate extends **StoredProcedureRepository.GetByMinStock** to filter expensive items in LINQ after the proc returns:

```csharp
public List<Product> GetExpensiveInStock(int minStock, decimal minPrice)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetProductsByMinStock @MinStock={0}", minStock)
        .Where(p => p.UnitPrice >= minPrice)
        .OrderBy(p => p.UnitPrice)
        .AsNoTracking()
        .ToList();
}
```

Tests pass on LocalDB with small data. What happens at the SQL layer, and what would you change for production?

---

**Answer:**

```csharp
public List<Product> GetExpensiveInStock(int minStock, decimal minPrice)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetProductsByMinStock @MinStock={0}", minStock)
        .Where(p => p.UnitPrice >= minPrice)
        .OrderBy(p => p.UnitPrice)
        .AsNoTracking()
        .ToList();
}
```

Tests pass on LocalDB with small data. What happens at the SQL layer, and what would you change for production?

**Answer:** `EXEC` stored-procedure SQL is **not composable** — EF Core cannot append a translated `WHERE UnitPrice >= @minPrice` to the procedure call. The provider typically executes the proc, materializes the full result set, then applies `.Where` and `.OrderBy` **in memory**, which silently becomes a client-side filter.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Client-side `.Where` after `EXEC` | Full proc result set shipped to the app — scales poorly on large catalogs |
| Correctness (hidden) | Appears like server-side LINQ | Reviewers assume SQL push-down; prod latency and memory spike under load |
| Design | Price filter belongs in proc or a composable SELECT | Duplicates business rules across C# and T-SQL |

**Fix (priority order):**

1. **Preferred:** Add `@MinPrice` to `usp_GetProductsByMinStock` (or a new proc) so filtering happens in SQL — matches how **SqlQueryRepository.GetLowStockReport** uses `EXEC` then **only** client sorts via `.AsEnumerable().OrderBy(...)`.
2. **Alternative:** Replace proc with a parameterized `FromSqlRaw` SELECT (composable) if you need LINQ composition: `SELECT ... FROM dbo.Products WHERE StockQuantity >= {0} AND UnitPrice >= {1}` then `.OrderBy` translates to SQL.
3. If client-side filtering is intentional, document it and call `.AsEnumerable()` before LINQ so intent is explicit — see **SqlQueryRepository.cs** Section 9 comment on non-composable SQL.
4. Load-test with realistic row counts — small LocalDB demos hide the trap.

**Production takeaway:** Composable raw SQL applies to plain `SELECT` fragments, not `EXEC`. Stacked LINQ after procs is a common Karat mechanism question — behavior is correct but often not what you want at scale. See foundation ADO.NET ch.07 — proc vs ad-hoc SQL trade-offs.

---

---

#### Q3. (R) A legacy reporting stored procedure returns a narrow shape — not full `Product` rows. A developer maps it to `DbSet<Product>` anyway:

```csharp
public List<Product> GetLowStockViaProc(int threshold)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetLowStockReport @Threshold={0}", threshold)
        .AsNoTracking()
        .ToList();
}
```

`usp_GetLowStockReport` returns `ProductId`, `ProductName`, `StockQuantity`, and a computed `Status` column (see **LowStockRow.cs**). Review this mapping choice. What breaks at runtime or in maintenance, and what EF Core API fits this shape?

---

**Answer:**

```csharp
public List<Product> GetLowStockViaProc(int threshold)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetLowStockReport @Threshold={0}", threshold)
        .AsNoTracking()
        .ToList();
}
```

`usp_GetLowStockReport` returns `ProductId`, `ProductName`, `StockQuantity`, and a computed `Status` column (see **LowStockRow.cs**). Review this mapping choice. What breaks at runtime or in maintenance, and what EF Core API fits this shape?

**Answer:** `DbSet<Product>.FromSqlRaw` expects a result shape that maps to **Product** columns — extra columns like `Status` are ignored, but missing required mapped columns (`UnitPrice`, `DiscontinuedDate`) leave properties at default values, producing silently wrong `Product` instances rather than a clear error.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Partial row mapped to full entity | `UnitPrice` = 0, `DiscontinuedDate` = null — consumers treat garbage as real inventory |
| Design | Report DTO forced into entity type | `Status` discarded; domain model polluted with read-model concerns |
| Maintainability | Proc column changes break entity assumptions | No compile-time check — wrong data in dashboards |

**Fix (priority order):**

1. Map to **`LowStockRow`** via `Database.SqlQueryRaw<LowStockRow>("EXEC dbo.usp_GetLowStockReport @Threshold={0}", threshold)` — as in **SqlQueryRepository.GetLowStockReport**.
2. Or register `LowStockRow` as a **keyless entity** (`HasNoKey()`) with `DbSet<LowStockRow>` if the shape participates in repeated LINQ graphs.
3. Reserve `DbSet<Product>.FromSqlRaw("EXEC ...")` for procs that SELECT the **same columns** as `dbo.Products` — like **StoredProcedureRepository.GetByMinStock**.
4. Add integration tests asserting column counts and non-default values for critical fields when mapping procs to entities.

**Production takeaway:** Stored-procedure mapping is not "any rows → any entity" — column contract must match. Karat tests whether you reach for `SqlQuery<T>` / keyless types vs misusing tracked entities. See **Program.cs** Section 8 vs Section 9 demos.

---

---

#### Q4. (R) Two developers argue about parameter safety. Compare these snippets from a refactored repository:

```csharp
// Developer A
public List<Product> ByPrefix(string prefix)
{
    return _context.Products
        .FromSqlRaw($"""
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductName LIKE '{prefix}%'
            """)
        .ToList();
}

// Developer B
public List<Product> ByPrefix(string prefix)
{
    return _context.Products
        .FromSqlInterpolated($"""
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductName LIKE {prefix + "%"}
            """)
        .ToList();
}
```

Developer A says both use `$"""` so both are parameterized. Who is correct, and why does `FromSqlRaw` vs `FromSqlInterpolated` matter here?

---

**Answer:**

```csharp
// Developer A
public List<Product> ByPrefix(string prefix)
{
    return _context.Products
        .FromSqlRaw($"""
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductName LIKE '{prefix}%'
            """)
        .ToList();
}

// Developer B
public List<Product> ByPrefix(string prefix)
{
    return _context.Products
        .FromSqlInterpolated($"""
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductName LIKE {prefix + "%"}
            """)
        .ToList();
}
```

Developer A says both use `$"""` so both are parameterized. Who is correct, and why does `FromSqlRaw` vs `FromSqlInterpolated` matter here?

**Answer:** Developer B is correct. Developer A passes a **fully formed string** to `FromSqlRaw` — C# interpolation runs first, inlining `prefix` as literal SQL. Developer B uses `FromSqlInterpolated`, which accepts a `FormattableString` and sends each interpolated expression as a **separate SqlParameter**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security (A) | `FromSqlRaw` + `$"..."` with embedded `{prefix}` | Equivalent to string concat — injection if `prefix` is user-controlled |
| Misconception (A) | `$"""` syntax confused with EF parameterization | `$` on `FromSqlRaw` does not invoke EF's param binder |
| Correct pattern (B) | `FromSqlInterpolated` + `{prefix + "%"}` | Values become `@p0` — matches **RawSqlQueryRepository.GetProductsByNamePrefix** |

**Fix (priority order):**

1. Delete Developer A's pattern; never pass interpolated strings to `FromSqlRaw`.
2. Safe `FromSqlRaw` form: `" ... WHERE ProductName LIKE {0}"` with `prefix + "%"` as the **second argument** — positional placeholders only.
3. Prefer `FromSqlInterpolated` when inline values read clearer — same safety as `{0}` args.
4. Code-review rule: if `FromSqlRaw` call has `$` before the string and no separate parameter arguments, flag it.

**Production takeaway:** The trap is syntactic — `$` string + `FromSqlRaw` looks modern but bypasses parameters. Karat stacks ADO.NET parameter lessons onto EF API names. See **RawSqlQueryRepository.cs** Section 6 parameter table.

---

---

#### Q5. (D) Your team needs inventory aggregate reports (**InventorySummary.cs**) in three services. One developer registers a keyless entity; another uses EF Core 8 `Database.SqlQuery<T>` only at call sites (as in **SqlQueryRepository.cs**). Compare:

```csharp
// Option A — OnModelCreating
modelBuilder.Entity<InventorySummary>().HasNoKey().ToView(null);
public DbSet<InventorySummary> InventorySummaries => Set<InventorySummary>();

// Option B — no DbSet registration (current chapter approach)
_context.Database.SqlQueryRaw<InventorySummary>("SELECT COUNT(*) AS ProductCount, ...").Single();
```

When would you pick keyless `DbSet<T>` vs `SqlQuery<T>`, and what pitfalls apply to each in a production API?

---

**Answer:**

```csharp
// Option A — OnModelCreating
modelBuilder.Entity<InventorySummary>().HasNoKey().ToView(null);
public DbSet<InventorySummary> InventorySummaries => Set<InventorySummary>();

// Option B — no DbSet registration (current chapter approach)
_context.Database.SqlQueryRaw<InventorySummary>("SELECT COUNT(*) AS ProductCount, ...").Single();
```

When would you pick keyless `DbSet<T>` vs `SqlQuery<T>`, and what pitfalls apply to each in a production API?

**Answer:** Both map read-only shapes without change tracking. Choose keyless `DbSet<T>` when the type is reused across queries, joins with entities, or global filters; choose `SqlQuery<T>` for one-off reports and EF Core 8+ ad-hoc DTO materialization without expanding the model.

- **Keyless `DbSet<T>` (`HasNoKey()`):** Good when `InventorySummary` appears in multiple repositories, needs `FromSqlRaw` on the set, or composes with entity LINQ. Pitfalls: must not call `Add`/`SaveChanges` on it; configure `ToView(null)` or explicit view name; team must know it is not a table — migrations won't create it.
- **`Database.SqlQuery<T>` / `SqlQueryRaw<T>`:** Good for isolated aggregates (**GetInventorySummary** in **SqlQueryRepository.cs**) — no model pollution, lighter for microservices that only need one report. Pitfalls: not on `DbSet` — no `Include`; composability same as raw SELECT; requires EF Core 8+.
- **Shared pitfall:** Property names must match column aliases (`ProductCount`, `TotalUnits`, `AveragePrice`); no parameterless constructor means materialization fails at runtime.
- **Production API:** Register keyless types in a shared `DbContext` when reports are first-class; use `SqlQuery` in vertical slices or read models to avoid bloating `OnModelCreating` for a single endpoint.
- **Avoid:** Treating either as an insert/update target — both are read-only; use entities or `ExecuteSqlRaw` for writes.

**Production takeaway:** Karat tests design judgment on read models — keyless entity is the EF-native reusable graph node; `SqlQuery<T>` is the EF 8 lightweight escape hatch. See **Models/InventorySummary.cs** comparison table and **EfCoreTutorialDbContext.cs** Section 4 note.

---

---

#### Q6. (P) **RawSqlQueryRepository.GetActiveProductsAbovePrice** appends LINQ after `FromSqlRaw`:

```csharp
return _context.Products
    .FromSqlRaw(
        """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL
          AND UnitPrice > {0}
        """,
        minimumPrice)
    .OrderBy(p => p.ProductName)
    .AsNoTracking()
    .ToList();
```

Explain how EF Core composes this query, when post-`FromSqlRaw` LINQ is safe vs when it pulls rows client-side, and one production scenario where you would **not** compose further LINQ on raw SQL.

---

---

### 11. Change Tracking, Async & Transactions

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/11. Change Tracking, Async & Transactions`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
return _context.Products
    .FromSqlRaw(
        """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL
          AND UnitPrice > {0}
        """,
        minimumPrice)
    .OrderBy(p => p.ProductName)
    .AsNoTracking()
    .ToList();
```

Explain how EF Core composes this query, when post-`FromSqlRaw` LINQ is safe vs when it pulls rows client-side, and one production scenario where you would **not** compose further LINQ on raw SQL.

**Answer:** For a composable `SELECT`, EF Core wraps the raw SQL as a subquery and appends translated LINQ (`ORDER BY ProductName`) in the same SQL command sent to SQL Server — so `.OrderBy` here is server-side, not an in-memory sort.

- **Composable (server-side):** Plain `SELECT` statements without `ORDER BY` in the raw fragment — EF can add `WHERE`, `ORDER BY`, `SKIP/TAKE` when the provider supports composition. This matches **RawSqlQueryRepository.cs** Section 6 — "You can append LINQ after FromSqlRaw."
- **Non-composable (client-side):** `EXEC` stored procedures, SQL with `ORDER BY` already in the raw string (provider-dependent), or vendor-specific batches — further LINQ may force client evaluation; use `.AsEnumerable()` explicitly when you accept that cost (**SqlQueryRepository.GetLowStockReport**).
- **Tracking note:** Without `.AsNoTracking()`, composed queries still return tracked entities if the raw SQL includes key columns (`ProductId`) — fine for updates, expensive for read-only APIs.
- **Scenario to avoid composition:** Legacy proc that already applies complex filtering — pushing extra `.Where` in LINQ hides that all rows cross the wire; fix the proc or use a composable SELECT instead.
- **Debugging:** Log `ToQueryString()` (EF Core 5+) or enable SQL logging to verify whether `ORDER BY` appears in the final batch — do not assume from LINQ syntax alone.

**Production takeaway:** Composable raw SQL is a strength of EF Core over hand-rolled ADO.NET for paginated/filtered reports — but only on plain SELECTs. Pair with Q2's `EXEC` trap: composition rules are the dividing line. See **Program.cs** QUICK REFERENCE — "When to stay on LINQ vs raw SQL."

---

---

### 11. Change Tracking, Async & Transactions

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/11. Change Tracking, Async & Transactions`

---

---

#### Q1. (R) A catalog endpoint loads products with `AsNoTracking()` for performance, then applies a "flash sale" discount in memory before calling `SaveChangesAsync`. QA passes on one row; production reports prices never change. Review this service method — what is wrong, and how do you fix it without abandoning no-tracking for the read path?

```csharp
public async Task ApplyFlashSaleAsync(int categoryId, decimal discountPct, CancellationToken ct)
{
    var products = await _context.Products
        .AsNoTracking()
        .Where(p => p.CategoryId == categoryId)
        .ToListAsync(ct);

    foreach (var p in products)
    {
        p.UnitPrice *= (1 - discountPct);
    }

    await _context.SaveChangesAsync(ct);
}
```

---

**Answer:**

**Answer:** `AsNoTracking()` entities are **not registered** in the change tracker, so mutating `UnitPrice` in memory and calling `SaveChangesAsync` produces **zero UPDATE statements** — the same phantom-edit failure demonstrated in **ChangeTrackingService.DemonstrateAsNoTrackingAsync**. The read optimization is correct; the write path must use a separate, explicit update mechanism.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracking | No-tracking query + in-memory edit | `SaveChangesAsync` returns 0; prices unchanged |
| API contract | Method completes without error | Silent failure — hardest EF bug in production |
| Design | Mixing read model and write model on same detached instances | Works only if developer mistakenly uses tracked queries |

**Fix (priority order):**

1. **Bulk server-side update (preferred at scale):** keep `AsNoTracking()` for reads; use `ExecuteUpdateAsync` for the discount — `SetProperty(p => p.UnitPrice, p => p.UnitPrice * (1 - discountPct))` filtered by `CategoryId` (Q6).
2. **Per-row tracked or attach:** for smaller sets, loop keys and either `Find` + modify tracked entity, or `Attach` + mark `UnitPrice` `IsModified` — matches **ProductUpdateService** Section 7 patterns.
3. Do not call `Update()` on full no-tracking entities if the query omitted columns — partial stubs with attach + `IsModified` avoid overwriting untouched columns.
4. Log or assert `SaveChangesAsync` row count; return failure when 0 rows expected.

**Production takeaway:** **AsNoTracking is for read models** — display, search grids, and DTO projection. Any persist path must re-enter the tracker (`Find`, `Attach`, `Update`) or bypass it with `ExecuteUpdate`. See **ChangeTrackingService.cs** Section 6 and **Program.cs** phantom-edit demo.

---

---

#### Q2. (P) Operations wants to purge inactive products older than two years — potentially tens of thousands of rows. A developer proposes loading each entity with `Find`, calling `Remove`, then one `SaveChangesAsync`. You suggest `ExecuteDeleteAsync` instead. When is `ExecuteDelete` the right tool, what does it skip compared to tracked delete, and what guardrails would you add before running it in production?

---

**Answer:**

**Answer:** `ExecuteDeleteAsync` (EF Core 7+) translates the LINQ filter directly into a **single set-based DELETE** on the server — no entity materialization, no change-tracker snapshots, and no per-row `Remove` calls. Use it for bulk purges where you do not need per-row domain logic, interceptors on individual entities, or cascade behavior that requires loaded graphs.

- **When to use:** large filtered deletes (archival purge, soft-delete migration cleanup) where the predicate is expressible in SQL and side effects are acceptable at the SQL layer.
- **What it skips:** change tracker entirely — `SaveChangesInterceptor`, `Deleting`/`Deleted` entity events, and client-evaluated logic in the loop do not run per row; only database FK/cascade rules apply.
- **Tracked delete still wins when:** you must audit each row, enforce business rules per entity, trigger domain events, or delete related graphs that EF models with tracked cascades you need to inspect.
- **Guardrails:** run against a replica or `BEGIN TRAN` + `SELECT COUNT(*)` preview first; require a **soft-delete** flag in many domains instead of hard delete; add a `WHERE` cutoff with indexed columns; log rows affected from the return value; restrict to admin/batch role; consider batched deletes (`TOP` chunks) to avoid long locks.

**Production takeaway:** **ExecuteDelete is the EF equivalent of `DELETE FROM … WHERE`** — pair no-tracking reads with set-based writes when scale matters. Do not load 50k rows into memory to call `Remove`. Forward reference: ch05 CRUD for tracked `Remove` semantics.

---

---

#### Q3. (R) A stock transfer must update `InventoryDbContext` and write an audit row through `AuditDbContext` (separate DbContext types, same SQL Server database). A teammate starts a transaction on each context independently. Review the orchestration — what breaks atomicity, and how do you coordinate a single commit across both contexts?

```csharp
public async Task TransferWithAuditAsync(StockTransfer transfer, CancellationToken ct)
{
    await using var invTx = await _inventory.Database.BeginTransactionAsync(ct);
    await using var auditTx = await _audit.Database.BeginTransactionAsync(ct);

    await _inventoryTransferService.TransferStockAsync(transfer, ct);
    _audit.AuditEntries.Add(new AuditEntry { Action = "Transfer", Payload = transfer.ToJson() });
    await _audit.SaveChangesAsync(ct);

    await invTx.CommitAsync(ct);
    await auditTx.CommitAsync(ct);
}
```

---

**Answer:**

**Answer:** Two independent `BeginTransactionAsync` calls create **two separate database transactions** on potentially different connections — committing `invTx` can succeed while `auditTx` fails (or vice versa), leaving inventory moved without an audit trail or an audit row without the stock change. This is not one atomic unit of work.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Transactions | One transaction per DbContext | Partial commit — data and audit diverge |
| Connection | Contexts may pool different connections | Nested commits do not enlist together automatically |
| Error handling | Order of commits matters | First commit irreversible if second fails |

**Fix (priority order):**

1. **`TransactionScope` + `RequiresNew`/`Required`:** wrap both saves in `using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);` — both contexts enlist in one ambient transaction when using the same database (MSDTC for cross-database/server).
2. **Share one connection:** open `DbConnection`, pass to both contexts via `UseSqlServer(connection)` / `context.Database.UseConnection(connection)`, call `BeginTransactionAsync` **once** on that connection, pass `IDbContextTransaction` or enlist both contexts — advanced but avoids MSDTC on same DB.
3. **Single DbContext** when inventory and audit share one database and bounded context — one `SaveChangesAsync` inside one `BeginTransactionAsync` (matches **InventoryTransactionService.TransferStockAsync** pattern in this chapter).
4. If audit is eventually consistent by design, document **outbox pattern** instead of pretending one EF transaction spans services.

**Production takeaway:** **`BeginTransactionAsync` is per-context, not per-business-operation** — multi-context atomicity needs ambient `TransactionScope` or a shared connection. See **InventoryTransactionService.cs** Section 8 for correct single-context transaction pattern.

---

---

#### Q4. (R) Under load, the inventory API thread pool queues grow and requests time out. Review this controller action — identify async anti-patterns and what you would change for "async all the way" through EF Core.

```csharp
[HttpGet("catalog")]
public IActionResult GetCatalog()
{
    var lines = _trackingService.GetProductCatalogAsync(CancellationToken.None).Result;
    return Ok(lines);
}

[HttpPost("transfer")]
public async Task<IActionResult> Transfer([FromBody] StockTransfer dto)
{
    string outcome = _transactionService.TransferStockAsync(dto).GetAwaiter().GetResult();
    return Ok(outcome);
}
```

---

**Answer:**

**Answer:** Both actions **block async I/O** — `.Result` and `GetAwaiter().GetResult()` on `Task`-returning EF methods sync-over-async, which can exhaust the ASP.NET thread pool under concurrency and cause deadlocks in contexts with a synchronization context. EF Core APIs like `ToListAsync` and `SaveChangesAsync` exist precisely to free threads during database waits.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `GetProductCatalogAsync` | Thread blocked during SQL I/O; pool starvation |
| Async | `.GetAwaiter().GetResult()` on transfer | Same — defeats async pipeline end-to-end |
| API signature | `IActionResult` instead of `Task<IActionResult>` | Compiler cannot enforce async controller |
| Cancellation | `CancellationToken.None` | Client disconnect does not cancel query |

**Fix (priority order):**

1. Make actions `async Task<IActionResult>` and **`await`** service calls — `var lines = await _trackingService.GetProductCatalogAsync(ct);`
2. Pass **`CancellationToken`** from `HttpContext.RequestAborted` (or method parameter) through to `ToListAsync` / `SaveChangesAsync`.
3. Ensure services use **`SaveChangesAsync` / `BeginTransactionAsync`** — never `SaveChanges()` in ASP.NET request paths.
4. Use **`ConfigureAwait(false)`** in library/service layers (as in this chapter's services); controllers need not call it.
5. Load-test after fix — thread-pool queue length should stay flat under parallel catalog reads.

```csharp
[HttpGet("catalog")]
public async Task<IActionResult> GetCatalog(CancellationToken ct)
{
    var lines = await _trackingService.GetProductCatalogAsync(ct);
    return Ok(lines);
}
```

**Production takeaway:** **Async all the way** means no blocking on `Task` from database code in request threads. See **Program.cs** async `Main` and service methods using `SaveChangesAsync` + `ConfigureAwait(false)`. See C# Module 06 — sync-over-async gotcha.

---

---

#### Q5. (R) `Product` now has a SQL Server `rowversion` concurrency token. Two editors save conflicting prices; the second save throws `DbUpdateConcurrencyException`. Review this catch block from the API layer — what is wrong with the recovery strategy, and what should happen before returning a response to the client?

```csharp
catch (DbUpdateConcurrencyException)
{
    await _context.SaveChangesAsync(ct); // retry same pending changes
    return Ok(updatedDto);
}
```

---

**Answer:**

**Answer:** Blindly calling `SaveChangesAsync` again **retries the same stale values** against a row whose token already changed — the second save will throw again (or worse, loop). The handler never reloads current database state, never merges the user's intent with fresh data, and returns **200 OK** as if the conflict was resolved.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Retry without reload | Repeated `DbUpdateConcurrencyException` or infinite retry |
| Correctness | Stale `updatedDto` returned | Client believes save succeeded with wrong version |
| HTTP semantics | `Ok` on conflict | Should be **409 Conflict** with current row or ProblemDetails |
| Token handling | Original `RowVersion` still in entry | EF WHERE clause keeps failing |

**Fix (priority order):**

1. Catch `DbUpdateConcurrencyException` and inspect **`ex.Entries`** — typically one `Product` entry in conflict.
2. **Reload** database values: `await entry.ReloadAsync(ct)` or query fresh row; compare `OriginalValues` vs `CurrentValues` (database) vs client intent.
3. **Merge policy:** client-wins (reapply property on fresh token), server-wins (return 409 + current DTO), or prompt user — never silent overwrite without decision.
4. After merge, set new token from reloaded entity and call **`SaveChangesAsync` once** — or abort and return 409 with `{ currentPrice, yourPrice, rowVersion }`.
5. Expose token to clients as **ETag** / DTO field so updates send If-Match semantics (extends ch05 Q5).

```csharp
catch (DbUpdateConcurrencyException ex)
{
    var entry = ex.Entries.Single();
    await entry.ReloadAsync(ct);
    return Conflict(new { message = "Product was modified by another user.", current = entry.CurrentValues.ToObject() });
}
```

**Production takeaway:** **`DbUpdateConcurrencyException` is a business event, not a transient I/O error** — reload, decide, then save or reject. Optimistic concurrency without handling is only half the feature. See ch05 CRUD Q5 for token setup; this chapter covers runtime handling.

---

---

#### Q6. (P) A nightly job bulk-updates `StockQuantity` for every warehouse row matching a filter. Compare these two approaches — tracked load + modify + `SaveChangesAsync` vs `ExecuteUpdateAsync` — and state which you would ship for 50k rows, including concurrency and observability trade-offs.

```csharp
// Approach A
var rows = await _context.Products.Where(p => p.WarehouseId == id).ToListAsync(ct);
foreach (var p in rows) p.StockQuantity += adjustment;
await _context.SaveChangesAsync(ct);

// Approach B
await _context.Products
    .Where(p => p.WarehouseId == id)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity + adjustment), ct);
```

---

**Answer:**

**Answer:** For **50k rows**, ship **Approach B (`ExecuteUpdateAsync`)** — one round trip, no change-tracker memory for 50k snapshots, and predictable duration. Approach A materializes every entity, holds original values for concurrency checks, and generates a large batched UPDATE (or many statements) through the tracker — workable in hundreds of rows, fragile at tens of thousands.

| Dimension | Approach A — tracked load + save | Approach B — `ExecuteUpdateAsync` |
|---|---|---|
| Memory | O(n) entities + snapshots | O(1) — no materialization |
| Round trips | 1 SELECT + 1 SAVE (large batch) | 1 UPDATE statement |
| Change tracker | Participates; interceptors fire on save | Bypasses tracker for the update |
| Concurrency tokens | Per-row `rowversion` in WHERE if configured | Token included in SET/WHERE if property mapped — still optimistic at SQL level |
| Partial failure | `SaveChanges` batch semantics | Single statement — all matching rows updated atomically |
| Observability | Can log each entity in interceptor | Log SQL + rows affected return value only |
| Domain logic per row | Possible in loop | Not possible — predicate must be pure SQL |

- **When A still wins:** each row needs different computed adjustment from loaded navigation data, or you must raise domain events per product.
- **Concurrency note:** if `Product` has `RowVersion`, Approach A detects conflicts per entity; Approach B updates all matching rows in one statement — concurrent edits to individual rows may throw or skip depending on provider SQL; for high contention, batch by key ranges or use explicit locking for the job window.
- **Observability:** log `rowsAffected` from `ExecuteUpdateAsync`; add a job audit row via separate `SaveChanges` in the same explicit transaction if needed (**InventoryTransactionService** pattern).

**Production takeaway:** Match **read model** (`AsNoTracking` catalog) and **write model** (set-based `ExecuteUpdate` / `ExecuteDelete`) to workload shape — tracking 50k entities for a scalar increment is a production incident waiting in QA. See **ChangeTrackingService.GetProductCatalogAsync** for read side; use Execute* for bulk writes.

---

---
