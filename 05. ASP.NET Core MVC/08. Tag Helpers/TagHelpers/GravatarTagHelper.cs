/*
 * FILE ROLE: Demonstrates a self-closing custom tag helper with a hyphened element name,
 *            synchronous Process override, and MD5-based Gravatar URL construction.
 *            Covers: [HtmlTargetElement] with hyphens, TagMode.SelfClosing,
 *            MD5.HashData static helper, and RestoreOriginalTagName pattern.
 *
 * SECTIONS IN THIS FILE:
 *   1. GravatarTagHelper — custom <img-gravatar email="..." size="80" /> element
 */
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TagHelpers.TagHelpers;

/*
 * SECTION 1: CUSTOM TAG HELPER — <img-gravatar email="..." size="80" alt="..." />
 *
 * WHY HYPHENED ELEMENT NAMES?
 *   HTML treats any element without a hyphen as either a known element or an error.
 *   Custom elements MUST contain a hyphen — browsers then treat them as valid but unknown.
 *   The convention for deriving class names from hyphened element names:
 *     <img-gravatar>   → remove hyphens + PascalCase → ImgGravatarTagHelper
 *   We use [HtmlTargetElement("img-gravatar")] explicitly because:
 *     1. The convention is ambiguous with hyphens in multi-word names.
 *     2. It documents intent clearly in code.
 *
 * GRAVATAR URL FORMAT:
 *   https://www.gravatar.com/avatar/{hash}?s={size}&d=identicon
 *   where {hash} = MD5(email.trim().toLowerCase())
 *   The email must be lowercased and trimmed before hashing — Gravatar's specification.
 *
 * MD5.HashData() (System.Security.Cryptography):
 *   Static helper introduced in .NET 5 — no need to create/dispose an MD5 instance.
 *   Takes a ReadOnlySpan<byte> and returns byte[].
 *   Convert.ToHexString() converts byte[] to uppercase hex; .ToLowerInvariant() adjusts.
 *
 * TAGMODE VALUES:
 *   TagMode.StartTagAndEndTag  → <div></div>            (block elements)
 *   TagMode.SelfClosing        → <img />                (void element, XML style)
 *   TagMode.StartTagOnly       → <img>                  (void element, HTML5 style)
 *
 * RestoreOriginalTagName pattern:
 *   Some tag helpers only ADD attributes without changing the element type.
 *   Example — a tag helper targeting <button> that adds a spinner class:
 *     output.TagName = context.TagName;  → keeps <button>, just modifies attributes
 *   Here we replace <img-gravatar> → <img> so RestoreOriginalTagName is not used,
 *   but the context.TagName would be "img-gravatar" if you needed to inspect it.
 */
[HtmlTargetElement("img-gravatar")] // must be explicit — hyphen prevents convention inference
public sealed class GravatarTagHelper : TagHelper
{
    [HtmlAttributeName("email")]
    public string Email { get; set; } = string.Empty; // required for Gravatar hash

    [HtmlAttributeName("size")]
    public int Size { get; set; } = 80; // pixel size (width=height), Gravatar range: 1-2048

    [HtmlAttributeName("alt")]
    public string AltText { get; set; } = "Profile picture"; // <img alt="...">

    /*
     * --- Na. SYNCHRONOUS Process() ---
     *
     * Use Process() (not ProcessAsync) when child content is NOT read.
     * <img-gravatar> is a self-closing element with no inner HTML, so synchronous
     * Process is correct and more efficient than an unnecessary async state machine.
     */
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        string hash = ComputeMd5Hash(Email.Trim().ToLowerInvariant()); // Gravatar spec
        string url = $"https://www.gravatar.com/avatar/{hash}?s={Size}&d=identicon";

        output.TagName = "img"; // replace <img-gravatar> with <img>
        output.TagMode = TagMode.SelfClosing; // renders as <img ... />

        output.Attributes.SetAttribute("src", url); // computed Gravatar CDN URL
        output.Attributes.SetAttribute("alt", AltText);
        output.Attributes.SetAttribute("width", Size.ToString(CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute("height", Size.ToString(CultureInfo.InvariantCulture));
    }

    /*
     * --- Nb. MD5 HASH HELPER ---
     *
     * MD5.HashData() is the modern static API (no disposable instance).
     * Convert.ToHexString() returns uppercase hex (e.g. "A3B4...").
     * .ToLowerInvariant() converts to lowercase ("a3b4...") as Gravatar requires.
     * Encoding.UTF8.GetBytes() handles non-ASCII email characters correctly.
     */
    private static string ComputeMd5Hash(string input)
    {
        byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(input)); // static helper — no IDisposable
        return Convert.ToHexString(bytes).ToLowerInvariant(); // "deadbeef..." format
    }
}
