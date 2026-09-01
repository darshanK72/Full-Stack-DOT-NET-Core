# Full-Stack .NET Core — Interview Prep

A structured collection of interview questions and answers covering the full .NET Core stack, from runtime internals to cloud deployment. Over **4,200 questions** with detailed answers across 10 topic areas.

---

## Topic Index

| # | Folder | Description | Questions | README |
|---|--------|-------------|-----------|--------|
| 01 | [.NET Framework Architecture](./01.%20.NET%20Framework%20Architecture/) | CLR, IL, CTS, GC, assemblies, managed code, evolution to modern .NET | 33 | [README](./01.%20.NET%20Framework%20Architecture/README.md) |
| 02 | [C# Language Fundamentals](./02.%20C%23%20Language%20Fundamentals/) | C# basics, OOP, generics, LINQ, async, advanced features, unit testing | 1,587 | [README](./02.%20C%23%20Language%20Fundamentals/README.md) |
| 03 | [.NET Data Access](./03.%20.NET%20Data%20Access/) | ADO.NET, Dapper, Entity Framework Core | 310 | [README](./03.%20.NET%20Data%20Access/README.md) |
| 04 | [ASP.NET Core](./04.%20ASP.NET%20Core/) | Hosting, middleware, DI, configuration, logging, routing, minimal APIs | 398 | [README](./04.%20ASP.NET%20Core/README.md) |
| 05 | [ASP.NET Core MVC](./05.%20ASP.NET%20Core%20MVC/) | MVC pattern, controllers, Razor views, model binding, tag helpers | 430 | [README](./05.%20ASP.NET%20Core%20MVC/README.md) |
| 06 | [ASP.NET Core Web API](./06.%20ASP.Net%20Core%20Web%20API/) | REST, API controllers, versioning, OpenAPI/Swagger, CORS, gRPC | 422 | [README](./06.%20ASP.Net%20Core%20Web%20API/README.md) |
| 07 | [ASP.NET Core Identity & Security](./07.%20ASP.NET%20Core%20Identity%20%26%20Security/) | Authentication, authorization, JWT, OAuth 2.0, OIDC, 2FA | 334 | [README](./07.%20ASP.NET%20Core%20Identity%20%26%20Security/README.md) |
| 08 | [Real-Time & Communication Protocols](./08.%20Real-Time%20%26%20Communication%20Protocols/) | HTTP, WebSockets, SSE, SignalR, WebRTC, GraphQL, gRPC, OData | — | [README](./08.%20Real-Time%20%26%20Communication%20Protocols/README.md) |
| 09 | [ASP.NET Microservices](./09.%20ASP.NET%20Microservices/) | Clean Arch, CQRS, DDD, Docker, Kubernetes, messaging, Saga, resilience | 358 | [README](./09.%20ASP.NET%20Microservices/README.md) |
| 10 | [ASP.NET Azure Services](./10.%20ASP.NET%20Azure%20Services/) | App Service, Azure SQL, Blob, Functions, Key Vault, Service Bus, Redis | 389 | [README](./10.%20ASP.NET%20Azure%20Services/README.md) |

---

## Structure

Each topic folder contains sub-topic folders. Each sub-topic has an `INTERVIEW_QA.md` with:

- **Table of Contents** — linked to every question
- **Questions with full answers** inline
- **Scenario-Based Questions** (Karat-format) where applicable

### Folder 01 (.NET Framework Architecture)
Single file at the root of the folder:
- [INTERVIEW_QA.md](./01.%20.NET%20Framework%20Architecture/INTERVIEW_QA.md)

### Folder 02 — C# Language Fundamentals (1,587 questions)

| Sub-topic | Questions |
|-----------|-----------|
| [01. C# Basics](./02.%20C%23%20Language%20Fundamentals/01.%20C%23%20Basics%20-%20Done/INTERVIEW_QA.md) | 256 |
| [02. Object Oriented Programming](./02.%20C%23%20Language%20Fundamentals/02.%20Object%20Oriented%20Programming/INTERVIEW_QA.md) | 216 |
| [03. Generics & Collections](./02.%20C%23%20Language%20Fundamentals/03.%20Generics%20%26%20Collections/INTERVIEW_QA.md) | 164 |
| [04. Functional Style Programming](./02.%20C%23%20Language%20Fundamentals/04.%20Functional%20Style%20Programming/INTERVIEW_QA.md) | 113 |
| [05. Language Integrated Query](./02.%20C%23%20Language%20Fundamentals/05.%20Language%20Integrated%20Query/INTERVIEW_QA.md) | 250 |
| [06. Multithreading & Async Programming](./02.%20C%23%20Language%20Fundamentals/06.%20Multithreading%20%26%20Async%20Programming/INTERVIEW_QA.md) | 173 |
| [07. File Input & Output and Streams](./02.%20C%23%20Language%20Fundamentals/07.%20File%20Input%20%26%20Outpout%20and%20Streams/INTERVIEW_QA.md) | 128 |
| [08. Advanced C# Features](./02.%20C%23%20Language%20Fundamentals/08.%20Advanced%20C%23%20Features/INTERVIEW_QA.md) | 187 |
| [09. Unit Testing](./02.%20C%23%20Language%20Fundamentals/09.%20Unit%20Testing/INTERVIEW_QA.md) | 100 |

### Folder 03 — .NET Data Access (310 questions)

| Sub-topic | Questions |
|-----------|-----------|
| [01. ADO.NET](./03.%20.NET%20Data%20Access/01.%20ADO.NET/INTERVIEW_QA.md) | 104 |
| [02. Dapper](./03.%20.NET%20Data%20Access/02.%20Dapper/INTERVIEW_QA.md) | 73 |
| [03. Entity Framework Core](./03.%20.NET%20Data%20Access/03.%20Entity%20Framework%20Core/INTERVIEW_QA.md) | 133 |

### Folder 04 — ASP.NET Core (398 questions)
15 sub-topics covering hosting, middleware, DI, configuration, logging, routing, filters, minimal APIs, and background services. See [README](./04.%20ASP.NET%20Core/README.md).

### Folder 05 — ASP.NET Core MVC (430 questions)
15 sub-topics covering the MVC pattern, controllers, Razor views, layouts, model binding, validation, tag helpers, routing, areas, and SignalR. See [README](./05.%20ASP.NET%20Core%20MVC/README.md).

### Folder 06 — ASP.NET Core Web API (422 questions)
16 sub-topics covering REST design, API controllers, routing conventions, content negotiation, versioning, OpenAPI/Swagger, CORS, problem details, auth, file handling, health checks, gRPC, GraphQL, and integration testing. See [README](./06.%20ASP.Net%20Core%20Web%20API/README.md).

### Folder 07 — ASP.NET Core Identity & Security (334 questions)
12 sub-topics covering authentication/authorization fundamentals, Identity setup, cookie auth, JWT, OAuth 2.0/OIDC, external logins, 2FA, web security, and secrets management. See [README](./07.%20ASP.NET%20Core%20Identity%20%26%20Security/README.md).

### Folder 09 — ASP.NET Microservices (358 questions)
14 sub-topics covering Clean Architecture, CQRS, DDD, event-driven design, API Gateway, service discovery, Docker, Kubernetes, distributed messaging, Saga, circuit breakers, and observability. See [README](./09.%20ASP.NET%20Microservices/README.md).

### Folder 10 — ASP.NET Azure Services (389 questions)
14 sub-topics covering Azure App Service, SQL Database, Blob Storage, Functions, API Management, DevOps Pipelines, Key Vault, Service Bus, Event Grid, Cosmos DB, Redis, Application Insights, and Entra ID. See [README](./10.%20ASP.NET%20Azure%20Services/README.md).
