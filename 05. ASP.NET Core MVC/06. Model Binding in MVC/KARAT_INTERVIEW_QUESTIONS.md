# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/06. Model Binding in MVC`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [05. ASP.NET Core/08. Model Binding & Validation](../../05.%20ASP.NET%20Core/08.%20Model%20Binding%20%26%20Validation/KARAT_INTERVIEW_QUESTIONS.md) (API / JSON binding — not duplicated here)

---

#### Q1. (R) Review this MVC create action. The Razor form submits correctly in the browser (Network tab shows `application/x-www-form-urlencoded` fields), but `ProductName` and `Price` arrive empty on the server.

```csharp
public class ProductEditViewModel
{
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
}

[HttpPost]
public IActionResult Create([FromBody] ProductEditViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    _products.Add(model);
    return RedirectToAction(nameof(Index));
}
```

Razor form uses `<form asp-action="Create" method="post">` with `<input asp-for="ProductName" />` and `<input asp-for="Price" />` — no `enctype` override.

---

#### Q2. (R) Review line-item editing. Users delete a middle row in the UI; after POST, quantities shift and the wrong SKU is updated. Form field names use explicit indices.

```html
<!-- Rendered names after user removes row 1 -->
<input name="Lines[0].Sku" value="WIDGET-A" />
<input name="Lines[0].Qty" value="2" />
<input name="Lines[2].Sku" value="WIDGET-C" />
<input name="Lines[2].Qty" value="5" />
```

```csharp
public class OrderEditViewModel
{
    public List<LineItemViewModel> Lines { get; set; } = new();
}

public class LineItemViewModel
{
    public string Sku { get; set; } = "";
    public int Qty { get; set; }
}

[HttpPost]
public IActionResult Save(OrderEditViewModel model)
{
    foreach (var line in model.Lines)
        _orders.Apply(line.Sku, line.Qty);
    return RedirectToAction("Index");
}
```

---

#### Q3. (R) Review nested address binding on checkout. `ShippingCity` binds but nested `Address.City` is always null despite visible inputs.

```csharp
public class CheckoutViewModel
{
    public string ShippingCity { get; set; } = ""; // top-level — works
    public AddressModel Address { get; set; } = new();
}

public class AddressModel
{
    public string City { get; set; } = "";
    public string PostalCode { get; set; } = "";
}
```

Partial view `_AddressEditor.cshtml` renders:

```html
<input name="City" asp-for="City" />
<input name="PostalCode" asp-for="PostalCode" />
```

Invoked from parent as `@await Html.PartialAsync("_AddressEditor", Model.Address)`.

---

#### Q4. (R) Review optional numeric and date fields on an HR form. Empty inputs produce unexpected values instead of "not provided."

```csharp
public class LeaveRequestViewModel
{
    public int? DaysRequested { get; set; }
    public DateTime? StartDate { get; set; }
}

[HttpPost]
public IActionResult Submit(LeaveRequestViewModel model)
{
    // Expected: omitted fields → null; user reports DaysRequested = 0, StartDate = 0001-01-01
    _leave.Submit(model);
    return RedirectToAction("Index");
}
```

View uses `<input asp-for="DaysRequested" />` and `<input asp-for="StartDate" type="date" />` — user clears both fields and submits.

---

#### Q5. (R) Review culture-sensitive date binding. UK users enter `31/01/2026`; US-hosted server culture is `en-US`. Binding fails silently and `ModelState` shows errors users do not understand.

```csharp
// Program.cs — no RequestLocalization middleware configured; server culture en-US

public class EventViewModel
{
    [Required]
    public DateTime EventDate { get; set; }
}

[HttpPost]
public IActionResult Create(EventViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    _events.Add(model);
    return RedirectToAction("Index");
}
```

Form posts `EventDate=31/01/2026` as text (no `<input type="date">` — legacy browser support requirement).

---

#### Q6. (R) Review document upload on an invoice form. File and metadata should bind together; `UploadedFile` is always null while other fields bind.

```html
<form asp-action="Upload" method="post">
    <input asp-for="InvoiceNumber" />
    <input asp-for="Amount" />
    <input type="file" name="UploadedFile" />
    <button type="submit">Upload</button>
</form>
```

