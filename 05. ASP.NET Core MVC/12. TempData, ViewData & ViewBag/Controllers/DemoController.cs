/*
 * FILE ROLE: DemoController demonstrates ViewData (ViewDataDictionary), ViewBag (dynamic
 *            wrapper over the same dictionary), and the [ViewData] attribute on controller
 *            properties. No redirect is performed — all data is for the current request.
 * SECTIONS IN THIS FILE:
 *   SECTION 5. ViewData — ViewDataDictionary, string keys, boxing/unboxing, lifetime
 *   SECTION 6. ViewBag — dynamic wrapper, comparison table, shared store
 *   SECTION 7. [ViewData] attribute — controller property auto-synced to ViewData
 */

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace TempDataViewDataViewBag.Controllers;

/*
 * SECTION 7: [VIEWDATA] ATTRIBUTE — CONTROLLER PROPERTY AUTO-POPULATED INTO VIEWDATA
 * ─────────────────────────────────────────────────────────────────────────────
 * The [ViewData] attribute (ViewDataAttribute in Microsoft.AspNetCore.Mvc) decorates
 * a controller property. MVC's ViewDataDictionaryControllerPropertyActivator filter
 * runs before the action and AFTER the action result:
 *
 *   1. Before action:  ViewData["PageTitle"] → copied INTO PageTitle property
 *   2. After action:   PageTitle property value → copied BACK to ViewData["PageTitle"]
 *
 * The ViewData key is the property NAME by default (can be overridden via the
 * [ViewData(Key = "CustomKey")] constructor parameter).
 *
 * Effect in the action:
 *   Setting PageTitle = "Hello" is exactly equivalent to ViewData["PageTitle"] = "Hello".
 *   The view accesses ViewData["PageTitle"] (or @ViewBag.PageTitle) — it has no
 *   direct reference to the controller property.
 *
 * Common use case — base controller with layout title:
 *   public abstract class BaseController : Controller
 *   {
 *       [ViewData] public string? Title { get; set; }
 *   }
 *   // Every derived controller sets Title = "..."; the layout reads @ViewData["Title"]
 *
 * CAVEAT: Only string and primitive types flow cleanly. Complex types on [ViewData]
 *         properties are subject to the same object? cast requirement in the view as
 *         a manually written ViewData["Key"] entry.
 */
public class DemoController : Controller
{
    [ViewData]   // auto-syncs PageTitle ↔ ViewData["PageTitle"]
    public string? PageTitle { get; set; }

