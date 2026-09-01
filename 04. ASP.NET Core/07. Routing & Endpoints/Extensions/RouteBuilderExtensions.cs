/*
 * FILE ROLE: Shows how to package related endpoints as an IEndpointRouteBuilder
 *            extension method — the recommended pattern for keeping Program.cs
 *            thin and making endpoint groups reusable and independently testable.
 * SECTIONS IN THIS FILE:
 *   5. IEndpointRouteBuilder extension method — health endpoints
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace RoutingEndpoints.Extensions;

/*
 * SECTION 5: IEndpointRouteBuilder EXTENSION METHOD
 *
 * The IEndpointRouteBuilder interface is implemented by both WebApplication and
 * RouteGroupBuilder, making it the correct parameter type for reusable endpoint
 * registration helpers.
 *
 * Why extend IEndpointRouteBuilder instead of calling app.MapGet directly in Program.cs?
 *
 *   1. Separation of concerns — each feature area owns its endpoint declarations.
 *   2. Reusability — the extension can be shared via a NuGet package or project reference.
 *   3. Testability — pass any IEndpointRouteBuilder mock or RouteGroupBuilder in tests.
 *   4. Readability — Program.cs becomes a clean app.MapXEndpoints() list.
 *
 * Method return type:
 *   Returning IEndpointRouteBuilder (not void) allows the caller to chain
 *   further conventions or group the result:
 *     app.MapHealthEndpoints().RequireAuthorization("InternalOnly");
 *
 * Endpoint metadata set in this extension:
 *   .WithName("HealthCheck")     — unique identifier; used by LinkGenerator.GetPathByName(...)
 *   .WithTags("Health")          — groups endpoint under one Swagger/OpenAPI heading
 *   .WithSummary("...")          — one-line description visible in Swagger UI
 *
 * Naming convention:
 *   Endpoint names must be GLOBALLY unique within the application.
 *   Convention: "FeatureAction" — e.g. "HealthCheck", "ReadyCheck", "GetOrderById".
 *   Duplicate names cause a runtime InvalidOperationException when LinkGenerator is called.
 */
public static class RouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        // GET /health — liveness probe; used by load balancers and Kubernetes to detect crashes
        routes.MapGet("/health", () =>
            Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }) // UtcNow: System.DateTime
        )
        .WithName("HealthCheck")                          // name used by LinkGenerator
        .WithTags("Health")                               // Swagger UI heading
        .WithSummary("Returns service liveness status");  // one-line OpenAPI description

        // GET /ready — readiness probe; separate from liveness (Kubernetes pattern)
        // Liveness: is the process alive?   Readiness: is it ready to accept traffic?
        routes.MapGet("/ready", () =>
            Results.Ok(new { Status = "Ready" })
        )
        .WithName("ReadyCheck")
        .WithTags("Health")
        .WithSummary("Returns service readiness status");

        return routes; // return routes so callers can continue chaining conventions
    }
}
