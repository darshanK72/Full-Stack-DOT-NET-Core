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
