/*
 * TOPIC: Building type hierarchies — sharing behavior through base classes,
 *        specializing with derived types, and invoking the right implementation
 *        at runtime through polymorphism.
 *
 * WHY IT MATTERS:
 *   Payroll systems treat contract staff, permanent staff, and managers differently
 *   yet share common fields (name, department, base salary). Inheritance models
 *   the IS-A relationship (a Manager is a PermanentEmployee is an Employee).
 *   Polymorphism lets one loop process every employee type without switch-on-type.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Inheritance syntax (`: BaseClass`)
 *   2.  Types of inheritance (single, multilevel, hierarchical)
 *   3.  No multiple class inheritance in C#
 *   4.  Generalization and specialization
 *   5.  The `base` keyword
 *   6.  Method overriding (`virtual` / `override`)
 *   7.  Runtime polymorphism
 *   8.  Method hiding (`new`)
 *   9.  IS-A vs HAS-A (composition)
 *  10.  Using inheritance in application design
 *  11.  Operator overloading
 *  12.  Preview: multiple inheritance via interfaces
 *  13.  Preview: sealed override
 */

using System;
using System.Globalization;

namespace InheritanceAndPolymorphism;

/*
 * SECTION 1: INHERITANCE AND THE IS-A RELATIONSHIP
 *
 * INHERITANCE lets a derived class reuse and extend a base class. The derived
 * type IS-A base type — a Manager can be used wherever an Employee is expected.
 *
 * Syntax:
 *
 *   class Derived : Base { }
 *
 * C# supports only SINGLE inheritance for classes: each class has at most one
 * direct base class. Every class ultimately derives from System.Object.
 *
 * Inheritance patterns (all single-chain per class):
 *
 *   Pattern          | Example in this chapter
 *   -----------------|--------------------------------------------------
 *   Single           | ContractEmployee : Employee (one parent)
 *   Multilevel       | Person → Employee → PermanentEmployee → Manager
 *   Hierarchical     | ContractEmployee : Employee
 *                    | PermanentEmployee : Employee (same base, siblings)
 *
 * MULTIPLE CLASS INHERITANCE is not supported:
 *
 *   class X : Employee, Person   // CS1721 — cannot have two base classes
 *
 * Use interfaces (Section 12 preview) when a type must combine several contracts.
 *
 * Scenario: Acme Corp payroll for one pay period. Values are fixed so the
 * program runs without keyboard input.
 */

/*
 * SECTION 2: HAS-A COMPOSITION — Department
 *
 * IS-A  — inheritance: Manager IS-A PermanentEmployee IS-A Employee
 * HAS-A — composition: Employee HAS-A Department (field, not `: Department`)
 *
 * Prefer composition when the relationship is "has/part-of" rather than
 * "is-a-kind-of". A Department is not a type of Employee — employees belong
 * to departments.
 *
 * Over-using inheritance for HAS-A relationships creates fragile hierarchies
 * (e.g. inheriting Address from Employee would be wrong).
 *
 *   public Department HomeDepartment { get; }   // HAS-A on Employee below
 */
public class Department
{
    public string Code { get; }
    public string Name { get; }

    public Department(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public override string ToString()
    {
        return $"{Code} — {Name}";
    }
}

/*
 * SECTION 3: OPERATOR OVERLOADING — Money
 *
 * Operators can be defined as static methods on a type:
 *
 *   public static Money operator +(Money left, Money right)
 *
 * Rules:
 *   • At least one operand must be the declaring type (Money here)
 *   • Cannot invent new operators; only existing C# operators
 *   • Keep overloads intuitive — + should mean addition for Money
 *
 * Used throughout CalculateNet() — BaseSalary + Perks - Deduction.
 */
public readonly struct Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Amount + right.Amount);
    }

    public static Money operator -(Money left, Money right)
    {
        return new Money(left.Amount - right.Amount);
    }

    public static Money operator -(Money value)
    {
        return new Money(-value.Amount);
    }

    public override string ToString()
    {
        return Amount.ToString("C", CultureInfo.CurrentCulture);
    }
}

