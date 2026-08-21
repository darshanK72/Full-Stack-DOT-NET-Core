# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/04. Layouts, Sections & Partial Views`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Review this view setup. The About page renders raw HTML with no site navigation, CSS, or footer — Home/Index and Contact look correct.

`Views/_ViewStart.cshtml`:

```cshtml
@{
    Layout = "_Layout";
}
```

`Views/Home/_ViewStart.cshtml` (added during a "Home-only experiment"):

```cshtml
@{
    Layout = null;
}
```

`Views/Home/About.cshtml`:

```cshtml
<h1>About Us</h1>
<p>Team bios and mission statement.</p>
```

`Views/Shared/_Layout.cshtml` exists and references `~/css/site.css`.

---

#### Q2. (R) Review this layout and checkout view. The page works in dev until marketing adds a new checkout step — then some pages throw at runtime and others silently omit analytics scripts.

`Views/Shared/_Layout.cshtml`:

```cshtml
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"] - Shop</title>
    @RenderSection("HeadScripts", required: false)
</head>
<body>
    @await Html.PartialAsync("_Nav")
    @RenderBody()
    @RenderSection("Scripts", required: true)
</body>
</html>
```

`Views/Checkout/Confirm.cshtml`:

```cshtml
@{
    ViewData["Title"] = "Confirm Order";
}
<h1>Confirm</h1>
<p>Order #@Model.OrderId</p>
```

`Views/Checkout/Payment.cshtml` defines `@section Scripts { ... }` but Confirm does not.

---

#### Q3. (D) A product dashboard needs a reusable "Recent Orders" panel on three pages. It runs a scoped repository query, shows a loading skeleton, and must be unit-testable without spinning up the full layout pipeline. The team proposes `@await Html.PartialAsync("_RecentOrders")` with data stuffed into `ViewBag.Orders`. What would you choose instead, and why?

---

#### Q4. (M) An admin section uses nested layouts: site chrome in `_Layout.cshtml`, admin sidebar in `_AdminLayout.cshtml`, and page content in individual views. Walk through how Razor resolves `Layout`, `@RenderBody()`, and `@section` definitions across the two layout files — where does each `@RenderSection` call execute?

`Views/Shared/_Layout.cshtml`:

```cshtml
<body>
    @RenderBody()
    @RenderSection("Scripts", required: false)
</body>
```

`Views/Shared/_AdminLayout.cshtml`:

```cshtml
@{
    Layout = "_Layout";
}
<aside>@await Html.PartialAsync("_AdminNav")</aside>
<main>@RenderBody()</main>
@section Scripts {
    @RenderSection("Scripts", required: false)
}
```

`Areas/Admin/Views/Reports/Index.cshtml`:

```cshtml
@{
    Layout = "_AdminLayout";
}
<h1>Reports</h1>
@section Scripts {
    <script src="~/js/reports.js"></script>
}
```

---

#### Q5. (R) Review this view. Build fails locally with a Razor compilation error; the developer claims "the second Scripts block should merge."

`Views/Products/Edit.cshtml`:

```cshtml
@model ProductEditViewModel

@section Scripts {
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
}

<form asp-action="Edit" method="post">...</form>

@section Scripts {
    <script src="~/js/product-editor.js"></script>
}
```

---

#### Q6. (R) Review this Area view. The partial renders empty — no order lines — even though the controller passed a populated model. Same partial works on a non-Area page.

`Areas/Billing/Views/Invoices/Details.cshtml`:

```cshtml
@model InvoiceDetailsViewModel

<h1>Invoice @Model.InvoiceNumber</h1>
<partial name="_LineItems" model="Model.Lines" />
```

`Areas/Billing/Controllers/InvoicesController.cs` returns `View(model)` with `Lines` populated.

Project also has:

- `Views/Shared/_LineItems.cshtml` (generic stub, `@model IEnumerable<LineItem>`)
- `Areas/Billing/Views/Shared/_LineItems.cshtml` (billing-specific markup)

Developer expected Area-local partial to win automatically.

---

#### Q7. (P) You inherit an MVC app where `Areas/Admin/Views/Shared/_Layout.cshtml` exists but Admin pages still use the root `_Layout` and break relative asset paths (`../css/admin.css` 404). Trace the layout resolution path for Area views and what to fix in `_ViewStart` / view `Layout` assignments.

`Areas/Admin/Views/_ViewStart.cshtml`:

```cshtml
@{
    Layout = "_Layout";
}
```

`Areas/Admin/Views/Dashboard/Index.cshtml`:

```cshtml
@{
    ViewData["Title"] = "Admin Dashboard";
}
<h1>Dashboard</h1>
```

Root `Views/Shared/_Layout.cshtml` links `~/css/site.css`. Admin layout links `~/css/admin.css` and includes `_AdminNav`.

---

#### Q8. (P) A catalog page renders 40 product tiles; each tile is a partial that injects `IProductService` and calls `GetRatingAsync(productId)` — 40 service calls per page load. Review the architecture: what breaks under load, and what MVC/Razor pattern reduces work without abandoning partial reuse?

```cshtml
@* Views/Catalog/Index.cshtml *@
@model CatalogPageViewModel

<div class="grid">
@foreach (var p in Model.Products)
{
    <partial name="_ProductTile" model="p" />
}
</div>
```

```cshtml
@* Views/Shared/_ProductTile.cshtml *@
@model ProductSummary
@inject IProductService ProductService
@{
    var rating = await ProductService.GetRatingAsync(Model.Id);
}
<div class="tile">@Model.Name — @rating.Stars stars</div>
```

---

#### Q9. (R) Review this new layout copied from a static HTML template. Views compile but render blank white pages in the browser — no exception in logs.

`Views/Shared/_MarketingLayout.cshtml`:

```cshtml
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"]</title>
    @RenderSection("Styles", required: false)
</head>
<body>
    @await Html.PartialAsync("_MarketingHeader")
    @RenderSection("Hero", required: false)
    @await Html.PartialAsync("_MarketingFooter")
    @RenderSection("Scripts", required: false)
</body>
</html>
```

`Views/Landing/Index.cshtml`:

```cshtml
@{
    Layout = "_MarketingLayout";
    ViewData["Title"] = "Welcome";
}
@section Hero {
    <h1>Launch offer</h1>
}
<p>Sign up today.</p>
```

---
