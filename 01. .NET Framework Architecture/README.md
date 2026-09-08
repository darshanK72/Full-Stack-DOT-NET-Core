# .NET Framework Architecture — Interview Q&A
> 35 questions · 10 gotchas · 6 scenarios · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is .NET? What is .NET Framework? What is .NET Core?](#q1-what-is-net-what-is-net-framework-what-is-net-core)
2. [Q2. How did .NET evolve from .NET Framework → .NET Core → unified .NET? What are the core differences, and when would you still choose .NET Framework 4.8 today?](#q2-how-did-net-evolve-from-net-framework-net-core-unified-net-what-are-the-core-differences-and-when-would-you-still-choose-net-framework-48-today)
3. [Q3. Compare .NET Framework and modern .NET across every major dimension — deployment, platform, web stack, desktop, security, hosting, configuration, and package management.](#q3-compare-net-framework-and-modern-net-across-every-major-dimension-deployment-platform-web-stack-desktop-security-hosting-configuration-and-package-management)
4. [Q4. What are the different components of old .NET Framework?](#q4-what-are-the-different-components-of-old-net-framework)
5. [Q5. What are the different components of modern .NET Core / unified .NET?](#q5-what-are-the-different-components-of-modern-net-core-unified-net)
6. [Q6. What are the different application frameworks available in old .NET Framework?](#q6-what-are-the-different-application-frameworks-available-in-old-net-framework)
7. [Q7. What are the different application frameworks available in modern .NET?](#q7-what-are-the-different-application-frameworks-available-in-modern-net)
8. [Q8. What is .NET Standard, why was it introduced, and is it still the right choice for new shared libraries? How does it compare to multi-targeting like `net8.0;net481`?](#q8-what-is-net-standard-why-was-it-introduced-and-is-it-still-the-right-choice-for-new-shared-libraries-how-does-it-compare-to-multi-targeting-like-net80net481)
9. [Q9. What is the LTS vs STS release cadence in modern .NET, and which version is currently LTS?](#q9-what-is-the-lts-vs-sts-release-cadence-in-modern-net-and-which-version-is-currently-lts)
10. [Q10. What is the difference between framework-dependent, self-contained, and Native AOT publishing?](#q10-what-is-the-difference-between-framework-dependent-self-contained-and-native-aot-publishing)
11. [Q11. What are the four layers of the .NET platform, and what is the difference between the Runtime and the SDK?](#q11-what-are-the-four-layers-of-the-net-platform-and-what-is-the-difference-between-the-runtime-and-the-sdk)
12. [Q12. What roles do Roslyn, RyuJIT, MSBuild, and the `dotnet` CLI play in building and running a .NET app?](#q12-what-roles-do-roslyn-ryujit-msbuild-and-the-dotnet-cli-play-in-building-and-running-a-net-app)
13. [Q13. What is MSBuild, how does it evaluate and execute a build, and what are properties, items, targets, tasks, and SDK-style projects?](#q13-what-is-msbuild-how-does-it-evaluate-and-execute-a-build-and-what-are-properties-items-targets-tasks-and-sdk-style-projects)
14. [Q14. What is Roslyn, how does its compilation pipeline work, and what are analyzers and source generators?](#q14-what-is-roslyn-how-does-its-compilation-pipeline-work-and-what-are-analyzers-and-source-generators)
15. [Q15. What is the CLR and what are its core responsibilities?](#q15-what-is-the-clr-and-what-are-its-core-responsibilities)
16. [Q16. Walk me through the end-to-end execution of a .NET app — from source code to CPU instructions.](#q16-walk-me-through-the-end-to-end-execution-of-a-net-app-from-source-code-to-cpu-instructions)
17. [Q17. What is tiered JIT (Tier 0 / Tier 1)? How does RyuJIT balance startup speed vs throughput?](#q17-what-is-tiered-jit-tier-0-tier-1-how-does-ryujit-balance-startup-speed-vs-throughput)
18. [Q18. What is the difference between ReadyToRun (R2R) and Native AOT? When would you pick each?](#q18-what-is-the-difference-between-readytorun-r2r-and-native-aot-when-would-you-pick-each)
19. [Q19. What is IL/CIL and why do compilers target it instead of native code? What does "stack-based VM" mean?](#q19-what-is-ilcil-and-why-do-compilers-target-it-instead-of-native-code-what-does-stack-based-vm-mean)
20. [Q20. What is metadata in a .NET assembly, and why do reflection, DI containers, and serializers depend on it?](#q20-what-is-metadata-in-a-net-assembly-and-why-do-reflection-di-containers-and-serializers-depend-on-it)
21. [Q21. What is IL verification, when can it be skipped, and what replaced Code Access Security (CAS) in modern .NET?](#q21-what-is-il-verification-when-can-it-be-skipped-and-what-replaced-code-access-security-cas-in-modern-net)
22. [Q22. What are the CTS and CLS, what is the difference between them, and why do they enable cross-language interop?](#q22-what-are-the-cts-and-cls-what-is-the-difference-between-them-and-why-do-they-enable-cross-language-interop)
23. [Q23. What is boxing and unboxing? What is the performance cost, and when does it happen implicitly?](#q23-what-is-boxing-and-unboxing-what-is-the-performance-cost-and-when-does-it-happen-implicitly)
24. [Q24. What is the difference between managed and unmanaged code? What services does managed code get from the CLR?](#q24-what-is-the-difference-between-managed-and-unmanaged-code-what-services-does-managed-code-get-from-the-clr)
25. [Q25. What is P/Invoke? How does managed code call a native DLL, and what is marshalling?](#q25-what-is-pinvoke-how-does-managed-code-call-a-native-dll-and-what-is-marshalling)
26. [Q26. What is a .NET assembly? What does it contain, and how is it different from a namespace or a native DLL?](#q26-what-is-a-net-assembly-what-does-it-contain-and-how-is-it-different-from-a-namespace-or-a-native-dll)
27. [Q27. What is the difference between a DLL and an EXE in .NET?](#q27-what-is-the-difference-between-a-dll-and-an-exe-in-net)
28. [Q28. What is strong naming? What does a strong name consist of, and is the GAC still used in modern .NET?](#q28-what-is-strong-naming-what-does-a-strong-name-consist-of-and-is-the-gac-still-used-in-modern-net)
29. [Q29. Explain generational GC — Gen 0, Gen 1, Gen 2. What is the weak generational hypothesis and what triggers a collection?](#q29-explain-generational-gc-gen-0-gen-1-gen-2-what-is-the-weak-generational-hypothesis-and-what-triggers-a-collection)
30. [Q30. Is it always true that value types live on the stack and reference types on the heap? Where does this break?](#q30-is-it-always-true-that-value-types-live-on-the-stack-and-reference-types-on-the-heap-where-does-this-break)
31. [Q31. What is the Large Object Heap (LOH)? How does it differ from the small-object heap, and what size triggers it?](#q31-what-is-the-large-object-heap-loh-how-does-it-differ-from-the-small-object-heap-and-what-size-triggers-it)
32. [Q32. What is the difference between `IDisposable.Dispose` and a finalizer? Explain the `Dispose(bool disposing)` pattern.](#q32-what-is-the-difference-between-idisposabledispose-and-a-finalizer-explain-the-disposebool-disposing-pattern)
33. [Q33. What does a memory leak look like in managed code despite having a GC? What causes GC pressure?](#q33-what-does-a-memory-leak-look-like-in-managed-code-despite-having-a-gc-what-causes-gc-pressure)
34. [Q34. What were AppDomains, why were they removed from .NET Core, and what are the modern alternatives?](#q34-what-were-appdomains-why-were-they-removed-from-net-core-and-what-are-the-modern-alternatives)
35. [Q35. What are the main motivations for migrating from .NET Framework to modern .NET? What is the strangler fig pattern, and what are the most common breaking changes to watch for?](#q35-what-are-the-main-motivations-for-migrating-from-net-framework-to-modern-net-what-is-the-strangler-fig-pattern-and-what-are-the-most-common-breaking-changes-to-watch-for)

---

## Q1. What is .NET? What is .NET Framework? What is .NET Core?

**Concepts**
- .NET as platform — runtime, BCL, tooling, and application frameworks combined
- .NET Framework — Windows-only, monolithic install, frozen at version 4.8
- .NET Core — cross-platform rewrite from scratch (2016), modular, container-friendly
- Unified .NET (v5+) — continuation of .NET Core; no parallel Framework development

**Answer**

The three names describe different eras of the same ecosystem. .NET is the umbrella term for the full platform — the runtime that executes code, the Base Class Library, build tooling, and app frameworks like ASP.NET Core. It covers web, desktop, mobile, cloud, and CLI apps. .NET Framework is the original implementation launched in 2002, which runs only on Windows and is tightly coupled to IIS and Win32. It is still supported but frozen at version 4.8 with no new features. .NET Core was a complete rewrite started in 2016 to make the platform cross-platform — it runs on Windows, Linux, and macOS, uses modular NuGet packages instead of one monolithic install, and runs outside IIS via Kestrel. .NET Core ended at version 3.1, after which the platform was unified and simply called ".NET" (versions 5, 6, 7, 8, 9, 10...). Today's unified .NET is a direct continuation of .NET Core, not .NET Framework.

---

## Q2. How did .NET evolve from .NET Framework → .NET Core → unified .NET? What are the core differences, and when would you still choose .NET Framework 4.8 today?

**Concepts**
- .NET Framework (2002) — Windows-only, machine-wide install, IIS-coupled, monolithic
- .NET Core (2016) — complete cross-platform rewrite; side-by-side versioning; Kestrel
- Unified .NET (5+, 2020) — version jumped 3.1 → 5 to avoid confusion with Framework 4.x
- Framework 4.8 still justified — Web Forms, COM interop, regulated re-certification cost

**Answer**

.NET Framework arrived in 2002 as a Windows-only platform where every app on the server shared the same machine-wide installation. .NET Core was built from scratch in 2016 specifically to support Linux, macOS, and containers — modular NuGet packages replaced the monolithic installer, Kestrel replaced IIS as the default web server, and side-by-side versioning meant two apps on the same host could run against different runtimes without interference. In 2020, Microsoft dropped the "Core" label since there was no longer any parallel "full Framework" under active development, and .NET 5 became the unified name — the version jumped from 3.1 to 5 to avoid confusion with Framework 4.x. .NET Framework 4.8 is still a reasonable choice when you have Web Forms or WCF with exotic bindings that would require a full rewrite to migrate, COM interop dependencies you cannot replace, or regulated environments where re-certification costs outweigh migration benefits. Framework still receives security patches — it is done, not dead.

---

## Q3. Compare .NET Framework and modern .NET across every major dimension — platform, open source, deployment, containerization, web stack, application frameworks, desktop, tooling, performance, security, configuration, and package management.

**Concepts**
- Platform: Windows-only vs. cross-platform (Windows, Linux, macOS)
- Open source: closed/Microsoft-only vs. fully open source on GitHub with community contributions
- Deployment: machine-wide GAC vs. per-app side-by-side versioning; self-contained and Native AOT options
- Containerization: no Linux container support vs. official Docker images and container-first design
- Web stack: System.Web + IIS-coupled vs. ASP.NET Core + Kestrel, middleware pipeline, Minimal APIs
- Application frameworks: bundled monolithic install vs. modular NuGet opt-in; Blazor, MAUI, gRPC added; Web Forms and WCF not ported
- Tooling: Visual Studio–only MSBuild vs. cross-platform dotnet CLI + Roslyn analyzers + source generators
- Performance: frozen baseline vs. continuous gains — Span\<T\>, tiered JIT, SIMD, Native AOT
- Security model: Code Access Security (CAS, removed) vs. OS/container boundaries + ASP.NET Core policies
- Configuration: web.config XML vs. layered appsettings.json + environment variables + IOptions\<T\>

**Answer**

**Platform and open source.** .NET Framework runs only on Windows because it depends on Win32, COM, and IIS internals. Modern .NET runs on Windows, Linux, and macOS. Equally important is the open source shift: .NET Framework was a closed, Microsoft-controlled codebase; modern .NET (CoreCLR, BCL, ASP.NET Core, EF Core) is fully open source at github.com/dotnet with community PRs, public roadmap, and transparent release process.

**Deployment and containerization.** Framework required a machine-wide installer so all apps on the server shared one runtime version. Modern .NET apps either use side-by-side versioning (each app binds its own runtime version), self-contained publish (runtime bundled in the output folder), or Native AOT (no runtime at all — single native binary). This makes Linux containers practical: official `mcr.microsoft.com/dotnet` images exist for runtime, SDK, and ASP.NET Core, enabling `FROM mcr.microsoft.com/dotnet/aspnet:10.0` multi-stage Docker builds. .NET Framework has no official Linux container support.

**Web stack and application frameworks.** Framework's ASP.NET ran through the IIS HTTP pipeline via `System.Web`, with `HttpContext.Current` as a static ambient object — this tight coupling is why Web Forms and classic MVC/Web API have no port path. ASP.NET Core uses Kestrel with a composable middleware pipeline, DI-injected `IHttpContextAccessor`, and runs standalone without IIS. Modern .NET adds frameworks that never existed in Framework: Blazor (component-based web UI with Server and WebAssembly render modes), .NET MAUI (cross-platform mobile/desktop), gRPC via grpc-dotnet, and Minimal APIs. On the other side, WCF's full server stack and Web Forms are not ported — CoreWCF covers basic HTTP/SOAP only.

**Tooling.** Framework builds required Visual Studio on Windows; the `msbuild.exe` CLI was cumbersome and Windows-only. Modern .NET ships the `dotnet` CLI — cross-platform, scriptable, and the single entry point for `new`, `restore`, `build`, `test`, `publish`, and `run`. Roslyn analyzers and source generators ship as NuGet packages and run in every editor (VS, VS Code, Rider), not just Visual Studio.

**Performance.** Framework 4.8 receives security patches only — no runtime performance improvements. Modern .NET receives continuous investment: `Span<T>` and `Memory<T>` enable zero-copy buffer operations, tiered JIT promotes hot methods to fully optimized native code, hardware intrinsics expose SIMD directly, and Native AOT eliminates JIT startup entirely. ASP.NET Core consistently tops TechEmpower plaintext and JSON benchmarks by orders of magnitude over classic ASP.NET.

**Security, configuration, and packages.** Code Access Security was removed — its stack walks were expensive and bypassable. Security now lives at the OS and container boundary with ASP.NET Core claims-based policies for application-layer authorization. Configuration moved from XML `web.config` (deploy-time only) to a layered provider model: `appsettings.json`, environment variables, Azure Key Vault, and user secrets compose at startup and surface as typed `IOptions<T>`. Package management moved from `packages.config` (local folder copies) to SDK-style `PackageReference` with NuGet transitive resolution. AppDomains were removed — `AssemblyLoadContext` replaces them for plugin/assembly isolation scenarios.

| Area | .NET Framework | Modern .NET |
|---|---|---|
| OS | Windows only | Windows, Linux, macOS |
| Open source | Closed | github.com/dotnet |
| Deployment | Machine-wide GAC | Per-app, side-by-side, self-contained, AOT |
| Containers | No Linux support | Official Docker images, container-first |
| Web | System.Web + IIS | ASP.NET Core + Kestrel + Minimal APIs |
| New frameworks | None | Blazor, MAUI, gRPC, SignalR |
| Missing in modern | — | Web Forms, WCF full server |
| Tooling | VS on Windows | dotnet CLI, cross-platform |
| Performance | Frozen at 4.8 | Continuous — Span\<T\>, tiered JIT, AOT |
| Security | CAS (removed) | OS/container + ASP.NET Core policies |
| Config | web.config (XML) | appsettings.json + env vars + IOptions\<T\> |
| Packages | packages.config | PackageReference + NuGet |
| AppDomains | Yes | No → AssemblyLoadContext |
| Status | Maintenance only | Active, annual LTS/STS releases |

---

## Q4. What are the different components of old .NET Framework?

**Concepts**
- CLR — JIT, GC, type safety, exception handling, thread pool
- BCL/FCL — full System.* namespace tree bundled in the monolithic install
- Monolithic bundling — one Windows installer versioned everything together
- GAC — machine-wide shared assembly store for strongly named assemblies
- Pre-Roslyn compilers and MSBuild; no dotnet CLI

**Answer**

The old .NET Framework shipped everything as a single Windows installer versioned together. The core runtime was the CLR (Common Language Runtime), which handled JIT compilation, garbage collection, type safety enforcement, structured exception handling, and the thread pool. On top of the CLR sat the BCL/FCL — the full System.* namespace tree including strings, I/O, collections, threading, XML, and cryptography.

All application frameworks were bundled into that same installer: ASP.NET Web Forms, MVC, and Web API for the web stack (all coupled to System.Web and IIS); WCF for SOAP and binary RPC with NetTcp, MSMQ, and named-pipe bindings; WPF and WinForms for desktop; Windows Workflow Foundation for business process automation; and the classic Entity Framework ORM. Deployment relied on the GAC (Global Assembly Cache) — a machine-wide store where strongly named assemblies lived so multiple apps could share one physical copy. Because everything shipped together, a WCF bug fix required all apps on the server to accept a new Framework version, which is the monolithic coupling that .NET Core was explicitly designed to eliminate.

---

## Q5. What are the different components of modern .NET Core / unified .NET?

**Concepts**
- CoreCLR — cross-platform JIT (RyuJIT), GC, type system
- Roslyn — C#/VB compiler platform; powers IDE features and source generators
- dotnet CLI — unified cross-platform entry point for build, run, publish, test
- NuGet + PackageReference — replaces GAC and packages.config
- Application frameworks as separate NuGet packages or SDK workloads

**Answer**

Modern .NET is modular rather than monolithic. The core runtime is CoreCLR — the cross-platform CLR implementation containing RyuJIT (the JIT compiler), the generational GC, and the type system. Alongside CoreCLR there is a Native AOT runtime option for apps that need no JIT at all. The compiler layer is Roslyn for C# and VB.NET, which powers IntelliSense, analyzers, and source generators in the IDE alongside code generation. MSBuild handles build orchestration and the dotnet CLI is the unified cross-platform entry point for new, restore, build, run, test, and publish commands.

Application frameworks are no longer bundled: ASP.NET Core (Web API, Razor Pages, Minimal APIs, gRPC, SignalR, Blazor), Worker Services, .NET MAUI, and Entity Framework Core all ship as NuGet packages or optional SDK workloads, so you only pull in what you use. NuGet with PackageReference in SDK-style projects replaces the GAC — each app owns its dependencies in its publish folder. AssemblyLoadContext replaces AppDomains for assembly isolation when plugins or dynamic loading are needed.

---

## Q6. What are the different application frameworks available in old .NET Framework?

**Concepts**
- System.Web coupling — Web Forms, classic MVC/Web API all shared the IIS pipeline
- WCF — full SOAP/binary RPC with NetTcp, MSMQ, named-pipe bindings
- WPF and WinForms — ported to modern .NET; viable migration path
- Windows Workflow Foundation — not ported; Framework-only
- .NET Remoting — completely removed; replaced by gRPC/REST

**Answer**

All old Framework app frameworks shipped bundled in the Windows installer. The web stack — Web Forms, MVC, and Web API — ran on System.Web, which was tightly coupled to the IIS pipeline with ViewState, postbacks, and `HttpContext.Current` as a static ambient context. This coupling is why there is no migration path for Web Forms to modern .NET: the entire pipeline model changed and there is no equivalent abstraction to port to.

WCF (Windows Communication Foundation) provided a full SOAP and binary RPC server stack with bindings for HTTP, NetTcp, MSMQ, and named pipes — only a partial client and a limited CoreWCF subset exist in modern .NET. WPF and WinForms did get ported and work on `net8.0-windows` and later, so existing desktop code has a viable path forward. Windows Workflow Foundation was not ported and remains Framework-only. .NET Remoting was removed entirely — gRPC and REST are the modern replacements for cross-process communication.

| Framework | Status in modern .NET |
|---|---|
| ASP.NET Web Forms | No migration path — rewrite required |
| ASP.NET MVC / Web API | Superseded by ASP.NET Core MVC/Web API |
| WCF (server) | Not ported — use CoreWCF or gRPC |
| WPF / WinForms | Ported — works on net8.0-windows |
| Windows Workflow Foundation | Not ported — Framework-only |
| Entity Framework (classic) | Superseded by EF Core |
| .NET Remoting | Removed — use gRPC/REST |

---

## Q7. What are the different application frameworks available in modern .NET?

**Concepts**
- ASP.NET Core — Kestrel-based, composable middleware, cross-platform
- Blazor — component-based web UI (Server SSR, WebAssembly, Hybrid)
- Worker Services — generic host for long-running background processes
- .NET MAUI — single codebase for Windows, macOS, iOS, Android
- Entity Framework Core — LINQ-based ORM, Code First, multiple DB providers

**Answer**

Modern .NET builds its app frameworks on top of the shared CoreCLR and BCL foundation, shipping them as NuGet packages or SDK workloads so you only pull in what you use. ASP.NET Core is the primary web framework — it uses Kestrel as the built-in cross-platform web server, a composable middleware pipeline, and supports Web API controllers, Razor Pages, Minimal APIs, gRPC services, and SignalR hubs from one codebase. Blazor is the component-based UI system with three render modes: Server (SSR with SignalR), WebAssembly (code runs in the browser), and Hybrid (code runs in a native app shell).

Worker Services use the generic host for long-running background processes — ideal for message consumers, schedulers, and container workloads. .NET MAUI provides a single codebase targeting Windows, macOS, iOS, and Android. WPF and WinForms remain available for Windows-specific desktop apps on the Windows TFM. Entity Framework Core is the modern ORM — LINQ-based, Code First migrations, multiple database providers — and ships independently of the runtime. The shift from Framework is that every framework is an opt-in package rather than a bundled install, so a minimal API project has substantially less overhead than an app that pulls in every workload.

---

## Q8. What is .NET Standard, why was it introduced, and is it still the right choice for new shared libraries? How does it compare to multi-targeting like `net8.0;net481`?

**Concepts**
- .NET Standard — an API contract/spec, not a runtime; guarantees portability across implementations
- netstandard2.0 — the bridge for Framework 4.6.1+ and .NET Core 2.0+ compatibility
- Multi-targeting (net481;net10.0) — modern alternative; separate compilation per TFM
- .NET Standard 2.1 — never implemented by .NET Framework 4.8; not a true bridge

**Answer**

.NET Standard is not a runtime you install; it is a specification — a contract listing which APIs any conforming .NET implementation must provide. Targeting `netstandard2.0` tells the compiler to restrict your code to that API surface, guaranteeing the library will run on anything that implements the spec: .NET Framework 4.6.1+, .NET Core 2.0+, and Mono. This was the bridge needed during the Framework-to-Core transition when you wanted one class library that worked across both.

For new libraries today, .NET Standard is not recommended. If you have no .NET Framework users, target `net10.0` directly and use all modern APIs without compromise. If you must support both Framework 4.x and modern .NET, multi-target with `<TargetFrameworks>net481;net10.0</TargetFrameworks>` — you get a separate compilation path for each and can use modern APIs in the modern path while maintaining Framework compatibility in the other. Existing .NET Standard 2.0 libraries that work can stay as-is, but there is no good reason to start new libraries on .NET Standard. One important gotcha: .NET Standard 2.1 was never implemented by .NET Framework 4.8, so it never served as a real cross-runtime bridge — it only targeted Core and Mono.

---

## Q9. What is the LTS vs STS release cadence in modern .NET, and which version is currently LTS?

**Concepts**
- Annual November release cadence
- LTS (even-numbered) — 3 years support; the production-grade choice
- STS (odd-numbered) — ~18 months support; new features sooner, faster upgrade cycle
- .NET 10 — current LTS (November 2025, TFM net10.0, supported until November 2028)

**Answer**

Modern .NET releases every November. Even-numbered versions are LTS (Long-Term Support), receiving 3 years of support — these are the right choice for production workloads where you want a stable upgrade cadence. Odd-numbered versions are STS (Standard Term Support) with around 18 months of support — they deliver new features sooner but require faster upgrade cycles since support ends before the next LTS ships. The current LTS is .NET 10, released November 2025, supported until November 2028, with TFM `net10.0`. .NET Framework 4.8 sits outside this cadence entirely in maintenance mode, receiving security fixes but no new APIs or performance improvements.

---

## Q10. What is the difference between framework-dependent, self-contained, and Native AOT publishing?

**Concepts**
- Framework-dependent — deploys app DLLs only; matching runtime must be installed on the host
- Self-contained — bundles the runtime; no pre-installed runtime required; large output
- Native AOT — all IL compiled to native at build time; no JIT, no CLR startup; fastest cold start
- Native AOT trade-off — no runtime code generation; limited reflection

**Answer**

The three publishing modes represent different trade-offs between deployment simplicity, startup speed, and compatibility. Framework-dependent publishing outputs only the app's DLLs — the host machine must have the matching .NET runtime installed, which keeps the output small and fast to deploy. This is the standard choice for Docker images that use a .NET base image since the runtime is already in the base layer.

Self-contained publishing bundles the entire .NET runtime alongside the app, so the target machine needs nothing pre-installed. The output is large — potentially hundreds of megabytes — but the app is fully portable to bare machines, which makes it the right choice for CLI tools distributed to end users.

Native AOT compiles the entire application to native machine code at build time, producing no IL and requiring no CLR JIT at runtime. The result is the fastest possible cold start, the smallest binary (especially with trimming), and a self-contained native executable. The trade-off is that runtime code generation is impossible: `Assembly.LoadFrom`, `Reflection.Emit`, and unbounded dynamic reflection do not work. Native AOT is best for serverless functions where cold start latency matters, high-performance CLIs, and lean microservices.

| | Framework-dependent | Self-contained | Native AOT |
|---|---|---|---|
| Runtime needed | Yes (shared) | No (bundled) | No |
| Output size | Small | Large | Small (trimmed) |
| Cold start | Fast | Fast | Fastest |
| Reflection support | Full | Full | Limited |

---

## Q11. What are the four layers of the .NET platform, and what is the difference between the Runtime and the SDK?

**Concepts**
- Runtime (CoreCLR) — GC, JIT, type safety, threading; runs on production servers
- BCL — standard library: System.Collections, System.IO, System.Threading, etc.
- SDK and toolchain — Roslyn, MSBuild, dotnet CLI; build-time only
- App frameworks — ASP.NET Core, MAUI, Blazor, WPF built on the lower layers

**Answer**

The .NET platform stacks into four layers. At the bottom is the runtime (CoreCLR), which handles JIT compilation via RyuJIT, garbage collection, type safety enforcement, threading infrastructure, and exception handling — this is what production servers need to run apps. On top of the runtime sits the BCL (Base Class Library): the standard `System.*` namespaces for collections, I/O, threading, JSON, networking, and more. Above the BCL sits the SDK and toolchain layer — Roslyn for compilation, MSBuild for build orchestration, and the dotnet CLI as the unified developer entry point. Finally, application frameworks (ASP.NET Core, MAUI, Blazor, WPF, WinForms, EF Core) build on everything below.

The practical implication is in what you install: production servers need only the runtime package (`dotnet-runtime`), which is smaller and has a narrower attack surface. Developer machines and CI servers need the SDK (`dotnet-sdk`), which includes the runtime plus all build tooling. Running `dotnet MyApp.dll` requires only the runtime; `dotnet build` requires the SDK.

---

## Q12. What roles do Roslyn, RyuJIT, MSBuild, and the `dotnet` CLI play in building and running a .NET app?

**Concepts**
- Roslyn (build time) — C#/VB source → IL + metadata in .dll assemblies
- MSBuild (build time) — orchestrates full build: NuGet restore, Roslyn invocation, bin/ output
- dotnet CLI (build + run) — front door wrapping MSBuild for builds and CoreCLR for running
- RyuJIT (run time, inside CoreCLR) — IL → native machine instructions on first method call

**Answer**

These four tools sit at different points in the pipeline: Roslyn and MSBuild operate at build time, RyuJIT operates at runtime, and the dotnet CLI bridges both. Roslyn is the C# (and VB.NET) compiler platform — it translates `.cs` source files into IL bytecode plus type metadata and packages them into `.dll` assemblies. It also powers IntelliSense, analyzers, and source generators in the IDE. MSBuild is the build orchestrator: it reads the `.csproj`, invokes Roslyn via the `CoreCompile` target, runs NuGet restore, copies content files, and produces output under `bin/` and `obj/`. The dotnet CLI is what developers actually type — `dotnet build` calls MSBuild under the hood, `dotnet run` builds then launches, and `dotnet MyApp.dll` starts the host which loads CoreCLR.

RyuJIT is the JIT compiler embedded inside CoreCLR and runs entirely at runtime — it never sees your `.cs` files. When a method is called for the first time, RyuJIT translates that method's IL into native x64 or ARM64 machine instructions and caches the result. Subsequent calls jump directly to the cached native code. With tiered compilation, hot methods are recompiled as Tier 1 with full optimization in the background. The end-to-end flow: source → Roslyn → IL → MSBuild packages it → `dotnet` launches the host → CoreCLR loads assemblies → RyuJIT compiles IL to native CPU instructions.

---

## Q13. What is MSBuild, how does it evaluate and execute a build, and what are properties, items, targets, tasks, and SDK-style projects?

**Concepts**
- MSBuild — XML build engine; two-phase: evaluation resolves all properties/items, then execution runs targets in DAG order
- Properties (`$(Name)`) / Items (`@(Name)`) — scalar vs. collection values; `Condition` attribute applies to any element
- SDK-style .csproj — `Sdk="Microsoft.NET.Sdk"` implicitly imports compile globs, NuGet restore, publish pipeline; `PackageReference` for dependencies
- Target — named ordered list of tasks; Task = atomic operation (Csc, Copy, Exec, Message)
- Ordering — `DependsOnTargets`, `BeforeTargets`, `AfterTargets`; extend without editing SDK targets; `Directory.Build.props` for repo-wide settings

**Answer**

MSBuild (Microsoft Build Engine) is the build orchestration system for all .NET projects. It reads `.csproj`, `.vbproj`, and `.fsproj` files — which are MSBuild XML — resolves all imported `.props` and `.targets` files, and executes a directed acyclic graph of targets in dependency order. `dotnet build` is a thin CLI wrapper over MSBuild; any MSBuild property can be passed through: `dotnet build -p:Configuration=Release`.

The build has two distinct phases. The **evaluation phase** parses all imported files, evaluates PropertyGroup and ItemGroup elements, and resolves the final property and item values before any task runs. **Properties** are named scalar strings accessed as `$(Name)` — later declarations override earlier ones, and command-line `-p:Name=Value` always wins. **Items** are named collections (typically file sets) accessed as `@(Name)` and support wildcards, `Remove`, and per-item metadata accessed as `%(FileName)`, `%(Extension)`, etc. The `Condition` attribute applies to any element, making it active only when the expression is true. The **execution phase** then runs targets in resolved dependency order.

The **SDK-style .csproj** reduces boilerplate dramatically. The `Sdk="Microsoft.NET.Sdk"` attribute implicitly imports `Sdk.props` at the very top and `Sdk.targets` at the very bottom, bringing in the default compile glob (`**\*.cs`), NuGet restore targets, the publish pipeline, and hundreds of default properties. A minimal working project needs only a `<TargetFramework>` declaration. `PackageReference` replaces `packages.config` — NuGet resolves the full transitive dependency graph. `Directory.Build.props` and `Directory.Build.targets` placed in any ancestor directory are automatically imported for every project beneath them, giving a single place to set repo-wide policies.

A **Target** is a named, ordered list of **Task** invocations: `Csc` invokes the C# compiler, `Copy` copies files, `Exec` runs a shell command, `Message` logs output. Target ordering comes from three attributes — `DependsOnTargets` declares prerequisites, `BeforeTargets="Build"` injects before `Build`, and `AfterTargets="Publish"` runs after `Publish` — all without modifying SDK-owned files. Declare `Inputs` and `Outputs` on a target to enable incremental builds; MSBuild skips the target when outputs are newer than inputs.

```xml
<Target Name="GenerateCode" BeforeTargets="CoreCompile">
  <Exec Command="python codegen.py" />
  <ItemGroup>
    <Compile Include="Generated\*.cs" />
  </ItemGroup>
</Target>
```

---

## Q14. What is Roslyn, how does its compilation pipeline work, and what are analyzers and source generators?

**Concepts**
- Roslyn — open-source C#/VB.NET compiler platform; "compiler as a service" with all pipeline stages as public APIs
- Pipeline — parse → full-fidelity SyntaxTree (no types) → bind → SemanticModel (types/symbols) → flow analysis → Emit (IL)
- Analyzers (`DiagnosticAnalyzer` + `CodeFixProvider`) — plug into compiler; report diagnostics; ship as NuGet; severity via `.editorconfig`
- Source generators (`IIncrementalGenerator`) — add `.cs` files to compilation at build time; pipeline of cached transformations; full IDE support

**Answer**

Roslyn is Microsoft's open-source compiler platform for C# and VB.NET, released in 2014. Its core principle is "compiler as a service": every stage of the compilation pipeline is exposed as public APIs that IDEs, analyzers, source generators, and code formatters consume rather than each tool reinventing parsing and type resolution from scratch.

The **compilation pipeline** has four stages. First, **parsing** converts source text into a `SyntaxTree` — an immutable, full-fidelity AST where every character including whitespace and comments is preserved as trivia. Parsing never throws: malformed code produces error nodes, so tools work on broken or partial code. Second, **binding** — a `Compilation` combines syntax trees with `MetadataReference` objects for referenced assemblies and builds the symbol table. `compilation.GetSemanticModel(tree)` returns a lazy `SemanticModel` that answers questions about any node: `GetSymbolInfo(node)` resolves an identifier to its declared symbol; `GetTypeInfo(expr)` returns the expression's compile-time type; `GetDiagnostics()` returns binding errors. Third, **flow analysis** runs on the bound representation: nullable reference type analysis, definite assignment checking, and reachability. Finally, **Emit** lowers everything to IL bytecode + PE metadata via `compilation.Emit(stream)`, producing the `.dll` the CLR loads.

**Roslyn analyzers** are classes extending `DiagnosticAnalyzer`. They register callbacks — on syntax nodes (`RegisterSyntaxNodeAction`), symbols (`RegisterSymbolAction`), or compilation events — and call `context.ReportDiagnostic(...)` when a rule is violated. A paired `CodeFixProvider` produces `CodeAction` document transformations shown as light-bulb suggestions in the IDE and applied via `dotnet format`. Analyzers ship as NuGet packages: MSBuild registers them automatically on install. Rule severity is configurable via `.editorconfig`: `dotnet_diagnostic.CA1001.severity = error`.

**Source generators** (`IIncrementalGenerator`) run during compilation and inject additional `.cs` files into the ongoing compilation, giving generated code full IntelliSense and debugger support. The `IIncrementalGenerator` API uses a pipeline of cached transformations — `SyntaxProvider.CreateSyntaxProvider` to filter nodes, then `Select`/`Where`/`Combine` — where each step is memoized: if its input did not change, the step is skipped entirely. Common use cases: `System.Text.Json` source-generated serialization (required for Native AOT), `Microsoft.Extensions.Logging` `LoggerMessage` generation (eliminates boxing on hot log paths), compiled regex, and ASP.NET Core Minimal API delegate generation.

---

## Q15. What is the CLR and what are its core responsibilities?

**Concepts**
- JIT compilation via RyuJIT — IL to native code on first method call
- Generational garbage collection — automatic memory reclamation
- Type safety enforcement — array bounds, null checks, cast verification
- Assembly loading — resolves and loads dependent DLLs
- Thread pool and async infrastructure backing Task, async/await, timers

**Answer**

The CLR (Common Language Runtime) is the managed execution engine that sits between your IL code and the operating system. Its primary job is JIT compilation: when your app calls a method for the first time, the CLR's embedded RyuJIT compiler translates that method's IL into native machine code and caches it. The CLR also runs the generational garbage collector, which automatically frees heap memory when objects are no longer reachable — you never call malloc or free in managed code.

Type safety is enforced throughout: the CLR checks array bounds, catches null dereferences before they corrupt memory, and validates casts. Structured exception handling — try/catch/finally — works consistently across all .NET languages because the CLR owns that mechanism. The assembly loader reads manifests, resolves external dependencies, and loads DLLs. The CLR provides the thread pool that backs Task, async/await, and timers. It also manages P/Invoke interop boundaries when managed code calls native DLLs. What the CLR does not do: compile your source code (that is Roslyn's job) or orchestrate builds (that is MSBuild's job).

---

## Q16. Walk me through the end-to-end execution of a .NET app — from source code to CPU instructions.

**Concepts**
- Roslyn compilation — .cs source → IL + metadata in .dll assemblies (build time)
- Host startup — dotnet reads runtimeconfig.json to select the correct runtime version
- CoreCLR initialization — GC heap, thread pool, and type system setup
- Assembly loading — resolved from deps.json and app folder
- Tiered JIT — Tier 0 for fast startup, Tier 1 background recompilation for hot paths

**Answer**

Execution begins at build time: Roslyn compiles your `.cs` files into IL bytecode and type metadata stored in `.dll` assemblies. When you run `dotnet MyApp.dll`, the native dotnet host starts, loads `hostfxr`, and reads `MyApp.runtimeconfig.json` to locate the correct runtime version. CoreCLR initializes — allocating the GC heap, starting the thread pool, and setting up the type system. The assembly loader then reads `MyApp.deps.json` to resolve all dependencies, probing the app folder and NuGet cache.

Your `Main` method is called and RyuJIT compiles it from IL to native x64 or ARM64 instructions on first invocation, caching the result so subsequent calls run it directly. As the app runs, tiered compilation monitors hot methods and recompiles them with full optimization in the background (Tier 0 for fast startup → Tier 1 for throughput). The GC runs periodically to collect unreachable objects. On exit, the CLR runs finalizers for any registered objects before shutting down. An important detail: IL is never interpreted line-by-line; even the quick Tier 0 JIT produces real native machine code.

---

## Q17. What is tiered JIT (Tier 0 / Tier 1)? How does RyuJIT balance startup speed vs throughput?

**Concepts**
- Classic JIT dilemma — aggressive optimization hurts startup; skipping it hurts steady-state throughput
- Tier 0 — fast compilation with minimal optimization; call counters inserted to track hot paths
- Tier 1 — background recompilation with full optimization when a method becomes hot
- ReadyToRun (R2R) — bakes pre-compiled native stubs to reduce Tier 0 work at startup

**Answer**

The fundamental JIT tension is that aggressive optimization takes time, which hurts startup, while skipping optimization leaves steady-state throughput on the table. Tiered JIT resolves this by compiling each method twice. On first call, RyuJIT uses Tier 0 — quick compilation with minimal optimization so the method runs immediately, with call counters inserted to track invocation frequency. Once a method crosses the hot threshold, a background thread recompiles it as Tier 1 with full optimization — inlining, loop unrolling, SIMD vectorization, and register allocation tuning — and atomically replaces the Tier 0 version. The result is fast app startup driven by Tier 0 and near-native steady-state throughput for hot paths once Tier 1 kicks in. Most methods are cold and never warrant Tier 1 recompilation. ReadyToRun precompilation can reduce the Tier 0 burden further by embedding pre-compiled native stubs in the assembly at publish time, so the app starts with less JIT work upfront.

---

## Q18. What is the difference between ReadyToRun (R2R) and Native AOT? When would you pick each?

**Concepts**
- ReadyToRun — pre-compiled native stubs at publish time; CLR and JIT still present at runtime
- Native AOT — full native binary at build time; no CLR, no JIT, no IL shipped
- R2R trade-off — faster startup with full reflection and dynamic features preserved
- Native AOT trade-off — fastest cold start, smallest binary; no runtime code generation

**Answer**

Both techniques reduce JIT work at runtime but take fundamentally different approaches. ReadyToRun pre-compiles IL to native code at publish time and stores both the native code and the original IL in the assembly. At runtime, the CLR uses the pre-compiled code instead of running Tier 0 JIT — this speeds up startup while keeping the full CLR present. Because IL is still present, R2R falls back to JIT when needed, meaning all runtime features work: reflection, `Assembly.LoadFrom`, dynamic code generation, and plugins. R2R is the right choice for web APIs and services that need faster startup without sacrificing any capabilities.

Native AOT goes further: the entire app is compiled to a native binary at build time, with no IL and no CLR JIT at runtime. Cold starts are the fastest possible, and with trimming the output binary is small — suitable for containers and serverless. The cost is that runtime code generation is impossible, unbounded reflection over unregistered types fails, and `Assembly.LoadFrom` at runtime does not work. Native AOT is best for serverless functions where cold start milliseconds matter, CLIs distributed to bare machines, and lean gRPC microservices. The mental model: R2R is faster startup with full flexibility; Native AOT is maximum performance with dynamic features traded away.

---

## Q19. What is IL/CIL and why do compilers target it instead of native code? What does "stack-based VM" mean?

**Concepts**
- IL (Intermediate Language) — CPU-neutral bytecode; same .dll runs on x64, ARM64, etc.
- Cross-language interop — all .NET languages produce the same IL format
- Runtime-aware JIT optimization — JIT detects actual CPU features at execution time
- Stack-based evaluation model — operations push/pop a typed evaluation stack

**Answer**

IL (Intermediate Language), also called CIL (Common Intermediate Language), is the CPU-neutral bytecode that every .NET compiler produces. Rather than compiling C# directly to x64 or ARM64, Roslyn emits IL, and RyuJIT converts IL to native code at runtime on each machine. This indirection buys three things: a single binary that runs on any supported CPU since the JIT generates architecture-specific code per machine, cross-language interoperability since C#, F#, and VB.NET all emit the same IL format (a C# class can inherit from an F# type at the binary level), and runtime-aware optimization since the JIT can detect the actual CPU features of the executing machine — AVX, NEON, SSE — and generate tuned code that a static ahead-of-time compiler targeting many architectures could not.

IL operates on a stack-based evaluation model rather than using CPU registers directly. Adding two numbers looks like: push `a`, push `b`, execute `add` (pops both, pushes result). The CLR's verifier tracks the type on every stack slot at every instruction, which is how it proves type safety statically before any code runs. RyuJIT maps this stack model onto actual CPU registers during JIT — the stack is an abstraction, not a performance penalty.

---

## Q20. What is metadata in a .NET assembly, and why do reflection, DI containers, and serializers depend on it?

**Concepts**
- Assembly metadata — binary tables describing every class, method, field, property, and attribute
- Reflection reads metadata at runtime without prior knowledge of the types
- DI containers scan constructor parameter metadata to determine injection targets
- Serializers map JSON keys to C# properties by reading property metadata
- Native AOT trimming removes "unused" metadata, which can break runtime reflection

**Answer**

Metadata is the complete type description embedded in every `.dll` alongside the IL. It is stored in binary tables inside the PE file and describes every class, interface, method, field, property, generic parameter, and custom attribute in the assembly. The CLR itself uses metadata during assembly loading to resolve type references across assemblies.

Metadata is what makes .NET's "magic" feel automatic. Reflection — `typeof(MyClass).GetProperties()` — reads these tables at runtime; because metadata is always present, the runtime knows every member of every type without any code generation. ASP.NET Core's DI container scans constructor parameter metadata to determine what services to inject — no manual registration of parameter types needed. `System.Text.Json` reads property metadata to map JSON keys to C# properties without configuration. The key gotcha is Native AOT with aggressive trimming: the trimmer removes metadata for types it analyzes as unreachable from the static call graph, so apps that discover types at runtime through reflection over unregistered assemblies will fail after trimming because the metadata reflection needs has been removed.

---

## Q21. What is IL verification, when can it be skipped, and what replaced Code Access Security (CAS) in modern .NET?

**Concepts**
- IL verification — static type-safety pass before JIT; tracks stack slot types at every instruction
- unsafe code — produces unverifiable IL; CLR trusts the developer and skips verification
- CAS (removed) — expensive stack walks per sensitive call; bypassable; impossible to configure correctly
- Modern security model — OS/container boundaries + ASP.NET Core authorization policies

**Answer**

IL verification is a static analysis pass the CLR runs before JIT-compiling a method. The verifier traces the type on every stack slot at every IL instruction — if a method tries to call a method on a null-typed slot or passes a wrong type, the verifier rejects it before any code runs. This is how the CLR guarantees type safety without executing the code first. Verification is skipped for methods compiled with C#'s `unsafe` keyword, which enables raw pointer arithmetic and produces unverifiable IL — the CLR trusts that `unsafe` code is correct since the developer opted into the unsafe context.

Code Access Security was .NET Framework's mechanism for assigning permissions based on code origin (internet zone, intranet zone, trusted publisher). Every sensitive BCL call performed a stack walk to verify that all frames in the call stack held the required permissions. In practice it was bypassable via reflection tricks, added overhead on hot paths, and was nearly impossible to configure correctly. Modern .NET removed CAS entirely — security now lives at the process and container boundary, with application-layer authorization handled by ASP.NET Core middleware using claims and policy handlers, which is both simpler and more reliable.

---

## Q22. What are the CTS and CLS, what is the difference between them, and why do they enable cross-language interop?

**Concepts**
- CTS (Common Type System) — the full type model all .NET languages must conform to
- CLS (Common Language Specification) — subset of CTS for public APIs consumable by any language
- Cross-language interop — all .NET languages compile to the same CTS-conformant IL
- CLSCompliant attribute — compiler warnings when a public API violates CLS rules

**Answer**

The CTS and CLS are two concentric circles. The CTS (Common Type System) is the full type model that all .NET languages must conform to: it defines what value types and reference types are, how inheritance works, visibility rules, and how generics work at the runtime level. When C# writes `int` and VB.NET writes `Integer`, both compile to `System.Int32` in the CTS — the same binary type. This is why cross-language inheritance works: both languages produce IL targeting the same type system, and the CLR loads and executes them identically.

The CLS (Common Language Specification) is a stricter subset of the CTS defining the rules a public API must follow to be consumable from any .NET language without friction. Examples of CLS rules include: no `uint` in public method signatures (because VB.NET had no unsigned integer literal), no member names that differ only by case (some languages are case-insensitive), and no public static constructors. Internal code can use any CTS feature; public APIs should follow CLS if cross-language consumers matter. Mark `[assembly: CLSCompliant(true)]` to get compiler warnings whenever a public API violates CLS rules.

---

## Q23. What is boxing and unboxing? What is the performance cost, and when does it happen implicitly?

**Concepts**
- Boxing — wraps a value type in a heap-allocated object so it can be referenced as object or interface
- Unboxing — extracts the value back; requires explicit cast and runtime type check
- Heap allocation per box — GC pressure in tight loops; value copied twice (in, then out)
- Implicit boxing sources: non-generic collections, string interpolation, object parameters

**Answer**

Boxing wraps a value type (int, struct) in a heap-allocated object so it can be referenced as `object` or through an interface. Unboxing extracts the original value back out, which requires an explicit cast and a runtime type check. Each box is a heap allocation the GC must eventually collect, so boxing in tight loops adds real pressure — the value is copied twice and the allocation work accumulates.

The sneaky part is how often boxing happens implicitly. Passing an `int` to a method that takes `object` boxes it. Storing value types in `ArrayList` or `Hashtable` boxes every element, which is why those collections were superseded by generic equivalents. String interpolation like `$"Value: {myInt}"` boxes `myInt`. Enum values passed to object-typed parameters box silently.

```csharp
int x = 42;
object boxed = x;      // Boxing — allocates on heap, copies value
int y = (int)boxed;    // Unboxing — type check + copy back
```

The fix is to use generics everywhere — `List<int>` instead of `ArrayList` prevents boxing entirely because the collection is specialized for the value type. `where T : struct` constraints on generic methods and `Span<T>` for buffer work are the other main strategies.

---

## Q24. What is the difference between managed and unmanaged code? What services does managed code get from the CLR?

**Concepts**
- Managed code — compiled to IL, runs under CLR control; gets GC, type safety, exception handling
- Unmanaged code — native DLLs, C/C++ libraries, COM; memory management is developer's responsibility
- P/Invoke boundary — CLR marshals data between managed and native memory layouts
- GC protections do not apply during native calls; crashes corrupt the whole process

**Answer**

Managed code is any code compiled to IL that runs under CLR supervision — the runtime handles memory reclamation, type safety enforcement, structured exception handling, and thread scheduling. Unmanaged code is native code — OS DLLs, C/C++ libraries, COM components — that runs entirely outside CLR control. Memory leaks, buffer overflows, and memory corruption are the developer's full responsibility since the GC has no visibility into native heap allocations.

When managed code calls unmanaged code via P/Invoke, the CLR must marshal data at the boundary: managed strings become C-style char pointers, managed structs need explicit layout attributes to match native memory layouts, and callbacks into managed code from native need delegate types with matching calling conventions. GC protections do not apply during the native call itself — a managed object can be moved by the GC during its execution unless pinned with `fixed`. A native DLL that crashes corrupts the entire process even if the calling managed code would otherwise be safe — there is no managed exception boundary around a native crash.

---

## Q25. What is P/Invoke? How does managed code call a native DLL, and what is marshalling?

**Concepts**
- P/Invoke — CLR mechanism for calling exported functions in native DLLs
- DllImport attribute — declares the native function signature, DLL name, and charset
- Marshalling — data conversion between managed and native memory representations
- LibraryImport (.NET 7+) — source-generated marshalling; works with Native AOT

**Answer**

P/Invoke (Platform Invocation Services) is the CLR mechanism for calling functions exported from native DLLs — Win32 APIs, C libraries, OS-level system calls. You declare the method signature with `[DllImport]` and the CLR handles the rest: it loads the native DLL, finds the exported function by name, and sets up the call transition.

```csharp
[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
static extern bool MoveFileW(string lpExistingFileName, string lpNewFileName);
```

When the managed method is called, the CLR marshals arguments from managed to native format — `string` becomes a null-terminated `char*` or `LPWSTR`, `bool` becomes a 4-byte `BOOL` (Win32's integer convention), and structs need `[StructLayout(LayoutKind.Sequential)]` to guarantee field ordering and padding match what the native function expects. Callbacks from native code into managed code need delegate types with the matching calling convention. The modern approach from .NET 7 onwards is `[LibraryImport]` instead of `[DllImport]` — it generates all marshalling code at compile time via source generators, works correctly with Native AOT (which cannot do runtime reflection-based marshalling), and removes runtime overhead.

---

## Q26. What is a .NET assembly? What does it contain, and how is it different from a namespace or a native DLL?

**Concepts**
- Assembly — unit of deployment: .dll or .exe containing IL, metadata, manifest, and resources
- Assembly manifest — identity (name, version, culture, public key token) + list of referenced assemblies
- Assembly vs namespace — namespace is a naming convention; assembly is a physical file
- Assembly vs native DLL — native DLL has machine code + flat export table; .NET assembly has IL + metadata

**Answer**

An assembly is the fundamental unit of deployment in .NET — a `.dll` or `.exe` that the CLR loads and executes as one coherent package. Inside every assembly is a manifest (the assembly's identity: name, version, culture, and optional public key token, plus the list of all external assemblies it references), complete type metadata describing every class, interface, method, field, property, and attribute, the IL bodies for all compiled methods, and optionally embedded resources like images or localized strings.

An assembly is distinct from a namespace: a namespace is purely a naming convention in source code, and `System.Collections.Generic` is defined across multiple assemblies while one assembly can define types in multiple namespaces. There is no file called "namespace." A .NET assembly is also distinct from a native DLL: a native DLL contains native machine code and a flat table of exported function addresses that the OS loader can resolve by name or ordinal. A .NET assembly contains IL and metadata in a PE (Portable Executable) wrapper — the OS can load the file, but only the CLR can execute the IL inside it.

---

## Q27. What is the difference between a DLL and an EXE in .NET?

**Concepts**
- .exe — has an entry point (Main method); can be launched directly by the OS
- .dll — no entry point; loaded by a host or referenced by another project
- dotnet MyApp.dll — perfectly normal in modern .NET; the dotnet host invokes the entry point
- Both are PE files and valid .NET assemblies

**Answer**

In .NET, both `.dll` and `.exe` are PE (Portable Executable) files and can be valid .NET assemblies. The practical difference is narrow: an `.exe` has a `Main` entry point (or top-level statements) and can be launched directly by the OS. A `.dll` has no entry point and must be loaded by a host process or referenced by another project.

In modern .NET specifically, the distinction blurs further. `dotnet publish` typically produces a `MyApp.dll` containing the entry point alongside a native launcher `MyApp.exe` that starts the dotnet host and hands it the DLL. Running `dotnet MyApp.dll` directly is perfectly normal — the dotnet host acts as the launcher, reads the DLL's entry point, and calls it. A class library project produces a `.dll` with no entry point and cannot be executed directly. The common gotcha from .NET Framework thinking is expecting an `.exe` to always be the executable artifact — in modern .NET, `dotnet MyApp.dll` is the standard invocation pattern on Linux and in containers.

---

## Q28. What is strong naming? What does a strong name consist of, and is the GAC still used in modern .NET?

**Concepts**
- Strong name — cryptographic identity: assembly name + version + culture + public key token
- Tamper detection — signature breaks if the DLL is modified after signing
- Strong naming provides unique identity and tamper detection, not execution trust or sandboxing
- GAC (Global Assembly Cache) — Framework-only machine-wide store; not used in modern .NET

**Answer**

Strong naming gives an assembly a cryptographic identity by signing it with a public/private key pair. A strong name combines four components: the assembly name, version, culture, and a public key token (8-byte hash of the public key). Together they uniquely identify an assembly and allow the CLR to distinguish `MyLibrary v1.0` from `MyLibrary v2.0` from different publishers. Strong naming also provides tamper detection — if the DLL is modified after signing, the signature verification fails. What strong naming does not provide is execution trust or sandboxing; it is about identity and versioning, not security policy.

The GAC (Global Assembly Cache) was the .NET Framework mechanism for machine-wide shared deployment of strongly named assemblies. Multiple apps on the same server could reference one physical copy from the GAC. In modern .NET, the GAC is gone: every app has its own private copies of all dependencies in its publish folder, and NuGet handles version resolution per project. Strong names are still used — they are required for libraries targeting .NET Framework or .NET Standard and provide a stable identity for the assembly binding system — but configuring the GAC for .NET 8+ applications is a red flag that signals outdated knowledge.

---

## Q29. Explain generational GC — Gen 0, Gen 1, Gen 2. What is the weak generational hypothesis and what triggers a collection?

**Concepts**
- Weak generational hypothesis — most objects die young
- Gen 0 — brand new short-lived objects; collected very frequently (milliseconds)
- Gen 1 — survived one Gen 0 collection; buffer between new and long-lived
- Gen 2 — long-lived objects (caches, singletons, statics); full GC is expensive and rare
- Collection triggers — Gen 0 fills, memory pressure, or explicit GC.Collect() (avoid in production)

**Answer**

The .NET GC is generational because of one empirical observation: most objects die young. Request buffers, LINQ iterators, temporary strings — they are created, used, and immediately unreachable. Scanning the entire heap for every collection would be wasteful since most of it is long-lived and unlikely to have new garbage. The weak generational hypothesis is just the formal name for this observation.

The GC divides the heap into three generations and collects younger ones more often. Gen 0 holds brand new allocations and is collected very frequently — often in milliseconds — because most allocations here are dead by the next collection. Survivors get promoted to Gen 1, which is collected less often and acts as a buffer. Long-lived objects — singletons, caches, static collections — eventually reach Gen 2, which is only collected during a full GC that pauses the application longer. The primary performance advice follows directly: avoid accidentally promoting objects to Gen 2 by creating unnecessary long-lived references, because Gen 2 collections are expensive. A collection is triggered when a generation's segment fills up, when the OS signals memory pressure, or explicitly via `GC.Collect()` — which you should almost never call in production code.

---

## Q30. Is it always true that value types live on the stack and reference types on the heap? Where does this break?

**Concepts**
- The "value types on stack" rule — an oversimplification
- Boxing — puts a value type on the heap inside a boxed object
- Fields inside a class — value-type fields live wherever the class object lives (the heap)
- Closures and async state machines — promote captured locals to the heap regardless of type

**Answer**

The rule that value types live on the stack and reference types on the heap is an oversimplification. The accurate statement is that locals which can be proven not to escape the current method may be stack-allocated, but value types end up on the heap in several common situations. Boxing is the obvious case — `object o = 42` puts the int on the heap inside a boxed object. A value-type field inside a class lives wherever the class object lives, which is the heap. Closures and lambdas that capture a local variable promote that local into a compiler-generated display class on the heap, regardless of whether the variable is a value type. Async methods promote their entire local variable set to a heap-allocated state machine struct when the first await is reached.

Going the other way, JIT escape analysis can stack-allocate small objects that provably do not escape a method, and `stackalloc` with `Span<T>` explicitly stack-allocates a buffer — but these are optimizations and explicit choices, not the default. The right mental model: reference-type locals store a pointer on the stack; the object they point to is on the heap. Value-type locals store their bits on the stack unless they are inside another heap object, boxed, captured by a closure, or part of an async state machine.

---

## Q31. What is the Large Object Heap (LOH)? How does it differ from the small-object heap, and what size triggers it?

**Concepts**
- LOH threshold — objects ≥ 85,000 bytes skip the generational heap and go here directly
- Collected only during Gen 2 full GC
- No compaction by default — LOH fragments over time; can cause OOM despite free space
- ArrayPool<byte>.Shared — rent-and-return pattern to avoid repeated LOH allocations

**Answer**

Objects at or above 85,000 bytes skip the generational small-object heap and go directly to the Large Object Heap — mostly large byte arrays used for network buffers, images, or big strings. Because the LOH is only collected during Gen 2 full GCs, LOH objects live a long time even if they are no longer referenced. More critically, the LOH is not compacted by default — moving a 10 MB array takes real time that would cause noticeable pauses, so the GC leaves LOH in place and reuses freed gaps. Over time, if new allocations are consistently different sizes from the freed gaps, the LOH becomes fragmented: you can have hundreds of megabytes of total free LOH space but still get `OutOfMemoryException` because no single contiguous gap is large enough for the next allocation.

| | Small Object Heap | Large Object Heap |
|---|---|---|
| Compacted after GC? | Yes | No (by default) |
| Collected when? | Gen 0, 1, or 2 | Gen 2 only |
| Fragmentation risk | Low | High over time |

The fix is to avoid repeated LOH allocations by reusing large buffers. `ArrayPool<byte>.Shared.Rent(size)` returns a buffer from a pool and `Return` sends it back — no new allocation, no LOH pressure. When you truly must compact the LOH, set `GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce` before the next GC.

---

## Q32. What is the difference between `IDisposable.Dispose` and a finalizer? Explain the `Dispose(bool disposing)` pattern.

**Concepts**
- Finalizer (~MyClass()) — GC-driven; indeterminate timing; safety net only for unmanaged resources
- IDisposable.Dispose — developer-driven; deterministic via using / await using
- Dispose(bool disposing) — separates managed vs. unmanaged cleanup paths
- GC.SuppressFinalize — prevents the extra GC cycle after Dispose cleans everything up

**Answer**

A finalizer (`~MyClass()` in C#) is called by the GC at some indeterminate future point — you have no control over when, and it might be seconds or minutes after the object becomes unreachable. Its only legitimate purpose is as a safety net: if a developer forgets to call `Dispose`, the finalizer ensures unmanaged resources are eventually released. The cost is that objects with finalizers are kept alive for at least one extra GC cycle while waiting for the finalizer thread, increasing memory pressure. Never rely on finalizers for timely cleanup of file handles, database connections, or locks.

`IDisposable.Dispose` is deterministic — the developer controls when it runs by calling it explicitly or wrapping the object in a `using` / `await using` block. The `Dispose(bool disposing)` pattern separates the two cleanup paths. When `disposing` is true, it is safe to access and dispose other managed objects because Dispose was called intentionally. When `disposing` is false, the finalizer is running and other managed objects may already have been collected, so only unmanaged handles should be released. `GC.SuppressFinalize(this)` in the public `Dispose` method removes the object from the finalization queue, preventing the extra GC cycle since cleanup already happened.

```csharp
public class ResourceHolder : IDisposable {
    private bool _disposed;

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (_disposed) return;
        if (disposing) {
            _managedResource?.Dispose();
        }
        CloseNativeHandle();
        _disposed = true;
    }

    ~ResourceHolder() {
        Dispose(disposing: false);
    }
}
```

---

## Q33. What does a memory leak look like in managed code despite having a GC? What causes GC pressure?

**Concepts**
- GC only collects objects with no GC roots — leaks mean objects are still reachable but should not be
- Static collections that grow indefinitely — survive to Gen 2, never shrink
- Event subscriptions not unsubscribed — publisher holds reference to subscriber indefinitely
- GC pressure — frequent collections from excessive short-lived allocations, not a leak

**Answer**

The GC only collects objects it can prove are unreachable. A memory leak in managed code means objects are still reachable through some GC root — they just should not be. The most common pattern is static collections that only grow: a `static List<byte[]>` that accumulates entries on every request never shrinks, fills Gen 2, and eventually causes OOM. Event subscriptions are the other classic source: when you subscribe to an event on a long-lived publisher without unsubscribing, the publisher's multicast delegate chain holds a reference to your handler, which captures `this`, keeping the subscriber alive as long as the publisher lives. Lambdas can cause the same issue when they capture large objects that outlive their expected scope. Unbounded caches — a `Dictionary<string, BigObject>` that only gains entries — are functionally equivalent to a static leak.

GC pressure is a different problem — not a leak, but degraded performance from too many allocations forcing frequent collection cycles. Boxing in tight loops, repeated large array allocations hitting the LOH, LINQ chains producing intermediate arrays, `string +` concatenation in loops, and `new Task<T>` where `ValueTask<T>` would suffice all create GC pressure without necessarily leaking. Tools like `dotnet-trace`, `dotnet-dump`, PerfView, and Visual Studio's Diagnostic Tools help diagnose both: oversized Gen 2 or LOH usage suggests leaks, while high Gen 0/1 collection rate suggests pressure.

---

## Q34. What were AppDomains, why were they removed from .NET Core, and what are the modern alternatives?

**Concepts**
- AppDomains — .NET Framework in-process isolation; IIS used them to host multiple web apps per process
- Removed because: enormous BCL marshalling cost; native crashes still killed the whole process
- AssemblyLoadContext — in-process assembly loading/unloading; no static variable isolation
- Separate processes — stronger isolation; communicate via gRPC, named pipes, or message bus

**Answer**

AppDomains were a .NET Framework feature that allowed in-process isolation: IIS hosted multiple web applications inside one `w3wp.exe` process, each in its own AppDomain with its own static variables, configuration, and assembly set. You could unload an AppDomain without killing the hosting process, which allowed hot deployment of individual apps. The isolation was enforced entirely in managed code rather than at the OS level.

They were removed from modern .NET for several reasons: every single BCL type had to support marshalling across AppDomain boundaries, which was an enormous ongoing maintenance burden. The isolation was never truly strong — a native crash in any AppDomain killed the entire process regardless. In the cloud era, process and container boundaries provide genuine strong isolation at lower complexity. The implementation was also deeply entangled with .NET Framework internals and could not be ported cleanly.

| What you need | Modern approach |
|---|---|
| Load/unload a set of assemblies (plugins) | `AssemblyLoadContext` — create one per plugin, dispose to unload |
| Crash isolation | Separate OS processes (gRPC, named pipes, message bus) |
| Multi-tenant isolation | Containers / Kubernetes namespaces |
| Per-request state isolation | `AsyncLocal<T>` — flows through async context |

`AssemblyLoadContext` is the closest in-process equivalent: each ALC has its own assembly set, and disposing it unloads those assemblies. It does not isolate static variables or provide security policy — for anything stronger, use a separate process.

---

## Q35. What are the main motivations for migrating from .NET Framework to modern .NET? What is the strangler fig pattern, and what are the most common breaking changes to watch for?

**Concepts**
- Performance — ASP.NET Core dramatically faster; Span<T>, tiered JIT, SIMD gains
- Cross-platform — Linux containers; Framework cannot run on Linux
- Framework is frozen — no new APIs, no performance improvements after 4.8
- Strangler fig via YARP — incremental route-by-route migration, always deployable
- Common breaking changes — System.Web, BinaryFormatter, WCF server, AppDomain, Thread.Abort

**Answer**

The four main motivations are performance, cross-platform deployment, access to new language and runtime features, and the fact that Framework is frozen. ASP.NET Core consistently tops TechEmpower web framework benchmarks, and `Span<T>`, Memory<T>, tiered JIT, and SIMD bring .NET performance close to native for many workloads — gains that will never come to Framework 4.8. Running on Linux cuts cloud hosting costs and enables the container-native deployment model that most infrastructure now assumes. New features like C# 9+, Native AOT, records, and improved async patterns are exclusive to modern .NET.

The strangler fig pattern makes migration low-risk by avoiding big-bang rewrites. A reverse proxy (YARP is the modern .NET choice) sits in front of the application and routes incoming requests: old routes go to the legacy Framework app still running, migrated routes go to the new modern .NET service. You strangle the old app route by route until nothing remains to route to it — always deployable, always partially complete.

The most common breaking changes to plan for: `System.Web` and Web Forms have no migration path and require a full rewrite to Razor Pages, MVC, or Blazor. `BinaryFormatter` is removed — use `System.Text.Json`, protobuf, or MessagePack. The WCF server stack was not ported — use CoreWCF for basic HTTP/SOAP or gRPC for RPC. `AppDomain` is removed — replace with `AssemblyLoadContext` or separate processes. `Thread.Abort()` is removed — use `CancellationToken`. `web.config` becomes `appsettings.json` plus the options pattern. `System.Data.SqlClient` becomes `Microsoft.Data.SqlClient`. The `try-convert` tool migrates legacy `.csproj` to SDK-style format, and the CA1416 platform analyzer flags Windows-only API calls.

---

## Gotchas — .NET Framework Architecture (Interview Traps)

---

#### Gotcha 1. CLR vs CoreCLR — 'cross-platform' doesn't mean 'identical behavior'

**Concepts**
- AppDomain removal in CoreCLR
- Thread.Abort removed
- COM interop reduced
- BCL surface differences

**Answer**

Moving from .NET Framework to modern .NET exposes subtle CLR differences even for identical C# code. AppDomain isolation no longer exists, Thread.Abort throws PlatformNotSupportedException, and COM-interop marshalling differences surface only at runtime on Windows, making full testing on the target platform mandatory.

---

#### Gotcha 2. JIT tiers don't mean every method gets fully optimized

**Concepts**
- Tier 0 uses minimal optimizations for fast startup
- Tier 1 is OSR-promoted hot methods
- ReadyToRun pre-compiles to Tier 0 equivalent
- PGO (Profile-Guided Optimization) is Tier 2

**Answer**

The tiered JIT compiles every method at Tier 0 first to minimize startup latency, promoting frequently-called methods to Tier 1 only after they prove hot. Code that runs only once or a few times during startup may never reach full optimization, which means benchmarking a method in isolation gives different performance numbers than production warm-up behavior.

---

#### Gotcha 3. Native AOT breaks reflection-heavy code at publish time, not at runtime in dev

**Concepts**
- Static tree-shaking removes unreachable types
- Activator.CreateInstance of trimmed types fails
- Serializers relying on reflection need source generators
- IlLink.Substitutions.xml for manual preservation

**Answer**

An application that works perfectly in JIT mode may fail after dotnet publish --aot because the trim analyzer removes types accessed only through reflection. The failures manifest as TypeLoadException or MissingMethodException in the published binary, not during development, making a proper AOT smoke-test suite essential before shipping.

---

#### Gotcha 4. 'Value types live on the stack' is a misleading simplification

**Concepts**
- Value types in class fields live on the heap (with the object)
- Captured variables in closures live on the heap
- Boxed value types live on the heap
- Only local value-type variables with no capture live on the stack

**Answer**

The 'stack vs heap' rule applies only to local value-type variables in methods where the value is not captured by a closure or returned as a reference. Any value type stored as a class field, boxed to object, or captured in a lambda is heap-allocated, which is why the simplification misleads performance discussions.

---

#### Gotcha 5. Strong naming is identity, not security — and mostly irrelevant in .NET Core

**Concepts**
- Strong name = name + version + culture + public key token
- GAC is gone in .NET Core
- NuGet provides binding resolution
- Strong naming prevents accidental version mix-up, not tampering

**Answer**

Strong names in modern .NET are a compatibility artifact rather than a security mechanism. The Global Assembly Cache does not exist in .NET Core deployments, so strong naming primarily serves version disambiguation in multi-version scenarios — its historical role as a deployment gate is replaced by NuGet package versioning and SHA-pinning.

---

#### Gotcha 6. GC.Collect() in production is almost always counterproductive

**Concepts**
- Forces Gen 2 collection suspending all managed threads
- Releases memory the OS may immediately reclaim back
- Does not reduce peak memory if allocations resume immediately
- Correct lever is GC configuration (Server GC, concurrent GC)

**Answer**

Calling GC.Collect() forces a blocking Gen 2 collection that pauses all application threads, which can spike latency significantly on high-throughput services. The correct way to manage GC behavior is through RuntimeConfigurationOptions (Server GC, concurrent GC, LOH compaction) or by profiling allocation patterns and reducing allocations at the source.

---

#### Gotcha 7. Objects with finalizers survive one extra GC cycle and delay collection

**Concepts**
- Finalizable objects move to the freachable queue after Gen 0/1 collection
- Finalizer thread runs them asynchronously
- GC.SuppressFinalize removes them from the queue
- Large finalizer backlog delays overall collection

**Answer**

When an object's finalizer has not yet run, the GC cannot collect it — the object is promoted to Gen 1 or 2 and placed in a special finalization queue. A backlog of objects awaiting finalization directly delays the collection of everything that transitively references them, which is why the dispose pattern calls GC.SuppressFinalize after deterministic cleanup.

---

#### Gotcha 8. Assembly binding redirects don't exist in .NET Core — NuGet resolves at build time

**Concepts**
- No app.config bindingRedirect in .NET Core
- runtimeconfig.json handles runtime version
- NuGet lock files pin exact versions
- Side-by-side loading via AssemblyLoadContext replaces redirects

**Answer**

.NET Framework app.config binding redirects allowed different DLLs in the same process to each use a different version of a shared library by redirecting at runtime. Modern .NET resolves version conflicts at build time through NuGet package graph unification, and side-by-side scenarios use AssemblyLoadContext, eliminating the runtime redirect mechanism entirely.

---

#### Gotcha 9. Nullable T? boxing behavior differs from T — null boxed nullable unboxes to throw, not null

**Concepts**
- Nullable<T> with HasValue=false boxes to a null reference
- Unboxing a null reference via (T?) still requires HasValue check
- (T)null throws NullReferenceException
- Use Nullable.GetValueOrDefault for safe unbox

**Answer**

Boxing a Nullable<T> that has HasValue=false produces a null object reference on the heap — the box contains nothing. Unboxing that null reference back to int throws NullReferenceException, not an InvalidCastException, which surprises developers who expected null to unbox safely to a nullable struct.

---

#### Gotcha 10. IntPtr.Size and Environment.Is64BitProcess are about address space, not performance width

**Concepts**
- IntPtr.Size == 4 on 32-bit process, == 8 on 64-bit
- SIMD width (SSE2=128, AVX2=256, AVX-512) is independent
- JIT uses CPU feature flags for SIMD regardless of process bitness
- 64-bit process gets larger address space, not wider SIMD

**Answer**

Running a process as 64-bit gives it a larger virtual address space (important for large in-memory caches), not automatically wider SIMD instructions. SIMD capabilities are determined by CPU feature flags that the JIT checks at startup — both 32-bit and 64-bit processes on a CPU with AVX2 can use 256-bit SIMD via System.Runtime.Intrinsics.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (P) Long-running service leaks memory via unremoved event subscriptions

**Concepts**
- Event subscription holds a strong reference from publisher to subscriber
- GC cannot collect an object that is still reachable through a GC root
- Singleton publisher keeps every subscribed handler alive indefinitely
- WeakReference and explicit unsubscribe in Dispose as the two remedies

**Answer**

A Worker Service creates a new request processor per job and subscribes it to an event on a shared `IMetricsCollector` singleton: `metricsCollector.RequestCompleted += processor.OnCompleted`. The processor looks short-lived, but the singleton's multicast delegate chain holds a strong reference to every handler ever added. Because the processor is never removed from the event and the singleton lives for the process lifetime, processors accumulate in Gen 2 despite appearing abandoned. Memory grows steadily until an `OutOfMemoryException` occurs under load.

The diagnostic path: a memory dump (`dotnet-dump analyze`) shows processors piling up in Gen 2 with reference chains rooted in the singleton's delegate field. The immediate fix is unsubscribing in `Dispose`: `metricsCollector.RequestCompleted -= processor.OnCompleted`, which requires the processor to implement `IDisposable` and be disposed after use. The robust design uses `IObservable<T>` subscription tokens — `IDisposable sub = observable.Subscribe(handler)` — so `sub.Dispose()` unsubscribes automatically when placed in a `using` block. The invariant: any object that subscribes to an event on a longer-lived object must unsubscribe before abandonment.

---

#### Q2. (R) Incorrect IDisposable implementation — missing finalizer and SuppressFinalize

**Concepts**
- Finalizer required as safety net for unmanaged resources
- Dispose(bool disposing) separates managed vs. unmanaged cleanup paths
- GC.SuppressFinalize removes object from finalization queue after deterministic cleanup
- Accessing managed objects inside finalizer path is unsafe — they may already be collected

**Problem Code**

```csharp
public class DatabaseConnection : IDisposable {
    private SqlConnection _connection;
    private IntPtr _nativeHandle;
    private bool _disposed;

    public DatabaseConnection(string connectionString) {
        _connection = new SqlConnection(connectionString);
        _nativeHandle = NativeLib.OpenHandle();
        _connection.Open();
    }

    public void Dispose() {
        if (_disposed) return;
        _connection.Dispose();
        NativeLib.CloseHandle(_nativeHandle);
        _disposed = true;
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No finalizer — if `Dispose` is never called, `_nativeHandle` leaks permanently | Native OS handle leak; resource exhaustion |
| Correctness | No `GC.SuppressFinalize` (even after adding a finalizer, object stays in finalization queue after `Dispose`) | Unnecessary extra GC cycle on every instance |
| Correctness | `_connection.Dispose()` would be called from finalizer path with no `disposing` guard | `_connection` may already be collected; unsafe dereference |

**Fix (priority order)**

1. Extract `Dispose(bool disposing)` — call `_connection.Dispose()` only when `disposing == true`
2. Add `~DatabaseConnection()` that calls `Dispose(false)` to ensure the native handle is released if `Dispose` is never called
3. Add `GC.SuppressFinalize(this)` at the end of the public `Dispose()` to remove the object from the finalization queue

**Answer**

The public `Dispose` handles managed cleanup but leaves no safety net for the native handle — if a caller forgets `using`, the handle leaks forever. A finalizer is required for unmanaged resources precisely because you cannot enforce call-site discipline. The `Dispose(bool disposing)` pattern separates the two paths: when `disposing` is true, other managed objects like `SqlConnection` are safe to dispose because Dispose was called intentionally; when `disposing` is false, the finalizer is running and other managed objects may already be collected — only native handles are safe to release. `GC.SuppressFinalize` in the public `Dispose` removes the object from the finalization queue so the GC reclaims it in the next collection cycle without an extra finalization pass.

---

#### Q3. (D) Choosing a publishing mode for a serverless gRPC microservice with strict cold-start SLA

**Concepts**
- Native AOT eliminates CLR startup, JIT warmup, and tiered compilation entirely
- AOT trimmer removes unreachable types — reflection over unregistered types fails at runtime
- ReadyToRun pre-compiles IL to native stubs but keeps CLR and full reflection
- Protobuf source generation is AOT-compatible; reflection-based serializers are not

**Answer**

For a serverless gRPC microservice where cold start directly affects billing and SLA, Native AOT is the optimal target — it eliminates CLR initialization, JIT compilation, and tiered promotion, producing a self-contained native binary that starts in tens of milliseconds rather than hundreds. gRPC with Protobuf is well-suited to AOT because the official Google.Protobuf library and grpc-dotnet support source-generated message serialization that bypasses runtime reflection.

The critical due-diligence step is auditing the full dependency tree for AOT compatibility before committing. DI containers and serializers that call `Assembly.GetTypes()` or `Activator.CreateInstance` over unregistered types will break after publish, not during development — failures manifest as `TypeLoadException` in the published binary. Add `dotnet publish -r linux-x64 --aot` to CI early and treat `IL3050`/`IL2026` analyzer warnings as blocking. If the dependency tree contains AOT-incompatible libraries, `<PublishReadyToRun>true</PublishReadyToRun>` is the fallback: it cuts startup by pre-compiling IL to native stubs while preserving full CLR capabilities. For always-warm containerized services where cold start is irrelevant, standard framework-dependent deployment avoids all AOT constraints and makes debugging easier.

---

#### Q4. (P) Large Object Heap fragmentation causes OOM under sustained load

**Concepts**
- LOH threshold at 85,000 bytes — large arrays bypass the generational small-object heap
- LOH collected only during Gen 2 full GC; not compacted by default
- Fragmentation accumulates when freed gaps cannot satisfy subsequent differently-sized allocations
- ArrayPool<byte>.Shared rent-and-return pattern eliminates repeated LOH allocations

**Answer**

An ASP.NET Core service that allocates `new byte[bufferSize]` per request for file upload buffering pushes every buffer ≥ 85,000 bytes onto the LOH. The LOH is collected only during Gen 2 full GCs, so buffers linger far longer than the request lifetime. More critically, the LOH is never compacted — freed address ranges remain as gaps rather than being consolidated. Under sustained load with varying buffer sizes (128 KB for small uploads, 4 MB for large ones), the gaps created by released 128 KB buffers cannot satisfy a 4 MB allocation. Over time, `dotnet-trace` shows a climbing Gen 2 collection rate, `dotnet-dump` shows a LOH with hundreds of megabytes of total free space but no single contiguous gap large enough for the next large allocation, and the service throws `OutOfMemoryException` despite abundant total RAM.

The fix is to stop allocating large arrays per-request entirely: `ArrayPool<byte>.Shared.Rent(bufferSize)` returns a buffer from a pre-allocated pool, and `ArrayPool<byte>.Shared.Return(buffer)` returns it — no LOH allocation, no pressure. The pool buckets by power-of-two size internally, so fragmentation cannot accumulate. `RecyclableMemoryStream` (from the Microsoft.IO.RecyclableMemoryStream package) applies the same principle to streaming scenarios. As a one-time emergency measure, `GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce` forces compaction on the next GC cycle, but the stop-the-world pause makes it unsuitable as a permanent solution.

---

#### Q5. (P) P/Invoke struct layout mismatch causes silent data corruption

**Concepts**
- StructLayout(LayoutKind.Sequential) with explicit Pack must match native compiler padding
- C compilers align struct fields to their natural size boundary by default
- CharSet mismatch produces wrong string encoding at the managed/native boundary
- Arrays passed by pointer must be pinned to prevent GC relocation mid-call

**Answer**

A managed struct is declared and passed by reference to a native Win32 function. It works on a developer machine but produces garbled data or access violations in production on a different build target. The cause is that C compilers pad structs based on alignment rules: on x64 Windows, MSVC pads a `{byte status; int code;}` struct with 3 bytes between the fields to align `code` on a 4-byte boundary, producing an 8-byte native struct. Without `[StructLayout(LayoutKind.Sequential, Pack = 4)]` matching the native compiler's pack setting, the CLR may lay out the managed struct differently, so the bytes the native function reads come from wrong offsets.

The verification step is to compare `sizeof` from a short C snippet (or the native header's documented size) with `Marshal.SizeOf<T>()` in .NET — any mismatch exposes a layout bug. For arrays passed by pointer, the GC can relocate managed objects during the native call unless pinned: use `fixed (byte* p = array)` for the duration of the call, or `GCHandle.Alloc(array, GCHandleType.Pinned)` for longer-lived scenarios, always releasing the handle in a `try/finally`. A related trap is `CharSet` mismatch: `[DllImport("...", CharSet = CharSet.Ansi)]` on a function expecting `LPWSTR` (UTF-16) produces double-byte nulls interpreted as an empty string on the native side — always check the native function's string convention against the declared `CharSet`.

---

#### Q6. (D) Migrating a .NET Framework WPF application with COM Office automation to modern .NET

**Concepts**
- WPF ported to net10.0-windows — viable migration path without rewriting the UI layer
- COM interop (PIA, RCW, late binding via dynamic) works on modern .NET on Windows
- BinaryFormatter removed — persisted data format must be migrated before runtime change
- Strangler fig via parallel process or feature flags for incremental rollout

**Answer**

WPF is fully ported and works on `net10.0-windows`, which eliminates the runtime and toolchain debt of staying on Framework 4.8. COM interop — Primary Interop Assemblies, Runtime Callable Wrappers, and late binding via `dynamic` — also works on modern .NET on Windows because COM interop operates at the OS layer, not the CLR layer. An app that automates `Microsoft.Office.Interop.Excel` can migrate without touching the automation code.

The blocking items to resolve before changing `TargetFramework` are: `BinaryFormatter` is removed in modern .NET — any persisted binary-serialized data must be migrated to `System.Text.Json`, protobuf, or MessagePack before switching the runtime, which requires a data migration strategy for existing saved files. `AppDomain.CreateDomain` calls must be replaced with `AssemblyLoadContext`. `Thread.Abort()` must be replaced with `CancellationToken`. The migration procedure: run `try-convert` to upgrade the project to SDK-style `.csproj`, change `TargetFramework` to `net10.0-windows`, fix all compilation errors, run the full test suite including COM automation paths on a real Office installation. For apps too large to migrate in one step, a feature-flag-gated parallel deployment runs the new modern .NET binary alongside the old one — users opt into the new version while the old one remains in production — and the old version is retired once the new one is stable.