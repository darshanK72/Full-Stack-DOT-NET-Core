# Hands-On Practice — Reference

Companion to [SKILL.md](SKILL.md).

---

## Module layout

```
00. Notes & Practice/01. Practice/
└── 01. C# & LINQ/
    └── 01. C# Language Fundamentals/
        ├── README.md
        ├── 01. Scenarios.md
        ├── 02. DebugReading.md
        ├── 03. COVERAGE.md
        └── Problems/
            ├── MiniInventory/
            │   ├── PROBLEM.md
            │   ├── EVALUATION.md
            │   └── (student .csproj + code)
            └── ClinicAppointmentManager/
                └── ...
```

**Path rule:** `01. Practice/{CurriculumModule}/{ReadingModule}/` — names must match the reading tutorial tree.

---

## Numbered root files

| # | Filename | Role |
|---|----------|------|
| — | `README.md` | Bank index and workflow |
| 01 | `01. Scenarios.md` | Design / predict / plan questions |
| 02 | `02. DebugReading.md` | Bug find / output predict |
| 03 | `03. COVERAGE.md` | TOPICS.md matrix + Problems index |
| — | `Problems/` | Build projects (folder, not numbered) |

---

## 01. Scenarios.md skeleton

```markdown
# {Reading Module Name} — Scenarios

Answer without running code unless asked. Check **Answers** at the end.

---

## Question 1

{Neutral scenario — multi-step, real domain}

---

## Answers

### Question 1
{Full answer with chapter concepts}
```

---

## 02. DebugReading.md skeleton

```markdown
# {Reading Module Name} — Debug Reading

Find the bug or predict output. Check **Answers** at the end.

---

## Question 1

​```csharp
// snippet
​```

---

## Answers

### Question 1
{Fix + CS#### + explanation}
```

---

## PROBLEM.md / EVALUATION.md

Unchanged: front matter, domain story, entities, requirements, rubric /100, AI prompt, checklist.

Student creates `.csproj` in `Problems/{Name}/` next to PROBLEM.md.

---

## 03. COVERAGE.md excerpt

```markdown
| Subtopic | Ch | Scenarios | Debug | Problems |
|----------|-----|-----------|-------|----------|
| Integer division | 04 | Q8 | Q5 | MiniInventory |
```

Link files: `[01. Scenarios.md](01.%20Scenarios.md)`.

---

## Legacy style sources

- `00. Notes & Practice/04. Citi Karat Interview Practice/`
- `00. Notes & Practice/02. Capgemini Exam Practice/`

Reference only — not syllabus scope.
