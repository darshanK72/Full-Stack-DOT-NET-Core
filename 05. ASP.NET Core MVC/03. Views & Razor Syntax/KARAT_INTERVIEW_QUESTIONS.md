# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/03. Views & Razor Syntax`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A product review page renders user-submitted comments. QA passes with test data; security scan flags stored XSS. Review this Razor fragment — what is wrong and how do you fix it without breaking allowed rich text?

```cshtml
@model ProductReviewViewModel

<h2>@Model.ProductName</h2>

@foreach (var comment in Model.Comments)
{
    <div class="comment">
        @Html.Raw(comment.Body)
    </div>
}
```

*(Controller passes `comment.Body` straight from the database — no server-side sanitization.)*

---

#### Q2. (R) A teammate "fixes" XSS by switching to `@comment.Body` but adds this analytics hook. Pen testers report script execution from a display name. Diagnose the encoding-context mistake.

```cshtml
@model OrderSummaryViewModel

<button onclick="trackPurchase('@Model.CustomerName', @Model.OrderTotal)">
    Complete order
</button>

<script>
    function trackPurchase(name, total) {
        console.log('Purchase by ' + name + ': ' + total);
    }
</script>
```

---

#### Q3. (R) Pricing rules live in the view because "it's just display math." After a tax-rate change, invoices are wrong in production but unit tests on the service pass. Review this `_InvoiceLine.cshtml` partial — what breaks and where should this logic live?

```cshtml
@model InvoiceLineViewModel

@{
    var discount = Model.Quantity >= 10 ? Model.UnitPrice * 0.15m : 0m;
    var subtotal = (Model.UnitPrice - discount) * Model.Quantity;
    var tax = subtotal * (Model.IsTaxExempt ? 0m : 0.0825m);
    var lineTotal = subtotal + tax;
}

<tr>
    <td>@Model.Sku</td>
    <td>@lineTotal.ToString("C")</td>
</tr>
```

---

#### Q4. (R) A layout renders the page title from `ViewData`. Some pages show a blank `<title>` with no exception. Review the controller and layout — what fails silently and how do you prevent recurrence?

```csharp
// HomeController.cs
public IActionResult About()
{
    ViewData["Titel"] = "About Us";  // typo — intentional trap
    return View();
}
```

```cshtml
@* _Layout.cshtml *@
<title>@(ViewData["Title"] as string ?? "MyApp")</title>
```

---

#### Q5. (D) A dashboard action currently passes six `ViewBag` properties and three `ViewData` keys to one view. The team debates a strongly typed `DashboardViewModel`. When is the refactor worth it, and when is dynamic view data still acceptable?

```csharp
public IActionResult Dashboard()
{
    ViewBag.RecentOrders = _orderService.GetRecent(5);
    ViewBag.AlertCount = _alertService.UnreadCount(User);
    ViewData["LastSync"] = _syncService.LastRunUtc;
    return View();
}
```

---

#### Q6. (M) After deploy to Production, first page load is fast but subsequent edits to `.cshtml` files on the server appear immediately without redeploy. Staging behaves the same. Review this `Program.cs` / project setup — what mechanism is active, and why is it a production risk?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();   // "so designers can tweak CSS wrappers"
var app = builder.Build();
// ... no environment check around runtime compilation
app.Run();
```

*(`.csproj` has no special Razor publish settings; Release build deployed to Linux.)*

---

#### Q7. (R) A developer centralizes "helper logic" in a view using `@functions` because partial views felt heavy. Under load, SQL time appears in view-render traces. Review this snippet — what is wrong?

```cshtml
@model int  @* categoryId *@

@functions {
    async Task<IEnumerable<Product>> LoadProducts(AppDbContext db, int categoryId)
    {
        return await db.Products
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }
}

@inject AppDbContext Db

<ul>
@foreach (var p in await LoadProducts(Db, Model))
{
    <li>@p.Name — @p.Price.ToString("C")</li>
}
</ul>
```

---

#### Q8. (R) Five list views each contain the same 25-line status-badge markup with slightly different CSS classes. A bug fix in one view is missed in the others. What pattern fixes duplication, and what Razor feature misuse often causes this drift?

```cshtml
@* excerpt from Orders/Index.cshtml — duplicated in Shipments, Returns, Quotes, Invoices *@
@{
    string badgeClass = Model.Status switch
    {
        "Pending" => "badge-warning",
        "Shipped" => "badge-info",
        "Delivered" => "badge-success",
        _ => "badge-secondary"
    };
}
<span class="badge @badgeClass">@Model.Status</span>
@if (Model.Status == "Pending" && Model.AgeDays > 7)
{
    <span class="text-danger">Overdue</span>
}
```

---

#### Q9. (P) Production throws `InvalidOperationException: The view 'Index' was not found` for `Home/Index` after CI publish, but `dotnet run` locally finds the view. The pipeline runs `dotnet publish -c Release -o ./out` and copies only `./out` to the server. What publish/view-layout mistakes cause this, and what do you verify in the artifact?

---

#### Q10. (M) Release builds use Razor precompilation by default in modern SDK-style projects. Explain what happens to `.cshtml` files at **build/publish** vs **first request** when precompilation is enabled, and how that differs from runtime compilation.

---

#### Q11. (R) A category page renders 200 products; each row invokes a synchronous partial that hits `_pricingService.GetTierPrice(productId)` inside the partial. TTFB spikes under concurrent users. Review this view pattern — what performance issues stack here, and what is the prioritized fix?

```cshtml
@model CategoryPageViewModel

<table>
@foreach (var product in Model.Products)
{
    <tr>
        <td>@product.Name</td>
        <td>@await Html.PartialAsync("_ProductPrice", product.Id)</td>
    </tr>
}
</table>
```

```cshtml
@* _ProductPrice.cshtml — @model int *@
@inject IPricingService Pricing
@{
    var price = Pricing.GetTierPrice(Model);  // sync DB/cache per row
}
<span>@price.ToString("C")</span>
```

---
