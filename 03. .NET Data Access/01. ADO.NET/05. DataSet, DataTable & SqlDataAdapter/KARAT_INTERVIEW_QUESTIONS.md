# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/05. DataSet, DataTable & SqlDataAdapter`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A legacy ASP.NET Core API endpoint loads an entire `production.orders` table into a `DataSet`, lets callers filter in memory, and returns JSON. Review the service method:

```csharp
public async Task<IActionResult> SearchOrders(string? statusFilter)
{
    using var connection = new SqlConnection(_connString);
    using var adapter = new SqlDataAdapter("SELECT * FROM production.orders", connection);
    var dataSet = new DataSet("OrdersSnapshot");
    adapter.Fill(dataSet); // ~400k rows, 40+ columns each

    DataTable orders = dataSet.Tables[0]!;
    if (!string.IsNullOrEmpty(statusFilter))
    {
        foreach (DataRow row in orders.Rows)
        {
            if (row["Status"]?.ToString() != statusFilter)
                row.Delete();
        }
        orders.AcceptChanges(); // "clean up" deleted rows before serialize
    }

    return Ok(orders); // serializes remaining rows to JSON
}
```

What breaks under load, and what would you change first?

---

#### Q2. (M) After `adapter.Update(table)` throws because two rows failed with a SQL error, a developer resets UI state so users can retry. They call `table.AcceptChanges()` on the whole table "to clear the error flags." Before that, one row was `Modified`, one was `Added`, and one was `Deleted`. What is wrong with calling `AcceptChanges()` here, and what should happen instead?

---

#### Q3. (R) Two users edit the same category row offline in a WinForms grid backed by `SqlDataAdapter`. User A changes `category_name` to "Mountain Bikes"; User B changes it to "MTB" and saves first. User A clicks Save. Review the update setup:

```csharp
const string selectSql = """
    SELECT category_id, category_name
    FROM production.categories
    WHERE category_id = @id
    """;
using var adapter = new SqlDataAdapter(selectSql, connection);
adapter.SelectCommand!.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = 1 });
using var builder = new SqlCommandBuilder(adapter);

// Fill → user edits row → Update
adapter.Update(categoriesTable);
categoriesTable.AcceptChanges();
```

`SqlCommandBuilder` generated an `UPDATE` with only `category_name` in the `SET` clause and `category_id` in the `WHERE` clause — no rowversion/timestamp check. What failure mode does User A hit, and how do you fix optimistic concurrency for this disconnected pattern?

---

#### Q4. (D) A microservices team proposes sharing a `DataSet` between an order API, a reporting worker, and a mobile sync service "so everyone has the same offline cache." When would you push back and recommend `SqlDataReader`, Dapper, or EF Core instead? Name at least two concrete reasons tied to deployment and API design.

---

#### Q5. (P) A nightly job must insert 2 million staging rows from a CSV import. One developer uses `SqlDataAdapter.Update` on a `DataTable` with 2M `Added` rows; another uses `SqlBulkCopy.WriteToServer(dataTable)` inside a transaction. Compare throughput, change tracking, and failure behavior. Which path matches this ETL job, and what from chapter 06 still applies?

---

#### Q6. (R) Production deploys a DB migration that renames column `UnitPrice` → `ListPrice`. The API still `Fill`s into a cached `DataTable` schema created at startup (columns defined in code). After deploy, users edit prices in the grid and call save. Review the save path:

```csharp
// Startup: table schema built in memory with columns ProductId, ProductName, UnitPrice
adapter.Fill(products); // DB now returns ListPrice — Fill adds a second price column

foreach (DataRow row in products.Rows)
{
    if (row["UnitPrice"] is DBNull && row["ListPrice"] is not DBNull)
        row["UnitPrice"] = row["ListPrice"]; // "migrate" in memory
}

adapter.Update(products);
products.AcceptChanges();
```

What RowState and persistence bugs appear after schema drift, and how do you prevent silent data loss?

---

#### Q7. (R) An internal admin API exposes search over an in-memory `DataTable` filled from `SqlDataAdapter`. The controller builds a filter from query string input:

```csharp
public IActionResult FindProducts(string nameContains)
{
    DataTable products = _catalogCache.GetProductsTable(); // shared singleton cache
    string filter = $"ProductName LIKE '%{nameContains}%'";
    DataRow[] matches = products.Select(filter);
    return Ok(matches.Select(r => r["ProductName"]));
}
```

Diagnose security and correctness issues (expression syntax, caching, concurrency). What pattern replaces string-built `DataTable.Select` filters?

---
