# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/09. Routing & Attribute Routing`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [05. ASP.NET Core/07. Routing & Endpoints](../../05.%20ASP.NET%20Core/07.%20Routing%20&%20Endpoints/KARAT_INTERVIEW_QUESTIONS.md) (endpoint routing, minimal APIs, link generation behind proxies)

---

#### Q1. (R) Review this `ProductsController` and `Program.cs`. `GET /products/sale` shows a product named "sale" instead of the sale landing page.

```csharp
// Program.cs
app.MapControllerRoute(
    name: "productSlug",
    pattern: "products/{id}",
    defaults: new { controller = "Products", action = "Details" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

public class ProductsController : Controller
{
    public IActionResult Index() => View(_repo.All());

    public IActionResult Details(string id) => View(_repo.BySlug(id));

    public IActionResult Sale() => View(_repo.SaleItems());
}
```

Marketing expects `/products/sale` to hit `Sale()`; QA reports it returns a product whose slug is `"sale"`.

---

#### Q2. (M) A team registers two conventional routes for an MVC storefront. Explain how optional `{id?}` and default values interact when a user visits `/catalog`, `/catalog/featured`, and `/catalog/list/42`.

```csharp
app.MapControllerRoute(
    name: "catalog",
    pattern: "catalog/{action=List}/{id?}",
    defaults: new { controller = "Catalog" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Which controller action handles each URL, and what happens if `List` is renamed but the route pattern is not updated?

---

#### Q3. (R) Review this route registration and action. `GET /orders/not-a-number` returns 500 with `FormatException` in logs instead of a client-friendly not-found.

```csharp
app.MapControllerRoute(
    name: "order",
    pattern: "orders/{id:int}",
    defaults: new { controller = "Orders", action = "Details" });

public class OrdersController : Controller
{
    public IActionResult Details(int id) => View(_orders.Get(id));
}
```

No attribute routes exist on `OrdersController`.

---

#### Q4. (R) Review `Program.cs` route order. After deploy, every unknown URL renders the home page with HTTP 200 instead of 404, and `/admin/users` never reaches `AdminController`.

```csharp
app.MapControllerRoute(
    name: "fallback",
    pattern: "{*path}",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

---

#### Q5. (R) Review this shared `_Layout.cshtml` link and area setup. Clicking **Manage Users** from the Shop area returns 404, but the same link works from the root site.

```html
<!-- _Layout.cshtml (shared by root and Shop area views) -->
<a asp-controller="Users" asp-action="Index">Manage Users</a>
```

```csharp
// Program.cs
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Areas/Admin/Controllers/UsersController.cs — no [Area] attribute missing
[Area("Admin")]
public class UsersController : Controller
{
    public IActionResult Index() => View();
}
```

Current request when bug occurs: `/shop/products` (Shop area).

---

#### Q6. (M) Compare attribute routing applied at the **controller** vs **action** level. Given the snippets below, what are the final URLs for `Monthly` and `Export`, and which tokens does each action inherit?

```csharp
[Route("reports/[controller]")]
public class ReportsController : Controller
{
    [HttpGet("monthly")]
    public IActionResult Monthly() => View();

    [HttpGet("/export/{year:int}")]
    public IActionResult Export(int year) => File(_svc.Export(year), "text/csv");
}
```

When would you put `[Route]` on the controller vs only on actions in a mixed MVC + API surface?

---

#### Q7. (R) Review this documentation site controller. `GET /docs/getting-started/install` returns 404, but `GET /docs/getting-started` works. Static files middleware is registered before routing.

```csharp
[Route("docs")]
public class DocsController : Controller
{
    [HttpGet("{*slug}")]
    public IActionResult Page(string slug)
    {
        var content = _store.Find(slug);
        return content is null ? NotFound() : View("Doc", content);
    }
}
```

---

#### Q8. (M) For the same conventional route `{controller=Home}/{action=Index}/{id?}`, contrast the HTTP status and routing outcome for:

1. `GET /home/index` when `Index` exists and allows GET  
2. `POST /home/index` when only GET is implemented (no `[HttpPost]` on `Index`)  
3. `GET /homme/index` (typo in controller name)

Explain how endpoint routing differs from "no route matched" vs "route matched, method not allowed."

---

#### Q9. (P) Production enables lowercase URLs for SEO. After enabling the option below, organic links work but several redirects and `Url.Action` calls still emit PascalCase paths that 404 on Linux containers.

```csharp
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Somewhere in AccountController:
return Redirect("/Account/Login?returnUrl=/Products/Details/5");
```

What must change in route registration, link generation, and hard-coded redirects for consistent behavior cross-platform?

---

#### Q10. (D) A legacy ASP.NET Core 2.2 MVC app is upgraded to .NET 8. The old `Startup.cs` still calls `UseMvcWithDefaultRoute()` while newer code uses `MapControllerRoute` inside `UseEndpoints`. Reviewers ask whether to keep both for compatibility.

```csharp
// Old (commented in places, still active in one branch)
app.UseMvcWithDefaultRoute();

// New
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

What breaks if both run, what is the modern replacement, and how does endpoint routing change action selection compared to legacy IRouter middleware?

---

#### Q11. (R) Review attribute + conventional mixing on one controller. Integration tests expect `GET /api/reports/summary` but receive 404; `GET /Reports/Summary` works. Tag Helper links in Razor views point at `/Reports/Summary`.

```csharp
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

[Route("api/[controller]")]
public class ReportsController : Controller
{
    [HttpGet("summary")]
    public IActionResult Summary() => Json(_svc.Summary());

    public IActionResult Index() => View();
}
```

```html
<a asp-controller="Reports" asp-action="Summary">Summary API</a>
```

---
