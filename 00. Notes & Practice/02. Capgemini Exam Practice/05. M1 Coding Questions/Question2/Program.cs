using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

class Operation
{
    public double Num1 { get; set; }
    public double Num2 { get; set; }
    public char Opr { get; set; }

    public Operation(int num1, int num2, char opr)
    {
        this.Num1 = num1;
        this.Num2 = num2;
        this.Opr = opr;
    }
}

class Source
{
    public string ExceptionHandle(Operation o)
    {
        try
        {
            if (o.Opr != '+' && o.Opr != '-' && o.Opr != '*' && o.Opr != '/')
            {
                throw new Exception("Invalid Operator");
            }
            else if (o.Opr == '/' && o.Num2 == 0)
            {
                throw new ArithmeticException("Division by zero is not allowed");
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return "No Exception Found";
    }

    public string Calculator(Operation o)
    {
        string output = "";
        switch (o.Opr)
        {
            case '+':
                output += (o.Num1 + o.Num2).ToString();
                break;
            case '-':
                output += (o.Num1 - o.Num2).ToString();
                break;
            case '*':
                output += (o.Num1 * o.Num2).ToString();
                break;
            case '/':
                if (o.Num2 == 0)
                {
                    return "Divide by zero";
                }
                else
                {
                    output += (o.Num1 / o.Num2).ToString();
                }
                break;
        }

        return output;
    }

}

class Program
{
    public static void Main(string[] args)
    {
        Operation o = new Operation(15, 0, '/');

        Source s = new Source();

        Console.WriteLine(s.ExceptionHandle(o));
        Console.WriteLine(s.Calculator(o));

    }
}

