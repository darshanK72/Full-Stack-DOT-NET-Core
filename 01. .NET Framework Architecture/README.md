# 01. .NET Framework Architecture

Conceptual foundations for the .NET platform — what it is, how it evolved, and how source code becomes running software. Nine focused reading chapters with no duplicated concepts across files.

## Prerequisites

None — start here if you are new to .NET.

## Chapters

| # | Chapter | Topics |
|---|---------|--------|
| 01 | [.NET Platform and Evolution](./01.%20.NET%20Platform%20and%20Evolution.md) | .NET Framework, .NET Core, unified .NET, .NET Standard, app frameworks, modularity |
| 02 | [CLI, Architecture, and Components](./02.%20CLI%2C%20Architecture%2C%20and%20Components.md) | ECMA-335, CLI, VES, BCL/FCL, CoreCLR, SDK, NuGet |
| 03 | [CLR, JIT, AOT, and Execution Flow](./03.%20CLR%2C%20JIT%2C%20AOT%2C%20and%20Execution%20Flow.md) | CLR, JIT, AOT, tiered compilation, execution pipeline, security |
| 04 | [IL and Metadata](./04.%20IL%20and%20Metadata.md) | CIL/MSIL, stack-based IL, metadata, PE overview |
| 05 | [CTS, CLS, and Cross-Language Interoperability](./05.%20CTS%2C%20CLS%2C%20and%20Cross-Language%20Interoperability.md) | Type system, CLS rules, multi-language interop |
| 06 | [Managed and Unmanaged Code](./06.%20Managed%20and%20Unmanaged%20Code.md) | Managed vs native, P/Invoke, COM, unsafe |
| 07 | [Assemblies, Loading, Strong Naming, and GAC](./07.%20Assemblies%2C%20Loading%2C%20Strong%20Naming%2C%20and%20GAC.md) | Assemblies, EXE vs DLL, strong names, GAC, binding |
| 08 | [Garbage Collection and Memory](./08.%20Garbage%20Collection%20and%20Memory.md) | Stack vs heap, generational GC, GC modes |
| 09 | [Application Domains and Isolation](./09.%20Application%20Domains%20and%20Isolation.md) | AppDomains (historical), modern isolation alternatives |

## Topic Index

Use this index to locate where each common architecture question is answered.

| Question | Chapter |
|---|---|
| What is the .NET Framework? | [01](./01.%20.NET%20Platform%20and%20Evolution.md) |
| What is .NET Core? | [01](./01.%20.NET%20Platform%20and%20Evolution.md) |
| Difference between .NET Framework and .NET Core? | [01](./01.%20.NET%20Platform%20and%20Evolution.md) |
| Evolution Framework → Core → unified .NET | [01](./01.%20.NET%20Platform%20and%20Evolution.md) |
| What is .NET Standard and why introduced? | [01](./01.%20.NET%20Platform%20and%20Evolution.md) |
| Frameworks under older .NET Framework | [01](./01.%20.NET%20Platform%20and%20Evolution.md) |
| Frameworks under modern .NET | [01](./01.%20.NET%20Platform%20and%20Evolution.md) |
| Goals of modular architecture in .NET Core / modern .NET | [01](./01.%20.NET%20Platform%20and%20Evolution.md), [02](./02.%20CLI%2C%20Architecture%2C%20and%20Components.md) |
| Common Language Infrastructure (CLI) | [02](./02.%20CLI%2C%20Architecture%2C%20and%20Components.md) |
| Virtual Execution System (VES) | [02](./02.%20CLI%2C%20Architecture%2C%20and%20Components.md) |
| Components of .NET Core / modern .NET | [02](./02.%20CLI%2C%20Architecture%2C%20and%20Components.md) |
| CLR (Common Language Runtime) | [03](./03.%20CLR%2C%20JIT%2C%20AOT%2C%20and%20Execution%20Flow.md) |
| FCL (Framework Class Library) | [02](./02.%20CLI%2C%20Architecture%2C%20and%20Components.md) |
| BCL (Base Class Library) | [02](./02.%20CLI%2C%20Architecture%2C%20and%20Components.md) |
| IL / MSIL / CIL | [04](./04.%20IL%20and%20Metadata.md) |
| JIT compiler and relation to IL | [03](./03.%20CLR%2C%20JIT%2C%20AOT%2C%20and%20Execution%20Flow.md) |
| AOT vs JIT compilation | [03](./03.%20CLR%2C%20JIT%2C%20AOT%2C%20and%20Execution%20Flow.md) |
| Managed vs unmanaged code | [06](./06.%20Managed%20and%20Unmanaged%20Code.md) |
| CLS (Common Language Specification) | [05](./05.%20CTS%2C%20CLS%2C%20and%20Cross-Language%20Interoperability.md) |
| CTS (Common Type System) | [05](./05.%20CTS%2C%20CLS%2C%20and%20Cross-Language%20Interoperability.md) |
| Cross-language interoperability | [05](./05.%20CTS%2C%20CLS%2C%20and%20Cross-Language%20Interoperability.md) |
| Assembly and types of assemblies | [07](./07.%20Assemblies%2C%20Loading%2C%20Strong%20Naming%2C%20and%20GAC.md) |
| Role of GAC | [07](./07.%20Assemblies%2C%20Loading%2C%20Strong%20Naming%2C%20and%20GAC.md) |
| Assembly loading, binding, probing | [07](./07.%20Assemblies%2C%20Loading%2C%20Strong%20Naming%2C%20and%20GAC.md) |
| Difference between .exe and .dll | [07](./07.%20Assemblies%2C%20Loading%2C%20Strong%20Naming%2C%20and%20GAC.md) |
| Strong naming of assemblies | [07](./07.%20Assemblies%2C%20Loading%2C%20Strong%20Naming%2C%20and%20GAC.md) |
| Role of metadata | [04](./04.%20IL%20and%20Metadata.md), [07](./07.%20Assemblies%2C%20Loading%2C%20Strong%20Naming%2C%20and%20GAC.md) |
| Application Domain (AppDomain) | [09](./09.%20Application%20Domains%20and%20Isolation.md) |
| Garbage Collection | [08](./08.%20Garbage%20Collection%20and%20Memory.md) |
| Execution flow (source to running app) | [03](./03.%20CLR%2C%20JIT%2C%20AOT%2C%20and%20Execution%20Flow.md) |
| .NET runtime security (CAS, role-based) | [03](./03.%20CLR%2C%20JIT%2C%20AOT%2C%20and%20Execution%20Flow.md) |
| Modularity and NuGet | [02](./02.%20CLI%2C%20Architecture%2C%20and%20Components.md) |

## Next module

After this module, proceed to **[02. C# & LINQ / 01. C# Language Fundamentals](../02.%20C%23%20%26%20LINQ/01.%20C%23%20Language%20Fundamentals/)**.

## Syllabus source

Maps to **Introduction & Environment Setup** and **.NET Framework Architecture** on [dotnettutorials.net](https://dotnettutorials.net/course/csharp-dot-net-tutorials-for-beginners/).
