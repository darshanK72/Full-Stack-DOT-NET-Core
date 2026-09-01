# Azure DevOps Pipelines — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [What is Azure DevOps Pipelines, and how does it fit into the Azure DevOps platfo…](#q1)
2. [What is the difference between classic (UI) pipelines and YAML pipelines in Azur…](#q2)
3. [What are the main top-level sections of an Azure DevOps YAML pipeline file?](#q3)
4. [What is a pipeline trigger, and what types of triggers exist for YAML pipelines?](#q4)
5. [What is an agent pool, and what is the difference between Microsoft-hosted and s…](#q5)
6. [What is the difference between a stage, a job, and a step in a YAML pipeline?](#q6)
7. [What tasks are commonly used to restore, build, test, and publish a .NET applica…](#q7)
8. [How do you specify the .NET SDK version in a YAML pipeline?](#q8)
9. [How do you run unit tests and publish test results and code coverage for a .NET …](#q9)
10. [What is the difference between `dotnet build` and `dotnet publish` in a CI/CD pi…](#q10)
11. [How do you cache NuGet packages in an Azure DevOps pipeline to speed up .NET bui…](#q11)
12. [How do you build and publish multiple projects or a solution with different conf…](#q12)
13. [What is the difference between pipeline variables, variable groups, and runtime …](#q13)
14. [How do Azure DevOps variable groups work, and how do you link them to Azure Key …](#q14)
15. [What is the difference between compile-time expressions (`${{ }}`) and runtime v…](#q15)
16. [How do you pass secrets securely to a .NET build or deployment step without expo…](#q16)
17. [What are pipeline parameters, and when should you use them instead of variables?](#q17)
18. [What is a multi-stage YAML pipeline, and why use stages instead of a single job?](#q18)
19. [What are Azure DevOps environments, and how do they relate to deployment stages?](#q19)
20. [How do approval gates and checks work in Azure DevOps deployment pipelines?](#q20)
21. [How do you conditionally run a stage or job based on branch, build result, or a …](#q21)
22. [What is the difference between deployment jobs and regular jobs in Azure DevOps …](#q22)
23. [What is a service connection in Azure DevOps, and why is it required for Azure d…](#q23)
24. [How do you create and use an Azure Resource Manager (ARM) service connection for…](#q24)
25. [What is the Azure App Service deploy task, and what deployment methods does it s…](#q25)
26. [How do you deploy an ASP.NET Core Web API or web app to Azure App Service using …](#q26)
27. [What is the difference between deploying to a staging slot and swapping to produ…](#q27)
28. [What are common causes and troubleshooting steps when an Azure DevOps pipeline f…](#q28)

---

## Q1. What is Azure DevOps Pipelines, and how does it fit into the Azure DevOps platform?

What is Azure DevOps Pipelines, and how does it fit into the Azure DevOps platform?

**Answer:** Azure DevOps Pipelines is the continuous integration and continuous delivery (CI/CD) service inside the Azure DevOps platform, responsible for automatically building, testing, and deploying application code whenever developers push changes or on a schedule. It sits alongside Azure Repos (source control), Azure Boards (work tracking), Azure Test Plans, and Azure Artifacts (package feeds) as one of the five core services in an Azure DevOps organization or project.

- A pipeline definition describes what should happen on each run: which agent executes the work, which tasks restore dependencies, compile code, run tests, and publish artifacts, and which deployment stages push the output to environments such as staging or production.
- Pipelines integrate directly with Git repositories hosted in Azure Repos or external providers such as GitHub, so a commit or pull request can automatically trigger a build that validates the change before merge.
- For .NET teams, Pipelines is the standard place to run `dotnet restore`, `dotnet build`, `dotnet test`, and `dotnet publish`, then deploy the resulting output to Azure App Service, Azure Functions, containers, or on-premises servers.
- Pipeline history, logs, and test results are retained in the Azure DevOps project, giving teams a single audit trail from commit to deployed release.

---

## Q2. What is the difference between classic (UI) pipelines and YAML pipelines in Azure DevOps?

What is the difference between classic (UI) pipelines and YAML pipelines in Azure DevOps?

**Answer:** Classic pipelines are defined through a visual designer in the Azure DevOps web portal, where you add tasks by clicking through forms, while YAML pipelines are defined as code in a `azure-pipelines.yml` file stored in the repository alongside the application source. YAML is the recommended approach for new projects because the pipeline version travels with the code, supports pull-request review, and is easier to replicate across branches and environments.

| | Classic (UI) pipelines | YAML pipelines |
|---|---|---|
| Definition | Created in the portal designer | Stored in a `.yml` file in source control |
| Versioning | Lives in Azure DevOps, not in Git | Committed and branched with application code |
| Reuse | Limited templating | Templates, extends, and reusable stage/job definitions |
| Microsoft guidance | Maintenance mode for new features | Primary investment area for CI/CD capabilities |

Both execution models run on the same agent infrastructure and support the same task catalog; the practical difference is whether pipeline changes require editing a YAML file in Git or clicking through the portal, and whether those changes can be reviewed in a pull request like application code.

---

## Q3. What are the main top-level sections of an Azure DevOps YAML pipeline file?

What are the main top-level sections of an Azure DevOps YAML pipeline file?

**Answer:** An Azure DevOps YAML pipeline file is structured as a hierarchy of named sections that describe when the pipeline runs, on which machine it runs, and what work it performs. The most common top-level keys are `trigger`, `pr`, `pool`, `variables`, `stages`, and optionally `parameters` and `resources`.

- **`trigger`** defines which branch commits automatically start a pipeline (for example `main` or `release/*`); omitting it or setting `trigger: none` means the pipeline runs only manually or from another pipeline.
- **`pr`** controls whether pull requests against specified branches invoke the pipeline for validation builds without deploying.
- **`pool`** names the agent pool (Microsoft-hosted or self-hosted) and optionally the agent image, such as `windows-latest` or `ubuntu-latest`.
- **`variables`** declares pipeline-scoped name-value pairs; these can also reference variable groups defined in the project library.
- **`stages`** groups related jobs into logical phases — typical .NET pipelines use a Build stage followed by Deploy stages for each environment.
- Each stage contains one or more **`jobs`**, each job runs on one agent and contains **`steps`** that invoke tasks (built-in or custom) or scripts.

A minimal .NET build pipeline might define a single stage with one job and steps for SDK install, restore, build, test, and publish artifact — multi-stage pipelines add separate deploy stages that consume the published artifact.

---

## Q4. What is a pipeline trigger, and what types of triggers exist for YAML pipelines?

What is a pipeline trigger, and what types of triggers exist for YAML pipelines?

**Answer:** A pipeline trigger is the rule that tells Azure DevOps when to start a pipeline run automatically, rather than requiring a manual queue. In YAML pipelines the primary triggers are branch push triggers (`trigger`), pull request triggers (`pr`), scheduled triggers (`schedules`), pipeline resource triggers, and manual or API-initiated runs.

- **Branch push trigger (`trigger`)** starts the pipeline when commits land on listed branches or paths; you can include or exclude branches and filter by changed file paths so documentation-only commits skip a full build.
- **Pull request trigger (`pr`)** runs validation builds when a pull request is opened or updated against target branches, giving reviewers build and test status before merge.
- **Scheduled trigger (`schedules`)** runs the pipeline on a cron expression, commonly used for nightly integration tests or dependency vulnerability scans.
- **Resource triggers** start a pipeline when another pipeline completes or when a container image in a registry is updated, enabling chained or event-driven release flows.
- Triggers defined in YAML take precedence over triggers configured in the pipeline settings UI for YAML pipelines, so teams should treat the YAML file as the source of truth for when builds run.

---

## Q5. What is an agent pool, and what is the difference between Microsoft-hosted and self-hosted agents?

What is an agent pool, and what is the difference between Microsoft-hosted and self-hosted agents?

**Answer:** An agent pool is a collection of build machines (agents) that Azure DevOps uses to execute pipeline jobs; each job is assigned to exactly one agent for the duration of that job. Microsoft provides hosted pools with pre-installed tools, or organizations can register their own self-hosted agents on servers or virtual machines they control.

- **Microsoft-hosted agents** are ephemeral virtual machines maintained by Microsoft; a fresh VM is provisioned for each job, pre-loaded with common runtimes including multiple .NET SDK versions, Node.js, Docker, and Azure CLI. You choose the image with `vmImage: ubuntu-latest`, `windows-latest`, or `macOS-latest`.
- **Self-hosted agents** run on infrastructure you manage — on-premises servers, Azure VMs, or containers — and persist between jobs, which makes them suitable for builds that need access to private networks, specialized hardware, or licensed software not available on hosted agents.
- Hosted agents have a monthly free tier for public projects and per-minute billing for private projects; self-hosted agents have no per-minute charge but you pay for the underlying compute and maintenance.
- For standard ASP.NET Core CI/CD, Microsoft-hosted `ubuntu-latest` or `windows-latest` agents are usually sufficient because the `UseDotNet@2` and `DotNetCoreCLI@2` tasks install the required SDK version on demand.

---

## Q6. What is the difference between a stage, a job, and a step in a YAML pipeline?

What is the difference between a stage, a job, and a step in a YAML pipeline?

**Answer:** A stage is the highest-level grouping of work in a multi-stage pipeline, representing a major phase such as Build, Deploy to Staging, or Deploy to Production. A job is a unit of work that runs on a single agent within a stage, and a step is an individual command or task executed sequentially inside a job.

- **Stages** run sequentially by default (unless you define parallel stages), and each stage can depend on the successful completion of prior stages; this is how build-then-deploy pipelines enforce that deployment never runs if tests fail.
- **Jobs** within the same stage can run in parallel on separate agents — for example, one job building the API and another building the Blazor front end — but each job is isolated and must explicitly publish and download artifacts to share output.
- **Steps** inside a job share the same workspace directory on the agent, so a `dotnet build` step can immediately follow a `dotnet restore` step without artifact transfer between them.
- Deployment stages typically use a **deployment job** (a specialized job type tied to an environment) rather than a regular job, which enables deployment history tracking, approval gates, and slot-based deployment strategies.

Understanding this hierarchy helps you place build logic in the Build stage's job steps, publish one artifact at the end of that job, and consume it in downstream deploy stage deployment jobs.

---

## Chapter 2 — Building & Testing .NET Applications

---

## Q7. What tasks are commonly used to restore, build, test, and publish a .NET application in an Azure DevOps YAML pipeline?

What tasks are commonly used to restore, build, test, and publish a .NET application in an Azure DevOps YAML pipeline?

**Answer:** The standard approach for .NET projects in Azure DevOps is to use the built-in `UseDotNet@2` task to install the required SDK version, then chain `DotNetCoreCLI@2` tasks for restore, build, test, and publish operations against the solution or project file. A final `PublishPipelineArtifact@1` task saves the publish output so later deployment stages can download it.

- **`UseDotNet@2`** installs a specific .NET SDK or runtime version on the agent before any `dotnet` commands run, ensuring the pipeline uses the same SDK as the development team regardless of what is pre-installed on the hosted agent image.
- **`DotNetCoreCLI@2`** with command `restore` downloads NuGet packages; `build` compiles without producing a deployment folder; `test` runs test projects and can publish TRX results; `publish` produces a deployment-ready folder with all runtime dependencies.
- **`PublishTestResults@2`** ingests test result files (`.trx` from `dotnet test --logger trx`) and displays pass/fail counts in the pipeline summary and pull request status.
- **`PublishCodeCoverageResults@2`** uploads Cobertura or other coverage formats generated by `dotnet test --collect:"XPlat Code Coverage"` for visibility in the Azure DevOps Tests tab.
- **`PublishPipelineArtifact@1`** uploads the publish folder as a named pipeline artifact (for example `drop`) that deployment jobs in later stages download with `DownloadPipelineArtifact@2`.

---

## Q8. How do you specify the .NET SDK version in a YAML pipeline?

How do you specify the .NET SDK version in a YAML pipeline?

**Answer:** You pin the .NET SDK version by adding a `UseDotNet@2` task at the beginning of the job, passing the desired version in the `version` input (for example `8.0.x` or an exact version like `8.0.404`). Alternatively, many teams commit a `global.json` file in the repository root, and the `UseDotNet@2` task with `useGlobalJson: true` reads the SDK version from that file automatically.

- Pinning the SDK prevents silent breakage when Microsoft updates the hosted agent image with a newer default SDK that introduces breaking changes or different behavior.
- The `version` input accepts semver ranges: `8.0.x` installs the latest patch of .NET 8, while an exact version string guarantees bit-for-bit reproducibility across months of pipeline runs.
- A `global.json` file checked into source control keeps the pipeline, local developer machines, and documentation aligned on one SDK version without duplicating the version string in YAML.
- For multi-targeting projects that build against more than one Target Framework Moniker (TFM), a single SDK install is usually sufficient because one SDK bundle contains all supported target frameworks for that release band.

```yaml
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '8.0.x'
```

---

## Q9. How do you run unit tests and publish test results and code coverage for a .NET project in Azure DevOps?

How do you run unit tests and publish test results and code coverage for a .NET project in Azure DevOps?

**Answer:** You run tests with a `DotNetCoreCLI@2` task using command `test`, passing arguments that produce machine-readable result files, then upload those files with `PublishTestResults@2` and `PublishCodeCoverageResults@2` so Azure DevOps displays results in the pipeline run and pull request checks. Failing tests automatically fail the pipeline step unless you explicitly allow continuation.

- The test task should target test projects explicitly or use a solution filter so only test assemblies run, not the main application project.
- Add `--logger trx --results-directory $(Agent.TempDirectory)/TestResults` to produce Visual Studio Test Results (TRX) files that `PublishTestResults@2` consumes with `testResultsFormat: 'VSTest'`.
- Enable code coverage by adding `--collect:"XPlat Code Coverage"` to the test arguments; this generates Cobertura XML that you publish with `PublishCodeCoverageResults@2` and `codeCoverageTool: 'Cobertura'`.
- Test result and coverage publishing makes failures visible on pull requests before merge, and coverage trends over time help teams spot untested code paths in critical .NET services.

```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: '--configuration Release --logger trx --collect:"XPlat Code Coverage"'

- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'

- task: PublishCodeCoverageResults@2
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
```

---

## Q10. What is the difference between `dotnet build` and `dotnet publish` in a CI/CD pipeline context?

What is the difference between `dotnet build` and `dotnet publish` in a CI/CD pipeline context?

**Answer:** `dotnet build` compiles the project and produces intermediate output in the `bin` folder, which is sufficient to verify that code compiles and to run unit tests, but it does not assemble everything needed to run the application on another machine. `dotnet publish` goes further by collecting the compiled assemblies, dependencies, configuration files, and static content into a single folder or package ready for deployment to Azure App Service, a container image, or a file share.

- In CI, teams typically run `dotnet build` (or rely on `dotnet test`, which builds first) during the validation phase to get fast feedback on compile errors.
- `dotnet publish` is run once before artifact upload, often with `--configuration Release` and an explicit output path such as `$(Build.ArtifactStagingDirectory)`, so the deployment stage receives a self-contained folder.
- Publish also applies deployment-specific transforms: for ASP.NET Core it copies `web.config` for IIS hosting, includes `appsettings.json`, and can apply ReadyToRun or trimming settings defined in the project file.
- Deploying the output of `dotnet build` instead of `dotnet publish` is a common mistake — the App Service or container will miss runtime dependencies and static files, leading to startup failures in production.

---

## Q11. How do you cache NuGet packages in an Azure DevOps pipeline to speed up .NET builds?

How do you cache NuGet packages in an Azure DevOps pipeline to speed up .NET builds?

**Answer:** You cache NuGet packages using the `Cache@2` task, which saves the NuGet global packages folder (or a project-local cache path) to the pipeline cache at the end of a job and restores it at the start of the next run if the cache key has not changed. This avoids re-downloading identical packages on every build, which can save several minutes on large solutions.

- The cache key should incorporate inputs that invalidate the cache when dependencies change — commonly a hash of all `*.csproj` and `packages.lock.json` files, plus the agent OS name.
- Set the cache path to `$(UserProfile)\.nuget\packages` on Windows agents or `~/.nuget/packages` on Linux agents, which is where `dotnet restore` stores downloaded packages by default.
- Run the `Cache@2` task before `dotnet restore`; when cache restoration succeeds, restore completes quickly because packages are already on disk.
- Lock files (`packages.lock.json` enabled via `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>`) make cache keys more precise because the lock file hash changes only when dependency versions actually change.

```yaml
- task: Cache@2
  inputs:
    key: 'nuget | "$(Agent.OS)" | **/packages.lock.json, **/*.csproj'
    path: $(NUGET_PACKAGES)
  displayName: Cache NuGet packages
```

---

## Q12. How do you build and publish multiple projects or a solution with different configurations in one pipeline?

How do you build and publish multiple projects or a solution with different configurations in one pipeline?

**Answer:** You can build an entire solution with a single `DotNetCoreCLI@2` task pointing at the `.sln` file, or define parallel jobs — each targeting one project — when projects need different configurations, target frameworks, or independent deployment paths. Artifacts from each job are published with distinct names so downstream stages deploy the correct output.

- A monolithic approach runs `dotnet build MySolution.sln --configuration Release` once, then `dotnet publish` on each deployable project with separate task instances specifying different `projects` inputs and output folders.
- A parallel approach defines one job per microservice or per deployable unit, each with its own restore-build-test-publish sequence, which reduces total wall-clock time when sufficient agents are available.
- Use MSBuild properties in the publish arguments to control output: `--output $(Build.ArtifactStagingDirectory)/api` for the Web API and a separate publish step for a Blazor WebAssembly client with a different output subfolder.
- Publish each output as a separately named pipeline artifact (`api-drop`, `web-drop`) so deployment stages download only the artifact they need rather than one combined folder with ambiguous contents.

---

## Chapter 3 — Variables, Variable Groups & Secrets

---

## Q13. What is the difference between pipeline variables, variable groups, and runtime parameters in Azure DevOps?

What is the difference between pipeline variables, variable groups, and runtime parameters in Azure DevOps?

**Answer:** Pipeline variables are name-value pairs defined inline in the YAML file or in the pipeline settings UI, scoped to a single pipeline definition. Variable groups are shared collections of variables stored in the project Library and linked to one or more pipelines, often backed by Azure Key Vault for secrets. Runtime parameters are typed inputs declared at the top of a YAML file that the user must supply or confirm each time the pipeline is run manually.

| | Pipeline variables | Variable groups | Runtime parameters |
|---|---|---|---|
| Scope | One pipeline (unless in a template) | Shared across many pipelines | One pipeline run at queue time |
| Definition | YAML `variables:` section or UI | Project Library → Variable groups | YAML `parameters:` section |
| Secret support | Yes (secret variables) | Yes, including Key Vault linkage | No — not for secrets |
| When resolved | Runtime (with compile-time exceptions) | Runtime, fetched at job start | At pipeline queue time |

Use inline variables for values specific to one pipeline, variable groups for shared configuration such as connection string names or API URLs across environments, and parameters when operators should choose options (such as which environment to deploy) at run time.

---

## Q14. How do Azure DevOps variable groups work, and how do you link them to Azure Key Vault?

How do Azure DevOps variable groups work, and how do you link them to Azure Key Vault?

**Answer:** Variable groups are named collections of variables stored at the project level in Azure DevOps, referenced from YAML with the `- group: MyGroupName` syntax under the `variables` key. When linked to Azure Key Vault, Azure DevOps fetches the selected secrets from the vault at pipeline run time and exposes them as pipeline variables without storing the secret values in Azure DevOps itself.

- Create a variable group in Pipelines → Library, add plain variables for non-secret values, or enable "Link secrets from an Azure key vault as variables" to pull secrets by name from a vault.
- Linking Key Vault requires an Azure Resource Manager service connection with Get permission on secrets in that vault; Azure DevOps reads secrets using that connection identity each time a job referencing the group starts.
- Secret variables in groups are masked in logs — pipeline output replaces their values with `***` — but you must still avoid passing secrets to commands that echo them or write them to artifacts.
- Reference the group in YAML so all jobs in the pipeline inherit the variables:

```yaml
variables:
  - group: ProductionSettings
  - name: buildConfiguration
    value: Release
```

See also the Key Vault variable group answer in the Secrets module (Q20) for rotation and approval-gate details; the mechanism is the same across modules.

---

## Q15. What is the difference between compile-time expressions (`${{ }}`) and runtime variables (`$( )`) in YAML pipelines?

What is the difference between compile-time expressions (`${{ }}`) and runtime variables (`$( )`) in YAML pipelines?

**Answer:** Compile-time expressions (`${{ }}`) are evaluated when Azure DevOps parses the YAML template before the pipeline run starts, so they can conditionally include or exclude entire sections of the pipeline definition. Runtime variables (`$( )`) are expanded when a step actually executes on the agent, which means they can reference values produced during the run such as output variables from prior tasks.

- Use `${{ }}` for template parameters, conditional insertion of stages or jobs (`${{ if eq(parameters.deployProd, true) }}`), and expressions that must be resolved before the agent is allocated.
- Use `$( )` for ordinary pipeline and job variables, predefined system variables like `$(Build.SourcesDirectory)`, and task output variables set with `##vso[task.setvariable]` syntax.
- A common pitfall is trying to use a runtime variable inside `${{ }}` — it will not work because the runtime value does not exist at compile time; conversely, template parameters cannot be referenced with `$( )` after the template is expanded.
- Macro syntax `$(var)` is processed just before a step runs; macro syntax cannot appear in keys that must be known at compile time, such as job display names defined via templates, where `${{ }}` is required instead.

---

## Q16. How do you pass secrets securely to a .NET build or deployment step without exposing them in pipeline logs?

How do you pass secrets securely to a .NET build or deployment step without exposing them in pipeline logs?

**Answer:** Store secrets as secret variables in a variable group or as Key Vault-linked variables, reference them in task inputs or script environment blocks, and never print them to the console or commit them to the repository. Azure DevOps automatically masks secret variable values in log output, but masking only works when the exact secret string would appear in logs — commands that transform or encode secrets can still leak information if written carelessly.

- Mark variables as secret when creating them in the Library or pipeline settings; secret variables are not exported to forked pull request builds from public repositories by default, which prevents untrusted code from reading production credentials.
- Pass secrets to tasks through the `env:` block on a script step rather than inline in a command string, which reduces the chance of the shell echoing the value: `env: CONNECTION_STRING: $(DbConnectionString)`.
- For Azure App Service deployment, prefer service connections and managed identities over embedding subscription keys in variables; the Azure App Service deploy task uses the service connection identity to authenticate.
- Avoid writing secrets into `appsettings.json` or `.env` files as part of the build artifact; instead, configure App Service application settings in the deployment task or through Azure CLI using secret variables at deploy time.

---

## Q17. What are pipeline parameters, and when should you use them instead of variables?

What are pipeline parameters, and when should you use them instead of variables?

**Answer:** Pipeline parameters are typed, declared inputs at the top of a YAML file under the `parameters` key; they appear as form fields when a user manually queues the pipeline and can enforce allowed values, defaults, and data types. Unlike variables, parameters are fixed for the entire run once the pipeline is queued and are intended for operator choices rather than secret configuration.

- Use parameters when the person triggering the pipeline should choose among known options — for example, which environment to deploy (`dev`, `staging`, `prod`) or whether to skip integration tests — because parameters support dropdown enums and validation at queue time.
- Use variables (or variable groups) for configuration that should be automatic and not require human input each run, such as artifact names, build configuration (`Release`), or connection string secret values.
- Parameters are expanded at compile time via `${{ parameters.environment }}`, which makes them suitable for conditional stage inclusion; variables are better for values that change based on task output during the run.
- Do not store secrets in parameters — they are visible in the queue dialog and run metadata; secrets belong in secret variables or Key Vault-linked groups.

```yaml
parameters:
  - name: environment
    type: string
    default: staging
    values: [dev, staging, prod]
```

---

## Chapter 4 — Multi-Stage Pipelines & Environments

---

## Q18. What is a multi-stage YAML pipeline, and why use stages instead of a single job?

What is a multi-stage YAML pipeline, and why use stages instead of a single job?

**Answer:** A multi-stage YAML pipeline divides work into named sequential phases — typically Build, Deploy to Staging, and Deploy to Production — each containing one or more jobs. Stages enforce ordering and dependencies so deployment only proceeds after build and test succeed, and they provide clear separation between CI validation work and CD release work.

- A single-job pipeline mixes build and deploy on one agent, which couples validation to deployment credentials and prevents deploying the exact same artifact to multiple environments with different approval rules.
- Multi-stage pipelines publish a build artifact once in the Build stage, then each deploy stage downloads that immutable artifact, guaranteeing that what was tested is exactly what reaches production.
- Stages support `dependsOn` and `condition` expressions, so you can skip production deployment on feature branches while still running the full build and test stage on every commit.
- Stage boundaries appear as distinct nodes in the Azure DevOps pipeline visualization, making it easy for operators to see which environment failed and to rerun only a failed deploy stage without rebuilding.

---

## Q19. What are Azure DevOps environments, and how do they relate to deployment stages?

What are Azure DevOps environments, and how do they relate to deployment stages?

**Answer:** An Azure DevOps environment is a named resource in a project — such as `development`, `staging`, or `production` — that tracks deployment history, holds environment-specific variables, and attaches approval and security checks to deployment jobs that target it. Deployment stages reference an environment with the `environment:` key on a deployment job, linking pipeline runs to a living record of what is deployed where.

- Each environment shows a timeline of deployments with commit, pipeline, and status information, giving teams a single pane for release auditing across App Service slots, Kubernetes clusters, or virtual machines.
- Environment-scoped variables and variable groups let you store different App Service names or connection strings per environment without hardcoding them in YAML — the same pipeline template resolves different values depending on which environment the deployment job targets.
- Environments can map to Azure resources such as a specific App Service instance or resource group, enabling traceability from a pipeline run to the Azure portal resource.
- Using environments is a prerequisite for approval gates, branch control checks, and exclusive deployment locks that prevent two releases from targeting production simultaneously.

---

## Q20. How do approval gates and checks work in Azure DevOps deployment pipelines?

How do approval gates and checks work in Azure DevOps deployment pipelines?

**Answer:** Approval gates and checks are pre-deployment conditions configured on an Azure DevOps environment that must be satisfied before a deployment job targeting that environment is allowed to run. An approval gate sends a notification to designated reviewers who must manually approve or reject the deployment, while automated checks validate conditions such as required template compliance, Azure Policy, or successful upstream pipeline completion.

- Configure approvals under Pipelines → Environments → select environment → Approvals and checks → add an Approval check with named approvers and optional timeout instructions.
- When a deployment job reaches an environment with an approval check, the pipeline pauses and waits; approved deployments proceed, rejected deployments fail the stage without modifying the target resource.
- Multiple checks can be chained — for example, require a successful security scan pipeline check and a manual approval before production — and all must pass before the deploy task executes.
- Approvals integrate with branch policies and audit logs, so regulated industries can demonstrate that production releases had human oversight separate from the developer who authored the commit.

---

## Q21. How do you conditionally run a stage or job based on branch, build result, or a custom expression?

How do you conditionally run a stage or job based on branch, build result, or a custom expression?

**Answer:** You add a `condition` property to a stage or job using Azure DevOps expression syntax, which evaluates at runtime to determine whether that stage or job should execute. Common patterns include deploying to production only from `main`, skipping deploy stages when tests fail, or running expensive integration tests only on scheduled builds.

- Branch conditions use built-in variables: `condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))` runs the stage only on successful builds from `main`.
- The default job condition inside a stage is `succeeded()`, meaning prior stages must succeed; override with `condition: always()` on a notification job that runs even when deployment fails.
- Combine functions: `and()`, `or()`, `eq()`, `ne()`, `startsWith()` — for example, deploy to staging from any release branch: `startsWith(variables['Build.SourceBranch'], 'refs/heads/release/')`.
- Stage-level conditions apply to all jobs in that stage; job-level conditions give finer control when one stage contains both optional and mandatory jobs.

```yaml
- stage: DeployProd
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
    - deployment: Deploy
      environment: production
      strategy:
        runOnce:
          deploy:
            steps:
              - download: current
                artifact: drop
```

---

## Q22. What is the difference between deployment jobs and regular jobs in Azure DevOps YAML?

What is the difference between deployment jobs and regular jobs in Azure DevOps YAML?

**Answer:** A deployment job is a specialized job type designed for deploying to environments, with built-in support for deployment history, environment protection checks, and deployment strategies such as runOnce, rolling, and canary. A regular job is a general-purpose unit of work on an agent with no environment binding or deployment lifecycle tracking.

- Deployment jobs use the `deployment` keyword instead of `job`, require an `environment` value, and wrap steps inside a `strategy` block — most commonly `runOnce` with `deploy:` steps for App Service zip deploy.
- Regular jobs are appropriate for build, test, and artifact publish work; deployment jobs are appropriate for any step that mutates a target environment and should appear in that environment's deployment history.
- Deployment jobs can define `runOnce`, `rolling`, or `canary` strategies under `strategy`, which controls how many targets receive the update and whether health checks gate progression — App Service slot swap fits naturally in a `runOnce` deploy phase followed by a separate swap step.
- Both job types run on agents from the specified pool, but only deployment jobs honor environment approval gates and record output in the Environments hub.

---

## Chapter 5 — Service Connections & Azure App Service Deployment

---

## Q23. What is a service connection in Azure DevOps, and why is it required for Azure deployments?

What is a service connection in Azure DevOps, and why is it required for Azure deployments?

**Answer:** A service connection is a stored authentication profile in an Azure DevOps project that allows pipeline tasks to access external services — most commonly Azure subscriptions, Docker registries, GitHub, or Kubernetes clusters — without embedding credentials in YAML. For Azure App Service deployment, an Azure Resource Manager (ARM) service connection grants the pipeline permission to publish packages, configure app settings, and manage deployment slots in your subscription.

- Service connections store a service principal or managed identity mapping plus subscription and tenant identifiers; pipeline tasks reference the connection by name in the `azureSubscription` input.
- Administrators can scope connections to specific resource groups and require pipeline authorization before a new pipeline can use the connection, which limits blast radius if a pipeline definition is compromised.
- Without a service connection, deploy tasks such as `AzureWebApp@1` cannot authenticate to Azure and the deployment step fails before any files are transferred.
- Rotating service principal secrets is done in the service connection settings or by switching to workload identity federation (OpenID Connect), which avoids long-lived client secrets entirely.

---

## Q24. How do you create and use an Azure Resource Manager (ARM) service connection for deploying to Azure?

How do you create and use an Azure Resource Manager (ARM) service connection for deploying to Azure?

**Answer:** You create an ARM service connection in Project settings → Service connections → New service connection → Azure Resource Manager, choosing service principal, managed identity, or workload identity federation authentication, then selecting the subscription and optionally scoping to a resource group. In YAML, reference the connection name in the `azureSubscription` input of Azure deployment tasks.

- The automatic service principal option creates an app registration in Microsoft Entra ID with Contributor (or narrower custom) role on the subscription or resource group; Azure DevOps stores the client secret or certificate and refreshes it as configured.
- Workload identity federation (recommended for new setups) links the service connection to a federated credential so the pipeline obtains short-lived tokens via OIDC without storing a permanent secret in Azure DevOps.
- After creation, grant pipeline access: either authorize each pipeline on first run when prompted, or configure the connection to allow all pipelines in the project (less restrictive, common in small teams).
- Use the connection in deploy tasks:

```yaml
- task: AzureWebApp@1
  inputs:
    azureSubscription: 'MyAzureConnection'
    appType: 'webAppLinux'
    appName: 'my-api-app'
    package: '$(Pipeline.Workspace)/drop/**/*.zip'
```

---

## Q25. What is the Azure App Service deploy task, and what deployment methods does it support?

What is the Azure App Service deploy task, and what deployment methods does it support?

**Answer:** The `AzureWebApp@1` task (Azure App Service deploy) is the primary built-in Azure DevOps task for deploying web applications to Azure App Service on Windows, Linux, or container-based plans. It packages your build output and pushes it to the App Service using one of several deployment technologies depending on the `appType` and package format you provide.

- **Zip Deploy** is the default for Linux App Service and modern Windows App Service: the task uploads a `.zip` of the publish folder, and the Kudu deployment engine extracts it into `wwwroot`; this is the standard path for ASP.NET Core apps published with `dotnet publish`.
- **Web Deploy (MSDeploy)** targets Windows App Service when `WebDeploy` package type is selected, preserving IIS site configuration and enabling incremental updates; it requires the publish profile or Web Deploy endpoint.
- **Container deploy** (`appType: webAppContainer`) pushes a Docker image reference to an App Service configured for containers, often paired with a prior `Docker@2` build-and-push step to Azure Container Registry.
- The task can also set App Service application settings and connection strings from pipeline variables during deployment, reducing the need for separate Azure CLI configuration steps.

---

## Q26. How do you deploy an ASP.NET Core Web API or web app to Azure App Service using a YAML pipeline?

How do you deploy an ASP.NET Core Web API or web app to Azure App Service using a YAML pipeline?

**Answer:** A typical ASP.NET Core App Service deployment pipeline builds and tests in a Build stage, publishes the project to a folder, archives it as a zip artifact, then in a Deploy stage downloads that artifact and runs `AzureWebApp@1` against the target App Service using an ARM service connection. The publish output must be the result of `dotnet publish`, not `dotnet build`, and the App Service runtime stack must match the application's target framework.

1. **Install SDK** — `UseDotNet@2` pins the SDK version matching the project.
2. **Restore, build, test** — `DotNetCoreCLI@2` tasks validate the solution; failing tests block deployment.
3. **Publish** — `DotNetCoreCLI@2` with command `publish`, `--configuration Release`, and output to `$(Build.ArtifactStagingDirectory)`.
4. **Archive** — `ArchiveFiles@2` zips the publish folder for Zip Deploy compatibility.
5. **Publish artifact** — `PublishPipelineArtifact@1` uploads the zip as a named artifact.
6. **Deploy stage** — deployment job targeting an environment downloads the artifact and runs `AzureWebApp@1` with `package` pointing to the zip and `appName` set to the App Service resource name.

Ensure the App Service has the correct .NET runtime stack configured in the Azure portal (for example .NET 8) and that application settings for production secrets are set via the task's `appSettings` input or Azure Key Vault references rather than baked into `appsettings.json` in the artifact.

---

## Q27. What is the difference between deploying to a staging slot and swapping to production in Azure App Service via a pipeline?

What is the difference between deploying to a staging slot and swapping to production in Azure App Service via a pipeline?

**Answer:** A deployment slot is a separate live instance of your App Service app with its own hostname; deploying to the staging slot updates the pre-production copy without touching production traffic. Slot swap is an atomic operation that exchanges the staging and production slot contents so the newly deployed bits receive production traffic and the previous production version moves to staging for fast rollback.

- **Deploy to slot** — the `AzureWebApp@1` task accepts a `deployToSlotOrASE: true` and `resourceGroupName` / `slotName` inputs to push the zip to the staging slot; production users see no change during this step.
- **Warm-up and validation** — after staging deployment, smoke tests or health checks can run against the staging URL (`https://myapp-staging.azurewebsites.net`) before swap.
- **Swap slots** — the `AzureAppServiceManage@0` task with action `Swap Slots` exchanges staging and production; swap is designed to minimize downtime because DNS and routing flip quickly without redeploying files.
- If validation fails after staging deploy, delete or redeploy the staging slot without ever affecting production; if issues appear after swap, swap again to roll back because the previous production bits now reside in staging.

This pattern is the App Service implementation of blue-green deployment and is the recommended release path for production ASP.NET Core APIs managed through Azure DevOps.

---

## Q28. What are common causes and troubleshooting steps when an Azure DevOps pipeline fails during .NET build or App Service deployment?

What are common causes and troubleshooting steps when an Azure DevOps pipeline fails during .NET build or App Service deployment?

**Answer:** Build failures usually trace to SDK version mismatches, missing NuGet feeds, failing tests, or incorrect project paths, while deployment failures usually trace to service connection permissions, wrong App Service names, invalid zip packages, or runtime stack mismatches on the target app. Pipeline logs in Azure DevOps show the exact failing task; enabling detailed logs helps diagnose authentication and MSDeploy issues.

**Build stage common causes:**
- SDK not installed or wrong version — confirm `UseDotNet@2` version matches `TargetFramework` in the csproj.
- NuGet restore fails on private feeds — add a `NuGetAuthenticate@1` task or supply credentials via service connection.
- Tests fail or timeout — inspect TRX results in the Tests tab; flaky integration tests are a frequent CI blocker.
- Project path glob matches nothing — verify `projects:` pattern against repository folder layout.

**Deploy stage common causes:**
- Service connection unauthorized — reauthorize the connection for the pipeline or verify the service principal still has Contributor on the resource group.
- `package` path empty — confirm artifact download step ran and the zip path glob matches the archived file name.
- App Service runtime mismatch — a `net8.0` published app fails on an App Service stack set to .NET 6; align stack settings in Azure portal with publish TFM.
- Startup failure after successful deploy — check App Service log stream and Application Insights; missing connection strings and incorrect `ASPNETCORE_ENVIRONMENT` are frequent post-deploy issues.

Enable `system.debug: true` as a pipeline variable for verbose task diagnostics, and use the "Rerun failed jobs" feature to retry deploy stages without a full rebuild once the root cause is fixed.

---
