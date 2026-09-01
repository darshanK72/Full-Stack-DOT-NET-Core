---
name: reading-tutorial
description: >-
  Creates reading-based code tutorials for any programming language, framework,
  or project type (Python, Java, TypeScript, Go, Rust, C#, Node.js, React, Django,
  Spring Boot, etc.). Teaches by reading code and comments — section comment then
  the code it explains. Single-file for scripts and language topics; multi-file for
  framework/project topics. Language, framework, and technology agnostic.
argument-hint: "[folder/path or topic name]"
---

# Reading-Based Tutorial Skill

Use this skill when creating or rewriting tutorial content for **any technology** — language fundamentals, framework chapters, CLI tools, data science notebooks, cloud SDKs, or full-stack project modules.

The learner learns by **reading code and comments**, not menus, runnable demos detached from explanation, or prose-only docs.

**Core rule:** A section comment explaining a concept goes **immediately above** the code that demonstrates it, in the same file that owns that code.

---

## When to apply

- User asks to create a tutorial, chapter, lesson, or `[entry-point file]` for a topic
- Creating a new topic folder for any technology
- Rewriting or improving existing tutorials that have scattered explanations or unexplained code
- Planning curriculum gaps across a module

---

## Step 1 — Detect language and project type

Before writing anything, identify:

1. **Language** — from file extensions, folder name, or user instruction
2. **Project type** — script/CLI, library, web framework, data pipeline, etc.
3. **Existing files** — what already exists in the folder

| Signal | Language / Tech |
|---|---|
| `.py` / `requirements.txt` / `pyproject.toml` | Python |
| `.java` / `pom.xml` / `build.gradle` | Java |
| `.ts` / `.tsx` / `tsconfig.json` | TypeScript |
| `.js` / `package.json` (no tsconfig) | JavaScript / Node.js |
| `.cs` / `*.csproj` / `*.sln` | C# / .NET |
| `.go` / `go.mod` | Go |
| `.rs` / `Cargo.toml` | Rust |
| `.rb` / `Gemfile` | Ruby |
| `.php` / `composer.json` | PHP |
| `.kt` / `*.gradle.kts` | Kotlin |
| `.swift` / `Package.swift` | Swift |
| `.sql` / `.ddl` | SQL |
| `Dockerfile` / `docker-compose.yml` | Docker / DevOps |
| `.ipynb` | Jupyter / Python data science |

If the user names a technology explicitly, that takes precedence over file signals.

---

## Step 2 — Choose layout mode

Two layout modes. Choose based on what is being taught:

| When to use | Layout | Entry point |
|---|---|---|
| Language fundamentals, scripting, algorithms, data structures, LINQ, lambdas, generators, type system | **Single-file** | Language-appropriate entry file |
| Web framework, ORM/data access, API layer, CLI tool with subcommands, ML pipeline, microservice | **Multi-file** | Entry file + organized folders |

**Rule:** If a single file would exceed ~600–800 lines even with good structure, propose a new numbered topic folder instead.

---

## Layout A — Single-file (scripts, language topics)

Put the **entire lesson in one file**. The reading path is top-to-bottom — scroll once to learn the whole chapter.

### Entry point by language

| Language | Typical single-file entry |
|---|---|
| Python | `main.py` or `topic_name.py` |
| Java | `Main.java` or `TopicName.java` |
| TypeScript / JavaScript | `index.ts` / `index.js` or `topic-name.ts` |
| C# | `Program.cs` |
| Go | `main.go` |
| Rust | `main.rs` |
| Ruby | `main.rb` |
| Kotlin | `Main.kt` |

### Reading order = file order

1. **File header** — TOPIC, WHY IT MATTERS, WHAT YOU WILL LEARN
2. **Imports / using / require** — only what this file actually uses
3. **Section blocks and code interleaved** — concept comment directly above the code it explains
4. **Quick reference** — cheat sheet at the end of the file (optional but recommended)

Each section:
```
/*
 * SECTION N: CONCEPT NAME
 * What it is, why it exists, tables of variants, pitfalls.
 */
[code that demonstrates it — directly below, no gap]
```

### Where each concept belongs

| What you are teaching | Where the comment goes | Where the code goes |
|---|---|---|
| Language syntax, built-ins, control flow | Above the block or helper call | Inline in main/entry block, or small helpers |
| A type definition (class, struct, enum, dataclass) | Directly above that type declaration | Same file, below the comment |
| Methods on a type | Above the method (or above the type if the whole type is one section) | Inside the type, same file |
| Static helpers / utilities | Above the function/method | Same file |

### Anti-patterns — do not produce

| Bad pattern | Why |
|---|---|
| Full lesson in comments inside main(); types at the bottom with no context | Bottom of file is unexplained |
| One giant main block with every line explained inline | No structure; types never shown properly |
| Inline comments only — no section blocks | Reader loses tables, forward refs, structure |
| Blank line between every code line | Doubles length; hard to scroll |
| Multiple demo files with a menu switcher | Not a single reading path |

