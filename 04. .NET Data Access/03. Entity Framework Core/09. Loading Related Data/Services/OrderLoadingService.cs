using System.Collections.Generic;
using System.Linq;
using LoadingRelatedData.Data;
using LoadingRelatedData.Models;
using Microsoft.EntityFrameworkCore;

namespace LoadingRelatedData.Services;

/*
 * FILE ROLE:
 *   Eager, explicit, and filtered loading patterns for Order + Customer + OrderLines.
 *
 * SECTIONS IN THIS FILE:
 *   4. Eager loading - Include and ThenInclude
 *   5. N+1 anti-pattern - separate query per order
 *   7. Explicit loading - Entry().Reference / Collection().Load()
 *   8. Filtered Include - Include collection with Where
 */

/*
 * SECTION 4: EAGER LOADING - Include AND ThenInclude
 *
 * Include expands the initial SQL with JOINs (or split queries) so related rows arrive
 * in the same round-trip. ThenInclude continues the chain after a collection Include.
 *
 *   context.Orders
 *       .Include(o => o.Customer)                         // reference navigation
 *       .Include(o => o.Lines)                             // collection navigation
 *           .ThenInclude(l => l.Product)                   // nested reference
 *       .ToList();
 *
 * | API            | Use when                                      |
 * |----------------|-----------------------------------------------|
 * | Include        | Load a direct navigation from the root entity |
 * | ThenInclude    | Load a navigation from an included entity       |
 * | AsSplitQuery   | Avoid cartesian explosion on multiple collections (preview in ch.08) |
 *
 * AsNoTracking: read-only demos - COVERED IN DETAIL -> 11. Change Tracking, Async & Transactions
 * -------------------------------------------------------------------------
 */
public sealed class OrderLoadingService
{
    private readonly string _connectionString;

    public OrderLoadingService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IList<Order> GetOrdersWithEagerLoading()
    {
        using ShopDbContext context = CreateContext();
        return context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .OrderBy(o => o.OrderId)
            .ToList();
    }

    /*
     * SECTION 5: N+1 ANTI-PATTERN - ONE QUERY PER ORDER FOR LINES
     *
     * Loads orders once, then runs a separate SQL query inside the loop for each order's lines.
     * Common mistake when developers skip Include and query children manually.
     *
     * 3 orders -> 1 (orders) + 3 (lines per order) = 4 SQL commands (N+1 where N = order count).
     * Fix: GetOrdersWithEagerLoading() above - typically 1 command (or 2 with split query).
     * -------------------------------------------------------------------------
     */
    public IList<Order> GetOrdersWithNPlusOnePattern()
    {
        using ShopDbContext context = CreateContext();
        List<Order> orders = context.Orders
            .AsNoTracking()
            .OrderBy(o => o.OrderId)
            .ToList();

        foreach (Order order in orders)
        {
            order.Lines = context.OrderLines
                .AsNoTracking()
                .Where(l => l.OrderId == order.OrderId)
                .ToList();
        }

        return orders;
    }

    /*
     * SECTION 7: EXPLICIT LOADING - Entry().Reference().Load() / Collection().Load()
     *
     * Start with a root query that does NOT include related data, then load navigations
     * on demand while the DbContext is still open.
     *
     *   context.Entry(order).Reference(o => o.Customer).Load();
     *   context.Entry(order).Collection(o => o.Lines).Load();
     *
     * Useful when you only know which navigations you need after business logic runs.
     * Each Load() issues a separate SQL command (similar cost to selective Includes).
     * -------------------------------------------------------------------------
     */
    public Order? GetOrderWithExplicitLoading(int orderId)
    {
        using ShopDbContext context = CreateContext();
        Order? order = context.Orders.FirstOrDefault(o => o.OrderId == orderId); // tracked - required for Load()

        if (order is null)
        {
            return null;
        }

        context.Entry(order).Reference(o => o.Customer).Load();
        context.Entry(order).Collection(o => o.Lines).Query().Include(l => l.Product).Load();

        return order;
    }

    /*
     * SECTION 8: FILTERED INCLUDE - ONLY MATCHING CHILD ROWS
     *
     * EF Core 5+ filters the included collection in SQL (not client-side after full load).
     *
     *   .Include(o => o.Lines.Where(l => l.Quantity >= minQuantity))
     *
     * Root orders are still fully returned; only Lines matching the predicate are populated.
     * Cannot filter the root entity with Include - use Where before Include for that.
     * -------------------------------------------------------------------------
     */
    public IList<Order> GetOrdersWithFilteredLines(int minQuantity)
    {
        using ShopDbContext context = CreateContext();
        return context.Orders
            .AsNoTracking()
            .Include(o => o.Lines.Where(l => l.Quantity >= minQuantity))
            .OrderBy(o => o.OrderId)
            .ToList();
    }

    private ShopDbContext CreateContext()
    {
        DbContextOptionsBuilder<ShopDbContext> builder = new DbContextOptionsBuilder<ShopDbContext>();
        builder.UseSqlServer(_connectionString);
        return new ShopDbContext(builder.Options);
    }
}
