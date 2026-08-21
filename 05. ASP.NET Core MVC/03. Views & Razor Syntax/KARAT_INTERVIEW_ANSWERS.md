# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/03. Views & Razor Syntax`

---

#### Q1. (R) A product review page renders user-submitted comments. QA passes with test data; security scan flags stored XSS. Review this Razor fragment — what is wrong and how do you fix it without breaking allowed rich text?

**Answer:** `@Html.Raw` bypasses Razor's HTML encoding, so any `<script>` or event-handler markup stored in `comment.Body` executes in the victim's browser — classic stored XSS. Default `@comment.Body` is safe for plain text but still wrong if you intentionally allow a subset of HTML.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `@Html.Raw` on untrusted DB content | Stored XSS — account takeover, session theft |
| Data handling | No sanitization at write or read | Malicious payload persists and affects every viewer |
| Design | Conflating "rich text" with "raw HTML" | Team thinks Razor "fixed" XSS by using Raw for formatting |

**Fix (priority order):**

1. **Plain text comments:** Render with `@comment.Body` (encoded) — never `Raw` on user input.
2. **Allowed rich text:** Sanitize server-side on save (and optionally on read) with an allowlist HTML sanitizer (e.g. Ganss.XSS / HtmlSanitizer) — strip `<script>`, `onerror=`, `javascript:` URLs; then you may use `Raw` on the **sanitized** output only.
3. Encode at the trust boundary: treat DB comment bodies as untrusted even from "internal" users.
4. Add CSP headers and HttpOnly cookies as defense in depth — not a substitute for encoding/sanitization.

**Production takeaway:** Razor `@` expressions HTML-encode by default; `@Html.Raw` is an explicit opt-out — use only on trusted or sanitized content. Karat pairs this with "QA passed" to test whether you trust happy-path test data over threat modeling.

---

#### Q2. (R) A teammate "fixes" XSS by switching to `@comment.Body` but adds this analytics hook. Pen testers report script execution from a display name. Diagnose the encoding-context mistake.

**Answer:** HTML encoding (`@Model.CustomerName` in attribute/text context) does not make values safe inside **JavaScript** string literals. A name like `'); alert(document.cookie);//` breaks out of the `'...'` string in `onclick` and runs arbitrary script — an encoding-context XSS.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | User data embedded in inline JS without JS encoding | XSS via `CustomerName` even when HTML encoding works elsewhere |
| Context | HTML encoder ≠ JavaScript string encoder | "Fixed" XSS in body but opened a worse vector in attributes |
| Design | Inline `onclick` with interpolated user data | Hard to audit; duplicates across views |

**Fix (priority order):**

1. Remove inline handlers; attach listeners in a script block using **`Json.Serialize(Model.CustomerName)`** (or `JsonSerializer.Serialize`) for JS string contexts — JSON encoding is correct for JS literals.
2. Prefer **`data-*` attributes** + one external script: `<button data-customer="@Model.CustomerName" data-total="@Model.OrderTotal">` and read with `element.dataset` (still encode if injecting into JS strings).
3. Never concatenate user input into `<script>` or event-handler attributes manually.
4. Use tag helpers / Content Security Policy (`script-src`) to limit inline script execution.

**Production takeaway:** Karat tests **encoding context** — HTML, JavaScript, URL, and CSS each need the right encoder. `@` in Razor is HTML-safe, not JavaScript-safe.

---

#### Q3. (R) Pricing rules live in the view because "it's just display math." After a tax-rate change, invoices are wrong in production but unit tests on the service pass. Review this `_InvoiceLine.cshtml` partial — what breaks and where should this logic live?

**Answer:** Business rules (volume discount thresholds, tax rates, exemptions) duplicated in the view are invisible to service-layer tests and drift from API/PDF/email calculations. The view renders one number while checkout and reporting use different code paths — silent revenue and compliance bugs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Pricing/tax logic in `.cshtml` | Untested, unreusable across PDF, API, batch jobs |
| Maintainability | Magic numbers (`0.15m`, `0.0825m`) in view | Tax change requires hunting every partial |
| Correctness | `lineTotal` computed only at render time | Email template or export using service math disagrees with UI |

**Fix (priority order):**

