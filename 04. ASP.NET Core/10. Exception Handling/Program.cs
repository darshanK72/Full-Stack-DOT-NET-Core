/*
 * TOPIC: Exception Handling in ASP.NET Core
 *
 * WHY IT MATTERS:
 *   Every API throws — database timeouts, missing records, validation failures, bugs.
 *   Without a centralised handler each controller has try/catch duplication,
 *   inconsistent error shapes, and missing log correlation. Done right, exception
 *   handling is a cross-cutting concern owned by ONE place: the middleware pipeline.
 *
 * WHAT YOU WILL LEARN:
 *   1. Custom exception hierarchy with HTTP status code mapping
 *   2. NotFoundException — 404 leaf type
 *   3. ValidationException — 422 leaf type with per-field errors
 *   4. Structured error DTO (ErrorResponse) vs ProblemDetails
 *   5. GlobalExceptionHandler via IExceptionHandler (.NET 8+)
 *      — IExceptionHandlerFeature, ProblemDetails (RFC 7807), correlation ID, logging
 *   6. Controller actions — throwing, exception filters vs middleware, IProblemDetailsService
 *   7. Middleware pipeline wiring (this file — sections below)
 *
 * CHAPTER MAP (reading order):
 *   1. Custom exception hierarchy     → Exceptions/AppException.cs
 *   2. NotFoundException (HTTP 404)   → Exceptions/NotFoundException.cs
 *   3. ValidationException (HTTP 422) → Exceptions/ValidationException.cs
 *   4. Structured error DTO           → Models/ErrorResponse.cs
 *   5. GlobalExceptionHandler         → Handlers/GlobalExceptionHandler.cs
 *   6. Controller — actions, filters  → Controllers/ProductsController.cs
 *   7. Middleware pipeline wiring     → Program.cs (this file, sections below)
 */

using ExceptionHandling.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

/*
 * SECTION 1: SERVICE REGISTRATION
 *
 * --- 1a. AddControllers ---
 * Registers the MVC controller pipeline: model binding, action filters,
 * IActionResult handling, and the default 400 response for invalid ModelState.
 *
 * --- 1b. AddProblemDetails ---
 * Registers IProblemDetailsService in DI. Effects:
 *   • UseExceptionHandler's built-in fallback returns ProblemDetails (not plain text).
 *   • Controllers and handlers can resolve IProblemDetailsService to write
 *     ProblemDetails responses manually (see Controllers/ProductsController.cs §3).
 *   • Accepts an options delegate to add custom logic to every problem response:
 *
 *       builder.Services.AddProblemDetails(options =>
 *       {
 *           options.CustomizeProblemDetails = ctx =>
 *           {
 *               ctx.ProblemDetails.Extensions["machineId"] = Environment.MachineName;
 *           };
 *       });
 *
 * --- 1c. AddExceptionHandler<T> ---
 * Registers GlobalExceptionHandler as an IExceptionHandler in the DI container.
 * UseExceptionHandler() (Section 3) resolves and invokes this chain.
 * Order matters: the first registered handler runs first.
 */
builder.Services.AddControllers();
builder.Services.AddProblemDetails();                                // registers IProblemDetailsService
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();     // IExceptionHandler chain entry

WebApplication app = builder.Build();

/*
 * SECTION 2: UseDeveloperExceptionPage — DEV-ONLY DETAILED ERRORS
 *
 * Shows a rich HTML page with the full exception, stack trace, request details,
 * and query string — invaluable during development.
 *
 * Rules:
 *   • NEVER enable in production — leaks stack traces, file paths, and server
 *     internals to potentially hostile clients.
 *   • In .NET 8+ WebApplicationBuilder automatically adds this page in the
 *     Development environment. The explicit call below is for clarity.
 *   • Primarily useful for browser-facing MVC/Razor apps. Web APIs return JSON,
 *     so the HTML page is less useful there — use structured logging instead.
 *   • Place FIRST in the pipeline so it wraps all middleware below it.
 *
 * Relationship with UseExceptionHandler (Section 3):
 *   Both catch exceptions, but UseDeveloperExceptionPage runs first in the pipeline
 *   when ordered before UseExceptionHandler. In this demo, UseExceptionHandler is
 *   added unconditionally so GlobalExceptionHandler demonstrates its behaviour even
 *   in development. In a real project use one or the other per environment:
 *
 *     if (app.Environment.IsDevelopment())
 *         app.UseDeveloperExceptionPage();       // dev: full HTML stack trace
 *     else
 *         app.UseExceptionHandler();             // prod: ProblemDetails via handler chain
 */
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // auto-added by SDK in dev; shown here for learning
}

