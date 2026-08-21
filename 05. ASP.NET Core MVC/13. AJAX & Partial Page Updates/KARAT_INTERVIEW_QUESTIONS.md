# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/13. AJAX & Partial Page Updates`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (D) A product page loads a comment list via AJAX. One teammate returns `PartialView("_Comments", comments)`; another returns `JsonResult(comments)` and builds HTML in JavaScript. The page also needs optimistic UI updates and SEO on the initial load. Which approach fits each concern, and what would you standardize on for this MVC app?

---

#### Q2. (R) Review this AJAX POST action and client script. The form submits via `fetch` but the server always returns 400 with no useful body.

```csharp
[HttpPost]
public IActionResult AddToCart(int productId, int quantity)
{
    _cart.Add(productId, quantity);
    return PartialView("_CartSummary", _cart.GetSummary());
}
```

```javascript
fetch('/Cart/AddToCart', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ productId: 42, quantity: 1 })
});
```

`Program.cs` has global antiforgery enabled; the view has no token in the fetch call.

---

#### Q3. (R) Review this partial-update script migrated from jQuery to native `fetch`. Add-to-cart works in Chrome locally but fails in production (cross-origin API subdomain) and antiforgery validation fails intermittently.

```javascript
// Before (worked):
// $.ajax({ url: '/Cart/Add', type: 'POST', data: $('#cartForm').serialize() });

fetch('/Cart/Add', {
    method: 'POST',
    body: new URLSearchParams(new FormData(document.getElementById('cartForm')))
});
```

No `credentials`, no `RequestVerificationToken` header, and the form lives on `www.example.com` while the action URL targets `api.example.com`.

---

#### Q4. (R) Review this search autocomplete partial. QA reports stored XSS when a user searches for `<script>alert(1)</script>` and the suggestion list renders in the DOM.

```csharp
[HttpGet]
public IActionResult Suggest(string term)
{
    var hits = _search.Find(term);
    return PartialView("_Suggestions", hits);
}
```

```html
<!-- _Suggestions.cshtml -->
@foreach (var hit in Model)
{
    <li data-term="@hit.Term">@Html.Raw(hit.HighlightedHtml)</li>
}
```

`HighlightedHtml` is built server-side by wrapping the user's `term` in `<mark>` tags without encoding.

---

#### Q5. (R) Review this AJAX filter endpoint. The grid partial never updates — `Model` is always empty defaults even though the network tab shows a JSON body.

```csharp
[HttpPost]
public IActionResult Filter([FromBody] ProductFilter filter)
{
    var rows = _catalog.Query(filter);
    return PartialView("_ProductGrid", rows);
}

public class ProductFilter
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
}
```

```javascript
const params = new URLSearchParams({ category: 'books', minPrice: '10' });
fetch('/Products/Filter', { method: 'POST', body: params });
```

---

#### Q6. (R) Review this "load more comments" feature. When the database throws, users see the full site layout (nav, footer, error styling) injected inside `#comments-panel`.

```csharp
[HttpGet]
public IActionResult MoreComments(int page)
{
    var pageResult = _comments.GetPage(page); // throws SqlException under load
    return PartialView("_CommentList", pageResult);
}
```

```javascript
fetch(`/Comments/MoreComments?page=${page}`)
    .then(r => r.text())
    .then(html => document.getElementById('comments-panel').innerHTML = html);
```

No status check; `Program.cs` uses `UseExceptionHandler("/Home/Error")` for HTML error pages.

---

#### Q7. (P) A `_StockBadge` partial is fetched via AJAX on every product hover. After deploy behind CloudFront, some users see stale "In stock" badges for sold-out items. The action returns `PartialView` with `Cache-Control` unset. What headers and action patterns fix this without disabling caching entirely for static assets?

---

#### Q8. (R) Review this dashboard that loads three partials into placeholders. After refresh, "Edit" buttons only work on the first panel; duplicate-id warnings appear in dev tools.

```html
<div id="panel-orders" data-url="/Dashboard/OrdersPartial"></div>
<div id="panel-shipping" data-url="/Dashboard/ShippingPartial"></div>
```

```csharp
// _OrdersPartial.cshtml and _ShippingPartial.cshtml both contain:
<button id="edit-row" type="button">Edit</button>
<form id="quick-edit-form">...</form>
```

```javascript
document.querySelectorAll('[data-url]').forEach(el => {
    fetch(el.dataset.url).then(r => r.text()).then(html => el.innerHTML = html);
});
document.getElementById('quick-edit-form').addEventListener('submit', ...); // wired once on full page load
```

---

#### Q9. (P) A team uses **unobtrusive AJAX** (`data-ajax="true"`) on a modal form with client-side validation (`jquery.validate.unobtrusive`). After a successful AJAX POST, the modal closes and reopens with a fresh empty form — but validation errors no longer appear until a full page reload. What causes this and how do you fix it in an MVC partial-update flow?

---

#### Q10. (D) Compare **ASP.NET MVC unobtrusive AJAX + jQuery** vs **HTMX** (or `fetch` + `innerHTML`) for a CRUD admin grid with inline edit, delete confirm, and server-rendered row partials. What breaks or gets simpler when you switch, and when would you not choose HTMX?

---

#### Q11. (M) Walk through what happens on an AJAX POST that returns `PartialView` when `[ValidateAntiForgeryToken]` is present: where the token is validated, what status code the client sees without a token, and how `[AutoValidateAntiforgeryToken]` on the controller differs from `[IgnoreAntiforgeryToken]` on one action.

---

#### Q12. (R) Review this hybrid endpoint used by both a full-page form POST and an AJAX refresh. AJAX callers receive JSON; browser form posts should redirect — but both paths now return HTML fragments and double-submit creates duplicate rows.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult CreateOrder(CreateOrderVm model)
{
    if (!ModelState.IsValid)
        return PartialView("_OrderForm", model);

    var order = _orders.Create(model);
    return PartialView("_OrderRow", order);
}
```

Full-page form: `<form asp-action="CreateOrder" method="post">` with no `data-ajax`.  
AJAX: `fetch` posts `FormData` and replaces `#order-table tbody` with the response HTML.
