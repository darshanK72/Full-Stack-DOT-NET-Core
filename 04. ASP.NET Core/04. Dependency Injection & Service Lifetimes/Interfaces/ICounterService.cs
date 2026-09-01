/*
 * FILE ROLE: Defines the ICounterService abstraction — the contract for the singleton
 *            counter that accumulates call counts across the entire application lifetime.
 * SECTIONS IN THIS FILE:
 *   3. ICounterService — stateful singleton interface contract
 */

using System;

namespace DependencyInjection.Interfaces;

/*
 * SECTION 3: ICOUNTERSERVICE — STATEFUL SINGLETON ABSTRACTION
 *
 * A counter that persists across all HTTP requests is a textbook singleton:
 *   • Created once at first resolution (lazy default)
 *   • Shared by all threads and all requests for the life of the application
 *   • Disposed when the host shuts down
 *
 * Because every concurrent request uses the SAME object, implementations MUST be
 * thread-safe. See Services/SingletonCounterService.cs (SECTION 4) for the
 * thread-safe implementation using Interlocked.Increment.
 *
 * REGISTERED AS:
 *   services.AddSingleton<ICounterService, SingletonCounterService>()
 *
 * CAPTIVE DEPENDENCY RULE:
 *   A singleton may safely depend on other singletons and on transients
 *   (though a transient captured in a singleton lives as long as the singleton).
 *   A singleton MUST NOT depend on a scoped service — that creates a
 *   "captive dependency" where the scoped service is never released.
 *   ASP.NET Core validates this in Development and throws at host.Build().
 *   FULL EXPLANATION → Program.cs SECTION 16.
 */
public interface ICounterService
{
    // Atomically increments the counter and returns the new value
    int Increment();

    // Read-only current value — no side effect
    int Count { get; }

    // Unique identifier — same Guid returned on every resolution (singleton contract)
    Guid InstanceId { get; }
}
