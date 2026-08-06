namespace ClassesAndObjects;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 07. Classes and Objects ===");
        Console.WriteLine("1. Customer Loan Interest");
        Console.WriteLine("2. Student Details");
        Console.Write("Enter choice: ");

        switch (Console.ReadLine())
        {
            case "1":
                CustomerLoanDemo.Run();
                break;
            case "2":
                StudentDemo.Run();
                break;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }
}