---

## Layout B — Multi-file (framework, ORM, API, data pipeline topics)

Use when the topic teaches real application structure — controllers, services, repositories, models, middleware, pipelines, etc.

### Typical folder structures by tech

**Python web (Flask/Django/FastAPI):**
```
topic_folder/
├── main.py / app.py          ← entry + wiring
├── models/                   ← ORM models, schemas
├── services/ or handlers/    ← business logic
├── repositories/ or db/      ← data access
├── utils/ or helpers/        ← shared utilities
└── requirements.txt
```

**Java Spring Boot:**
```
topic_folder/
├── src/main/java/com/example/
│   ├── Application.java      ← entry
│   ├── controller/           ← REST controllers
│   ├── service/              ← business logic
│   ├── repository/           ← data access (JPA, etc.)
│   └── model/                ← entities, DTOs
└── pom.xml
```

**TypeScript / Node.js (Express / NestJS):**
```
topic_folder/
├── src/
│   ├── index.ts              ← entry + wiring
│   ├── controllers/
│   ├── services/
│   ├── repositories/ or models/
│   └── middleware/
└── package.json / tsconfig.json
```

**Go:**
```
topic_folder/
├── main.go                   ← entry
├── handlers/
├── services/
├── models/
└── go.mod
```

**Only create folders the chapter actually uses.** Do not scaffold empty layers.

### Reading order across files

1. **Entry file intro** — TOPIC, WHY IT MATTERS, WHAT YOU WILL LEARN
2. **Chapter map in entry file** — numbered list of sections with the target file for each (e.g., `SECTION 3 → services/UserService.py`)
3. **Each other file** — has its own FILE ROLE header and section comments above its code
4. **Entry file main/startup** — wires the demo last; stays thin

### Where each concept belongs (multi-file)

| What you are teaching | File | Comment placement |
|---|---|---|
| Domain entity / data class / DTO | `models/` | Section block above the type |
| Data access / query / ORM operation | `repositories/` or `db/` | Section block above the method |
| Business logic / use case | `services/` or `handlers/` | Section block above the method |
| Shared utility / helper | `utils/` or `helpers/` | Section block above the function |
| Request handler / controller / route | `controllers/` or route file | Section block above the handler |
| DI wiring / middleware / startup | Entry file | Section block above the registration |
| Demo orchestration | Entry file `main` or startup | Short wiring comments; concepts live in other files |

### Multi-file anti-patterns

| Bad pattern | Fix |
|---|---|
| All logic crammed into entry file / main | Move to service/repository/handler with section comments |
| Model files with a bare class and no header | Add FILE ROLE + SECTION block above the type |
| Entry file explains the ORM; repository file has unexplained code | Move ORM section comment into the repository file |
| Bare files with one-liners and no context | Every file that holds lesson code needs section comments |

---

## Comment format by language

Adapt comment syntax to the detected language. Preserve the same two-layer approach:
- **Block comments** for section headers, tables, pitfalls, forward refs
- **Inline comments** for individual code lines that need a quick hint

| Language | Block comment | Inline comment |
|---|---|---|
| Python | `"""..."""` docstring or `# --- SECTION N ---` | `#` |
| Java / Kotlin | `/* ... */` | `//` |
| TypeScript / JavaScript | `/* ... */` or `/** */` | `//` |
| C# | `/* ... */` | `//` |
| Go | `/* ... */` or block of `//` | `//` |
| Rust | `/* ... */` or doc `///` | `//` |
| Ruby | `=begin ... =end` or `# ---` | `#` |
| PHP | `/* ... */` | `//` or `#` |
| SQL | `/* ... */` | `--` |

### Section block format (adapt comment syntax per language)

```
/*
 * SECTION N: CONCEPT NAME
 *
 * What it is in 1–3 sentences.
 * Why it matters or when to use it.
 *
 * Variants / comparison table (if useful):
 *   Option A — description
 *   Option B — description
 *
 * Pitfalls:
 *   - Common mistake and why it fails
 *
 * COVERED IN DETAIL LATER → [other topic folder] (for PREVIEW depth only)
 */
```

### Inline comment density

| Location | Guidance |
|---|---|
| Type fields / constructor | Comment first field group, constructor purpose, non-obvious initializers |
| Main / entry orchestration | Comment each logical step; note what outputs are expected |
| Self-explanatory boilerplate | Leave uncommented |
| Non-obvious operators or API calls | Add `// what this does in one phrase` |

Do not put a blank line between every code line. Normal spacing only (between methods, sections, logical groups).

---

## File intro (required for every file)

### Entry file / main file

