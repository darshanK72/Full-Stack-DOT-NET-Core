using System;
using System.Collections.Generic;
using System.Linq;
using DbContextAndDbSet.Data;
using DbContextAndDbSet.Models;
using DbContextAndDbSet.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbContextAndDbSet;

/*
 * TOPIC: DbContext & DbSet - the EF Core session (DbContext) and typed entity collections
 *        (DbSet<T>) that translate LINQ into SQL and track changes until SaveChanges.
 *
 * WHY IT MATTERS:
 *   Every EF Core application centers on a DbContext subclass. You configure the SQL Server
 *   provider once (DbContextOptions), expose tables as DbSet<T>, then query and persist through
 *   that single gateway. Understanding options, connection strings, and DI registration
 *   prevents the most common startup failures before you reach migrations and CRUD.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Product entity POCO mapped by convention
 *   2.  Connection string and DbContextOptionsBuilder
 *   3.  DbContextOptions<TContext> passed to the context constructor
 *   4.  DbContext responsibilities and disposal
 *   5.  DbSet<T> and Set<T>() as the query entry point
 *   6.  OnConfiguring fallback when options are not pre-configured
 *   7.  EnsureCreated bootstrap for LocalDB demo database EfCoreTutorial
 *   8.  AddDbContext DI registration preview (console scope, same shape as ASP.NET Core)
 *   9.  Runnable demo wiring both manual and DI-created contexts
 *
 * CHAPTER MAP:
 *   1.  Product entity POCO              -> Models/Product.cs
 *   2.  Connection string constant       -> Utils/ConnectionOptions.cs
 *   3.  DbContextOptionsBuilder          -> Utils/ConnectionOptions.cs
 *   4.  DbContext role                   -> Data/AppDbContext.cs
 *   5.  DbSet<T> and Set<T>()            -> Data/AppDbContext.cs
 *   6.  Options constructor injection    -> Data/AppDbContext.cs
 *   4b. OnConfiguring fallback           -> Data/AppDbContext.cs
 *   7.  EnsureCreated bootstrap          -> Utils/EfCoreTutorialBootstrap.cs
 *   8.  AddDbContext DI preview          -> Utils/DbContextServiceRegistration.cs
 *   9.  Demonstration                    -> Program.cs Main (below)
 *
 * DATABASE: LocalDB (localdb)\MSSQLLocalDB, database EfCoreTutorial.
 * Main calls EfCoreTutorialBootstrap.TryEnsureDatabaseReady (EnsureCreated + seed).
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true
 */
public class Program
{
    /*
     * SECTION 9: DEMONSTRATION - Main orchestrates the chapter demo
     *
     * Flow:
     *   1. Build DbContextOptions and ensure database exists
     *   2. Manual context with injected options (preferred path)
     *   3. Parameterless context (OnConfiguring fallback)
     *   4. DI-resolved scoped context (AddDbContext preview)
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 02. DbContext & DbSet (Entity Framework Core) ===");
        Console.WriteLine();

        DbContextOptions<AppDbContext> options = ConnectionOptions.BuildAppDbContextOptions();

        if (!EfCoreTutorialBootstrap.TryEnsureDatabaseReady(options))
        {
            return;
        }

        Console.WriteLine("--- SECTION 3 & 6: Manual AppDbContext with DbContextOptions ---");
        DemonstrateManualContextWithOptions(options);
        Console.WriteLine();

        Console.WriteLine("--- SECTION 4b: Parameterless AppDbContext (OnConfiguring) ---");
        DemonstrateOnConfiguringFallback();
        Console.WriteLine();

        Console.WriteLine("--- SECTION 8: DI AddDbContext (scoped resolution) ---");
        DemonstrateDependencyInjection();
    }

    private static void DemonstrateManualContextWithOptions(DbContextOptions<AppDbContext> options)
    {
        using AppDbContext context = new AppDbContext(options); // IDisposable - one unit of work

        List<Product> products = context.Products
            .Where(product => product.StockQuantity > 0)
            .OrderBy(product => product.Name)
            .ToList(); // IQueryable executes here as SQL SELECT

        Console.WriteLine($"  DbSet<Product> query returned {products.Count} in-stock product(s):");
        foreach (Product product in products)
        {
            Console.WriteLine($"    {product}");
        }

        Product? firstProduct = context.Set<Product>().OrderBy(product => product.Id).FirstOrDefault();
        if (firstProduct is not null)
        {
            Console.WriteLine($"  Set<Product>().First by Id: {firstProduct.Name} (Id {firstProduct.Id})");
        }
    }

    private static void DemonstrateOnConfiguringFallback()
    {
        using AppDbContext context = new AppDbContext(); // triggers OnConfiguring when not configured

        int count = context.Products.Count(); // COUNT(*) via provider
        Console.WriteLine($"  Parameterless ctor + OnConfiguring: Products.Count() = {count}");
    }

    private static void DemonstrateDependencyInjection()
    {
        ServiceProvider provider = DbContextServiceRegistration.BuildServiceProvider(
            ConnectionOptions.LocalDbConnectionString);

        try
        {
            IReadOnlyList<Product> products =
                DbContextServiceRegistration.GetProductsViaDependencyInjection(provider);

            Console.WriteLine($"  GetRequiredService<AppDbContext>() via scope: {products.Count} product(s)");
            foreach (Product product in products)
            {
                Console.WriteLine($"    {product}");
            }
        }
        finally
        {
            provider.Dispose(); // disposes scoped services created during the demo
        }
    }
}

/*
 * QUICK REFERENCE
 *
 * DbContext
 *   - Subclass DbContext; configure provider via DbContextOptions<T> or OnConfiguring
 *   - One instance ~ one unit of work; dispose with using
 *   - Scoped lifetime in web apps (AddDbContext)
 *
 * DbSet<T>
 *   - public DbSet<Product> Products { get; set; }  OR  => Set<Product>()
 *   - IQueryable<T> for LINQ; Add/Remove stage changes until SaveChanges (ch.05)
 *   - Set<T>() for generic access without a declared property
 *
 * DbContextOptions / DbContextOptionsBuilder
 *   new DbContextOptionsBuilder<AppDbContext>()
 *       .UseSqlServer(connectionString)
 *       .Options;
 *
 * DI (console / ASP.NET same registration shape)
 *   services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn));
 *   using var scope = provider.CreateScope();
 *   var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
 *
 * Demo database
 *   context.Database.EnsureCreated();  // no migrations - see ch.03 for Add-Migration
 *
 * Forward references
 *   Migrations, model evolution     -> ch.03 Code-First Models & Migrations
 *   SaveChanges, entity state         -> ch.05 CRUD Operations & SaveChanges
 *   Fluent API, data annotations      -> ch.07 Fluent API & Data Annotations
 *   LINQ translation, IQueryable      -> ch.08 LINQ to Entities & Query Patterns
 *   ASP.NET host + appsettings.json   -> Web API modules
 */
