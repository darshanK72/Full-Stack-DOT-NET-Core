---
name: reading-tutorial
description: >-
  Creates reading-based code tutorials for this .NET learning repository.
  Console / C# chapters: one Program.cs with sections, classes, and methods
  together. Data-access and web modules (ADO.NET, Dapper, EF Core, ASP.NET):
  proper project folders (Models/, Services/, Repositories/, Utils/, etc.)
  with multiline section comments in every relevant file, plus inline // hints.
  Coverage is driven by folder name, module README, and sibling chapters.
  Use when creating or editing tutorial chapters, topic folders, C# fundamentals,
  .NET Core modules, ASP.NET APIs, LINQ topics, or reading-based tutorials.
---

# Reading-Based Tutorial Skill

Use this skill as the **complete system prompt** when creating or rewriting tutorial content anywhere in this repo — `01. C# & LINQ`, `.NET Core`, ASP.NET APIs, EF Core, etc.

The user learns by **reading code and comments**, not menus or throwaway demos.

**This repo does not use `TOPICS.md`.** Teach everything important for the folder name with full context — do not leave obvious APIs or concepts out because a checklist file is missing.

---

## When to apply

- User @-mentions this skill or asks for a tutorial / chapter / `Program.cs` lesson
- Creating a new numbered topic folder (`NN. Topic Name`)
- Rewriting legacy folders (menus, `*Demo.cs`, `#pragma`, unused variables, sparse `Models/` splits)
- Planning curriculum gaps or proposing new topic folders

---

## Topic hierarchy

```
Module (e.g. 01. C# Language Fundamentals, 04. .NET Data Access, ASP.NET Core APIs)
  └── NN. Topic Folder/
        ├── Program.cs              ← entry point + chapter intro (always)
        ├── TopicName.csproj
        └── (layout depends on module — see Layout modes below)
```

| Level | Responsibility |
|-------|----------------|
| **Module folder** | Groups chapters; may contain `.sln` and `README.md` |
| **Topic folder** | One teachable unit; **folder name = scope boundary** |
| **Subtopic** | A numbered section comment with code **directly below** — in `Program.cs` or the file that owns that concept |
| **Future folder** | Full depth deferred until `NN.` folder exists |

---

## Layout modes — pick the right structure for the module

| Module type | Layout | Examples |
|-------------|--------|----------|
| **Console / C# & LINQ** | **Single-file** — entire chapter in `Program.cs` | Language fundamentals, OOP, generics, LINQ, async |
| **Data access** | **Multi-file** — realistic project folders | ADO.NET, Dapper, EF Core |
| **ASP.NET / Web API** | **Multi-file** — web project structure | Controllers, endpoints, middleware, DI chapters |

**Rule:** Comments follow the code. Teach each concept **in the file where that code lives** — never dump the full lesson into `Program.cs` while other files hold unexplained one-liners.

---

## Layout A — single-file `Program.cs` (console / C# chapters)

**Put the whole readable lesson in `Program.cs`.** Do not split types into `Models/` or similar for console chapters — merge stray splits back in when rewriting.

### Reading order = file order

The reader scrolls **top to bottom**. Each concept appears as:

1. **Section comment block** (`SECTION N:` + explanation + tables + pitfalls)
2. **The code that demonstrates it** — immediately below, with no gap

That code may live in:

- `Main` — orchestration, console output, wiring the demo
- **Helper methods** on `Program` — when a section teaches a reusable pattern
- **Other classes in the same file** — when a section teaches a type (properties, constructors, inheritance, etc.)

**Do not** teach a concept only in comments inside `Main` while the actual class sits in another file with a one-line header. **Do not** dump all explanation into `Main` and leave supporting types unexplained at the bottom.

### Where each concept belongs

| What you are teaching | Where the section comment goes | Where the code goes |
|----------------------|--------------------------------|---------------------|
| Language syntax, APIs, control flow | Above the block in `Main` or above a helper call | `Main` and/or small helpers on `Program` |
| A class, struct, record, enum | **Directly above that type declaration** | Same file, below the comment |
| Methods on a type | Above the method (or above the type if the whole type is one section) | Inside the type in the same file |
| Static helpers / parsers | Above the method | `Program` or a static helper class **in the same file** with its section comment above the class |

`Main` should **orchestrate** the demo (create objects, call methods, print results) — not hold pages of concept explanation that belongs above the types being taught.

### Anti-patterns (do not produce)

