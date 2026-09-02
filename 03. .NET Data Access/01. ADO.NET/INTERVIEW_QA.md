# 01. ADO.NET — Cross-Topic Interview Q&A
> Back to [.NET Data Access](../README.md)

## Subfolders

| # | Topic | Questions |
|---|-------|-----------|
| 01 | [Introduction to ADO.NET](01.%20Introduction%20to%20ADO.NET/INTERVIEW_QA.md) | Architecture, connected vs disconnected model |
| 02 | [SqlConnection & Connection Strings](02.%20SqlConnection%20%26%20Connection%20Strings/INTERVIEW_QA.md) | Connection pooling, `using`, secure config |
| 03 | [SqlCommand & Parameters](03.%20SqlCommand%20%26%20Parameters/INTERVIEW_QA.md) | Parameterized queries, SQL injection prevention |
| 04 | [SqlDataReader](04.%20SqlDataReader/INTERVIEW_QA.md) | Forward-only streaming, ordinal access, async |
| 05 | [DataSet, DataTable & SqlDataAdapter](05.%20DataSet%2C%20DataTable%20%26%20SqlDataAdapter/INTERVIEW_QA.md) | Disconnected model, in-memory tables, batch update |
| 06 | [Transactions & Connection Pooling](06.%20Transactions%20%26%20Connection%20Pooling/INTERVIEW_QA.md) | SqlTransaction, isolation levels, pool tuning |
| 07 | [Stored Procedures & Output Parameters](07.%20Stored%20Procedures%20%26%20Output%20Parameters/INTERVIEW_QA.md) | CommandType.StoredProcedure, output/return params |
| 08 | [Async ADO.NET](08.%20Async%20ADO.NET/INTERVIEW_QA.md) | `OpenAsync`, `ExecuteReaderAsync`, cancellation |

---

## Table of Contents

