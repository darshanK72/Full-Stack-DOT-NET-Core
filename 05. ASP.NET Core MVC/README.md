# 05. ASP.NET Core MVC

MVC pattern, controllers, views, Razor, model binding, validation, tag helpers, routing, areas.

## Topics

| # | Topic | Questions | Q&A File |
|---|-------|-----------|----------|
| 01 | Introduction to MVC Pattern | 28 | [README.md](./01.%20Introduction%20to%20MVC%20Pattern/README.md) |
| 02 | Controllers & Actions | 30 | [README.md](./02.%20Controllers%20%26%20Actions/README.md) |
| 03 | Views & Razor Syntax | 29 | [README.md](./03.%20Views%20%26%20Razor%20Syntax/README.md) |
| 04 | Layouts, Sections & Partial Views | 27 | [README.md](./04.%20Layouts,%20Sections%20%26%20Partial%20Views/README.md) |
| 05 | ViewModels & Strongly Typed Views | 28 | [README.md](./05.%20ViewModels%20%26%20Strongly%20Typed%20Views/README.md) |
| 06 | Model Binding in MVC | 29 | [README.md](./06.%20Model%20Binding%20in%20MVC/README.md) |
| 07 | Data Annotations & Validation | 30 | [README.md](./07.%20Data%20Annotations%20%26%20Validation/README.md) |
| 08 | Tag Helpers | 27 | [README.md](./08.%20Tag%20Helpers/README.md) |
| 09 | Routing & Attribute Routing | 29 | [README.md](./09.%20Routing%20%26%20Attribute%20Routing/README.md) |
| 10 | Areas | 28 | [README.md](./10.%20Areas/README.md) |
| 11 | Action Filters in MVC | 30 | [README.md](./11.%20Action%20Filters%20in%20MVC/README.md) |
| 12 | TempData, ViewData & ViewBag | 28 | [README.md](./12.%20TempData,%20ViewData%20%26%20ViewBag/README.md) |
| 13 | AJAX & Partial Page Updates | 30 | [README.md](./13.%20AJAX%20%26%20Partial%20Page%20Updates/README.md) |
| 14 | Client-Side Validation | 28 | [README.md](./14.%20Client-Side%20Validation/README.md) |
| 15 | Real-Time UI with SignalR | 29 | [README.md](./15.%20Real-Time%20UI%20with%20SignalR/README.md) |

---

> Each Q&A file has a **Table of Contents** at the top linking to every question.
> Files that include scenario-based Karat questions have a **Scenario-Based Questions** section at the bottom.

---

# 05. ASP.NET Core MVC — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [Introduction to MVC Pattern](01.%20Introduction%20to%20MVC%20Pattern/README.md) | MVC separation of concerns, request lifecycle, and how model, view, and controller collaborate in ASP.NET Core |
| 02 | [Controllers & Actions](02.%20Controllers%20%26%20Actions/README.md) | Routing requests to action methods, return types (`IActionResult`, `ActionResult<T>`), and the action execution context |
| 03 | [Views & Razor Syntax](03.%20Views%20%26%20Razor%20Syntax/README.md) | Razor template engine, C# expressions in views, HTML encoding, and view discovery |
| 04 | [Layouts, Sections & Partial Views](04.%20Layouts%2C%20Sections%20%26%20Partial%20Views/README.md) | Shared layout pages, `@RenderBody`/`@RenderSection`, and reusable partial view components |
| 05 | [ViewModels & Strongly Typed Views](05.%20ViewModels%20%26%20Strongly%20Typed%20Views/README.md) | Purpose-built view models, the `@model` directive, and type-safe data passing from controller to view |
| 06 | [Model Binding in MVC](06.%20Model%20Binding%20in%20MVC/README.md) | How HTTP request data maps to action parameters, binding sources, and over-posting prevention |
| 07 | [Data Annotations & Validation](07.%20Data%20Annotations%20%26%20Validation/README.md) | Declarative validation attributes, `ModelState`, `IValidatableObject`, and custom validators |
| 08 | [Tag Helpers](08.%20Tag%20Helpers/README.md) | Server-side helpers that generate HTML — `asp-for`, `asp-action`, form tag helpers, and custom tag helpers |
| 09 | [Routing & Attribute Routing](09.%20Routing%20%26%20Attribute%20Routing/README.md) | Conventional and attribute-based routes, route constraints, route order, and `IRouteConstraint` |
| 10 | [Areas](10.%20Areas/README.md) | Organizing large applications into feature areas with separate controllers, views, and routes |
| 11 | [Action Filters in MVC](11.%20Action%20Filters%20in%20MVC/README.md) | Filter pipeline (authorization → resource → action → exception → result), filter scope, and short-circuiting |
| 12 | [TempData, ViewData & ViewBag](12.%20TempData%2C%20ViewData%20%26%20ViewBag/README.md) | Lifetime and scope differences; cross-redirect data survival with `TempData` |
| 13 | [AJAX & Partial Page Updates](13.%20AJAX%20%26%20Partial%20Page%20Updates/README.md) | Returning partial views via AJAX, JSON responses, and updating page fragments without full reloads |
| 14 | [Client-Side Validation](14.%20Client-Side%20Validation/README.md) | Unobtrusive validation, jQuery Validate integration, and custom client-side validators |
| 15 | [Real-Time UI with SignalR](15.%20Real-Time%20UI%20with%20SignalR/README.md) | Hub configuration, connection management, broadcasting, and backplane for scale-out |

---

