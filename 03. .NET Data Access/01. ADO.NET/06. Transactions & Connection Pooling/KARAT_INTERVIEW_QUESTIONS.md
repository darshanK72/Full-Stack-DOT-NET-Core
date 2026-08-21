# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/06. Transactions & Connection Pooling`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

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

#### Q3. (D) You must transfer funds in SQL **and** publish a message to a separate SQL database (legacy reporting DB). A developer proposes `TransactionScope` with `Required` so both commits roll back together. Another proposes `BeginTransaction` on the primary DB only and "best-effort" reporting insert after commit.

Compare the approaches: distributed transaction pitfalls (MSDTC, cloud PaaS, latency), failure modes, and what you would ship in Azure SQL / containerized .NET 8.

---

#### Q4. (M) Two concurrent transfers between accounts 1 and 2 run at `IsolationLevel.ReadCommitted` (the chapter default in `AccountTransferService.TransferWithSqlTransaction`). Transfer A: 1 → 2 for $500. Transfer B: 2 → 1 for $400. Both read balances, both pass the funds check, both commit.

Explain whether this scenario can lose money or deadlock, how `Snapshot` differs from `ReadCommitted` here, and when you would enable database `ALLOW_SNAPSHOT_ISOLATION` vs bumping to `Serializable`.

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
