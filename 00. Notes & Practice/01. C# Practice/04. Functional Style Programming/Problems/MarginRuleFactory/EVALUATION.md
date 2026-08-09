# Margin Rule Factory — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| MakeCounter independent instances | 20 |
| MakeMarginRule capture + floor math | 25 |
| BuildBrokenIndexFuncs shows loop bug | 15 |
| BuildFixedIndexFuncs local copy fix | 15 |
| MutateSharedCaptureDemo final total 4 | 10 |
| QuoteSession uses shared counter delegate | 10 |
| Rounding AwayFromZero to 2 decimals | 5 |

## AI Review Prompt

Evaluate MarginRuleFactory against PROBLEM.md. Score /100. Check closure capture, factory isolation, loop fix, shared mutation, and margin floor formula. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Two counters do not share sequence
- [ ] Broken array elements all return last index value
- [ ] Fixed array returns 0..count-1
- [ ] Shared capture demo ends at 4
- [ ] Below-margin price rounds to cost / (1 - pct)
- [ ] QuoteSession QuotesRun equals number of Quote calls