## Table of Contents
- [CQ1. How does the model binding → data annotations → ViewModel round-trip work, and why does it prevent over-posting?](#cq1-how-does-the-model-binding--data-annotations--viewmodel-round-trip-work-and-why-does-it-prevent-over-posting)
- [CQ2. When should you use ViewBag, ViewData, TempData, or a ViewModel — and what breaks across redirects?](#cq2-when-should-you-use-viewbag-viewdata-tempdata-or-a-viewmodel--and-what-breaks-across-redirects)
- [CQ3. What is the MVC filter pipeline order, how do action and result filters differ, and what is the post-validation mutation trap?](#cq3-what-is-the-mvc-filter-pipeline-order-how-do-action-and-result-filters-differ-and-what-is-the-post-validation-mutation-trap)
- [CQ4. How can AJAX partial-view updates silently bypass both server-side ModelState and client-side validation?](#cq4-how-can-ajax-partial-view-updates-silently-bypass-both-server-side-modelstate-and-client-side-validation)
- [CQ5. How do SignalR hub routes interact with Area routing, and what CORS gotcha blocks only SignalR in production?](#cq5-how-do-signalr-hub-routes-interact-with-area-routing-and-what-cors-gotcha-blocks-only-signalr-in-production)
- [CQ6. How does TempData's one-request lifetime interact with redirects triggered by action filters, and when is Peek/Keep needed?](#cq6-how-does-tempdatas-one-request-lifetime-interact-with-redirects-triggered-by-action-filters-and-when-is-peekkeep-needed)
- [CQ7. What is the ambiguous-match gotcha when attribute routing and conventional routing coexist, and how does constraint precedence resolve ties?](#cq7-what-is-the-ambiguous-match-gotcha-when-attribute-routing-and-conventional-routing-coexist-and-how-does-constraint-precedence-resolve-ties)
- [CQ8. How do Tag Helpers replace HTML Helpers, what does asp-for bind to, and how do you build a custom Tag Helper that emits IHtmlContent?](#cq8-how-do-tag-helpers-replace-html-helpers-what-does-asp-for-bind-to-and-how-do-you-build-a-custom-tag-helper-that-emits-ihtmlcontent)
- [CQ9. How does _ViewStart.cshtml resolve layout selection inside an Area, and what causes the "layout could not be located" gotcha?](#cq9-how-does-_viewstartcshtml-resolve-layout-selection-inside-an-area-and-what-causes-the-layout-could-not-be-located-gotcha)
- [CQ10. Why is the @model directive required for strongly-typed views, and how do DisplayFor and EditorFor differ in their relationship to DataAnnotations?](#cq10-why-is-the-model-directive-required-for-strongly-typed-views-and-how-do-displayfor-and-editorfor-differ-in-their-relationship-to-dataannotations)
- [CQ11. How does jQuery Validate unobtrusive read data-val-* attributes from DataAnnotations, and why do custom validators need IClientModelValidator?](#cq11-how-does-jquery-validate-unobtrusive-read-data-val--attributes-from-dataannotations-and-why-do-custom-validators-need-iclientmodelvalidator)
- [CQ12. How does IExceptionFilter differ from global exception-handling middleware, and what gap does IAlwaysRunResultFilter fill that regular result filters leave?](#cq12-how-does-iexceptionfilter-differ-from-global-exception-handling-middleware-and-what-gap-does-ialwaysrunresultfilter-fill-that-regular-result-filters-leave)
- [CQ13. When should an AJAX action return PartialViewResult vs JsonResult, how is AJAX detected reliably, and why does AJAX break the PRG pattern?](#cq13-when-should-an-ajax-action-return-partialviewresult-vs-jsonresult-how-is-ajax-detected-reliably-and-why-does-ajax-break-the-prg-pattern)
- [CQ14. How does Area route registration order interact with conventional routes, and what does the [Area] attribute actually do for controller discovery?](#cq14-how-does-area-route-registration-order-interact-with-conventional-routes-and-what-does-the-area-attribute-actually-do-for-controller-discovery)
- [CQ15. How does the model binder handle nested ViewModels and collection binding, and what is the missing-hidden-input data-corruption gotcha?](#cq15-how-does-the-model-binder-handle-nested-viewmodels-and-collection-binding-and-what-is-the-missing-hidden-input-data-corruption-gotcha)
- [CQ16. How do required vs optional @RenderSection, nested layouts, and the PartialAsync-vs-Component.InvokeAsync choice interact?](#cq16-how-do-required-vs-optional-rendersection-nested-layouts-and-the-partialasync-vs-componentinvokeasync-choice-interact)
- [CQ17. How does TempData's backing store (session vs cookie) affect its availability, and why must flash messages call Keep() in multi-step flows?](#cq17-how-does-tempdatas-backing-store-session-vs-cookie-affect-its-availability-and-why-must-flash-messages-call-keep-in-multi-step-flows)
- [CQ18. When should you use SignalR vs AJAX polling for live UI updates, and how does the hybrid notify-then-fetch pattern combine both?](#cq18-when-should-you-use-signalr-vs-ajax-polling-for-live-ui-updates-and-how-does-the-hybrid-notify-then-fetch-pattern-combine-both)

---

## CQ1. How does the model binding → data annotations → ViewModel round-trip work, and why does it prevent over-posting?

**Concepts**
- Model binding maps form fields to action parameter properties before the action executes
- Data annotations on the ViewModel run as part of binding, accumulating errors in `ModelState`
- `ModelState.IsValid` is the controller's gate before any persistence
- Over-posting: a malicious client can post fields not rendered in the form — e.g., `IsAdmin=true`
- ViewModels expose only the properties the form legitimately sets; domain entities expose everything

**Answer**

When an HTTP POST arrives, ASP.NET Core's model binder reads form fields, route values, and query strings and assigns them to matching properties on the action's parameter — which should be a ViewModel, not a domain entity. As each property is set, any data annotation attributes (`[Required]`, `[Range]`, `[StringLength]`) run and record failures in `ModelState`. The action then checks `ModelState.IsValid`; if false, it returns the view with the same ViewModel so Razor and client-side validation can display errors beside the appropriate fields.

The over-posting risk arises if a domain entity is used directly as the parameter. The binder blindly applies any field name it finds in the request — including `IsAdmin`, `AccountBalance`, or `CreatedAt` — even if those inputs were never rendered in the form. A ViewModel is a purpose-built class that exposes only the properties the form legitimately collects, so the binder has nothing to target for those sensitive fields. In .NET 10, the recommended pattern is a record-based ViewModel with `[BindNever]` on fields that the server always computes, providing a belt-and-suspenders defence alongside ViewModel scoping.

---

## CQ2. When should you use ViewBag, ViewData, TempData, or a ViewModel — and what breaks across redirects?

**Concepts**
- `ViewBag` / `ViewData`: scoped to the current request and current view; not available after a redirect
- `TempData`: stored in the session (or cookie provider) and survives exactly one subsequent request
- ViewModel: strongly typed, compile-time checked, always the preferred mechanism for view data
- `ViewBag` is `dynamic` — no IntelliSense, no compile error on a typo, silently null at runtime
- Passing complex objects via `ViewBag` across a redirect yields null — only `TempData` survives

**Answer**

`ViewBag` and `ViewData` are two surfaces over the same `ViewDataDictionary` and share the same lifetime: they exist for the current request and are available to the current view and its partials. The moment you issue a `RedirectToAction`, the entire request ends and a new one begins — `ViewBag` and `ViewData` are gone. `TempData` is different: it is serialized to the session or a signed cookie and loaded at the start of the next request, giving it exactly one cross-redirect lifetime. It is consumed (cleared) on read, which is why `Peek` lets you read without consuming and `Keep` re-marks an already-read entry for retention.

In practice, ViewModels beat all three for view data. A ViewModel is a concrete class, so the compiler catches property name typos, Razor can infer field metadata for tag helpers, and the data contract between controller and view is explicit. `ViewBag` is appropriate for one-off supplementary data — a dropdown list, a page title — where creating a ViewModel property feels disproportionate. `TempData` is the right tool for post-redirect-get confirmation messages (`"Record saved"`) because it survives the redirect that PRG requires, whereas a ViewModel populated before the redirect would be lost.

---

## CQ3. What is the MVC filter pipeline order, how do action and result filters differ, and what is the post-validation mutation trap?

**Concepts**
- Pipeline order: Authorization → Resource → (model binding) → Action → (action executes) → Result → (response written) → Exception (on error)
- Action filters wrap the action method execution; result filters wrap the `IActionResult` execution
- Action filters receive `ActionExecutingContext.ActionArguments` — the already-bound, already-validated model
- Mutating model properties in `OnActionExecuting` after `ModelState` has been populated does not re-run validation
- Result filters can inspect or replace the `IActionResult` but cannot recover from action-phase short-circuits

**Answer**

ASP.NET Core's MVC filter pipeline runs in a fixed order: authorization filters first (short-circuit to 401/403 if denied), then resource filters (can short-circuit before model binding, useful for caching), then model binding and `ModelState` population, then action filters' `OnActionExecuting`, then the action method itself, then action filters' `OnActionExecuted`, then result filters, and finally the `IActionResult` is executed (view rendered or JSON written). Exception filters fire on any unhandled exception in action or result phases.

The distinction between action and result filters is timing relative to the `IActionResult`: action filters run before and after the action method decides *what* result to return; result filters run before and after that result is *written to the response*. A result filter can swap a `ViewResult` for a `JsonResult` based on `Accept` headers, but it cannot change the model that was already passed to the view.

The post-validation trap: action filters receive `context.ActionArguments`, which contains the already-bound, already-validated model. If an `OnActionExecuting` filter modifies a property on that model — say, forcing `order.Status = "Pending"` — `ModelState` is not re-evaluated. The controller sees `ModelState.IsValid == true` even if the mutated value would have failed `[Required]` or `[Range]`. The fix is to either perform mutations in the action method after the validity check, or call `ModelState.SetModelValue` / `ModelState.MarkFieldValid` explicitly when the filter legitimately changes input.

---

## CQ4. How can AJAX partial-view updates silently bypass both server-side ModelState and client-side validation?

**Concepts**
- Partial views rendered via AJAX do not carry the parent page's jQuery Validate form context
- `$.ajax` / `fetch` posts bypass `asp-antiforgery` tag helper wiring unless added explicitly
- The server must always check `ModelState.IsValid` regardless of whether the caller is AJAX or full-page
- AJAX callers cannot display `asp-validation-summary`; errors must be returned as JSON and injected into the DOM
- The pattern: return `BadRequest(ModelState)` for AJAX callers, full view for non-AJAX callers

**Answer**

When a standard form is submitted, jQuery Validate inspects the page's validator instance and blocks the POST if any annotated field fails. But when JavaScript code issues an `$.ajax` or `fetch` POST directly — or when a partial view is injected into an existing page without being wired into that page's jQuery Validate form — client-side validation never runs. The browser simply fires the request. On the server, the model binder still populates `ModelState`, but if the controller does not explicitly check `ModelState.IsValid` and return an error response, it proceeds to save invalid data.

The correct pattern is to always check `ModelState.IsValid` in the action, then branch on the caller type. Detect AJAX with `Request.Headers["X-Requested-With"] == "XMLHttpRequest"` or a custom header. For valid data, return `Json(new { success = true, redirectUrl = ... })`. For invalid data, return `BadRequest(ModelState)` — or serialize `ModelState` errors to a JSON structure the client can iterate to highlight the offending fields. Do not attempt to return a partial view with validation summaries for AJAX callers, because the partial's `asp-validation-summary` tag helper renders server-side error spans that the client JavaScript cannot automatically activate. Antiforgery tokens must also be manually included in AJAX headers — `X-CSRF-TOKEN` — or `[ValidateAntiForgeryToken]` will reject the request with a 400 before validation even begins.

---

## CQ5. How do SignalR hub routes interact with Area routing, and what CORS gotcha blocks only SignalR in production?

**Concepts**
- SignalR hubs are mapped via `app.MapHub<T>("/path")` in `Program.cs`, independent of MVC route tables
- Area routes use the `[Area]` attribute and a conventional `{area}/{controller}/{action}` template — hubs do not participate
- The SignalR WebSocket upgrade uses the same origin policy — CORS must explicitly allow the hub URL *and* permit credentials
- `AllowAnyOrigin()` is incompatible with `AllowCredentials()` — CORS rejects the combination at startup
- The WebSocket upgrade is a separate HTTP request; CORS policy must list the hub path explicitly or via a wildcard pattern

**Answer**

SignalR hub routes are registered imperatively via `app.MapHub<ChatHub>("/chat")` in the middleware pipeline, completely separate from the MVC convention-based or attribute-based route table. Areas add a route prefix (`admin/products/index`) and a controller discovery scope, but because hubs are not controllers, an Area prefix has no automatic effect on hub paths. If you want a hub to appear under an Area's URL prefix, you must embed the prefix manually in `MapHub<T>("/admin/chat")`. There is no `[Area]` attribute for hubs and no link generator entry, so `Url.Action` and `IUrlHelper` cannot resolve hub paths.

The CORS gotcha surfaces in production when the front-end is served from a different origin than the API. Developers commonly configure `AllowAnyOrigin()` during development, which works for `fetch` calls but fails for SignalR. SignalR transports — long-polling, Server-Sent Events, and WebSocket — all send credentials (cookies or the `Authorization` header) as part of the connection negotiation. CORS requires `AllowCredentials()` when credentials are present, but `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined — ASP.NET Core throws at startup or returns a CORS rejection at runtime. The fix is to replace `AllowAnyOrigin()` with `WithOrigins("https://your-client.com")` and add `AllowCredentials()`. MVC routes continue working through this because typical API calls from JavaScript do not send cookies in cross-origin requests unless `credentials: "include"` is explicitly set; SignalR's client library always sets it.

---

## CQ6. How does TempData's one-request lifetime interact with redirects triggered by action filters, and when is Peek/Keep needed?

**Concepts**
- `TempData` is written in one request and consumed (deleted) on the first read in the next request
- Action filters can short-circuit execution and return a `RedirectResult`, consuming the redirect as one of TempData's "next requests"
- A filter-issued redirect is a full round-trip: TempData written before the redirect is available in the redirected request, consumed on read
- If the redirected action also redirects (e.g., an authorization filter chain), TempData is consumed by the *first* response that reads it — subsequent redirects see null
- `Peek` reads without marking as consumed; `Keep` re-marks an already-read entry so it survives into the next request

**Answer**

`TempData` persists across exactly one redirect by serializing to the session store (or a signed cookie). When you write `TempData["Message"] = "Saved"` and then call `RedirectToAction`, the redirected action can read that value. On read, the framework marks the entry for deletion at the end of that request — which is what "one-request-only" means.

The interaction with action filters is subtle. Suppose a global action filter runs `OnActionExecuting`, finds a business rule violation, writes `TempData["Error"] = "Subscription expired"`, and returns a `RedirectToAction` to a landing page. That redirect is the normal cross-redirect hop, so TempData works as expected — the landing page reads and displays the message. Problems arise in layered filter chains: if the landing page's own authorization filter also redirects to a login page, TempData has already been loaded into memory for the landing-page request. When the landing-page filter's redirect response is sent, TempData's read flag causes the entry to be cleared. The login page sees null.

`Peek` solves read-without-consume: `TempData.Peek("Error")` returns the value but leaves the entry in place for the next request. `Keep` re-marks a value that was already read: call `TempData.Keep("Error")` after reading to ensure it survives one additional request. Both are necessary when a multi-hop filter chain or a conditional redirect path must carry TempData farther than one hop.

---

## CQ7. What is the ambiguous-match gotcha when attribute routing and conventional routing coexist, and how does constraint precedence resolve ties?

**Concepts**
- Conventional routes are registered via `app.MapControllerRoute(...)` and matched top-down
- A controller decorated with any `[Route]` or `[HttpGet]` attribute uses attribute routing exclusively — those actions no longer participate in any conventional template
- Partial decoration (some actions attributed, others not) leaves undecorated actions unreachable — their URLs are never registered
- When two routes score equally specific for the same request URL, ASP.NET Core throws `AmbiguousMatchException` at runtime, not at startup
- Constraint specificity order: literal segment > constrained parameter (`{id:int}`) > unconstrained parameter (`{id}`) > catch-all (`{**rest}`)

**Answer**

When both conventional and attribute routing are active in the same application, ASP.NET Core does not silently pick one winner. The routing middleware scores every registered endpoint against the incoming URL and selects the best match. If a controller class or any of its actions carries a `[Route]`, `[HttpGet]`, or similar attribute, attribute routing takes full ownership of that controller — none of its actions participate in the conventional route table. This is all-or-nothing per controller: decorating only half the actions leaves the other half with no registered URL and results in 404s that can be hard to diagnose because no error is thrown at startup.

The ambiguous-match trap fires at runtime when two endpoints — whether both attribute-routed, both conventional, or one of each — produce an identical URL pattern with the same specificity. The framework throws `AmbiguousMatchException` rather than silently taking the first match, making the bug loud but sometimes surprising in production if a new route was added without noticing the collision.

Constraint precedence is how the framework breaks near-ties. A literal segment (`/products/featured`) scores higher than a parameterized one (`/products/{slug}`), which scores higher than the same parameter with a constraint (`{slug:alpha}`). Wait — a constrained parameter scores *higher* than an unconstrained one because the constraint narrows the candidate set and represents a more specific intent. When two routes are genuinely equal in specificity and both pass their constraints for the same request, the ambiguous-match exception fires regardless of registration order. The fix is to add a differentiating constraint — `{id:int}` versus `{slug:alpha:minlength(3)}` — or restructure the templates so one is a strict subset of the other.

---

## CQ8. How do Tag Helpers replace HTML Helpers, what does asp-for bind to, and how do you build a custom Tag Helper that emits IHtmlContent?

**Concepts**
- HTML Helpers are C# extension methods on `IHtmlHelper` returning `IHtmlContent`; the markup is hidden inside a method call
- Tag Helpers are C# classes targeting HTML elements by tag name or attribute — the source reads as HTML, not C#
- `asp-for="PropertyName"` binds to the view's `@model` expression and derives `name`, `id`, `type`, and `data-val-*` attributes from the property and its DataAnnotations
- Custom Tag Helpers extend `TagHelper`, decorate with `[HtmlTargetElement]`, and override `Process` / `ProcessAsync`
- `output.Content.SetHtmlContent(...)` or `AppendHtml(...)` writes the rendered output; register via `@addTagHelper *, YourAssembly` in `_ViewImports.cshtml`

**Answer**

HTML Helpers predate Tag Helpers and work as C# method calls embedded in Razor: `@Html.TextBoxFor(m => m.Email)`. They return `IHtmlContent` and produce the correct `<input>` element, but the markup is invisible inside the method call — front-end developers editing the view see C#, not HTML. Tag Helpers flip this: you write `<input asp-for="Email" />`, and ASP.NET Core's Razor compiler recognizes `asp-for` as a server-side instruction. The compiled output is the same `<input>` with correct `name`, `id`, `type`, and validation data attributes, but the source file reads as ordinary HTML with extra attributes.

The `asp-for` attribute accepts a property name rooted in the `@model` type. The framework reads the property's CLR type to infer `type="number"` or `type="text"`, reads DataAnnotations such as `[DataType(DataType.Password)]` to emit `type="password"`, and reads `[Required]`, `[Range]`, and `[StringLength]` to populate `data-val-required`, `data-val-range-min`, etc. This is why the `@model` directive is mandatory for `asp-for` to work — without a declared model type there is no expression context.

A custom Tag Helper extends `TagHelper`, is annotated with `[HtmlTargetElement("alert-box")]` (matching either a tag name or an attribute), and overrides `Process(TagHelperContext context, TagHelperOutput output)`. Inside, call `output.Content.SetHtmlContent("<div class=\"alert\">...</div>")` or build content via `output.Content.AppendHtml(...)`. For asynchronous operations, override `ProcessAsync` instead. Register the assembly in `_ViewImports.cshtml` with `@addTagHelper *, YourAssembly` so Razor can discover the class at compile time.

---

## CQ9. How does _ViewStart.cshtml resolve layout selection inside an Area, and what causes the "layout could not be located" gotcha?

**Concepts**
- Razor discovers `_ViewStart.cshtml` by walking up from the view's directory toward the root — the first file found wins
- An Area view at `Areas/Admin/Views/Dashboard/Index.cshtml` checks `Areas/Admin/Views/Dashboard/`, then `Areas/Admin/Views/`, then `Views/`, then root
- A `_ViewStart.cshtml` placed in `Areas/Admin/Views/` takes priority over the root `Views/_ViewStart.cshtml` for all Admin Area views
- When the Area's `_ViewStart.cshtml` sets `Layout = "_Layout"`, Razor searches for the file using the Area view location expanders — first in `Areas/Admin/Views/Shared/_Layout.cshtml`
- If that file is absent, Razor throws `InvalidOperationException: Layout '_Layout' could not be located` even though the identical file exists in `Views/Shared/`

**Answer**

Razor resolves `_ViewStart.cshtml` by ascending the directory tree from the view being rendered. For an Area view at `Areas/Admin/Views/Dashboard/Index.cshtml`, Razor checks the `Dashboard/` subfolder, then `Areas/Admin/Views/`, then the root `Views/` folder. The first `_ViewStart.cshtml` found applies its `Layout` setting and stops the upward search.

The gotcha arises when you create `Areas/Admin/Views/_ViewStart.cshtml` and set `Layout = "_Layout"`. Razor now resolves the layout name using the Area's registered view location expanders, which expand `"_Layout"` to `~/Areas/Admin/Views/Shared/_Layout.cshtml` before falling back to the root. If that Area-specific path does not exist, Razor throws even though `~/Views/Shared/_Layout.cshtml` is right there and would have been used had you never created the Area `_ViewStart.cshtml`.

There are two clean fixes. The first is to use an absolute virtual path in the Area's `_ViewStart.cshtml`: `Layout = "~/Views/Shared/_Layout.cshtml"`. Absolute paths bypass the relative expander chain entirely. The second is to place a `_Layout.cshtml` (or a thin wrapper that sets `Layout = "~/Views/Shared/_Layout.cshtml"`) in `Areas/Admin/Views/Shared/`. The absolute-path approach is preferred because it avoids file duplication and makes the layout dependency explicit. If you want the Area to use a genuinely different layout, place the full layout file in the Area's `Shared/` folder and reference it with a relative name.

---

## CQ10. Why is the @model directive required for strongly-typed views, and how do DisplayFor and EditorFor differ in their relationship to DataAnnotations?

**Concepts**
- Without `@model`, `Model` is typed as `dynamic` — property typos compile silently and fail at runtime
- `@model MyApp.ViewModels.ProductViewModel` generates a strongly-typed `Model` property, enabling lambda expressions and compile-time view checking
- `@Html.DisplayFor(m => m.BirthDate)` renders read-only output via display templates; respects `[DisplayFormat]` and `[DataType]` for presentation
- `@Html.EditorFor(m => m.BirthDate)` renders an editable `<input>` via editor templates; respects `[DataType]` and `[UIHint]` to choose the control type
- Tag Helper `<input asp-for="BirthDate" />` is the preferred equivalent for `EditorFor` in .NET 10 — it also populates `data-val-*` attributes in one declaration

**Answer**

The `@model` directive at the top of a Razor file instructs the Razor compiler to generate a `Model` property of the specified type. Without it, `Model` is `dynamic`, which means the compiler accepts any property access — `Model.Nme` compiles fine and fails with a `RuntimeBinderException` at render time. With `@model`, the compiler validates property names, enables IntelliSense in IDEs, and (with Razor compilation on, the .NET 10 default) surfaces typos as build errors before the application ships.

`@Html.DisplayFor(m => m.BirthDate)` uses display templates to render a read-only representation. It consults `[DisplayFormat(DataFormatString = "{0:d}")]` to apply a date format string, and `[DataType(DataType.EmailAddress)]` to render an `<a href="mailto:...">` hyperlink. It never emits an editable control.

`@Html.EditorFor(m => m.BirthDate)` uses editor templates to render an input control. It reads `[DataType(DataType.Password)]` to emit `<input type="password" />`, `[DataType(DataType.MultilineText)]` to emit `<textarea>`, and `[UIHint("DatePicker")]` to select a custom template from `Views/Shared/EditorTemplates/DatePicker.cshtml`. The Tag Helper equivalent `<input asp-for="BirthDate" />` covers the same cases while also writing `data-val-*` attributes for client-side validation in a single element declaration, making it the cleaner and now-preferred form-field syntax. `DisplayFor` and `EditorFor` remain useful when you need template-driven rendering or custom editor templates that Tag Helpers cannot replicate inline.

---

## CQ11. How does jQuery Validate unobtrusive read data-val-* attributes from DataAnnotations, and why do custom validators need IClientModelValidator?

**Concepts**
- Tag Helpers and HTML Helpers emit `data-val="true"` plus rule-specific attributes (`data-val-required`, `data-val-range-min`, `data-val-length-max`) derived from DataAnnotation attributes on the ViewModel property
- `jquery.validate.unobtrusive.js` scans the DOM on page load, reads those attributes, and registers jQuery Validate rules — no server round-trip required for inline errors
- Built-in annotations (`[Required]`, `[Range]`, `[StringLength]`, `[EmailAddress]`, `[RegularExpression]`) each have a built-in model validator that also implements `IClientModelValidator`
- A custom `ValidationAttribute` only runs server-side unless it also implements `IClientModelValidator`
- `IClientModelValidator.AddValidation(ClientModelValidationContext)` adds entries to `context.Attributes` — these become the `data-val-*` attributes; a matching JavaScript adapter must also be registered

**Answer**

ASP.NET Core's Tag Helpers emit `data-val="true"` on any field whose ViewModel property carries a DataAnnotation. Additional attributes specify the rule: `data-val-required="The Name field is required."`, `data-val-range-min="1"`, `data-val-range-max="100"`, and so on. When `jquery.validate.unobtrusive.js` loads, it reads these attributes, maps them to jQuery Validate rule names via registered adapters, and attaches the rules to the form. Field errors appear inline without a server round-trip — the standard user experience for MVC forms.

This automatic pipeline works for built-in annotations because each one's internal model validator implements `IClientModelValidator`. When you create a custom attribute — say, `[FutureDateOnly]` inheriting `ValidationAttribute` — only the `IsValid` server-side override exists. The client sees no `data-val-futuredate` attribute, jQuery Validate has no rule, the form submits successfully, and only then does the server reject the value with a `ModelState` error. This two-step failure is jarring and erodes trust in the form.

To close the gap, implement `IClientModelValidator` on the custom attribute and override `AddValidation`:

```csharp
public void AddValidation(ClientModelValidationContext context)
{
    context.Attributes["data-val"] = "true";
    context.Attributes["data-val-futuredate"] = "Date must be in the future.";
}
```

Then register the JavaScript side using `$.validator.unobtrusive.adapters.addBool("futuredate")` paired with `$.validator.addMethod("futuredate", function (value) { ... })`. Both halves are required — the server side emits the attribute, the client side interprets it. In .NET 10, this contract is unchanged; the only improvement is that `ProblemDetails` responses from `BadRequest(ModelState)` make it easier to surface these server-side rejections in SPA clients that bypass unobtrusive validation entirely.

---

## CQ12. How does IExceptionFilter differ from global exception-handling middleware, and what gap does IAlwaysRunResultFilter fill that regular result filters leave?

**Concepts**
- `IExceptionFilter.OnException` runs within the MVC filter pipeline — after action execution, before the response is written; setting `context.ExceptionHandled = true` suppresses further propagation
- `UseExceptionHandler` middleware sits outside the MVC pipeline and catches anything the filter did not handle — the last-resort safety net
- Execution order for unhandled exceptions: controller-scoped `IExceptionFilter` → global MVC `IExceptionFilter` → exception-handling middleware
- A regular `IResultFilter` is not called when an authorization or resource filter short-circuits execution by assigning `context.Result`
- `IAlwaysRunResultFilter` fires for every result — including short-circuit results from authorization and resource filters — making it the correct hook for unconditional response transformations

**Answer**

`IExceptionFilter.OnException` runs inside the MVC pipeline, giving it access to the `ActionContext`, route data, and the current controller. A controller-scoped exception filter (applied via `[TypeFilter(typeof(MyExceptionFilter))]`) runs first; a globally registered one runs second. If any filter sets `context.ExceptionHandled = true` and assigns `context.Result`, MVC writes that result as a normal response and the exception never reaches the middleware pipeline. This makes `IExceptionFilter` well-suited for business exceptions — `EntityNotFoundException` mapping to a 404 view, or `UnauthorizedOperationException` mapping to a custom 403 page — because the filter has full view-rendering context.

`UseExceptionHandler` middleware runs outside MVC entirely. It catches any exception that the filter pipeline did not handle and is the correct place for infrastructure-level error handling — logging, `ProblemDetails` formatting, generic error pages. It does not have access to MVC route data or controller context.

The `IAlwaysRunResultFilter` gap: when an authorization filter short-circuits by assigning `context.Result = new UnauthorizedResult()`, the MVC pipeline skips action and standard result filters — a regular `IResultFilter` is never called for that result. `IAlwaysRunResultFilter` is invoked for every result, including short-circuit responses. This is the right extension point for unconditional cross-cutting output work — adding security response headers, enforcing a consistent JSON envelope, or appending audit log entries — that must apply to 401 and 403 responses just as much as to successful action results. Register it globally via `builder.Services.AddControllersWithViews(o => o.Filters.Add<ResponseEnvelopeFilter>())`.

---

## CQ13. When should an AJAX action return PartialViewResult vs JsonResult, how is AJAX detected reliably, and why does AJAX break the PRG pattern?

**Concepts**
- `PartialViewResult` returns a rendered HTML fragment — suitable when the client wants markup ready to inject into the DOM
- `JsonResult` returns serialized data — suitable when the client will format the data itself or when the response carries instructions (redirect URL, success flag)
- jQuery sets `X-Requested-With: XMLHttpRequest` automatically; the Fetch API does not — detection must not rely on this header alone
- The more reliable modern signal: send `Accept: application/json` in all AJAX requests and branch on `Request.Headers.Accept`
- Post-Redirect-Get prevents duplicate submissions by redirecting after POST; AJAX follows `302` transparently, so the browser address bar never changes — simulate PRG by returning a redirect URL in JSON and navigating with `window.location.href`

**Answer**

`PartialViewResult` is the right return type when the AJAX call's sole purpose is to refresh a section of the page with rendered markup — a filtered product list, a comment thread, a summary widget. The client inserts the HTML directly: `document.getElementById("results").innerHTML = response`. `JsonResult` is correct when the client needs structured data to format itself, or when the response is an instruction rather than content — `{ "success": true, "redirectUrl": "/orders/42" }` tells the client what to do next without prescribing markup.

Post-Redirect-Get prevents the browser's "resubmit form?" dialog on refresh: after a successful POST, the server issues a `302` redirect to a GET endpoint, and the browser stores the GET URL in history. AJAX breaks this because `XMLHttpRequest` and `fetch` follow `302` redirects automatically and transparently — the AJAX caller receives the body of the redirected GET response without ever changing the browser's address bar. Refreshing the page still shows the POST URL. To replicate PRG semantics over AJAX, return `Json(new { success = true, redirectUrl = Url.Action("Index", "Orders") })` for successful submissions, and have the client execute `window.location.href = data.redirectUrl`. The browser then navigates, updating history correctly.

For AJAX detection, jQuery adds `X-Requested-With: XMLHttpRequest` but `fetch` does not. The most robust approach in .NET 10 is a two-layer check: first, look for an explicit custom header your JavaScript always sets; second, check `Request.Headers.Accept` for `"application/json"` using `Request.Headers.Accept.Any(h => h.MediaType == "application/json")`. This works regardless of the JavaScript library used and degrades gracefully when the same action serves full-page requests.

---

## CQ14. How does Area route registration order interact with conventional routes, and what does the [Area] attribute actually do for controller discovery?

**Concepts**
- Area routes must be registered *before* the default conventional route — if the default route is registered first, it can match Area URLs before the Area route gets evaluated
- `app.MapAreaControllerRoute("admin_default", "Admin", "Admin/{controller=Dashboard}/{action=Index}/{id?}")` uses a literal `Admin` segment to prevent shadowing
- The `[Area("Admin")]` attribute on a controller marks it as belonging to that area in the route table — physical folder location alone is not sufficient
- Without `[Area]`, a controller in `Areas/Admin/Controllers/` is unreachable via the area route even though it is in the expected folder
- The shadow risk: a root `HomeController` and an Area `HomeController` without a literal area segment in the template causes ambiguous or wrong routing

**Answer**

In .NET 10, area routes are registered in `Program.cs` before the fallback conventional route:

```csharp
app.MapAreaControllerRoute("admin", "Admin", "Admin/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

Registration order is significant for conventional routes: the middleware tests templates top-down and stops at the first match. If the default route is registered first, a URL like `/Admin/Products/List` matches `{controller=Admin}` / `{action=Products}` / `{id=List}` — routing to a non-existent `AdminController.Products` action — before the area route is ever evaluated. The literal `Admin/` segment in the area template prevents this by requiring a more specific match that the default template cannot supply.

The `[Area("Admin")]` attribute is not optional convention — it is the mechanism by which a controller is registered in MVC's endpoint table as belonging to an area. A controller class physically located in `Areas/Admin/Controllers/DashboardController.cs` is not automatically associated with the "Admin" area. Without `[Area("Admin")]` on the class, the area route template `Admin/{controller}/{action}` has no endpoint that satisfies both the area constraint and the controller name "Dashboard", and the URL returns 404. Adding the attribute tells the route table that this controller handles the "Admin" area token.

The controller-name-collision shadow risk occurs when both a root `HomeController` and an `Areas/Admin/HomeController` exist. An overly broad area route template — one using `{area}` as a parameter rather than a literal segment — can match root URLs when `area` defaults to empty string or null, routing requests to the Area controller unexpectedly. Always use a literal area segment or an `{area:regex(Admin)}` constraint.

---

## CQ15. How does the model binder handle nested ViewModels and collection binding, and what is the missing-hidden-input data-corruption gotcha?

**Concepts**
- Nested ViewModel binding uses dot-notation prefixes: a field named `Address.Street` binds to `ViewModel.Address.Street`
- Collection binding uses indexed names: `Items[0].Quantity`, `Items[1].ProductId`; the index sequence must be contiguous starting at 0 — a gap stops binding at the first missing index
- Tag Helpers with `asp-for="Items[i].Property"` inside a `for` loop generate the correct indexed names automatically
- The missing-hidden-input gotcha: properties absent from the POST body revert to their CLR default (null, 0, false) — not their pre-edit database values
- Fix: include `<input type="hidden" asp-for="Id" />` for every property the form doesn't edit but the controller needs, or reload from the database and apply only posted fields

**Answer**

ASP.NET Core's default model binder maps form field names to action parameter properties by name matching with prefix navigation. For a `ProductViewModel` with an `Address` property of type `AddressViewModel`, the posted form must contain fields named `Address.Street`, `Address.City`, and `Address.PostalCode`. The binder splits the name on the dot, navigates the object graph, and assigns each leaf value. If the form uses flat names without the prefix — `Street`, `City` — the `Address` property is created with null leaves.

Collection binding for `List<OrderLineViewModel> Items` uses indexed names: `Items[0].Quantity`, `Items[0].ProductId`, `Items[1].Quantity`. The index sequence must start at 0 and be contiguous. If `Items[0]` is missing or there is a gap between indices 0 and 2, the binder processes entries only up to the gap. Using a `for` loop in Razor with `asp-for="Items[@i].Quantity"` generates the correct names automatically; a `foreach` loop does not produce indexed names and prevents collection binding.

The missing-input data-corruption bug is the most common production mistake with this binding model. A POST form that renders only editable fields — omitting `<input type="hidden" asp-for="Id" />` for the record key, or `asp-for="CreatedDate"` — causes those properties to bind as `0`, `null`, or `false`. If the controller passes this partially populated ViewModel to an ORM's `Update` method, the database record is silently overwritten with those defaults. The two reliable fixes are: include a hidden input for every property the form needs but does not edit; or, for sensitive properties, reload the entity from the database inside the action and apply only the whitelisted posted fields using `TryUpdateModelAsync<TEntity>(entity, "", m => m.AllowedProp1, m => m.AllowedProp2)`.

---

## CQ16. How do required vs optional @RenderSection, nested layouts, and the PartialAsync-vs-Component.InvokeAsync choice interact?

**Concepts**
- `@RenderSection("Scripts")` defaults to `required: true` — every view using this layout must define `@section Scripts { }` or Razor throws at render time
- `@RenderSection("Scripts", required: false)` makes the section optional; `@IsSectionDefined("Scripts")` tests for it before rendering a fallback
- In a nested layout (`_AdminLayout.cshtml` → `_Layout.cshtml`), a `@section` defined in the child view is available only to its immediate layout — it does not propagate automatically to the root layout
- `@await Html.PartialAsync("_Widget")` renders a `.cshtml` partial file, sharing the current view's `ViewData` and receiving an optional model argument; it has no independent business logic
- `@await Component.InvokeAsync("Widget")` runs a `ViewComponent` C# class with constructor-injected dependencies, its own `InvokeAsync` method, and isolated `ViewData`

**Answer**

`@RenderSection` in a layout file declares a named placeholder that child views can fill. The `required` parameter (default `true`) controls whether every view using the layout must define that section — if a view omits a required section, Razor throws an `InvalidOperationException` at render time. Setting `required: false` makes the section optional, and `@IsSectionDefined("Scripts")` lets the layout conditionally render fallback content or nothing. The standard pattern is `@RenderSection("Scripts", required: false)` at the bottom of `_Layout.cshtml` so individual views can inject page-specific scripts.

Nested layouts surface a common misunderstanding: `_AdminLayout.cshtml` may itself set `Layout = "_Layout.cshtml"`, rendering inside the root layout. But a `@section Scripts { }` block defined in a child view is available only to `_AdminLayout.cshtml` — Razor does not propagate sections upward automatically. To pass a section through, `_AdminLayout.cshtml` must declare its own `@section Scripts { @RenderSection("Scripts", required: false) }`, explicitly forwarding the child's section to the root layout's placeholder.

`Html.PartialAsync` renders a Razor `.cshtml` file, sharing the calling view's `ViewData` context. It is lightweight and appropriate for pure presentation reuse — a reusable card layout, a table header — where no server-side logic or dependency injection is needed. `Component.InvokeAsync` runs a `ViewComponent` class, which has its own `InvokeAsync(...)` method, receives constructor-injected services, and maintains isolated `ViewData`. Choose a ViewComponent when the widget must query a service, apply business logic, or manage its own data — not just format data the parent view already has. In .NET 10, ViewComponents also support the Tag Helper syntax `<vc:widget items="Model.Items" />`, making them composable alongside native HTML in Razor files.

---

## CQ17. How does TempData's backing store (session vs cookie) affect its availability, and why must flash messages call Keep() in multi-step flows?

**Concepts**
- `CookieTempDataProvider` (the .NET 6+ default) serializes TempData to a signed, Base64-encoded cookie — client-side storage, 4 KB limit applies after encoding
- `SessionStateTempDataProvider` stores data server-side (in-memory, Redis, or SQL Server); requires `services.AddSession()` and scales to large payloads
- TempData is loaded at request start; entries written by the current action are saved at response end and available to the *next* request
- Writing to TempData and returning a `ViewResult` (not a redirect) in the same request does not make the new entries visible in that view — they are for the next request
- In a multi-step wizard, reading TempData at each intermediate step marks entries consumed; `Keep("key")` after each read re-queues the entry for one more request

**Answer**

`CookieTempDataProvider`, the default since .NET 6, serializes the entire TempData dictionary to a signed cookie using `IDataProtector`. The data lives in the browser — no server session store is required — but the 4 KB cookie size limit constrains the payload. Large objects such as complex ViewModels or lists must either be trimmed, stored in a server-side session, or kept as IDs with a database lookup. Switch to `SessionStateTempDataProvider` by calling `services.AddSession()` and `services.AddMvc().AddSessionStateTempDataProvider()`, then choosing a distributed cache (Redis, SQL Server) for multi-server deployments.

The load-then-save cycle is important: TempData is loaded from the provider at the very start of a request. Entries a *prior* request wrote are now available. Entries written by the *current* action are saved at the end of the current request and become available to the next one. This means writing `TempData["Flash"] = "Saved"` and returning a `ViewResult` in the same action does not make `"Flash"` visible to that view — it is for the next request. Use `ViewBag` or the ViewModel for same-request display; use TempData only across a redirect.

Multi-step wizard flows extend this further. If step 2 reads `TempData["WizardData"]` to display a summary, the read marks the entry consumed. When step 3 loads, the entry is gone. Calling `TempData.Keep("WizardData")` immediately after reading re-queues the entry for one additional request, carrying it to step 3. Each intermediate step that reads and needs to forward the data must call `Keep`. For many-step flows, consider a dedicated session key or a database-backed draft record instead of chaining `Keep` calls.

---

## CQ18. When should you use SignalR vs AJAX polling for live UI updates, and how does the hybrid notify-then-fetch pattern combine both?

**Concepts**
- AJAX polling issues periodic `fetch` requests at a fixed interval — simple, works everywhere HTTP works, but generates O(clients × frequency) server requests regardless of whether data changed
- SignalR maintains a persistent connection per client (WebSocket preferred; SSE and long-polling as fallbacks) — the server pushes only when data changes, eliminating idle overhead
- WebSocket overhead at scale: Kestrel in .NET 10 handles tens of thousands of concurrent async connections, but a backplane (Redis, Azure SignalR Service) is required for multi-instance deployments
- Hybrid pattern: SignalR sends a lightweight `"orderUpdated"` event containing only the entity ID; the client fires `fetch("/api/orders/{id}")` to retrieve the full payload — decouples transport from data shape
- Choose polling when: updates are infrequent, WebSockets are blocked by a proxy, or simplicity outweighs real-time fidelity

**Answer**

AJAX polling — a `setInterval` firing a `fetch` every few seconds — is the path of least resistance: it works everywhere HTTP works, requires no persistent connection infrastructure, and is trivial to debug. Its cost scales linearly with clients and frequency. At 2,000 clients polling every 5 seconds that is 400 requests per second of overhead, most returning "nothing changed." This is acceptable for dashboards that refresh every minute and tolerate a few seconds of latency.

SignalR inverts the cost model. The server pushes a message only when data changes — zero traffic between events. A Kestrel instance in .NET 10 handles tens of thousands of WebSocket connections via async I/O without blocking threads, but the infrastructure cost rises: hubs must be registered, CORS must permit credentials, and any multi-instance deployment requires a backplane (Redis pub/sub or Azure SignalR Service) so that a message produced by instance A reaches clients connected to instance B.

The hybrid notify-then-fetch pattern captures the best of both. SignalR sends a minimal event — `Clients.Group(orderId.ToString()).SendAsync("orderUpdated", orderId)` — containing only the entity ID. The JavaScript client handles the event and fires `fetch(`/api/orders/${orderId}`)` to retrieve the full payload. This keeps hub messages tiny, leaves the REST endpoint independently testable and cacheable, and degrades gracefully: if the SignalR connection drops before reconnection, the client can temporarily fall back to polling the REST endpoint. The pattern also avoids the serialization mismatch risk of pushing large objects through the hub whose shape may diverge from what the REST endpoint returns. In .NET 10, the `HubConnectionBuilder` reconnect policy makes this hybrid resilient — automatic reconnect re-establishes the push channel without losing any AJAX-fetched data already on the page.

---

# 05. ASP.NET Core MVC — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [Introduction to MVC Pattern](01.%20Introduction%20to%20MVC%20Pattern/INTERVIEW_QA.md) | MVC separation of concerns, request lifecycle, and how model, view, and controller collaborate in ASP.NET Core |
| 02 | [Controllers & Actions](02.%20Controllers%20%26%20Actions/INTERVIEW_QA.md) | Routing requests to action methods, return types (`IActionResult`, `ActionResult<T>`), and the action execution context |
| 03 | [Views & Razor Syntax](03.%20Views%20%26%20Razor%20Syntax/INTERVIEW_QA.md) | Razor template engine, C# expressions in views, HTML encoding, and view discovery |
| 04 | [Layouts, Sections & Partial Views](04.%20Layouts%2C%20Sections%20%26%20Partial%20Views/INTERVIEW_QA.md) | Shared layout pages, `@RenderBody`/`@RenderSection`, and reusable partial view components |
| 05 | [ViewModels & Strongly Typed Views](05.%20ViewModels%20%26%20Strongly%20Typed%20Views/INTERVIEW_QA.md) | Purpose-built view models, the `@model` directive, and type-safe data passing from controller to view |
| 06 | [Model Binding in MVC](06.%20Model%20Binding%20in%20MVC/INTERVIEW_QA.md) | How HTTP request data maps to action parameters, binding sources, and over-posting prevention |
| 07 | [Data Annotations & Validation](07.%20Data%20Annotations%20%26%20Validation/INTERVIEW_QA.md) | Declarative validation attributes, `ModelState`, `IValidatableObject`, and custom validators |
| 08 | [Tag Helpers](08.%20Tag%20Helpers/INTERVIEW_QA.md) | Server-side helpers that generate HTML — `asp-for`, `asp-action`, form tag helpers, and custom tag helpers |
| 09 | [Routing & Attribute Routing](09.%20Routing%20%26%20Attribute%20Routing/INTERVIEW_QA.md) | Conventional and attribute-based routes, route constraints, route order, and `IRouteConstraint` |
| 10 | [Areas](10.%20Areas/INTERVIEW_QA.md) | Organizing large applications into feature areas with separate controllers, views, and routes |
| 11 | [Action Filters in MVC](11.%20Action%20Filters%20in%20MVC/INTERVIEW_QA.md) | Filter pipeline (authorization → resource → action → exception → result), filter scope, and short-circuiting |
| 12 | [TempData, ViewData & ViewBag](12.%20TempData%2C%20ViewData%20%26%20ViewBag/INTERVIEW_QA.md) | Lifetime and scope differences; cross-redirect data survival with `TempData` |
| 13 | [AJAX & Partial Page Updates](13.%20AJAX%20%26%20Partial%20Page%20Updates/INTERVIEW_QA.md) | Returning partial views via AJAX, JSON responses, and updating page fragments without full reloads |
| 14 | [Client-Side Validation](14.%20Client-Side%20Validation/INTERVIEW_QA.md) | Unobtrusive validation, jQuery Validate integration, and custom client-side validators |
| 15 | [Real-Time UI with SignalR](15.%20Real-Time%20UI%20with%20SignalR/INTERVIEW_QA.md) | Hub configuration, connection management, broadcasting, and backplane for scale-out |

---

## Table of Contents
- [CQ1. How does the model binding → data annotations → ViewModel round-trip work, and why does it prevent over-posting?](#cq1-how-does-the-model-binding--data-annotations--viewmodel-round-trip-work-and-why-does-it-prevent-over-posting)
- [CQ2. When should you use ViewBag, ViewData, TempData, or a ViewModel — and what breaks across redirects?](#cq2-when-should-you-use-viewbag-viewdata-tempdata-or-a-viewmodel--and-what-breaks-across-redirects)
- [CQ3. What is the MVC filter pipeline order, how do action and result filters differ, and what is the post-validation mutation trap?](#cq3-what-is-the-mvc-filter-pipeline-order-how-do-action-and-result-filters-differ-and-what-is-the-post-validation-mutation-trap)
- [CQ4. How can AJAX partial-view updates silently bypass both server-side ModelState and client-side validation?](#cq4-how-can-ajax-partial-view-updates-silently-bypass-both-server-side-modelstate-and-client-side-validation)
- [CQ5. How do SignalR hub routes interact with Area routing, and what CORS gotcha blocks only SignalR in production?](#cq5-how-do-signalr-hub-routes-interact-with-area-routing-and-what-cors-gotcha-blocks-only-signalr-in-production)
- [CQ6. How does TempData's one-request lifetime interact with redirects triggered by action filters, and when is Peek/Keep needed?](#cq6-how-does-tempdatas-one-request-lifetime-interact-with-redirects-triggered-by-action-filters-and-when-is-peekkeep-needed)
- [CQ7. What is the ambiguous-match gotcha when attribute routing and conventional routing coexist, and how does constraint precedence resolve ties?](#cq7-what-is-the-ambiguous-match-gotcha-when-attribute-routing-and-conventional-routing-coexist-and-how-does-constraint-precedence-resolve-ties)
- [CQ8. How do Tag Helpers replace HTML Helpers, what does asp-for bind to, and how do you build a custom Tag Helper that emits IHtmlContent?](#cq8-how-do-tag-helpers-replace-html-helpers-what-does-asp-for-bind-to-and-how-do-you-build-a-custom-tag-helper-that-emits-ihtmlcontent)
- [CQ9. How does _ViewStart.cshtml resolve layout selection inside an Area, and what causes the "layout could not be located" gotcha?](#cq9-how-does-_viewstartcshtml-resolve-layout-selection-inside-an-area-and-what-causes-the-layout-could-not-be-located-gotcha)
- [CQ10. Why is the @model directive required for strongly-typed views, and how do DisplayFor and EditorFor differ in their relationship to DataAnnotations?](#cq10-why-is-the-model-directive-required-for-strongly-typed-views-and-how-do-displayfor-and-editorfor-differ-in-their-relationship-to-dataannotations)
- [CQ11. How does jQuery Validate unobtrusive read data-val-* attributes from DataAnnotations, and why do custom validators need IClientModelValidator?](#cq11-how-does-jquery-validate-unobtrusive-read-data-val--attributes-from-dataannotations-and-why-do-custom-validators-need-iclientmodelvalidator)
- [CQ12. How does IExceptionFilter differ from global exception-handling middleware, and what gap does IAlwaysRunResultFilter fill that regular result filters leave?](#cq12-how-does-iexceptionfilter-differ-from-global-exception-handling-middleware-and-what-gap-does-ialwaysrunresultfilter-fill-that-regular-result-filters-leave)
- [CQ13. When should an AJAX action return PartialViewResult vs JsonResult, how is AJAX detected reliably, and why does AJAX break the PRG pattern?](#cq13-when-should-an-ajax-action-return-partialviewresult-vs-jsonresult-how-is-ajax-detected-reliably-and-why-does-ajax-break-the-prg-pattern)
- [CQ14. How does Area route registration order interact with conventional routes, and what does the [Area] attribute actually do for controller discovery?](#cq14-how-does-area-route-registration-order-interact-with-conventional-routes-and-what-does-the-area-attribute-actually-do-for-controller-discovery)
- [CQ15. How does the model binder handle nested ViewModels and collection binding, and what is the missing-hidden-input data-corruption gotcha?](#cq15-how-does-the-model-binder-handle-nested-viewmodels-and-collection-binding-and-what-is-the-missing-hidden-input-data-corruption-gotcha)
- [CQ16. How do required vs optional @RenderSection, nested layouts, and the PartialAsync-vs-Component.InvokeAsync choice interact?](#cq16-how-do-required-vs-optional-rendersection-nested-layouts-and-the-partialasync-vs-componentinvokeasync-choice-interact)
- [CQ17. How does TempData's backing store (session vs cookie) affect its availability, and why must flash messages call Keep() in multi-step flows?](#cq17-how-does-tempdatas-backing-store-session-vs-cookie-affect-its-availability-and-why-must-flash-messages-call-keep-in-multi-step-flows)
- [CQ18. When should you use SignalR vs AJAX polling for live UI updates, and how does the hybrid notify-then-fetch pattern combine both?](#cq18-when-should-you-use-signalr-vs-ajax-polling-for-live-ui-updates-and-how-does-the-hybrid-notify-then-fetch-pattern-combine-both)

---

## CQ1. How does the model binding → data annotations → ViewModel round-trip work, and why does it prevent over-posting?

**Concepts**
- Model binding maps form fields to action parameter properties before the action executes
- Data annotations on the ViewModel run as part of binding, accumulating errors in `ModelState`
- `ModelState.IsValid` is the controller's gate before any persistence
- Over-posting: a malicious client can post fields not rendered in the form — e.g., `IsAdmin=true`
- ViewModels expose only the properties the form legitimately sets; domain entities expose everything

**Answer**

When an HTTP POST arrives, ASP.NET Core's model binder reads form fields, route values, and query strings and assigns them to matching properties on the action's parameter — which should be a ViewModel, not a domain entity. As each property is set, any data annotation attributes (`[Required]`, `[Range]`, `[StringLength]`) run and record failures in `ModelState`. The action then checks `ModelState.IsValid`; if false, it returns the view with the same ViewModel so Razor and client-side validation can display errors beside the appropriate fields.

The over-posting risk arises if a domain entity is used directly as the parameter. The binder blindly applies any field name it finds in the request — including `IsAdmin`, `AccountBalance`, or `CreatedAt` — even if those inputs were never rendered in the form. A ViewModel is a purpose-built class that exposes only the properties the form legitimately collects, so the binder has nothing to target for those sensitive fields. In .NET 10, the recommended pattern is a record-based ViewModel with `[BindNever]` on fields that the server always computes, providing a belt-and-suspenders defence alongside ViewModel scoping.

---

## CQ2. When should you use ViewBag, ViewData, TempData, or a ViewModel — and what breaks across redirects?

**Concepts**
- `ViewBag` / `ViewData`: scoped to the current request and current view; not available after a redirect
- `TempData`: stored in the session (or cookie provider) and survives exactly one subsequent request
- ViewModel: strongly typed, compile-time checked, always the preferred mechanism for view data
- `ViewBag` is `dynamic` — no IntelliSense, no compile error on a typo, silently null at runtime
- Passing complex objects via `ViewBag` across a redirect yields null — only `TempData` survives

**Answer**

`ViewBag` and `ViewData` are two surfaces over the same `ViewDataDictionary` and share the same lifetime: they exist for the current request and are available to the current view and its partials. The moment you issue a `RedirectToAction`, the entire request ends and a new one begins — `ViewBag` and `ViewData` are gone. `TempData` is different: it is serialized to the session or a signed cookie and loaded at the start of the next request, giving it exactly one cross-redirect lifetime. It is consumed (cleared) on read, which is why `Peek` lets you read without consuming and `Keep` re-marks an already-read entry for retention.

In practice, ViewModels beat all three for view data. A ViewModel is a concrete class, so the compiler catches property name typos, Razor can infer field metadata for tag helpers, and the data contract between controller and view is explicit. `ViewBag` is appropriate for one-off supplementary data — a dropdown list, a page title — where creating a ViewModel property feels disproportionate. `TempData` is the right tool for post-redirect-get confirmation messages (`"Record saved"`) because it survives the redirect that PRG requires, whereas a ViewModel populated before the redirect would be lost.

---

## CQ3. What is the MVC filter pipeline order, how do action and result filters differ, and what is the post-validation mutation trap?

**Concepts**
- Pipeline order: Authorization → Resource → (model binding) → Action → (action executes) → Result → (response written) → Exception (on error)
- Action filters wrap the action method execution; result filters wrap the `IActionResult` execution
- Action filters receive `ActionExecutingContext.ActionArguments` — the already-bound, already-validated model
- Mutating model properties in `OnActionExecuting` after `ModelState` has been populated does not re-run validation
- Result filters can inspect or replace the `IActionResult` but cannot recover from action-phase short-circuits

**Answer**

ASP.NET Core's MVC filter pipeline runs in a fixed order: authorization filters first (short-circuit to 401/403 if denied), then resource filters (can short-circuit before model binding, useful for caching), then model binding and `ModelState` population, then action filters' `OnActionExecuting`, then the action method itself, then action filters' `OnActionExecuted`, then result filters, and finally the `IActionResult` is executed (view rendered or JSON written). Exception filters fire on any unhandled exception in action or result phases.

The distinction between action and result filters is timing relative to the `IActionResult`: action filters run before and after the action method decides *what* result to return; result filters run before and after that result is *written to the response*. A result filter can swap a `ViewResult` for a `JsonResult` based on `Accept` headers, but it cannot change the model that was already passed to the view.

The post-validation trap: action filters receive `context.ActionArguments`, which contains the already-bound, already-validated model. If an `OnActionExecuting` filter modifies a property on that model — say, forcing `order.Status = "Pending"` — `ModelState` is not re-evaluated. The controller sees `ModelState.IsValid == true` even if the mutated value would have failed `[Required]` or `[Range]`. The fix is to either perform mutations in the action method after the validity check, or call `ModelState.SetModelValue` / `ModelState.MarkFieldValid` explicitly when the filter legitimately changes input.

---

## CQ4. How can AJAX partial-view updates silently bypass both server-side ModelState and client-side validation?

**Concepts**
- Partial views rendered via AJAX do not carry the parent page's jQuery Validate form context
- `$.ajax` / `fetch` posts bypass `asp-antiforgery` tag helper wiring unless added explicitly
- The server must always check `ModelState.IsValid` regardless of whether the caller is AJAX or full-page
- AJAX callers cannot display `asp-validation-summary`; errors must be returned as JSON and injected into the DOM
- The pattern: return `BadRequest(ModelState)` for AJAX callers, full view for non-AJAX callers

**Answer**

When a standard form is submitted, jQuery Validate inspects the page's validator instance and blocks the POST if any annotated field fails. But when JavaScript code issues an `$.ajax` or `fetch` POST directly — or when a partial view is injected into an existing page without being wired into that page's jQuery Validate form — client-side validation never runs. The browser simply fires the request. On the server, the model binder still populates `ModelState`, but if the controller does not explicitly check `ModelState.IsValid` and return an error response, it proceeds to save invalid data.

The correct pattern is to always check `ModelState.IsValid` in the action, then branch on the caller type. Detect AJAX with `Request.Headers["X-Requested-With"] == "XMLHttpRequest"` or a custom header. For valid data, return `Json(new { success = true, redirectUrl = ... })`. For invalid data, return `BadRequest(ModelState)` — or serialize `ModelState` errors to a JSON structure the client can iterate to highlight the offending fields. Do not attempt to return a partial view with validation summaries for AJAX callers, because the partial's `asp-validation-summary` tag helper renders server-side error spans that the client JavaScript cannot automatically activate. Antiforgery tokens must also be manually included in AJAX headers — `X-CSRF-TOKEN` — or `[ValidateAntiForgeryToken]` will reject the request with a 400 before validation even begins.

---

## CQ5. How do SignalR hub routes interact with Area routing, and what CORS gotcha blocks only SignalR in production?

**Concepts**
- SignalR hubs are mapped via `app.MapHub<T>("/path")` in `Program.cs`, independent of MVC route tables
- Area routes use the `[Area]` attribute and a conventional `{area}/{controller}/{action}` template — hubs do not participate
- The SignalR WebSocket upgrade uses the same origin policy — CORS must explicitly allow the hub URL *and* permit credentials
- `AllowAnyOrigin()` is incompatible with `AllowCredentials()` — CORS rejects the combination at startup
- The WebSocket upgrade is a separate HTTP request; CORS policy must list the hub path explicitly or via a wildcard pattern

**Answer**

SignalR hub routes are registered imperatively via `app.MapHub<ChatHub>("/chat")` in the middleware pipeline, completely separate from the MVC convention-based or attribute-based route table. Areas add a route prefix (`admin/products/index`) and a controller discovery scope, but because hubs are not controllers, an Area prefix has no automatic effect on hub paths. If you want a hub to appear under an Area's URL prefix, you must embed the prefix manually in `MapHub<T>("/admin/chat")`. There is no `[Area]` attribute for hubs and no link generator entry, so `Url.Action` and `IUrlHelper` cannot resolve hub paths.

The CORS gotcha surfaces in production when the front-end is served from a different origin than the API. Developers commonly configure `AllowAnyOrigin()` during development, which works for `fetch` calls but fails for SignalR. SignalR transports — long-polling, Server-Sent Events, and WebSocket — all send credentials (cookies or the `Authorization` header) as part of the connection negotiation. CORS requires `AllowCredentials()` when credentials are present, but `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined — ASP.NET Core throws at startup or returns a CORS rejection at runtime. The fix is to replace `AllowAnyOrigin()` with `WithOrigins("https://your-client.com")` and add `AllowCredentials()`. MVC routes continue working through this because typical API calls from JavaScript do not send cookies in cross-origin requests unless `credentials: "include"` is explicitly set; SignalR's client library always sets it.

---

## CQ6. How does TempData's one-request lifetime interact with redirects triggered by action filters, and when is Peek/Keep needed?

**Concepts**
- `TempData` is written in one request and consumed (deleted) on the first read in the next request
- Action filters can short-circuit execution and return a `RedirectResult`, consuming the redirect as one of TempData's "next requests"
- A filter-issued redirect is a full round-trip: TempData written before the redirect is available in the redirected request, consumed on read
- If the redirected action also redirects (e.g., an authorization filter chain), TempData is consumed by the *first* response that reads it — subsequent redirects see null
- `Peek` reads without marking as consumed; `Keep` re-marks an already-read entry so it survives into the next request

**Answer**

`TempData` persists across exactly one redirect by serializing to the session store (or a signed cookie). When you write `TempData["Message"] = "Saved"` and then call `RedirectToAction`, the redirected action can read that value. On read, the framework marks the entry for deletion at the end of that request — which is what "one-request-only" means.

The interaction with action filters is subtle. Suppose a global action filter runs `OnActionExecuting`, finds a business rule violation, writes `TempData["Error"] = "Subscription expired"`, and returns a `RedirectToAction` to a landing page. That redirect is the normal cross-redirect hop, so TempData works as expected — the landing page reads and displays the message. Problems arise in layered filter chains: if the landing page's own authorization filter also redirects to a login page, TempData has already been loaded into memory for the landing-page request. When the landing-page filter's redirect response is sent, TempData's read flag causes the entry to be cleared. The login page sees null.

`Peek` solves read-without-consume: `TempData.Peek("Error")` returns the value but leaves the entry in place for the next request. `Keep` re-marks a value that was already read: call `TempData.Keep("Error")` after reading to ensure it survives one additional request. Both are necessary when a multi-hop filter chain or a conditional redirect path must carry TempData farther than one hop.

---

## CQ7. What is the ambiguous-match gotcha when attribute routing and conventional routing coexist, and how does constraint precedence resolve ties?

**Concepts**
- Conventional routes are registered via `app.MapControllerRoute(...)` and matched top-down
- A controller decorated with any `[Route]` or `[HttpGet]` attribute uses attribute routing exclusively — those actions no longer participate in any conventional template
- Partial decoration (some actions attributed, others not) leaves undecorated actions unreachable — their URLs are never registered
- When two routes score equally specific for the same request URL, ASP.NET Core throws `AmbiguousMatchException` at runtime, not at startup
- Constraint specificity order: literal segment > constrained parameter (`{id:int}`) > unconstrained parameter (`{id}`) > catch-all (`{**rest}`)

**Answer**

When both conventional and attribute routing are active in the same application, ASP.NET Core does not silently pick one winner. The routing middleware scores every registered endpoint against the incoming URL and selects the best match. If a controller class or any of its actions carries a `[Route]`, `[HttpGet]`, or similar attribute, attribute routing takes full ownership of that controller — none of its actions participate in the conventional route table. This is all-or-nothing per controller: decorating only half the actions leaves the other half with no registered URL and results in 404s that can be hard to diagnose because no error is thrown at startup.

The ambiguous-match trap fires at runtime when two endpoints — whether both attribute-routed, both conventional, or one of each — produce an identical URL pattern with the same specificity. The framework throws `AmbiguousMatchException` rather than silently taking the first match, making the bug loud but sometimes surprising in production if a new route was added without noticing the collision.

Constraint precedence is how the framework breaks near-ties. A literal segment (`/products/featured`) scores higher than a parameterized one (`/products/{slug}`), which scores higher than the same parameter with a constraint (`{slug:alpha}`). Wait — a constrained parameter scores *higher* than an unconstrained one because the constraint narrows the candidate set and represents a more specific intent. When two routes are genuinely equal in specificity and both pass their constraints for the same request, the ambiguous-match exception fires regardless of registration order. The fix is to add a differentiating constraint — `{id:int}` versus `{slug:alpha:minlength(3)}` — or restructure the templates so one is a strict subset of the other.

---

## CQ8. How do Tag Helpers replace HTML Helpers, what does asp-for bind to, and how do you build a custom Tag Helper that emits IHtmlContent?

**Concepts**
- HTML Helpers are C# extension methods on `IHtmlHelper` returning `IHtmlContent`; the markup is hidden inside a method call
- Tag Helpers are C# classes targeting HTML elements by tag name or attribute — the source reads as HTML, not C#
- `asp-for="PropertyName"` binds to the view's `@model` expression and derives `name`, `id`, `type`, and `data-val-*` attributes from the property and its DataAnnotations
- Custom Tag Helpers extend `TagHelper`, decorate with `[HtmlTargetElement]`, and override `Process` / `ProcessAsync`
- `output.Content.SetHtmlContent(...)` or `AppendHtml(...)` writes the rendered output; register via `@addTagHelper *, YourAssembly` in `_ViewImports.cshtml`

**Answer**

HTML Helpers predate Tag Helpers and work as C# method calls embedded in Razor: `@Html.TextBoxFor(m => m.Email)`. They return `IHtmlContent` and produce the correct `<input>` element, but the markup is invisible inside the method call — front-end developers editing the view see C#, not HTML. Tag Helpers flip this: you write `<input asp-for="Email" />`, and ASP.NET Core's Razor compiler recognizes `asp-for` as a server-side instruction. The compiled output is the same `<input>` with correct `name`, `id`, `type`, and validation data attributes, but the source file reads as ordinary HTML with extra attributes.

The `asp-for` attribute accepts a property name rooted in the `@model` type. The framework reads the property's CLR type to infer `type="number"` or `type="text"`, reads DataAnnotations such as `[DataType(DataType.Password)]` to emit `type="password"`, and reads `[Required]`, `[Range]`, and `[StringLength]` to populate `data-val-required`, `data-val-range-min`, etc. This is why the `@model` directive is mandatory for `asp-for` to work — without a declared model type there is no expression context.

A custom Tag Helper extends `TagHelper`, is annotated with `[HtmlTargetElement("alert-box")]` (matching either a tag name or an attribute), and overrides `Process(TagHelperContext context, TagHelperOutput output)`. Inside, call `output.Content.SetHtmlContent("<div class=\"alert\">...</div>")` or build content via `output.Content.AppendHtml(...)`. For asynchronous operations, override `ProcessAsync` instead. Register the assembly in `_ViewImports.cshtml` with `@addTagHelper *, YourAssembly` so Razor can discover the class at compile time.

---

## CQ9. How does _ViewStart.cshtml resolve layout selection inside an Area, and what causes the "layout could not be located" gotcha?

**Concepts**
- Razor discovers `_ViewStart.cshtml` by walking up from the view's directory toward the root — the first file found wins
- An Area view at `Areas/Admin/Views/Dashboard/Index.cshtml` checks `Areas/Admin/Views/Dashboard/`, then `Areas/Admin/Views/`, then `Views/`, then root
- A `_ViewStart.cshtml` placed in `Areas/Admin/Views/` takes priority over the root `Views/_ViewStart.cshtml` for all Admin Area views
- When the Area's `_ViewStart.cshtml` sets `Layout = "_Layout"`, Razor searches for the file using the Area view location expanders — first in `Areas/Admin/Views/Shared/_Layout.cshtml`
- If that file is absent, Razor throws `InvalidOperationException: Layout '_Layout' could not be located` even though the identical file exists in `Views/Shared/`

**Answer**

Razor resolves `_ViewStart.cshtml` by ascending the directory tree from the view being rendered. For an Area view at `Areas/Admin/Views/Dashboard/Index.cshtml`, Razor checks the `Dashboard/` subfolder, then `Areas/Admin/Views/`, then the root `Views/` folder. The first `_ViewStart.cshtml` found applies its `Layout` setting and stops the upward search.

The gotcha arises when you create `Areas/Admin/Views/_ViewStart.cshtml` and set `Layout = "_Layout"`. Razor now resolves the layout name using the Area's registered view location expanders, which expand `"_Layout"` to `~/Areas/Admin/Views/Shared/_Layout.cshtml` before falling back to the root. If that Area-specific path does not exist, Razor throws even though `~/Views/Shared/_Layout.cshtml` is right there and would have been used had you never created the Area `_ViewStart.cshtml`.

There are two clean fixes. The first is to use an absolute virtual path in the Area's `_ViewStart.cshtml`: `Layout = "~/Views/Shared/_Layout.cshtml"`. Absolute paths bypass the relative expander chain entirely. The second is to place a `_Layout.cshtml` (or a thin wrapper that sets `Layout = "~/Views/Shared/_Layout.cshtml"`) in `Areas/Admin/Views/Shared/`. The absolute-path approach is preferred because it avoids file duplication and makes the layout dependency explicit. If you want the Area to use a genuinely different layout, place the full layout file in the Area's `Shared/` folder and reference it with a relative name.

---

## CQ10. Why is the @model directive required for strongly-typed views, and how do DisplayFor and EditorFor differ in their relationship to DataAnnotations?

**Concepts**
- Without `@model`, `Model` is typed as `dynamic` — property typos compile silently and fail at runtime
- `@model MyApp.ViewModels.ProductViewModel` generates a strongly-typed `Model` property, enabling lambda expressions and compile-time view checking
- `@Html.DisplayFor(m => m.BirthDate)` renders read-only output via display templates; respects `[DisplayFormat]` and `[DataType]` for presentation
- `@Html.EditorFor(m => m.BirthDate)` renders an editable `<input>` via editor templates; respects `[DataType]` and `[UIHint]` to choose the control type
- Tag Helper `<input asp-for="BirthDate" />` is the preferred equivalent for `EditorFor` in .NET 10 — it also populates `data-val-*` attributes in one declaration

**Answer**

The `@model` directive at the top of a Razor file instructs the Razor compiler to generate a `Model` property of the specified type. Without it, `Model` is `dynamic`, which means the compiler accepts any property access — `Model.Nme` compiles fine and fails with a `RuntimeBinderException` at render time. With `@model`, the compiler validates property names, enables IntelliSense in IDEs, and (with Razor compilation on, the .NET 10 default) surfaces typos as build errors before the application ships.

`@Html.DisplayFor(m => m.BirthDate)` uses display templates to render a read-only representation. It consults `[DisplayFormat(DataFormatString = "{0:d}")]` to apply a date format string, and `[DataType(DataType.EmailAddress)]` to render an `<a href="mailto:...">` hyperlink. It never emits an editable control.

`@Html.EditorFor(m => m.BirthDate)` uses editor templates to render an input control. It reads `[DataType(DataType.Password)]` to emit `<input type="password" />`, `[DataType(DataType.MultilineText)]` to emit `<textarea>`, and `[UIHint("DatePicker")]` to select a custom template from `Views/Shared/EditorTemplates/DatePicker.cshtml`. The Tag Helper equivalent `<input asp-for="BirthDate" />` covers the same cases while also writing `data-val-*` attributes for client-side validation in a single element declaration, making it the cleaner and now-preferred form-field syntax. `DisplayFor` and `EditorFor` remain useful when you need template-driven rendering or custom editor templates that Tag Helpers cannot replicate inline.

---

## CQ11. How does jQuery Validate unobtrusive read data-val-* attributes from DataAnnotations, and why do custom validators need IClientModelValidator?

**Concepts**
- Tag Helpers and HTML Helpers emit `data-val="true"` plus rule-specific attributes (`data-val-required`, `data-val-range-min`, `data-val-length-max`) derived from DataAnnotation attributes on the ViewModel property
- `jquery.validate.unobtrusive.js` scans the DOM on page load, reads those attributes, and registers jQuery Validate rules — no server round-trip required for inline errors
- Built-in annotations (`[Required]`, `[Range]`, `[StringLength]`, `[EmailAddress]`, `[RegularExpression]`) each have a built-in model validator that also implements `IClientModelValidator`
- A custom `ValidationAttribute` only runs server-side unless it also implements `IClientModelValidator`
- `IClientModelValidator.AddValidation(ClientModelValidationContext)` adds entries to `context.Attributes` — these become the `data-val-*` attributes; a matching JavaScript adapter must also be registered

**Answer**

ASP.NET Core's Tag Helpers emit `data-val="true"` on any field whose ViewModel property carries a DataAnnotation. Additional attributes specify the rule: `data-val-required="The Name field is required."`, `data-val-range-min="1"`, `data-val-range-max="100"`, and so on. When `jquery.validate.unobtrusive.js` loads, it reads these attributes, maps them to jQuery Validate rule names via registered adapters, and attaches the rules to the form. Field errors appear inline without a server round-trip — the standard user experience for MVC forms.

This automatic pipeline works for built-in annotations because each one's internal model validator implements `IClientModelValidator`. When you create a custom attribute — say, `[FutureDateOnly]` inheriting `ValidationAttribute` — only the `IsValid` server-side override exists. The client sees no `data-val-futuredate` attribute, jQuery Validate has no rule, the form submits successfully, and only then does the server reject the value with a `ModelState` error. This two-step failure is jarring and erodes trust in the form.

To close the gap, implement `IClientModelValidator` on the custom attribute and override `AddValidation`:

```csharp
public void AddValidation(ClientModelValidationContext context)
{
    context.Attributes["data-val"] = "true";
    context.Attributes["data-val-futuredate"] = "Date must be in the future.";
}
```

Then register the JavaScript side using `$.validator.unobtrusive.adapters.addBool("futuredate")` paired with `$.validator.addMethod("futuredate", function (value) { ... })`. Both halves are required — the server side emits the attribute, the client side interprets it. In .NET 10, this contract is unchanged; the only improvement is that `ProblemDetails` responses from `BadRequest(ModelState)` make it easier to surface these server-side rejections in SPA clients that bypass unobtrusive validation entirely.

---

## CQ12. How does IExceptionFilter differ from global exception-handling middleware, and what gap does IAlwaysRunResultFilter fill that regular result filters leave?

**Concepts**
- `IExceptionFilter.OnException` runs within the MVC filter pipeline — after action execution, before the response is written; setting `context.ExceptionHandled = true` suppresses further propagation
- `UseExceptionHandler` middleware sits outside the MVC pipeline and catches anything the filter did not handle — the last-resort safety net
- Execution order for unhandled exceptions: controller-scoped `IExceptionFilter` → global MVC `IExceptionFilter` → exception-handling middleware
- A regular `IResultFilter` is not called when an authorization or resource filter short-circuits execution by assigning `context.Result`
- `IAlwaysRunResultFilter` fires for every result — including short-circuit results from authorization and resource filters — making it the correct hook for unconditional response transformations

**Answer**

`IExceptionFilter.OnException` runs inside the MVC pipeline, giving it access to the `ActionContext`, route data, and the current controller. A controller-scoped exception filter (applied via `[TypeFilter(typeof(MyExceptionFilter))]`) runs first; a globally registered one runs second. If any filter sets `context.ExceptionHandled = true` and assigns `context.Result`, MVC writes that result as a normal response and the exception never reaches the middleware pipeline. This makes `IExceptionFilter` well-suited for business exceptions — `EntityNotFoundException` mapping to a 404 view, or `UnauthorizedOperationException` mapping to a custom 403 page — because the filter has full view-rendering context.

`UseExceptionHandler` middleware runs outside MVC entirely. It catches any exception that the filter pipeline did not handle and is the correct place for infrastructure-level error handling — logging, `ProblemDetails` formatting, generic error pages. It does not have access to MVC route data or controller context.

The `IAlwaysRunResultFilter` gap: when an authorization filter short-circuits by assigning `context.Result = new UnauthorizedResult()`, the MVC pipeline skips action and standard result filters — a regular `IResultFilter` is never called for that result. `IAlwaysRunResultFilter` is invoked for every result, including short-circuit responses. This is the right extension point for unconditional cross-cutting output work — adding security response headers, enforcing a consistent JSON envelope, or appending audit log entries — that must apply to 401 and 403 responses just as much as to successful action results. Register it globally via `builder.Services.AddControllersWithViews(o => o.Filters.Add<ResponseEnvelopeFilter>())`.

---

## CQ13. When should an AJAX action return PartialViewResult vs JsonResult, how is AJAX detected reliably, and why does AJAX break the PRG pattern?

**Concepts**
- `PartialViewResult` returns a rendered HTML fragment — suitable when the client wants markup ready to inject into the DOM
- `JsonResult` returns serialized data — suitable when the client will format the data itself or when the response carries instructions (redirect URL, success flag)
- jQuery sets `X-Requested-With: XMLHttpRequest` automatically; the Fetch API does not — detection must not rely on this header alone
- The more reliable modern signal: send `Accept: application/json` in all AJAX requests and branch on `Request.Headers.Accept`
- Post-Redirect-Get prevents duplicate submissions by redirecting after POST; AJAX follows `302` transparently, so the browser address bar never changes — simulate PRG by returning a redirect URL in JSON and navigating with `window.location.href`

**Answer**

`PartialViewResult` is the right return type when the AJAX call's sole purpose is to refresh a section of the page with rendered markup — a filtered product list, a comment thread, a summary widget. The client inserts the HTML directly: `document.getElementById("results").innerHTML = response`. `JsonResult` is correct when the client needs structured data to format itself, or when the response is an instruction rather than content — `{ "success": true, "redirectUrl": "/orders/42" }` tells the client what to do next without prescribing markup.

Post-Redirect-Get prevents the browser's "resubmit form?" dialog on refresh: after a successful POST, the server issues a `302` redirect to a GET endpoint, and the browser stores the GET URL in history. AJAX breaks this because `XMLHttpRequest` and `fetch` follow `302` redirects automatically and transparently — the AJAX caller receives the body of the redirected GET response without ever changing the browser's address bar. Refreshing the page still shows the POST URL. To replicate PRG semantics over AJAX, return `Json(new { success = true, redirectUrl = Url.Action("Index", "Orders") })` for successful submissions, and have the client execute `window.location.href = data.redirectUrl`. The browser then navigates, updating history correctly.

For AJAX detection, jQuery adds `X-Requested-With: XMLHttpRequest` but `fetch` does not. The most robust approach in .NET 10 is a two-layer check: first, look for an explicit custom header your JavaScript always sets; second, check `Request.Headers.Accept` for `"application/json"` using `Request.Headers.Accept.Any(h => h.MediaType == "application/json")`. This works regardless of the JavaScript library used and degrades gracefully when the same action serves full-page requests.

---

## CQ14. How does Area route registration order interact with conventional routes, and what does the [Area] attribute actually do for controller discovery?

**Concepts**
- Area routes must be registered *before* the default conventional route — if the default route is registered first, it can match Area URLs before the Area route gets evaluated
- `app.MapAreaControllerRoute("admin_default", "Admin", "Admin/{controller=Dashboard}/{action=Index}/{id?}")` uses a literal `Admin` segment to prevent shadowing
- The `[Area("Admin")]` attribute on a controller marks it as belonging to that area in the route table — physical folder location alone is not sufficient
- Without `[Area]`, a controller in `Areas/Admin/Controllers/` is unreachable via the area route even though it is in the expected folder
- The shadow risk: a root `HomeController` and an Area `HomeController` without a literal area segment in the template causes ambiguous or wrong routing

**Answer**

In .NET 10, area routes are registered in `Program.cs` before the fallback conventional route:

```csharp
app.MapAreaControllerRoute("admin", "Admin", "Admin/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

Registration order is significant for conventional routes: the middleware tests templates top-down and stops at the first match. If the default route is registered first, a URL like `/Admin/Products/List` matches `{controller=Admin}` / `{action=Products}` / `{id=List}` — routing to a non-existent `AdminController.Products` action — before the area route is ever evaluated. The literal `Admin/` segment in the area template prevents this by requiring a more specific match that the default template cannot supply.

The `[Area("Admin")]` attribute is not optional convention — it is the mechanism by which a controller is registered in MVC's endpoint table as belonging to an area. A controller class physically located in `Areas/Admin/Controllers/DashboardController.cs` is not automatically associated with the "Admin" area. Without `[Area("Admin")]` on the class, the area route template `Admin/{controller}/{action}` has no endpoint that satisfies both the area constraint and the controller name "Dashboard", and the URL returns 404. Adding the attribute tells the route table that this controller handles the "Admin" area token.

The controller-name-collision shadow risk occurs when both a root `HomeController` and an `Areas/Admin/HomeController` exist. An overly broad area route template — one using `{area}` as a parameter rather than a literal segment — can match root URLs when `area` defaults to empty string or null, routing requests to the Area controller unexpectedly. Always use a literal area segment or an `{area:regex(Admin)}` constraint.

---

## CQ15. How does the model binder handle nested ViewModels and collection binding, and what is the missing-hidden-input data-corruption gotcha?

**Concepts**
- Nested ViewModel binding uses dot-notation prefixes: a field named `Address.Street` binds to `ViewModel.Address.Street`
- Collection binding uses indexed names: `Items[0].Quantity`, `Items[1].ProductId`; the index sequence must be contiguous starting at 0 — a gap stops binding at the first missing index
- Tag Helpers with `asp-for="Items[i].Property"` inside a `for` loop generate the correct indexed names automatically
- The missing-hidden-input gotcha: properties absent from the POST body revert to their CLR default (null, 0, false) — not their pre-edit database values
- Fix: include `<input type="hidden" asp-for="Id" />` for every property the form doesn't edit but the controller needs, or reload from the database and apply only posted fields

**Answer**

ASP.NET Core's default model binder maps form field names to action parameter properties by name matching with prefix navigation. For a `ProductViewModel` with an `Address` property of type `AddressViewModel`, the posted form must contain fields named `Address.Street`, `Address.City`, and `Address.PostalCode`. The binder splits the name on the dot, navigates the object graph, and assigns each leaf value. If the form uses flat names without the prefix — `Street`, `City` — the `Address` property is created with null leaves.

Collection binding for `List<OrderLineViewModel> Items` uses indexed names: `Items[0].Quantity`, `Items[0].ProductId`, `Items[1].Quantity`. The index sequence must start at 0 and be contiguous. If `Items[0]` is missing or there is a gap between indices 0 and 2, the binder processes entries only up to the gap. Using a `for` loop in Razor with `asp-for="Items[@i].Quantity"` generates the correct names automatically; a `foreach` loop does not produce indexed names and prevents collection binding.

The missing-input data-corruption bug is the most common production mistake with this binding model. A POST form that renders only editable fields — omitting `<input type="hidden" asp-for="Id" />` for the record key, or `asp-for="CreatedDate"` — causes those properties to bind as `0`, `null`, or `false`. If the controller passes this partially populated ViewModel to an ORM's `Update` method, the database record is silently overwritten with those defaults. The two reliable fixes are: include a hidden input for every property the form needs but does not edit; or, for sensitive properties, reload the entity from the database inside the action and apply only the whitelisted posted fields using `TryUpdateModelAsync<TEntity>(entity, "", m => m.AllowedProp1, m => m.AllowedProp2)`.

---

## CQ16. How do required vs optional @RenderSection, nested layouts, and the PartialAsync-vs-Component.InvokeAsync choice interact?

**Concepts**
- `@RenderSection("Scripts")` defaults to `required: true` — every view using this layout must define `@section Scripts { }` or Razor throws at render time
- `@RenderSection("Scripts", required: false)` makes the section optional; `@IsSectionDefined("Scripts")` tests for it before rendering a fallback
- In a nested layout (`_AdminLayout.cshtml` → `_Layout.cshtml`), a `@section` defined in the child view is available only to its immediate layout — it does not propagate automatically to the root layout
- `@await Html.PartialAsync("_Widget")` renders a `.cshtml` partial file, sharing the current view's `ViewData` and receiving an optional model argument; it has no independent business logic
- `@await Component.InvokeAsync("Widget")` runs a `ViewComponent` C# class with constructor-injected dependencies, its own `InvokeAsync` method, and isolated `ViewData`

**Answer**

`@RenderSection` in a layout file declares a named placeholder that child views can fill. The `required` parameter (default `true`) controls whether every view using the layout must define that section — if a view omits a required section, Razor throws an `InvalidOperationException` at render time. Setting `required: false` makes the section optional, and `@IsSectionDefined("Scripts")` lets the layout conditionally render fallback content or nothing. The standard pattern is `@RenderSection("Scripts", required: false)` at the bottom of `_Layout.cshtml` so individual views can inject page-specific scripts.

Nested layouts surface a common misunderstanding: `_AdminLayout.cshtml` may itself set `Layout = "_Layout.cshtml"`, rendering inside the root layout. But a `@section Scripts { }` block defined in a child view is available only to `_AdminLayout.cshtml` — Razor does not propagate sections upward automatically. To pass a section through, `_AdminLayout.cshtml` must declare its own `@section Scripts { @RenderSection("Scripts", required: false) }`, explicitly forwarding the child's section to the root layout's placeholder.

`Html.PartialAsync` renders a Razor `.cshtml` file, sharing the calling view's `ViewData` context. It is lightweight and appropriate for pure presentation reuse — a reusable card layout, a table header — where no server-side logic or dependency injection is needed. `Component.InvokeAsync` runs a `ViewComponent` class, which has its own `InvokeAsync(...)` method, receives constructor-injected services, and maintains isolated `ViewData`. Choose a ViewComponent when the widget must query a service, apply business logic, or manage its own data — not just format data the parent view already has. In .NET 10, ViewComponents also support the Tag Helper syntax `<vc:widget items="Model.Items" />`, making them composable alongside native HTML in Razor files.

---

## CQ17. How does TempData's backing store (session vs cookie) affect its availability, and why must flash messages call Keep() in multi-step flows?

**Concepts**
- `CookieTempDataProvider` (the .NET 6+ default) serializes TempData to a signed, Base64-encoded cookie — client-side storage, 4 KB limit applies after encoding
- `SessionStateTempDataProvider` stores data server-side (in-memory, Redis, or SQL Server); requires `services.AddSession()` and scales to large payloads
- TempData is loaded at request start; entries written by the current action are saved at response end and available to the *next* request
- Writing to TempData and returning a `ViewResult` (not a redirect) in the same request does not make the new entries visible in that view — they are for the next request
- In a multi-step wizard, reading TempData at each intermediate step marks entries consumed; `Keep("key")` after each read re-queues the entry for one more request

**Answer**

`CookieTempDataProvider`, the default since .NET 6, serializes the entire TempData dictionary to a signed cookie using `IDataProtector`. The data lives in the browser — no server session store is required — but the 4 KB cookie size limit constrains the payload. Large objects such as complex ViewModels or lists must either be trimmed, stored in a server-side session, or kept as IDs with a database lookup. Switch to `SessionStateTempDataProvider` by calling `services.AddSession()` and `services.AddMvc().AddSessionStateTempDataProvider()`, then choosing a distributed cache (Redis, SQL Server) for multi-server deployments.

The load-then-save cycle is important: TempData is loaded from the provider at the very start of a request. Entries a *prior* request wrote are now available. Entries written by the *current* action are saved at the end of the current request and become available to the next one. This means writing `TempData["Flash"] = "Saved"` and returning a `ViewResult` in the same action does not make `"Flash"` visible to that view — it is for the next request. Use `ViewBag` or the ViewModel for same-request display; use TempData only across a redirect.

Multi-step wizard flows extend this further. If step 2 reads `TempData["WizardData"]` to display a summary, the read marks the entry consumed. When step 3 loads, the entry is gone. Calling `TempData.Keep("WizardData")` immediately after reading re-queues the entry for one additional request, carrying it to step 3. Each intermediate step that reads and needs to forward the data must call `Keep`. For many-step flows, consider a dedicated session key or a database-backed draft record instead of chaining `Keep` calls.

---

## CQ18. When should you use SignalR vs AJAX polling for live UI updates, and how does the hybrid notify-then-fetch pattern combine both?

**Concepts**
- AJAX polling issues periodic `fetch` requests at a fixed interval — simple, works everywhere HTTP works, but generates O(clients × frequency) server requests regardless of whether data changed
- SignalR maintains a persistent connection per client (WebSocket preferred; SSE and long-polling as fallbacks) — the server pushes only when data changes, eliminating idle overhead
- WebSocket overhead at scale: Kestrel in .NET 10 handles tens of thousands of concurrent async connections, but a backplane (Redis, Azure SignalR Service) is required for multi-instance deployments
- Hybrid pattern: SignalR sends a lightweight `"orderUpdated"` event containing only the entity ID; the client fires `fetch("/api/orders/{id}")` to retrieve the full payload — decouples transport from data shape
- Choose polling when: updates are infrequent, WebSockets are blocked by a proxy, or simplicity outweighs real-time fidelity

**Answer**

AJAX polling — a `setInterval` firing a `fetch` every few seconds — is the path of least resistance: it works everywhere HTTP works, requires no persistent connection infrastructure, and is trivial to debug. Its cost scales linearly with clients and frequency. At 2,000 clients polling every 5 seconds that is 400 requests per second of overhead, most returning "nothing changed." This is acceptable for dashboards that refresh every minute and tolerate a few seconds of latency.

SignalR inverts the cost model. The server pushes a message only when data changes — zero traffic between events. A Kestrel instance in .NET 10 handles tens of thousands of WebSocket connections via async I/O without blocking threads, but the infrastructure cost rises: hubs must be registered, CORS must permit credentials, and any multi-instance deployment requires a backplane (Redis pub/sub or Azure SignalR Service) so that a message produced by instance A reaches clients connected to instance B.

The hybrid notify-then-fetch pattern captures the best of both. SignalR sends a minimal event — `Clients.Group(orderId.ToString()).SendAsync("orderUpdated", orderId)` — containing only the entity ID. The JavaScript client handles the event and fires `fetch(`/api/orders/${orderId}`)` to retrieve the full payload. This keeps hub messages tiny, leaves the REST endpoint independently testable and cacheable, and degrades gracefully: if the SignalR connection drops before reconnection, the client can temporarily fall back to polling the REST endpoint. The pattern also avoids the serialization mismatch risk of pushing large objects through the hub whose shape may diverge from what the REST endpoint returns. In .NET 10, the `HubConnectionBuilder` reconnect policy makes this hybrid resilient — automatic reconnect re-establishes the push channel without losing any AJAX-fetched data already on the page.
