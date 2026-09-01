/*
 * FILE ROLE: Registers all product CRUD endpoints using a route group,
 *            demonstrating MapGet/Post/Put/Delete, all parameter binding sources,
 *            TypedResults vs Results, endpoint filters, and OpenAPI metadata.
 * SECTIONS IN THIS FILE:
 *   8.  Extension method on IEndpointRouteBuilder — organizing endpoints
 *   9.  Route groups — MapGroup + WithTags + WithOpenApi
 *  10.  MapGet list    — implicit [FromQuery] + TypedResults.Ok
 *  11.  MapGet by ID   — Results<Ok<T>, NotFound> + TypedResults
 *  12.  MapPost        — implicit [FromBody] + TypedResults.Created + filter
 *  13.  MapPut         — async update + Results.Ok / Results.NotFound
 *  14.  MapDelete      — async delete + Results.NoContent / NotFound
 *  15.  OpenAPI metadata — WithName, WithSummary, WithDescription, Produces
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MinimalApis.Filters;
using MinimalApis.Models;
using MinimalApis.Services;

namespace MinimalApis.Endpoints;

/*
 * SECTION 8: EXTENSION METHOD ON IEndpointRouteBuilder
 *
 * Organizing endpoints as static extension methods on IEndpointRouteBuilder
 * keeps Program.cs thin (one line per feature area) while giving each feature
 * its own file with full documentation.
 *
 *   public static IEndpointRouteBuilder MapFooEndpoints(this IEndpointRouteBuilder routes)
 *   {
 *       var group = routes.MapGroup("/foo").WithTags("Foo");
 *       group.MapGet("/", handler);
 *       return routes; // return the original builder so calls can be chained
 *   }
 *
 * Called in Program.cs:
 *   app.MapProductEndpoints();
 *
 * app itself implements IEndpointRouteBuilder, so the extension method works
 * directly on WebApplication.  Route groups (MapGroup) also implement it, so
 * you can nest: outerGroup.MapProductEndpoints() to nest under a prefix.
 */
