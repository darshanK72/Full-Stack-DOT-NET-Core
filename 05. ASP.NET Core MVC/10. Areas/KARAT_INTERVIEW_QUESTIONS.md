# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/10. Areas`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A developer creates an Admin area following folder conventions but skips the area attribute. The project builds; `GET /Admin/Dashboard` returns 404. Review the controller and routing setup — what is wrong?

```csharp
// Areas/Admin/Controllers/DashboardController.cs
namespace MyApp.Areas.Admin.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
```

```csharp
// Program.cs
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

---

#### Q2. (R) After adding a Customer portal area, `/Portal/Orders/History` intermittently hits the wrong controller in staging. Review route registration — diagnose and fix in priority order.

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

*(Assume both root `OrdersController` and `Areas/Portal/Controllers/OrdersController` exist.)*

---

#### Q3. (R) QA reports Admin sidebar links land on the public site. Review the layout and a controller redirect — what breaks link generation?

```html
<!-- Areas/Admin/Views/Shared/_AdminNav.cshtml -->
<a asp-controller="Users" asp-action="Index">Users</a>
<a asp-controller="Reports" asp-action="Index">Reports</a>
```

```csharp
// Areas/Admin/Controllers/UsersController.cs
[Area("Admin")]
public class UsersController : Controller
{
    public IActionResult Create() { /* ... */ return RedirectToAction("Index"); }
}
```

*(Current request: `/Admin/Dashboard/Index`.)*

---

#### Q4. (P) Product wants one `_OrderSummary.cshtml` partial reused by **Store**, **Admin**, and **Support** areas, but each area keeps its own chrome (`_Layout`). Where should shared markup live, how do area views reference it, and what breaks if each team copies the partial into their area folder?

---

#### Q5. (R) A contractor reorganizes the Support area to "flatten" paths. Views compile in IDE but runtime throws `InvalidOperationException: The view 'Index' was not found`. Review the structure — what's wrong?

```
Areas/
  Support/
    TicketsController.cs          ← moved here
    Views/
      Ticket/
        Index.cshtml
    _ViewStart.cshtml
```

*(Expected convention: `Areas/{AreaName}/Controllers/` and `Areas/{AreaName}/Views/{Controller}/`.)*

---

#### Q6. (R) Root site and Admin area both define `HomeController`. `/` works; `/Admin/Home/Index` works; but `GET /Home/Index` from an Admin page and integration tests for "admin home" fail unpredictably. Explain the collision and how routing resolves it.

```csharp
// Controllers/HomeController.cs — public marketing site
public class HomeController : Controller { public IActionResult Index() => View(); }

// Areas/Admin/Controllers/HomeController.cs
[Area("Admin")]
public class HomeController : Controller { public IActionResult Index() => View(); }
```

---

#### Q7. (R) Admin views fail to compile after moving area-specific tag helpers and `@using` directives to the root `_ViewImports.cshtml` only. Review the view import scope — what is wrong and how do area-level imports interact with the root file?

```
Views/
  _ViewImports.cshtml          ← @using MyApp.Areas.Admin.ViewModels
  Shared/
Areas/
  Admin/
    Views/
      Dashboard/
        Index.cshtml           ← uses <admin-card> tag helper + AdminDashboardVm
    _ViewStart.cshtml
```

---

#### Q8. (P) Security audit: the Admin area must require the **Administrator** policy; Support requires **SupportAgent**; Store stays anonymous for browsing but checkout requires authentication. Review this partial setup — what gaps remain for a new controller added tomorrow?

```csharp
// Program.cs
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Administrator", p => p.RequireRole("Admin"));
    o.AddPolicy("SupportAgent", p => p.RequireRole("Support"));
});

// Areas/Admin/Controllers/UsersController.cs
[Area("Admin")]
[Authorize(Policy = "Administrator")]
public class UsersController : Controller { /* ... */ }

// Areas/Support/Controllers/TicketsController.cs
[Area("Support")]
[Authorize(Policy = "SupportAgent")]
public class TicketsController : Controller { /* ... */ }
```

*(No area-wide convention or filter registration — developers add controllers ad hoc.)*

---

#### Q9. (M) The team wants `/Admin` (no controller segment) to open the admin dashboard, while `/Admin/Users` still works. Review this route attempt — what matches, what 404s, and what is the correct pattern?

```csharp
app.MapControllerRoute(
    name: "admin_root",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

*(Request: `GET /Admin` — expected `DashboardController.Index` in Admin area.)*

---

#### Q10. (D) A monolith MVC app grows **Marketing**, **Store**, **Admin**, and **API** surfaces. Product asks whether to keep **Areas**, split into Razor Class Libraries, or separate deployable apps. What decision criteria matter for routing, auth boundaries, team ownership, and release cadence — and when do Areas become the wrong tool?
