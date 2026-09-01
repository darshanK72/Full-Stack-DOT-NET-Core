/*
 * TOPIC: Minimal APIs
 *
 * WHY IT MATTERS:
 *   Minimal APIs (introduced in .NET 6, fully featured in .NET 7/8) let you define
 *   HTTP endpoints directly on WebApplication with a handful of method calls — no
 *   controllers, no attribute routing classes, no action filters on derived types.
 *   The result is less ceremony for simple CRUD services, microservices, and APIs that
 *   don't need the full MVC pipeline.  .NET 8 added native OpenAPI document generation
 *   (no Swashbuckle required), typed endpoint filters, and route groups with prefixes
 *   and tags — making minimal APIs viable for production-grade applications.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Minimal APIs overview and comparison with controller-based APIs
 *   2.  RequestDelegate vs typed handlers
 *   3.  DI registration, EndpointsApiExplorer setup, and the app pipeline
 *   4.  Product and request/response DTOs with DataAnnotations
 *   5.  Order and request DTOs
 *   6.  IProductService interface and in-memory implementation
 *   7.  IEndpointFilter and reusable ValidationEndpointFilter
 *   8.  Extension methods on IEndpointRouteBuilder + route groups (MapGroup)
 *   9.  Full CRUD with MapGet/Post/Put/Delete and all parameter binding sources
 *  10.  TypedResults vs Results — type-safe HTTP responses
 *  11.  OpenAPI metadata — WithName, WithSummary, WithDescription, Produces, WithOpenApi
 *  12.  Async handlers, [FromHeader], and explicit [FromServices] injection
 *  13.  Auth preview — RequireAuthorization and AllowAnonymous
 *
 * CHAPTER MAP:
 *   1.  Minimal APIs overview + controller comparison  → Program.cs  (SECTION 1)
 *   2.  RequestDelegate vs typed handlers              → Program.cs  (SECTION 2)
 *   3.  DI registration + EndpointsApiExplorer + app pipeline → Program.cs (SECTION 3)
 *   4.  Product entity and request DTOs                → Models/Product.cs
 *   5.  Order entity and request DTOs                  → Models/Order.cs
 *   6.  IProductService + in-memory implementation     → Services/ProductService.cs
 *   7.  IEndpointFilter + ValidationEndpointFilter     → Filters/ValidationEndpointFilter.cs
 *   8.  Extension methods + route groups               → Endpoints/ProductEndpoints.cs
 *   9.  Full CRUD — MapGet/Post/Put/Delete + binding   → Endpoints/ProductEndpoints.cs
 *  10.  TypedResults vs Results                        → Endpoints/ProductEndpoints.cs
 *  11.  OpenAPI metadata                               → Endpoints/ProductEndpoints.cs
 *  12.  Async handlers + [FromHeader] / [FromServices] → Endpoints/OrderEndpoints.cs
 *  13.  Auth preview — RequireAuthorization            → Endpoints/AuthEndpoints.cs
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalApis.Endpoints;
using MinimalApis.Services;

/*
 * SECTION 1: MINIMAL APIS OVERVIEW — WHAT THEY ARE AND WHEN TO USE THEM
 *
 * Minimal APIs map HTTP verbs directly onto lambda handlers or named methods.
 * The entry point is the WebApplicationBuilder / WebApplication pair introduced
 * in .NET 6 (the "minimal hosting model").
 *
 *   Controller-based APIs (MVC)            Minimal APIs
 *   ─────────────────────────────────────  ──────────────────────────────────────
 *   Inherit ControllerBase                 No base class — plain methods or lambdas
 *   [ApiController], [Route] attributes    Route defined inline at registration
 *   Action filters, model binding via Mvc  Endpoint filters, lightweight binder
 *   Built-in validation with Mvc pipeline  Manual or filter-based validation
 *   Better for large APIs with shared      Better for microservices, small APIs,
 *   filters, versioning, and conventions   gRPC-like simplicity, function-as-service
 *
 * Rule of thumb:
 *   Use Minimal APIs when the endpoint count is small, cross-cutting concerns are
 *   handled via endpoint filters, and you want a thin entry point.
 *   Use controllers when the API is large, team-shared conventions matter, or you
 *   need ApiExplorer / versioning features that rely on the MVC pipeline.
 *
 * NOTE: Both styles can coexist in one project — MapControllers() + MapGet() etc.
 */

