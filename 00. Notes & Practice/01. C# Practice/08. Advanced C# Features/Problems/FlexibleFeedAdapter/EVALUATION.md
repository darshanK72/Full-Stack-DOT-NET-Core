# Flexible Feed Adapter — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| `FlexibleImportRow` DynamicObject overrides | 20 |
| `FromStrongTyped` mapping | 10 |
| `FromDynamic` reads all four fields | 20 |
| Missing field uses nameof in message | 15 |
| Numeric coercion | 10 |
| `GetOrDefault` returns default(T) | 15 |
| Demo catches missing Sku | 10 |

## AI Review Prompt

Evaluate FlexibleFeedAdapter against PROBLEM.md. Score /100. Verify dynamic binding, DynamicObject, nameof in errors, and default literal. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Dynamic property set/get works on FlexibleImportRow
- [ ] Missing Sku throws with "Sku" in message (from nameof)
- [ ] GetOrDefault on missing int returns 0
- [ ] Strong-typed path matches dynamic path for same values
