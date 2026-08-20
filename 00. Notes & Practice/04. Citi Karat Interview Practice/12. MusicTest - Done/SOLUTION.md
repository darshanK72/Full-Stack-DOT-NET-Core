# MusicTest — Solution & Concepts Guide

Interview problem: build and extend a music streaming analytics backend in C#.

| File | Purpose |
|------|---------|
| `MusicTest.cs` | Problem statement + stub implementations + inline test suite |
| `SOLUTION.md` | This guide — concepts + step-by-step solutions |

---

## Table of Contents

1. [Problem Overview](#problem-overview)
2. [Core Concepts](#core-concepts)
3. [Data Structures Used](#data-structures-used)
4. [Task 1 — Bug Fix](#task-1--bug-fix-getlistenerstats)
5. [Task 2.1 — Find Duplicate Groups](#task-21--findduplicategroups)
6. [Task 2.2 — Merged Count by Artist](#task-22--getmergedcountbyartist)
7. [Task 3 — Song Reports](#task-3--getsongreports)
8. [LINQ Cheat Sheet](#linq-cheat-sheet-for-this-problem)
9. [Interview Tips](#interview-tips)

---

## Problem Overview

`MusicLibrary` is the central service that:

- Stores **songs** in a catalog (by `songId`)
- Records **play events** (who listened, how long, which song)
- Computes **listener statistics**, **deduplication results**, and **per-song play reports**

Each task builds on the previous one — from a boundary-condition bug fix to case-insensitive grouping and full-catalog reporting.

```
┌─────────────────┐     ┌──────────────────┐
│   AddSong()     │────▶│ Dictionary<int,  │
│                 │     │ Song>            │
└─────────────────┘     └──────────────────┘
                               │
┌─────────────────┐            │
│ AddPlayEvent()  │────────────┼──▶ List<PlayEvent>
└─────────────────┘            │
                               ▼
                    ┌──────────────────────┐
                    │ Analytics methods    │
                    │ (Tasks 1–3)          │
                    └──────────────────────┘
```

---

## Core Concepts

### 1. Completed vs Skipped Plays

A play is **completed** when the listener reached (or exceeded) the song duration:

```csharp
// Completed: listenedSeconds >= durationSeconds
// Skipped:   listenedSeconds < durationSeconds
```

**Why it matters:** `IsCompleted()` is reused in Task 1 (`GetListenerStats`) and Task 3 (`GetSongReports`). A single off-by-one comparison (`>` vs `>=`) breaks both tasks.

| Play | Duration | Listened | Result |
|------|----------|----------|--------|
| Exact finish | 180 | 180 | **Completed** |
| Over-listen | 200 | 220 | **Completed** |
| Partial | 180 | 50 | Skipped |

---

### 2. Case-Insensitive Duplicate Detection (Task 2)

Two songs are duplicates when **all three** match (case-insensitive for text fields):

- `title` (ignore case)
- `artist` (ignore case)
- `durationSeconds` (exact integer match)

```csharp
// These are duplicates:
new Song(1, "Song A",  "Artist 1", 180)
new Song(2, "song a",  "ARTIST 1", 180)
new Song(3, "SONG A",  "artist 1", 180)

// NOT duplicates — different duration:
new Song(30, "Song A", "Artist 1", 999)
```

**Key insight:** Metadata fields (`description`, `album`, `genre`) are **ignored** for duplicate detection. Only title + artist + duration matter.

---

### 3. Kept Song vs Merged Songs

For each duplicate group:

- **Keep** the song with the **smallest `songId`**
- **Merge away** all others (`mergedIds` = other ids, sorted ascending, excluding the kept id)
- Return `DedupResult` list sorted by kept `songId` ascending

```
Group: songIds [3, 1, 2]  →  kept = 1, mergedIds = [2, 3]
```

---

### 4. Dictionary vs List

| Structure | Use case in this problem |
|-----------|--------------------------|
| `Dictionary<int, Song> songs` | O(1) lookup by `songId`; master catalog |
| `List<PlayEvent> playEvents` | Append-only play log; filter/group for analytics |
| `Dictionary<int, SongReport>` | Return type mapping every catalog song to stats |
| `Dictionary<string, int>` | Artist → merged-away count (Task 2.2) |

**Key insight:** Songs live in a dictionary (registry). Play events live in a list (event log). Analytics methods **query the list** and **key results by song or artist**.

---

### 5. LINQ Grouping with Composite Keys

Task 2 groups songs by a **tuple** of normalized fields:

```csharp
.GroupBy(s => (s.title.ToLowerInvariant(), s.artist.ToLowerInvariant(), s.durationSeconds))
```

Use `ToLowerInvariant()` (or `ToUpperInvariant()`) for culture-safe case folding in interviews.

---

### 6. Every Catalog Song Must Appear (Task 3)

`GetSongReports()` must return an entry for **every** song in `songs`, even if never played:

```csharp
// Song 103 never played → SongReport(0, 0, 0)
```

If you only iterate `playEvents`, unplayed songs would be missing.

---

## Data Structures Used

### Domain Classes

```
Song
├── songId           : int
├── title            : string
├── artist           : string
├── durationSeconds  : int
├── description      : string?
├── album            : string?
├── releaseYear      : int?
└── genre            : string?

PlayEvent
├── playId           : int
├── userId           : int
├── songId           : int
└── listenedSeconds  : int

ListenerStats (Task 1 output)
├── totalPlays       : int
├── uniqueSongs      : int
└── completionRate   : double

DedupResult (Task 2.1 output)
├── songId           : int      (kept song)
└── mergedIds        : List<int> (sorted ascending, excludes songId)

SongReport (Task 3 output)
├── totalPlays       : int
├── completedPlays   : int
└── uniqueListeners  : int
```

---

## Task 1 — Bug Fix: `GetListenerStats()`

### Requirement

For a given `userId`, return:

| Field | Rule |
|-------|------|
| `totalPlays` | Total play events by this user |
| `uniqueSongs` | Distinct `songId` values played (any completion status) |
| `completionRate` | `completedPlays / totalPlays` as a double in `[0.0, 1.0]`; `0.0` if no plays |

### The Bug

The failure is in `IsCompleted()` — a **boundary comparison** on listen duration:

```csharp
// BUG: exact-duration listens are NOT counted as completed
return evt.listenedSeconds > song.durationSeconds;

// FIX: use >= per the problem definition
return evt.listenedSeconds >= song.durationSeconds;
```

When a user finishes a song exactly (`180` of `180`), strict `>` returns `false`, undercounting completed plays and lowering `completionRate`.

### Test Data Breakdown (User 1)

| playId | songId | Listened | Duration | Completed? |
|--------|--------|----------|----------|------------|
| 1 | 101 | 180 | 180 | Yes (exact) |
| 2 | 102 | 220 | 200 | Yes (over) |
| 3 | 101 | 50 | 180 | No (skipped) |
| 4 | 103 | 100 | 240 | No (skipped) |

**Expected:** `totalPlays=4`, `uniqueSongs=3`, `completionRate=2/4=0.5`

With the `>` bug, play 1 would not count → `completionRate=1/4=0.25` → test fails.

### Fix

```csharp
private bool IsCompleted(PlayEvent evt)
{
    Song song = songs[evt.songId];
    return evt.listenedSeconds >= song.durationSeconds;
}
```

### Concept: Floating-Point Division

```csharp
completionRate = (double)completedEvents.Count / userEvents.Count;
```

Cast to `(double)` before dividing to avoid integer division (`2/4 = 0` instead of `0.5`).

---

## Task 2.1 — `FindDuplicateGroups()`

### Requirement

Return `List<DedupResult>` for all duplicate groups:

- Group by `(title, artist, durationSeconds)` — case-insensitive for title and artist
- Only groups with **2+ songs**
- Keep smallest `songId`; `mergedIds` = other ids sorted ascending
- Result list sorted by kept `songId` ascending

### Solution

```csharp
public List<DedupResult> FindDuplicateGroups()
{
    return songs.Values
        .GroupBy(s => (
            s.title.ToLowerInvariant(),
            s.artist.ToLowerInvariant(),
            s.durationSeconds))
        .Where(g => g.Count() > 1)
        .Select(g =>
        {
            List<int> ids = g.Select(s => s.songId).OrderBy(id => id).ToList();
            int keptId = ids[0];
            List<int> merged = ids.Skip(1).ToList();
            return new DedupResult(keptId, merged);
        })
        .OrderBy(r => r.songId)
        .ToList();
}
```

### Worked Example (Test Data)

| Group | Song IDs | Kept | mergedIds |
|-------|----------|------|-----------|
| "song a" / "artist 1" / 180 | 1, 2, 3 | 1 | [2, 3] |
| "late group" / "late artist" / 50 | 5, 7, 8 | 5 | [7, 8] |
| "song b" / "artist 2" / 200 | 10, 11 | 10 | [11] |
| "song c" / "artist 3" / 240 | 20, 21 | 20 | [21] |

Songs 30, 40, 50, 60, 61, 99 are **not** in any duplicate group (different title, artist, or duration).

### Concepts Applied

1. **Composite GroupBy key** — tuple of normalized fields
2. **Where(g => g.Count() > 1)** — only true duplicate groups
3. **OrderBy on songId** — deterministic kept/merged selection

---

## Task 2.2 — `GetMergedCountByArtist()`

### Requirement

Return `Dictionary<string, int>` mapping artist → total songs **merged away** across all duplicate groups.

**Rules:**

- For each duplicate group: `merged away = mergedIds.Count` (= group size − 1)
- Use the **kept song's original `artist` string** as the map key (preserve casing from catalog)
- Sum counts when the same artist key appears in multiple groups
- Only artists with at least one duplicate appear
- No duplicates anywhere → empty map

### Solution

```csharp
public Dictionary<string, int> GetMergedCountByArtist()
{
    Dictionary<string, int> counts = new Dictionary<string, int>();

    foreach (DedupResult group in FindDuplicateGroups())
    {
        Song kept = songs[group.songId];
        string artist = kept.artist;
        int mergedAway = group.mergedIds.Count;

        if (!counts.ContainsKey(artist))
            counts[artist] = 0;

        counts[artist] += mergedAway;
    }

    return counts;
}
```

### Worked Example (Test Data)

| Group | Kept songId | Kept artist | mergedIds.Count |
|-------|-------------|-------------|-----------------|
| A | 1 | "Artist 1" | 2 |
| A2 | 40 | "Artist 1" | 1 |
| B | 10 | "Artist 2" | 1 |

**Result:** `{ "Artist 1": 3, "Artist 2": 1 }`

"Solo Artist" (song 99) has no duplicates → omitted.

### LINQ Alternative

```csharp
return FindDuplicateGroups()
    .GroupBy(r => songs[r.songId].artist)
    .ToDictionary(g => g.Key, g => g.Sum(r => r.mergedIds.Count));
```

---

## Task 3 — `GetSongReports()`

### Requirement

Return `Dictionary<int, SongReport>` for **every** song in the catalog:

| Field | Rule |
|-------|------|
| `totalPlays` | Count of all play events for that song |
| `completedPlays` | Count where `IsCompleted(event)` is true |
| `uniqueListeners` | Distinct `userId` values who played that song (any status) |

Never-played songs → `SongReport(0, 0, 0)`.

### Solution

```csharp
public Dictionary<int, SongReport> GetSongReports()
{
    Dictionary<int, SongReport> reports = new Dictionary<int, SongReport>();

    foreach (KeyValuePair<int, Song> entry in songs)
    {
        int songId = entry.Key;
        List<PlayEvent> events = playEvents
            .Where(e => e.songId == songId)
            .ToList();

        int total = events.Count;
        int completed = events.Count(e => IsCompleted(e));
        int uniqueListeners = events
            .Select(e => e.userId)
            .Distinct()
            .Count();

        reports[songId] = new SongReport(total, completed, uniqueListeners);
    }

    return reports;
}
```

### Worked Example (Test Data)

**Song 101** — 3 plays:

| userId | listened | duration | Completed? |
|--------|----------|----------|------------|
| 1 | 180 | 180 | Yes |
| 1 | 50 | 180 | No |
| 2 | 200 | 180 | Yes |

→ `totalPlays=3`, `completedPlays=2`, `uniqueListeners=2` (users 1 and 2)

**Song 103** — never played → `(0, 0, 0)`

### Why Iterate `songs`, Not `playEvents`?

Play events only cover songs that were played. Iterating the catalog guarantees unplayed songs appear with zero stats.

---

## LINQ Cheat Sheet for This Problem

```
Filter:      .Where(e => e.userId == userId)
             .Where(e => e.songId == songId)
Group:       .GroupBy(s => (s.title.ToLowerInvariant(), s.artist.ToLowerInvariant(), s.durationSeconds))
             .GroupBy(r => songs[r.songId].artist)
Aggregate:   .Count()
             .Sum(r => r.mergedIds.Count)
             .Distinct().Count()
Sort:        .OrderBy(id => id)
             .OrderBy(r => r.songId)
Select:      .Select(e => e.songId)
             .Skip(1)                    // all but kept id
Convert:     .ToList()
             .ToDictionary(...)
```

### Equivalent Imperative Style (Task 2.1 without LINQ)

```csharp
var buckets = new Dictionary<(string, string, int), List<int>>();

foreach (Song s in songs.Values)
{
    var key = (s.title.ToLowerInvariant(), s.artist.ToLowerInvariant(), s.durationSeconds);
    if (!buckets.ContainsKey(key))
        buckets[key] = new List<int>();
    buckets[key].Add(s.songId);
}

var results = new List<DedupResult>();
foreach (var pair in buckets)
{
    if (pair.Value.Count < 2) continue;
    pair.Value.Sort();
    results.Add(new DedupResult(pair.Value[0], pair.Value.GetRange(1, pair.Value.Count - 1)));
}
results.Sort((a, b) => a.songId.CompareTo(b.songId));
return results;
```

---

## Interview Tips

### Reading the Problem

1. **Underline boundary rules** — `>=` vs `>` for completion
2. **Note case sensitivity** — duplicate detection is case-insensitive; map keys in Task 2.2 use the **kept song's original** artist string
3. **Check who must appear in output** — all catalog songs (Task 3) vs only duplicate groups (Task 2.1)

### Debugging Task 1

When `completionRate` is too low, ask: *"Are exact-duration plays counted as completed?"* Trace `IsCompleted()` with a play where `listenedSeconds == durationSeconds`.

### Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| `>` instead of `>=` in `IsCompleted` | Use `>=` per spec |
| Including kept songId in `mergedIds` | Only ids **other than** the kept one |
| Case-sensitive duplicate grouping | `ToLowerInvariant()` on title and artist |
| Different duration treated as duplicate | All three fields must match |
| Missing unplayed songs in Task 3 | Loop `songs`, not `playEvents` |
| Using merged song's artist for Task 2.2 | Use **kept** song's artist string |
| Integer division in `completionRate` | Cast to `(double)` before dividing |
| Sorting `mergedIds` descending | Sort **ascending** |

### Complexity (for follow-up questions)

For `s` songs and `p` play events:

| Method | Time | Space |
|--------|------|-------|
| `GetListenerStats` | O(p) | O(p) user events |
| `FindDuplicateGroups` | O(s log s) | O(s) |
| `GetMergedCountByArtist` | O(s log s) | O(d) duplicate groups |
| `GetSongReports` | O(s × p) | O(s) |

All acceptable for typical interview data sizes.

---

## Quick Reference — All Solutions

```csharp
// TASK 1 — fix completion boundary
return evt.listenedSeconds >= song.durationSeconds;

// TASK 2.1 — find duplicate groups
return songs.Values
    .GroupBy(s => (s.title.ToLowerInvariant(), s.artist.ToLowerInvariant(), s.durationSeconds))
    .Where(g => g.Count() > 1)
    .Select(g => { /* kept = min songId, merged = rest sorted */ })
    .OrderBy(r => r.songId)
    .ToList();

// TASK 2.2 — merged count by kept song's artist
foreach (DedupResult group in FindDuplicateGroups())
    counts[songs[group.songId].artist] += group.mergedIds.Count;

// TASK 3 — report for every catalog song
foreach (KeyValuePair<int, Song> entry in songs) { ... }
```

Run the project to verify:

```bash
dotnet run --project "12. MusicTest"
```

Expected when fully solved: **4 passed, 0 failed** (BUG 1-2, Task 2.1, Task 2.2, Task 3).
