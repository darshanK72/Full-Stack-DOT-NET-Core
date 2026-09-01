/*
 * TOPIC: Views & Razor Syntax
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   Razor is the templating engine that merges C# logic with HTML markup inside
 *   .cshtml files. Every ASP.NET Core MVC response that returns HTML passes
 *   through Razor. Understanding how the parser switches between HTML and C#,
 *   which directives configure the file, how data reaches the view, and how the
 *   framework discovers the right .cshtml file are essential skills for any
 *   .NET web developer.
 *
 * WHAT YOU WILL LEARN:
 *   1.  .cshtml file overview — what Razor files are and where they live
 *   2.  Razor parsing rules — @ transitions, HTML vs C# mode
 *   3.  Razor directives — @model, @using, @inject, @functions, @inherits,
 *       @implements, @addTagHelper, @removeTagHelper, @namespace
 *   4.  Code blocks — @{ }
 *   5.  Inline expressions — @variable, @(expression)
 *   6.  Control flow — @if/@else, @switch, @for, @foreach, @while
 *   7.  HTML encoding — automatic vs @Html.Raw
 *   8.  Comments — @* *@
 *   9.  @Html helper methods — DisplayFor, EditorFor, ActionLink, BeginForm,
 *       ValidationMessageFor
 *  10.  View discovery algorithm
 *  11.  _ViewStart.cshtml and _ViewImports.cshtml
 *  12.  Passing data — ViewBag vs ViewData vs @model (PREVIEW → 12)
 *
 * CHAPTER MAP:
 *   1.  ViewModel type               → Models/ArticleViewModel.cs
 *   2.  Data-passing & controller    → Controllers/DemoController.cs
 *   3.  _ViewImports — global setup  → Views/_ViewImports.cshtml
 *   4.  _ViewStart — default layout  → Views/_ViewStart.cshtml
 *   5.  Layout shell                 → Views/Shared/_Layout.cshtml
 *   6.  Index — expressions/blocks   → Views/Demo/Index.cshtml
 *   7.  Control — flow directives    → Views/Demo/Control.cshtml
 *   8.  Helpers — Html helpers       → Views/Demo/Helpers.cshtml
 *   9.  Directives — all directives  → Views/Demo/Directives.cshtml
 *  10.  DI + startup wiring          → Program.cs (below)
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ViewsRazorSyntax;

/*
 * SECTION 1: MINIMAL WEB APPLICATION HOST
 * ─────────────────────────────────────────────────────────────────────────────
 * WebApplication.CreateBuilder sets up configuration, logging, and the DI
 * container. For this chapter the only service registered is MVC with Views.
 *
 * AddControllersWithViews() registers:
 *   • IControllerFactory, IActionInvokerFactory, IViewEngine (Razor)
 *   • Tag Helper infrastructure
 *   • All built-in @Html helper services
 *
 * Contrast with AddControllers() (API only — no view services) and
 * AddRazorPages() (Razor Pages — no MVC controllers).
 *
 * VIEW DISCOVERY (how MVC finds the .cshtml to render):
 *   When a controller returns View() with no explicit name the framework
 *   searches these paths in order until a file is found:
 *
 *   1. /Views/{ControllerName}/{ActionName}.cshtml   ← conventional path
 *   2. /Views/Shared/{ActionName}.cshtml             ← shared fallback
 *
 *   If View("Name") is used, the search tries:
 *   1. /Views/{ControllerName}/Name.cshtml
 *   2. /Views/Shared/Name.cshtml
 *   3. Absolute path if the name starts with / or ~/
 *
 *   The algorithm is implemented by RazorViewEngine. The paths are controlled
 *   by RazorViewEngineOptions.ViewLocationFormats which can be customised in
 *   AddControllersWithViews(options => ...).
 *
 * _ViewImports.cshtml & _ViewStart.cshtml:
 *   These special files sit in Views/ (or any sub-folder) and are applied
 *   automatically by the Razor engine before rendering any .cshtml in the
 *   same folder or deeper. They are NOT routed themselves.
 *
 *   _ViewImports.cshtml → global @using and @addTagHelper declarations
 *   _ViewStart.cshtml   → sets Layout = "_Layout" (or null to suppress)
 *
 *   See Views/_ViewImports.cshtml and Views/_ViewStart.cshtml.
 */
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args); // sets up host config

        /* SECTION 1a: SERVICE REGISTRATION
         * AddControllersWithViews wires Razor engine + MVC conventions. */
        builder.Services.AddControllersWithViews(); // MVC + Razor view services

        var app = builder.Build();

        /* SECTION 1b: MIDDLEWARE PIPELINE
         * Static files must come before routing so CSS/JS are served without
         * hitting a controller. UseRouting + UseEndpoints / MapControllerRoute
         * wire the conventional {controller}/{action}/{id?} route. */
        app.UseStaticFiles();                       // serves wwwroot content
        app.UseRouting();                           // matches URL to a route

        /* SECTION 1c: DEFAULT ROUTE
         * The conventional route template maps:
         *   /           → Demo/Index
         *   /Demo       → Demo/Index
         *   /Demo/Control → Demo/Control
         * "Demo" is the controller name without the "Controller" suffix.
         * The default action is Index.
         *
         * Route template tokens:
         *   {controller=Demo}  — controller segment, default "Demo"
         *   {action=Index}     — action segment, default "Index"
         *   {id?}              — optional id segment
         */
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Demo}/{action=Index}/{id?}"); // conventional MVC route

        app.Run();
    }
}

/*
 * QUICK REFERENCE — Views & Razor Syntax
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * RAZOR TRANSITIONS:
 *   @variable             — inline expression (HTML-encoded)
 *   @(complex expr)       — parenthesised expression (HTML-encoded)
 *   @{ ... }              — code block (no output; sets variables, calls methods)
 *   @Html.Raw(str)        — outputs string without encoding (XSS risk!)
 *   @* comment *@         — Razor comment (stripped; not in HTML output)
 *
 * DIRECTIVES (configuration, not output):
 *   @model TypeName           declare the view's strongly-typed model
 *   @using Namespace          import a namespace for this file
 *   @inject ServiceType Alias DI inject a service into a view
 *   @functions { }            declare C# methods/properties local to the view
 *   @inherits BaseViewPage<T> change the base class of the compiled view
 *   @implements IInterface    add an interface to the compiled view class
 *   @addTagHelper *, Assembly enable Tag Helpers from an assembly
 *   @removeTagHelper          disable a specific Tag Helper
 *   @namespace                set the CLR namespace of the compiled view
 *
 * CONTROL FLOW (all prefixed with @):
 *   @if (cond) { } @else if (cond) { } @else { }
 *   @switch (expr) { case X: break; default: break; }
 *   @for (int i = 0; i < n; i++) { }
 *   @foreach (var item in list) { }
 *   @while (cond) { }
 *
 * DATA PASSING — PREVIEW (detail → 12. TempData, ViewData & ViewBag):
 *   Mechanism     Type       Strongly typed   Lifetime
 *   @model        POCO       Yes              current request
 *   ViewData      dict       No (cast needed) current request
 *   ViewBag       dynamic    No               current request (same store as ViewData)
 *
 * VIEW DISCOVERY ORDER:
 *   1. /Views/{Controller}/{Action}.cshtml
 *   2. /Views/Shared/{Action}.cshtml
 *
 * HTML HELPERS vs TAG HELPERS:
 *   @Html.*      — C# method calls in Razor, older MVC style
 *   <tag asp-*>  — Tag Helper attributes, modern MVC style (→ 08. Tag Helpers)
 */
