using System;
using System.Collections.Generic;
using LinqToEntitiesAndQueryPatterns.Data;
using LinqToEntitiesAndQueryPatterns.Models;
using LinqToEntitiesAndQueryPatterns.Services;
using Microsoft.EntityFrameworkCore;

namespace LinqToEntitiesAndQueryPatterns;

/*
 * TOPIC: LINQ to Entities and Query Patterns  -  query SQL Server through EF Core's IQueryable provider.
 *
 * WHY IT MATTERS:
 *   EF Core turns LINQ into SQL. The same Where/Select/OrderBy vocabulary you learned in the
 *   LINQ module now runs on the database  -  but translation rules, deferred execution scope, and
 *   filters like HasQueryFilter behave differently from in-memory IEnumerable queries.
 *
 * PREREQUISITE:
 *   02. C# & LINQ / 05. Language Integrated Query  -  especially ch.01 (deferred execution,
 *   IQueryable vs IEnumerable), ch.02 (Where), ch.08 (Select/projection).
 *   EF Core ch.02 DbContext & DbSet, ch.06 relationships (navigation used in filters/projections).
 *
 * WHAT YOU WILL LEARN:
 *   1.  IQueryable from DbSet  -  expression trees translated to SQL
 *   2.  Deferred execution  -  when queries actually hit the database
 *   3.  Filtering  -  Where, Any, composable search predicates
 *   4.  Projection  -  Select to DTOs, anonymous types, SelectMany
 *   5.  Compiled queries (preview)  -  EF.CompileQuery caching
 *   6.  Global query filters (preview)  -  HasQueryFilter and IgnoreQueryFilters
 *
 * CHAPTER MAP:
 *   1.  Category entity              -> Models/Category.cs
 *   2.  Product entity               -> Models/Product.cs
 *   3.  Order / OrderLine entities   -> Models/Order.cs
 *   4.  Projection DTO               -> Models/ProductListItem.cs
 *   5.  DbContext, seed, global filter -> Data/StoreDbContext.cs
 *   6.  LocalDB connection string    -> Data/LocalDbConnection.cs
 *   7.  IQueryable + deferred exec   -> Services/IQueryableDemonstrations.cs
 *   8.  Filtering patterns           -> Services/FilteringQueries.cs
 *   9.  Projection patterns          -> Services/ProjectionQueries.cs
 *   10. Compiled queries preview     -> Services/CompiledQueryPreview.cs
 *   11. Demonstration                -> Program.cs Main (below)
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreLinqDemo;Integrated Security=true;TrustServerCertificate=true
 */

