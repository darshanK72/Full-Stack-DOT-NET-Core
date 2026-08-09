namespace ChangeTrackingAsyncAndTransactions.Models;

/*
 * FILE ROLE:
 *   Simple DTO for a two-step inventory adjustment inside one database transaction.
 *
 * SECTIONS IN THIS FILE:
 *   2. StockTransfer DTO (disconnected update input)
 */

/*
 * SECTION 2: STOCK TRANSFER DTO (DISCONNECTED UPDATE INPUT)
 *
 * Web APIs and background jobs often receive plain objects that were never queried
 * through the current DbContext. StockTransfer models that shape: two product IDs
 * and a quantity moved from source to destination.
 *
 * This type is NOT mapped to a table — it drives attach/update logic in
 * Services/InventoryTransactionService.cs (SECTION 8).
 * -------------------------------------------------------------------------
 */
public sealed class StockTransfer
{
    public int FromProductId { get; init; }
    public int ToProductId { get; init; }
    public int Quantity { get; init; }
}
