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

## Gotchas

---

## Gotcha 15. `SaveChanges` without a transaction for multi-step updates

**Concepts**
- independent SaveChanges commits without explicit transaction
- partial update on multi-step failure
- BeginTransactionAsync and CommitAsync wrapping

**Answer**

Multiple `SaveChanges` calls or separate database operations that must succeed together commit independently by default, allowing partial updates that leave data in an inconsistent state when a later step fails. Wrap related saves and raw SQL in `BeginTransactionAsync`/`CommitAsync` on one `DbContext`; Prefer one `SaveChanges` per unit of work when all changes are tracked together on the same context. Retry logic after failure cannot assume earlier steps rolled back unless they shared a transaction boundary.

---

## Gotcha 5. Transaction started after first command

**Concepts**
- transaction started after first DML autocommit
- BeginTransaction before first command
- unit-of-work atomicity requirement

**Answer**

Beginning a `SqlTransaction` only after the first statement already executed means that statement committed under implicit autocommit, so later steps in the intended unit of work are not atomic with the first. Call `BeginTransaction` immediately after opening the connection, before any DML; EF Core `SaveChanges` without an explicit transaction auto-commits each call — wrap multi-step work explicitly. Integration tests with single-user data often miss this race because implicit commits appear to "work.".

---

## Gotcha 14. Tracking overhead on read-only queries

**Concepts**
- change tracking snapshot overhead on read-only queries
- AsNoTracking omission on GET endpoints
- global QueryTrackingBehavior.NoTracking setting

**Answer**

Omitting `AsNoTracking()` on large read-only lists makes EF Core snapshot every entity for change detection that will never run, wasting memory and CPU on GET endpoints. Tracking stores original and current values per property for each row materialized; ASP.NET Core read services should default to `AsNoTracking()` plus DTO projection. Global `QueryTrackingBehavior.NoTracking` with explicit tracking on command paths prevents accidental overhead.

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
