# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/11. Static Files & Request Pipeline`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (M) By default, `UseStaticFiles()` serves content from `wwwroot`. What is actually exposed to the internet if you drop `appsettings.Production.json`, a `.env` file, or source maps into `wwwroot`? How does static file serving differ from serving files from arbitrary project folders?

---

#### Q2. (R) Review this `Program.cs` fragment for a public-facing site. What security issues exist?

```csharp
var app = builder.Build();
app.UseDefaultFiles();
app.UseDirectoryBrowser();
app.UseStaticFiles();
app.MapControllers();
```

---

#### Q3. (P) You host a React SPA with ASP.NET Core as the API and static file host. Client-side routes like `/orders/123` return 404 after refresh. How do you configure static files, default files, and SPA fallback without breaking `/api` routes?

---

#### Q4. (R) API responses are correct but CSS/JS return 404 in Production. Review middleware order — what's wrong?

```csharp
var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();
```

---

#### Q5. (M) A `.wasm` or custom `.dat` file downloads instead of rendering because the browser gets `application/octet-stream`. How does `StaticFileOptions` / `FileExtensionContentTypeProvider` map extensions to MIME types, and when would you use `ServeUnknownFileTypes`?

---

#### Q6. (R) Review this custom endpoint that serves user-uploaded files from disk. What attack vector exists?

```csharp
app.MapGet("/files/{*path}", (string path) =>
{
    var fullPath = Path.Combine("/uploads", path);
    return Results.File(fullPath);
});
```

---

#### Q7. (P) Production static assets (hashed `main.a1b2c3.js`) should cache aggressively; `index.html` must not be cached stale after deploy. How do you set caching headers with `StaticFileOptions.OnPrepareResponse` or response headers at the reverse proxy?
