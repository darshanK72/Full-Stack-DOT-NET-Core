# Payroll Processor — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Abstract Employee + validation | 15 |
| Salaried proration by PayPeriod | 20 |
| Hourly gross calculation | 10 |
| Polymorphic total + GetGrossByType | 25 |
| AddEmployee overloads + duplicate id guard | 15 |
| ToString/GetSummary overrides | 10 |
| Constraints (decimal, virtual/override) | 5 |

## AI Review Prompt

Evaluate PayrollProcessor against PROBLEM.md. Score /100, per-method feedback, polymorphism correctness, rounding bugs, strengths, verdict.

---

## Model Answer Checklist

- [ ] Duplicate EmployeeId rejected
- [ ] Salaried WEEKLY/BIWEEKLY/MONTHLY divisors correct
- [ ] Hourly rejects negative hours / zero rate
- [ ] GetTotalGrossPay uses base references
- [ ] GetGrossByType includes both keys with 0 defaults
- [ ] Two AddEmployee overloads share core logic
