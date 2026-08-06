# 00. Notes & Practice

Practice materials and legacy reference code for this learning repository.

## Active curriculum practice

| Folder | Description |
|--------|-------------|
| **[01. Practice/](01.%20Practice/)** | Module banks aligned with reading tutorials |

**Path pattern:**

```
01. Practice/{CurriculumModule}/{ReadingModule}/
├── README.md
├── 01. Scenarios.md
├── 02. DebugReading.md
├── 03. COVERAGE.md
└── Problems/{Name}/PROBLEM.md + EVALUATION.md
```

**Example:** `01. Practice/01. C# & LINQ/01. C# Language Fundamentals/`

Students create each `.csproj` **inside** the matching `Problems/{Name}/` folder.

## Legacy reference (style only)

| Folder | Description |
|--------|-------------|
| 02. Capgemini Exam Practice | M1 coding problems |
| 03. Hackerrank Practice Questions | Interface + LINQ-style projects |
| 04. Citi Karat Interview Practice | Multi-task manager domains |

## For agents

- **Reading tutorials:** `{NN. Curriculum}/{NN. Reading Module}/NN. Topic/` — `@reading-tutorial`
- **Hands-on practice:** `01. Practice/{same curriculum}/{same reading module}/` — `@hands-on-practice`
- **Do not** create per-topic practice under `{Module}/00. Practice/NN. Topic/` — deprecated
