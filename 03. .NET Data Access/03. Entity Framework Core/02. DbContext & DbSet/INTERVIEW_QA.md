# 02. DbContext & DbSet — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q9. What is a `DbContext` in EF Core?](#q9-what-is-a-dbcontext-in-ef-core)
- [Q10. What is a `DbSet<T>`?](#q10-what-is-a-dbsett)
- [Q11. How do you register `DbContext` in ASP.NET Core dependency injection?](#q11-how-do-you-register-dbcontext-in-aspnet-core-dependency-injection)
- [Q12. Why is `DbContext` typically registered as scoped?](#q12-why-is-dbcontext-typically-registered-as-scoped)
- [Q13. What is the difference between injecting `DbContext` and using `IDbContextFactory<TContext>`?](#q13-what-is-the-difference-between-injecting-dbcontext-and-using-idbcontextfactorytcontext)
- [Q14. What does `OnModelCreating` do in a `DbContext`?](#q14-what-does-onmodelcreating-do-in-a-dbcontext)
- [Q15. What is `EnsureCreated`, and how does it differ from migrations?](#q15-what-is-ensurecreated-and-how-does-it-differ-from-migrations)
- [Q16. Can you reuse one `DbContext` across multiple threads?](#q16-can-you-reuse-one-dbcontext-across-multiple-threads)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 02. DbContext & DbSet

---

## Q9. What is a `DbContext` in EF Core?

**Concepts**
- primary session with the database
- unit-of-work per request
- DbSet<T> entry points
- short-lived and scoped lifetime

**Answer**

`DbContext` is the primary session with the database in EF Core — it coordinates querying, change tracking, and saving through a configured model. Each instance represents a unit of work for a logical operation such as one HTTP request. Exposes `DbSet<T>` properties as entry points for entity operations; `OnModelCreating` and `OnConfiguring` define mapping and provider options. `SaveChanges` and `SaveChangesAsync` persist tracked changes in a single transaction by default. Should be short-lived and disposed to release connections back to the pool.

---

## Q10. What is a `DbSet<T>`?

**Concepts**
- typed entity collection per table
- IQueryable<T> implementation
- Add/Update/Remove state marking
- change tracker integration

**Answer**

A `DbSet<T>` represents a collection of entities of type `T` mapped to a database table or view and exposes LINQ query methods plus add, update, and remove operations. It is the typed gateway for querying and mutating a specific entity type through the context. `context.Products` returns `DbSet<Product>` for LINQ: `Where`, `Include`, `AsNoTracking`; `Add`, `AddRange`, `Update`, `Remove`, and `RemoveRange` mark entities for `SaveChanges`. `DbSet` implements `IQueryable<T>`, so LINQ providers translate chained methods before execution. Under the hood, EF Core tracks entities added or queried through the context's change tracker.

---

## Q11. How do you register `DbContext` in ASP.NET Core dependency injection?

**Concepts**
- AddDbContext<T> DI registration
- connection string from IConfiguration
- design-time factory for CLI
- provider options configuration

**Answer**

Call `services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString))` in `Program.cs` (or `Startup.cs` on older templates). Inject `AppDbContext` into controllers, services, or minimal API handlers through constructor injection. Connection string comes from configuration: `builder.Configuration.GetConnectionString("Default")`; Provider-specific options configure retries, split queries, and command timeouts. Design-time factory (`IDesignTimeDbContextFactory`) supports migrations CLI without running the web host. Register related interceptors and `DbContext` options in the same configuration delegate when needed.

---

## Q12. Why is `DbContext` typically registered as scoped?

**Concepts**
- non-thread-safe single-unit-of-work design
- per-request scoped lifetime
- singleton and transient lifetime risks
- SaveChanges boundary alignment

**Answer**

`DbContext` is not thread-safe and caches state for one logical unit of work, so it aligns with ASP.NET Core's per-request scope. A scoped registration creates one context instance per HTTP request, shared by all services in that request, then disposes it when the request completes. Singleton registration would share tracked entities across requests — incorrect data and thread-safety violations; Transient registration works but creates multiple contexts per request, breaking a single unit of work and wasting connections. Scoped lifetime matches `SaveChanges` boundaries: one request, one context, one commit pattern. Background workers use `IDbContextFactory<TContext>` to create short-lived contexts outside request scope.

---

## Q13. What is the difference between injecting `DbContext` and using `IDbContextFactory<TContext>`?

**Concepts**
- scoped injection for single request context
- IDbContextFactory for parallel and singleton use
- captive dependency prevention
- explicit disposal in factory pattern

**Answer**

Injecting `DbContext` directly gives the request-scoped instance managed by dependency injection. `IDbContextFactory<TContext>` creates fresh `DbContext` instances on demand, which is required for singleton services, parallel work, or long-running background tasks. Register factory with `AddDbContextFactory<AppDbContext>` alongside or instead of scoped `AddDbContext`; Factory-created contexts must be disposed explicitly: `await using var context = await factory.CreateDbContextAsync()`. Avoid storing injected scoped `DbContext` in singleton fields — captive dependency anti-pattern. Factory supports multi-threaded scenarios where each thread needs its own isolated context.

---

## Q14. What does `OnModelCreating` do in a `DbContext`?

**Concepts**
- Fluent API configuration entry point
- key, relationship, and index mapping
- ApplyConfigurationsFromAssembly
- Fluent API precedence over annotations

**Answer**

`OnModelCreating` is the override where you configure the entity model using the Fluent API — keys, relationships, indexes, column types, and table names — beyond what conventions infer from property names. EF Core calls it once when building the model for that context type. `modelBuilder.Entity<Product>().HasKey(p => p.Sku)` sets explicit keys; `ApplyConfigurationsFromAssembly` loads `IEntityTypeConfiguration<T>` classes for modular mapping. Configuration here merges with data annotations; Fluent API wins conflicts on the same facet. Changes require a new migration in code-first workflows to update the database schema.

---

## Q15. What is `EnsureCreated`, and how does it differ from migrations?

**Concepts**
- EnsureCreated for throwaway schema creation
- migration history table absence
- prototype vs production use case
- incompatibility with existing migrations

**Answer**

`Database.EnsureCreated()` creates the database and schema from the current model if they do not exist, without using the migrations history table. It is intended for prototypes and tests — it cannot upgrade an existing schema when the model changes and should not replace migrations in production. Migrations apply incremental, versioned changes and support rollbacks via `Down` methods; `EnsureCreated` skips migration history — combining both on the same database causes conflicts. `EnsureCreatedAsync` exists for async bootstrap in test fixtures. Production EF Core 8 applications rely on `dotnet ef database update` or scripted migration bundles instead.

---

## Q16. Can you reuse one `DbContext` across multiple threads?

**Concepts**
- non-thread-safe change tracker
- per-operation context isolation requirement
- IDbContextFactory for parallel work
- concurrency corruption risk

**Answer**

No. `DbContext` is not thread-safe — concurrent operations on the same instance cause undefined behavior, corrupted change tracking, and intermittent exceptions. Each parallel task needs its own context instance, typically from `IDbContextFactory<TContext>`. ASP.NET Core handles concurrency by scoping one context per request on a single thread; Parallel LINQ over one `DbSet` with shared context is unsafe even for read-only queries. Use separate contexts and merge results in memory, or serialize database access per context. Thread safety applies to all context operations including `SaveChanges`, queries, and explicit loading.

---

## Gotchas — DbContext & DbSet (Interview Traps)

---

#### Gotcha 1. Registering DbContext as singleton — thread-safety and lifetime violations

**Concepts**
- `DbContext` is not thread-safe — singleton causes concurrent modification
- `AddDbContext` defaults to `ServiceLifetime.Scoped` — do not override with singleton
- tracked entities from one request visible to next request on same context
- connection pool exhaustion from one long-lived context holding one connection
- `IDbContextFactory<TContext>` for singleton services needing occasional DB access

**Answer**

Registering `DbContext` as a singleton violates its design contract — `DbContext` is not thread-safe, so concurrent requests sharing one instance corrupt the change tracker and produce unpredictable exceptions or cross-user data leaks. `AddDbContext` defaults to `ServiceLifetime.Scoped` (one instance per HTTP request), which is the correct lifetime. Never pass `ServiceLifetime.Singleton` to `AddDbContext`; if a singleton service needs database access, inject `IDbContextFactory<TContext>` and create and dispose short-lived context instances on demand.

---

#### Gotcha 2. Scoped DbContext captured in a singleton field — captive dependency

**Concepts**
- singleton service holding scoped `DbContext` — captive dependency
- scoped context disposed at end of first request while singleton lives on
- `ObjectDisposedException: Cannot access a disposed context`
- `IDbContextFactory<TContext>` as the correct singleton pattern
- long-lived background service as a common offender

**Answer**

A singleton service that injects and stores a scoped `DbContext` in a field will receive the context disposed at the end of the first HTTP request — all subsequent calls throw `ObjectDisposedException`. ASP.NET Core's DI container produces a warning for captive dependencies but does not prevent them. For background services and singletons that need periodic database access, inject `IDbContextFactory<TContext>` and call `factory.CreateDbContext()` inside each operation, disposing the context when the operation completes.

---

#### Gotcha 3. `OnConfiguring` hardcodes connection string — overrides DI registration

**Concepts**
- `OnConfiguring` called on every context instance construction
- hardcoded `UseSqlServer` in `OnConfiguring` ignores DI-registered options
- `DI-registered options` passed via `AddDbContext` silently replaced
- integration test uses production database instead of test database
- guard: `if (!optionsBuilder.IsConfigured)` before applying defaults

**Answer**

Overriding `OnConfiguring` in a `DbContext` subclass with a hardcoded `optionsBuilder.UseSqlServer(...)` call silently overrides the options registered via `AddDbContext` in `Program.cs`. Integration tests that register a different connection string via DI still use the hardcoded production database because `OnConfiguring` runs after DI option injection and overwrites it. Add the guard `if (!optionsBuilder.IsConfigured)` before any hardcoded configuration so that DI-provided options take precedence over defaults.

---

#### Gotcha 4. `DbSet<T>.Local` does not query the database — tracks only in-memory entities

**Concepts**
- `DbSet<T>.Local` returns only entities currently tracked in the context
- entities not yet loaded are absent from `Local` even if they exist in DB
- `Find(key)` checks `Local` first, then queries the database
- `DbSet.Local` useful for checking pending adds before `SaveChanges`
- common mistake: treating `Local.Count` as database row count

**Answer**

`DbSet<T>.Local` is an `ObservableCollection<T>` of entities currently tracked by the `ChangeTracker` — it does not issue a database query. An entity that exists in the database but has not been loaded by a prior query is absent from `Local`. Code that checks `context.Products.Local.Count` or iterates `Local` expecting all database rows will see only the entities already loaded in the current context scope. Use `Local` only to inspect or manipulate the in-context tracked state, not as a substitute for a database query.

---

#### Gotcha 5. `Database.EnsureCreated` and migrations are incompatible

**Concepts**
- `EnsureCreated` creates schema without recording migration history
- subsequent `MigrateAsync` attempts to apply all migrations including `InitialCreate`
- schema already exists — migration fails with table already exists error
- `EnsureCreated` appropriate only for ephemeral test databases
- production must always use `MigrateAsync` or migration bundle scripts

**Answer**

`Database.EnsureCreated()` creates the database schema from the current model snapshot without recording any entries in `__EFMigrationsHistory`. If you then run `dotnet ef database update`, EF Core sees no applied migrations and tries to apply all of them from the beginning — `InitialCreate` fails because the tables already exist. Use `EnsureCreated` only for ephemeral test databases that are dropped and recreated per test run. All other environments must use `MigrateAsync()` or the migration bundle/script tooling.

---

#### Gotcha 6. Multiple `SaveChanges` calls in one request — separate commits, not one unit of work

**Concepts**
- each `SaveChanges` issues a separate database commit
- two `SaveChanges` calls are not atomic — second can fail after first commits
- first commit's changes persist if second call throws
- one `SaveChanges` per unit of work as the design principle
- explicit `BeginTransactionAsync` + `CommitAsync` for true atomicity

**Answer**

Calling `SaveChanges` twice in one request handler commits changes to the database in two separate transactions. If the second call fails, the first commit is already durable — you have a partial state with no rollback path. The correct design is one `SaveChanges` per unit of work, accumulating all changes on the tracked context before the single commit. When two separate operations must be atomic together, wrap both in an explicit `await using var tx = await context.Database.BeginTransactionAsync()` and `await tx.CommitAsync()`.

---

#### Gotcha 7. Long-lived DbContext accumulates tracked entities — memory growth

**Concepts**
- every entity materialized by the context is tracked until disposal
- long-lived context grows tracked entity count without bound
- `ChangeTracker.Clear()` (EF Core 5+) to detach all tracked entities
- `AsNoTracking()` on read queries to avoid tracking read entities
- context lifetime should match unit-of-work lifetime

**Answer**

A `DbContext` instance tracks every entity it materializes in its `ChangeTracker` for the duration of the context's lifetime. A long-lived context (e.g., one held for minutes in a background job) accumulates tracked entities indefinitely, causing memory growth and increasingly slow `SaveChanges` calls as change detection must inspect a larger and larger tracked entity graph. Call `context.ChangeTracker.Clear()` after each logical unit of work in long-lived contexts, or use `AsNoTracking()` for read operations that do not need change tracking.

---

#### Gotcha 8. `context.Entry(entity).State = EntityState.Modified` marks all properties dirty

**Concepts**
- setting `EntityState.Modified` marks every property as modified
- UPDATE generated includes all columns, not just changed ones
- concurrency conflict risk from overwriting unchanged columns
- EF Core `Update(entity)` is equivalent — same behavior
- load entity first or use `SetProperty` (EF Core 7+) for targeted UPDATE

**Answer**

Setting `context.Entry(entity).State = EntityState.Modified` (or calling `context.Update(entity)`) marks every property of the entity as modified. The generated `UPDATE` statement includes all columns, overwriting values in the database even if those columns were not changed by the application. This can cause lost-update issues in concurrent systems where another process updated a different column between your read and write. Load the entity with `Find` or `FirstOrDefaultAsync`, mutate only the properties that changed, and call `SaveChanges` to generate a minimal UPDATE.

---

#### Gotcha 9. Transient DbContext from `AddTransient` creates a new context per injection

**Concepts**
- `AddTransient<AppDbContext>` creates a new instance per injection point
- two services in the same request each get separate contexts
- changes saved in one context not visible to the other
- no shared unit of work across transient contexts
- `Scoped` as the correct lifetime for request-level unit of work

**Answer**

Registering `DbContext` as transient (using `AddTransient`) creates a separate context instance for every service that injects it. Two services in the same request each operate on independent change trackers — changes tracked in one context are invisible to the other, and a `SaveChanges` in one does not include changes from the other. This breaks the unit-of-work pattern. Use the default `Scoped` lifetime so all services within a single HTTP request share one context and one unit of work.

---

#### Gotcha 10. `DbSet.Add()` on an already-tracked entity — duplicate key or duplicate tracking exception

**Concepts**
- `Add` marks entity as `EntityState.Added` regardless of prior tracking
- double-tracking causes `InvalidOperationException: entity already being tracked`
- `Attach` + `State.Modified` for pre-existing entities
- `Find()` to check if entity is already in `Local` before `Add`
- EF Core identity map: second load of same key returns tracked instance

**Answer**

Calling `context.Products.Add(entity)` on an entity that is already tracked by the context (e.g., loaded by an earlier query in the same request) throws `InvalidOperationException: The instance of entity type already has a key value and is being tracked`. For pre-existing entities you want to update, use `context.Products.Update(entity)` or load the entity with `Find`/`FirstOrDefault` and mutate the properties directly. `Add` is only for genuinely new entities with no matching key in the database.

---

## Scenario-Based Questions (Karat Format)

---

## Q96. (R) A teammate registers EF Core in an ASP.NET Core API like this and injects `AppDbContext` into a singleton `ProductCacheService` that stores query results in an instance field. Review the setup. What breaks under concurrent traffic, and how do you fix it?

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")),
    ServiceLifetime.Singleton);

builder.Services.AddSingleton<ProductCacheService>();

// ProductCacheService.cs
public sealed class ProductCacheService
{
    private readonly AppDbContext _db;
    private List<Product>? _cached;

    public ProductCacheService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct) =>
        _cached ??= await _db.Products.Where(p => p.DiscontinuedDate == null).ToListAsync(ct);
}
```

**Concepts**
- DbContext singleton registration risk
- captive dependency in singleton service
- scoped lifetime requirement
- IMemoryCache DTO caching pattern

**Answer**

`DbContext` is not thread-safe and must be scoped per request (or per unit of work), not registered as a singleton. Sharing one instance across concurrent HTTP requests causes change-tracker corruption, stale data, and cross-request state leakage — and caching query results on a singleton service mixes every caller's view of the catalog.

1. Register with default **scoped** lifetime: `builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(...));` — omit the third parameter or pass `ServiceLifetime.Scoped`.
2. If caching is required, inject `IMemoryCache` or a distributed cache into a **singleton** service and store **DTOs**, not `DbContext` or tracked entities — load through a scoped factory or `IServiceScopeFactory.CreateScope()` per refresh.
3. Keep `ProductCacheService` singleton only if it is stateless regarding EF; resolve `AppDbContext` inside a short-lived scope when refreshing cache data.
4. Enable scope validation in development (`builder.Services.ValidateScopes = true`) to catch captive scoped dependencies early.

---

## Q101. (R) Review this `AppDbContext` and `Program.cs` fragment from a web app that "works locally" but ignores environment-specific connection strings in deployed environments:

```csharp
public sealed class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Trusted_Connection=True;");
    }
}

// Program.cs
builder.Services.AddDbContext<AppDbContext>();
```

**Concepts**
- OnConfiguring with hardcoded connection string
- IsConfigured guard missing
- AddDbContext options lambda as production path
- environment-specific connection string flow

**Answer**

The parameterless constructor plus hardcoded LocalDB fallback in `OnConfiguring` fights DI-based configuration: design-time tools and accidental `new AppDbContext()` silently hit LocalDB, while deployed environments that expect `AddDbContext` lambda configuration may never use the intended connection string if options are not wired correctly. Production apps should use a single options-injected constructor and configure the provider only in composition root.

1. Remove the parameterless constructor and hardcoded connection string from production code paths.
2. Register explicitly:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

3. Keep `AppDbContext(DbContextOptions<AppDbContext> options) : base(options)` only — matches this chapter's `Data/AppDbContext.cs` preview.
4. For `dotnet ef`, add `IDesignTimeDbContextFactory<AppDbContext>` that reads configuration — do not embed secrets in source.
5. In tests, pass `UseInMemoryDatabase(Guid.NewGuid().ToString())` or Testcontainers via `DbContextOptionsBuilder` — never depend on LocalDB fallback.

---

## Q102. (R) A teammate "optimizes" startup by registering `AppDbContext` as a **Singleton** and injecting it into controllers. The API passes smoke tests locally but corrupts data under concurrent load. Review the registration and usage — what is wrong, and how do you fix it?

```csharp
// Program.cs
builder.Services.AddSingleton<AppDbContext>(sp =>
{
    var options = sp.GetRequiredService<DbContextOptions<AppDbContext>>();
    return new AppDbContext(options);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

```csharp
// ProductController.cs
public class ProductController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetInStock()
    {
        return await _db.Products
            .Where(p => p.StockQuantity > 0)
            .ToListAsync();
    }
}
```

**Concepts**
- singleton DbContext registration error
- concurrent change-tracker corruption
- duplicate registration confusion
- correct scoped AddDbContext pattern

**Answer**

`DbContext` is not thread-safe and must represent one unit of work per scope — registering it as a singleton shares one change tracker and connection semantics across all requests, which causes cross-request state bleed and race conditions under concurrency. The snippet also registers `AddDbContext` twice (singleton factory plus default scoped registration), creating ambiguous DI resolution.

1. Remove the `AddSingleton<AppDbContext>` registration entirely.
2. Keep only `services.AddDbContext<AppDbContext>(...)` — EF registers the context as **scoped** (one per HTTP request).
3. Inject `AppDbContext` into controllers/transient services within the request scope; never store it on singleton fields.
4. For background work, create an `IServiceScope` per job and resolve a fresh context inside that scope.

---

## Q103. (R) A background job processes a price-update queue with `Parallel.ForEachAsync`, reusing one injected `AppDbContext`. Review this worker — what breaks under concurrency, and what lifetime pattern replaces it?

```csharp
public sealed class PriceUpdateWorker : BackgroundService
{
    private readonly AppDbContext _db;
    private readonly IPriceQueue _queue;

    public PriceUpdateWorker(AppDbContext db, IPriceQueue queue)
    {
        _db = db;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Parallel.ForEachAsync(
            _queue.ReadAll(stoppingToken),
            stoppingToken,
            async (item, ct) =>
            {
                var product = await _db.Products.FindAsync(new object[] { item.ProductId }, ct);
                if (product is null) return;
                product.UnitPrice = item.NewPrice;
                await _db.SaveChangesAsync(ct);
            });
    }
}
```

**Concepts**
- Parallel.ForEachAsync with shared DbContext
- thread safety violation in background service
- IDbContextFactory per parallel item
- captive scoped context in singleton worker

**Answer**

A single `DbContext` instance must not be used concurrently from multiple threads — `Parallel.ForEachAsync` violates that rule, so EF Core's change tracker and internal state can corrupt updates or throw inconsistent exceptions. The worker also captures a scoped context in a singleton `BackgroundService`, which is a captive dependency that may be disposed before the host shuts down.

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

---

## Q104. (M) After deploying to a database that already has `dbo.Products`, EF queries fail with "Invalid object name 'Product'". The entity and context match this chapter's tutorial shape. Review the context — what naming mistake caused the mismatch, and how do you fix it without renaming the SQL table?

```csharp
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Product { get; set; } = null!; // singular property name

    // no OnModelCreating override
}
```

```csharp
public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
```

**Concepts**
- DbSet property name driving table name convention
- singular vs plural DbSet property
- ToTable Fluent API override
- convention detail beyond class-name mapping

**Answer**

EF Core maps `DbSet<Product> Product` to table name Product (singular) by convention — pluralizing uses the DbSet property name, not only the entity class name. The existing database table is `Products`, so generated SQL targets the wrong object.

1. Rename the property to plural `Products` (matches this chapter's `AppDbContext`: `public DbSet<Product> Products => Set<Product>()`).
2. Or keep the property name and pin the table in `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().ToTable("Products");
}
```

3. Add an integration test that runs a simple `context.Products.Any()` against the real schema in CI.

---

## Q105. (R) A developer copies the tutorial's `OnConfiguring` fallback into production but removes the `IsConfigured` guard so LocalDB always works in dev. DI registration supplies the real connection string from `appsettings.json`. Review the context — what breaks in staging/prod, and what is the correct split between `OnConfiguring` and `AddDbContext`?

```csharp
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true");
    }
}
```

```csharp
// Program.cs (ASP.NET Core)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

**Concepts**
- OnConfiguring unconditional UseSqlServer override
- IsConfigured guard requirement
- AddDbContext as single composition root
- environment connection string precedence

**Answer**

Without `if (!optionsBuilder.IsConfigured)`, `OnConfiguring` overwrites provider settings every time the context is constructed — the hard-coded LocalDB string wins over `AddDbContext`'s configuration, so staging and production connect to the wrong server (or fail entirely on Linux/containers where LocalDB does not exist).

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

---

## Q106. (R) A performance pass switches to `AddDbContextPool` but keeps request-scoped state on the context subclass. Under load, users occasionally see another user's `CurrentUserId` in audit columns. Review the design — what violates pooling rules, and how do you fix it?

```csharp
public sealed class AppDbContext : DbContext
{
    public int CurrentUserId { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<Product>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedByUserId = CurrentUserId;
        }
        return base.SaveChanges();
    }
}
```

```csharp
// Program.cs
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")),
    poolSize: 128);
```

**Concepts**
- AddDbContextPool with mutable instance state
- CurrentUserId bleed between pooled requests
- stateless context requirement for pooling
- ICurrentUserService injection alternative

**Answer**

`AddDbContextPool` reuses the same context instance across requests after reset — instance fields like `CurrentUserId` survive between leases if not cleared, so audit logic stamps the wrong user. Pooling requires the context type to behave as if freshly constructed on every lease.

1. Remove mutable request state from `AppDbContext` — no user id fields on the context class.
2. Pass `CurrentUserId` into repository/service methods, or inject `ICurrentUserService` and read it inside `SaveChanges` from scoped DI (not from a field set once on a pooled instance).
3. If you must set state per request, use non-pooled `AddDbContext`, or implement `IDisposable`/`Reset` patterns only as documented — prefer not pooling stateful contexts.
4. Add a load test asserting audit columns always match the authenticated user.

---

## Q107. (P) A console integration test host mirrors this chapter's `DbContextServiceRegistration` but resolves `AppDbContext` directly from the root `ServiceProvider` (no scope). With `ValidateScopes = true`, startup throws; with validation disabled, tests pass but production API calls fail intermittently. Explain both behaviors and show the correct resolution pattern for a scoped DbContext.

```csharp
public static class DbContextServiceRegistration
{
    public static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        return services.BuildServiceProvider(validateScopes: true);
    }

    public static IReadOnlyList<Product> GetProducts(ServiceProvider provider)
    {
        AppDbContext context = provider.GetRequiredService<AppDbContext>();
        return context.Products.OrderBy(p => p.Id).ToList();
    }
}
```

**Concepts**
- root ServiceProvider scope violation
- ValidateScopes detection of captive context
- IServiceScope creation before resolution
- per-operation scope disposal pattern

**Answer**

`AddDbContext` registers `AppDbContext` as scoped — resolving it from the root `ServiceProvider` creates a captive singleton context when validation is off, or throws `InvalidOperationException` when `ValidateScopes` is true. Scoped services must be resolved inside an `IServiceScope` so each unit of work gets its own context that is disposed with the scope.

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
