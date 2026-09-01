/*
 * FILE ROLE: Demonstrates the convention-based approach for custom middleware — no interface,
 *            no DI registration; recognised by ASP.NET Core through naming conventions.
 *
 * SECTIONS IN THIS FILE:
 *   1. Convention-based middleware contract and IMiddleware comparison
 *   2. Measuring elapsed time across in/out legs
 *   3. Adding a response header in the out leg
 */

using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MiddlewarePipeline.Middleware;

/*
 * SECTION 1: CONVENTION-BASED MIDDLEWARE
 *
 * ASP.NET Core can recognise a middleware class by CONVENTION — no interface required.
 * The framework looks for:
 *
 *   1. A public constructor whose FIRST parameter is RequestDelegate
 *      (additional constructor parameters are resolved from DI at startup)
 *
 *   2. A method named exactly Invoke or InvokeAsync that:
 *      • Returns Task
 *      • Takes HttpContext as its FIRST parameter
 *      • May accept additional parameters injected from DI (scoped services go here,
 *        NOT in the constructor, because the class is instantiated once at startup)
 *
 * Example skeleton:
 *
 *   public class MyMiddleware
 *   {
 *       private readonly RequestDelegate _next;
 *
 *       public MyMiddleware(RequestDelegate next) { _next = next; }
 *
 *       public async Task InvokeAsync(HttpContext context)
 *       {
 *           // in leg
 *           await _next(context);
 *           // out leg
 *       }
 *   }
 *
 * IMiddleware vs Convention-based — comparison:
 * ┌────────────────────────────┬──────────────────────────┬────────────────────────────┐
 * │ Aspect                     │ IMiddleware               │ Convention-based           │
 * ├────────────────────────────┼──────────────────────────┼────────────────────────────┤
 * │ Contract enforced at       │ Compile time (interface)  │ Startup (reflection check) │
 * │ DI registration required   │ Yes                       │ No                         │
 * │ Resolved from DI           │ Per request               │ Never (ctor runs at startup│
 * │ Scoped services in ctor    │ Yes (resolved per request)│ Inject via InvokeAsync     │
 * │ Effective lifetime         │ Whatever registered       │ Singleton (one instance)   │
 * │ Unit test effort           │ Easy (instantiate + call) │ Need RequestDelegate mock  │
 * └────────────────────────────┴──────────────────────────┴────────────────────────────┘
 *
 * IMPORTANT — singleton implications:
 *   Convention-based middleware is instantiated ONCE when the pipeline is built.
 *   Storing per-request state in fields is a concurrency bug. Use HttpContext.Items
 *   for per-request data sharing between middleware components instead.
 */
public sealed class TimingMiddleware
{
    private readonly RequestDelegate _next; // holds the next component in the pipeline

    public TimingMiddleware(RequestDelegate next)
    {
        _next = next; // framework passes the compiled pipeline continuation here
    }

    /*
     * SECTION 2: MEASURING ELAPSED TIME ACROSS IN/OUT LEGS
     *
     * Because code before _next() is the IN leg and code after is the OUT leg,
     * a Stopwatch started immediately before calling _next and stopped immediately
     * after measures the TOTAL time spent by all downstream middleware + the endpoint.
     *
     * This is the canonical illustration of the pipeline's "Russian doll" structure:
     *   • Outer middleware wraps inner middleware in time
     *   • The outermost middleware (first registered) measures the full request duration
     *   • An inner middleware measures only its own slice of the pipeline
     *
     * Stopwatch.StartNew() is preferred over new Stopwatch() + .Start() — it is a
     * one-liner that both constructs and starts the timer atomically.
     */
    public async Task InvokeAsync(HttpContext context)
    {
        Stopwatch sw = Stopwatch.StartNew(); // start timer — IN leg begins

        await _next(context); // ← the entire downstream pipeline runs inside this await

        sw.Stop(); // OUT leg — all downstream has finished; measure total elapsed time

        /*
         * SECTION 3: ADDING A RESPONSE HEADER IN THE OUT LEG
         *
         * Response headers can be written from the out leg as long as the response body
         * has not started (context.Response.HasStarted == false). Once the framework
         * begins flushing the response to the network, headers are locked.
         *
         * Custom headers (X-* prefix) are the safe way to expose internal metrics:
         *   X-Response-Time-Ms — total milliseconds for this request (timing middleware)
         *   X-Request-Id       — correlation id (usually set in the in leg)
         *
         * HasStarted guard: if a downstream component already started writing (e.g. a
         * streaming response), attempting to set headers would throw an InvalidOperationException.
         * Always guard with HasStarted before writing headers from the out leg.
         */
        if (!context.Response.HasStarted)                              // headers not yet flushed
        {
            context.Response.Headers["X-Response-Time-Ms"] =
                sw.Elapsed.TotalMilliseconds.ToString("F2");           // e.g. "4.37"
        }

        Console.WriteLine(
            $"[Timing] {context.Request.Method} {context.Request.Path} " +
            $"→ {context.Response.StatusCode} in {sw.Elapsed.TotalMilliseconds:F2} ms");
    }
}
