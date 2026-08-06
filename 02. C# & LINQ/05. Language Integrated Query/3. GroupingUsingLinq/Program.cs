namespace Practice2
{

    class Student
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public int Age { get; set; }

        public string Company { get;set; }
        public Student()
        {
        }

        public Student(int id, string name, int age, string company)
        {
            this.StudentID = id;
            this.StudentName = name;
            this.Age = age;
            Company = company;
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            IList<Student> studentList = new List<Student>() {
                new Student() { StudentID = 1, StudentName = "John", Age = 18 ,Company="TCS"} ,
                new Student() { StudentID = 2, StudentName = "Steve",  Age = 15  ,Company="TCS"} ,
                new Student() { StudentID = 3, StudentName = "Bill",  Age = 25  ,Company="TCS"} ,
                new Student() { StudentID = 4, StudentName = "Ram" , Age = 20  ,Company="XTS" } ,
                new Student() { StudentID = 5, StudentName = "Ron" , Age = 19 ,Company="XTS" },
                new Student() { StudentID = 6, StudentName = "Ram" , Age = 18  ,Company="XTS"},
                new Student() { StudentID = 7, StudentName = "Johnason", Age = 18  ,Company="CTS"} ,
                new Student() { StudentID = 8, StudentName = "Steven",  Age = 21 , Company = "CTS"} ,
                new Student() { StudentID = 9, StudentName = "Billy",  Age = 18  ,Company="XTS"} ,
                new Student() { StudentID = 10, StudentName = "Ramesh" , Age = 20 , Company = "CTS"} ,
                new Student() { StudentID = 11, StudentName = "Abram" , Age = 21  ,Company="CTS"}
            };


            /*var ageGroupResult = from stu in studentList
                                 group stu by stu.Age;*/

            var ageGroupResult = studentList.GroupBy(student => student.Age);

            //var ageGroupResult = studentList.ToLookup(student => student.Age);

            var res = studentList.GroupBy(stu => stu.Company).Select(x => new KeyValuePair<string,int>(x.Key,(int)x.Max(e => e.Age)));

            foreach(var r in res)
            {
                Console.WriteLine(r.Key + "    " + r.Value);
            }


            //foreach (var group in ageGroupResult)
            //{
            //    Console.WriteLine($"Group Key : {group.Key}");
            //    foreach (var stu in group)
            //    {
            //        Console.WriteLine($"{stu.StudentName} : {stu.Age}");
            //    }

            //}

        }
    }
}