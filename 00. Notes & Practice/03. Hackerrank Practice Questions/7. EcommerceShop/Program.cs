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

abstract class GroceryReceiptBase
{
    public Dictionary<string, decimal> Prices { get; set; }
    public Dictionary<string, int> Discounts { get; set; }
    public GroceryReceiptBase(Dictionary<string, decimal> prices, Dictionary<string, int> discounts) { Prices = prices; Discounts = discounts; }
    public abstract IEnumerable<(string fruit, decimal price, decimal total)> Calculate(List<Tuple<string, int>> shoppingList);
}

class GroceryReceipt : GroceryReceiptBase
{
    public GroceryReceipt(Dictionary<string, decimal> prices, Dictionary<string, int> discounts) : base(prices, discounts)
    {

    }

    public override IEnumerable<(string fruit, decimal price, decimal total)> Calculate(List<Tuple<string, int>> shoppingList)
    {
        List<(string, decimal, decimal)> output = new List<(string, decimal, decimal)>();
        foreach(var shopingItem in shoppingList)
        {
            string f = shopingItem.Item1;
            decimal p = Prices[f];
            decimal t = Prices[f] * shopingItem.Item2 * (100 - Discounts[f]) / 100;

            output.Add((f,p,t));
        }
        return output;
    }
}

class Program
{
    public static void Main(string[] args)
    {
        var p = new Dictionary<string, decimal>();
        p.Add("Apple", 31);
        p.Add("Banana", 39);
        p.Add("Orange", 47);

        var d = new Dictionary<string, int>();
        d.Add("Apple", 40);
        d.Add("Banana", 40);
        d.Add("Orange", 50);

        var s = new List<Tuple<string, int>>();
        s.Add(new Tuple<string, int>("Banana", 4));
        s.Add(new Tuple<string, int>("Apple", 3));

        var recipe = new GroceryReceipt(p, d);
        foreach(var o in recipe.Calculate(s))
        {
            Console.WriteLine(o.Item1 + " " + o.Item2 + " " + o.Item3);
        }
    }
}