# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/12. TempData, ViewData & ViewBag`

---

#### Q1. (R) After a successful save, users sometimes see no success banner. Review the POST action, redirect, layout, and detail view.

**Answer:** TempData is **read-once by default** — the layout consumes `SuccessMessage` on the first read, so the detail view's second read returns null and the banner disappears or appears inconsistently depending on render order.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| TempData semantics | Two reads of same key in one request | First consumer wins; second gets null |
| UX | Layout + view both display flash | Message missing in one location |
| Design | Implicit consumption without coordination | Intermittent "no banner" bug reports |

**Fix (priority order):**

1. Read once — only the layout **or** the view displays the message, not both.
2. Or use `TempData.Peek("SuccessMessage")` in the layout so the value stays available for the same request; use `Keep()` only if you need it on the **next** request too.
3. Prefer a single partial `_FlashMessages.cshtml` invoked from the layout that owns all TempData keys.

**Production takeaway:** TempData looks like session state but behaves like a one-shot queue — double-read in one round-trip is a classic Karat trap.

---

#### Q2. (P) Session-based TempData behind round-robin load balancer — flash messages intermittently disappear.

**Answer:** Session TempData stores payload server-side keyed by session id; after redirect the browser may hit a **different pod** that does not have that session entry unless sessions are shared or affinity is enabled — so TempData is empty on some requests.

**Fix (priority order):**

1. **Cookie TempData provider** (default in many templates) — state travels with the client; works across pods without sticky sessions (subject to size limits).
2. **Distributed session** — `AddStackExchangeRedisCache` / SQL session state so every pod reads the same session store; keep `SessionStateTempDataProvider`.
3. Avoid in-memory session only on multi-instance deployments without affinity.

**Production takeaway:** Scale-out exposes TempData provider choice — cookie vs session is a hosting decision, not a view-layer preference.

---

#### Q3. (M) Cookie-based vs session-based TempData — advantage and failure mode each.

**Answer:** Cookie TempData is stateless on the server and scales horizontally; session TempData keeps cookies small but requires shared or sticky session in multi-server setups.

| Provider | Advantage | Production failure mode |
|---|---|---|
| **Cookie** (`CookieTempDataProvider`) | No server session store; works on any pod after redirect | Payload serialized into cookie — **~4 KB total cookie budget** per request; large objects blow header limits |
| **Session** (`SessionStateTempDataProvider`) | Larger arbitrary objects in server session | **Pod affinity / distributed session** required; in-memory session on one node loses flash after redirect to another node |

- Cookie TempData is encrypted/signed by ASP.NET Core Data Protection — keys must be synchronized across instances (shared key ring) or cookies become unreadable after deploy to another node.
- Session path adds latency and ops burden (Redis/SQL) but fits large flash payloads.

**Production takeaway:** Default cookie provider is correct for most PRG flash strings; session provider is for larger state when you already run distributed session.

---

#### Q4. (R) Razor view renders blank where the user's display name should appear — compiles without error.

```csharp
ViewBag.UserName = user.DisplayName;
// Profile.cshtml: @ViewBag.UsreName
```

**Answer:** `ViewBag` is dynamically typed (`dynamic`) — a typo in the view (`UsreName` vs `UserName`) **does not fail at compile time**; at runtime the binder returns null and Razor renders empty output with no exception.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Typing | ViewBag dynamic property access | Typos silent at build time |
| Runtime | Missing member → null | Blank UI; hard to spot in code review |
| Maintainability | Magic strings across action and view | Rename refactor does not update views |

**Fix (priority order):**

1. Use a **strongly typed ViewModel** — `@model ProfileViewModel` and `@Model.UserName`; typos become compile errors.
2. If keeping ViewBag, use constants: `ViewBag[ViewBagKeys.UserName] = ...` with shared static class.
3. Enable Razor analysis / nullable reference checks where applicable; prefer ViewModels for anything beyond trivial one-off flags.

**Production takeaway:** ViewBag trades convenience for zero compile-time safety — Karat uses one-character typos to test whether you reach for ViewModels.

---

#### Q5. (P) Correct PRG flow for "Create Order" with validation surviving redirect and refresh-safe success.

**Answer:** POST validates and mutates; on success **redirect to GET** with a short TempData token; on validation failure either return View with ModelState (no redirect) or redirect with TempData-serialized errors via `TempData["Errors"]` / dedicated pattern — never return 200 View after successful POST.

**Recommended pattern:**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(OrderCreateViewModel model)
{
    if (!ModelState.IsValid)
        return View(model); // validation: stay on POST response, no PRG needed

    var orderId = _orders.Create(model);
    TempData["SuccessMessage"] = $"Order {orderId} created.";
    return RedirectToAction(nameof(Details), new { id = orderId });
}

