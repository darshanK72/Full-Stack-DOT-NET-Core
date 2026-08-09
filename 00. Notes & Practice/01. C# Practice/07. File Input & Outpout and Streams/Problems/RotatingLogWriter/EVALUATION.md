# Rotating Log Writer — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Append StreamWriter UTF-8 no BOM | 25 |
| WriteEntry format + Flush per line | 20 |
| CountNonEmptyLines via TextReader | 20 |
| ReadLastLines single-pass tail | 25 |
| Demo uses StringReader polymorphism | 10 |

## AI Review Prompt

Evaluate ApplicationLogWriter and LogAnalytics against PROBLEM.md. Score /100. Check using blocks, encoding, TextReader abstraction. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] Append mode preserves prior lines
- [ ] ReadLastLines(2) returns two most recent full lines
- [ ] StringReader demo counts without temp file
