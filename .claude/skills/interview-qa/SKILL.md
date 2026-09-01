---
name: interview-qa
description: >-
  Creates comprehensive interview Q&A for ANY technology, language, or framework
  (Java, Python, .NET, Node.js, React, Go, GenAI, SQL, DevOps — anything).
  On empty folders: generates INTERVIEW_QA.md with foundation questions, gotchas,
  and real-world scenarios derived from the folder topic. On nested folders: spawns
  one agent per subfolder. On existing files: offers full overwrite or incremental
  add. Answer format uses Concepts + flowing-prose Answer. Technology agnostic.
allowed-tools: [Read, Write, Edit, Glob, Agent, AskUserQuestion]
argument-hint: "[folder/path] [--overwrite | --add | --direction='...']"
---

# Interview Q&A Skill

Generate a **complete, self-contained interview Q&A file** for any technology, language, or framework. This skill works on any repo — .NET, Java, Python, JavaScript, Go, Rust, SQL, cloud, GenAI, DevOps, or any other domain.

The reader learns by **reading questions and answers**, not by memorizing scripts. Every answer must stand alone.

---

## Two layers this skill covers

| Layer | Question shape | Purpose |
|---|---|---|
| **Foundation** | "What is X?", "Difference between A and B?", "How does X work?" | Vocabulary, concepts, definitions |
| **Applied** | Code review, production patterns, design judgment, scenario-based | Engineering judgment under real conditions |

Both layers live in **one file** per folder. Gotchas (interview traps) bridge the two.

---

## Trigger

Invoke when the user:
- Applies this skill to a folder (with or without existing Q&A files)
- Asks to "create interview questions", "generate Q&A", "add questions", or "write interview prep"
- Names a technology/topic/module they want Q&A for

---

## Step 1 — Detect technology and context

Before generating anything, read the folder:

1. **Folder name** — primary signal: "Spring Boot REST APIs", "Python Decorators", "React Hooks", "SQL Joins", "GenAI Prompt Engineering"
2. **File extensions** — `.py` → Python, `.java` → Java, `.ts/.tsx` → TypeScript/React, `.cs` → C#, `.go` → Go, `.rs` → Rust, `.js` → JavaScript, `.rb` → Ruby, `.sql` → SQL
3. **Config/manifest files** — `package.json` → Node/JS, `pom.xml`/`build.gradle` → Java, `requirements.txt`/`pyproject.toml` → Python, `go.mod` → Go, `Cargo.toml` → Rust, `*.csproj`/`*.sln` → .NET, `Gemfile` → Ruby, `composer.json` → PHP
4. **README or existing content** — look for explicit technology mentions
5. **User instruction** — any explicit tech mention overrides all of the above

If the technology is ambiguous, note it in the file header and write questions that apply broadly (e.g., "general OOP" or "general REST API design").

---

## Step 2 — Check folder state and choose workflow

```
Is INTERVIEW_QA.md (or README.md with Q&A) present?
├── NO  → Does folder have immediate subdirectories with content?
│         ├── YES (nested folders) → Workflow C: spawn agents
│         └── NO (leaf folder)    → Workflow A: create from scratch
└── YES → Workflow B: update existing
```

---

## Workflow A — Create from scratch (empty or leaf folder)

Generate **`INTERVIEW_QA.md`** in the target folder with:

- **12–18 Foundation questions** covering the topic breadth
- **4–6 Gotchas** — common misconceptions or interview traps for that technology
- **5–8 Real-World Scenarios** — code review, production pattern, or design judgment

Write all questions AND answers in one pass. Do not create a questions-only file.

Use this document structure:

```markdown
# [Technology/Topic] — Interview Q&A

> **Topic:** [detected topic]
> **Level:** Foundation through Applied Production-Readiness

## Table of Contents

### Foundation Questions
- [Q1. Title](#q1-title)
...

### Gotchas
- [Gotcha 1. Title](#gotcha-1-title)
...

### Real-World Scenarios
- [Scenario 1. Title](#scenario-1-title)
...

---

## Foundation Questions

[questions with answers]

---

## Gotchas

[gotcha questions with answers]

---

## Real-World Scenarios

[scenario questions with answers]
```

---

## Workflow B — Update existing file

When `INTERVIEW_QA.md` already exists:

1. Read the file completely.
2. Ask the user which mode to use:

```
[AskUserQuestion]
Question: "INTERVIEW_QA.md already exists. How should we proceed?"
Options:
  A. Overwrite completely — replace all content with freshly generated Q&A
  B. Incremental add — analyze existing questions and append new ones (no duplicates)
  C. Incremental add with direction — same as B, but I'll provide a focus area or note
```

3. **Overwrite mode**: regenerate the full file from scratch using Workflow A rules.
4. **Incremental mode**: read all existing question titles; generate new questions that cover gaps; append them under the correct section headings; preserve existing content verbatim.
5. **With direction**: ask the user for a direction comment (e.g., "focus on async patterns", "add more cloud deployment scenarios", "add questions on error handling") before generating.

---

## Workflow C — Nested folders (spawn agents)

When the target folder contains immediate subdirectories that represent sub-topics:

1. List all immediate subdirectories.
2. Spawn one agent per subdirectory:
   - Agent task: run this same skill (`interview-qa`) on that subdirectory
   - Each agent writes its own `INTERVIEW_QA.md` beside the subfolder content
3. After all agents complete, write a **top-level `INTERVIEW_QA.md`** in the parent folder that:
   - Lists each subfolder with a link to its `INTERVIEW_QA.md`
   - Includes 3–5 cross-cutting questions that span the whole module

Agent prompt template:
```
Apply the interview-qa skill to: [subfolder path]
Technology context: [detected tech]
Create INTERVIEW_QA.md from scratch following the interview-qa skill rules.
```

---

## Question types — definitions

### Foundation questions

Cover the vocabulary, mechanics, and design of the technology. Every important concept implied by the folder name gets at least one question.

| Sub-type | Shape | Example |
|---|---|---|
| Definition | "What is X?" | "What is a Python decorator?" |
| Comparison | "Difference between A and B?" | "Difference between `==` and `is` in Python?" |
| How-it-works | "How does X work internally?" | "How does Go's garbage collector work?" |
| When-to-use | "When should you use X over Y?" | "When should you use `async/await` vs callbacks in Node.js?" |
| Evolution | "Why did X replace Y?" | "Why did Hooks replace class components in React?" |

Write 1–2 questions per major sub-topic of the folder topic. Do not leave important concepts uncovered.

### Gotcha questions

Common misconceptions that trip up candidates. Title each one with a short phrase naming the trap.

| What to trap | Example |
|---|---|
| Naming confusion | "Java vs JavaScript" |
| Subtle runtime behavior | "Python mutable default arguments" |
| Framework magic | "Spring bean scope when injected as field vs constructor" |
| Language-specific trap | "Go slice sharing after append" |
| Interview classic | ".equals() vs == in Java" |

Each gotcha: state what people wrongly believe, then the correct model.

### Real-World Scenario questions

Applied engineering judgment. Derive from the folder's actual topic and technology.

Three scenario sub-types:

**(R) Code Review** — embed a short code snippet (8–20 lines) with 2–4 realistic defects; ask candidate to identify and fix. Defects should span: correctness, performance, security, or idiom.

**(P) Production Pattern** — ask how to implement a pattern correctly and what breaks if done wrong. Include deployment or runtime context.

**(D) Design Judgment** — trade-off or architecture decision. Ask for approach and rationale.

Transform foundation concepts into scenarios:

| Avoid (foundation only) | Prefer (applied scenario) |
|---|---|
| "What is connection pooling?" | "Review this Python code that creates a new DB connection per request. What breaks at scale?" |
| "What is a memory leak?" | "This Node.js service leaks memory after 24 hours in production. What do you look for first?" |
| "What is JWT?" | "A developer stores the JWT secret in an env var that is logged at startup. What are the risks?" |

---

## Answer format (required for every question)

Use this exact two-section format — the same as reformat-qa:

```markdown
---

## Q{N}. [Question text]

**Concepts**
- [noun phrase naming one concept, mechanism, or trade-off — no explanation]
- [noun phrase]
- ...

**Answer**

[Flowing prose. First person optional ("I would...", "The key is..."). Sentences connect causally.
Start with the core insight, not a definition. Explain why the behavior exists.
Dense but readable — one reading is enough to understand completely.]
```

**Concepts rules:**
- 3–7 bullets per question
- One line only — no colons, no sub-bullets
- Name what the answer relies on: mechanisms, trade-offs, API names, design patterns
- Cut anything generic or obvious

**Answer rules:**
- Full sentences; causal connectors: "because", "since", "which means", "so", "rather than"
- Start with the core insight or mechanism, not a definition of the subject
- Where multiple options exist, address in logical order within the paragraph
- Include a code block only when words alone cannot convey the syntax

