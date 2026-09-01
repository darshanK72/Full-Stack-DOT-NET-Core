/*
 * TOPIC: Data Annotations & Validation
 *
 * WHY IT MATTERS:
 *   Web applications must validate user input both on the server (to prevent bad
 *   data from entering the database) and on the client (for UX feedback before a
 *   round-trip).  ASP.NET Core MVC's validation pipeline — powered by attributes
 *   in System.ComponentModel.DataAnnotations — lets you declare rules directly on
 *   ViewModel properties.  After model binding on a POST, the framework runs every
 *   attribute automatically and populates ModelState.  A single ModelState.IsValid
 *   check in the controller guards all business logic, and Tag Helpers in the Razor
 *   view render per-field error messages with zero extra code.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Core annotations: [Required], [StringLength], [MaxLength], [MinLength]
 *   2.  Format validators: [RegularExpression], [EmailAddress], [Phone], [Url]
 *   3.  Numeric/date ranges: [Range]
 *   4.  Cross-field equality: [Compare]
 *   5.  Display/scaffold: [DataType], [Display], [DisplayFormat], [ScaffoldColumn]
 *   6.  Credit card format: [CreditCard]
 *   7.  Skip validation: [ValidateNever]
 *   8.  Custom ValidationAttribute (inherit, override IsValid)
 *   9.  Cross-property rules: IValidatableObject
 *   10. Server-side flow: ModelState.IsValid, return View on failure, PRG redirect
 *   11. Error message customization: {0} field name, {1}/{2} range placeholders
 *   12. View: asp-validation-for, asp-validation-summary, Html.ValidationMessageFor
 *   13. Client-side unobtrusive validation (PREVIEW → 14. Client-Side Validation)
 *   14. Localizing validation messages (PREVIEW — ASP.NET Core Localization docs)
 *
 * CHAPTER MAP:
 *   1.  RegistrationViewModel — [Required], [StringLength], [MaxLength], [MinLength],
 *                               [EmailAddress], [Phone], [Url], [RegularExpression],
 *                               [DataType], [Compare], error message customization
 *                                                 → Models/RegistrationViewModel.cs
 *   2.  ProductViewModel     — [Range], [DataType], [Display], [DisplayFormat],
 *                               [ScaffoldColumn], [CreditCard], [ValidateNever]
 *                                                 → Models/ProductViewModel.cs
 *   3.  DateRangeModel       — IValidatableObject cross-property rules
 *                                                 → Models/DateRangeModel.cs
 *   4.  PastDateAttribute    — custom ValidationAttribute, override IsValid
 *                                                 → Validation/PastDateAttribute.cs
 *   5.  AccountController    — ModelState.IsValid, return View, PRG redirect
 *                                                 → Controllers/AccountController.cs
 *   6.  Register view        — asp-validation-for, asp-validation-summary,
 *                               Html.ValidationMessageFor, client-side PREVIEW
 *                                                 → Views/Account/Register.cshtml
 *   7.  App startup wiring   → Program.cs (this file, below)
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

/*
 * SECTION 7: APP STARTUP — MVC PIPELINE WIRING
 *
 * AddControllersWithViews registers:
 *   - MVC controllers and the action invoker
 *   - Razor view engine (compiles .cshtml at first request or build time)
 *   - Model binding pipeline (reads form fields → ViewModel properties)
 *   - DataAnnotations validation pipeline (runs attributes after binding)
 *   - Anti-forgery token services ([ValidateAntiForgeryToken])
 *
 * The validation pipeline is NOT a separate middleware — it runs inside the
 * action filter pipeline, after model binding but before the action method body
 * executes.  You get a fully populated ModelState by the time your action runs.
 *
 * UseStaticFiles() serves wwwroot content — needed at runtime for the
 * jquery.validate scripts that drive client-side validation (Chapter 14).
 */
var builder = WebApplication.CreateBuilder(args); // configure DI container + config sources
builder.Services.AddControllersWithViews();        // MVC + Razor + validation pipeline

var app = builder.Build();
app.UseStaticFiles();  // serve wwwroot (js/css) — required for client-side validation scripts
app.UseRouting();      // match URLs to controller/action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Register}/{id?}"); // default → Register form
app.Run();