```csharp
public class InvoiceUploadViewModel
{
    public string InvoiceNumber { get; set; } = "";
    public decimal Amount { get; set; }
    public IFormFile? UploadedFile { get; set; }
}

[HttpPost]
public IActionResult Upload(InvoiceUploadViewModel model)
{
    if (model.UploadedFile == null)
        return View(model); // always hits this branch
    _storage.Save(model);
    return RedirectToAction("Index");
}
```

---

#### Q7. (R) Review user profile update for over-posting. QA reports a non-admin can set themselves admin via browser dev tools.

```csharp
public class User  // EF entity used directly as the action parameter
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsAdmin { get; set; }
    [BindNever] public DateTime CreatedUtc { get; set; }
}

[HttpPost]
public IActionResult Edit(int id, User model)
{
    var user = _db.Users.Find(id);
    user.DisplayName = model.DisplayName;
    user.Email = model.Email;
    user.IsAdmin = model.IsAdmin; // copied from bound model
    _db.SaveChanges();
    return RedirectToAction("Index");
}
```

View only shows `DisplayName` and `Email` fields — no `IsAdmin` input in Razor.

---

#### Q8. (R) Review registration flow. Validation attributes fire (client-side shows errors), but invalid data still reaches the database after POST.

```csharp
public class RegisterViewModel
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}

[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    var user = _users.Create(model.Email, model.Password);
    _db.Users.Add(user);
    _db.SaveChanges();
    return RedirectToAction("Login");
}
```

View includes `_ValidationScriptsPartial` and `<div asp-validation-summary="All">`; `[HttpPost]` action never checks `ModelState`.

---

#### Q9. (M) A legacy admin page uses jQuery to POST updates without a full form submit. Fields never bind server-side. Compare what the client sends vs what the MVC action expects.

```javascript
// Client — submit handler
$.ajax({
    url: '/Admin/UpdateSettings',
    type: 'POST',
    contentType: 'application/json',
    data: JSON.stringify({ siteName: $('#siteName').val(), maxUsers: $('#maxUsers').val() })
});
```

```csharp
public class SiteSettingsViewModel
{
    public string SiteName { get; set; } = "";
    public int MaxUsers { get; set; }
}

[HttpPost]
public IActionResult UpdateSettings(SiteSettingsViewModel model)
{
    // model.SiteName empty, MaxUsers 0
    _settings.Save(model);
    return Ok();
}
```

Controller inherits `Controller` (not `[ApiController]`). No `[FromBody]` on the parameter.

---

#### Q10. (P) A team registers a custom `CurrencyModelBinder` to parse `"$1,234.56"` from form fields. The binder stores the last parsed culture in an instance field for logging. It is registered as a singleton. What breaks under concurrent form posts, and how should binders be written?

```csharp
public class CurrencyModelBinder : IModelBinder
{
    private CultureInfo? _lastCulture;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        _lastCulture = bindingContext.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture;
        if (decimal.TryParse(value, NumberStyles.Currency, _lastCulture, out var amount))
        {
            bindingContext.Result = ModelBindingResult.Success(amount);
            return Task.CompletedTask;
        }
        bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid currency amount.");
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}

// Startup
builder.Services.AddSingleton<IModelBinder, CurrencyModelBinder>();
```

---

#### Q11. (R) Review checkbox and hidden-field pattern for `bool` and `bool?` opt-in flags. Marketing reports opt-out users still show as subscribed after save.

```html
<form asp-action="SavePreferences" method="post">
    <input type="hidden" asp-for="UserId" />
    <!-- Newsletter: optional tri-state — null = no change on server -->
    <input type="checkbox" asp-for="Newsletter" />
    <!-- Terms: required true to submit -->
    <input type="checkbox" asp-for="AcceptedTerms" />
    <button type="submit">Save</button>
</form>
```

```csharp
public class PreferencesViewModel
{
    public int UserId { get; set; }
    public bool? Newsletter { get; set; }   // PATCH-style: null = unchanged
    public bool AcceptedTerms { get; set; }
}

[HttpPost]
public IActionResult SavePreferences(PreferencesViewModel model)
{
    _prefs.Update(model.UserId, newsletter: model.Newsletter, terms: model.AcceptedTerms);
    return RedirectToAction("Index");
}
```

User leaves Newsletter unchecked ( wants unchanged ) but AcceptedTerms checked. Saved newsletter becomes `false` instead of unchanged.

---
