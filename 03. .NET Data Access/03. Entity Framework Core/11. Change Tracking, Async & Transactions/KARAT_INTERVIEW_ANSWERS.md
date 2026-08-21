# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/11. Change Tracking, Async & Transactions`

---

#### Q1. (R) A catalog endpoint loads products with `AsNoTracking()` for performance, then applies a "flash sale" discount in memory before calling `SaveChangesAsync`. QA passes on one row; production reports prices never change. Review this service method — what is wrong, and how do you fix it without abandoning no-tracking for the read path?

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

#### Q2. (P) Operations wants to purge inactive products older than two years — potentially tens of thousands of rows. A developer proposes loading each entity with `Find`, calling `Remove`, then one `SaveChangesAsync`. You suggest `ExecuteDeleteAsync` instead. When is `ExecuteDelete` the right tool, what does it skip compared to tracked delete, and what guardrails would you add before running it in production?

**Answer:** `ExecuteDeleteAsync` (EF Core 7+) translates the LINQ filter directly into a **single set-based DELETE** on the server — no entity materialization, no change-tracker snapshots, and no per-row `Remove` calls. Use it for bulk purges where you do not need per-row domain logic, interceptors on individual entities, or cascade behavior that requires loaded graphs.

- **When to use:** large filtered deletes (archival purge, soft-delete migration cleanup) where the predicate is expressible in SQL and side effects are acceptable at the SQL layer.
- **What it skips:** change tracker entirely — `SaveChangesInterceptor`, `Deleting`/`Deleted` entity events, and client-evaluated logic in the loop do not run per row; only database FK/cascade rules apply.
- **Tracked delete still wins when:** you must audit each row, enforce business rules per entity, trigger domain events, or delete related graphs that EF models with tracked cascades you need to inspect.
- **Guardrails:** run against a replica or `BEGIN TRAN` + `SELECT COUNT(*)` preview first; require a **soft-delete** flag in many domains instead of hard delete; add a `WHERE` cutoff with indexed columns; log rows affected from the return value; restrict to admin/batch role; consider batched deletes (`TOP` chunks) to avoid long locks.

**Production takeaway:** **ExecuteDelete is the EF equivalent of `DELETE FROM … WHERE`** — pair no-tracking reads with set-based writes when scale matters. Do not load 50k rows into memory to call `Remove`. Forward reference: ch05 CRUD for tracked `Remove` semantics.

---

#### Q3. (R) A stock transfer must update `InventoryDbContext` and write an audit row through `AuditDbContext` (separate DbContext types, same SQL Server database). A teammate starts a transaction on each context independently. Review the orchestration — what breaks atomicity, and how do you coordinate a single commit across both contexts?

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

#### Q4. (R) Under load, the inventory API thread pool queues grow and requests time out. Review this controller action — identify async anti-patterns and what you would change for "async all the way" through EF Core.

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

#### Q5. (R) `Product` now has a SQL Server `rowversion` concurrency token. Two editors save conflicting prices; the second save throws `DbUpdateConcurrencyException`. Review this catch block from the API layer — what is wrong with the recovery strategy, and what should happen before returning a response to the client?

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

#### Q6. (P) A nightly job bulk-updates `StockQuantity` for every warehouse row matching a filter. Compare these two approaches — tracked load + modify + `SaveChangesAsync` vs `ExecuteUpdateAsync` — and state which you would ship for 50k rows, including concurrency and observability trade-offs.

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
