# Content Negotiation & Formatters — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is content negotiation in ASP.NET Core Web API?](#q1-what-is-content-negotiation-in-aspnet-core-web-api)
2. [Q2. What is the Accept HTTP header used for?](#q2-what-is-the-accept-http-header-used-for)
3. [Q3. What is the Content-Type header used for in API requests?](#q3-what-is-the-content-type-header-used-for-in-api-requests)
4. [Q4. What is the default JSON serializer in ASP.NET Core 8?](#q4-what-is-the-default-json-serializer-in-aspnet-core-8)
5. [Q5. What is camelCase JSON naming and why is it used in Web APIs?](#q5-what-is-camelcase-json-naming-and-why-is-it-used-in-web-apis)
6. [Q6. What is the difference between input formatters and output formatters?](#q6-what-is-the-difference-between-input-formatters-and-output-formatters)
7. [Q7. What HTTP status code is returned when content negotiation fails?](#q7-what-http-status-code-is-returned-when-content-negotiation-fails)
8. [Q8. What does `[Produces("application/json")]` do?](#q8-what-does-producesapplicationjson-do)
9. [Q9. What does `[Consumes("application/xml")]` do?](#q9-what-does-consumesapplicationxml-do)
10. [Q10. What is the difference between `System.Text.Json` and Newtonsoft.Json in ASP.NET Core?](#q10-what-is-the-difference-between-systemtextjson-and-newtonsoftjson-in-aspnet-core)
11. [Q11. How does model binding relate to input formatters?](#q11-how-does-model-binding-relate-to-input-formatters)
12. [Q12. What is an output formatter?](#q12-what-is-an-output-formatter)
13. [Q13. What is the difference between JSON and XML responses in Web APIs?](#q13-what-is-the-difference-between-json-and-xml-responses-in-web-apis)
14. [Q14. What does `[JsonPropertyName]` do?](#q14-what-does-jsonpropertyname-do)
15. [Q15. What is `AddJsonOptions` used for?](#q15-what-is-addjsonoptions-used-for)
16. [Q16. When would you register a custom output formatter?](#q16-when-would-you-register-a-custom-output-formatter)
17. [Q17. What is the default response format for ASP.NET Core Web API?](#q17-what-is-the-default-response-format-for-aspnet-core-web-api)
18. [Q18. What is the difference between serialization and model binding?](#q18-what-is-the-difference-between-serialization-and-model-binding)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is content negotiation in ASP.NET Core Web API?

**Concepts**
- Accept header — client media type preference
- Output formatter pipeline selection
- 406 Not Acceptable — negotiation failure
- `[Produces]` — action-level format constraint
- Input formatter selection via Content-Type

**Answer**

Content negotiation is the mechanism by which the server selects a response format based on the client's `Accept` header and the output formatters registered in the application. When a client sends `Accept: application/json`, the framework walks the registered output formatters looking for one that can produce that media type, so the server only serializes in formats it has been explicitly configured to handle. If no formatter matches and fallback is disabled, the server returns 406 Not Acceptable rather than silently defaulting to JSON. The input side works the same way in reverse — the `Content-Type` header on the request body selects the input formatter for deserialization. ASP.NET Core 8 defaults to JSON only; XML, CSV, and other formats require explicit formatter registration.

---

## Q2. What is the Accept HTTP header used for?

**Concepts**
- Accept header — client response format preference
- Quality values (`q=`) — preference ordering
- Output formatter selection at content negotiation
- 406 Not Acceptable — unsatisfied Accept
- `*/*` — wildcard Accept from browsers

**Answer**

The Accept header tells the server which response media types the client prefers, ordered by quality values such as `Accept: application/json, application/xml;q=0.9`. The framework picks the highest-preference type it can produce with a registered output formatter, so if the server has only a JSON formatter and the client lists XML first at full quality, the server either falls back to JSON or returns 406 depending on configuration. Accept governs the response format only — it says nothing about the request body format, which is the job of Content-Type. API clients should set Accept explicitly because browsers default to `*/*`, which accepts any format.

---

## Q3. What is the Content-Type header used for in API requests?

**Concepts**
- Content-Type — request body media type declaration
- Input formatter selection
- 415 Unsupported Media Type — mismatched Content-Type
- `[Consumes]` — action-level input type constraint
- `multipart/form-data` for file uploads

**Answer**

The Content-Type header declares the media type of the request body so the server can select the correct input formatter for deserialization. It is required on POST, PUT, and PATCH requests that include a body — `Content-Type: application/json` routes the body to the `System.Text.Json` input formatter, while `Content-Type: application/xml` requires a registered XML formatter. A mismatch between the Content-Type value and the actual body format causes model binding failures or a 415 Unsupported Media Type response. Applying `[Consumes("application/json")]` on an action rejects requests with other Content-Type values before binding even starts, which is useful for isolating endpoints that must only accept a specific format.

---

## Q4. What is the default JSON serializer in ASP.NET Core 8?

**Concepts**
- `System.Text.Json` — default serializer since ASP.NET Core 3+
- `AddControllers()` — registers JSON formatters automatically
- `AddJsonOptions()` — global serializer configuration
- camelCase naming policy — default in Web API templates
- `AddNewtonsoftJson()` — opt-in replacement

**Answer**

ASP.NET Core 8 uses `System.Text.Json` as the default JSON serializer for both input and output formatters, registered automatically when calling `AddControllers()`. It is configured via `AddJsonOptions()` which exposes `JsonSerializerOptions`, and the default naming policy applies camelCase to property names in all JSON responses. The reason Microsoft moved to `System.Text.Json` is performance — it is faster and allocates less memory than Newtonsoft.Json for typical API payloads since it is built on `Span<T>` and avoids boxing. I would add `AddNewtonsoftJson()` only when a project genuinely needs legacy features like `$type`-based polymorphism or specific custom converters that have no equivalent in `System.Text.Json`.

---

## Q5. What is camelCase JSON naming and why is it used in Web APIs?

**Concepts**
- `JsonNamingPolicy.CamelCase` — default naming policy
- PascalCase C# properties vs camelCase JSON keys
- `PropertyNameCaseInsensitive` — deserialization matching
- `[JsonPropertyName]` — per-property override
- OpenAPI schema naming alignment

**Answer**

camelCase naming serializes C# PascalCase property names — `CustomerName` — to lowercase-first JSON keys — `"customerName"` — which matches JavaScript and front-end conventions. ASP.NET Core 8 applies this automatically via `JsonNamingPolicy.CamelCase` so the React or Angular client can read `response.customerName` without manual mapping. The inbound side is case-sensitive by default, which means a legacy client sending `"CustomerName"` in the JSON body gets a null binding unless `PropertyNameCaseInsensitive = true` is configured in `AddJsonOptions`. OpenAPI/Swagger schemas reflect the camelCase keys, so generated TypeScript or C# clients also use camelCase — consistency all the way through avoids silent data loss at runtime.

---

## Q6. What is the difference between input formatters and output formatters?

**Concepts**
- Input formatters — request body deserialization
- Output formatters — response body serialization
- Content-Type selects input formatter
- Accept header selects output formatter
- `[FromBody]` triggers input formatter path

**Answer**

Input formatters deserialize the request body into action parameters before the action runs, selected based on the request's Content-Type header. Output formatters serialize the action result into the response body after the action executes, selected based on the Accept header and any `[Produces]` constraint. The two sets operate independently — an action can consume JSON via an input formatter and produce CSV via a custom output formatter. Custom formatters implement `InputFormatter` or `OutputFormatter` base classes respectively and are registered in `AddControllers(options => ...)`. A binding failure on the input side returns 400 or 415 before the action runs; a serialization failure on the output side produces a 500 at result execution time.

---

## Q7. What HTTP status code is returned when content negotiation fails?

**Concepts**
- 406 Not Acceptable — unsatisfied Accept header
- 415 Unsupported Media Type — unsupported Content-Type
- `ReturnHttpNotAcceptable` — opt-in strict behavior
- Default JSON fallback — framework default without configuration
- `[Produces]` — documents but does not alone enforce 406

**Answer**

HTTP 406 Not Acceptable is the correct response when the server cannot produce any representation matching the client's Accept header and no fallback formatter is configured. Without explicit configuration, the framework may silently fall back to the default JSON formatter rather than returning 406, which violates HTTP semantics and hides mismatches from clients. I would enable strict behavior via `options.ReturnHttpNotAcceptable = true` in the MVC options so clients reliably detect when they have requested an unsupported format. The input-side equivalent is 415 Unsupported Media Type, returned when the request Content-Type does not match any registered input formatter. Integration tests should assert 406 for unsupported Accept headers on strict APIs since the silent fallback is a common source of contract confusion.

---

## Q8. What does `[Produces("application/json")]` do?

**Concepts**
- `[Produces]` — metadata attribute for OpenAPI and formatter selection
- Output formatter restriction per action
- OpenAPI documentation accuracy
- Mismatch between `[Produces]` and registered formatters
- Does not alone enforce 406

**Answer**

`[Produces]` declares which content types an action can return, influencing both content negotiation and OpenAPI documentation. When only `"application/json"` is listed, the formatter selection pipeline restricts itself to the JSON output formatter for that action, so a CSV formatter registered globally will not accidentally handle that endpoint. The attribute is also metadata — Swashbuckle reads it to populate the response content type in the generated OpenAPI document, which affects the TypeScript clients generated from it. Declaring a type without a corresponding registered formatter misdocuments the API since `[Produces]` does not alone cause 406 — formatter configuration controls whether a fallback is used. I use it primarily to make the OpenAPI contract accurate and to prevent format bleed between endpoints.

---

## Q9. What does `[Consumes("application/xml")]` do?

**Concepts**
- `[Consumes]` — action selection constraint on Content-Type
- 415 Unsupported Media Type — non-matching request
- Input formatter requirement for consumed type
- Isolating legacy XML endpoints from JSON endpoints

**Answer**

`[Consumes]` acts as an action selection constraint — when a request arrives, routing checks whether the request's Content-Type matches any value listed in `[Consumes]` before selecting that action. A request with `Content-Type: application/json` sent to an action marked `[Consumes("application/xml")]` receives 415 Unsupported Media Type rather than reaching the action method. This is useful for maintaining legacy XML endpoints in an otherwise JSON-only API, since the constraint isolates which requests each action handles. The constraint only gates selection — the action still needs a registered XML input formatter to actually deserialize the body, so declaring `[Consumes("application/xml")]` without `AddXmlSerializerFormatters()` will select the action but fail during binding.

---

## Q10. What is the difference between `System.Text.Json` and Newtonsoft.Json in ASP.NET Core?

**Concepts**
- `System.Text.Json` — built-in, high-performance, default
- Newtonsoft.Json — feature-rich, third-party, optional via `AddNewtonsoftJson()`
- Configuration surface — `AddJsonOptions` vs `AddNewtonsoftJson`
- Reference loop handling and `$type` polymorphism — Newtonsoft-only features
- Mixing serializers in one app — inconsistency risk

**Answer**

`System.Text.Json` is the built-in serializer that ships with ASP.NET Core — faster, lower allocation, and strictly standards-compliant with camelCase defaults. Newtonsoft.Json is an opt-in replacement via `AddNewtonsoftJson()` that offers richer features: reference loop handling with `ReferenceLoopHandling.Ignore`, `$type`-based polymorphic deserialization, and a larger ecosystem of community converters. The configuration surfaces are completely separate — `System.Text.Json` uses `AddJsonOptions(o => o.JsonSerializerOptions...)` while Newtonsoft uses `AddNewtonsoftJson(o => o.SerializerSettings...)`, so I would never mix them in the same application since the naming and behavior differences create inconsistent contracts across endpoints. I default to `System.Text.Json` and only reach for Newtonsoft when a specific legacy feature has no equivalent.

---

## Q11. How does model binding relate to input formatters?

**Concepts**
- Model binding — orchestration layer for parameter population
- Input formatters — body deserialization component
- `[FromBody]` — triggers formatter-based binding
- Content-Type — formatter selection key
- Validation runs after binding

**Answer**

Model binding is the orchestration layer that decides where to read each action parameter from — route, query string, header, form, or body. For body parameters marked `[FromBody]`, model binding delegates to input formatters to deserialize the raw request stream into a CLR object, using the request's Content-Type header to select the right formatter. Route and query parameters bind directly without formatters, which is why a simple `int id` from a route template never touches `System.Text.Json`. If no input formatter matches the Content-Type, binding fails with 415 or leaves the model empty. Validation via DataAnnotations and `IValidatableObject` runs after binding completes, so a formatter that returns a syntactically valid but semantically wrong object will pass the formatter phase and only fail at the validation phase.

---

## Q12. What is an output formatter?

**Concepts**
- Output formatter — response body serialization component
- `CanWriteType` — CLR type eligibility check
- `WriteResponseBodyAsync` — response stream writer
- Custom formatter registration in MVC options
- Response Content-Type header set by formatter

**Answer**

An output formatter is the component responsible for serializing an action result object into the HTTP response body in a specific media type. The framework selects an output formatter during result execution by asking each registered formatter's `CanWriteType` whether it handles the result's CLR type, then matches the surviving candidates against the negotiated Accept header. The built-in `JsonOutputFormatter` writes JSON via `System.Text.Json`, and there are built-in formatters for XML and plain strings. Custom formatters extend `TextOutputFormatter` or `OutputFormatter`, override `CanWriteType` to declare which types they handle, and implement `WriteResponseBodyAsync` to write the serialized bytes to the response stream. The formatter is also responsible for setting the response `Content-Type` header — for example, `text/csv; charset=utf-8`.

---

## Q13. What is the difference between JSON and XML responses in Web APIs?

**Concepts**
- JSON — default, compact, JavaScript-native
- XML — verbose, schema-heavy, requires explicit registration
- `AddXmlSerializerFormatters()` — opt-in XML support
- Content negotiation — format selection at runtime
- Enterprise and legacy integration contexts for XML

**Answer**

JSON is the default lightweight text format for modern Web APIs — compact, natively parsed by JavaScript, and supported out of the box by `System.Text.Json` in ASP.NET Core 8. XML is the older, more verbose alternative that requires explicit registration via `AddXmlSerializerFormatters()` because it is not enabled by default. The verbosity of XML (`<CustomerName>Acme</CustomerName>` vs `"customerName":"Acme"`) makes it slower to parse and larger over the wire, which is why it has been largely displaced by JSON in mobile, SPA, and microservice communication. XML still persists in banking, government, and SOAP-adjacent integrations where schemas and namespaces provide contract guarantees. When both are needed I register both formatters and rely on content negotiation so clients declare which format they want via the Accept header.

---

## Q14. What does `[JsonPropertyName]` do?

**Concepts**
- `[JsonPropertyName]` — per-property JSON key override
- Overrides `JsonNamingPolicy` for a single property
- Mixed naming policy — documentation and client impact
- `System.Text.Json`-specific attribute
- OpenAPI schema alignment requirement

**Answer**

`[JsonPropertyName("custom_name")]` overrides the naming policy for a single property, forcing a specific JSON key for both serialization and deserialization regardless of the global `JsonNamingPolicy`. This is useful when integrating with an external schema that mandates specific field names — for example, a payment gateway that requires `"merchant_id"` rather than the camelCase default `"merchantId"`. The downside is that it creates a mixed naming contract: most properties follow camelCase while one uses snake_case, which means the OpenAPI schema must accurately document the exception so generated clients use the correct key. I use it sparingly because inconsistent naming across DTOs confuses code generators and developers who assume one policy applies everywhere.

---

## Q15. What is `AddJsonOptions` used for?

**Concepts**
- `AddJsonOptions` — global `System.Text.Json` configuration
- `JsonSerializerOptions` — serializer behavior settings
- `PropertyNamingPolicy` — naming convention
- `PropertyNameCaseInsensitive` — inbound matching
- Custom converters — `JsonStringEnumConverter` and others

**Answer**

`AddJsonOptions` configures `System.Text.Json` serializer settings globally for all JSON input and output formatters in the application, chained on `AddControllers()` in `Program.cs`. The most common uses are setting the naming policy to `JsonNamingPolicy.CamelCase`, enabling `PropertyNameCaseInsensitive = true` for legacy client compatibility, and registering custom converters such as `JsonStringEnumConverter` to serialize enums as strings rather than integers. Every controller in the application shares the same configuration, which is why I treat it as an application-wide contract decision rather than a per-endpoint tuning knob. Changes here affect inbound deserialization and outbound serialization simultaneously, so I test both directions when I modify these settings.

---

## Q16. When would you register a custom output formatter?

**Concepts**
- Custom output formatter — media type not supported by built-ins
- `OutputFormatter` base class
- `CanWriteType` — CLR type eligibility declaration
- `WriteResponseBodyAsync` — response stream serialization
- `[Produces]` — endpoint pinning to custom media type

**Answer**

I register a custom output formatter when the API must produce a media type that the built-in formatters do not support — CSV exports, PDF reports, Protocol Buffers, or proprietary binary formats. The formatter extends `TextOutputFormatter` or `OutputFormatter`, overrides `CanWriteType` to match only the intended CLR types such as `IEnumerable<ReportRow>`, and implements `WriteResponseBodyAsync` to serialize the object to the response stream. Registration goes in `AddControllers(o => o.OutputFormatters.Add(new CsvOutputFormatter()))` and the endpoint should be pinned with `[Produces("text/csv")]` so the formatter only activates when explicitly negotiated. The `CanWriteType` implementation must be narrow — returning `true` for all types is a common mistake that causes the formatter to intercept responses it cannot handle correctly.

---

## Q17. What is the default response format for ASP.NET Core Web API?

**Concepts**
- JSON default — `application/json` without configuration
- `System.Text.Json` — default serializer
- camelCase naming policy — default property naming
- `Ok(dto)` and `ActionResult<T>` — implicit JSON serialization
- Additional formats require explicit registration

**Answer**

The default response format for ASP.NET Core 8 Web API is JSON serialized by `System.Text.Json` with camelCase property naming, requiring no additional configuration beyond the standard `AddControllers()` call in the project template. Every `Ok(dto)`, `CreatedAtAction(...)`, and implicit `ActionResult<T>` return value is serialized to JSON and the response Content-Type header is set to `application/json; charset=utf-8`. Clients that omit the Accept header receive JSON since it is the first registered output formatter. XML, CSV, and any other formats must be explicitly opted into by registering additional formatters — the framework does not guess at alternative representations. This applies to both controller-based APIs and minimal APIs.

---

## Q18. What is the difference between serialization and model binding?

**Concepts**
- Serialization — CLR object to response body (outbound)
- Model binding — request data to CLR object (inbound)
- Input formatters — body deserialization within binding
- Validation — runs after binding, before action executes
- Failure modes — 400/415 for binding vs 500 for serialization

**Answer**

Serialization is an outbound operation — it converts a CLR object to a byte stream written to the HTTP response body via an output formatter after the action executes. Model binding is an inbound operation — it populates action parameters from the HTTP request before the action runs, reading from route values, query strings, headers, and the request body. Body deserialization is one step within model binding, performed by input formatters when the parameter is marked `[FromBody]`. Validation runs after model binding completes and before the action method body executes. The failure modes are distinct: a binding or deserialization failure returns 400 Bad Request or 415 Unsupported Media Type before the action runs, while a serialization failure at result execution time produces a 500 because the action has already completed.

---

## Gotchas — Content Negotiation & Formatters (Interview Traps)

---

#### Gotcha 1. XML formatter not registered but `Accept: application/xml` sent

**Concepts**
- `AddControllers()` registers only `System.Text.Json` by default
- XML output formatter requires explicit `AddXmlSerializerFormatters()` or `AddXmlDataContractSerializerFormatters()`
- No XML formatter available → falls back to JSON, not 406, by default
- `[Produces("application/xml")]` without registered formatter — documented but undeliverable

**Answer**

ASP.NET Core does not register an XML output formatter by default — calling `AddControllers()` only adds the `System.Text.Json` formatter. When a client sends `Accept: application/xml`, the framework finds no matching formatter and falls back to JSON rather than returning `406 Not Acceptable`. The OpenAPI document may advertise XML support via `[Produces]` attributes while the API silently serves JSON to every XML-accepting client. If XML is genuinely required I add `.AddXmlSerializerFormatters()` and write an integration test confirming `Content-Type: application/xml` in the response.

---

#### Gotcha 2. `ReturnHttpNotAcceptable` not enabled — 406 never returned

**Concepts**
- Default behavior — fall back to first registered formatter when no Accept match
- `options.ReturnHttpNotAcceptable = true` — enables strict 406 on unsatisfied Accept
- Client silence vs 406 for unsatisfied format requests
- `[Produces]` does not alone enforce 406

**Answer**

Without `options.ReturnHttpNotAcceptable = true` in `AddControllers(options => ...)`, the content negotiation pipeline falls back to the first registered formatter when it cannot satisfy the client's `Accept` header — the client receives JSON when it requested CSV or XML with no indication that the format is unsupported. I enable `ReturnHttpNotAcceptable = true` for strict REST compliance so clients receive `406 Not Acceptable` when the requested format is unavailable, which drives them to fix their client rather than silently consuming wrong-format responses.

---

#### Gotcha 3. `[Produces]` mismatch with actually registered formatters

**Concepts**
- `[Produces("application/json", "application/xml")]` — documents intent in OpenAPI
- `[Produces]` does not register a formatter or guarantee the format is producible
- OpenAPI spec advertising XML while only JSON is registered
- False contract — generated clients attempt XML that API cannot produce

**Answer**

`[Produces]` is a documentation and formatter-selection attribute — it narrows which registered formatters may be used and documents the content types in OpenAPI. It does not register a formatter or guarantee the format can be produced. Declaring `[Produces("application/json", "application/xml")]` when only the JSON formatter is registered causes Swagger to advertise XML as a valid response type, breaking code-generated clients that request XML and receive JSON or 406. I keep `[Produces]` in sync with the registered formatters and add an integration test for every advertised content type.

---

#### Gotcha 4. Custom formatter's `CanWriteType` returning `true` for all types

**Concepts**
- `CanWriteType(Type type)` — formatter eligibility gate called before selection
- Returning `true` for all types — formatter intercepts requests it cannot serialize
- Unsafe cast in `WriteResponseBodyAsync` fails for non-target types
- `IsAssignableFrom` — correct type check for collection subtypes

**Answer**

A custom `TextOutputFormatter` that returns `true` from `CanWriteType` for every CLR type becomes eligible for every response including `ProblemDetails`, strings, and other types it cannot handle. When selected for an incompatible type, the `WriteResponseBodyAsync` method throws `InvalidCastException` or produces a corrupt body. The correct implementation is `return typeof(IEnumerable<MyDto>).IsAssignableFrom(type)` so the formatter activates only for the types it knows how to serialize, and `List<MyDto>`, `MyDto[]`, and other collection subtypes are accepted via `IsAssignableFrom` without a direct equality check.

---

#### Gotcha 5. Missing `Vary: Accept` header when multiple representations exist

**Concepts**
- `Vary: Accept` — tells caches that the response varies by the Accept header
- Without it — shared cache serves cached JSON to an XML-requesting client
- CDN and proxy cache keying only on URL by default
- RFC 7234 — `Vary` required when content negotiation affects response

**Answer**

When an API supports multiple output formats, a shared cache (CDN or reverse proxy) that does not see `Vary: Accept` keys only on the URL and may serve the cached JSON response to a subsequent XML-requesting client. The XML client receives the wrong format silently. I add `Vary: Accept` to any endpoint that performs content negotiation with multiple registered formatters, either in the action response or at the middleware/proxy layer. Endpoints that produce only JSON do not need `Vary: Accept`, but documenting the single format with `[Produces("application/json")]` prevents this confusion.

---

#### Gotcha 6. Confusing `Content-Type` (inbound) with `Accept` (outbound) direction

**Concepts**
- `Content-Type` header — describes the format of the request body (inbound, for input formatters)
- `Accept` header — client's preferred response format (outbound, for output formatters)
- `[Consumes]` — constrains which `Content-Type` values are accepted as input
- `[Produces]` — declares what `Accept` values can be satisfied as output

**Answer**

`Content-Type` is a property of the request body and drives input formatter selection — the server reads it to know how to deserialize the inbound payload. `Accept` is the client's preferred response format and drives output formatter selection — the server reads it to know how to serialize the response. Mixing them up causes real bugs: adding `[Consumes("application/json")]` to restrict which clients can POST is correct; using `[Produces("application/json")]` to control deserialization is wrong. `[Consumes]` on an action also acts as a route constraint — a POST with the wrong `Content-Type` receives `415 Unsupported Media Type` before reaching the action body.

---

#### Gotcha 7. `ProblemDetails` not served as `application/problem+json`

**Concepts**
- RFC 7807 media type — `application/problem+json` for error responses
- Default `application/json` returned even for ProblemDetails bodies
- `AddProblemDetails()` in ASP.NET Core 7+ configures the correct media type
- Clients checking `Content-Type` to distinguish errors from success payloads

**Answer**

RFC 7807 mandates `Content-Type: application/problem+json` for `ProblemDetails` responses so clients can distinguish error shapes from success shapes by media type alone. Without calling `builder.Services.AddProblemDetails()`, error responses may be served with the default `application/json` content type, breaking clients that key on the media type to choose their error-handling path. `AddProblemDetails()` and `IExceptionHandler` together ensure that validation errors, unhandled exceptions, and explicit `ProblemDetails` results all carry the correct `application/problem+json` media type.

---

#### Gotcha 8. `System.Text.Json` vs Newtonsoft.Json default behavior differences

**Concepts**
- `System.Text.Json` default — camelCase output, case-sensitive input, no reference loop handling
- Newtonsoft.Json default — PascalCase output, case-insensitive input, reference loop ignore
- Migration from Newtonsoft — existing clients sending PascalCase keys break silently
- `AddNewtonsoftJson()` opt-in to restore Newtonsoft behavior in Swashbuckle

**Answer**

ASP.NET Core 8 defaults to `System.Text.Json`, which differs from Newtonsoft.Json in three ways that break existing clients: it outputs camelCase instead of PascalCase, performs case-sensitive deserialization by default, and throws on circular object references instead of ignoring them. A migration from `AddNewtonsoftJson()` to the default serializer causes existing partners sending PascalCase payloads to receive silent null bindings on every field. I evaluate the client impact before removing `AddNewtonsoftJson()`, enable `PropertyNameCaseInsensitive = true` during migration, and validate with contract tests against each known consumer.

---

#### Gotcha 9. Text encoding not specified in custom `TextOutputFormatter`

**Concepts**
- `TextOutputFormatter` requires `SupportedEncodings` populated in constructor
- Empty `SupportedEncodings` — formatter never selected by the pipeline
- `Encoding.UTF8` vs `new UTF8Encoding(false)` — BOM presence difference
- `WriteResponseBodyAsync(context, encoding)` — uses the negotiated encoding

**Answer**

A custom `TextOutputFormatter` subclass that does not add any encoding to `SupportedEncodings` in its constructor will never be selected by the content negotiation pipeline, silently falling through to the next formatter. I add at least `Encoding.UTF8` in the constructor: `SupportedEncodings.Add(Encoding.UTF8)`. The `WriteResponseBodyAsync(context, encoding)` override receives the negotiated encoding and must use it to write the response, rather than hard-coding a new `StreamWriter(context.HttpContext.Response.Body)` which bypasses the negotiated charset.

---

#### Gotcha 10. Output formatter intercepts responses it should not handle

**Concepts**
- Formatter added to end of `OutputFormatters` list — lower priority than built-ins
- `CanWriteType` and `CanWriteResult` both gates for formatter selection
- `OutputFormatters.Insert(0, ...)` — highest priority, may intercept all responses
- `[Produces]` on controller/action constraining formatter choice to declared types

**Answer**

Inserting a custom formatter at index 0 with `options.OutputFormatters.Insert(0, new MyFormatter())` gives it the highest priority and causes it to be evaluated first for every response, including `ProblemDetails`, `string`, and `Stream` results. If `CanWriteType` is not sufficiently narrow, the formatter intercepts responses it cannot handle, producing garbled output or exceptions. I add custom formatters with `options.OutputFormatters.Add(...)` (lowest priority) and gate selection tightly in `CanWriteType`, then use `[Produces("text/csv")]` on specific actions to force selection for those endpoints rather than competing with the built-in formatters globally.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Review serialization for a dual-client API. The JavaScript web app binds correctly; a legacy integration test sends PascalCase JSON and `CustomerName` arrives null.

```csharp
// Program.cs — template defaults only
builder.Services.AddControllers();

public class CreateCustomerRequest
{
    public string CustomerName { get; set; } = "";
    public decimal CreditLimit { get; set; }
}

[HttpPost]
public IActionResult Create([FromBody] CreateCustomerRequest request)
{
    if (string.IsNullOrEmpty(request.CustomerName))
        return BadRequest("CustomerName required");
    return Ok(_svc.Create(request));
}
```

Client body: `{ "CustomerName": "Acme", "CreditLimit": 5000 }`

---

**Concepts**
- `System.Text.Json` camelCase default — case-sensitive inbound matching
- Silent binding failure — PascalCase keys arrive as null/default
- `PropertyNameCaseInsensitive` — migration compatibility option
- `[Required]` — catches missing values but not wrong-case keys
- Breaking contract change from Newtonsoft defaults

**Answer**

The root cause is that `System.Text.Json` defaults to case-sensitive matching under the camelCase policy, so the PascalCase key `"CustomerName"` does not match the expected `"customerName"` key during deserialization. The property binds as empty string, the null check fires, and the request fails with 400 even though the client sent the correct data. The JavaScript web app works because it already sends camelCase keys that match the policy.

The fastest fix without touching the client is `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` in `Program.cs`, which makes deserialization accept both `"customerName"` and `"CustomerName"`. The right long-term fix is to have the legacy client adopt camelCase, documenting the change in an API changelog rather than relying on server-side tolerance indefinitely. I would also replace the manual empty-string check with `[Required]` on `CustomerName` so the error response becomes a proper `ValidationProblemDetails` 400 rather than a plain string.

---

#### Q2. (R) Review this export endpoint. A partner sends `Accept: application/xml` but always receives JSON with HTTP 200.

```csharp
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    [HttpGet("{id:int}/export")]
    public IActionResult Export(int id)
    {
        var report = _reports.Build(id);
        return Ok(report);
    }
}
```

`Program.cs` calls `AddControllers()` only — no XML formatters registered.

---

**Concepts**
- XML output formatter — not registered by default
- Content negotiation fallback — JSON returned when XML unavailable
- `AddXmlSerializerFormatters()` — opt-in XML support
- `ReturnHttpNotAcceptable` — strict 406 enforcement
- `[Produces]` — documenting supported formats

**Answer**

Because only `System.Text.Json` is registered, the framework has no XML output formatter to satisfy `Accept: application/xml`. Rather than returning 406 Not Acceptable, the default behavior falls back to the first available formatter — JSON — so the partner receives a JSON body with a 200 status and no indication that their preferred format was not honored. This violates HTTP semantics and breaks downstream XML parsers silently.

The fix depends on whether XML support is actually required. If the partner needs XML I add `builder.Services.AddControllers().AddXmlSerializerFormatters()` and add an integration test that sends `Accept: application/xml` and asserts the response Content-Type and body shape. If the API is JSON-only I should add `[Produces("application/json")]` and configure `options.ReturnHttpNotAcceptable = true` in MVC options so the partner receives 406 and knows to update their client rather than silently consuming wrong-format responses.

---

#### Q3. (R) Review content negotiation failure handling. QA expects HTTP 406 when an unsupported `Accept` header is sent; API returns JSON 200.

```csharp
[HttpGet("{id:int}")]
[Produces("application/json")]
public IActionResult Get(int id)
{
    var dto = _svc.Get(id);
    return Ok(dto);
}
```

Request: `GET /api/items/1` with header `Accept: application/pdf`

---

**Concepts**
- `[Produces]` — documents intent, does not alone enforce 406
- `ReturnHttpNotAcceptable` — MVC option for strict negotiation
- Default formatter fallback — JSON returned when no match
- 406 Not Acceptable — correct response for unsatisfied Accept

**Answer**

`[Produces("application/json")]` is a metadata attribute that documents what the action can produce and constrains formatter selection to JSON, but it does not by itself cause the framework to return 406 when the client requests a format it cannot satisfy. Without `options.ReturnHttpNotAcceptable = true` in the MVC formatter options, the pipeline falls back to the first registered formatter — JSON — and returns 200 even when the client requested PDF.

The fix is to add `builder.Services.AddControllers(options => options.ReturnHttpNotAcceptable = true)`, which tells the pipeline to return 406 rather than fall back when no registered formatter can satisfy the Accept header. I keep `[Produces("application/json")]` on the action because it accurately documents the contract and informs the OpenAPI document, and I add an integration test that sends `Accept: application/pdf` and asserts the 406 response code.

---

#### Q4. (P) Explain how `System.Text.Json` camelCase naming is configured in ASP.NET Core 8 Web APIs, and why `[JsonPropertyName("customer_name")]` on one property affects the whole contract story.

---

**Concepts**
- `JsonNamingPolicy.CamelCase` — default naming applied globally
- `[JsonPropertyName]` — per-property override of naming policy
- Mixed naming contract — some camelCase, one snake_case
- OpenAPI schema accuracy — generated clients must match actual keys
- `PropertyNameCaseInsensitive` — inbound matching tolerance

**Answer**

The Web API template configures camelCase via `builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)`, which applies to every controller in the application. On the outbound side `CustomerName` serializes as `"customerName"`, and on the inbound side deserialization expects `"customerName"` unless `PropertyNameCaseInsensitive = true` relaxes that.

When `[JsonPropertyName("customer_name")]` appears on one property, it overrides the naming policy for that property only, so the JSON contract becomes a mix: `"customerName"` for most fields and `"customer_name"` for that one. The problem is that code generators reading the OpenAPI schema must document this exception accurately — if the schema shows `"customerName"` but the actual JSON key is `"customer_name"`, generated TypeScript clients will send the wrong key and get silent null bindings. I use `[JsonPropertyName]` only when integrating with external systems that mandate specific field names, and I make sure the OpenAPI document reflects the override so every generated client uses the correct key.

---

#### Q5. (R) Review custom formatter registration. CSV downloads work locally but return empty bodies in staging — logs show formatter selected but model type mismatch.

```csharp
// Program.cs
builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Add(new CsvOutputFormatter());
});

public class CsvOutputFormatter : TextOutputFormatter
{
    public CsvOutputFormatter()
    {
        SupportedMediaTypes.Add("text/csv");
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type) => true;

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
    {
        var rows = (IEnumerable<OrderDto>)context.Object!;
        await context.HttpContext.Response.WriteAsync(ToCsv(rows));
    }
}

[HttpGet("export")]
[Produces("text/csv")]
public IActionResult Export() => Ok(_orders.All());
```

---

**Concepts**
- `CanWriteType` — must declare supported CLR types narrowly
- Unsafe cast in `WriteResponseBodyAsync` — runtime type mismatch
- `IEnumerable<OrderDto>` vs wrapper types — return value shape
- `context.Object` — actual value after `OkObjectResult` unwrapping
- `IsAssignableFrom` — correct type eligibility check

**Answer**

The bug is that `CanWriteType` returns `true` for every CLR type, so the formatter activates even when `context.Object` is not `IEnumerable<OrderDto>`. In staging, the action may return a slightly different type — a `List<OrderDto>`, a `PagedResult<OrderDto>`, or a wrapper — causing the hard cast to throw `InvalidCastException` or produce an empty body depending on how the exception is swallowed. Locally the test data happened to match the exact type, masking the problem.

The correct `CanWriteType` implementation is `return type != null && typeof(IEnumerable<OrderDto>).IsAssignableFrom(type)`, which correctly accepts `List<OrderDto>` as a valid subtype. Inside `WriteResponseBodyAsync` I replace the hard cast with a pattern match and add a null/empty check before calling `ToCsv`. The `[Produces("text/csv")]` on the action is correct and should stay so the formatter only activates when the client explicitly requests CSV — it prevents the formatter from intercepting JSON-accepting clients.

---

#### Q6. (M) A bank middleware still requires SOAP/XML for one endpoint while the rest of the platform is JSON. Where do input and output formatters sit in the pipeline relative to model binding, and what does `[Consumes("application/xml")]` change?

---

**Concepts**
- Input formatter — selected by Content-Type at model binding stage
- Output formatter — selected by Accept at result execution stage
- `[Consumes]` — action selection constraint, not just binding hint
- `AddXmlSerializerFormatters()` — opt-in for both input and output XML
- Formatter isolation — registering XML for specific controllers only

**Answer**

Input formatters sit within the model binding phase, which runs after routing selects an endpoint and before the action method executes. Model binding calls the input formatter selected by the request's Content-Type header to deserialize the body into the action parameter — so `Content-Type: application/xml` routes the body to the XML input formatter if one is registered. Output formatters run later, during result execution after the action returns, and select the serializer based on the Accept header and `[Produces]` constraints.

`[Consumes("application/xml")]` does more than hint at the expected format — it is an action selection constraint, meaning routing uses it to decide which action matches a request. A JSON POST to that route returns 415 Unsupported Media Type and never reaches the action method body. For the bank endpoint, I would register `AddXmlSerializerFormatters()` globally and use `[Consumes("application/xml")]` on the legacy endpoint to isolate XML intake from the JSON-only endpoints. For output, `[Produces("application/xml")]` on the same action ensures the response is XML regardless of the client's Accept header. If I want to avoid adding XML globally, I can register a custom XML formatter scoped to that controller only using a controller-level filter or a dedicated minimal controller.

---

#### Q7. (D) Leadership wants to drop XML support to reduce maintenance. One state-government client still posts `application/xml` to `POST /api/permits`. How do you decide retire vs adapter vs gateway translation?

---

**Concepts**
- Retire XML — requires contractual agreement and sunset deadline
- Adapter service — sidecar converting XML to JSON
- Gateway translation — APIM/nginx payload transformation
- In-app `AddXmlSerializerFormatters` — simplest but spreads legacy into codebase
- Deprecation signaling — `Sunset` and `Deprecation` headers

**Answer**

The decision turns on three inputs: whether the government contract has a migration clause, how long the transition period needs to be, and how much operational complexity the team can absorb. If the contract allows it and the client can migrate, I announce a sunset date via `Deprecation: true` and `Sunset: <date>` headers on the endpoint, provide a migration guide, and monitor usage until the date passes.

When the client cannot migrate on the API team's timeline, an adapter service is my preference over in-app formatters — a small sidecar converts incoming XML to a JSON DTO and calls the main API internally, keeping the JSON core clean and fully testable without the XML path in the main codebase. A gateway translation at APIM or nginx is operationally simpler to deploy but pushes XML schema knowledge into the ops layer, where schema changes are harder to test and own. Keeping `AddXmlSerializerFormatters` in the main app is the lowest-friction short-term fix but accumulates maintenance debt if XML support expands to more endpoints. I would document whichever approach is chosen in the ADR so the ownership is clear when the government client eventually migrates.

---

#### Q8. (R) Review `[Produces]` and `[ProducesResponseType]` usage. Swagger advertises XML and JSON responses; production returns JSON only and clients cache wrong content type.

```csharp
[ApiController]
[Route("api/catalog")]
[Produces("application/json", "application/xml")]
public class CatalogController : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public IActionResult Get(int id) => Ok(_catalog.Get(id));
}
```

No XML serializer configured; `AddControllers()` without `AddXmlSerializerFormatters()`.

---

**Concepts**
- `[Produces]` mismatch — declared format without registered formatter
- False OpenAPI contract — Swagger advertises XML that cannot be produced
- `Vary: Accept` — required when multiple representations exist
- Cache poisoning — shared cache serves JSON to XML-expecting clients
- `[Produces]` accuracy — must reflect registered formatters

**Answer**

`[Produces("application/json", "application/xml")]` on the controller tells Swashbuckle to document both content types in the OpenAPI spec, so generated clients and the Swagger UI show XML as a valid response format. Since no XML formatter is registered, every request receives JSON regardless of the Accept header — clients expecting XML receive JSON, and any caching proxy that does not include `Vary: Accept` will cache the JSON body and serve it to subsequent XML-accepting clients as well.

The immediate fix is to remove `"application/xml"` from `[Produces]` so the OpenAPI contract only advertises JSON, which is what the action actually produces. If XML support is genuinely needed, I add `AddXmlSerializerFormatters()` and add an integration test with `Accept: application/xml` asserting the correct response Content-Type and body. I also add `Response.Headers["Vary"] = "Accept"` or configure it at the reverse proxy so caches use the Accept header as part of the cache key when multiple representations exist.
