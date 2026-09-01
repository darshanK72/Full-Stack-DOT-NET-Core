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

What is Azure App Service, and why is it a common target for hosting ASP.NET Core Web APIs?

**Answer:** Azure App Service is a fully managed platform-as-a-service (PaaS) offering that runs web applications and HTTP APIs without you provisioning or patching virtual machines. Teams choose it for ASP.NET Core Web APIs because it handles the operating system, runtime hosting, scaling, HTTPS, and deployment slots while the application still runs as a standard published .NET assembly.

- App Service supports multiple language stacks, but ASP.NET Core is a first-class citizen: you publish a self-contained folder of binaries and configuration, and the platform loads the correct .NET runtime version for your target framework.
- Built-in features such as custom domains, TLS certificates, autoscaling, deployment slots, and integration with Azure SQL, Key Vault, and Application Insights reduce operational work compared to running APIs on raw infrastructure-as-a-service (IaaS) virtual machines.
- This module's `LocalServerWebApiApplication` project targets `net6.0` and is designed to deploy to an App Service named `apiapplication46310114` on a Standard (S1) plan, which is a typical pattern for training and small production APIs.

---

## Q2. What is the relationship between an App Service Plan, an App Service app, and a resource group?

What is the relationship between an App Service Plan, an App Service app, and a resource group?

**Answer:** A resource group is a logical container for related Azure resources in one subscription, an App Service Plan defines the compute tier and region where apps run, and each App Service app is an individual site or API hosted on exactly one plan. Multiple apps can share one plan to save cost, but they also share CPU, memory, and scaling limits.

- The `profile.arm.json` in this project creates resource group `rg-sprint`, an App Service Plan at Standard S1 tier, and the Web App `apiapplication46310114` with `httpsOnly: true` and a system-assigned managed identity.
- The plan's SKU (for example S1) controls price, instance size, and features such as deployment slots; the Web App itself is the deployable unit that receives your published API files.
- Keeping the API, its SQL database, storage account, and Event Grid topic in the same resource group (as this sprint project does) simplifies permissions, cost tracking, and cleanup, even though Azure allows resources in different groups.

---

## Q3. What does the `kind: "app"` setting on a Web App mean in Azure?

What does the `kind: "app"` setting on a Web App mean in Azure?

**Answer:** The `kind` property tells Azure what type of App Service workload the site is. `"app"` means a standard Web App for general HTTP workloads such as ASP.NET Core sites and REST APIs, as opposed to specialized kinds like `"functionapp"` for Azure Functions or `"linux,container"` for container-based hosting.

- This project's ARM template sets `"kind": "app"` on both the site resource and its properties, indicating a Windows-based App Service running the built-in .NET stack rather than a custom Docker image.
- The site configuration also sets metadata `CURRENT_STACK` to `dotnetcore`, which tells the App Service platform which runtime handler to use when starting the process.
- API Management, Azure Front Door, or Application Gateway can sit in front of any Web App regardless of `kind`; `"app"` only describes the hosting surface, not whether the workload is a browser UI or a JSON API.

---

## Q4. How does Azure App Service run an ASP.NET Core Web API — what runtime and web server are involved?

How does Azure App Service run an ASP.NET Core Web API — what runtime and web server are involved?

**Answer:** Azure App Service starts your published ASP.NET Core application as a .NET process using the runtime version that matches your target framework, and the Kestrel web server inside your app handles HTTP requests forwarded by the App Service front-end (ARR — Application Request Routing). You do not configure IIS as the primary request handler for modern ASP.NET Core on App Service the way you did with .NET Framework.

- When the platform receives a request, the App Service front-end terminates TLS, applies routing rules, and forwards the request to your app's listening port; Kestrel processes the request through the ASP.NET Core middleware pipeline defined in `Program.cs`.
- The `dotnet publish` output includes your DLL, dependencies, and `web.config` (on Windows) or startup command metadata that tells the host how to launch `dotnet LocalServerWebApiApplication.dll`.
- Locally, `launchSettings.json` runs the same Kestrel process with URLs like `https://localhost:7127`; on Azure, the platform injects `PORT` or `ASPNETCORE_URLS` so Kestrel binds to the port App Service expects.

