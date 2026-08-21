# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/02. Dapper/02. Queries, Execute & Async Methods`

---

#### Q1. (R) A catalog API exposes "get product by category." A teammate ships this repository method. Review it — what breaks in production as the catalog grows, and what Dapper API would you use instead?

**Answer:** `QuerySingle` throws `InvalidOperationException` when more than one row matches — fine for PK lookups, wrong for a non-unique `CategoryId` filter once a category has two or more products.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | `QuerySingle` on a non-unique key | Intermittent 500s when a second product shares the category |
| API design | Returns one `Product` for a one-to-many relationship | Wrong contract — callers cannot list or choose among matches |
| Dapper semantics | Misapplied `QuerySingle` vs `QueryFirst` | `QueryFirst` would hide duplicates silently; neither fixes the domain model |

**Fix (priority order):**

1. Change the contract: return `IReadOnlyList<Product>` via `Query<Product>` (or `QueryAsync` + `ToList()`) when many rows are valid.
2. If the business rule is truly "one product per category," enforce it in the database (unique constraint on `CategoryId`) and keep `QuerySingle` — or use `QuerySingleOrDefault` when absence is acceptable.
3. If you only need an arbitrary representative row, use `QueryFirst` with explicit `TOP 1` and `ORDER BY` — document that choice in the API.

**Production takeaway:** Karat tests whether you match Dapper's single-row helpers to **SQL uniqueness guarantees** — PK/`QuerySingle`, `TOP 1`/`QueryFirst`, many rows/`Query<T>`.

---

#### Q2. (R) An export endpoint needs to stream millions of product rows with minimal memory. A developer refactors the repository like this. Review the full path — what fails at runtime, and why does the bug pass unit tests that mock `IEnumerable<Product>`?

**Answer:** `buffered: false` keeps a live `SqlDataReader` tied to the connection; disposing the connection inside `StreamAllActive` before the controller enumerates causes read failures — mocks never open a real reader, so tests stay green.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Connection lifetime | `using` closes connection before deferred enumeration | `InvalidOperationException` or "Invalid attempt to call Read when reader is closed" in production |
| Deferred execution | Dapper SQL runs on first `MoveNext`, not at `Query` call | Failure happens in the controller, far from the repository — hard to diagnose |
| Testability | Mock `IEnumerable<Product>` returns an in-memory list | Hides the connection/reader coupling entirely |
| API / hosting | Returning deferred `IEnumerable` across layer boundaries | ASP.NET serialization may force full enumeration on a dead connection |

**Fix (priority order):**

1. Keep the connection open for the entire enumeration — e.g., pass an open scoped connection into the repository and enumerate inside the same request scope, **or** materialize inside the method with `buffered: true` / `.ToList()` when the set is bounded.
2. For true streaming at scale, use `buffered: false` but consume rows **inside** the repository or a dedicated streaming abstraction (`IAsyncEnumerable<Product>` with `await foreach`, connection open until the stream completes).
3. Add an integration test that uses a real `SqlConnection` and defers enumeration past the repository return — catches this class of bug.

**Production takeaway:** Unbuffered Dapper queries are a **connection-lifetime contract** — the tutorial's `DemonstrateUnbufferedConnectionRequirement` pitfall exactly; default `buffered: true` is safer unless you control the full read pipeline.

---

#### Q3. (R) A health-check endpoint reports inventory count. Review this action — identify compile-time, runtime, and scalability issues:

**Answer:** Blocking on `.Result` for `ExecuteScalarAsync` defeats async I/O, risks thread-pool starvation under load, and offers no cancellation — the sync `using` connection also fights ASP.NET Core's async request model.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `ExecuteScalarAsync` | Sync-over-async; thread blocked during network I/O |
| Scalability | Sync action + blocked thread per health probe | Orchestrator polling can saturate the thread pool |
| Hosting | `using var connection` in sync action | Works locally; composes poorly with async middleware and `CancellationToken` |
| Resilience | No `CancellationToken` passed to Dapper | Hung SQL calls cannot abort when the client disconnects |

**Fix (priority order):**

1. Make the action async: `public async Task<IActionResult> GetActiveProductCount(CancellationToken ct)` and `await connection.ExecuteScalarAsync<int>(sql, cancellationToken: ct)` (via `CommandDefinition` when you need token support — see Dapper ch03).
2. Prefer injecting a scoped repository/service rather than opening `SqlConnection` inline in the controller.
3. Remove `.Result` / `.Wait()` entirely — async end-to-end from controller through Dapper.

**Production takeaway:** Dapper's `*Async` siblings exist for the same reason as ADO.NET async — use `await ExecuteScalarAsync`, not `.Result`. See C# Module 06 — sync-over-async in ASP.NET actions.

---

#### Q4. (M) Two junior developers argue about the right Dapper call for these operations. For each snippet, name the **correct** Dapper method (`Query`, `QueryFirst`, `Execute`, or `ExecuteScalar`) and what goes wrong if they ship as written:

**Answer:** Each snippet uses the wrong Dapper entry point — `Execute`/`ExecuteScalar`/`Query` are not interchangeable; picking the wrong one yields cast exceptions, empty sequences, or silent logic bugs.

- **A — UPDATE rows affected:** Use **`Execute`**. `ExecuteScalar` returns the first column of the first row — for a plain `UPDATE` that is often `NULL` or meaningless, and casting to `int` is fragile. `Execute` returns `rows affected` directly.
- **B — single aggregate:** Use **`ExecuteScalar<decimal>`** (or `ExecuteScalar<decimal?>` with null-coalescing when no rows match). `Query<decimal>` materializes a result set and `.First()` throws on empty sets; aggregates belong on the scalar path.
- **C — INSERT success:** Use **`Execute`** and compare `rowsAffected == 1`. `Query<int>` expects a `SELECT` result set mapping to `int`; a bare `INSERT` returns no rows to map — `result` is empty, so `inserted` is always `false` even when the insert succeeded.

