using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;

public interface IBook
{
    int Id { get; set; }
    string Title { get; set; }
    string Author { get; set; }
    string Category { get; set; }
    int Price { get; set; }
}

public interface ILibrarySystem
{
    void AddBook(IBook book, int quantity);
    void RemoveBook(IBook book, int quantity);
    int CalculateTotal();
    List<(string, int)> CategoryTotalPrice();
    List<(string, int, int)> BooksInfo();
    List<(string, string, int)> CategoryAndAuthorWithCount();
}

public class Book : IBook
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Category { get; set; }
    public int Price { get; set; }

}
public class LibrarySystem : ILibrarySystem
{
    private Dictionary<IBook, int> _books = new Dictionary<IBook, int>();

    public void AddBook(IBook book, int quantity)
    {
        if (!this._books.ContainsKey(book))
        {
            this._books.Add(book, 0);
        }
        this._books[book] += quantity;
    }

    public void RemoveBook(IBook book, int quantity)
    {
        this._books[book] -= quantity;
        if (this._books[book] <= 0)
        {
            this._books.Remove(book);
        }
    }

    public int CalculateTotal()
    {
        int sum = 0;
        foreach (var entry in _books)
        {
            sum += entry.Key.Price * entry.Value;
        }
        return sum;
    }

    public List<(string, int)> CategoryTotalPrice()
    {
        Dictionary<string, int> categoryMap = new Dictionary<string, int>();

        foreach (var entry in _books)
        {
            if (!categoryMap.ContainsKey(entry.Key.Category))
            {
                categoryMap[entry.Key.Category] = 0;
            }
            categoryMap[entry.Key.Category] += entry.Key.Price * entry.Value;
        }

        List<(string, int)> output = new List<(string, int)>();
        foreach (var entry in categoryMap)
        {
            output.Add((entry.Key, entry.Value));
        }
        return output;
    }

    public List<(string, int, int)> BooksInfo()
    {

        List<(string, int, int)> output = new List<(string, int, int)>();
        foreach (var entry in _books)
        {
            output.Add((entry.Key.Title, entry.Value, entry.Key.Price));
        }
        return output;
    }

    public List<(string, string, int)> CategoryAndAuthorWithCount()
    {
        Dictionary<string, Dictionary<string, int>> categoryAuthorMap = new Dictionary<string, Dictionary<string, int>>();

        foreach (var entry in _books)
        {
            if (!categoryAuthorMap.ContainsKey(entry.Key.Category))
            {
                categoryAuthorMap.Add(entry.Key.Category, new Dictionary<string, int>());
            }
            if (!categoryAuthorMap[entry.Key.Category].ContainsKey(entry.Key.Author))
            {
                categoryAuthorMap[entry.Key.Category].Add(entry.Key.Author, 0);
            }

            categoryAuthorMap[entry.Key.Category][entry.Key.Author] += entry.Value;

        }

        List<(string, string, int)> output = new List<(string, string, int)>();

        foreach (var categoryEntry in categoryAuthorMap)
        {
            foreach (var authorEntry in categoryEntry.Value)
            {
                output.Add((categoryEntry.Key, authorEntry.Key, authorEntry.Value));
            }
        }
        return output;
    }

}


class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        ILibrarySystem librarySystem = new LibrarySystem();
        int bCount = Convert.ToInt32(Console.ReadLine().Trim());
        for (int i = 1; i <= bCount; i++)
        {
            var a = Console.ReadLine().Trim().Split(" ");

            IBook e = new Book();
            e.Id = Convert.ToInt32(a[0]);
            e.Title = a[1];
            e.Author = a[2];
            e.Category = a[3];
            e.Price = Convert.ToInt32(a[4]);


            librarySystem.AddBook(e, Convert.ToInt32(a[5]));
        }

        textWriter.WriteLine("Book Info:");

        var booksInfo = librarySystem.BooksInfo();
        foreach (var (title, quantity, price) in booksInfo.OrderBy(a => a.Item1))
        {
            textWriter.WriteLine($"Book Name:{title}, Quantity:{quantity}, Price:{price}");
        }

        textWriter.WriteLine("Category Total Price:");
        var categoryTotalPrice = librarySystem.CategoryTotalPrice();
        foreach (var (category, totalPrice) in categoryTotalPrice.OrderBy(a => a.Item1))
        {
            textWriter.WriteLine($"Category:{category}, Total Price:{totalPrice}");
        }

        List<(string, string, int)> categoryAndAuthorWithCount = librarySystem.CategoryAndAuthorWithCount();
        textWriter.WriteLine("Category And Author With Count:");
        foreach (var (category, author, count) in categoryAndAuthorWithCount.OrderBy(a => a.Item1))
        {
            textWriter.WriteLine($"Category:{category}, Author:{author}, Count:{count}");
        }


        int total = librarySystem.CalculateTotal();
        textWriter.WriteLine($"Total Price: {total}");


        textWriter.Flush();
        textWriter.Close();
    }
}

