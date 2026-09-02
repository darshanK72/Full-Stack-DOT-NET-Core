# C# Abstract Classes & Interfaces — Interview Q&A


## Table of Contents

1. [Q1. What is an abstract class in C#, and how does it differ from a concrete class?](#q1-what-is-an-abstract-class-in-c-and-how-does-it-differ-from-a-concrete-class)
2. [Q2. What is an interface in C#, and what could it contain before and after C# 8?](#q2-what-is-an-interface-in-c-and-what-could-it-contain-before-and-after-c-8)
3. [Q3. When would you choose an abstract class over an interface, and vice versa?](#q3-when-would-you-choose-an-abstract-class-over-an-interface-and-vice-versa)
4. [Q4. What is an abstract method and an abstract property, and who must implement them?](#q4-what-is-an-abstract-method-and-an-abstract-property-and-who-must-implement-them)
5. [Q5. What are default interface methods (C# 8), and what problem do they solve?](#q5-what-are-default-interface-methods-c-8-and-what-problem-do-they-solve)
6. [Q6. What is explicit interface implementation, and when is it necessary?](#q6-what-is-explicit-interface-implementation-and-when-is-it-necessary)
7. [Q7. What is the Interface Segregation Principle, and how does a "kitchen sink" interface violate it?](#q7-what-is-the-interface-segregation-principle-and-how-does-a-kitchen-sink-interface-violate-it)
8. [Q8. How do covariant (`IEnumerable<out T>`) and contravariant (`IComparer<in T>`) interfaces work in C#?](#q8-how-do-covariant-ienumerableout-t-and-contravariant-icomparerin-t-interfaces-work-in-c)
9. [Q9. What is the `IDisposable` pattern, and how does an abstract base class help implement it?](#q9-what-is-the-idisposable-pattern-and-how-does-an-abstract-base-class-help-implement-it)
10. [Q10. What are static abstract interface members (C# 11), and what enables generic math?](#q10-what-are-static-abstract-interface-members-c-11-and-what-enables-generic-math)
11. [Q11. What is the difference between `abstract override` and `override` in a derived abstract class?](#q11-what-is-the-difference-between-abstract-override-and-override-in-a-derived-abstract-class)
12. [Q12. When should you use both an abstract class and an interface together?](#q12-when-should-you-use-both-an-abstract-class-and-an-interface-together)
13. [Q13. An `IExportable` library interface adds a default method `ExportWithMetadata()`. A service targeting `netstandard2.0` references the updated package. What breaks?](#q13-an-iexportable-library-interface-adds-a-default-method-exportwithmetadata-a-service-targeting-netstandard20-references-the-updated-package-what-breaks)
14. [Q14. A storage service calls `invoice.GetName()` expecting a file-safe name, but gets `"Invoice: Q1-2025"` instead. Why?](#q14-a-storage-service-calls-invoicegetname-expecting-a-file-safe-name-but-gets-invoice-q1-2025-instead-why)
15. [Q15. An abstract base class `NotifierBase` already has `Email` and `SMS` as subclasses. A new `SmsNotifier : INotificationSender` is introduced from an external library. How do you bridge the two hierarchies without breaking LSP?](#q15-an-abstract-base-class-notifierbase-already-has-email-and-sms-as-subclasses-a-new-smsnotifier-inotificationsender-is-introduced-from-an-external-library-how-do-you-bridge-the-two-hierarchies-without-breaking-lsp)
16. [Q16. A document archive needs `SpreadsheetDocument` with shared `Title`/`CreatedOn`, optional CSV export, and a separate audit trail. How would you model this?](#q16-a-document-archive-needs-spreadsheetdocument-with-shared-titlecreatedon-optional-csv-export-and-a-separate-audit-trail-how-would-you-model-this)
17. [Q17. `DocumentProcessor` is hard-coupled to `PdfRenderer` and `List<InvoiceDocument>`. Unit tests are slow because they hit disk. How do you introduce test seams?](#q17-documentprocessor-is-hard-coupled-to-pdfrenderer-and-listinvoicedocument-unit-tests-are-slow-because-they-hit-disk-how-do-you-introduce-test-seams)
18. [Q18. You need to add `PushNotificationSender` to an existing notification system where `Email` and `SMS` already extend `NotifierBase`. Option A extends the abstract base; Option B uses a static helper. How do you decide?](#q18-you-need-to-add-pushnotificationsender-to-an-existing-notification-system-where-email-and-sms-already-extend-notifierbase-option-a-extends-the-abstract-base-option-b-uses-a-static-helper-how-do-you-decide)

---
## Foundation Questions

---

## Q1. What is an abstract class in C#, and how does it differ from a concrete class?

**Concepts**
- Cannot be instantiated directly
- May contain abstract and non-abstract members
- Abstract members have no body in the abstract class
- Derived class must implement all abstract members
- Provides shared implementation + enforced contract

**Answer**

An abstract class is declared with the `abstract` modifier and cannot be instantiated directly — `new AbstractDocument()` is a compile error. Its purpose is to serve as a base that provides shared implementation (concrete methods, fields, constructors) alongside abstract members that derived classes must implement. Abstract methods and properties have no body in the abstract class; they declare a contract that every non-abstract derived class must fulfill. If a derived class does not implement all abstract members, it must also be declared `abstract`. This makes abstract classes ideal when a group of related types share significant implementation while each requiring customized behavior at specific extension points — for example, a `Document` base with a concrete `Title` property and `CreatedOn` timestamp, but an `abstract string RenderContent()` that each document type implements differently.

---

## Q2. What is an interface in C#, and what could it contain before and after C# 8?

**Concepts**
- Before C# 8: method, property, indexer, and event declarations only (no bodies)
- C# 8+: default interface methods, static members
- C# 11+: static abstract interface members
- All members implicitly public (pre-C# 8)
- Cannot hold instance fields or constructors

**Answer**

An interface declares a contract — a set of members that implementing types must provide. Before C# 8, interfaces could only contain method, property, indexer, and event signatures without any implementation, and all members were implicitly `public`. C# 8 introduced default interface methods (DIMs), which allow interface members to have a default implementation body. This enables backward-compatible evolution of interfaces: adding a new method with a default body does not force all existing implementers to update. C# 11 extended this with static abstract interface members, enabling generic math and operator overloading through interfaces (`IAdditionOperators<T,T,T>`). Despite these additions, interfaces still cannot hold instance fields or constructors — these remain exclusive to classes and structs.

---

## Q3. When would you choose an abstract class over an interface, and vice versa?

**Concepts**
- Abstract class: IS-A, shared state, single inheritance
- Interface: CAN-DO (capability), multiple implementation, cross-hierarchy contracts
- Abstract class allows constructors and fields
- Interface allows multiple simultaneous implementations
- Decision rule: shared state → abstract class; cross-cutting behavior → interface

**Answer**

Choose an abstract class when the derived types have a genuine IS-A relationship, share significant state (fields, properties), or need shared constructor logic. The classic indicator is when you find yourself duplicating field initialization across multiple classes — that duplication belongs in an abstract base. Abstract classes also enforce a single inheritance constraint, which is appropriate when you need to control that derived types only specialize from one lineage. Choose an interface when the contract represents a capability or role that a type plays (`IDisposable`, `IComparable`, `INotificationSender`) rather than its identity, when the capability must apply across unrelated class hierarchies, or when you need multiple simultaneous implementations. A practical decision rule: if you need to share fields or constructors, use an abstract class; if you need to express a cross-cutting contract that unrelated types should fulfill, use an interface. In many real designs you use both: an abstract class for the shared implementation and an interface for the external contract.

---

## Q4. What is an abstract method and an abstract property, and who must implement them?

**Concepts**
- No body (not even `{}`), just the signature
- Derived concrete class must override with `override`
- Abstract class can also be a derived abstract class (defers implementation)
- Compile error if not implemented by first concrete type in chain
- `abstract` and `virtual` are mutually exclusive on the same member

**Answer**

An abstract method or property is declared without a body — it consists only of the signature followed by a semicolon: `public abstract string RenderContent();`. Any class that inherits an abstract member and is itself not abstract must provide an implementation using `override`. If a derived class is also abstract, it may defer the implementation further. The first concrete (non-abstract) class in the chain must have provided `override` for every inherited abstract member, or the compiler reports an error. Abstract members are implicitly virtual — `abstract` and `virtual` cannot appear together on the same declaration. Abstract properties require the derived class to implement only the accessors (`get`, `set`, or both) that the abstract property declares; if the abstract property has only `get`, the derived class only overrides `get`.

---

## Q5. What are default interface methods (C# 8), and what problem do they solve?

**Concepts**
- Method body in interface, used when type doesn't override
- Backward-compatible interface evolution
- Implementing type can still override
- Not inherited by classes (must cast to interface to call default)
- `netstandard2.0` runtime does not support DIMs

**Answer**

Default interface methods allow an interface to provide a method body that is used by any implementing type that does not supply its own override. The primary purpose is backward-compatible evolution: a library team can add a new method to a published interface without breaking the thousands of existing implementations that were compiled before the new method existed. Those old implementations get the default behavior automatically. New implementations can override the default to provide specialized behavior. A critical subtlety is that default interface methods are not inherited by classes — they are only callable through an interface reference. If `Document doc = new InvoiceDocument()`, you cannot call `doc.ExportWithMetadata()` unless `InvoiceDocument` explicitly overrides it; you must cast to the interface: `((IExportable)doc).ExportWithMetadata()`. This behavior confuses developers coming from virtual method backgrounds. Additionally, assemblies targeting `netstandard2.0` or .NET Framework cannot use DIMs because the feature requires runtime support added in .NET Core 3.0.

---

## Q6. What is explicit interface implementation, and when is it necessary?

**Concepts**
- `void IInterface.Method()` syntax — no access modifier
- Only accessible through interface reference
- Resolves member name conflicts between two interfaces
- Hides implementation from class's public surface
- Used when two interfaces share a method name with different semantics

**Answer**

Explicit interface implementation binds a member to a specific interface using qualified syntax: `string INamedDocument.GetName() { return _fileSafeName; }`. The method has no access modifier and is only accessible when the object is referenced through the interface type. This serves two purposes. First, when a class implements two interfaces that both declare a method with the same name but different intended semantics, explicit implementation allows each interface's contract to be satisfied independently. Second, it hides implementation details that should not appear on the class's public surface — callers of `InvoiceDocument` directly see only its public `GetName()` while consumers of `INamedDocument` get the file-safe version. The common bug is calling `invoice.GetName()` (public instance method) when `((INamedDocument)invoice).GetName()` (explicit implementation) is what returns the correct value, leading to a wrong file name in storage services.

---

## Q7. What is the Interface Segregation Principle, and how does a "kitchen sink" interface violate it?

**Concepts**
- ISP: clients should not depend on methods they don't use
- Fat interface forces unnecessary implementation
- Breaks extensibility — adding one method forces all implementers to update
- Fine-grained interfaces (IExportable, IPrintable, ISignable)
- Role interfaces vs general-purpose interfaces

**Answer**

The Interface Segregation Principle (ISP) states that no implementing type should be forced to depend on methods it does not use. A "kitchen sink" interface like `IDocumentCapabilities` with `Export`, `Print`, `GetName`, `RenderPdf`, `SendToPrinter`, and `SignWithCertificate` forces every implementing class to provide all six methods, even though invoices may not need printing and reports may not need signing. This creates three problems. First, classes must implement stub/throw methods for capabilities they do not support, which violates LSP if callers invoke those stubs. Second, adding a new method to the interface is a breaking change for all existing implementations. Third, `ExportOrchestrator` that receives `IDocumentCapabilities` and uses only `Export` carries an unnecessarily broad dependency. The fix is to split into role interfaces: `IExportable`, `IPrintable`, `ISignable`. Each class implements only the interfaces it supports. `ExportOrchestrator` depends only on `IExportable`, making its dependencies minimal and its test doubles trivial to write.

---

## Q8. How do covariant (`IEnumerable<out T>`) and contravariant (`IComparer<in T>`) interfaces work in C#?

**Concepts**
- `out T` — covariant, appears only in return positions
- `in T` — contravariant, appears only in parameter positions
- `IEnumerable<Manager>` assignable to `IEnumerable<Employee>` (covariance)
- `IComparer<Employee>` assignable to `IComparer<Manager>` (contravariance)
- Invariant by default — `IList<Manager>` not assignable to `IList<Employee>`

**Answer**

Covariance (`out T`) means a generic interface can be assigned to a variable of a broader generic type, provided the type parameter appears only in output (return) positions. Because `IEnumerable<T>` is declared `IEnumerable<out T>`, an `IEnumerable<Manager>` is implicitly convertible to `IEnumerable<Employee>` — you can enumerate managers as employees. This is safe because the sequence is read-only; you never write a non-`Manager` back in. Contravariance (`in T`) works in reverse: `IComparer<T>` is `IComparer<in T>`, so an `IComparer<Employee>` can be used where `IComparer<Manager>` is expected. An employee comparer can compare any two employees, including two managers, so accepting an employee comparer where a manager comparer is needed is safe. `IList<T>` is invariant (neither `in` nor `out`) because it has both read and write operations — allowing `IList<Manager>` as `IList<Employee>` would let callers insert a non-`Manager` `Employee`, breaking type safety.

---

## Q9. What is the `IDisposable` pattern, and how does an abstract base class help implement it?

**Concepts**
- `IDisposable.Dispose()` for deterministic resource cleanup
- `using` statement and `using` declaration
- Virtual protected `Dispose(bool disposing)` pattern
- Managed vs unmanaged resource cleanup
- Suppressing finalizer after dispose

**Answer**

`IDisposable` declares a single `Dispose()` method for deterministic cleanup of resources that should be released before garbage collection. The standard implementation pattern for a base class that may hold resources is to implement `IDisposable` publicly and expose a `protected virtual void Dispose(bool disposing)` method that derived classes override. The public `Dispose()` calls `Dispose(true)` and `GC.SuppressFinalize(this)` to cancel the finalizer. The `disposing` flag distinguishes between a deterministic `Dispose()` call (where managed resources can be freed) and a finalizer call (where managed objects may already be GC'd). An abstract base class is ideal here: it provides the scaffolding once and derived classes override only `Dispose(bool)` to free their specific resources, calling `base.Dispose(disposing)` to ensure the base also cleans up. The `using` statement guarantees `Dispose()` is called even if an exception is thrown, making resource cleanup safe.

---

## Q10. What are static abstract interface members (C# 11), and what enables generic math?

**Concepts**
- `static abstract` in interfaces
- Operators and factory methods declared on interfaces
- `IAdditionOperators<T,T,T>`, `INumber<T>` in BCL
- Consuming via generic type constraints
- Enables type-safe generic arithmetic

**Answer**

C# 11 introduced static abstract interface members, allowing interfaces to declare static methods and operators that implementing types must provide: `public interface IAddable<T> { static abstract T operator+(T left, T right); }`. Any type that implements `IAddable<T>` must supply that operator at the type level. Consuming code can then be generic over any such type: `T Sum<T>(IEnumerable<T> items) where T : IAddable<T>`. The .NET 7+ BCL shipped a comprehensive set of these interfaces — `INumber<T>`, `IAdditionOperators<T,T,T>`, `IMultiplyOperators<T,T,T>` — enabling fully generic numeric algorithms over `int`, `double`, `decimal`, and custom numeric types without boxing or reflection. In .NET 10, these interfaces are mature and widely used in SIMD intrinsics, linear algebra libraries, and financial calculation code. The feature requires the runtime support introduced in .NET 7 and cannot be used against `netstandard2.0`.

---

## Q11. What is the difference between `abstract override` and `override` in a derived abstract class?

**Concepts**
- `abstract override` — inherits virtual slot, re-abstracts it
- Deferred implementation to next concrete class
- Use case: inserting a new abstract layer in a hierarchy
- `override` seals the abstract chain at this level
- Rare pattern, used in framework design

**Answer**

In a class hierarchy where `A` declares a `virtual` method and `B` inherits from `A` and is itself abstract, `B` can use `abstract override` to re-abstract the method: `public abstract override string RenderContent()`. This means `B` takes ownership of the virtual slot from `A` but does not provide an implementation — it defers the obligation to the next concrete class that inherits from `B`. This removes the inherited default implementation from the virtual slot and forces all concrete subclasses to provide their own. This pattern appears in frameworks where an intermediate abstract layer wants to strengthen the contract (e.g., change return type conventions via documentation) without providing an implementation. A plain `override` in an abstract class is equally valid and provides a default that concrete subclasses can use or further override. The distinction matters when you want to prevent accidental reliance on a base implementation that may not be meaningful for the category of types that intermediate class represents.

---

## Q12. When should you use both an abstract class and an interface together?

**Concepts**
- Interface: external contract (dependency injection, mocking)
- Abstract class: shared implementation (state, constructor, template method)
- Typical pattern: `IDocument` interface + `DocumentBase` abstract class
- Interface for consumers; abstract class for implementers
- BCL examples: `Stream`, `TextReader`

**Answer**

The most powerful combination uses an interface for the external-facing contract and an abstract class for the shared implementation. For example, declare `IDocument` as the interface consumers and DI registrations use, and `DocumentBase` as an abstract class that implements `IDocument` and provides shared state (`Title`, `CreatedOn`) and common behavior (validation in the constructor, logging). Concrete types like `InvoiceDocument` and `SpreadsheetDocument` inherit from `DocumentBase` and implement the abstract method `RenderContent()`. Consumers and DI containers interact only with `IDocument`, keeping them loosely coupled. The abstract class eliminates code duplication across concrete types. This is the pattern used throughout the BCL: `Stream` (abstract class) implements `IDisposable` and `IAsyncDisposable`; `FileStream`, `MemoryStream` inherit the shared scaffolding. When a type you do not control already occupies the base class slot, you must use an interface alone and compose the shared behavior using helper classes.

---

## Gotcha Questions

---

## Q13. An `IExportable` library interface adds a default method `ExportWithMetadata()`. A service targeting `netstandard2.0` references the updated package. What breaks?

**Concepts**
- Default interface methods require .NET Core 3.0+ runtime
- `netstandard2.0` does not support DIMs at runtime
- Compile may succeed (SDK resolution) but runtime fails
- `NotSupportedException` or `MissingMethodException` at runtime
- Forward-compatibility considerations when publishing NuGet packages

**Answer**

Default interface methods (DIMs) are a compiler and runtime feature. The C# compiler in a modern SDK can produce the IL for a DIM in the library targeting `netstandard2.0`, because `netstandard2.0` is a compile-time abstraction. However, the runtime that executes the service — if it targets `netstandard2.0` on .NET Framework 4.x — does not support the virtual method dispatch mechanism that DIMs require. At runtime, invoking the default method on an instance cast to the interface may produce `MissingMethodException` or a `BadImageFormatException` depending on the runtime version. Even on a .NET 6+ runtime, a DIM is not callable through a derived class reference without casting to the interface, which surprises developers expecting normal virtual method semantics. The documentation obligation is: clearly state the minimum required TFM (`net6.0` or later) for any API that uses DIMs, and test the NuGet package against every claimed target framework using multi-targeted test projects.

---

## Q14. A storage service calls `invoice.GetName()` expecting a file-safe name, but gets `"Invoice: Q1-2025"` instead. Why?

**Concepts**
- Explicit interface implementation accessible only via interface reference
- Public `GetName()` and `INamedDocument.GetName()` are different methods
- Calling through class reference invokes public method
- Calling through interface reference invokes explicit implementation
- Common source of wrong behavior in service layers

**Answer**

`InvoiceDocument` has two `GetName` methods: a public `string GetName()` returning a human-readable label like `"Invoice: Q1-2025"`, and an explicit implementation `string INamedDocument.GetName()` returning the file-safe name. When `ResolveFileName` receives `InvoiceDocument` by its concrete type and calls `invoice.GetName()`, C# dispatches to the public method — the explicit interface implementation is invisible through the concrete-type reference. To call the explicit implementation, the variable must be typed as `INamedDocument` or cast to it: `((INamedDocument)invoice).GetName()`. The fix is to update `ResolveFileName` to accept `INamedDocument` rather than `InvoiceDocument`, or to cast internally: `string path = Path.Combine(_root, ((INamedDocument)invoice).GetName())`. The broader lesson is that if the file-safe name is the correct public behavior for all document types, it should be the public method, and the human-readable label should be a separate property (e.g., `DisplayName`).

---

## Q15. An abstract base class `NotifierBase` already has `Email` and `SMS` as subclasses. A new `SmsNotifier : INotificationSender` is introduced from an external library. How do you bridge the two hierarchies without breaking LSP?

**Concepts**
- Adapter pattern bridges incompatible hierarchies
- Cannot change an external type's base class
- Wrapper (`class SmsAdapter : NotifierBase`) delegates to external type
- Interface as the unifying consumer contract
- DI registration via `INotificationSender`

**Answer**

When `SmsNotifier` is an external type you cannot modify, and your codebase uses `NotifierBase` as the consumer contract internally, the standard bridge is the Adapter pattern. Create `public class SmsNotifierAdapter : NotifierBase` that holds a private `SmsNotifier` field and delegates `Send()` to it. This wraps the external type in your hierarchy without modifying either. For new code, the better design is to unify around the interface (`INotificationSender`) rather than the abstract base, because an interface imposes no inheritance constraint. Existing `NotifierBase` subclasses can implement `INotificationSender` explicitly or the base itself can implement the interface. The DI container registers all notifiers as `INotificationSender`, enabling polymorphic dispatch without needing a shared base class. The abstract base is then an implementation detail for types you own and control, not the external-facing contract. This is the design that most cleanly handles future cases where new notification channel providers come as external libraries.

---

## Real-World Scenarios

---

## Q16. A document archive needs `SpreadsheetDocument` with shared `Title`/`CreatedOn`, optional CSV export, and a separate audit trail. How would you model this?

**Concepts**
- Abstract class for shared state and template method
- Fine-grained interfaces for optional capabilities
- Explicit interface implementation for audit trail
- ISP — not every document type supports CSV or audit
- DI integration via interface registration

**Answer**

The model uses an abstract class `Document` for the shared identity (`Title`, `CreatedOn`, protected constructor validation) and the template method `RenderContent()`. Separate capability interfaces express the optional features: `ICsvExportable` with `string ExportCsv()` for spreadsheets, and `IAuditable` with `void WriteAuditEntry(string action)` for the audit trail. `SpreadsheetDocument` inherits from `Document` and implements both `ICsvExportable` and `IAuditable`. Other document types like `InvoiceDocument` inherit from `Document` and implement only the capabilities they support. Services that process CSV exports depend on `ICsvExportable`; audit logging infrastructure depends on `IAuditable`. Neither service knows or cares about the full `SpreadsheetDocument` type, keeping dependencies narrow. When the DI container registers `SpreadsheetDocument`, it can be registered as multiple interfaces simultaneously (`AddSingleton<ICsvExportable, SpreadsheetDocument>` and `AddSingleton<IAuditable, SpreadsheetDocument>`), or a single registration can be resolved for each interface using factory delegates.

---

## Q17. `DocumentProcessor` is hard-coupled to `PdfRenderer` and `List<InvoiceDocument>`. Unit tests are slow because they hit disk. How do you introduce test seams?

**Concepts**
- Dependency inversion — depend on abstractions
- Extract `IRenderer` interface from `PdfRenderer`
- Widen parameter type from concrete to `IReadOnlyList<Document>`
- Constructor injection
- Mock-friendly interface for test isolation

```csharp
public sealed class DocumentProcessor
{
    private readonly PdfRenderer _renderer = new PdfRenderer();

    public string BuildBatchSummary(IReadOnlyList<InvoiceDocument> documents)
    {
        var builder = new StringBuilder();
        foreach (var doc in documents)
        {
            builder.AppendLine(doc.GetSummary());
            builder.AppendLine(_renderer.Render(doc));
        }
        return builder.ToString();
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Hard dependency | `PdfRenderer` newed inside class — cannot be replaced | Tests require real renderer, which hits disk |
| Concrete parameter type | `IReadOnlyList<InvoiceDocument>` — cannot pass any other document type | Forces callers to downcast; breaks polymorphism |
| No interface boundary | `PdfRenderer.Render` not behind an abstraction | Impossible to inject a fake for unit tests |

**Fix priority list**
1. Extract `IRenderer` with `string Render(Document doc)` and have `PdfRenderer` implement it.
2. Inject `IRenderer` via constructor: `public DocumentProcessor(IRenderer renderer)`.
3. Widen parameter to `IReadOnlyList<Document>` (or `IEnumerable<Document>`).
4. Register `PdfRenderer` as `IRenderer` in DI; inject `FakeRenderer` in tests.

**Answer**

There are two independent coupling problems. First, `PdfRenderer` is instantiated with `new` inside the class, making it impossible to substitute a test double without changing the class itself. The fix is to extract an `IRenderer` interface and inject it via the constructor. In production, DI provides `PdfRenderer`; in tests, an in-memory `FakeRenderer` runs instantly without touching disk. Second, the method parameter `IReadOnlyList<InvoiceDocument>` is needlessly specific — it accepts only invoice documents even though the method only calls `GetSummary()` and `Render(doc)`, both of which operate on the abstract `Document` type. Widening to `IReadOnlyList<Document>` (or `IEnumerable<Document>`) makes the method reusable across all document types and aligns with the Dependency Inversion Principle: depend on abstractions, not concrete types. Together, these changes make `DocumentProcessor` testable with a trivial in-memory renderer and a list of any `Document` subtype.

---

## Q18. You need to add `PushNotificationSender` to an existing notification system where `Email` and `SMS` already extend `NotifierBase`. Option A extends the abstract base; Option B uses a static helper. How do you decide?

**Concepts**
- Abstract base for shared retry/logging behavior
- Interface for DI, mocking, cross-hierarchy use
- Static helper class as composition alternative
- Open/Closed Principle — adding types without modifying base
- Mixed approach when some types can and cannot inherit the base

**Answer**

The right decision depends on whether `PushNotificationSender` can and should inherit from `NotifierBase`. If it comes from an external library or already has a different base class, it cannot extend `NotifierBase` without a wrapper adapter. If it is a new type you control and the `Retry` helper in `NotifierBase` provides genuine value (reducing code duplication across all channel implementations), then `PushNotificationSender : NotifierBase` is appropriate and follows the original design. Option B's static `NotificationRetry.SendWithRetry` helper is composable: any type implementing `INotificationSender` can use it without inheriting a base class. This makes it more flexible when the type hierarchy is heterogeneous. The combined approach — keep `NotifierBase` for types you own and control, use `INotificationSender` as the universal consumer contract, and extract the retry logic into a static helper that both `NotifierBase` and standalone implementations can delegate to — gives maximum flexibility while preserving the shared behavior for types that benefit from it.
