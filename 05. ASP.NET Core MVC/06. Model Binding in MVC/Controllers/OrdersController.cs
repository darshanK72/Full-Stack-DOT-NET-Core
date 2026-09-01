/*
 * FILE ROLE: MVC controller demonstrating all binding sources ([FromForm], [FromRoute],
 *            [FromQuery], [FromBody]), ModelState validation, [Bind] allow-list,
 *            IFormFile upload, and TryUpdateModelAsync.
 * SECTIONS IN THIS FILE:
 *   5a. Controller setup and GET actions
 *   5b. [FromForm] — POST action with complex type binding
 *   5c. [FromRoute] — route segment parameter
 *   5d. [FromQuery] — query string model
 *   5e. [Bind] allow-list on an action parameter
 *   5f. ModelState — IsValid, Errors, Keys, AddModelError
 *   5g. [FromBody] — JSON body in an MVC controller
 *   5h. IFormFile — file upload action
 *   5i. TryUpdateModelAsync — safe edit pattern
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelBindingMvc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModelBindingMvc.Controllers;

/*
 * SECTION 5a: CONTROLLER SETUP — MVC CONTROLLER vs API CONTROLLER
 *
 * Controller (inheriting from Controller, not ControllerBase) is the MVC base class.
 * It adds View(), PartialView(), ViewBag, TempData, and form-oriented helpers.
 *
 * Key differences from [ApiController] (used in Web API):
 *   ┌─────────────────────────────┬────────────────────┬──────────────────────────────┐
 *   │ Behaviour                   │ MVC Controller     │ ApiController (Web API)      │
 *   ├─────────────────────────────┼────────────────────┼──────────────────────────────┤
 *   │ Binding inference           │ Route→Form→Query   │ Route/Query for simple types; │
 *   │                             │ No body inference  │ Body for complex types        │
 *   │ Automatic ModelState filter │ No                 │ Yes (400 before action runs) │
 *   │ [FromBody] required?        │ Must be explicit   │ Inferred for complex types   │
 *   │ Response type               │ IActionResult+View │ IActionResult (JSON default) │
 *   └─────────────────────────────┴────────────────────┴──────────────────────────────┘
 *
 * In MVC controllers, you MUST check ModelState.IsValid manually in POST actions
 * (the automatic short-circuit only exists with [ApiController]).
 *
 * Route: convention routing configured in Program.cs — {controller}/{action}/{id?}
 */
public class OrdersController : Controller
{
    // Simulated in-memory store (replaces a database for this tutorial)
    private static readonly List<CreateOrderViewModel> _orders = new List<CreateOrderViewModel>();

