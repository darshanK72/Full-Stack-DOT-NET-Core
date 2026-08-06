# Reading Tutorial — Extended Reference

Companion to [SKILL.md](SKILL.md). Primary instructions live in [SKILL.md](SKILL.md).

---

## TOPICS.md (required before any tutorial work)

**Path:** `NN. Topic Name/TOPICS.md` (same folder as `Program.cs`)

| Section | Purpose |
|---------|---------|
| Status | Not Started / In Progress / Complete |
| Purpose | Chapter goal |
| Subtopics to cover | Table with Depth: FULL, PREVIEW, DEFER |
| Program.cs requirements | Extra deliverables for this chapter |
| Syllabus mapping | Course outline alignment |

**Agent rule:** Read TOPICS.md first. Its depth table overrides generic examples in the skill. Format rules (comments, ImplicitUsings, no menus) still apply. Do not paste TOPICS.md content into Program.cs as a COVERAGE block.

If TOPICS.md is missing, propose creating one before a large write.

---

## Scope rule (original prompt)

Cover **only** what the **folder name** describes. Do not add command-line args, build pipeline, `.csproj` deep dives, top-level statements comparisons, or helper classes unless essential to that folder name.

## Original deliverable wording

One main tutorial file (usually `Program.cs`) that reads top-to-bottom like a **textbook chapter in code** and runs correctly when executed.

## ImplicitUsings — hidden global usings table (net8.0 console)

When `<ImplicitUsings>enable</ImplicitUsings>`, SDK generates `obj/GlobalUsings.g.cs`:

| Namespace | Typical types |
|-----------|---------------|
| System | Console, String, Exception, Convert |
| System.Collections.Generic | List, Dictionary |
| System.IO | File, Stream, Path |
| System.Linq | LINQ extensions |
| System.Net.Http | HttpClient |
| System.Threading | Thread, Monitor |
| System.Threading.Tasks | Task |

This repo uses **disable** so imports stay visible in source.

## Suggested curriculum gaps (C# Language Fundamentals)

Propose when relevant; adjust numbers to fit existing folders:

- Input & Output
- Strings (full)
- Classes & Objects
- Inheritance & Polymorphism
- Interfaces
- Generics
- Collections
- Exception Handling (exists — migrate to reading style)
- dynamic / reflection

## Suggested curriculum gaps (later repo modules)

- ASP.NET Core: Minimal APIs, Routing, Middleware, DI, Configuration, Authentication, EF Core
- Each as `NN. Topic Name` with same FULL/PREVIEW/DEFER rules

## readonly demonstration pattern (Phase 2)

When FULL depth requires `readonly` fields:

```
Models/OrderConfig.cs   — readonly field set in constructor
Program.cs              — instantiates and uses in runnable output
```

Do not use `file class` at bottom of Program.cs unless unavoidable.

## API tutorial adaptation

| Console fundamentals | API module equivalent |
|---------------------|------------------------|
| Program.cs main lesson | Program.cs + endpoint files or Controllers/ |
| Console output demo | HTTP response / logged output from real handler |
| Models/ | DTOs, request/response records |
| Services/ | Business logic injected into endpoints |
| PREVIEW | e.g. auth in Routing folder → defer to Authentication folder |

Same comment format applies to `WeatherEndpoints.cs`, `Program.cs`, etc.
FULL / PREVIEW / DEFER depth maps are agent planning only — never written into source files.
