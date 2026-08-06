---
name: reading-tutorial
description: >-
  Creates reading-based code tutorials (Program.cs or module entry files) for
  this .NET learning repository. Always reads TOPICS.md in the topic folder for
  coverage guidance. Covers topic depth (FULL/PREVIEW/DEFER), curriculum folders,
  naming, csproj settings, comment format, and project structure evolution. Use
  when creating or editing tutorial chapters, topic folders, C# fundamentals,
  .NET Core modules, ASP.NET APIs, LINQ topics, or reading-based Program.cs.
---

# Reading-Based Tutorial Skill

Use this skill as the **complete system prompt** when creating or rewriting tutorial content anywhere in this repo — `01. C# & LINQ`, `.NET Core`, ASP.NET APIs, EF Core, etc.

The user learns by **reading code and comments**, not menus or throwaway demos.

---

## When to apply

- User @-mentions this skill or asks for a tutorial / chapter / `Program.cs` lesson
- Creating a new numbered topic folder (`NN. Topic Name`)
- Rewriting legacy folders (menus, `*Demo.cs`, `#pragma`, unused variables)
- Planning curriculum gaps or proposing new topic folders

---

## Topic hierarchy

```
Module (e.g. 01. C# Language Fundamentals, 02. ASP.NET Core APIs)
  └── NN. Topic Folder/
        ├── TOPICS.md              ← coverage guide — read FIRST (required)
        ├── Program.cs           ← primary reading entry (Phase 1–2)
        ├── TopicName.csproj
        ├── Models/              ← Phase 2+
        ├── Services/
        ├── Repositories/
        └── Controllers/         ← API modules (Phase 3)
```

| Level | Responsibility |
|-------|----------------|
| **Module folder** | Groups chapters; may contain `.sln` |
| **Topic folder** | One teachable unit; **folder name = scope boundary** |
| **`TOPICS.md`** | Subtopics, depth, and deliverable requirements for this chapter |
| **Subtopic** | Section in main file or its own topic folder |
| **Future folder** | Full depth deferred until `NN.` folder exists |

---

## TOPICS.md — coverage guide (required reference)

Every topic folder includes **`TOPICS.md`**. **Always read it first** before creating or editing a tutorial (`Program.cs`) or planning practice for that folder.

### What TOPICS.md contains

| Section | Use |
|---------|-----|
| **Status** | `Not Started` / `In Progress` / `Complete` — current state |
| **Purpose** | Chapter goal in one paragraph |
| **Subtopics to cover** | Table: subtopic, **Depth** (FULL / PREVIEW / DEFER), Notes |
| **Program.cs requirements** | Extra deliverable rules for this chapter |
| **Syllabus mapping** | External course outline alignment |

### How to use it (with this skill's rules)

1. **Read `NN. Topic Name/TOPICS.md`** before writing any code.
2. Treat the **Subtopics to cover** table as the **checklist of what must appear** in the tutorial (sections, code, or previews).
3. **Depth column in TOPICS.md** aligns with FULL / PREVIEW / DEFER below — follow TOPICS.md when present; it overrides generic examples in this skill.
4. **Program.cs requirements** in TOPICS.md are additive (e.g. "order summary demo", "quick reference") — honor them unless they conflict with non-negotiables (0 warnings, no `#pragma`, no menus).
5. This skill's **format rules** (comments, structure, ImplicitUsings, no meta "how to use") still apply; TOPICS.md governs **what** to teach, not **how** to comment.
6. If a subtopic is in TOPICS.md but missing from `Program.cs`, add it. If `Program.cs` teaches something **not** in TOPICS.md and not implied by the folder name, remove or move it to the correct chapter.
7. If **`TOPICS.md` is missing**, infer from folder name + this skill; **propose or create** a TOPICS.md for the user before large writes.
8. After completing a major tutorial pass, optionally update **Status** in TOPICS.md (`In Progress` → `Complete`).

**TOPICS.md is guidance, not a rigid script** — use judgment for ordering and section merge/split, but **every FULL row should be covered**; PREVIEW rows get short treatment + forward refs; DEFER rows stay out of the chapter.

---

## Subtopic depth — decide before writing (agent only)

Use **TOPICS.md** as the primary depth map when it exists. The rules below apply when planning sections and when TOPICS.md has no depth column for an item.

