/*
 * TOPIC: Model Binding & Validation
 *
 * WHY IT MATTERS:
 *   Every HTTP request carries data: JSON in the body, values in the query string,
 *   IDs in URL path segments, credentials in headers, files in form fields. Model
 *   binding is ASP.NET Core's mechanism for mapping that raw HTTP data onto
 *   strongly-typed C# parameters automatically. Validation then ensures the data
 *   meets business rules before a controller action executes — stopping bad data
 *   at the HTTP perimeter rather than propagating it into business logic or the
 *   database layer.
 *
 * WHAT YOU WILL LEARN:
 *    1. Binding sources: [FromBody], [FromQuery], [FromRoute], [FromHeader],
 *       [FromForm], [FromServices] — purpose and precedence rules
 *    2. Complex type binding and collection binding from query strings
 *    3. Custom IModelBinder and IModelBinderProvider
 *    4. ModelState — accumulating and checking validation errors
 *    5. DataAnnotations: [Required], [Range], [StringLength], [RegularExpression],
 *       [EmailAddress], [Compare]
 *    6. IValidatableObject — cross-property rules in the model class
 *    7. Custom ValidationAttribute — reusable attribute-based rules
 *    8. ProblemDetails / ValidationProblemDetails — RFC 9457 error responses
 *    9. SuppressModelStateInvalidFilter — opt out of automatic 400 responses
 *   10. Minimal API parameter binding — route, query, body, [AsParameters]
 *   11. PREVIEW: FluentValidation (third-party library — depth deferred)
 *
 * CHAPTER MAP:
 *   1. DataAnnotations DTO        → Models/CreateProductRequest.cs
 *   2. Query & collection binding → Models/PaginationQuery.cs
 *   3. IValidatableObject         → Models/AddressModel.cs
 *   4. Custom ValidationAttribute → Validation/FutureDateAttribute.cs
 *   5. Custom IModelBinder        → Binders/CommaSeparatedListBinder.cs
 *   6. Controller & ModelState    → Controllers/ProductsController.cs
 *   7. MVC setup & API behavior   → Program.cs  Section 7
 *   8. Minimal API binding        → Program.cs  Section 8
 *
 * READ ORDER: open files 1–6 in the Chapter Map, then return here for Sections 7–8.
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModelBindingValidation.Models;

/*
 * SECTION 7: MVC REGISTRATION AND API BEHAVIOR OPTIONS
 *
 * AddControllers() registers the model binding and validation infrastructure:
 *   - Value providers (query string, route, form, header)
 *   - Model binder providers (complex types, collections, primitives, …)
 *   - DataAnnotations validation (attributes + IValidatableObject)
 *   - The [ApiController] automatic-validation filter
 *   - System.Text.Json input/output formatters
 *
 * ConfigureApiBehaviorOptions() controls what [ApiController] does automatically:
 *
 *   SuppressModelStateInvalidFilter (default: false)
 *   ─────────────────────────────────────────────────
 *   false → the framework short-circuits to 400/422 when ModelState.IsValid is
 *           false — action body does NOT execute. No boilerplate needed.
 *   true  → the action always runs; you handle validation errors manually.
 *           Use this when a single endpoint must process partial data or when
 *           you want to merge business-rule errors with validation errors before
 *           responding.
 *
 *   InvalidModelStateResponseFactory
 *   ──────────────────────────────────
 *   Replaces the default 400 response factory. Below, the status code is changed
 *   to 422 Unprocessable Content (RFC 9110 §15.5.22) and a traceId extension is
 *   added so clients can correlate errors with server logs.
 *
 * Custom ModelBinder registration (global, alternative to [ModelBinder] attribute):
 *   options.ModelBinderProviders.Insert(0, new CommaSeparatedListBinderProvider())
 *   This example uses [ModelBinder] on the parameter instead (see Controllers/
 *   ProductsController.cs Section 6i) — safer because it only affects that action.
 *   (Using CommaSeparatedListBinderProvider globally would replace the default
 *   collection binder for ALL List<int> parameters in the application.)
 */
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Uncomment to let every action run even when ModelState is invalid:
        // options.SuppressModelStateInvalidFilter = true;

        // Return 422 instead of 400, add traceId for log correlation
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title  = "One or more validation errors occurred.",
                Type   = "https://tools.ietf.org/html/rfc9110#section-15.5.22",
            };
            problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            return new UnprocessableEntityObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
            };
        };
    });

