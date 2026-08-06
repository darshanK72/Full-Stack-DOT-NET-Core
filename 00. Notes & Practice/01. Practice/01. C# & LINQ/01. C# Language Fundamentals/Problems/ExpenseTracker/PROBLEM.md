---
module: 01. C# Language Fundamentals
difficulty: Medium
chapters: 02 Data Types, 03 Input and Output, 05 Type Conversion, 06 Methods, 07 Control Flow, 08 Strings, 09 Loops
domain: PersonalFinance
---

# Personal Expense Tracker CLI

Build a **.NET 8 console application from scratch**.

## Business context

Employees log daily spending by category for reimbursement audits. Finance needs monthly totals per category and a grand total line.

## Definitions

| Entity | Fields |
|--------|--------|
| **Expense** | `Id`, `Date`, `Category` (uppercase stored), `Amount` (decimal), `Note` (optional) |

## Functional requirements

1. **Add** — date `yyyy-MM-dd` or empty for today; category required; amount decimal > 0; optional note
2. **List all** — sort by date ascending, then Id
3. **Monthly summary** — prompt year and month; print each category total alphabetically; last line `TOTAL` with sum
4. **Delete by Id** — report if not found
5. Menu loop until Exit

## Summary output shape

```
2026-03 Summary
FOOD       450.00
TRAVEL     120.50
TOTAL      570.50
```

Use `:F2` or culture-invariant decimal formatting for amounts in summary (currency symbol optional in v1).

## Parsing rules

- Invalid date → reprompt or skip with message
- Invalid amount → do not add
- Category: trim, store uppercase

## Constraints

- net8, explicit usings, in-memory list
- `decimal` amounts only
- At least one static helper method for date parsing isolated from menu code

## Non-goals

Charts, CSV export, multi-user auth

## Evaluation

[EVALUATION.md](EVALUATION.md)