[HttpGet]
public IActionResult Details(int id) => View(_orders.Get(id));
```

- **Validation on POST:** `return View(model)` keeps `ModelState` in the same request — TempData is not required for field errors.
- **Success:** always `RedirectToAction` — browser refresh repeats GET, not POST (no duplicate orders).
- **Success message:** single key in TempData, read once in layout partial.
- Do **not** pass the created entity through TempData — pass only `id` in route; load from store on GET.

**Production takeaway:** PRG separates command (POST) from query (GET); TempData is for ephemeral flash across that redirect, not for domain entities.

---

#### Q6. (D) 40-field model via `ViewData["Customer"]` vs strongly typed ViewModel — when is each acceptable?

**Answer:** Large domain/edit models belong in a **ViewModel** passed as `@model` — compile-time checks, IntelliSense, and consistent validation attributes; `ViewData`/`ViewBag` suit incidental page metadata, not primary page data.

| Use ViewData / ViewBag | Use ViewModel |
|---|---|
| Page title, layout flags, `"ActiveTab" => "Settings"` | Forms, lists, detail pages with many properties |
| One-off admin/debug toggles | Anything with data annotations / client validation |
| Passing data from action filter to layout | PRG GET actions that reload state from id + service |

- `ViewData` and ViewBag share the same backing dictionary — same string-key fragility.
- For PRG after validation failure, prefer `return View(model)` on POST; on GET reload ViewModel from database by id rather than stuffing 40 fields into TempData (size + serialization cost).
- ViewModels can compose subsets (create vs edit DTOs) — avoids over-posting and clarifies what the view actually needs.

**Production takeaway:** ViewBag for `"Title"` is fine; ViewBag for your entire customer aggregate is a maintainability and binding risk — insist on ViewModels for chapter 05-style strongly typed views.

---

#### Q7. (M) Cookie TempData throws cookie size / header errors when storing full `OrderSummaryViewModel`.

**Answer:** Cookie TempData serializes values into the **`.AspNetCore.Mvc.CookieTempDataProvider`** cookie; browsers enforce roughly **4096 bytes per cookie** and total header size limits (~8–16 KB depending on server/proxy) — a large view model with line items exceeds this and fails on redirect.

**Fix (priority order):**

1. Store only **`TempData["OrderId"] = orderId`** (or success message string); GET action loads `OrderSummaryViewModel` from `_orders.GetSummary(orderId)`.
2. If you must pass rich validation state across redirect, use **`SaveModelStateToTempData` / `Keep`** patterns sparingly or third-party helpers — still watch size; prefer redisplay without redirect for validation failures.
3. Switch to **session-based TempData with distributed session** only when flash payload is genuinely large and shared session already exists — not as default for view models.

**Production takeaway:** TempData is a flash channel, not a DTO transport — Karat tests whether you know cookie header limits.

---

#### Q8. (R) AJAX partial update expects TempData flash on the same request cycle.

**Answer:** TempData is designed for **the next HTTP request** after a redirect (or explicitly preserved keys); a partial view returned in the same POST response does not automatically surface TempData to a layout that rendered on the initial full page load, and no redirect occurs to trigger the usual flash lifecycle.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifecycle | TempData written on POST, layout already rendered | Toast never appears in DOM |
| AJAX | Partial HTML replaces fragment only | Layout TempData block not re-executed |
| Pattern mismatch | PRG flash vs in-place update | Wrong messaging mechanism |

**Fix (priority order):**

1. Return toast text in JSON: `{ "html": "...", "message": "Approved." }` and show via JavaScript.
2. Or include a `<div class="toast">` inside the partial itself when returning HTML.
3. Reserve TempData for full-page POST → redirect → GET flows; use response body or SignalR for SPA/partial updates.

**Production takeaway:** TempData + AJAX is a category error — same request needs explicit response payload, not cross-request flash storage.

---

#### Q9. (M) `TempData.Keep()` vs `TempData.Peek()` — layout reads once, child view reads same key same request, gone next request.

**Answer:** **`Peek`** reads without marking the key for deletion at end of request; **`Keep`** marks a key to survive **into the next request** after it would otherwise be consumed.

**Scenario walkthrough:**

```csharp
// Action after redirect set TempData["Status"] = "Saved";

// _Layout.cshtml — first read
var status = TempData.Peek("Status") as string; // still available this request

// Details.cshtml — second read same request
var status2 = TempData["Status"] as string;     // still works (not yet consumed)

// End of request: without Keep(), key would be marked deleted after first true read
// TempData.Keep("Status") — would ALSO show on NEXT request (usually wrong for flash)
```

- **Same request, multiple readers:** use **`Peek`** for all but the final consumer, or read once in a shared partial.
- **Next request too:** `Keep("Status")` — rare; flash should normally die after display (use Keep only when redirect chain needs two GETs).
- Default indexer read **`TempData["key"]`** marks for deletion at end of current request after first read (implementation marks on read).

**Production takeaway:** Peek = same-request idempotent read; Keep = extend life one more round-trip — confusing them causes duplicate banners or vanished messages.

---

#### Q10. (R) Register action — duplicate submission, lost validation, cross-request messaging.

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
```

**Answer:** Successful POST returns **200 + View** instead of redirect — browser refresh **re-submits the POST** and may create duplicate accounts; `ViewBag.Message` does not survive a redirect and is wrong for flash messaging across PRG.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| PRG | `return View("Confirmation")` after POST | Refresh duplicates registration |
| State | ViewBag only for current request | Message lost if user navigates away and back |
| TempData | Not used for success flash | Inconsistent with rest of app patterns |
| Validation | Invalid path OK (`return View(model)`) | Success path is the defect |

**Fix (priority order):**

1. After success: `TempData["Message"] = "Account created."; return RedirectToAction(nameof(Confirmation));`
2. Add `[HttpGet] Confirmation()` that returns the view; layout reads TempData once.
3. Keep validation failure as `return View(model)` without redirect — ModelState stays intact.
4. Optional: `[ValidateAntiForgeryToken]` on POST; idempotency key for high-risk creates.

**Production takeaway:** ViewBag on a POST success view is a double fault — no PRG and no cross-request flash; Karat stacks both in one snippet.
