using Microsoft.AspNetCore.Mvc;
using DataAnnotationsValidation.Models;

namespace DataAnnotationsValidation.Controllers;

/*
 * FILE ROLE: Server-side validation flow — ModelState.IsValid check, returning the view
 *            on failure (with errors), and Post-Redirect-Get on success.
 * SECTIONS IN THIS FILE:
 *   5a. GET action          — render the empty form
 *   5b. POST action         — ModelState.IsValid, return View on failure, PRG redirect
 *   5c. ModelState deep dive — error inspection, manual errors, clearing state
 */

/*
 * SECTION 5: ACCOUNTCONTROLLER — SERVER-SIDE VALIDATION FLOW
 *
 * THE FULL PIPELINE ON A POST (automatic, before your action body runs):
 *   1. Route matching → AccountController.Register (POST)
 *   2. Model binder reads form fields → creates RegistrationViewModel instance
 *   3. MVC runs every DataAnnotation attribute on every property
 *   4. MVC calls IValidatableObject.Validate() if all [Required] attributes passed
 *   5. All errors are stored in ModelState (a Dictionary<string, ModelStateEntry>)
 *   6. Your action method body runs — at this point ModelState.IsValid is set
 *
 * KEY DESIGN PRINCIPLES:
 *   a) NEVER trust client-supplied data — always check ModelState.IsValid on POST
 *      even when client-side validation (jQuery Validate) is active.  JavaScript
 *      can be disabled or bypassed.
 *   b) On failure: return View(model) — re-renders the same form, preserving the
 *      user's input values AND making ModelState errors available to Tag Helpers.
 *   c) On success: ALWAYS redirect (Post-Redirect-Get / PRG pattern).  A direct
 *      return View() after a successful POST allows the browser to re-POST the
 *      same form data on F5 / Refresh.
 *
 * POST-REDIRECT-GET (PRG) PATTERN:
 *   POST /Account/Register → validate → redirect → GET /Account/RegisterConfirm
 *   The GET response cannot be accidentally re-submitted by the browser.
 */
public sealed class AccountController : Controller
{
    // --- 5a. GET action — render the empty registration form ------------------
    /*
     * The GET action returns an empty View with no model.  The Razor view creates
     * a form whose asp-action="Register" attribute generates an action="/Account/Register"
     * and method="post" on the <form> element.
     *
     * The [HttpGet] attribute is optional here (GET is the default for actions with
     * no HTTP verb attribute) but it is good practice to be explicit, especially
     * when a POST overload of the same name exists.
     */
    [HttpGet]
    public IActionResult Register() => View();  // renders Views/Account/Register.cshtml

    // --- 5b. POST action — validate, return errors, or redirect ---------------
    /*
     * [ValidateAntiForgeryToken] — cross-site request forgery protection.
     *   When asp-action is used on a <form>, MVC automatically injects a hidden
     *   __RequestVerificationToken field.  This attribute validates it matches the
     *   cookie token, rejecting forged cross-origin requests.
     *   ALWAYS add [ValidateAntiForgeryToken] to every POST action that mutates state.
     *
     * ModelState.IsValid:
     *   - true  → all attributes passed, IValidatableObject.Validate() passed
     *   - false → at least one validation rule failed
     *
     * return View(model) on failure:
     *   - Re-renders Register.cshtml with the SAME model instance
     *   - The user's typed values are preserved in the form (model binding filled them)
     *   - Tag Helpers (asp-validation-for, asp-validation-summary) read ModelState
     *     and render error messages next to the offending fields
     *
     * IMPORTANT: Do NOT redirect on failure.  Redirecting loses ModelState
     * (it is a per-request object), so error messages would not appear.
     * Use TempData["errors"] = ... only when you must redirect and preserve errors.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegistrationViewModel model)
    {
        if (!ModelState.IsValid)            // at least one validation rule failed
            return View(model);             // re-render form; user's input + errors stay

        // All validation passed — process the registration here:
        //   - Save to database (EF Core, ADO.NET, etc.)
        //   - Send confirmation email
        //   - Sign the user in (SignInManager, cookie auth)
        //   Then redirect (PRG pattern)

        return RedirectToAction(nameof(RegisterConfirm));  // PRG: GET after POST
    }

    // --- 5c. Success confirmation page ----------------------------------------
    public IActionResult RegisterConfirm() => View();  // renders RegisterConfirm.cshtml
}

/*
 * SECTION 5c: ModelState DEEP DIVE
 *
 * ModelState is of type ModelStateDictionary — a dictionary keyed by property name.
 *
 * KEY MEMBERS:
 *   ModelState.IsValid
 *       — true when Count == 0 errors across all fields
 *
 *   ModelState["Email"]?.Errors
 *       — IList<ModelError> for the Email field; each has an .ErrorMessage string
 *
 *   ModelState.AddModelError("Email", "That email is already registered.")
 *       — manually add an error (e.g., after a DB uniqueness check in the action)
 *       — use string.Empty as the key for a model-level error (not field-specific):
 *           ModelState.AddModelError(string.Empty, "Registration is closed.");
 *
 *   ModelState.Remove("PhoneNumber")
 *       — remove errors for a specific field (rarely needed)
 *
 *   ModelState.Clear()
 *       — remove ALL errors and model state (use carefully; usually not needed)
 *
 * MANUAL ERROR PATTERN (after DataAnnotations pass but business rules fail):
 *
 *   [HttpPost]
 *   [ValidateAntiForgeryToken]
 *   public IActionResult Register(RegistrationViewModel model)
 *   {
 *       if (!ModelState.IsValid)
 *           return View(model);
 *
 *       // Business logic check after DataAnnotations pass
 *       if (await _userService.EmailExistsAsync(model.Email!))
 *       {
 *           ModelState.AddModelError(nameof(model.Email), "Email is already registered.");
 *           return View(model);
 *       }
 *
 *       // Save...
 *       return RedirectToAction(nameof(RegisterConfirm));
 *   }
 *
 * [ValidateNever] — SKIP VALIDATION FOR A PROPERTY (see Models/ProductViewModel.cs §2g):
 *   Applied to model properties populated server-side (e.g., dropdown lists loaded
 *   from the database).  The property IS still model-bound, just never validated.
 *   Namespace: Microsoft.AspNetCore.Mvc.ModelBinding.Validation
 *
 * [BindNever] — SKIP MODEL BINDING FOR A PROPERTY:
 *   The property is never populated from the request (neither binding NOR validation).
 *   Use for security-sensitive properties that must never come from user input.
 *   Namespace: Microsoft.AspNetCore.Mvc.ModelBinding
 */
