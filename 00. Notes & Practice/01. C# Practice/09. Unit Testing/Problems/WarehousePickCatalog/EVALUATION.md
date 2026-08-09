# Warehouse Pick Catalog — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| SkuCatalog + PickListBuilder | 25 |
| SkuCatalogFixture seeds ≥3 SKUs | 15 |
| IClassFixture test | 20 |
| CollectionDefinition + two collection tests | 25 |
| Shared fixture same Count across classes | 10 |
| Demo Main | 5 |

## AI Review Prompt

Evaluate WarehousePickCatalog against PROBLEM.md for IClassFixture and ICollectionFixture usage. Score /100, verdict.

---

## Model Answer Checklist

- [ ] Unknown SKU skipped in pick path
- [ ] Collection tests A and B see same catalog instance
- [ ] Class fixture test does not re-seed per test method unnecessarily
