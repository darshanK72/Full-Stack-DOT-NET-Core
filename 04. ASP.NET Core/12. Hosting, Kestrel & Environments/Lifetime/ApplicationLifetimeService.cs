/*
 * FILE ROLE: Implements IHostedService to demonstrate IHostApplicationLifetime --
 *            the ApplicationStarted, ApplicationStopping, and ApplicationStopped events,
 *            plus programmatic graceful shutdown via StopApplication().
 * SECTIONS IN THIS FILE:
 *   14. IHostedService -- background service lifecycle (StartAsync / StopAsync)
 *   15. IHostApplicationLifetime -- ApplicationStarted / Stopping / Stopped events
 *   16. Graceful shutdown -- StopApplication() and shutdown timeout
 */

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HostingKestrel.Lifetime;

/*
 * SECTION 14: IHostedService -- BACKGROUND SERVICE LIFECYCLE
 *
 * IHostedService is the .NET hosting contract for background and startup work.
 * The generic host calls these two methods:
 *
 *   StartAsync(CancellationToken cancellationToken)
 *     Called during host startup, after the DI container is built.
 *     Should return quickly -- blocking here delays Kestrel from starting.
 *     Use Task.Run() to fire a background loop without blocking.
 *
 *   StopAsync(CancellationToken cancellationToken)
 *     Called during graceful shutdown.
 *     The cancellationToken fires when the shutdown timeout expires.
 *     Perform clean-up (flush queues, close connections) before returning.
 *
 * Registration:
 *   builder.Services.AddHostedService<ApplicationLifetimeService>();
 *   Multiple services are started in registration order and stopped in reverse.
 *
 * BackgroundService (abstract base):
 *   Provides StartAsync / StopAsync and a linked-cancellation ExecuteAsync(token).
 *   Use it for polling loops or queue processors. Use raw IHostedService here
 *   because this service only registers event callbacks -- no loop needed.
 *
 * Pitfall: An exception thrown inside StartAsync does NOT stop the host by default.
 *          Set builder.Services.Configure<HostOptions>(o => o.BackgroundServiceExceptionBehavior
 *              = BackgroundServiceExceptionBehavior.StopHost) to change this.
 */

/*
 * SECTION 15: IHostApplicationLifetime -- APPLICATION LIFETIME EVENTS
 *
 * IHostApplicationLifetime exposes three CancellationToken properties:
 *
 *   +----------------------+--------------------------------------------------+
 *   | Property             | When the token is cancelled (callback fires)     |
 *   +----------------------+--------------------------------------------------+
 *   | ApplicationStarted   | Host is fully running. Kestrel is listening.     |
 *   |                      | All IHostedService.StartAsync calls have returned.|
 *   +----------------------+--------------------------------------------------+
 *   | ApplicationStopping  | Graceful shutdown has been initiated (SIGTERM,   |
 *   |                      | Ctrl+C, or StopApplication()). In-flight requests|
 *   |                      | may still be processing.                         |
 *   +----------------------+--------------------------------------------------+
 *   | ApplicationStopped   | Host shutdown complete. All hosted services have |
 *   |                      | returned from StopAsync.                         |
 *   +----------------------+--------------------------------------------------+
 *
 * Registering a callback:
 *   CancellationTokenRegistration reg = lifetime.ApplicationStarted.Register(callback);
 *   Store the returned registration and call reg.Dispose() in StopAsync
 *   to prevent the callback from firing after the service is torn down.
 *
 * Alternative -- inline in Program.cs (no hosted service needed):
 *   app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine("Ready!"));
 *
 * Pitfall: Callbacks run on a thread-pool thread. Do not perform long synchronous
 *          work inside them -- it delays the corresponding lifecycle phase completing.
 * Pitfall: ApplicationStopped callbacks run AFTER StopAsync returns for all services.
 *          If a service does not return from StopAsync before the shutdown timeout,
 *          ApplicationStopped may never fire.
 */

