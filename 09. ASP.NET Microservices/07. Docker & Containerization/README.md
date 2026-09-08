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

**Concepts**
- Container as portable isolated user-space sharing the host kernel
- Shared kernel vs VM full OS kernel isolation boundary
- Startup time and density advantages of containers over VMs
- Containers and VMs as complementary deployment layers

**Answer**

Docker packages an application and all its dependencies — runtime, libraries, OS libraries, configuration — into a portable unit called a container, solving the "works on my machine" problem: a container runs identically regardless of whether the host is a developer laptop, a CI server, or a production cloud VM. A VM includes a complete operating system kernel running on a hypervisor, making each VM an entire machine simulation with startup times measured in minutes and sizes measured in gigabytes. A container shares the host kernel and isolates only the user-space — filesystem, processes, and network — which makes containers far lighter and faster to start, typically in seconds or milliseconds, and allows hundreds of them to run where only dozens of VMs would fit. The security isolation trade-off is real: because the kernel is shared, a container escape is more damaging than a VM escape, so in practice you often run many containers inside a single VM to get the security benefits of VM isolation at the machine boundary and the density benefits of containers within that boundary.

| | Virtual Machine | Container |
|---|---|---|
| Isolation boundary | Full OS kernel + user-space | User-space only (shares host kernel) |
| Startup time | Minutes | Seconds or milliseconds |
| Size on disk | Gigabytes | Megabytes |
| Density on one host | Dozens | Hundreds |
| Security isolation | Strongest | Weaker (kernel shared) |

---

## Q2. What is the difference between a Docker image and a Docker container?

**Concepts**
- Docker image as read-only layered filesystem snapshot
- Container as running image instance with a thin writable layer
- Multiple containers sharing one image without interference
- Container ephemeral writable layer discarded on removal

**Answer**

A Docker image is a read-only, layered snapshot of a filesystem plus metadata — environment variables, entry point, exposed ports. It is the build artifact, the blueprint. A Docker container is a running instance of an image: the container engine adds a thin writable layer on top of the read-only image layers and starts a process inside that isolated environment. Multiple containers can run from the same image simultaneously; each has its own writable layer and process space, so they do not interfere with each other. Think of the image as a class definition and the container as an object instantiated from that class. Stopping a container does not delete it — it retains its writable layer and can be restarted. Removing a container discards the writable layer but leaves the underlying image untouched. An image is built once and run many times; a container is ephemeral and should be treated as disposable in production.

---

## Q3. What is a Dockerfile? Walk through the key instructions: `FROM`, `WORKDIR`, `COPY`, `RUN`, `EXPOSE`, `ENTRYPOINT`, and `CMD`.

**Concepts**
- Dockerfile as layered image build recipe
- FROM as base image and implicit OS selection
- EXPOSE as documentation-only — no runtime port publishing
- ENTRYPOINT as fixed executable vs CMD as overridable default arguments

**Answer**

