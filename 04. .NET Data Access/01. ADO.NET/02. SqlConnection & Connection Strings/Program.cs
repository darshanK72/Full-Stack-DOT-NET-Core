using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SqlConnectionAndConnectionStrings.Services;
using SqlConnectionAndConnectionStrings.Utils;

namespace SqlConnectionAndConnectionStrings;

/*
 * =============================================================================
 * TOPIC: Opening a SQL Server session with Microsoft.Data.SqlClient.SqlConnection,
 *        building connection strings safely, and managing connection lifetime.
 *
 * WHY IT MATTERS:
 *   Every ADO.NET read or write starts with a connection to SQL Server. A wrong
 *   connection string fails at runtime; a leaked connection exhausts the server.
 *   Dapper and EF Core hide SqlConnection, but the same keys, states, pooling,
 *   and security rules apply underneath. Master this chapter before SqlCommand.
 *
 * WHAT YOU WILL LEARN:
 *   1.  SqlConnection — constructor, ConnectionString property, DataSource, Database
 *   2.  Connection strings — semicolon key=value pairs and common keys
 *   3.  SqlConnectionStringBuilder — typed, validated string construction
 *   4.  Open / Close, ConnectionState, and the StateChange event
 *   5.  using and await using — IDisposable cleanup pattern
 *   6.  LocalDB and SQL Express connection examples (comments)
 *   7.  SqlException and invalid connection string errors
 *   8.  Security — never hardcode secrets; config and User Secrets preview
 *   9.  PREVIEW: connection pooling → ch.06; SqlCommand → ch.03
 *
 * CHAPTER MAP:
 *   1.  Connection string basics           → Utils/ConnectionStringSamples.cs
 *   2.  SqlConnectionStringBuilder         → Utils/DemoConnectionBuilder.cs
 *   3.  Connection string key reference    → Utils/DemoConnectionBuilder.cs
 *   4.  SqlConnection metadata             → Services/ConnectionLifecycleDemo.cs
 *   5.  Open / Close / ConnectionState     → Services/ConnectionLifecycleDemo.cs
 *   6.  StateChange event                    → Services/ConnectionLifecycleDemo.cs
 *   7.  using / await using disposal         → Services/ConnectionLifecycleDemo.cs
 *   8.  Connection string security         → Utils/ConnectionSecurityPreview.cs
 *   9.  Common errors (SqlException)       → Services/ConnectionErrorDemo.cs
 *  10.  Connection pooling preview         → Services/ConnectionPreviews.cs
 *  11.  SqlCommand preview                  → Services/ConnectionPreviews.cs
 *  12.  Demonstration                       → Program.cs Main (below)
 *
 * --- ONE-TIME DATABASE SETUP (run in SSMS or sqlcmd against master) ---
 *
 *   CREATE DATABASE AdoNetTutorial;
 *   GO
 *   USE AdoNetTutorial;
 *   GO
 *   CREATE TABLE dbo.DemoContact (
 *       ContactId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
 *       FullName  NVARCHAR(100) NOT NULL
 *   );
 *   INSERT INTO dbo.DemoContact (FullName) VALUES (N'Ada Lovelace');
 *
 * Live demos use (localdb)\MSSQLLocalDB and database AdoNetTutorial.
 * If LocalDB or the database is unavailable, offline sections still run.
 * =============================================================================
 */
public class Program
{
    private const string LocalDbServer = @"(localdb)\MSSQLLocalDB";
    private const string DemoDatabase = "AdoNetTutorial";

