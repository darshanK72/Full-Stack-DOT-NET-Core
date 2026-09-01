/*
 * FILE ROLE: Defines the Product view model used by HomeController, ProductsController,
 *            and ApiController to demonstrate typed action return values, model binding,
 *            and strongly typed views.
 *
 * SECTIONS IN THIS FILE:
 *   16. Product view model
 */

namespace ControllersActions.Models;

/*
 * SECTION 16: PRODUCT VIEW MODEL
 * ─────────────────────────────────────────────────────────────────────────────
 * A view model is a plain C# class shaped to match what a specific view or API
 * response needs. It is NOT necessarily a database entity — it can combine
 * fields from multiple sources or omit sensitive data.
 *
 * Nullable enable rules:
 *   • string properties MUST have a default (= string.Empty) or be nullable
 *     (string?) to suppress CS8618: Non-nullable property must be initialized.
 *   • Value types (int, decimal) are non-nullable by definition — no default needed.
 *
 * This model is used as:
 *   • A ViewResult model → returned by ProductsController.Index() and passed to
 *     Views/Products/Index.cshtml via return View(products)
 *   • A request body → received by ApiController.Create([FromBody] Product product)
 *   • A response body → serialised to JSON by ApiController.GetById(int id)
 */
public sealed class Product
{
    public int Id { get; set; }                              // primary key / URL identifier
    public string Name { get; set; } = string.Empty;        // display name; default prevents CS8618
    public decimal Price { get; set; }                       // monetary value; decimal avoids float rounding
    public string Category { get; set; } = string.Empty;    // grouping label
}
