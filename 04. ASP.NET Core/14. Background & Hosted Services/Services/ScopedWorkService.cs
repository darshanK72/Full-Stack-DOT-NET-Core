/*
 * FILE ROLE: Demonstrates the IServiceScopeFactory pattern for safely resolving
 *            scoped services from a singleton BackgroundService, and shows
 *            IHostApplicationLifetime for coordinating with host startup signals.
 * SECTIONS IN THIS FILE:
 *   4a. Captive dependency problem — why scoped services cannot be injected directly
 *   4b. IServiceScopeFactory pattern — creating a DI scope per unit of work
 *   4c. IHostApplicationLifetime — ApplicationStarted / ApplicationStopping tokens
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundHostedServices.Services;

/*
 * SECTION 4a: CAPTIVE DEPENDENCY PROBLEM
 *
 * BackgroundService (and all hosted services) are registered as singletons —
 * the host creates one instance at startup and keeps it for the application lifetime.
 *
 * Problem: injecting a scoped service directly into a singleton captures that scoped
 * instance forever. It is never released and outlives its intended scope.
 *
 *   // WRONG — scoped DbContext captured inside the singleton background service:
 *   public SomeService(AppDbContext db) { _db = db; }
 *   // db is never disposed; EF change tracker accumulates state; thread-unsafe
 *
 * The ASP.NET Core DI container will throw at runtime if you enable scope validation
 * (default in Development):
 *   InvalidOperationException: Cannot consume scoped service 'AppDbContext'
 *   from singleton 'SomeService'.
 *
 * The fix: inject IServiceScopeFactory (which IS singleton-safe) and create a
 * fresh scope for each logical unit of work. The scope — and all services resolved
 * from it — are disposed when the `using` block exits.
 *
 * SECTION 4b: IServiceScopeFactory PATTERN
 *
 *   using IServiceScope scope = _scopeFactory.CreateScope();
 *   var svc = scope.ServiceProvider.GetRequiredService<IScopedProcessor>();
 *   await svc.DoWorkAsync(ct);
 *   // scope.Dispose() here — all scoped instances released (including DbContext)
 *
 * Rules:
 *   - Create one scope per logical unit of work (one DB transaction, one message)
 *   - Always wrap in `using` — never cache services resolved from the scope
 *   - Pass the CancellationToken through to scoped services
 *   - IServiceScopeFactory itself is always registered as a singleton — safe to hold
 *
 * SECTION 4c: IHostApplicationLifetime — COORDINATING WITH HOST STARTUP
 *
 * IHostApplicationLifetime exposes three CancellationTokens:
 *
 *   ApplicationStarted   — fires once all IHostedService.StartAsync calls complete
 *   ApplicationStopping  — fires when shutdown begins (before StopAsync calls)
 *   ApplicationStopped   — fires after all StopAsync calls complete
 *
 * Common BackgroundService pattern: wait for ApplicationStarted before beginning
 * work. This ensures other services (database migration, cache warm-up, message
 * brokers) have finished their own StartAsync before you depend on them.
 *
 * Use StopApplication() to trigger a graceful host shutdown programmatically —
 * useful when a critical background service encounters an unrecoverable error
 * and the process should not continue running.
 *
 *   _appLifetime.StopApplication(); // sends shutdown signal; all services stop
 */

// IScopedProcessor — the scoped service resolved once per unit of work.
// In a real application this would live in its own file under Services/ or Processors/,
// with injected DbContext, repositories, or other scoped dependencies.
public interface IScopedProcessor
{
    Task DoWorkAsync(CancellationToken ct);
}

// ScopedProcessor is registered as Scoped — a new instance is created per DI scope.
// It is safe to inject DbContext, HttpClient factory, repositories, or other scoped
// services here, because the scope's lifetime is controlled by ScopedWorkService.
public sealed class ScopedProcessor : IScopedProcessor
{
    private readonly ILogger<ScopedProcessor> _logger;

    public ScopedProcessor(ILogger<ScopedProcessor> logger)
    {
        _logger = logger;
    }

    public async Task DoWorkAsync(CancellationToken ct)
    {
        _logger.LogInformation("ScopedProcessor executing at {Time}.", DateTimeOffset.UtcNow);
        await Task.Delay(50, ct); // simulate a database write or external service call
    }
}

/*
 * ScopedWorkService is the singleton BackgroundService that uses the factory pattern.
 * It holds only singleton-safe dependencies:
 *   IServiceScopeFactory  — creates scopes on demand (singleton by design)
 *   IHostApplicationLifetime — host event tokens (singleton)
 *   ILogger               — always singleton-safe
 */
public sealed class ScopedWorkService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<ScopedWorkService> _logger;

    public ScopedWorkService(
        IServiceScopeFactory scopeFactory,
        IHostApplicationLifetime appLifetime,
        ILogger<ScopedWorkService> logger)
    {
        _scopeFactory = scopeFactory;
        _appLifetime = appLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait until the host is fully started before beginning scoped work.
        // This ensures other hosted services (database migration, cache warm-up)
        // have completed their StartAsync before we depend on their state.
        await WaitForApplicationStartedAsync(stoppingToken);

        if (stoppingToken.IsCancellationRequested)
        {
            return; // host stopped before fully starting — bail out cleanly
        }

        _logger.LogInformation("ScopedWorkService: host fully started — beginning work loop.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoScopedWorkAsync(stoppingToken); // creates scope, resolves, disposes
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // stoppingToken was cancelled — exit cleanly
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScopedWorkService: unexpected error — will retry.");
            }
        }

        _logger.LogInformation("ScopedWorkService stopped.");
    }

    private async Task DoScopedWorkAsync(CancellationToken ct)
    {
        // Create a fresh DI scope for this unit of work.
        // `using` guarantees Dispose() runs, releasing all scoped instances (DbContext, etc.).
        using IServiceScope scope = _scopeFactory.CreateScope();

        // Resolve the scoped service from the scope's service provider — not the root provider.
        IScopedProcessor processor =
            scope.ServiceProvider.GetRequiredService<IScopedProcessor>();

        await processor.DoWorkAsync(ct); // executes within the scope's lifetime
    } // scope.Dispose() here — scoped services are released

    private Task WaitForApplicationStartedAsync(CancellationToken stoppingToken)
    {
        // ApplicationStarted is a CancellationToken that fires (is cancelled) once
        // the host completes all StartAsync calls — "cancelled" here means "triggered".
        TaskCompletionSource tcs =
            new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Complete the task when the host is fully started...
        _appLifetime.ApplicationStarted.Register(() => tcs.TrySetResult());
        // ...or if the host stops before it ever finishes starting.
        stoppingToken.Register(() => tcs.TrySetResult());

        return tcs.Task;
        // Note: CancellationTokenRegistration objects are freed when the tokens
        // are disposed (during host shutdown), so no explicit cleanup is needed here.
    }
}
