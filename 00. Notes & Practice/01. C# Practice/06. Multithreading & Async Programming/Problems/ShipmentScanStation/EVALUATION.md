# Shipment Scan Station — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| ParameterizedThreadStart + per-shipment Thread | 20 |
| Background threads + named threads | 10 |
| Join / WaitAll timeout semantics | 15 |
| CancellationToken cooperative loop | 20 |
| ThreadLocalScanLogger per-thread isolation | 15 |
| Locked result collection | 10 |
| Create factory validation | 10 |

## AI Review Prompt

Evaluate ShipmentScanStation against PROBLEM.md. Score /100. Check Thread (not Task), cooperative cancel, thread-local logging, lock on results. Strengths, gaps, verdict.

---

## Model Answer Checklist

- [ ] All threads are background
- [ ] WaitAll returns false when timeout expires before all join
- [ ] Cancelled run shows fewer boxes than BoxCount
- [ ] Each worker's ScanLog only contains its shipment id
