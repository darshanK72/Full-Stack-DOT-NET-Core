# 10. Managed and Unmanaged Code

## Status
`Not Started`

## Purpose
Contrast CLR-managed execution with native/unmanaged code and when each is used.

---

## In-depth content

### Managed code

- Runs under **CLR** supervision  
- Memory via **GC**; pointers not exposed by default  
- Exceptions, type safety, JIT  

All normal C# in this curriculum is **managed**.

### Unmanaged code

- Native CPU instructions outside CLR  
- Manual memory (C/C++), OS APIs  
- Used for performance, legacy libraries, drivers  

### Interop boundaries

| Mechanism | Use |
|-----------|-----|
| **P/Invoke** | Call native DLLs (`DllImport`) |
| **COM interop** | Legacy Windows COM |
| **C++/CLI** | Bridge native and managed |
| **`unsafe` / pointers** | Low-level within C# (requires `/unsafe`) |

### Resource management

Managed objects → GC. **Unmanaged resources** (file handles, sockets) → **`IDisposable`** pattern in C#.

Detail: **02. OOP** (Dispose), **07. File I/O**.

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| Managed vs unmanaged definition | FULL | |
| GC vs manual memory | FULL | |
| P/Invoke overview | PREVIEW | Diagram only |
| unsafe keyword | PREVIEW | Mention, don't require |
| IDisposable | PREVIEW | → 02. OOP, 07. Files |

## Syllabus mapping

- Managed and Unmanaged Code in .NET Framework  
