# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `01. .NET Framework Architecture`

---

#### Q1. (R) Review this interop wrapper used by a high-throughput API. What are the problems, and how would you fix them in priority order?

**Answer:** The wrapper leaks native handles when construction fails, is unsafe as a singleton because `_handle` is shared across requests, and lacks defensive checks after P/Invoke — a pattern that produces intermittent access violations and handle exhaustion under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime / DI | `NativeBufferReader` registered as **Singleton** with instance `_handle` | Concurrent requests share one native handle — corruption, wrong data, crashes |
| Interop / correctness | No check for `IntPtr.Zero` after `OpenBuffer` | Null handle passed to subsequent native calls — AV or undefined native behavior |
| Resource cleanup | `Dispose` does not guard `_handle` or set it to `Zero`; no finalizer fallback | Double-close risk; handle leak if `Dispose` never called |
| Dispose pattern | Missing `Dispose(bool)` + `SuppressFinalize`; not idempotent | Violates standard pattern; harder to extend safely |
| API design | `ReadAll` allocates `byte[]` sized by native length without cap | Potential LOH pressure or OOM if native reports huge length |

**Fix (priority order):**

1. Register as **scoped** or **transient**, or redesign to a stateless factory that opens/closes per operation — never share `_handle` across requests.
2. Validate `OpenBuffer` result; throw `IOException` or custom exception if zero; only call native methods with a valid handle.
3. Implement full dispose pattern: `if (_handle != IntPtr.Zero) { CloseBuffer(_handle); _handle = IntPtr.Zero; }` and make `Dispose()` idempotent.
4. Add bounded read (max length), use `SafeHandle`/`CriticalHandle` wrapper for automatic cleanup on finalizer path if needed.
5. Consider `LibraryImport` (.NET 7+) source-generated P/Invoke for better marshalling diagnostics.

**Production takeaway:** P/Invoke bugs rarely fail unit tests — they surface as handle leaks and cross-thread native corruption when lifetime and concurrency are wrong. See **Managed & Unmanaged Code** — pinning and `IDisposable` at the boundary.

---

#### Q2. (R) A long-running worker process grows memory until it OOMs. Review this cache helper. What is wrong, and what production symptoms would you expect?

**Answer:** The static dictionary never evicts entries, so every distinct symbol fetched stays reachable for the process lifetime — a classic managed memory leak despite GC, amplified by `ContainsKey` plus indexer doing double lookups and offering no thread safety.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory / GC | Static `_cache` grows without bound | Gen 2 promotion of all cached `Quote` graphs; rising working set until OOM |
| Concurrency | No synchronization on static dictionary | Race on add/read under parallel workers — corrupted internal state or lost updates |
| Performance | `ContainsKey` then indexer | Two hash lookups per hit; unnecessary allocation churn on miss path |
| Design | No TTL, size cap, or weak references | Stale quotes forever; cannot recover memory without restart |

**Fix (priority order):**

1. Replace with bounded cache — `MemoryCache` with size/TTL limits, or `IMemoryCache` in DI with eviction policies.
2. If static is required, use `ConcurrentDictionary` and document eviction (LRU, time-based refresh).
3. Use `TryGetValue` instead of `ContainsKey` + indexer.
4. Register cache in DI as singleton with explicit capacity; monitor cache size via metrics.
5. For cross-pod consistency, move hot symbols to Redis instead of unbounded in-process state.

**Production takeaway:** GC does not fix reachable objects — static graphs are GC roots. See **Garbage Collection & Memory** Gotcha — managed leaks look like "memory grows until restart."

---

#### Q3. (P) You publish a Linux container image for `linux-x64` using **framework-dependent** deployment. The pod starts, then crashes with `Failed to load libhostpolicy.so` / "No .NET runtimes found." What deployment model fixes this, and what trade-offs does each model have in Kubernetes?

**Answer:** Framework-dependent deployment assumes a matching .NET runtime is preinstalled in the container base image; the crash means the image has your DLLs but not the shared runtime — switch to a runtime base image (e.g., `mcr.microsoft.com/dotnet/aspnet:8.0`) or publish **self-contained** so CoreCLR ships with the app.