1. Move calculation to a **domain/service** method (e.g. `InvoiceLineCalculator.Compute(line)` or enrich `InvoiceLineViewModel` in the controller/mapper with `LineTotal`, `Tax`, `Discount`).
2. View becomes display-only: `@Model.LineTotal.ToString("C")` — no `@{}` business block.
3. Unit-test the calculator with table-driven cases (quantity 9 vs 10, tax-exempt flag).
4. Single source of truth for rates: `IOptions<TaxSettings>` or database-driven rates injected into the service, not hard-coded in Razor.

**Production takeaway:** Views should **format and layout**, not decide money. Karat uses "service tests pass" to trap candidates who test only the layer they own.

---

#### Q4. (R) A layout renders the page title from `ViewData`. Some pages show a blank `<title>` with no exception. Review the controller and layout — what fails silently and how do you prevent recurrence?

**Answer:** `ViewData["Titel"]` and `ViewData["Title"]` are unrelated keys — the typo writes a value the layout never reads, so `(ViewData["Title"] as string ?? "MyApp")` falls through to the default or empty cast without throwing. Magic strings fail at runtime with no compile-time check.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Maintainability | Typo in ViewData key `"Titel"` vs `"Title"` | Wrong or default `<title>` — SEO, tabs, accessibility |
| Type safety | `ViewData` is `object`- keyed dictionary | No IntelliSense; refactors don't rename keys |
| Observability | Silent fallback masks bug | Production pages ship with generic title |

**Fix (priority order):**

1. **Strongly typed view model** with `Title` property — compiler catches renames.
2. If keeping ViewData temporarily, **shared constants**: `ViewDataKeys.Title` used in both controller and layout.
3. `@model PageViewModel` with `[ViewData]` attribute or `_ViewStart` setting base title pattern.
4. Integration test: GET `/Home/About` asserts `<title>` contains "About Us".

**Production takeaway:** ViewData/ViewBag magic strings are a common Karat trap — prefer strongly typed models for anything that must be consistent across controller and view.

---

#### Q5. (D) A dashboard action currently passes six `ViewBag` properties and three `ViewData` keys to one view. The team debates a strongly typed `DashboardViewModel`. When is the refactor worth it, and when is dynamic view data still acceptable?

**Answer:** Refactor to `DashboardViewModel` when the view depends on multiple named pieces of data, you need compile-time safety, or the same shape is reused across actions/tests. Dynamic `ViewBag`/`ViewData` is acceptable only for trivial, single-key throwaway pages or layout cross-cutting keys with documented conventions.

- **Worth strongly typed:** Dashboard with 6+ properties, partial views expecting specific keys, API + MVC sharing shape, frequent refactors, multiple developers — eliminates magic-string bugs (see Q4) and enables view `@model` IntelliSense.
- **Acceptable dynamic:** One-off admin diagnostic page, prototype spike, or passing a single optional banner message from a filter where a shared `LayoutViewModel` would be heavy.
- **Middle ground:** `ViewComponent` with its own view model for `RecentOrders` and `AlertCount` — keeps action slim and boundaries clear.
- **Cost of delay:** Every new `ViewBag.Foo` increases coupling; partials casting `ViewBag.RecentOrders as IEnumerable<Order>` fail silently when null.

**Production takeaway:** Karat favors judgment — not "never ViewBag," but "this dashboard crossed the threshold where untyped dictionaries are tech debt."

---

#### Q6. (M) After deploy to Production, first page load is fast but subsequent edits to `.cshtml` files on the server appear immediately without redeploy. Staging behaves the same. Review this `Program.cs` / project setup — what mechanism is active, and why is it a production risk?

**Answer:** `AddRazorRuntimeCompilation()` compiles `.cshtml` from disk on demand (with file watching), so hot-editing views on the server works without redeploy. That is desirable in Development only; in Production it exposes compilation overhead, file-system dependency, and a **code injection surface** if an attacker can write to the views directory.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Runtime compile of changed `.cshtml` on server | Unauthorized view change → RCE-equivalent markup/logic in app process |
| Performance | Roslyn compile on cache miss / file change | CPU spikes, slower cold requests vs precompiled views |
| Deployment | Views expected to be immutable artifacts | Drift between nodes if one server’s files differ |
| Configuration | No `if (env.IsDevelopment())` guard | Same behavior in Staging/Production |

