# Catalog Projection Flattener — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| SelectMany with result selector preserves OrderId/Customer | 30 |
| Select vs SelectMany contrast in NestedLinesOnly | 15 |
| Enumerable.Range count semantics | 15 |
| Empty<T> for missing category | 15 |
| TopPickRows OrderByDescending + Take | 15 |
| SelectMany tag flatten (if implemented) | 10 |

## AI Review Prompt

Evaluate CatalogProjectionFlattener against PROBLEM.md. Score /100. Verify SelectMany flatten, Range, Empty, Take. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Select on Lines yields IEnumerable of collections not flat rows
- [ ] FlatPickList row count = sum of line counts
- [ ] Range(1, 5) yields 1..5 not 1..4
- [ ] Empty category foreach runs zero iterations
