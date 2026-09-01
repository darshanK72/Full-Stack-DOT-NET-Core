/*
 * FILE ROLE: Demonstrates how a controller feeds data to each Demo view,
 *            illustrating ViewBag, ViewData, and strongly-typed @model passing.
 *
 * SECTIONS IN THIS FILE:
 *   1. Controller basics — inheritance, IActionResult, and View() overloads
 *   2. Index action     — strongly-typed @model + ViewBag + ViewData combo
 *   3. Control action   — model with a collection to drive @foreach / @if / @switch
 *   4. Helpers action   — model with raw HTML to demonstrate encoding contrast
 *   5. Directives action — no model; illustrates ViewBag-only data passing
 */

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using ViewsRazorSyntax.Models;

namespace ViewsRazorSyntax.Controllers;

/*
 * SECTION 1: CONTROLLER BASICS
 * ─────────────────────────────────────────────────────────────────────────────
 * A controller is a public class that:
 *   1. Inherits Controller (MVC with views) or ControllerBase (API, no views).
 *   2. Has public methods called "actions" that handle HTTP requests.
 *   3. Returns IActionResult — a contract for any HTTP response type.
 *
 * VIEW RESULT OVERLOADS:
 *   View()                 → Views/{Controller}/{Action}.cshtml
 *   View("ViewName")       → Views/{Controller}/ViewName.cshtml  (explicit name)
 *   View(model)            → View() + passes model as ViewResult.Model
 *   View("Name", model)    → explicit name + typed model
 *
 * PASSING DATA TO VIEWS — three mechanisms:
 *
 *   Mechanism   Type        Strongly typed   Notes
 *   ──────────  ──────────  ───────────────  ──────────────────────────────────
 *   @model      POCO class  Yes              Preferred — use for structured data
 *   ViewData    dict        No               string keys / object values; cast needed
 *   ViewBag     dynamic     No               Syntactic sugar over ViewData; no cast
 *
 *   ViewBag.X and ViewData["X"] share the SAME underlying storage — setting one
 *   also sets the other. Both are suitable only for small incidental values
 *   (page title, breadcrumbs) where a ViewModel property would be overkill.
 *
 *   TempData persists across ONE redirect and is backed by a cookie or session.
 *   COVERED IN DETAIL LATER → 12. TempData, ViewData & ViewBag
 */
public sealed class DemoController : Controller
{
    /*
     * SECTION 2: INDEX ACTION — strongly-typed model + ViewBag + ViewData
     * ─────────────────────────────────────────────────────────────────────────
     * Three data-passing techniques are used together so Views/Demo/Index.cshtml
     * can compare them side by side.
     *
     * ArticleViewModel → passed as the typed model; accessed as @Model.* in the view
     * ViewBag.PageTitle → dynamic; accessed as @ViewBag.PageTitle (no cast)
     * ViewData["SubTitle"] → dictionary; accessed as @ViewData["SubTitle"] (returns object)
     */
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Views & Razor Syntax";                    // dynamic bag
        ViewData["SubTitle"] = "Section 7: Expressions & Code Blocks"; // dict entry

        var article = new ArticleViewModel
        {
            Title       = "Getting Started with Razor",
            Author      = "Darshan Khairnar",
            PublishedOn = new DateTime(2024, 3, 15),
            IsPublished = true,
            Tags        = new List<string> { "ASP.NET Core", "Razor", "MVC", "Views" },
            HtmlContent = "<strong>Razor</strong> blends C# and HTML in a single <em>.cshtml</em> file."
        };

        return View(article); // View(model) — passes ArticleViewModel as ViewResult.Model
    }

    /*
     * SECTION 3: CONTROL ACTION — collection-bearing model for control-flow views
     * ─────────────────────────────────────────────────────────────────────────────
     * Tags drives @foreach in Control.cshtml.
     * IsPublished drives the @if / @else branch.
     * Title is used in an @switch to demonstrate multi-way branching.
     */
    public IActionResult Control()
    {
        ViewBag.PageTitle = "Control Flow in Razor";

        var article = new ArticleViewModel
        {
            Title       = "Razor Control Flow",
            Author      = "Darshan Khairnar",
            PublishedOn = new DateTime(2024, 6, 1),
            IsPublished = true,
            Tags        = new List<string> { "@if / @else", "@switch", "@foreach", "@for", "@while" }
        };

        return View(article);
    }

    /*
     * SECTION 4: HELPERS ACTION — raw HTML string for encoding demonstration
     * ─────────────────────────────────────────────────────────────────────────
     * HtmlContent holds a trusted HTML string. Helpers.cshtml demonstrates:
     *   • Automatic HTML encoding: @Model.HtmlContent outputs escaped text
     *   • @Html.Raw: outputs the same string as literal HTML (skip encoding)
     *
     * ENCODING RULE:
     *   Always use automatic encoding (@variable) for any user-provided data.
     *   Use @Html.Raw ONLY for strings that are already trusted and sanitised.
     *   User-supplied HTML passed through @Html.Raw is a classic XSS vector.
     *
     * IsPublished = false here lets Helpers.cshtml show the "Draft" branch.
     */
    public IActionResult Helpers()
    {
        ViewBag.PageTitle = "@Html Helpers & HTML Encoding";

        var article = new ArticleViewModel
        {
            Title       = "HtmlHelper Methods",
            Author      = "Darshan Khairnar",
            PublishedOn = DateTime.Today,
            IsPublished = false,
            Tags        = new List<string> { "DisplayFor", "EditorFor", "ActionLink", "BeginForm" },
            HtmlContent = "<strong>Bold</strong> and <em>italic</em> via @Html.Raw."
        };

        return View(article);
    }

    /*
     * SECTION 5: DIRECTIVES ACTION — ViewBag-only data passing
     * ─────────────────────────────────────────────────────────────────────────
     * Directives.cshtml is a standalone reference view that demonstrates all
     * Razor directives. No typed model is needed — the view shows directives
     * using @functions (local methods) and @inject (DI in a view).
     *
     * Calling View() with no argument resolves to:
     *   Views/Demo/Directives.cshtml  (controller name + action name convention)
     */
    public IActionResult Directives()
    {
        ViewBag.PageTitle = "Razor Directives Reference";
        return View(); // no model; directives are self-contained in the .cshtml
    }
}