A Dockerfile is a plain-text recipe that describes how to build a Docker image, one instruction per layer. Each instruction adds a new layer on top of the previous one.

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0   # base image
WORKDIR /app                                 # set working directory inside the image
COPY publish/ .                              # copy files from host to image filesystem
RUN chmod +x ./entrypoint.sh                # execute a shell command during build
EXPOSE 8080                                  # document which port the app listens on
ENTRYPOINT ["dotnet", "MyApp.dll"]          # fixed command when container starts
```

`FROM` sets the base image and implicitly its OS, runtime, and libraries; multi-stage builds use multiple `FROM` statements. `WORKDIR` creates the directory if it does not exist and sets it as the working directory for subsequent `RUN`, `COPY`, `CMD`, and `ENTRYPOINT` instructions. `COPY` copies files or directories from the build context into the image; prefer it over `ADD` unless you need URL fetching or tar auto-extraction. `RUN` executes a shell command during the image build and commits the result as a new layer — common uses are `dotnet restore`, `apt-get install`, and file permission changes. `EXPOSE` is documentation only: it records which port the container's app listens on but does not publish that port to the host — publishing is done at `docker run -p`. `ENTRYPOINT` is the fixed executable for the container; arguments from `docker run` are appended to it. `CMD` provides default arguments passed to `ENTRYPOINT`, or the full command if no `ENTRYPOINT` is set, and is overridden by anything passed on the `docker run` command line.

---

## Q4. What is the difference between `CMD` and `ENTRYPOINT`, and why does exec form vs shell form matter for .NET apps?

**Concepts**
- ENTRYPOINT as fixed executable, CMD as overridable default arguments
- Exec form making the app PID 1 for direct signal handling
- Shell form making /bin/sh PID 1 — SIGTERM may not reach the app
- Graceful shutdown dependency on exec form for .NET containers

**Answer**

`ENTRYPOINT` defines the fixed executable that always runs when the container starts. `CMD` provides default arguments to `ENTRYPOINT`, or if there is no `ENTRYPOINT`, `CMD` is the full command. The key difference is overridability: arguments passed to `docker run` override `CMD` but are appended to `ENTRYPOINT`.

```dockerfile
ENTRYPOINT ["dotnet"]
CMD ["MyApp.dll"]
# docker run myimage OtherApp.dll → runs: dotnet OtherApp.dll
```

Exec form — `["dotnet", "MyApp.dll"]` — starts your process directly as PID 1. Shell form — `dotnet MyApp.dll` — wraps the command in `/bin/sh -c`, making the shell PID 1 and your application a child process. This matters for graceful shutdown: when Kubernetes or Docker stops a container, it sends SIGTERM to PID 1. With exec form, your .NET application is PID 1 and receives the signal, allowing it to drain in-flight requests, flush logs, and release resources before exiting. With shell form, SIGTERM goes to the shell, which may not forward it to the .NET process, forcing Docker to escalate to SIGKILL after the timeout and cutting off in-flight requests abruptly. Always use exec form for .NET production containers.

| | Exec form | Shell form |
|---|---|---|
| Syntax | `["dotnet", "MyApp.dll"]` | `dotnet MyApp.dll` |
| PID 1 | Your process directly | `/bin/sh -c` (shell) |
| Signal handling | SIGTERM reaches your app | SIGTERM goes to shell, may not reach app |

---

## Q5. What is a multi-stage build, and why is it essential for .NET containerization?

**Concepts**
- SDK image (~700 MB) for build stage only
- ASP.NET runtime image (~200 MB) for production stage
- COPY --from=stage for artifact transfer between stages
- Build tooling and source code exclusion from production image

**Answer**

A multi-stage build uses multiple `FROM` instructions in a single Dockerfile, where each `FROM` starts a new stage with its own base image. Artifacts from one stage are copied into a later stage using `COPY --from=<stage>`, and only the final stage ends up in the image that gets tagged and shipped. For .NET this is essential because the SDK image — which contains Roslyn, MSBuild, NuGet, and all tooling needed to compile and publish — is around 700 MB, while the ASP.NET Core runtime image is around 200 MB and contains only what is needed to run the application.

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

Without multi-stage builds, you would include the full SDK in production — adding roughly 500 MB and far more attack surface through compilers, build tools, and shell utilities that have no business in a running container. The SDK image also contains source code, `.obj` files, and NuGet caches that have no place in production. Multi-stage builds keep the entire build process reproducible in one file with no external build scripts.

---

## Q6. Which .NET base images should you use for production, and what is the difference between `sdk`, `aspnet`, `runtime`, and `runtime-deps`?

**Concepts**
- dotnet/sdk for build stages only — never ship in production
- dotnet/aspnet for ASP.NET Core apps with HTTP server
- dotnet/runtime for Worker Services without HTTP
- dotnet/runtime-deps for self-contained and Native AOT publishing
- Chiseled variants for minimal CVE surface in production

**Answer**

All official .NET images are published by Microsoft on MCR. The `dotnet/sdk` image contains the runtime plus ASP.NET plus all build tools and should only ever appear in build stages of multi-stage Dockerfiles — never in a production image. The `dotnet/aspnet` image contains the .NET runtime and ASP.NET Core libraries and is the correct production base for any app using `WebApplication`, controllers, Minimal APIs, gRPC, or SignalR. The `dotnet/runtime` image contains only the .NET runtime without ASP.NET Core and is the right choice for background Worker Services that do not host an HTTP server, since it is smaller. The `dotnet/runtime-deps` image contains only native OS dependencies with no .NET runtime at all and is used when publishing self-contained or Native AOT, since the .NET runtime is bundled in the app binary. Microsoft also publishes chiseled variants — stripped Ubuntu images with no shell, no package manager, and minimal attack surface — which are the most secure option for production.

| Image | Contains | Size | Use for |
|---|---|---|---|
| `dotnet/sdk` | Runtime + ASP.NET + build tools | ~700 MB | Build stages only |
| `dotnet/aspnet` | .NET runtime + ASP.NET Core | ~200 MB | ASP.NET Core apps |
| `dotnet/runtime` | .NET runtime only | ~120 MB | Console apps, Worker Services |
| `dotnet/runtime-deps` | Native OS deps only | ~30 MB | Self-contained or Native AOT |

---

## Q7. How does Docker layer caching work, and how do you structure a .NET Dockerfile to maximize cache hits?

**Concepts**
- Layer cache invalidation cascading to all subsequent layers
- csproj-first COPY pattern to isolate dotnet restore from source changes
- .dockerignore to minimize build context and prevent cache invalidation
- Source code change isolation so restore layer stays cached

**Answer**

Every Dockerfile instruction that modifies the filesystem creates a new image layer. Docker caches each layer by hashing its inputs — the instruction text plus the files it touches. On a rebuild, Docker reuses a cached layer if its inputs are unchanged, and reuses all subsequent layers too since nothing above has changed. As soon as any layer's inputs change, that layer and every layer after it are rebuilt from scratch. The expensive operation in a .NET build is `dotnet restore`, which downloads all NuGet packages. The trick is to copy only the project files and restore before copying source code, so the restore layer is only invalidated when project files change — not when source code changes.

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

Without this pattern, `COPY . .` would include source files so any code change invalidates the `dotnet restore` layer and re-downloads all packages on every build. For solutions with multiple projects, copy all `.csproj` and `.sln` files first, restore, then copy sources. Always use `.dockerignore` to exclude `bin/`, `obj/`, `.git/`, and credentials files from the build context — this keeps the context small and prevents unnecessary cache invalidation.

---

## Q8. How does ASP.NET Core determine which port to listen on inside a container? What roles do `EXPOSE`, `ASPNETCORE_URLS`, and `ASPNETCORE_HTTP_PORTS` play?

**Concepts**
- EXPOSE as metadata-only — no runtime port publishing
- ASPNETCORE_URLS for Kestrel listening address override
- ASPNETCORE_HTTP_PORTS as cleaner .NET 8+ alternative
- Default port 8080 in .NET 8+ to avoid root binding requirement

**Answer**

Kestrel determines its listening address from configuration at startup. The sources in priority order are `ASPNETCORE_URLS`, then `ASPNETCORE_HTTP_PORTS` and `ASPNETCORE_HTTPS_PORTS` added in .NET 8, then defaults in `appsettings.json` or code. In .NET 8+, the default listening port changed from 80 to 8080 so containers no longer need to run as root to bind to a privileged port. `EXPOSE 8080` in the Dockerfile is metadata only — it documents that the container app listens on port 8080 but does not publish or open that port. The actual port mapping is done at runtime with `docker run -p <host-port>:8080` or in `docker-compose.yml` under `ports`. `ASPNETCORE_URLS=http://+:8080` is the original way to override Kestrel's listening address; the `+` means all network interfaces, which is required inside a container since the app cannot listen on `localhost` alone and expect external traffic. `ASPNETCORE_HTTP_PORTS=8080` is the cleaner alternative in .NET 8+: set this environment variable and Kestrel listens on `http://+:<value>` automatically. In a Kubernetes deployment, you typically expose HTTP only inside the cluster on port 8080 and let the ingress controller handle TLS termination externally.