| Bad pattern | Why |
|-------------|-----|
| `Models/Foo.cs` with 3 lines; full lesson only in `Main` | Reader jumps files; type has no context |
| All sections as comments inside `Main`; classes at bottom with no comments | Bottom of file is unexplained |
| One giant `Main` with every line explained inline | Hard to read; types never shown properly |
| Inline comments only — no section blocks | Reader loses structure, tables, and forward refs |
| Blank line between every code line | Doubles file length; hard to scroll and review |
| Separate `*Demo.cs` or menu switches | Not a single reading path |
| **Multi-file module:** all teaching crammed into `Program.cs`; other files are bare code | Defeats realistic structure; reader cannot learn from Services/Repos/Models |
| **Multi-file module:** extra files with no section comments | Same as unexplained types at the bottom of a single file |

---

## Layout B — multi-file project structure (data access & web modules)

Use for **ADO.NET**, **Dapper**, **EF Core**, and **ASP.NET** chapters where the topic teaches real application structure — not just an API surface.

### Typical folder layout

```
NN. Topic Folder/
├── Program.cs                 ← chapter intro + thin entry / demo wiring
├── TopicName.csproj
├── Models/                    ← entities, DTOs, row types
├── Services/ or Repositories/   ← data access, business logic
├── Utils/ or Helpers/           ← connection helpers, mappers, extensions
├── Data/                      ← DbContext, SQL scripts (EF Core chapters)
└── (ASP.NET only)
    ├── Controllers/ or Endpoints/
    ├── Middleware/
    └── appsettings.json       ← when configuration is part of the lesson
```

Pick folders that match what the **folder name** teaches. Do not create empty layers — only folders the chapter actually uses.

### Reading order across files

1. **`Program.cs` file intro** — TOPIC, WHY IT MATTERS, WHAT YOU WILL LEARN (same as single-file chapters)
2. **Chapter map in `Program.cs` intro** — numbered list of sections and **which file to open next** (e.g. `SECTION 3 → Models/Product.cs`). This is curriculum navigation, not a "how to run" checklist.
3. **Follow the map** — each file has its own section comments above the code it owns
4. **`Program.cs` `Main` or startup** — wires the demo last (or references types defined in other files)

The reader opens files in the order the chapter map specifies. Every file is self-contained enough to read without hunting for explanation in `Program.cs`.

### Where each concept belongs (multi-file)

| What you are teaching | File | Comment placement |
|----------------------|------|-------------------|
| Entity / DTO / row type | `Models/` | Section block **above the type declaration** |
| Repository / data-access method | `Services/` or `Repositories/` | Section block **above the method or class** |
| Connection string helper, mapper | `Utils/` or `Helpers/` | Section block above the helper |
| DbContext, fluent config | `Data/` | Section block above `DbContext` or config method |
| API endpoint, controller action | `Controllers/` or endpoint file | Section block above the route/handler |
| DI registration, middleware pipeline | `Program.cs` | Section block above the registration / pipeline code |
| Demo orchestration | `Program.cs` `Main` or minimal hosting setup | Short wiring comments; concepts live in the files above |

### Multi-file anti-patterns

| Bad pattern | Fix |
|-------------|-----|
| All ADO.NET logic inline in `Main` | Move to `Repositories/` or `Services/` with section comments |
| `Models/Product.cs` — bare class, no header | Add full `SECTION N:` block above the type |
| `Program.cs` explains SqlCommand; `UserRepository.cs` has unexplained code | Move SqlCommand section comment into the repository file |
| One mega-file at 900 lines in a data-access chapter | Split by responsibility into folders; keep section comments in each file |

### When to stay single-file vs split

| Situation | Layout |
|-----------|--------|
| **C# & LINQ / console chapters** | **`Program.cs` only** — merge existing `Models/` back in when rewriting |
| **ADO.NET / Dapper / EF Core** | **Multi-file** — Models, Repositories/Services, Utils as the topic requires |
| **ASP.NET / Web API** | **Multi-file** — Controllers/endpoints, Services, Models, DI in `Program.cs` |
| **Intro / overview chapter** with no persistence yet | Single-file OK if scope is conceptual only |
| **File would exceed ~800 lines even after split** | Propose a **new numbered topic folder**, not a silent dump of unexplained files |

---

## Coverage planning — folder name drives scope

Before writing or rewriting a chapter, gather context from:

