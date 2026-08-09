---
module: 05. Language Integrated Query
difficulty: Medium
chapters: 06 Element Operations, 11 Partitioning Operations
domain: AccountsReceivable
---

# Invoice Element Finder

Build a **.NET 8 console application from scratch** that picks single invoices from streams, pages overdue lists, and batches export chunks.

## Business context

Accounts receivable clerks need the oldest overdue invoice, enforce exactly-one pending approval rule, page through sorted ledgers, and export fixed-size batches without loading unbounded lists twice.

## Definitions

**Record `Invoice`**

- `InvoiceId` (int)
- `Customer` (string)
- `DueDate` (DateTime)
- `Amount` (decimal)
- `Status` (string) — `"Pending"`, `"Approved"`, `"Paid"`

**Class `InvoiceElementFinder`**

- Constructor accepts `IEnumerable<Invoice> invoices`
- `Invoice? FirstOverdue(DateTime asOf)` — **`OrderByDescending` DueDate** then **`FirstOrDefault`** where DueDate < asOf and Status != `"Paid"` (do not use Last on unsorted data)
- `Invoice SinglePendingOrThrow()` — **`Single`** where Status == `"Pending"` — throws when zero or 2+
- `Invoice? ElementAtSafe(int index)` — **`ElementAtOrDefault`**
- `IReadOnlyList<Invoice> GetPage(int pageNumber, int pageSize)` — **`OrderBy` Customer** then **`Skip((pageNumber-1)*pageSize).Take(pageSize).ToList()`** — pageNumber is 1-based
- `IEnumerable<Invoice[]> ExportChunks(int chunkSize)` — **`Chunk(chunkSize)`** on invoices ordered by InvoiceId
- `decimal AverageOverdueAmount(DateTime asOf)` — filter overdue unpaid; **`DefaultIfEmpty(0m)`** before **`Average`** on Amount to avoid empty throw

## Demo Main

1. Print first overdue invoice id for a fixed `asOf`.
2. Try `SinglePendingOrThrow` on seed with exactly one pending — print id.
3. Demonstrate second call path with two pendings throws (catch and print message).
4. Print page 2 of size 3 after sort by customer.
5. Print chunk count and first id per chunk for chunk size 2.
6. Print average overdue with and without overdue rows (DefaultIfEmpty path).

## Constraints

- net8, element + partitioning operators
- Paging requires OrderBy before Skip/Take

## Non-goals

Database OFFSET/FETCH

## Evaluation

[EVALUATION.md](EVALUATION.md)
