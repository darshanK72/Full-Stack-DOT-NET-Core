/*
 * FILE ROLE: Transient service (registered as concrete type, no interface) that
 *            captures its construction time. Demonstrates transient lifetime —
 *            a brand-new instance created on every injection point.
 * SECTIONS IN THIS FILE:
 *   6. TransientTimestampService — transient lifetime implementation
 */

using System;

namespace DependencyInjection.Services;

/*
 * SECTION 6: TRANSIENT LIFETIME — NEW INSTANCE ON EVERY INJECTION POINT
 *
 * Registered with:  services.AddTransient<TransientTimestampService>()
 *   (or with an interface: services.AddTransient<ITimestampService, TransientTimestampService>())
 *
 * LIFECYCLE:
 *   Created  → every time the service is requested from the container
 *   Alive    → until the consumer is done; no shared state between callers
 *   Disposed → when the current scope ends (IDisposable.Dispose if implemented)
 *
 * IDEAL FOR:
 *   ✓ Lightweight, stateless helpers: formatters, validators, mappers, calculators
 *   ✓ Services that MUST NOT share state between callers
 *   ✓ Short-lived helpers where per-use allocation is cheap
 *
 * KEY DIFFERENCE FROM SCOPED:
 *   Even within ONE request, two classes injecting TransientTimestampService each
 *   get a DIFFERENT instance (different InstanceId).  Scoped gives the SAME instance.
 *
 * CAUTION:
 *   Avoid transient services that own heavy resources (DB connections, large byte arrays,
 *   stream handles) because those resources are reallocated on every injection, increasing
 *   GC pressure and risking resource exhaustion under load.
 *
 * LIFETIME COMPARISON:
 * ┌─────────────────┬──────────────────────────────┬─────────────────────────────────┐
 * │ Lifetime        │ Instances created            │ Shared within a request?        │
 * ├─────────────────┼──────────────────────────────┼─────────────────────────────────┤
 * │ AddSingleton    │ One per application          │ All requests, all threads       │
 * │ AddScoped       │ One per scope / request      │ Yes — same object per request   │
 * │ AddTransient    │ One per injection point      │ No  — always a new object       │
 * └─────────────────┴──────────────────────────────┴─────────────────────────────────┘
 */
public sealed class TransientTimestampService
{
    // DateTime.UtcNow captured at the moment of construction; changes every new instance
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    // InstanceId differs between callers even within the same request (transient proof)
    public Guid InstanceId { get; } = Guid.NewGuid();

    /*
     * --- 6a. GetTimestamp — returns construction time and instance identity ---
     * Calling the /api/demo/timestamp endpoint twice produces two different InstanceId
     * values, proving the transient contract: no sharing, ever.
     */
    public string GetTimestamp()
    {
        // :O → round-trip ISO-8601 format (e.g. 2025-03-14T12:34:56.7890000Z)
        return $"Created at {CreatedAt:O} [Transient instance: {InstanceId:N}]";
    }
}
