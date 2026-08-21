using System;
using CrudOperationsAndSaveChanges.Data;
using CrudOperationsAndSaveChanges.Models;
using CrudOperationsAndSaveChanges.Services;
using CrudOperationsAndSaveChanges.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CrudOperationsAndSaveChanges;

/*
 * =============================================================================
 * TOPIC: CRUD Operations & SaveChanges ù create, read, update, and delete rows
 *        through DbContext, and flush pending work with SaveChanges. Entity
 *        states (Added, Modified, Deleted, Unchanged) explain what EF will
 *        send to SQL before you call SaveChanges.
 *
 * WHY IT MATTERS:
 *   EF Core hides ADO.NET command text, but you still choose when data is
 *   persisted. Add/Remove/Update only change the in-memory change tracker;
 *   SaveChanges is the commit boundary. Misunderstanding that gap causes
 *   "my update did nothing" bugs and accidental partial saves.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Product entity mapped for CRUD
 *   2.  DbContext and DbSet<T> as the persistence session
 *   3.  CREATE ù DbSet.Add and Added state
 *   4.  READ ù Find / query tracked entities (LINQ preview -> ch08)
 *   5.  UPDATE ù modify tracked properties vs DbSet.Update()
 *   6.  DELETE ù DbSet.Remove and Deleted state
 *   7.  SaveChanges ù unit of work, return value, batching
 *   8.  Entity states ù Added, Modified, Deleted, Unchanged
 *   9.  LocalDB EfCoreTutorial bootstrap with EnsureCreated
 *
 * CHAPTER MAP:
 *   1.  Product entity for CRUD           -> Models/Product.cs
 *   2.  DbContext and DbSet<T>            -> Data/AppDbContext.cs
 *   3.  CREATE ù Add & SaveChanges        -> Services/ProductService.cs
 *   4.  READ ù tracked query (preview)    -> Services/ProductService.cs
 *   5.  UPDATE ù tracked & Update()       -> Services/ProductService.cs
 *   6.  DELETE ù Remove & SaveChanges     -> Services/ProductService.cs
 *   7.  SaveChanges ù unit of work          -> Services/ProductService.cs
 *   8.  Entity states                     -> Utils/EntityStateInspector.cs
 *   9.  LocalDB bootstrap                 -> Utils/EfCoreTutorialBootstrap.cs
 *  10.  Demonstration                      -> Program.cs Main (below)
 *
 * DATABASE: (localdb)\MSSQLLocalDB ù database EfCoreTutorial
 * Connection string -> Utils/ConnectionHelper.cs
 * Main calls EfCoreTutorialBootstrap.EnsureReady() (EnsureCreated + seed).
 * =============================================================================
 */
