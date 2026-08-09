---
module: 08. Advanced C# Features
difficulty: Medium
chapters: 03 Regular Expressions
domain: CustomerSupport
---

# Customer Import Validator

Build a **.NET 8 console application from scratch** that validates customer import rows with compiled `Regex` patterns, extracts order references from free text, and redacts phone numbers in notes.

## Business context

Support exports customer rows from a legacy CRM. Before loading into the new system, operations must flag invalid emails and product codes, pull order ids embedded in notes, and produce redacted notes safe for ticket display.

## Definitions

**Record `ImportRow`**

- `RowId` (string)
- `Email` (string)
- `Phone` (string)
- `ProductCode` (string)
- `Notes` (string)

**Record `ValidationIssue`**

- `RowId`, `Field` (string), `Message` (string)

**Class `ImportValidationEngine`**

- Store patterns as `private static readonly Regex` with `RegexOptions.Compiled | RegexOptions.CultureInvariant`:
  - Email: `^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$`
  - Product code: `^[A-Z]{3}-\d{4}$`
  - Order id in text (named group `id`): `(?i)order\s+(?<id>\d+)`
  - Phone redact: `\d{3}-\d{3}-\d{4}`
- `bool IsValidEmail(string email)` — empty fails
- `bool IsValidProductCode(string code)` — empty fails
- `IReadOnlyList<int> ExtractOrderIds(string notes)` — all matches of named group `id`, parsed to int, in discovery order
- `string RedactPhones(string notes)` — `Replace` phone pattern with `[REDACTED]`
- `IReadOnlyList<ValidationIssue> ValidateBatch(IReadOnlyList<ImportRow> rows)` — one issue per failed field per row (Email and/or ProductCode)

## Demo Main

1. Seed batch of at least 5 rows mixing valid/invalid email and product codes (include notes like "Refund order 42 today").
2. Print validation issues grouped by row.
3. For first row with order mention, print extracted order ids.
4. Print original vs redacted notes for a row containing `555-111-2222`.

## Constraints

- net8, explicit usings, reuse static Regex fields (no parse-per-call static helpers in hot paths)
- Check `match.Success` before reading groups
- `MatchTimeout` not required for demo

## Non-goals

Full RFC 5322 email validation, ReDoS stress tests

## Evaluation

[EVALUATION.md](EVALUATION.md)