public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        /*
         * SECTION 9: ROUTE GROUPS — MapGroup + WithTags
         *
         * MapGroup("/prefix") creates a RouteGroupBuilder.  Every endpoint
         * registered on the group automatically inherits:
         *   - the URL prefix (/products/...)
         *   - any metadata applied to the group (tags, auth requirements, filters)
         *
         * WithTags("Products") groups endpoints under the "Products" tag in
         * Swagger UI / the OpenAPI document — useful for organising large APIs.
         *
         * Nested groups:
         *   var sub = group.MapGroup("/special");  // route: /products/special/...
         *
         * Applying a filter to the group (alternative to per-endpoint):
         *   group.AddEndpointFilter<ValidationEndpointFilter>();
         * This would run the filter on ALL endpoints in the group, not just
         * the ones with a body.  Here we attach the filter per-endpoint instead
         * (on MapPost and MapPut) because GET / DELETE don't have a request body.
         */
        var group = routes.MapGroup("/products")
            .WithTags("Products");   // all endpoints in this group tagged "Products"

        // ── GET /products?category=Electronics ──────────────────────────────────

        /*
         * SECTION 10: MapGet LIST — [FromQuery] BINDING + TypedResults.Ok
         *
         * Parameter binding sources (auto-detected order of precedence):
         *   Route template token  → [FromRoute]   (highest priority)
         *   Registered DI service → [FromServices] (IProductService here)
         *   Simple type not in route → [FromQuery]  (string?, int?, etc.)
         *   Complex type (class)  → [FromBody]     (deserialized from JSON)
         *
         * `string? category` has no route token and is a simple type, so the
         * framework binds it from the query string: GET /products?category=Furniture
         * Writing [FromQuery] explicitly is optional here — it's shown for clarity.
         *
         * TypedResults.Ok vs Results.Ok:
         *   Results.Ok(value)      → returns IResult at compile time
         *   TypedResults.Ok(value) → returns Ok<IEnumerable<Product>> at compile time
         *
         * TypedResults is preferred for single-return-type endpoints because:
         *   1. OpenAPI can infer the response schema without a Produces<T>() call.
         *   2. The return type is part of the method signature, catchable at compile time.
         *   3. Unit tests can Assert on the concrete type (Ok<T>, Created<T>, etc.).
         */
        group.MapGet("/", (
            [FromQuery] string? category,    // [FromQuery] explicit — bound from ?category=
            IProductService service) =>      // [FromServices] implicit — resolved from DI
        {
            IEnumerable<Product> products = service.GetAll(category);
            return TypedResults.Ok(products); // Ok<IEnumerable<Product>> — type known at compile time
        })
        .WithName("GetProducts")                          // OpenAPI operationId
        .WithSummary("List products")
        .WithDescription("Returns all products.  Filter by category using ?category=Electronics.");

        // ── GET /products/{id} ──────────────────────────────────────────────────

        /*
         * SECTION 11: MapGet BY ID — Results<T1, T2> UNION TYPE + TypedResults
         *
         * When an endpoint can return two or more distinct HTTP response types,
         * declare the return type as Results<T1, T2> so OpenAPI knows ALL possible
         * responses without explicit Produces<T>() annotations.
         *
         *   Results<Ok<Product>, NotFound> — a discriminated union:
         *     Ok<Product>  → HTTP 200 with Product body
         *     NotFound     → HTTP 404 with no body
         *
         * The compiler enforces that only these two types are returned.
         * Both Ok<T> and NotFound are in the Microsoft.AspNetCore.Http.HttpResults
         * namespace (imported above).
         *
         * Route parameter binding:
         *   /{id:int} — the :int constraint ensures the route only matches when
         *   the segment is parseable as int; non-integer segments return 404.
         *   The parameter name `id` matches the {id} token — [FromRoute] is implicit.
         *
         * Pitfall: omitting the type constraint /{id} with a plain int parameter
         * returns 400 (BadRequest) when a non-integer segment is provided, not 404.
         * Always use :int (or :guid, :alpha, etc.) to get proper 404 behaviour.
         */
        group.MapGet("/{id:int}",
            Results<Ok<Product>, NotFound> (int id, IProductService service) =>
            {
                var product = service.GetById(id);
                if (product is null)
                    return TypedResults.NotFound();           // NotFound — HTTP 404
                return TypedResults.Ok(product);              // Ok<Product> — HTTP 200
            })
        .WithName("GetProductById")
        .WithSummary("Get product by ID");

        // ── POST /products ───────────────────────────────────────────────────────

        /*
         * SECTION 12: MapPost — [FromBody] BINDING + TypedResults.Created + FILTER
         *
         * Complex-type parameters (classes with properties) are bound from the
         * JSON request body automatically — [FromBody] is implicit.  Writing it
         * explicitly (as here) documents intent and prevents accidental query-string
         * binding if the class name conflicts with a registered service.
         *
         * TypedResults.Created(uri, value):
         *   Returns HTTP 201 Created with:
         *     Location: /products/5         (the URI of the new resource)
         *     Body:     { "id": 5, "name": "Widget", ... }
         *   Returns Created<Product> at compile time (OpenAPI infers schema).
         *
         * AddEndpointFilter<ValidationEndpointFilter>():
         *   Attaches the filter ONLY to this endpoint.
         *   The filter runs DataAnnotations validation on CreateProductRequest.
         *   If any [Required] / [Range] / [StringLength] fails → 400 ValidationProblem.
         *   If all pass → handler runs and the product is created.
         */
        group.MapPost("/", async (
            [FromBody] CreateProductRequest request, // explicit [FromBody] — JSON body
            IProductService service) =>
        {
            var product = await service.CreateAsync(request);         // async creation
            return TypedResults.Created($"/products/{product.Id}", product); // 201 + Location
        })
        .WithName("CreateProduct")
        .WithSummary("Create a product")
        .AddEndpointFilter<ValidationEndpointFilter>(); // validate body before handler runs

        // ── PUT /products/{id} ───────────────────────────────────────────────────

        /*
         * SECTION 13: MapPut — ASYNC UPDATE + Results.Ok / Results.NotFound
         *
         * Uses Results.Ok / Results.NotFound (not TypedResults) because the handler
         * returns two different response types.  Both share the IResult base type,
         * which is what the lambda infers.
         *
         * Alternatively, declare Results<Ok<Product>, NotFound> as the return type
         * and switch to TypedResults — the pattern is identical to section 11.
         *
         * async Task is required because UpdateAsync returns Task<Product?>.
         * Without await the compiler would warn CS4014 ("call not awaited").
         */
        group.MapPut("/{id:int}", async (
            int id,                            // [FromRoute] implicit — from {id:int}
            UpdateProductRequest request,      // [FromBody] implicit — JSON body
            IProductService service) =>
        {
            var updated = await service.UpdateAsync(id, request); // async — await the Task
            if (updated is null)
                return Results.NotFound();     // IResult — 404 if product not found
            return Results.Ok(updated);        // IResult — 200 with updated product body
        })
        .WithName("UpdateProduct")
        .WithSummary("Update a product")
        .Produces<Product>(200)               // explicit Produces because return type is IResult
        .Produces(404)
        .AddEndpointFilter<ValidationEndpointFilter>();

        // ── DELETE /products/{id} ────────────────────────────────────────────────

        /*
         * SECTION 14: MapDelete — ASYNC DELETE + Results.NoContent / Results.NotFound
         *
         * HTTP 204 No Content is the standard success response for DELETE — the
         * resource is gone so there is no body to return.
         *
         * Results.NoContent() — IResult, HTTP 204
         * Results.NotFound()  — IResult, HTTP 404 when the product does not exist
         *
         * TypedResults equivalents:
         *   TypedResults.NoContent()   → NoContent struct, HTTP 204
         *   TypedResults.NotFound()    → NotFound struct, HTTP 404
         * With both, declare Results<NoContent, NotFound> as return type.
         */
        group.MapDelete("/{id:int}", async (int id, IProductService service) =>
        {
            var deleted = await service.DeleteAsync(id); // async — returns Task<bool>
            return deleted
                ? Results.NoContent()  // 204 — resource deleted successfully
                : Results.NotFound();  // 404 — resource was not found
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product")
        .Produces(204)
        .Produces(404);

        /*
         * SECTION 15: OPENAPI METADATA — WithName / WithSummary / WithDescription /
         *             Produces / WithOpenApi
         *
         * Metadata methods annotate the RouteHandlerBuilder returned by MapGet/Post/etc.
         * They do NOT affect runtime routing — they only enrich the OpenAPI document.
         *
         *   .WithName("OperationId")     — operationId in the OpenAPI doc; must be unique
         *   .WithSummary("short text")   — one-line summary shown in Swagger UI
         *   .WithDescription("...")      — longer description (Markdown supported)
         *   .WithTags("Tag")             — tag(s) to group endpoints in Swagger UI
         *   .Produces<T>(200)            — response type declaration for IResult endpoints
         *                                  (TypedResults endpoints do NOT need Produces<T>)
         *   .Produces(404)               — status-only response (no body type)
         *   .WithOpenApi()               — applies the .NET 8 OpenAPI transformer, enabling
         *                                  richer customisation (parameter descriptions, etc.)
         *                                  Requires Microsoft.AspNetCore.OpenApi package.
         *
         * Produces<T>() vs TypedResults:
         *   TypedResults.Ok<Product>  → OpenAPI infers 200 + Product schema automatically
         *   Results.Ok(product)       → OpenAPI sees IResult; must add .Produces<Product>(200)
         *
         * The endpoints above already use WithName / WithSummary / Produces.
         * The search endpoint below shows a fuller OpenAPI example with WithOpenApi():
         */
        group.MapGet("/search", (
            [FromQuery] string? name,
            [FromQuery] decimal? maxPrice,
            IProductService service) =>
        {
            var all = service.GetAll();
            if (name is not null)
                all = all.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (maxPrice.HasValue)
                all = all.Where(p => p.Price <= maxPrice.Value);
            return TypedResults.Ok(all); // Ok<IEnumerable<Product>>
        })
        .WithName("SearchProducts")
        .WithSummary("Search products")
        .WithDescription("Filter products by partial name match and/or maximum price.")
        .WithOpenApi(); // .NET 8 OpenAPI transformer — richer schema generation

        return routes; // return the original builder — supports chaining in Program.cs
    }
}
