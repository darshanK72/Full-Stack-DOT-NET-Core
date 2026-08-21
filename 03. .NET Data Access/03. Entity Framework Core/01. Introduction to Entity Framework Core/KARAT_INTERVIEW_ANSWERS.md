# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/01. Introduction to Entity Framework Core`

---

#### Q1. (R) A teammate registers EF Core in an ASP.NET Core API like this and injects `AppDbContext` into a singleton `ProductCacheService` that stores query results in an instance field. Review the setup. What breaks under concurrent traffic, and how do you fix it?

**Answer:** `DbContext` is not thread-safe and must be scoped per request (or per unit of work), not registered as a singleton. Sharing one instance across concurrent HTTP requests causes change-tracker corruption, stale data, and cross-request state leakage — and caching query results on a singleton service mixes every caller's view of the catalog.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `AddDbContext` with `ServiceLifetime.Singleton` | One `DbContext` shared by all requests — not thread-safe |
| Architecture | Singleton `ProductCacheService` holds `DbContext` + mutable `_cached` | Captive dependency; tracker state and cache bleed across users |
| Correctness | Concurrent reads/writes on same context instance | Intermittent exceptions, wrong entities attached, flaky tests |
| Scale-out | In-memory `_cached` on singleton | Stale catalog until restart; not coherent across multiple pods |

**Fix (priority order):**

1. Register with default **scoped** lifetime: `builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(...));` — omit the third parameter or pass `ServiceLifetime.Scoped`.
2. If caching is required, inject `IMemoryCache` or a distributed cache into a **singleton** service and store **DTOs**, not `DbContext` or tracked entities — load through a scoped factory or `IServiceScopeFactory.CreateScope()` per refresh.
3. Keep `ProductCacheService` singleton only if it is stateless regarding EF; resolve `AppDbContext` inside a short-lived scope when refreshing cache data.
4. Enable scope validation in development (`builder.Services.ValidateScopes = true`) to catch captive scoped dependencies early.

**Production takeaway:** Karat uses DbContext lifetime to test whether you know EF is a per-request session, not a shared connection pool wrapper. See this chapter's `AppDbContext` — constructor injection via `DbContextOptions<T>` is the pattern `AddDbContext` expects.

---

#### Q2. (D) Your team owns a product catalog microservice: CRUD on `Product` with relationships, schema owned via Code-First migrations, and a separate admin dashboard that runs one hand-tuned SQL report per screen (aggregations, window functions, 500k+ rows, read-only). They want to pick **one** data-access stack for everything. What do you recommend for each path — EF Core, Dapper, or ADO.NET — and why would forcing a single choice hurt?

