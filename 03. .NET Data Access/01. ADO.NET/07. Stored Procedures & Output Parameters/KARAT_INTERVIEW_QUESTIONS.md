# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/07. Stored Procedures & Output Parameters`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A teammate "fixes" intermittent procedure-not-found errors by copying an SSMS snippet into the repository. Review:

```csharp
public int RunUpdateProductPrice(SqlConnection connection, int productId, decimal newPrice)
{
    using SqlCommand command = new SqlCommand("EXEC dbo.usp_UpdateProductPrice", connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
    command.Parameters.Add(new SqlParameter("@NewPrice", SqlDbType.Decimal)
    {
        Value = newPrice,
        Precision = 10,
        Scale = 2
    });

    return command.ExecuteNonQuery();
}
```

It passes locally against `(localdb)\MSSQLLocalDB` but fails in staging with `Could not find stored procedure 'EXEC dbo.usp_UpdateProductPrice'`. What is wrong, and what is the correct `CommandText` / `CommandType` pairing per this chapter's pattern in `StoredProcedureCommandBasics.cs`?

---

#### Q2. (R) After a DBA widens `usp_GetProductName` to `NVARCHAR(200)`, production logs show truncated product names and occasional `SqlException` about string output size. Review the client:

```csharp
public string? GetProductName(SqlConnection connection, int productId)
{
    using SqlCommand command = new SqlCommand("dbo.usp_GetProductName", connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

    var nameParameter = new SqlParameter("@ProductName", SqlDbType.NVarChar)
    {
        Direction = ParameterDirection.Output
        // Size not set — "works" for short names in QA
    };
    command.Parameters.Add(nameParameter);

    command.ExecuteNonQuery();
    return nameParameter.Value == DBNull.Value ? null : (string)nameParameter.Value;
}
```

What fails at runtime for string `OUTPUT` parameters, and how must `Size` relate to the T-SQL declaration?

---

#### Q3. (M) `dbo.usp_SearchProducts` returns a result set **and** sets `@TotalCount INT OUTPUT`. Two teammates ship different callers:

```csharp
// Path A — "we only need the count in the OUTPUT param"
command.ExecuteNonQuery();
int total = (int)totalCountParam.Value;

// Path B — "we need the rows"
using SqlDataReader reader = command.ExecuteReader();
while (reader.Read()) { /* map ProductDto */ }
int total = (int)totalCountParam.Value;
```

Path A reports `@TotalCount` correctly but never surfaces product rows. Path B throws or returns `0` for `@TotalCount` depending on timing. Explain when to use `ExecuteNonQuery`, `ExecuteReader`, or `ExecuteScalar` for procedures that mix result sets and output parameters, and when `@TotalCount.Value` is valid to read.

---

#### Q4. (R) A dashboard stored procedure returns two SELECT result sets plus an `@ErrorMessage NVARCHAR(500) OUTPUT`. Review:

```csharp
using SqlDataReader reader = command.ExecuteReader();

var summaryRows = new List<SummaryRow>();
while (reader.Read())
    summaryRows.Add(MapSummary(reader));

int errCode = (int)errorCodeParam.Value;  // read OUTPUT early

while (reader.NextResult())
{
    while (reader.Read())
        detailRows.Add(MapDetail(reader));
}

string? message = errorCodeParam.Value == DBNull.Value ? null : (string)errorMessageParam.Value;
```

QA passes when the proc succeeds; staging intermittently shows `InvalidCastException` or stale `@ErrorMessage`. Diagnose the ordering bug and show the correct read sequence for multiple result sets **and** output parameters.

---

#### Q5. (R) An admin API lets power users pick which reporting stored procedure to run. Review:

```csharp
public IReadOnlyList<ReportRow> RunReport(string procedureName, int tenantId)
{
    using SqlConnection connection = new SqlConnection(_connectionString);
    using SqlCommand command = new SqlCommand(procedureName, connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(new SqlParameter("@TenantId", SqlDbType.Int) { Value = tenantId });

    using SqlDataReader reader = command.ExecuteReader();
    return MapRows(reader);
}
```

`procedureName` comes from a query string. Parameters are bound safely — why is this still a critical vulnerability, and what production pattern replaces dynamic `CommandText` for procedure selection?

---

#### Q6. (D) Your team treats stored procedures as a versioned contract between the DBA and .NET services (`usp_InsertProduct` with `@NewProductId OUTPUT` today). Next sprint the DBA adds a required `@CreatedBy NVARCHAR(128)` with no default, and renames `@NewProductId` to `@ProductIdOut` in one environment before all app servers redeploy. What breaks at runtime, how do rolling deploys and blue/green make this worse, and what contract practices (naming, defaults, deployment order) would you enforce?

---

#### Q7. (R) A "try insert" path mirrors `ReturnValueDemo.TryGetProductName` but the author skips null checks "because OUTPUT is always set." Review:

```csharp
public (int NewId, int Status) TryInsertProduct(string name, decimal price)
{
    using SqlCommand command = new SqlCommand("dbo.usp_TryInsertProduct", connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar, 100) { Value = name });
    command.Parameters.Add(new SqlParameter("@UnitPrice", SqlDbType.Decimal) { Value = price, Precision = 10, Scale = 2 });

    var newIdParam = new SqlParameter("@NewProductId", SqlDbType.Int) { Direction = ParameterDirection.Output };
    var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
    command.Parameters.Add(newIdParam);
    command.Parameters.Add(returnParam);

    command.ExecuteNonQuery();

    int newId = (int)newIdParam.Value;
    int status = (int)returnParam.Value;
    return (newId, status);
}
```

When duplicate names violate a unique constraint, the proc sets `@NewProductId = NULL`, `RETURN 2`, and does not insert. What exception or wrong data reaches the caller, and how should the method handle `DBNull.Value` vs `RETURN` status (see `OutputParameterDemo.InsertProduct` and `ReturnValueDemo.TryGetProductName`)?

---

#### Q8. (P) An ASP.NET Core endpoint wraps `usp_InsertProduct` with output identity. Review the async repository under load:

```csharp
public int InsertProductAsync(string productName, decimal unitPrice)
{
    using SqlConnection connection = new SqlConnection(_connectionString);
    using SqlCommand command = new SqlCommand("dbo.usp_InsertProduct", connection);
    command.CommandType = CommandType.StoredProcedure;
    // ... input + @NewProductId OUTPUT params ...

    connection.Open();
    command.ExecuteNonQueryAsync().GetAwaiter().GetResult();

    return (int)newIdParameter.Value;
}
```

Registered as scoped and called from `Task`-returning controllers. What thread-pool and correctness issues appear in production, and how should this method be rewritten (including when to read `Output`/`ReturnValue` and how to pass cancellation)?
