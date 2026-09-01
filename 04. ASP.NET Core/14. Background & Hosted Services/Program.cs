/*
 * TOPIC: Background & Hosted Services in ASP.NET Core
 *
 * WHY IT MATTERS:
 *   Modern applications need work that runs outside the HTTP request cycle:
 *   sending emails, cleaning up stale data, processing queued commands, syncing
 *   with external systems, and scheduled maintenance. ASP.NET Core's hosted service
 *   model provides a first-class, DI-integrated mechanism to run this work within
 *   the same process — with graceful startup, cancellable shutdown, and full access
 *   to the DI container and application configuration.
 *
 * WHAT YOU WILL LEARN:
 *    1. IHostedService (StartAsync/StopAsync) — the foundational interface
 *    2. BackgroundService — abstract base class for long-running loops
 *    3. PeriodicTimer (net6+) — preferred periodic work primitive (no overlap)
 *    4. CancellationToken propagation and graceful shutdown
 *    5. IHostedLifecycleService (net8+) — extended lifecycle hooks
 *    6. Scoped services inside BackgroundService via IServiceScopeFactory
 *    7. IHostApplicationLifetime — reacting to ApplicationStarted / Stopping
 *    8. Channel<T> producer/consumer queue for background task offloading
 *    9. Exception handling in hosted services — per-item isolation
 *   10. Ordering multiple hosted services (registration order matters)
 *   11. Worker Service project template overview
 *
 * CHAPTER MAP:
 *    1. WorkItem record (queue payload)            → Models/WorkItem.cs
 *    2. IHostedService (StartAsync/StopAsync)      → Services/EmailNotificationService.cs
 *    3. BackgroundService + PeriodicTimer          → Services/PeriodicCleanupService.cs
 *       IHostedLifecycleService (net8+)
 *    4. IServiceScopeFactory + IHostApplicationLifetime → Services/ScopedWorkService.cs
 *    5. Channel<T> queue (producer interface)      → Queue/BackgroundTaskQueue.cs
 *    6. Queue consumer BackgroundService           → Queue/QueueProcessorService.cs
 *    7. AddHostedService registrations + ordering  → Program.cs (below)
 *
 * WORKER SERVICE PROJECT TEMPLATE:
 *   `dotnet new worker` scaffolds a project using Microsoft.NET.Sdk.Worker:
 *     - Host.CreateDefaultBuilder() wires logging, config, and DI (no HTTP pipeline)
 *     - Ideal for pure background processing — deploys as Windows Service, Linux
 *       systemd unit, Docker container, or Azure Worker
 *     - AddHostedService<Worker>() registers the generated Worker : BackgroundService
 *   This chapter uses Sdk.Web so the /enqueue endpoint can demonstrate the producer
 *   side. In production, separate the API (Sdk.Web) from workers (Sdk.Worker) into
 *   distinct deployable units unless they must run in the same process.
 */

using System;
using System.Threading;
using BackgroundHostedServices.Models;
using BackgroundHostedServices.Queue;
using BackgroundHostedServices.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

/*
 * SECTION 7: AddHostedService REGISTRATIONS AND SERVICE ORDERING
 *
 * AddHostedService<T>() registers T as:
 *   - A singleton in the DI container (same instance for the application lifetime)
 *   - An IHostedService implementation (the host calls StartAsync and StopAsync)
 *
 * Startup order: services start in registration order (first registered = first started).
 * Shutdown order: services stop in REVERSE registration order (last started = first stopped).
 *
 * Ordering guideline:
 *   1. Register infrastructure first (queue, cache, database connections)
 *   2. Register consumers that depend on infrastructure after (they use ApplicationStarted
 *      to delay their work until all StartAsync calls complete — see ScopedWorkService)
 *   3. Register long-running compute services last
 *
 * IHostedLifecycleService changes the ordering model:
 *   All StartingAsync methods run → All StartAsync methods run → All StartedAsync methods run
 *   (Symmetrically for Stopping/Stop/Stopped.)
 *
 * AddHostedService<T> vs AddSingleton<IHostedService, T>:
 *   Both register the service, but AddHostedService<T> also registers T directly as a
 *   singleton so other services CAN resolve T itself (not just IHostedService).
 *
 * Sharing a singleton between hosted services and API endpoints:
 *   Register the shared singleton BEFORE AddHostedService so both the hosted service
 *   and the endpoint handler resolve the SAME instance from the DI container.
 */

