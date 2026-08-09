# Generic Repository — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| `IRepository<TKey,TValue>` with `notnull` on TKey | 10 |
| `InMemoryRepository` TryGet/Set/Keys | 20 |
| `Quantity<TUnit>` struct + struct constraint | 15 |
| `Swap<T>` generic method | 15 |
| `CreateDefault<T>()` with `new()` constraint | 15 |
| `CompareOrdered<T>` with `IComparable<T>` | 10 |
| `DescribeDefault<T>` using typeof/default | 10 |
| Demo Main exercises all pieces | 5 |

## AI Review Prompt

Evaluate GenericRepository against PROBLEM.md. Score /100. Check generic constraints compile, closed types used correctly (string/int repos distinct), no boxing in int storage. List gaps, strengths, verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] `List<int>`-style — repository stores `TValue` without cast
- [ ] Missing key: TryGet returns false, no exception
- [ ] `where TKey : notnull` on interface/class
- [ ] Swap works for value and reference types in demo
