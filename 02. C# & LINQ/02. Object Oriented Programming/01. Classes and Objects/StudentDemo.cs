namespace ClassesAndObjects;

public class Student
{
    public int rollNumber { get; set; } = 0;
    public string studentName { get; set; } = string.Empty;
    public DateTime dateOfBirth { get; set; } = DateTime.MinValue;
    public int age { get; set; } = 0;
    public double percentages { get; set; } = 0.0;
    public string address { get; set; } = string.Empty;

    public override string? ToString()
    {
        return $"Name : {studentName}\nRoll No : {rollNumber}\nDate of Birth : {dateOfBirth}\nAge : {age}\nPercentages : {percentages}\nAddress : {address}\n-----------------------------";
    }
}

public static class StudentDemo
{
    public static void Run()
    {
        Student s1 = new Student
        {
            rollNumber = 101,
            studentName = "Darshan Khairnar",
            age = 15,
            dateOfBirth = new DateTime(2000, 12, 7),
            percentages = 78.52,
            address = "Anand Nagar, Malegaon, Soygaon"
        };

        Console.WriteLine(s1);
    }
}