var app = builder.Build();

/*
 * SECTION 8: MINIMAL API PARAMETER BINDING
 *
 * Minimal APIs (app.MapGet/MapPost/…) bind parameters without a controller class.
 * ASP.NET Core infers the binding source automatically:
 *
 *   Inference order in minimal APIs:
 *   ───────────────────────────────────────────────────────────────────────
 *   1. Route parameter    name matches a {segment} in the route template
 *   2. DI service         type is registered in the DI container
 *   3. HttpContext / etc. special known types (CancellationToken, HttpRequest, …)
 *   4. Query string       primitive / string not matched above
 *   5. Body               complex type not from DI — auto-inferred as [FromBody]
 *   Explicit [FromBody], [FromQuery], [FromHeader], [FromForm] always win.
 *
 *   BINDING PRECEDENCE COMPARISON — controllers vs minimal APIs:
 *   ─────────────────────────────────────────────────────────────────────
 *   | Parameter type   | Controller ([ApiCtrl]) | Minimal API           |
 *   |─────────────────────────────────────────────────────────────────|
 *   | Simple (int)     | [FromRoute] → [FromQuery] | route → query      |
 *   | Complex class    | [FromBody]                | route/query/body*   |
 *   | Registered DI    | [FromServices] (explicit) | auto from DI        |
 *   | IFormFile        | [FromForm] always         | [FromForm] always   |
 *   * Complex type in minimal API with no DI registration → [FromBody].
 *
 * [AsParameters] — bind multiple query/route params into a record/struct:
 *   Decorating a parameter with [AsParameters] tells the framework to map each
 *   public property (or primary-constructor parameter for records) from route and
 *   query string individually, rather than reading from the body. Equivalent to
 *   listing each parameter separately but grouped for readability.
 *   Requires .NET 7+ (available here: net8.0).
 *
 * PITFALL — validation in minimal APIs:
 *   Minimal APIs do NOT automatically run DataAnnotations validators. You must
 *   call Validator.TryValidateObject() manually, use a filter, or use
 *   FluentValidation with its IEndpointFilter integration.
 */

// Route parameter {id:int} — bound from URL path segment, route constraint applied
app.MapGet("/api/minimal/products/{id:int}", (int id) =>
    Results.Ok(new { id, name = $"Product {id}" }));

// Query string parameters with defaults — ?page=2&pageSize=50
app.MapGet("/api/minimal/products", (int page = 1, int pageSize = 20) =>
    Results.Ok(new { page, pageSize }));

// [AsParameters] — binds each record property individually from route + query
app.MapGet("/api/minimal/products/search",
    ([AsParameters] MinimalSearchQuery query) => Results.Ok(query));

// [FromBody] — explicit JSON body binding
app.MapPost("/api/minimal/products",
    ([FromBody] CreateProductRequest request) =>
        Results.Created("/api/minimal/products/1", request));

// [FromHeader] — explicit header binding
app.MapGet("/api/minimal/version",
    ([FromHeader(Name = "X-Api-Version")] string? version) =>
        Results.Ok(new { version }));

app.MapControllers();
app.Run();

