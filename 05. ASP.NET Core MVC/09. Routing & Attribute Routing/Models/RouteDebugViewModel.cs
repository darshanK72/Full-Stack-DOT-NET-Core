/*
 * FILE ROLE: Captures the currently matched route's data so Home/Index.cshtml
 *            can display it — making routing visible while the reader navigates
 *            the app and observes which template was selected.
 * SECTIONS IN THIS FILE:
 *   20. Route Debug View Model
 */

using System.Collections.Generic;

namespace RoutingAttributeRouting.Models;

/*
 * SECTION 20: ROUTE DEBUG VIEW MODEL
 * ─────────────────────────────────────────────────────────────────────────────
 * This ViewModel exists to make routing transparent.  When HomeController.Index
 * returns View(model), the view renders the matched controller, action, area,
 * the template pattern that was selected, and all extracted route values.
 *
 * RouteData (the source):
 *   Every controller action has access to RouteData via the base Controller.RouteData
 *   property.  It is populated by the routing middleware BEFORE the action executes.
 *
 *   RouteData.Values   — IDictionary<string, object?> of segments extracted from the URL
 *                        Keys: "controller", "action", "id", "area", and any custom segment
 *   RouteData.Routers  — the chain of IRouter objects that matched (legacy; rarely used)
 *   RouteData.DataTokens — extra non-segment metadata attached to the route (e.g. "area")
 *
 * Controller → ControllerContext → ActionDescriptor:
 *   ControllerContext.ActionDescriptor.AttributeRouteInfo?.Template
 *   returns the attribute route template string when attribute routing was used,
 *   or null when conventional routing matched.
 *
 * This class uses init-only setters (C# 9) so instances are set up once in
 * the controller action and treated as read-only in the view.
 *
 * init vs set:
 *   set   — property can be assigned any time after construction
 *   init  — property can only be assigned in the constructor or object initializer;
 *            thereafter it is read-only (behaves like readonly for the property value)
 */
public sealed class RouteDebugViewModel
{
    // Name of the controller class (without "Controller" suffix) extracted from route data
    public string Controller { get; init; } = string.Empty;

    // Name of the matched action method
    public string Action { get; init; } = string.Empty;

    // Area name — null when not inside an Area
    public string? Area { get; init; }

    // The route template that produced this match, e.g. "{controller=Home}/{action=Index}/{id?}"
    // or an attribute route template like "[controller]/{id:int}"
    public string MatchedTemplate { get; init; } = string.Empty;

    // All key/value pairs extracted from the URL by the routing middleware
    // e.g. { "controller": "Home", "action": "Index", "id": null }
    public Dictionary<string, string?> RouteValues { get; init; } = new Dictionary<string, string?>();
}
