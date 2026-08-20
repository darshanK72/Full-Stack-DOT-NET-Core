---
name: interview-answers
description: >-
  Writes clear, memorable interview answers for INTERVIEW_QUESTIONS.md banks in
  this repo. Output is INTERVIEW_ANSWERS.md with a complete opening Answer line
  and explanatory bullets. Use when the user asks to answer interview questions,
  create INTERVIEW_ANSWERS, or @INTERVIEW_QUESTIONS.md with this skill.
disable-model-invocation: true
---

# Interview Answers Skill

Write **readable, self-explanatory answers** for questions in a module's `INTERVIEW_QUESTIONS.md`. The reader should understand the full concept without guessing missing context — not memorize a script.

Pair with: `@INTERVIEW_QUESTIONS.md` + reading chapters in the same module folder for accuracy.

---

## Trigger

Use when the user asks to **write answers**, references `@interview-answers`, or names a chapter/range from an `INTERVIEW_QUESTIONS.md` file.

---

## Inputs

1. **`INTERVIEW_QUESTIONS.md`** — question numbering and chapter headings (source of truth).
2. **Reading material** in the same module (`.md` chapters, tutorials) — match repo terminology.
3. **User scope** — whole file, one chapter, or question range.

---

## Output

| Questions file | Answers file |
|----------------|--------------|
| `01. .NET Framework Architecture/INTERVIEW_QUESTIONS.md` | `INTERVIEW_ANSWERS.md` (same folder) |
| `02. C# Language Fundamentals/INTERVIEW_QUESTIONS.md` | `INTERVIEW_ANSWERS.md` (same folder) |

Mirror chapter headings exactly. For partial scope, append or replace only that chapter section.

---

## Answer style (required)

### Opening `**Answer:**` line

- Still **one block** labeled `**Answer:**` — usually **1–3 complete sentences**, not a telegram.
- Must **stand alone**: a reader who only reads this line grasps the core idea without reading the bullets.
- **No shortcuts**: spell out or briefly define acronyms on first use in that answer (e.g. "Common Language Runtime (CLR)" not bare "CLR" alone).
- **No implicit jumps**: if the question asks "what" or "why", the opening line states it explicitly — do not leave the reader to infer from jargon.
- Avoid fragment-style answers ("Windows-only, monolithic, maintenance mode.") — use **full sentences** with subject and verb.

**Weak opening:** `.NET Core was a cross-platform rewrite with modular packages.`

**Strong opening:** `.NET Core was a ground-up rewrite of the .NET runtime and libraries, designed so applications could run on Windows, Linux, and macOS using modular NuGet packages instead of a single machine-wide Windows install.`

### Supporting bullets

- Use **3–5 bullets** for most questions (2 minimum when the opening line already covers everything).
- Each bullet is **one or two full sentences** that explain *what*, *how*, or *why* — not a label or phrase.
- Bullets should **add detail** the opening line did not cover: mechanism, contrast, historical context, or consequence.
- Use a **comparison table** when contrasting two or more things — add a closing sentence that states the takeaway.

**Weak bullet:** `- Introduced Kestrel and dotnet CLI.`

**Strong bullet:** `- It introduced Kestrel as a cross-platform web server and the dotnet command-line interface, so web apps no longer depended on IIS and System.Web on Windows only.`

### General rules

- Plain, direct English — no interview coaching, tips, or motivational filler.
- Minimal code (3–8 lines) only when it clarifies; no large blocks.
- Exact curriculum terms (CLI, CoreCLR, TFM, BCL).
- Mark historical vs modern (.NET Framework GAC vs modern private deploy).
- Cross-reference adjacent questions briefly instead of repeating (`See Q4.`) when overlap exists.
- Self-contained — readable without other answers.

### Do not

- "Pro tips", "in interviews say…", "interviewer expects…"
- Telegraphic shorthand, unexplained acronyms, or answers that assume prior answers
- Duplicate long explanations across adjacent questions
- Large code samples or external doc links unless asked

---

## Per-answer template

```markdown
#### Q1. [Repeat question text exactly]

**Answer:** [1–3 complete sentences. Fully states the core idea. No implicit gaps.]

- [Full-sentence bullet explaining one aspect, mechanism, or consequence.]
- [Full-sentence bullet — add context, contrast, or "why it matters".]
- [Optional third–fifth bullet or short closing paragraph.]

[Optional: tiny code/table — only if it teaches.]
```

**Gotcha template:**

```markdown
#### Gotcha N. [Title from question bank]

**Answer:** [1–2 sentences: what people confuse, then the correct model.]

- [Sentence explaining why the wrong mental model fails.]
- [Sentence with a concrete example or consequence.]
```

---

## Length guide

| Question type | Opening `Answer:` | Bullets / body |
|---------------|-------------------|----------------|
| Definition | 2–3 sentences | 3–4 explanatory bullets |
| Difference | 2 sentences + table | 1–2 sentences closing takeaway |
| Evolution / flow | 2 sentences summary | 5–8 numbered steps, each a full sentence |
| Why / when | 2–3 sentences (problem + outcome) | 2–3 bullets on impact today |
| Gotcha | 2 sentences | 2–3 explanatory bullets |

**Total per question:** roughly **150–280 words** for standard items; **up to 350** for multi-part evolution or comparison questions. Prefer clarity over brevity.

---

## File header

```markdown
# [Module Name] — Interview Answers

Answers for [INTERVIEW_QUESTIONS.md](./INTERVIEW_QUESTIONS.md).  
Written for clarity and recall — concepts only, no interview coaching.

> **Scope:** [e.g. Chapter 01 only | All chapters]

---
```

When updating an existing file, merge sections — do not delete unrelated chapters.

---

## Workflow

1. Confirm scope from user message.
2. Read questions + matching reading chapter(s).
3. Create or open `INTERVIEW_ANSWERS.md`.
4. Write answers in order; preserve numbers and headings.
5. Skip appendices unless they contain numbered questions.
6. Self-check (see below).

---

## Self-check before finishing

- [ ] Every `**Answer:**` is complete prose — not fragments or shorthand
- [ ] Acronyms defined or obvious from full term in the same sentence
- [ ] Each bullet adds explanation, not just a keyword list
- [ ] No interview tips or "say this in the interview"
- [ ] Code ≤ 8 lines unless user asked for more
- [ ] Historical vs modern .NET labeled where relevant

---

## Accuracy rules

- **Unified .NET:** `.NET 5+` is the current product name; `.NET Core` is historical (3.1 and earlier).
- **.NET Framework 4.8:** maintenance mode — security/reliability fixes, not new features.
- **GAC / AppDomain / CAS:** historical .NET Framework — not the modern default.
- Prefer facts from the matching `.md` chapter in the same module folder.

---

## Examples

See [examples.md](examples.md) for full sample answers in the required style.

---

## User invocation

```
@interview-answers @01. .NET Framework Architecture/INTERVIEW_QUESTIONS.md
Write answers for Chapter 02 only.
```
