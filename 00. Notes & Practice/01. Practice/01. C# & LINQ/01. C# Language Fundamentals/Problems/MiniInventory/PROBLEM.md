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
