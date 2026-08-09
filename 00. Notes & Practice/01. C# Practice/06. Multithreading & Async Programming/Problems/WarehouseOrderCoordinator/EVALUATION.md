# Warehouse Order Coordinator — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Task.Run pipeline + ValidateAsync | 15 |
| SemaphoreSlim pick gating with finally release | 20 |
| Task.WhenAll batch validation | 15 |
| TaskCompletionSource + AwaitPaymentAsync | 20 |
| ContinueWith OnlyOnRanToCompletion / fault -1 | 15 |
| CancellationToken propagation | 10 |
| Gateway callback simulation | 5 |

## AI Review Prompt

Evaluate WarehouseOrderCoordinator against PROBLEM.md. Score /100. Task patterns, TCS, WhenAll, SemaphoreSlim. Strengths, gaps, verdict.

---

## Model Answer Checklist

- [ ] Pick gate limits concurrent picks to 2
- [ ] Invalid order (Total <= 0) excluded from ValidateAll result
- [ ] Payment await completes after simulated callback
