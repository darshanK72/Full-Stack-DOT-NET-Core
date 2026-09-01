/*
 * FILE ROLE: Demonstrates [FromQuery] binding for scalar action parameters and
 *            complex query models, including nullable types, defaults, and
 *            [Bind] allow-list on query models.
 * SECTIONS IN THIS FILE:
 *   2a. Scalar action parameters — inference rules for simple types
 *   2b. [FromQuery] on a complex model — all properties from query string
 *   2c. [Bind] allow-list on a model class
 *   2d. Nullable and default value binding
 */

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ModelBindingMvc.Models;

/*
 * SECTION 2a: SCALAR ACTION PARAMETERS — BINDING INFERENCE RULES
 *
 * MVC infers binding sources for action parameters following this priority:
 *
 *   1. Route segment match  → [FromRoute] (name matches a {segment} in the template)
 *   2. Form data            → [FromForm]  (if method=POST and content-type is form)
 *   3. Query string         → [FromQuery] (fallback for GET / unmatched names)
 *
 * "Simple types" that follow this inference:
 *   int, long, float, double, decimal, bool, char, string, Guid, DateTime,
 *   DateTimeOffset, TimeSpan, and their Nullable<T> counterparts.
 *
 * Example action signature (see Controllers/OrdersController.cs):
 *   public IActionResult Search(string? keyword, int page, int pageSize)
 *
 *   Routing: GET /Orders/Search?keyword=widget&page=2&pageSize=25
 *     keyword  → [FromQuery] (no route segment)
 *     page     → [FromQuery] (no route segment; default value = 1 if absent)
 *     pageSize → [FromQuery] (no route segment; default value = 20 if absent)
 *
 *   Route: GET /Orders/Search/3 (route template {controller}/{action}/{id?})
 *     An "id?" segment exists in the default route template but NOT "page" or "pageSize",
 *     so they still come from the query string.
 *
 * PITFALL — null vs missing:
 *   For a parameter declared as int (non-nullable) with no matching query key,
 *   the binder adds a ModelState error ("The value '' is invalid").
 *   Use int? or give the parameter a default value (int page = 1) to make it optional.
 *
 * PITFALL — string and [Required]:
 *   string without [Required] binds to null when the key is absent.
 *   string? makes the intent explicit; add [Required] to force presence.
 */

/*
 * SECTION 2b: [FromQuery] ON A COMPLEX QUERY MODEL
 *
 * Instead of listing many scalar parameters, group them in a class and annotate
 * the action parameter with [FromQuery]. MVC maps each property from the query string.
 *
 * Name resolution: property names are matched CASE-INSENSITIVELY against query keys.
 *   ?keyword=widget    → Keyword = "widget"
 *   ?page=2            → Page = 2
 *   ?sortBy=name       → SortBy = "name"
 *
 * Collection properties inside a [FromQuery] model:
 *   ?categoryIds=1&categoryIds=3&categoryIds=7   → CategoryIds = [1, 3, 7]  (repeated key)
 *   ?categoryIds[0]=1&categoryIds[1]=3           → same (index notation also works)
 *
 * Default values in C# property initializers are honoured — the binder only
 * overwrites a property if the key is present in the query string.
 */
public class SearchQuery
{
    // Optional search keyword; null when key absent from query string
    public string? Keyword { get; set; }

    // Pagination — defaults applied when query key is absent
    public int Page { get; set; } = 1;         // ?page=N; defaults to 1
    public int PageSize { get; set; } = 20;    // ?pageSize=N; defaults to 20

    // Sort control
    public string? SortBy { get; set; }        // ?sortBy=price
    public bool SortDescending { get; set; }   // ?sortDescending=true

    /*
     * SECTION 2d: NULLABLE AND DEFAULT VALUE BINDING
     *
     * Nullable<T> (T?) binding rules:
     *   - Key present with a parseable value → bound to that value
     *   - Key present with empty string      → null (not a ModelState error)
     *   - Key absent                         → null (not a ModelState error)
     *   - Key present with non-parseable value → ModelState error
     *
     * Non-nullable T (e.g. int) binding rules:
     *   - Key present with a parseable value → bound to that value
     *   - Key present with empty or non-parseable string → ModelState error
     *   - Key absent                         → ModelState error (missing required value)
     *   Fix: use int? or int page = 1 (default) to make the parameter optional.
     *
     * DateTime? example:
     *   ?startDate=2024-01-15  → StartDate = new DateTime(2024, 1, 15)
     *   ?startDate=            → StartDate = null
     *   (absent)               → StartDate = null
     */
    public decimal? MinPrice { get; set; }     // ?minPrice=10.00; null when absent
    public decimal? MaxPrice { get; set; }     // ?maxPrice=99.99; null when absent

    // Collection property — repeated key or index notation
    public List<int> CategoryIds { get; set; } = new List<int>(); // ?categoryIds=1&categoryIds=2

    /*
     * SECTION 2c: [Bind] ALLOW-LIST ON A MODEL CLASS
     *
     * [Bind("Prop1,Prop2,...)] applied to a class (not a parameter) restricts
     * binding to ONLY the named properties. Any property not in the list is treated
     * as if it has [BindNever] — the binder ignores matching form/query keys.
     *
     * How it differs from [Bind] on a parameter:
     *   [Bind("Name,Price")] on the class → affects ALL places where this class
     *     is used as a binding target (all actions that accept SearchQuery).
     *   [Bind("Name,Price")] on the action parameter → affects only THAT action.
     *   Prefer the parameter-level form for flexibility; class-level is too broad.
     *
     * Example (class-level — shown here conceptually; not applied to avoid limiting demos):
     *   [Bind("Keyword,Page,PageSize,SortBy,SortDescending")]
     *   public class SearchQuery { ... }
     *
     * Overposting scenario without [Bind]:
     *   A malicious request sends ?internalFlag=true. If SearchQuery had an
     *   InternalFlag property without [BindNever] and no [Bind] allow-list, it
     *   would be bound. The allow-list or ViewModel pattern prevents this.
     *
     * Practical recommendation:
     *   Use a dedicated ViewModel (SearchQuery here has only safe fields) rather
     *   than [Bind] on a domain entity. Reserve [Bind] for quick allow-listing
     *   when a full ViewModel split is not warranted.
     */
}
