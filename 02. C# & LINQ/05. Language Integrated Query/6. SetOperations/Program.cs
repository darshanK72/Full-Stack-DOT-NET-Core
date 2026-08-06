namespace SetOperations
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


    class StudentComparer : IEqualityComparer<Student>
    {
        public bool Equals(Student x, Student y)
        {
            if (x.StudentID == y.StudentID
                    && x.StudentName.ToLower() == y.StudentName.ToLower())
                return true;

            return false;
        }

        public int GetHashCode(Student obj)
        {
            return obj.StudentID.GetHashCode();
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            
            // Concat
            IList<int> collection1 = new List<int>() { 1, 2, 3 };
            IList<int> collection2 = new List<int>() { 4, 5, 6 };

            var collection3 = collection1.Concat(collection2);

            /*foreach (int i in collection3)
                Console.WriteLine(i);*/

            // Empty
            var arr = Enumerable.Empty<int>();

            // Range
            var arr1 = Enumerable.Range(10, 100);

            // Repeat
            var arr3 = Enumerable.Repeat<int>(10, 20);


            int[] arr4 = new int[] { 52, 672, 90, 92, 32, 653, 7, 71, 4, 7, 74, 5, 7, 84, 378, 8, 4, 5, 3, 4, 747, 2, 3, 2, 34, 4, 24 };


            // Distinct
            var distinctElements = arr4.Distinct();

            foreach(int i in distinctElements)
            {
                Console.WriteLine(i);
            }


            IList<Student> studentList = new List<Student>() {
                new Student() { StudentID = 1, StudentName = "John", Age = 18 } ,
                new Student() { StudentID = 2, StudentName = "Steve",  Age = 15 } ,
                new Student() { StudentID = 3, StudentName = "Bill",  Age = 25 } ,
                new Student() { StudentID = 3, StudentName = "Bill",  Age = 25 } ,
                new Student() { StudentID = 3, StudentName = "Bill",  Age = 25 } ,
                new Student() { StudentID = 3, StudentName = "Bill",  Age = 25 } ,
                new Student() { StudentID = 5, StudentName = "Ron" , Age = 19 }
            };


            var distinctStudentList = studentList.Distinct(new StudentComparer());

            foreach(Student s in distinctStudentList)
            {
                Console.WriteLine(s.StudentName);
            }

            IList<int> list1 = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 ,11,52,56,23 };
            IList<int> list2 = new List<int>() { 4, 5, 6,85,63,23,55,23,87,62,872,23,52,12 };

            // Except
            var list3 = list1.Except(list2);
            foreach(int i in list3){
                Console.Write(i + " ");
            }
            Console.WriteLine();

            // Intersect
            var list4 = list1.Intersect(list2);
            foreach (int i in list4)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();

            // Union
            var list5 = list1.Union(list2);
            foreach (int i in list5)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();
        }
    }
}