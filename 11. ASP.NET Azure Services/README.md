# 11. ASP.NET Azure Services

Hands-on **ASP.NET Core** projects that integrate common **Microsoft Azure** services — hosting, data, storage, serverless, API gateways, DevOps, and observability. Work through topics in order; later projects assume familiarity with App Service deployment and Azure resource configuration.

## Prerequisites

- **[07. ASP.Net Core Web API](../07.%20ASP.Net%20Core%20Web%20API/)** — REST APIs, Swagger, EF Core basics
- **[04. .NET Data Access](../04.%20.NET%20Data%20Access/)** — SQL Server / EF Core (for Azure SQL & Cosmos DB topics)
- **[08. ASP.NET Core Identity](../08.%20ASP.NET%20Core%20Identity/)** — helpful before Entra ID integration

## Azure subscription

Most projects require an **Azure subscription** (free tier works for learning). Create resources in the Azure Portal or with Azure CLI; store connection strings and keys in **User Secrets** or **Key Vault** — never commit secrets to source control.

## Topics (work in order)

| # | Topic | Status | Project / contents |
|---|--------|--------|-------------------|
| 01 | [Azure App Service](./01.%20Azure%20App%20Service/) | **Project** | Razor Pages — publish to App Service |
| 02 | [Azure SQL Database](./02.%20Azure%20SQL%20Database/) | **Project** | EF Core against Azure SQL |
| 03 | [Azure Blob Storage](./03.%20Azure%20Blob%20Storage/) | **Project** | Upload/download files via Blob SDK |
| 04 | [Azure Functions](./04.%20Azure%20Functions/) | **Project** | HTTP & SQL-triggered functions |
| 05 | [Azure Web API Deployment](./05.%20Azure%20Web%20API%20Deployment/) | **Project** | Web API + Azure SQL, Swagger, CORS |
| 06 | [Azure API Management](./06.%20Azure%20API%20Management/) | **Started** | APIM policy fragments (`temp.xml`); backend API TBD |
| 07 | [Azure DevOps Pipelines](./07.%20Azure%20DevOps%20Pipelines/) | **Placeholder** | Build & release pipelines |
| 08 | [Azure Key Vault](./08.%20Azure%20Key%20Vault/) | Placeholder | Secrets, certificates, Managed Identity |
| 09 | [Azure Service Bus](./09.%20Azure%20Service%20Bus/) | Placeholder | Queues, topics, pub/sub messaging |
| 10 | [Azure Event Grid](./10.%20Azure%20Event%20Grid/) | Placeholder | Event-driven workflows |
| 11 | [Azure Cosmos DB](./11.%20Azure%20Cosmos%20DB/) | Placeholder | NoSQL document API with EF Core or SDK |
| 12 | [Azure Cache for Redis](./12.%20Azure%20Cache%20for%20Redis/) | Placeholder | Distributed caching in ASP.NET Core |
| 13 | [Azure Application Insights](./13.%20Azure%20Application%20Insights/) | Placeholder | Telemetry, logging, performance monitoring |
| 14 | [Azure Entra ID](./14.%20Azure%20Entra%20ID/) | Placeholder | OAuth 2.0 / OpenID Connect authentication |

## Solution

Open **`AzureServices.sln`** at this level to load all existing `.csproj` projects in Visual Studio or Rider.

## Next module

Continue to advanced architecture tracks such as **[10. ASP.NET Microservices](../10.%20ASP.NET%20Microservices/)** once core Azure integrations are comfortable.
