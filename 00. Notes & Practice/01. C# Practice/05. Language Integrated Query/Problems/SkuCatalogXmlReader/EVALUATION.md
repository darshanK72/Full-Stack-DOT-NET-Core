# SKU Catalog XML Reader — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Descendants vs Elements usage | 15 |
| Project SkuRow from attributes/children | 25 |
| ActiveHardware filter | 15 |
| Average null-safe | 10 |
| DeactivateSku mutates attribute | 20 |
| SummaryElement aggregates | 15 |

## AI Review Prompt

Evaluate SkuCatalogXmlReader against PROBLEM.md. Score /100. Verify axis methods, LINQ on XML, mutation, typed casts. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Descendants finds all sku elements under catalog
- [ ] Inactive software excluded from ActiveHardware
- [ ] DeactivateSku visible on second query without re-parse
- [ ] Missing price handled without unhandled exception
