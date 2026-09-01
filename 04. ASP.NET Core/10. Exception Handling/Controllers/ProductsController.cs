/*
 * FILE ROLE: ProductsController — demonstrates throwing custom exceptions,
 *            using IProblemDetailsService for non-throw error responses,
 *            and explains exception filters vs middleware.
 * SECTIONS IN THIS FILE:
 *   1. Throwing custom exceptions from controller actions
 *   2. Exception filter vs middleware — decision table
 *   3. IProblemDetailsService — writing ProblemDetails without throwing
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExceptionHandling.Controllers;

/*
 * SECTION 1: THROWING CUSTOM EXCEPTIONS FROM CONTROLLER ACTIONS
 *
 * The controller's responsibility is to map HTTP input to domain operations
 * and domain results to HTTP output. Error conditions belong to the domain
 * layer (services / repositories); the controller simply propagates them.
 *
 * Pattern:
 *   1. Call a service or repository method.
 *   2. If the resource is absent → throw NotFoundException.
 *   3. If domain rules fail → throw ValidationException with field errors.
 *   4. GlobalExceptionHandler (Handlers/GlobalExceptionHandler.cs) catches both
 *      and writes a structured ProblemDetails response automatically.
 *
 * The controller contains NO try/catch for domain exceptions — that logic lives
 * entirely in the global handler.
 *
 * Endpoints in this controller:
 *   GET  /api/products/{id}       → returns product or throws NotFoundException
 *   POST /api/products            → validates input; throws ValidationException on failure
 *   GET  /api/products/{id}/safe  → returns product or writes 404 via IProblemDetailsService
 */
[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    // Simulate a tiny in-memory data store — no real DB needed for this demo
    private static readonly Dictionary<int, string> _products = new Dictionary<int, string>
    {
        [1] = "Widget",
        [2] = "Gadget",
        [3] = "Doohickey"
    };

    public ProductsController(
        ILogger<ProductsController> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger               = logger;
        _problemDetailsService = problemDetailsService;
    }

    // GET api/products/1  → 200 { "id": 1, "name": "Widget" }
    // GET api/products/99 → throws NotFoundException → GlobalExceptionHandler → 404 ProblemDetails
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        if (!_products.TryGetValue(id, out string? name))
            throw new NotFoundException("Product", id); // no try/catch needed — handler does the work

        _logger.LogInformation("Retrieved product {ProductId}", id);
        return Ok(new { Id = id, Name = name });
    }

    // POST api/products  → 201 Created, or 422 ProblemDetails with field errors
    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequest request)
    {
        List<ValidationError> errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError("name", "Product name is required."));

        if (request.Price <= 0)
            errors.Add(new ValidationError("price", "Price must be greater than zero."));

        if (errors.Count > 0)
            throw new ValidationException(errors); // GlobalExceptionHandler → 422 ProblemDetails

        _logger.LogInformation("Creating product {Name} at price {Price}", request.Name, request.Price);

        // Simulate assigned ID; in a real app this comes from the data layer
        return CreatedAtAction(nameof(GetById), new { id = 42 }, new { Id = 42, Name = request.Name });
    }

    /*
     * SECTION 2: EXCEPTION FILTER vs MIDDLEWARE — WHEN TO USE WHICH
     *
     * ASP.NET Core offers two interception points for exceptions:
     *
     * ┌────────────────────┬──────────────────────────────┬───────────────────────────────────────┐
     * │ Feature            │ Exception Filter              │ Middleware / IExceptionHandler        │
     * ├────────────────────┼──────────────────────────────┼───────────────────────────────────────┤
     * │ Scope              │ MVC/API controller actions   │ Entire request pipeline                │
     * │ Catches routing    │ No (not yet in MVC pipeline) │ Yes                                    │
     * │   errors           │                              │                                        │
     * │ Catches middleware │ No                           │ Yes                                    │
     * │   exceptions       │                              │                                        │
     * │ Catches background │ No                           │ No (background tasks are separate)     │
     * │ Access to action   │ Yes — ActionContext,          │ No — HttpContext only                 │
     * │   context          │ RouteData, ModelState         │                                        │
     * │ Response output    │ Set context.Result            │ Write directly to HttpContext.Response │
     * │ Best suited for    │ Controller-specific logic;   │ App-wide fallback; all pipeline errors │
     * │                    │ model-level decoration        │                                        │
     * └────────────────────┴──────────────────────────────┴───────────────────────────────────────┘
     *
     * Execution order when both are registered:
     *   Exception filters fire first (inside the MVC action invocation pipeline).
     *   If they handle the exception (context.ExceptionHandled = true), middleware never sees it.
     *   If they re-throw or leave it unhandled, middleware catches the re-thrown exception.
     *
     * Rule of thumb:
     *   Use exception filters when you need access to action metadata (route data, ModelState,
     *   HTTP method) or want to return a specific IActionResult type.
     *   Use IExceptionHandler (middleware) as the global safety net for all other cases.
     *
     * Example exception filter (not registered in this demo — reference only):
     *
     *   public class DomainExceptionFilter : IExceptionFilter
     *   {
     *       public void OnException(ExceptionContext context)
     *       {
     *           if (context.Exception is AppException ex)
     *           {
     *               context.Result = new ObjectResult(new { ex.Message })
     *                   { StatusCode = ex.StatusCode };
     *               context.ExceptionHandled = true;
     *           }
     *       }
     *   }
     *   // Registration: builder.Services.AddControllers(o => o.Filters.Add<DomainExceptionFilter>());
     *   // Or per-controller: [TypeFilter(typeof(DomainExceptionFilter))]
     */

    /*
     * SECTION 3: IProblemDetailsService — WRITING ProblemDetails WITHOUT THROWING
     *
     * Sometimes you want a structured error response without throwing an exception —
     * for example in conditional logic paths or when performance matters in a hot path.
     *
     * IProblemDetailsService.TryWriteAsync:
     *   • Writes a ProblemDetails body and sets Content-Type: application/problem+json.
     *   • Respects any ProblemDetailsOptions customisation registered with AddProblemDetails().
     *   • Returns true if the response was written, false if the context prevented writing
     *     (e.g. response already started).
     *
     * Requires: builder.Services.AddProblemDetails() in Program.cs.
     *
     * After calling TryWriteAsync, the response is committed. Return EmptyResult from
     * the action method to tell ASP.NET Core not to write an additional response.
     *
     * Comparison:
     *   IProblemDetailsService.TryWriteAsync   → consistent with app's problem-details config
     *   httpContext.Response.WriteAsJsonAsync  → simpler but bypasses config hooks
     *   throw exception                        → cleanest; let GlobalExceptionHandler own it
     */

    // GET api/products/{id}/safe → returns 200 or writes 404 ProblemDetails without throwing
    [HttpGet("{id:int}/safe")]
    public async Task<IActionResult> GetByIdSafe(int id, CancellationToken cancellationToken)
    {
        if (!_products.TryGetValue(id, out string? name))
        {
            // Write 404 ProblemDetails directly — no exception thrown
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext    = HttpContext,
                ProblemDetails =
                {
                    Title  = "Not Found",
                    Detail = $"Product with id '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound
                }
            });
            return new EmptyResult(); // response already written — ASP.NET Core writes nothing more
        }

        return Ok(new { Id = id, Name = name });
    }
}

// Request DTO for POST api/products — Name is nullable to allow explicit validation feedback
public sealed record CreateProductRequest(string? Name, decimal Price);
