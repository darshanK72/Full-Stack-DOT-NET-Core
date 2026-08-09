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


interface ICategory
{
    int Id { get; set; }
    string Name { get; set; }
    List<IProduct> Products { get; set; }
    void AddProduct(IProduct product);
}
interface IProduct
{
    int Id { get; set; }
    string Name { get; set; }
    decimal Price { get; set; }
}

interface ICompany
{
    //The top category name by product count
    string GetTopCategoryNameByProductCount();
    //The products are added to more than one category
    List<IProduct> GetProductsBelongsToMultipleCategory();
    //Top category according to the total price of its products
    (string categoryName, decimal totalValue) GetTopCategoryBySumOfProductPrices();
    //The list of categories with the sum of the prices of the products
    List<(ICategory category, decimal totalValue)> GetCategoriesWithSumOfTheProductPrices();
}
class Product : IProduct
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public Product(int Id, string Name, decimal Price)
    {
        this.Id = Id;
        this.Name = Name;
        this.Price = Price;
    }
}

class Category : ICategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<IProduct> Products { get; set; }

    public Category(int Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
        this.Products = new List<IProduct>();
    }

    public void AddProduct(IProduct product)
    {
        this.Products.Add(product);
    }
}

class Company : ICompany
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<ICategory> Categories { get; set; }

    public Company(int Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
        this.Categories = new List<ICategory>();
    }

    public void AddCategory(ICategory category)
    {
        this.Categories.Add(category);
    }

    public string GetTopCategoryNameByProductCount()
    {
        ICategory category = this.Categories.OrderByDescending(c => c.Products.Count()).FirstOrDefault();
        return category.Name;
    }

    public List<IProduct> GetProductsBelongsToMultipleCategory()
    {

        List<IProduct> allProducts = new List<IProduct>();
        List<IProduct> products = new List<IProduct>();

        int i = 0;
        foreach (var category in Categories)
        {
            i++;
            foreach (var product in category.Products)
            {

                for (int j = i; j < Categories.Count(); j++)
                {
                    if (Categories[j].Products.Exists(p => p.Id == product.Id))
                    {
                        if (!products.Contains(product))
                        {
                            products.Add(product);
                        }
                    }
                }
            }
        }

        return products;
    }

    public (string categoryName, decimal totalValue) GetTopCategoryBySumOfProductPrices()
    {

        string name = "";
        decimal value = 0;

        foreach (var category in Categories)
        {
            decimal sum = 0;
            foreach (var product in category.Products)
            {
                sum += product.Price;
            }

            if (sum > value)
            {
                name = category.Name;
                value = sum;
            }
        }

        return (name, value);
    }

    public List<(ICategory category, decimal totalValue)> GetCategoriesWithSumOfTheProductPrices()
    {

        List<(ICategory, decimal)> output = new List<(ICategory, decimal)>();

        foreach (var category in Categories)
        {
            decimal sum = 0;
            foreach (var product in category.Products)
            {
                sum += product.Price;
            }

            output.Add((category, sum));
        }

        return output;
    }

}

class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);
        var company = new Company(1, "Company 1");
        List<IProduct> products = new List<IProduct>();
        List<ICategory> categories = new List<ICategory>();

        int n = Convert.ToInt32(Console.ReadLine().Trim());
        for (int i = 1; i <= n; i++)
        {
            var a = Console.ReadLine().Trim().Split(" ");
            products.Add(new Product(Convert.ToInt32(a[0]), a[1], Convert.ToInt32(a[2])));
        }

        int m = Convert.ToInt32(Console.ReadLine().Trim());
        for (int i = 1; i <= m; i++)
        {
            var a = Console.ReadLine().Trim().Split(" ");
            categories.Add(new Category(Convert.ToInt32(a[0]), a[1]));
        }

        int p = Convert.ToInt32(Console.ReadLine().Trim());
        for (int i = 1; i <= p; i++)
        {
            var a = Console.ReadLine().Trim().Split(" ");
            var c = categories.FirstOrDefault(x => x.Id == Convert.ToInt32(a[0]));
            var pp = products.FirstOrDefault(x => x.Id == Convert.ToInt32(a[1]));
            if (c != null && pp != null)
            {
                c.AddProduct(pp);
            }

        }

        foreach (var item in categories)
        {
            company.AddCategory(item);
        }
        var topCategory = company.GetTopCategoryNameByProductCount();
        var commonProducts = company.GetProductsBelongsToMultipleCategory();
        var mostValuableCategory = company.GetTopCategoryBySumOfProductPrices();
        var categoriesWithTotalPrices = company.GetCategoriesWithSumOfTheProductPrices();

        textWriter.WriteLine("Top category:" + topCategory);
        textWriter.WriteLine("Common products:");
        foreach (var item in commonProducts)
        {
            textWriter.WriteLine(item.Name);
        }
        textWriter.WriteLine("Most valuable category:" + mostValuableCategory.categoryName
            + " " + mostValuableCategory.totalValue.ToString("0.0", new NumberFormatInfo() { NumberDecimalSeparator = "." }));
        foreach (var item in categoriesWithTotalPrices)
        {
            textWriter.WriteLine(item.category.Name + " " + item.totalValue.ToString("0.0", new NumberFormatInfo() { NumberDecimalSeparator = "." }));
        }


        textWriter.Flush();
        textWriter.Close();
    }
}