/*
 * SECTION 4: MULTILEVEL INHERITANCE — Person
 *
 * Person is the root of the payroll hierarchy. Multilevel inheritance chains
 * common identity fields once at the top:
 *
 *   Person                          (root — level 0)
 *     └── Employee                    (generalization — shared payroll contract)
 *           ├── ContractEmployee      (specialization — hierarchical branch A)
 *           └── PermanentEmployee     (specialization — hierarchical branch B)
 *                 └── Manager         (multilevel — extends PermanentEmployee)
 *
 * Derived constructors forward arguments to the base constructor:
 *
 *   public Employee(...) : base(id, fullName) { }
 *
 * Without `: base(...)`, the parameterless base constructor runs (if it exists).
 * If the base has only parameterized constructors and you omit `: base(...)`,
 * the compiler reports CS7036 (no suitable constructor).
 */
public class Person
{
    public int Id { get; }
    public string FullName { get; }

    public Person(int id, string fullName)
    {
        Id = id;
        FullName = fullName;
    }
}

/*
 * SECTION 5: GENERALIZATION — Employee
 *
 * GENERALIZATION moves common facts up the tree — Employee holds Id, Name,
 * Department, and BaseSalary shared by every payroll role.
 *
 * virtual members can be overridden in derived types for runtime polymorphism:
 *
 *   public virtual Money CalculateNet() { return BaseSalary; }
 *
 * Without `virtual` on the base member, `override` in a derived class fails
 * with CS0506.
 *
 * GetBadgeCode is intentionally NON-virtual — derived types will hide it with
 * `new` in Section 8 (method hiding vs overriding).
 */
public class Employee : Person
{
    public Department HomeDepartment { get; }
    public Money BaseSalary { get; }

    public Employee(int id, string fullName, Department homeDepartment, Money baseSalary)
        : base(id, fullName)
    {
        HomeDepartment = homeDepartment;
        BaseSalary = baseSalary;
    }

    public virtual string RoleLabel => "Employee";

    public virtual Money CalculateNet()
    {
        return BaseSalary;
    }

    public string GetBadgeCode()
    {
        return "EMP";
    }

    public override string ToString()
    {
        return $"{FullName} ({RoleLabel}) @ {HomeDepartment}";
    }
}

/*
 * SECTION 6: SPECIALIZATION — ContractEmployee
 *
 * SPECIALIZATION adds role-specific data and behavior lower in the tree.
 * ContractEmployee adds ContractBonus and overrides CalculateNet().
 *
 * Design rule: generalize only what every subtype truly shares; specialize
 * what differs. Over-generalizing produces bloated base classes.
 *
 * --- 6a. Method overriding (`override`) ---
 *
 *   public override Money CalculateNet()
 *   {
 *       return BaseSalary + ContractBonus;
 *   }
 *
 * Both methods must share the same signature. Runtime polymorphism (Section 7)
 * invokes the derived override when the object is a ContractEmployee.
 */
public class ContractEmployee : Employee
{
    public Money ContractBonus { get; }

    public ContractEmployee(int id, string fullName, Department homeDepartment, Money baseSalary, Money contractBonus)
        : base(id, fullName, homeDepartment, baseSalary)
    {
        ContractBonus = contractBonus;
    }

    public override string RoleLabel => "Contract";

    public override Money CalculateNet()
    {
        return BaseSalary + ContractBonus;
    }

    /*
     * --- 6b. Method hiding (`new`) ---
     *
     * When a base method is NOT virtual, a derived type can HIDE it with `new`:
     *
     *   public new string GetBadgeCode() => "CTR";
     *
     * Hiding is compile-time binding on the reference type:
     *
     *   Employee e = new ContractEmployee(...);
     *   e.GetBadgeCode();           // "EMP" — Employee's method (no override)
     *
     *   ContractEmployee c = new ContractEmployee(...);
     *   c.GetBadgeCode();           // "CTR" — derived hidden member
     *
     * Prefer virtual/override for polymorphic behavior. Use `new` only when
     * you intentionally want different behavior per static type.
     */
    public new string GetBadgeCode()
    {
        return "CTR";
    }
}

/*
 * SECTION 7: SPECIALIZATION — PermanentEmployee
 *
 * Second hierarchical branch under Employee (sibling of ContractEmployee).
 * Adds perks and provident-fund deduction to the net-pay calculation.
 */
public class PermanentEmployee : Employee
{
    public Money Perks { get; }
    public Money ProvidentFundDeduction { get; }

