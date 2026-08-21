using System;
using System.Threading;
using System.Threading.Tasks;
using ChangeTrackingAsyncAndTransactions.Data;
using ChangeTrackingAsyncAndTransactions.Models;
using ChangeTrackingAsyncAndTransactions.Services;
using Microsoft.EntityFrameworkCore;

namespace ChangeTrackingAsyncAndTransactions;

/*
 * TOPIC: Change Tracking, Async & Transactions ù how EF Core tracks entities,
 *        serves read-only queries, persists asynchronously, and groups work in transactions.
 *
 * WHY IT MATTERS:
 *   Production apps load data for display (no tracking), accept disconnected DTOs from
 *   APIs (attach/update), save without blocking threads (SaveChangesAsync), and move
 *   inventory or money atomically (explicit transactions). Misusing tracking causes
 *   silent lost updates, memory pressure, and partial commits.
 *
 * PREREQUISITE:
 *   EF Core ch.02 (DbContext), ch.05 (CRUD / SaveChanges basics), ch.08 (LINQ queries).
 *
 * WHAT YOU WILL LEARN:
 *   1. Entity states (Detached, Unchanged, Added, Modified, Deleted)
 *   2. Tracked queries and property-change detection
 *   3. AsNoTracking for read-only catalog/report queries
 *   4. Attach, Update, and selective IsModified updates (disconnected entities)
 *   5. SaveChangesAsync ù async persistence and row counts
 *   6. BeginTransactionAsync ù Commit and Rollback across multiple operations
 *   7. Interceptors and SaveChanges events (preview)
 *
 * CHAPTER MAP:
 *   1. Product entity + state recap          -> Models/Product.cs
 *   2. StockTransfer DTO                     -> Models/StockTransfer.cs
 *   3. InventoryDbContext                    -> Data/InventoryDbContext.cs
 *   4. Database bootstrap                    -> Data/DatabaseBootstrap.cs
 *   5. Tracked queries + EntityState         -> Services/ChangeTrackingService.cs
 *   6. AsNoTracking read-only queries         -> Services/ChangeTrackingService.cs
 *   7. Attach / Update patterns              -> Services/ProductUpdateService.cs
 *   8. SaveChangesAsync + BeginTransaction   -> Services/InventoryTransactionService.cs
 *   9. Interceptors preview                  -> Data/InterceptorsPreview.cs
 *  10. Demonstration (async Main)             -> Program.cs (below)
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreInventoryTutorial;Integrated Security=true;TrustServerCertificate=true
 */

public class Program
{
    /*
     * SECTION 10: DEMONSTRATION ù async Main orchestrates the chapter demo
     *
     * Bootstraps LocalDB schema, then runs tracking, no-tracking, attach/update,
     * async save, transaction, and interceptor preview sections in reading order.
     * -------------------------------------------------------------------------
     */
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== 11. Change Tracking, Async & Transactions (EF Core) ===");
        Console.WriteLine();

        Console.WriteLine("--- SECTION 9: Interceptors preview (see Data/InterceptorsPreview.cs) ---");
        Console.WriteLine("  AuditSaveChangesInterceptor logs tracked changes before save.");
        Console.WriteLine("  Uncomment AddInterceptors in InventoryDbContext.OnConfiguring to try it.");
        Console.WriteLine();

        bool databaseReady = await DatabaseBootstrap.TryEnsureSchemaAsync().ConfigureAwait(false);
        if (!databaseReady)
        {
            Console.WriteLine("Database demos skipped ù start LocalDB and re-run.");
            Console.WriteLine("  DDL via EnsureCreated documented in Data/DatabaseBootstrap.cs.");
            return;
        }

        Console.WriteLine("EfCoreInventoryTutorial ready on LocalDB.");
        Console.WriteLine();

        DbContextOptions<InventoryDbContext> options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer(InventoryDbContext.ConnectionString)
            .Options;