/*
 * SECTION 2: REQUESTDELEGATE VS TYPED HANDLERS
 *
 * A RequestDelegate is the raw ASP.NET Core delegate:
 *   delegate Task RequestDelegate(HttpContext context)
 * You can pass one directly to app.MapGet/Post/etc., but you must manually read
 * query strings, route values, and write the response.
 *
 * A typed handler is any compatible delegate or method whose parameters the
 * framework can bind automatically from the request (route, query, body, DI, etc.).
 * This is the standard Minimal APIs pattern.
 *
 *   RequestDelegate (manual):              Typed handler (auto-bound):
 *   ─────────────────────────────────────  ──────────────────────────────────────────
 *   app.MapGet("/raw", async ctx => {      app.MapGet("/typed", (string? name) =>
 *       var name = ctx.Request             Results.Ok($"Hello, {name ?? "World"}!"));
 *                    .Query["name"];
 *       await ctx.Response
 *                .WriteAsync($"Hello!");
 *   });
 *
 * The inline demos below compare both approaches at the /demo/* paths.
 */

// --- Inline demo: RequestDelegate (manual parameter extraction) ---
// This lambda matches RequestDelegate — HttpContext is passed explicitly.
// Not recommended for real endpoints; shown here for comparison only.

// --- Inline demo: typed handler (auto-bound parameters) ---
// Parameters are resolved from route, query string, DI, etc. automatically.
// This is the standard minimal API pattern used throughout this chapter.

/*
 * SECTION 3: WEBAPPLICATION SETUP — DI REGISTRATION AND APP PIPELINE
 *
 * WebApplicationBuilder.Services is the DI container.
 * Services registered here are available via implicit injection in handler parameters.
 *
 * DI lifetime choices:
 *   AddSingleton  — one instance for the app's lifetime          (in-memory stores, caches)
 *   AddScoped     — one instance per HTTP request                (DB contexts, unit-of-work)
 *   AddTransient  — new instance every time it is requested      (stateless helpers)
 *
 * OpenAPI metadata (net8.0 — Microsoft.AspNetCore.OpenApi 8.x):
 *   WithOpenApi() on individual endpoints enriches operation metadata for API explorers.
 *   For a full OpenAPI document in .NET 8, add Swashbuckle.AspNetCore and call:
 *     builder.Services.AddEndpointsApiExplorer();   builder.Services.AddSwaggerGen();
 *     app.UseSwagger();   app.UseSwaggerUI();
 *   In .NET 9+, AddOpenApi() / MapOpenApi() are built-in (no Swashbuckle needed).
 *
 * Middleware pipeline order matters:
 *   UseHttpsRedirection → UseAuthentication → UseAuthorization → MapXxx endpoints
 *   Endpoints must be LAST; auth middleware must precede any endpoint that calls
 *   RequireAuthorization().
 */

var builder = WebApplication.CreateBuilder(args);

// --- DI: register the product service as singleton (in-memory store) ---
builder.Services.AddSingleton<IProductService, ProductService>(); // resolves into handler params

// --- EndpointsApiExplorer: lets Swagger/Swashbuckle discover minimal API routes ---
// (AddSwaggerGen() + UseSwagger() + UseSwaggerUI() would complete the UI setup;
//  omitted here to keep the csproj packageless — WithOpenApi() still enriches metadata)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