**Do not include:**
- "Pro tips", "say this in the interview", "the interviewer expects"
- "It is worth noting", "as a developer", "in practice you should"
- "As mentioned earlier", "see the next question"
- Trailing summaries: "In summary", "The takeaway is"

---

## Code Review scenario format

For **(R)** scenarios, extend the answer block with an Issues table:

```markdown
---

## Scenario {N}. [Title] (Code Review)

**Concepts**
- [defect category 1]
- [defect category 2]
- ...

**Problem Code**

```[language]
// code with embedded defects (8–20 lines)
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| [Correctness / Performance / Security / Idiom] | [what is wrong] | [what breaks] |

**Fix (priority order)**

1. [Most critical fix — one sentence]
2. [Next fix]
...

**Answer**

[Flowing explanation of why each issue exists and what the correct approach is. Connect diagnosis to fix.]
```

Priority order for issues: **correctness/compile errors → runtime/async bugs → security → performance → idiom/style**.

---

## Length guide

| Question type | Concepts | Answer prose |
|---|---|---|
| Definition | 3–4 bullets | 2–3 sentences |
| Comparison | 3–5 bullets | Table + 1–2 sentence closing |
| How-it-works / flow | 4–5 bullets | Numbered steps or 3–5 sentence paragraph |
| Gotcha | 3–4 bullets | 2 sentences: wrong model then correct model |
| Code review (R) | 3–5 bullets | Issues table + Fix list + 2–4 sentence Answer |
| Production pattern (P) | 3–5 bullets | 3–5 sentences covering setup, failure mode, fix |
| Design judgment (D) | 3–5 bullets | 3–6 sentences: approach + trade-offs + decision guidance |

Total per question: **100–250 words** for foundation; **150–350 words** for applied scenarios.

---

## Technology-specific guidance

The skill applies to any technology. Tailor question content — not question structure — to the detected tech:

| Domain | Foundation emphasis | Applied scenario emphasis |
|---|---|---|
| Backend languages (Java, Python, Go, C#, etc.) | Types, concurrency, memory model, stdlib | Thread safety, resource leaks, async misuse, exception handling |
| Frontend (React, Vue, Angular, etc.) | Component model, state, lifecycle, rendering | Re-render performance, state leaks, event handler cleanup |
| Databases (SQL, NoSQL, ORM) | Indexing, joins, transactions, normalization | N+1 queries, missing indexes, concurrency anomalies, schema migration |
| APIs (REST, GraphQL, gRPC) | Status codes, contract design, versioning | Breaking changes, auth flows, rate limiting, error shape |
| Cloud / DevOps (AWS, Docker, K8s, CI/CD) | Service primitives, networking, IAM | Misconfigured security groups, missing health checks, rollback strategies |
| GenAI / LLM (RAG, agents, prompt engineering) | Token limits, embedding, chunking, hallucination | Prompt injection, retrieval quality, cost control, latency |
| Data / ML (pandas, Spark, ML pipelines) | DataFrames, feature engineering, train/test split | Data leakage, memory pressure, model drift detection |

---

## File header (required)

```markdown
# [Technology/Topic] — Interview Q&A

> **Topic:** [folder name or detected technology]
> **Level:** Foundation through Applied Production-Readiness
> **Covers:** Foundation questions · Gotchas · Real-World Scenarios

---
```

Do not include generation timestamp, agent name, or tool name in the header.

---

## Self-check before writing the file

- [ ] Technology correctly detected; questions are specific to that tech (not generic)
- [ ] Foundation section: covers all major sub-topics of the folder name
- [ ] Gotcha section: each trap names a real misconception, not just a hard question
- [ ] Scenario section: at least one (R) code review with a real snippet and Issues table
- [ ] Every **Answer** is flowing prose — no bullet-only answers
- [ ] Every **Concepts** section has 3–7 one-line noun phrases
- [ ] No "interview tips", trailing summaries, or cross-references
- [ ] Code blocks use the correct language tag
- [ ] Table of Contents links match actual headings
- [ ] Nested folder workflow: one agent spawned per subfolder, then parent index written

---

## Invocation examples

```
# Create Q&A from scratch for a Python async topic folder:
/interview-qa 04. Async Programming/

# Update existing with direction:
/interview-qa 02. Spring Boot REST/ --add --direction="focus on security and exception handling"

# Overwrite existing:
/interview-qa 03. SQL Joins/ --overwrite

# Auto-detect from current folder:
/interview-qa
```
