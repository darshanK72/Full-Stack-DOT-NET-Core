/*
 * FILE ROLE: Demonstrates complex type binding from HTML form fields, including
 *            nested object binding (dot-notation) and collection binding (index notation).
 *            Also shows [BindNever] and the ViewModel pattern for over-posting prevention.
 * SECTIONS IN THIS FILE:
 *   1a. Complex type binding — dot-notation form fields
 *   1b. Collection binding  — index-notation form fields for List<T>
 *   1c. [BindNever]         — opt a property out of binding
 *   1d. TryUpdateModelAsync — safe post-fetch model update pattern
 *   1e. ViewModel pattern   — over-posting prevention via dedicated binding model
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace ModelBindingMvc.Models;

/*
 * SECTION 1a: COMPLEX TYPE BINDING — DOT-NOTATION FORM FIELDS
 *
 * When an action parameter is a complex type (a class with properties), MVC
 * iterates each public settable property and looks for a matching form field.
 *
 * Name resolution:
 *   By default the binder looks for keys that match:
 *     "<paramName>.<PropertyName>"   e.g. order.CustomerName   (with prefix)
 *     "<PropertyName>"               e.g. CustomerName         (without prefix)
 *   MVC tries WITHOUT prefix first for the outermost model parameter, then WITH.
 *
 * Nested complex types (dot-notation):
 *   If a property is itself a complex type (e.g. ShippingAddress), the binder
 *   recurses and looks for:
 *     "ShippingAddress.Street"
 *     "ShippingAddress.City"
 *   The HTML form MUST use these dot-prefixed names.
 *   See Views/Orders/Create.cshtml for the matching <input> name attributes.
 *
 * DateTime binding:
 *   The binder parses DateTime using InvariantCulture for standard formats
 *   (ISO-8601 "yyyy-MM-dd", "MM/dd/yyyy", etc.). An empty string or absent field
 *   binding to DateTime? → null; binding to DateTime (non-nullable) → ModelState error.
 *   Best practice: use DateTime? and validate not-null in the controller or
 *   with [Required] so the error message is meaningful.
 */
public class OrderFormModel
{
    public string CustomerName { get; set; } = string.Empty;   // flat scalar — form field "CustomerName"
    public string CustomerEmail { get; set; } = string.Empty;  // flat scalar — form field "CustomerEmail"

    // Nested complex type — HTML: <input name="ShippingAddress.Street" />
    public AddressInput ShippingAddress { get; set; } = new AddressInput();

    // DateTime? — bound from form field "OrderDate"; null when absent or empty
    public DateTime? OrderDate { get; set; }

    /*
     * SECTION 1b: COLLECTION BINDING — INDEX-NOTATION FORM FIELDS
     *
     * To bind a List<T> from form fields, MVC uses EITHER:
     *
     *   1. Index notation (consecutive, zero-based):
     *        Items[0].ProductName
     *        Items[0].Quantity
     *        Items[1].ProductName
     *        Items[1].Quantity
     *      Gaps in indices stop collection binding — Items[0] and Items[2] without
     *      Items[1] will only bind Items[0] (the binder stops at the first missing index).
     *
     *   2. Repeated key notation (scalar collections only):
     *        Tags=Electronics&Tags=Sale&Tags=Featured
     *      Gives: List<string> { "Electronics", "Sale", "Featured" }
     *      Does NOT work for complex element types — use index notation for those.
     *
     *   3. ICollection<T> vs IList<T> vs T[]:
     *      All are supported. The binder creates a List<T> internally and assigns it.
     *      Declare the property as IList<T> or List<T> for full index access.
     *
     * PITFALL — non-sequential indices:
     *   Items[0].ProductName=Widget  + Items[3].ProductName=Gadget
     *   → only Items[0] is bound (binder stops at missing index 1).
     *   Use consecutive 0-based indices in the HTML form.
     */

    // List of order lines — HTML uses: Items[0].ProductName, Items[0].Quantity, etc.
    public List<OrderLineInput> Items { get; set; } = new List<OrderLineInput>();

    // Scalar collection — HTML: Tags=Electronics&Tags=Sale (repeated key)
    public List<string> Tags { get; set; } = new List<string>();

    /*
     * SECTION 1c: [BindNever] — OPT A PROPERTY OUT OF BINDING
     *
     * [BindNever] on a property tells the model binder to NEVER populate it from
     * any HTTP source (form, route, query). The property is completely ignored
     * during binding even if a matching form field is submitted.
     *
     * Use cases:
     *   - Server-computed properties (timestamps, audit fields, generated IDs)
     *   - Properties that must only be set by the application, not user input
     *   - Preventing over-posting for a specific field while keeping the model
     *     usable for output (display in views)
     *
     * PITFALL — [BindNever] vs [Bind]:
     *   [BindNever] works per-property; [Bind("Prop1,Prop2")] works per-model/parameter
     *   and is an allow-list — unlisted properties are treated as BindNever.
     *   They address the same over-posting risk from different directions.
     *
     * Note: [BindNever] does NOT prevent the property from being read or serialized
     *   to JSON — it only affects the INPUT binding side.
     */
    [BindNever]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // set by server; never from form

