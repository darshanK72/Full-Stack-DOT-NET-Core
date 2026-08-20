# Interview Answer Examples

Target style: complete opening **Answer**, explanatory bullets, no tips, no large code.

---

## Example 1 — Definition

#### Q2. What is ECMA-335, and why does it matter for .NET?

**Answer:** ECMA-335 is the international standard that formally defines the Common Language Infrastructure (CLI), including the rules for Intermediate Language (IL), metadata, assemblies, and how any compliant runtime must load and execute them.

- Because the standard is public, a C# compiler is not tied to one vendor runtime — any implementation that follows ECMA-335 (CoreCLR, Mono, and others) can run the same assemblies.
- IL and metadata layouts are documented in the spec, so decompilers, static analyzers, and profilers can rely on a stable binary format instead of reverse-engineering a proprietary layout.
- The standard is what makes .NET genuinely multi-language: F#, VB.NET, and C# are different front ends that all target the same CLI assembly model described by ECMA-335.

---

## Example 2 — Comparison

#### Q4. What is the difference between .NET Framework and .NET Core?

**Answer:** .NET Framework is the original Windows-only runtime and monolithic class library that installs machine-wide on Windows, while .NET Core was a complete rewrite built so the same applications could run on Linux and macOS using modular packages and side-by-side runtime versions. Today's unified .NET (version 5 and later) continues the .NET Core codebase and drops the "Core" name.

| | .NET Framework | .NET Core / modern .NET |
|---|---|---|
| Operating system | Windows only | Windows, Linux, macOS |
| Deployment | Machine-wide install; Global Assembly Cache (GAC) for some shared libraries | Application-local output or NuGet packages; designed for containers |
| Lifecycle | Maintenance mode on 4.8 — security fixes only | Active releases with Long-Term Support (LTS) on even-numbered versions |
| Typical stacks | Web Forms, WCF, System.Web with IIS | ASP.NET Core with Kestrel, minimal APIs, MAUI |

Both platforms compile source to IL plus metadata and execute through a Common Language Runtime-style engine; the practical differences are which operating systems you can deploy to, how libraries are versioned and shipped, and whether Microsoft still adds new features to that line.

---

## Example 3 — Pipeline / flow

#### Q11. Describe the overall execution flow of a .NET application (from source code to execution).

**Answer:** A .NET application is first compiled into a portable assembly containing IL and metadata, then the host loads the Common Language Runtime (CoreCLR), which Just-In-Time (JIT) compiles each method to native machine code when it is first invoked, and the CPU finally executes that native code while the runtime manages memory and threads.

1. **Compile** — The language compiler (for example Roslyn for C#) translates source into IL instruction streams and embeds complete type descriptions as metadata inside a `.dll` or `.exe` Portable Executable file.
2. **Launch** — Running `dotnet MyApp.dll` starts the host (`hostfxr`), which selects the requested runtime version and loads CoreCLR into the process.
3. **Load** — CoreCLR reads the assembly manifest, resolves referenced assemblies from disk, and constructs runtime type information from metadata.
4. **JIT compile** — The first time a method runs, the RyuJIT compiler translates its IL body into CPU-specific native instructions and caches that code for later calls.
5. **Execute** — The processor runs the native instructions; CoreCLR simultaneously manages the garbage-collected heap, thread scheduling, and structured exception handling.

Build-time options such as ReadyToRun embed pre-compiled native code for faster startup, and Native AOT compiles the entire application ahead of time so no JIT runs at launch.

---

## Example 4 — Gotcha

#### Gotcha 5. GAC is largely historical

**Answer:** Many people still describe the Global Assembly Cache (GAC) as the normal way .NET applications share libraries, but that model applied to .NET Framework on Windows, not to modern .NET deployment.

- In .NET Framework, strongly named assemblies could be registered in the GAC so multiple applications on one machine referenced a single shared copy; that required administrator involvement and careful version policy.
- In modern .NET, each application carries its own dependency copies in the publish folder (resolved via NuGet at build time), which makes container images and side-by-side services predictable without a machine-wide cache.
- Mention the GAC when discussing legacy Framework maintenance or migration interviews; describing it as the current sharing mechanism for .NET 8 or .NET 10 applications would be incorrect.

---

## Example 5 — Minimal code

#### Q2. What is the difference between `throw` and `throw ex`?

**Answer:** Inside a catch block, `throw;` rethrows the same exception object and preserves the original stack trace, while `throw ex;` throws a new exception instance that copies the message but resets the stack trace to the current catch line, which hides where the failure actually originated.

- Use `throw;` when you log or wrap up cleanup work but want callers and diagnostics to see the full path from the root failure.
- Use `throw new CustomException("...", ex)` when you intentionally wrap the error and pass the original as `InnerException` — not bare `throw ex`, which loses stack context without adding semantic value.

```csharp
catch (Exception ex)
{
    Log(ex);
    throw;      // preserves original stack trace
    // throw ex; // resets stack to this line — avoid
}
```
