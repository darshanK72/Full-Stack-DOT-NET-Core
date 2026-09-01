/*
 * FILE ROLE: Registers order endpoints in a second route group, demonstrating
 *            async handlers, [FromHeader] explicit binding, [FromServices] explicit
 *            injection, and Results.BadRequest for cross-entity validation.
 * SECTIONS IN THIS FILE:
 *  12a. Route group + static in-memory store
 *  12b. Async GET — awaiting a helper Task method
 *  12c. [FromHeader] + [FromServices] explicit parameter binding
 *  12d. Results.BadRequest for business-rule errors
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MinimalApis.Filters;
using MinimalApis.Models;
using MinimalApis.Services;

namespace MinimalApis.Endpoints;

/*
 * SECTION 12a: ORDER ROUTE GROUP + STATIC IN-MEMORY STORE
 *
 * This endpoint class uses a static list as the backing store to show that
 * not every endpoint group needs a registered DI service — sometimes a small,
 * self-contained endpoint file with its own state is the right choice for a
 * simple resource.
 *
 * Static fields share state across ALL requests (since the app is one process).
 * This is intentional here (simulating a singleton store) but means the data
 * is not thread-safe.  In production always use a proper DI-registered service
 * (see ProductService) or a concurrent collection.
 *
 * Extension method pattern: same as ProductEndpoints — keeps Program.cs thin.
 *   app.MapOrderEndpoints();  → calls this method
 */
public static class OrderEndpoints
{
    // Static seed data — visible across all requests (not thread-safe, tutorial only)
    private static readonly List<Order> _orders = new List<Order>
    {
        new Order { Id = 1, ProductId = 1, Quantity = 2, TotalPrice = 2599.98m,
                    CustomerEmail = "alice@example.com", Status = "Confirmed" },
        new Order { Id = 2, ProductId = 3, Quantity = 1, TotalPrice = 349.99m,
                    CustomerEmail = "bob@example.com",   Status = "Pending" },
    };
    private static int _nextOrderId = 3;

    // Helper returns Task<Order?> so handlers can await it, demonstrating async pattern
    private static Task<Order?> FindOrderAsync(int id) =>
        Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/orders")
            .WithTags("Orders");

        // ── GET /orders ──────────────────────────────────────────────────────────

        /*
         * SECTION 12b: ASYNC GET — AWAITING A Task METHOD
         *
         * A handler lambda can be async.  The framework accepts:
         *   Func<..., Task<IResult>>         async lambda returning IResult
         *   Func<..., ValueTask<IResult>>    same with ValueTask (lower allocation)
         *   Func<..., Task<T>>               async lambda returning a value directly
         *
         * When the return is not wrapped in IResult (e.g. returning Order? directly),
         * the framework serialises the value as JSON with HTTP 200, or returns
         * HTTP 204 No Content if the value is null.
         *
         * Here we await FindOrderAsync to show a real async code path.
         * The `await` keyword makes the lambda Func<int, Task<IResult>>, which the
         * framework handles natively — no special registration needed.
         */
        group.MapGet("/", () =>
        {
            // Synchronous list scan — no async needed; directly return IEnumerable
            return TypedResults.Ok(_orders); // Ok<List<Order>> — serialised as JSON array
        })
        .WithName("GetOrders")
        .WithSummary("List all orders");

        group.MapGet("/{id:int}", async (int id) =>
        {
            var order = await FindOrderAsync(id); // async — awaits a completed Task<Order?>
            return order is not null
                ? Results.Ok(order)      // IResult 200
                : Results.NotFound();    // IResult 404
        })
        .WithName("GetOrderById")
        .WithSummary("Get order by ID");

        // ── POST /orders ─────────────────────────────────────────────────────────