**Answer:** Use EF Core for the catalog CRUD and schema evolution path, and Dapper (or targeted raw SQL through EF's escape hatches) for the heavy read-only reports — not ADO.NET unless you need provider-specific streaming APIs Dapper cannot cover. Forcing one stack either sacrifices migration/change-tracking productivity on writes or accepts poor SQL and overhead on report queries.

- **Catalog CRUD + relationships + migrations:** EF Core — entities map to your domain, `SaveChanges` handles unit-of-work, Code-First migrations keep schema aligned (this chapter's stack comparison; detail in ch03).
- **Admin dashboards / large aggregations:** Dapper — you keep hand-tuned SQL, `Query<T>` maps rows with minimal overhead, no change tracker on 500k rows.
- **ADO.NET:** Reserve for max-control scenarios (manual `SqlDataReader` streaming, bulk copy APIs) when neither EF nor Dapper fits.
- **Hybrid is normal:** Same database, two access paths — EF for writes/domain services, Dapper for read-optimized report endpoints — with clear boundaries so teams do not duplicate business rules in SQL strings.
- **Forcing EF everywhere:** LINQ translation may produce suboptimal plans; materializing large graphs wastes memory; no change tracking needed on read-only reports.
- **Forcing Dapper everywhere:** You reimplement relationship graphs, migration tooling, and optimistic concurrency manually on the CRUD side.

**Production takeaway:** The intro chapter's "when to use" table is a decision guide, not a loyalty oath — senior judgment is picking the right tool per workload while sharing one connection string and schema.

---

#### Q3. (R) CI passes with this "integration" test setup, but staging against SQL Server fails on the same assertions. Review the test harness:

```csharp
public class ProductRepositoryTests : IDisposable
{
    private readonly AppDbContext _db;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ProductTests") // same name for every test class
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public void DeleteParent_cascades_to_child_rows()
    {
        // seed Category + Products, delete Category, assert children removed
    }
}
```

What is wrong with treating In-Memory as a SQL Server substitute here, and what would you change?

**Answer:** The In-Memory provider is an in-process dictionary, not a SQL engine — it does not enforce relational constraints, cascade rules, or T-SQL semantics the way SQL Server does. Sharing one database name across parallel tests also causes cross-test pollution, so green CI does not predict staging behavior.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Provider fidelity | `UseInMemoryDatabase` for cascade/FK behavior | In-Memory ignores or approximates SQL constraints — false positives |
| Test isolation | Fixed name `"ProductTests"` for all classes | Parallel tests share state; order-dependent failures |
| Schema strategy | `EnsureCreated()` instead of migrations against real DB | Skips migration SQL, indexes, and provider-specific DDL |
| Mislabeling | Class named "integration" but uses In-Memory | Team trusts CI; production SQL fails on delete rules |

**Fix (priority order):**

1. Use a **unique** In-Memory name per test (`Guid.NewGuid().ToString()`) if you only need fast unit tests of LINQ against `DbContext` — as in this chapter's `InMemoryProviderPreview` (`"EfCoreIntroCh01"`).
2. Move cascade/FK/delete-behavior tests to **SQL Server integration tests** — LocalDB, Testcontainers, or a dedicated CI database — with migrations applied (`Database.Migrate()` or `dotnet ef database update`).
3. Keep In-Memory for pure logic tests (query filters, mapping) where SQL translation differences are irrelevant; never assert provider-specific SQL behavior against In-Memory.
4. Dispose/`using` the context per test and avoid static shared stores.

**Production takeaway:** This chapter explicitly warns that In-Memory is preview/demo-only — Karat tests whether you would ship based on In-Memory "integration" tests. See `Utils/InMemoryProviderPreview.cs` limitations list.

---

#### Q4. (P) A developer adds `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.SqlServer` to the web project, writes `AppDbContext` and entities, then runs `dotnet ef migrations add InitialCreate` from the solution root. The tool reports that no `DbContext` was found or that design-time services cannot be resolved. Explain **design-time vs runtime** EF Core packages and list the fixes in priority order.

**Answer:** Runtime packages (`Microsoft.EntityFrameworkCore`, provider packages) power your app at execution time; design-time tooling needs `Microsoft.EntityFrameworkCore.Design` (and often `dotnet-ef` as a local tool) to construct `DbContext` outside the running host, discover options, and generate migrations. Missing design packages or wrong startup project causes the CLI to fail even when the app runs fine.

- **Runtime (app host):** `Microsoft.EntityFrameworkCore` + `Microsoft.EntityFrameworkCore.SqlServer` (or Npgsql, etc.) — loaded when the API runs; `AddDbContext` and queries use these.
- **Design-time (CLI / VS Package Manager):** `Microsoft.EntityFrameworkCore.Design` — referenced with `PrivateAssets="All"` in the project that owns `DbContext`; supplies `IDesignTimeDbContextFactory` resolution, MSBuild targets for `dotnet ef`.
- **Tooling:** Install `dotnet-ef` as a **local** tool (`dotnet tool install dotnet-ef`) or use PMC `Add-Migration` with Design package present — the global SDK does not include EF commands by default.
- **Startup project:** Run from the project containing `DbContext`, or pass `--project` (migrations project) and `--startup-project` (web app with `Program.cs` and connection string).
- **`IDesignTimeDbContextFactory<TContext>`:** Add when DI-only configuration (no parameterless ctor) prevents the tools from building options — factory builds `DbContextOptions` the same way as `Program.cs`.

**Fix (priority order):**

1. Add `Microsoft.EntityFrameworkCore.Design` to the `DbContext` project (matches version of runtime packages — e.g. 8.0.11 in this chapter's `.csproj`).
2. Install/restore `dotnet-ef` local tool; run `dotnet ef migrations add InitialCreate --project <ContextProject> --startup-project <WebProject>`.
3. Ensure `AppDbContext` is public and in an assembly referenced by the startup project; pass explicit `--context AppDbContext` if multiple contexts exist.
4. If options come only from DI, implement `IDesignTimeDbContextFactory<AppDbContext>` reading `appsettings.json` or env vars — mirrors production registration.

**Production takeaway:** "App runs, migrations fail" is a classic onboarding trap — runtime and design-time are separate deployment concerns; Design is not needed on the server, only in the repo for schema changes. See `EfCoreConcepts.ExplainNuGetSetup` — Design deferred to ch03/ch04.

---

#### Q5. (M) Production moves from on-prem SQL Server to Azure Database for PostgreSQL. The codebase still references only `Microsoft.EntityFrameworkCore.SqlServer` and calls `UseSqlServer(connectionString)` inside `OnConfiguring`. Entity classes and LINQ queries are unchanged. What must change for **provider selection**, and what still requires manual verification after the swap?

**Answer:** Swap the NuGet provider package and the `Use*` extension — replace `Microsoft.EntityFrameworkCore.SqlServer` + `UseSqlServer` with `Npgsql.EntityFrameworkCore.PostgreSQL` + `UseNpgsql` — and update connection strings. LINQ and entities largely stay the same, but dialect differences, migrations, and provider-specific types must be revalidated; you cannot copy SQL Server migration history verbatim.

- **Packages:** Remove `Microsoft.EntityFrameworkCore.SqlServer`; add `Npgsql.EntityFrameworkCore.PostgreSQL` (version aligned with EF Core 8.x).
- **Registration:** Centralize in `AddDbContext` / `DbContextOptionsBuilder` — `options.UseNpgsql(configuration.GetConnectionString("Default"))`; remove hardcoded `UseSqlServer` from `OnConfiguring` when using DI (see Q6).
- **Connection string:** PostgreSQL format (`Host=...;Database=...;Username=...;Password=...`) — often from Azure Key Vault or App Configuration, not LocalDB.
- **Migrations:** Regenerate or baseline a new migration history for PostgreSQL — SQL Server–specific annotations (clustered indexes, `nvarchar`, sequences) do not port automatically.
- **Manual verification:** Raw SQL (ch10), `decimal` precision, `DateTime` kind handling, string collation, JSON/hierarchy columns, and query plans for hot LINQ — providers translate differently (this chapter's provider table: same DbContext surface, different dialect).
- **Tests:** Replace In-Memory or SQL Server–only CI with PostgreSQL Testcontainers or Azure flexible server integration tests before cutover.

**Production takeaway:** Provider swap is a NuGet + extension + connection change at the surface, but production readiness means re-running migrations and performance tests against the target dialect — not assuming LINQ is byte-identical SQL.

---

#### Q6. (R) Review this `AppDbContext` and `Program.cs` fragment from a web app that "works locally" but ignores environment-specific connection strings in deployed environments:

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

What problems does this pattern create for DI, testing, and multi-environment deployment — and what is the production-ready registration shape?

**Answer:** The parameterless constructor plus hardcoded LocalDB fallback in `OnConfiguring` fights DI-based configuration: design-time tools and accidental `new AppDbContext()` silently hit LocalDB, while deployed environments that expect `AddDbContext` lambda configuration may never use the intended connection string if options are not wired correctly. Production apps should use a single options-injected constructor and configure the provider only in composition root.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Configuration | Hardcoded LocalDB in `OnConfiguring` | Staging/prod ignore `appsettings.Production.json` when fallback path runs |
| DI | Parameterless ctor enables `new AppDbContext()` | Bypasses scoped registration; hidden second configuration path |
| Testability | Cannot swap to In-Memory/Npgsql without hitting `OnConfiguring` | Tests accidentally touch LocalDB or duplicate provider setup |
| Design-time | Tools may use parameterless ctor | Migrations generated against wrong server |
| Pattern | `AddDbContext<AppDbContext>()` without options lambda | Relies on `OnConfiguring` — acceptable only if all config lives there intentionally |

**Fix (priority order):**

1. Remove the parameterless constructor and hardcoded connection string from production code paths.
2. Register explicitly:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

3. Keep `AppDbContext(DbContextOptions<AppDbContext> options) : base(options)` only — matches this chapter's `Data/AppDbContext.cs` preview.
4. For `dotnet ef`, add `IDesignTimeDbContextFactory<AppDbContext>` that reads configuration — do not embed secrets in source.
5. In tests, pass `UseInMemoryDatabase(Guid.NewGuid().ToString())` or Testcontainers via `DbContextOptionsBuilder` — never depend on LocalDB fallback.

**Production takeaway:** `DbContextOptions<T>` injection at the composition root is the seam for provider selection, environment connection strings, and test doubles — `OnConfiguring` with LocalDB is tutorial-friendly but becomes a deployment footgun in web apps. Forward reference: ch02 covers `OnConfiguring` vs options injection in depth.
