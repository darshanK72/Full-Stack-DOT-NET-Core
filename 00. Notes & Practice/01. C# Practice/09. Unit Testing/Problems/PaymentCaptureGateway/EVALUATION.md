# Payment Capture Gateway — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| PaymentOrchestrator rules | 25 |
| Setup + Verify Capture Once/Never | 25 |
| Throws simulation test | 20 |
| Callback amount capture | 20 |
| Avoids over-mocking internals | 10 |

## AI Review Prompt

Evaluate PaymentCaptureGateway against PROBLEM.md for Moq Setup/Verify/Throws/Callback. Score /100, verdict.

---

## Model Answer Checklist

- [ ] Fraud decline skips Capture entirely
- [ ] Processor exception handled without unhandled throw from test
- [ ] Verify uses Times.Once/Never appropriately
