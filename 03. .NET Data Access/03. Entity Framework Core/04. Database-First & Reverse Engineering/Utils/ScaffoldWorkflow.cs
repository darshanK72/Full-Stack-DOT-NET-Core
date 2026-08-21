using System;

namespace DatabaseFirstAndReverseEngineering.Utils;

/*
 * FILE ROLE:
 *   Re-scaffold workflow, Package Manager Console preview, and table/schema filtering preview.
 *
 * SECTIONS IN THIS FILE:
 *   8. Re-scaffold workflow when the database changes
 *   9. PREVIEW: Scaffold-DbContext (Package Manager Console)
 *  10. PREVIEW: --table and --schema filtering
 */

/*
 * SECTION 8: RE-SCAFFOLD WORKFLOW WHEN THE DATABASE CHANGES
 *
 * When DBAs add columns, rename tables, or change FKs, refresh the model:
 *
 *   1. Apply SQL change scripts to the database (source of truth stays SQL).
 *   2. Commit or stash manual edits in generated files (there should be none).
 *   3. Re-run scaffold with --force (overwrites generated files).
 *   4. Rebuild; fix compile errors from renamed columns or dropped tables.
 *   5. Partial class files (*.Partial.cs) merge automatically - verify they still compile.
 *
 * | File kind              | On re-scaffold                         |
 * |------------------------|----------------------------------------|
 * | Category.cs            | Overwritten                            |
 * | Category.Partial.cs    | Untouched                              |
 * | InventoryDbContext.cs  | Overwritten                            |
 * | Custom services/repos  | Untouched (not generated)              |
 *
 * Team tip: treat scaffold as a build step or document the exact command in README;
 *   never "fix" generated mapping by hand - it will be lost on the next scaffold.
 * -------------------------------------------------------------------------
 */
public static class ScaffoldWorkflow
{
    public const string SampleReScaffoldCommand =
        "dotnet ef dbcontext scaffold \"" +
        ConnectionHelper.EfScaffoldTutorial +
        "\" Microsoft.EntityFrameworkCore.SqlServer " +
        "--output-dir Models --context-dir Data --context InventoryDbContext " +
        "--namespace DatabaseFirstAndReverseEngineering.Models " +
        "--context-namespace DatabaseFirstAndReverseEngineering.Data --force";

    public static void PrintReScaffoldChecklist()
    {
        Console.WriteLine("  Re-scaffold checklist:");
        Console.WriteLine("    1. DBA script applied to SQL Server");
        Console.WriteLine("    2. No manual edits in generated Models/*.cs or Data/*DbContext.cs");
        Console.WriteLine("    3. Run scaffold with --force");
        Console.WriteLine("    4. dotnet build - fix breaks in app code, not generated files");
        Console.WriteLine("    5. Partial classes (*.Partial.cs) still compile");
    }
}

/*
 * SECTION 9: PREVIEW - Scaffold-DbContext (PACKAGE MANAGER CONSOLE)
 *
 * Visual Studio Package Manager Console equivalent of dotnet ef dbcontext scaffold:
 *
 *   PM> Scaffold-DbContext "Server=(localdb)\MSSQLLocalDB;Database=EfScaffoldTutorial;..." `
 *         Microsoft.EntityFrameworkCore.SqlServer `
 *         -OutputDir Models -ContextDir Data -Context InventoryDbContext `
 *         -Namespace DatabaseFirstAndReverseEngineering.Models `
 *         -ContextNamespace DatabaseFirstAndReverseEngineering.Data -Force
 *
 * Requires Microsoft.EntityFrameworkCore.Design (see .csproj) and dotnet-ef global tool
 * or PMC-bundled EF tools. CLI and PMC share the same underlying reverse-engineering engine.
 * -------------------------------------------------------------------------
 */

/*
 * SECTION 10: PREVIEW - --table AND --schema FILTERING
 *
 * Large legacy databases: scaffold only what the app needs.
 *
 *   # Single table (repeat --table for more):
 *   dotnet ef dbcontext scaffold "..." Microsoft.EntityFrameworkCore.SqlServer \
 *       --table dbo.Products --output-dir Models --context-dir Data --force
 *
 *   # Limit to dbo schema (excludes audit.ChangeLog in this tutorial DB):
 *   dotnet ef dbcontext scaffold "..." Microsoft.EntityFrameworkCore.SqlServer \
 *       --schema dbo --output-dir Models --context-dir Data --force
 *
 *   # Combine both when you need specific tables from one schema:
 *   dotnet ef dbcontext scaffold "..." Microsoft.EntityFrameworkCore.SqlServer \
 *       --schema dbo --table dbo.Categories --table dbo.Products --force
 *
 * Without filters, scaffold pulls all tables and views the login can read.
 * COVERED IN DETAIL LATER: relationship tuning in ch.06, fluent overrides in ch.07.
 * -------------------------------------------------------------------------
 */
public static class ScaffoldFilterPreview
{
    public const string TableFilterExample =
        "--table dbo.Categories --table dbo.Products";

    public const string SchemaFilterExample =
        "--schema dbo";

    public static void PrintFilterHints()
    {
        Console.WriteLine("  Filter preview:");
        Console.WriteLine($"    Tables only: {TableFilterExample}");
        Console.WriteLine($"    Schema only: {SchemaFilterExample} (skips audit.ChangeLog)");
    }
}
