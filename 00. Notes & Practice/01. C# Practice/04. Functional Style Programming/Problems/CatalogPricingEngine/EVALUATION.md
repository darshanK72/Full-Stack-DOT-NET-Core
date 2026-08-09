# Catalog Pricing Engine — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| `PriceTransform` + `PriceFilter` delegate types | 10 |
| `ApplyToAll` preserves order | 15 |
| `FilterPrices` correct predicate use | 15 |
| `ApplyPipeline` fold + null guards | 15 |
| Expression lambda usage | 10 |
| Statement lambda usage (`AboveMinimum` + demo) | 15 |
| Method group assignment (`TenPercentOff`) | 10 |
| Validation on AddItem / pipeline | 10 |

## AI Review Prompt

Evaluate CatalogPricingEngine against PROBLEM.md. Score /100. Verify lambda forms (expression vs statement), method groups, pipeline fold, and manual iteration (no LINQ). Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] `ApplyPipeline(100m, TenPercentOff, RoundToCents)` matches sequential apply
- [ ] Filter at 10m excludes sub-10 items only
- [ ] Statement lambda factory returns new delegate per minimum
- [ ] Null transform in pipeline throws
- [ ] Rounding uses AwayFromZero to 2 decimals
