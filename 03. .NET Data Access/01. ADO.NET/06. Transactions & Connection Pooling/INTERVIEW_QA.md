# Transactions & Connection Pooling — Interview Q&A
> Back to [ADO.NET Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Transactions & Connection Pooling](#chapter-06-transactions--connection-pooling)
  - [Q1. What are the ACID properties of a transaction?](#q1-what-are-the-acid-properties-of-a-transaction)
  - [Q2. How do you begin, commit, and rollback a transaction in ADO.NET?](#q2-how-do-you-begin-commit-and-rollback-a-transaction-in-adonet)
  - [Q3. Why must all commands in a transaction share the same connection?](#q3-why-must-all-commands-in-a-transaction-share-the-same-connection)
  - [Q4. What is `TransactionScope`, and how does it differ from `SqlTransaction`?](#q4-what-is-transactionscope-and-how-does-it-differ-from-sqltransaction)
  - [Q5. What is a pool exhaustion error, and what typically causes it?](#q5-what-is-a-pool-exhaustion-error-and-what-typically-causes-it)
  - [Q6. What isolation levels exist, and why do they matter?](#q6-what-isolation-levels-exist-and-why-do-they-matter)
  - [Q7. What is the correct pattern for rollback in a `try/catch` around ADO.NET transactions?](#q7-what-is-the-correct-pattern-for-rollback-in-a-trycatch-around-adonet-transactions)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 06. Transactions & Connection Pooling

### Q1. What are the ACID properties of a transaction?

**Concepts**
- Atomicity: all-or-nothing commit
- Consistency: constraints hold before and after
- Isolation: concurrent transactions see controlled views
- Durability: committed data survives crashes (log flush)

**Answer**

ACID describes the four guarantees a transactional database provides: Atomicity (all commands commit or all roll back), Consistency (constraints and rules remain valid), Isolation (concurrent transactions see controlled data views per the chosen isolation level), and Durability (committed changes persist even if power fails immediately afterward). Together these properties make multi-statement database operations reliable and predictable.

---

### Q2. How do you begin, commit, and rollback a transaction in ADO.NET?

**Concepts**
- `connection.BeginTransaction()` returns `SqlTransaction`
- Assign `cmd.Transaction = tx` to every participating command
- `tx.Commit()` on success, `tx.Rollback()` in catch
- All commands share the same connection and transaction instance

**Answer**

Open a connection, call `BeginTransaction()` to get a `SqlTransaction`, assign it to each command's `Transaction` property, execute all commands inside `try`, then call `Commit()` on success or `Rollback()` in `catch` before rethrowing. Dispose the transaction object when finished — `await using` handles this cleanly in async code. All participating commands must share both the same connection and the same transaction instance.

---

### Q3. Why must all commands in a transaction share the same connection?

**Concepts**
- Transaction bound to a single database session
- Separate pooled connections are independent sessions
- Atomicity across sessions requires distributed transaction coordinator
- EF Core enforces the same rule on its underlying connection

**Answer**

A transaction is bound to a single database session — SQL Server's transaction context does not span multiple physical connections from the pool. Each pooled connection is an independent session, so atomicity cannot cross sessions without an expensive distributed transaction coordinator. The standard pattern is one `using` connection, one transaction, and multiple sequential commands all assigned to that transaction.

---

### Q4. What is `TransactionScope`, and how does it differ from `SqlTransaction`?

**Concepts**
- `TransactionScope`: ambient transaction, can escalate to distributed
- `SqlTransaction`: lightweight, single-connection, explicit
- `Complete()` to commit; dispose without Complete = rollback
- Distributed transactions discouraged in modern cloud/microservices

**Answer**

`TransactionScope` is a `System.Transactions` API that marks a code block as transactional and can escalate to a distributed transaction when multiple connections or resource managers enlist. `SqlTransaction` is a lightweight, single-connection transaction tied explicitly to one ADO.NET connection. `SqlTransaction` is simpler and faster for single-database work in ASP.NET Core; distributed `TransactionScope` is discouraged in cloud apps — use sagas or outbox patterns instead.

---

### Q5. What is a pool exhaustion error, and what typically causes it?

**Concepts**
- All pool slots occupied past the timeout window
- Undisposed connections/readers and long-held transactions
- Error surfaces at `Open`, not at the original leak
- Fix: prompt disposal and short transaction lifetime

**Answer**

Pool exhaustion happens when no pooled connection is free within the timeout window — commonly because connections or readers were not disposed, or because long transactions hold slots during traffic spikes. The error surfaces at `Open` rather than at the original leak site, which makes diagnosis harder. Fixing disposal and keeping transactions short resolves most cases without raising `Max Pool Size`.

---

### Q6. What isolation levels exist, and why do they matter?

**Concepts**
- READ UNCOMMITTED, READ COMMITTED (default), REPEATABLE READ, SNAPSHOT, SERIALIZABLE
- Lower levels allow dirty reads / phantoms but reduce blocking
- SNAPSHOT uses row versioning, no shared read locks
- Set via `BeginTransaction(IsolationLevel)` in ADO.NET

**Answer**

Isolation levels control how much one transaction sees of another's uncommitted or in-flight changes, trading consistency for concurrency. SQL Server offers READ UNCOMMITTED, READ COMMITTED (default), REPEATABLE READ, SNAPSHOT, and SERIALIZABLE. Lower levels reduce blocking but allow phenomena like dirty reads; SERIALIZABLE prevents all anomalies at the cost of lock contention; SNAPSHOT provides consistent reads via row versioning without shared locks and is popular for read-heavy OLTP workloads.

---

### Q7. What is the correct pattern for rollback in a `try/catch` around ADO.NET transactions?

**Concepts**
- Begin transaction after connection opens, before first command
- `Commit()` only when all steps succeed
- `Rollback()` in `catch` before rethrowing
- `await using var tx` for async disposal

**Answer**

Begin the transaction after the connection is open, execute all commands inside `try`, and call `Commit()` only when every step succeeds. In `catch`, call `Rollback()` before rethrowing so callers know the operation failed — swallowing exceptions without rollback leaves the connection in an aborted state. Use `await using var tx` and explicit `RollbackAsync` in `catch` for the cleanest async pattern in ASP.NET Core.

---

## Gotchas

#### Gotcha 5. Transaction started after first command

**Answer:** Beginning a `SqlTransaction` only after the first statement already executed means that statement committed under implicit autocommit, so later steps in the intended unit of work are not atomic with the first.

- Call `BeginTransaction` immediately after opening the connection, before any DML.
- EF Core `SaveChanges` without an explicit transaction auto-commits each call — wrap multi-step work explicitly.
- Integration tests with single-user data often miss this race because implicit commits appear to "work."

---

#### Gotcha 4. Leaked connections exhaust the pool

**Answer:** Failing to dispose `SqlConnection`, `SqlDataReader`, or abandoning a `using` block early leaks connection pool slots until timeout, eventually causing "timeout expired obtaining connection from pool" errors under load.

- Always use `await using` for connections and readers so disposal runs on exceptions too.
- Symptoms appear only under concurrent load, making this a classic production-only failure mode.
- Long-lived undisposed `DbContext` instances cause the same exhaustion pattern.

---

#### Gotcha 6. Dapper `Query` without `using` on connection

**Answer:** Returning deferred `IEnumerable<T>` from Dapper before disposing the connection postpones execution until enumeration, failing at runtime or holding connections open until garbage collection.

- Materialize inside the connection scope with `.ToList()` or `.ToArray()` before returning from the method.
- Deferred execution means SQL runs when the caller iterates — often after the `using` block closed the connection.
- Async variants (`QueryAsync`) still require materialization before leaving the connection lifetime.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A teammate refactors `AccountTransferService` to "reuse one connection per request" and adds a nested transaction for audit logging. Review:

```csharp
public async Task TransferAsync(int fromId, int toId, decimal amount, CancellationToken ct)
{
    _connection.Open(); // static SqlConnection field, opened once at startup

    using SqlTransaction outer = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
    try
    {
        Debit(fromId, amount); // SqlCommand without cmd.Transaction set

        using SqlTransaction inner = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
        InsertAuditRow(fromId, toId, amount); // cmd.Transaction = inner
        inner.Commit();

        Credit(toId, amount); // cmd.Transaction = outer
        outer.Commit();
    }
    catch
    {
        outer.Rollback();
        throw;
    }
}
```

What breaks at runtime, what happens to the connection pool under load, and what is the prioritized fix?

---

**Answer:** This stacks three production failures: a **cached static connection** (not returned to the pool), **un-enlisted commands** that auto-commit outside the outer transaction, and an **invalid nested `BeginTransaction`** on SQL Server without savepoints. Under load the single connection becomes a cross-request bottleneck and pool metrics lie — most threads still open new connections elsewhere while this one never closes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pooling / design | Static `SqlConnection` opened at startup | Connection never `Close`/`Dispose` → not returned to pool; violates **ConnectionPoolingDemo** rule "borrow, use, return" |
| Correctness | `Debit` omits `cmd.Transaction = outer` | UPDATE runs in implicit auto-commit — debit persists even if outer rolls back |
| Runtime | Nested `BeginTransaction` on same connection | SQL Server throws *"The connection does not support this method"* (or similar) — no true nested transactions without **`Save`** savepoints |
| Concurrency | One shared connection across requests | Race on shared `SqlConnection` / `SqlTransaction`; undefined behavior under parallel calls |
| Async | Method is `async` but sync ADO.NET inside | Misleading signature; thread blocked during I/O (see chapter 08 preview) |

**Fix (priority order):**

1. **Remove static connection** — `using SqlConnection connection = new SqlConnection(_cs); await connection.OpenAsync(ct);` per operation (or per scoped unit of work).
2. **Enlist every command** — pass `connection` and `transaction` into constructors or set `cmd.Transaction = tx` on **Debit**, **Credit**, and audit (see **AccountTransferService** `ReadBalance` / `ExecuteBalanceUpdate`).
3. **Replace nested `BeginTransaction`** with one transaction + optional **`transaction.Save("Audit")`** / rollback to savepoint, or commit audit in the same transaction before debit/credit.
4. **Use `using` for transaction** — let `Dispose` roll back on failure; explicit `Rollback()` in `catch` is fine but redundant if rethrowing after dispose.
5. Wire **async end-to-end** when the method is async (`ExecuteNonQueryAsync`, pass `ct`).

**Production takeaway:** One open transaction + one pooled connection per request is normal; **one static connection for the app** is a pool leak and a concurrency bug. Nested transactions in ADO.NET mean **savepoints**, not a second `BeginTransaction`.

---

#### Q2. (R) Production intermittently reports `SqlException: Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool.` Review this "safe transfer" path adapted from the chapter's debit/credit pattern:

```csharp
public bool TransferWithSideEffects(int fromId, int toId, decimal amount)
{
    SqlConnection connection = new SqlConnection(_connectionString);
    connection.Open();
    SqlTransaction tx = connection.BeginTransaction(IsolationLevel.ReadCommitted);

    decimal balance = ReadBalance(connection, tx, fromId);
    if (balance < amount)
    {
        tx.Rollback();
        return false; // early exit — "handled"
    }

    ExecuteBalanceUpdate(connection, tx, fromId, -amount);
    SendFraudCheckEmail(fromId, toId, amount); // synchronous SMTP — 2–8 seconds
    ExecuteBalanceUpdate(connection, tx, toId, amount);

    tx.Commit();
    connection.Close();
    return true;
}
```

Identify correctness, pooling, and isolation issues. What would you change first for a high-traffic API?

---

**Answer:** The transaction **holds locks and a pooled connection** while waiting on SMTP, which starves the pool (`Max Pool Size=100` in **ConnectionStrings.AdoNetTutorial**) and increases deadlock risk. Early `return false` after rollback **leaks the connection** because `Close()` is skipped, and there is no `using`/`finally` — the opposite of **TransactionUsingPatternDemo**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pool exhaustion | Long-running txn while connection is open | Each in-flight transfer occupies a pool slot for seconds → `Timeout expired ... obtaining a connection from the pool` |
| Resource leak | `return false` after rollback without `Close`/`Dispose` | Connection not returned to pool — permanent slot loss until GC finalizer |
| Design | Non-transactional I/O inside SQL transaction | Extends lock duration on account rows; other transfers block or deadlock |
| Maintainability | No `using` / `try-finally` | Any exception path may skip `Rollback`/`Close` — relies on finalizer |
| Isolation | `ReadCommitted` + long txn | Writers on same accounts queue; deadlock probability rises |

**Fix (priority order):**

1. **`using` for connection and transaction** — or `try/finally` with `connection?.Dispose()` so every path returns the connection ( **Program.cs** quick reference: Dispose → auto-rollback if not committed).
2. **Shorten transaction scope** — open txn → read balance → debit → credit → commit; **move `SendFraudCheckEmail` after commit** (or outbox/event after success).
3. **Insufficient funds** — `return false` inside `using` block so dispose runs; no manual leak.
4. **Optional** — retry policy for transient deadlocks **outside** a held connection, not inside a multi-second txn.
5. Monitor **pool counters** and average txn duration; tune `Max Pool Size` only after fixing hold time.

**Production takeaway:** A transaction should cover **database work only**. Holding a pooled connection during network I/O is a common cause of pool timeouts — see **ConnectionPoolingDemo** (connections must be Closed/Disposed to return).

---

#### Q3. (D) You must transfer funds in SQL **and** publish a message to a separate SQL database (legacy reporting DB). A developer proposes `TransactionScope` with `Required` so both commits roll back together. Another proposes `BeginTransaction` on the primary DB only and "best-effort" reporting insert after commit.

Compare the approaches: distributed transaction pitfalls (MSDTC, cloud PaaS, latency), failure modes, and what you would ship in Azure SQL / containerized .NET 8.

---

**Answer:** **`TransactionScope` across two SQL databases** enlist a **distributed transaction (MSDTC)** — strong atomicity but fragile in modern cloud and container hosts. **`BeginTransaction` on the ledger only** keeps the money path simple; reporting consistency is handled with **outbox, idempotent consumers, or eventual sync** — the pattern most teams ship on Azure SQL / .NET 8.

- **`TransactionScope` + two databases:** Both connections enlist in DTC; commit is two-phase. **Pitfalls:** MSDTC must be enabled and reachable (often **not** available or discouraged on Linux containers, Azure App Service, serverless); higher latency; harder observability; one participant failure aborts all — including the primary transfer.
- **`BeginTransaction` (primary only) + post-commit insert:** Transfer is ACID on the ledger. If reporting insert fails after commit, **money moved but report missing** — requires reconciliation job or retry queue.
- **Production pattern (.NET 8 / Azure SQL):** Single-db **`SqlTransaction`** (as in **AccountTransferService**) for the transfer; write an **outbox row in the same transaction**; background worker publishes to reporting DB with **idempotency keys**. Avoid cross-db DTC unless compliance mandates it and infra supports MSDTC.
- **When `TransactionScope` is reasonable:** Single database (scope still works but **`connection.BeginTransaction`** is clearer), or explicit DTC with tested Windows/MSDTC topology — rare in greenfield cloud.
- **Failure modes:** DTC — partial enlist / timeout / firewall; best-effort — duplicate or missing reporting rows without idempotency.

**Production takeaway:** Karat tests whether you know **`TransactionScope` ≠ free nested local txn** — multi-resource scopes are **distributed** and often the wrong default in PaaS. Prefer **one SQL transaction + outbox** over two-phase commit across servers.

---

#### Q4. (M) Two concurrent transfers between accounts 1 and 2 run at `IsolationLevel.ReadCommitted` (the chapter default in `AccountTransferService.TransferWithSqlTransaction`). Transfer A: 1 → 2 for $500. Transfer B: 2 → 1 for $400. Both read balances, both pass the funds check, both commit.

Explain whether this scenario can lose money or deadlock, how `Snapshot` differs from `ReadCommitted` here, and when you would enable database `ALLOW_SNAPSHOT_ISOLATION` vs bumping to `Serializable`.

---

**Answer:** With the chapter's **read-check-update** pattern at **ReadCommitted**, both transfers can pass balance checks on stale reads and **overdraw an account** (lost update / race) — not prevented by ReadCommitted alone. They can also **deadlock** when each holds a lock on one account and waits on the other. **Snapshot** gives statement-level consistent reads without blocking writers (with row versioning); **Serializable** prevents the race but increases blocking.

- **ReadCommitted (default in `IsolationLevelReference`):** No dirty reads, but **non-repeatable reads** allowed — two concurrent txns can each read $1000 on account 1, both debit, one commits overdraft.
- **Deadlock:** A locks account 1 then waits on 2; B locks 2 then waits on 1 → SQL Server picks a **1205 deadlock victim** (see Q5). Ordering accounts by `AccountId` before locking mitigates.
- **Snapshot:** Uses **row versions** in `tempdb`; readers don't block writers. Still need **optimistic concurrency** (`WHERE Balance = @expected` or rowversion) to detect write conflicts on update — snapshot alone doesn't replace application checks.
- **`ALLOW_SNAPSHOT_ISOLATION`:** Database option + `BeginTransaction(IsolationLevel.Snapshot)` — good for **read-heavy reporting** and reducing reader/writer deadlocks; watch **tempdb** sizing.
- **Serializable:** Prevents phantom/non-repeatable issues for the transfer pattern but **holds range locks** — higher blocking; use sparingly for short units of work.
- **Practical fix for transfers:** **`UPDLOCK, ROWLOCK`** hint or single `UPDATE ... WHERE Balance >= @amount` with rows-affected check — keeps txn short as **IsolationLevelReference** recommends.

**Production takeaway:** **ReadCommitted + read-then-write** is a classic production bug; **Snapshot** improves read consistency but **doesn't replace atomic conditional updates**. Prefer **short transactions + consistent lock order + optimistic rowversion** over default Serializable.

---

#### Q5. (R) A catch block "matches the tutorial" but omits rollback on some paths. Review:

```csharp
public void Transfer(int fromId, int toId, decimal amount)
{
    using SqlConnection connection = new SqlConnection(_cs);
    connection.Open();
    using SqlTransaction tx = connection.BeginTransaction();

    try
    {
        ExecuteBalanceUpdate(connection, tx, fromId, -amount);
        ExecuteBalanceUpdate(connection, tx, toId, amount);
        tx.Commit();
    }
    catch (SqlException ex) when (ex.Number == 1205) // deadlock victim
    {
        _logger.LogWarning(ex, "Deadlock — retry later");
        // no Rollback()
    }
    catch (Exception ex)
    {
        tx.Rollback();
        throw;
    }
}
```

What state is the connection/transaction in after the deadlock catch, and how should retry + cleanup be structured? Relate to the chapter note that `Dispose` without `Commit` rolls back.

---

**Answer:** After the **1205 catch without rethrow**, execution falls through with a **doomed transaction** still open until `tx.Dispose()` — which **does roll back** per **AccountTransferService** / **TransactionUsingPatternDemo** notes, but the method **returns success implicitly** (void, no retry). Callers believe the transfer may have succeeded; the connection returns to the pool only after `using` ends — acceptable only if no further commands run on that txn.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Deadlock path swallowed — no retry, no failure signal | Caller thinks transfer completed; business event lost |
| Transaction state | Uncommitted txn until `Dispose` | Further commands on same `tx` would fail (*"Transaction has completed"*) |
| Design | Selective rollback only in generic `catch` | Inconsistent with **AccountTransferService** explicit rollback + result tuple |
| Reliability | No idempotency / max retry | Repeated deadlocks under load silently drop transfers |

**Fix (priority order):**

1. **Deadlock catch:** either **`throw`** after log (let caller retry) or **`tx.Rollback()`** + bounded retry with **exponential backoff** on a **new connection/transaction** — never continue using the victim txn.
2. Rely on **`using SqlTransaction`** — `Dispose` without `Commit` **always rolls back** (chapter note); explicit `Rollback()` documents intent but is optional before dispose.
3. Return **`(bool Success, ...)`** or throw **`TransferFailedException`** so callers don't assume commit.
4. **Retry policy:** only for transient errors (1205, -2 timeout); cap retries; **consistent account lock order** to reduce deadlocks (Q4).
5. Do **not** leave empty catch that completes normally — worst of both worlds (silent failure + unclear txn state).

**Production takeaway:** **`Dispose` saves you from a committed-when-you-meant-rollback bug**, but it **does not replace retry semantics or API contracts** — deadlocks must propagate or retry explicitly.

---

#### Q6. (P) Chapter 06 previews async transactions (`AsyncTransactionPreview`); chapter 08 covers full async ADO.NET. A service method is marked `async` but implemented like this:

```csharp
public async Task<bool> TransferAsync(int fromId, int toId, decimal amount, CancellationToken ct)
{
    using SqlConnection connection = new SqlConnection(_cs);
    await connection.OpenAsync(ct);
    using SqlTransaction tx = connection.BeginTransaction(IsolationLevel.ReadCommitted);

    decimal balance = (decimal)(await new SqlCommand(
        "SELECT Balance FROM dbo.Accounts WHERE AccountId = @id", connection, tx)
    {
        Parameters = { { "@id", fromId } }
    }.ExecuteScalarAsync()); // no ct passed

    ExecuteBalanceUpdate(connection, tx, fromId, -amount); // sync ExecuteNonQuery
    ExecuteBalanceUpdate(connection, tx, toId, amount);    // sync ExecuteNonQuery

    tx.Commit();
    return true;
}
```

What scalability and cancellation gaps remain, and what is the correct async transaction pattern end-to-end?

---

**Answer:** The method **async only at the door** — sync **`ExecuteNonQuery`** blocks thread-pool threads during writes, and **`ExecuteScalarAsync()` without `ct`** ignores client abort. **`BeginTransaction()`** remains synchronous (acceptable); everything enlisted in the txn should use **`*Async` + `CancellationToken`** and stay **short**, matching **AsyncTransactionPreview** forward reference to chapter 08.

- **Pass `ct` to every async execute** — `ExecuteScalarAsync(ct)`, `ExecuteNonQueryAsync(ct)`; cancellation aborts waiting I/O and should trigger dispose → rollback.
- **Replace sync updates** with async counterparts inside the same `SqlTransaction` — enlistment rules unchanged (**AccountTransferService** Section 6).
- **No sync-over-async** inside ASP.NET request — mixed pattern still starves the pool under concurrent load.
- **Structure:** `await OpenAsync` → `using var tx = BeginTransaction()` → await reads/writes → `Commit()`; on exception, dispose tx (rollback) and optionally rethrow.
- **`BeginTransactionAsync`** exists on some providers; **`Microsoft.Data.SqlClient`** — use sync `BeginTransaction` after `OpenAsync` (documented pattern).
- **Business rules unchanged** — insufficient funds check before debit; keep txn scope minimal (Q2).

**Production takeaway:** Async transactions mean **async I/O on every command in the unit of work**, not `async` on the method signature alone — see **AsyncTransactionPreview** and chapter 08 for full coverage.

---

#### Q7. (R) After a deploy, the API exhausts `Max Pool Size=100` within minutes. Review the repository registered as **Singleton**:

```csharp
public sealed class AccountRepository
{
    private readonly SqlConnection _connection = new SqlConnection(_cs);

    public decimal GetBalance(int accountId)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        // SELECT ... — no transaction
        return balance;
    }

    public void UpdateBalance(int accountId, decimal delta)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        // UPDATE ... — auto-commit each call
    }
}
```

Two `AccountTransferService` instances (scoped) call this singleton concurrently. Explain pool starvation vs "only one connection," and the fix aligned with `ConnectionPoolingDemo` rules.

---

**Answer:** A **singleton holding one open `SqlConnection`** removes that physical connection from the pool **for the app lifetime** and makes all requests **share one non-thread-safe connection** — concurrent scoped services corrupt reader/state and serialize DB access. Pool exhaustion still happens elsewhere because **other code paths correctly open pooled connections** that never return when leaked; this pattern is both a **logical leak** and a **concurrency defect**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pooling | Long-lived open connection on singleton | One slot permanently checked out; not returned until app shutdown — violates **ConnectionPoolingDemo** "Do not cache SqlConnection in static fields" |
| Concurrency | `SqlConnection` not thread-safe | Interleaved `GetBalance`/`UpdateBalance` from two scoped transfers → mixed results, exceptions |
| Transactions | Auto-commit per call | **Debit + credit not atomic** — crash between calls loses money; can't assign shared `SqlTransaction` safely across callers |
| DI lifetime | Singleton repo + scoped service | Classic captive dependency — stateful infra in wrong lifetime |

**Fix (priority order):**

1. **Register repository as scoped** (or transient) — **no instance fields holding `SqlConnection`**.
2. **Per-operation pattern:** `using var connection = new SqlConnection(_cs); await connection.OpenAsync(ct);` — matches **AccountTransferService** and **ConnectionPoolingDemo** open/close cycle.
3. **Transfers:** one connection, one **`BeginTransaction`**, enlist all commands — not two singleton auto-commit updates.
4. **Optional factory:** `IDbConnection` factory registered singleton creating **new** connections per use — factory is singleton, connections are not.
5. Diagnose leaks with pool timeout errors + ensure **`using`** on every `SqlConnection` (Q2 early return lesson).

**Production takeaway:** Pool exhaustion is often **leaked or hoarded connections**, not "need Max Pool Size=500." **Borrow → use → dispose** every time; singleton + open connection is a dual bug (pool + thread safety).
