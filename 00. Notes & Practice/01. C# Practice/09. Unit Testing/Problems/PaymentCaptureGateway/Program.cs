/*
 * PROBLEM: Payment Capture Gateway
 *
 * Checkout captures card payments only after fraud screening passes. Moq tests
 * simulate processor failures and verify capture is skipped when fraud blocks.
 *
 * This exercise covers:
 *   ch04 — Mock<T>, Setup, Returns, .Object injection
 *   ch04 — Verify with Times.Once / Times.Never
 *   ch04 — Setup Throws for dependency failure
 *   ch04 — Callback to capture invocation arguments
 */

using System;

namespace PaymentProcessing
{
    public sealed record PaymentRequest(int TransactionId, string CustomerId, decimal Amount);

    public sealed record PaymentResult(bool Captured, int TransactionId, string? DeclineReason);

    public interface IFraudScreen
    {
        bool Approve(PaymentRequest request);
    }

    public interface IPaymentProcessor
    {
        void Capture(PaymentRequest request);
    }

    /*
     * Two-phase payment: fraud screen then capture.
     */
    public sealed class PaymentOrchestrator
    {
        private readonly IFraudScreen _fraud;
        private readonly IPaymentProcessor _processor;

        public PaymentOrchestrator(IFraudScreen fraud, IPaymentProcessor processor)
        {
            _fraud = fraud;
            _processor = processor;
        }

        /*
         * Amount <= 0 throws ArgumentOutOfRangeException.
         * Fraud reject → Captured false, DeclineReason "Fraud", no Capture call.
         * Fraud approve → Capture then Captured true.
         */
        public PaymentResult ProcessPayment(PaymentRequest request)
        {
            // TODO: validate amount; orchestrate fraud + capture
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PaymentCaptureGateway — implement tests in PaymentCaptureGateway.Tests");
        }
    }
}
