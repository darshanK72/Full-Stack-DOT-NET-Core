/*
 * FILE ROLE: ViewModel for the Admin area's Dashboard/Index view — demonstrates
 *   how models can live in the root Models/ folder and be shared across areas,
 *   and what a typical area-specific ViewModel looks like.
 *
 * SECTIONS IN THIS FILE:
 *   9. Area ViewModel — shared root Models/ folder, ViewModel conventions
 */

namespace Areas.Models;

/*
 * SECTION 9: AREA VIEW MODELS — SHARED ROOT Models/ FOLDER
 * ─────────────────────────────────────────────────────────────────────────────
 * View Models are plain C# classes that carry exactly the data a specific view
 * needs. They are distinct from domain entities (EF Core models) and DTOs.
 *
 * SHARED vs. AREA-SPECIFIC MODELS:
 *
 *   Approach                 Folder                     When to use
 *   ──────────────────────   ────────────────────────   ────────────────────────────────
 *   Shared root Models/      Models/{Name}.cs           Models used across multiple areas
 *                                                        or by both area and non-area code
 *   Area-isolated Models/    Areas/{area}/Models/       Models that belong to ONE area;
 *                                                        used in large projects with teams
 *
 *   This chapter uses the shared root Models/ approach. AdminDashboardViewModel
 *   is referenced from Areas/Admin/Controllers/DashboardController.cs using:
 *     using Areas.Models;
 *   and from Areas/Admin/Views/Dashboard/Index.cshtml using:
 *     @model Areas.Models.AdminDashboardViewModel
 *
 * VIEW MODEL NAMING CONVENTION:
 *   {Area}{Controller}ViewModel or {Controller}{Action}ViewModel
 *   Examples: AdminDashboardViewModel, CustomerProfileViewModel
 *   Avoids ambiguity when multiple areas have same-named views.
 *
 * WHY NOT PASS INDIVIDUAL PIECES VIA ViewData/ViewBag?
 *   ViewData["TotalUsers"] = 42;    ← weakly typed, no intellisense, cast required
 *   @((int)ViewData["TotalUsers"])   ← error-prone in views
 *
 *   ViewModels are strongly typed, refactor-safe, and enable model validation
 *   when used with [Required] and data annotation attributes on form pages.
 *
 * NULLABLE CONTEXT:
 *   WelcomeMessage and AreaName are non-nullable strings initialised with
 *   default values (string.Empty) to satisfy the C# nullable reference types
 *   compiler. The controller sets them before passing the model to View().
 */
public sealed class AdminDashboardViewModel
{
    public string WelcomeMessage { get; set; } = string.Empty; // header text for the dashboard
    public int    TotalUsers     { get; set; }                  // total registered users
    public int    TotalOrders    { get; set; }                  // total orders placed
    public string AreaName       { get; set; } = string.Empty; // which area populated this model
}
