# 11. Change Tracking, Async & Transactions — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q72. What is change tracking in EF Core?](#q72-what-is-change-tracking-in-ef-core)
- [Q73. What entity states does EF Core track (`Added`, `Modified`, `Deleted`, `Unchanged`)?](#q73-what-entity-states-does-ef-core-track-added-modified-deleted-unchanged)
- [Q74. What does `AsNoTracking` do, and when should you use it?](#q74-what-does-asnotracking-do-and-when-should-you-use-it)
- [Q75. What is the performance impact of change tracking on read-heavy queries?](#q75-what-is-the-performance-impact-of-change-tracking-on-read-heavy-queries)
- [Q76. Why use async EF Core methods in ASP.NET Core?](#q76-why-use-async-ef-core-methods-in-aspnet-core)
- [Q77. How do you begin and commit a transaction in EF Core?](#q77-how-do-you-begin-and-commit-a-transaction-in-ef-core)
- [Q78. How does EF Core detect concurrency conflicts?](#q78-how-does-ef-core-detect-concurrency-conflicts)
- [Q79. What is a concurrency token or row version column?](#q79-what-is-a-concurrency-token-or-row-version-column)
- [Q80. How do you handle `DbUpdateConcurrencyException`?](#q80-how-do-you-handle-dbupdateconcurrencyexception)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 11. Change Tracking, Async & Transactions

---

## Q72. What is change tracking in EF Core?

**Concepts**
- original value snapshot per tracked entity
- EntityState assignment by change tracker
- INSERT/UPDATE/DELETE generation from state
- AsNoTracking opt-out for read-only

**Answer**

Change tracking is EF Core's mechanism for snapshotting entity state when loaded or attached, detecting modifications, and generating INSERT, UPDATE, and DELETE statements on `SaveChanges`. The `DbContext` maintains an entry per tracked entity with current and original values. Enabled by default for queries that return entity types without `AsNoTracking()`; The change tracker compares current property values to snapshots taken at query or attach time. Relationships and FK changes are tracked alongside scalar properties. Disabling tracking (`AsNoTracking`) skips snapshot overhead for read-only scenarios.

---

## Q73. What entity states does EF Core track (`Added`, `Modified`, `Deleted`, `Unchanged`)?

**Concepts**
- Added for INSERT on save
- Modified for UPDATE on save
- Deleted for DELETE on save
- Unchanged skipped and Detached outside tracker

**Answer**

EF Core assigns each tracked entity an `EntityState` describing what `SaveChanges` should do: `Added` inserts new rows, `Modified` updates changed rows, `Deleted` removes rows, and `Unchanged` skips entities with no detected changes. `Detached` means the entity is not in the tracker. `Add()` marks entities `Added` — INSERT on save even if they had a key value set manually; `Update()` marks all mapped properties `Modified` unless configured otherwise — issues a broad UPDATE. `Remove()` marks `Deleted` — DELETE on save; cascade rules apply to dependents. `Unchanged` entities are loaded but untouched; `Attach()` with unchanged values sets `Unchanged` until properties change.

---

## Q74. What does `AsNoTracking` do, and when should you use it?

**Concepts**
- snapshot overhead elimination for read-only
- AsNoTrackingWithIdentityResolution variant
- global QueryTrackingBehavior.NoTracking setting
- tracked queries required for write path

**Answer**

`AsNoTracking()` tells EF Core not to snapshot or track entities returned by a query, reducing memory and CPU for read-only operations. Use it on list endpoints, reports, and any query whose results will not be updated through the same `DbContext` instance. Tracked queries store original values for every property — unnecessary when you only serialize to JSON; `AsNoTrackingWithIdentityResolution()` deduplicates repeated instances in a single result without full change tracking. Set globally: `options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` for read-heavy apps. Omit `AsNoTracking` when you load entities specifically to modify and call `SaveChanges` on the same context.

---

## Q75. What is the performance impact of change tracking on read-heavy queries?

**Concepts**
- O(n) snapshot memory per entity
- identity resolution and fix-up overhead
- Gen2 GC pressure on large read lists
- DTO projection as superior alternative

**Answer**

Tracking adds memory for original/current value snapshots and CPU for fix-up and change detection on every materialized entity. On large read-only lists, this overhead can significantly increase Gen2 pressure and request latency compared to equivalent `AsNoTracking` queries. A 10,000-row GET that tracks entities holds two value sets per property per row in the worst case; Identity resolution and relationship fix-up add cost beyond flat property snapshots. Projection to DTOs avoids both tracking and loading unused columns — often the best read-path optimization. The regression is silent until load testing — results are identical, only resource use differs.

---

## Q76. Why use async EF Core methods in ASP.NET Core?

**Concepts**
- I/O-bound database wait released to thread pool
- request thread scalability under concurrency
- CancellationToken from RequestAborted
- .Result and .Wait deadlock risk

**Answer**

Async methods (`ToListAsync`, `SaveChangesAsync`, `FirstOrDefaultAsync`) release the request thread during I/O waits, improving scalability under concurrent load. ASP.NET Core thread pool threads are a shared resource — blocking them on database calls reduces throughput. Database latency is I/O-bound; async avoids thread starvation during waits for SQL Server responses; Pair with `CancellationToken` from `HttpContext.RequestAborted` so client disconnects cancel long queries. Never block async EF calls with `.Result` or `.Wait()` — causes deadlocks and defeats scalability benefits. Sync methods remain acceptable in console tools or single-threaded batch jobs with no concurrency pressure.

---

## Q77. How do you begin and commit a transaction in EF Core?

**Concepts**
- BeginTransactionAsync on database facade
- CommitAsync and dispose-based rollback
- explicit vs implicit per-SaveChanges transaction
- multi-step atomicity across raw SQL and tracked changes

**Answer**

Start a transaction on the context's database facade with `BeginTransactionAsync`, perform tracked changes and raw SQL, then call `CommitAsync` — or `RollbackAsync` on failure. All operations on the same `DbContext` instance participate in the transaction until commit or rollback. `await using var tx = await context.Database.BeginTransactionAsync();`; Make changes, `await context.SaveChangesAsync();`, optionally run `ExecuteSqlRaw` on the same context. `await tx.CommitAsync();` — dispose rolls back if commit was not called. `SaveChanges` alone wraps each call in an implicit transaction for its own batch, not across multiple separate calls.

---

## Q78. How does EF Core detect concurrency conflicts?

**Concepts**
- WHERE clause original token comparison
- zero rows affected as conflict signal
- DbUpdateConcurrencyException thrown by SaveChanges
- optimistic concurrency model

**Answer**

EF Core compares concurrency token values in the UPDATE or DELETE WHERE clause against the values read when the entity was loaded. If zero rows match because another transaction changed the row first, EF Core throws `DbUpdateConcurrencyException` on `SaveChanges`. Configure a concurrency token property with `[Timestamp]` / `rowversion` or `.IsConcurrencyToken()` on a property; Generated SQL includes `WHERE Id = @id AND RowVersion = @originalRowVersion`. No token configured means last-write-wins — later saves overwrite earlier changes silently. Optimistic concurrency suits web apps where simultaneous edits are possible but locking is undesirable.

---

## Q79. What is a concurrency token or row version column?

**Concepts**
- rowversion column with Timestamp attribute
- auto-incremented by database on UPDATE
- original value in UPDATE and DELETE WHERE predicate
- manual Version integer as alternative

**Answer**

A concurrency token is a property mapped to a database column whose value changes whenever the row is updated, used by EF Core to detect stale writes. SQL Server `rowversion` (`[Timestamp]` in EF) is the common choice — the database auto-increments it on every update. Mark with `[Timestamp]` on a `byte[]` property or Fluent API `.IsRowVersion()`; EF Core reads the token at query time and includes the original value in UPDATE/DELETE predicates. Any concurrent modification changes the token, causing the next save to affect zero rows and throw. Application-defined tokens (for example a manual `Version` integer) also work if incremented on each update.

---

## Q80. How do you handle `DbUpdateConcurrencyException`?

**Concepts**
- exception.Entries inspection for conflicting entities
- ReloadAsync for server-wins resolution
- client-wins retry after reload
- HTTP 409 Conflict response for API callers

**Answer**

Catch `DbUpdateConcurrencyException`, inspect `exception.Entries` for conflicting entities, and resolve by refreshing from the database, merging user changes, or returning a conflict response to the client. Production APIs typically return HTTP 409 with a message asking the user to reload and retry. Reload: `await entry.ReloadAsync()` discards stale client values and re-reads current database state; Client wins: reapply intended values after reload and retry `SaveChanges` if business rules allow overwriting. Server wins: return 409 Conflict with current row data so the UI can show what changed. Log concurrency conflicts for monitoring — frequent conflicts may indicate UX or workflow design issues.

---

## Gotchas

---

## Gotcha 14. Tracking overhead on read-only queries

**Concepts**
- change tracking snapshot overhead on read-only queries
- AsNoTracking omission on GET endpoints
- global QueryTrackingBehavior.NoTracking setting

**Answer**

Omitting `AsNoTracking()` on large read-only lists makes EF Core snapshot every entity for change detection that will never run, wasting memory and CPU on GET endpoints. Tracking stores original and current values per property for each row materialized; ASP.NET Core read services should default to `AsNoTracking()` plus DTO projection. Global `QueryTrackingBehavior.NoTracking` with explicit tracking on command paths prevents accidental overhead.

---

## Gotcha 9. Scoped `DbContext` captured in a singleton

**Concepts**
- scoped DbContext captured in singleton field
- captive dependency anti-pattern
- IDbContextFactory for singleton database access

**Answer**

Registering a singleton service that holds a scoped `DbContext` creates a captive dependency — the context may be disposed while the singleton lives, or state leaks across HTTP requests. `DbContext` is scoped per request in ASP.NET Core — singletons must not store it in fields; Inject `IDbContextFactory<TContext>` into singletons when long-lived services need occasional database access. Symptoms include "Cannot access a disposed context" or cross-user data contamination in tracked entities.

---

## Gotcha 15. `SaveChanges` without a transaction for multi-step updates

**Concepts**
- independent SaveChanges commits without explicit transaction
- partial update on multi-step failure
- BeginTransactionAsync and CommitAsync wrapping

**Answer**

Multiple `SaveChanges` calls or separate database operations that must succeed together commit independently by default, allowing partial updates that leave data in an inconsistent state when a later step fails. Wrap related saves and raw SQL in `BeginTransactionAsync`/`CommitAsync` on one `DbContext`; Prefer one `SaveChanges` per unit of work when all changes are tracked together on the same context. Retry logic after failure cannot assume earlier steps rolled back unless they shared a transaction boundary.

---

## Scenario-Based Questions (Karat Format)

---

## Q156. (R) A catalog endpoint loads products with `AsNoTracking()` for performance, then applies a "flash sale" discount in memory before calling `SaveChangesAsync`. QA passes on one row; production reports prices never change. Review this service method — what is wrong, and how do you fix it without abandoning no-tracking for the read path?

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

**Concepts**
- AsNoTracking query then SaveChanges for same entity
- no-tracking entities not updated by SaveChanges
- tracked query required for write path
- read path vs write path tracking strategy

**Answer**

`AsNoTracking()` entities are not registered in the change tracker, so mutating `UnitPrice` in memory and calling `SaveChangesAsync` produces zero UPDATE statements — the same phantom-edit failure demonstrated in ChangeTrackingService.DemonstrateAsNoTrackingAsync. The read optimization is correct; the write path must use a separate, explicit update mechanism.

1. **Bulk server-side update (preferred at scale):** keep `AsNoTracking()` for reads; use `ExecuteUpdateAsync` for the discount — `SetProperty(p => p.UnitPrice, p => p.UnitPrice * (1 - discountPct))` filtered by `CategoryId` (Q6).
2. **Per-row tracked or attach:** for smaller sets, loop keys and either `Find` + modify tracked entity, or `Attach` + mark `UnitPrice` `IsModified` — matches **ProductUpdateService** Section 7 patterns.
3. Do not call `Update()` on full no-tracking entities if the query omitted columns — partial stubs with attach + `IsModified` avoid overwriting untouched columns.
4. Log or assert `SaveChangesAsync` row count; return failure when 0 rows expected.

---

## Q157. (P) Operations wants to purge inactive products older than two years — potentially tens of thousands of rows. A developer proposes loading each entity with `Find`, calling `Remove`, then one `SaveChangesAsync`. You suggest `ExecuteDeleteAsync` instead. When is `ExecuteDelete` the right tool, what does it skip compared to tracked delete, and what guardrails would you add before running it in production?

**Concepts**
- ExecuteDeleteAsync vs tracked load and Remove for bulk delete
- domain event and interceptor bypass in ExecuteDelete
- guardrails before production bulk delete
- set-based vs entity-based delete trade-off

**Answer**

`ExecuteDeleteAsync` (EF Core 7+) translates the LINQ filter directly into a single set-based DELETE on the server — no entity materialization, no change-tracker snapshots, and no per-row `Remove` calls. Use it for bulk purges where you do not need per-row domain logic, interceptors on individual entities, or cascade behavior that requires loaded graphs. - When to use: large filtered deletes (archival purge, soft-delete migration cleanup) where the predicate is expressible in SQL and side effects are acceptable at the SQL layer. - What it skips: change tracker entirely — `SaveChangesInterceptor`, `Deleting`/`Deleted` entity events, and client-evaluated logic in the loop do not run per row; only database FK/cascade rules apply. - Tracked delete still wins when: you must audit each row, enforce business rules per entity, trigger domain events, or delete related graphs that EF models with tracked cascades you need to inspect. - Guardrails: run against a replica or `BEGIN TRAN` + `SELECT COUNT(*)` preview first; require a soft-delete flag in many domains instead of hard delete; add a `WHERE` cutoff with indexed columns; log rows affected from the return value; restrict to admin/batch role; consider batched deletes (`TOP` chunks) to avoid long locks. Production takeaway: ExecuteDelete is the EF equivalent of `DELETE FROM … WHERE` — pair no-tracking reads with set-based writes when scale matters. Do not load 50k rows into memory to call `Remove`. Forward reference: ch05 CRUD for tracked `Remove` semantics.

---

## Q158. (R) A stock transfer must update `InventoryDbContext` and write an audit row through `AuditDbContext` (separate DbContext types, same SQL Server database). A teammate starts a transaction on each context independently. Review the orchestration — what breaks atomicity, and how do you coordinate a single commit across both contexts?

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

**Concepts**
- separate transactions on two DbContext types
- cross-context atomicity with shared SQL Server connection
- ambient transaction pattern for multi-context commit
- TransactionScope or manual connection sharing

**Answer**

Two independent `BeginTransactionAsync` calls create two separate database transactions on potentially different connections — committing `invTx` can succeed while `auditTx` fails (or vice versa), leaving inventory moved without an audit trail or an audit row without the stock change. This is not one atomic unit of work.

1. **`TransactionScope` + `RequiresNew`/`Required`:** wrap both saves in `using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);` — both contexts enlist in one ambient transaction when using the same database (MSDTC for cross-database/server).
2. **Share one connection:** open `DbConnection`, pass to both contexts via `UseSqlServer(connection)` / `context.Database.UseConnection(connection)`, call `BeginTransactionAsync` **once** on that connection, pass `IDbContextTransaction` or enlist both contexts — advanced but avoids MSDTC on same DB.
3. **Single DbContext** when inventory and audit share one database and bounded context — one `SaveChangesAsync` inside one `BeginTransactionAsync` (matches **InventoryTransactionService.TransferStockAsync** pattern in this chapter).
4. If audit is eventually consistent by design, document **outbox pattern** instead of pretending one EF transaction spans services.

---

## Q159. (R) Under load, the inventory API thread pool queues grow and requests time out. Review this controller action — identify async anti-patterns and what you would change for "async all the way" through EF Core.

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

**Concepts**
- async anti-patterns in controller action
- Result and Wait causing thread deadlock
- async all-the-way-up requirement
- ConfigureAwait(false) in library code

**Answer**

Both actions block async I/O — `.Result` and `GetAwaiter().GetResult()` on `Task`-returning EF methods sync-over-async, which can exhaust the ASP.NET thread pool under concurrency and cause deadlocks in contexts with a synchronization context. EF Core APIs like `ToListAsync` and `SaveChangesAsync` exist precisely to free threads during database waits.

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

---

## Q160. (R) `Product` now has a SQL Server `rowversion` concurrency token. Two editors save conflicting prices; the second save throws `DbUpdateConcurrencyException`. Review this catch block from the API layer — what is wrong with the recovery strategy, and what should happen before returning a response to the client?

```csharp
catch (DbUpdateConcurrencyException)
{
    await _context.SaveChangesAsync(ct); // retry same pending changes
    return Ok(updatedDto);
}
```

**Concepts**
- DbUpdateConcurrencyException catch and rethrow without resolution
- stale entry not refreshed before retry
- ReloadAsync for current database values
- HTTP 409 response with current row data

**Answer**

Blindly calling `SaveChangesAsync` again retries the same stale values against a row whose token already changed — the second save will throw again (or worse, loop). The handler never reloads current database state, never merges the user's intent with fresh data, and returns 200 OK as if the conflict was resolved.

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

---

## Q161. (P) A nightly job bulk-updates `StockQuantity` for every warehouse row matching a filter. Compare these two approaches — tracked load + modify + `SaveChangesAsync` vs `ExecuteUpdateAsync` — and state which you would ship for 50k rows, including concurrency and observability trade-offs.

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

**Concepts**
- tracked load and modify vs ExecuteUpdateAsync for 50k rows
- O(n) snapshot memory vs single UPDATE statement
- concurrency token behavior per approach
- bulk update observability and domain logic trade-off

**Answer**

For 50k rows, ship Approach B (`ExecuteUpdateAsync`) — one round trip, no change-tracker memory for 50k snapshots, and predictable duration. Approach A materializes every entity, holds original values for concurrency checks, and generates a large batched UPDATE (or many statements) through the tracker — workable in hundreds of rows, fragile at tens of thousands. | Dimension | Approach A — tracked load + save | Approach B — `ExecuteUpdateAsync` | |---|---|---| | Memory | O(n) entities + snapshots | O(1) — no materialization | | Round trips | 1 SELECT + 1 SAVE (large batch) | 1 UPDATE statement | | Change tracker | Participates; interceptors fire on save | Bypasses tracker for the update | | Concurrency tokens | Per-row `rowversion` in WHERE if configured | Token included in SET/WHERE if property mapped — still optimistic at SQL level | | Partial failure | `SaveChanges` batch semantics | Single statement — all matching rows updated atomically | | Observability | Can log each entity in interceptor | Log SQL + rows affected return value only | | Domain logic per row | Possible in loop | Not possible — predicate must be pure SQL | - When A still wins: each row needs different computed adjustment from loaded navigation data, or you must raise domain events per product. - Concurrency note: if `Product` has `RowVersion`, Approach A detects conflicts per entity; Approach B updates all matching rows in one statement — concurrent edits to individual rows may throw or skip depending on provider SQL; for high contention, batch by key ranges or use explicit locking for the job window. - Observability: log `rowsAffected` from `ExecuteUpdateAsync`; add a job audit row via separate `SaveChanges` in the same explicit transaction if needed (InventoryTransactionService pattern). Production takeaway: Match read model (`AsNoTracking` catalog) and write model (set-based `ExecuteUpdate` / `ExecuteDelete`) to workload shape — tracking 50k entities for a scalar increment is a production incident waiting in QA. See ChangeTrackingService.GetProductCatalogAsync for read side; use Execute* for bulk writes.
