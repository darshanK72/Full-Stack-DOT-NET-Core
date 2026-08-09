using System.Reflection;

namespace Problem13
{
    class Employee
    {
        public string Name { get; set; }
        public int EmployeeId { get; set; }

    }

    class Student
    {
        public string Name { get; set; }
        public int RollNumber { get; set; }
    }

    class ObjectCreation
    {
        public object Create(string objectName)
        {
            Object obj;
            if (objectName == "Employee")
            {
                obj = (Employee)Activator.CreateInstance(typeof(Employee));
            }
            else
            {
                obj = (Student)Activator.CreateInstance(typeof(Student));
            }
            return obj;
        }

        public PropertyInfo[] GetObjectProperties(object obj)
        {
            return obj.GetType().GetProperties();
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            var objCreation = new ObjectCreation();
            Object obj = objCreation.Create("Employee");
            var properties = objCreation.GetObjectProperties(obj);

            foreach(var prop in properties)
            {
                Console.WriteLine(prop.Name);
            }

            Console.WriteLine("--------------------------------");

            obj = objCreation.Create("Student");
            properties = objCreation.GetObjectProperties(obj);

            foreach (var prop in properties)
            {
                Console.WriteLine(prop.Name);
            }

        }
    }
}