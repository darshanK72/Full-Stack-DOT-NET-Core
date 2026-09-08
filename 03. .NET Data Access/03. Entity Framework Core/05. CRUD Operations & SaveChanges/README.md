# 05. CRUD Operations & SaveChanges — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q31. How do you insert, update, and delete entities with EF Core?](#q31-how-do-you-insert-update-and-delete-entities-with-ef-core)
- [Q32. What does `SaveChanges()` do?](#q32-what-does-savechanges-do)
- [Q33. What is the difference between `Add`, `Update`, and `Remove` on a `DbSet`?](#q33-what-is-the-difference-between-add-update-and-remove-on-a-dbset)
- [Q34. What is attach versus add when working with disconnected entities?](#q34-what-is-attach-versus-add-when-working-with-disconnected-entities)
- [Q35. What is `ExecuteUpdate` / `ExecuteDelete` in EF Core 7+?](#q35-what-is-executeupdate-executedelete-in-ef-core-7)
- [Q36. What is the unit-of-work pattern in relation to `DbContext`?](#q36-what-is-the-unit-of-work-pattern-in-relation-to-dbcontext)
- [Q37. How do you perform a bulk update without loading entities into memory?](#q37-how-do-you-perform-a-bulk-update-without-loading-entities-into-memory)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 05. CRUD Operations & SaveChanges

---

## Q31. How do you insert, update, and delete entities with EF Core?

**Concepts**
- Add/Update/Remove state marking on DbSet
- SaveChangesAsync single-transaction commit
- identity key population after save
- disconnected entity reattachment

**Answer**

Add new entities with `context.Set<T>().Add(entity)` or `AddRange`, mark updates by modifying tracked entities or calling `Update`, remove with `Remove` or `RemoveRange`, then persist with `await context.SaveChangesAsync()`. EF Core generates insert, update, and delete statements from change tracker state. Insert: create object, `Add`, `SaveChanges` — identity keys populate after save when configured as store-generated; Update: query entity, mutate properties, `SaveChanges` — only changed columns appear in `UPDATE` when tracking detects modifications. Delete: `Remove(entity)` marks `Deleted`; `SaveChanges` issues `DELETE`. Disconnected updates from APIs attach or use `Update` with caution to avoid overwriting unchanged columns.

---

## Q32. What does `SaveChanges()` do?

**Concepts**
- single database transaction per call
- dependency-ordered SQL generation
- store-generated value refresh after save
- SavingChanges and SavedChanges events

**Answer**

`SaveChanges` (and `SaveChangesAsync`) commits all pending changes tracked by the context in a single database transaction by default. It generates SQL for added, modified, and deleted entities, executes it in dependency order, and updates store-generated values such as identity columns and row versions. Returns the number of state entries written to the database; Detects relationship changes and orders inserts to satisfy foreign key constraints. Raises `SavingChanges` and `SavedChanges` events and runs interceptors before and after persistence. If any statement fails, the transaction rolls back and no partial commit occurs within that `SaveChanges` call.

---

## Q33. What is the difference between `Add`, `Update`, and `Remove` on a `DbSet`?

**Concepts**
- Added state for insert
- Modified state for update
- Deleted state for delete
- full vs partial update trade-off with Update vs Attach

**Answer**

`Add` marks an entity as `Added` for insert on save. `Update` marks every mapped property as `Modified` for a full update (or attaches disconnected graphs as modified). `Remove` marks the entity as `Deleted` for delete on save. `Add` on an entity with an existing key value may throw or behave as update depending on key configuration — typically use `Add` only for new keys; `Update` is convenient for disconnected Data Transfer Objects from HTTP PUT but can overwrite columns not sent in the payload. `Remove` requires the entity to be known to the context — attach first if it came from the client with only an id. `Attach` plus setting `EntityState` manually offers finer control than blanket `Update`.

---

## Q34. What is attach versus add when working with disconnected entities?

**Concepts**
- Attach registers existing entity as Unchanged
- Add marks entity for INSERT
- IsModified for targeted column update
- Update shorthand for full disconnected update

**Answer**

`Add` tells EF Core the entity is new and should be inserted. `Attach` registers an existing entity with the context as `Unchanged` without inserting — you then mark specific properties or the whole entity `Modified` for targeted updates. Web APIs often receive ids from clients — `Attach` plus `Modified` state updates without a prior query; `Add` on an entity with a non-zero key may attempt insert and violate primary key constraints. `context.Entry(entity).Property(e => e.Name).IsModified = true` updates one column after attach. `Update` is shorthand for attach-all-properties-as-modified on disconnected instances.

---

## Q35. What is `ExecuteUpdate` / `ExecuteDelete` in EF Core 7+?

**Concepts**
- set-based SQL UPDATE and DELETE without loading
- change tracker bypass for performance
- no domain logic per row
- EF Core 7+ API shape

**Answer**

`ExecuteUpdate` and `ExecuteDelete` translate LINQ filters directly into SQL `UPDATE` and `DELETE` statements without loading entities into memory or running change tracking. EF Core 8 continues these APIs for high-performance bulk mutations on `IQueryable`. Example: `await context.Products.Where(p => p.Discontinued).ExecuteDeleteAsync()`; Example: `await context.Products.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, newPrice))`. Bypasses the change tracker — no entity instances materialized, lower memory and CPU. Does not run domain logic on entities, cascade through application-level validators, or update navigation fix-up in memory.

---

## Q36. What is the unit-of-work pattern in relation to `DbContext`?

**Concepts**
- atomic persistence boundary across operations
- single SaveChanges per unit of work
- repositories sharing one context instance
- rollback on failure

**Answer**

The unit-of-work pattern groups multiple repository operations into one atomic persistence boundary. In EF Core, `DbContext` implements this naturally — all tracked changes across `DbSet` operations commit together in one `SaveChanges` call inside one transaction. One request handler accumulates adds, updates, and deletes then calls `SaveChangesAsync` once; Multiple `SaveChanges` calls in one context create separate transactions unless wrapped in an explicit transaction. Repositories sharing one injected context participate in the same unit of work automatically. Failure during `SaveChanges` rolls back the entire batch, preserving consistency across related tables.

---

## Q37. How do you perform a bulk update without loading entities into memory?

**Concepts**
- ExecuteUpdateAsync for set-based bulk update
- raw SQL as fallback for complex mutations
- no interceptor events per row
- keyset pagination for large batches

**Answer**

Use `ExecuteUpdateAsync` (EF Core 7+) with a filtered `IQueryable` to push updates to the database in one SQL statement. For very large batches or provider-specific optimizations, raw SQL via `ExecuteSqlRawAsync` or third-party bulk extension libraries remain options. `ExecuteUpdateAsync` with `SetProperty` sets column values in SQL without selecting rows into the context; Ideal for flag updates, soft deletes, and price adjustments across many matching rows. Does not invoke interceptors or tracked-entity events per row the way individual updates would. When complex per-row logic is required in C#, batch with smaller queries or use keyset pagination instead of loading entire tables.

---

## Gotchas — CRUD Operations & SaveChanges (Interview Traps)

---

#### Gotcha 1. Entity not tracked — `SaveChanges` produces no SQL for the modification

**Concepts**
- untracked entity modified and passed to `SaveChanges` — no UPDATE generated
- `AsNoTracking()` entities are detached — changes not detected
- stub entity `new Product { Id = 1, Price = 9.99m }` not tracked by context
- `context.Update(entity)` marks all properties dirty for untracked entity
- `Find(id)` or `FirstOrDefaultAsync(id)` to load and track before modify

**Answer**

When an entity is modified but is not tracked by the `DbContext` — because it was loaded with `AsNoTracking()`, received from a client request, or constructed with `new` — calling `SaveChanges` generates no SQL and the change is silently lost. To update such an entity, either load it first with `Find`/`FirstOrDefaultAsync` (which returns a tracked instance), mutate properties, and call `SaveChanges`; or call `context.Update(entity)` to attach and mark all properties dirty. The former generates a targeted UPDATE; the latter generates an UPDATE for all columns.

---

#### Gotcha 2. `Remove()` on a detached entity — `InvalidOperationException`

**Concepts**
- `context.Remove(entity)` requires entity to be tracked or attached
- detached entity passed to `Remove` throws `InvalidOperationException`
- `context.Entry(entity).State = EntityState.Deleted` for detached delete
- `Find(id)` to load tracked instance before removal
- stub entity with only key populated: attach then mark deleted

**Answer**

`context.Remove(entity)` requires the entity to be tracked by the current context. Passing a detached entity (one loaded in a different context scope, received from a client, or constructed with `new`) throws `InvalidOperationException`. For detached entities where you have only the key value, use `context.Entry(new Product { Id = id }).State = EntityState.Deleted` to attach a stub and mark it for deletion without loading the full row. EF Core 7+ `ExecuteDeleteAsync` is more efficient for key-based deletes without loading the entity at all.

---

#### Gotcha 3. `SaveChanges` commits all tracked changes — unintended entities included

**Concepts**
- all `Added`/`Modified`/`Deleted` tracked entities flushed in one `SaveChanges`
- unrelated entity modified earlier in the request scope committed together
- one context per unit-of-work to avoid accidental multi-entity commits
- `ChangeTracker.Clear()` to discard unintended pending changes
- EF Core 7+ `ExecuteUpdate`/`ExecuteDelete` bypass change tracker entirely

**Answer**

`SaveChanges` commits every pending change tracked by the `DbContext` in a single transaction — not just the entity the caller intended to save. If a service method earlier in the request scope modified a `Category` entity and the current method calls `SaveChanges` to persist a `Product` change, the `Category` modification is also committed. Design each service method to operate on a focused scope, use `context.ChangeTracker.Clear()` to discard unintended pending state, or use EF Core 7+ `ExecuteUpdateAsync` for targeted single-entity updates that bypass the change tracker.

---

#### Gotcha 4. `DbUpdateConcurrencyException` not caught — silent lost update on optimistic concurrency conflict

**Concepts**
- optimistic concurrency via `[Timestamp]` or `ConcurrencyCheck`
- `DbUpdateConcurrencyException` on 0-rows-affected UPDATE
- uncaught exception propagates as 500 — no business-level conflict handling
- catch and return HTTP 409 Conflict with current database values
- `entry.GetDatabaseValues()` to reload the winning values for merge

**Answer**

When optimistic concurrency is configured with `[Timestamp]` or `[ConcurrencyCheck]`, EF Core throws `DbUpdateConcurrencyException` if a concurrent write modified the row between your read and your `SaveChanges`. If this exception is not caught, it propagates as an unhandled 500 error rather than a meaningful "data has been modified, please reload" response. Catch `DbUpdateConcurrencyException` at the service layer, reload the current row with `entry.GetDatabaseValues()`, and either merge changes or return HTTP 409 Conflict to the client.

---

#### Gotcha 5. Multiple `SaveChanges` calls in one handler — not a single unit of work

**Concepts**
- each `SaveChanges` is a separate database commit
- second `SaveChanges` can fail after first already committed
- partial state persisted with no rollback path
- one `SaveChanges` per unit of work as the design principle
- explicit `BeginTransactionAsync` + `CommitAsync` for cross-`SaveChanges` atomicity

**Answer**

Calling `SaveChanges` twice in a single handler creates two separate database commits — if the second call fails, the first commit is already durable and there is no automatic rollback. The correct design is to accumulate all changes on the tracked context and call `SaveChanges` once at the end. When two logically separate operations must be atomic together, wrap both in an explicit transaction with `await context.Database.BeginTransactionAsync()` and commit after all operations succeed.

---

#### Gotcha 6. `Find()` returns stale tracked entity — bypasses fresh database read

**Concepts**
- `Find(key)` checks `ChangeTracker.Local` cache before querying the database
- returns the tracked instance if already loaded — may have stale property values
- concurrent external update not reflected in returned `Find` result
- `FirstOrDefaultAsync` with `AsNoTracking()` for fresh read
- `entry.ReloadAsync()` to force a refresh of a tracked entity

**Answer**

`context.Products.Find(id)` looks up the identity map (tracked entities in `Local`) first and returns the already-tracked instance without hitting the database if the entity was previously loaded. If a concurrent external update modified the row after the initial load, `Find` returns the stale in-memory version. When freshness is required (e.g., before a write that depends on current values), use `await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id)` for a fresh database read, or call `await context.Entry(tracked).ReloadAsync()` to refresh a known tracked entity.

---

#### Gotcha 7. `context.Entry(entity).State = EntityState.Modified` marks all columns dirty

**Concepts**
- `EntityState.Modified` marks every property as changed
- generated UPDATE includes all columns, not just modified ones
- overwriting unchanged columns risks lost-update under concurrent writes
- `context.Update(entity)` is equivalent — same all-column behavior
- load entity and mutate specific properties for targeted UPDATE

**Answer**

Setting `context.Entry(entity).State = EntityState.Modified` (or calling `context.Update(entity)`) marks every property as modified, causing the generated `UPDATE` to set all columns. Under concurrent access, this can overwrite a column modified by another process between your read and write, even if your code did not intend to change that column. Load the entity from the database first, modify only the specific properties that changed, and call `SaveChanges` — EF Core's change tracking generates a minimal UPDATE that sets only the actually-changed columns.

---

#### Gotcha 8. Bulk operations with `SaveChanges` on large tracked entity sets — N individual SQL statements

**Concepts**
- EF Core batches inserts/updates/deletes (EF Core 7+ multi-row batching)
- `MaxBatchSize` limit and individual statement fallback for very large sets
- EF Core 7+ `ExecuteUpdateAsync`/`ExecuteDeleteAsync` for set-based bulk operations
- third-party `EFCore.BulkExtensions` for `SqlBulkCopy`-based bulk inserts
- tracking overhead for thousands of entities before `SaveChanges`

**Answer**

Adding thousands of entities to `DbContext` and calling `SaveChanges` generates individual or batched INSERT statements — EF Core 7+ batches up to `MaxBatchSize` rows per statement, but for very large sets this still involves many round-trips and significant change-tracking memory overhead. For bulk operations, use EF Core 7+ `ExecuteUpdateAsync`/`ExecuteDeleteAsync` for set-based updates and deletes, `EFCore.BulkExtensions` for bulk inserts using `SqlBulkCopy`, or raw ADO.NET `SqlBulkCopy` for maximum throughput. Avoid tracking thousands of entities in one `SaveChanges` call.

---

#### Gotcha 9. `SaveChanges` return value ignored — 0 rows affected goes undetected

**Concepts**
- `SaveChanges()` returns `int` — number of state entries written
- 0 returned when no tracked changes exist or all changes produce 0-row commands
- optimistic concurrency exception vs 0 rows affected distinction
- discarding the return value hides no-op saves silently
- assertion or logging on `SaveChanges` return value for critical writes

**Answer**

`SaveChanges()` returns the number of state entries written to the database — a return value of 0 means no changes were actually committed. This happens when the context has no pending changes or when an UPDATE matched no rows (which does not throw unless optimistic concurrency is configured). Ignoring the return value means a no-op save (perhaps because the entity was never tracked) goes undetected. For critical writes, log or assert on `var count = context.SaveChanges(); if (count == 0) Log.Warning("SaveChanges wrote nothing")` to surface silent non-operations.

---

#### Gotcha 10. Soft delete not configured in global query filter — deleted entities visible to all queries

**Concepts**
- soft-delete pattern: `IsDeleted` flag instead of physical row removal
- without global query filter, `context.Products.ToList()` includes deleted rows
- `HasQueryFilter(e => !e.IsDeleted)` in `OnModelCreating`
- `IgnoreQueryFilters()` to explicitly include soft-deleted rows when needed
- filter applies to all queries including `Include` navigation loads

**Answer**

The soft-delete pattern marks entities with `IsDeleted = true` instead of physically removing them, but without a global query filter every query returns deleted rows alongside active ones. Configure `modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted)` in `OnModelCreating` to exclude deleted rows from all queries automatically. Use `context.Products.IgnoreQueryFilters().Where(...)` when you explicitly need to query deleted records (e.g., admin recovery UI). The global filter also applies when EF Core loads navigation properties via `Include`, so soft-deleted related entities are filtered from collections automatically.

---

## Scenario-Based Questions (Karat Format)

---

## Q120. (R) An API endpoint maps a JSON body to `Product`, assigns `ProductId` from the route, changes `UnitPrice`, and calls `SaveChanges`. The response is 200 but the price in SQL is unchanged. Review this service method — what is wrong and how do you fix it?

```csharp
public void PatchPrice(AppDbContext context, int productId, decimal newPrice)
{
    var stub = new Product
    {
        ProductId = productId,
        UnitPrice = newPrice
    };
    // stub built from DTO — not loaded from context
    context.SaveChanges();
}
```

**Concepts**
- disconnected entity update without tracking
- Save response 200 but SQL unchanged
- Attach and Modify vs tracked query pattern
- missing context.Entry state assignment

**Answer**

The stub is detached — `SaveChanges` only executes SQL for entities the change tracker knows about in `Added`, `Modified`, or `Deleted` state. Building a `Product` in memory and never calling `Update()`, `Attach()`, or loading it with `Find` leaves nothing pending, so zero rows are written even though the method returns successfully.

1. **Preferred:** Load tracked entity — `var product = context.Products.Find(productId); product.UnitPrice = newPrice;` then `SaveChanges()` (matches `TryUpdateUnitPriceTracked` in **ProductService.cs**).
2. **When stub is required:** `context.Products.Update(stub)` or `context.Entry(stub).Property(p => p.UnitPrice).IsModified = true` after attach — only if no other instance with the same key is already tracked.
3. Check `SaveChanges()` return value or verify row state in tests — do not assume flush happened.
4. Return 404 when `Find` returns null instead of no-op save.

---

## Q121. (R) A teammate "fixes" detached updates by always calling `DbSet.Update()`. Partial PATCH requests now wipe columns the client did not send. Review this controller helper — what breaks, and what pattern fixes it without overwriting untouched fields?

```csharp
public async Task UpdateFromDto(AppDbContext context, ProductDto dto)
{
    var entity = new Product
    {
        ProductId = dto.Id,
        ProductName = dto.Name,
        UnitPrice = dto.Price
        // StockQuantity and IsDiscontinued not in DTO — default to 0 / false
    };
    context.Products.Update(entity);
    await context.SaveChangesAsync();
}
```

**Concepts**
- DbSet.Update overwriting partial PATCH payload
- full column update on disconnected stub
- property-level IsModified for targeted update
- PATCH semantic vs PUT semantic difference

**Answer**

`DbSet.Update()` is equivalent to Attach + mark entire entity Modified, so every mapped property is included in the UPDATE statement. Unset properties (`StockQuantity = 0`, `IsDiscontinued = false`) overwrite existing database values — a classic partial-update bug.

1. Load the tracked entity with `Find` or a query, mutate only DTO-supplied properties, then `SaveChanges()` — change tracker emits UPDATE only for changed columns (default).
2. For detached graphs: attach then mark individual properties — `context.Attach(entity); context.Entry(entity).Property(p => p.UnitPrice).IsModified = true;` (or `Entry(...).CurrentValues.SetValues(dto)` with explicit field list).
3. Use a dedicated PATCH model with nullable fields; apply only non-null members to the tracked entity.
4. Reserve `Update(entity)` for full replacement scenarios (PUT) where every column is intentionally supplied.

---

## Q122. (M) This code mirrors **Program.cs Section 5b** but skips `ChangeTracker.Clear()`. What exception or silent failure do you expect at runtime, and what are two safe ways to update a detached stub when another instance might already be tracked?

```csharp
public void RenameProduct(AppDbContext context, int id, string newName)
{
    Product? tracked = context.Products.Find(id);
    tracked!.ProductName = "stale name from earlier step"; // still Modified in tracker

    var stub = new Product { ProductId = id, ProductName = newName, UnitPrice = 9.99m };
    context.Products.Update(stub);
    context.SaveChanges();
}
```

**Concepts**
- ChangeTracker.Clear omission causing duplicate tracking
- InvalidOperationException on second attach
- detached stub update with already-tracked entity
- context.Entry state manipulation pattern

**Answer**

EF Core allows only one tracked instance per key per context. Calling `Update(stub)` while `tracked` is still in the change tracker throws `InvalidOperationException` — "The instance of entity type 'Product' cannot be tracked because another instance with the same key value is already being tracked."

1. **Best:** Reuse the tracked instance — mutate `tracked.ProductName = newName` and drop the stub entirely (no second instance).
2. Detach before attach: `context.Entry(tracked).State = EntityState.Detached` or `context.ChangeTracker.Clear()` (as in **Program.cs** line 118) then `Update(stub)` — only when you intentionally discard prior pending changes on that context.
3. Use a **new short-lived DbContext** per operation in web apps (scoped DI) so stale tracked entities from earlier steps in the same request are less common — still prefer single-instance update.
4. Never call `Update()` on a stub when you already have the entity from `Find` in the same method.

---

## Q123. (P) A catalog import reads 10,000 CSV rows and persists each with `Add` + `SaveChanges` inside the loop. It works on 50 rows in QA but times out in production. What is wrong with the persistence pattern, and what would you change?

```csharp
foreach (var row in csvRows)
{
    context.Products.Add(MapRow(row));
    context.SaveChanges(); // one round trip per row
}
```

**Concepts**
- N per-row SaveChanges inside import loop
- Add plus SaveChanges per row pattern
- batch insert with AddRange and single SaveChanges
- SaveChanges batching and ChangeTracker.Clear for large sets

**Answer**

Each `SaveChanges()` opens a transaction, generates INSERT SQL, waits for the database, and resets change-tracker state — 10,000 round trips instead of one batched unit of work. Throughput collapses under network latency, log growth, and lock duration even though the logic is "correct."

1. Batch: `Add` / `AddRange` many entities, then **one** `SaveChanges()` per chunk (e.g. 500–1000 rows) — matches **Program.cs** Section 7 batch pattern.
2. For very large loads, consider `BulkInsert` extensions or raw SQL/`SqlBulkCopy` when EF change tracking is unnecessary.
3. Wrap each chunk in an explicit transaction if all-or-nothing per batch is required.
4. Use `SaveChangesAsync` with cancellation for API-hosted imports.

---

## Q124. (P) Two admins edit the same product. Admin A loads price 10.00, Admin B loads price 10.00; B saves 12.00, then A saves 11.00. No error is thrown. The `Product` entity has no concurrency column. What data-loss scenario is this, and how would you add optimistic concurrency in EF Core?

**Concepts**
- last-write-wins without concurrency column
- optimistic concurrency token missing
- rowversion Timestamp attribute configuration
- DbUpdateConcurrencyException handling

**Answer**

This is last-write-wins — EF's default UPDATE uses only the primary key in the WHERE clause, so Admin A's save overwrites B's 12.00 with 11.00 without detecting that the row changed in between. No concurrency token means no `DbUpdateConcurrencyException` and no signal to retry or merge.

1. Add a concurrency property — e.g. `public byte[] RowVersion { get; set; }` with `[Timestamp]` or fluent `IsRowVersion()`, or a dedicated `uint Version` with `[ConcurrencyCheck]`.
2. On `SaveChanges`, EF includes the token in the WHERE clause; mismatch throws **`DbUpdateConcurrencyException`** — catch, reload, merge or return 409 Conflict to the client.
3. Expose current token to clients (ETag header or DTO field) so updates send **If-Match** semantics.
4. For high-contention fields, combine tokens with domain rules (minimum price floors) validated before save (Q6).

---

## Q125. (R) A bulk pricing endpoint accepts client-supplied `UnitPrice` and `StockQuantity`, calls `AddProduct` + `SaveChanges`, and returns the new `ProductId`. Negative prices and zero stock slip through to SQL. Review this flow — what is missing before `SaveChanges`, and where should validation live in a real API?

```csharp
public Product AddProduct(string name, decimal unitPrice, int stockQuantity)
{
    var product = new Product
    {
        ProductName = name,
        UnitPrice = unitPrice,
        StockQuantity = stockQuantity
    };
    _context.Products.Add(product);
    return product;
}

// caller
var created = service.AddProduct(dto.Name, dto.UnitPrice, dto.StockQuantity);
service.SaveChanges();
return Ok(created.ProductId);
```

**Concepts**
- negative price and zero stock persisted without validation
- SaveChanges without domain validation
- FluentValidation or guard clause before save
- validation layer placement in API pipeline

**Answer**

EF Core persists whatever the change tracker holds — it does not validate business rules. `AddProduct` only constructs an entity and sets `Added`; without guards, invalid scalars become valid INSERTs. Validation must run before `SaveChanges`, ideally before the entity enters the tracker.

1. Validate in the API: Data Annotations on DTO (`[Range]`) + `ModelState`, or FluentValidation in the controller/minimal-API filter **before** calling the service.
2. Enforce invariants in the service/domain layer — throw `ArgumentOutOfRangeException` or return `Result` failures so non-HTTP callers are also protected.
3. Add database CHECK constraints as last line of defense (does not replace app validation UX).
4. Optionally implement `SaveChanges` override or `IValidatableObject` on the entity — still keep API-level validation for early 400 responses.
