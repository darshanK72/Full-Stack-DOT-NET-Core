# 05. Language Integrated Query — Practice

Integrated practice for the whole module — not per-chapter folders.

## Files

| File / folder | Purpose |
|---------------|---------|
| [01. Scenarios.md](01.%20Scenarios.md) | Design & predict questions (24) — **Answers** at end |
| [02. DebugReading.md](02.%20DebugReading.md) | Bug fix & output prediction (22) — **Answers** at end |
| [Problems/](Problems/) | Build-from-scratch — specs + starter projects |
| [03. COVERAGE.md](03.%20COVERAGE.md) | Topic matrix |
| [LanguageIntegratedQuery.sln](LanguageIntegratedQuery.sln) | All 9 build projects |

## Build projects

| Problem | Domain | Difficulty |
|---------|--------|------------|
| [SalesPipelineAnalyzer](Problems/SalesPipelineAnalyzer/) | Regional sales pipelines & deferred execution | Medium |
| [InventoryFilterReport](Problems/InventoryFilterReport/) | Mixed feed filtering & aggregations | Medium |
| [WarehousePickSorter](Problems/WarehousePickSorter/) | Multi-key pick sort & zone comparer | Medium |
| [ShipmentGroupSummarizer](Problems/ShipmentGroupSummarizer/) | Carrier grouping & assignee lookup | Medium |
| [OrderFulfillmentJoiner](Problems/OrderFulfillmentJoiner/) | Inner & left outer customer joins | Hard |
| [InvoiceElementFinder](Problems/InvoiceElementFinder/) | Element ops & paged invoice export | Medium |
| [ChannelCatalogSync](Problems/ChannelCatalogSync/) | Dual-channel set sync & publish gates | Medium |
| [CatalogProjectionFlattener](Problems/CatalogProjectionFlattener/) | SelectMany pick flatten & Range/Empty | Hard |
| [SkuCatalogXmlReader](Problems/SkuCatalogXmlReader/) | LINQ to XML catalog queries | Hard |

Each problem folder includes a **starter `.csproj`** and **`Program.cs`** with class shells and `// TODO:` stubs — implement the TODOs, then evaluate with that folder's `EVALUATION.md` in Cursor. Open `LanguageIntegratedQuery.sln` to build all projects at once.

## Workflow

1. Read module tutorials (`02. C# & LINQ/05. Language Integrated Query/NN. Topic/`)
2. Work **[01. Scenarios.md](01.%20Scenarios.md)** and **[02. DebugReading.md](02.%20DebugReading.md)** — self-check answers
3. Pick a **Problems/{Name}/** folder — implement TODOs — AI review with **EVALUATION.md**

## Prerequisites

Complete **03. Generics & Collections** (`IEnumerable<T>`) and **04. Functional Style Programming** (lambdas, extension methods) before starting build projects.
