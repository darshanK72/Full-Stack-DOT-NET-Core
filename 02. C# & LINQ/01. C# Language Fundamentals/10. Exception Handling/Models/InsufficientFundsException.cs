using System;

namespace ExceptionHandling.Models;

/*
 * Thrown when a payment amount exceeds the available account balance.
 * Inherits from SystemException branch via Exception (see Program.cs hierarchy section).
 */
public class InsufficientFundsException : Exception
{
    public decimal RequestedAmount { get; }
    public decimal AvailableBalance { get; }

    public InsufficientFundsException(decimal requested, decimal available)
        : base($"Payment of {requested:C} declined — available balance is {available:C}.")
    {
        RequestedAmount = requested;
        AvailableBalance = available;
    }
}
