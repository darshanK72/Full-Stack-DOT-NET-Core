---
module: 03. Generics & Collections
difficulty: Hard
chapters: 06 Queue and Stack
domain: Warehouse
---

# Warehouse Pick Queue

Build a **.NET 8 console application from scratch** combining FIFO pick tickets with LIFO undo and optional BFS grid demo.

## Business context

Pickers receive tickets in arrival order. While filling a ticket they may undo the last quantity adjustment. Operations uses a small grid map to verify shortest-path routing logic for a robot pilot.

## Definitions

**Record `PickTicket`** — `Id` (int), `Sku` (string), `Units` (int)

**Class `PickFloorSession`**

- `Queue<PickTicket> Pending` — FIFO pending picks
- `Stack<int> UnitsUndoStack` — stores previous unit values for active ticket (LIFO undo)
- `int? ActiveTicketId` — null when none active
- `void EnqueueTicket(PickTicket ticket)` — Enqueue
- `bool TryStartNextPick(out PickTicket ticket)` — TryDequeue into active; clear undo stack; set ActiveTicketId
- `void AdjustUnits(int newUnits)` — requires active ticket; Push current units to undo stack; update active ticket units (store active ticket in field or lookup)
- `bool TryUndoUnits(out int restoredUnits)` — TryPop undo stack, restore active ticket units
- `int PendingCount` — queue count

**Class `GridNavigator`** (static or instance)

- Method `bool TryFindGoal(char[,] grid, int startRow, int startCol, out (int Row, int Col) goalCell)`
- Grid: `.` open, `#` wall, `S` start, `G` goal
- BFS using `Queue<(int Row, int Col)>`, 4-directional, returns true when `G` reached with coordinates

## Demo Main

1. Enqueue 3 tickets; start next; adjust units twice; undo once; print state
2. Drain remaining tickets with TryStartNextPick until false
3. Run BFS on 4×4 sample maze (from reading chapter pattern); print goal coordinates or not found
4. Empty queue: TryDequeue behavior — use TryStartNextPick, no exception

## Constraints

- net8
- Use TryDequeue/TryPop where empty is possible
- Pick session must not use List for fair ordering of tickets

## Non-goals

Full WMS UI, persistence

## Evaluation

[EVALUATION.md](EVALUATION.md)
