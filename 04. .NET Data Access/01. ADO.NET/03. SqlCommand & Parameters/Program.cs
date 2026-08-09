using System;
using Microsoft.Data.SqlClient;
using SqlCommandAndParameters.Repositories;
using SqlCommandAndParameters.Utils;

namespace SqlCommandAndParameters;

/*
 * =============================================================================
 * TOPIC: SqlCommand — send SQL (or stored procedures) to SQL Server through an
 *        open SqlConnection. Parameters bind values safely; ExecuteNonQuery,
 *        ExecuteScalar, and ExecuteReader run the command and return different
 *        result shapes.
 *
 * WHY IT MATTERS:
 *   Every INSERT, UPDATE, DELETE, and SELECT in ADO.NET flows through SqlCommand.
 *   Dapper and EF Core generate commands under the hood — understanding CommandText,
 *   parameters, and the three Execute* methods is how you debug slow queries, fix
 *   injection bugs, and tune timeouts. String-concatenated SQL from user input is
 *   one of the most exploited vulnerability classes in web apps; parameters are
 *   the fix.
 *
 * WHAT YOU WILL LEARN:
 *   1.  SqlCommand properties — CommandText, CommandType (Text default), Connection
 *   2.  ExecuteNonQuery — rows affected (INSERT/UPDATE/DELETE/DDL)
 *   3.  ExecuteScalar — first column of first row (aggregates, identity)
 *   4.  ExecuteReader — forward-only result stream (preview → ch04)
 *   5.  SqlParameter — names @p, typed parameters, AddWithValue pitfalls
 *   6.  SQL injection — unsafe concatenation vs parameterized queries
 *   7.  CommandTimeout — seconds before the server cancels the command
 *   8.  using / Dispose on SqlCommand (and why Connection is separate)
 *   9.  Transaction property preview (? ch06 Transactions)
 *  10.  CommandType.StoredProcedure preview (? ch07 Stored Procedures)
 *
 * CHAPTER MAP:
 *   1.  SqlCommand core properties        → Utils/CommandFactory.cs
 *   2.  ExecuteNonQuery                   → Repositories/ProductRepository.cs
 *   3.  ExecuteScalar                     → Repositories/ProductRepository.cs
 *   4.  ExecuteReader preview             → Repositories/ProductRepository.cs
 *   5.  SqlParameter / AddWithValue        → Repositories/ProductRepository.cs
 *   6.  SQL injection prevention          → Repositories/ProductRepository.cs
 *   7.  CommandTimeout                    → Repositories/ProductRepository.cs
 *   8.  SqlCommand disposal               → Repositories/ProductRepository.cs
 *   9.  Transaction property preview      → Utils/CommandPreviews.cs
 *  10.  StoredProcedure preview            → Utils/CommandPreviews.cs
 *  11.  Demonstration                     → Program.cs Main (below)
 *
 * DATABASE SETUP — run in SSMS or sqlcmd against (localdb)\MSSQLLocalDB:
 *
 *   CREATE DATABASE AdoNetTutorial;
 *   GO
 *   USE AdoNetTutorial;
 *   GO
 *   CREATE TABLE dbo.Products (
 *       ProductId   INT IDENTITY(1,1) PRIMARY KEY,
 *       Name        NVARCHAR(100) NOT NULL,
 *       UnitPrice   DECIMAL(10,2) NOT NULL,
 *       Stock       INT NOT NULL DEFAULT 0,
 *       IsActive    BIT NOT NULL DEFAULT 1
 *   );
 *   GO
 *   INSERT INTO dbo.Products (Name, UnitPrice, Stock) VALUES
 *       (N'Keyboard', 49.99, 25),
 *       (N'Mouse', 19.99, 100),
 *       (N'Monitor', 299.99, 10);
 *   GO
 *   CREATE OR ALTER PROCEDURE dbo.usp_ProductCount
 *   AS
 *   BEGIN
 *       SET NOCOUNT ON;
 *       SELECT COUNT(*) FROM dbo.Products WHERE IsActive = 1;
 *   END;
 *   GO
 *
 * Connection string used below:
 *   Server=(localdb)\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true
 * =============================================================================
 */
public class Program
{
    private const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true";

