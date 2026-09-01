/*
 * FILE ROLE: Second MapGroup example — user endpoints — showing that multiple groups
 *            coexist independently and that mixing :int and :alpha constraints within
 *            one group is the standard way to resolve overlapping route patterns.
 * SECTIONS IN THIS FILE:
 *   4. MapGroup route group (users)
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace RoutingEndpoints.Endpoints;

/*
 * SECTION 4: MapGroup ROUTE GROUP — USER ENDPOINTS
 *
 * This second MapGroup example highlights several concepts beyond OrderEndpoints:
 *
 * Multiple groups coexist independently:
 *   app.MapOrderEndpoints() registers /api/orders/...
 *   app.MapUserEndpoints()  registers /api/users/...
 *   Each group has its own prefix, tags, and conventions — no interference.
 *
 * Constraint-based route disambiguation within one group:
 *   /{id:int}        — matches integer segments:    /api/users/42
 *   /{username:alpha} — matches alphabetic segments: /api/users/alice
 *   ASP.NET Core picks the most specific matching route; a constraint failure
 *   causes the router to skip that candidate and try the next one.
 *
 * Constraint interaction examples:
 *   GET /api/users/42      → /{id:int}        (42 satisfies :int)
 *   GET /api/users/alice   → /{username:alpha} (alice satisfies :alpha)
 *   GET /api/users/alice42 → 404              (fails :int AND :alpha — no match)
 *
 * Group-level metadata via fluent chaining:
 *   routes.MapGroup(prefix)
 *         .WithTags("Users")          → all member endpoints share the Swagger tag
 *         .RequireAuthorization()     → all members require authentication
 *         .WithOpenApi()              → all members included in OpenAPI spec output
 *   Conventions applied to the group propagate to every endpoint registered on it.
 *
 * Registration order within the group:
 *   Register more-specific routes first.  Here /{id:int} is registered before
 *   /{username:alpha} so integer segments are tested for int-compatibility first.
 *   In practice, ASP.NET Core's endpoint selector evaluates ALL candidates and
 *   picks the best match — order is less critical than with legacy IRouter routing —
 *   but registering in specificity order is good practice and aids readability.
 */
public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/api/users").WithTags("Users");

        // GET /api/users — list all users
        group.MapGet("/", () =>
            Results.Ok(new[] { new { Id = 1, Name = "Alice" }, new { Id = 2, Name = "Bob" } })
        ).WithName("GetAllUsers").WithSummary("Returns all users");

        // GET /api/users/1 — lookup by integer id; :int constraint wins over :alpha for digits
        group.MapGet("/{id:int}", (int id) =>
            Results.Ok(new { Id = id, Name = "Alice", Email = "alice@example.com" })
        ).WithName("GetUserById");

        // GET /api/users/alice — lookup by username; :alpha ensures only letters match here
        // Registered AFTER /{id:int} so integer segments are tested first (good convention).
        group.MapGet("/{username:alpha}", (string username) =>
            Results.Ok(new { Username = username, DisplayName = username })
        ).WithName("GetUserByUsername");

        return group;
    }
}
