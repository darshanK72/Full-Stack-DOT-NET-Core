# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/10. Exception Handling`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (P) How would you implement centralized, uniform exception handling in an ASP.NET Core Web API — including status code mapping, `ProblemDetails` response shape, and different behavior in Development vs Production?

---

#### Q2. (R) Review this exception-handling code from a service layer. What would you change and why?

```csharp
try
{
    await _paymentGateway.ChargeAsync(orderId, amount);
}
catch (Exception ex)
{
    _logger.LogError(ex.Message);
    throw ex;
}
```

---

#### Q3. (M) In a catch block, what is the difference between `throw;` and `throw ex;`, and why does the choice affect production diagnostics in ASP.NET Core?

---

#### Q4. (P) How should structured logging use the exception object (`LogError(ex, "...")`) vs logging only `ex.Message`? What do you lose in Application Insights or Serilog when you log message-only?

---

#### Q5. (P) Register and implement `IExceptionHandler` (.NET 8+) for a global JSON error envelope. Where does it sit relative to `UseExceptionHandler`, and what does `TryHandleAsync` returning `true` vs `false` mean?

---

#### Q6. (M) Your API must return RFC 7807 `ProblemDetails` for all client-facing errors. Review this controller catch — what's wrong with the response contract?

```csharp
catch (ValidationException ex)
{
    return StatusCode(500, new { error = ex.Message, fields = ex.Errors });
}
catch (NotFoundException ex)
{
    return NotFound(ex.Message);
}
```

---

#### Q7. (D) Compare Development vs Production exception behavior: `DeveloperExceptionPage`, detailed `ProblemDetails`, stack traces in JSON, and what must never leak to external clients. How do you configure both without `#if DEBUG` in controllers?

---

#### Q8. (M) An MVC action throws inside an action filter; another failure occurs in custom middleware before routing. Which handlers run — exception filter, `IExceptionHandler`, `UseExceptionHandler` fallback — and in what order?

---

#### Q9. (P) Design a status-code mapping table for domain exceptions (`ValidationException` → 400, `NotFoundException` → 404, conflict → 409, unauthorized business rule → 403). Where should mapping live so controllers stay thin and OpenAPI stays accurate?
