using CodeFirstApproach.Models;

namespace CodeFirstApproach
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new SchoolContext())
            {
                // 
                //var grade = new Grade()
                //{
                //    GradeName = "A",
                //    Section = "First"
                //};

                //var student = new Student()
                //{
                //    StudentName = "Darshan",
                //    Height = 5.3M,
                //    Weight = 74,
                //    DateOfBirth = DateTime.Parse("2000-07-12"),
                //    Grade = grade
                //};

                //context.Students.Add(student);
                //context.SaveChanges();

                // Querying Data

                var students = from stu in context.Students
                               select stu;

                foreach(var stu in students)
                {
                    Console.WriteLine($"Student Name : {stu.StudentName}\nStudent Id : {stu.StudentID}");
                }
            }

        }
    }
}