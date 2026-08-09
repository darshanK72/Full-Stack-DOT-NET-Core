# 04. .NET Data Access

All major **.NET database connectivity** approaches — from low-level **ADO.NET** through **Dapper** to full **Entity Framework Core**. Read submodules in order; depth increases and boilerplate decreases as you move down the stack.

## Prerequisites

- **[03. MS SQL Server](../03.%20MS%20SQL%20Server/README.md)** — T-SQL, DDL/DML; **BikeStores** restored for hands-on chapters
- **[02. C# & LINQ](../02.%20C%23%20%26%20LINQ/README.md)** — language fundamentals, LINQ (before EF Core ch.07), async/await (before ADO.NET ch.08)

## Sample database

Hands-on chapters assume **SQL Server LocalDB** with **BikeStores** unless a chapter notes otherwise. Default target: `(localdb)\MSSQLLocalDB`.

## Submodules (read in order)

| # | Module | Role | Chapters |
|---|--------|------|----------|
| 01 | [ADO.NET](./01.%20ADO.NET/README.md) | **Primary low-level track** — connections, commands, readers, adapters, transactions, async | 8 |
| 02 | [Dapper](./02.%20Dapper/README.md) | **Micro-ORM** — thin mapping layer on ADO.NET | 4 |
| 03 | [Entity Framework Core](./03.%20Entity%20Framework%20Core/README.md) | **Full ORM** — Code-First & Database-First, DbContext, migrations, LINQ, change tracking | 11 |

## When to use which

| Need | Choose |
|------|--------|
| Maximum control, legacy code, hand-tuned SQL | ADO.NET |
| Raw SQL with minimal mapping boilerplate | Dapper |
| Domain models, migrations, relationship graphs, LINQ queries | EF Core |

## Next module

**[06. ASP.NET Core MVC](../06.%20ASP.NET%20Core%20MVC/)** (or continue to Web API / microservices tracks as your path dictates)

## Skill for authors

Use **`@reading-tutorial`** when creating or editing chapter `Program.cs` files.
