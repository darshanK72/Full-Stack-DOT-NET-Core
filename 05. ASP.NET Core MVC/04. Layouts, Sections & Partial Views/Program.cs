/*
 * TOPIC: Layouts, Sections & Partial Views
 *
 * WHY IT MATTERS:
 *   Real ASP.NET Core MVC apps share a consistent outer shell — nav bar, <head>
 *   meta tags, footer — across every page.  Duplicating that HTML in every view
 *   is fragile and unmaintainable.  ASP.NET Core provides four layered mechanisms:
 *
 *     1. Layouts          — outer HTML shell with @RenderBody() slot for page content
 *     2. Sections         — named slots in the layout that content views can fill
 *     3. Partial views    — reusable HTML fragments, optionally model-bound
 *     4. ViewComponents   — self-contained units with their own data-fetch + view
 *
 * WHAT YOU WILL LEARN:
 *   1.  _Layout.cshtml structure — <!DOCTYPE>, <head>, <body>, @RenderBody()
 *   2.  Layout assignment — _ViewStart.cshtml hierarchy vs. per-view Layout property
 *   3.  @RenderSection() — required vs. optional; @section blocks in content views
 *   4.  Nested layouts — _AlternateLayout.cshtml that itself uses _Layout.cshtml
 *   5.  Layout = null — opting out of the layout in a single view
 *   6.  Partial views — @Html.Partial, @Html.RenderPartial, @Html.PartialAsync,
 *       @Html.RenderPartialAsync (differences + when to use each)
 *   7.  Partial views with @model — passing a strongly-typed model
 *   8.  Passing ViewData to partial views
 *   9.  ViewComponents — ViewComponent base class, InvokeAsync,
 *       @await Component.InvokeAsync ("Navigation")
 *       PREVIEW: <vc:navigation /> tag-helper syntax → 08. Tag Helpers
 *
 * CHAPTER MAP:
 *    1. Layout structure + RenderBody + RenderSection
 *                                  → Views/Shared/_Layout.cshtml
 *    2. _ViewStart layout assignment + per-folder override
 *                                  → Views/_ViewStart.cshtml
 *    3. _ViewImports (usings, tag-helper registration)
 *                                  → Views/_ViewImports.cshtml
 *    4. Nested / alternate layout  → Views/Shared/_AlternateLayout.cshtml
 *    5. Content view — sections + partial helpers + ViewComponent invocation
 *                                  → Views/Page/Index.cshtml
 *    6. Layout = null              → Views/Page/NoLayout.cshtml
 *    7. NavItem model              → Models/NavItem.cs
 *    8. PageViewModel              → Models/PageViewModel.cs
 *    9. Partial view with @model   → Views/Shared/_NavigationPartial.cshtml
 *   10. ViewComponent logic        → ViewComponents/NavigationViewComponent.cs
 *   11. ViewComponent view         → Views/Shared/Components/Navigation/Default.cshtml
 *   12. Controller (wiring)        → Controllers/PageController.cs
 *   13. Startup / DI wiring        → Program.cs (below)
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

/*
 * AddControllersWithViews registers:
 *   - IControllerFactory, IActionInvokerFactory, etc. (MVC pipeline)
 *   - Razor view engine (IRazorViewEngine, IViewComponentHelper, IHtmlHelper)
 *   - ViewComponent discovery (scans assemblies for *ViewComponent classes)
 *   - Tag helper activation
 */
builder.Services.AddControllersWithViews();     // MVC + Razor view engine

var app = builder.Build();

app.UseStaticFiles();       // serve wwwroot/ (CSS, JS, images)
app.UseRouting();
app.MapDefaultControllerRoute();                // GET / → Page/Index
app.Run();

/*
 * QUICK REFERENCE — Layouts, Sections & Partial Views
 * ─────────────────────────────────────────────────────────────────────
 * Layout
 *   _ViewStart.cshtml   Layout = "_Layout"          sets default layout
 *   per-view            @{ Layout = "~/Views/Shared/_Alt.cshtml"; }
 *   opt out             @{ Layout = null; }
 *
 * Layout file markers
 *   @RenderBody()                          required; exactly one per layout
 *   @RenderSection("name")                 required section
 *   @RenderSection("name", required:false) optional section
 *   @if (IsSectionDefined("name")) { ... } test before rendering
 *
 * Content view sections
 *   @section Scripts { <script>…</script> }
 *
 * Partial views
 *   @await Html.PartialAsync("_Nav")              PREFERRED (async, returns IHtmlContent)
 *   @{ await Html.RenderPartialAsync("_Nav"); }   async, writes directly to response
 *   @Html.Partial("_Nav")                         OBSOLETE — sync, CS0618 warning
 *   @{ Html.RenderPartial("_Nav"); }              OBSOLETE — sync, CS0618 warning
 *
 * Partial views with model
 *   @await Html.PartialAsync("_Nav", Model.NavItems)
 *
 * Partial views with custom ViewData
 *   ViewData keys set by the controller are automatically available in partials.
 *   To add extra keys, set them in the view before calling PartialAsync.
 *
 * ViewComponent
 *   @await Component.InvokeAsync("Navigation")
 *   @await Component.InvokeAsync("Navigation", new { maxItems = 5 })
 *   PREVIEW: <vc:navigation /> tag helper → 08. Tag Helpers
 * ─────────────────────────────────────────────────────────────────────
 */