    public PermanentEmployee(
        int id,
        string fullName,
        Department homeDepartment,
        Money baseSalary,
        Money perks,
        Money providentFundDeduction)
        : base(id, fullName, homeDepartment, baseSalary)
    {
        Perks = perks;
        ProvidentFundDeduction = providentFundDeduction;
    }

    public override string RoleLabel => "Permanent";

    public override Money CalculateNet()
    {
        return BaseSalary + Perks - ProvidentFundDeduction;
    }
}

/*
 * SECTION 8: THE `base` KEYWORD — Manager
 *
 * `base` refers to the immediate parent class instance:
 *
 *   base(...)           — call parent constructor from derived constructor
 *   base.MethodName()   — invoke parent implementation from override
 *
 * Manager.CalculateNet() calls base.CalculateNet() to reuse permanent salary
 * logic, then adds TeamBonus — avoid duplicating PF/perks math.
 *
 * engineeringLead.Id comes from Person via Employee's `: base(id, ...)`.
 */
public class Manager : PermanentEmployee
{
    public Money TeamBonus { get; }

    public Manager(
        int id,
        string fullName,
        Department homeDepartment,
        Money baseSalary,
        Money perks,
        Money providentFundDeduction,
        Money teamBonus)
        : base(id, fullName, homeDepartment, baseSalary, perks, providentFundDeduction)
    {
        TeamBonus = teamBonus;
    }

    public override string RoleLabel => "Manager";

    public override Money CalculateNet()
    {
        return base.CalculateNet() + TeamBonus;
    }

    public new string GetBadgeCode()
    {
        return "MGR";
    }
}

