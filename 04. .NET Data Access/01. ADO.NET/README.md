# 01. ADO.NET

Core **ADO.NET** — the foundation for SQL Server access in .NET. Everything in Dapper and EF Core ultimately uses these primitives.

## Prerequisites

- **[03. MS SQL Server](../../03.%20MS%20SQL%20Server/README.md)** chapters 01–05
- **[02. C# & LINQ / 06. Multithreading & Async Programming](../../02.%20C%23%20%26%20LINQ/06.%20Multithreading%20%26%20Async%20Programming/README.md)** — before chapter 08

## NuGet package

Chapters **02–08** use **`Microsoft.Data.SqlClient`**.

## Demo databases (LocalDB)

| Database | Chapters | Notes |
|----------|----------|-------|
| **AdoNetTutorial** | 02–04, 06, 08 | Shared sample DB; each chapter includes setup SQL in `Program.cs` comments |
| **AdoNetSpDemo** | 07 | Stored procedure demos (separate schema to avoid conflicting with `AdoNetTutorial`) |
| **BikeStores** (optional) | 05 | Optional `SqlDataAdapter.Fill` demo if you already have this sample database |

## Chapters (read in order)

| # | Folder | Status | What you learn |
|---|--------|--------|----------------|
| 01 | [Introduction to ADO.NET](./01.%20Introduction%20to%20ADO.NET/) | Complete | Stack, providers, connected vs disconnected model, ADO.NET vs Dapper vs EF Core |
| 02 | [SqlConnection & Connection Strings](./02.%20SqlConnection%20%26%20Connection%20Strings/) | Complete | `SqlConnection`, connection strings, `using`, state, open/close |
| 03 | [SqlCommand & Parameters](./03.%20SqlCommand%20%26%20Parameters/) | Complete | `ExecuteNonQuery`/`ExecuteScalar`, `SqlParameter`, SQL injection prevention |
| 04 | [SqlDataReader](./04.%20SqlDataReader/) | Complete | Forward-only reads, typed accessors, `IsDBNull`, multiple result sets |
| 05 | [DataSet, DataTable & SqlDataAdapter](./05.%20DataSet%2C%20DataTable%20%26%20SqlDataAdapter/) | Complete | Disconnected model, `Fill`, `DataRow`/`DataColumn`, batch updates overview |
| 06 | [Transactions & Connection Pooling](./06.%20Transactions%20%26%20Connection%20Pooling/) | Complete | `SqlTransaction`, commit/rollback, pooling, `SqlBulkCopy` preview |
| 07 | [Stored Procedures & Output Parameters](./07.%20Stored%20Procedures%20%26%20Output%20Parameters/) | Complete | `CommandType.StoredProcedure`, input/output/return parameters |
| 08 | [Async ADO.NET](./08.%20Async%20ADO.NET/) | Complete | `OpenAsync`, `ExecuteReaderAsync`, cancellation tokens |

## Next submodule

**[02. Dapper](../02.%20Dapper/README.md)**
