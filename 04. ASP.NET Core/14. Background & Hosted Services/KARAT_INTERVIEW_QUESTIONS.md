# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/14. Background & Hosted Services`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (M) A team registers both a custom `IHostedService` that only implements `StartAsync`/`StopAsync` and a `BackgroundService` subclass. When would you choose each, and what does `BackgroundService` provide on top of raw `IHostedService`?

---

#### Q2. (R) Review this email-dispatch hosted service. The app starts cleanly in Development but throws `ObjectDisposedException` on the second queued email in Production. What is wrong?

```csharp
public sealed class EmailQueueService : BackgroundService
{
    private readonly Channel<EmailJob> _queue = Channel.CreateUnbounded<EmailJob>();
    private readonly AppDbContext _db; // scoped registration

    public EmailQueueService(AppDbContext db) => _db = db;

    public ValueTask EnqueueAsync(EmailJob job) =>
        _queue.Writer.WriteAsync(job);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            var user = await _db.Users.FindAsync(job.UserId);
            await SendAsync(user!.Email, job.Template, stoppingToken);
            job.MarkSent();
            await _db.SaveChangesAsync(stoppingToken);
        }
    }
}
```

---

#### Q3. (P) Kubernetes sends SIGTERM and gives the pod 30 seconds before SIGKILL. How should a `BackgroundService` honor graceful shutdown using `CancellationToken`, and what must you configure so in-flight HTTP calls and queue items can finish?

---

#### Q4. (P) You need to offload heavy report generation from API requests without losing work on restart. Describe the in-process queue pattern with `Channel<T>` and a hosted consumer — what belongs in the API vs the worker, and when do you outgrow in-process queues?

---

#### Q5. (D) Two designs poll for overdue invoices: (A) `PeriodicTimer` in `ExecuteAsync` with a DB query every minute, (B) `Channel<T>` fed by API events plus a nightly reconciliation sweep. Compare throughput, duplicate processing risk, and shutdown behavior.

---

#### Q6. (R) Review this `BackgroundService` that processes files in parallel. Under load, SQL connection pool exhaustion and duplicate charges appear. What are the defects?

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var files = Directory.GetFiles(_inbox, "*.json");
        var tasks = files.Select(async path =>
        {
            var json = await File.ReadAllTextAsync(path, stoppingToken);
            var order = JsonSerializer.Deserialize<OrderPayload>(json)!;
            await _billing.ChargeAsync(order); // uses DbContext inside
            File.Delete(path);
        });
        await Task.WhenAll(tasks); // unbounded parallelism
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
}
```

*(Assume `_billing` is a scoped service injected into the singleton hosted service via constructor.)*

---

#### Q7. (M) `IHostedService.StartAsync` runs during host startup. What happens if `StartAsync` blocks on long synchronous work or awaits a never-completing task, and how does that differ from work placed in `BackgroundService.ExecuteAsync`?

---

#### Q8. (R) This outbox publisher runs as a hosted service. Messages are marked published in the database but never reach the broker after deploys. Review the loop.

```csharp
public class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IMessageBus _bus; // singleton wrapper around broker client

    public OutboxPublisher(IServiceScopeFactory scopes, IMessageBus bus)
    {
        _scopes = scopes;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var batch = await db.Outbox.Where(x => !x.Published).Take(50).ToListAsync(stoppingToken);

            foreach (var msg in batch)
            {
                await _bus.PublishAsync(msg.Payload, stoppingToken);
                msg.Published = true;
            }
            await db.SaveChangesAsync(stoppingToken);
            // no delay — tight loop
        }
    }
}
```

*(Consider exceptions, shutdown, and duplicate delivery.)*
