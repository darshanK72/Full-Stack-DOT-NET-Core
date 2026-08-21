using System.Data;

namespace IntroductionToAdoNet.Services;

/*
 * FILE ROLE: In-memory DataTable/DataSet demo for disconnected model (Section 6 preview).
 * SECTIONS IN THIS FILE:
 *   6. DataSet and DataTable — disconnected in-memory model (preview)
 */

/*
 * =========================================================================
 * SECTION 6: DataSet AND DataTable — DISCONNECTED IN-MEMORY MODEL (PREVIEW)
 * =========================================================================
 *
 * DataTable is a tabular cache: columns (schema) + rows (data). DataSet is a
 * container for multiple related DataTables (like a mini in-memory database).
 *
 * Typical disconnected flow (FULL detail → ch.05 DataSet, DataTable & SqlDataAdapter):
 *
 *   using var adapter = new SqlDataAdapter(selectSql, connection);
 *   var table = new DataTable("Orders");
 *   adapter.Fill(table);          // pull data, connection can close
 *   // modify rows: table.Rows.Add / row["Col"] = value
 *   using var builder = new SqlCommandBuilder(adapter);
 *   adapter.Update(table);        // push changes back
 *
 * DataRowState tracks Added, Modified, Deleted, Unchanged — adapter uses this
 * on Update. Here we build and query a table entirely in memory.
 * -------------------------------------------------------------------------
 */
public static class DisconnectedOrderCache
{
    public static DataTable BuildSampleOrders()
    {
        var table = new DataTable("Orders");
        table.Columns.Add("OrderId", typeof(int));
        table.Columns.Add("Customer", typeof(string));
        table.Columns.Add("Amount", typeof(decimal));
        table.Columns.Add("Status", typeof(string));

        table.Rows.Add(1001, "Contoso Ltd", 250.00m, "Shipped");
        table.Rows.Add(1002, "Fabrikam Inc", 89.50m, "Pending");
        table.Rows.Add(1003, "Northwind Traders", 412.75m, "Shipped");

        return table;
    }

    public static decimal TotalShippedAmount(DataTable orders)
    {
        decimal total = 0m;
        foreach (DataRow row in orders.Rows)
        {
            if ((string)row["Status"]! == "Shipped")
            {
                total += (decimal)row["Amount"]!;
            }
        }

        return total;
    }

    public static DataSet WrapInDataSet(DataTable orders)
    {
        var dataSet = new DataSet("SalesSnapshot");
        dataSet.Tables.Add(orders.Copy()); // DataSet owns its own table instance
        return dataSet;
    }
}
