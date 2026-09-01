/*
 * FILE ROLE: Demonstrates all six binding-source attributes in controller actions,
 *            ModelState validation, ProblemDetails responses, and custom binder usage.
 * SECTIONS IN THIS FILE:
 *   6a. [ApiController] — automatic binding inference and ModelState filter
 *   6b. [FromBody]      — JSON request body
 *   6c. [FromQuery]     — complex query-string model
 *   6d. [FromRoute]     — URL path segment
 *   6e. [FromHeader]    — HTTP request header
 *   6f. [FromForm]      — multipart/form-data fields
 *   6g. [FromServices]  — DI container injection in action parameters
 *   6h. ModelState — IsValid, AddModelError, ValidationProblem()
 *   6i. [ModelBinder]   — explicit custom binder on a parameter
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelBindingValidation.Binders;
using ModelBindingValidation.Models;
using System;
using System.Collections.Generic;

namespace ModelBindingValidation.Controllers;

/*
 * SECTION 6a: [ApiController] — AUTOMATIC BINDING INFERENCE AND ModelState FILTER
 *
 * [ApiController] activates several opinionated behaviours on top of [Route]:
 *
 *   1. Automatic binding inference
 *      ────────────────────────────
 *      Without [ApiController], every parameter needs an explicit [From*].
 *      With [ApiController], the framework infers binding sources:
 *        - Simple types (int, string, bool, Guid, …) → [FromRoute] if a matching
 *          route segment exists, otherwise [FromQuery].
 *        - Complex types (classes / structs) → [FromBody].
 *        - IFormFile / IFormFileCollection → [FromForm] always.
 *      Explicit [From*] attributes on parameters override these inferences.
 *
 *   2. Automatic ModelState validation filter
 *      ────────────────────────────────────────
 *      Before the action body runs, if ModelState.IsValid is false, the filter
 *      returns 400 (or 422 if customised — see Program.cs) with a
 *      ValidationProblemDetails body. The action method is NOT called.
 *      Result: you rarely need `if (!ModelState.IsValid)` boilerplate in API
 *      controllers. Use ModelState manually only when adding EXTRA errors after
 *      binding (e.g., uniqueness checks against the database).
 *
 *   3. [FromBody] required body error
 *      ─────────────────────────────────
 *      If the body is missing or malformed for a [FromBody] parameter, the filter
 *      adds a binding error to ModelState and returns 400/422 immediately.
 *
 * PITFALL — multiple [FromBody] parameters:
 *   Only ONE parameter per action can carry [FromBody] (HTTP bodies are streams,
 *   read once). A second [FromBody] causes a runtime InvalidOperationException,
 *   not a compile-time error.
 */
