# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/04. Layouts, Sections & Partial Views`

---

#### Q1. (R) Review this view setup. The About page renders raw HTML with no site navigation, CSS, or footer — Home/Index and Contact look correct.

**Answer:** A nested `Views/Home/_ViewStart.cshtml` sets `Layout = null`, which overrides the root `_ViewStart` for every view under `Views/Home/` — About inherits that null layout and renders without `_Layout.cshtml` even though other folders still use the root default.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| View pipeline | Child `_ViewStart` sets `Layout = null` | All Home views skip layout — missing chrome and CSS |
| Cascade | `_ViewStart` runs closest-folder-first | Easy to miss during code review — only Home folder affected |
| Maintainability | "Experiment" file left in repo | Intermittent layout bugs by folder path |

**Fix (priority order):**

1. Remove `Views/Home/_ViewStart.cshtml` or change it to `Layout = "_Layout"` if Home needs the same chrome.
2. If Home truly needs a different layout, set `Layout = "_HomeLayout"` explicitly — not `null` — and ensure that layout exists in `Views/Shared/`.
3. Document `_ViewStart` cascade in team conventions: only one root `_ViewStart` unless a subfolder deliberately overrides.

**Production takeaway:** `_Layout not applied` is often a `_ViewStart` cascade bug, not a missing layout file — Karat tests whether you trace the view start chain before blaming the view itself.

---

#### Q2. (R) Review this layout and checkout view. The page works in dev until marketing adds a new checkout step — then some pages throw at runtime and others silently omit analytics scripts.

