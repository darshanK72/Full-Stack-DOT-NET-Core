/*
 * PROBLEM: Callback Orchestrator
 *
 * A checkout validation service runs custom ValidationRule delegates alongside
 * built-in Action and Func callbacks to report failures and compute processing fees.
 * Capstone mixing chapter 01 custom delegates with chapter 05 built-ins.
 *
 * This exercise covers:
 *   ch01 — custom delegate ValidationRule vs Func/Action shapes
 *   ch02 — target-typed lambdas for rules and fee calculator
 *   ch05 — Action<string> reporter and Func<Order, decimal> feeCalc
 *   ch06 — optional capture in rule lambdas (preview)
 */

using System;

namespace CallbackPipeline
{
    /*
     * Domain validation rule — bool result per order.
     * Same shape as Func<Order, bool> but named for the public contract.
     */
    delegate bool ValidationRule(Order order);

    /*
     * Order submitted for validation and fee calculation.
     */
    sealed class Order
    {
        public int Id { get; }
        public string Customer { get; }
        public decimal Subtotal { get; }
        public int ItemCount { get; }

        public Order(int id, string customer, decimal subtotal, int itemCount)
        {
            Id = id;
            Customer = customer;
            Subtotal = subtotal;
            ItemCount = itemCount;
        }
    }

    /*
     * Orchestrates validation rules and fee calculation callbacks.
     * No Console I/O — all output flows through reporter Action.
     */
    sealed class OrderValidator
    {
        /*
         * Runs each rule in order; on first failure reports via reporter and returns false.
         *
         * order — must not be null
         * rules — must not be null or empty
         * reporter — must not be null; invoked with human-readable messages
         * feeCalc — must not be null; invoked only when all rules pass
         *
         * Returns true when every rule passes; false on first failure.
         * When true, invokes reporter with success message including fee from feeCalc(order).
         */
        public bool Validate(
            Order order,
            ValidationRule[] rules,
            Action<string> reporter,
            Func<Order, decimal> feeCalc)
        {
            // TODO: null checks; loop rules; on fail report and return false
            // TODO: on success compute feeCalc(order), report success + fee, return true
            throw new NotImplementedException();
        }
    }

    class Program
    {
        /*
         * Sample named rule — assignable to ValidationRule via method group.
         */
        static bool HasPositiveSubtotal(Order order)
        {
            // TODO: return order.Subtotal > 0
            throw new NotImplementedException();
        }

        /*
         * Sample named rule for minimum item count.
         */
        static bool HasAtLeastOneItem(Order order)
        {
            // TODO: return order.ItemCount >= 1
            throw new NotImplementedException();
        }

        static void Main(string[] args)
        {
            // TODO: build OrderValidator and sample orders (valid + invalid)
            // TODO: ValidationRule[] mixing method groups and lambdas
            // TODO: Action<string> reporter writing to Console
            // TODO: Func<Order, decimal> feeCalc (e.g. flat fee or percent of subtotal)
            // TODO: call Validate for each order and print pass/fail outcome
            throw new NotImplementedException();
        }
    }
}
