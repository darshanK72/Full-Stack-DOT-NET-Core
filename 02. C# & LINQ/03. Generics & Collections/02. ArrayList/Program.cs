namespace ArrayListDemo;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 10. ArrayList ===");
        Console.WriteLine("1. Product Demo (object types)");
        Console.WriteLine("2. Product CRUD Application");
        Console.Write("Enter choice: ");

        switch (Console.ReadLine())
        {
            case "1":
                ProductObjectDemo.Run();
                break;
            case "2":
                ProductCrudDemo.Run();
                break;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }
}
