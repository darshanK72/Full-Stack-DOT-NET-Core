/*
 * FILE ROLE: Defines the IMessageService abstraction — the contract used to teach
 *            interface-based registration, constructor injection, and scoped lifetime.
 * SECTIONS IN THIS FILE:
 *   2. IMessageService — interface contract
 */

using System;

namespace DependencyInjection.Interfaces;

/*
 * SECTION 2: INTERFACE CONTRACTS — PROGRAM TO ABSTRACTIONS, NOT IMPLEMENTATIONS
 *
 * The Dependency Inversion Principle (D in SOLID) states that modules should
 * depend on abstractions (interfaces), not on concrete implementations.
 *
 * Interface-based registration separates WHAT a service does from HOW it does it:
 *   services.AddScoped<IMessageService, ScopedMessageService>()
 *   ─── "When any class asks for IMessageService, hand it a ScopedMessageService" ───
 *
 * BENEFITS:
 *   ✓ Swap implementations without touching any consumer (open/closed principle)
 *   ✓ Unit-test consumers by injecting a mock or stub in place of the real service
 *   ✓ Multiple implementations can be registered (different lifetimes or keyed keys)
 *
 * INTERFACE vs CONCRETE REGISTRATION:
 * ┌──────────────────────────────────────────────────────────────────────────────┐
 * │ services.AddScoped<IMessageService, ScopedMessageService>()  ← recommended  │
 * │   → consumer asks for IMessageService; receives ScopedMessageService         │
 * ├──────────────────────────────────────────────────────────────────────────────┤
 * │ services.AddScoped<ScopedMessageService>()                                   │
 * │   → consumer must ask for ScopedMessageService directly; hard to mock/test  │
 * └──────────────────────────────────────────────────────────────────────────────┘
 *
 * See Services/ScopedMessageService.cs (SECTION 5) for the scoped implementation.
 * See Controllers/DemoController.cs (SECTION 7) for constructor injection usage.
 */
public interface IMessageService
{
    // Returns a formatted greeting; the exact text is determined by the concrete class
    string GetMessage(string name);

    // Unique identifier of this service instance — lets callers prove same/different object
    Guid InstanceId { get; }
}
