using System;

namespace IntroductionToDapper.Utils;

/*
 * FILE ROLE: Conceptual introduction — what Dapper is, NuGet packages, and when
 *            to choose Dapper vs raw ADO.NET vs EF Core.
 *
 * SECTIONS IN THIS FILE:
 *   1. What Dapper is — micro-ORM, IDbConnection extension methods
 *   2. NuGet setup — Microsoft.Data.SqlClient + Dapper
 *   4. Dapper vs ADO.NET vs EF Core — when to pick each
 */

public static class DapperConcepts
{
    /*
     * =========================================================================
     * SECTION 1: WHAT DAPPER IS — MICRO-ORM, IDbConnection EXTENSION METHODS
     * =========================================================================
     *
     * Dapper is a "micro-ORM" — not a full object-relational mapper like EF Core.
     * You write SQL; Dapper executes it and maps rows to .NET types.
     *
     * Core API surface (extension methods on System.Data.IDbConnection):
     *
     *   connection.Query<T>(sql)           — rows → IEnumerable<T>
     *   connection.Execute(sql, param)     — INSERT/UPDATE/DELETE → int rows
     *   connection.ExecuteScalar<T>(sql)   — single value
     *
     * Implementation lives in the Dapper NuGet package as static extension methods
     * in the Dapper namespace. Any IDbConnection (SqlConnection, NpgsqlConnection,
     * etc.) gains these methods after `using Dapper;`.
     *
     * Dapper adds almost no allocation overhead beyond the ADO.NET provider —
     * it uses your existing connection and command objects under the hood.
     *
     * Method families (Query*, Single*, First*, async variants) → ch02.
     * -------------------------------------------------------------------------
     */
    public static void ExplainWhatDapperIs()
    {
        Console.WriteLine("--- SECTION 1: What Dapper is ---");
        Console.WriteLine("  Micro-ORM: you write SQL, Dapper maps rows to POCOs.");
        Console.WriteLine("  Extension methods on IDbConnection — add `using Dapper;` to see Query, Execute, …");
        Console.WriteLine("  Full Query/Execute method families → ch02 Queries, Execute & Async Methods.");
    }

    /*
     * =========================================================================
     * SECTION 2: NUGET SETUP — Microsoft.Data.SqlClient + Dapper
     * =========================================================================
     *
     * This project's IntroductionToDapper.csproj already references:
     *
     *   <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
     *   <PackageReference Include="Dapper" Version="2.1.35" />
     *
     * Microsoft.Data.SqlClient — SQL Server ADO.NET provider (SqlConnection).
     * Dapper — mapping layer; no separate runtime config file required.
     *
     * Minimum code to use Dapper after restore:
     *
     *   using System.Data;
     *   using Dapper;
     *   using Microsoft.Data.SqlClient;
     *
     *   using IDbConnection db = new SqlConnection(connectionString);
     *   IEnumerable<Product> rows = db.Query<Product>("SELECT …");
     *
     * Target framework: net8.0 (same as other tutorials in this repo).
     * -------------------------------------------------------------------------
     */
    public static void ExplainNuGetSetup()
    {
        Console.WriteLine("--- SECTION 2: NuGet setup ---");
        Console.WriteLine("  Packages: Microsoft.Data.SqlClient 5.2.2 + Dapper 2.1.35 (see .csproj).");
        Console.WriteLine("  Usings: System.Data, Dapper, Microsoft.Data.SqlClient.");
    }

    /*
     * =========================================================================
     * SECTION 4: DAPPER VS ADO.NET VS EF CORE — WHEN TO PICK EACH
     * =========================================================================
     *
     * | Approach      | You write        | Mapping              | Best for
     * |---------------|------------------|----------------------|----------------------------------
     * | Raw ADO.NET   | SQL + reader loop| Manual GetInt32/…    | Max control, provider-specific APIs
     * | Dapper        | SQL              | Automatic Query<T>   | Hand-tuned SQL, microservices, reports
     * | EF Core       | LINQ + entities  | Change tracker + SQL | Rich domain model, migrations, CRUD apps
     *
     * Choose Dapper when:
     *   • You want full SQL control without EF's change tracking overhead
     *   • Performance matters and queries are hand-optimized
     *   • Team already knows SQL; POCO mapping is enough
     *
     * Stay on ADO.NET when:
     *   • You need provider-specific features Dapper does not wrap
     *   • Streaming huge result sets with SequentialAccess (ch04 ADO.NET)
     *
     * Choose EF Core when:
     *   • Migrations, relationships, and LINQ are primary workflow
     *   • Unit-of-work and change tracking simplify the domain layer
     *
     * Many production apps combine EF Core for writes/domain and Dapper for
     * read-heavy reporting queries on the same database.
     * -------------------------------------------------------------------------
     */
    public static void ExplainStackComparison()
    {
        Console.WriteLine("--- SECTION 4: Dapper vs ADO.NET vs EF Core ---");
        Console.WriteLine("  ADO.NET  — manual SqlDataReader loops (ch04); max control.");
        Console.WriteLine("  Dapper   — same SQL, Query<T> maps rows; minimal overhead.");
        Console.WriteLine("  EF Core  — LINQ + change tracker; best for rich domain CRUD.");
    }
}
