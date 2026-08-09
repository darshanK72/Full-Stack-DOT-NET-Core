---
module: 07. File Input & Outpout and Streams
difficulty: Medium
chapters: 02 StreamReader & StreamWriter
domain: Application Logging
---

# Rotating Log Writer

Build a **.NET 8 console application from scratch** for append-only UTF-8 logs and text-reader-based tail utilities.

## Business context

Platform ops writes timestamped INFO/WARN lines to a shared log file. Support tools count non-empty lines and print the last N lines without loading multi-gigabyte files into memory.

## Definitions

**Class `ApplicationLogWriter`**

- Constructor `(string logFilePath)` — store path
- `void WriteEntry(string level, string message)` — append one line: `{UtcNow:yyyy-MM-dd HH:mm:ss} {level} {message}` using `StreamWriter(path, append: true)` with UTF-8 no BOM (`new UTF8Encoding(false)`); flush after each entry

**Static class `LogAnalytics`**

- `int CountNonEmptyLines(TextReader reader)` — loop `ReadLine()` until null; count lines where `Length > 0`
- `IReadOnlyList<string> ReadLastLines(TextReader reader, int count)` — single forward pass; keep a fixed-size queue/list of last `count` non-null lines (may return fewer if file shorter)

No `Console` in `ApplicationLogWriter` or `LogAnalytics`.

## Demo Main

1. Write three entries (INFO, WARN, INFO) to temp log under base directory
2. Re-open with `StreamReader`, print `CountNonEmptyLines`
3. Re-open, print `ReadLastLines(..., 2)` joined with ` | `
4. Demonstrate `StringReader` with inline text passed to `CountNonEmptyLines` (polymorphism)
5. Delete temp log file

## Constraints

- net8
- Always wrap file streams in `using`
- Matching UTF-8 encoding on writer and reader

## Non-goals

Actual log rotation by size, async I/O

## Evaluation

[EVALUATION.md](EVALUATION.md)
