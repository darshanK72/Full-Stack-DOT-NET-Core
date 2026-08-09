# Bank Account Ledger — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Encapsulated balance + validation | 20 |
| BalanceChanged event + EventArgs | 25 |
| Equals/GetHashCode by account number | 20 |
| Ledger duplicate guard + totals | 20 |
| ToString currency format | 10 |
| Subscribe/unsubscribe demo | 5 |

## AI Review Prompt

Evaluate BankAccountLedger against PROBLEM.md. Score /100, encapsulation/event safety, equality/hash issues, strengths, verdict.

---

## Model Answer Checklist

- [ ] Cannot set Balance directly
- [ ] Withdraw fails when balance too low
- [ ] Event fires with correct old/new/delta
- [ ] Dictionary keyed by account works with overridden equality
- [ ] Handler unsubscribed stops notifications