---

## Q9. How do you pass configuration and connection strings to a containerized ASP.NET Core app?

**Concepts**
- Environment variables as container-friendly config override mechanism
- Double-underscore hierarchy separator for nested keys on Linux
- Kubernetes ConfigMap for non-sensitive and Secret for sensitive config
- Never bake credentials into Docker image layers

**Answer**

ASP.NET Core's configuration system processes multiple sources in order — `appsettings.json`, `appsettings.{Environment}.json`, environment variables, command-line arguments — with each layer overriding the previous. Environment variables are the standard container-friendly mechanism because they require no image rebuild to change. ASP.NET Core uses double-underscore (`__`) as the hierarchy separator in environment variable names, so `ConnectionStrings__DefaultConnection` maps to `ConnectionStrings:DefaultConnection` in config. You can inject configuration via `docker run -e ConnectionStrings__DefaultConnection="Server=db;..."` for one-off testing, via the `environment` section in `docker-compose.yml` with `${VARIABLE}` references to keep secrets out of the file, via a `.env` file not committed to source control, via Kubernetes ConfigMaps for non-sensitive config, or via Kubernetes Secrets and Azure Key Vault for credentials. Never bake credentials into the Docker image itself — images are often stored in registries accessible to many people, and credentials embedded in layers persist even if removed in a later layer.

