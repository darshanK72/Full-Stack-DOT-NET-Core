# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/05. Inheritance &  Polymorphism`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Badge printing in production shows `"EMP"` for every staff member, including managers and contractors. Review this excerpt from the payroll service (pattern matches this chapter's `GetBadgeThroughEmployeeReference`). What is wrong, and how do you fix it?

```csharp
public abstract class Employee
{
    public string GetBadgeCode() => "EMP";          // not virtual
}

public class ContractEmployee : Employee
{
    public new string GetBadgeCode() => "CTR";
}

public class Manager : PermanentEmployee
{
    public new string GetBadgeCode() => "MGR";
}

public static string PrintBadge(Employee employee) => employee.GetBadgeCode();

// Called from HR export loop over Employee[] payrollStaff
```

---

#### Q2. (R) After adding `InternEmployee` to the payroll hierarchy, `ProcessPayroll` sometimes throws and totals are wrong. Review the new type and the unchanged payroll loop. What design rule did this violate, and what is the prioritized fix?

```csharp
public class InternEmployee : Employee
{
    public InternEmployee(/* ... */) : base(/* ... */) { }

    public override Money CalculateNet()
    {
        throw new InvalidOperationException("Interns are stipend-only; use StipendService");
    }
}

public static Money ProcessPayroll(IReadOnlyList<Employee> staff)
{
    Money total = new(0m);
    foreach (Employee employee in staff)
        total += employee.CalculateNet();   // no type checks — polymorphic sum
    return total;
}

// InternEmployee instances are stored in List<Employee> alongside permanent staff
```

---

#### Q3. (R) A developer adds `Director : Manager` but the project fails to compile. Review the constructors. What is wrong with the chain, and what runs (in order) when `new Director(...)` succeeds?

```csharp
public class Person
{
    public Person(int id, string fullName) { /* sets Id, FullName */ }
}

public class Employee : Person
{
    public Employee(int id, string fullName, Department dept, Money salary)
        : base(id, fullName) { /* ... */ }
}

public class Manager : PermanentEmployee
{
    public Manager(int id, string name, Department dept, Money salary,
        Money perks, Money pf, Money teamBonus)
        : base(id, name, dept, salary, perks, pf) { /* ... */ }
}

public class Director : Manager
{
    public Director(int id, string name, Department dept, Money salary,
        Money perks, Money pf, Money teamBonus, Money boardFee)
    {
        BoardFee = boardFee;   // CS7036 — no suitable base constructor
    }

    public Money BoardFee { get; }
}
```

---

#### Q4. (R) A refactor adds validation to the base payroll method. Contract net pay drops unexpectedly for some employees. Review the change. What broke, and how do you fix it without duplicating validation in every derived class?

```csharp
public class Employee
{
    public virtual Money CalculateNet()
    {
        ValidateNonNegative(BaseSalary);
        return BaseSalary;
    }

    protected void ValidateNonNegative(Money amount)
    {
        if (amount.Amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
    }
}

public class ContractEmployee : Employee
{
    public override Money CalculateNet()
    {
        // author assumed base still ran — only added bonus locally
        return BaseSalary + ContractBonus;
    }
}
```

---

#### Q5. (P) A teammate replaces the polymorphic payroll loop with explicit type checks "for clarity." New `ContractEmployee` rows are added to the database but never appear in the exported total. Review the method. What failed at runtime, and what pattern from this chapter should drive payroll aggregation instead?

```csharp
public static Money ProcessPayroll(IEnumerable<Employee> staff)
{
    Money total = new(0m);

    foreach (Employee employee in staff)
    {
        if (employee is PermanentEmployee permanent)
            total += permanent.CalculateNet();
        else if (employee is Manager manager)
            total += manager.CalculateNet();
        // ContractEmployee and future types not handled
    }

    return total;
}
```

---

#### Q6. (D) Product wants `Employee` to inherit from a shared `AuditableEntity` base that already inherits `EntityBase`, while payroll still needs `Person → Employee → PermanentEmployee → Manager`. The team also proposes `Employee : Department` so every employee "is a department" for reporting. What breaks in C#, and where do LSP and fragile-base-class risks show up even if it compiles?

---
