/*
 * FILE ROLE: Shows the standard MVC GET/POST pattern for a validated form — including
 *            the critical server-side ModelState.IsValid fallback that protects against
 *            requests that bypass client-side validation (JS disabled, Postman, etc.).
 * SECTIONS IN THIS FILE:
 *   5. Controller GET/POST Pattern + Server Fallback
 *      5a. GET — populate and return empty model
 *      5b. POST — server-side ModelState check before processing
 *      5c. Why server fallback is non-negotiable
 */

using Microsoft.AspNetCore.Mvc;
using ClientSideValidation.Models;

namespace ClientSideValidation.Controllers;

/*
 * SECTION 5: CONTROLLER GET/POST PATTERN + SERVER FALLBACK
 *
 * Client-side validation is a UX convenience, NOT a security control.
 * Any caller that disables JavaScript or sends a raw HTTP POST bypasses it entirely.
 * The POST action must always validate ModelState before trusting the data.
 *
 * Standard pattern:
 *   GET  /Account/Register  → return empty model (Section 5a)
 *   POST /Account/Register  → validate → if invalid, return View(model)
 *                                      → if valid, process and redirect (Section 5b)
 *
 * [ValidateAntiForgeryToken] pairs with <form asp-action="Register"> which auto-generates
 * a hidden __RequestVerificationToken field. This prevents CSRF attacks and is separate
 * from validation — always include it on POST actions.
 */
public sealed class AccountController : Controller
{
    /*
     * --- 5a. GET — populate and return empty model ---
     *
     * Pass a new (empty) model instance to the view so asp-for Tag Helpers have a
     * model to bind to. Without this, @Model is null and any property access throws
     * a NullReferenceException when Razor renders asp-validation-for spans.
     */
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegistrationViewModel()); // empty model; view renders form fields
    }

    /*
     * --- 5b. POST — server-side ModelState check before processing ---
     *
     * ModelState is populated by the model binder BEFORE this method is called.
     * It aggregates errors from:
     *   a) DataAnnotation attributes on the model
     *   b) Custom IModelValidator implementations (e.g. FutureDateAttribute.IsValid)
     *   c) Any errors you add manually via ModelState.AddModelError()
     *
     * If ModelState is invalid:
     *   Return View(model) — this re-renders the form WITH the current field values
     *   and error messages. The Razor Tag Helpers read ModelState to populate
     *   asp-validation-for spans and asp-validation-summary.
     *
     * If ModelState is valid:
     *   Process the request (save to database, send email, etc.) then
     *   use PRG pattern: Post/Redirect/Get — redirect to prevent double-submit on F5.
     *
     * Pitfall: returning View() without the model argument loses all field values,
     * forcing the user to re-type everything after a server-side error.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegistrationViewModel model)
    {
        if (!ModelState.IsValid)    // server-side guard — always check even if client validates
            return View(model);     // re-render with errors; model retains the submitted values

        // At this point all DataAnnotations and custom validators passed on the server.
        // In a real app: hash the password, save the user to a database, send confirmation email.
        // For this tutorial we simply redirect to a success page.
        TempData["RegisteredUser"] = model.UserName; // pass the name to the success view
        return RedirectToAction(nameof(RegisterSuccess)); // PRG: redirect after POST
    }

    /*
     * --- 5c. RegisterSuccess — GET-only success page ---
     *
     * After a successful redirect from POST, the browser issues a GET request here.
     * TempData persists across ONE redirect and is then cleared automatically.
     */
    [HttpGet]
    public IActionResult RegisterSuccess()
    {
        return View(); // TempData["RegisteredUser"] is available in the view
    }
}