**Production takeaway:** **`Execute`** = DML rows affected; **`ExecuteScalar<T>`** = one cell (COUNT, AVG, `SCOPE_IDENTITY()`); **`Query<T>`** = many mapped rows. Mixing them "because it compiles" is a common Karat trap.

---

#### Q5. (P) Your ASP.NET Core API uses scoped `SqlConnection` per request. A repository method mirrors the tutorial's async list load but returns `IEnumerable<Product>` directly from `QueryAsync`. Under load, callers intermittently see `InvalidOperationException` ("There is already an open DataReader…"). Explain the mechanism and show the production-safe pattern (including when to materialize vs stream).

**Answer:** `QueryAsync` returns a deferred sequence backed by an open reader on the shared scoped connection; if anything else uses that connection (second query, retry, logging) before enumeration finishes, SQL Server rejects overlapping readers on one connection.

- **Mechanism:** Default MARS off on many connection strings — one active reader per connection. Returning deferred `IEnumerable` from a repository exports the reader lifetime to unknown callers.
- **Production-safe (typical API list):** Materialize before returning, matching the tutorial's `GetAllActiveAsync`:

```csharp
public async Task<IReadOnlyList<Product>> GetAllActiveAsync(SqlConnection connection)
{
    const string sql = "...";
    IEnumerable<Product> rows = await connection.QueryAsync<Product>(sql);
    return rows.ToList(); // reader closed before caller runs another command
}
```

- **When to stream:** Use `buffered: false` or `IAsyncEnumerable<Product>` only when the **same** code path owns the connection until enumeration completes (export job, manual `await foreach`), not when returning bare `IEnumerable` to controllers or other services.
- **Alternative:** Separate connection for the streaming read, or enable MARS deliberately — still prefer explicit materialization for small/medium result sets.

**Production takeaway:** Treat `Query`/`QueryAsync` return values like ADO.NET readers — either finish reading inside the repository or hand back a fully materialized collection.

---

#### Q6. (D) The tutorial's `InsertProductReturningId` uses `MAX(ProductId) + 1` inside a batch and returns the new id via `ExecuteScalar<int>`. A teammate says "works in dev, ship it." Two API instances insert products concurrently under load. What breaks, and what pattern replaces both the id generation **and** the Dapper call sequence?

**Answer:** Concurrent transactions can compute the same `@NextId`, causing primary-key violations or lost updates — `ExecuteScalar` is fine for returning an id, but **`MAX + 1` is not a safe id strategy** under concurrency.

- **What breaks:** Two requests read the same `MAX(ProductId)` before either commits → duplicate `ProductId` insert → one request fails with PK violation, or worse behavior under weaker isolation.
- **Replace id generation:** Use an **`IDENTITY`** (or **`SEQUENCE`**) column on `ProductId` and let SQL Server allocate ids — remove manual `MAX + 1`.
- **Replace Dapper sequence:** Single batch with **`ExecuteScalar<int>`**:

```csharp
const string sql = """
    INSERT INTO dbo.Products (ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
    VALUES (@productName, @unitPrice, @stockQuantity, NULL);
    SELECT CAST(SCOPE_IDENTITY() AS INT);
    """;
int newId = await connection.ExecuteScalarAsync<int>(sql, param);
```

- Prefer **`SCOPE_IDENTITY()`** over `@@IDENTITY` (trigger-safe in scope). Wrap insert + follow-up DML in an explicit **`IDbTransaction`** passed to `Execute`/`ExecuteScalar` when multiple statements must commit together.

**Production takeaway:** Dapper does not fix application-level race conditions — **`ExecuteScalar` returns whatever your SQL makes atomic**; id generation must be delegated to the database or a serialized sequence.

---

#### Q7. (P) A batch job must insert a product **and** read the new `ProductId` in one round trip, then update a related audit row — all-or-nothing. The developer runs two separate Dapper calls on the same open connection without an explicit transaction. When does that silently corrupt data, and how do you wire `Execute` / `ExecuteScalar` with `IDbTransaction`?

**Answer:** Without `BeginTransaction`, each Dapper call auto-commits independently — if the audit `Execute` fails after the insert `ExecuteScalar` succeeds, you keep the product row but lose the audit trail (partial commit).

- **When it corrupts:** Any multi-step business operation that must be atomic — insert + audit update, transfer between tables, insert + inventory decrement — when the second call throws or the process crashes after the first succeeds.
- **Correct pattern:** One transaction, pass it to every Dapper call:

```csharp
await using var connection = new SqlConnection(_cs);
await connection.OpenAsync();
await using var tx = await connection.BeginTransactionAsync();

try
{
    int newId = await connection.ExecuteScalarAsync<int>(insertSql, param, transaction: tx);
    await connection.ExecuteAsync(auditSql, new { newId, userId }, transaction: tx);
    await tx.CommitAsync();
}
catch
{
    await tx.RollbackAsync();
    throw;
}
```

- **Dapper rule:** The `transaction` parameter on `Execute`, `ExecuteScalar`, and `Query` binds all commands to the same unit of work — same as ADO.NET `SqlTransaction`.
- **Single batch alternative:** Combine statements in one SQL batch with one `ExecuteScalar` return — still use an explicit transaction if other side effects (files, messages) must align.

**Production takeaway:** **`ExecuteScalar` after `INSERT` in one batch is one round trip; multiple Dapper calls are multiple commits unless you pass `IDbTransaction`** — Karat pairs Dapper API choice with transactional boundaries from ADO.NET.
