# .NET Framework Architecture — Interview Questions (Extended Reference Bank)

Organized by the curriculum chapters under `01. .NET Framework Architecture`.  
Overlapping questions are deduplicated; each topic appears once in its best-fit chapter.  
**Gotchas** at the end are real interview traps — patterns candidates commonly miss.

> **Purpose:** Long-term reference bank — questions only (no answers). Coverage-driven; chapter count varies by topic depth.

---

## Chapter 01. .NET Platform & Evolution

1. What is .NET (the platform as a whole)?
2. What is the .NET Framework?
3. What is .NET Core?
4. What is the difference between .NET Framework and .NET Core?
5. Explain the evolution from .NET Framework → .NET Core → unified .NET (.NET 5 and above).
6. Why did Microsoft skip version 4.x when naming unified .NET?
7. What is unified .NET (.NET 5+), and how does it differ from ".NET Core" branding?
8. What is .NET Standard, and why was it introduced?
9. What problem does .NET Standard solve for library authors?
10. What is the difference between .NET Standard, a target framework moniker (TFM), and a runtime?
11. Is .NET Standard still the recommended approach for new shared libraries? Why or why not?
12. What are the different application frameworks available under .NET Framework (ASP.NET Web Forms, WCF, WinForms, WPF, etc.)?
13. What are the different application frameworks available under modern .NET (ASP.NET Core, MAUI, Blazor, etc.)?
14. What are the main goals and benefits of the modular architecture in .NET Core / modern .NET?
15. What is the difference between cross-platform .NET and Windows-only .NET Framework?
16. What is the release cadence for modern .NET (LTS vs STS)?
17. What does "maintenance mode" mean for .NET Framework 4.x?
18. What are the four high-level layers of the .NET platform (runtime, BCL, SDK/toolchain, app frameworks)?
19. What is side-by-side deployment of runtime versions, and why did .NET Core introduce it?
20. What is framework-dependent deployment vs self-contained deployment?
21. What is the difference between targeting `net48`, `net8.0`, and `netstandard2.0` in a project file?
22. When would you still choose .NET Framework 4.8 for a new project today?
23. What is the role of Kestrel in the .NET Core / modern .NET web stack?
24. How does container-first deployment relate to the .NET Core redesign?

---

## Chapter 02. CLI, Architecture & Components

