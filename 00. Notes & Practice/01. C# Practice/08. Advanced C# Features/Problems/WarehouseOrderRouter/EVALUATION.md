# Warehouse Order Router — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Expression-bodied LineTotal / IsHighValue | 10 |
| Digit separators in constants | 5 |
| switch pattern routing + local function | 25 |
| Tuple return Route + surcharges | 15 |
| TryApplyQuantityOverride with out var | 15 |
| ref return FindOrderById | 15 |
| CountByPriority tuple + deconstruction in Main | 10 |
| In-place Critical update via ref | 5 |

## AI Review Prompt

Evaluate WarehouseOrderRouter against PROBLEM.md. Score /100. Verify C# 7 patterns: switch tuples, out vars, ref return, local functions, tuple deconstruction. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] High-value express routes to B2 with surcharge 250
- [ ] Standard order aisle C1, surcharge 0
- [ ] Invalid quantity text leaves quantity unchanged, returns false
- [ ] ref return mutates array element priority in place
