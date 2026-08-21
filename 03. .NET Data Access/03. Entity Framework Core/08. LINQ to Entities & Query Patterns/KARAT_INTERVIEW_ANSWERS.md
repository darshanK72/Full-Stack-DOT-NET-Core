# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/08. LINQ to Entities & Query Patterns`

---

#### Q1. (R) A catalog API endpoint is slow under load. Review this repository method from the EF Core LINQ chapter's `StoreDbContext` model. What executes on SQL Server vs in the app, and what would you change?

**Answer:** `AsEnumerable()` switches the query from LINQ to Entities to LINQ to Objects, so SQL Server returns every product row (respecting global filters) and the app filters by category name and price in memory — correct results, catastrophic scale.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Query translation | `AsEnumerable()` before `Where` | Provider becomes in-memory; SQL is `SELECT * FROM Products` (plus global filter) |
| Performance | Category name filter runs client-side | Full table pulled over the network; CPU and memory grow with catalog size |
| Design | Navigation `Category!.Name` used after provider switch | JOIN could run in SQL if the query stayed `IQueryable` |

**Fix (priority order):**

1. Remove `AsEnumerable()` — keep the chain on `IQueryable<Product>` until the terminal operator.
2. Filter on translatable members: `.Where(p => p.CategoryId == 1 && p.UnitPrice >= 50m)` or `.Where(p => p.Category!.Name == "Electronics" && p.UnitPrice >= 50m)` so EF generates `JOIN` + `WHERE`.
3. Prefer `.Select` to a DTO when the API only needs a subset of columns (see `ProjectionQueries.GetProductCatalogItems()`).
4. Use `ToQueryString()` or SQL logging in dev to verify predicates appear in `WHERE`, not only after materialization.

**Production takeaway:** `AsEnumerable()` / `ToList()` early in the pipeline is a classic Karat trap — same LINQ syntax, opposite execution location. See LINQ ch.01 — `IQueryable` vs `IEnumerable`; this chapter's **Program.cs** quick reference — client evaluation pitfall.

---

#### Q2. (R) This search compiles but throws at runtime when hit through the API. Review the query and helper. What failed to translate, and how do you fix it without loading the whole table?

**Answer:** EF Core cannot translate the custom `IsPremiumSku` method (and its `StringComparison` overload) into SQL, so query translation fails at execution time unless you rewrite the predicate with translatable expressions or explicitly opt into client evaluation — which you should avoid for filters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Translation | Non-mapped instance/static method in `Where` | `InvalidOperationException` — "could not be translated" at `ToList()` |
| API surface | `StringComparison.OrdinalIgnoreCase` in helper | Not a known SQL pattern for EF's translator |
| Performance risk | Workaround via `AsEnumerable()` before filter | Would load all products — unacceptable for search |

**Fix (priority order):**

1. Inline translatable string logic: `.Where(p => p.Name.StartsWith("Pro") && p.StockQuantity > 0)` — EF Core translates `StartsWith` to SQL `LIKE`.
2. For case-insensitive search on SQL Server, use `.Where(p => EF.Functions.Like(p.Name, "Pro%"))` or configure a case-insensitive collation on the column — do not pull rows to CLR for casing.
3. If logic is truly domain-specific and non-translatable, filter translatable columns in SQL first (category, price band), then apply the helper only on the reduced set — or persist a computed/stored flag (`IsPremium`) set on write.
4. Never hide untranslatable calls inside repository helpers without documenting they force client evaluation.

**Production takeaway:** Compilable LINQ ≠ translatable LINQ — Karat tests whether you diagnose translation failures vs blaming "EF is slow." See **FilteringQueries** — composable `IQueryable` filters that stay on the provider.

---

#### Q3. (R) An order-history endpoint works in dev with 3 seed orders but degrades badly in production. Review this service method. What query pattern causes the regression, and how do you fix it?

**Answer:** Materializing orders without eager-loading related data, then touching `Lines` and `Product` in LINQ to Objects, triggers lazy-loading N+1 queries — one round-trip for orders plus one (or more) per order line and product in production volume.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Data loading | `ToList()` on orders only | Parent rows loaded; navigations unset |
| N+1 | `o.Lines` and `l.Product!.Name` in memory loop | Each access can emit `SELECT` — 1 + N + M queries |
| Scalability | `string.Join` over lazy collections | Latency grows linearly with orders × lines; connection pool pressure |

**Fix (priority order):**

1. Eager-load in one translated query: `.Include(o => o.Lines).ThenInclude(l => l.Product)` before `ToList()` (detailed in ch.09 Loading Related Data).
2. Better for read-only summaries: project in SQL with `SelectMany` (see `ProjectionQueries.GetFlatOrderLineDescriptions()`) so the database returns flat rows — no entity graph, no lazy load.
3. Use `.AsNoTracking()` on read paths to avoid change-tracker overhead when entities are not updated.
4. Enable EF Core query logging or `TagWith` in staging and assert a single SQL batch (or known small count) per API call.

