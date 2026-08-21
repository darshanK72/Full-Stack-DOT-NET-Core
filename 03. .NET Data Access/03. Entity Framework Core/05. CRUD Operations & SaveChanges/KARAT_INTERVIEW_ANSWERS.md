# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/05. CRUD Operations & SaveChanges`

---

#### Q1. (R) An API endpoint maps a JSON body to `Product`, assigns `ProductId` from the route, changes `UnitPrice`, and calls `SaveChanges`. The response is 200 but the price in SQL is unchanged. Review this service method — what is wrong and how do you fix it?

**Answer:** The stub is **detached** — `SaveChanges` only executes SQL for entities the change tracker knows about in `Added`, `Modified`, or `Deleted` state. Building a `Product` in memory and never calling `Update()`, `Attach()`, or loading it with `Find` leaves nothing pending, so zero rows are written even though the method returns successfully.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracking | Detached entity never registered with context | `SaveChanges` returns 0; UPDATE never sent |
| API contract | HTTP 200 implies success | Silent data loss — hardest class of EF bug in production |
| Pattern | Confuses in-memory mutation with tracked update | Same mistake as tutorial "forget Update() on detached stub" |

**Fix (priority order):**

1. **Preferred:** Load tracked entity — `var product = context.Products.Find(productId); product.UnitPrice = newPrice;` then `SaveChanges()` (matches `TryUpdateUnitPriceTracked` in **ProductService.cs**).
2. **When stub is required:** `context.Products.Update(stub)` or `context.Entry(stub).Property(p => p.UnitPrice).IsModified = true` after attach — only if no other instance with the same key is already tracked.
3. Check `SaveChanges()` return value or verify row state in tests — do not assume flush happened.
4. Return 404 when `Find` returns null instead of no-op save.

**Production takeaway:** EF does not diff arbitrary objects against the database — only the change tracker drives SQL. See **Program.cs** Section 5b and **ProductService.UpdateProductDetached**.

---

#### Q2. (R) A teammate "fixes" detached updates by always calling `DbSet.Update()`. Partial PATCH requests now wipe columns the client did not send. Review this controller helper — what breaks, and what pattern fixes it without overwriting untouched fields?

**Answer:** `DbSet.Update()` is equivalent to **Attach + mark entire entity Modified**, so every mapped property is included in the UPDATE statement. Unset properties (`StockQuantity = 0`, `IsDiscontinued = false`) overwrite existing database values — a classic partial-update bug.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `Update()` marks **all** scalar properties Modified | Columns not in DTO reset to defaults |
| Data integrity | `StockQuantity` zeroed, flags cleared | Inventory and status corrupted on PATCH |
| Design | Treating PATCH like full PUT | Works in demos with complete objects; fails with real DTOs |

**Fix (priority order):**

1. Load the tracked entity with `Find` or a query, mutate only DTO-supplied properties, then `SaveChanges()` — change tracker emits UPDATE only for changed columns (default).
2. For detached graphs: attach then mark individual properties — `context.Attach(entity); context.Entry(entity).Property(p => p.UnitPrice).IsModified = true;` (or `Entry(...).CurrentValues.SetValues(dto)` with explicit field list).
3. Use a dedicated PATCH model with nullable fields; apply only non-null members to the tracked entity.
4. Reserve `Update(entity)` for full replacement scenarios (PUT) where every column is intentionally supplied.

**Production takeaway:** Attach vs Update is not interchangeable with PATCH semantics — **Update means "replace whole row."** See **ProductService.cs** Section 5 comments on `Update()` attaching + Modified for all mapped props.

---

#### Q3. (M) This code mirrors **Program.cs Section 5b** but skips `ChangeTracker.Clear()`. What exception or silent failure do you expect at runtime, and what are two safe ways to update a detached stub when another instance might already be tracked?

**Answer:** EF Core allows **only one tracked instance per key** per context. Calling `Update(stub)` while `tracked` is still in the change tracker throws `InvalidOperationException` — "The instance of entity type 'Product' cannot be tracked because another instance with the same key value is already being tracked."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracker | Two instances, same `ProductId` | Hard failure on `Update()` |
| Workflow | Earlier mutation left entity Modified | Even Unchanged duplicate keys conflict on attach |
| Demo gap | Tutorial clears tracker before detached update | Production code copying 5b without `Clear()` breaks |

**Fix (priority order):**

1. **Best:** Reuse the tracked instance — mutate `tracked.ProductName = newName` and drop the stub entirely (no second instance).
2. Detach before attach: `context.Entry(tracked).State = EntityState.Detached` or `context.ChangeTracker.Clear()` (as in **Program.cs** line 118) then `Update(stub)` — only when you intentionally discard prior pending changes on that context.
3. Use a **new short-lived DbContext** per operation in web apps (scoped DI) so stale tracked entities from earlier steps in the same request are less common — still prefer single-instance update.
4. Never call `Update()` on a stub when you already have the entity from `Find` in the same method.

