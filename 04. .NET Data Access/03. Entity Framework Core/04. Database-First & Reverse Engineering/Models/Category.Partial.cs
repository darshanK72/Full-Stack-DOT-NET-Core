namespace DatabaseFirstAndReverseEngineering.Models;

/*
 * FILE ROLE:
 *   Developer-owned partial class - survives re-scaffold because scaffold only touches Category.cs.
 *
 * SECTIONS IN THIS FILE:
 *   7. Partial classes - safe customization
 */

/*
 * SECTION 7: PARTIAL CLASSES - SAFE CUSTOMIZATION
 *
 * Scaffold generates partial types so you can split files:
 *
 *   Category.cs         <- regenerated on every scaffold (do not edit)
 *   Category.Partial.cs <- your computed helpers, validation, NotMapped members
 *
 * Rules:
 *   - Same namespace and class name as generated file
 *   - Never edit generated *.cs after initial scaffold; merge conflicts are painful
 *   - For DbContext tweaks, use OnModelCreatingPartial in a separate partial DbContext file
 *     (see Data/InventoryDbContext.Partial.cs pattern in comments there)
 * -------------------------------------------------------------------------
 */
public partial class Category
{
    public string DisplayLabel =>
        string.IsNullOrWhiteSpace(Description) ? Name : $"{Name} - {Description}";

    public int ProductCount => Products?.Count ?? 0;
}
