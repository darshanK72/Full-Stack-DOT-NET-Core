/*
 * FILE ROLE: GlobalExceptionHandler — implements IExceptionHandler (.NET 8+),
 *            the central hub that intercepts every unhandled exception and writes
 *            a structured ProblemDetails response with a correlation ID.
 * SECTIONS IN THIS FILE:
 *   1. IExceptionHandler interface overview (.NET 8+)
 *   2. IExceptionHandlerFeature — reading the captured exception in older patterns
 *   3. ProblemDetails (RFC 7807) — mandatory fields and extensions
 *   4. CorrelationId — linking response errors to server logs
 *   5. Logging exceptions with ILogger
 *   6. GlobalExceptionHandler implementation
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExceptionHandling.Handlers;

/*
 * SECTION 1: IExceptionHandler INTERFACE (.NET 8+)
 *
 * Introduced in .NET 8, IExceptionHandler moves exception-handling logic out of
 * the middleware pipeline declaration and into a dedicated, injectable class.
 *
 * Advantages over the older UseExceptionHandler lambda approach:
 *
 *   Older lambda (Program.cs)               IExceptionHandler (.NET 8+)
 *   ────────────────────────────────────    ──────────────────────────────────────
 *   Logic inline in Program.cs              Dedicated class, easy to unit-test
 *   DI injection via captured closure       Constructor injection works naturally
 *   Single handler only                     Multiple handlers tried in order (chain)
 *   Hard to reuse across projects           Portable — move to a shared library
 *
 * Registration (Program.cs):
 *   builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
 *   builder.Services.AddProblemDetails();
 *   app.UseExceptionHandler();   // no path argument needed with IExceptionHandler
 *
 * Chain of responsibility:
 *   TryHandleAsync returns true  → exception consumed; response written; chain stops.
 *   TryHandleAsync returns false → next registered IExceptionHandler is tried.
 *   All handlers return false    → UseExceptionHandler writes a default 500 response
 *                                   (a bare ProblemDetails when AddProblemDetails is registered).
 *
 * You can register multiple handlers in priority order:
 *   builder.Services.AddExceptionHandler<DomainExceptionHandler>();
 *   builder.Services.AddExceptionHandler<FallbackExceptionHandler>();
 */

/*
 * SECTION 2: IExceptionHandlerFeature — READING THE CAPTURED EXCEPTION
 *
 * When UseExceptionHandler() is in the pipeline, ASP.NET Core wraps the
 * original request in a try/catch and stores the caught exception in the
 * HttpContext feature collection as IExceptionHandlerFeature.
 *
 * Reading it manually (older pattern — lambda or error path endpoint):
 *   var feature = context.Features.Get<IExceptionHandlerFeature>();
 *   Exception? ex = feature?.Error;       // the caught exception
 *   string? path = (feature as IExceptionHandlerPathFeature)?.Path; // original path
 *
 * With IExceptionHandler (.NET 8+):
 *   The exception is passed directly as a parameter to TryHandleAsync —
 *   you do not need to read IExceptionHandlerFeature yourself.
 *   It is still available if you need the original request path:
 *   var path = httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path;
 *
 * Lambda overload (still valid — useful for simple projects):
 *   app.UseExceptionHandler(errorApp =>
 *   {
 *       errorApp.Run(async ctx =>
 *       {
 *           var feature = ctx.Features.Get<IExceptionHandlerFeature>();
 *           Exception? ex = feature?.Error;
 *           ctx.Response.StatusCode  = 500;
 *           ctx.Response.ContentType = "application/problem+json";
 *           await ctx.Response.WriteAsJsonAsync(new { error = ex?.Message });
 *       });
 *   });
 *
 * Path overload (re-executes a controller endpoint):
 *   app.UseExceptionHandler("/error");
 *   // Then GET /error reads IExceptionHandlerFeature and returns a response.
 *   // Simpler than a lambda but requires a real endpoint at /error.
 */

/*
 * SECTION 3: ProblemDetails — RFC 7807 STRUCTURED ERROR SHAPE
 *
 * RFC 7807 "Problem Details for HTTP APIs" defines a standard JSON error body
 * with Content-Type: application/problem+json.
 *
 *   Field        Type    Required  Description
 *   ──────────   ──────  ────────  ─────────────────────────────────────────────
 *   type         string  no        URI reference identifying the problem class
 *   title        string  no        Human-readable summary (same for all instances)
 *   status       int     no        HTTP status code (mirrors the response status)
 *   detail       string  no        Human-readable, instance-specific explanation
 *   instance     string  no        URI reference to this specific occurrence
 *   extensions   any     no        Custom fields — e.g. "errors", "traceId", "correlationId"
 *
 * Extensions and [JsonExtensionData]:
 *   ProblemDetails.Extensions is marked [JsonExtensionData] so System.Text.Json
 *   serialises its entries as FLAT top-level JSON properties — not nested under
 *   an "extensions" key. This matches the RFC 7807 spec for additional members.
 *
 * AddProblemDetails() (Program.cs):
 *   Registers IProblemDetailsService which middleware and controllers can use
 *   to write consistent ProblemDetails responses. Also configures the default
 *   UseExceptionHandler fallback to return ProblemDetails for 500 errors.
 *
 * ValidationProblemDetails:
 *   A subtype that adds an "errors" dictionary (field → string[]) — matches
 *   the shape ASP.NET Core model validation returns automatically.
 */