- [CQ1. Connected vs disconnected model: when to use SqlDataReader vs DataSet/DataAdapter.](#cq1-connected-vs-disconnected-model-when-to-use-sqldatareader-vs-datasetdataadapter)
- [CQ2. How do parameterized queries, stored procedures, and SqlTransaction work together to protect data integrity?](#cq2-how-do-parameterized-queries-stored-procedures-and-sqltransaction-work-together-to-protect-data-integrity)
- [CQ3. Connection pooling across the ADO.NET lifecycle: open, command, reader, close.](#cq3-connection-pooling-across-the-adonet-lifecycle-open-command-reader-close)
- [CQ4. Async ADO.NET: which operations block threads and how async methods fix that.](#cq4-async-adonet-which-operations-block-threads-and-how-async-methods-fix-that)

---

## CQ1. Connected vs disconnected model: when to use `SqlDataReader` vs `DataSet`/`DataAdapter`

**Concepts**
- SqlDataReader: forward-only, server cursor open for connection duration
- DataSet/DataAdapter: fills in-memory snapshot, connection released immediately
- server resource hold time as the key trade-off
- disconnected scenarios: offline sync, data binding, multi-table joins in memory
- streaming large result sets vs small cached result sets

**Answer**

The connected model keeps the database connection open for the entire read: `SqlDataReader` streams rows one at a time with a forward-only cursor, which minimises memory but holds the connection until `Close()` is called or the `using` block exits. This is correct for large result sets, high-concurrency APIs, and any scenario where you only need to process rows once and discard them. The disconnected model — `SqlDataAdapter.Fill(dataSet)` — opens the connection just long enough to pull all matching rows into an in-memory `DataTable`, then closes the connection immediately. The connection is free for other callers while your code works with the in-memory snapshot. Use `DataSet` when you need to hold data across multiple user interactions without re-querying (classic ASP.NET Web Forms binding, offline sync, unit-of-work batch updates with `SqlDataAdapter.Update`), or when the query returns multiple related tables that benefit from in-memory `DataRelation` navigation. For new API code, prefer `SqlDataReader` with manual mapping or Dapper, which gives connected-model efficiency with less boilerplate.

---

## CQ2. How do parameterized queries, stored procedures, and `SqlTransaction` work together to protect data integrity

**Concepts**
- parameterized query: treats input as data, not SQL text → blocks SQL injection
- stored procedure: server-side plan with explicit parameter contract
- SqlTransaction: ACID atomicity across multiple commands on one connection
- isolation level: controls read visibility under concurrent writers
- rollback on exception as the safe default

**Answer**

Parameterized queries and stored procedures both prevent SQL injection by binding user input to typed parameters rather than concatenating it into the SQL string. The difference is scope: a parameterized `SqlCommand` with `CommandType.Text` sends the parameterized query ad-hoc; `CommandType.StoredProcedure` invokes a server-side procedure whose text and plan are fixed — the caller can only supply declared parameters, not alter the SQL structure. Both are safe when used correctly. `SqlTransaction` wraps multiple commands in an ACID transaction on a single connection: open the connection, call `conn.BeginTransaction()`, assign the transaction to each command (`cmd.Transaction = tx`), and commit on success or roll back in the catch. Without an explicit transaction, each command auto-commits independently and a mid-sequence failure leaves partial state. The isolation level on the transaction controls whether readers see uncommitted writes (`ReadUncommitted`), committed writes mid-query (`ReadCommitted`, the SQL Server default), or get a consistent snapshot (`Snapshot`). The safe pattern: always wrap multi-command operations in a transaction, call `Rollback()` in the `catch`, and use parameterized commands or stored procedures for every user-supplied value.

---

## CQ3. Connection pooling across the ADO.NET lifecycle: open, command, reader, close

**Concepts**
- pool managed by ADO.NET driver per connection string
- `Open()` borrows from pool, `Close()`/`Dispose()` returns to pool
- reader left open holds the connection out of the pool
- `Min Pool Size` / `Max Pool Size` connection string keys
- pool exhaustion symptom: timeout waiting for connection

**Answer**

ADO.NET's SQL Server provider maintains a connection pool keyed to the connection string. `SqlConnection.Open()` does not create a new TCP socket — it borrows an already-warm connection from the pool. `Close()` or `Dispose()` (both via a `using` block) returns the logical connection to the pool, not to the server; the physical socket stays open, ready for the next borrow. The implication is that every part of the ADO.NET lifecycle that holds a connection delays its return: if you open a `SqlDataReader` and iterate slowly without closing it, that connection is unavailable to all other callers for the full duration. Under load this exhausts the pool (default max 100) and callers block waiting for a connection, throwing `InvalidOperationException: timeout expired` after 15 seconds. The safe pattern: open the connection as late as possible, close the reader and connection as early as possible using nested `using` blocks, and always close readers before opening a second command on the same connection. Pool tuning (`Min Pool Size`, `Max Pool Size`, `Connect Timeout`) belongs in `appsettings.json` rather than hard-coded.

---

## CQ4. Async ADO.NET: which operations block threads and how async methods fix that

**Concepts**
- synchronous Open/Execute blocks a thread during network I/O
- OpenAsync / ExecuteReaderAsync / ReadAsync release the thread to the pool
- CancellationToken propagation through async chain
- await using for async disposal
- no benefit for CPU-bound in-process work

**Answer**

Synchronous `Open()`, `ExecuteNonQuery()`, `ExecuteScalar()`, and `SqlDataReader.Read()` all perform network I/O to the database and block the calling thread for the entire round-trip — on a busy ASP.NET Core server this wastes a thread-pool thread that could be serving other requests. The async counterparts — `OpenAsync`, `ExecuteNonQueryAsync`, `ExecuteScalarAsync`, `ExecuteReaderAsync`, `ReadAsync` — issue the I/O and `await` the result, returning the thread to the pool during the wait. The correct pattern chains `await using` through the entire stack: `await using var conn = new SqlConnection(...); await conn.OpenAsync(ct); await using var cmd = ...; await using var reader = await cmd.ExecuteReaderAsync(ct); while (await reader.ReadAsync(ct)) { ... }`. Pass the `CancellationToken` from the HTTP request context to every async call so that client disconnects propagate cleanly and abort the database query rather than letting it run to completion after the response is gone.