**Production takeaway:** Small seed data hides N+1 — Karat pairs "works locally" with production query multiplication. See **Order** model comment — lines navigation without Include is intentional in the filtering demo, not in list endpoints.

---

#### Q4. (R) A junior dev refactors the product search to "keep filtering composable" by returning `IQueryable<Product>` from the repository. Under ASP.NET Core scoped `DbContext`, intermittent `ObjectDisposedException` appears in production. Review both layers. What leaked, and what boundary would you enforce?

**Answer:** The repository leaks `IQueryable<Product>` tied to a scoped `StoreDbContext`; when `ToList()` runs after the scope (or after the context is disposed), EF cannot execute the deferred query — composability across layers without a shared lifetime causes intermittent failures under timing pressure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `IQueryable` escapes repository with captured `_context` | Execution deferred past valid `DbContext` lifetime |
| Architecture | Service adds `OrderBy` then materializes | Looks composable, but couples callers to EF provider and context scope |
| Correctness | Intermittent `ObjectDisposedException` | Passes unit tests that dispose synchronously; fails in prod under async middleware |

**Fix (priority order):**

1. Materialize inside the scope that owns the context: repository (or service) calls `.ToListAsync()` / `.ToList()` before returning — return `IReadOnlyList<Product>` or DTOs, not `IQueryable`.
2. If dynamic filtering is required, pass filter parameters into one repository method that builds and executes the query internally (pattern in **FilteringQueries.SearchProducts**).
3. For advanced cases (reporting, admin grids), use a dedicated query object or `IQueryable` only within a single method where context lifetime is explicit — never return it from DI-scoped repositories to controllers.
4. Add an analyzer or team rule: public repository methods must not expose `IQueryable<T>`.

**Production takeaway:** `IQueryable` is deferred execution plus a live `DbContext` — leaking it is leaking scope. Karat tests repository boundaries, not just LINQ syntax. See **Program.cs** quick reference — disposed context pitfall.

---

#### Q5. (P) The chapter's `ProjectionQueries.GetProductCatalogItems()` projects to `ProductListItem` in the database. A teammate returns full `Product` entities from the repository and maps to DTOs in the controller with AutoMapper. Both compile. When would you insist on server-side `Select` projection, and what breaks if you skip it?

**Answer:** Server-side `Select` keeps column subsetting and joins in SQL; returning tracked entities and mapping in memory ships every column, hydrates navigations you may not need, and invites lazy-load surprises — acceptable only for small internal tools or true aggregate roots you will update.

- **Insist on SQL projection** for list/catalog APIs, reports, and any read-heavy endpoint where the client needs fewer fields than the entity — bandwidth, memory, and GC improve measurably.
- **Global filters and soft-delete** still apply in `Select` — discontinued products stay hidden unless `IgnoreQueryFilters()` is intentional (see **StoreDbContext** `HasQueryFilter`).
- **AutoMapper after full entity load** often triggers N+1 if DTOs reference `Category.Name` and `Category` was not included — projection folds the join into one `SELECT`.
- **Change tracking:** projected DTOs are not tracked — safer for read-only APIs; full entities carry tracker cost unless you add `AsNoTracking()`.
- **When full entities are OK:** single-entity get-by-id with update intent, or bounded admin screens with explicit `Include` plan.

**Production takeaway:** Composability of LINQ stops at the repository boundary; composability of *data shape* belongs in translatable `Select` — matches **ProjectionQueries** Section 1 and the chapter's DTO `ProductListItem`.

---

#### Q6. (M) A background job captures an `IQueryable<Product>` during request handling and enumerates it minutes later on a thread-pool thread. The chapter's `IQueryableDemonstrations` stresses deferred execution. Walk through what happens from query construction to `foreach`, and what materialization rule you would enforce in code review.

**Answer:** Assigning to `IQueryable<Product>` only builds an expression tree — no SQL runs until enumeration; deferring `foreach` until after the request-scoped `DbContext` is disposed (or on another thread without that scope) throws or behaves unpredictably because the query is bound to a dead context.

- **Construction:** `GetPendingPriceReview` chains `Where`/`OrderBy` on `_context.Products` — provider captures the context; **Program.cs** / **IQueryableDemonstrations** — no DB round-trip yet.
- **Deferred window:** Returning or storing `IQueryable` exports that capture — any later `foreach`, `ToList`, or async iteration is the first execution point.
- **Failure mode:** Request ends → scoped `StoreDbContext` disposed → background `foreach` calls into EF → `ObjectDisposedException` (or worse, reuse of a pooled context in incorrect scope if misconfigured).
- **Secondary trap:** Even with a live context, long-lived `IQueryable` reused across callers can compose unintended filters or race on shared scoped state.
- **Code-review rule:** Materialize (`ToListAsync`, etc.) before leaving the owning scope; pass `IReadOnlyList<T>`, keys, or messages to background work — never `IQueryable<T>`.

**Production takeaway:** Deferred execution is a lifetime contract, not a performance trick — same lesson as **DemonstrateDeferredExecution** in **Program.cs**, extended to async and background jobs. See LINQ ch.01 — terminal operators trigger execution.

---
