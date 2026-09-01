/*
 * FILE ROLE: Demonstrates constructor injection (the primary DI pattern) and
 *            [FromServices] method injection in an API controller. Also shows
 *            manual service resolution via HttpContext.RequestServices.
 * SECTIONS IN THIS FILE:
 *   7. Constructor injection in an ASP.NET Core controller
 *   8. [FromServices] method injection per action
 *   9. Manual resolution with IServiceProvider (service locator pattern)
 */

using DependencyInjection.Interfaces;
using DependencyInjection.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DependencyInjection.Controllers;

/*
 * SECTION 7: CONSTRUCTOR INJECTION — THE PRIMARY INJECTION PATTERN
 *
 * Constructor injection is the preferred way to receive services in ASP.NET Core.
 * When the DI container creates a controller for an incoming request, it inspects
 * the constructor, resolves each parameter type from the service registry, and
 * passes the instances automatically.
 *
 * WHY CONSTRUCTOR INJECTION IS PREFERRED:
 *   ✓ Dependencies are EXPLICIT — visible in the constructor signature
 *   ✓ Fail-fast — if a service is not registered, the first request (or startup
 *     with scope validation) throws immediately, not silently at runtime
 *   ✓ Easy to unit-test — pass mock or stub objects directly to the constructor
 *   ✓ Avoids hidden coupling to IServiceProvider (service locator anti-pattern)
 *
 * PROPERTY INJECTION — WHY TO AVOID:
 *   public IMessageService? MessageService { get; set; }  // NOT recommended
 *   • Dependencies are hidden — not part of the class's visible contract
 *   • Allows partially-constructed objects (set-able after creation; nullable risk)
 *   • ASP.NET Core's built-in DI container does NOT support property injection;
 *     third-party containers (Autofac, Castle Windsor) or [FromServices] workarounds
 *     are required — adding complexity without benefit over constructor injection
 *   • Makes unit tests harder — test code must know to set each property
 *
 * CONTROLLER LIFETIME:
 *   Controllers are created per-request (effectively scoped), so injecting scoped
 *   services into the constructor is safe and correct.
 */
[ApiController]
[Route("api/[controller]")]
public sealed class DemoController : ControllerBase
{
    // readonly fields — assigned once in the constructor, never replaced
    private readonly IMessageService _messageService;    // scoped: one per HTTP request
    private readonly ICounterService _counterService;    // singleton: shared across requests
    private readonly ILogger<DemoController> _logger;   // built-in: provided by host logging

    /*
     * --- 7a. Constructor — DI resolves all parameters automatically ---
     * The container matches each parameter type to a registered service descriptor.
     * Adding a new dependency is a one-line constructor change; no factory code needed.
     */
    public DemoController(
        IMessageService messageService,     // resolved → ScopedMessageService (scoped)
        ICounterService counterService,     // resolved → SingletonCounterService (singleton)
        ILogger<DemoController> logger)    // resolved → built-in ILogger (singleton)
    {
        _messageService = messageService;
        _counterService = counterService;
        _logger = logger;
    }

    /*
     * --- 7b. Greet — exercising the injected services ---
     * Hit GET /api/demo/greet/Alice to see:
     *   • TotalGreetCount growing across requests  → singleton state persists
     *   • CounterInstance staying the same Guid    → same singleton object every request
     *   • MessageInstance changing each request    → new scoped object per request
     */
    [HttpGet("greet/{name}")]
    public IActionResult Greet(string name)
    {
        _logger.LogInformation("Greet called for {Name}", name); // structured log: no string interpolation

        int newCount = _counterService.Increment();               // singleton: count accumulates globally
        string message = _messageService.GetMessage(name);       // scoped: same object within this request

        return Ok(new
        {
            Message         = message,
            TotalGreetCount = newCount,                           // grows per-request: proves singleton state
            CounterInstance = _counterService.InstanceId,        // same Guid every request: singleton proof
            MessageInstance = _messageService.InstanceId         // new Guid each request: scoped proof
        });
    }

    /*
     * SECTION 8: [FromServices] METHOD INJECTION
     *
     * [FromServices] instructs the model-binding infrastructure to resolve the annotated
     * parameter from the DI container rather than from the route, query string, or body.
     *
     * USE WHEN:
     *   • A dependency is only needed for ONE action, not the whole controller
     *   • The service is heavyweight and should not be allocated on every request
     *   • You want to make the action's extra dependency explicit in its signature
     *
     * DO NOT USE AS A REPLACEMENT FOR CONSTRUCTOR INJECTION:
     *   • If the service is needed in multiple actions, it belongs in the constructor
     *   • Scattering [FromServices] everywhere hides the controller's true dependencies
     *   • Constructor injection is easier to discover, mock, and maintain
     *
     * RUNTIME BEHAVIOR:
     *   The [FromServices] parameter is resolved from HttpContext.RequestServices —
     *   the scoped IServiceProvider for the current request.  All lifetime rules apply.
     */
    [HttpGet("timestamp")]
    public IActionResult GetTimestamp(
        [FromServices] TransientTimestampService ts) // new TransientTimestampService per call
    {
        // ts.InstanceId changes on every call: proves the transient contract
        return Ok(new
        {
            Timestamp  = ts.GetTimestamp(),
            InstanceId = ts.InstanceId    // different every request: new transient each time
        });
    }

    /*
     * SECTION 9: MANUAL RESOLUTION VIA IServiceProvider — SERVICE LOCATOR PATTERN
     *
     * HttpContext.RequestServices is the scoped IServiceProvider for the current request.
     * GetRequiredService<T> resolves a registered service by type at runtime.
     *
     * GetRequiredService<T>  vs  GetService<T>:
     * ┌──────────────────────────────────────────────────────────────────────────────┐
     * │ GetRequiredService<T> — throws InvalidOperationException if not registered  │ ← prefer
     * │ GetService<T>         — returns null if not registered; requires null check  │
     * └──────────────────────────────────────────────────────────────────────────────┘
     *
     * SERVICE LOCATOR ANTI-PATTERN — USE SPARINGLY:
     *   Resolving from IServiceProvider hides dependencies and makes unit testing harder
     *   (the test must configure a full service provider just to call the method).
     *   Legitimate uses:
     *     • Factory methods where the concrete type is determined at runtime
     *     • Plugin systems with types discovered via reflection
     *     • Startup or bootstrapping code before the DI graph is fully composed
     *   In normal application code, always prefer constructor or [FromServices] injection.
     */
    [HttpGet("manual-resolve")]
    public IActionResult ManualResolve()
    {
        // HttpContext.RequestServices is the scoped IServiceProvider for this request
        var counter = HttpContext.RequestServices.GetRequiredService<ICounterService>();

        return Ok(new
        {
            ManuallyResolvedCount = counter.Count,
            InstanceId            = counter.InstanceId // same Guid as constructor-injected singleton
        });
    }
}
