# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/12. Hosting, Kestrel & Environments`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (M) Explain the roles of Kestrel, IIS, and a reverse proxy (nginx, YARP, Azure Front Door) in an ASP.NET Core deployment. Who terminates TLS, who runs your application code, and why do you often use more than one layer?

---

#### Q2. (P) An API works locally over HTTPS but generates `http://` links and logs the wrong client IP when deployed behind nginx. Review this configuration — what is missing, and why does middleware order matter?

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
var app = builder.Build();
app.UseAuthentication();
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.MapControllers();
```

---

#### Q3. (M) When would you run Kestrel directly vs host in IIS in-process vs out-of-process? What problems does IIS still solve for Windows deployments even when Kestrel executes the .NET code?

---

#### Q4. (P) How does `ASPNETCORE_ENVIRONMENT` (and `IHostEnvironment.EnvironmentName`) drive behavior at startup and per request? What breaks if Production is mis-set to Development on a public server?

---

#### Q5. (M) Configure Kestrel to listen on specific URLs and ports — `ASPNETCORE_URLS`, `ListenAnyIP`, HTTPS certificate binding. How does this differ from IIS binding and from container `EXPOSE` / Kubernetes `containerPort`?

---

#### Q6. (P) TLS is terminated at nginx; Kestrel receives plain HTTP on port 8080. What must be true in Kestrel, forwarded headers, and cookie/`SameSite` settings so redirects, HSTS, and secure cookies still behave correctly?

---

#### Q7. (P) Implement host-level health checks for Kubernetes liveness vs readiness — `/health/live` vs `/health/ready` with DB dependency. Where do you register checks, and why should the liveness probe stay lightweight?

---

#### Q8. (P) A Docker image for your API fails health checks: the container listens on 8080 but orchestrator probes port 80. Trace configuration from `ASPNETCORE_URLS`, `WebApplication.Urls`, Kestrel `ListenOptions`, and the Dockerfile `EXPOSE` directive.

---

#### Q9. (R) Review this production `Program.cs` hosting setup. What would you fix before go-live?

```csharp
builder.WebHost.UseKestrel(o => o.ListenAnyIP(5000));
builder.Services.AddHealthChecks();
var app = builder.Build();
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
```

*(Assume deploy: Linux container behind nginx terminating TLS, `ASPNETCORE_ENVIRONMENT=Production`.)*

---

#### Q10. (D) Compare Development vs Production hosting configuration: hot reload, detailed errors, HTTPS dev certificates, logging verbosity, and forwarded headers trust. What belongs in `appsettings.Development.json` vs environment variables vs platform config?