Assign each subtopic **FULL / PREVIEW / DEFER** while planning the chapter.
This depth map is **for your reference only** — do **not** add a
**COVERAGE IN THIS FOLDER** block (or FULL / PREVIEW / DEFER lists) to
`Program.cs` or any tutorial source file.

### FULL — teach completely here

Subtopic belongs to this folder; no dedicated later folder planned soon.

- Numbered section + multiline explanation + tables + compile-error notes
- Runnable code; **every declaration used**
- Reader can start without another chapter

Examples (`02. Data Types and Variables`): `var`, `const`, `readonly`, casting, integers, decimal, nullable, string as type, scope, assignment operators.

### PREVIEW — introduce only; defer depth

Subtopic appears in this chapter but a **later folder** owns the full lesson.

- One concise explanation block (not a stub)
- Minimal code wired into the runnable program
- Forward reference in comments:

```
 * COVERED IN DETAIL LATER → NN. Topic Folder Name
 *   (headline concepts only: …)
```

Examples: `string` here → full lesson in future **Strings** folder; boxing preview → Generics later.

**Do not** duplicate the future chapter.

### DEFER — omit entirely

Outside folder name; has or will have its own topic folder.

Examples: `dynamic`, async, LINQ, file I/O, `struct` in Data Types and Variables unless the folder scope includes them.

**When unsure:** prefer a **new numbered topic folder** over bloating the current chapter.

---

## Curriculum & naming

### Create new topic folder when

- FULL content would exceed ~800 lines or ~20 full sections in one file
- Concept is a standard chapter on its own (Strings, Input & Output, Classes, Generics, Minimal APIs, Middleware, etc.)
- User asks to split, or gap analysis shows missing coverage

### Propose before creating

Tell the user suggested folders and numbers; create only when implementing.

### Naming — keep aligned

| Artifact | Convention | Example |
|----------|------------|---------|
| Topic folder | `NN. Human Readable Topic` | `02. Data Types and Variables` |
| `.csproj` | PascalCase | `DataTypesAndVariables.csproj` |
| `RootNamespace` / `namespace` | Same PascalCase | `DataTypesAndVariables` |
| Supporting dirs | PascalCase plural | `Models/`, `Services/`, `Repositories/`, `Utils/`, `Controllers/` |

**Folder naming:** Prefer **Data Types and Variables** over "Primitive Data Types" when the chapter includes reference types (`string`, etc.) at FULL depth. Keep **`struct`** DEFER to a later OOP/types chapter unless the folder explicitly covers custom value types.

**Renaming:** update folder, `.csproj`, `RootNamespace`, `namespace`, `.sln`, and comment cross-references together.

Use **`NN.`** prefix; leave numbering gaps for inserts.

---

## Project phases

### Phase 1 — Fundamentals (default)

- One **`Program.cs`** textbook chapter; runs correctly
- No `*Demo.cs`, no runtime menus / switches

### Phase 2 — Types needed

```
Program.cs + Models/   (real used types, not junk drawers)
```

### Phase 3 — App / API modules

```
Program.cs (thin entry) + Controllers/ + Services/ + Repositories/ + Models/
```

- Each file: multiline header comment explaining purpose
- **One reading path:** start at entry file; comments point to supporting files
- API topics: same comment-first style on `Program.cs`, endpoints, DTOs — scope still = folder name

### Split out of main file when

| Keep in Program.cs | Separate file |
|--------------------|---------------|
| One section worth | Reused types / topic-named types |
| ≤ ~15 lines | `Models/Order.cs`, `Services/OrderService.cs` |

---

## File intro (required multiline blocks)

1. **TOPIC**
2. **WHY IT MATTERS**
3. **WHAT YOU WILL LEARN** (numbered)

**Never include in source code:** COVERAGE / FULL / PREVIEW / DEFER maps,
"How to use this file", read-order instructions, IDE/run checklists, or
"this file" meta-talk.

---

## Comment format

| Use | For |
|-----|-----|
| Multiline `/* */` | Intro, `SECTION N:`, tables, tips, `--- Na. Subsection ---` |
| Inline `//` | Code lines only |
| Never `#` | Invalid in C# |

## Section structure

1. Intro blocks → `using` → `namespace` → `class Program` → `Main`
2. Explanation **before** code; comments immediately above what they describe
3. Subsections for multi-part sections (e.g. `--- 13a. Implicit ---`)
4. **Quick reference** cheat sheet at file end (outside `Main`)

---

## Code requirements

