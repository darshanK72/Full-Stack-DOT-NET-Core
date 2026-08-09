---
module: 01. C# Language Fundamentals
difficulty: Medium
chapters: 02 Data Types, 03 Input and Output, 06 Methods, 07 Control Flow, 08 Strings, 09 Loops, 09 Arrays
domain: RetailInventory
---

# Mini Inventory Management System

Build a complete **.NET 8 console application from scratch**. No starter code is provided.

## Business context

A regional retail warehouse needs a terminal tool for floor staff to manage SKU stock without Excel. The first version is in-memory only for a single shift; data resets when the app closes.

## Definitions

| Entity | Properties |
|--------|------------|
| **Product** | `Id` (int, auto 1..n), `Name` (string), `Price` (decimal), `Stock` (int) |

## Functional requirements

### FR1 — Main menu loop

Display options until Exit:

```
1) Add product   2) List   3) Sell   4) Restock   5) Search by name   0) Exit
```

Invalid choice prints a friendly message and redisplays the menu.

### FR2 — Add product

Prompt: name (required, trimmed), price (decimal, ≥ 0), stock (int, ≥ 0). Assign next Id. Confirm with `Added product #{Id}`.

### FR3 — List products

Table: Id, Name, Price (currency), Stock. If empty: `No products registered.`

### FR4 — Sell

Prompt Id and quantity (int > 0). If Id unknown → error. If stock insufficient → error with current stock. Else decrement and print remaining stock.

### FR5 — Restock

Prompt Id and quantity (int > 0). Increase stock or report errors as in FR4.

### FR6 — Search

Prompt partial name (case-insensitive). List all matches containing the substring. None → `No matches.`

## Sample transcript

```
> 1
Name: Widget
Price: 19.99
Stock: 50
Added product #1

> 3
Id: 1
Qty: 5
Sold. Stock now: 45

> 2
1  Widget  $19.99  45
```

## Technical constraints

- `net8.0` console, **ImplicitUsings disabled**, explicit usings only
- `decimal` for all money — never `double`
- In-memory collection (`List<Product>` or tracked array)
- Separate **UI** (`Program`) from **InventoryService** (business logic)
- Validate all user input; never crash on bad menu input

## Non-goals

Database, file persistence, LINQ requirement, unit test project, web API

## Suggested layout (same folder as this file)

```
Problems/MiniInventory/
├── PROBLEM.md
├── EVALUATION.md
├── MiniInventory.csproj      ← you create
├── Models/Product.cs
├── Services/InventoryService.cs
└── Program.cs
```

## Evaluation

Use [EVALUATION.md](EVALUATION.md) with your source in Cursor for scored review.

---

## Extended Scenarios

The following requirements extend the base problem with additional real-world situations.
Implement them after the core features are working.

### EX1 — Low stock alert

After any successful sale, check whether the product's remaining stock has fallen
to or below a configurable threshold (default: 5 units). When it has, print a
low-stock warning immediately after the sale confirmation.

This adds a conditional check (ch04, ch06) executed on the result of a successful operation.

### EX2 — Shift revenue total

Track the total revenue earned from all successful sales during the session.
Add a new menu option that displays the cumulative revenue formatted as a currency value.

This exercises decimal accumulation across multiple operations (ch02) and an
additional method on the service class (ch07).

### EX3 — Price adjustment

Add a menu option that allows staff to update the unit price of an existing product by Id.
The new price must be zero or positive. Print the previous and new price to confirm the change.

This exercises locating an item in a collection by Id (ch06) and parsing and
validating a decimal value from user input (ch05).

---

## Implementation Guide

### Classes and their responsibilities

| Class | Responsibility |
|-------|----------------|
| `Product` | Data model — holds the four fields for one SKU |
| `InventoryService` | All business rules; no screen output ever |
| `Program` | All Console I/O; delegates every decision to the service |

### Method contracts

| Method | What it does | Key validation |
|--------|-------------|----------------|
| `AddProduct(name, price, stock)` | Registers a new SKU; returns its auto-assigned Id | Name not empty; price ≥ 0; stock ≥ 0 |
| `GetAllProducts()` | Returns every product in the catalogue | Empty list when nothing has been added |
| `Sell(id, qty, out err)` | Reduces stock by qty; true on success | Id must exist; qty positive; stock must cover qty |
| `Restock(id, qty, out err)` | Increases stock by qty; true on success | Id must exist; qty positive |
| `Search(partial)` | Returns products whose Name contains the substring, ignoring case | Empty list when nothing matches |

### Business rules to enforce

- Product Ids are assigned sequentially by the service — the caller never supplies them
- Price must use `decimal`, never `double`, because it represents a monetary value (ch02)
- Sell and Restock communicate failure through an `out string` parameter so the caller can format the message (ch07)
- Search must match regardless of capitalisation — use a case-insensitive string comparison (ch08)

### Concepts by chapter

| Chapter | Where it applies |
|---------|-----------------|
| ch02 | `decimal` for price; `int` for Id and stock counts |
| ch05 | `decimal.TryParse` for price input; `int.TryParse` for quantities |
| ch06 | Menu loop; `switch` dispatch; loop inside service to find product by Id |
| ch07 | `out` parameter on Sell and Restock; static handler methods in Program |
| ch08 | `:C` currency format specifier in HandleList; case-insensitive match in Search |
| ch09 | `List<Product>` as the in-memory backing store |
