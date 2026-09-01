/*
 * FILE ROLE: Defines the base custom exception type for the application —
 *            every domain-layer exception inherits from AppException so the
 *            global handler can identify and map them to HTTP status codes.
 * SECTIONS IN THIS FILE:
 *   1. AppException — base class with HTTP status code and error code fields
 */

using System;

namespace ExceptionHandling.Exceptions;

/*
 * SECTION 1: CUSTOM EXCEPTION HIERARCHY — BASE CLASS
 *
 * Why define custom exceptions?
 * ─────────────────────────────
 * The global handler (Handlers/GlobalExceptionHandler.cs) catches EVERY
 * unhandled Exception. To decide whether to return 400, 404, 422, or 500 it
 * needs to inspect the exception type. Embedding the HTTP status code inside
 * the exception itself removes all conditionals from the handler and keeps
 * the "what HTTP code does this mean?" decision at the throw site.
 *
 * Hierarchy used in this chapter:
 *   System.Exception
 *     └── AppException              ← base (this file) — any domain error
 *           ├── NotFoundException   ← 404  (Exceptions/NotFoundException.cs)
 *           └── ValidationException ← 422  (Exceptions/ValidationException.cs)
 *
 * Design rules:
 *   • Derive from Exception directly (not ApplicationException — discouraged by MS).
 *   • Carry an HttpStatusCode (as int) for the global handler.
 *   • Carry an ErrorCode string for machine-readable categorisation in API clients.
 *   • Keep the hierarchy one level deep — avoid deep chains.
 *   • Provide a wrapping constructor (with inner) so callers can preserve context.
 *
 * When NOT to use custom exceptions:
 *   • Exceptional infrastructure failures (DB down, OOM) — let them propagate as-is;
 *     the global handler returns 500 for anything it doesn't recognise.
 *   • Validation handled by ASP.NET Core model binding — that already returns 400
 *     via ModelState; ValidationException here is for domain/business-rule violations.
 */
public class AppException : Exception
{
    // HTTP status code this exception maps to (e.g. 404, 422, 409)
    public int StatusCode { get; }

    // Machine-readable error category; API clients can switch on this string
    public string ErrorCode { get; }

    public AppException(string message, int statusCode = 500, string errorCode = "INTERNAL_ERROR")
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode  = errorCode;
    }

    // Wrapping constructor — preserves the inner exception for structured logging
    public AppException(string message, Exception inner, int statusCode = 500, string errorCode = "INTERNAL_ERROR")
        : base(message, inner)
    {
        StatusCode = statusCode;
        ErrorCode  = errorCode;
    }
}
