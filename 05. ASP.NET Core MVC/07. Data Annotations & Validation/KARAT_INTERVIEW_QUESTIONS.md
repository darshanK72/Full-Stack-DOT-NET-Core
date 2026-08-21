# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/07. Data Annotations & Validation`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Review this MVC registration flow. QA reports that disabling JavaScript in the browser still allows invalid accounts through in production.

```csharp
// RegisterViewModel.cs
public class RegisterViewModel
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}

// Views/Account/Register.cshtml (excerpt)
<form asp-action="Register" method="post">
    <input asp-for="Email" />
    <span asp-validation-for="Email"></span>
    <input asp-for="Password" type="password" />
    <button type="submit">Register</button>
</form>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}

// AccountController.cs
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    _users.Create(model.Email, model.Password);
    return RedirectToAction("Index", "Home");
}
```

`Program.cs` calls `AddControllersWithViews()`; `_ValidationScriptsPartial` is present on the GET view.

---

#### Q2. (R) Review this checkout ViewModel. Users can submit the form without accepting terms even though the UI shows a required checkbox.

```csharp
public class CheckoutViewModel
{
    [Required] public bool? AcceptTerms { get; set; }
    public string ShippingAddress { get; set; } = "";
}

// Razor excerpt
<input asp-for="AcceptTerms" type="checkbox" />
<label asp-for="AcceptTerms">I accept the terms</label>
```

POST body when checkbox is unchecked: `AcceptTerms=false` (or field omitted). Developer expected `[Required]` to block submission.

---

#### Q3. (R) Review password change form. Server accepts mismatched passwords; client-side shows an error only in Chrome.

```csharp
public class ChangePasswordViewModel
{
    [Required][DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [Required][Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";
}

[HttpPost]
public IActionResult ChangePassword(ChangePasswordViewModel model)
{
    if (!ModelState.IsValid) return View(model);
    _users.ChangePassword(UserId, model.NewPassword);
    return RedirectToAction("Profile");
}
```

Property names were refactored from `Password` → `NewPassword`; `_ValidationScriptsPartial` is loaded. Pen testers POST `{ "NewPassword": "x", "ConfirmPassword": "y" }` directly.

---

#### Q4. (P) A booking ViewModel needs `EndDate >= StartDate` and at least one guest name when `GuestCount > 0`. Compare putting this in `IValidatableObject.Validate` vs a custom `ValidationAttribute` on one property — what runs when in the MVC pipeline, and how do errors map to `asp-validation-for`?

```csharp
public class BookingViewModel : IValidatableObject
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int GuestCount { get; set; }
    public string? PrimaryGuestName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx) { /* ... */ }
}
```

---

#### Q5. (R) Review this custom attribute on a discount field. Validation passes for `-50` and `999999` in unit tests but fails unpredictably in the MVC form POST.

```csharp
public sealed class DiscountPercentAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is null) return ValidationResult.Success;
        var n = (int)value;
        if (n < 0 || n > 100) return new ValidationResult(ErrorMessage ?? "Invalid discount.");
        return ValidationResult.Success;
    }
}

public class ProductEditViewModel
{
    [DiscountPercent]
    public decimal DiscountPercent { get; set; }
}
```

Form POST sends `DiscountPercent=12.5` from a `<input type="number" step="0.01">`.

---

#### Q6. (D) A team annotates EF Core entities with `[Required]`, `[StringLength]`, and `[Range]` and passes the same entity type to MVC Razor views and to a minimal API layer. Review the architecture — what breaks, leaks, or becomes hard to test?

```csharp
public class Product  // EF entity + MVC model
{
    public int Id { get; set; }
    [Required][StringLength(100)] public string Name { get; set; } = "";
    [Range(0, 99999)] public decimal Price { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
```

---

#### Q7. (R) Review client vs server validation mismatch. Support tickets say "the form showed valid but server rejected with a date error."

```csharp
// ViewModel — server
public class EventViewModel
{
    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime EventDate { get; set; }
}

// wwwroot/js/site.js (legacy override)
$.validator.methods.date = function () { return true; }; // accept any string
```

Browser locale sends `31/12/2026`; server culture is `en-US`. `_ValidationScriptsPartial` is included.

---

#### Q8. (R) Review `[Remote]` username availability check. Production DB CPU spikes after marketing campaign; security flags possible enumeration.

```csharp
public class SignUpViewModel
{
    [Required]
    [Remote(action: "CheckUsername", controller: "Account")]
    public string Username { get; set; } = "";
}

// AccountController
[AcceptVerbs("GET", "POST")]
public IActionResult CheckUsername(string username)
{
    var taken = _db.Users.Any(u => u.Username == username);
    return Json(!taken);
}
```

Razor: `<input asp-for="Username" />` with unobtrusive validation scripts. No throttling on `CheckUsername`.

---

#### Q9. (P) An admin edit form binds a user ViewModel that includes a password hash for round-trip hidden fields. Model validation fails on POST with "The PasswordHash field is required." Explain `ValidateNever` — where to apply it, what still must be validated server-side, and what not to put in the form at all.

```csharp
public class EditUserViewModel
{
    public int Id { get; set; }
    [Required][EmailAddress] public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = ""; // populated on GET, round-tripped in hidden input
    [Required][MinLength(8)] public string? NewPassword { get; set; }
}
```

---

#### Q10. (D) Greenfield MVC app: team debates Data Annotations + unobtrusive client validation vs FluentValidation (`AddFluentValidationAutoValidation`). Compare maintainability, client-side parity, testing, and what happens on a direct POST with JavaScript disabled.

---

#### Q11. (M) Walk through the server-side validation order for an MVC POST to `[HttpPost] Create(ProductCreateViewModel model)` when the action has `[ValidateAntiForgeryToken]`, the ViewModel implements `IValidatableObject`, and one property has a custom `ValidationAttribute`. When does `ModelState.IsValid` become false, and when should the action short-circuit?

---

#### Q12. (R) Review this API-style POST added beside MVC views. Invalid models return 200 HTML instead of validation errors; `[ApiController]` is not used on this controller.

```csharp
public class OrdersController : Controller
{
    [HttpPost]
    public IActionResult QuickAdd([FromBody] QuickAddOrderViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _orders.Add(model);
        return Ok(new { id = model.Sku });
    }
}

public class QuickAddOrderViewModel
{
    [Required] public string Sku { get; set; } = "";
    [Range(1, 100)] public int Quantity { get; set; }
}
```

AJAX caller sends `{ "sku": "", "quantity": 0 }` with `Content-Type: application/json`. Same controller serves Razor views for `/Orders`.
