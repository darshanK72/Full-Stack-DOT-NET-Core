/*
 * FILE ROLE: ErrorResponse — a strongly-typed DTO alternative to the generic
 *            ProblemDetails class. Use when you want compile-time control over
 *            the JSON error shape returned to API clients.
 * SECTIONS IN THIS FILE:
 *   1. ErrorResponse record — typed error body
 *   2. FieldError record    — per-field validation failure DTO
 *   3. ProblemDetails vs ErrorResponse — decision guide
 */

using System;
using System.Collections.Generic;

namespace ExceptionHandling.Models;

/*
 * SECTION 1: ErrorResponse — TYPED ERROR BODY
 *
 * A plain C# record that mirrors the RFC 7807 ProblemDetails fields most APIs
 * expose, plus two additional fields (ErrorCode and CorrelationId) that teams
 * commonly add.
 *
 * Serialised by System.Text.Json with camelCase property names (ASP.NET Core
 * default). Typical JSON output:
 *
 *   {
 *     "status": 404,
 *     "errorCode": "NOT_FOUND",
 *     "message": "Product with id '99' was not found.",
 *     "correlationId": "0HN3EKL123456:00000001",
 *     "errors": []
 *   }
 *
 * Note: this class is NOT registered with AddProblemDetails() and is not
 * automatically used by ASP.NET Core. Handlers must explicitly create and
 * return it. See GlobalExceptionHandler.cs which uses ProblemDetails instead.
 */
public sealed record ErrorResponse
{
    // HTTP status code repeated in the body for clients that cannot read headers
    public int Status { get; init; }

    // Machine-readable error category — e.g. NOT_FOUND, VALIDATION_ERROR
    public string ErrorCode { get; init; } = string.Empty;

    // Human-readable explanation specific to this occurrence
    public string Message { get; init; } = string.Empty;

    // Links the response to server-side log entries — see SECTION 4 in GlobalExceptionHandler.cs
    public string CorrelationId { get; init; } = string.Empty;

    // Field-level failures; empty when error is not validation-related
    public IReadOnlyList<FieldError> Errors { get; init; } = Array.Empty<FieldError>();
}

/*
 * SECTION 2: FieldError — PER-FIELD VALIDATION FAILURE DTO
 *
 * Mirrors Exceptions/ValidationException.ValidationError in shape (Field + Message)
 * but lives in the Models namespace. This is intentional separation of concerns:
 *   • ValidationError (Exceptions/) is a domain type tied to exception throwing.
 *   • FieldError (Models/) is a serialisation DTO for the HTTP response body.
 *
 * In practice, the global handler maps ValidationError → FieldError when it
 * builds an ErrorResponse. When using ProblemDetails directly (as
 * GlobalExceptionHandler.cs does), you can skip the mapping and add the
 * ValidationError list straight into Extensions["errors"].
 */
public sealed record FieldError(string Field, string Message);

/*
 * SECTION 3: ProblemDetails vs ErrorResponse — DECISION GUIDE
 *
 * ProblemDetails  (Microsoft.AspNetCore.Mvc.ProblemDetails)
 * ──────────────────────────────────────────────────────────
 *  ✓ RFC 7807-compliant  — understood by standards-aware clients and OpenAPI tooling.
 *  ✓ Automatic integration with AddProblemDetails() / IProblemDetailsService.
 *  ✓ Used by ASP.NET Core model-validation failures (ValidationProblemDetails).
 *  ✓ Extensions are [JsonExtensionData] — serialised as flat top-level JSON fields.
 *  ✗ Extensions dictionary is loosely typed (IDictionary<string, object?>).
 *  ✗ Pulls Microsoft.AspNetCore.Mvc into shared/contract projects.
 *
 * ErrorResponse  (this type)
 * ──────────────────────────
 *  ✓ Strongly typed — property names enforced at compile time.
 *  ✓ No framework dependency — usable in shared class libraries.
 *  ✓ OpenAPI / Swagger reflects properties automatically.
 *  ✗ Not recognised by RFC 7807-aware clients (no "type"/"title" spec contract).
 *  ✗ Must be manually wired into every exception handler.
 *
 * Recommendation:
 *   Public / third-party APIs        → ProblemDetails (RFC 7807 compliance).
 *   Internal APIs (controlled both ends) → ErrorResponse (typed, no framework dep).
 *   This chapter uses ProblemDetails in GlobalExceptionHandler as the running demo.
 */
