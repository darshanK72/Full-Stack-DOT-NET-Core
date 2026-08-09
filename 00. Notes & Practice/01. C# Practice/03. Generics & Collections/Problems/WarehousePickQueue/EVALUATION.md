# Warehouse Pick Queue — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Queue FIFO ticket ordering | 25 |
| Stack LIFO undo for units | 25 |
| TryStartNextPick / empty queue safe | 15 |
| BFS Queue frontier finds G | 20 |
| Active ticket state coherent | 10 |
| Demo transcript | 5 |

## AI Review Prompt

Evaluate WarehousePickQueue against PROBLEM.md. Score /100. FIFO order preserved, undo restores previous units, BFS shortest path on sample grid. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] First enqueued ticket dequeued first
- [ ] Undo reverts only one adjustment level
- [ ] BFS does not use Stack for frontier
