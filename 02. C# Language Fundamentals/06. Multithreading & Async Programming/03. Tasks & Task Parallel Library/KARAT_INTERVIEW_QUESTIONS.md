# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/03. Tasks & Task Parallel Library/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) An ASP.NET Core batch-validation endpoint works in dev but stalls under load. Review the action:

```csharp
[HttpPost("orders/validate-batch")]
public IActionResult ValidateBatch([FromBody] int[] orderIds)
{
    Task<bool>[] validations = orderIds
        .Select(id => Task.Run(() => _orderValidator.Validate(id)))
        .ToArray();

    bool[] results = Task.WhenAll(validations).Result;
    return Ok(new { ValidCount = results.Count(r => r) });
}
```

What are the problems (threading, scalability, and API shape), and how do you fix them in priority order?

---

#### Q2. (R) A fulfillment service refactored raw threads to tasks, but ops reports missing line picks and intermittent duplicate shipments. Review:

```csharp
public Task FulfillMultiLineOrder(Order order)
{
    return Task.Factory.StartNew(() =>
    {
        foreach (string line in order.Lines)
        {
            Task.Run(() =>
            {
                Thread.Sleep(40);
                _pickLog.Record(order.OrderId, line);
            });
        }
    });
}

// Caller in a background worker:
_fulfillment.FulfillMultiLineOrder(order).Wait();
Console.WriteLine("Fulfillment complete — releasing dock slot");
```

What fails at runtime, and what is the corrected task composition?

---

#### Q3. (R) A payment integration wraps a legacy callback gateway with `TaskCompletionSource`. Declined payments sometimes hang until timeout; approved payments occasionally throw `InvalidOperationException`. Review:

```csharp
public Task<PaymentResult> ChargeAsync(int orderId, decimal amount)
{
    var tcs = new TaskCompletionSource<PaymentResult>();

    _gateway.PaymentCompleted += result =>
    {
        tcs.SetResult(result);
    };

    _gateway.PaymentFailed += ex =>
    {
        tcs.SetException(ex);
    };

    _gateway.Charge(orderId, amount);
    return tcs.Task;
}
```

What production defects are embedded here, and how do you harden the wrapper?

---

#### Q4. (R) A shipping pipeline chains pick → label with continuations after removing `async/await` "for clarity." Fault injection tests crash the worker process. Review:

```csharp
Task<string> pickTask = Task.Run(() =>
{
    if (_inventory.IsEmpty(slotId))
        throw new InvalidOperationException("Inventory slot empty");
    return $"Picked order #{orderId}";
});

Task<string> labelTask = pickTask.ContinueWith(
    antecedent => _labelService.Create(antecedent.Result));

string label = labelTask.Result;
_audit.Log($"Label created: {label}");
```

What breaks when the pick task faults, and how should the continuation chain be written?

---

#### Q5. (P) A warehouse API throttles concurrent picks with `SemaphoreSlim` (matching the chapter pattern). After a downstream timeout spike, throughput collapses to zero until restart. Review:

```csharp
private readonly SemaphoreSlim _pickerGate = new(4, 4);

public async Task<string> PickOrderAsync(Order order, CancellationToken ct)
{
    await _pickerGate.WaitAsync(ct);
    string pickResult = await _warehouseClient.PickAsync(order, ct);
    _pickerGate.Release();
    return pickResult;
}
```

What fails when `PickAsync` throws or the request is canceled mid-flight, and what is the production-safe throttle pattern?

---

#### Q6. (M) A carrier-selection service uses `Task.WhenAny` to take the fastest quote (as in the chapter demo). Load tests show open HTTP connection counts climbing. Review:

```csharp
public decimal GetBestShippingRate(Order order)
{
    Task<CarrierQuote>[] carrierTasks =
    [
        Task.Run(() => _fastFreight.Quote(order)),
        Task.Run(() => _economyPost.Quote(order)),
        Task.Run(() => _premiumAir.Quote(order)),
    ];

    Task<CarrierQuote> winner = Task.WhenAny(carrierTasks).Result;
    return winner.Result.Price;
}
```

Why do losing carrier calls keep consuming resources, and what changes after you pick a winner?

---

#### Q7. (D) A team must batch-validate four thousand orders every night. One developer proposes `Task.WaitAll` on thousands of `Task.Run(() => Validate(order))` calls; another wants `Parallel.ForEach` immediately; a third wants `async`/`await` with `Task.WhenAll` and a concurrency limit. What breaks at scale with the first approach, and what pattern would you ship?

---
