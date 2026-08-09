# Shipment Planner — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| ShipmentItem + IComparable by Priority | 15 |
| Three sort paths (natural, IComparer, Comparison) | 25 |
| FindAll / FindHeavy | 15 |
| AsReadOnly snapshot + live view behavior | 20 |
| Add/Count CRUD | 10 |
| ToString output | 5 |
| Demo shows all sort orders | 10 |

## AI Review Prompt

Evaluate ShipmentPlanner against PROBLEM.md. Score /100. Check Sort overloads, read-only wrapper, no InvalidOperationException on Sort. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] SortByPriority uses Sort() without args
- [ ] PublishSnapshot returns ReadOnlyCollection
- [ ] FindHeavy uses FindAll or equivalent
