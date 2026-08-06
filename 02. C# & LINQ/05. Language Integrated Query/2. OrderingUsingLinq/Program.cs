namespace Practice2
{

    class Student
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public int Age { get; set; }
        public Student()
        {
        }

        public Student(int id, string name, int age)
        {
            this.StudentID = id;
            this.StudentName = name;
            this.Age = age;
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            IList<Student> studentList = new List<Student>() {
                new Student() { StudentID = 1, StudentName = "John", Age = 18 } ,
                new Student() { StudentID = 2, StudentName = "Steve",  Age = 15 } ,
                new Student() { StudentID = 3, StudentName = "Bill",  Age = 25 } ,
                new Student() { StudentID = 4, StudentName = "Ram" , Age = 20 } ,
                new Student() { StudentID = 5, StudentName = "Ron" , Age = 19 },
                new Student() { StudentID = 6, StudentName = "Ram" , Age = 18 }
            };


            /* List<Student> orderedList = (from stu in studentList
                                         orderby stu.Age
                                         select stu).ToList<Student>();*/

            List<Student> orderedList = studentList.OrderBy(student => student.Age).ToList<Student>();


            /*List<Student> descendingOrderedList = (from stu in studentList
                                                   orderby stu.Age descending
                                                   select stu).ToList<Student>();*/

            List<Student> descendingOrderedList = studentList.OrderByDescending(student => student.Age).ToList<Student>();


            /* List<Student> orderByNameAge = (from stu in studentList
                                             orderby stu.StudentName, stu.Age
                                             select stu).ToList<Student>();*/

            List<Student> orderByNameAge = studentList.OrderBy(student => student.StudentName).ThenBy(student => student.Age).ToList<Student>();

            foreach (Student stu in orderByNameAge)
            {
                Console.WriteLine($"{stu.StudentName} : {stu.Age}");
            }

        }
    }
}