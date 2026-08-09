# Legacy Rule Migrator — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| `OrderRule` + `OrderNotifier` delegate types | 10 |
| `BuildMaxLineItemsRule` anonymous method (not lambda) | 25 |
| `BuildMaxLineItemsRuleLambda` equivalent behavior | 15 |
| Void anonymous method notifier | 15 |
| `EvaluateAll` short-circuit false | 15 |
| `NotifyAll` invokes all notifiers | 10 |
| `RulesMatch` comparison | 10 |

## AI Review Prompt

Evaluate LegacyRuleMigrator against PROBLEM.md. Score /100. Confirm anonymous method syntax in legacy factory, lambda parity, void notifier, and runner semantics. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Anonymous rule uses `delegate (Order order) { ... }` form
- [ ] Lambda and anonymous rules agree on pass/fail for same orders
- [ ] 6-line order fails max-5 rule
- [ ] Two notifiers both print on NotifyAll
- [ ] EvaluateAll stops at first failing rule
