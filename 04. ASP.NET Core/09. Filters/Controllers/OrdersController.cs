/*
 * FILE ROLE: Demonstrates filter attributes applied at controller and action
 *            scope — ServiceFilter, TypeFilter, and IFilterFactory — showing
 *            how filters compose and how scope + ordering governs execution order.
 * SECTIONS IN THIS FILE:
 *   8.  Controller-level filter scope — [ServiceFilter] and [TypeFilter]
 *   9.  Action-level filter scope — per-action TypeFilter, IFilterFactory usage
 *   10. IFilterFactory — attribute-based filter factory (RequireRoleAttribute)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Filters.Filters;

namespace Filters.Controllers;

/*
 * SECTION 8: CONTROLLER-LEVEL FILTER SCOPE
 * ─────────────────────────────────────────────────────────────────────────────
 * Filters on a controller class run for EVERY action in that controller,
 * at the "controller" scope — after global filters but before action-level ones.
 *
 * [ServiceFilter(typeof(ApiKeyAuthorizationFilter))]
 *   Resolves ApiKeyAuthorizationFilter from the DI container.
 *   The filter must be registered in Program.cs:
 *     builder.Services.AddScoped<ApiKeyAuthorizationFilter>();
 *   Honors the DI lifetime (Scoped here — one instance per HTTP request).
 *
 * [TypeFilter(typeof(ValidateModelFilter))]
 *   Creates ValidateModelFilter via ActivatorUtilities on each request.
 *   ValidateModelFilter does NOT need to be registered in DI.
 *   TypeFilter resolves constructor arguments from DI; extra params via Arguments:
 *     [TypeFilter(typeof(SomeFilter), Arguments = new object[] { "param1" })]
 *
 * Execution order (Before phase — outermost to innermost):
 *   Global:     RequestLoggingActionFilter  (Order -1000)
 *               GlobalExceptionFilter       (Order int.MaxValue — exception stage)
 *   Controller: ApiKeyAuthorizationFilter   (Authorization stage — always first)
 *               ValidateModelFilter         (Action stage — after auth)
 *   Action:     ResponseTimingResultFilter  (Result stage — innermost)
 *   → action method executes
 *
 * NOTE: Stage ordering (Auth → Resource → Action → Exception → Result) is
 * always preserved regardless of attribute order or scope.  Attributes on this
 * class are applied in declaration order only within the same stage.
 */
[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(ApiKeyAuthorizationFilter))]  // controller scope, DI-resolved
[TypeFilter(typeof(ValidateModelFilter))]            // controller scope, ActivatorUtilities
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ILogger<OrdersController> logger)
    {
        _logger = logger;
    }

    /*
     * SECTION 9: ACTION-LEVEL FILTER SCOPE
     * ─────────────────────────────────────────────────────────────────────────────
     * Filters on an action method run for that action only, at the innermost scope.
     *
     * [TypeFilter(typeof(ResponseTimingResultFilter))]
     *   Applies a result filter to this action only.
     *   ResponseTimingResultFilter is defined in Filters/RequestLoggingActionFilter.cs.
     *   TypeFilter creates it via ActivatorUtilities; ILogger<T> is resolved from DI.
     *
     * [RequireRole("Admin")]
     *   An IFilterFactory attribute (defined in Section 10 below).
     *   The attribute reads its "Admin" argument and forwards it to RoleCheckFilter.
     *   Clean usage: [RequireRole("Admin")] instead of
     *                [TypeFilter(typeof(RoleCheckFilter), Arguments = new[] { "Admin" })]
     *
     * Short-circuit from the action itself (not a filter short-circuit):
     *   if (id <= 0) return BadRequest(...)
     *   The action returns a result directly — this is normal action code, not
     *   the same as a filter setting context.Result in OnActionExecuting.
     */
    [HttpGet("{id:int}")]
    [TypeFilter(typeof(ResponseTimingResultFilter))]  // result filter at action scope
    public IActionResult GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new { error = "id must be a positive integer" }); // normal return

        _logger.LogInformation("Fetching order {Id}", id);
        return Ok(new { OrderId = id, Item = "Widget", Quantity = 3 }); // 200 JSON response
    }

    /*
     * --- 9a. Action that triggers GlobalExceptionFilter ---
     *
     * Throwing an unhandled exception from an action propagates to the
     * Exception filter stage.  GlobalExceptionFilter (registered globally
     * in Program.cs) catches it and returns a structured ProblemDetails response.
     *
     * Without GlobalExceptionFilter this request would return a 500 with the
     * default ASP.NET Core developer exception page or an empty 500 body.
     */
    [HttpGet("fail")]
    public IActionResult GetFail()
    {
        throw new KeyNotFoundException("Order 99 does not exist"); // caught by GlobalExceptionFilter
    }

    /*
     * --- 9b. POST action — ValidateModelFilter fires OnActionExecuting ---
     *
     * If the request body is missing the required "Item" field, model binding
     * sets ModelState.IsValid = false.  ValidateModelFilter.OnActionExecuting
     * sets context.Result to 422 Unprocessable Entity.  This method body never
     * runs in that case — ValidateModelFilter short-circuited the pipeline.
     */
    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("Creating order for {Item}", request.Item);
        return CreatedAtAction(
            nameof(GetById),
            new { id = 42 },
            new { OrderId = 42, request.Item, request.Quantity }); // 201 Created
    }

    /*
     * --- 9c. DELETE with [RequireRole] — demonstrates IFilterFactory ---
     *
     * The RequireRoleAttribute declared below implements IFilterFactory.
     * Applying [RequireRole("Admin")] is equivalent to a TypeFilter that
     * constructs a RoleCheckFilter with _role = "Admin" injected from
     * the attribute constructor — but with a clean, readable attribute syntax.
     */
    [HttpDelete("{id:int}")]
    [RequireRole("Admin")]  // IFilterFactory attribute — see Section 10
    public IActionResult DeleteOrder(int id)
    {
        _logger.LogInformation("Deleting order {Id}", id);
        return NoContent(); // 204
    }
}

