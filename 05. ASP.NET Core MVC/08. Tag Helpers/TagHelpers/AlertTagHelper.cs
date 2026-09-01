/*
 * FILE ROLE: Teaches how to write a custom Tag Helper by implementing a reusable
 *            <alert> element that renders a Bootstrap-styled alert <div>.
 *            Covers: TagHelper base class, [HtmlTargetElement], TagHelperContext,
 *            TagHelperOutput, [HtmlAttributeName], [HtmlAttributeNotBound],
 *            ProcessAsync with child content, and RestoreOriginalTagName.
 *
 * SECTIONS IN THIS FILE:
 *   1. AlertTagHelper — full custom tag helper walkthrough
 */
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TagHelpers.TagHelpers;

/*
 * SECTION 1: CUSTOM TAG HELPER — <alert type="danger" dismissible>...</alert>
 *
 * HOW TAG HELPERS ARE DISCOVERED:
 *   1. _ViewImports.cshtml declares: @addTagHelper *, TagHelpers
 *   2. At Razor compile time the engine scans the TagHelpers assembly for any public
 *      class that inherits from TagHelper (or ITagHelper).
 *   3. When a matching element is found in a view, the class is instantiated per-request
 *      by ITagHelperFactory (which supports constructor DI).
 *   4. Public settable properties are bound from HTML attributes before Process is called.
 *
 * [HtmlTargetElement("alert")]
 *   Explicitly binds this class to the <alert> element.
 *   Without the attribute, the class name drives matching:
 *     AlertTagHelper → <alert>     (suffix "TagHelper" stripped, PascalCase → kebab-case)
 *     FormTagHelper  → <form>      (conflicts with native <form>, so built-in uses the attribute)
 *   Useful overloads:
 *     [HtmlTargetElement("input", Attributes = "asp-my-attr")]  → target existing element + attribute
 *     [HtmlTargetElement(Attributes = "highlight")]             → match ANY element with attribute
 *
 * TagHelperContext  (input bag):
 *   .AllAttributes   — all HTML attributes as declared in markup (read-only)
 *   .UniqueId        — stable per-request ID for this element (useful for aria-describedby)
 *   .Items           — Dictionary<object,object> for passing data to nested tag helpers
 *   .TagName         — original element name before any ProcessAsync changes it
 *
 * TagHelperOutput  (output bag):
 *   .TagName         — set to change the emitted element name; null = suppress wrapper
 *   .TagMode         — StartTagAndEndTag / SelfClosing / StartTagOnly
 *   .Attributes      — mutable attribute collection
 *   .PreElement      — HTML written before the opening tag
 *   .PreContent      — HTML written after opening tag, before inner content
 *   .Content         — inner content (replace inner HTML)
 *   .PostContent     — HTML written after inner content, before closing tag
 *   .PostElement     — HTML written after the closing tag
 *   .SuppressOutput()— emits nothing (the element and all its content disappear)
 *
 * RestoreOriginalTagName:
 *   If a tag helper changes output.TagName (e.g. "div") and you later want to UNDO that:
 *     output.TagName = context.TagName;   → re-emits <alert> in the HTML output
 *   Useful when a tag helper should only ADD attributes without changing the element name.
 */
[HtmlTargetElement("alert")] // <alert type="..." dismissible>...</alert>
public sealed class AlertTagHelper : TagHelper
{
    /*
     * --- Na. ATTRIBUTE BINDING ---
     *
     * Convention: public property AlertType maps to HTML attribute "alert-type" by default
     * (PascalCase → kebab-case).  [HtmlAttributeName("type")] overrides that convention
     * so authors write <alert type="danger"> instead of <alert alert-type="danger">.
     *
     * Framework binding order:
     *   1. Find property whose kebab-case name matches the attribute, OR
     *   2. Find property decorated with [HtmlAttributeName("exact-name")]
     *   3. Set the property value (string is set directly; complex types use TypeConverter)
     *
     * For bool properties, the attribute's mere PRESENCE sets them to true:
     *   <alert dismissible>  →  Dismissible = true
     *   <alert>              →  Dismissible = false (default)
     */
    [HtmlAttributeName("type")]
    public string AlertType { get; set; } = "info"; // Bootstrap: info | success | warning | danger

    public bool Dismissible { get; set; } // presence of "dismissible" attribute → true

    /*
     * --- Nb. INTERNAL STATE ([HtmlAttributeNotBound]) ---
     *
     * [HtmlAttributeNotBound] prevents the framework from treating this property as
     * an HTML attribute source.  Common uses:
     *   - Services injected via constructor (IUrlHelper, IHtmlGenerator, IViewContextAware)
     *   - Internal tracking state set by parent tag helpers via context.Items
     *   - Computed values derived from other properties
     *
     * Without [HtmlAttributeNotBound], the framework would look for an HTML attribute
     * named "internal-state" in the markup and attempt to bind it.
     */
    [HtmlAttributeNotBound]
    public string InternalState { get; set; } = string.Empty; // not settable from HTML

    /*
     * --- Nc. PROCESS vs PROCESS ASYNC ---
     *
     * Override Process()      when child content is NOT needed (synchronous, no await).
     * Override ProcessAsync() when child content IS needed — GetChildContentAsync() is async.
     *
     * GetChildContentAsync():
     *   Returns the TagHelperContent between <alert>...</alert>.
     *   Calling it before output.Content is set lets you inspect or wrap the inner markup.
     *   Pass useCachedResult: false to force a re-render of child tag helpers.
     *
     * Output order:
     *   PreElement | <TagName Attributes> | PreContent | Content | PostContent | </TagName> | PostElement
     */
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div"; // replace <alert> with <div>
        output.TagMode = TagMode.StartTagAndEndTag; // <div>...</div>

        // Build Bootstrap CSS class string
        string css = $"alert alert-{AlertType}"; // e.g. "alert alert-danger"
        if (Dismissible)
            css += " alert-dismissible fade show"; // Bootstrap dismiss classes

        output.Attributes.SetAttribute("class", css);
        output.Attributes.SetAttribute("role", "alert"); // ARIA landmark role

        // Read child content written between <alert>...</alert>
        TagHelperContent child = await output.GetChildContentAsync(); // async — awaits child helpers
        output.Content.SetHtmlContent(child.GetContent()); // emit inner HTML verbatim

        // Inject dismiss button AFTER inner content (PostContent), inside the closing </div>
        if (Dismissible)
        {
            output.PostContent.SetHtmlContent(
                "<button type=\"button\" class=\"btn-close\" " +
                "data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>");
        }

        // EXAMPLE: to restore <alert> instead of <div>:
        //   output.TagName = context.TagName;   ← RestoreOriginalTagName
        // EXAMPLE: to suppress output entirely:
        //   output.SuppressOutput();
    }
}
