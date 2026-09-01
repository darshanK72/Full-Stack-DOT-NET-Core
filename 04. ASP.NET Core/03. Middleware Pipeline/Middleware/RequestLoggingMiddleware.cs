/*
 * FILE ROLE: Demonstrates IMiddleware — the interface-based approach for writing custom
 *            middleware that integrates with the ASP.NET Core DI container.
 *
 * SECTIONS IN THIS FILE:
 *   1. IMiddleware interface contract and DI integration
 *   2. HttpContext access: Request, Response, Connection
 *   3. Short-circuiting the pipeline (returning without calling next)
 */

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MiddlewarePipeline.Middleware;

/*
 * SECTION 1: IMiddleware — CLASS-BASED MIDDLEWARE WITH DEPENDENCY INJECTION
 *
 * IMiddleware is the strongly-typed interface for ASP.NET Core middleware. It defines
 * exactly one method:
 *
 *   Task InvokeAsync(HttpContext context, RequestDelegate next)
 *
 * Unlike convention-based middleware (see TimingMiddleware.cs), IMiddleware:
 *   • Is checked at compile time — if the signature is wrong, it will not build
 *   • Is resolved from the DI container on EVERY request (not once at startup)
 *   • Can be registered as Transient, Scoped, or Singleton
 *   • Supports full DI in the constructor — any service lifetime works
 *   • Is straightforward to unit test: just instantiate the class and call InvokeAsync
 *
 * Requirements for IMiddleware:
 *   1. Class implements IMiddleware
 *   2. DI registration:    builder.Services.AddTransient<RequestLoggingMiddleware>()
 *   3. Pipeline addition:  app.UseMiddleware<RequestLoggingMiddleware>()
 *                          (or the UseXxx extension — see Extensions/MiddlewareExtensions.cs)
 *
 * Lifetime guidance:
 *   AddTransient<T>  — new instance each request; safe for any service
 *   AddScoped<T>     — same instance within a request; good for DbContext-consuming middleware
 *   AddSingleton<T>  — one instance for the app lifetime; avoid if it uses scoped services
 */
public sealed class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger; // injected from DI

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger; // store the DI-injected logger for use in InvokeAsync
    }

    /*
     * SECTION 2: HttpContext — REQUEST, RESPONSE, AND CONNECTION
     *
     * HttpContext is the single object that carries the complete HTTP request/response cycle.
     * Every middleware receives it as its first parameter and can read or write any property.
     *
     *   HttpContext.Request — incoming HTTP details
     *     .Method            — verb: "GET", "POST", "DELETE", etc.
     *     .Path              — "/api/products/42"
     *     .QueryString       — "?page=2&size=10"
     *     .Headers           — IHeaderDictionary (case-insensitive)
     *     .Body              — readable Stream; read once only (buffer with EnableBuffering())
     *     .ContentType       — MIME type of the request body
     *     .Form              — parsed form fields (requires content-type: application/x-www-form-urlencoded)
     *
     *   HttpContext.Response — outgoing HTTP details
     *     .StatusCode        — 200, 201, 400, 404, 500, etc.
     *                          MUST be set BEFORE writing the body; cannot change after HasStarted
     *     .Headers           — response headers dictionary
     *     .Body              — writable Stream
     *     .HasStarted        — true once the first byte of the response has been sent;
     *                          once true, StatusCode and headers are locked
     *     .WriteAsync(str)   — convenience extension to write a string body
     *
     *   HttpContext.Connection — low-level transport details
     *     .RemoteIpAddress   — client IP (may be null if behind a proxy without forwarding headers)
     *     .LocalPort         — server port the request arrived on
     *     .ClientCertificate — mTLS client certificate, if configured
     *
     *   Other important members:
     *     .User               — ClaimsPrincipal; populated AFTER UseAuthentication runs
     *     .Items              — IDictionary<object,object?> per-request; share data between components
     *     .RequestServices    — IServiceProvider scoped to this request (for manual DI resolution)
     *     .RequestAborted     — CancellationToken; cancelled if the client disconnects
     */
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // --- IN LEG: capture request details before passing downstream ---
        string method = context.Request.Method;                          // HTTP verb
        string path   = context.Request.Path;                            // URL path
        string? ip    = context.Connection.RemoteIpAddress?.ToString();  // nullable — may be null behind proxy

        _logger.LogInformation(
            "[RequestLogging] → {Method} {Path} from {IP}",
            method, path, ip ?? "unknown");

        /*
         * SECTION 3: SHORT-CIRCUITING — RETURNING WITHOUT CALLING next
         *
         * A middleware can inspect the request and respond directly without forwarding
         * to the rest of the pipeline. This is called "short-circuiting".
         *
         * Short-circuiting is appropriate when:
         *   • The request fails validation (missing API key, bad Content-Type)
         *   • Rate limiting threshold is exceeded
         *   • A maintenance mode flag is active
         *   • A health check should bypass the full auth stack
         *
         * When you short-circuit you own the response — you must set:
         *   1. context.Response.StatusCode  (before writing the body)
         *   2. context.Response.ContentType (optional but good practice)
         *   3. The body — write it and then return (do NOT call next)
         *
         * Important: UPSTREAM middleware (components that called next before this one)
         * still execute their OUT legs. Only DOWNSTREAM components are skipped.
         */
        if (context.Request.Headers.ContainsKey("X-Block-Request"))
        {
            context.Response.StatusCode  = StatusCodes.Status403Forbidden; // set code first
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Blocked by RequestLoggingMiddleware"); // write body
            return; // do NOT call next — downstream middleware is skipped
        }

        // Pass control to the next component in the pipeline
        await next(context); // → all downstream middleware and the endpoint run here

        // --- OUT LEG: response is complete; read the final status code ---
        _logger.LogInformation(
            "[RequestLogging] ← {StatusCode} for {Method} {Path}",
            context.Response.StatusCode, method, path);
    }
}
