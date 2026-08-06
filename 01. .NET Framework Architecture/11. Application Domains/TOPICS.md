# 12. Application Domains

## Status
`Not Started`

## Purpose
Document **AppDomains** in .NET Framework history and why modern .NET uses different isolation models.

---

## In-depth content

### Application Domain (AppDomain)

In **.NET Framework**, an **AppDomain** was a CLR isolation boundary within a single OS process:

- Separate type loading and GC scopes  
- Unload without killing process (unloadable only in Framework)  
- Used for hosting plugins safely  

### Modern .NET (.NET Core / 5+)

**AppDomains are not supported for unloadable isolation** in the same way. Alternatives:

| Need | Modern approach |
|------|-----------------|
| Process isolation | Separate process / microservice |
| Plugin loading | `AssemblyLoadContext` (ALC) |
| Containers | Docker / Kubernetes |

### AssemblyLoadContext (preview)

`AssemblyLoadContext` allows loading and **unloading** collectible assemblies — advanced topic; mention for plugin scenarios.

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| AppDomain purpose (Framework) | FULL | Historical |
| Limitations and removal in Core | FULL | |
| Process vs AppDomain vs container | FULL | Comparison table |
| AssemblyLoadContext | PREVIEW | Name only + link to docs |

## Syllabus mapping

- App Domain in .NET Framework  
