# Log Retention Janitor — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| EnumerateFiles AllDirectories delete by age | 30 |
| FindNewestLog by LastWriteTimeUtc | 20 |
| TailReader single-pass last N | 25 |
| TailNewestLog opens StreamReader with using | 15 |
| Demo deletes stale file only | 10 |

## AI Review Prompt

Evaluate LogRetentionJanitor against PROBLEM.md. Score /100. Enumeration-based delete, stream tail without ReadAllText. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] DeleteOlderThan returns 1 when one stale log present
- [ ] TailNewestLog returns most recent lines in order
- [ ] Newest log path matches file with latest timestamp
