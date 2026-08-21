# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/02. DbContext & DbSet`

---

#### Q1. (R) A teammate "optimizes" startup by registering `AppDbContext` as a **Singleton** and injecting it into controllers. The API passes smoke tests locally but corrupts data under concurrent load. Review the registration and usage — what is wrong, and how do you fix it?

**Answer:** `DbContext` is not thread-safe and must represent one unit of work per scope — registering it as a singleton shares one change tracker and connection semantics across all requests, which causes cross-request state bleed and race conditions under concurrency. The snippet also registers `AddDbContext` twice (singleton factory plus default scoped registration), creating ambiguous DI resolution.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `AddSingleton<AppDbContext>` | One context shared by all HTTP requests |
| Thread safety | Concurrent `ToListAsync` / `SaveChanges` on same instance | Corrupted change tracker; intermittent wrong reads/writes |
| Design | Duplicate registration (`AddSingleton` + `AddDbContext`) | Unpredictable which registration wins; harder to diagnose |
| Resource | Long-lived context holds DB connection / tracked entities | Connection pool pressure; memory growth on tracked graphs |

**Fix (priority order):**

1. Remove the `AddSingleton<AppDbContext>` registration entirely.
2. Keep only `services.AddDbContext<AppDbContext>(...)` — EF registers the context as **scoped** (one per HTTP request).
3. Inject `AppDbContext` into controllers/transient services within the request scope; never store it on singleton fields.
4. For background work, create an `IServiceScope` per job and resolve a fresh context inside that scope.

**Production takeaway:** Karat pairs "singleton for performance" with EF — the correct default is scoped `AddDbContext`, not shared instances. See this chapter's `DbContextServiceRegistration` Section 8 and `AppDbContext` lifetime comments.

---

#### Q2. (R) A background job processes a price-update queue with `Parallel.ForEachAsync`, reusing one injected `AppDbContext`. Review this worker — what breaks under concurrency, and what lifetime pattern replaces it?

**Answer:** A single `DbContext` instance must not be used concurrently from multiple threads — `Parallel.ForEachAsync` violates that rule, so EF Core's change tracker and internal state can corrupt updates or throw inconsistent exceptions. The worker also captures a scoped context in a singleton `BackgroundService`, which is a captive dependency that may be disposed before the host shuts down.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Thread safety | Shared `_db` across parallel iterations | Undefined behavior; lost updates or tracker exceptions |
| DI lifetime | Scoped `AppDbContext` injected into singleton worker | Captive dependency; `ObjectDisposedException` after scope ends |
| Unit of work | One context tracking interleaved `SaveChanges` from parallel tasks | Conflicting `EntityState`; partial commits |
| Scalability | Serial connection reuse under fake parallelism | False sense of throughput; DB lock contention |

**Fix (priority order):**

1. Do **not** parallelize on one context — process sequentially on one `_db` **or** create one scope (and one context) **per item**.
2. Inject `IServiceScopeFactory` (or `IDbContextFactory<AppDbContext>`) instead of `AppDbContext` directly into the hosted service.
3. Per item:

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var product = await db.Products.FindAsync(new object[] { item.ProductId }, ct);
// update + SaveChangesAsync on this scope's context only
```

4. Optionally use `IDbContextFactory<AppDbContext>` for explicit `CreateDbContext()` per parallel partition when true parallelism is required.

**Production takeaway:** EF documents explicitly: do not share one `DbContext` across threads. Parallel + injected context is a classic Karat stack of thread safety and DI lifetime traps.

---

#### Q3. (M) After deploying to a database that already has `dbo.Products`, EF queries fail with "Invalid object name 'Product'". The entity and context match this chapter's tutorial shape. Review the context — what naming mistake caused the mismatch, and how do you fix it without renaming the SQL table?

**Answer:** EF Core maps `DbSet<Product> Product` to table name **Product** (singular) by convention — pluralizing uses the **DbSet property name**, not only the entity class name. The existing database table is `Products`, so generated SQL targets the wrong object.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Model mapping | Singular DbSet property `Product` | SQL queries `dbo.Product` instead of `dbo.Products` |
| Convention | Assumed class name alone drives table name | Mismatch when property name differs from table |
| Runtime | No compile error — fails at first query | Deploy-time surprise after schema already exists |

**Fix (priority order):**

1. Rename the property to plural `Products` (matches this chapter's `AppDbContext`: `public DbSet<Product> Products => Set<Product>()`).
2. Or keep the property name and pin the table in `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().ToTable("Products");
}
```

3. Add an integration test that runs a simple `context.Products.Any()` against the real schema in CI.

**Production takeaway:** DbSet **property naming** drives default table names — Karat tests whether you know convention details beyond "class Product maps to Products." Full fluent overrides → ch.07 Fluent API & Data Annotations.

---

#### Q4. (R) A developer copies the tutorial's `OnConfiguring` fallback into production but removes the `IsConfigured` guard so LocalDB always works in dev. DI registration supplies the real connection string from `appsettings.json`. Review the context — what breaks in staging/prod, and what is the correct split between `OnConfiguring` and `AddDbContext`?

**Answer:** Without `if (!optionsBuilder.IsConfigured)`, `OnConfiguring` overwrites provider settings every time the context is constructed — the hard-coded LocalDB string wins over `AddDbContext`'s configuration, so staging and production connect to the wrong server (or fail entirely on Linux/containers where LocalDB does not exist).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Configuration | Unconditional `UseSqlServer(LocalDB...)` in `OnConfiguring` | Ignores `appsettings.json` / environment connection strings |
| Hosting | LocalDB connection text in deployed binaries | Startup or query failures on non-Windows hosts |
| Security / ops | Connection secrets embedded in context class | Cannot rotate credentials via configuration |
| Design | Mixing demo fallback with production DI path | Environment-specific bugs that pass on developer machines |

**Fix (priority order):**

1. Restore the guard from this chapter's `AppDbContext`:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
        optionsBuilder.UseSqlServer(ConnectionOptions.LocalDbConnectionString);
}
```

