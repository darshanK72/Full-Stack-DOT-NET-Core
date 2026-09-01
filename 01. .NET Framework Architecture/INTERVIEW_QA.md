# .NET Framework Architecture — Interview Q&A
> 33 questions · Back to [README](../README.md)

## Table of Contents
1. [What is .NET? What is .NET Framework? What is .NET Core?](#q1)
2. [How did .NET evolve from .NET Framework → .NET Core → unified .NET? What are the…](#q2)
3. [Compare .NET Framework and modern .NET across every major dimension — deployment…](#q3)
4. [What are the different components of old .NET Framework?](#q4)
5. [What are the different components of modern .NET Core / unified .NET?](#q5)
6. [What are the different application frameworks available in old .NET Framework?](#q6)
7. [What are the different application frameworks available in modern .NET?](#q7)
8. [What is .NET Standard, why was it introduced, and is it still the right choice f…](#q8)
9. [What is the LTS vs STS release cadence in modern .NET, and which version is curr…](#q9)
10. [What is the difference between framework-dependent, self-contained, and Native A…](#q10)
11. [What are the four layers of the .NET platform, and what is the difference betwee…](#q11)
12. [What roles do Roslyn, RyuJIT, MSBuild, and the `dotnet` CLI play in building and…](#q12)
13. [What is the CLR and what are its core responsibilities?](#q13)
14. [Walk me through the end-to-end execution of a .NET app — from source code to CPU…](#q14)
15. [What is tiered JIT (Tier 0 / Tier 1)? How does RyuJIT balance startup speed vs t…](#q15)
16. [What is the difference between ReadyToRun (R2R) and Native AOT? When would you p…](#q16)
17. [What is IL/CIL and why do compilers target it instead of native code? What does …](#q17)
18. [What is metadata in a .NET assembly, and why do reflection, DI containers, and s…](#q18)
19. [What is IL verification, when can it be skipped, and what replaced Code Access S…](#q19)
20. [What are the CTS and CLS, what is the difference between them, and why do they e…](#q20)
21. [What is boxing and unboxing? What is the performance cost, and when does it happ…](#q21)
22. [What is the difference between managed and unmanaged code? What services does ma…](#q22)
23. [What is P/Invoke? How does managed code call a native DLL, and what is marshalli…](#q23)
24. [What is a .NET assembly? What does it contain, and how is it different from a na…](#q24)
25. [What is the difference between a DLL and an EXE in .NET?](#q25)
26. [What is strong naming? What does a strong name consist of, and is the GAC still …](#q26)
27. [Explain generational GC — Gen 0, Gen 1, Gen 2. What is the weak generational hyp…](#q27)
28. [Is it always true that value types live on the stack and reference types on the …](#q28)
29. [What is the Large Object Heap (LOH)? How does it differ from the small-object he…](#q29)
30. [What is the difference between `IDisposable.Dispose` and a finalizer? Explain th…](#q30)
31. [What does a memory leak look like in managed code despite having a GC? What caus…](#q31)
32. [What were AppDomains, why were they removed from .NET Core, and what are the mod…](#q32)
33. [What are the main motivations for migrating from .NET Framework to modern .NET? …](#q33)

---

## Q1. What is .NET? What is .NET Framework? What is .NET Core?

What is .NET? What is .NET Framework? What is .NET Core?

These three names confuse a lot of people because they sound similar but mean different things.

**.NET** is the overall platform — a combination of a runtime (executes your code), a standard library (BCL), build tooling, and application frameworks. Think of it as the ecosystem that turns C# or F# source code into a running application. It covers web, desktop, mobile, cloud, and CLI apps.

**.NET Framework** is the original version — Windows-only, launched in 2002. It's a monolithic install that couples tightly to Windows (IIS, Win32, COM). Still supported for existing apps, but frozen at version 4.8 with no new features.

**.NET Core** was a complete from-scratch rewrite (2016–2019) to make .NET cross-platform — Windows, Linux, macOS. Modular, containerization-friendly, and no longer tied to IIS. Ended at version 3.1, then became "unified .NET".

**Today the unified platform is just called ".NET"** (versions 5, 6, 7, 8, 9, 10...). It's a continuation of .NET Core, not .NET Framework. All new projects go here.

---

## Q2. How did .NET evolve from .NET Framework → .NET Core → unified .NET? What are the core differences, and when would you still choose .NET Framework 4.8 today?

How did .NET evolve from .NET Framework → .NET Core → unified .NET?

**.NET Framework (2002)** was the original. Windows-only, installed machine-wide, tightly tied to IIS and Win32. It had everything in one big box — Web Forms, WCF, WPF, WinForms. Great at the time, but you couldn't run it on Linux and updating it meant updating the whole machine.

**.NET Core (2016)** was a complete rewrite from scratch. Cross-platform, runs on Linux and macOS, modular NuGet packages, side-by-side versions (two apps on the same server can use different runtimes). Kestrel replaced IIS as the web server. .NET Core 3.1 was the last version under that name.

**Unified .NET (.NET 5+, 2020)** just merged everything. They dropped the "Core" name since there's no longer a parallel "full Framework" being developed. Version went straight from 3.1 to 5 (skipped 4 to avoid confusion with Framework 4.x). All new development goes here.

**When to still use Framework 4.8?**
- You have Web Forms or WCF that would need a full rewrite to migrate
- Third-party COM components or Windows-only dependencies you can't replace
- Heavily regulated environments where re-certification is expensive
- Simply no business case for migration — Framework still gets security patches

Framework is not dead — it just stopped getting new features. It's "done", not gone.

---

## Q3. Compare .NET Framework and modern .NET across every major dimension — deployment, platform, web stack, desktop, security, hosting, configuration, and package management.

Compare .NET Framework and modern .NET across every major dimension.

This is the most important thing to know — interviewers love asking about the differences.

**Platform / OS**
- Framework: Windows only. It relies on Win32, COM, and IIS internals.
- Modern .NET: Windows, Linux, macOS. One codebase runs on all three. Linux containers are a first-class scenario.

**Deployment**
- Framework: Machine-wide install. All apps on the server share the same framework version. Updating it could break other apps. GAC held shared libraries.
- Modern .NET: Per-app deployment. Each app carries its own runtime in self-contained mode, or binds to a specific installed version. No shared GAC. Side-by-side versioning means apps don't interfere.

**Web Stack**
- Framework: `System.Web` + IIS. Everything ran through the IIS pipeline. `HttpContext.Current` was a static god-object. Hard to unit test. No Linux.
- Modern .NET: ASP.NET Core + Kestrel. Composable middleware pipeline. `IHttpContextAccessor` injected via DI. Testable. Kestrel can run standalone or behind Nginx/IIS as a reverse proxy.

**Desktop Apps**
- Framework: WinForms and WPF were Framework-only for a long time.
- Modern .NET: WinForms and WPF were ported and run on `net8.0-windows`. MAUI adds cross-platform mobile/desktop. Blazor Hybrid runs in a native shell.

**Performance**
- Framework: Decent for its era but no longer actively optimized.
- Modern .NET: Dramatically faster. ASP.NET Core consistently tops TechEmpower benchmarks. Span<T>, Memory<T>, SIMD, and tiered JIT brought performance close to native for many workloads.

**Security Model**
- Framework: Code Access Security (CAS) — attempted to grant permissions based on where code came from. Expensive stack walks on every sensitive call, could be bypassed, confusing to configure.
- Modern .NET: CAS is gone. All code in a process is fully trusted. Security boundaries are at the OS/container level. Authorization is done in application code (ASP.NET Core policies, claims-based auth).

**Hosting Model**
- Framework: Tightly coupled to `Global.asax`, `web.config`, and IIS application lifecycle.
- Modern .NET: `Program.cs` with generic host (`WebApplication.CreateBuilder`). Runs as a console app — no IIS required.

**Configuration**
- Framework: `web.config` / `app.config` (XML). Connection strings and app settings baked in.
- Modern .NET: `appsettings.json` + environment variables + user secrets — layered, overridable, testable with `IOptions<T>`.

**Package Management**
- Framework: `packages.config` — packages listed separately, copied to a `packages/` folder.
- Modern .NET: `PackageReference` in SDK-style `.csproj`. NuGet resolves transitively. `dotnet restore` handles everything.

**AppDomains**
- Framework: Supported AppDomains for in-process isolation (IIS used them to host multiple apps).
- Modern .NET: AppDomains removed. Use `AssemblyLoadContext` for assembly isolation, separate processes for strong isolation.

**WCF / Service Communication**
- Framework: Full WCF server stack — SOAP, NetTcp, MSMQ bindings.
- Modern .NET: WCF server not ported. Use CoreWCF for basic HTTP/SOAP, gRPC for high-performance RPC, REST for general APIs.

| Area | .NET Framework | Modern .NET |
|---|---|---|
| OS | Windows only | Windows, Linux, macOS |
| Deployment | Machine-wide, GAC | Per-app, NuGet, containers |
| Web | System.Web + IIS | ASP.NET Core + Kestrel |
| Security | CAS (removed) | OS/container boundaries |
| Config | web.config (XML) | appsettings.json + env vars |
| Packages | packages.config | PackageReference |
| AppDomains | Yes | No → AssemblyLoadContext |
| Status | Maintenance | Active, annual releases |

---

## Q4. What are the different components of old .NET Framework?

What are the different components of old .NET Framework?

The old .NET Framework was a monolithic package — everything shipped together as one Windows install.

**Core Runtime**
- **CLR (Common Language Runtime)** — the execution engine: JIT compilation, GC, type safety, exception handling, thread pool
- **BCL / FCL (Base/Framework Class Library)** — `System.*` namespaces: strings, collections, I/O, threading, XML, crypto

**Compilers & Tooling**
- C#, VB.NET, and F# compilers (pre-Roslyn era)
- MSBuild — build engine
- Visual Studio integration (no `dotnet` CLI — that came with Core)

**Application Frameworks (all bundled in the install)**
- ASP.NET Web Forms, ASP.NET MVC, ASP.NET Web API
- WCF (Windows Communication Foundation)
- WPF (Windows Presentation Foundation)
- WinForms
- Windows Workflow Foundation (WF)
- Entity Framework (classic)
- .NET Remoting (deprecated)

**Deployment Infrastructure**
- GAC (Global Assembly Cache) — machine-wide shared assembly store
- Machine-wide install (typically via Windows Update or standalone installer)
- Strong naming and binding redirects via `app.config`

Everything above was part of a single installer and versioned together. If WCF needed a bug fix, everyone got a new Framework release.

---

## Q5. What are the different components of modern .NET Core / unified .NET?

What are the different components of modern .NET Core / unified .NET?

Modern .NET is modular — you install what you need via NuGet, not one big monolithic package.

**Core Runtime**
- **CoreCLR** — the cross-platform CLR implementation: JIT (RyuJIT), GC, type system, threading
- **BCL (Base Class Library)** — `System.*` core types: collections, I/O, JSON, threading, networking
- **Native AOT runtime** — alternative to CoreCLR for ahead-of-time compiled apps

**Compilers & Tooling**
- **Roslyn** — the C# and VB.NET compiler (also powers IDE features)
- **F# compiler**
- **RyuJIT** — JIT compiler inside CoreCLR (part of the runtime, not a separate install)
- **MSBuild** — build engine
- **`dotnet` CLI** — unified command-line entry point for build, run, publish, test

**Application Frameworks (separate NuGet packages / SDK workloads)**
- ASP.NET Core — Web API, MVC, Razor Pages, Minimal APIs, gRPC, SignalR
- Blazor — component-based UI (Server, WebAssembly, Hybrid)
- Worker Services — background processing
- .NET MAUI — cross-platform mobile and desktop
- WPF and WinForms — Windows-only desktop (ported to modern .NET)
- Entity Framework Core — ORM

**Package & Deployment Infrastructure**
- NuGet — package manager, replaces GAC for app dependencies
- `dotnet publish` — produces framework-dependent, self-contained, or Native AOT output
- `AssemblyLoadContext` — replaces AppDomains for assembly isolation

The key difference: every app framework is an independent NuGet package (or SDK workload). You only pull in what you use.

---

## Q6. What are the different application frameworks available in old .NET Framework?

What are the different application frameworks in old .NET Framework?

All of these shipped bundled in the monolithic .NET Framework Windows install.

| Framework | What it's for |
|---|---|
| **ASP.NET Web Forms** | Server-rendered web UI with a drag-drop designer, postback model, ViewState. No migration path to modern .NET. |
| **ASP.NET MVC (Framework)** | Model-View-Controller web apps on Framework. Superseded by ASP.NET Core MVC. |
| **ASP.NET Web API (Framework)** | REST HTTP services on Framework. Superseded by ASP.NET Core Web API. |
| **WCF (Windows Communication Foundation)** | SOAP and binary RPC services — HTTP, NetTcp, MSMQ, named pipes bindings. Full server stack. |
| **WPF (Windows Presentation Foundation)** | XAML-based rich desktop UI with data binding. Also available on modern .NET for Windows. |
| **WinForms** | Event-driven Windows desktop with a visual designer. Also available on modern .NET for Windows. |
| **Windows Workflow Foundation (WF)** | Visual workflow engine for business process automation. Framework-only — not ported. |
| **Entity Framework (classic)** | ORM for .NET Framework. Superseded by EF Core on modern .NET. |
| **.NET Remoting** | Cross-AppDomain / cross-process object communication. Completely removed and replaced by gRPC/REST. |

The web stack (Web Forms, MVC, Web API) all ran on `System.Web`, which was tightly coupled to IIS. This is why migrating them isn't straightforward — the whole pipeline model changed.

---

## Q7. What are the different application frameworks available in modern .NET?

What are the different application frameworks in modern .NET?

Modern .NET ships the runtime and BCL as a shared foundation; application frameworks are layered on top as NuGet packages or optional SDK workloads.

| Framework | What it's for | Platform |
|---|---|---|
| **ASP.NET Core** | Web APIs, MVC, Razor Pages, Minimal APIs, gRPC, SignalR | Cross-platform, Kestrel built-in |
| **Blazor** | Component-based web UI — Server (SSR), WebAssembly (WASM), or Hybrid (in native shell) | Browser + native |
| **Worker Services** | Long-running background processes with generic host | Cross-platform, container-friendly |
| **.NET MAUI** | Single codebase for Windows, macOS, iOS, Android apps | All major platforms |
| **WPF** | XAML-based rich Windows desktop UI | Windows only |
| **WinForms** | Traditional Windows desktop with designer | Windows only |
| **Entity Framework Core** | Modern ORM — LINQ-based, Code First, multiple DB providers | Cross-platform |
| **Console / CLI** | Command-line tools and scripts — no framework overhead | Cross-platform |

The shift from Framework: ASP.NET Core uses Kestrel + composable middleware instead of IIS + System.Web. You only include what you need — a minimal API project pulls in far less than a full MVC app with views.

---

## Q8. What is .NET Standard, why was it introduced, and is it still the right choice for new shared libraries? How does it compare to multi-targeting like `net8.0;net481`?

What is .NET Standard, is it still relevant, and how does it compare to multi-targeting?

Think of **.NET Standard** as a contract — a list of APIs that any conforming .NET runtime must provide. It's not a runtime you install; it's just a spec. When you target `netstandard2.0`, the compiler restricts your code to only APIs in that contract, ensuring your library can run on anything that implements it — .NET Framework 4.6.1+, .NET Core 2.0+, Mono.

**Why it existed:** During the Framework-to-Core transition, you wanted one library that worked on both. .NET Standard 2.0 was that bridge.

**Is it still recommended?** Not for new libraries:
- **No Framework users?** Target `net8.0` or `net10.0` directly — you get all modern APIs, no compromise.
- **Need to support Framework 4.x and modern .NET?** Multi-target: `<TargetFrameworks>net481;net10.0</TargetFrameworks>`. You get Framework support and modern APIs in separate compilation paths.
- **Existing Standard library that works?** Leave it alone, it's fine. But don't start new ones on Standard.

**Key gotcha:** .NET Standard 2.1 was never implemented by .NET Framework 4.8. So it was never a real bridge — just for Core/Mono.

---

## Q9. What is the LTS vs STS release cadence in modern .NET, and which version is currently LTS?

What is the LTS vs STS release cadence, and what's the current LTS?

Modern .NET releases every November. Two flavors:

- **LTS (Long-Term Support):** Even-numbered — .NET 6, 8, **10**. Supported for 3 years. Choose this for production.
- **STS (Standard Term Support):** Odd-numbered — .NET 7, 9. Supported for ~18 months. Gets new features sooner, but you must upgrade faster.

**Current LTS: .NET 10** (released November 2025, supported until November 2028, TFM is `net10.0`).

Framework 4.8 is in "maintenance mode" — security fixes only, no new features, no new APIs.

---

## Q10. What is the difference between framework-dependent, self-contained, and Native AOT publishing?

What is the difference between framework-dependent, self-contained, and Native AOT publishing?

**Framework-dependent** — you publish just your app's DLLs. The machine running it must have the matching .NET runtime installed. Small output, fast deploy. Standard for Docker images that use a .NET base image.

**Self-contained** — you bundle the entire .NET runtime with your app. The machine doesn't need .NET installed at all. Larger output, but portable to bare machines. Good for CLIs you distribute to users.

**Native AOT** — the compiler converts all IL to native machine code at build time. No JIT at runtime. No CLR startup overhead. Produces a single fast-starting native binary. Best for serverless (Lambda cold starts), CLIs, and high-performance microservices. Trade-off: no runtime code generation, limited reflection.

| | Framework-dependent | Self-contained | Native AOT |
|---|---|---|---|
| Runtime needed | Yes (shared) | No (bundled) | No |
| Output size | Small | Large | Small (trimmed) |
| Cold start | Fast | Fast | Fastest |
| Reflection support | Full | Full | Limited |

---

## Q11. What are the four layers of the .NET platform, and what is the difference between the Runtime and the SDK?

What are the four layers of the .NET platform? Runtime vs SDK?

.NET is built in four layers stacked on each other:

1. **Runtime (CoreCLR)** — executes your code. Handles GC, JIT, type safety, threading, exception handling. This is what runs on the production server.
2. **BCL (Base Class Library)** — the standard library. `System.Collections`, `System.IO`, `System.Threading`, `System.Text.Json`, etc.
3. **SDK and Toolchain** — build-time stuff. Roslyn (compiler), MSBuild (build engine), `dotnet` CLI. Needed on developer machines and CI.
4. **App Frameworks** — on top of all that: ASP.NET Core, MAUI, Blazor, WPF, WinForms.

**Runtime vs SDK:** Production machines need the runtime. Dev machines and build servers need the SDK (which includes the runtime too). Installing just `dotnet-runtime` is enough to run apps; `dotnet-sdk` is for building them.

---

## Q12. What roles do Roslyn, RyuJIT, MSBuild, and the `dotnet` CLI play in building and running a .NET app?

What do Roslyn, RyuJIT, MSBuild, and the `dotnet` CLI each do?

These four tools sit at different points in the pipeline — three are **build-time**, one is **runtime**. Mixing them up is common in interviews, so it helps to know *when* each one runs and *what it produces*.

**Roslyn (C# / VB compiler — build time)**

Roslyn is the .NET compiler platform. When you build a project, Roslyn turns your `.cs` files into **IL (Intermediate Language) + metadata** and packs them into `.dll` / `.exe` assemblies.

- It is not just a command-line compiler — the same engine powers **IntelliSense, refactoring, code fixes, and analyzers** in Visual Studio and VS Code (OmniSharp/Roslyn).
- Roslyn also emits **XML documentation**, **source generators** consume its syntax trees, and **nullable reference type** warnings come from its semantic analysis.
- Output is **portable IL** — not x64/ARM64 machine code. That translation happens later at runtime (RyuJIT) or optionally at publish time (ReadyToRun / Native AOT).
- Other languages (F#, C++/CLI) have their own front-end compilers, but they all target the same IL + metadata format Roslyn produces for C#.

**MSBuild (build engine — build time)**

MSBuild is the orchestrator behind `dotnet build` and `dotnet publish`. It reads your `.csproj`, runs targets in order, and wires the whole build together.

- It invokes **Roslyn** (via the `CoreCompile` target) to compile source files.
- It runs **NuGet restore**, copies content files, resolves project references, and produces output under `bin/` and `obj/`.
- **`dotnet build`** is essentially "call MSBuild with the right project and configuration." You rarely invoke MSBuild directly anymore, but it is still the engine.
- **`dotnet publish`** adds deployment steps — copying the runtime, trimming (optional), single-file bundling, ReadyToRun pre-compilation — all driven by MSBuild publish targets.

**`dotnet` CLI (developer entry point — build + run)**

The `dotnet` command is a native host executable that wraps MSBuild for builds and CoreCLR for running apps.

- **Project workflow:** `dotnet new` (scaffold), `dotnet restore`, `dotnet build`, `dotnet test`, `dotnet publish`.
- **Run workflow:** `dotnet run` builds (if needed) then launches; `dotnet MyApp.dll` runs an already-built assembly.
- When you run a DLL, `dotnet` loads **hostfxr** → reads `MyApp.runtimeconfig.json` → picks the correct **shared runtime** version → starts **CoreCLR**.
- It is the unified tool across Windows, Linux, and macOS — the same commands work everywhere modern .NET is installed.

**RyuJIT (runtime JIT compiler — run time, inside CoreCLR)**

RyuJIT (Real-time Generated JIT) is the Just-In-Time compiler embedded in CoreCLR. It runs **when your app is already deployed and executing**.

- On a method's **first call**, RyuJIT compiles that method's IL body into **native machine instructions** (x64, ARM64, etc.) for the current CPU.
- Compiled native code is **cached in memory** — subsequent calls jump straight to it without re-JIT-ing every time.
- **Tiered compilation:** Tier 0 compiles quickly with minimal optimization for fast startup; hot methods get recompiled as Tier 1 with full optimization in the background (see Q15).
- RyuJIT does **not** compile your `.cs` files — that is Roslyn's job at build time. RyuJIT only sees IL that is already inside loaded assemblies.

**How they connect — build vs run**

| Phase | Tool | Input → Output |
|---|---|---|
| Build | **Roslyn** | `.cs` source → IL + metadata in `.dll` |
| Build | **MSBuild** | `.csproj` + source → compiled output in `bin/` |
| Build / deploy | **`dotnet publish`** | MSBuild + runtime layout → deployable folder |
| Run | **`dotnet MyApp.dll`** | Starts host → loads CoreCLR |
| Run | **RyuJIT** | IL in memory → native CPU instructions |

**One-line flow:** source → **Roslyn** → IL assembly → **MSBuild** packages it → **`dotnet MyApp.dll`** starts CoreCLR → **RyuJIT** → native code on the CPU.

**Quick mental model:** Roslyn and MSBuild answer "how do I *make* the app?" RyuJIT answers "how does the app *run* on this machine?" The `dotnet` CLI is the front door to both.

---

## Q13. What is the CLR and what are its core responsibilities?

What is the CLR and what are its core responsibilities?

The CLR (Common Language Runtime) is the engine that actually runs your .NET code — the layer between your IL and the OS.

**What it does:**
- **JIT compilation** — converts IL to native machine code on first call (via RyuJIT)
- **Garbage collection** — automatically frees memory you're no longer using
- **Type safety** — enforces that you don't mix incompatible types, arrays don't go out of bounds, null refs throw before corrupting memory
- **Exception handling** — the `try/catch/finally` mechanism works consistently across all .NET languages
- **Assembly loading** — finds, verifies, and loads the DLLs your app references
- **Thread pool** — managed pool of threads for `Task`, async/await, timers
- **Interop** — bridges managed code to native DLLs (P/Invoke) and COM

What the CLR does NOT do: compile your source code (that's Roslyn) or orchestrate builds (that's MSBuild).

---

## Q14. Walk me through the end-to-end execution of a .NET app — from source code to CPU instructions.

Walk me through the end-to-end execution of a .NET app.

1. You write C# code and run `dotnet build`. **Roslyn** compiles it into IL + metadata stored in `.dll` files.
2. You run `dotnet MyApp.dll`. The native `dotnet.exe` host starts, loads `hostfxr.dll`, which reads `MyApp.runtimeconfig.json` to find the right runtime version.
3. **CoreCLR initializes** — sets up the GC heap, thread pool, and type system.
4. The **assembly loader** resolves all dependencies by reading `MyApp.deps.json` and probing the app folder and NuGet cache.
5. Your `Main()` method gets called. **RyuJIT** compiles it from IL to native x64/ARM64 code on first call. Compiled native code is cached — subsequent calls run it directly.
6. As your app runs, **tiered JIT** monitors method call frequency. Hot methods get recompiled with full optimization in the background (Tier 0 → Tier 1).
7. **GC** runs periodically to collect unreachable objects.
8. On exit, the CLR runs finalizers for any objects that registered them, then shuts down.

**Important:** IL is never interpreted line-by-line in production. Even Tier 0 JIT produces real native machine code.

---

## Q15. What is tiered JIT (Tier 0 / Tier 1)? How does RyuJIT balance startup speed vs throughput?

What is tiered JIT (Tier 0 / Tier 1)? How does RyuJIT balance startup vs throughput?

The classic JIT problem: aggressive optimization takes time (hurts startup), but skipping optimization leaves performance on the table at steady state. Tiered JIT solves this.

**Tier 0** — on first call, JIT compiles the method quickly with minimal optimization. Fast to produce, not super fast to execute. Call counters are inserted to track how hot the method is.

**Tier 1** — once a method crosses the call threshold (it's "hot"), the JIT recompiles it in the background with full optimization — inlining, loop unrolling, SIMD, register allocation tuning. The new version replaces Tier 0 atomically.

**Result:** App starts quickly (Tier 0), reaches near-native throughput at steady state (Tier 1). Most methods are cold and never need Tier 1.

**ReadyToRun (R2R)** precompilation can further reduce Tier 0 work by baking pre-compiled native stubs into the assembly at publish time.

---

## Q16. What is the difference between ReadyToRun (R2R) and Native AOT? When would you pick each?

What is the difference between ReadyToRun (R2R) and Native AOT?

**ReadyToRun (R2R):**
- At publish time, the toolchain pre-compiles IL to native code and stores both in the assembly
- At runtime, the CLR uses the pre-compiled code instead of Tier 0 JIT — faster startup
- The JIT is still present as a fallback. Full reflection, dynamic code, plugins still work
- Best for: web APIs and services that need faster startup but full runtime features

**Native AOT:**
- The entire app is compiled to a native binary at build time. No CLR JIT at runtime. No IL shipped.
- Fastest possible cold start. Smallest possible image (with trimming)
- Limitations: no `Assembly.LoadFrom` at runtime, no `Reflection.Emit`, limited dynamic reflection
- Best for: serverless functions (Lambda/Azure Functions cold starts), CLIs, gRPC microservices

Think of it as: R2R = faster startup with full flexibility; Native AOT = maximum startup speed but you give up dynamic runtime features.

---

## Q17. What is IL/CIL and why do compilers target it instead of native code? What does "stack-based VM" mean?

What is IL/CIL and why do compilers target it instead of native code? What is a stack-based VM?

**IL (Intermediate Language)**, also called **CIL (Common Intermediate Language)**, is the CPU-neutral bytecode that every .NET compiler produces. C# doesn't compile directly to x64 or ARM64 — it compiles to IL, and the JIT converts IL to native code at runtime.

**Why IL instead of native code?**
- **One binary, any CPU.** The same `.dll` runs on Windows x64, Linux ARM64, macOS Apple Silicon — the JIT generates CPU-specific code on each machine.
- **Cross-language interop.** C#, F#, and VB.NET all produce the same IL format. A C# class can inherit from an F# type because they're both just IL at the binary level.
- **JIT optimization at runtime.** The JIT can detect actual CPU features on the machine (AVX, NEON, SSE) and generate tuned code — something you can't do at compile time.

**Stack-based VM:** IL doesn't use CPU registers — it uses an evaluation stack. Adding two numbers looks like: push `a`, push `b`, execute `add` (pops two, pushes result). The verifier tracks types on every stack slot, which is how it proves type safety. Real CPUs are register-based, so RyuJIT maps the stack model to actual registers during JIT.

---

## Q18. What is metadata in a .NET assembly, and why do reflection, DI containers, and serializers depend on it?

What is metadata in a .NET assembly, and why do reflection, DI, and serializers depend on it?

**Metadata** is the complete type description embedded in every `.dll`. It describes every class, interface, method, field, property, generic parameter, and custom attribute — stored in binary tables inside the PE file alongside the IL.

**Why it matters:**
- **Reflection** — `typeof(MyClass).GetProperties()` reads metadata. The runtime knows every member at runtime because metadata is always there.
- **DI containers** — ASP.NET Core's DI scans constructors to figure out what to inject. It reads parameter types from metadata — no code gen or manual registration needed.
- **Serializers** — `System.Text.Json` reads your class's property metadata to map JSON keys to C# properties without any configuration code.
- **The CLR itself** — uses metadata during assembly loading to resolve type references across assemblies.

**Key gotcha:** Metadata is what makes .NET's "magic" feel automatic. Without it, you'd need to manually register every type for everything. This is also why Native AOT with aggressive trimming can break apps — the trimmer removes metadata for types it thinks are unused.

---

## Q19. What is IL verification, when can it be skipped, and what replaced Code Access Security (CAS) in modern .NET?

What is IL verification, when is it skipped, and what replaced CAS?

**IL verification** is a static analysis pass the CLR runs before JIT-compiling a method. The verifier tracks the type on every stack slot at every instruction. If types don't check out, the verifier rejects it. This is how the CLR guarantees type safety without running the code.

**When skipped:** Code compiled with C#'s `unsafe` keyword produces unverifiable IL (raw pointer arithmetic). The CLR skips verification for it and trusts the developer.

**Code Access Security (CAS) — gone:**
CAS was a .NET Framework mechanism where permissions were assigned based on where code came from — internet zone, local intranet, trusted publisher. Every sensitive BCL call walked the call stack checking permissions. The problem: bypassable via reflection tricks, added overhead on hot paths, almost impossible to configure correctly. Modern .NET dropped it entirely.

**What replaced it:** Security boundaries are now at the OS and container level. Application authorization is ASP.NET Core middleware (claims, policies). Untrusted code runs in a separate process or container. Much simpler, much stronger.

---

## Q20. What are the CTS and CLS, what is the difference between them, and why do they enable cross-language interop?

What are the CTS and CLS, and why do they enable cross-language interop?

**CTS (Common Type System)** — the type model that all .NET languages must conform to. Defines what value types and reference types are, how inheritance works, visibility rules, how generics work at the runtime level. When C# says `int` and VB.NET says `Integer`, both compile to `System.Int32` in the CTS. Same binary type. The runtime sees one thing.

**CLS (Common Language Specification)** — a subset of the CTS. A stricter set of rules that a public API must follow to be consumable from any .NET language:
- No `uint` in public method signatures (VB.NET has no unsigned int literal)
- No member names that differ only by case
- No public static constructors on classes

**The difference:** CTS is the full type system. CLS is the interop contract for public APIs. Your internal code can use anything CTS supports; your public API should follow CLS if you want other language users to consume it without friction.

**Why cross-language works:** A C# class and a VB.NET class both produce IL targeting the CTS. The CLR loads both the same way. A C# project can reference a VB.NET assembly and inherit from its types — no special bridges needed. Mark `[assembly: CLSCompliant(true)]` to get compiler warnings when your public API breaks CLS rules.

---

## Q21. What is boxing and unboxing? What is the performance cost, and when does it happen implicitly?

What is boxing and unboxing? What does it cost and when does it happen implicitly?

**Boxing** is wrapping a value type (like `int`, `struct`) in a heap-allocated object so it can be treated as `object` or an interface type. **Unboxing** is extracting the value back out.

```csharp
int x = 42;
object boxed = x;      // Boxing — allocates on heap, copies value
int y = (int)boxed;    // Unboxing — type check + copy back
```

**Why it costs:** Every box is a heap allocation → more work for the GC. The value is copied twice (once in, once out). In hot loops this adds up fast.

**When it happens implicitly (the sneaky ones):**
- Passing a value type to a method that takes `object` — very common in old non-generic APIs
- Storing value types in `ArrayList`, `Hashtable`, or any non-generic collection
- String interpolation with value types: `$"Value: {myInt}"` — this boxes `myInt`
- `Enum` values passed to `object`-typed parameters

**How to avoid it:** Use generics everywhere (`List<int>` not `ArrayList`). Use `where T : struct` constraints. Use `Span<T>` for buffer work.

---

## Q22. What is the difference between managed and unmanaged code? What services does managed code get from the CLR?

What is the difference between managed and unmanaged code?

**Managed code** is any code compiled to IL that runs under CLR control. The CLR takes care of it.

**Unmanaged code** is native code — OS DLLs, C/C++ libraries, COM components — that runs outside CLR control. Memory management and lifecycle are entirely the developer's problem.

**What managed code gets from the CLR:**
- Automatic garbage collection — no `malloc`/`free`
- Type safety — array bounds checks, null checks, cast checks
- Structured exception handling — `try/catch/finally` across the entire call stack
- Reflection via metadata
- Thread pool and async infrastructure
- Assembly isolation via `AssemblyLoadContext`

A native DLL that leaks memory will leak. A native DLL that buffer-overflows will corrupt memory or crash the process. Managed code can call unmanaged code via P/Invoke — but at the boundary, the CLR must marshal data between managed and native formats, and GC protections don't apply during the native call.

---

## Q23. What is P/Invoke? How does managed code call a native DLL, and what is marshalling?

What is P/Invoke and how does managed code call a native DLL? What is marshalling?

**P/Invoke (Platform Invocation Services)** is the CLR mechanism for calling functions exported from native DLLs — Win32 APIs, C libraries, OS APIs.

```csharp
[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
static extern bool MoveFileW(string lpExistingFileName, string lpNewFileName);
```

**How it works:**
1. The CLR sees `[DllImport]`, loads the native DLL, and finds the exported function.
2. When the method is called, the CLR **marshals** arguments from managed to native format.
3. The native function runs.
4. Return value is marshalled back to a managed type.

**Marshalling** is the data conversion between managed and native memory layouts:
- `string` → `char*` or `LPWSTR` (null terminator, encoding handling)
- `bool` → `BOOL` (4-byte int in Win32, not 1-byte)
- Structs need `[StructLayout(LayoutKind.Sequential)]` to match the native memory layout
- Callbacks into managed code need `delegate` types with the matching calling convention

**Modern approach:** Use `[LibraryImport]` (.NET 7+) instead of `[DllImport]`. It generates marshalling code at compile time — works with Native AOT, no reflection at runtime.

---

## Q24. What is a .NET assembly? What does it contain, and how is it different from a namespace or a native DLL?

What is a .NET assembly? What does it contain, and how is it different from a namespace or native DLL?

An **assembly** is the unit of deployment in .NET — a `.dll` or `.exe` that the CLR loads and executes as one coherent package.

**What's inside:**
- **Manifest** — the assembly's identity (name, version, culture, public key) + list of all referenced external assemblies
- **Type metadata** — complete descriptions of every class, interface, method, field, property, and attribute
- **IL method bodies** — the compiled code for all your methods
- **Embedded resources** — images, strings, config files if you embedded them

**Assembly vs namespace:** A namespace is just a naming convention in source code. `System.Collections.Generic` exists in multiple assemblies. One assembly can have types in multiple namespaces. There's no physical file called "namespace".

**Assembly vs native DLL:** A native DLL contains machine code and a flat export table of function addresses. A .NET assembly contains IL + metadata in a PE wrapper. The OS can load both, but only the CLR can execute a .NET assembly.

---

## Q25. What is the difference between a DLL and an EXE in .NET?

What is the difference between a DLL and an EXE in .NET?

Both `.dll` and `.exe` are PE (Portable Executable) files and both can be valid .NET assemblies. The difference is narrow:

| | `.exe` | `.dll` |
|---|---|---|
| Entry point | Has a `Main` method (or top-level statements) | No entry point |
| Can run directly | Yes — `MyApp.exe` launches it | No — must be loaded by a host |
| Can be referenced | Yes (but uncommon) | Yes — this is their main purpose |
| In .NET | Can still be a class library published as `.exe` | Standard library format |

**In .NET specifically:**
- When you `dotnet publish` an app, the main project becomes `MyApp.exe` (or `MyApp.dll` on Linux/macOS).
- Running `dotnet MyApp.dll` works fine — the `dotnet` host loads the assembly and calls the entry point.
- A `.dll` produced by a class library project has no entry point and can't be executed directly — it's meant to be referenced by other projects.

**Common gotcha:** In .NET Framework, `.exe` was always the entry point. In modern .NET, running `dotnet MyApp.dll` is perfectly normal — the `dotnet` host acts as the launcher.

---

## Q26. What is strong naming? What does a strong name consist of, and is the GAC still used in modern .NET?

What is strong naming and is the GAC still used in modern .NET?

**Strong naming** gives an assembly a cryptographic identity using a public/private key pair. A strong name has four parts:
1. Assembly name (e.g., `MyLibrary`)
2. Version (e.g., `2.0.0.0`)
3. Culture (e.g., `neutral`)
4. Public key token (8-byte hash of the public key)

Together: `MyLibrary, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089`

**What strong naming provides:** A unique identity that includes version and publisher. The CLR can tell `MyLibrary v1` from `MyLibrary v2` from different publishers. It also detects tampering — if the DLL is modified after signing, the signature breaks.

**What strong naming does NOT provide:** It's not a security trust mechanism for execution. It's about identity and versioning, not sandboxing.

**Is the GAC still used?** No — not for application dependencies in modern .NET.
- In .NET Framework, the GAC (Global Assembly Cache) was a machine-wide store where strongly named assemblies lived. All apps could share one copy.
- In modern .NET, each app has its own private copies of all dependencies in its publish folder. No GAC probing. No machine-wide shared libraries.
- Strong names still exist and are still required for libraries targeting .NET Framework or .NET Standard, but are not a requirement for modern .NET apps.

**Interview gotcha:** Mentioning GAC as something you configure for .NET 8+ apps is a red flag.

---

## Q27. Explain generational GC — Gen 0, Gen 1, Gen 2. What is the weak generational hypothesis and what triggers a collection?

Explain generational GC — Gen 0, 1, 2. What is the weak generational hypothesis?

The .NET GC is **generational**. The core insight: **most objects die young**. Request buffers, LINQ iterators, temp strings — they're created, used, and immediately unreachable. Scanning the entire heap every collection would be wasteful.

The GC divides the heap into generations and collects younger generations more often:

| Generation | What lives here | Collected |
|---|---|---|
| **Gen 0** | Brand new objects | Very often (milliseconds) |
| **Gen 1** | Survived one Gen 0 collection | Occasionally |
| **Gen 2** | Long-lived — caches, singletons, statics | Rarely (seconds to minutes) |

**How it works:**
- New objects go to Gen 0. When Gen 0 fills, a Gen 0 collection runs — fast, collects most of them.
- Survivors get promoted to Gen 1. When Gen 1 fills, Gen 0 + Gen 1 are collected.
- Long survivors reach Gen 2. A full GC (Gen 0+1+2) is the most expensive — avoid unnecessarily promoting objects to Gen 2.

**What triggers a collection:** Gen 0 segment fills up, system memory pressure, or explicit `GC.Collect()` (avoid this in production).

**Weak generational hypothesis** — just the formal name for the observation that "most objects die young." It's why generational GC is efficient: you usually collect a small, mostly-dead area instead of scanning the whole heap.

---

## Q28. Is it always true that value types live on the stack and reference types on the heap? Where does this break?

Is it always true that value types live on the stack and reference types on the heap?

**No, this is an oversimplification.** The more accurate rule: *locals that don't escape to the heap may be stack-allocated — everything else follows the object graph.*

**Where value types end up on the heap:**
- **Boxing:** `object o = 42;` — the int is now a heap object
- **Fields inside a class:** `class Foo { int x; }` — `x` lives inside the heap-allocated `Foo` object
- **Closures/lambdas:** An `int` captured by a lambda is moved to a compiler-generated heap object
- **`async` methods:** Local variables are promoted to a heap-allocated state machine struct when the method first awaits

**Where reference types can avoid the heap (rarely):**
- JIT escape analysis can stack-allocate small objects when they provably don't escape the method
- `stackalloc` + `Span<T>` explicitly stack-allocates a buffer

**The right mental model:** Reference-type locals store a *pointer* on the stack; the *object* is on the heap. Value-type locals store their bits on the stack — unless they're inside another object on the heap, boxed, or captured by a closure.

---

## Q29. What is the Large Object Heap (LOH)? How does it differ from the small-object heap, and what size triggers it?

What is the Large Object Heap (LOH)? How does it differ from the small-object heap?

Objects that are **≥ 85,000 bytes** go straight to the **Large Object Heap** — mostly large arrays like `byte[]` for network buffers, images, or big `string` objects.

**Key differences:**

| | Small Object Heap | Large Object Heap |
|---|---|---|
| Compacted after GC? | Yes | No (by default) |
| Collected when? | Gen 0, 1, or 2 | Gen 2 only |
| Fragmentation risk | Low | High over time |

**Why no compaction?** Moving a 10 MB array takes real time and would cause noticeable GC pauses. So the LOH stays fragmented. The GC reuses freed gaps, but if new allocations don't fit any gap, you can get `OutOfMemoryException` even with plenty of total free LOH space.

**How to handle it:** Don't allocate large arrays repeatedly. Use `ArrayPool<byte>.Shared.Rent()` to borrow a buffer and return it after use — no new LOH allocation. If you must compact the LOH: `GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce`.

---

## Q30. What is the difference between `IDisposable.Dispose` and a finalizer? Explain the `Dispose(bool disposing)` pattern.

What is the difference between `IDisposable.Dispose` and a finalizer? Explain the `Dispose(bool)` pattern.

**Finalizer** (`~MyClass()` in C#):
- Called by the GC *eventually* — you have no control over when
- Only purpose: safety net to release unmanaged resources if the developer forgot to call `Dispose`
- Objects with finalizers are kept alive one extra GC cycle — more memory pressure
- Never rely on finalizers for timely cleanup (file handles, DB connections, locks)

**`IDisposable.Dispose`:**
- Called deterministically by the developer (or via `using` / `await using`)
- Releases both managed and unmanaged resources
- Fast, predictable, under your control

**The `Dispose(bool disposing)` pattern:**

```csharp
public class ResourceHolder : IDisposable {
    private bool _disposed;

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // Skip finalizer — already cleaned up
    }

    protected virtual void Dispose(bool disposing) {
        if (_disposed) return;
        if (disposing) {
            // Safe to access other managed objects here
            _managedResource?.Dispose();
        }
        // Always clean up unmanaged resources
        CloseNativeHandle();
        _disposed = true;
    }

    ~ResourceHolder() {
        // Called by GC — don't access managed objects here (they may already be collected)
        Dispose(disposing: false);
    }
}
```

**`GC.SuppressFinalize`:** After `Dispose()` runs, the finalizer isn't needed. This removes the object from the finalization queue, avoiding the extra GC cycle. Always call it in `Dispose()`.

---

## Q31. What does a memory leak look like in managed code despite having a GC? What causes GC pressure?

What does a memory leak look like in managed code? What causes GC pressure?

The GC only collects objects with **no GC roots pointing to them**. A memory leak means objects are still reachable — they just shouldn't be.

**Common leak patterns:**

- **Static collections that never shrink:**
  ```csharp
  static List<byte[]> _cache = new(); // Grows forever → Gen 2 fills up
  ```

- **Event subscriptions not removed:**
  ```csharp
  button.Click += OnClick; // button holds reference to your handler
  // If button outlives your object, your object stays alive
  ```
  Fix: unsubscribe in `Dispose`, or use weak event patterns.

- **Closures capturing large objects:** A long-lived lambda captures the whole enclosing scope.

- **Unbounded caches:** A `Dictionary<string, BigObject>` that only grows. Use `MemoryCache` with size limits or `WeakReference<T>`.

**GC pressure (performance degradation, not a leak):**
- Boxing in tight loops: `int → object` on every iteration = lots of small heap allocations
- `new byte[bigSize]` in a loop → constant LOH allocations → fragmentation
- LINQ chains creating many intermediate arrays
- `string` concatenation with `+` in a loop — use `StringBuilder`
- `new Task<T>` when you could use `ValueTask<T>`

**How to diagnose:** Use `dotnet-trace`, `dotnet-dump`, PerfView, or Visual Studio Diagnostic Tools for heap snapshots. Look for unexpectedly large Gen 2 or LOH usage.

---

## Q32. What were AppDomains, why were they removed from .NET Core, and what are the modern alternatives?

What were AppDomains, why were they removed, and what are the modern alternatives?

**AppDomains** were a .NET Framework mechanism for in-process isolation. IIS used them to host multiple web apps inside one `w3wp.exe` — each site got its own AppDomain with its own static variables, config, and assembly set. You could unload an AppDomain without killing the process.

**Why they were removed from modern .NET:**
- Every single BCL type had to be marshalled across AppDomain boundaries — an enormous ongoing maintenance cost
- They were never truly isolated: a native crash in one AppDomain killed the whole process
- In the cloud era, process and container boundaries provide stronger isolation with lower complexity
- The implementation was deeply coupled to .NET Framework internals and couldn't be ported cleanly

**Modern alternatives:**

| What you need | Modern approach |
|---|---|
| Load/unload a set of assemblies (plugins) | `AssemblyLoadContext` — create one per plugin, dispose to unload |
| Crash isolation | Separate OS processes (communicate via gRPC, named pipes, message bus) |
| Multi-tenant isolation | Containers / Kubernetes namespaces |
| Per-request state isolation | `AsyncLocal<T>` — flows through async context |

**`AssemblyLoadContext`** is the closest in-process equivalent: each ALC has its own assembly set; disposing it unloads all those assemblies. But it does NOT isolate static variables or provide security policy — just assembly loading. For anything stronger, use a separate process.

---

## Q33. What are the main motivations for migrating from .NET Framework to modern .NET? What is the strangler fig pattern, and what are the most common breaking changes to watch for?

Why migrate from .NET Framework to modern .NET? What is the strangler fig pattern and what breaks?

**Why migrate?**
- **Performance:** ASP.NET Core is dramatically faster — consistently top-tier on benchmarks
- **Cross-platform:** Linux containers cut cloud hosting costs. Framework can't run in Linux.
- **New features:** Language features (C# 9+), runtime features (Native AOT, tiered JIT, `Span<T>`), and security patches only come to modern .NET
- **Framework is frozen:** No new APIs, no performance improvements after 4.8

**Strangler Fig pattern:** Instead of a risky big-bang rewrite, you incrementally replace pieces of the old app with new modern .NET services. A reverse proxy like **YARP** sits in front and routes:
- Old routes → legacy Framework app (still running)
- Migrated routes → new modern .NET service

Over time, you strangle the old app route by route until nothing is left. Low risk, always deployable.

**Most common breaking changes:**

- **`System.Web` / Web Forms / `HttpContext.Current`** — no migration path. Must rewrite as Razor Pages, MVC, or Blazor.
- **`BinaryFormatter`** — removed. Use `System.Text.Json`, protobuf, or `MessagePack`.
- **WCF server stack** — not ported. Use CoreWCF for HTTP/SOAP, or redesign as gRPC/REST.
- **`AppDomain`** — removed. Replace with `AssemblyLoadContext` or separate process.
- **`Thread.Abort()`** — removed. Use `CancellationToken`.
- **Configuration** — `web.config` → `appsettings.json` + options pattern.
- **`System.Data.SqlClient`** → `Microsoft.Data.SqlClient` NuGet package.
- **`packages.config`** → `PackageReference` (SDK-style project migration needed first).

**Tools:**
- **GitHub Copilot modernize-dotnet agent** — generates compatibility analysis and migration plan
- **`try-convert`** — converts legacy `.csproj` to SDK-style format
- **CA1416 platform analyzer** — flags Windows-only API calls
- **`Microsoft.Windows.Compatibility`** package — adds Windows-specific APIs for Windows-only migration targets

---
