# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/13. Integration with EF Core`

---

#### Q1. (R) Review this `OrdersController`. QA reports intermittent `DbUpdateConcurrencyException` and slow list endpoints under load.

**Answer:** Injecting `DbContext` directly into controllers couples HTTP, transactions, and persistence in one class — it works in demos but blocks test seams, scatters query logic, and makes concurrency and unit-of-work boundaries easy to get wrong. The immediate fixes are a service/repository layer, correct HTTP semantics on create, and explicit concurrency handling — not more logic in the controller.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Architecture | Controller owns EF queries and `SaveChanges` | Untestable without DB; business rules spread across endpoints |
| API contract | `POST` returns `200 OK` without `Location` or body | Clients cannot discover new resource id; violates REST create pattern |
| Concurrency | No row version / `DbUpdateConcurrencyException` handling | Intermittent 500s when two updates touch same order |
| DI / scope | Long controller methods hold scoped context for full request | Large graphs tracked longer than needed — memory pressure |

**Fix (priority order):**

1. Introduce `IOrderService` (scoped) — controller maps HTTP ↔ DTOs only; service owns `DbContext` usage.
2. Return `CreatedAtAction` with the new id after `SaveChangesAsync` (201 + Location).
3. Add concurrency token on `Order` and catch `DbUpdateConcurrencyException` → 409 Conflict.
4. Keep `AppDbContext` scoped via DI — never register as singleton.

**Production takeaway:** "Thin controller" means thin on HTTP concerns, not "inject DbContext and call EF inline." Karat uses this to test whether you know where persistence boundaries belong.

---

#### Q2. (R) Review this API projection. APM shows 1 query for the order list and 200 extra queries when the endpoint is hit with 100 rows.

**Answer:** The projection references navigations (`Customer.Name`, `Lines.Count`) without translating them in a single SQL shape — with lazy loading enabled, materializing `Order` entities (or partially evaluated graphs) triggers one query per parent for each navigation access. The fix is a single translated query using projection or explicit `Include`/`AsSplitQuery`, and disable lazy loading for API projects.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| N+1 | Navigation props in projection with lazy loading | 1 + N (+ M) SQL round trips per request |
| Query shape | `Select` may not fully translate if client eval sneaks in | Silent fallback to client evaluation in older patterns |
| Performance | List endpoint scales with row count | DB connection pool exhaustion under load |

**Fix (priority order):**

1. Project in one query — EF translates `CustomerName = o.Customer.Name` and `LineCount = o.Lines.Count` into JOIN/subquery SQL when navigations are included in the expression tree before materialization.
2. If using explicit loading pattern: `.Include(o => o.Customer)` and use split query for collections — still one round trip per include level, not N+1.
3. Disable lazy loading (`UseLazyLoadingProxies` off) in API apps — fail fast if navigations accessed accidentally.
4. Validate with logging: `LogTo` or APM — assert query count = 1 for list endpoints.

**Production takeaway:** API projections must be **fully translatable** or **eager-loaded once** — N+1 is the default failure mode when navigations appear in DTO mapping.

---

#### Q3. (R) Review this read-only catalog endpoint. Memory spikes on the API pods during flash sales; GC pauses correlate with `GET /api/products`.

**Answer:** Read-only list endpoints still **track every entity** returned by EF unless you opt out. `Include` pulls large graphs into the change tracker for the lifetime of the scoped `DbContext`, multiplying memory per request when thousands of products load with suppliers and reviews.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracking | No `AsNoTracking()` on read query | Full snapshot of entities + relationships in memory |
| Over-fetching | `Include` on list endpoint | Multiplies payload and tracked graph size |
| API design | Maps full entities then DTO | Loads columns and navigations clients never see |

**Fix (priority order):**

