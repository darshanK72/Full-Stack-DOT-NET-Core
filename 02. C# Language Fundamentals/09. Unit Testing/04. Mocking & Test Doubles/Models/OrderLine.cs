namespace MockingAndTestDoubles.Models;

/*
 * SECTION 2a: ORDER LINE — ONE SKU + QUANTITY
 *
 * OrderService.PlaceOrder loops order.Lines twice:
 *   1. HasStock for each line (read-only check)
 *   2. Reserve for each line (only when every line passed the check)
 *
 * Keeping lines as a simple DTO keeps the SUT focused on orchestration, not pricing.
 */
public sealed class OrderLine
{
    public string Sku { get; init; } = string.Empty; // warehouse product code
    public int Quantity { get; init; }                  // units the customer wants
}
