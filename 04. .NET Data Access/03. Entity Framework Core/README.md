# 03. Entity Framework Core

Full **EF Core** ORM track — domain models, migrations, relationships, LINQ queries, and change tracking. Complete **ADO.NET** and **Dapper** first so you understand what EF Core abstracts.

## Prerequisites

- **[03. MS SQL Server](../../03.%20MS%20SQL%20Server/README.md)** — schema, relationships, stored procedures
- **[02. C# & LINQ / 05. Language Integrated Query](../../02.%20C%23%20%26%20LINQ/05.%20Language%20Integrated%20Query/README.md)** — before chapter 08

## NuGet packages

| Package | Chapters |
|---------|----------|
| `Microsoft.EntityFrameworkCore` | all |
| `Microsoft.EntityFrameworkCore.SqlServer` | all |
| `Microsoft.EntityFrameworkCore.Design` | 03–11 (migrations, scaffolding, design-time) |

## Chapters (read in order)

| # | Folder | Status | What you learn |
|---|--------|--------|----------------|
| 01 | [Introduction to Entity Framework Core](./01.%20Introduction%20to%20Entity%20Framework%20Core/) | Complete | ORM concepts, EF Core providers, Code-First vs Database-First overview, vs ADO.NET/Dapper |
| 02 | [DbContext & DbSet](./02.%20DbContext%20%26%20DbSet/) | Complete | `DbContext`, `DbSet<T>`, options, connection configuration, DI registration |
| 03 | [Code-First Models & Migrations](./03.%20Code-First%20Models%20%26%20Migrations/) | Complete | Entity classes, conventions, `Add-Migration`, `Update-Database`, snapshots |
| 04 | [Database-First & Reverse Engineering](./04.%20Database-First%20%26%20Reverse%20Engineering/) | Complete | Existing DB as source of truth, `dotnet ef dbcontext scaffold`, generated entities, partial classes, re-scaffold workflow |
| 05 | [CRUD Operations & SaveChanges](./05.%20CRUD%20Operations%20%26%20SaveChanges/) | Complete | `Add`/`Update`/`Remove`, `SaveChanges`, entity state basics |
| 06 | [Relationships & Navigation Properties](./06.%20Relationships%20%26%20Navigation%20Properties/) | Complete | One-to-many, many-to-many, FKs, navigation properties, cascade delete |
| 07 | [Fluent API & Data Annotations](./07.%20Fluent%20API%20%26%20Data%20Annotations/) | Complete | `[Required]`, `[MaxLength]`, indexes, `OnModelCreating`, owned types preview |
| 08 | [LINQ to Entities & Query Patterns](./08.%20LINQ%20to%20Entities%20%26%20Query%20Patterns/) | Complete | `IQueryable`, deferred execution, filtering, projection, compiled queries preview |
| 09 | [Loading Related Data](./09.%20Loading%20Related%20Data/) | Complete | `Include`/`ThenInclude`, explicit loading, filtered include, lazy loading preview |
| 10 | [Raw SQL & Stored Procedures](./10.%20Raw%20SQL%20%26%20Stored%20Procedures/) | Complete | `FromSqlRaw`, `ExecuteSqlRaw`, `SqlQuery`, mapping to entities |
| 11 | [Change Tracking, Async & Transactions](./11.%20Change%20Tracking%2C%20Async%20%26%20Transactions/) | Complete | `AsNoTracking`, attach/update patterns, `SaveChangesAsync`, `BeginTransaction`, interceptors preview |

## Code-First vs Database-First

| Approach | When | Covered in |
|----------|------|------------|
| **Code-First** | Greenfield apps; C# model drives schema | ch.03 (+ migrations throughout) |
| **Database-First** | Legacy/existing SQL Server schema; DB drives model | ch.04 |

Both paths share the same `DbContext`, LINQ, and CRUD chapters from **ch.05** onward.

## LocalDB databases

| Database | Chapters | Purpose |
|----------|----------|---------|
| `EfCoreTutorial` | 02-03, 05-07, 10 | Code-First demos; ch06 adds `CatalogProducts` / relationship tables alongside earlier schemas |
| `EfScaffoldTutorial` | 04 | Database-First scaffold demo (`Categories`, `Products`) |
| `EfCoreLinqDemo` | 08 | LINQ query patterns (isolated seed data) |
| `AdoNetTutorial` | 09 | Loading related data (reuses ADO.NET/Dapper `Customers`, `Orders`, `OrderLines`) |
| `EfCoreInventoryTutorial` | 11 | Change tracking and transactions |

Chapter 04 bootstraps `EfScaffoldTutorial` at runtime; hand-written models match `dotnet ef dbcontext scaffold` output so the chapter builds without running the tool in CI.

## Grouped in chapters (no separate folder)

| Topic | Chapter |
|-------|---------|
| In-Memory provider (testing) | ch.01 (preview) |
| `Scaffold-DbContext` (Package Manager Console) | ch.04 |
| Table/column filtering during scaffold (`--table`, `--schema`) | ch.04 |
| Global query filters | ch.08 (preview) |
| Interceptors / events | ch.11 (preview) |
| EF Core with ASP.NET Core DI | DEFER → Web API modules |

## Next module

Continue to **[ASP.NET Core MVC](../../06.%20ASP.NET%20Core%20MVC/)** or **[ASP.Net Core Web API](../../08.%20ASP.Net%20Core%20Web%20API/)** depending on your learning path.
