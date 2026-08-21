# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/14. Client-Side Validation`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Review this registration action and Razor view. QA says the form "validates fine" in the browser, but attackers can create accounts with empty passwords.

```csharp
// AccountController.cs
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    // Client-side validation handles UX — skip server round-trip when invalid
    return RedirectToAction("Index", "Home");
}

public class RegisterViewModel
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}
```

```html
<!-- Views/Account/Register.cshtml -->
<form asp-action="Register" method="post">
    <input asp-for="Email" />
    <input asp-for="Password" type="password" />
    <button type="submit">Register</button>
</form>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

`_Layout.cshtml` includes jQuery and `_ValidationScriptsPartial` scripts. Model has data annotations.

---

#### Q2. (R) Review this layout and create form. Fields show `data-val-*` attributes in HTML, but nothing happens on blur or submit — no inline errors.

```html
<!-- Views/Shared/_Layout.cshtml -->
<script src="~/lib/jquery/dist/jquery.min.js"></script>
@RenderSection("Scripts", required: false)

<!-- Views/Products/Create.cshtml -->
<form asp-action="Create" method="post">
    <div asp-validation-summary="ModelOnly"></div>
    <input asp-for="Sku" />
    <span asp-validation-for="Sku"></span>
    <button type="submit">Save</button>
</form>
@section Scripts {
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
    <!-- jquery.validate.unobtrusive.min.js intentionally omitted to reduce bundle size -->
}
```

`Program.cs` uses standard MVC with Razor views; model has `[Required]` on `Sku`.

---

#### Q3. (P) A business rule attribute `[MustBeFutureDate]` validates that `ShipDate` is after today. Server-side validation works via `IValidatableObject`, but the browser never blocks past dates before POST. What is the MVC client-validation extension point, and what must you register?

```csharp
public class MustBeFutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        => value is DateTime d && d.Date <= DateTime.Today
            ? new ValidationResult("Ship date must be in the future.")
            : ValidationResult.Success;
}

public class OrderViewModel
{
    [MustBeFutureDate]
    public DateTime ShipDate { get; set; }
}
```

View uses `<input asp-for="ShipDate" />` and `_ValidationScriptsPartial`.

---

#### Q4. (R) Review username availability check. Users report duplicate accounts when they tab quickly through the field and submit.

```csharp
public class RegisterViewModel
{
    [Required]
    [Remote(action: "CheckUsername", controller: "Account")]
    public string Username { get; set; } = "";
}

// AccountController
public IActionResult CheckUsername(string username)
    => Json(_users.IsAvailable(username) ? true : $"Username '{username}' is taken.");
```

```html
<form asp-action="Register" method="post" id="register-form">
    <input asp-for="Username" />
    <span asp-validation-for="Username"></span>
    <button type="submit">Register</button>
</form>
```

`_ValidationScriptsPartial` is present. No debounce or submit guard in the view.

---

#### Q5. (R) Review this edit form. Server returns validation errors after POST, but users see a blank page above the fields — property errors never appear in the summary area they expect.

```html
<form asp-action="Edit" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    <input asp-for="ProductName" />
    <span asp-validation-for="ProductName" class="text-danger"></span>
    <input asp-for="UnitPrice" />
    <span asp-validation-for="UnitPrice" class="text-danger"></span>
    <button type="submit">Save</button>
</form>
```

Controller:

```csharp
[HttpPost]
public IActionResult Edit(ProductViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    _repo.Update(model);
    return RedirectToAction(nameof(Index));
}
```

Model: `[Required]` on `ProductName`, `[Range(0.01, 9999)]` on `UnitPrice`. Client scripts loaded.

---

#### Q6. (P) Ops wants client-side validation **disabled in Production** (rely on server validation + fewer script failures) but **enabled in Development** for faster feedback. Review the proposed `_ViewImports.cshtml` approach:

```csharp
@inject Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlHelper Html
@{
    Html.ViewContext.ClientValidationEnabled =
        Html.ViewContext.HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();
}
```

Layout still renders `_ValidationScriptsPartial` in all environments.

---

#### Q7. (M) A developer replaces tag helpers with hand-written HTML to match a design system. Required and range rules still work on the server after POST, but client validation silently disappears. Explain what `IHtmlHelper` / tag helpers emit for unobtrusive validation and what breaks when you drop them.

```html
<!-- Before (worked client-side) -->
<input asp-for="Quantity" />

<!-- After (server-only) -->
<input type="number" name="Quantity" id="Quantity" class="form-control" />
```

Model:

```csharp
public class LineItemViewModel
{
    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }
}
```

---

#### Q8. (P) A French-localized MVC app shows English jQuery Validate messages ("This field is required") while server-rendered `ModelState` errors are correctly in French. `_ViewImports` sets `@using System.ComponentModel.DataAnnotations` and resource files back `[Required]` display messages. What still needs wiring for client-side messages?

```html
<form asp-action="Create" method="post">
    <input asp-for="Nom" />
    <span asp-validation-for="Nom"></span>
</form>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

`Program.cs`: `RequestLocalization` configured with `fr-FR` as default culture.

---

#### Q9. (D) A team ships a React SPA that POSTs JSON to `/api/orders` while keeping legacy MVC Razor pages for admin. Product asks: "We already have data annotations on `OrderViewModel` — does that protect the API?" Compare where MVC unobtrusive validation applies vs what the SPA path requires.

```csharp
// MVC admin — Views/Orders/Create.cshtml with tag helpers + _ValidationScriptsPartial
public class OrderViewModel
{
    [Required][StringLength(50)] public string CustomerName { get; set; } = "";
    [Range(1, 1000)] public int Quantity { get; set; }
}

// API — separate controller, same property rules desired
[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] OrderViewModel model) { /* ... */ }
}
```

---

#### Q10. (R) Review this AJAX partial-form save from Chapter 13 patterns. Inline field validation works until the user clicks Save — the request fires even when `[Required]` fields are empty.

```html
<form id="profile-form" asp-action="SaveProfile" asp-controller="Account">
    <input asp-for="DisplayName" />
    <span asp-validation-for="DisplayName"></span>
    <button type="button" id="save-profile">Save</button>
</form>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        $('#save-profile').on('click', function () {
            $.post($('#profile-form').attr('action'), $('#profile-form').serialize());
        });
    </script>
}
```

`DisplayName` has `[Required]`. Standard unobtrusive scripts are loaded.
