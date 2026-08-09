# File Signature Scanner — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| FileStream read exact signature length | 30 |
| DetectType PNG/PDF order | 25 |
| ScanFolder EnumerateFiles AllDirectories | 20 |
| Short file returns false / null safely | 15 |
| Demo writes binary headers correctly | 10 |

## AI Review Prompt

Evaluate FileSignatureScanner against PROBLEM.md. Score /100. Byte comparison at file start only, enumeration-based scan. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] PNG and PDF test files detected
- [ ] Text file DetectType returns null
- [ ] ScanFolder lists two matches