2. In ASP.NET Core, configure **only** in `AddDbContext` using `IConfiguration.GetConnectionString("Default")` — leave `OnConfiguring` empty or dev-only behind the guard.
3. Remove hard-coded connection strings from the context class; use User Secrets / environment variables for local dev when not using DI.
4. Verify with a staging deploy that `context.Database.GetConnectionString()` reflects the configured server.

**Production takeaway:** `OnConfiguring` is a fallback for parameterless construction (tutorial Section 4b), not the primary production path — DI + options constructor is. Karat embeds the missing `IsConfigured` check as the trap.

---

#### Q5. (R) A performance pass switches to `AddDbContextPool` but keeps request-scoped state on the context subclass. Under load, users occasionally see another user's `CurrentUserId` in audit columns. Review the design — what violates pooling rules, and how do you fix it?

**Answer:** `AddDbContextPool` reuses the same context **instance** across requests after reset — instance fields like `CurrentUserId` survive between leases if not cleared, so audit logic stamps the wrong user. Pooling requires the context type to behave as if freshly constructed on every lease.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pooling contract | Mutable instance state (`CurrentUserId`) on pooled context | Cross-request data leak; wrong audit metadata |
| Correctness | `SaveChanges` reads stale `CurrentUserId` | Compliance / security incident under concurrency |
| Performance misconception | Pooling replaces scoped lifetime semantics | Faster startup, not safe for per-request fields on the context |
| Design | Business identity stored on EF context instead of call context | Hard to test; hidden coupling to HTTP user |

**Fix (priority order):**

1. Remove mutable request state from `AppDbContext` — no user id fields on the context class.
2. Pass `CurrentUserId` into repository/service methods, or inject `ICurrentUserService` and read it inside `SaveChanges` from scoped DI (not from a field set once on a pooled instance).
3. If you must set state per request, use non-pooled `AddDbContext`, or implement `IDisposable`/`Reset` patterns only as documented — prefer not pooling stateful contexts.
4. Add a load test asserting audit columns always match the authenticated user.

**Production takeaway:** `AddDbContextPool` is safe only when the context is stateless aside from EF's own reset — Karat uses audit-column bleed to test pooling misuse. Default `AddDbContext` (this chapter's preview) is the safer baseline until you understand pool rules.

---

#### Q6. (P) A console integration test host mirrors this chapter's `DbContextServiceRegistration` but resolves `AppDbContext` directly from the root `ServiceProvider` (no scope). With `ValidateScopes = true`, startup throws; with validation disabled, tests pass but production API calls fail intermittently. Explain both behaviors and show the correct resolution pattern for a scoped DbContext.

**Answer:** `AddDbContext` registers `AppDbContext` as scoped — resolving it from the root `ServiceProvider` creates a captive singleton context when validation is off, or throws `InvalidOperationException` when `ValidateScopes` is true. Scoped services must be resolved inside an `IServiceScope` so each unit of work gets its own context that is disposed with the scope.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI scope | `provider.GetRequiredService<AppDbContext>()` at root | Violates scoped lifetime; one long-lived context |
| Validation | `validateScopes: true` catches misuse at resolve time | Test host fails fast (correct behavior) |
| Production | Validation disabled in tests hides bug | Intermittent disposed-context errors under ASP.NET load |
| Resource | Root-resolved context never disposed per operation | Connection / tracker leak in long-running hosts |

**Fix (priority order):**

1. Always create a scope before resolving — match this chapter's `GetProductsViaDependencyInjection`:

```csharp
public static IReadOnlyList<Product> GetProducts(ServiceProvider provider)
{
    using IServiceScope scope = provider.CreateScope();
    AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    return context.Products.OrderBy(p => p.Id).ToList();
}
```

2. Keep `ValidateScopes = true` in integration tests to mirror ASP.NET Core's default scope validation in Development.
3. In web apps, rely on request scope — controllers get `AppDbContext` injected automatically per request.
4. For manual units of work in console jobs, `using var scope = provider.CreateScope()` per operation.

**Production takeaway:** Same registration shape as ASP.NET Core (`AddDbContext` → scoped) but resolution **must** happen inside a scope — this chapter's Section 8b demonstrates the correct pattern; root resolution is a common Karat trap paired with `ValidateScopes`.

---
