/*
 * FILE ROLE: NotFoundException — the leaf exception type for missing resources.
 *            Thrown when a requested entity (by ID, slug, etc.) does not exist.
 * SECTIONS IN THIS FILE:
 *   1. NotFoundException — HTTP 404 leaf type with convenience constructors
 */

namespace ExceptionHandling.Exceptions;

/*
 * SECTION 1: NotFoundException — HTTP 404 LEAF TYPE
 *
 * Usage contract:
 *   Throw NotFoundException when a resource cannot be found in the data store.
 *   The global handler pattern-matches on this type and maps it to a 404
 *   ProblemDetails response — no string-matching or magic numbers elsewhere.
 *
 * Examples:
 *   throw new NotFoundException("Product", id);          // → "Product with id '42' was not found."
 *   throw new NotFoundException("Category slug 'widgets' does not exist.");
 *
 * Why a sealed leaf type instead of new AppException(message, 404)?
 *   • The type itself is the semantic signal — handlers use `is NotFoundException`
 *     without inspecting StatusCode (defence-in-depth).
 *   • Named type appears clearly in logs and exception reports.
 *   • Prevents callers from accidentally passing the wrong status code.
 */
public sealed class NotFoundException : AppException
{
    // Produces a canonical message: "Product with id '42' was not found."
    public NotFoundException(string entity, object key)
        : base($"{entity} with id '{key}' was not found.", statusCode: 404, errorCode: "NOT_FOUND") { }

    // Free-form message overload for cases where key-based messaging doesn't fit
    public NotFoundException(string message)
        : base(message, statusCode: 404, errorCode: "NOT_FOUND") { }
}