**Answer:** `@RenderSection("Scripts", required: true)` throws `InvalidOperationException` when Confirm.cshtml omits the section, while `required: false` on HeadScripts silently skips analytics with no error — the `required` flag is the trap.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Scripts` section marked `required: true` | Confirm page crashes when section undefined |
| Silent omission | `HeadScripts` is `required: false` | Missing analytics/SEO tags with no exception |
| Consistency | Mixed required flags across checkout flow | Some steps fail loudly, others fail quietly |

**Fix (priority order):**

1. Decide contract: if every page needs footer scripts, keep `required: true` and add `@section Scripts { }` (even empty) to every view — or default scripts in layout and use `required: false` with `@if (IsSectionDefined("Scripts"))`.
2. For optional HeadScripts, use `required: false` but document which pages must define it; consider a shared partial `_AnalyticsScripts.cshtml` included from layout instead of a section.
3. Add integration/smoke tests that hit every checkout view — section errors only appear at render time.

**Production takeaway:** `required: false` is not "optional convenience" in prod — it means content can vanish with zero signal; `required: true` means one forgotten section takes down a page.

---

#### Q3. (D) A product dashboard needs a reusable "Recent Orders" panel on three pages. It runs a scoped repository query, shows a loading skeleton, and must be unit-testable without spinning up the full layout pipeline. The team proposes `@await Html.PartialAsync("_RecentOrders")` with data stuffed into `ViewBag.Orders`. What would you choose instead, and why?

**Answer:** Use a **View Component** (`RecentOrdersViewComponent` + `InvokeAsync`) — partials are synchronous rendering fragments with no invocation lifecycle, while view components support async data loading, explicit parameters, DI, and isolated testing.

- **Partial** fits static markup or data the parent view already fetched; `ViewBag` is untyped, not refactor-safe, and hides dependencies.
- **View Component** injects `IOrderRepository`, runs `InvokeAsync(int count)` async, returns a strongly typed view (`Default.cshtml`), and can be invoked via `<vc:recent-orders count="5" />` or `@await Component.InvokeAsync(...)`.
- Unit-test the component class directly; no layout or full Razor page required.
- Reserve partials for small, parent-supplied fragments (`_ValidationScriptsPartial`); use view components when the widget owns its query or state.

**Production takeaway:** Karat uses partial-vs-component to test whether you reach for the right reuse primitive — partials render; view components **execute**.

---

#### Q4. (M) An admin section uses nested layouts: site chrome in `_Layout.cshtml`, admin sidebar in `_AdminLayout.cshtml`, and page content in individual views. Walk through how Razor resolves `Layout`, `@RenderBody()`, and `@section` definitions across the two layout files — where does each `@RenderSection` call execute?

**Answer:** Razor builds an inside-out chain: Index content fills `_AdminLayout`'s `@RenderBody()`, then `_AdminLayout`'s output (including forwarded sections) fills `_Layout`'s `@RenderBody()` — child `@section Scripts` is captured once and forwarded by the inner layout's `@section Scripts { @RenderSection("Scripts", required: false) }`.

Execution order:

1. **Index.cshtml** — defines page markup and `@section Scripts { reports.js }`.
2. **`_AdminLayout.cshtml`** — `Layout = "_Layout"`; its `@RenderBody()` is replaced by Index body; its `@section Scripts` block **forwards** the child Scripts section upward (required pattern for nested layouts).
3. **`_Layout.cshtml`** — `@RenderBody()` receives the fully rendered admin shell (sidebar + main); `@RenderSection("Scripts")` at the bottom outputs the forwarded scripts once.

If `_AdminLayout` omits the `@section Scripts { @RenderSection(...) }` forwarder, scripts defined in Index never reach `_Layout` — a common nested-layout bug. Sections are **not** automatically bubbled through intermediate layouts.

**Production takeaway:** Nested layouts require explicit section forwarding in every intermediate layout — one missing `@RenderSection` bridge drops scripts or styles silently.

---

#### Q5. (R) Review this view. Build fails locally with a Razor compilation error; the developer claims "the second Scripts block should merge."

**Answer:** Razor allows each section name **once per view** — two `@section Scripts` blocks do not merge; the compiler reports a duplicate section definition error at build time.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `@section Scripts` defined twice in same view | Build fails — no merge semantics |
| Design | Assumption that sections accumulate like `@Html.Partial` calls | Blocks deployment until consolidated |

**Fix (priority order):**

1. Merge into one section:

```cshtml
@section Scripts {
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
    <script src="~/js/product-editor.js"></script>
}
```

2. Or extract shared scripts to a partial `_ProductEditScripts.cshtml` and invoke once inside a single section.
3. For nested layouts, forward with one `@RenderSection` per layout level — still one definition per **view**, not per layout file.

**Production takeaway:** "Section defined twice" fails at compile time — unlike HTML `<script>` tags, Razor sections have strict single-definition rules.

---

#### Q6. (R) Review this Area view. The partial renders empty — no order lines — even though the controller passed a populated model. Same partial works on a non-Area page.

**Answer:** `<partial name="_LineItems" />` resolves from the **calling view's** search paths — for an Area view, discovery order includes `Areas/Billing/Views/Shared/`, then `Areas/Billing/Views/Invoices/`, then **application** `Views/Shared/` — if the wrong file wins or the stub in root Shared matches first in some configurations, you get empty/wrong markup; explicit paths avoid ambiguity.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resolution | Implicit partial name without path | May bind to `Views/Shared/_LineItems.cshtml` stub instead of Area version |
| Areas | Area view location expander adds paths but order matters | Billing-specific partial ignored — empty or generic output |
| Model | Partial receives `model="Model.Lines"` | If wrong partial `@model` type differs, binding may render nothing |

**Fix (priority order):**

1. Use explicit path: `<partial name="~/Areas/Billing/Views/Shared/_LineItems.cshtml" model="Model.Lines" />`.
2. Or rename Area partial to `_BillingLineItems.cshtml` to avoid name collision with root Shared.
3. Prefer view components for Area-specific widgets — `View()` discovery uses `[AreaViewLocationFormats]`.

Partial search order (simplified): current view folder → `{Area}/Views/Shared` → `Views/Shared`. Same name in root Shared can shadow Area intent when developers assume Area isolation is automatic.

**Production takeaway:** Partial path resolution is view-context-relative — Area pages do not magically ignore root `Views/Shared` duplicates with the same name.

---

#### Q7. (P) You inherit an MVC app where `Areas/Admin/Views/Shared/_Layout.cshtml` exists but Admin pages still use the root `_Layout` and break relative asset paths (`../css/admin.css` 404). Trace the layout resolution path for Area views and what to fix in `_ViewStart` / view `Layout` assignments.

**Answer:** `Areas/Admin/Views/_ViewStart.cshtml` sets `Layout = "_Layout"`, which resolves via view location expanders to **`Views/Shared/_Layout.cshtml`** (root) — the Area-local `_Layout.cshtml` is never selected unless you use a distinct name or fully qualified path.

Resolution path for `Areas/Admin/Views/Dashboard/Index.cshtml`:

1. Run `Areas/Admin/Views/_ViewStart.cshtml` → `Layout = "_Layout"`.
2. Razor searches layout locations: `Areas/Admin/Views/Shared/_Layout.cshtml`, then `Views/Shared/_Layout.cshtml` — **first match wins**; if both exist, Area Shared should win, but identical names with wrong content or a typo in Area file causes root to match.
3. Root layout references `~/css/site.css`; admin assets in Area layout use paths that 404 when root layout renders.

Fixes:

- Set Area `_ViewStart` to `Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml"` or rename to `_AdminLayout` and reference explicitly.
- Use `~/css/admin.css` (application-root-relative) — never `../css/admin.css` in layouts.
- Verify `AddMvc()` / `AddControllersWithViews()` registers Area route `{area:exists}/...`.

