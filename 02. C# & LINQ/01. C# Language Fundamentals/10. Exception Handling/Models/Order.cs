using System;

namespace ExceptionHandling.Models;

/*
 * Order entity used by the payment scenario. Validate() throws InvalidOrderException
 * when business rules fail — callers catch the specific type before a general catch.
 */
public class Order
{
    public string OrderId { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    public Order(string orderId, int quantity, decimal unitPrice)
    {
        OrderId = orderId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public decimal Total => Quantity * UnitPrice;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(OrderId))
        {
            throw new InvalidOrderException(OrderId ?? string.Empty, "Order id is required.");
        }

        if (Quantity <= 0)
        {
            throw new InvalidOrderException(OrderId, "Quantity must be greater than zero.");
        }

        if (UnitPrice <= 0m)
        {
            throw new InvalidOrderException(OrderId, "Unit price must be greater than zero.");
        }
    }
}
