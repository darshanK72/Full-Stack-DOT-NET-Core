/*
 * FILE ROLE: ValidationException — thrown when business-rule or domain-level
 *            validation fails. Carries a list of per-field errors for structured
 *            API responses.
 * SECTIONS IN THIS FILE:
 *   1. ValidationError record — per-field error detail
 *   2. ValidationException   — HTTP 422 leaf exception
 */

using System.Collections.Generic;

namespace ExceptionHandling.Exceptions;

/*
 * SECTION 1: ValidationError — PER-FIELD ERROR DETAIL
 *
 * A small value type that pairs a field name with its failure message.
 * The global handler serialises a list of these into
 * ProblemDetails.Extensions["errors"] so API clients can display
 * field-level feedback without parsing a flat description string.
 *
 * Using a record:
 *   • Value equality — two ValidationError instances with the same field
 *     and message are equal (useful for unit tests and deduplication).
 *   • Immutable by default — suits an exception payload that should not
 *     be mutated after construction.
 */
public sealed record ValidationError(string Field, string Message);

/*
 * SECTION 2: ValidationException — HTTP 422 UNPROCESSABLE ENTITY
 *
 * HTTP 422 vs 400:
 * ─────────────────────────────────────────────────────────────────
 * 400 Bad Request          The request structure is wrong — malformed JSON,
 *                          missing required field (model binding catches this).
 * 422 Unprocessable Entity The request is well-formed but fails domain rules:
 *                          e.g. "end date must be after start date",
 *                               "insufficient stock for that quantity".
 *
 * Model binding (ASP.NET Core) returns 400 via ModelState automatically.
 * Use ValidationException for domain/business-rule violations the binder
 * cannot detect.
 *
 * Constructor overloads:
 *   ValidationException(IReadOnlyList<ValidationError>)  — multiple field errors
 *   ValidationException(string field, string message)    — single-field convenience
 *
 * The global handler (GlobalExceptionHandler.cs) checks for ValidationException
 * and adds the Errors list to ProblemDetails.Extensions["errors"].
 */
public sealed class ValidationException : AppException
{
    // Field-level error list — serialised into the ProblemDetails response body
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(IReadOnlyList<ValidationError> errors)
        : base("One or more validation errors occurred.", statusCode: 422, errorCode: "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    // Single-field convenience constructor — wraps into a one-element list
    public ValidationException(string field, string message)
        : this(new ValidationError[] { new ValidationError(field, message) }) { }
}