1. Add `.AsNoTracking()` (or `AsNoTrackingWithIdentityResolution` if needed) on all read-only queries.
2. Replace `Include` + entity map with `.Select(p => new ProductDto(...))` — SQL projects only required columns.
3. Paginate — never return unbounded catalog lists (see Q6).
4. For hot read paths, consider cached read models or compiled queries — still untracked.

**Production takeaway:** **AsNoTracking is the default for GET list/detail in APIs** — tracking is for commands that call `SaveChanges`, not for serialization endpoints.

---

#### Q4. (R) Review this checkout flow. Money is deducted from inventory but the order row is missing when support queries SQL after a transient failure mid-request.

**Answer:** Two separate `SaveChangesAsync` calls mean **two independent transactions** — stock can commit while order insert fails on the next flush, leaving inventory inconsistent with orders. Checkout must be one atomic unit of work.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Transaction | No explicit transaction wrapping both saves | Partial commit — stock decremented, no order row |
| Correctness | Null-forgiving `product!` after `FindAsync` | NullReference if id invalid — fails after partial path |
| API design | No idempotency key for checkout | Retries may double-charge stock |

**Fix (priority order):**

1. Wrap in `await using var tx = await _db.Database.BeginTransactionAsync()` — single `SaveChangesAsync` at end, then `CommitAsync`.
2. Prefer one `SaveChanges` after all entity mutations in the same context (transaction still recommended for isolation).
3. Return structured errors; use row-level concurrency on `Stock` to prevent oversell.
4. Add idempotency key header for POST checkout in production APIs.

**Production takeaway:** **One business operation = one transaction** — "transaction per request" often means explicit `BeginTransaction` for multi-entity commands, not relying on implicit single SaveChanges boundaries across failure points.

---

#### Q5. (R) Review this repository and controller pair. Filtering by date works in unit tests (in-memory list) but returns wrong counts in production and sometimes throws after deploy.

**Answer:** Returning **`IQueryable` from the repository** and composing in the controller is valid only if execution stays deferred until the controller builds the full expression tree. Calling synchronous `.ToList()` in the controller forces client-side evaluation of anything not yet translated, pulls entire tables into memory, and breaks when the provider differs from the in-memory test fake.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Repository leak | `GetAll()` exposes composable `IQueryable` | Unbounded queries; filters may not translate |
| Sync over async | `.ToList()` blocks; no `ToListAsync` | Thread pool pressure; inconsistent with async pipeline |
| Design | `Include` always applied in `GetAll` | Over-fetch even when DTO needs no lines |
| Testing gap | In-memory provider hides translation bugs | Passes tests, fails SQL Server |

**Fix (priority order):**

1. Push filter + pagination into repository method: `Task<PagedResult<OrderDto>> SearchAsync(DateTime since, int page, ...)`.
2. Execute with `ToListAsync` **inside** repository on a fully built `IQueryable` — never return `IQueryable` to controllers unless you fully control expression boundaries.
3. Make controller action async end-to-end.
4. Integration-test against real SQL (or Testcontainers) for query translation.

**Production takeaway:** **Leaked `IQueryable`** is a common "works in unit test" trap — the repository should expose intent-based methods, not raw composable queries.

---

#### Q6. (R) Review this paginated list endpoint. Page 2 sometimes repeats rows from page 1; under concurrent inserts, clients see duplicates and gaps.

**Answer:** **`Skip`/`Take` without a stable `OrderBy`** produces undefined row order — SQL Server (and others) may return rows in any physical order, so pages overlap or skip as data moves. Pagination requires deterministic sort plus, for high-churn feeds, keyset pagination.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pagination | Missing `OrderBy` before `Skip`/`Take` | Duplicate/missing rows across pages |
| Concurrency | Offset pagination under concurrent inserts | Classic page drift during writes |
| Performance | `CountAsync()` on full table every request | Expensive on large tables |

**Fix (priority order):**

