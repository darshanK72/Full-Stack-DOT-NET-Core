# OData APIs — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — OData APIs (Interview Traps)](#gotchas--odata-apis-interview-traps)

---

## Gotchas — OData APIs (Interview Traps)

---

#### Gotcha 1. Unrestricted $expand Traverses the Entity Graph Across Security Boundaries

**Concepts**
- $expand follows navigation properties declared in the Entity Data Model
- A single $expand request can traverse multiple relationship levels and return unintended data
- Row-level filtering on the root entity does not automatically filter expanded child entities
- MaxExpansionDepth in ODataValidationSettings restricts traversal depth

**Answer**

OData's `$expand` option instructs the server to include related entities inline: `GET /api/orders?$expand=customer($expand=paymentMethods)` can return order records, their associated customers, and each customer's stored payment methods in a single request. If the IQueryable is simply `_context.Orders` without ownership filtering, an authenticated user who should only see their own orders can use `$expand` to walk navigation properties and read data belonging to other tenants. The mitigation requires two layers: first, apply row-level filtering before the OData pipeline processes the query options (`_context.Orders.Where(o => o.UserId == currentUserId)`); second, restrict expansion depth and allowed navigation properties using `ODataValidationSettings.MaxExpansionDepth` and by limiting the navigation properties exposed in the EDM.

---

#### Gotcha 2. $expand Without EF Core Include() Causes N+1 Queries

**Concepts**
- OData $expand does not automatically translate to SQL JOIN or EF Core Include()
- Without Include(), each expanded entity triggers a separate lazy-load database query
- With Microsoft.AspNetCore.OData and EF Core, $expand translates to Include() when the provider supports it
- Testing with small datasets masks the N+1 problem until production scale

**Answer**

When OData processes a `$expand` request and the underlying IQueryable is an EF Core query, OData translates the `$expand` into EF Core `Include()` calls — but only when the OData-to-EF Core translation is properly configured and the navigation properties are correctly declared in the EDM. If the translation fails silently (due to a missing navigation property in the EDM, or an unsupported query shape), EF Core falls back to lazy loading and issues one SELECT per parent entity to load the expanded child collection. With 1,000 root entities, this produces 1,000 extra database round trips. This is identical to the N+1 problem in GraphQL resolvers and has the same fix: ensure the OData-to-EF Core query translation is exercising SQL JOINs by logging the generated SQL and verifying the `$expand` results in a single query or a small number of queries.

---

#### Gotcha 3. [EnableQuery] Applied to a Non-IQueryable Return Type Has No Effect

**Concepts**
- EnableQueryAttribute transforms the return IQueryable using the OData query options in the URL
- Methods returning IEnumerable<T>, List<T>, or already-materialised collections are not filtered server-side
- The client receives the full collection and filtering happens in memory, or not at all
- OData query processing must happen before materialisation to push predicates to the database

**Answer**

`[EnableQuery]` intercepts the action result and applies OData query options (`$filter`, `$select`, `$orderby`, `$top`, `$skip`) to the IQueryable before it is executed against the database. If the action method returns `IEnumerable<T>`, `List<T>`, or any collection that has already been materialised (e.g., the result of `ToList()` or `ToArray()`), `[EnableQuery]` has nothing to filter — the database query has already executed without the OData predicates. All records are fetched from the database and the client receives either the full unfiltered result or a client-side in-memory filtered subset, depending on the OData provider. The action must return `IQueryable<T>` to allow `[EnableQuery]` to compose OData operators into the SQL query before materialisation.

---

#### Gotcha 4. OData Metadata Endpoint ($metadata) Exposes the Full Entity Data Model to Unauthenticated Clients

**Concepts**
- OData services publish a $metadata endpoint that describes all entity types, properties, and relationships
- The endpoint is used by Excel, Power BI, and OData client generators to discover the schema
- By default, $metadata is accessible without authentication
- Schema discovery by attackers reveals internal entity names, relationship structures, and field names

**Answer**

OData's `$metadata` endpoint returns an XML document (EDMX) that completely describes the service's entity model: all entity type names, property names, property types, navigation properties, and their cardinalities. This is intended for client tooling — Excel add-ins, Power BI, auto-generated client libraries — but it also provides an attacker with a complete blueprint of the API's data model without requiring any authenticated calls. Restricting the metadata endpoint with `[Authorize]` or disabling it for internet-facing APIs reduces information disclosure. For internal APIs where client tooling needs the schema, requiring authentication on the `$metadata` path protects the schema from unauthenticated enumeration. In ASP.NET Core OData, the metadata endpoint can be disabled in the ODataOptions configuration.

---

#### Gotcha 5. OData Routing Conflicts with Conventional MVC Routing When Both Are Registered

**Concepts**
- OData routing uses its own convention-based route prefix separate from MVC attribute routing
- Registering both OData and conventional MVC routes on the same controller causes route conflicts
- OData controllers must inherit from ODataController, not ControllerBase, for OData routing to apply
- Mixed OData and non-OData endpoints in the same controller can produce 404s or incorrect route matching

**Answer**

ASP.NET Core OData registers its own route convention alongside the standard MVC routing system. When a controller inherits from `ODataController` and is also annotated with `[Route]` attributes for conventional API routes, the routing conventions conflict. Requests to `/api/products?$filter=...` may match the OData convention or the attribute route depending on registration order, leading to 404 responses or incorrect handler invocation. OData controllers should inherit from `ODataController` (or `ControllerBase` with explicit OData route configuration), and non-OData endpoints should live in separate controllers. Calling `MapControllers()` without calling `AddRouteComponents()` for OData (or vice versa) also produces silent routing failures where OData query options are ignored.

---

