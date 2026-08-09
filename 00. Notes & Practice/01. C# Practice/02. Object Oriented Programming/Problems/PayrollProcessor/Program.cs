/*
 * PROBLEM: Payroll Processor
 *
 * HR runs monthly payroll for mixed employment types. Each employee type
 * calculates gross pay differently. Finance needs totals and per-type
 * breakdowns from one polymorphic collection.
 *
 * This exercise covers:
 *   ch01 — classes and fields
 *   ch03 — constructors; method overloading; private core helper
 *   ch05 — abstract base; virtual/override; runtime polymorphism
 *   ch07 — encapsulation of employee collection
 */

using System;
using System.Collections.Generic;

namespace PayrollOperations
{
    enum PayPeriod
    {
        WEEKLY,
        BIWEEKLY,
        MONTHLY
    }

    abstract class Employee
    {
        public int EmployeeId { get; }
        public string Name { get; }
        public PayPeriod PayPeriod { get; }

        protected Employee(int employeeId, string name, PayPeriod payPeriod)
        {
            // TODO: validate non-empty name; assign fields
            throw new NotImplementedException();
        }

        public abstract decimal CalculateGrossPay();

        public virtual string GetSummary()
        {
            // TODO: "[{EmployeeId}] {Name}"
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return GetSummary();
        }
    }

    class SalariedEmployee : Employee
    {
        public decimal AnnualSalary { get; }

        public SalariedEmployee(int id, string name, PayPeriod period, decimal annualSalary)
            : base(id, name, period)
        {
            // TODO: validate annualSalary > 0
            throw new NotImplementedException();
        }

        public override decimal CalculateGrossPay()
        {
            // TODO: prorate by PayPeriod; round to 2 decimals AwayFromZero
            throw new NotImplementedException();
        }

        public override string GetSummary()
        {
            // TODO: append type tag e.g. "(Salaried)"
            throw new NotImplementedException();
        }
    }

    class HourlyEmployee : Employee
    {
        public decimal HourlyRate { get; }
        public decimal HoursWorked { get; }

        public HourlyEmployee(int id, string name, PayPeriod period, decimal hourlyRate, decimal hoursWorked)
            : base(id, name, period)
        {
            // TODO: validate rate > 0 and hours >= 0
            throw new NotImplementedException();
        }

        public override decimal CalculateGrossPay()
        {
            // TODO: HourlyRate * HoursWorked rounded to 2 decimals
            throw new NotImplementedException();
        }

        public override string GetSummary()
        {
            // TODO: append type tag e.g. "(Hourly)"
            throw new NotImplementedException();
        }
    }

    class PayrollProcessor
    {
        private readonly List<Employee> _employees = new List<Employee>();

        public IReadOnlyList<Employee> Employees => _employees.AsReadOnly();

        public bool AddEmployee(SalariedEmployee salaried)
        {
            return AddEmployeeCore(salaried);
        }

        public bool AddEmployee(HourlyEmployee hourly)
        {
            return AddEmployeeCore(hourly);
        }

        private bool AddEmployeeCore(Employee employee)
        {
            // TODO: reject duplicate EmployeeId
            throw new NotImplementedException();
        }

        public decimal GetTotalGrossPay()
        {
            // TODO: polymorphic sum of CalculateGrossPay()
            throw new NotImplementedException();
        }

        public Dictionary<string, decimal> GetGrossByType()
        {
            // TODO: keys "Salaried" and "Hourly"; missing type -> 0
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: seed at least one salaried and one hourly employee
            // TODO: print summaries, total gross, and dictionary breakdown
            throw new NotImplementedException();
        }
    }
}
