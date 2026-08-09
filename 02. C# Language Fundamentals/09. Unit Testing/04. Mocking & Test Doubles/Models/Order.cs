using System.Collections.Generic;

namespace MockingAndTestDoubles.Models;

/*
 * SECTION 2: ORDER — INPUT TO THE SYSTEM UNDER TEST
 *
 * OrderService.PlaceOrder(Order) is the public entry point we unit-test.
 * Total is pre-calculated here so this chapter stays about doubles, not pricing rules.
 *
 *   Field           | Role in PlaceOrder
 *   ----------------|--------------------------------------------------
 *   OrderId         | Passed to SendOrderConfirmation on success
 *   CustomerEmail   | Recipient of the confirmation email
 *   Lines           | SKUs checked and reserved in sequence
 *   Total           | Shown in the confirmation email body
 */
public sealed class Order
{
    public int OrderId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public IReadOnlyList<OrderLine> Lines { get; init; } = [];
    public decimal Total { get; init; }
}
