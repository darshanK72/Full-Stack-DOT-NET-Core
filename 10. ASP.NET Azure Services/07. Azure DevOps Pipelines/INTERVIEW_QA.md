# Azure DevOps Pipelines — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure DevOps Pipelines, and how does it fit into the Azure DevOps platform?](#q1-what-is-azure-devops-pipelines-and-how-does-it-fit-into-the-azure-devops-platform)
2. [Q2. What is the difference between classic (UI) pipelines and YAML pipelines in Azure DevOps?](#q2-what-is-the-difference-between-classic-ui-pipelines-and-yaml-pipelines-in-azure-devops)
3. [Q3. What are the main top-level sections of an Azure DevOps YAML pipeline file?](#q3-what-are-the-main-top-level-sections-of-an-azure-devops-yaml-pipeline-file)
4. [Q4. What is a pipeline trigger, and what types of triggers exist for YAML pipelines?](#q4-what-is-a-pipeline-trigger-and-what-types-of-triggers-exist-for-yaml-pipelines)
5. [Q5. What is an agent pool, and what is the difference between Microsoft-hosted and self-hosted agents?](#q5-what-is-an-agent-pool-and-what-is-the-difference-between-microsoft-hosted-and-self-hosted-agents)
6. [Q6. What is the difference between a stage, a job, and a step in a YAML pipeline?](#q6-what-is-the-difference-between-a-stage-a-job-and-a-step-in-a-yaml-pipeline)
7. [Q7. What tasks are commonly used to restore, build, test, and publish a .NET application in an Azure DevOps YAML pipeline?](#q7-what-tasks-are-commonly-used-to-restore-build-test-and-publish-a-net-application-in-an-azure-devops-yaml-pipeline)
8. [Q8. How do you specify the .NET SDK version in a YAML pipeline?](#q8-how-do-you-specify-the-net-sdk-version-in-a-yaml-pipeline)
9. [Q9. How do you run unit tests and publish test results and code coverage for a .NET project in Azure DevOps?](#q9-how-do-you-run-unit-tests-and-publish-test-results-and-code-coverage-for-a-net-project-in-azure-devops)
10. [Q10. What is the difference between `dotnet build` and `dotnet publish` in a CI/CD pipeline context?](#q10-what-is-the-difference-between-dotnet-build-and-dotnet-publish-in-a-cicd-pipeline-context)
11. [Q11. How do you cache NuGet packages in an Azure DevOps pipeline to speed up .NET builds?](#q11-how-do-you-cache-nuget-packages-in-an-azure-devops-pipeline-to-speed-up-net-builds)
12. [Q12. How do you build and publish multiple projects or a solution with different configurations in one pipeline?](#q12-how-do-you-build-and-publish-multiple-projects-or-a-solution-with-different-configurations-in-one-pipeline)
13. [Q13. What is the difference between pipeline variables, variable groups, and runtime parameters in Azure DevOps?](#q13-what-is-the-difference-between-pipeline-variables-variable-groups-and-runtime-parameters-in-azure-devops)
14. [Q14. How do Azure DevOps variable groups work, and how do you link them to Azure Key Vault?](#q14-how-do-azure-devops-variable-groups-work-and-how-do-you-link-them-to-azure-key-vault)
15. [Q15. What is the difference between compile-time expressions (`${{ }}`) and runtime variables (`$( )`) in YAML pipelines?](#q15-what-is-the-difference-between-compile-time-expressions-and-runtime-variables-in-yaml-pipelines)
16. [Q16. How do you pass secrets securely to a .NET build or deployment step without exposing them in pipeline logs?](#q16-how-do-you-pass-secrets-securely-to-a-net-build-or-deployment-step-without-exposing-them-in-pipeline-logs)
17. [Q17. What are pipeline parameters, and when should you use them instead of variables?](#q17-what-are-pipeline-parameters-and-when-should-you-use-them-instead-of-variables)
18. [Q18. What is a multi-stage YAML pipeline, and why use stages instead of a single job?](#q18-what-is-a-multi-stage-yaml-pipeline-and-why-use-stages-instead-of-a-single-job)
19. [Q19. What are Azure DevOps environments, and how do they relate to deployment stages?](#q19-what-are-azure-devops-environments-and-how-do-they-relate-to-deployment-stages)
20. [Q20. How do approval gates and checks work in Azure DevOps deployment pipelines?](#q20-how-do-approval-gates-and-checks-work-in-azure-devops-deployment-pipelines)
21. [Q21. How do you conditionally run a stage or job based on branch, build result, or a custom expression?](#q21-how-do-you-conditionally-run-a-stage-or-job-based-on-branch-build-result-or-a-custom-expression)
22. [Q22. What is the difference between deployment jobs and regular jobs in Azure DevOps YAML?](#q22-what-is-the-difference-between-deployment-jobs-and-regular-jobs-in-azure-devops-yaml)
23. [Q23. What is a service connection in Azure DevOps, and why is it required for Azure deployments?](#q23-what-is-a-service-connection-in-azure-devops-and-why-is-it-required-for-azure-deployments)
24. [Q24. How do you create and use an Azure Resource Manager (ARM) service connection for deploying to Azure?](#q24-how-do-you-create-and-use-an-azure-resource-manager-arm-service-connection-for-deploying-to-azure)
25. [Q25. What is the Azure App Service deploy task, and what deployment methods does it support?](#q25-what-is-the-azure-app-service-deploy-task-and-what-deployment-methods-does-it-support)
26. [Q26. How do you deploy an ASP.NET Core Web API or web app to Azure App Service using a YAML pipeline?](#q26-how-do-you-deploy-an-aspnet-core-web-api-or-web-app-to-azure-app-service-using-a-yaml-pipeline)
27. [Q27. What is the difference between deploying to a staging slot and swapping to production in Azure App Service via a pipeline?](#q27-what-is-the-difference-between-deploying-to-a-staging-slot-and-swapping-to-production-in-azure-app-service-via-a-pipeline)
28. [Q28. What are common causes and troubleshooting steps when an Azure DevOps pipeline fails during .NET build or App Service deployment?](#q28-what-are-common-causes-and-troubleshooting-steps-when-an-azure-devops-pipeline-fails-during-net-build-or-app-service-deployment)

---

## Q1. What is Azure DevOps Pipelines, and how does it fit into the Azure DevOps platform?

**Concepts**
- CI/CD service within the five-service Azure DevOps platform
- Pipeline definition hierarchy — agent, tasks, stages, and environments
- Git integration with Azure Repos and GitHub for commit/PR triggers
- .NET CLI commands (restore, build, test, publish) as standard pipeline steps
- Unified audit trail from commit through deployed release

**Answer**

Azure DevOps Pipelines is the CI/CD service within Azure DevOps, sitting alongside Azure Repos, Boards, Test Plans, and Artifacts as one of five platform services. It automatically builds, tests, and deploys code when commits land or on a schedule, using a pipeline definition that specifies which agent runs the work, which tasks to execute (restore, compile, test, publish), and which stages push output to environments like staging or production. Because pipelines integrate directly with Azure Repos and GitHub, a commit or pull request automatically triggers a validation build before merge, and all pipeline history, logs, and test results are retained in the project, giving teams a single audit trail from source change to deployed release.

---

## Q2. What is the difference between classic (UI) pipelines and YAML pipelines in Azure DevOps?

**Concepts**
- Classic pipelines — portal designer, not version-controlled
- YAML pipelines — definition stored in source alongside application code
- Pipeline-as-code — branch, review, and merge pipeline changes like application code
- Template reuse — extends, reusable stage/job definitions in YAML
- Microsoft investment direction — YAML is the primary focus, classic in maintenance mode

**Answer**

Classic pipelines are defined through a visual portal designer and stored in Azure DevOps, while YAML pipelines are checked into the repository as `azure-pipelines.yml` and versioned alongside application code. The critical difference is that YAML pipeline changes can be reviewed in a pull request, branched per feature, and rolled back with a git revert — none of which is practical with a portal-defined pipeline. YAML also supports templates, `extends`, and reusable stage or job definitions that classic pipelines cannot match. Both execution models run on the same agent infrastructure and task catalog; Microsoft is actively investing new features only in the YAML path, so classic pipelines are effectively in maintenance mode for new projects.

---

## Q3. What are the main top-level sections of an Azure DevOps YAML pipeline file?

**Concepts**
- trigger — branch push automation rule
- pr — pull request validation trigger
- pool — agent pool and VM image selection
- variables — pipeline-scoped name-value configuration
- stages/jobs/steps — execution hierarchy
- parameters and resources — optional queue-time inputs and external dependencies

**Answer**

An Azure DevOps YAML pipeline is built from a small set of top-level keys that answer three questions: when does it run, where does it run, and what does it do. `trigger` names the branches whose commits start the pipeline automatically; `pr` activates validation builds on pull requests. `pool` names the agent pool and optionally the VM image (`ubuntu-latest`, `windows-latest`). `variables` declares name-value pairs the pipeline uses throughout. `stages` is the outermost execution container — each stage holds one or more `jobs`, and each job holds `steps` that call tasks or scripts. `parameters` and `resources` are optional additions: parameters expose queue-time operator choices, and resources declare dependencies on other pipelines, containers, or repositories. A minimal .NET build pipeline uses a single stage with one job and steps for SDK install, restore, build, test, and artifact publish.

---

## Q4. What is a pipeline trigger, and what types of triggers exist for YAML pipelines?

**Concepts**
- Branch push trigger (`trigger`) — commit-based automation
- Pull request trigger (`pr`) — validation builds before merge
- Scheduled trigger (`schedules`) — cron-based recurring runs
- Resource trigger — pipeline chaining on upstream completion or registry event
- YAML trigger precedence over portal UI settings

**Answer**

A pipeline trigger is the rule that tells Azure DevOps when to start a run automatically rather than requiring manual queuing. The branch push trigger (`trigger`) fires when commits land on listed branches or paths, and path filters let documentation-only commits skip a build. The pull request trigger (`pr`) runs validation builds when a PR is opened or updated against target branches, giving reviewers build and test status before merge. The scheduled trigger (`schedules`) accepts a cron expression and is commonly used for nightly integration tests or vulnerability scans. Resource triggers start a pipeline when another pipeline completes or when a container image in a registry is updated, enabling event-driven release chains. Trigger definitions in the YAML file take precedence over any settings configured in the portal UI, so the file is always the source of truth for when a pipeline runs.

---

## Q5. What is an agent pool, and what is the difference between Microsoft-hosted and self-hosted agents?

**Concepts**
- Agent pool — collection of build machines for job execution
- Microsoft-hosted agents — ephemeral VMs provisioned fresh per job
- vmImage selection — ubuntu-latest, windows-latest, macOS-latest
- Self-hosted agents — persistent, private-network-capable infrastructure
- Billing model — per-minute (hosted) vs infrastructure cost (self-hosted)

**Answer**

An agent pool is a collection of build machines that Azure DevOps allocates to pipeline jobs; each job runs exclusively on one agent for its duration. Microsoft-hosted agents are ephemeral VMs provisioned fresh for each job, pre-loaded with common runtimes including multiple .NET SDK versions, Docker, and Azure CLI — you select the image with `vmImage: ubuntu-latest` or `windows-latest`. Self-hosted agents run on infrastructure you manage — on-premises servers, Azure VMs, or containers — and persist between jobs, which makes them appropriate for builds that need access to private networks, specialized hardware, or software not available on hosted images. Hosted agents have a monthly free tier for public projects and per-minute billing for private projects; self-hosted agents carry no per-minute charge but you pay for the underlying compute and bear the maintenance burden. For standard ASP.NET Core CI/CD, `ubuntu-latest` hosted agents are usually sufficient because `UseDotNet@2` installs the required SDK version on demand.

---

## Q6. What is the difference between a stage, a job, and a step in a YAML pipeline?

**Concepts**
- Stage — major pipeline phase (Build, Deploy Staging, Deploy Production)
- Job — single-agent unit of work within a stage
- Step — individual task or script executed sequentially inside a job
- Parallel jobs — independent agents within the same stage
- Deployment job — environment-bound specialized job type

**Answer**

A stage is the highest-level grouping, representing a major phase such as Build or Deploy to Production. Stages run sequentially by default and enforce ordering through `dependsOn`, so a deploy stage never starts if the build stage fails. A job is a unit of work that runs on a single agent within a stage; jobs within the same stage can run in parallel on separate agents, but they are isolated and must explicitly publish and download artifacts to share output between them. A step is an individual command or task executed sequentially inside a job — steps share the agent's workspace, so a `dotnet build` step can immediately follow `dotnet restore` without artifact transfer. Deployment stages typically use a deployment job (the `deployment` keyword) rather than a regular job, which binds the job to an environment and enables deployment history, approval gates, and deployment strategies like runOnce or rolling.

---

## Chapter 2 — Building & Testing .NET Applications

---

## Q7. What tasks are commonly used to restore, build, test, and publish a .NET application in an Azure DevOps YAML pipeline?

**Concepts**
- UseDotNet@2 — SDK version pinning on the agent
- DotNetCoreCLI@2 — wrapper for restore, build, test, and publish commands
- PublishTestResults@2 — TRX ingestion into pipeline summary
- PublishCodeCoverageResults@2 — Cobertura XML upload
- PublishPipelineArtifact@1 — artifact handoff between stages

**Answer**

The standard approach is to lead with `UseDotNet@2` to install the required SDK version, then chain `DotNetCoreCLI@2` tasks for each `dotnet` operation. The restore task downloads NuGet packages; the build task compiles without producing a deployment folder; the test task runs test projects and emits result files; the publish task assembles a deployment-ready folder. After testing, `PublishTestResults@2` ingests the TRX files generated by `--logger trx` and surfaces pass/fail counts in the pipeline summary and pull request checks. `PublishCodeCoverageResults@2` uploads Cobertura XML generated by `--collect:"XPlat Code Coverage"` for coverage visibility in the Tests tab. Finally, `PublishPipelineArtifact@1` uploads the publish output as a named artifact — for example `drop` — that deployment jobs in later stages download with `DownloadPipelineArtifact@2`.

---

## Q8. How do you specify the .NET SDK version in a YAML pipeline?

**Concepts**
- UseDotNet@2 — SDK install task
- Semver range vs exact version pinning (8.0.x vs 8.0.404)
- global.json — repo-anchored SDK version declaration
- useGlobalJson: true — reads version from global.json automatically
- Single SDK install covers all supported TFMs in that release band

**Answer**

I pin the SDK by placing a `UseDotNet@2` task at the start of the job with the desired version in the `version` input. A semver range like `8.0.x` installs the latest patch of .NET 8, which is convenient for staying current, while an exact string like `8.0.404` guarantees bit-for-bit reproducibility across months of pipeline runs and prevents silent breakage when Microsoft updates the hosted agent image. Alternatively, committing a `global.json` to the repository root and setting `useGlobalJson: true` on the task makes the pipeline read the SDK version from that file automatically, which keeps the pipeline, local developer machines, and documentation aligned on a single version without duplicating the string in YAML.

```yaml
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '8.0.x'
```

---

## Q9. How do you run unit tests and publish test results and code coverage for a .NET project in Azure DevOps?

**Concepts**
- DotNetCoreCLI@2 test command targeting test projects
- --logger trx — machine-readable Visual Studio Test Results format
- --collect:"XPlat Code Coverage" — Cobertura XML generation
- PublishTestResults@2 — pipeline summary and PR status integration
- PublishCodeCoverageResults@2 — coverage trend tracking

**Answer**

I run tests with a `DotNetCoreCLI@2` task using command `test`, targeting test project globs explicitly so only test assemblies run. I add `--logger trx --results-directory $(Agent.TempDirectory)/TestResults` to produce TRX files, then `PublishTestResults@2` with `testResultsFormat: 'VSTest'` ingests them and shows pass/fail counts in the pipeline run and pull request checks — failing tests automatically fail the pipeline step. Code coverage comes from adding `--collect:"XPlat Code Coverage"` to the test arguments, which generates Cobertura XML that `PublishCodeCoverageResults@2` with `codeCoverageTool: 'Cobertura'` uploads for visibility in the Tests tab and long-term trend tracking.

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

**Concepts**
- dotnet build — compile-time validation, intermediate bin output
- dotnet publish — deployment-ready folder with all runtime dependencies
- Release configuration and explicit output path in CI
- Deployment-specific transforms — web.config, appsettings, ReadyToRun
- Deploying build output instead of publish output — common startup failure cause

**Answer**

`dotnet build` compiles the project into the `bin` folder, which is sufficient to verify the code compiles and to run unit tests, but it does not assemble everything needed to run the application on another machine. `dotnet publish` collects compiled assemblies, runtime dependencies, configuration files, and static content into a single folder ready for deployment, so the target environment receives a self-contained package. In CI I run `dotnet build` (or rely on `dotnet test`, which builds implicitly) for fast compile feedback, then run `dotnet publish --configuration Release --output $(Build.ArtifactStagingDirectory)` once to produce the deploy artifact. Publish also applies deployment-specific transforms: for ASP.NET Core it copies `web.config` for IIS hosting, includes `appsettings.json`, and applies ReadyToRun or trimming settings from the project file. Deploying the output of `dotnet build` instead of `dotnet publish` is a common mistake — the App Service or container will be missing runtime dependencies and static files, causing startup failures in production.

---

## Q11. How do you cache NuGet packages in an Azure DevOps pipeline to speed up .NET builds?

**Concepts**
- Cache@2 task — pipeline cache save and restore between runs
- Cache key — hash of csproj and lock files plus agent OS
- NuGet global packages folder path (OS-dependent)
- packages.lock.json — precise cache invalidation on dependency changes
- Cache task placement before dotnet restore

**Answer**

I use the `Cache@2` task placed before `dotnet restore`, configured with a cache key that incorporates a hash of all `*.csproj` and `packages.lock.json` files plus the agent OS name. When the key matches a prior run, the task restores the NuGet global packages folder — `$(UserProfile)\.nuget\packages` on Windows or `~/.nuget/packages` on Linux — so `dotnet restore` finds packages already on disk and completes in seconds instead of minutes. When the key changes (because a dependency version changed), the cache misses and restore downloads fresh packages, then saves them at job end. Lock files (`packages.lock.json` enabled via `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>`) make the key more precise because the lock file hash changes only when dependency versions actually change, not just when project files are touched.

```yaml
- task: Cache@2
  inputs:
    key: 'nuget | "$(Agent.OS)" | **/packages.lock.json, **/*.csproj'
    path: $(NUGET_PACKAGES)
  displayName: Cache NuGet packages
```

---

## Q12. How do you build and publish multiple projects or a solution with different configurations in one pipeline?

**Concepts**
- Solution-level build vs per-project publish steps
- Parallel jobs per microservice or deployable unit
- MSBuild --output flag for separate publish paths
- Named pipeline artifacts per deployable unit
- Agent availability governing parallel job benefit

**Answer**

I build the entire solution with a single `DotNetCoreCLI@2` task pointing at the `.sln` file, then run separate `DotNetCoreCLI@2` publish tasks for each deployable project with distinct `projects` inputs and `--output` paths, such as `$(Build.ArtifactStagingDirectory)/api` and `$(Build.ArtifactStagingDirectory)/web`. Each publish output is then uploaded as a separately named pipeline artifact — `api-drop`, `web-drop` — so deployment stages can download only the artifact they need. When projects are truly independent microservices with different test suites and configurations, I prefer a parallel-job approach instead: one job per service with its own restore-build-test-publish sequence, which reduces wall-clock time when sufficient agents are available but requires each job to publish its own artifact since job workspaces are isolated.

---

## Chapter 3 — Variables, Variable Groups & Secrets

---

## Q13. What is the difference between pipeline variables, variable groups, and runtime parameters in Azure DevOps?

**Concepts**
- Pipeline variables — inline, single-pipeline scope
- Variable groups — shared collections across multiple pipelines
- Key Vault-linked variable groups for secrets
- Runtime parameters — typed, operator-supplied at queue time
- Compile-time (parameters) vs runtime (variables) resolution

**Answer**

Pipeline variables are name-value pairs defined in the YAML `variables:` section or the pipeline settings UI, scoped to one pipeline definition. Variable groups are shared collections stored in the project Library and linked to one or more pipelines with `- group: MyGroupName`; they can also be backed by Azure Key Vault so secrets are fetched at runtime without being stored in Azure DevOps. Runtime parameters are typed inputs declared under `parameters:` at the top of the YAML file — they appear as form fields when a user manually queues the pipeline and are resolved at queue time rather than runtime. I use inline variables for values specific to one pipeline, variable groups for shared configuration (connection string names, API URLs) across environments and pipelines, and parameters when an operator should explicitly choose among options — such as which environment to deploy — at run time.

---

## Q14. How do Azure DevOps variable groups work, and how do you link them to Azure Key Vault?

**Concepts**
- Variable groups in project Library — shared across pipelines
- - group: YAML syntax for referencing a group
- Key Vault secret linkage via ARM service connection with Get permission
- Secret variable masking in log output
- Group-level variable inheritance across all jobs in a pipeline

**Answer**

Variable groups are named collections of variables created in Pipelines → Library; referencing them with `- group: MyGroupName` under the `variables:` key makes all variables in the group available to every job in the pipeline. For secrets, I enable "Link secrets from an Azure key vault as variables" in the group settings, which requires an ARM service connection with Get permission on secrets in the target vault — Azure DevOps then fetches the selected secrets at job start using that connection's identity, so secret values are never stored in Azure DevOps itself. Secret variables from groups are automatically masked in logs, replacing their values with `***`, but masking only works for the exact string so I still avoid commands that transform or encode secrets before printing them.

```yaml
variables:
  - group: ProductionSettings
  - name: buildConfiguration
    value: Release
```

---

## Q15. What is the difference between compile-time expressions (`${{ }}`) and runtime variables (`$( )`) in YAML pipelines?

**Concepts**
- ${{ }} — compile-time template expression evaluated before run starts
- $( ) — runtime macro expanded when a step executes on the agent
- Template parameter scope vs runtime variable scope
- ##vso[task.setvariable] — task output variable set at runtime
- Pitfall of referencing runtime values inside compile-time expressions

**Answer**

Compile-time expressions (`${{ }}`) are evaluated when Azure DevOps parses the YAML before the pipeline run starts, which means they can conditionally include or exclude entire stages, jobs, or steps and can reference template parameters. Runtime variables (`$( )`) are expanded when a step actually executes on the agent, so they can reference values produced during the run such as output variables from prior tasks set with `##vso[task.setvariable]`. The key pitfall is mixing the two contexts: a runtime variable used inside `${{ }}` will not resolve because its value does not exist at parse time, and a template parameter cannot be referenced with `$( )` after the template is expanded. I use `${{ parameters.env }}` for conditional stage inclusion and `$(Build.SourceBranch)` for ordinary variable expansion inside step arguments.

---

## Q16. How do you pass secrets securely to a .NET build or deployment step without exposing them in pipeline logs?

**Concepts**
- Secret variable masking — automatic log redaction by Azure DevOps
- env: block — environment-variable injection without shell echoing
- Fork PR isolation — secret variables not exported to untrusted fork builds
- Service connections and managed identities over embedded credential variables
- Avoiding secrets in appsettings.json baked into the publish artifact

**Answer**

I store secrets in secret variables in a variable group or as Key Vault-linked variables, then pass them to steps through the `env:` block on script tasks rather than inline in a command string — this way the shell never echoes the value as part of the executed command. Azure DevOps automatically masks secret variable values in log output, but masking only catches the exact string, so commands that base64-encode or concatenate a secret can still leak information if written carelessly. Secret variables are not exported to fork PR builds from public repositories by default, which prevents untrusted code from reading production credentials. For Azure deployments I prefer service connections and managed identities over embedding subscription keys in variables; the App Service deploy task authenticates via the service connection identity rather than a stored key. I never write secrets into `appsettings.json` as part of the build artifact — instead I configure App Service application settings through the deploy task's `appSettings` input at deployment time.

---

## Q17. What are pipeline parameters, and when should you use them instead of variables?

**Concepts**
- parameters: key — typed, queue-time operator inputs
- Enum dropdown validation at queue time
- Compile-time expansion via ${{ parameters.x }}
- Parameters vs variables — human operator choice vs automatic configuration
- Parameters are not suitable for secrets

**Answer**

Pipeline parameters are typed, declared inputs under the `parameters:` key that appear as form fields when a user manually queues the pipeline; they support types like `string`, `boolean`, and `object`, and `values:` enumerates a dropdown of allowed choices enforced at queue time. They are resolved at compile time via `${{ parameters.environment }}`, which makes them the right tool for conditionally including or excluding entire stages based on operator intent. I use parameters when the person triggering the pipeline should make an explicit choice — which environment to deploy, whether to skip integration tests — because the type system and dropdown prevent invalid inputs. Variables (or variable groups) are better for configuration that should be automatic and not require human input, such as build configuration, artifact names, or secret connection strings. Parameters are visible in the queue dialog and run metadata, so secrets must never go in them.

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

**Concepts**
- Stage — named sequential pipeline phase
- dependsOn — stage ordering and dependency enforcement
- Immutable artifact handoff between Build and Deploy stages
- condition — branch or result-based stage skipping
- Azure DevOps pipeline visualization per stage

**Answer**

A multi-stage YAML pipeline divides work into named phases — typically Build, Deploy to Staging, and Deploy to Production — each containing one or more jobs, with stages enforcing ordering through `dependsOn`. The key reason to use stages over a single job is artifact immutability: the Build stage publishes one artifact, and every downstream deploy stage downloads and deploys that exact same artifact, guaranteeing that what was tested is precisely what reaches production. A single-job approach couples build and deploy on one agent, mixes credentials, and prevents deploying the same artifact to multiple environments with different approval rules. Stages also support `condition` expressions so production deployment is skipped on feature branches while the build and test stage still runs on every commit. Each stage renders as a distinct node in the Azure DevOps pipeline visualization, making it easy for operators to see which environment failed and rerun only the failed deploy stage without rebuilding.

---

## Q19. What are Azure DevOps environments, and how do they relate to deployment stages?

**Concepts**
- Environment — named deployment target with deployment history
- environment: key on deployment jobs
- Environment-scoped variables and variable groups
- Azure resource mapping for pipeline-to-portal traceability
- Prerequisite for approval gates, branch checks, and exclusive locks

**Answer**

An Azure DevOps environment is a named resource in the project — such as `development`, `staging`, or `production` — that tracks the history of every deployment targeting it: which commit, which pipeline run, and whether it succeeded. A deployment job references an environment with the `environment:` key, which links each pipeline run to that environment's record and surfaces the timeline in Pipelines → Environments. Environments can hold scoped variables and variable groups so the same YAML pipeline template resolves different App Service names or connection strings depending on which environment the job targets. They can also map to Azure resources like a specific App Service or Kubernetes namespace, enabling traceability from a pipeline run to the Azure portal resource. Using environments is a prerequisite for approval gates, branch protection checks, and exclusive deployment locks that prevent two releases from targeting production simultaneously.

---

## Q20. How do approval gates and checks work in Azure DevOps deployment pipelines?

**Concepts**
- Approval check — human gate configured on an environment
- Automated checks — policy compliance, upstream pipeline, Azure Policy
- Pipeline pause on pending approval
- Chained checks — all must pass before deployment executes
- Audit log integration for regulated environments

**Answer**

Approval gates and checks are pre-deployment conditions attached to a named Azure DevOps environment under Approvals and checks. An approval check sends a notification to designated reviewers who must manually approve or reject the deployment; when a deployment job reaches that environment, the pipeline pauses and waits for their decision — an approval allows the job to proceed, a rejection fails the stage without touching the target resource. Automated checks validate conditions such as required YAML template compliance, successful completion of a separate security scan pipeline, or passing Azure Policy rules, and they run without human intervention. Multiple checks can be chained so that, for example, both a security scan check and a manual approval must pass before production deploys. All approvals are recorded in the audit log, which gives regulated organizations evidence that production releases had human oversight separate from the developer who authored the commit.

---

## Q21. How do you conditionally run a stage or job based on branch, build result, or a custom expression?

**Concepts**
- condition: property on stage or job
- succeeded() — default implicit condition
- Build.SourceBranch — branch-based condition variable
- Expression functions — and(), or(), eq(), startsWith()
- Stage-level vs job-level condition scope

**Answer**

I add a `condition:` property to a stage or job using Azure DevOps expression syntax, which is evaluated at runtime to decide whether that unit of work should execute. The default condition for a stage is `succeeded()`, meaning all prior stages must have succeeded; I override this on notification jobs with `condition: always()` so they run even after a deployment failure. Branch conditions use `Build.SourceBranch`: `condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))` restricts production deployment to the main branch. I can combine functions — `and()`, `or()`, `startsWith()` — for example, deploying to staging from any release branch: `startsWith(variables['Build.SourceBranch'], 'refs/heads/release/')`. Stage-level conditions apply to all jobs in that stage; when I need finer control — one job optional, another mandatory — I set conditions at the job level instead.

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

**Concepts**
- deployment keyword — environment-bound specialized job type
- strategy block — runOnce, rolling, canary deployment lifecycle
- Deployment history tracking in Environments hub
- Approval gates — honored only by deployment jobs
- Regular jobs for build, test, and artifact publish work

**Answer**

A deployment job uses the `deployment` keyword instead of `job`, requires an `environment` value, and wraps its steps inside a `strategy` block — most commonly `runOnce` with `deploy:` steps. The environment binding is what makes deployment jobs different: they record each run in that environment's deployment history, honor approval gates and branch checks configured on the environment, and support deployment strategies like rolling or canary that control how many targets receive the update. Regular jobs are appropriate for build, test, and artifact publish work where no environment tracking is needed. Both types run on agents from the specified pool, but only deployment jobs appear in the Environments hub history, and only deployment jobs gate on environment-level approval checks — a regular job targeting the same tasks would bypass those checks entirely.

---

## Chapter 5 — Service Connections & Azure App Service Deployment

---

## Q23. What is a service connection in Azure DevOps, and why is it required for Azure deployments?

**Concepts**
- Service connection — stored authentication profile for external services
- ARM service connection for Azure subscriptions
- azureSubscription task input — connection reference in YAML
- Resource-group-scoped connections — limited blast radius
- Workload identity federation — OIDC, no long-lived client secrets

**Answer**

A service connection is a stored authentication profile in an Azure DevOps project that allows pipeline tasks to access external services — Azure subscriptions, Docker registries, GitHub, Kubernetes clusters — without embedding credentials in YAML. For Azure App Service deployment, an ARM service connection stores a service principal or workload identity federation mapping, and pipeline tasks reference it by name in the `azureSubscription` input. Without a service connection, `AzureWebApp@1` cannot authenticate to Azure and the deployment step fails before transferring any files. Administrators can scope connections to specific resource groups and require per-pipeline authorization, which limits blast radius if a pipeline definition is compromised. Rotating credentials is done in the service connection settings, or by switching to workload identity federation (OIDC), which issues short-lived tokens and eliminates long-lived client secrets entirely.

---

## Q24. How do you create and use an Azure Resource Manager (ARM) service connection for deploying to Azure?

**Concepts**
- Project settings → Service connections — creation entry point
- Service principal vs workload identity federation authentication options
- Federated credentials — OIDC short-lived tokens, no stored secrets
- Pipeline authorization — per-pipeline approval vs project-wide access
- azureSubscription input in deploy tasks

**Answer**

I create an ARM service connection in Project settings → Service connections → New service connection → Azure Resource Manager, choosing workload identity federation for new setups since it issues short-lived OIDC tokens rather than storing a permanent client secret in Azure DevOps. During creation I select the subscription and optionally scope to a specific resource group to limit the connection's permissions. After creation, pipeline authorization can be set to require explicit approval per pipeline (safer, common in enterprise) or to allow all pipelines in the project (convenient for small teams). In YAML I reference the connection by its display name in the `azureSubscription` input of any Azure task.

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

**Concepts**
- AzureWebApp@1 — primary App Service deploy task
- Zip Deploy — default for Linux App Service, Kudu extraction
- Web Deploy (MSDeploy) — Windows IIS incremental updates
- Container deploy — Docker image reference push to container-based plan
- App settings configuration during deployment via task inputs

**Answer**

`AzureWebApp@1` is the primary built-in task for deploying web applications to Azure App Service on Windows, Linux, or container-based plans. For Linux App Service and modern Windows App Service, it uses Zip Deploy by default: the task uploads a `.zip` of the `dotnet publish` output, and the Kudu deployment engine extracts it into `wwwroot` — this is the standard path for ASP.NET Core apps. For Windows App Service with IIS, selecting Web Deploy (`packageType: webDeploy`) allows incremental updates and preserves IIS site configuration. For container-based plans, `appType: webAppContainer` pushes a Docker image reference to an App Service configured for containers, typically paired with a prior `Docker@2` build-and-push step to Azure Container Registry. The task can also set App Service application settings from pipeline variables during deployment, which means production secrets are applied at deploy time rather than baked into the artifact.

---

## Q26. How do you deploy an ASP.NET Core Web API or web app to Azure App Service using a YAML pipeline?

**Concepts**
- dotnet publish output as the deployment artifact
- ArchiveFiles@2 — zip packaging for Zip Deploy
- AzureWebApp@1 with package glob and appName
- App Service runtime stack alignment with published TFM
- App settings and secrets applied at deploy time, not baked into artifact

**Answer**

A typical deployment pipeline has two stages. The Build stage installs the SDK with `UseDotNet@2`, runs restore, build, and test via `DotNetCoreCLI@2`, then publishes with `dotnet publish --configuration Release --output $(Build.ArtifactStagingDirectory)`. `ArchiveFiles@2` zips that folder for Zip Deploy compatibility, and `PublishPipelineArtifact@1` uploads the zip as a named artifact. The Deploy stage uses a deployment job targeting the appropriate environment; it downloads the artifact with `DownloadPipelineArtifact@2` and runs `AzureWebApp@1` with the `azureSubscription`, `appName`, and `package` pointing to the zip. The App Service runtime stack must match the application's target framework — deploying a `net10.0` app to an App Service configured for .NET 8 causes startup failures. Production secrets should be applied through the task's `appSettings` input or Azure Key Vault references rather than embedded in `appsettings.json` inside the artifact.

---

## Q27. What is the difference between deploying to a staging slot and swapping to production in Azure App Service via a pipeline?

**Concepts**
- Deployment slot — isolated pre-production App Service instance with its own hostname
- deployToSlotOrASE — slot targeting in AzureWebApp@1
- AzureAppServiceManage@0 Swap Slots action
- Warm-up and smoke testing against staging URL before swap
- Blue-green deployment — atomic swap with fast rollback via re-swap

**Answer**

A deployment slot is a separate live instance of an App Service app — `staging`, for example — with its own hostname and configuration. Deploying to the staging slot with `AzureWebApp@1` (using `deployToSlotOrASE: true` and `slotName: staging`) updates the pre-production copy without affecting production traffic at all. After deployment, I run smoke tests or health checks against the staging hostname to validate the new bits. Slot swap via `AzureAppServiceManage@0` with action `Swap Slots` is then an atomic operation that exchanges staging and production: the newly deployed version begins receiving production traffic and the previous production version moves to the staging slot. Since the previous production version is immediately available in staging, rolling back is another swap, not a re-deploy — which makes this the recommended zero-downtime release pattern for production ASP.NET Core APIs managed through Azure DevOps.

---

## Q28. What are common causes and troubleshooting steps when an Azure DevOps pipeline fails during .NET build or App Service deployment?

**Concepts**
- SDK version mismatch — UseDotNet@2 vs TargetFramework in csproj
- NuGetAuthenticate@1 for private feed authentication failures
- Service connection authorization and service principal role expiry
- App Service runtime stack mismatch with published TFM
- system.debug: true for verbose task diagnostics

**Answer**

Build failures almost always trace to one of four causes: the SDK version installed by `UseDotNet@2` does not match the `TargetFramework` in the project file; NuGet restore fails because private feed credentials are missing (fixed by adding `NuGetAuthenticate@1` before restore); failing or timing-out tests block the step; or the `projects:` glob in the task matches nothing because the project was moved or the pattern is wrong. Deployment failures have a different set of root causes: the service connection is unauthorized or the service principal's role assignment expired; the artifact download step did not run or the `package` path glob does not match the zip file name; the App Service runtime stack in the Azure portal is set to a different .NET version than the published TFM; or the app fails to start after a successful deploy because connection strings or `ASPNETCORE_ENVIRONMENT` are misconfigured — those appear in the App Service log stream, not the pipeline log. Setting `system.debug: true` as a pipeline variable enables verbose diagnostic output from all tasks, which is the fastest way to diagnose authentication and MSDeploy issues without guessing.

---

## Gotchas — Azure DevOps Pipelines (Interview Traps)

---

#### Gotcha 1. Secret variables are not automatically passed to child stages — they must be explicitly mapped

**Concepts**
- Secret pipeline variables are accessible as environment variables only when explicitly mapped
- `env:` mapping in job steps is required; reading `$(MySecret)` directly in a script fails
- Multi-stage pipelines require variable output or explicit variable passing between stages
- Secret variables cannot be echoed; they are masked in logs even when referenced directly

**Answer**

Pipeline variables marked as secret are not automatically injected as environment variables in every step. They must be explicitly mapped in the `env:` section of a step: `env: MY_SECRET: $(MySecret)`. A script that tries to read a secret variable using `$(MySecret)` inline in a bash or PowerShell command may see an empty string because the shell has no environment variable of that name, and the masking behavior hides the difference between an empty value and a masked value in the logs. Multi-stage pipelines have an additional restriction: secret variables defined in stage A are not automatically available in stage B; they require an output variable mechanism or must be declared at the pipeline level.

---

#### Gotcha 2. Service connection permissions are project-scoped — a pipeline in project A cannot use project B's service connection

**Concepts**
- Azure service connections are defined within a specific Azure DevOps project
- Cross-project service connection sharing requires explicit sharing configuration in project settings
- A pipeline that references a service connection by name from another project fails with "not found"
- Organization-level service connections do not exist; they must be shared per project

**Answer**

Azure DevOps service connections (used to deploy to Azure resources) are scoped to the project where they are created. A pipeline in a different project that references the connection by name receives a "service connection not found" error, even if both projects are in the same organization. To share a service connection, you must explicitly share it from the source project's settings to the target project. This is a common failure in organization-wide repository templates that reference a centrally managed service connection by a fixed name, when the consuming project has not had the connection shared to it.

---

#### Gotcha 3. Artifact retention policy silently deletes old pipeline runs — scripts referencing artifacts by run number break after retention expiry

**Concepts**
- Pipeline retention policies delete runs and their artifacts after a configured number of days
- Deployment stages that reference an artifact from a specific prior run fail if the run is deleted
- Retained runs (pinned) are exempt from the policy
- Passing artifact version information through output variables is more robust than run number references

**Answer**

Azure DevOps applies retention policies that delete pipeline runs and their associated artifacts after a configured period (typically 30 days by default). If a deployment script or release pipeline references an artifact by storing the originating run number, the reference will break after retention expires, causing deployment failures for rollback scenarios that rely on older artifact versions. The production practice is to pin important runs (retain indefinitely) for release builds, or to publish artifacts to Azure Artifacts or Azure Blob Storage outside the pipeline run scope, so they are not subject to pipeline retention limits.

---

#### Gotcha 4. Self-hosted agent runs as the configured service account — deployment to App Service requires that account to have Azure RBAC

**Concepts**
- Self-hosted agents run tasks as the Windows service account or Linux user they are configured with
- Azure CLI and deployment tasks authenticate using that account's Azure credentials or service principal
- Missing Azure RBAC on the subscription or resource group causes silent authentication errors
- Microsoft-hosted agents use ephemeral credentials; self-hosted agents carry persistent identity

**Answer**

A self-hosted Azure DevOps agent runs pipeline tasks as the Windows service account or Linux user it is installed under. When a pipeline step uses the Azure CLI or an Azure DevOps service connection to deploy to App Service, the agent's configured identity must have sufficient Azure RBAC (at minimum Contributor on the target resource group). On a Microsoft-hosted agent, this is handled automatically through the service connection; on a self-hosted agent, the underlying service account must separately have the correct roles. A common mistake is configuring the service connection correctly but using a self-hosted agent whose service account has no Azure permissions, producing an "Insufficient privileges" error.

---

#### Gotcha 5. Parallel stages without explicit dependsOn can deploy to production before staging validation completes

**Concepts**
- YAML multi-stage pipelines run stages in parallel unless `dependsOn` is specified
- A production deployment stage without `dependsOn: [Staging]` runs concurrently with staging
- Build stage success does not imply staging deployment success in parallel pipelines
- Explicit `dependsOn` with `condition: succeeded()` chains stages sequentially

**Answer**

In a YAML pipeline, multiple stages that do not declare `dependsOn` run in parallel by default. A pipeline with separate Build, Staging, and Production stages where Production does not declare `dependsOn: [Staging]` will deploy to production simultaneously with staging, not after it. This means staging validation, smoke tests, and approval gates in the staging stage have no bearing on production deployment timing. Every downstream stage must explicitly declare `dependsOn` on its predecessor to create a sequential dependency chain, and `condition: succeeded('Staging')` adds an explicit gate so a staging failure stops production deployment.

---

#### Gotcha 6. Environment approvals only apply to deployment jobs — regular pipeline jobs bypass environment gates

**Concepts**
- `environment:` key in a deployment job associates it with an APIM-protected Environment resource
- Regular `job:` steps that deploy using Azure CLI do not go through Environment approval gates
- Gates (pre-deployment conditions) are also only available on deployment jobs
- Approval checks configured on the Environment have no effect on non-deployment jobs

**Answer**

Azure DevOps Environments provide approval gates, required reviewer checks, and deployment history tracking. These controls apply only when a stage uses a `deployment` job type with the `environment:` key. A pipeline step inside a regular `job:` that runs `az webapp deploy` bypasses all environment approval checks completely, even if that environment has required approvals configured. Teams that configure environment approvals expecting all deployments to require sign-off are unaware that non-deployment job types silently bypass the gate, until an unauthorized deployment reaches production.

---

#### Gotcha 7. Key Vault variable group requires service connection approval — pipelines fail with access denied if not approved

**Concepts**
- Variable groups linked to Azure Key Vault use a service connection to fetch secrets at queue time
- The service connection must be approved for use in the pipeline in Azure DevOps project settings
- First-time use of a linked variable group prompts an approval that must be granted manually
- Pipeline failure message "access denied to variable group" is cryptic about the service connection requirement

**Answer**

An Azure DevOps variable group linked to Azure Key Vault uses an Azure service connection to access Key Vault secrets when the pipeline is queued. The first time a pipeline uses this variable group, Azure DevOps prompts for an authorization that must be explicitly granted in the pipeline settings or Project Settings. Pipelines that fail with "access denied to variable group" after a recent Key Vault variable group was added are almost always missing this authorization step. The service connection must also have Key Vault Secrets User role on the target Key Vault; otherwise secret fetch fails at runtime with a permissions error.

---

#### Gotcha 8. Running dotnet test without --no-build in a post-build stage causes double compilation

**Concepts**
- `dotnet test` compiles the project by default before running tests
- A pipeline stage that has already run `dotnet build` compiles twice if `--no-build` is not passed
- Double compilation doubles build time and wastes agent minutes
- `--no-build` combined with `--no-restore` produces the fastest test run in a multi-step pipeline

**Answer**

`dotnet test` invokes the build system by default before running tests, which is useful for standalone execution but wasteful in a CI pipeline where a preceding step has already called `dotnet build`. In a pipeline that separates Build and Test into two tasks, omitting `--no-build` on the `dotnet test` command causes the project to compile twice, doubling compile time and wasting agent minutes. The correct CI pipeline pattern is `dotnet build --configuration Release` followed by `dotnet test --no-build --configuration Release`, optionally combined with `--no-restore` if the restore step ran separately.

---

#### Gotcha 9. Branch policies for required pipeline success only apply to PRs — direct pushes to main bypass the pipeline

**Concepts**
- Branch policies requiring build validation apply to pull requests, not direct pushes
- A developer with push permission can bypass PR policies by pushing directly to main
- Branch protection that requires PR also requires "Require a pull request before merging" setting
- Service accounts used in pipelines may also have push access and bypass PR policies

**Answer**

Azure DevOps branch policies can require a successful build validation pipeline before a PR is completed. However, this policy applies only to pull request merges; it does not block direct pushes to the protected branch. A developer with Contribute permission can `git push` directly to main, bypassing the PR requirement and build validation entirely. To prevent this, the branch policy must also enable "Require a minimum number of reviewers" and check "When new changes are pushed, reset all approval votes", which together force all changes through PRs. Service accounts and pipeline identities with Contribute permission must also have their direct push access reviewed.

---

#### Gotcha 10. YAML template references require the template repository to be explicitly listed as a resource — cross-org templates silently fail

**Concepts**
- `extends: template@<alias>` requires the repository alias to be declared in `resources.repositories`
- Templates from external repositories require explicit access grants in project settings
- YAML syntax errors in referenced templates cause the pipeline to fail to parse with a confusing error
- Updating a shared template can break multiple consuming pipelines if the template interface changes

**Answer**

Using YAML templates from another repository (via `extends: template` or `steps: template`) requires the external repository to be declared in the pipeline's `resources.repositories` section with a `name`, `type`, and optionally a `ref`. Without this declaration, the template reference produces a confusing parse error rather than a clear "repository not found" message. Additionally, for external repositories in a different Azure DevOps project or organization, the service connection used for the repository resource must be explicitly authorized for pipeline use. Changing the interface of a shared template (renaming parameters, removing required inputs) breaks all consuming pipelines simultaneously unless versioned `ref` pinning is used.

---
