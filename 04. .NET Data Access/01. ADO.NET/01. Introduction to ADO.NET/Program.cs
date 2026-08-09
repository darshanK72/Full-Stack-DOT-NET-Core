/*
 * TOPIC: ADO.NET — the .NET data-access stack that sits between your application
 *        code and a database. You work through provider-specific types (for SQL
 *        Server: SqlConnection, SqlCommand, SqlDataReader) that inherit common
 *        abstractions in System.Data.Common, plus in-memory disconnected types
 *        (DataSet, DataTable) for caching and offline scenarios.
 *
 * WHY IT MATTERS:
 *   Every SQL Server call in .NET — whether you write raw ADO.NET, Dapper, or
 *   Entity Framework Core — ultimately opens connections, sends commands, and
 *   reads rows through these primitives. Understanding the stack helps you
 *   debug timeouts, connection leaks, transaction boundaries, and performance
 *   issues that ORMs hide until production. ADO.NET is also the right tool
 *   when you need full SQL control, bulk operations, or legacy integration.
 *
 * WHAT YOU WILL LEARN:
 *   1.  The ADO.NET stack: System.Data, System.Data.Common, and provider packages
 *   2.  Connected vs disconnected data-access models — when each fits
 *   3.  Key abstractions: DbConnection, DbCommand, DbDataReader (preview)
 *   4.  Disconnected types: DataSet, DataTable, DataRow (in-memory demo)
 *   5.  DbTransaction — atomic batches (preview → ch.06)
 *   6.  Provider model: Microsoft.Data.SqlClient vs legacy System.Data.SqlClient
 *   7.  Typical read/write workflow (diagram + simulated steps)
 *   8.  ADO.NET vs Dapper vs EF Core — choosing the right layer
 *
 * CHAPTER MAP:
 *   1.  ADO.NET stack layers              → Utils/AdoNetStackCatalog.cs
 *   2.  Connected vs disconnected        → Models/DataAccessModel.cs, Utils/ModelComparison.cs
 *   3.  DbConnection preview             → Utils/ConnectionConcept.cs
 *   4.  DbCommand preview                → Models/CommandExecuteMethod.cs, Utils/CommandConcept.cs
 *   5.  DbDataReader preview             → Services/ReaderDemo.cs
 *   6.  DataSet / DataTable preview      → Services/DisconnectedOrderCache.cs
 *   7.  DbTransaction preview            → Models/TransactionOutcome.cs, Utils/TransactionConcept.cs
 *   8.  Provider packages                → Utils/ProviderGuidance.cs
 *   9.  Typical workflows                → Utils/WorkflowSteps.cs
 *   10. ADO.NET vs Dapper vs EF Core      → Utils/TechnologyChoiceGuide.cs
 *   11. Demo                             → Program.cs Main
 */

using System;
using System.Data;
using System.Data.Common;
using IntroductionToAdoNet.Models;
using IntroductionToAdoNet.Services;
using IntroductionToAdoNet.Utils;

namespace IntroductionToAdoNet;

