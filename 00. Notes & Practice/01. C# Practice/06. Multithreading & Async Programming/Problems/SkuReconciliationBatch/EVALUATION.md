# SKU Reconciliation Batch — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| SequentialTotal baseline | 10 |
| Parallel.For thread-local merge | 25 |
| Parallel.ForEach + ParallelOptions | 20 |
| MaxDegreeOfParallelism honored | 10 |
| PLINQ TopSkusByValue | 15 |
| Totals match sequential | 10 |
| Cancellation handling | 10 |

## AI Review Prompt

Evaluate SkuReconciliationBatch against PROBLEM.md. Score /100. Parallel.For/ForEach, PLINQ, thread-local sums. Strengths, gaps, verdict.

---

## Model Answer Checklist

- [ ] Parallel and sequential totals match
- [ ] TopSkusByValue returns highest reconciliation values
- [ ] Cancelled run throws or reports cancellation cleanly
