---
module: 04. Functional Style Programming
difficulty: Medium
chapters: 01 Delegates
domain: OrderFulfillment
---

# Order Fulfillment Pipeline

Build a **.NET 8 console application from scratch** that routes orders through pluggable shipping rules and a multicast audit trail using custom delegates.

## Business context

An e-commerce fulfillment center charges shipping by weight and destination zone. Operations logs every pricing decision to multiple audit sinks (console, file buffer simulation). Supervisors swap shipping algorithms at runtime without recompiling the pipeline.

## Definitions

**Delegate `ShippingRule`**

```csharp
public delegate decimal ShippingRule(decimal weightKg, string zone);
```

**Delegate `OrderAuditHandler`**

```csharp
public delegate void OrderAuditHandler(string message);
```

**Record `ShipmentRequest`**

- `OrderId` (string, non-empty)
- `WeightKg` (decimal > 0)
- `Zone` (string, non-empty) — e.g. `"Domestic"`, `"International"`
- `ServiceLevel` (string) — `"Standard"`, `"Express"`, or `"Economy"`

**Static class `ShippingRules`** — three methods matching `ShippingRule`:

| Method | Behavior |
|--------|----------|
| `StandardRate` | `weightKg * 2.50m` when zone is `"Domestic"`; otherwise `weightKg * 5.00m` |
| `ExpressRate` | `StandardRate(weightKg, zone) * 1.75m` (call `StandardRate` as method group) |
| `EconomyRate` | `weightKg * 1.25m` domestic; `weightKg * 3.50m` otherwise |

**Static class `ShippingRuleSelector`**

- `ShippingRule? SelectShippingRule(string serviceLevel)` — case-insensitive match to the three methods above; return `null` for unknown levels

**Class `OrderPipeline`**

- Field holding multicast `OrderAuditHandler? _auditChain`
- `void RegisterAudit(OrderAuditHandler handler)` — reject null; append with `+=`
- `void UnregisterAudit(OrderAuditHandler handler)` — remove with `-=` (no-op if not present)
- `OrderAuditHandler CombineAudits(OrderAuditHandler first, OrderAuditHandler second)` — use `Delegate.Combine` cast back to `OrderAuditHandler`; reject null arguments
- `OrderAuditHandler? RemoveAudit(OrderAuditHandler chain, OrderAuditHandler toRemove)` — use `Delegate.Remove` cast back; return null if chain becomes empty
- `decimal QuoteShipping(ShipmentRequest request, ShippingRule rule)` — validate request (throw `ArgumentNullException` / `ArgumentException`); **null-safe invoke** audit chain with message `"Quoting {OrderId}: {weight}kg {zone} {service}"`; invoke `rule`; audit result `"Quoted {OrderId}: {cost:C}"`; return cost
- `void RaiseAudit(string message)` — public helper that null-safe invokes `_auditChain` (for demo of `?.Invoke`)

## Demo Main

1. Build two audit handlers (e.g. prefix `"[AUDIT]"` and `"[LOG]"`) and combine with `CombineAudits`, assign to pipeline via `RegisterAudit`.
2. Quote two shipments with different `SelectShippingRule` results; print costs.
3. `UnregisterAudit` one handler; quote again to show multicast shrink.
4. Call `QuoteShipping` with a null `ShippingRule` to demonstrate null-safe rule invoke returns `0` without throwing (use `rule?.Invoke(...) ?? 0m` pattern in your implementation).

## Constraints

- net8, explicit usings, `decimal` for money
- Custom delegate types at namespace scope (not `Func`/`Action` for shipping/audit)
- No LINQ required

## Non-goals

Real file I/O, async, events keyword

## Evaluation

[EVALUATION.md](EVALUATION.md)
