# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/04. SqlDataReader`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A production export job intermittently hangs with "Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool." Review this repository method copied from an internal tool:

```csharp
public IEnumerable<ProductRow> StreamProducts(string connectionString)
{
    var connection = new SqlConnection(connectionString);
    connection.Open();

    var command = new SqlCommand(
        "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products;",
        connection);

    SqlDataReader reader = command.ExecuteReader();

    while (reader.Read())
    {
        yield return new ProductRow
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            UnitPrice = reader.GetDecimal(2),
            StockQuantity = reader.GetInt32(3),
            DiscontinuedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
        };
    }
}
```

The caller does `foreach (var p in repo.StreamProducts(cs)) { await WriteCsvLine(p); }`. What breaks under load, and how do you fix it without loading the entire table into a `List<T>` first?

---

#### Q2. (R) Two teammates map the same `Products` query differently. Review both snippets:

```csharp
// Team A — BasicReadLoopDemo style
while (reader.Read())
{
    int id = reader.GetInt32(0);
    string name = reader.GetString(1);
    decimal price = reader.GetDecimal(2);
}

// Team B — ProductMapper style (called inside every Read())
while (reader.Read())
{
    products.Add(ProductMapper.MapFromReader(reader));
}

// ProductMapper.MapFromReader (current chapter code)
public static ProductRow MapFromReader(SqlDataReader reader)
{
    int ordId = reader.GetOrdinal("ProductId");
    int ordName = reader.GetOrdinal("ProductName");
    // … GetOrdinal for each column on every row …
}
```

A DBA adds `ModifiedAt` as the first column in `SELECT * FROM dbo.Products` for auditing. Team A's export still "works" but prices look like integers. Team B's page is slower on 200k rows. Diagnose both failure modes and describe the mapping approach you would standardize on.

---

#### Q3. (R) A nightly job reads discontinued products and crashes on row 847 with `SqlNullValueException`. Review the mapping helper:

```csharp
while (reader.Read())
{
    var row = new ProductRow
    {
        ProductId = (int)reader["ProductId"],
        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
        StockQuantity = reader.GetField<int>("StockQuantity"), // extension: (T)reader[name]
        DiscontinuedDate = (DateTime?)reader["DiscontinuedDate"]
    };
    rows.Add(row);
}
```

What is wrong with each nullable/value-type read, and how does this differ from the `IsDBNull` pattern in **NullHandlingDemo.cs**?

---

#### Q4. (M) A stored procedure returns three result sets: (1) product rows, (2) aggregate counts, (3) audit metadata. A junior developer adapts **MultipleResultSetsDemo.cs** like this:

```csharp
using var reader = await cmd.ExecuteReaderAsync();
var products = new List<ProductRow>();
int totalCount = 0;

do
{
    while (await reader.ReadAsync())
    {
        if (reader.FieldCount > 1)
            products.Add(MapProduct(reader));
        else if (reader.FieldCount == 1)
            totalCount = reader.GetInt32(0);
    }
}
while (await reader.ReadAsync()); // advance to next result set

return new ProductPage(products, totalCount);
```

In QA, the API hangs or returns `totalCount = 0` with only the first product row, and memory grows when the procedure returns 50k products. What did they misunderstand about `NextResult()`, and what is the correct consumption pattern?

---

#### Q5. (P) A data-access layer exposes streaming reads to upper layers:

```csharp
public SqlDataReader OpenProductStream()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    var cmd = new SqlCommand("SELECT * FROM dbo.Products ORDER BY ProductId", connection);
    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
}
```

The API controller does:

```csharp
public IActionResult Export()
{
    using var reader = _repo.OpenProductStream();
    // … write rows to response …
    return File(stream, "text/csv");
}
```

Under what conditions does this pattern work, and what connection-lifetime mistakes still cause "There is already an open DataReader associated with this Connection" or leaked connections in production?

---

#### Q6. (P) An ops team exports `DocumentBody varbinary(max)` for 10,000 rows (~5 MB each). A developer loads full rows like **BasicReadLoopDemo** and the app hits `OutOfMemoryException`. They switch to:

```csharp
using var reader = cmd.ExecuteReader(
    CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection);

while (reader.Read())
{
    int id = reader.GetInt32(0);
    string title = reader.GetString(1);
    byte[] body = (byte[])reader[2]; // DocumentBody
    await WriteToBlobStorage(id, body);
}
```

Explain how `CommandBehavior.SequentialAccess` is meant to work, what is still wrong with `(byte[])reader[2]`, and sketch the production-safe read pattern for large LOBs.

---

#### Q7. (D) `ProductMapper.MapFromReader` resolves columns by name (`GetOrdinal("ProductId")`, etc.) and is reused across three queries: a list screen, a search SP that aliases `ProductId AS Id`, and a reporting view that exposes `SKU` instead of `ProductName`. The chapter positions this mapper as the pattern "Dapper and EF Core automate." What fails in production as queries diverge, and how would you refactor for maintainability without jumping straight to EF Core?

---
