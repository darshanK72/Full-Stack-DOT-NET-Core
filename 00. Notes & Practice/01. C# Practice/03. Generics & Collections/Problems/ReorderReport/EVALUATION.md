# Reorder Report — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| SortedList auto key order + Keys[i]/Values[i] | 30 |
| SortedDictionary ordered foreach | 20 |
| StringComparer.OrdinalIgnoreCase tag board | 20 |
| TryGetQuantity / Remove | 15 |
| No LINQ OrderBy for main report | 10 |
| Demo shows sorted output | 5 |

## AI Review Prompt

Evaluate ReorderReport against PROBLEM.md. Score /100. Sorted iteration native, index access on SortedList, custom comparer on tag board. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] PrintReport alphabetical SKU without Sort()
- [ ] Tag board Count 1 after two case variants
- [ ] SortedList LowestSku matches Keys[0]
