# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/07. File Input & Outpout and Streams/03. FileStream & Binary Files`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

---

#### Q3. (R) A teammate ports `inventory.bin` readers from another language and swaps field order on one record type. The file opens fine but prices and names are nonsense after the first record. Review:

```csharp
for (int i = 0; i < recordCount; i++)
{
    int id = reader.ReadInt32();
    double price = reader.ReadDouble();   // was written as int32 + string + double + bool
    string name = reader.ReadString();
    bool inStock = reader.ReadBoolean();
    results[i] = new ProductRecord(id, name, price, inStock);
}
```

What breaks, why does corruption spread to later records, and how do you detect or recover safely?

---

#### Q4. (R) A log-rotation utility reads the last 8 bytes of a growing file to verify a footer magic. It intermittently returns wrong bytes under load. Review:

```csharp
public static byte[] ReadFooter(string path)
{
    using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    stream.Seek(0, SeekOrigin.End);           // jump to end
    byte[] footer = new byte[8];
    stream.Read(footer, 0, footer.Length);
    return footer;
}
```

What Position/Seek mistakes are here, and what else should you validate before trusting the footer?

---

#### Q5. (P) Your .NET service writes `metrics.bin` consumed by a Linux C tool on big-endian ARM. A developer uses default `BinaryWriter`/`BinaryReader` for `int` and `double` fields. Locally on x64 Windows everything works; in staging the C tool reads garbage. What is the root cause, and how do you design a cross-platform binary layout?

---

#### Q6. (D) A data pipeline must scan a 60 GB append-only binary archive for records matching a key — random access by fixed record index, not full sequential parse every time. A junior proposes `FileStream` + `Seek` per lookup; a senior suggests `MemoryMappedFile`. What are the trade-offs, and when would you still choose streaming?

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
