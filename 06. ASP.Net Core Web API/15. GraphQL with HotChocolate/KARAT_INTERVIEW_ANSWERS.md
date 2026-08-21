# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/15. GraphQL with HotChocolate`

---

#### Q1. (R) Review this Hot Chocolate query type. APM logs 1 query for authors and 50 follow-up queries when the client requests 50 authors each with `books { title }`.

**Answer:** The `Books` field resolver runs **once per parent author** and issues a separate SQL query each time — classic **GraphQL N+1**. Root `GetAuthors` loads authors; each nested field hits the database again.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| N+1 | Field resolver queries `Books` per author | 1 + N SQL round trips |
| Data access | New query in resolver instead of batching | DB saturation on wide queries |
| Design | Navigation exposed as lazy field resolver | Scales with selection set width |

**Fix (priority order):**

1. Register **`BatchDataLoader<int, Book[]>`** (or grouped loader) — collect author ids during field resolution, one `WHERE AuthorId IN (...)` query.
2. In Hot Chocolate: `.AddDataLoader<AuthorBooksDataLoader>()` and resolve via `dataLoader.LoadAsync(authorId)`.
3. Alternative for simple cases: project on root — `authors { books { title } }` loaded with single query using `Include` or split query at root (less flexible than DataLoader).
4. Monitor field-level query count in staging with Hot Chocolate execution diagnostics.

**Production takeaway:** **N+1 with DataLoader** is the defining GraphQL production pattern — resolvers must batch, not query per parent row.

---

#### Q2. (R) Review this resolver registration in `Program.cs`. Under concurrent GraphQL requests, you see `ObjectDisposedException` on `AppDbContext` and occasional cross-request data leaks in logs.

**Answer:** Registering `Query` as **Singleton** while it holds (or resolves) **scoped `AppDbContext`** creates a captive dependency — one disposed or shared context across requests. GraphQL resolvers and root types must align with DI lifetimes: **scoped** for data access, **singleton** only for stateless infrastructure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Singleton `Query` + scoped `DbContext` | `ObjectDisposedException`; undefined behavior |
| Concurrency | Shared instance across requests | Cross-request state bleed |
| Resolver DI | Field resolver pulls scoped service from singleton parent path | Lifetime mismatch |

**Fix (priority order):**

1. Remove singleton registration — Hot Chocolate resolves query types **per request** by default when registered through server builder; do not `AddSingleton<Query>()`.
2. Inject `AppDbContext` only into scoped services or use `[Service]` in resolvers within request scope.
3. Enable `ValidateScopes` on host build to fail startup on captive dependencies.
4. For expensive stateless helpers, inject singleton **services** into scoped resolvers, not the reverse.

**Production takeaway:** **Resolver DI lifetime** — data loaders and resolvers using EF are **scoped**; singleton is for caches and pure functions only.

---

#### Q3. (R) Review production GraphQL exposure. Security scan flags `/graphql` — anonymous clients can POST deeply nested queries; one request pinned CPU at 100% for two minutes.

**Answer:** GraphQL exposes a **programmatic query API** — without **depth and complexity limits**, attackers craft expensive nested queries (or introspection-driven fan-out) that bypass REST route-level throttling. Anonymous access amplifies abuse.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No auth on `/graphql` | Public attack surface |
| DoS | No depth/complexity limits | Single POST can exhaust CPU/DB |
| Schema | Deep self-referential types | Exponential resolver work |

**Fix (priority order):**

1. Add **`AddMaxExecutionDepth(10)`** (tune per schema) and **`AddCostAnalysis`** / complexity rules in Hot Chocolate.
2. Require authentication — `[Authorize]` on server or field level; rate limit by client id.
3. Disable or restrict introspection in production (`ModifyRequestOptions` / disable schema introspection for anonymous).
4. Persisted queries or allow-list for public mobile clients — reject ad-hoc arbitrary documents.

**Production takeaway:** **Query depth/complexity limits** are mandatory for public GraphQL — REST's fixed endpoints limit work per request; GraphQL does not unless you enforce it.

---

#### Q4. (R) Review this schema configuration for production vs development. Pen testers retrieved the full schema and built queries for admin-only fields using introspection.

**Answer:** Disabling Banana Cake Pop does **not** disable **introspection** — clients can still POST `{ __schema { types { name fields { name } } } }` and discover `GetPayrollTotal` and `GetAllUsers`. Admin fields without auth are callable once names are known.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Schema exposure | Introspection enabled in production | Full API surface leaked to attackers |
| Authorization | Sensitive fields on public `Query` | IDOR / data exfiltration |
| Tooling | Confusion between UI tool off vs introspection off | False sense of security |

**Fix (priority order):**

1. Disable introspection for production: Hot Chocolate `ModifyRequestOptions(o => o.IntrospectionAllowed = false)` or require auth for introspection.
2. Add `[Authorize(Roles = "Admin")]` on sensitive fields and types.
3. Split admin schema to internal endpoint/VPN or separate GraphQL server.
4. Keep Banana Cake Pop dev-only; audit logged queries in prod.

