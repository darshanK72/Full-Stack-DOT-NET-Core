# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/03. FileStream & Binary Files`

---

#### Q1. (R) A telemetry service reads a fixed 4-byte file signature from `signature.bin`. In production, short files produce garbage signatures without throwing. Review the reader:

```csharp
public static string ReadFileSignature(string path)
{
    byte[] buffer = new byte[4];

    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    stream.Read(buffer, 0, buffer.Length); // signature must be exactly 4 bytes

    return Encoding.ASCII.GetString(buffer);
}
```

What is wrong, and how would you harden this for truncated or partially written files?

**Answer:** `FileStream.Read` may return fewer bytes than requested — especially near EOF or on a file still being written — but the code ignores the return value and decodes the entire buffer, padding with `\0` or stale bytes. You must loop until you have 4 bytes or confirm EOF, and treat short reads as corrupt input.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Return value of `Read` ignored | Partial buffer decoded as full signature |
| Runtime | No EOF / short-file handling | Silent garbage strings instead of explicit failure |
| Concurrency | `FileShare.Read` while writer may still flush | Reader sees pre-flush or truncated file |

**Fix (priority order):**

1. Loop reads until `totalRead == 4` or `Read` returns 0 — throw `InvalidDataException` if fewer than 4 bytes after EOF.
2. Prefer `BinaryReader.ReadBytes(4)` when you need an exact count — it throws `EndOfStreamException` on short input (see **Program.cs** Section 9).
3. If another process writes the file, coordinate with `FileShare.ReadWrite` on the writer and validate magic before trusting content.
4. Decode only the bytes actually read: `Encoding.ASCII.GetString(buffer, 0, totalRead)`.

```csharp
int totalRead = 0;
while (totalRead < buffer.Length)
{
    int n = stream.Read(buffer, totalRead, buffer.Length - totalRead);
    if (n == 0) break;
    totalRead += n;
}
if (totalRead != buffer.Length)
    throw new InvalidDataException($"Expected 4 signature bytes, got {totalRead}.");
```

**Production takeaway:** Karat embeds the **Program.cs** Section 5 rule — always check bytes returned — inside realistic signature-reading code. A single `Read` call is not a contract for a full buffer.

---

#### Q2. (R) A background job appends binary audit records while a dashboard process tries to read the same file. The writer opens like this; the reader gets `IOException: The process cannot access the file`:

```csharp
// Writer (audit service)
using var stream = new FileStream(
    auditPath, FileMode.Append, FileAccess.Write, FileShare.None);

// Reader (dashboard — runs concurrently)
using var readStream = new FileStream(
    auditPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
```

What locking mismatch causes the failure, and what `FileShare` flags should each side use?

**Answer:** The writer holds an exclusive lock with `FileShare.None`, so no other process can open the file — even for read — until the handle is disposed. For concurrent append + read, the writer must allow shared read access (`FileShare.Read`) while the reader opens with `FileShare.ReadWrite` so both can coexist.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Locking | Writer uses `FileShare.None` | Exclusive lock blocks dashboard reader |
| Design | Long-lived writer handle (service loop) | File stays locked for entire process lifetime |
| Correctness | Reader assumes `ReadWrite` share fixes writer side | Share flags must match on **both** open calls |

**Fix (priority order):**

1. Change writer to `FileShare.Read` (or `FileShare.ReadWrite` if multiple writers are coordinated): `new FileStream(auditPath, FileMode.Append, FileAccess.Write, FileShare.Read)`.
2. Keep reader as `FileMode.Open, FileAccess.Read, FileShare.ReadWrite`.
3. Ensure writer `Flush()` / dispose runs periodically if readers need fresh tail bytes — buffered appends may not be visible until flush.
4. For high-concurrency append, consider one writer process or a queue instead of many exclusive handles.

**Production takeaway:** `FileShare` is negotiated at open time — the most restrictive combination wins. **Program.cs** Section 4 shows `FileShare.None` for exclusive writes and `FileShare.Read` for concurrent readers; production append+tail-read patterns need the writer to grant share.

---

#### Q3. (R) A teammate ports `inventory.bin` readers from another language and swaps field order on one record type. The file opens fine but prices and names are nonsense after the first record. Review:

```csharp
for (int i = 0; i < recordCount; i++)
{
    int id = reader.ReadInt32();
    double price = reader.ReadDouble();   // was written as int32 + string + bool
    string name = reader.ReadString();
    bool inStock = reader.ReadBoolean();
    results[i] = new ProductRecord(id, name, price, inStock);
}
```

What breaks, why does corruption spread to later records, and how do you detect or recover safely?

**Answer:** Binary files have no field names — the reader consumes bytes in strict write order. Reading `double` where a length-prefixed `string` was written misaligns the stream pointer, so every subsequent field and record parses garbage until `EndOfStreamException` or absurd values appear.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Read order ≠ write order (`id`, `name`, `price`, `bool`) | First record wrong; cursor permanently offset |
| Serialization | Treating on-disk layout like struct memory layout | Language ports assume field order matches CLR struct |
| Recovery | No per-record checksum or length guard | One mismatch corrupts entire remainder of file |

