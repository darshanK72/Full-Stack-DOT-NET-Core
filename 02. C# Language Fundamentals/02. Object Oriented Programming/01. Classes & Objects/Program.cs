/*
 * =============================================================================
 * 01. CLASSES AND OBJECTS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Object-oriented programming in C# — defining classes, creating
 *        objects, instance fields and methods, constructors, reference
 *        semantics, null, and the relationship between classes and System.Object.
 *
 * WHY IT MATTERS:
 *   Real applications model domain concepts (students, orders, accounts) as
 *   types with data and behavior. Classes group related state and operations;
 *   objects are the live instances your program manipulates at runtime.
 *
 * WHAT YOU WILL LEARN:
 *   1.  OOP overview — four pillars (preview)
 *   2.  Class vs object — blueprint and instance
 *   3.  Defining a class — fields and instance methods
 *   4.  Creating objects with new and a parameterized constructor
 *   5.  this — disambiguate fields from parameters
 *   6.  Parameterless constructor and default field values
 *   7.  Preview: static field on a class (CustomerCount)
 *   8.  Override ToString for readable output (preview)
 *   9.  Reference semantics — shared vs separate instances
 *  10.  null references and NullReferenceException
 *  11.  typeof, GetType(), is, and as
 *  12.  Preview: every class inherits System.Object
 *  13.  Preview: nullable reference types (string vs string?)
 *  14.  Preview: garbage collection and reachability
 *  15.  Preview: struct vs class (value vs reference types)
 *
 * =============================================================================
 */

using System;

namespace ClassesAndObjects;

/*
 * =========================================================================
 * SECTION 1: OOP OVERVIEW — FOUR PILLARS (PREVIEW)
 * =========================================================================
 *
 * Object-oriented programming organizes code around objects that combine
 * data and behavior. Four pillars guide design:
 *
 *   Pillar          | Headline idea
 *   ----------------|--------------------------------------------------
 *   Encapsulation   | Hide internal state; expose controlled access
 *   Abstraction     | Show essential behavior; hide implementation
 *   Inheritance     | Reuse and extend existing types
 *   Polymorphism    | One interface, many concrete behaviors
 *
 * COVERED IN DETAIL LATER → 07. Encapsulation and Access Modifiers
 * COVERED IN DETAIL LATER → 05. Inheritance and Polymorphism
 * COVERED IN DETAIL LATER → 06. Abstract Classes and Interfaces
 *
 * This chapter focuses on the building blocks: defining a class and creating
 * objects from it.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: CLASS VS OBJECT — DEFINING A TYPE
 * =========================================================================
 *
 * CLASS  — a type definition (blueprint). Describes fields and methods every
 *          instance will have. Written once in source code.
 *
 * OBJECT — a concrete instance of a class created at runtime. Each object has
 *          its own copy of instance fields.
 *
 * Terminology:
 *   Student   → class (the blueprint)
 *   student1  → object / instance (one live student in memory)
 *
 * --- 2a. Fields (instance state) ---
 *
 * FIELDS are variables declared inside a class. Each object gets its own field
 * values. Public fields are used here for clarity; production code usually
 * prefers properties (see chapter 02).
 *
 * --- 2b. Instance methods ---
 *
 * METHODS declared without static belong to each instance. They read and update
 * that object's fields: AssignDetails, Rename.
 *
 * --- 2c. Parameterized constructor ---
 *
 * A CONSTRUCTOR is a special method with the same name as the class and no
 * return type. It runs when you use new. Student(string, int) sets required
 * identity fields at creation time.
 *
 * COVERED IN DETAIL LATER → 03. Constructors and Method Overloading
 *   (overloaded ctors, :this(), :base(), static ctor, defaults)
 *
 * --- 2d. this keyword ---
 *
 * this refers to the current instance inside an instance method or constructor.
 *
 *   Use                          | Example
 *   -----------------------------|----------------------------------------
 *   Disambiguate field vs param  | this.StudentName = studentName;
 *   Pass current object          | SomeMethod(this);
 *   Chain constructors           | :this(...) — see chapter 03
 *
 * --- 2e. Override ToString (preview) ---
 *
 * Every class inherits ToString() from System.Object. Override it to return a
 * meaningful label when the object is printed or interpolated.
 *
 * COVERED IN DETAIL LATER → 02. Properties and Indexers (ToString depth)
 * -------------------------------------------------------------------------
 */
