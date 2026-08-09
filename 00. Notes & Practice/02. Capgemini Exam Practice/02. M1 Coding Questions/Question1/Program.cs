using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

class Program
{
    public string SENTINAL = "~";

    public string JoinIt(string[] input)
    {
        return string.Join("~", input);
    }

    public string[] SplitIt(string input)
    {
        return input.Split('~');
    }

    public StringBuilder Reverse(string input)
    {
        string[] arr = input.Split(' ');
        StringBuilder sb = new StringBuilder();
        for(int i = arr.Length - 1; i >= 0; i--)
        {
            sb.Append(arr[i]);
            sb.Append(" ");
        }
        return sb;
    }

    public string PadLeft(string input,int n,char a)
    {
        return input.PadLeft(n, a);
    }

    public static void Main(string[] args)
    {
        Program p = new Program();

        // JoinIt
        Console.WriteLine(p.JoinIt(new string[]{"one","two","three"}));

        // SplitIt
        string[] output = p.SplitIt("one~two~three~four~five~six");
        
        foreach(string s in output)
        {
            Console.Write(s + " ");
        }
        Console.WriteLine();

        // Reverse
        Console.WriteLine(p.Reverse("Seven two three"));

        // PadLeft
        Console.WriteLine(p.PadLeft("OneTwoThree",15,'~'));
    }
}