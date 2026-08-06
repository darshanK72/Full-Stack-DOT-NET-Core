---
module: 01. C# Language Fundamentals
difficulty: Hard
chapters: 06 Methods, 07 Control Flow, 08 Strings, 09 Arrays, 09 Loops, 10 Exception Handling
domain: LibraryOperations
---

# Community Library Loan Tracker

Build a **.NET 8 console application from scratch**.

## Business context

A community library replaces paper sign-out sheets with a terminal app. Librarians register titles, check copies out to members, process returns, and print overdue lists (simplified fine calculation: list only, no currency in v1).

## Definitions

| Entity | Fields |
|--------|--------|
| **Book** | `Isbn` (string, normalized uppercase no spaces), `Title`, `CopiesTotal`, `CopiesAvailable` |
| **Loan** | `Isbn`, `MemberName` (trimmed), `CheckoutDate` (DateTime date), `Returned` (bool) |

**Rule:** `CheckoutDate` is always `DateTime.Today` at checkout time.

**Overdue:** active loan (`Returned == false`) where `(DateTime.Today - CheckoutDate).Days > 14`.

## Service: `LibraryService`

Implement these methods (exact signatures may vary; behavior must match):

| Method | Behavior |
|--------|----------|
| `RegisterBook(isbn, title, copies)` | Normalize ISBN; reject duplicate ISBN; set available = total |
| `Checkout(isbn, memberName)` | Fail if unknown ISBN or no copies; decrement available; add loan |
| `Return(isbn, memberName)` | Find active loan matching both; mark returned; increment available |
| `GetAvailableBooks()` | List books with CopiesAvailable > 0 |
| `GetOverdueLoans()` | Loans meeting overdue rule |

## CLI menu

```
1 Register  2 Checkout  3 Return  4 Available  5 Overdue  0 Exit
```

Print clear errors: `Unknown ISBN`, `No copies available`, `No active loan found`.

## Examples

Register `978-0-1` → stored as `97801`, 3 copies.

Checkout to `Alice` → available 2.

Return from `Alice` → available 3.

If checkout was 15 days ago (simulate by allowing test hook or manual date entry in **advanced** optional FR — base spec: use Today only; overdue demo via seed data in code comment for self-test).

## Constraints

- net8, explicit usings, in-memory lists
- Normalize ISBN: trim, remove spaces, uppercase
- Optional: custom `LibraryException` for not-found vs invalid operation
- Use `StringBuilder` when building multi-line overdue report (≥3 lines)

## Non-goals

File I/O, fine payments, multi-branch

## Evaluation

[EVALUATION.md](EVALUATION.md)
