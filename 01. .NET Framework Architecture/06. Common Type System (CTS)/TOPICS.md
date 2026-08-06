# 08. Common Type System (CTS)

## Status
`Not Started`

## Purpose
Define how .NET unifies types across languages so they interoperate safely.

---

## In-depth content

### CTS role

The **Common Type System** specifies:
- How types are declared, used, and managed in CLR  
- Rules for inheritance, visibility, overloading  
- Value vs reference type behavior  

Any .NET language must map its types to CTS types.

### Type categories

| Category | Storage | Examples |
|----------|---------|----------|
| **Value types** | Stack or inline in object | `int`, `struct`, `enum` |
| **Reference types** | Object on heap; variable holds reference | `class`, `string`, `array`, `delegate` |

All types derive from **`System.Object`**.

### Type hierarchy (simplified)

```
System.Object
├── Value types (System.ValueType)
│   └── struct, enum, primitives
└── Reference types
    ├── class
    ├── interface
    ├── array
    ├── delegate
    └── string (special immutable class)
```

### Unified type rules

- **Assignment compatibility** — widening/narrowing, boxing  
- **Method signatures** — parameter and return types must be CLS-compliant for cross-language APIs  
- **Arrays and generics** — single-dimensional zero-based arrays are CLS-friendly  

Practical C# types: **02. Data Types and Variables**.

---

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| CTS purpose | FULL | |
| Value vs reference types | FULL | Tie to chapter 06 stack/heap |
| System.Object hierarchy | FULL | |
| Boxing/unboxing rules | PREVIEW | → 08. Memory Management |
| Generics in CTS | PREVIEW | → 03. Generics |
| C# syntax for types | DEFER | → 01. Fundamentals |

## Syllabus mapping

- Common Type System in .NET Framework  
