# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/02. Project Structure & Program.cs`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this top-level `Program.cs`. The app compiles but returns 404 for all controller routes and Swagger shows no endpoints. What structural mistake was made?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.Run();
```

---

#### Q2. (R) A developer registers middleware inside `builder.Services` and services inside the `app` pipeline block. What fails and how should `Program.cs` separate the two phases?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.UseHttpsRedirection();
builder.Services.AddControllers();

var app = builder.Build();
app.AddControllers();
app.MapGet("/ping", () => "pong");
app.Run();
```

---

#### Q3. (P) `launchSettings.json` sets `"applicationUrl": "https://localhost:7101;http://localhost:5101"` and `"ASPNETCORE_ENVIRONMENT": "Development"`. What does **not** carry over to production, and what must replace it in Azure App Service or Kubernetes?

---

#### Q4. (R) Review environment-specific configuration loading. Production connects to the dev SQL database after deploy. What is wrong with this setup?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile("appsettings.Development.json");
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
var app = builder.Build();
app.MapControllers();
app.Run();
```

*(Deploy sets `ASPNETCORE_ENVIRONMENT=Production`.)*

---

#### Q5. (M) Explain what happens at `var app = builder.Build()` vs `app.Run()` in the modern hosting model — what gets validated, what is still lazy, and when do misconfigured services first surface?

---

#### Q6. (R) Preview routing middleware order before the Routing chapter: controllers return 404, but minimal `/health` works. Identify the ordering bug.

```csharp
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRouting();
app.MapGet("/health", () => Results.Ok());
app.Run();
```

---

#### Q7. (D) `Program.cs` in a microservice has grown to 200 lines. The team debates `Program.cs` vs `Startup.cs` vs extension methods (`AddInfrastructure()`, `UseApiPipeline()`). What criteria decide the split without hiding middleware order?

---

#### Q8. (R) Review this production-hardening attempt. The app throws at startup in CI but worked on a developer laptop. Prioritize fixes.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IOrderNotifier, SmtpOrderNotifier>();
builder.Services.AddHttpClient<IExternalPricingClient, ExternalPricingClient>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseExceptionHandler("/error");
app.MapControllers();
app.Run();
```

*(CI log: `Unable to resolve service for type 'SmtpOrderNotifier' while attempting to activate 'IOrderNotifier'` — `SmtpOrderNotifier` constructor requires `IOptions<SmtpOptions>` never registered.)*
