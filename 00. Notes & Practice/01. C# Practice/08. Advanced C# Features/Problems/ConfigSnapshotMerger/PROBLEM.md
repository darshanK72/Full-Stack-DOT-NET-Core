---
module: 08. Advanced C# Features
difficulty: Medium
chapters: 01 Serialization & Desiralization
domain: ApplicationConfig
---

# Config Snapshot Merger

Build a **.NET 8 console application from scratch** that reads and patches JSON configuration documents with `JsonNode` without deserializing to a fixed C# type.

## Business context

DevOps stores environment settings as JSON files. Hotfix patches arrive as small JSON fragments (feature flags, timeout tweaks). The merger applies patch keys into a base document and writes the combined snapshot — without maintaining a class for every config shape.

## Definitions

**Class `ConfigSnapshotMerger`**

- `string MergeJson(string baseJson, string patchJson)` — parse both with `JsonNode.Parse`; for each property in patch object, set or replace same key on base object; return `baseNode.ToJsonString()` with `WriteIndented = true` via `JsonSerializerOptions`
- `bool TryGetInt(JsonNode? root, string key, out int value)` — navigate `root?[key]`; if node is `JsonValue` and convertible to int, set value and return true; else value = 0, return false
- `IReadOnlyList<string> ListTopLevelKeys(string json)` — parse object; return property names sorted alphabetically

## Demo Main

1. Base JSON: `{ "appName": "Portal", "timeoutSeconds": 30, "features": { "beta": false } }`
2. Patch JSON: `{ "timeoutSeconds": 45, "features": { "beta": true, "analytics": true } }`
3. Print merged JSON (nested `features` object replaced entirely by patch value — shallow merge at top level only).
4. `TryGetInt` on merged root for `timeoutSeconds` and missing key.
5. `ListTopLevelKeys` on merged document.

## Constraints

- net8, explicit usings, `System.Text.Json.Nodes`
- Shallow merge: top-level keys only; nested objects replaced as whole values from patch
- Null `root` in `TryGetInt` returns false

## Non-goals

Deep recursive merge, JSON Schema validation, file watchers

## Evaluation

[EVALUATION.md](EVALUATION.md)
