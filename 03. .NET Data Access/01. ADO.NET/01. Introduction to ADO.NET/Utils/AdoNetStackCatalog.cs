using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace IntroductionToAdoNet.Utils;

/*
 * FILE ROLE: Catalog of ADO.NET stack layers and representative type names (Section 1).
 * SECTIONS IN THIS FILE:
 *   1. The ADO.NET stack in .NET
 */

/*
 * =========================================================================
 * SECTION 1: THE ADO.NET STACK IN .NET
 * =========================================================================
 *
 * ADO.NET is not one NuGet package — it is a layered API spread across the
 * Base Class Library (BCL) and database-specific provider packages.
 *
 *   Layer                          | Namespace / package          | Role
 *   -------------------------------|------------------------------|-------------------------------
 *   Common abstractions            | System.Data.Common           | DbConnection, DbCommand,
 *                                  |                              | DbDataReader, DbTransaction
 *   Disconnected in-memory model   | System.Data                  | DataSet, DataTable, DataRow,
 *                                  |                              | DataColumn, DataView
 *   SQL Server provider (current)  | Microsoft.Data.SqlClient     | SqlConnection, SqlCommand,
 *                                  | (NuGet — ch.02 onward)       | SqlDataReader, SqlTransaction
 *   SQL Server provider (legacy)   | System.Data.SqlClient        | Same type names; frozen for
 *                                  | (deprecated in-box copy)     | .NET Framework compatibility
 *
 * Your code should target the *abstract* types (DbConnection, DbCommand) when
 * writing provider-agnostic libraries, and the *concrete* Sql* types in apps
 * that always talk to SQL Server. Dapper and EF Core use these same primitives
 * under the hood.
 *
 * This chapter uses only BCL assemblies — no live SQL Server and no SqlClient
 * package yet. Chapters 02–08 add Microsoft.Data.SqlClient and real connections.
 * -------------------------------------------------------------------------
 */
public static class AdoNetStackCatalog
{
    public static IReadOnlyList<string> DescribeLayers()
    {
        return new[]
        {
            $"Common abstractions  → {typeof(DbConnection).FullName}",
            $"Common abstractions  → {typeof(DbCommand).FullName}",
            $"Common abstractions  → {typeof(DbDataReader).FullName}",
            $"Common abstractions  → {typeof(DbTransaction).FullName}",
            $"Disconnected model    → {typeof(DataSet).FullName}",
            $"Disconnected model    → {typeof(DataTable).FullName}",
            $"SQL Server (ch.02+)   → Microsoft.Data.SqlClient.SqlConnection",
            $"SQL Server (ch.02+)   → Microsoft.Data.SqlClient.SqlCommand",
            $"SQL Server (ch.02+)   → Microsoft.Data.SqlClient.SqlDataReader",
        };
    }

    public static bool IsProviderAbstract(Type type)
    {
        return type.IsAbstract; // DbConnection, DbCommand, etc. cannot be new'd directly
    }
}
