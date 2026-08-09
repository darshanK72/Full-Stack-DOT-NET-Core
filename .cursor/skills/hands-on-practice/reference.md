# Hands-On Practice — Reference

Companion to [SKILL.md](SKILL.md).

---

## Module layout

```
00. Notes & Practice/01. Practice/
└── 01. C# & LINQ/
    └── 01. C# Language Fundamentals/
        ├── README.md
        ├── 01. Scenarios.md
        ├── 02. DebugReading.md
        ├── 03. COVERAGE.md
        ├── LanguageFundamentals.sln
        └── Problems/
            ├── MiniInventory/
            │   ├── PROBLEM.md
            │   ├── EVALUATION.md
            │   ├── MiniInventory.csproj
            │   └── Program.cs
            └── ClinicAppointmentManager/
                └── ...
```

**Path rule:** `01. Practice/{CurriculumModule}/{ReadingModule}/` — names must match the reading tutorial tree.

---

## Numbered root files

| # | Filename | Role |
|---|----------|------|
| — | `README.md` | Bank index, workflow, solution link |
| 01 | `01. Scenarios.md` | Design / predict / plan questions |
| 02 | `02. DebugReading.md` | Bug find / output predict |
| 03 | `03. COVERAGE.md` | Reading chapter matrix + Problems index |
| — | `{ModuleShortName}.sln` | All problem projects linked |
| — | `Problems/` | Build projects (folder, not numbered) |

---

## `{Name}.csproj` template

Filename: `Problems/{Name}/{Name}.csproj` (PascalCase matches folder name).

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

Create with `dotnet new console -n {Name} -f net8.0 --use-program-main` then set `ImplicitUsings` to `disable`, or write the file directly.

---

## `Program.cs` scaffold template

One file per problem (all types in one file unless spec is very large). **Do not** ship working implementations.

```csharp
/*
 * PROBLEM: {Title from PROBLEM.md}
 *
 * {2–4 sentences — business context from PROBLEM.md}
 *
 * This exercise covers:
 *   ch02 — {concept and why it appears here}
 *   ch06 — {concept}
 *   ch07 — {concept}
 */

using System;
using System.Collections.Generic;

namespace {DomainNamespace}
{
    /*
     * {Role of this type in the domain.}
     *
     * {Constraints from PROBLEM.md — e.g. no Console I/O in this class.}
     */
    class {EntityName}
    {
        public int Id { get; set; }

        /*
         * {What this method does.}
         *
         * {Validation rules and exceptions.}
         * {Return value meaning.}
         */
        public bool DoWork(string input, out string errorMessage)
        {
            // TODO: {requirement from PROBLEM.md in one line}
            throw new NotImplementedException();
        }
    }

    /*
     * Entry point. Owns all Console I/O.
     */
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: {demo step 1 from PROBLEM.md}
            // TODO: {demo step 2}
            throw new NotImplementedException();
        }
    }
}
```

### Comment checklist (per problem)

| Location | Must include |
|----------|----------------|
| File header | Title, context, chapter coverage list |
| Each type | Domain role + architectural constraints |
| Each method/ctor/indexer | Behavior, validation, return/out semantics |
| Each TODO | One-line instruction tied to `PROBLEM.md` |
| `Main` | Numbered demo steps from spec's "Demo Main" / workflow section |

### Compile rules

| Member kind | Scaffold rule |
|-------------|---------------|
| Readonly property / field | Assign in constructor from parameters |
| Simple auto-property | Declare; no TODO unless setter has validation |
| Method with logic | `// TODO:` + `throw new NotImplementedException();` |
| `Main` | TODO comments + `throw new NotImplementedException();` |
| Static constructor | May throw until implemented (type not loaded until called) |

After writing all problems: `dotnet build {ModuleShortName}.sln` — zero errors required.

---

## Bank solution

From the reading module practice folder:

```powershell
dotnet new sln -n {ModuleShortName} -o .
dotnet sln add Problems/MiniInventory/MiniInventory.csproj
# … repeat for each problem
dotnet build {ModuleShortName}.sln
```

Name examples: `LanguageFundamentals.sln`, `ObjectOrientedProgramming.sln`.

---

## README.md excerpt (Problems workflow)

```markdown
| [03. COVERAGE.md](03.%20COVERAGE.md) | Topic matrix |
| [{ModuleShortName}.sln]({ModuleShortName}.sln) | All build projects |

Each problem folder includes a **starter `.csproj`** and **`Program.cs`** with class
shells and `// TODO:` stubs — implement the TODOs, then evaluate with that folder's
`EVALUATION.md`. Open `{ModuleShortName}.sln` to build all projects at once.
```

---

## 01. Scenarios.md skeleton

```markdown
# {Reading Module Name} — Scenarios

Answer without running code unless asked. Check **Answers** at the end.

---

## Question 1

{Neutral scenario — multi-step, real domain}

---

## Answers

### Question 1
{Full answer with chapter concepts}
```

---

## 02. DebugReading.md skeleton

```markdown
# {Reading Module Name} — Debug Reading

Find the bug or predict output. Check **Answers** at the end.

---

## Question 1

​```csharp
// snippet
​```

---

## Answers

### Question 1
{Fix + CS#### + explanation}
```

---

## PROBLEM.md / EVALUATION.md

Unchanged: front matter, domain story, entities, requirements, rubric /100, AI prompt, checklist.

`PROBLEM.md` is the **source of truth** for type names, members, and demo steps — the `Program.cs` scaffold must mirror it exactly.

---

## 03. COVERAGE.md excerpt

```markdown
| Subtopic | Ch | Scenarios | Debug | Problems |
|----------|-----|-----------|-------|----------|
| Integer division | 04 | Q8 | Q5 | MiniInventory |
```

Audit section should note starter projects + solution:

```markdown
- [x] Starter `.csproj` + `Program.cs` scaffold per problem
- [x] `{ModuleShortName}.sln` links all build projects
```

Link files: `[01. Scenarios.md](01.%20Scenarios.md)`.

---

## Legacy style sources

- `00. Notes & Practice/04. Citi Karat Interview Practice/`
- `00. Notes & Practice/02. Capgemini Exam Practice/`

Reference only — not syllabus scope.

## Live scaffold examples

| Module | Example path |
|--------|----------------|
| Language Fundamentals | `Problems/MiniInventory/Program.cs`, `Problems/ExpenseTracker/Program.cs` |
| Object Oriented Programming | `Problems/ClinicBillingRegistry/Program.cs`, `Problems/BankAccountLedger/Program.cs` |
