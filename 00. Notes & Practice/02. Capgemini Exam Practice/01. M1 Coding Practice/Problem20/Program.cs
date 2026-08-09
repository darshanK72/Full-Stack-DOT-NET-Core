namespace Problem20
{
    class Student
    {
        public int roll { get; set; }
        public string name { get; set; }
        public int marks { get; set; }

        public Student(int roll, string name, int marks)
        {
            this.roll = roll;
            this.name = name;
            this.marks = marks;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public string getName()
        {
            return this.name;
        }

        public void setMakrs(int makrs)
        {
            this.marks = makrs;
        }


        public int getMakrs()
        {
            return this.marks;
        }

        public string generateExamCode()
        {
            if(this.roll < 10 || this.name.Length < 2)
            {
                return $"Verify Details, for roll no. {this.roll}";
            }
            else
            {
                return this.roll.ToString().Substring(0, 2) + this.name.Substring(0, 2);
            }
        }

        public string generateGrade()
        {
            char grade;
            if(this.marks > 90)
            {
                grade = 'A';
            }
            else if(this.marks > 80)
            {
                grade = 'B';
            }
            else if(this.marks > 70)
            {
                grade = 'C';
            }
            else
            {
                grade = 'D';
            }

            return $"{this.name} got {grade} grade";
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            Student student = new Student(1001, "Doselect", 99);
            Console.WriteLine(student.generateExamCode());
            Console.WriteLine(student.generateGrade());

        }
    }
}