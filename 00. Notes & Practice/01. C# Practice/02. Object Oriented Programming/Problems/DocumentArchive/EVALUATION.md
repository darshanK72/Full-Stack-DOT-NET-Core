# Document Archive — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Abstract Document + previews | 20 |
| PdfDocument + TextMemo rules | 15 |
| IExportable / ISearchable | 20 |
| Explicit IIndexedItem.Title | 15 |
| ArchiveService add/search/export | 20 |
| WordCount virtual | 10 |

## AI Review Prompt

Evaluate DocumentArchive against PROBLEM.md. Score /100, interface/abstract design, explicit implementation, strengths, verdict.

---

## Model Answer Checklist

- [ ] Cannot instantiate Document
- [ ] Preview truncation with ellipsis
- [ ] ExportAll only exportable docs
- [ ] Explicit storage key differs from public Title
- [ ] Search case-insensitive
