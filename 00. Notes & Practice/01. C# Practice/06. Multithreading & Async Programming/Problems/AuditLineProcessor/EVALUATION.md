# Audit Line Processor — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| QueueUserWorkItem + WaitCallback with state | 25 |
| CountdownEvent wait pattern | 20 |
| Results array indexed by LineId | 15 |
| Validate deterministic pass/fail | 10 |
| IsThreadPoolThread tracking | 10 |
| ThroughputReport + pool thread stats in Main | 10 |
| Stopwatch timing | 10 |

## AI Review Prompt

Evaluate AuditLineProcessor against PROBLEM.md. Score /100. Verify ThreadPool (not Task), CountdownEvent, state passing. Strengths, gaps, verdict.

---

## Model Answer Checklist

- [ ] Passed count ≈ 90% of batch (LineId % 10 == 0 fails)
- [ ] AllCallbacksUsedPoolThread is true
- [ ] No Task.Run in codebase
