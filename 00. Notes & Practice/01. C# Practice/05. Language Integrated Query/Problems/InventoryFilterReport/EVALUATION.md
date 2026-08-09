# Inventory Filter Report — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| `OfType<ProductRow>` strips non-products | 15 |
| `CountInCategory` uses Count(predicate) | 10 |
| `TotalInventoryValue` Sum with selector | 15 |
| `AverageUnitCost` null-safe on empty category | 20 |
| `WeightedAverageCost` Aggregate fold | 20 |
| LowStock filter correct | 10 |
| Chained demo query | 10 |

## AI Review Prompt

Evaluate InventoryFilterReport against PROBLEM.md. Score /100. Verify OfType, aggregation empty guards, Aggregate fold, Count(predicate). Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Strings in mixed feed excluded from product stats
- [ ] Empty category average returns null, not exception
- [ ] Weighted average matches manual calculation on seed data
- [ ] CountInCategory avoids unnecessary Where().Count()
