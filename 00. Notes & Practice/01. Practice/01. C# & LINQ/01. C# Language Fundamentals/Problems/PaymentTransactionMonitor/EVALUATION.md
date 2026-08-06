# Payment Transaction Monitor — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Net balance | 15 |
| Fee tiers (3 free, then 1/2) chronological | 35 |
| Suspicious 60s window | 30 |
| Demo + structure | 10 |
| Constraints | 10 |

## AI Review Prompt

Evaluate AccountManager vs PROBLEM.md. Score /100 with fee and suspicious-window test scenarios, verdict.

---

## Model Answer Checklist

- [ ] Fees chronological per account
- [ ] 4th txn on same account triggers fee
- [ ] Two debits within 60s flags account
- [ ] Suspicious list sorted ascending
