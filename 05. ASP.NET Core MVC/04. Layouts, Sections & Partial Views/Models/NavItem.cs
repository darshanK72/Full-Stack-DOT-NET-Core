/*
 * FILE ROLE: Defines NavItem — the model for a single navigation link.
 *            Used by NavigationViewComponent and _NavigationPartial.cshtml.
 * SECTIONS IN THIS FILE:
 *   1. NavItem — lightweight record for a navigation link
 */

namespace LayoutsSectionsPartials.Models;

/*
 * SECTION 1: NavItem MODEL
 *
 * A lightweight, immutable record representing one navigation link.
 *
 * This type is shared between two rendering mechanisms to show the same
 * model flowing through both:
 *   - _NavigationPartial.cshtml  (@model NavItem[])
 *   - NavigationViewComponent    (passes NavItem[] to its view)
 *
 * Using init-only properties ensures the model cannot be mutated after
 * construction — appropriate for view models that are read-only.
 */
public sealed class NavItem
{
    public string Label    { get; init; } = string.Empty;   // display text shown in the nav
    public string Href     { get; init; } = "#";            // destination URL or anchor
    public bool   IsActive { get; init; }                   // highlights the current page
}