**Fix (priority order):**

1. Restore exact write order from **Program.cs** `BinaryInventoryCodec`: `ReadInt32` → `ReadString` → `ReadDouble` → `ReadBoolean`.
2. Validate magic header and `recordCount` before the loop; cap `recordCount` against `stream.Length` to reject absurd headers (corrupt/truncated files).
3. Add optional per-record length prefix or CRC if you need partial recovery — without it, fail fast on first parse anomaly.
4. Never use `StructLayout` / `Marshal.StructureToPtr` interchangeably with `BinaryWriter` unless you explicitly define packing and endianness.

**Production takeaway:** BinaryReader mismatches are not localized bugs — one wrong primitive shifts the cursor for all following data. Magic bytes (**Program.cs** Section 3) catch wrong file types; they do not catch wrong field order within the right file.

---

#### Q4. (R) A log-rotation utility reads the last 8 bytes of a growing file to verify a footer magic. It intermittently returns wrong bytes under load. Review:

```csharp
public static byte[] ReadFooter(string path)
{
    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    byte[] footer = new byte[8];
    stream.Seek(0, SeekOrigin.End);           // jump to end
    stream.Read(footer, 0, footer.Length);
    return footer;
}
```

What Position/Seek mistakes are here, and what else should you validate before trusting the footer?

**Answer:** `Seek(0, SeekOrigin.End)` moves to EOF — **after** the last byte — not to the start of an 8-byte footer. The subsequent `Read` then pulls bytes from beyond the file (zeros/partial read) or fails silently depending on length. You need a negative offset from the end, e.g. `Seek(-8, SeekOrigin.End)`, and must handle files shorter than 8 bytes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Seek(0, End)` ≠ "last N bytes" | Reads past EOF; footer never matches |
| Edge case | No `Length < 8` guard | Short files produce partial/garbage footers |
| Concurrency | `FileShare.Read` while appender grows file | Footer position shifts between Seek and Read |

**Fix (priority order):**

1. Replace with `stream.Seek(-footer.Length, SeekOrigin.End)` — pattern from **Program.cs** `ReadLastTwoBytes` (`Seek(-2, SeekOrigin.End)`).
2. If `stream.Length < footer.Length`, throw or return a explicit failure — do not read.
3. Check `Read` return value or use `ReadBytes(8)` when you require exactly 8 bytes.
4. If the file is actively appended, re-read or use a stable snapshot (copy, or open with coordinated share + retry).

**Production takeaway:** `SeekOrigin.End` offsets are relative to EOF — zero means "after the last byte," not "the last byte." Karat tests whether you can translate "read tail" into signed Seek math.

---

#### Q5. (P) Your .NET service writes `metrics.bin` consumed by a Linux C tool on big-endian ARM. A developer uses default `BinaryWriter`/`BinaryReader` for `int` and `double` fields. Locally on x64 Windows everything works; in staging the C tool reads garbage. What is the root cause, and how do you design a cross-platform binary layout?

**Answer:** `BinaryWriter`/`BinaryReader` use the platform's native little-endian layout for multi-byte primitives on typical x64 Windows — the ARM C consumer expects big-endian (network) byte order, so numeric fields decode incorrectly even when field order and sizes match.

- Document an explicit wire format: field order, fixed sizes, and **endianness** (usually big-endian for cross-language files).
- Write primitives with explicit byte reversal (`BinaryPrimitives.WriteInt32BigEndian`) or a known serializer (Protocol Buffers, MessagePack) instead of assuming CLR defaults match C `struct` memory.
- Do not confuse **struct memory layout** (`StructLayout`, padding, alignment) with **BinaryWriter** layout — they are unrelated unless you carefully marshal.
- Add a version byte and magic header; integration-test round-trip with the C reader in CI on both endian platforms.

**Production takeaway:** "Same language on dev machine" hides endianness and padding issues until the first cross-platform consumer. **Program.cs** notes little-endian on typical x64 — that is an assumption, not a portable protocol.

---

#### Q6. (D) A data pipeline must scan a 60 GB append-only binary archive for records matching a key — random access by fixed record index, not full sequential parse every time. A junior proposes `FileStream` + `Seek` per lookup; a senior suggests `MemoryMappedFile`. What are the trade-offs, and when would you still choose streaming?

**Answer:** `MemoryMappedFile` maps file pages into virtual memory — excellent for repeated random access on large read-mostly files without loading 60 GB into a `byte[]`, and the OS caches hot regions efficiently. Pure `FileStream` + `Seek` per lookup works but pays more syscall overhead and does not leverage page cache as naturally for scattered access patterns.

- **Memory-mapped pros:** Fast indexed jumps when records are fixed-size or you maintain an offset index; multiple processes can share mapped views read-only; no manual buffer management for random reads.
- **Memory-mapped cons:** Less ideal for concurrent **writes** / append while mapped; address-space limits on 32-bit; careful handling of torn reads if writer appends without coordination; not a drop-in for variable-length records without an index.
- **Streaming pros:** Simpler lifecycle with `using`; natural for sequential export/import; better when records are variable-length and you must parse forward anyway; async `ReadAsync` pipelines for ETL.
- **Hybrid:** Build a sidecar index file (offset table) + mmap or seek; for one-pass full scan, sequential `FileStream` may be faster than millions of random seeks.

**Production takeaway:** Karat tests design judgment — mmap is not "always faster," but for **large, repeatedly indexed, read-heavy** binary archives it often beats naive per-lookup `Seek` on spinning disks and NVMe alike when record boundaries are known.

---

#### Q7. (R) An export worker writes large binary batches with async I/O, then signals a downstream processor via a message queue. The processor often reads zero-length or incomplete files. Review:

```csharp
public async Task ExportBatchAsync(string path, byte[] payload, CancellationToken ct)
{
    await using FileStream stream = new FileStream(
        path, FileMode.Create, FileAccess.Write, FileShare.Read);

    await stream.WriteAsync(payload, ct);
    // message published immediately after WriteAsync returns
    await _queue.PublishAsync(new BatchReadyMessage(path), ct);
}
```

What async/flush timing issue causes incomplete reads, and how do you fix it before publishing?

**Answer:** `WriteAsync` returning means data reached the `FileStream` buffer — not necessarily the OS disk cache or a stable on-disk length visible to another process. Publishing immediately races the consumer, which may open the file before flush/dispose completes and see zero or partial content.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No `FlushAsync` / dispose before signal | Consumer reads truncated file |
| Timing | Message queue is faster than disk visibility | Intermittent "empty file" failures |
| API | `FileShare.Read` allows concurrent open | Reader succeeds but gets stale length |

**Fix (priority order):**

1. `await stream.FlushAsync(ct)` before publishing — mirrors **Program.cs** Section 7 (`writer.Flush(); stream.Flush()`).
2. Prefer `await using` scope: dispose (close handle) before enqueue so OS metadata reflects final length.
3. Optionally write to a temp path and atomically `File.Move` to the final path, then publish — consumer never sees a half-written target.
4. Consumer should retry with backoff on short reads, but the producer must not rely on that alone.

```csharp
await stream.WriteAsync(payload, ct);
await stream.FlushAsync(ct);
// await using dispose runs here — then publish
await _queue.PublishAsync(new BatchReadyMessage(path), ct);
```

**Production takeaway:** Async I/O does not remove flush semantics — **Program.cs** warns that `FileInfo.Length` may be stale until flush/dispose. Queue-based pipelines need flush + atomic rename, not just `WriteAsync`.

---

#### Q8. (R) A cache service tries to wipe and rewrite `cache.bin` in one handle. It throws at runtime despite the path existing. Review both open attempts:

```csharp
// Attempt A — "open existing and overwrite first byte"
using var readOnly = new FileStream(cachePath, FileMode.Open, FileAccess.Read);
readOnly.WriteByte(0xFF);

