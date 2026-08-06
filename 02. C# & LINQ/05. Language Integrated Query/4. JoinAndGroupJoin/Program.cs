using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace JoinAndGroupJoin
{
    public class Student
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public int StandardID { get; set; }
    }

    public class Standard
    {
        public int StandardID { get; set; }
        public string StandardName { get; set; }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            List<Student> studentList = new List<Student>() {
                new Student() { StudentID = 1, StudentName = "John", StandardID = 1 },
                new Student() { StudentID = 2, StudentName = "Moin", StandardID = 1 },
                new Student() { StudentID = 3, StudentName = "Bill", StandardID = 2 },
                new Student() { StudentID = 4, StudentName = "Ram" , StandardID = 2 },
                new Student() { StudentID = 5, StudentName = "Ron", StandardID = 3 }
            };

            List<Standard> standardList = new List<Standard>() {
                new Standard(){ StandardID = 1, StandardName="Standard 1"},
                new Standard(){ StandardID = 2, StandardName="Standard 2"},
                new Standard(){ StandardID = 3, StandardName="Standard 3"}
            };


            /* var newList = from student in studentList
                           join standard in standardList
                           on student.StandardID equals standard.StandardID
                           select new { student.StudentName, standard.StandardID };*/

            var newList = studentList.Join(standardList, stu => stu.StandardID, std => std.StandardID, (stu, std) => new { stu.StudentName, std.StandardID });

            foreach (var item in newList)
            {
                Console.WriteLine($"{item.StudentName} : {item.StandardID}");
            }

            /*var groupList = from standard in standardList
                            join student in studentList
                            on standard.StandardID equals student.StandardID
                            into stuGroup
                            select new {
                                        Students = stuGroup,
                                        StandardName = standard.StandardName
                                       };*/

            var groupList = standardList.GroupJoin(studentList, standard => standard.StandardID, student => student.StandardID, (standard, stuGroup) => new { Students = stuGroup, StandardName = standard.StandardName });

            foreach(var grp in groupList)
            {
                Console.WriteLine("Group Name : " + grp.StandardName);
                foreach(Student std in grp.Students)
                {
                    Console.WriteLine(std.StudentName);
                }
            }

        }
    }
}