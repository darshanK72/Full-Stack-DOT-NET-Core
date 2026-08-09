# StockTradingMock — Solution & Concepts Guide

Interview problem: build and extend a stock trading data management backend in C#.

| File | Purpose |
|------|---------|
| `StockTradingMock.cs` | Problem statement + stub implementations + inline test suite |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix (Empty List)](#task-1--bug-fix-empty-list)
5. [Task 2 — Biggest Price Change](#task-2--getbiggestchange)
6. [Task 3 — Portfolio Valuation](#task-3--gettotal)
7. [Task 4 — Profit Calculation](#task-4--getprofit)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

The system tracks **stock prices over time** and **user transactions** (buys/sells), then computes portfolio analytics:

- **StockCollection** — price history for one stock; stats and day-over-day changes
- **Tradebook** — transaction log; portfolio value and profit/loss

Each task builds on the previous one — from empty-list guard clauses to multi-stock portfolio math.

```
┌──────────────────┐     ┌─────────────────────┐
│  PriceRecord     │────▶│  StockCollection    │
│  (date, price)   │     │  per stock symbol   │
└──────────────────┘     └─────────────────────┘
                                    │
┌──────────────────┐                │
│  Transaction     │────▶┌──────────▼──────────┐
│  (buy / sell)    │     │  Tradebook          │
└──────────────────┘     │  GetTotal, GetProfit│
                         └─────────────────────┘
```

---

## Core Concepts

### 1. Empty Collection Handling (Task 1)

LINQ aggregations like `.Max()`, `.Min()`, and `.Sum()` **throw** on empty sequences. Division by zero occurs when dividing by `Count == 0`.

**Pattern:** Guard at the top of the method:

```csharp
if (PriceRecords.Count == 0) return -1;   // sentinel for "no data"
```

The test expects `-1` for `GetMaxPrice()` on an empty collection. Apply the same guard to `GetMinPrice()` and `GetAvgPrice()` for consistency.

---

### 2. Sorting by Date String (Task 2)

Dates are stored as `"YYYY-MM-DD"` strings. **ISO 8601 date strings sort lexicographically in chronological order**, so:

```csharp
PriceRecords.OrderBy(r => r.Date)
```

works without parsing to `DateTime`. This is a common interview shortcut when the format is guaranteed.

---

### 3. Consecutive-Day Price Change (Task 2)

After sorting by date, walk adjacent pairs:

```
Date:   2023-06-25  →  2023-06-29  →  2023-07-01  →  2023-07-06
Price:      90      →     110      →     112      →     105
Change:          |20|          |2|           |7|
```

**Biggest change** = maximum **absolute** difference between consecutive prices, along with the two dates that produced it.

Return format: `object[] { changeAmount, startDate, endDate }`

---

### 4. Net Holdings from Transactions (Tasks 3 & 4)

For each stock symbol, net quantity held:

```csharp
quantity = buys - sells
// "buy"  → +Quantity
// "sell" → -Quantity
```

Use `GroupBy(t => t.Stock.Symbol)` then sum signed quantities.

---

### 5. Price Lookup by Date vs Latest Price

| Need | How to get price |
|------|------------------|
| Transaction cost/revenue (Task 4) | Price on the **transaction date** |
| Current portfolio value (Tasks 3 & 4) | **Latest** price record (max date) |

The problem guarantees every transaction date has a matching price record in the corresponding `StockCollection`.

---

### 6. Profit Formula (Task 4)

```
Total Profit = Revenue from Sells
             + Current Value of Remaining Holdings
             - Total Cost of All Buys
```

| Component | Calculation |
|-----------|-------------|
| Total Cost of Buys | Sum of (buy quantity × price on buy date) |
| Revenue from Sells | Sum of (sell quantity × price on sell date) |
| Current Value | Sum of (remaining quantity × latest price) per stock |

**Realized profit** is embedded in (Revenue minus cost of sold shares). **Unrealized profit** is (Current Value minus cost of unsold shares). The formula combines both in one expression.

---

## Data Structures Used

### Domain Classes

```
Stock
├── Symbol : string
└── Name   : string

PriceRecord
├── Stock : Stock
├── Price : int
└── Date  : string   (YYYY-MM-DD)

Transaction
├── Stock    : Stock
├── Type     : string   ("buy" | "sell")
├── Date     : string
└── Quantity : int

StockCollection
├── Stock         : Stock
└── PriceRecords  : List<PriceRecord>

Tradebook
└── Transactions  : List<Transaction>
```

---

## Task 1 — Bug Fix (Empty List)

### Requirement

`GetMaxPrice()`, `GetMinPrice()`, and `GetAvgPrice()` must not crash when `PriceRecords` is empty. The test expects `GetMaxPrice()` to return **-1** on an empty collection.

### The Bug

```csharp
// BUG: Max() throws InvalidOperationException on empty sequence
public int GetMaxPrice()
{
    return PriceRecords.Select(r => r.Price).Max();
}

// BUG: Min() throws on empty sequence
public int GetMinPrice()
{
    return PriceRecords.Select(r => r.Price).Min();
}

// BUG: division by zero when Count == 0
public double GetAvgPrice()
{
    double total = PriceRecords.Select(r => r.Price).Sum();
    return total / PriceRecords.Count;
}
```

### Fix

```csharp
public int GetMaxPrice()
{
    if (PriceRecords.Count == 0) return -1;
    return PriceRecords.Select(r => r.Price).Max();
}

public int GetMinPrice()
{
    if (PriceRecords.Count == 0) return -1;
    return PriceRecords.Select(r => r.Price).Min();
}

public double GetAvgPrice()
{
    if (PriceRecords.Count == 0) return -1.0;
    double total = PriceRecords.Select(r => r.Price).Sum();
    return total / PriceRecords.Count;
}
```

### Test Data

| Input | Expected |
|-------|----------|
| Empty `StockCollection` | `GetMaxPrice()` returns `-1` |

### Concept: Why LINQ Throws on Empty

`.Max()` and `.Min()` have no meaningful result for an empty sequence, so they throw `InvalidOperationException`. Always check `.Count == 0` or use `.DefaultIfEmpty(sentinel)` when empty input is valid.

---

## Task 2 — `GetBiggestChange()`

### Requirement

Return the largest **absolute** price change between any two **consecutive calendar days** (sorted by date), as:

```csharp
object[] { changeAmount, earlierDate, laterDate }
```

Return `null` if fewer than 2 price records exist.

### Solution

```csharp
public object[] GetBiggestChange()
{
    if (PriceRecords.Count < 2) return null;

    List<PriceRecord> sorted = PriceRecords.OrderBy(r => r.Date).ToList();

    int maxChange = 0;
    string startDate = "";
    string endDate = "";

    for (int i = 0; i < sorted.Count - 1; i++)
    {
        int change = Math.Abs(sorted[i + 1].Price - sorted[i].Price);
        if (change > maxChange)
        {
            maxChange = change;
            startDate = sorted[i].Date;
            endDate = sorted[i + 1].Date;
        }
    }

    return new object[] { maxChange, startDate, endDate };
}
```

### Step-by-Step (Test Data)

```
Unsorted input:
  90  on 2023-06-25
  110 on 2023-06-29

After OrderBy date:
  2023-06-25: 90  →  2023-06-29: 110
  change = |110 - 90| = 20

Result: { 20, "2023-06-25", "2023-06-29" }
```

### Full Example (from problem statement)

```
Unsorted:  110@06-29, 112@07-01, 90@06-25, 105@07-06

Sorted:    90 → 110 → 112 → 105
Changes:      20     2      7

Biggest: 20 (2023-06-25 → 2023-06-29)
```

### Bonus: `GetAllChanges()`

Returns every consecutive change (useful for debugging or extended tests):

```csharp
public List<object[]> GetAllChanges()
{
    List<PriceRecord> sorted = PriceRecords.OrderBy(r => r.Date).ToList();
    var result = new List<object[]>();

    for (int i = 0; i < sorted.Count - 1; i++)
    {
        int change = sorted[i + 1].Price - sorted[i].Price;  // signed, not absolute
        result.Add(new object[] { change, sorted[i].Date, sorted[i + 1].Date });
    }

    return result;
}
```

Note: `GetBiggestChange` uses **absolute** change; `GetAllChanges` typically returns **signed** change (+/-).

---

## Task 3 — `GetTotal()`

### Requirement

Return the **current total value** of all stocks the user still holds, priced at each stock's **latest** price record.

### Solution

```csharp
public int GetTotal(List<StockCollection> stockCollections)
{
    Dictionary<string, StockCollection> bySymbol =
        stockCollections.ToDictionary(sc => sc.Stock.Symbol);

    Dictionary<string, int> holdings = Transactions
        .GroupBy(t => t.Stock.Symbol)
        .ToDictionary(
            g => g.Key,
            g => g.Sum(t => t.Type == "buy" ? t.Quantity : -t.Quantity));

    int total = 0;

    foreach (KeyValuePair<string, int> entry in holdings)
    {
        if (entry.Value <= 0) continue;

        StockCollection sc = bySymbol[entry.Key];
        int latestPrice = sc.PriceRecords.OrderByDescending(r => r.Date).First().Price;
        total += entry.Value * latestPrice;
    }

    return total;
}
```

### Step-by-Step (Test Data)

```
Transaction: buy 10 AAPL on 2023-06-25
Latest price: 105 on 2023-07-06

Holdings: 10 shares
Value:    10 × 105 = 1050
```

### Concepts Applied

1. **GroupBy symbol** — aggregate buys and sells per stock
2. **Signed quantity** — `"buy"` adds, `"sell"` subtracts
3. **Latest price** — `OrderByDescending(r => r.Date).First()`
4. **Dictionary lookup** — O(1) access to `StockCollection` by symbol

---

## Task 4 — `GetProfit()`

### Requirement

Calculate total net profit/loss across all stocks:

```
Profit = Revenue from Sells + Current Value of Holdings - Total Cost of Buys
```

Return a `double`.

### Solution

```csharp
public double GetProfit(List<StockCollection> stockCollections)
{
    Dictionary<string, StockCollection> bySymbol =
        stockCollections.ToDictionary(sc => sc.Stock.Symbol);

    double totalCost = 0;
    double totalRevenue = 0;

    foreach (Transaction t in Transactions)
    {
        StockCollection sc = bySymbol[t.Stock.Symbol];
        int priceOnDate = sc.PriceRecords.First(r => r.Date == t.Date).Price;

        if (t.Type == "buy")
            totalCost += t.Quantity * priceOnDate;
        else
            totalRevenue += t.Quantity * priceOnDate;
    }

    Dictionary<string, int> holdings = Transactions
        .GroupBy(t => t.Stock.Symbol)
        .ToDictionary(
            g => g.Key,
            g => g.Sum(t => t.Type == "buy" ? t.Quantity : -t.Quantity));

    double currentValue = 0;

    foreach (KeyValuePair<string, int> entry in holdings)
    {
        if (entry.Value <= 0) continue;

        StockCollection sc = bySymbol[entry.Key];
        int latestPrice = sc.PriceRecords.OrderByDescending(r => r.Date).First().Price;
        currentValue += entry.Value * latestPrice;
    }

    return totalRevenue + currentValue - totalCost;
}
```

### Step-by-Step (Test Data)

```
Buy 10 AAPL on 2023-01-01 at price 100  →  cost = 1000
Latest price on 2023-01-02: 150

Revenue:       0        (no sells)
Current value: 10 × 150 = 1500
Total cost:    1000

Profit = 0 + 1500 - 1000 = 500.0
```

### Worked Example with Sells

```
Buy  10 @ $100 on Day 1   → cost += 1000
Sell  5 @ $120 on Day 2   → revenue += 600
Latest price: $110 on Day 3

Remaining: 5 shares
Current value: 5 × 110 = 550

Profit = 600 + 550 - 1000 = 150
```

Breakdown:
- Realized (sold 5): revenue 600 minus cost of 5 shares (500) = **+100**
- Unrealized (hold 5): value 550 minus cost of 5 shares (500) = **+50**
- Total: **+150**

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(r => r.Date == targetDate)
Sort:        .OrderBy(r => r.Date)              // ascending (chronological)
             .OrderByDescending(r => r.Date)    // latest first
Group:       .GroupBy(t => t.Stock.Symbol)
Aggregate:   .Sum(t => signedQuantity)
             .Max() / .Min()                    // guard empty first!
Lookup:      .First(r => r.Date == t.Date)
             .First()                           // after OrderByDescending = latest
Convert:     .ToDictionary(sc => sc.Stock.Symbol)
             .ToList()
```

### Helper: Price on a Specific Date

```csharp
int GetPriceOnDate(StockCollection sc, string date)
{
    return sc.PriceRecords.First(r => r.Date == date).Price;
}
```

### Helper: Latest Price

```csharp
int GetLatestPrice(StockCollection sc)
{
    return sc.PriceRecords.OrderByDescending(r => r.Date).First().Price;
}
```

---

## Interview Tips

### Reading the Problem

1. **Distinguish transaction-date price vs latest price** — Task 3/4 use both
2. **Absolute vs signed change** — Task 2 biggest change uses `Math.Abs`
3. **Empty input rules** — Task 1 sentinel `-1`; Task 2 returns `null` when fewer than 2 records
4. **Guaranteed data** — price records exist for every transaction date; no need to handle missing prices

### Debugging Task 1

When LINQ throws `InvalidOperationException: Sequence contains no elements`, check whether the collection is empty before calling `.Max()`, `.Min()`, or `.First()`.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Forgetting to sort by date before computing changes | `OrderBy(r => r.Date)` first |
| Using signed change instead of absolute for biggest | `Math.Abs(price2 - price1)` |
| Pricing holdings at transaction date instead of latest | Use `OrderByDescending(r => r.Date).First()` for current value |
| Using buy price for sell revenue | Look up price on **each transaction's date** |
| Not handling zero/negative holdings | Skip stocks where net quantity is 0 or less |
| CS0542 compile error (Main class + Main method) | Rename entry class to `StockTradingMockStub` |

### Complexity (for follow-up questions)

For `p` price records, `t` transactions, and `s` stocks:

| Method | Time | Space |
|--------|------|-------|
| GetMaxPrice / Min / Avg | O(p) | O(1) |
| GetBiggestChange | O(p log p) | O(p) |
| GetTotal | O(t + s × p) | O(s) |
| GetProfit | O(t + s × p) | O(s) |

All are fine for typical interview data sizes.

---

## Quick Reference — All Four Solutions

```csharp
// TASK 1 — guard empty PriceRecords
if (PriceRecords.Count == 0) return -1;

// TASK 2 — sort by date, find max absolute consecutive change
List<PriceRecord> sorted = PriceRecords.OrderBy(r => r.Date).ToList();
int change = Math.Abs(sorted[i + 1].Price - sorted[i].Price);

// TASK 3 — net holdings × latest price
int latestPrice = sc.PriceRecords.OrderByDescending(r => r.Date).First().Price;
total += quantity * latestPrice;

// TASK 4 — revenue + current value - cost
return totalRevenue + currentValue - totalCost;
```

---

## Running the Project

```bash
cd "15. StockTradingMock"
dotnet build
dotnet run
```

Expected output before solving (all tasks fail):

```
=== RUNNING STOCK TRADING TEST SUITE ===

FAIL: TASK 1: Bug Check (Empty List)
FAIL: TASK 2: Biggest Change Logic
FAIL: TASK 3: Portfolio Valuation
FAIL: TASK 4: Profit Calculation
```

After implementing all fixes, all four tests should **PASS**.
