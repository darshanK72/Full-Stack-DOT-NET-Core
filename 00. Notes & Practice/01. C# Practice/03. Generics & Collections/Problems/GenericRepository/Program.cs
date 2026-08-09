/*
 * PROBLEM: Generic Repository
 *
 * Reusable type-safe in-memory storage for SKU counts, bin labels, and weights.
 *
 * This exercise covers:
 *   ch01 — generic classes, interfaces, structs, methods
 *   ch01 — constraints: notnull, struct, new(), IComparable<T>
 *   ch01 — default(T), typeof(T), closed constructed types
 */

using System;
using System.Collections.Generic;

namespace WarehouseGenerics
{
    interface IRepository<TKey, TValue> where TKey : notnull
    {
        bool TryGet(TKey key, out TValue value);
        void Set(TKey key, TValue value);
        IReadOnlyCollection<TKey> Keys { get; }
    }

    sealed class InMemoryRepository<TKey, TValue> : IRepository<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _store = new Dictionary<TKey, TValue>();

        public IReadOnlyCollection<TKey> Keys => _store.Keys;

        public bool TryGet(TKey key, out TValue value) =>
            _store.TryGetValue(key, out value!);

        public void Set(TKey key, TValue value) => _store[key] = value;
    }

    struct Quantity<TUnit> where TUnit : struct
    {
        public int Count { get; set; }
        public TUnit Unit { get; set; }

        public string Format()
        {
            // TODO: return "{Count} {Unit}"
            throw new NotImplementedException();
        }
    }

    sealed class StockLine : IComparable<StockLine>
    {
        public string Sku { get; set; } = string.Empty;
        public int Units { get; set; }

        public int CompareTo(StockLine? other)
        {
            // TODO: compare by Sku ordinal; null sorts after non-null
            throw new NotImplementedException();
        }
    }

    static class RepositoryHelpers
    {
        public static void Swap<T>(ref T left, ref T right)
        {
            // TODO: swap two values
            throw new NotImplementedException();
        }

        public static T CreateDefault<T>() where T : new()
        {
            // TODO: return new T()
            throw new NotImplementedException();
        }

        public static string DescribeDefault<T>()
        {
            // TODO: typeof(T).Name and default(T) display ("null" for null ref)
            throw new NotImplementedException();
        }

        public static int CompareOrdered<T>(T left, T right) where T : IComparable<T>
        {
            // TODO: left.CompareTo(right)
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: InMemoryRepository<string,int> — set two SKUs, TryGet found/missing
            // TODO: Quantity<int> Format()
            // TODO: Swap two strings
            // TODO: CreateDefault<StockLine>(), CompareOrdered on two lines
            // TODO: DescribeDefault<int>() and DescribeDefault<string>()
            throw new NotImplementedException();
        }
    }
}