public class Program
{
    /*
     * SECTION 10: DEMONSTRATION ù Main orchestrates the chapter demo
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 05. CRUD Operations & SaveChanges ===");
        Console.WriteLine();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionHelper.LocalDbConnectionString)
            .Options;

        try
        {
            EfCoreTutorialBootstrap.EnsureReady(options);
            RunCrudDemo(options);
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Database unavailable or LocalDB not installed.");
            Console.WriteLine($"  SqlException: {ex.Message}");
            Console.WriteLine("  Ensure SQL Server LocalDB is available: (localdb)\\MSSQLLocalDB");
        }
    }

    private static void RunCrudDemo(DbContextOptions<AppDbContext> options)
    {
        using var context = new AppDbContext(options);
        var service = new ProductService(context);

        Console.WriteLine("--- After bootstrap: active products ---");
        foreach (Product product in service.GetActiveProducts())
        {
            Console.WriteLine($"  {product}");
        }
        Console.WriteLine();

        Console.WriteLine("--- SECTION 3: CREATE (Add -> Added, then SaveChanges) ---");
        Product newProduct = service.AddProduct("Part G", 4.25m, 200);
        Console.WriteLine($"  Before SaveChanges: {service.DescribeState(newProduct)}");
        Console.WriteLine($"  ProductId before save: {newProduct.ProductId} (0 until store generates key)");
        int inserted = service.SaveChanges();
        Console.WriteLine($"  SaveChanges returned {inserted} state entries written");
        Console.WriteLine($"  After SaveChanges:  {service.DescribeState(newProduct)}");
        Console.WriteLine($"  ProductId after save: {newProduct.ProductId}");
        Console.WriteLine();

        Console.WriteLine("--- SECTION 5: UPDATE tracked (property change -> Modified) ---");
        Product? tracked = service.GetById(newProduct.ProductId);
        if (tracked is not null)
        {
            Console.WriteLine($"  Loaded: {service.DescribeState(tracked)}");
            tracked.UnitPrice = 4.99m; // change tracker marks Modified
            Console.WriteLine($"  After price change: {service.DescribeState(tracked)}");
            Console.WriteLine("  ChangeTracker snapshot:");
            Console.WriteLine(service.DescribeAllTrackedStates());
            int updated = service.SaveChanges();
            Console.WriteLine($"  SaveChanges returned {updated}; state now Unchanged");
            Console.WriteLine($"  {tracked}");
        }
        Console.WriteLine();

        Console.WriteLine("--- SECTION 5b: UPDATE detached via DbSet.Update() ---");
        // Another tracked instance with the same ProductId causes InvalidOperationException on Update().
        context.ChangeTracker.Clear(); // detach all ó attach/update depth -> ch11
        var detachedStub = new Product
        {
            ProductId = newProduct.ProductId,
            ProductName = "Part G (renamed)",
            UnitPrice = 5.50m,
            StockQuantity = 180,
            IsDiscontinued = false
        };
        service.UpdateProductDetached(detachedStub);
        Console.WriteLine($"  After Update(): {service.DescribeState(detachedStub)}");
        service.SaveChanges();
        Console.WriteLine($"  Persisted: {service.GetById(newProduct.ProductId)}");
        Console.WriteLine();

        Console.WriteLine("--- SECTION 6: DELETE (Remove -> Deleted, then SaveChanges) ---");
        Product? toDelete = service.GetById(newProduct.ProductId);
        if (toDelete is not null)
        {
            Console.WriteLine($"  Before Remove: {service.DescribeState(toDelete)}");
            service.RemoveProduct(toDelete);
            Console.WriteLine($"  After Remove:  {service.DescribeState(toDelete)}");
            Console.WriteLine("  ChangeTracker snapshot:");
            Console.WriteLine(service.DescribeAllTrackedStates());
            int deleted = service.SaveChanges();
            Console.WriteLine($"  SaveChanges returned {deleted}; row removed from database");
            Console.WriteLine($"  Find after delete: {(service.GetById(newProduct.ProductId) is null ? "null" : "still exists")}");
        }
        Console.WriteLine();

        Console.WriteLine("--- SECTION 7: batch SaveChanges (Add + Update + Remove in one flush) ---");
        Product batchNew = service.AddProduct("Batch Item", 1.00m, 10);
        Product? batchTarget = service.GetById(1);
        if (batchTarget is not null)
        {
            batchTarget.StockQuantity += 5;
        }

        Product? batchRemove = service.GetById(3);
        if (batchRemove is not null)
        {
            service.TryRemoveById(batchRemove.ProductId);
        }

        Console.WriteLine("  Pending states before single SaveChanges:");
        Console.WriteLine(service.DescribeAllTrackedStates());
        int batchAffected = service.SaveChanges();
        Console.WriteLine($"  One SaveChanges persisted {batchAffected} state entries");
        Console.WriteLine();

        Console.WriteLine("--- Final catalog ---");
        foreach (Product product in service.GetActiveProducts())
        {
            Console.WriteLine($"  {product}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE ù CRUD OPERATIONS & SaveChanges
 * =========================================================================
 *
 * --- DbContext lifetime ---
 *
 *   using var context = new AppDbContext(options);
 *   // one unit of work; dispose closes connection
 *
 * --- CREATE ---
 *
 *   context.Products.Add(entity);     // EntityState.Added
 *   context.SaveChanges();            // INSERT; PK populated; -> Unchanged
 *
 * --- READ (tracked) ---
 *
 *   Product? p = context.Products.Find(id);
 *   var list = context.Products.Where(...).ToList();   // full LINQ -> ch08
 *
 * --- UPDATE ---
 *
 *   // tracked (same context):
 *   p.UnitPrice = 12m;                // -> Modified
 *
 *   // detached stub:
 *   context.Products.Update(stub);    // attach + Modified
 *
 * --- DELETE ---
 *
 *   context.Products.Remove(p);       // -> Deleted
 *   context.SaveChanges();            // DELETE
 *
 * --- SaveChanges ---
 *
 *   int n = context.SaveChanges();    // one transaction; flushes all pending states
 *
 * --- Entity states ---
 *
 *   Detached | Unchanged | Added | Modified | Deleted
 *   context.Entry(entity).State
 *   context.ChangeTracker.Entries()
 *
 * --- Bootstrap ---
 *
 *   EfCoreTutorialBootstrap.EnsureReady(options);  // EnsureCreated + seed
 *   // production: context.Database.Migrate();     // ch03 migrations
 *
 * --- Forward references ---
 *
 *   ch02 DbContext & DbSet              options, DI, DbContext pooling
 *   ch03 Code-First & Migrations        Add-Migration, Migrate(), snapshots
 *   ch06 Relationships                  cascade delete, FK constraints
 *   ch08 LINQ to Entities               IQueryable, deferred execution
 *   ch11 Change Tracking & Async        Attach, AsNoTracking, SaveChangesAsync
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Forget SaveChanges after Add/Update  | No SQL executed; data not saved
 *  Update detached without Update()     | Changes ignored ù entity not tracked
 *  Dispose context before SaveChanges   | Pending changes lost
 *  Expect Add to set store-generated Id | ProductId stays 0 until SaveChanges
 *
 * =========================================================================
 */