    /*
     * SECTION 5: VIEWDATA — ViewDataDictionary
     * ─────────────────────────────────────────────────────────────────────────
     * ViewData is a property on ControllerBase of type ViewDataDictionary, which
     * implements IDictionary<string, object?>. It is shared between the controller
     * action and the Razor view for the CURRENT REQUEST only — it does NOT survive
     * a redirect (a redirect creates a new HTTP request with a fresh ViewData).
     *
     * Key characteristics:
     *   - String keys (case-insensitive: "title" == "Title")
     *   - Values are object? — value types (int, bool, decimal) are BOXED on write
     *   - Must be cast or unboxed in the view before use
     *   - Null-safe reads: ViewData["Missing"] returns null (no KeyNotFoundException)
     *   - ViewData.Model is special: set automatically when the action returns View(model)
     *
     * BOXING pitfall (silent at compile time, costly or risky at runtime):
     *   ViewData["Count"] = 42;              // int 42 BOXED to object?
     *   var n = (int)ViewData["Count"]!;     // UNBOX — InvalidCastException if type wrong
     *   var n = ViewData["Count"] as int?;   // safe nullable unbox (returns null on mismatch)
     *
     * LIFETIME rule:
     *   ViewData["X"] = "value";
     *   return View();                       // value available in the view ✓
     *
     *   ViewData["X"] = "value";
     *   return RedirectToAction("Other");    // value LOST — redirect creates new request ✗
     *   // → use TempData for cross-redirect data (see ProductsController.cs)
     *
     * ViewData["Title"] is a convention recognised by the default MVC layout:
     *   _Layout.cshtml reads @ViewData["Title"] to set the <title> element.
     */
    public IActionResult ViewDataDemo()
    {
        // ── String values (no boxing — string is already object?) ──────────
        ViewData["DemoTitle"]   = "ViewData & ViewBag Demo";
        ViewData["Description"] = "Exploring ViewData and ViewBag access patterns.";

        // ── Value types (BOXING — int/bool/decimal stored as object?) ──────
        ViewData["ItemCount"] = 42;     // int boxed; view must cast: (int)ViewData["ItemCount"]!
        ViewData["IsPromo"]   = true;   // bool boxed

        // ── Reference type (List stored as object?) ─────────────────────────
        ViewData["Tags"] = new List<string> { "MVC", "ASP.NET Core", "Razor" };
        // view casts: ViewData["Tags"] as List<string>

        /*
         * SECTION 6: VIEWBAG — DYNAMIC WRAPPER OVER ViewDataDictionary
         * ───────────────────────────────────────────────────────────────────
         * ViewBag is a property on ControllerBase of type dynamic. Internally it is a
         * DynamicViewData instance that reads from and writes to the EXACT SAME
         * ViewDataDictionary that ViewData exposes.
         *
         * ViewData vs ViewBag — SAME UNDERLYING STORE:
         *
         *   Aspect               ViewData                     ViewBag
         *   ────────────────── ─────────────────────────────  ──────────────────────────────
         *   Declared type       ViewDataDictionary            dynamic (DynamicViewData)
         *   Key access syntax   ViewData["Key"]               ViewBag.Key
         *   Underlying store    IDictionary<string,object?>   SAME dictionary (shared!)
         *   Compile-time check  None                          None (dynamic — runtime only)
         *   IntelliSense        Property names as strings     None — dynamic member access
         *   Null access         null (no exception)           null (no exception)
         *   View syntax         @ViewData["Key"]              @ViewBag.Key
         *   Interchangeable?    YES — ViewData["X"] == ViewBag.X at all times
         *
         * Because they share the same store, the LAST WRITE WINS per key:
         *   ViewData["ItemCount"] = 42;    // sets ItemCount to 42
         *   ViewBag.ItemCount = 99;        // OVERWRITES — ViewData["ItemCount"] is now 99
         *   // After these two lines: ViewData["ItemCount"] == 99 == ViewBag.ItemCount
         *
         * NO COMPILE-TIME SAFETY:
         *   ViewBag.Typo = "hello";      // no error — creates a new dynamic member
         *   var x = ViewBag.Tpyo;        // typo! returns null silently — no build error
         *   Compare: product.Naem        // → CS1061 at compile time ← preferred with @model
         *
         * ViewBag PREFERRED over ViewData when:
         *   - Key is known at author time and the dot syntax reads more clearly
         *   - Passing select-list items: ViewBag.CategoryList = new SelectList(...)
         *   - Layout-level supplementary data (active nav item, theme setting)
         *
         * Both are DISCOURAGED for primary page data → use @model instead.
         */

        // ViewBag for supplementary, non-model data:
        ViewBag.Category  = "Electronics";         // ViewData["Category"] = "Electronics"
        ViewBag.Price     = 299.99m;               // decimal boxed; ViewData["Price"] = 299.99m
        ViewBag.ItemCount = 99;                    // OVERWRITES ViewData["ItemCount"] set above
        // ^ After this line: ViewData["ItemCount"] == 99 (not 42) — last write wins

        // ── [ViewData] attribute property (SECTION 7) ─────────────────────
        PageTitle = "ViewData Attribute Demo";
        // Equivalent to: ViewData["PageTitle"] = "ViewData Attribute Demo"
        // The view reads @ViewData["PageTitle"] or @ViewBag.PageTitle

        // Route to Views/Products/ViewDataDemo.cshtml with an explicit path.
        // By convention MVC looks in Views/{ControllerName}/ but this project's
        // ViewDataDemo view is in Views/Products/ to keep all demo views together.
        return View("~/Views/Products/ViewDataDemo.cshtml");
    }
}