/*
 * SECTION 4: CORRELATIONID — LINKING ERRORS TO SERVER LOGS
 *
 * A correlation ID is a unique string that appears in BOTH the HTTP error
 * response body AND the server-side log entries, enabling support teams to
 * search logs for the exact request that caused a reported error.
 *
 * Resolution strategy (priority order):
 *   1. X-Correlation-Id header  — set by the client or an API gateway upstream.
 *   2. X-Request-Id header      — alternative header name used by some gateways.
 *   3. HttpContext.TraceIdentifier — ASP.NET Core assigns one per request;
 *                                    guaranteed present, unique per process restart.
 *
 * Best practice:
 *   • Echo the incoming ID back in the response: the client can log it client-side
 *     and match against server logs without sharing stack traces.
 *   • Also set it as a response header (X-Correlation-Id) — some clients read
 *     headers before parsing the body.
 *   • Do NOT include stack traces in production responses — correlation ID is the
 *     safe bridge between client report and server log.
 */

/*
 * SECTION 5: LOGGING EXCEPTIONS WITH ILogger
 *
 * Best practices for exception logging in ASP.NET Core:
 *
 *   Level       When to use
 *   ─────────   ────────────────────────────────────────────────────────────────
 *   LogWarning  Expected domain exceptions (NotFoundException, ValidationException)
 *               — not a bug; part of normal operation; still useful in logs.
 *   LogError    Unexpected / unhandled exceptions — likely a bug or infra failure.
 *   LogCritical Infrastructure failures that require immediate human intervention.
 *
 * Always pass the Exception as the first argument to LogWarning/LogError:
 *   _logger.LogError(exception, "Message with {StructuredProperty}", value);
 *   ✓ Log providers (Serilog, Seq, ELK) capture the full stack trace, inner
 *     exceptions, and exception type as searchable structured fields.
 *   ✗ _logger.LogError(exception.ToString()); — flattened string; loses structure.
 *
 * Add domain context as structured properties:
 *   _logger.LogWarning(ex, "Domain {ExceptionType} code={Code} for {CorrelationId}",
 *       ex.GetType().Name, appEx.ErrorCode, correlationId);
 *   These become searchable fields in Seq, Application Insights, Datadog, etc.
 */

/*
 * SECTION 6: GlobalExceptionHandler IMPLEMENTATION
 *
 * Handles all AppException subtypes (NotFoundException, ValidationException, etc.)
 * and returns a ProblemDetails body. Returns false for unknown exceptions so
 * UseExceptionHandler's default 500 handler writes the final response.
 *
 * In production you may prefer to return true for ALL exceptions and always write
 * a generic 500 ProblemDetails — this prevents ASP.NET Core from writing a plain
 * text fallback response. The chain-of-responsibility return value shown here is
 * most useful when multiple IExceptionHandler implementations are registered.
 */
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Resolve correlation ID — prefer incoming gateway header, fall back to ASP.NET trace ID
        string correlationId = httpContext.Request.Headers["X-Correlation-Id"].ToString()
            is { Length: > 0 } incomingId
            ? incomingId
            : httpContext.TraceIdentifier;

        // Echo the ID back as a response header so callers can retrieve it without parsing JSON
        httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

        if (exception is AppException appEx)
        {
            // Expected domain path — log at Warning (not a bug; still worth recording)
            _logger.LogWarning(exception,
                "Domain exception {ExceptionType} (errorCode={ErrorCode}) for correlationId={CorrelationId}",
                exception.GetType().Name, appEx.ErrorCode, correlationId);

            int statusCode = appEx.StatusCode;

            // Build RFC 7807 ProblemDetails response
            ProblemDetails problem = new ProblemDetails
            {
                Status  = statusCode,
                Title   = ReasonPhrase(statusCode), // e.g. "Not Found"
                Detail  = exception.Message,         // instance-specific explanation
                Type    = $"https://httpstatuses.io/{statusCode}"
            };

            // Correlation ID goes into Extensions — serialised as a flat JSON field
            // because ProblemDetails.Extensions carries [JsonExtensionData]
            problem.Extensions["correlationId"] = correlationId;

            // ValidationException carries per-field error details — add to extensions
            if (appEx is ValidationException validationEx)
            {
                problem.Extensions["errors"] = validationEx.Errors; // serialised as JSON array
            }

            httpContext.Response.StatusCode  = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            // WriteAsJsonAsync respects [JsonExtensionData] — extensions are flattened
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true; // exception consumed — response written; pipeline stops here
        }

        // Unknown exception — log at Error (unexpected; likely a bug or infra failure)
        _logger.LogError(exception,
            "Unhandled {ExceptionType} for correlationId={CorrelationId}",
            exception.GetType().Name, correlationId);

        // Return false → UseExceptionHandler writes a default 500 ProblemDetails response
        // (because AddProblemDetails() is registered).
        return false;
    }

    // Maps common HTTP status codes to their standard RFC 7231 reason phrase
    private static string ReasonPhrase(int status) => status switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        429 => "Too Many Requests",
        500 => "Internal Server Error",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        _   => "Error"
    };
}