1. **Topic folder name** — primary scope boundary; include every important API, pattern, and pitfall a reader expects for that title
2. **Module `README.md`** — chapter list and any "grouped in chapters" notes (what belongs here vs a sibling folder)
3. **Sibling chapter folders** — avoid duplicating FULL depth owned by another `NN.` folder; use PREVIEW + forward refs instead
4. **Existing `Program.cs`** (when improving) — gap analysis: missing APIs, sparse split files, concepts explained away from their code
5. **Official .NET docs / syllabus** (when unsure) — confirm you are not missing commonly taught members

### Agent depth map (planning only — never paste into source)

Assign each subtopic **FULL / PREVIEW / DEFER** while planning the chapter.

#### FULL — teach completely here

- Numbered **section comment** + tables + compile-error notes
- **Runnable code directly below** the comment — in `Main`, a method, a class in `Program.cs`, **or the owning file** in multi-file modules
- **Every declaration used**
- **Prefer depth over brevity**

#### PREVIEW — introduce only; defer depth

- One concise section comment + minimal code in the same file
- Forward reference: `COVERED IN DETAIL LATER → NN. Topic Folder Name`

#### DEFER — omit entirely

Outside folder name; has or will have its own topic folder.

**When unsure:** prefer a **new numbered topic folder** over bloating the current chapter.

---

## Curriculum & naming

### Create new topic folder when

- FULL content would exceed ~800 lines or ~20 full sections in one `Program.cs`
- Concept is a standard chapter on its own
- User asks to split, or gap analysis shows missing coverage

### Naming — keep aligned

| Artifact | Convention | Example |
|----------|------------|---------|
| Topic folder | `NN. Human Readable Topic` | `02. Data Types and Variables` |
| `.csproj` | PascalCase | `DataTypesAndVariables.csproj` |
| `RootNamespace` / `namespace` | Same PascalCase | `DataTypesAndVariables` |

Use **`NN.`** prefix; leave numbering gaps for inserts.

---

## File intro (required multiline blocks)

### `Program.cs` (every chapter)

1. **TOPIC**
2. **WHY IT MATTERS**
3. **WHAT YOU WILL LEARN** (numbered)
4. **Multi-file chapters only:** **CHAPTER MAP** — numbered sections with target file paths (e.g. `3. SqlCommand with parameters → Repositories/OrderRepository.cs`)

### Other files (multi-file chapters)

Each `.cs` file that holds lesson code gets a **file-level header** at the top:

1. **FILE ROLE** — what this file teaches in one sentence
2. **SECTIONS IN THIS FILE** — numbered list matching the chapter map in `Program.cs`

Section comments (`SECTION N:`) still go **immediately above** the code they explain inside that file.

**Never include in source code:** COVERAGE / FULL / PREVIEW / DEFER maps,
"How to use this file", IDE/run checklists, or meta-talk about the agent workflow.

---

## Comment format — two layers

Use **both** multiline and inline comments. They serve different reading speeds.

| Use | For |
|-----|-----|
| Multiline `/* */` | File intro, `SECTION N:`, tables, tips, pitfalls, `--- Na. Subsection ---` |
| Inline `//` | Individual code lines **inside** a class, method, or `Main` block |
| Never `#` | Invalid in C# |

### Multiline blocks (concepts and structure)

- One block **immediately above** each section, type, or multi-step demo group
- Teach the *why*, terminology, tables, and forward refs here
- Do **not** repeat the whole block line-by-line in inline comments

### Inline comments (reading while scrolling code)

Add `//` on code lines where the reader benefits from a quick hint **at that line**:

- Non-obvious assignments, operators, or API calls (`this.`, `?.`, `??`, `as`, `ReferenceEquals`)
- What a constructor, method call, or field read **does** in one short phrase
- Group headers inside a type or `Main` (e.g. `// Instance fields — each object gets its own copy`)

**Do not:**

- Put a blank line between every code line (see **File formatting** below)
- Comment every trivial line (`i++`, empty `Console.WriteLine()`, obvious `return`)
- Duplicate the full section block text in inline comments
- Replace section blocks with inline-only comments — keep both layers

### Inline comment density

| Location | Guidance |
|----------|----------|
| Type fields / ctor / methods | Comment first field group, ctor purpose, and non-obvious lines |
| `Main` demo wiring | Comment each logical step; key output lines where behavior matters |
| Self-explanatory boilerplate | Leave uncommented |

## File formatting

