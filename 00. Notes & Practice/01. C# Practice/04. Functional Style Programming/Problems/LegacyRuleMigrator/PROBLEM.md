---
module: 04. Functional Style Programming
difficulty: Medium
chapters: 03 Anonymous Methods
domain: OrderCompliance
---

# Legacy Rule Migrator

Build a **.NET 8 console application from scratch** that models order validation rules using anonymous methods, then provides lambda equivalents for migration comparison.

## Business context

A legacy order-management service still registers compliance rules as anonymous methods against custom delegate types. You are modernizing the rule registry while preserving behavior for auditors who compare old and new implementations side by side.

## Definitions

**Delegate `OrderRule`**

```csharp
public delegate bool OrderRule(Order order);
```

**Delegate `OrderNotifier`**

```csharp
public delegate void OrderNotifier(string message);
```

**Record `Order`**

- `OrderId` (string)
- `LineCount` (int ≥ 0)
- `TotalAmount` (decimal ≥ 0)

**Static class `RuleFactory`**

- `OrderRule BuildMaxLineItemsRule(int maxLines)` — must return an **anonymous method** (block syntax with `delegate` keyword, explicit `(Order order)` parameter list) returning `order.LineCount <= maxLines`
- `OrderRule BuildMaxLineItemsRuleLambda(int maxLines)` — **lambda equivalent** of the above (for migration demo)
- `OrderNotifier BuildConsoleNotifier(string prefix)` — **void anonymous method** writing `"{prefix}: {message}"` to `Console.WriteLine`; reject null/empty prefix

**Class `RuleRunner`**

- `bool EvaluateAll(Order order, IEnumerable<OrderRule> rules)` — returns `false` immediately if any rule returns false; null order or rules throws
- `void NotifyAll(string message, IEnumerable<OrderNotifier> notifiers)` — invokes each notifier; skip null entries in collection

**Static class `RuleMigrationComparer`**

- `bool RulesMatch(Order sample, OrderRule legacy, OrderRule modern)` — returns whether both rules yield the same bool for the sample; null rules throw

## Demo Main

1. Build max-5-lines rule via anonymous method; test order with 4 lines (pass) and 6 lines (fail).
2. Build same threshold via lambda; use `RulesMatch` on both outcomes for two sample orders.
3. Register two console notifiers (different prefixes) via anonymous method factory; call `NotifyAll` once.
4. Print a one-line migration note showing anonymous vs lambda signatures are assignable to the same `OrderRule`.

## Constraints

- net8, explicit usings, `decimal` for money fields
- `BuildMaxLineItemsRule` **must** use anonymous method syntax (not lambda)
- No LINQ required in rule evaluation loops

## Non-goals

Expression trees, dynamic rules from config files

## Evaluation

[EVALUATION.md](EVALUATION.md)
