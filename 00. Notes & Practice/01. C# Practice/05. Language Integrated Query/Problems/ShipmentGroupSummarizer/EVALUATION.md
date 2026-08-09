# Shipment Group Summarizer — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| GroupBy with result selector summaries | 25 |
| Query syntax group into equivalent | 15 |
| ToLookup indexer empty for missing assignee | 20 |
| ToDictionary from grouped weights | 15 |
| CriticalInCarrier nested filter | 15 |
| Deferred vs immediate demo understanding | 10 |

## AI Review Prompt

Evaluate ShipmentGroupSummarizer against PROBLEM.md. Score /100. Verify GroupBy overloads, ToLookup vs GroupBy, ToDictionary. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] IGrouping.Key used correctly
- [ ] ToLookup["Unknown"] returns empty, not null reference to sequence
- [ ] ToDictionary built from unique carrier keys after aggregation
- [ ] Query syntax summaries match method syntax counts
