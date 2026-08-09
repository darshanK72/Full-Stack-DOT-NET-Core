# Community Bank Ledger — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| BankAccount lock on dedicated gate | 20 |
| Deposit/Withdraw validation | 15 |
| Interlocked vs unsafe race demo | 15 |
| ReaderWriterLockSlim read/write paths | 20 |
| Transfer deadlock-safe ordering | 20 |
| Dispose ReaderWriterLockSlim | 10 |

## AI Review Prompt

Evaluate CommunityBankLedger against PROBLEM.md. Score /100. lock, Interlocked, RW lock, deadlock prevention. Strengths, gaps, verdict.

---

## Model Answer Checklist

- [ ] Unsafe counter below expected total
- [ ] Interlocked counter equals workers × increments
- [ ] Transfer does not deadlock when swapping from/to in parallel threads
