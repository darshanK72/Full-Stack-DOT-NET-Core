/*
 * FILE ROLE: ProductsController implements the full Post-Redirect-Get (PRG) pattern
 *            using TempData, demonstrates the [TempData] attribute on controller
 *            properties, and covers Peek / Keep semantics.
 * SECTIONS IN THIS FILE:
 *   SECTION  9. TempData — ITempDataDictionary, [TempData] attribute
 *   SECTION 10. Peek and Keep — reading TempData without consuming
 *   SECTION 11. PRG pattern — Index GET, Create GET, Create POST
 */

using Microsoft.AspNetCore.Mvc;
using TempDataViewDataViewBag.Models;

namespace TempDataViewDataViewBag.Controllers;

/*
 * SECTION 9: TEMPDATA — ITempDataDictionary
 * ─────────────────────────────────────────────────────────────────────────────
 * TempData is a property on Controller of type ITempDataDictionary — an
 * IDictionary<string, object?> backed by an ITempDataProvider (cookie or session).
 *
 * LIFETIME — survives ONE redirect, then consumed on first read:
 *
 *   Request 1 (POST):  TempData["Msg"] = "Saved!"   → stored in cookie/session by provider
 *   Request 2 (GET):   var m = TempData["Msg"]       → "Saved!" returned; marked for deletion
 *   Request 3 (GET):   var m = TempData["Msg"]       → null (already consumed and deleted)
 *
 * WHEN TempData IS LOADED:
 *   The provider loads TempData from the cookie/session at the start of each request,
 *   and saves (or clears) it at the end of the request when the response is committed.
 *   Entries that were READ during the request are removed from the store.
 *   Entries that were only SET (not read) are preserved for the next request.
 *
 * SERIALISATION:
 *   Only JSON-primitives survive a provider round-trip natively:
 *     string, int, long, bool, double, float, decimal, Guid, DateTime
 *   Class instances → must be JSON-serialised manually before storing.
 *   See Models/ProductFormModel.cs SECTION 4b.
 *
 * [TEMPDATA] ATTRIBUTE — bidirectional auto-sync between controller property and TempData:
 *
 *   [TempData]
 *   public string? StatusMessage { get; set; }
 *
 *   The TempDataDictionaryControllerPropertyActivator filter runs:
 *   ┌──────────────────────────────────────────────────────────────────────────┐
 *   │ Request start:                                                            │
 *   │   TempData["StatusMessage"] → loaded into StatusMessage property         │
 *   │   (the entry is marked for deletion — same read-once semantics)          │
 *   │ After action executes (before response written):                          │
 *   │   StatusMessage property value → saved back to TempData["StatusMessage"] │
 *   │   (only if the property was SET during the action)                       │
 *   └──────────────────────────────────────────────────────────────────────────┘
 *
 * Property name == TempData key (case-sensitive match: "StatusMessage").
 * Override the key: [TempData(Key = "CustomKey")]
 *
 * Manual equivalent (without attribute):
 *   // Read:  var msg = TempData["StatusMessage"] as string;
 *   // Write: TempData["StatusMessage"] = "Product saved.";
 */
public class ProductsController : Controller
{
    [TempData]   // auto-syncs StatusMessage ↔ TempData["StatusMessage"]
    public string? StatusMessage { get; set; }

    [TempData]   // auto-syncs ErrorMessage   ↔ TempData["ErrorMessage"]
    public string? ErrorMessage { get; set; }

    /*
     * SECTION 10: PEEK AND KEEP — READING TEMPDATA WITHOUT CONSUMING
     * ─────────────────────────────────────────────────────────────────────────
     * Default TempData read: marks the entry for deletion at end of request.
     *
     * PEEK — reads a value WITHOUT marking it for deletion:
     *   string? msg = TempData.Peek("StatusMessage") as string;
     *   // TempData["StatusMessage"] is still present — NOT consumed
     *
     * KEEP — after a normal read (which marked for deletion), un-marks it:
     *   string? msg = TempData["StatusMessage"] as string;  // consumed (marked)
     *   TempData.Keep("StatusMessage");                     // cancel deletion mark
     *   // TempData["StatusMessage"] will survive to the NEXT request again
     *
     * KEEP ALL — preserve every current TempData entry for another request:
     *   TempData.Keep();   // no argument — keeps all entries
     *
     * Typical use cases:
     *   Peek  → inspect a value in the action without consuming it (logging,
     *           conditional logic that should not affect the UI's read)
     *   Keep  → show a success banner on TWO pages instead of one
     *           (confirmation page → final summary page)
     *   Both  → an audit filter that reads TempData for logging but must
     *           not consume the values the view still needs
     *
     * The PeekKeepDemo action below demonstrates all three in sequence:
     */
    public IActionResult PeekKeepDemo()
    {
        TempData["Notice"] = "Demonstration value — set in PeekKeepDemo.";

        // PEEK: read without consuming — "Notice" stays in TempData after this line
        string? peeked = TempData.Peek("Notice") as string;   // "Demonstration value…"

        // READ: normal access — "Notice" is now CONSUMED (marked for deletion)
        string? read = TempData["Notice"] as string;           // "Demonstration value…"

        // KEEP: reverse the consumption — "Notice" will survive to the NEXT request
        TempData.Keep("Notice");                               // cancels the deletion mark

        // Use StatusMessage ([TempData] property) to carry the result to Index.
        // The peeked and read values are the same string; the important part is
        // that Peek did not consume, Read did consume, and Keep un-consumed it.
        StatusMessage = $"Peek read: '{peeked}'. Keep preserved 'Notice' for next request.";

        return RedirectToAction(nameof(Index)); // PRG: Index will display StatusMessage
    }

