namespace DapperMappingAndAdvancedPatterns.Utils;

/*
 * FILE ROLE:
 *   Pointer to Dapper.SqlBuilder — grouped preview only (no runnable code in this chapter).
 *
 * SECTIONS IN THIS FILE:
 *   8. Dapper.SqlBuilder — preview pointer
 */

/*
 * SECTION 8: DAPPER.SQLBUILDER — PREVIEW POINTER
 *
 * Dapper.SqlBuilder (community package Dapper.SqlBuilder) builds dynamic WHERE/ORDER BY
 * clauses from a template string with magic placeholder tokens (select, where, orderby).
 *
 *   var builder = new SqlBuilder();
 *   var template = builder.AddTemplate(
 *       selectFromProductsTemplate,
 *       new { Status = "Active" });
 *   builder.Where("IsActive = @Status");
 *   builder.OrderBy("Name");
 *   connection.Query<Product>(template.RawSql, template.Parameters);
 *
 * The template literal embeds placeholder tokens that SqlBuilder replaces with the
 * generated SELECT column list, WHERE clause, and ORDER BY fragment.
 *
 * Useful for optional filters in report/search APIs without string concatenation.
 *
 * Not demonstrated here — dynamic SQL composition deserves its own focused treatment.
 * See Dapper.SqlBuilder NuGet package and official Dapper GitHub examples when you
 * need optional predicate building beyond anonymous parameters from ch.03.
 * -------------------------------------------------------------------------
 */
public static class SqlBuilderPreview
{
    public const string PackageName = "Dapper.SqlBuilder";
}
