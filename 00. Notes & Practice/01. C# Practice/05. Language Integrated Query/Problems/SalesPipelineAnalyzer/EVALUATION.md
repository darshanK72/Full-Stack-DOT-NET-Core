# Sales Pipeline Analyzer — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Deferred `FilterByRegion` / `SelectAmounts` (no premature ToList) | 20 |
| Query syntax equivalent to method syntax filter | 15 |
| `TotalForRegion` uses terminal `Sum` correctly | 15 |
| `MaterializeRegion` uses `ToList` snapshot | 15 |
| Execute counter reflects enumerations (deferred runs on demand) | 20 |
| Case-insensitive region filter | 10 |
| Demo shows build vs execute distinction | 5 |

## AI Review Prompt

Evaluate SalesPipelineAnalyzer against PROBLEM.md. Score /100. Verify deferred execution, query vs method syntax equivalence, ToList materialization, and enumeration counting. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Pipeline before foreach does not increment counter (or increments only on terminal ops used)
- [ ] Second foreach on same deferred query re-runs source
- [ ] `MaterializeRegion` snapshot stable if source changes later
- [ ] Query syntax and method syntax return same count for "east"
- [ ] Missing `using System.Linq` would not compile — student included it
