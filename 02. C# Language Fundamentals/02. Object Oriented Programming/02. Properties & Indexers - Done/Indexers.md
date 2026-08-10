# Indexers in C#

An indexer allows objects to be indexed like arrays, using `this[]` syntax. It's essentially a special property that takes parameters, letting you access elements of a class or struct using `object[index]` notation.

## Why use indexers?

They let custom classes (like collections or wrappers) support array-like syntax, making your API more intuitive.

## Basic syntax

```csharp
public class SampleCollection
{
    private string[] _items = new string[10];

    public string this[int index]
    {
        get { return _items[index]; }
        set { _items[index] = value; }
    }
}
```

Usage:

```csharp
SampleCollection collection = new SampleCollection();
collection[0] = "Hello";        // calls set
Console.WriteLine(collection[0]); // calls get
```

## Indexers with non-integer keys

Indexers aren't limited to `int` — you can use `string`, or any type, as the index:

```csharp
public class Config
{
    private Dictionary<string, string> _settings = new Dictionary<string, string>();

    public string this[string key]
    {
        get => _settings.TryGetValue(key, out var value) ? value : null;
        set => _settings[key] = value;
    }
}
```

```csharp
Config config = new Config();
config["theme"] = "dark";
Console.WriteLine(config["theme"]); // "dark"
```

## Overloaded indexers

A class can define multiple indexers with different parameter types:

```csharp
public class Matrix
{
    private double[,] _data = new double[10, 10];

    // Access by row and column
    public double this[int row, int col]
    {
        get => _data[row, col];
        set => _data[row, col] = value;
    }
}
```

```csharp
Matrix m = new Matrix();
m[1, 2] = 5.5;
Console.WriteLine(m[1, 2]);
```

## Read-only indexers

Omit `set` for a read-only indexer, or use expression-bodied syntax:

```csharp
public class Squares
{
    public int this[int index] => index * index;
}
```

```csharp
Squares sq = new Squares();
Console.WriteLine(sq[5]); // 25
```

## Validation in indexers

```csharp
public class SafeArray
{
    private int[] _data = new int[5];

    public int this[int index]
    {
        get
        {
            if (index < 0 || index >= _data.Length)
                throw new IndexOutOfRangeException();
            return _data[index];
        }
        set
        {
            if (index < 0 || index >= _data.Length)
                throw new IndexOutOfRangeException();
            _data[index] = value;
        }
    }
}
```

## Indexers vs. Properties

| Aspect | Property | Indexer |
|---|---|---|
| Declared with | Identifier name (e.g., `Name`) | `this[parameters]` |
| Parameters | None | One or more required |
| Access syntax | `obj.Name` | `obj[key]` |
| Static allowed? | Yes | No (must be instance member) |
| Overloading | Not applicable | Allowed (different parameter types) |

## Key points

- Indexers use the `this` keyword followed by square brackets containing parameter(s).
- They support `get`, `set`, or both, just like properties.
- They can be overloaded based on parameter type/count.
- Common in custom collection classes to mimic array/dictionary behavior.
- Indexers **cannot** be static, since they operate on a specific instance.

Indexers make custom types feel natural to use — turning something like a wrapper class into something you can interact with just like a built-in array or dictionary.