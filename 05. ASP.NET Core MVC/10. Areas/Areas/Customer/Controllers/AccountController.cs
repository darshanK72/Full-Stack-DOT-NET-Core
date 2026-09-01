/*
 * FILE ROLE: Customer area account controller — demonstrates [Area("Customer")] and
 *   how multiple independent areas coexist in one application. The Customer area
 *   intentionally has NO area-specific layout, demonstrating layout fallback to
 *   the root Views/Shared/_Layout.cshtml.
 *
 * SECTIONS IN THIS FILE:
 *   8. Multiple Areas — [Area("Customer")] controller and layout fallback
 */

using Microsoft.AspNetCore.Mvc;

namespace Areas.Customer.Controllers;

/*
 * SECTION 8: MULTIPLE AREAS — [Area("Customer")] CONTROLLER
 * ─────────────────────────────────────────────────────────────────────────────
 * A single application supports any number of areas. Each area is a completely
 * independent slice with its own:
 *   · Controllers (each with their own [Area("...")] attribute)
 *   · View folders
 *   · Optional area-specific layouts
 *   · Optional area-specific _ViewImports.cshtml
 *
 * ADMIN vs. CUSTOMER AREA COMPARISON (this chapter):
 *
 *   Feature                    Admin area                Customer area
 *   ────────────────────────   ─────────────────────     ─────────────────────────
 *   Area attribute             [Area("Admin")]           [Area("Customer")]
 *   Route prefix               /Admin/...               /Customer/...
 *   Route default controller   Dashboard                Account
 *   Route default action       Index                    Profile
 *   Area-specific layout       ✓ (Views/Shared/_Layout) ✗ (falls back to root)
 *   Area _ViewImports.cshtml   ✓ (adds @using + tags)   ✓ (adds @using + tags)
 *   Typed ViewModel            AdminDashboardViewModel  none (view-only page)
 *
 * LAYOUT FALLBACK DEMONSTRATED BY THIS AREA:
 *   Areas/Customer/Views/_ViewStart.cshtml sets Layout = "_Layout".
 *   The engine searches:
 *     1. Areas/Customer/Views/Account/_Layout.cshtml    → not found
 *     2. Areas/Customer/Views/Shared/_Layout.cshtml     → not found
 *     3. Views/Shared/_Layout.cshtml                    ← found — root layout used
 *   The Customer area renders with the same chrome as the root site, which is a
 *   common pattern when a "customer portal" should look identical to the main site.
 *
 * NO CONTROLLER NAME COLLISION:
 *   Both areas could each define their own "AccountController" without conflict:
 *     [Area("Admin")]    class AccountController → /Admin/Account/...
 *     [Area("Customer")] class AccountController → /Customer/Account/...
 *   The area token in the route distinguishes them completely.
 *
 * CROSS-AREA NAVIGATION FROM CONTROLLERS:
 *   From Customer area, linking to Admin area:
 *     return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
 *
 *   From Customer area, linking to non-area root:
 *     return RedirectToAction("Index", "Home", new { area = "" });
 *   Both require explicitly setting the area route value — the ambient area
 *   value from the current request does NOT carry over automatically.
 */
[Area("Customer")] // links this controller to the "Customer" area route
public class AccountController : Controller
{
    // GET /Customer/Account/Profile  (or /Customer or /Customer/Account — defaults apply)
    public IActionResult Profile()
    {
        return View(); // resolves to Areas/Customer/Views/Account/Profile.cshtml
    }
}
