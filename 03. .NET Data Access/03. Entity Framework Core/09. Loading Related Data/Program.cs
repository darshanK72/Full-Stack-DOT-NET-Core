using System;
using System.Collections.Generic;
using System.Linq;
using LoadingRelatedData.Data;
using LoadingRelatedData.Models;
using LoadingRelatedData.Services;
using LoadingRelatedData.Utils;
using Microsoft.EntityFrameworkCore;

namespace LoadingRelatedData;

/*
 * TOPIC: Loading Related Data - how EF Core loads navigations (Customer, OrderLines, Product)
 *        with eager Include/ThenInclude, explicit Load, filtered Include, and lazy loading preview.
 *
 * WHY IT MATTERS:
 *   ORM queries return entity graphs, not flat rows. Loading too little leaves navigations null;
 *   loading too much or in loops causes N+1 SQL round-trips. Production APIs need deliberate
 *   Include / projection / explicit Load strategies - the same relationships you modeled in ch.06.
 *
 * PREREQUISITE:
 *   EF Core ch.02 (DbContext), ch.06 (relationships), ch.08 (IQueryable / deferred execution preview).
 *
 * WHAT YOU WILL LEARN:
 *   1.  Entity types with navigation properties (Order, Customer, OrderLine, Product)
 *   2.  DbContext mapping to AdoNetTutorial tables
 *   3.  LocalDB bootstrap and seed data
 *   4.  Eager loading - Include and ThenInclude
 *   5.  N+1 anti-pattern vs single Include query (SQL command count)
 *   6.  Query diagnostics with LogTo
 *   7.  Explicit loading - Entry().Reference().Load() / Collection().Load()
 *   8.  Filtered Include - Include(o => o.Lines.Where(...))
 *   9.  Lazy loading preview (Proxies package - not enabled here)
 *   10. Demonstration wiring
 *
 * CHAPTER MAP:
 *   1.  Entity types + navigations       -> Models/OrderEntities.cs
 *   2.  DbContext configuration          -> Data/ShopDbContext.cs
 *   3.  Database bootstrap               -> Data/DatabaseBootstrap.cs
 *   1.  Connection string                -> Utils/ConnectionHelper.cs
 *   4.  Include / ThenInclude            -> Services/OrderLoadingService.cs
 *   5.  N+1 anti-pattern                 -> Services/OrderLoadingService.cs
 *   6.  SQL command counter              -> Utils/QueryDiagnostics.cs
 *   7.  Explicit loading                 -> Services/OrderLoadingService.cs
 *   8.  Filtered Include                 -> Services/OrderLoadingService.cs
 *   9.  Lazy loading preview             -> Utils/LazyLoadingPreview.cs
 *   10. Demonstration                    -> Program.cs Main (below)
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true
 */

