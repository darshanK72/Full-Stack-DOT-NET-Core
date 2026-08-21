using System;
using System.Data;
using Microsoft.Data.SqlClient;
using SqlCommandAndParameters.Utils;

namespace SqlCommandAndParameters.Repositories;

/*
 * FILE ROLE:
 *   Product table data-access methods demonstrating SqlCommand Execute* methods,
 *   parameters, SQL injection prevention, CommandTimeout, and disposal.
 *
 * SECTIONS IN THIS FILE:
 *   2. ExecuteNonQuery — rows affected (DML / DDL)
 *   3. ExecuteScalar — first column, first row
 *   4. ExecuteReader — preview (forward-only reads)
 *   5. SqlParameter — names, types, AddWithValue pitfalls
 *   6. SQL injection — concatenation vs parameters
 *   7. CommandTimeout
 *   8. using / disposing SqlCommand
 */

public sealed class ProductRepository
{
    /*
     * =========================================================================
     * SECTION 2: ExecuteNonQuery — ROWS AFFECTED (DML / DDL)
     * =========================================================================
     *
     * ExecuteNonQuery runs INSERT, UPDATE, DELETE, CREATE TABLE, etc.
     * Returns int — number of rows affected (-1 for some DDL/set statements).
     *
     * Does NOT return a result set. For SELECT rows use ExecuteReader (ch04).
     * For a single aggregate value use ExecuteScalar (SECTION 3).
     *
     * Always use parameters for values that came from user input or variables.
     * -------------------------------------------------------------------------
     */
    public int InsertProduct(SqlConnection connection, string name, decimal unitPrice, int stock)
    {
        const string sql = """
            INSERT INTO dbo.Products (Name, UnitPrice, Stock)
            VALUES (@name, @price, @stock);
            """;

        using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
        command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100) { Value = name });
        command.Parameters.Add(new SqlParameter("@price", SqlDbType.Decimal)
        {
            Value = unitPrice,
            Precision = 10,
            Scale = 2,
        });
        command.Parameters.Add(new SqlParameter("@stock", SqlDbType.Int) { Value = stock });

        int rowsAffected = command.ExecuteNonQuery(); // blocks until SQL Server finishes
        return rowsAffected;                          // INSERT ? typically 1
    }

    public int UpdateStock(SqlConnection connection, int productId, int newStock)
    {
        const string sql = "UPDATE dbo.Products SET Stock = @stock WHERE ProductId = @id;";

        using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
        command.Parameters.Add(new SqlParameter("@stock", SqlDbType.Int) { Value = newStock });
        command.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = productId });

        return command.ExecuteNonQuery(); // returns 0 if ProductId not found, 1 if updated
    }

    /*
     * =========================================================================
     * SECTION 3: ExecuteScalar — FIRST COLUMN, FIRST ROW
     * =========================================================================
     *
     * ExecuteScalar is ideal when the query returns exactly one value:
     *
     *   SELECT COUNT(*), SELECT MAX(Price), SELECT SCOPE_IDENTITY()
     *
     * Return type is object? — cast or Convert to the expected CLR type.
     * If the query returns no rows, ExecuteScalar returns null (DBNull is
     * normalized to null by the provider for scalar reads).
     *
     * Not for multi-row result sets — use ExecuteReader (SECTION 4 preview).
     * -------------------------------------------------------------------------
     */
    public int GetActiveProductCount(SqlConnection connection)
    {
        const string sql = "SELECT COUNT(*) FROM dbo.Products WHERE IsActive = 1;";

        using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
        object? scalar = command.ExecuteScalar(); // one round-trip, one value

        return scalar is null ? 0 : Convert.ToInt32(scalar);
    }

    public decimal GetAverageUnitPrice(SqlConnection connection)
    {
        const string sql = "SELECT AVG(UnitPrice) FROM dbo.Products WHERE IsActive = 1;";

        using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
        object? scalar = command.ExecuteScalar();

        return scalar is null or DBNull ? 0m : Convert.ToDecimal(scalar);
    }

    /*
     * =========================================================================
     * SECTION 4: ExecuteReader — PREVIEW (FORWARD-ONLY READS)
     * =========================================================================
     *
     * ExecuteReader sends the command and returns a live SqlDataReader — a
     * forward-only, read-only stream of rows. The connection stays busy until
     * the reader is closed/disposed (unless MARS is enabled on the connection).
     *
     * Minimal pattern shown here; typed accessors, IsDBNull, multiple result
     * sets, and performance patterns are ch04 SqlDataReader.
     *
     * COVERED IN DETAIL LATER → 04. SqlDataReader
     * -------------------------------------------------------------------------
     */
    public void PreviewExecuteReader(SqlConnection connection)
    {
        const string sql = """
            SELECT TOP 3 ProductId, Name, UnitPrice, Stock
            FROM dbo.Products
            WHERE IsActive = 1
            ORDER BY Name;
            """;

        using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
        using SqlDataReader reader = command.ExecuteReader(); // connection busy until reader disposed

        Console.WriteLine("--- ExecuteReader preview (first 3 products) ---");
        while (reader.Read()) // advance one row; false when no more rows
        {
            int id = reader.GetInt32(reader.GetOrdinal("ProductId"));
            string name = reader.GetString(reader.GetOrdinal("Name"));
            decimal price = reader.GetDecimal(reader.GetOrdinal("UnitPrice"));
            int stock = reader.GetInt32(reader.GetOrdinal("Stock"));
            Console.WriteLine($"  [{id}] {name} — ${price:F2}, stock {stock}");
        }

        Console.WriteLine("  (Full reader API ? ch04 SqlDataReader)");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 5: SqlParameter — NAMES, TYPES, AddWithValue PITFALLS
     * =========================================================================
     *
     * SQL Server parameters use @ prefix in both SQL and C#:
     *
     *   WHERE Name = @name     ?     new SqlParameter("@name", …)
     *
     * Names must match (case-insensitive on SQL Server). Omitting @ in C# still
     * works if AddWithValue("name", …) but the SQL placeholder must be @name.
     *
     * Preferred — typed parameter with explicit SqlDbType (and Size for strings):
     *
     *   cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100) { Value = name });
     *
     * AddWithValue(name, value) is convenient but has pitfalls:
     *
     *   Pitfall                         | Why it hurts
     *   --------------------------------|------------------------------------------
     *   Inferred type/size from CLR     | string ? NVARCHAR(4000) or NVARCHAR(MAX)
     *                                   | can prevent index use and bloat plan cache
     *   Ambiguous numeric inference     | small ints vs bigint mismatches in edge cases
     *   No Precision/Scale on decimal   | rounding surprises on money columns
     *
     * Rule of thumb: AddWithValue for quick prototypes; explicit SqlParameter
     * (or Add(name, SqlDbType).Value = …) in production code.
     *
     * Parameter collection helpers:
     *   Parameters.Add(SqlParameter)
     *   Parameters.AddWithValue(string, object)   // infer type — see pitfalls
     *   Parameters.Add(string, SqlDbType).Value = … // typed without full ctor
     * -------------------------------------------------------------------------
     */
    public void DemonstrateTypedParameters(SqlConnection connection)
    {
        const string sql = """
            SELECT ProductId, Name, UnitPrice
            FROM dbo.Products
            WHERE Name LIKE @namePattern AND UnitPrice >= @minPrice;
            """;

        using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);

        // Explicit typed parameters — Size on string, Precision/Scale on decimal
        command.Parameters.Add(new SqlParameter("@namePattern", SqlDbType.NVarChar, 100)
        {
            Value = "M%", // products starting with M
        });
        command.Parameters.Add(new SqlParameter("@minPrice", SqlDbType.Decimal)
        {
            Value = 15.00m,
            Precision = 10,
            Scale = 2,
        });

        using SqlDataReader reader = command.ExecuteReader();
        Console.WriteLine("--- Typed parameters (@namePattern, @minPrice) ---");
        while (reader.Read())
        {
            Console.WriteLine($"  {reader.GetString(1)} — ${reader.GetDecimal(2):F2}");
        }

        Console.WriteLine();
    }

    public void DemonstrateAddWithValuePitfall(SqlConnection connection)
    {
        /*
         * Same query with AddWithValue — works, but the provider picks types/sizes
         * from the runtime values. A long user-supplied string may become NVARCHAR(MAX)
         * in the plan while the column is NVARCHAR(100), hurting index seeks.
         */
        const string sql = "SELECT COUNT(*) FROM dbo.Products WHERE Name = @name;";

        using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
        command.Parameters.AddWithValue("@name", "Mouse"); // infers NVARCHAR from string length

        object? count = command.ExecuteScalar();
        Console.WriteLine("--- AddWithValue (@name = 'Mouse') ---");
        Console.WriteLine($"  Matching rows: {count}");
        Console.WriteLine("  Prefer explicit SqlDbType + Size in production to avoid plan-cache noise.");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 6: SQL INJECTION — CONCATENATION VS PARAMETERS
     * =========================================================================
     *
     * NEVER build SQL by concatenating untrusted input:
     *
     *   "SELECT * FROM Products WHERE Name = '" + userInput + "'"
     *
     * Attackers inject SQL fragments — e.g. userInput = `' OR 1=1 --`
     * returns every row or worse (DROP TABLE, xp_cmdshell in privileged setups).
     *
     * Parameters send values separately from the command text. SQL Server treats
     * parameter values as data, not executable SQL — the statement shape is fixed
     * at compile time.
     *
     * The bad example below uses a constant malicious string to illustrate the
     * logical flaw; it is NOT executed against the database (would be unsafe).
     * The safe path uses @name and ExecuteScalar.
     * -------------------------------------------------------------------------
     */
    public void DemonstrateSqlInjectionPrevention(SqlConnection connection)
    {
        string userInput = "Mouse";

        // UNSAFE pattern — shown for education only; do not run arbitrary user text this way
        string unsafeSql = "SELECT COUNT(*) FROM dbo.Products WHERE Name = '" + userInput + "';";

        string maliciousInput = "' OR 1=1 --";
        string exploitedSql = "SELECT COUNT(*) FROM dbo.Products WHERE Name = '" + maliciousInput + "';";
        // exploitedSql becomes: … WHERE Name = '' OR 1=1 --'  ? returns all rows

        Console.WriteLine("--- SQL injection: concatenation vs parameters ---");
        Console.WriteLine($"  Benign concat SQL:     {unsafeSql}");
        Console.WriteLine($"  Malicious concat SQL:  {exploitedSql}");
        Console.WriteLine("  Concatenation lets attacker alter query logic — parameters do not.");

        const string safeSql = "SELECT COUNT(*) FROM dbo.Products WHERE Name = @name;";
        using SqlCommand safeCommand = CommandFactory.CreateTextCommand(connection, safeSql);
        safeCommand.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100)
        {
            Value = maliciousInput, // treated as literal string data, not SQL
        });

        object? safeCount = safeCommand.ExecuteScalar();
        Console.WriteLine($"  Parameterized count for \"{maliciousInput}\": {safeCount} (no name match — safe)");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 7: CommandTimeout
     * =========================================================================
     *
     * CommandTimeout (seconds) applies to the Execute* call, not connection open.
     * Default is 30. Set on the SqlCommand instance:
     *
     *   command.CommandTimeout = 60;   // wait up to 60 seconds
     *   command.CommandTimeout = 0;    // no timeout (wait indefinitely)
     *
     * On timeout SqlException is thrown (Number often  -2 — client timeout).
     * Long-running reports may need a higher value; OLTP should stay low to
     * fail fast. ch.08 Async ADO.NET covers cancellation tokens for async paths.
     * -------------------------------------------------------------------------
     */
    public void DemonstrateCommandTimeout(SqlConnection connection)
    {
        using SqlCommand command = CommandFactory.CreateTextCommand(
            connection,
            "SELECT COUNT(*) FROM dbo.Products;");

        command.CommandTimeout = 15; // override default 30 for this command only

        object? result = command.ExecuteScalar();
        Console.WriteLine("--- CommandTimeout ---");
        Console.WriteLine($"  CommandTimeout = {command.CommandTimeout}s — scalar returned: {result}");
        Console.WriteLine("  0 = infinite wait; default 30; tune per command for long/short work.");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 8: USING / DISPOSING SqlCommand
     * =========================================================================
     *
     * SqlCommand implements IDisposable. Dispose:
     *
     *   • Releases the command object and parameter collection
     *   • Does NOT close the SqlConnection (connection lifetime is separate)
     *   • Cancels a pending async operation if still running (async — ch08)
     *
     * Pattern: using var cmd = … or connection.CreateCommand() inside using.
     * Create one command per operation; do not share a command across threads.
     *
     * If ExecuteReader is open, dispose the reader first (or use nested using).
     * -------------------------------------------------------------------------
     */
    public void DemonstrateCommandDisposal(SqlConnection connection)
    {
        int count;
        using (SqlCommand command = CommandFactory.CreateTextCommand(
                   connection,
                   "SELECT COUNT(*) FROM dbo.Products;"))
        {
            count = Convert.ToInt32(command.ExecuteScalar());
        } // command.Dispose() called here — connection remains open

        Console.WriteLine("--- SqlCommand disposal ---");
        Console.WriteLine($"  Command disposed after scalar; connection still open. Product count: {count}");
        Console.WriteLine();
    }
}
