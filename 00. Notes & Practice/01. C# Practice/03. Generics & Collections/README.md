# 03. Generics & Collections — Practice

Integrated practice for the whole module — not per-chapter folders.

## Files

| File / folder | Purpose |
|---------------|---------|
| [01. Scenarios.md](01.%20Scenarios.md) | Design & predict questions (22) — **Answers** at end |
| [02. DebugReading.md](02.%20DebugReading.md) | Bug fix & output prediction (20) — **Answers** at end |
| [Problems/](Problems/) | Build-from-scratch — `PROBLEM.md` + `EVALUATION.md` + starter project |
| [03. COVERAGE.md](03.%20COVERAGE.md) | Topic matrix |
| [GenericsAndCollections.sln](GenericsAndCollections.sln) | All 8 build projects in one solution |

## Build projects

| Problem | Domain | Difficulty |
|---------|--------|------------|
| [GenericRepository](Problems/GenericRepository/) | Typed in-memory store | Medium |
| [LegacyInventoryMigrator](Problems/LegacyInventoryMigrator/) | ArrayList → generic migration | Medium |
| [ShipmentPlanner](Problems/ShipmentPlanner/) | Shipment queue & sorting | Medium |
| [SkuCatalogRegistry](Problems/SkuCatalogRegistry/) | SKU dictionary catalog | Medium |
| [ArticleTagManager](Problems/ArticleTagManager/) | Blog tag dedup & set ops | Medium |
| [WarehousePickQueue](Problems/WarehousePickQueue/) | FIFO picks + LIFO undo | Hard |
| [ReorderReport](Problems/ReorderReport/) | Sorted reorder map | Medium |
| [PickTicketEnumerator](Problems/PickTicketEnumerator/) | Custom pick-ticket walk | Hard |

Each problem folder includes a **starter `.csproj`** and **`Program.cs`** with class shells and `// TODO:` stubs — implement the TODOs, then evaluate with that folder's `EVALUATION.md` in Cursor. Open `GenericsAndCollections.sln` to build all projects at once.

## Workflow

1. Read module tutorials (`02. C# & LINQ/03. Generics & Collections/NN. Topic/`)
2. Work **[01. Scenarios.md](01.%20Scenarios.md)** and **[02. DebugReading.md](02.%20DebugReading.md)** — self-check answers
3. Pick a **Problems/{Name}/** folder — build from zero in that folder — AI review with **EVALUATION.md**
