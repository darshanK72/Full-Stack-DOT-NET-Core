/*
 * FILE ROLE: Demonstrates RequireAuthorization and AllowAnonymous on Minimal API
 *            endpoints.  This is a PREVIEW — authentication setup is covered in
 *            depth in a dedicated Auth & JWT chapter.
 * SECTIONS IN THIS FILE:
 *  13a. RequireAuthorization — protecting endpoints
 *  13b. AllowAnonymous — overriding group-level authorization
 *  13c. Comparison: Minimal APIs vs controllers for auth
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MinimalApis.Endpoints;

/*
 * SECTION 13a: RequireAuthorization — PROTECTING ENDPOINTS
 *
 * RequireAuthorization() is an extension method on IEndpointConventionBuilder
 * (from Microsoft.AspNetCore.Authorization) that marks the endpoint as requiring
 * an authenticated and authorized user.
 *
 * Behind the scenes it adds an AuthorizeAttribute to the endpoint's metadata.
 * The authorization middleware (app.UseAuthorization()) reads this metadata and
 * returns HTTP 401 Unauthorized if the request has no valid identity, or HTTP 403
 * Forbidden if the user is authenticated but lacks the required policy/role.
 *
 * To use RequireAuthorization at runtime, Program.cs must register:
 *   builder.Services.AddAuthentication(...)   // configure scheme (JWT, Cookie, etc.)
 *   builder.Services.AddAuthorization()       // register authorization services
 *   app.UseAuthentication();                  // in middleware pipeline BEFORE UseAuthorization
 *   app.UseAuthorization();                   // resolves identity and checks policies
 *
 * In this tutorial those registrations are omitted to keep Program.cs focused on
 * Minimal API routing patterns.  The endpoints compile and appear in the OpenAPI doc
 * but return 401 at runtime without a bearer token.
 *
 * Overloads:
 *   RequireAuthorization()                    — any authenticated user
 *   RequireAuthorization("PolicyName")         — user must satisfy named policy
 *   RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
 *
 * Applying to a group (all endpoints in the group require auth):
 *   var secured = app.MapGroup("/admin").RequireAuthorization();
 *
 * COVERED IN DETAIL LATER → Authentication & JWT chapter (when created).
 *
 * SECTION 13b: AllowAnonymous — OVERRIDING GROUP-LEVEL AUTHORIZATION
 *
 * When a group has RequireAuthorization(), individual endpoints can opt out
 * with AllowAnonymous() — useful for a "login" or "register" endpoint that
 * lives under the same URL prefix as protected endpoints.
 *
 *   var group = app.MapGroup("/account").RequireAuthorization();
 *   group.MapPost("/login", handler).AllowAnonymous(); // publicly accessible
 *   group.MapGet("/profile", handler);                 // still requires auth
 *
 * SECTION 13c: MINIMAL APIS vs CONTROLLERS FOR AUTH
 *
 *   Controller-based                       Minimal APIs
 *   ─────────────────────────────────────  ──────────────────────────────────────
 *   [Authorize] on class or action         .RequireAuthorization() on endpoint/group
 *   [AllowAnonymous] on action             .AllowAnonymous() on endpoint
 *   [Authorize(Policy = "...")]            .RequireAuthorization("PolicyName")
 *   [Authorize(Roles = "Admin")]           .RequireAuthorization(new AuthorizeAttribute
 *                                              { Roles = "Admin" })
 *
 * The runtime behaviour is identical — both rely on the same authorization
 * middleware reading IAuthorizeData metadata on the endpoint.
 */
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        // Group-level authorization: every endpoint in this group requires auth by default
        var group = routes.MapGroup("/account")
            .WithTags("Account")
            .RequireAuthorization(); // any authenticated user

        // ── GET /account/profile — requires authentication ────────────────────────

        group.MapGet("/profile", (HttpContext context) =>
        {
            // HttpContext.User is the ClaimsPrincipal populated by authentication middleware
            var name  = context.User.Identity?.Name ?? "Unknown";
            var email = context.User.FindFirst("email")?.Value ?? "no email claim";
            return Results.Ok(new { name, email }); // 200 with anonymous object — serialised to JSON
        })
        .WithName("GetProfile")
        .WithSummary("Get authenticated user profile")
        .Produces(200)
        .Produces(401);

        // ── POST /account/logout — requires authentication ────────────────────────

        group.MapPost("/logout", (HttpContext context) =>
        {
            // In a cookie-auth app you'd call SignOutAsync; for JWT, instruct the
            // client to discard the token (no server-side invalidation for stateless JWT).
            return Results.Ok(new { message = "Logged out successfully." });
        })
        .WithName("Logout")
        .WithSummary("Logout current user");

        // ── POST /account/login — publicly accessible (AllowAnonymous overrides group) ──

        group.MapPost("/login", ([FromBody] LoginRequest request) =>
        {
            // PREVIEW: real login would validate credentials and issue a JWT.
            // Returns a placeholder token for tutorial purposes.
            if (request.Email == "admin@example.com" && request.Password == "password")
                return Results.Ok(new { token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." });

            return Results.Unauthorized(); // 401 — wrong credentials
        })
        .AllowAnonymous()                  // override group-level RequireAuthorization
        .WithName("Login")
        .WithSummary("Login (AllowAnonymous — overrides group auth)")
        .WithDescription(
            "This endpoint is in the /account group (RequireAuthorization) but is " +
            "marked AllowAnonymous so unauthenticated callers can reach it.");

        return routes;
    }
}

/*
 * Minimal DTO for the login endpoint — kept in this file because it is used
 * only by AuthEndpoints and does not warrant its own Models/ file.
 */
internal sealed class LoginRequest
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// [FromBody] above is Microsoft.AspNetCore.Mvc.FromBodyAttribute (imported via
// using Microsoft.AspNetCore.Mvc at the top).  The Minimal APIs binder recognises
// it without requiring AddControllers() — the attribute is just metadata.
