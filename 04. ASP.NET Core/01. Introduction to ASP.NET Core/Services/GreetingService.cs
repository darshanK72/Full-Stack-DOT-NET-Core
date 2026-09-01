/*
 * FILE ROLE: Stateless service registered with the DI container to demonstrate
 *            service definition, registration, and resolution in ASP.NET Core's
 *            built-in dependency injection system.
 * SECTIONS IN THIS FILE:
 *   12. GreetingService — service class, DI lifetime choice, resolution mechanics
 */

namespace IntroductionToAspNetCore.Services;

/*
 * SECTION 12: SERVICE CLASS — GreetingService
 * ─────────────────────────────────────────────────────────────────────────────
 * In ASP.NET Core, a "service" is any class registered with IServiceCollection
 * (builder.Services) and resolved automatically by the framework into:
 *   • Endpoint handler parameters  (Minimal API style — used in this chapter)
 *   • Controller constructors      (MVC / Web API style)
 *   • Middleware constructors
 *   • Other service constructors   (constructor-injection chain)
 *
 * SERVICE DESIGN PRINCIPLES:
 *   • Depend on abstractions (interfaces), not concrete types — allows unit
 *     tests to swap implementations via mocks. The interface pattern is omitted
 *     here to keep the intro focused on the hosting model; see the full pattern
 *     in 04. Dependency Injection & Service Lifetimes.
 *   • Stateless services are safe as Singleton — shared across all requests,
 *     no risk of stale or corrupted per-request data.
 *   • Stateful services (DbContext, HttpClient) should be Scoped or Transient
 *     so each request gets its own isolated instance.
 *
 * LIFETIME RECAP (registration is in Program.cs, SECTION 4):
 *
 *   Registration call                Lifetime               When to use
 *   ────────────────────────────     ─────────────────      ─────────────────────────────
 *   AddSingleton<GreetingService>()  App lifetime (1 ever)  Stateless, thread-safe logic
 *   AddScoped<GreetingService>()     Per HTTP request        DbContext, unit-of-work types
 *   AddTransient<GreetingService>()  Every resolution        Lightweight, cheap factories
 *
 *   GreetingService has no mutable state → AddSingleton is the correct choice.
 *   Using Scoped or Transient would also compile and run, but wastes allocations
 *   for a service that never changes between requests.
 *
 * HOW RESOLUTION WORKS (minimal API style):
 *   1. app.MapGet("/greet/{name}", (string name, GreetingService svc) => { … })
 *   2. The framework inspects handler parameters via reflection.
 *   3. Parameters not matched to route/query values are resolved from the DI
 *      container — GreetingService is found as a registered Singleton.
 *   4. The Singleton instance is passed as `svc` for each request (same object,
 *      no allocation overhead after the first resolution).
 *
 * COVERED IN DETAIL LATER → 04. Dependency Injection & Service Lifetimes
 */
public sealed class GreetingService
{
    /*
     * --- 12a. Greet ---
     * Returns a personalised greeting string for the given caller name.
     * Expression-bodied member (=>) is idiomatic for single-expression methods.
     * No instance state accessed → pure function, safe for Singleton lifetime.
     */
    public string Greet(string name) => // name supplied by route value or direct caller
        $"Hello, {name}! Welcome to ASP.NET Core."; // string interpolation builds the message
}