- **Normal spacing only** — blank lines between methods, sections, and logical groups; **not** between every line
- A ~550-line chapter must stay ~550 lines, not ~1100 from doubled spacing
- After writing or editing `Program.cs`, verify line count is reasonable and the file does not have a blank line after every line

### Windows line endings (agent writes)

On Windows, some editors/tools can corrupt saves to `\r\r\n`, which inserts a visible blank line between every line and doubles file length. After writing a tutorial file:

1. Read back a sample of the file — lines should be consecutive, not separated by empty lines
2. If corrupted, normalize to `\r\n` only (e.g. replace `\r\r\n` → `\n`, then normalize to `\r\n`)
3. Run `dotnet build` to confirm the file still compiles

## Section structure inside `Program.cs`

1. File intro blocks
2. `using` statements
3. `namespace`
4. **Section comments and code interleaved top to bottom** — types, helpers, then `Program` with `Main`, **or** `Program` first then types if `Main` references them (C# allows either order within the same file)
5. **Quick reference** cheat sheet at file end (outside types / outside `Main`)

Rules:

- Explanation **immediately above** the code it describes — same file, no forward jumps
- Subsections for multi-part sections (e.g. `--- 4a. Auto-properties ---`)
- `Main` calls into the types and methods defined above or below — keeps the demo runnable

---

## Code requirements

- Compiles and runs — **0 warnings**
- No `#pragma warning disable`
- No illustration-only variables — every executable line **used**
- One **cohesive runnable program** wired through `Main` (console) or startup (web)
- **Single-file chapters:** classes, records, enums, and helpers live in `Program.cs` with section comments
- **Multi-file chapters:** each type and method lives in the appropriate folder with section comments in **that file** — `Program.cs` stays thin (intro + wiring)
- `Console.WriteLine` / output only from the real demo, not narration spam

## Do not use (unless topic is explicitly about that)

- Runtime menus, "Run All Demos", teaching-only `*Demo.cs`
- `Models/` splits for **console** chapters when the type is part of the lesson (merge back into `Program.cs`)
- Bare extra files with no section comments in **data-access or web** modules
- Scope creep beyond folder name
- Over-engineering — only create folders the chapter actually teaches

---

## `.csproj` defaults (all tutorial projects)

```xml
<TargetFramework>net8.0</TargetFramework>
<ImplicitUsings>disable</ImplicitUsings>
<Nullable>enable</Nullable>
<OutputType>Exe</OutputType>
```

**Skeleton — section comment above each layer + inline hints on code lines:**

```csharp
using System;

namespace PropertiesAndIndexers;

/*
 * SECTION 3: AUTO-IMPLEMENTED PROPERTIES
 * ...
 */
public class Book
{
    public string Title { get; set; } = string.Empty; // auto-property with default
    public int PageCount { get; init; }                 // init-only — set at construction
}

public class Program
{
    /*
     * SECTION 4: DEMONSTRATION — Main orchestrates the chapter demo
     * ...
     */
    public static void Main(string[] args)
    {
        Book book = new Book { Title = "Clean Code", PageCount = 464 }; // object initializer
        Console.WriteLine($"{book.Title}: {book.PageCount} pages");     // Title/PageCount via properties
    }
}

/*
 * QUICK REFERENCE
 * ...
 */
```

- Explicit `using` at top; only what the file needs
- First namespace lesson: dedicated section on `using` + why ImplicitUsings is disabled

**Multi-file skeleton — ADO.NET / Dapper / web chapters:**

```
Program.cs          → TOPIC / WHY / LEARN + CHAPTER MAP + thin Main or startup
Models/Product.cs   → FILE ROLE + SECTION comments above types
Repositories/       → SECTION comments above data-access methods
Utils/              → SECTION comments above helpers
```

```csharp
// Program.cs — intro + map + wiring only
/*
 * TOPIC: SqlCommand with parameters
 * ...
 * CHAPTER MAP:
 *   1. Product row type        → Models/Product.cs
 *   2. Parameterized queries   → Repositories/ProductRepository.cs
 *   3. Demo                    → Program.cs Main
 */
public class Program
{
    public static void Main(string[] args)
    {
        var repo = new ProductRepository(ConnectionHelper.GetConnectionString());
        foreach (Product p in repo.GetAll()) // calls repository from ch.2
            Console.WriteLine($"{p.Id}: {p.Name}");
    }
}
```

```csharp
// Models/Product.cs — section comment lives with the type
/*
 * SECTION 1: PRODUCT ROW TYPE
 * Maps to dbo.Products columns returned by SqlDataReader.
 */
public sealed class Product
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
```

```csharp
// Repositories/ProductRepository.cs — section comment lives with the ADO.NET code
/*
 * SECTION 2: PARAMETERIZED SqlCommand
 * ...
 */
public sealed class ProductRepository
{
    public IEnumerable<Product> GetAll() { /* SqlConnection, SqlCommand, reader */ }
}
```

---

## Compile errors to mention in comments

CS0165 (unassigned), CS0103 (missing using), CS1012 (char vs string), CS0017 (multiple entry points), `FormatException` vs `TryParse`, bad unbox `InvalidCastException`.

---

## Workflow — creating a new tutorial

1. Read **folder name**, **module README**, and **sibling chapters** — list every subtopic at FULL depth
2. **Choose layout:** console → single `Program.cs`; ADO.NET / Dapper / EF Core / ASP.NET → multi-file folders
3. Plan **section order** and **file ownership** — comment block then code for each section in the file that owns it
4. Write **`Program.cs` intro** (+ chapter map for multi-file)
5. Write **other files** with file headers and section comments above their code
6. Wire demo in `Main` or startup — thin orchestration only for multi-file
7. Add quick reference at end of `Program.cs` (and optionally a one-line pointer in other files)
8. Run `dotnet build` — fix until 0 warnings
9. If scope exceeds ~800 lines total → propose a new `NN.` folder, not unexplained file dumps

## Workflow — improving legacy tutorials

### Console / single-file chapters

1. Gap analysis: missing APIs; explanation separated from code; types stranded in `Models/`
2. **Merge** `Models/*.cs` (and similar) into `Program.cs` — move each type's section comment **above that type**
3. Shorten `Main` to orchestration; move concept comments out of `Main` onto the types/methods they teach
4. Expand thin sections; add subsections for related API families
5. Add inline `//` comments on non-obvious code lines (keep multiline section blocks)
6. Disable ImplicitUsings; add explicit usings; align folder / csproj / namespace / `.sln`
7. Verify file spacing and line endings — no blank line between every line

### Data-access / web multi-file chapters

1. Gap analysis: logic crammed in `Program.cs`; bare files in `Models/` or `Services/` with no comments
2. **Split by responsibility** — move data access to Repositories/Services, types to Models, helpers to Utils
3. Add **file headers** and **section comments** in every file that holds lesson code
4. Add **chapter map** to `Program.cs` intro if missing
5. Trim `Program.cs` to intro + wiring; ensure comments live next to the code they explain
6. `dotnet build` — 0 warnings; verify every file in the chapter map is commented

---

## Finish checklist

- [ ] **Layout matches module:** single `Program.cs` for console; proper folders for ADO.NET / Dapper / EF Core / ASP.NET
- [ ] **Multi-file:** chapter map in `Program.cs`; every lesson file has file header + section comments
- [ ] Every section comment sits **directly above** the code it explains **in the file that owns that code**
- [ ] Inline `//` comments on non-obvious lines (alongside section blocks) in all relevant files
- [ ] Normal spacing — no blank line between every code line; line count not doubled
- [ ] No bare one-liner files — types, services, and helpers have **full section comments**
- [ ] `Main` / startup orchestrates the demo — not the only place concepts are explained
- [ ] Every important concept implied by **folder name** is covered
- [ ] PREVIEW items: short treatment + forward ref
- [ ] 0 warnings; no `#pragma`; ImplicitUsings disabled
- [ ] Quick reference at end of `Program.cs`

---

## Example depth map (planning fallback)

| Subtopic | Depth | Notes |
|----------|-------|-------|
| Auto-properties | FULL | Section above `Book` class in `Program.cs` |
| readonly fields | FULL | Section above type with ctor in same file |
| string | PREVIEW | → future **Strings** folder |
| dynamic | DEFER | own folder |

For File I/O: teach the full `File` text helper family at FULL in **File and Directory Operations**; stream control in **StreamReader and StreamWriter**.

---

## Additional reference

See [reference.md](reference.md).

After reading chapters are in place, use **`@hands-on-practice`** at:

```
00. Notes & Practice/01. Practice/{CurriculumModule}/{ReadingModule}/
```

Derive practice coverage from **reading chapter folders and `Program.cs` content**.
