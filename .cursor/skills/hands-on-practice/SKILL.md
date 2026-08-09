---
name: hands-on-practice
description: >-
  Creates module-level practice under 00. Notes & Practice/01. Practice/
  {CurriculumModule}/{ReadingModule}/. Layout: 01. Scenarios.md, 02.
  DebugReading.md, 03. COVERAGE.md, Problems/ (PROBLEM.md + EVALUATION.md +
  starter .csproj + commented Program.cs scaffold). Bank-level .sln links all
  build projects. COVERAGE maps reading chapters. Use after reading tutorials.
---

# Hands-On Practice Skill

Use **after** reading tutorials exist for a module. One **integrated bank per reading module** — multi-topic, production-style.

Pair with: `@reading-tutorial` (learn) → `@hands-on-practice` (apply).

---

## Module bank layout (only this shape)

Mirror the **reading tutorial path**: curriculum module folder, then reading module folder.

```
00. Notes & Practice/01. Practice/
└── 01. C# & LINQ/                          ← same name as curriculum module folder
    └── 01. C# Language Fundamentals/       ← same name as reading module folder
        ├── README.md
        ├── 01. Scenarios.md
        ├── 02. DebugReading.md
        ├── 03. COVERAGE.md
        ├── LanguageFundamentals.sln          ← all Problems/ projects linked
        └── Problems/
            ├── MiniInventory/
            │   ├── PROBLEM.md
            │   ├── EVALUATION.md
            │   ├── MiniInventory.csproj
            │   └── Program.cs                  ← commented scaffold; student fills TODOs
            └── ...
```

**Full path pattern:**

```
00. Notes & Practice/01. Practice/{NN. Curriculum Module}/{NN. Reading Module Name}/
```

Examples:

| Reading tutorials live at | Practice bank lives at |
|---------------------------|-------------------------|
| `02. C# & LINQ/01. C# Language Fundamentals/` | `01. Practice/01. C# & LINQ/01. C# Language Fundamentals/` |
| `02. ASP.NET Core/01. Minimal APIs/` (future) | `01. Practice/02. ASP.NET Core/01. Minimal APIs/` |

**Numbered root files (fixed order):**

| File | Purpose |
|------|---------|
| `README.md` | Index, problem list, workflow (unnumbered) |
| `01. Scenarios.md` | All scenario/design questions + **Answers** at end |
| `02. DebugReading.md` | All debug/snippet questions + **Answers** at end |
| `03. COVERAGE.md` | Reading chapter matrix + `Problems/` index |
| `Problems/` | Build-from-scratch specs (unnumbered folder) |

Each `Problems/{Name}/` folder ships a **complete starter project** alongside the specs:

| File | Role |
|------|------|
| `PROBLEM.md` | Requirements spec |
| `EVALUATION.md` | AI rubric /100 |
| `{Name}.csproj` | .NET 8 console project (see [reference.md](reference.md)) |
| `Program.cs` | Commented scaffold — student implements `// TODO:` sections |

Add a **bank-level `.sln`** (e.g. `LanguageFundamentals.sln`, `ObjectOrientedProgramming.sln`) linking every problem project. Verify `dotnet build` on the solution succeeds (warnings for unimplemented members are OK).

**Exactly three deliverable kinds:**

| Kind | File(s) | Student action | Verification |
|------|---------|----------------|--------------|
| **Scenarios** | `01. Scenarios.md` | Answer in plain language / pseudocode | Self-check **Answers** at file end |
| **Debug reading** | `02. DebugReading.md` | Find bug or predict output | Self-check **Answers** at file end |
| **Build projects** | `Problems/{Name}/` full folder above | Implement `Program.cs` TODOs | AI rubric in EVALUATION.md |

**Do not** create: `TypeA-*` folders, per-question markdown files, stub test harnesses, separate `StudentWork/` tree, or banks directly under `01. Practice/{ModuleName}/` without the curriculum parent folder.

**Do not** label content as "Type A/B/C" in filenames or headings.

**Do not** use unnumbered `Scenarios.md` / `COVERAGE.md` at the bank root — use **`01.` / `02.` / `03.`** prefixes.

---

## When to apply

- User @-mentions this skill or asks for practice / problem bank
- User points at a reading module (e.g. `01. C# Language Fundamentals` under `02. C# & LINQ`)
- Extending scenario/debug questions or adding a `Problems/` entry

Per-topic practice under `{Module}/00. Practice/NN. Topic/` is **deprecated**.

---

## Naming alignment

| Artifact | Convention |
|----------|------------|
| Curriculum folder under `01. Practice/` | **Same** as repo curriculum module (`01. C# & LINQ`, not `CSharpAndLinq`) |
| Reading module folder | **Same** as reading module folder name (character-for-character) |
| Scenario file | `01. Scenarios.md` |
| Debug file | `02. DebugReading.md` |
| Coverage file | `03. COVERAGE.md` |
| Problem folder | `Problems/{PascalCaseDomain}/` e.g. `MiniInventory`, `ClinicAppointmentManager` |
| Problem spec | `PROBLEM.md` + `EVALUATION.md` (uppercase, fixed names) |
| Starter project | `{Name}.csproj` + `Program.cs` (same `{Name}` as folder) |
| Bank solution | `{ModuleShortName}.sln` at reading-module practice root |

---

