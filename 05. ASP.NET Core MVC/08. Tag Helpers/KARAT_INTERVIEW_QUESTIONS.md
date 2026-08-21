# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/08. Tag Helpers`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Review this strongly typed edit view. QA reports that validation messages never appear for `Email`, and the posted form updates the wrong property. The controller and view compile cleanly.

```csharp
// EditCustomerViewModel.cs
public class EditCustomerViewModel
{
    public int Id { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

// Customer.cs (entity)
public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
}

// Edit.cshtml — top of file
@model Customer

<form asp-action="Edit" method="post">
    <input type="hidden" asp-for="Id" />
    <div asp-validation-summary="ModelOnly"></div>
    <label asp-for="Email"></label>
    <input asp-for="Email" class="form-control" />
    <span asp-validation-for="Email"></span>
    <label asp-for="FullName"></label>
    <input asp-for="FullName" class="form-control" />
    <button type="submit">Save</button>
</form>
```

Controller action: `public IActionResult Edit(Customer model)` with `[ValidateAntiForgeryToken]`.

---

#### Q2. (P) Your team is migrating a .NET Framework MVC 5 app to ASP.NET Core MVC. Hundreds of views still use `@Html.TextBoxFor`, `@Html.LabelFor`, and `@Html.ValidationMessageFor`. Product wants Tag Helpers on new screens only. What is the migration strategy, what breaks if you `@addTagHelper` globally without opt-out, and when would you keep HTML Helpers?

---

#### Q3. (M) You ship a custom `HighlightTagHelper` that wraps matching text in `<mark>`. It must run **after** built-in helpers so it does not rewrite attributes that `asp-for` still needs to process. Review this implementation and explain process order.

```csharp
[HtmlTargetElement("highlight")]
public class HighlightTagHelper : TagHelper
{
    public string Term { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        var content = output.GetChildContentAsync().Result.GetContent();
        output.Content.SetHtmlContent(content.Replace(Term, $"<mark>{Term}</mark>"));
    }
}

// View
<highlight term="error">
    <input asp-for="Notes" class="form-control" />
</highlight>
```

`_ViewImports.cshtml` has `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` and `@addTagHelper *, MyApp.Web`.

---

#### Q4. (R) Review this navigation partial. In staging, several links 404 or hit the wrong controller; one link drops route values on POST redirect. The app uses conventional routing `{controller=Home}/{action=Index}/{id?}` plus one attribute route.

```cshtml
@* _Nav.cshtml *@
<a asp-controller="Orders" asp-action="Details" asp-route-id="@Model.OrderId">Order</a>
<a asp-controller="Admin" asp-action="Index">Admin</a>
<a asp-action="Archive" asp-route-id="@Model.OrderId">Archive</a>
<a href="/reports/monthly">Reports</a>

@* Attribute route on ReportsController: [Route("reports/{year:int}/{month:int}")] *@
<a asp-controller="Reports" asp-action="Monthly" asp-route-year="2025" asp-route-month="8">August</a>
<a asp-controller="Reports" asp-action="Monthly" asp-route-year="2025">August (broken)</a>
```

Current request: `/Orders/Edit/42` — no `Admin` area registered; `Archive` action lives on `OrdersController`.

---

#### Q5. (P) Production users report stale JavaScript after every deploy until hard refresh. Review `_Layout.cshtml` and explain cache-busting behavior in Development vs Production, including what breaks if `ASPNETCORE_ENVIRONMENT` is wrong on the server.

```cshtml
<environment include="Development">
    <script src="~/js/site.js"></script>
    <script src="~/lib/jquery/dist/jquery.js"></script>
</environment>
<environment exclude="Development">
    <script src="~/js/site.min.js" asp-append-version="true"></script>
    <script src="~/lib/jquery/dist/jquery.min.js" asp-append-version="true"></script>
</environment>
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
```

CDN team asks whether `asp-append-version` works for absolute CDN URLs.

---

#### Q6. (R) Review this AJAX create form. The POST succeeds in the full-page version but returns 400 Antiforgery validation failed when submitted via `fetch`. Same view, same controller.

```cshtml
<form asp-action="Create" asp-controller="Tasks" id="createForm">
    <input asp-for="Title" />
    <button type="submit">Create</button>
</form>

<script>
document.getElementById('createForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const body = new FormData(e.target);
    await fetch('/Tasks/Create', { method: 'POST', body });
});
</script>
```

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(TaskInput model) { /* ... */ }
```

---

#### Q7. (R) Accessibility audit flags duplicate `id` attributes and broken label associations on this editor partial rendered inside a loop.

```cshtml
@foreach (var line in Model.Lines)
{
    <partial name="_LineEditor" model="line" />
}

@* _LineEditor.cshtml *@
@model OrderLineViewModel
<label for="Quantity">Qty</label>
<input asp-for="Quantity" id="Quantity" class="form-control" />
<span asp-validation-for="Quantity"></span>
```

Browser DevTools shows multiple elements with `id="Quantity"` and labels pointing at the first one only.

---

#### Q8. (P) A shared Razor Class Library (`MyCompany.Ui.Rcl`) ships reusable form components with custom Tag Helpers in namespace `MyCompany.Ui.TagHelpers`. Consumer MVC apps add `<ProjectReference>` but Tag Helpers never activate — views render raw `<summary-card>` elements. What registration steps are required in the RCL and consuming app, and what is a common pitfall with `_ViewImports` scope?

---

#### Q9. (M) A legacy partial must keep explicit `name` and `id` attributes for a jQuery plugin, but the rest of the app uses Tag Helpers. Review this mixed markup and explain when `!` opt-out is correct vs when it causes silent binding failures.

```cshtml
@model ProductViewModel

<input !type="text"
       !name="legacySku"
       !id="legacySku"
       value="@Model.Sku"
       class="legacy-picker" />

<input asp-for="Sku" class="form-control" />

<label !for="legacySku">Legacy SKU</label>
<label asp-for="Sku"></label>
```

POST action binds `[Bind(Prefix = "legacySku")] string legacySku` and `ProductViewModel` with property `Sku`.

---
