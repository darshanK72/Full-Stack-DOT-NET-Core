---
module: 04. Functional Style Programming
difficulty: Hard
chapters: 01 Delegates, 05 Func Action and Predicate
domain: OrderValidation
---

# Callback Orchestrator

Build a **.NET 8 console application from scratch** that orchestrates order validation through a custom delegate pipeline combined with `Func`, `Action`, and `Predicate` reporting hooks.

## Business context

Before release to fulfillment, orders pass a validation chain: business rules (custom delegate type for domain clarity) and structural checks (built-in generic delegates). A single orchestrator coordinates validation, enrichment, and audit reporting without tight coupling to console output.

## Definitions

**Delegate `ValidationRule`**

```csharp
public delegate ValidationResult ValidationRule(OrderContext context);
```

**Record `ValidationResult`**

- `bool IsValid`
- `string Code` (e.g. `"OK"`, `"AMOUNT"`, `"LINES"`)
- `string Message`

**Record `OrderContext`**

- `OrderId` (string)
- `LineCount` (int)
- `TotalAmount` (decimal)
- Mutable `List<string> AuditTrail` — append-only log strings

**Static class `DomainRules`** — methods matching `ValidationRule`:

| Method | Rule |
|--------|------|
| `RequireNonEmptyOrderId` | Fail `"ID"` if OrderId null/whitespace |
| `RequireMinimumAmount` | Fail `"AMOUNT"` if TotalAmount < 10m |
| `RequireLineLimit` | Fail `"LINES"` if LineCount > 50 |

**Class `CallbackOrchestrator`**

- Constructor accepts `Action<string>? onReport` (stored for optional console-style reporting)
- `ValidationResult RunValidation(OrderContext context, IEnumerable<ValidationRule> rules)` — null checks; foreach rule invoke; on first failure append audit `"FAIL {Code}"`, null-safe `_onReport` with message, return result; on success append `"PASS"` and return `new ValidationResult(true, "OK", "All rules passed")`
- `bool RunStructuralChecks(OrderContext context, params Predicate<OrderContext>[] checks)` — all must pass; use `Func`-free predicates only; append `"STRUCT PASS"` or `"STRUCT FAIL"` to audit
- `decimal ComputeFee(OrderContext context, Func<OrderContext, decimal> feeCalculator, Action<string>? onComputed)` — invoke calculator; null-safe notify `"Fee: {amount:C}"`; return amount
- `void PublishAudit(OrderContext context, Action<OrderContext> publisher)` — null-safe invoke publisher with context

**Static class `StructuralChecks`**

- `Predicate<OrderContext> HasLines` — `c => c.LineCount > 0`
- `Predicate<OrderContext> AmountWithin(decimal max)` — statement lambda block comparing `TotalAmount <= max`

## Demo Main

1. Build valid and invalid contexts.
2. Run `RunValidation` with three domain rules; print result codes.
3. Run structural checks with `HasLines` and `AmountWithin(500m)`.
4. Compute fee with `Func` `(ctx => ctx.TotalAmount * 0.03m)` and reporting Action.
5. `PublishAudit` with Action that prints joined audit trail.
6. Show custom `ValidationRule` assigned from method group vs lambda wrapping extra logging.

## Constraints

- net8, explicit usings, `decimal` for money
- Use custom `ValidationRule` for domain rules; `Predicate`/`Func`/`Action` for structural/fee/report steps
- Null-safe invoke on all optional Action callbacks

## Non-goals

Async pipelines, DI containers, events keyword

## Evaluation

[EVALUATION.md](EVALUATION.md)
