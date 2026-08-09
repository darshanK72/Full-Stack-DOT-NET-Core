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

---

## Extended Scenarios

Implement these after the core features are working.

### EX1 — Spending cap warning

Allow the user to set a monthly spending cap per category at startup or through
a menu option. After each expense is added, check whether the running total for that
category in the current month has reached or exceeded its cap and warn the user immediately.

This exercises decimal accumulation (ch02), conditional comparison (ch04), and
calling the summary method internally after each successful add (ch07).

### EX2 — Export summary as plain text

Add a menu option that writes the monthly summary for a chosen period to the console
using a StringBuilder, formatted so the output can be copied and pasted into a report.

This exercises StringBuilder (ch08) combined with the existing summary dictionary (ch09).

### EX3 — Filter by category

Add a menu option that lists all expenses matching a given category, sorted by date.
The filter should be case-insensitive so the user can type "food" and match "FOOD".

This exercises case-insensitive string comparison (ch08) and filtering a list (ch06).

---

## Implementation Guide

### Classes and their responsibilities

| Class | Responsibility |
|-------|----------------|
| `Expense` | Data model for one spending event |
| `ExpenseService` | Business rules, storage, and summary computation; no screen output |
| `Program` | All Console I/O; includes the required static date-parsing helper |

### Method contracts

| Method | What it does | Key validation |
|--------|-------------|----------------|
| `AddExpense(date, category, amount, note)` | Stores a new expense; returns its auto-assigned Id | Amount must be positive |
| `GetAllExpenses()` | Returns all expenses sorted by date then Id | Returns empty list when nothing added |
| `DeleteById(id)` | Removes the expense with that Id | Returns false when Id not found |
| `GetMonthlySummary(year, month)` | Returns category totals for the period | Empty dictionary when no matching expenses |

### Business rules to enforce

- Category is always stored in uppercase — "food", "Food", and "FOOD" must all group together in the summary (ch08)
- `GetAllExpenses` must sort by date ascending, with Id as the tiebreaker — do this without LINQ (ch06)
- `GetMonthlySummary` returns only the dictionary; the caller in Program is responsible for sorting the keys alphabetically and computing the TOTAL line (ch07 separation of concerns)
- `TryParseDate` must be a static method on Program, not on ExpenseService, because parsing raw user input is a UI boundary concern (ch07)

### Required static helper

`TryParseDate(string input, out DateTime date)` must handle two cases:
- Empty or whitespace input means the user wants today's date — set date to today and return true
- Any other input must be validated against the exact format `yyyy-MM-dd` using `DateTime.TryParseExact` with `CultureInfo.InvariantCulture` (ch05)

### Concepts by chapter

| Chapter | Where it applies |
|---------|-----------------|
| ch02 | `decimal` for amounts; `DateTime` for expense dates |
| ch05 | `DateTime.TryParseExact` in TryParseDate; `decimal.TryParse` for amount input |
| ch06 | Manual sort by two keys; loop to filter by year and month; menu loop |
| ch07 | Static helper method for date parsing; service/Program separation |
| ch08 | `.ToUpperInvariant()` for category normalisation; format specifiers `:F2` and `:yyyy-MM-dd` |
| ch09 | `List<Expense>` for storage; `Dictionary<string, decimal>` for the monthly summary |
- `Dictionary<string, decimal>` → ch09