/*
 * SECTION 3: UseExceptionHandler — PRODUCTION SAFETY NET
 *
 * Wraps the middleware below it in a try/catch. When an unhandled exception
 * propagates up, this middleware:
 *   1. Stores the exception in IExceptionHandlerFeature (HttpContext feature slot).
 *   2. Resets the response (clears headers and body written so far).
 *   3. Tries each registered IExceptionHandler in registration order.
 *   4. If all return false, writes a default 500 ProblemDetails response
 *      (because AddProblemDetails is registered in Section 1).
 *
 * Three call forms:
 *
 *   app.UseExceptionHandler()
 *     → Uses registered IExceptionHandler chain (recommended with .NET 8+).
 *
 *   app.UseExceptionHandler("/error")
 *     → Re-executes the request at /error; handler endpoint reads
 *       IExceptionHandlerPathFeature.Error for the original exception.
 *       Useful for MVC apps that need a full controller response.
 *
 *   app.UseExceptionHandler(errorApp => { errorApp.Run(async ctx => { ... }); })
 *     → Inline lambda; fine for simple apps. DI injection is possible but awkward.
 *       See Handlers/GlobalExceptionHandler.cs §2 for a lambda example.
 *
 * Middleware ordering rule:
 *   UseExceptionHandler MUST appear before UseRouting / MapControllers so it
 *   can catch exceptions from all subsequent middleware.
 */
app.UseExceptionHandler(); // invokes GlobalExceptionHandler chain (registered in Section 1)

/*
 * SECTION 4: STATUS CODE PAGES — HANDLING EMPTY ERROR RESPONSES
 *
 * UseStatusCodePages* intercepts responses that have an error status code
 * (4xx / 5xx) but an EMPTY body. This is different from exception handling:
 *
 *   UseExceptionHandler — catches THROWN EXCEPTIONS that propagate up the pipeline.
 *   UseStatusCodePages  — catches RESPONSES with error status and no body
 *                         (e.g. a 404 from routing when no route matches, a 405 Method Not Allowed).
 *
 * The three variants:
 *
 *   app.UseStatusCodePages()
 *     → Writes plain text: "Status Code: 404; Not Found". Minimal; rarely used in production.
 *
 *   app.UseStatusCodePagesWithRedirects("/error?code={0}")
 *     → Sends an HTTP 302 redirect to the error URL. The browser follows the redirect
 *       as a new GET request. Original status code is lost — response returns 200.
 *       Suitable for MVC apps with a visible error page. Avoid in pure APIs.
 *
 *   app.UseStatusCodePagesWithReExecute("/error/{0}")
 *     → Re-executes the SAME request pipeline at the new path WITHOUT a redirect.
 *       The original status code is preserved. Preferred for APIs and MVC apps.
 *       {0} is replaced with the numeric status code (e.g. /error/404).
 *
 * Ordering: place AFTER UseExceptionHandler (exceptions have already been handled)
 * and BEFORE MapControllers (routing errors happen in the endpoint layer).
 *
 * Loop prevention: if the /error/{0} endpoint itself throws or returns an error
 * status, ASP.NET Core detects the re-execution loop and stops further recursion.
 */
app.UseStatusCodePagesWithReExecute("/error/{0}"); // e.g. unmatched route → /error/404

