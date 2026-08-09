# Mini Inventory — Evaluation

## Rubric (100 points)

| Criterion | Points | Look for |
|-----------|--------|----------|
| Correctness | 40 | All FR1–FR6; edge cases |
| Structure | 20 | Product model; service vs Program; naming |
| Input validation | 15 | Bad ids, qty, prices handled |
| Clarity | 15 | Small methods; readable menu loop |
| Constraints | 10 | net8, explicit usings, decimal, in-memory |

## AI Review Prompt

Evaluate my Mini Inventory project against PROBLEM.md and this rubric. Return: score /100 with breakdown, strengths, issues, prioritized improvements, verdict (Ready / Needs revision / Incomplete). Cite my files and methods.

---

## Model Answer Checklist

- [ ] Product model Id/Name/Price/Stock
- [ ] Auto-increment Id
- [ ] List/search/sell/restock behave per spec
- [ ] Case-insensitive search
- [ ] decimal prices; no double money math
- [ ] Service class separates logic from Console prompts
