# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/10. Raw SQL & Stored Procedures`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A junior developer adds a product search endpoint by adapting `GetProductsByNamePrefix` from **RawSqlQueryRepository.cs**. Review the repository method:

```csharp
public List<Product> SearchByName(string userInput)
{
    var sql = $"""
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName LIKE '%{userInput}%'
        ORDER BY ProductId
        """;

    return _context.Products
        .FromSqlRaw(sql)
        .AsNoTracking()
        .ToList();
}
```

They argue it is safe because they used `FromSqlRaw`, not ADO.NET string building. What is wrong, and how do you fix it?

---

#### Q2. (M) A teammate extends **StoredProcedureRepository.GetByMinStock** to filter expensive items in LINQ after the proc returns:

```csharp
public List<Product> GetExpensiveInStock(int minStock, decimal minPrice)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetProductsByMinStock @MinStock={0}", minStock)
        .Where(p => p.UnitPrice >= minPrice)
        .OrderBy(p => p.UnitPrice)
        .AsNoTracking()
        .ToList();
}
```

Tests pass on LocalDB with small data. What happens at the SQL layer, and what would you change for production?

---

#### Q3. (R) A legacy reporting stored procedure returns a narrow shape — not full `Product` rows. A developer maps it to `DbSet<Product>` anyway:

```csharp
public List<Product> GetLowStockViaProc(int threshold)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetLowStockReport @Threshold={0}", threshold)
        .AsNoTracking()
        .ToList();
}
```

`usp_GetLowStockReport` returns `ProductId`, `ProductName`, `StockQuantity`, and a computed `Status` column (see **LowStockRow.cs**). Review this mapping choice. What breaks at runtime or in maintenance, and what EF Core API fits this shape?

---

#### Q4. (R) Two developers argue about parameter safety. Compare these snippets from a refactored repository:

```csharp
// Developer A
public List<Product> ByPrefix(string prefix)
{
    return _context.Products
        .FromSqlRaw($"""
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductName LIKE '{prefix}%'
            """)
        .ToList();
}

// Developer B
public List<Product> ByPrefix(string prefix)
{
    return _context.Products
        .FromSqlInterpolated($"""
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductName LIKE {prefix + "%"}
            """)
        .ToList();
}
```

Developer A says both use `$"""` so both are parameterized. Who is correct, and why does `FromSqlRaw` vs `FromSqlInterpolated` matter here?

---

#### Q5. (D) Your team needs inventory aggregate reports (**InventorySummary.cs**) in three services. One developer registers a keyless entity; another uses EF Core 8 `Database.SqlQuery<T>` only at call sites (as in **SqlQueryRepository.cs**). Compare:

```csharp
// Option A — OnModelCreating
modelBuilder.Entity<InventorySummary>().HasNoKey().ToView(null);
public DbSet<InventorySummary> InventorySummaries => Set<InventorySummary>();

// Option B — no DbSet registration (current chapter approach)
_context.Database.SqlQueryRaw<InventorySummary>("SELECT COUNT(*) AS ProductCount, ...").Single();
```

When would you pick keyless `DbSet<T>` vs `SqlQuery<T>`, and what pitfalls apply to each in a production API?

---

#### Q6. (P) **RawSqlQueryRepository.GetActiveProductsAbovePrice** appends LINQ after `FromSqlRaw`:

```csharp
return _context.Products
    .FromSqlRaw(
        """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL
          AND UnitPrice > {0}
        """,
        minimumPrice)
    .OrderBy(p => p.ProductName)
    .AsNoTracking()
    .ToList();
```

Explain how EF Core composes this query, when post-`FromSqlRaw` LINQ is safe vs when it pulls rows client-side, and one production scenario where you would **not** compose further LINQ on raw SQL.

---
