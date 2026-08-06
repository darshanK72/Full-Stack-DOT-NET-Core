# 13. GAC Strong Names and DLL Hell

## Status
`Not Started`

## Purpose
Explain strong naming, the Global Assembly Cache, DLL Hell, and how modern .NET avoids these problems.

---

## In-depth content

### DLL Hell

**DLL Hell** — applications break when a shared DLL is replaced with an incompatible version. Symptoms: missing entry points, subtle runtime failures.

### Strong-named assemblies

A **strong name** = assembly name + version + culture + **public key** + digital signature.

```bash
sn -k keypair.snk
# Reference in .csproj for signing
```

Ensures identity; required for GAC in Framework era.

### Global Assembly Cache (GAC)

**GAC** — machine-wide store for shared .NET Framework assemblies (e.g. `System.*` at specific versions).

Install with `gacutil` (legacy tooling). **Not used** for typical .NET Core / .NET 8 app deployment.

### Modern solutions

| Problem | Modern fix |
|---------|------------|
| Version conflicts | NuGet binding, isolated deployment |
| Shared libraries | NuGet packages, not GAC |
| Side-by-side | Self-contained publish, API versioning |
| Framework updates | Target `net8.0`; pin package versions |

**Binding redirects** still appear in some .NET Framework configs — know for maintenance interviews.

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| DLL Hell definition and causes | FULL | |
| Strong naming steps | FULL | Conceptual + sn.exe |
| GAC purpose and limitations | FULL | Historical |
| Why GAC faded | FULL | |
| NuGet + SDK-style projects | FULL | Modern default |
| Binding redirects | PREVIEW | Framework config only |

## Syllabus mapping

- Strong and Weak Assemblies in .NET Framework  
- How to Install an Assembly into GAC  
- DLL Hell Problem and Solution  