---

## Q10. What are Docker volumes, and when do you use a volume versus a bind mount?

**Concepts**
- Docker volume as Docker-managed opaque persistent storage
- Bind mount as explicit host-path-to-container mapping
- Volume for production database and file storage
- Bind mount for local development hot-reload workflows

**Answer**

By default, a container's writable layer is ephemeral — when the container is removed, all data written to it is lost. Volumes and bind mounts are the two mechanisms for storing data that survives beyond a container's lifetime. A volume is created and managed by Docker; the exact host path is opaque to you, data persists across container restarts and removals, and it can be shared between multiple containers. A bind mount maps a specific host directory directly into the container; you control the exact path and changes on the host are immediately visible in the container and vice versa. Use volumes for database data, uploaded files, and any production persistent storage — Docker handles backup, migration, and driver configuration and the storage works portably on any host. Use bind mounts during local development — for example, mounting the source directory into a container running `dotnet watch run` to get hot reload without rebuilding the image — but bind mounts are path-dependent and not appropriate for production since the host path must exist and match.

| | Volume | Bind Mount |
|---|---|---|
| Host path | Managed by Docker | Specified by you |
| Portability | High | Low (path must exist on host) |
| Best for production | Yes | No |
| Best for dev hot reload | Sometimes | Yes |

---

## Q11. Explain Docker networking — what are bridge, host, and overlay networks, and which applies to microservices communication?

**Concepts**
- Bridge network as default single-host virtual switch with service-name DNS
- Custom bridge network enabling name-based DNS resolution
- Overlay network for multi-host Docker Swarm clusters
- Docker Compose auto-created bridge network with container name resolution

**Answer**

Docker containers are isolated from the host network and from each other unless connected through a Docker network. Bridge networks are the default and most common: Docker creates a virtual switch, containers get private IP addresses, and they can communicate by service name within the same network. When Docker Compose creates a project, it automatically creates a bridge network and attaches all services to it, so services can reach each other by service name — `http://api:8080` — which Docker resolves via its built-in DNS. Custom bridge networks also support name-based DNS, unlike the default `bridge` network which only supports IP-based communication, so always use a named network in Compose projects. Host networking means the container shares the host's network stack with no NAT or isolation — available only on Linux and offers the highest network performance, but no isolation. Overlay networks span multiple Docker hosts and are used by Docker Swarm or as the foundation for Kubernetes networking; in Kubernetes, the CNI plugin handles cross-node networking and makes overlay concerns transparent. For ASP.NET Core microservices on a single machine in local dev or simple deployments, bridge networks with Compose are the correct choice.

| Driver | Description | Use case |
|---|---|---|
| bridge (default) | Private IPs; communicate by name within network | Local dev, single-host microservices |
| host | Shares host network stack; no NAT | Linux only; highest network performance |
| overlay | Spans multiple Docker hosts | Multi-host clusters |
| none | No network | Completely isolated containers |

---

## Q12. What is Docker Compose, and when would you use it instead of plain `docker run`?

**Concepts**
- Single YAML defining all services, networks, volumes, and dependencies
- docker compose up for single-command multi-container startup
- CI integration test dependency orchestration
- Kubernetes or Docker Swarm for multi-host production scaling

**Answer**

Docker Compose is a tool for defining and running multi-container applications. A single `docker-compose.yml` declares all services, their images or build contexts, environment variables, ports, volumes, networks, and dependencies, so a single `docker compose up` starts the entire application. The primary use cases are local development — spinning up an API plus SQL Server plus Redis plus a message broker with one command — and CI integration testing, where a pipeline can start all dependencies before running tests and then tear everything down cleanly. It is also viable for simple small deployments where Kubernetes is too much overhead. Docker Compose is not the right tool when you need multi-host orchestration, auto-scaling, rolling deployments, or sophisticated scheduling across a cluster — those require Kubernetes or Docker Swarm. Compose is excellent as a developer experience tool and for CI; it is generally not the production orchestrator for serious microservice deployments.

