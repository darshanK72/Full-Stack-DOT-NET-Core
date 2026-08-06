# 07. Intermediate Language (IL)

## Status
`Not Started`

## Purpose
Understand IL as the common output of .NET compilers and how to inspect it.

---

## In-depth content

### What is IL?

**Intermediate Language** (formerly MSIL/CIL) is a CPU-independent instruction set for .NET. All .NET languages compile to IL stored in assemblies.

Benefits:
- **Language interoperability** — C# and F# share libraries  
- **Platform portability** — JIT targets current OS/CPU  
- **Metadata** — rich type info embedded in assembly  

### Metadata

Assemblies contain **metadata** describing:
- Types, methods, fields  
- Attributes, visibility  
- Assembly references  

Reflection APIs read this at runtime (**08. Reflection**).

### Inspecting IL

| Tool | Usage |
|------|-------|
| **ILDASM** | Legacy GUI disassembler (Framework SDK) |
| **ilspycmd / dotPeek / dnSpy** | Third-party decompilers |
| **`dotnet tool`** | e.g. `ilspycmd` NuGet global tool |

Example workflow:

```bash
dotnet build
# Open MyApp.dll in ILSpy — compare C# source to IL
```

### Sample IL concepts

| IL | Meaning (simplified) |
|----|----------------------|
| `ldstr "Hi"` | Load string constant |
| `call` | Call method |
| `ret` | Return |
| `box` | Box value type to object |

**Boxing/unboxing** in C# maps to IL instructions — full lesson in **08. Memory Management**.

### ILASM

**ILASM** assembles `.il` text into an assembly (rare in app dev; useful for learning).

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| Why IL exists | FULL | |
| Metadata purpose | FULL | |
| Viewing IL with decompiler | FULL | Step-by-step |
| IL instruction samples | FULL | Map to C# constructs |
| ILASM | PREVIEW | Mention only |
| Boxing IL | PREVIEW | → Memory Management |

## Syllabus mapping

- Intermediate Language (ILDASM & ILASM) Code in C#  
