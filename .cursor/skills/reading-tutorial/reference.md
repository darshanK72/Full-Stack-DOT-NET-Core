# Reading Tutorial — Extended Reference

Companion to [SKILL.md](SKILL.md). Primary instructions live in [SKILL.md](SKILL.md).

---

## Two layout modes

| Mode | Modules | Rule |
|------|---------|------|
| **Single-file** | C# & LINQ, console fundamentals | Entire chapter in `Program.cs` |
| **Multi-file** | ADO.NET, Dapper, EF Core, ASP.NET | Realistic folders; comments in **every** lesson file |

**Comments follow the code** — in both modes, each section block sits immediately above the code it explains. Multi-file chapters do not cram explanations into `Program.cs`.

---

## Single-file chapter layout (console / C#)

Console / C# & LINQ chapters use **one `Program.cs`**. The reader never needs to open `Models/` or hunt for unexplained types at the bottom.

```
┌─────────────────────────────────────────┐
│  File intro (TOPIC / WHY / LEARN)       │
├─────────────────────────────────────────┤
│  using …                                │
│  namespace …                            │
├─────────────────────────────────────────┤
│  SECTION 1 comment                      │
│  code (Main fragment or helper)         │
├─────────────────────────────────────────┤
│  SECTION 2 comment                      │
│  public class Example { … }             │  ← comment above the type
├─────────────────────────────────────────┤
│  SECTION 3 comment                      │
│  public class Program { Main … }        │  ← Main wires the demo
├─────────────────────────────────────────┤
│  QUICK REFERENCE                        │
└─────────────────────────────────────────┘
```

**Rule:** If a section teaches `Book`, the `SECTION N:` block is **immediately above** `class Book`. `Main` creates a `Book` and prints — it does not replace the type's section.

---

## Multi-file chapter layout (data access & web)

ADO.NET, Dapper, EF Core, and ASP.NET chapters use **proper project structure**. Teaching content is spread across all relevant files.

```
┌─────────────────────────────────────────┐
│  Program.cs                             │
│  TOPIC / WHY / LEARN + CHAPTER MAP      │
│  thin Main or startup wiring            │
├─────────────────────────────────────────┤
│  Models/Product.cs                      │
│  FILE ROLE + SECTION 1 above type       │
├─────────────────────────────────────────┤
│  Repositories/ProductRepository.cs      │
│  FILE ROLE + SECTION 2 above methods    │
├─────────────────────────────────────────┤
│  Utils/ConnectionHelper.cs              │
│  FILE ROLE + SECTION 3 above helper     │
├─────────────────────────────────────────┤
│  (ASP.NET) Controllers/ProductsController │
│  FILE ROLE + SECTION N above endpoints  │
└─────────────────────────────────────────┘
```

### Typical folders

| Folder | Use |
|--------|-----|
| `Models/` | Entities, DTOs, row types mapped from SQL |
| `Repositories/` or `Services/` | ADO.NET, Dapper, or business logic |
| `Utils/` or `Helpers/` | Connection strings, mappers, extensions |
| `Data/` | DbContext, migrations context (EF Core) |
| `Controllers/` or endpoint files | ASP.NET routes and handlers |

Create only folders the chapter actually teaches — no empty layers.

### File headers (multi-file)

Every lesson `.cs` file (not just `Program.cs`):

```
/*
 * FILE ROLE: ...
 * SECTIONS IN THIS FILE:
 *   1. ...
 *   2. ...
 */
```

Then `SECTION N:` blocks immediately above the code inside that file.

### Chapter map (in Program.cs intro only)

```
 * CHAPTER MAP:
 *   1. Row type           → Models/Product.cs
 *   2. Parameterized query → Repositories/ProductRepository.cs
 *   3. Demo               → Program.cs Main
```

This is curriculum navigation — not a "how to run" or IDE checklist.

---

## Comment layers (multiline + inline)

| Layer | Role |
|-------|------|
| `/* SECTION N: … */` above a block | Structure, concepts, tables, pitfalls |
| `//` on code lines inside the block | Quick hints while reading line-by-line |

Use **both** in **every** file that holds lesson code. Section blocks teach the concept; inline comments explain non-obvious lines at the point of use. Do not add a blank line between every code line.

After agent edits on Windows, verify the file was not saved with `\r\r\n` (doubles visible line count). Normalize to `\r\n` if needed.

---

## Coverage planning (no TOPICS.md)

| Source | Use |
|--------|-----|
| **Topic folder name** | Primary scope — teach every important API/pattern for that title |
| **Module `README.md`** | Chapter list; grouped vs split topics |
| **Sibling `NN.` folders** | Avoid FULL duplication; PREVIEW + forward refs |
| **Existing source files** | Gaps; bare splits to comment; comments far from code |
| **.NET docs / syllabus** | Missing API families |

**Depth goal:** Complete chapters. Missing APIs or unexplained code in any file (single or multi-file) is a gap.

---

## Scope rule

Cover **only** what the **folder name** describes. Within that scope, cover **related API families completely**.

Do not add command-line args or `.csproj` deep dives unless the folder name requires it.

Multi-file structure is required when the module teaches real project layout (ADO.NET, Dapper, EF Core, ASP.NET) — not optional convenience.

---

## ImplicitUsings — hidden global usings table (net8.0 console)

When `<ImplicitUsings>enable</ImplicitUsings>`, SDK generates `obj/GlobalUsings.g.cs`:

| Namespace | Typical types |
|-----------|---------------|
| System | Console, String, Exception, Convert |
| System.Collections.Generic | List, Dictionary |
| System.IO | File, Stream, Path |
| System.Linq | LINQ extensions |

This repo uses **disable** so imports stay visible in source.

---

## readonly / property patterns

When FULL depth requires `readonly` fields or property patterns, the section comment goes **above the type** — in `Program.cs` for console chapters, or in `Models/` for multi-file chapters.

---

## Migrating legacy tutorials

### Console chapters (merge splits)

1. Copy type bodies into `Program.cs`
2. Move or write **section comments above each type**
3. Trim `Main` to demo wiring only
4. Delete empty `Models/` folder
5. `dotnet build` — 0 warnings

### Data-access / web chapters (split and comment)

1. Identify logic crammed into `Program.cs`
2. **Split** into Models, Repositories/Services, Utils as appropriate
3. Add **file headers** and **section comments** in every lesson file
4. Add **chapter map** to `Program.cs` intro
5. Trim `Program.cs` to intro + wiring
6. `dotnet build` — 0 warnings

---

## Module-specific guidance

| Module | Layout | Notes |
|--------|--------|-------|
| C# Language Fundamentals | Single-file | Merge any stray `Models/` back in |
| ADO.NET | Multi-file | Repositories for SqlConnection/Command/Reader; Models for row types |
| Dapper | Multi-file | Repositories for Query/Execute; Models for mapped types |
| EF Core | Multi-file | Data/ for DbContext; Models/ for entities; Services/ optional |
| ASP.NET Core | Multi-file | Controllers/endpoints, Services, Models, DI in Program.cs |

FULL / PREVIEW / DEFER maps are agent planning only — never written into source files.
