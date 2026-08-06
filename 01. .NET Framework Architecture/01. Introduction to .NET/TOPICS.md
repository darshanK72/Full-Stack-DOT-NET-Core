# 03. Introduction to .NET

## Status
`Not Started`

## Purpose
Clarify what ".NET" means today and which application types you can build with C#.

---

## In-depth content

### Evolution of .NET

| Name | Era | Notes |
|------|-----|-------|
| **.NET Framework** | 2002– | Windows-centric; full framework on Windows |
| **.NET Core** | 2016–2020 | Cross-platform, open source, modular |
| **.NET 5+** | 2020– | Unified branding: .NET 6, 7, 8, 9… |
| **.NET 8 (LTS)** | Current target for this repo | Long-term support |

This repository targets **`net8.0`** unless a legacy sample notes otherwise.

### What is .NET?

**.NET** is a **free, cross-platform developer platform** for building many application types, with:

- **Languages** — C#, F#, VB.NET  
- **Runtime** — CLR (CoreCLR on .NET Core+)  
- **Base Class Library (BCL)** — built-in APIs (`System.*`)  
- **SDK & tooling** — `dotnet` CLI, MSBuild, NuGet  

### Types of applications in C# / .NET

| Type | Description | Typical stack |
|------|-------------|---------------|
| **Console** | Text-based; learning & utilities | This curriculum |
| **Web (ASP.NET Core)** | APIs, MVC, Razor Pages | Later modules |
| **Desktop** | WPF, WinForms, MAUI | Optional track |
| **Mobile** | .NET MAUI | Optional track |
| **Cloud / microservices** | Containers, Azure | Later modules |
| **Libraries (DLL)** | Reusable code | Class libraries |

### Introduction to C# as a language

- **Statically typed** — types checked at compile time  
- **Object-oriented** — classes, interfaces, inheritance  
- **Modern & evolving** — records, pattern matching, nullable reference types  
- **Runs on .NET** — not tied to Windows only  

Full language tutorial: **01. C# Language Fundamentals**.

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| .NET Framework vs .NET Core vs .NET 8 | FULL | Timeline table |
| Platform components overview | PREVIEW | → chapter 04 |
| Application types | FULL | |
| Why C# for enterprise / full stack | FULL | |
| C# syntax and keywords | DEFER | → 01. C# Language Fundamentals |

## Syllabus mapping

- Introduction to .NET Framework  
- Different Types of Applications  
- Introduction to C# Programming Language  
