using DbFirstOperations.Models;

namespace DbFirstOperations
{
    class Program
    {
        public static void Main(string[] args)
        {
            using(var context = new SchoolDbContext())
            {
                // -------------- Saving Data To Database -----------------
                //StudentAddress ad = new StudentAddress()
                //{
                //    AddressId = 2,
                //    City = "Malegaon",
                //    Locality = "Joyti Nagar",
                //    Pincode = "423203"
                //};

                //Student stu = new Student()
                //{
                //    RollNo = 102,
                //    Name = "Aakash Khainrar",
                //    Division = "M",
                //    Marks = 83.12M,
                //    AddressId = 2
                //};
                //context.Add(ad);
                //context.Add(stu);

                //context.SaveChanges();

                //Console.WriteLine("Student Object Saved In Database !!");


                // ----------- Querying LINQ To Entits -----------
                Student student = context.Students.Where(s => s.Name == "Darshan Khainrar").FirstOrDefault<Student>();

                Console.WriteLine($"Name : {student.Name}\nAddress : {student.RollNo}");

                Student student1 = context.Students.Find(102);
                Console.WriteLine($"Name : {student1.Name}\nAddress : {student1.RollNo}");

                // 

               
            }

        }
    }
}