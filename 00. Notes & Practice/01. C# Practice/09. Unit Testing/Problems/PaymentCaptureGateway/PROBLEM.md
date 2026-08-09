---
module: 09. Unit Testing
difficulty: Hard
chapters: 04 Mocking & Test Doubles
domain: PaymentProcessing
---

# Payment Capture Gateway

Build a **.NET 8 solution** with a payment orchestrator and **Moq-based xUnit tests** — Setup, Returns, Verify, Throws, and Callback.

## Business context

Checkout captures card payments only after fraud screening passes. Tests must simulate processor failures and verify the orchestrator never captures when fraud blocks the sale.

## Solution layout

```
PaymentCaptureGateway/
  Program.cs
  PaymentCaptureGateway.Tests/
    PaymentOrchestratorMoqTests.cs
```

## Definitions (production)

**Record `PaymentRequest`:** `TransactionId` (int), `CustomerId`, `Amount` (decimal > 0)

**Record `PaymentResult`:** `Captured` (bool), `TransactionId`, `DeclineReason` (string?)

**Interface `IFraudScreen`:** `bool Approve(PaymentRequest request)`

**Interface `IPaymentProcessor`:** `void Capture(PaymentRequest request)`

**Class `PaymentOrchestrator`**

- Constructor `(IFraudScreen fraud, IPaymentProcessor processor)`
- `PaymentResult ProcessPayment(PaymentRequest request)` — reject invalid amount ≤ 0 with `ArgumentOutOfRangeException`; if fraud rejects, return `Captured=false`, `DeclineReason="Fraud"` without calling Capture; if approved, call Capture and return `Captured=true`

## Tests (Moq — complete TODOs)

| Test | Moq patterns |
|------|--------------|
| `ProcessPayment_WhenFraudApproves_CapturesOnce` | Setup Approve true; Verify Capture Times.Once |
| `ProcessPayment_WhenFraudDeclines_NeverCaptures` | Setup Approve false; Verify Capture Times.Never |
| `ProcessPayment_WhenProcessorThrows_ReturnsNotCaptured` | Setup Capture Throws; result Captured false |
| `ProcessPayment_CaptureCallback_RecordsAmount` | Callback captures amount from request |

Use `It.IsAny<PaymentRequest>()` where exact match is not the focus.

## Demo Main

Not required beyond placeholder — focus on tests.

## Constraints

- Moq package in test project
- Assert public behavior, not call order trivia

## Non-goals

Manual doubles (see HotelReservationNotifier), integration with Stripe

## Evaluation

[EVALUATION.md](EVALUATION.md)
