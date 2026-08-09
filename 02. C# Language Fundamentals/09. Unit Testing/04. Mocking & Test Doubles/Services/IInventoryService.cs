namespace MockingAndTestDoubles.Services;

/*
 * SECTION 3a: INVENTORY BOUNDARY — THE STOCK SEAM
 *
 * OrderService never talks to SQL or a warehouse API directly. It depends on this
 * interface so tests can inject a stub (canned stock), a fake (in-memory ledger),
 * or a Moq mock (Setup + Verify).
 *
 *   Method    | Called when                         | Test concern
 *   ----------|-------------------------------------|----------------------------------
 *   HasStock  | Before any reservation              | Return true/false per scenario
 *   Reserve   | After all lines pass HasStock       | Verify called once per line — or Never on failure
 *
 * Moq requires an interface (or virtual members) to generate a runtime proxy.
 */
public interface IInventoryService
{
    bool HasStock(string sku, int quantity);
    void Reserve(string sku, int quantity);
}
