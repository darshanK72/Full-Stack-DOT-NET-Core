# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/11. Change Tracking, Async & Transactions`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A catalog endpoint loads products with `AsNoTracking()` for performance, then applies a "flash sale" discount in memory before calling `SaveChangesAsync`. QA passes on one row; production reports prices never change. Review this service method — what is wrong, and how do you fix it without abandoning no-tracking for the read path?

```csharp
public async Task ApplyFlashSaleAsync(int categoryId, decimal discountPct, CancellationToken ct)
{
    var products = await _context.Products
        .AsNoTracking()
        .Where(p => p.CategoryId == categoryId)
        .ToListAsync(ct);

    foreach (var p in products)
    {
        p.UnitPrice *= (1 - discountPct);
    }

    await _context.SaveChangesAsync(ct);
}
```

---

#### Q2. (P) Operations wants to purge inactive products older than two years — potentially tens of thousands of rows. A developer proposes loading each entity with `Find`, calling `Remove`, then one `SaveChangesAsync`. You suggest `ExecuteDeleteAsync` instead. When is `ExecuteDelete` the right tool, what does it skip compared to tracked delete, and what guardrails would you add before running it in production?

---

#### Q3. (R) A stock transfer must update `InventoryDbContext` and write an audit row through `AuditDbContext` (separate DbContext types, same SQL Server database). A teammate starts a transaction on each context independently. Review the orchestration — what breaks atomicity, and how do you coordinate a single commit across both contexts?

```csharp
public async Task TransferWithAuditAsync(StockTransfer transfer, CancellationToken ct)
{
    await using var invTx = await _inventory.Database.BeginTransactionAsync(ct);
    await using var auditTx = await _audit.Database.BeginTransactionAsync(ct);

    await _inventoryTransferService.TransferStockAsync(transfer, ct);
    _audit.AuditEntries.Add(new AuditEntry { Action = "Transfer", Payload = transfer.ToJson() });
    await _audit.SaveChangesAsync(ct);

    await invTx.CommitAsync(ct);
    await auditTx.CommitAsync(ct);
}
```

---

#### Q4. (R) Under load, the inventory API thread pool queues grow and requests time out. Review this controller action — identify async anti-patterns and what you would change for "async all the way" through EF Core.

```csharp
[HttpGet("catalog")]
public IActionResult GetCatalog()
{
    var lines = _trackingService.GetProductCatalogAsync(CancellationToken.None).Result;
    return Ok(lines);
}

[HttpPost("transfer")]
public async Task<IActionResult> Transfer([FromBody] StockTransfer dto)
{
    string outcome = _transactionService.TransferStockAsync(dto).GetAwaiter().GetResult();
    return Ok(outcome);
}
```

---

#### Q5. (R) `Product` now has a SQL Server `rowversion` concurrency token. Two editors save conflicting prices; the second save throws `DbUpdateConcurrencyException`. Review this catch block from the API layer — what is wrong with the recovery strategy, and what should happen before returning a response to the client?

```csharp
catch (DbUpdateConcurrencyException)
{
    await _context.SaveChangesAsync(ct); // retry same pending changes
    return Ok(updatedDto);
}
```

---

#### Q6. (P) A nightly job bulk-updates `StockQuantity` for every warehouse row matching a filter. Compare these two approaches — tracked load + modify + `SaveChangesAsync` vs `ExecuteUpdateAsync` — and state which you would ship for 50k rows, including concurrency and observability trade-offs.

```csharp
// Approach A
var rows = await _context.Products.Where(p => p.WarehouseId == id).ToListAsync(ct);
foreach (var p in rows) p.StockQuantity += adjustment;
await _context.SaveChangesAsync(ct);

// Approach B
await _context.Products
    .Where(p => p.WarehouseId == id)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity + adjustment), ct);
```

---