// Singleton queue — shared between QueueProcessorService (consumer) and the API endpoint (producer).
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

// Scoped service resolved per unit of work by ScopedWorkService via IServiceScopeFactory.
builder.Services.AddScoped<IScopedProcessor, ScopedProcessor>();

// Hosted services registered in startup order:
builder.Services.AddHostedService<EmailNotificationService>();  // IHostedService (Timer-based)
builder.Services.AddHostedService<PeriodicCleanupService>();    // BackgroundService + IHostedLifecycleService
builder.Services.AddHostedService<ScopedWorkService>();         // IServiceScopeFactory + IHostApplicationLifetime
builder.Services.AddHostedService<QueueProcessorService>();     // Channel<T> consumer

WebApplication app = builder.Build();

/*
 * Demo endpoint — acts as the queue PRODUCER.
 * POST /enqueue?payload=hello-world
 *
 * Minimal API parameters resolved automatically:
 *   IBackgroundTaskQueue — from DI (the singleton registered above)
 *   string payload       — from the query string (?payload=...)
 *   CancellationToken    — from HttpContext.RequestAborted
 *
 * In a real application the producer might be a message broker consumer (RabbitMQ,
 * Azure Service Bus), a webhook receiver, or a scheduled trigger — not a plain HTTP
 * endpoint. The Channel<T> pattern is the same regardless of the source.
 */
app.MapPost("/enqueue", async (IBackgroundTaskQueue queue, string payload, CancellationToken ct) =>
{
    WorkItem item = new WorkItem(Guid.NewGuid(), payload, DateTimeOffset.UtcNow); // build work item
    await queue.EnqueueAsync(item, ct); // awaits if the bounded channel is full (backpressure)
    return Results.Accepted($"/queue/{item.Id}", item); // 202 — item accepted, processing async
});

app.Run();

/*
 * QUICK REFERENCE — Background & Hosted Services
 *
 * Interface / Type                 | Role
 * ──────────────────────────────────────────────────────────────────────────────
 * IHostedService                   | Fundamental contract: StartAsync + StopAsync
 * BackgroundService                | Abstract base: ExecuteAsync loop + stoppingToken
 * IHostedLifecycleService (net8+)  | Extra hooks: StartingAsync/StartedAsync/StoppingAsync/StoppedAsync
 * PeriodicTimer (net6+)            | Periodic work with no overlap; WaitForNextTickAsync
 * IServiceScopeFactory             | Safely resolve scoped services from a singleton
 * IHostApplicationLifetime         | Tokens: ApplicationStarted / Stopping / Stopped
 * Channel<T>                       | In-process producer/consumer queue; async + backpressure
 *
 * Key registrations
 * ──────────────────────────────────────────────────────────────────────────────
 * builder.Services.AddHostedService<T>()
 *     T as singleton + IHostedService; starts in registration order.
 *
 * builder.Services.Configure<HostOptions>(o => o.BackgroundServiceExceptionBehavior
 *     = BackgroundServiceExceptionBehavior.Ignore)
 *     Override .NET 6+ default (StopHost) — only when you handle all exceptions.
 *
 * Key API members
 * ──────────────────────────────────────────────────────────────────────────────
 * BackgroundService.ExecuteAsync(CancellationToken stoppingToken)
 *     Override this. Loop until stoppingToken is cancelled. Pass ct everywhere.
 *
 * PeriodicTimer.WaitForNextTickAsync(ct)
 *     Returns false (no throw) when ct fires — clean loop exit via `while`.
 *
 * IServiceScopeFactory.CreateScope()
 *     Returns IServiceScope. Resolve scoped services from scope.ServiceProvider.
 *     Always wrap in `using` — scoped instances released on Dispose.
 *
 * IHostApplicationLifetime.ApplicationStarted
 *     CancellationToken that fires once ALL hosted services complete StartAsync.
 *
 * Channel.CreateBounded<T>(new BoundedChannelOptions(n) { FullMode = ... })
 *     BoundedChannelFullMode.Wait — backpressure: WriteAsync awaits when full.
 *
 * Startup / shutdown timeout defaults (HostOptions):
 *     StartupTimeout  = 30 seconds
 *     ShutdownTimeout = 30 seconds (SIGTERM grace period)
 */
