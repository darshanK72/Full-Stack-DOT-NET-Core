using System.Collections.Generic;
using MockingAndTestDoubles.Models;
using MockingAndTestDoubles.Services;
using MockingAndTestDoubles.TestDoubles;
using Xunit;

namespace MockingAndTestDoubles.Tests;

/*
 * SECTION 9: MANUAL TEST DOUBLES — STUB + FAKE WITHOUT MOQ
 *
 * These tests inject hand-written doubles from TestDoubles/:
 *   StubInventoryService — canned stock levels
 *   FakeEmailSender      — captures sent messages in SentMessages
 *
 * AAA assumed from chapter 01. Assert behavior (result + observable state),
 * not internal call sequences.
 */
public sealed class OrderServiceManualDoubleTests
{
    [Fact]
    public void PlaceOrder_WithStubAndFake_SucceedsReservesStockAndSendsEmail()
    {
        // Arrange — stub owns stock; fake records emails
        var stock = new Dictionary<string, int>
        {
            ["WIDGET-01"] = 10,
            ["GADGET-02"] = 5,
        };
        IInventoryService inventory = new StubInventoryService(stock);
        FakeEmailSender email = new FakeEmailSender();
        OrderService service = new OrderService(inventory, email);

        Order order = new Order
        {
            OrderId = 1001,
            CustomerEmail = "buyer@example.com",
            Lines =
            [
                new OrderLine { Sku = "WIDGET-01", Quantity = 2 },
                new OrderLine { Sku = "GADGET-02", Quantity = 1 },
            ],
            Total = 49.99m,
        };

        // Act
        OrderResult result = service.PlaceOrder(order);

        // Assert — behavior via return value, stock mutation, and fake state
        Assert.True(result.Success);
        Assert.Equal(1001, result.OrderId);
        Assert.Equal(8, stock["WIDGET-01"]);
        Assert.Equal(4, stock["GADGET-02"]);
        Assert.Single(email.SentMessages);
        Assert.Equal(("buyer@example.com", 1001, 49.99m), email.SentMessages[0]);
    }

    [Fact]
    public void PlaceOrder_WhenStubReportsLowStock_FailsWithoutEmailOrReservation()
    {
        var stock = new Dictionary<string, int> { ["WIDGET-01"] = 1 };
        IInventoryService inventory = new StubInventoryService(stock);
        FakeEmailSender email = new FakeEmailSender();
        OrderService service = new OrderService(inventory, email);

        Order order = new Order
        {
            OrderId = 1002,
            CustomerEmail = "buyer@example.com",
            Lines = [new OrderLine { Sku = "WIDGET-01", Quantity = 5 }],
            Total = 25.00m,
        };

        OrderResult result = service.PlaceOrder(order);

        Assert.False(result.Success);
        Assert.Contains("WIDGET-01", result.FailureReason);
        Assert.Equal(1, stock["WIDGET-01"]);
        Assert.Empty(email.SentMessages);
    }
}