---

## Q5. What is the role of `AllowedHosts` in `appsettings.json` when the API is deployed to Azure?

What is the role of `AllowedHosts` in `appsettings.json` when the API is deployed to Azure?

**Answer:** `AllowedHosts` is an ASP.NET Core security setting read by the Host Filtering middleware that defines which HTTP `Host` header values the application will accept. This project's `appsettings.json` sets it to `"*"`, which disables strict host filtering and allows any host name — convenient for development but usually tightened in production.

- When a request arrives, the middleware compares the incoming `Host` header against the allowed list; a mismatch returns HTTP 400 before your controllers run, which mitigates some host-header and open-redirect attacks.
- On Azure App Service, clients typically reach the API via `https://apiapplication46310114.azurewebsites.net` or a custom domain; production configs often list those exact host names instead of `"*"`.
- `AllowedHosts` is separate from Azure networking rules or CORS: it only validates the host name inside the HTTP request, while CORS (configured in this project's `Program.cs` with `AllowAnyOrigin`) controls browser cross-origin access.

---

## Chapter 2 — Deployment Methods

---

## Q6. What deployment options are available for ASP.NET Core Web APIs on Azure App Service?

What deployment options are available for ASP.NET Core Web APIs on Azure App Service?

**Answer:** The main paths are Zip Deploy (uploading a compressed publish folder), Web Deploy / MSDeploy (incremental sync from Visual Studio or MSBuild), Git-based deployment (GitHub, Azure DevOps, local Git), container deployment, and continuous integration pipelines that call the Zip Deploy REST API or Azure CLI. All of them target the same App Service app but differ in tooling, speed, and whether files are synced incrementally or replaced as a unit.

- Zip Deploy is the default for Azure CLI (`az webapp deploy`), GitHub Actions, and many DevOps tasks because it is simple and works cross-platform.
- Web Deploy is tightly integrated with Visual Studio publish profiles and supports incremental updates, but it is primarily a Windows-oriented workflow.
- This project includes a Visual Studio publish profile named `apiapplication46310114 - Web Deploy` with linked service dependencies for SQL Server, reflecting the Web Deploy path from IDE to Azure.

---

## Q7. What is Zip Deploy, and how does it work for ASP.NET Core applications?

What is Zip Deploy, and how does it work for ASP.NET Core applications?

**Answer:** Zip Deploy uploads a `.zip` archive of your published application to App Service, where the Kudu deployment engine extracts it into the `wwwroot` folder (or mounts it when Run From Package is enabled) and restarts the site. For ASP.NET Core, you first run `dotnet publish -c Release`, zip the output folder, and push that archive to the deployment endpoint.

- Kudu runs deployment scripts and logs progress at `https://<app-name>.scm.azurewebsites.net`; failed extractions or missing `web.config` startup entries show up there.
- Zip Deploy replaces the entire site content by default, which makes deployments predictable — the live site matches exactly what you published.
- It does not require Visual Studio or MSDeploy agents, so the same zip artifact can be produced in a build pipeline on Linux and deployed to a Windows App Service.

---

## Q8. What is Web Deploy (MSDeploy), and when would you choose it over Zip Deploy?

What is Web Deploy (MSDeploy), and when would you choose it over Zip Deploy?

**Answer:** Web Deploy is Microsoft's incremental deployment technology that synchronizes only changed files from a publish profile to App Service, and Visual Studio uses it when you right-click Publish to Azure. You might choose it when developers deploy frequently from Visual Studio and want faster incremental syncs, or when you rely on MSBuild publish profiles with transform files and service dependency wiring.

- The publish profile in this project (`apiapplication46310114 - Web Deploy`) connects Visual Studio to the target App Service and can provision linked Azure SQL resources through ARM templates under `Properties/ServiceDependencies/`.
- Web Deploy depends on the Web Deploy handler being enabled on the App Service and is most natural on Windows development machines; cross-platform CI pipelines more often use Zip Deploy or `az webapp deploy`.
- Zip Deploy is simpler to reproduce in automation and avoids MSDeploy versioning issues; Web Deploy shines for interactive Visual Studio workflows with connected services.

---

## Q9. What does `dotnet publish` produce, and how does that output relate to Azure deployment?

What does `dotnet publish` produce, and how does that output relate to Azure deployment?

**Answer:** `dotnet publish` compiles the project in Release (or Debug) configuration and copies the application assembly, dependency DLLs, configuration files, and runtime assets into a single folder ready to run with `dotnet <AppName>.dll`. That publish folder — not the source code or intermediate `bin` build output — is what you zip or Web Deploy to Azure App Service.

- For this `net6.0` Web API project, publish output includes `LocalServerWebApiApplication.dll`, EF Core and Swashbuckle dependencies, `appsettings.json`, and a generated `web.config` that configures the AspNetCoreModule to launch the app on Windows App Service.
- Development-only settings in `appsettings.Development.json` are included only when publishing with Development configuration; Release publish typically omits or overrides them.
- Azure never runs `dotnet build` on your source; it only receives the published artifacts, so any missing file in the publish folder (migrations bundled as content, for example) will be absent in production.

---

## Q10. How does publish-from-Visual Studio work using a publish profile and service dependencies?

How does publish-from-Visual Studio work using a publish profile and service dependencies?

**Answer:** Visual Studio reads a publish profile (`.pubxml`) that names the target App Service, deployment method, and configuration, then builds, publishes, and pushes the output using Web Deploy or Zip Deploy while optionally provisioning connected Azure resources defined in `serviceDependencies.json` and ARM templates. Service dependencies map logical names like `mssql1` to connection string keys and Azure resource types.

- This project's base `serviceDependencies.json` declares an `mssql` dependency with `connectionId` `ConnectionStrings:DefaultConnectionString`, telling Visual Studio where to inject the SQL connection after provisioning.
- The profile-specific file `serviceDependencies.apiapplication46310114 - Web Deploy.json` adds Azure Service Connector metadata: the SQL server `sprintdbserver46310114`, database `sprintdb46310114`, and `secretStore: AzureAppSettings` so the connection string lands in App Service configuration rather than source code.
- ARM templates under `Properties/ServiceDependencies/apiapplication46310114 - Web Deploy/` describe the App Service Plan, Web App, and SQL resources Visual Studio can create or link during first publish.

---

## Q11. What is Run From Package (`WEBSITE_RUN_FROM_PACKAGE`), and why would you enable it?

What is Run From Package (`WEBSITE_RUN_FROM_PACKAGE`), and why would you enable it?

**Answer:** Run From Package is an App Service setting that mounts your deployment zip as a read-only filesystem instead of extracting files into `wwwroot`, so the site always runs from a known, immutable package. Teams enable it to get atomic deployments (the app switches to the new package in one step), faster cold starts on large apps, and reduced file-lock issues during deployment.

- When enabled, Kudu stores the zip in Azure Blob Storage linked to the site and sets an environment variable pointing to that package URL; failed deployments do not leave a half-extracted folder.
- Local file writes at runtime fail because the package is read-only, which encourages storing uploads and logs in Azure Blob Storage — relevant for this API, which writes department JSON to blob storage via `DepartmentHelper`.
- Zip Deploy and Run From Package work well together in CI/CD pipelines because the same artifact is both the build output and the runtime package.

---

## Chapter 3 — Configuration & Environment

---

## Q12. How do Azure App Service Application Settings map to ASP.NET Core configuration?

How do Azure App Service Application Settings map to ASP.NET Core configuration?

**Answer:** Application Settings and Connection Strings configured in the Azure portal are exposed to the running app as environment variables, and ASP.NET Core's default configuration providers read environment variables and override values from `appsettings.json`. A portal setting named `ConnectionStrings__DefaultConnectionString` maps to the nested JSON key `ConnectionStrings:DefaultConnectionString`.

- The configuration chain in `WebApplication.CreateBuilder(args)` loads `appsettings.json`, environment-specific JSON, then environment variables, so Azure-injected values win at runtime without rebuilding the app.
- This project's `Program.cs` calls `builder.Configuration.GetConnectionString("DefaultConnectionString")` for EF Core; on Azure, that value should come from App Service Connection Strings, not from the local SQL Server entry in `appsettings.json`.
- Non-connection settings such as `EventGridTopicEndpoint` or `Container` from this project's `appsettings.json` should likewise be moved to Application Settings in production so secrets are not deployed in the publish folder.

---

## Q13. Why should production secrets and connection strings not remain in `appsettings.json` on Azure?

Why should production secrets and connection strings not remain in `appsettings.json` on Azure?

**Answer:** Files in the publish output are deployed as plain text alongside your DLLs, so anyone with deployment read access, a compromised build artifact, or a misconfigured directory listing could extract SQL passwords, storage account keys, or Event Grid access keys. Azure App Service Application Settings, Key Vault references, and managed identities exist precisely to keep secrets out of source control and published packages.

- This project's local `appsettings.json` contains a SQL connection string, storage account key, and Event Grid access key for sprint training — acceptable locally but must be replaced by portal settings or Key Vault before real production use.
- Committing secrets to git creates a permanent exposure even after rotation, because history retains the old values.
- Service Connector in this project uses `"secretStore": "AzureAppSettings"` to write the SQL connection string directly into App Service configuration during publish, which is the correct direction for deployment automation.

---

## Q14. What is the difference between Connection Strings and Application Settings in the Azure portal for App Service?

What is the difference between Connection Strings and Application Settings in the Azure portal for App Service?

**Answer:** Both are stored as environment variables at runtime, but Connection Strings in the portal are a dedicated UI section that also records a type hint (SQL Server, SQL Azure, Custom, and others) for legacy frameworks and tooling. Application Settings are general key-value pairs with no type metadata.

- For ASP.NET Core, the practical difference is minimal because both surface as environment variables; EF Core and `IConfiguration` read either form when the naming convention matches.
- Connection Strings in the portal are often duplicated as `CUSTOMCONNSTR_<name>` for older Azure APIs, while ASP.NET Core prefers `ConnectionStrings__<name>` — App Service sets both patterns for SQL-type entries.
- Organizing database strings under Connection Strings and non-secret config under Application Settings is a team convention that improves clarity in the portal, not a runtime requirement for ASP.NET Core.

---

## Q15. What does the `ASPNETCORE_ENVIRONMENT` variable control when the API runs on Azure?

What does the `ASPNETCORE_ENVIRONMENT` variable control when the API runs on Azure?

**Answer:** `ASPNETCORE_ENVIRONMENT` selects which environment name ASP.NET Core uses to load optional configuration files and enable development-only behaviors. When set to `Production` on Azure, `appsettings.Production.json` loads if present, developer exception pages stay disabled, and Swagger might be gated off by convention — whereas `Development` enables richer errors and often Swagger UI.

- Locally, `launchSettings.json` sets `ASPNETCORE_ENVIRONMENT` to `Development` for both the Project and IIS Express profiles.
- On Azure, the default is typically `Production` unless you override it in Application Settings; leaving it as Development in production exposes detailed error information to clients.
- This project's `Program.cs` always calls `app.UseSwagger()` and `app.UseSwaggerUI()` without an environment check, so Swagger stays enabled in production unless you add a conditional guard.

---

## Q16. How does `launchSettings.json` differ from production configuration on Azure App Service?

How does `launchSettings.json` differ from production configuration on Azure App Service?

**Answer:** `launchSettings.json` is a local development-only file used by Visual Studio and `dotnet run` to set URLs, launch browser targets, and development environment variables; it is not published to Azure and has no effect on the deployed app. Production URLs, HTTPS ports, and environment name come from App Service platform settings and Application Settings instead.

- This file defines local URLs `https://localhost:7127` and `http://localhost:5034` and opens Swagger on launch — convenience for developers only.
- Azure assigns the public URL `https://<app-name>.azurewebsites.net` and terminates TLS at the front-end; you do not configure `applicationUrl` in Azure through `launchSettings.json`.
- IIS Express settings in the same file apply only when running under IIS Express on a Windows dev machine, not when the API runs on App Service with Kestrel behind ARR.

---

## Q17. What is Azure Service Connector (Service Linker), and how does it relate to `serviceDependencies.json` in this project?

What is Azure Service Connector (Service Linker), and how does it relate to `serviceDependencies.json` in this project?

**Answer:** Azure Service Connector (also called Service Linker) is an Azure service that creates secure connections between an App Service and backing services such as Azure SQL, storage, or Key Vault, and injects the resulting connection information into App Service configuration. Visual Studio represents those connections in `serviceDependencies.json` so publish workflows know which resources to link and which configuration keys to populate.

- The profile-specific dependency file references a Service Connector resource ID on site `apiapplication46310114` and maps it to `ConnectionStrings:DefaultConnectionString` for database `sprintdb46310114` on server `sprintdbserver46310114`.
- `"secretStore": "AzureAppSettings"` means the connector writes the connection string into App Service settings rather than into the project file, aligning with the principle that secrets live in Azure configuration.
- The base `serviceDependencies.json` without a publish profile only declares the dependency type (`mssql`) and connection ID, while the Web Deploy profile adds the full Azure resource paths for automated linking during Visual Studio publish.

---

## Chapter 4 — Database, Migrations & Connected Services

---

## Q18. How does Entity Framework Core resolve the SQL Server connection string when this API runs on Azure App Service?

How does Entity Framework Core resolve the SQL Server connection string when this API runs on Azure App Service?

**Answer:** At startup, `Program.cs` registers `Trainingdb46310114Context` with `options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"))`, so EF Core reads whatever value the configuration system supplies for that key — local JSON in development and Azure-injected environment variables in production. No code change is required between environments as long as the key name stays consistent.

- Locally, `appsettings.json` points to `Server=INBLRVM26590142;Database=trainingdb46310114;Trusted_Connection=True`, which uses Windows integrated authentication against a local or network SQL instance.
- On Azure, the same key must contain an Azure SQL connection string with SQL authentication or managed identity, typically injected by Service Connector or manual Connection String configuration in the portal.
- If the Azure value is missing or still points to the local server name, EF Core fails at first database access with a connection error even though the app itself starts successfully.

---

## Q19. What firewall and networking rules are typically required for Azure SQL Database when accessed from App Service?

What firewall and networking rules are typically required for Azure SQL Database when accessed from App Service?

**Answer:** Azure SQL Database blocks all external connections by default until you allow the client's IP address or enable Azure services access. App Service apps almost always rely on the "Allow Azure services and resources to access this server" firewall rule (or a private endpoint in advanced setups) so the API can reach the database without exposing the server to the entire internet.

- A connection string with the correct password still fails if the SQL server firewall rejects the App Service outbound IP addresses, which is a common deployment gotcha.
- For tighter security, teams replace the broad Azure services rule with a virtual network integration and private endpoint so traffic stays on the Microsoft backbone.
- This project's SQL ARM template provisions server `sprintdbserver46310114` and database `sprintdb46310114`; after deployment you must confirm firewall rules allow the Web App's outbound IPs or use the Azure services exception.

---

## Q20. How are EF Core migrations applied to an Azure SQL database as part of deployment?

How are EF Core migrations applied to an Azure SQL database as part of deployment?

**Answer:** EF Core migrations are not applied automatically when you Zip Deploy or Web Deploy the API; you must run `dotnet ef database update` against the production connection string during deployment or execute generated SQL scripts in a controlled pipeline step. The migration files in this project (`Migrations/20230623093644_init.cs`) define schema changes but only take effect when explicitly applied to the target database.

- A typical CI/CD approach runs a migration job before or after app deployment using the production connection string stored as a pipeline secret, or uses idempotent SQL scripts generated with `dotnet ef migrations script`.
- Applying migrations from the app at startup (`context.Database.Migrate()`) is possible but risky in multi-instance deployments because concurrent instances may race; many teams prefer pipeline-controlled migration.
- The Azure SQL database name in service dependencies (`sprintdb46310114`) must already exist and be reachable before the API's first EF Core query; an empty database with migrations applied matches what the `DepartmentsController` expects.

---

## Q21. What do the `Properties/ServiceDependencies/` ARM templates and publish profile represent in this project?

What do the `Properties/ServiceDependencies/` ARM templates and publish profile represent in this project?

**Answer:** They are Visual Studio infrastructure-as-code artifacts that describe which Azure resources to create or link when publishing, so developers can provision an App Service Plan, Web App, and SQL database from the IDE without writing separate Bicep or Terraform by hand. The ARM JSON files are subscription-level deployment templates parameterized with resource group name, location, and resource names.

- `profile.arm.json` defines compute resources: resource group `rg-sprint`, Standard S1 App Service Plan, Web App `apiapplication46310114` with HTTPS-only and system-assigned identity, and `CURRENT_STACK` set to `dotnetcore`.
- `mssql1.arm.json` defines SQL Server `sprintdbserver46310114` and Basic-tier database `sprintdb46310114` in the same resource group.
- Together with `serviceDependencies.apiapplication46310114 - Web Deploy.json`, they document the full sprint environment this Web API deploys into and how Visual Studio wires the SQL connection string into App Service settings.

---

## Chapter 5 — Staging Slots & CI/CD

---

## Q22. What are deployment slots in Azure App Service, and how do they support safer releases?

What are deployment slots in Azure App Service, and how do they support safer releases?

**Answer:** Deployment slots are separate instances of your app hosted on the same App Service Plan with their own host name (for example `staging-apiapplication46310114.azurewebsites.net`), configuration, and deployment history. You deploy and test a new build in a non-production slot before swapping it into the production slot, which reduces downtime and limits the blast radius of a bad release.

- Slots are available on Standard tier and above; this project's S1 plan supports them.
- Each slot runs the same codebase structure but can hold different Application Settings marked as slot-specific, so you can point a staging slot at a test database while production uses the live database.
- Warm-up requests and health checks can target the staging slot URL before swap, validating that EF Core connectivity and blob/Event Grid integration work in an Azure environment.

---

## Q23. What is slot swapping, and which settings are swapped versus marked as slot-sticky?

What is slot swapping, and which settings are swapped versus marked as slot-sticky?

**Answer:** Slot swapping exchanges the running application and most configuration between two slots (typically staging and production) by updating internal routing, so what was staging becomes production almost instantly without redeploying files. Application Settings and connection strings can be marked "deployment slot setting" (sticky) so they stay with the slot and do not travel with the code during swap.

- Non-sticky settings swap with the application content, which is useful when configuration is embedded in app settings tied to a build; sticky settings remain in place so production connection strings never accidentally follow a staging build into the wrong database.
- Swap can be configured with preview (warm up the incoming slot before final exchange) and auto-swap for hands-off promotion after a successful staging deployment.
- After swap, the old production code sits in the staging slot, giving you a fast rollback path by swapping again if errors appear.

---

## Q24. How would you set up a basic CI/CD pipeline to build and deploy this Web API to Azure?

How would you set up a basic CI/CD pipeline to build and deploy this Web API to Azure?

**Answer:** A typical pipeline checks out source, restores NuGet packages, runs `dotnet publish -c Release -o ./publish`, optionally applies EF Core migrations with a secret connection string, then deploys the publish folder to App Service using Zip Deploy via Azure CLI, GitHub Actions (`azure/webapps-deploy`), or an Azure DevOps AzureRmWebAppDeployment task. Service principal or federated identity authentication replaces publishing credentials in automated flows.

- The build stage should target `net6.0` to match `LocalServerWebApiApplication.csproj` and produce a Release artifact stored in the pipeline for traceability.
- The deploy stage sets App Service application settings (connection strings, Event Grid keys, storage keys) from a secret store rather than from committed `appsettings.json`.
- For staging slots, deploy to the staging slot first, run smoke tests against `/swagger` or `/api/Departments`, then execute `az webapp deployment slot swap` to promote to production.

---

## Q25. What is the difference between a staging slot and a separate staging App Service in another resource group?

What is the difference between a staging slot and a separate staging App Service in another resource group?

**Answer:** A deployment slot shares the App Service Plan compute with the production slot, swaps traffic quickly, and is designed for same-app promotion workflows, while a separate staging App Service is a fully independent app with its own plan, settings, and URL that mimics production infrastructure at greater isolation and cost.

- Slots are cheaper and faster for blue-green style releases because they reuse plan capacity and support one-step swap; they are best when staging and production configurations are nearly identical aside from slot-sticky settings.
- A separate App Service in another resource group suits long-lived integration environments, different scaling tiers, or testing network topology that slots cannot replicate.
- This sprint project uses a single Web App name (`apiapplication46310114`); adding a `staging` slot would be the natural next step for safer deployments without provisioning an entirely new app.

---

## Chapter 6 — Security, Swagger & Production Gotchas

---

## Q26. Why is `httpsOnly: true` on an App Service important, and how does it relate to middleware in `Program.cs`?

Why is `httpsOnly: true` on an App Service important, and how does it relate to middleware in `Program.cs`?

**Answer:** Setting `httpsOnly: true` on the App Service resource rejects plain HTTP requests at the platform edge and redirects clients to HTTPS before traffic reaches your application. The `app.UseHttpsRedirection()` middleware in `Program.cs` adds a second layer inside the app by redirecting HTTP requests that somehow reach Kestrel, but platform-level HTTPS-only enforcement is the primary protection for public endpoints.

- This project's ARM template enables `httpsOnly: true` on `apiapplication46310114`, matching security baseline expectations for APIs that expose data over the internet.
- TLS certificates for `*.azurewebsites.net` are managed by Azure; custom domains require you to bind your own or App Service-managed certificates.
- HTTPS protects connection strings, Event Grid keys, and API payloads in transit; it does not replace the need to remove secrets from published configuration files.

---

## Q27. What production concerns arise when Swagger is enabled in a deployed API (as in this project's `Program.cs`)?

What production concerns arise when Swagger is enabled in a deployed API (as in this project's `Program.cs`)?

**Answer:** Swagger UI exposes your full API surface, request models, and try-it-out functionality to anyone who can reach the URL, which aids development but expands attack reconnaissance in production. This project's `Program.cs` registers `UseSwagger()` and `UseSwaggerUI()` unconditionally, so the interactive documentation is available on Azure unless blocked by networking or conditional compilation.

- Attackers can discover endpoints such as `POST /api/Departments` and experiment with payloads without reading source code.
- Common mitigations include wrapping Swagger in `if (app.Environment.IsDevelopment())`, protecting it with authentication, restricting access by IP or Azure Front Door rule, or disabling it entirely in production builds.
- OpenAPI metadata does not bypass authorization middleware — this project calls `UseAuthorization()` but does not configure authentication — so Swagger reveals routes that may still be unprotected if no auth is applied.

---

## Q28. Gotcha: Why can a locally working API fail on Azure with database connection errors even when the connection string value looks correct?

Gotcha: Why can a locally working API fail on Azure with database connection errors even when the connection string value looks correct?

**Answer:** The most frequent causes are Azure SQL firewall blocking App Service outbound IPs, a connection string still pointing at a local server name from `appsettings.json` instead of the Azure-injected override, or SQL authentication credentials that were never set in App Service Connection Strings. The app starts because Kestrel does not open the database at startup, but the first EF Core query in `DepartmentsController` fails.

- Local development uses `Trusted_Connection=True` against `INBLRVM26590142`, which cannot work on Azure App Service because integrated Windows authentication to an on-premises server is unavailable in the cloud environment.
- Even with a valid Azure SQL password in the portal, missing "Allow Azure services" firewall rules produce timeout or login failures that look like bad credentials in logs.
- Verify configuration at runtime by checking the App Service Configuration blade for `ConnectionStrings__DefaultConnectionString`, testing connectivity from the Kudu console or a staging slot, and confirming Service Connector completed linking for `sprintdb46310114`.

---
