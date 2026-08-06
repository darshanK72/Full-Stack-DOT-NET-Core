# 01. C# & LINQ — Curriculum Index

This folder is the **C# language and LINQ** track of the Full Stack .NET Core learning repository.

## How to use this curriculum

Every **module** has a `README.md`. Every **chapter/topic folder** has a `TOPICS.md` that tells you (and AI agents) exactly what to cover when building `Program.cs` or related projects.

| File | Purpose |
|------|---------|
| `README.md` (module) | Chapter list, prerequisites, module goals |
| `TOPICS.md` (chapter) | Subtopics, depth (FULL / PREVIEW / DEFER), syllabus mapping, implementation status |

When creating or editing a tutorial chapter, **read `TOPICS.md` first**, then follow `@reading-tutorial` skill conventions.

## Module map

| # | Module | Focus |
|---|--------|-------|
| 00 | [.NET Framework Architecture](./00.%20.NET%20Framework%20Architecture/README.md) | How computers work, .NET platform, CLR, IL, assemblies, GAC |
| — | [Previous Practice Codes](./00.%20Previous%20Practice%20Codes/) | Legacy Capgemini, HackerRank, interview mocks (not tutorial chapters) |
| 01 | [C# Language Fundamentals](./01.%20C%23%20Language%20Fundamentals/README.md) | Syntax, types, control flow, methods, arrays, exceptions |
| 02 | [Object Oriented Programming](./02.%20Object%20Oriented%20Programming/README.md) | Classes, inheritance, polymorphism, interfaces, events |
| 03 | [Generics & Collections](./03.%20Generics%20%26%20Collections/README.md) | Generics, lists, dictionaries, non-generic & concurrent collections |
| 04 | [Functional Style Programming](./04.%20Functional%20Style%20Programming/README.md) | Delegates, lambdas, extension methods |
| 05 | [Language Integrated Query](./05.%20Language%20Integrated%20Query/README.md) | LINQ operators, grouping, joins, PLINQ |
| 06 | [Multithreading & Async Programming](./06.%20Multithreading%20%26%20Async%20Programming/README.md) | Threads, tasks, async/await, parallel, synchronization |
| 07 | [File Input & Output and Streams](./07.%20File%20Input%20%26%20Outpout%20and%20Streams/README.md) | File, stream, binary, CSV I/O |
| 08 | [Advanced C# Features](./08.%20Advanced%20C%23%20Features/README.md) | Serialization, reflection, memory, regex, C# 7/8, var/dynamic |
| 09 | [Testing](./09.%20Testing/README.md) | Unit testing, xUnit, MSTest, mocking |

## Depth legend (used in TOPICS.md)

| Tag | Meaning |
|-----|---------|
| **FULL** | Teach completely in this chapter's `Program.cs` |
| **PREVIEW** | Brief intro + forward reference to another folder |
| **DEFER** | Do not teach here; see linked chapter |

## Practice folders

Hands-on exercises live in each module's `00. Practice/` or chapter-level `Practice/` folders. Use `@hands-on-practice` skill after completing a reading tutorial chapter.
