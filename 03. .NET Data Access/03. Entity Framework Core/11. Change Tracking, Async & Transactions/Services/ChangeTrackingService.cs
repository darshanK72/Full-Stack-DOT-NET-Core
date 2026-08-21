using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChangeTrackingAsyncAndTransactions.Data;
using ChangeTrackingAsyncAndTransactions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ChangeTrackingAsyncAndTransactions.Services;

/*
 * FILE ROLE:
 *   Demonstrates tracked queries, entity states, and AsNoTracking read-only queries.
 *
 * SECTIONS IN THIS FILE:
 *   5. Tracked queries and EntityState transitions
 *   6. AsNoTracking for read-only workloads
 */

public sealed class ChangeTrackingService
{
    private readonly InventoryDbContext _context;

    public ChangeTrackingService(InventoryDbContext context)
    {
        _context = context;
    }

    /*
     * SECTION 5: TRACKED QUERIES AND ENTITYSTATE TRANSITIONS
     *
     * Default queries are tracked. EF snapshots property values; changing a property
     * marks the entity Modified without calling Update().
     *
     * Entry(entity).State lets you read or force a state (Attach sets Unchanged, etc.).
     * -------------------------------------------------------------------------
     */
    public async Task<string> DemonstrateTrackedStatesAsync(int productId, CancellationToken cancellationToken = default)
    {
        Product? product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken)
            .ConfigureAwait(false);

        if (product is null)
        {
            return $"Product {productId} not found.";
        }

        EntityState afterLoad = _context.Entry(product).State; // Unchanged — tracked from query

        decimal originalPrice = product.UnitPrice;
        product.UnitPrice = originalPrice + 1.00m;
        EntityState afterEdit = _context.Entry(product).State; // Modified — property changed

        int rows = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        EntityState afterSave = _context.Entry(product).State; // Unchanged — synced with database

        product.UnitPrice = originalPrice; // restore for later demos
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return $"Load={afterLoad}, AfterEdit={afterEdit}, AfterSave={afterSave}, RowsAffected={rows}";
    }

    /*
     * SECTION 6: ASNOTRACKING FOR READ-ONLY WORKLOADS
     *
     * AsNoTracking() skips change-tracker snapshots — lower memory, faster for lists,
     * reports, and API responses that will not be saved through the same context.
     *
     * | Scenario              | Use tracking? | API                         |
     * |-----------------------|---------------|-----------------------------|
     * | Edit form load + save | Yes           | default query               |
     * | Catalog / search grid | No            | .AsNoTracking()             |
     * | Global default        | No (opt-in)   | UseQueryTrackingBehavior    |
     *
     * Pitfall: modifying an AsNoTracking entity does nothing on SaveChanges unless
     * you Attach/Update it explicitly (SECTION 7).
     * -------------------------------------------------------------------------
     */
    public async Task<string> DemonstrateAsNoTrackingAsync(CancellationToken cancellationToken = default)
    {
        Product? tracked = await _context.Products
            .FirstAsync(p => p.ProductId == 1, cancellationToken)
            .ConfigureAwait(false);

        Product readOnly = await _context.Products
            .AsNoTracking()
            .FirstAsync(p => p.ProductId == 1, cancellationToken)
            .ConfigureAwait(false);

        bool sameInstance = ReferenceEquals(tracked, readOnly);
        bool trackedByContext = _context.ChangeTracker.Entries<Product>().Any(e => e.Entity == readOnly);

        readOnly.UnitPrice = 999.99m; // change is NOT tracked
        int rowsAfterPhantomEdit = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return $"SameInstance={sameInstance}, ReadOnlyIsTracked={trackedByContext}, RowsAfterPhantomEdit={rowsAfterPhantomEdit}";
    }

    public async Task<IReadOnlyList<string>> GetProductCatalogAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.ProductName)
            .Select(p => $"{p.ProductId}: {p.ProductName} @ ${p.UnitPrice:F2} (stock {p.StockQuantity})")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
