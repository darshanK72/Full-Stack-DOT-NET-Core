# GraphQL with HotChocolate — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — GraphQL with HotChocolate (Interview Traps)](#gotchas--graphql-with-hotchocolate-interview-traps)

---

## Gotchas — GraphQL with HotChocolate (Interview Traps)

---

#### Gotcha 1. N+1 Query Problem Occurs When Resolvers Hit the Database Without DataLoader

**Concepts**
- A resolver for a child field runs once per parent object in the result set
- Resolving an "author" field for 50 posts issues 50 separate database queries
- DataLoader batches all accumulated child IDs into a single query before any resolver returns
- The N+1 problem is silent — queries execute and results are correct, only performance suffers

**Answer**

In a GraphQL resolver tree, a field resolver for `Post.author` runs once for every `Post` in the result set. If the query returns 50 posts and each resolver independently calls `dbContext.Authors.FindAsync(post.AuthorId)`, 50 separate round trips are made to the database. The results are correct, but performance degrades proportionally with the result count, making the problem invisible in small tests and catastrophic in production. HotChocolate's DataLoader solves this by collecting all `AuthorId` values accumulated during one execution tick and issuing a single `SELECT * FROM Authors WHERE Id IN (...)`. The DataLoader then distributes the results back to each resolver that requested them. Every resolver that loads a related entity by ID should use a DataLoader rather than direct database access.

---

#### Gotcha 2. Mutations Must Be Used for Side-Effecting Operations — Queries Must Not Mutate State

**Concepts**
- GraphQL spec requires queries to be idempotent and side-effect-free
- Operations that create, update, or delete data must be mutations
- Clients and execution engines may cache or batch queries; mutations are never cached
- Using a query for a write operation can cause double-execution if the client retries

**Answer**

The GraphQL specification defines queries as read-only operations that can be safely parallelised, cached, and retried. Mutations are sequential operations that produce side effects. Using a query field to perform a write — registering a user, sending a message, updating a record — violates the contract and exposes the operation to unintended caching or retry behaviour. A client library that caches query results may cache the "write" response and never execute the operation again on the same arguments. Automatic query batching may execute the query concurrently with itself, running the write twice. HotChocolate enforces sequential mutation execution by default — parallel mutation execution is explicitly opt-in — while queries can run concurrently. Choosing the wrong operation type in schema design is a subtle correctness issue that manifests only under specific client or tooling behaviours.

---

#### Gotcha 3. GraphQL Subscriptions Require WebSocket Transport — They Cannot Run Over HTTP

**Concepts**
- Subscriptions push real-time events from server to client; HTTP request-response cannot model this
- HotChocolate uses the graphql-transport-ws WebSocket subprotocol for subscriptions
- SSE (server-sent events) is an alternative supported in newer versions but requires explicit configuration
- A client that calls a subscription over a standard HTTP fetch will receive a 400 or an immediate completion

**Answer**

GraphQL subscriptions establish a long-lived channel over which the server pushes events to the client. HTTP's request-response model cannot represent this natively — an HTTP response ends when the server writes the final byte. HotChocolate implements subscriptions using the `graphql-transport-ws` WebSocket subprotocol, where the client sends a `subscribe` message and the server streams event frames until the subscription is cancelled or the connection closes. Attempting to execute a subscription via a standard HTTP POST (as one would for queries and mutations) results in an error response. Applications that extend a REST or query-only GraphQL API with subscriptions must ensure the client switches to a WebSocket connection — typically via a split link in Apollo Client or `createClient` in graphql-ws — rather than reusing the HTTP transport used for queries.

---

#### Gotcha 4. Missing AddHttpContextAccessor Causes [Authorize] on Resolvers to Silently Pass All Requests

**Concepts**
- HotChocolate needs IHttpContextAccessor to resolve the current user's ClaimsPrincipal in resolver context
- Without it, IHttpContextAccessor.HttpContext is null inside resolver authorization checks
- The [Authorize] attribute on a type or resolver evaluates against a null principal and may allow access
- services.AddHttpContextAccessor() must be called before services.AddGraphQLServer()

**Answer**

HotChocolate's `[Authorize]` attribute on a resolver or type checks the current user's identity via `IHttpContextAccessor`. If `services.AddHttpContextAccessor()` is not registered, `IHttpContextAccessor.HttpContext` is null inside the resolver execution context, and the authorization check evaluates against a null `ClaimsPrincipal`. Depending on the policy definition, this can silently pass all requests rather than rejecting unauthenticated ones — the opposite of the intended behavior. `services.AddHttpContextAccessor()` must appear in the DI registration before `services.AddGraphQLServer()`. This issue is particularly insidious because the schema registers and queries execute without error in development; only security testing reveals that `[Authorize]` is not actually enforcing access control.

---

#### Gotcha 5. Input Types and Output Types Cannot Use the Same C# Class in HotChocolate

**Concepts**
- HotChocolate distinguishes input types (for arguments) from output types (for query/mutation fields)
- Using the same C# class as both an input and an output type is not allowed
- Separate InputType<T> and ObjectType<T> descriptors must be defined, or separate DTOs used
- Auto-registration attempts to register a class as both types and throws a schema build error

**Answer**

GraphQL has a strict separation between input types — the types used for mutation arguments and query filter parameters — and output types — the types returned in query and mutation results. They cannot be the same type in the schema. In HotChocolate, attempting to use one C# class for both an input argument and a query return field causes a schema build error: "Type `Foo` is registered as InputType and ObjectType." The fix is to create separate DTOs — a `FooInput` class for arguments and a `Foo` class for outputs — or to use HotChocolate's explicit type descriptors to map the same C# class to either role independently. This separation is a fundamental GraphQL design principle and is not specific to HotChocolate.

