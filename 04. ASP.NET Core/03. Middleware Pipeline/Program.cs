/*
 * TOPIC: Middleware Pipeline
 *
 * WHY IT MATTERS:
 *   Every HTTP request in ASP.NET Core travels through an ordered chain of
 *   middleware components before it reaches an endpoint — and then back out
 *   through the same chain as the response is built. This "pipeline" is the
 *   single place to add cross-cutting concerns (logging, timing, error handling,
 *   auth, CORS) without duplicating that logic in every controller or endpoint.
 *
 *   The pipeline execution model is a "Russian doll" (or "onion"):
 *
 *     Request
 *       │
 *       ▼
 *     ┌──────────────────────────────────────────────────────────────┐
 *     │ Middleware A — IN leg  (e.g. start timer, log request path) │
 *     │  ┌───────────────────────────────────────────────────────┐  │
 *     │  │ Middleware B — IN leg  (e.g. validate API key)        │  │
 *     │  │  ┌──────────────────────────────────────────────── ┐  │  │
 *     │  │  │  Endpoint handler / terminal middleware          │  │  │
 *     │  │  └──────────────────────────────────────────────── ┘  │  │
 *     │  │ Middleware B — OUT leg (e.g. reject if key missing)    │  │
 *     │  └───────────────────────────────────────────────────────┘  │
 *     │ Middleware A — OUT leg (e.g. write X-Response-Time header)  │
 *     └──────────────────────────────────────────────────────────────┘
 *       │
 *       ▼
 *     Response
 *
 *   Code BEFORE await next() runs on the way IN.
 *   Code AFTER  await next() runs on the way OUT (after all downstream finishes).
 *
 * WHAT YOU WILL LEARN:
 *   1. The pipeline execution model (in/out legs)
 *   2. Use() / Run() / Map() / MapWhen() / UseWhen() primitives
 *   3. Built-in middleware and why ORDER matters
 *   4. Writing a custom class-based middleware (IMiddleware)
 *   5. Writing a convention-based middleware
 *   6. Encapsulating registration with UseXxx extension methods
 *   7. HttpContext: Request, Response, Connection
 *   8. Short-circuiting the pipeline
 *
 * CHAPTER MAP:
 *   1. Pipeline primitives (Use/Run/Map/MapWhen/UseWhen)  → Program.cs  Section 1 (below)
 *   2. Built-in middleware & correct order                 → Program.cs  Section 2 (below)
 *   3. IMiddleware (class-based approach)                  → Middleware/RequestLoggingMiddleware.cs
 *   4. Convention-based middleware                         → Middleware/TimingMiddleware.cs
 *   5. UseXxx extension methods                            → Extensions/MiddlewareExtensions.cs
 *   6. Pipeline wiring — everything assembled              → Program.cs  Section 3 (below)
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiddlewarePipeline.Extensions;
using MiddlewarePipeline.Middleware;
using System;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

/*
 * DI REGISTRATION FOR MIDDLEWARE
 * IMiddleware-based components must be registered with the DI container so ASP.NET
 * Core can resolve them per request. Convention-based middleware does NOT need DI
 * registration — it is instantiated once at startup by the framework.
 * See Middleware/RequestLoggingMiddleware.cs (IMiddleware) vs
 *     Middleware/TimingMiddleware.cs (convention-based) for the full explanation.
 */
builder.Services.AddTransient<RequestLoggingMiddleware>(); // IMiddleware — DI required
builder.Services.AddAuthentication();                       // enables UseAuthentication()
builder.Services.AddAuthorization();                        // enables UseAuthorization()

WebApplication app = builder.Build();

/*
 * SECTION 1: Use() / Run() / Map() / MapWhen() / UseWhen()
 *
 * These are the five primitives for building the middleware pipeline on IApplicationBuilder.
 *
 * ┌──────────────┬──────────────────────────────────────────────────────────────────────┐
 * │ Method       │ Behaviour                                                            │
 * ├──────────────┼──────────────────────────────────────────────────────────────────────┤
 * │ Use(fn)      │ Adds a component. fn receives (HttpContext, RequestDelegate). Calling │
 * │              │ next passes control downstream. Can add in/out legs.                 │
 * │ Run(fn)      │ TERMINAL — adds a component with no next delegate. Pipeline ends     │
 * │              │ here; any middleware registered after Run() is NEVER reached.        │
 * │ Map(path,br) │ Forks pipeline when request path starts with 'path'. Control goes   │
 * │              │ into the branch; it does NOT rejoin the main pipeline afterwards.    │
 * │ MapWhen(p,br)│ Forks based on any HttpContext condition (not just path).            │
 * │              │ The branch does NOT rejoin the main pipeline.                        │
 * │ UseWhen(p,br)│ Adds conditional middleware that DOES rejoin the main pipeline after │
 * │              │ the branch — unlike Map/MapWhen.                                     │
 * └──────────────┴──────────────────────────────────────────────────────────────────────┘
 *
 * NOTE on short-circuiting with Run():
 *   When a middleware calls Run(), no further middleware in that scope is executed.
 *   Upstream middleware STILL runs its out leg (code after its own await next()).
 *   Only DOWNSTREAM middleware (not yet reached) is skipped.
 *
 * Compile note — two Use() overloads exist in .NET 8:
 *   Use(Func<HttpContext, RequestDelegate, Task>)   ← preferred; pass context to next
 *   Use(Func<HttpContext, Func<Task>, Task>)        ← legacy; next() with no args
 *   Always use the first form to avoid the deprecated overload warning.
 */