public class Program
{
    /*
     * SECTION 9: PAYROLL DEMONSTRATION
     *
     * Main wires the chapter demo — create objects, exercise inheritance,
     * polymorphism, hiding, and composition, then print results. Concept
     * explanations live above the types they describe; this method orchestrates
     * the runnable example.
     *
     * --- 9a. Runtime polymorphism ---
     *
     * A base reference can point at any derived object. The method actually
     * invoked is chosen at RUNTIME from the object's real type (virtual dispatch):
     *
     *   Employee staff = new ContractEmployee(...);
     *   staff.CalculateNet();   // ContractEmployee's override runs
     *
     * ProcessPayroll sums CalculateNet() for every entry — no role switch.
     *
     * --- 9b. Using inheritance in application design ---
     *
     * Good design:
     *   • Put shared contracts on the base (CalculateNet, RoleLabel)
     *   • Process collections as base type (Employee[])
     *   • Add new derived types without changing ProcessPayroll
     *
     * Avoid:
     *   • Deep trees solely for code reuse — consider composition/services
     *   • Switch on concrete type when virtual methods already express the rule
     *
     * --- 9c. Multiple inheritance via interfaces (PREVIEW) ---
     *
     * COVERED IN DETAIL LATER → 06. Abstract Classes and Interfaces
     *   (headline concepts only: a class inherits one base class plus many
     *    interfaces — e.g. class Manager : Employee, IPayrollExportable, ILead)
     *
     * --- 9d. Sealed override (PREVIEW) ---
     *
     * COVERED IN DETAIL LATER → 09. OOP Real-World Examples
     *   (headline concepts only: `public sealed override Money CalculateNet()`
     *    prevents further overriding in subclasses — useful when a leaf type
     *    must lock behavior)
     */
    public static void Main(string[] args)
    {
        Department engineering = new Department("ENG", "Engineering");
        Department finance = new Department("FIN", "Finance");

        ContractEmployee contractDev = new ContractEmployee(
            101,
            "Riya Sharma",
            engineering,
            new Money(72000m),
            new Money(3000m));

        PermanentEmployee permanentAnalyst = new PermanentEmployee(
            102,
            "Marcus Lee",
            finance,
            new Money(85000m),
            new Money(5000m),
            new Money(1200m));

        Manager engineeringLead = new Manager(
            103,
            "Elena Vasquez",
            engineering,
            new Money(98000m),
            new Money(6000m),
            new Money(1500m),
            new Money(4000m));

        bool devIsContract = contractDev is ContractEmployee;
        bool devIsEmployee = contractDev is Employee;
        bool devIsPerson = contractDev is Person;

        object payrollEntry = contractDev;
        bool devIsManager = payrollEntry is Manager;

        bool leadIsPermanent = engineeringLead is PermanentEmployee;
        bool leadIsEmployee = engineeringLead is Employee;
        bool leadIsPerson = engineeringLead is Person;
        int inheritanceDepthLevels = CountInheritanceDepth(engineeringLead);

        string generalizedRole = contractDev.RoleLabel;
        string specializedContractBonus = contractDev.ContractBonus.ToString();
        string specializedManagerBonus = engineeringLead.TeamBonus.ToString();

        Money managerViaBase = engineeringLead.CalculateNet();
        Money permanentOnlyBase = permanentAnalyst.CalculateNet();
        int managerIdFromPersonChain = engineeringLead.Id;

        string contractRole = contractDev.RoleLabel;
        string permanentRole = permanentAnalyst.RoleLabel;
        string managerRole = engineeringLead.RoleLabel;

        Employee[] payrollStaff =
        {
            contractDev,
            permanentAnalyst,
            engineeringLead
        };

        Money totalPayroll = ProcessPayroll(payrollStaff);
        Money polymorphicContractNet = InvokeCalculateNetThroughBaseReference(contractDev);

        string badgeViaEmployeeRef = GetBadgeThroughEmployeeReference(contractDev);
        string badgeViaContractRef = contractDev.GetBadgeCode();
        string badgeViaManagerRef = engineeringLead.GetBadgeCode();

        bool employeeHasDepartment = contractDev.HomeDepartment is Department;
        bool departmentIsEmployee = typeof(Employee).IsAssignableFrom(typeof(Department));
        string composedDepartmentLabel = contractDev.HomeDepartment.ToString();

        int staffCount = payrollStaff.Length;
        Money averagePay = new Money(totalPayroll.Amount / staffCount);

        Money grossComponents = contractDev.BaseSalary + contractDev.ContractBonus;
        Money netAfterDeduction = permanentAnalyst.BaseSalary + permanentAnalyst.Perks - permanentAnalyst.ProvidentFundDeduction;
        Money negatedSample = -new Money(500m);

        string interfacePreviewNote =
            "One base class + multiple interfaces — full lesson in ch.06";

        string sealedOverridePreviewNote =
            "sealed override locks a virtual method — see 09. OOP Real-World Examples";

        Console.WriteLine("=== Inheritance and Polymorphism — Acme Payroll ===");
        Console.WriteLine();
        Console.WriteLine($"Staff: {contractDev.FullName}, {permanentAnalyst.FullName}, {engineeringLead.FullName}");
        Console.WriteLine();
        Console.WriteLine("--- Section 1–2: Inheritance syntax / IS-A / HAS-A ---");
        Console.WriteLine($"Riya is ContractEmployee: {devIsContract} | Employee: {devIsEmployee} | Person: {devIsPerson} | Manager: {devIsManager}");
        Console.WriteLine($"Employee HAS-A Department: {employeeHasDepartment} | Department IS-A Employee: {departmentIsEmployee}");
        Console.WriteLine($"Riya's department: {composedDepartmentLabel}");
        Console.WriteLine();
        Console.WriteLine("--- Section 1: Inheritance types ---");
        Console.WriteLine($"Elena — PermanentEmployee: {leadIsPermanent} | Employee: {leadIsEmployee} | Person: {leadIsPerson} | chain depth: {inheritanceDepthLevels}");
        Console.WriteLine("Multiple class bases not allowed (CS1721) — use interfaces for extra contracts");
        Console.WriteLine();
        Console.WriteLine("--- Section 6: Generalization / specialization ---");
        Console.WriteLine($"General RoleLabel: {generalizedRole} | Contract bonus: {specializedContractBonus} | Manager team bonus: {specializedManagerBonus}");
        Console.WriteLine();
        Console.WriteLine("--- Section 8: base keyword ---");
        Console.WriteLine($"Manager net (base + team): {managerViaBase} | Permanent net: {permanentOnlyBase} | Id from Person chain: {managerIdFromPersonChain}");
        Console.WriteLine();
        Console.WriteLine("--- Section 5–6: virtual / override ---");
        Console.WriteLine($"Roles — Contract: {contractRole} | Permanent: {permanentRole} | Manager: {managerRole}");
        Console.WriteLine();
        Console.WriteLine("--- Section 9a: Runtime polymorphism ---");
        Console.WriteLine($"Polymorphic net via Employee ref: {polymorphicContractNet}");
        Console.WriteLine($"Total payroll ({staffCount} staff): {totalPayroll} | Average: {averagePay}");
        Console.WriteLine();
        Console.WriteLine("--- Section 6b: Method hiding (new) ---");
        Console.WriteLine($"Badge via Employee ref: {badgeViaEmployeeRef} | via Contract ref: {badgeViaContractRef} | via Manager ref: {badgeViaManagerRef}");
        Console.WriteLine();
        Console.WriteLine("--- Section 3: Operator overloading ---");
        Console.WriteLine($"Contract gross (Base + Bonus): {grossComponents}");
        Console.WriteLine($"Permanent net components: {netAfterDeduction} | Unary minus sample: {negatedSample}");
        Console.WriteLine();
        Console.WriteLine("--- Previews ---");
        Console.WriteLine($"Interfaces: {interfacePreviewNote}");
        Console.WriteLine($"Sealed override: {sealedOverridePreviewNote}");
        Console.WriteLine();
        Console.WriteLine("--- Per-employee breakdown (polymorphic loop) ---");

        foreach (Employee employee in payrollStaff)
        {
            Console.WriteLine($"  {employee} | Net: {employee.CalculateNet()} | Badge (Employee ref): {GetBadgeThroughEmployeeReference(employee)}");
        }
    }

