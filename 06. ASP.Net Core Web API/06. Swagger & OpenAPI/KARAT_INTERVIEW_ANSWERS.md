# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/06. Swagger & OpenAPI`

---

#### Q1. (R) Review production `Program.cs` — Swagger UI exposed in all environments.

**Answer:** Swagger UI on a public API exposes every endpoint, schema, and parameter shape to attackers — it must be disabled or restricted in production; example JWT tokens in config must never ship to production repos.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `UseSwagger` / `UseSwaggerUI` unconditional | Attack surface enumeration; internal routes visible |
| Secrets | Dev tokens in tracked config | Credential leakage if copied to prod |
| Auth bypass | Documented endpoints aid targeted probing | Faster exploitation of misconfigured auth |

**Fix (priority order):**

1. Gate Swagger behind environment: `if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }`.
2. Production alternative: internal VPN/docs portal, or IP-restricted `/swagger` via reverse proxy.
3. Remove real tokens from repo; use Swagger OAuth2 PKCE or manual bearer entry with short-lived dev tokens only locally.
4. Ensure `[Authorize]` on sensitive controllers — Swagger documents existence but auth must enforce access.

**Production takeaway:** OpenAPI docs are reconnaissance aids — treat public Swagger UI as a production security finding.

---

#### Q2. (R) Review schema generation — duplicate `ProductDto` class names across namespaces.

**Answer:** Swashbuckle generates schema IDs from short type names by default — two `ProductDto` types collide into one schema, merging or overwriting properties and breaking generated TypeScript/C# clients.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| OpenAPI | Schema ID collision on same short name | Wrong properties in client models |
| Design | Internal DTO exposed alongside public contract | Cost/supplier fields leak into public schema |
| Client gen | NSwag/Swagger Codegen compile failures | CI breaks on SDK regeneration |

**Fix (priority order):**

1. Assign unique schema IDs: `options.CustomSchemaIds(type => type.FullName?.Replace("+", "."))`.
2. Expose only public contract DTOs on controllers — keep internal types off API surface.
3. Use `[SwaggerSchema]` or separate assemblies for public vs internal models.

**Production takeaway:** Schema collisions are silent until client generation — `CustomSchemaIds` is standard on non-trivial APIs.

---

#### Q3. (P) Align OpenAPI nullability with `bool?` tri-state and optional `DateOnly?` on PATCH DTOs.

**Answer:** ASP.NET Core 8 + Swashbuckle infer nullability from C# nullable reference types and nullable value types — PATCH DTOs with `bool?` must appear as `nullable: true` in OpenAPI so clients know omission is valid; otherwise generators mark fields required and send `false` instead of omitting.

- Enable `<Nullable>enable</Nullable>` — Swashbuckle uses nullability context for reference types.
- For value types, `bool?` and `DateOnly?` automatically emit nullable schemas in OpenAPI 3.
- Use `[SwaggerSchema(Nullable = true)]` or FluentValidation + schema filters when inference is wrong.
- Document PATCH semantics in description: "Omit property to leave unchanged; null and false differ for booleans."
- NSwag: enable `AllowNullableBodyParameters` and verify generated PATCH clients use optional properties.
- Test: export `swagger.json` and assert `"emailAlerts": { "type": "boolean", "nullable": true }`.

**Production takeaway:** OpenAPI nullability must match binding semantics — wrong required flags cause clients to destroy tri-state PATCH behavior.

---

#### Q4. (P) JWT "Authorize" in Swagger UI without leaking production secrets.

