# Swagger & OpenAPI — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is OpenAPI?](#q1-what-is-openapi)
2. [Q2. What is Swagger in the context of ASP.NET Core?](#q2-what-is-swagger-in-the-context-of-aspnet-core)
3. [Q3. What is the difference between OpenAPI and Swagger UI?](#q3-what-is-the-difference-between-openapi-and-swagger-ui)
4. [Q4. What does `AddEndpointsApiExplorer` do?](#q4-what-does-addendpointsapiexplorer-do)
5. [Q5. What does `AddSwaggerGen` do?](#q5-what-does-addswaggergen-do)
6. [Q6. What is a Swagger document (`swagger.json`)?](#q6-what-is-a-swagger-document-swaggerjson)
7. [Q7. What is Swashbuckle?](#q7-what-is-swashbuckle)
8. [Q8. How does Swagger discover API endpoints?](#q8-how-does-swagger-discover-api-endpoints)
9. [Q9. What is a schema in OpenAPI?](#q9-what-is-a-schema-in-openapi)
10. [Q10. What does `[ProducesResponseType]` contribute to OpenAPI?](#q10-what-does-producesresponsetype-contribute-to-openapi)
11. [Q11. What is the purpose of Swagger UI?](#q11-what-is-the-purpose-of-swagger-ui)
12. [Q12. What is a security scheme in OpenAPI?](#q12-what-is-a-security-scheme-in-openapi)
13. [Q13. What is schema ID collision in Swagger generation?](#q13-what-is-schema-id-collision-in-swagger-generation)
14. [Q14. What does `IncludeXmlComments` do?](#q14-what-does-includexmlcomments-do)
15. [Q15. What is the difference between documenting controllers vs minimal APIs in Swagger?](#q15-what-is-the-difference-between-documenting-controllers-vs-minimal-apis-in-swagger)
16. [Q16. Why should Swagger UI be restricted in production?](#q16-why-should-swagger-ui-be-restricted-in-production)
17. [Q17. What is `DocInclusionPredicate` in Swagger?](#q17-what-is-docinclusionpredicate-in-swagger)
18. [Q18. What is the relationship between DTOs and OpenAPI schemas?](#q18-what-is-the-relationship-between-dtos-and-openapi-schemas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is OpenAPI?

**Concepts**
- OpenAPI specification — machine-readable REST API contract
- OpenAPI 3.x — JSON or YAML document format
- Paths, operations, components, schemas, security schemes
- Code generation — TypeScript, C#, Java client SDKs from a single source
- ASP.NET Core integration via Swashbuckle or `Microsoft.AspNetCore.OpenApi`

**Answer**

OpenAPI (formerly Swagger Specification) is a machine-readable standard for describing REST HTTP APIs — endpoints, parameters, request bodies, response schemas, authentication, and metadata — in a JSON or YAML document. The key value is that a single OpenAPI document drives documentation, TypeScript and C# client generation, API gateway configuration, and contract testing without duplication. ASP.NET Core 8 can produce OpenAPI documents via Swashbuckle or the built-in `Microsoft.AspNetCore.OpenApi` package, which reflects endpoint metadata at startup into the OpenAPI format. The spec describes the contract at a point in time — it does not execute or validate requests at runtime.

---

## Q2. What is Swagger in the context of ASP.NET Core?

**Concepts**
- Swashbuckle — NuGet package bridging ApiExplorer to OpenAPI
- `AddSwaggerGen()` — registers document generation services
- `UseSwagger()` — serves the JSON document
- `UseSwaggerUI()` — serves the interactive browser UI
- Built-in `AddOpenApi()` / `MapOpenApi()` — .NET 8 lightweight alternative

**Answer**

In ASP.NET Core, "Swagger" refers to the combination of OpenAPI document generation and the Swagger UI browser interface, typically integrated via the Swashbuckle.AspNetCore NuGet package. `AddSwaggerGen()` registers Swashbuckle services that build an OpenAPI document from ApiExplorer metadata at startup, `UseSwagger()` serves the JSON document at `/swagger/v1/swagger.json`, and `UseSwaggerUI()` serves the interactive browser interface. .NET 8 also offers `AddOpenApi()` and `MapOpenApi()` as a lighter built-in alternative that generates the document without the full Swashbuckle UI stack — useful when only the JSON document is needed for CI tooling. The Swagger UI lets developers explore and test endpoints without writing separate documentation.

---

## Q3. What is the difference between OpenAPI and Swagger UI?

**Concepts**
- OpenAPI — machine-readable JSON/YAML contract document
- Swagger UI — human-facing interactive browser renderer
- Document generation vs document presentation
- Serving JSON only — for CI and gateway tooling without UI

**Answer**

OpenAPI is the specification format — the `swagger.json` or `openapi.yaml` document that describes the API contract in a structured, machine-readable form. Swagger UI is a browser-based tool that renders that document for human exploration and testing, with a "Try it out" feature for sending real requests. The two are independent — I can serve the OpenAPI JSON document without exposing Swagger UI, which is the correct setup for production where CI pipelines and API management platforms consume the document but the interactive UI should not be publicly accessible. Swagger UI depends entirely on a valid OpenAPI document — an empty or malformed document produces an empty or broken interface.

---

## Q4. What does `AddEndpointsApiExplorer` do?

**Concepts**
- `IApiDescriptionGroupCollectionProvider` — endpoint metadata discovery interface
- Minimal API discovery — requires `AddEndpointsApiExplorer`
- Controller ApiExplorer — complemented by `AddMvcCore().AddApiExplorer()`
- Service registration order — must precede Swashbuckle registration

**Answer**

`AddEndpointsApiExplorer` registers the endpoint metadata explorer service that discovers minimal API routes for OpenAPI generation. Without it, minimal API `MapGet`, `MapPost`, and similar endpoint registrations are invisible to Swashbuckle and the built-in OpenAPI generator. Controller-based projects benefit from it as well since it complements the `AddMvcCore().AddApiExplorer()` path for full endpoint coverage. I call it in service registration — `builder.Services.AddEndpointsApiExplorer()` — before `AddSwaggerGen()` since Swashbuckle depends on the `IApiDescriptionGroupCollectionProvider` it registers.

---

## Q5. What does `AddSwaggerGen` do?

**Concepts**
- `ISwaggerProvider` — builds `OpenApiDocument` objects per document name
- Schema generation from DTO types — properties, nullability, data annotations
- Customization hooks — `CustomSchemaIds`, `IncludeXmlComments`, `DocInclusionPredicate`
- Document caching — generated on first request, cached thereafter
- `SwaggerDoc("v1", ...)` — names and metadata for each document

**Answer**

`AddSwaggerGen` registers Swashbuckle services that build an OpenAPI document at runtime by reflecting over ApiExplorer endpoint metadata, action parameters, return types, and schema generators for DTO types. The document is generated on the first request to `/swagger/v1/swagger.json` and cached for subsequent requests. The configuration callback on `AddSwaggerGen(options => ...)` is where I set document title and version, customize schema IDs to avoid collisions, include XML comments for property descriptions, register security definitions for Bearer JWT, and add `DocInclusionPredicate` for multi-version APIs. Schema generation reflects DTO property types, nullability annotations from nullable reference types, and DataAnnotations constraints like `[Range]` and `[StringLength]`.

---

## Q6. What is a Swagger document (`swagger.json`)?

**Concepts**
- `swagger.json` — serialized OpenAPI specification
- Served at `/swagger/v1/swagger.json` by default
- Multiple documents — separate JSON per API version
- ApiExplorer discovery — undocumented endpoints produce incomplete schemas
- Consumed by NSwag, OpenAPI Generator, API management platforms

**Answer**

A Swagger document is the serialized OpenAPI specification — listing all API paths, HTTP methods, parameters, request and response schemas, and security requirements in JSON format. Swashbuckle serves it at `/swagger/{docName}/swagger.json`, typically `/swagger/v1/swagger.json` for a single-version API. The document is the machine-readable source of truth that drives SDK generation, Swagger UI rendering, and API gateway configuration, so its accuracy matters — endpoints missing `[HttpGet]` attributes, actions without `[ProducesResponseType]`, and types excluded from ApiExplorer produce incomplete or misleading schemas. Multiple documents can coexist for versioned APIs, each with its own JSON file reflecting a distinct set of operations.

---

## Q7. What is Swashbuckle?

**Concepts**
- Swashbuckle.AspNetCore — NuGet package for OpenAPI generation
- Bridges ApiExplorer to OpenAPI 3.x documents
- XML comment integration, custom schema filters, multi-document versioning
- `Microsoft.AspNetCore.OpenApi` — built-in .NET 8 alternative without UI

**Answer**

Swashbuckle.AspNetCore is the NuGet package that integrates OpenAPI document generation and Swagger UI into ASP.NET Core applications. It bridges ApiExplorer metadata — the same metadata used for routing and binding — to OpenAPI 3.x documents with support for XML comment enrichment, custom schema filters, security definitions, and multi-document versioning. The main alternative in .NET 8 is the built-in `Microsoft.AspNetCore.OpenApi` package, which generates OpenAPI documents without the full Swashbuckle UI stack — useful for projects that serve the JSON document to CI pipelines or API gateways but present documentation through a separate portal. I choose Swashbuckle when I need the interactive Swagger UI during development, and the built-in package when I only need the document artifact.

---

## Q8. How does Swagger discover API endpoints?

**Concepts**
- ApiExplorer infrastructure — `IApiDescriptionGroupCollectionProvider`
- Controller discovery — `[Route]`, `[HttpGet]` attribute reflection
- Minimal API discovery — requires `AddEndpointsApiExplorer()` and `.WithOpenApi()`
- `[ApiExplorerSettings(IgnoreApi = true)]` — excludes endpoints
- Startup reflection — no runtime execution of controllers

**Answer**

Swagger discovers endpoints through the ASP.NET Core ApiExplorer infrastructure — `IApiDescriptionGroupCollectionProvider` collects metadata from controller actions and minimal API routes registered during application startup. Controller actions are discovered via `[Route]`, `[HttpGet]`, and similar attributes combined with parameter types and return type metadata. Minimal API routes are invisible to ApiExplorer without `AddEndpointsApiExplorer()`, and benefit from `.WithOpenApi()` on the route registration for richer metadata like summaries and tags. I use `[ApiExplorerSettings(IgnoreApi = true)]` on internal utility actions that should not appear in the public document. Discovery is entirely reflection-based at startup — Swashbuckle does not execute controllers or make real HTTP calls during document generation.

---

## Q9. What is a schema in OpenAPI?

**Concepts**
- OpenAPI schema — describes data type structure and constraints
- `components.schemas` — shared schema definitions referenced by `$ref`
- Generated from .NET DTO types via schema generation
- Nullable reference types — affect `nullable: true` in OpenAPI 3
- Schema ID collision — two types with the same short name

**Answer**

An OpenAPI schema describes the structure of a data type — properties, their types, formats, nullability, required fields, and constraints like minimum and maximum values. Schemas appear in request body definitions, response payload definitions, and parameter definitions, with shared schemas stored in `components.schemas` and referenced via `$ref` to avoid duplication. Swashbuckle generates schemas by reflecting over the .NET DTO types used in action parameters and return types, mapping C# `string` to `{ "type": "string" }`, `int?` to `{ "type": "integer", "nullable": true }`, and so on. Nullable reference types enable in the project affect whether `nullable: true` appears in schemas, which matters for generated client code. Schema ID collisions occur when two types share the same short class name — resolved with `CustomSchemaIds`.

---

## Q10. What does `[ProducesResponseType]` contribute to OpenAPI?

**Concepts**
- `[ProducesResponseType]` — adds status code and type metadata to ApiExplorer
- Multiple attributes — documents 200, 400, 404, 409 on one action
- `IActionResult` without generic — incomplete schema without `[ProducesResponseType]`
- Generated client accuracy — error response shapes alongside success

**Answer**

`[ProducesResponseType]` adds response metadata to ApiExplorer — the HTTP status code, the CLR response type, and optionally the content type — which Swashbuckle maps to OpenAPI response definitions with typed schemas. For example, `[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]` produces a 200 response schema that references `OrderDto` in the OpenAPI document. Without it, Swashbuckle may infer the return type incompletely from `IActionResult` return signatures, leaving the schema as `object` or omitted entirely. Multiple attributes on the same action document different status codes — 200 with the success type, 400 with `ValidationProblemDetails`, and 404 with `ProblemDetails` — which improves generated client SDKs by giving typed error handling rather than an untyped error branch.

---

## Q11. What is the purpose of Swagger UI?

**Concepts**
- Swagger UI — interactive browser interface for the OpenAPI document
- "Try it out" — sends real requests from the browser
- Authentication flows — Bearer token entry, OAuth2 PKCE support
- Multiple document versions — dropdown for versioned APIs
- Development and staging only — should be restricted in production

**Answer**

Swagger UI is an interactive browser interface that renders the OpenAPI document — listing endpoints with their parameters, request body schemas, and expected response shapes — and provides a "Try it out" feature that sends actual HTTP requests from the browser. It replaces manual documentation and reduces the friction of onboarding new team members and partners since they can explore the API surface and test calls without a separate tool. It supports authentication flows including Bearer token entry and OAuth2 authorization code with PKCE, so developers can test authenticated endpoints directly. I restrict Swagger UI to Development and Staging environments because in production it exposes the full API surface to anyone who can reach the URL, aiding reconnaissance.

---

## Q12. What is a security scheme in OpenAPI?

**Concepts**
- Security scheme — documents authentication method in OpenAPI
- `components.securitySchemes` — named scheme definitions
- Bearer JWT scheme — `SecuritySchemeType.Http` with scheme `"bearer"`
- `AddSecurityRequirement` — global requirement applies to all operations
- Swagger UI "Authorize" button — rendered from registered schemes

**Answer**

A security scheme defines how clients authenticate to the API — Bearer JWT, API key, OAuth2 flows, or basic auth — documented in the `components.securitySchemes` section and referenced per-operation or globally in the OpenAPI document. I add a Bearer scheme with `AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" })` and then apply it globally with `AddSecurityRequirement` so every operation in the document shows the lock icon in Swagger UI. The Swagger UI "Authorize" dialog renders based on registered security definitions, allowing developers to enter a token once and have it applied to all "Try it out" requests. Security schemes describe the authentication method — they do not store or validate credentials.

---

## Q13. What is schema ID collision in Swagger generation?

**Concepts**
- Schema ID collision — two types with the same short name produce one schema
- Default schema ID — short type name without namespace
- `CustomSchemaIds` — resolves collisions with unique identifiers
- Versioned parallel types — common source of collisions
- Silent overwrite — wrong properties in generated clients

**Answer**

Schema ID collision occurs when Swashbuckle generates the same schema identifier for two different .NET types that share a short class name — for example, `ProductDto` in both `Acme.Api.Contracts.v1` and `Acme.Api.Contracts.v2` namespaces. The default schema ID is the short type name without namespace, so both types map to the schema ID `"ProductDto"`, and one silently overwrites the other. The OpenAPI document then contains a `ProductDto` schema with wrong or merged properties, which breaks TypeScript and C# clients generated from it. The fix is `options.CustomSchemaIds(type => type.FullName?.Replace("+", "."))` in `AddSwaggerGen`, which produces fully qualified unique IDs. Collisions are silent during generation and only surface when a client SDK fails to compile or returns unexpected data shapes at runtime.

---

## Q14. What does `IncludeXmlComments` do?

**Concepts**
- `IncludeXmlComments` — reads XML documentation files into OpenAPI
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — required in `.csproj`
- `///` summary comments — mapped to operation descriptions and property docs
- Multiple XML files — controllers, models, and shared contract assemblies
- Human-readable descriptions beyond reflection metadata

**Answer**

`IncludeXmlComments` configures Swashbuckle to read the XML documentation file generated from `///` summary comments and attach the descriptions to OpenAPI operation summaries, parameter descriptions, and schema property descriptions. This enriches the Swagger UI and generated client code with human-readable documentation beyond what reflection alone can provide — type names and parameter types are available from reflection, but the intent and usage guidance come from the XML comments. I enable it by adding `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to the `.csproj`, then calling `c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "MyApi.xml"))` in `AddSwaggerGen`. Multiple XML files can be included for separate controller, model, and contract assemblies.

---

## Q15. What is the difference between documenting controllers vs minimal APIs in Swagger?

**Concepts**
- Controller discovery — convention-driven via `[Route]`, `[HttpGet]`, attributes
- Minimal API discovery — requires explicit `.WithOpenApi()` and metadata
- `[AsParameters]` — binds complex parameter types in minimal API route handlers
- `AddEndpointsApiExplorer()` — required for minimal API endpoint visibility
- Lambda return type inference — untyped without explicit `.Produces<T>(200)`

**Answer**

Controller-based APIs inherit rich default metadata from `[Route]`, `[HttpGet]`, `[ProducesResponseType]`, and parameter binding attributes — Swashbuckle discovers all of this automatically, so a well-attributed controller produces a complete OpenAPI document with minimal extra work. Minimal APIs use lambda handlers that lack the convention-based metadata framework that controllers provide, so I must be explicit: `.WithOpenApi()` adds the operation to the document, `.Produces<OrderDto>(200)` documents the success response type, `.ProducesProblem(400)` documents validation error responses, and `[AsParameters]` or explicit `[FromBody]` attributes tell the framework how to bind complex parameters. Both require `AddEndpointsApiExplorer()`. For a greenfield API I lean toward controllers for the documentation completeness they provide by convention; for lightweight internal APIs I use minimal APIs with explicit metadata.

---

## Q16. Why should Swagger UI be restricted in production?

**Concepts**
- API surface exposure — every endpoint, schema, and auth scheme visible
- Reconnaissance aid — field names and enum values useful for attacks
- `IsDevelopment()` environment check — standard gate
- OpenAPI JSON vs UI — document can serve CI without exposing UI
- IP restriction or auth-protected `/swagger` route

**Answer**

Swagger UI exposes the full API surface — every endpoint, parameter name, schema, enum value, and authentication scheme — to anyone who can reach the URL, which provides useful information for targeted attacks against misconfigured or undocumented endpoints. Even with all endpoints protected by `[Authorize]`, the Swagger UI documents their existence and parameter shapes, reducing the effort needed to craft a valid attack request. I gate Swagger UI behind `if (app.Environment.IsDevelopment())` so it never activates in production deployments. When internal developers need the OpenAPI document in production for SDK generation I serve the JSON document through a reverse proxy path behind an IP allowlist or require authentication on the `/swagger` route, keeping the interactive UI private while the document artifact remains accessible to tooling.

---

## Q17. What is `DocInclusionPredicate` in Swagger?

**Concepts**
- `DocInclusionPredicate` — `(docName, apiDescription) => bool` filter
- Multi-version APIs — routes v1 and v2 operations to separate documents
- `ApiDescription.GroupName` — matches ApiExplorer version group names
- `SwaggerDoc("v1", ...)` / `SwaggerDoc("v2", ...)` — separate document registrations
- Without predicate — all actions appear in all documents or only the default

**Answer**

`DocInclusionPredicate` is a Swashbuckle filter function `(docName, apiDescription) => bool` that decides which API descriptions appear in which OpenAPI document. It is essential for multi-version APIs where v1 and v2 operations must land in separate `swagger.json` files rather than all appearing in every document. With `Asp.Versioning.Mvc`, each API version produces a distinct ApiExplorer group name such as `"v1"` or `"v2"`, and the predicate `(docName, apiDesc) => apiDesc.GroupName == docName` routes each action to the document matching its version. Without a predicate, all actions may appear in all registered documents simultaneously, producing duplicate operations and wrong schemas. I pair it with separate `SwaggerDoc("v1", ...)` and `SwaggerDoc("v2", ...)` registrations and matching Swagger UI endpoint configurations.

---

## Q18. What is the relationship between DTOs and OpenAPI schemas?

**Concepts**
- DTOs as public contract — OpenAPI schema reflects DTO shape
- EF entity exposure — leaks internal columns and navigation properties
- `[ProducesResponseType(typeof(OrderResponseDto), 200)]` — pins schema to correct type
- DTO renames and removals — breaking schema changes requiring versioning
- Stable intentional schemas — only approved fields, no database columns

**Answer**

OpenAPI schemas are generated from the .NET types used in action parameters and return types — typically request and response DTOs. The DTO defines the public API contract and the OpenAPI schema is the machine-readable representation of that contract, so they must stay in sync. Exposing EF Core entities directly generates schemas that reflect the database column structure including internal fields, navigation properties, and circular references, which leaks implementation details to clients and breaks schema generation. I always map entities to response DTOs before returning from actions — `[ProducesResponseType(typeof(OrderResponseDto), 200)]` ensures the documented schema matches the actual response shape. DTO renames and property removals are breaking API changes that break generated clients, so I version DTOs alongside API versions rather than modifying them in place.

---

## Gotchas — Swagger & OpenAPI (Interview Traps)

---

#### Gotcha 1. Duplicate operation IDs with versioned API controllers

**Concepts**
- Swashbuckle generates `operationId` from controller name + action name by default
- Two versioned controllers with the same action names produce duplicate IDs
- Duplicate `operationId` — spec validation failure; code generators emit compile errors
- `IOperationFilter` or `UseOperationId` to make IDs unique per version

**Answer**

When two versioned controllers — `OrdersV1Controller` and `OrdersV2Controller` — both have a `Get(int id)` action, Swashbuckle generates the same `operationId` for both if no deduplication is in place. The merged OpenAPI document fails spec validation, and code-generated client SDKs produce duplicate method names that do not compile. I add an `IOperationFilter` that appends the API version to the operation ID, or configure Swashbuckle with `c.CustomOperationIds(e => ...)` that includes the version segment, ensuring every operation ID is globally unique across all version documents.

---

#### Gotcha 2. XML doc comments not included in Swagger

**Concepts**
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `.csproj` required
- `c.IncludeXmlComments(xmlPath)` in `AddSwaggerGen` wiring the file in
- Missing XML output path — comments exist but are not read by Swashbuckle
- Suppress `CS1591` warning for undocumented members or document all public symbols

**Answer**

XML documentation comments (`/// <summary>`) are not included in the OpenAPI document unless the project file generates a documentation XML file (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`) and Swashbuckle is pointed at it with `c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "MyApi.xml"))`. A common mistake is enabling `GenerateDocumentationFile` but not wiring `IncludeXmlComments`, or using the wrong file name in the path. I also enable this for any referenced model projects so DTO summaries appear in the schema descriptions.

---

#### Gotcha 3. Polymorphic response type not documented

**Concepts**
- `ActionResult<AnimalDto>` — Swashbuckle documents `AnimalDto` schema only
- Discriminated union response not representable without `[SwaggerSubType]` or `OneOf`
- `oneOf` schema requires Swashbuckle.AspNetCore 6+ and explicit configuration
- Missing sub-type schemas in OpenAPI — generated clients use base type, lose derived fields

**Answer**

An action that may return `DogDto` or `CatDto` (both extending `AnimalDto`) cannot be expressed simply as `ActionResult<AnimalDto>` — Swashbuckle only documents the base schema. Clients receiving a `DogDto` response see only `AnimalDto` fields in the generated type. Swashbuckle 6+ supports `oneOf` via `[SwaggerSubType(typeof(DogDto))]` and `UseOneOfForPolymorphism()` configuration, but requires discriminator setup to match the JSON discriminator field. I design polymorphic responses carefully and document each sub-type explicitly, or flatten to tagged union DTOs to avoid schema complexity.

---

#### Gotcha 4. Authentication scheme not shown in Swagger UI

**Concepts**
- `c.AddSecurityDefinition("Bearer", ...)` — declares the scheme in the OpenAPI document
- `c.AddSecurityRequirement(...)` — marks which operations require the scheme
- Missing security definition — Swagger UI shows no "Authorize" button
- `IOperationFilter` for per-operation security requirements vs global security

**Answer**

Adding `[Authorize]` to controllers does not automatically tell Swashbuckle about the authentication scheme — the OpenAPI security scheme must be declared separately in `AddSwaggerGen` with `c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {...})` and applied globally or per-operation with `c.AddSecurityRequirement(...)`. Without this, Swagger UI shows no "Authorize" button and generated client SDKs have no knowledge of the authentication mechanism. I add a `SecurityRequirementsOperationFilter` that reads `[Authorize]` attributes and applies the security requirement to the appropriate operations in the document.

---

#### Gotcha 5. Enum serialized as integer in schema vs string in JSON

**Concepts**
- `System.Text.Json` default — enums serialize as integers
- `JsonStringEnumConverter` configured in `AddJsonOptions` — enums serialize as strings
- OpenAPI schema generated by Swashbuckle — must match actual serialization format
- `c.UseInlineDefinitionsForEnums()` vs `EnumSchemaFilter` for string schema

**Answer**

If `JsonStringEnumConverter` is added to `AddJsonOptions` but Swashbuckle is not configured to match, the API sends string enum values while the OpenAPI schema documents integer enum values. Generated TypeScript clients read integer enum mappings but receive strings at runtime, causing deserialization failures. I add `c.UseInlineDefinitionsForEnums()` or an `EnumSchemaFilter` to Swashbuckle configuration that outputs string values in the schema, keeping the documentation in sync with the actual serialization. The same applies in reverse — if enums serialize as integers, the schema should document integer values, not string member names.

---

#### Gotcha 6. Versioned API documents — one merged document vs separate per version

**Concepts**
- `SwaggerEndpoint` with a single JSON URL — one document for all versions
- `AddVersionedApiExplorer()` — one document per version
- Mixing v1 and v2 endpoints in one Swagger document causes operation ID conflicts
- Client SDK generation targeting the specific version document

**Answer**

A versioned API that feeds all versions into a single Swashbuckle document produces operation ID conflicts and a confusing UI where deprecated v1 and current v2 operations are interleaved. The correct setup with `Asp.Versioning.Mvc` is to call `AddVersionedApiExplorer()` which generates a separate `ApiDescription` group per version, then configure one `SwaggerEndpoint` per version in `UseSwaggerUI`. Each version's document contains only its own operations, and code generators can target `v1` or `v2` in isolation.

---

#### Gotcha 7. `[ProducesResponseType]` on base controller not seen by Swashbuckle

**Concepts**
- Swashbuckle reads attributes on the action method and its immediate controller class
- `[ProducesResponseType]` on a base controller class — not inherited into the document
- Each controller or action must redeclare, or use an `IOperationFilter` to apply globally
- `ProducesDefaultResponseType` for fallback error shape documentation

**Answer**

`[ProducesResponseType(typeof(ProblemDetails), 400)]` placed on a `BaseApiController` is not picked up by Swashbuckle for derived controllers — the attribute is inherited by the CLR but Swashbuckle's API explorer integration reads only the concrete controller's attributes. Derived actions therefore have undocumented 400 responses in the OpenAPI spec. I use an `IOperationFilter` to inject common response types (400, 401, 403, 404, 500) globally for all operations, which is more maintainable than repeating the attributes on every action or every base class.

---

#### Gotcha 8. Non-nullable reference types generating nullable schema

**Concepts**
- `System.Text.Json` nullable annotations vs OpenAPI schema nullable flag
- `c.SupportNonNullableReferenceTypes()` — Swashbuckle option to honor C# nullability
- Missing `nullable: false` in schema — clients may generate optional fields for required properties
- `[Required]` on non-nullable properties vs NRT annotations

**Answer**

By default, Swashbuckle marks reference-type properties as nullable in the schema regardless of C# nullable reference type annotations, generating `string?` in TypeScript client types for what should be `string`. Calling `c.SupportNonNullableReferenceTypes()` in `AddSwaggerGen` tells Swashbuckle to honor the `?` annotations and emit `nullable: false` for non-nullable properties. This is a breaking change for clients that depended on all reference types being optional, so I enable it intentionally and validate generated client types in a contract test.

---

#### Gotcha 9. Swashbuckle not loading on staging — missing middleware check

**Concepts**
- `UseSwagger()` and `UseSwaggerUI()` gated behind `IsDevelopment()` check
- Staging or production environments — Swagger UI unavailable to internal teams
- Alternative: gate behind authorization middleware rather than environment only
- `MapSwagger()` minimal API equivalent

**Answer**

Gating Swagger behind `if (app.Environment.IsDevelopment())` blocks internal developers in staging who need the UI to test the latest deployment. I use a more granular guard: `if (app.Environment.IsDevelopment() || app.Environment.IsStaging())` or place the Swagger endpoints behind an authorization policy that allows authenticated internal users while blocking public access. For production internal portals, I serve the OpenAPI JSON document (without the interactive UI) through a path that is IP-restricted at the reverse proxy, giving internal tooling access without the full try-it-out surface.

---

#### Gotcha 10. Action returning `object` causes `{}` schema in Swagger

**Concepts**
- `IActionResult` or `object` return type — Swashbuckle emits empty schema `{}`
- `ActionResult<T>` or `[ProducesResponseType(typeof(T), 200)]` needed for schema inference
- `TypedResults` in minimal APIs providing compile-time type information
- Code-generated clients from `{}` schema produce `dynamic` or `object` types

**Answer**

An action returning `object` or `IActionResult` without any type annotation causes Swashbuckle to document the 200 response as an empty schema `{}`, which tells code generators to produce `dynamic`, `object`, or `any` in TypeScript. Clients lose all type safety on the response. `ActionResult<T>` lets Swashbuckle infer `T` as the success schema automatically. When using `IActionResult` for flexibility, I add `[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]` to provide the type hint. For minimal APIs, `TypedResults.Ok(dto)` achieves the same at compile time without any annotation.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Review this `Program.cs` from a production Web API. Swagger UI is reachable at `/swagger` in all environments; security review flagged it before go-live.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments API v1"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

Deployment: public internet, no IP allowlist, JWT-protected endpoints documented with real example tokens in `appsettings.Development.json`.

---

**Concepts**
- Swagger UI in all environments — exposes full API surface publicly
- `IsDevelopment()` — standard environment gate
- Real tokens in `appsettings.Development.json` — secret management failure
- OpenAPI JSON vs interactive UI — separate concerns for production tooling

**Answer**

The immediate risk is that `UseSwagger()` and `UseSwaggerUI()` run unconditionally, so every deployment including production exposes the full API surface — every endpoint, parameter, schema, and auth scheme — to anyone with the URL. For a payments API on the public internet this is a significant reconnaissance aid, giving an attacker documented request shapes for every endpoint.

The fix is to wrap both calls in `if (app.Environment.IsDevelopment())`. If the OpenAPI document is needed in staging for SDK generation or QA tooling, I serve `UseSwagger()` in staging but not `UseSwaggerUI()`, and protect the document endpoint behind an IP allowlist at the reverse proxy layer. The secondary issue — real example tokens in `appsettings.Development.json` — is a credential leak risk if that file is committed to source control. Development secrets belong in `dotnet user-secrets` or environment variables, never in committed configuration files.

---

#### Q2. (R) Review Swagger schema generation. Two DTOs in different namespaces share the same class name; generated OpenAPI shows merged/wrong properties and TypeScript client fails to compile.

```csharp
namespace Acme.Api.Contracts.v1 { public class ProductDto { public int Id { get; set; } public string Sku { get; set; } = ""; } }
namespace Acme.Api.Internal { public class ProductDto { public int Id { get; set; } public decimal Cost { get; set; } public string SupplierCode { get; set; } = ""; } }

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog API", Version = "v1" });
});
```

Both types appear on different controller actions exposed in the same document.

---

**Concepts**
- Schema ID collision — short type name without namespace
- `CustomSchemaIds` — unique schema identifiers from full type name
- Internal DTO exposure — `Acme.Api.Internal.ProductDto` should not be in public document
- TypeScript client compile failure — merged wrong schema breaks generated types

**Answer**

The default schema ID is the short class name without namespace, so both `ProductDto` types map to the same schema ID. The second one processed silently overwrites the first in the OpenAPI document's `components.schemas`, producing a `ProductDto` schema with a mix of properties from both types. The TypeScript client generated from this document has a `ProductDto` type that does not match either actual API response, causing compile errors or runtime mismatches.

There are two problems to fix. First, I add `options.CustomSchemaIds(type => type.FullName?.Replace("+", "."))` in `AddSwaggerGen` to produce unique fully qualified schema IDs. Second, I investigate why `Acme.Api.Internal.ProductDto` is exposed in the public API document at all — internal DTOs should never appear on public controller actions. The `Acme.Api.Internal` namespace suggests it is an implementation detail that should be mapped to a proper public DTO before being returned from the action. Removing the internal type from the public contract eliminates the collision and the leakage simultaneously.

---

#### Q3. (P) A PATCH DTO uses `bool?` for tri-state fields and `DateOnly?` for optional dates. Swagger UI and NSwag-generated clients show wrong nullability — required fields where omission is valid. How do you align OpenAPI schema with actual JSON binding behavior in ASP.NET Core 8?

---

**Concepts**
- Nullable reference types — affect `nullable: true` in generated schemas
- `bool?` and `DateOnly?` — OpenAPI must show `nullable: true` for these
- `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` — omit nulls from responses
- NSwag schema generation — reads nullability from type annotations
- `[Required]` on nullable property — incorrect, forces required in schema

**Answer**

The schema accuracy problem has two common causes. First, if the project does not have `<Nullable>enable</Nullable>` in the `.csproj`, Swashbuckle cannot read nullability annotations from the type system and may treat all reference types as required. Enabling nullable reference types lets Swashbuckle emit `nullable: true` for `bool?` and `DateOnly?` automatically. Second, any `[Required]` annotation on a nullable property contradicts the intent — `[Required]` marks the field as mandatory in the schema, but a PATCH field with `bool?` is explicitly optional. I remove `[Required]` from patch DTO properties that are meant to be omittable.

For NSwag-generated clients, the schema must also carry `"nullable": true` in the OpenAPI 3.0 format or `"x-nullable": true` in Swagger 2.0. I verify the generated document at `/swagger/v1/swagger.json` to confirm the nullable flags are present before regenerating clients. I also configure `JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull` so null patch properties are omitted from serialized responses, keeping the response contract consistent with the nullable-optional intent.

---

#### Q4. (P) Review JWT authentication setup for Swagger UI on an internal admin API. Developers paste bearer tokens manually today; you want "Authorize" in Swagger UI without leaking production secrets in the repo.

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });
    // no security definition yet
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* ... */);
```

What do you add to OpenAPI and Swagger UI, and what must stay out of source control?

---

**Concepts**
- `AddSecurityDefinition` — registers Bearer scheme in OpenAPI
- `AddSecurityRequirement` — applies scheme globally to all operations
- Swagger UI "Authorize" button — renders from registered security definitions
- `dotnet user-secrets` — development JWT configuration
- `appsettings.{Environment}.json` — secrets must not be committed

**Answer**

To enable the "Authorize" button in Swagger UI I add a security definition and a global security requirement in `AddSwaggerGen`:

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    Description = "Enter the JWT Bearer token"
});
c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
        Array.Empty<string>()
    }
});
```

This produces a lock icon on every operation in Swagger UI and an "Authorize" dialog where developers enter a token once — it is then applied automatically to all "Try it out" requests via the `Authorization: Bearer <token>` header.

What must stay out of source control: the JWT signing key, issuer URIs, audience values, and any token examples. These belong in `dotnet user-secrets` for local development (`dotnet user-secrets set "JwtSettings:Key" "..."`) and in environment variables or Azure Key Vault for deployed environments. The `appsettings.json` file can contain non-secret configuration like token validation parameters referencing the key by name, but the key material itself never goes into a committed file.

---

#### Q5. (R) Review this minimal API + Swashbuckle setup. Swagger document is empty — no operations listed — but controllers in the same project document correctly when migrated.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok("healthy"));

app.MapPost("/orders", (CreateOrderRequest req) =>
{
    return Results.Created($"/orders/{req.Id}", req);
});

app.Run();
```

`CreateOrderRequest` is a public record; endpoint uses lambda without `[FromBody]` attributes.

---

**Concepts**
- `AddEndpointsApiExplorer()` — required for minimal API discovery
- `.WithOpenApi()` — enriches minimal API endpoints with operation metadata
- Missing `AddControllers()` — minimal APIs do not need it but controllers do
- Lambda parameter binding inference — may not emit ApiExplorer metadata without hints

**Answer**

The Swagger document is empty because minimal API route handlers do not automatically emit rich ApiExplorer metadata the way controller actions do — they need explicit opt-in. The `AddEndpointsApiExplorer()` call is present and correct, but the endpoints are missing `.WithOpenApi()` which registers them with ApiExplorer so Swashbuckle can discover them.

The fix is to chain `.WithOpenApi()` on each route registration:

```csharp
app.MapGet("/health", () => Results.Ok("healthy"))
    .WithOpenApi();

app.MapPost("/orders", ([FromBody] CreateOrderRequest req) =>
    Results.Created($"/orders/{req.Id}", req))
    .WithOpenApi()
    .Produces<CreateOrderRequest>(201)
    .ProducesProblem(400);
```

I also add explicit `[FromBody]` on `CreateOrderRequest` so Swashbuckle knows the binding source for schema generation — without it the parameter may appear as a query parameter in the document. The `Produces<T>` call documents the response type so the TypeScript client gets a typed success schema.

---

#### Q6. (D) A team debates exposing EF Core entity types directly in Swagger instead of separate response DTOs to "move faster." Review the API surface and documentation implications for versioning, security, and client coupling.

```csharp
[HttpGet("{id:int}")]
[ProducesResponseType(typeof(Order), 200)]
public async Task<ActionResult<Order>> Get(int id)
    => await _db.Orders.Include(o => o.LineItems).FirstOrDefaultAsync(o => o.Id == id);

// Order entity includes: InternalNotes, CostPrice, RowVersion, SupplierId
```

OpenAPI generates full `Order` schema including all mapped columns.

---

**Concepts**
- Schema leakage — internal columns in public OpenAPI document
- Client coupling to database schema — DTO changes become breaking API changes
- Navigation property schemas — recursive schemas, circular references
- `InternalNotes`, `CostPrice` — sensitive business data exposed in schema
- DTO stability — protects clients from database schema migrations

**Answer**

Exposing `Order` entities directly creates four compounding problems. First, the OpenAPI schema includes every mapped column — `InternalNotes`, `CostPrice`, `RowVersion`, and `SupplierId` — fields that clients should never see. Once documented in the public OpenAPI spec and distributed in TypeScript client packages, these field names become part of the public contract even if never intentionally used. Second, any database schema migration that adds, renames, or removes an `Order` column is an unintentional breaking API change for all clients that generated their models from the schema. Third, navigation properties like `LineItems` create recursive schemas that may cause circular reference problems in Swashbuckle's schema generation. Fourth, lazy-loaded navigations included via `Include` in one version of the endpoint may be absent in another, producing inconsistent response shapes.

The correct approach is to create a dedicated `OrderResponse` DTO containing only the fields clients need, map the entity to it before returning, and use `[ProducesResponseType(typeof(OrderResponse), 200)]`. The DTO is stable and intentional — database schema changes are internal implementation details that only cause API changes when explicitly promoted to the DTO. This takes a few minutes longer today but prevents a category of breaking changes and security incidents across the entire API lifetime.

---

#### Q7. (M) Walk through what `AddSwaggerGen` produces at startup vs runtime, how `IncludeXmlComments` affects the document, and why `c.CustomSchemaIds(type => type.FullName)` is commonly added in larger APIs — tie to schema collision and polymorphic `$ref` behavior.

---

**Concepts**
- `AddSwaggerGen` at startup — registers services, not the document itself
- Document generation at first request — on-demand, then cached
- `IncludeXmlComments` — enriches operation and schema descriptions from XML file
- `CustomSchemaIds` — prevents schema ID collision in larger codebases
- Polymorphic `$ref` — discriminator requires unique schema IDs

**Answer**

`AddSwaggerGen` runs during service registration at startup and registers the `ISwaggerProvider`, schema generators, and configuration options — it does not actually build the OpenAPI document at this stage. The document is generated lazily on the first HTTP request to `/swagger/v1/swagger.json`, at which point Swashbuckle calls `IApiDescriptionGroupCollectionProvider` to enumerate endpoints, reflects over parameter and return types to generate schemas, and serializes the result to JSON. The document is then cached so subsequent requests return the cached version without re-reflecting.

`IncludeXmlComments` reads the `.xml` documentation file generated from `///` summary comments and attaches the text to corresponding operations, parameters, and schema properties in the document. This happens during document generation, so any endpoint or property without a summary comment gets no description in the OpenAPI schema.

`CustomSchemaIds(type => type.FullName)` exists because the default schema ID is the short type name, and in larger APIs with multiple versioned DTOs or types from different assemblies, name collisions are common — `ProductDto` in v1 and v2 namespaces map to the same ID and one silently overwrites the other. Full type names produce unique IDs at the cost of verbose schema names in the document. For polymorphic types using OpenAPI discriminators, unique schema IDs are required because the `$ref` chain from a base type to its derived types depends on each discriminated type having a distinct schema component name — a collision breaks the polymorphic schema entirely and generated clients cannot deserialize derived types correctly.

---

#### Q8. (R) Review multi-version OpenAPI setup. v1 and v2 controllers exist but Swagger UI only shows v1 operations; v2 clients get wrong schemas for shared DTO names.

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "API", Version = "v2" });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    // v2 endpoint not registered
});

// v2 controller: [ApiVersion("2.0")] on separate controller type
```

No `DocInclusionPredicate` or `ConfigureSwaggerOptions` ties actions to documents.

---

**Concepts**
- `DocInclusionPredicate` — routes actions to the correct versioned document
- Missing Swagger UI endpoint for v2 — UI cannot switch to v2 document
- `Asp.Versioning.Mvc` ApiExplorer group names — `"v1"`, `"v2"` per version
- `CustomSchemaIds` — required when v1 and v2 share DTO names
- `ConfigureSwaggerOptions` — conventional setup with `Asp.Versioning`

**Answer**

Two separate issues explain the symptoms. First, Swagger UI only shows v1 because the v2 `SwaggerEndpoint` is missing from `UseSwaggerUI`. Adding `c.SwaggerEndpoint("/swagger/v2/swagger.json", "v2")` makes the v2 document accessible in the UI dropdown. Second, without a `DocInclusionPredicate`, Swashbuckle puts all actions in all registered documents — v2 controller actions appear in both the v1 and v2 documents simultaneously — so the v1 document is polluted with v2 operations and the v2 document is polluted with v1 operations.

The fix is to add a `DocInclusionPredicate` that maps actions to their version document:

```csharp
options.DocInclusionPredicate((docName, apiDesc) =>
    apiDesc.GroupName == docName);
```

With `Asp.Versioning.Mvc`, each controller's `[ApiVersion("2.0")]` populates `apiDesc.GroupName` as `"v2"`, and the predicate routes v2 actions to the `"v2"` document only. For shared DTO names across versions I add `options.CustomSchemaIds(type => type.FullName?.Replace("+", "."))` to prevent the schema collision that produces wrong schemas in the v2 document.
