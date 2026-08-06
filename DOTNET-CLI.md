# 00. .NET CLI — Complete Command Reference

The **.NET CLI** (`dotnet`) is a cross-platform tool for creating, building, running, testing, and publishing .NET applications from the terminal.

Use this file as a reading-based reference — no project setup required.

---

## Table of Contents

1. [Check Installation](#1-check-installation)
2. [Create Projects & Solutions](#2-create-projects--solutions)
3. [Manage Solutions](#3-manage-solutions)
4. [Build, Run & Watch](#4-build-run--watch)
5. [Restore & NuGet Packages](#5-restore--nuget-packages)
6. [Project References](#6-project-references)
7. [Testing](#7-testing)
8. [Publish & Deploy](#8-publish--deploy)
9. [Clean & Format](#9-clean--format)
10. [Global & Local Tools](#10-global--local-tools)
11. [Workloads](#11-workloads)
12. [Common Workflows](#12-common-workflows)
13. [Quick Cheat Sheet](#13-quick-cheat-sheet)

---

## 1. Check Installation

```bash
dotnet --version          # installed SDK version (e.g. 8.0.100)
dotnet --info             # SDK, runtimes, host, OS details
dotnet --list-sdks        # all installed SDKs
dotnet --list-runtimes    # all installed runtimes
dotnet --help             # list all top-level commands
dotnet build --help       # help for a specific command
```

---

## 2. Create Projects & Solutions

### List available templates

```bash
dotnet new list                    # all project templates
dotnet new list --language C#      # filter by language
dotnet new console --help          # options for a specific template
```

### Create a new project

```bash
dotnet new console -n MyApp -o MyApp          # console app
dotnet new classlib -n MyLib -o MyLib        # class library
dotnet new web -n MyWeb -o MyWeb              # ASP.NET Core (empty)
dotnet new webapi -n MyApi -o MyApi            # Web API
dotnet new mvc -n MyMvc -o MyMvc                # MVC app
dotnet new blazorserver -n MyBlazor -o MyBlazor  # Blazor Server
dotnet new xunit -n MyTests -o MyTests          # xUnit test project
```

| Flag | Meaning |
|------|---------|
| `-n` / `--name` | Project name |
| `-o` / `--output` | Output folder |
| `-f` / `--framework` | Target framework (e.g. `net8.0`) |
| `--force` | Overwrite existing files |

### Specify framework

```bash
dotnet new console -n MyApp -f net8.0
```

### Create a solution file

```bash
dotnet new sln -n MySolution
dotnet new sln                    # creates Solution.sln in current folder
```

---

## 3. Manage Solutions

```bash
dotnet sln add MyApp/MyApp.csproj              # add project to solution
dotnet sln add src/App/App.csproj src/Lib/Lib.csproj   # add multiple
dotnet sln remove MyApp/MyApp.csproj           # remove project
dotnet sln list                                # list projects in solution
```

**Typical setup:**

```bash
dotnet new sln -n MySolution
dotnet new console -n MyApp -o MyApp
dotnet new classlib -n MyLib -o MyLib
dotnet sln add MyApp/MyApp.csproj MyLib/MyLib.csproj
dotnet add MyApp/MyApp.csproj reference MyLib/MyLib.csproj
```

---

## 4. Build, Run & Watch

### Build

```bash
dotnet build                           # build project/solution in current dir
dotnet build MyApp/MyApp.csproj        # build specific project
dotnet build --configuration Release   # Release build (default is Debug)
dotnet build --no-restore              # skip restore step
dotnet build -v detailed               # verbose output
```

| Configuration | Output folder | Optimized |
|---------------|---------------|-----------|
| `Debug` (default) | `bin/Debug/net8.0/` | No |
| `Release` | `bin/Release/net8.0/` | Yes |

### Run

```bash
dotnet run                             # build (if needed) and run
dotnet run --project MyApp/MyApp.csproj
dotnet run --configuration Release
dotnet run -- arg1 arg2                # pass arguments to your program
```

### Watch (auto-rebuild on file change)

```bash
dotnet watch run                       # run with hot reload on save
dotnet watch build                     # rebuild on save
dotnet watch test                      # re-run tests on save
```

---

## 5. Restore & NuGet Packages

`dotnet restore` downloads NuGet packages defined in `.csproj` files. It runs automatically before `build` and `run`.

```bash
dotnet restore                         # restore packages for current project/solution
dotnet restore MySolution.sln
```

### Add / remove packages

```bash
dotnet add package Newtonsoft.Json                    # latest version
dotnet add package Newtonsoft.Json --version 13.0.3   # specific version
dotnet add MyApp/MyApp.csproj package Serilog

dotnet remove package Newtonsoft.Json
dotnet remove MyApp/MyApp.csproj package Serilog
```

### List packages

```bash
dotnet list package                    # packages in current project
dotnet list package --outdated         # show packages with newer versions
dotnet list package --vulnerable       # show packages with known vulnerabilities
dotnet list package --include-transitive   # include indirect dependencies
```

### NuGet sources

```bash
dotnet nuget list source               # list configured feeds
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```

---

## 6. Project References

Link projects together (e.g. app references a library):

```bash
dotnet add reference ../MyLib/MyLib.csproj
dotnet add MyApp/MyApp.csproj reference MyLib/MyLib.csproj

dotnet remove reference ../MyLib/MyLib.csproj
dotnet list reference                  # list project references
```

---

## 7. Testing

```bash
dotnet test                            # build and run all tests
dotnet test MySolution.sln
dotnet test --filter "FullyQualifiedName~MyClass"   # run specific tests
dotnet test --logger "console;verbosity=detailed"   # verbose output
dotnet test --collect:"XPlat Code Coverage"         # code coverage
```

### Create a test project and link it

```bash
dotnet new xunit -n MyApp.Tests -o MyApp.Tests
dotnet sln add MyApp.Tests/MyApp.Tests.csproj
dotnet add MyApp.Tests/MyApp.Tests.csproj reference MyApp/MyApp.csproj
dotnet add MyApp.Tests/MyApp.Tests.csproj package Microsoft.NET.Test.Sdk
```

---

## 8. Publish & Deploy

Creates a deployable output (self-contained or framework-dependent):

```bash
dotnet publish                                    # publish to bin/Release/net8.0/publish/
dotnet publish -c Release                         # Release configuration
dotnet publish -o ./publish                       # custom output folder
dotnet publish --self-contained true              # include runtime in output
dotnet publish -r win-x64 --self-contained true   # target Windows x64
dotnet publish -r linux-x64 --self-contained true # target Linux x64
dotnet publish -p:PublishSingleFile=true          # single executable file
```

| Runtime Identifier (RID) | Platform |
|--------------------------|----------|
| `win-x64` | Windows 64-bit |
| `win-x86` | Windows 32-bit |
| `linux-x64` | Linux 64-bit |
| `osx-x64` | macOS Intel |
| `osx-arm64` | macOS Apple Silicon |

---

## 9. Clean & Format

```bash
dotnet clean                           # delete bin/ and obj/ folders
dotnet clean --configuration Release

dotnet format                          # apply code style fixes
dotnet format --verify-no-changes      # check formatting (CI pipelines)
dotnet format whitespace               # fix whitespace only
```

---

## 10. Global & Local Tools

.NET tools are NuGet packages installed as CLI commands (e.g. `dotnet-ef`, `dotnet-outdated`).

### Global tools (available everywhere)

```bash
dotnet tool install -g dotnet-ef                  # install globally
dotnet tool update -g dotnet-ef                 # update global tool
dotnet tool uninstall -g dotnet-ef              # uninstall
dotnet tool list -g                             # list global tools
```

### Local tools (scoped to a project via manifest)

```bash
dotnet new tool-manifest                        # create .config/dotnet-tools.json
dotnet tool install dotnet-ef                   # install locally
dotnet tool run dotnet-ef migrations add Init   # run local tool
dotnet tool restore                             # restore tools from manifest
dotnet tool list                                # list local tools
```

---

## 11. Workloads

Workloads add optional SDK features (mobile, WASM, etc.):

```bash
dotnet workload list                    # installed workloads
dotnet workload search <keyword>        # search available workloads
dotnet workload install wasm-tools      # install a workload
dotnet workload update                  # update all workloads
dotnet workload uninstall wasm-tools    # remove a workload
```

---

## 12. Common Workflows

### Start a new console app from scratch

```bash
mkdir MyApp && cd MyApp
dotnet new console -n MyApp
dotnet run
```

### Start a solution with app + library + tests

```bash
dotnet new sln -n MySolution
dotnet new console -n MyApp -o src/MyApp
dotnet new classlib -n MyLib -o src/MyLib
dotnet new xunit -n MyApp.Tests -o tests/MyApp.Tests

dotnet sln add src/MyApp/MyApp.csproj src/MyLib/MyLib.csproj tests/MyApp.Tests/MyApp.Tests.csproj
dotnet add src/MyApp/MyApp.csproj reference src/MyLib/MyLib.csproj
dotnet add tests/MyApp.Tests/MyApp.Tests.csproj reference src/MyApp/MyApp.csproj

dotnet build
dotnet test
dotnet run --project src/MyApp/MyApp.csproj
```

### Add a third-party package and run

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet restore
dotnet run
```

### Build for production deployment

```bash
dotnet publish -c Release -o ./publish
```

---

## 13. Quick Cheat Sheet

| Task | Command |
|------|---------|
| Check SDK version | `dotnet --version` |
| Create console app | `dotnet new console -n App -o App` |
| Create solution | `dotnet new sln -n Solution` |
| Add project to solution | `dotnet sln add App/App.csproj` |
| Add project reference | `dotnet add reference ../Lib/Lib.csproj` |
| Add NuGet package | `dotnet add package PackageName` |
| Restore packages | `dotnet restore` |
| Build | `dotnet build` |
| Run | `dotnet run` |
| Watch (auto rerun) | `dotnet watch run` |
| Run tests | `dotnet test` |
| Publish | `dotnet publish -c Release` |
| Clean build output | `dotnet clean` |
| List templates | `dotnet new list` |
| Format code | `dotnet format` |
| Install global tool | `dotnet tool install -g <tool>` |
| Help | `dotnet <command> --help` |

---

## Useful Tips

- Run commands from the folder containing the `.csproj` or `.sln` file, or pass `--project` / path explicitly.
- `dotnet run` implicitly calls `dotnet build` if the output is outdated.
- Use `dotnet watch run` during development — it rebuilds and restarts when you save a file.
- Project and package changes go in the `.csproj` file; the CLI commands above edit that file for you.
- For full official docs: [https://learn.microsoft.com/dotnet/core/tools/](https://learn.microsoft.com/dotnet/core/tools/)