---

## Q13. How do you write a production-ready `docker-compose.yml` for an ASP.NET Core microservice with a SQL Server database?

**Concepts**
- depends_on with condition: service_healthy for ordered startup
- ${VARIABLE} substitution to externalise secrets from the YAML file
- Named volume for database file persistence across container restarts
- Custom bridge network limiting inter-service exposure

**Answer**

A production-ready Compose file handles health checks, service dependencies, persistent volumes, network isolation, and externalised secrets. The `depends_on: condition: service_healthy` ensures the API waits until SQL Server passes its health check before starting — without this, the API often crashes trying to connect to a database that is still initialising. The `${DB_PASSWORD}` value comes from a `.env` file not committed to source control or from the machine's environment, keeping credentials out of the YAML file. The named volume `sqldata` persists database files across container restarts and removals.

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

Both services are on a custom `backend` network rather than the default network, limiting their exposure and ensuring they can resolve each other by service name.

---

## Q14. How do you add a health check to a .NET container, and how does Docker use it?

**Concepts**
- ASP.NET Core AddHealthChecks() for application-level endpoint
- HEALTHCHECK instruction as Docker daemon periodic probe
- depends_on condition: service_healthy in Compose
- Kubernetes liveness and readiness probes superseding Docker HEALTHCHECK

**Answer**

There are two layers to health checking a containerised .NET app: the application-level health endpoint and the Docker `HEALTHCHECK` instruction. At the application layer, ASP.NET Core's built-in health check middleware exposes the endpoint:

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("Default")!);

