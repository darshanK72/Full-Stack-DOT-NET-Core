---
name: karat-interview-answers
description: >-
  Creates folder-scoped Karat-style production interview Q&A from user-provided
  curriculum paths (module/subfolder tutorials). Tests judgment via code review,
  hosting, DI, async traps, and API design — not foundation recall. Writes
  KARAT_INTERVIEW_QUESTIONS.md and KARAT_INTERVIEW_ANSWERS.md beside the target
  folder. Use for Karat, Citi, production-readiness, or applied code-review prep.
disable-model-invocation: true
---

# Karat Interview Answers Skill

Write **applied, production-readiness** interview Q&A scoped to **whatever folder the user provides**. Do not use a fixed global topic list — derive questions from that folder's name, sibling tutorials, `Program.cs`, and reading material.

Foundation recall stays in `@interview-answers` + `INTERVIEW_QUESTIONS.md` banks. This skill is **Layer 2** (judgment). Layer 3 is timed live coding in practice folders.

---

## Three layers (repo prep model)

| Layer | Where | Primary goal | Question shape |
|---|---|---|---|
| **1 — Foundations** | `01.` / `02.` `INTERVIEW_QUESTIONS.md` | Coverage — know platform & language | "What is X?", "Difference A vs B?" |
| **2 — Production (this skill)** | User-named folder → `KARAT_INTERVIEW_*.md` | Judgment — ship & debug real apps | "What's wrong?", "How fix?", "What breaks in prod?" |
| **3 — Live execution** | Karat practice `.cs` specs + tests | Spec reading + implement + explain aloud | Edge cases, partitions, boundary rules |

**Level for Layer 2:** mid → senior **applied** .NET / ASP.NET Core — not "define middleware" but "middleware fails behind nginx because forwarded headers are wrong."

---

## How Layer 2 differs from `@interview-answers`

| Foundation banks | This skill |
|---|---|
| One concept per question | Often **syntax + runtime + design** in one snippet |
| Named gotcha + one-line setup | **8–20 lines** of almost-working code to read |
| Correct definitions expected | **Diagnosis, prioritized fixes, trade-offs** |
| Junior → mid recall | Mid → senior production engineering |

**Why Karat feels harder than foundation gotchas:**

1. Traps are **embedded in realistic code** — read, don't recognize a title.
2. Issues **stack** (compile + DI + null-forgiving + async).
3. Prompts ask **"what would you change and why?"** — prioritization matters.
4. **Hosting context** assumed — DI, middleware, HTTP, serialization together.

---

## What Layer 2 questions test (derive from folder content)

When reading the user's folder, bias questions toward these **test intents** (not fixed section names):

| Test intent | Example angles | Snippet / scenario signals |
|---|---|---|
| **HTTP & hosting path** | TLS offload, client IP, scheme, throttling | Kestrel, IIS, proxy, `ForwardedHeaders`, rate limit |
| **Framework under pressure** | Startup vs per-request behavior | Middleware short-circuit, `IEnumerable<T>` DI, options reload, scope validation |
| **Code review / defects** | Multi-category diagnosis | `.Result`, `new HttpClient()`, `throw ex`, broken generics, `ContainsKey`+indexer |
| **API & contracts** | REST + JSON semantics | 201 + Location, camelCase policy, optional `bool?` vs tri-state |
| **Architecture & state** | Lifetimes & scale-out | Singleton + user state, static cache, captive scoped in singleton |

**Do not** write pure definition questions without a scenario — redirect those to foundation banks.

---

## Trigger

User provides **folder path(s)** and optionally `@KARAT_DEBRIEF_SOURCE.txt` or a pasted debrief list to distribute.

Phrases: Karat, Citi, production readiness, code review interview, applied ASP.NET Core, Layer 2.

---

## Inputs

1. **User folder path** — e.g. `05. ASP.NET Core/03. Middleware Pipeline/` or `02. C# Language Fundamentals/06. Multithreading & Async Programming/`.
2. **Reading material in that folder** — `Program.cs`, tutorials, `.md` chapters, `.gitkeep` topic name.
3. **`KARAT_DEBRIEF_SOURCE.txt`** (optional) — raw debrief lines to map into matching folders.
4. **Foundation banks** (reference only) — avoid duplicating long definitions.

---

## Output (per target folder)

Write **both files in the same folder the user scoped**:

| File | Role |
|---|---|
| `KARAT_INTERVIEW_QUESTIONS.md` | Questions only |
| `KARAT_INTERVIEW_ANSWERS.md` | Answers for that folder's questions |

**Numbering:** `#### Q1`, `#### Q2`, … **restart at Q1 in each folder file**.

**Optional index** (only when user asks or after bulk distribute):

- `…/04. Citi Karat Interview Practice/KARAT_INDEX.md` — links to each folder's pair; not a duplicate question bank.

**Do not** hardcode a single monolithic bank path in answers — always relative to the scoped folder.

