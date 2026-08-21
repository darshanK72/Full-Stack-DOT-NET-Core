# Karat — Interview Questions

> **Folder:** `01. .NET Framework Architecture`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Review this interop wrapper used by a high-throughput API. What are the problems, and how would you fix them in priority order?

```csharp
public class NativeBufferReader : IDisposable
{
    private IntPtr _handle;

    public NativeBufferReader(string path)
    {
        _handle = NativeMethods.OpenBuffer(path);
    }

    public byte[] ReadAll()
    {
        var len = NativeMethods.GetLength(_handle);
        var buffer = new byte[len];
        NativeMethods.ReadBytes(_handle, buffer, len);
        return buffer;
    }

    public void Dispose() => NativeMethods.CloseBuffer(_handle);
}

public static class NativeMethods
{
    [DllImport("nativebuffer.dll", CharSet = CharSet.Ansi)]
    public static extern IntPtr OpenBuffer(string path);

    [DllImport("nativebuffer.dll")]
    public static extern int GetLength(IntPtr handle);

    [DllImport("nativebuffer.dll")]
    public static extern void ReadBytes(IntPtr handle, byte[] buffer, int length);

    [DllImport("nativebuffer.dll")]
    public static extern void CloseBuffer(IntPtr handle);
}
```

*(Assume `OpenBuffer` returns `IntPtr.Zero` on failure and that the service registers this type as **Singleton** in DI.)*

---

#### Q2. (R) A long-running worker process grows memory until it OOMs. Review this cache helper. What is wrong, and what production symptoms would you expect?

```csharp
public sealed class QuoteCache
{
    private static readonly Dictionary<string, Quote> _cache = new();

    public Quote GetOrFetch(string symbol, Func<string, Quote> fetch)
    {
        if (_cache.ContainsKey(symbol))
            return _cache[symbol];

        var quote = fetch();
        _cache[symbol] = quote;
        return quote;
    }

    public void Warmup(IEnumerable<string> symbols, Func<string, Quote> fetch)
    {
        foreach (var s in symbols)
            GetOrFetch(s, fetch);
    }
}
```

---

#### Q3. (P) You publish a Linux container image for `linux-x64` using **framework-dependent** deployment. The pod starts, then crashes with `Failed to load libhostpolicy.so` / "No .NET runtimes found." What deployment model fixes this, and what trade-offs does each model have in Kubernetes?

---

#### Q4. (M) A .NET Framework 4.8 IIS app fails at startup after upgrading `Newtonsoft.Json` from 12.0.3 to 13.0.3:

> `Could not load file or assembly 'Newtonsoft.Json, Version=12.0.0.0' ...`

Another team says "put the DLL in the GAC." A third says "copy the new DLL next to the site." Diagnose the binding failure and describe the **correct fix on Framework** vs what you would do on **modern .NET** for the same dependency bump.

---

#### Q5. (D) A 400k-line ASP.NET Web Forms + WCF monolith on .NET Framework 4.8 must move to modern .NET within 18 months. Leadership wants a **big-bang** rewrite; your team prefers a **strangler fig**. What would you recommend, what risks does each approach carry, and what is a realistic first slice to migrate?

---

#### Q6. (M) An API team enables **Native AOT** publish to cut cold-start time in AWS Lambda. After deploy, startup fails:

> `System.InvalidOperationException: Dynamic code generation is not supported.`

The app uses reflection-based DI (`AddControllers()`, Newtonsoft.Json, and a plugin that loads types with `Assembly.LoadFrom`). Explain why AOT breaks here and what you would change vs choosing **ReadyToRun** or tiered JIT instead.

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

---