    /*
     * SECTION 11: PRG PATTERN — INDEX (GET)
     * ─────────────────────────────────────────────────────────────────────────
     * This action is the landing page AFTER a Create POST redirect.
     * TempData values set in Create(POST) survive one redirect and arrive here.
     *
     * The [TempData] attribute filter runs BEFORE the action body:
     *   TempData["StatusMessage"] → auto-loaded into StatusMessage property (consumed)
     *   TempData["ErrorMessage"]  → auto-loaded into ErrorMessage property (consumed)
     *
     * The view reads them via TempData["StatusMessage"] / TempData["ErrorMessage"]
     * (the provider has not yet committed the deletions — values are still readable
     * during the current request via the in-memory ITempDataDictionary).
     *
     * On the NEXT request (e.g., F5 refresh): both entries are gone — cleaned up.
     * This is why PRG prevents stale messages from reappearing on refresh.
     */
    public IActionResult Index()
    {
        ViewData["Title"] = "Products"; // layout reads @ViewData["Title"] for <title>
        return View();
        // StatusMessage and ErrorMessage are already in TempData (for view access)
        // because [TempData] attribute saved them back after loading
    }

    /*
     * SECTION 11 (cont.): PRG PATTERN — CREATE (GET)
     * ─────────────────────────────────────────────────────────────────────────
     * Shows the empty form. ViewBag provides supplementary per-request UI hints —
     * the primary form data goes through the typed @model ProductFormModel.
     *
     * ViewBag.FormTitle  → used by the view for the <h1> heading
     * ViewData["SubmitLabel"] → demonstrates that ViewData and ViewBag are
     *                          equivalent for this purpose; both carry the same
     *                          object? value into the same Razor dictionary.
     */
    public IActionResult Create()
    {
        ViewBag.FormTitle        = "Add New Product";     // supplementary UI string
        ViewData["SubmitLabel"]  = "Create Product";      // same via ViewData (same store)
        ViewData["Title"]        = "Create Product";
        return View(new ProductFormModel());               // typed model initialised empty
    }

    /*
     * SECTION 11 (cont.): PRG PATTERN — CREATE (POST)
     * ─────────────────────────────────────────────────────────────────────────
     * The PRG pattern solves the double-submit problem:
     *
     *   Browser:  POST /Products/Create  (user submits form)
     *   Server:   validate ProductFormModel via ModelState
     *             ├─ FAIL  → return View(model)             [no redirect — errors shown]
     *             └─ PASS  → set TempData → RedirectToAction("Index")  [302 Redirect]
     *   Browser:  GET  /Products/Index   (browser follows redirect)
     *   Server:   read TempData["StatusMessage"] → render success banner
     *
     * Without PRG: pressing F5/Refresh re-submits the POST → duplicate insert.
     * With PRG:    F5/Refresh on the GET /Index just re-fetches the page — safe.
     *
     * [ValidateAntiForgeryToken] pairs with the hidden __RequestVerificationToken
     * field that the <form asp-action="Create"> tag helper injects automatically.
     * It prevents CSRF — a forged cross-site form cannot fake the token.
     *
     * COVERED IN DETAIL:
     *   Model binding  → 06. Model Binding in MVC
     *   Validation     → 07. Data Annotations & Validation
     *   CSRF           → 11. Action Filters in MVC
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProductFormModel model)
    {
        if (!ModelState.IsValid)
        {
            // Validation failed — return the SAME view with the posted model so
            // tag helpers re-render field values and validation messages.
            // ViewBag lives for this request only; no redirect needed on failure.
            ViewBag.FormTitle       = "Add New Product";
            ViewData["SubmitLabel"] = "Create Product";
            ViewData["Title"]       = "Create Product";
            return View(model);   // NOT a redirect — PRG only on success
        }

        // Validation passed — set StatusMessage via [TempData] property.
        // The attribute filter will save this to TempData["StatusMessage"] when the
        // action result (RedirectToAction) is executed, persisting it through the redirect.
        StatusMessage = $"Product '{model.Name}' added — £{model.Price:F2}, qty {model.Quantity}.";

        // 302 Redirect: browser sends GET /Products/Index.
        // TempData is committed to the cookie/session before the response flushes.
        // The NEXT GET request reads TempData["StatusMessage"] → shows success banner.
        return RedirectToAction(nameof(Index)); // PRG redirect
    }
}