/*
 * SECTION 11 — PREVIEW: FluentValidation
 *
 * FluentValidation is a popular third-party library (FluentValidation.AspNetCore NuGet)
 * that replaces DataAnnotations with a strongly-typed, fluent DSL defined in separate
 * validator classes.
 *
 *   COVERED IN DETAIL LATER → (dedicated FluentValidation chapter, when added)
 *
 * Why choose FluentValidation over DataAnnotations?
 *   ┌────────────────────────┬──────────────────────────────────────────────┐
 *   │ DataAnnotations        │ FluentValidation                             │
 *   ├────────────────────────┼──────────────────────────────────────────────┤
 *   │ Attributes on the DTO  │ Rules in a separate AbstractValidator<T>     │
 *   │ Limited composability  │ Composable: RuleFor().NotEmpty().MaxLength() │
 *   │ No async validation    │ Async rules: MustAsync(…)                    │
 *   │ Hard to unit-test      │ Validators are plain classes — easy to test  │
 *   └────────────────────────┴──────────────────────────────────────────────┘
 *
 * Integration sketch (NOT compiled — FluentValidation not referenced here):
 *
 *   public class CreateProductRequestValidator
 *       : AbstractValidator<CreateProductRequest>
 *   {
 *       public CreateProductRequestValidator()
 *       {
 *           RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
 *           RuleFor(x => x.Price).InclusiveBetween(0.01m, 99999.99m);
 *           RuleFor(x => x.ContactEmail).EmailAddress().When(x => x.ContactEmail != null);
 *       }
 *   }
 *
 *   // Registration in Program.cs:
 *   builder.Services
 *       .AddFluentValidationAutoValidation()
 *       .AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
 *
 * ─────────────────────────────────────────────────────────────────────────────
 * QUICK REFERENCE — Model Binding & Validation
 * ─────────────────────────────────────────────────────────────────────────────
 * BINDING SOURCE ATTRIBUTES
 *   [FromBody]                   JSON request body; one per action
 *   [FromQuery]                  ?key=value; complex types + collections
 *   [FromRoute]                  {segment} in the route template
 *   [FromHeader(Name = "X-…")]   HTTP request header
 *   [FromForm]                   multipart/form-data or x-www-form-urlencoded
 *   [FromServices]               DI container — no constructor param needed
 *
 * BINDING PRECEDENCE (controller with [ApiController])
 *   Simple type → [FromRoute] if name matches, else [FromQuery]
 *   Complex type → [FromBody]
 *   Explicit [From*] always wins
 *
 * DataAnnotations ATTRIBUTES
 *   [Required]                   Non-null, non-empty string required
 *   [Range(min, max)]            Numeric or IComparable range
 *   [StringLength(max)]          Character count; MinimumLength for lower bound
 *   [RegularExpression(pattern)] Regex match (whole string, use verbatim @"…")
 *   [EmailAddress]               Valid email format (not full RFC-5321)
 *   [Compare("OtherProp")]       Two properties must be equal; use nameof()
 *   Custom ValidationAttribute   Override IsValid(value, context) for reusable rules
 *
 * CROSS-PROPERTY VALIDATION
 *   IValidatableObject.Validate() Called after all per-property attributes pass
 *   yield return new ValidationResult(msg, new[]{nameof(Prop)})
 *
 * ModelState
 *   ModelState.IsValid            false if any binding/validation error exists
 *   ModelState.AddModelError(key, msg)  Add a custom error in the action
 *   ValidationProblem()           Returns 400 ValidationProblemDetails
 *   SuppressModelStateInvalidFilter    Disable the automatic 400 short-circuit
 *
 * PROBLEMDETAILS (RFC 9457)
 *   ValidationProblemDetails      Extends ProblemDetails with an Errors dict
 *   InvalidModelStateResponseFactory  Customize the automatic error response
 *   application/problem+json       Content-Type for ProblemDetails responses
 *
 * MINIMAL API BINDING
 *   Route segment                 Inferred from route template match
 *   Query string                  Inferred for simple types not in route
 *   [AsParameters]                Map record/struct properties from route + query
 *   [FromBody]                    Explicit JSON body binding
 *   DI service                    Auto-inferred if type registered in container
 *   ⚠ DataAnnotations NOT auto-validated — use filter or manual Validator.TryValidateObject
 */

// [AsParameters] target — positional record; each property bound from route/query
public record MinimalSearchQuery(
    string?  Name,
    decimal? MinPrice,
    decimal? MaxPrice,
    int      Page     = 1,
    int      PageSize = 20);
