using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Solution
{

    abstract class Employee
    {
        public string department;
        public string name;
        public string location;
        public bool isOnVacation;

        public Employee(string department, string name, string location)
        {
            this.department = department;
            this.name = name;
            this.location = location;
            this.isOnVacation = false;
        }

        public string GetDepartment()
        {
            return this.department;
        }

        public string GetName()
        {
            return this.name;
        }

        public string GetLocation()
        {
            return this.location;
        }

        public bool GetStatus()
        {
            return this.isOnVacation;
        }

        public void SwitchStatus()
        {
            if (this.isOnVacation)
            {
                this.isOnVacation = false;
            }
            else
            {
                this.isOnVacation = true;
            }
        }
    }
    class FinanceEmployee : Employee
    {

        public FinanceEmployee(string name, string location) : base("Finance", name, location)
        {

        }

    }
    class MarketingEmployee : Employee
    {
        public MarketingEmployee(string name, string location) : base("Marketing", name, location)
        {

        }
    }

    class Solution
    {
        static void Main()
        {
            Type baseType = typeof(Employee);
            if (!baseType.IsAbstract)
                throw new Exception($"{baseType.Name} type should be abstract");

            string str = Console.ReadLine();
            string[] strArr = str.Split(' ');
            Employee financeEmployee = new FinanceEmployee(strArr[0], strArr[1]);

            var department = financeEmployee.GetDepartment();
            var name = financeEmployee.GetName();
            var location = financeEmployee.GetLocation();
            var status = financeEmployee.GetStatus() ? "on" : "not on";

            Console.WriteLine($"FinanceEmployee info: Department - {department}, Name - {name}, Location - {location}");
            Console.WriteLine($"{name} is {status} vacation");

            Console.WriteLine("Switching");
            financeEmployee.SwitchStatus();
            status = financeEmployee.GetStatus() ? "on" : "not on";
            Console.WriteLine($"{name} is {status} vacation");

            Console.WriteLine("Switching");
            financeEmployee.SwitchStatus();
            status = financeEmployee.GetStatus() ? "on" : "not on";
            Console.WriteLine($"{name} is {status} vacation");

            str = Console.ReadLine();
            strArr = str.Split(' ');
            Employee marketingEmployee = new MarketingEmployee(strArr[0], strArr[1]);

            department = marketingEmployee.GetDepartment();
            name = marketingEmployee.GetName();
            location = marketingEmployee.GetLocation();
            status = marketingEmployee.GetStatus() ? "on" : "not on";

            Console.WriteLine($"MarketingEmployee info: Department - {department}, Name - {name}, Location - {location}");
            Console.WriteLine($"{name} is {status} vacation");

            Console.WriteLine("Switching");
            marketingEmployee.SwitchStatus();
            status = marketingEmployee.GetStatus() ? "on" : "not on";
            Console.WriteLine($"{name} is {status} vacation");

            Console.WriteLine("Switching");
            marketingEmployee.SwitchStatus();
            status = marketingEmployee.GetStatus() ? "on" : "not on";
            Console.WriteLine($"{name} is {status} vacation");
        }
    }
}