app.MapHealthChecks("/health");
```

At the Docker layer, the `HEALTHCHECK` instruction tells the Docker daemon how to test whether the container is working:

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

Docker calls the command every `--interval`. If it exits non-zero `--retries` times consecutively, the container is marked `unhealthy`. Docker itself does not restart an `unhealthy` container by default — that is the job of the orchestrator. In Docker Compose, `condition: service_healthy` in `depends_on` delays dependent services until this check passes. In Kubernetes, the `HEALTHCHECK` instruction in the Dockerfile is ignored entirely; Kubernetes uses its own liveness and readiness probes configured in the pod spec, which are more feature-rich and offer HTTP probes, gRPC probes, exec probes, and configurable initial delays. Keep the health endpoint lightweight — it should verify DB connectivity and critical dependencies but not trigger heavy computation.

---

## Q15. How do you manage secrets in a containerized environment — what are the options and their trade-offs?

**Concepts**
- Baked-into-image secrets as worst practice — visible in all image layers
- Environment variable visibility in docker inspect and process listings
- Volume-mounted Kubernetes Secrets as safer than env-var secrets
- Azure Key Vault with DefaultAzureCredential as recommended best practice

**Answer**

The worst approach is baking secrets into the Docker image via `ENV` instructions or `COPY` of credential files — they are permanently visible in all image layers to anyone who can pull the image. Environment variables at runtime are slightly better but still visible in `docker inspect` output, process listings, crash dumps, and any observability tool capturing process state. Kubernetes Secrets stored in etcd and injected as environment variables are commonly used, but volume-mounted secrets are safer since they are not exposed in `kubectl describe pod` output and are easier to rotate without restarting the pod. The most secure approach for .NET on Azure is to fetch secrets from Azure Key Vault at startup using `DefaultAzureCredential`, which authenticates via managed identity so no credential is stored anywhere in the container or its environment.

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{vaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

Environment variables are acceptable for non-critical configuration, but database passwords and API keys should come from a vault. Never store secrets in source control, in the Dockerfile, or in Docker build args that end up in image layers.

| Method | Risk |
|---|---|
| Baked into image | Visible in image layers — never do this |
| Environment variable at runtime | Visible in `docker inspect`, process list, logs |
| Kubernetes Secret as env var | Slightly better; still env var at runtime |
| Volume-mounted Kubernetes Secret | Not in kubectl describe; easier to rotate |
| External secret store (Key Vault, Vault) | Most secure; secrets never in container storage |

---

## Q16. What are the security best practices for running .NET applications in containers?

**Concepts**
- Non-root USER app to limit container escape blast radius
- Chiseled base images for minimal CVE surface
- Pinned image versions vs floating latest tag
- readOnlyRootFilesystem and capability dropping in Kubernetes securityContext

**Answer**

The most important practice is running as a non-root user. Linux containers run as root by default, so a process that escapes the container sandbox while running as root can cause significant damage. The official .NET images include a pre-created `app` user — add `USER app` in the final stage after copying files and before `ENTRYPOINT`. Chiseled Ubuntu images remove the shell, package manager, and hundreds of other utilities that are not needed at runtime, dramatically reducing the CVE surface because fewer installed packages means fewer vulnerable components. Pin image versions explicitly — using `mcr.microsoft.com/dotnet/aspnet:latest` means your image silently changes when Microsoft updates the tag; pin to `aspnet:10.0` or to a digest for maximum reproducibility. Use `.dockerignore` to exclude `bin/`, `obj/`, `.git/`, `.env`, and any credentials files from the build context so sensitive files never end up in the image. Scan images for vulnerabilities using `docker scout cves`, Trivy, or Snyk as part of the CI pipeline to catch known CVEs in base images before deployment. In Kubernetes, use the pod's `securityContext` to set `readOnlyRootFilesystem: true`, drop Linux capabilities you do not need, and set `allowPrivilegeEscalation: false`. Never run in `Development` mode in production since `ASPNETCORE_ENVIRONMENT=Development` enables detailed error pages that leak stack traces and internal paths.

---

## Q17. What is the difference between framework-dependent, self-contained, and Native AOT publishing for Docker, and which is preferred?

**Concepts**
- Framework-dependent as default — smallest output, shared runtime layer in registry
- Self-contained as runtime-bundled — larger output but no base image runtime dependency
- Native AOT for fastest cold start and smallest final image
- dotnet/runtime-deps base image for self-contained and Native AOT

**Answer**

Framework-dependent publishing (`dotnet publish -c Release`) produces output containing only the app's assemblies, not the .NET runtime, so the container image uses `dotnet/aspnet` or `dotnet/runtime` as a base which provides the shared runtime. This is the standard approach: the publish output is smallest, and multiple services can share the runtime layer in the container registry since it is pulled once and cached. Self-contained publishing (`dotnet publish -c Release --self-contained`) bundles the .NET runtime with the app, so the base image can be `dotnet/runtime-deps`, which contains only native OS dependencies and no .NET. This is useful when you need a base image with no pre-installed runtime, but it produces slower builds and larger images. Native AOT publishing (`dotnet publish -c Release -p:PublishAot=true`) compiles the entire application to a native binary at build time with no JIT and no CLR startup, producing the fastest cold start and the smallest final image when combined with trimming — making it worth the investment for serverless workloads or startup-sensitive services. The limitations of Native AOT are real: no `Assembly.Load` at runtime, limited dynamic reflection, and no `Reflection.Emit` support. For most ASP.NET Core services, framework-dependent with the `aspnet` base image is the correct and default choice.

---

## Q18. How do you debug a containerized ASP.NET Core application?

**Concepts**
- Visual Studio Docker launch profile with automatic vsdbg attachment
- VS Code Docker extension for manual debugger attach
- docker logs for stdout/stderr access without entering the container
- OpenTelemetry and structured logging for production-appropriate diagnostics

**Answer**

Visual Studio on Windows and Mac supports Docker launch profiles: when you add Docker support to a project, Visual Studio generates a Docker launch profile that builds the image, starts the container, and attaches the `vsdbg` debugger automatically when you press F5. Breakpoints work exactly as in local development. In VS Code, the Docker extension provides an "Attach to .NET" flow where you select the running container and the extension installs `vsdbg` if needed and attaches the debugger. For manual attachment you can use `docker exec -it <container-id> bash` to enter a shell and then connect VS Code's "Attach to Remote Process" or Visual Studio's "Attach to Process" targeting the container's process. For production-appropriate diagnostics, `docker logs <container>` tails stdout/stderr without entering the container, and centralised logging via Seq, Elastic Stack, Azure Monitor, or Grafana Loki combined with distributed tracing via OpenTelemetry and Jaeger or Zipkin gives you the full picture. Attach-based debugging is used during development and integration testing; production issues are diagnosed through structured logs, traces, and metrics rather than live debugging.

```csharp
// Set minimum log level to Debug when diagnosing container issues
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

---

## Gotchas — Docker & Containerization (Interview Traps)

---

#### Gotcha 1. Using ADD Instead of COPY in Dockerfile