    /*
     * SECTION 12: DEMONSTRATION — Main orchestrates the chapter demo
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== SqlConnection & Connection Strings ===");
        Console.WriteLine();

        string builtConnectionString = DemoConnectionBuilder.BuildLocalDbString(
            LocalDbServer,
            DemoDatabase,
            connectionTimeoutSeconds: 15,
            multipleActiveResultSets: false);

        Console.WriteLine("--- Section 2: SqlConnectionStringBuilder ---");
        Console.WriteLine($"  Built string: {builtConnectionString}");
        DemoConnectionBuilder.PrintBuilderBreakdown(builtConnectionString);
        Console.WriteLine();

        Console.WriteLine("--- Section 5: Literal sample constant ---");
        Console.WriteLine($"  Sample: {ConnectionStringSamples.LocalDbIntegratedSecurity}");
        Console.WriteLine();

        ConnectionSecurityPreview.PrintSecurityGuidance();
        Console.WriteLine();

        ConnectionErrorDemo.DemonstrateInvalidConnectionString();
        ConnectionErrorDemo.DemonstrateUnreachableServer();
        Console.WriteLine();

        PoolingPreview.PrintPoolingNote();
        CommandPreview.PrintCommandNote();
        Console.WriteLine();

        Console.WriteLine("--- Live LocalDB open (try/catch if unavailable) ---");
        bool liveOk = ConnectionOpenCloseDemo.TryOpenAndReport(builtConnectionString, out string? version);
        if (liveOk)
        {
            Console.WriteLine($"  Live connection succeeded. ServerVersion = {version}");
        }
        else
        {
            Console.WriteLine("  Live connection skipped — run setup SQL in file header comments.");
        }
        Console.WriteLine();

        if (liveOk)
        {
            Console.WriteLine("--- Section 6–7: StateChange + using block ---");
            DisposalPatternDemo.UsingBlockPattern(builtConnectionString);
            Console.WriteLine();

            Console.WriteLine("--- Section 7: await using + OpenAsync ---");
            try
            {
                DisposalPatternDemo.AwaitUsingPatternAsync(builtConnectionString)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"  await using OpenAsync failed: {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== Chapter demo complete ===");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — SQLCONNECTION & CONNECTION STRINGS
 * =========================================================================
 *
 * --- Package ---
 *
 *   Microsoft.Data.SqlClient (this repo: 5.2.2)
 *
 * --- Build a string ---
 *
 *   var b = new SqlConnectionStringBuilder {
 *       DataSource = @"(localdb)\MSSQLLocalDB",
 *       InitialCatalog = "AdoNetTutorial",
 *       IntegratedSecurity = true,
 *       Encrypt = true,
 *       TrustServerCertificate = true,   // dev only
 *       ConnectTimeout = 15,
 *       MultipleActiveResultSets = false
 *   };
 *   string cs = b.ConnectionString;
 *
 * --- Open / close ---
 *
 *   using var conn = new SqlConnection(cs);
 *   conn.Open();
 *   // conn.State == ConnectionState.Open
 *   conn.Close();   // optional inside using — Dispose closes
 *
 * --- Async (full chapter ch.08) ---
 *
 *   await using var conn = new SqlConnection(cs);
 *   await conn.OpenAsync();
 *
 * --- StateChange ---
 *
 *   conn.StateChange += (s, e) =>
 *       Console.WriteLine($"{e.OriginalState} -> {e.CurrentState}");
 *
 * --- Common connection string keys ---
 *
 *   Server / Data Source          SQL Server host or (localdb)\Instance
 *   Database / Initial Catalog    database name
 *   Integrated Security=true      Windows authentication
 *   User ID / Password            SQL authentication (secrets only)
 *   Encrypt=true                  encrypt transport (default in SqlClient 5)
 *   TrustServerCertificate=true   skip cert validation — local dev
 *   Connection Timeout=15         seconds to wait on Open
 *   MultipleActiveResultSets=true MARS
 *
 * --- LocalDB / Express (examples) ---
 *
 *   (localdb)\MSSQLLocalDB
 *   .\SQLEXPRESS
 *
 * --- Exceptions ---
 *
 *   ArgumentException   malformed connection string
 *   SqlException        network, login, or server errors (see Number)
 *
 * --- Security ---
 *
 *   Configuration / User Secrets / Key Vault — not hardcoded passwords
 *
 * --- Common mistakes ---
 *
 *  Mistake                           | Result
 *  ----------------------------------|----------------------------------------
 *  Typo in key name in raw string    | Silently ignored — wrong server/db
 *  Forgotten Dispose / using           | Pool exhaustion, server load
 *  TrustServerCertificate in prod    | MITM risk
 *  Password in git                     | Credential leak
 *  Holding connection open minutes   | Blocks pool slot — open late, close early
 *
 * --- Related chapters ---
 *
 *   01. Introduction to ADO.NET       stack, connected vs disconnected
 *   03. SqlCommand & Parameters       ExecuteNonQuery, parameters
 *   06. Transactions & Connection Pooling   pooling, SqlTransaction
 *   08. Async ADO.NET                   OpenAsync, cancellation
 *
 * =========================================================================
 */
