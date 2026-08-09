using System;
using System.Threading;
using System.Threading.Tasks;
using ChangeTrackingAsyncAndTransactions.Data;
using ChangeTrackingAsyncAndTransactions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChangeTrackingAsyncAndTransactions.Services;

/*
 * FILE ROLE:
 *   SaveChangesAsync and explicit database transactions with BeginTransactionAsync.
 *
 * SECTIONS IN THIS FILE:
 *   8. SaveChangesAsync and BeginTransactionAsync
 */

public sealed class InventoryTransactionService
{
    private readonly InventoryDbContext _context;

    public InventoryTransactionService(InventoryDbContext context)
    {
        _context = context;
    }

    /*
     * SECTION 8: SAVECHANGESASYNC AND BEGINTRANSACTIONASYNC
     *
     * SaveChangesAsync:
     *   Async I/O to the database provider. Always prefer in ASP.NET and library code
     *   that already uses async end-to-end. Returns the number of state entries written.
     *
     * BeginTransactionAsync:
     *   Wraps multiple SaveChanges calls (or raw SQL) in one atomic COMMIT/ROLLBACK.
     *   EF Core coordinates with the underlying ADO.NET transaction on the connection.
     *
     * Pattern:
     *   await using IDbContextTransaction tx = await context.Database.BeginTransactionAsync();
     *   try { ... await context.SaveChangesAsync(); await tx.CommitAsync(); }
     *   catch { await tx.RollbackAsync(); throw; }
     *
     * Note: a single SaveChangesAsync is already transactional for its own batch.
     * Use explicit transactions when multiple saves must succeed or fail together.
     * -------------------------------------------------------------------------
     */
    public async Task<string> TransferStockAsync(StockTransfer transfer, CancellationToken cancellationToken = default)
    {
        if (transfer.Quantity <= 0)
        {
            return "Quantity must be positive.";
        }

        await using IDbContextTransaction transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            Product? from = await _context.Products
                .FindAsync(new object[] { transfer.FromProductId }, cancellationToken)
                .ConfigureAwait(false);

            Product? to = await _context.Products
                .FindAsync(new object[] { transfer.ToProductId }, cancellationToken)
                .ConfigureAwait(false);

            if (from is null || to is null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return "One or both products not found — transaction rolled back.";
            }

            if (from.StockQuantity < transfer.Quantity)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return $"Insufficient stock on Product {from.ProductId} — transaction rolled back.";
            }

            from.StockQuantity -= transfer.Quantity;
            to.StockQuantity += transfer.Quantity;

            int rows = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return $"Committed transfer of {transfer.Quantity} unit(s): Product {from.ProductId} -> {to.ProductId}, Rows={rows}";
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<string> DemonstrateRollbackAsync(CancellationToken cancellationToken = default)
    {
        Product? product = await _context.Products
            .FindAsync(new object[] { 1 }, cancellationToken)
            .ConfigureAwait(false);

        if (product is null)
        {
            return "Product 1 not found.";
        }

        int stockBefore = product.StockQuantity;

        await using IDbContextTransaction transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        product.StockQuantity = -1; // invalid business state — we will roll back intentionally
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

        _context.Entry(product).Reload(); // refresh from database after rollback
        return $"Rollback demo: stock before={stockBefore}, after reload={product.StockQuantity}";
    }
}