```
/*
 * TOPIC: [folder/topic name]
 *
 * WHY IT MATTERS:
 * [1–3 sentences on real-world relevance]
 *
 * WHAT YOU WILL LEARN:
 * 1. [sub-topic]
 * 2. [sub-topic]
 * ...
 *
 * CHAPTER MAP (multi-file only):
 * 1. [Sub-topic A] → [relative/path/to/file.ext]
 * 2. [Sub-topic B] → [relative/path/to/file.ext]
 * 3. Demo wiring  → [this file] main / startup
 */
```

### Other files in a multi-file chapter

```
/*
 * FILE ROLE: [what this file teaches in one sentence]
 *
 * SECTIONS IN THIS FILE:
 * - SECTION N: [topic]
 * - SECTION M: [topic]
 */
```

**Never include in source:** FULL/PREVIEW/DEFER coverage maps, "How to run this", IDE instructions, or agent workflow notes.

---

## Coverage depth planning

Assign each sub-topic one of three depths **before writing** (planning only — never put this in the output file):

| Depth | What to write |
|---|---|
| **FULL** | Complete section block + tables + pitfalls + runnable code directly below |
| **PREVIEW** | One-paragraph section + minimal code + forward ref: `COVERED IN DETAIL LATER → [folder name]` |
| **DEFER** | Omit entirely — belongs in another topic folder |

**When unsure:** prefer a new numbered topic folder over bloating the current one.

---

## Project/build file defaults by technology

Include a minimal, working project config file for the relevant tech:

| Technology | Config file | Key settings to include |
|---|---|---|
| Python | `requirements.txt` or `pyproject.toml` | Pinned deps; Python version in pyproject |
| Java (Maven) | `pom.xml` | Java version, dependencies, main class |
| Java (Gradle) | `build.gradle` | Java version, dependencies |
| Node.js | `package.json` | `name`, `main`, `scripts.start`, deps |
| TypeScript | `tsconfig.json` | `target`, `module`, `strict`, `outDir` |
| C# | `*.csproj` | `TargetFramework`, `Nullable`, `ImplicitUsings` |
| Go | `go.mod` | Module path, Go version |
| Rust | `Cargo.toml` | `[package]` name/version, `[dependencies]` |

---

## Code requirements (all technologies)

- Runnable and working — no placeholder functions, no `TODO` stubs in the lesson code
- Every variable and declaration is actually used — no dead code
- One cohesive demo wired through the entry point
- No runtime menu switches or demo selectors
- Output / print statements only where they teach — not narration spam
- Use the idiomatic style for the language (list comprehensions in Python, streams in Java, destructuring in JS, etc.)

---

## Workflow — creating a new tutorial

1. Detect language and project type from folder/files/user instruction
2. Choose layout (A single-file vs B multi-file)
3. Plan section order and file ownership (depth: FULL / PREVIEW / DEFER)
4. Write entry file intro (+ chapter map for multi-file)
5. Write other files with FILE ROLE header and section comments
6. Wire demo in entry point — thin orchestration only
7. Add quick reference at end of entry file
8. Verify: no blank line between every code line; line count is reasonable

---

## Workflow — improving an existing tutorial

1. **Gap analysis** — what's missing given the folder name? What's explained away from its code?
2. **Merge strays** — for single-file topics: merge any split-out type files back into the main file with section comments above each type
3. **Fix comment placement** — move concept comments out of the entry/main block onto the types and methods they actually explain
4. **Expand thin sections** — add tables, pitfall notes, and sub-sections for major API families
5. **Split if needed** — if a file exceeds ~800 lines after improvements, propose a new topic folder

---

## Finish checklist

- [ ] Layout matches the topic: single-file for language fundamentals; multi-file for frameworks/projects
- [ ] Multi-file: chapter map in entry file; every lesson file has FILE ROLE + section comments
- [ ] Every section comment sits **directly above** the code it explains in the file that owns it
- [ ] Inline comments on non-obvious lines (alongside block comments)
- [ ] Normal spacing — no blank line between every code line
- [ ] No bare unexplained files — every file with lesson code has section headers
- [ ] Entry file / main orchestrates the demo — not the only place concepts are explained
- [ ] Every important concept implied by the folder name is covered (FULL or PREVIEW)
- [ ] PREVIEW items: brief treatment + forward ref to the deeper folder
- [ ] Code is idiomatic for the detected language
- [ ] Project config file present with working settings

---

## Invocation examples

```
# Create a Python async/await tutorial folder:
/reading-tutorial 05. Async-Await and Coroutines/

# Create a Spring Boot REST API chapter (multi-file):
/reading-tutorial 03. REST Controllers and Validation/

# Improve an existing JavaScript closures tutorial:
/reading-tutorial 02. Closures and Scope/ (existing folder)

# Create a GenAI RAG pipeline tutorial:
/reading-tutorial 06. Retrieval-Augmented Generation/
```
