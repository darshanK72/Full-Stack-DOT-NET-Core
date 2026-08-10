# Properties in C#

Properties are members that provide a flexible mechanism to read, write, or compute the value of a private field. They look like fields when accessed, but are actually special methods called **accessors** (`get` and `set`).

## Why use properties instead of public fields?

Properties let you control access to data — validate input, make fields read-only/write-only, or compute values on the fly — without changing how calling code interacts with the class.

## Basic syntax

```csharp
public class Person
{
    private string _name; // backing field

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
}
```

Usage looks just like a field:

```csharp
Person p = new Person();
p.Name = "Alice";      // calls set
Console.WriteLine(p.Name); // calls get
```

## Auto-implemented properties

If you don't need custom logic, let the compiler generate the backing field:

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

## Validation logic

```csharp
public class Person
{
    private int _age;

    public int Age
    {
        get { return _age; }
        set
        {
            if (value < 0)
                throw new ArgumentException("Age cannot be negative.");
            _age = value;
        }
    }
}
```

## Read-only properties

```csharp
public class Circle
{
    public double Radius { get; }

    public Circle(double radius)
    {
        Radius = radius; // can only be set in constructor
    }

    public double Area => Math.PI * Radius * Radius; // expression-bodied, computed property
}
```

## Access modifiers on accessors

You can restrict `set` independently of `get`:

```csharp
public class Account
{
    public decimal Balance { get; private set; }

    public void Deposit(decimal amount) => Balance += amount;
}
```

## init-only properties (C# 9+)

Allow setting only during object initialization, for immutability:

```csharp
public class Person
{
    public string Name { get; init; }
}

var p = new Person { Name = "Alice" }; // OK
// p.Name = "Bob";                     // Error: init-only after construction
```

## Static properties

Belong to the type itself, not an instance:

```csharp
public class Counter
{
    public static int Count { get; private set; }

    public Counter() => Count++;
}
```

## Key points

| Feature | Purpose |
|---|---|
| `get` | Returns the property's value |
| `set` | Assigns a value (via implicit `value` parameter) |
| `init` | Allows assignment only during object initialization |
| Auto-property | Compiler-generated backing field |
| Expression body (`=>`) | Concise computed/read-only properties |
| Access modifiers | Fine-grained control (e.g., `private set`) |

Properties are central to **encapsulation** in C#, letting you expose data safely while hiding implementation details.