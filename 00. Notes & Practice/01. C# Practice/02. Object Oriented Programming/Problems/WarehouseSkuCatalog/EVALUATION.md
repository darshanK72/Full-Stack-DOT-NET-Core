# Warehouse SKU Catalog — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| SkuItem validation + QuantityOnHand | 15 |
| Copy constructor | 10 |
| ToString override | 5 |
| Int + string indexers | 25 |
| Add/Remove duplicate rules | 15 |
| Static InstancesCreated tracking | 15 |
| CreateEmpty factory | 15 |

## AI Review Prompt

Evaluate WarehouseSkuCatalog against PROBLEM.md. Score /100, indexer/property issues, strengths, verdict.

---

## Model Answer Checklist

- [ ] Negative quantity rejected
- [ ] String indexer case-insensitive
- [ ] Int indexer set replaces item
- [ ] InstancesCreated increments per catalog constructed
