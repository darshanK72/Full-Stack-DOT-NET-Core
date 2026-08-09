# Inventory Binary Store — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Magic bytes + recordCount header | 20 |
| Typed write order id string decimal bool | 30 |
| Load mirrors write order | 25 |
| InvalidDataException on bad magic | 15 |
| using + Flush on streams | 10 |

## AI Review Prompt

Evaluate InventoryBinaryStore against PROBLEM.md. Score /100. Verify binary layout, exception on bad signature, no Console in store class. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] Round-trip restores three products accurately
- [ ] Corrupted magic throws InvalidDataException
- [ ] decimal prices survive round-trip
