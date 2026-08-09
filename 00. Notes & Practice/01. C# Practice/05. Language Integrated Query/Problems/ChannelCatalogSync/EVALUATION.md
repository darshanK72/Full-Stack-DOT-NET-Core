# Channel Catalog Sync — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| SkuEqualityComparer hash/equals contract | 15 |
| Union/Intersect/Except with comparer | 25 |
| DistinctBy or Distinct collapses case variants | 15 |
| CanPublish Any + All (empty fails) | 20 |
| SequenceEqual with sort note | 15 |
| IntersectBy key sequence usage | 10 |

## AI Review Prompt

Evaluate ChannelCatalogSync against PROBLEM.md. Score /100. Verify set operators, comparer, vacuous All on empty, SequenceEqual order. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] WEB and web same SKU with DistinctBy/Comparer
- [ ] Empty catalog CanPublish false (Any false)
- [ ] All on empty is true but guarded by Any first
- [ ] Except removes store SKUs from web side only