public class Program
{
    /*
     * SECTION 11: DEMONSTRATION  -  Main orchestrates the chapter demo
     *
     * EnsureCreated builds schema + HasData seed on first run. Services run in reading order.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 08. LINQ to Entities & Query Patterns (EF Core) ===");
        Console.WriteLine();

        DbContextOptions<StoreDbContext> options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseSqlServer(LocalDbConnection.EfCoreLinqDemo)
            .Options;

        try
        {
            using (StoreDbContext bootstrap = new StoreDbContext(options))
            {
                bootstrap.Database.EnsureCreated(); // creates EfCoreLinqDemo + seed on first run
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Database setup skipped  -  start LocalDB and re-run.");
            Console.WriteLine($"  {ex.Message}");
            return;
        }

        Console.WriteLine("EfCoreLinqDemo ready on LocalDB.");
        Console.WriteLine();

        using StoreDbContext context = new StoreDbContext(options);

        DemonstrateIQueryableAndDeferredExecution(context);
        DemonstrateFiltering(context);
        DemonstrateProjection(context);
        DemonstrateCompiledQueryPreview(context);
        DemonstrateGlobalQueryFilterPreview(context);
    }

    private static void DemonstrateIQueryableAndDeferredExecution(StoreDbContext context)
    {
        Console.WriteLine("--- SECTIONS 1-2: IQueryable translation and deferred execution ---");

        IQueryableDemonstrations demos = new IQueryableDemonstrations(context);

        const decimal minPrice = 20m;
        string sqlPreview = demos.ShowTranslatedSql(minPrice);
        Console.WriteLine($"  ToQueryString() before execution (min price ${minPrice:F2}):");
        Console.WriteLine("  " + sqlPreview.Replace(Environment.NewLine, Environment.NewLine + "  "));

        DeferredExecutionResult result = demos.DemonstrateDeferredExecution(minPrice);
        Console.WriteLine($"  Materialized {result.ResultCount} product(s) after ToList() (SQL ran once).");
        foreach (Product product in result.Products)
        {
            Console.WriteLine($"    {product.ProductId}: {product.Name} @ ${product.UnitPrice:F2}");
        }

        Console.WriteLine();
    }

    private static void DemonstrateFiltering(StoreDbContext context)
    {
        Console.WriteLine("--- SECTION 3: Filtering (Where, Any, composable search) ---");

        FilteringQueries queries = new FilteringQueries(context);

        IReadOnlyList<Product> inStock = queries.GetInStockProducts(minimumStock: 50);
        Console.WriteLine($"  In-stock (>= 50 units): {inStock.Count} product(s)");
        foreach (Product product in inStock)
        {
            Console.WriteLine($"    {product.Name}: {product.StockQuantity} unit(s)");
        }

        IReadOnlyList<Order> keyboardOrders = queries.GetOrdersContainingProductNamed("Keyboard");
        Console.WriteLine($"  Orders containing 'Keyboard': {keyboardOrders.Count}");
        foreach (Order order in keyboardOrders)
        {
            Console.WriteLine($"    Order {order.OrderId}  -  {order.CustomerName} ({order.OrderDate:d})");
        }

        IReadOnlyList<Product> electronicsMidRange = queries.SearchProducts(categoryId: 1, minPrice: 25m, maxPrice: 60m);
        Console.WriteLine($"  Electronics between $25-$60: {electronicsMidRange.Count} product(s)");
        foreach (Product product in electronicsMidRange)
        {
            Console.WriteLine($"    {product.Name} @ ${product.UnitPrice:F2}");
        }

        Console.WriteLine();
    }

    private static void DemonstrateProjection(StoreDbContext context)
    {
        Console.WriteLine("--- SECTION 4: Projection (Select, GroupBy summary, SelectMany) ---");

        ProjectionQueries queries = new ProjectionQueries(context);

        IReadOnlyList<ProductListItem> catalog = queries.GetProductCatalogItems();
        Console.WriteLine("  ProductListItem DTO projection:");
        foreach (ProductListItem item in catalog)
        {
            string stockLabel = item.InStock ? "in stock" : "out of stock";
            Console.WriteLine($"    [{item.CategoryName}] {item.Name}  -  ${item.UnitPrice:F2} ({stockLabel})");
        }

        Console.WriteLine();
        Console.WriteLine("  Category stock summary (anonymous type -> string):");
        foreach (string summary in queries.GetCategoryStockSummary())
        {
            Console.WriteLine($"    {summary}");
        }

        Console.WriteLine();
        Console.WriteLine("  Flat order lines (SelectMany):");
        foreach (string line in queries.GetFlatOrderLineDescriptions())
        {
            Console.WriteLine($"    {line}");
        }

        Console.WriteLine();
    }

    private static void DemonstrateCompiledQueryPreview(StoreDbContext context)
    {
        Console.WriteLine("--- SECTION 5: Compiled queries (preview) ---");

        CompiledQueryPreview compiled = new CompiledQueryPreview();
        IReadOnlyList<Product> officeProducts = compiled.GetProductsByCategory(context, categoryId: 2);
        Console.WriteLine($"  EF.CompileQuery  -  Office Supplies: {officeProducts.Count} active product(s)");
        foreach (Product product in officeProducts)
        {
            Console.WriteLine($"    {product.Name}");
        }

        Console.WriteLine($"  {compiled.AsyncCompiledQueryNote}");
        Console.WriteLine();
    }

    private static void DemonstrateGlobalQueryFilterPreview(StoreDbContext context)
    {
        Console.WriteLine("--- SECTION 6: Global query filter (preview) ---");

        FilteringQueries queries = new FilteringQueries(context);
        GlobalFilterComparison comparison = queries.CompareGlobalFilter();

        Console.WriteLine($"  Products with default filter (active only): {comparison.ActiveProductCount}");
        Console.WriteLine($"  Products with IgnoreQueryFilters(): {comparison.TotalIncludingDiscontinued}");
        Console.WriteLine("  Seed includes 2 discontinued SKUs hidden by HasQueryFilter on Product.");
        Console.WriteLine("  See Data/StoreDbContext.cs OnModelCreating for HasQueryFilter definition.");
        Console.WriteLine();
    }
}

/*
 * QUICK REFERENCE  -  LINQ TO ENTITIES & QUERY PATTERNS
 *
 * --- IQueryable from DbSet ---
 *
 *   IQueryable<Product> q = context.Products;
 *   // DbSet<T> : IQueryable<T>  -  provider = EF Core SQL Server
 *
 * --- Translation preview ---
 *
 *   string sql = query.ToQueryString(); // learning/diagnostics only
 *
 * --- Deferred execution ---
 *
 *   var q = context.Products.Where(p => p.UnitPrice > 10); // no SQL yet
 *   var list = q.ToList();                                  // SQL runs here
 *
 * --- LINQ to Objects vs LINQ to Entities ---
 *
 *   IEnumerable<T>  | in-memory (LINQ module)     | delegates run in CLR
 *   IQueryable<T>   | EF Core, IQueryable provider | expression tree -> SQL
 *
 * --- Filtering ---
 *
 *   .Where(p => p.StockQuantity > 0)
 *   .Where(o => o.Lines.Any(l => l.Quantity > 1))
 *   Composable: IQueryable<T> q = set; if (x) q = q.Where(...);
 *
 * --- Projection ---
 *
 *   .Select(p => new ProductListItem { ... })   // named DTO  -  preferred at boundaries
 *   .Select(p => new { p.Name, p.UnitPrice })   // anonymous  -  local reports
 *   .SelectMany(o => o.Lines, (o,l) => ...)     // flatten nested collection
 *
 * --- Global query filter (preview) ---
 *
 *   modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDiscontinued);
 *   context.Products.IgnoreQueryFilters()       // opt out deliberately
 *
 * --- Compiled query (preview) ---
 *
 *   static readonly Func<StoreDbContext, int, IEnumerable<Product>> Q =
 *       EF.CompileQuery((db, id) => db.Products.Where(p => p.CategoryId == id));
 *   var rows = Q(context, categoryId).ToList();
 *
 * --- Common pitfalls ---
 *
 *   Client evaluation        | Some C# methods cannot translate  -  watch logs / exceptions
 *   Disposed context         | Materialize before disposing DbContext
 *   Global filter surprises  | Admin queries may need IgnoreQueryFilters()
 *   N+1 queries              | ch.09  -  Include/ThenInclude for eager loading
 *
 * --- Forward references ---
 *
 *   09. Loading Related Data               -  Include, ThenInclude, explicit loading
 *   11. Change Tracking, Async & Transactions  -  AsNoTracking, SaveChangesAsync
 *   LINQ ch.01 Introduction to LINQ        -  deferred execution, IEnumerable vs IQueryable
 */
