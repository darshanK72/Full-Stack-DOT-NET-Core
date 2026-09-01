/*
 * FILE ROLE: Demonstrates the three IOptions interfaces and named options retrieval.
 *   Registered as a scoped service so all three interfaces can be injected at once.
 * SECTIONS IN THIS FILE:
 *   1. IOptions<T> vs IOptionsSnapshot<T> vs IOptionsMonitor<T> — comparison + lifetime rules
 *   2. Named options — IOptionsMonitor<T>.Get(name) for multiple configurations
 */

using System;
using Microsoft.Extensions.Options;
using ConfigurationOptions.Models;

namespace ConfigurationOptions.Services;

/*
 * SECTION 1: IOptions<T> vs IOptionsSnapshot<T> vs IOptionsMonitor<T>
 *
 * ASP.NET Core ships three interfaces for consuming registered options.
 * Choosing the wrong one causes either stale data or runtime DI errors.
 *
 * ┌─────────────────────┬───────────────────────────────┬──────────────────┬──────────────────────┐
 * │ Interface           │ When value is resolved        │ DI lifetime      │ Named options        │
 * ├─────────────────────┼───────────────────────────────┼──────────────────┼──────────────────────┤
 * │ IOptions<T>         │ Once at first request to .Value│ Singleton        │ Default only (.Value)│
 * │ IOptionsSnapshot<T> │ Once per DI scope (request)   │ Scoped only      │ .Get(name) per scope │
 * │ IOptionsMonitor<T>  │ Every read of .CurrentValue   │ Singleton safe   │ .Get(name) live      │
 * └─────────────────────┴───────────────────────────────┴──────────────────┴──────────────────────┘
 *
 * --- 1a. IOptions<T> — SINGLETON SNAPSHOT ---
 *
 *   .Value returns the options instance created when the DI container is built.
 *   It NEVER changes, even if appsettings.json is modified on disk.
 *
 *   Lifetime: singleton — safe to inject into controllers, singletons, and hosted services.
 *   Use when: app-wide settings that never change (feature flags from CI, app name, version).
 *   Pitfall:  If you hot-reload config and expect IOptions<T> to reflect new values, it won't.
 *             Use IOptionsMonitor<T> instead for settings that can change at runtime.
 *
 * --- 1b. IOptionsSnapshot<T> — PER-SCOPE RE-READ ---
 *
 *   .Value returns a snapshot computed once per DI scope.  In ASP.NET Core a scope
 *   is created per HTTP request, so .Value is consistent within one request but may
 *   differ from request to request if config reloads between them.
 *
 *   Lifetime: SCOPED — must NOT be injected into singleton services.
 *     If you try: InvalidOperationException at runtime:
 *     "Cannot consume scoped service 'IOptionsSnapshot<T>' from singleton."
 *
 *   Use when: settings that can change between deployments (zero-downtime config
 *     push) and you want consistency within a single request.
 *   Example: DatabaseOptions where the timeout might increase during a maintenance
 *     window — each request gets the current timeout, not the startup-time value.
 *
 * --- 1c. IOptionsMonitor<T> — LIVE UPDATES AND CHANGE CALLBACKS ---
 *
 *   .CurrentValue is recomputed on every access from the latest registered
 *   IConfigureOptions<T> delegates.  When a config provider reports a change
 *   (e.g., appsettings.json saved with reloadOnChange: true), .CurrentValue
 *   immediately reflects the new values.
 *
 *   Lifetime: singleton — safe to inject anywhere, including background services.
 *   Key members:
 *     .CurrentValue          — latest snapshot of the default ("") configuration
 *     .Get(name)             — latest snapshot of a named configuration
 *     .OnChange(callback)    — register a delegate fired on config change;
 *                              returns IDisposable — ALWAYS dispose to avoid leaks
 *
 *   Change notification flow:
 *     File modified on disk → IChangeToken fires → options recomputed
 *     → OnChange callbacks invoked → .CurrentValue updated
 *
 *   Use when: background services, middleware that needs live feature flag checks,
 *     or any singleton that must react to config changes without restarting.
 */

