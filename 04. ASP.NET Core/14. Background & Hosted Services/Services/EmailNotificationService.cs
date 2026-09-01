/*
 * FILE ROLE: Demonstrates IHostedService implemented directly — the lowest-level
 *            hosting contract in ASP.NET Core. Shows StartAsync/StopAsync lifecycles,
 *            System.Threading.Timer for periodic work, and IDisposable cleanup.
 * SECTIONS IN THIS FILE:
 *   2. IHostedService — StartAsync, StopAsync, Timer-based periodic work, IDisposable
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundHostedServices.Services;

/*
 * SECTION 2: IHostedService — LOWEST-LEVEL BACKGROUND CONTRACT
 *
 * IHostedService is the fundamental interface the hosting layer understands:
 *
 *   Task StartAsync(CancellationToken cancellationToken)
 *       Called when the host is ready to start the service.
 *       Should return quickly — kick off work on a background thread, not inline.
 *       The token fires if startup exceeds the configured timeout (default: 30 s).
 *
 *   Task StopAsync(CancellationToken cancellationToken)
 *       Called when the host is shutting down (SIGTERM, Ctrl+C, or StopApplication()).
 *       The token fires if shutdown takes too long (default: 30 s).
 *       Return Task.CompletedTask if the service can stop without async work.
 *
 * When to implement IHostedService directly vs using BackgroundService:
 *   ┌────────────────────────────────────┬──────────────────────────────────────────┐
 *   │ Use IHostedService directly when   │ Use BackgroundService when               │
 *   ├────────────────────────────────────┼──────────────────────────────────────────┤
 *   │ Simple start/stop wiring           │ You need a long-running loop             │
 *   │ External scheduler (Timer, Quartz) │ ExecuteAsync + stoppingToken pattern     │
 *   │ Fire-and-forget initialization     │ Graceful cancellation management needed  │
 *   │ Full control over async startup    │ (see PeriodicCleanupService for details) │
 *   └────────────────────────────────────┴──────────────────────────────────────────┘
 *
 * Pattern here: System.Threading.Timer fires on the ThreadPool at a fixed interval.
 *
 * Timer pitfall — overlapping callbacks:
 *   If the callback takes longer than the period, a second callback fires while the
 *   first is still running. Use period = Timeout.Infinite and reschedule at the end
 *   of each callback to prevent overlap, or use PeriodicTimer (net6+ — see SECTION 3b).
 *
 * Shutdown pitfall — Timer keeps firing after StopAsync:
 *   Fix: _timer?.Change(Timeout.Infinite, 0) in StopAsync suspends the timer
 *   without disposing it; Dispose() releases the OS handle.
 *
 * IDisposable: implement it when the service owns an unmanaged or native resource.
 * The host calls Dispose() on IDisposable services after StopAsync completes.
 */
public sealed class EmailNotificationService : IHostedService, IDisposable
{
    private readonly ILogger<EmailNotificationService> _logger;
    private Timer? _timer; // nullable — created in StartAsync, disposed in Dispose

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailNotificationService starting — scheduling batch emails.");

        // Timer fires once immediately (dueTime = TimeSpan.Zero), then every 5 minutes.
        // The callback is synchronous; for async work, use fire-and-forget:
        //   _ = Task.Run(() => SendBatchEmailsAsync(ct), ct);
        _timer = new Timer(SendBatchEmails, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        return Task.CompletedTask; // startup is non-blocking; timer runs independently
    }

    private void SendBatchEmails(object? state)
    {
        // Called on the ThreadPool — avoid blocking the thread with long-running I/O.
        _logger.LogInformation("EmailNotificationService: sending batch at {Time}.",
            DateTimeOffset.UtcNow);

        // Production code would query a database for pending notifications and send them.
        // Keep this method idempotent — the host may restart and call it again.
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailNotificationService stopping — suspending timer.");

        // Change to Timeout.Infinite prevents future callbacks without disposing.
        // This avoids a race where Dispose() is called while a callback is mid-execution.
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose(); // release the underlying OS waitable timer handle
    }
}