/*
 * SECTION 10: IFilterFactory — ATTRIBUTE-BASED FILTER FACTORY
 * ─────────────────────────────────────────────────────────────────────────────
 * IFilterFactory bridges Attribute and IFilterMetadata:
 *   • The attribute class itself implements IFilterFactory.
 *   • MVC calls CreateInstance(IServiceProvider) once per request.
 *   • The factory reads attribute constructor parameters and injects DI services
 *     to create the real filter instance.
 *
 * This solves a limitation of TypeFilter:
 *   TypeFilter.Arguments accepts extra params but the usage is verbose:
 *     [TypeFilter(typeof(RoleCheckFilter), Arguments = new object[] { "Admin" })]
 *
 *   IFilterFactory gives a clean custom attribute instead:
 *     [RequireRole("Admin")]
 *
 * IFilterFactory members:
 *   bool IsReusable
 *     true  → MVC may cache and reuse the filter instance (Singleton-like).
 *     false → new instance per request (Scoped-like); required for stateful filters.
 *
 *   IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
 *     Called by MVC to create the filter.  Use serviceProvider to resolve DI deps.
 *
 * The attribute class must NOT itself be the filter — keep the attribute and the
 * filter class separate so each has a single responsibility.
 */
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireRoleAttribute : Attribute, IFilterFactory
{
    private readonly string _role;

    public RequireRoleAttribute(string role)
    {
        _role = role; // stored on the attribute; forwarded to filter ctor below
    }

    public bool IsReusable => false; // new RoleCheckFilter instance per request

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        // Resolve DI dependencies and pass attribute param to the filter
        ILogger<RoleCheckFilter> logger =
            serviceProvider.GetRequiredService<ILogger<RoleCheckFilter>>(); // DI-resolved
        return new RoleCheckFilter(_role, logger); // attribute param forwarded here
    }
}

/*
 * RoleCheckFilter — the actual authorization filter created by RequireRoleAttribute.
 * Not registered in DI; created exclusively through IFilterFactory.CreateInstance.
 * Receives _role from the attribute and ILogger<T> from DI.
 */
public sealed class RoleCheckFilter : IAuthorizationFilter
{
    private readonly string _requiredRole;
    private readonly ILogger<RoleCheckFilter> _logger;

    public RoleCheckFilter(string requiredRole, ILogger<RoleCheckFilter> logger)
    {
        _requiredRole = requiredRole; // forwarded from the attribute
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.IsInRole(_requiredRole))
        {
            _logger.LogWarning("User lacks required role: {Role}", _requiredRole);
            context.Result = new ForbidResult(); // 403 — short-circuit the pipeline
        }
    }
}

// ── Request DTO — used by CreateOrder ─────────────────────────────────────────
/*
 * CreateOrderRequest demonstrates how data annotations on a DTO work with
 * ValidateModelFilter.  When Item is absent from the JSON body, model binding
 * sets ModelState["Item"].Errors — ValidateModelFilter.OnActionExecuting then
 * sets context.Result to 422 before CreateOrder is ever called.
 */
public sealed class CreateOrderRequest
{
    [Required]
    public string Item { get; set; } = string.Empty; // [Required] → ModelState error if absent

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;            // [Range] → ModelState error if < 1 or > 1000
}