1. Add stable sort: `.OrderBy(p => p.Id)` or `.OrderByDescending(p => p.CreatedUtc).ThenBy(p => p.Id)`.
2. For live catalogs, prefer keyset: `Where(p => p.Id > lastId).OrderBy(p => p.Id).Take(pageSize)`.
3. Cache or approximate total count when exact total is not required.
4. Combine with `AsNoTracking` and column projection (Q3).

**Production takeaway:** **Pagination + EF** always means **ORDER BY + SKIP/FETCH** — without order, pages are nondeterministic.

---

#### Q7. (P) An API team registers `AppDbContext` as scoped and injects it into controllers, services, and a **Singleton** `PricingCacheWarmupService` that preloads prices at startup. What failure mode appears in production, and what patterns fix EF usage in background work?

**Answer:** A singleton cannot consume a scoped `DbContext` — with `ValidateScopes` enabled, the app fails at startup; without validation, you get **captive dependency**: one disposed context reused across the app lifetime, or `ObjectDisposedException` after the first request scope ends. Background EF work needs its own scope per operation.

- Register `IDbContextFactory<AppDbContext>` or create a scope in the hosted service: `using var scope = _scopeFactory.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();`.
- Run warmup inside `IHostedService.StartAsync` with a **fresh scope**, not constructor-injected context.
- Never hold `DbContext` in singleton fields; cache **DTOs/primitives**, not the context.
- Enable `ValidateOnBuild` and scope validation in development to catch this at startup.

**Production takeaway:** Same captive-dependency rule as DI gotchas — **singleton + DbContext** is always wrong; factory or scoped resolution per job is the fix.

---

#### Q8. (D) You inherit an API where every list endpoint returns full EF entities (including navigation graphs) serialized directly to JSON. Product list responses are 2 MB and Swagger shows circular reference warnings. Compare three remediation options and when you would pick each.

**Answer:** Returning tracked entity graphs couples persistence model to HTTP contract, over-fetches data, and invites circular reference hacks (`ReferenceHandler.IgnoreCycles`) that hide design problems.

- **DTO + projection (preferred for most REST APIs):** `.Select(p => new ProductListItemDto(...))` with `AsNoTracking` — smallest payload, stable contract, no cycle risk. Pick when clients need predictable JSON and you own the API surface.
- **AutoMapper / manual mapper from entity with explicit includes:** Faster migration from legacy code; still load too much if includes are broad. Pick for short-term refactor when many endpoints must ship quickly — plan to narrow queries.
- **GraphQL or OData (selective fields):** Client-driven shape — adds complexity, auth, and N+1 risk. Pick when many clients need different field sets and you will invest in DataLoader/guards — not as a band-aid for lazy entity serialization.

**Production takeaway:** Serialize **contracts**, not **EF graphs** — `ReferenceHandler.IgnoreCycles` is a warning sign, not a production strategy.

---

#### Q9. (M) A teammate proposes `AddDbContextFactory<AppDbContext>()` alongside scoped `AppDbContext` for the same API. Under what request patterns does `IDbContextFactory` help, and when should handlers keep using scoped `DbContext` from DI?

**Answer:** Scoped `DbContext` from DI matches **one HTTP request = one unit of work** — controllers and services in the same request share the same tracker and transaction. `IDbContextFactory` creates **short-lived contexts** on demand — ideal when one request needs **multiple isolated units of work** (parallel tasks, middleware that must not share tracker state, background work triggered from a request).

- Use **scoped injection** for normal CRUD endpoints and services participating in one transaction per request.
- Use **factory** when: spawning `Task.Run` work (anti-pattern but seen), multi-tenant parallel queries, gRPC/streaming handlers that outlive a single logical UoW, or hosted services (with `CreateDbContext()` per operation).
- Do not inject both into the same class without clear boundaries — two contexts do not share change tracker; dual writes need explicit transaction coordination.
- Register factory with same options as scoped context (same connection, interceptors).

**Production takeaway:** Factory is not a replacement for scoped context in typical Web API actions — it solves **context lifetime shorter or multiple per operation** than the HTTP scope.

---