public class Student
{
    // Instance fields — each Student object gets its own copy of these values
    public int RollNumber;
    public string StudentName = string.Empty; // field initializer runs before constructor body
    public DateTime DateOfBirth;
    public int Age;
    public double Percentage;
    public string Address = string.Empty;

    // Parameterized constructor — runs when you write new Student(...)
    public Student(string studentName, int rollNumber)
    {
        this.StudentName = studentName; // this. disambiguates field from parameter
        this.RollNumber = rollNumber;
        this.Address = string.Empty;      // explicit non-null default for reference field
    }

    // Instance method — operates on the current object's fields
    public void AssignDetails(DateTime dateOfBirth, int age, double percentage, string address)
    {
        this.DateOfBirth = dateOfBirth;
        this.Age = age;
        this.Percentage = percentage;
        this.Address = address;
    }

    public void Rename(string studentName)
    {
        this.StudentName = studentName; // mutates only this instance's field
    }

    // override replaces inherited Object.ToString() for readable output
    public override string ToString()
    {
        return $"Name: {StudentName} | Roll: {RollNumber} | Age: {Age} | %: {Percentage:F2} | Address: {Address}";
    }
}

/*
 * =========================================================================
 * SECTION 3: PARAMETERLESS CONSTRUCTOR AND DEFAULT FIELD VALUES
 * =========================================================================
 *
 * If you declare NO constructors, the compiler supplies a parameterless default
 * constructor. Once you add ANY constructor (like Student above), the compiler
 * stops generating a default — callers must use a constructor you provide.
 *
 * Customer declares an explicit parameterless constructor that:
 *   - Assigns a sequential Id from a shared counter
 *   - Initializes Name to string.Empty (non-null default for reference field)
 *
 * --- 3a. Preview: static field ---
 *
 * static members belong to the TYPE, not to any single instance. CustomerCount
 * tracks how many Customer objects have been constructed.
 *
 * COVERED IN DETAIL LATER → 04. Static Members and Static Classes
 *
 * --- 3b. Instance method using fields ---
 *
 * CalculateTotalInterest reads LoanAmount, RateOfInterest, and DurationOfLoan
 * from the current Customer instance and returns a computed value.
 * -------------------------------------------------------------------------
 */
public class Customer
{
    public static int CustomerCount = 0; // shared across all Customer instances — lives on the type

    // Per-instance state — each Customer has its own Id, Name, loan details
    public int Id;
    public string Name = string.Empty;
    public double LoanAmount;
    public double RateOfInterest;
    public double DurationOfLoan;

    // Parameterless constructor — no arguments required at new Customer()
    public Customer()
    {
        Id = ++CustomerCount; // pre-increment assigns 1, 2, 3… as objects are created
        Name = string.Empty;
    }

    // Reads instance fields and returns a computed result (does not mutate state)
    public double CalculateTotalInterest()
    {
        return (LoanAmount * RateOfInterest * DurationOfLoan) / 100.0; // simple interest formula
    }

