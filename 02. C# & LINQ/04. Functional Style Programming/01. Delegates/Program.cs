namespace Delegates;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 13. Delegates ===");
        Console.WriteLine("1. Factorial using Delegate");
        Console.WriteLine("2. Multicast Delegate (Arithmetic Operations)");
        Console.Write("Enter choice: ");

        switch (Console.ReadLine())
        {
            case "1":
                FactorialDemo.Run();
                break;
            case "2":
                MulticastDelegateDemo.Run();
                break;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }
}
