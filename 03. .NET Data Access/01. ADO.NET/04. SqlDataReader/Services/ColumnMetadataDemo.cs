using System;
using Microsoft.Data.SqlClient;
using SqlClientReader = Microsoft.Data.SqlClient.SqlDataReader;

namespace SqlDataReader;

/*
 * FILE ROLE: Inspects reader column metadata and compares indexer vs typed accessors.
 *
 * SECTIONS IN THIS FILE:
 *   5. Column metadata — GetName, GetFieldType, GetOrdinal, indexer
 */

/*
 * =========================================================================
 * SECTION 5: COLUMN METADATA — GetName, GetFieldType, GetOrdinal, INDEXER
 * =========================================================================
 *
 * Before reading rows, inspect schema:
 *
 *   reader.FieldCount          number of columns in current result set
 *   reader.GetName(i)            column name at ordinal i
 *   reader.GetFieldType(i)       CLR type the provider exposes
 *   reader.GetOrdinal("Name")    resolve name → ordinal (throws if missing)
 *
 * Indexer alternatives to GetXxx:
 *   reader["ProductName"]        returns object — requires cast/unbox
 *   reader[ordinal]              same, by position
 *
 * GetOrdinal inside the row loop is slower — cache ordinals before Read().
 * -------------------------------------------------------------------------
 */
public static class ColumnMetadataDemo
{
    public static void Run(string connectionString)
    {
        Console.WriteLine("--- SECTION 5: metadata + indexer ---");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "SELECT TOP 1 ProductId, ProductName, UnitPrice FROM dbo.Products ORDER BY ProductId;",
            connection);

        using SqlClientReader reader = command.ExecuteReader();

        Console.WriteLine($"  FieldCount: {reader.FieldCount}");
        for (int i = 0; i < reader.FieldCount; i++)
        {
            string name = reader.GetName(i);
            Type clrType = reader.GetFieldType(i);
            Console.WriteLine($"    [{i}] {name} → {clrType.Name}");
        }

        if (reader.Read())
        {
            int nameOrdinal = reader.GetOrdinal("ProductName");
            object boxedName = reader["ProductName"];
            object boxedByOrdinal = reader[nameOrdinal];
            string typedName = reader.GetString(nameOrdinal);

            Console.WriteLine($"  Indexer [\"ProductName\"]: {boxedName} (object)");
            Console.WriteLine($"  Indexer [{nameOrdinal}]:     {boxedByOrdinal} (object)");
            Console.WriteLine($"  GetString(ordinal):          {typedName}");
        }
    }
}