**Production takeaway:** **Schema exposure** is a production control — treat introspection like Swagger UI on admin APIs: off or authenticated.

---

#### Q5. (P) Explain how **DataLoader** fixes the N+1 pattern in Q1. What does batching look like at the SQL level, and where do you register loaders in Hot Chocolate (`AddDataLoader`, scoped lifetime)?

**Answer:** DataLoader **defers and batches** loads within one GraphQL request execution. When 50 `Books` fields resolve, each calls `LoadAsync(authorId)` — the loader collects ids until the scheduler yields, then runs **one query**: `SELECT * FROM Books WHERE AuthorId IN (@p0, @p1, ...)`, distributes results back to awaiters.

- Register: `builder.AddGraphQLServer().AddDataLoader<AuthorBooksBatchLoader>()` — loader class inherits `BatchDataLoader<int, IReadOnlyList<Book>>`.
- Lifetime: **scoped per request** — batch cache must not leak across GraphQL operations.
- SQL: single IN clause or join from parent ids; for many-to-one, `GroupedDataLoader` maps author → list.
- Contrast with Include-at-root: DataLoader batches **only what the client selected**.

**Production takeaway:** DataLoader turns N resolver queries into **1 batched query per resource type per request** — essential for GraphQL at scale.

---

#### Q6. (D) A product owner asks: "We already have REST — why add GraphQL?" Compare **over-fetching / under-fetching** trade-offs for a mobile app that needs user profile + last 5 orders + avatar URL. When would you keep REST, when GraphQL, when both?

**Answer:** REST often forces **multiple round trips** (under-fetching) or **fat DTOs** (over-fetching) — `/users/{id}`, `/users/{id}/orders?take=5`, `/users/{id}/avatar` vs one `/users/{id}?include=everything` payload with unused fields. GraphQL lets the client request `{ user { name avatarUrl orders(take:5) { id total } } }` in **one HTTP call** with **no extra fields**.

- **Keep REST:** simple public API, CDN-cacheable resources, file uploads, teams without GraphQL operational maturity, strict rate limiting per route.
- **Add GraphQL:** many clients with different field needs, mobile/slow networks, rapid UI iteration without new endpoints — invest in DataLoader, limits, auth.
- **Both:** REST for writes/webhooks/cache-friendly reads; GraphQL for composite mobile BFF — common at scale behind gateway.

**Production takeaway:** GraphQL trades **endpoint simplicity** for **query flexibility** — operational cost (N+1, complexity attacks) must be budgeted.

---

#### Q7. (M) You must enforce authorization on GraphQL — some fields are public, `GetPayrollTotal` is admin-only, and users may only read their own `orders`. Compare **ASP.NET Core policy on the request**, **Hot Chocolate `[Authorize]` on fields**, and **manual checks inside resolvers**. What fails if you only put `[Authorize]` on the controller equivalent?

**Answer:** GraphQL has **no per-action controller** — one endpoint executes many fields. **Request-level `[Authorize]`** on `MapGraphQL` blocks anonymous users entirely but cannot express "public catalog + private orders" on the same schema. **Field/type `[Authorize]`** (Hot Chocolate integrates ASP.NET Core policies) applies policy per field — correct default for mixed schemas. **Manual checks** in resolvers (`if (userId != parent.UserId) throw ...`) handle row-level rules policies cannot express alone.

- Fail if only middleware/controller auth: entire schema locked or entirely open — no field granularity.
- Best practice: authenticate at HTTP layer + **`[Authorize]` on sensitive fields** + manual resource checks where policy needs entity id from parent.
- Subscriptions and mutations need the same field-level rules as queries.

**Production takeaway:** **Auth on GraphQL** is **field-level by default** — REST's `[Authorize]` on `MeController` does not map to one GraphQL endpoint without field attributes.

---

#### Q8. (R) Review this mutation and follow-up query in one GraphQL request. Clients report stale `inventoryCount` on `Product` immediately after `updateStock` succeeds.

**Answer:** Within one scoped `DbContext`, the mutation **tracks** the updated `Product`; the subsequent `product(id: 1)` resolver may return **cached tracked state**, skip a fresh read, or conflict with `AsNoTracking` expectations — clients see inconsistent stock depending on resolver implementation. Separate operations in one document still share one request scope and one context.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracker | Same context serves mutation and query fields | Stale or inconsistent reads in one response |
| Resolver | `FindAsync` returns tracked entity without refresh | May not reflect DB if another writer exists |
| Design | Returning entity directly from mutation | Clients depend on mutation payload vs query field semantics |

**Fix (priority order):**

1. Return updated values from mutation selection — client should read `updateStock { stock }` not rely on follow-up field in same doc for critical data.
2. In query resolver after mutation: `ReloadAsync` or new query with `AsNoTracking()` if fresh read required.
3. Consider separate DbContext for read vs write in same request only with clear boundaries (advanced — usually document client pattern instead).
4. Expose `stock` via DataLoader batch refresh for product fields.

**Production takeaway:** GraphQL **request-scoped DbContext** means mutation + query in one document share tracker state — design clients and resolvers explicitly.

---