    public override string ToString()
    {
        return $"Customer #{Id} {Name} — total interest: {CalculateTotalInterest():F2}";
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 4: DEMONSTRATION — CREATING OBJECTS WITH new
     * =========================================================================
     *
     * new T() does two things:
     *   1. Allocates memory for a new instance of type T on the managed heap.
     *   2. Invokes a constructor to initialize the object's fields.
     *
     * After construction, assign remaining field values and call instance methods.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        // new allocates heap memory and calls Student(string, int) constructor
        Student student1 = new Student("Darshan Khairnar", 101);
        student1.AssignDetails( // fills in fields not set by the constructor
            new DateTime(2000, 12, 7),
            15,
            78.52,
            "Anand Nagar, Malegaon, Soygaon");

        double reportedPercentage = student1.Percentage; // read a field value into a local variable
        student1.Rename("Darshan K.");                   // instance method mutates StudentName
        string renamedStudent = student1.StudentName;  // capture updated name for output later

        // Each new Customer() runs the parameterless ctor and bumps CustomerCount
        Customer parentOne = new Customer();
        parentOne.Name = "Meera Shah";           // set instance fields after construction
        parentOne.LoanAmount = 250_000;          // numeric literal separator for readability
        parentOne.RateOfInterest = 8.5;
        parentOne.DurationOfLoan = 5;

        Customer parentTwo = new Customer();     // separate object — own Id and field values
        parentTwo.Name = "Rahul Desai";
        parentTwo.LoanAmount = 180_000;
        parentTwo.RateOfInterest = 9.0;
        parentTwo.DurationOfLoan = 3;

        Customer parentThree = new Customer();
        parentThree.Name = "Anita Rao";
        parentThree.LoanAmount = 320_000;
        parentThree.RateOfInterest = 7.75;
        parentThree.DurationOfLoan = 7;

        double interestOne = parentOne.CalculateTotalInterest();   // each object computes from its own fields
        double interestTwo = parentTwo.CalculateTotalInterest();
        double interestThree = parentThree.CalculateTotalInterest();
        int customersCreated = Customer.CustomerCount; // static field — access via type name, not an instance


        /*
         * =========================================================================
         * SECTION 5: REFERENCE SEMANTICS
         * =========================================================================
         *
         * Reference types (classes) are accessed through REFERENCES — variables
         * store a pointer to the object on the heap, not the object itself.
         *
         * --- 5a. Two variables, one object ---
         *
         *   Student enrolled = student1;
         *   Student sameRef  = enrolled;     // copies the reference, not the object
         *
         * Both variables point to the same instance. Changing a field through
         * either variable affects the single shared object.
         *
         * --- 5b. Two variables, two objects ---
         *
         *   Student another = new Student(...);   // separate allocation
         *
         * ReferenceEquals(a, b) returns true only when a and b refer to the
         * exact same instance. For classes, == compares references by default
         * (unless overloaded — chapter 02).
         * -------------------------------------------------------------------------
         */

        Student enrolled = student1;              // copies reference — both vars point to same object
        Student sameReference = enrolled;         // another copy of the same reference
        sameReference.RollNumber = 999;           // mutates the shared object on the heap
        bool sharedMutation = student1.RollNumber == 999; // true — student1 sees the change too

        Student separateStudent = new Student("Priya Nair", 102); // new allocation — independent object
        separateStudent.AssignDetails(new DateTime(2001, 3, 15), 14, 91.0, "Pune");

        bool sameInstance = ReferenceEquals(enrolled, sameReference);           // true — same heap object
        bool differentInstances = !ReferenceEquals(student1, separateStudent);  // true — different objects
        bool equalitySameRef = enrolled == sameReference;       // == compares references for classes (default)
        bool equalityDifferent = student1 == separateStudent; // false — different instances

        enrolled.Rename("Darshan Khairnar");     // Rename on enrolled affects student1 (same object)
        string nameViaThis = enrolled.StudentName;


        /*
         * =========================================================================
         * SECTION 6: null REFERENCES
         * =========================================================================
         *
         * A reference variable can hold null — meaning "points to no object."
         *
         *   Student? missing = null;
         *
         * Dereferencing null (missing.RollNumber) throws NullReferenceException.
         * Always check before use, or use null-conditional ?. (C# 6+).
         *
         * default(Student) and default for a reference type also produce null.
         * -------------------------------------------------------------------------
         */

        Student? missingStudent = null;          // ? annotation — nullable reference type
        bool missingIsNull = missingStudent is null; // pattern check — safe, no dereference
        Student defaultStudent = default!;       // default for reference type is null; ! suppresses warning here
        bool defaultRefIsNull = defaultStudent is null;

        // ?. returns null instead of throwing; ?? supplies fallback when left side is null
        string nullSafeName = missingStudent?.StudentName ?? "(no student)";
        string nullCoalesceDemo = nullSafeName;


        /*
         * =========================================================================
         * SECTION 7: typeof, GetType(), is, and as
         * =========================================================================
         *
         *   Operator / API     | Purpose
         *   -------------------|------------------------------------------------
         *   typeof(Student)    | Type metadata from compile-time type name
         *   obj.GetType()      | Runtime type of the actual object
         *   obj is Student     | Pattern test — true if object is compatible type
         *   obj as Student     | Safe cast — returns null if incompatible
         *
         * GetType() on a variable declared as a base type reveals the derived
         * runtime type (preview of polymorphism in chapter 05).
         * -------------------------------------------------------------------------
         */

        Type studentTypeFromKeyword = typeof(Student); // compile-time Type from type name
        string studentTypeName = student1.GetType().Name; // runtime type of the actual object
        bool studentIsStudentType = student1 is Student;    // true — student1 is a Student instance

        object boxedAsObject = student1;                    // implicit upcast to System.Object
        bool objectIsCustomerType = boxedAsObject is Customer; // false — runtime type is Student

        Student? recoveredViaAs = boxedAsObject as Student;  // safe downcast — succeeds, not null
        bool asRecoveredNotNull = recoveredViaAs is not null;

        Customer? wrongCast = boxedAsObject as Customer;     // incompatible cast — returns null, no exception
        bool asReturnedNull = wrongCast is null;


        /*
         * =========================================================================
         * SECTION 8: System.Object — BASE OF ALL CLASSES (PREVIEW)
         * =========================================================================
         *
         * Every class implicitly inherits System.Object. You get:
         *   ToString(), Equals(object), GetHashCode(), GetType()
         *
         * Student and Customer override ToString(); Equals/GetHashCode depth
         * continues in chapter 02.
         * -------------------------------------------------------------------------
         */

        bool studentIsObject = separateStudent is object; // every class instance is an object
        string objectToString = parentOne.ToString();     // calls overridden ToString on Customer
        int studentHash = parentOne.GetHashCode();        // inherited from Object — hash code for collections
        bool studentEqualsSelf = ReferenceEquals(parentOne, parentOne); // same reference — always true


        /*
         * =========================================================================
         * SECTION 9: NULLABLE REFERENCE TYPES ON CLASSES (PREVIEW)
         * =========================================================================
         *
         * With <Nullable>enable</Nullable>, reference-type annotations express
         * intent: string means non-null expected; string? allows null.
         *
         *   string  requiredName = "Ada";   // should not be null
         *   string? optionalNote = null;    // explicitly nullable
         *
         * The compiler warns when you assign null to string or dereference a
         * possibly-null string? without checking.
         * -------------------------------------------------------------------------
         */

        string requiredName = "Learning Center"; // non-nullable annotation — compiler expects a value
        string? optionalNote = null;             // nullable annotation — null is intentional
        bool noteIsNull = optionalNote is null;
        string displayName = requiredName;       // safe assignment — both are non-null strings


        /*
         * =========================================================================
         * SECTION 10: GARBAGE COLLECTION ON OBJECTS (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → 01. .NET Framework Architecture /
         *   09. Memory Management
         *
         * When no live reference points to an object, it becomes eligible for
         * garbage collection. The CLR reclaims that memory automatically — you
         * do not call delete as in C++.
         *
         *   gcCandidate = null;   // object may be collected if nothing else references it
         *
         * Deterministic cleanup (IDisposable, using) is a separate topic.
         * -------------------------------------------------------------------------
         */

        long heapBytesBefore = GC.GetTotalMemory(forceFullCollection: false); // snapshot before allocation
        Customer gcCandidate = new Customer(); // object exists while gcCandidate holds a reference
        gcCandidate.Name = "GC candidate";
        gcCandidate = null!;                     // drop reference — object becomes eligible for GC
        GC.Collect();                            // request collection (demo only — don't rely on this in apps)
        GC.WaitForPendingFinalizers();           // wait for finalizers before measuring again
        long heapBytesAfter = GC.GetTotalMemory(forceFullCollection: true);
        bool gcRan = heapBytesAfter >= 0;        // flag used so demo lines are not dead code


        /*
         * =========================================================================
         * SECTION 11: STRUCT VS CLASS (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → dedicated value-types / struct chapters
         *
         *   Feature        | class (reference type)     | struct (value type)
         *   ---------------|----------------------------|---------------------------
         *   Storage        | heap (usually)             | inline or stack (often)
         *   Assignment     | copies reference           | copies entire value
         *   Default        | null allowed               | cannot be null (unboxed)
         *   Inheritance    | supports                   | sealed — no inheritance
         *
         * DateTime is a struct; Student and Customer are classes.
         * -------------------------------------------------------------------------
         */

        DateTime birthDateCopy = student1.DateOfBirth; // struct assignment copies the value, not a reference
        birthDateCopy = birthDateCopy.AddYears(1);     // AddYears returns new DateTime — original unchanged
        bool structCopyIndependent = student1.DateOfBirth.Year != birthDateCopy.Year; // true — copies are independent
        bool studentIsClass = typeof(Student).IsClass;           // Student is a reference type (class)
        bool dateTimeIsValueType = typeof(DateTime).IsValueType; // DateTime is a struct (value type)


        /*
         * =========================================================================
         * SECTION 12: LEARNING CENTER SUMMARY (OUTPUT)
         * =========================================================================
         */

        Console.WriteLine("=== Classes and Objects — Learning Center ===");
        Console.WriteLine();
        Console.WriteLine($"Student object type: {studentTypeName} | is Student: {studentIsStudentType}");
        Console.WriteLine($"typeof name: {studentTypeFromKeyword.Name} | object is Customer: {objectIsCustomerType}");
        Console.WriteLine(student1); // implicit ToString() — calls overridden Student.ToString()
        Console.WriteLine($"Percentage field: {reportedPercentage:F2} | Renamed to: {renamedStudent}");
        Console.WriteLine();
        Console.WriteLine($"Customers created (static preview): {customersCreated}");
        Console.WriteLine(parentOne); // ToString includes CalculateTotalInterest() result
        Console.WriteLine($"  Interest: {interestOne:F2}");
        Console.WriteLine(parentTwo);
        Console.WriteLine($"  Interest: {interestTwo:F2}");
        Console.WriteLine(parentThree);
        Console.WriteLine($"  Interest: {interestThree:F2}");
        Console.WriteLine();
        Console.WriteLine($"Reference — shared mutation (Roll 999): {sharedMutation}");
        Console.WriteLine($"ReferenceEquals same ref: {sameInstance} | different instances: {differentInstances}");
        Console.WriteLine($"== same ref: {equalitySameRef} | == different: {equalityDifferent}");
        Console.WriteLine($"Separate student: {separateStudent}");
        Console.WriteLine($"this.Rename restored: {nameViaThis}");
        Console.WriteLine();
        Console.WriteLine($"null — missing is null: {missingIsNull} | default ref null: {defaultRefIsNull}");
        Console.WriteLine($"null-coalesce demo: {nullCoalesceDemo}");
        Console.WriteLine($"as — recovered Student: {asRecoveredNotNull} | wrong as Customer null: {asReturnedNull}");
        Console.WriteLine();
        Console.WriteLine($"Object base — is object: {studentIsObject} | GetHashCode: {studentHash}");
        Console.WriteLine($"Equals(self): {studentEqualsSelf} | ToString: {objectToString}");
        Console.WriteLine();
        Console.WriteLine($"Nullable preview — display: {displayName} | optional note is null: {noteIsNull}");
        Console.WriteLine($"GC preview — heap before: {heapBytesBefore} bytes | after collect: {heapBytesAfter} bytes | ran: {gcRan}");
        Console.WriteLine();
        Console.WriteLine($"Struct preview — DateTime copy independent: {structCopyIndependent}");
        Console.WriteLine($"Student is class: {studentIsClass} | DateTime is value type: {dateTimeIsValueType}");
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — CLASSES AND OBJECTS
 * =============================================================================
 *
 * --- Core terms ---
 *
 *   class     Type blueprint (fields + methods)
 *   object    Runtime instance of a class
 *   new       Allocate + run constructor
 *   field     Instance state variable
 *   method    Instance behavior (function on the class)
 *
 * --- Reference semantics ---
 *
 *   Student a = new Student(...);
 *   Student b = a;              // same object — two references
 *   ReferenceEquals(a, b)       // true when same instance
 *   a == b                      // reference compare unless == overloaded
 *
 * --- null ---
 *
 *   Student? s = null;
 *   s?.Field                   // null-conditional — no throw
 *   s ?? fallback              // null-coalescing
 *
 * --- Type inspection ---
 *
 *   typeof(Student)            // compile-time Type
 *   obj.GetType()              // runtime Type
 *   obj is Student             // pattern test
 *   obj as Student             // safe cast → null if fail
 *
 * --- this ---
 *
 *   this.Field = param;         // disambiguate in ctor/method
 *   :this(...)                  // ctor chaining — chapter 03
 *
 * --- Preview pointers ---
 *
 *   Properties / fields         → 02. Properties and Indexers
 *   Constructors depth          → 03. Constructors and Method Overloading
 *   static members              → 04. Static Members and Static Classes
 *   Inheritance / polymorphism  → 05, 06
 *   Encapsulation               → 07. Encapsulation and Access Modifiers
 *   Nullable string?            → C# 8 nullable reference types
 *   GC / heap                   → 09. Memory Management
 *   struct vs class             → value-type chapters
 *
 * --- Common errors ---
 *
 *   Mistake                     | Result
 *   ----------------------------|------------------------------------------
 *   Use instance member on class| CS0176 — need an object instance
 *   Dereference null reference  | NullReferenceException
 *   Confuse = (ref) with copy   | Two refs mutate same object unexpectedly
 *   Read unassigned field       | CS0165 / default values (0, null)
 *
 * =============================================================================
 */
