# Karat Interview Answers — Reference

## Folder-scoped output (default)

Every generation run targets **one user-provided folder**:

```
[target-folder]/
├── KARAT_INTERVIEW_QUESTIONS.md
├── KARAT_INTERVIEW_ANSWERS.md
├── Program.cs / tutorials …   (existing reading material)
```

Question numbering **restarts at Q1** per folder file.

---

## Bulk distribute workflow

When user points at `KARAT_DEBRIEF_SOURCE.txt`:

1. Read each debrief bullet / snippet block.
2. Match to **best-fit curriculum folder** by reading folder name + tutorials — not by a fixed skill taxonomy.
3. Append or create that folder's `KARAT_INTERVIEW_*.md` pair.
4. Update `KARAT_INDEX.md` at Karat practice root (links only).

**Source file (do not delete):**

`00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt`

---

## Debrief → folder map (initial distribution)

Use as starting point; adjust when user scopes different modules.

| Debrief theme | Target folder |
|---|---|
| Kestrel, IIS, reverse proxy, forwarded headers | `05. ASP.NET Core/12. Hosting, Kestrel & Environments/` |
| Rate limiting, middleware purpose, 401 short-circuit | `05. ASP.NET Core/03. Middleware Pipeline/` |
| IEnumerable DI, ValidateScopes, singleton cart, captive dependency | `05. ASP.NET Core/04. Dependency Injection & Service Lifetimes/` |
| HTTP 201 Created | `05. ASP.NET Core/07. Routing & Endpoints/` |
| JSON naming, optional bool flags | `05. ASP.NET Core/08. Model Binding & Validation/` |
| Global exception handling, throw ex, structured logging | `05. ASP.NET Core/10. Exception Handling/` |
| IConfiguration, IOptions / Snapshot / Monitor | `05. ASP.NET Core/05. Configuration & Options Pattern/` |
| `.Result`, HttpClient lifetime, async recursion | `02. C# Language Fundamentals/06. Multithreading & Async Programming/` |
| Broken repository service | `02. C# Language Fundamentals/02. Object Oriented Programming/` |
| Dictionary ContainsKey pattern | `02. C# Language Fundamentals/03. Generics & Collections/04. Dictionary/` |
| Mock HTTP, test doubles, test pyramid | `02. C# Language Fundamentals/09. Unit Testing/` |
| static cache in singleton | `05. ASP.NET Core/04. Dependency Injection & Service Lifetimes/` |
| Live spec reading (Library LMS) | `00. Notes & Practice/04. Citi Karat Interview Practice/17. Library Management System/` |

---

## Index file format

Path: `00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_INDEX.md`

```markdown
# Karat Layer 2 — Question Index

Links to folder-scoped banks. Raw debrief: [KARAT_DEBRIEF_SOURCE.txt](./KARAT_DEBRIEF_SOURCE.txt)

| Folder | Questions | Answers |
|---|---|---|
| `05. ASP.NET Core/03. Middleware Pipeline/` | [Q](./../../05.%20ASP.NET%20Core/03.%20Middleware%20Pipeline/KARAT_INTERVIEW_QUESTIONS.md) | [A](./../../05.%20ASP.NET%20Core/03.%20Middleware%20Pipeline/KARAT_INTERVIEW_ANSWERS.md) |
```

Use relative links from index location.

---

## Issue scan order (Type R)

1. Compile / syntax  
2. Runtime / correctness  
3. Async / threading  
4. DI / lifetime  
5. HTTP / API contract  
6. Design / testability  

---

## Layer comparison (include in index intro)

Foundation `INTERVIEW_QUESTIONS.md` = recall.  
This skill = judgment under realistic code.  
Karat `.cs` practice projects = timed spec execution.

---

## Relation to `@interview-answers`

| | interview-answers | karat-interview-answers |
|---|---|---|
| Scope | Module `INTERVIEW_QUESTIONS.md` | User folder path |
| Headings | Mirror module chapters | Mirror **folder path** in header only |
| Answer body | Bullets + optional table | **+ Issues table + production takeaway** for (R) |
| Question style | Definitions & gotchas | Snippets & deployment scenarios |

Do not merge the two skills into one file.
