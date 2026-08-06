# 06. CLR and Program Execution

## Status
`Not Started`

## Purpose
Explain how .NET loads and runs assemblies: JIT compilation, type loading, and garbage collection at a conceptual level.

---

## In-depth content

### Common Language Runtime (CLR)

The **CLR** is the virtual machine component of .NET. Responsibilities:

| Function | Description |
|----------|-------------|
| **JIT compilation** | IL → native machine code at runtime |
| **Memory management** | Allocates objects; **GC** reclaims unused memory |
| **Type system** | Enforces CTS rules |
| **Exception handling** | Structured try/catch across languages |
| **Security** (historical) | Code access security in Framework era |
| **Threading** | Managed threads, thread pool |

### Execution process step-by-step

```
Source (.cs)
    ↓  Roslyn compiler
IL + Metadata (assembly .dll/.exe)
    ↓  Assembly Loader
Loaded types in memory
    ↓  JIT (per method, first call)
Native machine code
    ↓  CPU execution
```

**Tiered compilation** (.NET Core+): methods may be re-JIT optimized after profiling.

### Garbage Collection (overview)

- Objects on **managed heap**  
- GC finds unreachable objects and frees memory  
- Generations (0, 1, 2) for performance  
- **Finalizers** vs **`IDisposable`** — detail in **02. OOP** and **08. Memory Management**

### Stack vs heap (preview)

| | Stack | Heap |
|---|-------|------|
| Stores | Value types (local), references | Objects (reference types) |
| Lifetime | Method scope | Until GC collects |
| Speed | Very fast | Slower allocation |

Full lesson: **08. Advanced → Memory Management**.

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| CLR responsibilities | FULL | |
| Compile → IL → JIT → run pipeline | FULL | Diagram |
| Managed heap & GC overview | FULL | |
| Stack vs heap | PREVIEW | → 08. Memory Management |
| Finalize vs Dispose | PREVIEW | → 02. OOP |
| Threading internals | DEFER | → 06. Multithreading module |

## Optional demo

Use `typeof(Program).Assembly` and reflection to list loaded assembly; run under debugger and observe modules.

## Syllabus mapping

- Common Language Runtime in .NET Framework  
- .NET Program Execution Process  
- Stack and Heap Memory in .NET (intro)  
- Garbage Collection in .NET Framework (intro)  
