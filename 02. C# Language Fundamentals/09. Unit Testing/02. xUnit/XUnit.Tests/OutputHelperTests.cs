using System.Collections.Generic;
using XUnit.Models;
using XUnit.Services;
using Xunit;
using Xunit.Abstractions;

namespace XUnit.Tests;

/*
 * SECTION 7: ITestOutputHelper — PER-TEST DIAGNOSTIC OUTPUT
 *
 * Console.WriteLine inside a test often disappears in CI. xUnit injects
 * ITestOutputHelper when you declare it in the test class constructor:
 *
 *   public OutputHelperTests(ITestOutputHelper output) { _output = output; }
 *   _output.WriteLine($"Subtotal: {subtotal}");
 *
 * Output appears in Test Explorer, VS Code Test Log, and:
 *   dotnet test --logger "console;verbosity=detailed"
 *
 * Each test method gets its own output buffer — lines attach to the test that wrote them.
 */
public sealed class OutputHelperTests
{
    private readonly OrderPricingService _pricing = new OrderPricingService();
    private readonly ITestOutputHelper _output;

    public OutputHelperTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CalculateSubtotal_LogsLineBreakdown()
    {
        IReadOnlyList<OrderLine> lines =
        [
            new OrderLine { Sku = "KB-001", ProductName = "Keyboard", Quantity = 1, UnitPrice = 79.99m },
            new OrderLine { Sku = "MS-010", ProductName = "Mouse", Quantity = 2, UnitPrice = 29.50m },
        ];

        decimal subtotal = _pricing.CalculateSubtotal(lines);

        foreach (OrderLine line in lines)
        {
            decimal lineTotal = line.UnitPrice * line.Quantity;
            _output.WriteLine($"{line.Sku}: {line.Quantity} × {line.UnitPrice:C} = {lineTotal:C}");
        }

        _output.WriteLine($"Subtotal: {subtotal:C}");

        Assert.Equal(138.99m, subtotal);
    }
}
