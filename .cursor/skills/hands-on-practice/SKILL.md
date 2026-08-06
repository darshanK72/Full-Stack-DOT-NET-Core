---
name: hands-on-practice
description: >-
  Creates module-level practice under 00. Notes & Practice/01. Practice/
  {CurriculumModule}/{ReadingModule}/. Layout: 01. Scenarios.md, 02.
  DebugReading.md, 03. COVERAGE.md, Problems/ (PROBLEM.md + EVALUATION.md).
  Student .csproj lives in each Problems/ subfolder. COVERAGE maps TOPICS.md.
  Use after reading tutorials for that module.
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
        └── Problems/
            ├── MiniInventory/
            │   ├── PROBLEM.md
            │   ├── EVALUATION.md
            │   └── (student .csproj, Program.cs, …)
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
| `03. COVERAGE.md` | Topic matrix + `Problems/` index |
| `Problems/` | Build-from-scratch specs (unnumbered folder) |

Create the **.NET project in the same folder** as each problem's `PROBLEM.md` (alongside `EVALUATION.md`).

**Exactly three deliverable kinds:**

| Kind | File(s) | Student action | Verification |
|------|---------|----------------|--------------|
| **Scenarios** | `01. Scenarios.md` | Answer in plain language / pseudocode | Self-check **Answers** at file end |
| **Debug reading** | `02. DebugReading.md` | Find bug or predict output | Self-check **Answers** at file end |
| **Build projects** | `Problems/{Name}/PROBLEM.md` + `EVALUATION.md` | Create .NET project in that folder | AI rubric in EVALUATION.md |

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

---

## 03. COVERAGE.md workflow

1. Read **every** `TOPICS.md` under the reading module before writing.
2. Maintain `03. COVERAGE.md`:
   - Index of `Problems/` folders
   - Matrix: each **FULL** subtopic → covered in Scenarios, DebugReading, and/or a Problem
3. **Rules:**
   - **FULL** → ≥2 touches across the three kinds
   - **PREVIEW** → ≥1 mention in Scenarios or DebugReading
   - **DEFER** → exclude

No fixed question count — add until FULL rows are well covered.

Link to sibling files using numbered names: `[01. Scenarios.md](01.%20Scenarios.md)`.

---

## 01. Scenarios.md

Single file for the whole reading module.

**Rules:**

- Neutral question titles (`## Question 12`) — no concept names in prompts
- Real-world scenarios combining **multiple chapters**
- **`## Answers`** at **end only**
- Every important FULL subtopic in at least one question

---

## 02. DebugReading.md

Single file for the whole reading module.

**Rules:**

- Compile errors, logic bugs, wrong output
- **No `// BUG:`** in snippets
- **`## Answers`** at end — fix, error code, explanation

---

## Problems/ — build from scratch

Each subfolder = one production-style mini-system. Ship **only** `PROBLEM.md` + `EVALUATION.md` until the student adds their project in **that same folder**.

See prior skill sections for PROBLEM.md / EVALUATION.md content (domain story, entities, rubric, AI prompt).

Borrow depth from `00. Notes & Practice/04. Citi Karat Interview Practice/`. Web-search real CLI domains when drafting.

---

## Workflow — new module bank

1. Create `01. Practice/{CurriculumModule}/{ReadingModule}/`
2. Read all `TOPICS.md` → draft `03. COVERAGE.md`
3. Write `01. Scenarios.md` + `02. DebugReading.md`
4. Add `Problems/` entries (4–8+ deep specs)
5. Write `README.md` with links to numbered files
6. Re-audit `03. COVERAGE.md`

---

## Finish checklist

- [ ] Path: `01. Practice/{CurriculumModule}/{ReadingModule}/`
- [ ] Files: `README.md`, `01. Scenarios.md`, `02. DebugReading.md`, `03. COVERAGE.md`, `Problems/`
- [ ] No TypeA/B/C folders, no test harness, no StudentWork/
- [ ] Student code in `Problems/{Name}/` alongside PROBLEM + EVALUATION
- [ ] FULL subtopics ≥2 touches across the three kinds
- [ ] Folder names match reading tutorial module names exactly

See [reference.md](reference.md) for templates.
