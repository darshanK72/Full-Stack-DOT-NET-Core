# Order Fulfillment Pipeline — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Custom `ShippingRule` + `OrderAuditHandler` declarations | 10 |
| Three shipping rule methods + `SelectShippingRule` | 20 |
| Multicast register/unregister (`+=` / `-=`) | 15 |
| `Delegate.Combine` / `Delegate.Remove` helpers | 15 |
| Null-safe audit invoke (`?.Invoke`) | 15 |
| `QuoteShipping` validation + audit messages | 15 |
| Null-safe rule invoke returns 0 | 10 |

## AI Review Prompt

Evaluate OrderFulfillmentPipeline against PROBLEM.md. Score /100. Check custom delegate usage, multicast ordering, Combine/Remove casting, null-safe invoke patterns, and shipping math. List strengths, bugs, and verdict.

---

## Model Answer Checklist

- [ ] Unknown service level returns null from selector
- [ ] Both audit handlers fire on first quote
- [ ] After unregister, only one handler fires
- [ ] Null rule invoke returns 0 without exception
- [ ] ExpressRate delegates to StandardRate via method group
- [ ] Invalid ShipmentRequest throws before rule invoke
