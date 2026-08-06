# Food Delivery Platform — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Status filtering (DELIVERED only for revenue) | 30 |
| Average prep per restaurant | 25 |
| Top restaurant tie-break | 20 |
| Seeding demo + structure | 15 |
| Constraints | 10 |

## AI Review Prompt

Evaluate OrderManager implementation vs PROBLEM.md. Score /100, method-level issues, verdict.

---

## Model Answer Checklist

- [ ] Cancelled orders excluded from revenue
- [ ] Top restaurant tie → lower id
- [ ] PrepMinutes averaged correctly
