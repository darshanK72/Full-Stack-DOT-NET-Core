/*
 * FILE ROLE: Singleton implementation of ICounterService. Demonstrates the singleton
 *            lifetime — one instance per application, shared across all threads,
 *            with thread-safe atomic state management.
 * SECTIONS IN THIS FILE:
 *   4. SingletonCounterService — singleton lifetime implementation
 */

using System;
using System.Threading;
using DependencyInjection.Interfaces;

namespace DependencyInjection.Services;

/*
 * SECTION 4: SINGLETON LIFETIME — ONE INSTANCE FOR THE ENTIRE APPLICATION
 *
 * Registered with:  services.AddSingleton<ICounterService, SingletonCounterService>()
 *
 * LIFECYCLE:
 *   Created  → on first resolution (lazy, the ASP.NET Core default)
 *              OR immediately if registered as a pre-built instance:
 *              services.AddSingleton<ICounterService>(new SingletonCounterService())
 *   Alive    → for the full lifetime of the host process
 *   Disposed → IDisposable.Dispose() called when the host shuts down (if implemented)
 *
 * IDEAL FOR:
 *   ✓ In-memory caches (IMemoryCache)
 *   ✓ Application-wide counters, feature-flag snapshots, configuration caches
 *   ✓ IHttpClientFactory-backed HttpClient pools (avoids socket exhaustion)
 *   ✓ Stateful shared resources that must survive across requests
 *
 * THREAD SAFETY REQUIREMENT:
 *   All HTTP requests execute concurrently.  A singleton's mutable state MUST be
 *   protected against concurrent access.  Strategies (from lowest to highest cost):
 *
 *   Interlocked.Increment(ref _count)   — atomic integer ops; no lock overhead
 *   Volatile.Read / Volatile.Write      — memory-visibility guarantee; not atomicity
 *   lock (_lockObj) { ... }             — general-purpose; use for multi-step logic
 *   System.Collections.Concurrent.*    — thread-safe collections (ConcurrentDictionary)
 *
 * DO NOT INJECT INTO A SINGLETON:
 *   ✗ IMessageService (scoped)   — captive dependency; released only on host shutdown
 *   ✗ TransientTimestampService  — allowed but transient lives as long as singleton;
 *                                   memory leak risk if the transient is heavy
 */
public sealed class SingletonCounterService : ICounterService
{
    private int _count; // backing field; all access via Interlocked or Volatile

    // Assigned once at construction; proves same object across all resolutions
    public Guid InstanceId { get; } = Guid.NewGuid();

    /*
     * --- 4a. Thread-safe increment via Interlocked ---
     * Interlocked.Increment performs read-modify-write as a single atomic CPU instruction.
     * Without it, two concurrent threads could both read 5, both write 6, losing one increment
     * (classic lost-update / race condition).
     */
    public int Increment()
    {
        return Interlocked.Increment(ref _count); // atomic: no torn read, no race condition
    }

    /*
     * --- 4b. Volatile read for visibility ---
     * Volatile.Read issues a memory barrier so the calling thread sees the most recently
     * written value from any other thread.  Without it, a CPU or JIT might serve a cached
     * (stale) value from a register or local store buffer.
     */
    public int Count => Volatile.Read(ref _count); // memory barrier: fresh value guaranteed
}