    /*
     * GET /Orders/Create — display the empty order creation form.
     * No binding here; just return the view with an empty model.
     */
    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateOrderViewModel
        {
            OrderDate = DateTime.Today,
            Items = new List<OrderLineInput> { new OrderLineInput() } // pre-fill one row
        };
        return View(model);
    }

    /*
     * SECTION 5b: [FromForm] — POST ACTION WITH COMPLEX TYPE BINDING
     *
     * [FromForm] explicitly tells the binder to read from the form body.
     * In an MVC controller (no [ApiController]), the binder infers [FromForm] for
     * POST actions with a complex type parameter — explicit [FromForm] makes it
     * unambiguous and documents intent clearly.
     *
     * Binding flow for CreateOrderViewModel:
     *   1. MVC iterates each property of CreateOrderViewModel.
     *   2. For scalar properties (CustomerName, CustomerEmail):
     *        Looks for form field "CustomerName", "CustomerEmail".
     *   3. For nested object (ShippingAddress):
     *        Recurses into AddressInput; looks for "ShippingAddress.Street", etc.
     *   4. For list (Items):
     *        Looks for "Items[0].ProductName", "Items[0].Quantity", "Items[0].UnitPrice",
     *        "Items[1].ProductName", etc. (stops at first missing index).
     *   5. For scalar collection (Tags):
     *        Handled by CsvListModelBinderProvider (global) — "Tags" → split by comma.
     *        OR default: "Tags=a&Tags=b" (repeated key).
     *   6. OrderDate (DateTime?):
     *        Bound from "OrderDate" field; empty string → null; bad format → ModelState error.
     *   7. [BindNever] properties (CreatedAt, InternalReference on OrderFormModel):
     *        SKIPPED entirely by the binder.
     *
     * After binding, MVC runs DataAnnotations validation — results go into ModelState.
     * With an MVC controller (no [ApiController]) the action ALWAYS runs — you must
     * check ModelState.IsValid yourself.
     */
    [HttpPost]
    public IActionResult Create([FromForm] CreateOrderViewModel model)
    {
        /*
         * SECTION 5f: ModelState — IsValid, Errors, Keys, AddModelError
         *
         * ModelState (ModelStateDictionary) accumulates all binding and validation results:
         *
         *   ModelState.IsValid
         *     true  — no binding errors AND no validation attribute failures.
         *     false — at least one error exists. Re-display the form with errors.
         *
         *   ModelState.Errors (via ModelState[key].Errors)
         *     Each key maps to a ModelStateEntry with an Errors collection.
         *     Each ModelError has either an Exception (binding failed to parse)
         *     or an ErrorMessage (DataAnnotations validation failed).
         *
         *   ModelState.Keys
         *     All keys that have entries (not all have errors). Iterate to find errors:
         *       foreach (var key in ModelState.Keys)
         *           foreach (var error in ModelState[key]!.Errors)
         *               // key: e.g. "Items[0].Quantity", error.ErrorMessage
         *
         *   ModelState.AddModelError(key, message)
         *     Add a custom error. Key should be the property name (nameof()) for
         *     client-side validation helpers (asp-validation-for) to display it
         *     next to the correct field. Use "" (empty string) for model-level errors.
         *
         *   ModelState.AddModelError("", message)   — model-level (shown in validation summary)
         *   ModelState.AddModelError(nameof(model.CustomerEmail), message) — field-level
         */
        if (!ModelState.IsValid)
        {
            // Re-display the form; the view reads ModelState errors via Tag Helpers
            return View(model);
        }

        // Business rule validation AFTER ModelState is clean
        if (model.Items == null || model.Items.Count == 0)
        {
            ModelState.AddModelError("", "An order must have at least one item.");
            return View(model);
        }

        // Simulate saving; set server-side fields here (NOT via binding)
        _orders.Add(model);
        TempData["SuccessMessage"] = $"Order for {model.CustomerName} created.";
        return RedirectToAction(nameof(Index));
    }

    /*
     * SECTION 5c: [FromRoute] — ROUTE SEGMENT PARAMETER
     *
     * [FromRoute] binds the parameter from a {name} segment in the route template.
     * Convention routing: {controller}/{action}/{id?} matches /Orders/Details/5 → id = 5.
     *
     * int id (non-nullable): if the route segment is missing or non-integer, the
     * binder adds a ModelState error. Use int? for an optional segment.
     *
     * Route constraints in convention routing ("{id:int}") cause a 404 when the
     * segment is not an integer — the route simply does not match, so ModelState
     * is never involved. Without the constraint, the binder receives the string
     * and fails with a parse error in ModelState.
     *
     * GET /Orders/Details/3
     */
    [HttpGet]
    public IActionResult Details([FromRoute] int id)
    {
        if (id <= 0)
        {
            ModelState.AddModelError(nameof(id), "Id must be a positive integer.");
            return BadRequest(ModelState); // return ModelState error as 400 response
        }

        // Simulate a fetch; return first order or 404
        var order = _orders.ElementAtOrDefault(id - 1);
        if (order == null) return NotFound();
        return View(order);
    }

    /*
     * SECTION 5d: [FromQuery] — QUERY STRING MODEL
     *
     * The SearchQuery model groups all query parameters for the search feature.
     * [FromQuery] is explicit here; MVC would also infer query binding for
     * scalar parameters on a GET action.
     *
     * GET /Orders/Search?keyword=widget&page=2&pageSize=25&categoryIds=1&categoryIds=3
     *
     * Note on Tags (List<string> in SearchQuery — currently not in SearchQuery,
     * but if it were): because CsvListModelBinderProvider is registered globally,
     * a ?tags=a,b,c query string would produce ["a","b","c"] automatically.
     */
    [HttpGet]
    public IActionResult Search([FromQuery] SearchQuery query)
    {
        if (!ModelState.IsValid)
            return View(query); // validation errors on query (e.g. PageSize > max)

        // Simulate filtered results; display back in the view
        ViewBag.ResultCount = _orders.Count;
        ViewBag.Query = query;
        return View(query);
    }

    /*
     * SECTION 5e: [Bind] ALLOW-LIST ON AN ACTION PARAMETER
     *
     * [Bind("Prop1,Prop2")] on the action parameter restricts binding to ONLY
     * the named properties for THAT action call. Properties not listed are skipped
     * as if they had [BindNever] — they keep their default/init values.
     *
     * Overposting prevention example:
     *   A malicious form might include a "CreatedAt" field attempting to set the
     *   order timestamp. With [Bind("CustomerName,CustomerEmail,ShippingAddress")],
     *   OrderDate and Items are ignored even if the form sends them.
     *
     * Limitations of the string-based allow-list:
     *   - Typos in property names are NOT compile-time errors — they silently skip binding.
     *   - Refactoring (property rename) does not update the string automatically.
     *   Prefer the ViewModel approach (CreateOrderViewModel) for production code.
     *   [Bind] is shown here to teach the mechanism; use a dedicated ViewModel in practice.
     *
     * GET+POST /Orders/QuickCreate
     */
    [HttpGet]
    public IActionResult QuickCreate() => View(new CreateOrderViewModel());

    [HttpPost]
    public IActionResult QuickCreate(
        [Bind("CustomerName,CustomerEmail,ShippingAddress")] CreateOrderViewModel model)
    {
        // OrderDate and Items are NOT bound (not in the allow-list)
        // model.Items is always empty here; model.OrderDate is null
        if (!ModelState.IsValid)
            return View(model);

        _orders.Add(model);
        return RedirectToAction(nameof(Index));
    }

    /*
     * SECTION 5g: [FromBody] — JSON BODY IN AN MVC CONTROLLER
     *
     * [FromBody] must be EXPLICIT in MVC controllers — unlike [ApiController], MVC
     * does not infer body binding for complex types.
     *
     * Use case: an MVC controller action that also accepts AJAX / fetch JSON requests
     * alongside standard HTML form actions. Both HTML forms ([FromForm]) and JSON
     * clients ([FromBody]) can hit the same controller at different endpoints.
     *
     * PITFALL — only one [FromBody] per action:
     *   The HTTP request body is a stream read once. A second [FromBody] parameter
     *   throws InvalidOperationException at runtime.
     *
     * POST /Orders/CreateJson
     * Content-Type: application/json
     * Body: { "customerName": "Alice", "customerEmail": "alice@example.com", ... }
     */
    [HttpPost]
    public IActionResult CreateJson([FromBody] CreateOrderViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); // return JSON-compatible errors

        _orders.Add(model);
        return Ok(new { message = "Order created.", count = _orders.Count });
    }

    /*
     * SECTION 5h: IFormFile — FILE UPLOAD ACTION
     *
     * IFormFile parameters ALWAYS come from the form body ([FromForm] is implied).
     * The <form> must use enctype="multipart/form-data".
     *
     * Mixed payload: FileUploadModel has both scalar fields and IFormFile properties.
     * The binder populates all of them from the same multipart request.
     *
     * File validation is performed in the action AFTER binding. The model only
     * holds the binding targets; validation logic lives in the controller.
     *
     * POST /Orders/Upload
     */
    [HttpGet]
    public IActionResult Upload() => View(new FileUploadModel());

    [HttpPost]
    public async Task<IActionResult> Upload(FileUploadModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // File size validation (see Models/FileUploadModel.cs Section 3c)
        if (model.MainFile.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(nameof(model.MainFile), "Main file must be under 5 MB.");
            return View(model);
        }

        // Extension allow-list validation
        var allowed = new[] { ".pdf", ".docx", ".png", ".jpg", ".jpeg" };
        var ext = Path.GetExtension(model.MainFile.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
        {
            ModelState.AddModelError(nameof(model.MainFile), $"File type '{ext}' is not allowed.");
            return View(model);
        }

        // Validate each attachment in IList<IFormFile>
        foreach (IFormFile attachment in model.Attachments)
        {
            if (attachment.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.Attachments),
                    $"Attachment '{attachment.FileName}' exceeds 2 MB.");
                return View(model);
            }
        }

        // Safe save to a temporary path (production: use cloud storage or a configured path)
        // Generate server-side filename — NEVER use attachment.FileName directly as path
        var safeFileName = Guid.NewGuid().ToString("N") + ext;
        var tempPath = Path.Combine(Path.GetTempPath(), safeFileName);
        using (var stream = System.IO.File.Create(tempPath))
        {
            await model.MainFile.CopyToAsync(stream); // async copy — does not block thread
        }

        // Clean up temp file after demo (in production, store path in DB or move to storage)
        System.IO.File.Delete(tempPath);

        TempData["SuccessMessage"] =
            $"Uploaded '{model.MainFile.FileName}' ({model.MainFile.Length} bytes) + " +
            $"{model.Attachments.Count} attachment(s).";
        return RedirectToAction(nameof(Index));
    }

    /*
     * SECTION 5i: TryUpdateModelAsync — SAFE EDIT PATTERN
     *
     * TryUpdateModelAsync binds the current form values onto an existing model object,
     * applying only the explicitly listed properties (allow-list via expressions).
     * It also runs DataAnnotations validation and returns false if anything fails.
     *
     * Why use it instead of a ViewModel:
     *   - ORM-tracked entities: you need to mutate the tracked entity in place;
     *     mapping a ViewModel onto it manually is verbose for many properties.
     *   - TryUpdateModelAsync does the mapping AND validation in one call.
     *
     * The lambda allow-list prevents over-posting without a string literal:
     *   await TryUpdateModelAsync(entity, "", x => x.CustomerName, x => x.ShippingAddress)
     *   Only CustomerName and ShippingAddress are updated from the form.
     *
     * POST /Orders/Edit/3
     */
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var order = _orders.ElementAtOrDefault(id - 1);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, IFormCollection _) // IFormCollection triggers form reading
    {
        var existing = _orders.ElementAtOrDefault(id - 1);
        if (existing == null) return NotFound();

        // TryUpdateModelAsync binds and validates; lambda expressions are the allow-list
        bool updated = await TryUpdateModelAsync(
            existing,
            prefix: "",                             // no prefix — form fields are not prefixed
            x => x.CustomerName,                    // only these properties are updated
            x => x.CustomerEmail,
            x => x.ShippingAddress,
            x => x.OrderDate);

        if (!updated)                               // false = ModelState.IsValid is false
            return View(existing);

        TempData["SuccessMessage"] = "Order updated.";
        return RedirectToAction(nameof(Index));
    }

    // Index action — display all orders
    [HttpGet]
    public IActionResult Index()
    {
        return View(_orders);
    }
}
