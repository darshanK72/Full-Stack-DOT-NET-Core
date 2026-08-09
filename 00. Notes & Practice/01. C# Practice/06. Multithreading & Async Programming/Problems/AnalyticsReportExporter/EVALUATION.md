# Analytics Report Exporter — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| async/await pipeline ExportAsync | 20 |
| FetchWithRetryAsync exponential backoff | 20 |
| ExclusiveWriteGate RunExclusiveAsync | 20 |
| Parallel exports serialize at gate | 15 |
| CancellationToken through pipeline | 15 |
| No sync blocking (.Result/.Wait) | 10 |

## AI Review Prompt

Evaluate AnalyticsReportExporter against PROBLEM.md. Score /100. async patterns, retry, only-one gate, cancellation. Strengths, gaps, verdict.

---

## Model Answer Checklist

- [ ] Retry succeeds after configured failures
- [ ] Two parallel exports take longer than one due to exclusive write
- [ ] CancelAfter aborts with OperationCanceledException
