# Azure Web API Deployment — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure App Service, and why is it a common target for hosting ASP.NET Core Web APIs?](#q1-what-is-azure-app-service-and-why-is-it-a-common-target-for-hosting-aspnet-core-web-apis)
2. [Q2. What is the relationship between an App Service Plan, an App Service app, and a resource group?](#q2-what-is-the-relationship-between-an-app-service-plan-an-app-service-app-and-a-resource-group)
3. [Q3. What does the `kind: "app"` setting on a Web App mean in Azure?](#q3-what-does-the-kind-app-setting-on-a-web-app-mean-in-azure)
4. [Q4. How does Azure App Service run an ASP.NET Core Web API — what runtime and web server are involved?](#q4-how-does-azure-app-service-run-an-aspnet-core-web-api-what-runtime-and-web-server-are-involved)
5. [Q5. What is the role of `AllowedHosts` in `appsettings.json` when the API is deployed to Azure?](#q5-what-is-the-role-of-allowedhosts-in-appsettingsjson-when-the-api-is-deployed-to-azure)
6. [Q6. What deployment options are available for ASP.NET Core Web APIs on Azure App Service?](#q6-what-deployment-options-are-available-for-aspnet-core-web-apis-on-azure-app-service)
7. [Q7. What is Zip Deploy, and how does it work for ASP.NET Core applications?](#q7-what-is-zip-deploy-and-how-does-it-work-for-aspnet-core-applications)
8. [Q8. What is Web Deploy (MSDeploy), and when would you choose it over Zip Deploy?](#q8-what-is-web-deploy-msdeploy-and-when-would-you-choose-it-over-zip-deploy)
9. [Q9. What does `dotnet publish` produce, and how does that output relate to Azure deployment?](#q9-what-does-dotnet-publish-produce-and-how-does-that-output-relate-to-azure-deployment)
10. [Q10. How does publish-from-Visual Studio work using a publish profile and service dependencies?](#q10-how-does-publish-from-visual-studio-work-using-a-publish-profile-and-service-dependencies)
11. [Q11. What is Run From Package (`WEBSITE_RUN_FROM_PACKAGE`), and why would you enable it?](#q11-what-is-run-from-package-website_run_from_package-and-why-would-you-enable-it)
12. [Q12. How do Azure App Service Application Settings map to ASP.NET Core configuration?](#q12-how-do-azure-app-service-application-settings-map-to-aspnet-core-configuration)
13. [Q13. Why should production secrets and connection strings not remain in `appsettings.json` on Azure?](#q13-why-should-production-secrets-and-connection-strings-not-remain-in-appsettingsjson-on-azure)
14. [Q14. What is the difference between Connection Strings and Application Settings in the Azure portal for App Service?](#q14-what-is-the-difference-between-connection-strings-and-application-settings-in-the-azure-portal-for-app-service)
15. [Q15. What does the `ASPNETCORE_ENVIRONMENT` variable control when the API runs on Azure?](#q15-what-does-the-aspnetcore_environment-variable-control-when-the-api-runs-on-azure)
16. [Q16. How does `launchSettings.json` differ from production configuration on Azure App Service?](#q16-how-does-launchsettingsjson-differ-from-production-configuration-on-azure-app-service)
17. [Q17. What is Azure Service Connector (Service Linker), and how does it relate to `serviceDependencies.json` in this project?](#q17-what-is-azure-service-connector-service-linker-and-how-does-it-relate-to-servicedependenciesjson-in-this-project)
18. [Q18. How does Entity Framework Core resolve the SQL Server connection string when this API runs on Azure App Service?](#q18-how-does-entity-framework-core-resolve-the-sql-server-connection-string-when-this-api-runs-on-azure-app-service)
19. [Q19. What firewall and networking rules are typically required for Azure SQL Database when accessed from App Service?](#q19-what-firewall-and-networking-rules-are-typically-required-for-azure-sql-database-when-accessed-from-app-service)
20. [Q20. How are EF Core migrations applied to an Azure SQL database as part of deployment?](#q20-how-are-ef-core-migrations-applied-to-an-azure-sql-database-as-part-of-deployment)
21. [Q21. What do the `Properties/ServiceDependencies/` ARM templates and publish profile represent in this project?](#q21-what-do-the-propertiesservicedependencies-arm-templates-and-publish-profile-represent-in-this-project)
22. [Q22. What are deployment slots in Azure App Service, and how do they support safer releases?](#q22-what-are-deployment-slots-in-azure-app-service-and-how-do-they-support-safer-releases)
23. [Q23. What is slot swapping, and which settings are swapped versus marked as slot-sticky?](#q23-what-is-slot-swapping-and-which-settings-are-swapped-versus-marked-as-slot-sticky)
24. [Q24. How would you set up a basic CI/CD pipeline to build and deploy this Web API to Azure?](#q24-how-would-you-set-up-a-basic-cicd-pipeline-to-build-and-deploy-this-web-api-to-azure)
25. [Q25. What is the difference between a staging slot and a separate staging App Service in another resource group?](#q25-what-is-the-difference-between-a-staging-slot-and-a-separate-staging-app-service-in-another-resource-group)
26. [Q26. Why is `httpsOnly: true` on an App Service important, and how does it relate to middleware in `Program.cs`?](#q26-why-is-httpsonly-true-on-an-app-service-important-and-how-does-it-relate-to-middleware-in-programcs)
27. [Q27. What production concerns arise when Swagger is enabled in a deployed API (as in this project's `Program.cs`)?](#q27-what-production-concerns-arise-when-swagger-is-enabled-in-a-deployed-api-as-in-this-projects-programcs)
28. [Q28. Gotcha: Why can a locally working API fail on Azure with database connection errors even when the connection string value looks correct?](#q28-gotcha-why-can-a-locally-working-api-fail-on-azure-with-database-connection-errors-even-when-the-connection-string-value-looks-correct)

---

## Q1. What is Azure App Service, and why is it a common target for hosting ASP.NET Core Web APIs?

**Concepts**
- PaaS vs IaaS hosting model
- Managed OS and runtime lifecycle
- App Service Plan compute tier and SKU
- Built-in TLS, autoscaling, and deployment slots
- First-class ASP.NET Core runtime support

**Answer**

Azure App Service is a PaaS offering that removes VM provisioning and OS patching from the picture — I publish a folder of compiled binaries and configuration, and the platform handles the rest. ASP.NET Core is a first-class stack: App Service loads the correct .NET runtime version matching the target framework, so deploying a `net6.0` assembly works without configuring a server. Built-in capabilities like managed TLS certificates, custom domains, autoscaling rules, deployment slots, and native integration with Azure SQL, Key Vault, and Application Insights make it faster to operate than raw IaaS VMs, because those concerns are absorbed by the platform rather than delegated to the team. This module's `LocalServerWebApiApplication` targets `net6.0` and deploys to an App Service named `apiapplication46310114` on a Standard S1 plan, which is a typical pattern for small production APIs.

---

## Q2. What is the relationship between an App Service Plan, an App Service app, and a resource group?

**Concepts**
- Resource group as logical container for Azure resources
- App Service Plan compute tier, region, and shared scaling
- Web App as deployable unit within a plan
- SKU feature gating (slots, scaling, price)
- Cost and permission scope of a resource group

**Answer**

A resource group is a logical container that groups related Azure resources for billing, RBAC, and lifecycle management. An App Service Plan defines the compute tier, region, and scaling configuration — it is the actual infrastructure that runs apps. The App Service app (the Web App) is the deployable unit that receives published files and sits on exactly one plan; multiple apps can share a plan to reduce cost, but they also share CPU, memory, and scaling limits. The SKU controls what features are available: the S1 plan in this sprint project enables deployment slots, which the free and shared tiers do not. Keeping the API, SQL database, storage account, and Event Grid topic in the same resource group (`rg-sprint`) simplifies permissions and cost tracking, even though Azure does not require co-location.

---

## Q3. What does the `kind: "app"` setting on a Web App mean in Azure?

**Concepts**
- App Service `kind` discriminator
- Windows vs Linux hosting surface
- Built-in .NET stack vs custom container
- `CURRENT_STACK` metadata and runtime handler selection
- Workload type disambiguation (functions vs web app)

**Answer**

The `kind` property tells Azure what type of App Service workload the resource represents. `"app"` means a standard Windows-based Web App for general HTTP traffic — as opposed to `"functionapp"` for Azure Functions or `"linux,container"` for Docker-based hosting. This project's ARM template sets `"kind": "app"` on both the site resource and its properties, meaning the platform uses the built-in .NET runtime stack rather than a custom container image. The accompanying `CURRENT_STACK: dotnetcore` metadata tells the App Service runtime handler which managed runtime to load when starting the process; `"app"` alone does not communicate which language is in use.

---

## Q4. How does Azure App Service run an ASP.NET Core Web API — what runtime and web server are involved?

**Concepts**
- Kestrel as the in-process HTTP server
- Application Request Routing (ARR) as the platform front-end reverse proxy
- TLS termination at the platform edge
- `dotnet publish` output and `web.config` startup descriptor
- `ASPNETCORE_URLS` or `PORT` injection by the platform

**Answer**

App Service starts the published application as a .NET process using the runtime version matching the target framework, and Kestrel — the web server embedded in the ASP.NET Core host — handles HTTP requests forwarded by the App Service front-end (Application Request Routing). TLS is terminated at the ARR edge, so Kestrel receives plain HTTP internally, which is why `app.UseHttpsRedirection()` in `Program.cs` is supplementary rather than the primary HTTPS enforcement mechanism. The `dotnet publish` output on Windows includes a `web.config` that instructs the AspNetCoreModule to launch `dotnet LocalServerWebApiApplication.dll`; the platform then injects `ASPNETCORE_URLS` or `PORT` so Kestrel binds to the expected port, overriding the `https://localhost:7127` URL from `launchSettings.json`.

---

## Q5. What is the role of `AllowedHosts` in `appsettings.json` when the API is deployed to Azure?

**Concepts**
- Host Filtering middleware and HTTP `Host` header validation
- `AllowedHosts: "*"` — wildcard disabling strict filtering
- Host-header injection and open-redirect attack surface
- CORS vs host filtering — orthogonal concerns
- Production tightening to named hostnames

**Answer**

`AllowedHosts` is read by ASP.NET Core's Host Filtering middleware, which validates the incoming HTTP `Host` header against an allowed list and returns HTTP 400 before any controller code runs if the header does not match. This project's `appsettings.json` sets it to `"*"`, which disables that check entirely — useful for local development where the hostname varies, but a risk in production because it permits requests with arbitrary `Host` values that could enable open-redirect or cache-poisoning attacks. On Azure, clients reach the API via `https://apiapplication46310114.azurewebsites.net` or a custom domain, so production configuration should list those exact names rather than wildcarding. Host filtering is separate from CORS: CORS controls browser cross-origin access by inspecting the `Origin` header, while host filtering guards the `Host` header regardless of browser involvement.

---

## Chapter 2 — Deployment Methods

---

## Q6. What deployment options are available for ASP.NET Core Web APIs on Azure App Service?

**Concepts**
- Zip Deploy via Kudu REST API or Azure CLI
- Web Deploy (MSDeploy) incremental file sync
- Git-based continuous deployment (GitHub, Azure DevOps, local Git)
- Container deployment from a registry
- CI/CD pipeline integration with `az webapp deploy`

**Answer**

The main deployment paths are Zip Deploy, Web Deploy (MSDeploy), Git-based deployment, container deployment, and CI/CD pipelines calling the Zip Deploy REST API. All of them push artifacts to the same App Service app but differ in tooling, speed, and whether content is replaced atomically or synced incrementally. Zip Deploy dominates automated pipelines — the Azure CLI `az webapp deploy`, GitHub Actions `azure/webapps-deploy`, and Azure DevOps tasks all use it by default because it is cross-platform and straightforward. Web Deploy is tightly integrated with Visual Studio publish profiles and supports incremental syncs, which is why this project's publish profile is named `apiapplication46310114 - Web Deploy` and references linked SQL service dependencies. Git-based deployment is convenient for smaller teams but couples deployment to a version control push, making artifact traceability harder in mature pipelines.

---

## Q7. What is Zip Deploy, and how does it work for ASP.NET Core applications?

**Concepts**
- Kudu deployment engine and extraction into `wwwroot`
- Atomic content replacement vs incremental sync
- `dotnet publish` as the prerequisite step
- Run From Package as an alternative mount mode
- Cross-platform compatibility (Linux build to Windows App Service)

**Answer**

Zip Deploy uploads a `.zip` archive of the published application to App Service's Kudu deployment engine, which extracts it into the site's `wwwroot` folder and restarts the app. For ASP.NET Core, the prerequisite is `dotnet publish -c Release`, which produces the binary folder that gets zipped — App Service never runs `dotnet build` on source code. Because Zip Deploy replaces the entire site content on each deployment, the live app always reflects exactly what was published, eliminating drift from partial incremental updates. Kudu logs are available at `https://<app-name>.scm.azurewebsites.net` and show extraction progress and errors — a missing `web.config` or failed extraction will appear there. The zip can be built on Linux in CI and deployed to a Windows App Service without any agent compatibility issues, which gives Zip Deploy an advantage over MSDeploy in cross-platform pipelines.

---

## Q8. What is Web Deploy (MSDeploy), and when would you choose it over Zip Deploy?

**Concepts**
- MSDeploy incremental file synchronization
- Visual Studio publish profile integration
- MSBuild transform files and service dependency wiring
- Web Deploy handler requirement on App Service
- Platform and tooling constraints (Windows-oriented)

**Answer**

Web Deploy uses MSDeploy to synchronize only changed files from a Visual Studio publish profile to App Service, rather than replacing the full site content as Zip Deploy does. I would choose it when developers are deploying frequently from Visual Studio and want faster incremental pushes, or when the workflow relies on MSBuild publish profiles with XML transform files and Visual Studio's connected services wiring — as this project does, where `apiapplication46310114 - Web Deploy.json` maps the SQL Server dependency and injects connection strings during publish. Web Deploy requires the Web Deploy handler to be enabled on the App Service instance and is most natural on Windows development machines; cross-platform CI pipelines more commonly use Zip Deploy or `az webapp deploy` because they avoid MSDeploy versioning and agent compatibility issues. Zip Deploy is the better default for automation; Web Deploy earns its keep in interactive Visual Studio workflows.

---

## Q9. What does `dotnet publish` produce, and how does that output relate to Azure deployment?

**Concepts**
- Publish output: application DLL, dependencies, config, and `web.config`
- Release vs Debug configuration artifacts
- `appsettings.Development.json` inclusion rules by configuration
- Azure never runs `dotnet build` on source
- Missing publish-folder content absent in production

**Answer**

`dotnet publish` compiles the project and copies the application assembly, all dependency DLLs, configuration files, and runtime assets into a single folder ready to run with `dotnet <AppName>.dll`. That folder — not the source tree or intermediate `bin/` output — is what gets zipped or Web Deployed to Azure App Service. For this `net6.0` project, the publish output includes `LocalServerWebApiApplication.dll`, EF Core and Swashbuckle dependencies, `appsettings.json`, and a generated `web.config` that tells the AspNetCoreModule how to launch the process on Windows App Service. Release configuration omits `appsettings.Development.json` by convention, since the Development environment file is typically excluded from non-Development publish configurations. Because Azure receives only the published artifacts, anything missing from the publish folder — migration scripts bundled as content, for example — will simply be absent in production.

---

## Q10. How does publish-from-Visual Studio work using a publish profile and service dependencies?

**Concepts**
- `.pubxml` publish profile as deployment descriptor
- `serviceDependencies.json` dependency declaration
- ARM template provisioning during first publish
- `secretStore: AzureAppSettings` injection pattern
- Web Deploy vs Zip Deploy from the IDE

**Answer**

Visual Studio reads a `.pubxml` publish profile that names the target App Service, deployment method, configuration, and linked service dependencies, then builds the project, runs `dotnet publish`, and pushes the output to Azure using Web Deploy or Zip Deploy. Service dependencies are declared in `serviceDependencies.json` as logical names mapped to configuration keys — this project's base file names an `mssql` dependency with `connectionId: ConnectionStrings:DefaultConnectionString`, which tells Visual Studio where to wire the SQL connection string after provisioning. The profile-specific file `serviceDependencies.apiapplication46310114 - Web Deploy.json` adds Azure Service Connector metadata: the SQL server and database resource IDs, and `"secretStore": "AzureAppSettings"` so the connection string is injected into App Service configuration rather than written into source files. ARM templates under `Properties/ServiceDependencies/` describe the App Service Plan, Web App, and SQL resources that Visual Studio can create or link during first publish, making the full environment reproducible from the IDE.

---

## Q11. What is Run From Package (`WEBSITE_RUN_FROM_PACKAGE`), and why would you enable it?

**Concepts**
- Zip archive mounted as read-only filesystem instead of extraction
- Atomic deployment without file extraction race conditions
- Cold start performance for large applications
- Runtime write restriction and implications for file output
- Kudu blob storage for package persistence

**Answer**

Run From Package is an App Service setting that mounts the deployment zip as a read-only filesystem rather than extracting files into `wwwroot`, so the app always runs from a fixed, immutable package. I would enable it because deployments become atomic — the app switches to the new package in one step with no window where the site is half-extracted — and cold start performance improves because the runtime maps files from the zip rather than scanning a directory tree. The zip is stored in Azure Blob Storage linked to the site, and failed deployments do not leave behind a partially written folder. The read-only constraint matters for runtime code: local file writes fail, which means features like this project's `DepartmentHelper` writing JSON files must target Azure Blob Storage rather than the local filesystem. Zip Deploy and Run From Package pair naturally in CI/CD because the same artifact from `dotnet publish` serves as both the build output and the runtime package.

---

## Chapter 3 — Configuration & Environment

---

## Q12. How do Azure App Service Application Settings map to ASP.NET Core configuration?

**Concepts**
- App Service settings surfaced as environment variables at runtime
- ASP.NET Core environment variable configuration provider
- Double-underscore (`__`) as JSON key nesting separator
- Configuration provider priority — environment variables override JSON files
- `IConfiguration.GetConnectionString()` resolution order

**Answer**

Application Settings and Connection Strings in the Azure portal are surfaced to the running app as environment variables, and ASP.NET Core's default configuration system reads environment variables as a higher-priority provider than `appsettings.json`. The double-underscore acts as a nesting separator: a portal setting named `ConnectionStrings__DefaultConnectionString` maps to the nested key `ConnectionStrings:DefaultConnectionString` in `IConfiguration`, which is exactly what `builder.Configuration.GetConnectionString("DefaultConnectionString")` reads. The full provider chain in `WebApplication.CreateBuilder(args)` loads `appsettings.json`, then environment-specific JSON, then environment variables, so Azure-injected values win without rebuilding the app or changing code. Non-connection settings like `EventGridTopicEndpoint`, the storage container name, and the storage account key from this project's `appsettings.json` should be moved to Application Settings in production for the same reason — so secrets are not deployed as plain text in the publish folder.

---

## Q13. Why should production secrets and connection strings not remain in `appsettings.json` on Azure?

**Concepts**
- Plain-text secrets in publish artifact accessible via file system
- Git history as permanent secret exposure vector
- Azure App Service Application Settings as secure injection point
- Key Vault references for centralized secret management
- Service Connector `secretStore: AzureAppSettings` pattern

**Answer**

Files in the publish output are deployed as plain text alongside DLLs, so anyone with deployment read access, a compromised build artifact, or a misconfigured directory listing can extract SQL passwords, storage account keys, or Event Grid access keys. This project's `appsettings.json` contains a SQL connection string, storage key, and Event Grid key for sprint training — acceptable in a local learning environment but wrong for any real deployment because the secrets travel through the build pipeline and land in the App Service file system. The git history problem is even more persistent: if a secret is committed, it remains readable forever even after rotation and deletion, since `git log -p` reveals past file contents. The right pattern is what this project's Service Connector configuration already describes: `"secretStore": "AzureAppSettings"` writes the connection string directly into App Service configuration during publish, keeping the secret out of source control and the publish folder entirely.

---

## Q14. What is the difference between Connection Strings and Application Settings in the Azure portal for App Service?

**Concepts**
- Connection Strings section with type hint (SQL Server, SQL Azure, Custom)
- Application Settings as general key-value pairs with no type metadata
- Environment variable naming conventions (`CUSTOMCONNSTR_`, `ConnectionStrings__`)
- Legacy framework compatibility vs ASP.NET Core runtime behavior
- Team convention vs runtime requirement distinction

**Answer**

Both Connection Strings and Application Settings in the Azure portal ultimately surface as environment variables inside the running app, so from ASP.NET Core's perspective the practical difference is minimal. The distinction is that Connection Strings carry a type hint (SQL Server, SQL Azure, MySQL, Custom) that legacy Azure tooling and older frameworks use for special handling — App Service sets both `CUSTOMCONNSTR_<name>` and `ConnectionStrings__<name>` environment variables for SQL-type entries, while Application Settings are injected as-is. For ASP.NET Core, `IConfiguration.GetConnectionString("DefaultConnectionString")` resolves the `ConnectionStrings__DefaultConnectionString` environment variable regardless of which portal section it came from. Organizing database strings under Connection Strings and non-secret config under Application Settings is a team convention that improves clarity in the portal, not a runtime requirement.

---

## Q15. What does the `ASPNETCORE_ENVIRONMENT` variable control when the API runs on Azure?

**Concepts**
- Environment name and environment-specific config file loading
- Developer exception page gating on `IsDevelopment()`
- `appsettings.Production.json` as production-specific override layer
- Swagger conditional gating by environment
- Default environment name (`Production`) on App Service

**Answer**

`ASPNETCORE_ENVIRONMENT` controls which environment name ASP.NET Core uses when loading optional configuration files and deciding whether development-only behaviors are active. When it is `Production` (the App Service default), `appsettings.Production.json` loads if present and `app.Environment.IsDevelopment()` returns false, so developer exception pages with stack traces stay off. When it is `Development`, richer error pages turn on — acceptable on a developer machine but dangerous on a public endpoint because detailed exceptions leak internal paths and schema details. This project's `Program.cs` calls `app.UseSwagger()` and `app.UseSwaggerUI()` unconditionally, which means Swagger UI stays active in production regardless of the environment name; gating those calls on `app.Environment.IsDevelopment()` would close that gap. Locally, `launchSettings.json` sets the variable to `Development`, but that file is never published to Azure.

---

## Q16. How does `launchSettings.json` differ from production configuration on Azure App Service?

**Concepts**
- `launchSettings.json` as dev-only tooling configuration, excluded from publish
- Local Kestrel URL (`applicationUrl`) vs App Service-injected ports
- IIS Express profile applicability (local only)
- `ASPNETCORE_ENVIRONMENT` set locally vs in App Service Application Settings
- Platform-injected `ASPNETCORE_URLS` or `PORT` overriding local URL config

**Answer**

`launchSettings.json` is a development-only file consumed by Visual Studio and `dotnet run` to configure launch URLs, browser launch targets, and local environment variables like `ASPNETCORE_ENVIRONMENT: Development`. It is not included in `dotnet publish` output and has zero effect on the deployed app — Azure App Service never reads it. The local URLs defined in this project (`https://localhost:7127` and `http://localhost:5034`) are meaningless on Azure, where the platform assigns the public `https://apiapplication46310114.azurewebsites.net` hostname and injects the internal port Kestrel should bind to via `ASPNETCORE_URLS` or `PORT`. The IIS Express launch profile applies only when running under IIS Express on a Windows developer machine; on App Service, Kestrel handles requests directly behind the ARR reverse proxy. Production environment configuration flows from App Service Application Settings, not from this file.

---

## Q17. What is Azure Service Connector (Service Linker), and how does it relate to `serviceDependencies.json` in this project?

**Concepts**
- Service Connector as managed binding between App Service and backing services
- `serviceDependencies.json` as Visual Studio representation of connections
- `secretStore: AzureAppSettings` — connection string injected into portal settings
- Base file vs publish-profile-specific dependency file distinction
- Connection string injection without committing secrets to source

**Answer**

Azure Service Connector (formerly Service Linker) creates and manages secure connections between an App Service and backing services like Azure SQL, Storage, or Key Vault, and writes the resulting connection information into App Service configuration. Visual Studio represents these connections in `serviceDependencies.json` so publish workflows know which Azure resources to link and which configuration keys to populate — it is the IDE's view of what Service Connector does at the Azure API level. The base `serviceDependencies.json` in this project declares an `mssql` dependency type with `connectionId: ConnectionStrings:DefaultConnectionString`, while the publish-profile-specific file adds the full Azure resource paths: SQL server `sprintdbserver46310114`, database `sprintdb46310114`, and `"secretStore": "AzureAppSettings"`. That `secretStore` setting is the key detail — it means the SQL connection string is written directly into App Service settings during publish rather than into any source or config file, keeping the secret out of the repository.

---

## Chapter 4 — Database, Migrations & Connected Services

---

## Q18. How does Entity Framework Core resolve the SQL Server connection string when this API runs on Azure App Service?

**Concepts**
- `IConfiguration.GetConnectionString()` as the resolution entry point
- Environment variable override priority over `appsettings.json`
- `Trusted_Connection=True` unusable from App Service to on-premises SQL
- Missing Azure override causing silent local value fallback
- Service startup success vs first-query failure timing

**Answer**

At startup, `Program.cs` registers the DbContext with `options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"))`, so EF Core reads whatever value the configuration system returns for that key — and because environment variables have higher priority than `appsettings.json`, the Azure-injected connection string from App Service configuration wins over the local SQL Server entry without any code change. Locally, `appsettings.json` points to `Server=INBLRVM26590142;Database=trainingdb46310114;Trusted_Connection=True`, which uses Windows integrated authentication against an on-premises SQL instance — that mechanism is unavailable from App Service, so the Azure value must supply SQL authentication credentials or a managed identity token instead, typically injected by Service Connector. The tricky part is that the app starts successfully even when the Azure override is missing, because Kestrel and EF Core's DI registration succeed without opening a database connection; the failure surfaces only on the first `DepartmentsController` request that triggers an EF Core query.

---

## Q19. What firewall and networking rules are typically required for Azure SQL Database when accessed from App Service?

**Concepts**
- Azure SQL server-level firewall default deny-all
- "Allow Azure services" rule vs explicit outbound IP allowlist
- App Service outbound IP addresses and VNet integration
- Private endpoint for network-isolated access
- Firewall rejection vs credential failure distinction in error messages

**Answer**

Azure SQL Database blocks all inbound connections by default, so the first networking requirement is enabling the "Allow Azure services and resources to access this server" firewall rule — or explicitly whitelisting the App Service's outbound IP addresses. The broad Azure-services rule is the fastest path and is sufficient for most non-regulated workloads, but it allows any Azure tenant's traffic, not just your own App Service. For tighter isolation, teams use VNet integration on the App Service and a private endpoint on the SQL server so traffic stays on the Microsoft backbone and the public SQL endpoint is disabled. A common deployment gotcha is having a correct password in the connection string but forgetting the firewall rule: the SQL server returns a connection timeout or login error that is easy to mistake for a credential problem. This project provisions server `sprintdbserver46310114` and database `sprintdb46310114` via ARM template, but firewall configuration is a post-provisioning step that must be confirmed before the API's EF Core queries will succeed.

---

## Q20. How are EF Core migrations applied to an Azure SQL database as part of deployment?

**Concepts**
- `dotnet ef database update` as explicit migration execution step
- Idempotent SQL scripts from `dotnet ef migrations script --idempotent`
- `context.Database.Migrate()` at startup — race risk in multi-instance deployments
- Migration job ordering relative to app code deployment
- Migration files as code artifacts, not auto-applied transforms

**Answer**

EF Core migrations are not applied automatically when the publish artifact reaches App Service — the migration files define schema changes but are just C# code until explicitly applied to a target database. The standard pipeline approach is running `dotnet ef database update --connection "<prod-conn-string>"` as a dedicated pipeline step before or after deployment, using the production connection string from a pipeline secret store. A safer alternative is generating idempotent SQL scripts with `dotnet ef migrations script --idempotent` and executing them against Azure SQL in a database migration job, since that script can be reviewed before it runs. Applying migrations at startup with `context.Database.Migrate()` works in single-instance setups but is risky when App Service scales to multiple instances because concurrent startup migrations can race and cause foreign-key or duplicate-column errors. The initial migration `20230623093644_init.cs` in this project must be applied to `sprintdb46310114` before the `DepartmentsController` endpoints can return data.

---

## Q21. What do the `Properties/ServiceDependencies/` ARM templates and publish profile represent in this project?

**Concepts**
- ARM templates as subscription-level infrastructure-as-code
- App Service Plan and Web App provisioning via IDE workflow
- SQL Server and database ARM resource definitions
- `serviceDependencies.json` linking ARM resources to configuration keys
- Infrastructure reproducibility from Visual Studio without Bicep or Terraform

**Answer**

The ARM JSON files under `Properties/ServiceDependencies/` are infrastructure-as-code templates that Visual Studio uses to provision the Azure resources this Web API depends on during first publish, so a developer with an empty subscription can recreate the full sprint environment from the IDE. `profile.arm.json` defines the compute resources: resource group `rg-sprint`, a Standard S1 App Service Plan, and Web App `apiapplication46310114` with HTTPS-only enabled and a system-assigned managed identity. `mssql1.arm.json` defines SQL Server `sprintdbserver46310114` and a Basic-tier database `sprintdb46310114` in the same resource group. Together with `serviceDependencies.apiapplication46310114 - Web Deploy.json`, these files document the complete sprint environment and tell Visual Studio how to wire the SQL connection string into App Service settings after provisioning — all without requiring a separate Bicep or Terraform authoring step.

---

## Chapter 5 — Staging Slots & CI/CD

---

## Q22. What are deployment slots in Azure App Service, and how do they support safer releases?

**Concepts**
- Deployment slot as a separate app instance on the same App Service Plan
- Slot-specific hostname for pre-production testing
- Slot-specific Application Settings for environment isolation
- Warm-up and health checks before swap
- Standard tier (S1) minimum requirement for slots

**Answer**

Deployment slots are separate instances of the app hosted on the same App Service Plan, each with its own hostname (`staging-apiapplication46310114.azurewebsites.net`), deployment history, and configuration. I deploy and test a new release in the staging slot before swapping it into production, which gives near-zero-downtime promotion and a fast rollback — swapping again reverts the change in seconds. Each slot can hold different Application Settings flagged as slot-specific, so staging can point at a test database while production uses the live one, and those settings stay with the slot rather than traveling with the code during swap. Warm-up requests can validate EF Core connectivity, blob storage access, and Event Grid integration against the staging slot URL before the swap commits, catching configuration problems that would otherwise surface in production. This project's S1 plan supports slots, so adding a `staging` slot is the natural next step for safer releases.

---

## Q23. What is slot swapping, and which settings are swapped versus marked as slot-sticky?

**Concepts**
- Slot swap as atomic routing exchange between two slots
- Sticky (deployment slot) settings staying bound to their slot
- Non-sticky settings traveling with the application content during swap
- Swap with preview for pre-swap warm-up
- Auto-swap for hands-off continuous deployment promotion

**Answer**

Slot swapping exchanges the running application and most configuration between two slots — typically staging and production — by updating internal routing, so what was staging becomes production almost instantly without redeploying files. Application Settings and connection strings marked "deployment slot setting" stay bound to the slot they are defined on and do not travel with the code during swap, while non-sticky settings move with the application content. Sticky settings are the right choice for environment-specific secrets like production database connection strings, because a staging build should never accidentally connect to the production database after swap. Non-sticky settings suit configuration that is logically part of the application's behavior rather than its environment — feature flags or build-version metadata, for example. Swap with preview lets me warm up the incoming slot and run smoke tests before the final exchange commits; auto-swap can be configured to promote a slot to production automatically after a successful staging deployment, which suits pipelines where staging is always stable.

---

## Q24. How would you set up a basic CI/CD pipeline to build and deploy this Web API to Azure?

**Concepts**
- `dotnet publish -c Release` as the pipeline build step
- Zip Deploy via Azure CLI or `azure/webapps-deploy` GitHub Action
- Service principal or federated identity for authentication
- Migration job as a pre-deploy pipeline step
- Deployment to staging slot with promotion via swap

**Answer**

The pipeline would follow four stages: restore, build/publish, migrate, deploy. I would run `dotnet restore` followed by `dotnet publish -c Release -o ./publish` targeting `net6.0`, produce a Release artifact stored in the pipeline, then run `dotnet ef migrations script --idempotent` against the Azure SQL connection string stored as a pipeline secret to apply any schema changes before the new code goes live. The deploy step uses `az webapp deploy --src-path ./publish.zip --type zip` or the `azure/webapps-deploy` GitHub Action, authenticating via a service principal or federated identity rather than publishing credentials. App Service Application Settings — connection strings, Event Grid keys, and the storage key — are injected from the pipeline's secret store rather than from `appsettings.json`, so the artifact contains no credentials. For safer releases, I would deploy to the staging slot first, run smoke tests against `/swagger` or a health endpoint, and then execute `az webapp deployment slot swap` to promote to production.

---

## Q25. What is the difference between a staging slot and a separate staging App Service in another resource group?

**Concepts**
- Slot sharing the App Service Plan compute vs independent App Service Plan
- Swap speed and atomic promotion vs independent deployment workflow
- Slot-sticky settings for environment isolation
- Resource group boundary and VNet topology differences
- Cost trade-off: shared plan vs separate plan

**Answer**

A deployment slot shares the App Service Plan with production, swaps traffic in seconds, and is designed for same-app blue-green releases — it is the right choice when staging and production configurations differ only in slot-sticky settings like the database connection string. A separate staging App Service in another resource group has its own plan, URL, scaling rules, and network configuration, which makes it suitable for long-lived integration environments, load testing at a different tier, or validating network topology changes that slots on a shared plan cannot replicate. Slots are cheaper because they reuse plan capacity and require no additional provisioning; a separate App Service adds another plan's cost. The trade-off is isolation: a slot on the same plan means a misbehaving staging deployment can starve production CPU or memory, while a separate App Service has fully independent compute. This sprint project currently uses a single Web App `apiapplication46310114`; adding a `staging` slot would be the minimal next step toward safer releases without the cost and complexity of a separate environment.

---

## Chapter 6 — Security, Swagger & Production Gotchas

---

## Q26. Why is `httpsOnly: true` on an App Service important, and how does it relate to middleware in `Program.cs`?

**Concepts**
- Platform-level HTTPS enforcement at the ARR edge before app code runs
- `app.UseHttpsRedirection()` as in-app fallback layer
- TLS termination at App Service front-end (Kestrel receives plain HTTP internally)
- `*.azurewebsites.net` managed TLS certificates
- Defense in depth for data-in-transit protection

**Answer**

`httpsOnly: true` on the App Service resource rejects plain HTTP requests at the platform edge — before traffic reaches the application — and redirects clients to HTTPS. This is the primary enforcement mechanism because it applies regardless of what middleware the app has configured. `app.UseHttpsRedirection()` in `Program.cs` provides a second layer by redirecting HTTP requests that somehow reach Kestrel, but in practice on App Service, TLS is terminated by ARR and only forwarded requests arrive at the app, so the middleware's redirect rarely fires in production. The ARM template in this project enables `httpsOnly: true` on `apiapplication46310114`, and Azure manages the TLS certificate for `*.azurewebsites.net` automatically. HTTPS protects SQL connection strings, storage keys, and API payloads from interception in transit, but it does not address secrets stored in plain-text configuration files — those require App Service Application Settings or Key Vault.

---

## Q27. What production concerns arise when Swagger is enabled in a deployed API (as in this project's `Program.cs`)?

**Concepts**
- Swagger UI as public API surface disclosure and reconnaissance surface
- `IsDevelopment()` guard for conditional Swagger registration
- Authorization middleware vs Swagger exposure — orthogonal concerns
- IP restriction or Azure Front Door rule as alternative mitigation
- Unconditional `UseSwagger()` in `Program.cs` — current gap in this project

**Answer**

Swagger UI renders the full API surface, request models, and example payloads in an interactive browser interface, which makes it valuable for development but expands the attack reconnaissance surface in production — anyone who can reach `https://apiapplication46310114.azurewebsites.net/swagger` can enumerate every endpoint and experiment with payloads without reading source code. This project's `Program.cs` registers `UseSwagger()` and `UseSwaggerUI()` unconditionally, so the documentation is live on Azure. The standard fix is wrapping those calls in `if (app.Environment.IsDevelopment())`, which gates Swagger on the environment name set in App Service Application Settings. A complementary option is restricting the `/swagger` path by IP address using App Service access restrictions or an Azure Front Door rule, so Swagger remains accessible to the development team without being public. Exposing Swagger does not bypass authorization middleware — `UseAuthorization()` is called in `Program.cs` — but since no authentication scheme is configured, the routes it reveals are effectively unprotected anyway.

---

## Q28. Gotcha: Why can a locally working API fail on Azure with database connection errors even when the connection string value looks correct?

**Concepts**
- `Trusted_Connection=True` unusable from App Service to on-premises SQL
- Azure SQL firewall blocking App Service outbound IPs
- App startup succeeding before first EF Core query failure
- `ConnectionStrings__DefaultConnectionString` override verification in the portal
- Kudu console and App Service logs for connectivity debugging

**Answer**

The most common causes fall into three categories: wrong connection string content, wrong connection string delivery, and firewall rejection. The local `appsettings.json` uses `Trusted_Connection=True` against `INBLRVM26590142`, which is Windows integrated authentication to an on-premises SQL Server — that mechanism is unavailable from App Service, so even if the server were reachable, the connection would fail. The Azure-injected connection string must arrive via the App Service Connection Strings or Application Settings blade to override the JSON file value, and the most common mistake is the override never being set or using the wrong key name so `IConfiguration.GetConnectionString("DefaultConnectionString")` still reads the local value. Even with a valid Azure SQL password, if the SQL server firewall does not allow the App Service's outbound IPs or the Azure services exception, the connection is rejected before authentication — which surfaces as a timeout or login error that is easy to misread as a credential problem. The app starts cleanly in all these cases because Kestrel and EF Core's DI registration succeed without opening a database connection; the failure appears only when `DepartmentsController` executes the first EF Core query. I would verify by checking the Configuration blade for `ConnectionStrings__DefaultConnectionString`, confirming the SQL server firewall rule, and using the Kudu console to test TCP connectivity to the SQL endpoint.

---

## Gotchas — Azure Web API Deployment (Interview Traps)

---

#### Gotcha 1. Publish profile credentials stored in .pubxml files expose deployment keys in source control

**Concepts**
- `.pubxml` files generated by Visual Studio contain deployment credentials in plaintext
- These files are committed to source control by default if `.gitignore` is not configured
- Compromised publish credentials allow arbitrary code deployment to App Service
- GitHub Actions and service principal / managed identity replace publish profile credentials

**Answer**

Visual Studio's "Publish" wizard generates `.pubxml` files under `Properties/PublishProfiles/` that embed the deployment username and password for Web Deploy in plaintext XML. If these files are committed to a public or shared repository, anyone with access can download and deploy arbitrary code to the production App Service. The correct approach is to add `*.pubxml.user` and optionally `*.pubxml` to `.gitignore`, and to replace publish-profile-based deployment with GitHub Actions using `azure/webapps-deploy` and an OIDC service principal or App Service publishing identity that does not require embedding secrets in files.

---

#### Gotcha 2. Non-sticky (non-slot-specific) application settings travel with the swap — staging secrets reach production

**Concepts**
- Slot swap exchanges both the deployment files and non-sticky app settings
- A staging-environment secret (Stripe test key, lower-tier DB) set as non-sticky follows the code to production
- Slot-specific settings must be explicitly marked in the portal
- Post-swap testing may pass because the staging secret is now in production but the test matches

**Answer**

During a deployment slot swap, Azure exchanges the non-sticky application settings along with the code. A developer who sets a staging-tier Stripe API key or a development database connection string as an app setting without marking it as slot-specific will find that after the swap, production starts calling the staging payment endpoint or the development database. Post-swap smoke tests can still pass because the test environment now contains the staging credentials and the behavior looks correct until a real user transaction fails or audit logging reveals the wrong endpoint. Every environment-specific secret must be marked as slot-specific before any swap to production.

---

#### Gotcha 3. ASPNETCORE_ENVIRONMENT defaults to Production in Azure — staging slot still loads Production appsettings without explicit override

**Concepts**
- Azure App Service does not automatically set `ASPNETCORE_ENVIRONMENT` to "Staging" for staging slots
- Without explicit override the staging slot runs `appsettings.Production.json`
- Staging environment validation requires the same production appsettings tier
- Slot-specific `ASPNETCORE_ENVIRONMENT = Staging` must be added as a sticky setting

**Answer**

Azure App Service sets `ASPNETCORE_ENVIRONMENT` to "Production" by default for all slots unless explicitly overridden. This means a staging deployment slot running code that uses `appsettings.Staging.json` for database connection strings or feature flags will fall back to `appsettings.Production.json` because the environment variable does not say "Staging". To make the staging slot load staging-specific configuration, add `ASPNETCORE_ENVIRONMENT` as a slot-specific (sticky) application setting with the value "Staging" on the staging slot. This is one of the most common misconfigured staging environments in production deployment setups.

---

#### Gotcha 4. Swagger/OpenAPI UI left enabled in production exposes full API schema and try-it interface

**Concepts**
- `app.UseSwaggerUI()` in production enables public API documentation and a live try-it client
- Authenticated endpoints show their security schemes; unauthenticated callers can explore all routes
- Environment guard (`if (app.Environment.IsDevelopment())`) is the standard protection
- Security scans flag production Swagger endpoints as information disclosure

**Answer**

A `Program.cs` that calls `app.UseSwagger()` and `app.UseSwaggerUI()` without an `IsDevelopment()` guard exposes full API documentation including all endpoint paths, request schemas, and response types to anyone with the production URL. For internal or partially public APIs this is an information disclosure risk; automated security scanners flag the `/swagger/index.html` endpoint as a finding. The fix is to wrap the Swagger registration in `if (app.Environment.IsDevelopment())`, or alternatively, host Swagger behind authentication middleware in staging-only slots. Developers who test locally never notice because they rely on Swagger for development and forget to add the environment guard.

---

#### Gotcha 5. Framework-dependent publish requires the correct .NET runtime pre-installed on App Service — missing runtime causes 500.31

**Concepts**
- Framework-dependent deployment publishes DLLs without the .NET runtime
- The runtime must be installed on the App Service or selected as the "Stack"
- Publishing for .NET 10 but App Service stack set to .NET 8 causes HTTP 500.31 ANCM error
- Self-contained publish includes the runtime but doubles deployment size

**Answer**

Framework-dependent publish produces small deployment artifacts but assumes the target environment has the matching .NET runtime installed. If the App Service "Stack" setting (under Configuration > General settings) is set to a different .NET version than the one the application targets, the hosting module cannot load the application and returns HTTP 500.31 (ANCM Failed to Find Native Dependencies). This commonly happens when upgrading the application to a new .NET major version without updating the App Service stack setting. Self-contained publish avoids this dependency but produces significantly larger artifacts and the included runtime does not receive OS-level patches automatically.

---

#### Gotcha 6. Application settings with colon separators must use double underscores in App Service — single colons cause null reads

**Concepts**
- `appsettings.json` uses `:` as the section separator (`"Section:Key"`)
- App Service environment variables use `__` as the hierarchical separator (`Section__Key`)
- Single colon in an App Service setting name is not interpreted as a section separator
- The IConfiguration binding maps `__` to `:` automatically on all platforms

**Answer**

IConfiguration's hierarchical key syntax uses `:` as the separator in `appsettings.json` (for example `"Jwt:Issuer"`), but when an environment variable is the source, `:` is not a valid character for environment variable names on Linux. App Service maps hierarchical keys using `__` (double underscore): the setting `Jwt__Issuer` maps to `Configuration["Jwt:Issuer"]`. A developer who creates an App Service application setting named `Jwt:Issuer` (with a single colon) will find that `Configuration["Jwt:Issuer"]` returns null because the colon is not recognized as a section separator in the environment variable layer.

---

#### Gotcha 7. Startup errors return HTTP 500 or 503 without details — the root exception is only in Log Stream or Kudu

**Concepts**
- ASP.NET Core startup exceptions do not produce a developer exception page on Azure
- HTTP 503 from the App Service load balancer means the process failed to start or health check failed
- ANCM errors (502.5, 500.31) surface only in Event Log or Kudu console diagnostics
- `ASPNETCORE_DETAILEDERRORS=true` as an app setting reveals exception details in the response temporarily

**Answer**

When an ASP.NET Core application fails during startup (missing configuration, DI exception, database migration failure), Azure App Service returns HTTP 502.5 or 503 to callers with no details in the response body. The root exception is logged to the application event log accessible via Kudu (`/api/logs/application`) or the Log Stream panel. Developers who only look at the HTTP response code and body miss the actual exception. Setting `ASPNETCORE_DETAILEDERRORS=true` as a temporary app setting causes the ASP.NET Core error page to render the exception details in the HTTP response, which is useful for diagnosis but must be removed before the issue is resolved.

---

#### Gotcha 8. Blue/green swap drops in-flight long-running requests and WebSocket connections during the swap

**Concepts**
- Slot swap switches traffic at the load balancer level
- In-flight requests to the old slot are not drained by default
- Long-running HTTP streaming and SignalR WebSocket connections are abruptly closed
- `WEBSITE_SWAP_WARMUP_PING_PATH` and connection draining settings mitigate this

**Answer**

A deployment slot swap in Azure App Service switches traffic at the load balancer level when the new slot becomes warm. While App Service does attempt a warm-up using `WEBSITE_SWAP_WARMUP_PING_PATH`, it does not drain in-flight long-running HTTP requests or WebSocket connections to the old slot by default. Callers in the middle of a file download, a streaming response, or a SignalR session will receive a connection reset. For APIs that have long-polling or streaming endpoints, the deployment strategy must account for graceful connection draining or use a circuit breaker on the client side to reconnect after the swap.

---

#### Gotcha 9. Self-contained deployment does not receive automatic runtime security patches — OS-level vulnerabilities remain

**Concepts**
- Self-contained publish bundles the runtime inside the deployment package
- The bundled runtime is fixed at publish time and not updated by Azure's runtime patches
- Framework-dependent deployments inherit runtime patches from the App Service platform
- Regularly re-publishing a self-contained app is needed to incorporate runtime security fixes

**Answer**

Self-contained deployment includes the .NET runtime inside the deployment package, which means Azure's platform-level runtime patching does not apply to the bundled runtime. When Microsoft releases a .NET security patch, framework-dependent deployments automatically use the updated runtime the next time the App Service worker is recycled, but self-contained deployments continue running the older bundled version indefinitely until the application is recompiled and redeployed. Organizations that choose self-contained deployments for runtime version control must establish a process to rebuild and redeploy when .NET security advisories are issued.

---

#### Gotcha 10. Deployment to Linux App Service with file path casing differences causes FileNotFoundException at runtime

**Concepts**
- Linux file system is case-sensitive; Windows is case-insensitive
- A file referenced as `Views/Home/Index.cshtml` but stored as `views/home/index.cshtml` fails on Linux
- Visual Studio development on Windows masks casing issues because NTFS is case-insensitive
- Consistent lowercase or PascalCase conventions prevent cross-platform casing failures

**Answer**

Linux App Service runs on a case-sensitive file system, while Windows development machines use NTFS which is case-insensitive by default. An ASP.NET Core application that references a view, static file, or embedded resource with incorrect casing (for example `View("index")` for a file named `Index.cshtml`) will work correctly on a developer's Windows machine but throw `FileNotFoundException` on the Linux App Service. This is commonly discovered only after the first deployment to Linux because automated tests running on Windows CI agents also mask the issue. The fix is to enforce consistent naming conventions and run integration tests against a Linux container in CI.

---
