# SKU Catalog Registry — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Dictionary with case-insensitive keys | 20 |
| Register duplicate rejection | 15 |
| TryGetValue pattern | 15 |
| List insertion order + shared Product refs | 15 |
| UpdatePrice on dictionary entry | 15 |
| GetRequired / KeyNotFound demo | 10 |
| List scan vs dictionary comparison note | 10 |

## AI Review Prompt

Evaluate SkuCatalogRegistry against PROBLEM.md. Score /100. Check TryGetValue preferred over double lookup, case-insensitive SKU rules. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] "wh-1001" finds "WH-1001" if registered
- [ ] Duplicate register returns false, Count unchanged
- [ ] UpdatePrice visible in both dictionary and list entry
