namespace ClassesAndObjects;

public class Customer
{
    public static int customerCount = 0;
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public double loanAmount { get; set; }
    public double rateOfIntrest { get; set; }
    public double durationOfLoan { get; set; }
    public double totalIntrest { get; set; }

    public Customer()
    {
        id = ++customerCount;
    }

    public override string? ToString()
    {
        return $"Loan Intrest for Customer {name} : {totalIntrest}";
    }
}

public static class CustomerLoanDemo
{
    public static void Run()
    {
        Customer[] customers = new Customer[3];

        Console.WriteLine("Enter Details for Three Customers: \n");
        int i = 0;
        while (i < 3)
        {
            Customer customer = new Customer();

            Console.Write("Enter Customer Name : ");
            customer.name = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter Loan Amount : ");
            customer.loanAmount = Convert.ToDouble(Console.ReadLine());

            Console.Write("Enter Rate of Interst : ");
            customer.rateOfIntrest = Convert.ToDouble(Console.ReadLine());

            Console.Write("Enter Duration of Loan : ");
            customer.durationOfLoan = Convert.ToDouble(Console.ReadLine());

            Console.WriteLine();

            customer.totalIntrest = (customer.loanAmount * customer.rateOfIntrest * customer.durationOfLoan) / 100;

            customers[i] = customer;
            i++;
        }

        foreach (Customer customer in customers)
        {
            Console.WriteLine(customer);
        }
    }
}