**Production takeaway:** Layout in Areas requires explicit Area `_ViewStart` — an unused `Areas/Admin/Views/Shared/_Layout.cshtml` does nothing until the cascade points to it.

---

#### Q8. (P) A catalog page renders 40 product tiles; each tile is a partial that injects `IProductService` and calls `GetRatingAsync(productId)` — 40 service calls per page load. Review the architecture: what breaks under load, and what MVC/Razor pattern reduces work without abandoning partial reuse?

**Answer:** Forty async partials each hit the database independently — classic **N+1 at the view layer** — causing latency spikes, connection-pool pressure, and thread churn; partials have no batching and duplicate DI work per invocation.

- **Symptom under load:** p95 page time scales with tile count; DB QPS = tiles × concurrent users.
- **Fix (priority):** Controller or view-model builder loads all ratings in **one query** (`GetRatingsAsync(productIds)`) and maps into `ProductSummary.Rating` before the view runs — partial becomes dumb markup only.
- **Alternative:** View Component with batched fetch keyed by ids on first invoke (still prefer controller/service layer batching).
- **Partial reuse kept:** `_ProductTile.cshtml` still renders `@Model.Stars` — data fetched upstream.
- Avoid `@inject` + async DB in partials for list items; cache ratings at service layer if hot path.

**Production takeaway:** Performance of many partials is not about Razor compilation — it is about **data access inside each partial**; Karat tests moving queries out of the view tree.

---

#### Q9. (R) Review this new layout copied from a static HTML template. Views compile but render blank white pages in the browser — no exception in logs.

**Answer:** `_MarketingLayout.cshtml` never calls `@RenderBody()`, so view content (including the `<p>Sign up today.</p>` outside sections) is discarded — only partials and optional sections render, producing an nearly empty page with no error.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Layout contract | Missing `@RenderBody()` | Primary view markup never appears — blank main content |
| Sections only | Content outside `@section` blocks ignored | Body text in Index lost unless inside a section |
| Debugging | No exception — valid Razor | Silent functional bug, hard to spot in QA |

**Fix (priority order):**

1. Add `@RenderBody()` where main content belongs (between header and footer):

```cshtml
@await Html.PartialAsync("_MarketingHeader")
@RenderSection("Hero", required: false)
@RenderBody()
@await Html.PartialAsync("_MarketingFooter")
```

2. Move inline page copy into `@RenderBody()` placement or into a `@section Hero` / main section intentionally.
3. Smoke-test new layouts with minimal view containing non-section content to verify body renders.

**Production takeaway:** Missing `@RenderBody()` is the layout equivalent of a controller action that never returns the model — compile succeeds, output is wrong.

---
