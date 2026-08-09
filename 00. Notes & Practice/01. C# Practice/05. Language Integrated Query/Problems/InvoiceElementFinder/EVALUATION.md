# Invoice Element Finder — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| FirstOverdue uses sort + FirstOrDefault | 20 |
| Single throws on 0 or 2+ pending | 20 |
| GetPage OrderBy + Skip + Take + ToList | 20 |
| Chunk batching | 15 |
| DefaultIfEmpty before Average | 15 |
| ElementAtOrDefault | 10 |

## AI Review Prompt

Evaluate InvoiceElementFinder against PROBLEM.md. Score /100. Verify First vs Last, Single semantics, paging order, DefaultIfEmpty+Average. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Last on unsorted data not used for "latest overdue"
- [ ] Single throws InvalidOperationException on duplicate pending
- [ ] Page 2 stable because OrderBy Customer applied first
- [ ] Empty overdue average returns 0 via DefaultIfEmpty
