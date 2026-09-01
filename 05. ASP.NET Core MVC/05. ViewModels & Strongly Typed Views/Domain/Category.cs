/*
 * FILE ROLE: Domain entity for a product category. Shows a related entity with
 *            a collection navigation property — the kind of shape that must be
 *            carefully projected before passing anything to a view.
 * SECTIONS IN THIS FILE:
 *   6. Category domain entity
 */

using System.Collections.Generic;

namespace ViewModels.Domain;

/*
 * SECTION 6: CATEGORY DOMAIN ENTITY
 * ─────────────────────────────────────────────────────────────────────────────
 * Category is a related entity that Products reference via CategoryId.
 * It has a collection navigation property (Products) that is dangerous to pass
 * to a view because:
 *
 *   - If Products is not pre-loaded, accessing it triggers lazy-loading — a
 *     database query per row in a list view (the classic N+1 problem).
 *   - If Products IS loaded, the view receives far more data than it needs, and
 *     the circular reference (Category.Products[0].Category...) can cause
 *     serialization errors or accidental display of internal data.
 *
 * The ViewModel for a category display page carries only what is needed:
 *   CategorySummaryViewModel.Name          → string, display-ready
 *   CategorySummaryViewModel.ProductCount  → int, pre-computed in the controller
 *
 * The raw Products collection never appears in a ViewModel.
 *
 * NAMING NOTE — domain entities vs ViewModels:
 *   Domain entity:    Category           (lives in Domain/)
 *   Nested VM:        CategorySummaryViewModel  (lives in ViewModels/)
 *   Form VM item:     CategoryOption     (lives in ViewModels/ — for dropdowns)
 *
 * Each has a different shape chosen for its specific purpose.
 */
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // every category has a name

    public string? Description { get; set; }            // nullable: description is optional

    // Collection nav property — never pass this to a view directly
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
