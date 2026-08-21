# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/01. Introduction to Entity Framework Core`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A teammate registers EF Core in an ASP.NET Core API like this and injects `AppDbContext` into a singleton `ProductCacheService` that stores query results in an instance field. Review the setup. What breaks under concurrent traffic, and how do you fix it?

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

---

#### Q2. (D) Your team owns a product catalog microservice: CRUD on `Product` with relationships, schema owned via Code-First migrations, and a separate admin dashboard that runs one hand-tuned SQL report per screen (aggregations, window functions, 500k+ rows, read-only). They want to pick **one** data-access stack for everything. What do you recommend for each path — EF Core, Dapper, or ADO.NET — and why would forcing a single choice hurt?

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

---

#### Q4. (P) A developer adds `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.SqlServer` to the web project, writes `AppDbContext` and entities, then runs `dotnet ef migrations add InitialCreate` from the solution root. The tool reports that no `DbContext` was found or that design-time services cannot be resolved. Explain **design-time vs runtime** EF Core packages and list the fixes in priority order.

---

#### Q5. (M) Production moves from on-prem SQL Server to Azure Database for PostgreSQL. The codebase still references only `Microsoft.EntityFrameworkCore.SqlServer` and calls `UseSqlServer(connectionString)` inside `OnConfiguring`. Entity classes and LINQ queries are unchanged. What must change for **provider selection**, and what still requires manual verification after the swap?

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
