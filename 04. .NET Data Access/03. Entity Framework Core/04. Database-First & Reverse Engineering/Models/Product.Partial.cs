namespace DatabaseFirstAndReverseEngineering.Models;

/*
 * FILE ROLE:
 *   Developer-owned partial extensions for Product (not overwritten by scaffold).
 *
 * SECTIONS IN THIS FILE:
 *   7. Partial classes - safe customization (Product side)
 */

public partial class Product
{
    public bool IsInStock => StockQuantity > 0;

    public decimal InventoryValue => UnitPrice * StockQuantity;

    public string StockStatus => IsInStock ? "Available" : "Out of stock";
}
