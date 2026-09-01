/*
 * FILE ROLE: Demonstrates MapGroup — a static extension method on IEndpointRouteBuilder
 *            that registers a group of order-related endpoints sharing the /api/orders prefix.
 * SECTIONS IN THIS FILE:
 *   3. MapGroup endpoint extension (orders)
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace RoutingEndpoints.Endpoints;

/*
 * SECTION 3: MapGroup ENDPOINT EXTENSION — ORDER ENDPOINTS
 *
 * MapGroup(prefix) returns a RouteGroupBuilder — a sub-router scoped to a URL prefix.
 * Every Map* call on the group inherits that prefix automatically.
 *
 * Why encapsulate endpoints in a static extension method on IEndpointRouteBuilder?
 *   • Keeps Program.cs thin: one line per feature area (app.MapOrderEndpoints()).
 *   • Groups the feature's endpoints, metadata, and conventions in one place.
 *   • Mirrors the controller-per-resource pattern of MVC without requiring a class.
 *   • Accepts IEndpointRouteBuilder so it works with both WebApplication and
 *     nested RouteGroupBuilder (groups can be nested inside other groups).
 *
 * Route resolution inside a group:
 *   routes.MapGroup("/api/orders")   → shared prefix
 *   group.MapGet("/")                → GET  /api/orders/
 *   group.MapGet("/{id:int}")        → GET  /api/orders/42
 *   group.MapPost("/")               → POST /api/orders/
 *
 * Return type RouteGroupBuilder:
 *   Returning the group lets the caller chain additional conventions:
 *   app.MapOrderEndpoints().RequireAuthorization("AdminOnly");
 *
 * .WithTags("Orders"):
 *   Tags group endpoints under one heading in Swagger/OpenAPI UI.
 *   Applied to the group so ALL member endpoints inherit the tag.
 *
 * PREVIEW (covered in depth in 13. Minimal APIs):
 *   In the Minimal APIs chapter, handlers are extracted into typed static methods
 *   and the group wires full request/response patterns with TypedResults<T>.
 */
public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/api/orders").WithTags("Orders"); // tag shared by all members

        // GET /api/orders/ — list all orders
        group.MapGet("/", () =>
        {
            var orders = new[] { new { Id = 1, Total = 49.99m }, new { Id = 2, Total = 129.50m } };
            return Results.Ok(orders);
        }).WithName("GetAllOrders").WithSummary("Returns all orders");

        // GET /api/orders/1 — single order; :int prevents /api/orders/abc from matching
        group.MapGet("/{id:int}", (int id) =>
            Results.Ok(new { Id = id, Total = 99.99m, Status = "Pending" })
        ).WithName("GetOrderById").WithSummary("Returns one order by id");

        // POST /api/orders/ — create; returns 201 Created with a Location header
        group.MapPost("/", () =>
            Results.Created("/api/orders/3", new { Id = 3, Total = 0m, Status = "New" })
        ).WithName("CreateOrder");

        // PUT /api/orders/1 — full update; body binding covered in 08. Model Binding
        group.MapPut("/{id:int}", (int id) =>
        {
            bool valid = id > 0; // id from route; body binding shown in 08. Model Binding & Validation
            return valid ? Results.NoContent() : Results.NotFound();
        }).WithName("UpdateOrder");

        // DELETE /api/orders/1
        group.MapDelete("/{id:int}", (int id) =>
        {
            bool valid = id > 0;
            return valid ? Results.NoContent() : Results.NotFound();
        }).WithName("DeleteOrder");

        return group; // caller can chain .RequireAuthorization() or other group-level conventions
    }
}
