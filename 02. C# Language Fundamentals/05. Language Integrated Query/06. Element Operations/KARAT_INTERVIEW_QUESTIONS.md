# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/06. Element Operations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A nightly billing job crashes after month-end write-offs. Review the service method — what throws, and how would you fix it for the "maybe no matches" case?

```csharp
public Invoice GetHighestOverdueInvoice(IEnumerable<Invoice> invoices)
{
    return invoices
        .Where(inv => inv.Status == InvoiceStatus.Overdue)
        .OrderByDescending(inv => inv.Amount)
        .First(); // "there is always an overdue bill"
}

// Called after write-offs when the overdue filter can return zero rows:
var top = GetHighestOverdueInvoice(clinicInvoices);
Console.WriteLine($"{top.Id} — {top.Amount:C}");
```

---

#### Q2. (R) A dashboard endpoint uses `FirstOrDefault` but still mis-reports balances when no high-value invoice exists. Review the handler:

```csharp
public decimal GetLargestBillAmount(IEnumerable<Invoice> invoices)
{
    Invoice result = invoices.FirstOrDefault(inv => inv.Amount > 5000m);
    return result.Amount; // logged to metrics as "largest bill today"
}
```

What breaks at runtime, and what pattern from this chapter avoids the silent bad metric?

---

#### Q3. (R) A data-migration bug left two `Pending` invoices for the same patient. Review the account-reconciliation code:

```csharp
public Invoice GetOpenInvoiceForPatient(IEnumerable<Invoice> invoices, string patientId)
{
    return invoices.Single(inv =>
        inv.PatientId == patientId && inv.Status == InvoiceStatus.Pending);
}

// Reconciliation job after migration:
var open = GetOpenInvoiceForPatient(allInvoices, "P-004");
ProcessPayment(open);
```

What exception appears, why is `Single` the wrong operator here, and what would you change?

---

#### Q4. (R) A developer replaces `Single` with `SingleOrDefault` expecting duplicate rows to "just pick one." Review:

```csharp
public Invoice? FindPrimaryAdminInvoice(IEnumerable<Invoice> invoices)
{
    return invoices.SingleOrDefault(inv => inv.Status == InvoiceStatus.Overdue);
}

// Seed data has three Overdue rows (same as the tutorial registry):
var adminRow = FindPrimaryAdminInvoice(invoices);
if (adminRow is null)
    return; // never reached — job still crashes
```

What still throws, and how do you enforce uniqueness before picking one element?

---

#### Q5. (R) A repository exposes deferred LINQ; the service reads two positions and logs slow queries. Review:

```csharp
public class InvoiceRepository
{
    private readonly AppDbContext _db;

    public IQueryable<Invoice> GetOverdueQuery() =>
        _db.Invoices.Where(i => i.Status == InvoiceStatus.Overdue);
}

public void PrintTopTwoOverdue(InvoiceRepository repo)
{
    var query = repo.GetOverdueQuery().OrderByDescending(i => i.DaysOverdue);

    var first = query.ElementAt(0);
    var second = query.ElementAt(1);

    Console.WriteLine($"{first.Id}, {second.Id}");
}
```

How many database round trips occur, why does `ElementAt` cause it, and how would you fix this?

---

#### Q6. (D) Your team debates three approaches for "get the pending invoice for this patient, or nothing" in an EF Core API. Which do you recommend and why?

```csharp
// A
var invoice = await _db.Invoices
    .Where(i => i.PatientId == id && i.Status == InvoiceStatus.Pending)
    .FirstOrDefaultAsync();

// B
var invoice = await _db.Invoices
    .Where(i => i.PatientId == id && i.Status == InvoiceStatus.Pending)
    .SingleOrDefaultAsync();

// C
var invoice = await _db.Invoices
    .Where(i => i.PatientId == id && i.Status == InvoiceStatus.Pending)
    .Take(2)
    .ToListAsync();
// then branch on Count == 0 / 1 / 2+
```

Assume business rules say there **should** be at most one pending invoice per patient, but duplicates are possible from bad imports.
