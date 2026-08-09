---
module: 01. C# Language Fundamentals
difficulty: Hard
chapters: 06 Methods, 07 Control Flow, 09 Arrays, 09 Loops, 04 Operators
domain: FoodDelivery
---

# Food Delivery Order Platform

Build a **.NET 8 console application from scratch**.

## Business context

Operations tracks orders through a lifecycle and needs revenue and performance reports per restaurant.

## Definitions

**Enum `OrderStatus`:** `PLACED`, `PREPARING`, `OUT_FOR_DELIVERY`, `DELIVERED`, `CANCELLED`

**Class `Order`:** `OrderId`, `RestaurantId`, `Amount` (decimal), `Status`, `PlacedAt` (DateTime)

**Class `OrderManager`**

- `AddOrder(order)` — append
- Orders are never deleted in v1

## Reports (implement all)

### Revenue by restaurant

`GetRevenueByRestaurant()` → dictionary `restaurantId → sum of Amount` for orders where status is **`DELIVERED` only**. Restaurants with no delivered orders **do not appear**.

### Average prep cycle (simplified)

For delivered orders only, treat cycle minutes as `(PlacedAt.Minute % 60)` placeholder metric for this exercise — **actually use**: stored field `PrepMinutes` (int) you add to Order; average per restaurant for DELIVERED orders. Include restaurants with delivered orders only.

### Top restaurant by revenue

`GetTopRestaurantId()` — highest delivered revenue; tie → **lower restaurant id**.

## Sample data script

Your `Main` should seed at least:

| Id | Rest | Amount | Status |
|----|------|--------|--------|
| 1 | 10 | 50 | DELIVERED |
| 2 | 10 | 30 | CANCELLED |
| 3 | 20 | 40 | DELIVERED |

Restaurant 10 revenue = 50 only.

## Constraints

- net8, explicit usings, decimal amounts
- Loops + dictionaries; LINQ optional not required

## Evaluation

[EVALUATION.md](EVALUATION.md)

---

## Extended Scenarios

Implement these after the core reports are working.

### EX1 — Daily order summary

Add a method that accepts a date and returns the count and total revenue of DELIVERED
orders placed on that calendar day.

This exercises filtering by both status and date properties on the same object (ch06),
summing decimal amounts (ch04), and working with DateTime date components (ch02).

### EX2 — Status update

Add a method that updates an order's status by Id.  The status must progress
forward through the lifecycle — an order cannot move from DELIVERED back to PREPARING.
Return false when the Id is unknown or the transition would be backwards.

This exercises finding an item by Id (ch06), comparing enum values with integer ordering
(ch04), and enforcing a state-machine rule with a conditional (ch04).

### EX3 — Restaurant performance report

Add a method that builds a multi-line report string (using StringBuilder) listing
each restaurant's delivered revenue, average prep time, and order count, sorted by
revenue descending.

This exercises StringBuilder for multi-line output (ch08), combining results from
multiple dictionaries (ch09), and sorting by a numeric key (ch06).

---

## Implementation Guide

### Classes and their responsibilities

| Class | Responsibility |
|-------|----------------|
| `OrderStatus` | Enum representing the delivery lifecycle |
| `Order` | Data model for one customer order |
| `OrderManager` | Stores orders and produces revenue/performance reports |

### Method contracts

| Method | What it does | Key rule |
|--------|-------------|----------|
| `AddOrder(order)` | Appends an order to the log | No validation — orders are never removed |
| `GetRevenueByRestaurant()` | Sums DELIVERED order amounts per restaurant | CANCELLED and other statuses are excluded |
| `GetAveragePrepMinutes()` | Averages PrepMinutes per restaurant for DELIVERED orders | Only restaurants with delivered orders appear |
| `GetTopRestaurantId()` | Returns the restaurant Id with the highest DELIVERED revenue | Tie → lower Id wins; -1 when no delivered orders exist |

### Business rules to enforce

- Revenue and average prep time count only DELIVERED orders — a CANCELLED order with a large amount must never inflate a restaurant's figures
- `GetTopRestaurantId` must handle the tie case explicitly; do not assume dictionary iteration order breaks ties (ch04 comparison)
- `GetAveragePrepMinutes` requires an integer cast to double before division — integer division would truncate the result (ch04)
- When computing an average, you need both a running sum and a running count — a single pass is sufficient (ch06)

### Concepts by chapter

| Chapter | Where it applies |
|---------|-----------------|
| ch02 | Enum for order status; decimal for revenue amounts; DateTime for PlacedAt |
| ch04 | Integer-to-double cast for averaging; decimal comparison for top-restaurant tie-breaking |
| ch06 | Loop over orders; per-restaurant accumulation; running-max pattern |
| ch07 | Analytics methods return typed results; AddOrder is void with no return value |
| ch09 | `Dictionary<int, decimal>` for revenue; `Dictionary<int, double>` for averages |
- Running-max pattern → ch06
