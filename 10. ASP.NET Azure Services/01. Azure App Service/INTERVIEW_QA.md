# Azure App Service — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [What is Azure App Service, and how does it differ from running an ASP.NET Core a…](#q1)
2. [What is an App Service Plan, and what compute resources does it define for the w…](#q2)
3. [What are the main App Service Plan pricing tiers (Free, Shared, Basic, Standard,…](#q3)
4. [Can multiple web apps share one App Service Plan? What are the cost and performa…](#q4)
5. [How does Azure App Service host an ASP.NET Core application at runtime — what is…](#q5)
6. [What is the difference between deploying an ASP.NET Core app to a Windows App Se…](#q6)
7. [What deployment methods are available for publishing an ASP.NET Core app to App …](#q7)
8. [What is ZIP deploy, and why is it commonly used in continuous integration and co…](#q8)
9. [What is the "Run From Package" deployment option, and what performance and locki…](#q9)
10. [What are deployment slots in Azure App Service, and how do they support zero-dow…](#q10)
11. [How does slot swapping work, and what happens to application settings, connectio…](#q11)
12. [What is the difference between sticky (slot-specific) and non-sticky app setting…](#q12)
13. [How do you configure application settings and connection strings for an ASP.NET …](#q13)
14. [What is the role of the `ASPNETCORE_ENVIRONMENT` environment variable on App Ser…](#q14)
15. [What is Kudu, and what capabilities does the Advanced Tools (Kudu) site provide …](#q15)
16. [What is the Always On setting in App Service, and why does it matter for ASP.NET…](#q16)
17. [What is the difference between manual scaling, vertical scaling (changing plan t…](#q17)
18. [What is Application Request Routing (ARR) affinity (sticky sessions), and when s…](#q18)
19. [How do you bind a custom domain and configure Transport Layer Security (TLS)/SSL…](#q19)
20. [What is Azure App Service managed identity, and how can an ASP.NET Core app use …](#q20)
21. [What is Virtual Network (VNet) integration for App Service, and when do you need…](#q21)
22. [How does App Service health check work, and how does it interact with load balan…](#q22)
23. [What logging and diagnostics options are available on App Service for an ASP.NET…](#q23)
24. [What is the purpose of a startup command or startup script on Linux App Service,…](#q24)
25. [How do you troubleshoot a failed deployment or a web app that returns HTTP 502/5…](#q25)
26. [What is the relationship between an App Service web app, its resource group, and…](#q26)
27. [What are common security practices for hardening an ASP.NET Core App Service dep…](#q27)
28. [When would you choose Azure App Service over Azure Container Apps, Azure Functio…](#q28)

---

## Q1. What is Azure App Service, and how does it differ from running an ASP.NET Core application on a virtual machine (VM) or in Azure Kubernetes Service (AKS)?

What is Azure App Service, and how does it differ from running an ASP.NET Core application on a virtual machine (VM) or in Azure Kubernetes Service (AKS)?

**Answer:** Azure App Service is a fully managed Platform-as-a-Service (PaaS) offering for hosting web applications, REST APIs, and mobile backends without you provisioning or patching the underlying operating system. Microsoft operates the web server, load balancing, and scaling infrastructure while you deploy your published ASP.NET Core output and configure settings through the portal or automation.

- On a virtual machine, you install the runtime, configure Internet Information Services (IIS) or Kestrel as a service, apply security patches, and manage load balancers yourself — App Service removes that operational burden in exchange for less control over the host.
- Azure Kubernetes Service (AKS) runs containers on a cluster you design and operate (nodes, ingress, Helm charts, pod scaling), which suits complex microservice topologies but adds significant platform engineering overhead compared to App Service.
- App Service integrates deployment slots, built-in autoscale, custom domains, and managed certificates as first-class features, whereas those capabilities on a VM or AKS require additional Azure resources and configuration.
- For a typical ASP.NET Core Razor Pages or Web API workload like the sample project in this module, App Service is the simplest path from `dotnet publish` to a public HTTPS endpoint with minimal infrastructure code.

---

## Q2. What is an App Service Plan, and what compute resources does it define for the web apps that run on it?

What is an App Service Plan, and what compute resources does it define for the web apps that run on it?

**Answer:** An App Service Plan is the billing and capacity unit that defines the virtual machine size, region, pricing tier, and scale limits for every web app assigned to it. The plan — not the individual web app — owns the compute: central processing unit (CPU), memory, storage, and the maximum number of instances you can run.

- All web apps on the same plan share the same pool of worker instances, so heavy traffic on one app can consume CPU and memory that other apps on that plan would otherwise use.
- The plan tier controls features such as deployment slots, daily backup, custom domains, and Virtual Network (VNet) integration; a Free or Shared plan omits many production features available on Standard and Premium tiers.
- Scaling actions — manual instance count changes or autoscale rules — apply at the plan level, meaning every app on that plan scales out together when you add instances.
- When Visual Studio or an Azure Resource Manager (ARM) template creates a web app, it typically provisions a matching plan (for example `Plan` plus a unique suffix) because a web app cannot exist without one.

---

## Q3. What are the main App Service Plan pricing tiers (Free, Shared, Basic, Standard, Premium, Isolated), and how do they differ in terms of features and scaling?

What are the main App Service Plan pricing tiers (Free, Shared, Basic, Standard, Premium, Isolated), and how do they differ in terms of features and scaling?

**Answer:** App Service Plan tiers trade cost against compute power, isolation, and platform features, ranging from Free and Shared multi-tenant hobby tiers through Basic and Standard production tiers to Premium and Isolated enterprise options. Higher tiers provide dedicated workers, more scale-out instances, deployment slots, and advanced networking that lower tiers omit or limit severely.

| Tier | Typical use | Notable limits |
|------|-------------|----------------|
| Free / Shared | Learning, demos | Shared workers, no custom domains on Free, no Always On |
| Basic | Small production | Dedicated workers, manual scale, limited slots |
| Standard | Production web apps | Autoscale, five deployment slots, daily backup |
| Premium | High traffic / VNet | Larger SKUs, more slots, better performance |
| Isolated | Compliance / VNet injection | Private dedicated workers in your subnet |

- Free and Shared plans run on shared infrastructure with strict quotas and are unsuitable for production ASP.NET Core workloads that need custom domains or guaranteed uptime.
- Standard is the common starting point for production because it unlocks autoscale, staging slots, and backup — features teams rely on for safe ASP.NET Core releases.
- Premium and Isolated tiers add larger machine sizes, more instances, and App Service Environment (ASE)–style network isolation for regulated workloads that must reach private endpoints inside a VNet.

---

## Q4. Can multiple web apps share one App Service Plan? What are the cost and performance implications of that model?

Can multiple web apps share one App Service Plan? What are the cost and performance implications of that model?

**Answer:** Yes — multiple web apps can run on a single App Service Plan, and Azure bills you once per plan rather than per app, which makes plan sharing a common cost-optimization strategy for related services. All apps on that plan compete for the same worker instances, so their combined CPU, memory, and connection usage determines whether the plan is adequately sized.

- Sharing a plan reduces cost when you host several low-traffic internal tools or microservices that never peak simultaneously, because you pay for one set of workers instead of many underutilized plans.
- Performance isolation is logical, not physical: a memory leak or traffic spike in one ASP.NET Core app can slow every other app on the same plan because they share process space on the same worker instance.
- Scaling out adds instances to the entire plan, so you cannot scale one app independently while leaving sibling apps on a single instance unless you move that app to its own plan.
- Production teams often group apps with similar criticality and load profiles on one Standard or Premium plan, while isolating high-traffic or noisy-neighbor-sensitive APIs on dedicated plans.

---

## Q5. How does Azure App Service host an ASP.NET Core application at runtime — what is the relationship between the platform front end, Kestrel, and your published files?

How does Azure App Service host an ASP.NET Core application at runtime — what is the relationship between the platform front end, Kestrel, and your published files?

**Answer:** On App Service, Azure's front-end load balancers terminate incoming HTTP or HTTPS connections and forward requests to a worker instance where your ASP.NET Core app runs Kestrel as the in-process web server. Your published folder — the output of `dotnet publish` — contains the `.dll`, dependencies, `web.config` (on Windows), and static content that Kestrel serves after the platform starts the process.

- On Windows App Service, IIS historically acted as a reverse proxy in front of Kestrel using the ASP.NET Core Module; on modern stacks the platform still routes traffic to Kestrel listening on an assigned port rather than exposing Kestrel directly to the internet.
- The platform sets environment variables such as `PORT` or `ASPNETCORE_URLS` so Kestrel binds correctly inside the sandboxed worker without manual endpoint configuration in `Program.cs`.
- Static files under `wwwroot` are served by ASP.NET Core middleware (`UseStaticFiles`) after Kestrel receives the request, while the Azure front end may also cache or compress responses depending on configuration.
- Understanding this two-hop model — front-end load balancer to Kestrel — explains why platform settings (Always On, ARR affinity) affect process lifetime even though your code only references standard ASP.NET Core middleware.

---

## Q6. What is the difference between deploying an ASP.NET Core app to a Windows App Service and a Linux App Service?

What is the difference between deploying an ASP.NET Core app to a Windows App Service and a Linux App Service?

**Answer:** Both platforms run the same published .NET assemblies, but Windows App Service uses a Windows Server worker with IIS integration and a generated `web.config`, while Linux App Service runs your app in a Linux container image with Kestrel as the primary entry point and no IIS layer. The choice affects startup configuration, path conventions, and some platform features rather than the C# code you write.

- Windows deployment is familiar for teams migrating from on-premises IIS and supports certain legacy integration scenarios; Linux often provides better price-performance for stateless ASP.NET Core APIs and aligns with container-based workflows.
- On Linux, you may need an explicit startup command (`dotnet YourApp.dll`) in App Service configuration, whereas Windows deployments rely on the ASP.NET Core Module to launch the process defined in `web.config`.
- File path casing matters on Linux — references to `appsettings.Production.json` must match disk casing — while Windows is case-insensitive for configuration file loading.
- Both support the same App Service features at equivalent tiers (slots, autoscale, managed identity), but verify that any platform extension or site extension you need is available on your chosen operating system before committing.

---

## Q7. What deployment methods are available for publishing an ASP.NET Core app to App Service (for example Web Deploy, ZIP deploy, GitHub Actions, and Azure DevOps)?

What deployment methods are available for publishing an ASP.NET Core app to App Service (for example Web Deploy, ZIP deploy, GitHub Actions, and Azure DevOps)?

**Answer:** App Service accepts deployments through multiple channels that all push your published application package to the site's `wwwroot` (or mount it as a package), including Visual Studio Web Deploy, ZIP deploy via REST API or Azure CLI, Git-based continuous deployment, and pipeline tasks in GitHub Actions or Azure DevOps. The right method depends on whether you deploy from a developer workstation or an automated pipeline.

- Web Deploy (MSDeploy) integrates with Visual Studio Publish profiles — the sample project includes a `ServiceDependencies` Web Deploy profile targeting a named web app — and synchronizes files incrementally while optionally transforming configuration.
- ZIP deploy uploads a compressed publish folder to the Kudu endpoint (`/api/zipdeploy`) and is the standard approach in CI/CD because it is scriptable, idempotent, and works from any agent with network access to the site.
- GitHub Actions and Azure DevOps typically run `dotnet publish`, archive the output, and invoke ZIP deploy or the `AzureWebApp` task, often combined with deployment slots for staging before swap-to-production.
- Container-based App Service can pull from Azure Container Registry instead of deploying binaries, but framework-dependent ASP.NET Core deployments usually use ZIP or Web Deploy of the publish output.

---

## Q8. What is ZIP deploy, and why is it commonly used in continuous integration and continuous delivery (CI/CD) pipelines for App Service?

What is ZIP deploy, and why is it commonly used in continuous integration and continuous delivery (CI/CD) pipelines for App Service?

**Answer:** ZIP deploy is a REST-based mechanism that uploads a compressed archive of your published application to App Service's Kudu service, which extracts the contents into the site's file system and triggers a site restart. Pipeline agents use it because it requires only HTTPS and credentials — no Visual Studio or MSDeploy installation on the build machine.

- A typical pipeline step runs `dotnet publish -c Release -o ./publish`, zips the folder, and POSTs it to `https://{app-name}.scm.azurewebsites.net/api/zipdeploy` with a deployment credential or service principal token.
- ZIP deploy supports synchronous and asynchronous modes; asynchronous deployment returns immediately while Kudu processes extraction in the background, which suits large publish outputs.
- Because the entire application is replaced atomically from the platform's perspective, ZIP deploy pairs well with deployment slots: publish to the staging slot, validate, then swap to production.
- Unlike manual FTP uploads, ZIP deploy integrates with deployment history in Kudu so you can correlate a failed release with build artifacts and rollback by redeploying a previous ZIP.

---

## Q9. What is the "Run From Package" deployment option, and what performance and locking benefits does it provide for ASP.NET Core apps?

What is the "Run From Package" deployment option, and what performance and locking benefits does it provide for ASP.NET Core apps?

**Answer:** Run From Package mounts your deployment ZIP as a read-only filesystem at startup instead of extracting files into `wwwroot`, which reduces deployment time and eliminates file-lock conflicts during active requests. The setting `WEBSITE_RUN_FROM_PACKAGE=1` (or a blob URL pointing at the package) tells the worker to serve the app directly from the mounted archive.

- Extracting thousands of files on every deploy causes longer swap times and can fail if assemblies are locked by a running Kestrel process; mounting the package avoids touching individual files on disk during deployment.
- Read-only deployment reduces the risk of runtime file tampering and makes the running application directory a predictable, immutable artifact matching your build output.
- Cold start can improve because the platform skips extraction I/O, though very large packages still add mount time; storing the ZIP in Azure Blob Storage with a shared access signature (SAS) URL is a common pattern for multi-region consistency.
- Local development and debugging still use normal extracted folders; Run From Package is a production-oriented App Service configuration applied through application settings rather than a change to your project file.

---

## Q10. What are deployment slots in Azure App Service, and how do they support zero-downtime or low-risk releases?

What are deployment slots in Azure App Service, and how do they support zero-downtime or low-risk releases?

**Answer:** Deployment slots are separate instances of your web app — such as `staging` — that run on the same App Service Plan with their own host name, settings, and deployed bits while sharing compute with the production slot. You publish a new ASP.NET Core build to the staging slot, run validation tests against it, and then swap it into production with minimal downtime.

- Each slot has a unique URL (`{app-name}-staging.azurewebsites.net`), so quality assurance can hit the exact build that will go live without overwriting the production site prematurely.
- Because slots share the plan, staging does not require a duplicate billing unit, though Premium tiers allow more slots for canary or regional pre-production environments.
- Slot warm-up endpoints let you verify that Kestrel starts and database migrations succeed before traffic moves, reducing the chance of serving HTTP 502 responses immediately after a swap.
- If a release fails validation, you discard or redeploy the staging slot while production continues serving the previous build — a workflow that is difficult to replicate with single-instance FTP deployments.

---

## Q11. How does slot swapping work, and what happens to application settings, connection strings, and warm-up behavior during a swap?

How does slot swapping work, and what happens to application settings, connection strings, and warm-up behavior during a swap?

**Answer:** Slot swap exchanges the deployed content and most configuration between two slots by renaming their underlying workers' routing targets, so what was staging becomes production in seconds without re-uploading files. Azure performs a warm-up phase on the slot receiving production traffic to ensure Kestrel and dependencies initialize before the front end routes user requests there.

- By default, app settings and connection strings marked as "slot settings" stay with their slot; all other settings swap with the code, which lets staging use a test database while production keeps production connection strings after swap.
- The swap operation triggers `applicationInitialization` or the Auto-Swap warm-up ping (depending on configuration) so the incoming production slot processes requests only after the worker responds successfully.
- Custom domain bindings remain attached to the production slot name, so after swap the newly promoted bits immediately answer on your public domain without DNS changes.
- Swap is not a database migration tool: schema changes must remain backward compatible across swap, or you coordinate migrations separately to avoid breaking the still-live slot during the transition.

---

## Q12. What is the difference between sticky (slot-specific) and non-sticky app settings and connection strings when using deployment slots?

What is the difference between sticky (slot-specific) and non-sticky app settings and connection strings when using deployment slots?

**Answer:** A sticky setting is marked as a deployment slot setting, which means it stays bound to the slot that owns it and does not travel when you swap slots; a non-sticky setting swaps together with the application code. This distinction lets you keep environment-specific secrets and endpoints aligned with the correct slot even after a swap exchanges the published binaries.

- Production connection strings typically remain sticky so the staging slot continues pointing at a test database after swap while production keeps its production database — preventing a promoted build from accidentally writing to the wrong server.
- Non-sticky settings move with the code, which is useful when a configuration value is tied to the build itself (for example a feature flag default baked into that release) rather than the environment.
- In the Azure portal, checking "Deployment slot setting" on an app setting or connection string sets the sticky flag; ARM templates and Bicep express the same through the `slotSetting` property.
- Misconfigured stickiness is a common source of post-swap incidents: if a production secret is not sticky, it can swap into staging and expose production credentials on a publicly reachable staging URL.

---

## Q13. How do you configure application settings and connection strings for an ASP.NET Core app in App Service, and how do they map to `IConfiguration` at runtime?

How do you configure application settings and connection strings for an ASP.NET Core app in App Service, and how do they map to `IConfiguration` at runtime?

**Answer:** App Service application settings and connection strings are injected as environment variables into the worker process before Kestrel starts, and ASP.NET Core's default configuration providers read them automatically through the same key hierarchy as local development. Settings you define in the portal under Configuration become keys such as `Logging__LogLevel__Default` or `MySection__ApiKey` without changing `Program.cs`.

- Connection strings defined in App Service appear as environment variables prefixed with `CUSTOMCONNSTR_`, `SQLAZURECONNSTR_`, or `MYSQLCONNSTR_`, and the Connection Strings configuration provider maps them into `IConfiguration` under the `ConnectionStrings` section.
- Double underscores in setting names represent nested JSON sections, mirroring the convention used with `appsettings.json`, so `Database__Server` overrides the `Database:Server` value from file-based config.
- Environment-specific files such as `appsettings.Production.json` still load when `ASPNETCORE_ENVIRONMENT` is `Production`, but App Service settings override file values because environment variables rank later in the configuration chain.
- Storing secrets only in App Service configuration (or referencing Key Vault — see Q20) avoids committing production credentials to source control while keeping the same `builder.Configuration["Key"]` access pattern used locally.

---

## Q14. What is the role of the `ASPNETCORE_ENVIRONMENT` environment variable on App Service, and how does it relate to `appsettings.{Environment}.json`?

What is the role of the `ASPNETCORE_ENVIRONMENT` environment variable on App Service, and how does it relate to `appsettings.{Environment}.json`?

**Answer:** `ASPNETCORE_ENVIRONMENT` tells ASP.NET Core which hosting environment name is active, which selects environment-specific configuration files and controls branching in `Program.cs` such as the development exception page versus the production exception handler. On App Service you set this variable in Configuration → Application settings, typically to `Production` for live sites and `Staging` for non-production slots.

- When the value is `Production`, the host loads `appsettings.Production.json` after `appsettings.json`, applying overrides like reduced log verbosity — matching the pattern in the sample project's `Program.cs` where non-development environments enable HSTS and the `/Error` handler.
- Development-only middleware (`UseDeveloperExceptionPage`) remains disabled in Azure unless you explicitly set the environment to `Development`, which you should avoid on internet-facing production slots because it exposes stack traces.
- Each deployment slot can define its own `ASPNETCORE_ENVIRONMENT`, often marked sticky so staging stays `Staging` after swap while production remains `Production`.
- Other related variables include `ASPNETCORE_URLS` (usually set by the platform) and `DOTNET_ENVIRONMENT`, which some libraries read as a fallback when `ASPNETCORE_ENVIRONMENT` is unset.

---

## Q15. What is Kudu, and what capabilities does the Advanced Tools (Kudu) site provide for troubleshooting App Service deployments?

What is Kudu, and what capabilities does the Advanced Tools (Kudu) site provide for troubleshooting App Service deployments?

**Answer:** Kudu is the deployment engine and diagnostic console behind App Service, exposed at `https://{app-name}.scm.azurewebsites.net` as Advanced Tools in the Azure portal. It provides file browsing, log streaming, process inspection, and REST APIs (including ZIP deploy) that reveal what actually landed on the worker after your pipeline ran.

- The Debug Console lets you browse `site/wwwroot` to confirm that `dotnet publish` output — your `.dll`, dependencies, and `web.config` — is present and matches the expected build version.
- Deployment Center and the `/api/deployments` endpoint show history, status, and logs for each push, which is the first place to look when a GitHub Actions release reports success but the site still serves an old build.
- Log streaming aggregates platform logs, Docker container logs (on Linux), and application stdout/stderr so you can watch Kestrel startup errors in real time without downloading files.
- Kudu also supports running arbitrary commands in the worker sandbox (subject to tier limits), enabling quick checks such as `dotnet --info` or verifying environment variables visible to your ASP.NET Core process.

---

## Q16. What is the Always On setting in App Service, and why does it matter for ASP.NET Core background work and cold-start behavior?

What is the Always On setting in App Service, and why does it matter for ASP.NET Core background work and cold-start behavior?

**Answer:** Always On sends periodic requests to your application's root URL so the worker process stays loaded even when external traffic is idle, which prevents idle shutdown on supported tiers. Without it, App Service may unload your site after a period of inactivity, causing a cold start that reinitializes Kestrel, dependency injection, and cached data on the next request.

- Cold starts on ASP.NET Core manifest as several seconds of delay while the runtime JIT-compiles methods and your middleware pipeline builds, which hurts user experience for intermittently used apps.
- Background services implemented with `IHostedService` or `BackgroundService` stop when the process unloads, so scheduled or queue-processing work requires Always On (or a tier that supports it) to run reliably.
- Always On is unavailable on Free and Shared tiers; production ASP.NET Core deployments on Basic and above typically enable it unless the app runs in a continuously busy traffic pattern that naturally keeps the process warm.
- Always On complements but does not replace proper autoscale: it keeps one instance warm per worker, while scale-out still adds additional instances under load.

---

## Q17. What is the difference between manual scaling, vertical scaling (changing plan tier), and autoscale rules on an App Service Plan?

What is the difference between manual scaling, vertical scaling (changing plan tier), and autoscale rules on an App Service Plan?

**Answer:** Manual scaling changes the instance count on a fixed plan tier, vertical scaling moves the plan to a higher or lower pricing tier with different CPU and memory per instance, and autoscale dynamically adjusts instance count based on metrics or schedules. All three operate on the App Service Plan, so every web app on that plan experiences the scaling action together.

- Manual scale suits predictable traffic: you set instance count to three and pay for three workers until you change it, which is simple but requires human intervention for spikes.
- Vertical scale (for example Standard S1 to P1v3) gives each instance more horsepower without adding instances, which helps single-instance workloads that cannot scale out due to session state or licensing limits.
- Autoscale rules on Standard and above add or remove instances when average CPU, memory, HTTP queue length, or custom Application Insights metrics cross thresholds you define, with cooldown periods to prevent flapping.
- Choosing between them is a capacity-planning decision: autoscale handles diurnal traffic for stateless ASP.NET Core APIs, while vertical scale addresses per-request memory pressure that extra instances cannot fix.

---

## Q18. What is Application Request Routing (ARR) affinity (sticky sessions), and when should you enable or disable it for a stateless ASP.NET Core API?

What is Application Request Routing (ARR) affinity (sticky sessions), and when should you enable or disable it for a stateless ASP.NET Core API?

**Answer:** ARR affinity (often called sticky sessions) instructs the Azure front-end load balancer to route subsequent requests from the same client to the same worker instance using a cookie. It exists for applications that store session state in local memory rather than a shared store, but most modern ASP.NET Core APIs should leave it disabled.

- When affinity is on, scale-in events can drop in-memory session data for users pinned to removed instances, causing unexpected logouts or lost shopping carts unless state is externalized.
- Stateless ASP.NET Core Web APIs that authenticate with JSON Web Tokens (JWTs) and persist data in SQL or Redis perform correctly with affinity off, allowing the load balancer to distribute requests evenly across all instances.
- Enable affinity only when you rely on ASP.NET Core session state (`AddSession`) stored in memory on a specific instance and have not yet migrated to a distributed cache such as Redis.
- Affinity is configured under Configuration → General settings and affects routing before requests reach Kestrel; it does not replace proper session or cache architecture for multi-instance deployments.

---

## Q19. How do you bind a custom domain and configure Transport Layer Security (TLS)/SSL certificates for an App Service web app?

How do you bind a custom domain and configure Transport Layer Security (TLS)/SSL certificates for an App Service web app?

**Answer:** Custom domain binding maps your DNS name to the App Service site's default hostname through a CNAME record (for subdomains) or an A record plus a verification TXT record (for apex domains), after which you attach a TLS certificate so HTTPS terminates at the platform front end. Azure provides free managed certificates for apex and subdomain names on supported tiers, or you can upload your own certificate from a public CA or Key Vault.

- After adding the custom domain in the portal, App Service displays the required DNS targets; propagation delays are the most common reason a binding shows "Not verified" immediately after configuration.
- App Service Managed Certificate automates issuance and renewal for domains you own when DNS is hosted in Azure or correctly delegated, removing manual expiry tracking for many production sites.
- Uploading a `.pfx` or binding a Key Vault certificate suits wildcard domains or corporate policies that require a specific certificate authority.
- Enforcing HTTPS-only and setting a minimum TLS version (1.2 or 1.3) under Configuration → General settings redirects HTTP clients and blocks outdated protocols, which is standard hardening for public ASP.NET Core endpoints.

---

## Q20. What is Azure App Service managed identity, and how can an ASP.NET Core app use it to access Azure Key Vault or other Azure resources without storing secrets in configuration?

What is Azure App Service managed identity, and how can an ASP.NET Core app use it to access Azure Key Vault or other Azure resources without storing secrets in configuration?

**Answer:** Managed identity gives your App Service a Microsoft Entra ID (formerly Azure Active Directory) service principal that Azure automatically provisions and rotates, so your application authenticates to other Azure services without client secrets in configuration. In ASP.NET Core you enable the identity on the web app, grant it access policies on Key Vault or role assignments on other resources, and use `DefaultAzureCredential` or the Azure SDK to obtain tokens at runtime.

- System-assigned identity is tied to the web app's lifecycle and is deleted if the app is removed; user-assigned identity is a standalone resource you can attach to multiple apps.
- In Key Vault, you grant the identity `Get` permission on secrets rather than embedding connection strings in App Service settings — the app reads secrets by name through the Key Vault configuration provider or SDK.
- `DefaultAzureCredential` tries managed identity in Azure and falls back to local developer credentials during development, keeping the same code path across environments.
- This pattern eliminates secret sprawl in ZIP deploy artifacts and portal settings while aligning with zero-trust expectations that credentials should not appear in source control or build logs.

---

## Q21. What is Virtual Network (VNet) integration for App Service, and when do you need it to reach private databases or internal APIs?

What is Virtual Network (VNet) integration for App Service, and when do you need it to reach private databases or internal APIs?

**Answer:** VNet integration routes outbound traffic from your App Service worker through a subnet in your Azure Virtual Network, allowing the app to reach private IP addresses such as Azure SQL with private endpoint, internal REST APIs, or on-premises systems via ExpressRoute or VPN. Without it, App Service uses public internet egress and cannot resolve or connect to resources that block public access.

- Regional VNet integration (Standard tier and above) attaches your app to a delegated subnet and is the common choice for ASP.NET Core apps that query a privately linked database while the app itself remains publicly reachable.
- Inbound private access requires separate features such as private endpoints on the App Service resource or an Application Gateway front end, because outbound VNet integration alone does not hide the app's public URL.
- Network security groups and route tables on the integration subnet control which destinations the app may reach, letting security teams enforce least-privilege egress from the web tier.
- You need VNet integration when compliance or architecture mandates that database traffic never traverses the public internet, even if the web application serves external users over HTTPS.

---

## Q22. How does App Service health check work, and how does it interact with load balancing and instance replacement during scale operations?

How does App Service health check work, and how does it interact with load balancing and instance replacement during scale operations?

**Answer:** App Service health check periodically requests a path you configure (such as `/health`) on each worker instance and removes instances that fail repeatedly from the load balancer rotation. Healthy instances continue receiving traffic while unhealthy ones are restarted or replaced, which makes accurate health endpoints essential for ASP.NET Core apps that scale out.

- You enable health check under Monitoring → Health check by specifying a relative URL that returns HTTP 200 when Kestrel, the database, and critical dependencies are available — often implemented with ASP.NET Core health checks middleware (`MapHealthChecks`).
- Instances failing the probe are marked unhealthy and excluded from Application Request Routing distribution, reducing cascading 502 Bad Gateway responses during partial outages.
- During scale-in, unhealthy instances are preferentially removed first, so a misconfigured health path that always returns 404 can cause the platform to drain every new instance incorrectly.
- Health check complements but differs from Application Insights availability tests: platform health check governs instance routing inside App Service, while external probes validate end-to-end user-visible uptime.

---

## Q23. What logging and diagnostics options are available on App Service for an ASP.NET Core application (platform logs, application logs, Application Insights)?

What logging and diagnostics options are available on App Service for an ASP.NET Core application (platform logs, application logs, Application Insights)?

**Answer:** App Service captures platform-level web server logs, deployment logs, and application stdout/stderr, and you can forward structured ASP.NET Core logging to Application Insights for queryable telemetry. Enabling the right combination lets you correlate Kestrel output, HTTP failures, and custom log scopes from your `ILogger` calls in one investigation.

- Application Logging (Filesystem or Blob) captures stdout from `Console` and debug providers when ASP.NET Core writes through the default logging pipeline configured in `appsettings.json`.
- Detailed Error Messages and Failed Request Tracing (Windows) provide low-level HTTP diagnostics beyond what ASP.NET Core's exception middleware records, useful for platform-level failures outside your code.
- Application Insights, enabled via the portal or `AddApplicationInsightsTelemetry()`, ingests traces, requests, dependencies, and exceptions with correlation IDs across SQL and HTTP outbound calls.
- Log Stream and Kudu download give immediate access during incidents, while Blob storage retention supports longer forensic review without shipping logs off-platform manually.

---

## Q24. What is the purpose of a startup command or startup script on Linux App Service, and when is it required for .NET applications?

What is the purpose of a startup command or startup script on Linux App Service, and when is it required for .NET applications?

**Answer:** On Linux App Service, the startup command tells the container entry point which process to launch — typically `dotnet MyApp.dll` — when the platform cannot infer the correct assembly from the deployment layout alone. It is required when you publish a framework-dependent deployment without the default Oryx auto-detection path or when you need custom arguments such as `--urls` or environment setup before Kestrel starts.

- The command is configured under Configuration → General settings → Startup Command and runs in the context of `/home/site/wwwroot` where your publish output lives.
- Self-contained deployments that include the runtime may use `./MyApp` directly instead of the `dotnet` launcher, but the startup command must still reference the correct executable name.
- Startup scripts can chain operations such as running Entity Framework migrations before `dotnet MyApp.dll`, though running migrations at startup is a design choice with trade-offs for multi-instance races.
- Windows App Service generally does not need a manual startup command because the ASP.NET Core Module in `web.config` points IIS to the right `.dll`; Linux lacks that module, making explicit startup configuration more common.

---

## Q25. How do you troubleshoot a failed deployment or a web app that returns HTTP 502/503 errors on App Service using Kudu and the Log Stream?

How do you troubleshoot a failed deployment or a web app that returns HTTP 502/503 errors on App Service using Kudu and the Log Stream?

**Answer:** HTTP 502 and 503 on App Service usually mean Kestrel failed to start, crashed on startup, or is not listening on the port the platform expects, and Kudu plus Log Stream reveal the underlying exception or deployment error. Start from Deployment Center history to confirm the latest publish succeeded, then stream application logs while restarting the site to capture startup stack traces.

- A 502 Bad Gateway often indicates the worker process exited immediately — common causes include missing `.dll`, wrong `DOTNET` version on the plan, or an unhandled exception in `Program.cs` during service registration.
- A 503 Service Unavailable can appear during swap warm-up, plan restart, or when all instances fail health check simultaneously, so check whether the issue correlates with a recent deployment or scale event.
- In Kudu, open `LogFiles/Application` and `LogFiles/eventlog.xml` (Windows) or Docker logs (Linux) to find the exact runtime error that the portal summary omits.
- Verify application settings such as connection strings and Key Vault references: a secret resolution failure at startup prevents the host from building the dependency injection container and produces the same gateway errors as a code bug.

---

## Q26. What is the relationship between an App Service web app, its resource group, and Azure Resource Manager (ARM) templates in infrastructure-as-code deployments?

What is the relationship between an App Service web app, its resource group, and Azure Resource Manager (ARM) templates in infrastructure-as-code deployments?

**Answer:** An App Service web app is an Azure Resource Manager resource that always belongs to a resource group and depends on an App Service Plan resource in the same subscription; ARM templates (or Bicep modules) declare all three so environments can be reproduced consistently. Visual Studio publish profiles in this module generate ARM snippets — such as `profile.arm.json` — that parameterize resource names, regions, and plan SKUs for one-click provisioning.

- The resource group is the lifecycle boundary for deployment operations: deleting the group removes the web app, plan, and related monitoring resources together.
- ARM templates express dependencies explicitly — the web app resource references `serverFarmId` so Azure creates the plan before binding the app, preventing orphan resources during automated provisioning.
- Infrastructure-as-code pipelines deploy the template to dev, test, and production subscriptions with different parameter files while keeping the ASP.NET Core application deployment (ZIP deploy) as a separate release step.
- Treating the web app and plan as code artifacts documents SKU choices and region decisions in source control rather than only in the portal, which supports audit and disaster recovery scenarios.

---

## Q27. What are common security practices for hardening an ASP.NET Core App Service deployment (HTTPS-only, minimum TLS version, IP restrictions, authentication)?

What are common security practices for hardening an ASP.NET Core App Service deployment (HTTPS-only, minimum TLS version, IP restrictions, authentication)?

**Answer:** Hardening App Service combines platform configuration with ASP.NET Core security middleware so that transport, network access, and identity are enforced before requests reach your controllers or Razor Pages. The goal is to reduce attack surface at the edge and ensure the application fails closed when credentials or tokens are invalid.

- Enable HTTPS-only and TLS 1.2 minimum so clients cannot downgrade to unencrypted HTTP, complementing `UseHsts()` and HTTPS redirection already present in typical `Program.cs` production pipelines.
- Access restrictions (IP allow lists or Azure Front Door integration) limit who can reach management endpoints and the public site, which is valuable for internal admin APIs or pre-release slots.
- Enable Microsoft Entra authentication (Easy Auth) at the App Service layer for quick protection of staging sites, or rely on ASP.NET Core JWT and cookie middleware for fine-grained authorization inside the app.
- Store secrets in Key Vault with managed identity, disable remote debugging in production, keep the runtime on supported .NET versions, and scan deployment artifacts in CI to prevent credential leaks from reaching `wwwroot`.

---

## Q28. When would you choose Azure App Service over Azure Container Apps, Azure Functions, or AKS for hosting an ASP.NET Core web application?

When would you choose Azure App Service over Azure Container Apps, Azure Functions, or AKS for hosting an ASP.NET Core web application?

**Answer:** Choose App Service when you want the simplest managed hosting for a long-running ASP.NET Core web app or API with minimal container or orchestration overhead and built-in deployment slots, custom domains, and autoscale. Alternative services fit specialized shapes: Functions for short event-driven handlers, Container Apps for microservices with Kubernetes-style scale without managing a cluster, and AKS for full control over networking, service mesh, and mixed workloads.

| Option | Best fit |
|--------|----------|
| App Service | Traditional web apps, REST APIs, steady HTTP traffic, slot-based releases |
| Azure Functions | Short-lived triggers, webhooks, background jobs without always-on HTTP |
| Container Apps | Containerized services, event-driven scale to zero, Dapr integration |
| AKS | Multi-team platforms, custom ingress, GPU, complex service topology |

- App Service remains the default for teams publishing framework-dependent `dotnet publish` output from Visual Studio or GitHub Actions who do not want to maintain Dockerfiles unless container portability is a requirement.
- Move toward Container Apps or AKS when you need sidecar patterns, custom Linux images with native dependencies not supported on App Service, or unified orchestration across many language runtimes.
- Azure Functions with ASP.NET Core isolated worker suits auxiliary HTTP endpoints but is a poor primary host for full MVC or Razor Pages applications that expect continuous Kestrel availability and deployment slot workflows.

---
