# 04. .NET Architecture and Components

## Status
`Not Started`

## Purpose
Map the layers from your source code down to the operating system.

---

## In-depth content

### Layered architecture

```
┌─────────────────────────────────────┐
│  Your Application (C# code)         │
├─────────────────────────────────────┤
│  Base Class Library (BCL)           │  ← System.*, collections, IO, LINQ
├─────────────────────────────────────┤
│  Common Language Runtime (CLR)      │  ← JIT, GC, type system, exceptions
├─────────────────────────────────────┤
│  Operating System                   │
└─────────────────────────────────────┘
```

### Key components

| Component | Responsibility |
|-----------|----------------|
| **SDK** | Compilers, `dotnet` CLI, project templates |
| **Runtime** | Executes IL; manages memory and threads |
| **BCL / FCL** | Framework Class Library — APIs shipped with runtime |
| **NuGet** | Package manager for third-party libraries |
| **Roslyn** | C# / VB compilers |

### .NET program execution (summary)

1. Write `.cs` source files  
2. **Roslyn** compiles to **IL** in an **assembly** (`.dll` / `.exe`)  
3. **CLR** loads assembly, **JIT** compiles IL to native CPU instructions  
4. **GC** reclaims unused objects  

Deep dive: chapters **06**, **07**, **11**.

### Cross-language support

Any .NET language compiles to **IL** and runs on the **same CLR**, obeying **CTS** and **CLS** (chapters 08–09).

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| Layer diagram (app → BCL → CLR → OS) | FULL | |
| SDK vs runtime vs BCL | FULL | |
| NuGet role | PREVIEW | Used throughout repo |
| CLR internals | PREVIEW | → chapter 06 |
| IL and assemblies | PREVIEW | → chapters 07, 11 |

## Syllabus mapping

- .NET Framework Architecture and Components  
