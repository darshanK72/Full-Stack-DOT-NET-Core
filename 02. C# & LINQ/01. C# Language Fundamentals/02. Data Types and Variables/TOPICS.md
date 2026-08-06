# 02. Data Types and Variables

## Status
`Complete`

## Purpose
Built-in **value types** (numeric, bool, char), **reference types** used at this level (`string`, `object` preview), variables, literals, and related keywords in one cohesive program. This folder is intentionally **not** named "Primitive" — `string` and other reference types belong here alongside integers and decimal.

**Not in this chapter:** `struct` (custom value types) — deferred to a later OOP/types chapter.

## Subtopics to cover

| Subtopic | Depth | Notes |
|----------|-------|-------|
| Variables, declaration, assignment | FULL | |
| Naming conventions | FULL | |
| Scope (block, local) | FULL | |
| Literals (int, float, char, string, bool, null) | FULL | Syllabus: Literals in C# |
| Integer types (sbyte → ulong) | FULL | Value types |
| Floating point (float, double, decimal) | FULL | Value types |
| bool, char | FULL | Value types |
| `string` as a type | FULL | Reference type; literals and variables — not full API |
| Value types vs reference types (intro) | PREVIEW | string on heap; deep dive → Memory Management |
| `var` type inference | FULL | |
| `const` | FULL | |
| `readonly` | FULL | Use field + ctor or Models/ |
| Assignment operators | FULL | |
| Default values | FULL | |
| Nullable value types (`int?`) | FULL | |
| Type conversion & casting (basics) | FULL | Deep dive → chapter 05 |
| `checked` / `unchecked` | FULL | Overflow behavior |
| `object` type | PREVIEW | → boxing in Memory Management |
| Boxing/unboxing | PREVIEW | → `08. Memory Management` |
| `string` manipulation (methods) | PREVIEW | → `06. Strings` |
| `enum` | PREVIEW | → Methods/OOP context |
| Stack vs heap | PREVIEW | → Architecture ch.06, Memory Management |
| `struct` (custom value types) | DEFER | → later OOP / types chapter — not covered here |
| `dynamic` | DEFER | → `08. Var Dynamic and Special Keywords` |
| Methods, operators | DEFER | → chapters 04, 07 |

## Program.cs requirements

- Single runnable example (e.g. order summary or account demo) using every declared variable  
- Quick reference table of types and ranges  
- Do **not** teach `struct` in this chapter

## Syllabus mapping

- Data Types, Literals, Variables, Type Casting (intro)  
- Checked and Unchecked Keywords  
- Stack and Heap (preview), Boxing (preview)
