# Clinic Billing Registry — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Static ProcedureCatalog + static ctor | 20 |
| VisitInvoice private ctor + Create factory | 15 |
| Indexer + MaxLinesPerVisit guard | 20 |
| TryAddLine validation | 20 |
| BillingLine copy constructor | 10 |
| Expression-bodied FormattedAmount | 5 |
| GetTotal | 10 |

## AI Review Prompt

Evaluate ClinicBillingRegistry against PROBLEM.md. Score /100, static/indexer/constructor patterns, strengths, verdict.

---

## Model Answer Checklist

- [ ] Cannot new VisitInvoice() publicly
- [ ] Indexer throws on bad index
- [ ] Line 21 rejected
- [ ] Copy constructor independent amounts