#### Gotcha 6. $count with $filter Requires a Separate Database Round Trip Without Special Configuration

**Concepts**
- $count returns the total number of matching entities, often combined with $top/$skip for paging
- Without OData query option support, $filter and $count issue separate database queries
- EnableQuery with PageSize forces $top/$skip but does not automatically include $count in the same query
- Using the Count() and Where() composably on IQueryable enables single-query count+data responses

**Answer**

OData paging typically combines `$top` and `$skip` with `$count=true` to return a page of results and the total matching item count in a single response. Without careful IQueryable composition, the server issues one query for the filtered and paged data and a separate `COUNT(*)` query for the total. On tables with millions of rows, the count query can be slower than the data query and adds latency. Some OData implementations also separate the queries, returning an inconsistent count if records are inserted between the two queries. Using EF Core's ability to compose both operations on the same IQueryable — or using the OData `$count` option composably with the data query — ensures a consistent and efficient single-database interaction. Checking the generated SQL in development confirms whether the count is being fetched in one round trip or two.

---

#### Gotcha 7. AllowedQueryOptions Not Restricted Allows Clients to Use $select * and Bypass Field-Level Security

**Concepts**
- OData $select allows clients to choose which fields are returned in the response
- Without restrictions, $select * or no $select clause returns all entity properties
- Sensitive fields (internal IDs, audit timestamps, PII) should be excluded from the EDM or restricted by policy
- ODataValidationSettings.AllowedQueryOptions restricts which OData clauses are permitted

**Answer**

OData's `$select` clause lets clients request specific fields, which reduces bandwidth. Without explicit restrictions, a client can omit `$select` entirely and receive all entity properties, or use `$select=*` to explicitly request all fields. If the entity model exposes sensitive properties — internal surrogate keys, audit columns, soft-delete flags, PII — all authenticated users receive them without any additional access check. The first line of defence is to exclude sensitive properties from the EDM using `.Ignore(model => model.InternalField)` in the model builder. For runtime-per-user restrictions, a custom `ODataQueryValidator` or result-shaping middleware can strip fields based on the caller's claims after query execution. `ODataValidationSettings.AllowedQueryOptions` restricts which OData operators are permitted but does not restrict individual field access.

---

#### Gotcha 8. OData Patch with Delta<T> Does Not Validate Non-Nullable Properties for Absent Values

**Concepts**
- Delta<T> tracks which properties are explicitly set in a PATCH request
- Properties absent from the PATCH body are not set on the Delta and not applied to the entity
- A PATCH that sets a non-nullable property to null succeeds at the Delta level but fails at the database
- Required property validation must be applied manually against the entity state after Delta.Patch()

**Answer**

OData's `Delta<T>` class enables partial update (PATCH) semantics by tracking only the properties that were explicitly included in the request body. When `delta.Patch(entity)` is called, only the tracked properties are applied; unincluded properties retain their existing values. This works correctly for partial updates but creates a validation gap: if a client sends `{ "name": null }` and `name` is a non-nullable string, `Delta<T>` records the explicit null and applies it, which passes the .NET layer but fails with a database constraint violation when EF Core saves. The error surface is at the database level, not the API validation layer, producing a 500 Internal Server Error rather than a 400 Bad Request. Explicit validation against the entity's data annotations after `delta.Patch(entity)` and before `SaveChanges()` catches these violations and returns a meaningful API error.

---

#### Gotcha 9. Using OData for Simple CRUD APIs Adds Complexity Without Benefit

**Concepts**
- OData's value is in enabling client-driven ad-hoc queries over entity sets
- For fixed-shape CRUD endpoints, OData adds EDM configuration, routing complexity, and $metadata overhead
- OData's query flexibility creates a large attack surface that must be actively secured
- Simple REST APIs are easier to reason about, test, and secure than unrestricted OData surfaces

**Answer**

OData is designed for scenarios where clients have diverse, ad-hoc query requirements — reporting dashboards, Excel add-ins, admin UIs — and where the server team cannot feasibly define a dedicated endpoint for every query combination. For a standard CRUD API with fixed endpoint shapes (create product, get product by ID, update product, delete product), OData introduces the Entity Data Model, specialised routing conventions, `$metadata` exposure, and a broad query surface that must be secured with `AllowedQueryOptions`, `MaxExpansionDepth`, and row-level filtering. The operational overhead of auditing and restricting OData's query capabilities is substantial compared to a simple REST API where each endpoint's behaviour is completely predictable and defined. Teams that adopt OData for simple APIs frequently spend more time securing the query surface than they save in endpoint definition.

---

#### Gotcha 10. OData Paging with PageSize Conflicts with Client-Provided $top and $skip

**Concepts**
- EnableQueryAttribute PageSize forces server-side paging and sets a maximum page size
- When the client also provides $top, the smaller of PageSize and $top is used
- A client that requests $top=1000 receives PageSize (e.g. 100) rows and a nextLink for continuation
- Clients that do not follow nextLink pagination lose records when PageSize is smaller than their $top

**Answer**

Setting `PageSize = 100` on `[EnableQuery]` forces the server to never return more than 100 items per request, regardless of client-specified `$top`. If a client requests `$top=1000` expecting to receive 1,000 records, it receives only 100 with an `@odata.nextLink` pointing to the next page. A client that does not follow pagination links silently receives a truncated result set — 100 records instead of the requested 1,000 — with no error and no indication that records were omitted (beyond the presence of the nextLink). Clients consuming OData endpoints with server-side paging must follow the `@odata.nextLink` chain until no further link is present to retrieve the complete result set. Client code that reads only the first response page and discards the nextLink produces incorrect results that are difficult to diagnose at scale.

---
