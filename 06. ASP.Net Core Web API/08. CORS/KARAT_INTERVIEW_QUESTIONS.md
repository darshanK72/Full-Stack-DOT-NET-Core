# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/08. CORS`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this CORS policy for a SPA that sends JWT cookies on cross-origin requests. Browser console shows CORS error; policy appears permissive.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaPolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();
app.UseCors("SpaPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Frontend: `https://app.example.com` calling API at `https://api.example.com` with `credentials: 'include'`.

---

#### Q2. (R) Review preflight failures on a custom header. Simple GET works from Postman; browser POST with `X-Request-Id` and `Authorization` fails before reaching the controller.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.WithOrigins("https://portal.example.com")
              .AllowAnyMethod());
});

app.UseRouting();
app.UseCors("Default");
app.UseAuthentication();
app.MapControllers();
```

No explicit `AllowHeaders`; client sends `Content-Type: application/json`, `Authorization: Bearer ...`, `X-Request-Id: abc`.

---

#### Q3. (M) A JWT-protected API returns 401 on cross-origin requests; developers say "CORS is broken" because the response lacks `Access-Control-Allow-Origin`. Walk through middleware order — `UseCors`, `UseAuthentication`, `UseAuthorization`, endpoint — and explain when CORS headers appear on error responses vs when auth fails first.

---

#### Q4. (P) Production requires an explicit origin allowlist for three frontends and a staging tenant. Review this configuration approach vs `AllowAnyOrigin()` — environment-specific policies, secrets, and wildcard subdomain pitfalls.

```csharp
// appsettings.Production.json
"Cors": {
  "AllowedOrigins": [
    "https://app.example.com",
    "https://admin.example.com",
    "https://*.staging.example.com"
  ]
}
```

ASP.NET Core `WithOrigins` does not support `*.staging.example.com` wildcards natively.

---

#### Q5. (R) Review exposed headers for a file-download API. JavaScript client cannot read `Content-Disposition` or custom `X-Total-Count` from XHR despite successful 200.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Api", policy =>
        policy.WithOrigins("https://app.example.com")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
```

Response includes `Content-Disposition: attachment; filename="report.pdf"` and `X-Total-Count: 1500`; no `WithExposedHeaders` configured.

---

#### Q6. (P) Review middleware placement in `Program.cs`. CORS works for anonymous endpoints but authenticated routes still fail preflight in some environments.

```csharp
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("Default");
app.MapControllers();
```

Policy registered with `WithOrigins("https://app.example.com")` and `AllowAnyMethod()`.

---

#### Q7. (R) Review this global CORS middleware placed after endpoints. Local development works when hitting Kestrel directly; fails behind IIS reverse proxy with custom domain.

```csharp
var app = builder.Build();

app.MapControllers();
app.UseCors("DevPolicy");  // DevPolicy: AllowAnyOrigin, no credentials

app.Run();
```

IIS site binds `https://api.internal.corp`; SPA at `https://spa.internal.corp`.

---

#### Q8. (D) API team owns ASP.NET Core CORS; platform team adds Azure API Management in front with its own CORS policy. Browser sees duplicate or conflicting `Access-Control-Allow-Origin` headers. Who should own CORS in production, and how do you avoid double-application?
