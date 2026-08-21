# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/01. Threads & Thread Lifecycle/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse console tool spawns label printers on dedicated threads. Operators report the process "hangs" after pressing Enter to quit, even though cancellation was requested. Review the shutdown wiring:

```csharp
public static void Main()
{
    using var cts = new CancellationTokenSource();
    var labelThread = new Thread(() => PrintLabelsLoop(cts.Token));
    labelThread.Name = "LabelPrinter";
    labelThread.Start();

    Console.WriteLine("Press Enter to stop…");
    Console.ReadLine();
    cts.Cancel();
    // expects immediate exit after Enter
}

static void PrintLabelsLoop(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        Thread.Sleep(500);
        Console.WriteLine("[LabelPrinter] stamp…");
    }
}
```

What keeps the process alive, and how do you fix shutdown so cancellation is honored cleanly?

---

#### Q2. (R) A teammate copied the shipment worker from the chapter tutorial but dropped synchronization "for speed." Under load, totals and result lists disagree. Review:

```csharp
private static int _packagesProcessed;
private static readonly List<ShipmentResult> _completed = new();

public static void ProcessShipment(object? state)
{
    var work = (ShipmentWork)state!;
    int boxesDone = 0;

    for (int box = 1; box <= work.BoxCount; box++)
    {
        Thread.Sleep(work.MillisecondsPerBox);
        _packagesProcessed++;          // no lock
        boxesDone++;
    }

    _completed.Add(new ShipmentResult(
        work.ShipmentId, work.Destination, boxesDone,
        0, Thread.CurrentThread.ManagedThreadId, ""));

    Console.WriteLine($"[{Thread.CurrentThread.Name}] done {work.ShipmentId}");
}

// Main starts three ParameterizedThreadStart workers concurrently on shared static fields.
```

What fails in production, and what is the prioritized fix?

---

#### Q3. (R) After parallelizing shipment processing, every worker log shows the same shipment id (`SH-1003`) even though three different ids were queued. Review the spawn loop:

```csharp
ShipmentWork[] pending =
[
    new("SH-1001", "North", 3, 50),
    new("SH-1002", "South", 2, 50),
    new("SH-1003", "East", 4, 50),
];

var threads = new Thread[pending.Length];
for (int i = 0; i < pending.Length; i++)
{
    threads[i] = new Thread(() => ProcessShipment(pending[i]));
    threads[i].Name = $"Worker-{pending[i].ShipmentId}";
    threads[i].Start();
}

foreach (var t in threads) t.Join();
```

Why does every thread process the last shipment, and how do you fix it without changing the worker signature?

---

#### Q4. (P) A long-running inventory sweep runs on a dedicated `Thread` (like `RunInventorySweep` in the chapter demo). Ops wants the Windows Service to stop within 30 seconds on shutdown — no `Thread.Abort`. What production pattern replaces force-kill, and what must the worker loop guarantee?

---

#### Q5. (M) Main waits for workers using `IsAlive` and `Join(100)` in a loop (matching the chapter demo). Under heavy load, logs show hundreds of `"Waiting on Worker-…"` lines per second while workers are still running. Is this a bug, and what waiting pattern is preferable in production?

```csharp
foreach (Thread worker in workers)
{
    while (worker.IsAlive)
    {
        Console.WriteLine($"  Waiting on {worker.Name} … IsAlive={worker.IsAlive}");
        if (!worker.Join(millisecondsTimeout: 100))
            continue;
    }
    Console.WriteLine($"  {worker.Name} finished.");
}
```

---

#### Q6. (D) Apex Warehouse will scan 400 inbound shipments per hour. A developer proposes `new Thread(ProcessShipment)` per shipment forever, `ThreadPriority.AboveNormal` on express lanes, and `[ThreadStatic]` counters for per-worker metrics exported to Prometheus. What breaks at scale, and what would you use instead while still honoring lifecycle concepts from this chapter?

---

#### Q7. (R) A retry path tries to restart workers after a transient fault. Review:

```csharp
Thread worker = new Thread(ProcessShipment);
worker.Name = "Worker-Retry";
worker.Start(shipment);

worker.Join();
if (shipment.NeedsRetry)
{
    worker.Start(shipment);   // "restart same thread"
    worker.Join();
}
```

What fails at runtime, and what is the correct lifecycle approach?
