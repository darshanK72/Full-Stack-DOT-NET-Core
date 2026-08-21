using System.Threading;
using System.Threading.Tasks;
using ChangeTrackingAsyncAndTransactions.Data;
using ChangeTrackingAsyncAndTransactions.Models;
using Microsoft.EntityFrameworkCore;

namespace ChangeTrackingAsyncAndTransactions.Services;

/*
 * FILE ROLE:
 *   Attach and Update patterns for disconnected entities (DTOs, API payloads, other contexts).
 *
 * SECTIONS IN THIS FILE:
 *   7. Attach, Update, and selective property updates
 */

public sealed class ProductUpdateService
{
    private readonly InventoryDbContext _context;

    public ProductUpdateService(InventoryDbContext context)
    {
        _context = context;
    }

    /*
     * SECTION 7: ATTACH, UPDATE, AND SELECTIVE PROPERTY UPDATES
     *
     * Disconnected scenario: client sends a Product object that was never queried here.
     *
     * --- 7a. Update(entity) ---
     *   Marks the entire entity Modified — all columns included in UPDATE SET.
     *   Simple but can overwrite columns you did not intend to change (concurrency risk).
     *
     * --- 7b. Attach + Entry.State / IsModified ---
     *   Attach sets Unchanged; flip IsModified on specific properties for partial UPDATE.
     *
     * --- 7c. Find + modify (connected) ---
     *   When you have the key, query tracked entity and change properties — preferred
     *   when the DbContext lifetime spans the edit operation.
     *
     * | Method                         | Initial state | UPDATE shape              |
     * |--------------------------------|---------------|---------------------------|
     * | context.Update(dto)            | Modified      | All mapped properties     |
     * | Attach + IsModified per prop   | Unchanged->Mod| Only flagged properties   |
     * | Find + property assignment     | Unchanged->Mod| Only changed properties   |
     * Pitfall: only one tracked instance per key per DbContext — use a fresh context
     * for each disconnected attach demo, or Detach the conflicting entity first.
     * -------------------------------------------------------------------------
     */
    public async Task<string> UpdateWithUpdateMethodAsync(Product disconnected, CancellationToken cancellationToken = default)
    {
        _context.Update(disconnected); // entire entity -> Modified
        int rows = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return $"Update() saved {rows} row(s) for ProductId {disconnected.ProductId}";
    }

    public async Task<string> UpdateWithAttachAndModifiedFlagsAsync(
        int productId,
        decimal newPrice,
        CancellationToken cancellationToken = default)
    {
        Product stub = new Product
        {
            ProductId = productId,
            ProductName = string.Empty, // not sent by client — will not overwrite if not Modified
            UnitPrice = newPrice,
            StockQuantity = 0
        };

        _context.Attach(stub); // Detached -> Unchanged (key values must match a row)
        _context.Entry(stub).Property(p => p.UnitPrice).IsModified = true; // only UnitPrice in UPDATE

        int rows = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return $"Attach+IsModified saved {rows} row(s); UnitPrice -> {newPrice:F2}";
    }

    public async Task<string> UpdateWithFindAsync(
        int productId,
        int newStock,
        CancellationToken cancellationToken = default)
    {
        Product? tracked = await _context.Products
            .FindAsync(new object[] { productId }, cancellationToken)
            .ConfigureAwait(false);

        if (tracked is null)
        {
            return $"Product {productId} not found.";
        }

        tracked.StockQuantity = newStock;
        int rows = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return $"Find+modify saved {rows} row(s); StockQuantity -> {newStock}";
    }
}
