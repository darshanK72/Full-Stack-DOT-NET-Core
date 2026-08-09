# Warehouse Pick Sorter — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| ZoneComparer numeric suffix logic | 25 |
| OrderBy + ThenBy + ThenBy multi-key | 25 |
| A-3 before A-12 in demo output | 15 |
| Reverse() vs OrderByDescending distinction | 15 |
| OrderByDescending for priority | 10 |
| Query/method syntax sort equivalent | 10 |

## AI Review Prompt

Evaluate WarehousePickSorter against PROBLEM.md. Score /100. Verify ThenBy chain, custom comparer, Reverse vs OrderByDescending. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] A-3 sorts before A-12
- [ ] Equal priority preserves stable relative order within zone
- [ ] Second OrderBy would replace sort — student used ThenBy
- [ ] Reverse flips enumeration order of source, not re-sort by key
