# Library Loan Tracker — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Correctness (register/checkout/return/overdue) | 40 |
| Structure (models, service, Program) | 20 |
| ISBN normalization & string handling | 15 |
| Error messages / optional custom exception | 15 |
| Constraints (in-memory, explicit usings) | 10 |

## AI Review Prompt

Review my Library Loan Tracker against PROBLEM.md. Score /100, strengths, bugs, improvements, verdict.

---

## Model Answer Checklist

- [ ] ISBN normalized on register/lookup
- [ ] Copy count integrity on checkout/return
- [ ] Overdue filter 14 days, not returned
- [ ] StringBuilder or clear formatting for lists
