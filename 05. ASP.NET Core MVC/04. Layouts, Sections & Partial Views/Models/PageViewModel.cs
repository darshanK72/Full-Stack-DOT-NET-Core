/*
 * FILE ROLE: Defines PageViewModel — the strongly-typed view model passed
 *            from PageController.Index() to Views/Page/Index.cshtml.
 * SECTIONS IN THIS FILE:
 *   1. PageViewModel — page-level view model
 */

using System;

namespace LayoutsSectionsPartials.Models;

/*
 * SECTION 1: PageViewModel
 *
 * Carries all data that Index.cshtml needs.  Strongly-typed view models
 * are preferred over ViewBag/ViewData for page-level data because:
 *   - IntelliSense works in the view (refactor-safe property names)
 *   - Null-safety is enforced at compile time with Nullable enabled
 *   - Model binding and validation hooks in for form scenarios
 *
 * NavItems is included here to demonstrate passing an array to a partial
 * view directly from the page model (rather than always from ViewData).
 */
public sealed class PageViewModel
{
    public string   Title    { get; init; } = string.Empty;                  // <title> / <h1>
    public string   Body     { get; init; } = string.Empty;                  // main content paragraph
    public NavItem[] NavItems { get; init; } = Array.Empty<NavItem>();        // passed to partial/component
}
