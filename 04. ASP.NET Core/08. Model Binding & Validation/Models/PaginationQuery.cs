/*
 * FILE ROLE: Shows how ASP.NET Core binds a complex query-string model and
 *            how to receive collections from the query string.
 * SECTIONS IN THIS FILE:
 *   2a. [FromQuery] complex-type binding — how sub-properties map to query keys
 *   2b. Collection binding — List<T> from repeated keys or comma-separated values
 *   2c. Default values and optional parameters
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModelBindingValidation.Models;

/*
 * SECTION 2a: [FromQuery] COMPLEX-TYPE BINDING
 *
 * When you annotate a controller parameter with [FromQuery], ASP.NET Core reads
 * every public settable property on the model from the query string.
 *
 * Key name mapping:
 *   By default, property names are matched case-insensitively:
 *     GET /api/products?page=2&pageSize=50&sortBy=name&sortDescending=true
 *   maps to Page=2, PageSize=50, SortBy="name", SortDescending=true.
 *
 *   Override the query key name with [FromQuery(Name = "sort_by")] on the
 *   controller parameter OR with [BindProperty(Name = "…")] on the model property.
 *
 * Why use a model class instead of separate parameters?
 *   Separate parameters: GetAll(int page, int pageSize, string? sortBy, ...)
 *   Model class: GetAll([FromQuery] PaginationQuery q)
 *   ─────────────────────────────────────────────────────
 *   The model class approach is cleaner for ≥3 query params:
 *   - Validation attributes stay on the model rather than the method signature.
 *   - The same model reuses across multiple endpoints.
 *   - ModelState errors are reported per-property.
 *
 * BINDING PRECEDENCE for complex types (no explicit [From*]):
 *   With [ApiController]:
 *     Simple types (int, string, bool, Guid, …) → [FromRoute] if name matches a
 *       route segment, otherwise [FromQuery].
 *     Complex types (classes/structs) → [FromBody] (assuming JSON).
 *   Without [ApiController]:
 *     Complex types → [FromForm].
 *   Explicit [From*] attributes always override inference.
 */
public sealed class PaginationQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;                     // default → ?page absent → 1

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 20;                // default 20

    [StringLength(50, ErrorMessage = "SortBy cannot exceed 50 characters.")]
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; }               // ?sortDescending=true / false

    /*
     * SECTION 2b: COLLECTION BINDING
     *
     * ASP.NET Core can bind a List<T> from repeated query-string keys:
     *   GET /api/products?tags=electronics&tags=sale&tags=featured
     *   → Tags = [ "electronics", "sale", "featured" ]
     *
     * The binder also accepts square-bracket notation:
     *   ?tags[0]=electronics&tags[1]=sale
     *
     * Supported collection types:
     *   List<T>, IList<T>, ICollection<T>, T[], IEnumerable<T>
     *   Key-value collections: Dictionary<string,T>, IDictionary<string,T>
     *
     * PITFALL — comma-separated strings:
     *   ?tags=electronics,sale  is treated as ONE string "electronics,sale", not
     *   a two-element list. To split on commas you need a custom IModelBinder.
     *   See Binders/CommaSeparatedListBinder.cs for that implementation.
     *
     * CategoryIds demonstrates the same pattern for a strongly-typed int list:
     *   GET /api/products?categoryIds=1&categoryIds=5&categoryIds=12
     *   → CategoryIds = [ 1, 5, 12 ]
     */
    public List<string> Tags { get; set; } = new List<string>();

    public List<int> CategoryIds { get; set; } = new List<int>();

    /*
     * SECTION 2c: DEFAULT VALUES AND OPTIONAL PROPERTIES
     *
     * Properties with default values (Page=1, PageSize=20) are optional in the
     * query string. When the key is absent the default applies.
     *
     * Nullable properties (string? SortBy) are optional and null when absent.
     *
     * Non-nullable value types without defaults (e.g., decimal Price) would receive
     * their CLR default (0m) when the key is absent — add [Required] if absence
     * should fail validation.
     */
}
