# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/08. Async ADO.NET`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A Minimal API endpoint "fixes" async by blocking on the repository. Review:

```csharp
app.MapGet("/products/count", () =>
{
    string cs = builder.Configuration.GetConnectionString("AdoNetTutorial")!;
    int count = ProductRepository.GetProductCountAsync(cs).Result;
    return Results.Ok(count);
});
```

`ProductRepository.GetProductCountAsync` uses `OpenAsync` and `ExecuteScalarAsync` with `ConfigureAwait(false)` (see **Repositories/ProductRepository.cs**). Under load in ASP.NET Core, what breaks, and how do you fix it end-to-end?

---

#### Q2. (R) A report endpoint streams products to the client. A teammate copied the **ProductStream** preview pattern but left the connection open on the caller:

```csharp
app.MapGet("/products/stream", async (HttpContext ctx) =>
{
    await using SqlConnection conn = new SqlConnection(cs);
    await conn.OpenAsync();

    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    await foreach (ProductRow row in ProductStream.ReadProductsAsync(reader))
    {
        await ctx.Response.WriteAsJsonAsync(row);
    }
});
```

`ProductStream.ReadProductsAsync` passes a `CancellationToken` with `[EnumeratorCancellation]`. The endpoint does not pass `ctx.RequestAborted`, and a slow client disconnects mid-stream. Diagnose what keeps running after the client is gone and what you would change (token wiring + disposal).

---

#### Q3. (R) With `MultipleActiveResultSets=True` on the connection string, a service tries to update stock while a reader is still open:

```csharp
public async Task RefreshCatalogAsync(string connectionString)
{
    await using SqlConnection conn = new SqlConnection(connectionString); // MARS enabled in cs
    await conn.OpenAsync();

    await using SqlCommand select = conn.CreateCommand();
    select.CommandText = "SELECT ProductId, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await select.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        int id = reader.GetInt32(0);
        int stock = reader.GetInt32(1);

        if (stock == 0)
        {
            await using SqlCommand update = conn.CreateCommand();
            update.CommandText = "UPDATE dbo.Products SET Stock = 10 WHERE ProductId = @Id;";
            update.Parameters.Add(new SqlParameter("@Id", id));
            await update.ExecuteNonQueryAsync(); // second active command on same connection
        }
    }
}
```

What can go wrong with MARS + interleaved reader and command lifetime, and what pattern would you use instead in production?

---

#### Q4. (P) Your shared data-access library (same style as **ProductRepository**) is consumed by ASP.NET Core APIs and a WinForms desktop app. A junior adds `ConfigureAwait(false)` to every await in the **API controllers** "because the ADO.NET chapter says library code should." Another dev removes `ConfigureAwait(false)` from **ProductRepository** "because ASP.NET doesn't need it." Who is right in each case, and what is the production rule?

---

#### Q5. (R) A legacy sync helper was merged into an async export job:

```csharp
public async Task ExportProductsAsync(string connectionString, Stream output)
{
    await using SqlConnection conn = new SqlConnection(connectionString);
    conn.Open(); // sync open — "it's just one line"

    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    while (reader.Read()) // sync Read inside async method
    {
        decimal price = reader.GetDecimal(2);
        await output.WriteAsync(Encoding.UTF8.GetBytes($"{price}\n"));
    }
}
```

List the defects (sync/async mixing, thread blocking, reader semantics) and prioritize fixes.

---

#### Q6. (R) A dashboard action needs average price and count. The developer avoids "async all the way" for the scalar:

```csharp
app.MapGet("/dashboard", async (IConfiguration config) =>
{
    string cs = config.GetConnectionString("AdoNetTutorial")!;

    Task<int> countTask = ProductRepository.GetProductCountAsync(cs);

    await using SqlConnection conn = new SqlConnection(cs);
    await conn.OpenAsync();
    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT AVG(UnitPrice) FROM dbo.Products;";
    decimal avg = Convert.ToDecimal(cmd.ExecuteScalar()); // sync scalar on open async connection

    int count = await countTask;
    return Results.Ok(new { avg, count });
});
```

Two connections hit SQL Server concurrently, but throughput still collapses under load. Explain the thread-pool impact of `ExecuteScalar()` here and how you would rewrite this endpoint.

---

#### Q7. (D) You must expose a large product export from ADO.NET without loading a `List<ProductRow>`. Options: (A) buffer with `ExecuteReaderAsync` + `List<T>`, (B) `IAsyncEnumerable<ProductRow>` over `ReadAsync` like **Services/ProductStream.cs**, or (C) raw `SqlDataReader` returned from the repository. Compare memory, cancellation, connection lifetime, and ASP.NET response shaping — which do you ship and why?
