# 09. Common Language Specification (CLS)

## Status
`Not Started`

## Purpose
Explain rules that let .NET languages call each other's libraries reliably.

---

## In-depth content

### What is CLS?

The **Common Language Specification** is a subset of CTS rules. If a public API is **CLS-compliant**, any .NET language can consume it.

### CLS rules (common examples)

| Rule | Reason |
|------|--------|
| No unsigned types in public API | Not all languages support `uint` |
| No arrays of specific rank other than single 0-based | Interop consistency |
| No pointer types in public API | Unsafe not universal |
| Overloads must differ by more than return type only | Some languages can't use |
| Identifiers not differing by case only | VB is case-insensitive |

Mark assemblies/types with `[assembly: CLSCompliant(true)]` and avoid violating types in public surface.

### CTS vs CLS

| | CTS | CLS |
|---|-----|-----|
| Scope | All types CLR understands | Public cross-language contract |
| Strictness | Broader | Narrower subset |

Internal implementation can use non-CLS types; public APIs should comply.

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| CLS purpose and audience | FULL | |
| Key CLS rules with examples | FULL | |
| CLSCompliant attribute | FULL | |
| When to break CLS (internal code) | FULL | |
| Language interop demo | PREVIEW | Optional F# library reference |

## Syllabus mapping

- Common Language Specification in .NET Framework  
