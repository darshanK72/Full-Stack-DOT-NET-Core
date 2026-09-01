/*
 * FILE ROLE: Scoped implementation of IMessageService. Demonstrates the scoped
 *            lifetime — one instance per HTTP request, shared within that request,
 *            and released when the request ends.
 * SECTIONS IN THIS FILE:
 *   5. ScopedMessageService — scoped lifetime implementation
 */

using System;
using DependencyInjection.Interfaces;

namespace DependencyInjection.Services;

/*
 * SECTION 5: SCOPED LIFETIME — ONE INSTANCE PER REQUEST (OR DI SCOPE)
 *
 * Registered with:  services.AddScoped<IMessageService, ScopedMessageService>()
 *
 * LIFECYCLE:
 *   Created  → when a new DI scope begins; in ASP.NET Core, one scope = one HTTP request
 *   Alive    → for the duration of that scope (the entire request pipeline)
 *   Disposed → when the scope ends; IDisposable.Dispose() is called if implemented
 *
 * IDEAL FOR:
 *   ✓ EF Core DbContext — keeps all reads/writes on the same connection per request
 *   ✓ Unit-of-work objects that track change sets across a request
 *   ✓ Repositories that need a consistent DB connection within a request
 *   ✓ Services holding per-request state: current user, tenant context, trace ID
 *
 * SAME INSTANCE WITHIN ONE REQUEST:
 *   If DemoController and a middleware both inject IMessageService for the same request,
 *   they receive THE SAME ScopedMessageService instance (InstanceId will be equal).
 *   For a different request, a brand-new instance is created (InstanceId differs).
 *
 * DO NOT:
 *   ✗ Let a singleton capture a scoped service in its constructor — captive dependency
 *     (ASP.NET Core throws InvalidOperationException at host.Build() in Development)
 *   ✗ Use a scoped service from a background thread that outlives the HTTP request
 *     → use IServiceScopeFactory to create a fresh scope; see Program.cs SECTION 16
 */
public sealed class ScopedMessageService : IMessageService
{
    // Guid assigned when the scope (request) begins; constant within that request
    public Guid InstanceId { get; } = Guid.NewGuid();

    /*
     * --- 5a. GetMessage — returns greeting with the instance fingerprint ---
     * Two callers in the SAME request both see the SAME InstanceId value.
     * Two callers across DIFFERENT requests see different InstanceId values.
     * This makes the scoped contract directly observable at runtime.
     */
    public string GetMessage(string name)
    {
        // InstanceId fingerprints the object; same within a request, different across requests
        return $"Hello, {name}! [Scoped instance: {InstanceId:N}]";
    }
}