**Fix (priority order):**

1. Register runtime compilation **only in Development**:

   ```csharp
   var mvc = builder.Services.AddControllersWithViews();
   if (builder.Environment.IsDevelopment())
       mvc.AddRazorRuntimeCompilation();
   ```

2. Production: rely on **Razor precompilation at publish** (default in Release) and redeploy to change views.
3. Remove package `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` from Production deployments if unused.
4. Ensure CI publishes immutable artifacts — no manual `.cshtml` edits on servers.

**Production takeaway:** If views change without redeploy, runtime compilation is almost certainly enabled — treat that as a misconfiguration, not a feature.

---

#### Q7. (R) A developer centralizes "helper logic" in a view using `@functions` because partial views felt heavy. Under load, SQL time appears in view-render traces. Review this snippet — what is wrong?

**Answer:** `@functions` blocks belong to the view class for **presentation helpers** (formatting, CSS class mapping) — not data access. Injecting `AppDbContext` and querying inside `@functions` puts I/O in the view layer, bypasses controller/service testing, and runs per render with no clear transaction boundary.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Architecture | EF query in `.cshtml` via `@functions` | Business/data layer leak; untestable without view engine |
| Performance | `ToListAsync` per category page render | DB load under traffic; no caching seam |
| DI/lifetime | DbContext in view with `@inject` | Works scoped per request but hides N+1 if partial repeats |
| Maintainability | Async logic in generated view class | Harder to mock than service interface |

**Fix (priority order):**

1. Load products in **controller or view model factory**: `return View(new CategoryPageViewModel { Products = await _productService.GetByCategoryAsync(id) });`
2. View only iterates: `@foreach (var p in Model.Products)`.
3. Use **ViewComponent** if the query is reusable widget logic with its own view model — still no DbContext in `.cshtml`.
4. Reserve `@functions` for pure helpers, e.g. `string StatusBadgeClass(string status)`.

**Production takeaway:** `@functions` abuse is a Karat code-review signal — if SQL appears in view traces, move data up one layer.

---

#### Q8. (R) Five list views each contain the same 25-line status-badge markup with slightly different CSS classes. A bug fix in one view is missed in the others. What pattern fixes duplication, and what Razor feature misuse often causes this drift?

