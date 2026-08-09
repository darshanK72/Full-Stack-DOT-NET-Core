---
module: 07. File Input & Outpout and Streams
difficulty: Hard
chapters: 03 FileStream & Binary Files
domain: Retail Inventory
---

# Inventory Binary Store

Build a **.NET 8 console application from scratch** that writes and reads a structured binary product catalog.

## Business context

POS terminals sync a compact offline catalog. Text CSV is too slow to parse on device boot; a binary snapshot loads typed records in one pass with a magic signature guard.

## Definitions

**Record struct `ProductRecord`**

- `(int Id, string Name, decimal UnitPrice, bool InStock)`

**Static class `InventoryBinaryStore`**

File layout:

| Field | Type |
|-------|------|
| magic | 4 ASCII bytes `CAT1` |
| recordCount | int32 |
| records | repeat recordCount times: int32 id, string name (BinaryWriter Write string), decimal price, bool inStock |

- `void Save(string path, IReadOnlyList<ProductRecord> products)` — create/truncate via `FileMode.Create`, UTF-8 `BinaryWriter`, flush streams
- `ProductRecord[] Load(string path)` — open read-only; if magic mismatch throw `InvalidDataException` with message `"Invalid catalog signature."`; else read records in write order

No `Console` in `InventoryBinaryStore`.

## Demo Main

1. Save three sample products to `catalog.bin` under base directory
2. Load and print each id, name, price, stock flag
3. Corrupt magic in a copy file (overwrite first byte) and catch `InvalidDataException` when loading
4. Delete demo files

## Constraints

- net8
- Read/write field order must match exactly
- Same UTF-8 encoding on BinaryReader/Writer

## Non-goals

Version migration, compression, concurrent writers

## Evaluation

[EVALUATION.md](EVALUATION.md)