public class Program
{
    /*
     * SECTION 11: DEMONSTRATION — Main orchestrates the chapter demo
     *
     * Runs entirely in memory — no SQL Server required. Shows stack catalog,
     * disconnected DataTable work, DbDataReader over DataTableReader, and
     * prints workflow / technology guidance for reading alongside the code.
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 01. Introduction to ADO.NET ===");
        Console.WriteLine();

        Console.WriteLine("--- ADO.NET stack (BCL + provider packages) ---");
        foreach (string line in AdoNetStackCatalog.DescribeLayers())
        {
            Console.WriteLine($"  {line}");
        }

        Console.WriteLine($"  DbConnection.IsAbstract = {AdoNetStackCatalog.IsProviderAbstract(typeof(DbConnection))}");
        Console.WriteLine();

        Console.WriteLine("--- Connected vs disconnected ---");
        Console.WriteLine($"  Connected:     {ModelComparison.Describe(DataAccessModel.Connected)}");
        Console.WriteLine($"  Disconnected:  {ModelComparison.Describe(DataAccessModel.Disconnected)}");
        Console.WriteLine($"  Default State: {ConnectionConcept.ClosedDefault}");
        Console.WriteLine($"  Open pattern:  {ConnectionConcept.TypicalOpenPattern}");
        Console.WriteLine();

        Console.WriteLine("--- DbCommand execute methods (preview) ---");
        foreach (CommandExecuteMethod method in Enum.GetValues<CommandExecuteMethod>())
        {
            Console.WriteLine($"  {CommandConcept.DescribeExecuteMethod(method)}");
        }

        Console.WriteLine();

        DataTable orders = DisconnectedOrderCache.BuildSampleOrders();
        DataSet snapshot = DisconnectedOrderCache.WrapInDataSet(orders);
        decimal shippedTotal = DisconnectedOrderCache.TotalShippedAmount(orders);

        Console.WriteLine("--- Disconnected DataTable (in-memory) ---");
        Console.WriteLine($"  Rows in Orders: {orders.Rows.Count}");
        Console.WriteLine($"  DataSet name:   {snapshot.DataSetName}, tables: {snapshot.Tables.Count}");
        Console.WriteLine($"  Shipped total:  {shippedTotal:C}");
        Console.WriteLine();

        using (DbDataReader reader = orders.CreateDataReader())
        {
            int rowCount = ReaderDemo.CountRowsForwardOnly(reader);
            Console.WriteLine("--- DbDataReader demo (DataTableReader) ---");
            Console.WriteLine($"  Rows read forward-only: {rowCount}");
        }

        using (DbDataReader amountReader = orders.CreateDataReader())
        {
            decimal allAmounts = ReaderDemo.SumColumn(amountReader, "Amount");
            Console.WriteLine($"  Sum of Amount column:   {allAmounts:C}");
        }

        Console.WriteLine();

        Console.WriteLine("--- DbTransaction (preview) ---");
        Console.WriteLine($"  {TransactionConcept.DescribeOutcome(TransactionOutcome.Commit)}");
        Console.WriteLine($"  {TransactionConcept.DescribeOutcome(TransactionOutcome.Rollback)}");
        Console.WriteLine();

        Console.WriteLine("--- SQL Server provider packages ---");
        foreach (string line in ProviderGuidance.SqlClientComparison())
        {
            Console.WriteLine($"  {line}");
        }

        Console.WriteLine("  Other providers:");
        foreach (string line in ProviderGuidance.OtherProviders())
        {
            Console.WriteLine($"    {line}");
        }

        Console.WriteLine();

        Console.WriteLine("--- Typical connected read workflow ---");
        foreach (string step in WorkflowSteps.ConnectedReadSteps())
        {
            Console.WriteLine($"  {step}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Disconnected snapshot workflow ---");
        foreach (string step in WorkflowSteps.DisconnectedSnapshotSteps())
        {
            Console.WriteLine($"  {step}");
        }

        Console.WriteLine();

        Console.WriteLine("--- ADO.NET vs Dapper vs EF Core ---");
        foreach ((string tech, string bestFor) in TechnologyChoiceGuide.Recommendations())
        {
            Console.WriteLine($"  {tech,-8} → {bestFor}");
        }

        Console.WriteLine();
        Console.WriteLine("Forward refs: ch.02 SqlConnection — ch.08 Async ADO.NET");
    }
}

/*
 * QUICK REFERENCE — INTRODUCTION TO ADO.NET
 *
 * --- Stack layers ---
 *
 *   System.Data.Common     DbConnection, DbCommand, DbDataReader, DbTransaction
 *   System.Data            DataSet, DataTable, DataRow, DataColumn, DataView
 *   Microsoft.Data.SqlClient   SqlConnection, SqlCommand, SqlDataReader (ch.02+ NuGet)
 *
 * --- Connected vs disconnected ---
 *
 *   Connected      Open conn → command → reader/command result → dispose
 *   Disconnected   Adapter.Fill table → close conn → edit → adapter.Update
 *
 * --- DbCommand.Execute* (preview) ---
 *
 *   ExecuteNonQuery()   rows affected
 *   ExecuteScalar()     single value
 *   ExecuteReader()     DbDataReader stream
 *
 * --- Provider choice (SQL Server) ---
 *
 *   USE     Microsoft.Data.SqlClient
 *   AVOID   System.Data.SqlClient (legacy)
 *
 * --- Typical read pipeline ---
 *
 *   using var conn = new SqlConnection(cs);
 *   conn.Open();
 *   using var cmd = new SqlCommand(sql, conn);
 *   cmd.Parameters.AddWithValue("@id", id);
 *   using var reader = cmd.ExecuteReader();
 *   while (reader.Read()) { ... }
 *
 * --- Technology pick ---
 *
 *   ADO.NET   full SQL/control, bulk, legacy sprocs
 *   Dapper    explicit SQL + light POCO mapping
 *   EF Core   models, LINQ, migrations, relationships
 *
 * --- Related chapters ---
 *
 *   02. SqlConnection & Connection Strings
 *   03. SqlCommand & Parameters
 *   04. SqlDataReader
 *   05. DataSet, DataTable & SqlDataAdapter
 *   06. Transactions & Connection Pooling
 *   07. Stored Procedures & Output Parameters
 *   08. Async ADO.NET
 */
