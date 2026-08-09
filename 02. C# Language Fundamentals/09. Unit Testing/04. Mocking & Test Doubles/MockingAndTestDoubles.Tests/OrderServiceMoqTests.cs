using System;
using System.Collections.Generic;
using MockingAndTestDoubles.Models;
using MockingAndTestDoubles.Services;
using Moq;
using Xunit;

namespace MockingAndTestDoubles.Tests;

/*
 * SECTION 10: MOQ — MOCK<T>, SETUP, VERIFY, THROWS, CALLBACK
 *
 * Moq generates runtime proxies for interfaces. Each test follows
 * Arrange (mocks + Setup) → Act (PlaceOrder) → Assert + Verify.
 *
 *   Returns   — stub a method return value
 *   Throws    — simulate dependency failure
 *   Callback  — run code when a matched call occurs (capture side effects)
 *   Verify    — spy: assert a method was (or was not) invoked
 *   It.IsAny  — match any argument when exact values are not the focus
 */
public sealed class OrderServiceMoqTests
{
    private static Order CreateSampleOrder() =>
        new()
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

    [Fact]
    public void PlaceOrder_WhenStockAvailable_ReturnsSuccess()
    {
        Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
        inventoryMock.Setup(i => i.HasStock("GADGET-02", 1)).Returns(true);

        Mock<IEmailSender> emailMock = new Mock<IEmailSender>();
        OrderService service = new OrderService(inventoryMock.Object, emailMock.Object);

        OrderResult result = service.PlaceOrder(CreateSampleOrder());

        Assert.True(result.Success);
        Assert.Equal(1001, result.OrderId);
    }

    [Fact]
    public void PlaceOrder_WhenStockAvailable_ReservesEachLineAndSendsEmailOnce()
    {
        Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);

        Mock<IEmailSender> emailMock = new Mock<IEmailSender>();
        OrderService service = new OrderService(inventoryMock.Object, emailMock.Object);

        service.PlaceOrder(CreateSampleOrder());

        inventoryMock.Verify(i => i.Reserve("WIDGET-01", 2), Times.Once);
        inventoryMock.Verify(i => i.Reserve("GADGET-02", 1), Times.Once);
        emailMock.Verify(
            e => e.SendOrderConfirmation("buyer@example.com", 1001, 49.99m),
            Times.Once);
    }

    [Fact]
    public void PlaceOrder_WhenFirstLineOutOfStock_ReturnsFailureWithoutSideEffects()
    {
        Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(false);

        Mock<IEmailSender> emailMock = new Mock<IEmailSender>();
        OrderService service = new OrderService(inventoryMock.Object, emailMock.Object);

        OrderResult result = service.PlaceOrder(CreateSampleOrder());

        Assert.False(result.Success);
        Assert.Contains("WIDGET-01", result.FailureReason);

        inventoryMock.Verify(
            i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
        emailMock.Verify(
            e => e.SendOrderConfirmation(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
            Times.Never);
    }

    [Fact]
    public void PlaceOrder_WhenSecondLineOutOfStock_DoesNotReserveFirstLine()
    {
        Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
        inventoryMock.Setup(i => i.HasStock("GADGET-02", 1)).Returns(false);

        Mock<IEmailSender> emailMock = new Mock<IEmailSender>();
        OrderService service = new OrderService(inventoryMock.Object, emailMock.Object);

        OrderResult result = service.PlaceOrder(CreateSampleOrder());

        Assert.False(result.Success);
        inventoryMock.Verify(i => i.Reserve("WIDGET-01", 2), Times.Never);
        inventoryMock.Verify(i => i.Reserve("GADGET-02", 1), Times.Never);
        emailMock.Verify(
            e => e.SendOrderConfirmation(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
            Times.Never);
    }

    [Fact]
    public void PlaceOrder_WhenStockAvailable_ChecksStockBeforeEveryReserve()
    {
        Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);

        Mock<IEmailSender> emailMock = new Mock<IEmailSender>();
        OrderService service = new OrderService(inventoryMock.Object, emailMock.Object);

        service.PlaceOrder(CreateSampleOrder());

        inventoryMock.Verify(i => i.HasStock("WIDGET-01", 2), Times.Once);
        inventoryMock.Verify(i => i.HasStock("GADGET-02", 1), Times.Once);
    }

    [Fact]
    public void PlaceOrder_WhenReserveThrows_PropagatesException()
    {
        Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
        inventoryMock
            .Setup(i => i.Reserve("WIDGET-01", 2))
            .Throws(new InvalidOperationException("Warehouse offline"));

        Mock<IEmailSender> emailMock = new Mock<IEmailSender>();
        OrderService service = new OrderService(inventoryMock.Object, emailMock.Object);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => service.PlaceOrder(CreateSampleOrder()));

        Assert.Equal("Warehouse offline", ex.Message);
        emailMock.Verify(
            e => e.SendOrderConfirmation(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
            Times.Never);
    }

    [Fact]
    public void PlaceOrder_WhenStockAvailable_CallbackCapturesReservedSkus()
    {
        Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);

        List<string> reservedSkus = [];
        inventoryMock
            .Setup(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, int>((sku, quantity) => reservedSkus.Add($"{sku}:{quantity}"));

        Mock<IEmailSender> emailMock = new Mock<IEmailSender>();
        OrderService service = new OrderService(inventoryMock.Object, emailMock.Object);

        service.PlaceOrder(CreateSampleOrder());

        Assert.Equal(["WIDGET-01:2", "GADGET-02:1"], reservedSkus);
    }
}
