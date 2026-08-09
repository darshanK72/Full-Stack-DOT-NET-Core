namespace XUnit.Models;

/*
 * SECTION 1: ORDER LINE — ONE ROW ON AN ORDER
 *
 * A line item holds SKU, display name, quantity, and unit price. Tests exercise
 * pricing math through collections of OrderLine — the type stays simple so
 * assertions focus on xUnit features, not domain complexity.
 */
public sealed class OrderLine
{
    public required string Sku { get; init; }

    public required string ProductName { get; init; }

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }
}