    [BindNever]
    public string? InternalReference { get; set; }             // internal audit field; never from user
}

/*
 * SECTION 1a (continued): NESTED TYPE — AddressInput
 *
 * This class is a nested complex type inside OrderFormModel.ShippingAddress.
 * The binder will look for form fields named:
 *   "ShippingAddress.Street", "ShippingAddress.City", "ShippingAddress.PostalCode"
 *
 * There is no [BindNever] or other attribute here — all properties are bound.
 * Nullable types (string? → non-nullable + required validation is on the view model side).
 */
public class AddressInput
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "US";  // default value — form can override
}

/*
 * SECTION 1b (continued): COLLECTION ELEMENT TYPE — OrderLineInput
 *
 * Each element of OrderFormModel.Items is an OrderLineInput.
 * HTML form fields:
 *   Items[0].ProductName, Items[0].Quantity, Items[0].UnitPrice
 *   Items[1].ProductName, Items[1].Quantity, Items[1].UnitPrice
 * etc.
 */
public class OrderLineInput
{
    public string ProductName { get; set; } = string.Empty; // Items[N].ProductName
    public int Quantity { get; set; }                       // Items[N].Quantity (int — parse error → ModelState error)
    public decimal UnitPrice { get; set; }                  // Items[N].UnitPrice
}

/*
 * SECTION 1d: TryUpdateModelAsync — SAFE POST-FETCH MODEL UPDATE
 *
 * TryUpdateModelAsync<T>(entity, prefix, includeExpressions)
 *   - Binds the current request's form values ONTO an already-fetched entity.
 *   - Only properties listed in the lambda expressions are updated (allow-list).
 *   - Returns false if binding or DataAnnotations validation fails, and populates
 *     ModelState with the errors.
 *   - Prevents over-posting because you explicitly name each allowed property.
 *
 * Typical use (in an Edit POST action):
 *   var existingOrder = await _repo.GetByIdAsync(id);        // fetch first
 *   bool ok = await TryUpdateModelAsync(existingOrder, "",   // update in place
 *       o => o.CustomerName, o => o.ShippingAddress, o => o.OrderDate);
 *   if (!ok) return View(existingOrder);
 *   await _repo.SaveAsync();
 *   return RedirectToAction("Index");
 *
 * This is the canonical EDIT pattern when working with ORM-tracked entities:
 *   - Fetch from DB (avoids ID spoofing and missing property reset).
 *   - Bind only allowed properties (prevents over-posting).
 *   - Validate via ModelState (prevents invalid state).
 *   - Save only if valid.
 *
 * SECTION 1e: VIEWMODEL PATTERN — OVER-POSTING PREVENTION
 *
 * The safest approach to over-posting is a dedicated ViewModel (binding model)
 * that contains ONLY the properties you want the user to supply. This model:
 *   - Has no [BindNever] clutter — all its properties are intended inputs
 *   - Can be validated independently without polluting the domain entity
 *   - Maps explicitly to the domain entity after validation
 *
 * Example: CreateOrderViewModel below contains only the safe inputs for order
 * creation. The controller maps it to the OrderFormModel (or domain entity) after
 * ModelState.IsValid check, setting server-side fields (CreatedAt, etc.) explicitly.
 *
 * When to use which pattern:
 *   ┌──────────────────────────────┬──────────────────────────────────────────┐
 *   │ Pattern                      │ Best for                                 │
 *   ├──────────────────────────────┼──────────────────────────────────────────┤
 *   │ [BindNever] on model         │ Simple models; a few server-set fields   │
 *   │ [Bind("Prop1,Prop2")]        │ Quick allow-list; string is fragile      │
 *   │ Dedicated ViewModel          │ Recommended for all non-trivial forms    │
 *   │ TryUpdateModelAsync          │ Edit pattern with ORM-tracked entities   │
 *   └──────────────────────────────┴──────────────────────────────────────────┘
 */
public class CreateOrderViewModel
{
    // Only safe user-supplied fields — no CreatedAt, InternalReference, etc.
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public AddressInput ShippingAddress { get; set; } = new AddressInput();
    public DateTime? OrderDate { get; set; }
    public List<OrderLineInput> Items { get; set; } = new List<OrderLineInput>();
    public List<string> Tags { get; set; } = new List<string>();
}