---

#### Gotcha 6. Query Depth Not Limited Allows Clients to Craft Deeply Nested Queries That Exhaust Resources

**Concepts**
- A GraphQL query can traverse circular or deeply nested type relationships
- A query like { friends { friends { friends { ... } } } } is valid unless depth is limited
- MaxExecutionDepth option in HotChocolate enforces a maximum resolver recursion depth
- Query complexity limits (MaxAllowedComplexity) restrict the overall cost of a single query

**Answer**

GraphQL schemas with recursive or bidirectional relationships — users having friends who are users, categories containing sub-categories — allow clients to write queries of arbitrary depth. A query that requests 10 levels of nested relationships sends 10 rounds of resolver invocations and 10 sets of database queries (without DataLoader). An adversarial or buggy client can write a deeply nested query that exhausts connection pool resources or stack space. HotChocolate's `ModifyRequestOptions(o => o.MaxAllowedExecutionDepth = 10)` setting rejects queries that exceed the depth limit with a validation error before execution begins. Complexity-based limiting — where each field contributes a cost value and the total must not exceed a threshold — provides finer-grained control. Both protections must be configured explicitly; they are not enabled by default.

---

#### Gotcha 7. Nullable GraphQL Fields Cause Null Propagation to Bubble Up Through the Entire Response

**Concepts**
- A non-nullable field that throws or returns null causes the error to propagate up the response tree
- The nearest nullable ancestor is set to null, potentially nullifying large portions of the response
- A non-nullable field error on a root-level field can null out the entire query response
- Deliberately marking fields nullable provides error isolation at the cost of weaker type guarantees

**Answer**

In GraphQL, if a non-nullable field (`String!`) resolves to null or throws an exception, the error propagates upward through the response tree until it reaches the nearest nullable ancestor, which is then set to null. If that nullable ancestor is a root query field, the entire query response data becomes null even though other fields in the query resolved successfully. An unhandled exception in one deeply nested resolver can null out a large portion of an otherwise successful response. Schema designers must deliberately choose between non-nullable fields (strict contracts that cause broad null propagation on error) and nullable fields (weaker type guarantees but isolated error impact). HotChocolate's `QueryError` type and `IError` can be returned from resolvers to report partial failures without triggering null propagation.

---

#### Gotcha 8. Schema Stitching and Hot Chocolate Federation Are Not the Same Feature

**Concepts**
- Schema stitching: HotChocolate merges multiple remote schemas into a single gateway schema
- Apollo Federation: a standardised specification for distributed GraphQL with entity resolution
- HotChocolate supports Apollo Federation v1 and v2 natively but as a separate configuration from stitching
- Mixing stitching and federation types in the same gateway produces undefined behavior

**Answer**

HotChocolate supports two distinct patterns for building a distributed GraphQL API. Schema stitching pulls remote schemas into a single gateway by introspecting remote endpoints and merging their types, with the gateway handling query delegation to remote services. Apollo Federation defines a specification where each subgraph publishes its types with `@key` directives and an entities resolver; the gateway uses the Federation specification to plan and execute cross-service queries. These two approaches are architecturally incompatible: stitching is HotChocolate-specific, while Federation follows a vendor-neutral specification supported by Apollo Router and other gateways. Choosing Federation enables polyglot subgraphs where some services are not HotChocolate; choosing stitching provides tighter integration with HotChocolate features. The choice must be made at architecture time — retrofitting one onto the other requires rewriting the gateway configuration.

---

#### Gotcha 9. GraphQL Error Response Returns HTTP 200 Even When Resolvers Fail

**Concepts**
- GraphQL wraps all errors in the `errors` array of a 200 OK response body
- An HTTP 400 or 500 status is only returned for protocol-level errors (malformed request, schema build failure)
- Clients that check the HTTP status code for errors will miss resolver failures
- Monitoring and alerting based on HTTP error rates will not capture GraphQL application errors

**Answer**

A GraphQL server returns HTTP 200 OK even when one or more resolvers threw exceptions or returned errors. The errors are included in the response body as an `errors` array alongside the `data` field, which contains null or partial results. This means HTTP error-rate monitoring — alerts on 4xx/5xx rates, APM transaction error tracking based on HTTP status — completely misses GraphQL application-level failures. A query that always resolves to errors will show as 100% HTTP 200 in infrastructure dashboards. GraphQL-aware monitoring must parse the response body and inspect the `errors` array, using the `extensions.code` field for error classification. HotChocolate propagates exception types to error codes in the response, which allows error-rate tracking at the GraphQL operation level rather than the HTTP level.

---

#### Gotcha 10. Field Middleware and Type Interceptors Applied in Wrong Order Affect Authorization and Caching

**Concepts**
- HotChocolate field middleware forms a pipeline: each middleware calls next(context) to continue
- Middleware registered first in AddFieldMiddleware runs outermost (first on the way in, last on the way out)
- Authorization middleware must run before caching middleware to avoid caching unauthorized responses
- An incorrectly ordered pipeline can cache a response for user A and serve it to user B

**Answer**

HotChocolate's field middleware pipeline processes each resolver invocation through a chain of middleware components, similar to ASP.NET Core's HTTP pipeline. Middleware is executed in registration order, with the first-registered middleware wrapping all subsequent ones. If a caching middleware is registered before an authorization middleware, the cache check executes first: it may return a cached response for the field without ever checking whether the current user is authorised to see it. A response cached from an admin user's request could be returned to an unauthenticated user on the next request for the same field. Authorization middleware must always be positioned outermost (registered first) so that it evaluates before any caching, logging, or transformation middleware can return a cached result. HotChocolate's built-in `[Authorize]` attribute registers its middleware at the outermost position by default, but custom middleware registration must respect this ordering.

---
