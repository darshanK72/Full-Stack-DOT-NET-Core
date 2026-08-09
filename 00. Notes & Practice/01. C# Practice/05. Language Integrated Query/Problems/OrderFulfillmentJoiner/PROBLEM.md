---
module: 05. Language Integrated Query
difficulty: Hard
chapters: 05 Joins
domain: OrderFulfillment
---

# Order Fulfillment Joiner

Build a **.NET 8 console application from scratch** that inner-joins customers to orders and left-outer-joins customers to optional shipment tracking.

## Business context

Customer service pulls order history for active accounts and must still list customers who registered but never placed an order. Shipment rows may be missing for some orders.

## Definitions

**Record `Customer`**

- `CustomerId` (int)
- `Name` (string)

**Record `OrderHeader`**

- `OrderId` (int)
- `CustomerId` (int)
- `OrderTotal` (decimal)

**Record `Shipment`**

- `OrderId` (int)
- `TrackingNumber` (string)

**Record `CustomerOrderRow`**

- `CustomerName` (string)
- `OrderId` (int)
- `OrderTotal` (decimal)

**Record `CustomerWithOptionalShipment`**

- `CustomerName` (string)
- `OrderId` (int?)
- `TrackingNumber` (string?) — null when no shipment

**Class `OrderFulfillmentJoiner`**

- Constructor accepts customers, orders, shipments sequences
- `IEnumerable<CustomerOrderRow> InnerCustomerOrders()` — **`Join`** customers to orders on CustomerId
- `IEnumerable<CustomerWithOptionalShipment> LeftOuterCustomerShipments()` — **`GroupJoin`** customers to orders, **`DefaultIfEmpty`**, then join/SelectMany to shipments with left outer on shipment side OR equivalent: GroupJoin customer→order, DefaultIfEmpty, project; then left join shipment on OrderId
- Simpler required shape: **`LeftCustomersWithoutOrders()`** — GroupJoin customers to orders, `from o in g.DefaultIfEmpty()` select customer name and nullable OrderId/Total
- `IEnumerable<CustomerOrderRow> QuerySyntaxInnerJoin()` — query syntax `join … on … equals …`
- `int CountCustomersWithoutOrders()` — count from LeftCustomersWithoutOrders where OrderId is null

## Demo Main

1. Print inner join rows — customers without orders absent.
2. Print left outer — at least one customer with null OrderId.
3. Print customer with order but null TrackingNumber after shipment left join.
4. Query syntax inner join count matches method syntax.
5. Print `CountCustomersWithoutOrders`.

## Constraints

- net8, Join / GroupJoin / DefaultIfEmpty
- Query join uses **`equals`**, not `==`
- Null-safe projection for missing inner rows

## Non-goals

Cross join, EF Core

## Evaluation

[EVALUATION.md](EVALUATION.md)
