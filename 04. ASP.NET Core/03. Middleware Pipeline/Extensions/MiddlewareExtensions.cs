/*
 * FILE ROLE: Encapsulates middleware registration behind UseXxx extension methods on
 *            IApplicationBuilder — the standard pattern used by every built-in ASP.NET
 *            Core middleware and the convention callers should expect.
 *
 * SECTIONS IN THIS FILE:
 *   1. Why UseXxx extension methods — the ASP.NET Core convention
 *   2. UseRequestLogging — extension for IMiddleware-based middleware
 *   3. UseRequestTiming  — extension for convention-based middleware
 */

using Microsoft.AspNetCore.Builder;
using MiddlewarePipeline.Middleware;

namespace MiddlewarePipeline.Extensions;

/*
 * SECTION 1: THE UseXxx CONVENTION — ENCAPSULATING MIDDLEWARE REGISTRATION
 *
 * All built-in ASP.NET Core middleware ships with a UseXxx extension method:
 *   app.UseRouting()          instead of app.UseMiddleware<EndpointRoutingMiddleware>()
 *   app.UseStaticFiles()      instead of app.UseMiddleware<StaticFileMiddleware>()
 *   app.UseAuthentication()   instead of app.UseMiddleware<AuthenticationMiddleware>()
 *
 * You should apply the same pattern to your custom middleware. Benefits:
 *
 *   • Encapsulation — callers do not need to know whether the middleware uses IMiddleware
 *     or convention-based style, or what its DI registration requirements are.
 *   • Single call site — Program.cs stays clean: app.UseRequestLogging() is one line.
 *   • Options support — the extension can accept a configuration delegate:
 *       app.UseRequestLogging(opts => opts.IncludeHeaders = true)
 *   • Discoverability — IDE auto-complete surfaces app.UseRequest... to team members.
 *
 * Pattern:
 *   public static IApplicationBuilder UseXxx(this IApplicationBuilder app)
 *       => app.UseMiddleware<XxxMiddleware>();
 *
 * IApplicationBuilder.UseMiddleware<T>() is the generic helper that:
 *   • For IMiddleware types  — resolves T from the DI container on each request
 *   • For convention-based  — instantiates T once (at pipeline build time),
 *                             passing the compiled RequestDelegate to the constructor
 *
 * Return IApplicationBuilder to allow method chaining:
 *   app.UseRequestTiming()
 *      .UseRequestLogging();
 */
public static class MiddlewareExtensions
{
    /*
     * SECTION 2: UseRequestLogging — EXTENSION FOR AN IMiddleware COMPONENT
     *
     * Calling UseMiddleware<RequestLoggingMiddleware>() works here because:
     *   1. RequestLoggingMiddleware implements IMiddleware
     *   2. It was registered with DI in Program.cs:
     *      builder.Services.AddTransient<RequestLoggingMiddleware>()
     *
     * On each request ASP.NET Core resolves a fresh RequestLoggingMiddleware from the DI
     * container, enabling constructor-injected services (ILogger, DbContext, etc.) to
     * honour their own lifetimes correctly.
     */
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>(); // resolved from DI per request
    }

    /*
     * SECTION 3: UseRequestTiming — EXTENSION FOR A CONVENTION-BASED COMPONENT
     *
     * Calling UseMiddleware<TimingMiddleware>() works here because:
     *   1. TimingMiddleware follows the convention (ctor with RequestDelegate + InvokeAsync)
     *   2. No DI registration is required — the framework instantiates it once at startup.
     *
     * If TimingMiddleware needed extra constructor arguments beyond RequestDelegate, pass
     * them as additional parameters:
     *   app.UseMiddleware<TimingMiddleware>(someArg, anotherArg)
     * The framework fills RequestDelegate automatically; extras are taken from the args array.
     */
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TimingMiddleware>(); // instantiated once at startup
    }
}
