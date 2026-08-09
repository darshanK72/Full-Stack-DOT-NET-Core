# 06. Multithreading & Async Programming — Practice

Integrated practice for the whole module — not per-chapter folders.

## Files

| File / folder | Purpose |
|---------------|---------|
| [01. Scenarios.md](01.%20Scenarios.md) | Design & predict questions (22) — **Answers** at end |
| [02. DebugReading.md](02.%20DebugReading.md) | Bug fix & output prediction (22) — **Answers** at end |
| [Problems/](Problems/) | Build-from-scratch — `PROBLEM.md` + `EVALUATION.md` + starter project |
| [03. COVERAGE.md](03.%20COVERAGE.md) | Topic matrix |
| [MultithreadingAndAsyncProgramming.sln](MultithreadingAndAsyncProgramming.sln) | All 7 build projects in one solution |

## Build projects

| Problem | Domain | Difficulty |
|---------|--------|------------|
| [ShipmentScanStation](Problems/ShipmentScanStation/) | Logistics scan threads | Medium |
| [AuditLineProcessor](Problems/AuditLineProcessor/) | Invoice batch thread pool | Medium |
| [WarehouseOrderCoordinator](Problems/WarehouseOrderCoordinator/) | Order Task pipeline | Hard |
| [AnalyticsReportExporter](Problems/AnalyticsReportExporter/) | Async report export | Hard |
| [SkuReconciliationBatch](Problems/SkuReconciliationBatch/) | Parallel SKU reconciliation | Medium |
| [CommunityBankLedger](Problems/CommunityBankLedger/) | Bank locks & transfers | Medium |
| [PickTicketBuffer](Problems/PickTicketBuffer/) | Concurrent pick pipeline | Hard |

Each problem folder includes a **starter `.csproj`** and **`Program.cs`** with class shells and `// TODO:` stubs — implement the TODOs, then evaluate with that folder's `EVALUATION.md` in Cursor. Open `MultithreadingAndAsyncProgramming.sln` to build all projects at once.

## Workflow

1. Read module tutorials (`02. C# & LINQ/06. Multithreading & Async Programming/NN. Topic/`)
2. Work **[01. Scenarios.md](01.%20Scenarios.md)** and **[02. DebugReading.md](02.%20DebugReading.md)** — self-check answers
3. Pick a **Problems/{Name}/** folder — implement TODOs in `Program.cs` — AI review with **EVALUATION.md**

## Suggested order

Threads → ThreadPool → Tasks → Async → Parallel → Locks → Concurrent Collections. Problems follow that chapter order, but cross-chapter scenario questions can be attempted after reading the first four chapters.
