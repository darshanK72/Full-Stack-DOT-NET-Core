using System;
using Moq;
using PaymentProcessing;
using Xunit;

namespace PaymentCaptureGateway.Tests;

public sealed class PaymentOrchestratorMoqTests
{
    private static PaymentRequest SampleRequest() =>
        new PaymentRequest(5001, "cust-42", 149.99m);

    [Fact]
    public void ProcessPayment_WhenFraudApproves_CapturesOnce()
    {
        // TODO: fraud Setup Approve true; processor mock; Verify Capture Times.Once; Assert Captured
        throw new NotImplementedException();
    }

    [Fact]
    public void ProcessPayment_WhenFraudDeclines_NeverCaptures()
    {
        // TODO: Approve false; Verify Capture Times.Never; DeclineReason "Fraud"
        throw new NotImplementedException();
    }

    [Fact]
    public void ProcessPayment_WhenProcessorThrows_ReturnsNotCaptured()
    {
        // TODO: Setup Capture Throws InvalidOperationException; Assert Captured false
        throw new NotImplementedException();
    }

    [Fact]
    public void ProcessPayment_CaptureCallback_RecordsAmount()
    {
        // TODO: Callback on Capture to capture decimal amount; assert 149.99m recorded
        throw new NotImplementedException();
    }
}
