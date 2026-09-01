/*
 * FILE ROLE: Teaches IAsyncExceptionFilter — centralized exception handling
 *            that converts unhandled MVC exceptions into structured RFC 7807
 *            ProblemDetails API responses, covering ExceptionContext,
 *            ExceptionHandled, and exception-type mapping.
 * SECTIONS IN THIS FILE:
 *   5. IAsyncExceptionFilter — ExceptionContext, ExceptionHandled, ProblemDetails
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Filters.Filters;

/*
 * SECTION 5: IAsyncExceptionFilter — GLOBAL EXCEPTION FILTER
 * ─────────────────────────────────────────────────────────────────────────────
 * The exception filter stage sits between the Action and Result stages and
 * catches unhandled exceptions thrown by:
 *   ✓ Action methods
 *   ✓ Action filters (IActionFilter / IAsyncActionFilter)
 *   ✓ Result filters (IResultFilter / IAsyncResultFilter)
 *
 * It does NOT catch exceptions from:
 *   ✗ Middleware that runs before the MVC routing step
 *   ✗ Authorization filters (they wrap the exception filter stage)
 *   ✗ Resource filters (same — they wrap everything inside)
 *   ✗ Minimal API endpoint handlers
 *   ✗ Background services (IHostedService)
 *
 * IAsyncExceptionFilter — ONE method:
 *   Task OnExceptionAsync(ExceptionContext context)
 *
 * ExceptionContext properties:
 *   .Exception          — the unhandled System.Exception instance
 *   .ExceptionHandled   — set true to suppress rethrow; framework will NOT
 *                         propagate the exception to outer middleware
 *   .Result             — set an IActionResult to return to the client;
 *                         setting .Result also implies ExceptionHandled = true
 *   .HttpContext         — full request/response context
 *
 * SYNC VARIANT — IExceptionFilter:
 *   void OnException(ExceptionContext context)
 *   Use the sync variant when no async I/O is needed in the handler.
 *
 * MULTIPLE EXCEPTION FILTERS:
 *   When several filters are registered (global + controller + action),
 *   the first filter that sets context.ExceptionHandled = true wins.
 *   Order (IOrderedFilter) controls which filter runs first.
 *   Order = int.MaxValue here means "run last" — catch what others missed.
 *
 * EXCEPTION FILTER vs MIDDLEWARE (see Program.cs Section 7 PREVIEW):
 *   Use IExceptionFilter for MVC-specific structured responses (ProblemDetails).
 *   Use exception-handling middleware for a catch-all that covers the whole pipeline.
 *   COVERED IN DETAIL → 10. Exception Handling
 */
public sealed class GlobalExceptionFilter : IAsyncExceptionFilter, IOrderedFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public int Order => int.MaxValue; // run last — catch what other exception filters missed

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger; // ILogger<T> is always registered by ASP.NET Core host
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        Exception ex = context.Exception;

        _logger.LogError(ex, "Unhandled exception caught by GlobalExceptionFilter: {Message}", ex.Message);

        // Map well-known exception types to HTTP status codes
        int statusCode = ex switch
        {
            ArgumentNullException      => (int)HttpStatusCode.BadRequest,           // 400
            ArgumentException          => (int)HttpStatusCode.BadRequest,           // 400
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,        // 401
            KeyNotFoundException       => (int)HttpStatusCode.NotFound,             // 404
            NotImplementedException    => (int)HttpStatusCode.NotImplemented,       // 501
            _                          => (int)HttpStatusCode.InternalServerError,  // 500
        };

        // RFC 7807 ProblemDetails — standardized JSON error response body
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title  = ReasonPhrase(statusCode), // e.g. "Not Found"
            Detail = ex.Message,               // exception message as the detail field
        })
        {
            StatusCode = statusCode, // also sets HTTP response status code
        };

        context.ExceptionHandled = true; // suppress rethrow to outer middleware

        return Task.CompletedTask; // synchronous work; return completed task
    }

    private static string ReasonPhrase(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        404 => "Not Found",
        501 => "Not Implemented",
        _   => "Internal Server Error",
    };
}
