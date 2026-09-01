/*
 * FILE ROLE: MVC controller that wires views for each tag helper category.
 *            Actions are intentionally thin — all teaching content lives in
 *            the views and the custom TagHelper classes.
 *
 * SECTIONS IN THIS FILE:
 *   1. DemoController — GET / POST actions for Forms, Navigation, Assets, Custom
 *   2. BuildCategoryList — SelectList construction with dataValueField / dataTextField
 *   3. BuildGroupedOptions — SelectListGroup for <optgroup> rendering
 */
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TagHelpers.Models;

namespace TagHelpers.Controllers;

/*
 * SECTION 1: DEMO CONTROLLER
 *
 * Route: GET  /Demo/Forms    → renders the form tag helper demo
 *        POST /Demo/Forms    → validates model; redirects on success (PRG pattern)
 *        GET  /Demo/Navigation → renders anchor tag helper demo
 *        GET  /Demo/Assets     → renders link/script/image/environment/cache demos
 *        GET  /Demo/Custom     → renders custom AlertTagHelper + GravatarTagHelper
 *
 * PRG Pattern (Post-Redirect-Get):
 *   After a successful POST, redirect instead of returning View(model).
 *   This prevents duplicate form submissions when the user refreshes the page.
 *   TempData survives one redirect; read it in the GET action to show a success banner.
 *
 * Anti-forgery:
 *   <form asp-action="Forms"> automatically injects a hidden __RequestVerificationToken.
 *   [ValidateAntiForgeryToken] on the POST action verifies it, preventing CSRF attacks.
 *   The anti-forgery services are registered by AddControllersWithViews() in Program.cs.
 *
 * SelectList repopulation on POST failure:
 *   SelectList / IEnumerable<SelectListItem> properties are NOT included in the posted form.
 *   If ModelState is invalid and we return View(model), we MUST repopulate Categories
 *   and GroupedOptions before the view renders — otherwise asp-items gets null and throws.
 */
public class DemoController : Controller
{
    // GET /Demo/Forms
    public IActionResult Forms()
    {
        ContactFormViewModel vm = BuildViewModel(); // includes dropdown data
        return View(vm);
    }

    // POST /Demo/Forms
    [HttpPost]
    [ValidateAntiForgeryToken] // validates the token injected by <form asp-action="Forms">
    public IActionResult Forms(ContactFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = BuildCategoryList(); // repopulate — not posted back
            model.GroupedOptions = BuildGroupedOptions(); // same
            return View(model); // return with validation errors shown by asp-validation-for
        }

        TempData["SuccessMessage"] = $"Message from {model.Name} received!";
        return RedirectToAction(nameof(Forms)); // PRG: redirect to GET to prevent re-post
    }

    // GET /Demo/Navigation
    public IActionResult Navigation() => View();

    // GET /Demo/Assets
    public IActionResult Assets() => View();

    // GET /Demo/Custom
    public IActionResult Custom() => View();

    /*
     * SECTION 2: SELECTLIST CONSTRUCTION
     *
     * SelectList(IEnumerable items, string dataValueField, string dataTextField):
     *   items          — any IEnumerable; each element is reflected for the named fields
     *   dataValueField — property name whose value becomes <option value="...">
     *   dataTextField  — property name whose value becomes the visible <option> text
     *
     * Anonymous types work fine here because SelectList uses reflection.
     * In production code, prefer a named DTO or enum-based list for testability.
     *
     * The resulting SelectList implements IEnumerable<SelectListItem>, which is the type
     * expected by the SelectTagHelper's Items property (asp-items attribute).
     */
    private static SelectList BuildCategoryList()
    {
        var categories = new[]
        {
            new { Id = 1, Name = "Technical Support" },
            new { Id = 2, Name = "Billing" },
            new { Id = 3, Name = "General Enquiry" }
        };
        return new SelectList(categories, "Id", "Name"); // projects Id→value, Name→text
    }

    /*
     * SECTION 3: SELECTLISTGROUP FOR <optgroup>
     *
     * Assigning SelectListItem.Group renders <optgroup label="GroupName"> in the HTML:
     *
     *   <select>
     *     <optgroup label="Technical">
     *       <option value="1">Support</option>
     *       <option value="2">Bug Report</option>
     *     </optgroup>
     *     <optgroup label="Business">
     *       <option value="3">Billing</option>
     *       <option value="4">Sales</option>
     *     </optgroup>
     *   </select>
     *
     * SelectListGroup is not a separate enum — it is a plain C# object that SelectTagHelper
     * uses as an equality key: items sharing the same SelectListGroup instance end up in
     * the same <optgroup>. Different instances with the same Name → different groups.
     */
    private static IEnumerable<SelectListItem> BuildGroupedOptions()
    {
        var technical = new SelectListGroup { Name = "Technical" }; // one group object
        var business = new SelectListGroup { Name = "Business" };   // another group object

        return new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Support",    Group = technical },
            new SelectListItem { Value = "2", Text = "Bug Report", Group = technical },
            new SelectListItem { Value = "3", Text = "Billing",    Group = business  },
            new SelectListItem { Value = "4", Text = "Sales",      Group = business  }
        };
    }

    // Builds a fully populated ViewModel for the Forms GET action
    private static ContactFormViewModel BuildViewModel() =>
        new ContactFormViewModel
        {
            Categories = BuildCategoryList(),       // flat dropdown
            GroupedOptions = BuildGroupedOptions()  // grouped dropdown with <optgroup>
        };
}
