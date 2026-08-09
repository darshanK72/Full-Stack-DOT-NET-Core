# Order Fulfillment Joiner — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Inner Join drops customers without orders | 20 |
| GroupJoin + DefaultIfEmpty left outer | 25 |
| Null-safe nullable OrderId / TrackingNumber | 15 |
| Query syntax inner join equivalent | 15 |
| CountCustomersWithoutOrders correct | 15 |
| Shipment left outer for missing tracking | 10 |

## AI Review Prompt

Evaluate OrderFulfillmentJoiner against PROBLEM.md. Score /100. Verify Join vs GroupJoin, DefaultIfEmpty, query equals keyword. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Inner join row count = matched orders only
- [ ] Left outer includes customer with zero orders
- [ ] No NullReferenceException on missing shipment
- [ ] Query syntax uses equals not ==
