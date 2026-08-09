using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;

class Book
{
    public string Name { get; set; }
    public int Price { get; set; }

}

class BookImplementation
{
    public string NameOfAllBooks(IList<Book> books)
    {
        string output = "";

        foreach(Book bk in books)
        {
            output += bk.Name;
            output += " ";
        }
        return output;
    }

    public string PriceOfAllBooks(IList<Book> books)
    {

        string output = "";

        foreach (Book bk in books)
        {
            output += bk.Price.ToString();
            output += " ";
        }
        return output;

    }

    public int SumPriceOfAllBooks(IList<Book> books)
    {
        int output = 0;

        foreach (Book bk in books)
        {
            output += bk.Price;
        }
        return output;
    }

    public int Find(IList<Book> books,string s)
    {
        int i = 0;
        foreach(Book bk in books)
        {
            if(bk.Name == s)
            {
                return i;
            }
            i++;
        }

        return -1;

    }
}

class Program
{
    public static void Main(string[] args)
    {
        IList<Book> bookList = new List<Book>
        {
            new Book(){Name = "One",Price = 5000},
            new Book(){Name = "Two",Price = 200},
            new Book(){Name = "Three",Price = 500},
            new Book(){Name = "Four",Price = 1200},
            new Book(){Name = "Five",Price = 4700}
        };

        BookImplementation imp = new BookImplementation();

        Console.WriteLine(imp.NameOfAllBooks(bookList));
        Console.WriteLine(imp.PriceOfAllBooks(bookList));
        Console.WriteLine(imp.SumPriceOfAllBooks(bookList));
        Console.WriteLine(imp.Find(bookList,"Three"));
    }
}