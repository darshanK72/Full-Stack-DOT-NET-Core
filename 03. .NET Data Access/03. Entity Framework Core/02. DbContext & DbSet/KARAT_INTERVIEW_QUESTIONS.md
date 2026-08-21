# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/02. DbContext & DbSet`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A teammate "optimizes" startup by registering `AppDbContext` as a **Singleton** and injecting it into controllers. The API passes smoke tests locally but corrupts data under concurrent load. Review the registration and usage — what is wrong, and how do you fix it?

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

---

#### Q2. (R) A background job processes a price-update queue with `Parallel.ForEachAsync`, reusing one injected `AppDbContext`. Review this worker — what breaks under concurrency, and what lifetime pattern replaces it?

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

---

#### Q3. (M) After deploying to a database that already has `dbo.Products`, EF queries fail with "Invalid object name 'Product'". The entity and context match this chapter's tutorial shape. Review the context — what naming mistake caused the mismatch, and how do you fix it without renaming the SQL table?

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

---

#### Q4. (R) A developer copies the tutorial's `OnConfiguring` fallback into production but removes the `IsConfigured` guard so LocalDB always works in dev. DI registration supplies the real connection string from `appsettings.json`. Review the context — what breaks in staging/prod, and what is the correct split between `OnConfiguring` and `AddDbContext`?

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

---

#### Q5. (R) A performance pass switches to `AddDbContextPool` but keeps request-scoped state on the context subclass. Under load, users occasionally see another user's `CurrentUserId` in audit columns. Review the design — what violates pooling rules, and how do you fix it?

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

---

#### Q6. (P) A console integration test host mirrors this chapter's `DbContextServiceRegistration` but resolves `AppDbContext` directly from the root `ServiceProvider` (no scope). With `ValidateScopes = true`, startup throws; with validation disabled, tests pass but production API calls fail intermittently. Explain both behaviors and show the correct resolution pattern for a scoped DbContext.

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

---
