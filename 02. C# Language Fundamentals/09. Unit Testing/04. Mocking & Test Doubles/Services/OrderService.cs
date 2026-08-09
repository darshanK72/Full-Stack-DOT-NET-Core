using MockingAndTestDoubles.Models;

namespace MockingAndTestDoubles.Services;

/*
 * SECTION 4: SYSTEM UNDER TEST (SUT) — ORDER SERVICE
 *
 * OrderService orchestrates two collaborators injected via constructor (DI):
 *
 *   IInventoryService — stock check + reservation
 *   IEmailSender      — customer notification
 *
 * Unit tests isolate THIS class. Real database and SMTP stay outside the test.
 * Behavior to assert:
 *
 *   • Every line must pass HasStock before ANY Reserve runs (all-or-nothing check phase)
 *   • On success: Reserve each line, then one confirmation email
 *   • On failure: return Failed immediately — no Reserve, no email
 *
 * Pitfall: do not new() concrete infrastructure inside PlaceOrder — that hides
 * dependencies and makes mocking impossible without heavier integration tests.
 */
public sealed class OrderService
{
    private readonly IInventoryService _inventory;
    private readonly IEmailSender _emailSender;

    public OrderService(IInventoryService inventory, IEmailSender emailSender)
    {
        _inventory = inventory;
        _emailSender = emailSender;
    }

    public OrderResult PlaceOrder(Order order)
    {
        foreach (OrderLine line in order.Lines)
        {
            if (!_inventory.HasStock(line.Sku, line.Quantity))
            {
                return OrderResult.Failed($"Insufficient stock for SKU '{line.Sku}'.");
            }
        }

        foreach (OrderLine line in order.Lines)
        {
            _inventory.Reserve(line.Sku, line.Quantity);
        }

        _emailSender.SendOrderConfirmation(order.CustomerEmail, order.OrderId, order.Total);
        return OrderResult.Succeeded(order.OrderId);
    }
}
