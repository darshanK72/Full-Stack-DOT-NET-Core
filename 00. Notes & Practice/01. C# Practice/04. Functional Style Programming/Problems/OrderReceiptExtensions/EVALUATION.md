# Order Receipt Extensions — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| StringExtensions Masked + IsValidOrderId | 20 |
| DateTimeExtensions formatting + StartOfDay | 15 |
| OrderLineExtensions ToReceiptLine | 15 |
| IEnumerable TotalAmount (Sum allowed here) | 15 |
| Generic Clamp with IComparable constraint | 20 |
| Static class + `this` parameter rules | 10 |
| Demo uses instance + static call styles | 5 |

## AI Review Prompt

Evaluate OrderReceiptExtensions against PROBLEM.md. Score /100. Check extension method rules, generic Clamp, null handling, and receipt formatting. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Masked hides all but last 4 chars
- [ ] Empty string returns "(empty)"
- [ ] Invalid order id (wrong length) returns false
- [ ] TotalAmount matches manual sum of line totals
- [ ] Clamp throws when min > max
- [ ] Extensions compile without extending types directly