    private static Money ProcessPayroll(Employee[] staff)
    {
        Money runningTotal = new Money(0m);

        foreach (Employee employee in staff)
        {
            runningTotal = runningTotal + employee.CalculateNet();
        }

        return runningTotal;
    }

    private static Money InvokeCalculateNetThroughBaseReference(ContractEmployee source)
    {
        Employee polymorphicReference = source;
        return polymorphicReference.CalculateNet();
    }

    private static string GetBadgeThroughEmployeeReference(Employee employee)
    {
        return employee.GetBadgeCode();
    }

    private static int CountInheritanceDepth(object instance)
    {
        int depth = 0;
        Type? current = instance.GetType();

        while (current != null && current != typeof(object))
        {
            depth++;
            current = current.BaseType;
        }

        return depth;
    }
}

/*
 * QUICK REFERENCE — INHERITANCE AND POLYMORPHISM
 *
 * --- Syntax ---
 *
 *  class Derived : Base { }              // single direct base class
 *  public Derived(...) : base(...) { }  // constructor chain
 *  base.Method();                        // call parent implementation
 *
 * --- Inheritance patterns (classes) ---
 *
 *  Single        | B : A
 *  Multilevel    | C : B : A
 *  Hierarchical  | B : A and C : A (siblings)
 *  Multiple class| NOT allowed (CS1721) — one base class only
 *
 * --- Override vs hide ---
 *
 *  Base: virtual void M()     Derived: override void M()   → runtime polymorphism
 *  Base: void M()             Derived: new void M()         → hides; static binding
 *
 * --- Polymorphism ---
 *
 *  Base ref = new Derived();
 *  ref.OverriddenMethod();     // Derived body runs (virtual dispatch)
 *  ref.HiddenMethod();         // Base body if not virtual (reference type wins)
 *
 * --- Design ---
 *
 *  IS-A          | use inheritance (Employee → Manager)
 *  HAS-A         | use composition (Employee has Department)
 *  Generalize    | common state/behavior in base
 *  Specialize    | unique fields/overrides in derived
 *
 * --- Operator overloading ---
 *
 *  public static T operator +(T a, T b) { ... }
 *  At least one operand must be type T; cannot create new operator symbols
 *
 * --- Preview ---
 *
 *  Multiple contracts | class X : Base, IOne, ITwo     → ch.06 Interfaces
 *  Lock override      | sealed override void M()         → 09. OOP Real-World Examples
 *
 * --- Common errors ---
 *
 *  Mistake                         | Result
 *  --------------------------------|----------------------------------------
 *  override without virtual base   | CS0506
 *  two base classes                | CS1721
 *  expect hide to polymorph        | Wrong method runs via base reference
 *  inherit for HAS-A               | Fragile design — prefer composition
 */
