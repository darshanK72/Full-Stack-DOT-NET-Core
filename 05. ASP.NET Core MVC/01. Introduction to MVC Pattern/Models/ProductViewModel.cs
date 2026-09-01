/*
 * FILE ROLE: Defines the ProductViewModel — a ViewModel used to carry display
 *   data from the controller to the View. Demonstrates the "M" (Model) layer
 *   of ASP.NET Core MVC and the ViewModel pattern for type-safe, focused views.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 13: ViewModel — purpose, shape, and the Model layer in MVC
 */

namespace IntroductionToMvc.Models;

/*
 * SECTION 13: VIEWMODEL — THE MODEL LAYER IN MVC
 * ─────────────────────────────────────────────────────────────────────────────
 * A ViewModel is a class whose shape is designed for ONE specific view.
 * It is NOT the same as a domain entity (database row type).
 *
 * WHY USE A VIEWMODEL INSTEAD OF PASSING THE ENTITY DIRECTLY?
 *
 *   Problem with passing database entities to views:
 *     OVER-POSTING:   A form POST can bind fields the user should not control
 *                     (e.g. setting IsAdmin = true on a User entity).
 *     OVER-FETCHING:  The view sees navigation properties, internal flags,
 *                     and audit columns the template never renders.
 *     TIGHT COUPLING: Views depend on ORM-generated entity shapes; changing
 *                     the database schema breaks templates.
 *
 *   ViewModel solves this by:
 *     • Exposing only the properties the view needs — nothing more.
 *     • Carrying computed / formatted values (e.g. "PriceFormatted": "$29.99").
 *     • Combining data from multiple entities/services into one flat object.
 *     • Adding display-only properties (IsAvailable, FullName) without
 *       polluting the domain model.
 *
 * NAMING CONVENTION (pick one style and be consistent):
 *   {Feature}ViewModel   ProductViewModel, CheckoutViewModel, UserProfileViewModel
 *   {Feature}Model       ProductModel (also common, especially for Razor Pages)
 *
 * VIEWMODEL vs DTO vs INPUT MODEL:
 *   ViewModel     Data flowing FROM controller TO view (display purpose)
 *   DTO           Data crossing a service or API boundary (serialized / returned)
 *   Input model   Data flowing FROM a form/POST binding TO controller
 *                 (carries [Required], [MaxLength] etc. — bind what the form submits)
 *
 *   In simple CRUD pages these often collapse into one class.
 *   As complexity grows, separating them improves security and maintainability.
 *
 * @model DIRECTIVE IN RAZOR:
 *   The view declares its type with:
 *     @model IntroductionToMvc.Models.ProductViewModel
 *   This generates a strongly typed Model property in the Razor page class.
 *   @Model.Name compiles — the IDE offers IntelliSense; typos are caught early.
 *   Without @model, Model is dynamic — runtime errors instead of compile errors.
 *
 * DATA ANNOTATIONS (PREVIEW):
 *   Properties can carry display and validation attributes:
 *     [Display(Name = "Product Name")]    label text for Html helpers / Tag Helpers
 *     [DisplayFormat(DataFormatString = "{0:C}")]   currency format for Price
 *     [Required], [Range(0.01, 9999.99)]  validation (active on POST/input models)
 *   COVERED IN DETAIL LATER → 07. Data Annotations & Validation
 *
 * NULL SAFETY:
 *   Reference-type properties use = string.Empty as default values.
 *   This matches <Nullable>enable</Nullable> in the project — the view can
 *   render @Model.Name without a null-check guard or nullable warning.
 *   Always initialise ViewModel strings; never leave them as null? in display code.
 */
public class ProductViewModel
{
    public int Id { get; set; }                             // unique product identifier
    public string Name { get; set; } = string.Empty;       // product display name
    public decimal Price { get; set; }                      // price; rendered as currency in the view
    public string Category { get; set; } = string.Empty;   // category label for grouping/filtering
    public bool IsAvailable { get; set; }                   // availability flag for conditional markup
}
