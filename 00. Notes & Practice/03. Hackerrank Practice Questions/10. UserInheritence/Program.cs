using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace Solution
{

    public abstract class User
    {
        protected string type;
        protected string name;
        protected Gender gender;
        protected int age;

        public User(string type, string name, Gender gender, int age)
        {
            this.type = type;
            this.name = name;
            this.gender = gender;
            this.age = age;
        }

        public string GetUserName()
        {
            return name;
        }

        public string GetUserType()
        {
            return type;
        }

        public int GetAge()
        {
            return age;
        }

        public Gender GetGender()
        {
            return gender;
        }
    }

    public class Admin : User
    {
        public Admin(string name, Gender gender, int age)
            : base("Admin", name, gender, age)
        {
        }
    }

    public class Moderator : User
    {
        public Moderator(string name, Gender gender, int age)
            : base("Moderator", name, gender, age)
        {
        }
    }

    class Solution
    {
        static void Main()
        {
            Type baseType = typeof(User);
            if (!baseType.IsAbstract)
                throw new Exception($"{baseType.Name} type should be abstract");

            string values = Console.ReadLine();
            string[] valuesArr = values.Split(' ');
            var type = (Gender)Enum.Parse(typeof(Gender), valuesArr[1]);
            User admin = new Admin(valuesArr[0], type, int.Parse(valuesArr[2]));

            values = Console.ReadLine();
            valuesArr = values.Split(' ');
            type = (Gender)Enum.Parse(typeof(Gender), valuesArr[1]);
            User moderator = new Moderator(valuesArr[0], type, int.Parse(valuesArr[2]));

            var name = admin.GetUserName();
            Console.WriteLine($"Type of user {name} is {admin.GetUserType()}");
            Console.WriteLine($"Age of user {name} is {admin.GetAge()}");
            Console.WriteLine($"Gender of user {name} is {admin.GetGender()}");

            name = moderator.GetUserName();
            Console.WriteLine($"Type of user {name} is {moderator.GetUserType()}");
            Console.WriteLine($"Age of user {name} is {moderator.GetAge()}");
            Console.WriteLine($"Gender of user {name} is {moderator.GetGender()}");
        }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}