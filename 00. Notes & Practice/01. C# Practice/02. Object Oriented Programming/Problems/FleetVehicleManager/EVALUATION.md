# Fleet Vehicle Manager — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Distance struct + operators | 20 |
| Vehicle hierarchy + Drive rules | 25 |
| Truck 500 km cap + base.Drive | 15 |
| FleetManager duplicate plate guard | 15 |
| GetByKind + fleet total distance | 15 |
| Describe overrides | 10 |

## AI Review Prompt

Evaluate FleetVehicleManager against PROBLEM.md. Score /100, inheritance/operator issues, strengths, verdict.

---

## Model Answer Checklist

- [ ] Distance + and - behave correctly
- [ ] Truck rejects >500 km drive
- [ ] Polymorphic list stores Car and Truck
- [ ] GetFleetTotalDistance uses Distance operator+