- **Framework-dependent (FDD):** Smaller app layer; requires correct runtime image tag matching TFM (`net8.0` → `aspnet:8.0`); patches come from retagging the base image — good for many microservices sharing one runtime layer.
- **Self-contained (SCD):** Publishes runtime + app (`--self-contained true -r linux-x64`); larger image (~80–100 MB+ extra) but immune to "runtime missing" on minimal/distroless bases; you own runtime patch cadence via rebuild.
- **Native AOT:** Smallest cold start, no JIT, but reflection/DynamicDependency constraints — different tradeoff (see Q6).
- In Kubernetes, FDD + official runtime image is the default; SCD suits custom FROM `scratch`/distroless or when cluster nodes cannot guarantee runtime version alignment.

**Production takeaway:** Deployment model is part of platform architecture — FDD fails at the host chain (`dotnet`, hostfxr, hostpolicy, CoreCLR) when the container omits the runtime. See **CLR Startup and the Host Chain**.

---

#### Q4. (M) A .NET Framework 4.8 IIS app fails at startup after upgrading `Newtonsoft.Json` from 12.0.3 to 13.0.3:

> `Could not load file or assembly 'Newtonsoft.Json, Version=12.0.0.0' ...`

Another team says "put the DLL in the GAC." A third says "copy the new DLL next to the site." Diagnose the binding failure and describe the **correct fix on Framework** vs what you would do on **modern .NET** for the same dependency bump.

**Answer:** Some compiled dependency still references assembly version 12.0.0.0 while only 13.x is deployed — the CLR binding policy cannot unify them without a redirect on Framework; copying 13.x alone does not satisfy a 12.0.0.0 reference unless binding redirects or recompilation align versions.

**On .NET Framework 4.8:**

- **Root cause:** Strict assembly binding by full name (name, version, culture, token). Transitive libraries or your own DLLs may still reference 12.0.0.0 even after NuGet shows 13.0.3.
- **Correct fix:** Add/update `<bindingRedirect>` in `web.config` mapping `12.0.0.0` → `13.0.0.0`, **or** rebuild all projects against 13.0.3 so references emit the new version. Verify `bin` contains a single Newtonsoft copy.
- **GAC:** Wrong default for app-private NuGet libraries — introduces machine-wide binding complexity and admin overhead; GAC was for shared Framework-era assemblies, not typical app dependencies.
- **Copy DLL only:** Works only if every reference already targets 13.0.0.0; otherwise binding still fails at load time.

**On modern .NET (8+):**

- No `bindingRedirect` in `app.config` — dependencies resolve at **build/publish** time via NuGet graph; publish output includes the resolved version.
- Fix by updating package references, running `dotnet publish`, and ensuring no stale DLLs in output; use `<PackageReference>` centrally; `AssemblyLoadContext` loads one copy from app base.
- Plugin/isolation scenarios may use custom `AssemblyLoadContext` instead of GAC.

**Production takeaway:** Framework binding failures are loader/policy problems; modern .NET shifts resolution to the build — mentioning GAC for a .NET 8 API signals outdated architecture knowledge.

---

#### Q5. (D) A 400k-line ASP.NET Web Forms + WCF monolith on .NET Framework 4.8 must move to modern .NET within 18 months. Leadership wants a **big-bang** rewrite; your team prefers a **strangler fig**. What would you recommend, what risks does each approach carry, and what is a realistic first slice to migrate?

**Answer:** For that size and surface area, a phased strangler fig with dual-running Framework and modern .NET services is lower risk than an 18-month big-bang — big-bang defers value until the end and often misses hidden System.Web/WCF/EF6 couplings discovered too late.

**Big-bang risks:**

- Long freeze on features; business pressure to cut scope mid-flight.
- No production feedback until cutover — integration defects surface in one high-stakes weekend.
- Web Forms UI has no mechanical port; full UI rewrite is the long pole.

**Strangler benefits:**

- Route new features or high-change modules to ASP.NET Core behind a reverse proxy/API gateway.
- Keep stable Web Forms/WCF running while extracting bounded contexts (e.g., read APIs, batch jobs, authentication service).
- Dual-target shared libraries (`net481;net8.0`) or `netstandard2.0` bridge during transition.

**Realistic first slice:**

- Extract **stateless HTTP APIs** (JSON endpoints currently behind WCF or ASMX) to ASP.NET Core + Kestrel behind IIS/YARP/nginx path-based routing.
- Or migrate **batch/worker** workloads with no System.Web dependency — proves CI/CD, container hosting, and package graph on modern .NET.
- Run Upgrade Assistant / portability analysis first; inventory System.Web, EF6, WCF client/server, and Windows-only P/Invoke blockers.

**Production takeaway:** Migration architecture is a business risk decision — 18 months favors incremental proof points over a single rewrite cliff. See **Migration from .NET Framework to Modern .NET** — phased assess → pilot → strangler → cutover.

