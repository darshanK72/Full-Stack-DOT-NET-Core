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

---

## Extended Scenarios

Implement these after the core features are working.

### EX1 — Member loan history

Add a menu option that accepts a member name and lists all loans (active and returned)
for that member, showing the ISBN, title, checkout date, and current status.

This exercises filtering a list by a string field (ch06, ch08) and displaying DateTime
values in a readable format (ch08).

### EX2 — Copy count report

Add a menu option that prints each registered title alongside its total copies and
how many are currently available. Flag titles where all copies are checked out.

This exercises iterating a collection and applying a conditional to produce different
output (ch06, ch04).

### EX3 — Bulk registration

Allow librarians to register multiple copies of the same title in one step by entering
a quantity greater than one as a single action, rather than calling Register once per copy.

This exercises the interaction between user input (ch03), integer parsing (ch05),
and repeated service calls inside a loop (ch06).

---

## Implementation Guide

### Classes and their responsibilities

| Class | Responsibility |
|-------|----------------|
| `LibraryException` | Custom exception signalling invalid operations; extends Exception |
| `Book` | Represents one registered title and its available copy count |
| `Loan` | Records one checkout event linking a member to a book |
| `LibraryService` | All business rules; throws LibraryException on invalid operations |
| `Program` | All Console I/O; catches LibraryException and prints its message |

### Method contracts

| Method | What it does | Throws LibraryException when |
|--------|-------------|------------------------------|
| `NormalizeIsbn(isbn)` | Returns isbn trimmed, spaces removed, uppercased | — (private helper) |
| `RegisterBook(isbn, title, copies)` | Adds a new title to the catalogue | ISBN already registered |
| `Checkout(isbn, memberName)` | Records a loan; decrements available copies | ISBN unknown or no copies left |
| `Return(isbn, memberName)` | Marks an active loan as returned; restores one copy | No matching active loan found |
| `GetAvailableBooks()` | Lists titles with at least one copy available | — |
| `GetOverdueLoans()` | Lists active loans checked out more than 14 days ago | — |
| `BuildOverdueReport()` | Builds a multi-line report string using StringBuilder | — |

### Business rules to enforce

- The ISBN stored in Book and Loan is always the normalised form — never the raw user input
- A loan is overdue when the number of whole days since CheckoutDate is strictly greater than 14; subtracting two DateTime values gives a TimeSpan, and its `.Days` property gives the whole-day count (ch02)
- BuildOverdueReport must use StringBuilder to build the string, not string concatenation in a loop (ch08)
- Catching LibraryException at the menu level is the pattern — service methods throw, Program catches and displays (ch10)

### Concepts by chapter

| Chapter | Where it applies |
|---------|-----------------|
| ch02 | `DateTime` for loan dates; `bool` for Returned field; DateTime arithmetic for overdue check |
| ch06 | Loops inside service methods to find books and loans; menu loop in Program |
| ch07 | Void methods that throw instead of returning error codes |
| ch08 | ISBN normalisation; StringBuilder for overdue report |
| ch09 | `List<Book>` and `List<Loan>` as backing stores |
| ch10 | `LibraryException` custom exception; `try/catch` at the menu handler level |
- StringBuilder → ch08
- DateTime arithmetic → ch02
- Loop to find/filter items → ch06
- `out` / void methods with throws → ch07
