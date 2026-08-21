using System;

namespace QueriesExecuteAndAsyncMethods.Utils;

/*
 * FILE ROLE:
 *   Shared connection string for AdoNetTutorial on LocalDB — same database as ADO.NET chapters.
 *
 * SECTIONS IN THIS FILE:
 *   2. Connection string constant
 */

/*
 * =========================================================================
 * SECTION 2: CONNECTION STRING
 * =========================================================================
 *
 * Dapper extends IDbConnection (SqlConnection implements it). Open the connection
 * before Query/Execute, or pass an already-open connection from a unit-of-work.
 *
 * TrustServerCertificate=true avoids dev certificate prompts on modern SqlClient.
 * -------------------------------------------------------------------------
 */
public static class ConnectionHelper
{
    public const string AdoNetTutorial =
        "Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true";
}
