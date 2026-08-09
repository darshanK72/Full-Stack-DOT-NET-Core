# Document Archive — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| CreateDirectory chain for dated archive path | 25 |
| Copy with overwrite false + missing source guard | 25 |
| EnumerateFiles AllDirectories + FileInfo.Length | 20 |
| ListInboxFiles returns names only | 15 |
| PurgeEmptyInbox only when empty | 10 |
| Demo proves duplicate archive rejected | 5 |

## AI Review Prompt

Evaluate DocumentArchiveService against PROBLEM.md. Score /100. Verify static vs instance API usage, no Console in service class. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] Second archive of same name returns false
- [ ] GetArchivedSize returns positive bytes after successful archive
- [ ] PurgeEmptyInbox removes empty inbox only after files deleted