**Answer:** Copy-paste across `.cshtml` files diverges because partial extraction or view components were skipped "to save time." Centralize markup in a **partial view** or **View Component** with a small view model (`StatusBadgeViewModel` with `Status`, `AgeDays`, optional `CssVariant`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Maintainability | Identical switch + overdue rule in five views | Bug fixed in one place only — inconsistent UI |
| Design | Inline `@{}` logic blocks instead of shared partial | No single test surface for badge rules |
| DRY | Slightly different CSS per page copied wholesale | Fear of breaking one page blocks refactor |

**Fix (priority order):**

1. Extract `_StatusBadge.cshtml` partial or `StatusBadgeViewComponent` accepting `StatusBadgeModel`.
2. Replace duplicated blocks with `<partial name="_StatusBadge" model="..." />` or `@await Component.InvokeAsync("StatusBadge", ...)`.
3. Move `badgeClass` switch and overdue rule to one file (or tag helper `asp-status-badge`).
4. Add a snapshot/Razor test or single unit test for badge class mapping.

**Production takeaway:** Duplicate view logic is a maintenance defect — Karat expects **partial vs View Component** trade-off: partial for simple markup; View Component when independent data loading or encapsulation is needed.

---

#### Q9. (P) Production throws `InvalidOperationException: The view 'Index' was not found` for `Home/Index` after CI publish, but `dotnet run` locally finds the view. The pipeline runs `dotnet publish -c Release -o ./out` and copies only `./out` to the server. What publish/view-layout mistakes cause this, and what do you verify in the artifact?

**Answer:** `dotnet run` uses project source (including `Views/` on disk); publish output may contain **precompiled assemblies only** or an incomplete view tree depending on `.csproj` settings, paths, and what the deploy step copies. The runtime searches `Views/Home/Index.cshtml` and compiled view locations — if neither exists in `./out`, you get "view not found."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Publish | `<CopyRazorGenerateFilesToPublishDirectory>false</CopyRazorGenerateFilesToPublishDirectory>` (default) + precompilation failure | No `.cshtml` and no compiled view in output |
| Layout | Views outside `Views/` convention or wrong area path | Works locally with custom content paths; fails on server |
| Deploy | Copying trimmed artifact missing `Views` or `*.Views.dll` | Intermittent "works on my machine" |
| Case sensitivity | `Index.cshtml` vs `index.cshtml` on Linux | Windows dev OK; Linux prod 404 view |

**Fix (priority order):**

1. Inspect publish folder: presence of **`ProjectName.Views.dll`** (precompiled) **or** `Views/Home/Index.cshtml` if copying cshtml to publish.
2. Ensure `.csproj` does not exclude views: check `<Content Remove="Views/**" />`, `<None Remove=...>`, wrong `RazorCompileOnPublish`.
3. Fix view path: action returns `View()` → expects `Views/Home/Index.cshtml` (or explicit `return View("~/Views/...")`).
4. Match Linux casing; run `dotnet publish` locally and execute from `./out` before CI promote.
5. If using runtime compilation in prod (see Q6), package must include `.cshtml` files on disk.

**Production takeaway:** Always validate **`dotnet publish` output**, not `dotnet run` source tree — Karat ties MVC view errors to deployment artifacts.

---

#### Q10. (M) Release builds use Razor precompilation by default in modern SDK-style projects. Explain what happens to `.cshtml` files at **build/publish** vs **first request** when precompilation is enabled, and how that differs from runtime compilation.

**Answer:** With **Razor precompilation** (`RazorCompileOnBuild` / `RazorCompileOnPublish`, default true for publish), `.cshtml` files are compiled into assemblies (e.g. `MyApp.Views.dll`) at build/publish time. At runtime the view engine loads precompiled types — **no Roslyn compile on first request**; `.cshtml` may not be deployed to the server at all.

- **Build/publish (precompiled):** Razor SDK generates C# from views → compiled into views assembly; errors surface at build time; faster startup and steady-state render.
- **First request (precompiled):** View locator finds compiled view type — disk `.cshtml` optional unless configured to copy for editing.
- **Runtime compilation (`AddRazorRuntimeCompilation`):** Original `.cshtml` on disk parsed/compiled when needed; file watcher recompiles on change — Development workflow, not Production default.
- **Contrast:** Precompilation = immutable views in DLL; runtime compilation = views are source code at runtime.

**Production takeaway:** Know which mode your pipeline uses — "change cshtml on server" (Q6) means runtime compilation; proper Release deploy means **redeploy DLL** to change views.

---

#### Q11. (R) A category page renders 200 products; each row invokes a synchronous partial that hits `_pricingService.GetTierPrice(productId)` inside the partial. TTFB spikes under concurrent users. Review this view pattern — what performance issues stack here, and what is the prioritized fix?

**Answer:** The view triggers **O(n) synchronous service/DB calls** — one per row — via `Html.PartialAsync` rendering a partial that calls `GetTierPrice` inside `@{}`. Partials are fine for markup reuse but amplify N+1 when each invocation does I/O; async partial doesn't help if the service call is sync and repeated 200 times per request.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | 200 × `GetTierPrice` per page | N+1 queries; thread pool blocked on sync I/O |
| Architecture | Pricing lookup inside partial | Hidden from controller profiling; duplicated if row reused |
| Razor | `@foreach` + partial per item with `@inject` service | Compiles but scales linearly with catalog size |
| Caching | No batch API | Cache misses multiply latency |

**Fix (priority order):**

1. **Batch in controller/service** before render: `var prices = await _pricing.GetTierPricesAsync(productIds);` map into `ProductRowViewModel.Price`.
2. View/partials display only: `@product.DisplayPrice` — zero service calls in `.cshtml`.
3. Replace per-row partial with inline markup or single partial taking **precomputed** `ProductRowViewModel` if markup reuse needed.
4. If pricing must stay dynamic, add **bulk endpoint + cache** (`IMemoryCache` keyed by product set hash); never sync DB in loop.
5. Measure with MiniProfiler — confirm one query (or one cache round-trip) per page after fix.

**Production takeaway:** Complex Razor performance issues are usually **N+1 I/O disguised as partial reuse** — fix data shape before micro-optimizing Razor syntax.

---