- Compiles and runs — **0 warnings**
- No `#pragma warning disable`
- No illustration-only variables — every executable line **used**
- One **cohesive runnable program** (order summary, API seed demo, etc.)
- No helper classes at bottom unless topic requires — use `Models/` when splitting
- `Console.WriteLine` / output only from the real example, not narration spam

## Do not use (unless topic is explicitly about that)

- Runtime menus, "Run All Demos", teaching-only `*Demo.cs`
- Scope creep beyond folder name
- Over-engineering

---

## `.csproj` defaults (all tutorial projects)

```xml
<TargetFramework>net8.0</TargetFramework>
<ImplicitUsings>disable</ImplicitUsings>
<Nullable>enable</Nullable>
<OutputType>Exe</OutputType>
```

**Skeleton:**

```csharp
using System;

namespace TopicName;

public class Program
{
    public static void Main(string[] args)
    {
        // runnable example
    }
}
```

- Explicit `using` at top of **every** file; only what that file needs
- First namespace lesson (Hello World): dedicated section on `using` + why ImplicitUsings is disabled (hidden global usings table)
- Later chapters: add usings as new namespaces appear

---

## Compile errors to mention in comments

CS0165 (unassigned), CS0103 (missing using), CS1012 (char vs string), CS0017 (multiple entry points), `FormatException` vs `TryParse`, bad unbox `InvalidCastException`.

---

## Workflow — creating a new tutorial

1. Read **`NN. Topic Name/TOPICS.md`** — subtopics, depths, Program.cs requirements
2. Cross-check **folder name** and syllabus mapping
3. Draft numbered sections from TOPICS.md (FULL = deep; PREVIEW = short + forward ref; skip DEFER)
4. Build one runnable program using all declarations (per TOPICS.md requirements if any)
5. Add quick reference
6. Run `dotnet build` — fix until 0 warnings
7. If TOPICS.md scope is too large for one file → propose new `NN.` folder or update TOPICS.md with user
8. Update TOPICS.md **Status** if appropriate

## Workflow — improving legacy tutorials

1. Read **`TOPICS.md`** — gap analysis vs existing `Program.cs`
2. Map missing subtopics; align depths with TOPICS.md
3. Merge into one `Program.cs` (or Phase 2/3 layout)
4. Expand short FULL sections; shrink PREVIEW sections
5. Disable ImplicitUsings; add explicit usings
6. Align folder / csproj / namespace; update `.sln`

---

## Finish checklist

- [ ] **`TOPICS.md` read**; every **FULL** row has a section + used code
- [ ] Every **PREVIEW** row: short treatment + forward ref (per TOPICS.md Notes)
- [ ] **DEFER** rows not taught at full length
- [ ] **Program.cs requirements** from TOPICS.md satisfied
- [ ] Subtopics mapped FULL / PREVIEW / DEFER (agent planning — not in source)
- [ ] 0 warnings; no `#pragma`
- [ ] Names aligned (folder, csproj, namespace)
- [ ] ImplicitUsings disabled; usings explicit
- [ ] Quick reference at end

---

## Example depth map (fallback only)

Use **`TOPICS.md` in the topic folder** instead of this table when the file exists.

| Subtopic | Depth | Notes |
|----------|-------|-------|
| Variables, scope, naming | FULL | Core |
| var, const, readonly | FULL | readonly via field + ctor or `Models/` |
| Assignment operators | FULL | |
| Integer / float / decimal / bool / char | FULL | |
| Type conversion & casting | FULL | |
| Nullable value types | FULL | |
| Default values | FULL | |
| string | PREVIEW | → future **Strings** folder |
| object / boxing | PREVIEW | → Generics later |
| dynamic | DEFER | own folder |

For API modules example: **Minimal API setup** FULL in folder `01. Minimal APIs`; **JWT auth** PREVIEW if folder is `01. Routing` and auth gets folder `05. Authentication`.

---

## Additional reference

For the full expanded guide (duplicate of this skill for offline reading), see [reference.md](reference.md).

After a module's reading chapters are in place, use **`@hands-on-practice`** to build or extend the bank at:

```
00. Notes & Practice/01. Practice/{CurriculumModule}/{ReadingModule}/
```

Example: `01. Practice/01. C# & LINQ/01. C# Language Fundamentals/` with `01. Scenarios.md`, `02. DebugReading.md`, `03. COVERAGE.md`, and `Problems/`. Read **every** `TOPICS.md` in the reading module for coverage.
