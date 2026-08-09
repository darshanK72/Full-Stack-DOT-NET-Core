# 04. Functional Style Programming — Practice

Integrated practice for the whole module — not per-chapter folders.

## Files

| File / folder | Purpose |
|---------------|---------|
| [01. Scenarios.md](01.%20Scenarios.md) | Design & predict questions (24) — **Answers** at end |
| [02. DebugReading.md](02.%20DebugReading.md) | Bug fix & output prediction (22) — **Answers** at end |
| [Problems/](Problems/) | Build-from-scratch — specs + starter projects |
| [03. COVERAGE.md](03.%20COVERAGE.md) | Topic matrix |
| [FunctionalStyleProgramming.sln](FunctionalStyleProgramming.sln) | All 7 build projects |

## Build projects

| Problem | Domain | Difficulty |
|---------|--------|------------|
| [OrderFulfillmentPipeline](Problems/OrderFulfillmentPipeline/) | Warehouse shipping & audit delegates | Medium |
| [CatalogPricingEngine](Problems/CatalogPricingEngine/) | SKU pricing transforms (lambdas) | Medium |
| [LegacyRuleMigrator](Problems/LegacyRuleMigrator/) | Order validation (anonymous → lambda) | Medium |
| [OrderReceiptExtensions](Problems/OrderReceiptExtensions/) | Receipt formatting extensions | Medium |
| [WarehousePickProcessor](Problems/WarehousePickProcessor/) | Inventory Func/Action/Predicate | Hard |
| [MarginRuleFactory](Problems/MarginRuleFactory/) | Closure-based discount factories | Hard |
| [CallbackOrchestrator](Problems/CallbackOrchestrator/) | Mixed delegates + built-in callbacks | Hard |

Each problem folder includes a **starter `.csproj`** and **`Program.cs`** with class shells and `// TODO:` stubs — implement the TODOs, then evaluate with that folder's `EVALUATION.md` in Cursor. Open `FunctionalStyleProgramming.sln` to build all projects at once.

## Workflow

1. Read module tutorials (`02. C# & LINQ/04. Functional Style Programming/NN. Topic/`)
2. Work **[01. Scenarios.md](01.%20Scenarios.md)** and **[02. DebugReading.md](02.%20DebugReading.md)** — self-check answers
3. Pick a **Problems/{Name}/** folder — implement TODOs — AI review with **EVALUATION.md**
