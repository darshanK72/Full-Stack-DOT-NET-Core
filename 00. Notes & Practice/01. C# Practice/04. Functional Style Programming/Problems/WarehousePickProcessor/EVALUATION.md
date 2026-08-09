# Warehouse Pick Processor — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Product record + validation assumptions | 5 |
| FindActive uses FindAll + IsActive guard | 15 |
| ForEachLine null-safe Action | 15 |
| RemoveInactive RemoveAll semantics | 15 |
| ProcessInventory filter/sum/report pipeline | 25 |
| Predicate factories MinimumStock / SkuPrefix | 10 |
| Func line total via method group | 10 |
| Null predicate throws; null report safe | 5 |

## AI Review Prompt

Evaluate WarehousePickProcessor against PROBLEM.md. Score /100. Verify Func/Action/Predicate usage, BCL list methods, null-safe Action invoke, and pick total math. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Inactive products removed regardless of shouldRemove
- [ ] ProcessInventory ignores inactive even if pickFilter passes
- [ ] Null report does not throw during picks
- [ ] ForEachLine(null) is silent no-op
- [ ] LineValue equals UnitPrice * StockQty per SKU
- [ ] MinimumStock(5) excludes low-stock active items
