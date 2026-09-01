/*
 * FILE ROLE: Provides the strongly-typed model used across the Demo views.
 *            Every property is chosen to demonstrate a specific Razor feature:
 *            string rendering, DateTime formatting, bool branching, collection
 *            iteration, and raw HTML injection.
 *
 * SECTIONS IN THIS FILE:
 *   1. ArticleViewModel — the typed model passed from DemoController to views
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ViewsRazorSyntax.Models;

/*
 * SECTION 1: ARTICLE VIEW MODEL
 * ─────────────────────────────────────────────────────────────────────────────
 * A ViewModel is a class designed specifically for a view — it carries exactly
 * the data that view needs, no more. This differs from a domain entity, which
 * maps to a database table and may include navigation properties or fields
 * that are irrelevant to rendering.
 *
 * ArticleViewModel properties and the Razor feature each one demonstrates:
 *
 *   Property        Type            Demonstrates
 *   ─────────────   ─────────────   ──────────────────────────────────────────
 *   Title           string          @Model.Title — simple inline expression
 *   Author          string          @Model.Author — inline expression in HTML
 *   PublishedOn     DateTime        @Model.PublishedOn.ToString("d") — method in expr
 *   IsPublished     bool            @if (Model.IsPublished) — conditional rendering
 *   Tags            List<string>    @foreach (var t in Model.Tags) — collection loop
 *   HtmlContent     string?         @Model.HtmlContent vs @Html.Raw encoding contrast
 *
 * NAMING CONVENTION:
 *   The "ViewModel" suffix distinguishes view-specific types from domain entities.
 *   Common alternatives: Vm, Dto, Model (the last is MVC convention in tutorials).
 *
 * [Required] on Title illustrates how data annotations integrate with
 * Html.ValidationMessageFor in Helpers.cshtml (see SECTION 9).
 * Full data annotation coverage is in → 07. Data Annotations & Validation.
 *
 * SEALED — this type is a leaf; no subclasses needed for a ViewModel.
 *
 * COVERED IN DETAIL LATER:
 *   Full ViewModel patterns → 05. ViewModels & Strongly Typed Views
 *   Data Annotations        → 07. Data Annotations & Validation
 */
public sealed class ArticleViewModel
{
    [Required]                                                     // used by Html.ValidationMessageFor
    public string Title { get; init; } = string.Empty;            // article heading

    public string Author { get; init; } = string.Empty;           // author display name
    public DateTime PublishedOn { get; init; }                     // publication date
    public bool IsPublished { get; init; }                         // drives @if in Control.cshtml
    public List<string> Tags { get; init; } = new();              // drives @foreach in Control.cshtml
    public string? HtmlContent { get; init; }                     // nullable; illustrates @Html.Raw
}