        /*
         * SECTION 12c: [FromHeader] AND [FromServices] EXPLICIT PARAMETER BINDING
         *
         * [FromHeader(Name = "X-Customer-Id")] string? customerId
         *   Reads the X-Customer-Id HTTP header.
         *   [FromHeader] MUST be explicit — headers are never bound implicitly.
         *   The Name property lets you map a C# parameter to a header with a
         *   different casing or hyphenated name.
         *   The type must be string (or string?) — complex types are not supported
         *   for header binding.
         *
         * [FromServices] IProductService productService
         *   Injects the service explicitly.  Normally IProductService would be
         *   resolved implicitly (no attribute needed), but [FromServices] makes
         *   the intent clear and documents that this parameter comes from DI.
         *   Useful when the parameter type could be ambiguous (e.g., also a
         *   registered IEnumerable<T> that might conflict with [FromQuery]).
         *
         * [FromBody] CreateOrderRequest request
         *   Explicitly binds from the JSON body.  Same as implicit binding for
         *   complex types, but shown here to contrast with [FromHeader] and
         *   [FromServices] in the same parameter list.
         *
         * Summary of all binding sources in one handler signature:
         *   int id              → [FromRoute]   implicit (matches route token)
         *   string? category    → [FromQuery]   implicit (simple type, no route token)
         *   CreateOrderRequest  → [FromBody]    implicit (complex type) or explicit
         *   IProductService     → [FromServices] implicit (DI-registered type)
         *   string? headerId    → [FromHeader]  MUST be explicit (never implicit)
         *
         * SECTION 12d: Results.BadRequest — BUSINESS-RULE ERRORS
         *
         * HTTP 400 Bad Request covers both:
         *   a) Invalid input (handled by ValidationEndpointFilter returning ValidationProblem)
         *   b) Business-rule violations caught in the handler (shown here)
         *
         * Results.BadRequest(object? error) returns 400 with a plain message string.
         * Results.ValidationProblem(errors) returns 400 with RFC 7807 structured body.
         *
         * Use ValidationProblem for field-level validation feedback to the UI.
         * Use BadRequest(message) for application-level business rules.
         */
        group.MapPost("/", async (
            [FromBody] CreateOrderRequest request,                     // JSON body
            [FromHeader(Name = "X-Customer-Id")] string? customerId,  // request header
            [FromServices] IProductService productService) =>          // explicit DI
        {
            // Business rule: verify the referenced product exists
            var product = productService.GetById(request.ProductId);
            if (product is null)
                return Results.BadRequest($"Product {request.ProductId} does not exist."); // 400

            // Business rule: sufficient stock
            if (product.StockQuantity < request.Quantity)
                return Results.BadRequest(
                    $"Insufficient stock: {product.StockQuantity} available, " +
                    $"{request.Quantity} requested."); // 400

            var order = new Order
            {
                Id            = _nextOrderId++,
                ProductId     = request.ProductId,
                Quantity      = request.Quantity,
                TotalPrice    = product.Price * request.Quantity,
                CustomerEmail = request.CustomerEmail,
                CreatedAt     = DateTime.UtcNow,
                Status        = "Pending",
            };
            _orders.Add(order);

            // X-Customer-Id header value — used to auto-confirm orders from known customers.
            // [FromHeader] binds it; using it here avoids a dead-code pattern in the demo.
            order.Status = customerId is not null ? "Confirmed" : "Pending";

            return await Task.FromResult(
                Results.Created($"/orders/{order.Id}", order)); // 201 + Location header
        })
        .WithName("CreateOrder")
        .WithSummary("Place an order")
        .WithDescription(
            "Creates a new order.  Optionally supply X-Customer-Id header for tracking. " +
            "Returns 400 if the product does not exist or has insufficient stock.")
        .AddEndpointFilter<ValidationEndpointFilter>(); // validate CreateOrderRequest

        // ── PATCH /orders/{id}/status ────────────────────────────────────────────

        group.MapMethods("/{id:int}/status", new[] { "PATCH" }, async (
            int id,
            [FromBody] string newStatus) =>
        {
            var order = await FindOrderAsync(id);
            if (order is null)
                return Results.NotFound();

            order.Status = newStatus;
            return Results.Ok(order); // 200 with updated order
        })
        .WithName("UpdateOrderStatus")
        .WithSummary("Update order status (PATCH)")
        .WithDescription("Shows MapMethods — register any HTTP verb including PATCH.");

        return routes;
    }
}
