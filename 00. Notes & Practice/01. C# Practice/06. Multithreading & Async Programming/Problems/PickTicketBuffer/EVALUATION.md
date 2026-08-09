# Pick Ticket Buffer — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| ConcurrentDictionary GetOrAdd cache | 20 |
| AddOrUpdate surcharge | 15 |
| BlockingCollection bounded enqueue/dequeue | 25 |
| ConcurrentBag results from parallel pickers | 20 |
| CompleteAdding + consumer shutdown | 10 |
| Producer FeedTickets | 10 |

## AI Review Prompt

Evaluate PickTicketBuffer against PROBLEM.md. Score /100. Concurrent collections, producer/consumer, cache patterns. Strengths, gaps, verdict.

---

## Model Answer Checklist

- [ ] 20 pick results when 20 tickets fed
- [ ] Buffer capacity 5 does not lose tickets (may block producer)
- [ ] TryApplySurcharge returns false for unknown SKU