---

#### Q6. (M) An API team enables **Native AOT** publish to cut cold-start time in AWS Lambda. After deploy, startup fails:

> `System.InvalidOperationException: Dynamic code generation is not supported.`

The app uses reflection-based DI (`AddControllers()`, Newtonsoft.Json, and a plugin that loads types with `Assembly.LoadFrom`). Explain why AOT breaks here and what you would change vs choosing **ReadyToRun** or tiered JIT instead.

**Answer:** Native AOT ahead-of-time compiles a closed set of code paths and forbids runtime IL generation and unbounded reflection — MVC controller discovery, Newtonsoft's dynamic serializers, and `Assembly.LoadFrom` plugins require JIT/reflection surfaces that AOT did not trim and compile.

- **Why it breaks:** AOT publishing runs trimming; reflection-only code paths are removed unless preserved with `DynamicDependency` or `rd.xml`/`JsonSerializerContext`. `AddControllers()` scans assemblies at runtime; plugins loaded via `LoadFrom` are outside the published closure entirely.
- **If staying on AOT:** Replace Newtonsoft with `System.Text.Json` + source-generated `JsonSerializerContext`; use explicit controller registration or minimal APIs; load plugins via compile-time known set or redesign as out-of-process services; add trim/AOT analyzers and fix warnings.
- **ReadyToRun (R2R):** Pre-JITs known assemblies to native code while **keeping full reflection and dynamic load** — improves cold start vs pure JIT, smaller tradeoff than AOT; still requires runtime + JIT for dynamic paths.
- **Tiered JIT (default):** Best peak throughput and simplest model; cold start higher than AOT/R2R — often acceptable with Lambda provisioned concurrency or smaller assemblies.

**Production takeaway:** AOT is a deployment/architecture choice, not a publish flag — it conflicts with plugin models and reflection-heavy frameworks. See **CLR, JIT, AOT & Execution Flow** — JIT warmup vs Native AOT tradeoff.

---

#### Q7. (R) Review this plugin loader for a desktop host. What can go wrong at runtime in production?

```csharp
public class PluginHost
{
    private readonly List<Assembly> _loaded = new();

    public IPlugin LoadPlugin(string dllPath)
    {
        var asm = Assembly.LoadFrom(dllPath);
        _loaded.Add(asm);
        var type = asm.GetTypes().First(t => typeof(IPlugin).IsAssignableFrom(t));
        return (IPlugin)Activator.CreateInstance(type)!;
    }
}
```

*(Host and plugins each reference different versions of `Company.Contracts` from their own output folders.)*

**Answer:** `Assembly.LoadFrom` loads into the default context with load-from semantics, so plugin and host copies of `Company.Contracts` become incompatible types — `InvalidCastException` at cast time even when type names match — and assemblies are never unloaded, leaking memory on reload.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Loading model | `Assembly.LoadFrom` binds by file path, not simple name | Duplicate assembly identities; type identity mismatch across host/plugin |
| Version conflict | Host v1.0 and plugin v2.0 of same contract assembly | `IPlugin` from plugin ≠ host's `IPlugin` — cast fails at runtime |
| Discovery | `GetTypes().First(...)` | `ReflectionTypeLoadException` swallowed partially; picks wrong type if multiple implementations |
| Lifecycle | Assemblies held in `_loaded`, no `AssemblyLoadContext.Unload` | Cannot hot-reload plugins; memory grows on each reload attempt |
| Security | Loads arbitrary path without validation | Unsigned/untrusted DLL execution in host process |

**Fix (priority order):**

1. Use **`AssemblyLoadContext`** (collectible) per plugin — load plugin + dependencies in isolated context; share only stable contract abstractions loaded in default context or via `AssemblyDependencyResolver`.
2. Align contract assembly — plugins reference the **same** contract package version the host exposes; avoid copying conflicting DLLs into plugin folders.
3. Replace `GetTypes().First` with explicit plugin manifest (name in config) or `[Plugin]` attribute scan with error aggregation.
4. Validate signature/path allowlist before load; run plugins with reduced trust or out-of-process gRPC if untrusted.
5. On .NET Framework legacy, AppDomains were the isolation story — modern answer is ALC or separate process (see **Application Domains & Isolation**).

**Production takeaway:** Assembly loading is identity, not file path — Karat uses plugin hosts to test whether you know `LoadFrom` vs `AssemblyLoadContext` and why GAC/AppDomain answers are obsolete on modern .NET.

---