**Concepts**
- ADD auto-extracting tarballs and fetching remote URLs
- COPY as the explicit, predictable alternative
- ADD's hidden behavior causing unexpected image content
- COPY preferred for all local file operations

**Answer**

`ADD` and `COPY` both copy files into the image, but `ADD` has two hidden behaviours: it automatically extracts `.tar`, `.tar.gz`, and similar archives, and it accepts remote URLs as sources. These side effects make `ADD` unpredictable — adding a `.tar.gz` accidentally extracts it, and a URL source creates an implicit network dependency during the build. `COPY` does exactly one thing: copies local files into the image layer with no extraction or fetching. The Docker best-practice guide recommends using `COPY` for all local file operations and reserving `ADD` only for the rare case where automatic tar extraction is genuinely desired, which is almost never the case in a .NET application Dockerfile.

---

#### Gotcha 2. Multi-Stage Build Cache Busted by Copying All Files First

**Concepts**
- COPY . . before dotnet restore invalidating restore cache on every code change
- Layer cache invalidated when any copied file changes
- Copying only the .csproj files first to cache the restore step
- Cache-friendly Dockerfile layer ordering

**Answer**

A `Dockerfile` that copies all source files with `COPY . .` before running `dotnet restore` busts the restore cache on every single source code change — the entire `dotnet restore` step re-runs from scratch even when no `.csproj` or package dependency changed, adding significant build time in CI. The cache-friendly pattern is to copy only the `.csproj` and `.sln` files first, run `dotnet restore`, and then copy the remaining source files: the restore layer is invalidated only when package dependencies change, not when application code changes. This can reduce CI build times from minutes to seconds for frequent commits.

---

#### Gotcha 3. Container Running as Root

**Concepts**
- Default Docker container running as root inside the container
- Root-in-container with elevated host privileges via volume mounts
- USER instruction adding a non-root user in Dockerfile
- Container breakout risk when running as root

**Answer**

By default, processes inside a Docker container run as root (UID 0), which means a container vulnerability that allows code execution has root privileges inside the container — and depending on Docker configuration, root-in-container can interact dangerously with bind-mounted host paths. The fix is a `USER` instruction in the Dockerfile: create a non-root user with `RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app`, then `USER appuser` before the `ENTRYPOINT`. The .NET base images include a non-root `app` user (`USER app`) that you should switch to in the final stage, which is now the recommended practice in Microsoft's official .NET Dockerfiles.

---

#### Gotcha 4. Container Time Zone Mismatched With the Host

**Concepts**
- UTC in container versus local time zone on the host
- Logs showing timestamps in UTC while dashboards expect local time
- TZ environment variable and timezone data package in the container
- UTC as the recommended container standard

**Answer**

Containers run in UTC by default regardless of the host's time zone — if your application formats timestamps using `DateTime.Now` or `TimeZoneInfo.Local`, the output will be UTC in the container while developers expect local time, which causes confusion in logs and scheduled jobs. The recommended practice is to run all containers in UTC and convert time zones only in the presentation layer, treating UTC as the universal storage format. If a specific time zone is genuinely required inside the container (for a scheduled job, for example), install the `tzdata` package in the Dockerfile and set the `TZ` environment variable, or use `TimeZoneInfo.FindSystemTimeZoneById` with an explicit IANA zone name.

---

#### Gotcha 5. No .dockerignore File Causing Large Slow Builds

**Concepts**
- Build context sent to Docker daemon including unnecessary files
- node_modules, bin, obj, .git sent on every build
- .dockerignore filtering the build context before transfer
- Large build context slowing CI builds significantly

**Answer**

Without a `.dockerignore` file, the Docker CLI sends the entire working directory as the build context to the Docker daemon — including `bin/`, `obj/`, `.git/`, `node_modules/`, test results, and local tool caches that can be hundreds of megabytes. This transfer happens on every `docker build` command, dominating build time and consuming CI bandwidth. A `.dockerignore` file at the project root should exclude at minimum: `**/bin`, `**/obj`, `.git`, `.vs`, `node_modules`, `**/*.user`, and `**/TestResults` — mirroring the `.gitignore` pattern for build artifacts.

---

#### Gotcha 6. Image Tag Fixed at `latest` in Production

**Concepts**
- latest tag mutable and not idempotent
- Rollback to "latest" deploying the wrong image version
- Semantic version or commit SHA as immutable image tags
- Pinned image tags enabling reproducible deployments

