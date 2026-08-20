# FoodVersion2 — Solution & Concepts Guide

Interview problem: build and extend a food delivery platform order-management backend in C#.

| File | Purpose |
|------|---------|
| `FoodVersion2.cs` (parent folder) | Original problem statement + tests (Tasks 2–3 stubs) |
| `FoodVersion2.cs` (this folder) | Runnable project copy — implement tasks here |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix: `GetOrderStatistics`](#task-1--bug-fix-getorderstatistics)
5. [Task 2.1 — `AddDelivery`](#task-21--adddelivery)
6. [Task 2.2 — `GetAverageDeliveryTimeByRestaurant`](#task-22--getaveragedeliverytimebyrestaurant)
7. [Task 3 — `GetTopRestaurantsByRevenue`](#task-3--gettoprestaurantsbyrevenue)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`OrderManager` is the central service that:

- Stores **orders** placed at restaurants by customers
- Tracks **order status** through the delivery lifecycle
- Associates **deliveries** (time windows) with orders
- Computes **order statistics**, **average delivery times**, and **revenue rankings**

Each task builds on the previous one — from a simple status-count bug to cross-order aggregation and top-k ranking.

```
┌─────────────────┐     ┌──────────────────┐
│  AddOrder()     │────▶│  List<Order>     │
│  UpdateStatus() │     │  (each has       │
└─────────────────┘     │   deliverys[])   │
                        └──────────────────┘
                               │
┌─────────────────┐            │
│  AddDelivery()  │────────────┘
└─────────────────┘
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–3)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`OrderStatus`)

Orders move through a fixed lifecycle:

```csharp
enum OrderStatus
{
    PLACED,
    PREPARING,
    OUT_FOR_DELIVERY,
    DELIVERED,
    CANCELED
}
```

**Active** orders: `PLACED`, `PREPARING`, `OUT_FOR_DELIVERY`  
**Closed** orders: `DELIVERED` **and** `CANCELED` (both are terminal states)

This distinction is the root of the Task 1 bug — counting only `DELIVERED` as closed misses canceled orders.

---

### 2. Nested Collections (Orders → Deliveries)

Each `Order` owns a `List<Delivery> deliverys`. Deliveries are **not** stored globally — you reach them through their parent order.

| Operation | Where data lives |
|-----------|------------------|
| `AddDelivery(orderId, delivery)` | Append to matching order's `deliverys` list |
| Average time by restaurant | Flatten all deliveries across orders, group by `restaurantId` |

**Key insight:** `GetAverageDeliveryTimeByRestaurant` must aggregate deliveries from **all orders** for a restaurant, not just one order.

---

### 3. Duration Calculation

```csharp
public int GetDurationMinutes()
{
    return endMinute - startMinute;
}
```

Times are integer minutes (not `DateTime`). Average delivery time = mean of all delivery durations for that restaurant.

---

### 4. Revenue vs Status Filtering

| Metric | Which orders count |
|--------|-------------------|
| `closedOrders` (Task 1) | `DELIVERED` + `CANCELED` |
| Revenue (Task 3) | **Only** `DELIVERED` |
| Active count (Task 1) | `PLACED`, `PREPARING`, `OUT_FOR_DELIVERY` |

Canceled orders are **closed** but contribute **zero revenue** — do not conflate the two filters.

---

### 5. Tie-Breaking & Edge Cases (Task 3)

| Condition | Expected behavior |
|-----------|-------------------|
| Equal revenue | Lower `restaurantId` first |
| Zero delivered revenue | Restaurant **excluded** from results |
| `n <= 0` | Return empty list |
| `n` > qualifying restaurants | Return all qualifying restaurants |
| Empty manager | Return empty list |

---

## Data Structures Used

### Domain Classes

```
Delivery
├── deliveryId    : int
├── startMinute   : int
├── endMinute     : int
└── GetDurationMinutes() : int

Order
├── orderId       : int
├── restaurantId  : int
├── customerId    : int
├── orderValue    : double
├── distanceKm    : double
├── status        : OrderStatus
└── deliverys     : List<Delivery>

OrderStats (Task 1 output)
├── totalOrders   : int
├── activeOrders  : int
└── closedOrders  : int

OrderManager
└── orders : List<Order>
```

---

## Task 1 — Bug Fix: `GetOrderStatistics`

### Requirement

Return global order statistics:

- `totalOrders` — count of **all** orders
- `activeOrders` — count where status is `PLACED`, `PREPARING`, or `OUT_FOR_DELIVERY`
- `closedOrders` — count where status is `DELIVERED` **or** `CANCELED`

### The Bug

```csharp
foreach (Order o in orders)
{
    if (o.status == OrderStatus.DELIVERED)
    {
        closed++;
    }
}
```

This counts only `DELIVERED` orders as closed. **Canceled orders are also closed** — they will never become active again.

### Fix

```csharp
foreach (Order o in orders)
{
    if (o.status == OrderStatus.DELIVERED || o.status == OrderStatus.CANCELED)
    {
        closed++;
    }
}
```

**LINQ equivalent:**

```csharp
int closed = orders.Count(o =>
    o.status == OrderStatus.DELIVERED || o.status == OrderStatus.CANCELED);
```

### Test Data Breakdown

| orderId | Status | Counts toward |
|---------|--------|---------------|
| 1 | PLACED | total, active |
| 2 | PREPARING | total, active |
| 3 | OUT_FOR_DELIVERY | total, active |
| 4 | DELIVERED | total, closed |
| 5 | CANCELED | total, closed |

**Expected:** `total=5`, `active=3`, `closed=2`

---

## Task 2.1 — `AddDelivery`

### Requirement

Associate a `Delivery` with an order by `orderId`. If the order does not exist, **silently ignore** (no exception, no side effects).

### Solution

```csharp
public void AddDelivery(int orderId, Delivery delivery)
{
    foreach (Order o in orders)
    {
        if (o.orderId == orderId)
        {
            o.deliverys.Add(delivery);
            return;
        }
    }
}
```

**LINQ equivalent:**

```csharp
Order order = orders.FirstOrDefault(o => o.orderId == orderId);
if (order != null)
    order.deliverys.Add(delivery);
```

### Why the guard matters

`TestAddDeliveryIgnoresUnknown` adds a delivery for order `999` which does not exist. Without the guard, you'd throw or attach to the wrong entity. The method must be a **no-op** for unknown IDs.

---

## Task 2.2 — `GetAverageDeliveryTimeByRestaurant`

### Requirement

Return `Dictionary<int, double>` mapping each `restaurantId` to the **average delivery duration in minutes**.

**Rules:**
- Count **all deliveries** for that restaurant (across all orders)
- Only restaurants with at least one delivery appear in the map
- Empty manager → empty map

### Solution

```csharp
public Dictionary<int, double> GetAverageDeliveryTimeByRestaurant()
{
    return orders
        .SelectMany(o => o.deliverys.Select(d => new { o.restaurantId, d }))
        .GroupBy(x => x.restaurantId)
        .ToDictionary(
            g => g.Key,
            g => g.Average(x => (double)x.d.GetDurationMinutes()));
}
```

### Step-by-Step (Test Data)

```
Order 1 (restaurant 10): delivery 101 → 10–40 = 30 min
Order 2 (restaurant 10): delivery 102 → 50–80 = 30 min
                         delivery 103 → 90–150 = 60 min
Order 3 (restaurant 11): delivery 104 → 20–50 = 30 min
Delivery for order 999: ignored (order doesn't exist)
```

| restaurantId | Durations | Average |
|--------------|-----------|---------|
| 10 | [30, 30, 60] | 40.0 |
| 11 | [30] | 30.0 |

### Imperative Alternative

```csharp
var sums = new Dictionary<int, int>();
var counts = new Dictionary<int, int>();

foreach (Order o in orders)
{
    foreach (Delivery d in o.deliverys)
    {
        if (!sums.ContainsKey(o.restaurantId))
        {
            sums[o.restaurantId] = 0;
            counts[o.restaurantId] = 0;
        }
        sums[o.restaurantId] += d.GetDurationMinutes();
        counts[o.restaurantId]++;
    }
}

var result = new Dictionary<int, double>();
foreach (int restaurantId in sums.Keys)
    result[restaurantId] = (double)sums[restaurantId] / counts[restaurantId];

return result;
```

---

## Task 3 — `GetTopRestaurantsByRevenue`

### Requirement

Return the `restaurantId`s of the top `n` restaurants by **total delivered revenue**, ordered highest to lowest.

**Rules:**
- Revenue counts **only** `DELIVERED` orders (`orderValue` summed per restaurant)
- Restaurants with zero delivered revenue must **not** appear
- Tie on revenue → lower `restaurantId` first
- `n <= 0` → empty list
- `n` larger than qualifying count → return all qualifying restaurants

### Solution

```csharp
public List<int> GetTopRestaurantsByRevenue(int n)
{
    if (n <= 0)
        return new List<int>();

    return orders
        .Where(o => o.status == OrderStatus.DELIVERED)
        .GroupBy(o => o.restaurantId)
        .Select(g => new { RestaurantId = g.Key, Revenue = g.Sum(o => o.orderValue) })
        .OrderByDescending(x => x.Revenue)
        .ThenBy(x => x.RestaurantId)
        .Take(n)
        .Select(x => x.RestaurantId)
        .ToList();
}
```

### Step-by-Step (Test Data)

| restaurantId | Delivered orders | Revenue | Notes |
|--------------|------------------|---------|-------|
| 10 | 25 + 55 | 80 | Ties with 12 |
| 11 | 40 | 40 | CANCELED 100 ignored |
| 12 | 80 | 80 | Ties with 10 |
| 13 | (none) | 0 | PLACED only — excluded |

**Ranking:** 10 (80, id wins tie) → 12 (80) → 11 (40). Restaurant 13 never appears.

```
GetTopRestaurantsByRevenue(5) → [10, 12, 11]
GetTopRestaurantsByRevenue(2) → [10, 12]
GetTopRestaurantsByRevenue(1) → [10]
GetTopRestaurantsByRevenue(0) → []
```

### Why `GroupBy` naturally excludes zero revenue

Restaurants with no `DELIVERED` orders never enter the grouped sequence, so they cannot appear in the result — no extra filter needed.

---

## LINQ Cheat Sheet

| Goal | Pattern |
|------|---------|
| Filter by status | `.Where(o => o.status == OrderStatus.DELIVERED)` |
| Flatten nested list | `.SelectMany(o => o.deliverys.Select(d => ...))` |
| Group + average | `.GroupBy(...).ToDictionary(g => g.Key, g => g.Average(...))` |
| Sum per group | `.GroupBy(...).Select(g => g.Sum(o => o.orderValue))` |
| Top-k with tie-break | `.OrderByDescending(...).ThenBy(...).Take(n)` |
| Guard empty / invalid n | `if (n <= 0) return new List<int>();` at method start |

---

## Interview Tips

### Reading the Problem

1. **Underline status filters** — "closed" ≠ "delivered only"; "revenue" ≠ "all orders"
2. **Note where data is nested** — deliveries live on orders, not in a global list
3. **Check tie-break direction** — Task 3 uses ascending `restaurantId` on revenue ties

### Debugging Task 1

When a count is wrong, ask: *"Am I using the correct definition of closed/active?"* The bug was treating `closed` as synonymous with `DELIVERED`.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Counting only `DELIVERED` as closed | Include `CANCELED` in closed count |
| Throwing on unknown order in `AddDelivery` | Silently return — no-op |
| Averaging per order instead of per restaurant | Flatten all deliveries, group by `restaurantId` |
| Including CANCELED orders in revenue | Filter `status == DELIVERED` only |
| Including PLACED orders with high value in ranking | They have zero delivered revenue — excluded |
| Wrong tie-break on revenue | `.ThenBy(x => x.RestaurantId)` — lower ID wins |
| Forgetting `n <= 0` guard | Return empty list immediately |

### Complexity (for follow-up questions)

For `n` orders with `d` total deliveries and `r` restaurants:

| Method | Time | Space |
|--------|------|-------|
| Task 1 | O(n) | O(1) |
| AddDelivery | O(n) | O(1) |
| GetAverageDeliveryTimeByRestaurant | O(d) | O(r) |
| GetTopRestaurantsByRevenue | O(n log n) | O(r) |

All are fine for typical interview data sizes.

---

## Quick Reference — All Solutions

```csharp
// TASK 1 — fix closed count
if (o.status == OrderStatus.DELIVERED || o.status == OrderStatus.CANCELED)
    closed++;

// TASK 2.1 — add delivery to order (ignore unknown)
foreach (Order o in orders)
{
    if (o.orderId == orderId)
    {
        o.deliverys.Add(delivery);
        return;
    }
}

// TASK 2.2 — average delivery time by restaurant
return orders
    .SelectMany(o => o.deliverys.Select(d => new { o.restaurantId, d }))
    .GroupBy(x => x.restaurantId)
    .ToDictionary(g => g.Key, g => g.Average(x => (double)x.d.GetDurationMinutes()));

// TASK 3 — top restaurants by delivered revenue
if (n <= 0) return new List<int>();
return orders
    .Where(o => o.status == OrderStatus.DELIVERED)
    .GroupBy(o => o.restaurantId)
    .Select(g => new { RestaurantId = g.Key, Revenue = g.Sum(o => o.orderValue) })
    .OrderByDescending(x => x.Revenue)
    .ThenBy(x => x.RestaurantId)
    .Take(n)
    .Select(x => x.RestaurantId)
    .ToList();
```
