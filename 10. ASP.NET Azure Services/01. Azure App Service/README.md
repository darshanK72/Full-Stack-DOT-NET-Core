# Azure App Service — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure App Service, and how does it differ from running an ASP.NET Core application on a virtual machine (VM) or in Azure Kubernetes Service (AKS)?](#q1-what-is-azure-app-service-and-how-does-it-differ-from-running-an-aspnet-core-application-on-a-virtual-machine-vm-or-in-azure-kubernetes-service-aks)
2. [Q2. What is an App Service Plan, and what compute resources does it define for the web apps that run on it?](#q2-what-is-an-app-service-plan-and-what-compute-resources-does-it-define-for-the-web-apps-that-run-on-it)
3. [Q3. What are the main App Service Plan pricing tiers (Free, Shared, Basic, Standard, Premium, Isolated), and how do they differ in terms of features and scaling?](#q3-what-are-the-main-app-service-plan-pricing-tiers-free-shared-basic-standard-premium-isolated-and-how-do-they-differ-in-terms-of-features-and-scaling)
4. [Q4. Can multiple web apps share one App Service Plan? What are the cost and performance implications of that model?](#q4-can-multiple-web-apps-share-one-app-service-plan-what-are-the-cost-and-performance-implications-of-that-model)
5. [Q5. How does Azure App Service host an ASP.NET Core application at runtime — what is the relationship between the platform front end, Kestrel, and your published files?](#q5-how-does-azure-app-service-host-an-aspnet-core-application-at-runtime-what-is-the-relationship-between-the-platform-front-end-kestrel-and-your-published-files)
6. [Q6. What is the difference between deploying an ASP.NET Core app to a Windows App Service and a Linux App Service?](#q6-what-is-the-difference-between-deploying-an-aspnet-core-app-to-a-windows-app-service-and-a-linux-app-service)
7. [Q7. What deployment methods are available for publishing an ASP.NET Core app to App Service (for example Web Deploy, ZIP deploy, GitHub Actions, and Azure DevOps)?](#q7-what-deployment-methods-are-available-for-publishing-an-aspnet-core-app-to-app-service-for-example-web-deploy-zip-deploy-github-actions-and-azure-devops)
8. [Q8. What is ZIP deploy, and why is it commonly used in continuous integration and continuous delivery (CI/CD) pipelines for App Service?](#q8-what-is-zip-deploy-and-why-is-it-commonly-used-in-continuous-integration-and-continuous-delivery-cicd-pipelines-for-app-service)
9. [Q9. What is the "Run From Package" deployment option, and what performance and locking benefits does it provide for ASP.NET Core apps?](#q9-what-is-the-run-from-package-deployment-option-and-what-performance-and-locking-benefits-does-it-provide-for-aspnet-core-apps)
10. [Q10. What are deployment slots in Azure App Service, and how do they support zero-downtime or low-risk releases?](#q10-what-are-deployment-slots-in-azure-app-service-and-how-do-they-support-zero-downtime-or-low-risk-releases)
11. [Q11. How does slot swapping work, and what happens to application settings, connection strings, and warm-up behavior during a swap?](#q11-how-does-slot-swapping-work-and-what-happens-to-application-settings-connection-strings-and-warm-up-behavior-during-a-swap)
12. [Q12. What is the difference between sticky (slot-specific) and non-sticky app settings and connection strings when using deployment slots?](#q12-what-is-the-difference-between-sticky-slot-specific-and-non-sticky-app-settings-and-connection-strings-when-using-deployment-slots)
13. [Q13. How do you configure application settings and connection strings for an ASP.NET Core app in App Service, and how do they map to `IConfiguration` at runtime?](#q13-how-do-you-configure-application-settings-and-connection-strings-for-an-aspnet-core-app-in-app-service-and-how-do-they-map-to-iconfiguration-at-runtime)
14. [Q14. What is the role of the `ASPNETCORE_ENVIRONMENT` environment variable on App Service, and how does it relate to `appsettings.{Environment}.json`?](#q14-what-is-the-role-of-the-aspnetcore_environment-environment-variable-on-app-service-and-how-does-it-relate-to-appsettingsenvironmentjson)
15. [Q15. What is Kudu, and what capabilities does the Advanced Tools (Kudu) site provide for troubleshooting App Service deployments?](#q15-what-is-kudu-and-what-capabilities-does-the-advanced-tools-kudu-site-provide-for-troubleshooting-app-service-deployments)
16. [Q16. What is the Always On setting in App Service, and why does it matter for ASP.NET Core background work and cold-start behavior?](#q16-what-is-the-always-on-setting-in-app-service-and-why-does-it-matter-for-aspnet-core-background-work-and-cold-start-behavior)
17. [Q17. What is the difference between manual scaling, vertical scaling (changing plan tier), and autoscale rules on an App Service Plan?](#q17-what-is-the-difference-between-manual-scaling-vertical-scaling-changing-plan-tier-and-autoscale-rules-on-an-app-service-plan)
18. [Q18. What is Application Request Routing (ARR) affinity (sticky sessions), and when should you enable or disable it for a stateless ASP.NET Core API?](#q18-what-is-application-request-routing-arr-affinity-sticky-sessions-and-when-should-you-enable-or-disable-it-for-a-stateless-aspnet-core-api)
19. [Q19. How do you bind a custom domain and configure Transport Layer Security (TLS)/SSL certificates for an App Service web app?](#q19-how-do-you-bind-a-custom-domain-and-configure-transport-layer-security-tlsssl-certificates-for-an-app-service-web-app)
20. [Q20. What is Azure App Service managed identity, and how can an ASP.NET Core app use it to access Azure Key Vault or other Azure resources without storing secrets in configuration?](#q20-what-is-azure-app-service-managed-identity-and-how-can-an-aspnet-core-app-use-it-to-access-azure-key-vault-or-other-azure-resources-without-storing-secrets-in-configuration)
21. [Q21. What is Virtual Network (VNet) integration for App Service, and when do you need it to reach private databases or internal APIs?](#q21-what-is-virtual-network-vnet-integration-for-app-service-and-when-do-you-need-it-to-reach-private-databases-or-internal-apis)
22. [Q22. How does App Service health check work, and how does it interact with load balancing and instance replacement during scale operations?](#q22-how-does-app-service-health-check-work-and-how-does-it-interact-with-load-balancing-and-instance-replacement-during-scale-operations)
23. [Q23. What logging and diagnostics options are available on App Service for an ASP.NET Core application (platform logs, application logs, Application Insights)?](#q23-what-logging-and-diagnostics-options-are-available-on-app-service-for-an-aspnet-core-application-platform-logs-application-logs-application-insights)
24. [Q24. What is the purpose of a startup command or startup script on Linux App Service, and when is it required for .NET applications?](#q24-what-is-the-purpose-of-a-startup-command-or-startup-script-on-linux-app-service-and-when-is-it-required-for-net-applications)
25. [Q25. How do you troubleshoot a failed deployment or a web app that returns HTTP 502/503 errors on App Service using Kudu and the Log Stream?](#q25-how-do-you-troubleshoot-a-failed-deployment-or-a-web-app-that-returns-http-502503-errors-on-app-service-using-kudu-and-the-log-stream)
26. [Q26. What is the relationship between an App Service web app, its resource group, and Azure Resource Manager (ARM) templates in infrastructure-as-code deployments?](#q26-what-is-the-relationship-between-an-app-service-web-app-its-resource-group-and-azure-resource-manager-arm-templates-in-infrastructure-as-code-deployments)
27. [Q27. What are common security practices for hardening an ASP.NET Core App Service deployment (HTTPS-only, minimum TLS version, IP restrictions, authentication)?](#q27-what-are-common-security-practices-for-hardening-an-aspnet-core-app-service-deployment-https-only-minimum-tls-version-ip-restrictions-authentication)
28. [Q28. When would you choose Azure App Service over Azure Container Apps, Azure Functions, or AKS for hosting an ASP.NET Core web application?](#q28-when-would-you-choose-azure-app-service-over-azure-container-apps-azure-functions-or-aks-for-hosting-an-aspnet-core-web-application)

---

## Q1. What is Azure App Service, and how does it differ from running an ASP.NET Core application on a virtual machine (VM) or in Azure Kubernetes Service (AKS)?

**Concepts**
- Platform-as-a-Service — Microsoft-managed OS, patching, and load balancing
- VM IaaS — full OS control at the cost of manual operational burden
- AKS — container orchestration with significant platform engineering overhead
- Built-in features — deployment slots, autoscale, managed certificates

**Answer**

Azure App Service is a fully managed PaaS offering for hosting web applications and REST APIs where Microsoft operates the web server, load balancing, scaling infrastructure, and OS patching, so you deploy your published ASP.NET Core output and configure settings without provisioning or maintaining the underlying host. On a virtual machine you install the runtime, configure IIS or Kestrel as a service, apply security patches, and manage load balancers yourself — App Service removes that operational burden in exchange for less control over the host. AKS runs containers on a cluster you design and operate (nodes, ingress, Helm charts, pod scaling), which suits complex microservice topologies but adds significant platform engineering overhead compared to App Service. App Service integrates deployment slots, built-in autoscale, custom domains, and managed certificates as first-class features, whereas those capabilities on a VM or AKS require additional Azure resources and configuration, making it the simplest path from `dotnet publish` to a public HTTPS endpoint for a typical web API workload.

---

## Q2. What is an App Service Plan, and what compute resources does it define for the web apps that run on it?

**Concepts**
- App Service Plan as the billing and capacity unit
- Worker instances — CPU, memory, and storage shared by all apps on the plan
- Pricing tier controlling features and scale limits
- Scaling at the plan level, not per individual app

**Answer**

An App Service Plan is the billing and capacity unit that defines the virtual machine size, region, pricing tier, and scale limits for every web app assigned to it — the plan owns the compute, not the individual web app. All web apps on the same plan share the same pool of worker instances, so heavy traffic on one app can consume CPU and memory that other apps on that plan would otherwise use. The plan tier controls features such as deployment slots, daily backup, custom domains, and VNet integration, since a Free or Shared plan omits many production features available on Standard and Premium tiers. Scaling actions — whether manual instance count changes or autoscale rules — apply at the plan level, meaning every app on that plan scales out together when instances are added, which is why grouping apps with different load profiles on one plan can cause problems.

---

## Q3. What are the main App Service Plan pricing tiers (Free, Shared, Basic, Standard, Premium, Isolated), and how do they differ in terms of features and scaling?

**Concepts**
- Free and Shared tiers — multi-tenant with strict quotas
- Standard tier — first production tier unlocking autoscale and deployment slots
- Premium and Isolated tiers — larger SKUs, VNet injection, regulatory workloads

**Answer**

App Service Plan tiers trade cost against compute power, isolation, and platform features. Free and Shared plans run on shared infrastructure with strict quotas — no custom domains on Free, no Always On, and no dedicated workers — making them unsuitable for production ASP.NET Core workloads. Basic provides dedicated workers and manual scaling but lacks deployment slots and autoscale. Standard is the common starting point for production because it unlocks autoscale, up to five deployment slots, and daily backup — the features teams rely on for safe ASP.NET Core releases. Premium adds larger machine sizes, more slots, and better performance for high-traffic APIs. Isolated runs in an App Service Environment with private dedicated workers in your own subnet, which is the correct choice for compliance workloads requiring network isolation or for applications that must reach private endpoints inside a VNet without public internet exposure.

---

## Q4. Can multiple web apps share one App Service Plan? What are the cost and performance implications of that model?

**Concepts**
- Plan sharing — one billing unit for multiple apps
- Noisy-neighbor risk — shared CPU and memory across all apps on the plan
- Scaling at plan level preventing per-app instance count control
- Grouping apps by load profile for cost efficiency

**Answer**

Multiple web apps can run on a single App Service Plan, and Azure bills once per plan rather than per app, which makes plan sharing a common cost-optimization strategy for related services. Sharing a plan reduces cost when you host several low-traffic internal tools that never peak simultaneously, since you pay for one set of workers instead of many underutilized plans. The performance risk is that a memory leak or traffic spike in one ASP.NET Core app can slow every other app on the same plan because they share process space on the same worker instances — performance isolation is logical, not physical. Scaling out adds instances to the entire plan, so you cannot scale one app independently while leaving sibling apps on a single instance unless you move that app to its own plan. Production teams often group apps with similar criticality and load profiles on one Standard or Premium plan while isolating high-traffic or noise-sensitive APIs on dedicated plans.

---

## Q5. How does Azure App Service host an ASP.NET Core application at runtime — what is the relationship between the platform front end, Kestrel, and your published files?

**Concepts**
- Platform front-end load balancer terminating inbound connections
- Kestrel as the in-process web server on the worker
- Published folder — dotnet publish output on the worker file system
- ASPNETCORE_URLS environment variable set by the platform

**Answer**

On App Service, Azure's front-end load balancers terminate incoming HTTP or HTTPS connections and forward requests to a worker instance where your ASP.NET Core app runs Kestrel as the in-process web server. Your published folder — the output of `dotnet publish` — contains the `.dll`, dependencies, `web.config` on Windows, and static content that Kestrel serves after the platform starts the process. On Windows App Service, IIS historically acted as a reverse proxy in front of Kestrel using the ASP.NET Core Module; on modern stacks the platform still routes traffic to Kestrel listening on an assigned port rather than exposing Kestrel directly to the internet. The platform sets environment variables such as `ASPNETCORE_URLS` so Kestrel binds correctly inside the sandboxed worker without manual endpoint configuration in `Program.cs`, which explains why platform settings like Always On and ARR affinity affect process lifetime even though your code only references standard ASP.NET Core middleware.

---

## Q6. What is the difference between deploying an ASP.NET Core app to a Windows App Service and a Linux App Service?

**Concepts**
- Windows App Service — IIS integration and ASP.NET Core Module
- Linux App Service — Kestrel as primary entry point, no IIS layer
- Startup command requirement difference between platforms
- File path case sensitivity on Linux

**Answer**

Both platforms run the same published .NET assemblies, but Windows App Service uses a Windows Server worker with IIS integration and a generated `web.config`, while Linux App Service runs the app in a Linux container with Kestrel as the primary entry point and no IIS layer. On Linux you may need an explicit startup command (`dotnet YourApp.dll`) in App Service configuration, whereas Windows deployments rely on the ASP.NET Core Module to launch the process defined in `web.config`. File path casing matters on Linux — references to `appsettings.Production.json` must match disk casing exactly — while Windows is case-insensitive for configuration file loading, so a bug latent on Windows can surface immediately after moving to Linux. Linux often provides better price-performance for stateless ASP.NET Core APIs and aligns with container-based workflows, while Windows deployment is familiar for teams migrating from on-premises IIS or needing certain legacy integration scenarios.

---

## Q7. What deployment methods are available for publishing an ASP.NET Core app to App Service (for example Web Deploy, ZIP deploy, GitHub Actions, and Azure DevOps)?

**Concepts**
- Web Deploy (MSDeploy) — Visual Studio publish profile integration
- ZIP deploy — REST-based scriptable upload to the Kudu endpoint
- GitHub Actions and Azure DevOps — pipeline-driven automated deployment
- Container-based App Service pulling from Azure Container Registry

**Answer**

App Service accepts deployments through multiple channels that all push the published application package to the site's file system. Web Deploy integrates with Visual Studio publish profiles and synchronizes files incrementally, optionally transforming configuration, and is the natural fit for developer workstation pushes. ZIP deploy uploads a compressed publish folder to the Kudu endpoint at `/api/zipdeploy` and is the standard approach in CI/CD because it is scriptable, idempotent, and works from any agent with network access to the site. GitHub Actions and Azure DevOps pipelines typically run `dotnet publish`, archive the output, and invoke ZIP deploy or the `AzureWebApp` task, often combined with deployment slots for staging before a swap to production. Container-based App Service can pull from Azure Container Registry instead of deploying binaries, but framework-dependent ASP.NET Core deployments most commonly use ZIP or Web Deploy of the publish output.

---

## Q8. What is ZIP deploy, and why is it commonly used in continuous integration and continuous delivery (CI/CD) pipelines for App Service?

**Concepts**
- ZIP deploy — REST POST to Kudu extracting a compressed publish folder
- No Visual Studio or MSDeploy dependency on the build agent
- Atomic replacement and deployment history in Kudu
- Staging slot pairing for safe pipeline releases

**Answer**

ZIP deploy is a REST-based mechanism that uploads a compressed archive of your published application to App Service's Kudu service, which extracts the contents into the site's file system and triggers a site restart. Pipeline agents use it because it requires only HTTPS and credentials — no Visual Studio or MSDeploy installation on the build machine — so any CI runner can deploy. A typical pipeline step runs `dotnet publish -c Release -o ./publish`, zips the folder, and POSTs it to `https://{app-name}.scm.azurewebsites.net/api/zipdeploy` with a deployment credential or service principal token. ZIP deploy supports synchronous and asynchronous modes; asynchronous returns immediately while Kudu processes extraction in the background, which suits large publish outputs. Because the entire application is replaced atomically from the platform's perspective, ZIP deploy pairs naturally with deployment slots: publish to the staging slot, validate, then swap to production. Unlike manual FTP uploads, it integrates with deployment history in Kudu so you can correlate a failed release with build artifacts and rollback by redeploying a previous ZIP.

---

## Q9. What is the "Run From Package" deployment option, and what performance and locking benefits does it provide for ASP.NET Core apps?

**Concepts**
- Run From Package — ZIP mounted as read-only filesystem at startup
- WEBSITE_RUN_FROM_PACKAGE setting enabling mount behavior
- File-lock conflict elimination during active deployments
- Immutable deployment artifact matching the build output

**Answer**

Run From Package mounts the deployment ZIP as a read-only filesystem at startup instead of extracting files into `wwwroot`, which reduces deployment time and eliminates file-lock conflicts during active requests. The setting `WEBSITE_RUN_FROM_PACKAGE=1` (or a blob URL pointing at the package) tells the worker to serve the app directly from the mounted archive rather than copied files on disk. Extracting thousands of files on every deploy causes longer swap times and can fail if assemblies are locked by a running Kestrel process, since the extraction tries to overwrite files that are open, whereas mounting the package avoids touching individual files entirely. Read-only deployment also reduces the risk of runtime file tampering and makes the running application directory a predictable, immutable artifact matching the build output. Cold start can improve because the platform skips extraction I/O, though very large packages still add mount time; storing the ZIP in Azure Blob Storage with a SAS URL is a common pattern for multi-region consistency.

---

## Q10. What are deployment slots in Azure App Service, and how do they support zero-downtime or low-risk releases?

**Concepts**
- Deployment slots — separate app instances on the same App Service Plan
- Slot-specific hostname for pre-production validation
- Warm-up before swap protecting live traffic
- Discard or redeploy on failed validation without touching production

**Answer**

Deployment slots are separate instances of your web app — such as a `staging` slot — that run on the same App Service Plan with their own hostname, settings, and deployed bits while sharing compute with the production slot. You publish a new ASP.NET Core build to the staging slot, run validation tests against it at its dedicated URL like `{app-name}-staging.azurewebsites.net`, and then swap it into production with minimal downtime so the exact build that was tested is what goes live. Because slots share the plan, staging does not require a duplicate billing unit, though Premium tiers allow more slots for canary or pre-production environments. Slot warm-up endpoints let you verify that Kestrel starts and database migrations succeed before traffic moves, reducing the chance of serving HTTP 502 responses immediately after a swap. If a release fails validation, you discard or redeploy the staging slot while production continues serving the previous build — a workflow that is difficult to replicate with single-instance deployments.

---

## Q11. How does slot swapping work, and what happens to application settings, connection strings, and warm-up behavior during a swap?

**Concepts**
- Slot swap — routing target exchange without re-uploading files
- Sticky settings staying with their slot across swaps
- applicationInitialization warm-up before traffic reroutes
- Schema migration compatibility required across swap boundary

**Answer**

Slot swap exchanges the deployed content and most configuration between two slots by renaming their underlying workers' routing targets, so what was staging becomes production in seconds without re-uploading files. Azure performs a warm-up phase on the slot receiving production traffic to ensure Kestrel and dependencies initialize before the front end routes user requests there, so the newly promoted bits respond before live users hit them. By default, app settings and connection strings marked as slot settings stay with their slot — staging keeps its test database connection string while production keeps its production one after swap — while non-sticky settings swap with the code. Custom domain bindings remain attached to the production slot name, so after swap the newly promoted bits immediately answer on the public domain without DNS changes. Swap is not a database migration tool: schema changes must remain backward-compatible across swap, or migrations must be coordinated separately to avoid breaking the still-live old slot during the transition window.

---

## Q12. What is the difference between sticky (slot-specific) and non-sticky app settings and connection strings when using deployment slots?

**Concepts**
- Sticky setting — bound to the slot, does not travel on swap
- Non-sticky setting — travels with the deployed code on swap
- Production database credentials must be sticky to prevent misrouting
- ARM template slotSetting property controlling stickiness

**Answer**

A sticky setting is marked as a deployment slot setting, which means it stays bound to the slot that owns it and does not travel when you swap slots. A non-sticky setting swaps together with the application code, which is useful when a configuration value is tied to the build itself rather than to the environment — for example, a feature flag default baked into that release. Production connection strings should typically be sticky so the staging slot continues pointing at the test database after swap while production keeps its production database, preventing a promoted build from accidentally writing to the wrong server. In the Azure portal, checking "Deployment slot setting" on an app setting sets the sticky flag; ARM templates and Bicep express the same through the `slotSetting` property. Misconfigured stickiness is a common source of post-swap incidents: if a production secret is not sticky, it can swap into staging and expose production credentials on a publicly reachable staging URL.

---

## Q13. How do you configure application settings and connection strings for an ASP.NET Core app in App Service, and how do they map to `IConfiguration` at runtime?

**Concepts**
- App Service settings injected as environment variables before Kestrel starts
- Double-underscore hierarchy separator mapping to IConfiguration key paths
- Connection string prefix conventions — CUSTOMCONNSTR_, SQLAZURECONNSTR_
- Environment variable precedence over appsettings.json file values

**Answer**

App Service application settings and connection strings are injected as environment variables into the worker process before Kestrel starts, and ASP.NET Core's default configuration providers read them automatically through the same key hierarchy as local development. Settings defined in the portal under Configuration become environment variables where double underscores represent nested JSON sections — `Logging__LogLevel__Default` overrides the `Logging:LogLevel:Default` value from `appsettings.json` without any change to `Program.cs`. Connection strings defined in App Service appear as environment variables prefixed with `CUSTOMCONNSTR_`, `SQLAZURECONNSTR_`, or `MYSQLCONNSTR_`, and the connection strings provider maps them into `IConfiguration` under the `ConnectionStrings` section so `GetConnectionString("MyDb")` resolves them correctly. Environment-specific files like `appsettings.Production.json` still load when `ASPNETCORE_ENVIRONMENT` is `Production`, but App Service settings override file values because environment variables rank later in the configuration chain, which means storing secrets only in App Service configuration avoids committing production credentials to source control.

---

## Q14. What is the role of the `ASPNETCORE_ENVIRONMENT` environment variable on App Service, and how does it relate to `appsettings.{Environment}.json`?

**Concepts**
- ASPNETCORE_ENVIRONMENT selecting environment-specific configuration and middleware
- appsettings.{Environment}.json loaded after base appsettings.json
- Development middleware disabled in Azure unless variable is explicitly set
- Sticky environment variable per slot preventing post-swap environment mismatch

**Answer**

`ASPNETCORE_ENVIRONMENT` tells ASP.NET Core which hosting environment name is active, which selects environment-specific configuration files and controls branching in `Program.cs` — such as enabling the development exception page versus the production error handler. On App Service you set this variable in Configuration → Application settings, typically to `Production` for live sites, and the host loads `appsettings.Production.json` after `appsettings.json`, applying overrides like reduced log verbosity. Development-only middleware like `UseDeveloperExceptionPage` remains disabled in Azure unless you explicitly set the environment to `Development`, which you should avoid on internet-facing production slots since it exposes stack traces. Each deployment slot can define its own `ASPNETCORE_ENVIRONMENT` marked sticky, so staging stays `Staging` after a swap while production remains `Production`, ensuring the right middleware pipeline and configuration file are active in each slot without manual intervention after every swap.

---

## Q15. What is Kudu, and what capabilities does the Advanced Tools (Kudu) site provide for troubleshooting App Service deployments?

**Concepts**
- Kudu — deployment engine and diagnostic console behind App Service
- Debug Console for file system inspection of wwwroot
- Deployment Center history correlating pipeline results with live state
- Log streaming aggregating platform and application stdout in real time

**Answer**

Kudu is the deployment engine and diagnostic console behind App Service, exposed at `https://{app-name}.scm.azurewebsites.net` as Advanced Tools in the Azure portal. The Debug Console lets you browse `site/wwwroot` to confirm that `dotnet publish` output — the `.dll`, dependencies, and `web.config` — is present and matches the expected build version, which is the first check when a pipeline reports success but the site still serves an old build. Deployment Center and the `/api/deployments` endpoint show history, status, and logs for each push so you can identify exactly which deployment introduced a regression. Log streaming aggregates platform logs, Docker container logs on Linux, and application stdout and stderr so you can watch Kestrel startup errors in real time without downloading files. Kudu also supports running arbitrary commands in the worker sandbox, enabling quick checks like `dotnet --info` or verifying which environment variables are visible to your ASP.NET Core process.

---

## Q16. What is the Always On setting in App Service, and why does it matter for ASP.NET Core background work and cold-start behavior?

**Concepts**
- Always On — periodic ping preventing idle worker process unload
- Cold start — JIT compilation and DI container rebuild on first request
- IHostedService and BackgroundService stopped when process unloads
- Tier requirement — unavailable on Free and Shared plans

**Answer**

Always On sends periodic requests to the application's root URL so the worker process stays loaded even when external traffic is idle, which prevents idle shutdown on supported tiers. Without it, App Service may unload the site after a period of inactivity, causing a cold start that reinitializes Kestrel, JIT-compiles methods, and rebuilds the dependency injection container on the next request, adding several seconds of delay that hurts user experience for intermittently used apps. Background services implemented with `IHostedService` or `BackgroundService` stop when the process unloads, so scheduled or queue-processing work requires Always On to run reliably — a background worker that appears to stop randomly under low traffic is almost always caused by idle process unload. Always On is unavailable on Free and Shared tiers; production ASP.NET Core deployments on Basic and above should enable it unless the app runs in a continuously busy traffic pattern that naturally keeps the process warm.

---

## Q17. What is the difference between manual scaling, vertical scaling (changing plan tier), and autoscale rules on an App Service Plan?

**Concepts**
- Manual scaling — fixed instance count requiring human change
- Vertical scaling — larger machine per instance, not more instances
- Autoscale rules — metric-driven dynamic instance count adjustment
- Scaling applies at plan level affecting all hosted apps

**Answer**

Manual scaling changes the instance count on a fixed plan tier, which is simple and predictable but requires human intervention for traffic spikes. Vertical scaling moves the plan to a higher or lower pricing tier with different CPU and memory per instance, which helps single-instance workloads that cannot scale out due to session state or licensing limits, since it gives each instance more horsepower rather than adding more instances. Autoscale rules on Standard and above tiers add or remove instances when average CPU, memory, HTTP queue length, or custom Application Insights metrics cross thresholds you define, with cooldown periods to prevent flapping, so the system adapts to diurnal traffic patterns without manual intervention. All three operate at the App Service Plan level, so every web app on that plan experiences the scaling action together — you cannot autoscale one app on a shared plan independently of its siblings, which is another reason to separate apps with different load profiles into separate plans.

---

## Q18. What is Application Request Routing (ARR) affinity (sticky sessions), and when should you enable or disable it for a stateless ASP.NET Core API?

**Concepts**
- ARR affinity cookie routing requests to the same worker instance
- In-memory session state dependency on a specific instance
- Stateless JWT-authenticated APIs — affinity should be disabled
- Uneven load distribution under sticky session routing

**Answer**

ARR affinity instructs the Azure front-end load balancer to route subsequent requests from the same client to the same worker instance using a cookie, and it exists for applications that store session state in local memory. Most modern ASP.NET Core APIs should leave it disabled, because with affinity on, scale-in events can drop in-memory session data for users pinned to removed instances, causing unexpected logouts unless state is externalized. Stateless ASP.NET Core Web APIs that authenticate with JWTs and persist data in SQL or Redis perform correctly with affinity off, since every instance has identical state and the load balancer can distribute requests evenly across all instances. You should enable affinity only when relying on ASP.NET Core session state stored in memory on a specific instance and have not yet migrated to a distributed cache like Redis — it is a temporary accommodation for stateful design, not a feature to enable by default. Affinity is configured under Configuration → General settings and affects routing before requests reach Kestrel, so it does not replace proper session architecture for multi-instance deployments.

---

## Q19. How do you bind a custom domain and configure Transport Layer Security (TLS)/SSL certificates for an App Service web app?

**Concepts**
- CNAME record for subdomains, A record plus TXT for apex domains
- App Service Managed Certificate — automated issuance and renewal
- HTTPS-only enforcement and minimum TLS version hardening
- Key Vault certificate binding for wildcard or corporate CA requirements

**Answer**

Custom domain binding maps a DNS name to the App Service site's default hostname through a CNAME record for subdomains or an A record plus a verification TXT record for apex domains, after which you attach a TLS certificate so HTTPS terminates at the platform front end. After adding the custom domain in the portal, App Service displays the required DNS targets; propagation delays are the most common reason a binding shows "Not verified" immediately after configuration. App Service Managed Certificate automates issuance and renewal for domains you own when DNS is correctly delegated, removing manual expiry tracking for many production sites. Uploading a `.pfx` or binding a Key Vault certificate suits wildcard domains or corporate policies requiring a specific certificate authority. Enforcing HTTPS-only and setting a minimum TLS version of 1.2 or 1.3 under Configuration → General settings redirects HTTP clients and blocks outdated protocols, which is standard hardening complementing `UseHsts()` and HTTPS redirection already present in typical `Program.cs` production pipelines.

---

## Q20. What is Azure App Service managed identity, and how can an ASP.NET Core app use it to access Azure Key Vault or other Azure resources without storing secrets in configuration?

**Concepts**
- Managed identity — platform-provisioned Entra ID service principal
- System-assigned vs user-assigned identity lifecycle
- DefaultAzureCredential working across dev and production
- Key Vault secret access without credentials in configuration

**Answer**

Managed identity gives an App Service a Microsoft Entra ID service principal that Azure automatically provisions and rotates, so the application authenticates to other Azure services without client secrets anywhere in configuration. In ASP.NET Core you enable the identity on the web app, grant it access policies or RBAC role assignments on Key Vault or other resources, and use `DefaultAzureCredential` or the Azure SDK to obtain tokens at runtime. System-assigned identity is tied to the web app's lifecycle and is deleted if the app is removed; user-assigned identity is a standalone resource you can attach to multiple apps. In Key Vault, you grant the identity `Get` permission on secrets rather than embedding connection strings in App Service settings, so the app reads secrets by name through the Key Vault configuration provider or SDK. `DefaultAzureCredential` tries managed identity in Azure and falls back to local developer credentials during development, keeping the same code path across environments and eliminating secret sprawl in ZIP deploy artifacts and portal settings.

---

## Q21. What is Virtual Network (VNet) integration for App Service, and when do you need it to reach private databases or internal APIs?

**Concepts**
- VNet integration routing outbound traffic through a delegated subnet
- Regional VNet integration for Standard tier and above
- Private endpoint on Azure SQL or other resources enabling private IP access
- Network security groups controlling egress from the integration subnet

**Answer**

VNet integration routes outbound traffic from the App Service worker through a subnet in an Azure Virtual Network, allowing the app to reach private IP addresses such as Azure SQL with a private endpoint, internal REST APIs, or on-premises systems via ExpressRoute or VPN. Without it, App Service uses public internet egress and cannot resolve or connect to resources that block public access, which is a common requirement for compliance or zero-trust architectures. Regional VNet integration on Standard tier and above is the common choice for ASP.NET Core apps that query a privately linked database while the app itself remains publicly reachable — it provides private outbound access without hiding the app's public URL. Inbound private access requires separate features such as private endpoints on the App Service resource or an Application Gateway front end, since outbound VNet integration alone does not hide the app's public URL from the internet. Network security groups and route tables on the integration subnet control which destinations the app may reach, letting security teams enforce least-privilege egress from the web tier.

---

## Q22. How does App Service health check work, and how does it interact with load balancing and instance replacement during scale operations?

**Concepts**
- Health check path — periodic probe removing unhealthy instances from rotation
- MapHealthChecks middleware exposing the health endpoint
- Unhealthy instance preferential removal during scale-in
- Health check vs Application Insights availability tests

**Answer**

App Service health check periodically requests a path you configure — such as `/health` — on each worker instance and removes instances that fail repeatedly from the load balancer rotation, so healthy instances continue receiving traffic while unhealthy ones are restarted or replaced. You enable health check under Monitoring → Health check by specifying a relative URL that returns HTTP 200 when Kestrel, the database, and critical dependencies are available, often implemented with ASP.NET Core health checks middleware via `MapHealthChecks`. Instances failing the probe are marked unhealthy and excluded from Application Request Routing distribution, reducing cascading 502 Bad Gateway responses during partial outages. During scale-in, unhealthy instances are preferentially removed first, so a misconfigured health path that always returns 404 can cause the platform to drain every new instance incorrectly — the health path must genuinely reflect service readiness. Health check complements but differs from Application Insights availability tests: platform health check governs instance routing inside App Service, while external probes validate end-to-end user-visible uptime.

---

## Q23. What logging and diagnostics options are available on App Service for an ASP.NET Core application (platform logs, application logs, Application Insights)?

**Concepts**
- Application Logging capturing stdout from the ASP.NET Core logging pipeline
- Failed Request Tracing and Detailed Error Messages for platform-level HTTP failures
- Application Insights — structured telemetry with request and dependency correlation
- Log Stream and Kudu download for immediate incident investigation

**Answer**

App Service captures platform-level web server logs, deployment logs, and application stdout/stderr, and you can forward structured ASP.NET Core logging to Application Insights for queryable telemetry. Application Logging (Filesystem or Blob) captures stdout from `Console` and debug providers when ASP.NET Core writes through the default logging pipeline, so `ILogger` calls flow through. Detailed Error Messages and Failed Request Tracing on Windows provide low-level HTTP diagnostics beyond what ASP.NET Core's exception middleware records, useful for platform-level failures outside application code. Application Insights, enabled via the portal or `AddApplicationInsightsTelemetry()`, ingests traces, requests, dependencies, and exceptions with correlation IDs across SQL and HTTP outbound calls, giving you one place to correlate Kestrel output, HTTP failures, and custom log scopes from the same investigation. Log Stream and Kudu download give immediate access during incidents while Blob storage retention supports longer forensic review.

---

## Q24. What is the purpose of a startup command or startup script on Linux App Service, and when is it required for .NET applications?

**Concepts**
- Startup command telling the container entry point which process to launch
- Oryx auto-detection failing when deployment layout is non-standard
- dotnet YourApp.dll as the typical startup command for framework-dependent deployments
- Windows App Service relying on ASP.NET Core Module in web.config instead

**Answer**

On Linux App Service, the startup command tells the container entry point which process to launch — typically `dotnet MyApp.dll` — when the platform cannot infer the correct assembly from the deployment layout alone. It is required when publishing a framework-dependent deployment without the default Oryx auto-detection path, or when custom arguments like `--urls` or environment setup are needed before Kestrel starts. The command is configured under Configuration → General settings → Startup Command and runs in the context of `/home/site/wwwroot` where the publish output lives. Self-contained deployments that include the runtime may use `./MyApp` directly instead of the `dotnet` launcher, but the startup command must still reference the correct executable name. Windows App Service generally does not need a manual startup command because the ASP.NET Core Module in `web.config` points IIS to the right `.dll`, which is why Linux App Service makes explicit startup configuration more common and is worth verifying whenever a Linux-hosted app returns a 502 immediately after deployment.

---

## Q25. How do you troubleshoot a failed deployment or a web app that returns HTTP 502/503 errors on App Service using Kudu and the Log Stream?

**Concepts**
- HTTP 502 — worker process exited or not listening on expected port
- HTTP 503 — all instances failing health check or plan restart
- Kudu log files — application logs and Docker container logs for root cause
- Startup exception in Program.cs from missing settings or failed DI registration

**Answer**

HTTP 502 and 503 on App Service usually mean Kestrel failed to start, crashed on startup, or is not listening on the port the platform expects, and Kudu plus Log Stream reveal the underlying exception or deployment error. I start from Deployment Center history to confirm the latest publish succeeded, then stream application logs while restarting the site to capture startup stack traces. A 502 Bad Gateway often indicates the worker process exited immediately — common causes include a missing `.dll`, wrong .NET version on the plan, or an unhandled exception in `Program.cs` during service registration. A 503 Service Unavailable can appear during swap warm-up, plan restart, or when all instances fail health check simultaneously, so I check whether the issue correlates with a recent deployment or scale event. In Kudu, `LogFiles/Application` and `LogFiles/eventlog.xml` on Windows or Docker logs on Linux contain the exact runtime error that the portal summary omits. A frequent root cause is a secret resolution failure at startup — a missing Key Vault reference or incorrect connection string prevents the DI container from building and produces the same gateway errors as a code bug.

---

## Q26. What is the relationship between an App Service web app, its resource group, and Azure Resource Manager (ARM) templates in infrastructure-as-code deployments?

**Concepts**
- Resource group as the lifecycle and deployment boundary
- ARM template declaring web app, plan, and serverFarmId dependency
- Parameter files enabling the same template across dev, test, and production
- Separation of infrastructure deployment from application code deployment

**Answer**

An App Service web app is an Azure Resource Manager resource that always belongs to a resource group and depends on an App Service Plan in the same subscription, so ARM templates or Bicep modules declare all three to reproduce environments consistently. The resource group is the lifecycle boundary for deployment operations — deleting the group removes the web app, plan, and related monitoring resources together, which is why production designs usually treat the resource group as a long-lived container protected by resource locks. ARM templates express dependencies explicitly: the web app resource references `serverFarmId` so Azure creates the plan before binding the app, preventing orphan resources during automated provisioning. Infrastructure-as-code pipelines deploy the template to dev, test, and production subscriptions with different parameter files while keeping the ASP.NET Core application deployment as a separate release step, since treating the web app and plan as code artifacts documents SKU choices and region decisions in source control rather than only in the portal, supporting audit and disaster recovery scenarios.

---

## Q27. What are common security practices for hardening an ASP.NET Core App Service deployment (HTTPS-only, minimum TLS version, IP restrictions, authentication)?

**Concepts**
- HTTPS-only enforcement and minimum TLS version blocking outdated protocols
- IP access restrictions limiting exposure of management and admin endpoints
- Managed identity and Key Vault eliminating secrets from deployment artifacts
- Entra authentication (Easy Auth) for quick staging slot protection

**Answer**

Hardening App Service combines platform configuration with ASP.NET Core security middleware so transport, network access, and identity are enforced before requests reach controllers or Razor Pages. I enable HTTPS-only and TLS 1.2 minimum under Configuration → General settings so clients cannot downgrade to unencrypted HTTP, complementing `UseHsts()` and HTTPS redirection already present in typical `Program.cs` production pipelines. Access restrictions — IP allow lists or Azure Front Door integration — limit who can reach management endpoints and the public site, which is valuable for internal admin APIs or pre-release slots. I enable Microsoft Entra authentication (Easy Auth) at the App Service layer for quick protection of staging sites, or rely on ASP.NET Core JWT and cookie middleware for fine-grained authorization inside the app. Secrets belong in Key Vault with managed identity, remote debugging should be disabled in production, the runtime should stay on supported .NET versions, and deployment artifacts should be scanned in CI to prevent credential leaks from reaching `wwwroot`.

---

## Q28. When would you choose Azure App Service over Azure Container Apps, Azure Functions, or AKS for hosting an ASP.NET Core web application?

**Concepts**
- App Service for long-running HTTP workloads with minimal operational overhead
- Azure Functions for short-lived event-driven handlers
- Azure Container Apps for containerized microservices with scale-to-zero
- AKS for full orchestration control and complex multi-team service topologies

**Answer**

I choose App Service when I want the simplest managed hosting for a long-running ASP.NET Core web app or API with minimal container or orchestration overhead, and need built-in deployment slots, custom domains, and autoscale without managing infrastructure. Azure Functions fits short event-driven handlers and webhooks but is a poor primary host for full MVC or Razor Pages applications that expect continuous Kestrel availability and deployment slot workflows. Azure Container Apps suits containerized microservices where you want Kubernetes-style scale-to-zero and Dapr integration without managing a cluster, or when sidecar patterns or custom Linux images with native dependencies are needed. AKS belongs to multi-team platforms that need custom ingress, GPU, service mesh, or complex service topologies — the full orchestration control comes with significant platform engineering overhead. App Service remains the default for teams publishing framework-dependent `dotnet publish` output from Visual Studio or GitHub Actions who do not need Dockerfiles unless container portability is a specific requirement.

---

## Gotchas — Azure App Service (Interview Traps)

---

#### Gotcha 1. Always On is disabled by default on Basic and below — hosted services silently stop

**Concepts**
- Always On keeps the worker process alive between requests
- Free and Shared tiers do not support Always On
- IHostedService and BackgroundService workers require a persistent process
- Without Always On the app pool is recycled after ~20 minutes of inactivity

**Answer**

If Always On is disabled, IIS on Windows App Service recycles the worker process after a period of inactivity, which terminates IHostedService and BackgroundService workers and causes timer-driven jobs to stop firing. This trap is invisible in the portal until you notice that scheduled tasks stop running overnight. Always On requires at least the Basic pricing tier; developers who test on Free tier never see the issue because Free tier traffic usually keeps the process alive during testing, but overnight idle in production causes silent failures.

---

#### Gotcha 2. Deployment slot swap sends non-sticky settings to production but not sticky ones — apps mix old and new config

**Concepts**
- Sticky (slot-specific) settings stay with the slot, not the content
- Non-sticky settings are swapped together with the deployment
- Feature flags or third-party API keys stored as non-sticky follow the swap
- Incorrect stickiness causes production to use a staging-tier secret

**Answer**

When you swap staging to production, all non-sticky application settings travel with the code to production, while sticky settings remain with their respective slot. A common mistake is forgetting to mark a staging-only secret (for example a Stripe test key or a lower-tier database connection) as slot-specific, so after the swap production starts calling the staging payment endpoint. Conversely, marking a setting sticky when it should follow the code means the new app version boots in production with the old production setting, missing a configuration change that the staging validation depended on.

---

#### Gotcha 3. The SCM (Kudu) site shares the same custom domain firewall rules but has no separate IP restriction by default

**Concepts**
- SCM endpoint is at `<appname>.scm.azurewebsites.net`
- IP restrictions on the main site do not apply to the SCM site automatically
- Kudu exposes the file system, debug console, and log stream
- Unintended public SCM access is a security risk in production

**Answer**

App Service IP access restrictions configured on the main site endpoint do not automatically carry over to the SCM (Advanced Tools / Kudu) endpoint. This means an API with restricted public access still has its Kudu console, environment variable dump, and file system browser open to the internet. You must configure a separate IP restriction rule under the SCM site tab in Access restrictions. Production hardening checklists routinely miss this because testing the main site works correctly, but Kudu remains unguarded until an explicit rule is added.

---

#### Gotcha 4. ARR Affinity cookie breaks stateless horizontal scaling

**Concepts**
- ARR Affinity routes a client to the same instance via a cookie
- Stateless APIs should have ARR Affinity disabled
- Enabled ARR Affinity causes uneven load distribution during autoscale
- Disabling it requires an explicit portal toggle or ARM property

**Answer**

Application Request Routing Affinity is enabled by default on App Service and pins each client to a single backend instance via a session cookie. For stateless ASP.NET Core APIs this is unnecessary and counterproductive: when autoscale adds instances, existing clients are still routed to their original instance, causing uneven load distribution where some instances are saturated while new ones are idle. It also adds a small cookie overhead to every response. You should disable ARR Affinity for stateless Web APIs and ensure any distributed state (session, cache) lives in Redis or Azure SQL, not in-process.

---

#### Gotcha 5. Connection strings in App Service portal use a different environment variable prefix than app settings

**Concepts**
- App settings map to `MY__KEY` (double underscore for section separator)
- Connection strings map to `ConnectionStrings__Name` automatically
- `GetConnectionString()` reads the `ConnectionStrings:` section, not root config
- Placing a connection string in the app settings blade instead of connection strings blade changes the key path

**Answer**

App Service exposes two configuration blades: Application Settings and Connection Strings. Values from the Connection Strings blade are injected as `ConnectionStrings__Name` environment variables, which ASP.NET Core `IConfiguration` maps to `ConnectionStrings:Name`, the exact path `GetConnectionString("Name")` reads. If a developer places the connection string in the Application Settings blade instead, the key is `Name` at the root of configuration, and `GetConnectionString("Name")` returns null while `Configuration["Name"]` returns the value — a mismatch that is invisible until you check the blade type at runtime.

---

#### Gotcha 6. Run From Package locks the wwwroot folder — simultaneous deploys and file edits fail silently

**Concepts**
- `WEBSITE_RUN_FROM_PACKAGE=1` mounts the ZIP as a read-only virtual file system
- File edits via Kudu console are silently discarded
- A new deploy replaces the mounted package after a restart
- Deployment collisions on the same slot fail without a clear error

**Answer**

When `WEBSITE_RUN_FROM_PACKAGE` is enabled, the application files are served from a mounted ZIP package and the `wwwroot` directory is read-only. Any file changes made through the Kudu console or FTP appear to succeed but are lost on the next request because the mounted package overrides them. Additionally, if two deployments race to the same slot the second deploy can fail silently because the package swap requires a restart, which the first deploy's warm-up may not have completed. The fix is to deploy one version at a time and rely on deployment slot swap for zero-downtime releases.

---

#### Gotcha 7. Log Stream only shows stdout/stderr — Application Insights traces are invisible there

**Concepts**
- Log Stream reads the platform-level stdout and stderr redirected from the process
- ILogger traces routed to Application Insights do not appear in Log Stream
- `ASPNETCORE_LOGGING__CONSOLE__FORMATTERNAME` controls console format for Log Stream
- Developers confuse no Log Stream output with no logging at all

**Answer**

Log Stream in the Azure portal shows only the output your process writes to stdout and stderr, which is the console sink for ILogger. If you configure ILogger to use only Application Insights as a sink (removing the console provider), Log Stream will appear empty even though traces are flowing to Application Insights. Conversely, if console logging is too verbose it floods Log Stream with noise. Developers troubleshooting a production issue who see an empty Log Stream often conclude the app is not logging, when actually logs are in Application Insights under a different query path.

---

#### Gotcha 8. Autoscale rules trigger on metrics with a delay — scale-in cooldown can leave excess instances running for minutes

**Concepts**
- Autoscale triggers fire after the metric aggregation window elapses
- Scale-out adds instances faster than scale-in removes them (asymmetric cooldown)
- Default cooldown is 5 minutes, causing cost overrun during traffic bursts
- Scale-in rules require a separate rule; without one, instances never scale in

**Answer**

Autoscale scale-out rules typically have a 5-minute metric window plus up to a 5-minute cooldown, meaning new instances may not appear for 10 minutes after traffic spikes. More importantly, if you configure only scale-out rules and forget the corresponding scale-in rule, App Service will add instances during traffic bursts and never remove them, continuously accumulating compute cost. The asymmetric default behavior — scale out quickly to prevent overload, scale in slowly to prevent flapping — means autoscale during a brief traffic spike leaves excess instances running for the full cooldown period after the spike ends.

---

#### Gotcha 9. Managed identity role assignments take several minutes to propagate — immediate post-assignment tests fail

**Concepts**
- RBAC role assignments propagate through Azure AD with eventual consistency
- A 403 immediately after assignment does not mean the assignment is wrong
- `DefaultAzureCredential` uses cached tokens; a new assignment isn't reflected until token expiry
- Retry after 2–5 minutes is required before concluding there is a misconfiguration

**Answer**

After assigning a managed identity to a Key Vault or Storage Account role, the assignment does not take effect instantly. Azure RBAC propagates through Azure Active Directory with eventual consistency that can take 2–5 minutes, during which requests from the managed identity continue to receive 403. Developers who immediately test after assignment and see 403 often add more roles or check the wrong resource, when the problem is simply propagation delay. Additionally, `DefaultAzureCredential` caches the access token for its lifetime (up to one hour), so even after propagation the old cached token may be used until it expires.

---

#### Gotcha 10. Free and Shared tier apps share compute infrastructure — CPU quotas reset daily and the app is suspended when exceeded

**Concepts**
- Free tier has a 60-minute/day CPU quota shared across all apps in the plan
- Exceeding the quota suspends all apps in the Free/Shared plan until midnight UTC
- Shared tier has a higher quota but still shares underlying infrastructure
- Basic tier provides dedicated compute and removes the CPU quota suspension

**Answer**

On Free and Shared App Service tiers, all apps within the plan share underlying CPU cores and are subject to a daily CPU minute quota. When the quota is exceeded, Azure suspends all apps in the plan and they return HTTP 403 until the quota resets at midnight UTC. This suspension happens without warning in the Azure portal during the day and is commonly mistaken for a deployment failure or application bug. Moving to the Basic tier or higher provides dedicated compute without CPU quotas, which is required for any production workload or application with consistent background processing.

---
