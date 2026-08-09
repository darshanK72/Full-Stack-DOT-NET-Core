---
module: 03. Generics & Collections
difficulty: Medium
chapters: 05 HashSet
domain: Publishing
---

# Article Tag Manager

Build a **.NET 8 console application from scratch** for blog tag deduplication and set algebra.

## Business context

Editorial merges tags from draft articles before publishing. Tags must be unique per article (case-insensitive), and editors need union/intersect/difference reports across two articles.

## Definitions

**Class `Article`**

- `Title` (string)
- `HashSet<string> Tags` — constructed with `StringComparer.OrdinalIgnoreCase`
- Constructor `(title, IEnumerable<string> seedTags)` — adds all tags, duplicates collapsed
- `bool AddTag(string tag)` — wrapper on set Add
- `override string ToString()` — title + joined tags

**Class `Subscriber`**

- `Name`, `Email` (strings)
- No Equals override

**Class `SubscriberByEmailComparer`** — `IEqualityComparer<Subscriber>` — equality by email ordinal ignore case; GetHashCode from email

**Static class `TagAnalytics`**

- `HashSet<string> UnionTags(Article a, Article b)` — non-mutating union, same comparer
- `HashSet<string> SharedTags(Article a, Article b)` — intersect
- `HashSet<string> TagsOnlyIn(Article a, Article b)` — except (tags in a not b)
- `void MergeDraftTags(HashSet<string> editorPool, IEnumerable<string> draftTags)` — `UnionWith` in place
- `bool IsTagSubsetOf(string tag, Article article)` — single-tag subset check via `IsSubsetOf` on singleton set OR `Contains`

**Demo for subscribers**

- `HashSet<Subscriber>` with comparer: add two subscribers same email → count 1
- Default `HashSet<Subscriber>` without comparer: same scenario → count 2

## Demo Main

Two articles with overlapping tags. Print union, shared, only-in-first. MergeDraftTags on editor pool. Subscriber comparer demo. Print tag counts after duplicate AddTag("LINQ") on article that already has "linq".

## Constraints

- net8
- Use HashSet set operations (instance or LINQ → HashSet), not manual nested loops for union/intersect

## Non-goals

Database, full CMS

## Evaluation

[EVALUATION.md](EVALUATION.md)