        await using (InventoryDbContext context = new InventoryDbContext(options))
        {
            ChangeTrackingService tracking = new ChangeTrackingService(context);

            Console.WriteLine("--- SECTION 5: Tracked query and EntityState ---");
            string stateSummary = await tracking.DemonstrateTrackedStatesAsync(productId: 1).ConfigureAwait(false);
            Console.WriteLine($"  {stateSummary}");
            Console.WriteLine();

            Console.WriteLine("--- SECTION 6: AsNoTracking catalog ---");
            foreach (string line in await tracking.GetProductCatalogAsync().ConfigureAwait(false))
            {
                Console.WriteLine($"  {line}");
            }

            string noTrackResult = await tracking.DemonstrateAsNoTrackingAsync().ConfigureAwait(false);
            Console.WriteLine($"  Phantom edit check: {noTrackResult}");
            Console.WriteLine();
        }

        await using (InventoryDbContext context = new InventoryDbContext(options))
        {
            ProductUpdateService updates = new ProductUpdateService(context);

            Console.WriteLine("--- SECTION 7a: Update(disconnected entity) ---");
            Product dto = new Product
            {
                ProductId = 2,
                ProductName = "Widget B",
                UnitPrice = 15.00m,
                StockQuantity = 50
            };
            Console.WriteLine($"  {await updates.UpdateWithUpdateMethodAsync(dto).ConfigureAwait(false)}");
            Console.WriteLine();
        }

        await using (InventoryDbContext context = new InventoryDbContext(options))
        {
            ProductUpdateService updates = new ProductUpdateService(context);

            Console.WriteLine("--- SECTION 7b: Attach + IsModified (partial update) ---");
            Console.WriteLine($"  {await updates.UpdateWithAttachAndModifiedFlagsAsync(productId: 2, newPrice: 14.50m).ConfigureAwait(false)}");
            Console.WriteLine();
        }

        await using (InventoryDbContext context = new InventoryDbContext(options))
        {
            ProductUpdateService updates = new ProductUpdateService(context);

            Console.WriteLine("--- SECTION 7c: Find + modify (connected) ---");
            Console.WriteLine($"  {await updates.UpdateWithFindAsync(productId: 3, newStock: 30).ConfigureAwait(false)}");
            Console.WriteLine();
        }

        await using (InventoryDbContext context = new InventoryDbContext(options))
        {
            InventoryTransactionService transactions = new InventoryTransactionService(context);

            Console.WriteLine("--- SECTION 8: SaveChangesAsync inside BeginTransactionAsync ---");
            StockTransfer transfer = new StockTransfer
            {
                FromProductId = 1,
                ToProductId = 3,
                Quantity = 5
            };
            Console.WriteLine($"  {await transactions.TransferStockAsync(transfer).ConfigureAwait(false)}");
            Console.WriteLine();

            Console.WriteLine("--- SECTION 8b: Rollback ---");
            Console.WriteLine($"  {await transactions.DemonstrateRollbackAsync().ConfigureAwait(false)}");
            Console.WriteLine();
        }
    }
}

/*
 * QUICK REFERENCE ù CHANGE TRACKING, ASYNC & TRANSACTIONS
 *
 * --- Entity state ---
 *
 *   context.Entry(entity).State   // Detached, Unchanged, Added, Modified, Deleted
 *
 * --- Tracked vs no-tracking ---
 *
 *   var item = await context.Products.FirstAsync(p => p.Id == id);     // tracked
 *   var list = await context.Products.AsNoTracking().ToListAsync();   // read-only
 *
 * --- Disconnected update patterns ---
 *
 *   context.Update(dto);                              // all properties -> Modified
 *   context.Attach(stub);
 *   context.Entry(stub).Property(p => p.Price).IsModified = true;   // partial UPDATE
 *   var tracked = await context.Products.FindAsync(id);
 *   tracked!.Stock = newStock;                        // connected edit
 *
 * --- SaveChangesAsync ---
 *
 *   int rows = await context.SaveChangesAsync(cancellationToken);
 *
 * --- Explicit transaction ---
 *
 *   await using var tx = await context.Database.BeginTransactionAsync();
 *   try {
 *       // multiple operations / SaveChangesAsync calls
 *       await tx.CommitAsync();
 *   } catch {
 *       await tx.RollbackAsync();
 *       throw;
 *   }
 *
 * --- Interceptors (preview) ---
 *
 *   class AuditInterceptor : SaveChangesInterceptor { ... SavingChangesAsync ... }
 *   optionsBuilder.AddInterceptors(new AuditInterceptor());
 *   See Data/InterceptorsPreview.cs
 */