1. What is the Common Language Infrastructure (CLI)?
2. What is ECMA-335, and why does it matter for .NET?
3. What is the Virtual Execution System (VES) in the CLI?
4. How do CLR, IL, and metadata relate to the VES?
5. What are the main partitions defined by the CLI specification?
6. What are the different components of modern .NET (CoreCLR, BCL, SDK, app frameworks)?
7. Explain the Common Language Runtime (CLR). What is CoreCLR?
8. What is the Framework Class Library (FCL)?
9. What is the Base Class Library (BCL)?
10. What is the difference between BCL and FCL — are they the same thing?
11. What is the layered architecture of modern .NET (application → app frameworks → BCL → runtime → OS)?
12. What is the difference between the .NET Runtime and the .NET SDK?
13. When do you need only the runtime installed vs the full SDK?
14. How is modularity achieved in .NET?
15. What is the NuGet Package Manager, and how does it relate to modularity?
16. What is the difference between a NuGet package and an assembly?
17. What is the `dotnet` CLI, and what role does it play in the toolchain?
18. What is MSBuild's role in the .NET build pipeline?
19. What is the difference between a compiler (Roslyn) and the runtime (CoreCLR)?
20. What are CLI languages (C#, F#, VB.NET), and what do their compilers produce?
21. Why can any CLI language consume types written in another CLI language?
22. What is Mono, and how does it relate to the CLI standard?
23. What is the difference between machine-wide .NET Framework install and per-app runtime deployment in modern .NET?

---

## Chapter 03. CLR, JIT, AOT & Execution Flow

1. Explain the Common Language Runtime (CLR) in detail.
2. What are the core responsibilities of the CLR (execution, JIT, memory, type safety, exceptions, loading, interop)?
3. What is managed execution vs native (unmanaged) execution?
4. What is Intermediate Language (IL) / MSIL / CIL, and how does the CLR consume it?
5. What is the JIT compiler in .NET, and what is its relation to IL/MSIL code?
6. What is RyuJIT?
7. Explain Ahead-of-Time (AOT) compilation vs Just-in-Time (JIT) compilation in .NET.
8. What is tiered JIT, and what problem does it solve?
9. What is ReadyToRun (R2R) / crossgen, and how does it differ from full Native AOT?
10. What is Native AOT (.NET), and when would you choose it over JIT?
11. Describe the overall execution flow of a .NET application (from source code to CPU execution).
12. What is a pre-stub in the JIT compilation model?
13. Why does .NET use lazy (on-first-call) JIT compilation rather than compiling entire assemblies upfront?
14. What is the CLR startup / host chain (`dotnet.exe`, hostfxr, hostpolicy, CoreCLR)?
15. What happens when you run `dotnet MyApp.dll`?
16. What is the difference between an interpreter and the .NET JIT model?
17. How does the CLR enforce type safety on IL?
18. What is IL verification, and when can it be skipped?
19. How does the .NET runtime handle security today vs Code Access Security (CAS) in .NET Framework?
20. What was Code Access Security (CAS), and why was it largely removed?
21. What is role-based security in .NET, and where is it still relevant?
22. What is the modern trust model for .NET applications (fully trusted by default on own machine)?
23. How does structured exception handling work at the CLR level?
24. What is the thread pool, and how does the CLR provide it?
25. What tradeoffs does managed execution make vs hand-written C/C++?

---

## Chapter 04. IL & Metadata

1. What is Intermediate Language (IL) / MSIL / CIL?
2. Why do .NET compilers emit IL instead of native machine code?
3. What is the difference between IL, CIL, and MSIL — are they the same?
4. What is a stack-based virtual machine, and why is IL stack-based?
5. How does IL differ from native x64/ARM64 instructions?
6. Why is IL CPU-independent / platform-portable?
7. What are common IL instruction categories (load/store, arithmetic, branching, method calls)?
8. What is the difference between `call`, `callvirt`, and `calli` in IL?
9. What is metadata in .NET assemblies?
10. Explain the role of metadata in .NET assemblies.
11. What information does metadata contain (types, methods, fields, properties, attributes, assembly references)?
12. How does the CLR use metadata during loading, verification, and JIT compilation?
13. How do reflection, serialization, and dependency injection depend on metadata?
14. What is a metadata token?
15. What is the relationship between IL and metadata in a compiled assembly?
16. What is a Portable Executable (PE) file in the .NET context?
17. What is the difference between a .NET assembly and a plain native PE DLL?
18. What are ECMA-335's rules for IL and metadata encoding?
19. How does IL support generics at runtime (reified generics) vs erased generics in Java?
20. What tools can inspect IL (`ildasm`, ILSpy, dnSpy, dotPeek)?
21. What is `unsafe` IL, and how does it relate to verification?
22. What is the difference between IL and WebAssembly or Java bytecode at a high level?

---

## Chapter 05. CTS, CLS, & Cross-Language Interoperability

1. What is the Common Type System (CTS)?
2. What is the Common Language Specification (CLS)?
3. What is the difference between CTS and CLS?
4. What is CLS compliance, and why would you mark an assembly `[CLSCompliant(true)]`?
5. What are common CLS rules (no unsigned types in public API, no public static constructors on classes, etc.)?
6. How does the .NET runtime handle cross-language interoperability?
7. Why can a C# class inherit from a VB.NET class and vice versa?
8. What is the root of all types in the CTS (`System.Object`)?
9. How does the CTS define value types vs reference types?
10. How does the CTS handle enums, delegates, interfaces, and arrays?
11. What is boxing in the CTS, and why does it matter for cross-language calls?
12. What is the difference between language-specific types (`int` in C#, `Integer` in VB) and CTS types (`System.Int32`)?
13. What visibility rules does the CTS enforce (public, assembly, family, etc.)?
14. How do generics work in the CTS across languages?
15. What happens if a public API uses a non-CLS-compliant type (e.g., `uint` in a public method)?
16. Can F#, C#, and VB.NET all implement the same interface and be used interchangeably?
17. What is the difference between syntactic interoperability and binary interoperability in .NET?
18. How does the CTS handle exceptions across language boundaries?

---

## Chapter 06. Managed & Unmanaged Code

1. What is managed code?
2. What is unmanaged code?
3. What is the difference between managed and unmanaged code?
4. What services does the CLR provide to managed code (GC, type safety, exceptions, reflection)?
5. What are typical sources of unmanaged code in a .NET application (OS APIs, native libraries, COM)?
6. Why does unmanaged code still exist in .NET applications?
7. What is P/Invoke (Platform Invoke), and how does managed code call native functions?
8. What is COM interop?
9. What is the difference between `unsafe` C# code and unmanaged native code?
10. What happens at the boundary when transitioning from managed to unmanaged code (stack walk, marshalling)?
11. What is marshalling, and why is it needed for P/Invoke?
12. What is `DllImport`, and what attributes control calling convention and string marshalling?
13. What is pinning, and why does pinning affect garbage collection?
14. What is `GCHandle.Alloc` with `Pinned`, and when is it used?
15. What is `IDisposable`, and how does it relate to unmanaged resources?
16. What is the difference between managed memory (GC heap) and unmanaged memory (`IntPtr`, native allocations)?
17. What is `SuppressFinalize`, and when should it be used?
18. What risks does calling unmanaged code introduce (memory leaks, access violations, security)?
19. What is reverse P/Invoke (native code calling back into managed code)?
20. What is the `fixed` statement in C#, and how does it relate to unmanaged interop?

---

## Chapter 07. Assemblies, Loading, Strong Naming, & GAC

1. What is an assembly in .NET?
2. What are the types of assemblies (private, shared, satellite, single-file vs multi-file)?
3. What is the difference between an assembly and a namespace?
4. What is the difference between an assembly and a DLL file?
5. What is the difference between `.exe` and `.dll` in .NET?
6. What is an assembly manifest?
7. Explain the role of metadata in .NET assemblies (cross-ref with Chapter 04).
8. What is strong naming of assemblies, and why is it important?
9. What does a strong name consist of (name, version, culture, public key token)?
10. What is the Global Assembly Cache (GAC), and what role did it play in .NET Framework?
11. Is the GAC used in modern .NET for application dependencies? Why or why not?
12. Explain assembly loading, binding, and probing in .NET.
13. What is the difference between load-from-context and load-by-name binding?
14. What is assembly probing (application base, private bin paths, culture subfolders)?
15. What is binding redirect in .NET Framework `app.config`, and what replaces it in modern .NET?
16. What is a satellite assembly, and how does localization use it?
17. What is the difference between private deployment and shared deployment?
18. How does NuGet package restore relate to assembly deployment in modern .NET?
19. What is `AssemblyLoadContext`, and how does it relate to .NET Framework AppDomains?
20. What is the difference between `Assembly.Load`, `Assembly.LoadFrom`, and `AssemblyLoadContext.LoadFromAssemblyPath`?
21. What is a multi-file assembly (`.netmodule`), and is it supported in modern .NET?
22. What is the difference between compile-time references and runtime assembly resolution?
23. What is ilmerge / single-file publishing, and how does it relate to assembly deployment?
24. What is the public key token, and where does it appear in assembly references?

---

## Chapter 08. Garbage Collection & Memory

1. Explain garbage collection in .NET.
2. What is the managed heap?
3. What is the difference between the stack and the managed heap in .NET?
4. Is it always true that "structs live on the stack and classes on the heap"? Explain.
5. What is a GC root, and what objects are considered roots?
6. What is generational garbage collection (Gen 0, Gen 1, Gen 2)?
7. Why does .NET use generational GC (weak generational hypothesis)?
8. What is the Large Object Heap (LOH), and what size threshold triggers LOH allocation?
9. How does LOH compaction differ from small-object heap compaction?
10. What is the difference between workstation GC and server GC?
11. When would you configure server GC vs workstation GC?
12. What triggers a garbage collection?
13. What is a full GC vs ephemeral generation collection?
14. What is finalization (`Finalize` / destructor), and how does it interact with GC?
15. What is the difference between `IDisposable.Dispose` and a finalizer?
16. Why is the finalizer queue a performance and correctness concern?
17. What is the dispose pattern (`Dispose(bool disposing)`)?
18. What is object pinning, and how does it affect GC compaction?
19. What is `GC.Collect()`, and why is calling it manually usually discouraged?
20. What does a memory leak look like in managed code despite having a GC?
21. What is weak reference (`WeakReference`), and when is it used?
22. What is `IDisposable` vs `IAsyncDisposable` in relation to resource cleanup?
23. What is GC pressure, and what coding patterns cause it (boxing, excessive allocations)?
24. What is the difference between ephemeral allocations and long-lived object graphs?

---

## Chapter 09. Application Domains & Isolation

1. What is an Application Domain (AppDomain) in .NET Framework?
2. What resources did an AppDomain own (loader, config, security policy, object isolation)?
3. Why did AppDomains exist in .NET Framework (IIS hosting, isolation without full process cost)?
4. What isolation did AppDomains provide vs what they did not (same process, shared CLR)?
5. How did cross-AppDomain communication work (marshal by value, marshal by reference, .NET Remoting)?
6. Why were AppDomains removed in .NET Core / modern .NET?
7. What is the relevance of AppDomains today when reading legacy documentation or interview questions?
8. What is `AssemblyLoadContext`, and how is it the modern partial replacement for AppDomains?
9. What is the difference between process isolation, AppDomain isolation, and `AssemblyLoadContext` isolation?
10. When should you use a separate OS process instead of trying to isolate in one process?
11. How do containers relate to isolation compared to AppDomains?
12. What was .NET Remoting, and what replaced it?
13. What is the default AppDomain in .NET Framework, and could you create additional ones?
14. What happens to objects when an AppDomain is unloaded?

---

## Chapter 10. Migration from .NET Framework to Modern .NET

1. Why migrate from .NET Framework to modern .NET?
2. What does "maintenance mode" mean for .NET Framework 4.8, and what support does it still receive?
3. How do you choose a migration target (.NET 8 LTS, .NET 10 LTS, STS releases)?
4. What is the difference between LTS and STS support policies for modern .NET?
5. What is the strangler fig migration pattern for large Framework applications?
6. What is big-bang (all-at-once) migration, and when is it appropriate?
7. What is dual-targeting (`net481` + `net8.0`), and when is it used during migration?
8. How does .NET Standard 2.0 act as a bridge library during migration?
9. What is the .NET Upgrade Assistant, and what does it automate?
10. What is the portability analyzer / SDK-style project migration workflow?
11. What are common breaking changes when moving from .NET Framework to .NET Core / .NET 5+?
12. What replaces `System.Web` / ASP.NET Web Forms in modern .NET?
13. What replaces WCF for new services on modern .NET (gRPC, CoreWCF, REST)?
14. What is the migration path for EF6 to EF Core?
15. What Windows-only APIs block migration, and what alternatives exist (Windows Compatibility Pack)?
16. What is `Microsoft.Windows.Compatibility`, and when does it help?
17. How does IIS + System.Web differ from Kestrel + ASP.NET Core for migrated web apps?
18. What is `PackageReference` vs `packages.config`, and why does SDK-style migration matter?
19. What workloads are often reasonable to keep on .NET Framework 4.8 indefinitely?
20. What is a phased migration plan (assess → pilot → strangler → cutover)?
21. What role does containerization play in Framework-to-modern migration?
22. What is the difference between upgrading runtime version (6 → 8 → 10) and migrating from Framework?

---

## Gotchas — .NET Architecture (Interview Traps)

1. **".NET Framework" vs ".NET" naming** — Modern unified platform is called ".NET" (5+), not ".NET Core"; Framework 4.x is a different, Windows-only product in maintenance mode.
2. **BCL vs FCL** — Often used interchangeably in interviews; BCL is the core library; FCL historically included wider framework surface.
3. **IL is not interpreted line-by-line** — CLR JIT-compiles to native code; IL isn't the steady-state execution model.
4. **Same assembly, many CPUs** — One IL assembly can run on x64, ARM64, etc.; JIT generates CPU-specific code at runtime (unless Native AOT).
5. **GAC is largely historical** — Modern .NET uses private deployment + NuGet; mentioning GAC for .NET 8 apps dates your answer.
6. **AppDomains are gone** — .NET Core+ removed them; correct modern answer is process boundaries, containers, or `AssemblyLoadContext`.
7. **Structs aren't always on the stack** — Escaped structs, boxed structs, and struct fields inside heap objects live on the heap.
8. **`GC.Collect()` in production** — Usually harmful; indicates misunderstanding of GC tuning.
9. **Strong names ≠ signing for security** — Strong names are identity/versioning, not authentication; NuGet uses different trust models.
10. **.NET Standard is fading** — New shared libraries often multi-target `net8.0;net481` instead of `netstandard2.0` alone.
11. **CAS is deprecated** — Don't describe Code Access Security as the modern .NET security model.
12. **Metadata enables reflection** — Without embedded metadata, .NET couldn't do runtime type discovery, DI, or serialization the same way.
13. **JIT warmup vs Native AOT tradeoff** — JIT: faster deploy iteration, slower cold start; Native AOT: opposite.
14. **exe vs dll** — Both can be assemblies; `.exe` has an entry point, `.dll` does not — neither term implies "managed vs native."
15. **Finalizers run non-deterministically** — Relying on `~ClassName()` for timely cleanup of files or DB connections is a bug.

---

## Appendix — Question Relocation Map

| Original draft # | Topic | Chapter |
|---|---|---|
| 1–8 | Platform, evolution, modularity | 01 |
| 9–11, 12–14, 32 | CLI, components, NuGet | 02 |
| 12, 16–17, 30–31 | CLR, JIT, AOT, execution, security | 03 |
| 15, 27 | IL, metadata | 04 |
| 19–21 | CTS, CLS, interop | 05 |
| 18 | Managed vs unmanaged | 06 |
| 22–26 | Assemblies, GAC, strong naming | 07 |
| 29 | Garbage collection | 08 |
| 28 | AppDomain | 09 |
| — | Migration (not in original draft) | 10 |

---

## Appendix — Deduplication Notes

| Overlap | Resolution |
|---|---|
| CLR explained in Q12 and execution chapter | Chapter 02: overview; Chapter 03: depth |
| Metadata in Q27 and IL chapter | Chapter 04: primary; Chapter 07: assembly context |
| Modularity/NuGet in Q8 and Q32 | Chapter 01: goals; Chapter 02: NuGet mechanics |
| Managed/unmanaged in Q18 and execution | Chapter 03: contrast; Chapter 06: full treatment |
| BCL/FCL in Q13–14 | Chapter 02 only |
| Security Q31 | Chapter 03 (CAS deprecated emphasis) |
