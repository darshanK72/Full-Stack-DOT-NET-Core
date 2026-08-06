using System;

namespace ExceptionHandling.Models;

/*
 * Custom exception for business-rule violations on an order (quantity, total, id).
 * Derives from Exception — the standard base for application-specific errors.
 */
public class InvalidOrderException : Exception
{
    public string OrderId { get; }

    public InvalidOrderException(string orderId, string message)
        : base(message)
    {
        OrderId = orderId;
    }

    public InvalidOrderException(string orderId, string message, Exception innerException)
        : base(message, innerException)
    {
        OrderId = orderId;
    }
}
