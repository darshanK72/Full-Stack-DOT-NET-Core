# Customer Import Validator — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Static compiled Regex fields | 10 |
| Email validation (incl. empty fail) | 15 |
| Product code validation | 15 |
| Named group order id extraction | 20 |
| Phone redaction Replace | 15 |
| ValidateBatch issues per field | 15 |
| Demo covers invalid + extract + redact | 10 |

## AI Review Prompt

Evaluate CustomerImportValidator against PROBLEM.md. Score /100. Verify Regex reuse, named groups, Replace redaction, and batch validation. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] `bob@example` fails email
- [ ] `BAD-CODE` fails product code
- [ ] "order 42" yields id 42
- [ ] `555-111-2222` becomes `[REDACTED]`
- [ ] Empty email produces Email issue
