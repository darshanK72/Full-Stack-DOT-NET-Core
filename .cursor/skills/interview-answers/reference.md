# Interview Answers — Reference

## Output file structure (full module)

```markdown
# .NET Framework Architecture — Interview Answers

Answers for [INTERVIEW_QUESTIONS.md](./INTERVIEW_QUESTIONS.md).
Written for clarity and recall — concepts only, no interview coaching.

> **Scope:** All chapters

---

## Chapter 01. .NET Platform & Evolution

#### Q1. What is .NET (the platform as a whole)?

**Answer:** ...

#### Q2. What is the .NET Framework?

**Answer:** ...

---

## Chapter 02. CLI, Architecture & Components

...

---

## Gotchas — .NET Architecture (Interview Traps)

#### Gotcha 1. ".NET Framework" vs ".NET" naming

**Answer:** ...
```

---

## Incremental generation

| User request | Action |
|--------------|--------|
| "Chapter 01 only" | Write `## Chapter 01` section; create file or replace that section only |
| "Q1–15" | Answer that range under the correct chapter heading |
| "Continue from Q16" | Read existing `INTERVIEW_ANSWERS.md`; append from Q16 |
| "All chapters" | Full file; work chapter-by-chapter in one session or multiple |

---

## Formatting conventions

- **Bold** only for `**Answer:**` label and key terms on first use in an answer.
- Tables: max 4–5 rows for interview recall.
- Diagrams: ASCII one-liners OK (`Source → IL → JIT → Native`); avoid mermaid unless user asks.
- Lists: `-` bullets; numbered lists for sequences only.
- No emoji in answers.

---

## Module folder map (this repo)

| Questions file | Reading sources | Answers file |
|----------------|-----------------|--------------|
| `01. .NET Framework Architecture/INTERVIEW_QUESTIONS.md` | `01.`–`10.` `.md` chapters | `INTERVIEW_ANSWERS.md` |
| `02. C# Language Fundamentals/INTERVIEW_QUESTIONS.md` | `02. …/` tutorial folders | `INTERVIEW_ANSWERS.md` |

**Layer 2 (folder-scoped):** `@karat-interview-answers` — per-folder `KARAT_INTERVIEW_*.md`; index at `00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_INDEX.md`; debrief source `KARAT_DEBRIEF_SOURCE.txt`.

Future modules: same pattern — `INTERVIEW_ANSWERS.md` beside `INTERVIEW_QUESTIONS.md`.

---

## Quality checklist (agent self-check)

- [ ] Every numbered question in scope has an answer
- [ ] Question text matches bank exactly
- [ ] `**Answer:**` is 1–3 **complete sentences** — not fragments or keyword lists
- [ ] Acronyms spelled out or defined on first use in each answer
- [ ] Each bullet is **1–2 full sentences** that explain, not label
- [ ] No "tip", "remember in interview", "interviewer expects"
- [ ] Code ≤ 8 lines unless user asked for more
- [ ] Historical features labeled (.NET Framework vs modern .NET)
- [ ] No duplicate essays for overlapping questions
- [ ] Roughly 150–280 words per standard question unless scope is intentionally brief
