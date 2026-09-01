/*
 * FILE ROLE: Implements NavigationViewComponent — a self-contained UI unit
 *            that fetches its own data and renders its own view independently
 *            of the invoking page's model.
 * SECTIONS IN THIS FILE:
 *   1. What is a ViewComponent? (concept, discovery, invocation)
 *   2. NavigationViewComponent — InvokeAsync implementation
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LayoutsSectionsPartials.Models;

namespace LayoutsSectionsPartials.ViewComponents;

/*
 * SECTION 1: WHAT IS A VIEWCOMPONENT?
 *
 * A ViewComponent is a reusable, self-contained piece of UI that:
 *   - Has its own C# class (inherits ViewComponent)
 *   - Fetches or computes its own data in InvokeAsync (NOT tied to a controller)
 *   - Renders a dedicated Razor view
 *
 * Partial view vs. ViewComponent:
 * ┌───────────────────────────────┬─────────────────────────────────────────┐
 * │ Partial view                  │ ViewComponent                           │
 * ├───────────────────────────────┼─────────────────────────────────────────┤
 * │ Renders a .cshtml snippet     │ Renders a .cshtml snippet               │
 * │ Model passed from invoking    │ Fetches its own model (InvokeAsync)     │
 * │   view (or none)              │ — fully independent from the page       │
 * │ No DI constructor             │ DI-injected constructor supported       │
 * │ Cannot call services          │ Can call services (repo, API, cache…)   │
 * │ Good for pure presentation    │ Good for "mini-controller" patterns:    │
 * │   (render what you're given)  │   shopping cart, notification badge,    │
 * │                               │   dynamic navigation from a database    │
 * └───────────────────────────────┴─────────────────────────────────────────┘
 *
 * DISCOVERY:
 *   ASP.NET Core finds ViewComponents by convention or attribute:
 *   - Class named <Name>ViewComponent  ← convention (used here)
 *   - Class decorated with [ViewComponent(Name = "Navigation")]
 *   - Class in a ViewComponents/ folder is NOT required; just the naming matters.
 *
 * VIEW RESOLUTION ORDER (relative to project root):
 *   1. Views/<Controller>/Components/Navigation/Default.cshtml  (controller-specific)
 *   2. Views/Shared/Components/Navigation/Default.cshtml        ← standard location
 *
 * INVOCATION FROM RAZOR:
 *   @await Component.InvokeAsync("Navigation")
 *   @await Component.InvokeAsync("Navigation", new { maxItems = 5 })
 *
 *   PREVIEW — tag-helper syntax (requires @addTagHelper in _ViewImports):
 *   <vc:navigation />
 *   <vc:navigation max-items="5" />
 *   The <vc:> prefix is covered in → 08. Tag Helpers
 *
 * SECTION 2: NavigationViewComponent IMPLEMENTATION
 *
 * InvokeAsync:
 *   - Must return Task<IViewComponentResult>
 *   - Call View(model) to render the default view (Default.cshtml)
 *   - Call View("Custom", model) to render a non-default view
 *   - Parameters defined on InvokeAsync map to the anonymous object passed
 *     in the invocation: new { maxItems = 5 } → int maxItems parameter
 *
 * In a real app InvokeAsync would inject a service (e.g. INavRepository)
 * through the constructor and await a database call here.
 */
public sealed class NavigationViewComponent : ViewComponent
{
    // Constructor injection is supported — add services as constructor parameters.
    // Example: public NavigationViewComponent(INavRepository repo) { _repo = repo; }

    public Task<IViewComponentResult> InvokeAsync()
    {
        // Hard-coded items for tutorial purposes.
        // A real implementation would: await _repo.GetNavItemsAsync()
        NavItem[] items = new[]
        {
            new NavItem { Label = "Home",    Href = "/",               IsActive = true  },
            new NavItem { Label = "About",   Href = "/page/about",     IsActive = false },
            new NavItem { Label = "Contact", Href = "/page/contact",   IsActive = false },
        };

        // View(model) → Default.cshtml with NavItem[] as @Model
        // View("Custom", model) → Custom.cshtml if you want a named variant
        return Task.FromResult<IViewComponentResult>(View(items));
    }
}
