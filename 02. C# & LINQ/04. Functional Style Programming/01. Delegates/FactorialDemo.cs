namespace Delegates;

public static class FactorialDemo
{
    public delegate int MyDelegate(int x);

    public static void Run()
    {
        MyDelegate deleg = Factorial;
        Console.WriteLine("Factorial of 7 : " + deleg(7));
    }

    public static int Factorial(int x)
    {
        if (x == 0)
            return 1;
        return x * Factorial(x - 1);
    }
}
