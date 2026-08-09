# 09. Unit Testing — Practice

Integrated practice for the whole module — not per-chapter folders.

## Files

| File / folder | Purpose |
|---------------|---------|
| [01. Scenarios.md](01.%20Scenarios.md) | Design & predict questions (20) — **Answers** at end |
| [02. DebugReading.md](02.%20DebugReading.md) | Bug fix & output prediction (18) — **Answers** at end |
| [Problems/](Problems/) | Build SUT + tests — `PROBLEM.md` + `EVALUATION.md` + starter projects |
| [03. COVERAGE.md](03.%20COVERAGE.md) | Topic matrix |
| [UnitTesting.sln](UnitTesting.sln) | All 12 projects (6 production + 6 test) |

## Build projects

| Problem | Domain | Framework focus | Difficulty |
|---------|--------|-----------------|------------|
| [ExamScoreValidator](Problems/ExamScoreValidator/) | Academic grading | xUnit [Fact], AAA | Medium |
| [FreightRateCalculator](Problems/FreightRateCalculator/) | Logistics quoting | [Theory], MemberData, ITestOutputHelper | Medium |
| [MemberTierDiscounts](Problems/MemberTierDiscounts/) | Retail loyalty | MSTest lifecycle + DataRow | Medium |
| [HotelReservationNotifier](Problems/HotelReservationNotifier/) | Hotel bookings | Manual stub + fake | Hard |
| [PaymentCaptureGateway](Problems/PaymentCaptureGateway/) | Payment capture | Moq Setup / Verify / Callback | Hard |
| [WarehousePickCatalog](Problems/WarehousePickCatalog/) | Warehouse picks | IClassFixture + ICollectionFixture | Medium |

Each problem folder includes a **production `.csproj`**, a **companion test project**, and **`Program.cs`** (SUT scaffold) plus test files with `// TODO:` stubs. Implement production TODOs first, then tests, then run:

```powershell
dotnet test Problems/ExamScoreValidator/ExamScoreValidator.Tests
```

Open `UnitTesting.sln` to build all projects at once.

## Workflow

1. Read module tutorials (`02. C# & LINQ/09. Unit Testing/NN. Topic/`)
2. Work **[01. Scenarios.md](01.%20Scenarios.md)** and **[02. DebugReading.md](02.%20DebugReading.md)** — self-check answers
3. Pick a **Problems/{Name}/** folder — implement SUT + tests — AI review with **EVALUATION.md**