## 03. COVERAGE.md workflow

1. Read **every chapter folder** under the reading module (`Program.cs`, quick reference, module README) before writing.
2. Maintain `03. COVERAGE.md`:
   - Index of `Problems/` folders
   - Matrix: each important subtopic from the reading chapters → covered in Scenarios, DebugReading, and/or a Problem
3. **Rules:**
   - Core chapter concepts → ≥2 touches across the three kinds
   - Preview-only concepts (forward-ref in tutorials) → ≥1 mention in Scenarios or DebugReading
   - Out-of-module concepts → exclude

No fixed question count — add until core chapter concepts are well covered.

Link to sibling files using numbered names: `[01. Scenarios.md](01.%20Scenarios.md)`.

---

## 01. Scenarios.md

Single file for the whole reading module.

**Rules:**

- Neutral question titles (`## Question 12`) — no concept names in prompts
- Real-world scenarios combining **multiple chapters**
- **`## Answers`** at **end only**
- Every important chapter concept in at least one question

---

## 02. DebugReading.md

Single file for the whole reading module.

**Rules:**

- Compile errors, logic bugs, wrong output
- **No `// BUG:`** in snippets
- **`## Answers`** at end — fix, error code, explanation

---

## Problems/ — build from scratch

Each subfolder = one production-style mini-system. Ship **PROBLEM.md**, **EVALUATION.md**, **`.csproj`**, and a **commented `Program.cs` scaffold** in the same folder.

Borrow depth from `00. Notes & Practice/04. Citi Karat Interview Practice/`. Web-search real CLI domains when drafting.

Reference implementations for scaffold style:

- `01. Practice/01. C# & LINQ/01. C# Language Fundamentals/Problems/MiniInventory/Program.cs`
- `01. Practice/01. C# & LINQ/02. Object Oriented Programming/Problems/ClinicBillingRegistry/Program.cs`

---

## Program.cs scaffold rules

The scaffold is **not** a solution — it is a guided skeleton the student completes. Match `@reading-tutorial` comment density.

### File header (required)

Multiline `/* … */` block at top:

1. `PROBLEM:` title (matches `PROBLEM.md` heading)
2. 2–4 sentences — business context from the spec
3. `This exercise covers:` — bullet list of reading chapters/concepts (e.g. `ch02 — decimal for money`)

### Per-type comments

Above every `class`, `struct`, `enum`, and `interface`:

- One-line role in the domain
- Non-obvious constraints from `PROBLEM.md` (e.g. "No Console calls in this class", "only creatable via factory")

### Per-member comments

Above every method, constructor, property with logic, and indexer:

- What it does in plain language
- Validation rules and exceptions to throw
- Return semantics (`true`/`false`, `out` params, empty collections)

Keep signatures and member names **identical** to `PROBLEM.md`. Do not implement business logic — use:

```csharp
// TODO: {concise instruction mirroring PROBLEM.md requirement}
throw new NotImplementedException();
```

### Compile-safe stubs

- **Constructors:** assign parameters to fields/properties so readonly members compile; leave validation as `// TODO:` comments (do not throw in ctor unless spec requires it)
- **Auto-properties / simple getters:** may be fully declared with no TODO
- **`Main`:** `// TODO:` steps for the demo section in `PROBLEM.md`, then `throw new NotImplementedException();`
- **No solution code** in comments beyond guidance — student implements

### Project settings (every `.csproj`)

- `net8.0`, `OutputType` Exe
- `ImplicitUsings` **disable**, `Nullable` **enable**
- Explicit `using` statements in `Program.cs` (no global usings file)

### Namespace

Pick a domain namespace (e.g. `RetailInventory`, `HealthcareBilling`) — PascalCase, not the folder name unless they match naturally.

---

## Workflow — new module bank

1. Create `01. Practice/{CurriculumModule}/{ReadingModule}/`
2. Read all reading chapter `Program.cs` files → draft `03. COVERAGE.md`
3. Write `01. Scenarios.md` + `02. DebugReading.md`
4. Add `Problems/` entries (4–8+ deep specs): `PROBLEM.md`, `EVALUATION.md`, `.csproj`, commented `Program.cs`
5. Create `{ReadingModuleShortName}.sln` and `dotnet sln add` every problem project
6. Run `dotnet build` on the solution — fix compile errors (NotImplementedException at runtime is OK)
7. Write `README.md` with links to numbered files, solution, and scaffold workflow
8. Re-audit `03. COVERAGE.md`

When **extending** an existing bank with a new problem, add the four project files and update the `.sln`, README, and COVERAGE index.

---

## Finish checklist

- [ ] Path: `01. Practice/{CurriculumModule}/{ReadingModule}/`
- [ ] Files: `README.md`, `01. Scenarios.md`, `02. DebugReading.md`, `03. COVERAGE.md`, `{Module}.sln`, `Problems/`
- [ ] Each problem: `PROBLEM.md`, `EVALUATION.md`, `{Name}.csproj`, `Program.cs` (header + type + member comments, TODO stubs)
- [ ] `dotnet build` succeeds on the bank solution
- [ ] No TypeA/B/C folders, no test harness, no StudentWork/
- [ ] Core chapter concepts ≥2 touches across the three kinds
- [ ] Folder names match reading tutorial module names exactly

See [reference.md](reference.md) for templates.
