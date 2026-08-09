# FoodDeliveryPlatformMock — Solution & Concepts Guide

Interview problem: build and extend a food delivery order management backend in C#.

| File | Purpose |
|------|---------|
| `../FoodDeliveryPlatformMock.cs` | Original practice file (test runner class is named `Main` — CS0542 if copied as-is) |
| `FoodDeliveryPlatformMock.cs` | Runnable project copy (`Main` renamed to `FoodDeliveryPlatformStub`; Tasks 1–3 left as TODO) |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Bug Fix — Order Statistics](#bug-fix--getorderstatistics)
5. [Task 1 — Revenue Per Restaurant](#task-1--getrevenueperrestaurant)
6. [Task 2.1 — Average Delivery Distance Per Customer](#task-21--getaveragedeliverydistancepercustomer)
7. [Task 2.2 — Highly Active Customers](#task-22--gethighlyactivecustomers)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`OrderManager` is the central service that:

- Stores **orders** as they move through the delivery lifecycle
- Computes **statistics** over order status (total, active, closed)
- Aggregates **revenue**, **distance analytics**, and **customer activity**

Order lifecycle:

```
PLACED → PREPARING → OUT_FOR_DELIVERY → DELIVERED
                                      ↘ CANCELED (from any active stage)
```

```
┌─────────────────┐     ┌──────────────────┐
│   AddOrder()    │────▶│  List<Order>     │
└─────────────────┘     └──────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Bug + Tasks 1–3)    │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Enums (`OrderStatus`)

Enums restrict status values to a fixed set — safer than raw strings or magic integers.

```csharp
enum OrderStatus
{
    PLACED, PREPARING, OUT_FOR_DELIVERY, DELIVERED, CANCELED
}
```

**Active vs closed** is a business rule, not an enum feature:

| Category | Statuses |
|----------|----------|
| **Active** | `PLACED`, `PREPARING`, `OUT_FOR_DELIVERY` |
| **Closed** | `DELIVERED`, `CANCELED` |

Read the spec carefully: **closed ≠ delivered only**. Canceled orders are finished and count as closed.

---

### 2. Dictionary vs List — When to Use Which

| Structure | Use case in this problem |
|-----------|--------------------------|
| `List<Order>` | Master collection of all orders; filter/group/aggregate |
| `Dictionary<string, int>` | Named statistics keys (`total_orders`, `active_orders`, …) |
| `Dictionary<int, double>` | Map `restaurantId` or `customerId` to a computed metric |
| `List<int>` | Sorted list of customer IDs meeting a threshold |

**Key insight:** All analytics **query the list** and **project results** into the appropriate return type.

---

### 3. Filter Before Aggregate

Different tasks filter on different status subsets:

| Method | Which orders count? |
|--------|---------------------|
| `GetOrderStatistics` | All orders (partition by status) |
| `GetRevenuePerRestaurant` | **DELIVERED only** |
| `GetAverageDeliveryDistancePerCustomer` | **DELIVERED only** |
| `GetHighlyActiveCustomers` | **All statuses** (including CANCELED) |

This is the most common interview trap in this problem — underline the filter condition for each task.

---

### 4. LINQ (Language Integrated Query)

| Method | What it does |
|--------|--------------|
| `Where` | Filter (like SQL `WHERE`) |
| `GroupBy` | Partition into buckets by a key |
| `Select` | Project/transform each element |
| `OrderBy` | Sort ascending |
| `Sum`, `Average`, `Count` | Aggregations |
| `ToDictionary` | Materialize key-value pairs |
| `ToList` | Materialize a list |

---

### 5. Guard Clauses & Edge Cases

| Condition | Expected behavior |
|-----------|-------------------|
| Customer with no delivered orders (Task 2.1) | Not in the result map |
| Restaurant with no delivered orders (Task 1) | Not in the revenue map |
| Customer with fewer than 3 orders (Task 2.2) | Not in the active list |
| Empty order list | `total=0`, `active=0`, `closed=0`; empty maps/lists |

---

## Data Structures Used

### Domain Class

```
Order
├── orderId       : int
├── restaurantId  : int
├── customerId    : int
├── orderValue    : double
├── distanceKm    : double
└── status        : OrderStatus
```

### Return Types

| Method | Return type | Key | Value |
|--------|-------------|-----|-------|
| `GetOrderStatistics` | `Dictionary<string, int>` | stat name | count |
| `GetRevenuePerRestaurant` | `Dictionary<int, double>` | restaurantId | total revenue |
| `GetAverageDeliveryDistancePerCustomer` | `Dictionary<int, double>` | customerId | avg distance |
| `GetHighlyActiveCustomers` | `List<int>` | — | sorted customer IDs |

---

## Bug Fix — `GetOrderStatistics()`

### Requirement

Return global order statistics:

- `total_orders` — count of **all** orders
- `active_orders` — count where status is `PLACED`, `PREPARING`, or `OUT_FOR_DELIVERY`
- `closed_orders` — count where status is `DELIVERED` **or** `CANCELED`

### The Bug

```csharp
// BUG: only counts DELIVERED — ignores CANCELED
int closed = orders
    .Where(o => o.status == OrderStatus.DELIVERED)
    .Count();
```

Canceled orders are terminal (closed) but not counted, so any mix of delivered + canceled under-reports `closed_orders`.

### Fix

```csharp
int closed = orders
    .Where(o => o.status == OrderStatus.DELIVERED ||
                o.status == OrderStatus.CANCELED)
    .Count();
```

Alternative — count complement of active:

```csharp
int closed = total - active;
```

This works here because every order is either active or closed (no other statuses). Prefer the explicit filter when status sets might grow.

### Test Data Breakdown

| Order ID | Status | Counts toward |
|----------|--------|---------------|
| 1 | DELIVERED | total, closed |
| 2 | CANCELED | total, closed |

**Expected:** `total_orders=2`, `active_orders=0`, `closed_orders=2`

**Bug output:** `closed_orders=1` (CANCELED missing)

---

## Task 1 — `GetRevenuePerRestaurant()`

### Requirement

Return a dictionary `{ restaurantId: totalRevenue }`.

**Rules:**
- Only `DELIVERED` orders contribute to revenue
- Canceled and active orders are ignored
- Restaurants with no delivered orders do not appear in the map

### Solution

```csharp
public Dictionary<int, double> GetRevenuePerRestaurant()
{
    return orders
        .Where(o => o.status == OrderStatus.DELIVERED)
        .GroupBy(o => o.restaurantId)
        .ToDictionary(g => g.Key, g => g.Sum(o => o.orderValue));
}
```

### Step-by-Step (Test Case)

```
Orders:
  orderId=1, restaurantId=10, value=50.0, DELIVERED  ✓

After GroupBy:
  restaurant 10 → [50.0]  → sum = 50.0

Result: { 10: 50.0 }
```

### Worked Example (Multiple Orders)

```
restaurant 10: DELIVERED 50.0, DELIVERED 30.0  → 80.0
restaurant 20: PLACED 100.0                     → (ignored)
restaurant 20: CANCELED 40.0                    → (ignored)
restaurant 20: DELIVERED 20.0                   → 20.0

Result: { 10: 80.0, 20: 20.0 }
```

### Concepts Applied

1. **Filter by status first** — revenue only from completed deliveries
2. **GroupBy restaurantId** — bucket orders per restaurant
3. **Sum orderValue** — aggregate within each bucket
4. **ToDictionary** — materialize the result map

---

## Task 2.1 — `GetAverageDeliveryDistancePerCustomer()`

### Requirement

Return a dictionary `{ customerId: averageDistanceKm }`.

**Rules:**
- Only `DELIVERED` orders are included in the average
- If a customer has no delivered orders, they are **not** in the result
- Use `distanceKm` field for distance

### Solution

```csharp
public Dictionary<int, double> GetAverageDeliveryDistancePerCustomer()
{
    return orders
        .Where(o => o.status == OrderStatus.DELIVERED)
        .GroupBy(o => o.customerId)
        .ToDictionary(g => g.Key, g => g.Average(o => o.distanceKm));
}
```

### Step-by-Step (Test Case)

```
Orders:
  orderId=1, customerId=100, distance=5.0, DELIVERED  ✓

After GroupBy:
  customer 100 → [5.0]  → average = 5.0

Result: { 100: 5.0 }
```

### Worked Example (Multiple Deliveries)

```
customer 100: DELIVERED 5.0 km, DELIVERED 15.0 km  → avg = 10.0
customer 200: PLACED 8.0 km                           → (ignored)
customer 200: DELIVERED 4.0 km                        → avg = 4.0

Result: { 100: 10.0, 200: 4.0 }
```

Customer 200's PLACED order does not affect the average — only delivered distances count.

### Concepts Applied

1. **Same filter pattern as Task 1** — `DELIVERED` only
2. **GroupBy customerId** instead of restaurantId
3. **Average** instead of Sum — LINQ handles mean calculation

---

## Task 2.2 — `GetHighlyActiveCustomers()`

### Requirement

Return a **sorted list** of customer IDs who have placed **3 or more** orders in total.

**Rules:**
- Count **all** orders regardless of status (PLACED, CANCELED, DELIVERED, etc.)
- Return IDs sorted ascending
- Customers with fewer than 3 orders are excluded

### Solution

```csharp
public List<int> GetHighlyActiveCustomers()
{
    return orders
        .GroupBy(o => o.customerId)
        .Where(g => g.Count() >= 3)
        .Select(g => g.Key)
        .OrderBy(id => id)
        .ToList();
}
```

### Step-by-Step (Test Case)

```
Orders for customer 500:
  orderId=1, PLACED    ✓
  orderId=2, DELIVERED ✓
  orderId=3, CANCELED  ✓

Count = 3  →  meets threshold

Result: [500]
```

### Worked Example (Multiple Customers)

```
customer 100: 2 orders  → excluded
customer 200: 3 orders  → included
customer 300: 5 orders  → included
customer 400: 1 order   → excluded

Result: [200, 300]   (sorted ascending)
```

### Why No Status Filter?

The spec says "placed 3 or more orders in total (any status, including CANCELED)." A canceled order still counts as customer activity — they engaged with the platform.

### Concepts Applied

1. **GroupBy + Count** — tally orders per customer
2. **Where threshold** — `>= 3` filter after grouping
3. **OrderBy** — sorted output as required

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(o => o.status == OrderStatus.DELIVERED)
Group:       .GroupBy(o => o.restaurantId)
             .GroupBy(o => o.customerId)
Aggregate:   .Sum(o => o.orderValue)
             .Average(o => o.distanceKm)
             .Count()                    // on a group
Sort:        .OrderBy(id => id)
Threshold:   .Where(g => g.Count() >= 3)
Convert:     .ToDictionary(g => g.Key, g => g.Sum(...))
             .ToList()
```

### Equivalent Imperative Style (Task 1 without LINQ)

```csharp
var result = new Dictionary<int, double>();

foreach (Order o in orders)
{
    if (o.status != OrderStatus.DELIVERED)
        continue;

    if (!result.ContainsKey(o.restaurantId))
        result[o.restaurantId] = 0.0;

    result[o.restaurantId] += o.orderValue;
}

return result;
```

LINQ is shorter; imperative style shows you understand the underlying loop — useful in interviews.

---

## Interview Tips

### Reading the Problem

1. **Underline filter conditions** — "only DELIVERED" vs "any status" changes every task
2. **Know active vs closed** — closed includes CANCELED, not just DELIVERED
3. **Check sort requirements** — Task 2.2 requires ascending customer ID order
4. **Check inclusion rules** — empty buckets (no delivered orders) are omitted from maps

### Debugging the Bug

When a count is wrong, ask: *"Am I counting the right subset?"* The bug counted only DELIVERED for closed, missing CANCELED.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Closed = DELIVERED only | Include `CANCELED` in closed count |
| Revenue from all orders | Filter `status == DELIVERED` before Sum |
| Average distance includes active orders | Filter `DELIVERED` only |
| Highly active excludes CANCELED | Count **all** statuses |
| Unsorted customer list | Add `.OrderBy(id => id)` |
| Using `total - active` without verifying partition | Ensure no orphan statuses exist |

### Complexity (for follow-up questions)

For `n` orders:

| Method | Time | Space |
|--------|------|-------|
| Bug fix | O(n) | O(1) |
| Task 1 | O(n) | O(r) restaurants |
| Task 2.1 | O(n) | O(c) customers |
| Task 2.2 | O(n log c) | O(c) |

All are fine for typical interview data sizes.

---

## Quick Reference — All Solutions

```csharp
// BUG FIX — include CANCELED in closed count
int closed = orders
    .Where(o => o.status == OrderStatus.DELIVERED ||
                o.status == OrderStatus.CANCELED)
    .Count();

// TASK 1 — revenue per restaurant (DELIVERED only)
return orders
    .Where(o => o.status == OrderStatus.DELIVERED)
    .GroupBy(o => o.restaurantId)
    .ToDictionary(g => g.Key, g => g.Sum(o => o.orderValue));

// TASK 2.1 — average distance per customer (DELIVERED only)
return orders
    .Where(o => o.status == OrderStatus.DELIVERED)
    .GroupBy(o => o.customerId)
    .ToDictionary(g => g.Key, g => g.Average(o => o.distanceKm));

// TASK 2.2 — customers with 3+ orders (any status), sorted
return orders
    .GroupBy(o => o.customerId)
    .Where(g => g.Count() >= 3)
    .Select(g => g.Key)
    .OrderBy(id => id)
    .ToList();
```

Apply these implementations in `FoodDeliveryPlatformMock.cs` inside `OrderManager` to pass all tests.