/*
 * --- App pipeline + endpoint registration ---
 * Endpoint groups are defined in static extension methods (see Endpoints/ files).
 * Calling MapXxxEndpoints() here keeps Program.cs thin — all routing logic lives
 * in the Endpoints/ files where section comments explain it.
 *
 * COVERED IN DETAIL:
 *   MapProductEndpoints → Endpoints/ProductEndpoints.cs (sections 8–11)
 *   MapOrderEndpoints   → Endpoints/OrderEndpoints.cs   (section 12)
 *   MapAuthEndpoints    → Endpoints/AuthEndpoints.cs    (section 13, auth preview)
 *
 * Auth note: MapAuthEndpoints uses RequireAuthorization().  A real app must add
 * and configure authentication/authorization middleware before MapAuthEndpoints().
 * For this tutorial the auth endpoints are registered so the patterns compile and
 * are visible in the OpenAPI doc; they return 401 at runtime without a bearer token.
 */
app.UseHttpsRedirection();                 // redirect HTTP → HTTPS
app.MapProductEndpoints();                 // /products   — see ProductEndpoints.cs
app.MapOrderEndpoints();                   // /orders     — see OrderEndpoints.cs
app.MapAuthEndpoints();                    // /account    — see AuthEndpoints.cs (preview)

app.Run();

/*
 * QUICK REFERENCE — MINIMAL APIS
 * ─────────────────────────────────────────────────────────────────────────────
 * Route registration:
 *   app.MapGet("/path", handler)          HTTP GET
 *   app.MapPost("/path", handler)         HTTP POST
 *   app.MapPut("/path/{id}", handler)     HTTP PUT
 *   app.MapDelete("/path/{id}", handler)  HTTP DELETE
 *   app.MapMethods("/path", ["PATCH"], h) any verb(s)
 *
 * Parameter binding (auto, in priority order):
 *   {token}   → [FromRoute]   — matched segment from the route template
 *   ?key=val  → [FromQuery]   — query string (simple types or IEnumerable<T>)
 *   body JSON → [FromBody]    — complex type deserialized from JSON
 *   DI        → [FromServices]— registered service, resolved implicitly
 *   header    → [FromHeader]  — named request header (must be explicit attribute)
 *   IFormFile → [FromForm]    — multipart form upload
 *
 * Route groups:
 *   var g = app.MapGroup("/prefix").WithTags("Tag");
 *   g.MapGet("/", handler);              // route: GET /prefix/
 *   g.MapGet("/{id}", handler);          // route: GET /prefix/{id}
 *   Nested groups: g.MapGroup("/sub")    // route: /prefix/sub/...
 *
 * Results factory:
 *   Results.Ok(value)                    IResult — 200 + body
 *   Results.Created(uri, value)          IResult — 201 + Location header + body
 *   Results.NoContent()                  IResult — 204
 *   Results.NotFound()                   IResult — 404
 *   Results.BadRequest(error)            IResult — 400
 *   Results.ValidationProblem(errors)    IResult — 400 RFC 7807 problem details
 *   TypedResults.Ok(value)               Ok<T>  — compile-time type known to OpenAPI
 *   Results<Ok<T>, NotFound> return type — union, OpenAPI infers all response types
 *
 * OpenAPI metadata:
 *   .WithName("OperationId")             sets operationId in OpenAPI doc
 *   .WithSummary("short text")           sets summary
 *   .WithDescription("long text")        sets description
 *   .WithTags("Tag")                     groups endpoint in Swagger UI
 *   .Produces<T>(200)                    declares response type (use with Results)
 *   .WithOpenApi()                        enriches the operation via transformer (net8.0+)
 *
 * Endpoint filters:
 *   .AddEndpointFilter<TFilter>()        adds a filter that runs before/after handler
 *   .AddEndpointFilter((ctx, next) => …) inline filter as lambda
 *
 * Organizing endpoints:
 *   public static IEndpointRouteBuilder MapFooEndpoints(this IEndpointRouteBuilder r)
 *   { ... return r; }  — extension method keeps Program.cs thin
 * ─────────────────────────────────────────────────────────────────────────────
 */
