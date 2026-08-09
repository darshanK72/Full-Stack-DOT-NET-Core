# Callback Orchestrator — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Custom ValidationRule + ValidationResult | 10 |
| Three domain rules + short-circuit RunValidation | 20 |
| Audit trail append on pass/fail | 10 |
| RunStructuralChecks all Predicate pass | 15 |
| ComputeFee Func + null-safe Action | 15 |
| PublishAudit null-safe Action | 10 |
| Method group + lambda ValidationRule demo | 10 |
| Null-safe onReport in orchestrator | 10 |

## AI Review Prompt

Evaluate CallbackOrchestrator against PROBLEM.md. Score /100. Check custom delegate vs Func/Action/Predicate split, pipeline short-circuit, audit trail, and null-safe callbacks. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Empty OrderId fails with code ID before amount check
- [ ] Valid order returns OK and PASS audit entry
- [ ] Structural fail when LineCount is 0
- [ ] ComputeFee with null onComputed does not throw
- [ ] Fee equals 3% of TotalAmount in demo
- [ ] PublishAudit(null) is safe no-op