/*
 * SECTION 5: DEMO ENDPOINTS AND STARTUP
 *
 * Root endpoint — confirms the app is running.
 * Error endpoint — handles status-code-page re-execution from Section 4.
 * MapControllers — activates ProductsController (Controllers/ProductsController.cs).
 *
 * Try these routes once the app is running (dotnet run):
 *   GET  /                        → welcome message
 *   GET  /api/products/1          → 200 { "id": 1, "name": "Widget" }
 *   GET  /api/products/99         → 404 ProblemDetails (NotFoundException)
 *   POST /api/products {}         → 422 ProblemDetails (ValidationException with field errors)
 *   GET  /api/products/1/safe     → 200 via IProblemDetailsService path
 *   GET  /api/products/99/safe    → 404 via IProblemDetailsService (no throw)
 *   GET  /api/nonexistent         → routing 404 → /error/404 re-execution → ProblemDetails
 */

// Root welcome endpoint — no controller needed for this
app.MapGet("/", () => Results.Ok(new
{
    message = "Exception Handling demo running.",
    hint    = "Try GET /api/products/1, GET /api/products/99, or POST /api/products with {}"
}));

// Error re-execution endpoint — called by UseStatusCodePagesWithReExecute for empty 4xx/5xx
app.MapGet("/error/{statusCode:int}", (int statusCode) =>
    Results.Problem(
        title:      $"HTTP {statusCode}",
        detail:     "An error occurred. See server logs for details with the correlation ID.",
        statusCode: statusCode));

app.MapControllers(); // activates ProductsController at /api/products

app.Run();

/*
 * QUICK REFERENCE — Exception Handling in ASP.NET Core
 * ─────────────────────────────────────────────────────
 *
 * Registration (Program.cs)
 *   builder.Services.AddProblemDetails()                     → IProblemDetailsService + RFC 7807 defaults
 *   builder.Services.AddExceptionHandler<MyHandler>()        → registers IExceptionHandler (.NET 8+)
 *
 * Middleware (order matters — top = outermost wrapper)
 *   app.UseDeveloperExceptionPage()                          → dev only; HTML stack trace
 *   app.UseExceptionHandler()                                → invokes IExceptionHandler chain
 *   app.UseExceptionHandler("/error")                        → re-executes at /error endpoint
 *   app.UseExceptionHandler(errorApp => { ... })             → inline lambda handler
 *   app.UseStatusCodePages()                                 → plain text for empty 4xx/5xx
 *   app.UseStatusCodePagesWithReExecute("/error/{0}")        → re-executes (preserves status code) ✓
 *   app.UseStatusCodePagesWithRedirects("/error?code={0}")   → 302 redirect (loses status code)
 *
 * IExceptionHandler (.NET 8+)
 *   TryHandleAsync returns true  → exception consumed; pipeline stops
 *   TryHandleAsync returns false → next handler tried; default 500 if all return false
 *   Multiple registrations       → tried in registration order (chain of responsibility)
 *
 * IExceptionHandlerFeature (older pattern)
 *   context.Features.Get<IExceptionHandlerFeature>()?.Error            → caught exception
 *   context.Features.Get<IExceptionHandlerPathFeature>()?.Path         → original request path
 *
 * ProblemDetails (RFC 7807)   Content-Type: application/problem+json
 *   type, title, status, detail, instance     → standard fields
 *   Extensions["key"] = value                 → flat top-level JSON via [JsonExtensionData]
 *
 * CorrelationId strategy
 *   X-Correlation-Id header → X-Request-Id header → HttpContext.TraceIdentifier
 *   Echo in response header AND in ProblemDetails Extensions["correlationId"]
 *
 * Exception filter vs middleware
 *   Filter  → MVC actions only; access to ActionContext/ModelState; returns IActionResult
 *   Handler → entire pipeline; catches routing errors; writes directly to HttpContext
 *
 * Custom exception hierarchy
 *   AppException(message, statusCode, errorCode)
 *     ├── NotFoundException(entity, key)       → 404 NOT_FOUND
 *     └── ValidationException(errors)          → 422 VALIDATION_ERROR
 *
 * IProblemDetailsService (non-throw path)
 *   httpContext.Response.StatusCode = 404;
 *   await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext { ... });
 *   return new EmptyResult(); // response already written
 *
 * Logging levels for exceptions
 *   LogWarning → expected domain exceptions (NotFoundException, ValidationException)
 *   LogError   → unexpected / unhandled exceptions
 */
