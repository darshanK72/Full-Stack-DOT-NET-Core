using System;
using System.Data;

namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Sets PrimaryKey, UniqueConstraint, and demonstrates ForeignKeyConstraint enforcement.
 *
 * SECTIONS IN THIS FILE:
 *   7. Primary keys and constraints (basic)
 */

/*
 * =========================================================================
 * SECTION 7: PRIMARY KEYS AND CONSTRAINTS (BASIC)
 * =========================================================================
 *
 * PrimaryKey - required for Rows.Find and for SqlDataAdapter to match UPDATE/
 * DELETE commands to the correct row. Set before Fill or before Add rows:
 *
 *   table.PrimaryKey = new DataColumn[] { table.Columns["ProductId"]! };
 *
 * Constraint types on DataTable.Constraints:
 *
 *   UniqueConstraint   - no duplicate values in one or more columns
 *   ForeignKeyConstraint - child values must exist in parent (same DataSet)
 *
 * Violations throw System.Data.ConstraintException at runtime in memory -
 * same idea as SQL UNIQUE / FK errors, but enforced locally before Update.
 * -------------------------------------------------------------------------
 */
public static class ConstraintsDemo
{
    public static void DemonstrateKeysAndConstraints()
    {
        Console.WriteLine("=== PrimaryKey, UniqueConstraint, ForeignKeyConstraint ===");

        DataSet catalog = CatalogDataSetBuilder.CreateCatalogDataSet();
        DataTable products = catalog.Tables["Products"]!;
        DataTable categories = catalog.Tables["Categories"]!;

        products.PrimaryKey = new[] { products.Columns["ProductId"]! };
        products.Constraints.Add(new UniqueConstraint("UQ_ProductName", products.Columns["ProductName"]!));

        // Rows.Find uses PrimaryKey values in order
        DataRow? found = products.Rows.Find(2);
        Console.WriteLine($"Rows.Find(2): {found?["ProductName"]}");

        // ForeignKeyConstraint already added via DataRelation in section 4
        try
        {
            DataRow bad = products.NewRow();
            bad["ProductId"] = 50;
            bad["ProductName"] = "Invalid FK Row";
            bad["UnitPrice"] = 1m;
            bad["IsDiscontinued"] = false;
            bad["CategoryId"] = 999; // no CategoryId 999 in parent
            products.Rows.Add(bad);
            Console.WriteLine("Unexpected: FK violation was allowed.");
        }
        catch (Exception ex) when (ex is ConstraintException or InvalidConstraintException)
        {
            Console.WriteLine($"FK blocked invalid CategoryId: {ex.Message}");
        }

        try
        {
            products.Rows.Add(1, "Duplicate Id Row", 1m, false, 1); // ProductId 1 already exists
            Console.WriteLine("Unexpected: duplicate key was allowed.");
        }
        catch (Exception ex) when (ex is ConstraintException or InvalidConstraintException)
        {
            Console.WriteLine($"Primary key blocked duplicate ProductId: {ex.Message}");
        }

        Console.WriteLine($"Categories still has {categories.Rows.Count} rows.");
        Console.WriteLine();
    }
}