**Answer:** Register an OpenAPI `Bearer` security scheme and apply a global security requirement in `AddSwaggerGen` — developers paste short-lived dev tokens at runtime; production credentials stay in secret stores, never in source control.

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste a valid JWT (dev/staging only)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
```

- Never commit real JWTs, client secrets, or prod authority URLs with embedded keys.
- For OAuth2 flows, configure authorization code + PKCE pointing at dev IdP only.
- Disable Swagger UI in production or restrict by network — "Authorize" is a dev ergonomics feature.

**Production takeaway:** Swagger security definitions describe *how* to authenticate — the secrets live outside the repo and outside public Swagger in prod.

---

#### Q5. (R) Review minimal API + Swashbuckle — Swagger document empty.

**Answer:** Minimal API endpoints are discovered by `AddEndpointsApiExplorer()` — if the document is empty, typical causes are missing `AddSwaggerGen`, Swagger middleware not registered, wrong document name, or endpoints defined after `Run()`; lambda parameters need `[AsParameters]` or explicit `[FromBody]` for complex types in some versions.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Discovery | Endpoints not associated with OpenAPI document | Empty or incomplete swagger.json |
| Minimal APIs | Lambda inference may hide request body schema | POST `/orders` missing `CreateOrderRequest` body |
| Ordering | Middleware/endpoints mis-ordered | Swagger runs but explorer finds nothing |

**Fix (priority order):**

1. Confirm both `AddEndpointsApiExplorer()` and `AddSwaggerGen()` are registered.
2. For request bodies: `app.MapPost("/orders", ([FromBody] CreateOrderRequest req) => ...)` or use `[AsParameters]`.
3. Add `WithOpenApi()` on minimal routes for summaries, tags, and response types.
4. Verify `/swagger/v1/swagger.json` contains paths after startup — not just controller-based projects.

**Production takeaway:** Minimal APIs require explicit OpenAPI metadata — controllers get conventions; lambdas do not by default.

---

#### Q6. (D) Exposing EF entities directly in Swagger vs response DTOs.

**Answer:** Publishing EF entity types in OpenAPI couples clients to database schema, leaks internal columns (`CostPrice`, `RowVersion`, `InternalNotes`), and makes every migration a breaking API change — response DTOs define a stable, intentional contract.

| Concern | EF entity on wire | Public response DTO |
|---|---|---|
| Security | Internal fields in schema and JSON | Only approved fields exposed |
| Versioning | Column rename breaks clients | DTO versioning decoupled from DB |
| Serialization | Circular references, lazy-load surprises | Controlled shape, `[JsonIgnore]` not relied on |
| OpenAPI | Full persistence model documented | Accurate public contract |

- Map entity → `OrderResponseDto` in the action or via AutoMapper.
- `[ProducesResponseType(typeof(OrderResponseDto), 200)]` — never `typeof(Order)` for public endpoints.
- EF entities may appear in internal admin APIs behind stricter auth — still prefer DTOs for maintainability.

**Production takeaway:** Swagger documents what you expose — entity types teach integrators your database layout.

---

#### Q7. (M) What `AddSwaggerGen` produces; XML comments; `CustomSchemaIds` and polymorphic `$ref`.

**Answer:** Swashbuckle builds an `OpenApiDocument` at runtime from ApiExplorer endpoint metadata, action parameters, and serializer schema generators — it does not execute controllers; XML comments enrich descriptions when `IncludeXmlComments` points at generated `.xml` docs.

- **Startup vs runtime:** Service registration configures generators; the JSON is produced on first `/swagger/{docName}/swagger.json` request (cached thereafter).
- **`IncludeXmlComments`:** Requires `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — pulls `///` summaries onto operations and properties.
- **`CustomSchemaIds(type => type.FullName)`:** Prevents name collisions; `$ref` pointers use stable unique IDs — essential when multiple types share short names or generics flatten poorly.
- **Polymorphism:** System.Text.Json polymorphic types need `JsonDerivedType` or custom schema filters — default Swashbuckle may not emit `oneOf`/`discriminator` without `UseOneOfForPolymorphism()`.

**Production takeaway:** OpenAPI is generated metadata — collisions and missing XML comments are configuration problems, not runtime bugs.

---

#### Q8. (R) Review multi-version OpenAPI — v2 missing from Swagger UI; shared DTO names wrong.

**Answer:** Multiple `SwaggerDoc` registrations require Swagger UI endpoints for each document **and** a `DocInclusionPredicate` (or `IConfigureOptions<SwaggerGenOptions>` with versioning integration) to map controller actions to the correct document — without it, all actions land in one doc or v2 is omitted from UI.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Swagger UI | v2 endpoint not registered | Developers/clients never see v2 operations |
| Doc inclusion | No predicate tying `[ApiVersion]` to doc name | v1 doc contains v2 actions or vice versa |
| Schema | Shared DTO names across versions | Colliding schemas — wrong client models |

**Fix (priority order):**

1. Register both UI endpoints: `c.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");`.
2. Use `Asp.Versioning.Mvc.ApiExplorer` + `ConfigureSwaggerOptions` to set `DocInclusionPredicate`.
3. Apply `CustomSchemaIds` and version-specific DTO namespaces (`Contracts.V1`, `Contracts.V2`).
4. Generate separate NSwag clients per document version.

**Production takeaway:** API versioning without per-version OpenAPI documents breaks SDK generation — versioning and Swagger must be configured together.