**Answer**

Tagging production images with `latest` means the tag resolves to a different image every time a new build is pushed — a rollback operation that re-deploys the `latest` tag will deploy the newest image, not the previous version, making rollback semantically meaningless. In production, every image must be tagged with an immutable identifier: a semantic version (`1.4.2`), a git commit SHA, or a pipeline build number. The deployment manifest pins the exact tag, ensuring that running the same manifest twice deploys exactly the same binary. Most container registries support tag immutability policies that prevent overwriting an existing tag to enforce this.

---

#### Gotcha 7. Debug Configuration Build in the Final Image Stage

**Concepts**
- Debug build including debug symbols, slower JIT, and verbose logging
- Release build smaller, faster, and with optimisations enabled
- ARG CONFIGURATION=Release in Dockerfile for default
- docker build --build-arg overriding configuration for CI

**Answer**

A Dockerfile that runs `RUN dotnet build -c Debug` or omits the `-c Release` flag produces a debug-configuration binary in the final image — debug builds are significantly larger (include PDB symbol files), slower to start (skip certain JIT optimisations), and may log sensitive information at Debug level. Production images must always be built with `-c Release`. The common Dockerfile pattern is `RUN dotnet publish -c Release -o /app/publish`, with no `-c` flag defaulting to Debug and being a frequent mistake in hand-written Dockerfiles that omit the flag because it works locally.

---

#### Gotcha 8. Storing Connection Strings and Secrets in ENV Instructions

**Concepts**
- ENV instruction baking secrets into the image layer
- docker inspect revealing environment variable values
- Runtime secret injection via orchestrator, not Dockerfile
- BuildKit secrets mount for build-time secrets

**Answer**

Using `ENV DB_PASSWORD=SuperSecret123` in a Dockerfile bakes the secret into every image layer permanently — `docker history` and `docker inspect` can reveal environment variable values from any image, including shared registry copies. Secrets must be injected at container runtime by the orchestrator: Kubernetes Secrets mounted as environment variables or volume files, Docker Swarm secrets, or Azure Container Apps secrets. If a secret is needed at build time (a private NuGet feed token), use Docker BuildKit's `--mount=type=secret` which mounts the secret into the build step without writing it to any image layer.

---

#### Gotcha 9. ENTRYPOINT and CMD Confusion Breaking Container Overrides

**Concepts**
- ENTRYPOINT as the fixed executable that cannot be overridden by docker run arguments
- CMD as the default arguments that can be overridden by docker run
- Shell form versus exec form behaviour difference
- docker run <image> <args> replacing CMD, not ENTRYPOINT

**Answer**

A Dockerfile using `ENTRYPOINT ["dotnet", "MyApp.dll"]` means `docker run myimage --help` passes `--help` as an argument to `dotnet MyApp.dll`, which is correct. But using `CMD dotnet MyApp.dll` (shell form) means `docker run myimage --help` replaces the entire CMD with `--help`, which is not an executable, causing an immediate error. The exec form of ENTRYPOINT is the recommended pattern for defining the executable, with CMD providing overridable default arguments: `ENTRYPOINT ["dotnet", "MyApp.dll"]` and optionally `CMD []`. Using the shell form (`ENTRYPOINT dotnet MyApp.dll`) also wraps the command in `/bin/sh -c`, which intercepts SIGTERM and prevents graceful shutdown.

---

#### Gotcha 10. Not Specifying a HEALTHCHECK Instruction

**Concepts**
- Docker container healthy check versus Kubernetes readiness probe
- Standalone container in Docker Compose showing "Up" but actually unhealthy
- HEALTHCHECK instruction providing container-level health status
- docker inspect showing health status from HEALTHCHECK

**Answer**

A Docker container without a `HEALTHCHECK` instruction shows as "Up" in `docker ps` and Docker Compose as long as the process is running, even if the application inside has deadlocked, lost its database connection, or is returning 500 errors on every request. Adding `HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD curl -f http://localhost:8080/health || exit 1` provides a container-level health status visible in `docker inspect` and used by Docker Compose's `depends_on: condition: service_healthy` to delay dependent containers from starting. In Kubernetes, liveness and readiness probes serve the same purpose at the orchestration level, but `HEALTHCHECK` is the correct layer for standalone Docker deployments.

---