// Attempt B — "create fresh file but only pass Read access"
using var creator = new FileStream(cachePath, FileMode.Create, FileAccess.Read);
```

What `FileMode`/`FileAccess` mismatches cause each failure, and what is the correct combination for in-place rewrite (**Program.cs** Section 10 — Truncate pattern)?

**Answer:** `FileAccess` must authorize every operation you perform — `Read` forbids `WriteByte`, and `FileMode.Create` with `FileAccess.Read` is an invalid combination that throws `ArgumentException` at construction. For in-place rewrite, open with write-capable access and use `FileMode.Truncate` or `ReadWrite` + explicit length reset.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Attempt A: `FileAccess.Read` + `WriteByte` | `NotSupportedException` at first write |
| API contract | Attempt B: `FileMode.Create` + `FileAccess.Read` | `ArgumentException` — Create requires Write or ReadWrite |
| Design | Using `Open` + write when file should be cleared first | Old bytes remain if you only overwrite first byte without truncating |

**Fix (priority order):**

1. For wipe-and-rewrite in place: `new FileStream(cachePath, FileMode.Truncate, FileAccess.Write, FileShare.None)` — **Program.cs** Section 10 sets length to 0 then writes.
2. If you need read-then-write in one session: `FileMode.OpenOrCreate` or `Open` with `FileAccess.ReadWrite`, then `SetLength(0)` or `Truncate` semantics before writing.
3. Match `FileMode` to intent: `Create` truncates existing path; `Append` seeks to end; do not pair write modes with read-only access.
4. Always pair with `using` / dispose so locks release after rewrite.

```csharp
using var stream = new FileStream(
    cachePath, FileMode.Truncate, FileAccess.Write, FileShare.None);
stream.WriteByte(0xFF);
stream.Flush();
```

**Production takeaway:** `FileMode` chooses **how the OS opens the path**; `FileAccess` gates **what this handle may do** — Karat stacks both in one snippet to see if you diagnose constructor vs first-write failures separately.
