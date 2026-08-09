---
module: 07. File Input & Outpout and Streams
difficulty: Medium
chapters: 04 Path & Environment Classes
domain: Desktop Deployment
---

# Deployment Path Planner

Build a **.NET 8 console application from scratch** that resolves cross-platform application paths without hard-coded drive letters.

## Business context

Installer tooling must locate roaming settings, local cache, and per-machine log folders relative to known OS special folders and the running assembly directory.

## Definitions

**Static class `DeploymentPathPlanner`**

- `string GetRoamingSettingsDirectory(string company, string appName)` — `Path.Combine(Environment.GetFolderPath(ApplicationData), company, appName)`
- `string GetLocalCacheDirectory(string company, string appName)` — combine `LocalApplicationData` similarly
- `string ResolveBundledAsset(string relativeAssetPath)` — `Path.Combine(AppContext.BaseDirectory, relativeAssetPath)` then `Path.GetFullPath`
- `string GetMachineLogDirectory(string company, string appName)` — under local cache, append segments `logs`, `Environment.MachineName`
- `string CreateTempExportPath(string extension)` — `Path.Combine(Path.GetTempPath(), $"export-{Guid.NewGuid():N}{extension}")` (does not create file)
- `string ChangeLogExtension(string logPath, string newExtension)` — `Path.ChangeExtension(logPath, newExtension)` ensuring newExtension starts with `.`

No file creation in this class — path strings only. No `Console`.

## Demo Main

1. Print roaming, cache, machine log paths for company `Contoso`, app `Warehouse`
2. Print resolved bundled asset for `config\defaults.json`
3. Print sample temp export path ending in `.csv`
4. Print `ChangeLogExtension` for `C:\logs\app.log` → `.bak` (use sample rooted path string; do not assume drive exists on all OS)

## Constraints

- net8
- Never concatenate with `"\\"` manually — use `Path.Combine`

## Non-goals

Creating directories, reading environment variables beyond documented APIs

## Evaluation

[EVALUATION.md](EVALUATION.md)