    /*
     * SECTION 11: DEMONSTRATION — Main orchestrates the chapter demo
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== SqlCommand & Parameters (ADO.NET ch03) ===");
        Console.WriteLine();

        var repository = new ProductRepository();

        try
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open(); // ch02 — must be open before any Execute*
            Console.WriteLine($"Connected: {connection.DataSource} / {connection.Database}");
            Console.WriteLine();

            int inserted = repository.InsertProduct(connection, "Webcam", 79.99m, 15);
            Console.WriteLine($"ExecuteNonQuery INSERT — rows affected: {inserted}");

            int updated = repository.UpdateStock(connection, productId: 1, newStock: 30);
            Console.WriteLine($"ExecuteNonQuery UPDATE ProductId=1 — rows affected: {updated}");
            Console.WriteLine();

            int activeCount = repository.GetActiveProductCount(connection);
            decimal avgPrice = repository.GetAverageUnitPrice(connection);
            Console.WriteLine($"ExecuteScalar — active products: {activeCount}, avg price: ${avgPrice:F2}");
            Console.WriteLine();

            repository.PreviewExecuteReader(connection);
            repository.DemonstrateTypedParameters(connection);
            repository.DemonstrateAddWithValuePitfall(connection);
            repository.DemonstrateSqlInjectionPrevention(connection);
            repository.DemonstrateCommandTimeout(connection);
            repository.DemonstrateCommandDisposal(connection);
            TransactionPreview.PreviewCommandTransaction(connection);
            StoredProcedurePreview.PreviewStoredProcedure(connection);
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Database unavailable or setup incomplete.");
            Console.WriteLine($"  SqlException: {ex.Message}");
            Console.WriteLine("  Run the DATABASE SETUP block at the top of Program.cs in SSMS/sqlcmd.");
            Console.WriteLine("  Ensure LocalDB is installed: Server=(localdb)\\MSSQLLocalDB");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — SQLCOMMAND & PARAMETERS
 * =========================================================================
 *
 * --- Construct ---
 *
 *   using var cmd = connection.CreateCommand();
 *   cmd.CommandText = "UPDATE … SET Col = @p WHERE Id = @id";
 *   cmd.CommandType = CommandType.Text;   // default
 *
 *   using var cmd = new SqlCommand(sql, connection);
 *
 * --- Execute methods ---
 *
 *   int rows = cmd.ExecuteNonQuery();     // INSERT/UPDATE/DELETE/DDL ? rows affected
 *   object? val = cmd.ExecuteScalar();    // one value — COUNT, MAX, SCOPE_IDENTITY()
 *   SqlDataReader r = cmd.ExecuteReader(); // many rows — ch04 for full reader API
 *
 * --- Parameters (prefer typed) ---
 *
 *   cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = 42 });
 *   cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100) { Value = name });
 *   cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = 9.99m;  // alternate typed add
 *
 *   cmd.Parameters.AddWithValue("@x", value);  // quick but infers type/size — see pitfalls
 *
 * --- Security ---
 *
 *   NEVER: "… WHERE Name = '" + userInput + "'"
 *   ALWAYS: "… WHERE Name = @name" + parameter with Value = userInput
 *
 * --- Other properties ---
 *
 *   cmd.CommandTimeout = 30;        // seconds; 0 = no limit
 *   cmd.Transaction = tx;           // ch06 — enlist in SqlTransaction
 *   cmd.CommandType = CommandType.StoredProcedure;  // ch07 — procedure name in CommandText
 *
 * --- Disposal ---
 *
 *   using var cmd = …;             // Dispose command; does NOT close connection
 *   using var reader = cmd.ExecuteReader();  // dispose reader before next command on connection
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Execute* with closed connection        | InvalidOperationException
 *  String concat with user input          | SQL injection
 *  AddWithValue for every production param| Suboptimal plans, type/size inference
 *  Forget to dispose open SqlDataReader   | Connection stuck until reader closed
 *  Share one SqlCommand across threads    | Undefined behavior — not thread-safe
 *  Wrong @ name vs SQL placeholder        | Parameter not bound / wrong value
 *
 * --- Related chapters ---
 *
 *   02. SqlConnection & Connection Strings   open, close, connection string
 *   04. SqlDataReader                        ExecuteReader depth, IsDBNull, NextResult
 *   06. Transactions & Connection Pooling    BeginTransaction, Commit, Rollback
 *   07. Stored Procedures & Output Parameters CommandType.StoredProcedure, output params
 *   08. Async ADO.NET                        ExecuteNonQueryAsync, cancellation
 *
 * =========================================================================
 */
