---
module: 04. Functional Style Programming
difficulty: Medium
chapters: 04 Extension Methods
domain: OrderReceipt
---

# Order Receipt Extensions

Build a **.NET 8 console application from scratch** that formats order receipts using extension methods on strings, dates, line items, and sequences.

## Business context

Customer support prints order receipts from raw API payloads. Formatting helpers live in static extension classes so call sites read fluently (`orderId.Masked()`, `lines.TotalAmount()`) without polluting domain types.

## Definitions

**Record `OrderLine`**

- `Sku` (string)
- `Quantity` (int > 0)
- `UnitPrice` (decimal > 0)
- Read-only property `LineTotal => Quantity * UnitPrice`

**Static class `StringExtensions`** (same namespace as demo — no extra using needed)

- `string Masked(this string value, int visibleTail = 4)` — if null/empty return `"(empty)"`; if length ≤ visibleTail return all asterisks same length; otherwise mask all but last `visibleTail` chars with `*`
- `bool IsValidOrderId(this string? value)` — true when non-null, length 8–12, all alphanumeric

**Static class `DateTimeExtensions`**

- `string ToReceiptDate(this DateTime value)` — format `"yyyy-MM-dd HH:mm"` (24-hour)
- `DateTime StartOfDay(this DateTime value)` — date at 00:00:00 same kind (preserve `DateTimeKind` of input)

**Static class `OrderLineExtensions`**

- `string ToReceiptLine(this OrderLine line)` — `"{Sku} x{Quantity} @ {UnitPrice:C} = {LineTotal:C}"`
- `decimal TotalAmount(this IEnumerable<OrderLine> lines)` — sum `LineTotal`; null collection throws; **student may use `.Sum(l => l.LineTotal)`** inside this extension (only LINQ allowed)

**Static class `NumericExtensions`**

- `T Clamp<T>(this T value, T min, T max) where T : IComparable<T>` — return `min` if value < min, `max` if value > max, else value; throw if `min.CompareTo(max) > 0`

## Demo Main

1. Mask an order id and print; validate good/bad ids with `IsValidOrderId`.
2. Format `DateTime.Now` with `ToReceiptDate`; show `StartOfDay`.
3. Build 3+ `OrderLine` entries; print each `ToReceiptLine` and sequence `TotalAmount`.
4. Clamp sample decimals and ints via generic `Clamp`.

## Constraints

- net8, explicit usings, `decimal` for money
- Extension methods in static classes with `this` first parameter
- Call at least one extension in instance style and one in static style in Main (comment which is which)

## Non-goals

Full receipt PDF, localization, JSON parsing

## Evaluation

[EVALUATION.md](EVALUATION.md)
