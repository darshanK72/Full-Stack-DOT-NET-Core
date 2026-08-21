# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/03. SqlCommand & Parameters`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A junior developer ships a product search endpoint. Review the repository method:

```csharp
public IReadOnlyList<ProductDto> SearchByName(SqlConnection connection, string searchTerm)
{
    string sql = """
        SELECT ProductId, Name, UnitPrice, Stock
        FROM dbo.Products
        WHERE IsActive = 1 AND Name LIKE '%" + searchTerm + "%'
        ORDER BY Name;
        """;

    using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
    using SqlDataReader reader = command.ExecuteReader();
    // ... map rows to ProductDto ...
}
```

The method works in QA with `"Mouse"` and `"Keyboard"`. What breaks in production, how would an attacker exploit it, and what is the minimal safe fix?

---

#### Q2. (R) A reporting query returns wrong rows for a price filter. Review:

```csharp
public decimal GetAverageAbovePrice(SqlConnection connection, decimal minPrice)
{
    const string sql = """
        SELECT AVG(UnitPrice)
        FROM dbo.Products
        WHERE IsActive = 1 AND UnitPrice >= @minPrice;
        """;

    using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
    command.Parameters.Add(new SqlParameter("@minPrice", SqlDbType.Int) { Value = minPrice });

    object? scalar = command.ExecuteScalar();
    return scalar is null or DBNull ? 0m : Convert.ToDecimal(scalar);
}
```

`minPrice = 19.99m` is passed from the API. What is wrong, what symptom might QA miss, and how should the parameter be declared?

---

#### Q3. (P) Your API receives product names of varying length (5–80 characters). A teammate uses `AddWithValue` everywhere "because it works":

```csharp
command.Parameters.AddWithValue("@name", userSuppliedName);
// SQL: WHERE Name = @name  — column is NVARCHAR(100)
```

Explain what happens inside SQL Server's plan cache when thousands of distinct name lengths hit this query. Why does the chapter prefer explicit `SqlDbType.NVarChar` with `Size`, and when is `AddWithValue` still acceptable?

---

#### Q4. (P) An ASP.NET Core admin export runs a heavy `SELECT` through ADO.NET. In staging it completes in 45 seconds; in production it intermittently throws:

```
SqlException: Execution Timeout Expired. The timeout period elapsed...
```

The developer sets `command.CommandTimeout = 0` on that command "so exports never fail." What are the risks of that fix, and what production pattern would you use instead?

---

#### Q5. (R) A stored procedure `dbo.usp_ProductCount` exists (see `Program.cs` setup). A teammate copies the preview pattern but changes the call:

```csharp
using SqlCommand command = connection.CreateCommand();
command.CommandText = "EXEC dbo.usp_ProductCount";   // inline EXEC string
command.CommandType = CommandType.Text;              // default — not changed

object? count = command.ExecuteScalar();
```

Meanwhile another path calls the same proc correctly with `CommandType.StoredProcedure` and `CommandText = "dbo.usp_ProductCount"`. What differs at runtime, and what breaks when they add an `@CategoryId` filter parameter to the proc?

---

#### Q6. (R) An inventory sync proc returns the new stock via an output parameter. Review:

```csharp
using SqlCommand command = connection.CreateCommand();
command.CommandText = "dbo.usp_AdjustStock";
command.CommandType = CommandType.StoredProcedure;

command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
command.Parameters.Add(new SqlParameter("@Delta", SqlDbType.Int) { Value = delta });

var output = new SqlParameter("@NewStock", SqlDbType.Int) { Value = 0 };
command.Parameters.Add(output);

command.ExecuteNonQuery();
return (int)output.Value;
```

After deploy, `@NewStock` is always `0` even though the row updates correctly. Diagnose the defect and show the corrected parameter setup.

---

#### Q7. (R) A search builder constructs dynamic `WHERE` clauses at runtime:

```csharp
var sql = new StringBuilder("SELECT ProductId, Name FROM dbo.Products WHERE IsActive = 1");
var parameters = new List<SqlParameter>();

if (!string.IsNullOrEmpty(nameFilter))
{
    sql.Append(" AND Name LIKE @name");
    parameters.Add(new SqlParameter("name", SqlDbType.NVarChar, 100) { Value = $"%{nameFilter}%" });
}
if (minPrice.HasValue)
{
    sql.Append(" AND UnitPrice >= @minPrice");
    parameters.Add(new SqlParameter("@minPrice", SqlDbType.Decimal) { Value = minPrice.Value, Precision = 10, Scale = 2 });
}

using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql.ToString());
foreach (var p in parameters)
    command.Parameters.Add(p);
```

The query runs but `@name` never binds — every search returns unfiltered rows. What naming rule was violated, and how do you prevent this class of bug across dynamic SQL builders?

---

#### Q8. (R) To "reduce allocations," a singleton service caches one `SqlCommand` and reuses it across concurrent HTTP requests on pooled connections:

```csharp
public sealed class ProductLookupService
{
    private readonly SqlCommand _cachedCommand;

    public ProductLookupService(SqlConnection connection)
    {
        _cachedCommand = connection.CreateCommand();
        _cachedCommand.CommandText = "SELECT Name FROM dbo.Products WHERE ProductId = @id";
        _cachedCommand.Parameters.Add(new SqlParameter("@id", SqlDbType.Int));
    }

    public string? GetName(SqlConnection connection, int productId)
    {
        _cachedCommand.Connection = connection;
        _cachedCommand.Parameters["@id"].Value = productId;
        return _cachedCommand.ExecuteScalar() as string;
    }
}
```

Registered as `AddSingleton<ProductLookupService>()`. What fails under load, and what is the correct lifetime pattern for `SqlCommand` in a web app?
