---
module: 02. Object Oriented Programming
difficulty: Hard
chapters: 01 Classes, 03 Constructors, 05 Inheritance, 05 Polymorphism, 07 Encapsulation
domain: Payroll
---

# Payroll Processor

Build a **.NET 8 console application from scratch** modeling employee compensation with inheritance and polymorphism.

## Business context

HR runs monthly payroll for mixed employment types. Each employee type calculates gross pay differently. Finance needs totals and per-type breakdowns from one polymorphic collection.

## Definitions

**Enum `PayPeriod`:** `WEEKLY`, `BIWEEKLY`, `MONTHLY`

**Abstract base `Employee`**

- `EmployeeId` (int), `Name` (string), `PayPeriod` — set via constructor; validate non-empty name
- Abstract `decimal CalculateGrossPay()` 
- Virtual `string GetSummary()` → `"[{EmployeeId}] {Name}"` (override in derived types to append type tag)
- Override `ToString()` to delegate to `GetSummary()`

**`SalariedEmployee : Employee`**

- Additional `AnnualSalary` (decimal > 0)
- Constructor `(id, name, period, annualSalary)` with validation
- `CalculateGrossPay()` → prorate annual salary by period:
  - WEEKLY → `AnnualSalary / 52`
  - BIWEEKLY → `AnnualSalary / 26`
  - MONTHLY → `AnnualSalary / 12`
- Round each gross to **2 decimal places** (`MidpointRounding.AwayFromZero`)

**`HourlyEmployee : Employee`**

- `HourlyRate` (> 0), `HoursWorked` (≥ 0)
- Gross → `HourlyRate * HoursWorked`, rounded to 2 decimals

**Class `PayrollProcessor`**

- `AddEmployee(Employee e)` — reject duplicate `EmployeeId` (return false)
- `IReadOnlyList<Employee> Employees` or equivalent read-only view
- `decimal GetTotalGrossPay()` — sum all `CalculateGrossPay()` using polymorphism
- `Dictionary<string, decimal> GetGrossByType()` — keys `"Salaried"` and `"Hourly"` with totals (missing type → `0`)

## Method overloading

Implement **two** `AddEmployee` overloads on `PayrollProcessor`:

1. `(SalariedEmployee salaried)` 
2. `(HourlyEmployee hourly)`

Both delegate to a private `AddEmployeeCore(Employee e)` to avoid duplicate id logic.

## Demo Main

Seed at least one salaried and one hourly employee; print each summary, total gross, and dictionary breakdown.

## Constraints

- net8, explicit usings, `decimal` for money
- Use `virtual`/`override` for pay calculation and summaries
- No LINQ required

## Non-goals

Tax withholding, persistence, async

## Evaluation

[EVALUATION.md](EVALUATION.md)