---

## Question types

Tag optionally in question text: `(R)` review, `(P)` production pattern, `(D)` design judgment, `(M)` mechanism under scenario, `(Live)` spec-reading meta (practice folders only).

### (R) Code review

- 8–20 lines realistic C# / `Program.cs` / service code.
- 2–4 issues across: compile, runtime/async, DI lifetime, HTTP/API, design.
- Ask: problems **and/or** prioritized fix.

### (P) Production pattern

- How/when to implement correctly; what breaks if skipped.
- Include hosting or deployment context when folder is web-related.

### (D) Design judgment

- Trade-offs: optional flags, stateful singletons, test seams, error shape.

### (M) Mechanism under pressure

- Behavior at startup or per request — tied to code in the folder's topic.

### (Live) — only in Karat practice coding folders

- Meta questions on spec reading, edge cases, verbal walkthrough — link to local `.cs` + tests.

---

## Transform foundation → Karat (authoring recipe)

| Avoid (Layer 1) | Prefer (Layer 2) |
|---|---|
| "What is middleware?" | "401 returned but pipeline continues — what's wrong with this middleware?" |
| "Difference throw vs throw ex" | "Review this catch — what breaks observability?" |
| "What is Kestrel?" | "HTTPS locally, HTTP links in prod behind nginx — what's missing in Program.cs?" |

**Recipe:**

1. Start from folder tutorial or real 8–20 line snippet.
2. Inject 1–3 realistic mistakes matching folder topic.
3. Ask: what's wrong / how fix / what use in production (pattern name + why).
4. Answer with diagnosis before fix.

**Sources:** PR comments, debrief lists, Microsoft docs for that topic, production postmortems.

---

## Question file header

```markdown
# Karat — Interview Questions

> **Folder:** `[relative path from repo root]`
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)
> **Level:** Applied production readiness (Layer 2)

---
```

---

## Answer style (required)

### Opening `**Answer:**`

1–3 complete sentences: verdict, approach, or core diagnosis. Standalone.

### Type (R) — mandatory subsections

```markdown
**Issues:**

| Category | Problem | Impact |
|---|---|---|

**Fix (priority order):**

1. …

**Production takeaway:** …
```

Priority: **blocks build → correctness/runtime → maintainability/design**.

### Types (P)(D)(M)

3–5 full-sentence bullets **or** table + closing takeaway. Fix code 3–10 lines when it teaches.

### Rules

- Plain English — no "say this in the interview".
- Name real types (`ForwardedHeadersOptions`, `IHttpClientFactory`, `ProblemDetails`).
- Cross-ref foundation briefly (`See C# Module 01 Gotcha — throw; vs throw ex`) — don't re-teach CLR/GC.
- Modern `WebApplication` hosting unless question is historical.

### Do not

- Copy `@interview-answers` essay structure without Issues/takeaway for review questions.
- Put Layer 1 definition-only questions here.
- Fix without impact explanation.

---

## Answer file header

```markdown
# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `[same path]`

---
```

---

## Workflow

1. **Parse user scope** — one folder, module (all subfolders), or distribute from `KARAT_DEBRIEF_SOURCE.txt`.
2. **Read folder content** — tutorials, chapter title, code samples.
3. **Draft 3–8 questions per folder** (more for deep web chapters; fewer for narrow topics). Match folder topic exactly.
4. **Map debrief lines** — assign each source bullet to best-fit folder; remove from source tracking when done.
5. **Write** `KARAT_INTERVIEW_QUESTIONS.md` then `KARAT_INTERVIEW_ANSWERS.md` per folder.
6. **Update** `KARAT_INDEX.md` if bulk job.
7. Self-check.

---

## Self-check

- [ ] Questions derived from **user folder**, not a baked-in taxonomy
- [ ] No definition-only prompts without scenario
- [ ] (R) answers have Issues table + prioritized fix + production takeaway
- [ ] Q numbers restart per folder file
- [ ] Both Q and A files live in **same folder** as scope
- [ ] Layer 2 depth — stacked issues where appropriate

---

## Preparation guidance (for humans — optional note in index)

Layer 1 banks → vocabulary. Layer 2 → daily code-review drills on 10–15 line snippets. Layer 3 → timed specs with verbal walkthrough. Build one real API with forwarded headers, rate limits, global exception handler, `IHttpClientFactory`, options pattern.

---

## Examples & reference

- [examples.md](examples.md) — full Q&A samples
- [reference.md](reference.md) — debrief distribution map, index format, issue categories

---

## User invocation

```
@karat-interview-answers @05. ASP.NET Core/04. Dependency Injection & Service Lifetimes/
Create KARAT_INTERVIEW_QUESTIONS.md and answers from this folder's topic.
```

```
@karat-interview-answers
Distribute KARAT_DEBRIEF_SOURCE.txt into matching 05. ASP.NET Core subfolders with answers.
```
