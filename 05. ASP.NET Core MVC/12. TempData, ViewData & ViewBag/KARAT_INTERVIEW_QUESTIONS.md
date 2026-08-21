# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/12. TempData, ViewData & ViewBag`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) After a successful save, users sometimes see no success banner. Review the POST action, redirect, layout, and detail view.

```csharp
[HttpPost]
public IActionResult Edit(int id, ProductEditModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    _products.Update(id, model);
    TempData["SuccessMessage"] = "Product updated.";
    return RedirectToAction(nameof(Details), new { id });
}

// _Layout.cshtml
@if (TempData["SuccessMessage"] is string msg)
{
    <div class="alert alert-success">@msg</div>
}

// Views/Products/Details.cshtml
@if (TempData["SuccessMessage"] is string detailMsg)
{
    <p class="text-muted">@detailMsg</p>
}
```

---

#### Q2. (P) A team deploys to three Kubernetes pods behind a round-robin load balancer. They use **session-based** TempData (`AddSession()` + default `SessionStateTempDataProvider`). Flash messages intermittently disappear after redirect. What is happening, and what are the two production-viable fixes?

---

#### Q3. (M) Compare **cookie-based** TempData (`CookieTempDataProvider`) vs **session-based** TempData. For each, name one advantage and one failure mode in production (scale-out, size, security, or ops).

---

#### Q4. (R) A Razor view renders blank where the user's display name should appear. The action compiles and runs without exceptions locally.

```csharp
public IActionResult Profile()
{
    var user = _users.GetCurrent();
    ViewBag.UserName = user.DisplayName;
    ViewBag.LastLogin = user.LastLoginUtc;
    return View();
}

// Profile.cshtml
<h2>Welcome, @ViewBag.UsreName</h2>
<p>Last login: @ViewBag.LastLogin</p>
```

---

#### Q5. (P) Implement the correct **Post-Redirect-Get (PRG)** flow for a "Create Order" form. Validation errors must survive the redirect; success must not re-POST on browser refresh. Sketch controller actions and where TempData vs ModelState belong.

---

#### Q6. (D) A junior developer passes a 40-field `CustomerEditViewModel` through `ViewData["Customer"]` instead of a strongly typed view. When should `ViewData`/`ViewBag` be acceptable, and when should you insist on a ViewModel — especially for PRG and validation?

---

#### Q7. (M) After switching to cookie TempData, POST → redirect intermittently throws `InvalidOperationException` about cookie size or request headers. The team stores a full `OrderSummaryViewModel` (line items + audit trail) in TempData for the confirmation page. What is the limit, and what pattern fixes it?

---

#### Q8. (R) A SPA-style partial update uses AJAX; the developer expects a TempData flash message on the same request cycle.

```csharp
[HttpPost]
public IActionResult Approve(int id)
{
    _workflow.Approve(id);
    TempData["Toast"] = "Approved.";
    return PartialView("_ApprovalBadge", _workflow.GetStatus(id));
}

// JavaScript: POST /orders/approve/42, replace #badge with HTML response
// Layout toast reads TempData on full page loads only
```

---

#### Q9. (M) Explain **`TempData.Keep()`** vs **`TempData.Peek()`** with a scenario where the layout reads a flash key once and a child view must read the same key on the **same** request without losing it on the **next** request.

---

#### Q10. (R) Review this controller after a security review flagged "duplicate form submission" and "lost validation messages."

```csharp
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    _users.Create(model);
    ViewBag.Message = "Account created.";
    return View("Confirmation");
}

[HttpGet]
public IActionResult Register() => View();

// Confirmation.cshtml uses @ViewBag.Message
```

What breaks for PRG, refresh safety, and cross-request messaging — and what is the prioritized fix?
