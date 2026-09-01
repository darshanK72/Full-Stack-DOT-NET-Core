/*
 * TOPIC: Tag Helpers in ASP.NET Core MVC
 *
 * WHY IT MATTERS:
 *   Tag Helpers are a server-side feature that lets C# participate in creating and
 *   rendering HTML elements in Razor views using familiar HTML-like attributes rather
 *   than inline C# method calls.  They replace the older HtmlHelper (Html.BeginForm(),
 *   Html.TextBoxFor(), Html.ActionLink()) with markup that reads like plain HTML,
 *   integrates with editor tooling, and is friendlier to front-end developers.
 *
 *   Tag Helpers vs Html Helpers — key differences:
 *
 *   Aspect              | Html Helper                       | Tag Helper
 *   --------------------|-----------------------------------|----------------------------------
 *   Syntax              | @Html.TextBoxFor(m => m.Name)     | <input asp-for="Name" />
 *   Readability         | C# expression in HTML             | Looks like HTML
 *   IDE support         | Razor intellisense                | Full HTML editor support
 *   Reusability         | Extension methods                 | Classes (testable, injectable)
 *   Target             | Method returns IHtmlContent        | Transform tag output in-place
 *   Anti-forgery        | @Html.AntiForgeryToken() explicit | Automatic on <form asp-action>
 *
 * WHAT YOU WILL LEARN:
 *   1. ContactFormViewModel (model for form demos)  → Models/ContactFormViewModel.cs
 *   2. AlertTagHelper — custom <alert> element      → TagHelpers/AlertTagHelper.cs
 *   3. GravatarTagHelper — custom <img-gravatar>    → TagHelpers/GravatarTagHelper.cs
 *   4. DemoController — wires views                 → Controllers/DemoController.cs
 *   5. @addTagHelper / @removeTagHelper directives  → Views/_ViewImports.cshtml
 *   6. Form / Input / Label / Select / Validation   → Views/Demo/Forms.cshtml
 *   7. Anchor tag helper                            → Views/Demo/Navigation.cshtml
 *   8. Asset / Environment / Cache tag helpers      → Views/Demo/Assets.cshtml
 *   9. Custom tag helpers in action                 → Views/Demo/Custom.cshtml
 *
 * CHAPTER MAP:
 *   1. ViewModel                → Models/ContactFormViewModel.cs
 *   2. Custom TagHelper (alert) → TagHelpers/AlertTagHelper.cs
 *   3. Custom TagHelper (image) → TagHelpers/GravatarTagHelper.cs
 *   4. Controller               → Controllers/DemoController.cs
 *   5. @addTagHelper directives → Views/_ViewImports.cshtml
 *   6. Form tag helpers         → Views/Demo/Forms.cshtml
 *   7. Anchor tag helper        → Views/Demo/Navigation.cshtml
 *   8. Asset / cache helpers    → Views/Demo/Assets.cshtml
 *   9. Custom in action         → Views/Demo/Custom.cshtml
 */
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TagHelpers;

/*
 * SECTION 1: MVC STARTUP & DEPENDENCY INJECTION
 *
 * AddControllersWithViews() registers the full MVC pipeline:
 *   - Controller routing and model binding
 *   - Razor view engine (compiles .cshtml to C# at build or runtime)
 *   - Tag helper infrastructure (ITagHelperActivator, ITagHelperFactory)
 *   - DataAnnotations-based server-side model validation
 *   - Anti-forgery token services (required by <form asp-action="...">)
 *   - IFileVersionProvider — needed by asp-append-version on link/script/img
 *
 * Tag helper classes are NOT registered in the DI container here.
 * They are discovered from assemblies named in @addTagHelper directives
 * inside _ViewImports.cshtml at Razor compilation time.
 */
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args); // host + config + DI
        builder.Services.AddControllersWithViews(); // MVC + Razor + tag helper discovery

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler("/Home/Error"); // generic error page outside dev

        app.UseStaticFiles(); // serve wwwroot/ — required by asp-append-version at runtime
        app.UseRouting();
        app.UseAuthorization();

        /*
         * SECTION 2: DEFAULT ROUTE
         *
         * Convention route pattern: {controller}/{action}/{id?}
         * DemoController.Forms() is the entry point for this tutorial.
         * Tag helper links like <a asp-action="Navigation"> generate URLs
         * using this route template — they never hard-code path strings.
         */
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Demo}/{action=Forms}/{id?}"); // Demo/Forms is home page

        app.Run();
    }
}

