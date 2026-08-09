# Document Archive Pipeline — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| NRT on DocumentMetadata (Notes nullable) | 10 |
| Switch expression folder mapping | 20 |
| Range-based extension slice | 15 |
| BuildSummaryLine null-safe Notes | 15 |
| using declaration on StreamWriter | 15 |
| ??= lazy cache init | 15 |
| PageSpan readonly struct (declared) | 5 |
| Demo write/read summaries | 5 |

## AI Review Prompt

Evaluate DocumentArchivePipeline against PROBLEM.md. Score /100. Verify C# 8: switch expressions, ranges, using declarations, ??=, nullable annotations. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Text kind → text/ folder
- [ ] `report.pdf` extension is pdf
- [ ] Null Notes omitted from summary line
- [ ] WriteSummaries creates file without explicit Dispose block
- [ ] Second ResolveFolderCached returns cached folder