// --- Use() inline demo: logs every request with in/out legs ---
app.Use(async (HttpContext context, RequestDelegate next) =>
{
    Console.WriteLine($"[Pipeline] → {context.Request.Method} {context.Request.Path}"); // in leg
    await next(context);                                                                  // pass downstream
    Console.WriteLine($"[Pipeline] ← {context.Response.StatusCode}");                    // out leg
});

// --- Map() demo: branch the pipeline for the /api prefix ---
// Requests starting with /api enter this branch; the main pipeline is skipped for them.
app.Map("/api", (IApplicationBuilder apiApp) =>
{
    apiApp.Run(async (HttpContext ctx) =>                       // terminal inside the branch
    {
        await ctx.Response.WriteAsync("API branch — Map() demo"); // branch handles the response
    });
});

// --- MapWhen() demo: branch on a custom request header ---
app.MapWhen(
    predicate:     ctx => ctx.Request.Headers.ContainsKey("X-Demo-Branch"), // any bool condition
    configuration: branch =>
    {
        branch.Run(async (HttpContext ctx) =>
        {
            await ctx.Response.WriteAsync("MapWhen: X-Demo-Branch header detected");
        });
    }
);

// --- UseWhen() demo: conditional logic that rejoins the main pipeline ---
// Unlike Map/MapWhen, control returns to the main pipeline after the branch finishes.
app.UseWhen(
    predicate:     ctx => ctx.Request.Path.StartsWithSegments("/conditional"),
    configuration: branch =>
    {
        branch.Use(async (HttpContext ctx, RequestDelegate next) =>
        {
            Console.WriteLine("[UseWhen] branch ran — will rejoin main pipeline");
            await next(ctx); // rejoins the main pipeline after this component
        });
    }
);

/*
 * SECTION 2: BUILT-IN MIDDLEWARE AND ORDER
 *
 * ASP.NET Core ships many middleware components. Their ORDER in the pipeline is critical:
 *   • UseExceptionHandler must be FIRST — it can only catch errors thrown DOWNSTREAM.
 *   • UseStaticFiles should be EARLY — static-file hits short-circuit, saving auth overhead.
 *   • UseRouting must come BEFORE UseAuthentication/UseAuthorization so those components
 *     can inspect the matched endpoint's metadata (e.g. [Authorize] attribute).
 *   • UseAuthentication must come BEFORE UseAuthorization — auth populates HttpContext.User;
 *     authorization then reads it.
 *
 * Recommended order:
 *   Position │ Middleware                  │ Purpose
 *   ─────────┼─────────────────────────────┼────────────────────────────────────────
 *      1      │ UseExceptionHandler        │ Catch all unhandled exceptions
 *             │ UseDeveloperExceptionPage  │ Dev-only: rich stack-trace page
 *      2      │ UseHsts                    │ Strict-Transport-Security header
 *      3      │ UseHttpsRedirection        │ Redirect HTTP → HTTPS
 *      4      │ UseStaticFiles             │ Serve wwwroot files early; short-circuits
 *      5      │ UseRouting                 │ Match request path to an endpoint
 *      6      │ UseCors                    │ CORS policy (before auth)
 *      7      │ UseAuthentication          │ Populate HttpContext.User
 *      8      │ UseAuthorization           │ Enforce [Authorize] / policy
 *      9      │ Custom middleware           │ Cross-cutting business concerns
 *     10      │ MapGet / endpoints         │ Terminal endpoint handlers
 *   ─────────┴─────────────────────────────┴────────────────────────────────────────
 *
 * PREVIEW → 10. Exception Handling: UseExceptionHandler, ProblemDetails, IExceptionHandler.
 * PREVIEW → 11. Static Files & Request Pipeline: UseStaticFiles, UseDirectoryBrowser,
 *               UseDefaultFiles, PhysicalFileProvider.
 */
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // rich HTML error page with stack trace — dev only
                                     // COVERED IN DETAIL LATER → 10. Exception Handling
}
else
{
    app.UseExceptionHandler("/error"); // catch & redirect on unhandled exception — prod
                                       // COVERED IN DETAIL LATER → 10. Exception Handling
    app.UseHsts();                     // adds Strict-Transport-Security header (HTTPS)
}