/*
 * QUICK REFERENCE — Tag Helpers at a Glance
 *
 * REGISTRATION (_ViewImports.cshtml)
 *   @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers   → all built-in tag helpers
 *   @addTagHelper *, TagHelpers                            → custom tag helpers in this assembly
 *   @removeTagHelper Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper,
 *       Microsoft.AspNetCore.Mvc.TagHelpers                → opt-out one specific helper
 *   @tagHelperPrefix th:                                   → require th: prefix on every helper
 *
 * FORM TAG HELPERS (Views/Demo/Forms.cshtml)
 *   <form asp-action="Submit" asp-controller="Demo" asp-route-returnUrl="/" method="post">
 *   <label asp-for="Name"></label>         → <label for="Name">Full Name</label>
 *   <input asp-for="Name" />               → <input type="text" id="Name" name="Name"
 *                                              value="" data-val="true" ...>
 *   <input asp-for="Email" />              → type="email" (from [EmailAddress])
 *   <input asp-for="BirthDate"             → type="date", formatted value
 *          asp-format="{0:yyyy-MM-dd}" />
 *   <input asp-for="AcceptTerms" />        → type="checkbox"
 *   <select asp-for="CategoryId"
 *           asp-items="Model.Categories">  → <option> list from SelectList
 *   <textarea asp-for="Message"></textarea>
 *   <span asp-validation-for="Name"></span>
 *   <div asp-validation-summary="All"></div>
 *
 * ANCHOR TAG HELPER (Views/Demo/Navigation.cshtml)
 *   <a asp-action="Details" asp-controller="Product"
 *      asp-route-id="5" asp-area="Admin"
 *      asp-fragment="section1">Link</a>
 *
 * ASSET TAG HELPERS (Views/Demo/Assets.cshtml)
 *   <link rel="stylesheet" asp-append-version="true" href="~/css/site.css" />
 *   <script asp-append-version="true" src="~/js/app.js"></script>
 *   <img asp-append-version="true" src="~/img/logo.png" alt="Logo" />
 *   <link asp-src-include="~/css/**\/*.css" rel="stylesheet" />
 *   <script asp-src-include="~/js/**\/*.js" asp-src-exclude="~/js/vendor.js"></script>
 *   <environment include="Development">...</environment>
 *   <environment exclude="Development">...</environment>
 *   <partial name="_ContactInfo" />
 *   <partial name="_ModelPartial" model="@someObject" />
 *   <cache expires-after="@TimeSpan.FromMinutes(5)" vary-by-user="true">...</cache>
 *
 * CUSTOM TAG HELPER SKELETON
 *   [HtmlTargetElement("my-widget")]
 *   public class MyWidgetTagHelper : TagHelper
 *   {
 *       [HtmlAttributeName("title")]
 *       public string Title { get; set; } = string.Empty;
 *
 *       [HtmlAttributeNotBound]          // internal — not set from HTML attributes
 *       public string Internal { get; set; } = string.Empty;
 *
 *       public override void Process(TagHelperContext ctx, TagHelperOutput out)
 *       {
 *           out.TagName = "div";
 *           out.TagMode = TagMode.StartTagAndEndTag;
 *           out.Attributes.SetAttribute("class", "widget");
 *           out.Content.SetContent(Title);
 *           // out.TagName = ctx.TagName;  → RestoreOriginalTagName: keep <my-widget>
 *           // out.SuppressOutput();       → emit nothing (hide element)
 *       }
 *   }
 */
