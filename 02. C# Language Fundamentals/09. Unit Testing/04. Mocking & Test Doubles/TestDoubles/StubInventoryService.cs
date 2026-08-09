using System.Collections.Generic;
using MockingAndTestDoubles.Services;

namespace MockingAndTestDoubles.TestDoubles;

/*
 * SECTION 5: STUB — CANNED ANSWERS WITHOUT CALL TRACKING
 *
 * A stub stands in for IInventoryService and returns predetermined data.
 * StubInventoryService reads from a Dictionary<string, int> you control in the test
 * or in Program.cs Main demo.
 *
 *   Characteristic | Stub behavior here
 *   ---------------|--------------------------------------------------
 *   HasStock       | true when dictionary entry >= requested quantity
 *   Reserve        | Decrements dictionary (minimal side effect for demo only)
 *
 * Stubs answer "what should the dependency return?" They do NOT record how many
 * times Reserve was called — use a mock + Verify for that (see .Tests Moq file).
 *
 * Manual stub tests: MockingAndTestDoubles.Tests/OrderServiceManualDoubleTests.cs
 */
public sealed class StubInventoryService : IInventoryService
{
    private readonly Dictionary<string, int> _stockLevels;

    public StubInventoryService(Dictionary<string, int> stockLevels)
    {
        _stockLevels = stockLevels;
    }

    public bool HasStock(string sku, int quantity) =>
        _stockLevels.TryGetValue(sku, out int available) && available >= quantity;

    public void Reserve(string sku, int quantity)
    {
        if (_stockLevels.ContainsKey(sku))
        {
            _stockLevels[sku] -= quantity;
        }
    }
}