app.UseHttpsRedirection();  // redirect HTTP → HTTPS
app.UseStaticFiles();       // serve files from wwwroot; short-circuits on hit
                            // COVERED IN DETAIL LATER → 11. Static Files & Request Pipeline

app.UseRouting();           // match the incoming path to a registered endpoint descriptor

/*
 * SECTION 3: CUSTOM MIDDLEWARE REGISTRATION AND FULL PIPELINE WIRING
 *
 * Custom middleware registered here (after UseRouting, before UseAuthorization) so it
 * can read endpoint metadata set by routing and runs before authorization decisions.
 *
 * Open the following files to understand how each piece is implemented:
 *   Middleware/RequestLoggingMiddleware.cs — IMiddleware class, HttpContext access,
 *                                            short-circuiting, in/out legs
 *   Middleware/TimingMiddleware.cs         — convention-based approach, elapsed timing
 *   Extensions/MiddlewareExtensions.cs    — UseRequestLogging / UseRequestTiming wrappers
 *
 * Calling app.UseRequestTiming() and app.UseRequestLogging() (instead of the raw
 * app.UseMiddleware<T>() form) is the recommended pattern — it hides implementation
 * details and keeps Program.cs readable.
 */
app.UseRequestTiming();     // convention-based — see Middleware/TimingMiddleware.cs
app.UseRequestLogging();    // IMiddleware-based — see Middleware/RequestLoggingMiddleware.cs

app.UseAuthentication();    // populate HttpContext.User from auth tokens/cookies
app.UseAuthorization();     // enforce [Authorize] attributes and policies

// Terminal endpoint middleware — reached only after the full pipeline runs
app.MapGet("/", () => "Hello, Middleware Pipeline!");
app.MapGet("/ping", () => "pong");
app.MapGet("/error", () => Results.Problem("A handled server error occurred."));

app.Run(); // start the Kestrel web server and begin processing requests

/*
 * QUICK REFERENCE — Middleware Pipeline
 * ──────────────────────────────────────────────────────────────────────────────
 * Registration  │ Has next? │ Branches? │ Rejoins? │ Use for
 * ──────────────┼───────────┼───────────┼──────────┼────────────────────────────
 * Use(fn)       │ yes       │ no        │ n/a      │ in/out cross-cutting logic
 * Run(fn)       │ no        │ no        │ n/a      │ terminal handler, short-circuit
 * Map(path,br)  │ yes       │ yes       │ no       │ path-prefix routing / API branches
 * MapWhen(p,br) │ yes       │ yes       │ no       │ condition-based branching
 * UseWhen(p,br) │ yes       │ yes       │ YES      │ conditional middleware (rejoins)
 *
 * Custom middleware styles:
 *   IMiddleware        — implements interface; resolved from DI per request;
 *                        must be registered: builder.Services.AddTransient<T>()
 *   Convention-based   — ctor takes RequestDelegate; InvokeAsync(HttpContext);
 *                        no DI registration; effectively singleton lifetime
 *   Inline (Use/Run)   — anonymous delegate; fine for one-liners; avoid complex logic
 *
 * HttpContext key members:
 *   .Request            — Method, Path, QueryString, Headers, Body, ContentType
 *   .Response           — StatusCode, Headers, Body, HasStarted, WriteAsync()
 *   .Connection         — RemoteIpAddress, LocalPort, ClientCertificate
 *   .User               — ClaimsPrincipal (populated after UseAuthentication)
 *   .Items              — per-request dictionary; share data between middleware
 *   .RequestServices    — scoped DI container for this request
 *
 * Short-circuit pattern:
 *   context.Response.StatusCode  = 403;
 *   context.Response.ContentType = "text/plain";
 *   await context.Response.WriteAsync("Forbidden");
 *   return; // do NOT call next
 *
 * Correct built-in order (simplified):
 *   ExceptionHandler → Hsts → HttpsRedirection → StaticFiles → Routing →
 *   Cors → Authentication → Authorization → Custom → Endpoints
 * ──────────────────────────────────────────────────────────────────────────────
 */