public class Program
{
    /*
     * SECTION 10: DEMONSTRATION - Main orchestrates the chapter demo
     *
     * Bootstraps LocalDB, then runs loading patterns in reading order.
     * N+1 vs Include uses QueryDiagnostics to print SQL command counts.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 09. Loading Related Data (EF Core) ===");
        Console.WriteLine();

        Console.WriteLine($"--- SECTION 9: Lazy loading preview ({LazyLoadingPreview.PackageName}) ---");
        Console.WriteLine($"  {LazyLoadingPreview.Summary}");
        Console.WriteLine("  See Utils/LazyLoadingPreview.cs for setup steps and pitfalls.");
        Console.WriteLine();

        bool databaseReady = DatabaseBootstrap.TryEnsureSchema(ConnectionHelper.AdoNetTutorial);
        if (!databaseReady)
        {
            Console.WriteLine("Database demos skipped - start LocalDB and re-run.");
            Console.WriteLine("  DDL documented in Data/DatabaseBootstrap.cs and chapter intro above.");
            return;
        }

        Console.WriteLine("AdoNetTutorial ready on LocalDB.");
        Console.WriteLine();

        OrderLoadingService service = new OrderLoadingService(ConnectionHelper.AdoNetTutorial);

        DemonstrateNPlusOneVsInclude();
        DemonstrateEagerLoading(service);
        DemonstrateExplicitLoading(service);
        DemonstrateFilteredInclude(service);
    }

    private static void DemonstrateNPlusOneVsInclude()
    {
        Console.WriteLine("--- SECTIONS 5-6: N+1 anti-pattern vs Include (SQL command count) ---");

        QueryDiagnostics diagnostics = new QueryDiagnostics();

        diagnostics.Reset();
        using (ShopDbContext context = diagnostics.CreateContext(ConnectionHelper.AdoNetTutorial))
        {
            List<Order> orders = context.Orders.AsNoTracking().OrderBy(o => o.OrderId).ToList();
            foreach (Order order in orders)
            {
                _ = context.OrderLines.AsNoTracking().Where(l => l.OrderId == order.OrderId).ToList();
            }

            Console.WriteLine($"  N+1 loop (orders + lines per order): {diagnostics.CommandCount} SQL command(s)");
        }

        diagnostics.Reset();
        using (ShopDbContext context = diagnostics.CreateContext(ConnectionHelper.AdoNetTutorial))
        {
            List<Order> orders = context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Lines)
                .OrderBy(o => o.OrderId)
                .ToList();

            Console.WriteLine($"  Single query with Include:            {diagnostics.CommandCount} SQL command(s)");
            Console.WriteLine($"  Orders materialized: {orders.Count}; first customer: {orders[0].Customer?.Name}");
        }

        Console.WriteLine();
    }

    private static void DemonstrateEagerLoading(OrderLoadingService service)
    {
        Console.WriteLine("--- SECTION 4: Include / ThenInclude ---");

        IList<Order> orders = service.GetOrdersWithEagerLoading();
        foreach (Order order in orders)
        {
            Customer? customer = order.Customer;
            Console.WriteLine($"  Order {order.OrderId} ({order.OrderDate:d}) ${order.TotalAmount:F2} - {customer?.Name}");
            foreach (OrderLine line in order.Lines)
            {
                Product? product = line.Product;
                Console.WriteLine($"    Line {line.OrderLineId}: qty {line.Quantity}, {product?.ProductName} @ ${product?.UnitPrice:F2}");
            }
        }

        Console.WriteLine();
    }

    private static void DemonstrateExplicitLoading(OrderLoadingService service)
    {
        Console.WriteLine("--- SECTION 7: Explicit loading (Entry().Load()) ---");

        Order? order = service.GetOrderWithExplicitLoading(orderId: 1);
        if (order is null)
        {
            Console.WriteLine("  Order 1 not found.");
            return;
        }

        Console.WriteLine($"  Order {order.OrderId} loaded first; Customer and Lines loaded via Entry().Load()");
        Console.WriteLine($"  Customer: {order.Customer?.Name}; Lines: {order.Lines.Count}");
        Console.WriteLine();
    }

    private static void DemonstrateFilteredInclude(OrderLoadingService service)
    {
        Console.WriteLine("--- SECTION 8: Filtered Include ---");

        const int minQuantity = 1;
        IList<Order> orders = service.GetOrdersWithFilteredLines(minQuantity);
        foreach (Order order in orders)
        {
            Console.WriteLine($"  Order {order.OrderId}: {order.Lines.Count} line(s) with Quantity >= {minQuantity}");
        }

        Console.WriteLine("  Change minQuantity to 2 in seed data scenarios to see fewer included lines.");
        Console.WriteLine();
    }
}

/*
 * QUICK REFERENCE - LOADING RELATED DATA
 *
 * --- Default behavior ---
 *
 *   context.Orders.ToList();
 *   // order.Customer and order.Lines are NOT populated (null / empty)
 *
 * --- Eager loading (Include / ThenInclude) ---
 *
 *   context.Orders
 *       .Include(o => o.Customer)
 *       .Include(o => o.Lines)
 *           .ThenInclude(l => l.Product)
 *       .ToList();
 *
 * --- N+1 anti-pattern ---
 *
 *   var orders = context.Orders.ToList();           // 1 query
 *   foreach (var o in orders)
 *       o.Lines = context.OrderLines.Where(...).ToList();  // +N queries
 *
 * --- Explicit loading (entity must be tracked) ---
 *
 *   var order = context.Orders.First(o => o.OrderId == id);
 *   context.Entry(order).Reference(o => o.Customer).Load();
 *   context.Entry(order).Collection(o => o.Lines).Load();
 *
 * --- Filtered Include ---
 *
 *   context.Orders
 *       .Include(o => o.Lines.Where(l => l.Quantity >= 2))
 *       .ToList();
 *
 * --- Lazy loading (preview) ---
 *
 *   Install Microsoft.EntityFrameworkCore.Proxies
 *   options.UseLazyLoadingProxies();
 *   public virtual Customer? Customer { get; set; }  // virtual required
 *   See Utils/LazyLoadingPreview.cs
 *
 * --- Diagnostics ---
 *
 *   options.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
 *   // or count commands - see Utils/QueryDiagnostics.cs
 */
