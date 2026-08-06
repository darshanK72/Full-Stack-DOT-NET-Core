# 11. Assemblies DLL and EXE

## Status
`Not Started`

## Purpose
Understand the deployment unit of .NET: assemblies, manifests, and references.

---

## In-depth content

### What is an assembly?

An **assembly** is the primary unit of deployment, versioning, and security in .NET.

| Output | Typical use |
|--------|-------------|
| **`.dll`** | Class library; loaded by host |
| **`.exe`** | Executable; has entry point |

Both contain **IL + metadata + manifest**.

### Manifest contents

- Assembly identity (name, version, culture)  
- Referenced assemblies  
- Type exports  

### Versioning

```
Major.Minor.Build.Revision
```

**Strong naming** (chapter 13) binds identity for GAC scenarios.

### Project references vs NuGet

| | Project reference | NuGet package |
|---|-------------------|---------------|
| Source | Same solution | Downloaded package |
| Output | Builds dependency | Pre-built assembly |

### Deployment models (.NET Core / .NET 8)

- **Framework-dependent** — requires shared runtime  
- **Self-contained** — bundles runtime  
- **Single-file** — one executable (with caveats)  

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| DLL vs EXE | FULL | |
| Manifest and metadata | FULL | |
| Assembly references | FULL | |
| Version numbers | FULL | |
| Deployment models | FULL | Modern focus |
| GAC | PREVIEW | → chapter 13 |
| IL contents | PREVIEW | → chapter 07 |

## Optional demo

Build a class library + console host; inspect output with `dotnet build -v n` and reflection.

## Syllabus mapping

- Assembly DLL EXE in .NET Framework  
