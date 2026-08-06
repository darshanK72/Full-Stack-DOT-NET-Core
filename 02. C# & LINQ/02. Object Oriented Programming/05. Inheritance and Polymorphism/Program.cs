namespace InheritanceAndPolymorphism;

public abstract class Employee
{
    public static int employeeCount = 0;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public double Salary { get; set; }

    public Employee()
    {
        EmployeeId = ++employeeCount;
    }

    public Employee(string employeeName, string address, string city, string department, double salary)
    {
        EmployeeId = ++employeeCount;
        EmployeeName = employeeName;
        Address = address;
        City = city;
        Department = department;
        Salary = salary;
    }

    public abstract double CalculateSalary();
}

public class ContractEmployee : Employee
{
    public double Perks { get; set; } = 2000;

    public ContractEmployee(string employeeName, string address, string city, string department, double salary, double perks)
        : base(employeeName, address, city, department, salary)
    {
        Perks = perks;
    }

    public override double CalculateSalary()
    {
        return Salary + Perks;
    }

    public override string? ToString()
    {
        return $"Employee Name : {EmployeeName}\nSalary : {CalculateSalary()}";
    }
}

public class PermenentEmployee : Employee
{
    public double Perks { get; set; } = 5000;
    public int NoOfLeaves { get; set; } = 22;
    public double ProvidendFund { get; set; }

    public PermenentEmployee(string employeeName, string address, string city, string department, double salary, double perks, int noOfLeaves, double providendFund)
        : base(employeeName, address, city, department, salary)
    {
        Perks = perks;
        NoOfLeaves = noOfLeaves;
        ProvidendFund = providendFund;
    }

    public override double CalculateSalary()
    {
        return Salary + Perks - ProvidendFund;
    }

    public override string? ToString()
    {
        return $"Employee Name : {EmployeeName}\nSalary : {CalculateSalary()}";
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.Write("Enter Total No of Employes : ");
        int noOfEmployes = Convert.ToInt32(Console.ReadLine());

        Employee[] employes = new Employee[noOfEmployes];
        int i = 0;

        while (i < noOfEmployes)
        {
            Console.Write("Enter Type of Employee\n1. Contract\n2. Permenent\n Your Choice : ");
            int choice = Convert.ToInt32(Console.ReadLine());

            Console.Write("Enter Employee Details\n");
            Console.Write("Name : ");
            string? name = Console.ReadLine();
            Console.Write("Address : ");
            string? address = Console.ReadLine();
            Console.Write("City : ");
            string? city = Console.ReadLine();
            Console.Write("Department : ");
            string? department = Console.ReadLine();
            Console.Write("Salary : ");
            double salary = Convert.ToDouble(Console.ReadLine());
            Console.Write("Perks : ");
            double perks = Convert.ToDouble(Console.ReadLine());

            if (choice == 1)
            {
                employes[i] = new ContractEmployee(name!, address!, city!, department!, salary, perks);
            }
            else if (choice == 2)
            {
                Console.Write("No of Leaves: ");
                int noOfLeaves = Convert.ToInt32(Console.ReadLine());
                Console.Write("Providand Fund : ");
                double providendFund = Convert.ToDouble(Console.ReadLine());

                employes[i] = new PermenentEmployee(name!, address!, city!, department!, salary, perks, noOfLeaves, providendFund);
            }
            else
            {
                Console.WriteLine("Incorrect Choice");
            }

            i++;
        }

        Console.WriteLine("\n\n------------------- Salary of Employes ----------------------\n");
        foreach (Employee emp in employes)
        {
            if (emp != null)
                Console.WriteLine(emp);
        }
    }
}
