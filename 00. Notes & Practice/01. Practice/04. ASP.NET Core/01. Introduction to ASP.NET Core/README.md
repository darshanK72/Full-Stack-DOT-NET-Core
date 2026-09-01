# Practice Bank — Introduction to ASP.NET Core

This folder contains hands-on exercises derived from the **Introduction to ASP.NET Core** reading module.

## Contents

| File | Purpose |
|------|---------|
| [01. Scenarios.md](01.%20Scenarios.md) | Multi-concept scenario questions with an answers section at the end |
| [02. DebugReading.md](02.%20DebugReading.md) | Code snippets containing compile errors or logic bugs to diagnose and fix |
| [03. COVERAGE.md](03.%20COVERAGE.md) | Coverage matrix — which subtopics are exercised by which activities |
| [IntroductionToASPNETCore.sln](IntroductionToASPNETCore.sln) | Solution file that references all four coding problems |

## Coding Problems

| Folder | Concept Focus |
|--------|--------------|
| [Problems/MinimalWebApp](Problems/MinimalWebApp/PROBLEM.md) | Unified hosting model, Minimal API registration, environment configuration |
| [Problems/RequestPipelineDemo](Problems/RequestPipelineDemo/PROBLEM.md) | Custom middleware, pipeline ordering, request/response inspection |
| [Problems/HealthCheckEndpoint](Problems/HealthCheckEndpoint/PROBLEM.md) | Health check endpoint, ASPNETCORE_ENVIRONMENT, conditional middleware |
| [Problems/FrameworkMigrationReport](Problems/FrameworkMigrationReport/PROBLEM.md) | Migration from .NET Framework patterns — HttpContext.Current, ApiController, static data access |

## Scaffold Workflow

1. Open `IntroductionToASPNETCore.sln` in Visual Studio or run `dotnet build` at this folder.
2. Pick a problem folder and read `PROBLEM.md` fully before opening `Program.cs`.
3. Implement each `// TODO:` in `Program.cs` without removing the comment — it serves as a checklist.
4. When finished, compare your implementation against `EVALUATION.md` to self-score.
5. For scenarios and debug exercises, write your answers on paper or in a scratch file first, then check the `## Answers` section.
