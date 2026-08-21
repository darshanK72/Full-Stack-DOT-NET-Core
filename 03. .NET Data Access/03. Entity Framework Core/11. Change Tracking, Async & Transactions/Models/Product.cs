namespace ChangeTrackingAsyncAndTransactions.Models;

/*
 * FILE ROLE:
 *   Product entity used for change-tracking, async save, and transaction demos.
 *
 * SECTIONS IN THIS FILE:
 *   1. Product entity and entity-state recap
 */

/*
 * SECTION 1: PRODUCT ENTITY AND ENTITY-STATE RECAP
 *
 * EF Core tracks each loaded entity in the change tracker. On SaveChanges(Async),
 * the context compares current property values to snapshots taken at query time
 * and generates INSERT / UPDATE / DELETE statements.
 *
 * | EntityState | Meaning                                      | Typical trigger        |
 * |-------------|----------------------------------------------|------------------------|
 * | Detached    | Not tracked by this DbContext instance       | new Product(), DTO     |
 * | Unchanged   | Tracked; no property changes since load      | After query            |
 * | Added       | Tracked; will INSERT on save                 | Add / AddAsync         |
 * | Modified    | Tracked; will UPDATE on save                 | Property change, Update|
 * | Deleted     | Tracked; will DELETE on save                 | Remove / RemoveAsync   |
 *
 * Inspect state: context.Entry(product).State
 * Forward reference: Add/Update/Remove depth -> ch.05 CRUD Operations & SaveChanges
 * -------------------------------------------------------------------------
 */
public sealed class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