**Production takeaway:** Karat tests whether you read the tutorial's `ChangeTracker.Clear()` comment as a **symptom fix** and know the production fix is **one tracked instance per key**. Forward reference: attach patterns in ch11 Change Tracking.

---

#### Q4. (P) A catalog import reads 10,000 CSV rows and persists each with `Add` + `SaveChanges` inside the loop. It works on 50 rows in QA but times out in production. What is wrong with the persistence pattern, and what would you change?

**Answer:** Each `SaveChanges()` opens a transaction, generates INSERT SQL, waits for the database, and resets change-tracker state — **10,000 round trips** instead of one batched unit of work. Throughput collapses under network latency, log growth, and lock duration even though the logic is "correct."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | SaveChanges per iteration | N transactions; import timeouts |
| Scalability | Change tracker churn | Memory and CPU on tracker between flushes |
| Operations | Long-held connections | Pool exhaustion; harder to retry atomically |

**Fix (priority order):**

1. Batch: `Add` / `AddRange` many entities, then **one** `SaveChanges()` per chunk (e.g. 500–1000 rows) — matches **Program.cs** Section 7 batch pattern.
2. For very large loads, consider `BulkInsert` extensions or raw SQL/`SqlBulkCopy` when EF change tracking is unnecessary.
3. Wrap each chunk in an explicit transaction if all-or-nothing per batch is required.
4. Use `SaveChangesAsync` with cancellation for API-hosted imports.

**Production takeaway:** `SaveChanges` is the **commit boundary**, not a per-row persist call — the chapter demo batches Add + Update + Remove before a single flush for exactly this reason. See **ProductService.cs** Section 7.

---

#### Q5. (P) Two admins edit the same product. Admin A loads price 10.00, Admin B loads price 10.00; B saves 12.00, then A saves 11.00. No error is thrown. The `Product` entity has no concurrency column. What data-loss scenario is this, and how would you add optimistic concurrency in EF Core?

**Answer:** This is **last-write-wins** — EF's default UPDATE uses only the primary key in the WHERE clause, so Admin A's save overwrites B's 12.00 with 11.00 without detecting that the row changed in between. No concurrency token means no `DbUpdateConcurrencyException` and no signal to retry or merge.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Stale read persisted silently | Lost update; audit trail shows both "success" |
| Collaboration | No conflict detection | Support tickets: "my price change disappeared" |
| Schema | `Product` has no token column | EF cannot compare original vs current row version |

**Fix (priority order):**

1. Add a concurrency property — e.g. `public byte[] RowVersion { get; set; }` with `[Timestamp]` or fluent `IsRowVersion()`, or a dedicated `uint Version` with `[ConcurrencyCheck]`.
2. On `SaveChanges`, EF includes the token in the WHERE clause; mismatch throws **`DbUpdateConcurrencyException`** — catch, reload, merge or return 409 Conflict to the client.
3. Expose current token to clients (ETag header or DTO field) so updates send **If-Match** semantics.
4. For high-contention fields, combine tokens with domain rules (minimum price floors) validated before save (Q6).

**Production takeaway:** CRUD without concurrency tokens is fine for tutorials; multi-user production catalogs need optimistic concurrency or explicit locking. This chapter's `Product` entity intentionally omits tokens — migrations chapter adds schema evolution.

---

#### Q6. (R) A bulk pricing endpoint accepts client-supplied `UnitPrice` and `StockQuantity`, calls `AddProduct` + `SaveChanges`, and returns the new `ProductId`. Negative prices and zero stock slip through to SQL. Review this flow — what is missing before `SaveChanges`, and where should validation live in a real API?

**Answer:** EF Core persists whatever the change tracker holds — it does **not** validate business rules. `AddProduct` only constructs an entity and sets `Added`; without guards, invalid scalars become valid INSERTs. Validation must run **before** `SaveChanges`, ideally before the entity enters the tracker.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Domain rules | No checks on price/stock | Negative prices, bad inventory in production DB |
| Layering | Controller trusts DTO blindly | Invalid data crosses API boundary |
| EF behavior | `SaveChanges` succeeds on invalid business data | Silent acceptance — constraints only if DB CHECK/FK exist |

**Fix (priority order):**

1. Validate in the API: Data Annotations on DTO (`[Range]`) + `ModelState`, or FluentValidation in the controller/minimal-API filter **before** calling the service.
2. Enforce invariants in the service/domain layer — throw `ArgumentOutOfRangeException` or return `Result` failures so non-HTTP callers are also protected.
3. Add database CHECK constraints as last line of defense (does not replace app validation UX).
4. Optionally implement `SaveChanges` override or `IValidatableObject` on the entity — still keep API-level validation for early 400 responses.

**Production takeaway:** **SaveChanges is not a validation gate** — it translates tracked state to SQL. Pair CRUD services (like **ProductService.AddProduct**) with explicit validation and ProblemDetails for 400/409 responses in ASP.NET Core (see ch08 Model Binding & Validation). Return meaningful errors before flush, not after bad rows land in SQL.

---