/*
 * SECTION 16: GRACEFUL SHUTDOWN
 *
 * The host initiates shutdown via:
 *   SIGTERM                     Linux / container orchestrators (Kubernetes terminates pods).
 *   CTRL+C / CTRL+BREAK         Windows and Linux console hosts.
 *   IHostApplicationLifetime.StopApplication()  -- programmatic; use for unrecoverable errors.
 *
 * Shutdown sequence:
 *   1. ApplicationStopping fires.
 *   2. Web server stops accepting NEW connections; in-flight requests drain.
 *   3. IHostedService.StopAsync called in REVERSE registration order.
 *   4. ApplicationStopped fires.
 *
 * Shutdown timeout (default 5 seconds):
 *   builder.Services.Configure<HostOptions>(options =>
 *       options.ShutdownTimeout = TimeSpan.FromSeconds(30));
 *   If any StopAsync exceeds the timeout, the host logs a warning and force-exits.
 *
 * StopApplication() use cases:
 *   - Health check detects an unrecoverable dependency failure at startup.
 *   - A critical configuration value is absent; app must not accept traffic.
 *   - A background job processor drains a queue and the app should self-terminate.
 *
 * Pitfall: Calling StopApplication() inside OnApplicationStarted (ApplicationStarted
 *          callback) triggers an immediate shutdown -- use it only when the app
 *          genuinely cannot serve requests.
 * Pitfall: In Kubernetes, set the shutdown timeout LONGER than terminationGracePeriodSeconds
 *          minus a buffer, otherwise the pod is force-killed before cleanup finishes.
 */
public sealed class ApplicationLifetimeService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime; // injected by the DI container
    private readonly ILogger<ApplicationLifetimeService> _logger;

    // Store registrations so they can be disposed in StopAsync (avoids stale callbacks)
    private CancellationTokenRegistration _startedRegistration;
    private CancellationTokenRegistration _stoppingRegistration;
    private CancellationTokenRegistration _stoppedRegistration;

    public ApplicationLifetimeService(
        IHostApplicationLifetime lifetime,                   // Section 15: lifetime events
        ILogger<ApplicationLifetimeService> logger)
    {
        _lifetime = lifetime;
        _logger   = logger;
    }

    /*
     * Section 14: StartAsync -- register lifetime callbacks; return immediately.
     * Do not block; the web server cannot start until all StartAsync calls return.
     */
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Section 15: subscribe to all three lifetime tokens
        _startedRegistration  = _lifetime.ApplicationStarted.Register(OnApplicationStarted);
        _stoppingRegistration = _lifetime.ApplicationStopping.Register(OnApplicationStopping);
        _stoppedRegistration  = _lifetime.ApplicationStopped.Register(OnApplicationStopped);

        _logger.LogInformation("ApplicationLifetimeService: registered lifetime callbacks.");
        return Task.CompletedTask; // return immediately -- no long-running work here
    }

    /*
     * Section 14: StopAsync -- clean up registrations during graceful shutdown.
     * cancellationToken fires if StopAsync exceeds the configured shutdown timeout.
     */
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Dispose so callbacks are not invoked after service teardown
        _startedRegistration.Dispose();
        _stoppingRegistration.Dispose();
        _stoppedRegistration.Dispose();

        _logger.LogInformation("ApplicationLifetimeService: disposed lifetime registrations.");
        return Task.CompletedTask;
    }

    // Section 15: ApplicationStarted fires after Kestrel is listening; app is ready
    private void OnApplicationStarted()
    {
        _logger.LogInformation(
            "APPLICATION STARTED. PID={PID} | Kestrel is listening. App ready to serve requests.",
            Environment.ProcessId); // System.Environment.ProcessId (available since .NET 5)
    }

    // Section 16: ApplicationStopping fires when shutdown begins; in-flight requests still run
    private void OnApplicationStopping()
    {
        _logger.LogInformation(
            "APPLICATION STOPPING. Draining in-flight requests before StopAsync is called.");

        // Uncomment to trigger a programmatic self-shutdown from inside the app:
        // _lifetime.StopApplication();
    }

    // Section 16: ApplicationStopped fires after all IHostedService.StopAsync calls return
    private void OnApplicationStopped()
    {
        _logger.LogInformation(
            "APPLICATION STOPPED. All hosted services completed StopAsync. Process will exit.");
    }
}