/*
 * SECTION 2: NAMED OPTIONS — IOptionsMonitor<T>.Get(name)
 *
 * Multiple configurations of the same type are registered under string names.
 * Options.DefaultName == string.Empty ("") is the unnamed (default) configuration.
 *
 * Registration (Program.cs SECTION 5):
 *   services.Configure<FeatureFlags>(config.GetSection("FeatureFlags"));          // name: ""
 *   services.Configure<FeatureFlags>("Admin", config.GetSection("AdminFeatureFlags")); // name: "Admin"
 *   services.Configure<FeatureFlags>("Api",   config.GetSection("ApiFeatureFlags"));   // name: "Api"
 *
 * Retrieval:
 *   monitor.CurrentValue     — same as monitor.Get(Options.DefaultName) == Get("")
 *   monitor.Get("Admin")     — Admin-specific configuration
 *   monitor.Get("Api")       — API-specific configuration
 *
 * IOptionsSnapshot<T>.Get(name) also supports named options but is scoped (per request).
 * IOptions<T>.Value only ever returns the DEFAULT ("") configuration.
 */
public sealed class ConfigurationDemoService : IDisposable
{
    private readonly IOptions<AppSettings>          _appOptions;     // singleton snapshot
    private readonly IOptionsSnapshot<DatabaseOptions> _dbSnapshot;  // per-scope re-read
    private readonly IOptionsMonitor<FeatureFlags>  _featureMonitor; // live + named
    private readonly IDisposable?                   _changeListener; // OnChange cleanup token

    public ConfigurationDemoService(
        IOptions<AppSettings>             appOptions,
        IOptionsSnapshot<DatabaseOptions> dbSnapshot,
        IOptionsMonitor<FeatureFlags>     featureMonitor)
    {
        _appOptions      = appOptions;
        _dbSnapshot      = dbSnapshot;
        _featureMonitor  = featureMonitor;

        // OnChange fires whenever FeatureFlags config reloads (e.g., file saved on disk).
        // Returns IDisposable — store it and Dispose() to unsubscribe and prevent memory leaks.
        _changeListener = _featureMonitor.OnChange(static (flags, name) =>
        {
            Console.WriteLine($"[IOptionsMonitor] FeatureFlags changed (name: '{name ?? "default"}')");
            Console.WriteLine($"  EnableBetaApi is now: {flags.EnableBetaApi}");
        });
    }

    // --- IOptions<T> ————————————————————————————————————————————————————————

    /* Uses the singleton AppSettings snapshot — value is fixed after app startup. */
    public string GetAppInfo()
    {
        AppSettings s = _appOptions.Value; // resolved once; same object every call
        return $"App: {s.AppName} | Version: {s.Version} [IOptions<AppSettings>]";
    }

    // --- IOptionsSnapshot<T> ————————————————————————————————————————————————

    /* Uses the per-scope DatabaseOptions snapshot — re-read once per HTTP request. */
    public string GetDatabaseInfo()
    {
        DatabaseOptions db = _dbSnapshot.Value; // fresh per DI scope (HTTP request)
        return $"Timeout: {db.CommandTimeoutSeconds}s | Retries: {db.MaxRetryCount} [IOptionsSnapshot<DatabaseOptions>]";
    }

    // --- IOptionsMonitor<T> — default and named ——————————————————————————————

    /* Returns the live default FeatureFlags — always reflects the current config state. */
    public string GetFeatureInfo()
    {
        FeatureFlags f = _featureMonitor.CurrentValue; // never stale; updated on reload
        return $"Dashboard: {f.EnableNewDashboard} | Beta: {f.EnableBetaApi} | " +
               $"MaxUpload: {f.MaxUploadSizeMb}MB [IOptionsMonitor default]";
    }

    /* Returns a named FeatureFlags configuration — demonstrates .Get(name) for named options. */
    public string GetNamedFeatureInfo(string name)
    {
        FeatureFlags f = _featureMonitor.Get(name); // retrieve named configuration
        return $"[{name}] Dashboard: {f.EnableNewDashboard} | Beta: {f.EnableBetaApi} | " +
               $"MaxUpload: {f.MaxUploadSizeMb}MB [IOptionsMonitor.Get(\"{name}\")]";
    }

    // Dispose unsubscribes the OnChange listener — prevents a GC-root leak.
    public void Dispose() => _changeListener?.Dispose();
}
