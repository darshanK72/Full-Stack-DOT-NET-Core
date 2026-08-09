# Pick Ticket Enumerator — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| PickBatch IEnumerable + GetEnumerator | 25 |
| yield return HeavyLines lazy filter | 20 |
| SkuSegments iterator | 10 |
| Manual IEnumerator with using/Dispose | 15 |
| Modify-during-foreach demo | 15 |
| TotalWeight foreach sum | 10 |
| PickLine TotalWeightKg | 5 |

## AI Review Prompt

Evaluate PickTicketEnumerator against PROBLEM.md. Score /100. Custom enumerable works with foreach, yield lazy, modification exception caught. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] PickBatch works in foreach without exposing internal array directly
- [ ] HeavyLines not executed until enumerated
- [ ] DemoModifyDuringForeach returns collection modified message
