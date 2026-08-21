namespace LoadingRelatedData.Utils;

/*
 * FILE ROLE:
 *   Introduces lazy loading concepts without enabling proxy packages in this chapter.
 *
 * SECTIONS IN THIS FILE:
 *   9. Lazy loading - preview only
 */

/*
 * SECTION 9: LAZY LOADING - PREVIEW
 *
 * By default EF Core does NOT lazy-load navigations. Accessing order.Customer after
 * a plain Orders query returns null unless you Include, Load, or enable lazy loading.
 *
 * To enable lazy loading (not used in this project):
 *
 *   1. Install Microsoft.EntityFrameworkCore.Proxies
 *   2. optionsBuilder.UseLazyLoadingProxies();
 *   3. Navigation properties must be virtual (and classes non-sealed for proxy subclasses)
 *
 * Pitfalls:
 *   - N+1 queries when iterating entities and touching navigations in loops
 *   - DbContext must stay alive while navigations are accessed (disposed context throws)
 *   - Harder to reason about SQL than explicit Include
 *
 * COVERED IN DETAIL LATER -> dedicated lazy-loading / performance topic when added to curriculum
 * -------------------------------------------------------------------------
 */
public static class LazyLoadingPreview
{
    public const string PackageName = "Microsoft.EntityFrameworkCore.Proxies";

    public static string Summary =>
        "UseLazyLoadingProxies() + virtual navigations; prefer explicit Include in web/API code.";
}
