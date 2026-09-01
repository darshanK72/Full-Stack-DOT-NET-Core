# Interview Questions — Docker & Containerization — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Docker and what problem does it solve? How do containers differ from virtual machines?](#q1-what-is-docker-and-what-problem-does-it-solve-how-do-containers-differ-from-virtual-machines)
2. [Q2. What is the difference between a Docker image and a Docker container?](#q2-what-is-the-difference-between-a-docker-image-and-a-docker-container)
3. [Q3. What is a Dockerfile? Walk through the key instructions: `FROM`, `WORKDIR`, `COPY`, `RUN`, `EXPOSE`, `ENTRYPOINT`, and `CMD`.](#q3-what-is-a-dockerfile-walk-through-the-key-instructions-from-workdir-copy-run-expose-entrypoint-and-cmd)
4. [Q4. What is the difference between `CMD` and `ENTRYPOINT`, and why does exec form vs shell form matter for .NET apps?](#q4-what-is-the-difference-between-cmd-and-entrypoint-and-why-does-exec-form-vs-shell-form-matter-for-net-apps)
5. [Q5. What is a multi-stage build, and why is it essential for .NET containerization?](#q5-what-is-a-multi-stage-build-and-why-is-it-essential-for-net-containerization)
6. [Q6. Which .NET base images should you use for production, and what is the difference between `sdk`, `aspnet`, `runtime`, and `runtime-deps`?](#q6-which-net-base-images-should-you-use-for-production-and-what-is-the-difference-between-sdk-aspnet-runtime-and-runtime-deps)
7. [Q7. How does Docker layer caching work, and how do you structure a .NET Dockerfile to maximize cache hits?](#q7-how-does-docker-layer-caching-work-and-how-do-you-structure-a-net-dockerfile-to-maximize-cache-hits)
8. [Q8. How does ASP.NET Core determine which port to listen on inside a container? What roles do `EXPOSE`, `ASPNETCORE_URLS`, and `ASPNETCORE_HTTP_PORTS` play?](#q8-how-does-aspnet-core-determine-which-port-to-listen-on-inside-a-container-what-roles-do-expose-aspnetcore_urls-and-aspnetcore_http_ports-play)
9. [Q9. How do you pass configuration and connection strings to a containerized ASP.NET Core app?](#q9-how-do-you-pass-configuration-and-connection-strings-to-a-containerized-aspnet-core-app)
10. [Q10. What are Docker volumes, and when do you use a volume versus a bind mount?](#q10-what-are-docker-volumes-and-when-do-you-use-a-volume-versus-a-bind-mount)
11. [Q11. Explain Docker networking — what are bridge, host, and overlay networks, and which applies to microservices communication?](#q11-explain-docker-networking-what-are-bridge-host-and-overlay-networks-and-which-applies-to-microservices-communication)
12. [Q12. What is Docker Compose, and when would you use it instead of plain `docker run`?](#q12-what-is-docker-compose-and-when-would-you-use-it-instead-of-plain-docker-run)
13. [Q13. How do you write a production-ready `docker-compose.yml` for an ASP.NET Core microservice with a SQL Server database?](#q13-how-do-you-write-a-production-ready-docker-composeyml-for-an-aspnet-core-microservice-with-a-sql-server-database)
14. [Q14. How do you add a health check to a .NET container, and how does Docker use it?](#q14-how-do-you-add-a-health-check-to-a-net-container-and-how-does-docker-use-it)
15. [Q15. How do you manage secrets in a containerized environment — what are the options and their trade-offs?](#q15-how-do-you-manage-secrets-in-a-containerized-environment-what-are-the-options-and-their-trade-offs)
16. [Q16. What are the security best practices for running .NET applications in containers?](#q16-what-are-the-security-best-practices-for-running-net-applications-in-containers)
17. [Q17. What is the difference between framework-dependent, self-contained, and Native AOT publishing for Docker, and which is preferred?](#q17-what-is-the-difference-between-framework-dependent-self-contained-and-native-aot-publishing-for-docker-and-which-is-preferred)
18. [Q18. How do you debug a containerized ASP.NET Core application?](#q18-how-do-you-debug-a-containerized-aspnet-core-application)

---

## Q1. What is Docker and what problem does it solve? How do containers differ from virtual machines?

What is Docker and what problem does it solve? How do containers differ from virtual machines?

Docker is a platform that packages an application and all its dependencies — runtime, libraries, OS libraries, configuration — into a portable unit called a **container**. It solves the "works on my machine" problem: a container runs identically regardless of the host OS, whether that host is a developer laptop, a CI server, or a production cloud VM.

**Containers vs virtual machines (VMs):**

| | Virtual Machine | Container |
|---|---|---|
| Isolation boundary | Full OS kernel + user-space | User-space only (shares host kernel) |
| Startup time | Minutes | Seconds or milliseconds |
| Size on disk | Gigabytes (full OS image) | Megabytes (just app + minimal OS layer) |
| Density on one host | Dozens | Hundreds |
| Security isolation | Strongest (separate kernel) | Weaker (kernel shared) |
| Best for | Running different OS kernels, strong security isolation | Microservices, cloud-native apps, CI pipelines |

- A VM includes a complete operating system kernel running on a hypervisor. Each VM is an entire machine simulation.
- A container shares the host kernel and isolates only the user-space: filesystem, processes, and network. This makes containers far lighter and faster to start.
- Both can coexist: you often run many containers inside a single VM to get the security benefits of VM isolation at the machine boundary and the density benefits of containers within that boundary.

---

## Q2. What is the difference between a Docker image and a Docker container?

What is the difference between a Docker image and a Docker container?

A Docker **image** is a read-only, layered snapshot of a filesystem plus metadata (environment variables, entry point, exposed ports). It is the build artifact — the blueprint. Images are stored in registries (Docker Hub, Azure Container Registry) and pulled by name and tag.

A Docker **container** is a running instance of an image. The container engine adds a thin, writable layer on top of the read-only image layers and starts a process inside that isolated environment. Multiple containers can run from the same image simultaneously; each has its own writable layer and process space, so they don't interfere with each other.

- Think of the image as a class definition and the container as an object instantiated from that class.
- Stopping a container does not delete it — it retains its writable layer and can be restarted. Removing a container (`docker rm`) discards the writable layer, but the underlying image is untouched.
- An image is built once and run many times; a container is ephemeral and should be treated as disposable in production.

---

## Q3. What is a Dockerfile? Walk through the key instructions: `FROM`, `WORKDIR`, `COPY`, `RUN`, `EXPOSE`, `ENTRYPOINT`, and `CMD`.

What is a Dockerfile? Walk through the key instructions: `FROM`, `WORKDIR`, `COPY`, `RUN`, `EXPOSE`, `ENTRYPOINT`, and `CMD`.

A **Dockerfile** is a plain-text recipe that describes how to build a Docker image, one instruction per layer. Each instruction adds a new layer on top of the previous one.

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0   # base image
WORKDIR /app                                 # set working directory inside the image
COPY publish/ .                              # copy files from host to image filesystem
RUN chmod +x ./entrypoint.sh                # execute a shell command during build
EXPOSE 8080                                  # document which port the app listens on
ENTRYPOINT ["dotnet", "MyApp.dll"]          # fixed command when container starts
```

**Key instructions explained:**

- **`FROM`** — every Dockerfile starts here. Sets the base image (and implicitly its OS, runtime, libraries). Multi-stage builds use multiple `FROM` statements.
- **`WORKDIR`** — creates the directory if it doesn't exist and sets it as the working directory for subsequent `RUN`, `COPY`, `CMD`, and `ENTRYPOINT` instructions.
- **`COPY`** — copies files or directories from the build context (your host machine) into the image. `ADD` is a similar but more powerful instruction (supports URLs and tar auto-extraction); prefer `COPY` unless you need those extras.
- **`RUN`** — executes a shell command during the image build and commits the result as a new layer. Common uses: `dotnet restore`, `apt-get install`, file permission changes.
- **`EXPOSE`** — documentation only. It records which port the container's app listens on, but does not actually publish that port to the host. Publishing is done at `docker run -p`.
- **`ENTRYPOINT`** — the fixed executable for the container. Arguments from `docker run` are appended to it.
- **`CMD`** — default arguments passed to `ENTRYPOINT`, or the full command if no `ENTRYPOINT` is set. Overridden by anything passed on the `docker run` command line.

---

## Q4. What is the difference between `CMD` and `ENTRYPOINT`, and why does exec form vs shell form matter for .NET apps?

What is the difference between `CMD` and `ENTRYPOINT`, and why does exec form vs shell form matter for .NET apps?

`ENTRYPOINT` defines the **fixed executable** that always runs when the container starts. `CMD` provides **default arguments** to `ENTRYPOINT` — or, if there is no `ENTRYPOINT`, `CMD` is the full command. The key difference is overridability: arguments passed to `docker run` override `CMD` but are *appended* to `ENTRYPOINT`.

```dockerfile
ENTRYPOINT ["dotnet"]
CMD ["MyApp.dll"]
# docker run myimage OtherApp.dll → runs: dotnet OtherApp.dll
```

**Exec form vs shell form:**

| | Exec form | Shell form |
|---|---|---|
| Syntax | `["dotnet", "MyApp.dll"]` | `dotnet MyApp.dll` |
| PID 1 | Your process directly | `/bin/sh -c` (shell) |
| Signal handling | SIGTERM reaches your app | SIGTERM goes to shell, may not reach app |
| Variable expansion | No shell expansion | Yes (`$VAR` expands) |

For .NET apps this matters for **graceful shutdown**. When Kubernetes or Docker stops a container, it sends `SIGTERM` to PID 1. If your app is PID 1 (exec form), it receives the signal and can shut down gracefully — draining in-flight requests, flushing logs, releasing resources. If the shell is PID 1 (shell form), the signal may never reach the .NET process, forcing Docker to escalate to `SIGKILL` after the timeout.

Always use exec form for .NET production containers.

---

## Q5. What is a multi-stage build, and why is it essential for .NET containerization?

What is a multi-stage build, and why is it essential for .NET containerization?

A **multi-stage build** uses multiple `FROM` instructions in a single Dockerfile. Each `FROM` starts a new stage with its own base image. Artifacts from one stage are copied into a later stage using `COPY --from=<stage>`. Only the final stage ends up in the image that gets tagged and shipped.

For .NET this is essential because two very different images are needed at two different points:

- **Build stage** uses `mcr.microsoft.com/dotnet/sdk` (~700 MB) — contains Roslyn, MSBuild, NuGet, and all tooling needed to compile and publish.
- **Runtime stage** uses `mcr.microsoft.com/dotnet/aspnet` (~200 MB) — contains only the ASP.NET Core runtime, nothing else.

```dockerfile
# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2 — runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

- Without multi-stage builds, you would have to include the full SDK in production — a ~700 MB image versus ~200 MB, and with far more attack surface (compilers, build tools, shell utilities).
- The SDK image also contains source code, `.obj` files, and NuGet caches that have no place in a production image.
- Multi-stage builds keep the entire build process reproducible in one file with no external build scripts.

---

## Q6. Which .NET base images should you use for production, and what is the difference between `sdk`, `aspnet`, `runtime`, and `runtime-deps`?

Which .NET base images should you use for production, and what is the difference between `sdk`, `aspnet`, `runtime`, and `runtime-deps`?

All official .NET images are published by Microsoft on MCR (`mcr.microsoft.com/dotnet`):

| Image | Contains | Size | Use for |
|---|---|---|---|
| `dotnet/sdk` | Runtime + ASP.NET + build tools (Roslyn, MSBuild, dotnet CLI) | ~700 MB | Build stages only |
| `dotnet/aspnet` | .NET runtime + ASP.NET Core libraries | ~200 MB | ASP.NET Core apps in production |
| `dotnet/runtime` | .NET runtime only (no ASP.NET) | ~120 MB | Console apps, Worker Services |
| `dotnet/runtime-deps` | Native OS dependencies only, no .NET | ~30 MB | Self-contained or Native AOT apps |

- Use `aspnet` for any app using `WebApplication`, controllers, Minimal APIs, gRPC, or SignalR.
- Use `runtime` for background Worker Services that do not host an HTTP server.
- Use `runtime-deps` when publishing self-contained (`dotnet publish --self-contained`) or Native AOT — the .NET runtime is bundled in the app binary, so no runtime image is needed.
- Microsoft also publishes **chiseled variants** (`dotnet/nightly/aspnet:10.0-jammy-chiseled`) — stripped Ubuntu images with no shell, no package manager, and minimal attack surface. These are the most secure option for production.

---

## Q7. How does Docker layer caching work, and how do you structure a .NET Dockerfile to maximize cache hits?

How does Docker layer caching work, and how do you structure a .NET Dockerfile to maximize cache hits?

Every Dockerfile instruction that modifies the filesystem creates a new image **layer**. Docker caches each layer by hashing its inputs (the instruction text plus the files it touches). On a rebuild, Docker reuses a cached layer if its inputs are unchanged — and reuses all subsequent layers too, since nothing above has changed. As soon as any layer's inputs change, that layer and every layer after it are rebuilt from scratch.

The expensive operation in a .NET build is `dotnet restore` — it downloads all NuGet packages. The trick is to ensure restore is in its own layer that is only invalidated when project files change, not when source code changes:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Layer 1: copy only project files and restore — cached until .csproj changes
COPY MyApp.csproj .
RUN dotnet restore

# Layer 2: copy all source and publish — cache miss when any .cs changes
COPY . .
RUN dotnet publish -c Release -o /app/publish
```

- Without this pattern, `COPY . .` would include source files, so any code change would invalidate the `dotnet restore` layer and re-download all packages on every build.
- For solutions with multiple projects, copy all `.csproj` and `.sln` files first (using a wildcard or separate `COPY` lines), restore, then copy sources.
- Also use `.dockerignore` to exclude `bin/`, `obj/`, `.git/`, and other files from the build context — this keeps the context small and prevents unnecessary cache invalidation.

---

## Q8. How does ASP.NET Core determine which port to listen on inside a container? What roles do `EXPOSE`, `ASPNETCORE_URLS`, and `ASPNETCORE_HTTP_PORTS` play?

How does ASP.NET Core determine which port to listen on inside a container? What roles do `EXPOSE`, `ASPNETCORE_URLS`, and `ASPNETCORE_HTTP_PORTS` play?

Kestrel (ASP.NET Core's built-in web server) determines its listening address from configuration at startup. The sources, in priority order, are: `ASPNETCORE_URLS` environment variable, `ASPNETCORE_HTTP_PORTS` / `ASPNETCORE_HTTPS_PORTS` (added in .NET 8), then defaults in `appsettings.json` or code.

**Default port:** In .NET 8+, the default is `http://+:8080` (changed from port 80 in earlier versions). This change was made so containers do not need to run as root to bind to a privileged port.

**The three configuration points:**

- **`EXPOSE 8080`** in the Dockerfile is metadata only — it documents that the container app listens on port 8080 but does not publish or open that port. The actual port mapping is done at runtime with `docker run -p <host-port>:8080` or in `docker-compose.yml` under `ports`.
- **`ASPNETCORE_URLS=http://+:8080`** is the original way to override Kestrel's listening address. The `+` means "all network interfaces," which is required inside a container.
- **`ASPNETCORE_HTTP_PORTS=8080`** is the cleaner alternative in .NET 8+. Set this environment variable and Kestrel listens on `http://+:<value>` automatically. Use `ASPNETCORE_HTTPS_PORTS` alongside it if you terminate TLS inside the container (uncommon in Kubernetes where TLS terminates at the ingress).

In a Kubernetes deployment, you typically expose HTTP only inside the cluster (port 8080) and let the ingress controller handle TLS termination externally.

---

## Q9. How do you pass configuration and connection strings to a containerized ASP.NET Core app?

How do you pass configuration and connection strings to a containerized ASP.NET Core app?

ASP.NET Core's configuration system processes multiple sources in order, with each layer overriding the previous: `appsettings.json` → `appsettings.{Environment}.json` → environment variables → command-line arguments. Environment variables are the standard container-friendly mechanism because they require no image rebuild to change.

**Mapping environment variables to config sections:** ASP.NET Core uses `__` (double underscore) as the hierarchy separator in environment variable names. For example, `ConnectionStrings__DefaultConnection` maps to `ConnectionStrings:DefaultConnection` in config.

Ways to inject configuration at runtime:

- **`docker run -e`**: `docker run -e ConnectionStrings__DefaultConnection="Server=db;..." myapp` — one-off, good for testing.
- **`docker-compose.yml` `environment` section** — declarative, version-controlled (use `${VARIABLE}` references to keep secrets out of the file).
- **`.env` file with `env_file`** — environment variables in a file not committed to source control, referenced by Compose.
- **Kubernetes ConfigMaps** — for non-sensitive config; mounted as environment variables or files.
- **Kubernetes Secrets / Azure Key Vault** — for connection strings, API keys, and credentials.

Never bake credentials into the Docker image itself. Images are often stored in registries accessible to many people, and credentials embedded in layers persist even if removed in a later layer.

---

## Q10. What are Docker volumes, and when do you use a volume versus a bind mount?

What are Docker volumes, and when do you use a volume versus a bind mount?

By default, a container's writable layer is ephemeral — when the container is removed, all data written to it is lost. **Volumes** and **bind mounts** are the two mechanisms for storing data that survives beyond a container's lifetime.

**Volume** (`docker volume create mydata` / `-v mydata:/app/data`): Docker creates and manages the storage location on the host. The exact host path is opaque to you. Data persists across container restarts and removals, and can be shared between multiple containers.

**Bind mount** (`-v /host/path:/container/path`): A specific host directory is mounted directly into the container. You control the exact host path. Changes on the host are immediately visible in the container and vice versa.

| | Volume | Bind Mount |
|---|---|---|
| Host path | Managed by Docker | Specified by you |
| Portability | High (works on any host) | Low (path must exist on host) |
| Best for production | Yes | No (path-dependent) |
| Best for dev | Sometimes | Yes (live code reload) |

- Use **volumes** for database data, uploaded files, and any production persistent storage. Docker handles backup, migration, and driver configuration.
- Use **bind mounts** during local development — for example, mounting the source directory into a container running `dotnet watch run` to get hot reload without rebuilding the image.

---

## Q11. Explain Docker networking — what are bridge, host, and overlay networks, and which applies to microservices communication?

Explain Docker networking — what are bridge, host, and overlay networks, and which applies to microservices communication?

Docker containers are isolated from the host network and from each other unless connected through a Docker network. Docker provides several network drivers:

| Driver | Description | Use case |
|---|---|---|
| **bridge** (default) | Virtual switch; containers get private IPs; communicate by name within the same network | Local dev, single-host microservices |
| **host** | Container shares the host's network stack; no NAT, no isolation | Linux only; highest network performance |
| **overlay** | Spans multiple Docker hosts; used by Docker Swarm | Multi-host clusters |
| **none** | No network at all | Completely isolated containers |

- **Bridge networks** are the default and most common. When Docker Compose creates a project, it automatically creates a bridge network and attaches all services to it. Services can reach each other by service name (e.g., `http://api:8080`), which Docker resolves via its built-in DNS.
- **Custom bridge networks** (created explicitly) also support name-based DNS, unlike the default `bridge` network which only supports IP-based communication. Always use a named network in Compose projects.
- **Overlay networks** are for Docker Swarm or as the foundation for Kubernetes networking — they route packets across physical machines in a cluster.
- For ASP.NET Core microservices on a single machine (local dev or simple deployments), bridge networks with Compose are the correct choice. In Kubernetes, the CNI (Container Network Interface) plugin handles cross-node networking, making overlay concerns transparent.

---

## Q12. What is Docker Compose, and when would you use it instead of plain `docker run`?

What is Docker Compose, and when would you use it instead of plain `docker run`?

**Docker Compose** is a tool for defining and running multi-container applications. A single `docker-compose.yml` file declares all services, their images or build contexts, environment variables, ports, volumes, networks, and dependencies. A single `docker compose up` command starts the entire application.

Use Docker Compose when:

- **Local development**: Spin up an API + SQL Server + Redis + a message broker with one command. No manual `docker run` chains.
- **Integration tests**: CI pipelines can use Compose to start all dependencies before running tests, then tear everything down cleanly.
- **Simple deployments**: For small applications where Kubernetes is too much overhead.

Do not use Docker Compose when:

- You need multi-host orchestration, auto-scaling, or rolling deployments — use Kubernetes (or Docker Swarm).
- You need sophisticated scheduling, resource limits, or health-based restarts across a cluster.

Compose is excellent as a developer experience tool and for CI; it is generally not the production orchestrator for serious microservice deployments.

---

## Q13. How do you write a production-ready `docker-compose.yml` for an ASP.NET Core microservice with a SQL Server database?

How do you write a production-ready `docker-compose.yml` for an ASP.NET Core microservice with a SQL Server database?

A production-ready Compose file handles: health checks, service dependencies, persistent volumes, network isolation, and externalised secrets.

```yaml
services:
  api:
    build: .
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=AppDb;User=sa;Password=${DB_PASSWORD}
    depends_on:
      db:
        condition: service_healthy
    networks:
      - backend

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_PASSWORD}
    volumes:
      - sqldata:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$$SA_PASSWORD" -Q "SELECT 1" -No
      interval: 30s
      timeout: 5s
      retries: 5
    networks:
      - backend

volumes:
  sqldata:

networks:
  backend:
    driver: bridge
```

Key decisions:

- `depends_on: condition: service_healthy` ensures the API waits until SQL Server passes its health check before starting. Without this, the API often crashes on startup trying to connect to a database that is still initialising.
- `${DB_PASSWORD}` comes from a `.env` file (not committed to source control) or from the environment of the machine running Compose, keeping credentials out of the YAML file.
- The named volume `sqldata` persists database files across container restarts and removals.
- Both services are on a custom `backend` network; the API is not on the default network, limiting its exposure.

---

## Q14. How do you add a health check to a .NET container, and how does Docker use it?

How do you add a health check to a .NET container, and how does Docker use it?

There are two layers to health checking a containerised .NET app: the application-level health endpoint, and the Docker `HEALTHCHECK` instruction.

**Application layer** — ASP.NET Core has a built-in health check middleware:

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("Default")!);

app.MapHealthChecks("/health");
```

**Docker layer** — the `HEALTHCHECK` instruction in the Dockerfile tells the Docker daemon how to test whether the container is working:

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

Docker calls the command every `--interval`. If it exits non-zero `--retries` times consecutively, the container is marked `unhealthy`. Docker itself does not restart an `unhealthy` container by default — that is the job of the orchestrator.

- In Docker Compose, you can use `condition: service_healthy` in `depends_on` to delay dependent services until this check passes.
- In Kubernetes, `HEALTHCHECK` in the Dockerfile is ignored. Kubernetes uses its own **liveness** and **readiness probes** configured in the pod spec — these are more feature-rich (HTTP probes, gRPC probes, exec probes, configurable initial delays).
- Keep the health endpoint lightweight — it should check DB connectivity and critical dependencies, but not trigger heavy computation.

---

## Q15. How do you manage secrets in a containerized environment — what are the options and their trade-offs?

How do you manage secrets in a containerised environment — what are the options and their trade-offs?

**Options from least to most secure:**

| Method | How it works | Risk |
|---|---|---|
| Baked into image | Secret in Dockerfile `ENV` or `COPY` | Visible in image layers — never do this |
| Environment variable | `-e SECRET=value` at runtime | Visible in `docker inspect`, process list, logs |
| `.env` file (Compose) | File read by Compose, values set as env vars | File must be secured; still env vars at runtime |
| Docker Secrets (Swarm) | Encrypted at rest in Swarm state; mounted as file in `/run/secrets/` | Swarm-only |
| Kubernetes Secrets | Stored in etcd (encrypted at rest if configured); mounted as volume or env vars | Env var exposure risk; volume mount safer |
| External secret store | Azure Key Vault, AWS Secrets Manager, HashiCorp Vault — app fetches at startup via SDK | Most secure; secrets never in container storage |

For .NET on Azure, the recommended pattern is:

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{vaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

- Environment variables are acceptable for non-critical configuration, but database passwords and API keys should come from a vault or a secrets manager.
- In Kubernetes, prefer **volume-mounted secrets** over env-var secrets: files are not exposed in `kubectl describe pod` output and are easier to rotate without restarting the pod.
- Never store secrets in source control, Dockerfile, or build args that end up in image layers.

---

## Q16. What are the security best practices for running .NET applications in containers?

What are the security best practices for running .NET applications in containers?

**Run as a non-root user.** Linux containers run as root by default. A process running as root inside a container could cause significant damage if it escapes the container sandbox. The official .NET images include a pre-created `app` user:

```dockerfile
USER app
```

Add this line in the final stage, after copying files and before `ENTRYPOINT`.

**Use chiseled (minimal) base images.** Ubuntu Chiseled images remove the shell (`/bin/sh`), package manager, and hundreds of other utilities that are not needed at runtime. Fewer installed packages means fewer CVE (Common Vulnerability and Exposure) surface areas.

**Pin image versions.** Using `mcr.microsoft.com/dotnet/aspnet:latest` means your image silently changes when Microsoft updates the tag. Pin to a specific version (`aspnet:10.0`) or, for maximum reproducibility, pin to a digest (`@sha256:...`).

**Use `.dockerignore`.** Exclude `bin/`, `obj/`, `.git/`, `.env`, and any credentials files from the build context. Without it, sensitive files may end up in the image.

**Scan images for vulnerabilities.** Use `docker scout cves <image>`, Trivy, or Snyk as part of the CI pipeline. Catch known CVEs in base images before deployment.

**Restrict capabilities.** In Kubernetes, use `securityContext` to set `readOnlyRootFilesystem: true`, drop Linux capabilities you don't need, and set `allowPrivilegeEscalation: false`.

**Never run in `Development` mode in production.** `ASPNETCORE_ENVIRONMENT=Development` enables detailed error pages that leak stack traces and internal paths.

---

## Q17. What is the difference between framework-dependent, self-contained, and Native AOT publishing for Docker, and which is preferred?

What is the difference between framework-dependent, self-contained, and Native AOT publishing for Docker, and which is preferred?

**Framework-dependent (default):**

```
dotnet publish -c Release
```

The published output contains only the app's assemblies; the .NET runtime is not included. The container image uses a `dotnet/aspnet` or `dotnet/runtime` base image which provides the shared runtime. This is the standard approach.

- Smallest publish output.
- Multiple services can share the runtime layer in the container registry (pulled once, cached).
- The runtime must match the published `TargetFramework`.

**Self-contained:**

```
dotnet publish -c Release --self-contained
```

The .NET runtime is bundled with the app. The base image can be `dotnet/runtime-deps` (just native OS dependencies, no .NET). The final image is larger but has no dependency on a pre-installed runtime.

- Useful when you need to use a base image that has no .NET runtime (custom distroless or scratch images).
- Slower builds, larger images.

**Native AOT:**

```
dotnet publish -c Release -p:PublishAot=true
```

The entire application is compiled to a native binary at build time. No JIT, no CLR startup. Use `dotnet/runtime-deps` or `scratch` as the base.

- Fastest cold start (important for serverless — AWS Lambda, Azure Container Apps).
- Smallest final image when combined with trimming.
- Limitations: no `Assembly.Load` at runtime, limited dynamic reflection, `Reflection.Emit` not supported.

**Which to prefer for Docker?** Framework-dependent with the `aspnet` base image is the default and correct choice for most ASP.NET Core services. Native AOT is worth the investment for serverless or startup-sensitive services where image size and cold start time matter.

---

## Q18. How do you debug a containerized ASP.NET Core application?

How do you debug a containerised ASP.NET Core application?

**Visual Studio (Windows/Mac):** Add a Docker launch profile (Visual Studio does this automatically when you add Docker support). When you press F5 with the Docker profile selected, Visual Studio builds the image, starts the container, and attaches the `vsdbg` debugger automatically. Breakpoints work exactly as in local development.

**VS Code:** The Docker extension provides a "Docker: Attach to Node" / "Docker: Attach to .NET" flow. You select the running container and the extension installs `vsdbg` if needed and attaches the debugger.

**Manual vsdbg attach:**

```bash
docker exec -it <container-id> bash
# Install vsdbg if not present, then the debugger can attach
```

Connect using VS Code's "Attach to Remote Process" or Visual Studio's "Attach to Process" targeting the container's process.

**Logging and observability (production-appropriate):**

- `docker logs <container>` — tail stdout/stderr.
- Centralised logging via Seq, Elastic Stack, Azure Monitor, or Grafana Loki.
- Distributed tracing via OpenTelemetry + Jaeger or Zipkin.
- `ASPNETCORE_ENVIRONMENT=Development` to enable developer exception pages — but only in a non-production environment.

**Configuration for better diagnostics:**

```csharp
// Set minimum log level to Debug when diagnosing container issues
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

In practice, attach-based debugging is used during development and integration testing. Production issues are diagnosed through structured logs, traces, and metrics rather than live debugging.

---
