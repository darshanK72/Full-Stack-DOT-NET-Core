/*
 * FILE ROLE: Demonstrates BackgroundService (the preferred base class for
 *            long-running hosted services), PeriodicTimer (net6+) for periodic
 *            work without overlap, CancellationToken-based graceful shutdown,
 *            and IHostedLifecycleService (net8+) for extended lifecycle hooks.
 * SECTIONS IN THIS FILE:
 *   3a. BackgroundService + ExecuteAsync — the standard long-running loop
 *   3b. PeriodicTimer (net6+) — preferred periodic work primitive
 *   3c. CancellationToken propagation and graceful shutdown
 *   3d. IHostedLifecycleService (net8+) — extra lifecycle hooks
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundHostedServices.Services;

/*
 * SECTION 3a: BackgroundService — PREFERRED BASE CLASS FOR LONG-RUNNING WORK
 *
 * BackgroundService implements IHostedService and manages the task lifecycle:
 *
 *   protected abstract Task ExecuteAsync(CancellationToken stoppingToken)
 *       The host invokes this inside StartAsync. Run until stoppingToken fires.
 *       The token is linked to the host's shutdown signal (SIGTERM / Ctrl+C).
 *
 * BackgroundService vs IHostedService direct implementation:
 *   ┌──────────────────────────────┬──────────────────┬──────────────────────────┐
 *   │ Feature                      │ IHostedService   │ BackgroundService        │
 *   ├──────────────────────────────┼──────────────────┼──────────────────────────┤
 *   │ Long-running loop            │ Manual           │ ExecuteAsync() built-in  │
 *   │ Graceful stop token          │ Manual           │ stoppingToken provided   │
 *   │ Background task tracking     │ Manual           │ _executeTask tracked     │
 *   │ Exception behavior (net6+)   │ Manual           │ StopHost by default      │
 *   └──────────────────────────────┴──────────────────┴──────────────────────────┘
 *
 * BackgroundServiceExceptionBehavior (net6+):
 *   Default is StopHost — an unhandled exception in ExecuteAsync stops the host.
 *   Override with:
 *     builder.Services.Configure<HostOptions>(o =>
 *         o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
 *   Only set Ignore if you handle all exceptions inside your ExecuteAsync loop.
 *
 * SECTION 3b: PeriodicTimer (net6+) — NO OVERLAPPING CALLBACKS
 *
 *   using PeriodicTimer timer = new PeriodicTimer(period);
 *   while (await timer.WaitForNextTickAsync(stoppingToken)) { ... }
 *
 * Why PeriodicTimer beats System.Threading.Timer for periodic work:
 *   - WaitForNextTickAsync is truly awaitable — one await per tick
 *   - No overlapping: if work takes longer than the period, the next tick
 *     simply runs when the work completes (no timer drift accumulation)
 *   - Cancellation-aware: WaitForNextTickAsync returns false when ct is cancelled
 *     (no OperationCanceledException thrown — the loop exits cleanly via `while`)
 *   - Disposable: wrap in `using` to release the internal OS timer resource
 *
 * SECTION 3c: CancellationToken PROPAGATION AND GRACEFUL SHUTDOWN
 *
 * stoppingToken is cancelled when any of these occur:
 *   1. The host receives SIGTERM or the user presses Ctrl+C
 *   2. IHostApplicationLifetime.StopApplication() is called programmatically
 *   3. An unhandled exception in another hosted service stops the host (net6+)
 *
 * Best practice: pass stoppingToken to EVERY async call inside ExecuteAsync.
 * This ensures database queries, HTTP calls, and Task.Delay all cancel immediately
 * on shutdown rather than blocking the process for the timeout duration.
 *
 * Never swallow OperationCanceledException from stoppingToken — it means shutdown.
 * Use PeriodicTimer.WaitForNextTickAsync(ct) which returns false instead of throwing.
 *
 * SECTION 3d: IHostedLifecycleService (net8+) — EXTENDED LIFECYCLE HOOKS
 *
 * IHostedLifecycleService : IHostedService adds four hooks around StartAsync/StopAsync:
 *
 *   StartingAsync(ct)  — before StartAsync  | warm up caches, open connections
 *   StartedAsync(ct)   — after  StartAsync  | emit "service live" metrics, log readiness
 *   StoppingAsync(ct)  — before StopAsync   | drain queues, finish in-flight work
 *   StoppedAsync(ct)   — after  StopAsync   | flush telemetry, release licenses
 *
 * Ordering guarantee: ALL StartingAsync calls complete before any StartAsync runs.
 * Services stop in REVERSE registration order (last registered = first stopped).
 *
 * BackgroundService does NOT implement IHostedLifecycleService — you add it
 * yourself in the class declaration; BackgroundService satisfies the inherited
 * IHostedService portion (StartAsync / StopAsync are already provided).
 */
public sealed class PeriodicCleanupService : BackgroundService, IHostedLifecycleService
{
    private readonly ILogger<PeriodicCleanupService> _logger;
    private static readonly TimeSpan Period = TimeSpan.FromSeconds(10); // short for demo

    public PeriodicCleanupService(ILogger<PeriodicCleanupService> logger)
    {
        _logger = logger;
    }

    // ExecuteAsync is the heart of BackgroundService — runs for the service lifetime.
    // BackgroundService calls it inside StartAsync and awaits it in StopAsync.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "PeriodicCleanupService starting loop (period = {Period}).", Period);

        // PeriodicTimer is disposed automatically when ExecuteAsync returns (`using`).
        using PeriodicTimer timer = new PeriodicTimer(Period);

        // WaitForNextTickAsync returns false (not throw) when stoppingToken is cancelled —
        // the loop exits cleanly without needing a try/catch around the await.
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Catch per-tick errors here — a single failed pass should not
                // kill the service. If the error is unrecoverable, re-throw to let
                // the host's BackgroundServiceExceptionBehavior take effect.
                _logger.LogError(ex, "Cleanup pass failed — will retry on next tick.");
            }
        }

        _logger.LogInformation("PeriodicCleanupService loop exited (shutdown).");
    }

    private async Task PerformCleanupAsync(CancellationToken ct)
    {
        _logger.LogInformation("Running cleanup pass at {Time}.", DateTimeOffset.UtcNow);
        // Simulate async I/O — pass ct so this cancels immediately on shutdown.
        await Task.Delay(200, ct);
        _logger.LogInformation("Cleanup pass complete.");
    }

    // --- IHostedLifecycleService hooks ---

    // Fires before StartAsync (before ExecuteAsync is launched).
    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PeriodicCleanupService: StartingAsync — warming up.");
        return Task.CompletedTask;
    }

    // Fires after StartAsync (ExecuteAsync is running by this point).
    public Task StartedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PeriodicCleanupService: StartedAsync — service is live.");
        return Task.CompletedTask;
    }

    // Fires before StopAsync (stoppingToken is cancelled; ExecuteAsync is winding down).
    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PeriodicCleanupService: StoppingAsync — draining work.");
        return Task.CompletedTask;
    }

    // Fires after StopAsync (ExecuteAsync has returned; resources are released).
    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PeriodicCleanupService: StoppedAsync — fully stopped.");
        return Task.CompletedTask;
    }
}