[ApiController]
[Route("api/[controller]")]                                 // → /api/products
public sealed class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;   // injected via constructor DI

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }

    /*
     * SECTION 6b: [FromBody] — JSON REQUEST BODY
     *
     * [FromBody] reads and deserializes the request body (Content-Type: application/json).
     * The binder uses System.Text.Json by default; swap to Newtonsoft.Json by calling
     * AddControllers().AddNewtonsoftJson().
     *
     * Key behaviours:
     *   - [ApiController] adds [Required] semantics to [FromBody] implicitly.
     *     A missing or null body triggers a 400/422 before the action runs.
     *   - The body stream is read only once. Do not read HttpContext.Request.Body
     *     manually in the same action.
     *   - DataAnnotations on CreateProductRequest are validated automatically.
     *     IValidatableObject.Validate() runs if all attribute checks pass.
     *
     * Request: POST /api/products
     *   Body: { "name": "Widget", "price": 9.99, "stock": 50, "sku": "WGT-0001" }
     */
    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequest request)
    {
        // If we reach here, ModelState.IsValid == true (automatic filter ran first)
        _logger.LogInformation("Creating product: {Name}, Price: {Price}", request.Name, request.Price);

        // In a real app: call service layer, persist, return 201 with Location header
        return CreatedAtAction(nameof(GetById), new { id = 1 }, request);
    }

    /*
     * SECTION 6c: [FromQuery] — COMPLEX QUERY-STRING MODEL
     *
     * [FromQuery] on a complex type reads each property from the query string.
     * See Models/PaginationQuery.cs for the collection-binding details.
     *
     * Request: GET /api/products?page=2&pageSize=25&sortBy=name&tags=electronics&tags=sale
     */
    [HttpGet]
    public IActionResult GetAll([FromQuery] PaginationQuery pagination)
    {
        return Ok(new
        {
            pagination.Page,
            pagination.PageSize,
            pagination.SortBy,
            pagination.SortDescending,
            tags = pagination.Tags,
            categoryIds = pagination.CategoryIds,
        });
    }

    /*
     * SECTION 6d: [FromRoute] — URL PATH SEGMENT
     *
     * [FromRoute] binds the parameter from a {name} segment in the route template.
     * The route template "{id:int}" additionally applies a route constraint:
     * requests where {id} cannot parse as int are rejected with 404 (route not
     * matched) rather than a model-binding 400.
     *
     * Best practice: use route constraints (:int, :guid, :min(1)) for structural
     * validation (shape/format) and DataAnnotations for business validation (range,
     * patterns). Route constraint failures → 404; annotation failures → 400/422.
     *
     * Request: GET /api/products/42
     */
    [HttpGet("{id:int}")]
    public IActionResult GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            // Manually add a ModelState error and return ValidationProblemDetails
            ModelState.AddModelError(nameof(id), "Id must be a positive integer.");
            return ValidationProblem();             // ControllerBase helper → 400 ProblemDetails
        }
        return Ok(new { id, name = $"Product {id}" });
    }

    /*
     * SECTION 6e: [FromHeader] — HTTP REQUEST HEADER
     *
     * [FromHeader(Name = "X-Api-Version")] reads the named header value.
     * Header names are case-insensitive per HTTP spec; the Name argument
     * matches case-insensitively as well.
     *
     * PITFALL — [Required] on headers:
     *   Add [Required] to force the header's presence; without it, a missing
     *   header gives a null value and the action still runs.
     *
     * Request: GET /api/products/version   X-Api-Version: 2.1
     */
    [HttpGet("version")]
    public IActionResult GetVersion(
        [FromHeader(Name = "X-Api-Version")] string? apiVersion)
    {
        return Ok(new { apiVersion = apiVersion ?? "1.0" }); // default when header absent
    }

    /*
     * SECTION 6f: [FromForm] — FORM-ENCODED DATA
     *
     * [FromForm] reads values from multipart/form-data or
     * application/x-www-form-urlencoded request bodies.
     *
     * When to use:
     *   - File uploads (IFormFile parameters → [FromForm] is implied).
     *   - HTML form posts from browsers that cannot send JSON.
     *   - Mixed payloads: a file + scalar fields in one multipart request.
     *
     * [Consumes] restricts the action to a specific Content-Type, avoiding
     * routing ambiguity when a controller has both JSON and form endpoints.
     *
     * Request: POST /api/products/upload  Content-Type: multipart/form-data
     *   productName=Widget; price=9.99
     */
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public IActionResult Upload(
        [FromForm] string productName,
        [FromForm] decimal price)
    {
        return Ok(new { productName, price });
    }

    /*
     * SECTION 6g: [FromServices] — DI CONTAINER IN ACTION PARAMETERS
     *
     * [FromServices] injects a service from the DI container directly into an
     * action parameter, without adding a constructor parameter. Useful when:
     *   - The service is needed by only one action in the controller.
     *   - You want to keep the constructor lean (e.g., controller has many actions
     *     but only one needs a heavyweight service).
     *
     * ASP.NET Core resolves the service from the request scope, exactly the same
     * as constructor injection. The service must already be registered (e.g.,
     * builder.Services.AddLogging() in Program.cs).
     *
     * PITFALL — confusion with automatic inference:
     *   [ApiController] does NOT automatically infer [FromServices]. Without the
     *   explicit attribute, a complex type is assumed to come from [FromBody].
     *   Always add [FromServices] explicitly for DI-injected action parameters.
     *
     * Request: GET /api/products/log-test
     */
    [HttpGet("log-test")]
    public IActionResult LogTest(
        [FromServices] ILogger<ProductsController> logger) // same type, different scope demo
    {
        logger.LogInformation("LogTest action invoked via [FromServices] injection");
        return Ok(new { message = "Logged via [FromServices]-injected logger." });
    }

    /*
     * SECTION 6h: ModelState — IsValid, AddModelError, ValidationProblem
     *
     * ModelState (ModelStateDictionary) is the live record of binding + validation
     * errors for the current request.
     *
     *   ModelState.IsValid
     *     false if ANY binder or validator added an error. With [ApiController],
     *     the automatic filter short-circuits before this action runs when false.
     *     Use it manually when you disabled SuppressModelStateInvalidFilter or
     *     when you add extra errors inside the action.
     *
     *   ModelState.AddModelError(key, errorMessage)
     *     Adds a custom error keyed to a property name or to the model root ("").
     *     Use for business-rule validation (e.g., uniqueness) that cannot be
     *     expressed in DataAnnotations.
     *
     *   ValidationProblem()
     *     ControllerBase helper that packages ModelState errors into a
     *     ValidationProblemDetails response (HTTP 400 by default).
     *     Equivalent to BadRequest(new ValidationProblemDetails(ModelState)).
     *
     * IValidatableObject.Validate() (see Models/AddressModel.cs):
     *   The framework calls Validate() automatically during model binding if all
     *   per-property attributes pass. Any returned ValidationResult objects are
     *   added to ModelState. The automatic filter then catches them.
     *
     * Request: POST /api/products/validate-address
     *   Body: { "street": "123 Main St", "city": "Springfield", "postalCode": "62701",
     *           "isInternational": false }
     */
    [HttpPost("validate-address")]
    public IActionResult ValidateAddress([FromBody] AddressModel address)
    {
        // IValidatableObject.Validate() already ran during binding (State == false if
        // IsInternational=true without Country). Add one more controller-level rule:
        if (address.City.Equals(address.Street, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(address.City), "City cannot be the same as Street.");
        }

        if (!ModelState.IsValid)
            return ValidationProblem();             // package all errors as 400 ProblemDetails

        return Ok(address);
    }

    /*
     * SECTION 6i: [ModelBinder] — EXPLICIT CUSTOM BINDER ON A PARAMETER
     *
     * [ModelBinder(BinderType = typeof(...))] bypasses provider resolution and
     * uses the named binder directly for that parameter. This is the "opt-in"
     * approach (vs registering a provider globally in Program.cs options).
     *
     * Usage: GET /api/products/by-ids?ids=1,5,12
     *   The query string "1,5,12" is parsed into List<int>{ 1, 5, 12 } by
     *   CommaSeparatedListBinder. Repeated keys (?ids=1&ids=5) also work via the
     *   default collection binder, but this action explicitly requests the
     *   comma-separated binder instead.
     *
     * ModelState errors are added by the binder if any token is non-integer, and
     * the automatic [ApiController] filter catches them before this action runs.
     */
    [HttpGet("by-ids")]
    public IActionResult GetByIds(
        [ModelBinder(BinderType = typeof(CommaSeparatedListBinder))] List<int> ids)
    {
        return Ok(new { ids, count = ids.Count });
    }
}
