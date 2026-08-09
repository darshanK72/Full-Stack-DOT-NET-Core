---
module: 07. File Input & Outpout and Streams
difficulty: Medium
chapters: 01 File & Directory Operations, 02 StreamReader & StreamWriter
domain: DevOps / Logging
---

# Log Retention Janitor

Build a **.NET 8 console application from scratch** that prunes old log files and tails recent lines using enumeration and streaming reads.

## Business context

Microservices write rolling `.log` files under a folder tree. A janitor job deletes files not modified in the last N days and prints the last few lines of the newest remaining log for quick health checks.

## Definitions

**Class `LogRetentionJanitor`**

- Constructor `(string logsRoot)` — store root
- `int DeleteOlderThan(TimeSpan maxAge)` — `Directory.EnumerateFiles(logsRoot, "*.log", SearchOption.AllDirectories)`; for each, if `FileInfo.LastWriteTimeUtc` older than `UtcNow - maxAge`, `File.Delete`; return count deleted
- `string? FindNewestLog()` — enumerate same pattern; return full path of file with greatest `LastWriteTimeUtc`, or null if none
- `IReadOnlyList<string> TailNewestLog(int lineCount)` — if no logs return empty list; open newest with `StreamReader`, delegate tail logic to `TailReader`
- `static IReadOnlyList<string> TailReader(TextReader reader, int lineCount)` — same last-N-lines algorithm as RotatingLogWriter (single forward pass)

No `Console` in janitor except none allowed — static tail helper also no Console.

## Demo Main

1. Create nested log folder under base directory with three `.log` files
2. Set one file's last write time older than 10 days (write then adjust via `FileInfo.LastWriteTimeUtc`)
3. Print deleted count for `DeleteOlderThan(TimeSpan.FromDays(7))`
4. Append several lines to newest log via `StreamWriter` append
5. Print tailed last 2 lines from `TailNewestLog(2)`
6. Delete log tree recursively

## Constraints

- net8
- Use lazy enumeration, not `GetFiles` into array when deleting

## Non-goals

Compressed archives, cloud storage lifecycle rules

## Evaluation

[EVALUATION.md](EVALUATION.md)
