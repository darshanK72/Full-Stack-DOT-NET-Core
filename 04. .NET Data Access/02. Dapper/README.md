# 02. Dapper

**Dapper** micro-ORM — maps SQL results to C# objects with almost no overhead. Complete **[01. ADO.NET](../01.%20ADO.NET/README.md)** first.

## NuGet packages

All chapters: **`Microsoft.Data.SqlClient`**, **`Dapper`**.

**Database:** All chapters share **`AdoNetTutorial`** on `(localdb)\MSSQLLocalDB` with the **`dbo.Products`** schema from ADO.NET ch.04 (`ProductName`, `StockQuantity`, `DiscontinuedDate`). Each chapter includes runtime bootstrap; ch.04 also creates `Customers`, `Orders`, and `OrderLines` for JOIN demos.

## Chapters (read in order)

| # | Folder | Status | What you learn |
|---|--------|--------|----------------|
| 01 | [Introduction to Dapper](./01.%20Introduction%20to%20Dapper/) | Complete | Role, `IDbConnection` extensions, setup, vs ADO.NET vs EF Core |
| 02 | [Queries, Execute & Async Methods](./02.%20Queries%2C%20Execute%20%26%20Async%20Methods/) | Complete | `Query*`/`Single*`/`First*`, `Execute`/`ExecuteScalar`, async variants, `dynamic` |
| 03 | [Parameters, Stored Procedures & QueryMultiple](./03.%20Parameters%2C%20Stored%20Procedures%20%26%20QueryMultiple/) | Complete | Anonymous/`DynamicParameters`, output params, stored procs, `QueryMultiple`/`GridReader`, `CommandDefinition` |
| 04 | [Mapping, Multi-Mapping & Advanced Patterns](./04.%20Mapping%2C%20Multi-Mapping%20%26%20Advanced%20Patterns/) | Complete | Column mapping, `splitOn`, multi-map, type handlers preview, Dapper.Contrib preview |

## Grouped in chapters (no separate folder)

| Topic | Chapter |
|-------|---------|
| Buffered vs unbuffered queries | ch.02 |
| Identity / `SCOPE_IDENTITY()` after insert | ch.02 |
| TVPs (table-valued parameters) | ch.03 (preview) |
| Dapper.SqlBuilder | ch.04 (preview) |

## Next submodule

**[03. Entity Framework Core](../03.%20Entity%20Framework%20Core/README.md)**
